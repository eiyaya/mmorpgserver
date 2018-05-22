#region using

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Database;
using DataContract;
using DataTable;
using EventSystem;
using LogicServerService;
using Scorpion;
using NLog;

using Shared;

#endregion

namespace Logic
{
    public class LogicServerControlDefaultImpl : ILogicService, ITickable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Logger RechargeLogger = LogManager.GetLogger("RechargeLogger");
        private static readonly Logger kafaLogger = LogManager.GetLogger(Shared.LoggerName.KafkaLog);

        public IEnumerator NotifyInviteChallenge(Coroutine coroutine, LogicService _this, NotifyInviteChallengeInMessage msg)
        {
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (null == character)
            {
                Logger.Error("Logic.NotifyInviteChallenge Character is UnLine.");
                yield break;
            }

            CharacterController.ChallengeInvitor invitor = new CharacterController.ChallengeInvitor();
            invitor.CharacterId = msg.Request.InvitorId;
            invitor.Name = msg.Request.InvitorName;
            invitor.ServerId = msg.Request.InvitorServerId;

            character.ChallengeInvitors.Add(msg.Request.InvitorId, invitor);

            //1、通知客户端
            character.Proxy.ReceiveChallenge(msg.Request.InvitorId, msg.Request.InvitorName, Table.GetServerName(msg.Request.InvitorServerId).Name);

            //2、世界广播
            var args = new List<string>();
            args.Add(msg.Request.InvitorName);
            args.Add(character.GetName());
            var content = Utils.WrapDictionaryId(100003323, args);

            var msg2 = LogicServer.Instance.ChatAgent.SSBroadcastAllServerMsg(msg.CharacterId, (int)eChatChannel.SystemScroll, msg.Request.InvitorName, new ChatMessageContent { Content = content });
            yield return msg2.SendAndWaitUntilDone(coroutine);

            //3、开始倒计时
            LogicServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(12), () =>
            {
                if (character.ChallengeInvitors.ContainsKey(msg.Request.InvitorId))
                {
                    // 没有对角斗邀请做出回应，做拒绝处理
                    var inv = character.ChallengeInvitors[msg.Request.InvitorId];
                    character.ChallengeInvitors.Remove(msg.Request.InvitorId);

                    // 世界广播
                    var args1 = new List<string>();
                    args1.Add(character.GetName());
                    args1.Add(inv.Name);
                    var content1 = Utils.WrapDictionaryId(100003324, args1);

                    var msg3 = LogicServer.Instance.ChatAgent.SSBroadcastAllServerMsg(msg.CharacterId, (int)eChatChannel.SystemScroll, character.GetName(), new ChatMessageContent { Content = content1 });
                    msg3.SendAndWaitUntilDone(coroutine);
                }
            });
        }

        public IEnumerator OnConnected(Coroutine coroutine, LogicCharacterProxy charProxy, AsyncReturnValue<bool> ret)
        {
            ret.Value = false;
            var proxy = (LogicProxy) charProxy;

            LogManager.GetLogger("ConnectLost")
                .Info("character {0} - {1} Logic OnConnected 1", proxy.CharacterId, proxy.ClientId);
            PlayerLog.WriteLog(proxy.CharacterId, "-----Logic-----OnConnected----------{0}", proxy.CharacterId);
            var obj = CharacterManager.Instance.GetCharacterControllerFromMemroy(proxy.CharacterId);
            if (obj == null)
            {
                Logger.Fatal("onConnected CharacterId={0}", proxy.CharacterId);
                yield break;
            }
            proxy.Character = obj;
            obj.Proxy = proxy;
            proxy.Connected = true;

            obj.lExdata64.SetTime(Exdata64TimeType.LastOnlineTime, DateTime.Now);

            proxy.Character.State = CharacterState.Connected;


            {//问卷调查的日期判定
                Table.ForeachSurvey(tb =>
                {
                    if (tb.type != 0)
                    {//0:开服天数
                        return true;
                    }
                    if (proxy.Character.GetFlag(tb.flagHad) == true) //过滤已经完成的
                        return true;
                    if (proxy.Character.GetFlag(tb.flagCan) == true)//过滤已经判定可以完成的
                        return true;
                    var time = obj.lExdata64.GetTime(Exdata64TimeType.CreateTime);
                    var now = DateTime.Now;

                    if (now.Date >= time.Date.AddDays(tb.param).Date)
                    {
                        proxy.Character.SetFlag(tb.flagCan, true);
                    }
                    return true;
                });
            }

            //新添扩展计数的处理
            var tbServer = Table.GetServerConfig(4000);
            if (tbServer != null)
            {
                Dictionary<int,int> dic = new Dictionary<int, int>();
                var array = tbServer.Value.Split('|');
                foreach (var tmp in array)
                {
                    var arr = tmp.Split(':');
                    if (arr.Length > 0)
                    {
                        if(obj.GetFlag(int.Parse(arr[1]))==false)
                        {
                            var ex = Table.GetExdata(int.Parse(arr[0]));
                            if (ex != null)
                            {
                                obj.SetExData(int.Parse(arr[0]), ex.InitValue);
                            }
                            obj.SetFlag(int.Parse(arr[1]),true);
                        }
                    }
                }
            }
            if (obj.GetFlag(3520) == false)
            {//3520   修复老用户的时装称号
                obj.SetFlag(3520);
                {//修复exdata
                    {
                        var count = obj.GetBag((int)eBagType.EquipShiZhuang).GetNoFreeCount() +
                        obj.GetBag((int)eBagType.EquipShiZhuangBag).GetNoFreeCount();
                        obj.SetExData(790, count);
                    }
                    {
                        var count = obj.GetBag((int)eBagType.WingShiZhuang).GetNoFreeCount() +
                        obj.GetBag((int)eBagType.WingShiZhuangBag).GetNoFreeCount();
                        obj.SetExData(792, count);
                    }
                    {
                        var count = obj.GetBag((int)eBagType.WeaponShiZhuang).GetNoFreeCount() +
                        obj.GetBag((int)eBagType.WeaponShiZhuangBag).GetNoFreeCount();
                        obj.SetExData(791, count);
                    }
                }

                Table.ForeachFashionTitle(record =>
                {
                    if (record.Flag <= 0)
                        return true;
                    if (obj.GetFlag(record.Flag))
                        return true;
                    for (int i = 0; i < record.ExList.Count; i++)
                    {
                        var id = record.ExList[i];
                        var val = record.ValList[i];
                        if (obj.GetExData(id) < val)
                            return true;
                    }
                    obj.SetFlag(record.Flag);
                    if (record.Exdata > 0)
                        obj.SetExData(record.Exdata, 1);
                    return true;
                });
            }

            {//修正功能开放节奏调整 1.符文 2.灵兽
                {//符文
                    var level = obj.GetLevel();
                    var bag = obj.mBag.GetBag((int)eBagType.MedalUsed);
                    for (var i = 0; i < bag.GetNowCount(); i++)
                    {
                        var limit = Table.GetServerConfig(1006 + i).ToInt();
                        if (level >= limit)
                        {
                            continue;
                        }
                        int tmp = 0;
                        if (ErrorCodes.OK != obj.RemoveMedal((int)eBagType.MedalUsed, i, ref tmp))
                            break;
                    }
                }
                {//灵兽
                    obj.CheckBattleElf();  
                }
            }



            proxy.Character.OnlineTime = DateTime.Now;
            var msg = LogicServer.Instance.LoginAgent.GetTodayOnlineSeconds(proxy.ClientId, proxy.CharacterId);
            yield return msg.SendAndWaitUntilDone(coroutine);
            if (msg.State == MessageState.Reply && msg.ErrorCode == (int) ErrorCodes.OK)
            {
                proxy.Character.TodayTimes = msg.Response;
                proxy.Character.SetExData(31, (int) msg.Response);
            }
            else
            {
                proxy.Character.TodayTimes = 0;
                proxy.Character.SetExData(31, 0);
            }
            CharacterManager.Instance.UpdateSimpleData(proxy.CharacterId);

            var dbLoginSimple = LogicServer.Instance.LoginAgent.GetLoginSimpleData(proxy.ClientId, proxy.CharacterId);
            yield return dbLoginSimple.SendAndWaitUntilDone(coroutine);
            if (dbLoginSimple.State != MessageState.Reply)
            {
                yield break;
            }
            if (obj.Proxy == null)
            {
                Logger.Warn("logic OnConnected obj.Proxy is null! type=1, objId={0}", obj.mGuid);
                yield break;
            }
            proxy.Character.SetName(dbLoginSimple.Response.Name);
            var allianceSimple = LogicServer.Instance.TeamAgent.GetAllianceCharacterData(proxy.CharacterId,
                proxy.Character.serverId, proxy.CharacterId, proxy.Character.GetLevel());
            yield return allianceSimple.SendAndWaitUntilDone(coroutine);
            if (allianceSimple.State != MessageState.Reply)
            {
                yield break;
            }
            if (allianceSimple.ErrorCode != (int) ErrorCodes.OK)
            {
                yield break;
            }
            if (obj.Proxy == null)
            {
                Logger.Warn("logic OnConnected obj.Proxy is null! type=2, objId={0}", obj.mGuid);
                yield break;
            }
            var allianceData = allianceSimple.Response;
            if (proxy.Character.mAlliance.AllianceId != allianceData.AllianceId)
            {
                Logger.Warn("Character Alliance not same!character={0}, logic ={1},team={2}", proxy.CharacterId,
                    proxy.Character.mAlliance.AllianceId, allianceData.AllianceId);
                proxy.Character.mAlliance.AllianceId = allianceData.AllianceId;
            }

            {//修正称号
                var titleId = 2003 - proxy.Character.mAlliance.Ladder;
                var titles = new List<int>();
                var states = new List<bool>();
                for (var i = 2000; i <= 2003; i++)
                {
                    titles.Add(i);
                    states.Add(i == titleId && proxy.Character.mAlliance.AllianceId>0);
                }

                proxy.Character.ModityTitles(titles, states);
            }



            if (allianceData.AllianceId != 0)
            {
                proxy.Character.mAlliance.State = AllianceState.Have;
                proxy.Character.mAlliance.CleanApplyList();

                //修改城主称号
                var titleId = allianceData.Ladder == (int) eAllianceLadder.Chairman ? 5000 : 5001;
                var isOccupt = false;
                AllianceWarInfo info;
                if (StaticParam.AllianceWarInfo.TryGetValue(obj.serverId, out info))
                {
                    isOccupt = allianceData.AllianceId == info.OccupantId;
                }
                else
                {
                    Logger.Error("logic OnConnected  StaticParam.AllianceWarInfo ------ not find obj.serverId = {0}",
                        obj.serverId);
                }
                obj.ModityTitle(titleId, isOccupt);
            }
            else
            {
                proxy.Character.mAlliance.State = AllianceState.None;
            }
            if (proxy.Character.mAlliance.Ladder != allianceData.Ladder)
            {
                Logger.Warn("Character Ladder not same!character={0}, logic ={1},team={2}", proxy.CharacterId,
                    proxy.Character.mAlliance.Ladder, allianceData.Ladder);
                proxy.Character.mAlliance.Ladder = allianceData.Ladder;
            }
            var index = 0;
            var Temp = new int[3];
            foreach (var apply in allianceData.Applys)
            {
                Temp[index] = apply;
                index++;
            }
            for (var i = 286; i <= 288; ++i)
            {
                var tempExdata = proxy.Character.GetExData(i);
                if (tempExdata != 0)
                {
                    if (Temp.Contains(tempExdata))
                    {
                        Logger.Warn("Character Apply Alliance not same!character={4}, logic ={0},team={1},{2},{3}",
                            tempExdata, Temp[0], Temp[1], Temp[2], proxy.CharacterId);
                    }
                }
                proxy.Character.SetExData(i, Temp[i - 286]);
            }
            obj.mFriend.FriendNextUpdateTime = DateTime.Now;
            obj.mFriend.EnemyNextUpdateTime = DateTime.Now;
            obj.mFriend.ShieldNextUpdateTime = DateTime.Now;
            RegisterAllSyncData(proxy.Character);
            ret.Value = true;
            var msg1 = LogicServer.Instance.GameMasterAgent.CharacterConnected(proxy.CharacterId, proxy.CharacterId,
                (int) ServiceType.Logic);
            yield return msg1.SendAndWaitUntilDone(coroutine);
        }

        public bool OnSyncRequested(LogicCharacterProxy charProxy, ulong characterId, uint syncId)
        {
            var proxy = (LogicProxy) charProxy;
            var characterController =
                CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (characterController == null)
            {
                return false;
            }

            var resType = (eResourcesType) syncId;

            proxy.SyncCenter.AddSyncData(proxy.CharacterId, syncId, characterController.mBag, resType.ToString(),
                () => { return characterController.mBag.GetRes(resType); });

            return true;
        }

        private void QueryAllianceWarInfo()
        {
            CoroutineFactory.NewCoroutine(QueryAllianceWarInfoCoroutine).MoveNext();
        }

        private IEnumerator QueryAllianceWarInfoCoroutine(Coroutine co)
        {
            for (var i = 0; i < 3; i++)
            {
                var msg = LogicServer.Instance.TeamAgent.QueryAllianceWarInfo(0, 0);
                yield return msg.SendAndWaitUntilDone(co);
                if (msg.State == MessageState.Reply && msg.ErrorCode == (int) ErrorCodes.OK)
                {
                    StaticParam.AllianceWarInfo.Clear();
                    var infos = msg.Response.Data;
                    foreach (var info in infos)
                    {
                        StaticParam.AllianceWarInfo.Add(info.ServerId, info);
                    }
                    yield break;
                }
            }
            Logger.Fatal("QueryAllianceWarInfo failed! Logic server can not start!");
        }

        private void RegisterAllSyncData(CharacterController Character)
        {
            var proxy = Character.Proxy;
            if (proxy == null)
            {
                Logger.Error("Logic RegisterAllSyncData proxy is null!");
                return;
            }
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------RegisterAllSyncData----------");
            for (var i = eResourcesType.LevelRes; i < eResourcesType.CountRes; i++)
            {
                OnSyncRequested(proxy, Character.mGuid, (uint) i);
            }
        }

        private void RemoveAllSyncData(CharacterController Character)
        {
            var proxy = Character.Proxy;
            if (proxy == null)
            {
                Logger.Error("Logic RemoveAllSyncData proxy is null!");
                return;
            }
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------RegisterAllSyncData----------");
            for (var i = eResourcesType.LevelRes; i < eResourcesType.CountRes; i++)
            {
                proxy.SyncCenter.RemoveSyncData(Character.mGuid, (uint) i);
            }
        }

        public IEnumerator UpdateServer(Coroutine coroutine, LogicService _this, UpdateServerInMessage msg)
        {
            LogicServer.Instance.UpdateManager.Update();
            return null;
        }

        public Dictionary<int, int> GetGifts(string str)
        {
            Dictionary<int, int> dict = new Dictionary<int, int>();
            var strs = str.Split('|');
            if (strs.Length == 0)
                return dict;

            foreach (var s in strs)
            {
                var kv = s.Split(',');
                if (kv.Length < 2)
                    continue;
                dict.Add(int.Parse(kv[0]), int.Parse(kv[1]));
            }

            return dict;
        }

        public IEnumerator CloneCharacterDbById(Coroutine coroutine, LogicService _this, CloneCharacterDbByIdInMessage msg)
        {
            var fromId = msg.Request.FromId;
            var toId = msg.Request.ToId;
            var result = AsyncReturnValue<CharacterController>.Create();
            var co = CoroutineFactory.NewSubroutine(CharacterManager.Instance.GetOrCreateCharacterController, coroutine,
            fromId, new object[] { }, false, result);

            if (co.MoveNext())
            {
                yield return co;
            }

            if (result.Value == null)
            {
                msg.Reply((int)ErrorCodes.Error_PrepareEnterGameFailed);
                yield break;
            }
            result.Dispose();

            var  dataItem = new CharacterManager.DataItem();
            dataItem.Controller = result.Value;
            dataItem.SimpleData = result.Value.GetSimpleData();
            var id = dataItem.Controller.mDbData.Id;
            dataItem.Controller.mDbData.Id = toId;
            var co3 = CoroutineFactory.NewSubroutine(CharacterManager.Instance.SaveDataForClone, coroutine, toId, dataItem, true);
            if (co3.MoveNext())
            {
                yield return co3;
            }

            dataItem.Controller.mDbData.Id = id;

            msg.Reply((int) ErrorCodes.OK);
        }

        public IEnumerator OnServerStart(Coroutine coroutine, LogicService _this)
        {
            //Thread.Sleep(GlobalVariable.WaitToConnectTimespan);
            LogicServer.Instance.Start(_this);

            CharacterManager.Instance.Init(LogicServer.Instance.DB, DataCategory.LogicCharacter);
            //---------test----------
            PetMission2.InitByTable();
            //PetMissionManager.Init();
            StoneManager.Init();
            PetPiece.Init();
            Talent.Init();
            Attributes.Init();
            MissionManager.Init();
            MailManager.Init();
            AchievementManager.Init();
            ConditionManager.Init();
            GameMaster.Init();
            NodeFlag.Init();
            Exdata.Init();
            BagManager.staticInit();
	        OperationActivityManager.Instance.Init();
            BookManager.StaticInit();

            ((LogicServerControl) _this).TaskManager.Init(LogicServer.Instance.DB, CharacterManager.Instance,
                DataCategory.Logic,
                (int) LogicServer.Instance.Id, Logger, ((LogicServerControl) _this).ApplyTasks);
            
            InitGiftCodeItem();
            InitFirstChargeItem();

            LogicServer.Instance.IsReadyToEnter = true;
            _this.TickDuration = 0.05f;

            Console.WriteLine("LogicServer startOver. [{0}]", LogicServer.Instance.Id);

            //计算每个服务器的平均等级
            var waitServers = true;
            while (waitServers)
            {
                LogicServer.Instance.AreAllServersReady(ready =>
                {
                    if (ready)
                    {
                        waitServers = false;
                        QueryAllianceWarInfo();
                    }
                });

                yield return LogicServer.Instance.ServerControl.Wait(coroutine, TimeSpan.FromSeconds(5));
            }


            _this.Started = true;

            Console.WriteLine("AllServer startOver. ");
        }

        public IEnumerator Tick(Coroutine co, ServerAgentBase _this)
        {
            var __this = ((LogicServerControl) _this);

            __this.TickCount++;
            try
            {
                LogicServerControl.Timer.Update();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Tick error.");
            }
            try
            {
                CharacterManager.Instance.Tick();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "CharacterManager tick error");
            }
            try
            {
                // 每秒Tick一次
                if (__this.TickCount%20 == 0)
                {
                    ((LogicServerControl) _this).TaskManager.Tick();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "TimedTaskManager tick error");
            }

			try
			{
				//30秒
				if (__this.TickCount % 600 == 0)

				{
					OperationActivityManager.Instance.Tick();
				}
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "TimedTaskManager tick error");
			}

			LogicServerMonitor.TickRate.Mark();

            return null;
        }

        public IEnumerator SSSendMailById(Coroutine coroutine, LogicService _this, SSSendMailByIdInMessage msg)
        {
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (character != null)
            {
                character.mMail.PushMail(msg.Request.TableId, msg.Request.ExtendType, msg.Request.ExtendPara0, msg.Request.ExtendPara1);
            }
            return null;
        }

        public IEnumerator NotifyPlayerMoniterData(Coroutine coroutine, LogicService _this,NotifyPlayerMoniterDataInMessage msg)
        {
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (character != null)
            {
                character.SetMoniterData(msg.Request.Data);
            }
            return null;


        }

        public IEnumerator GetPlayerMoniterData(Coroutine coroutine, LogicService _this, GetPlayerMoniterDataInMessage msg)
        {
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (character != null)
            {
                msg.Response = character.moniterData;
            }
            msg.Reply();
            yield break;
        }
        public IEnumerator OnServerStop(Coroutine coroutine, LogicService _this)
        {
            try
            {
                var dict = CharacterManager.Instance.mDictionary;
                if (dict != null)
                {
                    foreach (var data in dict)
                    {
                        try
                        {
                            if (data.Value == null)
                            {
                                continue;
                            }

                            if (data.Value.Controller == null)
                            {
                                //Logger.Error("ForeachCharacter is null characterId={0}", dataItem.Key);
                                continue;
                            }

                            if (!data.Value.Controller.Online)
                            {
                                continue;
                            }

                            data.Value.Controller.OutLine();
                        }
                        catch (Exception ex)
                        {
                            Logger.Warn(ex, "Logic OnServerStop got a exception.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Logic OnServerStop got a exception. 2");
            }
            

            var co = CoroutineFactory.NewSubroutine(CharacterManager.Instance.SaveAllCharacter, coroutine,
                default(TimeSpan));
            if (co.MoveNext())
            {
                yield return co;
            }
            LogicServer.Instance.DB.Dispose();
        }

        public IEnumerator PrepareDataForLogout(Coroutine coroutine,
                                                LogicService _this,
                                                PrepareDataForLogoutInMessage msg)
        {
            msg.Reply();
            yield break;
        }

        public IEnumerator CreateCharacter(Coroutine coroutine, LogicService _this, CreateCharacterInMessage msg)
        {
            var characterId = msg.CharacterId;
            PlayerLog.WriteLog(characterId, "----------CreateCharacter.0-----Logic-----");
            var result = AsyncReturnValue<CharacterController>.Create();
            var co = CoroutineFactory.NewSubroutine(CharacterManager.Instance.GetOrCreateCharacterController, coroutine,
				characterId, new object[] { msg.Request.Type, msg.Request.ServerId,msg.Request.IsGM }, true, result);

            if (co.MoveNext())
            {
                yield return co;
            }
            PlayerLog.WriteLog(characterId, "----------CreateCharacter.1-----Logic-----");

            if (result.Value == null)
            {
                PlayerLog.WriteLog(characterId, "----------CreateCharacter-------Logic---Error--");
                msg.Reply((int) ErrorCodes.Error_PrepareEnterGameFailed);
                yield break;
            }
            result.Dispose();
            var co1 = CoroutineFactory.NewSubroutine(CharacterManager.Instance.RemoveCharacter, coroutine, characterId);
            if (co1.MoveNext())
            {
                yield return co1;
            }
            msg.Reply();
            PlayerLog.WriteLog(characterId, "----------CreateCharacter.2-----Logic-----");
        }

        public IEnumerator DelectCharacter(Coroutine coroutine, LogicService _this, DelectCharacterInMessage msg)
        {
            var characterId = msg.CharacterId;
            PlayerLog.WriteLog(characterId, "-----L:ogic-----DelectCharacter----------{0}", characterId);
            var co1 = CoroutineFactory.NewSubroutine(CharacterManager.Instance.DeleteCharacter, coroutine, characterId);
            if (co1.MoveNext())
            {
                yield return co1;
            }
            msg.Reply();
        }

        public IEnumerator GetLogicSimpleData(Coroutine coroutine, LogicService _this, GetLogicSimpleDataInMessage msg)
        {
            var response = msg.Response;
            var targetId = msg.CharacterId;
            response.Id = targetId;
            CharacterManager.Instance.GetSimpeData(targetId, simple =>
            {
                if (simple == null)
                {
                    Logger.Error("GetLogicSimpleData guid={0}", targetId);
                    msg.Reply((int) ErrorCodes.Error_CharacterNotFind);
                    return;
                }
                response.EquipsModel.AddRange(simple.EquipsModel);
                response.Exdatas.AddRange(simple.Exdatas);
                response.Level = simple.Level;
                response.Name = simple.Name;
                response.Ladder = simple.Ladder;
                response.TypeId = simple.TypeId;
                response.Vip = simple.Vip;
                response.StarNum = simple.StarNum;
                response.TitleList.Clear();
                if (simple.TitleList != null)
                {
                    response.TitleList.AddRange(simple.TitleList);
                }

                var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(targetId);
                if (character == null)
                {
                    response.Online = 0;
                }
                else
                {
                    response.Online = character.Proxy == null ? 0 : 1;
                }
                if (response.Equips == null)
                {
                    response.Equips = new ItemsChangeData();
                }
                if (simple.Equips != null)
                {
                    response.Equips.ItemsChange.AddRange(simple.Equips.ItemsChange);
                }
                response.City = new BuildingList();
                if (simple.City != null)
                {
                    response.City.Data.AddRange(simple.City.Data);
                }
                if (simple.Skills != null)
                {
                    response.Skills.AddRange(simple.Skills);
                }
                response.WorshipCount = simple.WorshipCount;
                if (response.Exchange == null)
                {
                    response.Exchange = new OtherStoreList();
                }
                response.Exchange.SellCharacterId = simple.Id;
                response.Exchange.SellCharacterName = simple.Name;
                if (simple.Exchange != null)
                {
                    foreach (var item in simple.Exchange.StoreItems)
                    {
                        response.Exchange.Items.Add(new OtherStoreOne
                        {
                            Id = item.Id,
                            ItemData = item.ItemData,
                            NeedCount = item.NeedCount,
                            State = item.State,
                            ManagerId = item.ManagerId,
                            NeedType = item.NeedType
                        });
                    }
                }

                response.MountId = simple.MountId;

                msg.Reply();
            });
            return null;
        }

        public IEnumerator CheckConnected(Coroutine coroutine, LogicService _this, CheckConnectedInMessage msg)
        {
            Logger.Error("Logic CheckConnected, {0}", msg.CharacterId);

            //LogicCharacterProxy proxy = null;
            //if (_this.Proxys.TryGetValue(msg.CharacterId, out proxy))
            //{
            //    if ((proxy as LogicProxy).Connected)
            //    {
            //        msg.Response = 1;
            //        msg.Reply();
            //        return null;
            //    }

            //   // (proxy as LogicProxy).WaitingCheckConnectedInMessages.Add(msg);
            //}
            msg.Reply((int) ErrorCodes.Unline);

            return null;
        }

        public IEnumerator CheckLost(Coroutine coroutine, LogicService _this, CheckLostInMessage msg)
        {
            Logger.Error("Logic CheckLost, {0}", msg.CharacterId);

            //LogicCharacterProxy proxy = null;
            //if (!_this.Proxys.TryGetValue(msg.CharacterId, out proxy))
            //{
            //    msg.Reply();
            //}
            //else
            //{
            //    if ((proxy as LogicProxy).Connected)
            //    {
            //        (proxy as LogicProxy).WaitingCheckLostInMessages.Add(msg);
            //    }
            //    else
            //    {
            //        msg.Reply();
            //    }
            //}
            yield break;
        }

        public IEnumerator QueryStatus(Coroutine coroutine, LogicService _this, QueryStatusInMessage msg)
        {
            yield break;
        }

        public IEnumerator PrepareDataForEnterGame(Coroutine co,
                                                   LogicService _this,
                                                   PrepareDataForEnterGameInMessage msg)
        {
            PlayerLog.WriteLog(msg.CharacterId, "-----Logic-----PrepareDataForEnterGame.0----------");
            var result = AsyncReturnValue<CharacterController>.Create();
            var co1 = CoroutineFactory.NewSubroutine(CharacterManager.Instance.GetOrCreateCharacterController, co,
                msg.CharacterId, new object[] {}, false, result);
            if (co1.MoveNext())
            {
                yield return co1;
            }

            PlayerLog.WriteLog(msg.CharacterId, "-----Logic-----PrepareDataForEnterGame.1----------");

            if (result.Value == null)
            {
                msg.Reply((int) ErrorCodes.Error_PrepareEnterGameFailed);
                yield break;
            }

            var character = result.Value;
            result.Dispose();
            character.serverId = msg.Request.ServerId;
            //设置玩家开服时间
            character.lExdata64.SetTime(Exdata64TimeType.ServerStartTime,
                DateTime.Parse(Table.GetServerName(character.serverId).OpenTime));
            character.mMail.GmMail();

            //检查副本未完成标记位
            var data421 = character.GetExData((int) eExdataDefine.e421);
            if (data421 != 0)
            {
                PlayerLog.WriteLog(msg.CharacterId, "-----Logic-----PrepareDataForEnterGame.2----------");
                //上次的副本未完成，则要检查一下，副本是否还存在
                character.SetExData((int) eExdataDefine.e421, 0);
                var sceneGuid = msg.Request.SceneGuid;
                var msg1 = LogicServer.Instance.SceneAgent.IsSceneExist(character.mGuid, sceneGuid);
                yield return msg1.SendAndWaitUntilDone(co);
                PlayerLog.WriteLog(msg.CharacterId, "-----Logic-----PrepareDataForEnterGame.3----------");
                if (msg1.State != MessageState.Reply)
                {
                    Logger.Error("SceneAgent.IsSceneExist did not reply!");
                }
                else if (msg1.ErrorCode != (int) ErrorCodes.OK)
                {
                    Logger.Error("SceneAgent.IsSceneExist ErrorCode = {0}!", msg1.ErrorCode);
                }
                else if (!msg1.Response)
                {
                    //副本已经不存在了，说明玩家该次副本被异常中断了，这时，需要给玩家补上副本材料
                    var tbFuben = Table.GetFuben(data421);
                    var items = new Dictionary<int, int>();
                    for (int i = 0, imax = tbFuben.NeedItemId.Length; i < imax; ++i)
                    {
                        var itemId = tbFuben.NeedItemId[i];
                        var itemCount = tbFuben.NeedItemCount[i];
                        if (itemId == -1)
                        {
                            break;
                        }
                        items.modifyValue(itemId, itemCount);
                    }
                    PlayerLog.WriteLog((int) LogType.ReturnDungeonCostMail, "Return dungeon cost!");
                    character.mBag.AddItemOrMail(0, items, null, eCreateItemType.ReturnDungeonRequire);
                }
            }

            PlayerLog.WriteLog(msg.CharacterId, "-----Logic-----PrepareDataForEnterGame.4----------");

            msg.Reply();
        }

        public IEnumerator PrepareDataForCreateCharacter(Coroutine coroutine,
                                                         LogicService _this,
                                                         PrepareDataForCreateCharacterInMessage msg)
        {
            PlayerLog.WriteLog(msg.CharacterId, "----------PrepareDataForCreateCharacter----------{0}", msg.CharacterId);
            var result = AsyncReturnValue<CharacterController>.Create();
            var co = CoroutineFactory.NewSubroutine(CharacterManager.Instance.GetOrCreateCharacterController, coroutine,
                msg.CharacterId, new object[] {msg.Request.Type}, true, result);

            if (co.MoveNext())
            {
                yield return co;
            }

            if (result.Value == null)
            {
                msg.Reply((int) ErrorCodes.Error_PrepareEnterGameFailed);
                yield break;
            }
            result.Dispose();
            msg.Reply();
        }

        public IEnumerator PrepareDataForCommonUse(Coroutine coroutine,
                                                   LogicService _this,
                                                   PrepareDataForCommonUseInMessage msg)
        {
            PlayerLog.WriteLog(msg.CharacterId, "----------PrepareDataForCommonUse----------{0}", msg.CharacterId);
            msg.Reply();
            return null;
        }

        //获得装备数据
        public IEnumerator LogicGetEquipList(Coroutine coroutine, LogicService _this, LogicGetEquipListInMessage msg)
        {
            Logger.Info("Enter Game {0} - LogicGetEquipList - 1 - {1}", msg.Request.ChararcterId,
                TimeManager.Timer.ElapsedMilliseconds);
            var Id = msg.Request.ChararcterId;
            PlayerLog.WriteLog(Id, "----------Scene2Logic-----LogicGetEquipList----------");
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(Id);
            if (cl == null)
            {
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }
            var bag = cl.mBag;
            foreach (var i in EquipExtension.Equips)
            {
                BagBase thisBagBase;
                if (!bag.mBags.TryGetValue(i, out thisBagBase))
                {
                    Logger.Error("LogicGetEquipList CharacterId={0}  BagIndex={1} not find", Id, i);
                    continue;
                }
                foreach (var ib in thisBagBase.mLogics)
                {
                    if (ib.GetId() < 0)
                    {
                        continue;
                    }
                    var ibd = new ItemBaseData
                    {
                        ItemId = ib.GetId(),
                        Count = ib.GetCount(),
                        Index = i*10 + ib.GetIndex()
                    };
                    ib.CopyTo(ibd.Exdata);
                    ib.ReCalcBuff();
                    msg.Response.Items.Add(ibd);
                }
                //for (int j = 0; j != 2; ++j)
                //{
                //    ItemBase ib = thisBagBase.GetItemByIndex(j);
                //    if (ib == null) continue;
                //    ItemBaseData ibd = new ItemBaseData()
                //    {
                //        ItemId = ib.GetId(),
                //        Count = ib.GetCount(),
                //        Index = i * 10
                //    };
                //    ib.CopyTo(ibd.Exdata);
                //    msg.Response.Items.Add(ibd);
                //}
            }
            Logger.Info("Enter Game {0} - LogicGetEquipList - 2 - {1}", msg.Request.ChararcterId,
                TimeManager.Timer.ElapsedMilliseconds);
            msg.Reply();
        }

        //获得技能数据
        public IEnumerator LogicGetSkillData(Coroutine coroutine, LogicService _this, LogicGetSkillDataInMessage msg)
        {
            var Id = msg.Request.ChararcterId;
            PlayerLog.WriteLog(Id, "----------Scene2Logic-----LogicGetSkillData----------");
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(Id);
            if (cl == null)
            {
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }
            //---Todo  //初始化技能 不用所有技能都会，需要会普攻，被动，和EquipSkills
            foreach (var i in cl.mSkill.mDbData.Skills)
            {
                if (i.Value > 0)
                {
                    var tbskill = Table.GetSkill(i.Key);
                    if (tbskill == null)
                    {
                        continue;
                    }
                    if (BitFlag.GetLow(tbskill.ControlType, 1))
                    {
                        msg.Response.Data[i.Key] = i.Value;
                    }
                    else if (tbskill.CastType == 3)
                    {
                        msg.Response.Data[i.Key] = i.Value;
                    }
                }
            }

            foreach (var i in cl.mSkill.mDbData.EquipSkills)
            {
                if (i == -1)
                {
                    continue;
                }
                var nLevel = cl.mSkill.GetSkillLevel(i);
                if (nLevel > 0)
                {
                    msg.Response.Data[i] = nLevel;
                }
            }
            msg.Response.Data.Add(-1, cl.mBag.GetRes(eResourcesType.LevelRes));
            msg.Response.Data.Add(-2, cl.GetExData((int) eExdataDefine.e51));
            msg.Response.Data.Add(-3, cl.GetExData((int) eExdataDefine.e250));
            msg.Response.Data.Add(-4, cl.mBag.GetRes(eResourcesType.VipLevel));
            if (cl.GetFlag(2682))//是否充值终身卡
            {
                Logger.Warn("LogicServerCtr 875 flag : {0}", cl.GetFlag(2682));
                msg.Response.Data.Add(-5, 1);
            }
            //foreach (var i in cl.mSkill.mDbData.Skills)
            //{
            //    if (i.Value > 0)
            //    {
            //        msg.Response.Data[i.Key] = i.Value;
            //    }
            //}
            //msg.Response.Data.Add(-1, cl.mBag.GetRes(eResourcesType.LevelRes));
            msg.Reply();
        }

        //获得天赋数据
        public IEnumerator LogicGetTalentData(Coroutine coroutine, LogicService _this, LogicGetTalentDataInMessage msg)
        {
            var Id = msg.Request.ChararcterId;
            PlayerLog.WriteLog(Id, "----------Scene2Logic-----LogicGetTalentData----------");
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(Id);
            if (cl == null)
            {
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }
            foreach (var i in cl.mTalent.Talents)
            {
                msg.Response.Data[i.Key] = i.Value;
            }
            msg.Reply();
        }

        public IEnumerator LogicGetTitleList(Coroutine coroutine, LogicService _this, LogicGetTitleListInMessage msg)
        {
            var id = msg.CharacterId;
            PlayerLog.WriteLog(id, "----------Scene2Logic-----LogicGetTitleList----------");
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(id);
            if (cl == null)
            {
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }

            var result = LogicServer.Instance.ActivityAgent.SSApplyLastResult(id, cl.serverId);
            yield return result.SendAndWaitUntilDone(coroutine);
            if (result.State == MessageState.Reply && result.ErrorCode == (int)ErrorCodes.OK)
            {
                var failFlag = cl.GetMieShiFailTitleFlag();
                var endTime = DateTime.FromBinary(result.Response.EndTime);
                if (result.Response.Result == 1)
                {
                    cl.SetTitleFlag(failFlag, false, 0, endTime);
                }
                else if (result.Response.Result == 0)
                {
                    cl.SetTitleFlag(failFlag, true, 0, endTime);
                }
            }
            else
            {
                Logger.Error("SSApplyLastResult() return with ErrorCode = {0}", result.ErrorCode);
            }

            cl.RemoveOverTimeTitles();

            msg.Response.Titles = new Int32Array();
            msg.Response.Titles.Items.AddRange(cl.mDbData.Titles.Keys);
            msg.Response.EquipedTitles = new Int32Array();
            msg.Response.EquipedTitles.Items.AddRange(cl.mDbData.ViewTitles.Where(t => t >= 0));
            msg.Reply();
        }

        //请求图鉴属性数据
        public IEnumerator LogicGetBookAttrData(Coroutine coroutine,
                                                LogicService _this,
                                                LogicGetBookAttrDataInMessage msg)
        {
            var Id = msg.Request.ChararcterId;
            PlayerLog.WriteLog(Id, "----------Scene2Logic-----LogicGetBookAttrData----------");
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(Id);
            if (cl == null)
            {
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }

            var fightId = cl.GetBooksAttr(msg.Response.bookAttrs, msg.Response.monsterAttrs);
	        msg.Response.fightId = fightId;
            msg.Reply();
        }

        public IEnumerator LogicGetElfData(Coroutine coroutine, LogicService _this, LogicGetElfDataInMessage msg)
        {
            var Id = msg.Request.ChararcterId;
            PlayerLog.WriteLog(Id, "----------Scene2Logic-----LogicGetElfData----------");
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(Id);
            if (cl == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }
            cl.GetElfBuff(msg.Response.Buff);
            cl.SetRefreshFightPoint(true);
            msg.Response.FightPoint = cl.GetElfFightPoint();
            msg.Reply();
        }
        public IEnumerator LogicGetMountData(Coroutine coroutine, LogicService _this, LogicGetMountDataInMessage msg)
        {
            var Id = msg.Request.ChararcterId;
            PlayerLog.WriteLog(Id, "----------Scene2Logic-----LogicGetElfData----------");
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(Id);
            if (cl == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }
            cl.GetMountBuff(msg.Response.Buff);
            msg.Response.MountId = cl.mMount.mDbData.Ride;
            msg.Reply();
        }

        //杀怪事件
        public IEnumerator LogicKillMonster(Coroutine coroutine, LogicService _this, LogicKillMonsterInMessage msg)
        {
            var characterId = msg.CharacterId;
            var exp = msg.Request.AddExp;
//             PlayerLog.WriteLog(characterId, "----------Scene2Logic-----LogicKillMonster----------mId={0},exp={1}",
//                 msg.Request.MonsterId, exp);
            var tbMonster = Table.GetNpcBase(msg.Request.MonsterId);
            if (tbMonster == null)
            {
                yield break;
            }
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (cl == null)
            {
                yield break;
            }
            var e = new KillMonster(cl, msg.Request.MonsterId);
            EventDispatcher.Instance.DispatchEvent(e);
            cl.AddExData((int) eExdataDefine.e30, 1);
            cl.AddExData((int) eExdataDefine.e45, 1);
            switch (tbMonster.Id)
            {
                case 1000:
                {
                    cl.AddExData((int) eExdataDefine.e58, 1);
                }
                    break;
            }
            cl.mBag.AddExp(exp, eCreateItemType.PickUp);

            var tbScene = Table.GetScene(msg.Request.SceneId);
            if (tbScene.FubenId != -1)
            {
                var tbFuben = Table.GetFuben(tbScene.FubenId);
                if (tbFuben != null && tbFuben.AssistType == (int) eDungeonAssistType.AncientBattlefield)
                {
//检查古战场经验
                    var exDataIdx = (int) eExdataDefine.e435;
                    var newValue = cl.GetExData(exDataIdx) + exp;
                    cl.SetExData(exDataIdx, newValue);
                    if (newValue >= tbFuben.ScriptId)
                    {
//如果经验超出了今日上限，就踢出副本，不让进了
                        var quitMsg = LogicServer.Instance.SceneAgent.SSExitDungeon(cl.mGuid, 0);
                        yield return quitMsg.SendAndWaitUntilDone(coroutine);
                        if (quitMsg.State != MessageState.Reply)
                        {
                            Logger.Error("SSExitDungeon reture with State = {0}", quitMsg.State);
                            yield break;
                        }
                        if (quitMsg.ErrorCode != (int) ErrorCodes.OK)
                        {
                            Logger.Error("SSExitDungeon reture with ErrorCode = {0}", quitMsg.ErrorCode);
                        }
                    }
                }
            }
        }

        //给予物品(目前用于Scene拾取物品)
        public IEnumerator GiveItem(Coroutine coroutine, LogicService _this, GiveItemInMessage msg)
        {
            var Id = msg.CharacterId;
            //PlayerLog.WriteLog(Id, "----------Scene2Logic-----GiveItem----------");
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(Id);
            if (cl == null)
            {
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }

            var errorCodes = cl.mBag.AddItem(msg.Request.ItemId, msg.Request.ItemCount, msg.Request.From > 0 ? (eCreateItemType)msg.Request.From : eCreateItemType.PickUp);
            msg.Response = (int) errorCodes;
            msg.Reply();
        }

        public IEnumerator GiveItemList(Coroutine coroutine, LogicService _this, GiveItemListInMessage msg)
        {
            var Id = msg.CharacterId;
            //PlayerLog.WriteLog(Id, "----------Scene2Logic-----GiveItem----------");
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(Id);
            if (cl == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }
                cl.mBag.AddItems(msg.Request.Items.Data,
                    msg.Request.From > 0 ? (eCreateItemType) msg.Request.From : eCreateItemType.PickUp);
            msg.Reply();

        }
        //删除道具靠索引
        public IEnumerator SSDeleteItemByIndex(Coroutine coroutine, LogicService _this, SSDeleteItemByIndexInMessage msg)
        {
            var Id = msg.CharacterId;
            PlayerLog.WriteLog(Id, "----------Scene2Logic-----SSDeleteItemByIndex----------{0},{1},{2}",
                msg.Request.BagId, msg.Request.BagIndex, msg.Request.ItemCount);
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(Id);
            if (cl == null)
            {
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }
            var bag = cl.GetBag(msg.Request.BagId);
            if (bag == null)
            {
                msg.Reply((int) ErrorCodes.Error_BagID);
                yield break;
            }
            var item = bag.GetItemByIndex(msg.Request.BagIndex);
            if (item == null || item.GetId() == -1)
            {
                msg.Reply((int) ErrorCodes.ItemNotEnough);
                yield break;
            }
            if (item.GetCount() < msg.Request.ItemCount)
            {
                msg.Reply((int) ErrorCodes.ItemNotEnough);
                yield break;
            }
            bag.ReduceCountByIndex(msg.Request.BagIndex, msg.Request.ItemCount, eDeleteItemType.UseItem);
            msg.Reply();
        }

        //请求某个任务可以相位到的场景
        public IEnumerator SSGetMissionEnterScene(Coroutine coroutine,
                                                  LogicService _this,
                                                  SSGetMissionEnterSceneInMessage msg)
        {
            var Id = msg.CharacterId;
            PlayerLog.WriteLog(Id, "----------Scene2Logic-----SSGetMissionEnterScene----------{0}",
                msg.Request.MissionId);
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(Id);
            if (cl == null)
            {
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }
            var m = cl.mTask.GetMission(msg.Request.MissionId);
            if (m == null)
            {
                msg.Reply((int) ErrorCodes.Error_NotHaveMission);
                yield break;
            }
            var tbMis = Table.GetMission(m.Id);
            if (tbMis.FinishCondition == (int)eMissionType.Tollgate)
            {
                int fubenId = tbMis.FinishParam[0];
                var tabCopy = Table.GetFuben(fubenId);
                msg.Response = tabCopy.SceneId;
                msg.Reply();
                yield break;
            }
                if (tbMis.FinishCondition != (int) eMissionType.Dungeon)
            {
                msg.Reply((int) ErrorCodes.Error_NotHaveMission);
                yield break;
            }
            msg.Response = tbMis.FinishParam[0];
            msg.Reply();
        }

        //删除道具
        public IEnumerator DeleteItem(Coroutine coroutine, LogicService _this, DeleteItemInMessage msg)
        {
            if (msg.Request == null)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            var Id = msg.CharacterId;
            PlayerLog.WriteLog(Id, "----------Scene2Logic-----DeleteItem----------{0},{1}", msg.Request.ItemId,
                msg.Request.ItemCount);
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(Id);
            if (cl == null)
            {
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }
            if (cl.mBag.GetItemCount(msg.Request.ItemId) < msg.Request.ItemCount)
            {
                msg.Response = (int)ErrorCodes.ItemNotEnough;
                msg.Reply((int) ErrorCodes.ItemNotEnough);
                yield break;
            }

            var errorCodes = cl.mBag.DeleteItem(msg.Request.ItemId, msg.Request.ItemCount, (eDeleteItemType)msg.Request.DeleteType);
            msg.Response = (int) errorCodes;
            msg.Reply(msg.Response);
        }

        //第一次登陆
        public IEnumerator FirstOnline(Coroutine coroutine, LogicService _this, FirstOnlineInMessage msg)
        {
            var ContinuedLanding = msg.Request.ContinuedLanding; //持续登陆天数
            var Id = msg.CharacterId;
            PlayerLog.WriteLog(Id, "----------Loginc2Logic-----FirstOnline----------continueDays={1},characterId={2}", ContinuedLanding, Id);
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(Id);
            if (cl == null)
            {
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }

          //  var firstOnline = false;
          //  var lastTime = cl.lExdata64.GetTime(Exdata64TimeType.FirstOnlineTime);
          //  if (lastTime == DateTime.MinValue)
          //  { // 第一次登陆游戏
           //     firstOnline = true;
           // }

            cl.DailyFirstOnline(ContinuedLanding);
          //  msg.Response = (int) ErrorCodes.OK;
          //  msg.Reply();
          //  if (firstOnline)
          //  {
                PlayerLog.WriteLog(Id, "----------Loginc2Logic-----OldPlayer----------characterId={1}", Id);
                var msg4 = LogicServer.Instance.GameMasterAgent.TakeOldPlayerReward(Id, msg.Request.ClientId);  // 
                yield return msg4.SendAndWaitUntilDone(coroutine);
          //  }
            msg.Reply();
        }

        //重载表格
		public IEnumerator ServerGMCommand(Coroutine coroutine, LogicService _this, ServerGMCommandInMessage msg)
        {
			
//             if (msg.Request.TableName == "CM")
//             {
//                 Logger.Info("NetCount={0},CharacterCount={1}", ((LogicServerControl) _this).GetPlayerCount(),
//                     CharacterManager.Instance.CharacterCount());
//                 ((LogicServerControl) _this).LookProxy();
//                 CharacterManager.Instance.LookCharacters();
//                 yield break;
//             }
			var cmd = msg.Request.Cmd;
			var param = msg.Request.Param;

			Logger.Info("Logic----------ServerGMCommand----------cmd={0}|param={1}", cmd,param);


			try
			{
				if ("ReloadTable"==cmd)
				{
					Table.ReloadTable(param);

					if (param == "FirstRecharge")
					{
						InitFirstChargeItem();
					}
					else if (param == "GiftCode")
					{
						InitGiftCodeItem();
					}
					else if (param == "YunYing")
				    {
				        OperationActivityManager.Instance.Reload();
                    }
					else if (param == "OperationActivity")
				    {
				        OperationActivityManager.Instance.Reload();
                    }
					else if (param == "ServerName")
				    {
				        OperationActivityManager.Instance.Reload();
                    }
                    else if (param == "All")
					{
					    OperationActivityManager.Instance.Reload();
                    }
                    else if (param == "Store")
                    {
                        StoneManager.Init();
                    }
				}
				else if ("ReloadOperationActivity" == cmd)
				{
					OperationActivityManager.Instance.Reload();
				}
			}
			catch (Exception e)
			{

				Logger.Error("Logic----------ServerGMCommand----------error={0}", e.Message);
			}
			finally
			{

			}
            yield break;
        }

        //完成了副本
        public IEnumerator CompleteFuben(Coroutine co, LogicService _this, CompleteFubenInMessage msg)
        {
            var characterId = msg.CharacterId;
            var result = msg.Request.Result;
            var controller = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (controller == null)
            {
                CharacterManager.Instance.ModifyVolatileData(characterId, DataCategory.LogicCharacter, oldData =>
                {
                    //fuben result
                    oldData.FubenResult.Add(result);
                    return oldData;
                });
                yield break;
            }
            controller.CompleteFuben(result);
        }

        //耐久度下降
        public IEnumerator DurableDown(Coroutine coroutine, LogicService _this, DurableDownInMessage msg)
        {
            var Id = msg.CharacterId;
            //PlayerLog.WriteLog(Id, "----------Scene2Logic-----DurableDown----------");
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(Id);
            if (cl == null)
            {
                yield break;
            }
            foreach (var i in msg.Request.BagidList.Data)
            {
                cl.DurableDown(i.Key, i.Value);
            }
        }

        //NPC服务
        public IEnumerator NpcService(Coroutine coroutine, LogicService _this, NpcServiceInMessage msg)
        {
            var Id = msg.CharacterId;
            PlayerLog.WriteLog(Id, "----------Scene2Logic-----NpcService----------");
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(Id);
            if (cl == null)
            {
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }
            msg.Reply((int) cl.NpcService(msg.Request.ServiceId));
        }

        //抽奖
        public IEnumerator PushDraw(Coroutine coroutine, LogicService _this, PushDrawInMessage msg)
        {
            var Id = msg.CharacterId;
            PlayerLog.WriteLog(Id, "----------Scene2Logic-----PushDraw----------{0}", Id);
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(Id);
            if (cl == null)
            {
                yield break;
            }
            ItemBase tempItemBase;
            cl.PushDraw(msg.Request.DrawId, out tempItemBase);
        }

        //判断玩家是否能进某副本
        public IEnumerator CheckCharacterInFuben(Coroutine coroutine,
                                                 LogicService _this,
                                                 CheckCharacterInFubenInMessage msg)
        {
            var Id = msg.CharacterId;
            PlayerLog.WriteLog(Id, "----------Team2Logic-----CheckCharacterInFuben----------");
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(Id);
            if (cl == null)
            {
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }
            int delailMsg=0;
            int error = (int)cl.CheckFubenDetail(msg.Request.FubenId, ref delailMsg);
            msg.Response=delailMsg;
            msg.Reply(error,true);
            //msg.Reply((int)cl.CheckFuben(msg.Request.FubenId));
        }

        //天梯结果
        public IEnumerator LogicP1vP1FightOver(Coroutine coroutine, LogicService _this, LogicP1vP1FightOverInMessage msg)
        {
            var Id = msg.CharacterId;
            var result = msg.Request.Result;
            PlayerLog.WriteLog(Id, "----------Team2Logic-----LogicP1vP1FightOver----------{0}", result);
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(Id);
            if (cl == null)
            {
                Logger.Warn("LogicP1vP1FightOver not find character ={0}", Id);
                yield break;
            }
            Logger.Warn("LogicP1vP1FightOver todo! character ={0}", Id);
            if (msg.Request.Result == 1)
            {
                //胜利
                //cl.GiveP1vP1Win(20);
            }
            //var msgTeamChgScene = LogicServer.Instance.RankAgent.RankP1vP1FightOver(msg.CharacterId, cl.serverId, msg.CharacterId, msg.Request.CharacterId, msg.Request.Result);
            //yield return msgTeamChgScene.SendAndWaitUntilDone(coroutine);
        }

        //天梯名次有所前进
        public IEnumerator LogicP1vP1LadderAdvance(Coroutine coroutine,
                                                   LogicService _this,
                                                   LogicP1vP1LadderAdvanceInMessage msg)
        {
            var Id = msg.CharacterId;
            var rank = msg.Request.Rank;
            PlayerLog.WriteLog(Id, "----------Team2Logic-----LogicP1vP1LadderAdvance----------{0}", rank);
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(Id);
            if (cl == null)
            {
                Logger.Warn("LogicP1vP1LadderAdvance not find character ={0}", Id);
                yield break;
            }
            var oldData = cl.GetExData(93);
            Logger.Info("LogicP1vP1LadderAdvance todo! character ={0},oldRank={1},newRank={2}", Id, rank, oldData);
            if (oldData == -1 || oldData > rank)
            {
                cl.SetExData(93, rank);
            }
        }

        //记录天梯变化
        public IEnumerator PushP1vP1LadderChange(Coroutine coroutine,
                                                 LogicService _this,
                                                 PushP1vP1LadderChangeInMessage msg)
        {
            var Id = msg.CharacterId;
            var oldRank = msg.Request.OldRank;
            var newRank = msg.Request.NewRank;
           
            PlayerLog.WriteLog(Id, "----------Team2Logic-----PushP1vP1LadderChange----------{0}->{1}", oldRank, newRank);
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(Id);
            if (cl != null)
            {
                if (msg.Request.Type == 0)
                {
                    //主动方要做的事情
                    var oldData = cl.GetExData(93);
                    if (oldData == -1 || oldData > newRank && newRank != -1)
                    {
                        var reward = Reward.GetMaxLadder(oldData, newRank);
                        if (reward > 0)
                        {
                            if (oldData >= 1000)
                            {
                                var tbMail = Table.GetMail(96);
                                cl.mMail.PushMail(tbMail.Title, string.Format(tbMail.Text, newRank),
                                    new Dictionary<int, int> {{(int) eResourcesType.DiamondBind, reward}}, tbMail.Sender);
                            }
                            else
                            {
                                var tbMail = Table.GetMail(99);
                                cl.mMail.PushMail(tbMail.Title, string.Format(tbMail.Text, newRank, oldData - newRank),
                                    new Dictionary<int, int> {{(int) eResourcesType.DiamondBind, reward}}, tbMail.Sender);
                            }
                        }
                        cl.SetExData(93, newRank);
                    }
                    var data = new P1vP1RewardData();
                    if (msg.Request.Result != 1)
                    {
                        cl.GiveP1vP1Lost(newRank, data.Items);
                    }
                    else
                    {
                        cl.GiveP1vP1Win(newRank, data.Items);
                    }

                    if (cl.Proxy != null)
                    {
                        data.NewRank = newRank;
                        data.OldRank = oldRank;
                        data.Result = msg.Request.Result;
                        data.OpponentName = msg.Request.Name;
                        cl.Proxy.LogicP1vP1FightResult(data);
                    }
                }
                //存储记录
                var one = cl.PushP1vP1Change(msg.Request.Type, msg.Request.Name, oldRank, newRank);
                //通知被动方
                if (msg.Request.Type == 1 && cl.Proxy != null)
                {
                    cl.Proxy.NotifyP1vP1Change(one);
                }
                yield break;
            }
            //不在线时的处理
            //读取玩家等级
            var logicSimple = LogicServer.Instance.LogicAgent.GetLogicSimpleData(msg.CharacterId, 0);
            yield return logicSimple.SendAndWaitUntilDone(coroutine);
            if (logicSimple.State != MessageState.Reply)
            {
                Logger.Warn("PushP1vP1LadderChange GetLogicSimpleData False! guid={0}", msg.CharacterId);
                yield break;
            }

            //不在线的存储记录
            CharacterManager.Instance.ModifyVolatileData(msg.CharacterId, DataCategory.LogicCharacter, oldData =>
            {
                if (msg.Request.Type == 0)
                {
                    var tempmail = new DBMail_One();
                    tempmail.StartTime = DateTime.Now.ToBinary();
                    tempmail.State = 0;
                    tempmail.Name = "P1vP1";
                    tempmail.Text = "Reward";
                    tempmail.OverTime = DateTime.Now.AddDays(15).ToBinary();
                    var items = new Dictionary<int, int>();
                    if (msg.Request.Result != 1)
                    {
                        Reward.GiveP1vP1Lost(null, newRank, items, logicSimple.Response.Level);
                    }
                    else
                    {
                        Reward.GiveP1vP1Win(null, newRank, items, logicSimple.Response.Level);
                        oldData.ExdataChange.modifyValue((int) eExdataDefine.e49, 1);
                    }

                    foreach (var i in items)
                    {
                        var itemDb = new ItemBaseData();
                        ShareItemFactory.Create(i.Key, itemDb);
                        itemDb.Count = i.Value;
                        tempmail.Items.Add(itemDb);
                    }
                    oldData.NewMails.Add(tempmail);
                }
                var one = new P1vP1Change_One
                {
                    Type = msg.Request.Type,
                    Name = msg.Request.Name,
                    OldRank = oldRank,
                    NewRank = newRank
                };
                oldData.NewP1vP1s.Add(one);
                if (oldData.NewP1vP1s.Count > 50)
                {
                    oldData.NewP1vP1s.RemoveAt(0);
                }
                return oldData;
            });
        }

        //支持其他服务器获取标记位和获取条件表 1为条件通过
        public IEnumerator SSGetFlagOrCondition(Coroutine coroutine,
                                                LogicService _this,
                                                SSGetFlagOrConditionInMessage msg)
        {
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.Request.Guid);
            if (cl == null)
            {
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }
            var flagresult = cl.GetFlag(msg.Request.FlagId) ? 1 : 0;
            msg.Response = 0;
            if (flagresult == 1)
            {
                msg.Response = 1;
                msg.Reply();
                yield break;
            }
            if (msg.Request.Conditionid == -1)
            {
                msg.Reply();
                yield break;
            }
            var conditionresult = cl.CheckCondition(msg.Request.Conditionid) == -2 ? 1 : 0;
            if (conditionresult == 0)
            {
                msg.Response = 2;
            }
            msg.Reply();
        }

        //支持其他服务器获取条件表 1为条件通过
        //public IEnumerator SSGetCondition(Coroutine coroutine, LogicService _this, SSGetConditionInMessage msg)
        //{
        //    var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.Request.Guid);
        //    if (cl == null)
        //    {
        //        msg.Reply((int)ErrorCodes.Unline);
        //        yield break;
        //    }
        //    msg.Response = cl.CheckCondition(msg.Request.FlagId)==-2 ? 1 : 0;
        //    msg.Reply();
        //}
        //查询玩家是否屏蔽了另一个玩家
        public IEnumerator SSIsShield(Coroutine coroutine, LogicService _this, SSIsShieldInMessage msg)
        {
            var Id = msg.Request.Guid;
            PlayerLog.WriteLog(Id, "----------Chat2Logic-----SSIsShield----------{0}", msg.Request.Shield);
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.Request.Guid);
            if (cl == null)
            {
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }
            msg.Response = cl.mFriend.CheckAddShield(msg.Request.Shield) == ErrorCodes.OK ? 0 : 1;
            msg.Reply();
        }

        //修改扩展数据
        public IEnumerator SSChangeExdata(Coroutine co, LogicService _this, SSChangeExdataInMessage msg)
        {
            var Id = msg.CharacterId;
            //PlayerLog.WriteLog(Id, "----------Chat2Logic-----SSChangeExdata----------");
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(Id);
            if (cl != null)
            {
                foreach (var i in msg.Request.Changes.Data)
                {
                    cl.SetExData(i.Key, cl.GetExData(i.Key) + i.Value);
                }
                yield break;
            }
            CharacterManager.Instance.ModifyVolatileData(Id, DataCategory.LogicCharacter, oldData =>
            {
                foreach (var i in msg.Request.Changes.Data)
                {
                    oldData.ExdataChange.modifyValue(i.Key, i.Value);
                }
                return oldData;
            });
        }

        public IEnumerator ReadyToEnter(Coroutine coroutine, LogicService _this, ReadyToEnterInMessage msg)
        {
            if (LogicServer.Instance.IsReadyToEnter && LogicServer.Instance.AllAgentConnected())
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

        //获得所有有我的好友数据
        public IEnumerator SSGetFriendList(Coroutine coroutine, LogicService _this, SSGetFriendListInMessage msg)
        {
            var charController = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (charController == null)
            {
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }
            if (charController.mFriend.mDbData.BeHaveFriends.Count > 0)
            {
                var tempList = new Uint64Array();
                msg.Response.Data[0] = tempList;
                tempList.Items.AddRange(charController.mFriend.mDbData.BeHaveFriends);
            }
            if (charController.mFriend.mDbData.BeHaveEnemys.Count > 0)
            {
                var tempList = new Uint64Array();
                msg.Response.Data[1] = tempList;
                tempList.Items.AddRange(charController.mFriend.mDbData.BeHaveEnemys);
            }
            if (charController.mFriend.mDbData.BeHaveShield.Count > 0)
            {
                var tempList = new Uint64Array();
                msg.Response.Data[2] = tempList;
                tempList.Items.AddRange(charController.mFriend.mDbData.BeHaveShield);
            }
            msg.Reply();
        }

        public IEnumerator SSSendSimpleData(Coroutine coroutine, LogicService _this, SSSendSimpleDataInMessage msg)
        {
            var charController = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.Request.HaveId);
            if (charController != null)
            {
                var changeId = msg.Request.SimpleData.Id;
                charController.mFriend.PushDataChange(changeId, msg.Request.SimpleData);
                yield break;
            }
        }

        public IEnumerator SSFriendpPssiveChange(Coroutine coroutine,
                                                 LogicService _this,
                                                 SSFriendpPssiveChangeInMessage msg)
        {
            var characterId = msg.CharacterId;
            var type = msg.Request.Type;
            var friendId = msg.Request.CharacterId;
            var operate = msg.Request.Operate;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (character != null)
            {
                character.mFriend.SetBehaveData(type, friendId, operate);
            }
            else
            {
                CharacterManager.Instance.ModifyVolatileData(characterId, DataCategory.LogicCharacter, oldData =>
                {
                    if (oldData.FriendChanges == null)
                    {
                        oldData.FriendChanges = new DBFriendChange();
                    }
                    var changes = oldData.FriendChanges;
                    switch (type)
                    {
                        case 0:
                        {
                            changes.Friends.modifyValue(friendId, operate);
                        }
                            break;
                        case 1:
                        {
                            changes.Enemys.modifyValue(friendId, operate);
                        }
                            break;
                        case 2:
                        {
                            changes.Blacks.modifyValue(friendId, operate);
                        }
                            break;
                    }
                    return oldData;
                });
            }
            yield break;
        }

        public IEnumerator RechargeSuccess(Coroutine coroutine, LogicService _this, RechargeSuccessInMessage msg)
        {
            var oid = msg.Request.OrderId;
            var request = msg.Request;
            var characterId = msg.CharacterId;
            var platform = request.Platform;
            var payType = request.PayType;
            if (payType >= 1000)
            {
                payType = payType / 1000;
            }
            var price = request.Price;
            var isOffLine = true;
            var offLineCount = 0;
            var rechargeId = -1;

            var strs = msg.Request.Channel.Split('.');
            var channel = String.Empty;
            if (strs.Count() > 1)
            {
                channel = strs[1];
            }

            try
            {
                if (LogicServerControl.LastOrderSerial.Equals(oid))
                {
                    RechargeLogger.Warn("SSRechargeSuccess lastorderid = now orderid ,orderid:{0}", oid);
                    msg.Reply();
                    yield break;
                }

                RechargeLogger.Info(
                    "SSRechargeSuccess! CharacterId = {0}, platform = {1}, payType = {2}, price = {3},orderid = {4}, step 1",
                    characterId, platform, payType, price, oid);

                StaticParam.RLogger.Info(
                    "SSRechargeSuccess! CharacterId = {0}, platform = {1}, payType = {2}, price = {3}",
                    characterId, platform, payType, price);

                var rechargeData = StaticParam.RechargeData;
                Dictionary<int, Dictionary<int, int>> types;
                if (rechargeData.TryGetValue(platform, out types))
                {
                    Dictionary<int, int> prices;
                    if (payType == 3)
                    {
                        var tableId = request.PayType%1000;
                        var tbRechargeTable = Table.GetRecharge(tableId);
                        if (Math.Abs(tbRechargeTable.Price - price) < 0.01f)
                        {
                            rechargeId = tableId;
                        }
                        else
                        {
                            rechargeId = -1;
                        }
                    }
                    else
                    {
                        if (types.TryGetValue(payType, out prices))
                        {
                            if (!prices.TryGetValue((int)price, out rechargeId))
                            {
                                rechargeId = -1;
                            }
                        }   
                    }
                }
                var tbMail = Table.GetMail(130);
                var title = tbMail.Title;
                var tbRecharge = Table.GetRecharge(rechargeId);
                var formatDic = tbRecharge != null ? 270252 : 270302;
                var args = new List<string>();
                args.Add(Utils.AddDate(DateTime.Now.ToBinary(), 270251));
                if (tbRecharge != null)
                {
                    args.Add(tbRecharge.Name);
                }
                else
                {
                    args.Add(((int) Math.Round(price*10)).ToString());
                }
                var content = Utils.WrapDictionaryId(formatDic, args);

                var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
                if (cl != null)
                {
                    RechargeLogger.Info("SSRechargeSuccess character online,characterId:{0} step 2", characterId);
                    cl.OnRechargeSuccess(platform, request.PayType, price);
                    RechargeLogger.Info("SSRechargeSuccess OnRechargeSuccess success,characterId:{0} step 3",
                        characterId);
                    cl.mMail.PushMail(title, content, new Dictionary<int, int>(), tbMail.Sender);

                    PlayerLog.BackDataLogger((int) BackDataType.PayData, "{0}|{1}|{2}|{3}|{4}|{5}|{6}|{9}|{7}{8}",
                        characterId, rechargeId, price, cl.serverId, cl.lExdata64.GetTime(Exdata64TimeType.CreateTime),
                        cl.GetExData(69), cl.GetLevel(), platform, payType, oid);
                    isOffLine = false;

                    //(orderid, money, gold, orderdate, spid, serverid, userid, charname, flags) values ({0}, {1}, {2}, '{3}', '{4}', {5}, {6}, '{7}', {8})" />
                    try
                    {


                        var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
                        if (character != null && tbRecharge != null)
                        {
                            string paylog = string.Format("paylog#{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}",
                                oid, // 订单ID
                                price,
                                tbRecharge.Diamond,
                                DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                                channel,
                                cl.serverId,
                                characterId,
                                character.GetName(),
                                -1
                                );
                            kafaLogger.Info(paylog);

                            // <add key="payinfo" value="insert into smcdb.payinfo (characterid, type, money, serverid, createtime, paycount, level, orderid, channeltypeid, paytime, spid) 
                            // values ({0}, {1}, {2}, {3}, '{4}', {5}, {6}, '{7}', '{8}', '{9}', {10})" />
                            string payinfolog = string.Format("payinfo#{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}",
                                characterId,
                                0,
                                price,
                                cl.serverId,
                                DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                                character.GetExData((int)eExdataDefine.e69),
                                character.GetLevel(),
                                oid,
                                platform,
                                DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                                channel);
                            kafaLogger.Info(payinfolog);
                        }

                    }
                    catch (Exception e)
                    {
                        Logger.Error("kafka charge error{0}", e.Message);
                    }     
                }
                else
                {
                    RechargeLogger.Info("SSRechargeSuccess character offline,characterId:{0}, orderid:{1}", characterId,
                        oid);
                    StaticParam.RLogger.Info("SSRechargeSuccess, character is unline! CharacterId = {0}", characterId);
                    CharacterManager.Instance.ModifyVolatileData(characterId, DataCategory.LogicCharacter, oldData =>
                    {
                        oldData.Recharge.Add(new RechargeArgs
                        {
                            Platform = platform,
                            Type = request.PayType,
                            Price = price
                        });
                        //oldData.Recharge.Count;
                        //
                        offLineCount = oldData.Recharge.Count;
                        var now = DateTime.Now;
                        var mail = new DBMail_One();
                        mail.StartTime = now.ToBinary();
                        mail.OverTime = now.AddDays(15).ToBinary();
                        mail.Name = title;
                        mail.Text = content;
                        mail.Send = tbMail.Sender;
                        oldData.NewMails.Add(mail);

                        return oldData;
                    });
                }
            }
            catch (Exception ex)
            {
                LogicServerControl.LastOrderSerial = string.Empty;
                Logger.Fatal("SSRechargeSuccess throw exception :" + ex);
                RechargeLogger.Fatal("SSRechargeSuccess throw exception :" + ex);
                msg.Reply((int) ErrorCodes.Error_RechargeSuccess_ThrowException);
            }

            if (isOffLine)
            {
                var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
                var oldCount = 0;
                var serverId = 0;
                DateTime createTime;
                int level;
                var name = string.Empty;
                if (cl == null)
                {
                    //加载DB
                    var data = CharacterManager.Instance.DB.Get<DBCharacterLogic>(coroutine,
                        DataCategory.LogicCharacter, characterId);
                    yield return data;
                    var dbCharacter = data.Data;
                    if (dbCharacter == null)
                    {
                        Logger.Error("RechargeSuccess characterId={0}", characterId);
                        LogicServerControl.LastOrderSerial = oid;
                        msg.Reply();
                        yield break;
                    }
                    oldCount = dbCharacter.ExData[69];
                    serverId = dbCharacter.ServerId;
                    createTime = DateTime.FromBinary(dbCharacter.ExData64[(int) Exdata64TimeType.CreateTime]);
                    level = dbCharacter.Bag.Resources[(int) eResourcesType.LevelRes];
                }
                else
                {
                    createTime = cl.lExdata64.GetTime(Exdata64TimeType.CreateTime);
                    level = cl.GetLevel();
                    name = cl.GetName();
                    serverId = cl.serverId;
                }
                PlayerLog.BackDataLogger((int) BackDataType.PayData, "{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}{8}{9}",
                    characterId, rechargeId, price, serverId, createTime, oldCount + offLineCount, level, platform,
                    payType, oid);

                try
                {
                    // 记录日志
                    var rechargeDiamond = 0;
                    if (rechargeId > 0)
                    {
                        var tbRecharge = Table.GetRecharge(rechargeId);
                        if (tbRecharge != null)
                        {
                            rechargeDiamond = tbRecharge.Diamond;
                        }
                    }

                    string paylog = string.Format("paylog#{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}",
                        oid, price, rechargeDiamond, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                        channel, serverId, characterId, name, -1);
                    kafaLogger.Info(paylog);

                    string payinfolog = string.Format("payinfo#{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}",
                        characterId, 0, price, serverId, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                        oldCount + offLineCount, level, oid, platform,
                        DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), channel);
                    kafaLogger.Info(payinfolog);
                }
                catch (Exception e)
                {
                    Logger.Error("kafka charge error{0}", e.Message);
                } 
            }

            LogicServerControl.LastOrderSerial = oid;
            msg.Reply();
        }

        public IEnumerator GetItemCount(Coroutine co, LogicService _this, GetItemCountInMessage msg)
        {
            var characterId = msg.CharacterId;
            var itemId = msg.Request.ItemId;
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (cl == null)
            {
                Logger.Error("In GetItemCount(). Player unline!! id = {0}", characterId);
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }
            msg.Response = cl.mBag.GetItemCount(itemId);
            msg.Reply();
        }

        //同步战盟Buff
        public IEnumerator SSGetAllianceBuff(Coroutine coroutine, LogicService _this, SSGetAllianceBuffInMessage msg)
        {
            var characterId = msg.CharacterId;
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (cl == null)
            {
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }
            for (var i = 550; i <= 553; i++)
            {
                var exd = cl.GetExData(i);
                if (exd != 0)
                {
                    msg.Response.Items.Add(exd);
                }
            }
            msg.Reply();
        }

        public IEnumerator NotifyAllianceWarInfo(Coroutine coroutine,
                                                 LogicService _this,
                                                 NotifyAllianceWarInfoInMessage msg)
        {
            var info = msg.Request.Info;
            StaticParam.AllianceWarInfo[info.ServerId] = info;
            yield break;
        }

        public IEnumerator NotifyEnterFuben(Coroutine coroutine, LogicService _this, NotifyEnterFubenInMessage msg)
        {
            var characterId = msg.CharacterId;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (character == null)
            {
                yield break;
            }
            var fubenId = msg.Request.FubenId;
            var tbFuben = Table.GetFuben(fubenId);
            //增加副本次数
            if (tbFuben.FubenCountNode == (int) eDungeonSettlementNode.Start)
            {
                character.AddFubenCount(tbFuben);
            }
        }

        public IEnumerator ChangeServer(Coroutine co, LogicService _this, ChangeServerInMessage msg)
        {
            var characterId = msg.CharacterId;
            var serverId = msg.Request.NewServerId;
            var msg1 = LogicServer.Instance.SceneAgent.GetSceneSimpleData(characterId, 0);
            yield return msg1.SendAndWaitUntilDone(co);
            if (msg1.State != MessageState.Reply)
            {
                msg.Reply((int) ErrorCodes.Error_TimeOut);
                yield break;
            }
            if (msg1.ErrorCode != 0)
            {
                msg.Reply(msg1.ErrorCode);
                yield break;
            }
            var fightValue = msg1.Response.FightPoint;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (character == null)
            {
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }

            //通知旧rank，修改我的排名
            var tempList1 = new RankChangeDataList
            {
                CharacterId = characterId,
                Name = character.Name,
                ServerId = character.serverId
            };
            for (var i = RankType.FightValue; i < RankType.TypeCount; i++)
            {
                var temp = new RankChangeData
                {
                    RankType = (int) i
                };
                tempList1.Changes.Add(temp);
            }
            var msg2 = LogicServer.Instance.RankAgent.SSCharacterChangeDataList(characterId, tempList1);
            yield return msg2.SendAndWaitUntilDone(co);

            //通知新rank，修改我的排名
            var tempList2 = new RankChangeDataList
            {
                CharacterId = characterId,
                Name = character.Name,
                ServerId = serverId
            };
            var bag = character.mBag;
            {
                var temp = new RankChangeData
                {
                    RankType = (int) RankType.FightValue,
                    Value = fightValue
                };
                tempList2.Changes.Add(temp);
            }
            {
                var temp = new RankChangeData
                {
                    RankType = (int) RankType.Level,
                    Value = (bag.GetRes(eResourcesType.LevelRes))*4000000000L + bag.GetRes(eResourcesType.ExpRes)
                };
                tempList2.Changes.Add(temp);
            }
            {
                var temp = new RankChangeData
                {
                    RankType = (int) RankType.Money,
                    Value = bag.GetRes(eResourcesType.GoldRes)
                };
                tempList2.Changes.Add(temp);
            }
            {
                var temp = new RankChangeData
                {
                    RankType = (int) RankType.CityLevel,
                    Value = (bag.GetRes(eResourcesType.HomeLevel))*4000000000L + bag.GetRes(eResourcesType.HomeExp)
                };
                tempList2.Changes.Add(temp);
            }
	        {
				var temp = new RankChangeData
				{
					RankType = (int)RankType.RechargeTotal,
					Value = character.GetExData((int)eExdataDefine.e78_TotalRechargeDiamond)
				};
				tempList2.Changes.Add(temp);
	        }
            {
                var temp = new RankChangeData
                {
                    RankType = (int)RankType.Mount,
                    Value = character.mMount.GetFightPoint(character.GetLevel(), character.GetRole())
                };
                if (temp.Value > 0)
                {
                    tempList2.Changes.Add(temp);
                }
            }
            var msg3 = LogicServer.Instance.RankAgent.SSCharacterChangeDataList(characterId, tempList2);
            yield return msg3.SendAndWaitUntilDone(co);
            msg.Reply();
        }

        public IEnumerator SSNotifyCharacterOnConnet(Coroutine coroutine,
                                                     LogicService _this,
                                                     SSNotifyCharacterOnConnetInMessage msg)
        {
            var characterId = msg.Request.CharacterId;
            var clientId = msg.Request.ClientId;
            //var proxy = NewCharacterIn(characterId, clientId);
            var proxy = new LogicProxy(_this, characterId, clientId);

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
                yield break;
            }

            while (proxy.Connected)
            {
                yield return proxy.Wait(coroutine, TimeSpan.FromSeconds(1));
                try
                {
                    proxy.Sync();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Tick error.");
                }
            }
        }

        public IEnumerator BSNotifyCharacterOnLost(Coroutine coroutine,
                                                   LogicService _this,
                                                   BSNotifyCharacterOnLostInMessage msg)
        {
            var characterId = msg.Request.CharacterId;
            LogicCharacterProxy charProxy;
            if (!_this.Proxys.TryGetValue(characterId, out charProxy))
            {
                yield break;
            }
            var proxy = (LogicProxy) charProxy;

            LogManager.GetLogger("ConnectLost")
                .Info("character {0} - {1} Logic OnLost 1", proxy.CharacterId, proxy.ClientId);
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic--------------------OnLost--------------------{0}",
                proxy.CharacterId);
            //TODO
            //断线删除
            //Character.TestBagDbIndex();

            if (proxy.Character == null)
            {
                yield break;
            }
            RemoveAllSyncData(proxy.Character);
            foreach (var i in proxy.Character.mFriend.mDbData.BeHaveFriends)
            {
                var SceneMsg = LogicServer.Instance.SceneAgent.SendOutLineFriend(i, 0, proxy.CharacterId, i);
                yield return SceneMsg.SendAndWaitUntilDone(coroutine);
            }
            foreach (var i in proxy.Character.mFriend.mDbData.BeHaveEnemys)
            {
                var SceneMsg = LogicServer.Instance.SceneAgent.SendOutLineFriend(i, 1, proxy.CharacterId, i);
                yield return SceneMsg.SendAndWaitUntilDone(coroutine);
            }
            foreach (var i in proxy.Character.mFriend.mDbData.BeHaveShield)
            {
                var SceneMsg = LogicServer.Instance.SceneAgent.SendOutLineFriend(i, 2, proxy.CharacterId, i);
                yield return SceneMsg.SendAndWaitUntilDone(coroutine);
            }
            proxy.Character.OutLine();
            var co = CoroutineFactory.NewSubroutine(CharacterManager.Instance.RemoveCharacter, coroutine,
                proxy.CharacterId);
            if (co.MoveNext())
            {
                yield return co;
            }
            proxy.Character.Proxy = null;

            proxy.Connected = false;
        }

        public IEnumerator SSGetTodayFunbenCount(Coroutine coroutine,
                                                 LogicService _this,
                                                 SSGetTodayFunbenCountInMessage msg)
        {
            var charController = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.Request.CharacterId);
            if (charController == null)
            {
                var data = LogicServer.Instance.DB.Get<DBCharacterLogic>(coroutine, DataCategory.LogicCharacter,
                    msg.CharacterId);
                yield return data;
                if (data.Data == null)
                {
                    Logger.Error("can not load character {0} 's data from db.", msg.CharacterId);
                    msg.Reply((int) ErrorCodes.Error_CharacterId_Not_Exist);
                    yield break;
                }
                switch (msg.Request.Selecttype)
                {
                    case 0:
                    {
                        msg.Response = data.Data.ExData[21] + data.Data.ExData[22] + data.Data.ExData[23] -
                                       data.Data.ExData[36];
                        msg.Reply();
                    }
                        break;
                    case 1:
                    {
                        msg.Response = data.Data.ExData[36];
                        msg.Reply();
                    }
                        break;
                    case 2:
                    {
                        msg.Response = data.Data.ExData[24] + data.Data.ExData[25];
                        msg.Reply();
                    }
                        break;
                }
                msg.Reply();
                yield break;
            }
            charController.GetExData();
            switch (msg.Request.Selecttype)
            {
                case 0:
                {
                    msg.Response = charController.GetExData((int) eExdataDefine.e21) +
                                   charController.GetExData((int) eExdataDefine.e22) +
                                   charController.GetExData((int) eExdataDefine.e23) -
                                   charController.GetExData((int) eExdataDefine.e36);
                    msg.Reply();
                }
                    break;
                case 1:
                {
                    msg.Response = charController.GetExData((int) eExdataDefine.e36);
                    msg.Reply();
                }
                    break;
                case 2:
                {
                    msg.Response = charController.GetExData((int) eExdataDefine.e24) +
                                   charController.GetExData((int) eExdataDefine.e25);
                    msg.Reply();
                }
                    break;
            }
        }

        public IEnumerator LogicGetAnyData(Coroutine coroutine, LogicService _this, LogicGetAnyDataInMessage msg)
        {
            throw new NotImplementedException();
        }

		public IEnumerator SSSyncCharacterFightPoint(Coroutine coroutine, LogicService _this, SSSyncCharacterFightPointInMessage msg)
	    {
			var charController = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
			if (null != charController)
			{
				charController.mOperActivity.OnRankDataChange(RankType.FightValue, (long) msg.Request.Fp);
			}
			msg.Reply();
			yield break;
	    }

	    public IEnumerator OnPlayerEnterSceneOver(Coroutine coroutine, LogicService _this, OnPlayerEnterSceneOverInMessage msg)
	    {
		    var charController = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
		    if (charController != null)
		    {
			    var tb = Table.GetScene(msg.Request.SceneId);
			    if (null != tb)
			    {
				    if (-1 != tb.FubenId && null!=Table.GetFuben(tb.FubenId))
				    {
					    charController.mOperActivity.OnEnterFuben(tb.FubenId);
				    }
			    }
                charController.SetExData((int)eExdataDefine.e666, 0);
		    }
            Dict_int_int_Data data = new Dict_int_int_Data();
	        {
	            do
	            {
	                var str = Table.GetServerConfig(3005).Value.Trim();
	                if (string.IsNullOrEmpty(str))
	                {
	                    break;
	                }
	                var Ids = str.Split('|');
	                foreach (var id in Ids)
	                {
	                    int val = charController.GetExData(int.Parse(id));
                        msg.Response.exData.Add(int.Parse(id),val);
	                }
	            } while (false);

                do
                {
                    var str = Table.GetServerConfig(3006).Value.Trim();
                    if (string.IsNullOrEmpty(str))
                    {
                        break;
                    }
                    var Ids = str.Split('|');
                    foreach (var id in Ids)
                    {
                        if (charController.GetFlag(int.Parse(id)))
                        {
                            msg.Response.flagData.Add(int.Parse(id),1);
                        }
                    }
                } while (false);
	        }
			msg.Reply();
			yield break;
	    }

        public IEnumerator AnchorGift(Coroutine coroutine, LogicService _this, AnchorGiftInMessage msg)
        {
            var itemId = msg.Request.ItemId;
            var count = msg.Request.Count;
            var characterId = msg.CharacterId;
            var charController = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (null != charController && msg.Request != null)
            {
                // 检查物品
                var giftRate = int.Parse(Table.GetServerConfig(404).Value);
                var needDiamond = giftRate * count;
                if (needDiamond > charController.mBag.GetRes(eResourcesType.DiamondRes))
                {
                    msg.Reply((int)ErrorCodes.DiamondNotEnough);
                    yield break;
                }

                // 删除物品
                ErrorCodes error = charController.mBag.DelRes(eResourcesType.DiamondRes, needDiamond, eDeleteItemType.PresentGift);
                if (error != ErrorCodes.OK)
                {
                    msg.Reply((int)ErrorCodes.DiamondNotEnough);
                    yield break;
                }

                //个人累计送花数
                charController.AddExData((int)eExdataDefine.e598, count);

                // 排行
                var tempList1 = new RankChangeDataList
                {
                    CharacterId = characterId,
                    Name = charController.Name,
                    ServerId = charController.serverId
                };

                Action<RankType, eExdataDefine> changeFunc = (type, exdata) =>
                {
                    charController.AddExData((int)exdata, count);
                    charController.SetRankFlag(type);
                    var temp = new RankChangeData
                    {
                        RankType = (int)type,
                        Value = charController.GetExData((int)exdata)
                    };
                    tempList1.Changes.Add(temp);
                };

                changeFunc(RankType.DailyGift, eExdataDefine.e595);
                changeFunc(RankType.WeeklyGift, eExdataDefine.e596);
                changeFunc(RankType.TotalGift, eExdataDefine.e597);

                if (tempList1.Changes.Count > 0)
                {
                    var msg3 = LogicServer.Instance.RankAgent.SSCharacterChangeDataList(characterId, tempList1);
                    yield return msg3.SendAndWaitUntilDone(coroutine);
                }
            }
            else
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;  
            }
            msg.Reply();
            yield break;
        }

        public IEnumerator SSLearnSkill(Coroutine coroutine, LogicService _this, SSLearnSkillInMessage msg)
        {
            var charController = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (null != charController && msg.Request != null)
            {
                charController.mSkill.LearnSkill(msg.Request.SkillId, msg.Request.SkillLevel);
            }
            msg.Reply();
            yield break;
        }

        public IEnumerator GetCharacterData(Coroutine co, LogicService _this, GetCharacterDataInMessage msg)
        {
            var charController = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.Request.Id);
            if (charController != null)
            {
                var charDetailInfo = new GMCharacterDetailInfo();
                charDetailInfo.Id = charController.mGuid;
                charDetailInfo.RoleId = charController.GetRole();
                charDetailInfo.Level = charController.GetLevel();
                charDetailInfo.VipLevel = charController.mBag.GetRes(eResourcesType.VipLevel);
                charDetailInfo.VipPoint = charController.mBag.GetRes(eResourcesType.VipExpRes);
                charDetailInfo.Yuanbao = charController.mBag.GetRes(eResourcesType.DiamondRes);
                charDetailInfo.Money = charController.mBag.GetRes(eResourcesType.GoldRes);
                charDetailInfo.IsOnline = 1;
                charDetailInfo.LastLogin = charController.lExdata64.GetExData((int) Exdata64TimeType.LastOnlineTime);
                charDetailInfo.CreateTime = charController.lExdata64.GetExData((int) Exdata64TimeType.CreateTime);
                charDetailInfo.Experience = charController.mBag.GetRes(eResourcesType.ExpRes);
                msg.Response = charDetailInfo;
                msg.Reply();
            }
            else
            {
                var data = LogicServer.Instance.DB.Get<DBCharacterLogic>(co, DataCategory.LogicCharacter,
                    msg.CharacterId);
                yield return data;

                // can not get data from db
                if (data.Data == null)
                {
                    Logger.Error("can not load character {0} 's data from db.", msg.CharacterId);
                    msg.Reply((int) ErrorCodes.Error_CharacterId_Not_Exist);
                    yield break;
                }

                var charDetailInfo = new GMCharacterDetailInfo();
                try
                {
                    charDetailInfo.Id = data.Data.Id;
                    charDetailInfo.RoleId = data.Data.TypeId;
                    charDetailInfo.Level = data.Data.Bag.Resources[(int)eResourcesType.LevelRes];
                    charDetailInfo.VipLevel = data.Data.Bag.Resources.GetIndexValue((int)eResourcesType.VipLevel);
                    charDetailInfo.VipPoint = data.Data.Bag.Resources.GetIndexValue((int)eResourcesType.VipExpRes);
                    charDetailInfo.Yuanbao = data.Data.Bag.Resources.GetIndexValue((int)eResourcesType.DiamondRes);
                    charDetailInfo.Money = data.Data.Bag.Resources.GetIndexValue((int)eResourcesType.GoldRes);
                    charDetailInfo.IsOnline = 0;
                    charDetailInfo.LastLogin = data.Data.ExData64.GetIndexValue((int)Exdata64TimeType.LastOnlineTime);
                    charDetailInfo.CreateTime = data.Data.ExData64.GetIndexValue((int)Exdata64TimeType.CreateTime);
                    charDetailInfo.Experience = data.Data.Bag.Resources.GetIndexValue((int)eResourcesType.ExpRes);
                    msg.Response = charDetailInfo;
                }
                catch (Exception e)
                {
                    Logger.Error("can not load character {0} 's data from db. set value err {1}", msg.CharacterId, e);
                }

                msg.Reply();
            }
        }

        public IEnumerator SendMailToCharacter(Coroutine coroutine, LogicService _this, SendMailToCharacterInMessage msg)
        {
            GameMaster.PushMailToSomeone(msg.CharacterId, msg.Request.Title, msg.Request.Content, msg.Request.Items.Data, msg.Request.State);
            yield break;
        }

        public IEnumerator SendMailToServer(Coroutine coroutine, LogicService _this, SendMailToServerInMessage msg)
        {
            var mailId = msg.Request.MailId;
            var serverId = (int) msg.Request.ServerId;
            //加载邮件
            var result = CoroutineFactory.NewSubroutine(MailManager.ReadMail, coroutine, serverId, mailId);
            if (result.MoveNext())
            {
                yield return result;
            }
            MailManager.SetNowGmMailGuid(serverId, mailId);
            //循环发送
            CharacterManager.Instance.ForeachCharacter(character =>
            {
                var temp = character as CharacterController;
                if (temp == null)
                {
                    return true;
                }
                if (temp.serverId != serverId)
                {
                    return true;
                }
                //temp.mMail.PushMail();
                MailManager.PushMail(temp, mailId);
                temp.mMail.GmGuid = mailId;
                return true;
            });
        }

        public IEnumerator SendMailToCharacterById(Coroutine coroutine, LogicService _this, SendMailToCharacterByIdInMessage msg)
        {
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (character != null)
            {
                if ((SendToCharacterMailType) msg.Request.Type == SendToCharacterMailType.Normal)
                    character.mBag.AddMailItems(msg.Request.MailId, (eCreateItemType) msg.Request.CreateType);
                else
                {
                    character.mBag.AddRechargeRetMailItems(msg.Request.MailId, msg.Request.CountList.Items, (eCreateItemType)msg.Request.CreateType);
                }
            }
            yield break;
        }
        public  IEnumerator ApplyMayaSkill(Coroutine coroutine, LogicService _this, ApplyMayaSkillInMessage msg)
        {
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (character != null)
            {
                var id = character.GetExData((int)eExdataDefine.e648);
                if (id > 0)
                {
                    var e = new TollgateNextFinish(character, id);
                    EventDispatcher.Instance.DispatchEvent(e);
                }
                character.SetExData((int)eExdataDefine.e648, 0);
            }
            yield break;
        }
        public IEnumerator SSBattleResult(Coroutine coroutine, LogicService _this, SSBattleResultInMessage msg)
        {
            var fubenId = msg.Request.FubenId;
            var type = msg.Request.Type;
            var uid = msg.CharacterId;
            PlayerLog.WriteLog(uid, "----------Logic----------SSBattleResult----------");

            var charController = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (charController == null)
            {
                msg.Reply((int) ErrorCodes.Unknow);
                yield break;
            }
            charController.BattleResult(fubenId, type);
            msg.Reply();
        }

        //获得某人的商店道具
        public IEnumerator GetExchangeData(Coroutine coroutine, LogicService _this, GetExchangeDataInMessage msg)
        {
            var targetId = msg.Request.CharacterId;
            var charController = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (charController != null)
            {
                //var simple = charController.GetSimpleData();
                msg.Response.SellCharacterId = targetId;
                msg.Response.SellCharacterName = charController.GetName();
                foreach (var item in charController.mExchange.mDbData.StoreItems)
                {
                    msg.Response.Items.Add(new OtherStoreOne
                    {
                        Id = item.Id,
                        ItemData = item.ItemData,
                        NeedCount = item.NeedCount,
                        State = item.State,
                        ManagerId = item.ManagerId,
                        NeedType = item.NeedType
                    });
                }
                msg.Reply();
                yield break;
            }
            CharacterManager.Instance.GetSimpeData(targetId, simple =>
            {
                if (simple != null)
                {
                    msg.Response.SellCharacterId = simple.Id;
                    msg.Response.SellCharacterName = simple.Name;
                    CharacterManager.Instance.ModifyVolatileData(targetId, DataCategory.LogicCharacter, oldData =>
                    {
                        if (simple.Exchange != null)
                        {
                            foreach (var item in simple.Exchange.StoreItems)
                            {
                                var storeId = item.Id;
                                var targetItem = new OtherStoreOne
                                {
                                    Id = item.Id,
                                    ItemData = item.ItemData,
                                    NeedCount = item.NeedCount,
                                    ManagerId = item.ManagerId,
                                    NeedType = item.NeedType
                                };
                                if (oldData.ExchangeBuyed.ContainsKey(storeId))
                                {
                                    targetItem.State = (int)StoreItemType.Buyed;
                                }
                                else
                                {
                                    targetItem.State = item.State;
                                }
                                msg.Response.Items.Add(targetItem);
                            }
                        }
                        msg.Reply();
                        return oldData;
                    });
                }
                else
                {
                    msg.Reply();
                }
            });
        }

        //交易系统：有A要买B的东西
        public IEnumerator SSStoreOperationBuy(Coroutine coroutine, LogicService _this, SSStoreOperationBuyInMessage msg)
        {
            var targetId = msg.CharacterId;
            var storeId = msg.Request.StoreId;
            var charController = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (charController == null)
            {
                CharacterManager.Instance.ModifyVolatileData(targetId, DataCategory.LogicCharacter, oldData =>
                {
                    if (oldData.ExchangeBuyed.ContainsKey(storeId))
                    {
                        msg.Reply((int) ErrorCodes.Error_ExchangeItemState);
                        return oldData;
                    }
                    oldData.ExchangeBuyed.Add(storeId, new Uint64StringPair
                    {
                        Key = msg.Request.Aid,
                        Value = msg.Request.Name
                    });
                    if (oldData.SellHistory == null)
                    {
                        oldData.SellHistory = new SellHistoryList();
                    }
                    if (oldData.SellHistory.items.Count >= 50)
                    {
                        oldData.SellHistory.items.RemoveAt(0);
                    }
                    oldData.SellHistory.items.Add(new SellHistoryOne
                    {
                        sellTime = DateTime.Now.ToBinary(),
                        ItemData = msg.Request.Itemdata,
                        buyCharacterId = msg.Request.Aid,
                        buyCharacterName = msg.Request.Name,
                        resType = msg.Request.ResType,
                        resCount = msg.Request.ResCount
                    });
                    msg.Reply();
                    return oldData;
                });
                yield break;
            }
            var item = charController.mExchange.GetItemByStoreId(storeId);
            if (item == null)
            {
                msg.Reply((int) ErrorCodes.Error_ItemNotFind);
                yield break;
            }
            if (item.State != StoreItemType.Normal)
            {
                msg.Reply((int) ErrorCodes.Error_ExchangeItemState);
                yield break;
            }
            item.State = StoreItemType.Buyed;
            item.mDbdata.BuyCharacterId = msg.Request.Aid;
            item.mDbdata.BuyCharacterName = msg.Request.Name;
            if (charController.Proxy != null)
            {
                charController.Proxy.NotifyStoreBuyed(storeId, msg.Request.Aid, msg.Request.Name);
            }
            msg.Reply();
        }

        public IEnumerator SendMailToCharacterByItems(Coroutine coroutine,
                                                      LogicService _this,
                                                      SendMailToCharacterByItemsInMessage msg)
        {
            var characterId = msg.CharacterId;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            var tbMail = Table.GetMail(msg.Request.MailId);
            if (tbMail == null)
            {
                yield break;
            }
            var items = new List<ItemBaseData>();
            var item = msg.Request.Item;
            if (item != null && item.ItemId != -1)
            {
                items.Add(item);
            }
            for (var i = 0; i != 5; ++i)
            {
                if (tbMail.ItemId[i] < 0)
                {
                    continue;
                }
                if (tbMail.ItemCount[i] < 1)
                {
                    continue;
                }
                var itemDb = new ItemBaseData();
                ShareItemFactory.Create(tbMail.ItemId[i], itemDb);
                itemDb.Count = tbMail.ItemCount[i];
                items.Add(itemDb);
            }
            var mailContent = string.Format(tbMail.Text, msg.Request.Args.Items.ToArray());
            if (character != null)
            {
                character.mMail.PushMail(tbMail.Title, mailContent, items,0,tbMail.Sender);
//                character.mMail.PushMail(tbMail.Title, mailContent, new Dictionary<int, int>(),tbMail.Sender,items);
            }
            else
            {
                CharacterManager.Instance.ModifyVolatileData(characterId, DataCategory.LogicCharacter, oldData =>
                {
                    var mail = new DBMail_One();
                    mail.StartTime = DateTime.Now.ToBinary();
                    mail.OverTime = DateTime.Now.AddDays(15).ToBinary();
                    mail.State = 0;
                    mail.Name = tbMail.Title;
                    mail.Text = mailContent;
                    if (items.Count > 0)
                    {
                        mail.Items.AddRange(items);
                    }
                    oldData.NewMails.Add(mail);
                    return oldData;
                });
            }
        }

        //额外增加好友，仇人等
        public IEnumerator SSAddFriendById(Coroutine coroutine, LogicService _this, SSAddFriendByIdInMessage msg)
        {
            var uid = msg.Request.CharacterId;
            PlayerLog.WriteLog(msg.CharacterId, "----------Logic----------SSAddFriendById----------");

            var charController = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (charController == null)
            {
                yield break;
            }

            var errorCodes = ErrorCodes.OK; // Character.mFriend.CheckAddFriend(msg.Request.CharacterId);
            switch (msg.Request.Type)
            {
                case 1: //好友
                    //是否已经是自己的好友
                    errorCodes = charController.mFriend.CheckAddFriend(uid);
                    if (errorCodes != ErrorCodes.OK)
                    {
                        //Character.mFriend.DelFriend(msg.Request.CharacterId);
                        yield break;
                    }
                    break;
                case 2: //仇人                    
                    //是否已经是仇人
                    var ff = charController.mFriend.GetEnemy(uid);
                    if (ff != null && ff.Guid == uid)
                    {
                        ff.mDbData.Time = DateTime.Now.ToBinary();
                        yield break;
                    }
                    errorCodes = charController.mFriend.CheckAddEnemy(uid);
                    if (errorCodes == ErrorCodes.Error_EnemyIsMore)
                    {
                        var first = charController.mFriend.GetFirstAutoEnemy();
                        if (first == null)
                        {
                            //说明都是手动加的了
                            yield break;
                        }

                        charController.mFriend.DelEnemy(first.Guid);
                        charController.mFriend.AddEnemy(uid, 0);
                        if (charController.Proxy != null)
                        {
                            charController.Proxy.SyncFriendDelete(1, first.Guid);
                            var dbSceneSimple = LogicServer.Instance.SceneAgent.GetSceneSimpleData(uid, 0);
                            yield return dbSceneSimple.SendAndWaitUntilDone(coroutine);
                            if (dbSceneSimple.State != MessageState.Reply)
                            {
                                yield break;
                            }
                            if (dbSceneSimple.ErrorCode != (int) ErrorCodes.OK)
                            {
                                yield break;
                            }
                            var temp = new CharacterSimpleData
                            {
                                Id = dbSceneSimple.Response.Id,
                                TypeId = dbSceneSimple.Response.TypeId,
                                Name = dbSceneSimple.Response.Name,
                                SceneId = dbSceneSimple.Response.SceneId,
                                FightPoint = dbSceneSimple.Response.FightPoint,
                                Level = dbSceneSimple.Response.Level,
                                Ladder = dbSceneSimple.Response.Ladder,
                                ServerId = dbSceneSimple.Response.ServerId,
                                Vip = dbSceneSimple.Response.Vip

                                
                            };
                            charController.Proxy.SyncAddFriend(2, temp);
                        }
                        yield break;
                    }
                    if (errorCodes != ErrorCodes.OK)
                    {
                        yield break;
                    }
                    break;
                case 3: //屏蔽
                    //是否已经屏蔽
                    errorCodes = charController.mFriend.CheckAddShield(uid);
                    if (errorCodes != ErrorCodes.OK)
                    {
                        yield break;
                    }
                    break;
            }
          
            if (charController.Proxy != null)
            {
                var dbSceneSimple1 = LogicServer.Instance.SceneAgent.GetSceneSimpleData(uid, 0);
                yield return dbSceneSimple1.SendAndWaitUntilDone(coroutine);
                if (dbSceneSimple1.State != MessageState.Reply)
                {
                    yield break;
                }
                if (dbSceneSimple1.ErrorCode != (int) ErrorCodes.OK)
                {
                    yield break;
                }
                switch (msg.Request.Type)
                {
                    case 1: //好友
                        charController.mFriend.AddFriend(uid);
                        break;
                    case 2: //仇人

                        var scene = Table.GetScene(dbSceneSimple1.Response.SceneId);
                        if (scene == null) yield break;
                        var vs = Table.GetPVPRule(scene.PvPRule);
                        if (vs == null) yield break;
                        if (vs.IsAutoAddEnemy == 0) yield break;

                        charController.mFriend.AddEnemy(uid, 0);
                        break;
                    case 3: //屏蔽
                        charController.mFriend.AddShield(uid);
                        break;
                }
                var temp = new CharacterSimpleData
                {
                    Id = dbSceneSimple1.Response.Id,
                    TypeId = dbSceneSimple1.Response.TypeId,
                    Name = dbSceneSimple1.Response.Name,
                    SceneId = dbSceneSimple1.Response.SceneId,
                    FightPoint = dbSceneSimple1.Response.FightPoint,
                    Level = dbSceneSimple1.Response.Level,
                    Ladder = dbSceneSimple1.Response.Ladder,
                    ServerId = dbSceneSimple1.Response.ServerId,
                    Online = dbSceneSimple1.Response.Online,
                    Vip = dbSceneSimple1.Response.Vip
                };
               
                charController.Proxy.SyncAddFriend(2, temp);
            }
        }

        //战盟信息发生变化的通知
        public IEnumerator AllianceDataChange(Coroutine coroutine, LogicService _this, AllianceDataChangeInMessage msg)
        {
            var charController = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (charController == null)
            {
                yield break;
            }
            var serverId = charController.serverId;
            var occupantId = StaticParam.AllianceWarInfo[serverId].OccupantId;
            var oldAllianceId = charController.mAlliance.AllianceId;
            switch (msg.Request.Type)
            {
                case 0: //申请被同意后，通知申请者
                {
                    charController.mAlliance.AllianceId = msg.Request.AllianceId;
                    charController.mAlliance.State = AllianceState.Have;
                    charController.mAlliance.Ladder = msg.Request.Ladder;
                    charController.mAlliance.CleanApplyList();
                    charController.ModityTitle(2003, true);
                    if (msg.Request.AllianceId == occupantId)
                    {
//如果是王城占领者公会，需要改称号
                        charController.ModityTitle(5001, true);
                    }
                    charController.SetFlag(2801);
                    var result = LogicServer.Instance.SceneAgent.SSAllianceDataChange(msg.CharacterId,
                        msg.Request.AllianceId, 0, msg.Request.Name);
                    yield return result.SendAndWaitUntilDone(coroutine);
                }
                    break;
                case 1: //被踢出
                {
                    charController.mAlliance.AllianceId = 0;
                    charController.mAlliance.State = AllianceState.None;
                    var titles = new List<int>();
                    var states = new List<bool>();
                    for (var i = 2000; i <= 2003; i++)
                    {
                        titles.Add(i);
                        states.Add(false);
                    }
                    if (oldAllianceId == occupantId)
                    {
//如果是王城占领者公会，需要改称号
                        titles.Add(5001);
                        states.Add(false);
                    }
                    charController.ModityTitles(titles, states);
                    var result = LogicServer.Instance.SceneAgent.SSAllianceDataChange(msg.CharacterId,
                        msg.Request.AllianceId, 1, "");
                    yield return result.SendAndWaitUntilDone(coroutine);
                }
                    break;
                case 2:
                {
                    //被拒绝
                    if (charController.GetExData(286) == msg.Request.AllianceId)
                    {
                        charController.SetExData(286, 0);
                    }
                    if (charController.GetExData(287) == msg.Request.AllianceId)
                    {
                        charController.SetExData(287, 0);
                    }
                    if (charController.GetExData(288) == msg.Request.AllianceId)
                    {
                        charController.SetExData(288, 0);
                    }
                }
                    break;
                case 3:
                {
//战盟权限变化
                    var oldLadder = charController.mAlliance.Ladder;
                    var ladder = msg.Request.Ladder;
                    charController.mAlliance.Ladder = ladder;

                    var titleId = 2003 - ladder;
                    var titles = new List<int>();
                    var states = new List<bool>();
                    for (var i = 2000; i <= 2003; i++)
                    {
                        titles.Add(i);
                        states.Add(i == titleId);
                    }
                    if (oldAllianceId == occupantId && oldLadder != ladder)
                    {
                        var v = ladder == (int) eAllianceLadder.Chairman;
                        titles.Add(5000);
                        states.Add(v);
                        titles.Add(5001);
                        states.Add(!v);
                    }
                    charController.ModityTitles(titles, states);
                }
                    break;
            }
        }

        //获取某人的Exdata
        public IEnumerator SSFetchExdata(Coroutine coroutine, LogicService _this, SSFetchExdataInMessage msg)
        {
            var idList = msg.Request.IdList.Items;
            var retList = msg.Response.Items;

            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (cl != null)
            {
//如果玩家在线
                if (idList.Count > 0)
                {
                    retList.AddRange(idList.Select(i => cl.GetExData(i)));
                }
                else
                {
                    retList.AddRange(cl.GetExData());
                }
                msg.Reply();
                yield break;
            }

            //如果玩家不在线，暂不处理
            msg.Reply((int) ErrorCodes.Unline);
        }
        public IEnumerator ApplyPlayerFlag(Coroutine coroutine, LogicService _this, ApplyPlayerFlagInMessage msg)
        {
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (cl != null)
            {
                foreach (var v in msg.Request.FlagList.Items)
                {
                    msg.Response.Data.Add(v,cl.GetFlag(v)?1:0);
                }
            }
            msg.Reply();
            yield break;
        }

        
        //请求家园建筑随从数据
        public IEnumerator SSRequestCityBuidlingPetData(Coroutine coroutine,
                                                        LogicService _this,
                                                        SSRequestCityBuidlingPetDataInMessage msg)
        {
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.Request.Guid);
            if (cl == null)
            {
//如果玩家不在线，暂不处理
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }

            //msg.Response.Pets = new CityBuildingPet();

            foreach (var pair in cl.mCity.Buildings)
            {
                var building = pair.Value;
                if (building.PetList.Count <= 0)
                {
                    continue;
                }

                var petId = building.PetList[0];

                if (-1 == petId)
                {
                    continue;
                }

                var pet = cl.GetPet(petId);
                if (null == pet)
                {
                    continue;
                }

                var petMsg = new CityBuildingPet();
                petMsg.AreaId = building.AreaId;
                petMsg.PetId = petId;
                petMsg.Level = pet.GetExdata(PetItemExtDataIdx.Level);

                msg.Response.Pets.Add(petMsg);
            }

            msg.Reply();
        }

        //副本结束了，通知logic（目前用于血色，恶魔的奖励领取）
        public IEnumerator NotifyDungeonClose(Coroutine co, LogicService _this, NotifyDungeonCloseInMessage msg)
        {
            var db = LogicServer.Instance.DB;
            var fubenId = msg.Request.FubenId;
            var ids = msg.Request.PlayerIds.Items;
            foreach (var id in ids)
            {
                var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(id);
                if (character == null)
                {
                    var tbFuben = Table.GetFuben(fubenId);
                    if (tbFuben == null)
                    {
                        Logger.Error("In NotifyDungeonClose(),tbFuben == null! fuben id = {0}", fubenId);
                        yield break;
                    }
                    if (tbFuben.AssistType < 4 || tbFuben.AssistType > 5)
                    {
                        Logger.Error("Wrong fuben id [{0}] for SelectDungeonReward()", fubenId);
                        yield break;
                    }
                    var result = db.Get<DBCharacterLogic>(co, DataCategory.LogicCharacter, id);
                    yield return result;
                    if (result.Status != DataStatus.Ok)
                    {
                        Logger.Error("GetAndClean DataCategory.LogicCharacter Error, status = {0}", result.Status);
                        yield break;
                    }
                    var charDb = result.Data;
                    var index = tbFuben.AssistType - 4;
                    var exDataIdx = (int) eExdataDefine.e408 + index;
                    var data = charDb.ExData[exDataIdx];
                    if (data == 0)
                    {
                        yield break;
                    }

                    var tbMail = Table.GetMail(53);
                    if (tbMail == null)
                    {
                        Logger.Error("In NotifyDungeonClose(),tbMail == null! mail id = {0}", tbMail.Id);
                        yield break;
                    }

                    var simpleDataMsg = LogicServer.Instance.LogicAgent.GetLogicSimpleData(id, 0);
                    yield return simpleDataMsg.SendAndWaitUntilDone(co);

                    if (simpleDataMsg.State != MessageState.Reply)
                    {
                        Logger.Error(
                            "In NotifyDungeonClose(), GetLogicSimpleData() return with state = {0}, character id = {1}",
                            simpleDataMsg.State, id);
                        yield break;
                    }
                    if (simpleDataMsg.ErrorCode != (int) ErrorCodes.OK)
                    {
                        Logger.Error(
                            "In NotifyDungeonClose(), GetLogicSimpleData() return with error = {0}, character id = {1}",
                            simpleDataMsg.ErrorCode, id);
                        yield break;
                    }

                    var level = simpleDataMsg.Response.Level;

                    #region 组织副本奖励的数据

                    var exp = 0;
                    if (tbFuben.IsDynamicExp == 1)
                    {
                        exp = (int) (1.0*tbFuben.DynamicExpRatio*Table.GetLevelData(level).DynamicExp/10000);
                    }

                    //发奖励
                    var itemIds = tbFuben.RewardId;
                    var itemCounts = tbFuben.RewardCount;
                    var items = new Dictionary<int, int>();
                    for (int i = 0, imax = itemIds.Length; i < imax; i++)
                    {
                        var itemId = itemIds[i];
                        if (itemId == -1)
                        {
                            break;
                        }
                        var itemCount = itemCounts[i];
                        items.Add(itemId, itemCount);
                    }
                    if (exp > 0)
                    {
                        items.modifyValue((int) eResourcesType.ExpRes, exp);
                    }

                    #endregion

                    #region 组织邮件数据

                    var tempMail = new DBMail_One();
                    tempMail.StartTime = DateTime.Now.ToBinary();
                    tempMail.State = 0;
                    tempMail.Name = tbMail.Title;
                    tempMail.Text = tbMail.Text;
                    tempMail.OverTime = DateTime.Now.AddDays(15).ToBinary();

                    foreach (var i in items)
                    {
                        var itemDb = new ItemBaseData();
                        ShareItemFactory.Create(i.Key, itemDb);
                        itemDb.Count = i.Value;
                        tempMail.Items.Add(itemDb);
                    }

                    #endregion

                    CharacterManager.Instance.ModifyVolatileData(id, DataCategory.LogicCharacter, oldData =>
                    {
                        oldData.NewMails.Add(tempMail);
                        oldData.ExdataSetValue[exDataIdx] = 0;
                        int expDefine = -1, diamondDefine = -1;
                        CharacterControllerDefaultImpl.GetMultyExpExData(tbFuben.AssistType, ref expDefine, ref diamondDefine);
                        if (expDefine > 0 && diamondDefine > 0)
                        {
                            //const int maxExpTimes = ActivityDungeonConstants.MaxExpTimes;
                            //oldData.ExdataSetValue[expDefine] = oldData.ExdataSetValue[expDefine] + exp * (maxExpTimes - 1);
                            //oldData.ExdataSetValue[diamondDefine] = oldData.ExdataSetValue[diamondDefine] + StaticParam.HaploidDia * (maxExpTimes - 1);
                        }
                        return oldData;
                    });
                }
                else
                {
                    character.SelectDungeonReward(fubenId, 0, true);
                }
            }
        }

        public IEnumerator SSSetFlag(Coroutine coroutine, LogicService _this, SSSetFlagInMessage msg)
        {
            var characterId = msg.CharacterId;
            var changes = msg.Request.Changes.Data;
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (cl != null)
            {
                foreach (var change in changes)
                {
                    cl.SetFlag(change.Key, change.Value == 1);
                }
            }
            else
            {
                CharacterManager.Instance.ModifyVolatileData(characterId, DataCategory.LogicCharacter, oldData =>
                {
                    foreach (var change in changes)
                    {
                        oldData.FlagSetValue[change.Key] = change.Value;
                    }
                    return oldData;
                });
            }
            msg.Reply();
            yield break;
        }

        public IEnumerator SSSetExdata(Coroutine coroutine, LogicService _this, SSSetExdataInMessage msg)
        {
            var characterId = msg.CharacterId;
            var changes = msg.Request.Changes.Data;
            var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (cl != null)
            {
                foreach (var change in changes)
                {
                    cl.SetExData(change.Key, change.Value);
                }
            }
            else
            {
                CharacterManager.Instance.ModifyVolatileData(characterId, DataCategory.LogicCharacter, oldData =>
                {
                    foreach (var change in changes)
                    {
                        oldData.ExdataSetValue[change.Key] = change.Value;
                    }
                    return oldData;
                });
            }
            msg.Reply();
            yield break;
        }

        public IEnumerator GMCommand(Coroutine coroutine, LogicService _this, GMCommandInMessage msg)
        {
            var request = msg.Request;
            var characterId = msg.CharacterId;
            var commands = request.Commonds.Items;

	        if (ulong.MaxValue == characterId)
	        {
		        if (commands.Contains("!!ReloadOperationActivity"))
		        {
			        OperationActivityManager.Instance.Reload();
					msg.Reply((int)ErrorCodes.OK);
					yield break;
		        }
	        }

            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (character == null)
            {
                Logger.Error("In GMCommand character == null, id = {0}", characterId);
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }
            var errs = msg.Response.Items;
            foreach (var command in commands)
            {
                errs.Add((int) character.GmCommand(command));
            }
            msg.Reply();
        }

        void InitGiftCodeItem()
        {
            var server = LogicServer.Instance.ServerControl;
            server.GiftCodeItems.Clear();
            Table.ForeachGiftCode(item =>
            {
                var items = new List<Dictionary<int, int>>();
                server.GiftCodeItems.Add(item.Id, items);
                items.Add(GetGifts(item.Drop1Id));
                items.Add(GetGifts(item.Drop2Id));
                items.Add(GetGifts(item.Drop3Id));

                return true;
            });
        }

        void InitFirstChargeItem()
        {
            var firstChargeItems = LogicServer.Instance.ServerControl.FirstChargeItemDict;
            firstChargeItems.Clear();

            var firstChargeFlags = LogicServer.Instance.ServerControl.FirstChargeFlagDict;
            firstChargeFlags.Clear();

            var firstChargeModels = LogicServer.Instance.ServerControl.FirstChargeModelDict;
            firstChargeModels.Clear();
            
//             configIdDict[0] = 590; // 剑士
//             configIdDict[1] = 591; // 法师
//             configIdDict[2] = 592; // 弓箭
  
            Table.ForeachFirstRecharge(table =>
            {
                var itemDic = new Dictionary<int, List<FirstChargeItem>>();
                var itemList1 = new List<FirstChargeItem>();
                var itemList2= new List<FirstChargeItem>();
                var itemList3 = new List<FirstChargeItem>();

                var itemStrArrayJob1 = table.job1Items.Split('|');
                ParseFirstChargeTableItem(itemStrArrayJob1, itemList1);
                itemDic.Add(0, itemList1);

                var itemStrArrayJob2 = table.job2Items.Split('|');
                ParseFirstChargeTableItem(itemStrArrayJob2, itemList2);
                itemDic.Add(1, itemList2);

                var itemStrArrayJob3 = table.job3Items.Split('|');
                ParseFirstChargeTableItem(itemStrArrayJob3, itemList3);
                itemDic.Add(2, itemList3);

                // items
                firstChargeItems.Add(table.diamond, itemDic);

                // flags
                firstChargeFlags.Add(table.diamond, table.flag);

                // models
                List<string> modelStr = new List<string>();
                modelStr.Add(table.job1Path);
                modelStr.Add(table.job2Path);
                modelStr.Add(table.job3Path);
                firstChargeModels.Add(table.diamond, modelStr);

                return true;
            });
        }

        private static void ParseFirstChargeTableItem(string[] itemStrArrayJob1, List<FirstChargeItem> itemList1)
        {
            foreach (var itemStr in itemStrArrayJob1)
            {
                var itemArray = itemStr.Split('*');
                var item = new FirstChargeItem();
                if (itemArray.Length == 0)
                {
                    continue;
                }
                item.itemid = int.Parse(itemArray[0]);
                item.count = 1;
                if (itemArray.Count() > 1)
                {
                    item.count = int.Parse(itemArray[1]);
                }
                itemList1.Add(item);
            }
        }

        public IEnumerator GMDeleteMessage(Coroutine coroutine, LogicService _this, GMDeleteMessageInMessage msg)
        {
            var charController = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (charController != null)
            {
                charController.mAlliance.CheckOver(2, -1, -1);
            }
            Logger.Error("GMDeleteMessage characterid=" + msg.CharacterId);
            yield break;
        }
    }

    public class LogicServerControl : LogicService
    {
        //最后充值订单号，防止因为网络超时造成重复充值
        public static string LastOrderSerial = string.Empty;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); //
        public static TimeManager Timer = new TimeManager();
        public Dictionary<int, Dictionary<int, List<FirstChargeItem>>> FirstChargeItemDict = new Dictionary<int, Dictionary<int, List<FirstChargeItem>>>();
        public Dictionary<int, int> FirstChargeFlagDict = new Dictionary<int, int>();
        public Dictionary<int, List<string>> FirstChargeModelDict = new Dictionary<int, List<string>>();
        public Dictionary<int, List<Dictionary<int, int>>> GiftCodeItems = new Dictionary<int, List<Dictionary<int, int>>>();
        private long tickTime = 0;

        public LogicServerControl()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (LogicServerControl),
                typeof (LogicServerControlDefaultImpl),
                o => { SetServiceImpl((ILogicService) o); });
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (LogicProxy), typeof (LogicProxyDefaultImpl),
                o => { SetProxyImpl((ILogicCharacterProxy) o); });
        }

        public readonly TimedTaskManager mTimedTaskManager = new TimedTaskManager();
        public ulong TickCount;

        public TimedTaskManager TaskManager
        {
            get { return mTimedTaskManager; }
        }

        public void ApplyTasks(int id)
        {
        }

        public int GetPlayerCount()
        {
            return Proxys.Count;
        }

        public void LookProxy()
        {
            foreach (var proxy in Proxys)
            {
                Logger.Info("proxy={0}", proxy.Key);
            }
        }

        public override LogicCharacterProxy NewCharacterIn(ulong characterId, ulong clientId)
        {
            return new LogicProxy(this, characterId, clientId);
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
                //dict.TryAdd("WaitingReplyMessage", OutMessage.WaitingMessageCount.ToString());

                //foreach (var agent in LogicServer.Instance.Agents.ToArray())
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
                Logger.Error(ex, "LogicServerControl Status Error!");
            }
        }

        public override IEnumerator Tick(Coroutine coroutine)
        {
            tickTime++;
            return ((ITickable) mImpl).Tick(coroutine, this);
        }
    }
}