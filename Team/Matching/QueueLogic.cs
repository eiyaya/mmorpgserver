#region using

using System;
using System.Collections.Generic;
using System.Linq;
using DataContract;
using DataTable;
using NLog;
using Shared;

#endregion

namespace Team
{
    public interface IQueueCharacter
    {
        void Construct(QueueCharacter _this, QueueLogic logic, List<CharacterSimpleData> datas);
        string GetLog(QueueCharacter _this);
    }

    public class QueueCharacterDefaultImpl : IQueueCharacter
    {
        public void Construct(QueueCharacter _this, QueueLogic logic, List<CharacterSimpleData> datas)
        {
            _this.mDatas = datas;
            _this.mLogic = logic;
            _this.Guid = QueueCharacter.NextId++;
            _this.StartTime = DateTime.Now;
            PlayerLog.WriteLog((int) LogType.QueueMessage, "Construct Guid={0}, ids={1}", _this.Guid,
                datas.Select(d => d.Id).GetDataString());
#if DEBUG
            var matchs = QueueManager.Matchings.Values;
            foreach (var match in matchs)
            {
                var f = match.mCharacters.Find(c => c.mDatas[0].Id == datas[0].Id);
                if (f != null)
                {
                    PlayerLog.WriteLog((int) LogType.QueueLog,
                        "Duplicate queue, queueId0 = {0}, team0 = {1}, queueId1 = {2}, team1 = {3}", _this.Guid,
                        datas.Select(d => d.Id).GetDataString(), f.Guid, f.mDatas.Select(d => d.Id).GetDataString());
                }
            }
#endif
        }

        public string GetLog(QueueCharacter _this)
        {
            var ccc = "";
            var index = 0;
            foreach (var data in _this.mDatas)
            {
                if (index == 0)
                {
                    ccc = string.Format("[{0}", data.Id);
                }
                else
                {
                    ccc = string.Format("{0}|{1}", ccc, data.Id);
                }
            }
            ccc = string.Format("{0}]", ccc);
            return ccc;
        }
    }

    public class QueueCharacter
    {
        private static IQueueCharacter mImpl;
        public static ulong NextId = 1;

        static QueueCharacter()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (QueueCharacter), typeof (QueueCharacterDefaultImpl),
                o => { mImpl = (IQueueCharacter) o; });
        }

        public QueueCharacter(QueueLogic logic, List<CharacterSimpleData> datas)
        {
            mImpl.Construct(this, logic, datas);
        }

        public ulong Guid;
        public List<CharacterSimpleData> mDatas; //一起排队的成员
        public QueueLogic mLogic; //
        public QueueResultBase result;
        public DateTime StartTime;
        public QueueTeam team;

        public string GetLog()
        {
            return mImpl.GetLog(this);
        }
    }

    public interface IQueueLogic
    {
        void Construct(QueueLogic _this, int queueId);

        void DealWithTeamChange(QueueLogic _this,
                                TeamChangedType type,
                                QueueCharacter character,
                                List<ulong> teamList,
                                ulong characterId);

        int GetAverageTime(QueueLogic _this);
        void MatchOver(QueueLogic _this);
        void OnAllOk(QueueLogic _this, QueueResultBase result);
        void Pop(QueueLogic _this, QueueCharacter character, eLeaveMatchingType type);
        void PushBack(QueueLogic _this, QueueCharacter queueC);
        void PushFront(QueueLogic _this, QueueCharacter queueC);
        void PushLog(QueueLogic _this);
        void PushSuccessTime(QueueLogic _this, int second);
        void StartTrigger(QueueLogic _this);
        void StopTrigger(QueueLogic _this);
        void Tick(QueueLogic _this);
    }

    public class QueueLogicDefaultImpl : IQueueLogic
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Construct(QueueLogic _this, int queueId)
        {
            _this.mQueueId = queueId;
            _this.tbQueue = Table.GetQueue(queueId);
        }

        public void PushLog(QueueLogic _this)
        {
            var dddd = "";
            foreach (var data in _this.mCharacters)
            {
                dddd = dddd + data.Guid + ",";
            }
            PlayerLog.WriteLog((int) LogType.QueueLog, "Matching Id={0} Count[{1}]={2}, AverageTime={3} ",
                _this.mQueueId, _this.mCharacters.Count, dddd, GetAverageTime(_this));
        }

        public void PushBack(QueueLogic _this, QueueCharacter queueC)
        {
            if (_this.mCharacters.Count == 0)
            {
                _this.StartTrigger();
            }
#if DEBUG
            foreach (var data in queueC.mDatas)
            {
                var t = QueueManager.GetMatchingCharacter(data.Id);
                if (t == null)
                {
                    PlayerLog.WriteLog((ulong) LogType.QueueMessage, "PushBack t == null!! c = {0} ids = {1}",
                        queueC.Guid, queueC.mDatas.Select(d => d.Id).GetDataString());
                }
            }
            foreach (var queueCharacter in _this.mCharacters)
            {
                foreach (var datas in queueCharacter.mDatas)
                {
                    foreach (var data in queueC.mDatas)
                    {
                        if (datas.Id == data.Id)
                        {
                            PlayerLog.WriteLog((ulong) LogType.QueueMessage,
                                "PushBack datas.Id == data.Id!! c = {0} ids = {1}",
                                queueC.Guid, queueC.mDatas.Select(d => d.Id).GetDataString());
                        }
                    }
                }
            }
#endif
            PlayerLog.WriteLog((int) LogType.QueueMessage, "PushBack mCharacters.Add c={0}, ids={1}", queueC.Guid,
                queueC.mDatas.Select(d => d.Id).GetDataString());
            _this.mCharacters.Add(queueC);
        }

        public void PushFront(QueueLogic _this, QueueCharacter queueC)
        {
            if (_this.mCharacters.Count == 0)
            {
                _this.StartTrigger();
            }
            PlayerLog.WriteLog((int) LogType.QueueMessage, "PushFront  queueC={1},character={0}", queueC.mDatas[0].Id,
                queueC.Guid);
            queueC.result = null;
#if DEBUG
            foreach (var data in queueC.mDatas)
            {
                var t = QueueManager.GetMatchingCharacter(data.Id);
                if (t == null)
                {
                    PlayerLog.WriteLog((ulong) LogType.QueueMessage, "PushFront t == null!! c = {0} ids = {1}",
                        queueC.Guid, queueC.mDatas.Select(d => d.Id).GetDataString());
                }
            }
            foreach (var queueCharacter in _this.mCharacters)
            {
                foreach (var datas in queueCharacter.mDatas)
                {
                    foreach (var data in queueC.mDatas)
                    {
                        if (datas.Id == data.Id)
                        {
                            PlayerLog.WriteLog((ulong) LogType.QueueMessage,
                                "PushFront datas.Id == data.Id!! c = {0} ids = {1}",
                                queueC.Guid, queueC.mDatas.Select(d => d.Id).GetDataString());
                        }
                    }
                }
            }
#endif
            PlayerLog.WriteLog((int) LogType.QueueMessage, "PushFront mCharacters.Insert c={0}, ids={1}", queueC.Guid,
                queueC.mDatas.Select(d => d.Id).GetDataString());
            _this.mCharacters.Insert(0, queueC);
        }

        public void Pop(QueueLogic _this, QueueCharacter character, eLeaveMatchingType type)
        {
            _this.mCharacters.Remove(character);
#if DEBUG
            PlayerLog.WriteLog((int) LogType.QueueMessage, "Pop mCharacters.Remove c={0}, ids={1}, type={2}",
                character.Guid, character.mDatas.Select(d => d.Id).GetDataString(), type);
#endif
            if (_this.mCharacters.Count == 0)
            {
                StopTrigger(_this);
            }
        }

        public void StartTrigger(QueueLogic _this)
        {
            if (_this.mTrigger != null)
            {
                Logger.Warn("StartTrigger!");
                return;
            }
            _this.mTrigger = TeamServerControl.tm.CreateTrigger(DateTime.Now.AddSeconds(1), _this.Tick, 1000);
        }

        //关闭定时器
        public void StopTrigger(QueueLogic _this)
        {
            if (_this.mTrigger != null)
            {
                TeamServerControl.tm.DeleteTrigger(_this.mTrigger);
                _this.mTrigger = null;
            }
        }

        public void Tick(QueueLogic _this)
        {
        }

        public void MatchOver(QueueLogic _this)
        {
        }

        public void OnAllOk(QueueLogic _this, QueueResultBase result)
        {
        }

        public void DealWithTeamChange(QueueLogic _this,
                                       TeamChangedType type,
                                       QueueCharacter character,
                                       List<ulong> teamList,
                                       ulong characterId)
        {
            switch (type)
            {
                case TeamChangedType.Request:
                    break;
                case TeamChangedType.AcceptRequest:
                case TeamChangedType.AcceptJoin:
                    var queueCharacter = QueueManager.GetMatchingCharacter(characterId);
                    if (queueCharacter != null && queueCharacter.result != null)
                    {
                        return;
                    }
                    QueueManager.PushOneCharacter(characterId, character);
                    break;
                case TeamChangedType.Leave:
                case TeamChangedType.Kick:
                    if (character.result == null)
                    {
                        QueueManager.Pop(characterId, eLeaveMatchingType.TeamChange);
                    }
                    break;
                case TeamChangedType.Disband:
                    break;
            }
        }

        #region 时间相关

        public void PushSuccessTime(QueueLogic _this, int second)
        {
            var now = DateTime.Now;
            //超过1小时的会移除
            foreach (var i in _this.MatchOkey)
            {
                if ((now - i.Key).TotalSeconds > 3600)
                {
                    _this.MatchOkey.Remove(i.Key);
                    return;
                }
            }
            _this.MatchOkey[now] = second;
            //超过25个之外的会移除
            if (_this.MatchOkey.Count > 25)
            {
                _this.MatchOkey.Remove(_this.MatchOkey.First().Key);
            }
        }

        public int GetAverageTime(QueueLogic _this)
        {
            if (_this.MatchOkey.Count == 0)
            {
                return -1;
            }
            var totalSeconds = 0;
            foreach (var i in _this.MatchOkey)
            {
                totalSeconds += i.Value;
            }
            var avgTime = totalSeconds/_this.MatchOkey.Count;
            var maxWaitSeconds = 0;
            if (_this.mCharacters.Count > 0)
            {
                var c = _this.mCharacters[0];
                var now = DateTime.Now;
                maxWaitSeconds = (int) (now - c.StartTime).TotalSeconds;
            }
            if (avgTime < maxWaitSeconds)
            {
                return maxWaitSeconds + 60;
            }
            return avgTime;
        }

        #endregion
    }

    public class QueueLogic
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static IQueueLogic mImpl;

        static QueueLogic()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (QueueLogic), typeof (QueueLogicDefaultImpl),
                o => { mImpl = (IQueueLogic) o; });
        }

        public QueueLogic(int queueId)
        {
            mImpl.Construct(this, queueId);
        }

        public List<QueueCharacter> mCharacters = new List<QueueCharacter>(); //正在排队的玩家
        public int mQueueId;
        public Trigger mTrigger;
        public QueueRecord tbQueue;

        public virtual void DealWithTeamChange(TeamChangedType type,
                                               QueueCharacter character,
                                               List<ulong> teamList,
                                               ulong characterId)
        {
            mImpl.DealWithTeamChange(this, type, character, teamList, characterId);
        }

        public virtual void MatchOver()
        {
            mImpl.MatchOver(this);
        }

        //进入战场时响应
        public virtual void OnAllOk(QueueResultBase result)
        {
            mImpl.OnAllOk(this, result);
        }

        public void Pop(QueueCharacter character, eLeaveMatchingType type)
        {
            mImpl.Pop(this, character, type);
        }

        public void PushBack(QueueCharacter queueC)
        {
            mImpl.PushBack(this, queueC);
        }

        public void PushFront(QueueCharacter queueC)
        {
            mImpl.PushFront(this, queueC);
        }

        public void PushLog()
        {
            mImpl.PushLog(this);
        }

        public virtual void StartTrigger()
        {
            mImpl.StartTrigger(this);
        }

        //关闭定时器
        public void StopTrigger()
        {
            mImpl.StopTrigger(this);
        }

        public virtual void Tick()
        {
            mImpl.Tick(this);
        }

        #region 时间相关

        //缓存已经匹配的队伍大概用的时间
        public Dictionary<DateTime, int> MatchOkey = new Dictionary<DateTime, int>();

        public void PushSuccessTime(int second)
        {
            mImpl.PushSuccessTime(this, second);
        }

        public int GetAverageTime()
        {
            return mImpl.GetAverageTime(this);
        }

        #endregion
    }
}