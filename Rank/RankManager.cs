#region using

using System;
using System.Collections;
using System.Collections.Generic;
using DataContract;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Rank
{
    public interface IRankManager
    {
        IEnumerator AddRanking(Coroutine coroutine,
                               RankManager _this,
                               int serverId,
                               int nRankType,
                               string name,
                               int count,
                               bool wholeServer);

        int ChangeData(RankManager _this, int serverId, string name, ulong Id, long value, string characterName);
        void ChangePlayerName(RankManager _this, int serverId, string name, ulong Id, string characterName);
        void CheckResetFirstTime(RankManager _this);
        IEnumerator FlushAll(Coroutine coroutine, RankManager _this);
        int GetPlayerRank(RankManager _this, int nRankType, ulong Id);
        int GetPlayerRank(RankManager _this, string name, ulong Id);
        long GetPlayerRankValue(RankManager _this, int nRankType, ulong Id);
        long GetPlayerRankValue(RankManager _this, string name, ulong Id);
        List<DBRank_One> GetRankData(RankManager _this, int nRankType, int min_rank, int max_rank);
        List<DBRank_One> GetRankData(RankManager _this, string name, int min_rank, int max_rank);
        Ranking GetRanking(RankManager _this, int nRankType);
        IEnumerator Init(Coroutine coroutine, RankManager _this, int serverId, int rankId);
        void AddClearTrigger(RankManager _this, RankType rankType);
        int NowRankCount(RankManager _this, string name);
        void ResetFirstTime(RankManager _this);
        void ShowLog(RankManager _this);
        List<DBRank_One> GetFightRankListBackUp(RankManager _this, int serverId);
        void FightRankListBackUp(RankManager _this);
    }

    public class RankManagerDefaultImpl : IRankManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //判断时间是否需要重置时间
        public void CheckResetFirstTime(RankManager _this)
        {
            var nowTime = DateTime.Now;
            if (RankManager.FirstTime > nowTime)
            {
                return;
            }
            ResetFirstTime(_this);
        }

        //重置时间
        public void ResetFirstTime(RankManager _this)
        {
            var nowTime = DateTime.Now;
#if DEBUG
            nowTime = nowTime.AddMinutes(1);
#else
            nowTime = nowTime.AddMinutes(59 - nowTime.Minute);
#endif
            RankManager.FirstTime = nowTime.AddSeconds(60 - nowTime.Second).AddSeconds(2); //添加2秒 作为网络延时的误差
        }

        //初始化排行榜
        public IEnumerator Init(Coroutine co, RankManager _this, int serverId, int rankId)
        {
            CheckResetFirstTime(_this);
            _this.ServerId = serverId;
            _this.RankId = rankId;

            var co0 = CoroutineFactory.NewSubroutine(AddRanking, co, _this, serverId, (int) RankType.FightValue,
                ServerRankManager.SwordRank, 500, false);
            if (co0.MoveNext())
            {
                yield return co0;
            }
            var co1 = CoroutineFactory.NewSubroutine(AddRanking, co, _this, serverId, (int) RankType.Level,
                ServerRankManager.LevelRank, 500, false);
            if (co1.MoveNext())
            {
                yield return co1;
            }
            var co2 = CoroutineFactory.NewSubroutine(AddRanking, co, _this, serverId, (int) RankType.Money,
                ServerRankManager.MoneyRank, 500, false);
            if (co2.MoveNext())
            {
                yield return co2;
            }
            var co3 = CoroutineFactory.NewSubroutine(AddRanking, co, _this, serverId, (int) RankType.Arena,
                ServerRankManager.P1vp1Rank, 1000, false);
            if (co3.MoveNext())
            {
                yield return co3;
            }
            var co4 = CoroutineFactory.NewSubroutine(AddRanking, co, _this, serverId, (int) RankType.CityLevel,
                ServerRankManager.HomeRank, 500, false);
            if (co4.MoveNext())
            {
                yield return co4;
            }
            var co5 = CoroutineFactory.NewSubroutine(AddRanking, co, _this, serverId, (int) RankType.WingsFight,
                ServerRankManager.WingRank, 500, false);
            if (co5.MoveNext())
            {
                yield return co5;
            }
            var co6 = CoroutineFactory.NewSubroutine(AddRanking, co, _this, serverId, (int) RankType.PetFight,
                ServerRankManager.PetRank, 500, false);
            if (co6.MoveNext())
            {
                yield return co6;
            }

			var co7 = CoroutineFactory.NewSubroutine(AddRanking, co, _this, serverId, (int)RankType.RechargeTotal,
                ServerRankManager.TotalChargeDimaondRank, 500, false);
			if (co7.MoveNext())
			{
				yield return co7;
			}

            var co8 = CoroutineFactory.NewSubroutine(AddRanking, co, _this, serverId, (int)RankType.Mount,
                ServerRankManager.MountRank, 500, false);
            if (co8.MoveNext())
            {
                yield return co8;
            }
        }

        //存储排行榜
        public IEnumerator FlushAll(Coroutine coroutine, RankManager _this)
        {
            foreach (var ranking in _this.rank)
            {
                var co = CoroutineFactory.NewSubroutine(ranking.Value.FlushAll, coroutine);
                if (co.MoveNext())
                {
                    yield return co;
                }
            }
        }

        //添加排行榜
        public IEnumerator AddRanking(Coroutine coroutine,
                                      RankManager _this,
                                      int serverId,
                                      int nRankType,
                                      string name,
                                      int count,
                                      bool totalServer)
        {
            Ranking temprank;
            int rankType;
            if (!_this.rank_name.TryGetValue(name, out rankType))
                //if (!rank_name.ContainsKey(name))
            {
                _this.rankType2Name[nRankType] = name;
                temprank = new Ranking(totalServer);
                var co = CoroutineFactory.NewSubroutine(temprank.Init, coroutine, _this.RankId, nRankType, serverId,
                    name);
                if (co.MoveNext())
                {
                    yield return co;
                }
                //temprank.Init(RankId, nRankType, serverId, name);
                temprank.MaxRankMemberCount = count;
                _this.rank_name[name] = nRankType;
                _this.rank[nRankType] = temprank;
                //下次刷新数据的时间
                //var nowTime = DateTime.Now;
                //nowTime = nowTime.AddMinutes(1);
                //nowTime = nowTime.AddSeconds(60 - nowTime.Second);
                //int diffSecond = (serverId % 6) * 10 + nRankType;
                //nowTime = nowTime.AddSeconds(diffSecond);//压力均分
                RankManager.FirstTime = RankManager.FirstTime.AddSeconds(0.05);
                PlayerLog.WriteLog((int) LogType.SaveRanking,
                    "--------------------CreateTriggerSave--------------------s={0},n={1},t={2},t={3}", serverId, name,
                    nRankType, RankManager.FirstTime);

#if DEBUG
                RankServerControl.Timer.CreateTrigger(RankManager.FirstTime, temprank.FlushAll, 60000); //每1分钟存储一次
#else
                RankServerControl.Timer.CreateTrigger(RankManager.FirstTime, temprank.FlushAll, 60000 * 15);//每30分钟存储一次
#endif
                yield break;
            }
            temprank = _this.rank[rankType];
            //temprank.Init(RankId, nRankType,serverId, name);
            var co2 = CoroutineFactory.NewSubroutine(temprank.Init, coroutine, _this.RankId, nRankType, serverId, name);
            if (co2.MoveNext())
            {
                yield return co2;
            }
        }

        public void AddClearTrigger(RankManager _this, RankType rankType)
        {
            switch (rankType)
            {
                case RankType.DailyGift:
                {
                    var triggerTime = DateTime.Today.AddDays(1);
                    const int autoInterval = 24 * 60 * 60 * 1000; // 天
                    _this.rank[(int) rankType].CreateClearTrigger(triggerTime, autoInterval);
                }
                    break;
                case RankType.WeeklyGift:
                {
                    var dayOfWeek = Convert.ToInt32(DateTime.Now.DayOfWeek.ToString("d"));
                    if (dayOfWeek == 0) // 周日
                        dayOfWeek = 7;
                    var startWeek = DateTime.Today.AddDays(1 - dayOfWeek);
                    var triggerTime = startWeek.AddDays(7);
                    const int autoInterval = 7 * 24 * 60 * 60 * 1000; // 周
                    _this.rank[(int)rankType].CreateClearTrigger(triggerTime, autoInterval);
                }
                    break;
            }
        }

        //获取某个排行榜
        public Ranking GetRanking(RankManager _this, int nRankType)
        {
            Ranking thisRank;
            if (_this.rank.TryGetValue(nRankType, out thisRank))
            {
                return thisRank;
            }
            return null;
        }

        //添加或更新数据
        public int ChangeData(RankManager _this, int serverId, string name, ulong Id, long value, string characterName)
        {
            int rankType;
            if (!_this.rank_name.TryGetValue(name, out rankType))
                //if (!rank_name.ContainsKey(name))
            {
                return -2;
            }
            return _this.rank[rankType].AddRanking(serverId, Id, value, characterName);
        }
        public void ChangePlayerName(RankManager _this, int serverId, string name, ulong Id, string characterName)
        {
            int rankType;
            if (!_this.rank_name.TryGetValue(name, out rankType))
            //if (!rank_name.ContainsKey(name))
            {
                return;
            }
            DBRank_One dbPlayer;
            if( _this.rank[rankType].DBRankCache.TryGetValue(Id,out dbPlayer)){
                dbPlayer.Name=characterName;
                _this.rank[rankType].Dirty=true;
            }
        }
        //查询自己的数据库排名（不同于DBRankCache）
        //public int GetPlayerDBRank(int nRankType, ulong Id)
        //{
        //    if (!rankType2Name.ContainsKey(nRankType))
        //        return -1;
        //    var dbrank = Data.DB.GetData<DBRank_List>(rankType2Name[nRankType]);
        //    if (dbrank == null)
        //    {
        //        return 0;
        //    }

        //    for (int i = 0; i < dbrank.mData.Count; i++)
        //    {
        //        if (dbrank.mData[i].Id == Id)
        //            return dbrank.mData[i].Rank;
        //    }
        //    return 0;
        //}


        //查询自己的排名
        public int GetPlayerRank(RankManager _this, int nRankType, ulong Id)
        {
            Ranking thisRank;
            if (!_this.rank.TryGetValue(nRankType, out thisRank))
            {
                //该玩家不在此排行榜
                return -1;
            }
            return thisRank.GetPlayerLadder(Id);
        }

        //查询自己的排名
        public int GetPlayerRank(RankManager _this, string name, ulong Id)
        {
            int rankId;
            if (!_this.rank_name.TryGetValue(name, out rankId))
            {
                //排行榜不存在
                return -2;
            }
            return GetPlayerRank(_this, rankId, Id);
        }

        //查询自己的数据库排名（不同于DBRankCache）
        //public long GetPlayerDBRankValue(int nRankType, ulong Id)
        //{
        //    if (!rankType2Name.ContainsKey(nRankType))
        //        return -1;
        //    var dbrank = Data.DB.GetData<DBRank_List>(rankType2Name[nRankType]);
        //    if (dbrank == null)
        //    {
        //        return 0;
        //    }
        //    for (int i = 0; i < dbrank.mData.Count; i++)
        //    {
        //        if (dbrank.mData[i].Id == Id)
        //            return dbrank.mData[i].Value;
        //    }
        //    return 0;
        //}
        //查询自己的排行榜值
        public long GetPlayerRankValue(RankManager _this, int nRankType, ulong Id)
        {
            Ranking thisRank;
            if (!_this.rank.TryGetValue(nRankType, out thisRank))
            {
                //该玩家不在此排行榜
                return -1;
            }
            return thisRank.DBRankCache[Id].Value;
        }

        //查询自己的排行榜值
        public long GetPlayerRankValue(RankManager _this, string name, ulong Id)
        {
            int rankId;
            if (!_this.rank_name.TryGetValue(name, out rankId))
            {
                //排行榜不存在
                return -2;
            }
            return GetPlayerRankValue(_this, rankId, Id);
        }

        //查询排行榜的总人数
        public int NowRankCount(RankManager _this, string name)
        {
            int rankType;
            if (!_this.rank_name.TryGetValue(name, out rankType))
                //if (!rank_name.ContainsKey(name))
            {
                return -2;
            }
            return _this.rank[rankType].RankUUIDList.Count;
        }

        //根据类型查询排行榜数据
        public List<DBRank_One> GetRankData(RankManager _this, int nRankType, int min_rank, int max_rank)
        {
            string rName;
            if (_this.rankType2Name.TryGetValue(nRankType, out rName))
            {
                return GetRankData(_this, rName, min_rank, max_rank);
            }
            return null;
        }

        //查询排行榜的某个名次
        public List<DBRank_One> GetRankData(RankManager _this, string name, int min_rank, int max_rank)
        {
            var templist = new List<DBRank_One>();
            int rankType;
            if (!_this.rank_name.TryGetValue(name, out rankType))
                //if (!rank_name.ContainsKey(name))
            {
                return templist;
            }
            var tempranking = _this.rank[rankType];
            var count = tempranking.RankUUIDList.Count;
            if (min_rank < 1)
            {
                min_rank = 1;
            }
            if (max_rank > count)
            {
                max_rank = count;
            }
            for (var i = min_rank; i <= max_rank; i++)
            {
                var temp = tempranking.GetRankOneByIndex(i - 1);
                long oldValue;
                if (tempranking.SaveOldValueByUUID.TryGetValue(temp.Guid, out oldValue))
                {
                    var t = new DBRank_One();
                    t.Guid = temp.Guid;
                    t.Name = temp.Name;
                    t.Value = oldValue;
                    t.Rank = temp.Rank;
                    t.OldRank = temp.OldRank;
                    t.MaxRank = temp.MaxRank;
                    t.FightPoint = temp.FightPoint;
                    t.ServerId = temp.ServerId;
                    templist.Add(t);
                }
                else
                {
                    templist.Add(temp);
                }
            }
            return templist;
        }

        public void FightRankListBackUp(RankManager _this)
        {
            _this.FightRankServerToRnak.Clear();
            

            DataTable.Table.ForeachServerName(record =>
            {
                int serverId = record.LogicID;
                if (record.LogicID == record.Id)
                {
                    var data = ServerRankManager.GetRankDataByServerId(serverId, 0, 1, 20);

                    if (null != data) _this.FightRankServerToRnak.Add(serverId, data);                    
                }
                return true;
            });
        }

        public List<DBRank_One> GetFightRankListBackUp(RankManager _this, int serverId)
        {
            List<DBRank_One> dataList = new List<DBRank_One>();
            if (_this.FightRankServerToRnak.TryGetValue(serverId,out dataList))
            {
                return dataList;
            }

            return null;
        }
        #region 状态日志

        public void ShowLog(RankManager _this)
        {
            Logger.Info("    RankManager RankId={0},Ranks={1}", _this.RankId, _this.rank.Count);
            if (_this.rank.Count > 0)
            {
                Logger.Info("    {");
                foreach (var ranking in _this.rank)
                {
                    ranking.Value.ShowLog();
                }
                Logger.Info("    }");
            }
        }

        #endregion
    }

    //排行榜实例类
    public class RankManager
    {
        public static DateTime FirstTime;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static IRankManager mImpl;

        static RankManager()
        {
            RankServer.Instance.UpdateManager.InitStaticImpl(typeof (RankManager), typeof (RankManagerDefaultImpl),
                o => { mImpl = (IRankManager) o; });
        }

        public Dictionary<int, Ranking> rank = new Dictionary<int, Ranking>(); //key=RankId value = ranking   id到具体内容
        public Dictionary<string, int> rank_name = new Dictionary<string, int>(); //key=Name value=rankid  名字到排行id的转换
        public int RankId;
        public Dictionary<int, string> rankType2Name = new Dictionary<int, string>(); //key=RankId value=Name  id到名字的转换
        public Dictionary<int, List<DBRank_One>> FightRankServerToRnak = new Dictionary<int, List<DBRank_One>>(); // 每天指定时间 备份的排行榜数据
        public int ServerId;
        //添加排行榜
        public IEnumerator AddRanking(Coroutine coroutine, int serverId, int nRankType, string name, int count, bool wholeServer)
        {
            return mImpl.AddRanking(coroutine, this, serverId, nRankType, name, count, wholeServer);
        }

        //添加或更新数据
        public int ChangeData(int serverId, string name, ulong Id, long value, string characterName)
        {
            return mImpl.ChangeData(this, serverId, name, Id, value, characterName);
        }
        public void ChangePlayerName(int serverId, string name, ulong Id, string characterName)
        {
            mImpl.ChangePlayerName(this, serverId, name, Id, characterName);
        }
        //判断时间是否需要重置时间
        public void CheckResetFirstTime()
        {
            mImpl.CheckResetFirstTime(this);
        }

        //存储排行榜
        public IEnumerator FlushAll(Coroutine coroutine)
        {
            return mImpl.FlushAll(coroutine, this);
        }

        //查询自己的数据库排名（不同于DBRankCache）
        //public int GetPlayerDBRank(int nRankType, ulong Id)
        //{
        //    if (!rankType2Name.ContainsKey(nRankType))
        //        return -1;
        //    var dbrank = Data.DB.GetData<DBRank_List>(rankType2Name[nRankType]);
        //    if (dbrank == null)
        //    {
        //        return 0;
        //    }

        //    for (int i = 0; i < dbrank.mData.Count; i++)
        //    {
        //        if (dbrank.mData[i].Id == Id)
        //            return dbrank.mData[i].Rank;
        //    }
        //    return 0;
        //}


        //查询自己的排名
        public int GetPlayerRank(int nRankType, ulong id)
        {
            return mImpl.GetPlayerRank(this, nRankType, id);
        }

        //查询自己的排名
        public int GetPlayerRank(string name, ulong id)
        {
            return mImpl.GetPlayerRank(this, name, id);
        }

        //查询自己的数据库排名（不同于DBRankCache）
        //public long GetPlayerDBRankValue(int nRankType, ulong Id)
        //{
        //    if (!rankType2Name.ContainsKey(nRankType))
        //        return -1;
        //    var dbrank = Data.DB.GetData<DBRank_List>(rankType2Name[nRankType]);
        //    if (dbrank == null)
        //    {
        //        return 0;
        //    }
        //    for (int i = 0; i < dbrank.mData.Count; i++)
        //    {
        //        if (dbrank.mData[i].Id == Id)
        //            return dbrank.mData[i].Value;
        //    }
        //    return 0;
        //}
        //查询自己的排行榜值
        public long GetPlayerRankValue(int nRankType, ulong id)
        {
            return mImpl.GetPlayerRankValue(this, nRankType, id);
        }

        //查询自己的排行榜值
        public long GetPlayerRankValue(string name, ulong id)
        {
            return mImpl.GetPlayerRankValue(this, name, id);
        }

        //根据类型查询排行榜数据
        public List<DBRank_One> GetRankData(int nRankType, int min_rank, int max_rank)
        {
            return mImpl.GetRankData(this, nRankType, min_rank, max_rank);
        }

        //查询排行榜的某个名次
        public List<DBRank_One> GetRankData(string name, int min_rank, int max_rank)
        {
            return mImpl.GetRankData(this, name, min_rank, max_rank);
        }

        //获取某个排行榜
        public Ranking GetRanking(int nRankType)
        {
            return mImpl.GetRanking(this, nRankType);
        }

        //初始化排行榜
        public IEnumerator Init(Coroutine coroutine, int serverId, int rankId)
        {
            return mImpl.Init(coroutine, this, serverId, rankId);
        }

        public void AddClearTrigger(RankType rankType)
        {
            mImpl.AddClearTrigger(this, rankType);
        }

        //查询排行榜的总人数
        public int NowRankCount(string name)
        {
            return mImpl.NowRankCount(this, name);
        }

        //重置时间
        public void ResetFirstTime()
        {
            mImpl.ResetFirstTime(this);
        }

        public void FightRankListBackUp()
        {
            mImpl.FightRankListBackUp(this);
        }

        public List<DBRank_One> GetFightRankListBackUp(int serverId)
        {
            return mImpl.GetFightRankListBackUp(this,serverId);
        }
        #region 状态日志

        public void ShowLog()
        {
            mImpl.ShowLog(this);
        }

        #endregion
    }
}