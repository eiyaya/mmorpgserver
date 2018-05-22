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
    public class MysteryStoreManagerDefaultImpl : IMysteryStoreManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public void Init(MysteryStoreManager _this, int serverId)
        {
            CoroutineFactory.NewCoroutine(ReadDb, _this, serverId).MoveNext();
        }

        public IEnumerator FlushAll(Coroutine coroutine, MysteryStoreManager _this)
        {
            var co = CoroutineFactory.NewSubroutine(SaveDb, coroutine, _this);
            if (co.MoveNext())
            {
                yield return co;
            }
        }

        public string GetDbName(int serverId)
        {
            return string.Format("DBMysteryStore_{0}", serverId);
        }

        public void InitByBase(MysteryStoreManager _this)
        {
            List<StoreRecord> tempList = new List<StoreRecord>();
            Table.ForeachStore(record =>
            {
                if (record.Type == 1106)//珍宝商店
                {
                    tempList.Add(record);
                }
                return true;
            });
            //Debug.Assert(false);
            GetMysteryStoreItems(_this, tempList);
        }

        public int ApplyStoreInfo(MysteryStoreManager _this, ref List<StoneItem> info)
        {
            var storeData = _this.mDbData.MysteryStoreInfo;
            if (null == storeData || storeData.Count == 0)
            {
                return (int)ErrorCodes.Unknow;
            }
            foreach (var data in storeData)
            {
                var item = new StoneItem();
                item.itemid = data.ItemId;
                item.itemcount = data.ItemCount;
                info.Add(item);
            }
            return (int)ErrorCodes.OK;
        }

        public int ConsumeStoreItem(MysteryStoreManager _this, int storeId, int consumeCount)
        {
            var tbStore = Table.GetStore(storeId);
            if (null == tbStore)
            {
                return (int)ErrorCodes.Error_StoreID;
            }
            var storeData = _this.mDbData.MysteryStoreInfo;
            if (null == storeData)
            {
                return (int)ErrorCodes.Unknow;
            }
            for (int i = 0; i < storeData.Count; i++)
            {
                var data = storeData[i];
                if (data.ItemId == storeId)
                {
                    if (_this.mDbData.MysteryStoreInfo[i].ItemCount < consumeCount)
                    {
                        return (int)ErrorCodes.Error_TreasureStoreItemCountNotEnough;
                    }
                    else
                    {
                        _this.mDbData.MysteryStoreInfo[i].ItemCount -= consumeCount;
                        return (int)ErrorCodes.OK;
                    }
                }
            }
            return (int)ErrorCodes.Error_TreasureStoreBuyFailed;
        }

        public int GetStoreItemCount(MysteryStoreManager _this, int storeId, ref int itemCount)
        {
            var tbStore = Table.GetStore(storeId);
            if (null == tbStore)
            {
                return (int)ErrorCodes.Error_StoreID;
            }
            var storeData = _this.mDbData.MysteryStoreInfo;
            if (null == storeData)
            {
                return (int)ErrorCodes.Unknow;
            }
            foreach (var data in storeData)
            {
                if (data.ItemId == storeId)
                {
                    itemCount = data.ItemCount;
                    return (int)ErrorCodes.OK;
                }
            }
            return (int)ErrorCodes.OK;
        }

        private void GetMysteryStoreItems(MysteryStoreManager _this, List<StoreRecord> tempList)
        {
            _this.mDbData.MysteryStoreInfo.Clear();
            var MysteryStoreItems = new List<StoreRecord>();
            for (int i = 0; i < tempList.Count; i++)
            {
                MysteryStoreItems.Add(tempList[i]);
            }
            var limitCount = Table.GetServerConfig(1204).ToInt();
            for (int i = 0; i < limitCount; i++)
            {
                var item = GetItemByWeight(MysteryStoreItems);
                if (null != item)
                {
                    var tempItem = new MysteryStoreItem();
                    tempItem.ItemId = item.Id;
                    tempItem.ItemCount = item.FuBenCount;//策划指定使用黑市限购数量
                    MysteryStoreItems.Remove(item);
                    _this.mDbData.MysteryStoreInfo.Add(tempItem);
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

        private IEnumerator SaveDb(Coroutine coroutine, MysteryStoreManager _this)
        {
            if (_this.mDbData != null)
            {
                PlayerLog.WriteLog((int)LogType.MysteryStoreSave,
                                   "--------------------SaveMysteryStoreData--------------------{0}"
                                   , _this.mDbData);
                var ret = ActivityServer.Instance.DB.Set(coroutine, DataCategory.MysteryStore, GetDbName(_this.ServerId), _this.mDbData);
                yield return ret;
            }
        }

        private IEnumerator ReadDb(Coroutine coroutine, MysteryStoreManager _this, int serverId)
        {
            var storeData = ActivityServer.Instance.DB.Get<DBMysteryStore>(coroutine, DataCategory.MysteryStore, GetDbName(serverId));
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

    public interface IMysteryStoreManager
    {
        #region 数据

        void Init(MysteryStoreManager _this, int serverId);
        IEnumerator FlushAll(Coroutine coroutine, MysteryStoreManager _this);
        string GetDbName(int serverId);
        void InitByBase(MysteryStoreManager _this);
        int ApplyStoreInfo(MysteryStoreManager _this, ref List<StoneItem> info);
        int ConsumeStoreItem(MysteryStoreManager _this, int storeId, int consumeCount);
        int GetStoreItemCount(MysteryStoreManager _this, int storeId, ref int itemCount);

        #endregion
    }

    public class MysteryStoreManager
    {
        public DBMysteryStore mDbData = new DBMysteryStore();
        public int ServerId;
        private static IMysteryStoreManager mImpl;
        static MysteryStoreManager()
        {
            ActivityServer.Instance.UpdateManager.InitStaticImpl(typeof(MysteryStoreManager),
                                                                 typeof(MysteryStoreManagerDefaultImpl),
                                                                 o => { mImpl = (IMysteryStoreManager)o; });
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

    public class ServerMysteryStoreManagerDefaultImpl : IServerMysteryStoreManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public void Init()
        {
            Table.ForeachServerName(record =>
            {
                if (record.LogicID == record.Id && ServerMysteryStoreManager.Servers.ContainsKey(record.LogicID) == false && (record.IsClientDisplay == 1 || record.IsClientDisplay == 2))
                {

                    MysteryStoreManager temp = new MysteryStoreManager();
                    temp.Init(record.LogicID);
                    ServerMysteryStoreManager.Servers.Add(record.LogicID, temp);
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
                    if (record.LogicID == record.Id && ServerMysteryStoreManager.Servers.ContainsKey(record.LogicID) == false &&(record.IsClientDisplay == 1 || record.IsClientDisplay == 2))
                    {
                        MysteryStoreManager temp = new MysteryStoreManager();
                        temp.Init(record.LogicID);
                        ServerMysteryStoreManager.Servers.Add(record.LogicID, temp);
                    }
                    return true;
                });
            }
            if (v.tableName == "Store")
            {
                ResetStore();
            }
        }
        public MysteryStoreManager GetMysteryStoreManager(int serverId)
        {
            MysteryStoreManager MysteryStoreMgr = null;
            if (ServerMysteryStoreManager.Servers.TryGetValue(serverId, out MysteryStoreMgr) == false)
            {
                MysteryStoreMgr = new MysteryStoreManager();
                ServerMysteryStoreManager.Servers.Add(serverId, MysteryStoreMgr);
            }
            return MysteryStoreMgr;
        }

        public int ApplyStoreInfo(int serverId, List<StoneItem> info)
        {
            var logicId = SceneExtension.GetServerLogicId(serverId);
            var mgr = GetMysteryStoreManager(logicId);
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
            var mgr = GetMysteryStoreManager(logicId);
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
            var mgr = GetMysteryStoreManager(logicId);
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
            foreach (var server in ServerMysteryStoreManager.Servers)
            {
                server.Value.InitByBase();
            }
            SetTrigger();
        }

        private IEnumerator RefreshAll(Coroutine coroutine)
        {
            foreach (var MysteryStoreMgr in ServerMysteryStoreManager.Servers)
            {
                var co = CoroutineFactory.NewSubroutine(MysteryStoreMgr.Value.FlushAll, coroutine);
                if (co.MoveNext())
                {
                    yield return co;
                }
            }
        }

        private void SetTrigger()
        {
            if (ServerMysteryStoreManager.Trigger != null)
            {
                ActivityServerControl.Timer.DeleteTrigger(ServerMysteryStoreManager.Trigger);
                ServerMysteryStoreManager.Trigger = null;
            }
            if (DateTime.Now < DateTime.Now.Date.AddHours(11.5))
            {
                ServerMysteryStoreManager.Trigger = ActivityServerControl.Timer.CreateTrigger(
                                                    DateTime.Now.Date.AddHours(11.5),
                                                    ResetStore);
            }
            else if (DateTime.Now < DateTime.Now.Date.AddHours(17.5))
            {
                ServerMysteryStoreManager.Trigger = ActivityServerControl.Timer.CreateTrigger(
                                                    DateTime.Now.Date.AddHours(17.5),
                                                    ResetStore);
            }
            else if (DateTime.Now < DateTime.Now.Date.AddHours(24))
            {
                ServerMysteryStoreManager.Trigger = ActivityServerControl.Timer.CreateTrigger(
                                                    DateTime.Now.Date.AddHours(24),
                                                    ResetStore);
            }
            CoroutineFactory.NewCoroutine(RefreshAll).MoveNext();
        }
    }

    public interface IServerMysteryStoreManager
    {
        void Init();
        void ResetStore();
        MysteryStoreManager GetMysteryStoreManager(int serverId);
        int ApplyStoreInfo(int serverId, List<StoneItem> info);
        int ConsumeStoreItem(int serverId, int storeId, int consumeCount);
        int GetStoreItemCount(int serverId, int storeId, ref int itemCount);
    }

    public static class ServerMysteryStoreManager
    {
        public static Dictionary<int, MysteryStoreManager> Servers = new Dictionary<int, MysteryStoreManager>();
        private static IServerMysteryStoreManager mImpl;
        public static Trigger Trigger = null;
        static ServerMysteryStoreManager()
        {
            ActivityServer.Instance.UpdateManager.InitStaticImpl(typeof(ServerMysteryStoreManager),
                                                                 typeof(ServerMysteryStoreManagerDefaultImpl),
                                                                 o => { mImpl = (IServerMysteryStoreManager)o; });
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
