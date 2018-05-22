#region using

using System;
using System.Collections.Generic;
using DataContract;
using DataTable;
using Mono.GameMath;
using NLog;
using Shared;

#endregion

namespace Scene
{
    public partial class ObjCharacterDefaultImpl
    {
        //最后一个攻击我的敌人id
        public ulong GetLastEnemyId(ObjCharacter _this)
        {
            if (_this.EnemyList.Count > 0)
            {
                return _this.EnemyList[_this.EnemyList.Count - 1];
            }
            return TypeDefine.INVALID_ULONG;
        }

        #region 初始化

        public virtual bool InitEquip(ObjCharacter _this, int level)
        {
            return true;
        }

        public virtual bool InitAttr(ObjCharacter _this, int level)
        {
            _this.Attr.InitAttributesAll();
            return true;
        }

        public virtual bool InitSkill(ObjCharacter _this, int level)
        {
            foreach (var skillId in _this.TableCharacter.InitSkill)
            {
                if (-1 != skillId)
                {
                    _this.Skill.AddSkill(skillId, level, eAddskillType.InitSkill);
                }
            }

            return true;
        }

        public virtual bool InitBuff(ObjCharacter _this, int level)
        {
            return true;
        }

        #endregion

        #region 技能限制

        public bool CanSkill(ObjCharacter _this)
        {
            return DateTime.Now >= _this.mCanSkill;
        }

        public bool CanMove(ObjCharacter _this)
        {
            if (DateTime.Now < _this.mCanMove)
            {
                return false;
            }

            if (_this.BuffList.IsNoMove())
            {
                return false;
            }
            return true;
        }

        public void SetSkill(ObjCharacter _this, int SkillTime)
        {
            _this.mCanSkill = DateTime.Now.AddMilliseconds(SkillTime);
        }

        public void SetMove(ObjCharacter _this, int MoveTime)
        {
            //_this.mCanMove = DateTime.Now.AddMilliseconds(MoveTime);
        }

        #endregion

        #region 数据同步

        //技能发生了变化 nType=变化规则{0删除，1新增，2修改} nId=技能ID   nLevel=技能等级
        public void SkillChange(ObjCharacter _this, int nType, int nId, int nLevel)
        {
            switch (nType)
            {
                case 0:
                    {
                        _this.Skill.DelSkill(nId);
                    }
                    break;
                case 1:
                    {
                        _this.Skill.AddSkill(nId, nLevel, eAddskillType.SkillChange);
                    }
                    break;
                case 2:
                    {
                        _this.Skill.ResetSkill(nId, nLevel);
                    }
                    break;
            }
        }

        //重新装备技能
        public void EquipSkill(ObjCharacter _this, List<int> dels, List<int> adds, List<int> lvls)
        {
            //添加技能
            var count = Table.GetServerConfig(700).ToInt();
            for (var i = 0; i != count; ++i)
            {
                if (i >= adds.Count)
                {
                    break;
                }
                var addId = adds[i];
                if (addId == -1)
                {
                    continue;
                }
                if (dels.Contains(addId))
                {
                    continue; //如果已有，则不需添加
                }

                var AddSkill = _this.Skill.AddSkill(addId, lvls[i], eAddskillType.EquipSkill);
                var delId = -1;
                if (i < dels.Count)
                {
                    delId = dels[i];
                }
                if (delId != -1)
                {
                    var delSkill = _this.Skill.GetSkill(delId);
                    if (delSkill != null)
                    {
                        if (delSkill.mDoCount != delSkill.mDoMax)
                        {
                            //如果对应位置有要删除的技能，并且该技能没有完全CD，那么此技能直接进入CD状态
                            AddSkill.mDoCount = 0;
                            AddSkill.CreateCdTrigger();
                        }
                    }
                }
            }
            //删除不需要添加的技能
            foreach (var i in dels)
            {
                if (adds.Contains(i))
                {
                    continue;
                }
                if (i == -1)
                {
                    continue;
                }
                _this.Skill.DelSkill(i);
            }
        }

        //天赋发生了变化 nType=变化规则{0删除，1新增，2修改,3清空} nId=天赋ID   nLevel=天赋层数
        public void TalentChange(ObjCharacter _this, int nType, int nId, int nLayer)
        {
            switch (nType)
            {
                case 0:
                    {
                        _this.Skill.DelTalent(nId);
                    }
                    break;
                case 1:
                    {
                        _this.Skill.AddTalent(nId, nLayer);
                    }
                    break;
                case 2:
                    {
                        _this.Skill.ResetTalent(nId, nLayer);
                    }
                    break;
                case 3:
                    {
                        _this.Skill.ResetAllTalent();
                    }
                    break;
            }
            _this.Skill.SetFightPointFlag();
            _this.Attr.SetFightPointFlag();
        }

        #endregion

        #region 技能相关

        //检查技能是否能使用
        public ErrorCodes CheckUseSkill(ObjCharacter _this, ref int skillId, ObjCharacter target = null)
        {
            return _this.Skill.CheckSkill(ref skillId, target);
        }

        public ErrorCodes RequestUseSkill(ObjCharacter _this,
                                          int skillId,
                                          List<int> skillIds,
                                          ObjCharacter target = null)
        {
            var ret = _this.Skill.CheckSkill(ref skillId, target);
            if (ret == ErrorCodes.OK)
            {
                _this.Skill.WillSkill(skillId);
                ret = _this.Skill.DoSkill(ref skillId, target);
                if (ErrorCodes.OK == ret)
                {
                    var effectSkillId = _this.Skill.GetModifySkillValue(skillId, (int)eModifySkillType.BroadcastSkillId,
                        skillId); //计算用于播特效的技能ID
                    ObjCharacter obj = null;
                    _this.Skill.LastSkillMainTarget.TryGetTarget(out obj);
                    OnUseSkill(_this, effectSkillId, obj);

                    skillIds.Add(skillId);
                    if (effectSkillId != skillId)
                    {
                        skillIds.Add(effectSkillId); //skillID 要走CD的   effectSkillId播特效
                    }
                }
            }
            return ret;
        }

        //使用技能
        public virtual ErrorCodes UseSkill(ObjCharacter _this, ref int skillId, ObjCharacter target = null)
        {
            var ret = _this.Skill.CheckSkill(ref skillId, target);
            if (ret == ErrorCodes.OK)
            {
                _this.Skill.WillSkill(skillId);
                ret = _this.Skill.DoSkill(ref skillId, target);
                if (ErrorCodes.OK == ret)
                {
                    skillId = _this.Skill.GetModifySkillValue(skillId, (int)eModifySkillType.BroadcastSkillId, skillId);
                    //计算用于播特效的技能ID
                    ObjCharacter obj = null;
                    _this.Skill.LastSkillMainTarget.TryGetTarget(out obj);
                    OnUseSkill(_this, skillId, obj);
                }
            }
            return ret;
        }

        //不判断技能
        public ErrorCodes MustSkill(ObjCharacter _this, ref int skillId, ObjCharacter target = null)
        {
            _this.Skill.WillSkill(skillId);
            var ret = _this.Skill.DoSkill(ref skillId, target);
            if (ErrorCodes.OK == ret)
            {
                //OnUseSkill(skillId, Skill.LastSkillMainTarget);
                OnUseSkill(_this, skillId, target);
            }
            return ret;
        }

        //使用技能成功
        public virtual void OnUseSkill(ObjCharacter _this, int skillId, ObjCharacter target)
        {
            _this.BroadcastUseSkill(skillId, target);
        }

        #endregion

        #region Buff相关

        public BuffData AddBuff(ObjCharacter _this, BuffRecord tbBuff, int bufflevel, ObjCharacter casterHero)
        {
            var buffid = tbBuff.Id;
            var buff = ObjectPool<BuffData>.NewObject();
            //var buff = ObjectPool<BuffData>.NewObject();
            buff.Reset(_this.BuffList.GetNextUniqueId(), buffid, bufflevel, casterHero, tbBuff, casterHero, eHitType.Hit,
                1.0f);
            //mBuff = tbbuff,
            buff.mBuff = tbBuff;
            buff.Init();
            _this.BuffList.mData.Add(buff);
            GetBuff.DoEffect(null, _this, buff, 0);
            return buff;
        }

        public ErrorCodes CheckAddBuff(ObjCharacter _this, int buffid, int bufflevel, ObjCharacter casterHero)
        {
            if (buffid < 0)
            {
                return ErrorCodes.Error_BuffID;
            }
            var tbBuff = Table.GetBuff(buffid);
            if (tbBuff == null)
            {
                return ErrorCodes.Error_BuffID;
            }
            if (_this.IsInvisible())
            {
                return ErrorCodes.Error_BuffID;
            }
            if (tbBuff.Die != 1)
            {
                if (_this.mIsDead)
                {
                    //Logger.Log(LogLevel.Warn, string.Format("[{0}]死了！Buff释放失败", ObjId));
                    Logger.Info("[{0}]死了！Buff释放失败", _this.ObjId);
                    return ErrorCodes.Error_Death;
                }
            }
            //免疫Buff   目前没有类型  暂留
            if (_this.BuffList.GetEffectParam_Bin(eEffectType.NoBuffType, 0, tbBuff.Type))
            {
                Logger.Info("[{0}]免疫了Buff={1}", _this.ObjId, buffid);
                return ErrorCodes.Error_BuffLevelTooLow;
            }
            //能否上去的检查  互斥  优先级  替换ID  承受者上限
            if (tbBuff.HuchiId != -1)
            {
                //互斥检查
                var bearmax = tbBuff.BearMax;
                BuffData huchiBuff;
                if (bearmax == 1)
                {
                    huchiBuff = _this.BuffList.Get_Same_Caster_Huchi_Buff(tbBuff.HuchiId);
                    // Get_Same_Caster_Huchi_Buff(tbBuff.HuchiId);
                }
                else
                {
                    huchiBuff = _this.BuffList.Get_Same_Caster_Huchi_Buff(tbBuff.HuchiId, casterHero);
                    //huchiBuff = Get_Same_Caster_Huchi_Buff(tbBuff.HuchiId, casterHero);
                }
                if (huchiBuff != null)
                {
                    var tbHuchiBuff = huchiBuff.mBuff;
                    if (tbHuchiBuff.TihuanId != tbBuff.TihuanId)
                    {
                        //替换ID不一样，则直接替换
                    }
                    else if (tbHuchiBuff.PriorityId > tbBuff.PriorityId)
                    {
                        //新Buff优先级低，需要被抛弃
                        return ErrorCodes.Error_BuffLevelTooLow;
                    }
                }
            }

            return ErrorCodes.OK;
        }

        /// 获得Buff时
        /// <summary>
        ///     增加Buff
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="buffid">BuffId</param>
        /// <param name="bufflevel"></param>
        /// <param name="casterHero">施放者</param>
        /// <param name="DelayView"></param>
        /// <param name="hitType">命中类型</param>
        /// <param name="fBili">Buff的修正比例</param>
        /// <param name="buffLastTime">Buff持续时间 部分buff的持续时间不读buff表</param>
        /// <returns></returns>
        public BuffData AddBuff(ObjCharacter _this,
                                int buffid,
                                int bufflevel,
                                ObjCharacter casterHero,
                                int DelayView = 0,
                                eHitType hitType = eHitType.Hit,
                                float fBili = 1.0f,
                                double buffLastTime = 0)
        {
            if (buffid < 0)
            {
                return null; //ErrorCodes.Error_BuffID;
            }
            var tbBuff = Table.GetBuff(buffid);
            if (tbBuff == null)
            {
                return null; //ErrorCodes.Error_BuffID;
            }
            if (_this.IsInvisible())
            {
                return null;
            }
            if (tbBuff.Die != 1)
            {
                if (_this.mIsDead)
                {
                    //Logger.Log(LogLevel.Warn, string.Format("[{0}]死了！Buff释放失败", ObjId));
                    Logger.Info("[{0}]死了！Buff释放失败", _this.ObjId);
                    return null; //ErrorCodes.Error_Death;
                }
            }
            //免疫Buff   目前没有类型  暂留
            if (_this.BuffList.GetEffectParam_Bin(eEffectType.NoBuffType, 0, tbBuff.Type))
            {
                Logger.Info("[{0}]免疫了Buff={1}", _this.ObjId, buffid);
                return null;
            }
            //能否上去的检查  互斥  优先级  替换ID  承受者上限
            if (tbBuff.HuchiId != -1)
            {
                //互斥检查
                var bearmax = tbBuff.BearMax;
                BuffData huchiBuff;
                if (bearmax == 1)
                {
                    huchiBuff = _this.BuffList.Get_Same_Caster_Huchi_Buff(tbBuff.HuchiId);
                    // Get_Same_Caster_Huchi_Buff(tbBuff.HuchiId);
                }
                else
                {
                    huchiBuff = _this.BuffList.Get_Same_Caster_Huchi_Buff(tbBuff.HuchiId, casterHero);
                    //huchiBuff = Get_Same_Caster_Huchi_Buff(tbBuff.HuchiId, casterHero);
                }
                if (huchiBuff != null)
                {
                    var tbHuchiBuff = huchiBuff.mBuff;
                    if (tbHuchiBuff.TihuanId != tbBuff.TihuanId)
                    {
                        //替换ID不一样，则直接替换
                        //LogSystem.WriteLog(enmLOG_TYPE.LOG_TYPE_FIGHT, "[{0}]对[{1}]上Buff={2},互斥掉了Buff={3}", Table.hero[caster_hero.DataID].Name, Table.hero[_this.DataID].Name, buffid, huchi_buff.DataID);
                        MissBuff.DoEffect(_this.Scene, _this, huchiBuff);
                        DeleteBuff(_this, huchiBuff, eCleanBuffType.Huchi);
                    }
                    else if (tbHuchiBuff.PriorityId > tbBuff.PriorityId)
                    {
                        //新Buff优先级低，需要被抛弃
                        return null; //ErrorCodes.Error_BuffLevelTooLow;
                    }
                    else if (tbHuchiBuff.PriorityId == tbBuff.PriorityId)
                    {
                        //叠加逻辑
                        if (huchiBuff.GetLayer() < tbHuchiBuff.LayerMax)
                        {
                            //层数没到上限
                            huchiBuff.SetLayer(huchiBuff.GetLayer() + 1);
                        }
                        if (tbBuff.RefleshRule == 1)
                        {
                            huchiBuff.AddDuration((float)tbHuchiBuff.Duration / 1000);
                        }
                        else
                        {
                            huchiBuff.SetDuration((float)tbHuchiBuff.Duration / 1000, true);
                        }
                        BuffList.ModifyBuffTableData(casterHero, huchiBuff, huchiBuff.m_nBuffId, bufflevel);
                        //LogSystem.WriteLog(enmLOG_TYPE.LOG_TYPE_FIGHT, "[{0}]对[{1}]上Buff={2},导致Buff层数+1,当前层数{3}", Table.hero[caster_hero.DataID].Name, Table.hero[_this.DataID].Name, huchi_buff.DataID, huchi_buff.Layer);
                        //获得BUFF的事件
                        //Logger.Trace("Addbuff 叠加ID={0}, Layer={1} ", buffid, huchiBuff.GetLayer());
                        GetBuff.DoEffect(_this.Scene, _this, huchiBuff, DelayView);
                        //仅仅是生效了一下的Buff
                        if (tbHuchiBuff.Effect[0] != -1 || tbHuchiBuff.IsView == 1)
                        {
                            var replyMsg = new BuffResultMsg();
                            var buffResult = new BuffResult
                            {
                                SkillObjId = casterHero.ObjId,
                                TargetObjId = _this.ObjId,
                                BuffTypeId = buffid,
                                BuffId = huchiBuff.mId,
                                Type = BuffType.HT_EFFECT,
                                ViewTime = Extension.AddTimeDiffToNet(DelayView)
                            };
                            if (huchiBuff.mBuff.IsView == 1)
                            {
                                if (buffLastTime > 0)
                                {
                                    huchiBuff.SetDuration((int)buffLastTime);
                                }
                                buffResult.Param.Add(huchiBuff.GetLastSeconds());
                                buffResult.Param.Add(huchiBuff.GetLayer());
                                buffResult.Param.Add(huchiBuff.m_nLevel);
                            }
                            replyMsg.buff.Add(buffResult);
                            _this.BroadcastBuffList(replyMsg);
                        }
                        return huchiBuff; //ErrorCodes.OK;
                    }
                    else
                    {
                        //需要删除老buff
                        MissBuff.DoEffect(_this.Scene, _this, huchiBuff);
                        DeleteBuff(_this, huchiBuff, eCleanBuffType.Huchi);
                    }
                }
            }
            else
            {
                var huchiBuff = _this.BuffList.Get_Same_Caster_Same_Buff(tbBuff.Id, casterHero);
                if (huchiBuff != null)
                {
                    var tbHuchiBuff = huchiBuff.mBuff;
                    if (huchiBuff.GetLayer() < tbHuchiBuff.LayerMax)
                    {
                        //层数没到上限
                        huchiBuff.SetLayer(huchiBuff.GetLayer() + 1);
                    }
                    huchiBuff.SetDuration((float)tbHuchiBuff.Duration / 1000, true);
                    BuffList.ModifyBuffTableData(casterHero, huchiBuff, huchiBuff.m_nBuffId, bufflevel);
                    //获得BUFF的事件
                    //Logger.Trace("Addbuff 叠加ID={0}, Layer={1} ", buffid, huchiBuff.GetLayer());
                    GetBuff.DoEffect(_this.Scene, _this, huchiBuff, DelayView);
                    //仅仅是生效了一下的Buff
                    if (tbHuchiBuff.Effect[0] != -1 || tbHuchiBuff.IsView == 1)
                    {
                        var replyMsg = new BuffResultMsg();
                        var buffResult = new BuffResult
                        {
                            SkillObjId = casterHero.ObjId,
                            TargetObjId = _this.ObjId,
                            BuffTypeId = buffid,
                            BuffId = huchiBuff.mId,
                            Type = BuffType.HT_EFFECT,
                            ViewTime = Extension.AddTimeDiffToNet(DelayView)
                        };
                        if (huchiBuff.mBuff.IsView == 1)
                        {
                            if (buffLastTime > 0)
                            {
                                huchiBuff.SetDuration((int)buffLastTime);
                            }
                            buffResult.Param.Add(huchiBuff.GetLastSeconds());
                            buffResult.Param.Add(huchiBuff.GetLayer());
                            buffResult.Param.Add(huchiBuff.m_nLevel);
                        }
                        replyMsg.buff.Add(buffResult);
                        _this.BroadcastBuffList(replyMsg);
                    }
                    return huchiBuff; //ErrorCodes.OK;
                }
            }
            //增加Buff
            var buff = _this.BuffList.AddBuff(buffid, bufflevel, casterHero, _this, fBili, hitType);
            //获得BUFF的事件
            GetBuff.DoEffect(_this.Scene, _this, buff, DelayView);
            //有可能立即删除Buff
            if (buff.mBuff.Duration == 0)
            {
                DeleteBuff(_this, buff, eCleanBuffType.TimeOver);
                //仅仅是生效了一下的Buff
                if (tbBuff.Effect[0] != -1 || tbBuff.IsView == 1)
                {
                    var replyMsg = new BuffResultMsg();
                    replyMsg.buff.Add(new BuffResult
                    {
                        SkillObjId = casterHero.ObjId,
                        TargetObjId = _this.ObjId,
                        BuffTypeId = buffid,
                        BuffId = buff.mId,
                        Type = BuffType.HT_EFFECT,
                        ViewTime = Extension.AddTimeDiffToNet(DelayView)
                    });
                    _this.BroadcastBuffList(replyMsg);
                }
            }
            else
            {
                //将会持续一段时间的Buff
                if (tbBuff.Effect[0] != -1 || tbBuff.IsView == 1)
                {
                    var replyMsg = new BuffResultMsg();
                    var buffResult = new BuffResult
                    {
                        SkillObjId = casterHero.ObjId,
                        TargetObjId = _this.ObjId,
                        BuffTypeId = buffid,
                        BuffId = buff.mId,
                        Type = BuffType.HT_ADDBUFF,
                        ViewTime = Extension.AddTimeDiffToNet(DelayView)
                    };
                    if (buff.mBuff.IsView == 1)
                    {
                        if (buffLastTime > 0)
                        {
                            buff.SetDuration((int)buffLastTime);
                        }
                        buffResult.Param.Add(buff.GetLastSeconds());
                        buffResult.Param.Add(buff.GetLayer());
                        buffResult.Param.Add(buff.m_nLevel);
                    }
                    if (buff.GetActive())
                    {
                        replyMsg.buff.Add(buffResult);
                    }

                    _this.BroadcastBuffList(replyMsg);
                }
            }
            _this.BuffList.Do_Del_Buff(); //这个不一定可以删，如果外边有循环的话，可能会有问题
            return buff; //ErrorCodes.OK;
        }

        /// 删除Buff
        /// <summary>
        ///     删除Buff (知道Buff实例)
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="deletebuff">Buff实例</param>
        /// <param name="type">删除的原因类型</param>
        public void DeleteBuff(ObjCharacter _this, BuffData deletebuff, eCleanBuffType type)
        {
#if DEBUG
            string because;
            switch (type)
            {
                case eCleanBuffType.TimeOver:
                    because = "TimeOver";
                    break;
                case eCleanBuffType.Huchi:
                    because = "BuffHuchi";
                    break;
                case eCleanBuffType.Clear:
                    because = "Clear";
                    break;
                case eCleanBuffType.EffectOver:
                    because = "EffectOver";
                    break;
                case eCleanBuffType.LayerZero:
                    because = "LayerZero";
                    break;
                case eCleanBuffType.ForgetSkill:
                    because = "ForgetSkill";
                    break;
                case eCleanBuffType.ForgetTalent:
                    because = "ForgetTalent";
                    break;
                case eCleanBuffType.EquipTie:
                    because = "EquipTie";
                    break;
                case eCleanBuffType.Die:
                    because = "Die";
                    break;
                case eCleanBuffType.RetinueDie:
                    because = "RetinueDie";
                    break;
                case eCleanBuffType.DeleteEquip:
                    because = "DeleteEquip";
                    break;
                case eCleanBuffType.RemoveElf:
                    because = "RemoveElf";
                    break;
                case eCleanBuffType.AbsorbOver:
                    because = "AbsorbOver";
                    break;
                case eCleanBuffType.GoHome:
                    because = "GoHome";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
            if (_this is ObjPlayer)
            {
                Logger.Log(LogLevel.Info,
                    string.Format("{0} missbuff={1}({2})", _this.ObjId, deletebuff.GetBuffId(), because));
            }
#endif

            if (deletebuff.mBuff.Effect[0] != -1 || deletebuff.mBuff.IsView == 1)
            {
                var replyMsg = new BuffResultMsg();
                replyMsg.buff.Add(new BuffResult
                {
                    SkillObjId = deletebuff.mCasterId,
                    TargetObjId = deletebuff.GetBear().ObjId,
                    BuffTypeId = deletebuff.GetBuffId(),
                    BuffId = deletebuff.mId,
                    Type = BuffType.HT_DELBUFF,
                    Damage = (int)type
                });
                _this.BroadcastBuffList(replyMsg);
            }
            _this.BuffList.DelBuff(deletebuff);
        }

        /// 删除Buff
        /// <summary>
        ///     删除Buff (知道BuffId)
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="nBuffId">BuffId</param>
        /// <param name="type">删除的原因类型</param>
        public void DeleteBuff(ObjCharacter _this, int nBuffId, eCleanBuffType type)
        {
            var buffs = _this.BuffList.GetBuffById(nBuffId);
            foreach (var buff in buffs)
            {
                MissBuff.DoEffect(_this.Scene, _this, buff);
                DeleteBuff(_this, buff, type);
            }
        }

        #endregion

        #region 效果相关

        //造成绝对伤害
        /// <summary>
        ///     造成伤害
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="hitType">命中类型</param>
        /// <param name="buff">来源Buff</param>
        /// <param name="caster">造成者</param>
        /// <param name="delayView"></param>
        /// <param name="damage">伤害量</param>
        /// <param name="damageType">伤害类型</param>
        public void DoRealDamage(ObjCharacter _this,
                                 eHitType hitType,
                                 BuffData buff,
                                 ObjCharacter caster,
                                 int delayView,
                                 ref int damage,
                                 int damageType)
        {
            if (_this.BuffList.GetEffectParam_And(eEffectType.NoBuffType, 2, damageType))
            {
                //免疫
                damage = 0;
                return;
            }
            //伤害最小为1
            if (damage < 1)
            {
                damage = 1;
            }
            //伤害计算
            //int old_damage = damage;
            var old_hp = _this.Attr.GetDataValue(eAttributeType.HpNow);
            //int now_maxhp = _this.Attr.GetDataValue(eAttributeType.HpMax);
            //int old_hp_bili = old_hp * 10000 / now_maxhp;
            var new_hp = old_hp - damage;
            new_hp = Math.Max(new_hp, 0);
            //int new_hp_bili = new_hp * 10000 / now_maxhp;
            _this.Attr.SetDataValue(eAttributeType.HpNow, new_hp);

#if DEBUG
            //输出
            var damagett = "";
            if (damageType == 0)
            {
                damagett = "物理";
            }
            else if (damageType == 1)
            {
                damagett = "法术";
            }
            else if (damageType == 2)
            {
                damagett = "无视护甲";
            }
            else if (damageType == 3)
            {
                damagett = "无视法抗";
            }
            else if (damageType == 4)
            {
                damagett = "流血";
            }
            else if (damageType == 5)
            {
                damagett = "反弹";
            }
            if (caster is ObjPlayer)
            {
                Logger.Info("id={0} 的角色受到了{1}点{2}伤害", _this.ObjId, damage, damagett);
            }
#endif
            //死亡检测
            _this.OnDamage(caster, damage);
            CheckDie(_this, buff, caster, delayView, damage);
        }

        private void OnLoseHp(ObjCharacter _this, int value)
        {
            var oldHp = _this.Attr.GetDataValue(eAttributeType.HpNow);
            var newHp = oldHp - value;
            newHp = Math.Max(newHp, 0);
            _this.Attr.SetDataValue(eAttributeType.HpNow, newHp);

            HpLessThanCentainPercent.DoEffect(_this.Scene, _this);
        }

        /// <summary>
        /// 根据攻击被击方战斗力，从表格匹配修正的数值
        /// </summary>
        /// <param name="sendFP"></param>
        /// <param name="receiveFP"></param>
        /// <returns></returns>
        private float GetAdjustByFightPoint(int sendFP, int receiveFP)
        {
            int row = (sendFP / 50000) - 1;
            // 从ID=1开始
            row = Math.Max(1, row);
            row = Math.Min(100, row);
            // 从第0列开始
            int col = (receiveFP / 50000) - 2;
            col = Math.Max(0, col);

            var record = Table.GetBattleCorrect(row);
            if (null == record)
            {
                Logger.Error("Battle Correct not find id = {0}", row);
                // 没找到，先取目前最大的59
                record = Table.GetBattleCorrect(59);
            }
            col = Math.Min(record.MyFight.Length - 1, col);
            int value = record.MyFight[col];
            /*
            int tmpReceive = receiveFP;
            int tmpFightPoint = 0;
            int Id = 0;
            List<int> listFightPoint = new List<int>();
            Table.ForeachBattleCorrect(record =>
                {
                    if (0 == Id)
                    {// 取最小战斗力
                        tmpReceive = Math.Max(receiveFP, record.Id);
                    }
                    Id = record.Id;
                    listFightPoint.Add(record.Id);
                    var recordFP = record.Id;
                    if (sendFP > recordFP)
                    {
                        tmpFightPoint = recordFP;
                    }
                    if (tmpFightPoint < sendFP && sendFP <= recordFP)
                    {
                        return false;
                    }
                    return true;
                }
            );

            int value = 0;
            var configFPs = Table.GetBattleCorrect(Id);
            for (int i = listFightPoint.Count - 1; i >= 0; --i)
            {
                if (listFightPoint[i] <= tmpReceive)
                {
                    value = configFPs.MyFight[i];
                    break;
                }
            }
            */
            return value / 10000.0f;
        }

        /// <summary>
        /// 修正PVP的伤害
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="enemy"></param>
        /// <param name="damage"></param>
        /// <returns></returns>
        protected int AdjustPvPDamage(ObjCharacter _this, ObjCharacter enemy, int damage)
        {
            if (ObjType.PLAYER != _this.GetObjType() || ObjType.PLAYER != enemy.GetObjType())
                return damage;

            if (null == _this.Scene || null == _this.Scene.TableSceneData || enemy == _this)
                return damage;

            // 未开启修正
            var tbConfig = Table.GetServerConfig(270);
            if (null == tbConfig || 1 != tbConfig.ToInt())
                return damage;

            float adjust = GetAdjustByFightPoint(enemy.Attr.GetFightPoint(), _this.Attr.GetFightPoint());
            return (int)(damage * (1.0f - adjust));
        }

        //造成伤害
        /// <summary>
        ///     造成伤害
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="hitType">命中类型</param>
        /// <param name="buff">来源Buff</param>
        /// <param name="caster">造成者</param>
        /// <param name="delayView"></param>
        /// <param name="damage">伤害量</param>
        /// <param name="damageType">伤害类型</param>
        /// <param name="absorbDamage">吸收伤害</param>
        public void DoDamage(ObjCharacter _this,
                             eHitType hitType,
                             BuffData buff,
                             ObjCharacter caster,
                             int delayView,
                             ref int damage,
                             int damageType,
                             ref int absorbDamage)
        {
            if (_this.BuffList.GetEffectParam_Bin(eEffectType.NoBuffType, 2, damageType))
            {
                //免疫
                damage = 0;
                return;
            }
            //释放方与承受方   增伤,减伤生效
            BuffEffect.CASTER_REF_DAMAGE(caster, _this, ref damage, damageType);
            BuffEffect.BEAR_REF_DAMAGE(caster, _this, ref damage, damageType);

            absorbDamage = BuffEffect.AbsorbInjury(_this, damageType, ref damage);
#if DEBUG
            if (absorbDamage > 0 && caster is ObjPlayer)
            {
                Logger.Info("id={0} 的角色吸收了{1}点{2}伤害", _this.ObjId, absorbDamage, damageType);
            }
#endif
            // 战力减伤 PVP修正
            damage = AdjustPvPDamage(_this, caster, damage);

            if (damage <= 0)
            {
                damage = 0;
                return;
            }
            //潜规则(目前怪物等级过高)
            //if (caster is ObjNPC)
            //{
            //    damage = damage/2;
            //}
            //伤害最小为1
            if (damage < 1)
            {
                damage = 1;
            }
            //伤害计算
            //int old_damage = damage;
            var realDamage = damage;
            if (_this.Scene != null && _this.Scene.isNeedDamageModify)
            {
                var b = Scene.IsNeedChangeHp(_this);
                var c = Scene.IsNeedChangeHp(caster);
                if (b != null && c == null)
                {
                    realDamage = _this.Scene.PlayerDoDamageModify(realDamage);
                }
                else if (b == null && c != null)
                {
                    damage = _this.Scene.NpcDoDamageModify(damage);
                    realDamage = damage;
                }
                //if (this is ObjNPC)//打怪
                //{
                //    if (caster is ObjPlayer)
                //    {
                //        realDamage = Scene.PlayerDoDamageModify(realDamage);
                //    }
                //    else if (caster is ObjRetinue)
                //    {
                //        var c = caster as ObjRetinue;
                //        if (c.Owner is ObjPlayer)
                //        {
                //            realDamage = Scene.PlayerDoDamageModify(realDamage);
                //        }
                //    }
                //}
                //else
                //{//打人
                //    if (caster is ObjNPC)
                //    {
                //        damage = Scene.NpcDoDamageModify(damage);
                //        realDamage = damage;
                //    }
                //}
            }

            OnLoseHp(_this, realDamage);

#if DEBUG
            //输出
            var damagett = "";
            if (damageType == 0)
            {
                damagett = "物理";
            }
            else if (damageType == 1)
            {
                damagett = "法术";
            }
            else if (damageType == 2)
            {
                damagett = "无视护甲";
            }
            else if (damageType == 3)
            {
                damagett = "无视法抗";
            }
            else if (damageType == 4)
            {
                damagett = "流血";
            }
            else if (damageType == 5)
            {
                damagett = "反弹";
            }
            if (caster is ObjPlayer)
            {
                Logger.Info("id={0} 的角色受到了{1}点{2}伤害", _this.ObjId, damage, damagett);
            }
#endif
            //受到伤害事件
            BearDamage.DoEffect(_this.Scene, _this, caster, damage);
            //buff或技能阶段添加：HP改变事件
            //Hp_Change hpChange = new Hp_Change(buffResult.NewBuff, this, target_hero, old_hp_bili, new_hp_bili);
            //造成伤害事件
            CauseDamage.DoEffect(_this.Scene, _this, caster, damage);
            //死亡检测
            _this.OnDamage(caster, damage);
            CheckDie(_this, buff, caster, delayView, damage);
        }

        //造成治疗
        /// <summary>
        ///     造成治疗
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="hitType">命中类型</param>
        /// <param name="buff">来源Buff</param>
        /// <param name="caster">造成者</param>
        /// <param name="health">治疗量</param>
        /// <param name="healthType">治疗类型</param>
        /// <param name="IsToClient"></param>
        public void DoHealth(ObjCharacter _this,
                             eHitType hitType,
                             BuffData buff,
                             ObjCharacter caster,
                             ref int health,
                             int healthType,
                             bool IsToClient = false)
        {
            //释放方与承受方   增伤,减伤生效
            BuffEffect.CASTER_REF_HEALTH(caster, _this, ref health, healthType);
            BuffEffect.BEAR_REF_HEALTH(caster, _this, ref health, healthType);
            //治疗计算
            //int oldHp = Attr.GetDataValue(eAttributeType.HpNow);
            //int now_maxhp = Attr.GetDataValue(eAttributeType.HpMax);
            //int old_hp_bili = oldHp * 10000 / now_maxhp;
            //int new_hp = oldHp + health;
            //int new_hp_bili = new_hp * 10000 / now_maxhp;
            _this.Attr.SetDataValue(eAttributeType.HpNow, _this.Attr.GetDataValue(eAttributeType.HpNow) + health);
            HpLessThanCentainPercent.DoEffect(_this.Scene, _this);

#if DEBUG
            //输出
            var healthtt = "";
            if (healthType == 0)
            {
                healthtt = "道具";
            }
            else if (healthType == 1)
            {
                healthtt = "技能";
            }
            else if (healthType == 2)
            {
                healthtt = "吸血";
            }
            Logger.Info("id={0} 的角色受到了{1}点{2}治疗", _this.ObjId, health, healthtt);
#endif
            //buff或技能阶段添加：治疗事件   
            BearHealth.DoEffect(_this.Scene, _this, caster, health);
            //buff或技能阶段添加：HP改变事件
            //Hp_Change hpChange = new Hp_Change(buffResult.NewBuff, this, target_hero, old_hp_bili, new_hp_bili);
            //buff或技能阶段添加：造成治疗事件
            CauseHealth.DoEffect(_this.Scene, _this, caster, health);
            //发包
            if (IsToClient)
            {
                var nowHp = _this.GetAttribute(eAttributeType.HpNow);
                if (_this.Scene != null && _this.Scene.isNeedDamageModify)
                {
                    if (Scene.IsNeedChangeHp(_this) != null)
                    {
                        nowHp = (int)(nowHp / _this.Scene.BeDamageModify);
                    }
                }

                var replyMsg = new BuffResultMsg();
                replyMsg.buff.Add(new BuffResult
                {
                    SkillObjId = buff.mCasterId,
                    TargetObjId = _this.ObjId,
                    BuffTypeId = buff.GetBuffId(),
                    Type = BuffType.HT_HEALTH,
                    Damage = health,
                    Param = { 0, nowHp }
                });
                _this.BroadcastBuffList(replyMsg);
            }
        }

        //造成回蓝
        /// <summary>
        ///     造成回蓝
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="hitType">命中类型</param>
        /// <param name="buff">来源Buff</param>
        /// <param name="caster">造成者</param>
        /// <param name="mana">治疗量</param>
        /// <param name="healthType">治疗类型</param>
        /// <param name="IsToClient"></param>
        public void DoMana(ObjCharacter _this,
                           eHitType hitType,
                           BuffData buff,
                           ObjCharacter caster,
                           ref int mana,
                           int healthType,
                           bool IsToClient = false)
        {
            //治疗计算
            //int oldMp = Attr.GetDataValue(eAttributeType.MpNow);
            //int now_maxmp = Attr.GetDataValue(eAttributeType.MpMax);
            //int old_mp_bili = oldMp * 10000 / now_maxmp;
            //int new_mp = oldMp + mana;
            //int new_mp_bili = new_mp * 10000 / now_maxmp;
            _this.Attr.SetDataValue(eAttributeType.MpNow, _this.Attr.GetDataValue(eAttributeType.MpNow) + mana);
            //输出

#if DEBUG
            var healthtt = "";
            if (BitFlag.GetAnd(healthType, 1) > 0)
            {
                healthtt = "道具";
            }
            else if (BitFlag.GetAnd(healthType, 2) > 0)
            {
                healthtt = "技能";
            }
            else if (BitFlag.GetAnd(healthType, 4) > 0)
            {
                healthtt = "吸蓝";
            }
            Logger.Info("id={0} 的角色受到了{1}点{2}回蓝", _this.ObjId, mana, healthtt);
#endif
            //发包
            if (IsToClient)
            {
                var replyMsg = new BuffResultMsg();
                replyMsg.buff.Add(new BuffResult
                {
                    SkillObjId = buff.mCasterId,
                    TargetObjId = _this.ObjId,
                    BuffTypeId = buff.GetBuffId(),
                    Type = BuffType.HT_MANA,
                    Damage = mana
                });
                _this.BroadcastBuffList(replyMsg);
            }
        }

        //死亡检测
        /// <summary>
        ///     死亡检测
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="buff">源于Buff</param>
        /// <param name="caster">造成者</param>
        /// <param name="delayView"></param>
        /// <param name="damage">当次伤害</param>
        /// <returns>真的死亡了没</returns>
        public bool CheckDie(ObjCharacter _this, BuffData buff, ObjCharacter caster, int delayView, int damage)
        {
            if (_this.Scene == null)
                return false;
            if (_this.Attr.GetDataValue(eAttributeType.HpNow) <= 0)
            {
                WillDie.DoEffect(_this.Scene, _this, buff, caster, damage);
                if (_this.Attr.GetDataValue(eAttributeType.HpNow) <= 0 && (_this.Scene.IsOnlineDamage == false || _this.GetObjType() != ObjType.NPC))
                {
                    Die(_this, caster.ObjId, delayView, damage);
                    //导致人死亡的事件
                    CauseDie.DoEffect(_this.Scene, _this, buff, caster, damage);
                    //真正死亡的的事件
                    RealDie.DoEffect(_this.Scene, _this, buff, caster, damage);
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region 随从相关

        //创建随从
        public ObjRetinue CreateRetinue(ObjCharacter _this,
                                        int dataId,
                                        int level,
                                        Vector2 pos,
                                        Vector2 dir,
                                        int camp,
                                        bool isNeedAdd = true)
        {
            if (_this.Scene == null)
            {
                return null;
            }
            if (_this.Scene.TableSceneData.CanSummonMonster == 0)
                return null;
            var npc = _this.Scene.CreateRetinue(dataId, _this, pos, dir, camp, level);
            if (npc == null)
            {
                return null;
            }
            if (isNeedAdd)
            {
                _this.mRetinues.Add(npc);
            }
            return npc;
        }



        //是否有某个随从
        public bool IsHaveRetinue(ObjCharacter _this, ObjRetinue obj)
        {
            if (_this.mRetinues.Contains(obj))
            {
                return true;
            }
            return false;
        }

        public List<ObjRetinue> GetRetinueList(ObjCharacter _this)
        {
            return _this.mRetinues;
        }
        public void ClearRetinue(ObjCharacter _this)
        {

            while (_this.mRetinues.Count > 0)
            {
                RemoveRetinue(_this, _this.mRetinues[0]);
            }
        }
        //删除随从
        public void RemoveRetinue(ObjCharacter _this, ObjRetinue retinue)
        {
            if (retinue == null)
            {
                return;
            }
            if (IsHaveRetinue(_this, retinue))
            {
                if (!retinue.IsSendDie)
                {
                    if (retinue.Scene != null)
                    {
                        retinue.Scene.LeaveScene(retinue);
                    }
                }
                _this.mRetinues.Remove(retinue);
            }
        }

        //使随从攻击
        public void RetinueAttack(ObjCharacter _this)
        {
            foreach (var retinue in _this.mRetinues)
            {
                if (retinue.Active && !retinue.IsDead())
                {
                    retinue.EnterState(BehaviorState.Combat);
                }
            }
        }

        #endregion

        #region 子弹相关

        //创建子弹
        public void PushBullet(ObjCharacter _this, Bullet b)
        {
            _this.mBullets.TryAdd(b, false);
        }

        //删除子弹
        public void RemoveBullet(ObjCharacter _this, Bullet b)
        {
            _this.mBullets.Remove(b);
        }

        //清空子弹
        public void CleanBullet(ObjCharacter _this)
        {
            foreach (var bullet in _this.mBullets)
            {
                bullet.Key.OnDestroy();
            }
            _this.mBullets.Clear();
        }

        #endregion
    }

    public partial class ObjCharacter : ObjBase
    {
        public Dictionary<Bullet, bool> mBullets = new Dictionary<Bullet, bool>(); //子弹
        public List<ObjRetinue> mRetinues = new List<ObjRetinue>(); //随从
        //最后一个攻击我的敌人id
        public ulong LastEnemyId
        {
            get { return mImpl.GetLastEnemyId(this); }
        }

        public DateTime mCanMove { get; set; }
        public DateTime mCanSkill { get; set; }

        #region 初始化

        public virtual bool InitEquip(int level)
        {
            return mImpl.InitEquip(this, level);
        }

        public virtual bool InitAttr(int level)
        {
            return mImpl.InitAttr(this, level);
        }

        public virtual bool InitSkill(int level)
        {
            return mImpl.InitSkill(this, level);
        }

        public virtual bool InitBuff(int level)
        {
            return mImpl.InitBuff(this, level);
        }

        #endregion

        #region 技能限制

        public bool CanSkill()
        {
            return mImpl.CanSkill(this);
        }

        public bool CanMove()
        {
            return mImpl.CanMove(this);
        }

        public void SetSkill(int SkillTime)
        {
            mImpl.SetSkill(this, SkillTime);
        }

        public void SetMove(int MoveTime)
        {
            mImpl.SetMove(this, MoveTime);
        }

        #endregion

        #region 数据同步

        //技能发生了变化 nType=变化规则{0删除，1新增，2修改} nId=技能ID   nLevel=技能等级
        public void SkillChange(int nType, int nId, int nLevel)
        {
            mImpl.SkillChange(this, nType, nId, nLevel);
        }

        //重新装备技能
        public void EquipSkill(List<int> dels, List<int> adds, List<int> lvls)
        {
            mImpl.EquipSkill(this, dels, adds, lvls);
        }

        //天赋发生了变化 nType=变化规则{0删除，1新增，2修改,3清空} nId=天赋ID   nLevel=天赋层数
        public void TalentChange(int nType, int nId, int nLayer)
        {
            mImpl.TalentChange(this, nType, nId, nLayer);
        }

        #endregion

        #region 技能相关

        //检查技能是否能使用
        public ErrorCodes CheckUseSkill(ref int skillId, ObjCharacter target = null)
        {
            return mImpl.CheckUseSkill(this, ref skillId, target);
        }

        public ErrorCodes RequestUseSkill(int skillId, List<int> skillIds, ObjCharacter target = null)
        {
            return mImpl.RequestUseSkill(this, skillId, skillIds, target);
        }

        //使用技能
        public virtual ErrorCodes UseSkill(ref int skillId, ObjCharacter target = null)
        {
            return mImpl.UseSkill(this, ref skillId, target);
        }

        //不判断技能
        public ErrorCodes MustSkill(ref int skillId, ObjCharacter target = null)
        {
            return mImpl.MustSkill(this, ref skillId, target);
        }

        //使用技能成功
        public virtual void OnUseSkill(int skillId, ObjCharacter target)
        {
            mImpl.OnUseSkill(this, skillId, target);
        }

        #endregion

        #region Buff相关

        public BuffData AddBuff(BuffRecord tbBuff, int bufflevel, ObjCharacter casterHero)
        {
            return mImpl.AddBuff(this, tbBuff, bufflevel, casterHero);
        }

        public ErrorCodes CheckAddBuff(int buffid, int bufflevel, ObjCharacter casterHero)
        {
            return mImpl.CheckAddBuff(this, buffid, bufflevel, casterHero);
        }

        /// 获得Buff时
        /// <summary>
        ///     增加Buff
        /// </summary>
        /// <param name="buffid">BuffId</param>
        /// <param name="bufflevel"></param>
        /// <param name="casterHero">施放者</param>
        /// <param name="DelayView"></param>
        /// <param name="hitType">命中类型</param>
        /// <param name="fBili">Buff的修正比例</param>
        /// <param name="buffLastTime">Buff持续时间 部分buff的持续时间不读buff表</param>
        /// <returns></returns>
        public BuffData AddBuff(int buffid,
                                int bufflevel,
                                ObjCharacter casterHero,
                                int DelayView = 0,
                                eHitType hitType = eHitType.Hit,
                                float fBili = 1.0f,
                                double buffLastTime = 0
                                )
        {
            return mImpl.AddBuff(this, buffid, bufflevel, casterHero, DelayView, hitType, fBili, buffLastTime);
        }

        /// 删除Buff
        /// <summary>
        ///     删除Buff (知道Buff实例)
        /// </summary>
        /// <param name="deletebuff">Buff实例</param>
        /// <param name="type">删除的原因类型</param>
        public void DeleteBuff(BuffData deletebuff, eCleanBuffType type)
        {
            mImpl.DeleteBuff(this, deletebuff, type);
        }

        /// 删除Buff
        /// <summary>
        ///     删除Buff (知道BuffId)
        /// </summary>
        /// <param name="nBuffId">BuffId</param>
        /// <param name="type">删除的原因类型</param>
        public void DeleteBuff(int nBuffId, eCleanBuffType type)
        {
            mImpl.DeleteBuff(this, nBuffId, type);
        }

        #endregion

        #region 效果相关

        //造成绝对伤害
        /// <summary>
        ///     造成伤害
        /// </summary>
        /// <param name="hitType">命中类型</param>
        /// <param name="buff">来源Buff</param>
        /// <param name="caster">造成者</param>
        /// <param name="delayView"></param>
        /// <param name="damage">伤害量</param>
        /// <param name="damageType">伤害类型</param>
        public void DoRealDamage(eHitType hitType,
                                 BuffData buff,
                                 ObjCharacter caster,
                                 int delayView,
                                 ref int damage,
                                 int damageType)
        {
            mImpl.DoRealDamage(this, hitType, buff, caster, delayView, ref damage, damageType);
        }

        //造成伤害
        /// <summary>
        ///     造成伤害
        /// </summary>
        /// <param name="hitType">命中类型</param>
        /// <param name="buff">来源Buff</param>
        /// <param name="caster">造成者</param>
        /// <param name="delayView"></param>
        /// <param name="damage">伤害量</param>
        /// <param name="damageType">伤害类型</param>
        /// <param name="absorbDamage">吸收伤害</param>
        public void DoDamage(eHitType hitType,
                             BuffData buff,
                             ObjCharacter caster,
                             int delayView,
                             ref int damage,
                             int damageType,
                             ref int absorbDamage)
        {
            mImpl.DoDamage(this, hitType, buff, caster, delayView, ref damage, damageType, ref absorbDamage);
        }

        //造成治疗
        /// <summary>
        ///     造成治疗
        /// </summary>
        /// <param name="hitType">命中类型</param>
        /// <param name="buff">来源Buff</param>
        /// <param name="caster">造成者</param>
        /// <param name="health">治疗量</param>
        /// <param name="healthType">治疗类型</param>
        /// <param name="IsToClient"></param>
        public void DoHealth(eHitType hitType,
                             BuffData buff,
                             ObjCharacter caster,
                             ref int health,
                             int healthType,
                             bool IsToClient = false)
        {
            mImpl.DoHealth(this, hitType, buff, caster, ref health, healthType, IsToClient);
        }

        //造成回蓝
        /// <summary>
        ///     造成回蓝
        /// </summary>
        /// <param name="hitType">命中类型</param>
        /// <param name="buff">来源Buff</param>
        /// <param name="caster">造成者</param>
        /// <param name="mana">治疗量</param>
        /// <param name="healthType">治疗类型</param>
        /// <param name="IsToClient"></param>
        public void DoMana(eHitType hitType,
                           BuffData buff,
                           ObjCharacter caster,
                           ref int mana,
                           int healthType,
                           bool IsToClient = false)
        {
            mImpl.DoMana(this, hitType, buff, caster, ref mana, healthType, IsToClient);
        }

        //死亡检测
        /// <summary>
        ///     死亡检测
        /// </summary>
        /// <param name="buff">源于Buff</param>
        /// <param name="caster">造成者</param>
        /// <param name="delayView"></param>
        /// <param name="damage">当次伤害</param>
        /// <returns>真的死亡了没</returns>
        public bool CheckDie(BuffData buff, ObjCharacter caster, int delayView, int damage)
        {
            return mImpl.CheckDie(this, buff, caster, delayView, damage);
        }

        #endregion

        #region 随从相关

        //创建随从
        public ObjRetinue CreateRetinue(int dataId, int level, Vector2 pos, Vector2 dir, int camp, bool isNeedAdd = true)
        {
            return mImpl.CreateRetinue(this, dataId, level, pos, dir, camp, isNeedAdd);
        }

        //是否有某个随从
        public bool IsHaveRetinue(ObjRetinue obj)
        {
            return mImpl.IsHaveRetinue(this, obj);
        }

        public List<ObjRetinue> GetRetinueList()
        {
            return mImpl.GetRetinueList(this);
        }

        //删除随从
        public void RemoveRetinue(ObjRetinue retinue)
        {
            mImpl.RemoveRetinue(this, retinue);
        }

        //使随从攻击
        public void RetinueAttack()
        {
            mImpl.RetinueAttack(this);
        }

        public void ClearRetinue()
        {
            mImpl.ClearRetinue(this);
        }

        #endregion

        #region 子弹相关

        //创建子弹
        public void PushBullet(Bullet b)
        {
            mImpl.PushBullet(this, b);
        }

        //删除子弹
        public void RemoveBullet(Bullet b)
        {
            mImpl.RemoveBullet(this, b);
        }

        //清空子弹
        public void CleanBullet()
        {
            mImpl.CleanBullet(this);
        }

        #endregion
    }
}