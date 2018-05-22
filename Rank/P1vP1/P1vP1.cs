#region using

using System.Collections;
using System.Collections.Generic;
using DataContract;
using Scorpion;
using Shared;

#endregion

namespace Rank
{
    public interface IP1vP1
    {
        IEnumerator GetPvPList(Coroutine coroutine,
                               int serverId,
                               ulong guid,
                               string name,
                               List<ulong> targetList,
                               List<int> rankList);
    }

    public class P1vP1DefaultImpl : IP1vP1
    {
        public IEnumerator GetPvPList(Coroutine coroutine,
                                      int serverId,
                                      ulong guid,
                                      string name,
                                      List<ulong> targetList,
                                      List<int> rankList)

        {
            P1vP1.DBRank_One = null;
            var rank = ServerRankManager.GetRankByType(serverId, P1vP1.P1vP1RankTypeId);
            if (rank == null)
            {
                yield break;
            }
            //获得我的名次
            int myLadder;
            P1vP1.DBRank_One = rank.GetPlayerData(guid);
            if (P1vP1.DBRank_One == null)
            {
                var dbSceneSimple = RankServer.Instance.SceneAgent.GetSceneSimpleData(guid, 0);
                yield return dbSceneSimple.SendAndWaitUntilDone(coroutine);
                if (dbSceneSimple.State != MessageState.Reply)
                {
                    yield break;
                }
                if (dbSceneSimple.ErrorCode != (int) ErrorCodes.OK)
                {
                    yield break;
                }
                rank.AddRanking(serverId, guid, 0, name, dbSceneSimple.Response.FightPoint);
                P1vP1.DBRank_One = rank.GetPlayerData(guid);
                myLadder = 1000;
            }
            else
            {
                myLadder = P1vP1.DBRank_One.Rank;
            }
            //int myLadder = rank.GetPlayerLadder(guid);
            //if (myLadder == -1)
            //{
            //    myLadder = 9999;
            //}
            var rankCount = rank.GetRankListCount();
            if (myLadder > rankCount)
            {
                myLadder = rankCount;
            }
            //计算应该取得名次
            if (myLadder < 4)
            {
                for (var i = 0; i < 4; i++)
                {
                    if (i != myLadder - 1)
                    {
                        var rankOne = rank.GetRankOneByIndex(i);
                        if (rankOne != null)
                        {
                            targetList.Add(rankOne.Guid);
                            rankList.Add(rankOne.Rank);                            
                        }
                    }
                }
                //DBRank_One = null;
                yield break;
            }
            //随机3个阶段
            for (var i = 0; i != 3; ++i)
            {
                var minRank = myLadder*(i*2 + 4)/10;
                var maxRank = myLadder*(i*2 + 6)/10;
                if (myLadder < 100)
                {
                    if (i == 2)
                    {                       
                        maxRank = myLadder * i;
                    }
                }
                var thisRank = MyRandom.Random(minRank, maxRank - 1) - 1;
                var one = rank.GetRankOneByIndex(thisRank);
                
                if (one == null)
                {
                    targetList.Add(rank.GetRankOneByIndex(0).Guid);
                    rankList.Add(0);
                    continue;
                }
                while (one.Guid == guid)
                {
                    thisRank = MyRandom.Random(minRank, maxRank - 1) - 1;
                    one = rank.GetRankOneByIndex(thisRank);
                }

                targetList.Add(one.Guid);
                rankList.Add(thisRank + 1);
            }
            //targetList.Add(rank.GetRankOneByIndex(MyRandom.Random(myLadder * 4 / 10, myLadder * 6 / 10)).Guid);
            //targetList.Add(rank.GetRankOneByIndex(MyRandom.Random(myLadder * 6 / 10, myLadder * 8 / 10)).Guid);
            //targetList.Add(rank.GetRankOneByIndex(MyRandom.Random(myLadder * 8 / 10, myLadder)).Guid);
        }
    }

    public static class P1vP1
    {
        //获得列表
        public static DBRank_One DBRank_One;
        private static IP1vP1 mImpl;
        public static int P1vP1RankTypeId = 3;

        static P1vP1()
        {
            RankServer.Instance.UpdateManager.InitStaticImpl(typeof (P1vP1), typeof (P1vP1DefaultImpl),
                o => { mImpl = (IP1vP1) o; });
        }

        public static IEnumerator GetPvPList(Coroutine coroutine,
                                             int serverId,
                                             ulong guid,
                                             string name,
                                             List<ulong> targetList,
                                             List<int> rankList)
        {
            return mImpl.GetPvPList(coroutine, serverId, guid, name, targetList, rankList);
        }
    }
}