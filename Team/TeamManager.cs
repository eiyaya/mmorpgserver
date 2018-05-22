#region using

using System;
using System.Collections;
using System.Collections.Generic;
using DataContract;
using DataTable;
using Scorpion;
using NLog;
using Shared;
using TeamServerService;

#endregion

namespace Team
{
    public enum TeamState
    {
        None = -1,
        Leader = 0,
        Member = 1,
        Leaver = 2
    }

    public interface ITeam
    {
        void AddInvite(Team _this, ulong characterId);
        void CancelFubenResult(Team _this);
        bool CheckDisband(Team _this, bool isSendMessage = true);
        void Construct(Team _this, ulong teamId);
        void Disband(Team _this);
        void EnterAutoDisband(Team _this, int second);
        int GetTeamCount(Team _this);
        bool IsHaveCharacter(Team _this, ulong characterId);
        bool LeaveAutoDisband(Team _this);
        void MergeSceneByTeam(Team _this);
        void NotifyQueueMessage(ulong id, int queueId, List<SceneSimpleData> characters, List<ulong> results);
        void OnLeave(Team _this, ulong uId, TeamState state);
        void OnLost(Team _this, ulong uId, TeamState state);
        void PushLog(Team _this);
        void RemoveInvite(Team _this, ulong characterId);
        void TeamEnterFuben(Team _this, int fubenId, int serverId);
        void TeamEnterFubenResult(Team _this, ulong id, int fubenId, int result);
        void TeamEnterSyncMessage(Team _this, ulong uId);
        IEnumerator TeamEnterRefuseResult(Coroutine co, ulong id,ulong otherId, int type, string name);
     
    }

    public class TeamDefaultImpl : ITeam
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //一起进入副本
        private void EnterFuben(Team _this, int fubenId)
        {
            if (_this.AutoFubenResult != null)
            {
                TeamServerControl.tm.DeleteTrigger(_this.AutoFubenResult);
                _this.AutoFubenResult = null;
            }
            if (_this.TeamList.Count > 0)
            {
                var tbFuben = Table.GetFuben(fubenId);
                if (tbFuben != null)
                {
                    if (tbFuben.FubenCountNode == (int) eDungeonSettlementNode.Start)
                    {
                        foreach (var id in _this.TeamList)
                        {
                            Utility.NotifyEnterFuben(id, fubenId);
                        }
                    }
                    var ids = new List<ulong>();
                    ids.AddRange(_this.TeamList);
                    CoroutineFactory.NewCoroutine(Utility.AskEnterDungeonByTeamCoroutine, ids, _this.mServerId,
                        Table.GetFuben(fubenId), (ulong) 0).MoveNext();
                }
            }
            _this.mFubenResult.Clear();
            _this.mFubenId = -1;
            _this.mServerId = -1;
        }

        private IEnumerator MergeSceneByTeamCoroutine(Coroutine co, Team _this)
        {
            var ids = new IdList();
            ids.Ids.AddRange(_this.TeamList);
            var msgChgScene = TeamServer.Instance.SceneAgent.MergeSceneByTeam(_this.TeamList[0], ids);
            yield return msgChgScene.SendAndWaitUntilDone(co);
        }

        private void SendLeaveMsg(Team _this, ulong characterId)
        {
            CoroutineFactory.NewCoroutine(SendLeaveMsgCoroutine, _this, characterId).MoveNext();
        }

        private IEnumerator SendLeaveMsgCoroutine(Coroutine co, Team _this, ulong characterId)
        {
            var name = AsyncReturnValue<string>.Create();
            var job = AsyncReturnValue<int>.Create();
            var level = AsyncReturnValue<int>.Create();
            var co1 = CoroutineFactory.NewSubroutine(Utility.GetCharacterNameCoroutine, co, characterId, name, job, level);
            if (co1.MoveNext())
            {
                yield return co1;
            }
            var args = name.Value;
            name.Dispose();
            job.Dispose();
            level.Dispose();

            if (String.IsNullOrEmpty(args))
            {
                yield break;
            }

            name.Dispose();
            foreach (var id in _this.TeamList)
            {
                if (id != characterId)
                {
                    TeamServer.Instance.ServerControl.TeamServerMessage(id, (int) eLeaveMatchingType.LeaderLeave, args);
                }
            }
        }

        public void Construct(Team _this, ulong teamId)
        {
            _this.TeamId = teamId;
        }

        //队伍是否有某人
        public bool IsHaveCharacter(Team _this, ulong characterId)
        {
            foreach (var id in _this.TeamList)
            {
                if (id == characterId)
                {
                    return true;
                }
            }
            return false;
        }

        //获得队伍人数
        public int GetTeamCount(Team _this)
        {
            return _this.TeamList.Count;
        }

        //解散
        public void Disband(Team _this)
        {
            // 清除組隊目標數據
            for (int r = 0; r < _this.TeamList.Count; r++)
            {
                var chaId = _this.TeamList[r];
                TeamCharacterProxy Proxy;
                if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(chaId, out Proxy))
                    if (null != Proxy)
                        Proxy.NotifyChangetTeamTarget(0, 0, 0, 0, 0);
            }

            if (_this.AutoDisband != null)
            {
                TeamServerControl.tm.DeleteTrigger(_this.AutoDisband);
            }
            foreach (var uid in _this.TeamList)
            {
                TeamManager.RemoveCharacter(uid);
            }
            TeamManager.RemoveTeam(_this.TeamId);
            //--todo 发送网络包通知其他队员
        }

        //进入自动解散状态
        public void EnterAutoDisband(Team _this, int second)
        {
            _this.InviteList.Clear();
            _this.AutoDisband = TeamServerControl.tm.CreateTrigger(DateTime.Now.AddSeconds(second), _this.Disband);
        }

        //删除自动解散状态
        public bool LeaveAutoDisband(Team _this)
        {
            if (_this.AutoDisband != null)
            {
                TeamServerControl.tm.DeleteTrigger(_this.AutoDisband);
                _this.AutoDisband = null;
                return true;
            }
            return false;
        }

        //增加邀请者
        public void AddInvite(Team _this, ulong characterId)
        {
            Trigger InviteTrigger;
            if (_this.InviteList.TryGetValue(characterId, out InviteTrigger))
            {
                TeamServerControl.tm.ChangeTime(ref InviteTrigger, DateTime.Now.AddSeconds(32));
                return;
            }
            InviteTrigger = TeamServerControl.tm.CreateTrigger(DateTime.Now.AddSeconds(32),
                () => { _this.RemoveInvite(characterId); });
            _this.InviteList[characterId] = InviteTrigger;
        }

        //去除邀请者
        public void RemoveInvite(Team _this, ulong characterId)
        {
            _this.InviteList.Remove(characterId);
            CheckDisband(_this, false);
        }

        //检查是否需要解散
        public bool CheckDisband(Team _this, bool isSendMessage = true)
        {
//            if (GetTeamCount(_this) == 1 && _this.InviteList.Count == 0)
//            {
////如果队伍只剩下一个人
//                Disband(_this);
//                if (isSendMessage)
//                {
//                    TeamManager.SendMessage(_this.TeamList[0], 7, _this.TeamId, 0);
//                }
//                return true;
//            }
            return false;
        }

        //通知大家进入副本
        public void TeamEnterFuben(Team _this, int fubenId, int serverId)
        {
            _this.mFubenId = fubenId;
            _this.mServerId = serverId;
            _this.mFubenResult.Clear();
            _this.mFubenResult.Add(_this.TeamList[0]);
            var isOk = true;
            foreach (var uId in _this.TeamList)
            {
                if (_this.TeamList[0] != uId)
                {
                    isOk = false;
                    TeamEnterSyncMessage(_this, uId);
                }
            }
            if (isOk)
            {
                EnterFuben(_this, fubenId);
            }
            else
            {
                if (_this.AutoFubenResult != null)
                {
                    TeamServerControl.tm.DeleteTrigger(_this.AutoFubenResult);
                }
                _this.AutoFubenResult =
                    TeamServerControl.tm.CreateTrigger(DateTime.Now.AddSeconds(StaticParam.ConfirmDungeonWaitTime),
                        _this.CancelFubenResult);
            }
        }

        //给某人发包通知进入副本
        public void TeamEnterSyncMessage(Team _this, ulong uId)
        {
            if (_this.mFubenId < 0)
            {
                return;
            }
            TeamCharacterProxy toCharacterProxy;
            if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(uId, out toCharacterProxy))
            {
                var ChattoCharacterProxy = toCharacterProxy as TeamProxy;
                ChattoCharacterProxy.SyncTeamEnterFuben(_this.mFubenId);
            }
            else
            {
                Logger.Warn("TeamEnterFuben not find Id={0}", uId);
            }
        }

        //进入副本的消息返回
        public void TeamEnterFubenResult(Team _this, ulong id, int fubenId, int result)
        {
            if (fubenId != _this.mFubenId || _this.mFubenId < 0)
            {
                return;
            }
            if (!_this.TeamList.Contains(id))
            {
                return;
            }
            if (!_this.mFubenResult.Contains(id))
            {
                if (result == 0)
                {
                    foreach (var uId in _this.TeamList)
                    {
                        if (uId != id)
                        {
                            TeamServer.Instance.ServerControl.NotifyQueueResult(uId, id, 0);
                            CoroutineFactory.NewCoroutine(TeamEnterRefuseResult,uId,id,(int)eLeaveMatchingType.TeamRefuse, "").MoveNext ();
                        }
                    }
                    CancelFubenResult(_this);
                    return;
                }
                _this.mFubenResult.Add(id);
                foreach (var uId in _this.TeamList)
                {
                    if (uId != id)
                    {
                        TeamServer.Instance.ServerControl.NotifyQueueResult(uId, id, 1);
                    }
                    else
                    {
                        NotifyQueueMessage(uId, -1, _this.TeamDatas, _this.mFubenResult);
                    }
                }
            }
            var isOk = true;
            foreach (var uId in _this.TeamList)
            {
                if (!_this.mFubenResult.Contains(uId))
                {
                    isOk = false;
                }
            }
            if (isOk)
            {
                //进入副本
                EnterFuben(_this, _this.mFubenId);
            }
        }

        //自动取消副本进入
        public void CancelFubenResult(Team _this)
        {
            _this.mFubenResult.Clear();
            _this.mFubenId = -1;
            if (_this.AutoFubenResult != null)
            {
                TeamServerControl.tm.DeleteTrigger(_this.AutoFubenResult);
                _this.AutoFubenResult = null;
            }
        }

        //这些人变成了一队
        public void MergeSceneByTeam(Team _this)
        {
            CoroutineFactory.NewCoroutine(MergeSceneByTeamCoroutine, _this).MoveNext();
        }

        public void PushLog(Team _this)
        {
            Logger.Info("Team Id={0} TeamListCount={1},InviteListCount={2},ApplyListCount={3}", _this.TeamId,
                _this.TeamList.Count, _this.InviteList.Count, _this.ApplyList.Count);
        }

        public void OnLost(Team _this, ulong uId, TeamState state)
        {
            if (_this.mFubenId == -1)
            {
                return;
            }
            if (state == TeamState.Leader)
            {
                foreach (var id in _this.TeamList)
                {
                    if (id != uId)
                    {
                        TeamServer.Instance.ServerControl.TeamServerMessage(id, (int) eLeaveMatchingType.LeaderLost,
                            string.Empty);
                    }
                }
                _this.CancelFubenResult();
            }
        }

        public void OnLeave(Team _this, ulong uId, TeamState state)
        {
            if (_this.mFubenId == -1)
            {
                return;
            }
            if (state == TeamState.Leader)
            {
                _this.CancelFubenResult();
                SendLeaveMsg(_this, uId);
            }
        }

        public void NotifyQueueMessage(ulong id, int queueId, List<SceneSimpleData> characters, List<ulong> results)
        {
            var tcm = new TeamCharacterMessage();
            tcm.QueueId = queueId;

            foreach (var character in characters)
            {
                var tco = new TeamCharacterOne
                {
                    CharacterId = character.Id,
                    CharacterName = character.Name,
                    RoleId = character.TypeId,
                    Level = character.Level,
                    Ladder = character.Ladder,
                    FightPoint = character.FightPoint
                };
                tco.QueueResult = results.Contains(tco.CharacterId) ? 1 : -1;
                tcm.Characters.Add(tco);
            }

            TeamCharacterProxy toCharacterProxy;
            if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(id, out toCharacterProxy))
            {
                var ChattoCharacterProxy = toCharacterProxy as TeamProxy;
                ChattoCharacterProxy.NotifyQueueMessage(tcm);
            }
        }

        public IEnumerator TeamEnterRefuseResult(Coroutine co, ulong id,ulong otherId, int type, string name)
        {
            var dbSceneSimple = TeamServer.Instance.SceneAgent.GetSceneSimpleData(otherId, 0);
            yield return dbSceneSimple.SendAndWaitUntilDone(co);
            if (dbSceneSimple.State == MessageState.Reply)
            {
                if (dbSceneSimple.ErrorCode == (int)ErrorCodes.OK)
                {
                    TeamServer.Instance.ServerControl.TeamServerMessage(id, (int)eLeaveMatchingType.TeamRefuse,dbSceneSimple.Response.Name);
                }
            }
        }
    }

    public class Team
    {
        private static ITeam mImpl;

        static Team()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof(Team), typeof(TeamDefaultImpl),
                o => { mImpl = (ITeam)o; });
        }

        public Team(ulong teamId)
        {
            mImpl.Construct(this, teamId);
        }

        public List<ulong> ApplyList = new List<ulong>(); //申请列表
        public Trigger AutoDisband;
        public Trigger AutoFubenResult;
        public Dictionary<ulong, Trigger> InviteList = new Dictionary<ulong, Trigger>(); //邀请列表
        public int mFubenId = -1;
        public List<ulong> mFubenResult = new List<ulong>();
        public int mServerId = -1;
        public List<SceneSimpleData> TeamDatas = new List<SceneSimpleData>(); //队伍成员SceneSimpleData
        public List<ulong> TeamList = new List<ulong>(); //队伍成员列表
        public Dictionary<ulong,bool> MemberState = new Dictionary<ulong, bool> ();
        public ulong TeamId { get; set; }

        public TeamTargetType type { get; set; }
        public int teamTargetID { get; set; }
        public int levelMini { get; set; }
        public int levelMax { get; set; }
        //增加邀请者
        public void AddInvite(ulong characterId)
        {
            mImpl.AddInvite(this, characterId);
        }

        //自动取消副本进入
        public void CancelFubenResult()
        {
            mImpl.CancelFubenResult(this);
        }

        //检查是否需要解散
        public bool CheckDisband(bool isSendMessage = true)
        {
            return mImpl.CheckDisband(this, isSendMessage);
        }

        //解散
        public void Disband()
        {
            mImpl.Disband(this);
        }

        //进入自动解散状态
        public void EnterAutoDisband(int second)
        {
            mImpl.EnterAutoDisband(this, second);
        }

        public static ITeam GetImpl()
        {
            return mImpl;
        }

        //获得队伍人数
        public int GetTeamCount()
        {
            return mImpl.GetTeamCount(this);
        }

        //队伍是否有某人
        public bool IsHaveCharacter(ulong characterId)
        {
            return mImpl.IsHaveCharacter(this, characterId);
        }

        //删除自动解散状态
        public bool LeaveAutoDisband()
        {
            return mImpl.LeaveAutoDisband(this);
        }

        //这些人变成了一队
        public void MergeSceneByTeam()
        {
            mImpl.MergeSceneByTeam(this);
        }

        public void OnLeave(ulong uId, TeamState state)
        {
            mImpl.OnLeave(this, uId, state);
        }

        public void OnLost(ulong uId, TeamState state)
        {
            mImpl.OnLost(this, uId, state);
        }

        public void PushLog()
        {
            mImpl.PushLog(this);
        }

        //去除邀请者
        public void RemoveInvite(ulong characterId)
        {
            mImpl.RemoveInvite(this, characterId);
        }

        //通知大家进入副本
        public void TeamEnterFuben(int fubenId, int serverId)
        {
            mImpl.TeamEnterFuben(this, fubenId, serverId);
        }

        //进入副本的消息返回
        public void TeamEnterFubenResult(ulong id, int fubenId, int result)
        {
            mImpl.TeamEnterFubenResult(this, id, fubenId, result);
        }

        //给某人发包通知进入副本
        public void TeamEnterSyncMessage(ulong uId)
        {
            mImpl.TeamEnterSyncMessage(this, uId);
        }
    }

    public interface ICharacter
    {
        void Construct(Character _this, ulong id, TeamState state, Team team);
        void GetName(Character _this);
    }

    public class CharacterDefaultImpl : ICharacter
    {
        private IEnumerator GetNameCoroutine(Coroutine co, Character _this)
        {
            var ret = AsyncReturnValue<string>.Create();
            var retjob = AsyncReturnValue<int>.Create();
            var retLevel = AsyncReturnValue<int>.Create();
            var co1 = CoroutineFactory.NewSubroutine(Utility.GetCharacterNameCoroutine, co, _this.CharacterId, ret, retjob, retLevel);
            if (co1.MoveNext())
            {
                yield return co1;
            }
            _this.Name = ret.Value;
            ret.Dispose();
            retjob.Dispose();
            retLevel.Dispose();
        }

        public void Construct(Character _this, ulong id, TeamState state, Team team)
        {
            _this.CharacterId = id;
            _this.TeamState = state;
            _this.team = team;
            GetName(_this);
        }

        public void GetName(Character _this)
        {
            CoroutineFactory.NewCoroutine(GetNameCoroutine, _this).MoveNext();
        }
    }

    public class Character
    {
        private static ICharacter mImpl;

        static Character()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (Character), typeof (CharacterDefaultImpl),
                o => { mImpl = (ICharacter) o; });
        }

        public Character(ulong id, TeamState state, Team team)
        {
            mImpl.Construct(this, id, state, team);
        }

        public void GetName()
        {
            mImpl.GetName(this);
        }

        #region 数据

        public ulong CharacterId;
        public string Name;
        public TeamState TeamState;
        public Team team;

        #endregion
    }

    public interface ITeamManager
    {
        ErrorCodes AcceptJoin(ulong characterId, ulong teamId, ulong toCharacterId);
        ErrorCodes AcceptRequest(ulong characterId, ulong teamId);
        ErrorCodes ApplyJoin(ulong characterId, ulong toCharacterId);
        Team CreateTeam(ulong characterId);
        ErrorCodes CreateTeamEx(ulong characterId,ref ulong teamId);

        ErrorCodes Disband(ulong characterId, ulong teamId);
        Character GetCharacterTeam(ulong characterId);
        ulong GetNextTeamId();
        ErrorCodes Kick(ulong characterId, ulong teamId, ulong characterId2);
        ErrorCodes Leave(ulong characterId, ulong teamId);
        void OnLine(ulong characterId);
        void OnLost(ulong characterId);
        void PushLog();
        ErrorCodes RefuseInvite(ulong teamId, ulong selfId);
        ErrorCodes RefuseJoin(ulong leaderId, ulong toId);
        void RemoveCharacter(ulong characterId);
        void RemoveTeam(ulong teamId);
        ErrorCodes Request(ulong characterId, ref ulong teamId, ulong toCharacterId);
        ErrorCodes SendMessage(ulong toCharacterId, int type, ulong teamid, ulong characterId);
        ErrorCodes SwapLeader(ulong oldLeaderId, ulong newLeaderId, bool isLeave = false);
        void addOneToTeam(Team theTeam, ulong characterId, TeamState ts);
        TeamSearchItemList GetTeamsList();
        IEnumerator NotifyTeamSceneGuid(Coroutine co, ulong characterId);
        ErrorCodes ClearApplyList(ulong characterId);
        IEnumerator TeamApplyListSync(Coroutine co, ulong characterId);
        string GetCharacterName(ulong characterId);
        void OnCharacterLevelUp(ulong characterId,ulong TeamId,int reborn,int level) ;

        void OnCharacterChangeName(ulong characterId, ulong TeamId, string chagneName);
    }

    public class TeamManagerDefaultImpl : ITeamManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        [Updateable("TeamManager")]
        private static readonly int MaxTeamCount = 5;

        private void AddCharacter(Team theTeam, ulong characterId, TeamState ts)
        {
            theTeam.TeamList.Add(characterId);
            theTeam.TeamEnterSyncMessage(characterId);
            TeamManager.mCharacters[characterId] = new Character(characterId, ts, theTeam);
            if (ts == TeamState.Leader)
            {
                CoroutineFactory.NewCoroutine(SceneTeamMessageCoroutine, characterId, 0, theTeam.TeamId, 0).MoveNext();
            }
            else
            {
                CoroutineFactory.NewCoroutine(SceneTeamMessageCoroutine, characterId, 1, theTeam.TeamId, 1).MoveNext();
            }
        }

        //获得某个玩家的队伍状态
        private TeamState GetCharacterTeamState(ulong characterId)
        {
            Character character;
            if (TeamManager.mCharacters.TryGetValue(characterId, out character))
            {
                return character.TeamState;
            }
            return TeamState.None;
        }

        //获得队伍
        private Team GetTeam(ulong teamId)
        {
            Team team;
            if (TeamManager.mTeams.TryGetValue(teamId, out team))
            {
                return team;
            }
            return null;
        }

        //队员希望邀请某人(消息发送给toCharacterId )
        private IEnumerator MemberToLeaderCoroutine(Coroutine co,
                                                    int type,
                                                    ulong toCharacterId,
                                                    ulong characterId,
                                                    ulong characterId2)
        {
            var memJob = 0;
            var memLevel = 1;
            var name1 = "";
            var name2 = "";
            {
                var name = AsyncReturnValue<string>.Create();
                var job = AsyncReturnValue<int>.Create();
                var level = AsyncReturnValue<int>.Create();
                var result = CoroutineFactory.NewSubroutine(Utility.GetCharacterNameCoroutine, co, characterId, name, job, level);
                if (result.MoveNext())
                {
                    yield return result;
                }
                if (String.IsNullOrEmpty(name.Value))
                {
                    if(9 != type)yield break;
                }
                name1 = name.Value;
                memJob = job.Value;
                memLevel = level.Value;
                name.Dispose();
                job.Dispose();
                level.Dispose();
            }
            if (type == 2)
            {
                var character = TeamManager.GetCharacterTeam(characterId2);
                if (character != null && !string.IsNullOrWhiteSpace(character.Name))
                {
                    name2 = character.Name;
                }
                else
                {
                    var name = AsyncReturnValue<string>.Create();
                    var retJob = AsyncReturnValue<int>.Create();
                    var retLevel = AsyncReturnValue<int>.Create();
                    var result = CoroutineFactory.NewSubroutine(Utility.GetCharacterNameCoroutine, co, characterId2, name, retJob, retLevel);
                    if (result.MoveNext())
                    {
                        yield return result;
                    }
                    if (String.IsNullOrEmpty(name.Value))
                    {
                        yield break;
                    }
                    name2 = name.Value;
                    memJob = retJob.Value;
                    memLevel = retLevel.Value;
                    name.Dispose();
                    retJob.Dispose();
                    retLevel.Dispose();
                }
            }
            PlayerLog.WriteLog((int) LogType.TeamMessage,
                "SC->MemberToLeaderCoroutine toCharacterId={0}, type={1}, name1={2}, characterId2={3},name2={4}",
                toCharacterId, type, name1, characterId2, name2);
            TeamCharacterProxy toCharacterProxy;
            if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(toCharacterId, out toCharacterProxy))
            {
                var ChattoCharacterProxy = toCharacterProxy as TeamProxy;
                switch (type)
                {
                    case 1:
                    case 2:
                        ChattoCharacterProxy.MemberWantInvite(type, name1, memJob, memLevel, name2, characterId2);
                        break;
                    default:
                        ChattoCharacterProxy.MemberWantInvite(type, type == 9 ? TeamManager.GetCharacterName(characterId): name1, memJob, memLevel, name2, characterId);
                        break;
                }
            }
        }

        //带名字的消息
        private ErrorCodes MemberToLeaderMessage(ulong toCharacterId, int type, ulong characterId, ulong characterId2)
        {
            CoroutineFactory.NewCoroutine(MemberToLeaderCoroutine, type, toCharacterId, characterId, characterId2)
                .MoveNext();
            return ErrorCodes.OK;
        }

        //添加一个队伍成员
        //         public void AddCharacter(Team theTeam, ulong characterId,TeamState ts)
        //         {
        //             theTeam.TeamList.Add(characterId);
        //             TeamManager.mCharacters[characterId] = new Character()
        //             {
        //                 CharacterId = characterId,
        //                 TeamState = ts,
        //                 team = theTeam
        //             };
        //             var msg = ChatServer.Instance.SceneAgent.SceneTeamMessage(characterId, characterId, 1, theTeam.TeamId, 0);
        //         }

        //组队信息同步到Scene
        private IEnumerator SceneTeamMessageCoroutine(Coroutine co, ulong characterId, int type, ulong teamId, int state)
        {
            var msg = TeamServer.Instance.SceneAgent.SceneTeamMessage(characterId, characterId, type, teamId, state);
            yield return msg.SendAndWaitUntilDone(co);

            if (TeamManager.mCharacters.ContainsKey(characterId))
            {
                TeamCharacterProxy proxy;
                for (int i = 0; i < TeamManager.mCharacters[characterId].team.TeamList.Count; i++)
                {
                    var teamType = TeamManager.mCharacters[characterId].team.type;
                    var targetId = TeamManager.mCharacters[characterId].team.teamTargetID;
                    var levelMini = TeamManager.mCharacters[characterId].team.levelMini;
                    var levelMax = TeamManager.mCharacters[characterId].team.levelMax;
                    var temCharacId = TeamManager.mCharacters[characterId].team.TeamList[i];
                    if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(temCharacId, out proxy))
                    {
                        if (null != proxy)
                        {
                            if (characterId == proxy.CharacterId)
                                proxy.NotifyChangetTeamTarget((int)teamType, targetId, levelMini, levelMax, 0);
                        }
                        
                    }
                }
            }
        }

        //队伍发生变化
        private void TeamChange(TeamChangedType type, Team theTeam, ulong characterId)
        {
            if (type == TeamChangedType.Kick)
            {
                // 清除組隊目標數據
                TeamCharacterProxy Proxy;
                if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(characterId, out Proxy))
                    if (null != Proxy)
                        Proxy.NotifyChangetTeamTarget(0, 0, 0, 0, 0);
            }

            if (type == TeamChangedType.AcceptRequest || type == TeamChangedType.AcceptJoin)
            {
                theTeam.MergeSceneByTeam();
            }
            QueueManager.DealWithTeamChange(type, theTeam.TeamList, characterId);
            CoroutineFactory.NewCoroutine(TeamManager.NotifyTeamSceneGuid,characterId).MoveNext ();
            if (null != theTeam && theTeam.TeamList.Count == 5)
            {
                TeamCharacterProxy proxy;
                if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(theTeam.TeamList[0], out proxy))
                {
                    proxy.AutoMatchStateChange(0);
                }
            }
        }

        //创建队伍
        public Team CreateTeam(ulong characterId)
        {
            if (GetCharacterTeamState(characterId) != TeamState.None)
            {
                return null;
            }
            var TeamId = GetNextTeamId();
            var newTeam = new Team(TeamId);
            newTeam.type = TeamTargetType.NOTTARGET;
            TeamManager.mTeams.Add(TeamId, newTeam);
            AddCharacter(newTeam, characterId, TeamState.Leader);
            return newTeam;
        }

        public ErrorCodes CreateTeamEx(ulong characterId,ref ulong teamId)
        {
            var toCharacter = GetCharacterTeam(characterId);
            if (toCharacter != null)
            {
                Logger.Info("CreateTeamEx Error!!!----toCharacterId is have Team---- toCharacterId ={0},teamId={1}",
                    characterId, toCharacter.team.TeamId);
                return ErrorCodes.Error_CharacterHaveTeam;
            }

            var team = CreateTeam(characterId);
            teamId = team.TeamId;
            TeamChange(TeamChangedType.Request, team, characterId);
            return ErrorCodes.OK;
        }
        //邀请组队
        public ErrorCodes Request(ulong characterId, ref ulong teamId, ulong toCharacterId)
        {
            if (toCharacterId == characterId)
            {
                Logger.Error(
                    "Team Request Error!!!----toCharacterId = characterId---- toCharacterId ={0},characterId={1}",
                    toCharacterId, characterId);
                return ErrorCodes.Error_TeamNotSame;
            }
            var toCharacter = GetCharacterTeam(toCharacterId);
            if (toCharacter != null)
            {
                Logger.Info("Team Request Error!!!----toCharacterId is have Team---- toCharacterId ={0},teamId={1}",
                    toCharacterId, toCharacter.team.TeamId);
                return ErrorCodes.Error_CharacterHaveTeam;
            }

            var characterMember = GetCharacterTeam(characterId);
            Team team;
            if (characterMember == null)
            {
                team = CreateTeam(characterId);
                teamId = team.TeamId;
                characterMember = GetCharacterTeam(characterId);
                TeamChange(TeamChangedType.Request, team, characterId);
            }
            else
            {
                team = characterMember.team;
            }
            if (team.GetTeamCount() >= MaxTeamCount)
            {
                return ErrorCodes.Error_TeamIsFull;
            }
            if (characterMember == null)
            {
                Logger.Error(
                    "Team Request Error!!!----TeamManager.mCharacters not find character---- characterId ={0},teamId={1}",
                    characterId, teamId);
                return ErrorCodes.Error_CharacterHaveTeam;
            }

            if (characterMember.TeamState == TeamState.Leader)
            {
                team.AddInvite(toCharacterId); //添加到邀请列表
                return MemberToLeaderMessage(toCharacterId, 1, characterId, team.TeamId);
            }
            if (characterMember.TeamState == TeamState.Member)
            {
                var result = MemberToLeaderMessage(team.TeamList[0], 2, characterId, toCharacterId);
                if (result != ErrorCodes.OK)
                {
                    return result;
                }
                return ErrorCodes.Error_AlreadyToLeader;
            }
            Logger.Error("Team Request Error!!!----character not Online---- characterId ={0},teamId={1}", characterId,
                teamId);
            return ErrorCodes.Error_CharacterOutLine;
        }

        //拒绝邀请
        public ErrorCodes RefuseInvite(ulong teamId, ulong selfId)
        {
            var theTeam = GetTeam(teamId);
            if (theTeam == null)
            {
                Logger.Info("Team RefuseInvite Error!!!----not find Team---- ,teamId={0}", teamId);
                return ErrorCodes.Error_TeamNotFind;
            }
            Trigger trigger;
            if (!theTeam.InviteList.TryGetValue(selfId, out trigger))
            {
                return ErrorCodes.Error_CharacterNotInvite;
            }
            TeamServerControl.tm.DeleteTrigger(trigger);
            //theTeam.InviteList.Remove(selfId);
            MemberToLeaderMessage(theTeam.TeamList[0], 11, selfId, selfId);
            theTeam.RemoveInvite(selfId);
            return ErrorCodes.OK;
        }

        //拒绝申请
        public ErrorCodes RefuseJoin(ulong leaderId, ulong toId)
        {
            var characterMember = GetCharacterTeam(leaderId);
            if (characterMember == null)
            {
                return ErrorCodes.Error_CharacterNotTeam;
            }
            if (characterMember.TeamState != TeamState.Leader)
            {
                return ErrorCodes.Error_CharacterNotLeader;
            }
            if (characterMember.team != null)
            {
                if (characterMember.team.ApplyList.Contains(toId))
                {
                    characterMember.team.ApplyList.Remove(toId);
                    SendMessage(toId, 12, characterMember.team.TeamId, 0);
                    CoroutineFactory.NewCoroutine(TeamManager.TeamApplyListSync, leaderId).MoveNext();
                    return ErrorCodes.OK;
                }
            }
            return ErrorCodes.Unknow;
        }

        //同意邀请组队
        public ErrorCodes AcceptRequest(ulong characterId, ulong teamId)
        {
            var theTeam = GetTeam(teamId);
            if (theTeam == null)
            {
                Logger.Info("Team AcceptRequest Error!!!----not find Team---- characterId ={0},teamId={1}", characterId,
                    teamId);
                return ErrorCodes.Error_TeamNotFind;
            }
            if (theTeam.GetTeamCount() >= MaxTeamCount)
            {
                Logger.Info("Team AcceptRequest Error!!!----Team is Full---- characterId ={0},teamId={1}", characterId,
                    teamId);
                return ErrorCodes.Error_TeamIsFull;
            }
            var characterMember = GetCharacterTeam(characterId);
            if (characterMember != null)
            {
                Logger.Warn("Team AcceptRequest Error!!!----character is have Team---- characterId ={0},teamId={1}",
                    characterId, teamId);
                return ErrorCodes.Error_CharacterHaveTeam;
            }
            Trigger InviteTrigger;
            if (!theTeam.InviteList.TryGetValue(characterId, out InviteTrigger))
            {
                Logger.Info("Team AcceptRequest Error!!!----character is not invited---- characterId ={0},teamId={1}",
                    characterId, teamId);
                return ErrorCodes.Error_CharacterNotInvite;
            }
            TeamServerControl.tm.DeleteTrigger(InviteTrigger);
            foreach (var uId in theTeam.TeamList)
            {
                MemberToLeaderMessage(uId, 3, characterId, theTeam.TeamId);
            }
            //增加到队伍
            TeamManager.mCharacters[characterId] = new Character(characterId, TeamState.Member, theTeam);
            AddCharacter(theTeam, characterId, TeamState.Member);
            TeamChange(TeamChangedType.AcceptRequest, theTeam, characterId);
            theTeam.RemoveInvite(characterId);
            return ErrorCodes.OK;
        }

        //申请加入
        public ErrorCodes ApplyJoin(ulong characterId, ulong toCharacterId)
        {
            var characterMember = GetCharacterTeam(characterId);
            if (characterMember != null)
            {
                Logger.Info("Team ApplyJoin Error!!!----character is have Team---- characterId ={0},Leader={1}",
                    characterId, toCharacterId);
                return ErrorCodes.Error_CharacterHaveTeam;
            }
            var TeamLeader = GetCharacterTeam(toCharacterId);
            if (TeamLeader == null)
            {
                Logger.Info("Team ApplyJoin Error!!!----TeamLeader is not Team---- characterId ={0},Leader={1}",
                    characterId, toCharacterId);
                return ErrorCodes.Error_CharacterNotTeam;
            }
            var theTeam = TeamLeader.team;
            if (theTeam == null)
            {
                Logger.Error("Team ApplyJoin Error!!!----not find Team---- characterId ={0},Leader={1}", characterId,
                    toCharacterId);
                return ErrorCodes.Error_TeamNotFind;
            }
            if (theTeam.GetTeamCount() >= MaxTeamCount)
            {
                Logger.Info("Team ApplyJoin Error!!!----Team is Full---- characterId ={0},Leader={1}", characterId,
                    toCharacterId);
                return ErrorCodes.Error_TeamIsFull;
            }
            if(!theTeam.ApplyList.Contains (characterId))theTeam.ApplyList.Add(characterId);
            if (theTeam.ApplyList.Count > 15) theTeam.ApplyList.RemoveAt(0);
            CoroutineFactory.NewCoroutine(TeamManager.TeamApplyListSync, toCharacterId).MoveNext();

            var Leader = GetCharacterTeam(theTeam.TeamList[0]);
            if (Leader == null)
            {
                return ErrorCodes.Unknow;
            }
            if (Leader.TeamState == TeamState.Leaver)
            {
                return ErrorCodes.Unline;
            }
            MemberToLeaderMessage(theTeam.TeamList[0], 4, characterId, characterId);
            return ErrorCodes.OK;
        }

        //同意加入申请
        public ErrorCodes AcceptJoin(ulong characterId, ulong teamId, ulong toCharacterId)
        {
            var characterLeader = GetCharacterTeam(characterId);
            if (characterLeader == null)
            {
                Logger.Warn("Team AcceptJoin Error!!!----character not find---- characterId ={0},teamId={1}",
                    characterId, teamId);
                return ErrorCodes.Error_CharacterNotTeam;
            }
            var theTeam = characterLeader.team;
            if (theTeam == null)
            {
                Logger.Error("Team AcceptJoin Error!!!----not find Team---- characterId ={0},teamId={1}", characterId,
                    teamId);
                return ErrorCodes.Error_TeamNotFind;
            }
            if (theTeam.GetTeamCount() >= MaxTeamCount)
            {
                Logger.Info("Team AcceptJoin Error!!!----Team is Full---- characterId ={0},teamId={1}", characterId,
                    teamId);
                return ErrorCodes.Error_TeamIsFull;
            }
            var characterMember = GetCharacterTeam(toCharacterId);
            if (characterMember != null)
            {
                Logger.Warn(
                    "Team AcceptJoin Error!!!----character is have Team---- characterId ={0},teamId={1},otherTeam={2}",
                    toCharacterId, teamId, characterMember.team.TeamId);
                return ErrorCodes.Error_OtherHasTeam;
            }
            if (characterLeader.TeamState != TeamState.Leader)
            {
                Logger.Warn("Team AcceptJoin Error!!!----character not leader---- characterId ={0},teamId={1}",
                    characterId, teamId);
                return ErrorCodes.Error_CharacterNotLeader;
            }
            if (!theTeam.ApplyList.Remove(toCharacterId))
            {
                return ErrorCodes.Unknow;
            }
            CoroutineFactory.NewCoroutine(TeamManager.TeamApplyListSync, characterId).MoveNext();
            //todo 广播给其他队友
            foreach (var uId in theTeam.TeamList)
            {
                //SendMessage(uId, 3, theTeam.TeamId, characterId, 0);
                MemberToLeaderMessage(uId, 3, toCharacterId, theTeam.TeamId);
            }
            TeamManager.mCharacters[toCharacterId] = new Character(toCharacterId, TeamState.Member, theTeam);
            AddCharacter(theTeam, toCharacterId, TeamState.Member);
            TeamChange(TeamChangedType.AcceptJoin, theTeam, toCharacterId); //队伍变化事件
            SendMessage(toCharacterId, 13, theTeam.TeamId, characterId);
            
            return ErrorCodes.OK;
        }

        //退出队伍
        public ErrorCodes Leave(ulong characterId, ulong teamId)
        {
            var team = GetTeam(teamId);
            if (team == null)
            {
                Logger.Warn("Team Leave Error!!!----not find Team---- characterId ={0},teamId={1}", characterId, teamId);
                return ErrorCodes.Error_TeamNotFind;
            }
            if (!team.IsHaveCharacter(characterId))
            {
                return ErrorCodes.Error_CharacterNotTeam;
            }

            #region 
            {//队伍解散 自动匹配标记清除
                if (AutoMatchManager.teamMatchDic.ContainsKey(characterId))
                {
                    AutoMatchManager.teamMatchDic.Remove(characterId);
                    TeamCharacterProxy toCharacterProxy;
                    if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(characterId, out toCharacterProxy))
                    {
                        Logger.Info("TeamWorkRefrerrence 队伍解散 teamMatchDic");
                        toCharacterProxy.AutoMatchStateChange(0);
                    }
                }

                if (AutoMatchManager.nullTeamMatchDic.ContainsKey(characterId))
                {
                    AutoMatchManager.nullTeamMatchDic.Remove(characterId);
                    TeamCharacterProxy toCharacterProxy;
                    if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(characterId, out toCharacterProxy))
                    {
                        Logger.Info("TeamWorkRefrerrence 队伍解散 nullTeamMatchDic");
                        toCharacterProxy.AutoMatchStateChange(0);
                    }
                }

                // 清除組隊目標數據
                for (int r = 0; r < team.TeamList.Count; r++)
                {
                    var chaId = team.TeamList[r];
                    if(chaId != characterId)continue;
                    TeamCharacterProxy Proxy;
                    if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(chaId, out Proxy))
                        if(null != Proxy)
                            Proxy.NotifyChangetTeamTarget(0, 0, 0, 0, 0);
                }
            }
            #endregion
            TeamChange(TeamChangedType.Leave, team, characterId);
            var Index = -1;
            var LeaderId = team.TeamList[0];
            ulong newLeaderId = LeaderId == characterId &&team.TeamList.Count > 1 ? team.TeamList[1] : 0;
            if (newLeaderId > 0)
            {
                SwapLeader(LeaderId, newLeaderId,true);
                LeaderId = newLeaderId;
            }

            if (LeaderId == characterId)
            {
                team.OnLeave(characterId, TeamState.Leader);
                var isFinish = false;
                foreach (var uId in team.TeamList)
                {
                    Index++;
                    if (Index == 0)
                    {
                        continue;
                    }
                    var characterMember = GetCharacterTeam(uId);
                    if (characterMember == null)
                    {
                        Logger.Error(
                            "Team Leave Error!!!----characterMember not Team---- characterMember ={0},teamId={1}",
                            characterMember.CharacterId, teamId);
                        continue;
                    }
                    if (characterMember.TeamState == TeamState.Member)
                    {
                        isFinish = true;
                        team.TeamList.Remove(uId);
                        team.CancelFubenResult();
                        RemoveCharacter(characterId);
                        team.TeamList[0] = uId; //设置新队长
                        characterMember.TeamState = TeamState.Leader;
                        foreach (var uId2 in team.TeamList)
                        {
                            MemberToLeaderMessage(uId2, 5, characterId, 0);
                        }
                        if (team.CheckDisband())
                        {
                            return ErrorCodes.OK;
                        }
                        foreach (var uId2 in team.TeamList)
                        {
                            if (uId2 == team.TeamList[0])
                            {
                                SendMessage(uId2, 6, team.TeamId, uId2);
                            }
                            else
                            {
                                MemberToLeaderMessage(uId2, 14, team.TeamList[0], 0);
                            }
                        }
                        break;
                    }
                }
                if (!isFinish)
                {
                    team.TeamList.Remove(characterId);
                    team.CancelFubenResult();
                    RemoveCharacter(characterId);
                    team.EnterAutoDisband(30);
                    CoroutineFactory.NewCoroutine(TeamManager.TeamApplyListSync, characterId).MoveNext();
                }
            }
            else
            {
//一般成员离队
                team.TeamList.Remove(characterId);
                team.CancelFubenResult();
                RemoveCharacter(characterId);
                if (team.CheckDisband())
                {
                    MemberToLeaderMessage(team.TeamList[0], 5, characterId, 0);
                    return ErrorCodes.OK;
                }
                foreach (var uId2 in team.TeamList)
                {
                    MemberToLeaderMessage(uId2, 5, characterId, 0);
                }
            }
            return ErrorCodes.OK;
        }

        //切换队长
        public ErrorCodes SwapLeader(ulong oldLeaderId, ulong newLeaderId, bool isLeave = false)
        {
            var character = GetCharacterTeam(newLeaderId);
            if (character == null)
            {
                Logger.Warn("Team SwapLeader Error!!!----characterId not Team---- newLeaderId ={0}", newLeaderId);
                return ErrorCodes.Error_CharacterNotTeam;
            }
            if (character.TeamState == TeamState.Leaver)
            {
                return ErrorCodes.Unline;
            }
            var Oldcharacter = GetCharacterTeam(oldLeaderId);
            if (Oldcharacter == null)
            {
                Logger.Warn("Team SwapLeader Error!!!----characterId not Team---- oldLeaderId ={0}", oldLeaderId);
                return ErrorCodes.Error_CharacterNotTeam;
            }
            if (character.team != Oldcharacter.team)
            {
                Logger.Warn(
                    "Team SwapLeader Error!!!----Character is not  Member---- characterId ={0},Team={1},TeamState={2}",
                    character.team.TeamId, character.TeamState);
                return ErrorCodes.Error_TeamNotSame;
            }
            if (character.team.TeamList[0] != oldLeaderId)
            {
                return ErrorCodes.Error_CharacterNotLeader;
            }

            if (AutoMatchManager.teamMatchDic.ContainsKey(oldLeaderId))
            {
                AutoMatchManager.teamMatchDic.Remove(oldLeaderId);
            }

            TeamCharacterProxy proxy;
            if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(oldLeaderId, out proxy))
            {
                proxy.AutoMatchStateChange(0);
            }
            CoroutineFactory.NewCoroutine(SceneTeamMessageCoroutine, oldLeaderId, 1, character.team.TeamId, 1)
                .MoveNext();
            CoroutineFactory.NewCoroutine(SceneTeamMessageCoroutine, newLeaderId, 1, character.team.TeamId, 0)
                .MoveNext();
            var Index = -1;
            foreach (var uId in character.team.TeamList)
            {
                Index++;
                if (uId == newLeaderId)
                {
                    character.team.TeamList[Index] = oldLeaderId;
                    character.team.TeamList[0] = newLeaderId;
                    Oldcharacter.TeamState = TeamState.Member;
                    character.TeamState = TeamState.Leader;
                    foreach (var uId2 in character.team.TeamList)
                    {
                        if (uId2 == newLeaderId)
                        {
                            SendMessage(uId2, 6, character.team.TeamId, newLeaderId);
                            CoroutineFactory.NewCoroutine(TeamManager.TeamApplyListSync, newLeaderId).MoveNext();
                        }
                        else
                        {
                            if (!(isLeave && oldLeaderId == uId2))
                            {
                                MemberToLeaderMessage(uId2, 14, newLeaderId, 0);
                                if (uId2 == oldLeaderId) CoroutineFactory.NewCoroutine(TeamManager.TeamApplyListSync, oldLeaderId).MoveNext();
                            }
                        }
                    }
                    
                    return ErrorCodes.OK;
                }
            }
            Logger.Error("Team SwapLeader Error!!!----Not find Character in Team---- characterId ={0},OldTeam={1}",
                newLeaderId, character.team.TeamId);
            return ErrorCodes.Unknow;
        }

        //解散队伍
        public ErrorCodes Disband(ulong characterId, ulong teamId)
        {
            var character = GetCharacterTeam(characterId);
            if (character == null)
            {
                Logger.Error("Team Disband Error!!!----characterId not Team---- characterId ={0},teamId={1}",
                    characterId, teamId);
                return ErrorCodes.Error_CharacterNotTeam;
            }
            if (character.team.TeamId != teamId)
            {
                Logger.Error("Team Disband Error!!!----DbTeam != NowTeam---- characterId ={0},OldTeam={1},NowTeam={2}",
                    characterId, teamId, character.team.TeamId);
                return ErrorCodes.Error_TeamNotSame;
            }
            if (character.TeamState != TeamState.Leader)
            {
                Logger.Error("Team Disband Error!!!----characterId not Leader---- characterId ={0},TeamId={1}",
                    characterId, teamId);
                return ErrorCodes.Error_CharacterNotLeader;
            }
            CoroutineFactory.NewCoroutine(TeamManager.TeamApplyListSync, characterId).MoveNext();
            foreach (var uId in character.team.TeamList)
            {
                SendMessage(uId, 7, character.team.TeamId, 0);
            }
            TeamChange(TeamChangedType.Disband, character.team, characterId);
            character.team.Disband();
            return ErrorCodes.OK;
        }

        //踢出队伍
        public ErrorCodes Kick(ulong characterId, ulong teamId, ulong characterId2)
        {
            if (characterId == characterId2)
            {
                return ErrorCodes.Unknow;
            }
            var character = GetCharacterTeam(characterId);
            if (character == null)
            {
                Logger.Warn("Team Disband Error!!!----characterId not Team---- characterId ={0},teamId={1}", characterId,
                    teamId);
                return ErrorCodes.Error_CharacterNotTeam;
            }
            if (character.team.TeamId != teamId)
            {
                Logger.Error("Team Disband Error!!!----DbTeam != NowTeam---- characterId ={0},OldTeam={1},NowTeam={2}",
                    characterId, teamId, character.team.TeamId);
                return ErrorCodes.Error_TeamNotSame;
            }
            if (character.TeamState != TeamState.Leader)
            {
                Logger.Error("Team Disband Error!!!----characterId not Leader---- characterId ={0},TeamId={1}",
                    characterId, teamId);
                return ErrorCodes.Error_CharacterNotLeader;
            }
            if (!character.team.IsHaveCharacter(characterId2))
            {
                Logger.Warn("Team Disband Error!!!----characterId2 not Team---- characterId ={0},TeamId={1}",
                    characterId, teamId, characterId2);
                return ErrorCodes.Error_CharacterNotTeam;
            }
            SendMessage(characterId2, 10, character.team.TeamId, characterId2);
            character.team.TeamList.Remove(characterId2);
            character.team.CancelFubenResult();
            RemoveCharacter(characterId2);
            TeamChange(TeamChangedType.Kick, character.team, characterId2);
            //if (character.team.TeamList.Count == 1 && character.team.InviteList.Count == 0)
            //{
            //    if (null != character.team.TeamList)
            //    {
            //        if (AutoMatchManager.teamMatchDic.ContainsKey(character.team.TeamList[0]))
            //        {
            //            AutoMatchManager.teamMatchDic.Remove(character.team.TeamList[0]);
            //        }

            //        if (AutoMatchManager.nullTeamMatchDic.ContainsKey(character.team.TeamList[0]))
            //        {
            //            AutoMatchManager.nullTeamMatchDic.Remove(character.team.TeamList[0]);
            //        }
            //        TeamCharacterProxy proxy;
            //        if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(character.team.TeamList[0], out proxy))
            //        {
            //            if(null != proxy)proxy.AutoMatchStateChange(0);
            //        }
            //    }
            //    SendMessage(character.team.TeamList[0], 7, character.team.TeamId, 0);
            //    character.team.Disband();
            //    return ErrorCodes.OK;
            //}
            foreach (var uId in character.team.TeamList)
            {
                if (characterId != uId)
                {
                    MemberToLeaderMessage(uId, 5, characterId2, 0);
                }
            }

            return ErrorCodes.OK;
        }

        //离线通知
        public void OnLost(ulong characterId)
        {
            var character = GetCharacterTeam(characterId);
            if (character == null)
            {
                return;
            }
            if (character.team == null)
            {
                return;
            }
            if (character.team.TeamList == null)
            {
                return;
            }
            //{
            //    if (character.team.MemberState.ContainsKey(characterId))
            //        character.team.MemberState[characterId] = false;
            //    Logger.Error("TeamManagerMemberState  Id=" + characterId + " state=" + false);
            //}
            foreach (var uId in character.team.TeamList)
            {
                if (characterId != uId)
                {
                    MemberToLeaderMessage(uId, 8, characterId, 0);
                }
            }
            if (character.TeamState == TeamState.Leader)
            {
                character.team.OnLost(characterId, character.TeamState);
                var Index = -1;
                foreach (var uId in character.team.TeamList)
                {
                    Index++;
                    if (uId == characterId)
                    {
                        continue;
                    }
                    var characterMember = GetCharacterTeam(uId);
                    if (characterMember == null)
                    {
                        Logger.Error(
                            "Team OnLost Error!!!----characterMember not Team---- characterMember ={0},teamId={1}",
                            characterMember.CharacterId, character.team.TeamId);
                        continue;
                    }
                    if (characterMember.TeamState == TeamState.Member)
                    {
                        SwapLeader(characterId, characterMember.CharacterId);
                        character.TeamState = TeamState.Leaver;
                        return;
                    }
                }
                character.TeamState = TeamState.Leaver;
                character.team.EnterAutoDisband(30);
                return;
            }
            if (character.TeamState == TeamState.Member)
            {
                character.TeamState = TeamState.Leaver;
                return;
            }
            Logger.Error("Team OnLine Error!!!----character state error----characterId ={0},DbTeam={1},TeamState={2}",
                characterId, character.team.TeamId, character.TeamState);
        }

        //上线通知
        public void OnLine(ulong characterId)
        {
            var character = GetCharacterTeam(characterId);
            if (character == null)
            {
                //CoroutineFactory.NewCoroutine(TeamManager.SceneTeamMessageCoroutine, characterId, 2, (ulong)0, (ulong)0).MoveNext();
                CoroutineFactory.NewCoroutine(TeamManager.TeamApplyListSync, characterId).MoveNext();
                return;
            }
            if (character.TeamState != TeamState.Leaver)
            {
                Logger.Error("Team OnLine Error!!!----character state!=Leaver----characterId ={0},TeamState={1}",
                    characterId, character.TeamState);
                if (!character.team.IsHaveCharacter(characterId))
                {
                    Logger.Error("Team OnLine Error!!!----NowTeam  Not Have character----characterId ={0},NowTeam={1}",
                        characterId, character.team.TeamId);
                    return;
                }
            }
            if (character.team.LeaveAutoDisband())
            {
                character.TeamState = TeamState.Leader;
                var oldLeaderId = character.team.TeamList[0];
                if (characterId != oldLeaderId)
                {
                    var index = 0;
                    foreach (var uId in character.team.TeamList)
                    {
                        if (characterId == uId)
                        {
                            character.team.TeamList[0] = characterId;
                            character.team.TeamList[index] = oldLeaderId;
                            CoroutineFactory.NewCoroutine(TeamManager.TeamApplyListSync, characterId).MoveNext();
                            break;
                        }
                        index++;
                    }
                }
            }
            else
            {
                character.TeamState = TeamState.Member;
            }
            CoroutineFactory.NewCoroutine(TeamManager.TeamApplyListSync, characterId).MoveNext();
            foreach (var uId in character.team.TeamList)
            {
                if (characterId != uId)
                {
                    MemberToLeaderMessage(uId, 9, characterId, 0);
                }
            }

            //{
            //    if (character.team.MemberState.ContainsKey(characterId))
            //        character.team.MemberState[characterId] = true;
            //    Logger.Error("TeamManagerMemberState  Id=" + characterId + " state=" + true);
            //}
        }

        //获得某个玩家所在的队伍
        public Character GetCharacterTeam(ulong characterId)
        {
            Character character;
            if (TeamManager.mCharacters.TryGetValue(characterId, out character))
            {
                return character;
            }
            return null;
        }

        //获得下一个队伍ID
        public ulong GetNextTeamId()
        {
            return TeamManager.NextTeamId++;
        }

        //移除一个组队的玩家
        public void RemoveCharacter(ulong characterId)
        {
            TeamManager.mCharacters.Remove(characterId);

            CoroutineFactory.NewCoroutine(SceneTeamMessageCoroutine, characterId, 2, (ulong) 0, 0).MoveNext();
            //ChatServer.Instance.SceneAgent.SceneTeamMessage(characterId, characterId, 2, 0, 0);
        }

        //移除一个队伍
        public void RemoveTeam(ulong teamId)
        {
            TeamManager.mTeams.Remove(teamId);
        }

        //通知某人，某人某队伍在邀请他
        public ErrorCodes SendMessage(ulong toCharacterId, int type, ulong teamid, ulong characterId)
        {
            PlayerLog.WriteLog((int) LogType.TeamMessage,
                "SC->NotifyTeamMessage characterId={0}, type={1}, teamId={2}, characterId={3}", toCharacterId, type,
                teamid, characterId);
            TeamCharacterProxy toCharacterProxy;
            if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(toCharacterId, out toCharacterProxy))
            {
                var ChattoCharacterProxy = toCharacterProxy as TeamProxy;
                ChattoCharacterProxy.NotifyTeamMessage(type, teamid, characterId);
                return ErrorCodes.OK;
            }
            return ErrorCodes.Error_CharacterOutLine;
        }


        //通知队伍其他玩家信息不符合
        public ErrorCodes OnSyncTeamMemberInfo(ulong toCharacterId, string info)
        {
            PlayerLog.WriteLog((int) LogType.TeamMessage,
                "SC->NotifyTeamMessage characterId={0}, info={1}", toCharacterId, info);
            TeamCharacterProxy toCharacterProxy;
            if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(toCharacterId, out toCharacterProxy))
            {
                var ChattoCharacterProxy = toCharacterProxy as TeamProxy;
                return ErrorCodes.OK;
            }
            return ErrorCodes.Error_CharacterOutLine;
        }
        
        //输出Log
        public void PushLog()
        {
            Logger.Info("TeamManager characterCount={0}", TeamManager.mCharacters.Count);
            if (TeamManager.mCharacters.Count > 0)
            {
                Logger.Info("{");
                foreach (var character in TeamManager.mCharacters)
                {
                    Logger.Info("    character id={0},TeamState={1}", character.Key, character.Value.TeamState);
                }
                Logger.Info("}");
            }
            Logger.Info("TeamManager TeamCount={0}", TeamManager.mTeams.Count);
            if (TeamManager.mTeams.Count > 0)
            {
                Logger.Info("{");
                foreach (var team in TeamManager.mTeams)
                {
                    team.Value.PushLog();
                }
                Logger.Info("}");
            }
        }

        public void addOneToTeam(Team theTeam, ulong characterId, TeamState ts)
        {
            if (theTeam == null) return;
            var toCharacterId = theTeam.TeamList[0];
            if(!theTeam.TeamList.Contains(characterId))theTeam.TeamList.Add(characterId);
            theTeam.TeamEnterSyncMessage(characterId);
            TeamManager.mCharacters[characterId] = new Character(characterId, ts, theTeam);
            if (ts == TeamState.Leader)
            {
                CoroutineFactory.NewCoroutine(SceneTeamMessageCoroutine, characterId, 0, theTeam.TeamId, 0).MoveNext();
            }
            else
            {
                CoroutineFactory.NewCoroutine(SceneTeamMessageCoroutine, characterId, 1, theTeam.TeamId, 1).MoveNext();
            }

            foreach (var uId in theTeam.TeamList)
            {
                MemberToLeaderMessage(uId, 3, characterId, theTeam.TeamId);
            }
            TeamChange(TeamChangedType.AcceptRequest, theTeam, characterId);
        }

        public TeamSearchItemList GetTeamsList()
        {
            TeamSearchItemList list = new TeamSearchItemList();
            foreach (var item in TeamManager.mTeams)
            {
                if (null != item.Value && null != item.Value.TeamList && item.Value.TeamList.Count > 0)
                {
                    var characterId = item.Value.TeamList[0];
                    var characteData = TeamManager.GetCharacterTeam(characterId);

                    if (null != characteData)
                    {
                        TeamSearchItem ite = new TeamSearchItem();
                        ite.TargetId = item.Value.teamTargetID;
                        ite.TeamGroupType = (int)item.Value.type;
                        ite.TeamID = (int)item.Value.TeamId;
                        ite.LevelMax = item.Value.levelMax;
                        ite.LevelMini = item.Value.levelMini;
                        ite.Name = characteData.Name;
                        ite.CharacterId = (int)characterId;
                        ite.TeamID = (int)item.Value.TeamId;
                        list.SearchList.Add(ite);
                    }
                }
            }
            return list;
        }

        public IEnumerator NotifyTeamSceneGuid (Coroutine co, ulong characterId)
        {
            var sceneDataMsg = TeamServer.Instance.SceneAgent.SSGetCharacterSceneData(characterId, characterId);
            yield return sceneDataMsg.SendAndWaitUntilDone(co);
            if (sceneDataMsg.State != MessageState.Reply)
            {
                yield break;
            }
            if (sceneDataMsg.ErrorCode == (int)ErrorCodes.OK)
            {
                Logger.Info("TeamGetCharacterSceneData SceneGuid=" + sceneDataMsg.Response.SceneGuid + " charaId=" + characterId);
                if (!TeamManager.mCharacters.ContainsKey(characterId)) yield break;
                TeamCharacterProxy proxy;
                for (int i = 0; i < TeamManager.mCharacters[characterId].team.TeamList.Count; i++)
                {
                    var temCharacId = TeamManager.mCharacters[characterId].team.TeamList[i];
                    if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(temCharacId, out proxy))
                    {
                        if (null != proxy)
                            proxy.NotifyTeamScenGuid(temCharacId, characterId, sceneDataMsg.Response.SceneGuid);
                    }
                }

                for (int i = 0; i < TeamManager.mCharacters[characterId].team.TeamList.Count; i++)
                {
                    var temCharacId1 = TeamManager.mCharacters[characterId].team.TeamList[i];

                    var sceneDataMsgMem = TeamServer.Instance.SceneAgent.SSGetCharacterSceneData(temCharacId1, temCharacId1);
                    yield return sceneDataMsgMem.SendAndWaitUntilDone(co);
                    if (sceneDataMsgMem.State != MessageState.Reply)
                    {
                        yield break;
                    }
                    if (sceneDataMsgMem.ErrorCode == (int)ErrorCodes.OK)
                    {
                        Logger.Info("TeamGetCharacterSceneData SceneGuid=" + sceneDataMsgMem.Response.SceneGuid + " charaId=" + temCharacId1);
                        if (!TeamManager.mCharacters.ContainsKey(temCharacId1)) yield break;
                        TeamCharacterProxy proxy1;

                        if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(characterId, out proxy1))
                        {
                            if (null != proxy1)
                                proxy1.NotifyTeamScenGuid(characterId, temCharacId1, sceneDataMsgMem.Response.SceneGuid);
                        }
                    }
                }
            }
        }

        public ErrorCodes ClearApplyList(ulong characterId)
        {
            var characterMember = GetCharacterTeam(characterId);
            if (characterMember == null || null == characterMember.team)
            {
                return ErrorCodes.Error_ClearApplyListFail_001;
            }
            if (characterMember.team != null)
            {
                if (null != characterMember.team.ApplyList)
                {
                    characterMember.team.ApplyList.Clear();
                }
            }
            CoroutineFactory.NewCoroutine(TeamManager.TeamApplyListSync,characterId).MoveNext ();

            return ErrorCodes.OK;
        }

        public IEnumerator TeamApplyListSync(Coroutine co, ulong characterId)
        {
            TeamCharacterProxy proxy1;

            if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(characterId, out proxy1))
            {
                bool state = false;
                var character = GetCharacterTeam(characterId);
                if (null != character)
                {
                    if(null != character.team && null != character.team.TeamList && character.team.TeamList.Count > 0)
                        if(character.team.TeamList[0] == characterId)
                            state = character.team.ApplyList.Count > 0;
                }

                if (null != proxy1)
                    proxy1.TeamApplyListSync(characterId, state);
            }
            return null;
        }
        public string GetCharacterName(ulong characterId)
        {
            Character character = null;
            if (TeamManager.mCharacters.TryGetValue(characterId,out character) == true)
                return character.Name;
            return "";
        }
        public void OnCharacterLevelUp(ulong characterId, ulong TeamId, int reborn, int level)
        {
            var team = GetTeam(TeamId);
            if (team == null)
                return;
            var characterChange = (ulong)0;
            foreach (var _id in team.TeamList)
            {
                if (_id > 0)
                {
                    if (_id == characterId)
                    {
                        characterChange = _id;
                    }
                }
            }
            foreach (var _id in team.TeamList)
            {
                if (_id == characterId)
                {
                    continue;
                }
                TeamCharacterProxy Proxy;
                if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(_id, out Proxy))
                    if (null != Proxy)
                        Proxy.SCSyncTeamMemberLevelChange(_id, characterChange, reborn, level);
            }
        }

        public void OnCharacterChangeName(ulong characterId, ulong TeamId, string chagneName)
        {
             Character character = null;
            if (TeamManager.mCharacters.TryGetValue(characterId,out character)){
                character.Name = chagneName;
            }
            else
            {
                return;
            }

            var team = GetTeam(TeamId);
            if (team == null)
                return;
            foreach (var _id in team.TeamList)
            {
                TeamCharacterProxy Proxy;
                if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(_id, out Proxy))
                    if (null != Proxy)
                        Proxy.NodifyTeamMemberPlayerNameChange(_id, characterId, chagneName);
            }
        }
    }

    public static class TeamManager
    {
        public static Dictionary<ulong, Character> mCharacters = new Dictionary<ulong, Character>();
        private static ITeamManager mImpl;
        public static Dictionary<ulong, Team> mTeams = new Dictionary<ulong, Team>();
        public static ulong NextTeamId = 1;
        public static RequestManager WebRequestManager = null;

        static TeamManager()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (TeamManager), typeof (TeamManagerDefaultImpl),
                o => { mImpl = (ITeamManager) o; });
        }

        //同意加入申请
        public static ErrorCodes AcceptJoin(ulong characterId, ulong teamId, ulong toCharacterId)
        {
            return mImpl.AcceptJoin(characterId, teamId, toCharacterId);
        }

        //同意邀请组队
        public static ErrorCodes AcceptRequest(ulong characterId, ulong teamId)
        {
            return mImpl.AcceptRequest(characterId, teamId);
        }

        //申请加入
        public static ErrorCodes ApplyJoin(ulong characterId, ulong toCharacterId)
        {
            return mImpl.ApplyJoin(characterId, toCharacterId);
        }

        //创建队伍
        public static Team CreateTeam(ulong characterId)
        {
            return mImpl.CreateTeam(characterId);
        }

        public static ErrorCodes CreateTeamEx(ulong characterId,ref ulong teamId)
        {
            return mImpl.CreateTeamEx(characterId,ref teamId);
        }

        //解散队伍
        public static ErrorCodes Disband(ulong characterId, ulong teamId)
        {
            return mImpl.Disband(characterId, teamId);
        }

        //获得某个玩家所在的队伍
        public static Character GetCharacterTeam(ulong characterId)
        {
            return mImpl.GetCharacterTeam(characterId);
        }

        //获得下一个队伍ID
        public static ulong GetNextTeamId()
        {
            return mImpl.GetNextTeamId();
        }

        //踢出队伍
        public static ErrorCodes Kick(ulong characterId, ulong teamId, ulong characterId2)
        {
            return mImpl.Kick(characterId, teamId, characterId2);
        }

        //退出队伍
        public static ErrorCodes Leave(ulong characterId, ulong teamId)
        {
            return mImpl.Leave(characterId, teamId);
        }

        //上线通知
        public static void OnLine(ulong characterId)
        {
            mImpl.OnLine(characterId);
        }

        //离线通知
        public static void OnLost(ulong characterId)
        {
            mImpl.OnLost(characterId);
        }

        //输出Log
        public static void PushLog()
        {
            mImpl.PushLog();
        }

        //拒绝邀请
        public static ErrorCodes RefuseInvite(ulong teamId, ulong selfId)
        {
            return mImpl.RefuseInvite(teamId, selfId);
        }

        //拒绝申请
        public static ErrorCodes RefuseJoin(ulong leaderId, ulong toId)
        {
            return mImpl.RefuseJoin(leaderId, toId);
        }

        public static ErrorCodes ClearApplyList(ulong characterId)
        {
            return mImpl.ClearApplyList(characterId);
        }

        //移除一个组队的玩家
        public static void RemoveCharacter(ulong characterId)
        {
            mImpl.RemoveCharacter(characterId);
        }

        //移除一个队伍
        public static void RemoveTeam(ulong teamId)
        {
            mImpl.RemoveTeam(teamId);
        }

        //邀请组队
        public static ErrorCodes Request(ulong characterId, ref ulong teamId, ulong toCharacterId)
        {
            return mImpl.Request(characterId, ref teamId, toCharacterId);
        }

        //通知某人，某人某队伍在邀请他
        public static ErrorCodes SendMessage(ulong toCharacterId, int type, ulong teamid, ulong characterId)
        {
            return mImpl.SendMessage(toCharacterId, type, teamid, characterId);
        }

        //切换队长
        public static ErrorCodes SwapLeader(ulong oldLeaderId, ulong newLeaderId)
        {
            return mImpl.SwapLeader(oldLeaderId, newLeaderId);
        }

        public static void addOneToTeam(Team theTeam, ulong characterId, TeamState ts)
        {
            mImpl.addOneToTeam(theTeam,characterId,ts);
        }

        public static TeamSearchItemList GetTeamsList()
        {
            return mImpl.GetTeamsList();
        }

        public static IEnumerator NotifyTeamSceneGuid(Coroutine co, ulong characterId)
        {
            return mImpl.NotifyTeamSceneGuid(co, characterId);
        }

        public static IEnumerator TeamApplyListSync(Coroutine co, ulong characterId)
        {
            return mImpl.TeamApplyListSync(co, characterId);
        }
        public static string GetCharacterName(ulong characterId)
        {
            return mImpl.GetCharacterName(characterId);
        }
        public static void OnCharacterLevelUp(ulong characterId,ulong TeamId,int reborn,int level)
        {
            mImpl.OnCharacterLevelUp(characterId,TeamId,reborn, level);
        }
        public static void OnCharacterChangeName(ulong characterId, ulong TeamId, string changeName)
        {
            mImpl.OnCharacterChangeName(characterId, TeamId,  changeName);
        }
    }
}