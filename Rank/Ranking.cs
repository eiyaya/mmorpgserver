#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Database;
using DataContract;
using DataTable;
using Scorpion;
using NLog;
using ProtoBuf;
using Shared;

#endregion

namespace Rank
{
    public interface IRanking
    {
        int AddRanking(Ranking _this, int serverId, ulong Id, long value, string name, int fightPoint = -1);
        IEnumerator FlushAll(Coroutine coroutine, Ranking _this);
        void FlushAll(Ranking _this);
        IEnumerator Get(Coroutine coroutine, Ranking _this, int serverId, string DBkeyName);
        string GetDbName(Ranking _this, int serverId, string keyName);
        DBRank_One GetPlayerData(Ranking _this, ulong Guid);
        int GetPlayerLadder(Ranking _this, ulong Guid);
        int GetRankCount(Ranking _this);
        int GetRankListCount(Ranking _this);
        DBRank_One GetRankOneByIndex(Ranking _this, int nIndex);
        IEnumerator Init(Coroutine coroutine, Ranking _this, int rankId, int rankType, int serverId, string dbname);
        void Clear(Ranking _this);
        bool RemoveCharacter(Ranking _this, ulong guid);
        IEnumerator Save(Coroutine coroutine, Ranking _this, string dbKeyName);

        IEnumerator SaveOne<T>(Coroutine coroutine, Ranking _this, DataCategory cat, string key, T v)
            where T : IExtensible;

        void ShowLog(Ranking _this);
        void ShowRank(Ranking _this, ulong rankManager = 20000);
        List<KeyValuePair<ulong, DBRank_One>> Sort(Ranking _this, bool IsMustDo = false);
        bool SwapIndex(Ranking _this, int index1, int index2);
        bool SwapIndex(Ranking _this, DBRank_One db1, DBRank_One db2);
        void CreateClearTrigger(Ranking _this, DateTime triggerTime, int autoInterval);
    }

    public class RankingDefaultImpl : IRanking
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //数据变更
        private void ChangeValue(Ranking _this, DBRank_One one, long value)
        {
            one.Value = value;
            if (value < 0)
            {
                Logger.Error("Ranking.ChangeValue: Id={0},value={1}", one.Guid, value);
            }
        }

        //从排行榜移除某个玩家
        private void RemoveOne(Ranking _this, DBRank_One one)
        {
            _this.DBRankCache.Remove(one.Guid);
            DBRank_List tempDb;
            if (_this.mDBData.TryGetValue(one.ServerId, out tempDb))
            {
                tempDb.mData.Remove(one);
            }
        }

        //重新排序
        private void SortAndSave(Ranking _this)
        {
            if (_this.Dirty == false)
            {
                return;
            }
            var result = Sort(_this);
            CoroutineFactory.NewCoroutine(Save, _this, _this.DBKeyName).MoveNext();

            // 后台统计
            try
            {
                if (result != null)
                {
                    var saveCount = Math.Min(20, result.Count);
                    for (int i = result.Count - 1; i >= result.Count - saveCount && saveCount>0; i--)
                    {
                        string v = string.Format("Rank_info#{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                            result[i].Value.ServerId,
                            0,
                            result[i].Value.Name,
                            _this.RankType,
                            result[i].Value.Value,
                            result[i].Value.Rank,
                            DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")); // 时间
                        PlayerLog.Kafka(v);
                    }  
                }
            }
            catch (Exception)
            {
            }
        }

        //DB名称获取
        public string GetDbName(Ranking _this, int serverId, string keyName)
        {
            var key = serverId;
            if (_this.IsTotalServer)
            {
                key = -1;
            }
            return string.Format("Rank_{0}_{1}", key, keyName);
        }

        //初始化
        public IEnumerator Init(Coroutine coroutine,
                                Ranking _this,
                                int rankId,
                                int rankType,
                                int serverId,
                                string dbname)
        {
            _this.RankType = rankType;
            _this.DBKeyName = dbname;
            _this.serverList.Add(serverId);
            _this.RankId = rankId;
            var co = CoroutineFactory.NewSubroutine(Get, coroutine, _this, serverId, _this.DBKeyName);
            if (co.MoveNext())
            {
                yield return co;
            }
        }

        public void Clear(Ranking _this)
        {
            _this.DBRankCache.Clear();
            _this.mDBData.Clear();
            _this.RankUUIDList.Clear();
            
            _this.Dirty = true;
            FlushAll(_this);
        }

        //储存
        public IEnumerator FlushAll(Coroutine coroutine, Ranking _this)
        {
            Sort(_this, true);
            //if (_this.RankType == 3)
            //{
            //    ShowRank(_this, 40000);
            //}
            var co = CoroutineFactory.NewSubroutine(Save, coroutine, _this, _this.DBKeyName);
            if (co.MoveNext())
            {
                yield return co;
            }
        }

        public void FlushAll(Ranking _this)
        {
            SortAndSave(_this);
        }

        public IEnumerator Save(Coroutine coroutine, Ranking _this, string dbKeyName)
        {
            //LogSystem.WriteLog(enmLOG_TYPE.LOG_TYPE_DB, "Save {0}", DBKeyName);
            if (_this.mDBData != null)
            {
                foreach (var serverData in _this.mDBData)
                {
                    var co = CoroutineFactory.NewSubroutine(SaveOne, coroutine, _this, DataCategory.Rank,
                        GetDbName(_this, serverData.Key, dbKeyName), serverData.Value);
                    if (co.MoveNext())
                    {
                        yield return co;
                    }
                }
            }
        }

        public IEnumerator SaveOne<T>(Coroutine coroutine, Ranking _this, DataCategory cat, string key, T v)
            where T : IExtensible
        {
            PlayerLog.WriteLog((int) LogType.SaveRanking, "--------------------RankSave--------------------{0}", key);
            var ret = RankServer.Instance.DB.Set(coroutine, cat, key, v);
            yield return ret;
        }

        public IEnumerator Get(Coroutine coroutine, Ranking _this, int serverId, string DBkeyName)
        {
            PlayerLog.WriteLog((int) LogType.GetRanking, "--------------------rankGet--------------------{0}",
                GetDbName(_this, serverId, _this.DBKeyName));
            var tasks = RankServer.Instance.DB.Get<DBRank_List>(coroutine, DataCategory.Rank,
                GetDbName(_this, serverId, _this.DBKeyName));
            yield return tasks;
            if (tasks.Data == null)
            {
                _this.mDBData[serverId] = new DBRank_List();
                yield break;
            }
            _this.mDBData[serverId] = tasks.Data;
            foreach (var dbRankOne in tasks.Data.mData)
            {
                _this.DBRankCache[dbRankOne.Guid] = dbRankOne;
                //if (StaticData.IsRobot(dbRankOne.Guid))
                //{
                //    dbRankOne.Name =Table.GetJJCRoot((int)dbRankOne.Guid).Name;
                //}
                //rank_old_value[dbRankOne.Id] = dbRankOne.Value; //之后排序的话这里就不用加了
                //RankUUIDIndex[dbRankOne.Rank] = dbRankOne.Guid;
                _this.RankUUIDList.Add(dbRankOne.Guid);
                if (_this.RankType == 3)
                {
                    dbRankOne.Value = 1000000/dbRankOne.Rank;
                }
            }
            _this.Dirty = true;
            Sort(_this);
        }

        //尝试添加新记录
        public int AddRanking(Ranking _this, int serverId, ulong Id, long value, string name, int fightPoint = -1)
        {
            //PlayerLog.WriteLog((int)LogType.AddRanking, "AddRanking s={0},id={1},v={2},n={3}", serverId, Id, value,name);
            DBRank_One dbRankOnetemp;
            if (_this.DBRankCache.TryGetValue(Id, out dbRankOnetemp))
            {
                if (dbRankOnetemp.Value == value)
                {
                    return dbRankOnetemp.Rank;
                }
                _this.Dirty = true;
                if (!dbRankOnetemp.Name.Equals(name))
                {
                    dbRankOnetemp.Name = name;
                }
                ChangeValue(_this, dbRankOnetemp, value);
                return dbRankOnetemp.Rank;
            }
            //新数据
            var new_rank = GetRankCount(_this) + 1;
            dbRankOnetemp = new DBRank_One
            {
                Guid = Id,
                Rank = new_rank,
                Value = value,
                OldRank = -1,
                ServerId = serverId,
                Name = name,
                MaxRank = -1,
                FightPoint = fightPoint
            };
            _this.DBRankCache[Id] = dbRankOnetemp;

            var key = serverId;
            if (_this.IsTotalServer)
            {
                key = -1;
            }

            DBRank_List tempDb;
            if (_this.mDBData.TryGetValue(key, out tempDb))
            {
                tempDb.mData.Add(dbRankOnetemp);
            }
            _this.Dirty = true;
            return dbRankOnetemp.Rank;
            //Sort();
            ////排序后玩家可能不存在
            //if (DBRankCache.TryGetValue(Id, out dbRankOnetemp))
            //{
            //    return dbRankOnetemp.Rank;
            //}
            //return -1;
        }

        //排序
        public List<KeyValuePair<ulong, DBRank_One>> Sort(Ranking _this, bool IsMustDo = false)
        {
            if (!IsMustDo)
            {
                if (_this.RankType == 3)
                {
                    _this.Dirty = false;
                    return null;
                }
                if (!_this.Dirty)
                {
                    return null;
                }
            }
            var sortArray = _this.DBRankCache.OrderBy(item => item.Value.Value).ToList();
            var count = sortArray.Count();
            var nowCount = count;
            if (_this.MaxRankMemberCount < count)
            {
                //移除多余的
                nowCount = _this.MaxRankMemberCount;
                var delCount = count - _this.MaxRankMemberCount;
                for (var i = 0; i < delCount; ++i)
                {
                    RemoveOne(_this, sortArray[0].Value);
                    sortArray.Remove(sortArray[0]);
                    count--;
                }
            }
            var listCout = _this.RankUUIDList.Count;
            if (listCout > nowCount)
            {
                for (var i = nowCount; i < listCout; i++)
                {
                    _this.RankUUIDList.RemoveAt(nowCount);
                }
            }
            else
            {
                for (var i = listCout; i < nowCount; i++)
                {
                    _this.RankUUIDList.Add(0);
                }
            }
            //RankUUIDIndex.Clear();
            _this.SaveOldValueByUUID.Clear();
            foreach (var rankOne in sortArray)
            {
                var rank = _this.DBRankCache[rankOne.Value.Guid];
                _this.RankUUIDList[count - 1] = rankOne.Value.Guid;
                //RankUUIDIndex[count] = rankOne.Value.Guid;
                rank.OldRank = rankOne.Value.Rank;
                rank.Rank = count;
                _this.SaveOldValueByUUID[rankOne.Key] = rankOne.Value.Value;
                count--;
                if (_this.RankType == 3)
                {
                    rank.Value = 1000000/rank.Rank;
                }
            }
            _this.Dirty = false;

            return sortArray;
        }

        //获取第几名的人
        public DBRank_One GetRankOneByIndex(Ranking _this, int nIndex)
        {
            if (nIndex < 0 || nIndex >= _this.RankUUIDList.Count)
            {
                Logger.Warn("GetRankOneByIndex index={0},RankUUIDList.Count={1}", nIndex, _this.RankUUIDList.Count);
                return null;
            }
            var guid = _this.RankUUIDList[nIndex];
            var one = _this.DBRankCache.GetValue(guid);
            if (one == null)
            {
                return one;
            }
            if (one.Rank != nIndex + 1)
            {
                Logger.Warn("GetRankOneByIndex Guid={2},Rank={0},nIndex={1}", one.Rank, nIndex, guid);
            }
            return one;
        }

        //获取某玩家的DB数据
        public DBRank_One GetPlayerData(Ranking _this, ulong Guid)
        {
            DBRank_One one;
            if (_this.DBRankCache.TryGetValue(Guid, out one))
            {
                return one;
            }
            return null;
        }

        //获取某玩家的名次
        public int GetPlayerLadder(Ranking _this, ulong Guid)
        {
            DBRank_One one;
            if (_this.DBRankCache.TryGetValue(Guid, out one))
            {
                return one.Rank;
            }
            return -1;
        }

        //交换两个玩家的名次
        public bool SwapIndex(Ranking _this, int index1, int index2)
        {
            //if (RankUUIDList.Count <= index1)
            //{
            //    return false;
            //}
            if (_this.RankUUIDList.Count <= index2)
            {
                return false;
            }
            var db1 = GetRankOneByIndex(_this, index1);
            var db2 = GetRankOneByIndex(_this, index2);
            db1.OldRank = db1.Rank;
            db2.OldRank = db2.Rank;
            _this.RankUUIDList[index1] = db2.Guid;
            _this.RankUUIDList[index2] = db1.Guid;
            if (index1 != db1.Rank - 1)
            {
                Logger.Warn("SwapIndex index1 index={0},rank={1}", index1, db1.Rank);
            }
            if (index2 != db2.Rank - 1)
            {
                Logger.Warn("SwapIndex index2 index={0},rank={1}", index2, db2.Rank);
            }
            var tempRank = db1.Rank;
            db1.Rank = db2.Rank;
            var isNewMax = false;
            if (db1.Rank < db1.MaxRank || db1.MaxRank == -1)
            {
                db1.MaxRank = db1.Rank;
                isNewMax = true;
            }
            db2.Rank = tempRank;
            _this.Dirty = true;
            return isNewMax;
        }

        //交换两个玩家的名次
        public bool SwapIndex(Ranking _this, DBRank_One db1, DBRank_One db2)
        {
            //ShowRank(_this, 50000);
            db1.OldRank = db1.Rank;
            db2.OldRank = db2.Rank;
            var index1 = db1.Rank - 1;
            var index2 = db2.Rank - 1;
            if (!(index1 >= _this.RankUUIDList.Count))
            {
                _this.RankUUIDList[index1] = db2.Guid;
            }
            if (!(index2 >= _this.RankUUIDList.Count))
            {
                _this.RankUUIDList[index2] = db1.Guid;
            }
            //if (index1 != db1.Rank - 1)
            //{
            //    Logger.Warn("SwapIndex index1 index={0},rank={1}", index1, db1.Rank);
            //}
            //if (index2 != db2.Rank - 1)
            //{
            //    Logger.Warn("SwapIndex index2 index={0},rank={1}", index2, db2.Rank);
            //}
            var tempRank = db1.Rank;
            db1.Rank = db2.Rank;
            var isNewMax = false;
            if (db1.Rank < db1.MaxRank || db1.MaxRank == -1)
            {
                db1.MaxRank = db1.Rank;
                isNewMax = true;
            }
            db2.Rank = tempRank;
            _this.Dirty = true;
            db1.Value = 1000000/db1.Rank;
            db2.Value = 1000000/db2.Rank;
            //ShowRank(_this, 60000);
            return isNewMax;
        }

        //JJC移除玩家(自动补足成原始的机器人）
        public bool RemoveCharacter(Ranking _this, ulong guid)
        {
            var d = GetPlayerData(_this, guid);
            if (d == null)
            {
                return false;
            }
            if (d.Rank < 0 || d.Rank > 1000)
            {
                return false;
            }
            var tbRobot = Table.GetJJCRoot(d.Rank);
            if (tbRobot == null)
            {
                return false;
            }
            _this.DBRankCache.Remove(guid);
            var newGuid = (ulong) d.Rank;
            _this.DBRankCache[newGuid] = d;
            d.Guid = newGuid;
            d.Name = tbRobot.Name;
            long oldValue;
            if (_this.SaveOldValueByUUID.TryGetValue(guid, out oldValue))
            {
                _this.SaveOldValueByUUID.Remove(guid);
                _this.SaveOldValueByUUID[newGuid] = oldValue;
            }
            if (_this.RankUUIDList[d.Rank - 1] == guid)
            {
                _this.RankUUIDList[d.Rank - 1] = newGuid;
            }
            //if (_this.RankUUIDList[d.Rank] == guid)
            //{
            //    _this.RankUUIDList[d.Rank] = newGuid;
            //}
            return true;
        }

        //获得排行榜人数
        public int GetRankCount(Ranking _this)
        {
            return _this.DBRankCache.Count;
        }

        //获得排行榜已排次序的数量
        public int GetRankListCount(Ranking _this)
        {
            return _this.RankUUIDList.Count;
        }

        #region 状态日志

        public void ShowLog(Ranking _this)
        {
            Logger.Info("        Ranking RankType={0} severCount={1} characterCount={2}", _this.RankType,
                _this.mDBData.Count, _this.RankUUIDList.Count);
            if (_this.mDBData.Count > 0)
            {
                Logger.Info("        {");
                foreach (var db in _this.mDBData)
                {
                    Logger.Info("            Ranking ServerId={0} ServerCount={1}", db.Key, db.Value.mData.Count);
                }
                Logger.Info("        }");
            }
        }

        public void ShowRank(Ranking _this, ulong rankManager = 20000)
        {
            var rCount = GetRankCount(_this);
            var rlCount = GetRankListCount(_this);
            var logid = rankManager + (ulong) _this.RankId*100 + (ulong) _this.RankType;
            if (rlCount > _this.MaxRankMemberCount)
            {
                PlayerLog.WriteLog(logid, "---------------r={0},t={1},c={2},lc={3}--------------", _this.RankId,
                    _this.RankType, rCount, rlCount);
            }
            else
            {
                PlayerLog.WriteLog(logid, "---------------r={0},t={1},c={2},lc={3}--------------", _this.RankId,
                    _this.RankType, rCount, rlCount);
            }

            for (var i = 0; i < rlCount; i++)
            {
                var one = GetRankOneByIndex(_this, i);
                if (one == null)
                {
                    continue;
                }
                PlayerLog.WriteLog(logid, "i={0},n={1},r={2},v={3}", one.Guid, one.Name, one.Rank, one.Value);
            }
        }

        public void CreateClearTrigger(Ranking _this, DateTime triggerTime, int autoInterval)
        {
            RankServerControl.Timer.CreateTrigger(triggerTime, _this.Clear, autoInterval);
        }

        #endregion
    }

    public class Ranking
    {
        private static IRanking mImpl;

        static Ranking()
        {
            RankServer.Instance.UpdateManager.InitStaticImpl(typeof (Ranking), typeof (RankingDefaultImpl),
                o => { mImpl = (IRanking) o; });
        }

        public Ranking(bool totalServer)
        {
            IsTotalServer = totalServer;
        }

        public string DBKeyName;
        //public Dictionary<int, ulong> RankUUIDIndex = new Dictionary<int, ulong>();//名次的Map
        public Dictionary<ulong, DBRank_One> DBRankCache = new Dictionary<ulong, DBRank_One>(); //ID的数据库Map(玩家ID->DB)
        public bool Dirty;
        public int MaxRankMemberCount = 5000; //最大排行数量
        //private List<DBRank_List> mDBData=new List<DBRank_List>();
        public Dictionary<int, DBRank_List> mDBData = new Dictionary<int, DBRank_List>(); //serverId ->DBdata
        public bool IsTotalServer = false;
        public int RankId;
        public int RankType;
        public List<ulong> RankUUIDList = new List<ulong>(); //名次的List
        public Dictionary<ulong, long> SaveOldValueByUUID = new Dictionary<ulong, long>(); //ID的旧值Map
        public List<int> serverList = new List<int>();
        //尝试添加新记录
        public int AddRanking(int serverId, ulong id, long value, string name, int fightPoint = -1)
        {
            return mImpl.AddRanking(this, serverId, id, value, name, fightPoint);
        }

        //储存
        public IEnumerator FlushAll(Coroutine coroutine)
        {
            return mImpl.FlushAll(coroutine, this);
        }

        public void FlushAll()
        {
            mImpl.FlushAll(this);
        }

        //DB名称获取
        public string GetDbName(int serverId, string keyName)
        {
            return mImpl.GetDbName(this, serverId, keyName);
        }

        //获取某玩家的DB数据
        public DBRank_One GetPlayerData(ulong guid)
        {
            return mImpl.GetPlayerData(this, guid);
        }

        //获取某玩家的名次
        public int GetPlayerLadder(ulong guid)
        {
            return mImpl.GetPlayerLadder(this, guid);
        }

        //获得排行榜人数
        public int GetRankCount()
        {
            return mImpl.GetRankCount(this);
        }

        //获得排行榜已排次序的数量
        public int GetRankListCount()
        {
            return mImpl.GetRankListCount(this);
        }

        //获取第几名的人
        public DBRank_One GetRankOneByIndex(int nIndex)
        {
            return mImpl.GetRankOneByIndex(this, nIndex);
        }

        //初始化
        public IEnumerator Init(Coroutine coroutine, int rankId, int rankType, int serverId, string dbname)
        {
            return mImpl.Init(coroutine, this, rankId, rankType, serverId, dbname);
        }

        public void Clear()
        {
            mImpl.Clear(this);
        }

        //移除一个玩家的数据
        public bool RemoveCharacter(ulong guid)
        {
            return mImpl.RemoveCharacter(this, guid);
        }

        //排序
        public List<KeyValuePair<ulong, DBRank_One>> Sort(bool isMustDo = false)
        {
            return mImpl.Sort(this, isMustDo);
        }

        //交换两个玩家的名次
        public bool SwapIndex(int index1, int index2)
        {
            return mImpl.SwapIndex(this, index1, index2);
        }

        //交换两个玩家的名次
        public bool SwapIndex(DBRank_One db1, DBRank_One db2)
        {
            return mImpl.SwapIndex(this, db1, db2);
        }

        #region 状态日志

        public void ShowLog()
        {
            mImpl.ShowLog(this);
        }

        public void ShowRank(ulong rankManager = 20000)
        {
            mImpl.ShowRank(this, rankManager);
        }

        public void CreateClearTrigger(DateTime triggerTime, int autoInterval)
        {
            mImpl.CreateClearTrigger(this, triggerTime, autoInterval);
        }

        #endregion
    }
}