#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataContract;
using Scorpion;
using Mono.GameMath;
using NLog;
using Shared;
using DataTable;
using System.Diagnostics;
#endregion

namespace Scene
{
    public class ClimbingTower : DungeonScene
    {
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #region 接口
        public override void OnCreate()
        {
            base.OnCreate();
            State = eDungeonState.Start;
            var waitMin = mFubenRecord == null ? 10 : mFubenRecord.TimeLimitMinutes;
            StartTimer(eDungeonTimerType.WaitEnd, DateTime.Now.AddMinutes(waitMin), TimeOverEnd);
        }
        public override bool CanPk()
        {
            return false;
        }
        public override void OnNpcDie(ObjNPC npc, ulong characterId = 0)
        {
            
            base.OnNpcDie(npc, characterId);
            if (npc == null || npc.GetCamp() != 2)
            {
                return;
            }
            if (--mMonsterCount == 0)
            {
                OnWin();
            }
            else
            {
                foreach (var objPlayer in EnumAllPlayer())
                {
                    objPlayer.Proxy.BroadcastSceneMonsterCount(mMonsterCount);
                }
            }
            BroadCastMonsterCount();
        }
        #endregion

        public void OnNpcBehaviourChanged(ObjNPC npc, BehaviorState newState, BehaviorState oldState)
        {
            //状态监听，记录是否有异常
            if (BehaviorState.Combat == oldState && newState != BehaviorState.Die)
            {
                var enemy = npc.Script.GetAttackTarget(npc);
                if (null != enemy && !enemy.IsDead())
                {// 玩家活着时，npc状态异常
                    Logger.Error("Climbing Tower npc state error!!! {0}", newState);
                    // 强制设置成idle
                    if (BehaviorState.Idle != newState)
                    {
                        npc.EnterState(BehaviorState.Idle);
                    }
                }
            }
        }

        public override void OnNpcEnter(ObjNPC npc)
        {
            if (null != npc && npc.GetCamp() == 2)
            {
                mMonsterCount++;
                BroadCastMonsterCount();

                npc.OnBehaviourChangeCallback += OnNpcBehaviourChanged;
            }
        }
        public void OnWin()
        {
            var result = new FubenResult();
            result.CompleteType = (int)eDungeonCompleteType.Success;
            CompleteToAll(result);
        }

        public override void OnNpcRespawn(ObjNPC npc)
        {
            if (null != npc && npc.GetCamp() == 2)
            {
                mMonsterCount++;
                BroadCastMonsterCount();
            }
        }
        public override void OnPlayerEnter(ObjPlayer player)
        {
            base.OnPlayerEnter(player);
            player.Proxy.BroadcastSceneMonsterCount(mMonsterCount);

            player.Attr.SetDataValue(eAttributeType.HpNow, player.Attr.GetDataValue(eAttributeType.HpMax));
            player.Attr.SetDataValue(eAttributeType.MpNow, player.Attr.GetDataValue(eAttributeType.MpMax));
        }
        private void BroadCastMonsterCount()
        {
            foreach (var objPlayer in EnumAllPlayer())
            {
                objPlayer.Proxy.BroadcastSceneMonsterCount(mMonsterCount);
            }
        }
    }
}