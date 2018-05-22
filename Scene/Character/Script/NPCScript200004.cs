#region using

using System;
using System.Collections.Generic;
using DataTable;
using Mono.GameMath;

#endregion

namespace Scene
{
    //一直朝目标点走，如果被攻击就一直打目标，超出范围了就继续回到被打的地方继续走
    public class NPCScript200004 : NPCScript200000
    {
        //路径点，可以在Scene脚本的OnNpcEnter里判断如果是这个NPC脚本
        public List<Vector2> ListDestination = new List<Vector2>();
        //路径点索引
        protected int mPtIdx;
        //等待时间
        protected DateTime mTime = DateTime.Now;
        //每个路径点间的等待时间
        public float WaitTime = 0;

        public override bool IsForceTick()
        {
            return true;
        }

        public override void OnEnterCombat(ObjNPC npc)
        {
            npc.BornPosition = npc.GetPosition();
            npc.BornDirection = npc.GetDirection();

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

        public override void OnEnterIdle(ObjNPC npc)
        {
            if (ListDestination.Count <= 0)
            {
//没有初始化？
                Logger.Error("NPCScript200004 Destination is invalid,assign a destination");
            }
        }

        public override void OnRespawn(ObjNPC npc)
        {
            base.OnRespawn(npc);
            mPtIdx = 0;
        }

        public override void OnTickIdle(ObjNPC npc, float delta)
        {
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
                        return;
                    }
                }
            }

            if (!npc.CanMove())
            {
                return;
            }

            if (npc.IsMoving())
            {
                return;
            }

            if (mPtIdx < 0 || mPtIdx >= ListDestination.Count)
            {
                //npc.Disapeare();
                return;
            }

            if (mTime > DateTime.Now)
            {
                return;
            }

            if (MoveResult.AlreadyThere == npc.MoveTo(ListDestination[mPtIdx]))
            {
                mPtIdx++;
                mTime = DateTime.Now.AddSeconds(WaitTime);
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