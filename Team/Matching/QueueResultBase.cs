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
using TeamServerService;

#endregion

namespace Team
{
    public interface IQueueResultBase
    {
        void Construct(QueueResultBase _this, QueueLogic q);
        void EnterFuben(QueueResultBase _this);
        ErrorCodes MatchingBack(QueueResultBase _this, ulong guid, bool Agree = false);
        void PushCharacter(QueueResultBase _this, List<QueueCharacter> ListQueue);
        //压到队伍前列
        void PushFront(QueueResultBase _this, eLeaveMatchingType type = eLeaveMatchingType.Unknow);
        string PushLog(QueueResultBase _this);
        void PushOneCharacter(QueueResultBase _this, QueueCharacter queue);
        void RemoveCharacterList(QueueResultBase _this, List<QueueCharacter> cs);
        void RemoveCharacterOne(QueueResultBase _this, QueueCharacter character);
        void StartTrigger(QueueResultBase _this);
        void StopTrigger(QueueResultBase _this);
        void TimeOver(QueueResultBase _this);
    }

    public class QueueResultBaseDefaultImpl : IQueueResultBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private void FaildList(QueueResultBase _this, List<ulong> guids)
        {
            PlayerLog.WriteLog((int) LogType.QueueMessage, "FaildList  character={0}, type={1}", guids.GetDataString(),
                _this.GetType());
            foreach (var i in guids)
            {
                _this.CharacterState.Remove(i);
            }
            QueueManager.PopTimeOver(guids);
        }

        private void FaildOne(QueueResultBase _this, ulong guid)
        {
            StopTrigger(_this);
            QueueManager.Pop(guid, eLeaveMatchingType.Refuse);
        }

        //通知玩家修改PvP的Camp
        private IEnumerator NotifyCharacterSceneCamp(Coroutine co, QueueResultBase _this, ulong character, int camp)
        {
            var msgChgScene = TeamServer.Instance.SceneAgent.SSPvPSceneCampSet(character, camp);
            yield return msgChgScene.SendAndWaitUntilDone(co);
        }

        //通知排队信息
        public void NotifyQueueMessage(ulong guid, TeamCharacterMessage tcm)
        {
            TeamCharacterProxy toCharacterProxy;
            if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(guid, out toCharacterProxy))
            {
                toCharacterProxy.NotifyQueueMessage(tcm);
            }
        }

        //通知排队反馈信息
        private void NotifyQueueResult(ulong toGuid, ulong guid, int type)
        {
            TeamCharacterProxy toCharacterProxy;
            if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(toGuid, out toCharacterProxy))
            {
                toCharacterProxy.NotifyQueueResult(guid, type);
            }
        }

        private void RemoveCharacter(QueueResultBase _this, QueueCharacter character)
        {
#if DEBUG
            PlayerLog.WriteLog((int) LogType.QueueMessage, "RemoveCharacter c={0}, ids={1}", character.Guid,
                character.mDatas.Select(c => c.Id).GetDataString());
#endif
            foreach (var data in character.mDatas)
            {
                _this.CharacterState.Remove(data.Id);
            }
            var isRemove = false;
            foreach (var team in _this.mTeamList) //所有队伍 -》临时队伍
            {
                foreach (var c in team) //临时队伍-》单独排队
                {
                    if (character == c)
                    {
                        team.Remove(c);
                        c.result = null;
                        isRemove = true;
                        break;
                    }
                    if (c.mDatas[0].Id == character.mDatas[0].Id)
                    {
                        PlayerLog.WriteLog((int) LogType.QueueLog, "RemoveCharacter  Error not Same in={0},for={1}",
                            character.Guid, c.Guid);
                    }
                }
                if (isRemove)
                {
                    break;
                }
            }
            if (!isRemove)
            {
                PlayerLog.WriteLog((int) LogType.QueueLog, "RemoveCharacter  Error c={0},n={1}",
                    character.mDatas[0].Id, character.mDatas.Count);
            }
        }

        public void Construct(QueueResultBase _this, QueueLogic q)
        {
            _this.mQueue = q;
        }

        public string PushLog(QueueResultBase _this)
        {
            var nt = "[";
            foreach (var newTeam in _this.newTeams)
            {
                nt = nt + newTeam.TeamList.Count + "";
            }
            nt = nt + "]";
            var tl = "[";
            foreach (var list in _this.mTeamList)
            {
                tl = tl + list.Count + "{";
                foreach (var character in list)
                {
                    tl = tl + character.mDatas.Count + "(" + character.GetLog() + ")";
                }
                tl = tl + "};";
            }
            tl = tl + "]";

            var cccc = string.Format("c={0},t1={1},t2={2}", _this.CharacterState.Count, tl, nt);
            return cccc;
        }

        public void StartTrigger(QueueResultBase _this)
        {
            foreach (var i in _this.CharacterState)
            {
                TeamCharacterProxy toCharacterProxy;
                if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(i.Key, out toCharacterProxy))
                {
                    toCharacterProxy.MatchingSuccess(_this.mQueue.mQueueId);
                }
            }

            _this.mTrigger =
                TeamServerControl.tm.CreateTrigger(DateTime.Now.AddSeconds(StaticParam.ConfirmDungeonWaitTime),
                    () => _this.TimeOver());

            var tcm = new TeamCharacterMessage();
            tcm.QueueId = _this.mQueue.mQueueId;
            foreach (var list in _this.mTeamList)
            {
                foreach (var character in list)
                {
                    foreach (var data in character.mDatas)
                    {
                        var one = new TeamCharacterOne
                        {
                            CharacterId = data.Id,
                            CharacterName = data.Name,
                            RoleId = data.TypeId,
                            Level = data.Level,
                            Ladder = data.Ladder,
                            FightPoint = data.FightPoint
                        };
                        one.QueueResult = -1;
                        tcm.Characters.Add(one);
                    }
                }
            }
            foreach (var i in _this.CharacterState)
            {
                NotifyQueueMessage(i.Key, tcm);
            }
        }

        public void StopTrigger(QueueResultBase _this)
        {
            if (_this.mTrigger != null)
            {
                TeamServerControl.tm.DeleteTrigger(_this.mTrigger);
                _this.mTrigger = null;
            }
        }

        public void TimeOver(QueueResultBase _this)
        {
            _this.mTrigger = null;
            var guids = new List<ulong>();
            foreach (var i in _this.CharacterState)
            {
                if (i.Value == 0)
                {
                    guids.Add(i.Key);
                }
            }
            FaildList(_this, guids);
        }

        //排队结果，移除一堆人
        public void RemoveCharacterList(QueueResultBase _this, List<QueueCharacter> cs)
        {
            foreach (var character in cs)
            {
                RemoveCharacter(_this, character);
            }
            PushFront(_this, eLeaveMatchingType.OtherRefuse);
        }

        //排队结果，移除一个人
        public void RemoveCharacterOne(QueueResultBase _this, QueueCharacter character)
        {
            RemoveCharacter(_this, character);
        }

        //排队结果，添加一组人
        public void PushCharacter(QueueResultBase _this, List<QueueCharacter> ListQueue)
        {
            foreach (var queue in ListQueue)
            {
                queue.result = _this;
                foreach (var i in queue.mDatas)
                {
                    if (_this.CharacterState.ContainsKey(i.Id))
                    {
                        Logger.Error("result  PushCharacter Same C={0}", i.Id);
                        _this.CharacterState[i.Id] = 0;
                    }
                    else
                    {
                        _this.CharacterState.Add(i.Id, 0);
                    }
                }
            }
            _this.mTeamList.Add(ListQueue);
        }

        //排队结果，添加一个队伍的人，目前只有活动副本的预约要用这个函数
        public void PushOneCharacter(QueueResultBase _this, QueueCharacter queue)
        {
            queue.result = _this;
            foreach (var i in queue.mDatas)
            {
                if (_this.CharacterState.ContainsKey(i.Id))
                {
                    Logger.Error("result  PushCharacter Same C={0}", i.Id);
                    _this.CharacterState[i.Id] = 0;
                }
                else
                {
                    _this.CharacterState.Add(i.Id, 0);
                }
            }
            var teamList = new List<QueueCharacter>();
            teamList.Add(queue);
            _this.mTeamList.Add(teamList);
            _this.ServerId = queue.mDatas[0].ServerId;
        }

        //压到队伍前列
        public void PushFront(QueueResultBase _this, eLeaveMatchingType type = eLeaveMatchingType.Unknow)
        {
            foreach (var team in _this.mTeamList) //所有队伍 -》临时队伍
            {
                foreach (var character in team) //临时队伍-》单独排队
                {
                    foreach (var data in character.mDatas) //单独排队 -》 一个玩家
                    {
                        if (type != eLeaveMatchingType.Unknow)
                        {
                            TeamServer.Instance.ServerControl.TeamServerMessage(data.Id, (int) type, string.Empty);
                        }
                    }

                    _this.mQueue.PushFront(character);
                }
            }
        }

        //确认消息返回
        public ErrorCodes MatchingBack(QueueResultBase _this, ulong guid, bool Agree = false)
        {
            PlayerLog.WriteLog((int) LogType.QueueMessage, "MatchOver  character={0},Agree={1}", guid, Agree);
            if (_this.mTrigger == null)
            {
                return ErrorCodes.Unknow;
            }
            if (Agree) //点了同意
            {
                int state;
                if (_this.CharacterState.TryGetValue(guid, out state))
                {
                    if (state == 0)
                    {
                        _this.CharacterState[guid] = 1;
                        _this.okCount++;
                        if (_this.okCount == _this.CharacterState.Count)
                        {
                            StopTrigger(_this);
                            _this.AllOK();
                        }
                        else
                        {
                            foreach (var i in _this.CharacterState)
                            {
                                NotifyQueueResult(i.Key, guid, 1);
                            }
                        }
                    }
                }
            }
            else //点了失败
            {
                foreach (var i in _this.CharacterState)
                {
                    NotifyQueueResult(i.Key, guid, 0);
                }
                FaildOne(_this, guid);
            }
            return ErrorCodes.OK;
        }

        public void EnterFuben(QueueResultBase _this)
        {
            var tbQueue = _this.mQueue.tbQueue;
            var fubenId = tbQueue.Param;
            var tbFuben = Table.GetFuben(fubenId);
            if (tbFuben == null)
            {
                Logger.Error("Not Find FubenID! QueueId = {0} ", tbQueue.Id);
                return;
            }
            if (_this.CharacterState.Count < 1)
            {
                return;
            }
            var tbScene = Table.GetScene(tbFuben.SceneId);
            switch ((eQueueType) tbQueue.AppType)
            {
                case eQueueType.Dungeon:
                {
                    var characters = new List<ulong>();
                    foreach (var character in _this.CharacterState)
                    {
                        characters.Add(character.Key);
                    }
                    var serverId = tbScene.CanCrossServer == 1 ? -1 : _this.mTeamList[0][0].mDatas[0].ServerId;
                    CoroutineFactory.NewCoroutine(Utility.AskEnterDungeonByTeamCoroutine, characters, serverId, tbFuben,
                        (ulong) 0).MoveNext();
                    foreach (var matchingCharacter in _this.CharacterState)
                    {
                        QueueManager.Pop(matchingCharacter.Key, eLeaveMatchingType.Success);
                    }
                }
                    break;
                case eQueueType.BattleField:
                {
                    if (_this.mTeamList.Count%2 != 0)
                    {
                        Logger.Error("fightQueue mTeamList Count ={0}", _this.mTeamList.Count);
                    }
                    var index = 0;
                    var characters = new List<ulong>();
                    var tempScene = QueueSceneManager.CreateScene(tbQueue.Id);
                    foreach (var list in _this.mTeamList)
                    {
                        foreach (var character in list)
                        {
                            foreach (var data in character.mDatas)
                            {
                                characters.Add(data.Id);
                                CoroutineFactory.NewCoroutine(NotifyCharacterSceneCamp, _this, data.Id, index)
                                    .MoveNext();
                                tempScene.InitCharacter(data.Id, index);
                            }
                        }
                        if (index == 0)
                        {
                            index = 1;
                        }
                        else
                        {
                            index = 0;
                        }
                    }
                    var serverId = tbScene.CanCrossServer == 1 ? -1 : _this.mTeamList[0][0].mDatas[0].ServerId;
                    CoroutineFactory.NewCoroutine(Utility.AskEnterDungeonByTeamCoroutine, characters, serverId, tbFuben,
                        (ulong) 0).MoveNext();
                    foreach (var matchingCharacter in _this.CharacterState)
                    {
                        QueueManager.Pop(matchingCharacter.Key, eLeaveMatchingType.Success);
                    }
                }
                    break;
                case eQueueType.ActivityDungeon:
                {
                    var characters = new List<ulong>();
                    foreach (var character in _this.CharacterState)
                    {
                        characters.Add(character.Key);
                        if (tbFuben.FubenCountNode == (int) eDungeonSettlementNode.Start)
                        {
                            Utility.NotifyEnterFuben(character.Key, fubenId);
                        }
                    }
                    var serverId = tbScene.CanCrossServer == 1 ? -1 : _this.ServerId;
                    CoroutineFactory.NewCoroutine(Utility.AskEnterDungeonByTeamCoroutine, characters, serverId,
                        tbFuben, (ulong) 0).MoveNext();
                    foreach (var matchingCharacter in _this.CharacterState)
                    {
                        QueueManager.Pop(matchingCharacter.Key, eLeaveMatchingType.Success);
                    }
                }
                    break;
            }
        }
    }

    public abstract class QueueResultBase
    {
        private static IQueueResultBase mImpl;

        static QueueResultBase()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (QueueResultBase),
                typeof (QueueResultBaseDefaultImpl), o => { mImpl = (IQueueResultBase) o; });
        }

        public QueueResultBase(QueueLogic q)
        {
            mImpl.Construct(this, q);
        }

        public Dictionary<ulong, int> CharacterState = new Dictionary<ulong, int>();
            //玩家的返回状态 key=characterId ,value =0 还没返回 1 确认完毕

        public QueueLogic mQueue;
        public List<List<QueueCharacter>> mTeamList = new List<List<QueueCharacter>>(); //一组玩家的列表，未来要分到一组队
        public Trigger mTrigger;
        public List<QueueTeam> newTeams = new List<QueueTeam>();
        public int okCount;
        public int ServerId = -1;
        //排队结果，所有人正常同意
        public abstract void AllOK();

        public void EnterFuben()
        {
            mImpl.EnterFuben(this);
        }

        public static IQueueResultBase GetImpl()
        {
            return mImpl;
        }

        //确认消息返回
        public virtual ErrorCodes MatchingBack(ulong guid, bool agree = false)
        {
            return mImpl.MatchingBack(this, guid, agree);
        }

        //排队结果，添加一组人
        public void PushCharacter(List<QueueCharacter> listQueue)
        {
            mImpl.PushCharacter(this, listQueue);
        }

        //压到队伍前列
        public void PushFront(eLeaveMatchingType type = eLeaveMatchingType.Unknow)
        {
            mImpl.PushFront(this, type);
        }

        public string PushLog()
        {
            return mImpl.PushLog(this);
        }

        //排队结果，添加一个队伍的人，目前只有活动副本的预约要用这个函数
        public virtual void PushOneCharacter(QueueCharacter queue)
        {
            mImpl.PushOneCharacter(this, queue);
        }

        //排队结果，移除一堆人
        public virtual void RemoveCharacterList(List<QueueCharacter> cs)
        {
            mImpl.RemoveCharacterList(this, cs);
        }

        //排队结果，移除一个人
        public virtual void RemoveCharacterOne(QueueCharacter character)
        {
            mImpl.RemoveCharacterOne(this, character);
        }

        public void StartTrigger()
        {
            mImpl.StartTrigger(this);
        }

        public void StopTrigger()
        {
            mImpl.StopTrigger(this);
        }

        //排队结果，添加一个队伍的人，目前只有活动副本的预约要用这个函数
        public virtual void TimeOver()
        {
            mImpl.TimeOver(this);
        }
    }
}