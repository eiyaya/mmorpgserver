#region using

using System;
using System.Collections;
using System.Linq;
using DataContract;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Team
{
    //Logic所有GM命令
    public interface IGameMaster
    {
        void AllianceWarBegin(int serverId, DateTime time);
        ErrorCodes AllianceWarBid(ulong guid, int value);
        void CleanMatching(int id);
        void PushMatchingLog();
        void ReloadTable(string tableName);
        void UnionMoneyAdd(ulong guid, int value);
        void AllianceWarStartBid(int serverId);
    }

    public class GameMasterDefaultImpl : IGameMaster
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IEnumerator ReloadTableCoroutine(Coroutine coroutine, string tableName)
        {
			var Reloadtable = TeamServer.Instance.TeamAgent.ServerGMCommand("ReloadTable",tableName);
            yield return Reloadtable.SendAndWaitUntilDone(coroutine);
        }

        //清空排队
        public void CleanMatching(int id)
        {
            if (id == -1)
            {
                QueueSceneManager.ClearScene(id);
            }
            QueueManager.ClearQueue(id);
        }

        //查看排队，组队Log
        public void PushMatchingLog()
        {
            QueueManager.PushLog();
            QueueSceneManager.PushLog();
            QueueTeamManager.PushLog();
            TeamManager.PushLog();
        }

        //增加战盟资金
        //addGmGMCommand("!!UnionMoneyAdd", CommandType.GMTeam);
        public void UnionMoneyAdd(ulong guid, int value)
        {
            if (value <= 0)
            {
                return;
            }
            var a = ServerAllianceManager.GetAllianceByCharacterId(guid);
            if (a == null)
            {
                return;
            }
            a.SetMoney(a.Money + value);
        }

        public ErrorCodes AllianceWarBid(ulong guid, int value)
        {
            var alliance = ServerAllianceManager.GetAllianceByCharacterId(guid);
            if (alliance == null)
            {
                return ErrorCodes.Error_CharacterNoAlliance;
            }
            var allianceId = alliance.AllianceId;
            var serverId = alliance.ServerId;
            var logicServerId = SceneExtension.GetServerLogicId(serverId);
            var allianceManager = ServerAllianceManager.GetAllianceByServer(logicServerId);
            if (allianceManager == null)
            {
                return ErrorCodes.Error_AllianceState;
            }
            var dbAlliance = allianceManager.GetServerData(serverId);
            if (dbAlliance == null)
            {
                return ErrorCodes.Error_AllianceState;
            }
            var dbAllianceNew = allianceManager.GetServerData(logicServerId);
            if (dbAllianceNew == null)
            {
                return ErrorCodes.Error_AllianceState;
            }
            if (allianceId == dbAllianceNew.Occupant)
            {
                return ErrorCodes.Error_OccupantNoNeedBid;
            }
            var bidDatas = dbAlliance.BidDatas;
            int price;
            bidDatas.TryGetValue(allianceId, out price);
            price += value;
            bidDatas[allianceId] = price;
            return ErrorCodes.OK;
        }

        public void AllianceWarBegin(int serverId, DateTime time)
        {
            var allianceManager = ServerAllianceManager.GetAllianceByServer(serverId);
            if (allianceManager == null)
            {
                return;
            }
            DBServerAllianceData data;
            if (allianceManager.mDBData.TryGetValue(serverId, out data))
            {
                data.Challengers.Clear();
                //排序整理出前两名
                var datas = data.BidDatas;
                var order = datas.OrderByDescending(o => o.Value);
                var e = order.GetEnumerator();
                for (var i = 0; e.MoveNext(); i++)
                {
                    var cur = e.Current;
                    var a = ServerAllianceManager.GetAllianceById(cur.Key);
                    if (a == null)
                    {
                        Logger.Error("In BidOver().alliance == null! id = {0}", cur.Key);
                        continue;
                    }
                    if (i < 2)
                    {
                        data.Challengers.Add(cur.Key);
                    }
                }
                allianceManager.GetServerData(data.ServerId).LastBattleTime = 0;
                allianceManager.SetFlag(data.ServerId);

                var war = AllianceWarManager.WarDatas[serverId];
                war.BidOver();
                war.BattleFieldGuid = 0;

                if (GameMaster.StartActivityTrigger != null)
                {
                    TeamServerControl.tm.DeleteTrigger(GameMaster.StartActivityTrigger);
                }
                GameMaster.StartActivityTrigger = TeamServerControl.tm.CreateTrigger(time, war.StartActivity);
                if (GameMaster.StartFightTrigger != null)
                {
                    TeamServerControl.tm.DeleteTrigger(GameMaster.StartFightTrigger);
                }
                GameMaster.StartFightTrigger = TeamServerControl.tm.CreateTrigger(time.AddMinutes(10),
                    war.StartActivity);
            }
        }
        public void AllianceWarStartBid(int serverId)
        {
            var allianceManager = ServerAllianceManager.GetAllianceByServer(serverId);
            if (allianceManager == null)
            {
                return;
            }
            var war = AllianceWarManager.WarDatas[serverId];
            if (war == null)
            {
                return;
            }
            war.StartBid();
        }
        public void ReloadTable(string tableName)
        {
            CoroutineFactory.NewCoroutine(ReloadTableCoroutine, tableName).MoveNext();
        }
    }

    //Logic所有GM命令
    public static class GameMaster
    {
        private static IGameMaster mImpl;
        public static Trigger StartActivityTrigger;
        public static Trigger StartFightTrigger;

        static GameMaster()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (GameMaster), typeof (GameMasterDefaultImpl),
                o => { mImpl = (IGameMaster) o; });
        }

        //
        public static void AllianceWarBegin(int serverId, DateTime time)
        {
            mImpl.AllianceWarBegin(serverId, time);
        }
        //盟战报名
        public static void AllianceWarStartBid(int serverId)
        {
            mImpl.AllianceWarStartBid(serverId);
        }
        //
        public static ErrorCodes AllianceWarBid(ulong guid, int value)
        {
            return mImpl.AllianceWarBid(guid, value);
        }

        //清空排队
        public static void CleanMatching(int id)
        {
            mImpl.CleanMatching(id);
        }

        //查看排队，组队Log
        public static void PushMatchingLog()
        {
            mImpl.PushMatchingLog();
        }

        public static void ReloadTable(string tableName)
        {
            mImpl.ReloadTable(tableName);
        }

        //增加战盟资金
        //addGmGMCommand("!!UnionMoneyAdd", CommandType.GMTeam);
        public static void UnionMoneyAdd(ulong guid, int value)
        {
            mImpl.UnionMoneyAdd(guid, value);
        }
    }
}