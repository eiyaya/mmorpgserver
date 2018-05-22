#region using

using System;
using System.Collections;
using System.Collections.Generic;
using DataContract;
using DataTable;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Team
{
    //工具函数
    public interface IUtility
    {
        IEnumerator AskEnterDungeonByTeamCoroutine(Coroutine co,
                                                   List<ulong> characters,
                                                   int serverId,
                                                   FubenRecord tbFuben,
                                                   ulong sceneGuid);

        IEnumerator GetCharacterNameCoroutine(Coroutine co, ulong characterId, AsyncReturnValue<string> name, AsyncReturnValue<int> job, AsyncReturnValue<int> level);
        IEnumerator GmCommand(Coroutine co, ulong characterId, string command, AsyncReturnValue<ErrorCodes> err);
        void NotifyEnterFuben(ulong characterId, int fubenId);
        void SendMail(ulong id, string title, string content, Dict_int_int_Data items);
        void SSChangeExdata(ulong id, Dict_int_int_Data changes);
    }

    public class UtilityDefaultImpl : IUtility
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //扣除进入副本需要的材料，并从characters中，剔除材料不足的玩家
        private IEnumerator DeleteDungeonMaterialCoroutine(Coroutine co, List<ulong> characters, FubenRecord tbFuben)
        {
            List<ulong> removes = new List<ulong>();

            foreach (var character in characters)
            {
                var removeThisOne = false;
                //消耗
                for (int i = 0, jmax = tbFuben.NeedItemId.Length; i != jmax; ++i)
                {
                    var itemId = tbFuben.NeedItemId[i];
                    var itemCount = tbFuben.NeedItemCount[i];
                    if (itemId == -1)
                    {
                        break;
                    }
                    var delItemMsg = TeamServer.Instance.LogicAgent.DeleteItem(character, itemId, itemCount, (int)eDeleteItemType.EnterFuBen);
                    yield return delItemMsg.SendAndWaitUntilDone(co);
                    if (delItemMsg.State != MessageState.Reply)
                    {
                        removeThisOne = true;
                        PlayerLog.WriteLog((ulong) LogType.TeamEnterDungeonDelItems,
                            "delItemMsg not replied! playerId = {0}, itemId = {1}, itemCount = {2}", character,
                            itemId, itemCount);
                        break;
                    }
                    if (delItemMsg.ErrorCode != (int) ErrorCodes.OK)
                    {
                        removeThisOne = true;
                        PlayerLog.WriteLog((ulong) LogType.TeamEnterDungeonDelItems,
                            "delItemMsg return with ERROR[{3}]! playerId = {0}, itemId = {1}, itemCount = {2}",
                            character, itemId, itemCount, delItemMsg.ErrorCode);
                        break;
                    }
                }
                if (removeThisOne)
                {
                    removes.Add(character);
                }
            }

            characters.RemoveAll(id => removes.Contains(id));
        }

        private static IEnumerator NotifyEnterFubenCoroutine(Coroutine co, ulong characterId, int fubenId)
        {
            var msg = TeamServer.Instance.LogicAgent.NotifyEnterFuben(characterId, fubenId);
            yield return msg.SendAndWaitUntilDone(co);
        }

        private IEnumerator SendMailCoroutine(Coroutine co,
                                              ulong id,
                                              string title,
                                              string content,
                                              Dict_int_int_Data items)
        {
            var msg = TeamServer.Instance.LogicAgent.SendMailToCharacter(id, title, content, items, 0);
            yield return msg.SendAndWaitUntilDone(co);
        }

        private static IEnumerator SSChangeExdataCoroutine(Coroutine co, ulong id, Dict_int_int_Data changes)
        {
            var logicAgent = TeamServer.Instance.LogicAgent;
            var msg = logicAgent.SSChangeExdata(id, changes);
            yield return msg.SendAndWaitUntilDone(co);
        }

        //开始创建副本
        public IEnumerator AskEnterDungeonByTeamCoroutine(Coroutine co,
                                                          List<ulong> characters,
                                                          int serverId,
                                                          FubenRecord tbFuben,
                                                          ulong sceneGuid)
        {
            PlayerLog.WriteLog((int) LogType.QueueMessage, "NotifyCreateChangeSceneCoroutine  Team={0}",
                characters.GetDataString());

            //先把进入副本应扣除的材料，扣除掉
            var co1 = CoroutineFactory.NewSubroutine(DeleteDungeonMaterialCoroutine, co, characters, tbFuben);
            if (co1.MoveNext())
            {
                yield return co1;
            }

            //组队进入副本时，根据合服ID进行
            var serverLogicId = SceneExtension.GetServerLogicId(serverId);
            var sceneInfo = new ChangeSceneInfo
            {
                SceneId = tbFuben.SceneId,
                ServerId = serverLogicId,
                SceneGuid = sceneGuid,
                Type = (int) eScnenChangeType.EnterDungeon
            };
            sceneInfo.Guids.AddRange(characters);
            if (sceneInfo.Pos == null)
            {
                var param = new SceneParam();
                sceneInfo.Pos = param;
                if (tbFuben.OpenTime[0] != -1)
                {
                    int hour;
                    int min;
                    if (Utils.GetDungeonOpenTime(tbFuben, out hour, out min))
                    {
                        param.Param.Add(hour);
                        param.Param.Add(min);
                    }
                    else
                    {
                        Logger.Warn("NotifyCreateChangeSceneCoroutine can't enter scene {0}", tbFuben.SceneId);
                        yield break;
                    }
                }
            }

            var msgChgScene = TeamServer.Instance.SceneAgent.SBChangeSceneByTeam(characters[0], sceneInfo);
            yield return msgChgScene.SendAndWaitUntilDone(co, TimeSpan.FromSeconds(30));
        }

        public IEnumerator GetCharacterNameCoroutine(Coroutine co, ulong characterId, AsyncReturnValue<string> name, AsyncReturnValue<int> job, AsyncReturnValue<int> level)
        {
            name.Value = "";
            var msg = TeamServer.Instance.LogicAgent.GetLogicSimpleData(characterId, 0);
            yield return msg.SendAndWaitUntilDone(co);
            if (msg.State != MessageState.Reply)
            {
                yield break;
            }
            if (msg.ErrorCode != 0)
            {
                yield break;
            }
            name.Value = msg.Response.Name;
            level.Value = msg.Response.Level;
            job.Value = msg.Response.TypeId;
        }

        public void SSChangeExdata(ulong id, Dict_int_int_Data changes)
        {
            CoroutineFactory.NewCoroutine(SSChangeExdataCoroutine, id, changes).MoveNext();
        }

        public void SendMail(ulong id, string title, string content, Dict_int_int_Data items)
        {
            CoroutineFactory.NewCoroutine(SendMailCoroutine, id, title, content, items).MoveNext();
        }

        public void NotifyEnterFuben(ulong characterId, int fubenId)
        {
            CoroutineFactory.NewCoroutine(NotifyEnterFubenCoroutine, characterId, fubenId).MoveNext();
        }

        public IEnumerator GmCommand(Coroutine co, ulong characterId, string command, AsyncReturnValue<ErrorCodes> err)
        {
            err.Value = ErrorCodes.OK;

            var strs = command.Split(',');
            if (strs.Length < 1)
            {
                err.Value = ErrorCodes.ParamError;
                yield break;
            }
            if (String.Compare(strs[0], "!!ReloadTable", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (strs.Length < 2)
                {
                    err.Value = ErrorCodes.ParamError;
                    yield break;
                }
                GameMaster.ReloadTable(strs[1]);
                yield break;
            }
            var nIndex = 0;
            var IntData = new List<int>();
            foreach (var s in strs)
            {
                if (nIndex != 0)
                {
                    int TempInt;
                    if (!Int32.TryParse(s, out TempInt))
                    {
                        err.Value = ErrorCodes.ParamError;
                        yield break;
                    }
                    IntData.Add(TempInt);
                }
                nIndex++;
            }
            if (String.Compare(strs[0], "!!LookMatch", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 0)
                {
                    //设置血量
                    GameMaster.PushMatchingLog();
                }
                else
                {
                    err.Value = ErrorCodes.ParamError;
                }
            }
            else if (String.Compare(strs[0], "!!CleanMatch", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 1)
                {
                    //设置血量
                    GameMaster.CleanMatching(IntData[0]);
                }
                else
                {
                    err.Value = ErrorCodes.ParamError;
                }
            }
            else if (String.Compare(strs[0], "!!UnionMoneyAdd", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 1)
                {
                    //设置血量
                    GameMaster.UnionMoneyAdd(characterId, IntData[0]);
                }
                else
                {
                    err.Value = ErrorCodes.ParamError;
                }
            }
            else if (String.Compare(strs[0], "!!AllianceWarBid", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 1)
                {
                    //设置血量
                    var e = GameMaster.AllianceWarBid(characterId, IntData[0]);
                    err.Value = e;
                }
                else
                {
                    err.Value = ErrorCodes.ParamError;
                }
            }
            else if (String.Compare(strs[0], "!!AllianceWarBegin", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 6)
                {
                    //设置血量
                    GameMaster.AllianceWarBegin(IntData[0],
                        new DateTime(IntData[1], IntData[2], IntData[3], IntData[4], IntData[5], 0));
                }
                else
                {
                    err.Value = ErrorCodes.ParamError;
                }
            }
            else if (string.Compare(strs[0], "!!AllianceWarStartBid", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (IntData.Count == 1)
                {
                    //战盟攻城申请
                    GameMaster.AllianceWarStartBid(IntData[0]);
                }
                else
                {
                    err.Value = ErrorCodes.ParamError;
                }
            }
        }
    }

    //工具函数
    public static class Utility
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static IUtility mImpl;

        static Utility()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (Utility), typeof (UtilityDefaultImpl),
                o => { mImpl = (IUtility) o; });
        }

        //开始创建副本
        public static IEnumerator AskEnterDungeonByTeamCoroutine(Coroutine co,
                                                                 List<ulong> characters,
                                                                 int serverId,
                                                                 FubenRecord tbFuben,
                                                                 ulong sceneGuid)
        {
            return mImpl.AskEnterDungeonByTeamCoroutine(co, characters, serverId, tbFuben, sceneGuid);
        }

        //获得一个玩家的名字
        public static IEnumerator GetCharacterNameCoroutine(Coroutine co,
                                                            ulong characterId,
                                                            AsyncReturnValue<string> name, AsyncReturnValue<int> job, AsyncReturnValue<int> level)
        {
            return mImpl.GetCharacterNameCoroutine(co, characterId, name, job, level);
        }

        public static IEnumerator GmCommand(Coroutine co,
                                            ulong characterId,
                                            string command,
                                            AsyncReturnValue<ErrorCodes> err)
        {
            return mImpl.GmCommand(co, characterId, command, err);
        }

        public static void NotifyEnterFuben(ulong characterId, int fubenId)
        {
            mImpl.NotifyEnterFuben(characterId, fubenId);
        }

        public static void SendMail(ulong id, string title, string content, Dict_int_int_Data items)
        {
            mImpl.SendMail(id, title, content, items);
        }

        public static void SSChangeExdata(ulong id, Dict_int_int_Data changes)
        {
            mImpl.SSChangeExdata(id, changes);
        }
    }

    public enum TeamChangedType
    {
        Request = 1, //加入
        AcceptRequest, //同意邀请
        AcceptJoin, //同意申请
        Leave, //离开
        Disband, //解散
        Kick //踢人
    }
}