#region using

using System;
using System.Collections.Generic;
using System.Linq;
using DataContract;
using DataTable;
using EventSystem;
using NLog;
using Shared;

#endregion

namespace Logic
{
    public interface IMission
    {
        void Init(Mission _this, CharacterController character, MissionRecord tbmis);
        void Mission(Mission _this, CharacterController character, DBMission dbdata);
        void Mission(Mission _this, CharacterController character, int nId);
        bool IsGang(int missionId);
        int GetRandSeedExDataId(int missionId);
        int GetTimesExDataId(int missionId);
        List<int> FindHunterMissions(int missionId);
        int RandomHunterMissionId(Mission _this, CharacterController character, int nId, int oldMisId);
    }

    public class MissionDefaultImpl : IMission
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); //
        //构造任务
        public void Mission(Mission _this, CharacterController character, DBMission dbdata)
        {
            _this.mDbData = dbdata;
            if (_this.Data[0] == 1 || _this.Data[0] == 3)
            {
                return;
            }
            var tbmis = Table.GetMission(dbdata.Id);
            _this.Data[1] = tbmis.FinishCondition;
            //if (tbmis.ViewType == 2 && _this.Data[0] == 3)
            //{
            //    _this.Data[0] = 0;
            //}
            var eMission = (eMissionType)tbmis.FinishCondition;
            switch (eMission)
            {
                case eMissionType.Finish:
                    {
                        _this.Data[0] = (int)eMissionState.Finished;
                    }
                    break;
                case eMissionType.KillMonster:
                    {
                        _this.Data[3] = tbmis.FinishParam[0]; //怪物ID
                        _this.Data[4] = tbmis.FinishParam[1]; //需求数量
                    }
                    break;
                case eMissionType.AcceptProgressBar:
                    {
                        //Data[0] = (int)eMissionState.Finished;
                        _this.Data[3] = tbmis.FinishParam[0]; //读条时间
                    }
                    break;
                case eMissionType.AreaProgressBar:
                    {
                        _this.Data[3] = tbmis.FinishParam[2]; //区域ID
                        _this.Data[4] = tbmis.FinishParam[0]; //读条时间
                    }
                    break;
                case eMissionType.CheckItem:
                    {
                        _this.Data[3] = tbmis.FinishParam[0]; //物品ID
                        _this.Data[4] = tbmis.FinishParam[1]; //需求数量
                        _this.Data[2] = character.mBag.GetItemCount(tbmis.FinishParam[0]); //刷新当前数量
                        if (_this.Data[2] >= _this.Data[4])
                        {
                            _this.Data[0] = (int)eMissionState.Finished;
                        }
                    }
                    break;
                case eMissionType.AcceptStroy:
                    {
                        //Data[0] = (int)eMissionState.Finished;
                        _this.Data[3] = tbmis.FinishParam[0]; //剧情ID
                    }
                    break;
                case eMissionType.AreaStroy:
                    {
                        _this.Data[3] = tbmis.FinishParam[2]; //区域ID
                        _this.Data[4] = tbmis.FinishParam[0]; //剧情ID
                    }
                    break;
                case eMissionType.Tollgate:
                    {
                        _this.Data[3] = tbmis.FinishParam[0]; //副本ID
                        _this.Data[4] = tbmis.FinishParam[1]; //需求次数
                        if (_this.Data[2] >= _this.Data[4])
                        {
                            _this.Data[0] = (int)eMissionState.Finished;
                        }
                    }
                    break;
                case eMissionType.BuyItem:
                    {
                        _this.Data[3] = tbmis.FinishParam[0]; //物品ID
                        _this.Data[4] = tbmis.FinishParam[1]; //物品数量
                        if (_this.Data[2] >= _this.Data[4])
                        {
                            _this.Data[0] = (int)eMissionState.Finished;
                        }
                    }
                    break;
                case eMissionType.EquipItem:
                    {
                        _this.Data[3] = tbmis.FinishParam[0]; //部位参数
                        _this.Data[4] = 1; //tbmis.FinishParam[1]; //次数
                        if (_this.Data[2] >= _this.Data[4])
                        {
                            _this.Data[0] = (int)eMissionState.Finished;
                        }
                    }
                    break;
                case eMissionType.EnhanceEquip:
                    {
                        _this.Data[3] = tbmis.FinishParam[0]; //部位参数
                        _this.Data[4] = tbmis.FinishParam[1]; //次数
                        if (_this.Data[2] >= _this.Data[4])
                        {
                            _this.Data[0] = (int)eMissionState.Finished;
                        }
                    }
                    break;
                case eMissionType.AdditionalEquip:
                    {
                        _this.Data[3] = tbmis.FinishParam[0]; //部位参数
                        _this.Data[4] = tbmis.FinishParam[1]; //次数
                        if (_this.Data[2] >= _this.Data[4])
                        {
                            _this.Data[0] = (int)eMissionState.Finished;
                        }
                    }
                    break;
                case eMissionType.UpgradeSkill:
                    {
                        _this.Data[3] = tbmis.FinishParam[0]; //技能ID
                        _this.Data[4] = tbmis.FinishParam[1]; //次数
                        if (_this.Data[2] >= _this.Data[4])
                        {
                            _this.Data[0] = (int)eMissionState.Finished;
                        }
                    }
                    break;
                case eMissionType.NpcServe:
                    {
                        _this.Data[3] = tbmis.FinishParam[0]; //服务ID
                        _this.Data[4] = tbmis.FinishParam[1]; //次数
                        if (_this.Data[2] >= _this.Data[4])
                        {
                            _this.Data[0] = (int)eMissionState.Finished;
                        }
                    }
                    break;
                case eMissionType.Arena:
                    {
                        _this.Data[4] = tbmis.FinishParam[0]; //次数
                        if (_this.Data[2] >= _this.Data[4])
                        {
                            _this.Data[0] = (int)eMissionState.Finished;
                        }
                    }
                    break;
                case eMissionType.AddFriend:
                    {
                        _this.Data[4] = tbmis.FinishParam[0]; //次数
                        if (_this.Data[2] >= _this.Data[4])
                        {
                            _this.Data[0] = (int)eMissionState.Finished;
                        }
                    }
                    break;
                case eMissionType.ComposeItem:
                    {
                        _this.Data[3] = tbmis.FinishParam[0]; //合成类型
                        //mData[3] = tbmis.FinishParam[1]; //道具ID
                        _this.Data[4] = tbmis.FinishParam[2]; //次数
                        if (_this.Data[2] >= _this.Data[4])
                        {
                            _this.Data[0] = (int)eMissionState.Finished;
                        }
                    }
                    break;
                case eMissionType.ExdataAdd:
                    {
                        _this.Data[3] = tbmis.FinishParam[0]; //扩展计数ID
                        _this.Data[4] = tbmis.FinishParam[1]; //次数
                        if (_this.Data[2] >= _this.Data[4])
                        {
                            _this.Data[0] = (int)eMissionState.Finished;
                        }
                    }
                    break;
                case eMissionType.ExDataChange:
                    {
                        _this.Data[3] = tbmis.FinishParam[0]; //扩展计数ID
                        _this.Data[4] = tbmis.FinishParam[1]; //次数
                        _this.Data[2] = character.GetExData(tbmis.FinishParam[0]); //刷新当前值
                        if (_this.Data[2] >= _this.Data[4])
                        {
                            _this.Data[0] = (int)eMissionState.Finished;
                        }
                    }
                    break;
                case eMissionType.LearnSkill:
                    {
                        var skillId = tbmis.FinishParam[0];
                        _this.Data[3] = skillId; //技能 
                        _this.Data[4] = 1;
                        _this.Data[2] = character.mSkill.GetSkillLevel(skillId);
                        if (_this.Data[2] >= 1)
                        {
                            _this.Data[0] = (int)eMissionState.Finished;
                        }
                    }
                    break;
                case eMissionType.GetSkillTalant:
                    {
                        var skillId = tbmis.FinishParam[0];
                        _this.Data[3] = skillId; //技能 
                        _this.Data[4] = tbmis.FinishParam[1];
                        _this.Data[2] = character.mTalent.GetSkillTalentCount(skillId);
                        if (_this.Data[2] >= _this.Data[4])
                        {
                            _this.Data[0] = (int)eMissionState.Finished;
                        }
                    }
                    break;
                case eMissionType.Dungeon:
                    {
                        _this.Data[3] = tbmis.FinishParam[0]; //副本ID
                        _this.Data[4] = tbmis.FinishParam[1]; //需求次数
                        if (_this.Data[2] >= _this.Data[4])
                        {
                            _this.Data[0] = (int)eMissionState.Finished;
                        }
                    }
                    break;
                case eMissionType.DepotTakeOut:
                    {
                        _this.Data[3] = tbmis.FinishParam[0]; //道具
                        _this.Data[4] = tbmis.FinishParam[1]; //需求次数
                        if (_this.Data[2] >= _this.Data[4])
                        {
                            _this.Data[0] = (int)eMissionState.Finished;
                        }
                    }
                    break;
                case eMissionType.FlagTrue:
                    {
                        _this.Data[3] = tbmis.FinishParam[0]; //扩展计数ID
                        _this.Data[4] = 1; //次数
                        _this.Data[2] = character.GetFlag(tbmis.FinishParam[0]) ? 1 : 0; //刷新当前值
                        if (_this.Data[2] >= _this.Data[4])
                        {
                            _this.Data[0] = (int)eMissionState.Finished;
                        }
                        break;
                    }
                case eMissionType.ObtainItem:
                {
                    _this.Data[3] = tbmis.FinishParam[0]; //物品ID
                    _this.Data[4] = tbmis.FinishParam[1]; //需求数量
                    if (_this.Data[2] >= _this.Data[4])
                    {
                        _this.Data[0] = (int)eMissionState.Finished;
                    }
                }
                    break;

                default:
                    {
                        Logger.Error("MissionId is Type[{0}] is overflow", eMission);
                        break;
                    }
            }
        }

        /// <summary>
        /// 获取随机任务ID
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="character"></param>
        /// <param name="nId"></param>
        /// <returns></returns>
        private int RandomMissionId(Mission _this, CharacterController character, int nId)
        {
            var tbmis = Table.GetMission(nId);
            if (tbmis == null)
            {
                return -1;
            }

            var tbSkillUp = Table.GetSkillUpgrading(tbmis.RandomTaskID);
            if (tbSkillUp == null)
            {
                Logger.Error("GetSkillUpgrading={0} not find", tbmis.RandomTaskID);
                return -1;
            }

            var randSeedExId = _this.GetRandSeedExDataId(nId);     // 随机任务组用
            var idx = character.GetExData(randSeedExId);
            var timesExId = _this.GetTimesExDataId(nId); // 完成次数
            var times = character.GetExData(timesExId);

            var n = (int)tbSkillUp.Values.Count / 3;      //每个分组长度     //潜规则 分三组,每组取五个
            var baseIdx = ((int)(times / 5)) * n;         //根据当前次数取得当前分组位置

            idx += MyRandom.Random(1, 3);

            idx = idx >= n ? idx - n : idx;

            if (idx + baseIdx >= tbSkillUp.Values.Count)
                idx = tbSkillUp.Values.Range();

            character.SetExData(randSeedExId, idx);
            //character.SetExData(timesExId, times + 1);
            idx += baseIdx;

            var misId = tbSkillUp.GetSkillUpgradingValue(idx);// tbSkillUp.Values.Range();  
            var index = 0;
            var oldMisId = character.mTask.mDbData.OldPerMissions.getValue(tbmis.MutexID);
            if (oldMisId == misId)
            {
                while (true)
                {
                    index++;
                    if (index == 100)
                    {
                        break;
                    }
                    misId = tbSkillUp.Values.Range();
                    if (misId != oldMisId)
                    {
                        break;
                    }
                }
            }

            return misId;
        }

        /// <summary>
        /// 随机狩猎任务
        /// </summary>
        /// <returns></returns>
        public int RandomHunterMissionId(Mission _this, CharacterController character, int nId, int oldMisId)
        {
            var tbmis = Table.GetMission(nId);
            if (tbmis == null)
                return -1;

            var tbSkillUp = Table.GetSkillUpgrading(tbmis.RandomTaskID);
            if (tbSkillUp == null)
            {
                Logger.Error("GetSkillUpgrading={0} not find", tbmis.RandomTaskID);
                return -1;
            }

            var randomList = new List<int>();
            foreach (var missionId in tbSkillUp.Values)
            {
                var tbMission = Table.GetMission(missionId);
                if (tbMission == null)
                    continue;

                var needPiece = tbMission.FinishParam[0];
                int bookId;
                if (BookManager.PieceBookId.TryGetValue(needPiece, out bookId))
                {
                    if (!character.mBook.IsFullStar(bookId))
                    {
                        randomList.Add(missionId);
                    }
                }
            }

            if (randomList.Count > 0)
            {
                var misId = randomList[0];

                randomList.Remove(oldMisId);
                if (randomList.Count > 0)
                {
                    misId = randomList.Range();
                }
                return misId;
            }

            return -1;
        }

        /// <summary>
        /// 根据任务ID找可接的狩猎任务ID
        /// </summary>
        /// <param name="missionId"></param>
        /// <returns></returns>
        public List<int> FindHunterMissions(int missionId)
        {
            var seqMis = new List<int>();  // 按顺序查找的任务列表
            var testMisId = new List<int>();
            var tbSkillUpgrading = Table.GetSkillUpgrading(73010);
            if (tbSkillUpgrading != null)
            {
                testMisId.AddRange(tbSkillUpgrading.Values);
            }
            bool find = false;
            foreach (var i in testMisId)
            {
                if (find == false)
                {
                    seqMis.Insert(0, i);
                    if (i == missionId)
                        find = true;
                }
                else
                {
                    seqMis.Add(i);
                }
            }

            if (!find)
                seqMis.Clear();

            return seqMis;
        }

        public void Mission(Mission _this, CharacterController character, int nId)
        {
            var tbmis = Table.GetMission(nId);
            if (tbmis == null)
            {
                Logger.Error("MissionId={0} not find", nId);
                _this.mDbData = null;
                return;
            }

            if (tbmis.MutexID != -1)
            {
                var seqMis = FindHunterMissions(nId);
                int misId = -1;
                if (seqMis.Count > 0)
                { // 狩猎任务
                    var oldMisId = -1;
                    if (character.mTask.mDbData != null && character.mTask.mDbData.OldPerMissions != null)
                        character.mTask.mDbData.OldPerMissions.getValue(tbmis.MutexID);
                    foreach (var i in seqMis)
                    {
                        misId = RandomHunterMissionId(_this, character, i, oldMisId);
                        if (misId >= 0)
                            break;
                    }
                }
                else
                {
                    misId = RandomMissionId(_this, character, nId);
                }

                if (misId == -1)
                {
                    _this.mDbData = null;
                    return;
                }

                PlayerLog.WriteLog(character.mGuid, "MissionInit oldId={0},changeId={1}", nId, misId);
                tbmis = Table.GetMission(misId);
                if (tbmis == null)
                {
                    Logger.Error("MissionId oldId={0},changeId={1}", nId, misId);
                    _this.mDbData = null;
                    return;
                }
                nId = misId;
            }
            _this.mDbData = new DBMission();
            _this.Id = nId;
            Init(_this, character, tbmis);
        }

        //初始化任务扩展数据
        public void Init(Mission _this, CharacterController character, MissionRecord tbmis)
        {
            var eMission = (eMissionType)tbmis.FinishCondition;
            var mData = new int[5];
            //mData[0] = 0;  //0=未完成  1=已完成  2=失败  3=可接
            mData[1] = tbmis.FinishCondition;
            switch (eMission)
            {
                case eMissionType.Finish:
                    {
                        mData[0] = (int)eMissionState.Finished;
                    }
                    break;
                case eMissionType.KillMonster:
                    {
                        mData[3] = tbmis.FinishParam[0]; //怪物ID
                        mData[4] = tbmis.FinishParam[1]; //需求数量
                    }
                    break;
                case eMissionType.AcceptProgressBar:
                    {
                        mData[0] = (int)eMissionState.Finished;
                        mData[3] = tbmis.FinishParam[0]; //读条时间
                    }
                    break;
                case eMissionType.AreaProgressBar:
                    {
                        mData[3] = tbmis.FinishParam[2]; //区域ID
                        mData[4] = tbmis.FinishParam[0]; //读条时间
                    }
                    break;
                case eMissionType.CheckItem:
                    {
                        mData[3] = tbmis.FinishParam[0]; //物品ID
                        mData[4] = tbmis.FinishParam[1]; //需求数量
                        mData[2] = character.mBag.GetItemCount(tbmis.FinishParam[0]); //刷新当前数量
                        if (mData[2] >= mData[4])
                        {
                            mData[0] = (int)eMissionState.Finished;
                        }
                    }
                    break;
                case eMissionType.AcceptStroy:
                    {
                        mData[0] = (int)eMissionState.Finished;
                        mData[3] = tbmis.FinishParam[0]; //剧情ID
                    }
                    break;
                case eMissionType.AreaStroy:
                    {
                        mData[3] = tbmis.FinishParam[2]; //区域ID
                        mData[4] = tbmis.FinishParam[0]; //剧情ID
                    }
                    break;
                case eMissionType.Tollgate:
                    {
                        mData[3] = tbmis.FinishParam[0]; //副本ID
                        mData[4] = tbmis.FinishParam[1]; //需求次数
                    }
                    break;
                case eMissionType.BuyItem:
                    {
                        mData[3] = tbmis.FinishParam[0]; //物品ID
                        mData[4] = tbmis.FinishParam[1]; //物品数量
                    }
                    break;
                case eMissionType.EquipItem:
                    {
                        mData[3] = tbmis.FinishParam[0]; //部位参数
                        mData[4] = 1; //tbmis.FinishParam[1]; //次数
                        /*
                        //先判断下是否已经完成
                        if (null != character)
                        {
                            foreach (var i in EquipExtension.Equips)
                            {
                                var bag = character.GetBag(i);
                                if (null == bag)
                                {
                                    continue;
                                }
                                foreach (var itemBase in bag.mLogics)
                                {
                                    if (itemBase.GetId() < 0)
                                    {
                                        continue;
                                    }
                                    var tbEquip = Table.GetEquip(itemBase.GetId());
                                    if (null == tbEquip)
                                    {
                                        continue;
                                    }
                                    if (BitFlag.GetAnd(tbEquip.Part, mData[3]) == 0)
                                    {
                                        continue;
                                    }

                                    mData[2]++;
                                }
                            }
                        }


                        if (mData[2] >= mData[4])
                        {
                            mData[0] = (int) eMissionState.Finished;
                        }
                         * */
                    }
                    break;
                case eMissionType.EnhanceEquip:
                    {
                        mData[3] = tbmis.FinishParam[0]; //部位参数
                        mData[4] = tbmis.FinishParam[1]; //次数
                    }
                    break;
                case eMissionType.AdditionalEquip:
                    {
                        mData[3] = tbmis.FinishParam[0]; //部位参数
                        mData[4] = tbmis.FinishParam[1]; //次数
                    }
                    break;
                case eMissionType.UpgradeSkill:
                    {
                        mData[3] = tbmis.FinishParam[0]; //技能ID
                        mData[4] = tbmis.FinishParam[1]; //次数
                    }
                    break;
                case eMissionType.NpcServe:
                    {
                        mData[3] = tbmis.FinishParam[0]; //服务ID
                        mData[4] = tbmis.FinishParam[1]; //次数
                    }
                    break;
                case eMissionType.Arena:
                    {
                        mData[4] = tbmis.FinishParam[0]; //次数
                    }
                    break;
                case eMissionType.AddFriend:
                    {
                        mData[4] = tbmis.FinishParam[0]; //次数
                    }
                    break;
                case eMissionType.ComposeItem:
                    {
                        mData[3] = tbmis.FinishParam[0]; //合成类型
                        //mData[3] = tbmis.FinishParam[1]; //道具ID
                        mData[4] = tbmis.FinishParam[2]; //次数
                    }
                    break;
                case eMissionType.ExdataAdd:
                    {
                        mData[3] = tbmis.FinishParam[0]; //扩展计数ID
                        mData[4] = tbmis.FinishParam[1]; //次数
                    }
                    break;
                case eMissionType.ExDataChange:
                    {
                        mData[3] = tbmis.FinishParam[0]; //扩展计数ID
                        mData[4] = tbmis.FinishParam[1]; //次数
                        mData[2] = character.GetExData(tbmis.FinishParam[0]); //刷新当前值
                        if (mData[2] >= mData[4])
                        {
                            mData[0] = (int)eMissionState.Finished;
                        }
                    }
                    break;
                case eMissionType.LearnSkill:
                    {
                        var skillId = tbmis.FinishParam[0];
                        mData[3] = skillId; //技能 
                        mData[4] = 1;
                        mData[2] = character.mSkill.GetSkillLevel(skillId);
                        if (mData[2] >= 1)
                        {
                            mData[0] = (int)eMissionState.Finished;
                        }
                    }
                    break;
                case eMissionType.GetSkillTalant:
                    {
                        var skillId = tbmis.FinishParam[0];
                        mData[3] = skillId; //技能 
                        mData[4] = tbmis.FinishParam[1];
                        mData[2] = character.mTalent.GetSkillTalentCount(skillId);
                        if (mData[2] >= mData[4])
                        {
                            mData[0] = (int)eMissionState.Finished;
                        }
                    }
                    break;
                case eMissionType.Dungeon:
                    {
                        mData[3] = tbmis.FinishParam[0]; //副本ID
                        mData[4] = tbmis.FinishParam[1]; //需求次数
                    }
                    break;

                case eMissionType.DepotTakeOut:
                    {
                        mData[3] = tbmis.FinishParam[0]; //道具
                        mData[4] = tbmis.FinishParam[1]; //需求数量
                    }
                    break;
                case eMissionType.FlagTrue:
                    {
                        mData[3] = tbmis.FinishParam[0]; //扩展计数ID
                        mData[4] = 1; //次数
                        mData[2] = character.GetFlag(tbmis.FinishParam[0]) ? 1 : 0; //刷新当前值
                        if (mData[2] >= mData[4])
                        {
                            mData[0] = (int)eMissionState.Finished;
                        }
                        break;
                    }
                case eMissionType.ObtainItem:
                {
                    mData[3] = tbmis.FinishParam[0]; //物品ID
                    mData[4] = tbmis.FinishParam[1]; //需求数量
                    if (mData[2] >= mData[4])
                    {
                        mData[0] = (int)eMissionState.Finished;
                    }
                }
                    break;

                default:
                    {
                        Logger.Error("MissionId is Type[{0}] is overflow", eMission);
                        break;
                    }
            }
            _this.Data.Clear();
            _this.Data.AddRange(mData);
        }

        public bool IsGang(int missionId)
        {
            return missionId > 50000 && missionId < 60000;
        }

        public int GetRandSeedExDataId(int missionId)
        {
            var tbMis = Table.GetMission(missionId);
            if (tbMis != null)
            {
                if (tbMis.ViewType == (int)eMissionMainType.Farm)
                {
                    return 714;
                }
            }
            return IsGang(missionId) ? 717 : 716;
        }

        public int GetTimesExDataId(int missionId)
        {
            var tbMis = Table.GetMission(missionId);
            if (tbMis != null)
            {
                if (tbMis.ViewType == (int)eMissionMainType.Farm)
                {
                    return 719;
                }
            }

            return IsGang(missionId) ? 715 : 443;
        }
    }

    public class Mission : NodeBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); //
        private static IMission mImpl;

        static Mission()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof(Mission), typeof(MissionDefaultImpl),
                o => { mImpl = (IMission)o; });
        }

        //构造任务
        public Mission(CharacterController character, DBMission dbdata)
        {
            mImpl.Mission(this, character, dbdata);
        }

        public Mission(CharacterController character, int nId)
        {
            mImpl.Mission(this, character, nId);
        }

        public override IEnumerable<NodeBase> Children
        {
            get { return null; }
        }

        public List<int> Data
        {
            get { return mDbData.ExData; }
        } //0=任务状态、1=任务目标类型、2=任务目标计数、3=目标类型参数、4=目标需求数量

        public int Id
        {
            get { return mDbData.Id; }
            set { mDbData.Id = value; }
        }

        //public int[] mData = new int[5]; 
        //public bool mFlag;
        public DBMission mDbData { get; set; }
        //初始化任务扩展数据
        public void Init(CharacterController character, MissionRecord tbmis)
        {
            mImpl.Init(this, character, tbmis);
        }

        /// <summary>
        /// 是否帮派任务（把原先各种50000判断统一写到这了）
        /// </summary>
        /// <returns></returns>
        public bool IsGang(int missionId)
        {
            return mImpl.IsGang(missionId);
        }

        /// <summary>
        /// 任务随机种子扩展计数
        /// </summary>
        /// <returns></returns>
        public int GetRandSeedExDataId(int missionId)
        {
            return mImpl.GetRandSeedExDataId(missionId);
        }

        /// <summary>
        /// 完成次数扩展计数
        /// </summary>
        /// <returns></returns>
        public int GetTimesExDataId(int missionId)
        {
            return mImpl.GetTimesExDataId(missionId);
        }

        /// <summary>
        /// 随机狩猎任务
        /// </summary>
        /// <param name="character"></param>
        /// <param name="nId"></param>
        /// <param name="oldMisId">上次狩猎任务ID</param>
        /// <returns></returns>
        public int RandomHunterMissionId(CharacterController character, int nId, int oldMisId)
        {
            return mImpl.RandomHunterMissionId(this, character, nId, oldMisId);
        }

        public List<int> FindHunterMissions(int missionId)
        {
            return mImpl.FindHunterMissions(missionId);
        }
    }

    public interface IMissionManager
    {
        Mission Accept(MissionManager _this, CharacterController character, int nId);
        void AddFriend(IEvent ievent);
        void AdditionalEquip(IEvent ievent);
        void Arena(IEvent ievent);
        void BuyItem(IEvent ievent);
        bool CheckConditionEventMission(int nConditionId);
        ErrorCodes Commit(MissionManager _this, CharacterController character, int nId, bool isGM);
        ErrorCodes Complete(MissionManager _this, CharacterController character, int nId, bool force = false);
        void ComposeItem(IEvent ievent);
        void DepotTakeOutEvent(IEvent ievent);
        ErrorCodes Drop(MissionManager _this, int nId);
        void EnhanceEquip(IEvent ievent);
        void EnterAreaEvent(IEvent ievent);
        void EquipItem(IEvent ievent);
        void EventAddFriend(CharacterController character, int param0 = 0, int param1 = 0);
        void EventAdditionalEquip(CharacterController character, int param0 = 0, int param1 = 0);
        void EventArena(CharacterController character, int param0 = 0, int param1 = 0);
        void EventBuyItem(CharacterController character, int param0 = 0, int param1 = 0);
        void EventByEnterArea(CharacterController character, int param0 = 0, bool param1 = false);
        void EventByItemChange(CharacterController character, int param0 = 0, int param1 = 0);
        void EventByKillMonster(CharacterController character, int param0 = 0, int param1 = 1);
        void EventByTollgate(CharacterController character, int param0 = 0, int param1 = 0);
        void EventComposeItem(CharacterController character, int param0 = 0, int param1 = 0);
        void EventEnhanceEquip(CharacterController character, int param0 = 0, int param1 = 0);
        void EventEquipItemChange(CharacterController character, int param0 = 0, int param1 = 0);
        void EventNpcService(CharacterController character, int param0 = 0, int param1 = 0);
        void EventUpgradeSkill(CharacterController character, int param0 = 0, int param1 = 0);
        void ExdataAdd(CharacterController character, int exdataId, int addValue);
        void ExdataAdd(IEvent ievent);
        void ExDataChangeEvent(IEvent ievent);
        void FlagFalseEvent(IEvent ievent);
        void FlagTrueEvent(IEvent ievent);
        void FlagTrueEvent(CharacterController character, int exdataId, int addValue);
        void GetCanAcceptMission(MissionManager _this);
        Mission GetMission(MissionManager _this, int nId);
        void GetNetDirtyMissions(MissionManager _this, MissionDataMessage msg);
        void Init();
        MissionData InitByBase(MissionManager _this, CharacterController character);
        void InitByDB(MissionManager _this, CharacterController character, MissionData mission);
        void ItemChangeEvent(IEvent ievent);
        void KillMonsterEvent(IEvent ievent);
        int ModifyByLevel(MissionManager _this, int nOldValue, int nLevel);
        void NetDirtyHandle(MissionManager _this);
        void NpcServe(IEvent ievent);
        ErrorCodes SetMissionParam(MissionManager _this, CharacterController character, int nId, int nIndex, int nValue);
        MayaBaseRecord GetEraByFubenId(MissionManager _this, CharacterController character, int fubenId);
        void SkillPointChange(CharacterController character, int skillId, int value);
        void SkillPointChange(IEvent ievent);
        void TollgateEvent(IEvent ievent);
        void TollgateNextEvent(IEvent ievent);
        void TriggerMissionByEvent(CharacterController character, eEventType type, int param0 = 0, int param1 = 0);
        Mission TryAccept(MissionManager _this, CharacterController character, int nId);
        void UpgradeSkill(IEvent ievent);
        ErrorCodes RefreshHunterMission(MissionManager _this, CharacterController character);
    }

    public class MissionManagerDefaultImpl : IMissionManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Logger kafaLogger = LogManager.GetLogger(Shared.LoggerName.KafkaLog);

        public int ModifyByLevel(MissionManager _this, int nOldValue, int nLevel)
        {
            if (nOldValue < 10000000)
            {
                return nOldValue;
            }
            var tbUpgrade = Table.GetSkillUpgrading(nOldValue % 10000000);
            if (tbUpgrade == null)
            {
                return nOldValue;
            }
            return tbUpgrade.GetSkillUpgradingValue(nLevel);
        }

        public void NetDirtyHandle(MissionManager _this)
        {
            //if (mCharacter.Proxy == null)
            //{
            //    return;
            //}
            var msg = new MissionDataMessage();
            foreach (var mission in _this.Children)
            {
                if (mission.NetDirty) //脏任务
                {
                    var tempMission = (Mission)mission;
                    var tempChanges = new MissionBaseData
                    {
                        MissionId = tempMission.Id
                    };
                    tempChanges.Exdata.AddRange(tempMission.Data);
                    msg.Missions[tempMission.Id] = tempChanges;
                }
            }
            _this.mCharacter.Proxy.SyncMissions(msg);
        }

        public void GetNetDirtyMissions(MissionManager _this, MissionDataMessage msg)
        {
            foreach (var mission in _this.Children)
            {
                if (mission.NetDirty) //脏任务
                {
                    var tempMission = (Mission)mission;
                    var tempChanges = new MissionBaseData
                    {
                        MissionId = tempMission.Id
                    };
                    tempChanges.Exdata.AddRange(tempMission.Data);
                    msg.Missions[tempMission.Id] = tempChanges;
                }
            }
        }

        #region   初始化

        //初始化静态数据
        public void Init()
        {
            //初始化任务相关的静态触发数据
            Table.ForeachMission(record =>
            {
                InitOneMission(record.Id, record.Condition);
                return true;
            });

            //注册事件
            EventDispatcher.Instance.AddEventListener(ItemChange.EVENT_TYPE, MissionManager.ItemChangeEvent);
            EventDispatcher.Instance.AddEventListener(KillMonster.EVENT_TYPE, MissionManager.KillMonsterEvent);
            EventDispatcher.Instance.AddEventListener(EnterArea.EVENT_TYPE, MissionManager.EnterAreaEvent);
            EventDispatcher.Instance.AddEventListener(TollgateFinish.EVENT_TYPE, MissionManager.TollgateEvent);
            EventDispatcher.Instance.AddEventListener(ChacacterFlagTrue.EVENT_TYPE, MissionManager.FlagTrueEvent);
            EventDispatcher.Instance.AddEventListener(ChacacterFlagFalse.EVENT_TYPE, MissionManager.FlagFalseEvent);
            EventDispatcher.Instance.AddEventListener(CharacterExdataChange.EVENT_TYPE, MissionManager.ExDataChangeEvent);
            EventDispatcher.Instance.AddEventListener(BuyItemEvent.EVENT_TYPE, MissionManager.BuyItem);
            EventDispatcher.Instance.AddEventListener(EquipItemEvent.EVENT_TYPE, MissionManager.EquipItem);
            EventDispatcher.Instance.AddEventListener(EnhanceEquipEvent.EVENT_TYPE, MissionManager.EnhanceEquip);
            EventDispatcher.Instance.AddEventListener(AdditionalEquipEvent.EVENT_TYPE, MissionManager.AdditionalEquip);
            EventDispatcher.Instance.AddEventListener(UpgradeSkillEvent.EVENT_TYPE, MissionManager.UpgradeSkill);
            EventDispatcher.Instance.AddEventListener(NpcServeEvent.EVENT_TYPE, MissionManager.NpcServe);
            EventDispatcher.Instance.AddEventListener(ArenaEvent.EVENT_TYPE, MissionManager.Arena);
            EventDispatcher.Instance.AddEventListener(AddFriendEvent.EVENT_TYPE, MissionManager.AddFriend);
            EventDispatcher.Instance.AddEventListener(ComposeItemEvent.EVENT_TYPE, MissionManager.ComposeItem);
            EventDispatcher.Instance.AddEventListener(CharacterExdataAddEvent.EVENT_TYPE, MissionManager.ExdataAdd);
            EventDispatcher.Instance.AddEventListener(SkillPointChangeEvent.EVENT_TYPE, MissionManager.SkillPointChange);
            EventDispatcher.Instance.AddEventListener(CharacterDepotTakeOutEvent.EVENT_TYPE,MissionManager.DepotTakeOutEvent);
            EventDispatcher.Instance.AddEventListener(TollgateNextFinish.EVENT_TYPE, MissionManager.TollgateNextEvent);

        }

        public void ExDataChangeEvent(IEvent ievent)
        {
            var ee = ievent as CharacterExdataChange;
            TriggerMissionByEvent(ee.character, eEventType.ExDataChange, ee.ExdataId);
            //这里会响应每日活跃度
            if (ee.ExdataValue > 0)
            {
                List<DailyActivityRecord> temp;
                if (StaticParam.TriggerActivity.TryGetValue(ee.ExdataId, out temp))
                {
                    ee.character.CheckDailyActivity(temp);
                }
            }
            //任务目标18
            foreach (var mission in ee.character.mTask.mData)
            {
                var mis = mission.Value;
                if (mis.Data[0] != 0 || (mis.IsGang(mis.Id) && ee.character.mAlliance.AllianceId <= 0))
                {
                    continue; //完成状态判断
                }
                if (mis.Data[1] != (int)eMissionType.ExDataChange)
                {
                    continue; //目标判断
                }
                if (mis.Data[3] != ee.ExdataId)
                {
                    continue; //ID判断
                }
                //数据处理
                mis.Data[2] = ee.ExdataValue;
                if (mis.Data[2] >= mis.Data[4])
                {
                    mis.Data[0] = 1;
                }
                //ee.character.mTask.mFlag = true;
                //mis.mFlag = true;
                mis.MarkDirty();
            }
        }

        public void FlagFalseEvent(IEvent ievent)
        {
            var ee = ievent as ChacacterFlagFalse;
            TriggerMissionByEvent(ee.character, eEventType.Falseflag, ee.FlagId);
        }

        public void FlagTrueEvent(IEvent ievent)
        {
            var ee = ievent as ChacacterFlagTrue;
            TriggerMissionByEvent(ee.character, eEventType.Trueflag, ee.FlagId);
            FlagTrueEvent(ee.character, ee.FlagId, 1);
        }

        public void ItemChangeEvent(IEvent ievent)
        {
            var ee = ievent as ItemChange;
            EventByItemChange(ee.character, ee.mItemId, ee.mItemCount);
            if (ee.mItemId == 0)
            {
                TriggerMissionByEvent(ee.character, eEventType.ItemChange, ee.mItemId, ee.mItemCount);
            }
        }

        public void KillMonsterEvent(IEvent ievent)
        {
            var ee = ievent as KillMonster;
            EventByKillMonster(ee.character, ee.mMonsterId);
            //TriggerMissionByEvent(ee.character, eEventType.KillMonster, ee.mMonsterId);
        }

        public void EnterAreaEvent(IEvent ievent)
        {
            var ee = ievent as EnterArea;
            EventByEnterArea(ee.character, ee.mAreaId, ee.mIsEnter);
            //TriggerMissionByEvent(ee.character, eEventType.EnterArea, ee.mAreaId);
        }

        public void TollgateEvent(IEvent ievent)
        {
            var ee = ievent as TollgateFinish;
            EventByTollgate(ee.character, ee.TollgateId);
            //TriggerMissionByEvent(ee.character, eEventType.Tollgate, ee.TollgateId);
        }
        public void TollgateNextEvent(IEvent ievent)
        {
            var ee = ievent as TollgateNextFinish;
            EventByNextTollgate(ee.character, ee.TollgateId);
            //TriggerMissionByEvent(ee.character, eEventType.Tollgate, ee.TollgateId);
        }

        public void BuyItem(IEvent ievent)
        {
            var ee = ievent as BuyItemEvent;
            EventBuyItem(ee.character, ee.ItemId, ee.ItemCount);
        }

        public void EquipItem(IEvent ievent)
        {
            var ee = ievent as EquipItemEvent;
            EventEquipItemChange(ee.character, ee.EquipPart);
        }

        public void EnhanceEquip(IEvent ievent)
        {
            var ee = ievent as EnhanceEquipEvent;
            EventEnhanceEquip(ee.character, ee.EquipPart);
        }

        public void AdditionalEquip(IEvent ievent)
        {
            var ee = ievent as AdditionalEquipEvent;
            EventAdditionalEquip(ee.character, ee.EquipPart);
        }

        public void UpgradeSkill(IEvent ievent)
        {
            var ee = ievent as UpgradeSkillEvent;
            EventUpgradeSkill(ee.character, ee.SkillID);
        }

        public void NpcServe(IEvent ievent)
        {
            var ee = ievent as NpcServeEvent;
            EventNpcService(ee.character, ee.NpcServeID);
        }

        public void Arena(IEvent ievent)
        {
            var ee = ievent as ArenaEvent;
            EventArena(ee.character);
        }

        public void AddFriend(IEvent ievent)
        {
            var ee = ievent as AddFriendEvent;
            EventAddFriend(ee.character);
        }

        public void ComposeItem(IEvent ievent)
        {
            var ee = ievent as ComposeItemEvent;
            EventComposeItem(ee.character, ee.ComposeId, ee.ItemId);
        }

        public void ExdataAdd(IEvent ievent)
        {
            var ee = ievent as CharacterExdataAddEvent;
            ExdataAdd(ee.character, ee.ExdataId, ee.AddValue);
        }


        public void SkillPointChange(IEvent ievent)
        {
            var ee = ievent as SkillPointChangeEvent;
            SkillPointChange(ee.character, ee.SkillId, ee.Value);
        }

        public void DepotTakeOutEvent(IEvent ievent)
        {
            var ee = ievent as CharacterDepotTakeOutEvent;
            DepotTakeOut(ee.character, ee.itemID, ee.count);
        }

        //构造所有条件影响的任务(当某个条件被触发时，可以快速知道哪个任务可以接了)
        private void InitOneMission(int missionid, int conditionid)
        {
            List<int> tempList;
            if (MissionManager.TriggerMission.TryGetValue(conditionid, out tempList))
            {
                tempList.Add(missionid);
            }
            else
            {
                tempList = new List<int> { missionid };
                MissionManager.TriggerMission[conditionid] = tempList;
            }
        }

        //创建时的初始化
        public MissionData InitByBase(MissionManager _this, CharacterController character)
        {
            var dbData = new MissionData();
            _this.mDbData = dbData;
            _this.mCharacter = character;
            //_this.mFlag = true;

            /* 修改任务接受，全自动接0
            if (character.GetRole() == 2)
            {
                Accept(_this,character, 50);
            }
            else
            {
                Accept(_this,character, 0);
            }
			 * */
            Accept(_this, character, 0);
            _this.MarkDirty();
            return dbData;
        }

        public void InitByDB(MissionManager _this, CharacterController character, MissionData mission)
        {
            _this.mCharacter = character;
            _this.mDbData = mission;
            var delList = new List<int>();
            foreach (var dbMission in mission.Missions)
            {
                var tbmis = Table.GetMission(dbMission.Key);
                if (tbmis == null)
                {
                    delList.Add(dbMission.Key);
                    continue;
                }
                if (_this.mCharacter.CheckCondition(tbmis.Condition) != -2)
                {
                    delList.Add(dbMission.Key);
                    continue;
                }
                var mis = new Mission(character, dbMission.Value);
                _this.mData.Add(dbMission.Key, mis);
                _this.AddChild(mis);
            }
            foreach (var i in delList)
            {
                mission.Missions.Remove(i);
            }
            //_this.mFlag = false;
            GetCanAcceptMission(_this);
        }

        #endregion

        #region   常用接口(接受，放弃，完成,提交）

        //检查有没有新的可接任务
        public void GetCanAcceptMission(MissionManager _this)
        {
            //初始化任务相关的静态触发数据
            Table.ForeachMission(record =>
            {
                //if (record.ViewType < 0 || record.ViewType > 1) return true;  //只有主线，和支线任务会增加到可接列表
                if (record.FlagId < 0)
                {
                    if (record.MutexID == -1)
                    {
                        Logger.Warn("Mission[{0}] Flag is -1", record.Id);
                    }
                    return true;
                }
                if (_this.mCharacter.GetFlag(record.FlagId))
                {
                    return true; //只有没完成过的任务会显示在可接列表
                }
                TryAccept(_this, _this.mCharacter, record.Id);
                //mCharacter.
                return true;
            });
        }

        //看任务是否可接
        public Mission TryAccept(MissionManager _this, CharacterController character, int nId)
        {
            var tbMission = Table.GetMission(nId);
            if (tbMission == null)
            {
                Logger.Debug(string.Format("TryAccept Mission Error! Id={0}", nId));
                return null;
            }
            if (tbMission.FlagId < 0)
            {
                //Logger.Warn("Mission[{0}] Flag is -1", tbMission.FlagId);
                return null;
            }
            if (character.GetFlag(tbMission.FlagId))
            {
                return null; //只有没完成过的任务会显示在可接列表
            }
            Mission mis;
            if (_this.mData.TryGetValue(nId, out mis))
            {
                //Logger.Info(string.Format("TryAccept Mission Haved！Id={0}", nId));
                return null;
            }
            if (character.CheckCondition(tbMission.Condition) != -2)
            {
                return null; //接受条件不满足
            }
            //如果任务时5类型的，检查是否有相同ID
            if (tbMission.MutexID != -1)
            {
                foreach (var mission in _this.mData)
                {
                    var tbMis = Table.GetMission(mission.Key);
                    if (tbMis.MutexID == tbMission.MutexID)
                    {
                        return null;
                    }
                }
            }
            //增加任务
            mis = new Mission(character, nId);
            if (mis.mDbData == null)
            {
                Logger.Error(string.Format("TryAccept Mission Error!mis.mDbData is null Id={0}", nId));
                return null;
            }

            //防止有任务会重，如果原来包含就删除掉
            if (_this.mDbData.Missions.ContainsKey(mis.Id))
            {
                _this.mDbData.Missions.Remove(mis.Id);
                Logger.Error("TryAccept  _this.mDbData.Missions.ContainsKey({0})", mis.Id);
            }

            _this.mDbData.Missions.Add(mis.Id, mis.mDbData);
            if (tbMission.ViewType == 2 ||
                tbMission.ViewType == 5)
            {
                if (mis.Data[0] != 1)
                {
                    mis.Data[0] = 0;
                }
            }
            else
            {
                mis.Data[0] = 3;
            }
            PlayerLog.DataLog(character.mGuid, "ma,{0},{1}", mis.Id, 0);
            _this.AddChild(mis);
            _this.mData[mis.Id] = mis;
            //_this.mFlag = true;
            mis.MarkDirty();
            return mis;
        }

        //接受任务
        public Mission Accept(MissionManager _this, CharacterController character, int nId)
        {
            var tbMission = Table.GetMission(nId);
            if (tbMission == null)
            {
                Logger.Debug(string.Format("Accept Mission Error! Id={0}", nId));
                return null;
            }
            if (tbMission.MutexID != -1)
            {
                return null;
            }
            if (tbMission.FlagId < 0)
            {
                Logger.Warn("Mission[{0}] Flag is -1", tbMission.FlagId);
                return null;
            }
            if (character.GetFlag(tbMission.FlagId))
            {
                return null; //只有没完成过的任务会显示在可接列表
            }
            Mission mis;
            if (_this.mData.TryGetValue(nId, out mis))
            {
                if (mis.Data[0] != 3)
                {
                    Logger.Warn(string.Format("Accept Mission Haved！Id={0}", nId));
                    return null;
                }
                mis.Init(character, Table.GetMission(mis.Id));
                //给予接任务的buff
                if (tbMission.BuffClean != -1)
                {
                    CharacterController.mImpl.AddBuff(character, tbMission.BuffClean, 1);
                }
            }
            else
            {
                if (character.CheckCondition(tbMission.Condition) != -2)
                {
                    return null; //接受条件不满足
                }
                mis = new Mission(character, nId); //如果是环任务，有可能会修改其ID
                if (mis.mDbData == null)
                {
                    Logger.Error(string.Format("TryAccept Mission Error!mis.mDbData is null Id={0}", nId));
                    return null;
                }
                _this.mDbData.Missions.Add(mis.Id, mis.mDbData);
                _this.AddChild(mis);
                _this.mData[mis.Id] = mis;
            }
            PlayerLog.DataLog(character.mGuid, "ma,{0},{1}", mis.Id, 1);
            if (mis != null)
            {
                try
                {
                    var missionAccept = string.Format("task#{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                        character.mGuid,
                        mis.Id,
                        tbMission.Name,
                        tbMission.ViewType,
                        0,
                        "",
                        DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
                        );
                    kafaLogger.Info(missionAccept);
                }
                catch (Exception e)
                {
                    Logger.Error(string.Format("kafka mission commit err! Id={0}", e));
                }
            }

            //_this.mFlag = true;
            mis.MarkDirty();
            mis.CleanNetDirty();

            //---------------------防止任务次数不够
            if (mis.Id == 517)//121 517
            {//机关坊任务
                if (_this.mCharacter.GetExData(448) >= 1)
                {
                    _this.mCharacter.SetExData(448, 0);
                }
            }
            // 			else if (mis.Id == 110)
            // 			{//暗夜神殿
            // 				if (_this.mCharacter.GetExData(100) >= 1)
            // 				{
            // 					_this.mCharacter.SetExData(100, 0);
            // 				}
            // 			}
            else if (mis.Id == 115)
            {//丛林石窟
                if (_this.mCharacter.GetExData(112) >= 1)
                {
                    _this.mCharacter.SetExData(112, 0);
                }
            }
            else if (mis.Id == 119)
            {//地狱监牢
                if (_this.mCharacter.GetExData(300) >= 1)
                {
                    _this.mCharacter.SetExData(300, 0);
                }
            }
            else if (mis.Id == 141)
            {//幻雪圣殿
                if (_this.mCharacter.GetExData(124) >= 1)
                {
                    _this.mCharacter.SetExData(124, 0);
                }
            }
            else if (mis.Id == 155)
            {//亡灵城堡
                if (_this.mCharacter.GetExData(299) >= 1)
                {
                    _this.mCharacter.SetExData(299, 0);
                }
            }
            else if (mis.Id == 412)
            {//首位大陆
                if (_this.mCharacter.GetExData(446) >= 1)
                {
                    _this.mCharacter.SetExData(446, 0);
                }
            }
            //else if (mis.Id == StaticParam.MissionIdToOpenBook)
            //{
            //    _this.mCharacter.SetExData((int)eExdataDefine.e710, 0);
            //    // 特殊处理
            //    MayaBaseRecord tbMaya;
            //    if (StaticParam.EraMissionDict.TryGetValue(mis.Id, out tbMaya))
            //    {
            //        if (tbMaya.FlagId >= 0)
            //        {
            //            character.SetFlag(tbMaya.FlagId);
            //        }
            //    }
            //}
            return mis;
        }

        //放弃任务
        public ErrorCodes Drop(MissionManager _this, int nId)
        {
            Mission mis;
            if (!_this.mData.TryGetValue(nId, out mis))
            {
                Logger.Error(string.Format("Drop Mission Error! Id={0}", nId));
                return ErrorCodes.Error_NotHaveMission;
            }
            //判断放弃条件
            //执行放弃操作
            //_this.mFlag = true;
            mis.MarkDirty();
            _this.mData.Remove(nId);
            _this.mDbData.Missions.Remove(nId);
            PlayerLog.DataLog(_this.mCharacter.mGuid, "md,{0},{1}", nId, 0);
            return ErrorCodes.OK;
        }

        //完成任务
        public ErrorCodes Complete(MissionManager _this, CharacterController character, int nId, bool force = false)
        {
            Mission mis;
            if (!_this.mData.TryGetValue(nId, out mis))
            {
                Logger.Error(string.Format("Complete Mission Error! Id={0}", nId));
                return ErrorCodes.Error_NotHaveMission;
            }
            //表格检查
            var tbmis = Table.GetMission(nId);
            if (tbmis == null)
            {
                Logger.Error(string.Format("Complete Mission not find from table! Id={0}", nId));
                _this.mData.Remove(nId);
                _this.mDbData.Missions.Remove(nId);
                return ErrorCodes.Error_MissionID;
            }
            //判断完成条件
            if (!CheckFinish(character, mis, force))
            {
                return ErrorCodes.Error_ConditionNoEnough;
            }
            mis.Data[0] = (int)eMissionState.Finished;
            //_this.mFlag = true;
            mis.MarkDirty();

            CheckEraLearnSkill(character, nId);
            CheckEraChange(character, nId);

            return ErrorCodes.OK;
        }

        //提交任务
        public ErrorCodes Commit(MissionManager _this, CharacterController character, int nId, bool isGM)
        {
            Mission mis;
            if (!_this.mData.TryGetValue(nId, out mis))
            {
                Logger.Error(string.Format("Commit Mission Error! Id={0}", nId));
                return ErrorCodes.Error_NotHaveMission;
            }
            //表格检查

            var tbmis = Table.GetMission(nId);
            if (tbmis == null)
            {
                Logger.Error(string.Format("Complete Mission not find from table! Id={0}", nId));
                _this.mData.Remove(nId);
                _this.mDbData.Missions.Remove(nId);
                _this.MarkDirty();
                return ErrorCodes.Error_MissionID;
            }

            //判断完成条件
            if (!isGM)
            {
                if (!CheckFinish(character, mis))
                {
                    return ErrorCodes.Error_ConditionNoEnough;
                }
            }

            if (mis.IsGang(mis.Id) && character.mAlliance.AllianceId <= 0)
            {
                return ErrorCodes.Error_CharacterNoAlliance;
            }
            //给予完成任务的buff
            if (tbmis.BuffAdd != -1)
            {
                CharacterController.mImpl.AddBuff(character, tbmis.BuffAdd, 1);
            }
            //给予奖励
            var roleId = character.GetRole();
            if (roleId < 0 || roleId > 2)
            {
                return ErrorCodes.RoleIdError;
            }
            var items = new Dictionary<int, int>();

            for (var i = 0; i != 2; ++i)
            {
                if (tbmis.RoleRewardId[roleId, i] < 0 || tbmis.RoleRewardCount[roleId, i] <= 0)
                {
                    continue;
                }
                items.modifyValue(tbmis.RoleRewardId[roleId, i], tbmis.RoleRewardCount[roleId, i]);
                //character.mBag.AddItem(tbmis.RoleRewardId[roleId, i], tbmis.RoleRewardCount[roleId, i]);
            }
            for (var i = 0; i != 3; ++i)
            {
                if (tbmis.RewardItem[i] < 0 || tbmis.RewardItemCount[i] <= 0)
                {
                    continue;
                }
                var itemCount = ModifyByLevel(_this, tbmis.RewardItemCount[i], _this.mCharacter.GetLevel());
                items.modifyValue(tbmis.RewardItem[i], itemCount);
                //character.mBag.AddItem(tbmis.RewardItem[i], tbmis.RewardItemCount[i]);
            }

            var result = BagManager.CheckAddItemList(character.mBag, items);
            if (result != ErrorCodes.OK)
            {
                return result;
            }

            //完成数据处理
            character.SetFlag(tbmis.FlagId);
            CheckEraLearnSkill(character, nId);
            CheckEraChange(character, tbmis.Id);

            foreach (var item in items)
            {
                character.mBag.AddItem(item.Key, item.Value, eCreateItemType.MissionSubmit);
            }
            if (tbmis.IsDynamicExp == 1)
            {
                character.mBag.AddExp(
                    (int)(tbmis.DynamicExpRatio / 10000.0f * Table.GetLevelData(character.GetLevel()).DynamicExp),
                    eCreateItemType.MissionSubmit);
            }
            //删除道具
            if ((eMissionType)mis.Data[1] == eMissionType.CheckItem)
            {
                character.mBag.DeleteItem(mis.Data[3], mis.Data[4], eDeleteItemType.MissionSubmit);
            }
            if (tbmis.DeleteItem.Count > 0)
            {
                if (tbmis.DeleteItem.Count == 1 && tbmis.DeleteItem[0] > 0)
                {
                    var tbItem = Table.GetItemBase(tbmis.DeleteItem[0]);
                    if (tbItem != null)
                    {
                        if (tbItem.AutoUse > 0 && tbItem.Type == 15000)
                        {
                            //暂时只针对坐骑
                            character.mMount.DeleteMount(tbItem.Exdata[2]);
                            character.Mount(0);
                            character.Proxy.SendMountData(character.mMount.GetMountData());
                        }
                        else
                            character.mBag.DeleteItem(tbmis.DeleteItem[0], 1, eDeleteItemType.MissionSubmit);
                    }
                }
                else
                {
                    if (character.GetRole() < tbmis.DeleteItem.Count)
                    {
                        var itemId = tbmis.DeleteItem[character.GetRole()];
                        if (itemId > 0)
                        {
                            if (ErrorCodes.OK != character.mBag.DeleteItem(itemId, 1, eDeleteItemType.MissionSubmit)
                                && false == character.DeleteEquip(itemId, (int) eDeleteItemType.MissionSubmit))
                            {//身上没有找仓库
                                character.mBag.mBags[(int) eBagType.Depot].ForceDeleteItem(itemId, 1);
                            }
                        }

                    }
                }

            }

            //删除任务
            //_this.mFlag = true;
            if (tbmis.MutexID != -1)
            {
                _this.mDbData.OldPerMissions[tbmis.MutexID] = tbmis.Id;
            }
            mis.MarkDbDirty();
            _this.mData.Remove(nId);
            _this.mDbData.Missions.Remove(nId);
            //配置的扩展计数
            if (tbmis.ExdataId > 0)
            {
                character.AddExData(tbmis.ExdataId, tbmis.ExdataValue);
            }
            if (tbmis.ViewType == 2 || tbmis.ViewType == 6 || tbmis.ViewType == (int)eMissionMainType.Farm)
            {
                character.AddExData((int)eExdataDefine.e20, 1);
                character.AddExData((int)eExdataDefine.e2, 1);
            }
            else
            {
                character.AddExData((int)eExdataDefine.e1, 1);
            }

            if (roleId >= 0 && roleId < tbmis.GetSkill.Count)
            {
                var skillId = tbmis.GetSkill[roleId];
                if (skillId >= 0)
                {
                    character.mSkill.LearnSkill(skillId, 1);
                }
            }
            //PlayerLog.WriteLog(character.mGuid, "----------CommitMission----------MissionId={0}", nId);
            //潜规则 学会某技能
            //if (nId == 6)
            //{
            //    if (character.GetRole() == 0)
            //    {
            //        character.mSkill.LearnSkill(4, 1);
            //    }
            //    else if (character.GetRole() == 1)
            //    {
            //        character.mSkill.LearnSkill(104, 1);
            //    }
            //    else if (character.GetRole() == 2)
            //    {
            //        character.mSkill.LearnSkill(204, 1);
            //    }
            //}
            //else if (nId == 107)
            //{
            //    if (character.GetRole() == 0)
            //    {
            //        character.mSkill.LearnSkill(6, 1);
            //    }
            //    else if (character.GetRole() == 1)
            //    {
            //        character.mSkill.LearnSkill(106, 1);
            //    }
            //    else if (character.GetRole() == 2)
            //    {
            //        character.mSkill.LearnSkill(207, 1);
            //    }
            //}

            try
            {
                _this.mCharacter.mOperActivity.OnCommitMission(tbmis.ViewType);
            }
            catch (Exception e)
            {
                Logger.Error("mission commit err! Id={0}\n{1}", e, e.StackTrace);
            }

            //else if (nId == 55)
            //{
            //    if (character.GetRole() == 2)
            //    {
            //        character.mSkill.LearnSkill(204, 1);
            //    }
            //}
            PlayerLog.DataLog(character.mGuid, "md,{0},{1}", nId, 1);

            try
            {
                var missionCommit = string.Format("task#{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                    character.mGuid,
                    mis.Id,
                    tbmis.Name,
                    tbmis.ViewType,
                    1,
                    "",
                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
                    );
                kafaLogger.Info(missionCommit);
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("kafka mission commit err! Id={0}", e));
            }



            return ErrorCodes.OK;
        }

        private void CheckEraChange(CharacterController character, int missionId)
        {
            MayaBaseRecord tbMaya;
            if (StaticParam.EraMissionDict.TryGetValue(missionId, out tbMaya))
            {
                if (tbMaya.FinishFlagId == -1 || character.GetFlag(tbMaya.FinishFlagId))
                    return;

                character.SetFlag(tbMaya.FinishFlagId, true);
                if (tbMaya.FlagId >= 0)
                {
                    character.SetFlag(tbMaya.FlagId, true, 1);
                    character.SetExData((int)eExdataDefine.e711, tbMaya.Id);
                }
                if (tbMaya.ActiveType == 0 && tbMaya.Type == 0) // 只有主线任务才改变
                {
                    var oldValue = character.GetExData((int)eExdataDefine.e710);
                    if (oldValue >= 0)
                    {
                        character.SetExData((int)eExdataDefine.e710, tbMaya.NextId);
                    }
                }
            }
        }
        private void SaveMayaExData(CharacterController character, int missionId)
        {
            MayaBaseRecord tbMaya;
            if (StaticParam.EraMissionDict.TryGetValue(missionId, out tbMaya))
            {
                if (tbMaya.FinishFlagId == -1 || character.GetFlag(tbMaya.FinishFlagId))
                    return;

                var roleId = character.GetRole();
                if (roleId >= 0 && roleId < tbMaya.SkillIds.Count)
                {
                    var skillId = tbMaya.SkillIds[roleId];
                    if (skillId >= 0 && !character.mSkill.HaveSkill(skillId))
                    {
                        character.SetExData((int)eExdataDefine.e711, tbMaya.Id);
                    }
                }
            }
        }
        private void CheckEraLearnSkill2(CharacterController character, int missionId)
        {
            MayaBaseRecord tbMaya;
            if (StaticParam.EraMissionDict.TryGetValue(missionId, out tbMaya))
            {
                if (tbMaya.FinishFlagId == -1 || character.GetFlag(tbMaya.FinishFlagId))
                    return;

                var roleId = character.GetRole();
                if (roleId >= 0 && roleId < tbMaya.SkillIds.Count)
                {
                    var skillId = tbMaya.SkillIds[roleId];
                    if (skillId >= 0 && !character.mSkill.HaveSkill(skillId))
                    {
                        character.mSkill.LearnSkill(skillId, 1);
                    }
                }
            }
        }

        private void CheckEraLearnSkill(CharacterController character, int missionId)
        {
            MayaBaseRecord tbMaya;
            if (StaticParam.EraMissionDict.TryGetValue(missionId, out tbMaya))
            {
                if (tbMaya.FinishFlagId == -1 || character.GetFlag(tbMaya.FinishFlagId))
                    return;

                var roleId = character.GetRole();
                if (roleId >= 0 && roleId < tbMaya.SkillIds.Count)
                {
                    var skillId = tbMaya.SkillIds[roleId];
                    if (skillId >= 0 && !character.mSkill.HaveSkill(skillId))
                    {
                        character.mSkill.LearnSkill(skillId, 1);
                        character.SetExData((int)eExdataDefine.e711, tbMaya.Id);
                    }
                }
            }
        }

        //获得任务
        public Mission GetMission(MissionManager _this, int nId)
        {
            Mission mis;
            if (_this.mData.TryGetValue(nId, out mis))
            {
                return mis;
            }
            return null;
        }

        public ErrorCodes SetMissionParam(MissionManager _this,
                                          CharacterController character,
                                          int nId,
                                          int nIndex,
                                          int nValue)
        {
            Mission mis;
            if (!_this.mData.TryGetValue(nId, out mis))
            {
                Logger.Error(string.Format("SetMissionParam Mission Error! Id={0}", nId));
                return ErrorCodes.Error_NotHaveMission;
            }
            //表格检查
            var tbmis = Table.GetMission(nId);
            if (tbmis == null)
            {
                Logger.Error(string.Format("Complete Mission not find from table! Id={0}", nId));
                _this.mData.Remove(nId);
                _this.mDbData.Missions.Remove(nId);
                return ErrorCodes.Error_MissionID;
            }
            if (nIndex < 0 || nIndex > 4)
            {
                return ErrorCodes.Unknow;
            }
            mis.Data[nIndex] = nValue;
            //_this.mFlag = true;
            mis.MarkDirty();
            return ErrorCodes.OK;
        }

        // 查找当前狩猎任务
        private Mission FindHunterMission(MissionManager _this)
        {
            foreach (var kv in _this.mData)
            {
                var missionId = kv.Key;
                var mission = kv.Value;
                var tbMis = Table.GetMission(missionId);
                if (tbMis != null && tbMis.ViewType == (int)eMissionMainType.Farm)
                {
                    return mission;
                }
            }

            return null;
        }

        // 查找当前等级可接取的狩猎母任务
        private int FindCanAcceptHunterMission(int charLevel)
        {
            var tbSkillUpgrading = Table.GetSkillUpgrading(73010);
            if (tbSkillUpgrading == null)
                return -1;

            foreach (var misId in tbSkillUpgrading.Values)
            {
                var tbM = Table.GetMission(misId);
                if (tbM == null)
                    continue;

                var tbCondition = Table.GetConditionTable(tbM.Condition);
                if (tbCondition == null)
                    continue;

                for (var i = 0; i < tbCondition.ItemId.Length; ++i)
                { // 判断等级满足
                    if (tbCondition.ItemId[i] == 0
                        && charLevel >= tbCondition.ItemCountMin[i]
                        && charLevel <= tbCondition.ItemCountMax[i])
                    {
                        return misId;
                    }
                }
            }

            return -1;
        }

        // 刷新狩猎任务
        public ErrorCodes RefreshHunterMission(MissionManager _this, CharacterController character)
        {
            var hunterMission = FindHunterMission(_this);
            if (hunterMission == null)
                return ErrorCodes.Error_NotHaveMission;

            var errorCodes = _this.Drop(hunterMission.Id);
            if (errorCodes != ErrorCodes.OK)
                return errorCodes;

            var curMisId = FindCanAcceptHunterMission(character.GetLevel());
            if (curMisId < 0)
                return ErrorCodes.Error_NotHaveMission;

            var tbMission = Table.GetMission(curMisId);
            if (tbMission == null)
            {
                return ErrorCodes.Error_MissionID;
            }

            //增加任务
            var mis = new Mission(character, curMisId);
            if (mis.mDbData == null)
            {
                Logger.Error("TryAccept Mission Error!mis.mDbData is null Id={0}", curMisId);
                return ErrorCodes.Error_AcceptMission;
            }

            //防止有任务会重，如果原来包含就删除掉
            if (_this.mDbData.Missions.ContainsKey(mis.Id))
            {
                _this.mDbData.Missions.Remove(mis.Id);
                Logger.Error("TryAccept  _this.mDbData.Missions.ContainsKey({0})", mis.Id);
            }

            _this.mDbData.Missions.Add(mis.Id, mis.mDbData);
            if (tbMission.ViewType == 2 || tbMission.ViewType == 5)
            {
                if (mis.Data[0] != 1)
                {
                    mis.Data[0] = 0;
                }
            }
            else
            {
                mis.Data[0] = 3;
            }
            PlayerLog.DataLog(character.mGuid, "ma,{0},{1}", mis.Id, 0);
            _this.AddChild(mis);
            _this.mData[mis.Id] = mis;
            mis.MarkDirty();

            return ErrorCodes.OK;
        }

        #endregion

        #region

        public MayaBaseRecord GetEraByFubenId(MissionManager _this, CharacterController character, int fubenId)
        {
            MayaBaseRecord mayaBase;
            if (StaticParam.EraFubenDict.TryGetValue(fubenId, out mayaBase))
            {
                return mayaBase;
            }

            return null;
        }

        #endregion

        #region   任务事件

        public void TriggerMissionByEvent(CharacterController character, eEventType type, int param0 = 0, int param1 = 0)
        {
            //获得该事件影响的条件
            var conlist = ConditionManager.EventTriggerCondition(type, param0);
            if (conlist == null)
            {
                return;
            }
            MissionManager.tasklist.Clear();
            //整理这些条件影响了哪些任务
            foreach (var i in conlist)
            {
                ConditionMission(character, MissionManager.tasklist, i.Key);
            }
            //尝试接受这些任务
            foreach (var i in MissionManager.tasklist)
            {
                character.mTask.TryAccept(character, i.Key);
            }
        }

        //查看条件是否完成，然后影响到任务的
        private void ConditionMission(CharacterController character, Dictionary<int, int> tasklist, int nConId)
        {
            //看这个条件是否有影响的任务
            List<int> tempList;
            if (!MissionManager.TriggerMission.TryGetValue(nConId, out tempList))
            {
                return; //这个条件没有影响任务
            }
            //条件是否完成了
            if (character.CheckCondition(nConId) != -2)
            {
                return; //没有完成
            }
            //整理这些任务
            foreach (var missionid in tempList)
            {
                tasklist[missionid] = 1;
            }
        }

        //资源变化事件
        public void EventByItemChange(CharacterController character, int param0 = 0, int param1 = 0)
        {
            foreach (var mission in character.mTask.mData)
            {
                var mis = mission.Value;
                if (mis.Data[0] != 0 || (mis.IsGang(mis.Id) && character.mAlliance.AllianceId <= 0))
                {
                    continue; //不是未完成状态
                }
                if (mis.Data[1] == (int)eMissionType.CheckItem)
                {
                    if (mis.Data[3] != param0)
                    {
                        continue; //资源ID不一致
                    }
                    if (param1 == 0)
                    {
                        continue;
                    }
                    //数据处理
                    var nNowCount = mis.Data[2];
                    mis.Data[2] = nNowCount + param1;
                    if (mis.Data[2] >= mis.Data[4])
                    {
                        mis.Data[0] = 1;
                    }
                    //character.mTask.mFlag = true;
                    //mis.mFlag = true;
                }
                if (mis.Data[1] == (int)eMissionType.BuyItem)
                {
                    if (mis.Data[3] != param0)
                    {
                        continue;
                    }
                    mis.Data[0] = 1;
                    //character.mTask.mFlag = true;
                    //mis.mFlag = true;
                    mis.MarkDirty();
                }
                if (mis.Data[1] == (int)eMissionType.ObtainItem)
                {
                    if (mis.Data[3] != param0)
                    {
                        continue; //资源ID不一致
                    }
                    if (param1 <= 0)
                    {
                        continue;
                    }
                    //数据处理
                    var nNowCount = mis.Data[2];
                    mis.Data[2] = nNowCount + param1;
                    if (mis.Data[2] >= mis.Data[4])
                    {
                        mis.Data[0] = 1;
                    }
                    //character.mTask.mFlag = true;
                    //mis.mFlag = true;
                    mis.MarkDirty();
                }
            }
        }

        //杀怪事件
        public void EventByKillMonster(CharacterController character, int param0 = 0, int param1 = 1)
        {
            foreach (var mission in character.mTask.mData)
            {
                var mis = mission.Value;
                if (mis.Data[0] != (int)eMissionState.Unfinished)
                {
                    continue; //不是未完成状态
                }
                if (mis.Data[1] == (int)eMissionType.KillMonster)
                {
                    if (mis.Data[3] != param0)
                    {
                        continue; //怪物ID不一致
                    }
                    if (param1 <= 0)
                    {
                        continue;
                    }
                    //杀怪任务的数据处理
                    var nNowCount = mis.Data[2];
                    mis.Data[2] = nNowCount + param1;
                    if (mis.Data[2] >= mis.Data[4])
                    {
                        mis.Data[0] = 1;
                    }
                    mis.MarkDirty();
                }
                //激活掉落任务
                else if ((int)eMissionType.CheckItem == mis.Data[1])
                {
                    if (character.mBag.GetItemCount(mis.Data[3]) < mis.Data[4])
                    {
                        var tbMis = Table.GetMission(mis.Id);
                        for (var i = 0; i < tbMis.DropMonsterId.Length; i++)
                        {
                            if (MyRandom.Random(10000) < tbMis.DropPro[i])
                            {
                                if (tbMis.DropMonsterId[i] == param0)
                                {
                                    var result = character.mBag.AddItem(tbMis.DropId[i], 1,
                                        eCreateItemType.MissionActivate);
                                    if (result == ErrorCodes.OK)
                                    {
                                        mis.MarkDirty();
                                    }
                                    else
                                    {
                                        if (character.Proxy != null)
                                        {
                                            character.Proxy.LogicNotifyMessage((int)eLogicNotifyType.BagFull, "302", 0);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //区域事件
        public void EventByEnterArea(CharacterController character, int param0 = 0, bool param1 = false)
        {
            foreach (var mission in character.mTask.mData)
            {
                var mis = mission.Value;
                if (mis.Data[0] != 0 || (mis.IsGang(mis.Id) && character.mAlliance.AllianceId <= 0))
                {
                    continue; //不是未完成状态
                }
                if (mis.Data[3] != param0)
                {
                    continue; //区域ID不一致
                }
                if (!param1)
                {
                    continue; //不是进入区域
                }
                if (mis.Data[1] != (int)eMissionType.AreaProgressBar && mis.Data[1] != (int)eMissionType.AreaStroy)
                {
                    continue; //不是区域的不管
                }
                //数据处理
                mis.Data[0] = 1;
                //character.mTask.mFlag = true;
                //mis.mFlag = true;
                mis.MarkDirty();
            }
        }

        //关卡事件
        public void EventByTollgate(CharacterController character, int param0 = 0, int param1 = 0)
        {
            character.SetExData((int)eExdataDefine.e0, param0);
            //标记信息
            foreach (var mission in character.mTask.mData)
            {
                var mis = mission.Value;
                if (mis.Data[0] != 0 || (mis.IsGang(mis.Id) && character.mAlliance.AllianceId <= 0))
                {
                    continue; //不是未完成状态
                }
                if (mis.Data[1] != (int)eMissionType.Tollgate && mis.Data[1] != (int)eMissionType.Dungeon)
                {
                    continue; //不是关卡的不管
                }
                if (mis.Data[3] != -1)
                {

                    if (mis.Data[3] != param0)
                    {
                        bool ok = false;
                        Table.ForeachDynamicActivity((tb) =>
                        {
                            if (tb.FuBenID.Contains(mis.Data[3]))
                            {
                                if (tb.FuBenID.Contains(param0))
                                {
                                    ok = true;
                                }
                                return false;
                            }
                            return true;
                        });

                        if (!ok)
                        {
                            continue; //关卡ID不一致    
                        }
                    }
                }
                //数据处理
                mis.Data[2]++;
                if (mis.Data[2] >= mis.Data[4])
                {
                    character.SetExData((int)eExdataDefine.e648, mis.Id);
                    SaveMayaExData(character, mis.Id);
                    //  CheckEraLearnSkill(character, mis.Id);
                    //  CheckEraChange(character, mis.Id);
                    mis.Data[0] = (int)eMissionState.Finished;
                }
                //character.mTask.mFlag = true;
                //mis.mFlag = true;
                mis.MarkDirty();
            }
        }

        public void EventByNextTollgate(CharacterController character, int param0 = 0, int param1 = 0)
        {
            var id = character.GetExData((int)eExdataDefine.e648);
            CheckEraLearnSkill2(character, id);
            CheckEraChange(character, id);

        }
        //购买道具
        public void EventBuyItem(CharacterController character, int param0 = 0, int param1 = 0)
        {
            foreach (var mission in character.mTask.mData)
            {
                var mis = mission.Value;
                if (mis.Data[0] != 0 || (mis.IsGang(mis.Id) && character.mAlliance.AllianceId <= 0))
                {
                    continue;
                }
                if (mis.Data[1] != (int)eMissionType.EnhanceEquip)
                {
                    continue;
                }
                if (param1 < 1)
                {
                    continue;
                }
                if (mis.Data[3] != -1)
                {
                    if (BitFlag.GetAnd(param0, mis.Data[3]) == 0)
                    {
                        continue;
                    }
                }
                //var tbItem = Table.GetItemBase(param0);
                //if (tbItem.Type != mis.Data[3])
                //    continue;
                if (mis.Data[2] + param1 >= mis.Data[4])
                {
                    mis.Data[2] = mis.Data[4];
                    mis.Data[0] = 1;
                }
                else
                {
                    mis.Data[2] += param1;
                }
                //character.mTask.mFlag = true;
                //mis.mFlag = true;
                mis.MarkDirty();
            }
        }

        //穿装备
        public void EventEquipItemChange(CharacterController character, int param0 = 0, int param1 = 0)
        {
            foreach (var mission in character.mTask.mData)
            {
                var mis = mission.Value;
                if (mis.Data[0] != 0 || (mis.IsGang(mis.Id) && character.mAlliance.AllianceId <= 0))
                {
                    continue;
                }
                if (mis.Data[1] != (int)eMissionType.EquipItem)
                {
                    continue;
                }
                if (mis.Data[3] != -1)
                {
                    if (BitFlag.GetAnd(param0, mis.Data[3]) == 0)
                    {
                        continue;
                    }
                }
                //var tbItem = Table.GetItemBase(param0);
                //if (tbItem.Type != mis.Data[3])
                //    continue;
                mis.Data[2]++;
                if (mis.Data[2] >= mis.Data[4])
                {
                    mis.Data[0] = 1;
                }
                //character.mTask.mFlag = true;
                //mis.mFlag = true;
                mis.MarkDirty();
            }
        }

        //强化装备
        public void EventEnhanceEquip(CharacterController character, int param0 = 0, int param1 = 0)
        {
            foreach (var mission in character.mTask.mData)
            {
                var mis = mission.Value;
                if (mis.Data[0] != 0 || (mis.IsGang(mis.Id) && character.mAlliance.AllianceId <= 0))
                {
                    continue;
                }
                if (mis.Data[1] != (int)eMissionType.EnhanceEquip)
                {
                    continue;
                }
                if (mis.Data[3] != -1)
                {
                    if (BitFlag.GetAnd(param0, mis.Data[3]) == 0)
                    {
                        continue;
                    }
                }
                //var tbItem = Table.GetItemBase(param0);
                //if (tbItem.Type != mis.Data[3])
                //    continue;
                mis.Data[2]++;
                if (mis.Data[2] >= mis.Data[4])
                {
                    mis.Data[0] = 1;
                }
                //character.mTask.mFlag = true;
                //mis.mFlag = true;
                mis.MarkDirty();
            }
        }

        //追加装备
        public void EventAdditionalEquip(CharacterController character, int param0 = 0, int param1 = 0)
        {
            foreach (var mission in character.mTask.mData)
            {
                var mis = mission.Value;
                if (mis.Data[0] != 0 || (mis.IsGang(mis.Id) && character.mAlliance.AllianceId <= 0))
                {
                    continue;
                }
                if (mis.Data[1] != (int)eMissionType.AdditionalEquip)
                {
                    continue;
                }
                if (mis.Data[3] != -1)
                {
                    if (BitFlag.GetAnd(param0, mis.Data[3]) == 0)
                    {
                        continue;
                    }
                }
                //var tbItem = Table.GetItemBase(param0);
                //if (tbItem.Type != mis.Data[3])
                //    continue;
                mis.Data[2]++;
                if (mis.Data[2] >= mis.Data[4])
                {
                    mis.Data[0] = 1;
                }
                //character.mTask.mFlag = true;
                //mis.mFlag = true;
                mis.MarkDirty();
            }
        }

        //升级技能
        public void EventUpgradeSkill(CharacterController character, int param0 = 0, int param1 = 0)
        {
            foreach (var mission in character.mTask.mData)
            {
                var mis = mission.Value;
                if (mis.Data[0] != 0 || (mis.IsGang(mis.Id) && character.mAlliance.AllianceId <= 0))
                {
                    continue;
                }
                if (mis.Data[1] == (int)eMissionType.LearnSkill)
                {
                    if (mis.Data[3] != -1)
                    {
                        if (param0 != mis.Data[3])
                        {
                            continue;
                        }
                    }
                    mis.Data[2]++;
                    mis.Data[0] = 1;
                    mis.MarkDirty();
                    continue;
                }
                if (mis.Data[1] != (int)eMissionType.UpgradeSkill)
                {
                    continue;
                }
                if (mis.Data[3] != -1)
                {
                    if (param0 != mis.Data[3])
                    {
                        continue;
                    }
                }
                mis.Data[2]++;
                if (mis.Data[2] >= mis.Data[4])
                {
                    mis.Data[0] = 1;
                }
                //character.mTask.mFlag = true;
                //mis.mFlag = true;
                mis.MarkDirty();
            }
        }

        //使用NPC服务
        public void EventNpcService(CharacterController character, int param0 = 0, int param1 = 0)
        {
            foreach (var mission in character.mTask.mData)
            {
                var mis = mission.Value;
                if (mis.Data[0] != 0 || (mis.IsGang(mis.Id) && character.mAlliance.AllianceId <= 0))
                {
                    continue;
                }
                if (mis.Data[1] != (int)eMissionType.NpcServe)
                {
                    continue;
                }
                if (mis.Data[3] != -1)
                {
                    if (param0 != mis.Data[3])
                    {
                        continue;
                    }
                }
                mis.Data[2]++;
                if (mis.Data[2] >= mis.Data[4])
                {
                    mis.Data[0] = 1;
                }
                //character.mTask.mFlag = true;
                //mis.mFlag = true;
                mis.MarkDirty();
            }
        }

        //参与竞技场
        public void EventArena(CharacterController character, int param0 = 0, int param1 = 0)
        {
            foreach (var mission in character.mTask.mData)
            {
                var mis = mission.Value;
                if (mis.Data[0] != 0 || (mis.IsGang(mis.Id) && character.mAlliance.AllianceId <= 0))
                {
                    continue;
                }
                if (mis.Data[1] != (int)eMissionType.Arena)
                {
                    continue;
                }
                mis.Data[2]++;
                if (mis.Data[2] >= mis.Data[4])
                {
                    mis.Data[0] = 1;
                }
                //character.mTask.mFlag = true;
                //mis.mFlag = true;
                mis.MarkDirty();
            }
        }

        //添加好友
        public void EventAddFriend(CharacterController character, int param0 = 0, int param1 = 0)
        {
            foreach (var mission in character.mTask.mData)
            {
                var mis = mission.Value;
                if (mis.Data[0] != 0 || (mis.IsGang(mis.Id) && character.mAlliance.AllianceId <= 0))
                {
                    continue;
                }
                if (mis.Data[1] != (int)eMissionType.AddFriend)
                {
                    continue;
                }
                mis.Data[2]++;
                if (mis.Data[2] >= mis.Data[4])
                {
                    mis.Data[0] = 1;
                }
                //character.mTask.mFlag = true;
                //mis.mFlag = true;
                mis.MarkDirty();
            }
        }

        //合成道具
        public void EventComposeItem(CharacterController character, int param0 = 0, int param1 = 0)
        {
            foreach (var mission in character.mTask.mData)
            {
                var mis = mission.Value;
                if (mis.Data[0] != 0 || (mis.IsGang(mis.Id) && character.mAlliance.AllianceId <= 0))
                {
                    continue;
                }
                if (mis.Data[1] != (int)eMissionType.ComposeItem)
                {
                    continue;
                }
                if (mis.Data[3] != -1 && Table.GetItemCompose(param0).Type != mis.Data[3])
                {
                    continue;
                }
                var itemId = Table.GetMission(mis.Id).FinishParam[1];
                if (itemId != -1 && itemId != param1)
                {
                    continue;
                }
                mis.Data[2]++;
                if (mis.Data[2] >= mis.Data[4])
                {
                    mis.Data[0] = 1;
                }
                //character.mTask.mFlag = true;
                //mis.mFlag = true;
                mis.MarkDirty();
            }
        }
        //标记位为真
        public void FlagTrueEvent(CharacterController character, int exdataId, int addValue)
        {
            foreach (var mission in character.mTask.mData)
            {
                var mis = mission.Value;
                if (mis.Data[0] != 0 || (mis.IsGang(mis.Id) && character.mAlliance.AllianceId <= 0))
                {
                    continue; //完成状态判断
                }
                if (mis.Data[1] != (int)eMissionType.FlagTrue)
                {
                    continue; //目标判断
                }
                if (mis.Data[3] != exdataId)
                {
                    continue; //ID判断
                }
                //数据处理
                mis.Data[2] += addValue;
                if (mis.Data[2] >= mis.Data[4])
                {
                    mis.Data[0] = 1;
                }
                mis.MarkDirty();
            }
        }
        //扩展计数增加
        public void ExdataAdd(CharacterController character, int exdataId, int addValue)
        {
            //任务目标17
            foreach (var mission in character.mTask.mData)
            {
                var mis = mission.Value;
                if (mis.Data[0] != 0 || (mis.IsGang(mis.Id) && character.mAlliance.AllianceId <= 0))
                {
                    continue; //完成状态判断
                }
                if (mis.Data[1] != (int)eMissionType.ExdataAdd)
                {
                    continue; //目标判断
                }
                if (mis.Data[3] != exdataId)
                {
                    continue; //ID判断
                }
                //数据处理
                mis.Data[2] += addValue;
                if (mis.Data[2] >= mis.Data[4])
                {
                    mis.Data[0] = 1;
                }
                mis.MarkDirty();
            }
        }

        //技能天赋点数修改
        public void SkillPointChange(CharacterController character, int skillId, int value)
        {
            //任务目标20
            foreach (var mission in character.mTask.mData)
            {
                var mis = mission.Value;
                if (mis.Data[0] != 0 || (mis.IsGang(mis.Id) && character.mAlliance.AllianceId <= 0))
                {
                    continue; //完成状态判断
                }
                if (mis.Data[1] != (int)eMissionType.GetSkillTalant)
                {
                    continue; //目标判断
                }
                if (mis.Data[3] != skillId)
                {
                    continue; //ID判断
                }
                //数据处理
                mis.Data[2] = value;
                if (value >= mis.Data[4])
                {
                    mis.Data[0] = 1;
                }
                //character.mTask.mFlag = true;
                //mis.mFlag = true;
                mis.MarkDirty();
            }
        }

        //从仓库取出物品
        private void DepotTakeOut(CharacterController character, int itemID, int outCount)
        {
            foreach (var mission in character.mTask.mData)
            {
                var mis = mission.Value;
                if (mis.Data[0] != 0 || (mis.IsGang(mis.Id) && character.mAlliance.AllianceId <= 0))
                {
                    continue; //完成状态判断
                }
                if (mis.Data[1] != (int)eMissionType.DepotTakeOut)
                {
                    continue; //目标判断
                }
                if (mis.Data[3] != itemID)
                {
                    continue; //ID判断
                }
                //数据处理
                //数据处理
                mis.Data[2] += outCount;
                if (mis.Data[2] >= mis.Data[4])
                {
                    mis.Data[0] = 1;
                }
                mis.MarkDirty();
            }
        }

        #endregion

        #region   任务判断

        //判断任务是否完成
        private bool CheckFinish(CharacterController character, Mission mis, bool force = false)
        {
            if (mis.Data[0] == 1)
            {
                return true;
            }
            var eMission = (eMissionType)mis.Data[1];
            switch (eMission)
            {
                case eMissionType.Finish:
                    return true;
                case eMissionType.KillMonster:
                    {
                        if (mis.Data[2] >= mis.Data[4])
                        {
                            return true;
                        }
                        return false;
                    }
                case eMissionType.AcceptProgressBar:
                    return true;
                case eMissionType.AreaProgressBar:
                    {
                        return true;
                    }
                case eMissionType.CheckItem:
                    {
                        var nRealCount = character.mBag.GetItemCount(mis.Data[3]); //刷新当前数量
                        if (mis.Data[2] != nRealCount)
                        {
                            Logger.Warn("CheckFinish Mission={0} ItemCount Warn! itemid={1} , RealCount={2},DataCount={3}",
                                mis.Id, mis.Data[3], nRealCount, mis.Data[2]);
                            mis.Data[2] = nRealCount;
                            //character.mTask.mFlag = true;
                            //mis.mFlag = true;
                        }
                        if (mis.Data[2] >= mis.Data[4])
                        {
                            return true;
                        }
                        return false;
                    }
                case eMissionType.AcceptStroy:
                    return true;
                case eMissionType.AreaStroy:
                    return false;
                case eMissionType.Tollgate:
                    return false;
                case eMissionType.Dungeon:
                    return false;
                case eMissionType.ObtainItem:
                {
                    if (mis.Data[2] >= mis.Data[4])
                    {
                        return true;
                    }
                    return false;
                }
                default:
                    Logger.Warn("CheckFinish Mission={0} eMissionType ={1} case not find!", mis.Id, eMission);
                    break;
            }
            if (force)
            {
                return true;
            }
            return false;
        }

        public bool CheckConditionEventMission(int nConditionId)
        {
            if (MissionManager.TriggerMission.ContainsKey(nConditionId))
            {
                return true;
            }
            return false;
        }

        #endregion
    }

    public class MissionManager : NodeBase
    {
        public override IEnumerable<NodeBase> Children
        {
            get { return mData.Values; }
        }

        public static void AddFriend(IEvent ievent)
        {
            mImpl.AddFriend(ievent);
        }

        public static void AdditionalEquip(IEvent ievent)
        {
            mImpl.AdditionalEquip(ievent);
        }

        public static void Arena(IEvent ievent)
        {
            mImpl.Arena(ievent);
        }

        public static void BuyItem(IEvent ievent)
        {
            mImpl.BuyItem(ievent);
        }

        #region   任务判断

        public static bool CheckConditionEventMission(int nConditionId)
        {
            return mImpl.CheckConditionEventMission(nConditionId);
        }

        #endregion

        public static void ComposeItem(IEvent ievent)
        {
            mImpl.ComposeItem(ievent);
        }

        public static void DepotTakeOutEvent(IEvent ievent)
        {
            mImpl.DepotTakeOutEvent(ievent);
        }

        public static void EnhanceEquip(IEvent ievent)
        {
            mImpl.EnhanceEquip(ievent);
        }

        public static void EnterAreaEvent(IEvent ievent)
        {
            mImpl.EnterAreaEvent(ievent);
        }

        public static void EquipItem(IEvent ievent)
        {
            mImpl.EquipItem(ievent);
        }

        public static void ExdataAdd(IEvent ievent)
        {
            mImpl.ExdataAdd(ievent);
        }

        public static void ExDataChangeEvent(IEvent ievent)
        {
            mImpl.ExDataChangeEvent(ievent);
        }

        public static void FlagFalseEvent(IEvent ievent)
        {
            mImpl.FlagFalseEvent(ievent);
        }

        public static void FlagTrueEvent(IEvent ievent)
        {
            mImpl.FlagTrueEvent(ievent);
        }

        public void GetNetDirtyMissions(MissionDataMessage msg)
        {
            mImpl.GetNetDirtyMissions(this, msg);
        }

        public static void ItemChangeEvent(IEvent ievent)
        {
            mImpl.ItemChangeEvent(ievent);
        }

        public static void KillMonsterEvent(IEvent ievent)
        {
            mImpl.KillMonsterEvent(ievent);
        }

        public int ModifyByLevel(int nOldValue, int nLevel)
        {
            return mImpl.ModifyByLevel(this, nOldValue, nLevel);
        }

        public override void NetDirtyHandle()
        {
            mImpl.NetDirtyHandle(this);
        }

        public static void NpcServe(IEvent ievent)
        {
            mImpl.NpcServe(ievent);
        }

        public static void SkillPointChange(IEvent ievent)
        {
            mImpl.SkillPointChange(ievent);
        }

        public static void TollgateEvent(IEvent ievent)
        {
            mImpl.TollgateEvent(ievent);
        }

        public static void TollgateNextEvent(IEvent ievent)
        {
            mImpl.TollgateNextEvent(ievent);
        }
        public static void UpgradeSkill(IEvent ievent)
        {
            mImpl.UpgradeSkill(ievent);
        }

        #region   数据结构

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); //

        public static Dictionary<int, List<int>> TriggerMission = new Dictionary<int, List<int>>();
        //Key=条件ID      Value=影响的任务列表

        public CharacterController mCharacter; //所在角色
        public Dictionary<int, Mission> mData = new Dictionary<int, Mission>();
        public MissionData mDbData;
        //public bool mFlag = true;
        private static IMissionManager mImpl;

        static MissionManager()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof(MissionManager),
                typeof(MissionManagerDefaultImpl),
                o => { mImpl = (IMissionManager)o; });
        }

        #endregion

        #region   初始化

        //初始化静态数据
        public static void Init()
        {
            mImpl.Init();
        }

        //创建时的初始化
        public MissionData InitByBase(CharacterController character)
        {
            return mImpl.InitByBase(this, character);
        }

        public void InitByDB(CharacterController character, MissionData mission)
        {
            mImpl.InitByDB(this, character, mission);
        }

        #endregion

        #region   常用接口(接受，放弃，完成,提交）

        //检查有没有新的可接任务
        public void GetCanAcceptMission()
        {
            mImpl.GetCanAcceptMission(this);
        }

        //看任务是否可接
        public Mission TryAccept(CharacterController character, int nId)
        {
            return mImpl.TryAccept(this, character, nId);
        }

        //接受任务
        public Mission Accept(CharacterController character, int nId)
        {
            return mImpl.Accept(this, character, nId);
        }

        //放弃任务
        public ErrorCodes Drop(int nId)
        {
            return mImpl.Drop(this, nId);
        }

        //完成任务
        public ErrorCodes Complete(CharacterController character, int nId, bool force = false)
        {
            return mImpl.Complete(this, character, nId, force);
        }

        //提交任务
        public ErrorCodes Commit(CharacterController character, int nId, bool isGM = false)
        {
            return mImpl.Commit(this, character, nId, isGM);
        }

        //获得任务
        public Mission GetMission(int nId)
        {
            return mImpl.GetMission(this, nId);
        }

        public ErrorCodes SetMissionParam(CharacterController character, int nId, int nIndex, int nValue)
        {
            return mImpl.SetMissionParam(this, character, nId, nIndex, nValue);
        }

        public ErrorCodes RefreshHunterMission(CharacterController character)
        {
            return mImpl.RefreshHunterMission(this, character);
        }

        #endregion

        #region

        public MayaBaseRecord GetEraByFubenId(CharacterController character, int fubenId)
        {
            return mImpl.GetEraByFubenId(this, character, fubenId);
        }

        #endregion

        #region   任务事件

        //触发任务靠事件
        public static Dictionary<int, int> tasklist = new Dictionary<int, int>();

        public static void TriggerMissionByEvent(CharacterController character,
                                                 eEventType type,
                                                 int param0 = 0,
                                                 int param1 = 0)
        {
            mImpl.TriggerMissionByEvent(character, type, param0, param1);
        }

        //资源变化事件
        public static void EventByItemChange(CharacterController character, int param0 = 0, int param1 = 0)
        {
            mImpl.EventByItemChange(character, param0, param1);
        }

        //杀怪事件
        public static void EventByKillMonster(CharacterController character, int param0 = 0, int param1 = 1)
        {
            mImpl.EventByKillMonster(character, param0, param1);
        }

        //区域事件
        public static void EventByEnterArea(CharacterController character, int param0 = 0, bool param1 = false)
        {
            mImpl.EventByEnterArea(character, param0, param1);
        }

        //关卡事件
        public static void EventByTollgate(CharacterController character, int param0 = 0, int param1 = 0)
        {
            mImpl.EventByTollgate(character, param0, param1);
        }

        //购买道具
        public static void EventBuyItem(CharacterController character, int param0 = 0, int param1 = 0)
        {
            mImpl.EventBuyItem(character, param0, param1);
        }

        //穿装备
        public static void EventEquipItemChange(CharacterController character, int param0 = 0, int param1 = 0)
        {
            mImpl.EventEquipItemChange(character, param0, param1);
        }

        //强化装备
        public static void EventEnhanceEquip(CharacterController character, int param0 = 0, int param1 = 0)
        {
            mImpl.EventEnhanceEquip(character, param0, param1);
        }

        //追加装备
        public static void EventAdditionalEquip(CharacterController character, int param0 = 0, int param1 = 0)
        {
            mImpl.EventAdditionalEquip(character, param0, param1);
        }

        //升级技能
        public static void EventUpgradeSkill(CharacterController character, int param0 = 0, int param1 = 0)
        {
            mImpl.EventUpgradeSkill(character, param0, param1);
        }

        //使用NPC服务
        public static void EventNpcService(CharacterController character, int param0 = 0, int param1 = 0)
        {
            mImpl.EventNpcService(character, param0, param1);
        }

        //参与竞技场
        public static void EventArena(CharacterController character, int param0 = 0, int param1 = 0)
        {
            mImpl.EventArena(character, param0, param1);
        }

        //添加好友
        public static void EventAddFriend(CharacterController character, int param0 = 0, int param1 = 0)
        {
            mImpl.EventAddFriend(character, param0, param1);
        }

        //合成道具
        public static void EventComposeItem(CharacterController character, int param0 = 0, int param1 = 0)
        {
            mImpl.EventComposeItem(character, param0, param1);
        }

        //扩展计数增加
        public static void ExdataAdd(CharacterController character, int exdataId, int addValue)
        {
            mImpl.ExdataAdd(character, exdataId, addValue);
        }

        //技能天赋点数修改
        public static void SkillPointChange(CharacterController character, int skillId, int value)
        {
            mImpl.SkillPointChange(character, skillId, value);
        }

        #endregion
    }
}