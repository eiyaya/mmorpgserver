#region using

using DataContract;
using DataTable;

#endregion

namespace Shared
{
    public class TreasureMap : ItemBase
    {
        /*藏宝图目的地的位置
         * 0、场景id
         * 1、坐标x
         * 2、坐标y
         * 3、
         * 4、
         * 5、
         * 6、
         * 7、
         * 8、
         * 9、
         */

        public TreasureMap()
        {
        }

        public TreasureMap(int nId, ItemBaseData Dbdata)
        {
            Init(nId, Dbdata);
        }

        public TreasureMap(ItemBaseData Dbdata, bool IsNull = true)
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

            var tbItem = Table.GetItemBase(nId);
            var tbSU = Table.GetSkillUpgrading(tbItem.Exdata[0]);
            var values = tbSU.Values;
            if (values.Count%3 != 0)
            {
                Logger.Error("tbSU.Values.Count can't be divided by 3!!");
                return;
            }
            var count = values.Count/3;
            var idx = MyRandom.Random(count)*3;
            var sceneId = values[idx++];
            var x = values[idx++];
            var y = values[idx++];
            //初始exdata
            AddExdata(sceneId);
            AddExdata(x);
            AddExdata(y);
        }
    }
}