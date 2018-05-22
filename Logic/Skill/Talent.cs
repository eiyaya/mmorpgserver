#region using

using System.Collections.Generic;
using DataContract;
using DataTable;
using EventSystem;
using NLog;
using Shared;

#endregion

namespace Logic
{
    public interface ITalent
    {
        void AddSkillPoint(Talent _this, int nSkillId, int nAdd);
        ErrorCodes AddTalent(Talent _this, CharacterController character, int talent);
        void AddTalentPoint(Talent _this, int nAdd);
        void ChangeDistributablePoint(Talent _this, int id, int changeValue);
        void CleanTalent(Talent _this, int nId);
        int GetDistributablePoint(Talent _this, int id);
        List<TalentRecord> GetSkillRef(int skillId);
        int GetSkillTalentAddCount(Talent _this, int nSkillId);
        int GetSkillTalentCount(Talent _this, int nSkillId);
        int GetTalent(Talent _this, int nId);
        int GetTalentCount(Talent _this, int type);
        void Init();
        DBInnate InitByBase(Talent _this, CharacterController character);
        void InitByDB(Talent _this, CharacterController character, DBInnate TalentData);
        void RefreshTalent(Talent _this, CharacterController character);
        ErrorCodes ResetSkillTalent(Talent _this, CharacterController character, int skillId);
        void ResetSkillTalent(Talent _this, int nSkillId);
    }

    public class TalentDefaultImpl : ITalent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region  初始化

        //整理技能ID相关的所有天赋
        public void Init()
        {
            Table.ForeachTalent(record =>
            {
                var skillID = record.ModifySkill;
                if (skillID == -1)
                {
                    return true;
                }
                List<TalentRecord> tempList;
                if (!Talent.mSkillRef.TryGetValue(skillID, out tempList))
                {
                    tempList = new List<TalentRecord>();
                    Talent.mSkillRef[skillID] = tempList;
                }
                tempList.Add(record);
                return true;
            });
        }

        //用第一次创建
        public DBInnate InitByBase(Talent _this, CharacterController character)
        {
            _this.mDbData = new DBInnate();
            _this.mDbData.InnateCount = 0;
            _this.mCharacter = character;
            _this.mFlag = true;
            //foreach (var i in mCharacter.mSkill.mDbData.Skills)
            //{
            //    AddSkillPoint(i.Key, 5);
            //}
            _this.MarkDbDirty();
            return _this.mDbData;
        }

        //用数据库数据
        public void InitByDB(Talent _this, CharacterController character, DBInnate TalentData)
        {
            _this.mCharacter = character;
            _this.mDbData = TalentData;
            //TalentCount = TalentData.InnateCount;
            //foreach (var i in TalentData.Innates)
            //{
            //    int value = 0;
            //    if (!Talents.TryGetValue(i.Key, out value))
            //    {
            //        Talents.Add(i.Key, i.Value);
            //    }
            //    else
            //    {
            //        Talents[i.Key] = i.Value;
            //    }
            //}
            //mFlag = false;
            //Dirty = false;
        }

        #endregion

        #region  天赋相关

        //获得可分配点数(skillID,改变值)
        public void ChangeDistributablePoint(Talent _this, int id, int changeValue)
        {
            var tbTalent = Table.GetTalent(id);
            if (tbTalent == null)
            {
                return;
            }
            var skillid = tbTalent.ModifySkill;
            int count;

            if (!_this.Skills.TryGetValue(skillid, out count))
            {
                //不用修改天赋剩余点数了，改为消耗资源
                 count = _this.TalentCount + changeValue;
                 _this.TalentCount = count;
                 if (changeValue < 0)
                 {
                     //加修炼天赋
                     var oldCount = _this.mCharacter.GetExData((int) eExdataDefine.e91);
                     var nowCount = GetTalentCount(_this, 0);
                     if (nowCount > oldCount)
                     {
                         _this.mCharacter.SetExData((int) eExdataDefine.e91, nowCount);
                     }
                 }
                return;
            }
            if (skillid != -1)
            {
                count = count + changeValue;
                _this.Skills[skillid] = count;
                if (changeValue < 0)
                {
                    //加技能天赋
                    var oldCount = _this.mCharacter.GetExData((int) eExdataDefine.e86);
                    var nowCount = GetTalentCount(_this, 1);
                    if (nowCount > oldCount)
                    {
                        _this.mCharacter.SetExData((int) eExdataDefine.e86, nowCount);
                    }
                }
            }
        }

        //获得可分配点数
        public int GetDistributablePoint(Talent _this, int id)
        {
            var tbTalent = Table.GetTalent(id);
            var skillId = tbTalent.ModifySkill;
            if (skillId == -1)
            {
                //不在消耗技能点数改消耗道具
                return 1; //_this.TalentCount;

            }
            int count;
            if (_this.Skills.TryGetValue(skillId, out count))
            {
                return count;
            }
            return 0;
        }

        //洗天赋
        public void RefreshTalent(Talent _this, CharacterController character)
        {
            var templist = new List<int>();
            foreach (var i in _this.Talents)
            {
                if (Table.GetTalent(i.Key).ModifySkill == -1)
                {
                    templist.Add(i.Key);
                }
            }
            foreach (var id in templist)
            {
                DeleteTalent(_this, id);
            }
            //foreach (var i in Talents)
            //{
            //    ChangeDistributablePoint(i.Key, i.Value);
            //    //TalentCount += i.Value;
            //}
            //Talents.Clear();
            _this.mFlag = true;
            _this.MarkDirty();
            PlayerLog.WriteLog(_this.mCharacter.mGuid, "----------RefreshTalent----------TalentCount={0}",
                _this.TalentCount);
            //Logger.Info("RefreshTalent is Success! TalentCount={0}", TalentCount);
            _this.mCharacter.TalentChange(3, -1, -1);
        }

        //重置技能天赋
        public ErrorCodes ResetSkillTalent(Talent _this, CharacterController character, int skillId)
        {
            var nowPoint = GetSkillTalentAddCount(_this, skillId);
            if (nowPoint < 1)
            {
                return ErrorCodes.Error_SkillTalentNoReset;
            }
            var tbSkill = Table.GetSkill(skillId);
            if (tbSkill == null)
            {
                return ErrorCodes.Unknow;
            }
            var needCount = tbSkill.ResetCount*nowPoint;
            if (character.mBag.GetRes(eResourcesType.Spar) < needCount)
            {
                return ErrorCodes.Error_ResNoEnough;
            }
            character.mBag.DelRes(eResourcesType.Spar, needCount, eDeleteItemType.ResetSkillTalent);
            //if (character.mBag.GetItemCount(tbSkill.SkillID) < 1)
            //{
            //    return ErrorCodes.ItemNotEnough;
            //}
            //character.mBag.DeleteItem(tbSkill.SkillID, 1);
            ResetSkillTalent(_this, skillId);
            return ErrorCodes.OK;
        }

        //删除某个天赋天赋数据
        private bool DeleteTalent(Talent _this, int nId)
        {
            var nLayer = GetTalent(_this, nId);
            if (nLayer < 1)
            {
                return false;
            }
            _this.Talents.Remove(nId);
            ChangeDistributablePoint(_this, nId, nLayer);
            //TalentCount += nLayer;
            _this.mFlag = true;
            _this.MarkDirty();

            var tbTalent = Table.GetTalent(nId);
            if (tbTalent == null)
            {
                Logger.Error("CleanTalent={0} is Success! but Talent Not Find!", nId);
                return true;
            }
            if (tbTalent.ActiveSkillId != -1)
            {
                var nOldLevel = _this.mCharacter.mSkill.GetSkillLevel(tbTalent.ActiveSkillId);
                if (nOldLevel == 0)
                {
                    Logger.Info("CleanTalent={0} is Success! ForgetSkill is Faild SkillId={1}", nId,
                        tbTalent.ActiveSkillId);
                    return true;
                }
                _this.mCharacter.mSkill.ForgetSkill(tbTalent.ActiveSkillId);
                if (tbTalent.ForgetSkillId != -1)
                {
                    _this.mCharacter.mSkill.LearnSkill(tbTalent.ForgetSkillId, nOldLevel);
                }
            }
            return true;
        }

        //清除某个天赋
        public void CleanTalent(Talent _this, int nId)
        {
            if (!DeleteTalent(_this, nId))
            {
                Logger.Warn("CleanTalent={0} is faild!", nId);
                return;
            }
            PlayerLog.WriteLog(_this.mCharacter.mGuid, "----------CleanTalent----------TalentId={0}", nId);
            //Logger.Info("CleanTalent={0} is Success!", nId);
            _this.mCharacter.TalentChange(0, nId, 0);
        }

        //增加某个天赋的投入点数（接口）
        public ErrorCodes AddTalent(Talent _this, CharacterController character, int talent)
        {
            //PlayerLog.WriteLog(_this.mCharacter.mGuid, "----------AddTalent----------TalentId={0}", talent);
            var tbTalent = Table.GetTalent(talent);
            if (tbTalent == null)
            {
                return ErrorCodes.Error_InnateID;
            }
            //剩余点数判断
            if (GetDistributablePoint(_this, talent) <= 0)
            {
                return ErrorCodes.Error_InnateNoPoint;
            }

            //职业判断
            //最大点数判断
            var nLayer = GetTalent(_this, talent);
            if (nLayer >= tbTalent.MaxLayer)
            {
                return ErrorCodes.Error_InnateMaxLayer;
            }
            //前置判断
            if (tbTalent.BeforeId != -1 && GetTalent(_this, tbTalent.BeforeId) < tbTalent.BeforeLayer)
            {
                return ErrorCodes.Error_InnateNoBefore;
            }

            //天赋需要消耗资源
            if (tbTalent.ModifySkill == -1)
            {
                if (tbTalent.NeedLevel > _this.mCharacter.GetLevel())
                {
                    return ErrorCodes.Error_LevelNoEnough;
                }

                var tbUpgrade = Table.GetSkillUpgrading(tbTalent.CastItemCount);
                var needValue = tbUpgrade.GetSkillUpgradingValue(nLayer);
                if (needValue > _this.mCharacter.mBag.GetItemCount(tbTalent.CastItemId))
                {
                    return ErrorCodes.Error_ResNoEnough;
                }
                _this.mCharacter.mBag.DeleteItem(tbTalent.CastItemId, needValue, eDeleteItemType.UpgradeSkill);
            }


            //互斥判断
            if (GetTalent(_this, tbTalent.HuchiId) != 0)
            {
//如果互斥了,需要所有互斥的点数，然后增加这个
                CleanTalent(_this, tbTalent.HuchiId);
            }
            //执行操作
            nLayer = AddTalent(_this, talent);
            if (tbTalent.ActiveSkillId != -1 && nLayer == 1)
            {
                if (tbTalent.ForgetSkillId != -1)
                {
                    var nOldLevel = character.mSkill.GetSkillLevel(tbTalent.ForgetSkillId);
                    if (nOldLevel == 0)
                    {
                        Logger.Info("AddTalent={0} is Faild! SkillId={1}", talent, tbTalent.ForgetSkillId);
                    }
                    else
                    {
                        character.mSkill.ForgetSkill(tbTalent.ForgetSkillId);
                        character.mSkill.LearnSkill(tbTalent.ActiveSkillId, nOldLevel);
                    }
                }
                else
                {
                    character.mSkill.LearnSkill(tbTalent.ActiveSkillId, 1);
                }
            }
            _this.mFlag = true;
            _this.MarkDirty();
            ChangeDistributablePoint(_this, talent, -1);
			EventDispatcher.Instance.DispatchEvent(new AddTalentEvent(character, talent));
            return ErrorCodes.OK;
        }

        //增加某个天赋的投入点数（执行）
        private int AddTalent(Talent _this, int nId)
        {
            int value;
            if (_this.Talents.TryGetValue(nId, out value))
            {
                var nLayer = value + 1;
                _this.Talents[nId] = nLayer;
                _this.mCharacter.TalentChange(2, nId, nLayer);
                Logger.Info("AddTalent={0} is Success! Level={1}", nId, nLayer);
                return nLayer;
            }
            _this.Talents[nId] = 1;
            _this.mCharacter.TalentChange(1, nId, 1);
            Logger.Info("AddTalent={0} is Success! Level={1}", nId, 1);
            return 1;
        }

        //获得某个天赋点数
        public int GetTalent(Talent _this, int nId)
        {
            int nLayer;
            if (_this.Talents.TryGetValue(nId, out nLayer))
            {
                return nLayer;
            }
            return 0;
        }

        //增加天赋的可分配点数
        public void AddTalentPoint(Talent _this, int nAdd)
        {
            if (nAdd <= 0)
            {
                Logger.Warn("AddTalentPoint error! Value < 0");
                return;
            }
            _this.mFlag = true;
            _this.MarkDirty();
            _this.TalentCount += nAdd;
            if (_this.TalentCount < 0)
            {
                _this.TalentCount = 0;
            }
            PlayerLog.WriteLog(_this.mCharacter.mGuid, "----------AddTalentPoint----------TalentCount={0}",
                _this.TalentCount);
            //Logger.Info("AddTalentPoint is Success! TalentCount=={0}", TalentCount);
        }

        //重置某个技能的相关天赋
        public void ResetSkillTalent(Talent _this, int nSkillId)
        {
            var list = GetSkillRef(nSkillId);
            if (list == null)
            {
                return;
            }
            foreach (var record in list)
            {
                //DeleteTalent(record.Id);
                CleanTalent(_this, record.Id);
            }
            //List<int> removeList = new List<int>();
            //foreach (KeyValuePair<int, int> i in Talents)
            //{
            //    var tbTalent = Table.GetTalent(i.Key);
            //    if(tbTalent==null) continue;
            //    if (tbTalent.ModifySkill == nSkillId)
            //    {
            //        removeList.Add(i.Key);
            //    }
            //}
            //foreach (int i in removeList)
            //{
            //    DeleteTalent(i);
            //}
        }

        //获取天赋总点数 0=修炼  1=天赋
        public int GetTalentCount(Talent _this, int type)
        {
            var count = 0;
            if (type == 0)
            {
                foreach (var i in _this.Talents)
                {
                    var tbTalent = Table.GetTalent(i.Key);
                    if (tbTalent.ModifySkill == -1)
                    {
                        count += i.Value;
                    }
                }
            }
            else if (type == 1)
            {
                foreach (var i in _this.Talents)
                {
                    var tbTalent = Table.GetTalent(i.Key);
                    if (tbTalent.ModifySkill != -1)
                    {
                        count += i.Value;
                    }
                }
            }
            return count;
        }

        //获取某技能的总天赋数
        public int GetSkillTalentCount(Talent _this, int nSkillId)
        {
            int count;
            if (_this.Skills.TryGetValue(nSkillId, out count))
            {
            }
            var list = GetSkillRef(nSkillId);
            if (list == null)
            {
                Logger.Warn("GetSkillTalentCount not find = {0}", nSkillId);
                return count;
            }
            foreach (var record in list)
            {
                count += GetTalent(_this, record.Id);
            }
            return count;
        }

        //获取某技能的已点天赋数
        public int GetSkillTalentAddCount(Talent _this, int nSkillId)
        {
            var count = 0;
            var list = GetSkillRef(nSkillId);
            if (list == null)
            {
                Logger.Warn("GetSkillTalentCount not find = {0}", nSkillId);
                return count;
            }
            foreach (var record in list)
            {
                count += GetTalent(_this, record.Id);
            }
            return count;
        }

        #endregion

        #region  技能相关

        public List<TalentRecord> GetSkillRef(int skillId)
        {
            List<TalentRecord> tempList;
            if (Talent.mSkillRef.TryGetValue(skillId, out tempList))
            {
                return tempList;
            }
            return null;
        }

        public void AddSkillPoint(Talent _this, int nSkillId, int nAdd)
        {
            if (nAdd <= 0)
            {
                Logger.Warn("AddSkillPoint error! Value < 0");
                return;
            }
            _this.mFlag = true;
            _this.MarkDirty();
            int count;
            if (_this.Skills.TryGetValue(nSkillId, out count))
            {
                count += nAdd;
                _this.Skills[nSkillId] = count;
                var eAdd = new SkillPointChangeEvent(_this.mCharacter, nSkillId, count);
                EventDispatcher.Instance.DispatchEvent(eAdd);
            }
            else
            {
                _this.Skills[nSkillId] = nAdd;
                var eAdd = new SkillPointChangeEvent(_this.mCharacter, nSkillId, nAdd);
                EventDispatcher.Instance.DispatchEvent(eAdd);
            }
        }

        #endregion
    }

    public class Talent : NodeBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static ITalent mImpl;
        public static Dictionary<int, List<TalentRecord>> mSkillRef = new Dictionary<int, List<TalentRecord>>();

        static Talent()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (Talent), typeof (TalentDefaultImpl),
                o => { mImpl = (ITalent) o; });
        }

        public CharacterController mCharacter; //角色
        public DBInnate mDbData;

        public override IEnumerable<NodeBase> Children
        {
            get { return null; }
        }

        public bool mFlag { get; set; }

        public Dictionary<int, int> Skills
        {
            get { return mDbData.Skills; }
        } //技能数据（技能ID，点数）

        public int TalentCount
        {
            get { return mDbData.InnateCount; }
            set { mDbData.InnateCount = value; }
        } //天赋剩余点数

        public Dictionary<int, int> Talents
        {
            get { return mDbData.Innates; }
        } //天赋数据（天赋，层数）

        public override void NetDirtyHandle()
        {
        }

        #region  初始化

        //整理技能ID相关的所有天赋
        public static void Init()
        {
            mImpl.Init();
        }

        //用第一次创建
        public DBInnate InitByBase(CharacterController character)
        {
            return mImpl.InitByBase(this, character);
        }

        //用数据库数据
        public void InitByDB(CharacterController character, DBInnate TalentData)
        {
            mImpl.InitByDB(this, character, TalentData);
        }

        #endregion

        #region  天赋相关

        //获得可分配点数(skillID,改变值)
        public void ChangeDistributablePoint(int id, int changeValue)
        {
            mImpl.ChangeDistributablePoint(this, id, changeValue);
        }

        //获得可分配点数
        public int GetDistributablePoint(int id)
        {
            return mImpl.GetDistributablePoint(this, id);
        }

        //洗天赋
        public void RefreshTalent(CharacterController character)
        {
            mImpl.RefreshTalent(this, character);
        }

        //重置技能天赋
        public ErrorCodes ResetSkillTalent(CharacterController character, int skillId)
        {
            return mImpl.ResetSkillTalent(this, character, skillId);
        }

        //清除某个天赋
        public void CleanTalent(int nId)
        {
            mImpl.CleanTalent(this, nId);
        }

        //增加某个天赋的投入点数（接口）
        public ErrorCodes AddTalent(CharacterController character, int talent)
        {
            return mImpl.AddTalent(this, character, talent);
        }

        //获得某个天赋点数
        public int GetTalent(int nId)
        {
            return mImpl.GetTalent(this, nId);
        }

        //增加天赋的可分配点数
        public void AddTalentPoint(int nAdd)
        {
            mImpl.AddTalentPoint(this, nAdd);
        }

        //重置某个技能的相关天赋
        public void ResetSkillTalent(int nSkillId)
        {
            mImpl.ResetSkillTalent(this, nSkillId);
        }

        //获取天赋总点数 0=修炼  1=天赋
        public int GetTalentCount(int type)
        {
            return mImpl.GetTalentCount(this, type);
        }

        //获取某技能的总天赋数
        public int GetSkillTalentCount(int nSkillId)
        {
            return mImpl.GetSkillTalentCount(this, nSkillId);
        }

        //获取某技能的已点天赋数
        public int GetSkillTalentAddCount(int nSkillId)
        {
            return mImpl.GetSkillTalentAddCount(this, nSkillId);
        }

        #endregion

        #region  技能相关

        public static List<TalentRecord> GetSkillRef(int skillId)
        {
            return mImpl.GetSkillRef(skillId);
        }

        public void AddSkillPoint(int nSkillId, int nAdd)
        {
            mImpl.AddSkillPoint(this, nSkillId, nAdd);
        }

        #endregion
    }
}