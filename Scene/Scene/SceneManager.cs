#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DataContract;
using DataTable;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Scene
{
    public enum RemoveScene
    {
        SceneClose = 0,
        SceneMerge = 1,
        NoPlayer = 2
    }

    public interface ISceneManager
    {
        Scene CreateScene(SceneManager _this,
                          int serverId,
                          int sceneId,
                          ulong regionId = 0,
                          SceneParam sceneParam = null);

        void CreateSpeMonsters(SceneManager _this, List<int> ids);
        void FreshBossHome(SceneManager _this, List<int> ids);
        void KillAllBoss(SceneManager _this);
        void FreshLodeTimer(SceneManager _this,int ServerId, List<int> ids);
        
        Scene EnterScene(SceneManager _this, ObjPlayer obj, ulong guid);
        Scene GetScene(SceneManager _this, ulong sceneGuid);
        Scene GetScene(SceneManager _this, int serverId, int sceneId, ulong guid = 0, bool bCreate = false);
        List<Scene> GetScenes(SceneManager _this, int serverId, int sceneId);
        bool Init(SceneManager _this);
        void LevelScene(SceneManager _this, ObjCharacter obj);
        void Log(SceneManager _this);
        void RemoveScene(SceneManager _this, ulong guid, RemoveScene removeType);
        void RemoveScene(SceneManager _this, Scene scene, RemoveScene removeType);
        void Tick(SceneManager _this, float delta);
        void CheckAvgLevelBuff(SceneManager _this, ObjPlayer objPlayer);
        void CheckAddLifeCardBuff(SceneManager _this, ObjPlayer objPlayer);
        int GetAvgLevel(SceneManager _this, int serverId);
        void PushAvgLevel(SceneManager _this, Dictionary<int, int> list);
    }

    public class SceneManagerDefaultImpl : ISceneManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();



        //获取成员的完整信息
        private IEnumerator NotifyChangeScene(Coroutine coroutine, ObjPlayer obj, int sceneId)
        {
            var sceneSimpleData = SceneServer.Instance.TeamAgent.SSNotifyPlayerChangeScene(obj.ObjId, obj.ServerId,
                obj.ObjId, sceneId, obj.GetLevel(), obj.Attr.GetFightPoint());
            yield return sceneSimpleData.SendAndWaitUntilDone(coroutine);
        }

        public bool Init(SceneManager _this)
        {
            _this.ScenesDic = new Dictionary<int, Dictionary<int, Dictionary<ulong, Scene>>>();

            Table.ForeachSceneNpc(record =>
            {
                var sceneId = record.SceneID;
                List<SceneNpcRecord> tempList;
                if (!SceneManager.SceneNpcs.TryGetValue(sceneId, out tempList))
                {
                    tempList = new List<SceneNpcRecord>();
                    SceneManager.SceneNpcs.Add(sceneId, tempList);
                }
                tempList.Add(record);
                return true;
            });

            Table.ForeachMapTransfer(record =>
            {
                var sceneId = record.SceneID;
                List<MapTransferRecord> tempList;
                if (!SceneManager.SceneMapNpcs.TryGetValue(sceneId, out tempList))
                {
                    tempList = new List<MapTransferRecord>();
                    SceneManager.SceneMapNpcs.Add(sceneId, tempList);
                }
                tempList.Add(record);
                return true;
            });

#if DEBUG
            string str;
            Table.ForeachScene(table =>
            {
                str = "../Scene/" + table.ResName + ".path";
                var obstacle = new SceneObstacle(str);
                obstacle.GetObstacleValue(0, 0);
                return true;
            });
#endif

            _this.minLevel = int.Parse(Table.GetServerConfig(450).Value);
            _this.maxExpMul = int.Parse(Table.GetServerConfig(451).Value);
            _this.minDev = int.Parse(Table.GetServerConfig(452).Value);
            _this.maxDev = int.Parse(Table.GetServerConfig(453).Value);

            EventSystem.EventDispatcher.Instance.AddEventListener(ReloadTableEvent.EVENT_TYPE, evt =>
            {
                if ((evt as ReloadTableEvent).tableName == "ServerConfig")
                {
                    _this.minLevel = int.Parse(Table.GetServerConfig(450).Value);
                    _this.maxExpMul = int.Parse(Table.GetServerConfig(451).Value);
                    _this.minDev = int.Parse(Table.GetServerConfig(452).Value);
                    _this.maxDev = int.Parse(Table.GetServerConfig(453).Value);
                }
            });

            return true;
        }

        public void CreateSpeMonsters(SceneManager _this, List<int> ids)
        {
            foreach (var id in ids)
            {
                var record = Table.GetWorldBOSS(id);
                if (record == null)
                {
					Logger.Error("Error null==Table.GetWorldBOSS({0})",id);
                    continue;
                }

                var tbSceneNpc = Table.GetSceneNpc(record.SceneNpc);
                foreach (var serverScenes in _this.ScenesDic)
                {
                    Dictionary<ulong, Scene> scenes;
                    if (!serverScenes.Value.TryGetValue(tbSceneNpc.SceneID, out scenes))
                    {
                        continue;
                    }

                    var sInfo = new ServerMonsterCreateInfo();
                    sInfo.ServerId = serverScenes.Key;

                    foreach (var scenePair in scenes)
                    {
                        var scene = scenePair.Value;
                        if (scene.CreateSpeMonster(record) != null)
                        {
                            sInfo.SceneId.Add(tbSceneNpc.SceneID);
                        }
                    }
                }
            }
        }

        public void FreshLodeTimer(SceneManager _this, int ServerId, List<int> ids)
        {
            foreach (var id in ids)
            {
                foreach (var serverScenes in _this.ScenesDic)
                {
                    if (serverScenes.Key != ServerId)
                        continue;
                    Dictionary<ulong, Scene> scenes;
                    if (!serverScenes.Value.TryGetValue(id, out scenes))
                    {
                        continue;
                    }
                    foreach (var scenePair in scenes)
                    {
                        var scene = scenePair.Value;
                        if (scene.IsLodeMap)
                        {
                            scene.RefreshLodeTimer();                            
                        }
                    }
                }                
            }
        }

        public void KillAllBoss(SceneManager _this)
        {
            foreach (var serverScenes in _this.ScenesDic)
            {
                foreach (var scenePair in serverScenes.Value)
                {
                    foreach (var scenes in scenePair.Value)
                    {
                        var scene = scenes.Value;
                        if (scene.TableSceneData == null)
                            continue;
                        if (scene.TableSceneData.Type == (int) eSceneType.BossHome)
                        {
                            var bossHome = scene as BossHome;
                            if (bossHome != null)
                            {
                                bossHome.KillBoss();
                            }
                        }
                    }
                }
            }
        }
        public void FreshBossHome(SceneManager _this, List<int> ids)
        {
            foreach (var id in ids)
            {
                var tbBossHome = Table.GetBossHome(id);
                if (tbBossHome == null)
                    continue;
                foreach (var serverScenes in _this.ScenesDic)
                {
                    Dictionary<ulong, Scene> scenes;
                    if (!serverScenes.Value.TryGetValue(tbBossHome.Scene, out scenes))
                    {
                        continue;
                    }
                    foreach (var scenePair in scenes)
                    {
                        var scene = scenePair.Value as BossHome;
                        if(scene != null)
                            scene.RefreshBoss(tbBossHome.SceneNpcId);
                    }
                }
            }
        }
        public Scene GetScene(SceneManager _this, ulong sceneGuid)
        {
            Scene scene;
            if (_this.Scenes.TryGetValue(sceneGuid, out scene))
            {
                return scene;
            }

            return null;
        }

        public Scene GetScene(SceneManager _this, int serverId, int sceneId, ulong guid = 0, bool bCreate = false)
        {
            Dictionary<int, Dictionary<ulong, Scene>> scenes;
            if (_this.ScenesDic.TryGetValue(serverId, out scenes))
            {
                Dictionary<ulong, Scene> regions;
                if (scenes.TryGetValue(sceneId, out regions))
                {
                    Scene value2;
                    if (regions.TryGetValue(guid, out value2))
                    {
                        return value2;
                    }
                }
            }

            return null;
        }

        public List<Scene> GetScenes(SceneManager _this, int serverId, int sceneId)
        {
            Dictionary<int, Dictionary<ulong, Scene>> serverScenes;
            if (_this.ScenesDic.TryGetValue(serverId, out serverScenes))
            {
                Dictionary<ulong, Scene> scenes;
                if (serverScenes.TryGetValue(sceneId, out scenes))
                {
                    return scenes.Values.ToList();
                }
            }
            return null;
        }

        public Scene CreateScene(SceneManager _this,
                                 int serverId,
                                 int sceneId,
                                 ulong regionId = 0,
                                 SceneParam sceneParam = null)
        {
            Dictionary<int, Dictionary<ulong, Scene>> servers = null;
            if (!_this.ScenesDic.ContainsKey(serverId))
            {
                servers = new Dictionary<int, Dictionary<ulong, Scene>>();
                _this.ScenesDic.Add(serverId, servers);
            }
            else
            {
                servers = _this.ScenesDic[serverId];
            }

            Dictionary<ulong, Scene> scenes = null;
            if (!servers.ContainsKey(sceneId))
            {
                scenes = new Dictionary<ulong, Scene>();
                servers.Add(sceneId, scenes);
            }
            else
            {
                scenes = servers[sceneId];
            }

            //var scene = new Scene {TypeId = sceneId, ServerId = serverId, Guid = regionId};
            Scene scene;
            if (sceneId == 0)
            {
                scene = new LeaverScene();
                scene.Init(sceneParam);
            }
            else
            {
                scene = SceneRegister.CreateScene(sceneId, serverId, regionId);
                scene.Init(sceneParam);
            }


            scenes.Add(regionId, scene);
            _this.Scenes.Add(regionId, scene);
            try
            {
                scene.OnCreate();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return scene;
        }

        public Scene EnterScene(SceneManager _this, ObjPlayer obj, ulong guid)
        {
            var scene = GetScene(_this, guid);
            if (scene == null)
            {
                Logger.Error("EnterScene Error sceneGuid={0},objGuid={1}", guid, obj.ObjId);
                return null;
            }
            scene.EnterScene(obj);
            if (obj.GetAllianceId() != 0)
            {
                CoroutineFactory.NewCoroutine(NotifyChangeScene, obj, scene.TypeId).MoveNext();
            }
            //后台统计
            try
            {               
                string v = string.Format("scene#{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                        obj.ServerId,
                        obj.ObjId,
                        obj.GetLevel(),
                        obj.Attr.GetFightPoint(),//战力
                        obj.Scene.TypeId,
                        0,          //0:进入  1:离开
                        DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")); // 时间                            
                PlayerLog.Kafka(v);                  
            }
            catch (Exception )
            {
            }
            return scene;
        }

        public void LevelScene(SceneManager _this, ObjCharacter obj)
        {
            if (obj == null)
            {
                return;
            }
            var scene = obj.Scene;
            if (scene == null)
            {
                Logger.Info("GetSceneObj null...............");
                return;
            }

            //后台统计
            try
           {               
                string v = string.Format("scene#{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                        obj.ServerId,
                        obj.ObjId,
                        obj.GetLevel(),
                        obj.Attr.GetFightPoint(),//战力
                        obj.Scene.TypeId,
                        1,          //0:进入  1:离开
                        DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")); // 时间                            
                    PlayerLog.Kafka(v);                
            }
            catch (Exception )
            {
            }
            scene.LeaveScene(obj);
        }

        public void RemoveScene(SceneManager _this, ulong guid, RemoveScene removeType)
        {
            Scene scene;
            if (_this.Scenes.TryGetValue(guid, out scene))
            {
                RemoveScene(_this, scene, removeType);
            }
        }

        public void RemoveScene(SceneManager _this, Scene scene, RemoveScene removeType)
        {
            var serverId = scene.ServerId;
            Dictionary<int, Dictionary<ulong, Scene>> scenes;
            if (_this.ScenesDic.TryGetValue(serverId, out scenes))
            {
                Dictionary<ulong, Scene> regions;
                var sceneId = scene.TypeId;
                if (scenes.TryGetValue(sceneId, out regions))
                {
                    //Dictionary<ulong, Scene> regions = scenes[sceneId];
                    var guid = scene.Guid;
                    if (regions.ContainsKey(guid))
                    {
                        PlayerLog.WriteLog(889, "RemoveScene ok! sceneId={0},sceneGuid={1},type={2},serverId={3}",
                            scene.TypeId, scene.Guid, removeType, serverId);
                        regions.Remove(guid);
                    }
                    else
                    {
                        PlayerLog.WriteLog(889,
                            "RemoveScene notfind guid sceneId={0},sceneGuid={1},type={2},serverId={3}", scene.TypeId,
                            scene.Guid, removeType, serverId);
                    }
                    if (regions.Count == 0)
                    {
                        scenes.Remove(sceneId);
                    }
                }
                else
                {
                    PlayerLog.WriteLog(889, "RemoveScene notfind scene sceneId={0},sceneGuid={1},type={2},serverId={3}",
                        scene.TypeId, scene.Guid, removeType, serverId);
                }
                if (scenes.Count == 0)
                {
                    _this.ScenesDic.Remove(serverId);
                }
            }
            else
            {
                PlayerLog.WriteLog(889, "RemoveScene notfind server sceneId={0},sceneGuid={1},type={2},serverId={3}",
                    scene.TypeId, scene.Guid, removeType, serverId);
            }

            scene.Destroy();
            _this.Scenes.Remove(scene.Guid);
        }

        public void Tick(SceneManager _this, float delta)
        {
#if DEBUG
            var s = Stopwatch.StartNew();
#endif
            foreach (var scene in _this.Scenes)
            {
                if (scene.Value.Active)
                {
                    scene.Value.Tick(delta);
                }
            }
            _this.TickCount++;

#if DEBUG
            _this.TS += s.Elapsed;

            if (0 == _this.TickCount%SceneManager.StatisticFrameCount)
            {
                var framesPerScond = SceneManager.StatisticFrameCount/(DateTime.Now - _this.lastTime).TotalSeconds;
                if (framesPerScond < SceneServerControl.Frequence - 8)
                {
                    var timePertTick = _this.TS.TotalSeconds/SceneManager.StatisticFrameCount;
                    Logger.Warn("---------------scene tick average cost[{0}]", timePertTick);
                    Logger.Warn("---------------[{0}]ticks average 1 second", framesPerScond);
                }

                _this.TS = new TimeSpan();

                _this.lastTime = DateTime.Now;
            }
#endif
			SceneServerMonitor.TickRate.Mark();
        }

        public void CheckAvgLevelBuff(SceneManager _this, ObjPlayer objPlayer)
        {
            objPlayer.DeleteBuff(319, eCleanBuffType.TimeOver);
            var serverId = SceneExtension.GetServerLogicId(objPlayer.ServerId);
            var avgLevel = _this.GetAvgLevel(serverId);
            if (avgLevel <= 150)
            {
                return;
            }
            var playerLevel = objPlayer.GetLevel();
            if (playerLevel < _this.minLevel)
            {
                return;
            }
            if (playerLevel + _this.minDev > avgLevel)
            {
                return;
            }
            var a = (_this.maxExpMul - 2.0f) / (_this.maxDev - _this.minDev);
            var b = _this.maxExpMul - a * _this.maxDev;
            var buffLevel = ((avgLevel - playerLevel) * a + b);
            if (buffLevel > _this.maxExpMul)
            {
                buffLevel = _this.maxExpMul;
            }
            objPlayer.AddBuff(319, (int) buffLevel * 100, objPlayer);
        }

        public void CheckAddLifeCardBuff(SceneManager _this, ObjPlayer objPlayer)
        {
            var tbRecharge = Table.GetRecharge(41);
            if (null == tbRecharge)
                return;
            var buffId = tbRecharge.Param[1];
            objPlayer.DeleteBuff(buffId, eCleanBuffType.TimeOver);
            objPlayer.AddBuff(buffId, 1, objPlayer);
        }

        public int GetAvgLevel(SceneManager _this, int serverId)
        {
            int l;
            if (_this.ServerAvgLevel.TryGetValue(serverId, out l))
            {
                return l;
            }
            return -1;
        }
        public void PushAvgLevel(SceneManager _this, Dictionary<int, int> list)
        {
            _this.ServerAvgLevel = list;
            foreach (var pair in _this.Scenes)
            {
                var scene = pair.Value;
                foreach (var objPlayer in scene.EnumAllPlayer())
                {
                    _this.CheckAvgLevelBuff(objPlayer);
                    //int serverId = objPlayer.ServerId;
                    //int avgLevel = GetAvgLevel(serverId);
                    //if (avgLevel <= 150) continue;
                    //int playerLevel = objPlayer.GetLevel();
                    //if (playerLevel <= 100 )  continue;
                    //if(playerLevel + 30 > avgLevel) continue;
                    //objPlayer.AddBuff(319, avgLevel - playerLevel, objPlayer);
                }
            }
        }

        public void Log(SceneManager _this)
        {
            PlayerLog.WriteLog((int) LogType.SceneManager, "SceneManager SceneCount={0}", _this.Scenes.Count);
            if (_this.Scenes.Count > 0)
            {
                PlayerLog.WriteLog((int) LogType.SceneManager, "{");
                foreach (var scene in _this.Scenes)
                {
                    PlayerLog.WriteLog((int) LogType.SceneManager,
                        "    SceneGuid={0},ServerId={1},SceneId={2},CharacterCount={3}", scene.Key, scene.Value.ServerId,
                        scene.Value.TypeId, scene.Value.PlayerCount);

                    PlayerLog.WriteLog((int) LogType.SceneManager, "    {");
                    foreach (var player in scene.Value.EnumAllPlayer())
                    {
                        PlayerLog.WriteLog((int) LogType.SceneManager,
                            "    CharacterId={0},Name={1},ServerId={2},Level={3},Role={4}", player.ObjId,
                            player.GetName(), player.ServerId, player.GetLevel(), player.TypeId);
                    }
                    PlayerLog.WriteLog((int) LogType.SceneManager, "    }");
                }
                PlayerLog.WriteLog((int) LogType.SceneManager, "}");
            }
        }
    }

    public class SceneManager
    {
        private static SceneManager _instance;
        private static ISceneManager mImpl;

        public static Dictionary<int, List<MapTransferRecord>> SceneMapNpcs =
            new Dictionary<int, List<MapTransferRecord>>();

        public static Dictionary<int, List<SceneNpcRecord>> SceneNpcs = new Dictionary<int, List<SceneNpcRecord>>();
        public static RequestManager WebRequestManager = null;
        public float minLevel;
        public float maxExpMul;
        public float minDev;
        public float maxDev;

        static SceneManager()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (SceneManager), typeof (SceneManagerDefaultImpl),
                o => { mImpl = (ISceneManager) o; });
        }

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public Dictionary<ulong, Scene> Scenes = new Dictionary<ulong, Scene>(); //guid -> scene

        public Dictionary<int, Dictionary<int, Dictionary<ulong, Scene>>> ScenesDic;
            // Server -> SceneId -> guid -> scene

        public ulong TickCount;

        public static SceneManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SceneManager();
                }
                return _instance;
            }
        }

        public Scene CreateScene(int serverId, int sceneId, ulong regionId = 0, SceneParam sceneParam = null)
        {
            return mImpl.CreateScene(this, serverId, sceneId, regionId, sceneParam);
        }

        public void CreateSpeMonsters(List<int> ids)
        {
            mImpl.CreateSpeMonsters(this, ids);
        }
        public void FreshBossHome(List<int> ids)
        {
            mImpl.FreshBossHome(this, ids);
        }

        public void KillAllBoss()
        {
            mImpl.KillAllBoss(this);
        }
        public void FreshLodeTimer(int serverId,List<int> ids)
        {
            mImpl.FreshLodeTimer(this,serverId,ids);            
        }
        public Scene EnterScene(ObjPlayer obj, ulong guid)
        {
            return mImpl.EnterScene(this, obj, guid);
        }

        public Scene GetScene(ulong sceneGuid)
        {
            return mImpl.GetScene(this, sceneGuid);
        }

        public Scene GetScene(int serverId, int sceneId, ulong guid = 0, bool bCreate = false)
        {
            return mImpl.GetScene(this, serverId, sceneId, guid, bCreate);
        }

        public List<Scene> GetScenes(int serverId, int sceneId)
        {
            return mImpl.GetScenes(this, serverId, sceneId);
        }

        public bool Init()
        {
            return mImpl.Init(this);
        }

        public void LevelScene(ObjCharacter obj)
        {
            mImpl.LevelScene(this, obj);
        }

        public void Log()
        {
            mImpl.Log(this);
        }

        public void RemoveScene(ulong guid, RemoveScene removeType)
        {
            mImpl.RemoveScene(this, guid, removeType);
        }

        public void RemoveScene(Scene scene, RemoveScene removeType)
        {
            mImpl.RemoveScene(this, scene, removeType);
        }

        public void Tick(float delta)
        {
            mImpl.Tick(this, delta);
        }

#if DEBUG
        public TimeSpan TS;
        public DateTime lastTime = DateTime.Now;
        public const int StatisticFrameCount = 200;
#endif

        #region 玩家平均等级相关

        public Dictionary<int, int> ServerAvgLevel = new Dictionary<int, int>();

        public void PushAvgLevel(Dictionary<int, int> list)
        {
            mImpl.PushAvgLevel(this, list);
        }

        public void CheckAvgLevelBuff(ObjPlayer objPlayer)
        {
            mImpl.CheckAvgLevelBuff(this, objPlayer);
        }

        public void CheckAddLifeCardBuff(ObjPlayer objPlayer)
        {
            mImpl.CheckAddLifeCardBuff(this, objPlayer);
        }

        public int GetAvgLevel(int serverId)
        {
            return mImpl.GetAvgLevel(this, serverId);
        }

        #endregion
    }
}