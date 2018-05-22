#region using

using System.Collections.Generic;
using DataContract;
using DataTable;

#endregion

namespace Shared
{
    public class MedalItem : ItemBase
    {
        /*很重要的一个东西就是宠物的附加属性条数
         * 0、等级
         * 1、经验
         * 2、
         * 3、
         * 4、
         * 5、
         * 6、
         * 7、 
         * 8、 
         * 9、 
         * 10、
         * 11、
         * 12、
         * 13、
         * 14、
         */
        //private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public MedalItem()
        {
        }

        public MedalItem(int nId, ItemBaseData Dbdata)
        {
            Init(nId, Dbdata);
        }

        public MedalItem(ItemBaseData Dbdata, bool IsNull = true)
        {
            mDbData = Dbdata;
            if (IsNull)
            {
                SetId(-1);
                SetCount(0);
            }
        }

        //获得经验
        public void AddExp(int value)
        {
            var tbMedal = Table.GetMedal(GetId());
            if (tbMedal == null)
            {
                return;
            }
            var exp = GetExdata(1) + value;
            var tbskillupgrading = Table.GetSkillUpgrading(tbMedal.LevelUpExp);
            var needexp = GetNeedExp(tbskillupgrading);
            //升级计算
            var oldLevel = GetExdata(0);
            var levelup = 0;
            while (exp >= needexp)
            {
                levelup++;
                exp -= needexp;
                if (tbMedal.MaxLevel <= oldLevel + levelup)
                {
                    break;
                }
                needexp = GetNeedExp(tbskillupgrading, oldLevel + levelup);
            }
            if (levelup > 0)
            {
                SetExdata(0, oldLevel + levelup);
            }
            SetExdata(1, exp);
            SetExdata(2,needexp);
            MarkDirty();
        }

        //属性计算
        public static void GetAttrList(Dictionary<int, int> AttrList,
                                       ItemBase wing,
                                       WingQualityRecord tbWing,
                                       int characterLevel)
        {
        }

        //获得经验
        public int GetExp()
        {
            return GetExdata(1);
        }

        //获取当前可提供经验
        public int GetGiveExp()
        {
            var tbMedal = Table.GetMedal(GetId());
            if (tbMedal == null)
            {
                return 0;
            }
            var level = GetExdata(0);
            var giveExp = GetExp() + tbMedal.InitExp;
            if (level > 1)
            {
                var tbskillupgrading = Table.GetSkillUpgrading(tbMedal.LevelUpExp);
                for (var i = 1; i < level; ++i)
                {
                    giveExp += tbskillupgrading.GetSkillUpgradingValue(i);
                }
            }
            return giveExp;
        }

        //获取升级需求经验
        public int GetNeedExp(SkillUpgradingRecord tbskillupgrading, int level = -1)
        {
            if (level != -1)
            {
                return tbskillupgrading.GetSkillUpgradingValue(level);
            }
            return tbskillupgrading.GetSkillUpgradingValue(GetExdata(0));
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
            var tbMedal = Table.GetMedal(nId);
            if (tbMedal != null)
            {
                var exp = Table.GetSkillUpgrading(tbMedal.LevelUpExp).GetSkillUpgradingValue(1);
                AddExdata(exp);                
            }
            else
            {
                AddExdata(0);                
            }
        }

        //获得经验
        public void SetExp(int value)
        {
            SetExdata(1, value);
        }
    }
}