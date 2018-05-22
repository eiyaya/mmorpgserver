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
using Shared;

#endregion

namespace Team
{
    public interface IGroupShopOne
    {
        int CharacterBuy(GroupShopOne _this, ulong cId, int buyCount);
        bool CheckCountLimit(GroupShopOne _this, ulong cId, int buyCount);
        GroupShopItemOne GetNetData(GroupShopOne _this, ulong cId);
        void InitByBase(GroupShopOne _this, GroupShopRecord tbTable);
        void InitByDB(GroupShopOne _this, DBGroupShopOne dbData);
        void OverCopy(GroupShopOne _this, GroupShopOne one);
    }

    public class GroupShopOneDefaultImpl : IGroupShopOne
    {
        #region 数据结构

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); //

        #endregion

        #region 初始化

        public void InitByBase(GroupShopOne _this, GroupShopRecord tbTable)
        {
            _this.mDbData = new DBGroupShopOne();
            if (_this.mDbData.ItemData == null)
            {
                _this.mDbData.ItemData = new ItemBaseData();
            }
            _this.tbGroupShop = tbTable;
            _this.mDbData.GroupShopId = tbTable.Id;
            _this.OverTime = DateTime.Now.AddHours(tbTable.ExistTime);
            var items = new Dictionary<int, int>();
            ShareDrop.DropMother(tbTable.MotherID, items);
            var itemsCount = items.Count;
            if (itemsCount < 1)
            {
                ShareItemFactory.Create(22000, _this.mDbData.ItemData);
            }
            else
            {
                var itemT = items.First();
                var item = ShareItemFactory.Create(itemT.Key, _this.mDbData.ItemData);
                item.SetCount(itemT.Value);
                if (itemsCount > 1)
                {
                    Logger.Warn("GroupShopOne InitByBase itemsCount ={0} GroupShopRecord={1}", itemsCount, tbTable.Id);
                }
            }
            _this.overTrigger = TeamServerControl.tm.CreateTrigger(_this.OverTime, () => TimeOver(_this));
        }

        public void InitByDB(GroupShopOne _this, DBGroupShopOne dbData)
        {
            _this.mDbData = dbData;
            _this.tbGroupShop = Table.GetGroupShop(dbData.GroupShopId);
            foreach (var character in dbData.Characters)
            {
                _this.CharactersCount.modifyValue(character, 1);
            }
            if (_this.State == (int) eGroupShopItemState.WaitResult)
            {
                _this.overTrigger = TeamServerControl.tm.CreateTrigger(_this.OverTime, () => GiveItem(_this));
            }
            else if (_this.State == (int) eGroupShopItemState.OnSell)
            {
                _this.overTrigger = TeamServerControl.tm.CreateTrigger(_this.OverTime, () => TimeOver(_this));
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        ///     团购物品发出去后，给各个服务器发送通知
        ///     对于最高档的物品，通知会发给所有服务器的所有人，否则，就只发给中奖者所在的服务器的所有人
        /// </summary>
        /// <param name="co"></param>
        /// <param name="_this"></param>
        /// <returns></returns>
        private IEnumerator SendChatNotify(Coroutine co, GroupShopOne _this)
        {
            var strs = new List<string>
            {
                _this.mDbData.LuckyName,
                _this.mDbData.LuckyCount.ToString(),
                Utils.AddItemId(_this.mDbData.ItemData.ItemId)
            };
            var exData = new List<int>(_this.mDbData.ItemData.Exdata);
            var content = Utils.WrapDictionaryId(300409, strs, exData);
            var chatAgent = TeamServer.Instance.ChatAgent;
            if (GroupShop.TopItems.Contains(_this.mDbData.GroupShopId))
            {
//最高档的奖励，发所有服务器的所有人
                //缓存一下这条消息
                var notifys = GroupShop.DbData.Notifys.Items;
                if (notifys.Count > 20)
                {
                    notifys.RemoveAt(0);
                }
                notifys.Add(content);

                //发送给所有服务器的所有人
                var serverIds = new List<int>();
                Table.ForeachServerName(r =>
                {
                    var serverId = r.LogicID;
                    if (!serverIds.Contains(serverId))
                    {
                        serverIds.Add(serverId);
                    }
                    return true;
                });
                foreach (var id in serverIds)
                {
                    chatAgent.BroadcastWorldMessage((uint) id, (int) eChatChannel.WishingGroup, 0, string.Empty,
                        new ChatMessageContent {Content = content});
                    yield return TeamServerControl.Instance.Wait(co, TimeSpan.FromSeconds(3));
                }
            }
            else
            {
//只发本服务器的人
                var serverId = SceneExtension.GetServerLogicId(_this.mDbData.LuckyServerId);
                chatAgent.BroadcastWorldMessage((uint) serverId, (int) eChatChannel.WishingGroup, 0, string.Empty,
                    new ChatMessageContent {Content = content});
            }
        }

        /// <summary>
        ///     尚未买满，但团购时间结束了
        /// </summary>
        /// <returns></returns>
        private void TimeOver(GroupShopOne _this)
        {
            _this.overTrigger = null;
            if (_this.BuyCharacterList.Count == 0)
            {
                Reset(_this);
                return;
            }
            WaitResult(_this);
        }

        private void Reset(GroupShopOne _this)
        {
            if (_this.overTrigger != null)
            {
                TeamServerControl.tm.DeleteTrigger(_this.overTrigger);
                _this.overTrigger = null;
            }
            var guid = GroupShop.GetNextGuid();
            GroupShop.ResetShopItem(_this, guid);
            var items = new Dictionary<int, int>();
            ShareDrop.DropMother(_this.tbGroupShop.MotherID, items);
            var itemsCount = items.Count;
            if (itemsCount < 1)
            {
                ShareItemFactory.Create(22000, _this.mDbData.ItemData);
            }
            else
            {
                var itemT = items.First();
                var item = ShareItemFactory.Create(itemT.Key, _this.mDbData.ItemData);
                item.SetCount(itemT.Value);
                if (itemsCount > 1)
                {
                    Logger.Warn("GroupShopOne InitByBase itemsCount ={0} GroupShopRecord={1}", itemsCount,
                        _this.tbGroupShop.Id);
                }
            }
            _this.OverTime = DateTime.Now.AddHours(_this.tbGroupShop.ExistTime);
            _this.overTrigger = TeamServerControl.tm.CreateTrigger(_this.OverTime, () => TimeOver(_this));
        }

        /// <summary>
        ///     买满了，等待揭晓结果
        /// </summary>
        /// <returns></returns>
        private void WaitResult(GroupShopOne _this)
        {
            GroupShop.Dirty = true;
            _this.State = (int) eGroupShopItemState.WaitResult;

            //流拍逻辑
            if (_this.NowCount < _this.tbGroupShop.LimitMinCount)
            {
                CoroutineFactory.NewCoroutine(PassIn, _this).MoveNext();
                return;
            }

            var t = DateTime.Now.AddMinutes(GroupShopOne.OverTimeMinutes);
            _this.OverTime = t;
            if (_this.overTrigger != null)
            {
                TeamServerControl.tm.DeleteTrigger(_this.overTrigger);
            }
            _this.overTrigger = TeamServerControl.tm.CreateTrigger(t, () => GiveItem(_this));

            var characterId = _this.BuyCharacterList.Range();
            var count = _this.CharactersCount[characterId];
            _this.mDbData.LuckyId = characterId;
            _this.mDbData.LuckyCount = count;
            CoroutineFactory.NewCoroutine(GetLogicSimpleDataCoroutine, _this).MoveNext();

            var item = _this.mDbData.ItemData;
            PlayerLog.WriteLog((ulong) LogType.GroupShopOldDatasKeySame,
                "WaitResult, luckyId = {0}, luckyCount = {1}, item id = {2}, count = {3}", characterId, count,
                item.ItemId, item.Count);
        }

        private IEnumerator PassIn(Coroutine co, GroupShopOne _this)
        {
            var args = new StringArray();
            var item = new ItemBaseData();  
            item.ItemId = (int)eResourcesType.DiamondRes;
            var tbItem = Table.GetItemBase(_this.mDbData.ItemData.ItemId);
            if (tbItem != null)
            {
                args.Items.Add(tbItem.Name);
            }
            foreach (var v in _this.CharactersCount)
            {
                item.Count = v.Value;     
                var msg = TeamServer.Instance.LogicAgent.SendMailToCharacterByItems(v.Key, 5, item, args);
                msg.SendAndWaitUntilDone(co);
            }
            Reset(_this);
            yield break;
        }
        //给某个玩家发奖
        private void GiveItem(GroupShopOne _this)
        {
            //给予物品
            CoroutineFactory.NewCoroutine(GroupShopMailCoroutine, _this).MoveNext();
            Reset(_this);
        }

        private IEnumerator GroupShopMailCoroutine(Coroutine co, GroupShopOne _this)
        {
            var data = _this.mDbData;
            var item = data.ItemData;
            var characterId = data.LuckyId;

            PlayerLog.WriteLog((ulong) LogType.GroupShopOldDatasKeySame,
                "GroupShopMailCoroutine, luckyId = {0}, luckyCount = {1}, item id = {2}, count = {3}", characterId,
                data.LuckyCount,
                item.ItemId, item.Count);

            var args = new StringArray();
            args.Items.Add(data.LuckyCount.ToString());
            var msg = TeamServer.Instance.LogicAgent.SendMailToCharacterByItems(characterId, 52, item, args);
            yield return msg.SendAndWaitUntilDone(co);
        }

        private IEnumerator GetLogicSimpleDataCoroutine(Coroutine co, GroupShopOne _this)
        {
            var characterId = _this.mDbData.LuckyId;
            var loginMsg = TeamServer.Instance.LoginAgent.GetLoginSimpleData(characterId, characterId);
            yield return loginMsg.SendAndWaitUntilDone(co);
            if (loginMsg.State != MessageState.Reply)
            {
                yield break;
            }
            if (loginMsg.ErrorCode != (int) ErrorCodes.OK)
            {
                yield break;
            }
            var charData = loginMsg.Response;
            _this.mDbData.LuckyName = charData.Name;
            _this.mDbData.LuckyServerId = charData.ServerId;
        }

        #endregion

        #region 对外方法

        public bool CheckCountLimit(GroupShopOne _this, ulong cId, int buyCount)
        {
            if (_this.tbGroupShop.BuyLimit > 0)
            {
                int count;
                if (!_this.CharactersCount.TryGetValue(cId, out count))
                {
                    count = 0;
                }
                if (count + buyCount > _this.tbGroupShop.BuyLimit)
                {
                    return true;
                }
            }
            return false;
        }

        //某玩家买了一次
        public int CharacterBuy(GroupShopOne _this, ulong cId, int buyCount)
        {
            _this.CharactersCount.modifyValue(cId, buyCount);
            for (var i = 0; i < buyCount; i++)
            {
                _this.BuyCharacterList.Add(cId);
            }
            var count = _this.BuyCharacterList.Count;
            if (count >= _this.MaxCount)
            {
                //重置物品
                WaitResult(_this);
            }
            return count;
        }

        //网络类型转换
        public GroupShopItemOne GetNetData(GroupShopOne _this, ulong cId)
        {
            var netData = new GroupShopItemOne
            {
                Guid = _this.Guid,
                GroupShopId = _this.mDbData.GroupShopId,
                ItemData = _this.mDbData.ItemData,
                OverTime = _this.mDbData.OverTime,
                State = _this.State
            };

            var tbServerName = Table.GetServerName(_this.mDbData.LuckyServerId);
            switch ((eGroupShopItemState) _this.State)
            {
                case eGroupShopItemState.OnSell:
                    netData.CharacterCount = _this.BuyCharacterList.Count;
                    break;
                case eGroupShopItemState.WaitResult:
                    netData.CharacterCount = _this.BuyCharacterList.Count;
                    netData.LuckyPlayer = _this.mDbData.LuckyName;
                    netData.LuckyCount = _this.mDbData.LuckyCount;
                    netData.LuckyServer = tbServerName != null ? tbServerName.Name : "Nothing";
                    break;
                case eGroupShopItemState.Sold:
                    netData.CharacterCount = _this.mDbData.BuyCount;
                    netData.LuckyPlayer = _this.mDbData.LuckyName;
                    netData.LuckyCount = _this.mDbData.LuckyCount;
                    netData.LuckyServer = tbServerName != null ? tbServerName.Name : "Nothing";
                    break;
            }

            int count;
            if (_this.CharactersCount.TryGetValue(cId, out count))
            {
                netData.SelfCount = count;
            }
            return netData;
        }

        public void OverCopy(GroupShopOne _this, GroupShopOne one)
        {
            if (_this.mDbData == null)
            {
                _this.mDbData = new DBGroupShopOne();
                if (_this.mDbData.ItemData == null)
                {
                    _this.mDbData.ItemData = new ItemBaseData();
                }
            }
            _this.Guid = one.Guid;
            _this.tbGroupShop = one.tbGroupShop;
            _this.State = (int) eGroupShopItemState.Sold;
            _this.CharactersCount.AddRange(one.CharactersCount);
            _this.mDbData.Characters.AddRange(one.mDbData.Characters);
            _this.mDbData.GroupShopId = one.tbGroupShop.Id;
            _this.mDbData.OverTime = one.mDbData.OverTime;
            _this.mDbData.ItemData.ItemId = one.mDbData.ItemData.ItemId;
            _this.mDbData.ItemData.Count = one.mDbData.ItemData.Count;
            _this.mDbData.ItemData.Exdata.AddRange(one.mDbData.ItemData.Exdata);
            _this.mDbData.BuyCount = one.NowCount;
            _this.mDbData.LuckyId = one.mDbData.LuckyId;
            _this.mDbData.LuckyCount = one.mDbData.LuckyCount;
            _this.mDbData.LuckyName = one.mDbData.LuckyName;
            _this.mDbData.LuckyServerId = one.mDbData.LuckyServerId;

            if (_this.mDbData.LuckyId > 0)
            {
                CoroutineFactory.NewCoroutine(SendChatNotify, _this).MoveNext();
            }
        }

        #endregion
    }

    public class GroupShopOne
    {
        #region 数据结构

        public static int OverTimeMinutes = Table.GetServerConfig(413).ToInt();
        public DBGroupShopOne mDbData;

        public long Guid
        {
            get { return mDbData.Guid; }
            set { mDbData.Guid = value; }
        }

        public int NowCount
        {
            get { return mDbData.Characters.Count; }
        }

        public int MaxCount
        {
            get { return tbGroupShop.LuckNumber; }
        }

        public GroupShopRecord tbGroupShop;

        public List<ulong> BuyCharacterList
        {
            get { return mDbData.Characters; }
        } //买此东西的玩家，有重复
        public Dictionary<ulong, int> CharactersCount = new Dictionary<ulong, int>(); //每个玩家买了几个

        public DateTime OverTime
        {
            get { return DateTime.FromBinary(mDbData.OverTime); }
            set { mDbData.OverTime = value.ToBinary(); }
        }

        public ItemBaseData Item;

        public int State
        {
            get { return mDbData.State; }
            set { mDbData.State = value; }
        }

        public Trigger overTrigger;

        #endregion

        #region 初始化

        private static IGroupShopOne mImpl;

        static GroupShopOne()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (GroupShopOne), typeof (GroupShopOneDefaultImpl),
                o => { mImpl = (IGroupShopOne) o; });
        }

        public void InitByBase(GroupShopRecord tbTable)
        {
            mImpl.InitByBase(this, tbTable);
        }

        public void InitByDB(DBGroupShopOne dbData)
        {
            mImpl.InitByDB(this, dbData);
        }

        #endregion

        #region 对外方法

        public bool CheckCountLimit(ulong cId, int buyCount)
        {
            return mImpl.CheckCountLimit(this, cId, buyCount);
        }

        //某玩家买了一次
        public int CharacterBuy(ulong cId, int buyCount)
        {
            return mImpl.CharacterBuy(this, cId, buyCount);
        }

        //网络类型转换
        public GroupShopItemOne GetNetData(ulong cId)
        {
            return mImpl.GetNetData(this, cId);
        }

        public void OverCopy(GroupShopOne one)
        {
            mImpl.OverCopy(this, one);
        }

        #endregion
    }

    public interface IGroupShop
    {
        void FlushAll();
        string GetDbName(string keyName);
        List<long> GetExpired(List<long> ids);
        List<GroupShopOne> GetHistorys(List<long> ids);
        GroupShopOne GetItem(long guid);
        List<GroupShopOne> GetItems(List<long> ids);

        /// <summary>
        ///     获得道具列表
        /// </summary>
        /// <param name="characterId">角色id</param>
        /// <param name="type">0-3，表示要哪个级别的商品列表</param>
        /// <param name="profession">表示是哪个职业</param>
        /// <param name="cachedItems">Logic上缓存的商品id列表</param>
        /// <param name="itemList">返回商品列表</param>
        /// <param name="dirty">返回Logic上缓存的商品是否有变化</param>
        /// <returns>返回错误码</returns>
        ErrorCodes GetList(ulong characterId,
                           int type,
                           int profession,
                           List<long> cachedItems,
                           out GroupShopItemList itemList,
                           out bool dirty);

        ErrorCodes GetNewList(ulong characterId,
                              int type,
                              int profession,
                              List<long> cachedItems,
                              out GroupShopItemList itemList);

        long GetNextGuid();
        void Init();
        void ReloadTable(string tableName);
        void ResetShopItem(GroupShopOne one, long guid);
        IEnumerator SaveDB(Coroutine co);
    }

    public class GroupShopDefaultImpl : IGroupShop
    {
        #region 数据

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); //

        #endregion

        #region 初始化

        public void Init()
        {
            ReInitTopTimes();
            InitStoreList();
            CoroutineFactory.NewCoroutine(Get).MoveNext();
#if DEBUG
            TeamServerControl.tm.CreateTrigger(StaticParam.FirstSaveDbTime, FlushAll, 60000); //每1分钟存储一次
#else
            TeamServerControl.tm.CreateTrigger(StaticParam.FirstSaveDbTime, FlushAll, 60000 * 30);//每30分钟存储一次
#endif
        }

        private void ReInitTopTimes()
        {
            var tbGsu = Table.GetGroupShopUpdate(3);
            var topGoods = tbGsu.Goods.Where(i => i >= 0);
            var ids = new List<int>();
            foreach (var topGood in topGoods)
            {
                var tbSU = Table.GetSkillUpgrading(topGood);
                if (tbSU != null)
                {
                    ids.AddRange(tbSU.Values);
                }
                else
                {
                    Logger.Error("In ReInitTopTimes().tbSU == null! id = {0}", topGood);
                }
            }
            GroupShop.TopItems.Clear();
            GroupShop.TopItems.AddRange(ids.Distinct());
            GroupShop.TopItems.Remove(-1);
        }

        private void InitByBase()
        {
            GroupShop.DbData = new DBGroupShop();
            GroupShop.DbData.Notifys = new StringArray();
            RefreshItem();
        }

        private void InitByDb(DBGroupShop dbData)
        {
            GroupShop.DbData = dbData;
            var delList = new List<DBGroupShopOne>();
            foreach (var item in GroupShop.DbData.ItemList)
            {
                List<GroupShopOne> list;
                if (GroupShop.StoreList.TryGetValue(item.GroupShopId, out list))
                {
                    var temp = new GroupShopOne();
                    temp.InitByDB(item);
                    GroupShop.Datas.Add(temp.Guid, temp);
                    list.Add(temp);
                }
                else
                {
                    delList.Add(item);
                }
            }
            if (delList.Count > 0)
            {
                GroupShop.DbData.ItemList.RemoveAll(i => delList.Contains(i));
            }
            //移除过期的历史
            var overTime = DateTime.Now.AddDays(-7).ToBinary();
            var count = GroupShop.DbData.OldItems.RemoveAll(item => item.OverTime <= overTime);
            if (count > 0)
            {
                GroupShop.Dirty = true;
            }
            foreach (var item in GroupShop.DbData.OldItems)
            {
                var temp = new GroupShopOne();
                temp.InitByDB(item);
                if (GroupShop.OldDatas.ContainsKey(temp.Guid))
                {
                    PlayerLog.WriteLog((int) LogType.GroupShopOldDatasKeySame, "GroupShop.OldDatas has duplicate key!");
                    Logger.Fatal(
                        "----------------------------GroupShop.OldDatas has duplicate key!--------------------------------");
                    continue;
                }
                GroupShop.OldDatas.Add(temp.Guid, temp);
            }
            CheckTableChange();
        }

        //如果表格加行了，就把缺失的商品补上来
        private void CheckTableChange()
        {
            Table.ForeachGroupShop(record =>
            {
                var imax = record.RefleshCount - GroupShop.StoreList[record.Id].Count;
                if (imax > 0)
                {
                    for (var i = 0; i < imax; ++i)
                    {
                        AddItem(record);
                    }
                }
                return true;
            });
            FlushAll();
        }

        //DB名称获取
        public string GetDbName(string keyName)
        {
            return string.Format("GroupShop_{0}", keyName);
        }

        public void FlushAll()
        {
            if (!GroupShop.Dirty)
            {
                return;
            }
            CoroutineFactory.NewCoroutine(SaveDB).MoveNext();
        }

        public IEnumerator SaveDB(Coroutine co)
        {
            if (GroupShop.DbData != null)
            {
                var ret = TeamServer.Instance.DB.Set(co, DataCategory.TeamShop, GetDbName(GroupShop.DBKeyName),
                    GroupShop.DbData);
                yield return ret;
                if (ret.Status == DataStatus.Ok)
                {
                    GroupShop.Dirty = false;
                }
            }
        }

        private IEnumerator Get(Coroutine co)
        {
            var tasks = TeamServer.Instance.DB.Get<DBGroupShop>(co, DataCategory.TeamShop,
                GetDbName(GroupShop.DBKeyName));
            yield return tasks;
            if (tasks.Data == null)
            {
                InitByBase();
            }
            else
            {
                InitByDb(tasks.Data);
            }
        }

        private void InitStoreList()
        {
            Table.ForeachGroupShop(record =>
            {
                GroupShop.StoreList[record.Id] = new List<GroupShopOne>();
                return true;
            });
        }

        private void RefreshItem()
        {
            Table.ForeachGroupShop(record =>
            {
                for (var i = 0; i != record.RefleshCount; ++i)
                {
                    AddItem(record);
                }
                return true;
            });
            FlushAll();
        }

        #endregion

        #region 对外方法

        //重置内容
        public ErrorCodes GetNewList(ulong characterId,
                                     int type,
                                     int profession,
                                     List<long> cachedItems,
                                     out GroupShopItemList itemList)
        {
            itemList = new GroupShopItemList();

            var tbGSU = Table.GetGroupShopUpdate(type);
            if (tbGSU == null)
            {
                return ErrorCodes.Error_DataOverflow;
            }

            var i = 0;
            foreach (var tableId in tbGSU.Goods)
            {
                if (tableId < 0)
                {
                    break;
                }
                var count = tbGSU.Count[i];
                if (count <= 0)
                {
                    continue;
                }
                var id = GetGroupShopId(tableId, profession);
                List<GroupShopOne> tempList;
                if (GroupShop.StoreList.TryGetValue(id, out tempList))
                {
                    var selectList = tempList.RandRange(0, count);
                    foreach (var one in selectList)
                    {
                        itemList.Items.Add(one.GetNetData(characterId));
                    }
                }
                ++i;
            }

            return ErrorCodes.OK;
        }

        private class SortedGroupShopItemOne
        {
            public int Id;
            public GroupShopItemOne Item;
        }

        public ErrorCodes GetList(ulong characterId,
                                  int type,
                                  int profession,
                                  List<long> cachedItems,
                                  out GroupShopItemList itemList,
                                  out bool dirty)
        {
            dirty = false;
            if (cachedItems.Count == 0)
            {
                dirty = true;
                var err = GetNewList(characterId, type, profession, cachedItems, out itemList);
                return err;
            }

            itemList = new GroupShopItemList();
            var tbGSU = Table.GetGroupShopUpdate(type);
            if (tbGSU == null)
            {
                return ErrorCodes.Error_DataOverflow;
            }

            var items = new Dictionary<int, int>();
            var sortedItems = new List<SortedGroupShopItemOne>();
            var index = 0;
            foreach (var tableId in tbGSU.Goods)
            {
                if (tableId < 0)
                {
                    break;
                }
                var id = GetGroupShopId(tableId, profession);
                var count = tbGSU.Count[index];
                items.modifyValue(id, count);
                index++;
            }

            //检查已有的内容
            foreach (var l in cachedItems)
            {
                var temp = GetItem(l);
                if (temp != null)
                {
                    var id = temp.tbGroupShop.Id;
                    items.modifyValue(id, -1);
                    sortedItems.Add(new SortedGroupShopItemOne
                    {
                        Id = id,
                        Item = temp.GetNetData(characterId)
                    });
                }
            }

            //补充新内容
            foreach (var i in items)
            {
                if (i.Value <= 0)
                {
                    continue;
                }
                List<GroupShopOne> tempList;
                if (GroupShop.StoreList.TryGetValue(i.Key, out tempList))
                {
                    for (var j = 0; j < i.Value; ++j)
                    {
                        var newList = new List<GroupShopOne>();
                        foreach (var one in tempList)
                        {
                            if (one.State != (int) eGroupShopItemState.OnSell)
                            {
                                continue;
                            }
                            if (sortedItems.All(item => one.Guid != item.Item.Guid))
                            {
                                newList.Add(one);
                            }
                        }
                        if (newList.Count <= 0)
                        {
                            Logger.Error("In GetList().newList.Count <= 0! id = {0}, j = {1}", i.Key, j);
                            continue;
                        }
                        dirty = true;
                        var select = newList.Range();
                        sortedItems.Add(new SortedGroupShopItemOne
                        {
                            Id = i.Key,
                            Item = select.GetNetData(characterId)
                        });
                    }
                }
                else
                {
                    Logger.Error("In GetList(). GroupShop.StoreList.TryGetValue(i.Key, out tempList) return false!");
                }
            }

            // sort
            sortedItems.Sort((l, r) =>
            {
                if (l.Id < r.Id)
                {
                    return -1;
                }
                if (l.Id > r.Id)
                {
                    return 1;
                }
                return 0;
            });

            // 把 sortedItems 中的内容挪到 itemListItems 里
            var itemListItems = itemList.Items;
            itemListItems.AddRange(sortedItems.Select(one => one.Item));
            return ErrorCodes.OK;
        }

        //获取一个道具
        public GroupShopOne GetItem(long guid)
        {
            GroupShopOne gsOne;
            if (GroupShop.Datas.TryGetValue(guid, out gsOne))
            {
                return gsOne;
            }
            return null;
        }

        //获取一堆道具
        public List<GroupShopOne> GetItems(List<long> ids)
        {
            GroupShopOne one = null;
            return (from id in ids where GroupShop.Datas.TryGetValue(id, out one) select one).ToList();
        }

        //获取一堆历史道具
        public List<GroupShopOne> GetHistorys(List<long> ids)
        {
            GroupShopOne one = null;
            return (from id in ids where GroupShop.OldDatas.TryGetValue(id, out one) select one).ToList();
        }

        //从ids中获取已经不在当前商品池中的商品id
        public List<long> GetExpired(List<long> ids)
        {
            var keys = GroupShop.Datas.Keys;
            return ids.Where(k => !keys.Contains(k)).ToList();
        }

        public void ReloadTable(string tableName)
        {
            if (tableName == "GroupShopUpdate")
            {
                ReInitTopTimes();
            }
            else if (tableName == "GroupShop")
            {
                CheckTableChange();
            }
        }

        public long GetNextGuid()
        {
            return ++GroupShop.DbData.NextGuid;
        }

        //更换guid
        public void ResetShopItem(GroupShopOne one, long guid)
        {
            //老的
            if (one.NowCount > 0)
            {
                var temp = new GroupShopOne();
                temp.OverCopy(one);
                GroupShop.DbData.OldItems.Add(temp.mDbData);
                GroupShop.OldDatas.Add(temp.Guid, temp);
            }

            //新的
            GroupShop.Datas.Remove(one.Guid);
            one.Guid = guid;
            one.mDbData.LuckyId = 0;
            one.mDbData.LuckyCount = 0;
            one.mDbData.LuckyName = string.Empty;
            one.mDbData.Characters.Clear();
            one.CharactersCount.Clear();
            one.State = (int) eGroupShopItemState.OnSell;
            GroupShop.Datas.Add(guid, one);
            GroupShop.Dirty = true;
        }

        #endregion

        #region 私有方法

        private void AddItem(GroupShopRecord tbGroupShop)
        {
            var nextGuid = GetNextGuid();
            var temp = new GroupShopOne();
            temp.InitByBase(tbGroupShop);
            temp.Guid = nextGuid;
            GroupShop.DbData.ItemList.Add(temp.mDbData);
            GroupShop.Datas.Add(nextGuid, temp);
            var tempList = GroupShop.StoreList[tbGroupShop.Id];
            tempList.Add(temp);
            GroupShop.Dirty = true;
        }

        private int GetGroupShopId(int skillUpgradingId, int profession)
        {
            var tbSU = Table.GetSkillUpgrading(skillUpgradingId);
            if (tbSU == null)
            {
                return -1;
            }
            if (profession == -1)
            {
                profession = MyRandom.Random(3);
            }
            return tbSU.GetSkillUpgradingValue(profession);
        }

        #endregion
    }

    public static class GroupShop
    {
        #region 数据

        public static DBGroupShop DbData;
        public static Dictionary<long, GroupShopOne> Datas = new Dictionary<long, GroupShopOne>(); //按ID整理

        public static Dictionary<int, List<GroupShopOne>> StoreList = new Dictionary<int, List<GroupShopOne>>();
            //按商品所在许愿池等级整理

        public static Dictionary<long, GroupShopOne> OldDatas = new Dictionary<long, GroupShopOne>();
        //最高级的团购id，最高级的团购，需要发全服通知
        public static List<int> TopItems = new List<int>();

        public static string DBKeyName = string.Empty;
        public static bool Dirty;

        #endregion

        #region 初始化

        private static IGroupShop mImpl;

        static GroupShop()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (GroupShop), typeof (GroupShopDefaultImpl),
                o => { mImpl = (IGroupShop) o; });
        }

        public static void Init()
        {
            mImpl.Init();
        }

        //DB名称获取
        public static string GetDbName(string keyName)
        {
            return mImpl.GetDbName(keyName);
        }

        public static IEnumerator SaveDB(Coroutine co)
        {
            return mImpl.SaveDB(co);
        }

        #endregion

        #region 对外方法

        /// <summary>
        ///     获得道具列表
        /// </summary>
        /// <param name="characterId">角色id</param>
        /// <param name="type">0-3，表示要哪个级别的商品列表</param>
        /// <param name="profession">表示是哪个职业</param>
        /// <param name="cachedItems">Logic上缓存的商品id列表</param>
        /// <param name="itemList">返回商品列表</param>
        /// <param name="dirty">返回Logic上缓存的商品是否有变化</param>
        /// <returns>返回错误码</returns>
        public static ErrorCodes GetList(ulong characterId,
                                         int type,
                                         int profession,
                                         List<long> cachedItems,
                                         out GroupShopItemList itemList,
                                         out bool dirty)
        {
            return mImpl.GetList(characterId, type, profession, cachedItems, out itemList, out dirty);
        }

        //获取一个道具
        public static GroupShopOne GetItem(long guid)
        {
            return mImpl.GetItem(guid);
        }

        //获取一堆道具
        public static List<GroupShopOne> GetItems(List<long> ids)
        {
            return mImpl.GetItems(ids);
        }

        //获取一堆历史道具
        public static List<GroupShopOne> GetHistorys(List<long> ids)
        {
            return mImpl.GetHistorys(ids);
        }

        //从ids中获取已经不在当前商品池中的商品id
        public static List<long> GetExpired(List<long> ids)
        {
            return mImpl.GetExpired(ids);
        }

        public static void ReloadTable(string tableName)
        {
            mImpl.ReloadTable(tableName);
        }

        public static long GetNextGuid()
        {
            return mImpl.GetNextGuid();
        }

        //更换guid
        public static void ResetShopItem(GroupShopOne one, long guid)
        {
            mImpl.ResetShopItem(one, guid);
        }

        #endregion
    }
}