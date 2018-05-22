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
    public interface IBookManager
    {
        ErrorCodes ActivateBook(BookManager _this, int itemId);
        void RefreshAttr(BookManager _this);
        void StaticInit();
        bool IsFullStar(BookManager _this, int itemId);
    }

    public class BookManagerDefaultImpl : IBookManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region  方法

        public void StaticInit()
        {
            Table.ForeachHandBook(record =>
            {
                BookManager.PieceBookId[record.PieceId] = record.Id;
                return true;
            });
        }

        //刷新属性
        public void RefreshAttr(BookManager _this)
        {
            _this.attrs.Clear();
            _this.MonsterAttrs.Clear();
            //图鉴收集
            foreach (var book in _this.Books)
            {
                var tbbook = Table.GetHandBook(book.Key);
                if (tbbook == null)
                {
                    Logger.Fatal("null==Table.GetHandBook({0})", book.Key);
                    continue;
                }
                //----------人物属性
//                ItemEquip2.PushEquipAttr(_this.attrs, tbbook.AttrId, tbbook.AttrValue, _this.mCharacter.GetLevel(), _this.mCharacter.GetAttackType());
                //----------魔物属性
                int attrId = book.Key*100 + book.Value;
                var tbBookBase = Table.GetBookBase(attrId);
                if(tbBookBase != null)
                {
                    for (int i = 0; i < tbBookBase.AttrList.Length; i++)
                    {
                        _this.attrs.modifyValue(i + 1, tbBookBase.AttrList[i]*tbBookBase.AddAttr/10000);
                        _this.MonsterAttrs.modifyValue(i + 1, tbBookBase.AttrList[i]);
                    }
                }

            }
            //组收集
            var groupOkCount = 0;
            foreach (var i in _this.Group)
            {
                var tbGroupBook = Table.GetBookGroup(i);
                if (tbGroupBook == null)
                {
                    Logger.Fatal("null==Table.GetBookGroup({0})", i);
                    continue;
                }   
                groupOkCount++;
                for (var j = 0; j != tbGroupBook.GroupAttrId.Length; j++)
                {
                    if (tbGroupBook.GroupAttrId[j] > 0)
                    {
                        ItemEquip2.PushEquipAttr(_this.attrs, tbGroupBook.GroupAttrId[j],
                            tbGroupBook.GroupAttrValue[j], _this.mCharacter.GetLevel(),
                            _this.mCharacter.GetAttackType());
                    }
                }
            }
            var nowMaxCount = _this.mCharacter.GetExData((int)eExdataDefine.e347);
            if (groupOkCount > nowMaxCount)
            {
                _this.mCharacter.SetExData((int)eExdataDefine.e347, groupOkCount);
            }
        }

        //激活图鉴
        public ErrorCodes ActivateBook(BookManager _this, int itemId)
        {
            var tbbook = Table.GetHandBook(itemId);
            if (tbbook == null)
            {
                return ErrorCodes.Error_BookID;
            }
            int lv = 1;
            if (_this.Books.ContainsKey(itemId))
            {
                lv = _this.Books[itemId] + 1;
            }
            if (lv > tbbook.ListCost.Count)
            {
                return ErrorCodes.CharacterLevelMax;
            }
            int cost = tbbook.ListCost[lv-1];
            if (_this.mCharacter.mBag.GetItemCount(tbbook.PieceId) < cost)
            {
                return ErrorCodes.ItemNotEnough;
            }
            var color = Table.GetItemBase(tbbook.PieceId);
            if (lv == 1)
            {    
                if (color.Quality >= 3)
                {
                    var args = new List<string>
                    {
                        Utils.AddCharacter(_this.mCharacter.mGuid,_this.mCharacter.GetName()),
                        //Utils.GetTableColorString(color.Quality),
                         string.Format("[{0}]{1}[-]",Utils.GetTableColorString(color.Quality),tbbook.Name)  
                        //tbbook.Name
                    };
                    var exExdata = new List<int>();
                    _this.mCharacter.SendSystemNoticeInfo(291006, args, exExdata);                        
                }
               

                _this.Books.Add(itemId, lv);
            }
            else
            {
                _this.Books[itemId] = lv;
                if (lv >= tbbook.MaxLevel)
                {
                    var args = new List<string>
                {
                    Utils.AddCharacter(_this.mCharacter.mGuid,_this.mCharacter.GetName()),
                    string.Format("[{0}]{1}[-]",Utils.GetTableColorString(color.Quality),tbbook.Name),  
                };
                    var exExdata = new List<int>();
                    _this.mCharacter.SendSystemNoticeInfo(291005, args, exExdata);
                }
            }
   
            _this.mCharacter.AddExData((int) eExdataDefine.e88, 1);
            _this.mCharacter.mBag.DeleteItem(tbbook.PieceId, cost, eDeleteItemType.ActivateBook);
            _this.mCharacter.mBag.AddRes(eResourcesType.GoldRes, tbbook.Money, eCreateItemType.ActivateBook);

            try
            {
                var klog = string.Format("mowuzhi#{0}|{1}|{2}|{3}|{4}|{5}",
                    _this.mCharacter.mGuid,
                    _this.mCharacter.GetLevel(),
                    _this.mCharacter.serverId,
                    tbbook.Id,
                    lv,
                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                PlayerLog.Kafka(klog, 2);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            //else
            //{
            //    var tbGroup = Table.GetBookGroup(groupId);
            //    if (tbGroup == null)
            //    {
            //        return ErrorCodes.Error_BookGroupID;
            //    }
            //    if (index < 0 || index > 5)
            //    {
            //        Logger.Error("ActivateBook index={0}", index);
            //        return ErrorCodes.Error_DataOverflow;
            //    }
            //    if (tbGroup.ItemId[index] != itemId)
            //    {
            //        Logger.Error("ActivateBook index={0} itemid={1}", index, itemId);
            //        return ErrorCodes.Error_BookNotSame;
            //    }
            //    int Value;
            //    if (_this.Group.TryGetValue(groupId, out Value))
            //    {
            //        if (BitFlag.GetLow(Value, index))
            //        {
            //            return ErrorCodes.Error_BookActivated;
            //        }
            //        _this.mCharacter.mBag.DeleteItem(itemId, 1, eDeleteItemType.ActivateBook);
            //        _this.Group[groupId] = BitFlag.IntSetFlag(Value, index);
            //    }
            //    else
            //    {
            //        _this.mCharacter.mBag.DeleteItem(itemId, 1, eDeleteItemType.ActivateBook);
            //        _this.Group[groupId] = BitFlag.IntSetFlag(0, index);
            //    }
            //}
            CheckActiveGroup(_this, itemId, lv);
            RefreshAttr(_this);
            _this.mCharacter.BooksChange();
            _this.MarkDirty();
            return ErrorCodes.OK;
        }

        private void CheckActiveGroup(BookManager _this, int bookId,int level)
        {
            Table.ForeachBookGroup(group =>
            {
                if (group.Level > level)
                {
                    return true;
                }
                if (_this.Group.Contains(group.Id) == true)
                {
                    return true;
                }
                bool bHas = false;
                for (int i = 0; i < group.ItemId.Length; i++)
                {
                    if (group.ItemId[i] == bookId)
                    {
                        bHas = true;
                        break;
                    }
                }
                if (bHas == true)
                {
                    for (int i = 0; i < group.ItemId.Length; i++)
                    {
                        if (group.ItemId[i] <= 0)
                            continue;
                        int lv = 0;
                        if(false == _this.Books.TryGetValue(group.ItemId[i], out lv) || lv < group.Level)
                        {
                            return true;
                        }
                    }
                    {//激活并通知
                        _this.Group.Add(group.Id);
                        _this.mCharacter.Proxy.ActivateBookGroup(group.Id);
                    }
                }
                return true; 
            });
        }

        public bool IsFullStar(BookManager _this, int itemId)
        {
            if (_this.Books == null)
                return false;

            int lvl;
            if (_this.Books.TryGetValue(itemId, out lvl))
            {
                var tbBook = Table.GetHandBook(itemId);
                if (tbBook != null)
                {
                    return lvl >= tbBook.MaxLevel;
                }
            }
            return false;
        }
        #endregion
    }


    //图鉴
    public class BookManager : NodeBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static IBookManager mStaticImpl;

        static BookManager()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (BookManager), typeof (BookManagerDefaultImpl),
                o => { mStaticImpl = (IBookManager) o; });
        }

        public static void StaticInit()
        {
            mStaticImpl.StaticInit();
        }

        //数据结构
        public Dictionary<int, int> attrs = new Dictionary<int, int>(); //图鉴所提供的属性列表
        public Dictionary<int, int> MonsterAttrs = new Dictionary<int, int>(); //召唤魔物依赖的属性列表
        public static Dictionary<int, int> PieceBookId = new Dictionary<int, int>(); // 碎片对应的魔物ID

        public CharacterController mCharacter; //角色
        public DBBook mDbData;

        public Dictionary<int, int> Books
        {
            get { return mDbData.BooksDic; }
        } //收集数量

        public int Fight
        {
            get { return mDbData.Fight; }
            set { mDbData.Fight = value; }
        }

        public override IEnumerable<NodeBase> Children
        {
            get { return null; }
        }

        public List<int> Group
        {
            get { return mDbData.GroupList; }
        } //图鉴组
        //初始化

        #region  初始化

        //用第一次创建
        public DBBook InitByBase(CharacterController character)
        {
            mDbData = new DBBook();
            mCharacter = character;
            MarkDirty();
            return mDbData;
        }

        //用数据库数据
        public void InitByDB(CharacterController character, DBBook TalentData)
        {
            mCharacter = character;
            mDbData = TalentData;
            RefreshAttr();
        }

        #endregion

        #region  方法

        //刷新属性
        public void RefreshAttr()
        {
            mStaticImpl.RefreshAttr(this);
        }

        //激活图鉴
        public ErrorCodes ActivateBook(int itemId)
        {
            return mStaticImpl.ActivateBook(this, itemId);
        }

        public bool IsFullStar(int itemId)
        {
            return mStaticImpl.IsFullStar(this, itemId);
        }

        #endregion
    }
}