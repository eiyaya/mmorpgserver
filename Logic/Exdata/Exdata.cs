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
    public class Exdata : NodeBase
    {
        public CharacterController mCharacter;
        public List<int> mData;
        public Dictionary<int, int> mNetDirtyList = new Dictionary<int, int>();

        public override IEnumerable<NodeBase> Children
        {
            get { return null; }
        }

        public override void NetDirtyHandle()
        {
            var count = mNetDirtyList.Count;
            if (count < 1)
            {
                return;
            }
            //if (mCharacter.Proxy == null) return;
            if (count == 1)
            {
                foreach (var i in mNetDirtyList)
                {
                    mCharacter.Proxy.SyncExdata(i.Key, i.Value);
                }
            }
            else
            {
                var diffData = new Dict_int_int_Data();
                diffData.Data.AddRange(mNetDirtyList);
                mCharacter.Proxy.SyncExdataList(diffData);
            }
            mNetDirtyList.Clear();
        }

        //每次修改值
        public void PerChange(int nIndex)
        {
            var tbExdata = Table.GetExdata(nIndex);
            if (tbExdata == null)
            {
                return;
            }
            mCharacter.SetExData(nIndex, mCharacter.GetExData(nIndex) + tbExdata.Change);
        }

        //重置扩展数据
        public void Reset(int nIndex)
        {
            var tbExdata = Table.GetExdata(nIndex);
            if (tbExdata == null)
            {
                return;
            }

            var oldValue = mCharacter.GetExData(nIndex);
            if (oldValue > tbExdata.RefreshValue[1] && tbExdata.IsRefresh == 0)
            {
                return;
            }

            mCharacter.SetExData(nIndex, MyRandom.Random(tbExdata.RefreshValue[0], tbExdata.RefreshValue[1]));
        }

        //根据事件重置标记位
        public void ResetByTime(int type, int hour)
        {
            Logger.Info("ResetByTime type={0} hour={1}", type, hour);
            switch (type)
            {
                case 0:
                {
                    var a = DateTime.Now.Ticks;
                    List<int> tempList;
                    if (DaysHour.TryGetValue(hour, out tempList))
                    {
                        foreach (var i in tempList)
                        {
                            Reset(i);
                        }
                    }
                    var b = DateTime.Now.Ticks;
                    var c = b - a;
                }
                    break;
                case 1:
                {
                    List<int> tempList;
                    if (WeekHour.TryGetValue(hour, out tempList))
                    {
                        foreach (var i in tempList)
                        {
                            Reset(i);
                        }
                    }
                }
                    break;
                case 2:
                {
                    List<int> tempList;
                    if (MonthHour.TryGetValue(hour, out tempList))
                    {
                        foreach (var i in tempList)
                        {
                            Reset(i);
                        }
                    }
                }
                    break;
            }
        }

        #region   静态数据

        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 2017.09.18 跟策划张竞统一过，Exdata表id超过1023，程序标记的上限id数值不大于1024(id从0开始，所以id数值不能大于1024)
        /// </summary>
        public static int ExdataCount = 1024;

        public static Dictionary<int, List<int>> DaysHour = new Dictionary<int, List<int>>();
            //每天刷新，key=小时，value=需要刷新的索引

        public static Dictionary<int, List<int>> WeekHour = new Dictionary<int, List<int>>();
            //每周刷新，key=小时，value=需要刷新的索引

        public static Dictionary<int, List<int>> MonthHour = new Dictionary<int, List<int>>();
            //每月刷新，key=小时，value=需要刷新的索引

        //静态数据初始化
        public static void Init()
        {
            Table.ForeachExdata(record =>
            {
                switch (record.RefreshRule)
                {
                    case 1:
                    {
                        if (record.RefreshTime < 0 || record.RefreshTime > 23)
                        {
                            Logger.Error("Exdata[{0}] refresh Error!", record.Id);
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
                            Logger.Error("Exdata[{0}] refresh Error!", record.Id);
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
                            Logger.Error("Exdata[{0}] refresh Error!", record.Id);
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
            mCharacter = character;
            mData = dbplayer.ExData;
            for (var i = 0; i != ExdataCount; ++i)
            {
                var tbExdata = Table.GetExdata(i);
                if (tbExdata != null)
                {
                    mData.Add(tbExdata.InitValue);
                    continue;
                }
                mData.Add(0);
            }


            MarkDirty();
        }

        //初始化（按数据库配置）
        public void InitByDB(CharacterController character, DBCharacterLogic dbplayer)
        {
            mCharacter = character;
            mData = dbplayer.ExData;
            if (mData.Count > ExdataCount)
            {
                Logger.Error("Exdata InitByDB too Long={0}", mData.Count);
            }
            else if (mData.Count < ExdataCount)
            {
                for (var i = mData.Count; i < ExdataCount; ++i)
                {
                    var tbExdata = Table.GetExdata(i);
                    if (tbExdata != null)
                    {
                        mData.Add(tbExdata.InitValue);
                        continue;
                    }
                    mData.Add(0);
                }
            }
        }

        #endregion
    }
}