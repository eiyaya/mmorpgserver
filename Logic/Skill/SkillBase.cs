#region using

using System;
using System.Collections.Generic;
using DataContract;
using DataTable;
using EventSystem;
using NLog;
using Shared;

#endregion

namespace Logic
{
    public interface ISkillData
    {
        ErrorCodes EquipSkills(SkillData _this, List<int> skills);
        void ForgetSkill(SkillData _this, int nId);
        int GetSkillLevel(SkillData _this, int nId);
        DBSkill InitByBase(SkillData _this, CharacterController character, int characterTableId);
        void InitByDB(SkillData _this, CharacterController character, DBSkill skillData);
        void LearnSkill(SkillData _this, int nId, int nLevel);
        bool HaveSkill(SkillData _this, int nId);
        void LevelUpSkill(SkillData _this, int nId, int nLevel = 0);
        ErrorCodes UpgradeSkill(SkillData _this, int nId, ref int result);

		int GetSkillTotalLevel(SkillData _this);
    }

    public class SkillDataDefaultImpl : ISkillData
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region  初始化

        //用第一次创建
        public DBSkill InitByBase(SkillData _this, CharacterController character, int characterTableId)
        {
            _this.mCharacter = character;
            var dbData = new DBSkill();
            _this.mDbData = dbData;
            var tbcr = Table.GetCharacterBase(characterTableId);
            foreach (var i in tbcr.InitSkill)
            {
                if (i < 0)
                {
                    continue;
                }
                var tbskill = Table.GetSkill(i);
                if (tbskill.Type == 0) //普攻
                {
                    _this.mDbData.Skills[i] = 1;
                }
                else if (tbskill.Type == 1) //技能
                {
                    _this.mDbData.Skills[i] = 0;
                }
                else if (tbskill.Type == 3) //xp技能
                {
                    _this.mDbData.Skills[i] = 0;
                }
                else
                {
                    Logger.Warn("Skill InitByBase Type is Error!! ID={0}", i);
                }
            }

            //被动技能
            {
                //if (!string.IsNullOrEmpty(tbcr.PassiveSkillGroup))
                //{
                //    var passiveSkillGroup = tbcr.PassiveSkillGroup.Split('|');
                //    foreach (var passive in passiveSkillGroup)
                //    {
                //        var skillId = -1;
                //        if (int.TryParse(passive, out skillId))
                //        {
                //            if (skillId < 0)
                //            {
                //                continue;
                //            }
                //            var tbSkill = Table.GetSkill(skillId);
                //            if (tbSkill != null)
                //            {
                //                _this.mDbData.Skills[skillId] = 0;                           
                //            }                            
                //        }
                //        else
                //        {
                //            Logger.Warn("Skill InitByBase Type is Error!! ID={0}", skillId);
                //        }
                //    }
                //}
            }


            var maxEquipCount = Table.GetServerConfig(700).ToInt();


            for (var i = 0; i < maxEquipCount; i++)
            {
                _this.mDbData.EquipSkills.Add(-1);
                //_this.mDbData.EquipSkills_Passive.Add(-1);
            }
            

            //switch (characterTableId)
            //{
            //    case 0:
            //    {
            //        mDbData.EquipSkills.Add(4);
            //        mDbData.EquipSkills.Add(5);
            //        mDbData.EquipSkills.Add(6);
            //        mDbData.EquipSkills.Add(8);
            //    }
            //    break;
            //    case 1:
            //    {
            //        mDbData.EquipSkills.Add(105);
            //        mDbData.EquipSkills.Add(106);
            //        mDbData.EquipSkills.Add(108);
            //        mDbData.EquipSkills.Add(110);
            //    }
            //    break;
            //    case 2:
            //    {
            //        mDbData.EquipSkills.Add(204);
            //        mDbData.EquipSkills.Add(206);
            //        mDbData.EquipSkills.Add(205);
            //        mDbData.EquipSkills.Add(208);
            //    }
            //    break;
            //}

            _this.mFlag = true;
            _this.MarkDirty();
            return dbData;
        }

        //用数据库数据
        public void InitByDB(SkillData _this, CharacterController character, DBSkill skillData)
        {
            _this.mCharacter = character;
            _this.mDbData = skillData;
            var maxEquipCount = Table.GetServerConfig(700).ToInt();

            if (skillData.EquipSkills.Count < maxEquipCount)
            {
                for (var i = skillData.EquipSkills.Count; i != maxEquipCount; ++i)
                {
                    skillData.EquipSkills.Add(-1);
                    //skillData.EquipSkills_Passive.Add(-1);
                }
            }
            //if (Skill == null)
            //{
            //    Skill = new Dictionary<int, int>();
            //}
            //foreach (var i in skillData)
            //{
            //    int value = 0;
            //    if (!Skill.TryGetValue(i.Key, out value))
            //    {
            //        Skill.Add(i.Key, i.Value);
            //    }
            //    else
            //    {
            //        Skill[i.Key] = i.Value;
            //    }
            //}
            //mFlag = false;
            //Dirty = false;
        }

        #endregion

        #region  技能相关

        //装备技能
        public ErrorCodes EquipSkills(SkillData _this, List<int> skills)
        {
            var index = 0;
            var OldSkills = new Int32Array();
            var TempEquipSkills = OldSkills.Items;
            TempEquipSkills.AddRange(_this.mDbData.EquipSkills);
            var maxEquipCount = Table.GetServerConfig(700).ToInt();
            foreach (var i in skills)
            {
                if (i != -1)
                {
                    if (GetSkillLevel(_this, i) == 0)
                    {
                        return ErrorCodes.Error_NotHaveSkill;
                    }
                }
                if (index > maxEquipCount)
                {
                    return ErrorCodes.Error_DataOverflow;
                }
                if (i != -1 && skills.GetValueCount(i) > 1)
                {
                    Logger.Error("EquipSkills ={0}", skills.GetDataString());
                    return ErrorCodes.Unknow;
                }
                index++;
            }
            index = 0;
            foreach (var i in skills)
            {
                _this.mDbData.EquipSkills[index] = i;
                index++;
            }
            //news
            var newSkills = new Int32Array();
            var skillLevels = new Int32Array();
            foreach (var i in _this.mDbData.EquipSkills)
            {
                newSkills.Items.Add(i);
                skillLevels.Items.Add(GetSkillLevel(_this, i));
            }
            _this.mCharacter.EquipSkillChange(OldSkills, newSkills, skillLevels);
            //foreach (int skillId in TempEquipSkills)
            //{
            //    if(skillId==-1) continue;
            //    if (!mDbData.EquipSkills.Contains(skillId))
            //    {
            //        mCharacter.SkillChange(0, skillId, 0);
            //    }
            //}
            //foreach (int skillId in mDbData.EquipSkills)
            //{
            //    if (!TempEquipSkills.Contains(skillId))
            //    {
            //        mCharacter.SkillChange(1, skillId, GetSkillLevel(skillId));
            //    }
            //}
            _this.MarkDirty();
            return ErrorCodes.OK;
        }

        public bool HaveSkill(SkillData _this, int nId)
        {
            if (nId < 0)
            {
                return false;                
            }

            int nOldLevel;
            if (_this.mDbData.Skills.TryGetValue(nId, out nOldLevel))
            {
                if (nOldLevel != 0)
                {
                    return true;
                }
            }

            return false;
        }

        //学习技能
        public void LearnSkill(SkillData _this, int nId, int nLevel)
        {
            if (nId < 0)
                return;
            //PlayerLog.WriteLog(_this.mCharacter.mGuid, "----------LearnSkill----------skillId={0}:Lv{1}", nId, nLevel);
            int nOldLevel;
            if (_this.mDbData.Skills.TryGetValue(nId, out nOldLevel))
            {
                Logger.Warn("LearnSkill={0} is haved! OldLevel={1},NewLevel={2}", nId, nOldLevel, nLevel);
            }
            _this.mDbData.Skills[nId] = nLevel;
            _this.mFlag = true;
            _this.MarkDirty();
            Logger.Info("LearnSkill={0} is Success! Level={1}", nId, nLevel);
            _this.mCharacter.SkillChange(1, nId, nLevel);
            _this.mCharacter.AddExData((int) eExdataDefine.e332, 1);
            var e = new UpgradeSkillEvent(_this.mCharacter, nId);
            EventDispatcher.Instance.DispatchEvent(e);

            var tbSkill = Table.GetSkill(nId);
            if (tbSkill == null||tbSkill.Type == 2) /* ||tbSkill.Type == 2*/
                return;

            var HaveFree = false;
            var list = new List<int>();
            foreach (var i in _this.mDbData.EquipSkills)
            {
                if (i == -1 && !HaveFree)
                {
                    list.Add(nId);
                    HaveFree = true;
                }
                else
                {
                    list.Add(i);
                }
            }
            if (HaveFree)
            {
                if (nId == 30 || nId == 231 || nId == 133)
                {
                    if (list.Contains(nId))
                    {
                        list.Remove(nId);
                        list.Add(nId);
                    }
                } 
                EquipSkills(_this, list);
            }
        }

        //是否是被动技能
        //public bool IsPassiveSkill(int charId , int Id)
        //{
        //    var tbcr = Table.GetCharacterBase(charId);
        //    if (!string.IsNullOrEmpty(tbcr.PassiveSkillGroup))
        //    {
        //        var passiveSkillGroup = tbcr.PassiveSkillGroup.Split('|');
        //        foreach (var passive in passiveSkillGroup)
        //        {
        //            var skillId = -1;
        //            if (int.TryParse(passive, out skillId))
        //            {
        //                if (Id == skillId)
        //                {
        //                    return true;                            
        //                }
        //            }
        //        }
        //    }
        //    return false;
        //}

        //遗忘技能
        public void ForgetSkill(SkillData _this, int nId)
        {
            PlayerLog.WriteLog(_this.mCharacter.mGuid, "----------ForgetSkill----------skillId={0}", nId);
            int nOldLevel;
            if (!_this.mDbData.Skills.TryGetValue(nId, out nOldLevel))
            {
                Logger.Warn("ForgetSkill={0} not find!", nId);
                return;
            }
            _this.mDbData.Skills[nId] = -nOldLevel;
            _this.mFlag = true;
            _this.MarkDirty();
            _this.mCharacter.SkillChange(0, nId, 0);
            Logger.Info("ForgetSkill={0} is Success!", nId);
        }

        public ErrorCodes UpgradeSkill(SkillData _this, int nId, ref int result)
        {
            int nOldLevel;
            if (!_this.mDbData.Skills.TryGetValue(nId, out nOldLevel))
            {
                Logger.Warn("UpgradeSkill={0} not find !", nId);
                return ErrorCodes.Error_NotHaveSkill;
            }
            if (nOldLevel*5 > _this.mCharacter.GetLevel())
            {
                return ErrorCodes.Error_SkillLevelMax;
            }
            //int maxLevel = mCharacter.GetLevel()/5 + 10;  //todo
            //if (nOldLevel >= maxLevel)
            //{
            //    return ErrorCodes.Error_SkillLevelMax;
            //}
            var tbSkill = Table.GetSkill(nId);
            if (tbSkill == null)
            {
                return ErrorCodes.Error_SkillID;
            }
            if (tbSkill.NeedMoney == -1)//表中需要改此数值
            {
                return ErrorCodes.Error_SkillNoTUpgrade;
            }
            var tbUpgrade = Table.GetSkillUpgrading(tbSkill.NeedMoney);
            var needValue = tbUpgrade.GetSkillUpgradingValue(nOldLevel - 1);
            var tbUpgradeTyp = Table.GetSkillUpgrading(999);
            var needValueType = tbUpgradeTyp.GetSkillUpgradingValue(nOldLevel - 1);
            if (needValue < 1)
            {
                return ErrorCodes.Error_SkillNoTUpgrade;
            }
            if (_this.mCharacter.mBag.GetItemCount(needValueType) < needValue)
            {
                return ErrorCodes.Error_ResNoEnough;
            }
            _this.mCharacter.mBag.DeleteItem(needValueType, needValue, eDeleteItemType.UpgradeSkill);
            LevelUpSkill(_this, nId, nOldLevel + 1);
            var e = new UpgradeSkillEvent(_this.mCharacter, nId);
            EventDispatcher.Instance.DispatchEvent(e);
            _this.mCharacter.AddExData((int) eExdataDefine.e89, 1);

            try
            {
                var klog = string.Format("upgradeskill#{0}|{1}|{2}|{3}|{4}|{5}",
                    _this.mCharacter.mGuid,
                    _this.mCharacter.GetLevel(),
                    _this.mCharacter.serverId,
                    nId,
                    nOldLevel,
                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                PlayerLog.Kafka(klog, 2);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
            }

            return ErrorCodes.OK;
        }


	    //升级技能
        public void LevelUpSkill(SkillData _this, int nId, int nLevel = 0)
        {
            PlayerLog.WriteLog(_this.mCharacter.mGuid, "----------LevelUpSkill----------skillId={0}:Lv{1}", nId, nLevel);
            var skills = _this.mDbData.Skills;
            int nOldLevel;
            if (!skills.TryGetValue(nId, out nOldLevel))
            {
                Logger.Warn("LevelUpSkill={0} not find !", nId);
                return;
            }
            if (nLevel == 0)
            {
                nLevel = nOldLevel + 1;
            }
            _this.mFlag = true;
            _this.MarkDirty();
            skills[nId] = nLevel;
            if (_this.mDbData.EquipSkills.Contains(nId))
            {
                _this.mCharacter.SkillChange(2, nId, nLevel);
            }
            _this.mCharacter.RefreshSkillTitle();
            Logger.Info("LevelUpSkill={0} is Success! Level={1}", nId, nLevel);
        }

        ////设置技能等级
        //public void SetSkillLevel(int nId, int nLevel)
        //{
        //    int nOldLevel;
        //    if (!mDbData.Skills.TryGetValue(nId, out nOldLevel))
        //    {
        //        Logger.Warn("SetSkillLevel={0} not find !", nId);
        //        return;
        //    }
        //    mFlag = true;
        //    MarkDirty();
        //    mDbData.Skills[nId] = nLevel;
        //    mCharacter.SkillChange(2, nId, nLevel);
        //}
        //获得某个技能ID的等级
        public int GetSkillLevel(SkillData _this, int nId)
        {
            int nOldLevel;
            if (!_this.mDbData.Skills.TryGetValue(nId, out nOldLevel))
            {
                Logger.Warn("GetSkillLevel={0} not find !", nId);
                return 0;
            }
            return nOldLevel;
        }

		public int GetSkillTotalLevel(SkillData _this)
	    {
		    int ret = 0;
			foreach (var skill in _this.mDbData.Skills)
			{
				var tb = Table.GetSkill(skill.Key);
				if (null != tb && 1 == tb.Type)
				{
					ret += skill.Value;	
				}
			}
		    return ret;
	    }

        #endregion
    }

    public class SkillData : NodeBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static ISkillData mImpl;

        static SkillData()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (SkillData), typeof (SkillDataDefaultImpl),
                o => { mImpl = (ISkillData) o; });
        }

        public override IEnumerable<NodeBase> Children
        {
            get { return null; }
        }

        public override void NetDirtyHandle()
        {
        }

        #region  数据结构

        public DBSkill mDbData; //技能数据（ID，等级）
        public CharacterController mCharacter; //角色
        public bool mFlag { get; set; }

        #endregion

        #region  初始化

        //用第一次创建
        public DBSkill InitByBase(CharacterController character, int characterTableId)
        {
            return mImpl.InitByBase(this, character, characterTableId);
        }

        //用数据库数据
        public void InitByDB(CharacterController character, DBSkill skillData)
        {
            mImpl.InitByDB(this, character, skillData);
        }

        #endregion

        #region  技能相关

        //装备技能
        public ErrorCodes EquipSkills(List<int> skills)
        {
            return mImpl.EquipSkills(this, skills);
        }

        public bool HaveSkill(int nId)
        {
            return mImpl.HaveSkill(this, nId);
        }

        //学习技能
        public void LearnSkill(int nId, int nLevel)
        {
            mImpl.LearnSkill(this, nId, nLevel);
        }

        //遗忘技能
        public void ForgetSkill(int nId)
        {
            mImpl.ForgetSkill(this, nId);
        }

        public ErrorCodes UpgradeSkill(int nId, ref int result)
        {
            return mImpl.UpgradeSkill(this, nId, ref result);
        }

        //升级技能
        public void LevelUpSkill(int nId, int nLevel = 0)
        {
            mImpl.LevelUpSkill(this, nId, nLevel);
        }

        public int GetSkillLevel(int nId)
        {
            return mImpl.GetSkillLevel(this, nId);
        }

		public int GetSkillTotalLevel()
        {
			return mImpl.GetSkillTotalLevel(this);
        }
		
        #endregion
    }
}