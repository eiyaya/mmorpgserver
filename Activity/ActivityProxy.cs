#region using

using System;
using System.Collections;
using ActivityServerService;
using Scorpion;
using DataContract;
using Shared;

#endregion

namespace Activity
{
    public class ActivityProxyDefaultImpl : IActivityCharacterProxy
    {
        public IEnumerator ApplyOrderSerial(Coroutine coroutine,
                                            ActivityCharacterProxy _this,
                                            ApplyOrderSerialInMessage msg)
        {
            var proxy = (ActivityProxy) _this;

            PlayerLog.WriteLog(proxy.CharacterId, "----------Activity----------ApplyOrderSerial----------:{0}",
                msg.Request.Msg.GoodId);

            return InAppPurchase.ApplyOrderSerial(coroutine, _this.Service, msg);
        }

        public IEnumerator OnConnected(Coroutine coroutine, ActivityCharacterProxy characterProxy, uint packId)
        {
            yield break;
        }

        public IEnumerator OnLost(Coroutine coroutine, ActivityCharacterProxy characterProxy, uint packId)
        {
            yield break;
        }

        public bool OnSyncRequested(ActivityCharacterProxy characterProxy, ulong characterId, uint syncId)
        {
            return true;
        }

        public IEnumerator ApplyActivityState(Coroutine co,
                                              ActivityCharacterProxy _this,
                                              ApplyActivityStateInMessage msg)
        {
            var state = WorldBossManager.GetState(SceneExtension.GetServerLogicId(msg.Request.ServerId));
            msg.Response.Data.Add((int) eActivity.WorldBoss, (int) state);
            msg.Reply();
            yield break;
        }

        public IEnumerator ApplyMieShiData(Coroutine coroutine, ActivityCharacterProxy _this,
            ApplyMieShiDataInMessage msg)
        {
            MieShiManager.GetActivityData(SceneExtension.GetServerLogicId(msg.Request.ServerId), _this.CharacterId, msg.Response);
            msg.Reply();
            //var temp = DateTime.FromBinary((long)msg.Response.Datas[0].actiTime);
            yield break;
        }
        public IEnumerator ApplyMieshiHeroLogData(Coroutine coroutine, ActivityCharacterProxy _this,
            ApplyMieshiHeroLogDataInMessage msg)
        {
            MieShiManager.ApplyMieshiHeroLogData(SceneExtension.GetServerLogicId(msg.Request.ServerId), msg.Response);
            msg.Reply();
            //var temp = DateTime.FromBinary((long)msg.Response.Datas[0].actiTime);
            yield break;
        }

        public IEnumerator ApplyBatteryData(Coroutine coroutine, ActivityCharacterProxy _this,
            ApplyBatteryDataInMessage msg)
        {
            MieShiManager.GetBatteryData(SceneExtension.GetServerLogicId(msg.Request.ServerId), msg.Request.ActivityId, _this.CharacterId, msg.Response);
            msg.Reply();
            yield break;
        }

        public IEnumerator ApplyContriRankingData(Coroutine coroutine, ActivityCharacterProxy _this,
            ApplyContriRankingDataInMessage msg)
        {
            MieShiManager.GetContriRankingData(SceneExtension.GetServerLogicId(msg.Request.ServerId), msg.Request.ActivityId, _this.CharacterId, msg.Response);
            msg.Reply();
            yield break;
        }

        public IEnumerator ApplyPointRankingData(Coroutine coroutine, ActivityCharacterProxy _this,
            ApplyPointRankingDataInMessage msg)
        {
            MieShiManager.GetPointRankingData(SceneExtension.GetServerLogicId(msg.Request.ServerId), msg.Request.ActivityId, _this.CharacterId, msg.Response);
            msg.Reply();
            yield break;
        }

        public IEnumerator ApplyPortraitData(Coroutine coroutine, ActivityCharacterProxy _this,
            ApplyPortraitDataInMessage msg)
        {
            PlayerInfoMsg data = new PlayerInfoMsg();
            ErrorCodes result = MieShiManager.ApplyPortraitData(SceneExtension.GetServerLogicId(msg.Request.ServerId), ref data);
            msg.Response = data;
            msg.Reply((int)result);
            yield break;
        }
        public IEnumerator ApplyBossHome(Coroutine coroutine, ActivityCharacterProxy _this, ApplyBossHomeInMessage msg)
        {
            var bossDieDic = BossHomeManager.RefreshBossHomeData(msg.Request.ServerId);
            msg.Response.Data.AddRange(bossDieDic);
            msg.Reply((int)ErrorCodes.OK);
            yield break;
        }

        public IEnumerator ApplyChickenRankData(Coroutine coroutine, ActivityCharacterProxy _this,ApplyChickenRankDataInMessage msg)
        {
            var data = ChickenManager.ApplyChickenRank(msg.Request.CharacterId);
            msg.Response = data;
            msg.Reply((int)ErrorCodes.OK);
            yield break;
        }
        public IEnumerator ApplyAcientBattle(Coroutine coroutine, ActivityCharacterProxy _this, ApplyAcientBattleInMessage msg)
        {
            var bossDieDic = AcientBattleManager.RefreshAcientBattleData(msg.Request.ServerId, -1);
            msg.Response.Data.AddRange(bossDieDic);
            msg.Reply((int)ErrorCodes.OK);
            yield break;
        }
    }

    public class ActivityProxy : ActivityCharacterProxy
    {
        public ActivityProxy(ActivityService service, ulong characterId, ulong clientId)
            : base(service, characterId, clientId)
        {
        }

        public bool Connected { get; set; }
    }
}