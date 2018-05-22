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
    public class Exdata64 : NodeBase
    {
        public CharacterController mCharacter;
        public List<long> mData;

        public override IEnumerable<NodeBase> Children
        {
            get { return null; }
        }

        public override void NetDirtyHandle()
        {
        }

        #region   静态数据

        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static int ExdataCount = 20;
        //静态数据初始化
        public static void Init()
        {
        }

        #endregion

        #region   初始化

        //初始化（按初始配置）
        public void InitByBase(CharacterController character, DBCharacterLogic dbplayer)
        {
            mCharacter = character;
            mData = dbplayer.ExData64;
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
            SetTime(Exdata64TimeType.CreateTime, DateTime.Now);

            MarkDirty();
        }

        //初始化（按数据库配置）
        public void InitByDB(CharacterController character, DBCharacterLogic dbplayer)
        {
            mCharacter = character;
            mData = dbplayer.ExData64;
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

        #region   执行方法

        public List<long> GetExData()
        {
            return mData;
        }

        public long GetExData(int index)
        {
            if (0 > index || mData.Count <= index)
            {
                Logger.Warn("GetExData64 Out Id={0}", index);
                return 0;
            }

            return mData[index];
        }

        //设置扩展计数
        public void SetExData(int index, long value, bool forceNotToClient = false)
        {
            if (0 > index || mData.Count <= index)
            {
                return;
            }
            if (value == mData[index])
            {
                return;
            }
            //if (value > lExdata.mData[index])
            //{
            //    CharacterExdataAddEvent eAdd = new CharacterExdataAddEvent(this, index, value - lExdata.mData[index]);
            //    EventDispatcher.Instance.DispatchEvent(eAdd);
            //}
            mData[index] = value;
            MarkDbDirty();
            //lExdata.MarkDirty();
            //触发事件
            //CharacterExdataChange e = new CharacterExdataChange(this, index, value);
            //EventDispatcher.Instance.DispatchEvent(e);
            if (mCharacter.Proxy != null)
            {
                mCharacter.Proxy.SyncExdata64(index, value);
            }
        }

        //增加扩展计数
        public void AddExData(int index, long value)
        {
            if (0 > index || mData.Count <= index)
            {
                Logger.Warn("AddExData64 Out Id={0} addValue={1}", index, value);
                return;
            }
            var newValue = GetExData(index) + value;
            SetExData(index, newValue);
        }

        #endregion

        #region   时间方法

        public DateTime GetTime(Exdata64TimeType index)
        {
            return DateTime.FromBinary(GetExData((int) index));
        }

        public void SetTime(Exdata64TimeType index, DateTime setTime)
        {
            SetExData((int) index, setTime.ToBinary());
        }

        #endregion
    }
}