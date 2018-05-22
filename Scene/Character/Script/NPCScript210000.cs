#region using

using System;
using System.Collections.Generic;
using DataTable;
using Mono.GameMath;
using Shared;

#endregion

namespace Scene
{
    public class NPCScript210000 : NPCScriptBase
    {
        public const int DEFAULT_NORMAL_ATTACK_INTERVAL_MILLISECONDS = 1500;
        //下次普攻时间
        private DateTime lastNoMoveIdeTime = DateTime.Now.AddSeconds(3);
        //下次想攻击的人
        private ulong LastTargetId;
        //下次普攻时间
        private DateTime mNextNormalAttackTime;
        //技能距离
        private float mSkillDistance;
        //普攻表格
        private SkillRecord mTableNormalSkill;
        private readonly Dictionary<int, DateTime> SkillCd = new Dictionary<int, DateTime>();

	    public override bool IsForceTick()
	    {
		    return true;
	    }

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

        public ObjCharacter GetEnemyCharacter(ObjRetinue retinue)
        {
            //优先寻找我上次的攻击目标
            var enemy = retinue.Scene.FindCharacter(LastTargetId);
            var pos = retinue.Owner.GetPosition();
            if (enemy != null && enemy.Active && !enemy.IsDead())
            {
                if ((enemy.GetPosition() - pos).LengthSquared() < 55)
                {
                    return enemy;
                }
            }
            //优先寻找玩家的当前敌人
            enemy = retinue.Owner.Skill.EnemyTarget;
            if (enemy != null && enemy.Active && !enemy.IsDead())
            {
                if ((enemy.GetPosition() - pos).LengthSquared() < 55)
                {
                    return enemy;
                }
            }
            //优先攻击攻击玩家的敌人
            enemy = retinue.Scene.FindCharacter(retinue.Owner.LastEnemyId);
            if (enemy != null && enemy.Active && !enemy.IsDead())
            {
                if ((enemy.GetPosition() - pos).LengthSquared() < 55)
                {
                    return enemy;
                }
            }
            //优先攻击攻击自己的敌人
            enemy = retinue.Scene.FindCharacter(retinue.LastEnemyId);
            if (enemy != null && enemy.Active && !enemy.IsDead())
            {
                if ((enemy.GetPosition() - pos).LengthSquared() < 55)
                {
                    return enemy;
                }
            }
            return null;
        }

        //战斗
        public override void OnEnterCombat(ObjNPC npc)
        {
            var retinue = npc as ObjRetinue;
            if (retinue == null)
            {
                return;
            }
            if (retinue.Owner == null)
            {
                return;
            }

            //先停止移动
            if (npc.IsMoving())
            {
                npc.StopMove();
            }
            mTableNormalSkill = Table.GetSkill(npc.TableAI.CommonSkill);
            //保存下进入战斗时的位置，距离这个位置太远了就要回家
            //mBackPosition = npc.GetPosition();

            var enemyTarget = retinue.Owner.Skill.EnemyTarget;
            //获得目标
            //npc.Scene.FindCharacter(npc.LastEnemyId);
            if (null != enemyTarget)
            {
                npc.TurnFaceTo(enemyTarget.GetPosition());
                LastTargetId = enemyTarget.ObjId;
            }

            //保存普攻数据
            if (-1 != npc.NormalSkillId)
            {
                //mTableNormalSkill = Table.GetSkill(npc.NormalSkillId);

                //计算普攻技能距离
                //mSkillDistance = SkillManager.GetSkillDistance((SkillTargetType)mTableNormalSkill.TargetType, mTableNormalSkill.TargetParam);

                //上来先来个cd，要不攻击者一攻击怪一瞬间怪就反击
                npc.SetSkill(1000);
                //int milliseconds = mTableNormalSkill.Cd <= 0 ? 3000 : mTableNormalSkill.Cd;
                //mNextNormalAttackTime = DateTime.Now + TimeSpan.FromMilliseconds(milliseconds);
            }
        }

        //死亡
        public override void OnEnterDie(ObjNPC npc)
        {
//             ObjRetinue retinue = npc as ObjRetinue;
//             if (retinue == null) return;
//             if (retinue.Owner != null)
//             {
//                 //retinue.Owner.DeleteBuff(retinue.Buff, eCleanBuffType.RetinueDie);
//             }
            //retinue.Scene.LeaveScene(retinue);
        }

        //回家
        public override void OnEnterGoHome(ObjNPC npc)
        {
            //base.OnEnterGoHome(npc);
            if (npc.Scene == null)
            {
                return;
            }
            var retinue = npc as ObjRetinue;
            if (retinue == null)
            {
                return;
            }
            if (retinue.Owner == null)
            {
                retinue.Scene.LeaveScene(retinue);
                return;
            }
            npc.MoveTo(retinue.Owner.GetPosition(), 1);
        }

        //休闲O
        public override void OnEnterIdle(ObjNPC npc)
        {
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

        public override void OnTickCombat(ObjNPC npc, float delta)
        {
            var retinue = npc as ObjRetinue;
            if (retinue == null)
            {
                return;
            }
            if (retinue.Owner == null || retinue.Owner.Scene == null)
            {
                retinue.Scene.LeaveScene(retinue);
                return;
            }
            var myPos = retinue.GetPosition();
            var ownerPos = retinue.Owner.GetPosition();
            if (mNextNormalAttackTime > DateTime.Now)
            {
                return;
            }
            var distance = (myPos - ownerPos).LengthSquared();
            //if (distance > 400)
            //{
            //    var diff = GetRandomPostion();
            //    diff += ownerPos;
            //    npc.SetCheckPostion(diff);
            //    //retinue.MoveTo(ownerPos, 2);
            //    npc.EnterState(BehaviorState.GoHome);
            //    return;
            //}
            if (distance > 64)
            {
                retinue.MoveTo(ownerPos, 2);
                return;
                //var p = myPos + Vector2.Normalize(ownerPos - myPos) * 2;
                //retinue.MoveTo(p);
            }
            var tag = retinue.Owner.Scene.GetObstacleValue(ownerPos.X, ownerPos.Y);
            if (tag == SceneObstacle.ObstacleValue.Walkable) //自身在安全区禁止攻击
                return;

            var enemyTarget = GetEnemyCharacter(retinue);
            if (enemyTarget == null)
            {
                npc.EnterState(BehaviorState.GoHome);
                return;
            }
            if(enemyTarget.GetObjType() == ObjType.PLAYER)
            {
                tag = enemyTarget.Scene.GetObstacleValue(ownerPos.X, ownerPos.Y);
                if (tag == SceneObstacle.ObstacleValue.Walkable) //对方在安全区禁止攻击
                    return;
            }

            LastTargetId = enemyTarget.ObjId;


            var WillskillId = GetCanDoSkill(npc); //npc.NormalSkillId;
            var mTableWillskill = Table.GetSkill(WillskillId);
            if (mTableWillskill == null)
            {
                return;
            }
            mSkillDistance = SkillManager.GetSkillDistance((SkillTargetType) mTableWillskill.TargetType,
                mTableWillskill.TargetParam);
            //跟着敌人打
            if ((npc.GetPosition() - enemyTarget.GetPosition()).Length() > mSkillDistance)
            {
                npc.MoveTo(enemyTarget.GetPosition(), mSkillDistance - 0.3f);
            }
            else
            {
                if (npc.IsMoving())
                {
                    npc.StopMove();
                }
                if (npc.UseSkill(ref WillskillId, enemyTarget) == ErrorCodes.OK)
                {
                    //int cd = mTableNormalSkill.Cd;
                    //if (cd < mTableNormalSkill.CommonCd)
                    //{
                    //    cd = mTableNormalSkill.CommonCd;
                    //}
                    //int milliseconds = cd <= 0 ? DEFAULT_NORMAL_ATTACK_INTERVAL_MILLISECONDS : cd;
                    //mNextNormalAttackTime = DateTime.Now + TimeSpan.FromMilliseconds(milliseconds);
                    PushSkillCd(npc, WillskillId);
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
                npc.EnterState(BehaviorState.Idle);
            }
        }

        public override void OnTickIdle(ObjNPC npc, float delta)
        {
            var retinue = npc as ObjRetinue;
            if (retinue == null)
            {
                return;
            }
            if (retinue.Owner == null)
            {
                retinue.Scene.LeaveScene(retinue);
            }
            var myPos = retinue.GetPosition();
            var ownerPos = retinue.Owner.GetPosition();
            var distance = (myPos - ownerPos).LengthSquared();
            //if (distance > 400)
            //{
            //    var diff = GetRandomPostion();
            //    diff += ownerPos;
            //    npc.SetCheckPostion(diff);
            //    //retinue.MoveTo(ownerPos, 2);
            //    npc.EnterState(BehaviorState.GoHome);
            //    return;
            //}
            if (distance > 9)
            {
                retinue.MoveTo(ownerPos, 2);
                lastNoMoveIdeTime = DateTime.Now.AddSeconds(MyRandom.Random(3, 7));
                //var p = myPos + Vector2.Normalize(ownerPos - myPos) * 2;
                //retinue.MoveTo(p);
            }
            else if (lastNoMoveIdeTime < DateTime.Now)
            {
                var diff = Utility.GetRandomPostion();
                retinue.MoveTo(new Vector2(ownerPos.X + diff.X, ownerPos.Y + diff.Y), 0.2f);
                lastNoMoveIdeTime = DateTime.Now.AddSeconds(MyRandom.Random(3, 7));
            }
        }

        private bool PushSkillCd(ObjNPC npc, int skillId)
        {
            for (var i = 0; i != 4; ++i)
            {
                if (skillId == npc.TableAI.SpecialSkill[i])
                {
                    var cdTime = npc.TableAI.Cd[i];
                    SkillCd[skillId] = DateTime.Now.AddSeconds(cdTime);
                    var tbSkill = Table.GetSkill(skillId);
                    var milliseconds = tbSkill.CommonCd <= 0
                        ? DEFAULT_NORMAL_ATTACK_INTERVAL_MILLISECONDS
                        : mTableNormalSkill.CommonCd;
                    mNextNormalAttackTime = DateTime.Now + TimeSpan.FromMilliseconds(milliseconds);
                    return true;
                }
            }
            if (skillId == npc.TableAI.CommonSkill)
            {
                var milliseconds = DEFAULT_NORMAL_ATTACK_INTERVAL_MILLISECONDS;
                if (null != mTableNormalSkill && 0 < mTableNormalSkill.Cd)
                {
                    milliseconds = mTableNormalSkill.Cd;
                }
                mNextNormalAttackTime = DateTime.Now + TimeSpan.FromMilliseconds(milliseconds);
                return true;
            }
            return false;
        }
    }
}