#region using

using DataContract;

#endregion

namespace Shared
{
    internal class Astrology : ItemBase
    {
        /*很重要的一个东西就是宠物的附加属性条数
         * 0、等级
         * 1、经验
         * 2、
         * 3、
         */
        //private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public Astrology()
        {
        }

        public Astrology(int nId, ItemBaseData Dbdata)
        {
            Init(nId, Dbdata);
        }

        public Astrology(ItemBaseData Dbdata, bool IsNull = true)
        {
            mDbData = Dbdata;
            if (IsNull)
            {
                SetId(-1);
                SetCount(0);
            }
        }

        //初始化数据
        private void Init(int nId, ItemBaseData Dbdata)
        {
            mDbData = Dbdata;
            SetId(nId);
            SetCount(1);
            CleanExdata();
            //初始等级
            AddExdata(1);
            //初始经验
            AddExdata(0);
        }
    }
}