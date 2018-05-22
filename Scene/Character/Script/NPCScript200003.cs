#region using

using System;
using System.Collections.Generic;
using Mono.GameMath;

#endregion

namespace Scene
{
    //被打不还手，就一直朝目标点走，如果到目标点就把自己消失
    public class NPCScript200003 : NPCScriptBase
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

        public override void OnEnterIdle(ObjNPC npc)
        {
            if (ListDestination.Count <= 0)
            {
//没有初始化？
                Logger.Error("NPCScript200003 Destination is invalid,assign a destination");
            }
        }

        public override void OnRespawn(ObjNPC npc)
        {
            base.OnRespawn(npc);
            mPtIdx = 0;
        }

        public override void OnTickCombat(ObjNPC npc, float delta)
        {
            npc.EnterState(BehaviorState.Idle);
        }

        public override void OnTickIdle(ObjNPC npc, float delta)
        {
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
                npc.Disapeare();
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
    }
}