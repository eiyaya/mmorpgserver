#region using

using System;
using System.Collections.Generic;
using DataTable;

#endregion

namespace Scene
{
    public class NpcFrozenThrone250000 : NPCScript200000
    {
        private readonly List<int> mAddList = new List<int>
        {
             220020,
             220021,
             220022,
             220023
               //223105,
               //223106,
               //223107,
               //223108
        };

        private readonly ObjCharacter mLastEnemy = null;

        private ObjCharacter GetCanAttack(ObjNPC npc)
        {
            if (mLastEnemy == null || mLastEnemy.IsDead() || !mLastEnemy.Active)
            {
                ObjCharacter temp = null;
                var maxDis = 9999999.0f;
                foreach (var pair in npc.Scene.mObjDict)
                {
                    var t = pair.Value as ObjCharacter;
                    if (t == null)
                    {
                        continue;
                    }
                    if (t.IsDead() || !t.Active)
                    {
                        continue;
                    }
                    if (!mAddList.Contains(t.TypeId))
                    {
                        continue;
                    }
                    var dis = (npc.GetPosition() - t.GetPosition()).LengthSquared();
                    if (dis < maxDis)
                    {
                        temp = t;
                        maxDis = dis;
                    }
                }
                return temp;
            }
            return mLastEnemy;
        }

        //战斗
        public override void OnEnterCombat(ObjNPC npc)
        {
        }

        //死亡
        public override void OnEnterDie(ObjNPC npc)
        {
        }

        //回家
        public override void OnEnterGoHome(ObjNPC npc)
        {
            //base.OnEnterGoHome(npc);
            //npc.MoveTo(npc.BornPosition);
        }

        public override void OnExitDie(ObjNPC npc)
        {
        }

        public override void OnExitGoHome(ObjNPC npc)
        {
        }

        public override void OnTickCombat(ObjNPC npc, float delta)
        {
            if (!npc.CanSkill())
            {
                return;
            }
            //判断敌人是否存在
            var enemy = GetCanAttack(npc);
            if (null == enemy)
            {
                npc.EnterState(BehaviorState.GoHome);
                return;
            }


            var WillskillId = npc.NormalSkillId; //GetCanDoSkill(npc);//npc.NormalSkillId;
            var mTableWillskill = Table.GetSkill(WillskillId);
            if (mTableWillskill == null)
            {
                return;
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
                    //if (reCodes == ErrorCodes.OK)
                    //{
                    //    PushSkillCd(npc, WillskillId);
                    //}
                }
            }
        }

        public override void OnTickDie(ObjNPC npc, float delta)
        {
        }

        public override void OnTickGoHome(ObjNPC npc, float delta)
        {
        }

        //心跳普通
        public override void OnTickIdle(ObjNPC npc, float delta)
        {
            npc.EnterState(BehaviorState.Combat);
        }
    }
}