#region using

using System;
using System.Collections.Generic;
using DataTable;

#endregion

namespace Scene
{
    public class NpcMieShi250100 : NPCScript200000
    {

        /*
                private readonly List<int> mAddList = new List<int>
                {
                    MieShiWar.NPCTower1,
                    MieShiWar.NPCTower2,
                    MieShiWar.NPCTower3,
                    MieShiWar.NPCTower4,
                    MieShiWar.NPCTower5,
                    MieShiWar.NPCTower6,
                    MieShiWar.NPCChancel
                };*/
        private bool bAIChange = false;
        private int PatrolTime = 0;
        private const int MieShiBuildingAI = 100505;
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
                    if (t == null ||npc.Equals(t) || npc.GetCamp() ==t.GetCamp())
                    {
                        continue;
                    }
                    if (t.IsDead() || !t.Active)
                    {
                        continue;
                    }
                    /**if (!mAddList.Contains(t.TypeId))
                    {
                        continue;
                    }**/
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
            if (npc.TableAI.Id != MieShiBuildingAI)
            {
                base.OnEnterCombat(npc);
            }
        }
        public override void OnEnterIdle(ObjNPC npc)
        {
            if (npc.TableAI.Id != MieShiBuildingAI)
            {
                base.OnEnterIdle(npc);
            }
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
            if (npc.TableAI.Id != MieShiBuildingAI)
            {
                base.OnEnterGoHome(npc);
            }
        }

        public override void OnExitDie(ObjNPC npc)
        {
        }

        public override void OnExitGoHome(ObjNPC npc)
        {
        }

        public override void OnExitCombat(ObjNPC npc)
        {
            if (npc.TableAI.Id != MieShiBuildingAI)
            {
                base.OnExitCombat(npc);
            }
        }

        public override void OnTickCombat(ObjNPC npc, float delta)
        {
            if (npc.TableAI.Id == MieShiBuildingAI)
            {
                if (!npc.CanSkill())
                {
                    return;
                }

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

                mSkillDistance = SkillManager.GetSkillDistance((SkillTargetType)mTableWillskill.TargetType,
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
            else
            {
                PatrolTime += 500;
                //判断是否需要状态转换
                if (!bAIChange)
                {
                    var nextAi = CheckToStateChange(npc);
                    if (nextAi > -1)
                    {

                        Logger.Warn("======================ChangeAi============================[179] {0}",npc.mTypeId);
                        ChangeAi(npc, nextAi);
                        bAIChange = true;
                        return;
                    }
                }
                base.OnTickCombat(npc, delta);
            }
        }

        public override void OnTickDie(ObjNPC npc, float delta)
        {
        }

        public override void OnTickGoHome(ObjNPC npc, float delta)
        {
            if (npc.TableAI.Id != MieShiBuildingAI)
            {
                PatrolTime += 500;
                //判断是否需要状态转换
                if (!bAIChange)
                {
                    var nextAi = CheckToStateChange(npc);
                    if (nextAi > -1)
                    {
                        Logger.Warn("======================ChangeAi============================[205] {0}", npc.mTypeId);
                        ChangeAi(npc, nextAi);
                        bAIChange = true;
                        return;
                    }
                }
                base.OnTickGoHome(npc, delta);
            }
			Logger.Error("OnTickGoHome[{0}][{1}]", npc.TableNpc.Name, npc.TableNpc.Id);
        }

        //心跳普通
        public override void OnTickIdle(ObjNPC npc, float delta)
        {
            if (npc.TableAI.Id == MieShiBuildingAI)
            {
                npc.EnterState(BehaviorState.Combat);
            }
            else
            {
                PatrolTime += 500;
                //判断是否需要状态转换
                if (!bAIChange)
                {
                    var nextAi = CheckToStateChange(npc);
                    if (nextAi > -1)
                    {
                        Logger.Warn("======================ChangeAi============================[232] {0}", npc.mTypeId);
                        ChangeAi(npc, nextAi);
                        bAIChange = true;
                        return;
                    }
                }
                base.OnTickIdle(npc, delta);
            }
        }

        public int CheckToStateChange(ObjNPC npc)
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
                                    if ((float)npc.Attr.GetDataValue(eAttributeType.HpNow) /
                                        npc.Attr.GetDataValue(eAttributeType.HpMax) < (float)npc.TableAI.Param / 10000)
                                    {
                                        return npc.TableAI.NextAI;
                                    }
                                }
                                break;
                            case 3: //高于百分比
                                {
                                    if ((float)npc.Attr.GetDataValue(eAttributeType.HpNow) /
                                        npc.Attr.GetDataValue(eAttributeType.HpMax) > (float)npc.TableAI.Param / 10000)
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
                                    if (PatrolTime / 1000 < npc.TableAI.Param)
                                    {
                                        return npc.TableAI.NextAI;
                                    }
                                }
                                break;
                            case 1: //高于绝对值
                                {
                                    if (PatrolTime / 1000 > npc.TableAI.Param)
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
    }
}