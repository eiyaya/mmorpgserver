#region using

using System;
using System.Collections.Generic;
using System.Linq;
using DataContract;
using Shared;

#endregion

namespace Team
{
    public interface IFightQueue
    {
        void Construct(FightQueue _this, int queueId);
        void MatchOver(FightQueue _this);
        void OnAllOk(FightQueue _this, QueueResultBase result);
        void Tick(FightQueue _this);
    }

    public class FightQueueDefaultImpl : IFightQueue
    {
        public void Construct(FightQueue _this, int queueId)
        {
        }

        public void Tick(FightQueue _this)
        {
            var needCount = _this.tbQueue.CountLimit;
            var needCount1 = _this.tbQueue.CountLimit;
            var needCount2 = _this.tbQueue.CountLimit;

            var noFullSceneList = QueueSceneManager.GetQueueNotFullList(_this.mQueueId);
            foreach (var nofullscene in noFullSceneList)
            {
                needCount1 = needCount - nofullscene.team1.Count;
                needCount2 = needCount - nofullscene.team2.Count;
                if (needCount1 == needCount2)
                {
                    //两边要同时进人
                    if (needCount2 < 1)
                    {
                        continue;
                    }
                    //找人
                    {
                        QueueCharacter gotoTeam1 = null;
                        foreach (var mCharacter in _this.mCharacters)
                        {
                            if (mCharacter.mDatas.Count > needCount1)
                            {
                                continue;
                            }
                            if (gotoTeam1 == null)
                            {
                                gotoTeam1 = mCharacter;
                            }
                            else if (mCharacter.mDatas.Count == gotoTeam1.mDatas.Count)
                            {
                                nofullscene.FollowCharacter(gotoTeam1, 0);
                                nofullscene.FollowCharacter(mCharacter, 1);
                                return;
                            }
                        }
                    }
                }
                else
                {
                    //少的一边先进
                    var diff = Math.Abs(needCount1 - needCount2);
                    foreach (var mCharacter in _this.mCharacters)
                    {
                        if (diff >= mCharacter.mDatas.Count)
                        {
                            if (needCount1 > needCount2)
                            {
                                nofullscene.FollowCharacter(mCharacter, 0);
                                return;
                            }
                            nofullscene.FollowCharacter(mCharacter, 1);
                            return;
                        }
                    }
                }
            }


            needCount1 = needCount;
            needCount2 = needCount;
            _this.tempTeam1.Clear();
            _this.tempTeam2.Clear();
            var isCanOver1 = false;
            var isCanOver2 = false;
            foreach (var mCharacter in _this.mCharacters)
            {
                if (mCharacter.mDatas.Count <= needCount1)
                {
                    _this.tempTeam1.Add(mCharacter);
                    needCount1 -= mCharacter.mDatas.Count;
                    if (needCount1 == 0)
                    {
                        isCanOver1 = true;
                        if (isCanOver2)
                        {
                            break;
                        }
                    }
                }
                else if (mCharacter.mDatas.Count <= needCount2)
                {
                    _this.tempTeam2.Add(mCharacter);
                    needCount2 -= mCharacter.mDatas.Count;
                    if (needCount2 == 0)
                    {
                        isCanOver2 = true;
                        if (isCanOver1)
                        {
                            break;
                        }
                    }
                }
            }
            if (isCanOver1 && isCanOver2)
            {
                _this.MatchOver();
            }

            // 如果已经很长时间没有匹配到人了，把这些人降级
            {
                if (_this.NextQueue != null)
                {
                    List<QueueCharacter> removes = new List<QueueCharacter>();
                    foreach (var character in _this.mCharacters)
                    {
                        if ((DateTime.Now - character.StartTime).Seconds > _this.tbQueue.WaitTime)
                        {
                            removes.Add(character);
                            //_this.Pop内会执行mCharacters.Remove(character)，导致移除
                            //character.mLogic = _this.NextQueue;
                            //character.StartTime = DateTime.Now;
                            //_this.Pop(character, eLeaveMatchingType.MoveDown);
                            //_this.NextQueue.PushBack(character);
                        }
                    }
                    foreach (var queueCharacter in removes)
                    {
                        queueCharacter.mLogic = _this.NextQueue;
                        queueCharacter.StartTime = DateTime.Now;
                        _this.Pop(queueCharacter, eLeaveMatchingType.MoveDown);
                        _this.NextQueue.PushBack(queueCharacter);
                    }
                }
            }
        }

        public void MatchOver(FightQueue _this)
        {
            //检查一次排队是否有问题
            var guids = new List<ulong>();
            var isHave = new Dictionary<ulong, int>();
            foreach (var character in _this.tempTeam1)
            {
                foreach (var data in character.mDatas)
                {
                    isHave.modifyValue(data.Id, 1);
                    guids.Add(data.Id);
                }
            }

            foreach (var character in _this.tempTeam2)
            {
                foreach (var data in character.mDatas)
                {
                    isHave.modifyValue(data.Id, 1);
                    guids.Add(data.Id);
                }
            }
            PlayerLog.WriteLog((int) LogType.QueueLog, "MatchOver characterids={0}", guids.GetDataString());
            if (isHave.Count != _this.tbQueue.CountLimit*2)
            {
                PlayerLog.WriteLog((int) LogType.QueueLog, "MatchOver CheckId Count={0}", isHave.Count);
                foreach (var i in isHave)
                {
                    PlayerLog.WriteLog((int) LogType.QueueLog, "cId={0},cValue={1}", i.Key, i.Value);
                    QueueManager.Pop(i.Key, eLeaveMatchingType.TeamChange);
                }
            }
            //执行完毕的结果
            foreach (var character in _this.tempTeam1)
            {
                _this.PushSuccessTime((int) DateTime.Now.GetDiffSeconds(character.StartTime));
                _this.mCharacters.Remove(character);
#if DEBUG
                PlayerLog.WriteLog((int) LogType.QueueMessage, "MatchOver mCharacters.Remove c={0}, ids={1}",
                    character.Guid, character.mDatas.Select(d => d.Id).GetDataString());
#endif
            }
            foreach (var character in _this.tempTeam2)
            {
                _this.PushSuccessTime((int) DateTime.Now.GetDiffSeconds(character.StartTime));
                _this.mCharacters.Remove(character);
#if DEBUG
                PlayerLog.WriteLog((int) LogType.QueueMessage, "MatchOver mCharacters.Remove c={0}, ids={1}",
                    character.Guid, character.mDatas.Select(d => d.Id).GetDataString());
#endif
            }
            var result = new QueueResult(_this);
            result.PushCharacter(_this.tempTeam1);
            result.PushCharacter(_this.tempTeam2);
            result.StartTrigger();
            //results.Add(result);
            _this.tempTeam1 = new List<QueueCharacter>();
            _this.tempTeam2 = new List<QueueCharacter>();
        }

        public void OnAllOk(FightQueue _this, QueueResultBase result)
        {
            // 战场开始，所有人的战场计数+1
            var changes = new Dict_int_int_Data();
            changes.Data.Add((int) eExdataDefine.e530, 1);
            foreach (var list in result.mTeamList)
            {
                foreach (var character in list)
                {
                    foreach (var data in character.mDatas)
                    {
                        Utility.SSChangeExdata(data.Id, changes);
                    }
                }
            }
        }
    }

    public class FightQueue : QueueLogic
    {
        private static IFightQueue mImpl;

        static FightQueue()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (FightQueue), typeof (FightQueueDefaultImpl),
                o => { mImpl = (IFightQueue) o; });
        }

        public FightQueue(int queueId)
            : base(queueId)
        {
            mImpl.Construct(this, queueId);
        }

        public List<QueueCharacter> tempTeam1 = new List<QueueCharacter>();
        public List<QueueCharacter> tempTeam2 = new List<QueueCharacter>();
        public FightQueue NextQueue;

        public override void MatchOver()
        {
            mImpl.MatchOver(this);
        }

        public override void OnAllOk(QueueResultBase result)
        {
            mImpl.OnAllOk(this, result);
        }

        public override void Tick()
        {
            mImpl.Tick(this);
        }
    }
}