#region using

using System;
using System.Collections.Generic;
using Shared;

#endregion

namespace Team
{
    public interface IStoreOne
    {
        void Construct(StoreOne _this, StoreList d);
    }

    public class StoreOneDefaultImpl : IStoreOne
    {
        public static readonly int RemoveCount = 600;

        private void Remove(StoreOne _this)
        {
            if (_this.trigger != null)
            {
                TeamServerControl.tm.DeleteTrigger(_this.trigger);
                _this.trigger = null;
            }
            _this.mDad.ItemList.Remove(_this.StoreGuid);
        }

        //心跳
        private void Updata(StoreOne _this)
        {
            _this.UpdataCount++;
            if (_this.UpdataCount >= RemoveCount)
            {
                Remove(_this);
                return;
            }
            if (!_this.isNew)
            {
                StoreManager.TryPushNewItem(_this);
            }
        }

        public void Construct(StoreOne _this, StoreList d)
        {
            _this.trigger = TeamServerControl.tm.CreateTrigger(DateTime.Now.AddSeconds(5), () => Updata(_this), 5000);
            _this.mDad = d;
        }
    }

    public class StoreOne
    {
        private static IStoreOne mImpl;

        static StoreOne()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (StoreOne), typeof (StoreOneDefaultImpl),
                o => { mImpl = (IStoreOne) o; });
        }

        public StoreOne(StoreList d)
        {
            mImpl.Construct(this, d);
        }

        public ulong CharacterGuid;
        public int Count;
        public bool isNew = true;
        public int ItemId;
        public int Level;
        public int lookCount;
        public StoreList mDad;
        public int Name;
        public int Need;
        public ulong StoreGuid;
        public Trigger trigger;
        public int UpdataCount;
    }

    public interface IStoreList
    {
        void Construct(StoreList _this, int id);
    }

    public class StoreListDefaultImpl : IStoreList
    {
        public void Construct(StoreList _this, int id)
        {
            _this.ItemId = id;
        }
    }

    public class StoreList
    {
        private static IStoreList mImpl;

        static StoreList()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (StoreList), typeof (StoreListDefaultImpl),
                o => { mImpl = (IStoreList) o; });
        }

        public StoreList(int id)
        {
            mImpl.Construct(this, id);
        }

        public int ItemId;
        public Dictionary<ulong, StoreOne> ItemList = new Dictionary<ulong, StoreOne>();
    }

    public interface IStoreManager
    {
        void AddItem(int itemId, int count, int need, ulong cGuid, int level, string name);
        List<StoreOne> GetItem(ulong guid);
        void Init();
        void TryPushNewItem(StoreOne newItem);
    }

    public class StoreManagerDefaultImpl : IStoreManager
    {
        public static readonly int ResetTimeMax = 300;
        public static readonly int ResetTimeMin = 30;
        //初始化
        public void Init()
        {
            StoreManager.trigger =
                TeamServerControl.tm.CreateTrigger(DateTime.Now.AddSeconds(StoreManager.ResetTimeSeonds*1000), Updata,
                    StoreManager.ResetTimeSeonds*1000);
        }

        #region 对外方法

        //添加一个道具的广播
        public void AddItem(int itemId, int count, int need, ulong cGuid, int level, string name)
        {
            var guid = GetNextId();
            var sl = GetStoreList(itemId);
            var temp = new StoreOne(sl)
            {
                StoreGuid = guid,
                ItemId = itemId,
                Count = count,
                Need = need,
                CharacterGuid = cGuid
            };
            sl.ItemList[guid] = temp;
            PushNewItem(temp);
        }

        //请求一次拍卖列表
        public List<StoreOne> GetItem(ulong guid)
        {
            var temp = new List<StoreOne>();
            var index = 0;
            foreach (var one in StoreManager.mNewList)
            {
                if (index >= 20)
                {
                    return temp;
                }
                temp.Add(one);
                one.lookCount++;
                index++;
            }
            return temp;
        }

        //尝试加入
        public void TryPushNewItem(StoreOne newItem)
        {
            if (StoreManager.mNewList.Count > 200)
            {
                return;
            }
            if (newItem.lookCount > 9999)
            {
                return;
            }
            PushNewItem(newItem);
        }

        #endregion

        #region 私有方法

        //获得下一个队伍ID
        private ulong GetNextId()
        {
            return StoreManager.NextId++;
        }

        //获取StoreList
        private StoreList GetStoreList(int itemId)
        {
            StoreList store;
            if (StoreManager.mStoreList.TryGetValue(itemId, out store))
            {
                return store;
            }
            store = new StoreList(itemId);
            StoreManager.mStoreList[itemId] = store;
            return store;
        }

        //添加一个新的内容
        private void PushNewItem(StoreOne newItem)
        {
            StoreManager.mNewList.Add(newItem);
        }

        //心跳
        private void Updata()
        {
            var nowCount = StoreManager.mNewList.Count;
            if (nowCount < 20)
            {
                return;
            }
            var delCount = nowCount - 20;
            if (delCount > 20)
            {
                delCount = 20;
            }
            for (var i = 0; i < delCount; i++)
            {
                StoreManager.mNewList[0].isNew = false;
                StoreManager.mNewList.RemoveAt(0);
            }
            CheckUpdata();
        }

        //检查是否需要修改时间
        private void CheckUpdata()
        {
            if (StoreManager.mNewList.Count > 200)
            {
                ResetRefreshTime(StoreManager.ResetTimeSeonds - 10);
            }
            else if (StoreManager.mNewList.Count < 30)
            {
                ResetRefreshTime(StoreManager.ResetTimeSeonds + 10);
            }
        }

        //重置刷新时间
        private void ResetRefreshTime(int times)
        {
            if (times < ResetTimeMin)
            {
                times = ResetTimeMin;
            }
            else if (times > ResetTimeMax)
            {
                times = ResetTimeMax;
            }
            StoreManager.ResetTimeSeonds = times;
            TeamServerControl.tm.ChangeTime(ref StoreManager.trigger, DateTime.Now.AddSeconds(times), times*1000);
        }

        #endregion
    }

    public static class StoreManager
    {
        private static IStoreManager mImpl;
        public static List<StoreOne> mNewList = new List<StoreOne>(); //新的道具列表
        public static Dictionary<int, StoreList> mStoreList = new Dictionary<int, StoreList>();
        public static ulong NextId;
        public static int ResetTimeSeonds = 150; //
        public static Trigger trigger;

        static StoreManager()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (StoreManager), typeof (StoreManagerDefaultImpl),
                o => { mImpl = (IStoreManager) o; });
        }

        //初始化
        public static void Init()
        {
            mImpl.Init();
        }

        #region 对外方法

        //添加一个道具的广播
        public static void AddItem(int itemId, int count, int need, ulong cGuid, int level, string name)
        {
            mImpl.AddItem(itemId, count, need, cGuid, level, name);
        }

        //请求一次拍卖列表
        public static List<StoreOne> GetItem(ulong guid)
        {
            return GetItem(guid);
        }

        //尝试加入
        public static void TryPushNewItem(StoreOne newItem)
        {
            mImpl.TryPushNewItem(newItem);
        }

        #endregion
    }
}