#region using

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DataContract;
using DataTable;
using Scorpion;
using Mono.GameMath;
using NLog;
using Scene.Character;
using SceneServerService;
using Shared;

#endregion

namespace Scene
{

    #region  技能数据

    public interface ISkillData
    {
        ErrorCodes CheckSkill(SkillData _this, ObjCharacter caster);
        bool IsPassiveSkill(SkillData _this);
        void CreateCdTrigger(SkillData _this);
        ErrorCodes DoEffect(SkillData _this, ObjCharacter caster, ObjCharacter target, int mainIndex = -1);
        void DoSuccess(SkillData _this);
        void DoSuccessNoCd(SkillData _this);
        void DoCdAndCount(SkillData _this);
        void PosGuideBefore(SkillData _this, ObjCharacter caster, ObjCharacter target);
        void StartUpdata(SkillData _this, ObjCharacter caster, ObjCharacter target);
        void StopSkill(SkillData _this);
        void Updata(SkillData _this);
    }

    public class SkillDataDefaultImpl : ISkillData
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region  生效相关

		static public bool FilterObjType(ObjCharacter obj, int val)
		{
			if (!obj.CanBeAttacked())
			{
				return false;
			}

			if (obj.GetObjType() == ObjType.PLAYER)
			{
				return 0 != (val & eSkillTargetObjType.Player);
			}
			else if (obj.GetObjType() == ObjType.RETINUE)
			{
				return 0 != (val & eSkillTargetObjType.Retinue);	
			}
			else if (obj.GetObjType() == ObjType.NPC)
			{
				var npc = obj as ObjNPC;
				if (2 == npc.TableCharacter.Type)
				{
					return 0 != (val & eSkillTargetObjType.Monster);
				}
				else if (3 == npc.TableCharacter.Type)
				{
					return 0 != (val & eSkillTargetObjType.Boss);
				}
				return true;
			}
			return true;
		}

        

        //执行技能效果
        public ErrorCodes DoEffect(SkillData _this, ObjCharacter caster, ObjCharacter target, int mainIndex = -1)
        {
            //PlayerLog.WriteLog(mSelf.ObjId, "----------DoEffect----------SkillId={0}", mId);
            //检查caster 和 target的合法性
            //目标选择
            var targetType = (eTargetType) _this.mTable.TargetType;
            if (caster is ObjPlayer)
            {
                //Logger.Info("DoEffect skillid={0} mainIndex={1}", mId, mainIndex);
                //PlayerLog.WriteLog(caster.ObjId, "DoEffect skillid={0} mainIndex={1}", mId, mainIndex);
                if (target != null)
                {
                    if (targetType == eTargetType.Rect)
                    {
                        targetType = eTargetType.TargetRect;
                    }
                    else if (targetType == eTargetType.Fan)
                    {
                        targetType = eTargetType.TargetFan;
                    }
                }
            }
            List<ObjCharacter> targetlist;
            if (_this.mTable.CastType == (int)SkillType.FixedPoint
                || _this.mTable.CastType == (int)SkillType.FixedPointSelf
                || (_this.mTable.TargetType == 5 && _this.mTable.TargetParam[2] == 1))
            {
                targetlist = SkillManager.GetPositionByCondition(caster, _this.pos, targetType, _this.mTable.TargetParam,
                    (eCampType) _this.mTable.CampType, _this.mTable.TargetCount, target);
            }
            else
            {
                targetlist = SkillManager.GetTargetByCondition(caster, target, targetType, _this.mTable.TargetParam,
                    (eCampType) _this.mTable.CampType, _this.mTable.TargetCount);
            }

			//类型筛选
	        {
				for (int i = 0; i < targetlist.Count; )
		        {
					if (!FilterObjType(targetlist[i], _this.mTable.TargetObjType))
			        {
						targetlist.RemoveAt(i);
				        continue;
			        }
			        i++;
		        }
	        }
            if (caster is ObjPlayer)
            {
                #region 玩家 次数限制筛选
                var player = caster as ObjPlayer;
                for (int i = 0; i < targetlist.Count; )
                {
                    if (targetlist[i].GetObjType() == ObjType.NPC)
                    {
                        var npc = targetlist[i] as ObjNPC;
                        if (npc.TableNpc.LimitFlag > 0)
                        {
                            int times = 0;
                            if (player.dicFlagTemp.TryGetValue(npc.TableNpc.LimitFlag, out times) == true)
                            {
                                 if (times >= npc.TableNpc.LimitTimes)
                                {
                                    targetlist.RemoveAt(i);
                                    if (_this.LastAttackTime == null)
                                    {
                                        _this.LastAttackTime = DateTime.Now;
                                    }
                                    if (_this.LastAttackTime != null)
                                    {
                                        var v = (DateTime.Now - _this.LastAttackTime).TotalSeconds;
                                        if (v >= 3f)
                                        {
                                            player.Proxy.NotifyBattleReminder(27, Utils.WrapDictionaryId(280002), 1);
                                            _this.LastAttackTime = DateTime.Now;
                                        }
                                    }                                    
                                    continue;
                                }
                            }                            
                        }
                    }
                    i++;
                }
                #endregion
            }
            if (caster is ObjRetinue)
            {
                #region 魔物 次数限制筛选
                var player = (caster as ObjRetinue).Owner as ObjPlayer;
                for (int i = 0; i < targetlist.Count; )
                {
                    if (targetlist[i].GetObjType() == ObjType.NPC)
                    {
                        var npc = targetlist[i] as ObjNPC;
                        if (npc.TableNpc.LimitFlag > 0)
                        {
                            int times = 0;
                            if (player.dicFlagTemp.TryGetValue(npc.TableNpc.LimitFlag, out times) == true)
                            {
                                if (times >= npc.TableNpc.LimitTimes)
                                {
                                    targetlist.RemoveAt(i);
                                    continue;
                                }
                            }
                        }
                    }
                    i++;
                }
                #endregion
            }
            //目标筛选
            if (targetlist.Count <= 0)
            {
                caster.Skill.LastSkillMainTarget.SetTarget(null);
                return ErrorCodes.Error_SkillNoTarget;
            }
            //targetlist = ScreeningTarget(targetlist, caster, (eCampType) thisskill.mTable.CampType,thisskill.mTable.TargetCount);
            if (targetlist[0] != null)
            {
                if (SkillManager.CheckCamp(caster, targetlist[0], eCampType.Enemy))
                {
                    caster.Skill.EnemyTarget = targetlist[0];
                }
                if (targetlist[0] != caster)
                {
                    caster.Skill.LastSkillMainTarget.SetTarget(targetlist[0]);
                }
            }
            //增加效果
            if (_this.mTable.BulletId < 0)
            {
                var nIndex = 0;
                var fModify = 1.0f;
                foreach (var character in targetlist)
                {
                    if (character is ObjNPC && !(character is AutoPlayer))
                    {
                        var npc = character as ObjNPC;
                        if (npc.TableNpc.NpcType == 3 && caster == character)
                        {
                            continue;
                        }
                    }
                    var hitResult = caster.Attr.GetHitResult(character, (eSkillHitType) _this.mTable.HitType);
                    var SpecialDelayTime = 0;
                    //潜规则转圈甩剑的伤害延迟
                    if ((caster.Skill.GetModifySkillValue(_this.mId, (int) eModifySkillType.BroadcastSkillId, _this.mId) ==
                         114))
                    {
                        var mDir = caster.GetDirection();
                        var dif = character.GetPosition() - caster.GetPosition();
                        var TotleTime = 1000;
                        if (_this.mId == 108)
                        {
                            TotleTime = 1500;
                        }
                        var a = Math.Atan2(mDir.Y, mDir.X) - 45*Math.PI/180.0f;
                        var b = Math.Atan2(dif.Y, dif.X);
                        var c = b - a;
                        var pi2 = (float) Math.PI*2.0f;
                        if (c < 0)
                        {
                            c += pi2;
                        }
                        while (c > pi2)
                        {
                            c -= pi2;
                        }
                        SpecialDelayTime = (int) (c/pi2*TotleTime);
                        //Logger.Fatal("SpecialDelayTime ={0} characterId={1}", SpecialDelayTime, character.ObjId);
                    }
                    else if (_this.mId == 104)
                    {
                        var dif = character.GetPosition() - caster.GetPosition();
                        var dis = dif.Length();
                        SpecialDelayTime = (int) (dis/2)*150;
                    }

                    if (hitResult != eHitType.Miss)
                    {
                        if (mainIndex == -1)
                        {
                            if (nIndex == 0)
                            {
//主目标
                                character.AddBuff(_this.mTable.MainTarget[0], _this.mLevel, caster,
                                    _this.mTable.DelayView + SpecialDelayTime, hitResult, fModify);
                                character.AddBuff(_this.mTable.MainTarget[1], _this.mLevel, caster,
                                    _this.mTable.DelayView + SpecialDelayTime, hitResult, fModify);
                                character.AddBuff(_this.mTable.MainTarget[2], _this.mLevel, caster,
                                    _this.mTable.DelayView + SpecialDelayTime, hitResult, fModify);
                                character.AddBuff(_this.mTable.MainTarget[3], _this.mLevel, caster,
                                    _this.mTable.DelayView + SpecialDelayTime, hitResult, fModify);
                            }
                            else
                            {
//副目标
                                character.AddBuff(_this.mTable.MainTarget[0], _this.mLevel, caster,
                                    _this.mTable.DelayView + SpecialDelayTime, hitResult, fModify);
                                character.AddBuff(_this.mTable.MainTarget[1], _this.mLevel, caster,
                                    _this.mTable.DelayView + SpecialDelayTime, hitResult, fModify);
                                character.AddBuff(_this.mTable.OtherTarget[0], _this.mLevel, caster,
                                    _this.mTable.DelayView + SpecialDelayTime, hitResult, fModify);
                                character.AddBuff(_this.mTable.OtherTarget[1], _this.mLevel, caster,
                                    _this.mTable.DelayView + SpecialDelayTime, hitResult, fModify);
                            }
                        }
                        else
                        {
                            if (mainIndex >= 0 && mainIndex <= 3)
                            {
                                if (nIndex == 0 && mainIndex >= 0 && mainIndex <= 1)
                                {
                                    character.AddBuff(_this.mTable.OtherTarget[mainIndex], _this.mLevel, caster,
                                        _this.mTable.DelayView + SpecialDelayTime, hitResult, fModify);
                                }
                                character.AddBuff(_this.mTable.MainTarget[mainIndex], _this.mLevel, caster,
                                    _this.mTable.DelayView + SpecialDelayTime, hitResult, fModify);
                            }
                            else
                            {
                                Logger.Error("DoEffect AddBuff[{0}] is overflow", mainIndex);
                            }
                        }
                        if (targetType == eTargetType.Ejection)
                        {
                            fModify = fModify*_this.mTable.TargetParam[5]/10000;
                        }
                    }
                    else
                    {
                        var replyMsg = new BuffResultMsg();
                        replyMsg.buff.Add(new BuffResult
                        {
                            SkillObjId = caster.ObjId,
                            TargetObjId = character.ObjId,
                            Type = BuffType.HT_MISS,
                            ViewTime = Extension.AddTimeDiffToNet(_this.mTable.DelayView)
                        });
                        caster.BroadcastBuffList(replyMsg);
                        if (targetType == eTargetType.Ejection)
                        {
                            break;
                        }
                    }
                    nIndex++;
                }
            }
            else
            {
//处理子弹
                var fModify = 1.0f;
                foreach (var character in targetlist)
                {
                    if (character.IsDead())
                    {
                        continue;
                    }
                    Bullet bullet = Bullet.CreateBullet(caster, _this.mTable.BulletId, _this.mLevel,
                        (eSkillHitType) _this.mTable.HitType, character);
                    if (bullet == null)
                    {
                        continue;
                    }
                    bullet.Modify = fModify;
                    if (targetType == eTargetType.Ejection)
                    {
                        fModify = fModify*_this.mTable.TargetParam[5]/10000;
                    }
                }
            }
            return ErrorCodes.OK;
        }

        #endregion

        #region  结束相关

        //打断技能
        public void StopSkill(SkillData _this)
        {
            if (_this.mYindaoTrigger != null)
            {
                SceneServerControl.Timer.DeleteTrigger(_this.mYindaoTrigger);
                _this.mYindaoTrigger = null;
            }
        }

        #endregion

        #region  冷却相关

        //冷却的回调
        private void Cd(SkillData _this)
        {
            //PlayerLog.WriteLog(mSelf.ObjId, "----------Cd----------SkillId={0}", mId);
            if (_this.mDoCount < _this.mDoMax)
            {
                _this.mDoCount++;
            }
            if (_this.mDoCount >= _this.mDoMax)
            {
                DeleteCdTrigger(_this);
                if (!_this.mIsActive)
                {
                    _this.mSelf.Skill.mData.Remove(_this.mId);
                }
            }
        }

        //创建冷却触发器
        public void CreateCdTrigger(SkillData _this)
        {
            if (_this.mCd <= 0)
            {
                return;
            }
            //PlayerLog.WriteLog(mSelf.ObjId, "----------CreateCdTrigger----------SkillId={0}", mId);
            if (_this.mCDTrigger == null)
            {
                _this.mCDTrigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddMilliseconds(_this.mCd),
                    () => { Cd(_this); }, _this.mCd);
                //SceneServerControl.Timer.DeleteTrigger(mCDTrigger);
            }
        }

        //删除冷却触发器
        private void DeleteCdTrigger(SkillData _this)
        {
            //PlayerLog.WriteLog(mSelf.ObjId, "----------DeleteCdTrigger----------SkillId={0}", mId);
            if (_this.mCDTrigger != null)
            {
                SceneServerControl.Timer.DeleteTrigger(_this.mCDTrigger);
                _this.mCDTrigger = null;
            }
        }

        #endregion

        #region  引导相关
        // 设置定点引导的位置
        private void SetFixedPoint(SkillData _this, Vector2 pos)
        {
            _this.pos = pos;
        }

        //定点引导的特殊引导前
        public void PosGuideBefore(SkillData _this, ObjCharacter caster, ObjCharacter target)
        {
            if (target == null)
            {
                return;
            }
            if (_this.mTable.MainTarget[0] == -1)
            {
                return;
            }
            var temp = new BuffResult
            {
                Type = BuffType.HT_EFFECT,
                BuffTypeId = _this.mTable.MainTarget[0],
                ViewTime = Extension.AddTimeDiffToNet(0)
            };
            temp.Param.Add(_this.mTable.Id);
            if (_this.mTable.CastType == (int)SkillType.FixedPointSelf)
            {
                SetFixedPoint(_this, caster.GetPosition());
                temp.TargetObjId = caster.ObjId;
            }
            else if (_this.mTable.CastType == (int)SkillType.FixedPoint)
            {
                SetFixedPoint(_this, target.GetPosition());
                temp.TargetObjId = target.ObjId;
            }
            else
            {
                return;
            }
            temp.Param.Add((int) (_this.pos.X*100));
            temp.Param.Add((int) (_this.pos.Y*100));
            temp.Param.Add((int) (caster.GetDirection().X*1000));
            temp.Param.Add((int) (caster.GetDirection().Y*1000));
            temp.Param.Add(_this.mTable.CastParam[1]);

            var replyMsg = new BuffResultMsg();
            replyMsg.buff.Add(temp);
            target.BroadcastBuffList(replyMsg);
        }

        //定点引导的特殊引导前
        public void SelfPosGuideBefore(SkillData _this, ObjCharacter caster, ObjCharacter target)
        {
            if (_this.mTable.CastType != (int) SkillType.FixedPointSelf || caster == null)
            {
                return;
            }

            if (_this.mTable.MainTarget[0] == -1)
            {
                return;
            }

            SetFixedPoint(_this, caster.GetPosition());

            if (target == null)
            {
                return;
            }

            var temp = new BuffResult
            {
                Type = BuffType.HT_EFFECT,
                BuffTypeId = _this.mTable.MainTarget[0],
                ViewTime = Extension.AddTimeDiffToNet(0)
            };
            temp.Param.Add(_this.mTable.Id);
            temp.TargetObjId = caster.ObjId;
            temp.Param.Add((int)(_this.pos.X * 100));
            temp.Param.Add((int)(_this.pos.Y * 100));
            temp.Param.Add((int)(caster.GetDirection().X * 1000));
            temp.Param.Add((int)(caster.GetDirection().Y * 1000));
            temp.Param.Add(_this.mTable.CastParam[1]);

            var replyMsg = new BuffResultMsg();
            replyMsg.buff.Add(temp);
            target.BroadcastBuffList(replyMsg);
        }

        //创建引导
        public void StartUpdata(SkillData _this, ObjCharacter caster, ObjCharacter target)
        {
            //PlayerLog.WriteLog(mSelf.ObjId, "----------StartUpdata----------SkillId={0}", mId);
            if (_this.mSelf != caster)
            {
                Logger.Warn("Skill Self != Caster!!!");
            }
            _this.mTarget = target;
            if (_this.mTable.CastType == (int)SkillType.FixedPoint)
            {
                PosGuideBefore(_this, caster, target);
                _this.mYindaoCount = 1;
                _this.mYindaoTrigger =
                    SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddMilliseconds(_this.mTable.CastParam[1]),
                        () => { Updata(_this); }, _this.mTable.CastParam[1]);
            }
            else if (_this.mTable.CastType == (int) SkillType.FixedPointSelf)
            {
                SelfPosGuideBefore(_this, caster, target);
                _this.mYindaoCount = 1;
                _this.mYindaoTrigger =
                    SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddMilliseconds(_this.mTable.CastParam[1]),
                        () => { Updata(_this); }, _this.mTable.CastParam[1]);
            }
            else if (_this.mTable.CastType == 4)
            {
                if (_this.mTable.TargetType == 5 && _this.mTable.TargetParam[2] == 1 && target != null)
                {
                    _this.pos = target.GetPosition();
                }
                DoEffect(_this, _this.mSelf, _this.mTarget, 0);
                _this.mYindaoCount = 1;
                _this.mYindaoTrigger =
                    SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddMilliseconds(_this.mTable.CastParam[1]),
                        () => { Updata(_this); });
            }
            else
            {
                DoEffect(_this, _this.mSelf, _this.mTarget);
                _this.mYindaoCount = 1;
                _this.mYindaoTrigger =
                    SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddMilliseconds(_this.mTable.CastParam[1]),
                        () => { Updata(_this); }, _this.mTable.CastParam[1]);
            }
        }

        //引导心跳
        public void Updata(SkillData _this)
        {
            //检查caster 和 target的合法性
            //获得技能数据
            if (_this.mTable.CastType == 5 || _this.mTable.CastType == 7)
            {
                try
                {
                    DoEffect(_this, _this.mSelf, _this.mTarget, 1);
                }
                catch (Exception)
                {
                    Logger.Warn("Updata caster={0} skillId={1}", _this.mSelf.ObjId, _this.mTable.Id);
                }
                finally
                {
                    if (_this.mYindaoCount == _this.mTable.CastParam[0])
                    {
                        if (_this.mYindaoTrigger != null)
                        {
                            SceneServerControl.Timer.DeleteTrigger(_this.mYindaoTrigger);
                            _this.mYindaoTrigger = null;
                        }
                        _this.mYindaoCount = 0;
                        _this.mSelf.Skill.SkillOver(_this.mId, _this.mTarget);
                    }
                    else
                    {
                        _this.mYindaoCount++;
                        PosGuideBefore(_this, _this.mSelf, _this.mTarget);
                    }
                }
            }
            else if (_this.mTable.CastType == 4)
            {
                try
                {
                    DoEffect(_this, _this.mSelf, _this.mTarget, _this.mYindaoCount);
                }
                catch (Exception)
                {
                    Logger.Warn("Updata caster={0} skillId={1}", _this.mSelf.ObjId, _this.mTable.Id);
                }
                finally
                {
                    if (_this.mYindaoCount == _this.mTable.CastParam[0])
                    {
                        if (_this.mYindaoTrigger != null)
                        {
                            SceneServerControl.Timer.DeleteTrigger(_this.mYindaoTrigger);
                            _this.mYindaoTrigger = null;
                        }
                        _this.mYindaoCount = 0;
                        _this.mSelf.Skill.SkillOver(_this.mId, _this.mTarget);
                    }
                    else
                    {
                        _this.mYindaoCount++;
                        if (_this.mYindaoCount < _this.mTable.CastParam.Length)
                        {
                            _this.mYindaoTrigger =
                                SceneServerControl.Timer.CreateTrigger(
                                    DateTime.Now.AddMilliseconds(_this.mTable.CastParam[_this.mYindaoCount]),
                                    () => { Updata(_this); });
                        }
                    }
                }
            }
            else
            {
                try
                {
                    DoEffect(_this, _this.mSelf, _this.mTarget);
                }
                catch (Exception)
                {
                    Logger.Warn("Updata caster={0} skillId={1}", _this.mSelf.ObjId, _this.mTable.Id);
                }
                finally
                {
                    _this.mYindaoCount++;
                    var nCountTotle = _this.mTable.CastParam[0]/_this.mTable.CastParam[1];
                    if (_this.mYindaoCount >= nCountTotle)
                    {
                        if (_this.mYindaoTrigger != null)
                        {
                            SceneServerControl.Timer.DeleteTrigger(_this.mYindaoTrigger);
                            _this.mYindaoTrigger = null;
                        }
                        _this.mYindaoCount = 0;
                        _this.mSelf.Skill.SkillOver(_this.mId, _this.mTarget);
                    }
                }
            }
        }

        #endregion

        #region  释放相关

        public bool IsPassiveSkill(SkillData _this)
        {
            if (_this.mTable == null)
            {
                return false;
            }

            if (_this.mTable.Type == (int)eDoSkillType.ForceSkill)
            {
                return true;
            }

            return false;
        }

        //检查技能是否可以释放
        public ErrorCodes CheckSkill(SkillData _this, ObjCharacter caster)
        {
            if (_this.mCd > 0)
            {
                if (_this.mDoCount <= 0)
                {
                    return ErrorCodes.Error_SkillNoCD;
                }
            }
            if (caster.BuffList.GetEffectParam_And(eEffectType.SpecialState, 1, _this.mTable.ControlType))
            {
                return ErrorCodes.Error_SkillNotUse;
            }
           
            if (!_this.ItemSkill)
            {
                if (!IsPassiveSkill(_this))
                {
                    if (!caster.CanSkill())
                    {
                        return ErrorCodes.Error_SkillNoCD;
                    }
                }
            }
            if (caster.Attr.GetDataValue(eAttributeType.HpNow) < _this.mHp)
            {
                return ErrorCodes.Error_HpNoEnough;
            }
            if (caster.Attr.GetDataValue(eAttributeType.MpNow) < _this.mMp)
            {
                return ErrorCodes.Error_MpNoEnough;
            }

            return ErrorCodes.OK;
        }

        //释放成功
        public void DoSuccess(SkillData _this)
        {
            //PlayerLog.WriteLog(mSelf.ObjId, "----------DoSuccess----------SkillId={0}", mId);
            //修改扩展计数
            if (_this.mTable.ExdataChange != 0)
            {
                _this.mSelf.AddExdata(99, _this.mTable.ExdataChange);
            }
            //int ExdataValue;
            //if (mSelf.SkillExdata.TryGetValue(99, out ExdataValue))
            //{
            //    mSelf.SkillExdata[99] += mTable.ExdataChange;
            //}
            //else
            //{
            //    mSelf.SkillExdata[99] = mTable.ExdataChange;
            //}
            //判断使用次数
            if (_this.mDoMax <= 0)
            {
                return; //无限使用
            }
            _this.mDoCount--;
            //创建CD
            CreateCdTrigger(_this);
            if (_this.mSelf is ObjPlayer)
            {
                //增加CD自动播放下一个技能
                if (_this.mTable.CommonCd > 100)
                {
                    SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddMilliseconds(_this.mTable.CommonCd),
                        () => { _this.mSelf.Skill.DoNextSkill(); });
                }
            }
        }

        public void DoSuccessNoCd(SkillData _this)
        {
            if (_this.mTable.ExdataChange != 0)
            {
                _this.mSelf.AddExdata(99, _this.mTable.ExdataChange);
            }

            //判断使用次数
            if (_this.mDoMax <= 0)
            {
                return; //无限使用
            }

            if (_this.mSelf is ObjPlayer)
            {
                //增加CD自动播放下一个技能
                if (_this.mTable.CommonCd > 100)
                {
                    SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddMilliseconds(_this.mTable.CommonCd),
                        () => { _this.mSelf.Skill.DoNextSkill(); });
                }
            }
        }

        public void DoCdAndCount(SkillData _this)
        {
            //判断使用次数
            if (_this.mDoMax <= 0)
            {
                return; //无限使用
            }
            _this.mDoCount--;
            //创建CD
            CreateCdTrigger(_this);
        }

        #endregion
    }

    public class SkillData
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static ISkillData mImpl;
        public DateTime LastAttackTime;
        static SkillData()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (SkillData), typeof (SkillDataDefaultImpl),
                o => { mImpl = (ISkillData) o; });
        }

        #region  冷却相关

        //创建冷却触发器
        public void CreateCdTrigger()
        {
            mImpl.CreateCdTrigger(this);
        }

        #endregion

        #region  生效相关

        //执行技能效果
        public ErrorCodes DoEffect(ObjCharacter caster, ObjCharacter target, int mainIndex = -1)
        {
            return mImpl.DoEffect(this, caster, target, mainIndex);
        }

        #endregion

        #region  结束相关

        //打断技能
        public void StopSkill()
        {
            mImpl.StopSkill(this);
        }

        #endregion

        #region  数据结构

        public int mId; //技能ID
        public int mLevel; //技能等级
        public int mDoCount; //剩余的使用次数
        public bool mIsActive = true;
        public eAddskillType From;

        public int mDoMax //最大的使用次数
        {
            get { return mTable.Layer; }
        }

        public int mCd //CD的时间(毫秒)
        {
            get { return mTable.Cd; }
        }

        public int mHp //Hp消耗
        {
            get { return mTable.NeedHp; }
        }

        public int mMp //Mp消耗
        {
            get { return mTable.NeedMp; }
        }

        public int mAnger //怒气消耗
        {
            get { return mTable.NeedAnger; }
        }

        public Trigger mCDTrigger; //CD时间触发器
        public BuffData mBuff; //被动Buff
        public SkillRecord mTable; //该技能的表格
        public Trigger mYindaoTrigger; //引导的时间触发器
        public int mYindaoCount; //当前的引导次数
        public ObjCharacter mSelf; //技能的主人（释放者)
        public ObjCharacter mTarget; //技能的目标
        public bool ItemSkill; //是不是属于物品技能
        public Vector2 pos;

        #endregion

        #region  引导相关

        //定点引导的特殊引导前
        public void PosGuideBefore(ObjCharacter caster, ObjCharacter target)
        {
            mImpl.PosGuideBefore(this, caster, target);
        }

        //创建引导
        public void StartUpdata(ObjCharacter caster, ObjCharacter target)
        {
            mImpl.StartUpdata(this, caster, target);
        }

        //引导心跳
        public void Updata()
        {
            mImpl.Updata(this);
        }

        #endregion

        #region  释放相关

        //检查技能是否可以释放
        public ErrorCodes CheckSkill(ObjCharacter caster)
        {
            return mImpl.CheckSkill(this, caster);
        }

        public bool IsPassiveSkill()
        {
            return mImpl.IsPassiveSkill(this);
        }

        //释放成功
        public void DoSuccess()
        {
            mImpl.DoSuccess(this);
        }

        public void DoSuccessNoCd()
        {
            mImpl.DoSuccessNoCd(this);
        }

        public void DoCdAndCount()
        {
            mImpl.DoCdAndCount(this);
        }

        #endregion
    }

    #endregion

    #region  天赋数据

    public class TalentData
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public ulong buffGuid;
        public BuffData mBuff; //影响的Buff
        public int mId; //天赋ID
        public int mLevel; //天赋等级
        public TalentRecord mTable; //该天赋的表格
    }

    #endregion

    #region  天赋修改数据（By skill   Or   By Buff)

    public interface IModifyData
    {
        void AddFixedModify(ModifyData _this, int nField, int nValue);
        void AddScaleModify(ModifyData _this, int nField, int nValue);
        void AddSetModify(ModifyData _this, int nField, int nValue);
        int GetValue(ModifyData _this, int nField, int nOldValue);
        void RemoveFixedModify(ModifyData _this, int nField, int nValue);
        void RemoveScaleModify(ModifyData _this, int nField, int nValue);
        void RemoveSetModify(ModifyData _this, int nField, int nValue);
    }

    public class ModifyDataDefaultImpl : IModifyData
    {
        public int GetValue(ModifyData _this, int nField, int nOldValue)
        {
            int OldValue;
            if (_this.SetModify.TryGetValue(nField, out OldValue))
            {
//覆盖优先
                return OldValue;
            }

            if (_this.FixedModify.TryGetValue(nField, out OldValue))
            {
                nOldValue = nOldValue + OldValue;
            }
            if (_this.ScaleModify.TryGetValue(nField, out OldValue))
            {
                nOldValue = (int) (1.0*nOldValue*OldValue/10000);
            }
            return nOldValue;
        }

        public void RemoveFixedModify(ModifyData _this, int nField, int nValue)
        {
            int OldValue;
            if (_this.FixedModify.TryGetValue(nField, out OldValue))
            {
                var newValue = _this.FixedModify[nField] - nValue;
                if (newValue == 0)
                {
                    _this.FixedModify.Remove(nField);
                    return;
                }
                _this.FixedModify[nField] = newValue;
            }
        }

        public void RemoveScaleModify(ModifyData _this, int nField, int nValue)
        {
            int OldValue;
            if (_this.ScaleModify.TryGetValue(nField, out OldValue))
            {
                //避免后面出现除零的异常
                if (nValue == -10000)
                {
                    PlayerLog.WriteLog((ulong) LogType.RemoveScaleModify,
                        "ModifyData.RemoveScaleModify(), nValue = -10000, this will cause exception!!!");
                    _this.ScaleModify.Remove(nField);
                    return;
                }
                var newValue = (int) (1.0*_this.ScaleModify[nField]*10000/(10000 + nValue));
                if (newValue == 10000)
                {
                    _this.ScaleModify.Remove(nField);
                    return;
                }
                _this.ScaleModify[nField] = newValue;
            }
        }

        public void RemoveSetModify(ModifyData _this, int nField, int nValue)
        {
            //FixedModify[nField] = nValue;
            _this.SetModify.Remove(nField);
        }

        public void AddFixedModify(ModifyData _this, int nField, int nValue)
        {
            int OldValue;
            if (_this.FixedModify.TryGetValue(nField, out OldValue))
            {
                _this.FixedModify[nField] += nValue;
            }
            else
            {
                _this.FixedModify[nField] = nValue;
            }
        }

        public void AddScaleModify(ModifyData _this, int nField, int nValue)
        {
            int OldValue;
            if (_this.ScaleModify.TryGetValue(nField, out OldValue))
            {
                _this.ScaleModify[nField] = (int) (1.0*_this.ScaleModify[nField]*(10000 + nValue)/10000);
            }
            else
            {
                _this.ScaleModify[nField] = 10000 + nValue;
            }
        }

        public void AddSetModify(ModifyData _this, int nField, int nValue)
        {
            _this.SetModify[nField] = nValue;
        }
    }

    public class ModifyData
    {
        private static IModifyData mImpl;

        static ModifyData()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (ModifyData), typeof (ModifyDataDefaultImpl),
                o => { mImpl = (IModifyData) o; });
        }

        public Dictionary<int, int> FixedModify = new Dictionary<int, int>(); //字段ID,固定值
        public Dictionary<int, int> ScaleModify = new Dictionary<int, int>(); //字段ID,缩放值
        public Dictionary<int, int> SetModify = new Dictionary<int, int>(); //字段ID,覆盖值

        public void AddFixedModify(int nField, int nValue)
        {
            mImpl.AddFixedModify(this, nField, nValue);
        }

        public void AddScaleModify(int nField, int nValue)
        {
            mImpl.AddScaleModify(this, nField, nValue);
        }

        public void AddSetModify(int nField, int nValue)
        {
            mImpl.AddSetModify(this, nField, nValue);
        }

        public int GetValue(int nField, int nOldValue)
        {
            return mImpl.GetValue(this, nField, nOldValue);
        }

        public void RemoveFixedModify(int nField, int nValue)
        {
            mImpl.RemoveFixedModify(this, nField, nValue);
        }

        public void RemoveScaleModify(int nField, int nValue)
        {
            mImpl.RemoveScaleModify(this, nField, nValue);
        }

        public void RemoveSetModify(int nField, int nValue)
        {
            mImpl.RemoveSetModify(this, nField, nValue);
        }
    }

    #endregion

    //技能管理器


    public interface ISkillManager
    {
        SkillData AddSkill(SkillManager _this, int nSkill, int nLevel, eAddskillType type);
        TalentData AddTalent(SkillManager _this, int talent, int nLayer);
        int ChangeSkillId(SkillManager _this, int skillId);
        bool CheckCamp(ObjCharacter caster, ObjCharacter target, eCampType campType);
        void CheckCurrentSkill(SkillManager _this, eSkillEventType skillEvent);
        ErrorCodes CheckDoSkill(SkillManager _this, int skillId, int Level);
        ErrorCodes CheckSkill(SkillManager _this, ref int SkillId, ObjCharacter target = null);
        void CleanBuffModify(SkillManager _this, int nBuffId, int nFieldType, int nModifyType, int nModifyValue);
        void CleanSkillModify(SkillManager _this, int nSkillId, int nFieldType, int nModifyType, int nModifyValue);
        void DelSkill(SkillManager _this, int nSkill);
        void DelTalent(SkillManager _this, int TalentId);
        void DoNextSkill(SkillManager _this);
        ErrorCodes DoSkill(SkillManager _this, int skillId, int Level);
        ErrorCodes DoSkill(SkillManager _this, ref int skillId, ObjCharacter target = null);
        void EventToSkill(SkillManager _this, eSkillEventType skillEvent);
        int GetFightPoint(SkillManager _this);
        bool GetFightPointFlag(SkillManager _this);
        int GetModifyBuffValue(SkillManager _this, int nBuffId, int ModifyType, int OldValue);
        int GetModifySkillValue(SkillManager _this, int nSkillId, int ModifyType, int OldValue);

        List<ObjCharacter> GetPositionByCondition(ObjCharacter caster,
                                                  Vector2 pos,
                                                  eTargetType targetType,
                                                  int[] nTargetParam,
                                                  eCampType campType,
                                                  int nCount,
                                                  ObjCharacter target);

        SkillData GetSkill(SkillManager _this, int nId);
        float GetSkillDistance(SkillTargetType type, int[] param);
        TalentData GetTalent(SkillManager _this, int nId);

        List<ObjCharacter> GetTargetByCondition(ObjCharacter caster,
                                                ObjCharacter target,
                                                eTargetType targetType,
                                                int[] nTargetParam,
                                                eCampType campType,
                                                int nCount);

        void InitSkillManager(SkillManager _this, ObjCharacter obj);
        BuffRecord ModifyBuff(SkillManager _this, BuffRecord OldBuff, int nLevel);
        int ModifyByLevel(SkillManager _this, int nOldValue, int nLevel);
        SkillRecord ModifySkill(SkillManager _this, SkillRecord OldSkill, int nLevel);
        void PushBuffModify(SkillManager _this, int nBuffId, int nFieldType, int nModifyType, int nModifyValue);
        void PushSkillModify(SkillManager _this, int nSkillId, int nFieldType, int nModifyType, int nModifyValue);
        void RefreshModify(SkillManager _this);
        void ResetAllTalent(SkillManager _this);
        void ResetSkill(SkillManager _this, int nSkill, int nLevel);
        void ResetTalent(SkillManager _this, int talentId, int nLayer);
        void SetFightPointFlag(SkillManager _this, bool b = true);
        void SetNextSkill(SkillManager _this, SendUseSkillRequestInMessage msg, int nSkillId);
        void SkillOver(SkillManager _this, int skillId, ObjCharacter target = null);
        void SkillStart(SkillManager _this, SkillData thisskill);
        void StopCurrentSkill(SkillManager _this);
        ErrorCodes WillSkill(SkillManager _this, int skillId);
    }

    public class SkillManagerDefaultImpl : ISkillManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region  数据结构

        public void SetNextSkill(SkillManager _this, SendUseSkillRequestInMessage msg, int nSkillId)
        {
            if (_this.mNextSkillMsg != null)
            {
                _this.mNextSkillMsg.Reply((int) ErrorCodes.Error_SkillNoCD);
            }
            _this.mNextSkillMsg = msg;
            _this.mNextSkillId = nSkillId;
        }

        #endregion

        #region  初始化

        public void InitSkillManager(SkillManager _this, ObjCharacter obj)
        {
            _this.character = obj;
        }

        #endregion

        #region 等级修正

        public int ModifyByLevel(SkillManager _this, int nOldValue, int nLevel)
        {
            if (nOldValue < 10000000)
            {
                return nOldValue;
            }
            var tbUpgrade = Table.GetSkillUpgrading(nOldValue%10000000);
            if (tbUpgrade == null)
            {
                return nOldValue;
            }
            return tbUpgrade.GetSkillUpgradingValue(nLevel - 1);
            //switch (tbUpgrade.Type)
            //{
            //    case 0://枚举
            //        {
            //            if (nLevel > tbUpgrade.Values.Count || nLevel < 1)
            //            {
            //                Logger.Warn("ModifyByLevel=[{0}]  Level=[{1}]  is Out", nOldValue, nLevel);
            //                return 0;
            //            }
            //            int result = tbUpgrade.Values[nLevel - 1];
            //            PlayerLog.WriteLog(character.ObjId, "----------DoSkill----------OldValue={0},Lv={1},NewValue={2}", nOldValue, nLevel, result);
            //            return result;
            //        }
            //    case 1:
            //        {
            //            int result = tbUpgrade.Param[0] + tbUpgrade.Param[1] * nLevel;
            //            PlayerLog.WriteLog(character.ObjId, "----------DoSkill----------OldValue={0},Lv={1},NewValue={2}", nOldValue, nLevel, result);
            //            return result;
            //        }
            //}

            //Logger.Warn("ModifyByLevel=[{0}]  Level=[{1}]  is not find Type", nOldValue, nLevel);
            //return nOldValue;
        }

        #endregion

        //获得技能距离
        public float GetSkillDistance(SkillTargetType type, int[] param)
        {
            switch (type)
            {
                case SkillTargetType.SELF:
                    return 0;
                case SkillTargetType.SINGLE:
                    return param[0];
                case SkillTargetType.CIRCLE:
                    return param[0];
                case SkillTargetType.SECTOR:
                    return param[0];
                case SkillTargetType.RECT:
                    return param[1];
                case SkillTargetType.TARGET_CIRCLE:
                    return param[1];
                case SkillTargetType.TARGET_RECT:
                    return param[2];
                case SkillTargetType.TARGET_SECTOR:
                    return param[2];
                default:
                {
                    Logger.Warn("(SkillTargetType)[{0}] not Find ", type);
                    return 0;
                }
            }
        }

        #region 技能数据(增删改查)

        //增加技能（学会技能时处理）
        public SkillData AddSkill(SkillManager _this, int nSkill, int nLevel, eAddskillType type)
        {
            if (_this.character is ObjPlayer)
            {
                PlayerLog.WriteLog(_this.character.ObjId, "----------LearnSkill----------skillId={0}:Lv{1}", nSkill,
                    nLevel);
            }
            if (nSkill == -1)
            {
                return null;
            }
            var tbskill = Table.GetSkill(nSkill);
            if (tbskill == null)
            {
                Logger.Warn("AddSkill Error!  SkillId={0} not find!", nSkill);
                return null;
            }
            var skillData = GetSkill(_this, nSkill);
            if (skillData != null)
            {
                if (skillData.mIsActive)
                {
                    Logger.Warn("AddSkill Error!  SkillId={0} ,type={1},Is Have!", nSkill, type); //之后refresh skilldata
                    if (skillData.mBuff != null)
                    {
                        MissBuff.DoEffect(_this.character.Scene, _this.character, skillData.mBuff);
                        _this.character.DeleteBuff(skillData.mBuff, eCleanBuffType.ForgetSkill);
                        _this.character.BuffList.Do_Del_Buff();
                        skillData.mBuff = null;
                    }
                }
                else
                {
                    skillData.mIsActive = true;
                }
                var newTable = ModifySkill(_this, tbskill, nLevel);
                if (skillData.mTable.Layer != newTable.Layer)
                {
                    skillData.mDoCount = skillData.mTable.Layer;
                }
            }
            else
            {
                skillData = new SkillData();
                skillData.mId = nSkill;
                skillData.mSelf = _this.character;
                skillData.mTable = ModifySkill(_this, tbskill, nLevel);
                skillData.mDoCount = skillData.mTable.Layer;
            }
            skillData.mLevel = nLevel;
            skillData.From = type;
            if (skillData.mTable.CastType == 3)
            {
//被动技能
                skillData.mBuff = _this.character.AddBuff(skillData.mTable.CastParam[0], nLevel, _this.character, 0,
                    eHitType.Hit, 1);
            }
            _this.mData[nSkill] = skillData;
            //SkillData OldskillData;
            //if (_this.mData.TryGetValue(nSkill, out OldskillData))
            //{
            //    _this.mData[nSkill] = skillData;
            //    Logger.Warn("AddSkill is Have!skillId={0}", nSkill);
            //}
            //else
            //{
            //    _this.mData.Add(nSkill, skillData);
            //}
            SetFightPointFlag(_this);
            return skillData;
        }

        //删除技能（遗忘技能时处理）
        public void DelSkill(SkillManager _this, int nSkill)
        {
            PlayerLog.WriteLog(_this.character.ObjId, "----------DelSkill----------skillId={0}", nSkill);
            var skillData = GetSkill(_this, nSkill);
            if (skillData == null)
            {
                Logger.Warn("DelSkill not Find! Id=[{0}] ", nSkill);
                return;
            }
            //看是否有被动Buff
            if (skillData.mBuff != null)
            {
                MissBuff.DoEffect(_this.character.Scene, _this.character, skillData.mBuff);
                _this.character.DeleteBuff(skillData.mBuff, eCleanBuffType.ForgetSkill);
                _this.character.BuffList.Do_Del_Buff();
                skillData.mBuff = null;
            }
            if (skillData.mDoCount >= skillData.mDoMax)
            {
                _this.mData.Remove(nSkill);
            }
            else
            {
                skillData.mIsActive = false;
            }
            SetFightPointFlag(_this);
        }

        //重置技能等级
        public void ResetSkill(SkillManager _this, int nSkill, int nLevel)
        {
            if (nSkill == -1)
                return;

            PlayerLog.WriteLog(_this.character.ObjId, "----------ResetSkill----------skillId={0}:Lv{1}", nSkill, nLevel);
            var skillData = GetSkill(_this, nSkill);
            if (skillData == null)
            {
                Logger.Warn("ResetSkill not Find! Id=[{0}] ", nSkill);
            }
            else
            {
                DelSkill(_this, nSkill);
            }
            AddSkill(_this, nSkill, nLevel, eAddskillType.ResetSkill);
        }

        //获得技能数据
        public SkillData GetSkill(SkillManager _this, int nId)
        {
            SkillData thisskill;
            _this.mData.TryGetValue(nId, out thisskill);
            return thisskill;
        }

        #endregion

        #region 天赋数据(增删改查)

        //增加天赋
        public TalentData AddTalent(SkillManager _this, int talent, int nLayer)
        {
            PlayerLog.WriteLog(_this.character.ObjId, "----------AddTalent----------TalentId={0}:Layer={1}", talent,
                nLayer);
            var tbTalent = Table.GetTalent(talent);
            if (tbTalent == null)
            {
                Logger.Warn("AddTalent Error!  TalentId={0} not find!", talent);
                return null;
            }
            if (nLayer <= 0 || nLayer > tbTalent.MaxLayer)
            {
                Logger.Warn("AddTalent Error!  TalentId={0} of Level={1} is Out!", talent, nLayer);
                return null;
            }
            var talentData = GetTalent(_this, talent);
            if (talentData != null)
            {
                Logger.Warn("AddTalent Error!  TalentId={0} Is Have!", talent); //之后refresh skilldata
            }
            else
            {
                talentData = new TalentData();
            }
            talentData.mTable = tbTalent;
            talentData.mId = talent;
            talentData.mLevel = nLayer;
            //如果有技能的影响，应该由Logic同步过来
            var nBuffId = tbTalent.BuffId[nLayer - 1];
            if (nBuffId >= 0)
            {
//被动技能
                talentData.mBuff = _this.character.AddBuff(nBuffId, 1, _this.character, 0, eHitType.Hit, 1);
                if (null != talentData.mBuff)
                {
                    talentData.buffGuid = talentData.mBuff.mId;
                }
            }
            _this.mTalent[talent] = talentData;
            //属性相关
            if (tbTalent.AttrId != -1)
            {
                var tbskillup = Table.GetSkillUpgrading(tbTalent.SkillupgradingId);
                if (tbskillup != null)
                {
                    var attrs = new Dictionary<int, int>();
                    attrs.Add(tbTalent.AttrId, tbskillup.GetSkillUpgradingValue(nLayer));
                    ItemEquip2.AttrConvert(attrs, _this.character.Attr.mTalentData, _this.character.Attr.mTalentDataRef,
                        _this.character.Attr.GetAttackType());
                    _this.character.Attr.SetFlagByAttrId(tbTalent.AttrId);
                    //_this.character.Attr.SetFightPointFlag();
                }
            }
            return talentData;
        }

        //删除天赋
        public void DelTalent(SkillManager _this, int TalentId)
        {
            PlayerLog.WriteLog(_this.character.ObjId, "----------DelTalent----------TalentId={0}", TalentId);
            var talentData = GetTalent(_this, TalentId);
            if (talentData == null)
            {
                Logger.Warn("DelTalent not Find! Id=[{0}] ", TalentId);
                return;
            }
            if (talentData.mBuff != null)
            {
                if (talentData.mBuff.mId == talentData.buffGuid)
                {
                    MissBuff.DoEffect(_this.character.Scene, _this.character, talentData.mBuff);
                    _this.character.DeleteBuff(talentData.mBuff, eCleanBuffType.ForgetTalent);
                    _this.character.BuffList.Do_Del_Buff();
                }
                else
                {
                    Logger.Error("buff of talent {0} has been destroy.", talentData.mId);
                }
            }
            //属性相关
            var tbTalent = talentData.mTable;
            if (tbTalent.AttrId != -1)
            {
                var tbskillup = Table.GetSkillUpgrading(tbTalent.SkillupgradingId);
                if (tbskillup != null)
                {
                    var attrs = new Dictionary<int, int>();
                    attrs.Add(tbTalent.AttrId, -tbskillup.GetSkillUpgradingValue(talentData.mLevel));
                    ItemEquip2.AttrConvert(attrs, _this.character.Attr.mTalentData, _this.character.Attr.mTalentDataRef,
                        _this.character.Attr.GetAttackType());
                    _this.character.Attr.SetFlagByAttrId(tbTalent.AttrId);
                    //_this.character.Attr.SetFightPointFlag();
                }
            }
            _this.mTalent.Remove(TalentId);
        }

        //重置天赋点数
        public void ResetTalent(SkillManager _this, int talentId, int nLayer)
        {
            PlayerLog.WriteLog(_this.character.ObjId, "----------ResetTalent----------TalentId={0}:Layer={1}", talentId,
                nLayer);
            var talentData = GetTalent(_this, talentId);
            if (talentData == null)
            {
                Logger.Warn("ResetTalent not Find! Id=[{0}] ", talentId);
            }
            else
            {
                DelTalent(_this, talentId);
            }
            AddTalent(_this, talentId, nLayer);
        }

        //重置所有天赋点数
        public void ResetAllTalent(SkillManager _this)
        {
            PlayerLog.WriteLog(_this.character.ObjId, "----------ResetAllTalent----------");
            var templist = new List<int>();
            foreach (var i in _this.mTalent)
            {
                if (Table.GetTalent(i.Key).ModifySkill == -1)
                {
                    templist.Add(i.Key);
                }
            }
            foreach (var i in templist)
            {
                DelTalent(_this, i);
            }
            _this.character.BuffList.Do_Del_Buff();
            //mTalent.Clear();
        }

        //获得天赋数据
        public TalentData GetTalent(SkillManager _this, int nId)
        {
            TalentData thisTalent;
            _this.mTalent.TryGetValue(nId, out thisTalent);
            return thisTalent;
        }

        #endregion

        #region 技能释放(检查，消耗，作用，结束)

        //判断技能是否能释放
        public ErrorCodes CheckSkill(SkillManager _this, ref int SkillId, ObjCharacter target = null)
        {
            if (_this.character.IsDead())
            {
                return ErrorCodes.Error_CharacterDie;
            }
            //潜规则技能ID修改
            //if (SkillId == 0 || SkillId == 100 || SkillId == 200)
            //{
            //    int exdataValue;
            //    if (character.SkillExdata.TryGetValue(99, out exdataValue))
            //    {
            //        SkillId = SkillId + exdataValue;
            //    }
            //}
            //获得技能数据
            var thisskill = _this.character.Skill.GetSkill(SkillId);
            if (thisskill == null)
            {
                return ErrorCodes.Error_NotHaveSkill;
            }
            if (!thisskill.mIsActive)
            {
                return ErrorCodes.Error_NotHaveSkill;
            }
            //检查技能释放条件
            var errorCodes = thisskill.CheckSkill(_this.character);
            if (errorCodes != ErrorCodes.OK)
            {
                return errorCodes;
            }
            //技能是不是被动的
            if (thisskill.mTable.CastType == 3)
            {
                return ErrorCodes.Error_SkillNotCast;
            }
            //阵营检查
            if (target != null)
            {
                if (!CheckCamp(_this.character, target, (eCampType) thisskill.mTable.CampType)) //目标类型满足的单位
                {
                    return ErrorCodes.Error_CharacterCamp;
                }
            }
            //距离检查
            switch ((eTargetType) thisskill.mTable.TargetType)
            {
                case eTargetType.Self:
                    return ErrorCodes.OK;
                case eTargetType.Target:
                {
                    if (target == null)
                    {
                        return ErrorCodes.Error_SkillNoTarget;
                    }
                    if ((_this.character.GetPosition() - target.GetPosition()).LengthSquared() >
                        thisskill.mTable.TargetParam[0]*thisskill.mTable.TargetParam[0])
                    {
                        return ErrorCodes.Error_SkillDistance;
                    }
                    return ErrorCodes.OK;
                }
                case eTargetType.Around:
                case eTargetType.Fan:
                case eTargetType.Rect:
                {
                    return ErrorCodes.OK;
                }
                case eTargetType.TargetAround:
                {
                    if (target == null)
                    {
                        return ErrorCodes.Error_SkillNoTarget;
                    }
                    if ((_this.character.GetPosition() - target.GetPosition()).LengthSquared() >
                        thisskill.mTable.TargetParam[1]*thisskill.mTable.TargetParam[1])
                    {
                        return ErrorCodes.Error_SkillDistance;
                    }
                    return ErrorCodes.OK;
                }
                case eTargetType.TargetRect:
                {
                    if (target == null)
                    {
                        return ErrorCodes.Error_SkillNoTarget;
                    }
                    if ((_this.character.GetPosition() - target.GetPosition()).LengthSquared() >
                        thisskill.mTable.TargetParam[2]*thisskill.mTable.TargetParam[2])
                    {
                        return ErrorCodes.Error_SkillDistance;
                    }
                    return ErrorCodes.OK;
                }
                case eTargetType.TargetFan:
                {
                    if (target == null)
                    {
                        return ErrorCodes.Error_SkillNoTarget;
                    }
                    if ((_this.character.GetPosition() - target.GetPosition()).LengthSquared() >
                        thisskill.mTable.TargetParam[2]*thisskill.mTable.TargetParam[2])
                    {
                        return ErrorCodes.Error_SkillDistance;
                    }
                    return ErrorCodes.OK;
                }
                case eTargetType.Ejection:
                {
                    if (target == null)
                    {
                        return ErrorCodes.Error_SkillNoTarget;
                    }
                    if ((_this.character.GetPosition() - target.GetPosition()).LengthSquared() >
                        thisskill.mTable.TargetParam[0]*thisskill.mTable.TargetParam[0])
                    {
                        return ErrorCodes.Error_SkillDistance;
                    }
                    return ErrorCodes.OK;
                }
                default:
                {
                    Logger.Warn("(eTargetType)[{0}] not Find ", thisskill.mTable.TargetType);
                    return ErrorCodes.Unknow;
                }
            }
        }

        public int ChangeSkillId(SkillManager _this, int skillId)
        {
            //潜规则技能ID修改
            if (skillId == 0 || skillId == 100 || skillId == 200)
            {
                if (_this.character.Equip.ContainsKey(170))
                {
                    if (_this.lastEquip)
                    {
                        int exdataValue;
                        if (_this.character.SkillExdata.TryGetValue(99, out exdataValue))
                        {
                            skillId = skillId + exdataValue;
                        }
                    }
                    _this.lastEquip = true;
                }
                else
                {
                    switch (_this.character.TypeId)
                    {
                        case 0:
                        {
                            //战士
                            int exdataValue;
                            skillId = 13;
                            if (!_this.lastEquip)
                            {
                                if (_this.character.SkillExdata.TryGetValue(99, out exdataValue))
                                {
                                    skillId = skillId + exdataValue;
                                }
                                if (skillId < 13 || skillId > 16)
                                {
                                    skillId = 13;
                                }
                            }
                        }
                            break;
                        case 1:
                        {
                            //法师
                            int exdataValue;
                            skillId = 115;
                            if (!_this.lastEquip)
                            {
                                if (_this.character.SkillExdata.TryGetValue(99, out exdataValue))
                                {
                                    skillId = skillId + exdataValue;
                                }
                                if (skillId < 115 || skillId > 118)
                                {
                                    skillId = 115;
                                }
                            }
                        }
                            break;
                        case 2:
                        {
                            //弓手
                            int exdataValue;
                            skillId = 213;
                            if (!_this.lastEquip)
                            {
                                if (_this.character.SkillExdata.TryGetValue(99, out exdataValue))
                                {
                                    skillId = skillId + exdataValue;
                                }
                                if (skillId < 213 || skillId > 216)
                                {
                                    skillId = 213;
                                }
                            }
                        }
                            break;
                    }
                    _this.lastEquip = false;
                }

                skillId = GetRebornSkill(_this, _this.character.TypeId);
            }

            return skillId;
        }

        //获得转生普攻技能
        public int GetRebornSkill(SkillManager _this, int roleType)
        {
            var rebornId = _this.character.Attr.Ladder;
            var tbRe = Table.GetTransmigration(rebornId);
            var skillId = 0;
            if (tbRe != null && tbRe.PropPoint != -1)
            {
                switch (roleType)
                {
                    case 0:
                        {
                            //战士
                            int exdataValue;
                            var minSkillId = tbRe.zsRebornSkill[0];
                            var maxSkillId = tbRe.zsRebornSkill[3];
                            skillId = minSkillId;
                            if (_this.character.SkillExdata.TryGetValue(99, out exdataValue))
                            {
                                skillId = skillId + exdataValue;
                            }
                            if (skillId < minSkillId || skillId > maxSkillId)
                            {
                                skillId = minSkillId;
                            }
                        }
                        break;
                    case 1:
                        {
                            //法师
                            int exdataValue;
                            var minSkillId = tbRe.fsRebornSkill[0];
                            var maxSkillId = tbRe.fsRebornSkill[3];
                            skillId = minSkillId;
                            if (_this.character.SkillExdata.TryGetValue(99, out exdataValue))
                            {
                                skillId = skillId + exdataValue;
                            }
                            if (skillId < minSkillId || skillId > maxSkillId)
                            {
                                skillId = minSkillId;
                            }
                        }
                        break;
                    case 2:
                        {
                            //弓手
                            int exdataValue;
                            var minSkillId = tbRe.gsRebornSkill[0];
                            var maxSkillId = tbRe.gsRebornSkill[3];
                            skillId = minSkillId;
                            if (_this.character.SkillExdata.TryGetValue(99, out exdataValue))
                            {
                                skillId = skillId + exdataValue;
                            }
                            if (skillId < minSkillId || skillId > maxSkillId)
                            {
                                skillId = minSkillId;
                            }
                        }
                        break;
                }
            }

            return skillId;
        }

        //将要使用技能
        public ErrorCodes WillSkill(SkillManager _this, int skillId)
        {
            //潜规则技能ID修改
            skillId = ChangeSkillId(_this, skillId);

            //获得技能数据
            var thisskill = _this.character.Skill.GetSkill(skillId);
            if (thisskill == null)
            {
                return ErrorCodes.Error_NotHaveSkill;
            }
            //数据修改
            if (thisskill.mHp > 0)
            {
                _this.character.Attr.SetDataValue(eAttributeType.HpNow,
                    _this.character.Attr.GetDataValue(eAttributeType.HpNow) - thisskill.mHp);
            }
            if (thisskill.mMp > 0)
            {
                _this.character.Attr.SetDataValue(eAttributeType.MpNow,
                    _this.character.Attr.GetDataValue(eAttributeType.MpNow) - thisskill.mMp);
            }
            if (!thisskill.IsPassiveSkill()) // 装备技能不走公共cd
            {
                _this.character.SetSkill(thisskill.mTable.CommonCd);
                _this.character.StopMove();
                // 增加服务器容错100毫秒
                _this.character.SetMove(thisskill.mTable.NoMove - 100);
            }

            return ErrorCodes.OK;
        }

        //强制使用某技能，只有CD的检查
        public ErrorCodes CheckDoSkill(SkillManager _this, int skillId, int Level)
        {
            if (_this.character.IsDead())
            {
                return ErrorCodes.Error_CharacterDie;
            }
            var thisskill = _this.character.Skill.GetSkill(skillId);
            if (thisskill == null)
            {
                thisskill = AddSkill(_this, skillId, Level, eAddskillType.CheckDoSkill);
                thisskill.ItemSkill = true;
            }
            else
            {
                thisskill.mLevel = Level;
            }
            var result = thisskill.CheckSkill(_this.character);
            return result;
        }

        public ErrorCodes DoSkill(SkillManager _this, int skillId, int Level)
        {
            if (_this.character.IsDead())
            {
                return ErrorCodes.Error_CharacterDie;
            }
            var thisskill = _this.character.Skill.GetSkill(skillId);
            if (thisskill == null)
            {
                thisskill = AddSkill(_this, skillId, Level, eAddskillType.DoSkill);
                thisskill.ItemSkill = true;
            }
            else
            {
                thisskill.mLevel = Level;
            }
            var result = thisskill.CheckSkill(_this.character);
            if (result != ErrorCodes.OK)
            {
                return result;
            }
            //自身先获得buff
            _this.character.AddBuff(thisskill.mTable.BeforeBuff[0], thisskill.mLevel, _this.character);
            _this.character.AddBuff(thisskill.mTable.BeforeBuff[1], thisskill.mLevel, _this.character);
            //技能类型处理
            switch (thisskill.mTable.CastType)
            {
                case 0: //瞬发
                {
                    thisskill.DoEffect(_this.character, _this.character);
                    break;
                }
                case 1: //吟唱
                {
                    break;
                }
                case 2: //引导
                {
                    _this.CurrentSkill = thisskill;
                    thisskill.StartUpdata(_this.character, _this.character);
                    thisskill.DoSuccess();
                    return ErrorCodes.OK;
                }
                case 3: //被动
                {
                    return ErrorCodes.Error_SkillNoCD;
                }
            }
            //释放后  自身获得Buff
            SkillOver(_this, skillId, _this.character);
            //使用成功
            thisskill.DoSuccess();
            return ErrorCodes.OK;
        }

        //执行技能
        public ErrorCodes DoSkill(SkillManager _this, ref int skillId, ObjCharacter target = null)
        {
            if (_this.character.IsDead())
            {
                return ErrorCodes.Error_CharacterDie;
            }
            //潜规则技能ID修改
            skillId = ChangeSkillId(_this, skillId);
            if (target != null)
            {
                if (CheckCamp(_this.character, target, eCampType.Enemy))
                {
                    _this.EnemyTarget = target;
                }
            }
            //if (mData.Count == 0)
            //{
            //    return ErrorCodes.Error_NotHaveSkill;
            //}
            //skillId = mData.Random().Key;
            if (_this.character is ObjPlayer)
            {
                Logger.Info("----------DoSkill----------skillId={0},{1}", skillId, target == null ? 0 : target.ObjId);
            }
            //获得技能数据
            var thisskill = GetSkill(_this, skillId);
            if (thisskill == null)
            {
                thisskill = AddSkill(_this, skillId, 1, eAddskillType.DoSkill2);
                //return ErrorCodes.Error_NotHaveSkill;
            }
            if (!thisskill.IsPassiveSkill())
            {
                StopCurrentSkill(_this);

                thisskill.mTarget = target;
                _this.CurrentSkill = thisskill;                
            }

            //自身先获得buff
            _this.character.AddBuff(thisskill.mTable.BeforeBuff[0], thisskill.mLevel, _this.character);
            _this.character.AddBuff(thisskill.mTable.BeforeBuff[1], thisskill.mLevel, _this.character);
            //技能类型处理
            switch (thisskill.mTable.CastType)
            {
                case 0: //瞬发
                {
                    thisskill.DoCdAndCount();
                    thisskill.DoEffect(_this.character, target);
                    break;
                }
                case 1: //吟唱
                {
                    thisskill.DoCdAndCount();
                    break;
                }
                case 2: //引导
                {
                    thisskill.DoCdAndCount();
                    thisskill.StartUpdata(_this.character, target);
                    thisskill.DoSuccessNoCd();
                    return ErrorCodes.OK;
                }
                case 3: //被动
                {
                    return ErrorCodes.Error_SkillNoCD;
                }
                case 4: //特殊引导
                {
                    //thisskill.StartUpdata(_this.character, target);
                    //thisskill.DoSuccess();
                    thisskill.DoCdAndCount();// DoEffect会触发其它技能，如果不先设置cd会无限递归
                    thisskill.StartUpdata(_this.character, target);
                    thisskill.DoSuccessNoCd();
                    return ErrorCodes.OK;
                }
                case 5: //定点引导
                {
                    thisskill.DoCdAndCount();
                    thisskill.StartUpdata(_this.character, target);
                    thisskill.DoSuccessNoCd();
                    return ErrorCodes.OK;
                }
                case 6: //追踪引导
                {
                    thisskill.DoCdAndCount();
                    thisskill.StartUpdata(_this.character, target);
                    thisskill.DoSuccessNoCd();
                    return ErrorCodes.OK;
                }
                case 7: //定点引导（自己）
                {
                    thisskill.DoCdAndCount();
                    thisskill.StartUpdata(_this.character, target);
                    thisskill.DoSuccessNoCd();
                    return ErrorCodes.OK;
                }
            }
            //释放后  自身获得Buff
            SkillOver(_this, skillId, target);
            //使用成功
            thisskill.DoSuccessNoCd();
            return ErrorCodes.OK;
        }

        //技能结束时
        public void SkillOver(SkillManager _this, int skillId, ObjCharacter target = null)
        {
            //获得技能数据
            var thisskill = _this.character.Skill.GetSkill(skillId);
            if (thisskill == null)
            {
                return;
            }
            //自身先获得buff
            _this.character.AddBuff(thisskill.mTable.AfterBuff[0], thisskill.mLevel, _this.character);
            _this.character.AddBuff(thisskill.mTable.AfterBuff[1], thisskill.mLevel, _this.character);
            _this.CurrentSkill = null;
            //if ((thisskill.mSelf is ObjPlayer))
            //{
            //    DoNextSkill();
            //}

            //ObjPlayer player = character as ObjPlayer;
            //if (player!=null)
            //{
            //    //攻击装备耐久相关
            //    Dictionary<int, int> DurableList = new Dictionary<int, int>();
            //    foreach (var itemEquip2 in player.Equip)
            //    {
            //        if (itemEquip2.Key == 120) continue;
            //        ItemEquip2 equip = itemEquip2.Value;
            //        int now = equip.GetExdata(22);
            //        if (now <= 0) continue;
            //        if (Table.GetEquip(equip.GetId()).DurableType != 1) continue;
            //        if (MyRandom.Random(10000) > 100) continue;
            //        equip.SetDurable(now - 1);
            //        DurableList.Add(itemEquip2.Key, -1);
            //    }
            //    if (DurableList.Count > 0)
            //    {
            //        player.EquipDurableDown(DurableList);
            //    }
            //}
        }

        public void SkillStart(SkillManager _this, SkillData thisskill)
        {
        }

        public void DoNextSkill(SkillManager _this)
        {
            if (_this.mNextSkillId == -1)
            {
                return;
            }
            if (_this.mNextSkillMsg != null)
            {
                if (_this.character is ObjPlayer)
                {
                    _this.mNextSkillId = -1;
                    var co = CoroutineFactory.NewCoroutine(SceneServer.Instance.ServerControl.SendUseSkillRequest,
                        (_this.character as ObjPlayer).Proxy, _this.mNextSkillMsg);
                    co.MoveNext();
                    _this.mNextSkillMsg = null;
                }
                //character.UseSkill(mNextSkillId);
            }
        }

        #endregion

        #region 事件相关

        public void EventToSkill(SkillManager _this, eSkillEventType skillEvent)
        {
            switch (skillEvent)
            {
                case eSkillEventType.Move:
                {
//移动了，需要打断引导技能
                    CheckCurrentSkill(_this, skillEvent);
                }
                    break;
                case eSkillEventType.Silence:
                {
                }
                    break;
                case eSkillEventType.Stun:
                {
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("skillEvent");
            }
        }

        //检查事件是否需要打断技能
        public void CheckCurrentSkill(SkillManager _this, eSkillEventType skillEvent)
        {
            if (_this.CurrentSkill == null)
            {
                return;
            }
            if (skillEvent == eSkillEventType.ForceStop)
            {
                StopCurrentSkill(_this);
            }
            else if (_this.CurrentSkill.mTable.CastParam[3] == 1 && skillEvent == eSkillEventType.Move)
            {
                StopCurrentSkill(_this);
            }
        }

        //打断技能
        public void StopCurrentSkill(SkillManager _this)
        {
            if (_this.CurrentSkill == null)
            {
                return;
            }
            _this.CurrentSkill.StopSkill();
            _this.CurrentSkill = null;
        }

        #endregion

        #region 天赋修正

        //获得天赋修改后的新值(Buff)
        public int GetModifyBuffValue(SkillManager _this, int nBuffId, int ModifyType, int OldValue)
        {
            ModifyData modifyData;
            if (!_this.mBuff.TryGetValue(nBuffId, out modifyData))
            {
                return OldValue;
            }
            return modifyData.GetValue(ModifyType, OldValue);
        }

        //获得天赋修改后的新值(Skill)
        public int GetModifySkillValue(SkillManager _this, int nSkillId, int ModifyType, int OldValue)
        {
            ModifyData modifyData;
            if (!_this.mSkill.TryGetValue(nSkillId, out modifyData))
            {
                return OldValue;
            }
            return modifyData.GetValue(ModifyType, OldValue);
        }

        public void RefreshModify(SkillManager _this)
        {
            _this.mBuff.Clear();
            _this.mSkill.Clear();
            foreach (var buffData in _this.character.BuffList.mData)
            {
                for (var i = 0; i != buffData.mBuff.effectid.Length; ++i)
                {
                    if (buffData.mBuff.effectid[i] == (int) eEffectType.ModifyBuff)
                    {
                        //修改Buff
                        var nBuffId = buffData.mBuff.effectparam[i, 0];
                        var nFieldType = buffData.mBuff.effectparam[i, 1];
                        var nModifyType = buffData.mBuff.effectparam[i, 2];
                        var nModifyValue = buffData.mBuff.effectparam[i, 3];
                        ModifyData modifyData;
                        if (!_this.mSkill.TryGetValue(nBuffId, out modifyData))
                        {
                            modifyData = new ModifyData();
                            _this.mSkill[nBuffId] = modifyData;
                        }
                        switch (nModifyType)
                        {
                            case 0: //覆盖
                            {
                                modifyData.AddSetModify(nFieldType, nModifyValue);
                            }
                                break;
                            case 1: //万份比
                            {
                                modifyData.AddScaleModify(nFieldType, nModifyValue);
                            }
                                break;
                            case 2:
                            {
//固定值
                                modifyData.AddFixedModify(nFieldType, nModifyValue);
                            }
                                break;
                        }
                    }
                    else if (buffData.mBuff.effectid[i] == (int) eEffectType.ModifySkill)
                    {
                        //修改Skillint 
                        var nSkillId = buffData.mBuff.effectparam[i, 0];
                        var nFieldType = buffData.mBuff.effectparam[i, 1];
                        var nModifyType = buffData.mBuff.effectparam[i, 2];
                        var nModifyValue = buffData.mBuff.effectparam[i, 3];
                        ModifyData modifyData;
                        if (!_this.mSkill.TryGetValue(nSkillId, out modifyData))
                        {
                            modifyData = new ModifyData();
                            _this.mSkill[nSkillId] = modifyData;
                        }
                        switch (nModifyType)
                        {
                            case 0: //覆盖
                            {
                                modifyData.AddSetModify(nFieldType, nModifyValue);
                            }
                                break;
                            case 1: //万份比
                            {
                                modifyData.AddScaleModify(nFieldType, nModifyValue);
                            }
                                break;
                            case 2:
                            {
//固定值
                                modifyData.AddFixedModify(nFieldType, nModifyValue);
                            }
                                break;
                        }
                    }
                }
            }
        }

        public SkillRecord ModifySkill(SkillManager _this, SkillRecord OldSkill, int nLevel)
        {
            var NewSkill = new SkillRecord();
            var nId = OldSkill.Id;
            NewSkill.Id = OldSkill.Id;
            NewSkill.NeedHp = GetModifySkillValue(_this, nId, (int) eModifySkillType.NeedHp,
                ModifyByLevel(_this, OldSkill.NeedHp, nLevel));
            NewSkill.NeedMp = GetModifySkillValue(_this, nId, (int) eModifySkillType.NeedMp,
                ModifyByLevel(_this, OldSkill.NeedMp, nLevel));
            NewSkill.NeedAnger = GetModifySkillValue(_this, nId, (int) eModifySkillType.NeedAnger,
                ModifyByLevel(_this, OldSkill.NeedAnger, nLevel));
            NewSkill.Cd = GetModifySkillValue(_this, nId, (int) eModifySkillType.Cd,
                ModifyByLevel(_this, OldSkill.Cd, nLevel));
            NewSkill.Layer = GetModifySkillValue(_this, nId, (int) eModifySkillType.Layer,
                ModifyByLevel(_this, OldSkill.Layer, nLevel));
            NewSkill.CommonCd = GetModifySkillValue(_this, nId, (int) eModifySkillType.CommonCd,
                ModifyByLevel(_this, OldSkill.CommonCd, nLevel));
            NewSkill.BulletId = GetModifySkillValue(_this, nId, (int) eModifySkillType.BulletId, OldSkill.BulletId);
            NewSkill.ActionId = OldSkill.ActionId;
            NewSkill.NoMove = OldSkill.NoMove;
            NewSkill.CastType = GetModifySkillValue(_this, nId, (int) eModifySkillType.CastType,
                ModifyByLevel(_this, OldSkill.CastType, nLevel));
            NewSkill.CastParam[0] = GetModifySkillValue(_this, nId, (int) eModifySkillType.CastTypeParam1,
                ModifyByLevel(_this, OldSkill.CastParam[0], nLevel));
            NewSkill.CastParam[1] = GetModifySkillValue(_this, nId, (int) eModifySkillType.CastTypeParam2,
                ModifyByLevel(_this, OldSkill.CastParam[1], nLevel));
            NewSkill.CastParam[2] = GetModifySkillValue(_this, nId, (int) eModifySkillType.CastTypeParam3,
                ModifyByLevel(_this, OldSkill.CastParam[2], nLevel));
            NewSkill.CastParam[3] = GetModifySkillValue(_this, nId, (int) eModifySkillType.CastTypeParam4,
                ModifyByLevel(_this, OldSkill.CastParam[3], nLevel));
            NewSkill.ControlType = GetModifySkillValue(_this, nId, (int) eModifySkillType.ControlType,
                ModifyByLevel(_this, OldSkill.ControlType, nLevel));
            NewSkill.BeforeBuff[0] = GetModifySkillValue(_this, nId, (int) eModifySkillType.BeforeBuf1,
                ModifyByLevel(_this, OldSkill.BeforeBuff[0], nLevel));
            NewSkill.BeforeBuff[1] = GetModifySkillValue(_this, nId, (int) eModifySkillType.BeforeBuf2,
                ModifyByLevel(_this, OldSkill.BeforeBuff[1], nLevel));
            NewSkill.TargetType = GetModifySkillValue(_this, nId, (int) eModifySkillType.TargetType,
                ModifyByLevel(_this, OldSkill.TargetType, nLevel));
            NewSkill.TargetParam[0] = GetModifySkillValue(_this, nId, (int) eModifySkillType.TargetParam1,
                ModifyByLevel(_this, OldSkill.TargetParam[0], nLevel));
            NewSkill.TargetParam[1] = GetModifySkillValue(_this, nId, (int) eModifySkillType.TargetParam2,
                ModifyByLevel(_this, OldSkill.TargetParam[1], nLevel));
            NewSkill.TargetParam[2] = GetModifySkillValue(_this, nId, (int) eModifySkillType.TargetParam3,
                ModifyByLevel(_this, OldSkill.TargetParam[2], nLevel));
            NewSkill.TargetParam[3] = GetModifySkillValue(_this, nId, (int) eModifySkillType.TargetParam4,
                ModifyByLevel(_this, OldSkill.TargetParam[3], nLevel));
            NewSkill.TargetParam[4] = GetModifySkillValue(_this, nId, (int) eModifySkillType.TargetParam5,
                ModifyByLevel(_this, OldSkill.TargetParam[4], nLevel));
            NewSkill.TargetParam[5] = GetModifySkillValue(_this, nId, (int) eModifySkillType.TargetParam6,
                ModifyByLevel(_this, OldSkill.TargetParam[5], nLevel));

            NewSkill.CampType = OldSkill.CampType;
            NewSkill.DelayTarget = ModifyByLevel(_this, OldSkill.DelayTarget, nLevel);
            NewSkill.DelayView = ModifyByLevel(_this, OldSkill.DelayView, nLevel);
            NewSkill.TargetCount = GetModifySkillValue(_this, nId, (int) eModifySkillType.TargetCount,
                ModifyByLevel(_this, OldSkill.TargetCount, nLevel));
            NewSkill.AfterBuff[0] = GetModifySkillValue(_this, nId, (int) eModifySkillType.AfterBuff1,
                ModifyByLevel(_this, OldSkill.AfterBuff[0], nLevel));
            NewSkill.AfterBuff[1] = GetModifySkillValue(_this, nId, (int) eModifySkillType.AfterBuff2,
                ModifyByLevel(_this, OldSkill.AfterBuff[1], nLevel));
            NewSkill.MainTarget[0] = GetModifySkillValue(_this, nId, (int) eModifySkillType.MainTarget1,
                ModifyByLevel(_this, OldSkill.MainTarget[0], nLevel));
            NewSkill.MainTarget[1] = GetModifySkillValue(_this, nId, (int) eModifySkillType.MainTarget2,
                ModifyByLevel(_this, OldSkill.MainTarget[1], nLevel));
            NewSkill.MainTarget[2] = GetModifySkillValue(_this, nId, (int) eModifySkillType.MainTarget3,
                ModifyByLevel(_this, OldSkill.MainTarget[2], nLevel));
            NewSkill.MainTarget[3] = GetModifySkillValue(_this, nId, (int) eModifySkillType.MainTarget4,
                ModifyByLevel(_this, OldSkill.MainTarget[3], nLevel));
            NewSkill.MainTarget[0] = GetModifySkillValue(_this, nId, (int) eModifySkillType.MainTarget1,
                ModifyByLevel(_this, OldSkill.MainTarget[0], nLevel));
            NewSkill.MainTarget[1] = GetModifySkillValue(_this, nId, (int) eModifySkillType.MainTarget2,
                ModifyByLevel(_this, OldSkill.MainTarget[1], nLevel));
            NewSkill.OtherTarget[0] = GetModifySkillValue(_this, nId, (int) eModifySkillType.OtherTarget1,
                ModifyByLevel(_this, OldSkill.OtherTarget[0], nLevel));
            NewSkill.OtherTarget[1] = GetModifySkillValue(_this, nId, (int) eModifySkillType.OtherTarget2,
                ModifyByLevel(_this, OldSkill.OtherTarget[1], nLevel));
            NewSkill.ExdataChange = OldSkill.ExdataChange;
            NewSkill.HitType = GetModifySkillValue(_this, nId, (int) eModifySkillType.HitType, OldSkill.HitType);
            NewSkill.FightPoint = ModifyByLevel(_this, OldSkill.FightPoint, nLevel);
            NewSkill.Type = OldSkill.Type;
			NewSkill.TargetObjType = OldSkill.TargetObjType;
            return NewSkill;
        }

        public BuffRecord ModifyBuff(SkillManager _this, BuffRecord OldBuff, int nLevel)
        {
            var newBuff = new BuffRecord();
            newBuff.Id = OldBuff.Id;
            newBuff.Effect[0] = OldBuff.Effect[0];
            newBuff.Effect[1] = OldBuff.Effect[1];
            newBuff.Duration = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Duration,
                ModifyByLevel(_this, OldBuff.Duration, nLevel));
            newBuff.Type = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Type,
                ModifyByLevel(_this, OldBuff.Type, nLevel));
            newBuff.DownLine = OldBuff.DownLine;
            newBuff.IsView = OldBuff.IsView;
            newBuff.Die = OldBuff.Die;
            newBuff.SceneDisappear = OldBuff.SceneDisappear;
            newBuff.DieDisappear = OldBuff.DieDisappear;
            newBuff.HuchiId = OldBuff.HuchiId;
            newBuff.TihuanId = OldBuff.TihuanId;
            newBuff.PriorityId = OldBuff.PriorityId;
            newBuff.BearMax = OldBuff.BearMax;
	        newBuff.CoolDownTime = OldBuff.CoolDownTime;
            newBuff.LayerMax = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.LayerMax,
                ModifyByLevel(_this, OldBuff.LayerMax, nLevel));
            newBuff.effectid[0] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect1Id,
                OldBuff.effectid[0]);
            newBuff.effectid[1] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect2Id,
                OldBuff.effectid[1]);
            newBuff.effectid[2] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect3Id,
                OldBuff.effectid[2]);
            newBuff.effectid[3] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect4Id,
                OldBuff.effectid[3]);
            newBuff.effectpoint[0] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect1Point,
                OldBuff.effectpoint[0]);
            newBuff.effectpoint[1] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect2Point,
                OldBuff.effectpoint[1]);
            newBuff.effectpoint[2] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect3Point,
                OldBuff.effectpoint[2]);
            newBuff.effectpoint[3] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect4Point,
                OldBuff.effectpoint[3]);
            newBuff.EffectPointParam[0] = GetModifyBuffValue(_this, OldBuff.Id, (int)eModifyBuffType.Effect1PointParam, OldBuff.EffectPointParam[0]);
            newBuff.EffectPointParam[1] = GetModifyBuffValue(_this, OldBuff.Id, (int)eModifyBuffType.Effect2PointParam, OldBuff.EffectPointParam[1]);
            newBuff.EffectPointParam[2] = GetModifyBuffValue(_this, OldBuff.Id, (int)eModifyBuffType.Effect3PointParam, OldBuff.EffectPointParam[2]);
            newBuff.EffectPointParam[3] = GetModifyBuffValue(_this, OldBuff.Id, (int)eModifyBuffType.Effect4PointParam, OldBuff.EffectPointParam[3]);
            newBuff.effectparam[0, 0] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect1Param1,
                ModifyByLevel(_this, OldBuff.effectparam[0, 0], nLevel));
            newBuff.effectparam[0, 1] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect1Param2,
                ModifyByLevel(_this, OldBuff.effectparam[0, 1], nLevel));
            newBuff.effectparam[0, 2] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect1Param3,
                ModifyByLevel(_this, OldBuff.effectparam[0, 2], nLevel));
            newBuff.effectparam[0, 3] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect1Param4,
                ModifyByLevel(_this, OldBuff.effectparam[0, 3], nLevel));
            newBuff.effectparam[0, 4] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect1Param5,
                ModifyByLevel(_this, OldBuff.effectparam[0, 4], nLevel));
            newBuff.effectparam[0, 5] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect1Param6,
                ModifyByLevel(_this, OldBuff.effectparam[0, 5], nLevel));
            newBuff.effectparam[1, 0] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect2Param1,
                ModifyByLevel(_this, OldBuff.effectparam[1, 0], nLevel));
            newBuff.effectparam[1, 1] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect2Param2,
                ModifyByLevel(_this, OldBuff.effectparam[1, 1], nLevel));
            newBuff.effectparam[1, 2] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect2Param3,
                ModifyByLevel(_this, OldBuff.effectparam[1, 2], nLevel));
            newBuff.effectparam[1, 3] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect2Param4,
                ModifyByLevel(_this, OldBuff.effectparam[1, 3], nLevel));
            newBuff.effectparam[1, 4] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect2Param5,
                ModifyByLevel(_this, OldBuff.effectparam[1, 4], nLevel));
            newBuff.effectparam[1, 5] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect2Param6,
                ModifyByLevel(_this, OldBuff.effectparam[1, 5], nLevel));
            newBuff.effectparam[2, 0] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect3Param1,
                ModifyByLevel(_this, OldBuff.effectparam[2, 0], nLevel));
            newBuff.effectparam[2, 1] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect3Param2,
                ModifyByLevel(_this, OldBuff.effectparam[2, 1], nLevel));
            newBuff.effectparam[2, 2] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect3Param3,
                ModifyByLevel(_this, OldBuff.effectparam[2, 2], nLevel));
            newBuff.effectparam[2, 3] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect3Param4,
                ModifyByLevel(_this, OldBuff.effectparam[2, 3], nLevel));
            newBuff.effectparam[2, 4] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect3Param5,
                ModifyByLevel(_this, OldBuff.effectparam[2, 4], nLevel));
            newBuff.effectparam[2, 5] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect3Param6,
                ModifyByLevel(_this, OldBuff.effectparam[2, 5], nLevel));
            newBuff.effectparam[3, 0] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect4Param1,
                ModifyByLevel(_this, OldBuff.effectparam[3, 0], nLevel));
            newBuff.effectparam[3, 1] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect4Param2,
                ModifyByLevel(_this, OldBuff.effectparam[3, 1], nLevel));
            newBuff.effectparam[3, 2] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect4Param3,
                ModifyByLevel(_this, OldBuff.effectparam[3, 2], nLevel));
            newBuff.effectparam[3, 3] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect4Param4,
                ModifyByLevel(_this, OldBuff.effectparam[3, 3], nLevel));
            newBuff.effectparam[3, 4] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect4Param5,
                ModifyByLevel(_this, OldBuff.effectparam[3, 4], nLevel));
            newBuff.effectparam[3, 5] = GetModifyBuffValue(_this, OldBuff.Id, (int) eModifyBuffType.Effect4Param6,
                ModifyByLevel(_this, OldBuff.effectparam[3, 5], nLevel));
            return newBuff;
        }

        public void PushBuffModify(SkillManager _this, int nBuffId, int nFieldType, int nModifyType, int nModifyValue)
        {
            //int nBuffId = buffData.mBuff.effectparam[i, 0];
            //int nFieldType = buffData.mBuff.effectparam[i, 1];
            //int nModifyType = buffData.mBuff.effectparam[i, 2];
            //int nModifyValue = buffData.mBuff.effectparam[i, 3];
            ModifyData modifyData;
            if (!_this.mBuff.TryGetValue(nBuffId, out modifyData))
            {
                modifyData = new ModifyData();
                _this.mBuff[nBuffId] = modifyData;
            }
            switch (nModifyType)
            {
                case 0: //覆盖
                {
                    modifyData.AddSetModify(nFieldType, nModifyValue);
                }
                    break;
                case 1: //万份比
                {
                    modifyData.AddScaleModify(nFieldType, nModifyValue);
                }
                    break;
                case 2:
                {
//固定值
                    modifyData.AddFixedModify(nFieldType, nModifyValue);
                }
                    break;
            }
        }

        public void PushSkillModify(SkillManager _this, int nSkillId, int nFieldType, int nModifyType, int nModifyValue)
        {
            //int nSkillId = buffData.mBuff.effectparam[i, 0];
            //int nFieldType = buffData.mBuff.effectparam[i, 1];
            //int nModifyType = buffData.mBuff.effectparam[i, 2];
            //int nModifyValue = buffData.mBuff.effectparam[i, 3];
            ModifyData modifyData;
            if (!_this.mSkill.TryGetValue(nSkillId, out modifyData))
            {
                modifyData = new ModifyData();
                _this.mSkill[nSkillId] = modifyData;
            }
            switch (nModifyType)
            {
                case 0: //覆盖
                {
                    modifyData.AddSetModify(nFieldType, nModifyValue);
                }
                    break;
                case 1: //万份比
                {
                    modifyData.AddScaleModify(nFieldType, nModifyValue);
                }
                    break;
                case 2:
                {
//固定值
                    modifyData.AddFixedModify(nFieldType, nModifyValue);
                }
                    break;
            }
            //重置该技能数据
            var skill = GetSkill(_this, nSkillId);
            if (skill == null)
            {
                return;
            }
            skill.mTable = ModifySkill(_this, Table.GetSkill(nSkillId), skill.mLevel);
            skill.mDoCount = skill.mTable.Layer;
        }

        public void CleanBuffModify(SkillManager _this, int nBuffId, int nFieldType, int nModifyType, int nModifyValue)
        {
            //int nBuffId = buffData.mBuff.effectparam[i, 0];
            //int nFieldType = buffData.mBuff.effectparam[i, 1];
            //int nModifyType = buffData.mBuff.effectparam[i, 2];
            //int nModifyValue = buffData.mBuff.effectparam[i, 3];
            ModifyData modifyData;
            if (!_this.mBuff.TryGetValue(nBuffId, out modifyData))
            {
                return;
            }
            switch (nModifyType)
            {
                case 0: //覆盖
                {
                    modifyData.RemoveSetModify(nFieldType, nModifyValue);
                }
                    break;
                case 1: //万份比
                {
                    modifyData.RemoveScaleModify(nFieldType, nModifyValue);
                }
                    break;
                case 2:
                {
//固定值
                    modifyData.RemoveFixedModify(nFieldType, nModifyValue);
                }
                    break;
            }
        }

        public void CleanSkillModify(SkillManager _this, int nSkillId, int nFieldType, int nModifyType, int nModifyValue)
        {
            //int nSkillId = buffData.mBuff.effectparam[i, 0];
            //int nFieldType = buffData.mBuff.effectparam[i, 1];
            //int nModifyType = buffData.mBuff.effectparam[i, 2];
            //int nModifyValue = buffData.mBuff.effectparam[i, 3];
            ModifyData modifyData;
            if (!_this.mSkill.TryGetValue(nSkillId, out modifyData))
            {
                return;
            }
            switch (nModifyType)
            {
                case 0: //覆盖
                {
                    modifyData.RemoveSetModify(nFieldType, nModifyValue);
                }
                    break;
                case 1: //万份比
                {
                    modifyData.RemoveScaleModify(nFieldType, nModifyValue);
                }
                    break;
                case 2:
                {
//固定值
                    modifyData.RemoveFixedModify(nFieldType, nModifyValue);
                }
                    break;
            }
            //重置该技能数据
            var skill = GetSkill(_this, nSkillId);
            if (skill == null)
            {
                return;
            }
            skill.mTable = ModifySkill(_this, Table.GetSkill(nSkillId), skill.mLevel);
        }

        #endregion

        #region 目标筛选

        //PK模式 0是相同强制不能打（优先）   1是不同则打（非优先）   2是无视
        public bool CheckPKModel(ObjPlayer casterPlayer, ObjPlayer targetPlayer)
        {
            if (casterPlayer == targetPlayer)
            {
                return false;
            }
            var tbPKModel = Table.GetPKMode(casterPlayer.mDbData.PKModel);
            if (tbPKModel == null)
            {
                Logger.Error("CheckPKModel is null PKModel ={0}", casterPlayer.mDbData.PKModel);
                return false;
            }
            var PkValue = targetPlayer.KillerValue;
            var isRedName = PkValue >= 100 || targetPlayer.PkTime > 0;
            if (isRedName)
            {
                if (casterPlayer.GetTeamId() != 0 && casterPlayer.GetTeamId() == targetPlayer.GetTeamId())
                {
                    return tbPKModel.RedTeam == 2;
                }
                if (tbPKModel.RedTeam == 1 || tbPKModel.RedTeam == 2)
                {
                    return true;
                }

                if (casterPlayer.GetAllianceId() != 0 && casterPlayer.GetAllianceId() == targetPlayer.GetAllianceId())
                {
                    return tbPKModel.RedUnion == 2;
                }
                if (tbPKModel.RedUnion == 1 || tbPKModel.RedUnion == 2)
                {
                    return true;
                }

                if (tbPKModel.RedState == 1)
                {
                    return true;
                }
                return false;
            }
            if (casterPlayer.GetTeamId() != 0 && casterPlayer.GetTeamId() == targetPlayer.GetTeamId())
            {
                if (tbPKModel.NomalTeam == 2)
                {
                    return true;
                }
                return false;
            }
            if (tbPKModel.NomalTeam == 1 || tbPKModel.NomalTeam == 2)
            {
                return true;
            }

            if (casterPlayer.GetAllianceId() != 0 && casterPlayer.GetAllianceId() == targetPlayer.GetAllianceId())
            {
                if (tbPKModel.NomalUnion == 2)
                {
                    return true;
                }
                return false;
            }
            if (tbPKModel.NomalUnion == 1 || tbPKModel.NomalUnion == 2)
            {
                return true;
            }

            if (tbPKModel.NomalState == 1)
            {
                return true;
            }
            return false;
        }

        //阵营比较
        public bool CheckCamp(ObjCharacter caster, ObjCharacter target, eCampType campType)
        {
            var tempCaster = caster;
            var retinueCaster = caster as ObjRetinue;
            if (retinueCaster != null)
            {
                tempCaster = retinueCaster.Owner;
            }
            if (tempCaster == null)
            {
                return false;
            }
            var tempTarget = target;
            var retinueTarget = target as ObjRetinue;
            if (retinueTarget != null)
            {
                tempTarget = retinueTarget.Owner;
            }
            if (tempTarget == null)
            {
                return false;
            }
            switch (campType)
            {
                case eCampType.Enemy:
                {
                    if (tempCaster is ObjPlayer && tempTarget is ObjPlayer && tempTarget.Scene != null)
                    {
                        var scene = tempTarget.Scene;
                        var pvpRuleId = scene.PvpRuleId;
                        //潜规则，5号pvprule的规则就是各自为战
                        if (pvpRuleId == 5)
                        {
                            return scene.CanPk() && tempCaster != tempTarget;
                        }
                        var tbPvP = Table.GetPVPRule(pvpRuleId);
                        if (tbPvP != null && tbPvP.CanPK == 1)
                        {
                            if (tempTarget.GetLevel() >= tbPvP.ProtectLevel &&
                                tempCaster.GetLevel() >= tbPvP.ProtectLevel)
                            {
                                var casterPlayer = (ObjPlayer) tempCaster;
                                var targetPlayer = (ObjPlayer) tempTarget;
                                if (CheckPKModel(casterPlayer, targetPlayer))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    var tbCasterCamp = Table.GetCamp(tempCaster.GetCamp());
                    if (tbCasterCamp == null)
                    {
                        Logger.Warn("CheckCamp Caster={0},Target={1}", tempCaster.GetCamp(), tempTarget.GetCamp());
                        return false;
                    }
                    var targetCamp = tempTarget.GetCamp();
                    if (targetCamp < 0 || targetCamp >= tbCasterCamp.Camp.Length)
                    {
                        Logger.Warn("CheckCamp Caster={0},Target={1}", tempCaster.GetCamp(), tempTarget.GetCamp());
                        return false;
                    }
                    if (tbCasterCamp.Camp[targetCamp] == 1)
                    {
                        return true;
                    }
                    return false;
                    //if (caster.GetCamp() == target.GetCamp())
                    //{
                    //    return false;
                    //}
                    //return true;
                }
                case eCampType.Neutral:
                {
                    if (tempCaster.GetCamp() == tempTarget.GetCamp())
                    {
                        return true;
                    }
                    return true;
                }
                case eCampType.Friend:
                {
                    var tbCasterCamp = Table.GetCamp(tempCaster.GetCamp());
                    if (tbCasterCamp == null)
                    {
                        Logger.Warn("CheckCamp Caster={0},Target={1}", tempCaster.GetCamp(), tempTarget.GetCamp());
                        return false;
                    }
                    var targetCamp = tempTarget.GetCamp();
                    if (targetCamp < 0 || targetCamp >= tbCasterCamp.Camp.Length)
                    {
                        Logger.Warn("CheckCamp Caster={0},Target={1}", tempCaster.GetCamp(), tempTarget.GetCamp());
                        return false;
                    }
                    if (tbCasterCamp.Camp[targetCamp] != 1)
                    {
                        return true;
                    }
                    return false;
                    //if (caster.GetCamp() == target.GetCamp())
                    //{
                    //    return true;
                    //}
                    //return false;
                }
                case eCampType.Team:
                {
                    if (tempCaster == tempTarget)
                    {
                        return true;
                    }
                    var casterTeamId = tempCaster.GetTeamId();
                    if (casterTeamId == 0)
                    {
                        return false;
                    }
                    if (casterTeamId == tempTarget.GetTeamId())
                    {
                        return true;
                    }
                    return false;
                }
                case eCampType.All:
                {
                    return true;
                }
                default:
                {
                    Logger.Warn("CheckCamp  eCampType[{0}] not checkcase ", campType);
                    return true;
                }
            }
        }

        //阵营目标筛选
        public List<ObjCharacter> ScreeningTarget(List<ObjCharacter> targetlist,
                                                  ObjCharacter caster,
                                                  eCampType type,
                                                  int nCount)
        {
            var list = new List<ObjCharacter>();
            foreach (var character in targetlist)
            {
                if (CheckCamp(caster, character, type))
                {
                    list.Add(character);
                }
            }
            return list;
        }

        //根据范围和阵营条件获得目标List
        public List<ObjCharacter> GetTargetByCondition(ObjCharacter caster,
                                                       ObjCharacter target,
                                                       eTargetType targetType,
                                                       int[] nTargetParam,
                                                       eCampType campType,
                                                       int nCount)
        {
            var list = new List<ObjCharacter>();
            Shape shape = null;
            switch (targetType)
            {
                case eTargetType.Self:
                {
                    list.Add(caster);
                    return list;
                }
                case eTargetType.Target:
                {
//0=施法距离
                    if (target == null)
                    {
                        return list;
                    }
                    list.Add(target);
                    return list;
                }
                case eTargetType.Around:
                {
//0=半径
                    shape = new CircleShape(caster.GetPosition(), nTargetParam[0]);
                }
                    break;
                case eTargetType.Fan:
                {
//0=半径  1=度数
                    shape = new FanShape(caster.GetPosition(), caster.GetDirection(), nTargetParam[0], nTargetParam[1]);
                }
                    break;
                case eTargetType.Rect:
                {
//0=宽度  1=长度
                    shape = new OrientedBox(caster.GetPosition(), nTargetParam[0], nTargetParam[1],
                        caster.GetDirection());
                }
                    break;
                case eTargetType.TargetAround:
                {
//0=半径  1=施法距离
                    if (target == null)
                    {
                        return list;
                    }
                    shape = new CircleShape(target.GetPosition(), nTargetParam[0]);
                }
                    break;
                case eTargetType.TargetRect:
                {
//0=宽度  1=长度  2=施法距离
                    if (target == null)
                    {
                        return list;
                    }
                    if (nTargetParam[1] < 0)
                    {
                        var h = (target.GetPosition() - caster.GetPosition()).Length();
                        shape = new OrientedBox(caster.GetPosition(), nTargetParam[0], h, caster.GetDirection());
                    }
                    else
                    {
                        shape = new OrientedBox(caster.GetPosition(), nTargetParam[0], nTargetParam[1],
                            Vector2.Normalize(target.GetPosition() - caster.GetPosition()));
                    }
                }
                    break;
                case eTargetType.TargetFan:
                {
//0=半径  1=度数  2=施法距离
                    if (target == null)
                    {
                        return list;
                    }
                    shape = new FanShape(caster.GetPosition(),
                        Vector2.Normalize(target.GetPosition() - caster.GetPosition()), nTargetParam[0], nTargetParam[1]);
                }
                    break;
                case eTargetType.Ejection:
                {
//0=施法距离  1=传递半径  2=传递次数 3=是否弹射  4=单目标承受次数  5=参数修正
                    if (target == null)
                    {
                        return list;
                    }
                    list.Add(target);
                    var tempCharacter = target; //上次的目标
                    for (var i = 0; i < nCount - 1; ++i)
                    {
                        //List<ObjCharacter> tempList = new List<ObjCharacter>();
                        var tempDistance = new Dictionary<ObjCharacter, float>();
                        Shape tempShape = new CircleShape(tempCharacter.GetPosition(), nTargetParam[1]);
                        tempCharacter.Scene.SceneShapeAction(tempShape, obj =>
                        {
                            var tempCount = list.Count(a => a == tempCharacter);
                            if (tempCount < nTargetParam[4]) //选择承受次数不足的单位
                            {
                                if (CheckCamp(caster, obj, campType)) //目标类型满足的单位
                                {
                                    tempDistance[obj] =
                                        (tempCharacter.GetPosition() - obj.GetPosition()).LengthSquared();
                                    //tempList.Add(obj);
                                }
                            }
                        });
                        if (nTargetParam[3] == 1)
                        {
//如果弹射，那么上次目标不在这次选择列表
                            tempDistance.Remove(tempCharacter);
                        }
                        if (tempDistance.Count <= 0)
                        {
//可弹射的目标已经没有了
                            return list;
                        }
                        //选择目标逻辑目前是随机，也可以找次数少的，或者距离近的
                        tempCharacter = tempDistance.Random().Key;
                            //   tempList[tempindex[0]];   //tempDistance.Min().Key;
                        //添加到下次弹射的目标列表中
                        list.Add(tempCharacter);
                    }
                    return list;
                }
                default:
                    throw new ArgumentOutOfRangeException("nType");
            }
            if (caster.Scene != null)
            {
                caster.Scene.SceneShapeAction(shape, obj =>
                {
                    if (CheckCamp(caster, obj, campType))
                    {
                        if (obj == null || obj.IsDead() )
                            return;
                        if ((campType == eCampType.Team || campType == eCampType.Friend) && obj.GetObjType() == ObjType.RETINUE)
                        {
                            return;
                        }
                        list.Add(obj);
                    }
                });
            }
            else
            {
                caster.Skill.StopCurrentSkill();
            }
            if (target != null)
            {
                var index = 0;
                foreach (var objCharacter in list)
                {
                    if (objCharacter == target)
                    {
                        var tempSwap = list[0];
                        list[0] = target;
                        list[index] = tempSwap;
                        break;
                    }
                    index++;
                }
            }

            //数量筛选
            if (nCount > 0 && nCount < list.Count)
            {
				list.RemoveRange(nCount,list.Count-nCount);
				/*
                var tempObjList = new List<ObjCharacter>();
                tempObjList.Add(list[0]);
                if (nCount == 1)
                {
                    return tempObjList;
                }
                tempObjList.AddRange(list.RandRange(1, nCount - 1));
                return tempObjList;
				 * */
            }
            return list;
        }


        //根据范围和阵营条件获得目标List
        public List<ObjCharacter> GetPositionByCondition(ObjCharacter caster,
                                                         Vector2 pos,
                                                         eTargetType targetType,
                                                         int[] nTargetParam,
                                                         eCampType campType,
                                                         int nCount,
                                                         ObjCharacter target)
        {
            var list = new List<ObjCharacter>();
            Shape shape = null;
            switch (targetType)
            {
                case eTargetType.Self:
                {
                    list.Add(caster);
                    return list;
                }
                case eTargetType.Target:
                {
//0=施法距离
                    if (target != null)
                    {
                        list.Add(target);
                    }
                    return list;
                }
                case eTargetType.Around:
                { //0=半径
                    shape = new CircleShape(pos, nTargetParam[0]);
                }
                    break;
                case eTargetType.Fan:
                {
//0=半径  1=度数
                    shape = new FanShape(caster.GetPosition(), caster.GetDirection(), nTargetParam[0], nTargetParam[1]);
                }
                    break;
                case eTargetType.Rect:
                {
//0=宽度  1=长度
                    shape = new OrientedBox(caster.GetPosition(), nTargetParam[0], nTargetParam[1],
                        caster.GetDirection());
                }
                    break;
                case eTargetType.TargetAround:
                {
//0=半径  1=施法距离
                    shape = new CircleShape(pos, nTargetParam[0]);
                }
                    break;
                case eTargetType.TargetRect:
                {
//0=宽度  1=长度  2=施法距离
                    if (nTargetParam[1] < 0)
                    {
                        var h = (pos - caster.GetPosition()).Length();
                        shape = new OrientedBox(caster.GetPosition(), nTargetParam[0], h, caster.GetDirection());
                    }
                    else
                    {
                        shape = new OrientedBox(caster.GetPosition(), nTargetParam[0], nTargetParam[1],
                            caster.GetDirection());
                    }
                }
                    break;
                case eTargetType.TargetFan:
                {
//0=半径  1=度数  2=施法距离
                    shape = new FanShape(caster.GetPosition(), caster.GetDirection(), nTargetParam[0], nTargetParam[1]);
                }
                    break;
                case eTargetType.Ejection:
                {
//0=施法距离  1=传递半径  2=传递次数 3=是否弹射  4=单目标承受次数  5=参数修正
                    return list;
                }
                default:
                    throw new ArgumentOutOfRangeException("nType");
            }
            if (caster.Scene != null)
            {
                caster.Scene.SceneShapeAction(shape, obj =>
                {
                    if (CheckCamp(caster, obj, campType))
                    {
                        if (obj != null && !obj.IsDead()) // 死亡的目标暂时不选择了
                        {
                            list.Add(obj);
                        }
                    }
                });
            }
            else
            {
                caster.Skill.StopCurrentSkill();
            }
            //数量筛选
            if (nCount > 0 && nCount < list.Count)
            {
				list.RemoveRange(nCount, list.Count - nCount);
				/*
                var tempObjList = new List<ObjCharacter>();
                tempObjList.Add(list[0]);
                if (nCount == 1)
                {
                    return tempObjList;
                }
                tempObjList.AddRange(list.RandRange(1, nCount - 1));
                return tempObjList;
				 * */
            }
            return list;
        }

        //筛选目标(区域)
        public List<ObjCharacter> ScreeningObjListByArea(ObjCharacter caster,
                                                         List<ObjCharacter> newList,
                                                         int nType,
                                                         int nParam0,
                                                         int nParam1)
        {
            var result = new List<ObjCharacter>();
            switch (nType)
            {
                case 0: //扇形
                {
                    foreach (var character in newList)
                    {
                        if (
                            !MyMath.IsInSector(caster.GetPosition(), caster.GetDirection(), character.GetPosition(),
                                nParam0,
                                MyMath.Angle2Radian(nParam1/2))) //0=距离  1=角度
                        {
                            result.Add(character);
                        }
                    }
                }
                    break;
                case 1: //周围
                {
                    foreach (var character in newList)
                    {
                        var dif = character.GetPosition() - caster.GetPosition();
                        if (dif.LengthSquared() <= nParam0*nParam0)
                        {
                            result.Add(character);
                        }
                    }
                }
                    break;
            }
            return result;
        }

        //筛选目标(关系)
        public List<ObjCharacter> ScreeningObjListByCamp(ObjCharacter caster,
                                                         List<ObjCharacter> newList,
                                                         eCampType campType)
        {
            var result = new List<ObjCharacter>();
            foreach (var character in newList)
            {
                if (CheckCamp(caster, character, campType))
                {
                    result.Add(character);
                }
            }
            return result;
        }

        #endregion

        #region 战斗力

        //获取技能提供的战斗力
        public int GetFightPoint(SkillManager _this)
        {
            if (!_this.FightPointFlag)
            {
                return _this.FightPoint;
            }
            _this.FightPointFlag = false;
            _this.FightPoint = 0;
            foreach (var data in _this.mData)
            {
                if (!data.Value.mIsActive)
                {
                    continue;
                }
                if (data.Value.mTable.FightPoint < 0)
                {
                }
                else
                {
                    _this.FightPoint +=
                        Table.GetSkillUpgrading(data.Value.mTable.FightPoint).GetSkillUpgradingValue(data.Value.mLevel);
                }
            }
            //天赋提供的战斗力
            foreach (var talent in _this.mTalent)
            {
                if (talent.Value.mTable.ModifySkill > 0)
                {
                    if (_this.mData.ContainsKey(talent.Value.mTable.ModifySkill) == false)
                        continue;
                    if (_this.mData[talent.Value.mTable.ModifySkill].mIsActive == false)
                        continue;
                }
                
                var skillup = talent.Value.mTable.FightPointBySkillUpgrading;
                if (skillup > 0)
                {
                    var tab = Table.GetSkillUpgrading(skillup);
                    if (tab != null)
                    {
                        _this.FightPoint += tab.GetSkillUpgradingValue(talent.Value.mLevel - 1);
                    }
                }
            }
            return _this.FightPoint;
        }

        //设置战斗力标记
        public void SetFightPointFlag(SkillManager _this, bool b = true)
        {
            _this.FightPointFlag = b;
            _this.character.Attr.OnPropertyChanged((uint) eSceneSyncId.SyncFightValue);
        }

        public bool GetFightPointFlag(SkillManager _this)
        {
            return _this.FightPointFlag;
        }

        #endregion
    }

    public class SkillManager : INotifyPropertyChanged
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static ISkillManager mImpl;

        static SkillManager()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (SkillManager), typeof (SkillManagerDefaultImpl),
                o => { mImpl = (ISkillManager) o; });
        }

        #region  初始化

        public SkillManager(ObjCharacter obj)
        {
            mImpl.InitSkillManager(this, obj);
        }

        #endregion

        //获得技能距离
        public static float GetSkillDistance(SkillTargetType type, int[] param)
        {
            return mImpl.GetSkillDistance(type, param);
        }

        #region 等级修正

        public int ModifyByLevel(int nOldValue, int nLevel)
        {
            return mImpl.ModifyByLevel(this, nOldValue, nLevel);
        }

        #endregion

        public virtual void OnPropertyChanged(string propertyName)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void Reset()
        {
            StopCurrentSkill();
            EnemyTarget = null;
            LastSkillMainTarget.SetTarget(null);
            foreach (var skillData in mData)
            {
                skillData.Value.mTarget = null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region  数据结构

        public Dictionary<int, SkillData> mData = new Dictionary<int, SkillData>(); //技能ID，技能等级
        public Dictionary<int, TalentData> mTalent = new Dictionary<int, TalentData>(); //天赋ID，技能等级
        public Dictionary<int, ModifyData> mBuff = new Dictionary<int, ModifyData>(); //BuffID，字段修改
        public Dictionary<int, ModifyData> mSkill = new Dictionary<int, ModifyData>(); //SkillID，字段修改
        public ObjCharacter character;
        public ObjCharacter EnemyTarget;
        public WeakReference<ObjCharacter> LastSkillMainTarget = new WeakReference<ObjCharacter>(null);
        public SkillData CurrentSkill { get; set; }
        public int mNextSkillId = -1;
        //private object mNextTrigger = null;   //CD时间触发器
        public SendUseSkillRequestInMessage mNextSkillMsg;

        public void SetNextSkill(SendUseSkillRequestInMessage msg, int nSkillId)
        {
            mImpl.SetNextSkill(this, msg, nSkillId);
        }

        public int FightPoint;
        public bool FightPointFlag = true;

        #endregion

        #region 技能数据(增删改查)

        //增加技能（学会技能时处理）
        public SkillData AddSkill(int nSkill, int nLevel, eAddskillType type)
        {
            return mImpl.AddSkill(this, nSkill, nLevel, type);
        }

        //删除技能（遗忘技能时处理）
        public void DelSkill(int nSkill)
        {
            mImpl.DelSkill(this, nSkill);
        }

        //重置技能等级
        public void ResetSkill(int nSkill, int nLevel)
        {
            mImpl.ResetSkill(this, nSkill, nLevel);
        }

        //获得技能数据
        public SkillData GetSkill(int nId)
        {
            return mImpl.GetSkill(this, nId);
        }

        #endregion

        #region 天赋数据(增删改查)

        //增加天赋
        public TalentData AddTalent(int talent, int nLayer)
        {
            return mImpl.AddTalent(this, talent, nLayer);
        }

        //删除天赋
        public void DelTalent(int TalentId)
        {
            mImpl.DelTalent(this, TalentId);
        }

        //重置天赋点数
        public void ResetTalent(int talentId, int nLayer)
        {
            mImpl.ResetTalent(this, talentId, nLayer);
        }

        //重置所有天赋点数
        public void ResetAllTalent()
        {
            mImpl.ResetAllTalent(this);
        }

        //获得天赋数据
        public TalentData GetTalent(int nId)
        {
            return mImpl.GetTalent(this, nId);
        }

        #endregion

        #region 技能释放(检查，消耗，作用，结束)

        //判断技能是否能释放
        public ErrorCodes CheckSkill(ref int SkillId, ObjCharacter target = null)
        {
            return mImpl.CheckSkill(this, ref SkillId, target);
        }

        public int ChangeSkillId(int skillId)
        {
            return mImpl.ChangeSkillId(this, skillId);
        }

        //将要使用技能
        public ErrorCodes WillSkill(int skillId)
        {
            return mImpl.WillSkill(this, skillId);
        }

        //强制使用某技能，只有CD的检查
        public ErrorCodes CheckDoSkill(int skillId, int Level)
        {
            return mImpl.CheckDoSkill(this, skillId, Level);
        }

        public ErrorCodes DoSkill(int skillId, int Level)
        {
            return mImpl.DoSkill(this, skillId, Level);
        }

        public bool lastEquip;
        //执行技能
        public ErrorCodes DoSkill(ref int skillId, ObjCharacter target = null)
        {
            return mImpl.DoSkill(this, ref skillId, target);
        }

        //技能结束时
        public void SkillOver(int skillId, ObjCharacter target = null)
        {
            mImpl.SkillOver(this, skillId, target);
        }

        public void SkillStart(SkillData thisskill)
        {
            mImpl.SkillStart(this, thisskill);
        }

        public void DoNextSkill()
        {
            mImpl.DoNextSkill(this);
        }

        #endregion

        #region 事件相关

        public void EventToSkill(eSkillEventType skillEvent)
        {
            mImpl.EventToSkill(this, skillEvent);
        }

        //检查事件是否需要打断技能
        public void CheckCurrentSkill(eSkillEventType skillEvent)
        {
            mImpl.CheckCurrentSkill(this, skillEvent);
        }

        //打断技能
        public void StopCurrentSkill()
        {
            mImpl.StopCurrentSkill(this);
        }

        #endregion

        #region 天赋修正

        //获得天赋修改后的新值(Buff)
        public int GetModifyBuffValue(int nBuffId, int ModifyType, int OldValue)
        {
            return mImpl.GetModifyBuffValue(this, nBuffId, ModifyType, OldValue);
        }

        //获得天赋修改后的新值(Skill)
        public int GetModifySkillValue(int nSkillId, int ModifyType, int OldValue)
        {
            return mImpl.GetModifySkillValue(this, nSkillId, ModifyType, OldValue);
        }

        public void RefreshModify()
        {
            mImpl.RefreshModify(this);
        }

        public SkillRecord ModifySkill(SkillRecord OldSkill, int nLevel)
        {
            return mImpl.ModifySkill(this, OldSkill, nLevel);
        }

        public BuffRecord ModifyBuff(BuffRecord OldBuff, int nLevel)
        {
            return mImpl.ModifyBuff(this, OldBuff, nLevel);
        }

        public void PushBuffModify(int nBuffId, int nFieldType, int nModifyType, int nModifyValue)
        {
            mImpl.PushBuffModify(this, nBuffId, nFieldType, nModifyType, nModifyValue);
        }

        public void PushSkillModify(int nSkillId, int nFieldType, int nModifyType, int nModifyValue)
        {
            mImpl.PushSkillModify(this, nSkillId, nFieldType, nModifyType, nModifyValue);
        }

        public void CleanBuffModify(int nBuffId, int nFieldType, int nModifyType, int nModifyValue)
        {
            mImpl.CleanBuffModify(this, nBuffId, nFieldType, nModifyType, nModifyValue);
        }

        public void CleanSkillModify(int nSkillId, int nFieldType, int nModifyType, int nModifyValue)
        {
            mImpl.CleanSkillModify(this, nSkillId, nFieldType, nModifyType, nModifyValue);
        }

        #endregion

        #region 目标筛选

        //阵营比较
        public static bool CheckCamp(ObjCharacter caster, ObjCharacter target, eCampType campType)
        {
            return mImpl.CheckCamp(caster, target, campType);
        }

        //根据范围和阵营条件获得目标List
        public static List<ObjCharacter> GetTargetByCondition(ObjCharacter caster,
                                                              ObjCharacter target,
                                                              eTargetType targetType,
                                                              int[] nTargetParam,
                                                              eCampType campType,
                                                              int nCount)
        {
            return mImpl.GetTargetByCondition(caster, target, targetType, nTargetParam, campType, nCount);
        }

        //根据范围和阵营条件获得目标List
        public static List<ObjCharacter> GetPositionByCondition(ObjCharacter caster,
                                                                Vector2 pos,
                                                                eTargetType targetType,
                                                                int[] nTargetParam,
                                                                eCampType campType,
                                                                int nCount,
                                                                ObjCharacter target)
        {
            return mImpl.GetPositionByCondition(caster, pos, targetType, nTargetParam, campType, nCount, target);
        }

        #endregion

        #region 战斗力

        //获取技能提供的战斗力
        public int GetFightPoint()
        {
            return mImpl.GetFightPoint(this);
        }

        //设置战斗力标记
        public void SetFightPointFlag(bool b = true)
        {
            mImpl.SetFightPointFlag(this, b);
        }

        public bool GetFightPointFlag()
        {
            return mImpl.GetFightPointFlag(this);
        }

        #endregion
    }
}