using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Database;
using DataContract;
using DataTable;
using GameMasterServerService;
using GiftCodeDb;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using Scorpion;
using ServiceStack;
using ServiceStack.Text;
using Shared;

namespace GameMaster
{
    public static class GameMasterServerControlHelper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Logger GmToolsLogger = LogManager.GetLogger("GmToolsLogger");

        public static IEnumerator DoCommand(Coroutine coroutine, ulong characterId, string command, AsyncReturnValue<GMCommandResultBaseMsg> GMResult)
        {
            GMResult.Value = new GMCommandResultBaseMsg();
            if (command.Trim().Length == 0)
            {
                GMResult.Value.RetStr = "DoCommand Failed";
                yield break;
            }
            var commands = command.Split('\n').ToList();
            //先检查gm命令是否正确
            var invalidCmd = GMCommandManager.CheckCommands(commands);
            if (invalidCmd.Length > 0)
            {
                GMResult.Value.RetStr = "DoCommand Failed";
                yield break;
            }
            //检查玩家是否在线，如果在线，就直接把gm命令发过去
            //all
            var all = GMCommandManager.SplitCommands(commands, eGmCommandType.GMAll);
            var allOk = all.Count > 0;
            //logic
            {
                var cs = GMCommandManager.SplitCommands(commands, eGmCommandType.GMLogic);
                if (cs.Count > 0 || all.Count > 0)
                {
                    var sa = new StringArray();
                    sa.Items.AddRange(cs);
                    sa.Items.AddRange(all);
                    var msg1 = GameMasterServer.Instance.LogicAgent.GMCommand(characterId, sa);
                    yield return msg1.SendAndWaitUntilDone(coroutine);
                    if (msg1.State == MessageState.Reply)
                    {
                        if (msg1.ErrorCode == (int)ErrorCodes.OK)
                        {
                            commands.RemoveAll(c => cs.Contains(c));
                        }
                        else
                        {
                            allOk = false;
                        }
                    }
                    else
                    {
                        allOk = false;
                    }
                }
            }
            //scene
            {
                var cs = GMCommandManager.SplitCommands(commands, eGmCommandType.GMScene);
                if (cs.Count > 0 || all.Count > 0)
                {
                    var sa = new StringArray();
                    sa.Items.AddRange(cs);
                    sa.Items.AddRange(all);
                    var msg1 = GameMasterServer.Instance.SceneAgent.GMCommand(characterId, sa);
                    yield return msg1.SendAndWaitUntilDone(coroutine);
                    if (msg1.State == MessageState.Reply)
                    {
                        if (msg1.ErrorCode == (int)ErrorCodes.OK)
                        {
                            commands.RemoveAll(c => cs.Contains(c));
                        }
                        else
                        {
                            allOk = false;
                        }
                    }
                    else
                    {
                        allOk = false;
                    }
                }
            }
            //chat
            {
                var cs = GMCommandManager.SplitCommands(commands, eGmCommandType.GMChat);
                if (cs.Count > 0 || all.Count > 0)
                {
                    var sa = new StringArray();
                    sa.Items.AddRange(cs);
                    sa.Items.AddRange(all);
                    var msg1 = GameMasterServer.Instance.ChatAgent.GMCommand(characterId, sa);
                    yield return msg1.SendAndWaitUntilDone(coroutine);
                    if (msg1.State == MessageState.Reply)
                    {
                        if (msg1.ErrorCode == (int)ErrorCodes.OK)
                        {
                            commands.RemoveAll(c => cs.Contains(c));
                        }
                        else
                        {
                            allOk = false;
                        }
                    }
                    else
                    {
                        allOk = false;
                    }
                }
            }
            //rank
            {
                var cs = GMCommandManager.SplitCommands(commands, eGmCommandType.GMRank);
                if (cs.Count > 0 || all.Count > 0)
                {
                    var sa = new StringArray();
                    sa.Items.AddRange(cs);
                    sa.Items.AddRange(all);
                    var msg1 = GameMasterServer.Instance.RankAgent.GMCommand(characterId, sa);
                    yield return msg1.SendAndWaitUntilDone(coroutine);
                    if (msg1.State == MessageState.Reply)
                    {
                        if (msg1.ErrorCode == (int)ErrorCodes.OK)
                        {
                            commands.RemoveAll(c => cs.Contains(c));
                        }
                        else
                        {
                            allOk = false;
                        }
                    }
                    else
                    {
                        allOk = false;
                    }
                }
            }
            //team
            {
                var cs = GMCommandManager.SplitCommands(commands, eGmCommandType.GMTeam);
                if (cs.Count > 0 || all.Count > 0)
                {
                    var sa = new StringArray();
                    sa.Items.AddRange(cs);
                    sa.Items.AddRange(all);
                    var msg1 = GameMasterServer.Instance.TeamAgent.GMCommand(characterId, sa);
                    yield return msg1.SendAndWaitUntilDone(coroutine);
                    if (msg1.State == MessageState.Reply)
                    {
                        if (msg1.ErrorCode == (int)ErrorCodes.OK)
                        {
                            commands.RemoveAll(c => cs.Contains(c));
                        }
                        else
                        {
                            allOk = false;
                        }
                    }
                    else
                    {
                        allOk = false;
                    }
                }
            }
            if (allOk)
            {
                commands.RemoveAll(c => all.Contains(c));
            }
            if (commands.Count == 0)
            {
                GMResult.Value.RetStr = "DoCommand Success";
                yield break;
            }
            //如果不在线，则把gm命令先存在gmserver里
            var oldCommands = GMManager.GetCommand(characterId);
            if (oldCommands != null)
            {
                commands.AddRange(oldCommands);
            }
            GMManager.AddCommand(characterId, commands);
            GMResult.Value.RetStr = "DoCommand Success";
        }

        public static IEnumerator SendMailsByName(Coroutine co, GameMasterService _this, List<string> Names, long Time, string Title, string Content, Dictionary<int, int> Items, AsyncReturnValue<GMSendMailRetMsg> gmResult)
        {
            gmResult.Value = new GMSendMailRetMsg();
            gmResult.Value.RetStr = "SendMailsByName FailedId";
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var loginAgent = GameMasterServer.Instance.LoginAgent;
            var ids = new List<ulong>();
            foreach (var name in Names)
            {
                var playerIdData = loginAgent.GetCharacterIdByName(0, name);
                yield return playerIdData.SendAndWaitUntilDone(co);

                if (playerIdData.State != MessageState.Reply)
                {
                    yield break;
                }

                if (playerIdData.ErrorCode != (int)ErrorCodes.OK)
                {
                    yield break;
                }

                ids.Add(playerIdData.Response);

                yield return gameMasterServerControl.Wait(co, TimeSpan.FromMilliseconds(50));

                Logger.Info("{0}: arg name = {1}.", DateTime.Now, name);
            }

            var mailId = GameMasterServer.Instance.DB.GetNextId(co, (int)DataCategory.GameMasterMail,
                GameMasterServerControl.DBGmMailIdKey);
            yield return mailId;

            if (mailId.Status != DataStatus.Ok)
            {
                gmResult.Value.RetStr = "Can not get mail id in func SendMailsById()";
                Logger.Error("Can not get mail id in func SendMailsById()!");
                yield break;
            }

            var mail = new GmMailData();
            mail.Id = mailId.Data;
            mail.Time = Time;
            mail.Title = Title;
            mail.Content = Content;
            mail.Gm = "admin";
            Dict_int_int_Data tempItems = new Dict_int_int_Data();
            foreach (var data in Items)
            {
                tempItems.Data.Add(data.Key, data.Value);
            }
            mail.Items = tempItems;
            mail.Characters.AddRange(ids);

            gameMasterServerControl.mTimedMailData.AddData(mail);

            gmResult.Value.MailId = mail.Id;
            gmResult.Value.RetStr = "SendMailsByName Success";
        }

        public static IEnumerator SendMailsByIdEx(Coroutine co, GameMasterService _this, List<ulong> Ids, long Time, string Title, string Content, AsyncReturnValue<GMSendMailRetMsg> gmResult)
        {
            gmResult.Value = new GMSendMailRetMsg();
            gmResult.Value.RetStr = "SendMailsByIdEx Failed";
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var mailId = GameMasterServer.Instance.DB.GetNextId(co, (int)DataCategory.GameMasterMail,
                GameMasterServerControl.DBGmMailIdKey);
            yield return mailId;

            if (mailId.Status != DataStatus.Ok)
            {
                gmResult.Value.RetStr = "Can not get mail id in func SendMailsById()!";
                Logger.Error("Can not get mail id in func SendMailsById()!");
                yield break;
            }

            var mail = new GmMailData();
            mail.Id = mailId.Data;
            mail.Time = Time;
            mail.Title = Title;
            mail.Content = Content;
            mail.Gm = "admin";
            Dict_int_int_Data tempItems = new Dict_int_int_Data();
            mail.Items = tempItems;
            mail.IsFanKui = 1;
            mail.Characters.AddRange(Ids);

            gameMasterServerControl.mTimedMailData.AddData(mail);

            gmResult.Value.MailId = mail.Id;
            gmResult.Value.RetStr = "SendMailsByIdEx Success";
        }

        public static IEnumerator SendMailsById(Coroutine co, GameMasterService _this, List<ulong> Ids, long Time, string Title, string Content, Dictionary<int, int> Items, AsyncReturnValue<GMSendMailRetMsg> gmResult)
        {
            gmResult.Value = new GMSendMailRetMsg();
            gmResult.Value.RetStr = "SendMailsById Failed";
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var mailId = GameMasterServer.Instance.DB.GetNextId(co, (int)DataCategory.GameMasterMail,
                GameMasterServerControl.DBGmMailIdKey);
            yield return mailId;

            if (mailId.Status != DataStatus.Ok)
            {
                gmResult.Value.RetStr = "Can not get mail id in func SendMailsById()!";
                Logger.Error("Can not get mail id in func SendMailsById()!");
                yield break;
            }

            var mail = new GmMailData();
            mail.Id = mailId.Data;
            mail.Time = Time;
            mail.Title = Title;
            mail.Content = Content;
            mail.Gm = "admin";
            Dict_int_int_Data tempItems = new Dict_int_int_Data();
            foreach (var data in Items)
            {
                tempItems.Data.Add(data.Key, data.Value);
            }
            mail.Items = tempItems;
            mail.IsFanKui = 0;
            mail.Characters.AddRange(Ids);

            gameMasterServerControl.mTimedMailData.AddData(mail);

            gmResult.Value.MailId = mail.Id;
            gmResult.Value.RetStr = "SendMailsById Success";
        }

        public static IEnumerator SendMailsToServers(Coroutine co, GameMasterService _this, List<uint> Servers, List<long> lt, string Title, string Content, Dictionary<int, int> Items, AsyncReturnValue<GMSendMailRetMsg> gmResult)
        {
            gmResult.Value = new GMSendMailRetMsg();
            gmResult.Value.RetStr = "SendMailsToServers....";
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var mailId = GameMasterServer.Instance.DB.GetNextId(co, (int)DataCategory.GameMasterMail,
                GameMasterServerControl.DBGmMailIdKey);
            yield return mailId;

            if (mailId.Status != DataStatus.Ok)
            {
                gmResult.Value.RetStr = "Can not get mail id in func SendMailsToServers()!";
                Logger.Error("Can not get mail id in func SendMailsToServers()!");
                yield break;
            }
            var Time = lt[0];
            var OverTime = lt[1];
            var isNew = (int) lt[2];
            var serverIds = Servers;

            var mail = new GmMailData();
            mail.Id = mailId.Data;
            mail.Time = Time;
            mail.Title = Title;
            mail.Content = Content;
            mail.Gm = "admin";
            mail.OverTime = OverTime;
            mail.IsNew = isNew;
            Dict_int_int_Data tempItems = new Dict_int_int_Data();
            foreach (var data in Items)
            {
                tempItems.Data.Add(data.Key, data.Value);
            }
            mail.Items = tempItems;

            if (serverIds.Count > 0)
            {
                mail.Servers.AddRange(serverIds);
            }
            else
            {
                for (uint i = 0; i < Constants.ServerCount; i++)
                {
                    mail.Servers.Add(i);
                }
            }

            gameMasterServerControl.mTimedMailData.AddData(mail);
            gmResult.Value.MailId = mail.Id;
            gmResult.Value.RetStr += "......Send Over";
        }

        public static Coroutine DelWaitingMails(Coroutine co, GameMasterService _this, List<ulong> items)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var mails = gameMasterServerControl.mTimedMailData.Mails.Mails;
            var tmp = new List<DataContract.GmMailData>();
            foreach (var id in items)
            {
                foreach (var data in mails)
                {
                    if (data.Id == id)
                    {
                        tmp.Add(data);
                        break;
                    }
                }
            }
            foreach (var value in tmp)
            {
                mails.Remove(value);
            }

            if (mails.Count > 0)
            {
                gameMasterServerControl.mTimedMailData.StartTimer();
            }
            gameMasterServerControl.mTimedMailData.Mails.NextIdx = 0;

            var co1 = CoroutineFactory.NewSubroutine(gameMasterServerControl.SaveGmMails, co);
            return co1;
        }
        public static IEnumerator ReloadTable(Coroutine co, GameMasterService _this, string tableName, AsyncReturnValue<GMCommandResultBaseMsg> gmResult)
        {
            Table.ReloadTable(tableName);

            gmResult.Value = new GMCommandResultBaseMsg();
            var instance = GameMasterServer.Instance;
            var name = tableName;

			var msg0 = instance.SceneAgent.ServerGMCommand("ReloadTable",name);
			var msg1 = instance.RankAgent.ServerGMCommand("ReloadTable", name);
			var msg2 = instance.TeamAgent.ServerGMCommand("ReloadTable", name);
			var msg3 = instance.LogicAgent.ServerGMCommand("ReloadTable", name);
			var msg4 = instance.ChatAgent.ServerGMCommand("ReloadTable", name);
			var msg5 = instance.ActivityAgent.ServerGMCommand("ReloadTable", name);
			var msg6 = instance.LoginAgent.ServerGMCommand("ReloadTable", name);

            msg0.SendAndWaitUntilDone(co);
            msg1.SendAndWaitUntilDone(co);
            msg2.SendAndWaitUntilDone(co);
            msg3.SendAndWaitUntilDone(co);
            msg4.SendAndWaitUntilDone(co);
            msg5.SendAndWaitUntilDone(co);
            msg6.SendAndWaitUntilDone(co);

            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;

            gmResult.Value.RetStr = "ReloadTable Success";
        }
        public static IEnumerator Broadcast(Coroutine co, GameMasterService _this, List<uint> serverIds, long time, string content, AsyncReturnValue<GMSendBroadCastRetMsg> gmResult)
        {
            gmResult.Value = new GMSendBroadCastRetMsg();
            gmResult.Value.RetStr = "Broadcast Failed";
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var broadcastId = GameMasterServer.Instance.DB.GetNextId(co, (int)DataCategory.GameMasterBroadcast,
                GameMasterServerControl.DBGmBroadcastIdKey);
            yield return broadcastId;

            if (broadcastId.Status != DataStatus.Ok)
            {
                Logger.Error("Can not get mail id in func SendMailsById()!");
                yield break;
            }

            gmResult.Value.BroadCastId = broadcastId.Data;

            var broadcast = new GmBroadcastData();
            broadcast.Id = broadcastId.Data;
            broadcast.Time = time;
            broadcast.Content = content;
            broadcast.Gm = "admin";
            if (serverIds.Count > 0)
            {
                broadcast.Servers.AddRange(serverIds);
            }
            else
            {
                for (uint i = 0; i < Constants.ServerCount; i++)
                {
                    broadcast.Servers.Add(i);
                }
            }

            gameMasterServerControl.mTimedBroadcasts.AddData(broadcast);

            gmResult.Value.RetStr = "Broadcast Success";
        }
        public static Coroutine DelWaitingBroadcasts(Coroutine co, GameMasterService _this, List<ulong> items)
        {
            var gameMasterServerControl = (GameMasterServerControl)_this;
            var broadcasts = gameMasterServerControl.mTimedBroadcasts.Broadcasts;
            var broadcastsInner = broadcasts.Broadcasts;

            var tmp = new List<DataContract.GmBroadcastData>();
            foreach (var id in items)
            {
                foreach (var data in broadcastsInner)
                {
                    if (data.Id == id)
                    {
                        tmp.Add(data);
                        break;
                    }
                }
            }
            foreach (var value in tmp)
            {
                broadcastsInner.Remove(value);
            }

            if (broadcastsInner.Count > 0)
            {
                gameMasterServerControl.mTimedBroadcasts.StartTimer();
            }
            broadcasts.NextIdx = 0;

            var co1 = CoroutineFactory.NewSubroutine(gameMasterServerControl.SaveGmBroadcasts, co);
            return co1;
        }

        public static IEnumerator UpdateServerAll(Coroutine co, GameMasterService _this)
        {
            GameMasterServer.Instance.UpdateManager.Update();

            var instance = GameMasterServer.Instance;

            var msg0 = instance.SceneAgent.UpdateServer(0);
            var msg1 = instance.LoginAgent.UpdateServer(0);
            var msg2 = instance.ActivityAgent.UpdateServer(0);
            var msg3 = instance.RankAgent.UpdateServer(0);
            var msg4 = instance.TeamAgent.UpdateServer(0);
            var msg5 = instance.LogicAgent.UpdateServer(0);
            var msg6 = instance.ChatAgent.UpdateServer(0);

            msg0.SendAndWaitUntilDone(co);
            msg1.SendAndWaitUntilDone(co);
            msg2.SendAndWaitUntilDone(co);
            msg3.SendAndWaitUntilDone(co);
            msg4.SendAndWaitUntilDone(co);
            msg5.SendAndWaitUntilDone(co);
            msg6.SendAndWaitUntilDone(co);

            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
        }

		public static IEnumerator GenGiftCode(Coroutine coroutine, GameMasterService _this, int type, int count,int channelId, AsyncReturnValue<string> gmResult)
        {
            if (gmResult == null)
            {
                yield break;
            }

            var gift = Table.GetGiftCode(type);
            if (null == gift)
            {
				yield break;
            }
            //var drop = Table.GetDropMother(gift.DropId);
            if (string.IsNullOrEmpty(gift.Drop1Id) && string.IsNullOrEmpty(gift.Drop2Id) &&
                string.IsNullOrEmpty(gift.Drop2Id))
            {
				yield break;
            }
            var prefix = IdGenerator.Confuse((ulong)type);
            var sb = new StringBuilder();
            var codes = new List<string>();
            for (var i = 0; i < count; i++)
            {
                var code = prefix + IdGenerator.Next(8);
                codes.Add(code);
                sb.Append(code).AppendLine();
            }


            var asyncRet = GameMasterServerControl.GiftCodeDb.ExecuteSqlTranAsync(coroutine, GiftCodeDbConnection.NewGiftCodeSqls(codes, channelId), GameMasterServer.Instance.GameMasterAgent);
			yield return asyncRet;
			var ret = asyncRet.Value;
			if (ret > 0)
			{
				gmResult.Value = sb.ToString();
			}
			else
			{
				gmResult.Value = "GenGiftCode Failed";

			}
        }

        public static IEnumerator Silence(Coroutine co, GameMasterService _this, JObject jsonResult, AsyncReturnValue<GMCommandResultBaseMsg> gmResult)
        {
            gmResult.Value = new GMCommandResultBaseMsg();
            gmResult.Value.RetStr = "Silence Failed";
            var characterId = (ulong)jsonResult["CharacterId"];
            var mask = (uint)jsonResult["Mask"];

            var msg1 = GameMasterServer.Instance.ChatAgent.Silence(characterId, mask);
            yield return msg1.SendAndWaitUntilDone(co);
            if (msg1.State != MessageState.Reply)
            {
                yield break;
            }

            var msg2 = GameMasterServer.Instance.SceneAgent.Silence(characterId, mask);
            yield return msg2.SendAndWaitUntilDone(co);
            if (msg2.State != MessageState.Reply)
            {
                yield break;
            }

            gmResult.Value.RetStr = "Silence Success";
        }

        public static IEnumerator GetPlayerDataByPlayerName(Coroutine co, GameMasterService _this, string name, AsyncReturnValue<GMPlayerInfoMsg> gmResult)
        {
            gmResult.Value = new GMPlayerInfoMsg();
            gmResult.Value.RetStr = "GetPlayerDataByPlayerName Failed";

            var loginAgent = GameMasterServer.Instance.LoginAgent;
            var playerIdData = loginAgent.GetPlayerIdByAccount(0, name);
            yield return playerIdData.SendAndWaitUntilDone(co);

            if (playerIdData.State != MessageState.Reply)
            {
                yield break;
            }

            if (playerIdData.ErrorCode != (int)ErrorCodes.OK)
            {
                yield break;
            }

            var playerId = playerIdData.Response;
            var playerData1 = loginAgent.GetPlayerData(0, playerId, 0);
            yield return playerData1.SendAndWaitUntilDone(co);

            if (playerData1.State != MessageState.Reply)
            {
                yield break;
            }

            if (playerData1.ErrorCode != (int)ErrorCodes.OK)
            {
                yield break;
            }
            var ret = gmResult.Value;
            ret.CharacterServers.Clear();
            ret.Ret = (int)ErrorCodes.OK;
            ret.RetStr = "GetPlayerDataByPlayerName Success";
            ret.PlayerId = playerData1.Response.Id;
            ret.Account = playerData1.Response.Name;
            ret.Type = playerData1.Response.Type;
            ret.FoundTime = DateTime.FromBinary(playerData1.Response.FoundTime).ToString("yyyy/MM/dd HH:mm:ss");
            ret.LoginDay = playerData1.Response.LoginDay;
            ret.LoginTotal = playerData1.Response.LoginTotal;
            ret.LastTime = playerData1.Response.LastTime;
            ret.BindPhone = playerData1.Response.BindPhone;
            ret.BindEmail = playerData1.Response.BindEmail;
            ret.LockTime = DateTime.FromBinary(playerData1.Response.LockTime).ToString("yyyy/MM/dd HH:mm:ss");
            foreach (var data in playerData1.Response.CharactersServers)
            {
                GMCharacterServers tmp = new GMCharacterServers();
                tmp.Infos.Clear();
                tmp.ServerId = data.ServerId;
                foreach (var value in data.Characters)
                {
                    GMCharacterInfo info = new GMCharacterInfo();
                    info.CharacterId = value.CharacterId;
                    info.IsOnline = value.IsOnline;
                    info.Name = value.Name;
                    info.Type = value.Type;

                    // logic
                    var characterData = GameMasterServer.Instance.LogicAgent.GetCharacterData(value.CharacterId, value.CharacterId);
                    yield return characterData.SendAndWaitUntilDone(co);
                    if (characterData.State == MessageState.Reply && characterData.ErrorCode == (int)ErrorCodes.OK)
                    {
                        if (characterData.Response != null)
                        {
                            info.Level = characterData.Response.Level;
                            info.VipLevel = characterData.Response.VipLevel;
                            info.Diamond = characterData.Response.Yuanbao;
                            info.Money = characterData.Response.Money;
                        }
                    }

                    // chat
                    var chatAgent = GameMasterServer.Instance.ChatAgent;
                    var chatData = chatAgent.GetSilenceState(value.CharacterId, 0);
                    yield return chatData.SendAndWaitUntilDone(co);
                    if (chatData.State == MessageState.Reply && chatData.ErrorCode == (int)ErrorCodes.OK)
                    {
                        info.SilenceMask = chatData.Response;
                    }

                    // scene
                    var sceneAgent = GameMasterServer.Instance.SceneAgent;
                    var playerData2 = sceneAgent.GetCharacterData(value.CharacterId, value.CharacterId);
                    yield return playerData2.SendAndWaitUntilDone(co);
                    if (playerData2.State == MessageState.Reply && playerData2.ErrorCode == (int)ErrorCodes.OK)
                    {
                        info.Name = playerData2.Response.Name;
                        info.SceneId = playerData2.Response.SceneId;
                        info.PosX = playerData2.Response.X;
                        info.PosY = playerData2.Response.Y;
                        info.Attr.Clear();
                        if (playerData2.Response != null)
                        {
                            info.Attr = playerData2.Response.AttrList;
                        }
                    }

                    tmp.Infos.Add(info);
                }
                ret.CharacterServers.Add(tmp);
            }
        }

        public static IEnumerator GetPlayerDataByPlayerId(Coroutine co, GameMasterService _this, ulong Id, AsyncReturnValue<GMPlayerInfoMsg> gmResult)
        {
            gmResult.Value = new GMPlayerInfoMsg();
            gmResult.Value.RetStr = "GetPlayerDataByPlayerId Failed";

            var playerId = Id;
            var loginAgent = GameMasterServer.Instance.LoginAgent;

            var playerData1 = loginAgent.GetPlayerData(0, playerId, 0);
            yield return playerData1.SendAndWaitUntilDone(co);

            if (playerData1.State != MessageState.Reply)
            {
                yield break;
            }

            if (playerData1.ErrorCode != (int)ErrorCodes.OK)
            {
                yield break;
            }

            var ret = gmResult.Value;
            ret.CharacterServers.Clear();
            ret.Ret = (int)ErrorCodes.OK;
            ret.RetStr = "GetPlayerDataByPlayerId Success";
            ret.PlayerId = playerData1.Response.Id;
            ret.Account = playerData1.Response.Name;
            ret.Type = playerData1.Response.Type;
            ret.FoundTime = DateTime.FromBinary(playerData1.Response.FoundTime).ToString("yyyy/MM/dd HH:mm:ss");
            ret.LoginDay = playerData1.Response.LoginDay;
            ret.LoginTotal = playerData1.Response.LoginTotal;
            ret.LastTime = playerData1.Response.LastTime;
            ret.BindPhone = playerData1.Response.BindPhone;
            ret.BindEmail = playerData1.Response.BindEmail;
            ret.LockTime = DateTime.FromBinary(playerData1.Response.LockTime).ToString("yyyy/MM/dd HH:mm:ss");
            foreach (var data in playerData1.Response.CharactersServers)
            {
                GMCharacterServers tmp = new GMCharacterServers();
                tmp.Infos.Clear();
                tmp.ServerId = data.ServerId;
                foreach (var value in data.Characters)
                {
                    GMCharacterInfo info = new GMCharacterInfo();
                    info.CharacterId = value.CharacterId;
                    info.IsOnline = value.IsOnline;
                    info.Name = value.Name;
                    info.Type = value.Type;

                    // logic
                    var characterData = GameMasterServer.Instance.LogicAgent.GetCharacterData(value.CharacterId, value.CharacterId);
                    yield return characterData.SendAndWaitUntilDone(co);
                    if (characterData.State == MessageState.Reply && characterData.ErrorCode == (int)ErrorCodes.OK)
                    {
                        if (characterData.Response != null)
                        {
                            info.Level = characterData.Response.Level;
                            info.VipLevel = characterData.Response.VipLevel;
                            info.Diamond = characterData.Response.Yuanbao;
                            info.Money = characterData.Response.Money;
                        }
                    }

                    // chat
                    var chatAgent = GameMasterServer.Instance.ChatAgent;
                    var chatData = chatAgent.GetSilenceState(value.CharacterId, 0);
                    yield return chatData.SendAndWaitUntilDone(co);
                    if (chatData.State == MessageState.Reply && chatData.ErrorCode == (int)ErrorCodes.OK)
                    {
                        info.SilenceMask = chatData.Response;
                    }

                    // scene
                    var sceneAgent = GameMasterServer.Instance.SceneAgent;
                    var playerData2 = sceneAgent.GetCharacterData(value.CharacterId, value.CharacterId);
                    yield return playerData2.SendAndWaitUntilDone(co);
                    if (playerData2.State == MessageState.Reply && playerData2.ErrorCode == (int)ErrorCodes.OK)
                    {
                        info.Name = playerData2.Response.Name;
                        info.SceneId = playerData2.Response.SceneId;
                        info.PosX = playerData2.Response.X;
                        info.PosY = playerData2.Response.Y;
                        info.Attr.Clear();
                        if (playerData2.Response != null)
                        {
                            info.Attr = playerData2.Response.AttrList;
                        }
                    }

                    tmp.Infos.Add(info);
                }
                ret.CharacterServers.Add(tmp);
            }
        }

        public static IEnumerator GetPlayerDataByCharacterName(Coroutine co, GameMasterService _this, string name, AsyncReturnValue<GMPlayerInfoMsg> gmResult)
        {
            gmResult.Value = new GMPlayerInfoMsg();
            gmResult.Value.RetStr = "GetPlayerDataByCharacterName Failed";

            var loginAgent = GameMasterServer.Instance.LoginAgent;
            var playerIdData = loginAgent.GetCharacterIdByName(0, name);
            yield return playerIdData.SendAndWaitUntilDone(co);

            if (playerIdData.State != MessageState.Reply)
            {
                yield break;
            }

            if (playerIdData.ErrorCode != (int)ErrorCodes.OK)
            {
                yield break;
            }

            var charId = playerIdData.Response;
            var playerData1 = loginAgent.GetPlayerData(0, 0, charId);
            yield return playerData1.SendAndWaitUntilDone(co);

            if (playerData1.State != MessageState.Reply)
            {
                yield break;
            }

            if (playerData1.ErrorCode != (int)ErrorCodes.OK)
            {
                yield break;
            }
            var ret = gmResult.Value;
            ret.CharacterServers.Clear();
            ret.Ret = (int)ErrorCodes.OK;
            ret.RetStr = "GetPlayerDataByCharacterName Success";
            ret.PlayerId = playerData1.Response.Id;
            ret.Account = playerData1.Response.Name;
            ret.Type = playerData1.Response.Type;
            ret.FoundTime = DateTime.FromBinary(playerData1.Response.FoundTime).ToString("yyyy/MM/dd HH:mm:ss");
            ret.LoginDay = playerData1.Response.LoginDay;
            ret.LoginTotal = playerData1.Response.LoginTotal;
            ret.LastTime = playerData1.Response.LastTime;
            ret.BindPhone = playerData1.Response.BindPhone;
            ret.BindEmail = playerData1.Response.BindEmail;
            ret.LockTime = DateTime.FromBinary(playerData1.Response.LockTime).ToString("yyyy/MM/dd HH:mm:ss");
            foreach (var data in playerData1.Response.CharactersServers)
            {
                GMCharacterServers tmp = new GMCharacterServers();
                tmp.Infos.Clear();
                tmp.ServerId = data.ServerId;
                foreach (var value in data.Characters)
                {
                    GMCharacterInfo info = new GMCharacterInfo();
                    info.CharacterId = value.CharacterId;
                    info.IsOnline = value.IsOnline;
                    info.Name = value.Name;
                    info.Type = value.Type;

                    // logic
                    var characterData = GameMasterServer.Instance.LogicAgent.GetCharacterData(value.CharacterId, value.CharacterId);
                    yield return characterData.SendAndWaitUntilDone(co);
                    if (characterData.State == MessageState.Reply && characterData.ErrorCode == (int)ErrorCodes.OK)
                    {
                        if (characterData.Response != null)
                        {
                            info.Level = characterData.Response.Level;
                            info.VipLevel = characterData.Response.VipLevel;
                            info.Diamond = characterData.Response.Yuanbao;
                            info.Money = characterData.Response.Money;
                        }
                    }

                    // chat
                    var chatAgent = GameMasterServer.Instance.ChatAgent;
                    var chatData = chatAgent.GetSilenceState(value.CharacterId, 0);
                    yield return chatData.SendAndWaitUntilDone(co);
                    if (chatData.State == MessageState.Reply && chatData.ErrorCode == (int)ErrorCodes.OK)
                    {
                        info.SilenceMask = chatData.Response;
                    }

                    // scene
                    var sceneAgent = GameMasterServer.Instance.SceneAgent;
                    var playerData2 = sceneAgent.GetCharacterData(value.CharacterId, value.CharacterId);
                    yield return playerData2.SendAndWaitUntilDone(co);
                    if (playerData2.State == MessageState.Reply && playerData2.ErrorCode == (int)ErrorCodes.OK)
                    {
                        info.Name = playerData2.Response.Name;
                        info.SceneId = playerData2.Response.SceneId;
                        info.PosX = playerData2.Response.X;
                        info.PosY = playerData2.Response.Y;
                        info.Attr.Clear();
                        if (playerData2.Response != null)
                        {
                            info.Attr = playerData2.Response.AttrList;
                        }
                    }

                    tmp.Infos.Add(info);
                }
                ret.CharacterServers.Add(tmp);
            }
        }
        public static IEnumerator GetPlayerDataByCharacterId(Coroutine co, GameMasterService _this, ulong Id, AsyncReturnValue<GMPlayerInfoMsg> gmResult)
        {
            gmResult.Value = new GMPlayerInfoMsg();
            gmResult.Value.RetStr = "GetPlayerDataByCharacterId Failed";

            var charId = Id;
            var loginAgent = GameMasterServer.Instance.LoginAgent;

            var playerData1 = loginAgent.GetPlayerData(0, 0, charId);
            yield return playerData1.SendAndWaitUntilDone(co);

            if (playerData1.State != MessageState.Reply)
            {
                yield break;
            }

            if (playerData1.ErrorCode != (int)ErrorCodes.OK)
            {
                yield break;
            }
            var ret = gmResult.Value;
            ret.CharacterServers.Clear();
            ret.Ret = (int)ErrorCodes.OK;
            ret.RetStr = "GetPlayerDataByCharacterId Success";
            ret.PlayerId = playerData1.Response.Id;
            ret.Account = playerData1.Response.Name;
            ret.Type = playerData1.Response.Type;
            ret.FoundTime = DateTime.FromBinary(playerData1.Response.FoundTime).ToString("yyyy/MM/dd HH:mm:ss");
            ret.LoginDay = playerData1.Response.LoginDay;
            ret.LoginTotal = playerData1.Response.LoginTotal;
            ret.LastTime = playerData1.Response.LastTime;
            ret.BindPhone = playerData1.Response.BindPhone;
            ret.BindEmail = playerData1.Response.BindEmail;
            ret.LockTime = DateTime.FromBinary(playerData1.Response.LockTime).ToString("yyyy/MM/dd HH:mm:ss");
            foreach (var data in playerData1.Response.CharactersServers)
            {
                GMCharacterServers tmp = new GMCharacterServers();
                tmp.Infos.Clear();
                tmp.ServerId = data.ServerId;
                foreach (var value in data.Characters)
                {
                    GMCharacterInfo info = new GMCharacterInfo();
                    info.CharacterId = value.CharacterId;
                    info.IsOnline = value.IsOnline;
                    info.Name = value.Name;
                    info.Type = value.Type;

                    // logic
                    var characterData = GameMasterServer.Instance.LogicAgent.GetCharacterData(value.CharacterId, value.CharacterId);
                    yield return characterData.SendAndWaitUntilDone(co);
                    if (characterData.State == MessageState.Reply && characterData.ErrorCode == (int)ErrorCodes.OK)
                    {
                        if (characterData.Response != null)
                        {
                            info.Level = characterData.Response.Level;
                            info.VipLevel = characterData.Response.VipLevel;
                            info.Diamond = characterData.Response.Yuanbao;
                            info.Money = characterData.Response.Money;
                        }
                    }

                    // chat
                    var chatAgent = GameMasterServer.Instance.ChatAgent;
                    var chatData = chatAgent.GetSilenceState(value.CharacterId, 0);
                    yield return chatData.SendAndWaitUntilDone(co);
                    if (chatData.State == MessageState.Reply && chatData.ErrorCode == (int)ErrorCodes.OK)
                    {
                        info.SilenceMask = chatData.Response;
                    }

                    // scene
                    var sceneAgent = GameMasterServer.Instance.SceneAgent;
                    var playerData2 = sceneAgent.GetCharacterData(value.CharacterId, value.CharacterId);
                    yield return playerData2.SendAndWaitUntilDone(co);
                    if (playerData2.State == MessageState.Reply && playerData2.ErrorCode == (int)ErrorCodes.OK)
                    {
                        info.Name = playerData2.Response.Name;
                        info.SceneId = playerData2.Response.SceneId;
                        info.PosX = playerData2.Response.X;
                        info.PosY = playerData2.Response.Y;
                        info.Attr.Clear();
                        if (playerData2.Response != null)
                        {
                            info.Attr = playerData2.Response.AttrList;
                        }
                        info.fight_value = playerData2.Response.fight_value;//新增统计的战斗力
                    }

                    tmp.Infos.Add(info);
                }
                ret.CharacterServers.Add(tmp);
            }
        }
		static string HtmlContent = "";
        public static IEnumerator ProcessRequestAsync(Coroutine co, GameMasterServerControl _this, HttpListenerContext context)
        {
            // 翻译context 执行逻辑  返回request
            var url = context.Request.Url;
            var api = context.Request.RawUrl;
            string postData;
            using (var br = new BinaryReader(context.Request.InputStream))
            {
                postData =
                    Encoding.UTF8.GetString(
                        br.ReadBytes(int.Parse(context.Request.ContentLength64.ToString())));
            }

	        if (api == "/")
	        {
#if DEBUG
		        if (1==1)
#else
				if (string.IsNullOrEmpty(HtmlContent))
#endif

				{
					HtmlContent = "";
					StreamReader sr = null;
					try
					{
						sr = new StreamReader("GMPage.html", Encoding.Default);
						String line;
						while ((line = sr.ReadLine()) != null)
						{
							HtmlContent += line + "\n";
						}
					}
					catch (Exception e)
					{
						Logger.Error(e.Message);
					}
					finally
					{
						if (null != sr)
						{
							sr.Close();
						}
					}
		        }
		        
			    context.Response.ContentType = "text/html";
			    context.Response.StatusCode = 200;
			    context.Response.StatusDescription = "OK";
				ReplyResult(ref context, HtmlContent);
		        
		        yield break;
	        }

            GmToolsLogger.Info("url = "+ url + "    api = "+ api + "    postData =" + postData);
            var GMResult = "Failed";

	        JObject jsonResult = null;

	        try
	        {
		        if (!string.IsNullOrEmpty(postData))
		        {
					jsonResult = (JObject)JsonConvert.DeserializeObject(postData);    
		        }
	        }
	        catch (Exception e)
	        {
				GmToolsLogger.Error(e.Message);
				ReplyResult(ref context, e.Message);
				yield break;
	        }

// 			if (null == jsonResult)
// 			{
// 				yield break;
// 			}


	        switch (api)
	        {
		        case "/GMCommand": // "CharacterId": "123123","Command":"100000082"
		        {
			        var ret = new GMCommandResultBaseMsg();
			        ret.RetStr = "GMCommand Failed";
			        GMResult = JsonConvert.SerializeObject(ret);

		            if (jsonResult == null)
		            {
                        ReplyResult(ref context, GMResult);
                        yield break;
		            }

		            var request = JsonConvert.DeserializeObject<GMRequestCommand>(jsonResult.ToString());
                    if (request == null || request.CharacterId == null || request.Command == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }

			        var returnValue = AsyncReturnValue<GMCommandResultBaseMsg>.Create();
                    var subroutine = CoroutineFactory.NewSubroutine(DoCommand, co, ulong.Parse(request.CharacterId), request.Command, returnValue);
			        if (subroutine.MoveNext())
			        {
				        yield return subroutine;
				        ret = returnValue.Value;
				        GMResult = JsonConvert.SerializeObject(ret);
			        }
			        returnValue.Dispose();
		        }
			        break;
                case "/ReloadTable": // "TableName":"a.txt"
		        {
			        var ret = new GMCommandResultBaseMsg();
			        ret.RetStr = "ReloadTable Failed";
			        GMResult = JsonConvert.SerializeObject(ret);

                    if (jsonResult == null)
                    {
                        ReplyResult(ref context, GMResult);
                        yield break;
                    }

                    var request = JsonConvert.DeserializeObject<GMRequestReloadTable>(jsonResult.ToString());
                    if (request == null || request.TableName == null)
                    {
                        ReplyResult(ref context, GMResult);
                        yield break;
                    }

			        var returnValue = AsyncReturnValue<GMCommandResultBaseMsg>.Create();
                    var subroutine = CoroutineFactory.NewSubroutine(ReloadTable, co, _this, request.TableName, returnValue);
			        if (subroutine.MoveNext())
			        {
				        yield return subroutine;
				        ret = returnValue.Value;
				        GMResult = JsonConvert.SerializeObject(ret);
			        }
			        returnValue.Dispose();
		        }
					break;
		        case "/SendMailByName" :
			        //"Names": "名字1,名字2", "Time": 123, "Title": "title", "Content": "context", "Items": "2,100|3,200"
		        {
			        var ret = new GMSendMailRetMsg();
			        ret.RetStr = "SendMailByName Failed";
			        GMResult = JsonConvert.SerializeObject(ret);

			        var returnValue = AsyncReturnValue<GMSendMailRetMsg>.Create();
			        var result = SendMailByName(co, _this, jsonResult, returnValue);
			        if (result != null)
			        {
				        result.MoveNext();
				        yield return result;
				        ret = returnValue.Value;
				        GMResult = JsonConvert.SerializeObject(ret);
			        }
			        returnValue.Dispose();
		        }
					break;
		        case "/SendMailById" :
			        //"Ids": "123456,45678", "Time": 123, "Title": "title", "Content": "context", "Items": "2,100|3,200"
		        {
			        var ret = new GMSendMailRetMsg();
			        ret.RetStr = "SendMailById Failed";
			        GMResult = JsonConvert.SerializeObject(ret);

			        var returnValue = AsyncReturnValue<GMSendMailRetMsg>.Create();
			        var result = SendMailById(co, _this, jsonResult, returnValue);
			        if (result != null)
			        {
				        result.MoveNext();
				        yield return result;
				        ret = returnValue.Value;
				        GMResult = JsonConvert.SerializeObject(ret);
			        }
			        returnValue.Dispose();
		        }
					break;
		        case "/SendMailByServers" :
			        //"Servers": "123456,45678", "Time": 123, "Title": "title", "Content": "context", "Items": "2,100|3,200"
		        {
			        var ret = new GMSendMailRetMsg();
			        ret.RetStr = "SendMailByServers Failed";
			        GMResult = JsonConvert.SerializeObject(ret);

			        var returnValue = AsyncReturnValue<GMSendMailRetMsg>.Create();
			        var result = SendMailByServers(co, _this, jsonResult, returnValue);
			        if (result != null)
			        {
				        result.MoveNext();
				        yield return result;
				        ret = returnValue.Value;
				        GMResult = JsonConvert.SerializeObject(ret);
			        }
			        returnValue.Dispose();
		        }
				break;
		        case "/DelWaitingMails" : // "Mails":"100000082"
		        {
			        var result = DelWaitingMails(co, _this, jsonResult);
			        if (result != null)
			        {
				        result.MoveNext();
				        yield return result;
			        }
		        }
			        break;
		        case "/BroadCastMessage" : //"Servers": "123456,45678", "Time": 123, "Content": "context"
		        {
			        var ret = new GMSendBroadCastRetMsg();
			        ret.RetStr = "BroadCastMessage Failed";
			        GMResult = JsonConvert.SerializeObject(ret);

			        var returnValue = AsyncReturnValue<GMSendBroadCastRetMsg>.Create();
			        var result = BroadCastMessage(co, _this, jsonResult, returnValue);
			        if (result != null)
			        {
				        result.MoveNext();
				        yield return result;
				        ret = returnValue.Value;
				        GMResult = JsonConvert.SerializeObject(ret);
			        }
			        returnValue.Dispose();
		        }
			        break;
		        case "/DelWaitingBroadCastMessage" : //"BroadcastsMessageIds":"100000082"
		        {
			        var result = DelBroadCastMessage(co, _this, jsonResult);
			        if (result != null)
			        {
				        result.MoveNext();
				        yield return result;
			        }
		        }
			        break;
		        case "/KickCharacter" : //"CharacterId":"123456", "Name":"XX"
		        {
			        var ret = new GMCommandResultBaseMsg();
			        ret.RetStr = "KickCharacter Success";
			        GMResult = JsonConvert.SerializeObject(ret);
			        if (jsonResult["CharacterId"] == null || jsonResult["Name"] == null)
			        {
				        ret.RetStr = "KickCharacter Failed";
				        GMResult = JsonConvert.SerializeObject(ret);
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }

			        var charId = (ulong) jsonResult["CharacterId"];
			        var name = jsonResult["Name"].ToString();
			        var kickMsg = GameMasterServer.Instance.LoginAgent.GMKickCharacter(0, charId, name);
			        yield return kickMsg.SendAndWaitUntilDone(co);
		        }
			        break;
		        case "/UpdateServerAll" : // API = /UpdateServerAll
		        {
			        var ret = new GMCommandResultBaseMsg();
			        ret.RetStr = "UpdateServerAll Success";
			        GMResult = JsonConvert.SerializeObject(ret);

			        var subroutine = CoroutineFactory.NewSubroutine(UpdateServerAll, co, _this);
			        if (subroutine.MoveNext())
			        {
				        yield return subroutine;
			        }
		        }
			        break;
		        case "/GenGiftCode" :
		        {
			        var ret = new GMCommandResultBaseMsg();
			        ret.RetStr = "GenGiftCode Failed";
			        GMResult = JsonConvert.SerializeObject(ret);

			        if (jsonResult["Type"] == null || jsonResult["Count"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }

			        var type = (int) jsonResult["Type"];
			        var count = (int) jsonResult["Count"];
		            var channelId = (int)jsonResult["channels"];
		            
			        var returnValue = AsyncReturnValue<string>.Create();

                    var subroutine = CoroutineFactory.NewSubroutine(GenGiftCode, co, _this, type, count, channelId, returnValue);
					if (subroutine.MoveNext())
					{
						yield return subroutine;
					}
				    ret.RetStr = returnValue.Value;

			        GMResult = JsonConvert.SerializeObject(ret);
		        }
			        break;
		        case "/CheckGiftCode" :
		        {
			        var ret = new GMCommandResultBaseMsg();
			        ret.RetStr = "CheckGiftCode Failed";
			        GMResult = JsonConvert.SerializeObject(ret);

			        if (jsonResult["Code"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }

			        var code = jsonResult["Code"].ToString();
					var result = GameMasterServerControl.GiftCodeDb.CheckCodeIsActiveAsync(co, code, GameMasterServer.Instance.GameMasterAgent);
			        yield return result;

			        ret.RetStr = result.Value.ToString();
			        GMResult = JsonConvert.SerializeObject(ret);
		        }
			        break;
		        case "/Silence" : // "CharacterId":"123456", "Mask":"0"  0---关闭禁言  1--禁言
		        {
			        var ret = new GMCommandResultBaseMsg();
			        ret.RetStr = "Silence Failed";
			        GMResult = JsonConvert.SerializeObject(ret);

			        if (jsonResult["CharacterId"] == null || jsonResult["Mask"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }

			        var returnValue = AsyncReturnValue<GMCommandResultBaseMsg>.Create();
			        var subroutine = CoroutineFactory.NewSubroutine(Silence, co, _this, jsonResult, returnValue);
			        if (subroutine.MoveNext())
			        {
				        yield return subroutine;
				        ret = returnValue.Value;
				        GMResult = JsonConvert.SerializeObject(ret);
			        }
			        returnValue.Dispose();
		        }
			        break;
		        case "/LockAccount" : // "PlayerId":"123456", "EndTime":"2017-08-01 12:20:30"
		        {
			        var ret = new GMCommandResultBaseMsg();
			        ret.RetStr = "LockAccount Failed";
			        GMResult = JsonConvert.SerializeObject(ret);

			        if (jsonResult["PlayerId"] == null || jsonResult["EndTime"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }

			        var playerId = (ulong) jsonResult["PlayerId"];
			        var endTime = jsonResult["EndTime"];
			        var time = Convert.ToDateTime(endTime.ToString());
			        if (time == DateTime.MinValue)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }

			        var msg1 = GameMasterServer.Instance.LoginAgent.LockAccount(0, playerId, time.ToBinary());
			        yield return msg1.SendAndWaitUntilDone(co);

			        ret.RetStr = "LockAccount Success";
			        GMResult = JsonConvert.SerializeObject(ret);
		        }
			        break;
		        case "/GetPlayerDataByPlayerName" : // "Account":"名字"
		        {
			        var ret = new GMPlayerInfoMsg();
			        ret.RetStr = "GetPlayerDataByPlayerName Failed";
			        GMResult = JsonConvert.SerializeObject(ret);

			        if (jsonResult["Account"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }

			        var name = jsonResult["Account"].ToString();
			        var returnValue = AsyncReturnValue<GMPlayerInfoMsg>.Create();
			        var subroutine = CoroutineFactory.NewSubroutine(GetPlayerDataByPlayerName, co, _this, name, returnValue);
			        if (subroutine.MoveNext())
			        {
				        yield return subroutine;
				        ret = returnValue.Value;
				        GMResult = JsonConvert.SerializeObject(ret);
			        }
			        returnValue.Dispose();
		        }
			        break;
		        case "/GetPlayerDataByPlayerId" : // "PlayerId":123
		        {
			        var ret = new GMPlayerInfoMsg();
			        ret.RetStr = "GetPlayerDataByPlayerId Failed";
			        GMResult = JsonConvert.SerializeObject(ret);

			        if (jsonResult["PlayerId"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }

			        var Id = (ulong) jsonResult["PlayerId"];
			        var returnValue = AsyncReturnValue<GMPlayerInfoMsg>.Create();
			        var subroutine = CoroutineFactory.NewSubroutine(GetPlayerDataByPlayerId, co, _this, Id, returnValue);
			        if (subroutine.MoveNext())
			        {
				        yield return subroutine;
				        ret = returnValue.Value;
				        GMResult = JsonConvert.SerializeObject(ret);
			        }
			        returnValue.Dispose();
		        }
			        break;
		        case "/GetPlayerDataByCharacterName" : //"Name":"名字"
		        {
			        var ret = new GMPlayerInfoMsg();
			        ret.RetStr = "GetPlayerDataByCharacterName Failed";

			        GMResult = JsonConvert.SerializeObject(ret);
			        if (jsonResult["Name"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }

			        var name = jsonResult["Name"].ToString();
			        var returnValue = AsyncReturnValue<GMPlayerInfoMsg>.Create();
			        var subroutine = CoroutineFactory.NewSubroutine(GetPlayerDataByCharacterName, co, _this, name, returnValue);
			        if (subroutine.MoveNext())
			        {
				        yield return subroutine;
				        ret = returnValue.Value;
				        GMResult = JsonConvert.SerializeObject(ret);
			        }
			        returnValue.Dispose();
		        }
			        break;
		        case "/GetPlayerDataByCharacterId" : // "CharacterId":123
		        {
			        var ret = new GMPlayerInfoMsg();
			        ret.RetStr = "GetPlayerDataByCharacterName Failed";
			        GMResult = JsonConvert.SerializeObject(ret);

			        if (jsonResult["CharacterId"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }

			        var Id = (ulong) jsonResult["CharacterId"];
			        var returnValue = AsyncReturnValue<GMPlayerInfoMsg>.Create();
			        var subroutine = CoroutineFactory.NewSubroutine(GetPlayerDataByCharacterId, co, _this, Id, returnValue);
			        if (subroutine.MoveNext())
			        {
				        yield return subroutine;
				        ret = returnValue.Value;
				        GMResult = JsonConvert.SerializeObject(ret);
			        }
			        returnValue.Dispose();
		        }
			        break;
		        case "/GetLog" : // "CharacterId":100010117, "Date":"2017-08-14"
		        {
			        var ret = new GMCommandResultBaseMsg();
			        ret.RetStr = "GetLog Failed";
			        GMResult = JsonConvert.SerializeObject(ret);

			        if (jsonResult["CharacterId"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }
			        if (jsonResult["Date"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }
			        var characterId = (ulong) jsonResult["CharacterId"];
			        var date = jsonResult["Date"].ToString();

			        string data;
			        var err = LogSettings.ReadBlock(characterId, date, out data);
			        if (err == ErrorCodes.OK)
			        {
				        ret.RetStr = data;
			        }
			        GMResult = JsonConvert.SerializeObject(ret);
		        }
			        break;
		        case "/GetFanKui" :
		        {
			        var ret = new GMGetFanKuiList();
			        ret.RetStr = "GetFanKui Failed";
			        GMResult = JsonConvert.SerializeObject(ret);

			        if (jsonResult["BeginTime"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }
			        if (jsonResult["EndTime"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }
			        if (jsonResult["StartIndex"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }
			        if (jsonResult["EndIndex"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }
			        if (jsonResult["State"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }

			        var BeginTime = jsonResult["BeginTime"].ToString();
			        var EndTime = jsonResult["EndTime"].ToString();
			        var StartIndex = (int) jsonResult["StartIndex"];
			        var EndIndex = (int) jsonResult["EndIndex"];
			        var State = (int) jsonResult["State"];

			        //读数据库
					var result = GameMasterServerControl.GiftCodeDb.GetCharacterFanKuiAsync(co,
						BeginTime, 
						EndTime,
						StartIndex,
				        EndIndex,
						State,
						GameMasterServer.Instance.GameMasterAgent);
			        yield return result;

			        ret.RetStr = "GetFanKui Success";
			        ret.FanKuiList.Clear();
			        foreach (var value in result.Value.FanKuiList)
			        {
				        GMGetFanKui tmp = new GMGetFanKui();
				        tmp.Id = value.Id;
				        tmp.CharacterId = value.CharacterId;
				        tmp.Name = value.Name;
				        tmp.Title = value.Title;
				        tmp.Content = value.Content;
				        tmp.State = value.State;
				        tmp.CreateTime = value.CreateTime;
				        ret.FanKuiList.Add(tmp);
			        }
			        GMResult = JsonConvert.SerializeObject(ret);
		        }
			        break;
		        case "/SendGMFanKuiMail" : //"Ids": "123456,45678", "Time": 123, "Title": "title", "Content": "context"
		        {
			        var ret = new GMSendMailRetMsg();
			        ret.RetStr = "SendMailsByIdEx Failed";
			        GMResult = JsonConvert.SerializeObject(ret);

			        var returnValue = AsyncReturnValue<GMSendMailRetMsg>.Create();
			        var result = SendMailById(co, _this, jsonResult, returnValue, 1);
			        if (result != null)
			        {
				        result.MoveNext();
				        yield return result;
				        ret = returnValue.Value;
				        GMResult = JsonConvert.SerializeObject(ret);
			        }
			        returnValue.Dispose();
		        }
			        break;
		        case "/SetGMFanKuiState" : // "Ids": "1,2,3,4,5,6,7,8,9", State:66
		        {
			        var ret = new GMSendMailRetMsg();
			        ret.RetStr = "SetGMFanKuiState Failed";
			        GMResult = JsonConvert.SerializeObject(ret);


			        if (jsonResult["Ids"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }
			        if (jsonResult["State"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }

			        var ids = jsonResult["Ids"].ToString();
			        var state = (int) jsonResult["State"];

			        var str = ids.Split(',');
			        var list = new List<int>();
			        foreach (var vaule in str)
			        {
				        list.Add(int.Parse(vaule));
			        }

					var result = GameMasterServerControl.GiftCodeDb.SetCharacterFanKuiStateAsync(co, 
						list, 
						state, 
						GameMasterServer.Instance.GameMasterAgent);
			        yield return result;
					ret.RetStr = "SetGMFanKuiState Ret" + result.Value;
			        GMResult = JsonConvert.SerializeObject(ret);
		        }
			        break;
		        case "/GetAlianceInfo" : // "ServerId": 3, "StartIndex": 0, "EndIndex":10
		        {
			        var ret = new GMGetAlianceInfoList();
			        ret.RetStr = "GetAlianceInfo Failed";
			        GMResult = JsonConvert.SerializeObject(ret);

			        if (jsonResult["ServerId"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }
			        if (jsonResult["StartIndex"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }
			        if (jsonResult["EndIndex"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }

			        var serverId = (int) jsonResult["ServerId"];
			        var startIndex = (int) jsonResult["StartIndex"];
			        var endIndex = (int) jsonResult["EndIndex"];

			        var kickMsg = GameMasterServer.Instance.TeamAgent.SSGetAlliance(0, serverId, startIndex, endIndex,
				        string.Empty);
			        yield return kickMsg.SendAndWaitUntilDone(co);
			        if (kickMsg.Response == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }
			        ret.Infos.Clear();
			        foreach (var value in kickMsg.Response.alliances)
			        {
				        GMGetAlianceInfo info = new GMGetAlianceInfo();
				        info.Id = value.Id;
				        info.Name = value.Name;
				        info.Leader = value.Leader;
				        info.LeaderName = value.LeaderName;
				        info.Member.Clear();
				        foreach (var memData in value.Members)
				        {
					        GMAlianceMember tmp = new GMAlianceMember();
					        tmp.Id = memData.Id;
					        tmp.Name = memData.Name;
					        tmp.Level = memData.Level;
					        tmp.RoleId = memData.RoleId;
					        info.Member.Add(tmp);
				        }
				        info.ServerId = value.ServerId;
				        info.State = value.State;
				        info.Notice = value.Notice;
				        info.Level = value.Level;
				        info.CreateTime = value.CreateTime;

				        ret.Infos.Add(info);
			        }
			        ret.RetStr = "GetAlianceInfo Success";

			        GMResult = JsonConvert.SerializeObject(ret);
		        }
			        break;
		        case "/GetAlianceInfoByName" :// "ServerId": 3, "Name": "ddd"
		        {
			        var ret = new GMGetAlianceInfoList();
			        ret.RetStr = "GetAlianceInfo Failed";
			        GMResult = JsonConvert.SerializeObject(ret);

			        if (jsonResult["ServerId"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }
			        if (jsonResult["Name"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }

			        var serverId = (int) jsonResult["ServerId"];
			        var name = jsonResult["Name"].ToString();

			        var kickMsg = GameMasterServer.Instance.TeamAgent.SSGetAlliance(0, serverId, 0, 0, name);
			        yield return kickMsg.SendAndWaitUntilDone(co);
			        if (kickMsg.Response == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }
			        ret.Infos.Clear();
			        foreach (var value in kickMsg.Response.alliances)
			        {
				        GMGetAlianceInfo info = new GMGetAlianceInfo();
				        info.Id = value.Id;
				        info.Name = value.Name;
				        info.Leader = value.Leader;
				        info.LeaderName = value.LeaderName;
				        info.Member.Clear();
				        foreach (var memData in value.Members)
				        {
					        GMAlianceMember tmp = new GMAlianceMember();
					        tmp.Id = memData.Id;
					        tmp.Name = memData.Name;
					        tmp.Level = memData.Level;
					        tmp.RoleId = memData.RoleId;
					        info.Member.Add(tmp);
				        }
				        info.ServerId = value.ServerId;
				        info.State = value.State;
				        info.Notice = value.Notice;
				        info.Level = value.Level;
				        info.CreateTime = value.CreateTime;

				        ret.Infos.Add(info);
			        }
			        ret.RetStr = "GetAlianceInfo Success";

			        GMResult = JsonConvert.SerializeObject(ret);
		        }
			        break;
		        case "/SetAlianceJob" : // "ServerId": 3, "AllianceId": 111, "Guid":100010137, "Type":-1  (3 = 盟主   -1 = 踢出帮派)
		        {
			        var ret = new GMGetAlianceInfoList();
			        ret.RetStr = "SetAlianceJob Failed";
			        GMResult = JsonConvert.SerializeObject(ret);

			        if (jsonResult["ServerId"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }
			        if (jsonResult["AllianceId"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }
			        if (jsonResult["Guid"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }
			        if (jsonResult["Type"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }

			        var serverId = (int) jsonResult["ServerId"];
			        var allianceId = (int) jsonResult["AllianceId"];
			        var opGuid = (ulong) jsonResult["Guid"];
			        var type = (int) jsonResult["Type"];

			        var kickMsg = GameMasterServer.Instance.TeamAgent.GMChangeJurisdiction(0, serverId, allianceId, opGuid, type);
			        yield return kickMsg.SendAndWaitUntilDone(co);
			        if (kickMsg.State == MessageState.Reply)
			        {
				        if (kickMsg.ErrorCode == (int) ErrorCodes.OK)
				        {
					        ret.RetStr = "SetAlianceJob Success";
				        }
			        }
			        GMResult = JsonConvert.SerializeObject(ret);
		        }
			        break;
		        case "/SetAlianceNotice" : // "ServerId": 3, "AllianceId": 111, "Content":"asdasd"
		        {
			        var ret = new GMGetAlianceInfoList();
			        ret.RetStr = "SetAlianceNotice Failed";
			        GMResult = JsonConvert.SerializeObject(ret);

			        if (jsonResult["ServerId"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }
			        if (jsonResult["AllianceId"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }
			        if (jsonResult["Content"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }

			        var serverId = (int) jsonResult["ServerId"];
			        var allianceId = (int) jsonResult["AllianceId"];
			        var content = jsonResult["Content"].ToString();

			        var kickMsg = GameMasterServer.Instance.TeamAgent.GMChangeAllianceNotice(0, serverId, allianceId, content);
			        yield return kickMsg.SendAndWaitUntilDone(co);
			        if (kickMsg.State == MessageState.Reply)
			        {
				        if (kickMsg.ErrorCode == (int) ErrorCodes.OK)
				        {
					        ret.RetStr = "SetAlianceNotice Success";
				        }
			        }
			        GMResult = JsonConvert.SerializeObject(ret);
		        }
			        break;
		        case "/GMDelAllicance" : //"AllianceId": 84
		        {
			        var ret = new GMGetAlianceInfoList();
			        ret.RetStr = "GMDelAllicance Failed";
			        GMResult = JsonConvert.SerializeObject(ret);

			        if (jsonResult["AllianceId"] == null)
			        {
				        ReplyResult(ref context, GMResult);
				        yield break;
			        }

			        var allianceId = (int) jsonResult["AllianceId"];

			        var kickMsg = GameMasterServer.Instance.TeamAgent.GMDelAllicance(0, allianceId);
			        yield return kickMsg.SendAndWaitUntilDone(co);
			        if (kickMsg.State == MessageState.Reply)
			        {
				        if (kickMsg.ErrorCode == (int) ErrorCodes.OK)
				        {
					        ret.RetStr = "GMDelAllicance Success";
				        }
			        }
			        GMResult = JsonConvert.SerializeObject(ret);
		        }
			        break;

				case "/GetWaitingMails" :
				//case "/DelWaitingMails":
			        {
				         var mails = _this.mTimedMailData.Mails;
					        var ret = new GMMailInfoMsg();
					        ret.mailList.Clear();
					        for (int i = 0; i < mails.Mails.Count; i++)
					        {
						        GMMailData tmp = new GMMailData();
						        tmp.mailId = mails.Mails[i].Id;
						        tmp.time = DateTime.FromBinary(mails.Mails[i].Time).ToString("yyyy/MM/dd HH:mm:ss");
						        tmp.title = mails.Mails[i].Title;
						        tmp.content = mails.Mails[i].Content;
						        tmp.items.Clear();
					            tmp.IsNew = mails.Mails[i].IsNew;
                                tmp.OverTime = DateTime.FromBinary(mails.Mails[i].OverTime).ToString("yyyy/MM/dd HH:mm:ss");
						        foreach (var value in mails.Mails[i].Items.Data)
						        {
							        tmp.items.Add(value.Key, value.Value);
						        }
						        ret.mailList.Add(tmp);
					        }
					        GMResult = JsonConvert.SerializeObject(ret);
					         ReplyResult(ref context, GMResult);
			        }
			        break;
		        case "/GetBroadCastMessage" :
				//case "/DelWaitingBroadCastMessage" :
		        {
			        
				        var Broadcasts = _this.mTimedBroadcasts.Broadcasts;
				        var ret = new GMGMBroadCastDataMsg();
				        ret.BroadCastList.Clear();
				        for (int i = 0; i < Broadcasts.Broadcasts.Count; i++)
				        {
					        GMBroadCastData tmp = new GMBroadCastData();
					        tmp.BroadCastId = Broadcasts.Broadcasts[i].Id;
					        tmp.time = DateTime.FromBinary(Broadcasts.Broadcasts[i].Time).ToString("yyyy/MM/dd HH:mm:ss");
					        ret.BroadCastList.Add(tmp);
				        }

				        GMResult = JsonConvert.SerializeObject(ret);
				        ReplyResult(ref context, GMResult);
		        }
			        break;
		        case "/ReloadOperationActivity" :
		        {
					
					var msg = GameMasterServer.Instance.LogicAgent.ServerGMCommand("ReloadOperationActivity", "");
					yield return msg.SendAndWaitUntilDone(co);

					GMResult = "ReloadOperationActivity OK";
		        }
			        break;
		        case "/UpdateAnchor":
		        {
			     
				    var msg = GameMasterServer.Instance.ChatAgent.ServerGMCommand("UpdateAnchor", "");
				    yield return msg.SendAndWaitUntilDone(co);

					GMResult = "UpdateAnchor OK";
		        }
			    break;
				case "/ReloadAccountList":
				{

					var msg = GameMasterServer.Instance.LoginAgent.ServerGMCommand("ReloadAccountList", "");
					yield return msg.SendAndWaitUntilDone(co);

					GMResult = "ReloadAccountList OK";
					
				}
				break;
				case "/BackupRank":
				{

					var msg = GameMasterServer.Instance.RankAgent.ServerGMCommand("BackupRank", "");
					yield return msg.SendAndWaitUntilDone(co);

					GMResult = "BackupRank OK";
				}
				break;
				default:
		        {
			        ReplyResult(ref context, GMResult);
		        }
				break;
	        }

			 ReplyResult(ref context, GMResult);



	        yield break;
        }

        private static void ReplyResult(ref HttpListenerContext context, string GMResult)
        {
            using (var outputStream = new StreamWriter(context.Response.OutputStream))
            {
                outputStream.Write(GMResult);
                outputStream.Flush();
            }
        }

        private static Coroutine DelBroadCastMessage(Coroutine co, GameMasterServerControl _this, JObject jsonResult)
        {
            if (jsonResult["BroadcastsMessageIds"] == null)
            {
                return null;
            }
            var messages = jsonResult["BroadcastsMessageIds"].ToString();
            var messagesArry = messages.Split(',');
            var messagesList = new List<ulong>();
            foreach (var data in messagesArry)
            {
                messagesList.Add(ulong.Parse(data));
            }

            return DelWaitingBroadcasts(co, _this, messagesList);
        }

        private static Coroutine BroadCastMessage(Coroutine co, GameMasterServerControl _this, JObject jsonResult, AsyncReturnValue<GMSendBroadCastRetMsg> gmResult)
        {
            if (jsonResult["Servers"] == null)
            {
                return null;
            }
            if (jsonResult["Time"] == null)
            {
                return null;
            }
            if (jsonResult["Content"] == null)
            {
                return null;
            }

            var servers = jsonResult["Servers"].ToString(); // List<uint> serverIds, long time, string content
            var serverArry = servers.Split(',');
            var serverList = new List<uint>();
            foreach (var data in serverArry)
            {
                serverList.Add(uint.Parse(data));
            }
            var time = Convert.ToDateTime(jsonResult["Time"].ToString());
            if (time == DateTime.MinValue)
            {
                return null;
            }
            //var time = (long)jsonResult["Time"];
            var content = jsonResult["Content"].ToString();

            var subroutine = CoroutineFactory.NewSubroutine(Broadcast, co, _this, serverList, time.ToBinary(), content, gmResult);
            return subroutine;
        }
        private static Coroutine DelWaitingMails(Coroutine co, GameMasterServerControl _this, JObject jsonResult)
        {
            if (jsonResult["Mails"] == null)
            {
                return null;
            }
            var mails = jsonResult["Mails"].ToString();
            var mailsArry = mails.Split(',');
            var mailsList = new List<ulong>();
            foreach (var data in mailsArry)
            {
                mailsList.Add(ulong.Parse(data));
            }
            return DelWaitingMails(co, _this, mailsList);
        }
        private static Coroutine SendMailByServers(Coroutine co, GameMasterServerControl _this, JObject jsonResult, AsyncReturnValue<GMSendMailRetMsg> gmResult)
        {
            if (jsonResult["Servers"] == null)
            {
                return null;
            }
            if (jsonResult["Time"] == null)
            {
                return null;
            }
            if (jsonResult["Title"] == null)
            {
                return null;
            }
            if (jsonResult["Content"] == null)
            {
                return null;
            }
            if (jsonResult["Items"] == null)
            {
                return null;
            }
            if (jsonResult["OverTime"] == null)
            {
                return null;
            }
            if (jsonResult["IsNew"] == null)
            {
                return null;
            }
            var servers = jsonResult["Servers"].ToString(); // name1,name2
            var serverArry = servers.Split(',');
            var serverList = new List<uint>();
            foreach (var data in serverArry)
            {
                serverList.Add(uint.Parse(data));
            }

            var time = Convert.ToDateTime(jsonResult["Time"].ToString());
            var overTime = Convert.ToDateTime(jsonResult["OverTime"].ToString());
            if (time == DateTime.MinValue || overTime == DateTime.MinValue)
            {
                return null;
            }
            var isNew = int.Parse(jsonResult["IsNew"].ToString());
            var title = jsonResult["Title"].ToString();
            var content = jsonResult["Content"].ToString();

            var itemsDic = new Dictionary<int, int>();
            var items = jsonResult["Items"].ToString(); // 3,1|4,2
            if (!string.IsNullOrEmpty(items))
            {
                var itemsArry = items.Split('|');
                var itemTmp = new List<string>();
                foreach (var data in itemsArry)
                {
                    itemTmp.Add(data);
                }
                
                foreach (var data in itemTmp)
                {
                    var cell = data.Split(',');
                    if (cell.Count() < 2)
                    {
                        continue;
                    }
                    itemsDic.Add(int.Parse(cell[0]), int.Parse(cell[1]));
                }  
            }
            List<long> tl = new List<long>();
            tl.Add(time.ToBinary());
            tl.Add(overTime.ToBinary());
            tl.Add((long)isNew);
            var subroutine = CoroutineFactory.NewSubroutine(SendMailsToServers, co, _this, serverList, tl, title, content, itemsDic, gmResult);
            return subroutine;
        }
        private static Coroutine SendMailById(Coroutine co, GameMasterServerControl _this, JObject jsonResult, AsyncReturnValue<GMSendMailRetMsg> gmResult, int type = 0)
        {
            if (jsonResult["Ids"] == null)
            {
                return null;
            }
            if (jsonResult["Time"] == null)
            {
                return null;
            }
            if (jsonResult["Title"] == null)
            {
                return null;
            }
            if (jsonResult["Content"] == null)
            {
                return null;
            }
            
            var names = jsonResult["Ids"].ToString(); // name1,name2
            var nameArry = names.Split(',');
            var nameList = new List<ulong>();
            foreach (var data in nameArry)
            {
                ulong res = 0;
                if (!ulong.TryParse(data, out res))
                {
                    return null;
                }

                nameList.Add(res);
            }

            var time = Convert.ToDateTime(jsonResult["Time"].ToString());
            if (time == DateTime.MinValue)
            {
                return null;
            }
            var title = jsonResult["Title"].ToString();
            var content = jsonResult["Content"].ToString();

            if (jsonResult["Items"] != null)
            {
                var itemsDic = new Dictionary<int, int>();
                var items = jsonResult["Items"].ToString(); // 3,1|4,2
                if (!string.IsNullOrEmpty(items))
                {
                    var itemsArry = items.Split('|');
                    var itemTmp = new List<string>();
                    foreach (var data in itemsArry)
                    {
                        itemTmp.Add(data);
                    }
                    
                    foreach (var data in itemTmp)
                    {
                        var cell = data.Split(',');
                        if (cell.Count() < 2)
                        {
                            continue;
                        }
                        itemsDic.Add(int.Parse(cell[0]), int.Parse(cell[1]));
                    } 
                }

                if (type == 0)
                {
                    var subroutine = CoroutineFactory.NewSubroutine(SendMailsById, co, _this, nameList, time.ToBinary(), title, content, itemsDic, gmResult);
                    return subroutine;
                }
            }
            else
            {
                if (type == 1)
                {
                    var subroutine = CoroutineFactory.NewSubroutine(SendMailsByIdEx, co, _this, nameList, time.ToBinary(), title, content, gmResult);
                    return subroutine;
                }
                else
                {
                    return null;
                } 
            }
            return null;
        }
        private static Coroutine SendMailByName(Coroutine co, GameMasterServerControl _this, JObject jsonResult, AsyncReturnValue<GMSendMailRetMsg> gmResult)
        {
            if (jsonResult["Names"] == null)
            {
                return null;
            }
            if (jsonResult["Time"] == null)
            {
                return null;
            }
            if (jsonResult["Title"] == null)
            {
                return null;
            }
            if (jsonResult["Content"] == null)
            {
                return null;
            }
            if (jsonResult["Items"] == null)
            {
                return null;
            }

            var names = jsonResult["Names"].ToString(); // name1,name2
            var nameArry = names.Split(',');
            var nameList = new List<string>();
            foreach (var data in nameArry)
            {
                nameList.Add(data);
            }

            var time = Convert.ToDateTime(jsonResult["Time"].ToString());
            if (time == DateTime.MinValue)
            {
                return null;
            }
            var title = jsonResult["Title"].ToString();
            var content = jsonResult["Content"].ToString();

            var itemsDic = new Dictionary<int, int>();
            var items = jsonResult["Items"].ToString(); // 3,1|4,2
            if (!string.IsNullOrEmpty(items))
            {
                var itemsArry = items.Split('|');
                var itemTmp = new List<string>();
                foreach (var data in itemsArry)
                {
                    itemTmp.Add(data);
                }

                foreach (var data in itemTmp)
                {
                    var cell = data.Split(',');
                    if (cell.Count() < 2)
                    {
                        continue;
                    }
                    itemsDic.Add(int.Parse(cell[0]), int.Parse(cell[1]));
                } 
            }

            var subroutine = CoroutineFactory.NewSubroutine(SendMailsByName, co, _this, nameList, time.ToBinary(), title, content, itemsDic, gmResult);
            return subroutine;
        }
    }
}
