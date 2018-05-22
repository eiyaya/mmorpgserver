#region using

using System;
using System.Collections;
using System.Collections.Generic;
using Database;
using DataContract;
using DataTable;
using EventSystem;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Activity
{
    public interface IWorldBoss
    {
        DamageListForServer ApplyDamage(WorldBoss _this, ulong sceneGuid, DamageList damage);
        void BossNext(WorldBoss _this);
        void BossStart(WorldBoss _this);
        void BossStop(WorldBoss _this);
        void BossWillStart(WorldBoss _this);
        void Construct(WorldBoss _this, int serverId, eActivityState state);
        void SetBossLevel(WorldBoss _this, int level);
    }

    public class WorldBossDefaultImpl : IWorldBoss
    {
        [Updateable("WorldBoss")]
        public const string DbKey = "WBoss:";
        private static readonly Logger Logger = LogManager.GetLogger("WorldBoss");

        public void Construct(WorldBoss _this, int serverId, eActivityState state)
        {
            _this.ServerId = serverId;
            _this.State = state;
            GetDbBossLevel(_this);
        }

        public void SetBossLevel(WorldBoss _this, int level)
        {
            if (_this.BossLevel == level)
            {
                return;
            }
            var newLevel = level;
            newLevel = Math.Min(29, newLevel);
            newLevel = Math.Max(0, newLevel);
            if (_this.BossLevel == newLevel)
            {
                return;
            }
            _this.BossLevel = newLevel;
            SetDbBossLevel(_this);
        }

        #region DB

        private void GetDbBossLevel(WorldBoss _this)
        {
            CoroutineFactory.NewCoroutine(GetDbBossLevelCoroutine, _this).MoveNext();
        }

        private IEnumerator GetDbBossLevelCoroutine(Coroutine co, WorldBoss _this)
        {
            var dbBossLevel = ActivityServer.Instance.DB.Get<DBInt>(co, DataCategory.SceneWorldBoss,
                DbKey + _this.ServerId);
            yield return dbBossLevel;
            if (dbBossLevel.Status != DataStatus.Ok)
            {
                Logger.Fatal("GetDbBossLevel get data from db faild!");
                yield break;
            }
            if (dbBossLevel.Data != null)
            {
                SetBossLevel(_this, dbBossLevel.Data.Value);
            }
            else
            {
                SetBossLevel(_this, 0);
            }
            ResetData(_this);
        }

        private void SetDbBossLevel(WorldBoss _this)
        {
            CoroutineFactory.NewCoroutine(SetDbBossLevelCoroutine, _this).MoveNext();
        }

        private IEnumerator SetDbBossLevelCoroutine(Coroutine co, WorldBoss _this)
        {
            var data = new DBInt();
            data.Value = _this.BossLevel;
            var dbBossLevel = ActivityServer.Instance.DB.Set(co, DataCategory.SceneWorldBoss, DbKey + _this.ServerId,
                data);
            yield return dbBossLevel;
            if (dbBossLevel.Status != DataStatus.Ok)
            {
                Logger.Fatal("SetDbBossLevel set data to db faild!");
                yield return ActivityServer.Instance.ServerControl.Wait(co, TimeSpan.FromMilliseconds(100));

                if (++_this.SetDbFailCounter < 3)
                {
                    SetDbBossLevel(_this);
                }
                else
                {
                    _this.SetDbFailCounter = 0;
                }
            }
        }

        #endregion

        #region 对外方法

        public DamageListForServer ApplyDamage(WorldBoss _this, ulong sceneGuid, DamageList damage)
        {
            //本服玩家 guids
            List<ulong> players;
            if (!_this.ServerPlayers.TryGetValue(sceneGuid, out players))
            {
                players = new List<ulong>();
                _this.ServerPlayers.Add(sceneGuid, players);
            }
            //如果boss已经死了，或者时间已经结束，不再计算伤害，直接返回该服务器对应的DamageList
            if (!_this.mBossAlive)
            {
                return GetServerDamageList(_this, players);
            }

            
            var restMin = 0;
            
            {
                var trigger = WorldBossManager.StopTrigger as Trigger;
                var nowTime = DateTime.Now;
                var dueTime = trigger.Time;
                restMin = (int)(dueTime - nowTime).Minutes;
            }


            //boss没死，计算伤害
            //最后一击的player id
            ulong lastPlayerId = 0;
            var damageDatas = damage.Data;
            foreach (var damageUnit in damageDatas)
            {
                var charId = damageUnit.CharacterId;
                var maxDamage = _this.BossMaxHp - _this.TotalDamage;
                damageUnit.Damage = Math.Min(maxDamage, damageUnit.Damage);
                DamageUnit playerDamage;
                if (_this.PlayerDamages.TryGetValue(charId, out playerDamage))
                {
                    playerDamage.Damage += damageUnit.Damage;
                }
                else
                {
                    players.Add(charId);
                    _this.PlayerDamages.Add(charId, damageUnit);
                    _this.DamageList.Add(damageUnit);
                }
                _this.TotalDamage += damageUnit.Damage;
                if (_this.TotalDamage > _this.BossMaxHp * 0.7)
                {//世界Boss血回复
                    if (restMin >= 5 && restMin <= 7)
                    {
                        _this.TotalDamage -= (int)(_this.BossMaxHp * 0.3);
                    }
                    else if (restMin >= 7 && restMin <= 9)
                    {
                        _this.TotalDamage -= (int)(_this.BossMaxHp * 0.5);
                    }
                    else if (restMin > 9)
                    {
                        _this.TotalDamage -= (int)(_this.BossMaxHp * 0.7);
                    }
                }
                if (_this.TotalDamage >= _this.BossMaxHp)
                {
                    _this.TotalDamage = _this.BossMaxHp;
                    lastPlayerId = damageUnit.CharacterId;
                    //通知所有Scene服务器，世界boss已经死了
                    NotifyBossDie(_this);
                    OnBossDie(_this);
                    break;
                }
            }
            //排序并赋予名次
            _this.DamageList.Sort(_this.DuComparer);
            var idx = 1;
            foreach (var unit in _this.DamageList)
            {
                unit.Rank = idx++;
            }
            return GetServerDamageList(_this, players, lastPlayerId);
        }

        public void BossWillStart(WorldBoss _this)
        {
            _this.State = eActivityState.WillStart;
        }

        public void BossStart(WorldBoss _this)
        {
            _this.State = eActivityState.Start;
            _this.mBossAlive = true;
            Logger.Info("BossStart,Level = {0}", _this.BossLevel);
        }

        public void BossStop(WorldBoss _this)
        {
            if (!_this.mBossAlive)
            {
                return;
            }

            PrintDamageList(_this);

            _this.State = eActivityState.WillEnd;
            _this.mBossAlive = false;
            SetBossLevel(_this, _this.BossLevel - 1);
            SendFirstPlayerNotify(_this);
        }

        public void BossNext(WorldBoss _this)
        {
            ResetData(_this);

            _this.State = eActivityState.WaitNext;
            NotifyActivityState(_this);
        }

        #endregion

        #region 私有方法

        private void ResetData(WorldBoss _this)
        {
            _this.mBossAlive = true;
            _this.TotalDamage = 0;
            _this.ServerPlayers = new Dictionary<ulong, List<ulong>>();
            _this.PlayerDamages = new Dictionary<ulong, DamageUnit>();
            _this.DamageList = new List<DamageUnit>();

            _this.mBossNpcId = 90000 + _this.BossLevel;
            var tbCharBase = Table.GetCharacterBase(_this.mBossNpcId);
            _this.BossMaxHp = tbCharBase.Attr[13];
        }

        private void PrintDamageList(WorldBoss _this)
        {
            Logger.Info("Boss Level = {0}", _this.BossLevel);
            for (int i = 0, imax = _this.DamageList.Count; i < imax; i++)
            {
                var unit = _this.DamageList[i];
                Logger.Info("No.{0}, player id = {1}, name = {2}, damage = {3}", i + 1, unit.CharacterId, unit.Name,
                    unit.Damage);
            }
        }

        private void OnBossDie(WorldBoss _this)
        {
            PrintDamageList(_this);

            _this.State = eActivityState.WillEnd;
            _this.mBossAlive = false;

            var trigger = WorldBossManager.StartTrigger as Trigger;
            var nowTime = DateTime.Now;
            var dueTime = trigger.Time;
            var restSec = (nowTime - dueTime).TotalMinutes;
            if (restSec <= 7)
            {//七分钟内完成boss级别+1
                SetBossLevel(_this, _this.BossLevel + 1);
            }

            NotifyActivityState(_this);
            SendFirstPlayerNotify(_this);
        }

        private void NotifyBossDie(WorldBoss _this)
        {
            CoroutineFactory.NewCoroutine(NotifyBossDieCoroutine, _this).MoveNext();
        }

        private IEnumerator NotifyBossDieCoroutine(Coroutine co, WorldBoss _this)
        {
            var msg = ActivityServer.Instance.SceneAgent.BossDie((uint)_this.ServerId, _this.ServerId);
            yield return msg.SendAndWaitUntilDone(co);
        }

        //发公告，谁谁是第一名
        private void SendFirstPlayerNotify(WorldBoss _this)
        {
            var damageList = _this.DamageList;
            if (damageList.Count == 0)
            {
                return;
            }
            var args = new List<string>();
            args.Add(Utils.AddCharacter(damageList[0].CharacterId, damageList[0].Name));
            var content = Utils.WrapDictionaryId(215003, args);
            ActivityServer.Instance.ChatAgent.BroadcastWorldMessage((uint) _this.ServerId,
                (int) eChatChannel.SystemScroll, 0,
                string.Empty, new ChatMessageContent {Content = content});
        }

        //通知全服，boss挂了，活动已经结束了
        private void NotifyActivityState(WorldBoss _this)
        {
            ActivityServer.Instance.ServerControl.NotifyActivityState((uint) _this.ServerId, (int) eActivity.WorldBoss,
                (int) _this.State);
        }

        private DamageListForServer GetServerDamageList(WorldBoss _this, List<ulong> players, ulong lastPlayer = 0)
        {
            var ret = new DamageListForServer();
            var retData = ret.Data;
            var topPlayers = ret.TopPlayers;
            for (int i = 0, imax = _this.DamageList.Count > 5 ? 5 : _this.DamageList.Count; i < imax; ++i)
            {
                topPlayers.Add(_this.DamageList[i]);
            }
            foreach (var playerId in players)
            {
                retData.Add(_this.PlayerDamages[playerId]);
            }
            retData.Sort(_this.DuComparer);
            ret.TotalDamage = _this.TotalDamage;
            ret.LastPlayer = lastPlayer;
            return ret;
        }

        #endregion
    }

    public class WorldBoss
    {
        #region 数据

        public int ServerId;
        //server guid => player guid list
        public Dictionary<ulong, List<ulong>> ServerPlayers;

        //玩家guid到伤害的对应关系
        public Dictionary<ulong, DamageUnit> PlayerDamages;

        //所有玩家的伤害列表
        public List<DamageUnit> DamageList;

        public DamageUnitComparer DuComparer = new DamageUnitComparer();

        public int SetDbFailCounter;
        public bool mBossAlive;
        public int TotalDamage;
        public int BossMaxHp;
        public int mBossNpcId;
        public eActivityState State = eActivityState.WillStart;

        public int BossLevel = -1;

        public void SetBossLevel(int level)
        {
            mImpl.SetBossLevel(this, level);
        }

        private static IWorldBoss mImpl;

        #endregion

        #region Init

        static WorldBoss()
        {
            ActivityServer.Instance.UpdateManager.InitStaticImpl(typeof (WorldBoss), typeof (WorldBossDefaultImpl),
                o => { mImpl = (IWorldBoss) o; });
        }

        public WorldBoss(int serverId, eActivityState state)
        {
            mImpl.Construct(this, serverId, state);
        }

        #endregion

        #region 对外方法

        public DamageListForServer ApplyDamage(ulong sceneGuid, DamageList damage)
        {
            return mImpl.ApplyDamage(this, sceneGuid, damage);
        }

        public void BossWillStart()
        {
            mImpl.BossWillStart(this);
        }

        public void BossStart()
        {
            mImpl.BossStart(this);
        }

        public void BossStop()
        {
            mImpl.BossStop(this);
        }

        public void BossNext()
        {
            mImpl.BossNext(this);
        }

        #endregion
    }

    public interface IWorldBossManager
    {
        DamageListForServer ApplyDamage(int serverId, ulong sceneGuid, DamageList damage);
        eActivityState GetState(int serverId);
        void Init();
    }

    public class WorldBossManagerDefaultImpl : IWorldBossManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Init()
        {
            var tbScene = Table.GetScene(6000);
            var tbFuben = Table.GetFuben(tbScene.FubenId);
            WorldBossManager.OpenLastMin = tbFuben.OpenLastMinutes;
            WorldBossManager.TimeLimitMin = tbFuben.TimeLimitMinutes;
            var dungeonTotalTime = WorldBossManager.OpenLastMin + WorldBossManager.TimeLimitMin;

            //检查副本开启时间
            var now = DateTime.Now;
            foreach (var time in tbFuben.OpenTime)
            {
                var tarTime = new DateTime(now.Year, now.Month, now.Day, time/100, time%100, 0, DateTimeKind.Local);
                if (tarTime.AddMinutes(dungeonTotalTime) < now)
                {
                    tarTime = tarTime.AddDays(1);
                    ++WorldBossManager.TimeIdx;
                }
                WorldBossManager.TargetTimes.Add(tarTime);
            }
            WorldBossManager.TimeIdx = WorldBossManager.TimeIdx%WorldBossManager.TargetTimes.Count;

            //计算当前的世界boss副本状态
            var openTime = WorldBossManager.TargetTimes[WorldBossManager.TimeIdx];
            var startTime = openTime.AddMinutes(WorldBossManager.OpenLastMin);
            var endTime = startTime.AddMinutes(WorldBossManager.TimeLimitMin);

            eActivityState state;
            if (openTime > now)
            {
                state = eActivityState.WaitNext;
            }
            else if (startTime > now)
            {
                state = eActivityState.WillStart;
            }
            else if (endTime > now)
            {
                state = eActivityState.Start;
            }
            else
            {
                state = eActivityState.WaitNext;
            }

            Table.ForeachServerName(record =>
            {
                var id = record.LogicID;
                if (!WorldBossManager.Boss.ContainsKey(id))
                {
                    WorldBossManager.Boss.Add(id, new WorldBoss(id, state));
                }
                return true;
            });

            WaitNextBoss();
            EventDispatcher.Instance.AddEventListener(ReloadTableEvent.EVENT_TYPE, ReloadTable);
        }
         private static void ReloadTable(IEvent ievent)
        {
            var v = ievent as ReloadTableEvent;
            if (v == null)
                return;
            if (v.tableName == "ServerName")
            {
                eActivityState state = eActivityState.WaitNext;
                foreach (var val in WorldBossManager.Boss)
                {
                    state = val.Value.State;
                    break;
                }
                Table.ForeachServerName(record =>
                {
                    var id = record.LogicID;
                    if (!WorldBossManager.Boss.ContainsKey(id))
                    {
                        WorldBossManager.Boss.Add(id, new WorldBoss(id, state));
                    }
                    return true;
                });
            }
        }

        public DamageListForServer ApplyDamage(int serverId, ulong sceneGuid, DamageList damage)
        {
            WorldBoss boss;
            if (WorldBossManager.Boss.TryGetValue(serverId, out boss))
            {
                return boss.ApplyDamage(sceneGuid, damage);
            }
            return null;
        }

        public eActivityState GetState(int serverId)
        {
            WorldBoss boss;
            if (WorldBossManager.Boss.TryGetValue(serverId, out boss))
            {
                return boss.State;
            }
            return eActivityState.End;
        }

        #region 私有方法

        private void WaitNextBoss()
        {
            var tarTime = WorldBossManager.TargetTimes[WorldBossManager.TimeIdx];
            WorldBossManager.OpenTrigger = ActivityServerControl.Timer.CreateTrigger(tarTime, BossWillStart);

            tarTime = tarTime.AddMinutes(WorldBossManager.OpenLastMin);
            WorldBossManager.StartTrigger = ActivityServerControl.Timer.CreateTrigger(tarTime, BossStart);

            tarTime = tarTime.AddMinutes(WorldBossManager.TimeLimitMin);
            WorldBossManager.StopTrigger = ActivityServerControl.Timer.CreateTrigger(tarTime, BossStop);

            tarTime = tarTime.AddMinutes(2);
            ActivityServerControl.Timer.CreateTrigger(tarTime, BossNext);

            WorldBossManager.TargetTimes[WorldBossManager.TimeIdx] =
                WorldBossManager.TargetTimes[WorldBossManager.TimeIdx].AddDays(1);
            WorldBossManager.TimeIdx = ++WorldBossManager.TimeIdx%WorldBossManager.TargetTimes.Count;
        }

        private void BossWillStart()
        {
            foreach (var worldBoss in WorldBossManager.Boss.Values)
            {
                worldBoss.BossWillStart();
            }
        }

        private void BossStart()
        {
            foreach (var worldBoss in WorldBossManager.Boss.Values)
            {
                worldBoss.BossStart();
            }
        }

        private void BossStop()
        {
            foreach (var worldBoss in WorldBossManager.Boss.Values)
            {
                worldBoss.BossStop();
            }
        }

        private void BossNext()
        {
            foreach (var worldBoss in WorldBossManager.Boss.Values)
            {
                worldBoss.BossNext();
            }
            WaitNextBoss();
        }

        #endregion
    }

    public static class WorldBossManager
    {
        // server logic id => WorldBoss
        public static Dictionary<int, WorldBoss> Boss = new Dictionary<int, WorldBoss>();
        private static IWorldBossManager mImpl;
        public static int OpenLastMin;
        public static object OpenTrigger; //活动开始进入
        public static object StartTrigger; //活动正式开始
        public static object StopTrigger; //活动时间结束
        public static List<DateTime> TargetTimes = new List<DateTime>();
        public static int TimeIdx;
        public static float TimeLimitMin;

        static WorldBossManager()
        {
            ActivityServer.Instance.UpdateManager.InitStaticImpl(typeof (WorldBossManager),
                typeof (WorldBossManagerDefaultImpl),
                o => { mImpl = (IWorldBossManager) o; });
        }

        public static DamageListForServer ApplyDamage(int serverId, ulong sceneGuid, DamageList damage)
        {
            return mImpl.ApplyDamage(serverId, sceneGuid, damage);
        }

        public static eActivityState GetState(int serverId)
        {
            return mImpl.GetState(serverId);
        }

        public static void Init()
        {
            mImpl.Init();
        }
    }
}