#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using C5;
using Database;
using DataContract;
using DataTable;
using EventSystem;
using Scorpion;
using NLog;
using Shared;
using Logger = NLog.Logger;

#endregion

namespace Activity
{
    public interface ITimedNotifier
    {
        int CompareTo(TimedNotifier _this, TimedNotifier other);
    }

    public class TimedNotifierDefaultImpl : ITimedNotifier
    {
        public int CompareTo(TimedNotifier _this, TimedNotifier other)
        {
            if (_this.TargetTime < other.TargetTime)
            {
                return -1;
            }
            if (_this.TargetTime == other.TargetTime)
            {
                return 0;
            }
            return 1;
        }
    }

    public class TimedNotifier : IComparable<TimedNotifier>
    {
        private static ITimedNotifier mImpl;

        static TimedNotifier()
        {
            ActivityServer.Instance.UpdateManager.InitStaticImpl(typeof (TimedNotifier),
                typeof (TimedNotifierDefaultImpl),
                o => { mImpl = (ITimedNotifier) o; });
        }

        public IPriorityQueueHandle<TimedNotifier> Handle;
        public CreateSpeMonsterNotifier Notifier;
        public DateTime TargetTime;

        public int CompareTo(TimedNotifier other)
        {
            return mImpl.CompareTo(this, other);
        }
    }

    public interface ICreateSpeMonsterNotifier
    {
        void Construct(CreateSpeMonsterNotifier _this, MonsterConfig config, eSpeMonsterType type);
        TimedNotifier GetTimed(CreateSpeMonsterNotifier _this);
        void NotifyCreate(CreateSpeMonsterNotifier _this);
    }

    public class CreateSpeMonsterNotifierDefaultImpl : ICreateSpeMonsterNotifier
    {
         [Updateable("SpeMonster")]
        private static readonly int[] DictionaryId =
        {
            220441,
            220443,
            -1
        };

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IEnumerator NotifyCreateCoroutine(Coroutine co, CreateSpeMonsterNotifier _this)
        {
            //通知各服务器生成boss
            var ids = new Int32Array();
            ids.Items.AddRange(_this.TableIds);

            var notifyMsg = ActivityServer.Instance.SceneAgent.NotifyCreateSpeMonster(ids);
            yield return notifyMsg.SendAndWaitUntilDone(co);
			Logger.Info("ActivityServer.Instance.SceneAgent.NotifyCreateSpeMonster({0})", ids);
            var notifyId = DictionaryId[(int) _this.Type];
            if (notifyId < 0)
            {
                yield break;
            }

            var scenes = new Dictionary<int, int>();
            foreach (var tableId in _this.TableIds)
            {
                var tbBoss = Table.GetWorldBOSS(tableId);
                if (tbBoss == null)
                {
                    continue;
                }
                var tbSceneNpc = Table.GetSceneNpc(tbBoss.SceneNpc);
                if (tbSceneNpc == null)
                {
                    continue;
                }
                scenes[tbSceneNpc.SceneID] = 0;
            }

            foreach (var s in SpeMonsterManager.ServerSceneMonsters)
            {
                var serverId = s.Key;
                Dictionary<int, int> validScenes;
                if (SpeMonsterManager.ServerValidScenes.TryGetValue(serverId, out validScenes))
                {
                    var createScenes = validScenes.Keys.Intersect(scenes.Keys);
                    if (!createScenes.Any())
                    {
                        continue;
                    }

                    var args = new List<string>();
                    args.Add(Utils.AddSceneId(createScenes.ToList()));
                    var str = Utils.WrapDictionaryId(notifyId, args);
                    ActivityServer.Instance.ChatAgent.BroadcastWorldMessage((uint) serverId,
                        (int) eChatChannel.SystemScroll, 0, string.Empty, new ChatMessageContent {Content = str});
                }
            }
        }

        public TimedNotifier GetTimed(CreateSpeMonsterNotifier _this)
        {
            if (_this._timed == null)
            {
                _this._timed = new TimedNotifier();
                _this._timed.Notifier = _this;
                _this._timed.TargetTime = _this.Config.UseNextTime();
            }
            return _this._timed;
        }

        public void Construct(CreateSpeMonsterNotifier _this, MonsterConfig config, eSpeMonsterType type)
        {
            _this.TableIds = _this.Config.Records.Select(r => r.Id).ToList();
        }

        public void NotifyCreate(CreateSpeMonsterNotifier _this)
        {
            var timed = _this.GetTimed();

            //通知各服务器生成boss
            CoroutineFactory.NewCoroutine(NotifyCreateCoroutine, _this).MoveNext();
            SpeMonsterManager.AddToCurAvailableDealers(timed);

            SpeMonsterManager.CreateMonsterDealers.Delete(timed.Handle);
            timed.TargetTime = _this.Config.UseNextTime();
            SpeMonsterManager.CreateMonsterDealers.Add(ref timed.Handle, timed);
        }
    }

    public class CreateSpeMonsterNotifier : CreateSpeMonsterBase
    {
        private static ICreateSpeMonsterNotifier mImpl;

        static CreateSpeMonsterNotifier()
        {
            ActivityServer.Instance.UpdateManager.InitStaticImpl(typeof (CreateSpeMonsterNotifier),
                typeof (CreateSpeMonsterNotifierDefaultImpl),
                o => { mImpl = (ICreateSpeMonsterNotifier) o; });
        }

        public CreateSpeMonsterNotifier(MonsterConfig config, eSpeMonsterType type)
            : base(config, type)
        {
            mImpl.Construct(this, config, type);
        }

        public TimedNotifier _timed;
        public List<int> TableIds;

        public TimedNotifier GetTimed()
        {
            return mImpl.GetTimed(this);
        }

        public void NotifyCreate()
        {
            mImpl.NotifyCreate(this);
        }
    }

    public interface ISpeMonsterManager
    {
        void AddToCurAvailableDealers(TimedNotifier notifier);
        Int32Array GetCreateMonsterData(int serverId, int sceneId);
        void Init();
        void NotifyCreate();
        void NotifyDelete();
    }

    public class SpeMonsterManagerDefaultImpl : ISpeMonsterManager
    {
        [Updateable("SpeMonsterManager")]
        private const string DBKey = "ServerAvailableScenes";

        private void CheckToStart()
        {
            CoroutineFactory.NewCoroutine(CheckToStartCoroutine).MoveNext();
        }

        private IEnumerator CheckToStartCoroutine(Coroutine co)
        {
            var waitServers = true;
            while (waitServers)
            {
                ActivityServer.Instance.AreAllServersReady(ready =>
                {
                    if (ready)
                    {
                        waitServers = false;
                        Start();
                    }
                });

                yield return ActivityServer.Instance.ServerControl.Wait(co, TimeSpan.FromSeconds(5));
            }
        }

        private void GetNextNotifier()
        {
            SpeMonsterManager.NextCreateNotifier = SpeMonsterManager.CreateMonsterDealers.FindMin().Notifier;
            ActivityServerControl.Timer.CreateTrigger(SpeMonsterManager.NextCreateNotifier.GetTimed().TargetTime,
                SpeMonsterManager.NotifyCreate);
        }

        private bool IsServerSceneValid(int serverId, int sceneId)
        {
            Dictionary<int, int> scenes;
            if (SpeMonsterManager.ServerValidScenes.TryGetValue(serverId, out scenes))
            {
                return scenes.ContainsKey(sceneId);
            }
            return false;
        }

        private void Load()
        {
            CoroutineFactory.NewCoroutine(LoadCoroutine).MoveNext();
        }

        private IEnumerator LoadCoroutine(Coroutine co)
        {
            var ret = ActivityServer.Instance.DB.Get<DBServerScenesData>(co, DataCategory.SpecialMonster, DBKey);
            yield return ret;
            if (ret.Status != DataStatus.Ok)
            {
                ActivityServer.Instance.Logger.Error("Load {0} failed!!", DBKey);
                yield break;
            }

            var dbData = ret.Data;
            if (dbData == null)
            {
                yield break;
            }

            SpeMonsterManager.ServerValidScenes.Clear();
            foreach (var serverScenes in dbData.Data)
            {
                var sceneDic = new Dictionary<int, int>();
                SpeMonsterManager.ServerValidScenes.Add(serverScenes.ServerId, sceneDic);
                foreach (var sceneId in serverScenes.SceneIds)
                {
                    sceneDic.Add(sceneId, 0);
                }
            }
        }

        private void Save()
        {
            if (SpeMonsterManager.SaveTrigger == null)
            {
                return;
            }
            SpeMonsterManager.SaveTrigger = null;
            CoroutineFactory.NewCoroutine(SaveCoroutine).MoveNext();
        }

        private IEnumerator SaveCoroutine(Coroutine co)
        {
            var dbData = new DBServerScenesData();
            foreach (var serverValidScene in SpeMonsterManager.ServerValidScenes)
            {
                var serverScenes = new DBServerScenes();
                dbData.Data.Add(serverScenes);
                serverScenes.ServerId = serverValidScene.Key;
                serverScenes.SceneIds.AddRange(serverValidScene.Value.Keys);
            }
            var ret = ActivityServer.Instance.DB.Set(co, DataCategory.SpecialMonster, DBKey, dbData);
            yield return ret;
        }

        private void Start()
        {
            if (SpeMonsterManager.IsStarted)
            {
                return;
            }
            SpeMonsterManager.IsStarted = true;

            var configArrays = SpeMonsterUtil.Instance.SpeMonsterConfigs;
            for (var i = 0; i < configArrays.Length; ++i)
            {
                var configArray = configArrays[i];
                for (var j = 0; j < configArray.Count; ++j)
                {
                    var config = configArray[j];
                    var notifier = new CreateSpeMonsterNotifier(config, (eSpeMonsterType) i);
                    var timed = notifier.GetTimed();
                    SpeMonsterManager.CreateMonsterDealers.Add(ref timed.Handle, timed);
                }
            }

            GetNextNotifier();
        }

        private void TryModifyServerScenes(int serverId, int sceneId)
        {
            serverId = SceneExtension.GetServerLogicId(serverId);
            Dictionary<int, int> scenes;
            if (!SpeMonsterManager.ServerValidScenes.TryGetValue(serverId, out scenes))
            {
                scenes = new Dictionary<int, int>();
                SpeMonsterManager.ServerValidScenes.Add(serverId, scenes);
            }
            if (scenes.ContainsKey(sceneId))
            {
                return;
            }
            scenes.Add(sceneId, 0);
            if (SpeMonsterManager.SaveTrigger == null)
            {
                SpeMonsterManager.SaveTrigger = ActivityServerControl.Timer.CreateTrigger(DateTime.Now.AddMinutes(1),
                    Save);
            }
        }

        public void Init()
        {
            Table.ForeachServerName(r =>
            {
                var serverId = r.LogicID;
                if (!SpeMonsterManager.ServerSceneMonsters.ContainsKey(serverId))
                {
                    SpeMonsterManager.ServerSceneMonsters.Add(serverId, new Dictionary<int, List<int>>());
                }
                return true;
            });
            Load();
            CheckToStart();
            EventDispatcher.Instance.AddEventListener(ReloadTableEvent.EVENT_TYPE, ReloadTable);
        }
        private static void ReloadTable(IEvent ievent)
        {
            var v = ievent as ReloadTableEvent;
            if (v == null)
                return;
            if (v.tableName == "ServerName")
            {
                Table.ForeachServerName(record =>
                {
                    if (!SpeMonsterManager.ServerSceneMonsters.ContainsKey(record.LogicID))
                    {
                        SpeMonsterManager.ServerSceneMonsters.Add(record.LogicID, new Dictionary<int, List<int>>());
                    }
                    return true;
                });
            }
        }
        public void NotifyCreate()
        {
            SpeMonsterManager.NextCreateNotifier.NotifyCreate();
            GetNextNotifier();
        }

        public void AddToCurAvailableDealers(TimedNotifier notifier)
        {
            var records = notifier.Notifier.Config.Records;
            foreach (var record in records)
            {
                var tbSceneNpc = Table.GetSceneNpc(record.SceneNpc);
                var sceneId = tbSceneNpc.SceneID;
                foreach (var s in SpeMonsterManager.ServerSceneMonsters)
                {
                    var serverId = s.Key;
                    if (IsServerSceneValid(serverId, sceneId))
                    {
                        var sceneMonsters = s.Value;
                        List<int> monsters;
                        if (!sceneMonsters.TryGetValue(sceneId, out monsters))
                        {
                            monsters = new List<int>();
                            sceneMonsters.Add(sceneId, monsters);
                        }
                        monsters.Add(record.Id);
                    }
                }
            }

            var timed = new TimedNotifier();
            timed.TargetTime = notifier.TargetTime.AddSeconds(SpeMonsterUtil.AvailableSeconds);
            timed.Notifier = notifier.Notifier;
            SpeMonsterManager.CurAvailableDealers.Add(ref timed.Handle, timed);
            if (SpeMonsterManager.DeleteTrigger == null)
            {
                SpeMonsterManager.NextDelNotifier = timed;
                SpeMonsterManager.DeleteTrigger = ActivityServerControl.Timer.CreateTrigger(timed.TargetTime,
                    NotifyDelete);
            }
        }

        public void NotifyDelete()
        {
            SpeMonsterManager.CurAvailableDealers.Delete(SpeMonsterManager.NextDelNotifier.Handle);
            if (SpeMonsterManager.CurAvailableDealers.Count > 0)
            {
                SpeMonsterManager.NextDelNotifier = SpeMonsterManager.CurAvailableDealers.FindMin();
                SpeMonsterManager.DeleteTrigger =
                    ActivityServerControl.Timer.CreateTrigger(SpeMonsterManager.NextDelNotifier.TargetTime, NotifyDelete);
            }
            else
            {
                SpeMonsterManager.DeleteTrigger = null;
            }
        }

        public Int32Array GetCreateMonsterData(int serverId, int sceneId)
        {
            var data = new Int32Array();

            bool hasNotCreateMonster = false;
            if (SpeMonsterManager.ServerSceneMonsters.ContainsKey(serverId))
            {
                var sceneMonsters = SpeMonsterManager.ServerSceneMonsters[serverId];
                List<int> ids;
                if (sceneMonsters.TryGetValue(sceneId, out ids) && ids.Count > 0)
                {
                    //此服务器的此场景，是有效的，并且缓存中有尚未造出的怪
                    data.Items.AddRange(ids);
                    ids.Clear();
                    hasNotCreateMonster = true;
                }
            }
            
            if(false == hasNotCreateMonster)
            {
                foreach (var timed in SpeMonsterManager.CurAvailableDealers)
                {
                    data.Items.AddRange(timed.Notifier.TableIds);
                }
            }

            TryModifyServerScenes(serverId, sceneId);

            return data;
        }
    }

    public static class SpeMonsterManager
    {
        public static IntervalHeap<TimedNotifier> CreateMonsterDealers = new IntervalHeap<TimedNotifier>();
        public static IntervalHeap<TimedNotifier> CurAvailableDealers = new IntervalHeap<TimedNotifier>();
        public static object DeleteTrigger;
        public static bool IsStarted;
        private static ISpeMonsterManager mImpl;
        public static CreateSpeMonsterNotifier NextCreateNotifier;
        public static TimedNotifier NextDelNotifier;
        public static object SaveTrigger;
        //server id => scene id => List<int> 该场景需要创建的所有 worldBOSS id，用来存哪个服务器的哪个场景需要创建哪些worldBOSS
        public static Dictionary<int, Dictionary<int, List<int>>> ServerSceneMonsters =
            new Dictionary<int, Dictionary<int, List<int>>>();

        //server id => scene id => 无意义，用来存每个服务器有人去过的场景
        public static Dictionary<int, Dictionary<int, int>> ServerValidScenes =
            new Dictionary<int, Dictionary<int, int>>();

        static SpeMonsterManager()
        {
            ActivityServer.Instance.UpdateManager.InitStaticImpl(typeof (SpeMonsterManager),
                typeof (SpeMonsterManagerDefaultImpl),
                o => { mImpl = (ISpeMonsterManager) o; });
        }

        public static void AddToCurAvailableDealers(TimedNotifier notifier)
        {
            mImpl.AddToCurAvailableDealers(notifier);
        }

        public static Int32Array GetCreateMonsterData(int serverId, int sceneId)
        {
            return mImpl.GetCreateMonsterData(serverId, sceneId);
        }

        public static void Init()
        {
            mImpl.Init();
        }

        public static void NotifyCreate()
        {
            mImpl.NotifyCreate();
        }

        public static void NotifyDelete()
        {
            CurAvailableDealers.Delete(NextDelNotifier.Handle);
            if (CurAvailableDealers.Count > 0)
            {
                NextDelNotifier = CurAvailableDealers.FindMin();
                DeleteTrigger = ActivityServerControl.Timer.CreateTrigger(NextDelNotifier.TargetTime, NotifyDelete);
            }
            else
            {
                DeleteTrigger = null;
            }
        }
    }
}