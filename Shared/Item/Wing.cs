#region using

using System.Collections.Generic;
using DataContract;
using DataTable;

#endregion

namespace Shared
{
    public class WingItem : ItemBase
    {
        /*很重要的一个东西就是宠物的附加属性条数
         * 0、翅膀祝福值
         * 1、翅翼数据：WingTrain = Id
         * 2、翅翼经验
         * 3、翅鞘数据：WingTrain = Id
         * 4、翅鞘经验
         * 5、翅羽数据：WingTrain = Id 
         * 6、 翅羽经验
         * 7、 翅骨数据：WingTrain = Id
         * 8、 翅骨经验
         * 9、 翅翎数据：WingTrain = Id
         * 10、翅翎经验
         * 11、
         * 12、
         * 13、
         * 14、
         * 15、
         * 16、
         * 17、
         * 18、
         * 19、
         * 20、
         */
        //private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public WingItem()
        {
        }

        public WingItem(int nId, ItemBaseData Dbdata)
        {
            Init(nId, Dbdata);
        }

        public WingItem(ItemBaseData Dbdata, bool IsNull = true)
        {
            mDbData = Dbdata;
            if (IsNull)
            {
                SetId(-1);
                SetCount(0);
            }
        }

        //增加经验
        public void AddExp(int type, int value)
        {
            var newExp = GetExp(type);
            var tbWingt = Table.GetWingTrain(GetExdata(1 + type*2));
            if (tbWingt == null)
            {
                Logger.Error("GetWingTrain not find id={0}", GetExdata(1 + type*2));
                SetExdata(2 + type*2, newExp);
                return;
            }
            if (tbWingt.ExpLimit <= newExp)
            {
                //升级
                newExp -= tbWingt.ExpLimit;
            }
            SetExdata(2 + type*2, newExp);
            MarkDirty();
        }

        private void AttrConvert(Dictionary<int, int> AttrList, int[] attr, int[] attrRef, int roleId)
        {
            foreach (var i in AttrList)
            {
                if (i.Key < (int) eAttributeType.AttrCount)
                {
                    attr[i.Key] = i.Value;
                }
                else
                {
                    switch (i.Key)
                    {
                        case 105:
                        {
                            if (roleId != 1)
                            {
                                attr[(int) eAttributeType.PhyPowerMin] += i.Value;
                                attr[(int) eAttributeType.PhyPowerMax] += i.Value;
                            }
                            else
                            {
                                attr[(int) eAttributeType.MagPowerMin] += i.Value;
                                attr[(int) eAttributeType.MagPowerMax] += i.Value;
                            }
                        }
                            break;
                        case 106:
                        {
                            attrRef[(int) eAttributeType.MagPowerMin] += i.Value*100;
                            attrRef[(int) eAttributeType.MagPowerMax] += i.Value*100;
                            attrRef[(int) eAttributeType.PhyPowerMin] += i.Value*100;
                            attrRef[(int) eAttributeType.PhyPowerMax] += i.Value*100;
                        }
                            break;
                        case 110:
                        {
                            attr[(int) eAttributeType.PhyArmor] += i.Value;
                            attr[(int) eAttributeType.MagArmor] += i.Value;
                        }
                            break;
                        case 111:
                        {
                            attrRef[(int) eAttributeType.PhyArmor] += i.Value*100;
                            attrRef[(int) eAttributeType.MagArmor] += i.Value*100;
                        }
                            break;
                        case 113:
                        {
                            attrRef[(int) eAttributeType.HpMax] += i.Value*100;
                        }
                            break;
                        case 114:
                        {
                            attrRef[(int) eAttributeType.MpMax] += i.Value*100;
                        }
                            break;
                        case 119:
                        {
                            attrRef[(int) eAttributeType.Hit] += i.Value*100;
                        }
                            break;
                        case 120:
                        {
                            attrRef[(int) eAttributeType.Dodge] += i.Value*100;
                        }
                            break;
                    }
                }
            }
        }

        private int GetAttrFightPoint(Dictionary<int, int> fightAttr, int characterLevel, int roleId)
        {
            var talentData = new int[(int) eAttributeType.AttrCount];
            var talentDataRef = new int[(int) eAttributeType.AttrCount];
            AttrConvert(fightAttr, talentData, talentDataRef, roleId);
            var fightPoint = GetFightPoint(talentData, talentDataRef, characterLevel, roleId);
            return fightPoint;
        }

        //属性计算
        public static void GetAttrList(Dictionary<int, int> AttrList,
                                       ItemBase wing,
                                       WingQualityRecord tbWing,
                                       int characterLevel,
                                       int attackType)
        {
            var Quality = tbWing.Segment;
            if (wing.mDbData.Exdata.Count < 11)
            {
                return;
            }
            //基础属性
            for (var i = 0; i != tbWing.AddPropID.Length; ++i)
            {
                var nAttrId = tbWing.AddPropID[i];
                if (nAttrId < 0)
                {
                    break;
                }
                var nValue = tbWing.AddPropValue[i];
                if (nValue > 0 && nAttrId != -1)
                {
                    ItemEquip2.PushEquipAttr(AttrList, nAttrId, nValue, characterLevel, attackType);
                }
            }
            //培养属性
            ////for (var i = 0; i != 5; ++i)
            ////{
            ////    var tbWingTrain = Table.GetWingTrain(wing.GetExdata(1 + i*2));
            ////    if (tbWingTrain == null)
            ////    {
            ////        continue;
            ////    }
            ////    //if (tbWingTrain.Condition > Quality)
            ////    //{
            ////    //    continue;
            ////    //}
            ////    for (var j = 0; j != tbWingTrain.AddPropID.Length; ++j)
            ////    {
            ////        var nAttrId = tbWingTrain.AddPropID[j];
            ////        var nValue = tbWingTrain.AddPropValue[j];
            ////        if (nAttrId < 0 || nValue <= 0)
            ////        {
            ////            break;
            ////        }
            ////        if (nValue > 0 && nAttrId != -1)
            ////        {
            ////            ItemEquip2.PushEquipAttr(AttrList, nAttrId, nValue, characterLevel, attackType);
            ////        }
            ////    }
            ////}
            var tbWingTrain = Table.GetWingTrain(wing.GetExdata(1));
            if (tbWingTrain != null)
            {
                for (var j = 0; j != tbWingTrain.AddPropID.Length; ++j)
                {
                    var nAttrId = tbWingTrain.AddPropID[j];
                    var nValue = tbWingTrain.AddPropValue[j];
                    if (nAttrId < 0 || nValue <= 0)
                    {
                        break;
                    }
                    if (nValue > 0 && nAttrId != -1)
                    {
                        ItemEquip2.PushEquipAttr(AttrList, nAttrId, nValue, characterLevel, attackType);
                    }
                }
            }
            
            // 成长属性
            for (var i = (int)eWingExDefine.eGrowProperty; i < wing.mDbData.Exdata.Count; ++i)
            {
                var attrId = WingItem.GetGrowAttrId(wing.mDbData, i);
                if (attrId > 0)
                {
                    var value = wing.GetExdata(i + 1);
                    if (value > 0)
                    {
                        ItemEquip2.PushEquipAttr(AttrList, attrId, value, characterLevel, attackType);
                    }
                    ++i;
                }
            }
        }

        //获得成长值
        public int GetGrowValue()
        {
            return GetExdata((int)eWingExDefine.eGrowValue);
        }

        //获得经验
        public int GetExp(int type)
        {
            return GetExdata(2 + type*2);
        }

        //获取翅膀战斗力
        public int GetFightPoint(int characterLevel, int attackType)
        {
            var value = 0;
            var attrlist = new Dictionary<int, int>();
            GetAttrList(attrlist, this, Table.GetWingQuality(GetId()), characterLevel, attackType);
            value = GetAttrFightPoint(attrlist, characterLevel, attackType);
            return value;
        }

        private int GetFightPoint(int[] attr, int[] attrRef, int level, int careerId)
        {
            //var level = GetLevel();
            var tbLevel = Table.GetLevelData(level);
            if (tbLevel == null)
            {
                return 0;
            }
            var FightPoint = 0L;
            for (var type = eAttributeType.PhyPowerMin; type != eAttributeType.Count; ++type)
            {
                //基础固定属性
                long nValue = attr[(int) type];
                switch ((int) type)
                {
                    case 15:
                    {
                        FightPoint += nValue*tbLevel.LuckyProFightPoint/100;
                    }
                        break;
                    case 17:
                    {
                        FightPoint += nValue*tbLevel.ExcellentProFightPoint/100;
                    }
                        break;
                    case 21:
                    {
                        FightPoint += nValue*tbLevel.DamageAddProFightPoint/100;
                    }
                        break;
                    case 22:
                    {
                        FightPoint += nValue*tbLevel.DamageResProFightPoint/100;
                    }
                        break;
                    case 23:
                    {
                        FightPoint += nValue*tbLevel.DamageReboundProFightPoint/100;
                    }
                        break;
                    case 24:
                    {
                        FightPoint += nValue*tbLevel.IgnoreArmorProFightPoint/100;
                    }
                        break;
                    default:
                    {
                        var tbState = Table.GetStats((int) type);
                        if (tbState == null)
                        {
                            continue;
                        }
                        if (careerId >= 0)
                        {
                            FightPoint += tbState.FightPoint[careerId]*nValue/100;
                        }
                    }
                        break;
                }
            }

            //百分比计算
            FightPoint += attrRef[(int) eAttributeType.MagPowerMin]*tbLevel.PowerFightPoint/10000;
            FightPoint += attrRef[(int) eAttributeType.PhyArmor]*tbLevel.ArmorFightPoint/10000;
            FightPoint += attrRef[(int) eAttributeType.HpMax]*tbLevel.HpFightPoint/10000;
            FightPoint += attrRef[(int) eAttributeType.MpMax]*tbLevel.MpFightPoint/10000;
            FightPoint += attrRef[(int) eAttributeType.Hit]*tbLevel.HitFightPoint/10000;
            FightPoint += attrRef[(int) eAttributeType.Dodge]*tbLevel.DodgeFightPoint/10000;
            return (int)FightPoint;
        }

        //获得某个类型的ID
        public int GetTypeId(int type)
        {
            return GetExdata(1 + type*2);
        }

        //初始化数据
        private void Init(int nId, ItemBaseData Dbdata)
        {
            mDbData = Dbdata;
            SetId(nId);
            SetCount(1);
            CleanExdata();
            var tbWing = Table.GetWingQuality(nId);
            if (tbWing == null)
            {
                Logger.Error("WingQualityId  Id={0} not find", nId);
                return;
            }

            //初始翅膀祝福值
            AddExdata((int)eWingExDefine.eGrowValue);
            //培养数据
            for (var i = 0; i != 5; i++)
            {
                AddExdata(1 + i*10000);
                AddExdata(0);
            }
            AddExdata(1);  //11 装备模型显隐（1:显示 0:隐藏）
            // 保留
            for (var i = 0; i < 4; ++i)
            {
                AddExdata(0);
            }
        }

        //设置成长值
        public void SetGrowValue(int value)
        {
            SetExdata((int)eWingExDefine.eGrowValue, value);
        }

        //获得经验(type:[0,4])
        public void SetExp(int type, int value)
        {
            SetExdata(2 + type*2, value);
        }

        //设置某个类型的ID(type:[0,4])
        public void SetTypeId(int type, int value)
        {
            SetExdata(1 + type*2, value);
        }

        private Dictionary<int, int> growPropDict = null;

        public void ClearGrowProperty()
        {
            if (growPropDict != null)
                growPropDict.Clear();

            for (var i = (int)eWingExDefine.eGrowProperty; i < mDbData.Exdata.Count; ++i)
            {
                mDbData.Exdata[i] = 0;
            }
        }

        public void SetGrowProperty(int attrId, int value)
        {
            if (growPropDict == null)
                growPropDict = new Dictionary<int,int>();

            var extendIndex = -1;
            if (!growPropDict.TryGetValue(attrId, out extendIndex))
            {
                // 没有缓存，查找
                var found = false;
                for (var i = (int)eWingExDefine.eGrowProperty; i < mDbData.Exdata.Count; ++i)
                {
                    var saveId = GetGrowAttrId(mDbData, i);
                    if (saveId == attrId)
                    {
                        extendIndex = i + 1;
                        found = true;
                        break;
                    }

                    if (saveId == 0)    // 
                    {
                        mDbData.Exdata[i] = GetGrowSaveValue(attrId);
                        mDbData.Exdata[i + 1] = value;
                        break;
                    }
                    
                    if (saveId > 0)
                        ++i;
                }

                if (found == false)
                { // 没找到
                    mDbData.Exdata.Add(GetGrowSaveValue(attrId));
                    mDbData.Exdata.Add(value);
                    extendIndex = mDbData.Exdata.Count - 1;
                }
            }

            if (extendIndex >= (int)eWingExDefine.eGrowProperty)
            {
                growPropDict[attrId] = extendIndex;
                mDbData.Exdata[extendIndex] += value;
            }
        }

        private static int GetGrowAttrId(ItemBaseData dbData, int index)
        {
            if (index < (int)eWingExDefine.eGrowProperty)
                return -1;

            var attrId = -1;
            if (index < dbData.Exdata.Count)
            {
                attrId = dbData.Exdata[index];
            }
            return attrId;
        }

        private int GetGrowSaveValue(int attrId)
        {
            return attrId;
        }
    }
}