#region using

using System;
using Mono.GameMath;
using Shared;

#endregion

namespace Scene
{
    public enum BehaviorState
    {
        Invalid = -1,
        Idle = 0,
        Combat,
        GoHome,
        Die
    }

    public partial class ObjNPCDefaultImpl
    {
        private void CalculateRotation()
        {
            ObjNPC.PreCalculatedClockwiseRotation = new Vector2[72];
            ObjNPC.PreCalculatedCounterClockwiseRotation = new Vector2[72];

            for (var i = 0; i < 72; i += 2)
            {
                // 带点随机偏移，防止有时候停不下来
                var angle = ((i + 1)*5 + (MyRandom.Random(5) + 1))*2*Math.PI/360;
                ObjNPC.PreCalculatedClockwiseRotation[i + 0] = new Vector2((float) Math.Cos(angle),
                    (float) -Math.Sin(angle));
                ObjNPC.PreCalculatedClockwiseRotation[i + 1] = new Vector2((float) Math.Sin(angle),
                    (float) Math.Cos(angle));
                ObjNPC.PreCalculatedCounterClockwiseRotation[i + 0] = new Vector2((float) Math.Cos(angle),
                    (float) Math.Sin(angle));
                ObjNPC.PreCalculatedCounterClockwiseRotation[i + 1] = new Vector2((float) -Math.Sin(angle),
                    (float) Math.Cos(angle));
            }
        }

        /// <summary>
        ///     进入某个AI状态
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="state"></param>
        public void EnterState(ObjNPC _this, BehaviorState state)
        {
            if (state == _this.CurrentState)
            {
                return;
            }

            try
            {
                switch (_this.CurrentState)
                {
                    case BehaviorState.Idle:
                        _this.Script.OnExitIdle(_this);
                        break;
                    case BehaviorState.Combat:
                        _this.Script.OnExitCombat(_this);
                        break;
                    case BehaviorState.GoHome:
                        _this.Script.OnExitGoHome(_this);
                        break;
                    case BehaviorState.Die:
                        _this.Script.OnExitDie(_this);
                        break;
                    default:
                        //Logger.Warn("Unknow ai type");
                        break;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            _this.LastState = _this.CurrentState;
            _this.CurrentState = state;
            try
            {
                switch (_this.CurrentState)
                {
                    case BehaviorState.Idle:
                        _this.Script.OnEnterIdle(_this);
                        break;
                    case BehaviorState.Combat:
                        _this.Script.OnEnterCombat(_this);
                        break;
                    case BehaviorState.GoHome:
                        _this.Script.OnEnterGoHome(_this);
                        _this.RemoveMeFromOtherEnemyList();
                        _this.ClearEnemy();
                        _this.Skill.LastSkillMainTarget.SetTarget(null);
                        break;
                    case BehaviorState.Die:
                        _this.Script.OnEnterDie(_this);
                        break;
                    default:
                        Logger.Warn("Unknow ai type");
                        break;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            // 爬塔怪物行为状态监听
            if (null != _this.OnBehaviourChangeCallback)
            {
                _this.OnBehaviourChangeCallback.Invoke(_this, _this.CurrentState, _this.LastState);
            }
        }

        /// <summary>
        ///     AI心跳
        /// </summary>
        public virtual void Tick_AI(ObjNPC _this)
        {
            if (!_this.Active)
            {
                return;
            }

            if (null == _this.Zone)
            {
                return;
            }

            //如果可以看见我的zone里没有玩家
            if (!_this.Zone.HasPlayerInAllVisibleZone && _this.CurrentState != BehaviorState.Combat) //战斗状态还是需要按需心跳的
            {
                if (!_this.Script.IsForceTick())
                {
                    return;
                }
            }

            try
            {
                switch (_this.CurrentState)
                {
                    case BehaviorState.Idle:
                        _this.Script.OnTickIdle(_this, _this.mAiTickSeconds);
                        _this.Tick_Patrol(_this.mAiTickSeconds);
                        break;
                    case BehaviorState.Combat:
                        _this.Script.OnTickCombat(_this, _this.mAiTickSeconds);
                        break;
                    case BehaviorState.GoHome:
                        _this.Script.OnTickGoHome(_this, _this.mAiTickSeconds);
                        break;
                    case BehaviorState.Die:
                        _this.Script.OnTickDie(_this, _this.mAiTickSeconds);
                        break;
                    default:
                        Logger.Warn("Unknow ai type");
                        break;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public void Seperate(ObjNPC _this, ObjCharacter enemy, float dist)
        {
            if (ObjNPC.PreCalculatedClockwiseRotation == null)
            {
                CalculateRotation();
            }

            var move = false;
            ObjCharacter npc;

            // 如果人很多的话，减小距离
            var d = 2*Math.PI*dist/enemy.EnemyList.Count;
            var t = Math.Min(d*d, 0.25f);
            var s = 0;
            var i = 0;
            for (; i < enemy.EnemyList.Count; i++)
            {
                var npcId = enemy.EnemyList[i];
                if (npcId == _this.ObjId)
                {
                    break;
                }

                npc = _this.Scene.FindCharacter(npcId);
                if (npc == null)
                {
                    enemy.EnemyList.RemoveAt(i);
                    --i;
                    continue;
                }
                var distSqr = (npc.GetPosition() - _this.GetPosition()).LengthSquared();

                if (distSqr < 1)
                {
                    if (distSqr < t)
                    {
                        move = true;
                    }

                    s++;
                }
            }


            if (s >= 36)
            {
                // 怪实在太多了，随便找个地方给他吧
                s = MyRandom.Random(3);
            }

            if (move)
            {
                var pos = _this.GetPosition();
                var p = (pos - enemy.GetPosition());

                //顺时针旋转
                if (_this.ObjId%2 == 0)
                {
                    var x = Vector2.Dot(p, ObjNPC.PreCalculatedClockwiseRotation[2*s]);
                    var y = Vector2.Dot(p, ObjNPC.PreCalculatedClockwiseRotation[2*s + 1]);
                    pos = enemy.GetPosition() + new Vector2(x, y);
                }
                else
                {
                    var x = Vector2.Dot(p, ObjNPC.PreCalculatedCounterClockwiseRotation[2*s]);
                    var y = Vector2.Dot(p, ObjNPC.PreCalculatedCounterClockwiseRotation[2*s + 1]);
                    pos = enemy.GetPosition() + new Vector2(x, y);
                }

                if (enemy.Scene.ValidPosition(pos))
                {
                    _this.SetPosition(pos);
                }
            }
        }
    }

    public partial class ObjNPC : ObjCharacter
    {
        public static Vector2[] PreCalculatedClockwiseRotation;
        public static Vector2[] PreCalculatedCounterClockwiseRotation;
        public BehaviorState CurrentState { get; set; }
        public BehaviorState LastState { get; set; }
        public NPCScriptBase Script { get; set; }

        /// <summary>
        ///     进入某个AI状态
        /// </summary>
        /// <param name="state"></param>
        public void EnterState(BehaviorState state)
        {
            mImpl.EnterState(this, state);
        }

        public void Seperate(ObjCharacter enemy, float dist)
        {
            mImpl.Seperate(this, enemy, dist);
        }

        /// <summary>
        ///     AI心跳
        /// </summary>
        public virtual void Tick_AI()
        {
            mImpl.Tick_AI(this);
        }
    }
}