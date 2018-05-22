#region using

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using ChatServerService;
using Database;
using DataContract;
using DataTable;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Chat
{
    public class ChatServerControlDefaultImpl : IChatService, ITickable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public IEnumerator OnConnected(Coroutine coroutine, ChatCharacterProxy charProxy, AsyncReturnValue<bool> ret)
        {
            ret.Value = false;
            var proxy = (ChatProxy) charProxy;
            Logger.Info("[{0}] has enter connected", proxy.CharacterId);
            var characterId = proxy.CharacterId;

            var obj = CharacterManager.Instance.GetCharacterControllerFromMemroy(proxy.CharacterId);
            if (obj == null)
            {
                Logger.Fatal("onConnected CharacterId={0}", proxy.CharacterId);
                yield break;
            }

            var dbLoginSimple = ChatServer.Instance.LoginAgent.GetLoginSimpleData(proxy.ClientId, characterId);
            yield return dbLoginSimple.SendAndWaitUntilDone(coroutine);
            if (dbLoginSimple.State != MessageState.Reply || dbLoginSimple.ErrorCode != (int) ErrorCodes.OK)
            {
                yield break;
            }
            proxy.Character = obj;
            obj.ServerId = dbLoginSimple.Response.ServerId;
            obj.Proxy = proxy;
            obj.Name = dbLoginSimple.Response.Name;
            proxy.Connected = true;

            proxy.Character.State = CharacterState.Connected;

            CharacterManager.Instance.UpdateSimpleData(proxy.CharacterId);
            CityChatManager.EnterChannel(1, obj);
            obj.mChat.Online();

            ret.Value = true;
        }

        public IEnumerator NotifyPlayerEnterGame(Coroutine coroutine, ChatService _this, NotifyPlayerEnterGameInMessage msg)
        {
            ChatCharacterController charController = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (charController == null)
            {
                Logger.Fatal("NotifyPlayerEnterGame CharacterId={0}", msg.CharacterId);
                yield break;
            }
            charController.SetMoniterData(msg.Request.Data);
        }
        public IEnumerator AddPlayerToChatMonitor(Coroutine coroutine, ChatService _this, AddPlayerToChatMonitorInMessage msg)
        {
            var charController = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (charController == null || null == charController.moniterData)
            {
                yield break;
            }
            if(charController.moniterData.channel.Equals("37") == false)
                yield break;
            var unixTimer = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            var dic = new Dictionary<string, string>();
            dic.Add("time", Math.Ceiling(unixTimer).ToString());
            dic.Add("uid", charController.moniterData.uid);
            dic.Add("gid", charController.moniterData.gid.ToString());
            dic.Add("dsid", charController.ServerId.ToString());
            dic.Add("memo","Vip");
            dic.Add("actor_name", HttpUtility.UrlEncode(charController.Name));
            dic.Add("actor_id", charController.mGuid.ToString());

            var md5Key = "Ob7mD7HqInhGxTNt";
            var strsign = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}", 
                md5Key, Math.Ceiling(unixTimer), charController.moniterData.uid, charController.moniterData.gid, charController.ServerId, charController.mGuid.ToString(), HttpUtility.UrlEncode(charController.Name), "Vip");//md5Key + "}{" + proxy.Character.moniterData.uid + "}{""}{" + proxy.Character.ServerId.ToString()+ "}{" + unixTimer + "}{" + dic["type"] + "}");
            var sign = RequestManager.Encrypt_MD5_UTF8(strsign);
            dic.Add("sign", sign);

            var url = @"http://cm3.api.37.com.cn/Content/_addPrivateChatTempWhiteList";

            var result = AsyncReturnValue<string>.Create();
            yield return ChatManager.WebRequestManager.DoRequest(coroutine, url, dic, result);

            if (string.IsNullOrEmpty(result.Value))
            {
                Logger.Error("ChatChatMessage get webResponse is null.url{0}", url);
                yield break;
            }

            var jsonObj = (JObject)JsonConvert.DeserializeObject(result.Value);
            var resultCode = int.Parse(jsonObj["state"].ToString());
            if (resultCode != 1)
            {
                Logger.Error("ChatChatMessage get resultCode is error[{0}][{1}]  sign=[{2}]", resultCode, jsonObj["msg"], strsign);
                msg.Reply((int)ErrorCodes.Error_BannedToPost);
                yield break;
            }
            yield break;
        }

        public IEnumerator GetAnchorIsInRoom(Coroutine coroutine, ChatService _this, GetAnchorIsInRoomInMessage msg)
        {
            msg.Response = AnchorManager.Instance.IsInAnchorRooml;
            msg.Reply();
            yield break;
        }

        public IEnumerator NodifyModifyPlayerName(Coroutine coroutine, ChatService _this, NodifyModifyPlayerNameInMessage msg)
        {
            var charController = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.Request.CharacterId);
            if (charController != null)
            {
                charController.Name = msg.Request.ModifyName;
            }
           
            ChatCharacterProxy proxy;
            if (_this.Proxys.TryGetValue(msg.Request.CharacterId, out proxy))
            {
                var chatproxy = (ChatProxy)proxy;
                chatproxy.Character.Name = msg.Request.ModifyName;
            }
            yield break;
        }
        public IEnumerator AddRecentcontacts(Coroutine coroutine, ChatService _this, AddRecentcontactsInMessage msg)
        {
            var charController = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (charController != null)
            {
                charController.PushChat(msg.Request.SimpleData);
            }
            yield break;
        }

        //缓存私聊
        public IEnumerator CacheChatMessage(Coroutine coroutine, ChatService _this, CacheChatMessageInMessage msg)
        {
            var charController = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (charController != null)
            {
                ChatManager.ToClinetMessage(msg.CharacterId, (int) eChatChannel.MyWhisper, msg.Request.CharacterId,
                    msg.Request.CharacterName, msg.Request.Content);
                yield break;
            }

            var request = msg.Request;
            var content = request.Content;
            var savemsg = new DBChatMsg
            {
                FromId = request.CharacterId,
                Type = request.ChatType,
                Content = content.Content,
                ToId = msg.CharacterId,
                Name = request.CharacterName,
                SoundData = content.SoundData,
                Vip = content.Vip
            };
            //缓存私聊
            CharacterManager.Instance.ModifyVolatileData(msg.CharacterId, DataCategory.ChatCharacter, oldData =>
            {
                oldData.Chats.Add(savemsg);
                if (oldData.Chats.Count > 50)
                {
                    oldData.Chats.RemoveAt(0);
                }
                return oldData;
            });
        }

        public IEnumerator SSBroadcastAllServerMsg(Coroutine coroutine,
            ChatService _this,
            SSBroadcastAllServerMsgInMessage msg)
        {
            ChatManager.BroadcastAllServerMessage(msg.Request.ChatType, msg.CharacterId, "", msg.Request.Content);
            msg.Reply();
            yield break;
        }

        public IEnumerator SSGetCurrentAnchor(Coroutine coroutine, ChatService _this, SSGetCurrentAnchorInMessage msg)
        {
            msg.Response = AnchorManager.Instance.GetCurrentAnchor();
            msg.Reply();
            yield break;
        }

        public IEnumerator SSNotifyCharacterOnConnet(Coroutine coroutine,
                                                     ChatService _this,
                                                     SSNotifyCharacterOnConnetInMessage msg)
        {
            var characterId = msg.Request.CharacterId;
            var clientId = msg.Request.ClientId;
            var proxy = new ChatProxy(_this, characterId, clientId);

            _this.Proxys[characterId] = proxy;

            var ret = AsyncReturnValue<bool>.Create();
            var subCo = CoroutineFactory.NewSubroutine(OnConnected, coroutine, proxy, ret);
            if (subCo.MoveNext())
            {
                yield return subCo;
            }
            var isOk = ret.Value;
            ret.Dispose();
            if (isOk)
            {
                msg.Reply((int) ErrorCodes.OK);
                AnchorManager.Instance.CharOnline(characterId, proxy.Character.Name);
            }
            else
            {
                msg.Reply((int) ErrorCodes.ConnectFail);
            }
        }

        public IEnumerator BSNotifyCharacterOnLost(Coroutine coroutine,
                                                   ChatService _this,
                                                   BSNotifyCharacterOnLostInMessage msg)
        {
            var characterId = msg.Request.CharacterId;
            ChatCharacterProxy charProxy;
            if (!_this.Proxys.TryGetValue(characterId, out charProxy))
            {
                yield break;
            }
            var proxy = (ChatProxy) charProxy;

            if (proxy.Character != null)
            {
                proxy.Character.Proxy = null;
            }
            var co = CoroutineFactory.NewSubroutine(CharacterManager.Instance.RemoveCharacter, coroutine,
                proxy.CharacterId);
            if (co.MoveNext())
            {
                yield return co;
            }
            AnchorManager.Instance.CharOffline(characterId);
            proxy.Connected = false;
        }

        public IEnumerator UpdateServer(Coroutine coroutine, ChatService _this, UpdateServerInMessage msg)
        {
            ChatServer.Instance.UpdateManager.Update();
            return null;
        }

        public IEnumerator GMCommand(Coroutine co, ChatService _this, GMCommandInMessage msg)
        {
            var cmds = msg.Request.Commonds.Items;
            var errs = msg.Response.Items;
            var err = new AsyncReturnValue<ErrorCodes>();
            foreach (var cmd in cmds)
            {
                var co1 = CoroutineFactory.NewSubroutine(GameMaster.GmCommand, co, cmd, err);
                if (co1.MoveNext())
                {
                    yield return co1;
                }
                errs.Add((int) err.Value);
            }
            err.Dispose();
            msg.Reply();
        }

        public IEnumerator CloneCharacterDbById(Coroutine coroutine, ChatService _this, CloneCharacterDbByIdInMessage msg)
        {
            msg.Reply((int)ErrorCodes.OK);
            yield break;
        }

        public IEnumerator OnServerStart(Coroutine coroutine, ChatService _this)
        {
            //Thread.Sleep(GlobalVariable.WaitToConnectTimespan);
            ChatServer.Instance.Start(_this);
            ChatManager.Init();
            var __this = (ChatServerControl)_this;
            ChatManager.WebRequestManager = new RequestManager(__this);
            CityChatManager.Init();
            CharacterManager.Instance.Init(ChatServer.Instance.DB, DataCategory.ChatCharacter);
            AnchorManager.Instance.Init();
            ChatServer.Instance.IsReadyToEnter = true;

            _this.TickDuration = 0.5f;

            _this.Started = true;

            Console.WriteLine("ChatServer startOver. [{0}]", ChatServer.Instance.Id);
            yield break;
        }

        public IEnumerator Tick(Coroutine co, ServerAgentBase server)
        {
            try
            {
                ChatServerControl.tm.Update();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Tick error.");
            }
	        try
	        {
		        ChatServerMonitor.TickRate.Mark();
	        }
			catch (Exception)
			{
				
			}

            return null;
        }

        public IEnumerator OnServerStop(Coroutine coroutine, ChatService _this)
        {
            ChatManager.WebRequestManager.Stop();
            var co = CoroutineFactory.NewSubroutine(CharacterManager.Instance.SaveAllCharacter, coroutine,
                default(TimeSpan));
            if (co.MoveNext())
            {
                yield return co;
            }
            ChatServer.Instance.DB.Dispose();
        }

        public IEnumerator PrepareDataForEnterGame(Coroutine coroutine,
                                                   ChatService _this,
                                                   PrepareDataForEnterGameInMessage msg)
        {
            Logger.Info("Enter Game {0} - PrepareDataForEnterGame - 1 - {1}", msg.CharacterId,
                TimeManager.Timer.ElapsedMilliseconds);
            //var result = AsyncReturnValue<CharacterController>.Create();
            //var co1 =CoroutineFactory.NewSubroutine(CharacterManager.Instance.GetOrCreateCharacterController, co,msg.CharacterId, new object[] { }, false, result);
            //if (co1.MoveNext())
            //{
            //    yield return co1;
            //}

            //if (result.Value == null)
            //{
            //    msg.Reply((int)ErrorCodes.Error_PrepareEnterGameFailed);
            //    yield break;
            //}

            var characterId = msg.CharacterId;
            var result = AsyncReturnValue<ChatCharacterController>.Create();
            var co = CoroutineFactory.NewSubroutine(CharacterManager.Instance.GetOrCreateCharacterController, coroutine,
                characterId, new object[] {}, false, result);
            if (co.MoveNext())
            {
                yield return co;
            }
            var obj = result.Value;
            result.Dispose();
            if (obj == null)
            {
                msg.Reply((int) ErrorCodes.Error_PrepareEnterGameFailed);
                yield break;
            }
            msg.Reply();
        }

        public IEnumerator PrepareDataForCreateCharacter(Coroutine coroutine,
                                                         ChatService _this,
                                                         PrepareDataForCreateCharacterInMessage msg)
        {
            PlayerLog.WriteLog(msg.CharacterId, "----------PrepareDataForCreateCharacter----------{0}", msg.CharacterId);

            var result = AsyncReturnValue<ChatCharacterController>.Create();
            var co = CoroutineFactory.NewSubroutine(CharacterManager.Instance.CreateCharacterController, coroutine,
                msg.CharacterId, result,
                new object[] {msg.Request.Type});

            if (co.MoveNext())
            {
                yield return co;
            }
            var retValue = result.Value;
            result.Dispose();
            if (retValue == null)
            {
                msg.Reply((int) ErrorCodes.Error_PrepareEnterGameFailed);
                yield break;
            }
            msg.Reply();
        }

        public IEnumerator PrepareDataForCommonUse(Coroutine coroutine,
                                                   ChatService _this,
                                                   PrepareDataForCommonUseInMessage msg)
        {
            msg.Reply();
            return null;
        }

        public IEnumerator PrepareDataForLogout(Coroutine coroutine,
                                                ChatService _this,
                                                PrepareDataForLogoutInMessage msg)
        {
            msg.Reply();
            return null;
        }

        public IEnumerator CreateCharacter(Coroutine coroutine, ChatService _this, CreateCharacterInMessage msg)
        {
            var characterId = msg.CharacterId;
            PlayerLog.WriteLog(characterId, "----------PrepareDataForCreateCharacter----------{0}", characterId);

            var result = AsyncReturnValue<ChatCharacterController>.Create();
            var co = CoroutineFactory.NewSubroutine(CharacterManager.Instance.CreateCharacterController, coroutine,
                characterId, result,
                new object[] {msg.Request.Type});

            if (co.MoveNext())
            {
                yield return co;
            }
            var retValue = result.Value;
            result.Dispose();
            if (retValue == null)
            {
                msg.Reply((int) ErrorCodes.Error_PrepareEnterGameFailed);
                yield break;
            }

            var co1 = CoroutineFactory.NewSubroutine(CharacterManager.Instance.RemoveCharacter, coroutine, characterId);
            if (co1.MoveNext())
            {
                yield return co1;
            }

            msg.Reply();
        }

        public IEnumerator DelectCharacter(Coroutine coroutine, ChatService _this, DelectCharacterInMessage msg)
        {
            var characterId = msg.CharacterId;
            PlayerLog.WriteLog(characterId, "----Chat------DelectCharacter----------{0}", characterId);
            var co1 = CoroutineFactory.NewSubroutine(CharacterManager.Instance.DeleteCharacter, coroutine, characterId);
            if (co1.MoveNext())
            {
                yield return co1;
            }
            msg.Reply();
        }

        public IEnumerator CheckConnected(Coroutine coroutine, ChatService _this, CheckConnectedInMessage msg)
        {
            Logger.Error("Chat CheckConnected, {0}", msg.CharacterId);

            //ChatCharacterProxy proxy = null;
            //if (_this.Proxys.TryGetValue(msg.CharacterId, out proxy))
            //{
            //    if ((proxy as ChatProxy).Connected)
            //    {
            //        msg.Response = 1;
            //        msg.Reply();
            //        return null;
            //    }

            //    (proxy as ChatProxy).WaitingCheckConnectedInMessages.Add(msg);
            //}

            return null;
        }

        public IEnumerator CheckLost(Coroutine coroutine, ChatService _this, CheckLostInMessage msg)
        {
            Logger.Error("Chat CheckLost, {0}", msg.CharacterId);

            //ChatCharacterProxy proxy = null;
            //if (!_this.Proxys.TryGetValue(msg.CharacterId, out proxy))
            //{
            //    msg.Reply();
            //}
            //else
            //{
            //    if ((proxy as ChatProxy).Connected)
            //    {
            //        (proxy as ChatProxy).WaitingCheckLostInMessages.Add(msg);
            //    }
            //    else
            //    {
            //        msg.Reply();
            //    }
            //}

            return null;
        }

        public IEnumerator QueryStatus(Coroutine coroutine, ChatService _this, QueryStatusInMessage msg)
        {
            var common = new ServerCommonStatus();
            common.Id = ChatServer.Instance.Id;
            common.ByteReceivedPerSecond = _this.ByteReceivedPerSecond;
            common.ByteSendPerSecond = _this.ByteSendPerSecond;
            common.MessageReceivedPerSecond = _this.MessageReceivedPerSecond;
            common.MessageSendPerSecond = _this.MessageSendPerSecond;
            common.ConnectionCount = _this.ConnectionCount;

            msg.Response.CommonStatus = common;

            msg.Response.ConnectionInfo.AddRange(ChatServer.Instance.Agents.Select(kv =>
            {
                var conn = new ConnectionStatus();
                var item = kv.Value;
                conn.ByteReceivedPerSecond = item.ByteReceivedPerSecond;
                conn.ByteSendPerSecond = item.ByteSendPerSecond;
                conn.MessageReceivedPerSecond = item.MessageReceivedPerSecond;
                conn.MessageSendPerSecond = item.MessageSendPerSecond;
                conn.Target = item.Id;
                conn.Latency = item.Latency;

                return conn;
            }));

            msg.Reply();

            yield break;
        }

		public IEnumerator ServerGMCommand(Coroutine coroutine, ChatService _this, ServerGMCommandInMessage msg)
        {
			var cmd = msg.Request.Cmd;
			var param = msg.Request.Param;

			Logger.Info("Chat----------ServerGMCommand----------cmd={0}|param={1}", cmd, param);

			try
			{
				if ("ReloadTable" == cmd)
				{
					Table.ReloadTable(param);
				}
				else if ("UpdateAnchor" == cmd)
				{
					AnchorManager.Instance.LoadConfig();
				}
			}
			catch (Exception e)
			{
				Logger.Error("Chat----------ServerGMCommand----------error={0}", e.Message);
			}
			finally
			{
				
			}
			yield break;
        }

        public IEnumerator ReadyToEnter(Coroutine coroutine, ChatService _this, ReadyToEnterInMessage msg)
        {
            if (ChatServer.Instance.IsReadyToEnter && ChatServer.Instance.AllAgentConnected())
            {
                msg.Response = 1;
            }
            else
            {
                msg.Response = 0;
            }

            msg.Reply();

            return null;
        }

        public IEnumerator Silence(Coroutine co, ChatService _this, SilenceInMessage msg)
        {
            var id = msg.CharacterId;
            var mask = msg.Request.Mask;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(id);
            if (character == null)
            {
                var dbChat = ChatServer.Instance.DB.Get<DBCharacterChat>(co, DataCategory.ChatCharacter, id);
                yield return dbChat;
                dbChat.Data.BannedToPost = mask;
                var dbSet = ChatServer.Instance.DB.Set(co, DataCategory.ChatCharacter, id, dbChat.Data);
                yield return dbSet;
            }
            else
            {
                character.mDbData.BannedToPost = mask;
                character.MarkDbDirty();
            }
            msg.Reply();
        }

        public IEnumerator GetSilenceState(Coroutine co, ChatService _this, GetSilenceStateInMessage msg)
        {
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (character == null)
            {
                var dbChat = ChatServer.Instance.DB.Get<DBCharacterChat>(co, DataCategory.ChatCharacter,
                    msg.CharacterId);
                yield return dbChat;
                msg.Response = dbChat.Data.BannedToPost;
            }
            else
            {
                msg.Response = character.mDbData.BannedToPost;
            }
            msg.Reply();
        }
    }

    public class ChatServerControl : ChatService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static TimeManager tm = new TimeManager();
        private long tickTime = 0;

        public ChatServerControl()
        {
            ChatServer.Instance.UpdateManager.InitStaticImpl(typeof (ChatServerControl),
                typeof (ChatServerControlDefaultImpl),
                o => { SetServiceImpl((IChatService) o); });
            ChatServer.Instance.UpdateManager.InitStaticImpl(typeof (ChatProxy), typeof (ChatProxyDefaultImpl),
                o => { SetProxyImpl((IChatCharacterProxy) o); });
        }

        public override ChatCharacterProxy NewCharacterIn(ulong characterId, ulong clientId)
        {
            return new ChatProxy(this, characterId, clientId);
        }

        public override void OnException(Exception ex)
        {
            Logger.Error(ex, "Network error");
        }

        public override IEnumerator OnServerStart(Coroutine coroutine)
        {
            return mImpl.OnServerStart(coroutine, this);
        }

        public override IEnumerator OnServerStop(Coroutine coroutine)
        {
            return mImpl.OnServerStop(coroutine, this);
        }

        public override IEnumerator PerformenceTest(Coroutine coroutine, ServerClient client, ServiceDesc desc)
        {
            client.SendMessage(desc);
            yield break;
        }

        public void Status(ConcurrentDictionary<string, string> dict)
        {
            try
            {
                dict.TryAdd("_Listening", Listening.ToString());
                dict.TryAdd("Started", Started.ToString());
                dict.TryAdd("TickTime", tickTime.ToString());
                //dict.TryAdd("ByteReceivedPerSecond", ByteReceivedPerSecond.ToString());
                //dict.TryAdd("ByteSendPerSecond", ByteSendPerSecond.ToString());
                //dict.TryAdd("MessageReceivedPerSecond", MessageReceivedPerSecond.ToString());
                //dict.TryAdd("MessageSendPerSecond", MessageSendPerSecond.ToString());
                //dict.TryAdd("ConnectionCount", ConnectionCount.ToString());
                //dict.TryAdd("WaitingReplyMessage", OutMessage.WaitingMessageCount.ToString());

                //foreach (var agent in ChatServer.Instance.Agents.ToArray())
                //{
                //    dict.TryAdd(agent.Key + " Latency", agent.Value.Latency.ToString());
                //    dict.TryAdd(agent.Key + " ByteReceivedPerSecond", agent.Value.ByteReceivedPerSecond.ToString());
                //    dict.TryAdd(agent.Key + " ByteSendPerSecond", agent.Value.ByteSendPerSecond.ToString());
                //    dict.TryAdd(agent.Key + " MessageReceivedPerSecond", agent.Value.MessageReceivedPerSecond.ToString());
                //    dict.TryAdd(agent.Key + " MessageSendPerSecond", agent.Value.MessageSendPerSecond.ToString());
                //}
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "ChatServerControl Status Error!{0}");
            }
        }

        public override IEnumerator Tick(Coroutine coroutine)
        {
            tickTime++;
            return ((ITickable) mImpl).Tick(coroutine, this);
        }
    }
}