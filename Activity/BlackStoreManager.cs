#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Database;
using DataContract;
using DataTable;
using EventSystem;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Activity
{
    public class BlackStoreManagerDefaultImpl : IBlackStoreManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public void Init(BlackStoreManager _this, int serverId)
        {
            CoroutineFactory.NewCoroutine(ReadDb, _this, serverId).MoveNext();
        }

        public IEnumerator FlushAll(Coroutine coroutine, BlackStoreManager _this)
        {
            var co = CoroutineFactory.NewSubroutine(SaveDb, coroutine, _this);
            if (co.MoveNext())
            {
                yield return co;
            }
        }

        public string GetDbName(int serverId)
        {
            return string.Format("DBBlackStore_{0}", serverId);
        }

        public void InitByBase(BlackStoreManager _this)
        {
            List<StoreRecord> tempList = new List<StoreRecord>();
            Table.ForeachStore(record =>
            {
                if (record.Type == 1100)//黑市商店
                {
                    tempList.Add(record);
                }
                return true;
            });
            //Debug.Assert(false);
            GetBlackStoreItems(_this, tempList);
        }

        public int ApplyStoreInfo(BlackStoreManager _this, ref List<StoneItem> info)
        {
            var storeData = _this.mDbData.BlackStoreInfo;
            if (null == storeData || storeData.Count == 0)
            {
                return (int)ErrorCodes.Unknow;
            }
            foreach (var data in storeData)
            {
                var item = new StoneItem();
                item.itemid = data.StoreItemId;
                item.itemcount = data.StoreItemCount;
                info.Add(item);
            }
            return (int)ErrorCodes.OK;
        }

        public int ConsumeStoreItem(BlackStoreManager _this, int storeId, int consumeCount)
        {
            var tbStore = Table.GetStore(storeId);
            if (null == tbStore)
            {
                return (int)ErrorCodes.Error_StoreID;
            }
            var storeData = _this.mDbData.BlackStoreInfo;
            if (null == storeData)
            {
                return (int)ErrorCodes.Unknow;
            }
            for (int i = 0; i < storeData.Count; i++)
            {
                var data = storeData[i];
                if (data.StoreItemId == storeId)
                {
                    if (_this.mDbData.BlackStoreInfo[i].StoreItemCount < consumeCount)
                    {
                        return (int)ErrorCodes.Error_TreasureStoreItemCountNotEnough;
                    }
                    else
                    {
                        _this.mDbData.BlackStoreInfo[i].StoreItemCount -= consumeCount;
                        return (int)ErrorCodes.OK;
                    }
                }
            }
            return (int)ErrorCodes.Error_TreasureStoreBuyFailed;
        }

        public int GetStoreItemCount(BlackStoreManager _this, int storeId, ref int itemCount)
        {
            var tbStore = Table.GetStore(storeId);
            if (null == tbStore)
            {
                return (int)ErrorCodes.Error_StoreID;
            }
            var storeData = _this.mDbData.BlackStoreInfo;
            if (null == storeData)
            {
                return (int)ErrorCodes.Unknow;
            }
            foreach (var data in storeData)
            {
                if (data.StoreItemId == storeId)
                {
                    itemCount = data.StoreItemCount;
                    return (int)ErrorCodes.OK;
                }
            }
            return (int)ErrorCodes.OK;
        }

        private void GetBlackStoreItems(BlackStoreManager _this, List<StoreRecord> tempList)
        {
            _this.mDbData.BlackStoreInfo.Clear();
            var BlackStoreItems = new List<StoreRecord>();
            for (int i = 0; i < tempList.Count; i++)
            {
                BlackStoreItems.Add(tempList[i]);
            }
            var limitCount = Table.GetServerConfig(1204).ToInt();
            for (int i = 0; i < limitCount; i++)
            {
                var item = GetItemByWeight(BlackStoreItems);
                if (null != item)
                {
                    var tempItem = new BlackStoreItem();
                    tempItem.StoreItemId = item.Id;
                    tempItem.StoreItemCount = item.FuBenCount;//策划指定使用黑市限购数量
                    BlackStoreItems.Remove(item);
                    _this.mDbData.BlackStoreInfo.Add(tempItem);
                }
            }
        }

        private StoreRecord GetItemByWeight(List<StoreRecord> tempList)
        {
            var totalWeightSum = 0;
            foreach (var item in tempList)
            {
                totalWeightSum += item.Weight + 1;
            }
            var ranWeight = 0;
            var curWeightSum = 0;
            ranWeight = MyRandom.Random(1, totalWeightSum);
            foreach (var item in tempList)
            {
                curWeightSum += item.Weight;
                if (curWeightSum >= ranWeight)
                {
                    return item;
                }
            }
            return null;
        }

        private IEnumerator SaveDb(Coroutine coroutine, BlackStoreManager _this)
        {
            if (_this.mDbData != null)
            {
                PlayerLog.WriteLog((int)LogType.BlackStoreSave,
                                   "--------------------SaveBlackStoreData--------------------{0}"
                                   , _this.mDbData);
                var ret = ActivityServer.Instance.DB.Set(coroutine, DataCategory.BlackStore, GetDbName(_this.ServerId), _this.mDbData);
                yield return ret;
            }
        }

        private IEnumerator ReadDb(Coroutine coroutine, BlackStoreManager _this, int serverId)
        {
            var storeData = ActivityServer.Instance.DB.Get<DBBlackStore>(coroutine, DataCategory.BlackStore, GetDbName(serverId));
            yield return storeData;
            _this.ServerId = serverId;
            if (storeData.Data == null)
            {
                _this.InitByBase();
                yield break;
            }
            _this.mDbData = storeData.Data;
            yield break;
        }
    }

    public interface IBlackStoreManager
    {
        #region 数据

        void Init(BlackStoreManager _this, int serverId);
        IEnumerator FlushAll(Coroutine coroutine, BlackStoreManager _this);
        string GetDbName(int serverId);
        void InitByBase(BlackStoreManager _this);
        int ApplyStoreInfo(BlackStoreManager _this, ref List<StoneItem> info);
        int ConsumeStoreItem(BlackStoreManager _this, int storeId, int consumeCount);
        int GetStoreItemCount(BlackStoreManager _this, int storeId, ref int itemCount);

        #endregion
    }

    public class BlackStoreManager
    {
        public DBBlackStore mDbData = new DBBlackStore();
        public int ServerId;
        private static IBlackStoreManager mImpl;
        static BlackStoreManager()
        {
            ActivityServer.Instance.UpdateManager.InitStaticImpl(typeof(BlackStoreManager),
                                                                 typeof(BlackStoreManagerDefaultImpl),
                                                                 o => { mImpl = (IBlackStoreManager)o; });
        }
        public void Init(int serverId)
        {
            mImpl.Init(this, serverId);
        }

        public IEnumerator FlushAll(Coroutine coroutine)
        {
            return mImpl.FlushAll(coroutine, this);
        }

        public string GetDbName(int serverId)
        {
            return mImpl.GetDbName(serverId);
        }

        public void InitByBase()
        {
            mImpl.InitByBase(this);
        }

        public int ApplyStoreInfo(ref List<StoneItem> info)
        {
            return mImpl.ApplyStoreInfo(this, ref info);
        }

        public int ConsumeStoreItem(int storeId, int consumeCount)
        {
            return mImpl.ConsumeStoreItem(this, storeId, consumeCount);
        }

        public int GetStoreItemCount(int storeId, ref int itemCount)
        {
            return mImpl.GetStoreItemCount(this, storeId, ref itemCount);
        }
    }

    public class ServerBlackStoreManagerDefaultImpl : IServerBlackStoreManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public void Init()
        {
            Table.ForeachServerName(record =>
            {
                if (record.Id==record.LogicID&&record.IsClientDisplay == 1 && ServerBlackStoreManager.Servers.ContainsKey(record.LogicID) == false)
                {
                    BlackStoreManager temp = new BlackStoreManager();
                    temp.Init(record.LogicID);
                    ServerBlackStoreManager.Servers.Add(record.LogicID, temp);
                }
                return true;
            });
            SetTrigger();
            EventDispatcher.Instance.AddEventListener(ReloadTableEvent.EVENT_TYPE, ReloadTable);
        }
        private void ReloadTable(IEvent ievent)
        {
            var v = ievent as ReloadTableEvent;
            if (v == null)
                return;
            if (v.tableName == "ServerName")
            {
                Table.ForeachServerName(record =>
                {
                    if (record.Id == record.LogicID && record.IsClientDisplay == 1 && ServerBlackStoreManager.Servers.ContainsKey(record.LogicID) == false)
                    {
                        BlackStoreManager temp = new BlackStoreManager();
                        temp.Init(record.LogicID);
                        ServerBlackStoreManager.Servers.Add(record.LogicID, temp);
                    }
                    return true;
                });
            }
            if (v.tableName == "Store")
            {
                ResetStore();
            }
        }
        public BlackStoreManager GetBlackStoreManager(int serverId)
        {
            BlackStoreManager BlackStoreMgr = null;
            if (ServerBlackStoreManager.Servers.TryGetValue(serverId, out BlackStoreMgr) == false)
            {
                BlackStoreMgr = new BlackStoreManager();
                ServerBlackStoreManager.Servers.Add(serverId, BlackStoreMgr);
            }
            return BlackStoreMgr;
        }

        public int ApplyStoreInfo(int serverId, List<StoneItem> info)
        {
            var logicId = SceneExtension.GetServerLogicId(serverId);
            var mgr = GetBlackStoreManager(logicId);
            if (mgr == null)
            {
                return (int)ErrorCodes.Unknow;
            }
            var result = mgr.ApplyStoreInfo(ref info);
            return result;
        }

        public int ConsumeStoreItem(int serverId, int storeId, int consumeCount)
        {
            var logicId = SceneExtension.GetServerLogicId(serverId);
            var mgr = GetBlackStoreManager(logicId);
            if (mgr == null)
            {
                return (int)ErrorCodes.Unknow;
            }
            var result = mgr.ConsumeStoreItem(storeId, consumeCount);
            if (result == (int)ErrorCodes.OK)
            {
                CoroutineFactory.NewCoroutine(mgr.FlushAll).MoveNext();
            }
            return result;
        }

        public int GetStoreItemCount(int serverId, int storeId, ref int itemCount)
        {
            var logicId = SceneExtension.GetServerLogicId(serverId);
            var mgr = GetBlackStoreManager(logicId);
            if (mgr == null)
            {
                return (int)ErrorCodes.Unknow;
            }
            var result = mgr.GetStoreItemCount(storeId, ref itemCount);
            return result;
        }

        /// <summary>
        /// 重置商店
        /// </summary>
        public void ResetStore()
        {
            foreach (var server in ServerBlackStoreManager.Servers)
            {
                server.Value.InitByBase();
            }
            SetTrigger();
        }

        private IEnumerator RefreshAll(Coroutine coroutine)
        {
            foreach (var BlackStoreMgr in ServerBlackStoreManager.Servers)
            {
                var co = CoroutineFactory.NewSubroutine(BlackStoreMgr.Value.FlushAll, coroutine);
                if (co.MoveNext())
                {
                    yield return co;
                }
            }
        }

        private void SetTrigger()
        {
            if (ServerBlackStoreManager.Trigger != null)
            {
                ActivityServerControl.Timer.DeleteTrigger(ServerBlackStoreManager.Trigger);
                ServerBlackStoreManager.Trigger = null;
            }
            ServerBlackStoreManager.Trigger = ActivityServerControl.Timer.CreateTrigger(
                                              DateTime.Now.Date.AddHours(24),
                                              ResetStore);
            CoroutineFactory.NewCoroutine(RefreshAll).MoveNext();
        }
    }

    public interface IServerBlackStoreManager
    {
        void Init();
        void ResetStore();
        BlackStoreManager GetBlackStoreManager(int serverId);
        int ApplyStoreInfo(int serverId, List<StoneItem> info);
        int ConsumeStoreItem(int serverId, int storeId, int consumeCount);
        int GetStoreItemCount(int serverId, int storeId, ref int itemCount);
    }

    public static class ServerBlackStoreManager
    {
        public static Dictionary<int, BlackStoreManager> Servers = new Dictionary<int, BlackStoreManager>();
        private static IServerBlackStoreManager mImpl;
        public static Trigger Trigger = null;
        static ServerBlackStoreManager()
        {
            ActivityServer.Instance.UpdateManager.InitStaticImpl(typeof(ServerBlackStoreManager),
                                                                 typeof(ServerBlackStoreManagerDefaultImpl),
                                                                 o => { mImpl = (IServerBlackStoreManager)o; });
        }
        public static void Init()
        {
            mImpl.Init();
        }
        public static int ApplyStoreInfo(int serverId, ref List<StoneItem> info)
        {
            return mImpl.ApplyStoreInfo(serverId, info);
        }
        public static int ConsumeStoreItem(int serverId, int storeId, int consumeCount)
        {
            return mImpl.ConsumeStoreItem(serverId, storeId, consumeCount);
        }
        public static int GetStoreItemCount(int serverId, int storeId, ref int itemCount)
        {
            return mImpl.GetStoreItemCount(serverId, storeId, ref itemCount);
        }
    }

}
