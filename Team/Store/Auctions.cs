#region using

using System;
using System.Collections;
using System.Collections.Generic;
using Database;
using DataContract;
using DataTable;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Team
{
    public class Auctions
    {
        public AuctionItemOne dbData;
        public Trigger mTrigger;
        public int serverId;

        public long ManagerId
        {
            get { return dbData.ManagerId; }
        }

        public void TimeOver()
        {
            var aux = ServerAuctionManager.instance.GetAuction(serverId);
            if (aux == null)
            {
                return;
            }
            mTrigger = null;
            aux.RemoveItem(this);
        }
    }

    public class AuctionManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static int saveCount = 60;
        public Dictionary<int, DBAuctionItemServer> dbs = new Dictionary<int, DBAuctionItemServer>(); //key = serverId  
        public Dictionary<long, Auctions> items = new Dictionary<long, Auctions>(); //道具的Guid

        public Dictionary<int, Dictionary<long, Auctions>> selects = new Dictionary<int, Dictionary<long, Auctions>>();
            //key = selectId , key2 = 道具的Guid

        #region 服务器相关

        //增加服务器的DB进入这个拍卖行
        public void PushServerId(int serverId)
        {
            CoroutineFactory.NewCoroutine(GetDb, serverId).MoveNext();
            saveCount += 10;
            TeamServerControl.tm.CreateTrigger(DateTime.Now.AddSeconds(saveCount), () =>
            {
                var aux = GetServerDb(serverId);
                if (aux == null)
                {
                    return;
                }
                CoroutineFactory.NewCoroutine(SaveOne, aux, serverId.ToString()).MoveNext();
            }, 30*1000);
        }

        public IEnumerator SaveAll(Coroutine coroutine)
        {
            foreach (var dbAuctionItemServer in dbs)
            {
                var co = CoroutineFactory.NewSubroutine(SaveOne, coroutine, dbAuctionItemServer.Value,
                    dbAuctionItemServer.Key.ToString());
                if (co.MoveNext())
                {
                    yield return co;
                }
            }
        }

        private IEnumerator SaveOne(Coroutine coroutine, DBAuctionItemServer value, string dbKeyName)
        {
            var ret = TeamServer.Instance.DB.Set(coroutine, DataCategory.TeamAuctions, dbKeyName, value);
            yield return ret;
        }

        //增加服务器
        public IEnumerator GetDb(Coroutine coroutine, int serverId)
        {
            var tasks = TeamServer.Instance.DB.Get<DBAuctionItemServer>(coroutine, DataCategory.TeamAuctions,
                serverId.ToString());
            yield return tasks;
            if (tasks.Data == null)
            {
                tasks.Data = new DBAuctionItemServer();
                tasks.Data.ServerId = serverId;
            }
            else
            {
                PushServerData(tasks.Data);
            }
            dbs[serverId] = tasks.Data;
        }

        //获取相关服务器DB
        private DBAuctionItemServer GetServerDb(int serverId)
        {
            DBAuctionItemServer db;
            if (dbs.TryGetValue(serverId, out db))
            {
                return db;
            }
            return null;
        }

        //初始化ServerDb
        public void PushServerData(DBAuctionItemServer dbServer)
        {
            dbs[dbServer.ServerId] = dbServer;
            var del = new List<long>();
            foreach (var item in dbServer.Items)
            {
                var over = DateTime.FromBinary(item.Value.OverTime);
                if (over < DateTime.Now)
                {
                    del.Add(item.Key);
                    continue;
                }
                AddItem(new Auctions {serverId = dbServer.ServerId, dbData = item.Value}, false);
            }
            foreach (var l in del)
            {
                dbServer.Items.Remove(l);
            }
        }

        #endregion

        #region 道具相关

        //获取道具信息
        public Auctions GetItem(long guid)
        {
            Auctions auc;
            if (items.TryGetValue(guid, out auc))
            {
                return auc;
            }
            return null;
        }

        //移除道具
        public void RemoveItem(Auctions auc)
        {
            if (auc.mTrigger != null)
            {
                TeamServerControl.tm.DeleteTrigger(auc.mTrigger);
            }
            items.Remove(auc.ManagerId);
            var sdb = GetServerDb(auc.serverId);
            if (sdb == null)
            {
                Logger.Warn("AuctionManager RemoveItem id={0},serverid={1}", auc.ManagerId, auc.serverId);
                return;
            }
            sdb.Items.Remove(auc.ManagerId);
            var tbItem = Table.GetItemBase(auc.dbData.ItemData.ItemId);
            if (tbItem != null)
            {
                Dictionary<long, Auctions> finds;
                if (selects.TryGetValue(tbItem.AuctionType[2], out finds))
                {
                    finds.Remove(auc.ManagerId);
                }
            }
        }

        //添加道具
        public void AddItem(Auctions auc, bool isAddDB = true)
        {
            var tbItem = Table.GetItemBase(auc.dbData.ItemData.ItemId);
            if (tbItem == null)
            {
                Logger.Warn("AuctionManager AddItem id={0},ItemId={1}", auc.ManagerId, auc.dbData.ItemData.ItemId);
                return;
            }
            if (isAddDB)
            {
                var sdb = GetServerDb(auc.serverId);
                if (sdb == null)
                {
                    Logger.Warn("AuctionManager AddItem id={0},serverid={1}", auc.ManagerId, auc.serverId);
                    return;
                }
                sdb.Items[auc.ManagerId] = auc.dbData;
            }
            var overTime = DateTime.FromBinary(auc.dbData.OverTime);
            //if (overTime < DateTime.Now)
            //{
            //    return;
            //}
            items[auc.ManagerId] = auc;
            var selectType = tbItem.AuctionType[2];
            Dictionary<long, Auctions> finds;
            if (!selects.TryGetValue(selectType, out finds))
            {
                finds = new Dictionary<long, Auctions>();
                selects[selectType] = finds;
            }
            finds.Add(auc.ManagerId, auc);
            auc.mTrigger = TeamServerControl.tm.CreateTrigger(overTime, auc.TimeOver);
        }

        //查询道具
        public Dictionary<long, Auctions> SelectItem(int selectType)
        {
            var selectId = selectType;
            Dictionary<long, Auctions> finds;
            if (selects.TryGetValue(selectId, out finds))
            {
                return finds;
            }
            return null;
        }

        #endregion
    }

    public class ServerAuctionManager
    {
        public static int AuctionTime = Table.GetServerConfig(611).ToInt();
        private static ServerAuctionManager mInstance;

        public Dictionary<int, List<int>> aId2sId = new Dictionary<int, List<int>>();
            //key = serverLogicId value = serverid s

        public Dictionary<int, AuctionManager> servers = new Dictionary<int, AuctionManager>(); //key = serverLogicId

        public static ServerAuctionManager instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new ServerAuctionManager();
                }
                return mInstance;
            }
        }

        #region 初始化

        //初始化
        public void Init()
        {
            Table.ForeachServerName(record =>
            {
                List<int> list;
                if (!aId2sId.TryGetValue(record.AuctionId, out list))
                {
                    list = new List<int>();
                    aId2sId[record.AuctionId] = list;
                }
                list.Add(record.Id);
                return true;
            });
            foreach (var a2sid in aId2sId)
            {
                var auction = new AuctionManager();
                servers[a2sid.Key] = auction;
                foreach (var i in a2sid.Value)
                {
                    auction.PushServerId(i);
                }
            }
        }

        //存储
        public IEnumerator Save(Coroutine coroutine)
        {
            //IEnumerator SaveOne(Coroutine coroutine, DBAuctionItemServer value, string dbKeyName)
            foreach (var manager in servers)
            {
                var co = CoroutineFactory.NewSubroutine(manager.Value.SaveAll, coroutine);
                if (co.MoveNext())
                {
                    yield return co;
                }
            }
        }

        #endregion

        #region 服务器逻辑

        public AuctionManager GetAuction(int serverId)
        {
            var tbServer = Table.GetServerName(serverId);
            if (tbServer == null)
            {
                return null;
            }
            return servers[tbServer.AuctionId];
        }

        //获取下一个ID
        public long GetNextId()
        {
            ExchangeManager.mDbData.NextItemId++;
            return ExchangeManager.mDbData.NextItemId;
        }

        #endregion
    }
}