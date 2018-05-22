#region using

using System;
using DataTable;

#endregion

namespace Scene
{
    //可以主动工具怪
    public class NPCScript200005 : NPCScript200000
    {
        public override void OnEnterCombat(ObjNPC npc)
        {
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
                mSkillDistance =
                    SkillManager.GetSkillDistance((SkillTargetType) mTableNormalSkill.TargetType,
                        mTableNormalSkill.TargetParam) - 0.8f;
                mSkillDistance = Math.Max(mSkillDistance, 0);
                //上来先来个cd，要不攻击者一攻击怪一瞬间怪就反击
                //GenNextNormalAttackTime();
            }

            mNoMoveTime = 0;
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
                        npc.Seperate(enemy, mSkillDistance/3);
                        var skillId = npc.NormalSkillId;
                        npc.UseSkill(ref skillId, enemy);
                        GenNextNormalAttackTime();
                        mNoMoveTime = 0; //攻击了一次就把自己站在原地的累计时间清空
                    }
                }
                else
                {
                    var targetPos = npc.CalculatePostionToEnemy(enemy, mSkillDistance/3);
                    if (MoveResult.CannotReach == npc.MoveTo(targetPos))
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
                    var target = ScanEnemy(npc, (float) npc.TableNpc.ViewDistance);
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

        protected ObjCharacter ScanEnemy(ObjNPC npc, float distance)
        {
            if (null == npc)
            {
                return null;
            }

            if (null == npc.Zone)
            {
                return null;
            }

            var sq = distance*distance;
            ObjCharacter target = null;
            var objs = npc.Zone.EnumAllVisibleObj();
            foreach (var obj in objs)
            {
                if (null == obj)
                {
                    continue;
                }

                if (obj.ObjId == npc.ObjId)
                {
                    continue;
                }

                if (!obj.IsCharacter())
                {
                    continue;
                }

                var character = obj as ObjCharacter;
                if (null == character)
                {
                    continue;
                }

                if (character.IsDead())
                {
                    continue;
                }

                if (!npc.IsMyEnemy(character))
                {
                    continue;
                }

                var temp = (npc.GetPosition() - character.GetPosition()).LengthSquared();

                if (temp <= sq)
                {
                    target = character;
                    sq = temp;
                }
            }

            return target;
        }
    }
}