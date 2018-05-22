#region using

using System.Collections.Generic;
using DataContract;
using DataTable;
using NLog;
using Shared;

#endregion

namespace Logic
{
    public interface INodeFlag
    {
        void InitByBase(NodeFlag _this, CharacterController character, DBCharacterLogic dbplayer);
        void InitByDB(NodeFlag _this, CharacterController character, DBCharacterLogic dbplayer);
        void NetDirtyHandle(NodeFlag _this);
        void Reset(NodeFlag _this, int nIndex);
        void ResetByTime(NodeFlag _this, int type, int hour);
        void SetFlag(NodeFlag _this, int index, bool b = true, bool forceNotToClient = false);
    }

    public class NodeFlagDefaultImpl : INodeFlag
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private void AddNetDirty(NodeFlag _this, int index, bool b)
        {
            _this.mNetDirtyList[index] = b;
        }

        #region   初始化

        //静态数据初始化
        public static void Init()
        {
            Table.ForeachFlag(record =>
            {
                switch (record.RefreshType)
                {
                    case 1:
                    {
                        if (record.RefreshTime < 0 || record.RefreshTime > 23)
                        {
                            Logger.Error("Flag[{0}] refresh Error!", record.Id);
                            return true;
                        }
                        List<int> temp;
                        if (!NodeFlag.DaysHour.TryGetValue(record.RefreshTime, out temp))
                        {
                            temp = new List<int>();
                            NodeFlag.DaysHour[record.RefreshTime] = temp;
                        }
                        temp.Add(record.Id);
                        return true;
                    }
                    case 2:
                    {
                        if (record.RefreshTime < 0 || record.RefreshTime > 23)
                        {
                            Logger.Error("Flag[{0}] refresh Error!", record.Id);
                            return true;
                        }
                        List<int> temp;
                        if (!NodeFlag.WeekHour.TryGetValue(record.RefreshTime, out temp))
                        {
                            temp = new List<int>();
                            NodeFlag.WeekHour[record.RefreshTime] = temp;
                        }
                        temp.Add(record.Id);
                        return true;
                    }
                    case 3:
                    {
                        if (record.RefreshTime < 0 || record.RefreshTime > 23)
                        {
                            Logger.Error("Flag[{0}] refresh Error!", record.Id);
                            return true;
                        }
                        List<int> temp;
                        if (!NodeFlag.MonthHour.TryGetValue(record.RefreshTime, out temp))
                        {
                            temp = new List<int>();
                            NodeFlag.MonthHour[record.RefreshTime] = temp;
                        }
                        temp.Add(record.Id);
                        return true;
                    }
                }
                return true;
            });
        }

        #endregion

        #region   基础方法

        public void SetFlag(NodeFlag _this, int index, bool b = true, bool forceNotToClient = false)
        {
            if (b)
            {
                _this.mData.SetFlag(index);
                if (index == 30)
                {
                    var bag = _this.mCharacter.mBag.GetBag((int) eBagType.Depot);
                    bag.ForceAddItem(22000, 2, _this.mCharacter, eCreateItemType.HiddenRules);
                }
            }
            else
            {
                _this.mData.CleanFlag(index);
            }
            if (forceNotToClient)
            {
                _this.MarkDbDirty();
            }
            else
            {
                _this.MarkDirty();
                AddNetDirty(_this, index, b);
            }
        }

        #endregion

        #region   初始化

        //初始化（按初始配置）
        public void InitByBase(NodeFlag _this, CharacterController character, DBCharacterLogic dbplayer)
        {
            _this.mCharacter = character;
            _this.mData = new BitFlag(NodeFlag.FlagCount, dbplayer.Flag);
            _this.MarkDirty();
        }

        //初始化（按数据库配置）
        public void InitByDB(NodeFlag _this, CharacterController character, DBCharacterLogic dbplayer)
        {
            _this.mCharacter = character;
            _this.mData = new BitFlag(NodeFlag.FlagCount, dbplayer.Flag);
        }

        #endregion

        #region 节点方法

        public void NetDirtyHandle(NodeFlag _this)
        {
            var count = _this.mNetDirtyList.Count;
            if (count < 1)
            {
                return;
            }
            //if(mCharacter.Proxy==null) return;
            if (count == 1)
            {
                foreach (var i in _this.mNetDirtyList)
                {
                    _this.mCharacter.Proxy.SyncFlag(i.Key, i.Value ? 1 : 0);
                }
            }
            else
            {
                var trueList = new Int32Array();
                var falseList = new Int32Array();
                foreach (var i in _this.mNetDirtyList)
                {
                    if (i.Value)
                    {
                        trueList.Items.Add(i.Key);
                    }
                    else
                    {
                        falseList.Items.Add(i.Key);
                    }
                }
                _this.mCharacter.Proxy.SyncFlagList(trueList, falseList);
            }
            _this.mNetDirtyList.Clear();
        }

        //重置标记位
        public void Reset(NodeFlag _this, int nIndex)
        {
            _this.mCharacter.SetFlag(nIndex, false);
        }

        //根据事件重置标记位
        public void ResetByTime(NodeFlag _this, int type, int hour)
        {
            switch (type)
            {
                case 0:
                {
                    List<int> tempList;
                    if (NodeFlag.DaysHour.TryGetValue(hour, out tempList))
                    {
                        foreach (var i in tempList)
                        {
                            Reset(_this, i);
                        }
                    }
                }
                    break;
                case 1:
                {
                    List<int> tempList;
                    if (NodeFlag.WeekHour.TryGetValue(hour, out tempList))
                    {
                        foreach (var i in tempList)
                        {
                            Reset(_this, i);
                        }
                    }
                }
                    break;
                case 2:
                {
                    List<int> tempList;
                    if (NodeFlag.MonthHour.TryGetValue(hour, out tempList))
                    {
                        foreach (var i in tempList)
                        {
                            Reset(_this, i);
                        }
                    }
                }
                    break;
            }
        }

        #endregion
    }

    public class NodeFlag : NodeBase
    {
        private static INodeFlag mImpl;

        static NodeFlag()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (NodeFlag), typeof (NodeFlagDefaultImpl),
                o => { mImpl = (INodeFlag) o; });
        }

        public CharacterController mCharacter;
        public BitFlag mData;
        public Dictionary<int, bool> mNetDirtyList = new Dictionary<int, bool>();

        #region   基础方法

        public void SetFlag(int index, bool b = true, bool forceNotToClient = false)
        {
            mImpl.SetFlag(this, index, b, forceNotToClient);
        }

        #endregion

        #region   静态数据

        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static int FlagCount = Table.GetServerConfig(217).ToInt();

        public static Dictionary<int, List<int>> DaysHour = new Dictionary<int, List<int>>();
            //每天刷新，key=小时，value=需要刷新的索引

        public static Dictionary<int, List<int>> WeekHour = new Dictionary<int, List<int>>();
            //每周刷新，key=小时，value=需要刷新的索引

        public static Dictionary<int, List<int>> MonthHour = new Dictionary<int, List<int>>();
            //每月刷新，key=小时，value=需要刷新的索引

        //静态数据初始化
        public static void Init()
        {
            Table.ForeachFlag(record =>
            {
                switch (record.RefreshType)
                {
                    case 1:
                    {
                        if (record.RefreshTime < 0 || record.RefreshTime > 23)
                        {
                            Logger.Error("Flag[{0}] refresh Error!", record.Id);
                            return true;
                        }
                        List<int> temp;
                        if (!DaysHour.TryGetValue(record.RefreshTime, out temp))
                        {
                            temp = new List<int>();
                            DaysHour[record.RefreshTime] = temp;
                        }
                        temp.Add(record.Id);
                        return true;
                    }
                    case 2:
                    {
                        if (record.RefreshTime < 0 || record.RefreshTime > 23)
                        {
                            Logger.Error("Flag[{0}] refresh Error!", record.Id);
                            return true;
                        }
                        List<int> temp;
                        if (!WeekHour.TryGetValue(record.RefreshTime, out temp))
                        {
                            temp = new List<int>();
                            WeekHour[record.RefreshTime] = temp;
                        }
                        temp.Add(record.Id);
                        return true;
                    }
                    case 3:
                    {
                        if (record.RefreshTime < 0 || record.RefreshTime > 23)
                        {
                            Logger.Error("Flag[{0}] refresh Error!", record.Id);
                            return true;
                        }
                        List<int> temp;
                        if (!MonthHour.TryGetValue(record.RefreshTime, out temp))
                        {
                            temp = new List<int>();
                            MonthHour[record.RefreshTime] = temp;
                        }
                        temp.Add(record.Id);
                        return true;
                    }
                }
                return true;
            });
        }

        #endregion

        #region   初始化

        //初始化（按初始配置）
        public void InitByBase(CharacterController character, DBCharacterLogic dbplayer)
        {
            mImpl.InitByBase(this, character, dbplayer);
        }

        //初始化（按数据库配置）
        public void InitByDB(CharacterController character, DBCharacterLogic dbplayer)
        {
            mImpl.InitByDB(this, character, dbplayer);
        }

        #endregion

        #region 节点方法

        public override IEnumerable<NodeBase> Children
        {
            get { return null; }
        }

        public override void NetDirtyHandle()
        {
            mImpl.NetDirtyHandle(this);
        }

        //重置标记位
        public void Reset(int nIndex)
        {
            mImpl.Reset(this, nIndex);
        }

        //根据事件重置标记位
        public void ResetByTime(int type, int hour)
        {
            mImpl.ResetByTime(this, type, hour);
        }

        #endregion
    }
}