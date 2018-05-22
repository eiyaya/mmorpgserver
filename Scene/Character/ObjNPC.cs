#region using

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Algorithms;
using DataContract;
using DataTable;
using Mono.GameMath;
using NLog;
using Shared;

#endregion

namespace Scene
{
    public interface IObjNPC
    {
        void AddEnemy(ObjNPC _this, ulong objId);
        Vector2 CalculatePostionToEnemy(ObjNPC _this, ObjCharacter enemy, float distance);
        void CasterDie(ObjNPC _this, ObjCharacter caster);
        void CasterLeaveScene(ObjNPC _this, ObjCharacter caster);
        void CleanHatre(ObjNPC _this);
        void ClearEnemy(ObjNPC _this);
        void DeleteAITimeTrigger(ObjNPC _this);
        void Destroy(ObjNPC _this);

        /// <summary>
        ///     消失
        /// </summary>
        void Disapeare(ObjNPC _this);

        void Dispose(ObjNPC _this);

        /// <summary>
        ///     进入某个AI状态
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="state"></param>
        void EnterState(ObjNPC _this, BehaviorState state);

        Dictionary<ObjCharacter, int> GetAllHatre(ObjNPC _this);
        ObjCharacter GetCharacterByRole(ObjNPC _this, List<int> TypeList);
        List<ObjCharacter> GetExpList(ObjNPC _this, Dictionary<ObjCharacter, int> GiveExp);
        int GetLevelRef(ObjNPC _this, int MonsterLevel, int TargetLevel);
        ObjCharacter GetMaxDistanceEnemy(ObjNPC _this);
        ObjCharacter GetMaxHatre(ObjNPC _this);
        ObjCharacter GetMaxHatreByTeam(ObjNPC _this, ulong teamId);
        ObjCharacter GetMaxHpNow(ObjNPC _this);
        ObjCharacter GetMinDistanceEnemy(ObjNPC _this);
        ObjCharacter GetMinHatre(ObjNPC _this);
        ObjCharacter GetMinHpNow(ObjNPC _this);
        int GetNowHatre(ObjNPC _this, ObjCharacter character);
        ObjType GetObjType(ObjNPC _this);

        /// <summary>
        ///     初始化
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="id"></param>
        /// <param name="dataId"></param>
        void Init(ObjNPC _this, ulong id, int dataId, int level);

        void InitAI(ObjNPC _this, int level);
        bool InitAttr(ObjNPC _this, int level);
        void InitObjNPC(ObjNPC _this);

        /// <summary>
        ///     初始化表格数据，基类的Init会调用，逼不得已不要手动调
        /// </summary>
        int InitTableData(ObjNPC _this, int level);

        bool IsAggressive(ObjNPC _this);
        bool IsInside(ObjNPC _this, float x, float y);
        bool IsMonster(ObjNPC _this);
		bool IsBoss(ObjNPC _this);
        bool IsInvisible(ObjNPC _this);
        string MyToString(ObjNPC _this);

        /// <summary>
        ///     被攻击时
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="enemy"></param>
        /// <param name="damage"></param>
        void OnDamage(ObjNPC _this, ObjCharacter enemy, int damage);

        /// <summary>
        ///     死亡时
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="characterId"></param>
        /// <param name="viewTime"></param>
        /// <param name="damage"></param>
        void OnDie(ObjNPC _this, ulong characterId, int viewTime, int damage = 0);

        void OnEnemyDie(ObjNPC _this, ObjCharacter obj);

        /// <summary>
        ///     当进入场景时
        /// </summary>
        void OnEnterScene(ObjNPC _this);

        void OnLeaveScene(ObjNPC _this);
        void PushHatre(ObjNPC _this, ObjCharacter caster, int hatre);

        /// <summary>
        ///     复活
        /// </summary>
		void Relive(ObjNPC _this, bool byItem = false);

        void RemoveEnemy(ObjNPC _this, ulong objId);
        void RemoveMeFromOtherEnemyList(ObjNPC _this);

        /// <summary>
        ///     重置
        /// </summary>
        void Reset(ObjNPC _this);

        /// <summary>
        ///     重新刷出(注意和复活的区别)
        /// </summary>
        void Respawn(ObjNPC _this);

        ObjPlayer ScanEnemy(ObjNPC _this, float distance);
        void Seperate(ObjNPC _this, ObjCharacter enemy, float dist);
        void SetBornPosition(ObjNPC _this, Vector2 value);

        /// <summary>
        ///     AI心跳
        /// </summary>
        void Tick_AI(ObjNPC _this);

        void Tick_Patrol(ObjNPC _this, float delta);
    }

    public partial class ObjNPCDefaultImpl : IObjNPC
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Logger kafaLogger = LogManager.GetLogger(Shared.LoggerName.KafkaLog);
        //Obj类型
        public ObjType GetObjType(ObjNPC _this)
        {
            return ObjType.NPC;
        }

        #region 逻辑数据

        public void SetBornPosition(ObjNPC _this, Vector2 value)
        {
            _this.mBornPosition = GetPathPointFromKey(GetPathPointKey(value));
            _this.mPathPoints.Add(GetPathPointKey(_this.mBornPosition));
            for (var i = 1; i < ObjNPC.MAX_PATH_SAVED; i++)
            {
                _this.mPathPoints.Add(GetPathPointKey(GetRandomPosAround(_this)));
            }
        }

        #endregion

        public void InitObjNPC(ObjNPC _this)
        {
            _this.LastState = BehaviorState.Invalid;
            _this.CurrentState = BehaviorState.Invalid;
        }

        /// <summary>
        ///     初始化
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="id"></param>
        /// <param name="dataId"></param>
        public void Init(ObjNPC _this, ulong id, int dataId, int level)
        {
            ObjCharacter.GetImpl().Init(_this, id, dataId, level);
            _this.InitAI(level);
        }

        /// <summary>
        ///     初始化表格数据，基类的Init会调用，逼不得已不要手动调
        /// </summary>
        public int InitTableData(ObjNPC _this, int level)
        {
            level = ObjCharacter.GetImpl().InitTableData(_this, level);
            //Debug.Assert(ObjType.NPC == (ObjType)TableCharacter.Type);
            if (_this.TableCharacter == null)
            {
                _this.TableNpc = Table.GetNpcBase(_this.TypeId);
                Logger.Error("not find CharacterBase id={0}", _this.TypeId);
            }
            else
            {
                _this.TableNpc = Table.GetNpcBase(_this.TableCharacter.ExdataId);
                _this.mCamp = _this.TableCharacter.Camp; //注意这里不用NPC表里的阵营id了
                _this.TableCamp = Table.GetCamp(_this.TableCharacter.Camp);
                _this.NormalSkillId = _this.TableCharacter.InitSkill[0];
            }
            if (_this.TableNpc == null)
            {
                if (level < 1)
                {
                    level = 1;
                }
                _this.SetLevel(level);
                return level;
            }
            _this.mName = _this.TableNpc.Name;
            if (-1 != _this.TableNpc.mAI)
            {
                _this.TableAI = Table.GetAI(_this.TableNpc.mAI);
            }

            if (_this.TableNpc.NPCStopRadius > 0.0f)
            {
                _this.SquaredRadius = _this.TableNpc.NPCStopRadius*_this.TableNpc.NPCStopRadius;
            }
            if (level == -1)
            {
                level = _this.TableNpc.Level;
            }
            _this.SetLevel(level);
            return level;
        }

        public bool InitAttr(ObjNPC _this, int level)
        {
            ObjCharacter.GetImpl().InitAttr(_this, level);
            return true;
        }

        public void InitAI(ObjNPC _this, int level)
        {
            _this.Script = NPCScriptRegister.CreateScriptInstance(_this.TableNpc.AIID);
			_this.Script.Init(_this);
            _this.mNextAction = DateTime.Now.AddSeconds(MyRandom.Random(ObjNPC.MIN_PATROL_TIME, ObjNPC.MAX_PATROL_TIME));

            if (null != _this.TableNpc && _this.TableNpc.HeartRate > 0)
            {
                _this.mAiTickSeconds = Math.Max(ObjNPC.MIN_AI_TICK_SECOND, _this.TableNpc.HeartRate*0.001f);
                var delta = SceneServerControl.Performance*(_this.ObjId%SceneServerControl.Frequence);
                    //把NPC心跳都错开，别让Timer在同一帧里执行
                _this.m_AITimer = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(delta),
                    () => { _this.Tick_AI(); },
                    (int) (_this.mAiTickSeconds*1000));
            }
        }

        /// <summary>
        ///     重置
        /// </summary>
        public void Reset(ObjNPC _this)
        {
            ObjCharacter.GetImpl().Reset(_this);
            _this.SetPosition(_this.BornPosition);
            _this.SetDirection(_this.BornDirection);
            _this.mIsMoving = false;
            _this.mIsForceMoving = false;
            _this.mWaitingToMove = false;
            _this.mTargetPos.Clear();
            _this.EnterState(BehaviorState.Idle);
            _this.mNextAction = DateTime.Now.AddSeconds(MyRandom.Random(ObjNPC.MIN_PATROL_TIME, ObjNPC.MAX_PATROL_TIME));
        }

        /// <summary>
        ///     当进入场景时
        /// </summary>
        public void OnEnterScene(ObjNPC _this)
        {
            _this.Scene.OnNpcEnter(_this);
            ObjCharacter.GetImpl().OnEnterScene(_this);

            _this.EnterState(BehaviorState.Idle);

            try
            {
                _this.Script.OnEnterScene(_this);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            if (_this.TableNpc != null)
            {
                if (_this.TableNpc.NPCStopRadius > 0.0f)
                {
                    var scene = _this.Scene;
                    _this.Scene.AddCollider(_this, _this.TableNpc.NPCStopRadius, id =>
                    {
                        if (!_this.Active)
                        {
                            scene.RemoveCollider(id);
                            return;
                        }
                        _this.mObstacleId = id;
                    });
                }
            }
        }

        public void OnLeaveScene(ObjNPC _this)
        {
            if (_this.TableNpc != null)
            {
                if (_this.TableNpc.NPCStopRadius > 0.0f)
                {
                    if (_this.mObstacleId != null)
                    {
                        _this.Scene.RemoveCollider(_this.mObstacleId.Value);
                        _this.mObstacleId = null;
                    }
                }
            }
            _this.DeleteAITimeTrigger();
            ObjCharacter.GetImpl().OnLeaveScene(_this);
        }

        /// <summary>
        ///     死亡时
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="characterId"></param>
        /// <param name="viewTime"></param>
        /// <param name="damage"></param>
        public void OnDie(ObjNPC _this, ulong characterId, int viewTime, int damage = 0)
        {
            //统计 放到前面是因为ObjCharacter.GetImpl().OnDie会清空伤害列表
            if (_this.Scene != null)
            {
                try
                {
                    var character = _this.Scene.FindCharacter(characterId);
                    ulong charId = 0;
                    var charName = string.Empty;
                    var charLevel = 0;
                    var fightPoint = 0;
                    var serverid = 0;
                    if (character != null)
                    {
                        ObjPlayer p = null;
                        var owner = character.GetRewardOwner();
                        if (null != owner && owner.GetObjType() == ObjType.PLAYER)
                        {
                            charId = owner.ObjId;
                            charName = owner.GetName();
                            charLevel = owner.GetLevel();
                            fightPoint = owner.Attr.GetFightPoint();
                            serverid = owner.ServerId;
                            p = owner as ObjPlayer;
                        }
                        else
                        {
                            charId = characterId;
                            charName = character.GetName();
                            charLevel = character.GetLevel();
                            fightPoint = character.Attr.GetFightPoint();
                            serverid = character.ServerId;
                            if (character.GetObjType() == ObjType.PLAYER)
                            {
                                p = character as ObjPlayer;
                            }
                        }
                        //if (p != null && _this.TableNpc.LimitFlag > 0)
                        //{
                        //    Dict_int_int_Data data = new Dict_int_int_Data();
                        //    data.Data.Add(_this.TableNpc.LimitFlag ,1);
                        //    p.SendExDataChange(data);
                        //}
                    }
                    WorldBOSSRecord bossRecord;
                    if (StaticParam.BossDict.TryGetValue(_this.TableNpc.Id, out bossRecord))
                    {
                        string v = string.Format("BossDie_info#{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}",
                                      serverid,
                                      charId,
                                      charName,
                                      charLevel,
                                      fightPoint,
                                      _this.Scene.TypeId,
                                      _this.TableNpc.Id,
                                      _this.mHatres.Count,
                                      _this.mBornTime.ToString("yyyy/MM/dd HH:mm:ss"),
                                      DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")); // 时间
                        PlayerLog.Kafka(v);
                    }
                }
                catch (Exception)
                {

                }
            }

            try
            {
                if (_this.mDropOnDie)
                {
                    Drop.MonsterKill(_this, characterId); //这里必须是杀死的人，因为杀死他的人有职业的特殊掉落
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            
            try
            {
                _this.Scene.OnNpcDie(_this, characterId);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            finally
            {
                _this.EnterState(BehaviorState.Die);
                _this.mTrigger =
                    SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddMilliseconds(_this.TableNpc.CorpseTime),
                        _this.Disapeare);
                if (_this.TableNpc != null)
                {
                    if (_this.TableNpc.NPCStopRadius > 0.0f)
                    {
                        if (_this.mObstacleId != null)
                        {
                            _this.Scene.RemoveCollider(_this.mObstacleId.Value);
                            _this.mObstacleId = null;
                        }
                    }
                }
            }
            ObjCharacter.GetImpl().OnDie(_this, characterId, viewTime, damage);
            try
            {
                _this.Script.OnDie(_this, characterId, viewTime, damage);

            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        /// <summary>
        ///     消失
        /// </summary>
        public void Disapeare(ObjNPC _this)
        {
            try
            {
                if (null != _this.mTrigger)
                {
                    SceneServerControl.Timer.DeleteTrigger(_this.mTrigger);
                    _this.mTrigger = null;
                }
                _this.Script.OnDisapeare(_this);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            if (_this.TableNpc.IsReviveTime > 0 && _this.CanRelive)
            {
                // 判断定点刷新有没有填值， 如果填了就走定点刷新
                var result = _this.TableNpc.ReviveTime;

                if (_this.TableNpc.RefreshTime != null && _this.TableNpc.RefreshTime.Count > 0)
                {
                    var temp = -1;
                    var vec = _this.TableNpc.RefreshTime;
                    foreach (var data in vec)
                    {
                        var tableTime = DateTime.Now.Date.AddSeconds(data);
                        var deltaSecond = (tableTime - DateTime.Now).TotalMilliseconds;
                        if (deltaSecond > 0)
                        {
                            temp = (int)deltaSecond;
                            break;
                        }
                    }

                    if (temp == -1 && vec.Any())
                    {
                        // 超过今天所有时间了
                        var tableTime = DateTime.Now.Date.AddDays(1); // 加一天
                        tableTime = tableTime.AddSeconds(vec[0]);
                        var deltaSecond = (tableTime - DateTime.Now).TotalMilliseconds;
                        if (deltaSecond > 0)  // 加了一天还比当前时间小 这种情况理论上不会发生
                        {
                            temp = (int)deltaSecond;
                        }
                    }

                    if (temp != -1)
                    {
                        result = temp;
                    }
                }

                _this.mTrigger =
                    SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddMilliseconds(result),
                        _this.Respawn);
                _this.BroadcastDestroyMe(ReasonType.Dead);
            }
            else
            {
                if (_this.Scene != null)
                {
                    _this.Scene.LeaveScene(_this, ReasonType.Dead);
                }
            }
            _this.RemoveMeFromOtherEnemyList();
            _this.ClearEnemy();
            _this.Active = false;
        }

        /// <summary>
        ///     重新刷出(注意和复活的区别)
        ///     复活是指怪物死之后没有消失，被救活了。
        /// </summary>
        public void Respawn(ObjNPC _this)
        {
            _this.mIsDead = false;
            _this.Reset();
            _this.Active = true;
            _this.mBornTime = DateTime.Now;
            _this.BroadcastCreateMe(ReasonType.Born);
            if (null != _this.mTrigger)
            {
                SceneServerControl.Timer.DeleteTrigger(_this.mTrigger);
                _this.mTrigger = null;
            }
            try
            {
                if(_this.Scene != null)
                {
                    _this.Scene.OnNpcRespawn(_this);

                    //发系统消息
                    if (_this.TableNpc != null)
                    {
                        var dicIndex = _this.TableNpc.WorldBroadCastDic;
                        if (dicIndex != -1)
                        {
                            var message = Utils.WrapDictionaryId(dicIndex);
                            if (_this.Scene.mPlayerDict != null && !string.IsNullOrEmpty(message))
                            {
                                var t = _this.Scene.mPlayerDict.Keys.ToList();
                                SceneServer.Instance.ChatAgent.ChatNotify(t, (int)eChatChannel.System, 0, string.Empty,
                                    new ChatMessageContent { Content = message });
                            }
                        }
                    }
                }
                _this.Script.OnRespawn(_this);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            _this.EnterState(BehaviorState.Idle);
        }

        /// <summary>
        ///     复活
        /// </summary>
		public void Relive(ObjNPC _this, bool byItem = false)
        {
            if (_this.mTrigger != null)
            {
                SceneServerControl.Timer.DeleteTrigger(_this.mTrigger);
            }
			ObjCharacter.GetImpl().Relive(_this, byItem);

            try
            {
                _this.Script.OnRelive(_this);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        /// <summary>
        ///     被攻击时
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="enemy"></param>
        /// <param name="damage"></param>
        public void OnDamage(ObjNPC _this, ObjCharacter enemy, int damage)
        {
            if (!_this.Active || _this.IsDead())
            {
                return;
            }

// 			if(mIsMoving)
// 			{
// 				StopMove();//不用被打一下停一下，因为他下个状态会控制他
// 			}

            ObjCharacter.GetImpl().OnDamage(_this, enemy, damage);
	        if (enemy.CanBeAttacked())
	        {
				_this.PushHatre(enemy, damage);    
	        }
            
            _this.EnterState(BehaviorState.Combat);
            //某些需要统计伤害的场景，如灭世之战
            if (null != _this.Scene)
            {
                _this.Scene.OnNpcDamage(_this, damage, enemy);
            }
            try
            {
                _this.Script.OnDamage(_this, enemy, damage);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            if (_this.OnDamageCallback != null)
            {
                _this.OnDamageCallback(_this, enemy, damage);
            }
        }

        //计算自己对于目标敌人该站的地方
        public Vector2 CalculatePostionToEnemy(ObjNPC _this, ObjCharacter enemy, float distance)
        {
            var desPos = enemy.GetPosition() +
                         Vector2.Normalize(_this.GetPosition() - enemy.GetPosition())*(distance - 0.2f);
            if (_this.Scene.ValidPosition(desPos))
            {
                return desPos;
            }

            var desTempPos = _this.Scene.FindNearestValidPosition(desPos, distance);
            if (null != desTempPos)
            {
                return desTempPos.Value;
            }

            return enemy.GetPosition();

// 			const float SkillDistanceThreshold = 0.2f;//技能误差阈值
// 			const float MinSkillDitance = 0.5f;
// 			const float EnemyWidth = 1.5f;
// 			float diffDis = distance - SkillDistanceThreshold;
// 			if (diffDis < MinSkillDitance) diffDis = MinSkillDitance;
// 
// 			int i = 0;
// 			for (; i < enemy.EnemyList.Count; i++)
// 			{
// 				if (enemy.EnemyList[i] == ObjId)
// 				{
// 					break;
// 				}
// 			}
// 
// 		    var n = -1; // 计算这只怪应该排在第几圈
// 		    var index = i;
// 		    while (index >= 0)
//             {
//                 n++;
// 		        index -= (int) Math.Floor((2*Math.PI*(n*EnemyWidth + diffDis))/EnemyWidth);
// 		    }
// 
// 		    diffDis += n*EnemyWidth;
// 
// 			float startRadian = 0.0f;
// 
// 			ObjCharacter firstObj = null;
// 			if(enemy.EnemyList.Count>0)
// 			{
// 				firstObj = Scene.FindCharacter(enemy.EnemyList[0]);
// 			}
// 			
// 			if(null==firstObj)
// 			{
// 				firstObj = _this;
// 			}
// 			
// 			var dif = Vector2.Normalize(firstObj.GetPosition() - enemy.GetPosition());
// 			startRadian = (float)Math.Atan((double)(dif.Y / dif.X));
// 			float radian = startRadian;
// 			if(0!=i)
// 			{
// 				int dir = 0==i%2 ? 1 : -1;
// 				radian += dir * ((i - 1) / 2 + 1) * EnemyWidth / diffDis;
// 			}
// 
// 			var desPos = enemy.GetPosition() + new Vector2((float)Math.Cos(radian) * diffDis, (float)Math.Sin(radian) * diffDis);
// 			if(Scene.ValidPosition(desPos))
// 			{
// 				return desPos;
// 			}
// 
// 			var desTempPos = Scene.FindNearestValidPosition(desPos, diffDis);
// 			if (null != desTempPos)
// 			{
// 				return desTempPos.Value;
// 			}
// 
// 			return enemy.GetPosition();
        }

        //是否主动攻击的怪
        public bool IsAggressive(ObjNPC _this)
        {
            if (null != _this.TableNpc)
            {
                return _this.TableNpc.ViewDistance > 0;
            }

            return false;
        }

        public ObjPlayer ScanEnemy(ObjNPC _this, float distance)
        {
            if (null == _this.Zone)
            {
                return null;
            }

            var sq = distance*distance;
            ObjPlayer target = null;

            foreach (var player in _this.Zone.EnumAllVisiblePlayer())
            {
                if (player != null && !player.IsDead() && _this.IsMyEnemy(player))
                {
                    var temp = (_this.GetPosition() - player.GetPosition()).LengthSquared();
                    if (temp <= sq)
                    {
                        target = player;
                        sq = temp;
                    }
                }
            }

            return target;
        }

        //是否无敌
        public bool IsInvisible(ObjNPC _this)
        {
            if (BehaviorState.GoHome == _this.CurrentState)
            {
                return true;
            }
            return false;
        }

        public void AddEnemy(ObjNPC _this, ulong objId)
        {
            if (objId == _this.ObjId)
            {
                return;
            }

            if (!_this.EnemyList.Contains(objId))
            {
                _this.EnemyList.Add(objId);
            }
        }

        public void OnEnemyDie(ObjNPC _this, ObjCharacter obj)
        {
            _this.CasterDie(obj);

            if (null != _this.Script)
            {
                _this.Script.OnEnemyDie(_this, obj);
            }
        }

        public bool IsInside(ObjNPC _this, float x, float y)
        {
            var c = SceneObstacle.ToSceneCoordinate(x, y);
            if ((_this.GetPosition() - c).LengthSquared() <= _this.SquaredRadius)
            {
                return true;
            }

            return false;
        }

        public bool IsMonster(ObjNPC _this)
        {
			return _this.TableCharacter.Type == 2 || IsBoss(_this);
        }

		public bool IsBoss(ObjNPC _this)
		{
			return _this.TableCharacter.Type == 3;
		}

        public void DeleteAITimeTrigger(ObjNPC _this)
        {
            if (null != _this.m_AITimer)
            {
                SceneServerControl.Timer.DeleteTrigger(_this.m_AITimer);
                _this.m_AITimer = null;
            }
        }

        public void Destroy(ObjNPC _this)
        {
            _this.BuffList.OnDestroy();
            _this.Skill.Reset();
            _this.ClearEnemy();
            _this.DeleteAITimeTrigger();
            if (null != _this.m_AITimer)
            {
                SceneServerControl.Timer.DeleteTrigger(_this.m_AITimer);
                _this.m_AITimer = null;
            }

            if (null != _this.mTrigger)
            {
                SceneServerControl.Timer.DeleteTrigger(_this.mTrigger);
                _this.mTrigger = null;
            }
            ObjCharacter.GetImpl().Destroy(_this);
            _this.Scene = null;
            _this.Zone = null;
        }

        public void Dispose(ObjNPC _this)
        {
            _this.DeleteAITimeTrigger();
        }

        //移除敌人
        public void RemoveEnemy(ObjNPC _this, ulong objId)
        {
            if (null != _this.Scene)
            {
                var obj = _this.Scene.FindCharacter(objId);
                if (null != obj)
                {
                    RemoveCharacter(_this, obj);
                }
            }

            if (_this.EnemyList.Contains(objId))
            {
                _this.EnemyList.Remove(objId);
            }
        }

        //把自己从别人的敌人列表里清除
        public void RemoveMeFromOtherEnemyList(ObjNPC _this)
        {
            if (null != _this.Scene)
            {
                foreach (var id in _this.EnemyList)
                {
                    var obj = _this.Scene.FindCharacter(id);
                    if (null != obj)
                    {
                        obj.RemoveEnemy(_this.ObjId);
                    }
                }
            }
        }

        //清除敌人列表
        public void ClearEnemy(ObjNPC _this)
        {
            _this.EnemyList.Clear();
            _this.mHatres.Clear();
            _this.Skill.Reset();
			_this.SetTargetCharacterId(ulong.MaxValue);
        }

        public string MyToString(ObjNPC _this)
        {
            var sb = new StringBuilder();

            sb.AppendLine("type: NPC");
            sb.AppendLine("name: " + _this.mName);
            sb.AppendLine("pos: " + _this.GetPosition());
            sb.AppendLine("state: " + _this.CurrentState);

            return sb.ToString();
        }

        #region 仇恨系统

        //获取某个单位的当前仇恨
        public int GetNowHatre(ObjNPC _this, ObjCharacter character)
        {
            var result = 0;
            _this.mHatres.TryGetValue(character, out result);
            return result;
        }

        //获得所有带仇恨的单位
        public Dictionary<ObjCharacter, int> GetAllHatre(ObjNPC _this)
        {
            return _this.mHatres;
        }

        //离开场景
        public void CasterLeaveScene(ObjNPC _this, ObjCharacter caster)
        {
            RemoveCharacter(_this, caster);
        }

        private void RemoveCharacter(ObjNPC _this, ObjCharacter caster)
        {
            var t = _this.mHatres.Remove(caster);
            if (t)
            {
                ObjCharacter obj = null;
                _this.Skill.LastSkillMainTarget.TryGetTarget(out obj);
                if (obj == caster)
                {
                    _this.Skill.StopCurrentSkill();
                }
            }
        }

        //死亡了
        public void CasterDie(ObjNPC _this, ObjCharacter caster)
        {
            RemoveCharacter(_this, caster);
        }

        //增加仇恨
        public void PushHatre(ObjNPC _this, ObjCharacter caster, int hatre)
        {
            if (caster == _this)
            {
                return;
            }
            int h;
            if (_this.mHatres.TryGetValue(caster, out h))
            {
                _this.mHatres[caster] = h + hatre;
            }
            else
            {
                _this.mHatres.Add(caster, hatre);
            }
	        if (1==_this.TableNpc.IsShowItemOwnerIcon)
	        {
                //var items = from k in _this.mHatres.Keys
                //            orderby (_this.mHatres[k]) descending
                //            select k;
                //foreach (var objCharacter in items)
                //{
                //    if (null != objCharacter )
                //    {
                //        var owner = objCharacter.GetRewardOwner();
                //        if (null != owner && owner.GetObjType() == ObjType.PLAYER)
                //        {
                //            _this.NormalAttr.TargetCharacter = objCharacter.ObjId;
                //            break;    
                //        }
                //    }
                //}

                //目前只处理 1=队内伤害拾取，见：NpcBase.BelongType ,这里可能效率有些低，应该有时间整体整理下，比如，把队伍伤害排行存起来，而不是每次都计算
                if (_this.TableNpc.BelongType == 1)
                {
                    var GiveExp = new Dictionary<ObjCharacter, int>();
                    var playerList = _this.GetExpList(GiveExp);
                    if (playerList==null || playerList.Count == 0) return;
                    if (playerList[0].GetTeamId() == 0)
                    {
                        var p = playerList[0] as ObjPlayer;
                        if (p != null)
                        {
                            _this.NormalAttr.TargetCharacter = p.ObjId;
                        }
                    }
                    else
                    {
                        var p = playerList[0];
                        var maxHit=_this.GetNowHatre(p);
                        for (int i = 1; i < playerList.Count; i++)
                        {
                            var tempH=_this.GetNowHatre(playerList[i]);
                            if (maxHit < tempH)
                            {
                                maxHit = tempH;
                                p = playerList[i];
                            }
                        }
                        p = p as ObjPlayer;
                        if (p != null)
                        {
                            _this.NormalAttr.TargetCharacter = p.ObjId;
                        }
                    }
                }
	        }
        }

        //清空仇恨列表
        public void CleanHatre(ObjNPC _this)
        {
            _this.mHatres.Clear();
			_this.SetTargetCharacterId(ulong.MaxValue);
        }

        //获得仇恨大的单位
        public ObjCharacter GetMaxHatre(ObjNPC _this)
        {
            if (_this.mHatres.Count == 0)
            {
                return null;
            }
            //int maxHatre = mHatres.Values.Max();
            ObjCharacter maxCharacter = null;
            var maxHatre = -1;
            foreach (var hatre in _this.mHatres)
            {
                if (hatre.Key.IsDead() || !hatre.Key.Active)
                {
                    continue;
                }
                var objPlayer = hatre.Key as ObjPlayer;
                if (objPlayer != null)
                {
                    if (!objPlayer.Online)
                    {
                        continue;
                    }
                }
                if (maxCharacter == null)
                {
                    maxCharacter = hatre.Key;
                    maxHatre = hatre.Value;
                }
                else if (maxHatre < hatre.Value)
                {
                    maxCharacter = hatre.Key;
                    maxHatre = hatre.Value;
                }
            }
            return maxCharacter;
        }

        //获得仇恨小的单位
        public ObjCharacter GetMinHatre(ObjNPC _this)
        {
            if (_this.mHatres.Count == 0)
            {
                return null;
            }
            //int maxHatre = mHatres.Values.Max();
            ObjCharacter minCharacter = null;
            var minHatre = -1;
            foreach (var hatre in _this.mHatres)
            {
                if (hatre.Key.IsDead() || !hatre.Key.Active)
                {
                    continue;
                }
                var objPlayer = hatre.Key as ObjPlayer;
                if (objPlayer != null)
                {
                    if (!objPlayer.Online)
                    {
                        continue;
                    }
                }
                if (minCharacter == null)
                {
                    minCharacter = hatre.Key;
                    minHatre = hatre.Value;
                }
                else if (minHatre > hatre.Value)
                {
                    minCharacter = hatre.Key;
                    minHatre = hatre.Value;
                }
            }
            return minCharacter;
        }

        //获得距离最远的单位
        public ObjCharacter GetMaxDistanceEnemy(ObjNPC _this)
        {
            if (_this.mHatres.Count == 0)
            {
                return null;
            }
            ObjCharacter maxCharacter = null;
            float maxDistance = -1;
            foreach (var hatre in _this.mHatres)
            {
                if (hatre.Key.IsDead() || !hatre.Key.Active)
                {
                    continue;
                }
                var objPlayer = hatre.Key as ObjPlayer;
                if (objPlayer != null)
                {
                    if (!objPlayer.Online)
                    {
                        continue;
                    }
                }
                if (maxCharacter == null)
                {
                    maxCharacter = hatre.Key;
                    maxDistance = (maxCharacter.GetPosition() - _this.GetPosition()).LengthSquared();
                }
                else
                {
                    var distance = (hatre.Key.GetPosition() - _this.GetPosition()).LengthSquared();
                    if (maxDistance < distance)
                    {
                        maxCharacter = hatre.Key;
                        maxDistance = distance;
                    }
                }
            }
            return maxCharacter;
        }

        //获得距离最近的单位
        [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
        public ObjCharacter GetMinDistanceEnemy(ObjNPC _this)
        {
            if (_this.mHatres.Count == 0)
            {
                return null;
            }
            ObjCharacter minCharacter = null;
            float minDistance = -1;
            foreach (var hatre in _this.mHatres)
            {
                if (hatre.Key.IsDead() || !hatre.Key.Active)
                {
                    continue;
                }
                var objPlayer = hatre.Key as ObjPlayer;
                if (objPlayer != null)
                {
                    if (!objPlayer.Online)
                    {
                        continue;
                    }
                }
                if (minCharacter == null)
                {
                    minCharacter = hatre.Key;
                    minDistance = (minCharacter.GetPosition() - _this.GetPosition()).LengthSquared();
                }
                else
                {
                    var distance = (hatre.Key.GetPosition() - _this.GetPosition()).LengthSquared();
                    if (minDistance > distance)
                    {
                        minCharacter = hatre.Key;
                        minDistance = distance;
                    }
                }
            }
            return minCharacter;
        }

        //血量最多的
        public ObjCharacter GetMaxHpNow(ObjNPC _this)
        {
            if (_this.mHatres.Count == 0)
            {
                return null;
            }
            //int maxHatre = mHatres.Values.Max();
            ObjCharacter maxCharacter = null;
            var maxHpNow = -1;
            foreach (var hatre in _this.mHatres)
            {
                if (hatre.Key.IsDead() || !hatre.Key.Active)
                {
                    continue;
                }
                var objPlayer = hatre.Key as ObjPlayer;
                if (objPlayer != null)
                {
                    if (!objPlayer.Online)
                    {
                        continue;
                    }
                }
                var hpNow = hatre.Key.GetAttribute(eAttributeType.HpNow);
                if (maxHpNow < hpNow)
                {
                    maxCharacter = hatre.Key;
                    maxHpNow = hpNow;
                }
            }
            return maxCharacter;
        }

        //血量最少的
        public ObjCharacter GetMinHpNow(ObjNPC _this)
        {
            if (_this.mHatres.Count == 0)
            {
                return null;
            }
            //int maxHatre = mHatres.Values.Max();
            ObjCharacter minCharacter = null;
            var minHpNow = -1;
            foreach (var hatre in _this.mHatres)
            {
                if (hatre.Key.IsDead() || !hatre.Key.Active)
                {
                    continue;
                }
                var objPlayer = hatre.Key as ObjPlayer;
                if (objPlayer != null)
                {
                    if (!objPlayer.Online)
                    {
                        continue;
                    }
                }
                var HpNow = hatre.Key.GetAttribute(eAttributeType.HpNow);
                if (minCharacter == null)
                {
                    minCharacter = hatre.Key;
                    minHpNow = HpNow;
                }
                else if (minHpNow > HpNow)
                {
                    minCharacter = hatre.Key;
                    minHpNow = HpNow;
                }
            }
            return minCharacter;
        }

        //按职业优先的
        public ObjCharacter GetCharacterByRole(ObjNPC _this, List<int> TypeList)
        {
            if (_this.mHatres.Count == 0)
            {
                return null;
            }
            ObjCharacter character = null;
            var TypeIndexNow = -1;
            foreach (var hatre in _this.mHatres)
            {
                if (hatre.Key.IsDead() || !hatre.Key.Active)
                {
                    continue;
                }
                var objPlayer = hatre.Key as ObjPlayer;
                if (objPlayer != null)
                {
                    if (!objPlayer.Online)
                    {
                        continue;
                    }
                }
                var TypeId = hatre.Key.TypeId;
                var TypeIndex = 9999;
                var Index = 0;
                foreach (var i in TypeList)
                {
                    if (TypeId == i)
                    {
                        TypeIndex = Index;
                    }
                    Index++;
                }
                if (character == null)
                {
                    character = hatre.Key;
                    TypeIndexNow = TypeIndex;
                }
                else if (TypeIndexNow > TypeIndex)
                {
                    character = hatre.Key;
                    TypeIndexNow = TypeIndex;
                }
                else if (TypeIndexNow == TypeIndex && _this.mHatres[character] < hatre.Value)
                {
                    character = hatre.Key;
                }
            }
            return character;
        }

        public int GetLevelRef(ObjNPC _this, int MonsterLevel, int TargetLevel)
        {
            var t_m = TargetLevel - MonsterLevel;
            if (ObjNPC.LockCount[t_m + 1000] != null)
            {
                return ObjNPC.LockCount[t_m + 1000].ExpZoom;
            }
            ExpInfoRecord tbExp = null;
            Table.ForeachExpInfo(record =>
            {
                if (t_m < record.LevelDiff)
                {
                    ObjNPC.LockCount[t_m + 1000] = tbExp;
                    return false;
                }
                tbExp = record;
                return true;
            });
            if (tbExp == null)
            {
                return 10000;
            }
            return tbExp.ExpZoom;
        }

        public List<ObjCharacter> GetExpList(ObjNPC _this, Dictionary<ObjCharacter, int> GiveExp)
        {
            var TeamDamage = new Dictionary<ulong, int>();
            var TeamCharacter = new Dictionary<ulong, List<ObjCharacter>>();
            var maxDamage = 0;
            ObjCharacter maxCharacter = null;
            ulong maxTeamId = 0;
            //int nHpMax = GetAttribute(eAttributeType.HpMax);
            var nHpMax = 0;
            var minExp = _this.TableNpc.Exp > 0 ? 1 : 0;
            foreach (var hatre in _this.mHatres)
            {
                if (hatre.Value < 2)
                {
                    continue;
                }
                nHpMax += hatre.Value;
            }
            var monsterLevel = _this.GetLevel();
            foreach (var hatre in _this.mHatres)
            {
                if (hatre.Value < 2)
                {
                    continue;
                }
                var rewardObj = hatre.Key.GetRewardOwner();
                if (rewardObj == null)
                {
                    continue;
                }
                var teamId = rewardObj.GetTeamId();
                if (teamId == 0)
                {
                    if (nHpMax < 1)
                    {
                        PlayerLog.WriteLog((int) LogType.RefreshGem, "GetExpList NPCid={0},nHpMax = {1}",
                            _this.TableNpc.Id,
                            nHpMax);
                        nHpMax = 1;
                    }
                    var exp = (float) hatre.Value*_this.TableNpc.Exp/nHpMax;
                    exp = exp*_this.GetLevelRef(monsterLevel, hatre.Key.GetLevel())/10000;
                    if (exp <= 1)
                    {
                        GiveExp.modifyValue(rewardObj, minExp);
                    }
                    else
                    {
                        GiveExp.modifyValue(rewardObj, (int) exp);
                    }
                    if (hatre.Value >= maxDamage)
                    {
                        maxDamage = hatre.Value;
                        maxCharacter = rewardObj;
                    }
                }
                else
                {
                    int TempDamag;
                    if (TeamDamage.TryGetValue(teamId, out TempDamag))
                    {
                        TempDamag = TempDamag + hatre.Value;
                        if (!TeamCharacter[teamId].Contains(rewardObj))
                        {
                            TeamCharacter[teamId].Add(rewardObj);
                        }
                    }
                    else
                    {
                        TempDamag = hatre.Value;
                        TeamCharacter[teamId] = new List<ObjCharacter> {rewardObj};
                    }
                    TeamDamage[teamId] = TempDamag;
                    if (TempDamag >= maxDamage)
                    {
                        maxCharacter = null;
                        maxTeamId = teamId;
                        maxDamage = TempDamag;
                    }
                }
            }
            foreach (var i in TeamDamage)
            {
                var exp = (double) _this.TableNpc.Exp*i.Value/nHpMax;
                if (exp >= 1)
                {
                    //查找附近同队伍的人
                    var TeamCount = 0;
                    ObjNPC.teamPlayer.Clear();
                    var totleLevel = 0;
                    var totleLevelP = 0;
                    var totleLevelRef = 0;
                    //5538 [优化]：关于组队经验分享，现在只能同屏可以分享到经验，要改为同场景都可以
                    IEnumerable<ObjPlayer> temp = _this.Zone.EnumAllVisiblePlayer();
                    if (_this.Zone.mScene != null)
                        temp = _this.Zone.mScene.EnumAllPlayer();
                    foreach (var objPlayer in temp)
                   // foreach (var objPlayer in _this.Zone.EnumAllVisiblePlayer())
                    {
                        if (objPlayer.GetTeamId() != i.Key)
                        {
                            continue;
                        }
                        TeamCount++;
                        ObjNPC.teamPlayer.Add(objPlayer);
                        var level = objPlayer.GetLevel();
                        totleLevel += level;
                        totleLevelP += level*level;
                        totleLevelRef += _this.GetLevelRef(monsterLevel, level);
                    }
                    if (TeamCount < 1)
                    {
                        continue;
                    }
                    totleLevelRef = totleLevelRef/TeamCount;
                    ////取队伍人数
                    //int TeamCount = TeamCharacter[i.Key].Count; //队伍人数
                    ////平均等级
                    //int avgLevel=0;
                    //int totleLevel = 0;
                    //int totleLevelRef = 0;
                    //foreach (ObjCharacter character in TeamCharacter[i.Key])
                    //{
                    //    totleLevel += character.GetLevel();
                    //    totleLevelRef += GetLevelRef(monsterLevel, character.GetLevel());
                    //}
                    var avgLevel = totleLevelP/TeamCount;
                    var dAvgLevel = Math.Sqrt(avgLevel);
                    //队伍人数修正
                    var TeamCountRef = Table.GetExpInfo(TeamCount).CountExpProp;
                    //TeamrefRecord.Param[TeamCount * 2 -1];
                    TeamCountRef = TeamCountRef*TeamCount;
                    //队伍等级修正
                    //int teamRef = GetLevelRef(monsterLevel, avgLevel);
                    //int teamRef = totleLevelRef * TeamCount;
                    var newTeamAddRef = 1;
                    {// 2017.08.30视野内队员加成新增乘以系数：
                        newTeamAddRef = Table.GetExpInfo(TeamCount).TeamCountExpProp;
                    }
                    //分配给队伍成员
                    var dTotleLevel = totleLevel + TeamCount*dAvgLevel;
                    exp = exp*totleLevelRef/10000*TeamCountRef/10000* newTeamAddRef/10000;
                    List<ObjCharacter> teamCharacters;
                    if (TeamCharacter.TryGetValue(i.Key, out teamCharacters))
                    {
                        foreach (var character in ObjNPC.teamPlayer)
                        {
                            var level = character.GetLevel();
                            var expThis = (int) (exp*(level + dAvgLevel)/dTotleLevel);
                            if (expThis < 1)
                            {
                                expThis = minExp;
                            }
                            GiveExp[character] = expThis;
                            if (!teamCharacters.Contains(character))
                            {
                                teamCharacters.Add(character);
                            }
                        }
                    }
                }
            }
            if (maxCharacter == null)
            {
                List<ObjCharacter> characters = null;
                TeamCharacter.TryGetValue(maxTeamId, out characters);
                return characters;
                //foreach (KeyValuePair<ObjCharacter, int> hatre in mHatres)
                //{
                //    if (hatre.Key.GetTeamId() == maxTeamId)
                //    {
                //        characters.Add(hatre.Key);
                //    }
                //}
            }
            else
            {
                var characters = new List<ObjCharacter>();
                characters.Add(maxCharacter);
                return characters;
            }
        }

        //根据队伍Id取伤害最大的单位
        public ObjCharacter GetMaxHatreByTeam(ObjNPC _this, ulong teamId)
        {
            var maxDamage = 0;
            ObjCharacter maxCharacter = null;
            var hatresMax = new Dictionary<ObjCharacter, int>();
            foreach (var hatre in _this.mHatres)
            {
                var rewardObj = hatre.Key.GetRewardOwner();
                if (rewardObj == null)
                {
                    continue;
                }
                if (rewardObj.GetTeamId() != teamId)
                {
                    continue;
                }
                hatresMax.modifyValue(hatre.Key, hatre.Value);
                var value = hatresMax[hatre.Key];
                if (value > maxDamage)
                {
                    maxDamage = value;
                    maxCharacter = rewardObj;
                }
            }
            return maxCharacter;
        }

        //巡逻
        public void Tick_Patrol(ObjNPC _this, float delta)
        {
            if (BehaviorState.Idle != _this.CurrentState)
            {
                return;
            }

            if (_this.IsMoving())
            {
                return;
            }

            if (_this.TableNpc == null)
            {
                return;
            }
            if (1 != _this.TableNpc.Patrol || _this.TableNpc.PatrolRadius <= 0)
            {
                return;
            }

            if (DateTime.Now > _this.mNextAction)
            {
                RandomMove(_this);
                _this.mNextAction =
                    DateTime.Now.AddSeconds(MyRandom.Random(ObjNPC.MIN_PATROL_TIME, ObjNPC.MAX_PATROL_TIME));
            }
        }

        /// <summary>
        ///     用空间换时间，预先存储 MAX_PATH_SAVED 条路径，随机选一条
        ///     不一次性都初始化好是为了降低初始化场景的瞬间压力
        /// </summary>
        private void RandomMove(ObjNPC _this)
        {
            var start = _this.Scene.FindNearestValidPosition(_this.GetPosition()).GetValueOrDefault();
            var key = GetPathPointKey(start);

            if (!_this.mPathPoints.Contains(key))
            {
                if (_this.GetObjType() != ObjType.RETINUE)
                {
                    _this.MoveTo(_this.BornPosition, 0);
                }
                return;
            }

            var i = MyRandom.Random(0, _this.mPathPoints.Count - 1);
            while (_this.mPathPoints[i] == key)
            {
                i = MyRandom.Random(0, _this.mPathPoints.Count - 1);
            }

            var targetKey = _this.mPathPoints[i];

            Dictionary<uint, List<Vector2>> pathes;
            if (_this.mPathes.TryGetValue(key, out pathes))
            {
                List<Vector2> path;
                if (pathes.TryGetValue(targetKey, out path))
                {
                    _this.Move(new List<Vector2>(path));
                }
                else
                {
                    _this.MoveTo(GetPathPointFromKey(targetKey), 0, true, false,
                        p => { pathes[targetKey] = new List<Vector2>(p); });
                }
            }
            else
            {
                pathes = new Dictionary<uint, List<Vector2>>();
                _this.mPathes.Add(key, pathes);
                _this.MoveTo(GetPathPointFromKey(targetKey), 0, true, false,
                    p => { pathes[targetKey] = new List<Vector2>(p); });
            }
        }

        private Vector2 GetRandomPosAround(ObjNPC _this)
        {
            var r = 2*Math.PI*MyRandom.Random();

            var distance = _this.TableNpc.PatrolRadius*Math.Max(0.5 + MyRandom.Random(), 1);
            var xOffset = (float) (Math.Cos(r)*distance);
            var yOffset = (float) (Math.Sin(r)*distance);

            var targetPos = new Vector2(_this.BornPosition.X + xOffset, _this.BornPosition.Y + yOffset);
            return targetPos;
        }

        private uint GetPathPointKey(Vector2 v)
        {
            return ((uint) (v.X*2) << 16) + (uint) (v.Y*2);
        }

        private Vector2 GetPathPointFromKey(uint i)
        {
            return new Vector2(((i & 0xFFFF0000) >> 16)/2.0f, (i & 0xFFFF)/2.0f);
        }

        #endregion
    }


    public partial class ObjNPC : ObjCharacter, ICollider
    {
        public const int MAX_PATH_SAVED = 5;
        public const int MAX_PATROL_TIME = 18;
        private static IObjNPC mImpl;
        public const float MIN_AI_TICK_SECOND = 0.1f;
        public const int MIN_PATROL_TIME = 8;

        static ObjNPC()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (ObjNPC), typeof (ObjNPCDefaultImpl),
                o => { mImpl = (IObjNPC) o; });
        }

        //构造函数
        public ObjNPC()
        {
            mImpl.InitObjNPC(this);
        }

        // BehaviorState 变化回调
        public Action<ObjNPC, BehaviorState, BehaviorState> OnBehaviourChangeCallback;

        //脚本
        public NPCScriptBase mNPCScriptBase;
        public Action<ObjNPC, ObjCharacter, int> OnDamageCallback;
        public float SquaredRadius;

        public override void AddEnemy(ulong objId)
        {
            mImpl.AddEnemy(this, objId);
        }

        //计算自己对于目标敌人该站的地方
        public Vector2 CalculatePostionToEnemy(ObjCharacter enemy, float distance)
        {
            return mImpl.CalculatePostionToEnemy(this, enemy, distance);
        }

        //清除敌人列表
        public override void ClearEnemy()
        {
            mImpl.ClearEnemy(this);
        }

        public void DeleteAITimeTrigger()
        {
            mImpl.DeleteAITimeTrigger(this);
        }

        public override void Destroy()
        {
            mImpl.Destroy(this);
        }

        /// <summary>
        ///     消失
        /// </summary>
        public virtual void Disapeare()
        {
            mImpl.Disapeare(this);
        }

        public override void Dispose()
        {
            mImpl.Dispose(this);
        }

        public new static IObjNPC GetImpl()
        {
            return mImpl;
        }

        //Obj类型
        public override ObjType GetObjType()
        {
            return mImpl.GetObjType(this);
        }

        /// <summary>
        ///     初始化
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dataId"></param>
        public override void Init(ulong id, int dataId, int level)
        {
            mImpl.Init(this, id, dataId, level);
        }

        public virtual void InitAI(int level)
        {
            mImpl.InitAI(this, level);
        }

        public override bool InitAttr(int level)
        {
            return mImpl.InitAttr(this, level);
        }

        /// <summary>
        ///     初始化表格数据，基类的Init会调用，逼不得已不要手动调
        /// </summary>
        public override int InitTableData(int level)
        {
            return mImpl.InitTableData(this, level);
        }

        //是否主动攻击的怪
        public bool IsAggressive()
        {
            return mImpl.IsAggressive(this);
        }

        //是否无敌
        public override bool IsInvisible()
        {
            return mImpl.IsInvisible(this);
        }

        /// <summary>
        ///     被攻击时
        /// </summary>
        /// <param name="enemy"></param>
        /// <param name="damage"></param>
        public override void OnDamage(ObjCharacter enemy, int damage)
        {
            mImpl.OnDamage(this, enemy, damage);
        }

        /// <summary>
        ///     死亡时
        /// </summary>
        /// <param name="characterId"></param>
        /// <param name="viewTime"></param>
        /// <param name="damage"></param>
        public override void OnDie(ulong characterId, int viewTime, int damage = 0)
        {
            mImpl.OnDie(this, characterId, viewTime, damage);
        }

        public override void OnEnemyDie(ObjCharacter obj)
        {
            mImpl.OnEnemyDie(this, obj);
        }

        /// <summary>
        ///     当进入场景时
        /// </summary>
        public override void OnEnterScene()
        {
            mImpl.OnEnterScene(this);
        }

        public override void OnLeaveScene()
        {
            mImpl.OnLeaveScene(this);
        }

        /// <summary>
        ///     复活
        /// </summary>
		public override void Relive(bool byItem = false)
        {
            mImpl.Relive(this,byItem);
        }

        //移除敌人
        public override void RemoveEnemy(ulong objId)
        {
            mImpl.RemoveEnemy(this, objId);
        }

        //把自己从别人的敌人列表里清除
        public override void RemoveMeFromOtherEnemyList()
        {
            mImpl.RemoveMeFromOtherEnemyList(this);
        }

        /// <summary>
        ///     重置
        /// </summary>
        public override void Reset()
        {
            mImpl.Reset(this);
        }

        /// <summary>
        ///     重新刷出(注意和复活的区别)
        /// </summary>
        public virtual void Respawn()
        {
            mImpl.Respawn(this);
        }

        public ObjPlayer ScanEnemy(float distance)
        {
            return mImpl.ScanEnemy(this, distance);
        }

        public override string ToString()
        {
            return mImpl.MyToString(this);
        }

        public bool IsInside(float x, float y)
        {
            return mImpl.IsInside(this, x, y);
        }

        public bool IsMonster()
        {
            return mImpl.IsMonster(this);
        }

	    public bool IsBoss()
	    {
			return mImpl.IsBoss(this);
	    }

        #region 逻辑数据

        //触发器
        public Trigger mTrigger;

        //出生点
        public Vector2 mBornPosition;

        //出生时间
        public DateTime mBornTime;

        public Vector2 BornPosition
        {
            get { return mBornPosition; }
            set { mImpl.SetBornPosition(this, value); }
        }

        //出生方向
        public Vector2 BornDirection;

        //普攻技能
        public int NormalSkillId { get; set; }

        //站在原地不动的时间
        public DateTime mNextAction = DateTime.Now;

        //private ulong mTickIndex = 0;

        //AI Time Trigger
        public Trigger m_AITimer;

        //AI心跳时间间隔，0表示不心跳
        public float mAiTickSeconds;

        //死的时候是否执行掉落函数
        public bool mDropOnDie = true;

        //动态阻挡id
        public uint? mObstacleId;

        //默认都不能重生
        public bool CanRelive = false;

        //场景表Id
        public SceneNpcRecord tbSceneNpc;

        //最迟复生时间点(用于蔑视之战)
        public DateTime ReliveTimer ;
        #endregion

        #region 表格数据

        //NPC表
        public virtual NpcBaseRecord TableNpc { get; set; }

        //AI表
        public AIRecord TableAI { get; set; }

        #endregion

        #region 仇恨系统

        public readonly Dictionary<ObjCharacter, int> mHatres = new Dictionary<ObjCharacter, int>();

        //获取某个单位的当前仇恨
        public int GetNowHatre(ObjCharacter character)
        {
            return mImpl.GetNowHatre(this, character);
        }

        //获得所有带仇恨的单位
        public Dictionary<ObjCharacter, int> GetAllHatre()
        {
            return mImpl.GetAllHatre(this);
        }

        //离开场景
        public void CasterLeaveScene(ObjCharacter caster)
        {
            mImpl.CasterLeaveScene(this, caster);
        }

        //死亡了
        public void CasterDie(ObjCharacter caster)
        {
            mImpl.CasterDie(this, caster);
        }

        //增加仇恨
        public void PushHatre(ObjCharacter caster, int hatre)
        {
            mImpl.PushHatre(this, caster, hatre);
        }

        //清空仇恨列表
        public void CleanHatre()
        {
            mImpl.CleanHatre(this);
        }

        //获得仇恨大的单位
        public ObjCharacter GetMaxHatre()
        {
            return mImpl.GetMaxHatre(this);
        }

        //获得仇恨小的单位
        public ObjCharacter GetMinHatre()
        {
            return mImpl.GetMinHatre(this);
        }

        //获得距离最远的单位
        public ObjCharacter GetMaxDistanceEnemy()
        {
            return mImpl.GetMaxDistanceEnemy(this);
        }

        //获得距离最近的单位
        [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
        public ObjCharacter GetMinDistanceEnemy()
        {
            return mImpl.GetMinDistanceEnemy(this);
        }

        //血量最多的
        public ObjCharacter GetMaxHpNow()
        {
            return mImpl.GetMaxHpNow(this);
        }

        //血量最少的
        public ObjCharacter GetMinHpNow()
        {
            return mImpl.GetMinHpNow(this);
        }

        //按职业优先的
        public ObjCharacter GetCharacterByRole(List<int> TypeList)
        {
            return mImpl.GetCharacterByRole(this, TypeList);
        }

        //获取有权获得经验的队伍
        public static DropConfigRecord TeamrefRecord = Table.GetDropConfig(1);
        public static DropConfigRecord LevelrefRecord = Table.GetDropConfig(0);
        public static ExpInfoRecord[] LockCount = new ExpInfoRecord[2000];

        public int GetLevelRef(int MonsterLevel, int TargetLevel)
        {
            return mImpl.GetLevelRef(this, MonsterLevel, TargetLevel);
        }

        public static List<ObjPlayer> teamPlayer = new List<ObjPlayer>();

        public List<ObjCharacter> GetExpList(Dictionary<ObjCharacter, int> GiveExp)
        {
            return mImpl.GetExpList(this, GiveExp);
        }

        //根据队伍Id取伤害最大的单位
        public ObjCharacter GetMaxHatreByTeam(ulong teamId)
        {
            return mImpl.GetMaxHatreByTeam(this, teamId);
        }

        public List<uint> mPathPoints = new List<uint>();

        public Dictionary<uint, Dictionary<uint, List<Vector2>>> mPathes =
            new Dictionary<uint, Dictionary<uint, List<Vector2>>>();

        //巡逻
        public void Tick_Patrol(float delta)
        {
            mImpl.Tick_Patrol(this, delta);
        }

        #endregion
    }
}