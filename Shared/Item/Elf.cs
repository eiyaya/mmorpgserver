#region using

using System.Collections.Generic;
using System.Linq;
using DataContract;
using DataTable;

#endregion

namespace Shared
{
    public class ElfItem : ItemBase
    {
        private static readonly Dictionary<int, int> AttrIdtoIndex = new Dictionary<int, int>
        {
            {105, 0},
            {17, 1},
            {21, 2},
            {22, 3},
            {23, 4},
            {24, 5},
            {106, 6},
            {111, 7},
            {113, 8},
            {114, 9},
            {119, 10},
            {120, 11}
        };

        private static readonly Dictionary<int, int> AttrIdtoRefIndex = new Dictionary<int, int>
        {
            //1,2,3,4,5,6,7,8,10,11,13,14,19,20
            {1, 0},
            {2, 1},
            {3, 2},
            {4, 3},
            {5, 4},
            {6, 5},
            {7, 6},
            {8, 7},
            {10, 8},
            {11, 9},
            {13, 10},
            {14, 11},
            {19, 12},
            {20, 13}
        };

        /*很重要的一个东西就是宠物的附加属性条数
         * 0、  等级
         * 1、  是否出战(0是下阵，1是上阵休息，2上阵战斗) 
         * 2、  紫色属性1ID
         * 3、  紫色属性2ID
         * 4、  紫色属性3ID
         * 5、  紫色属性4ID
         * 6、  紫色属性5ID
         * 7、  紫色属性6ID
         * 8、  紫色属性1值
         * 9、  紫色属性2值
         * 10、 紫色属性3值
         * 11、 紫色属性4值
         * 12、 紫色属性5值
         * 13、 紫色属性6值
         * 14、 冲星等级
         * 15、 buff
         * 16、 buff
         * 17、 buff
         * 18、 buff
         * 19、 buff
         * 20、 buff
         */
        //private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static int BattleRef = Table_Tamplet.Convert_Int(Table.GetServerConfig(101).Value);
        //索引属性修正
        private static readonly List<int> IndextoAttrId = new List<int>
        {
            105,
            17,
            21,
            22,
            23,
            24,
            106,
            111,
            113,
            114,
            119,
            120
        };

        public static int NoBattleRef = Table_Tamplet.Convert_Int(Table.GetServerConfig(102).Value);
        //表格索引

        //初始属性修正
        private static readonly List<int> RefIndextoAttrId = new List<int>
        {
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            10,
            11,
            13,
            14,
            19,
            20
        };

        public ElfItem()
        {
        }

        public ElfItem(int nId, ItemBaseData Dbdata)
        {
            Init(nId, Dbdata);
        }

        public ElfItem(ItemBaseData Dbdata, bool IsNull = true)
        {
            mDbData = Dbdata;
            if (IsNull)
            {
                SetId(-1);
                SetCount(0);
            }
            else
            {
                for (var i = mDbData.Exdata.Count; i < (int)ElfExdataDefine.Count; ++i)
                {
                    AddExdata(-1);
                }
            }
        }

        //增加附加属性
        private void AddAttr(int nIndex, int nAttrId, int nAttrValue)
        {
            SetExdata(nIndex + 2, nAttrId);
            SetExdata(nIndex + 8, nAttrValue);
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
            var fightPoint = GetFightPoint(talentData, talentDataRef, characterLevel);
            return fightPoint;
        }

        public static int GetAttrId(int index)
        {
            if (index > IndextoAttrId.Count || index < 0)
            {
                Logger.Error("Elf GetAttrId index={0}", index);
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
            Logger.Error("Elf GetAttrIndex attrId={0}", attrId);
            return -1;
        }

        public int GetStarLevel()
        {
            return GetExdata((int) ElfExdataDefine.StarLevel);
        }

        public void SetStarLevel(int level)
        {
            var lastLevel = GetStarLevel();
            if (lastLevel == level)
                return;

            SetExdata((int)ElfExdataDefine.StarLevel, level);

            RefreshActiveAttr();
            MarkDirty();
        }

        //获得属性列表
        public void GetAttrList(Dictionary<int, int> AttrList, ElfRecord tbElf, int characterLevel, int baseValueBili)
        {
            var elfLevel = GetExdata(0);
            var isBattle = GetIndex(); //GetExdata(1);
            //基础属性
            for (var i = 0; i < tbElf.ElfInitProp.Length; i++)
            {
                var attrId = tbElf.ElfInitProp[i];
                if (attrId == -1)
                {
                    break;
                }
                long attrValue = tbElf.ElfProp[i];
                if (elfLevel > 1)
                {
                    var upvalue = tbElf.GrowAddValue[i];
                    attrValue += upvalue*(elfLevel - 1);
                    //int attrindex = GetAttrIndex(attrId);
                    //if (attrindex != -1)
                    //{
                    //    attrValue += tbLevelRef.PropPercent[attrindex] * (elfLevel - 1);
                    //}
                    //else
                    //{
                    //    attrindex = GetBaseAttrIndex(attrId);
                    //    if (attrindex != -1)
                    //    {
                    //        attrValue += tbLevelRef.Attr[attrindex] * (elfLevel - 1);
                    //    }
                    //    else
                    //    {
                    //        Logger.Warn("elf GetAttrList not find attr id={0},attrId={1}", tbElf.Id, attrId);
                    //    }
                    //}
                }
                if (isBattle == 0)
                {
                    var v = (int)(attrValue*BattleRef/10000*baseValueBili/10000);
                    AttrList.modifyValue(attrId, v);
                }
                else
                {
                    var v = (int)(attrValue * NoBattleRef / 10000 * baseValueBili / 10000);
                    AttrList.modifyValue(attrId, v);
                }
            }
            //随机属性
            //if (isBattle == 0)    // 上阵的都有天赋属性
            {
                for (var i = 2; i < 8; ++i)
                {
                    var attrId = GetExdata(i);
                    if (attrId == -1)
                    {
                        continue;
                    }
                    var attrValue = GetExdata(i + 6);
                    AttrList.modifyValue(attrId, attrValue);
                }
            }
        }

        public static int GetBaseAttrId(int index)
        {
            if (index > RefIndextoAttrId.Count || index < 0)
            {
                Logger.Error("Elf GetBaseAttrId index={0}", index);
                return -1;
            }
            return RefIndextoAttrId[index];
        }

        private static int GetBaseAttrIndex(int attrId)
        {
            int index;
            if (AttrIdtoRefIndex.TryGetValue(attrId, out index))
            {
                return index;
            }
            Logger.Error("Elf GetBaseAttrIndex attrId={0}", attrId);
            return -1;
        }

        //获取精灵战斗力
        public int GetFightPoint(int characterLevel, int attackType, int baseValueBili)
        {
            var value = 0;
            var attrlist = new Dictionary<int, int>();
            GetAttrList(attrlist, Table.GetElf(GetId()), characterLevel, baseValueBili);
            value = GetAttrFightPoint(attrlist, characterLevel, attackType);
            value += GetSkillFightPoint();
            return value;
        }

        public int GetSkillFightPoint()
        {
            var value = 0;
            for (var i = 0; i < 3; ++i)
            {
                var buffId = GetBuffId(i);
                var buffLevel = GetBuffLevel(i);
                value += GetOneBuffFightPoint(buffId, buffLevel);
            }

            return value;
        }
        public static int GetOneBuffFightPoint(int buffId, int buffLevel)
        {
            if (buffId < 0)
                return 0;

            var tbBuff = Table.GetBuff(buffId);
            if (tbBuff == null)
                return 0;

            var fightPoint = tbBuff.FightPoint;
            if (fightPoint == -1)
                return 0;

            var tb = Table.GetSkillUpgrading(fightPoint);
            if (tb == null)
            {
                Logger.Error("Elf GetOneSkillFightPoint skillupgrading is null  skillID={0}", buffId);
                return 0;
            }

            var value = tb.GetSkillUpgradingValue(buffLevel - 1);
            if (value < 0)
            {
                Logger.Error("Elf GetOneSkillFightPoint skillupgrading is null  buffId={0} buffLevel={1}", buffId, buffLevel);
                return 0;
            }
            return value;
        }

        private int GetFightPoint(int[] attr, int[] attrRef, int level)
        {
            //var level = GetLevel();
            var tbLevel = Table.GetLevelData(level);
            if (tbLevel == null)
            {
                return 0;
            }
            var FightPoint = 0L;
            for (var type = eAttributeType.PhyPowerMin; type != eAttributeType.HitRecovery; ++type)
            {
                //基础固定属性
                long nValue = attr[(int) type];
                switch ((int) type)
                {
                    case 15:
                    {
                        FightPoint += nValue*tbLevel.LuckyProFightPoint/10000;
                    }
                        break;
                    case 17:
                    {
                        FightPoint += nValue*tbLevel.ExcellentProFightPoint/10000;
                    }
                        break;
                    case 21:
                    {
                        FightPoint += nValue*tbLevel.DamageAddProFightPoint/10000;
                    }
                        break;
                    case 22:
                    {
                        FightPoint += nValue*tbLevel.DamageResProFightPoint/10000;
                    }
                        break;
                    case 23:
                    {
                        FightPoint += nValue*tbLevel.DamageReboundProFightPoint/10000;
                    }
                        break;
                    case 24:
                    {
                        FightPoint += nValue*tbLevel.IgnoreArmorProFightPoint/10000;
                    }
                        break;
                    default:
                    {
                        var tbState = Table.GetStats((int) type);
                        if (tbState == null)
                        {
                            continue;
                        }
                        FightPoint += tbState.PetFight*nValue/100;
                    }
                        break;
                }
            }

            //百分比计算
            FightPoint += attrRef[(int) eAttributeType.MagPowerMin]*tbLevel.PowerFightPoint/10000/100;
            FightPoint += attrRef[(int) eAttributeType.PhyArmor]*tbLevel.ArmorFightPoint/10000/100;
            FightPoint += attrRef[(int) eAttributeType.HpMax]*tbLevel.HpFightPoint/10000/100;
            FightPoint += attrRef[(int) eAttributeType.MpMax]*tbLevel.MpFightPoint/10000/100;
            FightPoint += attrRef[(int) eAttributeType.Hit]*tbLevel.HitFightPoint/10000/100;
            FightPoint += attrRef[(int) eAttributeType.Dodge]*tbLevel.DodgeFightPoint/10000/100;
            return (int)FightPoint;
        }

        //初始化数据
        private void Init(int nId, ItemBaseData Dbdata)
        {
            mDbData = Dbdata;
            SetId(nId);
            SetCount(1);
            CleanExdata();
            var tbElf = Table.GetElf(nId);
            if (tbElf == null)
            {
                Logger.Error("ElfId  Id={0} not find", nId);
                return;
            }
            //初始等级
            AddExdata(1);
            //是否出战
            AddExdata(0);
            //随机附加属性
            for (var i = 0; i != 12; ++i)
            {
                AddExdata(-1);
            }

            AddExdata(0); // 14 StarLevel
            RefreshActiveAttr();

            for (var e = ElfExdataDefine.BuffId1; e <= ElfExdataDefine.BuffLevel3; ++e)
            {
                AddExdata(-1);
            }

            var buffId = RandBuffId(tbElf.BuffGroupId);
            var index = SetBuffId(buffId);
            SetBuffLevel(index, 1);
        }

        private void RefreshActiveAttr()
        {
            var tbElf = Table.GetElf(GetId());
            if (tbElf == null)
                return;

            //var addCount = RandomAddCount(tbElf, tbElf.RandomPropCount);
            //InitAddAttr(tbElf, addCount);
            var starLevel = GetStarLevel();
            for (var i = 0; i < tbElf.StarAttrId.Length; i++)
            {
                if (i > 5)
                {
                    Logger.Error("Elf Max Attribut Count is 6");
                    break;
                }

                var attrId = tbElf.StarAttrId[i];
                var attrValue = tbElf.StarAttrValue[i];

                if (GetExdata(2 + i) != attrId)
                {
                    if (attrId >= 0 && i < starLevel)
                    {
                        SetExdata(2 + i, attrId);
                        SetExdata(8 + i, attrValue);
                    }
                    else
                    {
                        SetExdata(2 + i, -1);
                    }
                }
            }
        }

        //初始化附加属性
        public void InitAddAttr(ElfRecord tbElf, int addCount)
        {
            if (addCount <= 0 || addCount > 6)
            {
                return;
            }
            int nRandom, nTotleRandom;
            var TbAttrPro = Table.GetEquipEnchantChance(tbElf.RandomPropPro);
            if (TbAttrPro == null)
            {
                Logger.Error("Equip InitAddAttr Id={0} not find EquipEnchantChance Id={1}", tbElf.Id,
                    tbElf.RandomPropPro);
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
            var tbEnchant = Table.GetEquipEnchant(tbElf.RandomPropValue);
            if (tbEnchant == null)
            {
                Logger.Error("Equip InitAddAttr Id={0} not find tbEquipEnchant Id={1}", tbElf.Id, tbElf.RandomPropValue);
                return;
            }
            //整理概率
            var AttrValue = new Dictionary<int, int>();
            for (var i = 0; i != addCount; ++i)
            {
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
            var NowAttrCount = AttrValue.Count;
            if (NowAttrCount < addCount)
            {
                Logger.Error("Equip InitAddAttr AddAttr Not Enough AddCount={0},NowAttrCount={1}", addCount,
                    NowAttrCount);
            }

            //EquipRelateRecord tbAttrRelate = Table.GetEquipRelate(tbElf.RandomAttrInterval);
            //if (tbAttrRelate == null)
            //{
            //    Logger.Error("Equip tbAttrRelate Id={0} not find EquipRelate Id={1}", tbElf.Id, tbElf.RandomAttrInterval);
            //    return;
            //}

            for (var i = 0; i != NowAttrCount; ++i)
            {
                var nKey = AttrValue.Keys.Min();
                var nAttrId = ItemEquip2.GetAttrId(nKey);
                if (nAttrId == -1)
                {
                    continue;
                }
                var fValue = tbEnchant.Attr[nKey];
                //int AttrValueMin = fValue * tbElf.RandomValueMin / 100;
                //int AttrValueMax = fValue * tbElf.RandomValueMax / 100;
                //int AttrValueDiff = AttrValueMax - AttrValueMin;
                //nRandom = MyRandom.Random(10000);
                //nTotleRandom = 0;
                //int rMin = 0;
                //int rMax = 10000;
                //for (int j = 0; j != tbAttrRelate.Value.Length; ++j)
                //{
                //    nTotleRandom += tbAttrRelate.Value[j];
                //    if (nRandom < nTotleRandom)
                //    {
                //        switch (j)
                //        {
                //            case 0:
                //                {
                //                    rMin = 0;
                //                    rMax = 5000;
                //                }
                //                break;
                //            case 1:
                //                {
                //                    rMin = 5000;
                //                    rMax = 7500;
                //                }
                //                break;
                //            case 2:
                //                {
                //                    rMin = 7500;
                //                    rMax = 9000;
                //                }
                //                break;
                //            case 3:
                //                {
                //                    rMin = 9000;
                //                    rMax = 10000;
                //                }
                //                break;
                //        }
                //        break;
                //    }
                //}
                //int AttrValueMin2 = AttrValueMin + AttrValueDiff * rMin / 10000;
                //int AttrValueMax2 = AttrValueMin + AttrValueDiff * rMax / 10000;
                //int AttrRealValue = MyRandom.Random(AttrValueMin2, AttrValueMax2);
                AddAttr(i, nAttrId, fValue);
                AttrValue.Remove(nKey);
            }
        }

        //随机属性条数随机
        private int RandomAddCount(ElfRecord tbElf, int EquipRelateId)
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
                    return i;
                    //if (i == 0) return 0;
                    //AddCount = i;
                    //break;
                }
            }
            Logger.Error("Elf RandomAddCount not find Pro! EquipRelateId={0}", EquipRelateId);
            return AddCount;
        }

        //获得经验
        public void SetLevel(int value)
        {
            SetExdata(0, value);
            MarkDirty();
        }

        #region buff

        public void FillAllBuff(Dictionary<int, int> buffs)
        {
            for (var i = 0; i < 3; ++i)
            {
                var buffId = GetBuffId(i);
                if (buffId >= 0)
                {
                    int lastLevel;
                    var buffLevel = GetBuffLevel(i);
                    if (buffs.TryGetValue(buffId, out lastLevel))
                    {
                        if (buffLevel > lastLevel)
                        {
                            buffs[buffId] = buffLevel;
                        }
                    }
                    else
                    {
                        buffs[buffId] = buffLevel;
                    }
                }
            }
        }

        public override int GetBuffId(int index)
        {
            var idx = GetBuffIdDefine(index);
            return GetExdata(idx);
        }

        public override int GetBuffLevel(int index)
        {
            var idx = GetBuffLevelDefine(index);
            return GetExdata(idx);
        }

        public int SetBuffId(int buffId)
        {
            var tbBuff = Table.GetBuff(buffId);
            if (tbBuff == null)
                return -1;
            var buffType = tbBuff.SkillType;
            var exdefine = GetBuffIdDefine(buffType);
            SetExdata(exdefine, buffId);
            MarkDirty();

            return buffType;
        }

        public int ReplaceBuff(int buffGroupId, int posType)
        {
            var buffId = RandBuffId(buffGroupId);
            var tbBuff = Table.GetBuff(buffId);
            if (tbBuff == null)
                return -1;
            var skillType = tbBuff.SkillType;
            if (skillType != posType)
            {
                return -1;
            }

            SetBuffId(buffId);
            return buffId;
        }

        public void SetBuffLevel(int index, int level)
        {
            var exdefine = GetBuffLevelDefine(index);
            SetExdata(exdefine, level);
            MarkDirty();
        }

        private int GetBuffIdDefine(int index)
        {
            var exIdx = -1;
            if (index == 0)
                exIdx = (int)ElfExdataDefine.BuffId1;
            else if (index == 1)
                exIdx = (int)ElfExdataDefine.BuffId2;
            else if (index == 2)
                exIdx = (int)ElfExdataDefine.BuffId3;

            return exIdx;
        }

        private int GetBuffLevelDefine(int index)
        {
            var exIdx = 0;
            if (index == 0)
                exIdx = (int)ElfExdataDefine.BuffLevel1;
            else if (index == 1)
                exIdx = (int)ElfExdataDefine.BuffLevel2;
            else if (index == 2)
                exIdx = (int)ElfExdataDefine.BuffLevel3;

            return exIdx;
        }
        #endregion
    }
}