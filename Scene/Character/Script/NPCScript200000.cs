#region using

using System;
using DataTable;
using Mono.GameMath;

#endregion

namespace Scene
{
    public class NPCScript200000 : NPCScriptBase
    {
        public const int DEFAULT_NORMAL_ATTACK_INTERVAL_MILLISECONDS = 900;
        public const float MAX_NO_MOVE_TIME = 10;
        //进入战斗的位置(距离过远就回家)
        protected Vector2 mBackPosition;
        //下次普攻时间
        protected DateTime mNextNormalAttackTime;
        //不能移动时间
        protected float mNoMoveTime;
        //技能距离
        protected float mSkillDistance;
        //普攻表格
        protected SkillRecord mTableNormalSkill;
        // 30 秒后如果还没有回去，强制拉回去，并开始idle
        protected DateTime TimeToReset;
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

        //战斗
        public override void OnEnterCombat(ObjNPC npc)
        {
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

            npc.BroadcastDirection();

            //保存普攻数据
            if (-1 != npc.NormalSkillId)
            {
                mTableNormalSkill = Table.GetSkill(npc.NormalSkillId);

                //计算普攻技能距离
                mSkillDistance = SkillManager.GetSkillDistance((SkillTargetType) mTableNormalSkill.TargetType,
                    mTableNormalSkill.TargetParam);

                //上来先来个cd，要不攻击者一攻击怪一瞬间怪就反击
                GenNextNormalAttackTime();
            }

            mNoMoveTime = 0;
        }

        //死亡
        public override void OnEnterDie(ObjNPC npc)
        {
        }

        //回家
        public override void OnEnterGoHome(ObjNPC npc)
        {
            base.OnEnterGoHome(npc);
            if ((npc.GetPosition() - npc.BornPosition).LengthSquared() < 0.01f)
            {
                npc.EnterState(BehaviorState.Idle);
                return;
            }
            // 30 秒后如果还没有回去，强制拉回去，并开始idle
            TimeToReset = DateTime.Now.AddSeconds(30);
            npc.MoveTo(npc.BornPosition);
        }

        //休闲
        public override void OnEnterIdle(ObjNPC npc)
        {
            if (npc.LastState == BehaviorState.GoHome)
            {
                npc.SetDirection(npc.BornDirection);
                npc.BroadcastDirection();
            }
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

        public override void OnExitIdle(ObjNPC npc)
        {
        }

        private void TryToCallPartner(ObjNPC npc, ObjCharacter enemy, float distSqr = 100)
        {
            try
            {
                if (enemy != null)
                {
                    foreach (ObjBase o in npc.Zone.EnumAllVisibleObj())
                    {
                        try
                        {
                            if (!(o is ObjNPC))
                            {
                                continue;
                            }

                            var obj = o as ObjNPC;

                            if (!obj.Active)
                            {
                                continue;
                            }

                            if (obj.CurrentState != BehaviorState.Idle)
                            {
                                continue;
                            }

                            if (obj.GetObjType() != ObjType.NPC)
                            {
                                continue;
                            }

                            if (!obj.IsVisibleTo(enemy))
                            {
                                continue;
                            }

                            if (obj.tbSceneNpc == null || npc.tbSceneNpc == null)
                            {
                                continue;
                            }

                            if (obj.tbSceneNpc.ChouHenGroupId != npc.tbSceneNpc.ChouHenGroupId)
                            {
                                continue;
                            }

                            if (obj.mCamp != npc.mCamp)
                            {
                                continue;
                            }

                            if((obj.GetPosition() - enemy.GetPosition()).LengthSquared() > distSqr)
                                continue;

                            obj.EnterState(BehaviorState.Combat);
                            obj.PushHatre(enemy, 1);
                            obj.AddEnemy(enemy.ObjId);
                            enemy.AddEnemy(obj.ObjId);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch { }
        }

        public override void OnTickCombat(ObjNPC npc, float delta)
        {
            ObjCharacter enemy = null;

            //判断敌人是否存在
            if (npc.TableAI == null)
            {
                enemy = npc.GetMaxHatre();
            }
            else
            {
                enemy = GetAttackTarget(npc);
            }

            //判断敌人是否存在

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

            //如果有普通
            if (null != mTableNormalSkill)
            {
                //跟着敌人打
                if ((npc.GetPosition() - enemy.GetPosition()).Length() <= mSkillDistance)
                {
                    npc.TurnFaceTo(enemy.GetPosition());
                    if (npc.IsMoving())
                    {
                        GenNextNormalAttackTime(100);
                        npc.StopMove();
                    }
                    if (DateTime.Now >= mNextNormalAttackTime)
                    {
                        npc.Seperate(enemy, mSkillDistance);
                        var skillId = npc.NormalSkillId;
                        npc.UseSkill(ref skillId, enemy);
                        GenNextNormalAttackTime();
                        mNoMoveTime = 0; //攻击了一次就把自己站在原地的累计时间清空
                    }
                }
                else
                {
                    var targetPos = enemy.GetPosition();
                    if (MoveResult.CannotReach == npc.MoveTo(targetPos, mSkillDistance - 0.5f))
                    {
                        //如果长时间无法到达目的地说明卡怪了， 就回去
                        mNoMoveTime += delta;
                        if (mNoMoveTime >= MAX_NO_MOVE_TIME)
                        {
                            npc.EnterState(BehaviorState.GoHome);
                        }
                    }
                    else
                    {
                        mNoMoveTime = 0;
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
                if ((npc.BornPosition - npc.GetPosition()).LengthSquared() < 1.5f)
                {
                    npc.EnterState(BehaviorState.Idle);
                }
                else
                {
                    npc.MoveTo(npc.BornPosition, 1);
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

                        // 搜索仇恨组
                        if(npc.tbSceneNpc != null && npc.tbSceneNpc.ChouHenGroupId != -1)
                            TryToCallPartner(npc, target, 2000);
                    }
                }
            }
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