#region using

using System;
using System.Collections.Generic;
using DataTable;

#endregion

namespace Scene
{
    public class NPCScript300000 : NPCScriptBase
    {
        public const int DEFAULT_NORMAL_ATTACK_INTERVAL_MILLISECONDS = 1500;
        public const float MAX_NO_MOVE_TIME = 10;
        private int CombatTime;
        //下次普攻时间
        protected DateTime mNextNormalAttackTime;
        //不能移动时间
        protected float mNoMoveTime = 0;
        //技能距离
        protected float mSkillDistance;
        //普攻表格
        protected SkillRecord mTableNormalSkill;
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

        //生成下一次普攻时间
        protected virtual void GenNextNormalAttackTime(int milliseconds = -1)
        {
            if (milliseconds <= 0)
            {
                if (null != mTableNormalSkill)
                {
                    milliseconds = Math.Min(DEFAULT_NORMAL_ATTACK_INTERVAL_MILLISECONDS, mTableNormalSkill.Cd);
                }
                if (milliseconds <= 0)
                {
                    milliseconds = DEFAULT_NORMAL_ATTACK_INTERVAL_MILLISECONDS;
                }
            }

            mNextNormalAttackTime = DateTime.Now + TimeSpan.FromMilliseconds(milliseconds);
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
                    return npc.TableAI.SpecialSkill[3 - i];
                }
            }
            return npc.TableAI.CommonSkill;
        }

        //战斗
        public override void OnEnterCombat(ObjNPC npc)
        {
            ChangeAi(npc, npc.TableNpc.mAI);
            //获得目标
            var enemy = npc.Scene.FindCharacter(npc.LastEnemyId);
            if (null != enemy)
            {
                npc.TurnFaceTo(enemy.GetPosition());
            }

            npc.BroadcastDirection();

            //保存普攻数据
            if (-1 != npc.TableAI.CommonSkill)
            {
                mTableNormalSkill = Table.GetSkill(npc.TableAI.CommonSkill);

                //计算普攻技能距离
                mSkillDistance = SkillManager.GetSkillDistance((SkillTargetType) mTableNormalSkill.TargetType,
                    mTableNormalSkill.TargetParam);
            }
            CombatTime = 0;

            for (var i = 0; i != 4; ++i)
            {
                if (npc.TableAI.SpecialSkill[i] == -1)
                {
                    continue;
                }
                PushSkillCd(npc, npc.TableAI.SpecialSkill[i], 0);
            }
            mNextNormalAttackTime = DateTime.Now;
        }

        //死亡
        public override void OnEnterDie(ObjNPC npc)
        {
        }

        //回家
        public override void OnEnterGoHome(ObjNPC npc)
        {
        }

        //休闲
        public override void OnEnterIdle(ObjNPC npc)
        {
        }

        public override void OnExitCombat(ObjNPC npc)
        {
            //npc.CleanHatre();
        }

        public override void OnExitDie(ObjNPC npc)
        {
        }

        public override void OnExitGoHome(ObjNPC npc)
        {
        }

        public override void OnExitIdle(ObjNPC npc)
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
            if (null == enemy || enemy.IsDead() || !enemy.Active)
            {
                npc.EnterState(BehaviorState.Idle);
                npc.SetDirection(npc.BornDirection);
                npc.BroadcastDirection();
                npc.RemoveMeFromOtherEnemyList();
                npc.ClearEnemy();
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
                npc.SetDirection(npc.BornDirection);
                npc.BroadcastDirection();
            }
            else
            {
                npc.TurnFaceTo(enemy.GetPosition());

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
            npc.EnterState(BehaviorState.Idle);
        }

        public override void OnTickIdle(ObjNPC npc, float delta)
        {
            if (npc.Scene == null)
            {
                return;
            }

            if (npc.IsDead())
            {
                return;
            }

            if (npc.IsAggressive())
            {
                if (npc.TableNpc.ViewDistance > 0)
                {
                    var target = npc.ScanEnemy((float) npc.TableNpc.ViewDistance);
                    if (target != null)
                    {
                        npc.EnterState(BehaviorState.Combat);
                        npc.PushHatre(target, 1);
                        npc.AddEnemy(target.ObjId);
                        target.AddEnemy(npc.ObjId);
                    }
                }
            }
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

        protected void ResetNextNormalAttackTime()
        {
            var milliseconds = DEFAULT_NORMAL_ATTACK_INTERVAL_MILLISECONDS;
            if (null != mTableNormalSkill && 0 < mTableNormalSkill.Cd)
            {
                milliseconds = mTableNormalSkill.Cd;
            }
            mNextNormalAttackTime = DateTime.Now + TimeSpan.FromMilliseconds(milliseconds);
        }
    }
}