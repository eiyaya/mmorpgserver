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
    public interface IQueueManager
    {
        ErrorCodes CheckPush(QueueRecord tbQuene, List<ulong> ids);
        void DealWithTeamChange(TeamChangedType type, List<ulong> teamList, ulong characterId);
        QueueLogic GetMatching(QueueRecord tbQueue);
        QueueCharacter GetMatchingCharacter(ulong guid);
        QueueCharacter GetQueueInfo(ulong guid);
        void Init();
        ErrorCodes MatchingBack(ulong guid, int result);
        void OnLine(ulong characterId);
        void OnLost(ulong characterId);
        void Pop(ulong guid, eLeaveMatchingType type);
        void PopTimeOver(List<ulong> guids);
        ErrorCodes Push(int queueId, List<CharacterSimpleData> simpleDatas, MatchingResult result);
        ErrorCodes PushFront(QueueCharacter character);
        void PushLog();
        void PushOneCharacter(ulong characterId, QueueCharacter character);
        void Remove(ulong guid, eLeaveMatchingType type);
    }

    public class QueueManagerDefaultImpl : IQueueManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //把一个新的characterId 加入到 character 里
        private IEnumerator PushOneCharacterCoroutine(Coroutine co, ulong characterId, QueueCharacter character)
        {
            if (character.result != null)
            {
                yield break;
            }
            PlayerLog.WriteLog((int) LogType.QueueMessage, "PushOnCharacter character={0}", characterId);

            //在线检查
            if (!TeamServer.Instance.ServerControl.Proxys.ContainsKey(characterId))
            {
                Logger.Error("PushOneCharacterCoroutine(), character {0} not online", characterId);
                yield break;
            }
            //检查进入条件
            if (QueueManager.Characters.ContainsKey(characterId))
            {
                Logger.Error("PushOneCharacterCoroutine(), character {0} already in queue", characterId);
                yield break;
            }
            //确定玩家的进入条件满足
            var tbQueue = character.mLogic.tbQueue;
            var logicResult = TeamServer.Instance.LogicAgent.CheckCharacterInFuben(characterId, tbQueue.Param);
            yield return logicResult.SendAndWaitUntilDone(co);
            if (logicResult.State != MessageState.Reply)
            {
                Logger.Error(
                    "PushOneCharacterCoroutine(), CheckCharacterInFuben not replied! character = {0},fubenid = {1}",
                    characterId, tbQueue.Id);
                yield break;
            }
            if (logicResult.ErrorCode != (int) ErrorCodes.OK)
            {
                Logger.Debug("PushOneCharacterCoroutine(), character {0} already in queue", characterId);
                yield break;
            }
            //从scene获取simpledata
            var dbSceneSimple = TeamServer.Instance.SceneAgent.GetSceneSimpleData(characterId, 0);
            yield return dbSceneSimple.SendAndWaitUntilDone(co);
            if (dbSceneSimple.State != MessageState.Reply)
            {
                Logger.Error("PushOnCharacter Error! GetSceneSimpleData not replied! Id = {0}", characterId);
                yield break;
            }
            if (dbSceneSimple.ErrorCode != (int) ErrorCodes.OK)
            {
                Logger.Error("PushOnCharacter Error! GetSceneSimpleData return with err = {0}", dbSceneSimple.ErrorCode);
                yield break;
            }
            var data = new CharacterSimpleData
            {
                Id = dbSceneSimple.Response.Id,
                TypeId = dbSceneSimple.Response.TypeId,
                Name = dbSceneSimple.Response.Name,
                SceneId = dbSceneSimple.Response.SceneId,
                FightPoint = dbSceneSimple.Response.FightPoint,
                Level = dbSceneSimple.Response.Level,
                Ladder = dbSceneSimple.Response.Ladder,
                ServerId = dbSceneSimple.Response.ServerId
            };
            if (character.result != null)
            {
                yield break;
            }
            character.mDatas.Add(data);
            SendTeamMemberMatch(data, character);
#if DEBUG
            PlayerLog.WriteLog((int) LogType.QueueMessage, "PushOneCharacterCoroutine().Matching Add c={0}, ids={1}",
                character.Guid, character.mDatas.Select(d => d.Id).GetDataString());
#endif
            QueueManager.Characters.Add(characterId, character);
        }

        //通知队伍进行了某个排队
        private void SendTeamMatch(QueueCharacter character)
        {
            foreach (var data in character.mDatas)
            {
                SendTeamMemberMatch(data, character);
            }
        }

        //通知某个队员，进行了某个排队
        private void SendTeamMemberMatch(CharacterSimpleData data, QueueCharacter character)
        {
            TeamCharacterProxy toCharacterProxy;
            if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(data.Id, out toCharacterProxy))
            {
                var info = new QueueInfo();
                info.QueueId = character.mLogic.mQueueId;
                info.NeedSeconds = character.mLogic.GetAverageTime();
                info.StartTime = character.StartTime.ToBinary();

                var ChattoCharacterProxy = toCharacterProxy as TeamProxy;
                ChattoCharacterProxy.NotifyMatchingData(info);
            }
        }

        //初始化
        public void Init()
        {
        }

        //获得某个类型的排队
        public QueueLogic GetMatching(QueueRecord tbQueue)
        {
            QueueLogic matching;
            if (!QueueManager.Matchings.TryGetValue(tbQueue.Id, out matching))
            {
                switch ((eQueueType) tbQueue.AppType)
                {
                    case eQueueType.Dungeon:
                        matching = new FubenQueue(tbQueue.Id);
                        break;
                    case eQueueType.BattleField:
                        matching = new FightQueue(tbQueue.Id);
                        break;
                    case eQueueType.ActivityDungeon:
                        matching = new ActivityFubenQueue(tbQueue.Id);
                        break;
                }
                QueueManager.Matchings.Add(tbQueue.Id, matching);
                QueueManager.RefreshMatchRelation();
            }
            return matching;
        }

        //获得某个玩家
        public QueueCharacter GetMatchingCharacter(ulong guid)
        {
            QueueCharacter character;
            if (QueueManager.Characters.TryGetValue(guid, out character))
            {
                return character;
            }
            return null;
        }

        public ErrorCodes CheckPush(QueueRecord tbQuene, List<ulong> ids)
        {
            if (ids.Count > tbQuene.CountLimit)
            {
                return ErrorCodes.Error_QueueCountMax;
            }
            foreach (var id in ids)
            {
                if (QueueManager.Characters.ContainsKey(id))
                {
                    Logger.Error("MatchingManager Push Error !characterId is Have ! Id={0}", id);
                    return ErrorCodes.Error_CharacterHaveQueue;
                }
            }
            return ErrorCodes.OK;
        }

        //把一个新的characterId 加入到 character 里
        public void PushOneCharacter(ulong characterId, QueueCharacter character)
        {
            CoroutineFactory.NewCoroutine(PushOneCharacterCoroutine, characterId, character).MoveNext();
        }

        //开始排队
        public ErrorCodes Push(int queueId, List<CharacterSimpleData> simpleDatas, MatchingResult result)
        {
            PlayerLog.WriteLog((int) LogType.QueueMessage, "Push character={0}", simpleDatas[0].Id);
            var tbQuene = Table.GetQueue(queueId);
            if (tbQuene == null)
            {
                return ErrorCodes.Error_FubenID;
            }
            if (tbQuene.CountLimit > 0 && simpleDatas.Count > tbQuene.CountLimit)
            {
                return ErrorCodes.Error_QueueCountMax;
            }
            var nowCharacter = GetMatchingCharacter(simpleDatas[0].Id);
            if (nowCharacter != null && nowCharacter.result != null)
            {
#if DEBUG
                PlayerLog.WriteLog((int) LogType.QueueMessage, "Push 1 id={0}, ids = {1}", nowCharacter.Guid,
                    nowCharacter.mDatas.Select(d => d.Id).GetDataString());
#endif
                nowCharacter.result.MatchingBack(simpleDatas[0].Id, false);
            }
            else
            {
                foreach (var characterSimpleData in simpleDatas)
                {
                    if (QueueManager.Characters.ContainsKey(characterSimpleData.Id))
                    {
#if DEBUG
                        PlayerLog.WriteLog((int) LogType.QueueMessage, "Push 2 id={0}", characterSimpleData.Id);
#endif
                        QueueManager.Pop(characterSimpleData.Id, eLeaveMatchingType.PushCannel);
                    }
                }
            }
            var matching = GetMatching(tbQuene);
            var character = new QueueCharacter(matching, simpleDatas);
            SendTeamMatch(character);
            foreach (var characterSimpleData in simpleDatas)
            {
#if DEBUG
                PlayerLog.WriteLog((int) LogType.QueueMessage, "Push().Matching Add character={0} c={1} ids={2}",
                    characterSimpleData.Id, character.Guid, character.mDatas.Select(d => d.Id).GetDataString());
#endif
                QueueManager.Characters.Add(characterSimpleData.Id, character);
            }
            matching.PushBack(character);
            return ErrorCodes.OK;
        }

        //新增至某队首
        public ErrorCodes PushFront(QueueCharacter character)
        {
            var tbQuene = Table.GetQueue(character.mLogic.mQueueId);
            if (tbQuene == null)
            {
                return ErrorCodes.Error_FubenID;
            }
            var matching = GetMatching(tbQuene);
            foreach (var characterSimpleData in character.mDatas)
            {
#if DEBUG
                PlayerLog.WriteLog((int) LogType.QueueMessage,
                    "In PushFront(). Matching Add ccharacter={0} c={1} ids={2}", characterSimpleData.Id, character.Guid,
                    character.mDatas.Select(d => d.Id).GetDataString());
#endif
                QueueManager.Characters.Add(characterSimpleData.Id, character);
            }
            matching.PushFront(character);
            //PushLog();
            return ErrorCodes.OK;
        }

        //因为时间结束，而要移除一堆人
        public void PopTimeOver(List<ulong> guids)
        {
            var cs = new List<QueueCharacter>();
            QueueResultBase result = null;
            foreach (var guid in guids)
            {
                var character = GetMatchingCharacter(guid);
                if (character == null)
                {
                    continue;
                }
                if (character.result != null)
                {
                    if (result != null && result != character.result)
                    {
                        Logger.Error("PopTimeOver result not same!");
                    }
                    result = character.result;
                    cs.Add(character);
                }
                foreach (var matchingCharacter in character.mDatas)
                {
                    if (guids.Contains(matchingCharacter.Id))
                    {
                        Remove(matchingCharacter.Id, eLeaveMatchingType.TimeOut);
                    }
                    else
                    {
                        Remove(matchingCharacter.Id, eLeaveMatchingType.TeamOther);
                    }
                }
            }
            if (result != null)
            {
                result.RemoveCharacterList(cs);
            }
        }

        //结束排队
        public void Pop(ulong guid, eLeaveMatchingType type)
        {
            PlayerLog.WriteLog((int) LogType.QueueMessage, "Pop  character={0},type={1}", guid, type);
            var character = GetMatchingCharacter(guid);
            if (character == null)
            {
                return;
            }
            if (type != eLeaveMatchingType.Success)
            {
                if (character.result != null)
                {
                    character.result.StopTrigger();
                    character.result.RemoveCharacterOne(character);
                }
            }
            switch (type)
            {
                case eLeaveMatchingType.Unknow:
                    break;
                case eLeaveMatchingType.TimeOut:
                    break;
                case eLeaveMatchingType.TeamOther:
                    break;
                case eLeaveMatchingType.Refuse:
                {
                    foreach (var matchingCharacter in character.mDatas)
                    {
                        //characters.Remove(matchingCharacter.Id);
                        if (matchingCharacter.Id == guid)
                        {
                            Remove(matchingCharacter.Id, eLeaveMatchingType.Refuse);
                            //TeamServer.Instance.ServerControl.TeamServerMessage(guid, (int)eLeaveMatchingType.Refuse, TeamServer.Instance);
                        }
                        else
                        {
                            Remove(matchingCharacter.Id, eLeaveMatchingType.TeamRefuse);
                            //TeamServer.Instance.ServerControl.TeamServerMessage(matchingCharacter.Id, (int)eLeaveMatchingType.TeamRefuse, TeamServer.Instance);
                        }
                    }
                }
                    break;
                case eLeaveMatchingType.TeamRefuse:
                    break;
                case eLeaveMatchingType.Cannel:
                case eLeaveMatchingType.PushCannel:
                case eLeaveMatchingType.MatchingBackCannel:
                {
#if DEBUG
                    PlayerLog.WriteLog((int) LogType.QueueMessage,
                        "Pop().Matching remove character={0} c={1} ids={2} type={3}",
                        guid, character.Guid, character.mDatas.Select(d => d.Id).GetDataString(), type);
#endif
                    QueueManager.Characters.Remove(guid);
                    foreach (var matchingCharacter in character.mDatas)
                    {
                        if (matchingCharacter.Id != guid)
                        {
                            Remove(matchingCharacter.Id, eLeaveMatchingType.TeamCannel);
                        }
                    }
                }
                    break;
                case eLeaveMatchingType.TeamCannel:
                    break;
                case eLeaveMatchingType.TeamChange:
                    foreach (var matchingCharacter in character.mDatas)
                    {
                        Remove(matchingCharacter.Id, eLeaveMatchingType.TeamChange);
                    }
                    break;
                case eLeaveMatchingType.InTemp:
                    break;
                case eLeaveMatchingType.Onlost:
#if DEBUG
                    PlayerLog.WriteLog((int) LogType.QueueMessage,
                        "Pop().Matching remove character={0} c={1} ids={2} type={3}",
                        guid, character.Guid, character.mDatas.Select(d => d.Id).GetDataString(), type);
#endif
                    QueueManager.Characters.Remove(guid);
                    break;
                case eLeaveMatchingType.OtherRefuse:
                    break;
                case eLeaveMatchingType.Success:
                    foreach (var matchingCharacter in character.mDatas)
                    {
                        Remove(matchingCharacter.Id, type);
                        //characters.Remove(matchingCharacter.Id);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
            character.mLogic.Pop(character, type);
        }

        //移除玩家的排队信息
        public void Remove(ulong guid, eLeaveMatchingType type)
        {
            var result = QueueManager.Characters.Remove(guid);
            PlayerLog.WriteLog((int) LogType.QueueMessage, "Remove().Matching remove id={0},type={1},result={2}", guid,
                type, result);
            TeamServer.Instance.ServerControl.TeamServerMessage(guid, (int) type, string.Empty);
        }

        //排队信息反馈
        public ErrorCodes MatchingBack(ulong guid, int result)
        {
            Logger.Info("Matching Back id={0},result={1}", guid, result);
            var character = GetMatchingCharacter(guid);
            if (character == null)
            {
                return ErrorCodes.Unknow;
            }
            if (character.result == null)
            {
                if (result == 0)
                {
                    Pop(guid, eLeaveMatchingType.MatchingBackCannel);
                }
                return ErrorCodes.Unknow;
            }
            return character.result.MatchingBack(guid, result == 1);
        }

        //获取排队信息
        public QueueCharacter GetQueueInfo(ulong guid)
        {
            var character = GetMatchingCharacter(guid);
            return character;
        }

        //处理队伍变化
        public void DealWithTeamChange(TeamChangedType type, List<ulong> teamList, ulong characterId)
        {
            Pop(characterId, eLeaveMatchingType.TeamChange);
            if (teamList.Count == 0)
            {
                PlayerLog.WriteLog((int) LogType.QueueMessage, "DealWithTeamChange Error! Team Count == 0!!");
                return;
            }
            PlayerLog.WriteLog((int) LogType.QueueMessage, "DealWithTeamChange character={0} type={1}", characterId,
                type);
            var captainId = teamList[0];
            var character = GetMatchingCharacter(captainId);
            if (character == null)
            {
                return;
            }
            character.mLogic.DealWithTeamChange(type, character, teamList, characterId);
        }

        //上线通知
        public void OnLine(ulong characterId)
        {
            var character = GetMatchingCharacter(characterId);
            if (character == null)
            {
                return;
            }
            if (character.result != null)
            {
                int state;
                if (character.result.CharacterState.TryGetValue(characterId, out state))
                {
                    if (state == 0)
                    {
                        TeamCharacterProxy toCharacterProxy;
                        if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(characterId, out toCharacterProxy))
                        {
                            toCharacterProxy.MatchingSuccess(character.mLogic.mQueueId);
                        }
                    }
                }
            }
        }

        //下线通知
        public void OnLost(ulong characterId)
        {
            var character = GetMatchingCharacter(characterId);
            if (character == null)
            {
                return;
            }
            if (character.result == null)
            {
                var characterteam = TeamManager.GetCharacterTeam(characterId);
                if (characterteam == null)
                {
//没有队伍的情况下
                    Pop(characterId, eLeaveMatchingType.Onlost);
                }
            }
            //if (character.team == null)
            //{//还在排队过程中
            //    Character characterteam = TeamManager.GetCharacterTeam(characterId);
            //    if (characterteam == null)
            //    {
            //        //没有队伍的情况下
            //        Pop(characterId, eLeaveMatchingType.Onlost);
            //        return;
            //    }
            //    else
            //    {
            //        //有队伍时，先继续这么排着
            //    }
            //}
            //else
            //{//已经排上队伍了
            //    Character characterteam = TeamManager.GetCharacterTeam(characterId);
            //    if (characterteam == null)
            //    {
            //        //没有队伍的情况下
            //        Pop(characterId, eLeaveMatchingType.Onlost);
            //        return;
            //    }
            //    else
            //    {
            //        //有队伍时，先继续这么排着
            //    }
            //}
        }

        //输出日志看看状态
        public void PushLog()
        {
            //每个排队列表的
            PlayerLog.WriteLog((int) LogType.QueueLog, "MatchingManager Matchings={0}--------------------------------",
                QueueManager.Matchings.Count);
            if (QueueManager.Matchings.Count > 0)
            {
                PlayerLog.WriteLog((int) LogType.QueueLog, "{");
                foreach (var matching in QueueManager.Matchings)
                {
                    matching.Value.PushLog();
                }
                PlayerLog.WriteLog((int) LogType.QueueLog, "}");
            }
            //每个玩家的
            PlayerLog.WriteLog((int) LogType.QueueLog,
                "MatchingManager characters={0}-------------------------------------", QueueManager.Characters.Count);
            if (QueueManager.Characters.Count > 0)
            {
                PlayerLog.WriteLog((int) LogType.QueueLog, "{");
                foreach (var matchingCharacter in QueueManager.Characters)
                {
                    var cccc = "";
                    if (matchingCharacter.Value.result == null)
                    {
                        cccc = "null";
                    }
                    else
                    {
                        cccc = matchingCharacter.Value.result.PushLog();
                    }
                    var dddd = "";

                    foreach (var data in matchingCharacter.Value.mDatas)
                    {
                        dddd = dddd + data.Id + ",";
                    }
                    PlayerLog.WriteLog((int) LogType.QueueLog, "MatchingCharacter id={0},result={1},character={2}",
                        matchingCharacter.Value.Guid, cccc, dddd);
                }
                PlayerLog.WriteLog((int) LogType.QueueLog, "}");
            }
        }
    }

    public static class QueueManager
    {
        public static Dictionary<ulong, QueueCharacter> Characters = new Dictionary<ulong, QueueCharacter>(); //排队玩家
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static Dictionary<int, QueueLogic> Matchings = new Dictionary<int, QueueLogic>(); //排队列表
        private static IQueueManager mStaticImpl;

        static QueueManager()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (QueueManager), typeof (QueueManagerDefaultImpl),
                o => { mStaticImpl = (IQueueManager) o; });
        }

        public static ErrorCodes CheckPush(QueueRecord tbQuene, List<ulong> ids)
        {
            return mStaticImpl.CheckPush(tbQuene, ids);
        }

        //清空某个排队ID下的所有正在排队的玩家
        public static void ClearQueue(int id)
        {
            if (id == -1)
            {
#if DEBUG
                PlayerLog.WriteLog((int) LogType.QueueMessage, "In ClearQueue(). Characters.Clear()");
#endif
                Matchings.Clear();
                Characters.Clear();
                return;
            }
            QueueLogic matching;
            if (!Matchings.TryGetValue(id, out matching))
            {
                return;
            }
            foreach (var character in matching.mCharacters)
            {
                foreach (var data in character.mDatas)
                {
                    Pop(data.Id, eLeaveMatchingType.Cannel);
                }
            }
            matching.mCharacters.Clear();
        }

        //处理队伍变化
        public static void DealWithTeamChange(TeamChangedType type, List<ulong> teamList, ulong characterId)
        {
            mStaticImpl.DealWithTeamChange(type, teamList, characterId);
        }

        //获得某个类型的排队
        public static QueueLogic GetMatching(QueueRecord tbQueue)
        {
            return mStaticImpl.GetMatching(tbQueue);
        }

        //获得某个玩家
        public static QueueCharacter GetMatchingCharacter(ulong guid)
        {
            return mStaticImpl.GetMatchingCharacter(guid);
        }

        //获取排队信息
        public static QueueCharacter GetQueueInfo(ulong guid)
        {
            return mStaticImpl.GetQueueInfo(guid);
        }

        //初始化
        public static void Init()
        {
            mStaticImpl.Init();
        }

        //排队信息反馈
        public static ErrorCodes MatchingBack(ulong guid, int result)
        {
            return mStaticImpl.MatchingBack(guid, result);
        }

        //上线通知
        public static void OnLine(ulong characterId)
        {
            mStaticImpl.OnLine(characterId);
        }

        //下线通知
        public static void OnLost(ulong characterId)
        {
            mStaticImpl.OnLost(characterId);
        }

        //结束排队
        public static void Pop(ulong guid, eLeaveMatchingType type)
        {
            mStaticImpl.Pop(guid, type);
        }

        //因为时间结束，而要移除一堆人
        public static void PopTimeOver(List<ulong> guids)
        {
            mStaticImpl.PopTimeOver(guids);
        }

        //开始排队
        public static ErrorCodes Push(int queueId, List<CharacterSimpleData> simpleDatas, MatchingResult result)
        {
            return mStaticImpl.Push(queueId, simpleDatas, result);
        }

        //新增至某队首
        public static ErrorCodes PushFront(QueueCharacter character)
        {
            return mStaticImpl.PushFront(character);
        }

        //输出日志看看状态
        public static void PushLog()
        {
            mStaticImpl.PushLog();
        }

        //把一个新的characterId 加入到 character 里
        public static void PushOneCharacter(ulong characterId, QueueCharacter character)
        {
            mStaticImpl.PushOneCharacter(characterId, character);
        }

        //移除玩家的排队信息
        public static void Remove(ulong guid, eLeaveMatchingType type)
        {
            mStaticImpl.Remove(guid, type);
        }

        internal static void RefreshMatchRelation()
        {
            foreach (var queueLogic in Matchings)
            {
                var queue = queueLogic.Value as FightQueue;
                if (queue != null)
                {
                    if (queue.NextQueue == null)
                    {
                        QueueLogic next = null;
                        Matchings.TryGetValue(queue.tbQueue.PreId, out next);
                        queue.NextQueue = next as FightQueue;
                    }
                }
            }
        }
    }
}