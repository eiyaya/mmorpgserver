#region using

using System;
using System.Collections.Generic;
using Mono.GameMath;

#endregion

namespace Scene
{
    //巡逻
    public class NPCScript400000 : NPCScriptBase
    {
        //路径点，可以在Scene脚本的OnNpcEnter里判断如果是这个NPC脚本
        public List<Vector2> ListDestination = new List<Vector2>();
        //方向  true是超前
        protected bool mForward = true;
        //路径点索引
        protected int mPtIdx;
        //到某个路点等待时间
        protected DateTime mStayTime = DateTime.Now;
        //每个路径点间的等待时间
        public float WaitTime = 8;

        public override bool IsForceTick()
        {
            return true;
        }

        public override void OnEnterIdle(ObjNPC npc)
        {
            base.OnEnterIdle(npc);
            if (null == npc.TableNpc)
            {
                Logger.Error("NPCScript400000.OnEnterIdle null == npc.TableNpc");
                return;
            }
            var val = npc.TableNpc.ServerParam[0].Split(',');
#if DEBUG
            if (0 != val.Length%2 || val.Length < 4)
            {
                Logger.Error("npc.TableNpc.ServerParam，format must be p1x,p1y,p2x,p2y");
                return;
            }
#endif

            ListDestination.Clear();
            for (var i = 0; i < val.Length;)
            {
                var p = new Vector2(float.Parse(val[i++]), float.Parse(val[i++]));
                ListDestination.Add(p);
            }

            if (ListDestination.Count <= 0)
            {
//没有初始化？
                Logger.Error("NPCScript400000 Destination is invalid,assign a destination");
            }
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

            if (mStayTime > DateTime.Now)
            {
                return;
            }

            if (ListDestination.Count <= 0)
            {
                return;
            }

            if (MoveResult.AlreadyThere == npc.MoveTo(ListDestination[mPtIdx]))
            {
                if (mForward)
                {
                    mPtIdx++;
                    if (mPtIdx >= ListDestination.Count)
                    {
                        mForward = false;
                        mPtIdx = ListDestination.Count - 2;
                    }
                }
                else
                {
                    mPtIdx--;
                    if (mPtIdx < 0)
                    {
                        mForward = true;
                        mPtIdx = 1;
                    }
                }

                mStayTime = DateTime.Now.AddSeconds(WaitTime);
            }
        }
    }
}