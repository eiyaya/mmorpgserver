#region using

using System;
using System.Collections;
using System.Collections.Generic;
using DataContract;
using DataTable;
using EventSystem;
using NLog;
using Shared;

#endregion

namespace Logic
{
    public interface IStoreItem
    {
        void StoreItem(StoreItem _this, DBStore dbdata);
        void StoreItem(StoreItem _this, int nId);
    }

    public class StoreItemDefaultImpl : IStoreItem
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //构造商店
        public void StoreItem(StoreItem _this, DBStore dbdata)
        {
            _this.tbStore = Table.GetStore(dbdata.Id);
            _this.mDbData = dbdata;
        }

        public void StoreItem(StoreItem _this, int nId)
        {
            _this.tbStore = Table.GetStore(nId);
            if (_this.tbStore == null)
            {
                Logger.Error("StoreId={0} not find", nId);
                return;
            }
            _this.mDbData = new DBStore();
            _this.Id = nId;
            //if (tbStore.EventId != -1)
            //{

            //}
        }
    }

    public class StoreItem
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static IStoreItem mImpl;

        static StoreItem()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (StoreItem), typeof (StoreItemDefaultImpl),
                o => { mImpl = (IStoreItem) o; });
        }

        //构造商店
        public StoreItem(DBStore dbdata)
        {
            mImpl.StoreItem(this, dbdata);
        }

        public StoreItem(int nId)
        {
            mImpl.StoreItem(this, nId);
        }

        public int Id
        {
            get { return mDbData.Id; }
            set { mDbData.Id = value; }
        }

        public DBStore mDbData { get; set; }
        public StoreRecord tbStore { get; set; }

        public long TriggerTime
        {
            get { return mDbData.NextTime; }
            set { mDbData.NextTime = value; }
        }
    }

    public interface IStoneManager
    {
        ErrorCodes BuyEquipItem(StoneManager _this, int id, int bagId, int bagIndex, ref int isAddEquip);
        ErrorCodes BuyItem(StoneManager _this, int id, int count);
        void EventTrigger(StoneManager _this, int eventId);
        StoreItem GetItem(StoneManager _this, int id);
        void GetItemList(StoneManager _this, int type, List<StoneItem> items);
        StoreItem GetStoreItem(StoneManager _this, int groupId);
        void Init();
        DBStores InitByBase(StoneManager _this, CharacterController character);
        void InitByDB(StoneManager _this, CharacterController character, DBStores storeData);
        bool ResetGroup(StoneManager _this, int GroupId, out StoreItem newItem);
        bool ResetItem(StoneManager _this, int id);
        void ResetStoreItem(StoneManager _this);
        void ResetStoreItemByDb(StoneManager _this, DBStores storeData);
        void SetGroup(StoneManager _this, StoreRecord oldStore, StoreItem Item);
    }

    public class StoneManagerDefaultImpl : IStoneManager
    {
        #region   静态数据

        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Logger kafaLogger = LogManager.GetLogger(Shared.LoggerName.KafkaLog);
        //静态数据初始化
        public void Init()
        {
            StoneManager.StoneNoGroup.Clear();
            StoneManager.StoneGroup.Clear();
            Table.ForeachStore(record =>
            {
                if (record.GroupId == -1)
                {
                    List<StoreItem> tempList;
                    if (!StoneManager.StoneNoGroup.TryGetValue(record.Type, out tempList))
                    {
                        tempList = new List<StoreItem>();
                        StoneManager.StoneNoGroup[record.Type] = tempList;
                    }
                    tempList.Add(new StoreItem(record.Id));
                }
                else
                {
                    List<StoreItem> tempList;
                    if (!StoneManager.StoneGroup.TryGetValue(record.GroupId, out tempList))
                    {
                        tempList = new List<StoreItem>();
                        StoneManager.StoneGroup[record.GroupId] = tempList;
                    }
                    tempList.Add(new StoreItem(record.Id));
                }
                return true;
            });
        }

        #endregion

        #region 初始化

        //用第一次创建
        public DBStores InitByBase(StoneManager _this, CharacterController character)
        {
            _this.mDbData = new DBStores();
            _this.mCharacter = character;
            ResetStoreItem(_this);
            _this.MarkDirty();
            return _this.mDbData;
        }

        //用数据库数据
        public void InitByDB(StoneManager _this, CharacterController character, DBStores storeData)
        {
            _this.mCharacter = character;
            _this.mDbData = storeData;
            ResetStoreItemByDb(_this, storeData);
        }

        //重置商店内容
        public void ResetStoreItem(StoneManager _this)
        {
          
            foreach (var pair in StoneManager.StoneGroup)
            {
                var temp = pair.Value.Range();
                SetGroup(_this, temp.tbStore, temp);
            }
        }

        //获得某个组的商店
        public StoreItem GetStoreItem(StoneManager _this, int groupId)
        {
            var delList = new List<int>();
            StoreItem first = null;
            foreach (var groupData in _this.GroupDatas)
            {
                if (groupData.Value.tbStore.GroupId == groupId)
                {
                    if (first == null)
                    {
                        first = groupData.Value;
                    }
                    else
                    {
                        delList.Add(groupData.Key);
                    }
                }
            }
            foreach (var i in delList)
            {
                _this.GroupDatas.Remove(i);
            }
            return first;
        }

        //重置商店内容
        public void ResetStoreItemByDb(StoneManager _this, DBStores storeData)
        {
            //初始化商店
            var delList = new List<int>();
            foreach (var dbDatas in _this.mDbData.Items)
            {
                var tbStore = Table.GetStore(dbDatas.Key);
                if (tbStore == null)
                {
                    delList.Add(dbDatas.Key);
                    continue;
                }
                if (tbStore.GroupId == -1)
                {
                    delList.Add(dbDatas.Key);
                }
            }
          
            //如果有表格找不到的则干掉
            foreach (var i in delList)
            {
                _this.mDbData.Items.Remove(i);
            }
          
            //如果新的没有找到，需要新加
            foreach (var pair in StoneManager.StoneGroup)
            {
                var temp = GetStoreItem(_this, pair.Key);
                if (temp == null)
                {
                    var temp2 = pair.Value.Range();
                    SetGroup(_this, temp2.tbStore, temp2);
                }
            }
        }

        #endregion

        #region  商店方法

        public void GetItemList(StoneManager _this, int type, List<StoneItem> items)
        {
            var roleType = _this.mCharacter.GetRole();
            foreach (var item in _this.GroupDatas)
            {
                if (type == item.Value.tbStore.Type)
                {
                    if (BitFlag.GetLow(item.Value.tbStore.SeeCharacterID, roleType) == false)
                    {
                        continue;
                    }

                    if (item.Value.tbStore.FuBenCount >= 0)
                        continue;
//                     if (item.Value.tbStore.DisplayCondition != -1)
//                     {
//                         var cc = _this.mCharacter.CheckCondition(item.Value.tbStore.DisplayCondition);
//                         if (cc != -2)
//                         {
//                             continue;
//                         }
//                     }
                    var tempItem = new StoneItem();
                    tempItem.itemid = item.Value.Id;
                    tempItem.itemcount = -1;
                    items.Add(tempItem);
                }
                //if (type == -1 || type == item.Value.tbStore.Type)
                //{
                //    items.Add(item.Value.Id);
                //}
            }

            List<StoreItem> tempList;
            if (StoneManager.StoneNoGroup.TryGetValue(type, out tempList))
            {
                foreach (var item in tempList)
                {
                    if (BitFlag.GetLow(item.tbStore.SeeCharacterID, roleType) == false)
                    {
                        continue;
                    }
                    if (item.tbStore.FuBenCount >= 0)
                        continue;

                    //                     if (item.tbStore.DisplayCondition != -1)
//                     {
//                         var cc = _this.mCharacter.CheckCondition(item.tbStore.DisplayCondition);
//                         if (cc != -2)
//                         {
//                             continue;
//                         }
//                     }
                    var tempItem = new StoneItem();
                    tempItem.itemid = item.Id;
                    tempItem.itemcount = -1;
                    items.Add(tempItem);
                }
            }
        }

        //获取一个老物品
        public StoreItem GetItem(StoneManager _this, int id)
        {
            StoreItem oldItem;
            if (_this.GroupDatas.TryGetValue(id, out oldItem))
            {
                return oldItem;
            }
            return null;
        }

        //重置道具Id
        public bool ResetItem(StoneManager _this, int id)
        {
            var oldItem = GetItem(_this, id);
            if (oldItem == null)
            {
                return false;
            }
            var tbStore = oldItem.tbStore;
            if (tbStore == null)
            {
                return false;
            }
            if (tbStore.EventId == -1)
            {
                return false;
            }
            if (tbStore.GroupId != -1)
            {
                StoreItem newItem;
                var result = ResetGroup(_this, tbStore.GroupId, out newItem);
                if (result)
                {
                    SetGroup(_this, tbStore, newItem);
                }
                return result;
            }
            return true;
        }

        //重置组Id
        public bool ResetGroup(StoneManager _this, int GroupId, out StoreItem newItem)
        {
            List<StoreItem> tempList;
            if (StoneManager.StoneGroup.TryGetValue(GroupId, out tempList))
            {
                if (tempList.Count > 0)
                {
                    newItem = tempList.Range();
                    return true;
                }
            }
            newItem = null;
            return false;
        }

        //重置组内容
        public void SetGroup(StoneManager _this, StoreRecord oldStore, StoreItem Item)
        {
            _this.GroupDatas.Remove(oldStore.Id);
            _this.GroupDatas[Item.Id] = Item;
            _this.mDbData.Items.Remove(oldStore.Id);
            _this.mDbData.Items[Item.Id] = Item.mDbData;
        }

        //买东西
        public ErrorCodes BuyEquipItem(StoneManager _this, int id, int bagId, int bagIndex, ref int isAddEquip)
        {
            var tbStore = Table.GetStore(id);
            if (tbStore == null)
            {
                return ErrorCodes.Error_StoreID;
            }
            //检查职业
            var roleType = _this.mCharacter.GetRole();
            if (BitFlag.GetLow(tbStore.SeeCharacterID, roleType) == false)
            {
                return ErrorCodes.RoleIdError;
            }
            //检查条件
            if (tbStore.DisplayCondition != -1)
            {
                var retCond = _this.mCharacter.CheckCondition(tbStore.DisplayCondition);
                if (retCond != -2)
                {
                    return ErrorCodes.Error_ConditionNoEnough;
                }
            }
            var count = 1;
            //限购
            if (tbStore.DayCount >= 0)
            {
                if (_this.mCharacter.GetExData(tbStore.DayCount) < count)
                {
                    return ErrorCodes.Error_StoreBuyCountMax;
                }
            }
            if (tbStore.WeekCount >= 0)
            {
                if (_this.mCharacter.GetExData(tbStore.WeekCount) < count)
                {
                    return ErrorCodes.Error_StoreBuyCountMax;
                }
            }
            if (tbStore.MonthCount >= 0)
            {
                if (_this.mCharacter.GetExData(tbStore.MonthCount) < count)
                {
                    return ErrorCodes.Error_StoreBuyCountMax;
                }
            }
            //资源是否足够
            var nowCount = _this.mCharacter.mBag.GetItemCount(tbStore.NeedType);
            var needCount = tbStore.NeedValue*count;
            if (nowCount < needCount)
            {
                return ErrorCodes.Error_ResNoEnough;
            }

            //组判断
            if (tbStore.GroupId != -1)
            {
                if (!_this.GroupDatas.ContainsKey(id))
                {
                    return ErrorCodes.Error_StoreNotHaveItem;
                }
            }
            if (tbStore.NeedItem == -1)
            {
                return ErrorCodes.Unknow;
            }
            var item = _this.mCharacter.GetItemByBagByIndex(bagId, bagIndex);
            if (item == null || item.GetId() == -1 || !(item is ItemEquip2))
            {
                return ErrorCodes.Error_ResNoEnough;
            }
            //物品是否足够
            if (item.GetId() != tbStore.NeedItem)
            {
                return ErrorCodes.Error_ResNoEnough;
            }
            var oldEquip = Table.GetEquip(item.GetId());
            var nextEquip = Table.GetEquip(tbStore.ItemId);

            if (oldEquip.Part != nextEquip.Part || nextEquip.Occupation != oldEquip.Occupation)
            {
                //添加道具
                var result = _this.mCharacter.mBag.CheckAddItem(tbStore.ItemId, tbStore.ItemCount*1);
                if (result != ErrorCodes.OK)
                {
                    if (result != ErrorCodes.OK)
                    {
                        return result;
                    }
                }
                _this.mCharacter.mBag.AddItem(tbStore.ItemId, tbStore.ItemCount*1, eCreateItemType.StoreBuy);
                _this.mCharacter.GetBag(bagId).ReduceCountByIndex(bagIndex, 1, eDeleteItemType.StoreBuy);
                _this.mCharacter.EquipChange(0, bagId, bagIndex, item);
                isAddEquip = 1;
            }
            else
            {
                isAddEquip = 0;
                //设置道具
                item.SetId(tbStore.ItemId);
                item.MarkDbDirty();
                //通知属性变化
                _this.mCharacter.EquipChange(2, bagId, bagIndex, item);
            }

            //扣除资源
            _this.mCharacter.mBag.DeleteItem(tbStore.NeedType, needCount, eDeleteItemType.StoreBuy);

            try
            {
                //记录商店购买日志
                // server, account, charname, charid, goodsname, buycount, pricetype, totalprice, logtime
                string v = string.Format("gamegoodslog#{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}",
                            _this.mCharacter.serverId,
                            "account",
                            _this.mCharacter.GetName(),
                            _this.mCharacter.mGuid,
                            ((int)tbStore.ItemId + 1) * 1000,
                            "",
                            tbStore.ItemCount * count,
                            (int)tbStore.NeedType,
                            needCount,
                            DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")); // 时间
                kafaLogger.Info(v);
            }
            catch (Exception)
            {
            }

            var e = new BuyItemEvent(_this.mCharacter, tbStore.ItemId, count);
            EventDispatcher.Instance.DispatchEvent(e);
            //限购次数设置
            if (tbStore.DayCount >= 0)
            {
                _this.mCharacter.AddExData(tbStore.DayCount, -count);
            }
            if (tbStore.WeekCount >= 0)
            {
                _this.mCharacter.AddExData(tbStore.WeekCount, -count);
            }
            if (tbStore.MonthCount >= 0)
            {
                _this.mCharacter.AddExData(tbStore.MonthCount, -count);
            }
            //设置标记位
            if (tbStore.BugSign != -1)
            {
                _this.mCharacter.SetFlag(tbStore.BugSign, true, 1);
            }
            return ErrorCodes.OK;
        }

        //买东西
        public ErrorCodes BuyItem(StoneManager _this, int id, int count)
        {
            var tbStore = Table.GetStore(id);
            if (tbStore == null)
            {
                return ErrorCodes.Error_StoreID;
            }
            int NeedType = tbStore.NeedType;
            int NeedValue = tbStore.NeedValue;


            //检查职业
            var character = _this.mCharacter;
            var bag = character.mBag;
            var roleType = character.GetRole();
            if (BitFlag.GetLow(tbStore.SeeCharacterID, roleType) == false)
            {
                return ErrorCodes.RoleIdError;
            }
            //检查条件
            if (tbStore.DisplayCondition != -1)
            {
                var retCond = character.CheckCondition(tbStore.DisplayCondition);
                if (retCond != -2)
                {
                    return ErrorCodes.Error_ConditionNoEnough;
                }
            }
            //限购
            var vipLevel = character.mBag.GetRes(eResourcesType.VipLevel);
            var tbVip = Table.GetVIP(vipLevel);
            var idx = -1;
            for (int i = 0, imax = tbVip.BuyItemId.Length; i < imax; i++)
            {
                if (tbVip.BuyItemId[i] == id)
                {
                    idx = i;
                    break;
                }
            }
            var vipAddCount = idx < 0 ? 0 : tbVip.BuyItemCount[idx];
            if (tbStore.DayCount >= 0)
            {
                if (character.GetExData(tbStore.DayCount) + vipAddCount < count)
                {
                    return ErrorCodes.Error_StoreBuyCountMax;
                }
            }
            if (tbStore.WeekCount >= 0)
            {
                if (character.GetExData(tbStore.WeekCount) + vipAddCount < count)
                {
                    return ErrorCodes.Error_StoreBuyCountMax;
                }
            }
            if (tbStore.MonthCount >= 0)
            {
                if (character.GetExData(tbStore.MonthCount) + vipAddCount < count)
                {
                    return ErrorCodes.Error_StoreBuyCountMax;
                }
            }
            //资源是否足够
            var nowCount = bag.GetItemCount(NeedType);
            var needCount = (long)NeedValue * count;
            //价格波动处理
            if(tbStore.WaveValue >= 0)
            {
                //从store表指到skillupgrading中读取次数2价格,然后把价格和购买个数重置
                int exIdx = -1 ;
                if (tbStore.DayCount >= 0)
                {
                    exIdx = tbStore.DayCount ;
                }
                else if (tbStore.WeekCount >= 0)
                {
                    exIdx = tbStore.WeekCount;                    
                }
                else if (tbStore.MonthCount >= 0)
                {
                    exIdx = tbStore.MonthCount;                    
                }
                if(exIdx>=0)
                {
                    var tbExdata = Table.GetExdata(exIdx);  
                    //已经购买的次数
                    int times = tbExdata.RefreshValue[0] - character.GetExData(exIdx);
                    var tbCost = Table.GetSkillUpgrading(tbStore.WaveValue);
                    if (tbCost == null)
                        return ErrorCodes.Error_StoreNotHaveItem;
                    if (times < 0 || tbCost.Values.Count <= 0 )
                        return ErrorCodes.Error_StoreBuyCountMax;
                    if (times >= tbCost.Values.Count)
                        times = tbCost.Values.Count - 1;

                    needCount = tbCost.GetSkillUpgradingValue(times);
                    count = 1;
                }
            }

            if (nowCount < needCount)
            {
                return ErrorCodes.Error_ResNoEnough;
            }

            //物品是否足够
            var nowItemCount = bag.GetItemCount(tbStore.NeedItem);
            var needItemCount = tbStore.NeedCount*count;
            if (nowItemCount < needItemCount)
            {
                return ErrorCodes.Error_ResNoEnough;
            }

            //组判断
            if (tbStore.GroupId != -1)
            {
                if (!_this.GroupDatas.ContainsKey(id))
                {
                    return ErrorCodes.Error_StoreNotHaveItem;
                }
            }
            //添加道具
            int level = _this.mCharacter.GetLevel();
            int tempItemCount = 0;
            tempItemCount = tbStore.ItemCount * count;
            if (tbStore.ItemId == (int)eResourcesType.DiamondRes)//祈福根据玩家等级获得不同的经验加成
            {
                if (level > 100)
                {
                    tempItemCount = tbStore.ItemCount * count + ((level - 100) / 10) * tbStore.BlessGrow;
                }                
            }            
            var result = bag.CheckAddItem(tbStore.ItemId, tempItemCount);
            if (result != ErrorCodes.OK)
            {
                if (result != ErrorCodes.OK)
                {
                    return result;
                }
            }
            bag.AddItem(tbStore.ItemId, tempItemCount, eCreateItemType.StoreBuy);
            var e = new BuyItemEvent(character, tbStore.ItemId, tempItemCount);
            EventDispatcher.Instance.DispatchEvent(e);
            //扣除资源
            bag.DeleteItem(NeedType, (int)needCount, eDeleteItemType.StoreBuy);
            if (NeedType == (int) eResourcesType.Contribution)
            {
                _this.mCharacter.AddExData(953,1);
            }
            //记录商店购买日志
            // server, account, charname, charid, goodsname, buycount, pricetype, totalprice, logtime

            try
            {
                string v = string.Format("gamegoodslog#{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}",
                            _this.mCharacter.serverId,
                            "account",
                            _this.mCharacter.GetName(),
                            _this.mCharacter.mGuid,
                            ((int)tbStore.ItemId + 1) * 1000,
                            "",
                            tempItemCount,
                            (int)NeedType,
                            needCount,
                            DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")); // 时间
                kafaLogger.Info(v);
            }
            catch (Exception)
            {
            }


            //限购次数设置
            if (tbStore.DayCount >= 0)
            {
                character.AddExData(tbStore.DayCount, -count);
            }
            if (tbStore.WeekCount >= 0)
            {
                character.AddExData(tbStore.WeekCount, -count);
            }
            if (tbStore.MonthCount >= 0)
            {
                character.AddExData(tbStore.MonthCount, -count);
            }
            if (tbStore.FuBenCount >= 0)
            {

            }
            //设置标记位
            if (tbStore.BugSign != -1)
            {
                character.SetFlag(tbStore.BugSign, true, 1);
            }

            _this.mCharacter.BroadCastGetEquip(tbStore.ItemId, 100002168);

            return ErrorCodes.OK;
        }

        //事件刷新
        public void EventTrigger(StoneManager _this, int eventId)
        {
            var refreshList = new List<StoreItem>();
            foreach (var groupData in _this.GroupDatas)
            {
                if (groupData.Value.tbStore.EventId == eventId)
                {
                    refreshList.Add(groupData.Value);
                }
            }
            foreach (var item in refreshList)
            {
                StoreItem newItem;
                var result = ResetGroup(_this, item.tbStore.GroupId, out newItem);
                if (result)
                {
                    SetGroup(_this, item.tbStore, newItem);
                }
            }
        }

        #endregion
    }

    public class StoneManager : NodeBase
    {
        public Dictionary<int, StoreItem> GroupDatas = new Dictionary<int, StoreItem>(); //ID,商店物品
        public CharacterController mCharacter; //所在角色
        //List<StoreItem> SimpleDatas = new List<StoreItem>();
        public DBStores mDbData;

        public override IEnumerable<NodeBase> Children
        {
            get { return null; }
        }

        #region   静态数据

        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static Dictionary<int, List<StoreItem>> StoneGroup = new Dictionary<int, List<StoreItem>>();
            //每天刷新，key=组ID，物品列表

        public static Dictionary<int, List<StoreItem>> StoneNoGroup = new Dictionary<int, List<StoreItem>>();
            //每天刷新，key=storeId，物品列表

        private static IStoneManager mImpl;

        static StoneManager()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (StoneManager), typeof (StoneManagerDefaultImpl),
                o => { mImpl = (IStoneManager) o; });
        }

        //静态数据初始化
        public static void Init()
        {
            mImpl.Init();
        }

        #endregion

        #region 初始化

        //用第一次创建
        public DBStores InitByBase(CharacterController character)
        {
            return mImpl.InitByBase(this, character);
        }

        //用数据库数据
        public void InitByDB(CharacterController character, DBStores storeData)
        {
            mImpl.InitByDB(this, character, storeData);
        }

        //重置商店内容
        public void ResetStoreItem()
        {
            mImpl.ResetStoreItem(this);
        }

        //获得某个组的商店
        public StoreItem GetStoreItem(int groupId)
        {
            return mImpl.GetStoreItem(this, groupId);
        }

        //重置商店内容
        public void ResetStoreItemByDb(DBStores storeData)
        {
            mImpl.ResetStoreItemByDb(this, storeData);
        }

        #endregion

        #region  商店方法

        public void GetItemList(int type, List<StoneItem> items)
        {
            mImpl.GetItemList(this, type, items);
        }

        //获取一个老物品
        public StoreItem GetItem(int id)
        {
            return mImpl.GetItem(this, id);
        }

        //重置道具Id
        public bool ResetItem(int id)
        {
            return mImpl.ResetItem(this, id);
        }

        //重置组Id
        public bool ResetGroup(int GroupId, out StoreItem newItem)
        {
            return mImpl.ResetGroup(this, GroupId, out newItem);
        }

        //重置组内容
        public void SetGroup(StoreRecord oldStore, StoreItem Item)
        {
            mImpl.SetGroup(this, oldStore, Item);
        }

        //买兑换装备
        public ErrorCodes BuyEquipItem(int id, int bagId, int bagIndex, ref int result)
        {
            return mImpl.BuyEquipItem(this, id, bagId, bagIndex, ref result);
        }

        //买东西
        public ErrorCodes BuyItem(int id, int count)
        {
            return mImpl.BuyItem(this, id, count);
        }

        //事件刷新
        public void EventTrigger(int eventId)
        {
            mImpl.EventTrigger(this, eventId);
        }

        #endregion
    }
}