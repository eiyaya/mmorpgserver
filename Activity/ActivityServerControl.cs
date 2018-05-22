#region using

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ActivityServerService;
using DataContract;
using DataTable;
using EventSystem;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Activity
{
    public class ActivityServerControlDefaultImpl : IActivityService, ITickable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public IEnumerator OnConnected(Coroutine coroutine,
                                       ActivityCharacterProxy characterProxy,
                                       AsyncReturnValue<bool> ret)
        {
            ret.Value = true;
            var proxy = (ActivityProxy) characterProxy;
            proxy.Connected = true;

            yield break;
        }

        public IEnumerator PrepareDataForEnterGame(Coroutine co,
                                                   ActivityService _this,
                                                   PrepareDataForEnterGameInMessage msg)
        {
            msg.Reply();
            Logger.Info("Enter Game {0} - PrepareDataForEnterGame - 1 - {1}", msg.CharacterId,
                TimeManager.Timer.ElapsedMilliseconds);
            return null;
        }

        public IEnumerator PrepareDataForCreateCharacter(Coroutine co,
                                                         ActivityService _this,
                                                         PrepareDataForCreateCharacterInMessage msg)
        {
            msg.Reply();
            Logger.Info("Reply PrepareDataForCreateCharacter Activity {0}", msg.CharacterId);
            return null;
        }

        public IEnumerator PrepareDataForCommonUse(Coroutine co,
                                                   ActivityService _this,
                                                   PrepareDataForCommonUseInMessage msg)
        {
            msg.Reply();
            return null;
        }

        public IEnumerator PrepareDataForLogout(Coroutine co, ActivityService _this, PrepareDataForLogoutInMessage msg)
        {
            msg.Reply();

            yield break;
        }

		public IEnumerator ServerGMCommand(Coroutine coroutine, ActivityService _this, ServerGMCommandInMessage msg)
        {
			var cmd = msg.Request.Cmd;
			var param = msg.Request.Param;

			Logger.Info("Activity----------ServerGMCommand----------cmd={0}|param={1}", cmd, param);

			try
			{
				if ("ReloadTable" == cmd)
				{
					Table.ReloadTable(param);
				}
			}
			catch (Exception e)
			{

				Logger.Error("Activity----------ServerGMCommand----------error={0}", e.Message);
			}
			finally
			{

			}
			yield break;
        }

        public IEnumerator CheckConnected(Coroutine coroutine, ActivityService _this, CheckConnectedInMessage msg)
        {
            Logger.Error("Activity CheckConnected, {0}", msg.CharacterId);

            //ActivityCharacterProxy proxy = null;
            //if (_this.Proxys.TryGetValue(msg.CharacterId, out proxy))
            //{
            //    if ((proxy as ActivityProxy).Connected)
            //    {
            //        msg.Response = 1;
            //        msg.Reply();
            //        return null;
            //    }

            //    (proxy as ActivityProxy).WaitingCheckConnectedInMessages.Add(msg);
            //}

            return null;
        }

        public IEnumerator CheckLost(Coroutine coroutine, ActivityService _this, CheckLostInMessage msg)
        {
            Logger.Error("Activity CheckLost, {0}", msg.CharacterId);

            //ActivityCharacterProxy proxy = null;
            //if (!_this.Proxys.TryGetValue(msg.CharacterId, out proxy))
            //{
            //    msg.Reply();
            //}
            //else
            //{
            //    if ((proxy as ActivityProxy).Connected)
            //    {
            //        (proxy as ActivityProxy).WaitingCheckLostInMessages.Add(msg);
            //    }
            //    else
            //    {
            //        msg.Reply();
            //    }
            //}

            return null;
        }

        public IEnumerator QueryStatus(Coroutine co, ActivityService _this, QueryStatusInMessage msg)
        {
            var common = new ServerCommonStatus();
            common.Id = ActivityServer.Instance.Id;
            common.ByteReceivedPerSecond = _this.ByteReceivedPerSecond;
            common.ByteSendPerSecond = _this.ByteSendPerSecond;
            common.MessageReceivedPerSecond = _this.MessageReceivedPerSecond;
            common.MessageSendPerSecond = _this.MessageSendPerSecond;
            common.ConnectionCount = _this.ConnectionCount;

            msg.Response.CommonStatus = common;

            msg.Response.ConnectionInfo.AddRange(ActivityServer.Instance.Agents.Select(kv =>
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

        public IEnumerator UpdateServer(Coroutine coroutine, ActivityService _this, UpdateServerInMessage msg)
        {
            ActivityServer.Instance.UpdateManager.Update();
            return null;
        }

        public IEnumerator SSNotifyCharacterOnConnet(Coroutine coroutine,
                                                     ActivityService _this,
                                                     SSNotifyCharacterOnConnetInMessage msg)
        {
            var characterId = msg.Request.CharacterId;
            var clientId = msg.Request.ClientId;
            var proxy = new ActivityProxy(_this, characterId, clientId);
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
                                                   ActivityService _this,
                                                   BSNotifyCharacterOnLostInMessage msg)
        {
            var characterId = msg.Request.CharacterId;
            ActivityCharacterProxy charProxy;
            if (!_this.Proxys.TryGetValue(characterId, out charProxy))
            {
                yield break;
            }
            var proxy = (ActivityProxy) charProxy;
            proxy.Connected = false;
        }

        public IEnumerator OnServerStart(Coroutine co, ActivityService _this)
        {
            //Thread.Sleep(10000);
            ActivityServer.Instance.Start(_this);
            SpeMonsterManager.Init();
            WorldBossManager.Init();
            InAppPurchase.Init();
            MieShiManager.Init();
            BossHomeManager.Init();
            AcientBattleManager.Init();
            GeneralActivityManager.Init();
            ServerMysteryStoreManager.Init();
            ServerBlackStoreManager.Init();
            ActivityServer.Instance.IsReadyToEnter = true;
            ChickenManager.Init();
            _this.TickDuration = ActivityServerControl.Performance;

            var __this = (ActivityServerControl) _this;
            __this.payDbManagerManager.Init(_this);
            __this.WebRequestManager = new RequestManager(__this);
            var timesp = DateTime.Now - DateTime.Parse("1970-1-1");
            var milliseconds = timesp.TotalMilliseconds;
            __this.udidSeed = (long)milliseconds % 100000000000L;
            __this.Started = true;

            yield break;
        }

        public IEnumerator Tick(Coroutine coroutine, ServerAgentBase _this)
        {
            try
            {
                ActivityServerControl.Timer.Update();
                InAppPurchase.Update((ActivityServerControl) _this);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Tick error.");
            }

	        try
	        {
				ActivityServerMonitor.TickRate.Mark();
	        }
	        catch (Exception)
	        {
		        
	        }
            return null;
        }

        public IEnumerator OnServerStop(Coroutine co, ActivityService _this)
        {
            MieShiManager.UnInit();
            ActivityServer.Instance.DB.Dispose();
            ((ActivityServerControl) _this).payDbManagerManager.Stop();
            ((ActivityServerControl)_this).WebRequestManager.Stop();
            ChickenManager.UnInit();
            yield break;
        }

        public IEnumerator ReadyToEnter(Coroutine coroutine, ActivityService _this, ReadyToEnterInMessage msg)
        {
            if (ActivityServer.Instance.IsReadyToEnter && ActivityServer.Instance.AllAgentConnected())
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

        public IEnumerator QueryCreateMonsterData(Coroutine co,
                                                  ActivityService _this,
                                                  QueryCreateMonsterDataInMessage msg)
        {
            bool isBossHome = false;
            Table.ForeachBossHome(record =>
            {
                if (record.Scene == msg.Request.SceneId)
                {
                    isBossHome = true;
                    return false;
                }
                return true;
            });
            if (isBossHome)
            {
                var tmp = BossHomeManager.RefreshBossHomeData(msg.Request.ServerId);
                Int32Array ret = new Int32Array();
                foreach (var v in tmp)
                {
                    if (v.Value == 0)
                    {
                        var tb = Table.GetBossHome(v.Key);
                        if (tb != null)
                        {
                           ret.Items.Add(tb.SceneNpcId);
                        }
                    }
                }
                msg.Response = ret;
            }
            else
            {
                msg.Response = SpeMonsterManager.GetCreateMonsterData(msg.Request.ServerId, msg.Request.SceneId);                
            }
            msg.Reply();
            yield return null;
        }

        public IEnumerator NotifyDamageList(Coroutine co, ActivityService _this, NotifyDamageListInMessage msg)
        {
            var response = WorldBossManager.ApplyDamage(msg.Request.ServerId, msg.Request.SceneGuid, msg.Request.List);
            if (response != null)
            {
                msg.Response = response;
                msg.Reply();
            }
            else
            {
                msg.Reply((int) ErrorCodes.ServerID);
            }
            yield return null;
        }

        public IEnumerator SSApplyActivityState(Coroutine coroutine,
                                                ActivityService _this,
                                                SSApplyActivityStateInMessage msg)
        {
            var state = WorldBossManager.GetState(msg.Request.ServerId);
            msg.Response = (int) state;
            msg.Reply();
            yield return null;
        }
        public IEnumerator SSAskMieshiTowerUpTimes(Coroutine coroutine,
                                                ActivityService _this,
                                                SSAskMieshiTowerUpTimesInMessage msg)
        {
            int times = 0;
            ErrorCodes result = MieShiManager.GetPlayerTowerUpTimes(msg.Request.ServerId,msg.Request.ActivityId,msg.Request.CharacterId,ref times);
            msg.Response = times;
            msg.Reply((int)result);
            yield return null;
        }

        public IEnumerator SSGetBlackStoreItems(Coroutine coroutine,
                                                ActivityService _this,
                                                SSGetBlackStoreItemsInMessage msg)
        {
            var serverId = msg.Request.ServerId;
            var tempList = new List<StoneItem>();
            var result = ServerBlackStoreManager.ApplyStoreInfo(serverId, ref tempList);
            if (result != (int)ErrorCodes.OK)
            {
                msg.Reply(result);
                yield break;
            }
            //Logger.Warn("GetBlackStoreItems--------------serverId : {0}", serverId);
            msg.Response.items.AddRange(tempList);
            msg.Reply((int)ErrorCodes.OK);
            yield break;
        }

        public IEnumerator SSGetTreasureShopItems(Coroutine coroutine,
                                                  ActivityService _this,
                                                  SSGetTreasureShopItemsInMessage msg)
        {
            var serverId = msg.Request.ServerId;
            var tempList = new List<StoneItem>();
            var result = ServerMysteryStoreManager.ApplyStoreInfo(serverId, ref tempList);
            if (result != (int)ErrorCodes.OK)
            {
                msg.Reply(result);
                yield break;
            }
            //Logger.Warn("GetTreasureShopItems--------------serverId : {0}", serverId);
            msg.Response.items.AddRange(tempList);
            msg.Reply((int)ErrorCodes.OK);
            yield break;
        }

        public IEnumerator SSGetTreasureShopItemCount(Coroutine coroutine,
                                                      ActivityService _this,
                                                      SSGetTreasureShopItemCountInMessage msg)
        {
            var serverId = msg.Request.ServerId;
            var storeId = msg.Request.StoreId;
            var itemCount = 0;
            var result = ServerMysteryStoreManager.GetStoreItemCount(serverId, storeId, ref itemCount);
            if (result != (int)ErrorCodes.OK)
            {
                msg.Reply(result);
                yield break;
            }
            //Logger.Warn("GetTreasureShopItemCount--------------storeId : {0},itemCount : {1}", storeId, itemCount);
            msg.Response = itemCount;
            msg.Reply(result);
            yield break;
        }

        public IEnumerator SSConsumeTreasureShopItem(Coroutine coroutine,
                                                     ActivityService _this,
                                                     SSConsumeTreasureShopItemInMessage msg)
        {
            var serverId = msg.Request.ServerId;
            var storeId = msg.Request.StoreId;
            var consumeCount = msg.Request.ConsumeCount;
            var result = ServerMysteryStoreManager.ConsumeStoreItem(serverId, storeId, consumeCount);
            //Logger.Warn("ConsumeTreasureShopItem--------------storeId : {0}", storeId);
            msg.Reply(result);
            yield break;
        }

        public IEnumerator BossHomeSceneRequest(Coroutine coroutine, ActivityService _this, BossHomeSceneRequestInMessage msg)
        {
            BossHomeManager.OnBossDie(msg.Request.ServerId, msg.Request.NpcId);
            msg.Reply((int)ErrorCodes.OK);
           yield return null;
        }

        public IEnumerator SSAcientBattleSceneRequest(Coroutine coroutine, ActivityService _this, SSAcientBattleSceneRequestInMessage msg)
        {
            var bossDieDic = AcientBattleManager.RefreshAcientBattleData(msg.Request.ServerId, msg.Request.NpcId);
            msg.Response.Data.AddRange(bossDieDic);
            msg.Reply((int)ErrorCodes.OK);
            yield return null;
        }
        public IEnumerator SSApplyPromoteHP(Coroutine coroutine, ActivityService _this, SSApplyPromoteHPInMessage msg)
        {
            BatteryUpdateData update = new BatteryUpdateData();
            ErrorCodes result = MieShiManager.PromoteBatteryHp(msg.Request.ServerId, msg.Request.ActivityId,
                msg.Request.BatteryId, msg.Request.PromoteType, msg.Request.CharacterId, msg.Request.Name, ref update);
            msg.Response = update;
            msg.Reply((int)result);
            yield return null;
        }

        public IEnumerator SSApplyPromoteSkill(Coroutine coroutine, ActivityService _this, SSApplyPromoteSkillInMessage msg)
        {
            BatteryUpdateData update = new BatteryUpdateData();
            ErrorCodes result = MieShiManager.PromoteBatterySkill(msg.Request.ServerId, msg.Request.ActivityId,
                msg.Request.BatteryId, msg.Request.PromoteType, msg.Request.CharacterId, msg.Request.Name, ref update);
            msg.Response = update;
            msg.Reply((int)result);
            yield return null;
        }

        public IEnumerator SSApplyJoinActivity(Coroutine coroutine, ActivityService _this,
            SSApplyJoinActivityInMessage msg)
        {
            ErrorCodes result = MieShiManager.ApplyJoinActivity(msg.Request.ServerId, msg.Request.ActivityId, msg.CharacterId);
            if (result == ErrorCodes.OK)
            {
                msg.Response = 1;
            }
            else
            {
                msg.Response = 0;
            }
            msg.Reply((int)result);
            yield break;
        }

        public IEnumerator SSSyncMieShiData(Coroutine coroutine, ActivityService _this, SSSyncMieShiDataInMessage msg)
        {
            MieShiManager.SetPointRankData(msg.Request.ServerId, msg.Request.ActivityId, msg.Request.List);
            msg.Response = 1;
            msg.Reply((int)ErrorCodes.OK);
            yield return null;
        }
        public IEnumerator SSAskMieshiTowerReward(Coroutine coroutine, ActivityService _this, SSAskMieshiTowerRewardInMessage msg)
        {
//            MieShiManager.SetPointRankData(msg.Request.ServerId, msg.Request.ActivityId, msg.Request.List);
            int flag = 0;

            ErrorCodes errorcode = MieShiManager.OnPlayerGetTowerReward(msg.Request.ServerId, msg.Request.ActivityId, msg.Request.CharacterId,msg.Request.Idx,ref flag);
            msg.Response = flag;
            msg.Reply((int)errorcode);
            yield return null;
        }      

        public IEnumerator SSApplyContributeRate(Coroutine coroutine, ActivityService _this,
            SSApplyContributeRateInMessage msg)
        {
            ContriRateList rate = new ContriRateList();
            ErrorCodes result = MieShiManager.ApplyContributeRate(msg.Request.ServerId, msg.Request.ActivityId, ref rate);
            msg.Response = rate;
            msg.Reply((int)result);
            yield return null;
        }

        public IEnumerator SSSaveActivityResult(Coroutine coroutine, ActivityService _this,
            SSSaveActivityResultInMessage msg)
        {
            ErrorCodes result = MieShiManager.SetActivityResult(msg.Request.ServerId, msg.Request.ActivityId, msg.Request.Result);
            msg.Reply((int)result);
            yield return null;
        }

        public IEnumerator SSApplyActiResultList(Coroutine coroutine, ActivityService _this,
            SSApplyActiResultListInMessage msg)
        {
            MieShiActivityResultList list = new MieShiActivityResultList();
            MieShiManager.ApplyActiResultList(0, list);
            msg.Response = list;
            msg.Reply((int)ErrorCodes.OK);
            yield return null;
        }

        public IEnumerator SSSetAndGetActivityData(Coroutine coroutine, ActivityService _this, SSSetAndGetActivityDataInMessage msg)
        {
            var actiInfo = MieShiManager.SaveGuidAndGetActiInfo(msg.Request.SceneGuid,msg.Request.ServerId, msg.Request.ActivityId, msg.Request.GuidList);
            msg.Response = actiInfo;
            msg.Reply((int)ErrorCodes.OK);
            yield return null;
        }
        public IEnumerator SSSyncChickenScore(Coroutine coroutine, ActivityService _this, SSSyncChickenScoreInMessage msg)
        {
            ChickenManager.SaveChickenScore(msg.Request.RankData);
            yield return null;
        }

        public IEnumerator SSApplyMieShiCanIn(Coroutine coroutine, ActivityService _this,
            SSApplyMieShiCanInInMessage msg)
        {
            ErrorCodes result = MieShiManager.ApplyEnterActivity(msg.CharacterId, msg.Request.ServerId, msg.Request.ActivityId);
            msg.Response = (int)result;
            msg.Reply((int)result);
            yield return null;
        }

        public IEnumerator SyncActivityAllPlayerExit(Coroutine coroutine, ActivityService _this,
            SyncActivityAllPlayerExitInMessage msg)
        {
            MieShiManager.SyncActivityAllPlayerExit(msg.CharacterId, msg.Request.ServerId, msg.Request.ActivityId);
            yield return null;
        }
        
        public IEnumerator SSSyncMieShiBoxCanPickUp(Coroutine coroutine, ActivityService _this,
            SSSyncMieShiBoxCanPickUpInMessage msg)
        {
            ErrorCodes result = MieShiManager.SyncCanPickUpBox(msg.Request.ServerId, msg.Request.ActivityId, msg.Request.NpcId);
            msg.Reply((int)result);
            yield return null;
        }

        public IEnumerator SSApplyPickUpBox(Coroutine coroutine, ActivityService _this, SSApplyPickUpBoxInMessage msg)
        {
            ErrorCodes result = MieShiManager.ApplyPickUpBox(msg.Request.ServerId, msg.Request.ActivityId, msg.Request.NpcId, msg.CharacterId);
            msg.Reply((int)result);
            yield return null;
        }

        public IEnumerator SSApplyPortraitAward(Coroutine coroutine, ActivityService _this,
            SSApplyPortraitAwardInMessage msg)
        {
            ErrorCodes result = MieShiManager.ApplyPortraitAward(msg.Request.ServerId, msg.CharacterId);
            if ((int) result == 1)
            {
                msg.Response = 1;
            }
            else
            {
                msg.Response = 0;
            }
            msg.Reply((int)result);
            yield return null;
        }

        public IEnumerator SSSaveBatteryDestroy(Coroutine coroutine, ActivityService _this,
            SSSaveBatteryDestroyInMessage msg)
        {
            ErrorCodes result = MieShiManager.SaveBatteryDestroy(msg.Request.ServerId, msg.Request.ActivityId, msg.Request.BatteryGuid);
            if ((int)result == 1)
            {
                msg.Response = 1;
            }
            else
            {
                msg.Response = 0;
            }
            msg.Reply((int)result);
            yield return null;
        }

        public IEnumerator SSApplyLastResult(Coroutine coroutine, ActivityService _this, SSApplyLastResultInMessage msg)
        {
            MieShiActiGroup actiGroup;
            if (MieShiManager.Activity.TryGetValue(msg.Request.ServerId, out actiGroup))
            {
                if (actiGroup.DBData != null)
                {
                    msg.Response.EndTime = actiGroup.DBData.LastEndTime;
                    msg.Response.Result = actiGroup.DBData.LastResult;
                    msg.Reply();
                    yield break;
                }
            }

            msg.Reply((int)ErrorCodes.Unknow);
            yield return null;            
        }
    }

    public class ActivityServerControl : ActivityService
    {
        //心跳频率
        public const float Frequence = 20.0f;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //每帧时长
        public const float Performance = 1/Frequence;
        public static TimeManager Timer = new TimeManager();
        private static Trigger trigger;
        private long tickTime = 0;

        //给loopgame发送订单用
        public RequestManager WebRequestManager = null;

        public long udidSeed;
        public string PayServerNotifyAddress;
        public string PayServerVerifyAddress;
        public ActivityServerControl()
        {
            ActivityServer.Instance.UpdateManager.InitStaticImpl(typeof (ActivityServerControl),
                typeof (ActivityServerControlDefaultImpl),
                o => { SetServiceImpl((IActivityService) o); });
            ActivityServer.Instance.UpdateManager.InitStaticImpl(typeof (ActivityProxy),
                typeof (ActivityProxyDefaultImpl),
                o => { SetProxyImpl((IActivityCharacterProxy) o); });
            EventDispatcher.Instance.AddEventListener(ReloadTableEvent.EVENT_TYPE, ReloadTable);
        }

        public PayDbManager payDbManagerManager = new PayDbManager();

        public override ActivityCharacterProxy NewCharacterIn(ulong characterId, ulong clientId)
        {
            return new ActivityProxy(this, characterId, clientId);
        }

        private static void NotifyTableChange()
        {
            trigger = null;
            ActivityServer.Instance.ServerControl.NotifyTableChange((int) eNotifyTableChangeFlag.RechargeTables);
        }

        public override void OnException(Exception ex)
        {
            Logger.Error(ex, "Network error");
        }

        public override IEnumerator OnServerStart(Coroutine co)
        {
            return mImpl.OnServerStart(co, this);
        }

        public override IEnumerator OnServerStop(Coroutine co)
        {
            return mImpl.OnServerStop(co, this);
        }

        public override IEnumerator PerformenceTest(Coroutine co, ServerClient client, ServiceDesc desc)
        {
            client.SendMessage(desc);
            yield break;
        }

        private static void ReloadTable(IEvent ievent)
        {
            var e = ievent as ReloadTableEvent;
            switch (e.tableName)
            {
                case "RechargeActive":
                case "RechargeActiveNotice":
                case "RechargeActiveCumulative":
                case "RechargeActiveCumulativeReward":
                case "RechargeActiveInvestment":
                case "RechargeActiveInvestmentReward":
                {
                    if (trigger == null)
                    {
                        trigger = Timer.CreateTrigger(DateTime.Now.AddSeconds(5), NotifyTableChange);
                    }
                }
                    break;
            }
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

                //foreach (var agent in ActivityServer.Instance.Agents.ToArray())
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
                Logger.Error(ex, "ActivityServerControl Status Error!{0}");
            }
        }

        public override IEnumerator Tick(Coroutine coroutine)
        {
            tickTime++;
            return ((ITickable) mImpl).Tick(coroutine, this);
        }
    }
}