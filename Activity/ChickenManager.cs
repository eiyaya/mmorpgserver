using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database;
using DataContract;
using Scorpion;
using Shared;
using DataTable;
namespace Activity
{
    public interface IChickenManager
    {
        #region 数据
        void Init();
        IEnumerator FlushAll(Coroutine coroutine);
        string GetDbName();
        void InitByBase();
        #endregion
        #region 逻辑
        void SaveChickenScore(ChickenRankData rankData);

        ChickenRankData ApplyChickenRank(ulong charId);
        void FinalLogic();
        void UnInit();
        #endregion
    }

    public class ChickenManagerDefaultImpl : IChickenManager
    {
        [Updateable("Chicken")]
        public const string DbKey = "Chicken";
        #region 数据
        public void Init()
        {
            var tbFuben = Table.GetFuben(30000);
            ChickenManager.OpenLastMin = tbFuben.OpenLastMinutes;
            ChickenManager.TimeLimitMin = tbFuben.TimeLimitMinutes;
            var dungeonTotalTime = ChickenManager.OpenLastMin + ChickenManager.TimeLimitMin;

            //检查副本开启时间
            var now = DateTime.Now;
            foreach (var time in tbFuben.OpenTime)
            {
                var tarTime = new DateTime(now.Year, now.Month, now.Day, time / 100, time % 100, 0, DateTimeKind.Local);
                if (tarTime.AddMinutes(dungeonTotalTime) < now)
                {
                    tarTime = tarTime.AddDays(1);
                    ++ChickenManager.TimeIdx;
                }
                ChickenManager.TargetTimes.Add(tarTime);
            }
            ChickenManager.TimeIdx = ChickenManager.TimeIdx % ChickenManager.TargetTimes.Count;

            //计算当前的世界boss副本状态
            var openTime = ChickenManager.TargetTimes[ChickenManager.TimeIdx];
            var startTime = openTime.AddMinutes(ChickenManager.OpenLastMin);
            var endTime = startTime.AddMinutes(ChickenManager.TimeLimitMin);

            if (openTime > now)
            {
                ChickenManager.state = eActivityState.WaitNext;
            }
            else if (startTime > now)
            {
                ChickenManager.state = eActivityState.WillStart;
            }
            else if (endTime > now)
            {
                ChickenManager.state = eActivityState.Start;
            }
            else
            {
                ChickenManager.state = eActivityState.WaitNext;
            }
            WaitNextBoss();
            //Debug.Assert(false);
            CoroutineFactory.NewCoroutine(ReadDb).MoveNext();
            PlayerLog.WriteLog((int)LogType.ClientError, "--------------------------------------CheckenManager-------------------------------------------");
            if (ChickenManager.SaveTrigger == null)
            {
                ChickenManager.SaveTrigger = ActivityServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(30),
                    () => Save(), 30000);
            }
        }

        public void Save()
        {
            if (ChickenManager.SaveTrigger == null)
            {
                return;
            }
            CoroutineFactory.NewCoroutine(FlushAll).MoveNext();
        }
        public IEnumerator FlushAll(Coroutine coroutine)
        {
            if (ChickenManager.bDirty == false)
                yield break;
            ChickenManager.bDirty = false;
            var co = CoroutineFactory.NewSubroutine(SaveDb, coroutine);
            if (co.MoveNext())
            {
                yield return co;
            }
        }

        public void InitByBase()
        {
            var t = DateTime.Now.Date.AddDays(1).AddSeconds(5);
            ActivityServerControl.Timer.CreateTrigger(t, () => { OnDayTimer(); }, 60 * 60 * 24 * 1000);
        }

        private void OnDayTimer()
        {
            FinalLogic();
            ChickenManager.bDirty = true;
        }

        public void FinalLogic()
        {
            if (ChickenManager.mDbData == null)
                return;
            Dictionary<int, Dict_int_int_Data> title = new Dictionary<int, Dict_int_int_Data>();
            var tb = Table.GetNameTitle(9000);
            if (tb != null)
            {
                Dict_int_int_Data tmp = new Dict_int_int_Data();
                tmp.Data.Add(tb.FlagId, 1);
                title.Add(0, tmp);
            }

            if (ChickenManager.mDbData.Count > 0)
            {
                var id = ChickenManager.mDbData[0].Guid;
                ChangeNameTitle(id, title[0]);
            }
            FinalActivity();
            ChickenManager.mDbData.Clear();
            Save();

        }
        private void ChangeNameTitle(ulong characterId, Dict_int_int_Data change)
        {
            CoroutineFactory.NewCoroutine(ChangeNameTitleCoroutine, characterId, change).MoveNext();
        }

        private IEnumerator ChangeNameTitleCoroutine(Coroutine co, ulong characterId, Dict_int_int_Data change)
        {
            var msg = ActivityServer.Instance.LogicAgent.SSSetFlag(characterId, change);
            yield return msg.SendAndWaitUntilDone(co);
        }
        public void FinalActivity()
        {
            if (ChickenManager.mDbData == null)
                return;
            for (int i = 0; i < ChickenManager.mDbData.Count; i++)
            {
                var tbReward = GetCheckenReward(ChickenManager.mDbData[i].Rank);
                if (tbReward != null)
                {
                    var tbMail = Table.GetMail(97);
                    if (tbMail == null)
                        return;
                    var reward = new Dictionary<int, int>();
                    for (int j = 0; j < tbReward.RankItemID.Length && j < tbReward.RankItemCount.Length; j++)
                    {
                        if(tbReward.RankItemID[j] > 0 && tbReward.RankItemCount[j] > 0)
                            reward.Add(tbReward.RankItemID[j], tbReward.RankItemCount[j]);
                    }
                    string content = string.Format(tbMail.Text, i);
                    var items = new Dict_int_int_Data();
                    items.Data.AddRange(reward);
                    CoroutineFactory.NewCoroutine(SendMailCoroutine, ChickenManager.mDbData[i].Guid, tbMail.Title, content, items).MoveNext();
                }
            }

        }
        public CheckenRewardRecord GetCheckenReward(int id)
        {
            CheckenRewardRecord tbAr = null;
            Table.ForeachCheckenReward(record =>
            {
                if (id == record.Id)
                {
                    tbAr = record;
                    return false;
                }
                return true;
            });
            return tbAr;
        }
        public void UnInit()
        {
            if (ChickenManager.SaveTrigger != null)
            {
                ActivityServerControl.Timer.DeleteTrigger(ChickenManager.SaveTrigger);
                ChickenManager.SaveTrigger = null;
            }
        }
        private IEnumerator SendMailCoroutine(Coroutine co,
                                              ulong id,
                                              string title,
                                              string content,
                                              Dict_int_int_Data items)
        {
            var msg = ActivityServer.Instance.LogicAgent.SendMailToCharacter(id, title, content, items, 0);
            yield return msg.SendAndWaitUntilDone(co);
        }
     

        public string GetDbName()
        {
            return DbKey;
        }

        private IEnumerator SaveDb(Coroutine coroutine)
        {
            if (ChickenManager.mDbData != null)
            {
                DBChickenRankData data = new DBChickenRankData();
                for (int i = 0; i < ChickenManager.mDbData.Count; i++)
                {
                    data.RankList.Add(ChickenManager.mDbData[i]);
                }

                PlayerLog.WriteLog((int)LogType.SaveCheckenData, "--------------------SaveChickenData--------------------s={0}", data);
                var ret = ActivityServer.Instance.DB.Set(coroutine, DataCategory.Chicken, GetDbName(), data);
                yield return ret;
            }
        }

        //读取数据
        private IEnumerator ReadDb(Coroutine coroutine)
        {
            var tasks = ActivityServer.Instance.DB.Get<DBChickenRankData>(coroutine, DataCategory.Chicken,
                GetDbName());
            yield return tasks;
            if (tasks.Data == null)
            {

                ChickenManager.InitByBase();
                yield break;
            }
            PlayerLog.WriteLog((int)LogType.GetCheckenData, "--------------------GetCheckenData--------------------s={0}");
            foreach (var lodedata in tasks.Data.RankList)
            {
                ChickenManager.mDbData.Add(lodedata);
            }
            ChickenManager.InitByBase();
        }
        #endregion

        public ChickenRankData ApplyChickenRank(ulong charId)
        {
            if (ChickenManager.mDbData == null)
                ChickenManager.mDbData = new List<DBChickenData>();
            ChickenRankData result = new ChickenRankData();
            for (int i = 0; i < ChickenManager.mDbData.Count; i++)
            {
                var db = new ChickenData();
                db.Score = ChickenManager.mDbData[i].Score;
                db.FightValue = ChickenManager.mDbData[i].FightValue;
                db.Guid = ChickenManager.mDbData[i].Guid;
                db.Level = ChickenManager.mDbData[i].Level;
                db.Name = ChickenManager.mDbData[i].Name;
                db.Profession = ChickenManager.mDbData[i].Profession;
                db.Rank = ChickenManager.mDbData[i].Rank;
                result.RankList.Add(db);
            }
            var idx = ChickenManager.mDbData.FindIndex(o => { return o.Guid == charId; });
            if (idx != -1)
            {
                result.MyRank = new ChickenData();
                result.MyRank.Score = ChickenManager.mDbData[idx].Score;
                result.MyRank.FightValue = ChickenManager.mDbData[idx].FightValue;
                result.MyRank.Guid = ChickenManager.mDbData[idx].Guid;
                result.MyRank.Level = ChickenManager.mDbData[idx].Level;
                result.MyRank.Name = ChickenManager.mDbData[idx].Name;
                result.MyRank.Profession = ChickenManager.mDbData[idx].Profession;
                result.MyRank.Rank = ChickenManager.mDbData[idx].Rank;
            }
            return result;
        }

        public void SaveChickenScore(ChickenRankData rankData)
        {
            if (ChickenManager.mDbData == null)
                ChickenManager.mDbData = new List<DBChickenData>();
            for (int i = 0; i < rankData.RankList.Count; i++)
            {
                var idx = ChickenManager.mDbData.FindIndex(o => { return o.Guid == rankData.RankList[i].Guid; });
                DBChickenData db = null;
                if (idx >= 0)
                {
                    db = ChickenManager.mDbData[idx];
                    db.Score += rankData.RankList[i].Score;
                    db.FightValue = rankData.RankList[i].FightValue;
                }
                if (db == null)
                {
                    db = new DBChickenData();
                    db.Score = rankData.RankList[i].Score;
                    db.FightValue = rankData.RankList[i].FightValue;
                    db.Guid = rankData.RankList[i].Guid;
                    db.Level = rankData.RankList[i].Level;
                    db.Name = rankData.RankList[i].Name;
                    db.Profession = rankData.RankList[i].Profession;
                    db.Rank = rankData.RankList[i].Rank;
                    ChickenManager.mDbData.Add(db);
                }
            }
            ChickenManager.mDbData.Sort((DBChickenData rankA, DBChickenData rankB) => { return rankA.Score > rankB.Score ? -1 : 1; });
            var rank = 1;
            foreach (var unit in ChickenManager.mDbData)
            {
                unit.Rank = rank++;
            }
            ChickenManager.bDirty = true;
        }

        private void WaitNextBoss()
        {
            var tarTime = ChickenManager.TargetTimes[ChickenManager.TimeIdx];
            ChickenManager.OpenTrigger = ActivityServerControl.Timer.CreateTrigger(tarTime, BossWillStart);

            tarTime = tarTime.AddMinutes(ChickenManager.OpenLastMin);
            ChickenManager.StartTrigger = ActivityServerControl.Timer.CreateTrigger(tarTime, BossStart);

            tarTime = tarTime.AddMinutes(ChickenManager.TimeLimitMin);
            ChickenManager.StopTrigger = ActivityServerControl.Timer.CreateTrigger(tarTime, BossStop);

            tarTime = tarTime.AddMinutes(2);
            ActivityServerControl.Timer.CreateTrigger(tarTime, BossNext);

            ChickenManager.TargetTimes[ChickenManager.TimeIdx] = ChickenManager.TargetTimes[ChickenManager.TimeIdx].AddDays(1);
            ChickenManager.TimeIdx = ++ChickenManager.TimeIdx % ChickenManager.TargetTimes.Count;
        }
        private void BossWillStart()
        {
            ChickenManager.state = eActivityState.WillStart;
        }

        private void BossStart()
        {
            ChickenManager.state = eActivityState.Start;
        }

        private void BossStop()
        {
            ChickenManager.state = eActivityState.WillEnd;
        }

        private void BossNext()
        {
            ChickenManager.state = eActivityState.WaitNext;
            WaitNextBoss();
        }
    }

    public class ChickenManager
    {
        public static List<DBChickenData> mDbData = new List<DBChickenData>();
        public static bool bDirty = false;
        private static IChickenManager mImpl;
        public static Trigger SaveTrigger;
        public static object OpenTrigger; //活动开始进入
        public static object StartTrigger; //活动正式开始
        public static object StopTrigger; //活动时间结束
        public static List<DateTime> TargetTimes = new List<DateTime>();
        public static int TimeIdx;

        public static int OpenLastMin;
        public static float TimeLimitMin;
        public static eActivityState state;
        static ChickenManager()
        {
            ActivityServer.Instance.UpdateManager.InitStaticImpl(typeof(ChickenManager),
                typeof(ChickenManagerDefaultImpl),
                o => { mImpl = (IChickenManager)o; });
        }
        #region 数据
        public static void InitByBase()
        {
            mImpl.InitByBase();
        }


        public static void Init()
        {
            mImpl.Init();
        }
        public static void UnInit()
        {
            mImpl.UnInit();
        }
        #endregion
        #region 逻辑

        public static ChickenRankData ApplyChickenRank(ulong charId)
        {
            return mImpl.ApplyChickenRank(charId);
        }
        public static void SaveChickenScore(ChickenRankData rankData)
        {
            mImpl.SaveChickenScore(rankData);
        }
        
        #endregion

    }

}
