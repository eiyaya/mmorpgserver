#region using

using System;
using System.Collections.Generic;
using DataContract;
using DataTable;

#endregion

namespace Shared
{
    public class PetItem : ItemBase, LevelExp
    {
        //计算宠物战斗力
        private static readonly Dictionary<int, int> Ref = new Dictionary<int, int>();
        /*很重要的一个东西就是宠物的附加属性条数
         * 0、宠物等级
         * 1、宠物经验
         * 2、宠物星级
         * 3、宠物状态
         * 4、 碎片数量
         * 5、 特长1
         * 6、 特长2 
         * 7、 特长3
         * 8、 
         * 9、 
         * 10、
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
        public PetItem()
        {
        }

        public PetItem(int nId, ItemBaseData Dbdata)
        {
            Init(nId, Dbdata);
        }

        public PetItem(ItemBaseData Dbdata, bool IsNull = true)
        {
            mDbData = Dbdata;
            if (IsNull)
            {
                SetId(-1);
                SetCount(0);
            }
        }

        private readonly Dictionary<int, int> AttrRef_IdtoRefIndex = new Dictionary<int, int>
        {
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

        //增加碎片
        public void AddPiece(int count)
        {
            SetExdata(4, GetExdata(4) + count);
            MarkDirty();
        }

        public int CalculatePetFightPower(int level)
        {
            var power = 0;
            Ref.Clear();
            Ref.Add((int) eAttributeType.PhyPowerMin, GetPetAttribut(eAttributeType.PhyPowerMin));
            Ref.Add((int) eAttributeType.PhyPowerMax, GetPetAttribut(eAttributeType.PhyPowerMax));
            Ref.Add((int) eAttributeType.MagPowerMin, GetPetAttribut(eAttributeType.MagPowerMin));
            Ref.Add((int) eAttributeType.MagPowerMax, GetPetAttribut(eAttributeType.MagPowerMax));
            Ref.Add((int) eAttributeType.PhyArmor, GetPetAttribut(eAttributeType.PhyArmor));
            Ref.Add((int) eAttributeType.MagArmor, GetPetAttribut(eAttributeType.MagArmor));
            Ref.Add((int) eAttributeType.HpMax, GetPetAttribut(eAttributeType.HpMax));
            foreach (var pair in Ref)
            {
                var type = pair.Key;
                //基础固定属性
                var nValue = pair.Value; //随从的直接用算好的属性
                var tbState = Table.GetStats(type);
                if (tbState == null)
                {
                    continue;
                }
                power += tbState.PetFight*nValue/100;
            }
            return Math.Max(0, power);
        }

        //循环所有技能
        public void ForeachSkill(Func<PetSkillRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Actor act is null");
                return;
            }
            var tbPet = Table.GetPet(GetId());
            var ladder = GetExdata(2);
            for (var i = 0; i < tbPet.Skill.Length; i++)
            {
                if (ladder >= tbPet.ActiveLadder[i] && tbPet.Skill[i] != -1)
                {
                    try
                    {
                        if (!act(Table.GetPetSkill(tbPet.Skill[i])))
                        {
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }

        //循环所有特长
        public void ForeachSpecial(Func<PetSkillRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Actor act is null");
                return;
            }
            var tbPet = Table.GetPet(GetId());
            var level = GetExdata(0);
            for (var i = 0; i < 3; ++i)
            {
                if (level >= tbPet.Speciality[i])
                {
                    var sId = GetExdata(i + 5);
                    if (sId == -1)
                    {
                        continue;
                    }
                    try
                    {
                        if (!act(Table.GetPetSkill(sId)))
                        {
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            //for (int i = 0; i < tbPet.Skill.Length; i++)
            //{
            //    if (ladder >= tbPet.ActiveLadder[i] && tbPet.Skill[i] != -1)
            //    {
            //        try
            //        {
            //            if (!act(Table.GetPetSkill(tbPet.Skill[i])))
            //                break;
            //        }
            //        catch (Exception)
            //        {
            //            throw;
            //        }
            //    }
            //}
        }

        private int GetAttrRef_Id2Index(int attrId)
        {
            int index;
            if (AttrRef_IdtoRefIndex.TryGetValue(attrId, out index))
            {
                return index;
            }
            return -1;
        }

        public int GetFightPoint()
        {
            return CalculatePetFightPower(GetLevel());
        }

        //获取等级
        public int GetLevel()
        {
            return Level;
        }

        public int GetPetAttribut(eAttributeType attrId)
        {
            var tbPet = Table.GetPet(GetId());
            if (tbPet == null)
            {
                return -1;
            }
            var tbCharacter = Table.GetCharacterBase(tbPet.CharacterID);
            if (tbCharacter == null)
            {
                return -1;
            }
            if (attrId < eAttributeType.Level || attrId > eAttributeType.HitRecovery)
            {
                return -1;
            }
            var value = tbCharacter.Attr[(int) attrId];
            //var tbAttrRef = Table.GetAttrRef(tbPet.AttrRef);
            //if (tbAttrRef == null) return value;
            //int index = GetAttrRef_Id2Index((int)attrId);
            //if (index < 0) return value;
            var level = GetExdata(0);
            //value += tbAttrRef.Attr[index] * (level - 1);
            for (var i = 1; i != 4; ++i)
            {
                var skillId = tbCharacter.InitSkill[i];
                var tbSkil = Table.GetSkill(skillId);
                if (tbSkil == null)
                {
                    continue;
                }
                if (tbSkil.CastType != 3)
                {
                    continue;
                }
                var tbBuff = Table.GetBuff(tbSkil.CastParam[0]);
                if (tbBuff == null)
                {
                    continue;
                }
                for (var j = 0; j < tbBuff.effectid.Length; j++)
                {
                    var effectId = tbBuff.effectid[j];
                    if (effectId != 2)
                    {
                        continue;
                    }
                    if (tbBuff.effectparam[j, 0] != (int) attrId)
                    {
                        continue;
                    }
                    var skillUp = Table.GetSkillUpgrading(tbBuff.effectparam[j, 3] - 10000000);
                    if (skillUp == null)
                    {
                        continue;
                    }
                    value += skillUp.GetSkillUpgradingValue(level - 1);
                }
            }
            return value;
        }

        //获得碎片数量
        public int GetPiece()
        {
            return GetExdata(4);
        }

        public int GetState()
        {
            return GetExdata(3);
        }

        //获取当前最大需求经验

        public int GetTotleNeedExp()
        {
            var skillup = Table.GetSkillUpgrading(Table.GetPet(GetId()).NeedExp);
            var oldExp = GetExdata(1);
            var level = GetExdata(0);
            var need = 0;
            for (var i = level; i < GetMaxLevel(); i++)
            {
                if (i == level)
                {
                    need = skillup.GetSkillUpgradingValue(i) - oldExp;
                }
                else
                {
                    need += skillup.GetSkillUpgradingValue(i);
                }
            }
            return need;
        }

        //初始化数据
        private void Init(int nId, ItemBaseData Dbdata)
        {
            mDbData = Dbdata;
            SetId(nId);
            SetCount(1);
            CleanExdata();
            var tbPet = Table.GetPet(nId);
            if (tbPet == null)
            {
                Logger.Error("PetId  Id={0} not find", nId);
                return;
            }

            //初始等级
            AddExdata(1);
            //初始经验
            AddExdata(0);
            //初始星级
            AddExdata(tbPet.Ladder);
            //初始状态
            AddExdata((int) PetStateType.Idle);
            //碎片数量
            AddExdata(0);
            //特长
            for (var i = 0; i != 3; i++)
            {
                AddExdata(-1);
            }
            //随机特长
            RefreshSpeciality(this, tbPet);
        }

        //获得经验
        public void PetAddExp(int value)
        {
            //var skillup = Table.GetSkillUpgrading(Table.GetPet(GetId()).NeedExp);
            //int oldExp = GetExdata(1);
            //int level = GetExdata(0);
            //int needexp = skillup.GetSkillUpgradingValue(level);
            //if (level == GetMaxLevel())
            //{
            //    if (oldExp == needexp)
            //    {
            //        return;
            //    }
            //    else
            //    {
            //        int exp2 = oldExp + value;
            //        if (exp2 > needexp)
            //        {
            //            exp2 = needexp;
            //        }
            //        SetExdata(1, exp2);
            //        MarkDirty();
            //    }
            //}
            //int exp = oldExp + value;
            //while (exp >= needexp)
            //{
            //    level++;
            //    exp -= needexp;
            //    if (level == GetMaxLevel())
            //    {
            //        if (exp > needexp)
            //        {
            //            exp = needexp;
            //        }
            //        break;
            //    }
            //    needexp = skillup.GetSkillUpgradingValue(level);

            //}
            //SetExdata(0, level);
            //SetExdata(1, exp);
            this.AddExp(value);
            MarkDirty();
        }

        //随机宠物特长
        public static void RefreshSpeciality(ItemBase item, PetRecord tbPet)
        {
            var skills = new Dictionary<int, int>();
            for (var index = 0; index != 3; ++index)
            {
                var tbPetSkill = Table.GetPetSkillBase(tbPet.SpecialityLibrary[index]);
                if (tbPetSkill == null)
                {
                    return;
                }
                var needCount = 1;
                //if (tbPet.Speciality[index] > 1)
                //{
                //    continue;
                //}
                //for (int i = 0; i != 3; i++)
                //{
                //    if (tbPet.Speciality[i] <= 1)
                //    {
                //        needCount++;
                //    }
                //}
                skills.Clear();
                var nTotleAttrPro = 0;
                for (var i = 0; i < tbPetSkill.SpecialityPro.Length; i++)
                {
                    if (tbPetSkill.SpecialityPro[i] < 0 || tbPetSkill.Speciality[i] < 0)
                    {
                        break;
                    }
                    skills.Add(tbPetSkill.Speciality[i], tbPetSkill.SpecialityPro[i]);
                    nTotleAttrPro += tbPetSkill.SpecialityPro[i];
                }
                //整理概率
                int nRandom, nTotleRandom;
                for (var i = 0; i != needCount; ++i)
                {
                    nRandom = MyRandom.Random(nTotleAttrPro);
                    nTotleRandom = 0;
                    foreach (var i1 in skills)
                    {
                        nTotleRandom += i1.Value;
                        if (nRandom < nTotleRandom)
                        {
                            item.SetExdata(5 + index, i1.Key);
                            nTotleAttrPro -= i1.Value;
                            skills.Remove(i1.Key);
                            break;
                        }
                    }
                }
            }
        }

        //状态相关
        public void SetState(PetStateType pst)
        {
            SetExdata(3, (int) pst);
            MarkDirty();
        }

        public int GetMaxLevel()
        {
            return 50;
        }

        public int GetNeedExp(int lvl)
        {
            var skillup = Table.GetSkillUpgrading(Table.GetPet(GetId()).NeedExp);
            return skillup.GetSkillUpgradingValue(lvl);
        }

        public int Exp
        {
            get { return GetExdata(1); }
            set { SetExdata(1, value); }
        }

        public int Level
        {
            get { return GetExdata(0); }
            set { SetExdata(0, value); }
        }
    }
}