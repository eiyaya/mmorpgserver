#region using

using System;
using System.Collections.Generic;
using System.Linq;
using DataContract;
using DataTable;

#endregion

namespace Shared
{
    /*很重要的一个东西就是装备的附加属性条数
     * 0、装备强化等级
     * 1、追加值
     * 2、 绿色属性1的值
     * 3、 绿色属性2的值
     * 4、 绿色属性3的值
     * 5、 绿色属性4的值
     * 6、 紫色属性1ID	
     * 7、 紫色属性2ID	
     * 8、 紫色属性3ID	
     * 9、 紫色属性4ID	
     * 10、紫色属性5ID	
     * 11、紫色属性6ID	
     * 12、紫色属性1值	
     * 13、紫色属性2值	
     * 14、紫色属性3值	
     * 15、紫色属性4值	
     * 16、紫色属性5值	
     * 17、紫色属性6值	
     * 18、【绿色属性洗炼值1】
     * 19、【绿色属性洗炼值2】
     * 20、【绿色属性洗炼值3】
     * 21、【绿色属性洗炼值4】
     * 22、 当前耐久度
     * 23、 是否绑定(1是绑定)
     * 24、洗绿色属性的次数
     * 25、追加消耗道具数量
     * 26、初始装备所带技能等级
     * 27、初始装备所带buff
     * 28、随机技能暂时保存
     * 29、试用时间
     * 30、？
     * 31、装备模型显隐
     * 32、时装限时
     * 33、随机属性1
     * 34、随机属性2
     * 35、【星级(紫色)属性ID1】
     * 36、【星级(紫色)属性ID2】
     * 37、【星级(紫色)属性ID3】
     * 38、【星级(紫色)属性ID4】
     * 39、【星级(紫色)属性ID5】
     * 40、【星级(紫色)属性ID6】
     * 41、【星级(紫色)属性随灵值1】
     * 42、【星级(紫色)属性随灵值2】
     * 43、【星级(紫色)属性随灵值3】
     * 44、【星级(紫色)属性随灵值4】
     * 45、【星级(紫色)属性随灵值5】
     * 46、【星级(紫色)属性随灵值6】
     * 47、 
     * 48、 
     */

    public class ItemEquip2 : ItemBase
    {
        #region 属性ID和表中索引互转数据
        private static readonly Dictionary<int, int> AttrIdtoIndex = new Dictionary<int, int>
        {
            {13, 0},
            {14, 1},
            {9, 2},
            {12, 3},
            {19, 4},
            {20, 5},
            {17, 6},
            {21, 7},
            {22, 8},
            {23, 9},
            {24, 10},
            {26, 11},
            {25, 12},
            {105, 13},
            {110, 14},
            {113, 15},
            {114, 16},
            {119, 17},
            {120, 18},
            {106, 19},
            {111, 20},
            {98, 21},
            {99, 22}
        };

        //表格索引
        private static readonly List<int> IndextoAttrId = new List<int>
        {
            13,
            14,
            9,
            12,
            19,
            20,
            17,
            21,
            22,
            23,
            24,
            26,
            25,
            105,
            110,
            113,
            114,
            119,
            120,
            106,
            111,
            98,
            99
        };
        #endregion
        //private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public ItemEquip2()
        {
        }

        public ItemEquip2(int nId, ItemBaseData Dbdata)
        {
            Init(nId, Dbdata);
            //Dictionary<int,int> attrlist=new Dictionary<int, int>();
            //foreach (KeyValuePair<int, int> i in attrlist)
            //{
            //    string attrname = Table.GetAttrRef(i.Key).Desc;
            //    Logger.Info("-----属性:{0}+{1}-----", attrname, i.Value);
            //}
        }

        public ItemEquip2(int nId, ItemBaseData Dbdata, int addAttrCount)
        {
            Init(nId, Dbdata, addAttrCount);
        }

        public ItemEquip2(ItemBaseData Dbdata, bool IsNull = true)
        {
            mDbData = Dbdata;
            if (IsNull)
            {
                SetId(-1);
                SetCount(0);
            }
            for (var i = mDbData.Exdata.Count; i < (int) EquipExdataDefine.Count; ++i)
            {
                AddExdata(-1);
            }
            CheckTrialEquip();

            //现在的星级(紫色)属性，必须撸一遍以前的星级(紫色)属性，把以前的值扩大100呗
            for (var i = 12; i != 17; ++i)
            {
                CheckOldAddAttriValueGetExdata(i);
            }
            //随机得到的星级(紫色)属性，必须撸一遍以前的星级(紫色)属性，把以前的值扩大100呗
            for (var i = 41; i != 46; ++i)
            {
                CheckOldAddAttriValueGetExdata(i);
            }
        }

        //增加附加属性
        private void AddAttr(int nIndex, int nAttrId, int nAttrValue,bool isSave = false)
        {

            if (isSave)
            {
                SetExdata(nIndex + 6, nAttrId); 
                SetExdata(nIndex + 12, nAttrValue);
            }
            else
            {
                SetExdata(nIndex + 35, nAttrId); //只用来保存随机出来的值
                SetExdata(nIndex + 41, nAttrValue); //只用来保存随机出来的值
            }
        }

        //属性转换 fromList toList toRefList
        public static void AttrConvert(Dictionary<int, int> AttrList,
                                       int[] attr,
                                       int[] attrRef,
                                       int attackType,
                                       bool IsEquip = false)
        {
            foreach (var i in AttrList)
            {
                if (i.Key < (int) eAttributeType.AttrCount)
                {
                    attr[i.Key] += i.Value;
                }
                else
                {
                    switch (i.Key)
                    {
                        case 105:
                        {
                            if (attackType == 1)
                            {
                                attr[(int) eAttributeType.MagPowerMin] += i.Value;
                                attr[(int) eAttributeType.MagPowerMax] += i.Value;
                            }
                            else
                            {
                                attr[(int) eAttributeType.PhyPowerMin] += i.Value;
                                attr[(int) eAttributeType.PhyPowerMax] += i.Value;
                            }
                        }
                            break;
                        case 106:
                        {
                            if (IsEquip)
                            {
                                //attrRef[(int) eAttributeType.MagPowerMin] += i.Value*100;
                                //attrRef[(int) eAttributeType.MagPowerMax] += i.Value*100;
                                //attrRef[(int) eAttributeType.PhyPowerMin] += i.Value*100;
                                //attrRef[(int) eAttributeType.PhyPowerMax] += i.Value*100;
                                attrRef[(int)eAttributeType.MagPowerMin] += i.Value;
                                attrRef[(int)eAttributeType.MagPowerMax] += i.Value;
                                attrRef[(int)eAttributeType.PhyPowerMin] += i.Value;
                                attrRef[(int)eAttributeType.PhyPowerMax] += i.Value;
                            }
                            else
                            {
                                attrRef[(int) eAttributeType.MagPowerMin] += i.Value;
                                attrRef[(int) eAttributeType.MagPowerMax] += i.Value;
                                attrRef[(int) eAttributeType.PhyPowerMin] += i.Value;
                                attrRef[(int) eAttributeType.PhyPowerMax] += i.Value;
                            }
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
                            if (IsEquip)
                            {
                                //attrRef[(int) eAttributeType.PhyArmor] += i.Value*100;
                                //attrRef[(int) eAttributeType.MagArmor] += i.Value*100;
                                attrRef[(int)eAttributeType.PhyArmor] += i.Value;
                                attrRef[(int)eAttributeType.MagArmor] += i.Value;
                            }
                            else
                            {
                                attrRef[(int) eAttributeType.PhyArmor] += i.Value;
                                attrRef[(int) eAttributeType.MagArmor] += i.Value;
                            }
                        }
                            break;
                        case 113:
                        {
                            if (IsEquip)
                            {
                                //attrRef[(int) eAttributeType.HpMax] += i.Value*100;
                                attrRef[(int)eAttributeType.HpMax] += i.Value;
                            }
                            else
                            {
                                attrRef[(int) eAttributeType.HpMax] += i.Value;
                            }
                        }
                            break;
                        case 114:
                        {
                            if (IsEquip)
                            {
                                //attrRef[(int) eAttributeType.MpMax] += i.Value*100;
                                attrRef[(int)eAttributeType.MpMax] += i.Value;
                            }
                            else
                            {
                                attrRef[(int) eAttributeType.MpMax] += i.Value;
                            }
                        }
                            break;
                        case 119:
                        {
                            if (IsEquip)
                            {
                                //attrRef[(int) eAttributeType.Hit] += i.Value*100;
                                attrRef[(int)eAttributeType.Hit] += i.Value;
                            }
                            else
                            {
                                attrRef[(int) eAttributeType.Hit] += i.Value;
                            }
                        }
                            break;
                        case 120:
                        {
                            if (IsEquip)
                            {
                                //attrRef[(int) eAttributeType.Dodge] += i.Value*100;
                                attrRef[(int)eAttributeType.Dodge] += i.Value ;
                            }
                            else
                            {
                                attrRef[(int) eAttributeType.Dodge] += i.Value;
                            }
                        }
                            break;
                    }
                }
            }
        }

        public static int GetAttrId(int index)
        {
            if (index > IndextoAttrId.Count || index < 0)
            {
                Logger.Error("GetAttrId index={0}", index);
                return -1;
            }
            return IndextoAttrId[index];
        }

        private static int GetAttrIndex(int attrId)
        {
            int index;
            if (AttrIdtoIndex.TryGetValue(attrId, out index))
            {
                return index;
            }
            Logger.Error("GetAttrIndex attrId={0}", attrId);
            return -1;
        }

        //获得属性列表
        public void GetAttrList(Dictionary<int, int> AttrList, EquipRecord tbEquip, int characterLevel, int attackType)
        {
            var nLevel = GetExdata(0);
            var tblevel = Table.GetLevelData(nLevel);
            //会被强化影响的基础属性
            for (var i = 0; i != 4; ++i)
            {
                var nAttrId = tbEquip.BaseAttr[i];
                if (nAttrId == -1)
                {
                    continue;
                }
                var nValue = tbEquip.BaseValue[i];
                nValue = GetBaseValueRef(nAttrId, nValue, tblevel);
                PushEquipAttr(AttrList, nAttrId, nValue, characterLevel, attackType);
            }
            //基础属性
            for (var i = 0; i <2; ++i)
            {
                var nAttrId = tbEquip.BaseFixedAttrId[i];
                if (nAttrId == -1)
                {
                    continue;
                }
                var nValue = GetExdata(33+i);//MyRandom.Random(tbEquip.BaseFixedAttrValue[i], tbEquip.BaseFixedAttrValueMax[i]);
                if(nValue>0)
                    PushEquipAttr(AttrList, nAttrId, nValue, characterLevel, attackType);
            }
            //卓越属性
            for (var i = 0; i != 4; ++i)
            {
                var nValue = GetExdata(2 + i);
                var nAttrId = tbEquip.ExcellentAttrId[i];
                if (nValue > 0 && nAttrId != -1)
                {
                    PushEquipAttr(AttrList, nAttrId, nValue, characterLevel, attackType);
                }
            }
            //追加属性
            if (tbEquip.AddAttrId != -1)
            {
                PushEquipAttr(AttrList, tbEquip.AddAttrId, GetExdata(1), characterLevel, attackType);
            }
            //紫色属性
            for (var i = 0; i != 6; ++i)
            {
                var nValue = (GetExdata(12 + i)); //虽然紫色属性扩大了100倍，但是这里先不能处理，这里是百分比，得在计算完的时候在处理
                var nAttrId = GetExdata(6 + i);
                if (nValue > 0 && nAttrId != -1)
                {
                    PushEquipAttr(AttrList, nAttrId, nValue, characterLevel, attackType);
                }
            }
        }

        //获得某个属性的值
        public int GetAttrValue(int nAttrId, EquipRecord tbEquip, LevelDataRecord tblevel, ref int bili)
        {
            var nValue = 0;
            nValue += GetAttrValueBase(nAttrId, GetExdata(0), tbEquip);
            nValue += GetAttrValueAdd(nAttrId, GetExdata(1), tblevel);
            //取消宝石表
            //nValue += GetAttrValueGem(nAttrId);
            return nValue;
        }

        //获得附加属性
        private int GetAttrValueAdd(int nAttrId, int nLevel, LevelDataRecord tblevel)
        {
            var nValue = 0;
            for (var i = 0; i != 8; ++i)
            {
                if (GetExdata(i + 6) == nAttrId)
                {
                    nValue += (GetExdata(i + 12));
                }
            }
            if (nValue <= 0)
            {
                return 0;
            }
            return nValue;
        }

        //获得基础属性
        private int GetAttrValueBase(int nAttrId, int nLevel, EquipRecord tbEquip = null)
        {
            if (tbEquip == null)
            {
                var equipid = Table.GetItemBase(GetId()).Exdata[0];
                tbEquip = GetTableEquip(equipid);
                if (tbEquip == null)
                {
                    Logger.Error("GetAttrValue itemId  Id={0} not find equip={1}", GetId(), equipid);
                    return 0;
                }
            }

            var nValue = 0;
            for (var i = 0; i != tbEquip.BaseAttr.Length; ++i)
            {
                if (tbEquip.BaseAttr[i] == nAttrId)
                {
                    nValue += tbEquip.BaseValue[i];
                }
            }
            if (nValue == 0)
            {
                return 0;
            }
            return GetBaseValueRef(nAttrId, nValue);
        }

        //获得基础属性的强化等级修正
        private int GetBaseValueRef(int nAttrId, int nValue, LevelDataRecord tblevel = null)
        {
            if (tblevel == null)
            {
                tblevel = Table.GetLevelData(GetExdata(0));
            }
            var attr = (eAttributeType) nAttrId;
            switch (attr)
            {
                case eAttributeType.PhyPowerMin:
                {
                    if (tblevel != null)
                    {
                        return nValue*tblevel.PhyPowerMinScale/100 + tblevel.PhyPowerMinFix;
                    }
                }
                    break;
                case eAttributeType.PhyPowerMax:
                {
                    if (tblevel != null)
                    {
                        return nValue*tblevel.PhyPowerMaxScale/100 + tblevel.PhyPowerMaxFix;
                    }
                }
                    break;
                case eAttributeType.MagPowerMin:
                {
                    if (tblevel != null)
                    {
                        return nValue*tblevel.MagPowerMinScale/100 + tblevel.MagPowerMinFix;
                    }
                }
                    break;
                case eAttributeType.MagPowerMax:
                {
                    if (tblevel != null)
                    {
                        return nValue*tblevel.MagPowerMaxScale/100 + tblevel.MagPowerMaxFix;
                    }
                }
                    break;
                case eAttributeType.PhyArmor:
                {
                    if (tblevel != null)
                    {
                        return nValue*tblevel.PhyArmorScale/100 + tblevel.PhyArmorFix;
                    }
                }
                    break;
                case eAttributeType.MagArmor:
                {
                    if (tblevel != null)
                    {
                        return nValue*tblevel.MagArmorScale/100 + tblevel.MagArmorFix;
                    }
                }
                    break;
                case eAttributeType.HpMax:
                {
                    if (tblevel != null)
                    {
                        return nValue*tblevel.HpMaxScale/100 + tblevel.HpMaxFix;
                    }
                }
                    break;
                case eAttributeType.ExcellentPro:
                {
                    return nValue;
                }
                default:
                    Logger.Error("GetBaseValueRef attr={0}", attr);
                    return nValue;
            }
            return nValue;
        }

        public bool GetBinding()
        {
            var old = GetExdata(23);
            return BitFlag.GetLow(old, 0);
        }

        //获得某个属性的最大值
        public static int GetExcellentMaxValue(EquipEnchantRecord tbEnchant, int nAttrId)
        {
            var index = GetAttrIndex(nAttrId);
            if (index == -1)
            {
                return -1;
            }
            var Value = tbEnchant.Attr[index];
            return Value;
        }

        //获得槽数量
        public int GetSlotCount()
        {
            var nCount = 0;
            for (var i = 0; i != 4; ++i)
            {
                if (GetExdata(i + 2) > -2)
                {
                    nCount++;
                }
            }
            return nCount;
        }

        //获得装备物品
        public static EquipRecord GetTableEquip(int nId)
        {
            return Table.GetEquip(nId);
        }

        //初始化数据
        private void Init(int nId, ItemBaseData Dbdata, int addAttrCount = -1)
        {
            mDbData = Dbdata;
            SetId(nId);
            SetCount(1);
            CleanExdata();
            var tbitem = Table.GetItemBase(GetId());
            var equipid = tbitem.Exdata[0];
            var tbEquip = GetTableEquip(equipid);
            if (tbEquip == null)
            {
                Logger.Error("itemId  Id={0} not find equip={1}", GetId(), equipid);
                return;
            }

            //初始强化等级
            AddExdata(0);
            //初始话追加属性
            AddExdata(MyRandom.Random(tbEquip.AddAttrUpMinValue, tbEquip.AddAttrUpMaxValue));
            //随机卓越属性
            for (var i = 0; i != 4; ++i)
            {
                AddExdata(0);
            }
            var ExcellentCount = RandomAddCount(tbEquip, tbEquip.ExcellentAttrCount);
            InitExcellentData(tbEquip, ExcellentCount);
            ////随机宝石
            //for (int i = 0; i != 4; ++i)
            //{
            //    AddExdata(-2);
            //}
            //InitSlot(tbEquip.RandomSlotCount);
            //随机附加属性
            for (var i = 0; i != 12; ++i)
            {
                AddExdata(-1);
            }
            var addCount = addAttrCount;
            if (addCount == -1)
            {
                addCount = RandomAddCount(tbEquip, tbEquip.RandomAttrCount);
            }
            InitAddAttr(tbEquip, addCount);
            //洗随机属性
            for (var i = 0; i != 4; ++i)
            {
                AddExdata(-1);
                //SetExdata(18 + i, -1);
            }
            //耐久度
            AddExdata(tbEquip.Durability);
            //是否绑定
            AddExdata(0);
            //洗绿色属性的次数
            AddExdata(0);
            AddExdata(0);
            //初始装备所带技能等级
            AddExdata(tbEquip.AddBuffSkillLevel);   // 26
            AddExdata(RandBuffId());   // 27,随机技能
            AddExdata(-1);  // 28
            var time = tbitem.TimeLimit;
            if (time >= 0)
            {
                time = tbitem.TimeLimit * 60;
                time = Math.Max(0, time);
            }
            AddExdata(time);  // 29 试用时间s（-1，永久）
            AddExdata(-1);  // 30 
            AddExdata(1);  // 31 装备模型显隐（1:显示 0:隐藏）
            AddExdata(-1); // 32 时装限时
            for (int i = 0; i < 2; i++)
            {//33 34 随机属性
                if (tbEquip.BaseFixedAttrId[i] >= 0)
                {
                    int nValue = MyRandom.Random(tbEquip.BaseFixedAttrValue[i], tbEquip.BaseFixedAttrValueMax[i]);
                    AddExdata(nValue); //33 随机属性1
                }
                else
                {
                    AddExdata(-1); //33 随机属性1
                }
            }
            //星级(紫色)属性随机值得临时保存
            for (var i = 0; i != 12; ++i)
            {
                AddExdata(-1);
            }
            CheckTrialEquip();
        }

        public void CheckTrialEquip()
        {
            var time = GetExdata((int)EquipExdataDefine.TrialTime);
            if (time >= 0)
            {
                isTrialEquip = true;
                thisTrialTime = DateTime.Now;
                SetExdata((int)EquipExdataDefine.TrialEndTime, DateTime.Now.AddSeconds(time).GetTimeStampSeconds());
            }
        }

        public override bool TrialTimeCost()
        {
            if (isTrialEquip)
            {
                var leftTime = GetExdata((int) EquipExdataDefine.TrialTime);
                if (leftTime > 0)
                {
                    var seconds = (int)DateTime.Now.GetDiffSeconds(thisTrialTime);
                    if (seconds < 1)
                        return false;

                    leftTime = Math.Max(0, leftTime - seconds);
                    if (leftTime < 5)
                    {
                        leftTime = 0;
                    }
                    SetExdata((int)EquipExdataDefine.TrialTime, leftTime);
                    if (leftTime == 0)
                    {
                        MarkDbDirty();
                        return true;
                    }
                }
            }

            return false;
        }

        public override bool IsTrialEnd()
        {
            if (isTrialEquip)
            {
                return DateTime.Now.GetTimeStampSeconds() >= GetExdata((int)EquipExdataDefine.TrialEndTime);
            }

            return false;
        }

        // 策划给武器添加或删除buff时，重新处理下
        public override void ReCalcBuff()
        {
            var tbItem = Table.GetItemBase(GetId());
            if (tbItem != null)
            {
                if (tbItem.InitInBag == (int)eBagType.Wing)
                    return;

                var equipId = tbItem.Exdata[0];
                var tbEquip = Table.GetEquip(equipId);
                if (tbEquip != null)
                {
                    var buffId = GetExdata((int)EquipExdataDefine.BuffId);
                    if (tbEquip.BuffGroupId >= 0)
                    { // 有buff
                        if (buffId < 0)
                        { // 原先无buff,随机下
                            ReRandomBuffId();
                        }
                    }
                    else
                    { // 无buff
                        if (buffId >= 0)
                        { // 原先有buff,删除
                            SetExdata((int)EquipExdataDefine.BuffId, -1);
                            MarkDirty();
                        }                        
                    }
                }
            }
        }

        public override int GetBuffId(int index)
        {
            return GetExdata((int)EquipExdataDefine.BuffId);
        }

        public override int GetBuffLevel(int index)
        {
            return GetExdata((int)EquipExdataDefine.SkillLevel);
        }

        public void SetBuffLevel(int level)
        {
            SetExdata((int)EquipExdataDefine.SkillLevel, level);
        }

        // 重置buffId
        public void ReRandomBuffId()
        {
            var buffId = RandBuffId();
            if (buffId != GetExdata((int) EquipExdataDefine.BuffId))
            {
                SetExdata((int)EquipExdataDefine.BuffId, buffId);
                MarkDirty();
            }
        }

        public int RandBuff(int itemId)
        {
            var tbItem = Table.GetItemBase(itemId);
            if (tbItem != null)
            {
                var buffGroupId = tbItem.Exdata[0];
                var buffId = RandBuffId(buffGroupId);
                SetExdata((int)EquipExdataDefine.RandBuffId, buffId);
                MarkDirty();
                return buffId;
            }
            return -1;
        }

        public bool UseRandBuff()
        {
            var buffId = GetExdata((int) EquipExdataDefine.RandBuffId);
            if (buffId != -1 && buffId != GetBuffId(0))
            {
                SetExdata((int)EquipExdataDefine.BuffId, buffId);
                SetExdata((int)EquipExdataDefine.RandBuffId, -1);
                MarkDirty();
                return true;
            }

            return false;
        }

        public void CancleRandBuff()
        {
            SetExdata((int)EquipExdataDefine.RandBuffId, -1);
            MarkDirty();
        }

        //private void InitExcellentData(EquipRecord tbEquip, int NowAttrCount)
        //{
        //    if (NowAttrCount <= 0 || NowAttrCount > 4) return;
        //    EquipEnchantRecord tbEnchant = Table.GetEquipEnchant(tbEquip.ExcellentAttrValue);
        //    if (tbEnchant == null)
        //    {
        //        Logger.Error("InitExcellentData:Equip Id={0} not find ExcellentAttrValue Id={1}", tbEquip.Id, tbEquip.ExcellentAttrValue);
        //        return;
        //    }
        //    EquipRelateRecord tbAttrRelate = Table.GetEquipRelate(tbEquip.ExcellentAttrInterval);
        //    if (tbAttrRelate == null)
        //    {
        //        Logger.Error("InitExcellentData:Equip Id={0} not find EquipRelate Id={1}", tbEquip.Id, tbEquip.ExcellentAttrInterval);
        //        return;
        //    }
        //    int nRandom, nTotleRandom;
        //    for (int i = 0; i != NowAttrCount; ++i)
        //    {
        //        int nAttrId = tbEquip.ExcellentAttrId[i];
        //        int index = GetAttrIndex(nAttrId);
        //        if (index == -1) continue;
        //        int fValue = tbEnchant.Attr[index];
        //        int AttrValueMin = fValue * tbEquip.ExcellentValueMin / 100;
        //        int AttrValueMax = fValue * tbEquip.ExcellentValueMax / 100;
        //        int AttrValueDiff = AttrValueMax - AttrValueMin;
        //        nRandom = MyRandom.Random(10000);
        //        nTotleRandom = 0;
        //        int rMin = 0;
        //        int rMax = 10000;
        //        for (int j = 0; j != tbAttrRelate.Value.Length; ++j)
        //        {
        //            nTotleRandom += tbAttrRelate.Value[j];
        //            if (nRandom < nTotleRandom)
        //            {
        //                switch (j)
        //                {
        //                    case 0:
        //                        {
        //                            rMin = 0;
        //                            rMax = 5000;
        //                        }
        //                        break;
        //                    case 1:
        //                        {
        //                            rMin = 5000;
        //                            rMax = 7500;
        //                        }
        //                        break;
        //                    case 2:
        //                        {
        //                            rMin = 7500;
        //                            rMax = 9000;
        //                        }
        //                        break;
        //                    case 3:
        //                        {
        //                            rMin = 9000;
        //                            rMax = 10000;
        //                        }
        //                        break;
        //                }
        //                break;
        //            }
        //        }
        //        int AttrValueMin2 = AttrValueMin + AttrValueDiff * rMin / 10000;
        //        int AttrValueMax2 = AttrValueMin + AttrValueDiff * rMax / 10000;
        //        int AttrValue = MyRandom.Random(AttrValueMin2, AttrValueMax2);
        //        SetExdata(i + 2, AttrValue);
        //    }
        //}
        //初始化附加属性

        /// <summary>
        /// 初始化星级属性(紫色属性)
        /// </summary>
        /// <param name="tbEquip">装备表记录</param>
        /// <param name="addCount">随机条数</param>
        public void InitAddAttr(EquipRecord tbEquip, int addCount) 
        {
            if (addCount <= 0 || addCount > 6)
            {
                return;
            }
            int nRandom, nTotleRandom;
            var TbAttrPro = Table.GetEquipEnchantChance(tbEquip.RandomAttrPro);//tbEquip.RandomAttrPro->紫色属性类型概率
            if (TbAttrPro == null)
            {
                Logger.Error("Equip InitAddAttr Id={0} not find EquipEnchantChance Id={1}", tbEquip.Id,
                    tbEquip.RandomAttrPro);
                return;
            }
            var tempAttrPro = new Dictionary<int, int>();//每种属性的概率
            var nTotleAttrPro = 0;//总概率
            for (var i = 0; i != 23; ++i)
            {
                var nAttrpro = TbAttrPro.Attr[i];
                if (nAttrpro > 0)
                {
                    nTotleAttrPro += nAttrpro;
                    tempAttrPro[i] = nAttrpro;
                }
            }
            //属性值都在这里，存的都是上限
            var tbEnchant = Table.GetEquipEnchant(tbEquip.RandomAttrValue);
            if (tbEnchant == null)
            {
                Logger.Error("Equip InitAddAttr Id={0} not find tbEquipEnchant Id={1}", tbEquip.Id,
                    tbEquip.RandomAttrValue);
                return;
            }
            //整理概率
            var AttrValue = new Dictionary<int, int>();//随机到的属性和属性值上限
            for (var i = 0; i != addCount; ++i)//随机几条属性
            {
                nRandom = MyRandom.Random(nTotleAttrPro);
                nTotleRandom = 0;
                foreach (var i1 in tempAttrPro)
                {
                    nTotleRandom += i1.Value;
                    if (nRandom < nTotleRandom)//随机到了
                    {
                        //AddCount = i1.Key;
                        AttrValue[i1.Key] = tbEnchant.Attr[i1.Key];
                        nTotleAttrPro -= i1.Value;
                        tempAttrPro.Remove(i1.Key);//随即到的属性移除掉，不要重复随机
                        break;//必须加个break，删了会报错
                    }
                }
            }
            var NowAttrCount = AttrValue.Count;
            if (NowAttrCount < addCount)
            {
                Logger.Error("Equip InitAddAttr AddAttr Not Enough AddCount={0},NowAttrCount={1}", addCount,
                    NowAttrCount);
            }

            var tbAttrRelate = Table.GetEquipRelate(tbEquip.RandomAttrInterval);
            if (tbAttrRelate == null)
            {
                Logger.Error("Equip tbAttrRelate Id={0} not find EquipRelate Id={1}", tbEquip.Id,
                    tbEquip.RandomAttrInterval);
                return;
            }

            for (var i = 0; i != NowAttrCount; ++i)
            {
                var nKey = AttrValue.Keys.Min();
                var nAttrId = GetAttrId(nKey);//EquipEnchantChance、EquipEnchant列索引转成对应的属性值，比如第一列Key=0，对应的是生命上限属性枚举值
                if (nAttrId == -1)
                {
                    continue;
                }
                var fValue = tbEnchant.Attr[nKey];                       //属性值的上限，也可能不是上线只是个值
                var AttrValueMin = fValue*tbEquip.RandomValueMin/100;    //变化的最小浮动值
                var AttrValueMax = fValue * tbEquip.RandomValueMax / 100;//变化的最大浮动值
                var AttrValueDiff = AttrValueMax - AttrValueMin;         //浮动差差值
                nRandom = MyRandom.Random(10000);
                nTotleRandom = 0;
                var rMin = 0;
                var rMax = 10000;
                for (var j = 0; j != tbAttrRelate.Value.Length; ++j)  //差值再取百分比，变化差值的50%,75% 90%,100%
                {
                    nTotleRandom += tbAttrRelate.Value[j];
                    if (nRandom < nTotleRandom)
                    {
                        switch (j)
                        {
                            case 0:
                            {
                                rMin = 0;
                                rMax = 5000;
                            }
                                break;
                            case 1:
                            {
                                rMin = 5000;
                                rMax = 7500;
                            }
                                break;
                            case 2:
                            {
                                rMin = 7500;
                                rMax = 9000;
                            }
                                break;
                            case 3:
                            {
                                rMin = 9000;
                                rMax = 10000;
                            }
                                break;
                        }
                        break;
                    }
                }
                var AttrValueMin2 = AttrValueMin + AttrValueDiff*rMin/10000;
                var AttrValueMax2 = AttrValueMin + AttrValueDiff*rMax/10000;
                var AttrRealValue = MyRandom.Random(AttrValueMin2, AttrValueMax2);

                // 小凡的需求，要把 AttrRealValue 在 (0, 1) 范围内的数值，变成1
                if (AttrRealValue > 0 && AttrRealValue < 100)
                    AttrRealValue = 100;//最小取1%
                if (AttrRealValue > 100)
                {
                    var singleDigit = AttrRealValue % 10;
                    AttrRealValue = AttrRealValue - singleDigit;
                }
                AddAttr(i, nAttrId, AttrRealValue,true);
                AttrValue.Remove(nKey);
            }
        }

        //初始化卓越属性(暂时改为初始化时为最低值)
        private void InitExcellentData(EquipRecord tbEquip, int NowAttrCount)
        {
            if (NowAttrCount <= 0 || NowAttrCount > 4)
            {
                return;
            }
            var tbEnchant = Table.GetEquipEnchant(tbEquip.ExcellentAttrValue);
            if (tbEnchant == null)
            {
                Logger.Error("InitExcellentData:Equip Id={0} not find ExcellentAttrValue Id={1}", tbEquip.Id,
                    tbEquip.ExcellentAttrValue);
                return;
            }
            var tbAttrRelate = Table.GetEquipRelate(tbEquip.ExcellentAttrInterval);
            if (tbAttrRelate == null)
            {
                Logger.Error("InitExcellentData:Equip Id={0} not find EquipRelate Id={1}", tbEquip.Id,
                    tbEquip.ExcellentAttrInterval);
                return;
            }
            for (var i = 0; i != NowAttrCount; ++i)
            {
                var nAttrId = tbEquip.ExcellentAttrId[i];
                var index = GetAttrIndex(nAttrId);
                if (index == -1)
                {
                    continue;
                }
                var fValue = tbEnchant.Attr[index];
                int AttrValueMin;
                if (nAttrId == 98 || nAttrId == 99)
                {
                    AttrValueMin = fValue*tbEquip.ExcellentValueMax/100;
                }
                else
                {
                    AttrValueMin = fValue*tbEquip.ExcellentValueMin/100;
                }
                SetExdata(i + 2, AttrValueMin);
            }
        }

        //初始化槽
        private void InitSlot(int RndSlot)
        {
            if (RndSlot == -1)
            {
                return;
            }
            var tbRelate = Table.GetEquipRelate(RndSlot);
            if (tbRelate == null)
            {
                Logger.Error("EquipRelate Id={0} not find", RndSlot);
                return;
            }
            var SlotCount = 0;
            var nRandom = MyRandom.Random(10000);
            var nTotleRandom = 0;
            for (var i = 0; i != tbRelate.Slot.Length; ++i)
            {
                nTotleRandom += tbRelate.Slot[i];
                if (nRandom < nTotleRandom)
                {
                    SlotCount = i;
                    break;
                }
            }
            for (var i = 0; i < SlotCount; ++i)
            {
                SetExdata(i + 2, -1);
            }
        }

        //是否可以穿
        public static bool IsCanEquip(EquipRecord tbEquip, int PartBag)
        {
            return BitFlag.GetLow(tbEquip.Part, PartBag - 7);
        }

        public static bool IsShiZhuangCanEquip(EquipRecord tbEquip, int PartBag)
        {
            return BitFlag.GetLow(tbEquip.Part, PartBag - 17);
        }

        //取消宝石表
        ////获得宝石属性
        //private int GetAttrValueGem(int nAttrId)
        //{
        //    int nValue = 0;
        //    for (int i = 2; i < 6; ++i)
        //    {
        //        int gemid = GetExdata(i);
        //        if(gemid<=0) continue;
        //        var tbGem = Table.GetGem(gemid);
        //        for (int j = 0; j != tbGem.AttrId.Length; ++j)
        //        {
        //            if (tbGem.AttrId[j] == nAttrId)
        //            {
        //                nValue += tbGem.AttrValue[j];
        //            }
        //        }
        //    }
        //    return nValue;
        //}
        //输出属性
        public void OutLook()
        {
            Logger.Info("-----Equip-----Itemid={0}-----Slot={1}", GetId(), GetSlotCount());
            for (var i = 0; i != 21; ++i)
            {
                var nValue = GetAttrValueBase(i, GetExdata(0));
                if (nValue > 0)
                {
                    var attrname = Table.GetAttrRef(i + 1).Desc;
                    Logger.Info("-----基础属性:{0}+{1}-----", attrname, nValue);
                }
            }
            var tblevel = Table.GetLevelData(GetExdata(1));
            for (var i = 0; i != 21; ++i)
            {
                var nValue = GetAttrValueAdd(i, GetExdata(1), tblevel);
                if (nValue > 0)
                {
                    var attrname = Table.GetAttrRef(i + 1).Desc;
                    Logger.Info("-----附加属性:{0}+{1}-----", attrname, nValue);
                }
            }
        }

        //添加属性到字典
        public static void PushEquipAttr(Dictionary<int, int> AttrList,
                                         int AttrId,
                                         int AttrValue,
                                         int characterLevel,
                                         int attackType)
        {
            if (AttrId == -1)
            {
            }
            else if (AttrId == 98)
            {
                var nValue = characterLevel/AttrValue;
                if (attackType != 1)
                {
                    AttrList.modifyValue((int) eAttributeType.PhyPowerMin, nValue);
                    AttrList.modifyValue((int) eAttributeType.PhyPowerMax, nValue);
                }
                else
                {
                    AttrList.modifyValue((int) eAttributeType.MagPowerMin, nValue);
                    AttrList.modifyValue((int) eAttributeType.MagPowerMax, nValue);
                }
            }
            else if (AttrId == 99)
            {
                var nValue = characterLevel/AttrValue;
                AttrList.modifyValue((int) eAttributeType.PhyArmor, nValue);
                AttrList.modifyValue((int) eAttributeType.MagArmor, nValue);
            }
            else
            {
                AttrList.modifyValue(AttrId, AttrValue);
            }
        }

        //随机属性条数随机
        private int RandomAddCount(EquipRecord tbEquip, int EquipRelateId)
        {
            if (EquipRelateId == -1)
            {
                return 0;
            }
            var tbRelate = Table.GetEquipRelate(EquipRelateId);
            if (tbRelate == null)
            {
                Logger.Error("EquipRelate Id={0} not find", EquipRelateId);
                return 0;
            }
            var AddCount = 0;
            var nRandom = MyRandom.Random(10000);
            var nTotleRandom = 0;
            for (var i = 0; i != tbRelate.AttrCount.Length; ++i)
            {
                nTotleRandom += tbRelate.AttrCount[i];
                if (nRandom < nTotleRandom)
                {
                    if (i == 0)
                    {
                        return 0;
                    }
                    AddCount = i;
                    break;
                }
            }
            return AddCount;
        }

        //设置绑定状态
        public void SetBinding()
        {
            var old = GetExdata(23);
            SetExdata(23, BitFlag.IntSetFlag(old, 0));
        }

        //设置耐久度
        public void SetDurable(int value)
        {
            if (value < 0)
            {
                value = 0;
            }
            SetExdata(22, value);
            //装备掉耐久不会掉到0，为了解决战斗力自动掉的问题
            //SetExdata(22, Math.Max(1, value));
            MarkDbDirty();
        }

        //设置等级
        public void SetLevel(int nLevel)
        {
            MarkDirty();
            SetExdata(0, nLevel);
        }

        #region 洗炼绿色属性

        //随机新的绿色卓越属性
        public void RandNewGreenAttr(EquipRecord tbEquip, List<int> attrList)
        {
            for (var i = 0; i != 4; ++i)
            {
                SetExdata(18 + i, -1);
                attrList.Add(0);
            }
            var NowAttrCount = RandomAddCount(tbEquip, tbEquip.ExcellentAttrCount);
            //InitExcellentData(tbEquip, ExcellentCount);
            if (NowAttrCount <= 0 || NowAttrCount > 4)
            {
                return;
            }
            var tbEnchant = Table.GetEquipEnchant(tbEquip.ExcellentAttrValue);
            if (tbEnchant == null)
            {
                Logger.Error("InitExcellentData:Equip Id={0} not find ExcellentAttrValue Id={1}", tbEquip.Id,
                    tbEquip.ExcellentAttrValue);
                return;
            }
            var tbAttrRelate = Table.GetEquipRelate(tbEquip.ExcellentAttrInterval);
            if (tbAttrRelate == null)
            {
                Logger.Error("InitExcellentData:Equip Id={0} not find EquipRelate Id={1}", tbEquip.Id,
                    tbEquip.ExcellentAttrInterval);
                return;
            }
            var RefreshCount = GetExdata(24);
            if (RefreshCount < 0)
            {
                RefreshCount = 0;
            }
            int rMin, rMax;
            switch (RefreshCount)
            {
                case 0:
                    rMin = 500;
                    rMax = 500;
                    break;
                case 1:
                    rMin = 700;
                    rMax = 700;
                    break;
                case 2:
                    rMin = 1000;
                    rMax = 1000;
                    break;
                default:
                    rMin = (RefreshCount - 2)*50 + 1000;
                    rMax = (RefreshCount - 2)*200 + 1000;
                    if (rMin > 10000)
                    {
                        rMin = 10000;
                    }
                    if (rMax > 10000)
                    {
                        rMax = 10000;
                    }
                    break;
            }
            for (var i = 0; i != NowAttrCount; ++i)
            {
                var nAttrId = tbEquip.ExcellentAttrId[i];
                var index = GetAttrIndex(nAttrId);
                if (index == -1)
                {
                    continue;
                }
                var fValue = tbEnchant.Attr[index];

                int AttrValueMin, AttrValueMax;
                if (nAttrId == 98 || nAttrId == 99)
                {
                    AttrValueMin = fValue*tbEquip.ExcellentValueMax/100;
                    AttrValueMax = fValue*tbEquip.ExcellentValueMin/100;
                }
                else
                {
                    AttrValueMin = fValue*tbEquip.ExcellentValueMin/100;
                    AttrValueMax = fValue*tbEquip.ExcellentValueMax/100;
                }
                //int AttrValueMin = fValue * tbEquip.ExcellentValueMin / 100;
                //int AttrValueMax = fValue * tbEquip.ExcellentValueMax / 100;
                var AttrValueDiff = AttrValueMax - AttrValueMin;
                var AttrValueMin2 = AttrValueMin + AttrValueDiff*rMin/10000;
                var AttrValueMax2 = AttrValueMin + AttrValueDiff*rMax/10000;
                var AttrValue = MyRandom.Random(AttrValueMin2, AttrValueMax2);
                SetExdata(i + 18, AttrValue);
                attrList[i] = AttrValue;
            }
            SetExdata(24, RefreshCount + 1);
        }

        //public void RandNewGreenAttr(EquipRecord tbEquip, List<int> attrList)
        //{
        //    for (int i = 0; i != 4; ++i)
        //    {
        //        SetExdata(18 + i, -1);
        //        attrList.Add(0);
        //    }
        //    int NowAttrCount = RandomAddCount(tbEquip, tbEquip.ExcellentAttrCount);
        //    //InitExcellentData(tbEquip, ExcellentCount);
        //    if (NowAttrCount <= 0 || NowAttrCount > 4) return;
        //    EquipEnchantRecord tbEnchant = Table.GetEquipEnchant(tbEquip.ExcellentAttrValue);
        //    if (tbEnchant == null)
        //    {
        //        Logger.Error("InitExcellentData:Equip Id={0} not find ExcellentAttrValue Id={1}", tbEquip.Id, tbEquip.ExcellentAttrValue);
        //        return;
        //    }
        //    EquipRelateRecord tbAttrRelate = Table.GetEquipRelate(tbEquip.ExcellentAttrInterval);
        //    if (tbAttrRelate == null)
        //    {
        //        Logger.Error("InitExcellentData:Equip Id={0} not find EquipRelate Id={1}", tbEquip.Id, tbEquip.ExcellentAttrInterval);
        //        return;
        //    }
        //    int nRandom, nTotleRandom;
        //    for (int i = 0; i != NowAttrCount; ++i)
        //    {
        //        int nAttrId = tbEquip.ExcellentAttrId[i];
        //        int index = GetAttrIndex(nAttrId);
        //        if (index == -1) continue;
        //        int fValue = tbEnchant.Attr[index];
        //        int AttrValueMin = fValue * tbEquip.ExcellentValueMin / 100;
        //        int AttrValueMax = fValue * tbEquip.ExcellentValueMax / 100;
        //        int AttrValueDiff = AttrValueMax - AttrValueMin;
        //        //int nRandomAttr = tbEquip.ExcellentValueMax - tbEquip.ExcellentValueMin;
        //        nRandom = MyRandom.Random(10000);
        //        nTotleRandom = 0;
        //        //int AttrValueMAX = tbEquip.RandomValueMax * fValue;
        //        int rMin = 0;
        //        int rMax = 10000;
        //        for (int j = 0; j != tbAttrRelate.Value.Length; ++j)
        //        {
        //            nTotleRandom += tbAttrRelate.Value[j];
        //            if (nRandom < nTotleRandom)
        //            {
        //                switch (j)
        //                {
        //                    case 0:
        //                        {
        //                            rMin = 0;
        //                            rMax = 5000;
        //                        }
        //                        break;
        //                    case 1:
        //                        {
        //                            rMin = 5000;
        //                            rMax = 7500;
        //                        }
        //                        break;
        //                    case 2:
        //                        {
        //                            rMin = 7500;
        //                            rMax = 9000;
        //                        }
        //                        break;
        //                    case 3:
        //                        {
        //                            rMin = 9000;
        //                            rMax = 10000;
        //                        }
        //                        break;
        //                }
        //                break;
        //            }
        //        }
        //        int AttrValueMin2 = AttrValueMin + AttrValueDiff * rMin / 10000;
        //        int AttrValueMax2 = AttrValueMin + AttrValueDiff * rMax / 10000;
        //        int AttrValue = MyRandom.Random(AttrValueMin2, AttrValueMax2);
        //        SetExdata(i + 18, AttrValue);
        //        attrList[i] = AttrValue;
        //    }
        //}
        //使用新的绿色卓越属性
        public ErrorCodes UseNewGreenAttr(int ok)
        {
            if (GetExdata(18) == -1)
            {
                return ErrorCodes.Unknow;
            }
            if (ok == 1)
            {
                for (var i = 0; i != 4; ++i)
                {
                    var newValue = GetExdata(18 + i);
                    if (newValue != -1)
                    {
//防止把属性值设为-1
                        SetExdata(2 + i, newValue);
                    }
                }
            }
            for (var i = 0; i != 4; ++i)
            {
                SetExdata(18 + i, -1);
            }
            return ErrorCodes.OK;
        }
        //使用新的星级属性
        public ErrorCodes UseNewSuperExcellentAttr(int ok)
        {
            if (1 == ok)
            {
                for (var i = 0; i <= 6; ++i)
                {
                    var newId = GetExdata(35 + i);
                    var newValue = GetExdata(41 + i);
                    if (newId != -1 && newValue!=0)
                    {
                        SetExdata(6 + i, newId);
                        SetExdata(12 + i, newValue);
                    }
                }
            }
            for (var i = 0; i <= 6; ++i)
            {
                SetExdata(35 + i, -1);
                SetExdata(41 + i, 0);
            }
            if (0 == ok)
            {
                return ErrorCodes.Unknow;
            }
            return ErrorCodes.OK;
        }

        #endregion

        #region 洗炼紫色属性

        //获得紫色属性条数
        public int GetPurpleAttrCount()
        {
            var count = 0;
            for (var i = 0; i != 6; ++i)
            {
                if (GetExdata(i + 6) == -1)
                {
                    break;
                }
                count++;
            }
            return count;
        }

        //随机新的紫色卓越属性
        public void  RandNewPurpleAttr(EquipRecord tbEquip,
                                      int EquipRelateId,
                                      List<int> attrBlockList,
                                      List<int> attrIdList,
                                      List<int> attrValueList)
        {
            #region 老的逻辑 ，属性和值一起随机
            /*
            for (var i = 0; i != 6; ++i)
            {
                attrIdList.Add(-1);
                attrValueList.Add(0);
            }
            var addCount = RandomAddCount(tbEquip, EquipRelateId);
            if (addCount <= 0 || addCount > 6)
            {
                return;
            }
            int nRandom, nTotleRandom;
            var TbAttrPro = Table.GetEquipEnchantChance(tbEquip.NewRandomAttrPro);
            if (TbAttrPro == null)
            {
                Logger.Error("Equip InitAddAttr Id={0} not find EquipEnchantChance Id={1}", tbEquip.Id,
                    tbEquip.NewRandomAttrPro);
                return;
            }
            var tempAttrPro = new Dictionary<int, int>();
            var nTotleAttrPro = 0;
            for (var i = 0; i != 23; ++i)
            {
                var nAttrpro = TbAttrPro.Attr[i];
                if (nAttrpro > 0)
                {
                    nTotleAttrPro += nAttrpro;
                    tempAttrPro[i] = nAttrpro;
                }
            }
            //属性值都在这里
            var tbEnchant = Table.GetEquipEnchant(tbEquip.NewRandomAttrValue);
            if (tbEnchant == null)
            {
                Logger.Error("Equip InitAddAttr Id={0} not find tbEquipEnchant Id={1}", tbEquip.Id,
                    tbEquip.NewRandomAttrValue);
                return;
            }
            //整理概率
            var AttrValue = new Dictionary<int, int>();

            for (var i = 0; i != addCount; ++i)
            {
                if (attrBlockList[i] == 1)
                {
                    var attrid = GetExdata(i + 6);
                    var index = GetAttrIndex(attrid);
                    var value = GetExdata(i + 12);
                    //AttrValue[index] = tbEnchant.Attr[index];
                    attrIdList[i] = attrid;
                    attrValueList[i] = value;
                    nTotleAttrPro -= tempAttrPro[index];
                    tempAttrPro.Remove(index);
                }
            }
            for (var i = 0; i != addCount; ++i)
            {
                if (attrBlockList[i] == 1)
                {
                    continue;
                }
                nRandom = MyRandom.Random(nTotleAttrPro);
                nTotleRandom = 0;
                foreach (var i1 in tempAttrPro)
                {
                    nTotleRandom += i1.Value;
                    if (nRandom < nTotleRandom)
                    {
                        //AddCount = i1.Key;
                        AttrValue[i1.Key] = tbEnchant.Attr[i1.Key];
                        nTotleAttrPro -= i1.Value;
                        tempAttrPro.Remove(i1.Key);
                        break;
                    }
                }
            }
            //int NowAttrCount = AttrValue.Count;
            //if (NowAttrCount < addCount)
            //{
            //    Logger.Error("Equip InitAddAttr AddAttr Not Enough AddCount={0},NowAttrCount={1}", addCount, NowAttrCount);
            //}

            var tbAttrRelate = Table.GetEquipRelate(tbEquip.NewRandomAttrInterval);
            if (tbAttrRelate == null)
            {
                Logger.Error("Equip tbAttrRelate Id={0} not find EquipRelate Id={1}", tbEquip.Id,
                    tbEquip.NewRandomAttrInterval);
                return;
            }

            for (var i = 0; i != addCount; ++i)
            {
                if (attrBlockList[i] == 1)
                {
                    continue;
                }
                var nKey = AttrValue.Keys.Min();
                var nAttrId = GetAttrId(nKey);
                if (nAttrId == -1)
                {
                    continue;
                }
                var fValue = tbEnchant.Attr[nKey];
                var AttrValueMin = fValue*tbEquip.NewRandomValueMin/100;
                var AttrValueMax = fValue*tbEquip.NewRandomValueMax/100;
                var AttrValueDiff = AttrValueMax - AttrValueMin;
                nRandom = MyRandom.Random(10000);
                nTotleRandom = 0;
                var rMin = 0;
                var rMax = 10000;
                for (var j = 0; j != tbAttrRelate.Value.Length; ++j)
                {
                    nTotleRandom += tbAttrRelate.Value[j];
                    if (nRandom < nTotleRandom)
                    {
                        switch (j)
                        {
                            case 0:
                            {
                                rMin = 0;
                                rMax = 5000;
                            }
                                break;
                            case 1:
                            {
                                rMin = 5000;
                                rMax = 7500;
                            }
                                break;
                            case 2:
                            {
                                rMin = 7500;
                                rMax = 9000;
                            }
                                break;
                            case 3:
                            {
                                rMin = 9000;
                                rMax = 10000;
                            }
                                break;
                        }
                        break;
                    }
                }
                var AttrValueMin2 = AttrValueMin + AttrValueDiff*rMin/10000;
                var AttrValueMax2 = AttrValueMin + AttrValueDiff*rMax/10000;
                var AttrRealValue = MyRandom.Random(AttrValueMin2, AttrValueMax2);

                // 小凡的需求，要把 AttrRealValue 在 (0, 1) 范围内的数值，变成1
                if (AttrRealValue >= 0 && AttrRealValue < 100)
                    AttrRealValue = 100;
                if (AttrRealValue > 100)
                {
                    var singleDigit = AttrRealValue % 10;
                    AttrRealValue = AttrRealValue - singleDigit;
                }
              
                AddAttr(i, nAttrId, AttrRealValue,false);
                attrIdList[i] = nAttrId;
                attrValueList[i] = AttrRealValue;
                AttrValue.Remove(nKey);
            }
             */
#endregion

            for (var i = 0; i != 6; ++i)
            {
                attrIdList.Add(-1);
                attrValueList.Add(0);
            }
            var tbEnchant = Table.GetEquipEnchant(tbEquip.NewRandomAttrValue);
            if (tbEnchant == null)
            {
                Logger.Error("Equip InitAddAttr Id={0} not find tbEquipEnchant Id={1}", tbEquip.Id,
                    tbEquip.NewRandomAttrValue);
                return;
            }
            var tbAttrRelate = Table.GetEquipRelate(tbEquip.NewRandomAttrInterval);
            if (tbAttrRelate == null)
            {
                Logger.Error("Equip tbAttrRelate Id={0} not find EquipRelate Id={1}", tbEquip.Id,
                    tbEquip.NewRandomAttrInterval);
                return;
            }
            int nRandom, nTotleRandom;
            for (var i = 0; i < 6; i++)
            {
                var nAttrId = GetExdata(i + 6);
                if (nAttrId == -1)
                {
                    continue;
                }
                if (attrBlockList[i] == 1)
                {
                    attrIdList[i] = nAttrId;
                    attrValueList[i] = GetExdata(i + 12);
                    continue;
                }

                var fValue = tbEnchant.Attr[GetAttrIndex(nAttrId)];
                var AttrValueMin = fValue * tbEquip.NewRandomValueMin / 100;
                var AttrValueMax = fValue * tbEquip.NewRandomValueMax / 100;
                var AttrValueDiff = AttrValueMax - AttrValueMin;
                nRandom = MyRandom.Random(10000);
                nTotleRandom = 0;
                var rMin = 0;
                var rMax = 10000;
                for (var j = 0; j != tbAttrRelate.Value.Length; ++j)
                {
                    nTotleRandom += tbAttrRelate.Value[j];
                    if (nRandom < nTotleRandom)
                    {
                        switch (j)
                        {
                            case 0:
                                {
                                    rMin = 0;
                                    rMax = 5000;
                                }
                                break;
                            case 1:
                                {
                                    rMin = 5000;
                                    rMax = 7500;
                                }
                                break;
                            case 2:
                                {
                                    rMin = 7500;
                                    rMax = 9000;
                                }
                                break;
                            case 3:
                                {
                                    rMin = 9000;
                                    rMax = 10000;
                                }
                                break;
                        }
                        break;
                    }
                }
                var AttrValueMin2 = AttrValueMin + AttrValueDiff * rMin / 10000;
                var AttrValueMax2 = AttrValueMin + AttrValueDiff * rMax / 10000;
                var AttrRealValue = MyRandom.Random(AttrValueMin2, AttrValueMax2);

                // 小凡的需求，要把 AttrRealValue 在 (0, 1) 范围内的数值，变成1
                if (AttrRealValue >= 0 && AttrRealValue < 100)
                    AttrRealValue = 100;
                if (AttrRealValue > 100)
                {
                    var singleDigit = AttrRealValue % 10;
                    AttrRealValue = AttrRealValue - singleDigit;
                }

                AddAttr(i, nAttrId, AttrRealValue, false);
                attrIdList[i] = nAttrId;
                attrValueList[i] = AttrRealValue;
            }
        }
        /// <summary>
        /// 处理以前的星级属性值，现在的值都统一扩大了100倍，数据最小是1%，所以小于100并且还有值说明是以前的数据，以前的数据最小为1最大不会超过100,新的值最小是100
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        void CheckOldAddAttriValueGetExdata(int index)  
        {
            var value=GetExdata(index);
            if (value >=1 && value < 10)
            {
                value *= 100;
                SetExdata(index, value);
            }
        }

        bool ContainAddAttrID(int attrID)
        {
            for (var i = 0; i < 6; i++)
            {
                var attrid = GetExdata(i + 6);
                if (attrid == attrID)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}