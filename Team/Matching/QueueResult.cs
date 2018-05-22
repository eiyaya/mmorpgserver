#region using

using DataContract;
using NLog;

#endregion

namespace Team
{
    public interface IQueueResult
    {
        //排队结果，所有人正常同意
        void AllOK(QueueResult _this);
        void PushOneCharacter(QueueResult _this, QueueCharacter queue);
        void RemoveCharacterOne(QueueResult _this, QueueCharacter character);
    }

    public class QueueResultDefaultImpl : IQueueResult
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //重新组织新队长
        private void ResetNewLeader(QueueResultBase _this)
        {
            foreach (var list in _this.mTeamList)
            {
                //筛选队长
                CharacterSimpleData LeaderCharacter = null;
                var maxCount = 0;
                var FightPoint = 0;
                foreach (var qCharacters in list)
                {
                    foreach (var matchingCharacter in qCharacters.mDatas)
                    {
                        var character = TeamManager.GetCharacterTeam(matchingCharacter.Id);
                        if (character != null)
                        {
                            if (character.TeamState == TeamState.Leader)
                            {
                                if (character.team.GetTeamCount() > maxCount)
                                {
                                    maxCount = character.team.GetTeamCount();
                                    LeaderCharacter = matchingCharacter;
                                    FightPoint = matchingCharacter.FightPoint;
                                }
                                else if (character.team.GetTeamCount() == maxCount)
                                {
                                    if (matchingCharacter.FightPoint > FightPoint)
                                    {
                                        FightPoint = matchingCharacter.FightPoint;
                                        LeaderCharacter = matchingCharacter;
                                    }
                                }
                                //character.team.Disband();
                            }
                        }
                        else
                        {
                            if (matchingCharacter.FightPoint > FightPoint && LeaderCharacter == null)
                            {
                                FightPoint = matchingCharacter.FightPoint;
                                LeaderCharacter = matchingCharacter;
                            }
                        }
                    }
                }
                if (LeaderCharacter == null)
                {
                    return;
                }
                //组队
                var tempTeam = QueueTeamManager.CreateTeam(_this.mQueue.mQueueId);
                _this.newTeams.Add(tempTeam);
                tempTeam.PushCharacter(LeaderCharacter.Id);

                foreach (var qCharacters in list)
                {
                    qCharacters.team = tempTeam;
                    foreach (var matchingCharacter in qCharacters.mDatas)
                    {
                        if (LeaderCharacter != matchingCharacter)
                        {
                            tempTeam.PushCharacter(matchingCharacter.Id);
                        }
                    }
                }
            }
        }

        public void PushOneCharacter(QueueResult _this, QueueCharacter queue)
        {
            QueueResultBase.GetImpl().PushOneCharacter(_this, queue);
        }

        public void RemoveCharacterOne(QueueResult _this, QueueCharacter character)
        {
            QueueResultBase.GetImpl().RemoveCharacterOne(_this, character);
            _this.PushFront(eLeaveMatchingType.OtherRefuse);
        }

        public void AllOK(QueueResult _this)
        {
            _this.mQueue.OnAllOk(_this);
            ResetNewLeader(_this);
            _this.EnterFuben();
            _this.mTeamList.Clear();
        }
    }

    public class QueueResult : QueueResultBase
    {
        private static IQueueResult mImpl;

        static QueueResult()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (QueueResult), typeof (QueueResultDefaultImpl),
                o => { mImpl = (IQueueResult) o; });
        }

        public QueueResult(QueueLogic q) : base(q)
        {
        }

        //排队结果，所有人正常同意
        public override void AllOK()
        {
            mImpl.AllOK(this);
        }

        public override void PushOneCharacter(QueueCharacter queue)
        {
            mImpl.PushOneCharacter(this, queue);
        }

        //排队结果，移除一个人
        public override void RemoveCharacterOne(QueueCharacter character)
        {
            mImpl.RemoveCharacterOne(this, character);
        }
    }
}