#region using

using System.Collections.Generic;
using DataTable;
using NLog;
using Shared;

#endregion

namespace Team
{
    internal interface IActivityFubenQueue
    {
        void Construct(ActivityFubenQueue _this, int queueId);

        void DealWithTeamChange(ActivityFubenQueue _this,
                                TeamChangedType type,
                                QueueCharacter character,
                                List<ulong> teamList,
                                ulong characterId);

        void MatchOver(ActivityFubenQueue _this);
        void StartTrigger(ActivityFubenQueue _this);
    }

    internal class ActivityFubenQueueDefaultImpl : IActivityFubenQueue
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Construct(ActivityFubenQueue _this, int queueId)
        {
        }

        public void StartTrigger(ActivityFubenQueue _this)
        {
            if (_this.mTrigger != null)
            {
                Logger.Error("StartTrigger! mTrigger != null");
                return;
            }
            var fubenId = _this.tbQueue.Param;
            var tbFuben = Table.GetFuben(fubenId);
            if (tbFuben == null)
            {
                Logger.Error("StartTrigger! tbFuben == null");
                return;
            }
            var time = Utils.GetNextDungeonOpenTime(tbFuben);
            _this.mTrigger = TeamServerControl.tm.CreateTrigger(time, _this.MatchOver);
        }

        //通知匹配成功
        public void MatchOver(ActivityFubenQueue _this)
        {
            foreach (var queueCharacter in _this.mCharacters)
            {
                var result = new QueueResult(_this);
                result.PushOneCharacter(queueCharacter);
                result.StartTrigger();
            }

#if DEBUG
            PlayerLog.WriteLog((int) LogType.QueueMessage, "MatchOver mCharacters.Clear()");
#endif
            _this.mCharacters.Clear();
        }

        public void DealWithTeamChange(ActivityFubenQueue _this,
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
                {
                    var queueCharacter = QueueManager.GetMatchingCharacter(characterId);
                    if (queueCharacter != null)
                    {
                        if (queueCharacter.result == null)
                        {
                            QueueManager.Pop(characterId, eLeaveMatchingType.TeamChange);
                        }
                        else
                        {
                            return;
                        }
                    }
                    QueueManager.PushOneCharacter(characterId, character);
                }
                    break;
                case TeamChangedType.Kick:
                case TeamChangedType.Leave:
                {
//characterId离队的情况
                    if (character.result == null)
                    {
                        var characterData = character.mDatas.Find(data => data.Id == characterId);
                        if (characterData != null)
                        {
                            character.mDatas.Remove(characterData);
                        }
                        else
                        {
                            Logger.Error("Can't find character data for character[{0}]", characterId);
                        }
                        QueueManager.Remove(characterId, eLeaveMatchingType.TeamChange);
                    }
                }
                    break;
                case TeamChangedType.Disband:
                    break;
            }
        }
    }

    internal class ActivityFubenQueue : QueueLogic
    {
        private static IActivityFubenQueue mImpl;

        static ActivityFubenQueue()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (ActivityFubenQueue),
                typeof (ActivityFubenQueueDefaultImpl),
                o => { mImpl = (IActivityFubenQueue) o; });
        }

        public ActivityFubenQueue(int queueId)
            : base(queueId)
        {
            mImpl.Construct(this, queueId);
        }

        public override void DealWithTeamChange(TeamChangedType type,
                                                QueueCharacter character,
                                                List<ulong> teamList,
                                                ulong characterId)
        {
            mImpl.DealWithTeamChange(this, type, character, teamList, characterId);
        }

        //通知匹配成功
        public override void MatchOver()
        {
            mImpl.MatchOver(this);
        }

        public override void StartTrigger()
        {
            mImpl.StartTrigger(this);
        }
    }
}