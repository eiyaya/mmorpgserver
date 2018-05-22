#region using

using System;
using System.Collections.Generic;
using DataContract;
using DataTable;
using EventSystem;
using NLog;
using Shared;

#endregion

namespace Logic
{
    public interface IPetPiece
    {
        void Init();
    }

    public class PetPieceDefaultImpl : IPetPiece
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Init()
        {
            Table.ForeachItemBase(record =>
            {
                if (record.Type == 70000)
                {
                    var tbItem = Table.GetItemBase(record.Exdata[2]);
                    if (tbItem == null)
                    {
                        Logger.Warn("PetPiece Init petnotFind Piece={0}", record.Id);
                        return true;
                    }
                    if (tbItem.Type != 60000)
                    {
                        Logger.Warn("PetPiece Init itemType not Pet! Piece={0}", record.Id);
                        return true;
                    }
                    var tbPet = Table.GetPet(tbItem.Id);
                    if (tbPet == null)
                    {
                        Logger.Warn("PetPiece Init not find Pet! Piece={0},PetId = {1}", record.Id, tbItem.Id);
                        return true;
                    }
                    if (tbPet.IsCanView == 1)
                    {
                        PetPiece.pets.Add(record.Id, record);
                    }
                }
                return true;
            });
        }
    }

    public static class PetPiece
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        private static IPetPiece mImpl;
        public static Dictionary<int, ItemBaseRecord> pets = new Dictionary<int, ItemBaseRecord>();

        static PetPiece()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (PetPiece), typeof (PetPieceDefaultImpl),
                o => { mImpl = (IPetPiece) o; });
        }

        public static void Init()
        {
            mImpl.Init();
        }
    }

    public enum BagChange
    {
        AddCount = 0, //增加堆叠数量
        ReduceCount = 1, //减少堆叠数量
        CleanItem = 2, //这个物品没了
        AddItem = 3, //新增道具
        IdChange = 4, //物品ID发生了变化
        ExdataChange = 5, //扩展数据发生变化
        BagChangeCount
    }

    public interface IBagBase
    {
        void AddChange(BagBase _this, int nIndex, BagChange bc);
        ErrorCodes CheckAddItem(BagBase _this, int nId, int nCount);
        ErrorCodes CheckItem(BagBase _this, int nId);
        ErrorCodes CheckItemId(BagBase _this, int nId);
        void CleanItemByIndex(BagBase _this, int nIndex);

        int ForceAddItem(BagBase _this,
                         int nId,
                         int nCount,
                         CharacterController character,
                         eCreateItemType createItemType);

        ItemBase ForceAddItem_GetItem(BagBase _this,
                                      int nId,
                                      int nCount,
                                      CharacterController character,
                                      eCreateItemType createItemType);

        int ForceAddItemByDb(BagBase _this,
                             ItemBaseData itemDb,
                             CharacterController character,
                             eCreateItemType createItemType);

        int ForceAddMultiItem(BagBase _this, int nId, int nCount);
        int ForceAddNewItem(BagBase _this, int nId, int nCount);
        int ForceAddNewItem(BagBase _this, ItemBaseData itemDb);
        ItemBase ForceAddNewItem_GetItem(BagBase _this, int nId, ref int nLast);
        int ForceDeleteItem(BagBase _this, int nId, int nCount);
        ItemBase GetFirstByItemId(BagBase _this, int id);
        int GetFirstFreeIndex(BagBase _this, int begin = 0);
        int GetFreeCount(BagBase _this);
        ItemBase GetItemByIndex(BagBase _this, int nIndex);
        int GetItemCount(BagBase _this, int nId);
        int GetNeedTime(BagBase _this);
        int GetNeedTime(BagBase _this, int index, BagBaseRecord tbBag);
        int GetNeedKey(BagBase _this, int index, BagBaseRecord tbBag);
        int GetNoFreeCount(BagBase _this);
        PetItem GetSamePetByPetId(BagBase _this, int petId);
        void InitByBase(BagBase _this, int nBagId, int nNowCount);
        void InitByDB(BagBase _this, BagBaseData dbData, CharacterController character);
        void ReduceCountByIndex(BagBase _this, int nIndex, int nCount, eDeleteItemType deleteItemType);
        void ResetItemByItem(BagBase _this, int nIndex, ItemBase item, int nCount);
        void ResetItemByItemId(BagBase _this, int nIndex, int nItemId);
        void SetCountByIndex(BagBase _this, int nIndex, int nCount);
        void SetNowCount(BagBase _this, int nNowCount);
    }

    public class BagBaseDefaultImpl : IBagBase
    {
        #region 标记相关

        //public void CleanChange() { m_Changes.Clear(); }
        public void AddChange(BagBase _this, int nIndex, BagChange bc)
        {
            //if(!m_Changes.ContainsKey(nIndex))
            //{
            //    m_Changes[nIndex] = new BitFlag((int)BagChange.BagChangeCount);
            //}
            //m_Changes[nIndex].SetFlag((int)bc);
            //mCharacter.mBag.SetFlag();
        }

        #endregion

        #region 初始化

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// 初始化（按初始配置）
        /// <summary>
        ///     初始化（按初始配置）
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="nBagId">包裹ID</param>
        /// <param name="nNowCount">包裹当前容量</param>
        public void InitByBase(BagBase _this, int nBagId, int nNowCount)
        {
            _this.mDbData = new BagBaseData();
            _this.SetBagId(nBagId);
            SetNowCount(_this, nNowCount);
            _this.SetNextTime(GetNeedTime(_this));
            if (nBagId == (int) eBagType.Pet)
            {
                var index = 0;
                foreach (var pair in PetPiece.pets)
                {
                    if (index >= _this.mLogics.Count)
                    {
                        break;
                    }
                    var record = pair.Value;
                    var dbBase = _this.mLogics[index].mDbData;
                    var ib = new PetItem(record.Exdata[2], dbBase);
                    ib.SetState(PetStateType.Piece);
                    _this.mLogics[index] = ib;
                    //ib.SetIndex(index);
                    _this.AddChild(ib);
                    index++;
                }
                //Table.ForeachItemBase(record =>
                //{
                //    if (record.Type == 70000)
                //    {
                //        if (index < mLogics.Count)
                //        {
                //            ItemBaseData dbBase = mLogics[index].mDbData;
                //            PetItem ib = new PetItem(record.Exdata[2], dbBase);
                //            ib.SetState(PetStateType.Piece);
                //            mLogics[index] = ib;
                //            AddChild(ib);
                //            index++;
                //        }
                //    }
                //    return true;
                //});
            }
        }

        //初始化（按数据库配置）
        public void InitByDB(BagBase _this, BagBaseData dbData, CharacterController character)
        {
            _this.mDbData = dbData;
            _this.mCharacter = character;
            var index = 0;
            var index2 = 0;
            var pets = new Dictionary<int, int>();
            foreach (var item in dbData.Items)
            {
                var temp = ShareItemFactory.CreateByDb(item); //new ItemBase(dbData.BagId, item);
                if (temp == null)
                {
                    temp = new ItemBase(item);
                }
                if (temp.GetIndex() != index)
                {
                    Logger.Warn("bag InitByDB oldIndex={0},newIndex={1}", temp.GetIndex(), index);
                    temp.SetIndex(index);
                }
                temp.SetBagId(_this.GetBagId());
                _this.AddChild(temp);
                _this.mLogics.Add(temp);
                index++;
                if (_this.GetBagId() == (int) eBagType.Pet && item.ItemId > 0)
                {
                    var tbPet = Table.GetPet(item.ItemId);
                    if (tbPet == null)
                    {
						Logger.Fatal("null==Table.GetPet({0})", item.ItemId);
                        continue;
                    }
                    pets[tbPet.NeedItemId] = 1;
                    index2++;
                }
            }
            if (_this.GetBagId() == (int) eBagType.Pet)
            {
                foreach (var pair in PetPiece.pets)
                {
                    if (index2 >= _this.mLogics.Count)
                    {
                        break;
                    }
                    if (pets.ContainsKey(pair.Key))
                    {
                        continue;
                    }
                    var record = pair.Value;
                    var dbBase = _this.mDbData.Items[index2];
                    var ib = new PetItem(record.Id, dbBase);
                    ib.SetState(PetStateType.Piece);
                    _this.mLogics[index2] = ib;
                    //_this.mDbData.Items[index2] = dbBase;
                    //mDbData.Items.Add(ib.mDbData);
                    ib.SetIndex(index2);
                    _this.AddChild(ib);
                    index2++;
                }
                //Table.ForeachItemBase(record =>
                //{
                //    if (record.Type == 70000)
                //    {
                //        if (pets.ContainsKey(record.Id))
                //        {
                //            if (index2 >= mLogics.Count)
                //            {
                //                return false;
                //            }
                //            ItemBaseData dbBase = mDbData.Items[index2];
                //            PetItem ib = new PetItem(record.Id, dbBase);
                //            ib.SetState(PetStateType.Piece);
                //            mLogics[index2]=ib;
                //            //mDbData.Items.Add(ib.mDbData);
                //            ib.SetIndex(index2);
                //            AddChild(ib);
                //            index2++;
                //        }
                //    }
                //    return true;
                //});
            }
        }

        #endregion

        #region 基础方法

        /*--基础方法    */

        public void SetNowCount(BagBase _this, int nNowCount)
        {
            _this.mDbData.NowCount = nNowCount;
            if (_this.mDbData.Items.Count < nNowCount)
            {
                for (var i = _this.mDbData.Items.Count; i != nNowCount; ++i)
                {
                    var ib = new ItemBase();
                    _this.mLogics.Add(ib);
                    _this.mDbData.Items.Add(ib.mDbData);
                    ib.SetIndex(i);
                    _this.AddChild(ib);
                }
            }
            //if (_this.mCharacter != null)
            //{
            //    if (_this.GetBagId() == 0)
            //    {
            //        _this.mCharacter.AddExData((int) eExdataDefine.e334, 1);
            //    }
            //    else if (_this.GetBagId() == 1)
            //    {
            //        _this.mCharacter.AddExData((int) eExdataDefine.e333, 1);
            //    }
            //}
        }

        //获得包裹的第一个空格
        public int GetFirstFreeIndex(BagBase _this, int begin = 0)
        {
            for (var i = begin; i < _this.GetNowCount(); ++i)
            {
                if (_this.mLogics[i].GetId() == -1)
                {
                    return i;
                }
            }
            return -1;
        }

        //根据物品ID取第一个一样的道具

        public ItemBase GetFirstByItemId(BagBase _this, int id)
        {
            for (var i = 0; i < _this.GetNowCount(); ++i)
            {
                if (_this.mLogics[i].GetId() == id)
                {
                    return _this.mLogics[i];
                }
            }
            return null;
        }

        //根据物品ID取第一个一样的道具

        public PetItem GetSamePetByPetId(BagBase _this, int petId)
        {
            var tbPet = Table.GetPet(petId);
            if (tbPet == null)
            {
                return null;
            }
            foreach (var itemBase in _this.mLogics)
            {
                if (itemBase.GetId() < 0)
                {
                    continue;
                }
                var tbPet1 = Table.GetPet(itemBase.GetId());
                if (tbPet1 == null)
                {
                    Logger.Error("GetSamePetByPetId = {0}", itemBase.GetId());
                    continue; //避免抛异常
                }
                if (tbPet1.NeedItemId == tbPet.NeedItemId)
                {
                    return itemBase as PetItem;
                }
            }
            return null;
        }

        #endregion

        #region 增加相关

        //直接给予物品（包裹数量不足也会给予物品，返回没加进去的物品数量)
        /// <summary>
        ///     直接给予物品（返回剩余数量)
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="nId">物品ID</param>
        /// <param name="nCount">物品数量</param>
        /// <param name="character"></param>
        /// <param name="createItemType"></param>
        /// <returns></returns>
        public int ForceAddItem(BagBase _this,
                                int nId,
                                int nCount,
                                CharacterController character,
                                eCreateItemType createItemType)
        {
            var ErrorResult = CheckItem(_this, nId);
            if (ErrorResult != ErrorCodes.OK)
            {
                return nCount;
            }
            var tb_item = Table.GetItemBase(nId);
            var nLast = nCount;
            if (tb_item.MaxCount > 1)
            {
                //可堆叠物品
                nLast = ForceAddMultiItem(_this, nId, nCount);
            }
            if (nLast > 0)
            {
                //创建新物品
                nLast = ForceAddNewItem(_this, nId, nLast);
            }
            if (character != null)
            {
                var addCount = nCount - nLast;
                var e = new ItemChange(character, nId, addCount);
                EventDispatcher.Instance.DispatchEvent(e);
                PlayerLog.DataLog(character.mGuid, "ia,{0},{1},{2}", nId, addCount, (int) createItemType);
                kafkaBagLog(_this, nId, tb_item, nCount, createItemType);
            }
            //ConditionManager.PushEvent(character, eEventType.ItemChange, nId, nCount - nLast);
            return nLast;
        }

        public int ForceAddItemByDb(BagBase _this,
                                    ItemBaseData itemDb,
                                    CharacterController character,
                                    eCreateItemType createItemType)
        {
            var nId = itemDb.ItemId;
            var ErrorResult = CheckItem(_this, nId);
            var nCount = itemDb.Count;
            if (ErrorResult != ErrorCodes.OK)
            {
                return nCount;
            }
            var tb_item = Table.GetItemBase(nId);
            var nLast = nCount;
            if (tb_item.MaxCount > 1)
            {
                //可堆叠物品
                nLast = ForceAddMultiItem(_this, nId, nCount);
            }
            if (nLast > 0)
            {
                //创建新物品
                nLast = ForceAddNewItem(_this, itemDb);
            }
            if (character != null)
            {
                var addCount = nCount - nLast;
                var e = new ItemChange(character, nId, addCount);
                EventDispatcher.Instance.DispatchEvent(e);
                PlayerLog.DataLog(character.mGuid, "ia,{0},{1},{2}", nId, addCount, (int) createItemType);
                kafkaBagLog(_this, nId, tb_item, nCount, createItemType);
            }
            //ConditionManager.PushEvent(character, eEventType.ItemChange, nId, nCount - nLast);
            return nLast;
        }

        //直接给予物品（返回添加物品的实例)
        public ItemBase ForceAddItem_GetItem(BagBase _this,
                                             int nId,
                                             int nCount,
                                             CharacterController character,
                                             eCreateItemType createItemType)
        {
            var ErrorResult = CheckItem(_this, nId);
            if (ErrorResult != ErrorCodes.OK)
            {
                return null;
            }
            var tb_item = Table.GetItemBase(nId);
            var nLast = nCount;
            if (tb_item.MaxCount > 1)
            {
                //可堆叠物品
                nLast = ForceAddMultiItem(_this, nId, nCount);
            }
            ItemBase tempItem = null;
            if (nLast > 0)
            {
                //创建新物品
                tempItem = ForceAddNewItem_GetItem(_this, nId, ref nLast);
            }
            if (character != null)
            {
                var addCount = nCount - nLast;
                var e = new ItemChange(character, nId, addCount);
                EventDispatcher.Instance.DispatchEvent(e);
                PlayerLog.DataLog(character.mGuid, "ia,{0},{1},{2}", nId, addCount, (int) createItemType);
                kafkaBagLog(_this, nId, tb_item, nCount, createItemType);
            }
            //ConditionManager.PushEvent(character, eEventType.ItemChange, nId, nCount - nLast);
            return tempItem;
        }

        private void kafkaBagLog(BagBase _this, int nId, ItemBaseRecord tb_item, int nCount, eCreateItemType createItemType)
        {
            try
            {
                if (_this.mCharacter != null)
                {
                    string str = string.Empty;
                    if (_this.GetBagId() == (int)eBagType.Piece)
                    {
                        str = "mowuzhi";
                    }
                    else if (nId == 22017 || nId == 22018)
                    {
                        str = nId.ToString();
                    }
                    else if (_this.GetBagId() == (int)eBagType.Elf)
                    {
                        str = "lingshou";
                    }
                    else if (tb_item.Type == 26900)
                    {
                        str = "lingshouguoshi";
                    }

                    if (!string.IsNullOrEmpty(str))
                    {
                        string v = string.Format(str + "get_info#{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                                         _this.mCharacter.serverId,
                                         _this.mCharacter.mGuid,
                                         nId,
                                         nCount,
                                         DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), // 时间
                                         (int)createItemType,
                                         _this.mCharacter.GetLevel());
                        PlayerLog.Kafka(v);
                    }


                    // 37测试专用 下次记得删除
                    string vv = string.Format("allitem_get_info#{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                                         _this.mCharacter.serverId,
                                         _this.mCharacter.mGuid,
                                         nId,
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
        }
        #endregion

        #region 删除相关

        //清除一个索引的物品
        /// <summary>
        ///     清除一个索引的物品
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="nIndex">包裹索引</param>
        public void CleanItemByIndex(BagBase _this, int nIndex)
        {
            var Item = _this.mLogics[nIndex];
            Item.SetId(-1);
            Item.SetCount(0);
            Item.CleanExdata();
            Item.MarkDirty();
            AddChange(_this, nIndex, BagChange.CleanItem);
        }

        //减少某个索引的物品数量
        /// <summary>
        ///     减少某个索引的物品数量
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="nIndex">包裹索引</param>
        /// <param name="nCount">减少的数量</param>
        /// <param name="deleteItemType"></param>
        public void ReduceCountByIndex(BagBase _this, int nIndex, int nCount, eDeleteItemType deleteItemType)
        {
            var item = _this.mLogics[nIndex];
            var nNow = item.GetCount();
            if (nNow > nCount)
            {
                if (deleteItemType != eDeleteItemType.None)
                {
                    PlayerLog.DataLog(_this.mCharacter.mGuid, "id,{0},{1},{2}", item.GetId(), nCount,
                        (int) deleteItemType);
                }
                item.SetCount(item.GetCount() - nCount);
                item.MarkDirty();
                AddChange(_this, nIndex, BagChange.ReduceCount);
            }
            else
            {
                if (deleteItemType != eDeleteItemType.None)
                {
                    PlayerLog.DataLog(_this.mCharacter.mGuid, "id,{0},{1},{2}", item.GetId(), nNow, (int) deleteItemType);
                }
                CleanItemByIndex(_this, nIndex);
            }
        }

        //强制删除某道具
        /// <summary>
        ///     强制删除某道具
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="nId">物品ID</param>
        /// <param name="nCount">物品数量</param>
        /// <returns></returns>
        public int ForceDeleteItem(BagBase _this, int nId, int nCount)
        {
            var ErrorResult = CheckItem(_this, nId);
            if (ErrorResult != ErrorCodes.OK)
            {
                return nCount;
            }
            //这里为了倒序从包裹中删除
            var nLast = nCount;
            for (var i = _this.mLogics.Count - 1; i >= 0; i--)
            {
                if (_this.mLogics[i].GetId() != nId)
                {
                    continue;
                }
                var nNowCount = _this.mLogics[i].GetCount();
                if (nNowCount > nLast)
                {
                    _this.mLogics[i].SetCount(nNowCount - nLast);
                    _this.mLogics[i].MarkDirty();
                    AddChange(_this, i, BagChange.ReduceCount);
                    nLast = 0;
                    return nLast;
                }
                if (nNowCount == nLast)
                {
                    CleanItemByIndex(_this, i);
                    nLast = 0;
                    return nLast;
                }
                nLast -= nNowCount;
                CleanItemByIndex(_this, i);
            }
            return nLast;
        }

        #endregion

        #region 修改相关

        //给某个索引设置道具
        /// <summary>
        ///     给某个索引设置道具
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="nIndex"></param>
        /// <param name="nItemId"></param>
        public void ResetItemByItemId(BagBase _this, int nIndex, int nItemId)
        {
            var oldItem = _this.mLogics[nIndex];
            var newItem = ShareItemFactory.Create(nItemId, oldItem.mDbData);
            if (newItem == null)
            {
                return;
            }
            _this.mLogics[nIndex] = newItem;
            newItem.SetIndex(nIndex);
            newItem.SetBagId(_this.GetBagId());
            _this.AddChild(newItem);
            newItem.MarkDirty();
            AddChange(_this, nIndex, BagChange.AddItem);
        }

        //给某个索引设置某个道具的镜像(不改变Item的任何数据)
        public void ResetItemByItem(BagBase _this, int nIndex, ItemBase item, int nCount)
        {
            var oldItem = _this.mLogics[nIndex];
            var newItem = ShareItemFactory.CreateNull(item.GetId(), oldItem.mDbData);
            newItem.CopyFrom(item);
            _this.mLogics[nIndex] = newItem;
            newItem.SetIndex(nIndex);
            newItem.SetBagId(_this.GetBagId());
            _this.AddChild(newItem);
            newItem.MarkDirty();
            AddChange(_this, nIndex, BagChange.AddItem);
        }

        //设置某个索引的物品数量
        /// <summary>
        ///     设置某个索引的物品数量
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="nIndex">包裹索引</param>
        /// <param name="nCount">新数量</param>
        public void SetCountByIndex(BagBase _this, int nIndex, int nCount)
        {
            var item = _this.mLogics[nIndex];
            var nOldCount = item.GetCount();
            item.SetCount(nCount);
            item.MarkDirty();
            if (nOldCount > nCount)
            {
                AddChange(_this, nIndex, BagChange.ReduceCount);
            }
            else if (nOldCount < nCount)
            {
                AddChange(_this, nIndex, BagChange.AddCount);
            }
        }

        #endregion

        #region 查询相关

        /// 检查某个索引是否有物品
        /// <summary>
        ///     检查某个索引是否有物品
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="nIndex">包裹索引</param>
        /// <returns></returns>
        public ItemBase GetItemByIndex(BagBase _this, int nIndex)
        {
            if (nIndex < 0 || nIndex >= _this.mLogics.Count)
            {
                return null;
            }
            if (_this.mLogics[nIndex].GetId() < 0)
            {
                return null;
            }
            return _this.mLogics[nIndex];
        }


        //检查物品ID是否非法，检查物品是否能放在该包裹
        public ErrorCodes CheckItem(BagBase _this, int nId)
        {
            var ErrorResult = CheckItemId(_this, nId);
            if (ErrorResult != ErrorCodes.OK)
            {
                return ErrorResult;
            }
            ErrorResult = CheckItemCanInBag(_this, nId);
            if (ErrorResult != ErrorCodes.OK)
            {
                return ErrorResult;
            }
            return ErrorCodes.OK;
        }

        /// 检查物品ID是否存在
        /// <summary>
        ///     检查物品ID是否存在
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="nId">物品ID</param>
        /// <returns></returns>
        public ErrorCodes CheckItemId(BagBase _this, int nId)
        {
            if (Table.GetItemBase(nId) == null)
            {
                return ErrorCodes.Error_ItemID;
            }
            return ErrorCodes.OK;
        }

        /// 检查物品Id是否能放在该包裹
        /// <summary>
        ///     检查物品Id是否能放在该包裹
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="nId">物品ID</param>
        /// <returns></returns>
        private ErrorCodes CheckItemCanInBag(BagBase _this, int nId)
        {
            var tb_item = Table.GetItemBase(nId);
            if (!BitFlag.GetLow(tb_item.CanInBag, _this.GetBagId()))
            {
                return ErrorCodes.Error_ItemNoInBag;
            }
            return ErrorCodes.OK;
        }

        /// 检查添加物品
        /// <summary>
        ///     检查添加物品
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="nId">物品ID</param>
        /// <param name="nCount">物品数量</param>
        /// <returns></returns>
        public ErrorCodes CheckAddItem(BagBase _this, int nId, int nCount)
        {
            var ErrorResult = CheckItem(_this, nId);
            if (ErrorResult != ErrorCodes.OK)
            {
                return ErrorResult;
            }
            var tb_item = Table.GetItemBase(nId);
            if (tb_item.Type == 60000)
            {
                return ErrorCodes.OK;
            }
            if (tb_item.Type == 70000)
            {
                return ErrorCodes.OK;
            }
            if (tb_item.Type == 10005)
            {
                return ErrorCodes.OK;
            }
            //itemRecord record = Table.item[(int)itemType];
            //检查空余
            var free = 0;
            var item_max = tb_item.MaxCount;
            var beginIndex = 0;
            if (_this.GetBagId() == (int) eBagType.Elf)
            {
                beginIndex = 3; //潜规则精灵包裹的前3个位置是出战用的
            }
            for (var i = beginIndex; i < _this.GetNowCount(); ++i)
            {
                if (_this.mLogics[i].GetId() < 0)
                {
                    free = free + item_max;
                }
                else if (nId == _this.mLogics[i].GetId())
                {
                    free = free + item_max - _this.mLogics[i].GetCount();
                }
                if (free >= nCount)
                {
                    return ErrorCodes.OK;
                }
            }
            //差距为： nCount - free;
            return ErrorCodes.Error_ItemNoInBag_All;
        }

        /// 获取物品数量
        /// <summary>
        ///     获取物品数量
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="nId">物品ID</param>
        /// <returns></returns>
        public int GetItemCount(BagBase _this, int nId)
        {
            var ErrorResult = CheckItem(_this, nId);
            if (ErrorResult != ErrorCodes.OK)
            {
                return 0;
            }
            var nCount = 0;
            foreach (var item in _this.mLogics)
            {
                if (item.GetId() == nId)
                {
                    nCount += item.GetCount();
                }
            }
            return nCount;
        }

        public int GetElfCount(BagBase _this)
        {
            if (_this.GetBagId() != (int) eBagType.Elf)
                return -1;
            var nCount = 0;
            for (var i = 0; i < _this.GetNowCount(); ++i)
            {
                if (_this.mLogics[i].GetId() > 0)
                {
                    nCount += 1;
                }
            }
            return nCount;
        }
        //获得当前所有非空格数
        public int GetNoFreeCount(BagBase _this)
        {
            var beginIndex = 0;
            if (_this.GetBagId() == (int) eBagType.Elf)
            {
                beginIndex = 3; //潜规则精灵包裹的前3个位置是出战用的
            }
            var nCount = 0;
            for (var i = beginIndex; i < _this.GetNowCount(); ++i)
            {
                if (_this.mLogics[i].GetId() > 0)
                {
                    nCount += 1;
                }
            }
            //foreach (var item in _this.mLogics)
            //{
            //    if (item.GetId() > 0)
            //    {
            //        nCount += 1;
            //    }
            //}
            return nCount;
        }

        //获得当前的所有空格数
        public int GetFreeCount(BagBase _this)
        {
            if (_this.GetBagId() == (int) eBagType.Elf)
            {
                return _this.GetNowCount() - GetNoFreeCount(_this) - 3;
            }
            return _this.GetNowCount() - GetNoFreeCount(_this);
        }

        #endregion

        #region 私有方法

        //强制增加可堆叠物品
        /// <summary>
        ///     强制增加可堆叠物品
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="nId">物品ID</param>
        /// <param name="nCount">物品数量</param>
        /// <returns></returns>
        public int ForceAddMultiItem(BagBase _this, int nId, int nCount)
        {
            var nLast = nCount;
            var tbItem = Table.GetItemBase(nId);
            if (tbItem == null)
            {
                return 0;
            }
            if (tbItem.Type == 70000 && _this.GetBagId() == (int) eBagType.Pet)
            {
                var pet = GetSamePetByPetId(_this, tbItem.Exdata[2]);
                if (pet != null)
                {
                    pet.AddPiece(nCount);
                }
                return 0;
            }
            var nIndex = 0;
            foreach (var item in _this.mLogics)
            {
                if (item.GetId() == nId)
                {
                    var increase = tbItem.MaxCount - item.GetCount(); //这个格子可增加的
                    if (0 < increase)
                    {
                        if (nLast <= increase)
                        {
//叠加无剩余
                            item.SetCount(item.GetCount() + nLast);
                            nLast = 0;
                            item.MarkDirty();
                            AddChange(_this, nIndex, BagChange.AddCount);
                            break;
                        }
                        //叠加有剩余
                        item.SetCount(tbItem.MaxCount);
                        item.MarkDirty();
                        AddChange(_this, nIndex, BagChange.AddCount);
                        nLast -= increase;
                    }
                }
                nIndex++;
            }
            return nLast;
        }

        //强制增加新物品
        /// <summary>
        ///     强制增加新物品
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="nId">物品ID</param>
        /// <param name="nCount">物品数量</param>
        /// <returns></returns>
        public int ForceAddNewItem(BagBase _this, int nId, int nCount)
        {
            var tb_item = Table.GetItemBase(nId);
            var nLast = nCount;
            var beginIndex = 0;
            if (_this.GetBagId() == (int) eBagType.Elf)
            {
                beginIndex = 3; //潜规则精灵包裹的前3个位置是出战用的
            }
            ItemBase item = null;
            for (var i = beginIndex; i < _this.GetNowCount(); ++i)
            {
                if (_this.mLogics[i].GetId() >= 0)
                {
                    continue;
                }

                //处理物品数量
                var count = nLast;
                if (nLast > tb_item.MaxCount)
                {
                    count = tb_item.MaxCount;
                }
                nLast -= count;

                //创建物品
                item = ShareItemFactory.Create(nId, _this.mLogics[i].mDbData);
                item.SetCount(count);
                item.SetIndex(i);
                item.SetBagId(_this.GetBagId());
                _this.mLogics[i] = item;
                _this.AddChild(item);
                item.MarkDirty();
                AddChange(_this, i, BagChange.AddItem);
                if (0 == nLast)
                {
                    break;
                }
            }

            if (_this.GetBagId() == (int) eBagType.Elf)
            {
                //精灵数量
                if (_this.mCharacter != null)
                {
                    _this.mCharacter.AddElfItem(item);

                    var oldCount = _this.mCharacter.GetExData((int) eExdataDefine.e87);
                    var nowCount = GetElfCount(_this);
                    if (nowCount > oldCount)
                    {
                        _this.mCharacter.SetExData((int) eExdataDefine.e87, nowCount);

	                    if (_this.mCharacter.GetExData((int) eExdataDefine.e328) <= 0)
	                    {
		                    _this.mCharacter.SetExData((int) eExdataDefine.e328, 1);
	                    }
                    }
                    
                }
            }
            bool b = false;
            if (_this.GetBagId() == (int)eBagType.EquipShiZhuang || _this.GetBagId() == (int)eBagType.EquipShiZhuangBag)
            {
                b = true;
                var count = _this.mCharacter.GetBag((int) eBagType.EquipShiZhuang).GetNoFreeCount() +
                _this.mCharacter.GetBag((int) eBagType.EquipShiZhuangBag).GetNoFreeCount();
                _this.mCharacter.SetExData(790,count);
            }
            if (_this.GetBagId() == (int)eBagType.WingShiZhuang || _this.GetBagId() == (int)eBagType.WingShiZhuangBag)
            {
                b = true;
                var count = _this.mCharacter.GetBag((int)eBagType.WingShiZhuang).GetNoFreeCount() +
                _this.mCharacter.GetBag((int)eBagType.WingShiZhuangBag).GetNoFreeCount();
                _this.mCharacter.SetExData(792, count);
            }
            if (_this.GetBagId() == (int)eBagType.WeaponShiZhuang || _this.GetBagId() == (int)eBagType.WeaponShiZhuangBag)
            {
                b = true;
                var count = _this.mCharacter.GetBag((int)eBagType.WeaponShiZhuang).GetNoFreeCount() +
                _this.mCharacter.GetBag((int)eBagType.WeaponShiZhuangBag).GetNoFreeCount();
                _this.mCharacter.SetExData(791, count);
            }
            if (b)
            {
                Table.ForeachFashionTitle(record =>
                {
                    if (record.Flag <= 0)
                        return true;
                    if (_this.mCharacter.GetFlag(record.Flag))
                        return true;
                    for (int i = 0; i < record.ExList.Count; i++)
                    {
                        var id = record.ExList[i];
                        var val = record.ValList[i];
                        if (_this.mCharacter.GetExData(id) < val)
                            return true;
                    }
                    _this.mCharacter.SetFlag(record.Flag);
                    if(record.Exdata>0)
                        _this.mCharacter.SetExData(record.Exdata,1);
                    return true;
                });
            }

            return nLast;
        }

        public ItemBase ForceAddNewItem_GetItem(BagBase _this, int nId, ref int nLast)
        {
            var tb_item = Table.GetItemBase(nId);
            var beginIndex = 0;
            if (_this.GetBagId() == (int) eBagType.Elf)
            {
                beginIndex = 3; //潜规则精灵包裹的前3个位置是出战用的
            }
            ItemBase lastItem = null;
            for (var i = beginIndex; i < _this.GetNowCount(); ++i)
            {
                if (_this.mLogics[i].GetId() >= 0)
                {
                    continue;
                }

                //处理物品数量
                var count = nLast;
                if (nLast > tb_item.MaxCount)
                {
                    count = tb_item.MaxCount;
                }
                nLast -= count;

                //创建物品
                lastItem = ShareItemFactory.Create(nId, _this.mLogics[i].mDbData);
                lastItem.SetCount(count);
                lastItem.SetIndex(i);
                lastItem.SetBagId(_this.GetBagId());
                _this.mLogics[i] = lastItem;
                _this.AddChild(lastItem);
                lastItem.MarkDirty();
                AddChange(_this, i, BagChange.AddItem);
                if (0 == nLast)
                {
                    break;
                }
            }

            if (_this.GetBagId() == (int) eBagType.Elf)
            {
                lastItem = _this.mCharacter.AddElfItem(lastItem);
                //精灵数量
                var oldCount = _this.mCharacter.GetExData((int) eExdataDefine.e87);
                var nowCount = GetElfCount(_this);
                if (nowCount > oldCount)
                {
                    _this.mCharacter.SetExData((int) eExdataDefine.e87, nowCount);
					if (_this.mCharacter.GetExData((int)eExdataDefine.e328) <= 0)
					{
						_this.mCharacter.SetExData((int)eExdataDefine.e328, 1);
					}
                }
               
            }
            return lastItem;
        }

        public int ForceAddNewItem(BagBase _this, ItemBaseData itemDb)
        {
            var nId = itemDb.ItemId;
            var nCount = itemDb.Count;
            var tb_item = Table.GetItemBase(nId);
            var nLast = nCount;
            var beginIndex = 0;
            if (_this.GetBagId() == (int) eBagType.Elf)
            {
                beginIndex = 3; //潜规则精灵包裹的前3个位置是出战用的
            }
            ItemBase item = null;
            for (var i = beginIndex; i < _this.GetNowCount(); ++i)
            {
                if (_this.mLogics[i].GetId() >= 0)
                {
                    continue;
                }

                //处理物品数量
                var count = nLast;
                if (nLast > tb_item.MaxCount)
                {
                    count = tb_item.MaxCount;
                }
                nLast -= count;

                //创建物品
                item = ShareItemFactory.CreateNull(nId, _this.mLogics[i].mDbData);
                item.mDbData.ItemId = itemDb.ItemId;
                item.SetCount(count);
                item.SetIndex(i);
                item.SetBagId(_this.GetBagId());
                item.mDbData.Exdata.Clear();
                item.mDbData.Exdata.AddRange(itemDb.Exdata);
                _this.mLogics[i] = item;
                _this.AddChild(item);
                item.MarkDirty();
                //item = mLogics[i];
                //AddChange(i, BagChange.AddItem);
                if (0 == nLast)
                {
                    break;
                }
            }

            if (_this.GetBagId() == (int) eBagType.Elf)
            {
                _this.mCharacter.AddElfItem(item);
                //精灵数量
                var oldCount = _this.mCharacter.GetExData((int) eExdataDefine.e87);
                var nowCount = GetElfCount(_this);
                if (nowCount > oldCount)
                {
                    _this.mCharacter.SetExData((int) eExdataDefine.e87, nowCount);
					if (_this.mCharacter.GetExData((int)eExdataDefine.e328) <= 0)
					{
						_this.mCharacter.SetExData((int)eExdataDefine.e328, 1);
					}
                }
                
            }
            return nLast;
        }

        #endregion

        #region 包裹的扩展方法

        //计算下次的时间
        public int GetNeedTime(BagBase _this)
        {
            var tbBag = Table.GetBagBase(_this.GetBagId());
            if (tbBag == null)
            {
                return -1;
            }
            if (tbBag.Expression == -1)
            {
                return -1;
            }
            var tbSkillUpgrading = Table.GetSkillUpgrading(tbBag.Expression);
            if (tbSkillUpgrading == null)
            {
                return -1;
            }
            return
                tbSkillUpgrading.GetSkillUpgradingValue((_this.GetNowCount() - tbBag.InitCapacity)/tbBag.ChangeBagCount)*
                tbBag.TimeMult*60;
        }

        //获取某个格子的等待时间
        public int GetNeedTime(BagBase _this, int index, BagBaseRecord tbBag)
        {
            if (index < 0 || index >= tbBag.MaxCapacity)
            {
                return -1;
            }
            if (index == _this.GetNowCount())
            {
                var lastTime = _this.GetNextTime() - (int) DateTime.Now.GetDiffSeconds(_this.mCharacter.OnlineTime) +
                               _this.RemoveBuyTimes;
                if (lastTime < 0)
                {
                    return 0;
                }
                return lastTime;
            }
            var tbSkillUpgrading = Table.GetSkillUpgrading(tbBag.Expression);
            if (tbSkillUpgrading == null)
            {
                return -1;
            }
            return tbSkillUpgrading.GetSkillUpgradingValue((index - tbBag.InitCapacity)/tbBag.ChangeBagCount)*
                   tbBag.TimeMult*60;
        }

        //获取某个格子所需钥匙数
        public int GetNeedKey(BagBase _this, int index, BagBaseRecord tbBag)
        {
            if (index < 0 || index >= tbBag.MaxCapacity)
            {
                return -1;
            }
            var tbSkillUpgrading = Table.GetSkillUpgrading(tbBag.Expression);
            if (tbSkillUpgrading == null)
            {
                return -1;
            }
            return tbSkillUpgrading.GetSkillUpgradingValue((index - tbBag.InitCapacity)/tbBag.ChangeBagCount);
        }

        ////尝试开格子
        //public ErrorCodes OpenCell()
        //{
        //    if (GetNowCount() >= Table.GetBagBase(GetBagId()).MaxCapacity)
        //    {
        //        return ErrorCodes.Unknow;
        //    }
        //    if (DateTime.Now.GetDiffSeconds(mCharacter.OnlineTime) < GetNextTime())
        //    {
        //        return ErrorCodes.Unknow;
        //    }
        //    SetNowCount(GetNowCount() + 1);
        //    SetNextTime(GetNeedTime());
        //    return ErrorCodes.OK;
        //}
        ////用钻石买格子
        //public ErrorCodes BuyCell(int useCount)
        //{
        //    int seconds = GetNextTime();
        //    var tbBag = Table.GetBagBase(GetBagId());
        //    if (GetNowCount() >= tbBag.MaxCapacity)
        //    {
        //        return ErrorCodes.Unknow;
        //    }
        //    int needCount = seconds/tbBag.TimeMult/60;
        //    if (needCount > useCount)
        //    {
        //        return ErrorCodes.Unknow;
        //    }
        //    if (mCharacter.mBag.GetRes(eResourcesType.DiamondRes) < needCount)
        //    {
        //        return ErrorCodes.DiamondNotEnough;
        //    }
        //    mCharacter.mBag.DelRes(eResourcesType.DiamondRes, needCount,eDeleteItemType.BagCellBuy);
        //    SetNowCount(GetNowCount() + 1);
        //    SetNextTime(GetNeedTime());
        //    return ErrorCodes.OK;
        //}

        #endregion
    }


    public class BagBase : NodeBase
    {
        public override IEnumerable<NodeBase> Children
        {
            get { return mLogics; }
        }

        #region 标记相关

        //public void CleanChange() { m_Changes.Clear(); }
        public void AddChange(int nIndex, BagChange bc)
        {
            mImpl.AddChange(this, nIndex, bc);
        }

        #endregion

        public ItemBase AddItemBase(int index)
        {
            var db = new ItemBaseData();
            mDbData.Items.Insert(index, db);
            var i = new ItemBase();
            i.mDbData = db;
            mLogics.Insert(index, i);
            return i;
        }

        #region  数据结构

        private static Logger Logger = LogManager.GetCurrentClassLogger();
        //private int m_nBagId;           //包裹ID
        //private int m_nNowCount;        //当前容量
        public CharacterController mCharacter; //所在角色
        //private int m_nMaxCount;        //最大容量
        public List<ItemBase> mLogics = new List<ItemBase>(); //物品列表
        public BagBaseData mDbData;
        //private Dictionary<int, BitFlag> m_Changes = new Dictionary<int, BitFlag>();  //被修改过的列表

        #endregion

        #region 初始化

        private static IBagBase mImpl;

        static BagBase()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (BagBase), typeof (BagBaseDefaultImpl),
                o => { mImpl = (IBagBase) o; });
        }

        //构造
        public BagBase(CharacterController character)
        {
            mCharacter = character;
        }

        /// 初始化（按初始配置）
        /// <summary>
        ///     初始化（按初始配置）
        /// </summary>
        /// <param name="nBagId">包裹ID</param>
        /// <param name="nNowCount">包裹当前容量</param>
        public void InitByBase(int nBagId, int nNowCount)
        {
            mImpl.InitByBase(this, nBagId, nNowCount);
        }

        //初始化（按数据库配置）
        public void InitByDB(BagBaseData dbData, CharacterController character)
        {
            mImpl.InitByDB(this, dbData, character);
        }

        #endregion

        #region 基础方法

        /*--基础方法    */

        public int GetBagId()
        {
            return mDbData.BagId;
        }

        public void SetBagId(int nBagId)
        {
            mDbData.BagId = nBagId;
        }

        public int GetNowCount()
        {
            return mDbData.NowCount;
        }

        public void SetNowCount(int nNowCount)
        {
            mImpl.SetNowCount(this, nNowCount);
        }

        //获得包裹的第一个空格
        public int GetFirstFreeIndex(int begin = 0)
        {
            return mImpl.GetFirstFreeIndex(this, begin);
        }

        //根据物品ID取第一个一样的道具

        public ItemBase GetFirstByItemId(int id)
        {
            return mImpl.GetFirstByItemId(this, id);
        }

        //根据物品ID取第一个一样的道具

        public PetItem GetSamePetByPetId(int petId)
        {
            return mImpl.GetSamePetByPetId(this, petId);
        }

        #endregion

        #region 增加相关

        //直接给予物品（包裹数量不足也会给予物品，返回没加进去的物品数量)
        /// <summary>
        ///     直接给予物品（返回剩余数量)
        /// </summary>
        /// <param name="nId">物品ID</param>
        /// <param name="nCount">物品数量</param>
        /// <param name="character"></param>
        /// <param name="createItemType"></param>
        /// <returns></returns>
        public int ForceAddItem(int nId, int nCount, CharacterController character, eCreateItemType createItemType)
        {
            return mImpl.ForceAddItem(this, nId, nCount, character, createItemType);
        }

        public int ForceAddItemByDb(ItemBaseData itemDb, CharacterController character, eCreateItemType createItemType)
        {
            return mImpl.ForceAddItemByDb(this, itemDb, character, createItemType);
        }

        //直接给予物品（返回添加物品的实例)
        public ItemBase ForceAddItem_GetItem(int nId,
                                             int nCount,
                                             CharacterController character,
                                             eCreateItemType createItemType)
        {
            return mImpl.ForceAddItem_GetItem(this, nId, nCount, character, createItemType);
        }

        #endregion

        #region 删除相关

        //清除一个索引的物品
        /// <summary>
        ///     清除一个索引的物品
        /// </summary>
        /// <param name="nIndex">包裹索引</param>
        public void CleanItemByIndex(int nIndex)
        {
            mImpl.CleanItemByIndex(this, nIndex);
        }

        //减少某个索引的物品数量
        /// <summary>
        ///     减少某个索引的物品数量
        /// </summary>
        /// <param name="nIndex">包裹索引</param>
        /// <param name="nCount">减少的数量</param>
        /// <param name="deleteItemType"></param>
        public void ReduceCountByIndex(int nIndex, int nCount, eDeleteItemType deleteItemType)
        {
            mImpl.ReduceCountByIndex(this, nIndex, nCount, deleteItemType);
        }

        //强制删除某道具
        /// <summary>
        ///     强制删除某道具
        /// </summary>
        /// <param name="nId">物品ID</param>
        /// <param name="nCount">物品数量</param>
        /// <returns></returns>
        public int ForceDeleteItem(int nId, int nCount)
        {
            return mImpl.ForceDeleteItem(this, nId, nCount);
        }

        #endregion

        #region 修改相关

        //给某个索引设置道具
        /// <summary>
        ///     给某个索引设置道具
        /// </summary>
        /// <param name="nIndex"></param>
        /// <param name="nItemId"></param>
        public void ResetItemByItemId(int nIndex, int nItemId)
        {
            mImpl.ResetItemByItemId(this, nIndex, nItemId);
        }

        //给某个索引设置某个道具的镜像(不改变Item的任何数据)
        public void ResetItemByItem(int nIndex, ItemBase item, int nCount)
        {
            mImpl.ResetItemByItem(this, nIndex, item, nCount);
        }

        //设置某个索引的物品数量
        /// <summary>
        ///     设置某个索引的物品数量
        /// </summary>
        /// <param name="nIndex">包裹索引</param>
        /// <param name="nCount">新数量</param>
        public void SetCountByIndex(int nIndex, int nCount)
        {
            mImpl.SetCountByIndex(this, nIndex, nCount);
        }

        #endregion

        #region 查询相关

        /// 检查某个索引是否有物品
        /// <summary>
        ///     检查某个索引是否有物品
        /// </summary>
        /// <param name="nIndex">包裹索引</param>
        /// <returns></returns>
        public ItemBase GetItemByIndex(int nIndex)
        {
            return mImpl.GetItemByIndex(this, nIndex);
        }


        //检查物品ID是否非法，检查物品是否能放在该包裹
        public ErrorCodes CheckItem(int nId)
        {
            return mImpl.CheckItem(this, nId);
        }

        /// 检查物品ID是否存在
        /// <summary>
        ///     检查物品ID是否存在
        /// </summary>
        /// <param name="nId">物品ID</param>
        /// <returns></returns>
        public ErrorCodes CheckItemId(int nId)
        {
            return mImpl.CheckItemId(this, nId);
        }

        /////检查物品Id是否能放在该包裹
        ///// <summary>
        ///// 检查物品Id是否能放在该包裹
        ///// </summary>
        ///// <param name="nId">物品ID</param>
        ///// <returns></returns>
        //private ErrorCodes CheckItemCanInBag(int nId)
        //{
        //    return mImpl.CheckItemCanInBag(this, nId);
        //}

        /// 检查添加物品
        /// <summary>
        ///     检查添加物品
        /// </summary>
        /// <param name="nId">物品ID</param>
        /// <param name="nCount">物品数量</param>
        /// <returns></returns>
        public ErrorCodes CheckAddItem(int nId, int nCount)
        {
            return mImpl.CheckAddItem(this, nId, nCount);
        }

        /// 获取物品数量
        /// <summary>
        ///     获取物品数量
        /// </summary>
        /// <param name="nId">物品ID</param>
        /// <returns></returns>
        public int GetItemCount(int nId)
        {
            return mImpl.GetItemCount(this, nId);
        }

        //获得当前所有非空格数
        public int GetNoFreeCount()
        {
            return mImpl.GetNoFreeCount(this);
        }

        //获得当前的所有空格数
        public int GetFreeCount()
        {
            return mImpl.GetFreeCount(this);
        }

        #endregion

        #region 私有方法

        //强制增加可堆叠物品
        /// <summary>
        ///     强制增加可堆叠物品
        /// </summary>
        /// <param name="nId">物品ID</param>
        /// <param name="nCount">物品数量</param>
        /// <returns></returns>
        public int ForceAddMultiItem(int nId, int nCount)
        {
            return mImpl.ForceAddMultiItem(this, nId, nCount);
        }

        //强制增加新物品
        /// <summary>
        ///     强制增加新物品
        /// </summary>
        /// <param name="nId">物品ID</param>
        /// <param name="nCount">物品数量</param>
        /// <returns></returns>
        public int ForceAddNewItem(int nId, int nCount)
        {
            return mImpl.ForceAddNewItem(this, nId, nCount);
        }

        public ItemBase ForceAddNewItem_GetItem(int nId, ref int nLast)
        {
            return mImpl.ForceAddNewItem_GetItem(this, nId, ref nLast);
        }

        public int ForceAddNewItem(ItemBaseData itemDb)
        {
            return mImpl.ForceAddNewItem(this, itemDb);
        }

        #endregion

        #region 包裹的扩展方法

        //去掉本次在线需要忽视的时间
        public int RemoveBuyTimes;

        //设置下次的时间
        public void SetNextTime(int seconds)
        {
            if (mCharacter != null)
            {
                var nowSecond = (int) DateTime.Now.GetDiffSeconds(mCharacter.OnlineTime);
                RemoveBuyTimes = nowSecond;
            }
            mDbData.NextSecond = seconds;
        }

        //获取下次的时间
        public int GetNextTime()
        {
            return mDbData.NextSecond;
        }

        //计算下次的时间
        public int GetNeedTime()
        {
            return mImpl.GetNeedTime(this);
        }

        //获取某个格子的等待时间
        public int GetNeedTime(int index, BagBaseRecord tbBag)
        {
            return mImpl.GetNeedTime(this, index, tbBag);
        }

        //获取某个格子所需钥匙数
        public int GetNeedKey(int index, BagBaseRecord tbBag)
        {
            return mImpl.GetNeedKey(this, index, tbBag);
        }

        #endregion
    }
}