#region using

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Database;
using DataContract;
using DataTable;
using Scorpion;
using NLog;
using RankServerService;
using Shared;

#endregion

namespace Rank
{
    public class RankServerControlDefaultImpl : IRankService, ITickable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public IEnumerator OnConnected(Coroutine coroutine, RankCharacterProxy charProxy, AsyncReturnValue<bool> ret)
        {
            ret.Value = true;
            var proxy = (RankProxy) charProxy;
            proxy.Connected = true;

            yield break;
        }

        public IEnumerator UpdateServer(Coroutine coroutine, RankService _this, UpdateServerInMessage msg)
        {
            RankServer.Instance.UpdateManager.Update();
            return null;
        }

	    public IEnumerator SSGetRankDataByServerId(Coroutine coroutine, RankService _this, SSGetRankDataByServerIdInMessage msg)
	    {
		    var serverList = msg.Request.ServerList;
		    var time = msg.Request.Time;
		    var type = msg.Request.Ranktype;

            //请求这个时间点的数据不存在，那就把当前的排行榜数据存成那个时间点，防止下次再取时，又会取下次的排行榜，但排行榜会变化
	        var needSave = false;


			var timeStr = ServerRankBackupManager.FormatDateTimeToKey(DateTime.FromBinary(time));

			foreach (var sid in serverList.Items)
			{
				string tempKey = timeStr + "|" + sid;
				DBRankBackupServer data = null;
				if (!ServerRankBackupManager.DictRankDataCache.TryGetValue(tempKey,out data))
				{
					var result = RankServer.Instance.DB.Get<DBRankBackupServer>(coroutine, DataCategory.RankBackup, tempKey);

					yield return result;
					if (DataStatus.Ok == result.Status && null != result.Data)
					{
						data = result.Data;

						if (ServerRankBackupManager.DictRankDataCache.ContainsKey(tempKey))
						{
							ServerRankBackupManager.DictRankDataCache[tempKey] = data;
						}
						else
						{
							ServerRankBackupManager.DictRankDataCache.Add(tempKey, data);
						}	
					}
					else
					{
						Logger.Fatal("RankServer.Instance.DB.Get  DataStatus.Ok != result.Status [{0}]", tempKey);
					}
					
				}

				bool ok = false;
				if (null != data)
				{
					foreach (var item in data.List)
					{
						if (item.Type != type)
						{
							continue;
						}
						MsgRankList msgList = new MsgRankList();
						msgList.ServerId = sid;
						msgList.Type = type;

						foreach (var rankItem in item.Items)
						{
							if (msgList.Items.Count >= ServerRankBackupManager.MAXMember)
							{
								break;
							}
							MsgRankItemData msgItem = new MsgRankItemData();
							msgItem.CharacterId = rankItem.CharacterId;
							msgItem.Name = rankItem.Name;
							msgItem.Value = rankItem.Value;

							msgList.Items.Add(msgItem);
						}
						msg.Response.Data.Add(msgList);

						ok = true;
						break;
					}
				}

				if(!ok)
				{//数据库没找到就取当前数据
				    needSave = true;

                    foreach (var rankMgrKV in ServerRankManager.Ranks)
					{
						if (rankMgrKV.Value.ServerId != sid)
						{
							continue;
						}
						foreach (var rank in rankMgrKV.Value.rank)
						{
							if (rank.Value.RankType != type)
							{
								continue;
							}
							ok = true;
							MsgRankList msgList = new MsgRankList();
							msgList.ServerId = sid;
							msgList.Type = type;
							foreach (var charId in rank.Value.RankUUIDList)
							{
								if (msgList.Items.Count >= ServerRankBackupManager.MAXMember)
								{
									break;
								}

								DBRank_One one = null;
								if (!rank.Value.DBRankCache.TryGetValue(charId, out one))
								{
									continue;
								}

								MsgRankItemData msgItem = new MsgRankItemData();
								msgItem.CharacterId = one.Guid;
								msgItem.Name = one.Name;
								msgItem.Value = one.Value;

								msgList.Items.Add(msgItem);
							}
							msg.Response.Data.Add(msgList);
							break;
						}

						break;

					}
				}

				if (!ok)
				{
					Logger.Fatal("SSGetRankDataByServerId  [{0}]-[{1}]", timeStr, type);
				}
			}

			msg.Reply((int)ErrorCodes.OK);

	        if (needSave)
	        {//需要保存数据库
	            var passedTime = DateTime.FromBinary(time);
	            var serverl = new List<int>();
	            foreach (var item in serverList.Items)
	            {
	                serverl.Add(item);
                }
                var co = CoroutineFactory.NewSubroutine(ServerRankBackupManager.BackupAllRankCoroutine, coroutine, serverl, passedTime);
	            if (co.MoveNext())
	            {
	                yield return co;
	            }

            }

			yield break;
	    }

	    public IEnumerator SSNotifyCharacterOnConnet(Coroutine coroutine,
                                                     RankService _this,
                                                     SSNotifyCharacterOnConnetInMessage msg)
        {
            var characterId = msg.Request.CharacterId;
            var clientId = msg.Request.ClientId;
            var proxy = new RankProxy(_this, characterId, clientId);

            _this.Proxys[characterId] = proxy;

            var ret = AsyncReturnValue<bool>.Create();
            var subCo = CoroutineFactory.NewSubroutine(OnConnected, coroutine, proxy, ret);
            if (subCo.MoveNext())
            {
                yield return subCo;
            }
            var isOk = ret.Value;
            ret.Dispose();
            if (isOk)
            {
                msg.Reply((int) ErrorCodes.OK);
            }
            else
            {
                msg.Reply((int) ErrorCodes.ConnectFail);
            }
        }

        public IEnumerator BSNotifyCharacterOnLost(Coroutine coroutine,
                                                   RankService _this,
                                                   BSNotifyCharacterOnLostInMessage msg)
        {
            var characterId = msg.Request.CharacterId;
            RankCharacterProxy charProxy;
            if (!_this.Proxys.TryGetValue(characterId, out charProxy))
            {
                yield break;
            }
            var proxy = (RankProxy) charProxy;
            proxy.Connected = false;
        }

        public IEnumerator GMCommand(Coroutine co, RankService _this, GMCommandInMessage msg)
        {
            var request = msg.Request;
            var commands = request.Commonds.Items;
            var errs = msg.Response.Items;
            var err = new AsyncReturnValue<ErrorCodes>();
            foreach (var command in commands)
            {
                var co1 = CoroutineFactory.NewSubroutine(GameMaster.GmCommand, co, command, err);
                if (co1.MoveNext())
                {
                    yield return co1;
                }
                errs.Add((int) err.Value);
            }
            err.Dispose();
            msg.Reply();
        }

        public IEnumerator OnServerStart(Coroutine coroutine, RankService _this)
        {
            var rankServerControl = (RankServerControl) _this;

            //Thread.Sleep(GlobalVariable.WaitToConnectTimespan);
            RankServer.Instance.Start(rankServerControl);
            CoroutineFactory.NewCoroutine(ServerRankManager.Init).MoveNext();
			ServerRankBackupManager.Init();
            RankServer.Instance.IsReadyToEnter = true;
            _this.TickDuration = 1.0f;

            _this.Started = true;

            Console.WriteLine("RankServer startOver. [{0}]", RankServer.Instance.Id);
            return null;
        }

        public IEnumerator Tick(Coroutine co, ServerAgentBase server)
        {
            try
            {
                RankServerControl.Timer.Update();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Tick error.");
            }
	        try
	        {
				RankServerMonitor.TickRate.Mark();
	        }
	        catch (Exception)
	        {
		        
	        }
            return null;
        }

        public IEnumerator OnServerStop(Coroutine coroutine, RankService _this)
        {
            var co = CoroutineFactory.NewSubroutine(ServerRankManager.RefreshAll, coroutine);
            if (co.MoveNext())
            {
                yield return co;
            }
            RankServer.Instance.DB.Dispose();
        }

        public IEnumerator PrepareDataForEnterGame(Coroutine coroutine,
                                                   RankService _this,
                                                   PrepareDataForEnterGameInMessage msg)
        {
            msg.Reply();
            Logger.Info("Enter Game {0} - PrepareDataForEnterGame - 1 - {1}", msg.CharacterId,
                TimeManager.Timer.ElapsedMilliseconds);
            return null;
        }

        public IEnumerator PrepareDataForCreateCharacter(Coroutine coroutine,
                                                         RankService _this,
                                                         PrepareDataForCreateCharacterInMessage msg)
        {
            msg.Reply();
            Logger.Info("Reply PrepareDataForCreateCharacter Rank {0}", msg.CharacterId);
            return null;
        }

        public IEnumerator PrepareDataForCommonUse(Coroutine coroutine,
                                                   RankService _this,
                                                   PrepareDataForCommonUseInMessage msg)
        {
            msg.Reply();
            return null;
        }

        public IEnumerator PrepareDataForLogout(Coroutine coroutine,
                                                RankService _this,
                                                PrepareDataForLogoutInMessage msg)
        {
            msg.Reply();
            yield break;
        }

        public IEnumerator CheckConnected(Coroutine coroutine, RankService _this, CheckConnectedInMessage msg)
        {
            Logger.Error("Rank CheckConnected, {0}", msg.CharacterId);

            //RankCharacterProxy proxy = null;
            //if (_this.Proxys.TryGetValue(msg.CharacterId, out proxy))
            //{
            //    if ((proxy as RankProxy).Connected)
            //    {
            //        msg.Response = 1;
            //        msg.Reply();
            //        return null;
            //    }

            //    (proxy as RankProxy).WaitingCheckConnectedInMessages.Add(msg);
            //}

            return null;
        }

        public IEnumerator CheckLost(Coroutine coroutine, RankService _this, CheckLostInMessage msg)
        {
            Logger.Error("Rank CheckLost, {0}", msg.CharacterId);

            //RankCharacterProxy proxy = null;
            //if (!_this.Proxys.TryGetValue(msg.CharacterId, out proxy))
            //{
            //    msg.Reply();
            //}
            //else
            //{
            //    if ((proxy as RankProxy).Connected)
            //    {
            //        (proxy as RankProxy).WaitingCheckLostInMessages.Add(msg);
            //    }
            //    else
            //    {
            //        msg.Reply();
            //    }
            //}

            return null;
        }

        public IEnumerator QueryStatus(Coroutine coroutine, RankService _this, QueryStatusInMessage msg)
        {
            var rankServerControl = (RankServerControl) _this;

            var common = new ServerCommonStatus();
            common.Id = RankServer.Instance.Id;
            common.ByteReceivedPerSecond = rankServerControl.ByteReceivedPerSecond;
            common.ByteSendPerSecond = rankServerControl.ByteSendPerSecond;
            common.MessageReceivedPerSecond = rankServerControl.MessageReceivedPerSecond;
            common.MessageSendPerSecond = rankServerControl.MessageSendPerSecond;
            common.ConnectionCount = rankServerControl.ConnectionCount;

            msg.Response.CommonStatus = common;

            msg.Response.ConnectionInfo.AddRange(RankServer.Instance.Agents.Select(kv =>
            {
                var conn = new ConnectionStatus();
                var item = kv.Value;
                conn.ByteReceivedPerSecond = item.ByteReceivedPerSecond;
                conn.ByteSendPerSecond = item.ByteSendPerSecond;
                conn.MessageReceivedPerSecond = item.MessageReceivedPerSecond;
                conn.MessageSendPerSecond = item.MessageSendPerSecond;
                conn.Target = item.Id;
                conn.Latency = item.Latency;

                return conn;
            }));

            msg.Reply();

            yield break;
        }

        //修改等级
        public IEnumerator CharacterChangeLevel(Coroutine coroutine,
                                                RankService _this,
                                                CharacterChangeLevelInMessage msg)
        {
            ServerRankManager.ResetLevel(msg.Request.ServerId, msg.Request.Name, msg.Request.Guid,
                msg.Request.Level*Constants.RankLevelFactor + msg.Request.Exp);
            yield break;
        }

        //修改数据
        public IEnumerator CharacterChangeData(Coroutine coroutine, RankService _this, CharacterChangeDataInMessage msg)
        {
            switch (msg.Request.RankType)
            {
                case (int) RankType.FightValue: //战斗力
                    ServerRankManager.ResetFightPoint(msg.Request.ServerId, msg.Request.Name, msg.Request.Guid,
                        msg.Request.Value);
                    break;
                case (int) RankType.Money: //钱
                    ServerRankManager.ResetMoney(msg.Request.ServerId, msg.Request.Name, msg.Request.Guid,
                        msg.Request.Value);
                    break;
                case (int) RankType.CityLevel: //家园等级
                    ServerRankManager.ResetCityLevel(msg.Request.ServerId, msg.Request.Name, msg.Request.Guid,
                        msg.Request.Value);
                    break;
                case (int) RankType.WingsFight: //翅膀战力
                    ServerRankManager.ResetWingsFight(msg.Request.ServerId, msg.Request.Name, msg.Request.Guid,
                        msg.Request.Value);
                    break;
                case (int) RankType.PetFight: //精灵战力
                    ServerRankManager.ResetPetFight(msg.Request.ServerId, msg.Request.Name, msg.Request.Guid,
                        msg.Request.Value);
                    break;
				case (int)RankType.RechargeTotal: //精灵战力
					ServerRankManager.ResetTotalRecharge(msg.Request.ServerId, msg.Request.Name, msg.Request.Guid,
						msg.Request.Value);
					break;
                case (int)RankType.DailyGift:
                case (int)RankType.WeeklyGift:
                case (int)RankType.TotalGift:
                    ServerRankManager.ResetGiftRank(msg.Request.RankType, msg.Request.ServerId,
                        msg.Request.Name, msg.Request.Guid, msg.Request.Value);
                    break;
                case (int) RankType.Mount:  // 坐骑
                    ServerRankManager.ResetMountRank(msg.Request.ServerId, msg.Request.Name, msg.Request.Guid, msg.Request.Value);
                    break;
            }
            yield break;
        }

        //获取PvP的列表
        public IEnumerator Rank_GetP1vP1List(Coroutine coroutine, RankService _this, Rank_GetP1vP1ListInMessage msg)
        {
            var co = CoroutineFactory.NewSubroutine(P1vP1.GetPvPList, coroutine, msg.Request.ServerId,
                msg.Request.CharacterId, msg.Request.Name, msg.Response.characters, msg.Response.ranks);
            if (co.MoveNext())
            {
                yield return co;
            }
            if (P1vP1.DBRank_One != null)
            {
                msg.Response.nowRank = P1vP1.DBRank_One.Rank;
            }
            //P1vP1.GetPvPList(msg.Request.ServerId, msg.Request.CharacterId, msg.Request.Name, msg.Response.characters, msg.Response.ranks, ref dbRankOne);
            //if (dbRankOne != null)
            //{
            //    msg.Response.nowRank = dbRankOne.Rank;
            //}
            msg.Reply();
        }

        //对比玩家与名次是否相符
        public IEnumerator CompareRank(Coroutine coroutine, RankService _this, CompareRankInMessage msg)
        {
            msg.Response = ServerRankManager.CompareRank(msg.Request.ServerId, P1vP1.P1vP1RankTypeId,
                msg.Request.CharacterId, msg.Request.Rank);
            msg.Reply();
            yield break;
        }

        public IEnumerator GetRankValue(Coroutine coroutine, RankService _this, GetRankValueInMessage msg)
        {
            var request = msg.Request;
            var serverId = request.ServerId;
            var rankType = request.RankType;
            var idx = request.Idx;
            msg.Response = ServerRankManager.GetRankData(serverId, rankType, idx);
            msg.Reply();
            yield break;
        }

        public IEnumerator RankP1vP1FightOver(Coroutine coroutine, RankService _this, RankP1vP1FightOverInMessage msg)
        {
            var selfId = msg.Request.CharacterId;
            var pvpId = msg.Request.PvpCharacterId;

            var ranking = ServerRankManager.GetRankByType(msg.Request.ServerId, P1vP1.P1vP1RankTypeId);
            if (null == ranking)
            {
                Logger.Error("RankP1vP1FightOver not find ranking serverid={0},type={1}", msg.Request.ServerId, P1vP1.P1vP1RankTypeId);
                yield break;
            }

            var self = ranking.GetPlayerData(selfId);
            var pvp = ranking.GetPlayerData(pvpId);
            if (self == null)
            {
                self = new DBRank_One();
                self.Rank = ranking.GetRankCount() + 1;
                Logger.Error("RankP1vP1FightOver not find selfId={0}", selfId);
                yield break;
            }
            if (pvp == null)
            {
                Logger.Error("RankP1vP1FightOver not find pvpId={0}", pvpId);
                yield break;
            }
            var selfIndex = self.Rank;
            var pvpIndex = pvp.Rank;
            var result = msg.Request.Result;
            if (result != 1)
            {
                var selfMessage = RankServer.Instance.LogicAgent.PushP1vP1LadderChange(selfId, 0, msg.Request.PvpName,
                    result, selfIndex, selfIndex);
                yield return selfMessage.SendAndWaitUntilDone(coroutine);
                if (!StaticData.IsRobot(pvpId))
                    //if (pvpId > 999999)
                {
                    var pvpMessage = RankServer.Instance.LogicAgent.PushP1vP1LadderChange(pvpId, 1, msg.Request.Name,
                        result, pvpIndex, pvpIndex);
                    yield return pvpMessage.SendAndWaitUntilDone(coroutine);
                }
                yield break;
            }

            if (selfIndex > pvpIndex)
            {
                ranking.SwapIndex(self, pvp);
                var selfMessage = RankServer.Instance.LogicAgent.PushP1vP1LadderChange(selfId, 0, msg.Request.PvpName,
                    result, selfIndex, pvpIndex);
                yield return selfMessage.SendAndWaitUntilDone(coroutine);
                if (!StaticData.IsRobot(pvpId))
                    //if (pvpId > 999999)
                {
                    var pvpMessage = RankServer.Instance.LogicAgent.PushP1vP1LadderChange(pvpId, 1, msg.Request.Name,
                        result, pvpIndex, selfIndex);
                    yield return pvpMessage.SendAndWaitUntilDone(coroutine);

                    {//邮件通知对方
                        var item = new ItemBaseData();
                        item.ItemId = -1;
                        var args = new StringArray();
                        args.Items.Add(msg.Request.Name);
                        args.Items.Add(selfIndex.ToString());
                        var msgMail = RankServer.Instance.LogicAgent.SendMailToCharacterByItems(pvpId, 95, item, args);
                        yield return msgMail.SendAndWaitUntilDone(coroutine);
                    }

                }
            }
            else
            {
                //selfOverIndex = -1;//-1代表打的人名次比我要差
                var selfMessage = RankServer.Instance.LogicAgent.PushP1vP1LadderChange(selfId, 0, msg.Request.PvpName,
                    result, selfIndex, -1);
                yield return selfMessage.SendAndWaitUntilDone(coroutine);
                if (!StaticData.IsRobot(pvpId))
                    //if (pvpId > 999999)
                {
                    var pvpMessage = RankServer.Instance.LogicAgent.PushP1vP1LadderChange(pvpId, 1, msg.Request.Name,
                        result, pvpIndex, -1);
                    yield return pvpMessage.SendAndWaitUntilDone(coroutine);
                }
            }
        }

		public IEnumerator ServerGMCommand(Coroutine coroutine, RankService _this, ServerGMCommandInMessage msg)
        {
			var cmd = msg.Request.Cmd;
			var param = msg.Request.Param;

			Logger.Info("Rank----------ServerGMCommand----------cmd={0}|param={1}", cmd, param);

			try
			{
				if ("ReloadTable" == cmd)
				{
					Table.ReloadTable(param);
				}
				else if ("BackupRank" == cmd)
				{
					ServerRankBackupManager.BackupAllRank(null, DateTime.Now);
				}
			}
			catch (Exception e)
			{

				Logger.Error("Rank----------ServerGMCommand----------error={0}", e.Message);
			}
			finally
			{

			}
            yield break;
        }

        public IEnumerator ReadyToEnter(Coroutine coroutine, RankService _this, ReadyToEnterInMessage msg)
        {
            if (RankServer.Instance.IsReadyToEnter && RankServer.Instance.AllAgentConnected())
            {
                msg.Response = 1;
            }
            else
            {
                msg.Response = 0;
            }

            msg.Reply();

            return null;
        }

        //玩家的排行榜数据修改
        public IEnumerator SSCharacterChangeDataList(Coroutine coroutine,
                                                     RankService _this,
                                                     SSCharacterChangeDataListInMessage msg)
        {
            var name = msg.Request.Changes.Name;
            var ServerId = msg.Request.Changes.ServerId;
            foreach (var change in msg.Request.Changes.Changes)
            {
                switch (change.RankType)
                {
                    case (int) RankType.FightValue: //战斗力
                        ServerRankManager.ResetFightPoint(ServerId, name, msg.CharacterId, change.Value);
                        break;
                    case (int) RankType.Level: //等级
                        ServerRankManager.ResetLevel(ServerId, name, msg.CharacterId, change.Value);
                        break;
                    case (int) RankType.Money: //钱
                        ServerRankManager.ResetMoney(ServerId, name, msg.CharacterId, change.Value);
                        break;
                    case (int) RankType.CityLevel: //家园等级
                        ServerRankManager.ResetCityLevel(ServerId, name, msg.CharacterId, change.Value);
                        break;
                    case (int) RankType.WingsFight: //翅膀战力
                        ServerRankManager.ResetWingsFight(ServerId, name, msg.CharacterId, change.Value);
                        break;
                    case (int) RankType.PetFight: //精灵战力
                        ServerRankManager.ResetPetFight(ServerId, name, msg.CharacterId, change.Value);
                        break;
					case (int)RankType.RechargeTotal: //总充值
						ServerRankManager.ResetTotalRecharge(ServerId, name, msg.CharacterId, change.Value);
						break;
                    case (int)RankType.DailyGift:
                    case (int)RankType.WeeklyGift:
                    case (int)RankType.TotalGift:
                        ServerRankManager.ResetGiftRank(change.RankType, ServerId, name, msg.CharacterId, change.Value);
                        break;
                    case (int)RankType.Mount:  // 坐骑
                        ServerRankManager.ResetMountRank(ServerId, name, msg.CharacterId, change.Value);
                        break;
                }
            }
            yield break;
        }
        public IEnumerator NodifyModifyPlayerName(Coroutine coroutine, RankService _this, NodifyModifyPlayerNameInMessage msg)
        {
            var name = msg.Request.ModifyName;
            var ServerId = msg.Request.ServerId;
            ServerRankManager.ChangePlayerName(ServerId, name, msg.Request.Guid);
            yield break;
        }
        //获取某排行榜数据
        public IEnumerator SSGetServerRankData(Coroutine coroutine, RankService _this, SSGetServerRankDataInMessage msg)
        {
            var serverId = msg.Request.ServerId;
            var type = msg.Request.Ranktype;
            var tempList = ServerRankManager.GetRankDataByServerId(serverId, type, 1, 100);
            if (tempList == null)
            {
                msg.Reply();
                yield break;
            }
            foreach (var one in tempList)
            {
                if (one == null)
                {
                    Logger.Error("GetRankList serverId={0},type={1}", serverId, type);
                    continue;
                }
                msg.Response.RankType = type;
                var rankMessage = new RankOne
                {
                    Id = one.Guid,
                    Name = one.Name //ServerRankManager.GetName(serverId, one.Guid),
                };
                if (type == (int) RankType.Level)
                {
                    rankMessage.Value = (int) (one.Value/Constants.RankLevelFactor);
                }
                else if (type == (int) RankType.CityLevel)
                {
                    rankMessage.Value = (int) (one.Value/Constants.RankLevelFactor);
                }
                else if (type == (int) RankType.Arena)
                {
                    rankMessage.Value = one.FightPoint;
                }
                else
                {
                    rankMessage.Value = (int) one.Value;
                }
                if (rankMessage.Name == "")
                {
                    var dbSceneSimple = RankServer.Instance.SceneAgent.GetSceneSimpleData(rankMessage.Id, 0);
                    yield return dbSceneSimple.SendAndWaitUntilDone(coroutine);
                    if (dbSceneSimple.State == MessageState.Reply)
                    {
                        rankMessage.Name = dbSceneSimple.Response.Name;
                        one.Name = dbSceneSimple.Response.Name;
                    }
                    else
                    {
                        //未找到
                        rankMessage.Name = "^301036";
                        one.Name = "^301036";
                    }
                }
                msg.Response.RankData.Add(rankMessage);
            }
            msg.Reply();
        }
    }

    public class RankServerControl : RankService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static TimeManager Timer = new TimeManager();
        private long tickTime = 0;

        public RankServerControl()
        {
            RankServer.Instance.UpdateManager.InitStaticImpl(typeof (RankServerControl),
                typeof (RankServerControlDefaultImpl),
                o => { SetServiceImpl((IRankService) o); });

            RankServer.Instance.UpdateManager.InitStaticImpl(typeof (RankProxy), typeof (RankProxyDefaultImpl),
                o => { SetProxyImpl((IRankCharacterProxy) o); });
        }

        public override RankCharacterProxy NewCharacterIn(ulong characterId, ulong clientId)
        {
            return new RankProxy(this, characterId, clientId);
        }

        public override void OnException(Exception ex)
        {
            Logger.Error(ex, "Network error");
        }

        public override IEnumerator OnServerStart(Coroutine coroutine)
        {
            return mImpl.OnServerStart(coroutine, this);
        }

        public override IEnumerator OnServerStop(Coroutine coroutine)
        {
            return mImpl.OnServerStop(coroutine, this);
        }

        public override IEnumerator PerformenceTest(Coroutine coroutine, ServerClient client, ServiceDesc desc)
        {
            client.SendMessage(desc);
            yield break;
        }

        public void Status(ConcurrentDictionary<string, string> dict)
        {
            try
            {
                dict.TryAdd("_Listening", Listening.ToString());
                dict.TryAdd("Started", Started.ToString());
                dict.TryAdd("TickTime", tickTime.ToString());
                //dict.TryAdd("ByteReceivedPerSecond", ByteReceivedPerSecond.ToString());
                //dict.TryAdd("ByteSendPerSecond", ByteSendPerSecond.ToString());
                //dict.TryAdd("MessageReceivedPerSecond", MessageReceivedPerSecond.ToString());
                //dict.TryAdd("MessageSendPerSecond", MessageSendPerSecond.ToString());
                //dict.TryAdd("ConnectionCount", ConnectionCount.ToString());

                //foreach (var agent in RankServer.Instance.Agents.ToArray())
                //{
                //    dict.TryAdd(agent.Key + " Latency", agent.Value.Latency.ToString());
                //    dict.TryAdd(agent.Key + " ByteReceivedPerSecond", agent.Value.ByteReceivedPerSecond.ToString());
                //    dict.TryAdd(agent.Key + " ByteSendPerSecond", agent.Value.ByteSendPerSecond.ToString());
                //    dict.TryAdd(agent.Key + " MessageReceivedPerSecond", agent.Value.MessageReceivedPerSecond.ToString());
                //    dict.TryAdd(agent.Key + " MessageSendPerSecond", agent.Value.MessageSendPerSecond.ToString());
                //}
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "RankServerControl Status Error!{0}");
            }
        }

        public override IEnumerator Tick(Coroutine coroutine)
        {
            tickTime++;
            return ((ITickable) mImpl).Tick(coroutine, this);
        }
    }
}