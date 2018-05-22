#region using

using System.Collections.Generic;
using System.Linq;
using DataTable;
using Scene.Character;

#endregion

namespace Scene
{
    public class AutoPlayer260000 : NPCScriptBase
    {
        public const int DEFAULT_NORMAL_ATTACK_INTERVAL_MILLISECONDS = 1500;
        //下次想攻击的人
        private ulong LastTargetId;
        //技能距离
        private float mSkillDistance;
        private int NextSkill = -1;
        private bool SelectLastTarger;
        private int normalSkill = 0;
        private readonly List<int> skills = new List<int>();

        public int FindFreeSkill(ObjNPC npc, ObjCharacter target)
        {
            foreach (var i in skills)
            {
                var skillId = i;
                ErrorCodes result;
                switch (skillId)
                {
                    case 7:
                        if (npc.BuffList.IsHaveBuffById(7))
                        {
                            continue;
                        }
                        result = npc.CheckUseSkill(ref skillId, npc);
                        break;
                    case 111:
                        if (npc.BuffList.IsHaveBuffById(114))
                        {
                            continue;
                        }
                        result = npc.CheckUseSkill(ref skillId, npc);
                        break;
                    case 209:
                        if (npc.BuffList.IsHaveBuffById(211))
                        {
                            continue;
                        }
                        result = npc.CheckUseSkill(ref skillId, npc);
                        break;
                    case 208:
                        if (npc.BuffList.IsHaveBuffById(210))
                        {
                            continue;
                        }
                        result = npc.CheckUseSkill(ref skillId, npc);
                        break;
                    default:
                        result = npc.CheckUseSkill(ref skillId, target);
                        break;
                }
                if (result == ErrorCodes.OK || result == ErrorCodes.Error_SkillDistance)
                {
                    return i;
                }
            }
            return normalSkill;
        }

        public ObjCharacter GetEnemyCharacter(ObjNPC npc)
        {
            //优先寻找我上次的攻击目标
            ObjCharacter enemy;
            if (SelectLastTarger)
            {
                enemy = npc.Scene.FindCharacter(LastTargetId);
                if (enemy != null && enemy.Active && !enemy.IsDead())
                {
                    return enemy;
                }
            }
            //优先攻击攻击自己的敌人
            enemy = npc.Scene.FindCharacter(npc.LastEnemyId);
            if (enemy != null && enemy.Active && !enemy.IsDead())
            {
                return enemy;
            }
            return null;
        }

        private static void IsHaveBuff(ObjNPC npc)
        {
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
            //mBackPosition = npc.GetPosition();

            var enemyTarget = GetEnemyCharacter(npc);
            //获得目标
            if (null != enemyTarget)
            {
                npc.TurnFaceTo(enemyTarget.GetPosition());
                LastTargetId = enemyTarget.ObjId;
                SelectLastTarger = true;
            }
            SkillSort(npc);

            npc.SetSkill(1000);
        }

        //死亡
        public override void OnEnterDie(ObjNPC npc)
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
            retinue.Owner.DeleteBuff(retinue.Buff, eCleanBuffType.RetinueDie);
        }

        //回家
        public override void OnEnterGoHome(ObjNPC npc)
        {
            base.OnEnterGoHome(npc);
            var retinue = npc as ObjRetinue;
            if (retinue == null)
            {
                return;
            }
            if (retinue.Owner == null)
            {
                retinue.Scene.LeaveScene(retinue);
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

            var enemyTarget = GetEnemyCharacter(npc);
            if (enemyTarget == null)
            {
                npc.EnterState(BehaviorState.Idle);
                return;
            }
            if (!npc.CanMove())
            {
                return;
            }
            NextSkill = FindFreeSkill(npc, enemyTarget);
            if (NextSkill == -1)
            {
                return;
            }
            var tbSkill = Table.GetSkill(NextSkill);
            if (tbSkill == null)
            {
                return;
            }

            if ((SkillTargetType)tbSkill.TargetType == SkillTargetType.SELF ||
                (eCampType)tbSkill.CampType == eCampType.Team)
            {
                if (npc.IsMoving())
                {
                    npc.StopMove();
                }
                if (npc.CanSkill())
                {
                    LastTargetId = enemyTarget.ObjId;
                    SelectLastTarger = true;
                    npc.TurnFaceTo(enemyTarget.GetPosition());
                    npc.UseSkill(ref NextSkill, npc);
                }
                return;
            }


            mSkillDistance = SkillManager.GetSkillDistance((SkillTargetType)tbSkill.TargetType, tbSkill.TargetParam);
            //跟着敌人打
            if ((npc.GetPosition() - enemyTarget.GetPosition()).Length() > mSkillDistance)
            {
                npc.MoveTo(enemyTarget.GetPosition(), mSkillDistance - 0.1f);
            }
            else
            {
                if (npc.IsMoving())
                {
                    npc.StopMove();
                }
                if (npc.CanSkill())
                {
                    LastTargetId = enemyTarget.ObjId;
                    SelectLastTarger = true;
                    npc.TurnFaceTo(enemyTarget.GetPosition());
                    npc.UseSkill(ref NextSkill, enemyTarget);
                }
                
            }
        }

        public override void OnTickDie(ObjNPC npc, float delta)
        {
        }

        public override void OnTickGoHome(ObjNPC npc, float delta)
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
            if (!npc.IsMoving())
            {
                if ((npc.BornPosition - npc.GetPosition()).LengthSquared() < 1.5f)
                {
                    npc.EnterState(BehaviorState.Idle);
                }
                else
                {
                    npc.MoveTo(retinue.Owner.GetPosition(), 1);
                }
            }
        }

        public override void OnTickIdle(ObjNPC npc, float delta)
        {
            var target = npc.ScanEnemy(50);
            if (target != null)
            {
                npc.EnterState(BehaviorState.Combat);
                npc.PushHatre(target, 1);
                npc.AddEnemy(target.ObjId);
                target.AddEnemy(npc.ObjId);
            }
        }

        public void SkillSort(ObjNPC npc)
        {
            
            for (int i = 1; i < npc.TableCharacter.InitSkill.Length; i++)
            {
                if (npc.TableCharacter.InitSkill[i] >= 0)
                {
                    skills.Add(npc.TableCharacter.InitSkill[i]);                  
                }
            }
            normalSkill = npc.TableCharacter.InitSkill[0];
        }
    }
}