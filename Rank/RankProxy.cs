#region using

using System.Collections;
using System.Collections.Generic;
using DataContract;
using DataTable;
using Scorpion;
using NLog;
using RankServerService;
using Shared;

#endregion

namespace Rank
{
    public class RankProxyDefaultImpl : IRankCharacterProxy
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //获取排行榜列表
        public IEnumerator GetRankList(Coroutine coroutine, RankCharacterProxy charProxy, GetRankListInMessage msg)
        {
            var proxy = charProxy;

            var serverId = SceneExtension.GetServerLogicId(msg.Request.ServerId);
            var type = msg.Request.RankType;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Rank----------GetRankList----------{0},{1}", serverId, type);

            var needServerName = false;
            List<DBRank_One> tempList = null; 
            if (type == (int) RankType.DailyGift || type == (int) RankType.WeeklyGift ||
                type == (int) RankType.TotalGift)
            {
                needServerName = true;
                tempList = ServerRankManager.GetTotalRankData(type, 1, 100);
            }
            else
            {
                tempList = ServerRankManager.GetRankDataByServerId(serverId, type, 1, 100);
            }

            if (tempList == null)
            {
                msg.Reply();
                yield break;
            }
            msg.Response.RankType = type;
            foreach (var one in tempList)
            {
                if (one == null)
                {
                    Logger.Error("GetRankList serverId={0},type={1}", serverId, type);
                    continue;
                }
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

                if (needServerName && one.ServerId > 0)
                {
                    var tbServerName = Table.GetServerName(one.ServerId);
                    if (tbServerName != null)
                    {
                        rankMessage.ServerName = tbServerName.Name;                        
                    }
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

        public IEnumerator GMRank(Coroutine co, RankCharacterProxy charProxy, GMRankInMessage msg)
        {
            var proxy = charProxy;

            var command = msg.Request.Commond;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Rank----------GMRank----------{0}", command);

            var err = new AsyncReturnValue<ErrorCodes>();
            var co1 = CoroutineFactory.NewSubroutine(GameMaster.GmCommand, co, command, err);
            if (co1.MoveNext())
            {
                yield return co1;
            }
            msg.Reply((int) err.Value);
            err.Dispose();
        }

        public IEnumerator ApplyServerActivityData(Coroutine coroutine,
                                                   RankCharacterProxy _this,
                                                   ApplyServerActivityDataInMessage msg)
        {
            yield break;
        }

        public IEnumerator OnConnected(Coroutine coroutine, RankCharacterProxy charProxy, uint packId)
        {
//             RankProxy proxy = (RankProxy)charProxy;
//             proxy.Connected = true;
            yield break;
            //foreach (var waitingCheckConnectedInMessages in proxy.WaitingCheckConnectedInMessages)
            //{
            //    waitingCheckConnectedInMessages.Reply();
            //}
            //proxy.WaitingCheckConnectedInMessages.Clear();

            //var notifyConnectedMsg = RankServer.Instance.LoginAgent.NotifyConnected(proxy.ClientId, proxy.CharacterId,
            //    (int)ServiceType.Rank, (int)ErrorCodes.OK);
            //yield return notifyConnectedMsg.SendAndWaitUntilDone(coroutine);
        }

        public IEnumerator OnLost(Coroutine coroutine, RankCharacterProxy charProxy, uint packId)
        {
//             RankProxy proxy = (RankProxy)charProxy;
//             proxy.Connected = false; 
//             //foreach (var waitingCheckLostInMessage in proxy.WaitingCheckLostInMessages)
//             //{
//             //    waitingCheckLostInMessage.Reply();
//             //}
//             //proxy.WaitingCheckLostInMessages.Clear();
//             return null;
            yield break;
        }

        public bool OnSyncRequested(RankCharacterProxy charProxy, ulong characterId, uint syncId)
        {
            var proxy = charProxy;
            return false;
        }

        //获取主城战力排行榜数据
        public IEnumerator GetFightRankList(Coroutine coroutine, RankCharacterProxy charProxy, GetFightRankListInMessage msg)
        {
            var proxy = charProxy;

            var serverId = SceneExtension.GetServerLogicId(msg.Request.ServerId);
            var type = msg.Request.RankType;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Rank----------GetFightRankList----------{0},{1}", serverId, type);

            var timeNow = System.DateTime.Now;
            var record = DataTable.Table.GetServerName(serverId);
            if (null != record)
            {
                var startTime = System.DateTime.Parse(record.OpenTime);
                System.TimeSpan span = (timeNow - startTime);
                if (startTime.Date.Equals (timeNow.Date))
                {
                    if (timeNow.Day == startTime.Day)
                    {
                        msg.Reply();
                        yield break;
                    }
                }
            }

            var needServerName = false;
            List<DBRank_One> tempList = null;
            tempList = ServerRankManager.GetFightRankList(serverId,type);

            if (tempList == null)
            {
                msg.Reply();
                yield break;
            }
            msg.Response.RankType = type;
            List<int> professCount = new List<int>();
            foreach (var one in tempList)
            {
                if (professCount.Count >= 3) break;
                if (one == null)
                {
                    Logger.Error("GetRankList serverId={0},type={1}", serverId, type);
                    continue;
                }
                var rankMessage = new RankOne
                {
                    Id = one.Guid,
                    Name = one.Name //ServerRankManager.GetName(serverId, one.Guid),
                };
                if (type == (int)RankType.Level)
                {
                    rankMessage.Value = (int)(one.Value / Constants.RankLevelFactor);
                }
                else if (type == (int)RankType.CityLevel)
                {
                    rankMessage.Value = (int)(one.Value / Constants.RankLevelFactor);
                }
                else if (type == (int)RankType.Arena)
                {
                    rankMessage.Value = one.FightPoint;
                }
                else
                {
                    rankMessage.Value = (int)one.Value;
                }

                if (needServerName && one.ServerId > 0)
                {
                    var tbServerName = Table.GetServerName(one.ServerId);
                    if (tbServerName != null)
                    {
                        rankMessage.ServerName = tbServerName.Name;
                    }
                }

                var dbSceneSimple = RankServer.Instance.SceneAgent.GetSceneSimpleData(rankMessage.Id, 0);
                yield return dbSceneSimple.SendAndWaitUntilDone(coroutine);
                if (dbSceneSimple.State == MessageState.Reply)
                {
                    if (dbSceneSimple.ErrorCode == (int)ErrorCodes.OK)
                    {
                        if (rankMessage.Name == "")
                        {
                            rankMessage.Name = dbSceneSimple.Response.Name;
                            one.Name = dbSceneSimple.Response.Name;
                        }

                        int typeId = dbSceneSimple.Response.TypeId;
                        if (!professCount.Contains(typeId))
                        {
                            professCount.Add(typeId);
                            msg.Response.RankData.Add(rankMessage);
                        }
                    }
                }
                else
                {
                    if (rankMessage.Name == "")
                    {
                        //未找到
                        rankMessage.Name = "^301036";
                        one.Name = "^301036";
                    }
                }
            }
            
            msg.Reply();
        }
    }

    public class RankProxy : RankCharacterProxy
    {
        public RankProxy(RankService service, ulong characterId, ulong clientId)
            : base(service, characterId, clientId)
        {
        }

        //public List<CheckConnectedInMessage> WaitingCheckConnectedInMessages = new List<CheckConnectedInMessage>();
        //public List<CheckLostInMessage> WaitingCheckLostInMessages = new List<CheckLostInMessage>();

        public bool Connected { get; set; }
    }
}