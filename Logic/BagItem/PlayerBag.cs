#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DataContract;
using DataTable;
using EventSystem;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Logic
{

    #region 接口类

    public interface IBagManager
    {
        void AddExp(BagManager _this, int addExp, eCreateItemType createItemType);
        ErrorCodes AddItem(BagManager _this, ItemBaseData itemDb, eCreateItemType createItemType);
        ErrorCodes AddItem(BagManager _this, int nId, int nCount, eCreateItemType createItemType);

        ErrorCodes AddItemByMail(BagManager _this,
                                 int mailId,
                                 Dictionary<int, int> items,
                                 List<ItemBaseData> datas,
                                 eCreateItemType createItemType,
                                 string from);

        ErrorCodes AddMailItems(BagManager _this, int mailId, eCreateItemType createItemType);
        ErrorCodes AddRechargeRetMailItems(BagManager _this, int mailId,List<int> countList, eCreateItemType createItemType);

        

        ItemBase AddItemGetItem(BagManager _this, int nId, int nCount, eCreateItemType createItemType);

        ErrorCodes AddItemOrMail(BagManager _this,
                                 int mailId,
                                 Dictionary<int, int> items,
                                 List<ItemBaseData> datas,
                                 eCreateItemType createItemType,
                                 string from);

        ErrorCodes AddItems(BagManager _this, Dictionary<int, int> items, eCreateItemType createItemType);
        ErrorCodes AddItemToAstrologyBag(BagManager _this, int nId, int nCount, List<ItemBaseData> resultItems);
        ErrorCodes AddItemToElf(BagManager _this, int nId, int nCount, List<ItemBaseData> resultItems);
        ErrorCodes AddItemToWishingPool(BagManager _this, int nId, int nCount, List<ItemBaseData> resultItems);
        ErrorCodes AddRes(BagManager _this, eResourcesType type, int nCount, eCreateItemType createItemType);
        int CaculateVipLevel(BagManager _this, int exp, int level);
        ErrorCodes CheckAddItem(BagManager _this, int nId, int nCount);
        ErrorCodes CheckAddItemList(BagManager theBag, Dictionary<int, int> items);
        ErrorCodes DeleteItem(BagManager _this, int nId, int nCount, eDeleteItemType deleteItemType, string exData);
        ErrorCodes DelRes(BagManager _this, eResourcesType type, int nCount, eDeleteItemType createItemType, string exData = "");
        BagBase GetBag(BagManager _this, int BagId);
        int GetItemCount(BagManager _this, int nId);
        int GetItemCountEx(BagManager _this, int nId);
        int GetLevel(BagManager _this);
        void GetNetDirtyMissions(BagManager _this, BagsChangeData msg);
        int GetRes(BagManager _this, eResourcesType type);
        BagData InitByBase(BagManager _this, CharacterController character);
        void InitByDB(BagManager _this, CharacterController character, BagData bag);
        ErrorCodes MoveItem(BagManager _this, int lBagType, int lBagIndex, int rBagType, int rBagIndex, int itemCount);
        void NetDirtyHandle(BagManager _this);
        void RefreshElfAttr(BagManager _this);
        void RefreshGemAttr(BagManager _this);
        void RefreshMedal(BagManager _this);
        void ResChange(BagManager _this, eResourcesType type, int addValue);
        void SaveDB(BagManager _this);
        int SetExp(BagManager _this, int nExp);
        void SetRes(BagManager _this, eResourcesType type, int count);
        BagBase SortBag(BagManager _this, int BagId);
        void staticInit();
        void SwapItem(BagManager _this, ItemBase lItem, ItemBase rItem);
    }

    #endregion

    public class BagManagerDefaultImpl : IBagManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Logger kafaLogger = LogManager.GetLogger(Shared.LoggerName.KafkaLog);

        public void staticInit()
        {
            BagManager.staticBag.InitByBase(null);
            var str = Table.GetServerConfig(1351).Value.Trim();
            if (string.IsNullOrEmpty(str))
            {
                return;
            }
            var Ids = str.Split('|');
            foreach (var v in Ids)
            {
                BagManager.SoleItemList.Add(int.Parse(v));                
            }

        }

        public ErrorCodes CheckAddItemList(BagManager theBag, Dictionary<int, int> items)
        {
            //for (int i = 0; i != 6; ++i)
            foreach (var i in BagManager.BagInitItemList)
            {
                var playerBase = theBag.mBags[i];
                var count = playerBase.GetNowCount();
                var staticBase = BagManager.staticBag.mBags[i];
                staticBase.SetNowCount(count);
                for (var j = 0; j != count; ++j)
                {
                    staticBase.mLogics[j].SetId(playerBase.mLogics[j].GetId());
                    staticBase.mLogics[j].SetCount(playerBase.mLogics[j].GetCount());
                }
            }

            foreach (var item in items)
            {
                if (item.Value < 0)
                {
                    var result = BagManager.staticBag.DeleteItem(item.Key, item.Value, eDeleteItemType.CheckAdd);
                    if (result != ErrorCodes.OK)
                    {
                        return result;
                    }
                }
            }
            foreach (var item in items)
            {
                if (item.Value < 0)
                {
                    continue;
                }
                var result = BagManager.staticBag.AddItem(item.Key, item.Value, eCreateItemType.None);
                if (result != ErrorCodes.OK)
                {
                    return result;
                }
            }
            return ErrorCodes.OK;
        }

        #region  存储

        public void SaveDB(BagManager _this)
        {
            //BagData bag = mCharacter.DbCharacter.Bag;
            //if (mCharacter.DbCharacter.Bag == null)
            //{
            //    mCharacter.DbCharacter.Bag = new BagData();
            //    bag = mCharacter.DbCharacter.Bag;
            //}
            //bag.Resources.Clear();
            //bag.Resources.AddRange(mResources);
            //foreach (var bagBase in mBags)
            //{
            //    BagBaseData dbBagBase = null;
            //    if (!bag.Bags.TryGetValue(bagBase.Key, out dbBagBase))
            //    {
            //        dbBagBase = new BagBaseData();
            //        bag.Bags[bagBase.Key] = dbBagBase;
            //    }
            //    dbBagBase.BagId = bagBase.Key;
            //    dbBagBase.NowCount = bagBase.Value.GetNowCount();

            //    foreach (var itemBase in bagBase.Value.m_Items)
            //    {
            //        ItemBaseData dbBagItem = new ItemBaseData();
            //        dbBagItem.ItemId = itemBase.GetId();
            //        dbBagItem.Count = itemBase.GetCount();
            //        dbBagItem.Index = itemBase.GetIndex();
            //        dbBagBase.Items.Add(dbBagItem);
            //    }
            //}
        }

        #endregion

        public void NetDirtyHandle(BagManager _this)
        {
            //资源脏否
            if (_this.mResChange)
            {
                _this.mCharacter.Proxy.NotifGainRes(_this.mChanges);
                _this.mChanges.Chaneges.Clear();
                _this.mResChange = false;
            }
            //包裹脏否
            if (!_this.NetDirty)
            {
                return;
            }
            var msg = new BagsChangeData();
            foreach (var bag in _this.Children)
            {
                if (bag.NetDirty) //脏包裹
                {
                    var tempBag = (BagBase) bag;
                    var tempChanges = new ItemsChangeData();
                    msg.BagsChange[tempBag.GetBagId()] = tempChanges;

                    foreach (var item in bag.Children)
                    {
                        if (item.NetDirty) //赃物品
                        {
                            var tempItem = (ItemBase) item;
                            tempChanges.ItemsChange[tempItem.GetIndex()] = tempItem.mDbData;
                        }
                    }
                }
            }
            _this.mCharacter.Proxy.SyncItems(msg);
        }

        public void GetNetDirtyMissions(BagManager _this, BagsChangeData msg)
        {
            foreach (var bag in _this.Children)
            {
                if (bag.NetDirty) //脏包裹
                {
                    var tempBag = (BagBase) bag;
                    var tempChanges = new ItemsChangeData();
                    msg.BagsChange[tempBag.GetBagId()] = tempChanges;

                    foreach (var item in bag.Children)
                    {
                        if (item.NetDirty) //赃物品
                        {
                            var tempItem = (ItemBase) item;
                            tempChanges.ItemsChange[tempItem.GetIndex()] = tempItem.mDbData;
                        }
                    }
                }
            }
        }

        #region   初始化

        //初始化（按初始配置）
        public BagData InitByBase(BagManager _this, CharacterController character)
        {
            var dbData = new BagData();
            _this.mDbData = dbData;
            for (var i = 0; i != BagManager.mResCount; ++i)
            {
                var tbItem = Table.GetItemBase(i);
                if (tbItem == null)
                {
                    Logger.Error("Init Bag Resources Error itemid={0}!", i);
                    continue;
                }
                if (tbItem.Exdata[0] <= 0)
                {
                    _this.mDbData.Resources.Add(0);
                }
                else
                {
                    _this.mDbData.Resources.Add(tbItem.Exdata[0]);
                }
            }
            _this.mCharacter = character;
            //mResources = new int[mResCount];

            Table.ForeachBagBase(record =>
            {
                if (record.InitCapacity <= 0)
                {
                    return true;
                }
                var pBag = new BagBase(_this.mCharacter);
                _this.AddChild(pBag);
                if (record.Id == 4)
                {
                    pBag.InitByBase(record.Id, record.InitCapacity + 3);
                }
                else
                {
                    pBag.InitByBase(record.Id, record.InitCapacity);
                }
                _this.mBags[record.Id] = pBag;
                _this.mDbData.Bags.Add(pBag.GetBagId(), pBag.mDbData);
                return true;
            });
            //_this.mFlag = true;
            _this.MarkDirty();
            //剑士测试头盔	!!AddItem,200002,1
            //剑士测试项链	!!AddItem,201002,1
            //剑士测试胸甲	!!AddItem,204002,1
            //剑士测试戒指	!!AddItem,206002,1
            //剑士测试手套	!!AddItem,207002,1
            //剑士测试裤子	!!AddItem,208002,1
            //剑士测试鞋子	!!AddItem,209002,1
            //剑士测试主手	!!AddItem,210002,1
            //剑士测试副手	!!AddItem,211002,1
            //剑士测试单手	!!AddItem,212002,1
            ////剑士测试双手	!!AddItem,213002,1
            //AddItem(200002, 1);
            //AddItem(201002, 1);
            //AddItem(204002, 1);
            //AddItem(206002, 1);
            //AddItem(207002, 1);
            //AddItem(208002, 1);
            //AddItem(209002, 1);
            //AddItem(210002, 1);
            //AddItem(211002, 1);
            //AddItem(212002, 1);
            //AddItem(213002, 1);

            return dbData;
        }

        //初始化（按数据库配置）
        public void InitByDB(BagManager _this, CharacterController character, BagData bag)
        {
            _this.mCharacter = character;
            _this.mDbData = bag;
            for (var i = _this.mDbData.Resources.Count; i < BagManager.mResCount; ++i)
            {
                var tbItem = Table.GetItemBase(i);
                if (tbItem == null)
                {
                    Logger.Error("Init Bag Resources Error itemid={0}!", i);
                    continue;
                }
                if (tbItem.Exdata[0] < 0)
                {
                    _this.mDbData.Resources.Add(0);
                }
                else
                {
                    _this.mDbData.Resources.Add(tbItem.Exdata[0]);
                }
            }
            if (bag != null)
            {
                //mResources = bag.Resources.ToArray();
                foreach (var dbBagBase in bag.Bags)
                {
                    BagBase bagBase = null;
                    if (!_this.mBags.TryGetValue(dbBagBase.Key, out bagBase))
                    {
                        bagBase = new BagBase(character);
                        bagBase.InitByDB(dbBagBase.Value, character);
                        _this.mBags.Add(dbBagBase.Key, bagBase);
                        _this.AddChild(bagBase);
                    }
                    else
                    {
                        Logger.Warn("InitByDB! {0} is haved!", dbBagBase.Key);
                    }
                }
                //检查有没有表格中新配置的包裹
                Table.ForeachBagBase(record =>
                {
                    if (record.InitCapacity <= 0)
                    {
                        return true;
                    }
                    if (_this.mBags.ContainsKey(record.Id))
                    {
                        return true;
                    }
                    var pBag = new BagBase(_this.mCharacter);
                    _this.AddChild(pBag);
                    pBag.InitByBase(record.Id, record.InitCapacity);
                    _this.mBags[record.Id] = pBag;
                    _this.mDbData.Bags.Add(pBag.GetBagId(), pBag.mDbData);
                    return true;
                });
                //_this.mFlag = false;
            }
            RefreshElfAttr(_this);
            RefreshMedal(_this);
        }

        #endregion

        #region  道具相关(增删改查)

        //检查增加物品是否可行
        /// <summary>
        ///     检查增加物品是否可行（不能浪费主要)
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="nId">物品ID</param>
        /// <param name="nCount">数量</param>
        /// <returns></returns>
        public ErrorCodes CheckAddItem(BagManager _this, int nId, int nCount)
        {
            var tb_item = Table.GetItemBase(nId);
            if (tb_item == null)
            {
                return ErrorCodes.Error_ItemID;
            }

            if (tb_item.InitInBag < 0)
            {
//资源型数据
                return ErrorCodes.OK;
            }
            return _this.mBags[tb_item.InitInBag].CheckAddItem(nId, nCount);
        }

        //已经实例化好的道具给予
        public ErrorCodes AddItem(BagManager _this, ItemBaseData itemDb, eCreateItemType createItemType)
        {
            var nId = itemDb.ItemId;
            var tb_item = Table.GetItemBase(nId);
            if (tb_item == null)
            {
                return ErrorCodes.Error_ItemID;
            }
            var nCount = itemDb.Count;
            if (tb_item.InitInBag < 0)
            {
//资源型数据
                return AddRes(_this, (eResourcesType) nId, nCount, createItemType);
            }
            if (tb_item.Type == 60000) //潜规则宠物
            {
                var pet = _this.mBags[(int) eBagType.Pet].GetSamePetByPetId(nId);
                if (pet == null)
                {
                    _this.mBags[(int) eBagType.Pet].ForceAddItem(nId, 1, _this.mCharacter, createItemType);
                    pet = _this.mBags[(int) eBagType.Pet].GetSamePetByPetId(nId);
                    if (pet == null)
                    {
                        Logger.Error("AddItem By Pet id={0},count={1}", nId, nCount);
                        return ErrorCodes.Unknow;
                    }
                }
                if (pet.GetState() == (int) PetStateType.Piece)
                {
                    pet.SetId(nId);
                    pet.SetState(PetStateType.Idle);
                    pet.MarkDirty();
                    return ErrorCodes.OK;
                }
                var tbPet = Table.GetPet(nId);
                if (tbPet == null)
                {
                    return ErrorCodes.Unknow;
                }
                pet.AddPiece(tbPet.ResolvePartCount);
                return ErrorCodes.OK;
            }
            if (tb_item.Type == 70000) //潜规则宠物碎片
            {
                var pet = _this.mBags[(int) eBagType.Pet].GetSamePetByPetId(tb_item.Exdata[2]);
                if (pet == null)
                {
                    _this.mBags[(int) eBagType.Pet].ForceAddItem(tb_item.Exdata[2], 1, _this.mCharacter, createItemType);
                    pet = _this.mBags[(int) eBagType.Pet].GetSamePetByPetId(tb_item.Exdata[2]);
                    if (pet == null)
                    {
                        Logger.Error("AddItem By Pet id={0},count={1}", nId, nCount);
                        return ErrorCodes.Unknow;
                    }
                }
                pet.AddPiece(nCount);
                return ErrorCodes.OK;
            }
            var BagId = tb_item.InitInBag;
            if (eCreateItemType.BraveHarbor == createItemType)
            {
                BagId = Table.GetServerConfig(1099).ToInt();
            }

            var errorResult = _this.mBags[BagId].CheckAddItem(nId, nCount);
            if (errorResult == ErrorCodes.OK)
            {
                var nLast = _this.mBags[BagId].ForceAddItemByDb(itemDb, _this.mCharacter, createItemType);
                if (nLast != 0)
                {
                    Logger.Warn("AddItem Count Warn Id={0},Count={1}", nId, nCount);
                }
                if (BagId == (int)eBagType.Wing)
                {
                    if (_this.mCharacter != null)
                    {
                        _this.mCharacter.SetRankFlag(RankType.WingsFight);
                        _this.mCharacter.EquipChange(2, (int) eBagType.Wing, 0, _this.mCharacter.GetWing());
                    }
                }
            }
            return errorResult;
        }

        //添加道具
        /// <summary>
        ///     添加道具
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="nId"> 物品ID</param>
        /// <param name="nCount">物品数量</param>
        /// <param name="createItemType"></param>
        /// <returns></returns>
        public ErrorCodes AddItem(BagManager _this, int nId, int nCount, eCreateItemType createItemType)
        {
            var tb_item = Table.GetItemBase(nId);
            if (tb_item == null)
            {
                Logger.Error("AddItem Warning id={0},count={1},createItemType={2}", nId, nCount, createItemType);
                return ErrorCodes.Error_ItemID;
            }
            if (BagManager.SoleItemList.Find(n => { return n == nId; }) > 0 && _this.GetItemCountEx(nId)>0)
            {
                return ErrorCodes.OK;
            }




            if (tb_item.AutoUse > 0)
            {
                if(_this.mCharacter != null)
                    return _this.mCharacter.AutoUseItem(nId);
                else 
                    return ErrorCodes.OK;
            }



            if (tb_item.InitInBag < 0)
            {
//资源型数据
                return AddRes(_this, (eResourcesType) nId, nCount, createItemType);
            }
            if (tb_item.Type == 60000) //潜规则宠物
            {
                var pet = _this.mBags[(int) eBagType.Pet].GetSamePetByPetId(nId);
                if (pet == null)
                {
                    _this.mBags[(int) eBagType.Pet].ForceAddItem(nId, 1, _this.mCharacter, createItemType);
                    pet = _this.mBags[(int) eBagType.Pet].GetSamePetByPetId(nId);
                    if (pet == null)
                    {
                        Logger.Error("AddItem By Pet id={0},count={1}", nId, nCount);
                        return ErrorCodes.Unknow;
                    }
                }
                if (pet.GetState() == (int) PetStateType.Piece)
                {
                    pet.SetId(nId);
                    pet.SetState(PetStateType.Idle);
                    if (_this.mCharacter != null)
                    {
                        _this.mCharacter.AddExData((int) eExdataDefine.e330, 1);
                        _this.mCharacter.AddExData((int) eExdataDefine.e331, 1);
                        var fp = pet.GetFightPoint();
                        _this.mCharacter.SetExdataToMore(68, fp);
                    }
                    pet.MarkDirty();
                    return ErrorCodes.OK;
                }
                var tbPet = Table.GetPet(nId);
                if (tbPet == null)
                {
                    return ErrorCodes.Unknow;
                }
                pet.AddPiece(tbPet.ResolvePartCount);
                return ErrorCodes.OK;
            }
            if (tb_item.Type == 70000) //潜规则宠物碎片
            {
                var pet = _this.mBags[(int) eBagType.Pet].GetSamePetByPetId(tb_item.Exdata[2]);
                if (pet == null)
                {
                    _this.mBags[(int) eBagType.Pet].ForceAddItem(tb_item.Exdata[2], 1, _this.mCharacter, createItemType);
                    pet = _this.mBags[(int) eBagType.Pet].GetSamePetByPetId(tb_item.Exdata[2]);
                    if (pet == null)
                    {
                        Logger.Error("AddItem By Pet id={0},count={1}", nId, nCount);
                        return ErrorCodes.Unknow;
                    }
                }
                pet.AddPiece(nCount);
                return ErrorCodes.OK;
            }

            if (tb_item.Type >= 10500 && tb_item.Type <= 10502) //潜规则时装
            {
                if (tb_item.Type == 10500)//装备时装
                {
                    if (_this.GetBag(tb_item.InitInBag).GetItemCount(nId) > 0 ||
                        _this.GetBag((int)eBagType.EquipShiZhuang).GetItemCount(nId) > 0)
                    {
                        {//返还逻辑
                            var giftId = tb_item.Exdata[2];//ItemBase扩展数据3为礼包Id
                            var items = new Dictionary<int, int>();
                            var tbGift = Table.GetGift(giftId);
                            if (null != tbGift)
                            {
                                for (var i = 0; i != 4; ++i)
                                {
                                    if (tbGift.Param[i * 2] != -1)
                                    {
                                        items.modifyValue(tbGift.Param[i * 2], tbGift.Param[i * 2 + 1]);
                                    }
                                }
                            }
                            var error = BagManager.CheckAddItemList(_this.mCharacter.mBag, items);
                            if (error == ErrorCodes.OK)
                            {
                                var tbMail = Table.GetMail(506);
                                if (tbMail != null)
                                {
                                    var content = string.Format(tbMail.Text, tb_item.Name);
                                    _this.mCharacter.mMail.PushMail(tbMail.Title, content, items, tbMail.Sender);
                                }
                            }
                        }
                        return ErrorCodes.OK;
                    }
                }
            }

            var BagId = tb_item.InitInBag;
            if (eCreateItemType.BraveHarbor == createItemType)
            {
                BagId = Table.GetServerConfig(1099).ToInt();
            }

            if (tb_item.Type == 10005)//潜规则翅膀
            {
	            if (null != _this.mCharacter)
	            {
					var curWing = _this.mCharacter.GetWing();
					if (null != curWing && null != _this.mCharacter)
					{
						var curWingId = curWing.GetId();
						var tbCurWing = Table.GetWingQuality(curWingId);
						if (null == tbCurWing)
							return ErrorCodes.Error_WingID;
						var tbTargetWing = Table.GetWingQuality(tb_item.Id);
						if (null == tbTargetWing)
							return ErrorCodes.Error_WingID;
						if (tbCurWing.Segment < tbTargetWing.Segment)//高于当前阶数 直接升级到此阶
						{
                            var error = _this.mCharacter.WingForceFormation(nId);
							if (error == ErrorCodes.OK)
							{
                                _this.mBags[BagId].ForceDeleteItem(nId, 1);
								error = _this.mBags[BagId].CheckAddItem(nId, nCount);
								if (error == ErrorCodes.OK)
									_this.mBags[BagId].ForceAddItem(nId, nCount, _this.mCharacter, createItemType);
							}
							return error;
						}
						else//低于当前阶数 发送给玩家其他物品
						{
							var tbItemBase = Table.GetItemBase(nId);
							var giftId = tbItemBase.Exdata[1];//ItemBase扩展数据2为礼包Id
							var giftCount = tbItemBase.Exdata[2];//ItemBase扩展数据3为礼包数量
							var tbGift = Table.GetGift(giftId);
							var items = new Dictionary<int, int>();
                            for (var i = 0; i != 6; ++i)
                            {
                                if (tbGift.Param[i * 2] != -1)
                                {
                                    items.modifyValue(tbGift.Param[i * 2], tbGift.Param[i * 2 + 1] * giftCount);
                                }
                            }
							var error = BagManager.CheckAddItemList(_this.mCharacter.mBag, items);
							if (error == ErrorCodes.OK)
							{
								_this.mCharacter.mBag.AddItems(items, createItemType);
							}
                            else if (error == ErrorCodes.Error_ItemNoInBag_All)
                            {
                                var tbMail = Table.GetMail(63);
                                if (tbMail != null)
                                {
                                    _this.mCharacter.mBag.AddItemByMail(tbMail.Id, items, null, eCreateItemType.None, tbMail.Sender);
                                    return ErrorCodes.OK;
                                }
                            }
							return error;
						}
					}
	            }
                
            }

            var errorResult = _this.mBags[BagId].CheckAddItem(nId, nCount);
            if (errorResult == ErrorCodes.OK)
            {
                var nLast = _this.mBags[BagId].ForceAddItem(nId, nCount, _this.mCharacter, createItemType);
                if (nLast != 0)
                {
                    Logger.Warn("AddItem Count Warn Id={0},Count={1}", nId, nCount);
                }
                if (tb_item.InitInBag == (int) eBagType.Wing)
                {
                    if (_this.mCharacter != null)
                    {
                        _this.mCharacter.AddExData((int) eExdataDefine.e308, 1);
                        _this.mCharacter.AddExData((int)eExdataDefine.e679, 1);
                        _this.mCharacter.EquipChange(2, (int) eBagType.Wing, 0, _this.mCharacter.GetWing());
                    }
                }
            }
            return errorResult;
        }

        public ErrorCodes AddItems(BagManager _this, Dictionary<int, int> items, eCreateItemType createItemType)
        {
            var err = BagManager.CheckAddItemList(_this, items);
            if (err != ErrorCodes.OK)
            {
                return err;
            }
            foreach (var i in items)
            {
                AddItem(_this, i.Key, i.Value, createItemType);
            }
            return ErrorCodes.OK;
        }

        public ErrorCodes AddItemOrMail(BagManager _this,
                                        int mailId,
                                        Dictionary<int, int> items,
                                        List<ItemBaseData> datas,
                                        eCreateItemType createItemType,
                                        string from = "")
        {
            if (items.Count < 1)
            {
                return ErrorCodes.Unknow;
            }
            var ret = ErrorCodes.OK;
            BagManager.mailItems.Clear();
            foreach (var i in items)
            {
                var checck = CheckAddItem(_this, i.Key, i.Value);
                if (checck == ErrorCodes.Error_ItemNoInBag_All)
                {
                    ret = checck;
                }
                if (checck == ErrorCodes.OK)
                {
                    var tempItem = AddItemGetItem(_this, i.Key, i.Value, createItemType);
                    if (tempItem == null)
                    {
                        if (datas != null)
                        {
                            datas.Add(new ItemBaseData
                            {
                                ItemId = i.Key,
                                Count = i.Value
                            });
                        }
                    }
                    else
                    {
                        if (datas != null)
                        {
                            datas.Add(tempItem.mDbData);
                        }
                    }
                }
                else
                {
                    BagManager.mailItems.modifyValue(i.Key, i.Value);
                }
            }
            if (BagManager.mailItems.Count > 0)
            {
                var tbMail = Table.GetMail(mailId);
                if (tbMail == null)
                    return ErrorCodes.ParamError;

                var name = tbMail.Title;
                var content = tbMail.Text;
                if (from != string.Empty)
                {
                    name = string.Format(name, from);
                    content = string.Format(content, from);
                }

                _this.mCharacter.mMail.PushMail(name, content, BagManager.mailItems, tbMail.Sender, datas);
                BagManager.mailItems.Clear();
                PlayerLog.WriteLog((int) LogType.ReturnDungeonCostMail,
                    "Send mail in BagManager.AddItemOrMail().mailId = {0}, createItemType = {1}", mailId, createItemType);
            }

            return ret;
        }

        public ErrorCodes AddItemByMail(BagManager _this,
                                        int mailId,
                                        Dictionary<int, int> items,
                                        List<ItemBaseData> datas,
                                        eCreateItemType createItemType,
                                        string from)
        {
            if (items.Count < 1)
            {
                return ErrorCodes.Unknow;
            }
            var ret = ErrorCodes.OK;
            BagManager.mailItems.Clear();
            foreach (var i in items)
            {
                BagManager.mailItems.modifyValue(i.Key, i.Value);
            }
            if (BagManager.mailItems.Count > 0)
            {
                var tbMail = Table.GetMail(mailId);
                if (tbMail == null)
                    return ErrorCodes.ParamError;

                var name = tbMail.Title;
                var content = tbMail.Text;
                if (from != string.Empty)
                {
                    name = string.Format(name, from);
                    content = string.Format(content, from);
                }

                _this.mCharacter.mMail.PushMail(name, content, items, tbMail.Sender, datas);
                PlayerLog.WriteLog((int) LogType.ReturnDungeonCostMail,
                    "Send mail in BagManager.AddItemOrMail().mailId = {0}, createItemType = {1}", mailId, createItemType);
            }

            return ret;
        }

        public ErrorCodes AddMailItems(BagManager _this, int mailId, eCreateItemType createItemType)
        {
            var tbMail = Table.GetMail(mailId);
            if (tbMail == null)
            {
                return ErrorCodes.Error_MailNotFind;
            }

            var items = new Dictionary<int, int>();
            var itemId = tbMail.ItemId;
            var itemCount = tbMail.ItemCount;
            for (int i = 0, imax = itemId.Length; i < imax; i++)
            {
                if (itemId[i] == -1)
                {
                    break;
                }
                var item = new ItemBaseData();
                item.ItemId = itemId[i];
                item.Count = itemCount[i];
                items.modifyValue(item.ItemId, item.Count);
            }

            return AddItemByMail(_this, mailId, items, null, createItemType, string.Empty);
        }
        public ErrorCodes AddRechargeRetMailItems(BagManager _this, int mailId,List<int> countList, eCreateItemType createItemType)
        {
            var tbMail = Table.GetMail(mailId);
            if (tbMail == null)
            {
                return ErrorCodes.Error_MailNotFind;
            }
            var items = new Dictionary<int, int>();
            var itemId = tbMail.ItemId;
            var itemCount = tbMail.ItemCount;
            for (int i = 0; i < itemId.Length && i < countList.Count; i++)
            {
                if (itemId[i] < 0 || countList[i] < 0)
                    continue;
                var item = new ItemBaseData();
                item.ItemId = itemId[i];
                item.Count = itemCount[i] * countList[i];//此处表里配的数量作为 倍率来使用
                items.modifyValue(item.ItemId, item.Count);
            }

            return AddItemByMail(_this, mailId, items, null, createItemType, string.Empty);
        }
        //添加道具
        /// <summary>
        ///     添加道具
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="nId"> 物品ID</param>
        /// <param name="nCount">物品数量</param>
        /// <param name="createItemType"></param>
        /// <returns></returns>
        public ItemBase AddItemGetItem(BagManager _this, int nId, int nCount, eCreateItemType createItemType)
        {
            var tb_item = Table.GetItemBase(nId);
            if (tb_item == null)
            {
                return null;
            }
            if (tb_item.MaxCount <= 0)
            {
                AddItem(_this, nId, nCount, createItemType);
                return null;
            }
            if (tb_item.InitInBag < 0)
            {
//资源型数据
                AddRes(_this, (eResourcesType) nId, nCount, createItemType);
            }
            if (tb_item.Type == 60000) //潜规则宠物
            {
                var pet = _this.mBags[(int) eBagType.Pet].GetSamePetByPetId(nId);
                if (pet == null)
                {
                    _this.mBags[(int) eBagType.Pet].ForceAddItem(nId, 1, _this.mCharacter, createItemType);
                    pet = _this.mBags[(int) eBagType.Pet].GetSamePetByPetId(nId);
                    if (pet == null)
                    {
                        Logger.Error("AddItem By Pet id={0},count={1}", nId, nCount);
                        return null;
                    }
                }
                if (pet.GetState() == (int) PetStateType.Piece)
                {
                    pet.SetId(nId);
                    pet.SetState(PetStateType.Idle);
                    pet.MarkDirty();
                    return null;
                }
                var tbPet = Table.GetPet(nId);
                if (tbPet == null)
                {
                    return null;
                }
                pet.AddPiece(tbPet.ResolvePartCount);
                return null;
            }
            if (tb_item.Type == 70000) //潜规则宠物碎片
            {
                var pet = _this.mBags[(int) eBagType.Pet].GetSamePetByPetId(tb_item.Exdata[2]);
                if (pet == null)
                {
                    _this.mBags[(int) eBagType.Pet].ForceAddItem(tb_item.Exdata[2], 1, _this.mCharacter, createItemType);
                    pet = _this.mBags[(int) eBagType.Pet].GetSamePetByPetId(tb_item.Exdata[2]);
                    if (pet == null)
                    {
                        Logger.Error("AddItem By Pet id={0},count={1}", nId, nCount);
                        return null;
                    }
                }
                pet.AddPiece(nCount);
                return null;
            }
            var errorResult = _this.mBags[tb_item.InitInBag].CheckAddItem(nId, nCount);
            if (errorResult == ErrorCodes.OK)
            {
                var nLast = _this.mBags[tb_item.InitInBag].ForceAddItem_GetItem(nId, nCount, _this.mCharacter,
                    createItemType);
                //if (nLast == null)
                //{
                //    Logger.Warn("AddItem Count Warn Id={0},Count={1}", nId, nCount);
                //}
                if (tb_item.InitInBag == (int) eBagType.Wing)
                {
                    if (_this.mCharacter != null)
                    {
                        _this.mCharacter.EquipChange(2, (int) eBagType.Wing, 0, _this.mCharacter.GetWing());
                    }
                }
                return nLast;
            }
            return null;
        }

        //删除道具
        /// <summary>
        ///     删除道具
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="nId">道具ID</param>
        /// <param name="nCount">道具数量</param>
        /// <param name="deleteItemType"></param>
        /// <param name="exData"></param>
        /// <returns></returns>
        public ErrorCodes DeleteItem(BagManager _this, int nId, int nCount, eDeleteItemType deleteItemType, string exData)
        {
            var tb_item = Table.GetItemBase(nId);
            if (tb_item == null)
            {
                return ErrorCodes.Error_ItemID;
            }

            if (tb_item.InitInBag < 0)
            {
//资源型数据
                return DelRes(_this, (eResourcesType)nId, nCount, deleteItemType, exData);
            }
            var nNowCount = _this.mBags[tb_item.InitInBag].GetItemCount(nId);
            if (nNowCount < nCount)
            {
                return ErrorCodes.Error_ResNoEnough;
            }
            var nLast = _this.mBags[tb_item.InitInBag].ForceDeleteItem(nId, nCount);
            if (_this.mCharacter != null)
            {
                PlayerLog.DataLog(_this.mCharacter.mGuid, "id,{0},{1},{2}", nId, nCount, (int) deleteItemType);
                var e2 = new ItemChange(_this.mCharacter, nId, nLast - nCount);
                EventDispatcher.Instance.DispatchEvent(e2);
            }
            //ConditionManager.PushEvent(mCharacter, eEventType.ItemChange, nId, nLast - nCount);
            return ErrorCodes.OK;
        }

        private void CheckNotifySceneElfBuff(BagManager _this, int lBagIndex, ItemBase lItem, int rBagIndex, ItemBase rItem,
            List<int> removeBuff, Dictionary<int, int> addBuff)
        {
            ItemBase addBuffItem = null;
            ItemBase removeBuffItem = null;
            if (rBagIndex >= 3 && lBagIndex < 3)
            {
                addBuffItem = rItem;
                removeBuffItem = lItem;
            }
            if (rBagIndex < 3 && lBagIndex >= 3)
            {
                addBuffItem = lItem;
                removeBuffItem = rItem;
            }

            if (addBuffItem != null || removeBuffItem != null)
            {
                for (var i = 0; i < 3; ++i)
                {
                    if (addBuffItem != null)
                    {
                        var buffId = addBuffItem.GetBuffId(i);
                        var buffLevel = addBuffItem.GetBuffLevel(i);
                        if (buffId >= 0)
                        {
                            addBuff[buffId] = buffLevel;
                        }
                    }
                    if (removeBuffItem != null)
                    {
                        var buffId = removeBuffItem.GetBuffId(i);
                        if (buffId >= 0)
                            removeBuff.Add(buffId);
                    }
                }
            }            
        }

        //移动道具
        /// <summary>
        ///     移动道具
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="lBagType">从这个包裹</param>
        /// <param name="lBagIndex">的这个索引</param>
        /// <param name="rBagType">移动到这个包裹</param>
        /// <param name="rBagIndex">的这个索引</param>
        /// <param name="itemCount">移动数量</param>
        /// <returns></returns>
        public ErrorCodes MoveItem(BagManager _this,
                                   int lBagType,
                                   int lBagIndex,
                                   int rBagType,
                                   int rBagIndex,
                                   int itemCount)
        {
            if (lBagType == rBagType && lBagIndex == rBagIndex)
            {
                //错误的移动消息
                return ErrorCodes.Error_MoveItemFalse;
            }
            if (lBagType < 0 || lBagType >= BagManager.mBagCount)
            {
                Logger.Log(LogLevel.Warn, string.Format("警告：无此包裹，函数：MoveItem(), lBagType={0}", lBagType));
                return ErrorCodes.Error_BagID;
            }
            var lItem = _this.mBags[lBagType].GetItemByIndex(lBagIndex);
            if (lItem == null || lItem.GetId() < 0)
            {
                Logger.Log(LogLevel.Warn,
                    string.Format("警告：源包裹该位置没物品，函数：MoveItem(), lBagType={0}, lBagIndex={1}", lBagType, lBagIndex));
                return ErrorCodes.Error_BagIndexNoItem;
            }
            if (_this.mBags[lBagType].GetNowCount() <= lBagIndex || lBagIndex < 0)
            {
                Logger.Log(LogLevel.Warn,
                    string.Format("警告：源包裹下标越界，函数：MoveItem(), lBagType={0}, lBagIndex={1}", lBagType, lBagIndex));
                return ErrorCodes.Error_BagIndexOverflow;
            }
            if (rBagType < 0 || rBagType >= BagManager.mBagCount)
            {
                Logger.Log(LogLevel.Warn, string.Format("警告：无此包裹，函数：MoveItem(), rBagType={0}", rBagType));
                return ErrorCodes.Error_BagID;
            }
            if (_this.mBags[rBagType].GetNowCount() <= rBagIndex || rBagIndex < 0)
            {
                Logger.Log(LogLevel.Warn,
                    string.Format("警告：源包裹下标越界，函数：MoveItem(), rBagType={0}, rBagIndex={1}", rBagType, rBagIndex));
                return ErrorCodes.Error_BagIndexOverflow;
            }
            var lItemId = lItem.GetId();
            var lItemCount = lItem.GetCount();
            //检查物品是否存在  数量是否足够
            if (lItemCount < itemCount)
            {
                Logger.Log(LogLevel.Warn,
                    string.Format(
                        "警告：源包裹内该物品数量不足，函数：MoveItem(), lBagType={0}, lBagIndex={1}, rBagType={2}, rBagIndex={3}, count={4}",
                        lBagType, lBagIndex, rBagType, rBagIndex, itemCount));
                return ErrorCodes.Error_ResNoEnough;
            }
            //检查物品是否能放入某包裹
            var point = Table.GetItemBase(lItemId).CanInBag;
            if (!BitFlag.GetLow(point, rBagType))
            {
                Logger.Log(LogLevel.Warn,
                    string.Format(
                        "警告：源包裹内该物品无法放入目标包裹，函数：MoveItem(), lBagType={0}, lBagIndex={1}, rBagType={2}, ItemId={3},",
                        lBagType, lBagIndex, rBagType, lItemId));
                return ErrorCodes.Error_ItemNoInBag;
            }

            var rItem = _this.mBags[rBagType].GetItemByIndex(rBagIndex);

            // 灵兽背包
            var removeBuff = new List<int>();
            var addBuff = new Dictionary<int, int>();
            if (lBagType == (int)eBagType.Elf && rBagType == (int)eBagType.Elf)
            {
                CheckNotifySceneElfBuff(_this, lBagIndex, lItem, rBagIndex, rItem, removeBuff, addBuff);
            }

            if (rItem == null || rItem.GetId() < 0) //移动  或  拆分
            {
                if (lItemCount == itemCount)
                {
                    //移动
                    //ItemBase newItem = ItemFactory.Create(lItemId, mBags[rBagType].GetItemByIndex(rBagIndex).mDbData);
                    _this.mBags[rBagType].ResetItemByItem(rBagIndex, lItem, itemCount);
                    _this.mBags[lBagType].CleanItemByIndex(lBagIndex);
                    if (_this.mCharacter != null)
                    {
                        _this.mCharacter.ElfChange(removeBuff, addBuff);
                    }
                    return ErrorCodes.OK;
                }
                //拆分
                //2号创建
                _this.mBags[rBagType].ResetItemByItem(rBagIndex, lItem, itemCount);
                //1号数量
                _this.mBags[lBagType].ReduceCountByIndex(lBagIndex, itemCount, eDeleteItemType.None);
                if (_this.mCharacter != null)
                {
                    _this.mCharacter.ElfChange(removeBuff, addBuff);
                }
                return ErrorCodes.OK;
            }
            if (lItem.GetBagId() != lBagType || lItem.GetIndex() != lBagIndex)
            {
                Logger.Warn(
                    "警告：左道具数据有问题，函数：MoveItem(), lBagType={0}, ItemBagType={1}, lBagIndex={2}, ItemBagType={3},ItemId={4},",
                    lBagType, lItem.GetBagId(), lBagIndex, lItem.GetIndex(), lItemId);
                lItem.SetBagId(lBagType);
                lItem.SetIndex(lBagIndex);
            }
            var rItemId = rItem.GetId();
            if (rItem.GetBagId() != rBagType || rItem.GetIndex() != rBagIndex)
            {
                Logger.Warn(
                    "警告：右道具数据有问题，函数：MoveItem(), rBagType={0}, ItemBagType={1}, rBagIndex={2}, ItemBagType={3},ItemId={4},",
                    rBagType, rItem.GetBagId(), rBagIndex, rItem.GetIndex(), rItemId);
                rItem.SetBagId(rBagType);
                rItem.SetIndex(rBagIndex);
            }
            var rPoint = Table.GetItemBase(rItemId).CanInBag;
            if (!BitFlag.GetLow(rPoint, lBagType))
            {
                Logger.Log(LogLevel.Warn,
                    string.Format(
                        "警告：目标包裹内该物品无法放入源包裹，函数：MoveItem(), lBagType={0}, rBagType={1}, rBagIndex={2}, ItemId={3},",
                        lBagType, rBagType, rBagIndex, rItemId));
                return ErrorCodes.Error_ItemNoInBag;
            }
            var rItemCount = rItem.GetCount();
            if (lItemId == rItemId)
            {
                //堆叠
                var tb_item = Table.GetItemBase(lItemId);
                var count_max = tb_item.MaxCount;
                if (rItemCount == count_max)
                {
                    //交换
                    SwapItem(_this, lItem, rItem);
                    //mBags[rBagType].ResetItem(rBagIndex, lItem);
                    //mBags[lBagType].ResetItem(lBagIndex, rItem);
                    if (_this.mCharacter != null)
                    {
                        _this.mCharacter.ElfChange(removeBuff, addBuff);
                    }
                    return ErrorCodes.OK;
                }
                if (rItemCount + itemCount > count_max)
                {
                    //部分堆叠部分还原
                    _this.mBags[rBagType].SetCountByIndex(rBagIndex, count_max);
                    _this.mBags[lBagType].SetCountByIndex(lBagIndex, lItemCount + rItemCount - count_max);
                    if (_this.mCharacter != null)
                    {
                        _this.mCharacter.ElfChange(removeBuff, addBuff);
                    }
                    return ErrorCodes.OK;
                }
                if (lItemCount == itemCount)
                {
                    //全堆叠
                    _this.mBags[rBagType].SetCountByIndex(rBagIndex, rItemCount + itemCount);
                    _this.mBags[lBagType].CleanItemByIndex(lBagIndex);
                    if (_this.mCharacter != null)
                    {
                        _this.mCharacter.ElfChange(removeBuff, addBuff);
                    }
                    return ErrorCodes.OK;
                }
                //部分堆叠
                _this.mBags[rBagType].SetCountByIndex(rBagIndex, rItemCount + itemCount);
                _this.mBags[lBagType].SetCountByIndex(lBagIndex, lItemCount - itemCount);
                if (_this.mCharacter != null)
                {
                    _this.mCharacter.ElfChange(removeBuff, addBuff);
                }
                return ErrorCodes.OK;
            }
            if (lItemCount == itemCount)
            {
                //交换
                SwapItem(_this, lItem, rItem);
                //mBags[rBagType].ResetItem(rBagIndex, lItem);
                //mBags[lBagType].ResetItem(lBagIndex, rItem);
                if (_this.mCharacter != null)
                {
                    _this.mCharacter.ElfChange(removeBuff, addBuff);
                }
                return ErrorCodes.OK;
            }
            //例如：拿着1/N瓶红药  往N瓶蓝药上移动  （目前仍然让他交换）
            SwapItem(_this, lItem, rItem);
            //mBags[rBagType].ResetItem(rBagIndex, lItem);
            //mBags[lBagType].ResetItem(lBagIndex, rItem);
            Logger.Log(LogLevel.Warn,
                string.Format(
                    "警告：理论上不应该出现，函数：MoveItem(), lBagType={0}, lBagIndex={1}, rBagType={2}, rBagIndex={3}, Count={4}",
                    lBagType, lBagIndex, rBagType, rBagIndex, itemCount));
            if (_this.mCharacter != null)
            {
                _this.mCharacter.ElfChange(removeBuff, addBuff);
            }
            return ErrorCodes.OK;
        }

        //交换两个物品
        public void SwapItem(BagManager _this, ItemBase lItem, ItemBase rItem)
        {
            var tempItem = new ItemBase();
            tempItem.CopyFrom(lItem);
            lItem.CopyFrom(rItem);
            rItem.CopyFrom(tempItem);
            lItem.MarkDirty();
            rItem.MarkDirty();
        }

        //获得道具数量
        /// <summary>
        ///     获得道具数量
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="nId">资源ID</param>
        /// <returns></returns>
        public int GetItemCount(BagManager _this, int nId)
        {
            if (nId == -1)
            {
                return 0;
            }
            var tb_item = Table.GetItemBase(nId);
            if (tb_item == null)
            {
                Logger.Log(LogLevel.Warn, string.Format("警告：资源ID有误，函数：PlayerBag::GetItemCount(), ItemId={0}", nId));
                return 0;
            }
            if (tb_item.InitInBag < 0)
            {
//资源型数据
                return GetRes(_this, (eResourcesType) nId);
            }
            return _this.mBags[tb_item.InitInBag].GetItemCount(nId);
        }

        public int GetItemCountEx(BagManager _this, int nId)
        {
            int n = 0;

            for (int i = (int)eBagType.Equip01; i <= (int)eBagType.Equip12; i++)
            {
                n += _this.mBags[i].GetItemCount(nId); 
            }
            n += _this.mBags[(int) eBagType.Depot].GetItemCount(nId);
            return n + GetItemCount(_this, nId);
        }
        //许愿池增加道具
        public ErrorCodes AddItemToWishingPool(BagManager _this, int nId, int nCount, List<ItemBaseData> resultItems)
        {
            var errorResult = _this.mBags[22].CheckAddItem(nId, nCount);
            if (errorResult == ErrorCodes.OK)
            {
                var tempItem = _this.mBags[22].ForceAddItem_GetItem(nId, nCount, _this.mCharacter,
                    eCreateItemType.DrawWishingPool);
                if (tempItem == null)
                {
                    resultItems.Add(new ItemBaseData
                    {
                        ItemId = nId,
                        Count = nCount
                    });
                }
                else
                {
                    var itemDB = new ItemBaseData();
                    itemDB.ItemId = tempItem.mDbData.ItemId;
                    itemDB.Count = nCount;
                    itemDB.Exdata.AddRange(tempItem.mDbData.Exdata);
                    resultItems.Add(itemDB);
                }
            }
            else
            {
                Logger.Error("AddItemToWishingPool error!!!");
            }
            //else
            //{
            //    AddItemOrMail(52, new Dictionary<int, int>() { { nId, nCount } }, resultItems, eCreateItemType.DrawWishingPool);
            //}
            return errorResult;
        }

        //占星台增加道具
        public ErrorCodes AddItemToAstrologyBag(BagManager _this, int nId, int nCount, List<ItemBaseData> resultItems)
        {
            var errorResult = _this.mBags[(int) eBagType.GemBag].CheckAddItem(nId, nCount);
            if (errorResult == ErrorCodes.OK)
            {
                var tempItem = _this.mBags[(int) eBagType.GemBag].ForceAddItem_GetItem(nId, nCount, _this.mCharacter,
                    eCreateItemType.AstrologyDraw);
                if (tempItem == null)
                {
                    resultItems.Add(new ItemBaseData
                    {
                        ItemId = nId,
                        Count = nCount
                    });
                }
                else
                {
                    resultItems.Add(tempItem.mDbData);
                }
            }
            else
            {
                AddItemOrMail(_this, 115, new Dictionary<int, int> {{nId, nCount}}, resultItems,
                    eCreateItemType.AstrologyDraw);
            }
            return errorResult;
        }

        //精灵增加道具
        public ErrorCodes AddItemToElf(BagManager _this, int nId, int nCount, List<ItemBaseData> resultItems)
        {
            var errorResult = _this.mBags[(int) eBagType.Elf].CheckAddItem(nId, nCount);
            if (errorResult == ErrorCodes.OK)
            {
                var tempItem = _this.mBags[(int) eBagType.Elf].ForceAddItem_GetItem(nId, nCount, _this.mCharacter,
                    eCreateItemType.DrawElf);
                if (tempItem == null)
                {
                    resultItems.Add(new ItemBaseData
                    {
                        ItemId = nId,
                        Count = nCount
                    });
                }
                else
                {
                    var itemDB = new ItemBaseData();
                    itemDB.ItemId = tempItem.mDbData.ItemId;
                    itemDB.Count = nCount;
                    itemDB.Exdata.AddRange(tempItem.mDbData.Exdata);
                    resultItems.Add(itemDB);
                }
            }
            //else
            //{
            //    AddItemOrMail(52, new Dictionary<int, int>() { { nId, nCount } }, resultItems, eCreateItemType.DrawWishingPool);
            //}
            return errorResult;
        }

        #endregion

        #region  资源相关(增删改查)

        //检查资源有效性
        /// <summary>
        ///     检查资源有效性
        /// </summary>
        /// <param name="type">物品ID</param>
        /// <returns></returns>
        private ErrorCodes CheckResId(eResourcesType type)
        {
            if (type <= eResourcesType.InvalidRes || type >= eResourcesType.CountRes)
            {
                return ErrorCodes.Error_ResIdOverflow;
            }
            return ErrorCodes.OK;
        }

        //整理资源的修改
        public void ResChange(BagManager _this, eResourcesType type, int addValue)
        {
            _this.mResChange = true;
            _this.MarkNetDirty();
            _this.mChanges.Chaneges.Add(new DataChangeMessage
            {
                ChangeId = (int) type,
                ChangeValue = addValue
            });
        }

        //增加资源（非道具）
        /// <summary>
        ///     增加资源（非道具）
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="type">资源ID</param>
        /// <param name="nCount">资源数量</param>
        /// <param name="createItemType"></param>
        /// <returns></returns>
        public ErrorCodes AddRes(BagManager _this, eResourcesType type, int nCount, eCreateItemType createItemType)
        {
            var ErrorResult = CheckResId(type);
            if (ErrorResult != ErrorCodes.OK)
            {
                return ErrorResult;
            }
            //检查是否溢出
            var nowCount = GetRes(_this, type) + nCount;
            if (nCount > 0)
            {
                ResChange(_this, type, nCount);
            }
            SetRes(_this, type, nowCount);

            if (_this.mCharacter != null)
            {
                var e2 = new ItemChange(_this.mCharacter, (int) type, nCount);
                EventDispatcher.Instance.DispatchEvent(e2);
                if ((type != eResourcesType.GoldRes && type != eResourcesType.ExpRes) || createItemType != eCreateItemType.PickUp)
                    PlayerLog.DataLog(_this.mCharacter.mGuid, "ra,{0},{1},{2},{3}", (int) type, nCount, nowCount,(int) createItemType);

                // 统计
                try
                {
                    if (type == eResourcesType.DiamondRes)
                    {
                        string v = string.Format("diamonds_info#{0}|{1}|{2}|{3}|{4}|{5}",
                                      "",
                                      _this.mCharacter.serverId,
                                      _this.mCharacter.mGuid,
                                      nCount,
                                      (int)createItemType,
                                      DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")); // 时间
                        kafaLogger.Info(v);
                    }
                    else if (type == eResourcesType.Alchemy) //炼金货币
                    {
                        string v = string.Format("alchemyget_info#{0}|{1}|{2}|{3}|{4}|{5}",
                                      _this.mCharacter.serverId,
                                      _this.mCharacter.mGuid,
                                      nCount,
                                      (int)createItemType,
                                      DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                                      _this.mCharacter.GetLevel()); // 时间
                        PlayerLog.Kafka(v);
                    }
                    else if (type == eResourcesType.HomeExp) //符文经验
                    {
                        string v = string.Format("fuwenexpget_info#{0}|{1}|{2}|{3}|{4}|{5}",
                                      _this.mCharacter.serverId,
                                      _this.mCharacter.mGuid,
                                      nCount,
                                      (int)createItemType,
                                      DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                                      _this.mCharacter.GetLevel()); // 时间
                        PlayerLog.Kafka(v);
                    }
                    else if (type == eResourcesType.Spar) //智慧结晶
                    {
                        string v = string.Format("sparget_info#{0}|{1}|{2}|{3}|{4}|{5}",
                                      _this.mCharacter.serverId,
                                      _this.mCharacter.mGuid,
                                      nCount,
                                      (int)createItemType,
                                      DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                                      _this.mCharacter.GetLevel()); // 时间
                        PlayerLog.Kafka(v);
                    }
                    else if (type == eResourcesType.ElfPiece) //精魄
                    {
                        string v = string.Format("jingpoget_info#{0}|{1}|{2}|{3}|{4}|{5}",
                                      _this.mCharacter.serverId,
                                      _this.mCharacter.mGuid,
                                      nCount,
                                      (int)createItemType,
                                      DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                                      _this.mCharacter.GetLevel()); // 时间
                        PlayerLog.Kafka(v);
                    }
                    else if (type == eResourcesType.DiamondBind)
                    {
                        string vv = string.Format("allitem_get_info#{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                            _this.mCharacter.serverId,
                            _this.mCharacter.mGuid,
                            (int)type,
                            nCount,
                            DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), // 时间
                            (int)createItemType,
                            _this.mCharacter.GetLevel());
                        PlayerLog.Kafka(vv);
                    }
                }
                catch (Exception)
                {
                }
                if (nCount > 0 && type == eResourcesType.Contribution && _this.mCharacter.mAlliance.AllianceId>0)
                {
                    _this.mCharacter.OnAddCharacterContribution(nCount);
                }
            }
            if (GetRes(_this, type) < 0)
            {
                SetRes(_this, type, 0);
                return ErrorCodes.Error_DataOverflow;
            }
            

            return ErrorCodes.OK;
        }

        //删除资源（非道具）
        /// <summary>
        ///     删除资源（非道具）
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="type">资源ID</param>
        /// <param name="nCount">资源数量</param>
        /// <param name="createItemType"></param>
        /// <param name="exData">扩展字符串判断具体消耗途径</param>
        /// <returns></returns>
        public ErrorCodes DelRes(BagManager _this, eResourcesType type, int nCount, eDeleteItemType createItemType, string exData)
        {
            if (nCount < 0)
            {
                Logger.Error("DelRes Error!C={3},T={0},V={1},B={2}", type, -nCount, createItemType,
                    _this.mCharacter.mGuid);
                return ErrorCodes.Error_CountNotEnough;
            }
            var ErrorResult = CheckResId(type);
            if (ErrorResult != ErrorCodes.OK)
            {
                return ErrorResult;
            }
            var nowCount = GetRes(_this, type);
            if (nowCount < nCount)
            {
                Logger.Warn("DelRes Error_ResNoEnough  old={0},del={1}", nowCount, nCount);
                var e = new ItemChange(_this.mCharacter, (int) type, -nowCount);
                EventDispatcher.Instance.DispatchEvent(e);
                PlayerLog.DataLog(_this.mCharacter.mGuid, "re,{0},{1},{2},{3}", (int) type, -nCount, nowCount,
                    (int) createItemType);
                SetRes(_this, type, 0);
                //ConditionManager.PushEvent(character, eEventType.ItemChange, (int)type, -GetRes(type));
                //MarkDirty();

                return ErrorCodes.Error_ResNoEnough;
            }
            //Logger.Info("DelRes,ResId={0},ResValue={1},DelValue={2}", (int)type, GetRes(type), nCount);
            nowCount -= nCount;
            SetRes(_this, type, nowCount);

	        if (type == eResourcesType.DiamondRes)
	        {
		        if (null != _this.mCharacter && null != _this.mCharacter.mOperActivity)
		        {
					_this.mCharacter.mOperActivity.OnUseDiamondEvent(nCount);
		        }

	        }

            if (_this.mCharacter != null)
            {
                var e2 = new ItemChange(_this.mCharacter, (int) type, -nCount);
                EventDispatcher.Instance.DispatchEvent(e2);
                PlayerLog.DataLog(_this.mCharacter.mGuid, "rd,{0},{1},{2},{3}", (int) type, -nCount, nowCount,
                    (int) createItemType);

                try
                {
                    if (createItemType != eDeleteItemType.StoreBuy && createItemType != eDeleteItemType.CheckAdd)
                    {
                        string v = string.Format("gamegoodslog#{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}",
                                  _this.mCharacter.serverId,
                                  "account",
                                  _this.mCharacter.GetName(),
                                  _this.mCharacter.mGuid,
                                  (int)createItemType,
                                  exData,
                                  1,
                                  (int)type,
                                  nCount,
                                  DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")); // 时间
                        kafaLogger.Info(v);  
                    }
                }
                catch (Exception)
                {
                }
            }
            //ConditionManager.PushEvent(character, eEventType.ItemChange, (int)type, -nCount);
            return ErrorCodes.OK;
        }

        //设置资源（非道具）
        /// <summary>
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="type"></param>
        /// <param name="count"></param>
        public void SetRes(BagManager _this, eResourcesType type, int count)
        {
            if (_this.mCharacter == null)
            {
                return;
            }
            var ErrorResult = CheckResId(type);
            if (ErrorResult != ErrorCodes.OK)
            {
                return;
            }
            if (type == eResourcesType.ExpRes)
            {
                count = SetExp(_this, count);
            }
            else if (type == eResourcesType.LevelRes)
            {
                if (count > Constants.LevelMax)
                {
                    Logger.Warn("SetLevel too More = {0}", count);
                    count = Constants.LevelMax;
                }
                Logger.Info("syn Level={0}", count);
                _this.mCharacter.SkillChange(2, -1, count); //通知scenesever等级发生了变化
                var oldmaxLevel = _this.mCharacter.GetExData((int) eExdataDefine.e46);
                if (oldmaxLevel < count)
                {
                    _this.mCharacter.OnLevelUp(count);
                    _this.mCharacter.SetExData((int) eExdataDefine.e46, count);
                }

                PlayerLog.BackDataLogger((int) BackDataType.LevelData, "{0}|{1}|{2}|{3}|{4}",
                    _this.mCharacter.mGuid, _this.mDbData.Resources[(int) type], count, _this.mCharacter.serverId,
                    _this.mDbData.Resources[(int) eResourcesType.VipLevel]);

                try
                {
                    string userlevel = string.Format("userlevel#{0}|{1}|{2}",
                            _this.mCharacter.mGuid,// charid
                            count, //level
                            DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
                            );
                    kafaLogger.Info(userlevel);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }

                //升级后导致的技能修炼点数变化
                if (count >= StaticParam.TalentPointBeginLevel)
                {
                    var oldLevel = oldmaxLevel;
                    var talentPoint = 0;
                    if (oldLevel < StaticParam.TalentPointBeginLevel)
                    {
                        talentPoint = StaticParam.TalentPointBeginPoint;
                        oldLevel = StaticParam.TalentPointBeginLevel;
                    }
                    else if (oldLevel > StaticParam.TalentPointBeginLevel)
                    {
                        oldLevel = oldLevel - oldLevel%StaticParam.TalentPointEveryLevel;
                    }

                    talentPoint += (count - oldLevel)/StaticParam.TalentPointEveryLevel;
                    if (talentPoint > 0)
                    {
                        _this.mCharacter.mTalent.AddTalentPoint(talentPoint);
                        if (_this.mCharacter.Proxy != null)
                        {
                            _this.mCharacter.Proxy.TalentCountChange(-1, _this.mCharacter.mTalent.TalentCount);
                        }
                    }
                }
	            try
	            {
					_this.mCharacter.mOperActivity.OnRankDataChange(RankType.Level, count);
	            }
	            catch (Exception e)
	            {
					Logger.Fatal(e.Message);   
	            }
            }
            else if (type == eResourcesType.GoldRes)
            {
                if (count > _this.mDbData.Resources[(int) type])
                {
                    //潜规则
                    if (_this.mCharacter != null)
                    {
                        _this.mCharacter.AddExData((int) eExdataDefine.e48, count - _this.mDbData.Resources[(int) type]);
                    }
                }
                else if (count < _this.mDbData.Resources[(int) type])
                {
                    if (_this.mCharacter != null)
                    {
                        _this.mCharacter.AddExData((int) eExdataDefine.e83, _this.mDbData.Resources[(int) type] - count);
                    }
                }
            }
            else if (type == eResourcesType.DiamondRes)
            {
                if (count > _this.mDbData.Resources[(int) type])
                {
                    if (_this.mCharacter != null)
                    {
                        _this.mCharacter.AddExData((int) eExdataDefine.e95, count - _this.mDbData.Resources[(int) type]);
                    }
                }
                else if (count < _this.mDbData.Resources[(int) type])
                {
                    var s = _this.mDbData.Resources[(int) type] - count;
                    if (_this.mCharacter != null)
                    {
                        _this.mCharacter.AddExData((int) eExdataDefine.e84, s);
                        _this.mCharacter.AddExData(StaticParam.CumulativeConsumeExdataId, s);
                    }
                }
            }
            //else if (type == eResourcesType.HomeExp)
            //{
            //    var oldLevel = _this.mCharacter.mCity.Level;
            //    var tbLvl = Table.GetLevelData(oldLevel);
            //    if (tbLvl == null)
            //    {
            //        Logger.Warn("City AddExp Error! level={0}", oldLevel);
            //        return;
            //    }
            //    if (tbLvl.UpNeedExp < 1)
            //    {
            //        Logger.Warn("City AddExp Error! level={0}", oldLevel);
            //        return;
            //    }
            //    var oldExp = count;
            //    while (oldExp >= tbLvl.UpNeedExp && tbLvl.UpNeedExp > 0)
            //    {
            //        oldLevel++;
            //        oldExp -= tbLvl.UpNeedExp;
            //        tbLvl = Table.GetLevelData(oldLevel);
            //    }
            //    if (tbLvl.UpNeedExp < 0)
            //    {
            //        oldExp = 0;
            //    }
            //    _this.mCharacter.mCity.Level = oldLevel;
            //    count = oldExp;
            //}
            //else if (type == eResourcesType.HomeLevel)
            //{
            //    mCharacter.SetExData((int)eExdataDefine.e46, count);
            //}
            else if (type == eResourcesType.VipExpRes)
            {
                var oldLevel = GetRes(_this, eResourcesType.VipLevel);
                var newLevel = CaculateVipLevel(_this, count, oldLevel);
                if (newLevel > oldLevel)
                {
                    AddRes(_this, eResourcesType.VipLevel, newLevel - oldLevel, eCreateItemType.None);
                    _this.mCharacter.OnVipLevelChanged(oldLevel, newLevel);
                }
            }
            else if (type == eResourcesType.VipLevel)
            {
                PlayerLog.BackDataLogger((int) BackDataType.VipData, "{0}|{1}|{2}|{3}|{4}", _this.mCharacter.mGuid,
                    _this.mDbData.Resources[(int) type], count, _this.mCharacter.serverId, _this.mCharacter.GetLevel());
            }

            _this.mDbData.Resources[(int) type] = count;
            //_this.SetFlag();
            _this.MarkDbDirty();

            _this.OnPropertyChanged(type.ToString());
            switch (type)
            {
                case eResourcesType.LevelRes:
                    _this.mCharacter.SetRankFlag(RankType.Level);
                    _this.mCharacter.SetRankFlag(RankType.PetFight);
                    break;
                case eResourcesType.ExpRes:
                    _this.mCharacter.SetRankFlag(RankType.Level);
                    break;
                case eResourcesType.GoldRes:
                    _this.mCharacter.SetRankFlag(RankType.Money);
                    break;
                case eResourcesType.HomeLevel:
                    _this.mCharacter.SetRankFlag(RankType.CityLevel);
                    break;
                case eResourcesType.HomeExp:
                    _this.mCharacter.SetRankFlag(RankType.CityLevel);
                    break;
                case eResourcesType.VipLevel:
                    //扩展两个参数，这里用不到
                    _this.mCharacter.OnVipLevelChanged(0,0);
                    break;
                case eResourcesType.AchievementScore:
                    _this.mCharacter.SetExData((int)eExdataDefine.e50, count);
                    break;
            }
        }

        //获取资源（非道具）
        public int GetRes(BagManager _this, eResourcesType type)
        {
            var ErrorResult = CheckResId(type);
            if (ErrorResult != ErrorCodes.OK)
            {
                return -1;
            }

            return _this.mDbData.Resources[(int) type];
        }

        //设置等级
        public int SetExp(BagManager _this, int nExp)
        {
            var nLevel = GetRes(_this, eResourcesType.LevelRes);
            if (nLevel >= Constants.LevelMax)
            {
                return 0;
            }
            LevelDataRecord tbLevel;
            var nNeedExp = 0;
            var levelUp = 0;

            while (true)
            {
                tbLevel = Table.GetLevelData(nLevel + levelUp);
                if (tbLevel == null)
                {
                    break;
                }
                nNeedExp = tbLevel.NeedExp;
                if (nExp >= nNeedExp)
                {
                    levelUp++;
                    nExp -= nNeedExp;
                }
                else
                {
                    break;
                }
            }

            if (levelUp > 0)
            {
                AddRes(_this, eResourcesType.LevelRes, levelUp, eCreateItemType.None);
            }
            return nExp;
        }

        //计算vip等级
        public int CaculateVipLevel(BagManager _this, int exp, int level)
        {
            VIPRecord tbVip;
            var nNeedExp = 0;

            while (true)
            {
                tbVip = Table.GetVIP(level + 1);
                if (tbVip == null)
                {
                    break;
                }
                nNeedExp = tbVip.NeedVipExp;
                if (exp >= nNeedExp)
                {
                    level++;
                }
                else
                {
                    break;
                }
            }
            return Math.Min(level, Constants.VipMax);
        }

        //设置vip等级
        public void OnVipLevelChange(BagManager _this, int level)
        {
            var tbVip = Table.GetVIP(level);
            if (tbVip.GetItem != -1)
            {
                _this.AddItem(tbVip.GetItem, 1, eCreateItemType.Vip);
            }
            if (tbVip.GetTitle != -1)
            {
                var tbNameTitle = Table.GetNameTitle(tbVip.GetTitle);
                _this.mCharacter.SetFlag(tbNameTitle.FlagId);
            }
        }

        #endregion

        #region 整理

        public BagBase GetBag(BagManager _this, int BagId)
        {
            BagBase bag;
            if (_this.mBags.TryGetValue(BagId, out bag))
            {
                return bag;
            }
            return null;
        }

        public BagBase SortBag(BagManager _this, int BagId)
        {
            if (BagId < 0)
            {
                return null;
            }
            if (BagId > 3 && BagId != 6 && BagId != 21 && BagId != 22 && BagId != 23 && BagId < 26)
            {
                return null;
            }
            //mCharacter.TestBagLogicIndex();
            //mCharacter.TestBagDbIndex();
            switch (BagId)
            {
                case 0: //装备包裹
                {
                    var Bag = _this.mBags[BagId];
                    var temp_list = new List<int>();
                    var sort_list = new List<KeyValuePair<ItemBase, long>>();
                    foreach (var itemBase in Bag.mLogics)
                    {
                        if (itemBase.GetId() == -1)
                        {
                            sort_list.Add(new KeyValuePair<ItemBase, long>(itemBase, 0));
                            continue;
                        }
                        temp_list.Add(itemBase.GetIndex());
                        var tb_item = Table.GetItemBase(itemBase.GetId());
                        //int paixu = 100000 ;
                        //  00000(新的排序字段)00000(物品ID)
                        long thissortvalue = tb_item.SortLadder; //paixu * 100000 + tb_item.Id;
                        sort_list.Add(new KeyValuePair<ItemBase, long>(itemBase, thissortvalue));
                    }
                    var result_array = (from item in sort_list orderby -item.Value select item).ToArray();
                    //重构
                    var dbdata = new BagBaseData();
                    var i = 0;
                    foreach (var keyValuePair in result_array)
                    {
                        Bag.mLogics[i] = keyValuePair.Key;
                        dbdata.Items.Add(keyValuePair.Key.mDbData);
                        keyValuePair.Key.SetIndex(i);
                        ++i;
                    }
                    //mCharacter.TestBagLogicIndex();
                    for (var index = 0; index != Bag.mLogics.Count; ++index)
                    {
                        var itemDbData = Bag.mLogics[index].mDbData;
                        Bag.mDbData.Items[index] = itemDbData;
                        if (itemDbData.Index != index)
                        {
                            itemDbData.Index = index;
                        }
                    }
                    Bag.MarkDbDirty();
                    //mCharacter.TestBagLogicIndex();
                    //mCharacter.TestBagDbIndex();
                    //List<ItemBase> mLogics = new List<ItemBase>();
                    //int i = 0;
                    //foreach (KeyValuePair<ItemBase, long> keyValuePair in result_array)
                    //{
                    //    dbdata.Items.Add(keyValuePair.Key.mDbData);
                    //    mLogics.Add(keyValuePair.Key);
                    //    ++i;
                    //}
                    //i = 0;
                    //mDbData.Bags[BagId] = dbdata;
                    //foreach (ItemBase itemBase in mLogics)
                    //{
                    //    Bag.mLogics[i] = itemBase;
                    //    itemBase.mDbData = dbdata.Items[i];
                    //    ++i;
                    //}
                    return Bag;
                }
                case 1: //道具包裹
                case 2: //碎片包裹
                case 3: //仓库包裹
                case 6: //勋章包裹
                case 21: //农场仓库
                case 22: //许愿池背包
                case 23: //宝石仓库
                case 26: //装备时装
                case 27: //翅膀时装
                case 28: //武器时装
                {
                    var Bag = _this.mBags[BagId];
                    var temp_list = new List<int>();
                    var sort_list = new List<KeyValuePair<ItemBase, long>>();
                    foreach (var itemBase in Bag.mLogics)
                    {
                        if (itemBase.GetId() == -1)
                        {
                            sort_list.Add(new KeyValuePair<ItemBase, long>(itemBase, 0));
                            continue;
                        }
                        temp_list.Add(itemBase.GetIndex());
                        var tb_item = Table.GetItemBase(itemBase.GetId());
                        //int paixu = 100000 ;
                        //  00000(新的排序字段)00000(物品ID)
                        long thissortvalue = tb_item.SortLadder; //paixu * 100000 + tb_item.Id;
                        sort_list.Add(new KeyValuePair<ItemBase, long>(itemBase, thissortvalue));
                    }
                    var result_array = (from item in sort_list orderby -item.Value select item).ToArray();
                    //堆叠数量整理
                    ItemBase before = null;
                    var i = 0;
                    var delList = new List<int>();
                    foreach (var keyValuePair in result_array)
                    {
                        if (before == null)
                        {
                            before = keyValuePair.Key;
                            ++i;
                            continue;
                        }
                        if (keyValuePair.Key.GetId() == -1)
                        {
                            before = null;
                            ++i;
                            continue;
                        }
                        if (keyValuePair.Key.GetId() == before.GetId())
                        {
                            var tbItem = Table.GetItemBase(keyValuePair.Key.GetId());
                            if (tbItem == null)
                            {
                                continue;
                            }
                            if (tbItem.MaxCount > 1)
                            {
                                var needAdd = tbItem.MaxCount - before.GetCount();
                                if (needAdd > 0)
                                {
                                    var thisCount = keyValuePair.Key.GetCount();
                                    if (thisCount > needAdd)
                                    {
                                        before.SetCount(tbItem.MaxCount);
                                        keyValuePair.Key.SetCount(thisCount - needAdd);
                                    }
                                    else
                                    {
                                        before.SetCount(before.GetCount() + thisCount);
                                        delList.Add(i);
                                        before = null;
                                        ++i;
                                        continue;
                                    }
                                }
                            }
                        }
                        before = keyValuePair.Key;
                        ++i;
                    }
                    //重构
                    i = 0;
                    var dbdata = new BagBaseData();
                    //Bag.mDbData = dbdata;
                    //dbdata.BagId = BagId;
                    //dbdata.NowCount = Bag.GetNowCount();
                    var iTemp = 0;
                    foreach (var keyValuePair in result_array)
                    {
                        if (delList.Contains(iTemp))
                        {
                            iTemp++;
                            continue;
                        }
                        Bag.mLogics[i] = keyValuePair.Key;
                        //Logger.Fatal("sortBag log i={0},oldindex={1},sortvalue={2}", i, keyValuePair.Key.GetIndex(),keyValuePair.Value);
                        keyValuePair.Key.SetIndex(i);
                        dbdata.Items.Add(keyValuePair.Key.mDbData);
                        ++i;
                        iTemp++;
                    }
                    for (var tempIndex = i; tempIndex < Bag.GetNowCount(); ++tempIndex)
                    {
                        var ib = new ItemBase();
                        Bag.mLogics[tempIndex] = ib;
                        dbdata.Items.Add(ib.mDbData);
                        ib.SetIndex(i);
                        Bag.AddChild(ib);
                    }

                    //Logger.Warn("sortBag logic index is ok");
                    //mCharacter.TestBagLogicIndex();
                    for (var index = 0; index != Bag.mLogics.Count; ++index)
                    {
                        var itemDbData = Bag.mLogics[index].mDbData;
                        Bag.mDbData.Items[index] = itemDbData;
                        if (itemDbData.Index != index)
                        {
                            itemDbData.Index = index;
                        }
                    }

                    //Logger.Warn("sortBag Dbase index is ok");

                    Bag.MarkDbDirty();
                    //mCharacter.TestBagLogicIndex();
                    //mCharacter.TestBagDbIndex();
                    return Bag;
                }
                case 4: //宝石包裹
                {
                }
                    break;
                default:
                    break;
            }
            return null;
        }

        #endregion

        #region   特定接口(经验)

        public int GetLevel(BagManager _this)
        {
            return GetRes(_this, 0);
        }

        public void AddExp(BagManager _this, int addExp, eCreateItemType createItemType)
        {
            AddRes(_this, eResourcesType.ExpRes, addExp, createItemType);
        }

        #endregion

        #region   属性相关

        //刷新精灵属性
        public void RefreshElfAttr(BagManager _this)
        {
            _this.Elfattrs.Clear();

            var groupList = new Dictionary<int, int>();
            var elfBag = GetBag(_this, (int) eBagType.Elf);
            var elfLevel = _this.mCharacter.GetExData(82);
            var tbLevel = Table.GetLevelData(elfLevel);
            if (tbLevel == null)
            {
				Logger.Fatal("null==Table.GetLevelData({0})", elfLevel);
                return;
            }
            //foreach (var item in elfBag.mLogics)
            //{
            //    var elf = item as ElfItem;
            //    if (elf == null) continue;
            //    if (elf.GetExdata(1) == 0) continue;
            //    var tbElf = Table.GetElf(elf.GetId());
            //    if (tbElf == null) continue;
            //    elf.GetAttrList(Elfattrs, tbElf, GetLevel(), tbLevel.FightingWayIncome);
            //    foreach (int groupId in tbElf.BelongGroup)
            //    {
            //        if (groupId != -1)
            //        {
            //            groupList.modifyValue(groupId, 1);
            //        }
            //    }
            //}
            for (var i = 0; i < 3; ++i)
            {
                var item = elfBag.GetItemByIndex(i);
                var elf = item as ElfItem;
                if (elf == null)
                {
                    continue;
                }
                //if (elf.GetExdata(1) == 0) continue;
                var tbElf = Table.GetElf(elf.GetId());
	            if (null == tbElf)
	            {
					Logger.Fatal("null==Table.GetElf({0})", elf.GetId());
		            continue;
	            }
                elf.GetAttrList(_this.Elfattrs, tbElf, GetLevel(_this), (10000 + tbLevel.FightingWayIncome));
                foreach (var groupId in tbElf.BelongGroup)
                {
                    if (groupId != -1)
                    {
                        groupList.modifyValue(groupId, 1);
                    }
                }
            }
            foreach (var pair in groupList)
            {
                var tbElfGroup = Table.GetElfGroup(pair.Key);
	            if (null == tbElfGroup)
	            {
					Logger.Fatal("RefreshElfAttr null==Table.GetElfGroup({0})", pair.Key);
		            continue;
	            }
                var count = 0;
                foreach (var i in tbElfGroup.ElfID)
                {
                    if (i != -1)
                    {
                        count++;
                    }
                }
                if (count == pair.Value)
                {
                    for (var i = 0; i < tbElfGroup.GroupPorp.Length; i++)
                    {
                        var attrId = tbElfGroup.GroupPorp[i];
                        if (attrId == -1)
                        {
                            break;
                        }
                        var attrValue = tbElfGroup.PropValue[i];
                        _this.Elfattrs.modifyValue(attrId, attrValue);
                    }
                }
            }
        }
        //刷新勋章属性
        public void RefreshMedal(BagManager _this)
        {
            _this.Medalattrs.Clear();
            var bag19 = GetBag(_this, (int) eBagType.MedalUsed);
            if (bag19 == null)
            {
                return;
            }
            foreach (var itemBase in bag19.mLogics)
            {
                if (itemBase.GetId() == -1)
                {
                    continue;
                }
                var tbMedal = Table.GetMedal(itemBase.GetId());
                if (tbMedal == null)
                {
                    continue;
                }
                for (var i = 0; i < tbMedal.AddPropID.Length; i++)
                {
                    if (tbMedal.AddPropID[i] != -1)
                    {
                        _this.Medalattrs.modifyValue(tbMedal.AddPropID[i],
                            Table.GetSkillUpgrading(tbMedal.PropValue[i]).GetSkillUpgradingValue(itemBase.GetExdata(0)));
                    }
                }
            }
        }

        //刷新宝石属性
        public void RefreshGemAttr(BagManager _this)
        {
            _this.Gemattrs.Clear();
            var bag24 = GetBag(_this, (int) eBagType.GemEquip);
            if (bag24 == null)
            {
                return;
            }
            //拿到可能需要的条件
            var build = _this.mCharacter.mCity.GetBuildByType(3);
            if (build == null)
            {
                return;
            }
            //宠物总等级
            var petLevel = 0;
            var petCount = build.PetList.Count;
            var petFighting = 0;
            foreach (var i in build.PetList)
            {
                var pet = _this.mCharacter.GetPet(i);
                if (pet == null)
                {
                    continue;
                }
                petFighting += pet.GetFightPoint();
                petLevel += pet.GetExdata(1);
            }
            //int petLevel = 0;
            //int petCount = 0;
            //int petFighting = 0;
            //计算属性
            var groups = new Dictionary<int, int>();
            for (var i = 0; i < 12; ++i)
            {
                groups.Clear();
                for (var j = 0; j < 3; ++j)
                {
                    var index = j*12 + i;
                    var tempItem = bag24.GetItemByIndex(index);
                    if (tempItem == null)
                    {
                        continue;
                    }
                    //if (tempItem.GetId() == -1) continue;
                    var tbGem = Table.GetGem(tempItem.GetId());
                    if (tbGem == null)
                    {
                        continue;
                    }
                    //组合统计
                    if (tbGem.Combination != -1)
                    {
                        groups.modifyValue(tbGem.Combination, 1);
                    }
                    //属性计算
                    var level = tempItem.GetExdata(0);
                    for (var k = 0; k < 6; ++k)
                    {
                        var con = tbGem.ActiveCondition[k];
                        if (con == -1)
                        {
                            break;
                        }
                        var isCan = false;
                        switch (con)
                        {
                            case 0:
                            {
//无要求
                                isCan = true;
                            }
                                break;
                            case 1:
                            {
//镶嵌星座要求
                                if (tbGem.Param[k] == j)
                                {
                                    isCan = true;
                                }
                            }
                                break;
                            case 2:
                            {
//随从等级要求
                                if (tbGem.Param[k] <= petLevel)
                                {
                                    isCan = true;
                                }
                            }
                                break;
                            case 3:
                            {
//随从战力要求
                                if (tbGem.Param[k] <= petFighting)
                                {
                                    isCan = true;
                                }
                            }
                                break;
                            case 4:
                            {
//随从个数要求
                                if (tbGem.Param[k] <= petCount)
                                {
                                    isCan = true;
                                }
                            }
                                break;
                        }
                        if (isCan)
                        {
                            var aId = tbGem.Prop1[k];
                            if (aId != -1)
                            {
                                _this.Gemattrs.modifyValue(aId,
                                    Table.GetSkillUpgrading(tbGem.PropValue1[k]).GetSkillUpgradingValue(level));
                            }
                            aId = tbGem.Prop2[k];
                            if (aId != -1)
                            {
                                _this.Gemattrs.modifyValue(aId,
                                    Table.GetSkillUpgrading(tbGem.PropValue2[k]).GetSkillUpgradingValue(level));
                                //Gemattrs.modifyValue(aId, tbGem.PropValue2[k]);
                            }
                        }
                    }
                }
                //组合属性
                foreach (var pair in groups)
                {
                    if (pair.Value < 2)
                    {
                        continue;
                    }
                    var tbGemGroup = Table.GetGemGroup(pair.Key);
                    if (tbGemGroup == null)
                    {
                        PlayerLog.WriteLog((int) LogType.RefreshGem, "GetGemGroup not find Id= {0}", pair.Key);
                        continue;
                    }
                    var tempIndex = 0;
                    foreach (var i1 in tbGemGroup.Towprop)
                    {
                        if (i1 != -1)
                        {
                            _this.Gemattrs.modifyValue(i1, tbGemGroup.TowValue[tempIndex]);
                        }
                        tempIndex++;
                    }
                    if (pair.Value > 2)
                    {
                        tempIndex = 0;
                        foreach (var i1 in tbGemGroup.Threeprop)
                        {
                            if (i1 != -1)
                            {
                                _this.Gemattrs.modifyValue(i1, tbGemGroup.ThreeValue[tempIndex]);
                            }
                            tempIndex++;
                        }
                    }
                }
            }
            //foreach (ItemBase itemBase in bag24.mLogics)
            //{
            //    if (itemBase.GetId() == -1) continue;
            //    var tbGem = Table.GetGem(itemBase.GetId());
            //    if (tbGem == null) continue;
            //}
        }

        #endregion
    }

    public class BagManager : NodeBase, INotifyPropertyChanged
    {
        public static List<int> BagInitItemList = new List<int> {0, 1, 2, 4, 5, 12, 20, 21};
        public static List<int> SoleItemList = new List<int>();
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        public static BagManager staticBag = new BagManager();

        public override IEnumerable<NodeBase> Children
        {
            get { return mBags.Values; }
        }

        public static ErrorCodes CheckAddItemList(BagManager theBag, Dictionary<int, int> items)
        {
            return mImpl.CheckAddItemList(theBag, items);
        }

        public IBagManager GetFunctionImpl()
        {
            return mImpl;
        }

        public void GetNetDirtyMissions(BagsChangeData msg)
        {
            mImpl.GetNetDirtyMissions(this, msg);
        }

        public override void NetDirtyHandle()
        {
            mImpl.NetDirtyHandle(this);
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region  存储

        public void SaveDB()
        {
            mImpl.SaveDB(this);
        }

        #endregion

        public static void staticInit()
        {
            mImpl.staticInit();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region  数据结构

        public static readonly int mResCount = (int)eResourcesType.CountRes;
        public static readonly int mBagCount = 32;
        public CharacterController mCharacter; //所在角色
        public Dictionary<int, BagBase> mBags = new Dictionary<int, BagBase>();
        public bool EquipDurableChange = false;
        public Dictionary<int, int> Elfattrs = new Dictionary<int, int>(); //精灵所提供的属性列表
        public Dictionary<int, int> Medalattrs = new Dictionary<int, int>(); //勋章所提供的属性列表
        public Dictionary<int, int> Gemattrs = new Dictionary<int, int>(); //宝石所提供的属性列表
        public DataChangeList mChanges = new DataChangeList(); //资源的所有修改
        public bool mResChange;
        //public int[] mResources = new int[mResCount];


        private static IBagManager mImpl;

        static BagManager()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (BagManager), typeof (BagManagerDefaultImpl),
                o => { mImpl = (IBagManager) o; });
        }

        public BagData mDbData;
        //public bool mFlag = true;

        //public void SetFlag()
        //{
        //    this.mFlag = true;
        //}

        #endregion

        #region   初始化

        //初始化（按初始配置）
        public BagData InitByBase(CharacterController character)
        {
            return mImpl.InitByBase(this, character);
        }

        //初始化（按数据库配置）
        public void InitByDB(CharacterController character, BagData bag)
        {
            mImpl.InitByDB(this, character, bag);
        }

        #endregion

        #region  道具相关(增删改查)

        //检查增加物品是否可行
        /// <summary>
        ///     检查增加物品是否可行（不能浪费主要)
        /// </summary>
        /// <param name="nId">物品ID</param>
        /// <param name="nCount">数量</param>
        /// <returns></returns>
        public ErrorCodes CheckAddItem(int nId, int nCount)
        {
            return mImpl.CheckAddItem(this, nId, nCount);
        }

        //已经实例化好的道具给予
        public ErrorCodes AddItem(ItemBaseData itemDb, eCreateItemType createItemType)
        {
            return mImpl.AddItem(this, itemDb, createItemType);
        }

        //添加道具
        /// <summary>
        ///     添加道具
        /// </summary>
        /// <param name="nId"> 物品ID</param>
        /// <param name="nCount">物品数量</param>
        /// <param name="createItemType"></param>
        /// <returns></returns>
        public ErrorCodes AddItem(int nId, int nCount, eCreateItemType createItemType)
        {
            return mImpl.AddItem(this, nId, nCount, createItemType);
        }

        /// <summary>
        ///     添加一堆道具
        /// </summary>
        /// <param name="items"> 物品 </param>
        /// <param name="createItemType"></param>
        /// <returns></returns>
        public ErrorCodes AddItems(Dictionary<int, int> items, eCreateItemType createItemType)
        {
            return mImpl.AddItems(this, items, createItemType);
        }

        //增加物品，如果装不下则发送邮件
        public static Dictionary<int, int> mailItems = new Dictionary<int, int>();

        public ErrorCodes AddItemOrMail(int mailId,
                                        Dictionary<int, int> items,
                                        List<ItemBaseData> datas,
                                        eCreateItemType createItemType,
                                        string from = "")
        {
            return mImpl.AddItemOrMail(this, mailId, items, datas, createItemType, from);
        }

        //增加物品，用邮件
        public ErrorCodes AddItemByMail(int mailId,
                                        Dictionary<int, int> items,
                                        List<ItemBaseData> datas,
                                        eCreateItemType createItemType,
                                        string from)
        {
            return mImpl.AddItemByMail(this, mailId, items, datas, createItemType, from);
        }

        public ErrorCodes AddMailItems(int mailId, eCreateItemType createItemType)
        {
            return mImpl.AddMailItems(this, mailId, createItemType);            
        }

        public ErrorCodes AddRechargeRetMailItems(int mailId,List<int> countList, eCreateItemType createItemType)
        {
            return mImpl.AddRechargeRetMailItems(this, mailId, countList, createItemType);
        }

        //添加道具
        /// <summary>
        ///     添加道具
        /// </summary>
        /// <param name="nId"> 物品ID</param>
        /// <param name="nCount">物品数量</param>
        /// <param name="createItemType"></param>
        /// <returns></returns>
        public ItemBase AddItemGetItem(int nId, int nCount, eCreateItemType createItemType)
        {
            return mImpl.AddItemGetItem(this, nId, nCount, createItemType);
        }

        //删除道具
        /// <summary>
        ///     删除道具
        /// </summary>
        /// <param name="nId">道具ID</param>
        /// <param name="nCount">道具数量</param>
        /// <param name="deleteItemType"></param>
        /// <param name="exData"></param>
        /// <returns></returns>
        public ErrorCodes DeleteItem(int nId, int nCount, eDeleteItemType deleteItemType, string exData = "")
        {
            return mImpl.DeleteItem(this, nId, nCount, deleteItemType, exData);
        }

        //移动道具
        /// <summary>
        ///     移动道具
        /// </summary>
        /// <param name="lBagType">从这个包裹</param>
        /// <param name="lBagIndex">的这个索引</param>
        /// <param name="rBagType">移动到这个包裹</param>
        /// <param name="rBagIndex">的这个索引</param>
        /// <param name="itemCount">移动数量</param>
        /// <returns></returns>
        public ErrorCodes MoveItem(int lBagType, int lBagIndex, int rBagType, int rBagIndex, int itemCount)
        {
            return mImpl.MoveItem(this, lBagType, lBagIndex, rBagType, rBagIndex, itemCount);
        }

        //交换两个物品
        public void SwapItem(ItemBase lItem, ItemBase rItem)
        {
            mImpl.SwapItem(this, lItem, rItem);
        }

        //获得道具数量
        /// <summary>
        ///     获得道具数量
        /// </summary>
        /// <param name="nId">资源ID</param>
        /// <returns></returns>
        public int GetItemCount(int nId)
        {
            return mImpl.GetItemCount(this, nId);
        }

        public int GetItemCountEx(int nId)
        {
            return mImpl.GetItemCountEx(this, nId);
        }

        //许愿池增加道具
        public ErrorCodes AddItemToWishingPool(int nId, int nCount, List<ItemBaseData> resultItems)
        {
            return mImpl.AddItemToWishingPool(this, nId, nCount, resultItems);
        }

        //占星台增加道具
        public ErrorCodes AddItemToAstrologyBag(int nId, int nCount, List<ItemBaseData> resultItems)
        {
            return mImpl.AddItemToAstrologyBag(this, nId, nCount, resultItems);
        }

        //精灵增加道具
        public ErrorCodes AddItemToElf(int nId, int nCount, List<ItemBaseData> resultItems)
        {
            return mImpl.AddItemToElf(this, nId, nCount, resultItems);
        }

        #endregion

        #region  资源相关(增删改查)

        //增加资源（非道具）
        /// <summary>
        ///     增加资源（非道具）
        /// </summary>
        /// <param name="type">资源ID</param>
        /// <param name="nCount">资源数量</param>
        /// <param name="createItemType"></param>
        /// <returns></returns>
        public ErrorCodes AddRes(eResourcesType type, int nCount, eCreateItemType createItemType)
        {
            return mImpl.AddRes(this, type, nCount, createItemType);
        }

        //删除资源（非道具）
        /// <summary>
        ///     删除资源（非道具）
        /// </summary>
        /// <param name="type">资源ID</param>
        /// <param name="nCount">资源数量</param>
        /// <param name="createItemType"></param>
        /// <returns></returns>
        public ErrorCodes DelRes(eResourcesType type, int nCount, eDeleteItemType createItemType)
        {
            return mImpl.DelRes(this, type, nCount, createItemType);
        }

        //设置资源（非道具）
        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="count"></param>
        public void SetRes(eResourcesType type, int count)
        {
            mImpl.SetRes(this, type, count);
        }

        //获取资源（非道具）
        public int GetRes(eResourcesType type)
        {
            return mImpl.GetRes(this, type);
        }

        //设置等级
        public int SetExp(int nExp)
        {
            return mImpl.SetExp(this, nExp);
        }

        //计算vip等级
        public int CaculateVipLevel(int exp, int level)
        {
            return mImpl.CaculateVipLevel(this, exp, level);
        }

        #endregion

        #region 整理

        public BagBase GetBag(int BagId)
        {
            return mImpl.GetBag(this, BagId);
        }

        public BagBase SortBag(int BagId)
        {
            return mImpl.SortBag(this, BagId);
        }

        #endregion

        #region   特定接口(经验)

        public int GetLevel()
        {
            return mImpl.GetLevel(this);
        }

        public void AddExp(int addExp, eCreateItemType createItemType)
        {
            mImpl.AddExp(this, addExp, createItemType);
        }

        #endregion

        #region   属性相关

        //刷新精灵属性
        public void RefreshElfAttr()
        {
            mImpl.RefreshElfAttr(this);
        }

        //刷新勋章属性
        public void RefreshMedal()
        {
            mImpl.RefreshMedal(this);
        }


        //刷新宝石属性
        public void RefreshGemAttr()
        {
            mImpl.RefreshGemAttr(this);
        }

        #endregion
    }

    #region  测试

    public class TestBag
    {
        public BagManager m_testBag;

        private IEnumerator Init(Coroutine coroutine, ulong characterId)
        {
            return null;

            //PlayerManager4Server<CharacterLogic>.GetPlayerGet(0, out character);
        }
    }

    #endregion
}