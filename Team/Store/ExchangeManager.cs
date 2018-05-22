#region using

using System;
using System.Collections;
using System.Collections.Generic;
using Database;
using DataContract;
using DataTable;
using Scorpion;
using Shared;

#endregion

namespace Team
{
    public interface IExchangeBroadcast
    {
        void Construct(ExchangeBroadcast _this, ulong cId);
        DBExchangeOne IsHave(ExchangeBroadcast _this, long id);
    }

    public class ExchangeBroadcastDefaultImpl : IExchangeBroadcast
    {
        private void RemoveThis(ExchangeBroadcast _this)
        {
            _this.trigger = null;
            ExchangeManager.RemoveBroadcast(_this.characterId);
        }

        public void Construct(ExchangeBroadcast _this, ulong cId)
        {
            _this.characterId = cId;
            _this.trigger = TeamServerControl.tm.CreateTrigger(
                DateTime.Now.AddMinutes(ExchangeBroadcast.RefreshCDTime), () => RemoveThis(_this));
        }

        public DBExchangeOne IsHave(ExchangeBroadcast _this, long id)
        {
            foreach (var one in _this.LookList)
            {
                if (one.Id == id)
                {
                    return one;
                }
            }
            return null;
        }
    }

    public class ExchangeBroadcast
    {
        private static IExchangeBroadcast mImpl;
        public static int RefreshCDTime = Table.GetServerConfig(304).ToInt();

        static ExchangeBroadcast()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (ExchangeBroadcast),
                typeof (ExchangeBroadcastDefaultImpl),
                o => { mImpl = (IExchangeBroadcast) o; });
        }

        public ExchangeBroadcast(ulong cId)
        {
            mImpl.Construct(this, cId);
        }

        public ulong characterId;
        public List<DBExchangeOne> LookList = new List<DBExchangeOne>();
        public Trigger trigger;

        public DBExchangeOne IsHave(long id)
        {
            return mImpl.IsHave(this, id);
        }
    }

    public interface IExchangeManager
    {
        void CancelItem(ulong characterId, long itemGuid);
        StoreBroadcastOne GetConverNetData(DBExchangeOne dbExchangeOne);
        void GetItems(int type, ulong characterId, int level, int count, StoreBroadcastList sbList);
        void Init();
        void InitStaticValue();

        long PushItem(ulong characterId,
                      string characterName,
                      ItemBaseData itemBaseData,
                      int needCount,
                      int ContinueMinutes);

        void RemoveBroadcast(ulong characterId);
        IEnumerator SaveDB(Coroutine coroutine);
    }

    public class ExchangeManagerDefaultImpl : IExchangeManager
    {
        #region 初始化

        public void InitStaticValue()
        {
            for (var i = 0; i < 400; ++i)
            {
                ExchangeManager.ItemList[i] = new List<DBExchangeOne>();
            }
        }

        public void Init()
        {
            CoroutineFactory.NewCoroutine(GetDB).MoveNext();
            var nextTime = DateTime.Now;
            nextTime = nextTime.AddSeconds(57 - nextTime.Second);
#if DEBUG
            nextTime = nextTime.AddMinutes(1);
            TeamServerControl.tm.CreateTrigger(nextTime, Updata, 60000); //30分钟存储一次
#else
            nextTime = nextTime.AddMinutes(59 - nextTime.Minute);
            TeamServerControl.tm.CreateTrigger(nextTime, Updata, 60000 * 30);//30分钟存储一次
#endif
        }

        private string GetExchangeDbName()
        {
            return "_Exchange_";
        }

        public IEnumerator GetDB(Coroutine coroutine)
        {
            var tasks = TeamServer.Instance.DB.Get<DBExchangeDataAll>(coroutine, DataCategory.TeamExchange,
                GetExchangeDbName());
            yield return tasks;
            if (tasks.Data == null)
            {
                InitByBase();
                yield break;
            }
            InitByDB(tasks.Data);
        }

        private void InitByBase()
        {
            ExchangeManager.mDbData = new DBExchangeDataAll();
        }

        private void InitByDB(DBExchangeDataAll dbData)
        {
            ExchangeManager.mDbData = dbData;
            var delList = new List<long>();
            foreach (var item in dbData.StoreItems)
            {
                var overTime = DateTime.FromBinary(item.Value.OverTime);
                if (overTime < DateTime.Now)
                {
                    delList.Add(item.Key);
                    continue;
                }
                AddItem(item.Value);
                //var tbItem = Table.GetItemBase(item.Value.ItemData.ItemId);
                //ExchangeManager.ItemList[tbItem.LevelLimit].Add(item.Value);
            }
            foreach (var l in delList)
            {
                dbData.StoreItems.Remove(l);
            }
        }

        public IEnumerator SaveDB(Coroutine coroutine)
        {
            var ret = TeamServer.Instance.DB.Set(coroutine, DataCategory.TeamExchange, GetExchangeDbName(),
                ExchangeManager.mDbData);
            yield return ret;
        }

        private void Updata()
        {
            CoroutineFactory.NewCoroutine(SaveDB).MoveNext();
        }

        #endregion

        #region 对外方法

        //添加广播道具
        public long PushItem(ulong characterId,
                             string characterName,
                             ItemBaseData itemBaseData,
                             int needCount,
                             int ContinueMinutes)
        {
            var temp = new DBExchangeOne();
            temp.Id = GetNextId();
            temp.SellCharacterName = characterName;
            temp.SellCharacterId = characterId;
            temp.NeedCount = needCount;
            temp.ItemData = itemBaseData;
            temp.OverTime = DateTime.Now.AddMinutes(ContinueMinutes).ToBinary();
            ExchangeManager.mDbData.StoreItems.Add(temp.Id, temp);
            AddItem(temp);
            //tbItem.
            return temp.Id;
        }

        //查询ID
        public void CancelItem(ulong characterId, long itemGuid)
        {
            var item = GetItemById(itemGuid);
            if (item == null)
            {
                return;
            }
            Trigger trigger;
            if (ExchangeManager.mTriggers.TryGetValue(item.Id, out trigger))
            {
                TeamServerControl.tm.DeleteTrigger(trigger);
            }
            RemoveItem(item);
        }

        //查询广播道具
        public void GetItems(int type, ulong characterId, int level, int count, StoreBroadcastList sbList)
        {
            ExchangeBroadcast findEB = null;
            var beginlevel = ExchangeManager.FindLevel[level];
            var beginIndex = ExchangeManager.FindIndex[level];
            var isEnd = false;
            //ExchangeManager.mBroadcasts.TryGetValue(characterId, out findEB);
            if (!ExchangeManager.mBroadcasts.TryGetValue(characterId, out findEB))
            {
                findEB = new ExchangeBroadcast(characterId);
                ExchangeManager.mBroadcasts[characterId] = findEB;
            }
            sbList.CacheOverTime = TeamServerControl.tm.GetNextTime(findEB.trigger).ToBinary();
            if (type != 0)
            {
                findEB.LookList.Clear();
                for (var i = 0; i != count; ++i)
                {
                    var item = GetNextItem(level, beginlevel, beginIndex, ref isEnd);
                    if (item == null)
                    {
                        break;
                    }
                    foreach (var one in sbList.Items)
                    {
                        if (one.Id == item.Id)
                        {
                            item = null;
                            break;
                        }
                    }
                    if (item == null)
                    {
                        break;
                    }
                    if (item.SellCharacterId != characterId) //是否要过滤自己的东西
                    {
                        findEB.LookList.Add(item);
                        sbList.Items.Add(GetConverNetData(item));
                    }
                    else
                    {
                        i--;
                    }
                    if (isEnd)
                    {
                        break;
                    }
                }
                return;
            }
            var nowCount = 0;
            foreach (var l in findEB.LookList)
            {
                sbList.Items.Add(GetConverNetData(l));
                nowCount ++;
            }
            for (var i = nowCount; i < count; ++i)
            {
                var item = GetNextItem(level, beginlevel, beginIndex, ref isEnd);
                if (item == null)
                {
                    break;
                }
                foreach (var one in sbList.Items)
                {
                    if (one.Id == item.Id)
                    {
                        item = null;
                        break;
                    }
                }
                if (item == null)
                {
                    break;
                }
                if (item.SellCharacterId != characterId) //是否要过滤自己的东西
                {
                    if (findEB.IsHave(item.Id) == null)
                    {
                        findEB.LookList.Add(item);
                        sbList.Items.Add(GetConverNetData(item));
                    }
                }
                else
                {
                    i--;
                }
                if (isEnd)
                {
                    break;
                }
            }
        }

        //某个人到时间需要移除
        public void RemoveBroadcast(ulong characterId)
        {
            ExchangeManager.mBroadcasts.Remove(characterId);
        }

        public StoreBroadcastOne GetConverNetData(DBExchangeOne dbExchangeOne)
        {
            var temp = new StoreBroadcastOne
            {
                Id = dbExchangeOne.Id,
                StartTime = dbExchangeOne.OverTime,
                ItemData = dbExchangeOne.ItemData,
                NeedCount = dbExchangeOne.NeedCount,
                SellCharacterId = dbExchangeOne.SellCharacterId,
                SellCharacterName = dbExchangeOne.SellCharacterName,
                NeedType = dbExchangeOne.NeedType
            };
            return temp;
        }

        #endregion

        #region 私有方法

        //获取下一个ID
        private long GetNextId()
        {
            ExchangeManager.mDbData.NextItemId++;
            return ExchangeManager.mDbData.NextItemId;
        }

        //添加一个道具
        private void AddItem(DBExchangeOne item)
        {
            var tbItem = Table.GetItemBase(item.ItemData.ItemId);
            var tempList = ExchangeManager.ItemList[tbItem.LevelLimit - 1];
            tempList.Add(item);
            var trigger = TeamServerControl.tm.CreateTrigger(DateTime.FromBinary(item.OverTime),
                () => { RemoveItem(item); });
            ExchangeManager.mTriggers.Add(item.Id, trigger);
            if (tempList.Count >= ExchangeManager.MaxLenth)
            {
                ExchangeManager.MaxLenth = tempList.Count;
                ExchangeManager.MaxLenIndex = tbItem.LevelLimit - 1;
            }
        }

        //移除一个道具
        private void RemoveItem(DBExchangeOne item)
        {
            var tbItem = Table.GetItemBase(item.ItemData.ItemId);
            ExchangeManager.ItemList[tbItem.LevelLimit - 1].Remove(item); //等级物品 的快速查询中移除
            ExchangeManager.mDbData.StoreItems.Remove(item.Id); //数据库引用移除
            ExchangeManager.mTriggers.Remove(item.Id); //该物品定时器移除
            if (tbItem.LevelLimit == ExchangeManager.MaxLenIndex)
            {
                ResetMaxLen(); //重新计算最大长度
            }
        }

        //刷新最大长度
        private void ResetMaxLen()
        {
            ExchangeManager.MaxLenth = 0;
            var index = 0;
            foreach (var list in ExchangeManager.ItemList)
            {
                if (list.Count > ExchangeManager.MaxLenth)
                {
                    ExchangeManager.MaxLenth = list.Count;
                    ExchangeManager.MaxLenIndex = index;
                }
                index++;
            }
        }

        //根据某个Id获得道具
        private DBExchangeOne GetItemById(long id)
        {
            DBExchangeOne temp;
            if (ExchangeManager.mDbData.StoreItems.TryGetValue(id, out temp))
            {
                return temp;
            }
            return null;
        }

        //获得一个某等级的下个物品
        private DBExchangeOne GetNextItem(int level, int stopLevel, int stopIndex, ref bool isEnd)
        {
            if (ExchangeManager.MaxLenth == 0)
            {
                return null;
            }
            var beginlevel = ExchangeManager.FindLevel[level];
            var beginIndex = ExchangeManager.FindIndex[level];
            var nowLookLevel = beginlevel;
            var nowLookIndex = beginIndex;
            DBExchangeOne result = null;
            while (true)
            {
                var list = ExchangeManager.ItemList[nowLookLevel];
                if (nowLookIndex < list.Count)
                {
                    result = list[nowLookIndex];
                }
                if (nowLookLevel >= level)
                {
                    nowLookLevel = 0;
                    nowLookIndex = nowLookIndex + 1;
                    if (nowLookIndex >= ExchangeManager.MaxLenth)
                    {
                        nowLookIndex = 0;
                    }
                    //else if (nowLookIndex == beginIndex && nowLookLevel == beginlevel)
                    //{//如果饶了一圈，则返回空
                    //    return null;
                    //}
                }
                else
                {
                    nowLookLevel++;
                }
                if (result != null)
                {
                    break;
                }
                if (nowLookIndex == stopIndex && nowLookLevel == stopLevel)
                {
//如果饶了一圈，则返回空
                    isEnd = true;
                    return null;
                }
            }
            ExchangeManager.FindLevel[level] = nowLookLevel;
            ExchangeManager.FindIndex[level] = nowLookIndex;
            if (nowLookIndex == stopIndex && nowLookLevel == stopLevel)
            {
//如果饶了一圈，则返回空
                isEnd = true;
            }
            return result;
        }

        #endregion
    }

    public static class ExchangeManager
    {
        #region 数据结构

        public static DBExchangeDataAll mDbData;
        public static List<DBExchangeOne>[] ItemList = new List<DBExchangeOne>[400]; //物品等级优化管理
        public static Dictionary<long, Trigger> mTriggers = new Dictionary<long, Trigger>(); //物品广播的定时器管理

        public static Dictionary<ulong, ExchangeBroadcast> mBroadcasts = new Dictionary<ulong, ExchangeBroadcast>();
            //玩家的请求列表管理

        public static int[] FindLevel = new int[400];
        public static int[] FindIndex = new int[400];
        public static int MaxLenth;
        public static int MaxLenIndex;

        #endregion

        #region 初始化

        private static IExchangeManager mImpl;

        static ExchangeManager()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (ExchangeManager),
                typeof (ExchangeManagerDefaultImpl),
                o => { mImpl = (IExchangeManager) o; });

            mImpl.InitStaticValue();
        }

        public static void Init()
        {
            mImpl.Init();
        }

        public static IEnumerator SaveDB(Coroutine coroutine)
        {
            return mImpl.SaveDB(coroutine);
        }

        #endregion

        #region 对外方法

        //添加广播道具
        public static long PushItem(ulong characterId,
                                    string characterName,
                                    ItemBaseData itemBaseData,
                                    int needCount,
                                    int continueMinutes)
        {
            return mImpl.PushItem(characterId, characterName, itemBaseData, needCount, continueMinutes);
        }

        //查询ID
        public static void CancelItem(ulong characterId, long itemGuid)
        {
            mImpl.CancelItem(characterId, itemGuid);
        }

        //查询广播道具
        public static void GetItems(int type, ulong characterId, int level, int count, StoreBroadcastList sbList)
        {
            mImpl.GetItems(type, characterId, level, count, sbList);
        }

        //某个人到时间需要移除
        public static void RemoveBroadcast(ulong characterId)
        {
            mImpl.RemoveBroadcast(characterId);
        }

        public static StoreBroadcastOne GetConverNetData(this DBExchangeOne dbExchangeOne)
        {
            var temp = new StoreBroadcastOne
            {
                Id = dbExchangeOne.Id,
                StartTime = dbExchangeOne.OverTime,
                ItemData = dbExchangeOne.ItemData,
                NeedCount = dbExchangeOne.NeedCount,
                SellCharacterId = dbExchangeOne.SellCharacterId,
                SellCharacterName = dbExchangeOne.SellCharacterName
            };
            return temp;
        }

        #endregion
    }
}