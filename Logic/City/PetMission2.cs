#region using

using System;
using System.Collections.Generic;
using System.Linq;
using DataContract;
using DataTable;
using NLog;
using Shared;

#endregion

namespace Logic
{
    public interface IDictionaryPro
    {
        int GetKey(DictionaryPro _this);
        void PushPro(DictionaryPro _this, int key, int pro);
    }

    public class DictionaryProDefaultImpl : IDictionaryPro
    {
        public void PushPro(DictionaryPro _this, int key, int pro)
        {
            _this.totlePros += pro;
            _this.Pros.modifyValue(key, pro);
        }

        public int GetKey(DictionaryPro _this)
        {
            var c = _this.Pros.Count;
            if (c < 1)
            {
                PlayerLog.WriteLog((int) LogType.DictionaryPro, "DictionaryPro is null");
                return -1;
            }
            if (c == 1)
            {
                return _this.Pros.First().Key;
            }
            var r = MyRandom.Random(_this.totlePros);
            foreach (var pro in _this.Pros)
            {
                if (r < pro.Value)
                {
                    return pro.Key;
                }
                r -= pro.Value;
            }

            PlayerLog.WriteLog((int) LogType.DictionaryPro, "DictionaryPro t={0},keys={1}", _this.totlePros,
                _this.Pros.GetDataString());
            return -1;
        }
    }

    public class DictionaryPro
    {
        private static IDictionaryPro mImpl;

        static DictionaryPro()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (DictionaryPro), typeof (DictionaryProDefaultImpl),
                o => { mImpl = (IDictionaryPro) o; });
        }

        public Dictionary<int, int> Pros = new Dictionary<int, int>();
        public int totlePros;

        public int GetKey()
        {
            return mImpl.GetKey(this);
        }

        public void PushPro(int key, int pro)
        {
            mImpl.PushPro(this, key, pro);
        }
    }

    public enum PetMissionConditionType
    {
        MissionType = 0, //任务类型
        Area = 1, //地点
        Weather = 2, //天气
        Time = 3, //时段
        Quality = 4, //稀有度
        PetType = 5, //优势种族
        TotleFightPoint = 10, //总战斗力
        TotleStars = 11 //总星级
    }

    public interface IPetMission2
    {
        ErrorCodes Commit(PetMission2 _this);
        void DeleteSelf(PetMission2 _this);
        int GetFinishTime(PetMission2 _this, List<PetItem> petItems);
        int GetMissionTime(PetMission2 _this);
        PetMissionData GetNetData(PetMission2 _this);
        DateTime GetOverTime(PetMission2 _this);
        void Init(PetMission2 _this, CharacterController character, int missionGuid, int hideLevel);
        void InitByTable();
        void InitPetMission2(PetMission2 _this, CharacterController character, DBPetMission dbdata);
        void InitPetMission2(PetMission2 _this, CharacterController character, int nId, int mLevel);
        void OnDestroy(PetMission2 _this);
        void SetOverTime(PetMission2 _this, DateTime value);
    }

    public class PetMission2DefaultImpl : IPetMission2
    {
        #region   数据结构

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); //

        public DateTime GetOverTime(PetMission2 _this)
        {
            return DateTime.FromBinary(_this.mDbData.OverTime);
        }

        public void SetOverTime(PetMission2 _this, DateTime value)
        {
            _this.mDbData.OverTime = value.ToBinary();
            //if (_this.mOverTrigger != null)
            //{
            //    LogicServerControl.Timer.ChangeTime(_this.mOverTrigger, value);
            //}
            //else
            //{
            //    _this.mOverTrigger = LogicServerControl.Timer.CreateTrigger(value, () =>
            //    {
            //        TimeOver(_this);
            //    });
            //}
        }

        //返回任务时间修正的效果ID
        //时间  int 8小时 ：小于136  等于大于137
        public int GetMissionTime(PetMission2 _this)
        {
            //if (TbRecord.NeedTime[PetCount - 1] < 480)
            {
                return 137;
            }
            //return 136;
        }

        public void OnDestroy(PetMission2 _this)
        {
            //if (_this.mOverTrigger != null)
            //{
            //    LogicServerControl.Timer.DeleteTrigger(_this.mOverTrigger);
            //    _this.mOverTrigger = null;
            //}
        }

        //删除该任务
        public void DeleteSelf(PetMission2 _this)
        {
            _this.mCharacter.mPetMission.DeleteMission(_this.Id);
        }

        //定时结束了
        //private void TimeOver(PetMission2 _this)
        //{
        //    _this.mOverTrigger = null;
        //    PetMissionStateType type = (PetMissionStateType) _this.State;
        //    switch (type)
        //    {
        //        case PetMissionStateType.New:
        //            {
        //                //需要删除到时间还没做的任务
        //                DeleteSelf(_this);
        //                if (_this.mCharacter.Proxy != null)
        //                {
        //                    _this.mCharacter.Proxy.DeletePetMission(_this.Id);
        //                }
        //            }
        //            break;
        //        case PetMissionStateType.Do:
        //            {
        //                //做任务结束了
        //                _this.State = (int)PetMissionStateType.Finish;
        //                _this.MarkDirty();
        //            }
        //            break;
        //        case PetMissionStateType.Finish:
        //            break;
        //        case PetMissionStateType.Delete:
        //            break;
        //        default:
        //            throw new ArgumentOutOfRangeException();
        //    }
        //}
        //提交任务
        public ErrorCodes Commit(PetMission2 _this)
        {
            var rewardId = _this.Level*1000000 + _this.Type*10000 + _this.Quality*100 + _this.PetCount;
            var tbMisW = Table.GetGetMissionReward(rewardId);
            if (tbMisW == null)
            {
                Logger.Error("GetMissionReward not find!!! rewardId={0}", rewardId);
                return ErrorCodes.Error_MissionID;
            }
            //检查随从状态
            var petItems = new List<PetItem>();
            foreach (var i in _this.PetList)
            {
                var pet = _this.mCharacter.GetPet(i);
                if (pet == null)
                {
                    return ErrorCodes.Error_PetNotFind;
                }
                if (pet.GetId() == -1)
                {
                    return ErrorCodes.Error_PetNotFind;
                }
                petItems.Add(pet);
            }
            //检查物品是否能装下包裹
            var result = _this.mCharacter.mBag.CheckAddItem(tbMisW.RewardType, tbMisW.RewardCount);
            if (result != ErrorCodes.OK)
            {
                return result;
            }
            _this.mCharacter.mBag.AddItem(tbMisW.RewardType, tbMisW.RewardCount, eCreateItemType.PetMissionSubmit);
            //下场随从,随从经验奖励
            var TbRecord = Table.GetGetMissionInfo(_this.Level);
            var petexp = TbRecord.PetGetExp; // * point / maxPoint;
            _this.mCharacter.mPetMission.AddTotleExp(TbRecord.HomeExp[_this.PetCount - 1]);
            //_this.mCharacter.mCity.CityAddExp(TbRecord.HomeExp[_this.PetCount - 1]);
            foreach (var petItem in petItems)
            {
                var oldLevel = petItem.GetLevel();
                petItem.PetAddExp(petexp);
                var newLevel = petItem.GetLevel();
                if (newLevel > oldLevel)
                {
                    _this.mCharacter.AddExData((int) eExdataDefine.e331, newLevel - oldLevel);
                    var fp = petItem.GetFightPoint();
                    _this.mCharacter.SetExdataToMore(68, fp);
                }
                petItem.SetState(PetStateType.Idle);
                petItem.MarkDirty();
            }
            _this.PetList.Clear();
            _this.State = (int) PetMissionStateType.Finish;
            return ErrorCodes.OK;
        }

        //计算完成时间
        public int GetFinishTime(PetMission2 _this, List<PetItem> petItems)
        {
            var tbRecord = Table.GetGetMissionInfo(_this.Level);
            if (tbRecord == null)
            {
                Logger.Error("GetFinishTime MissionLevel ={0}", _this.Level);
                return 1;
            }

            var seconds = _this.mDbData.NeedTime*60; //tbRecord.TaskTime[PetCount - 1] * 60;
            var modifyPoint = 20000.0f;
            var index = 0;


            //MissionType = 0,        //任务类型
            //Area = 1,               //地点
            //Weather = 2,            //天气
            //Time = 3,               //时段
            //Quality = 4,           //稀有度
            //PetType = 5,            //优势种族
            //TotleFightPoint = 10,    //总战斗力
            //TotleStars = 11,         //总星级


            foreach (var petItem in petItems)
            {
                petItem.ForeachSkill(skill =>
                {
                    if (5 == skill.EffectId && (int) PetMissionConditionType.MissionType == skill.Param[0] &&
                        _this.mDbData.Type == skill.Param[1])
                    {
                        var tbC = Table.GetMissionConditionInfo((int) PetMissionConditionType.MissionType);
                        modifyPoint += tbC.Param;
                        return false;
                    }
                    return true;
                });
            }

            foreach (var conditionId in _this.mDbData.ConditionIds)
            {
                var c = (PetMissionConditionType) conditionId;
                var param = _this.mDbData.ConditionParam[index];
                switch (c)
                {
                    case PetMissionConditionType.MissionType: //任务类型
                    case PetMissionConditionType.Area: //地点
                    case PetMissionConditionType.Weather: //天气
                    case PetMissionConditionType.Time: //时段
                    {
                        foreach (var petItem in petItems)
                        {
                            petItem.ForeachSkill(skill =>
                            {
                                if (5 == skill.EffectId && conditionId == skill.Param[0] && param == skill.Param[1])
                                {
                                    var tbC = Table.GetMissionConditionInfo(conditionId);
                                    modifyPoint += tbC.Param;
                                    return false;
                                }
                                return true;
                            });
                        }
                    }
                        break;

                    case PetMissionConditionType.PetType: //优势种族
                    {
                        foreach (var petItem in petItems)
                        {
                            var tbPet = Table.GetPet(petItem.GetId());
                            if (tbPet.Type == param)
                            {
                                var tbC = Table.GetMissionConditionInfo(conditionId);
                                modifyPoint += tbC.Param;
                                break;
                            }
                        }
                    }
                        break;
                    case PetMissionConditionType.TotleFightPoint: //总战斗力
                    {
                        var petFightPoint = 0;
                        foreach (var petItem in petItems)
                        {
                            petFightPoint += petItem.GetFightPoint();
                        }
                        if (petFightPoint >= _this.mDbData.ConditionParam[index])
                        {
                            var tbC = Table.GetMissionConditionInfo(conditionId);
                            modifyPoint += tbC.Param;
                        }
                    }
                        break;
                    case PetMissionConditionType.TotleStars: //总星级
                    {
                        var petStars = 0;
                        foreach (var petItem in petItems)
                        {
                            petStars += Table.GetPet(petItem.GetId()).Ladder;
                        }
                        if (petStars >= param)
                        {
                            var tbC = Table.GetMissionConditionInfo(conditionId);
                            modifyPoint += tbC.Param;
                        }
                    }
                        break;
                    case PetMissionConditionType.Quality: //稀有度
                    {
                        foreach (var petItem in petItems)
                        {
                            petItem.ForeachSkill(skill =>
                            {
                                if (conditionId == skill.EffectId && param == skill.Param[0])
                                {
                                    var tbC = Table.GetMissionConditionInfo(conditionId);
                                    modifyPoint += tbC.Param;
                                    return false;
                                }
                                return true;
                            });
                        }
                    }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                index++;
            }
            return (int) (seconds*modifyPoint/10000);
        }

        //获取网络结构
        public PetMissionData GetNetData(PetMission2 _this)
        {
            var tempChanges = new PetMissionData
            {
                Id = _this.Id,
                State = _this.State,
                OverTime = _this.mDbData.OverTime,
                FinishPro = _this.FinishPro,
                PetCount = _this.PetCount,
                Level = _this.Level,
                Type = _this.Type,
                Quality = _this.Quality,
                Name = _this.mDbData.Name,
                NeedTime = _this.mDbData.NeedTime
            };
            tempChanges.ConditionIds.AddRange(_this.mDbData.ConditionIds);
            tempChanges.ConditionParam.AddRange(_this.mDbData.ConditionParam);
            tempChanges.PetList.AddRange(_this.PetList);
            return tempChanges;
        }

        #endregion

        #region   构造任务

        public void InitPetMission2(PetMission2 _this, CharacterController character, DBPetMission dbdata)
        {
            //TbRecord = Table.GetPetMission(dbdata.Id);
            //if (TbRecord == null)
            //{
            //    Logger.Error("PetMissionId={0} not find by db", dbdata.Id);
            //    return;
            //}
            _this.mDbData = dbdata;
            _this.mCharacter = character;
            var type = (PetMissionStateType) _this.State;
            switch (type)
            {
                case PetMissionStateType.New:
                {
                    //if (DateTime.Now > _this.OverTime)
                    //{
                    //    _this.State = (int) PetMissionStateType.Delete;
                    //    _this.mDbData = null;
                    //}
                    //else
                    //{
                    //    _this.mOverTrigger = LogicServerControl.Timer.CreateTrigger(_this.OverTime, () =>
                    //    {
                    //        TimeOver(_this);
                    //    });
                    //}
                }
                    break;
                case PetMissionStateType.Do:
                {
                    //if (DateTime.Now > _this.OverTime)
                    //{
                    //    _this.State = (int) PetMissionStateType.Finish;
                    //}
                    //else
                    //{
                    //    _this.mOverTrigger = LogicServerControl.Timer.CreateTrigger(_this.OverTime, () =>
                    //    {
                    //        TimeOver(_this);
                    //    });
                    //}
                }
                    break;
                case PetMissionStateType.Finish:
                    _this.State = (int) PetMissionStateType.Do;
                    _this.OverTime = DateTime.Now;
                    break;
                case PetMissionStateType.Delete:
                    break;
            }
        }

        public void InitPetMission2(PetMission2 _this, CharacterController character, int nId, int mLevel)
        {
            _this.mCharacter = character;
            _this.mDbData = new DBPetMission();
            try
            {
                Init(_this, character, nId, mLevel);
            }
            catch (Exception)
            {
                _this.mDbData.State = (int) PetMissionStateType.Delete;
            }
        }

        private string GetRandomName(PetMission2 _this, GetMissionNameRecord record)
        {
            return record.Name[MyRandom.Random(record.RandomNameCount)];
        }

        private string GetSceneName(PetMission2 _this, int sceneId)
        {
            switch (sceneId)
            {
                case 0:
                    return Table.GetScene(3).Name;
                case 1:
                    return Table.GetScene(5).Name;
                case 2:
                    return Table.GetScene(1).Name;
                case 3:
                    return Table.GetScene(2).Name;
                case 4:
                    return Table.GetScene(4).Name;
                case 5:
                    return Table.GetScene(6).Name;
                case 6:
                    return Table.GetScene(7).Name;
                case 7:
                    return Table.GetScene(8).Name;
            }
            return "";
        }

        //构造任务
        public void Init(PetMission2 _this, CharacterController character, int missionGuid, int hideLevel)
        {
            _this.Id = missionGuid;
            //随机任务等级
            _this.Level = InitMissionLevel(_this, hideLevel);
            //随机任务类型
            _this.Type = InitMissionType(_this, hideLevel);
            //随机品质
            //Quality = InitMissionQuality(Level);
            var tbSkill320 = Table.GetSkillUpgrading(320);
            var c = tbSkill320.Values.Count;
            _this.Quality = tbSkill320.GetSkillUpgradingValue(character.GetExData(442) % c);
            character.AddExData(442, 1);
            //随机随从数量
            if (_this.Quality == 2)
            {
                _this.PetCount = 3;
            }
            else
            {
                _this.PetCount = InitMissionPetCount(_this, _this.Level);
            }
            //随机条件1
            InitMissionCondition(_this, _this.Level);
            //随机条件2
            var areaParam = InitMissionCondition2(_this, _this.Level);
            //任务剩余时间
            var TbRecord = Table.GetGetMissionInfo(_this.Level);
            //OverTime = DateTime.Now.AddMinutes(TbRecord.ExistTime);
            //随机任务时间
            _this.mDbData.NeedTime = TbRecord.TaskTime[_this.Quality];

            //根据任务类型确定任务名称
            var key = _this.Type*10000 + _this.Level*100 + _this.Quality;
            var tbName = Table.GetGetMissionName(key);
            switch (_this.Type)
            {
                case 0:
                {
                    if (tbName == null)
                    {
                        // "掠夺:入侵者营地"
                        _this.mDbData.Name = "^301002";
                        Logger.Error("掠夺:level={0},Quality={1}", _this.Level, _this.Quality);
                    }
                    else
                    {
                        //掠夺:{0}
                        _this.mDbData.Name = string.Format("^301003^{0}", GetRandomName(_this, tbName));
                    }
                }

                    break;
                case 1:
                    if (areaParam == -1)
                    {
                        areaParam = InitConditionParam2(_this, _this.Level, (int) PetMissionConditionType.Area);
                    }
                    if (tbName == null)
                    {
                        //"探索:仙踪林的起源"
                        _this.mDbData.Name = "^301004";
                        Logger.Error("探索:level={0},Quality={1},areaParam{2}", _this.Level, _this.Quality, areaParam);
                    }
                    else
                    {
                        //探索:{1}的{0}
                        _this.mDbData.Name = string.Format("^301005^{0}|{1}", GetRandomName(_this, tbName),
                            GetSceneName(_this, areaParam));
                    }

                    break;
                case 2:
                    if (tbName == null)
                    {
                        //招募:正义的帮手
                        _this.mDbData.Name = "^301006";
                        Logger.Error("招募:level={0},Quality={1}", _this.Level, _this.Quality);
                    }
                    else
                    {
                        //招募:{0}
                        _this.mDbData.Name = string.Format("^301007^{0}", GetRandomName(_this, tbName));
                    }
                    break;
                case 3:
                    if (areaParam == -1)
                    {
                        areaParam = InitConditionParam2(_this, _this.Level, (int) PetMissionConditionType.Area);
                    }

                    if (tbName == null)
                    {
                        //寻宝:正义的帮手
                        _this.mDbData.Name = "^301008";
                        Logger.Error("寻宝:level={0},Quality={1},areaParam{2}", _this.Level, _this.Quality, areaParam);
                    }
                    else
                    {
                        //寻宝:{1}的{0}
                        _this.mDbData.Name = string.Format("^301009^{1}|{0}", GetRandomName(_this, tbName),
                            GetSceneName(_this, areaParam));
                    }
                    break;
                case 4:
                    if (tbName == null)
                    {
                        //讨伐:伤人的野兽
                        _this.mDbData.Name = "^301010";
                        Logger.Error("讨伐:level={0},Quality={1}", _this.Level, _this.Quality);
                    }
                    else
                    {
                        //讨伐:{0}
                        _this.mDbData.Name = string.Format("301011#{0}", GetRandomName(_this, tbName));
                    }
                    break;
                case 5:
                    if (tbName == null)
                    {
                        //讨伐:邪恶大黑魔导
                        _this.mDbData.Name = "^301012";
                        Logger.Error("挑战:level={0},Quality={1}", _this.Level, _this.Quality);
                    }
                    else
                    {
                        //挑战:{0}
                        _this.mDbData.Name = string.Format("^301013^{0}", GetRandomName(_this, tbName));
                    }
                    break;
                case 6:
                    if (tbName == null)
                    {
                        //抓捕:黄金幼龙
                        _this.mDbData.Name = "^301014";
                        Logger.Error("抓捕:level={0},Quality={1}", _this.Level, _this.Quality);
                    }
                    else
                    {
                        //抓捕:{0}
                        _this.mDbData.Name = string.Format("^301015^{0}", GetRandomName(_this, tbName));
                    }
                    break;
            }
        }

        private void PushCondition(PetMission2 _this, int conditionId, int conditionParam)
        {
            _this.mDbData.ConditionIds.Add(conditionId);
            _this.mDbData.ConditionParam.Add(conditionParam);
        }

        #endregion

        #region 随机初始化任务数据

        //初始化
        public void InitByTable()
        {
            PetMission2.hideLevel_misLevel.Clear();
            PetMission2.misLevel_misType.Clear();
            PetMission2.misLevel_Quality.Clear();
            PetMission2.misLevel_PetCount.Clear();
            PetMission2.misLevel_NeedTime.Clear();
            PetMission2.misLevel_Condition.Clear();
            PetMission2.misLevel_ConditionParam.Clear();
            PetMission2.misLevel_Condition2.Clear();
            PetMission2.misLevel_ConditionParam2.Clear();

            Table.ForeachGetMissionLevel(record =>
            {
                var t = new DictionaryPro();
                PetMission2.hideLevel_misLevel[record.Id] = t;
                var index = 0;
                foreach (var i in record.LevelProb)
                {
                    if (i > 0)
                    {
                        t.PushPro(index + 1, i);
                    }
                    index ++;
                }
                return true;
            });
            Table.ForeachGetMissionType(record =>
            {
                var t = new DictionaryPro();
                PetMission2.misLevel_misType[record.Id] = t;
                var index = 0;
                foreach (var i in record.TypeProb)
                {
                    if (i > 0)
                    {
                        t.PushPro(index, i);
                    }
                    index++;
                }
                return true;
            });

            Table.ForeachGetMissionQulity(record =>
            {
                var t = new DictionaryPro();
                PetMission2.misLevel_Quality[record.Id] = t;
                var index = 0;
                foreach (var i in record.DiffProb)
                {
                    if (i > 0)
                    {
                        t.PushPro(index, i);
                    }
                    index++;
                }
                return true;
            });

            Table.ForeachGetPetCount(record =>
            {
                var t = new DictionaryPro();
                PetMission2.misLevel_PetCount[record.Id] = t;
                var index = 0;
                foreach (var i in record.PersonProb)
                {
                    index++;
                    if (i > 0)
                    {
                        t.PushPro(index, i);
                    }
                }
                return true;
            });
            //条件1内容整理
            Table.ForeachGetMissionInfo(record =>
            {
                var t = new DictionaryPro();
                PetMission2.misLevel_Condition[record.Id] = t;
                //战斗力
                var cId1 = (int) PetMissionConditionType.TotleFightPoint;
                t.PushPro(cId1, 5000);
                var index = 0;
                foreach (var i in record.FightNeed)
                {
                    index++;
                    PetMission2.misLevel_ConditionParam[record.Id*10000 + cId1*100 + index] = i;
                }

                if (record.Id >= 3)
                {
                    //星级
                    var cId2 = (int) PetMissionConditionType.TotleStars;
                    index = 0;
                    var isHavePro = false;
                    foreach (var i in record.StarNeed)
                    {
                        index++;
                        PetMission2.misLevel_ConditionParam[record.Id*10000 + cId2*100 + index] = i;
                        if (i > 0)
                        {
                            isHavePro = true;
                        }
                    }
                    if (isHavePro)
                    {
                        t.PushPro(cId2, 5000);
                    }
                }
                return true;
            });
            //条件2内容整理
            var C2 = new List<int>[11];
            for (var i = 1; i <= 10; i++)
            {
                PetMission2.misLevel_ConditionParam2[i] = new Dictionary<int, DictionaryPro>();
                var t = new DictionaryPro();
                PetMission2.misLevel_Condition2[i] = t;
                C2[i] = new List<int>();
            }
            Table.ForeachGetMissionPlace(record =>
            {
                var t = new DictionaryPro();
                var index = 0;
                foreach (var i in record.LandProb)
                {
                    if (i > 0)
                    {
                        t.PushPro(index, i);
                    }
                    index++;
                }
                if (t.GetKey() != -1)
                {
                    C2[record.Id].Add((int) PetMissionConditionType.Area);
                    PetMission2.misLevel_ConditionParam2[record.Id][(int) PetMissionConditionType.Area] = t;
                }
                return true;
            });
            Table.ForeachGetMissionWeather(record =>
            {
                var t = new DictionaryPro();
                var index = 0;
                foreach (var i in record.WeatherProb)
                {
                    if (i > 0)
                    {
                        t.PushPro(index, i);
                    }
                    index++;
                }
                if (t.GetKey() != -1)
                {
                    C2[record.Id].Add((int) PetMissionConditionType.Weather);
                    PetMission2.misLevel_ConditionParam2[record.Id][(int) PetMissionConditionType.Weather] = t;
                }
                return true;
            });
            Table.ForeachGetMissionTimeLevel(record =>
            {
                var t = new DictionaryPro();
                var index = 0;
                foreach (var i in record.TimeTask)
                {
                    if (i > 0)
                    {
                        t.PushPro(index, i);
                    }
                    index++;
                }
                if (t.GetKey() != -1)
                {
                    C2[record.Id].Add((int) PetMissionConditionType.Time);
                    PetMission2.misLevel_ConditionParam2[record.Id][(int) PetMissionConditionType.Time] = t;
                }
                return true;
            });
            Table.ForeachGetPetType(record =>
            {
                var t = new DictionaryPro();
                var index = 0;
                foreach (var i in record.MonsterProb)
                {
                    if (i > 0)
                    {
                        t.PushPro(index, i);
                    }
                    index++;
                }
                if (t.GetKey() != -1)
                {
                    C2[record.Id].Add((int) PetMissionConditionType.PetType);
                    PetMission2.misLevel_ConditionParam2[record.Id][(int) PetMissionConditionType.PetType] = t;
                }
                return true;
            });
            for (var i = 1; i <= 10; i++)
            {
                var l = C2[i];
                for (var j = 0; j < l.Count; j++)
                {
                    for (var k = j + 1; k < l.Count; k++)
                    {
                        PetMission2.misLevel_Condition2[i].PushPro(l[j]*10 + l[k], 1);
                    }
                }
            }
        }

        //初始化任务等级
        private int InitMissionLevel(PetMission2 _this, int hideLevel)
        {
            return PetMission2.hideLevel_misLevel[hideLevel].GetKey();
        }

        //初始化任务类型
        private int InitMissionType(PetMission2 _this, int misLevel)
        {
            return PetMission2.misLevel_misType[misLevel].GetKey();
        }

        //初始化任务品质
        private int InitMissionQuality(PetMission2 _this, int misLevel)
        {
            return PetMission2.misLevel_Quality[misLevel].GetKey();
        }

        //初始化随从数量
        private int InitMissionPetCount(PetMission2 _this, int misLevel)
        {
            return PetMission2.misLevel_PetCount[misLevel].GetKey();
        }

        //初始化任务用时
        private int InitMissionNeedTime(PetMission2 _this, int misLevel)
        {
            return PetMission2.misLevel_NeedTime[misLevel].GetKey();
        }

        //初始化任务条件(第一类)  战斗力  总随从星级
        private void InitMissionCondition(PetMission2 _this, int misLevel)
        {
            var cId = PetMission2.misLevel_Condition[misLevel].GetKey(); //随机一个条件
            PushCondition(_this, cId, InitConditionParam(_this, misLevel, cId));
        }

        //随机任务条件参数(第一类)
        private int InitConditionParam(PetMission2 _this, int misLevel, int cId)
        {
            var key = misLevel*10000 + cId*100 + _this.PetCount;
            if (cId == 10)
            {
                var totlePetLevel = _this.mCharacter.GetExData((int) eExdataDefine.e331);
                var totlePetCount = _this.mCharacter.GetExData((int) eExdataDefine.e330);
                var pvLevel = 1;
                if (totlePetCount > 0 && totlePetLevel > 0)
                {
                    pvLevel = totlePetLevel/totlePetCount;
                }
                if (pvLevel < 1)
                {
                    pvLevel = 1;
                }
                var tbLevel = Table.GetLevelData(pvLevel);
                //return tbLevel.PetPointRatio * _this.PetCount;
                return tbLevel.PetPointRatio*PetMission2.misLevel_ConditionParam[key]/MyRandom.Random(90, 110);
            }
            return PetMission2.misLevel_ConditionParam[key];
        }

        //初始化任务条件(第二类)  地点  优势种族   天气  时段
        private int InitMissionCondition2(PetMission2 _this, int misLevel)
        {
            var key = PetMission2.misLevel_Condition2[misLevel].GetKey();
            var areaParam = -1;
            while (key != 0)
            {
                var cId = key%10;
                var param = InitConditionParam2(_this, misLevel, cId);
                PushCondition(_this, cId, param);
                key = key/10;
                if (cId == (int) PetMissionConditionType.Area)
                {
                    areaParam = param;
                }
            }
            return areaParam;
        }

        //随机任务条件参数(第二类)
        private int InitConditionParam2(PetMission2 _this, int misLevel, int cId)
        {
            return PetMission2.misLevel_ConditionParam2[misLevel][cId].GetKey();
        }

        #endregion
    }

    public class PetMission2 : NodeBase
    {
        #region   数据结构

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); //
        public CharacterController mCharacter; //所在角色
        private static IPetMission2 mImpl;

        static PetMission2()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (PetMission2), typeof (PetMission2DefaultImpl),
                o => { mImpl = (IPetMission2) o; });
        }

        public int Id
        {
            get { return mDbData.Id; }
            set { mDbData.Id = value; }
        }

        public int State
        {
            get { return mDbData.State; }
            set { mDbData.State = value; }
        }

        public int FinishPro
        {
            get { return mDbData.FinishPro; }
            set { mDbData.FinishPro = value; }
        }

        public int Level
        {
            get { return mDbData.Level; }
            set { mDbData.Level = value; }
        } //任务等级

        public int Type
        {
            get { return mDbData.Type; }
            set { mDbData.Type = value; }
        } //任务类型

        public int PetCount
        {
            get { return mDbData.PetCount; }
            set { mDbData.PetCount = value; }
        } //随从数量

        public int Quality
        {
            get { return mDbData.Quality; }
            set { mDbData.Quality = value; }
        } //任务品质

        public DateTime OverTime
        {
            get { return mImpl.GetOverTime(this); }
            set { mImpl.SetOverTime(this, value); }
        }

        public List<int> PetList
        {
            get { return mDbData.PetList; }
            //set
            //{
            //    mDbData.PetList.Clear();
            //    mDbData.PetList.AddRange(value);
            //}
        }

        public DBPetMission mDbData { get; set; }
        //public object mOverTrigger = null;
        //返回任务时间修正的效果ID
        //时间  int 8小时 ：小于136  等于大于137
        public int GetMissionTime()
        {
            return mImpl.GetMissionTime(this);
        }

        public void OnDestroy()
        {
            mImpl.OnDestroy(this);
        }

        //删除该任务
        public void DeleteSelf()
        {
            mImpl.DeleteSelf(this);
        }

        //提交任务
        public ErrorCodes Commit()
        {
            return mImpl.Commit(this);
        }

        //计算完成时间
        public int GetFinishTime(List<PetItem> petItems)
        {
            return mImpl.GetFinishTime(this, petItems);
        }

        //获取网络结构
        public PetMissionData GetNetData()
        {
            return mImpl.GetNetData(this);
        }

        #endregion

        #region   构造任务

        public PetMission2(CharacterController character, DBPetMission dbdata)
        {
            mImpl.InitPetMission2(this, character, dbdata);
        }

        public PetMission2(CharacterController character, int nId, int mLevel)
        {
            mImpl.InitPetMission2(this, character, nId, mLevel);
        }

        //构造任务
        public void Init(CharacterController character, int missionGuid, int hideLevel)
        {
            mImpl.Init(this, character, missionGuid, hideLevel);
        }

        public override IEnumerable<NodeBase> Children
        {
            get { return null; }
        }

        #endregion

        #region 随机初始化任务数据

        //初始化
        public static void InitByTable()
        {
            mImpl.InitByTable();
        }

        public static Dictionary<int, DictionaryPro> hideLevel_misLevel = new Dictionary<int, DictionaryPro>();
            //key1 = 指挥部等级  key2 = 任务等级 key3 = 概率

        public static Dictionary<int, DictionaryPro> misLevel_misType = new Dictionary<int, DictionaryPro>();
            //key1 = 任务等级  key2 = 任务类型 key3 = 概率

        public static Dictionary<int, DictionaryPro> misLevel_Quality = new Dictionary<int, DictionaryPro>();
            //key1 = 任务等级  key2 = 品质 key3 = 概率

        public static Dictionary<int, DictionaryPro> misLevel_PetCount = new Dictionary<int, DictionaryPro>();
            //key1 = 任务等级  key2 = 随从数量 key3 = 概率

        public static Dictionary<int, DictionaryPro> misLevel_NeedTime = new Dictionary<int, DictionaryPro>();
            //key1 = 任务等级  key2 = 任务用时 key3 = 概率

        public static Dictionary<int, DictionaryPro> misLevel_Condition = new Dictionary<int, DictionaryPro>();
            //key1 = 任务等级  key2 = 条件ID组 key3 = 概率

        public static Dictionary<int, int> misLevel_ConditionParam = new Dictionary<int, int>();
            //key1 = 任务等级*10000 + 条件ID*100 + 随从数量   key2 = 参数

        public static Dictionary<int, DictionaryPro> misLevel_Condition2 = new Dictionary<int, DictionaryPro>();
            //key1 = 任务等级  key2 = 条件ID组 key3 = 概率

        public static Dictionary<int, Dictionary<int, DictionaryPro>> misLevel_ConditionParam2 =
            new Dictionary<int, Dictionary<int, DictionaryPro>>(); //key1 = 任务等级  key2 = 条件ID key3 = 参数 key4 = 概率

        #endregion
    }

    public interface IPetMissionManager2
    {
        PetMission2 AddMission(PetMissionManager2 _this, int missGuid);
        void AddTotleExp(PetMissionManager2 _this, int addValue);
        ErrorCodes CommitMission(PetMissionManager2 _this, int missId);
        ErrorCodes DeleteMission(PetMissionManager2 _this, int missId);
        ErrorCodes DoMission(PetMissionManager2 _this, int missId, List<int> petList, bool isBuy);
        void FinishNowDoMission(PetMissionManager2 _this);
        int GetExp(PetMissionManager2 _this);
        int GetLevel(PetMissionManager2 _this);
        PetMission2 GetMission(PetMissionManager2 _this, int missionId);
        int GetNextId(PetMissionManager2 _this);
        DBPetMissionData InitByBase(PetMissionManager2 _this, CharacterController character);
        void InitByDB(PetMissionManager2 _this, CharacterController character, DBPetMissionData mission);
        bool IsHaveMission(PetMissionManager2 _this, int missionId);
        void NetDirtyHandle(PetMissionManager2 _this);
        void OnDestroy(PetMissionManager2 _this);
        void Refresh(PetMissionManager2 _this);
        void RefreshFirst(PetMissionManager2 _this);
        void SetExp(PetMissionManager2 _this, int value);
        void SetLevel(PetMissionManager2 _this, int value);
    }

    public class PetMissionManager2DefaultImpl : IPetMissionManager2
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void SetExp(PetMissionManager2 _this, int value)
        {
            _this.mCharacter.SetExData(62, value);
        }

        public int GetExp(PetMissionManager2 _this)
        {
            return _this.mCharacter.GetExData(62);
        }

        public void SetLevel(PetMissionManager2 _this, int value)
        {
            _this.mCharacter.SetExData(63, value);
        }

        public int GetLevel(PetMissionManager2 _this)
        {
            return _this.mCharacter.GetExData(63);
        }

        #region   初始化

        //创建时的初始化
        public DBPetMissionData InitByBase(PetMissionManager2 _this, CharacterController character)
        {
            var dbData = new DBPetMissionData();
            _this.mDbData = dbData;
            _this.mCharacter = character;
            _this.MarkDirty();
            return dbData;
        }

        public void InitByDB(PetMissionManager2 _this, CharacterController character, DBPetMissionData mission)
        {
            _this.mCharacter = character;
            _this.mDbData = mission;
            var configsError = new List<DBPetMission>();
            foreach (var dbMission in mission.Missions)
            {
                var mis = new PetMission2(character, dbMission.Value);
                if (mis.mDbData == null)
                {
                    configsError.Add(dbMission.Value);
                    continue;
                }
                _this.mData.Add(dbMission.Key, mis);
                _this.AddChild(mis);
            }
            //删除数据初始化失败的
            foreach (var petMission in configsError)
            {
                mission.Missions.Remove(petMission.Id);
            }
            //删除已经需要删除的
            var removeList = new List<int>();
            foreach (var child in _this.mData)
            {
                if (child.Value.State == (int) PetMissionStateType.Delete)
                {
                    removeList.Add(child.Key);
                }
            }

            foreach (var item in removeList)
            {
                DeleteMission(_this, item);
            }
        }

        #endregion

        #region   基础接口

        //是否有某任务
        public bool IsHaveMission(PetMissionManager2 _this, int missionId)
        {
            return _this.mData.ContainsKey(missionId);
        }

        //获取任务
        public PetMission2 GetMission(PetMissionManager2 _this, int missionId)
        {
            PetMission2 pm;
            if (_this.mData.TryGetValue(missionId, out pm))
            {
                return pm;
            }
            return null;
        }

        //增加总任务经验
        public void AddTotleExp(PetMissionManager2 _this, int addValue)
        {
            if (_this.Level >= PetMissionManager2.TotleMaxLevel)
            {
                return;
            }
            var tbmissLevel = Table.GetGetMissionLevel(_this.Level);
            if (tbmissLevel == null)
            {
                return;
            }
            var nowexp = _this.Exp + addValue;
            if (nowexp >= tbmissLevel.LevelUpExp)
            {
                _this.Level = _this.Level + 1;
                _this.Exp = nowexp - tbmissLevel.LevelUpExp;
            }
            else
            {
                _this.Exp = nowexp;
            }
        }

        #endregion

        #region   刷新接口

        public int GetNextId(PetMissionManager2 _this)
        {
            return _this.mDbData.NextPetMissionId++;
        }

        //刷新普通任务
        public void Refresh(PetMissionManager2 _this)
        {
            if (!_this.mCharacter.GetFlag(PetMissionManager2.IsRefreshFlag))
            {
                return;
            }
            var build = _this.mCharacter.mCity.GetBuildByType(0);
            if (build == null)
            {
                return;
            }
            var maxCount = build.TbBs.Param[2];
            //var tbMissKu = Table.GetGetMissionInfo(Level);
            var tbMissKu = Table.GetGetMissionLevel(_this.Level);
            if (tbMissKu == null)
            {
                Logger.Error("not find petmission Level={0}", _this.Level);
                return;
            }
            var refreshCount = tbMissKu.RefleshCount;
            var nowCount = _this.mData.Count;
            if (maxCount - nowCount < refreshCount)
            {
                refreshCount = maxCount - nowCount;
            }
            PlayerLog.WriteLog(_this.mCharacter.mGuid,
                "RefreshPetMission Level={0},refreshCount={1},now={2},max={3},add={4}", _this.Level,
                _this.mDbData.RefreshCount, nowCount, maxCount, refreshCount);
            if (_this.mDbData.RefreshCount < 2)
            {
                RefreshFirst(_this);
                refreshCount -= 2;
                _this.mDbData.RefreshCount++;
                if (refreshCount < 1)
                {
                    return;
                }
            }
            for (var i = 0; i < refreshCount; i++)
            {
                AddMission(_this, GetNextId(_this));
            }
            _this.mDbData.RefreshCount++;
        }

        //第一次刷新
        public void RefreshFirst(PetMissionManager2 _this)
        {
            if (_this.mDbData.RefreshCount == 0)
            {
                var pm1 = AddMission(_this, GetNextId(_this));
                if (pm1 != null)
                {
                    pm1.Level = 1;
                    pm1.Quality = 3;
                    pm1.PetCount = 1;
                    pm1.Type = 2;
                    //"招募:得力助手"
                    pm1.mDbData.Name = "^301032";
                    pm1.mDbData.ConditionParam[0] = 5668;
                    pm1.mDbData.NeedTime = 10;
                    pm1.mDbData.ConditionIds[1] = 1;
                    pm1.mDbData.ConditionParam[1] = 0;
                    pm1.mDbData.ConditionIds[2] = 5;
                    pm1.mDbData.ConditionParam[2] = 4;
                }
                var pm2 = AddMission(_this, GetNextId(_this));
                if (pm2 != null)
                {
                    pm2.Level = 1;
                    pm2.Quality = 3;
                    pm2.PetCount = 1;
                    pm2.Type = 1;
                    //探索:成长诀窍
                    pm2.mDbData.Name = "^301033";
                    pm2.mDbData.ConditionParam[0] = 6688;
                    pm2.mDbData.NeedTime = 20;
                    pm2.mDbData.ConditionIds[1] = 1;
                    pm2.mDbData.ConditionParam[1] = 1;
                    pm2.mDbData.ConditionIds[2] = 5;
                    pm2.mDbData.ConditionParam[2] = 0;
                }
            }
            else
            {
                var pm1 = AddMission(_this, GetNextId(_this));
                if (pm1 != null)
                {
                    pm1.Level = 2;
                    pm1.Quality = 3;
                    pm1.PetCount = 1;
                    pm1.Type = 2;
                    //招募:收集魂魄
                    pm1.mDbData.Name = "^301034";
                    pm1.mDbData.ConditionParam[0] = 7324;
                    pm1.mDbData.NeedTime = 30;
                    pm1.mDbData.ConditionIds[1] = 1;
                    pm1.mDbData.ConditionParam[1] = 0;
                    pm1.mDbData.ConditionIds[2] = 5;
                    pm1.mDbData.ConditionParam[2] = 2;
                }
                var pm2 = AddMission(_this, GetNextId(_this));
                if (pm2 != null)
                {
                    pm2.Level = 1;
                    pm2.Quality = 3;
                    pm2.PetCount = 1;
                    pm2.Type = 0;
                    //掠夺:小有收获
                    pm2.mDbData.Name = "^301035";
                    pm2.mDbData.ConditionParam[0] = 6978;
                    pm2.mDbData.NeedTime = 120;
                    pm2.mDbData.ConditionIds[1] = 1;
                    pm2.mDbData.ConditionParam[1] = 1;
                    pm2.mDbData.ConditionIds[2] = 5;
                    pm2.mDbData.ConditionParam[2] = 0;
                }
            }
            //AddMission(_this, 10002);
        }

        #endregion

        #region   任务（增、做、完成、提交、删除）

        //增加任务
        public PetMission2 AddMission(PetMissionManager2 _this, int missGuid)
        {
            var mis = new PetMission2(_this.mCharacter, missGuid, _this.Level);
            _this.mDbData.Missions.Add(missGuid, mis.mDbData);
            _this.mData[missGuid] = mis;
            _this.AddChild(mis);
            mis.MarkDirty();
            return mis;
        }

        //立即到期正在执行的随从任务
        public void FinishNowDoMission(PetMissionManager2 _this)
        {
            foreach (var petMission in _this.mData)
            {
                if (petMission.Value.State == (int) PetMissionStateType.Do)
                {
                    petMission.Value.OverTime = DateTime.Now.AddSeconds(1);
                }
            }
        }

        //派随从完成任务
        public ErrorCodes DoMission(PetMissionManager2 _this, int missId, List<int> petList, bool isBuy)
        {
            var mis = GetMission(_this, missId);
            if (mis == null)
            {
                return ErrorCodes.Error_PetMissionNotFind;
            }
            var build = _this.mCharacter.mCity.GetBuildByType(0);
            if (build == null)
            {
                return ErrorCodes.Error_BuildNotFind;
            }

            //任务状态检查
            if (mis.State != (int) PetMissionStateType.New)
            {
                return ErrorCodes.Error_PetMissionState;
            }
            //宠物数量检查
            if (mis.PetCount > petList.Count || mis.PetCount < petList.Count)
            {
                return ErrorCodes.Error_PetPartakeCount;
            }
            //宠物状态验证
            var petItems = new List<PetItem>();
            foreach (var i in petList)
            {
                var pet = _this.mCharacter.GetPet(i);
                if (pet == null)
                {
                    return ErrorCodes.Error_PetNotFind;
                }
                if (pet.GetId() == -1)
                {
                    return ErrorCodes.Error_PetNotFind;
                }
                if (pet.GetState() != (int) PetStateType.Idle)
                {
                    return ErrorCodes.Error_PetState;
                }
                petItems.Add(pet);
            }

            var doMaxCount = build.TbBs.Param[1];
            var nowCount = _this.mCharacter.GetExData(64); //每日的随从数量执行计数

            if (nowCount >= doMaxCount)
            {
//当前次数已经不够了
                if (isBuy)
                {
//如果是要买次数
                    var config = Table.GetServerConfig(246).ToInt();
                    var currentTimes = _this.mCharacter.GetExData(65);
                    var needDiamonds = Table.GetSkillUpgrading(config).GetSkillUpgradingValue(currentTimes + 1);
                    if (_this.mCharacter.mBag.GetRes(eResourcesType.DiamondRes) < needDiamonds)
                    {
                        return ErrorCodes.DiamondNotEnough;
                    }
                    _this.mCharacter.mBag.DelRes(eResourcesType.DiamondRes, needDiamonds, eDeleteItemType.CityMission);
                }
                else
                {
                    return ErrorCodes.Error_DoPetMissionCountMax;
                }
            }

            //执行结果
            mis.PetList.Clear();
            foreach (var petItem in petItems)
            {
                petItem.SetState(PetStateType.Mission);
                mis.PetList.Add(petItem.GetId());
            }
            mis.State = (int) PetMissionStateType.Do;
            var seconds = mis.GetFinishTime(petItems);
            PlayerLog.WriteLog(_this.mCharacter.mGuid, "FinishTime={0}", seconds);
            mis.OverTime = DateTime.Now.AddSeconds(seconds);
            mis.MarkDirty();

            if (isBuy)
            {
                _this.mCharacter.AddExData(65, 1); //每日购买的次数
            }
            else
            {
                _this.mCharacter.AddExData(64, 1); //每日的随从数量执行计数    
            }
            //潜规则引导标记位
            if (!_this.mCharacter.GetFlag(509))
            {
                _this.mCharacter.SetFlag(509);
            }
            return ErrorCodes.OK;
        }

        ////完成任务
        //public ErrorCodes CompleteMission(int missId)
        //{
        //    var mis = GetMission(missId);
        //    if (mis == null) return ErrorCodes.Error_PetMissionNotFind;
        //    if (mis.State != (int)PetMissionStateType.Finish) return ErrorCodes.Error_PetMissionState;
        //    ErrorCodes result = mis.Complete();
        //    if (result == ErrorCodes.OK)
        //    {
        //        CommitMission(missId);
        //    }
        //    else if (result != ErrorCodes.Unknow)
        //    {
        //        return ErrorCodes.OK;
        //    }
        //    return result;
        //}
        //提交任务
        public ErrorCodes CommitMission(PetMissionManager2 _this, int missId)
        {
            var mis = GetMission(_this, missId);
            if (mis == null)
            {
                return ErrorCodes.Error_PetMissionNotFind;
            }
            if (mis.State != (int) PetMissionStateType.Do || mis.OverTime > DateTime.Now)
            {
                return ErrorCodes.Error_PetMissionState;
            }
            //if (mis.State != (int) PetMissionStateType.Finish) 
            //任务经验奖励
            var result = mis.Commit();
            if (result != ErrorCodes.OK)
            {
                return result;
            }
            //扩展计数
            _this.mCharacter.AddExData((int) eExdataDefine.e414, 1);
            _this.mCharacter.AddExData((int) eExdataDefine.e273, 1);
            //删除
            DeleteMission(_this, missId);

            return ErrorCodes.OK;
        }

        //删除任务
        public ErrorCodes DeleteMission(PetMissionManager2 _this, int missId)
        {
            var mis = GetMission(_this, missId);
            if (mis == null)
            {
                return ErrorCodes.Error_PetMissionNotFind;
            }
            if (mis.State == (int) PetMissionStateType.Do)
            {
                return ErrorCodes.Error_PetMissionState;
            }
            mis.OnDestroy();
            _this.mDbData.Missions.Remove(missId);
            _this.mData.Remove(missId);
            if (null != _this.mCharacter && null != _this.mCharacter.Proxy)
            {
                _this.mCharacter.Proxy.DeletePetMission(missId);
            }
            _this.MarkDbDirty();

            return ErrorCodes.OK;
        }

        #endregion

        #region 节点相关

        public void NetDirtyHandle(PetMissionManager2 _this)
        {
            var msg = new PetMissionList();
            foreach (var mission in _this.Children)
            {
                if (mission.NetDirty) //脏任务
                {
                    var tempMission = (PetMission2) mission;
                    //PetMissionData tempChanges = new PetMissionData()
                    //{
                    //    Id = tempMission.Id,
                    //    State = tempMission.State,
                    //    OverTime = tempMission.mDbData.OverTime,
                    //    FinishPro = tempMission.FinishPro,
                    //    PetCount = tempMission.PetCount,
                    //    Level = tempMission.Level,
                    //    Type = tempMission.Type,
                    //    Quality = tempMission.Quality,
                    //    Name = tempMission.mDbData.Name,
                    //};
                    //tempChanges.ConditionIds.AddRange(tempMission.mDbData.ConditionIds);
                    //tempChanges.ConditionParam.AddRange(tempMission.mDbData.ConditionParam);
                    //tempChanges.PetList.AddRange(tempMission.PetList);
                    msg.Data.Add(tempMission.GetNetData());
                }
            }
            _this.mCharacter.Proxy.SyncPetMission(msg);
        }

        public void OnDestroy(PetMissionManager2 _this)
        {
            foreach (var child in _this.Children)
            {
                var petmission = child as PetMission2;
                if (petmission == null)
                {
                    continue;
                }
                petmission.OnDestroy();
            }
        }

        #endregion
    }

    public class PetMissionManager2 : NodeBase
    {
        #region   数据结构

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static int TotleMaxLevel = 10;
        public CharacterController mCharacter; //所在角色
        public Dictionary<int, PetMission2> mData = new Dictionary<int, PetMission2>();
        public static int IsRefreshFlag = Table.GetServerConfig(1100).ToInt();
        private static IPetMissionManager2 mImpl;
        public DBPetMissionData mDbData;

        static PetMissionManager2()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (PetMissionManager2),
                typeof (PetMissionManager2DefaultImpl),
                o => { mImpl = (IPetMissionManager2) o; });
        }

        public int Exp
        {
            get { return mImpl.GetExp(this); }
            set { mImpl.SetExp(this, value); }
        }

        public int Level
        {
            get { return mImpl.GetLevel(this); }
            set { mImpl.SetLevel(this, value); }
        }

        #endregion

        #region   初始化

        //创建时的初始化
        public DBPetMissionData InitByBase(CharacterController character)
        {
            return mImpl.InitByBase(this, character);
        }

        public void InitByDB(CharacterController character, DBPetMissionData mission)
        {
            mImpl.InitByDB(this, character, mission);
        }

        #endregion

        #region   基础接口

        //是否有某任务
        public bool IsHaveMission(int missionId)
        {
            return mImpl.IsHaveMission(this, missionId);
        }

        //获取任务
        public PetMission2 GetMission(int missionId)
        {
            return mImpl.GetMission(this, missionId);
        }

        //增加总任务经验
        public void AddTotleExp(int addValue)
        {
            mImpl.AddTotleExp(this, addValue);
        }

        #endregion

        #region   刷新接口

        public int GetNextId()
        {
            return mImpl.GetNextId(this);
        }

        //刷新普通任务
        public void Refresh()
        {
            mImpl.Refresh(this);
        }

        //第一次刷新
        public void RefreshFirst()
        {
            mImpl.RefreshFirst(this);
        }

        #endregion

        #region   任务（增、做、完成、提交、删除）

        //增加任务
        public PetMission2 AddMission(int missGuid)
        {
            return mImpl.AddMission(this, missGuid);
        }

        //立即到期正在执行的随从任务
        public void FinishNowDoMission()
        {
            mImpl.FinishNowDoMission(this);
        }

        //派随从完成任务
        public ErrorCodes DoMission(int missId, List<int> petList, bool isBuy)
        {
            return mImpl.DoMission(this, missId, petList, isBuy);
        }

        public ErrorCodes CommitMission(int missId)
        {
            return mImpl.CommitMission(this, missId);
        }

        //删除任务
        public ErrorCodes DeleteMission(int missId)
        {
            return mImpl.DeleteMission(this, missId);
        }

        #endregion

        #region 节点相关

        public override IEnumerable<NodeBase> Children
        {
            get { return mData.Values; }
        }

        public override void NetDirtyHandle()
        {
            mImpl.NetDirtyHandle(this);
        }

        public void OnDestroy()
        {
            mImpl.OnDestroy(this);
        }

        #endregion
    }
}