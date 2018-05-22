#region using

using System;
using System.Collections.Generic;
using System.Linq;
using DataTable;

#endregion

namespace Scene
{
    public class NPCScript200001 : NPCScript200000
    {
        private int CombatTime;
        private readonly Dictionary<int, DateTime> SkillCd = new Dictionary<int, DateTime>();

        private bool CheckSkillCd(int skillId)
        {
            DateTime dt;
            if (!SkillCd.TryGetValue(skillId, out dt))
            {
                return true;
            }
            if (dt > DateTime.Now)
            {
                return false;
            }
            return true;
        }

        public int CheckStateChange(ObjNPC npc)
        {
            if (npc.TableAI == null)
            {
                return -1;
            }
            if (npc.TableAI.NextAI == -1)
            {
                return -1;
            }
            switch (npc.TableAI.NextAICondition)
            {
                case 0: //血量
                {
                    switch (npc.TableAI.Type)
                    {
                        case 0: //低于绝对值
                        {
                            if (npc.Attr.GetDataValue(eAttributeType.HpNow) < npc.TableAI.Param)
                            {
                                return npc.TableAI.NextAI;
                            }
                        }
                            break;
                        case 1: //高于绝对值
                        {
                            if (npc.Attr.GetDataValue(eAttributeType.HpNow) > npc.TableAI.Param)
                            {
                                return npc.TableAI.NextAI;
                            }
                        }
                            break;
                        case 2: //低于百分比
                        {
                            if ((float) npc.Attr.GetDataValue(eAttributeType.HpNow)/
                                npc.Attr.GetDataValue(eAttributeType.HpMax) < (float) npc.TableAI.Param/10000)
                            {
                                return npc.TableAI.NextAI;
                            }
                        }
                            break;
                        case 3: //高于百分比
                        {
                            if ((float) npc.Attr.GetDataValue(eAttributeType.HpNow)/
                                npc.Attr.GetDataValue(eAttributeType.HpMax) > (float) npc.TableAI.Param/10000)
                            {
                                return npc.TableAI.NextAI;
                            }
                        }
                            break;
                    }
                }
                    break;
                case 1: //战斗时间
                {
                    switch (npc.TableAI.Type)
                    {
                        case 0: //低于绝对值
                        {
                            if (CombatTime/1000 < npc.TableAI.Param)
                            {
                                return npc.TableAI.NextAI;
                            }
                        }
                            break;
                        case 1: //高于绝对值
                        {
                            if (CombatTime/1000 > npc.TableAI.Param)
                            {
                                return npc.TableAI.NextAI;
                            }
                        }
                            break;
                    }
                }
                    break;
            }
            return -1;
        }
        /// <summary>
        /// 特殊处理怪物技能，基础角色表中没有的技能，无法释放
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="skillId"></param>
        /// <returns></returns>
        private bool CheckInitSkillSkill(ObjNPC npc, int skillId)
        {
            var tb = Table.GetCharacterBase(npc.TypeId);
            if (null != tb)
            {
                if (!tb.InitSkill.Contains(skillId))
                {
                    return false;
                }
            }
            return true;
        }

        private int GetCanDoSkill(ObjNPC npc)
        {
            for (var i = 0; i != 4; ++i)
            {
                if (npc.TableAI.SpecialSkill[3 - i] == -1)
                {
                    continue;
                }
                var can = CheckSkillCd(npc.TableAI.SpecialSkill[3 - i]);
                if (can)
                {
                    if (55 == npc.TableAI.Id || 56 == npc.TableAI.Id)//AI为55,56的NPC特殊处理
                    {
                        var isChecked = CheckInitSkillSkill(npc, npc.TableAI.SpecialSkill[3 - i]);
                        if (isChecked)
                        {
                            return npc.TableAI.SpecialSkill[3 - i];
                        }
                    }
                    else
                    {
                        return npc.TableAI.SpecialSkill[3 - i];
                    }
                }
            }
            return npc.TableAI.CommonSkill;
        }

        //战斗
        public override void OnEnterCombat(ObjNPC npc)
        {
            ChangeAi(npc, npc.TableNpc.mAI);
            //先停止移动
            if (npc.IsMoving())
            {
                npc.StopMove();
            }

            //保存下进入战斗时的位置，距离这个位置太远了就要回家
            mBackPosition = npc.GetPosition();

            //获得目标
            var enemy = npc.Scene.FindCharacter(npc.LastEnemyId);
            if (null != enemy)
            {
                npc.TurnFaceTo(enemy.GetPosition());
            }

            //保存普攻数据
            if (-1 != npc.TableAI.CommonSkill)
            {
                mTableNormalSkill = Table.GetSkill(npc.TableAI.CommonSkill);

                //计算普攻技能距离
                mSkillDistance = SkillManager.GetSkillDistance((SkillTargetType) mTableNormalSkill.TargetType,
                    mTableNormalSkill.TargetParam);

                //上来先来个cd，要不攻击者一攻击怪一瞬间怪就反击
                var milliseconds = mTableNormalSkill.Cd <= 0 ? 3000 : mTableNormalSkill.Cd;
                mNextNormalAttackTime = DateTime.Now + TimeSpan.FromMilliseconds(milliseconds);
            }
            CombatTime = 0;

            for (var i = 0; i != 4; ++i)
            {
                if (npc.TableAI.SpecialSkill[i] == -1)
                {
                    continue;
                }
                PushSkillCd(npc, npc.TableAI.SpecialSkill[i], npc.TableAI.InitCd[i]);
            }
        }

        //死亡
        public override void OnEnterDie(ObjNPC npc)
        {
        }

        //回家
        public override void OnEnterGoHome(ObjNPC npc)
        {
            base.OnEnterGoHome(npc);
            npc.MoveTo(npc.BornPosition);
        }

        public override void OnExitCombat(ObjNPC npc)
        {
            npc.RemoveMeFromOtherEnemyList();
            npc.ClearEnemy();
            npc.CleanHatre();
        }

        public override void OnExitDie(ObjNPC npc)
        {
        }

        public override void OnExitGoHome(ObjNPC npc)
        {
        }

        public override void OnTickCombat(ObjNPC npc, float delta)
        {
            CombatTime += 500;
            if (!npc.CanSkill())
            {
                return;
            }
            //判断是否需要状态转换
            var nextAi = CheckStateChange(npc);
            ChangeAi(npc, nextAi);
            //判断敌人是否存在
            var enemy = GetAttackTarget(npc);
            if (null == enemy)
            {
                npc.EnterState(BehaviorState.GoHome);
                return;
            }

            //判断敌人是否有效
            if (enemy.IsDead() || !enemy.Active)
            {
                npc.EnterState(BehaviorState.GoHome);
                return;
            }

            //判断是否太远了
            var dis = (npc.GetPosition() - mBackPosition).Length();
            if (npc.TableNpc.MaxCombatDistance > 0 && dis >= npc.TableNpc.MaxCombatDistance)
            {
                npc.EnterState(BehaviorState.GoHome);
                return;
            }
            var WillskillId = GetCanDoSkill(npc); //npc.NormalSkillId;
            var mTableWillskill = Table.GetSkill(WillskillId);
            if (mTableWillskill == null)
            {
                return;
            }
            switch ((SkillTargetType) mTableWillskill.TargetType)
            {
                case SkillTargetType.SELF:
                {
                    if (DateTime.Now >= mNextNormalAttackTime)
                    {
                        var reCodes = npc.MustSkill(ref WillskillId, npc); //npc.UseSkill(ref skillId, enemy);
                        if (reCodes == ErrorCodes.OK)
                        {
                            PushSkillCd(npc, WillskillId);
                        }
                        return;
                    }
                    mTableWillskill = Table.GetSkill(npc.TableAI.CommonSkill);
                    if (mTableWillskill == null)
                    {
                        return;
                    }
                }
                    break;
                case SkillTargetType.SINGLE:
                    break;
                case SkillTargetType.CIRCLE:
                    break;
                case SkillTargetType.SECTOR:
                    break;
                case SkillTargetType.RECT:
                    break;
                case SkillTargetType.TARGET_CIRCLE:
                    break;
                case SkillTargetType.TARGET_RECT:
                    break;
                case SkillTargetType.TARGET_SECTOR:
                    break;
                default:
                    break;
            }
            mSkillDistance = SkillManager.GetSkillDistance((SkillTargetType) mTableWillskill.TargetType,
                mTableWillskill.TargetParam);
            //跟着敌人打
            if ((npc.GetPosition() - enemy.GetPosition()).Length() > mSkillDistance)
            {
                var diffDis = mSkillDistance - 1.0f;
                if (diffDis < 0)
                {
                    diffDis = 0;
                }
                npc.MoveTo(enemy.GetPosition(), diffDis);
            }
            else
            {
                npc.TurnFaceTo(enemy.GetPosition());
                if (npc.IsMoving())
                {
                    npc.StopMove();
                }
                if (DateTime.Now >= mNextNormalAttackTime)
                {
                    var reCodes = npc.MustSkill(ref WillskillId, enemy); //npc.UseSkill(ref skillId, enemy);
                    if (reCodes == ErrorCodes.OK)
                    {
                        PushSkillCd(npc, WillskillId);
                    }
                }
            }
        }

        public override void OnTickDie(ObjNPC npc, float delta)
        {
        }

        public override void OnTickGoHome(ObjNPC npc, float delta)
        {
            if (!npc.IsMoving())
            {
                if ((npc.BornPosition - npc.GetPosition()).LengthSquared() < 0.5f)
                {
                    npc.EnterState(BehaviorState.Idle);
                }
                else
                {
                    npc.MoveTo(npc.BornPosition);
                }
            }

            if (DateTime.Now > TimeToReset)
            {
                npc.SetPosition(npc.BornPosition);
                npc.EnterState(BehaviorState.Idle);
                TimeToReset = TimeToReset.AddYears(100);
            }
        }

        public override void OnTickIdle(ObjNPC npc, float delta)
        {
            base.OnTickIdle(npc, delta);
        }

        private bool PushSkillCd(ObjNPC npc, int skillId)
        {
            for (var i = 0; i != 4; ++i)
            {
                if (skillId == npc.TableAI.SpecialSkill[i])
                {
                    SkillCd[skillId] = DateTime.Now.AddSeconds(npc.TableAI.Cd[i]);
                    ResetNextNormalAttackTime();
                    return true;
                }
            }
            if (skillId == npc.TableAI.CommonSkill)
            {
                ResetNextNormalAttackTime();
                return true;
            }
            return false;
        }

        private bool PushSkillCd(ObjNPC npc, int skillId, int cdtime)
        {
            for (var i = 0; i != 4; ++i)
            {
                if (skillId == npc.TableAI.SpecialSkill[i])
                {
                    SkillCd[skillId] = DateTime.Now.AddSeconds(cdtime);
                    ResetNextNormalAttackTime();
                    return true;
                }
            }
            if (skillId == npc.TableAI.CommonSkill)
            {
                ResetNextNormalAttackTime();
                return true;
            }
            return false;
        }
    }
}