#region using

using System.Collections.Generic;
using NLog;
using Shared;

#endregion

namespace Team
{
    //队伍状态
    public enum eTeamState
    {
        Tick = 0, //在心跳中等待新玩家匹配进入
        WaitBack, //已排队完毕，等待玩家回复
        Enter, //等待玩家进入副本
        Failed //有人拒绝，进入散队状态
    }

    //系统排出来的队伍
    public interface IQueueTeam
    {
        void Construct(QueueTeam _this, ulong teamId, int queueId);
        void PushCharacter(QueueTeam _this, ulong cId);
        void PushLog(QueueTeam _this);
        void RemoveCharacter(QueueTeam _this, ulong cId);
    }

    public class QueueTeamDefaultImpl : IQueueTeam
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Construct(QueueTeam _this, ulong teamId, int queueId)
        {
            _this.TeamId = teamId;
            _this.mQueueId = queueId;
        }

        public void PushCharacter(QueueTeam _this, ulong cId)
        {
            _this.TeamList.Add(cId);
            QueueTeamManager.mCharacters[cId] = _this;
        }

        //移除玩家从队伍
        public void RemoveCharacter(QueueTeam _this, ulong cId)
        {
            _this.TeamList.Remove(cId);
            QueueTeamManager.RemoveCharacter(cId);
            if (_this.TeamList.Count == 0)
            {
                QueueTeamManager.RemoveTeam(_this.TeamId);
            }
        }

        public void PushLog(QueueTeam _this)
        {
            PlayerLog.WriteLog((int) LogType.QueueLog, "      t={0},c={1},d={2}", _this.TeamId, _this.TeamList.Count,
                _this.TeamList.GetDataString());
        }
    }

    //系统排出来的队伍
    public class QueueTeam
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static IQueueTeam mImpl;

        static QueueTeam()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (QueueTeam), typeof (QueueTeamDefaultImpl),
                o => { mImpl = (IQueueTeam) o; });
        }

        public QueueTeam(ulong teamId, int queueId)
        {
            mImpl.Construct(this, teamId, queueId);
        }

        public int mQueueId = -1;
        public eTeamState State = eTeamState.Tick;
        public List<ulong> TeamList = new List<ulong>(); //队伍成员列表
        public ulong TeamId { get; set; }

        public void PushCharacter(ulong cId)
        {
            mImpl.PushCharacter(this, cId);
        }

        public void PushLog()
        {
            mImpl.PushLog(this);
        }

        //移除玩家从队伍
        public void RemoveCharacter(ulong cId)
        {
            mImpl.RemoveCharacter(this, cId);
        }
    }

    //系统排出来的队伍管理器
    public interface IQueueTeamManager
    {
        void AutoEnterTeam(FightQueueScene scene, ulong guid, int camp);
        QueueTeam CreateTeam(int q);
        QueueTeam GetCharacterTeam(ulong characterId);
        void LeaveScene(ulong characterId);
        void PushLog();
        void RemoveCharacter(ulong characterId);
        void RemoveTeam(ulong teamId);
    }

    public class QueueTeamManagerDefaultImpl : IQueueTeamManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //获得下一个队伍ID
        private ulong GetNextTeamId()
        {
            return TeamManager.GetNextTeamId();
        }

        //创造队伍
        public QueueTeam CreateTeam(int q)
        {
            var id = GetNextTeamId();
            var qt = new QueueTeam(id, q);
            QueueTeamManager.mTeams.Add(qt.TeamId, qt);
            return qt;
        }

        //获得某个玩家所在的队伍
        public QueueTeam GetCharacterTeam(ulong characterId)
        {
            QueueTeam character;
            if (QueueTeamManager.mCharacters.TryGetValue(characterId, out character))
            {
                return character;
            }
            return null;
        }

        public void RemoveCharacter(ulong characterId)
        {
            QueueTeamManager.mCharacters.Remove(characterId);
        }

        //移除一个队伍
        public void RemoveTeam(ulong teamId)
        {
            QueueTeamManager.mTeams.Remove(teamId);
        }

        //离开场景
        public void LeaveScene(ulong characterId)
        {
            var team = GetCharacterTeam(characterId);
            if (team == null)
            {
                return;
            }
            team.RemoveCharacter(characterId);
        }

        //按照某个玩家的阵营，自动分队
        public void AutoEnterTeam(FightQueueScene scene, ulong guid, int camp)
        {
            var team = scene.team1;
            if (camp == 1)
            {
                team = scene.team2;
            }
            foreach (var i in team)
            {
                if (i.Key == guid)
                {
                    continue;
                }
                var t = GetCharacterTeam(i.Key);
                if (t == null)
                {
                    continue;
                }
                if (t.TeamList.Count < 5)
                {
                    t.PushCharacter(guid);
                    return;
                }
            }
        }

        //log
        public void PushLog()
        {
            PlayerLog.WriteLog((int) LogType.QueueLog, "QueueTeamManager -------------------------");
            PlayerLog.WriteLog((int) LogType.QueueLog, "QueueTeamManager.mCharacters count={0}",
                QueueTeamManager.mCharacters.Count);
            PlayerLog.WriteLog((int) LogType.QueueLog, "QueueTeamManager.mTeams count={0}",
                QueueTeamManager.mTeams.Count);
            foreach (var mTeam in QueueTeamManager.mTeams)
            {
                mTeam.Value.PushLog();
            }
            PlayerLog.WriteLog((int) LogType.QueueLog, "QueueTeamManager -------------------------");
        }
    }

    //系统排出来的队伍管理器
    public static class QueueTeamManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static Dictionary<ulong, QueueTeam> mCharacters = new Dictionary<ulong, QueueTeam>();
        private static IQueueTeamManager mImpl;
        public static Dictionary<ulong, QueueTeam> mTeams = new Dictionary<ulong, QueueTeam>();

        static QueueTeamManager()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (QueueTeamManager),
                typeof (QueueTeamManagerDefaultImpl),
                o => { mImpl = (IQueueTeamManager) o; });
        }

        //按照某个玩家的阵营，自动分队
        public static void AutoEnterTeam(FightQueueScene scene, ulong guid, int camp)
        {
            mImpl.AutoEnterTeam(scene, guid, camp);
        }

        //创造队伍
        public static QueueTeam CreateTeam(int q)
        {
            return mImpl.CreateTeam(q);
        }

        //获得某个玩家所在的队伍
        public static QueueTeam GetCharacterTeam(ulong characterId)
        {
            return mImpl.GetCharacterTeam(characterId);
        }

        //离开场景
        public static void LeaveScene(ulong characterId)
        {
            mImpl.LeaveScene(characterId);
        }

        //log
        public static void PushLog()
        {
            mImpl.PushLog();
        }

        public static void RemoveCharacter(ulong characterId)
        {
            mImpl.RemoveCharacter(characterId);
        }

        //移除一个队伍
        public static void RemoveTeam(ulong teamId)
        {
            mImpl.RemoveTeam(teamId);
        }
    }
}