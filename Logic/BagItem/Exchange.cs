#region using

using System;
using System.Collections.Generic;
using DataContract;
using DataTable;
using NLog;
using Shared;

#endregion

namespace Logic
{
    public interface IExchangeItem
    {
        void ExchangeItem(ExchangeItem _this, CharacterController character);
        void ExchangeItem(ExchangeItem _this, CharacterController character, long guid, ItemBaseData item);
        void ExchangeItem(ExchangeItem _this, CharacterController character, DBStoreLogicOne dbdata);
        void ResetData(ExchangeItem _this, long guid, ItemBaseData itemBase, int needCount);
    }

    public class ExchangeItemDefaultImpl : IExchangeItem
    {
        public void ExchangeItem(ExchangeItem _this, CharacterController character)
        {
            _this.mCharacter = character;
            _this.mDbdata = new DBStoreLogicOne();
            _this.mDbdata.State = (int) StoreItemType.Free;
        }

        public void ExchangeItem(ExchangeItem _this, CharacterController character, long guid, ItemBaseData item)
        {
            _this.mCharacter = character;
            _this.mDbdata = new DBStoreLogicOne();
            _this.mDbdata.Id = guid;
            _this.mDbdata.ItemData = item;
        }

        public void ExchangeItem(ExchangeItem _this, CharacterController character, DBStoreLogicOne dbdata)
        {
            _this.mCharacter = character;
            _this.mDbdata = dbdata;
        }

        public void ResetData(ExchangeItem _this, long guid, ItemBaseData itemBase, int needCount)
        {
            _this.mDbdata.Id = guid;
            _this.mDbdata.ItemData = itemBase;
            _this.mDbdata.StartTime = DateTime.Now.ToBinary();
            _this.mDbdata.NeedCount = needCount;
            _this.State = StoreItemType.Normal;
        }
    }

    public class ExchangeItem : NodeBase
    {
        private static IExchangeItem mImpl;

        static ExchangeItem()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (ExchangeItem), typeof (ExchangeItemDefaultImpl),
                o => { mImpl = (IExchangeItem) o; });
        }

        public ExchangeItem(CharacterController character)
        {
            mImpl.ExchangeItem(this, character);
        }

        public ExchangeItem(CharacterController character, long guid, ItemBaseData item)
        {
            mImpl.ExchangeItem(this, character, guid, item);
        }

        public ExchangeItem(CharacterController character, DBStoreLogicOne dbdata)
        {
            mImpl.ExchangeItem(this, character, dbdata);
        }

        public CharacterController mCharacter; //所在角色
        public DBStoreLogicOne mDbdata;

        public override IEnumerable<NodeBase> Children
        {
            get { return null; }
        }

        public int ItemCount
        {
            get { return mDbdata.ItemData.Count; }
            set { mDbdata.ItemData.Count = value; }
        }

        public int ItemId
        {
            get { return mDbdata.ItemData.ItemId; }
            set { mDbdata.ItemData.ItemId = value; }
        }

        public int NeedCount
        {
            get { return mDbdata.NeedCount; }
            set { mDbdata.NeedCount = value; }
        }

        public int NeedType
        {
            get { return mDbdata.NeedType; }
            set { mDbdata.NeedType = value; }
        }

        public StoreItemType State
        {
            get { return (StoreItemType) mDbdata.State; }
            set { mDbdata.State = (int) value; }
        }

        public void ResetData(long guid, ItemBaseData itemBase, int needCount)
        {
            mImpl.ResetData(this, guid, itemBase, needCount);
        }
    }

    public interface IExchange
    {
        ErrorCodes CancelItem(Exchange _this, long id, ref ExchangeItem resultItem);
        void ChechTemp(Exchange _this, StoreBroadcastList temp);
        int GetBroadcastMinutes(Exchange _this);
        StoreBroadcastList GetChech(Exchange _this, int count);
        ExchangeItem GetItemByStoreId(Exchange _this, long storeId);
        ErrorCodes Harvest(Exchange _this, long storeId);
        DBCharacterStoreData InitByBase(Exchange _this, CharacterController character);
        void InitByDB(Exchange _this, CharacterController character, DBCharacterStoreData storeData);

        ErrorCodes PushItem(Exchange _this,
                            int type,
                            int bagId,
                            int bagIndex,
                            int count,
                            int needtype,
                            int needCount,
                            int storeIndex,
                            ref ExchangeItem resultItem);

        void ResetCount(Exchange _this, int count);
    }

    public class ExchangeDefaultImpl : IExchange
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); //

        #region 初始化

        //创建时的初始化
        public DBCharacterStoreData InitByBase(Exchange _this, CharacterController character)
        {
            var dbData = new DBCharacterStoreData();
            _this.mDbData = dbData;
            _this.mCharacter = character;
            _this.MarkDirty();
            return dbData;
        }

        public void InitByDB(Exchange _this, CharacterController character, DBCharacterStoreData storeData)
        {
            _this.mCharacter = character;
            _this.mDbData = storeData;
            foreach (var dbItem in storeData.StoreItems)
            {
                var item = new ExchangeItem(character, dbItem);
                _this.mDataList.Add(item);
                if (item.State != StoreItemType.Free)
                {
                    _this.mData.Add(dbItem.Id, item);
                }
                _this.AddChild(item);
            }
        }

        #endregion

        #region 对外方法

        //重新设置商店的空格
        public void ResetCount(Exchange _this, int count)
        {
            if (_this.mDataList.Count > count)
            {
                return;
            }
            for (var i = _this.mDataList.Count; i < count; ++i)
            {
                var temp = new ExchangeItem(_this.mCharacter);
                _this.mDataList.Add(temp);
                _this.mDbData.StoreItems.Add(temp.mDbdata);
            }
        }

        //商店放入一个道具
        public ErrorCodes PushItem(Exchange _this,
                                   int type,
                                   int bagId,
                                   int bagIndex,
                                   int count,
                                   int needType,
                                   int needCount,
                                   int storeIndex,
                                   ref ExchangeItem resultItem)
        {
            //参数条件检查
            var bag = _this.mCharacter.mBag.GetBag(bagId);
            if (bag == null)
            {
                return ErrorCodes.Error_BagID;
            }
            var item = bag.GetItemByIndex(bagIndex);
            if (item == null || item.GetId() == -1)
            {
                return ErrorCodes.Error_ItemNotFind;
            }
            if (storeIndex < 0 || storeIndex >= _this.mDataList.Count)
            {
                return ErrorCodes.Error_DataOverflow;
            }
            if (item.GetCount() < count)
            {
                return ErrorCodes.Error_CountNotEnough;
            }
            var storeItem = _this.mDataList[storeIndex];
            if (storeItem.State != StoreItemType.Free)
            {
                return ErrorCodes.Error_ExchangeItemState;
            }
            var tbItem = Table.GetItemBase(item.GetId());
            if (tbItem == null)
            {
                return ErrorCodes.Error_ItemID;
            }


            var equip = item as ItemEquip2;
            if (equip != null)
            {
                if (equip.GetBinding())
                {
                    return ErrorCodes.Error_ItemNoExchange;
                }
            }
            if (type != -1)
            {
                if (!BitFlag.GetLow(tbItem.CanTrade, 0))
                {
                    return ErrorCodes.Error_ItemNoExchange;
                }

                if (needType == 0)
                {
                    if (tbItem.BuyNeedCount*count > needCount)
                    {
                        return ErrorCodes.Error_ExchangeValueNotEnough;
                    }
                }
                else if (needType == 1)
                {
                    if (needCount < StaticParam.AuctionMinValue)
                    {
                        return ErrorCodes.Error_ExchangeValueNotEnough;
                    }
                }
                else
                {
                    Logger.Error("PushItem type={0},needType={1}", type, needType);
                }
            }
            else
            {
                if (!BitFlag.GetLow(tbItem.CanTrade, 1))
                {
                    return ErrorCodes.Error_ItemNoExchange;
                }

                if (needType == 10)
                {
                    if (tbItem.BuyNeedCount*count > needCount)
                    {
                        return ErrorCodes.Error_ExchangeValueNotEnough;
                    }
                }
                else if (needType == 11)
                {
                    if (needCount < StaticParam.AuctionMinValue)
                    {
                        return ErrorCodes.Error_ExchangeValueNotEnough;
                    }
                }
                else
                {
                    Logger.Error("PushItem type={0},needType={1}", type, needType);
                }
            }
            //是否有消耗
            switch (type)
            {
                case -1: //拍卖行
                    break;
                case 0: //正常不广播
                    break;
                case 1: //正常广播
                {
                    if (DateTime.FromBinary(_this.mDbData.NextFreeTime) > DateTime.Now)
                    {
                        return ErrorCodes.Error_ExchangeFreeBroadcast;
                    }
                    _this.mDbData.NextFreeTime = DateTime.Now.AddSeconds(GetBroadcastCD(_this)).ToBinary();
                }
                    break;
                case 2: //购买广播
                {
                    if (DateTime.FromBinary(_this.mDbData.NextFreeTime) > DateTime.Now)
                    {
                        if (_this.mCharacter.mBag.GetRes(eResourcesType.DiamondRes) < Exchange.BuyBroadcastNeedRes)
                        {
                            return ErrorCodes.DiamondNotEnough;
                        }
                        _this.mCharacter.mBag.DelRes(eResourcesType.DiamondRes, Exchange.BuyBroadcastNeedRes,
                            eDeleteItemType.ExchangeBroadcast);
                        _this.mDbData.NextFreeTime = DateTime.Now.AddSeconds(GetBroadcastCD(_this)).ToBinary();
                    }
                }
                    break;
            }
            //执行
            var guid = GetNextId(_this);
            var itemBaseData = new ItemBaseData();
            itemBaseData.ItemId = item.GetId();
            itemBaseData.Count = count;
            item.CopyTo(itemBaseData.Exdata);
            storeItem.ResetData(guid, itemBaseData, needCount);
            storeItem.NeedType = needType;
            if (type == -1)
            {
                bag.ReduceCountByIndex(bagIndex, count, eDeleteItemType.AuctionPush);
            }
            else
            {
                bag.ReduceCountByIndex(bagIndex, count, eDeleteItemType.ExchangePush);
            }
            _this.mData.Add(guid, storeItem);
            resultItem = storeItem;
            return ErrorCodes.OK;
        }

        //收回一个商店放入的道具
        public ErrorCodes CancelItem(Exchange _this, long id, ref ExchangeItem resultItem)
        {
            ExchangeItem eItem;
            if (_this.mData.TryGetValue(id, out eItem))
            {
                if (eItem.State == StoreItemType.Normal)
                {
                    var result = _this.mCharacter.mBag.CheckAddItem(eItem.ItemId, eItem.ItemCount);
                    if (result != ErrorCodes.OK)
                    {
                        return result;
                    }
                    _this.mCharacter.mBag.AddItem(eItem.mDbdata.ItemData, eCreateItemType.ExchangeCancel);
                    eItem.State = StoreItemType.Free;
                    resultItem = eItem;
                    _this.mData.Remove(id);
                    return ErrorCodes.OK;
                }
                return ErrorCodes.Error_ExchangeItemState;
            }
            return ErrorCodes.Error_ItemNotFind;
        }

        //缓存商店可看到的其他玩家
        public void ChechTemp(Exchange _this, StoreBroadcastList temp)
        {
            _this.ChechTempList = temp;
            _this.ChechOverTime = DateTime.FromBinary(temp.CacheOverTime);
                //DateTime.Now.AddMinutes(Exchange.RefreshCDTime);
        }

        //获取某一个道具
        public ExchangeItem GetItemByStoreId(Exchange _this, long storeId)
        {
            ExchangeItem ei;
            if (_this.mData.TryGetValue(storeId, out ei))
            {
                return ei;
            }
            return null;
        }

        //获取缓存的内容
        public StoreBroadcastList GetChech(Exchange _this, int count)
        {
            if (DateTime.Now > _this.ChechOverTime)
            {
                return null;
            }
            if (count != _this.ChechTempList.Items.Count)
            {
                return null;
            }
            return _this.ChechTempList;
        }

        //收获贩卖掉的道具
        public ErrorCodes Harvest(Exchange _this, long storeId)
        {
            var item = GetItemByStoreId(_this, storeId);
            if (item == null)
            {
                return ErrorCodes.Error_ItemNotFind;
            }
            if (item.State != StoreItemType.Buyed)
            {
                return ErrorCodes.Error_ExchangeItemState;
            }
            var b = _this.mCharacter.mCity.GetBuildByType((int) BuildingType.Exchange);
            var res = 1500.0f;
            if (b != null)
            {
                res = b.TbBs.Param[4];
                //if (b.TbBuild.Level > 1)
                //{
                //    res = res - b.TbBuild.Level * 0.01f;
                //}
            }
            var needGive = (int) (item.NeedCount*(10000 - res)/10000);
            if (item.NeedType == 0 || item.NeedType == 10)
            {
                _this.mCharacter.mBag.AddRes(eResourcesType.Other16, needGive, eCreateItemType.ExchangeHarvest);
            }
            else
            {
                _this.mCharacter.mBag.AddRes(eResourcesType.DiamondRes, needGive, eCreateItemType.ExchangeHarvest);
            }
            _this.mData.Remove(storeId);
            item.State = StoreItemType.Free;
            return ErrorCodes.OK;
        }

        #endregion

        #region 私有方法

        //获取下一个ID
        private long GetNextId(Exchange _this)
        {
            _this.mDbData.NextItem++;
            return _this.mDbData.NextItem;
        }

        //取广播CD时间
        private int GetBroadcastCD(Exchange _this)
        {
            var cdTimes = Exchange.BroadcastCDTime*60;
            var build = _this.mCharacter.mCity.GetBuildByType((int) BuildingType.Exchange);
            if (build == null)
            {
                return cdTimes;
            }
            var pets = build.GetPets();
            var refTimes = BuildingBase.GetBSParamByIndex((int) BuildingType.Exchange, build.TbBs, pets, 2);
            return cdTimes*refTimes/10000;
        }

        //获取广播持续时间
        public int GetBroadcastMinutes(Exchange _this)
        {
            var build = _this.mCharacter.mCity.GetBuildByType((int) BuildingType.Exchange);
            if (build == null)
            {
                return 0;
            }
            var pets = build.GetPets();
            var refTimes = BuildingBase.GetBSParamByIndex((int) BuildingType.Exchange, build.TbBs, pets, 3);
            return Exchange.BroadcastTime*refTimes/10000;
        }

        #endregion
    }

    public class Exchange : NodeBase
    {
        #region 节点相关

        public override IEnumerable<NodeBase> Children
        {
            get { return mData.Values; }
        }

        #endregion

        #region 私有方法

        //获取广播持续时间
        public int GetBroadcastMinutes()
        {
            return mImpl.GetBroadcastMinutes(this);
        }

        #endregion

        #region 数据结构

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); //
        public CharacterController mCharacter; //所在角色
        public DBCharacterStoreData mDbData { get; set; }
        public List<ExchangeItem> mDataList = new List<ExchangeItem>(); //商店列表
        public Dictionary<long, ExchangeItem> mData = new Dictionary<long, ExchangeItem>(); //有货的道具
        public static int BroadcastCDTime = Table.GetServerConfig(300).ToInt();
        public static int BroadcastTime = Table.GetServerConfig(301).ToInt(); //广播持续时间
        public static int BuyBroadcastNeedRes = Table.GetServerConfig(302).ToInt(); //购买广播需求价格

        public StoreBroadcastList ChechTempList;
        public DateTime ChechOverTime;
        public static int RefreshCDTime = Table.GetServerConfig(304).ToInt(); //刷新商店内容的CD时间

        #endregion

        #region 初始化

        private static IExchange mImpl;

        static Exchange()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (Exchange), typeof (ExchangeDefaultImpl),
                o => { mImpl = (IExchange) o; });
        }

        //创建时的初始化
        public DBCharacterStoreData InitByBase(CharacterController character)
        {
            return mImpl.InitByBase(this, character);
        }

        public void InitByDB(CharacterController character, DBCharacterStoreData storeData)
        {
            mImpl.InitByDB(this, character, storeData);
        }

        #endregion

        #region 对外方法

        //重新设置商店的空格
        public void ResetCount(int count)
        {
            mImpl.ResetCount(this, count);
        }

        //商店放入一个道具
        public ErrorCodes PushItem(int type,
                                   int bagId,
                                   int bagIndex,
                                   int count,
                                   int needtype,
                                   int needCount,
                                   int storeIndex,
                                   ref ExchangeItem resultItem)
        {
            return mImpl.PushItem(this, type, bagId, bagIndex, count, needtype, needCount, storeIndex, ref resultItem);
        }

        //收回一个商店放入的道具
        public ErrorCodes CancelItem(long id, ref ExchangeItem resultItem)
        {
            return mImpl.CancelItem(this, id, ref resultItem);
        }

        //缓存商店可看到的其他玩家
        public void ChechTemp(StoreBroadcastList temp)
        {
            mImpl.ChechTemp(this, temp);
        }

        //获取某一个道具
        public ExchangeItem GetItemByStoreId(long storeId)
        {
            return mImpl.GetItemByStoreId(this, storeId);
        }

        //获取缓存的内容
        public StoreBroadcastList GetChech(int count)
        {
            return mImpl.GetChech(this, count);
        }

        //收获贩卖掉的道具
        public ErrorCodes Harvest(long storeId)
        {
            return mImpl.Harvest(this, storeId);
        }

        #endregion
    }
}