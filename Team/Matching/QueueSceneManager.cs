#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataContract;
using DataTable;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Team
{
    public enum SceneState
    {
        Init = 0, //初始化
        Full = 1, //满了
        NoFull = 2 //缺人
    }

    //对战类的副本结果(准备模仿WOW战场做的）
    public interface IFightQueueScene
    {
        void Construct(FightQueueScene _this, ulong sceneGuid, int queueId);
        void EnterScene(FightQueueScene _this, ulong cId, ulong sceneGuid);
        void FollowCharacter(FightQueueScene _this, QueueCharacter c, int camp);
        void FollowResult(FightQueueScene _this, QueueCharacter c, int result);
        void InitCharacter(FightQueueScene _this, ulong cId, int camp);
        void LeaveScene(FightQueueScene _this, ulong cId, ulong sceneGuid);
        void RemoveFollowCharacter(FightQueueScene _this, QueueCharacter c);
        void SetState(FightQueueScene _this, SceneState value);
    }

    public class FightQueueSceneDefaultImpl : IFightQueueScene
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //获取玩家在哪个阵营
        private int GetCharacterCamp(FightQueueScene _this, ulong cId)
        {
            if (_this.team1.ContainsKey(cId))
            {
                return 0;
            }
            if (_this.team2.ContainsKey(cId))
            {
                return 1;
            }
            return -1;
        }

        //通知玩家修改PvP的Camp
        private static IEnumerator NotifyCharacterSceneCamp(Coroutine co,
                                                            FightQueueScene _this,
                                                            ulong character,
                                                            int camp)
        {
            var msgChgScene = TeamServer.Instance.SceneAgent.SSPvPSceneCampSet(character, camp);
            yield return msgChgScene.SendAndWaitUntilDone(co);
        }

        //开始创建副本
        private static IEnumerator NotifyCreateChangeSceneCoroutine(Coroutine co,
                                                                    FightQueueScene _this,
                                                                    List<ulong> characters,
                                                                    int serverId)
        {
            //排队创建场景时，根据合服ID进行
            var serverLogicId = SceneExtension.GetServerLogicId(serverId);
            PlayerLog.WriteLog((int) LogType.QueueMessage, "NotifyCreateChangeSceneCoroutine  Team={0}",
                characters.GetDataString());
            var sceneInfo = new ChangeSceneInfo
            {
                SceneId = -1,
                ServerId = serverLogicId,
                SceneGuid = _this.mSceneGuid,
                Type = (int) eScnenChangeType.EnterDungeon
            };
            sceneInfo.Guids.AddRange(characters);
            var msgChgScene = TeamServer.Instance.SceneAgent.SBChangeSceneByTeam(characters[0], sceneInfo);
            yield return msgChgScene.SendAndWaitUntilDone(co);
            if (msgChgScene.State != MessageState.Reply)
            {
                yield break;
            }
            if (msgChgScene.Response == 0)
            {
                foreach (var characterGuid in characters)
                {
                    PlayerLog.WriteLog((int) LogType.BattleLog, "SSCharacterEnterBattle   Faild   c={0},s={1}",
                        characterGuid, _this.mSceneGuid);
                    QueueTeamManager.LeaveScene(characterGuid);
                    QueueSceneManager.LeaveScene(characterGuid, _this.mSceneGuid);
                }
            }
        }

        private void Stoptrigger(FightQueueScene _this)
        {
            if (_this.mTrigger != null)
            {
                TeamServerControl.tm.DeleteTrigger(_this.mTrigger);
                _this.mTrigger = null;
            }
        }

        private void TimeOver(FightQueueScene _this)
        {
            List<ulong> leaves = new List<ulong>();
            foreach (var i in _this.team1)
            {
                if (i.Value == 0)
                {
                    leaves.Add(i.Key);
                }
            }
            foreach (var i in _this.team2)
            {
                if (i.Value == 0)
                {
                    leaves.Add(i.Key);
                }
            }
            // 移除超时的
            foreach (var id in leaves)
            {
                LeaveScene(_this, id, _this.mSceneGuid);
            }
            //if (State == SceneState.Init)
            //{
            //    //一直没人进来
            //    FightMatchingSceneManager.CleanScene(_this);
            //    return;
            //}
            Logger.Warn("FightMatchingScene TimeOver State ={0}", _this.mState);
        }

        public void Construct(FightQueueScene _this, ulong sceneGuid, int queueId)
        {
            _this.QueueId = queueId;
            _this.mSceneGuid = sceneGuid;
            _this.mTrigger = TeamServerControl.tm.CreateTrigger(DateTime.Now.AddSeconds(20), () => TimeOver(_this));
            _this.mStartTime = DateTime.Now;
        }

        public void SetState(FightQueueScene _this, SceneState value)
        {
            switch (value)
            {
                case SceneState.Init: //初始状态
                {
                }
                    break;
                case SceneState.Full: //已全部进场景状态
                {
                    Stoptrigger(_this);
                    QueueSceneManager.EnterFull(_this);
                }
                    break;
                case SceneState.NoFull: //缺人状态
                {
                    if (_this.mState != SceneState.NoFull)
                    {
                        QueueSceneManager.FirstNotFull(_this);
                    }
                }
                    break;
            }
            _this.mState = value;
        }

        //初始化添加某个玩家进入
        public void InitCharacter(FightQueueScene _this, ulong cId, int camp)
        {
            Dictionary<ulong, int> team;
            if (camp == 0)
            {
                team = _this.team1;
            }
            else
            {
                team = _this.team2;
            }
            if (team.ContainsKey(cId))
            {
                Logger.Error("InitCharacter cId = {0}", cId);
            }
            else
            {
                team.Add(cId, 0);
            }
            if (QueueSceneManager.characters.ContainsKey(cId))
            {
                QueueSceneManager.characters[cId] = _this;
            }
            else
            {
                QueueSceneManager.characters.Add(cId, _this);
            }
        }

        //后续排入的某个玩家进入
        public void FollowCharacter(FightQueueScene _this, QueueCharacter c, int camp)
        {
            PlayerLog.WriteLog((int) LogType.QueueMessage, "FollowCharacter c={0}, ids={1}", c.Guid,
                c.mDatas.Select(d => d.Id).GetDataString());
            var result = new FollowQueueResult(c.mLogic);
            result.PushOneCharacter(c);
            result.StartTrigger();
            c.mLogic.Pop(c, eLeaveMatchingType.InTemp);
            _this.FollowCharacters.Add(c);
            foreach (var data in c.mDatas)
            {
                InitCharacter(_this, data.Id, camp);
            }
        }

        //后续排队的人返回结果了
        public void FollowResult(FightQueueScene _this, QueueCharacter c, int result)
        {
            if (!_this.FollowCharacters.Contains(c))
            {
                Logger.Warn("FollowResult error! c={0},r={1}", c.Guid, result);
                return;
            }
            PlayerLog.WriteLog((int) LogType.QueueMessage, "FollowResult(). c={0}, ids={1}, result={2}", c.Guid,
                c.mDatas.Select(d => d.Id).GetDataString(), result);
            _this.FollowCharacters.Remove(c);
            Stoptrigger(_this);
            if (result == 0)
            {
//拒绝了
                RemoveFollowCharacter(_this, c);
            }
            else
            {
//同意了
                _this.mStateCount += c.mDatas.Count;
                var tbQueue = Table.GetQueue(_this.QueueId);
                if (_this.mStateCount == tbQueue.CountLimit*2)
                {
                    SetState(_this, SceneState.Full);
                }
                var charList = new List<ulong>();
                foreach (var data in c.mDatas)
                {
                    var id = data.Id;
                    charList.Add(id);
                    var camp = GetCharacterCamp(_this, id);
                    if (camp == -1)
                    {
                        Logger.Error("FollowResult camp error!c={0}", c);
                        return;
                    }
                    //把该玩家的排队队伍整理一下
                    QueueTeamManager.AutoEnterTeam(_this, id, camp);
                    CoroutineFactory.NewCoroutine(NotifyCharacterSceneCamp, _this, id, camp).MoveNext();
                }
                CoroutineFactory.NewCoroutine(NotifyCreateChangeSceneCoroutine, _this, charList, c.mDatas[0].ServerId)
                    .MoveNext();
            }
        }

        public void RemoveFollowCharacter(FightQueueScene _this, QueueCharacter c)
        {
#if DEBUG
            PlayerLog.WriteLog((int) LogType.QueueMessage, "RemoveFollowCharacter(). c={0}, ids={1}", c.Guid,
                c.mDatas.Select(d => d.Id).GetDataString());
#endif
            _this.FollowCharacters.Remove(c);
            foreach (var data in c.mDatas)
            {
                var id = data.Id;
                QueueSceneManager.characters.Remove(id);
                int state;
                if (_this.team1.TryGetValue(id, out state))
                {
                    _this.team1.Remove(id);
#if DEBUG
                    PlayerLog.WriteLog((int) LogType.QueueMessage, "RemoveFollowCharacter() team1.Remove id={0}", id);
#endif
                }
                else if (_this.team2.TryGetValue(id, out state))
                {
                    _this.team2.Remove(id);
#if DEBUG
                    PlayerLog.WriteLog((int) LogType.QueueMessage, "RemoveFollowCharacter() team2.Remove id={0}", id);
#endif
                }
                else
                {
                    Logger.Warn("FightMatchingScene LeaveScene cId={0},sceneGuid={1}", id, _this.mSceneGuid);
                }
            }
        }

        //有人进入场景了
        public void EnterScene(FightQueueScene _this, ulong cId, ulong sceneGuid)
        {
            if (_this.mSceneGuid == 0)
            {
                _this.mSceneGuid = sceneGuid;
            }
            int state;
            if (_this.team1.TryGetValue(cId, out state))
            {
                if (state == 0)
                {
#if DEBUG
                    PlayerLog.WriteLog((int) LogType.QueueMessage, "EnterScene() team1[{0}] = 1", cId);
#endif
                    _this.team1[cId] = 1;
                    _this.mStateCount++;
                    if (_this.mStateCount == Table.GetQueue(_this.QueueId).CountLimit*2)
                    {
                        SetState(_this, SceneState.Full);
                    }
                }
            }
            else if (_this.team2.TryGetValue(cId, out state))
            {
                if (state == 0)
                {
#if DEBUG
                    PlayerLog.WriteLog((int) LogType.QueueMessage, "EnterScene() team2[{0}] = 1", cId);
#endif
                    _this.team2[cId] = 1;
                    _this.mStateCount++;
                    if (_this.mStateCount == Table.GetQueue(_this.QueueId).CountLimit*2)
                    {
                        SetState(_this, SceneState.Full);
                    }
                }
            }
            else
            {
                Logger.Warn("FightMatchingScene EnterScene cId={0},sceneGuid={1}", cId, sceneGuid);
            }
        }

        //有人退出场景了
        public void LeaveScene(FightQueueScene _this, ulong cId, ulong sceneGuid)
        {
            if (sceneGuid != _this.mSceneGuid)
            {
                Logger.Warn("FightMatchingScene LeaveScene scene not same! cId={0},sceneGuid={1},teamGuid ={2}", cId,
                    sceneGuid, _this.mSceneGuid);
                return;
            }
            QueueSceneManager.characters.Remove(cId);
            int state;
            if (_this.team1.TryGetValue(cId, out state))
            {
#if DEBUG
                PlayerLog.WriteLog((int) LogType.QueueMessage, "LeaveScene() team1.Remove id={0}", cId);
#endif
                _this.team1.Remove(cId);
                _this.mStateCount--;
                SetState(_this, SceneState.NoFull);
                var t = QueueTeamManager.GetCharacterTeam(cId);
                if (t != null)
                {
                    t.RemoveCharacter(cId);
                }
            }
            else if (_this.team2.TryGetValue(cId, out state))
            {
#if DEBUG
                PlayerLog.WriteLog((int) LogType.QueueMessage, "LeaveScene() team2.Remove id={0}", cId);
#endif
                _this.team2.Remove(cId);
                _this.mStateCount--;
                SetState(_this, SceneState.NoFull);
                var t = QueueTeamManager.GetCharacterTeam(cId);
                if (t != null)
                {
                    t.RemoveCharacter(cId);
                }
            }
            else
            {
                Logger.Warn("FightMatchingScene LeaveScene cId={0},sceneGuid={1}", cId, sceneGuid);
            }
        }
    }

    //对战类的副本结果(准备模仿WOW战场做的）
    public class FightQueueScene
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static IFightQueueScene mImpl;

        static FightQueueScene()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (FightQueueScene),
                typeof (FightQueueSceneDefaultImpl),
                o => { mImpl = (IFightQueueScene) o; });
        }

        public FightQueueScene(ulong sceneGuid, int queueId)
        {
            mImpl.Construct(this, sceneGuid, queueId);
        }

        public List<QueueCharacter> FollowCharacters = new List<QueueCharacter>();
        public ulong mSceneGuid;
        public SceneState mState = 0;
        public int mStateCount;
        public Trigger mTrigger;
        public int QueueId;
        public Dictionary<ulong, int> team1 = new Dictionary<ulong, int>(); //队伍1，玩家状态  0=正在进场景  1=已经进入 
        public Dictionary<ulong, int> team2 = new Dictionary<ulong, int>(); //队伍2，玩家状态
        public DateTime mStartTime;

        public SceneState State
        {
            get { return mState; }
        }

        //有人进入场景了
        public void EnterScene(ulong cId, ulong sceneGuid)
        {
            mImpl.EnterScene(this, cId, sceneGuid);
        }

        //后续排入的某个玩家进入
        public void FollowCharacter(QueueCharacter c, int camp)
        {
            mImpl.FollowCharacter(this, c, camp);
        }

        //后续排队的人返回结果了
        public void FollowResult(QueueCharacter c, int result)
        {
            mImpl.FollowResult(this, c, result);
        }

        //初始化添加某个玩家进入
        public void InitCharacter(ulong cId, int camp)
        {
            mImpl.InitCharacter(this, cId, camp);
        }

        //有人退出场景了
        public void LeaveScene(ulong cId, ulong sceneGuid)
        {
            mImpl.LeaveScene(this, cId, sceneGuid);
        }

        public void PushLog()
        {
            var team1Str = "";
            foreach (var c in team1)
            {
                team1Str += string.Format("[{0}={1}]", c.Key, c.Value);
            }

            var team2Str = "";
            foreach (var c in team2)
            {
                team2Str += string.Format("[{0}={1}]", c.Key, c.Value);
            }

            var wait = string.Empty;
            foreach (var character in FollowCharacters)
            {
                wait += character.GetLog();
            }
            PlayerLog.WriteLog((int) LogType.QueueLog, "FightQueueScene QueueId={1} guid={0} t1={2} t2={3} wait={4}",
                mSceneGuid, QueueId, team1Str, team2Str, wait);
        }

        //移除一个queuecharacter
        public void RemoveFollowCharacter(QueueCharacter c)
        {
            mImpl.RemoveFollowCharacter(this, c);
        }

        public void SetState(SceneState value)
        {
            mImpl.SetState(this, value);
        }
    }

    //对战类副本的管理器
    public interface IQueueSceneManager
    {
        FightQueueScene CreateScene(int qId);
        void EnterFull(FightQueueScene scene);
        void EnterScene(ulong cId, ulong sceneGuid);
        void FirstNotFull(FightQueueScene scene);
        FightQueueScene GetCharacterScene(ulong cId);
        List<FightQueueScene> GetQueueNotFullList(int queueId);
        void LeaveScene(ulong cId, ulong sceneGuid);
        void OverScene(ulong sceneGuid);
        void PushLog();
    }

    public class QueueSceneManagerDefaultImpl : IQueueSceneManager
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //某个场景要结束了
        private void CleanScene(FightQueueScene scene)
        {
            foreach (var i in scene.team1)
            {
                QueueSceneManager.characters.Remove(i.Key);
                QueueTeamManager.LeaveScene(i.Key);
#if DEBUG
                PlayerLog.WriteLog((int) LogType.BattleLog, "SSCharacterLeaveBattle  CleanScene c={0},s={1}", i.Key,
                    scene.mSceneGuid);
#endif
            }
            foreach (var i in scene.team2)
            {
                QueueSceneManager.characters.Remove(i.Key);
                QueueTeamManager.LeaveScene(i.Key);
#if DEBUG
                PlayerLog.WriteLog((int) LogType.BattleLog, "SSCharacterLeaveBattle  CleanScene c={0},s={1}", i.Key,
                    scene.mSceneGuid);
#endif
            }
            foreach (var character in scene.FollowCharacters)
            {
                foreach (var data in character.mDatas)
                {
                    TeamServer.Instance.ServerControl.TeamServerMessage(data.Id, (int) eLeaveMatchingType.SceneOver,
                        string.Empty);
                }
                character.mLogic.PushFront(character);
            }
            scene.FollowCharacters.Clear();
#if DEBUG
            PlayerLog.WriteLog((int) LogType.QueueMessage, "CleanScene(). scene.FollowCharacters.Clear(). scene={0}",
                scene.mSceneGuid);
#endif
            switch (scene.State)
            {
                case SceneState.Init:
                {
                }
                    break;
                case SceneState.Full:
                {
                }
                    break;
                case SceneState.NoFull:
                {
                }
                    break;
            }
        }

        //某个队满了又
        private void RemoveSceneQueue(FightQueueScene scene)
        {
            var list = GetQueueNotFullList(scene.QueueId);
            if (list == null)
            {
                return;
            }
            list.Remove(scene);
        }

        //获得某个玩家的所在场景
        public FightQueueScene GetCharacterScene(ulong cId)
        {
            FightQueueScene temp;
            if (QueueSceneManager.characters.TryGetValue(cId, out temp))
            {
                return temp;
            }
            return null;
        }

        //一个新的排队OK了
        public FightQueueScene CreateScene(int qId)
        {
            var temp = new FightQueueScene(0, qId);
            QueueSceneManager.newCreateScenes.Add(temp);
            return temp;
        }

        //有人进入场景了
        public void EnterScene(ulong cId, ulong sceneGuid)
        {
            FightQueueScene temp;
            if (QueueSceneManager.characters.TryGetValue(cId, out temp))
            {
                temp.EnterScene(cId, sceneGuid);
                return;
            }
            Logger.Warn("FightMatchingSceneManager EnterScene not find cId={0}", cId);
        }

        //有人退出场景了
        public void LeaveScene(ulong cId, ulong sceneGuid)
        {
            FightQueueScene temp;
            if (QueueSceneManager.characters.TryGetValue(cId, out temp))
            {
                temp.LeaveScene(cId, sceneGuid);
                QueueSceneManager.characters.Remove(cId);
                return;
            }
            Logger.Warn("FightMatchingSceneManager LeaveScene not find cId={0}", cId);
        }

        //某个场景要结束了
        public void OverScene(ulong sceneGuid)
        {
            FightQueueScene temp;
            if (QueueSceneManager.fullScenes.TryGetValue(sceneGuid, out temp))
            {
                if (temp.State != SceneState.Full)
                {
                    Logger.Error("OverScene Error state={0}", sceneGuid);
                }
                QueueSceneManager.fullScenes.Remove(sceneGuid);
                CleanScene(temp);
            }
            else if (QueueSceneManager.noFullScenes.TryGetValue(sceneGuid, out temp))
            {
                RemoveSceneQueue(temp);
                if (temp.State != SceneState.NoFull)
                {
                    Logger.Error("OverScene Error state={0}", sceneGuid);
                }
                QueueSceneManager.noFullScenes.Remove(sceneGuid);
                CleanScene(temp);
            }
            else
            {
                Logger.Error("FightMatchingSceneManager OverScene not find scene={0}", sceneGuid);
            }
        }

        //有场景进满人了
        public void EnterFull(FightQueueScene scene)
        {
            if (scene.mSceneGuid == 0)
            {
                Logger.Error("EnterFull sceneGuid={0}", scene.mSceneGuid);
                return;
            }
            if (scene.State == SceneState.Init)
            {
                QueueSceneManager.newCreateScenes.Remove(scene);
            }
            else if (scene.State == SceneState.NoFull)
            {
                QueueSceneManager.noFullScenes.Remove(scene.mSceneGuid);
                var tempList = GetQueueNotFullList(scene.QueueId);
                tempList.Remove(scene);
            }
            QueueSceneManager.fullScenes.Add(scene.mSceneGuid, scene);
        }

        //有场景缺人了
        public void FirstNotFull(FightQueueScene scene)
        {
            if (scene.State == SceneState.Init)
            {
                QueueSceneManager.newCreateScenes.Remove(scene);
            }
            else if (scene.State == SceneState.Full)
            {
                QueueSceneManager.fullScenes.Remove(scene.mSceneGuid);
            }
            QueueSceneManager.noFullScenes.Add(scene.mSceneGuid, scene);
            var tempList = GetQueueNotFullList(scene.QueueId);
            tempList.Add(scene);
        }

        //获得某个队列还没满的列表
        public List<FightQueueScene> GetQueueNotFullList(int queueId)
        {
            List<FightQueueScene> tempList;
            if (!QueueSceneManager.QueueFight.TryGetValue(queueId, out tempList))
            {
                tempList = new List<FightQueueScene>();
                QueueSceneManager.QueueFight[queueId] = tempList;
            }
            return tempList;
        }

        //log
        public void PushLog()
        {
            PlayerLog.WriteLog((int) LogType.QueueLog, "QueueSceneManager -------------------------");
            PlayerLog.WriteLog((int) LogType.QueueLog, "QueueSceneManager.fullScenes count={0}",
                QueueSceneManager.fullScenes.Count);
            foreach (var fullScene in QueueSceneManager.fullScenes)
            {
                fullScene.Value.PushLog();
            }


            PlayerLog.WriteLog((int) LogType.QueueLog, "QueueSceneManager.noFullScenes count={0}",
                QueueSceneManager.noFullScenes.Count);
            PlayerLog.WriteLog((int) LogType.QueueLog, "QueueSceneManager.characters count={0}",
                QueueSceneManager.characters.Count);
            PlayerLog.WriteLog((int) LogType.QueueLog, "QueueSceneManager.newCreateScenes count={0}",
                QueueSceneManager.newCreateScenes.Count);
            var count = 0;
            foreach (var i in QueueSceneManager.QueueFight)
            {
                count += i.Value.Count;
            }
            PlayerLog.WriteLog((int) LogType.QueueLog, "QueueSceneManager.QueueFight count={0}", count);
            PlayerLog.WriteLog((int) LogType.QueueLog, "QueueSceneManager -------------------------");
        }
    }

    //对战类副本的管理器
    public static class QueueSceneManager
    {
        public static Dictionary<ulong, FightQueueScene> characters = new Dictionary<ulong, FightQueueScene>(); //玩家的数据

        public static Dictionary<ulong, FightQueueScene> fullScenes = new Dictionary<ulong, FightQueueScene>();
            //已经满了的场景

        private static IQueueSceneManager mImpl;
        public static List<FightQueueScene> newCreateScenes = new List<FightQueueScene>(); //刚刚排满的场景

        public static Dictionary<ulong, FightQueueScene> noFullScenes = new Dictionary<ulong, FightQueueScene>();
            //还没有满的场景 

        public static Dictionary<int, List<FightQueueScene>> QueueFight = new Dictionary<int, List<FightQueueScene>>();
            //排队ID为Key  的所有没满的数据

        static QueueSceneManager()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (QueueSceneManager),
                typeof (QueueSceneManagerDefaultImpl),
                o => { mImpl = (IQueueSceneManager) o; });
        }

        public static void ClearScene(int id)
        {
            fullScenes.Clear();
            noFullScenes.Clear();
            characters.Clear();
            newCreateScenes.Clear();
            QueueFight.Clear();
        }

        //一个新的排队OK了
        public static FightQueueScene CreateScene(int qId)
        {
            return mImpl.CreateScene(qId);
        }

        //有场景进满人了
        public static void EnterFull(FightQueueScene scene)
        {
            mImpl.EnterFull(scene);
        }

        //有人进入场景了
        public static void EnterScene(ulong cId, ulong sceneGuid)
        {
            mImpl.EnterScene(cId, sceneGuid);
        }

        //有场景缺人了
        public static void FirstNotFull(FightQueueScene scene)
        {
            mImpl.FirstNotFull(scene);
        }

        //获得某个玩家的所在场景
        public static FightQueueScene GetCharacterScene(ulong cId)
        {
            return mImpl.GetCharacterScene(cId);
        }

        //获得某个队列还没满的列表
        public static List<FightQueueScene> GetQueueNotFullList(int queueId)
        {
            return mImpl.GetQueueNotFullList(queueId);
        }

        //有人退出场景了
        public static void LeaveScene(ulong cId, ulong sceneGuid)
        {
            mImpl.LeaveScene(cId, sceneGuid);
        }

        //某个场景要结束了
        public static void OverScene(ulong sceneGuid)
        {
            mImpl.OverScene(sceneGuid);
        }

        //log
        public static void PushLog()
        {
            mImpl.PushLog();
        }
    }
}