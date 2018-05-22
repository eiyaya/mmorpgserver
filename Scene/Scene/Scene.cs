#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DataContract;
using DataTable;
using Scorpion;
using Mono.GameMath;
using NLog;
using Scene.Character;
using Shared;

#endregion

namespace Scene
{
    public interface IScene
    {
        void AddCollider(Scene _this, ObjCharacter obj, float radius, Action<uint> callback);
        void SetLodeInfo(Scene _this, MsgSceneLode info);
        void AddObj(Scene _this, ObjBase obj);
        void BroadcastSceneAction(Scene _this, int actionId);
        void BroadcastSceneAction(Scene _this, TriggerAreaRecord tbArea);
        bool CanPk(Scene _this);
        ReasonType GetBornVisibleType();
        void ChangeDifficulty(Scene _this, int count);
        AutoPlayer CreateAutoPlayer(Scene _this, LogicSimpleData logic, SceneSimpleData scene, Vector2 pos, Vector2 dir);
        AutoPlayer CreateAutoPlayer(Scene _this, int RobotId, Vector2 pos, Vector2 dir);
        ObjBoss CreateBoss(Scene _this, int dataId, Vector2 pos, Vector2 dir, string name = "", int level = -1);

        ObjDropItem CreateDropItem(Scene _this,
                                   int type,
                                   List<ulong> ownerList,
                                   ulong teamId,
                                   int itemId,
                                   int count,
                                   Vector2 pos);

        ObjNPC CreateNpc(Scene _this, SceneNpcRecord sceneNpcRecord, int dataId, Vector2 pos, Vector2 dir, string name = "", int level = -1);

        ObjRetinue CreateRetinue(Scene _this,
                                 int dataId,
                                 ObjCharacter owner,
                                 Vector2 pos,
                                 Vector2 dir,
                                 int camp,
                                 int level = -1);

        ObjBoss CreateSceneBoss(Scene _this, int sceneNpcId, Vector2 pos = default(Vector2));
        ObjNPC CreateSceneNpc(Scene _this, int sceneNpcId, Vector2 pos = default(Vector2), int level = -1);
        ObjNPC CreateSpeMonster(Scene _this, WorldBOSSRecord record);
        void CreateSpeMonsters(Scene _this, List<int> worldBossIds);
        Trigger CreateTimer(Scene _this, DateTime targetTime, Action act, int interval = -1);
        void DealWith(Scene _this, string name, object param);
        void DeleteTimer(Scene _this, Trigger trigger);
        void Destroy(Scene _this);
        string DumpInfo(Scene _this);
        bool EnterScene(Scene _this, ObjBase obj);
        IEnumerable<ObjPlayer> EnumAllPlayer(Scene _this);
        IEnumerable<ulong> EnumAllPlayerId(Scene _this);
        void ExitDungeon(Scene _this, ObjPlayer player);
        ObjCharacter FindCharacter(Scene _this, ulong objId);

        /// <summary>
        ///     找离pos最近的合法的路径点
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="pos">当前点</param>
        /// <param name="maxDist">搜索半径</param>
        /// <returns>如果太远没找到，则返回null</returns>
        Vector2? FindNearestValidPosition(Scene _this, Vector2 pos, float maxDist = 5.0f);

        ObjBase FindObj(Scene _this, ulong objId);
        bool FindPathTo(Scene _this, ObjCharacter obj, Vector2 target, Action<List<Vector2>> callback);

        bool FindPathTo(Scene _this,
                        Coroutine co,
                        ObjCharacter obj,
                        Vector2 target,
                        AsyncReturnValue<List<Vector2>> result);

        ObjPlayer FindPlayer(Scene _this, ulong objId);

        int GetAlivePlayerNum(Scene _this);
        ulong GenerateNextId(Scene _this);
        SceneObstacle.ObstacleValue GetObstacleValue(Scene _this, float x, float y);
        Zone GetZone(Scene _this, int id);
        bool Init(Scene _this, SceneParam param);
        void InitMapNPC(Scene _this);
        bool InitNPC(Scene _this);
        bool InitZone(Scene _this);
        bool IsFull(Scene _this);
        ObjNPC IsNeedChangeHp(ObjBase mObj);
        void KickAllPlayer(Scene _this);
        void KickPlayer(Scene _this, ulong id);
        void LateDeleteScene(Scene _this, int waitSec, RemoveScene removeType);
        bool LeaveScene(Scene _this, ObjBase obj, ReasonType reason = ReasonType.VisibilityChanged);
        void Log(Scene _this, Logger logger, string text, params object[] args);
        void LogError(Scene _this, Logger logger, string text, params object[] args);
        int NpcDoDamageModify(Scene _this, int value);
        void OnAreaTick(Scene _this, int areaId, IEnumerator<ObjCharacter> enumerator);
        void OnCharacterEnterArea(Scene _this, int areaId, ObjCharacter character);
        void OnCharacterLeaveArea(Scene _this, int areaId, ObjCharacter character);
        void OnCreate(Scene _this);
        void OnDestroy(Scene _this);
        void OnNpcDie(Scene _this, ObjNPC npc, ulong characterId = 0);
        void OnNpcRespawn(Scene _this, ObjNPC npc);
        void OnPlayerPickUp(Scene _this, ulong objId,int itemId,int count);
        void OnNpcEnter(Scene _this, ObjNPC player);
        void OnNpcDamage(Scene _this, ObjNPC npc, int damage, ObjBase enemy);
		void OnPlayerExDataChanged(Scene _this, ObjPlayer npc, int idx, int val);
        void OnObjBeforeEnterScene(Scene _this, ObjBase obj);
        void OnPlayerDie(Scene _this, ObjPlayer player, ulong characterId = 0);
		void OnPlayerRelive(Scene _this, ObjPlayer player, bool byItem);
        void OnPlayerEnter(Scene _this, ObjPlayer player);
        void OnPlayerEnterOver(Scene _this, ObjPlayer player);
        void RefreshLodeTimer(Scene _this);
        void AfterPlayerEnterOver(Scene _this, ObjPlayer player);
        
        void OnPlayerLeave(Scene _this, ObjPlayer player);
        int PlayerDoDamageModify(Scene _this, int value);
        int Pos2ZoneId(Scene _this, float x, float y);
        void PushActionToAllObj(Scene _this, Action<ObjBase> action);
        void PushActionToAllPlayer(Scene _this, Action<ObjPlayer> action);
        bool Raycast(Scene _this, ObjCharacter obj, Vector2 target, Action<Vector2> callback);
        bool Raycast(Coroutine co, Scene _this, ObjCharacter obj, Vector2 target, AsyncReturnValue<Vector2> result);
        void RemoveAllObj(Scene _this);
        void RemoveAllNPC(Scene _this);
        void RemoveCollider(Scene _this, uint id);
        void RemoveObj(Scene _this, ObjBase obj);
        void SceneShapeAction(Scene _this, Shape shape, Action<ObjCharacter> action);
        void Tick(Scene _this, float delta);
        IEnumerator TryDeleteScene(Coroutine co, Scene _this, RemoveScene removeType);
        bool ValidPath(Scene _this, Vector2 currentPos, IEnumerable<Vector2> path);

        /// <summary>
        ///     判断当前点是否合法
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        bool ValidPosition(Scene _this, Vector2 pos);

        IEnumerator ApplyLodeInfo(Coroutine co,Scene _this);
        void BroadCastLodeInfo(Scene _this);
        IEnumerator ChangeSceneCoroutine(Coroutine co, ObjCharacter character, int scneneId, int x, int y);
        void AutoRelive(Scene _this, ObjPlayer player);
    }

    public partial class SceneDefaultImpl : IScene
    {
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IEnumerator AddColliderImpl(Coroutine co,
                                            Scene _this,
                                            ObjCharacter obj,
                                            float radius,
                                            Action<uint> callback)
        {
            var result = AsyncReturnValue<uint>.Create();
            var b = PathManager.AddCollider(co, _this.Obstacle, obj, radius, result);
            yield return b;
            if (callback != null)
            {
                callback(result.Value);
            }
        }

        private IEnumerator FindPathToImpl(Coroutine co,
                                           Scene _this,
                                           ObjCharacter obj,
                                           Vector2 target,
                                           Action<List<Vector2>> callback)
        {
            var result = AsyncReturnValue<List<Vector2>>.Create();
            var b = _this.FindPathTo(co, obj, target, result);
            yield return b;
            if (callback != null)
            {
                callback(result.Value);
            }
        }

        private IEnumerator RaycastImpl(Coroutine co,
                                        Scene _this,
                                        ObjCharacter obj,
                                        Vector2 target,
                                        Action<Vector2> callback)
        {
            var result = AsyncReturnValue<Vector2>.Create();
            var res = result;
            result.Dispose();
            var b = _this.Raycast(co, obj, target, res);
            yield return b;

            if (callback != null)
            {
                callback(res.Value);
            }
        }

        #region 常量定义

        //生成下个obj id
        public ulong GenerateNextId(Scene _this)
        {
            if (_this.mNextObjId > 10000000000)
            {
                _this.mNextObjId = 10000;
            }
            return _this.mNextObjId++;
        }

        #endregion

        public virtual bool Init(Scene _this, SceneParam param)
        {
            _this.mMonsterCount = 0;
            _this.TableSceneData = Table.GetScene(_this.TypeId);
            _this.mSceneWidth = _this.TableSceneData.TerrainHeightMapWidth;
            _this.mSceneHeight = _this.TableSceneData.TerrainHeightMapLength;
            _this.ZoneSideLength = _this.TableSceneData.SeeArea;
            if (_this.ZoneSideLength <= 0)
            {
                _this.ZoneSideLength = Scene.ZONE_SIDE;
                Logger.Error("Error TableSceneData.SeeArea={0}", _this.TableSceneData.SeeArea);
            }
            _this.InitZone();

            if (!Scene.SceneObstacles.TryGetValue(_this.TypeId, out _this.Obstacle))
            {
                _this.Obstacle = new SceneObstacle("../Scene/" + _this.TableSceneData.ResName + ".path");
                if (null == _this.Obstacle)
                {
                    Logger.Fatal("!!!!!!!!null == Obstacle[{1}]", "../Scene/" + _this.TableSceneData.ResName + ".path");
                }
            }

            _this.InitNPC();
            InitMapNPC(_this);
            InitTriggerArea(_this);
            _this.Active = true;
            _this.Param = param;

            PlayerLog.WriteLog((int) LogType.SceneInfo, "Create Scene OK [{0}][{1}][{2}]", _this.TypeId, _this.ServerId,
                _this.Guid);

			SceneServerMonitor.SceneTotalNumber.Increment();

            return true;
        }

        public void AddCollider(Scene _this, ObjCharacter obj, float radius, Action<uint> callback)
        {
            var co = CoroutineFactory.NewCoroutine(AddColliderImpl, _this, obj, radius, callback);
            co.MoveNext();
        }

        public void SetLodeInfo(Scene _this, MsgSceneLode info)
        {
            var tb = Table.GetWarFlag(info.FlagId);
            if (tb == null)
                return;
            _this.LodeInfo = info;
            if (_this.resetTrigger != null)
            {
                SceneServerControl.Timer.DeleteTrigger(_this.resetTrigger);                
            }
            DateTime st = (new DateTime(1970, 1, 1, 0, 0, 0)).AddSeconds(info.ResetTime);
                //DateTime.Now.Date.AddHours((int) (tb.BelongToTime[0]/100)).AddMinutes(tb.BelongToTime[0]%100);
            
            _this.resetTrigger = SceneServerControl.Timer.CreateTrigger(st,()=>{_this.LodeInfo.TeamId = 0;
                                                                                   _this.OwnerAllianceId = 0;
                                                                                   _this.OwnerAllianceName = "";
                                                                                _this.LodeInfo.TeamName = "";
                                                                                _this.BroadCastLodeInfo();});
            

        }
        public void RemoveCollider(Scene _this, uint id)
        {
            PathManager.RemoveCollider(_this.Obstacle, id);
        }

        public bool FindPathTo(Scene _this, ObjCharacter obj, Vector2 target, Action<List<Vector2>> callback)
        {
            var co = CoroutineFactory.NewCoroutine(FindPathToImpl, _this, obj, target, callback);
            co.MoveNext();
            return (bool) co.Current;
        }

        public bool FindPathTo(Scene _this,
                               Coroutine co,
                               ObjCharacter obj,
                               Vector2 target,
                               AsyncReturnValue<List<Vector2>> result)
        {
            Interlocked.Increment(ref SceneServer.Instance.ServerControl.PathFindingCount);
            SceneServer.Instance.ServerControl.PathFindingTimer.Start();

            var start = _this.FindNearestValidPosition(obj.GetPosition()).GetValueOrDefault();
            var end = _this.FindNearestValidPosition(target).GetValueOrDefault();

            var ret = PathManager.FindPath(co, _this.Obstacle, obj, start, end, result);

            SceneServer.Instance.ServerControl.PathFindingTimer.Stop();

            return ret;
        }

        public bool Raycast(Scene _this, ObjCharacter obj, Vector2 target, Action<Vector2> callback)
        {
            var co = CoroutineFactory.NewCoroutine(RaycastImpl, _this, obj, target, callback);
            co.MoveNext();
            return (bool) co.Current;
        }

        public bool Raycast(Coroutine co,
                            Scene _this,
                            ObjCharacter obj,
                            Vector2 target,
                            AsyncReturnValue<Vector2> result)
        {
            return PathManager.Raycast(co, _this.Obstacle, obj, obj.GetPosition(), target, result);
        }

        /// <summary>
        ///     判断当前点是否合法
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool ValidPosition(Scene _this, Vector2 pos)
        {
            if (_this.Obstacle == null)
            {
                return false;
            }
            return _this.Obstacle.GetObstacleValue(pos.X, pos.Y) != SceneObstacle.ObstacleValue.Obstacle;
        }

        public bool ValidPath(Scene _this, Vector2 currentPos, IEnumerable<Vector2> path)
        {
            const float step = 3.0f;
            foreach (var p in path)
            {
                while ((currentPos - p).Length() > step)
                {
                    currentPos += Vector2.Normalize(p - currentPos)*step;
                    if (!_this.ValidPosition(currentPos))
                    {
                        if (!_this.FindNearestValidPosition(currentPos, 0.8f).HasValue)
                        {
                            return false;
                        }
                    }
                }

                currentPos = p;
                if (!_this.ValidPosition(currentPos))
                {
                    if (!_this.FindNearestValidPosition(currentPos, 0.8f).HasValue)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public SceneObstacle.ObstacleValue GetObstacleValue(Scene _this, float x, float y)
        {
            if (_this.Obstacle == null)
            {
                return SceneObstacle.ObstacleValue.Obstacle;
            }
            return _this.Obstacle.GetObstacleValue(x, y);
        }

        /// <summary>
        ///     找离pos最近的合法的路径点
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="pos">当前点</param>
        /// <param name="maxDist">搜索半径</param>
        /// <returns>如果太远没找到，则返回null</returns>
        public Vector2? FindNearestValidPosition(Scene _this, Vector2 pos, float maxDist = 5.0f)
        {
            if (_this.ValidPosition(pos))
            {
                return pos;
            }

            var dist = ((int) (maxDist*2) + 2);
            var d = 1;
            var step = 2;
            while (step < dist)
            {
                var p = pos + Scene.Corner*d;
                for (var j = 0; j < 4; j++)
                {
                    for (var i = 0; i < step; i++)
                    {
                        p += Scene.Dir[j];

                        if (_this.ValidPosition(p))
                        {
                            return p;
                        }
                    }
                }

                step += 2;
                d++;
            }

            return null;
        }

        //心跳
        public virtual void Tick(Scene _this, float delta)
        {
            //if (!Active) return;

            //add
            foreach (var pair in _this.mNeedToAddObjDict)
            {
                try
                {
                    _this.EnterScene(pair.Value);
                }
                catch (Exception e)
                {
                    Logger.Fatal(e);
                }
            }
            _this.mNeedToAddObjDict.Clear();


            //remove
            foreach (var pair in _this.mNeedToRemoveObjDict)
            {
                try
                {
                    _this.LeaveScene(pair.Value);
                }
                catch (Exception e)
                {
                    Logger.Fatal(e);
                }
            }
            _this.mNeedToRemoveObjDict.Clear();

            //tick
            _this.mTicking = true;
            foreach (var pair in _this.mObjDict)
            {
                if (!pair.Value.IsCharacter())
                {
                    continue;
                }

                try
                {
                    pair.Value.Tick(delta);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "obj[{0}]", pair.Value.ObjId);
                    pair.Value.Active = false; //出现异常了，没办法先把他disable吧
                }
            }
            _this.mTicking = false;

            foreach (var trigger in _this.mAreaDict)
            {
                try
                {
                    trigger.Value.Tick(delta);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }

            var hasSyncData = false;
            if (DateTime.Now > _this.NextSyncTime)
            {
                foreach (var zone in _this.mZoneList)
                {
                    _this.mSyncData.Datas.Clear();
                    foreach (var obj in zone.ObjDict)
                    {
                        if (obj.Value.IsCharacter() && obj.Value.Active)
                        {
                            _this.mSyncData.Datas.AddRange(((ObjCharacter) obj.Value).GetSyncData());
                        }
                    }

                    if (_this.mSyncData.Datas.Count > 0)
                    {
                        SceneServer.Instance.ServerControl.SyncDataToClient(zone.EnumAllVisiblePlayerId(),
                            _this.mSyncData);
                    }
                }
                _this.NextSyncTime = DateTime.Now + Scene.MinSyncTimeSpan;
                hasSyncData = true;
            }

            // 避免同一帧干太多事情，如果同步了数据，那下一帧再同步位置吧
            if (!hasSyncData && DateTime.Now > _this.NextSyncPathPosTime)
            {
                foreach (var zone in _this.mZoneList)
                {
                    _this.mSyncPathPos.Data.Clear();
                    foreach (var obj in zone.ObjDict)
                    {
                        if ((obj.Value.GetObjType() == ObjType.NPC /* || obj.Value.GetObjType() == ObjType.PLAYER*/) &&
                            obj.Value.Active)
                        {
                            //if (obj.Value.GetObjType() == ObjType.PLAYER)
                            //{
                            //    var speed = ((ObjPlayer) obj.Value).GetMoveSpeed();
                            //    var sppedSqr = speed*speed;
                            //    var player = ((ObjPlayer) obj.Value);
                            //    if (player.mTargetPos.Count == 0 ||
                            //        (player.GetPosition() - player.mTargetPos.Last()).LengthSquared() < sppedSqr)
                            //    {
                            //        continue;
                            //    }
                            //}

                            var npc = (ObjCharacter) obj.Value;
                            if (npc.mIsMoving)
                            {
                                _this.mSyncPathPos.Data.Add(new SyncPathPos
                                {
                                    ObjId = obj.Value.ObjId,
                                    X = obj.Value.GetPosition().X,
                                    Y = obj.Value.GetPosition().Y,
                                    Index = (uint) ((ObjCharacter) obj.Value).mTargetPos.Count
                                });
                            }
                        }
                    }

                    if (_this.mSyncPathPos.Data.Count > 0)
                    {
                        SceneServer.Instance.ServerControl.SyncObjPosition(zone.EnumAllVisiblePlayerId(),
                            _this.mSyncPathPos);
                    }
                }
                _this.NextSyncPathPosTime = DateTime.Now + Scene.MinSyncPathPosTimeSpan;
            }
        }

        //判断是否满了
        public bool IsFull(Scene _this)
        {
            if (_this.mObjDict.Count > _this.TableSceneData.PlayersMaxA)
            {
                return true;
            }
            return false;
        }

        //提出某个玩家 不要再Tick中调用
        public void KickPlayer(Scene _this, ulong id)
        {
            if (_this.mTicking)
            {
                Logger.Error("Scene.KickPlayer({0}):  Can not call this in Scene.Tick", id);
                return;
            }

            ObjPlayer player = null;
            if (!_this.mPlayerDict.TryGetValue(id, out player))
            {
                Logger.Fatal("Scene.KickPlayer({0}):  Can not find player", id);
                return;
            }

            Logger.Info("Scene.KickPlayer({0})--------------------begin", id);

            try
            {
                SceneServer.Instance.LoginAgent.KickCharacter(player.Proxy.ClientId, 0);
                _this.LeaveScene(player);
            }
            catch (Exception e)
            {
                Logger.Fatal(e);
            }

            Logger.Info("Scene.KickPlayer({0})--------------------end", id);
        }

        //踢出所有玩家 不要再Tick中调用
        public void KickAllPlayer(Scene _this)
        {
            if (_this.mTicking)
            {
                Logger.Error("Scene.KickAllPlayer():  Can not call this in Scene.Tick");
                return;
            }

            var dic = new Dictionary<ulong, ObjPlayer>(_this.mPlayerDict);

            foreach (var pair in dic)
            {
                try
                {
                    _this.KickPlayer(pair.Key);
                }
                catch (Exception e)
                {
                    Logger.Fatal(e, "Scene:KickAllPlayer");
                }
            }
            dic.Clear();

            Logger.Info("Scene--------------KickAllPlayer--------------");
        }

        //移除所有obj 不要再Tick中调用
        public void RemoveAllObj(Scene _this)
        {
            var back = new Dictionary<ulong, ObjBase>(_this.mObjDict);
            foreach (var pair in back)
            {
                pair.Value.LeavelScene();
                _this.RemoveObj(pair.Value);
            }

            _this.mObjDict.Clear();
            _this.mPlayerDict.Clear();
            _this.mNeedToAddObjDict.Clear();
            _this.mNeedToRemoveObjDict.Clear();
        }

        //清除场景内所有敌人和怪物
        public void RemoveAllNPC(Scene _this)
        {
            var back = new Dictionary<ulong, ObjBase>(_this.mObjDict);
            List<ObjBase> npc=new List<ObjBase> ();
            foreach (var pair in back)
            {
                if (pair.Value.GetObjType() == ObjType.NPC)
                {
                    npc.Add(pair.Value);   
                }    
            }
            foreach (var npcRemove in npc)
            {
                _this.LeaveScene(npcRemove);
            }
        }

        //销毁当前场景 不要再Tick中调用
        public void Destroy(Scene _this)
        {
            _this.Active = false;
            try
            {
                _this.KickAllPlayer();
                _this.RemoveAllObj();

                _this.OnDestroy();
                foreach (var zone in _this.mZoneList)
                {              
                    zone.Reset();
                }
                _this.mZoneList.Clear();

                PathManager.DisposeObstacle(_this.Obstacle);
            }
            catch (Exception e)
            {
                Logger.Fatal(e, "Scene:Destroy");
            }

            //Logger.Info("Scene--------------Destroy--------------");
            PlayerLog.WriteLog((int) LogType.SceneInfo, "Destroy Scene OK [{0}][{1}][{2}]", _this.TypeId, _this.ServerId,
                _this.Guid);

			SceneServerMonitor.SceneTotalNumber.Decrement();
        }

        //添加Obj
        public void AddObj(Scene _this, ObjBase obj)
        {
            _this.mObjDict.Add(obj.ObjId, obj);
            if (obj.GetObjType() == ObjType.PLAYER)
            {
                var player = obj as ObjPlayer;
                _this.mPlayerDict.Add(obj.ObjId, player);
                try
                {
                    _this.OnPlayerEnter(player);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }

				SceneServerMonitor.PlayerNumber.Increment();
            }

			SceneServerMonitor.ObjNumber.Increment();
        }

        public void RemoveObj(Scene _this, ObjBase obj)
        {
            if (obj == null)
            {
                return;
            }

	        bool isPlayer = obj.GetObjType() == ObjType.PLAYER;

            obj.Destroy();
            _this.mObjDict.Remove(obj.ObjId);
            _this.mPlayerDict.Remove(obj.ObjId);
			/*
            if (obj.GetObjType() == ObjType.PLAYER)
            {
                var player = obj as ObjPlayer;
                try
                {
                    _this.OnPlayerLeave(player);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
            else if (obj.GetObjType() == ObjType.NPC)
            {
                if (_this.SpeMonsterToWorldBossId.ContainsKey(obj))
                {
                    Log(_this, Logger, "Special monster killed!Type = {0}", obj.TypeId);
                    var worldBossId = _this.SpeMonsterToWorldBossId[obj];
                    --_this.SpeMonsterAlwaysRecords[worldBossId];
                    _this.SpeMonsterToWorldBossId.Remove(obj);
                }
            }
			 * */

			SceneServerMonitor.ObjNumber.Decrement();
	        if (isPlayer)
	        {
				SceneServerMonitor.PlayerNumber.Decrement();    
	        }
        }

        public ObjBase FindObj(Scene _this, ulong objId)
        {
            ObjBase obj = null;
            if (_this.mObjDict.TryGetValue(objId, out obj))
            {
                return obj;
            }
            return null;
        }

        public ObjCharacter FindCharacter(Scene _this, ulong objId)
        {
            ObjBase character = null;
            if (_this.mObjDict.TryGetValue(objId, out character))
            {
                if (character.IsCharacter())
                {
                    return character as ObjCharacter;
                }
            }
            return null;
        }

        public ObjPlayer FindPlayer(Scene _this, ulong objId)
        {
            ObjPlayer player = null;
            if (_this.mPlayerDict.TryGetValue(objId, out player))
            {
                return player;
            }
            return null;
        }

        public int GetAlivePlayerNum(Scene _this)
        {
            return _this.mPlayerDict.Count(obj => !obj.Value.IsDead());
        }

        public IEnumerable<ObjPlayer> EnumAllPlayer(Scene _this)
        {
            foreach (var obj in _this.mPlayerDict)
            {
                yield return obj.Value;
            }
        }

        public IEnumerable<ulong> EnumAllPlayerId(Scene _this)
        {
            foreach (var obj in _this.mPlayerDict)
            {
                yield return obj.Value.ObjId;
            }
        }

        public void PushActionToAllPlayer(Scene _this, Action<ObjPlayer> action)
        {
            foreach (var obj in _this.mPlayerDict)
            {
                try
                {
                    if (null == obj.Value || null == obj.Value.Proxy)
                        continue;
                    action(obj.Value);
                }
                catch (Exception e)
                {
                    Logger.Fatal("player[{0}]:[{1}]", obj.Key, e);
                }
            }
        }

        public void PushActionToAllObj(Scene _this, Action<ObjBase> action)
        {
            foreach (var obj in _this.mObjDict)
            {
                action(obj.Value);
            }
        }

        public virtual void OnCreate(Scene _this)
        {
            _this.dicGetRewardPlayers.Clear();
            _this.IsLodeMap = false;
            _this.OwnerAllianceId = -1;
            _this.OwnerAllianceName = "";
            _this.AdditionLode = 1.0f;
            _this.AdditionExp = 1.0f;
            _this.LodeInfo = null;
            _this.IsOnlineDamage = false;

            Table.ForeachWarFlag(tb =>
            {
                if (tb.FlagInMap == _this.TypeId)
                {
                    _this.IsLodeMap = true;
                    _this.HoldTimeBegin = DateTime.Now.Date.AddHours(Math.Ceiling((double)tb.BelongToTime[0]/100)).AddMinutes(tb.BelongToTime[0]%100);
                    _this.HoldTimeEnd = DateTime.Now.Date.AddHours(Math.Ceiling((double)tb.BelongToTime[1] / 100)).AddMinutes(tb.BelongToTime[1] % 100);
                    _this.AdditionExp = (float)(10000+tb.EXPAdd)/10000.0f;
                    _this.AdditionLode = (float) (10000 + tb.MiningAdd)/10000.0f;
                    CoroutineFactory.NewCoroutine(_this.ApplyLodeInfo).MoveNext();
                    return false;
                }
                return true;
            });

        }
        public IEnumerator ApplyLodeInfo(Coroutine co,Scene _this)
        {
            var msg = SceneServer.Instance.TeamAgent.ApplyHoldLode(0,_this.ServerId, _this.TypeId);
            yield return msg.SendAndWaitUntilDone(co);
            if (msg.State != MessageState.Reply)
            {
                LogError(_this, Logger, "ApplyLodeInfo error. {0}, message not replied.", _this.ServerId);
            }
            else
            {
                //msg.Response
                _this.OwnerAllianceId = msg.Response.TeamId;
                _this.OwnerAllianceName = msg.Response.TeamName;
                SetLodeInfo(_this,msg.Response);
                //_this.LodeInfo = msg.Response;
            }
            yield break;
        }

        public void BroadCastLodeInfo(Scene _this)
        {
            foreach (var player in _this.EnumAllPlayer())
            {
                player.Proxy.NotifyLodeInfo(_this.LodeInfo);
            }
        }
        //当Obj进入场景前，(Obj还没有进入场景，他的Scene是空)这里只可以写场景逻辑(例如改个坐标)，不可以同步数据
		public virtual void OnObjBeforeEnterScene(Scene _this, ObjBase obj)
        {
			if (obj.GetObjType() == ObjType.PLAYER)
			{
				if (null != _this.TableSceneData)
				{
					var player = obj as ObjPlayer;
					if (player.TypeId >= 0 && player.TypeId < _this.TableSceneData.AddBuff.Length)
					{
						var id = _this.TableSceneData.AddBuff[player.TypeId];
						if (-1 != id)
						{
							player.AddBuff(id, 1, player);			
						}
					}
				}
			}
        }

        //这里只可以写场景逻辑，不可以同步数据，因为这时候客户端有可能还没加载好
        public virtual void OnNpcEnter(Scene _this, ObjNPC npc)
        {
        }

        public virtual void OnNpcDamage(Scene _this, ObjNPC npc, int damage, ObjBase enemy)
        {
        }

		public virtual void OnPlayerExDataChanged(Scene _this, ObjPlayer npc, int idx, int val)
        {
        }
		
        //这里只可以写场景逻辑，不可以同步数据，因为这时候客户端有可能还没加载好
        public virtual void OnPlayerEnter(Scene _this, ObjPlayer player)
        {
            if (_this.mDelTrigger != null)
            {
                DeleteTimer(_this, _this.mDelTrigger);
                _this.mDelTrigger = null;
            }
        }

        public virtual void OnPlayerLeave(Scene _this, ObjPlayer player)
        {
            //如果场景里没有人了，过三分钟把场景删除掉
            if (_this.PlayerCount == 0)
            {
                LateDeleteScene(_this, 60*3, RemoveScene.NoPlayer);
            }
        }

        //这里可以同步一些数据，这时候客户端已经都ok了
        public virtual void OnPlayerEnterOver(Scene _this, ObjPlayer player)
        {
            if (player.Proxy != null)
            {
                player.Proxy.NotifyNpcStatus(_this.MapNpcInfo);
                if (_this.IsLodeMap)
                {
                    player.Proxy.NotifyLodeInfo(_this.LodeInfo);
                }
            }
        }

        public virtual void RefreshLodeTimer(Scene _this)
        {
            if (_this.IsLodeMap == false)
                return;
            foreach (var v in _this.LodeInfo.LodeList)
            {
                var tb = Table.GetLode(v.Key);
                if (tb != null)
                {
                    v.Value.Times = tb.CanCollectNum;
                    v.Value.UpdateTime = 0;
                }
            }
            BroadCastLodeInfo(_this);
        }
        public virtual void AfterPlayerEnterOver(Scene _this, ObjPlayer player)
        {
            
        }
        public virtual void OnPlayerDie(Scene _this, ObjPlayer player, ulong characterId = 0)
        {
            var obj = FindCharacter(_this,characterId);

            if (obj == null)
            {
                LogError(_this, Logger, "OnPlayerDie error. {0}, FindCharacter==null,characterId={1}.", _this.ServerId, characterId);
                return;
            }
            var killer = obj.GetRewardOwner();
            if (killer == null)
            {
                LogError(_this, Logger, "OnPlayerDie error. {0}, GetRewardOwner==null.", _this.ServerId);
                return;
            }
            if (killer.GetObjType() != ObjType.PLAYER)
            {
                return;
            }
            var pl = killer as ObjPlayer;
            if (pl == null)
                return;
            var dict = new Dict_int_int_Data();
            dict.Data.Add(951, 1);
            pl.SendExDataChange(dict);
        }

		public virtual void OnPlayerRelive(Scene _this, ObjPlayer player, bool byItem)
	    {
		    
	    }

        public virtual void OnNpcDie(Scene _this, ObjNPC npc, ulong characterId = 0)
        {
        }
        public virtual void OnNpcRespawn(Scene _this, ObjNPC npc)
        {
        }
        public virtual void OnPlayerPickUp(Scene _this, ulong objId,int itemId,int count)
        {

        }
        public virtual void ExitDungeon(Scene _this, ObjPlayer player)
        {
        }

        public virtual void OnDestroy(Scene _this)
        {
            foreach (var timer in _this.Timers)
            {
                SceneServerControl.Timer.DeleteTrigger(timer);
            }
            _this.Timers.Clear();
        }

        public void LateDeleteScene(Scene _this, int waitSec, RemoveScene removeType)
        {
            var tarTime = DateTime.Now.AddSeconds(waitSec);
            if (_this.mDelTrigger == null)
            {
                CreateTimer(_this, tarTime, () =>
                {
                    _this.mDelTrigger = null;
                    if (_this.PlayerCount == 0)
                    {
                        CoroutineFactory.NewCoroutine(_this.TryDeleteScene, removeType).MoveNext();
                    }
                });
            }
            else
            {
                SceneServerControl.Timer.ChangeTime(ref _this.mDelTrigger, tarTime);
            }
        }

        public IEnumerator TryDeleteScene(Coroutine co, Scene _this, RemoveScene removeType)
        {
            _this.mDelTrigger = null;
            for (var i = 0; i < 5; ++i)
            {
                var msg = SceneServer.Instance.SceneAgent.SBDestroyScene(0, _this.Guid);
                yield return msg.SendAndWaitUntilDone(co);

                if (msg.State != MessageState.Reply)
                {
                    LogError(_this, Logger, "Delete scene error. {0}, message not replied.", _this.ServerId);
                }
                else
                {
                    if (msg.Response == 1)
                    {
                        Log(_this, Logger, "Delete scene successful. {0}", _this.ServerId);

                        OnDestroy(_this);
                        SceneManager.Instance.RemoveScene(_this, removeType);
                        yield break;
                    }
                    Log(_this, Logger, "Delete scene error. {0}", _this.ServerId);
                }

                //如果删除失败，那么5秒后再试一次
                yield return SceneServer.Instance.ServerControl.Wait(co, TimeSpan.FromSeconds(5*(i + 1)));
            }
        }

        public void BroadcastSceneAction(Scene _this, int actionId)
        {
            var tbArea = Table.GetTriggerArea(actionId);
            if (tbArea == null)
            {
                return;
            }
            _this.BroadcastSceneAction(tbArea);
        }

        public void BroadcastSceneAction(Scene _this, TriggerAreaRecord tbArea)
        {
            SceneServer.Instance.ServerControl.NotifySceneAction(_this.EnumAllPlayerId(), tbArea.ClientAnimation);
        }

        public virtual void DealWith(Scene _this, string name, object param)
        {
        }

        public Trigger CreateTimer(Scene _this, DateTime targetTime, Action act, int interval = -1)
        {
            Trigger timer = null;
            timer = SceneServerControl.Timer.CreateTrigger(targetTime, () =>
            {
                _this.Timers.Remove(timer);
                if (act != null)
                {
                    act();
                }
            }, interval);
            _this.Timers.Add(timer);
            return timer;
        }

        public void DeleteTimer(Scene _this, Trigger trigger)
        {
            SceneServerControl.Timer.DeleteTrigger(trigger);
            _this.Timers.Remove(trigger);
        }

        public bool CanPk(Scene _this)
        {
            return true;
        }

        public ReasonType GetBornVisibleType()
        {
            return ReasonType.VisibilityChanged;
        }

        public void Log(Scene _this, Logger logger, string text, params object[] args)
        {
            logger.Info("SceneId[" + _this.TypeId + "] " + "Guid[" + _this.Guid + "] " + text, args);
        }

        public void LogError(Scene _this, Logger logger, string text, params object[] args)
        {
            logger.Error("SceneId[" + _this.TypeId + "] " + "Guid[" + _this.Guid + "] " + text, args);
        }

        public string DumpInfo(Scene _this)
        {
            var info = "--------------Scene---------" + _this.BeginTime + "\n";
            info += string.Format("ServerId=[{0}],TypeId=[{1}],Guid=[{2}]\n", _this.ServerId, _this.TypeId, _this.Guid);
            info += string.Format("ObjCount=[{0}],PlayerCount=[{1}]\n", _this.mObjDict.Count, _this.PlayerCount);
            info += "------------------------Scene------------------------";
            Logger.Warn(info);
            return info;
        }

        //主动请求切换场景
        public IEnumerator ChangeSceneCoroutine(Coroutine co, ObjCharacter character, int scneneId, int x, int y)
        {
            //主动请求切换场景,根据合服ID进行
            var serverLogicId = SceneExtension.GetServerLogicId(character.ServerId);
            var sceneInfo = new ChangeSceneInfo
            {
                SceneId = scneneId,
                ServerId = serverLogicId,
                SceneGuid = 0,
                Type = (int)eScnenChangeType.Position
            };
            sceneInfo.Guids.Add(character.ObjId);
            sceneInfo.Pos = new SceneParam();
            sceneInfo.Pos.Param.Add(x);
            sceneInfo.Pos.Param.Add(y);
            var msgChgScene = SceneServer.Instance.SceneAgent.SBChangeSceneByTeam(character.ObjId, sceneInfo);
            yield return msgChgScene.SendAndWaitUntilDone(co, TimeSpan.FromSeconds(30));
        }

        // 自动复活（代码是从ObjPlayer移到这里的）
        public virtual void AutoRelive(Scene _this, ObjPlayer player)
        {
            if (player == null)
                return;

            var tbScene = _this.TableSceneData;
            if (tbScene == null)
            {
                return;
            }

            var gotoSceneID = _this.TypeId;
            if (tbScene.ReliveType[1] == 0)
            {
                var pos = new Vector2((float)tbScene.Entry_x, (float)tbScene.Entry_z);
                pos = Utility.RandomMieShiEntryPosition((eSceneType)tbScene.Type, gotoSceneID, pos);
                player.SetPosition(pos);
            }
            else
            {
                var tbCityScene = Table.GetScene(tbScene.CityId);
                if (tbCityScene != null)
                {
                    if (gotoSceneID != tbScene.CityId)
                    {
                        CoroutineFactory.NewCoroutine(ChangeSceneCoroutine, player, tbScene.CityId,
                            (int)tbCityScene.Entry_x, (int)tbCityScene.Entry_z).MoveNext();
                        return;
                    }
                    var pos = new Vector2((float)tbCityScene.Entry_x, (float)tbCityScene.Entry_z);
                    pos = Utility.RandomMieShiEntryPosition((eSceneType)tbCityScene.Type, gotoSceneID, pos);
                    player.SetPosition(pos);
                }
            }

            player.Relive();
        }

        #region 动态场景难度

        //伤害修正
        public int PlayerDoDamageModify(Scene _this, int value)
        {
            return (int) (value*_this.BeDamageModify);
        }

        public int NpcDoDamageModify(Scene _this, int value)
        {
            return (int) (value*_this.DoDamageModify);
        }

        public ObjNPC IsNeedChangeHp(ObjBase mObj)
        {
            var n = mObj as ObjNPC;
            if (n == null) //不是怪物
            {
                return null;
            }
            var c = mObj as ObjRetinue;
            if (c != null)
            {
                if (!(c.Owner is ObjPlayer))
                {
                    return null;
                }
            }
            return n;
        }

        public void ChangeDifficulty(Scene _this, int count)
        {
            if (!_this.isNeedDamageModify)
            {
                return;
            }
            var difficultyIdx = count - 1;
            var oldDoDamageModify = _this.mDoDamageModify;
            if (difficultyIdx < 1)
            {
                _this.mDoDamageModify = 1.0f;
                _this.mBeDamageModify = 1.0f;
            }
            else
            {
                difficultyIdx = Math.Min(9, difficultyIdx);
                _this.mDoDamageModify = Scene.fModifyDoDamage[difficultyIdx];
                _this.mBeDamageModify = Scene.fModifyBeDamage[difficultyIdx];
            }
            if (Math.Abs(oldDoDamageModify - _this.mDoDamageModify) < 0.01f)
            {
                return;
            }
            _this.PushActionToAllObj(mObj =>
            {
                var n = Scene.IsNeedChangeHp(mObj);
                if (n != null)
                {
                    n.Attr.OnPropertyChanged((uint) eSceneSyncId.SyncHpMax);
                    n.Attr.OnPropertyChanged((uint) eSceneSyncId.SyncHpNow);
                }
            });
        }

        #endregion

        #region Zone

        //Zone示意图
        /*
		|  mSceneWidth  |
		-----------------           --
		|	|	|	|	|ZONE_SIDE
		-----------------
		|	|	|	|	|
		-----------------			mSceneHeight
		|	|	|	|	|
		-----------------			--
		
		 
		  
		  
		----------------- line 0
		
		----------------- line 1
		
		----------------- line 2
		
		----------------- line 3

		
	column0 column1	  column2  column3
		|		|		|		|
		 		 		 	
		|		|		|		|
		 		 		 	
		|		|		|		|
		 		 		 	
		|		|		|		|
			
		 * 
		*/

        public int Pos2ZoneId(Scene _this, float x, float y)
        {
            return ((int) (x/_this.ZoneSideLength)) + (int) (y/_this.ZoneSideLength)*_this.mZoneColumnCount;
        }

        public Zone GetZone(Scene _this, int id)
        {
            if (id < 0 || id >= _this.mZoneList.Count)
            {
                return null;
            }
            return _this.mZoneList[id];
        }

        //初始化Zone
        public bool InitZone(Scene _this)
        {
            //计算场景内zone行和列
            _this.mZoneLineCount = (int) Math.Ceiling(_this.mSceneHeight/(double) _this.ZoneSideLength);
            _this.mZoneColumnCount = (int) Math.Ceiling(_this.mSceneWidth/(double) _this.ZoneSideLength);

            //创建zone
            for (var i = 0; i < _this.mZoneLineCount; i++)
            {
                for (var j = 0; j < _this.mZoneColumnCount; j++)
                {
                    var id = i*_this.mZoneColumnCount + j;
                    var zone = new Zone(_this, id, j*_this.ZoneSideLength, i*_this.ZoneSideLength, _this.ZoneSideLength,
                        _this.ZoneSideLength);
                    _this.mZoneList.Add(zone);
                }
            }

            //最大周围多少个格子被可见 MaxNearByZoneNumber*MaxNearByZoneNumber 个
            const int MaxNearByZoneNumber = 3;
            //可见格子的起始偏移
            const int Offset = MaxNearByZoneNumber/2;

            //把周围可见格子加入列表
            /*
			 *   -------------
			 * 	 |0	|1	|2	|
			 * 	 -------------
			 * 	 |3	|4	|5	|
			 * 	 -------------
			 * 	 |6	|7	|8	|
			 *	 -------------
			 */

            //0的可见格子为 0134
            //4的可见格子为 0123456789
            //7的可见格子为 345678

            //遍历所有格子行
            for (var i = 0; i < _this.mZoneLineCount; i++)
            {
                //遍历所有格子列
                for (var j = 0; j < _this.mZoneColumnCount; j++)
                {
                    //当前要判断可见性的格子id
                    var id = i*_this.mZoneColumnCount + j;
                    var zone = _this.mZoneList[id];

                    //遍历周围 MaxNearByZoneNumber*MaxNearByZoneNumber个格子
                    //判断[x,y]这个格子是否出界

                    //行
                    for (var n = 0; n < MaxNearByZoneNumber; n++)
                    {
                        var y = i - Offset + n;
                        if (y < 0 || y >= _this.mZoneLineCount)
                        {
                            continue; //y出界
                        }

                        //列
                        for (var m = 0; m < MaxNearByZoneNumber; m++)
                        {
                            var x = j - Offset + m;
                            if (x < 0 || x >= _this.mZoneColumnCount)
                            {
                                continue; //x出界
                            }

                            //算当前格子id
                            var visibleZoneId = y*_this.mZoneColumnCount + x;

                            //if (visibleZoneId == id) continue;//是自己格子也要加入，自己属于自己可见格子

                            zone.AddVisibleZone(_this.mZoneList[visibleZoneId]);
                        }
                    }
                }
            }

            return true;
        }

        #endregion

        #region 事件区域

        private void InitTriggerArea(Scene _this)
        {
            Table.ForeachTriggerArea(table =>
            {
                if (table.SceneId != _this.TypeId)
                {
                    return true;
                }

                var area = new TriggerArea(table.Id, _this);
                _this.mAreaDict.Add(table.Id, area);

                return true;
            });
        }

        public virtual void OnCharacterEnterArea(Scene _this, int areaId, ObjCharacter character)
        {
            //Logger.Info("[{0}] enter area[{1}]", character.GetName(), areaId);
        }

        public virtual void OnAreaTick(Scene _this, int areaId, IEnumerator<ObjCharacter> enumerator)
        {
        }

        public virtual void OnCharacterLeaveArea(Scene _this, int areaId, ObjCharacter character)
        {
            //Logger.Info("[{0}] leave area[{1}]", character.GetName(), areaId);
        }

        #endregion
    }

    public partial class Scene : IScriptScene
    {
        public static Vector2 Corner = new Vector2(0.5f, 0.5f);

        public static Vector2[] Dir =
        {
            new Vector2(-0.5f, 0),
            new Vector2(0f, -0.5f),
            new Vector2(0.5f, 0),
            new Vector2(0f, 0.5f)
        };

        private static IScene mImpl;
        public static TimeSpan MinSyncPathPosTimeSpan = TimeSpan.FromSeconds(5);
        public static TimeSpan MinSyncTimeSpan = TimeSpan.FromSeconds(1);
        //场景Obj
        public static Dictionary<int, SceneObstacle> SceneObstacles = new Dictionary<int, SceneObstacle>();

        static Scene()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (Scene), typeof (SceneDefaultImpl),
                o => { mImpl = (IScene) o; });
        }

        public Scene()
        {
            BeginTime = DateTime.Now;
        }
        public Dictionary<ulong, int> dicGetRewardPlayers = new Dictionary<ulong, int>(); 

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public MapNpcInfos MapNpcInfo = new MapNpcInfos();
        public List<MapTransferRecord> MapNpcRecords;
        public SceneSyncData mSyncData = new SceneSyncData();
        public SyncPathPosMsg mSyncPathPos = new SyncPathPosMsg();
        public DateTime NextSyncPathPosTime = DateTime.Now;
        public DateTime NextSyncTime = DateTime.Now;
        public SceneObstacle Obstacle;
        public SceneParam Param;
        public int OwnerAllianceId;
        public string OwnerAllianceName;
        public DateTime HoldTimeBegin = DateTime.Now;
        public DateTime HoldTimeEnd   = DateTime.Now;
        public float AdditionExp = 1.0f;
        public float AdditionLode = 1.0f;
        public bool IsLodeMap = false;
        public MsgSceneLode LodeInfo;
        public Trigger resetTrigger;
        public bool IsOnlineDamage = false;
        

        //用来保存死后刷新的 worldBOSS 表 id
        public List<int> SpeMonsterAfterDieRecordIds = new List<int>();
        //worldBOSS 表 id => 当前存在的数量, 用来保存一直刷新的 worldBOSS 表 id 对应的存在数量
        public Dictionary<int, int> SpeMonsterAlwaysRecords = new Dictionary<int, int>();
        //ObjBase => worldBOSS 表 id, 用来保存活着的 特殊怪物 所对应的 WorldBOSS 表 id
        public Dictionary<ObjBase, int> SpeMonsterToWorldBossId = new Dictionary<ObjBase, int>();
        //Timer管理器，防止Timer泄露
        public List<Trigger> Timers = new List<Trigger>();

        public Dictionary<int, TriggerArea> AreaDict
        {
            get { return mAreaDict; }
        }

        public void AddCollider(ObjCharacter obj, float radius, Action<uint> callback)
        {
            mImpl.AddCollider(this, obj, radius, callback);
        }

        public void SetLodeInfo(MsgSceneLode info)
        {
            mImpl.SetLodeInfo(this,info);
        }
        //添加Obj
        public void AddObj(ObjBase obj)
        {
            mImpl.AddObj(this, obj);
        }

        public void BroadcastSceneAction(int actionId)
        {
            mImpl.BroadcastSceneAction(this, actionId);
        }

        public void BroadcastSceneAction(TriggerAreaRecord tbArea)
        {
            mImpl.BroadcastSceneAction(this, tbArea);
        }

        public virtual bool CanPk()
        {
            return mImpl.CanPk(this);
        }

        public virtual ReasonType GetBornVisibleType()
        {
            return mImpl.GetBornVisibleType();
        }

        public Trigger CreateTimer(DateTime targetTime, Action act, int interval = -1)
        {
            return mImpl.CreateTimer(this, targetTime, act, interval);
        }

        public virtual void DealWith(string name, object param)
        {
            mImpl.DealWith(this, name, param);
        }

        public void DeleteTimer(Trigger trigger)
        {
            mImpl.DeleteTimer(this, trigger);
        }

        public IEnumerator ApplyLodeInfo(Coroutine co)
        {
            return mImpl.ApplyLodeInfo(co,this);
        }
        //销毁当前场景 不要再Tick中调用
        public void Destroy()
        {
            mImpl.Destroy(this);
        }

        public string DumpInfo()
        {
            return mImpl.DumpInfo(this);
        }

        public IEnumerable<ObjPlayer> EnumAllPlayer()
        {
            return mImpl.EnumAllPlayer(this);
        }

        public IEnumerable<ulong> EnumAllPlayerId()
        {
            return mImpl.EnumAllPlayerId(this);
        }

        public ObjCharacter FindCharacter(ulong objId)
        {
            return mImpl.FindCharacter(this, objId);
        }

        /// <summary>
        ///     找离pos最近的合法的路径点
        /// </summary>
        /// <param name="pos">当前点</param>
        /// <param name="maxDist">搜索半径</param>
        /// <returns>如果太远没找到，则返回null</returns>
        public Vector2? FindNearestValidPosition(Vector2 pos, float maxDist = 5.0f)
        {
            return mImpl.FindNearestValidPosition(this, pos, maxDist);
        }

        public ObjBase FindObj(ulong objId)
        {
            return mImpl.FindObj(this, objId);
        }

        public bool FindPathTo(ObjCharacter obj, Vector2 target, Action<List<Vector2>> callback)
        {
            return mImpl.FindPathTo(this, obj, target, callback);
        }

        public bool FindPathTo(Coroutine co, ObjCharacter obj, Vector2 target, AsyncReturnValue<List<Vector2>> result)
        {
            return mImpl.FindPathTo(this, co, obj, target, result);
        }

        public ObjPlayer FindPlayer(ulong objId)
        {
            return mImpl.FindPlayer(this, objId);
        }
        public int GetAlivePlayerNum()
        {
            return mImpl.GetAlivePlayerNum(this);
        }

        public static IScene GetImpl()
        {
            return mImpl;
        }

        public SceneObstacle.ObstacleValue GetObstacleValue(float x, float y)
        {
            return mImpl.GetObstacleValue(this, x, y);
        }

        public virtual bool Init(SceneParam param)
        {
            return mImpl.Init(this, param);
        }

        //判断是否满了
        public bool IsFull()
        {
            return mImpl.IsFull(this);
        }

        //踢出所有玩家 不要再Tick中调用
        public void KickAllPlayer()
        {
            mImpl.KickAllPlayer(this);
        }

        //提出某个玩家 不要再Tick中调用
        public void KickPlayer(ulong id)
        {
            mImpl.KickPlayer(this, id);
        }

        protected void LateDeleteScene(int waitSec, RemoveScene removeType)
        {
            mImpl.LateDeleteScene(this, waitSec, removeType);
        }

        public void Log(Logger logger, string text, params object[] args)
        {
            mImpl.Log(this, logger, text, args);
        }

        public void LogError(Logger logger, string text, params object[] args)
        {
            mImpl.LogError(this, logger, text, args);
        }

        public virtual void OnNpcEnter(ObjNPC npc)
        {
            mImpl.OnNpcEnter(this, npc);
        }

        public virtual void OnNpcDamage(ObjNPC obj, int damage, ObjBase enemy)
        {
            mImpl.OnNpcDamage(this, obj, damage, enemy);
        }
		public virtual void OnPlayerExDataChanged(ObjPlayer obj, int idx, int val)
		{
			mImpl.OnPlayerExDataChanged(this, obj, idx, val);
		}
        //Obj进入场景前的逻辑，这时候这个角色还没有进入场景，这里只能做设置坐标，加些buff，不能做同步数据，
        public virtual void OnObjBeforeEnterScene(ObjBase obj)
        {
            mImpl.OnObjBeforeEnterScene(this, obj);
        }

        //这里可以同步一些数据，这时候客户端已经都ok了
        public virtual void OnPlayerEnterOver(ObjPlayer player)
        {
            mImpl.OnPlayerEnterOver(this, player);
        }

        public virtual void RefreshLodeTimer()
        {
            mImpl.RefreshLodeTimer(this);
        }
        public virtual void AfterPlayerEnterOver(ObjPlayer player)
        {
            mImpl.AfterPlayerEnterOver(this, player);
        }
        public virtual void OnPlayerPickItem(ObjPlayer player, ObjDropItem item)
        {
        }

        public void PushActionToAllObj(Action<ObjBase> action)
        {
            mImpl.PushActionToAllObj(this, action);
        }

        public void PushActionToAllPlayer(Action<ObjPlayer> action)
        {
            mImpl.PushActionToAllPlayer(this, action);
        }

        public bool Raycast(ObjCharacter obj, Vector2 target, Action<Vector2> callback)
        {
            return mImpl.Raycast(this, obj, target, callback);
        }

        public bool Raycast(Coroutine co, ObjCharacter obj, Vector2 target, AsyncReturnValue<Vector2> result)
        {
            return mImpl.Raycast(co, this, obj, target, result);
        }

        //移除所有obj 不要再Tick中调用
        public void RemoveAllObj()
        {
            mImpl.RemoveAllObj(this);
        }
        public void RemoveAllNPC()
        {
            mImpl.RemoveAllNPC(this);
        }
        public void RemoveCollider(uint id)
        {
            mImpl.RemoveCollider(this, id);
        }

        public void BroadCastLodeInfo()
        {
            mImpl.BroadCastLodeInfo(this);
        }
        public void RemoveObj(ObjBase obj)
        {
            mImpl.RemoveObj(this, obj);
        }

        //心跳
        public virtual void Tick(float delta)
        {
            mImpl.Tick(this, delta);
        }

        public IEnumerator TryDeleteScene(Coroutine co, RemoveScene removeType)
        {
            return mImpl.TryDeleteScene(co, this, removeType);
        }

        public bool ValidPath(Vector2 currentPos, IEnumerable<Vector2> path)
        {
            return mImpl.ValidPath(this, currentPos, path);
        }

        /// <summary>
        ///     判断当前点是否合法
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool ValidPosition(Vector2 pos)
        {
            return mImpl.ValidPosition(this, pos);
        }

        public virtual void OnCreate()
        {
            mImpl.OnCreate(this);
        }

        //这里只可以写场景逻辑，不可以同步数据，因为这时候客户端有可能还没加载好
        public virtual void OnPlayerEnter(ObjPlayer player)
        {
            mImpl.OnPlayerEnter(this, player);
        }

        public virtual void OnPlayerLeave(ObjPlayer player)
        {
            mImpl.OnPlayerLeave(this, player);
        }

        public virtual void OnPlayerDie(ObjPlayer player, ulong characterId = 0)
        {
            mImpl.OnPlayerDie(this, player, characterId);
        }

		public virtual void OnPlayerRelive(ObjPlayer player, bool byItem)
		{
			mImpl.OnPlayerRelive(this, player, byItem);
		}
        public virtual void OnNpcDie(ObjNPC npc, ulong characterId = 0)
        {
            mImpl.OnNpcDie(this, npc, characterId);
        }

        public virtual void OnNpcRespawn(ObjNPC npc)
        {
            mImpl.OnNpcRespawn(this, npc);
        }
        public virtual void OnPlayerPickUp(ulong objId,int itemId,int count)
        {
            mImpl.OnPlayerPickUp(this, objId,itemId,count);
        }
        
        
        public virtual void ExitDungeon(ObjPlayer player)
        {
            mImpl.ExitDungeon(this, player);
        }

        public virtual void OnDestroy()
        {
            mImpl.OnDestroy(this);
        }

        public virtual void AutoRelive(ObjPlayer player)
        {
            mImpl.AutoRelive(this, player);
        }

        //主动请求切换场景
        public IEnumerator ChangeSceneCoroutine(Coroutine co, ObjCharacter character, int scneneId, int x, int y)
        {
            return mImpl.ChangeSceneCoroutine(co, character, scneneId, x, y);
        }

        #region 常量定义

        //zone的单位宽高
        public const float ZONE_SIDE = 10.0f;

        //场景内非Player的Id最大值
        public const ulong MAXID = 50000;

        //用于生成下个obj id, 从1开始，0不要了
        public ulong mNextObjId = 1;

        //生成下个obj id
        public ulong GenerateNextId()
        {
            return mImpl.GenerateNextId(this);
        }

        #endregion

        #region 逻辑数据

        //场景开启时间
        public DateTime BeginTime;

        //玩家人数
        public int PlayerCount
        {
            get { return mPlayerDict.Count; }
        }

        //场景是否激活
        public bool Active { get; set; }

        //场景所在Server Id
        public int ServerId { get; set; }

        //场景表格id
        public int TypeId { get; set; }

        //场景所在线号
        public ulong Guid { get; set; }

        //场景宽高
        public float mSceneWidth;
        public float mSceneHeight;
        public int mMonsterCount { get; set; }

        //格子边长
        public float ZoneSideLength { get; set; }

        //格子列数和行数
        public int mZoneColumnCount;
        public int mZoneLineCount;

        //场景的表格数据
        private SceneRecord _tableSceneData;

        public SceneRecord TableSceneData
        {
            get { return _tableSceneData; }
            set
            {
                _tableSceneData = value;
                PvpRuleId = _tableSceneData.PvPRule;
            }
        }

        //场景的PVP Rule Id
        public int PvpRuleId = -1;

        //场景扩展数据
        public int Exdata;
        //场景内obj
        public Dictionary<ulong, ObjBase> mObjDict = new Dictionary<ulong, ObjBase>();

        //场景内Character
        public Dictionary<ulong, ObjPlayer> mPlayerDict = new Dictionary<ulong, ObjPlayer>();

        //下一帧要被添加的
        public Dictionary<ulong, ObjBase> mNeedToAddObjDict = new Dictionary<ulong, ObjBase>();

        //下一帧要被删除的
        public Dictionary<ulong, ObjBase> mNeedToRemoveObjDict = new Dictionary<ulong, ObjBase>();

        //zone list
        public List<Zone> mZoneList = new List<Zone>();

        //是否正在Ticking
        public bool mTicking;

        //触发区域
        public Dictionary<int, TriggerArea> mAreaDict = new Dictionary<int, TriggerArea>();

        //本场景是否特殊掉落
        public int SpecialDrop = -1;

        //删除场景的定时器对象
        public Trigger mDelTrigger;

        #endregion

        #region 动态场景难度

        //静态参数
        public static float[] fModifyDoDamage =
        {
            1.0f,
            1.0f + Table.GetServerConfig(652).ToInt()/100.0f,
            1.0f + Table.GetServerConfig(654).ToInt()/100.0f,
            1.0f + Table.GetServerConfig(656).ToInt()/100.0f,
            1.0f + Table.GetServerConfig(658).ToInt()/100.0f,
            1.0f + Table.GetServerConfig(660).ToInt()/100.0f,
            1.0f + Table.GetServerConfig(662).ToInt()/100.0f,
            1.0f + Table.GetServerConfig(664).ToInt()/100.0f,
            1.0f + Table.GetServerConfig(666).ToInt()/100.0f,
            1.0f + Table.GetServerConfig(668).ToInt()/100.0f
        };

        public static float[] fModifyBeDamage =
        {
            1.0f,
            1.0f - Table.GetServerConfig(651).ToInt()/100.0f,
            1.0f - Table.GetServerConfig(653).ToInt()/100.0f,
            1.0f - Table.GetServerConfig(655).ToInt()/100.0f,
            1.0f - Table.GetServerConfig(657).ToInt()/100.0f,
            1.0f - Table.GetServerConfig(659).ToInt()/100.0f,
            1.0f - Table.GetServerConfig(661).ToInt()/100.0f,
            1.0f - Table.GetServerConfig(663).ToInt()/100.0f,
            1.0f - Table.GetServerConfig(665).ToInt()/100.0f,
            1.0f - Table.GetServerConfig(667).ToInt()/100.0f
        };

        //当前修正值
        public bool isNeedDamageModify = false;
        public float mDoDamageModify = 1.0f;
        public float mBeDamageModify = 1.0f;
        //读取伤害修正
        public float DoDamageModify
        {
            get { return mDoDamageModify; }
            set { mDoDamageModify = value; }
        }

        public float BeDamageModify
        {
            get { return mBeDamageModify; }
            set { mBeDamageModify = value; }
        }

        //伤害修正
        public int PlayerDoDamageModify(int value)
        {
            return mImpl.PlayerDoDamageModify(this, value);
        }

        public int NpcDoDamageModify(int value)
        {
            return mImpl.NpcDoDamageModify(this, value);
        }

        public static ObjNPC IsNeedChangeHp(ObjBase mObj)
        {
            return mImpl.IsNeedChangeHp(mObj);
        }

        public void ChangeDifficulty(int count)
        {
            mImpl.ChangeDifficulty(this, count);
        }

        #endregion

        #region Zone

        //Zone示意图
        /*
		|  mSceneWidth  |
		-----------------           --
		|	|	|	|	|ZONE_SIDE
		-----------------
		|	|	|	|	|
		-----------------			mSceneHeight
		|	|	|	|	|
		-----------------			--
		
		 
		  
		  
		----------------- line 0
		
		----------------- line 1
		
		----------------- line 2
		
		----------------- line 3

		
	column0 column1	  column2  column3
		|		|		|		|
		 		 		 	
		|		|		|		|
		 		 		 	
		|		|		|		|
		 		 		 	
		|		|		|		|
			
		 * 
		*/

        public int Pos2ZoneId(float x, float y)
        {
            return mImpl.Pos2ZoneId(this, x, y);
        }

        public Zone GetZone(int id)
        {
            return mImpl.GetZone(this, id);
        }

        //初始化Zone
        public bool InitZone()
        {
            return mImpl.InitZone(this);
        }

        #endregion

        #region 事件区域

        public virtual void OnCharacterEnterArea(int areaId, ObjCharacter character)
        {
            mImpl.OnCharacterEnterArea(this, areaId, character);
        }

        public virtual void OnAreaTick(int areaId, IEnumerator<ObjCharacter> enumerator)
        {
            mImpl.OnAreaTick(this, areaId, enumerator);
        }

        public virtual void OnCharacterLeaveArea(int areaId, ObjCharacter character)
        {
            mImpl.OnCharacterLeaveArea(this, areaId, character);
        }

        #endregion
    }
}