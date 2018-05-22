#region using


using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using ChatServerService;
using Database;
using DataContract;
using DataTable;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scorpion;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
using NLog;
using SceneClientService;
using System.Threading.Tasks;
using Shared;

#endregion

namespace Chat
{
    public class ChatProxyDefaultImpl : IChatCharacterProxy
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Logger kafaLogger = LogManager.GetLogger(Shared.LoggerName.KafkaLog);

        public IEnumerator GMChat(Coroutine coroutine, ChatCharacterProxy charProxy, GMChatInMessage msg)
        {
            var proxy = (ChatProxy) charProxy;
            var command = msg.Request.Commond;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Chat----------GMChat----------:{0}", command);

            var err = new AsyncReturnValue<ErrorCodes>();
            var co1 = CoroutineFactory.NewSubroutine(GameMaster.GmCommand, coroutine, command, err);
            if (co1.MoveNext())
            {
                yield return co1;
            }
            msg.Reply((int) err.Value);
            err.Dispose();
        }

        public IEnumerator PresentGift(Coroutine coroutine, ChatCharacterProxy charProxy, PresentGiftInMessage msg)
        {
            var proxy = (ChatProxy)charProxy;
            var character = proxy.Character;
            var itemId = msg.Request.ItemId;
            var count = msg.Request.Count;
            var characterId = msg.CharacterId;
            PlayerLog.WriteLog(proxy.CharacterId,
                "----------Logic----------PresentGift------itemid={0} count={1}",
                msg.Request.ItemId, msg.Request.Count);

            // 主播是否在
            var curAnchor = AnchorManager.Instance.GetCurrentAnchor();
            if (curAnchor == 0)
            {
                msg.Reply((int)ErrorCodes.Error_AnchorNotInRoom);
                yield break;
            }

            // 检查物品
            if (count <= 0)
            {
                msg.Reply((int)ErrorCodes.Error_ItemNotFind);
                yield break;
            }

            var giftMsg = ChatServer.Instance.LogicAgent.AnchorGift(proxy.CharacterId, itemId, count);
            yield return giftMsg.SendAndWaitUntilDone(coroutine);
            if (giftMsg.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply(giftMsg.ErrorCode);
                yield break;
            }

            // 主播获得鲜花
            var anchorChar = CharacterManager.Instance.GetCharacterControllerFromMemroy(curAnchor);
            if (anchorChar != null)
            {
                var data = new Dict_int_int_Data();
                data.Data.Add((int)eExdataDefine.e626, count);
                var dataMsg = ChatServer.Instance.LogicAgent.SSChangeExdata(curAnchor, data);
                yield return dataMsg.SendAndWaitUntilDone(coroutine);
            }

            // 广播
            var tbSkillUpgrading = Table.GetSkillUpgrading(73001);
            var dictId = tbSkillUpgrading.GetSkillUpgradingValue(count);
            if (dictId <= 0)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            var vipLevel = 0;
            var vipMsg = ChatServer.Instance.LogicAgent.GetItemCount(proxy.CharacterId, (int)eResourcesType.VipLevel);
            yield return vipMsg.SendAndWaitUntilDone(coroutine);
            if (vipMsg.State == MessageState.Reply && vipMsg.ErrorCode == (int)ErrorCodes.OK)
            {
                vipLevel = vipMsg.Response;
            }

            var strs = new List<string>();
            strs.Add("");
            strs.Add(count.ToString());
            var content = Utils.WrapDictionaryId(dictId, strs);
            ChatManager.BroadcastAllServerMessage((int)eChatChannel.Anchor,
                characterId, character.Name, new ChatMessageContent { Content = content, Vip = vipLevel, Ladder = -1 });//临时使用Ladder字段标记广播事件
            msg.Response = 1;
            msg.Reply();
            
            if (count == 999)
            {
                var strss = new List<string>();
                strss.Add(character.Name);                
                strss.Add(count.ToString());
                var content1 = Utils.WrapDictionaryId(273005, strss);
                charProxy.BroadcastWorldMessage((uint)proxy.Character.ServerId, (int)eChatChannel.SystemScroll,
                    proxy.Character.mGuid, character.Name, new ChatMessageContent { Content = content1, Ladder = -1 });
                ChatServer.Instance.ServerControl.NotifyChatRoseEffectChange(count);
            }            
        }

        //主播进入直播间
        public IEnumerator NotifyAnchorEnterRoomChange(Coroutine coroutine, ChatCharacterProxy charProxy, NotifyAnchorEnterRoomChangeInMessage msg)
        {
            if (null != charProxy)
            {
                try
                {
                    var proxy = (ChatProxy)charProxy;
                    PlayerLog.WriteLog(proxy.CharacterId, "----------Chat----------AnchorEnterRoom----------");
                    
                    AnchorManager.Instance.AnchorEnterRoom(proxy.CharacterId, proxy.Character.Name);
                    ChatServer.Instance.ServerControl.BroadcastAnchorEnterRoom(AnchorManager.Instance.GetCurrentAnchorName());                    
                    msg.Reply((int)ErrorCodes.OK);
                }
                catch(Exception e)
                {
                    Logger.Error(e.ToString());
                }

            }
            yield break;
        }

        public IEnumerator AnchorExitRoom(Coroutine coroutine, ChatCharacterProxy _this, AnchorExitRoomInMessage msg)
        {                        
            var proxy = (ChatProxy)_this;

            AnchorManager.Instance.AnchorExitRoom(proxy.CharacterId);

            yield break;
        }

        //void Async2()
        //{

        //        var l = new KeyValuePair<string, string>("1111", "2222");
        //        var listt = new List<KeyValuePair<string, string>>();
        //        listt.Add(l);
        //        GetPageSizeAsync("http://127.0.0.1:8888", listt);
        //}
        [Updateable("ChatProxy")]
        readonly HttpClient client = new HttpClient();

        private void GetPageSizeAsync(string url, List<KeyValuePair<string, string>> param)
        {
            Task.Run(() =>
            {
                var content = new FormUrlEncodedContent(param);
                //client.Timeout = TimeSpan.FromSeconds(10);
                client.PostAsync(url, content);    
            });
        }

        public IEnumerator ChatChatMessage(Coroutine coroutine,
                                           ChatCharacterProxy charProxy,
                                           ChatChatMessageInMessage msg)
        {

            var proxy = (ChatProxy) charProxy;
            var content = msg.Request.Content;
            var type = msg.Request.ChatType;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Chat----------ChatChatMessage----------:{0},{1}",
                content.Content, type);

            ChatCharacterController tochar = null;//
            bool bNormal = content.Content.IndexOf("{!") ==-1 && content.Content.IndexOf("!}") == -1;

            LogicSimpleData fromData = null;
            LogicSimpleData toData   = null;
            {//发送者信息
                var msgGetFrom = ChatServer.Instance.LogicAgent.GetLogicSimpleData(proxy.CharacterId, 0);
                yield return msgGetFrom.SendAndWaitUntilDone(coroutine);
                if (msgGetFrom.State != MessageState.Reply)
                {
                    msg.Reply((int)ErrorCodes.Unknow);
                    yield break;
                }
                if (msgGetFrom.ErrorCode != (int)ErrorCodes.OK)
                {
                    msg.Reply(msgGetFrom.ErrorCode);
                    yield break;
                }
                fromData = msgGetFrom.Response;
                if (fromData == null)
                {
                    msg.Reply((int)ErrorCodes.Unknow);     
                    yield break;
                }
            }
            string newContent = "";
            var newContentSound = new ChatMessageContent();
            #region 私聊之前
            if (type == (int)eChatChannel.Whisper)
            {//接受者信息
                //私聊

                
                var name = ChatManager.GetChatName(content.Content, out newContent);
                newContentSound.Content = newContent;
                newContentSound.SoundData = content.SoundData;
                newContentSound.Vip = content.Vip;
                newContentSound.Ladder = fromData.Ladder;
                if (newContent.Length < 1)
                {
                    msg.Reply((int)ErrorCodes.Error_ChatNone);
                    yield break;
                }
                if (name.Length < 1)
                {
                    msg.Reply((int)ErrorCodes.Error_WhisperNameNone);
                    yield break;
                }
                if (ChatManager.IsRobot(name))
                {
                    msg.Reply((int)ErrorCodes.Error_CharacterOutLine);
                    yield break;
                }
                var result = ChatServer.Instance.LoginAgent.GetCharacterIdByName(proxy.ClientId, name);
                yield return result.SendAndWaitUntilDone(coroutine);
                if (result.State != MessageState.Reply)
                {
                    msg.Reply((int)ErrorCodes.Unknow);
                    yield break;
                }
                var toCharacterId = msg.CharacterId;
                if (result.ErrorCode == (int)ErrorCodes.OK)
                {
                    toCharacterId = result.Response;
                }
                else
                {
                    msg.Reply(result.ErrorCode);
                    yield break;
                }
                if (toCharacterId == msg.CharacterId)
                {
                    msg.Reply((int)ErrorCodes.Error_NotWhisperSelf);
                    yield break;
                }
                var msgGetTo = ChatServer.Instance.LogicAgent.GetLogicSimpleData(toCharacterId, 0);
                yield return msgGetTo.SendAndWaitUntilDone(coroutine);
                if (msgGetTo.State != MessageState.Reply)
                {
                    msg.Reply((int)ErrorCodes.Unknow);
                    yield break;
                }
                if (msgGetTo.ErrorCode != (int)ErrorCodes.OK)
                {
                    msg.Reply(msgGetTo.ErrorCode);
                    yield break;
                }
                toData = msgGetTo.Response;
                tochar = CharacterManager.Instance.GetCharacterControllerFromMemroy(toCharacterId);
                if (tochar == null || toData == null)
                {
                    msg.Reply((int)ErrorCodes.Unline);
                    yield break;
                }

            }
            #endregion
          
            try
			{
				//ChatServerMonitor.ChatRate.Mark();
			}
			catch (Exception)
			{

			}

            if (proxy.Character.mDbData.BannedToPost != 0)
            {
//被禁言了
                msg.Reply((int) ErrorCodes.Error_BannedToPost);
                yield break;
            }

            if (!StringExtension.CheckChatStr(content.Content))
            {
                msg.Reply((int) ErrorCodes.Error_ChatLengthMax);
                yield break;
            }
            #region 37监测
            if (proxy.Character.moniterData.channel.Equals("37") && bNormal == true)
            {
                #region cm3
                {//CM3
                    var unixTimer = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
                    var dic = new Dictionary<string, string>();
                    dic.Add("time", Math.Ceiling(unixTimer).ToString());
                    dic.Add("uid", proxy.Character.moniterData.uid);
                    dic.Add("gid", proxy.Character.moniterData.gid.ToString());
                    dic.Add("dsid", proxy.Character.ServerId.ToString());
                    var md5Key = "Ob7mD7HqInhGxTNt";
                    {
                        switch (type)
                        {
                            case (int)eChatChannel.World:
                                dic.Add("type", "1");
                                break;
                            case (int)eChatChannel.Team:
                                dic.Add("type", "3");
                                break;
                            case (int)eChatChannel.Guild:
                                dic.Add("type", "2");
                                break;
                            case (int)eChatChannel.Whisper:
                                {
                                    dic.Add("type", "5");
                                    {//发送者信息
                                        dic.Add("actor_level", fromData.Level.ToString());
                                        if (fromData.Exdatas.ContainsKey(78))
                                        {
                                            dic.Add("actor_recharge_gold", fromData.Exdatas[78].ToString());
                                        }
                                    }
                                    {//接受者信息
                                        dic.Add("pid", tochar.moniterData.pid);
                                        dic.Add("to_actor_level", toData.Level.ToString());
                                        dic.Add("to_uid", tochar.moniterData.uid);
                                        dic.Add("to_actor_name", toData.Name);
                                        dic.Add("to_actor_id", toData.Id.ToString());
                                        if (toData.Exdatas.ContainsKey(78))
                                        {
                                            dic.Add("to_actor_recharge_gold", fromData.Exdatas[78].ToString());
                                        }
                                    }
                                }
                                break;
                            case (int)eChatChannel.Scene:
                                dic.Add("type", "6");
                                break;
                            case (int)eChatChannel.City:
                                dic.Add("type", "4");
                                break;
                            case (int)eChatChannel.Horn:
                                dic.Add("type", "7");
                                break;
                            case (int)eChatChannel.Anchor:
                                dic.Add("type", "9");
                                break;
                            case (int)eChatChannel.TeamWorld:
                                dic.Add("type", "11");
                                break;
                            default:
                                dic.Add("type", "8");
                                break;
                        }

                    }
                    var strsign = string.Format("{0}{1}{2}{3}{4}{5}", md5Key, proxy.Character.moniterData.uid, proxy.Character.moniterData.gid, proxy.Character.ServerId, Math.Ceiling(unixTimer), dic["type"]);//md5Key + "}{" + proxy.Character.moniterData.uid + "}{""}{" + proxy.Character.ServerId.ToString()+ "}{" + unixTimer + "}{" + dic["type"] + "}");
                    var sign = RequestManager.Encrypt_MD5_UTF8(strsign);
                    dic.Add("sign", sign);
                    dic.Add("actor_name", HttpUtility.UrlEncode(proxy.Character.Name));
                    dic.Add("actor_id", proxy.Character.mGuid.ToString());
                    dic.Add("chat_time", Math.Ceiling(unixTimer).ToString());
                    dic.Add("content", HttpUtility.UrlEncode(msg.Request.Content.Content));


                    var url = @"http://cm3.api.37.com.cn/Content/_checkContent";
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
                }
                #endregion
                #region CM2
                {//CM2
                    var unixTimer = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
                    var strTimer = Math.Ceiling(unixTimer).ToString();
                    //var dic = new Dictionary<string, string>();
                    //dic.Add("time", Math.Ceiling(unixTimer).ToString());
                    //dic.Add("chat_time", Math.Ceiling(unixTimer).ToString());
                    //dic.Add("uid", proxy.Character.moniterData.uid);
                    //dic.Add("gid", proxy.Character.moniterData.gid.ToString());
                    //dic.Add("dsid", HttpUtility.UrlEncode(proxy.Character.ServerId.ToString()));
                    //dic.Add("actor_name", HttpUtility.UrlEncode(proxy.Character.Name));
                    //dic.Add("actor_id", proxy.Character.mGuid.ToString());
                    var chatType = 8;
                    switch (type)
                    {
                        case (int)eChatChannel.World:
                            //dic.Add("type", "1");
                            chatType = 1;
                            break;
                        case (int)eChatChannel.Team:
                            //dic.Add("type", "3");
                            chatType = 3;
                            break;
                        case (int)eChatChannel.Guild:
                            //dic.Add("type", "2");
                            chatType = 2;
                            break;
                        case (int)eChatChannel.Whisper:
                            //dic.Add("type", "5");
                            chatType = 5;
                            break;
                        case (int)eChatChannel.Scene:
                            //dic.Add("type", "6");
                            chatType = 6;
                            break;
                        case (int)eChatChannel.City:
                            //dic.Add("type", "4");
                            chatType = 4;
                            break;
                        case (int)eChatChannel.Horn:
                            //dic.Add("type", "7");
                            chatType = 7;
                            break;
                        case (int)eChatChannel.Anchor:
                            //dic.Add("type", "9");
                            chatType = 9;
                            break;
                        case (int)eChatChannel.TeamWorld:
                            //dic.Add("type", "9");
                            chatType = 11;
                            break;
                        default:
                            chatType = 8;
                            //dic.Add("type", "8");
                            break;
                    }
                    //dic.Add("content", HttpUtility.UrlEncode(msg.Request.Content.Content));
                    //if (tochar != null)
                    //{//接受者信息
                    //    dic.Add("to_uid", tochar.moniterData.uid);
                    //    dic.Add("to_actor_name", HttpUtility.UrlEncode(toData.Name));
                    //}
                    //else
                    //{
                    //    dic.Add("to_uid", "");
                    //    dic.Add("to_actor_name", "");
                    //}
                    //dic.Add("user_ip", proxy.Character.moniterData.ip);
                    //dic.Add("idfa", "");
                    //dic.Add("idfv", "");
                    //dic.Add("mac", "");
                    //dic.Add("imei", "");



                    //var url = @"http://cm2.api.37.com.cn";
                    //var wReq = (HttpWebRequest)WebRequest.Create(url);

                    //wReq.KeepAlive = false;
                    //wReq.Proxy = null;

                    //RequestManager.SetPostData(wReq, dic);
                    //wReq.ContentType = "application/x-www-form-urlencoded";
                    //yield return ChatManager.WebRequestManager.DoRequest(coroutine, wReq);
                    //if (wReq != null)
                    //{
                    //    wReq.Abort();
                    //}
                    List<KeyValuePair<string, string>> param = new List<KeyValuePair<string, string>>();
                    param.Add(new KeyValuePair<string, string>("time", strTimer));
                    param.Add(new KeyValuePair<string, string>("uid", proxy.Character.moniterData.uid));
                    param.Add(new KeyValuePair<string, string>("gid", proxy.Character.moniterData.gid.ToString()));
                    param.Add(new KeyValuePair<string, string>("dsid", HttpUtility.UrlEncode(proxy.Character.ServerId.ToString())));
                    param.Add(new KeyValuePair<string, string>("type", chatType.ToString()));
                    param.Add(new KeyValuePair<string, string>("actor_name", HttpUtility.UrlEncode(proxy.Character.Name)));
                    param.Add(new KeyValuePair<string, string>("actor_id", proxy.Character.mGuid.ToString()));
                    if (tochar != null)
                    {
                        param.Add(new KeyValuePair<string, string>("to_uid", tochar.mGuid.ToString()));
                        param.Add(new KeyValuePair<string, string>("to_actor_name", HttpUtility.UrlEncode(tochar.Name)));

                    }
                    else
                    {
                        param.Add(new KeyValuePair<string, string>("to_uid", ""));
                        param.Add(new KeyValuePair<string, string>("to_actor_name", ""));
                    }
                    param.Add(new KeyValuePair<string, string>("content", HttpUtility.UrlEncode(msg.Request.Content.Content)));
                    param.Add(new KeyValuePair<string, string>("chat_time", strTimer));
                    param.Add(new KeyValuePair<string, string>("user_ip", proxy.Character.moniterData.ip));
                    param.Add(new KeyValuePair<string, string>("idfa", ""));
                    param.Add(new KeyValuePair<string, string>("idfv", ""));
                    param.Add(new KeyValuePair<string, string>("mac", ""));
                    param.Add(new KeyValuePair<string, string>("imei", ""));

                    const string cm2Url = "http://cm2.api.37.com.cn";
                    // const string cm2Url = "http://127.0.0.1:8888";
                    GetPageSizeAsync(cm2Url, param);
                }
                #endregion

            }
            #endregion
            content.Vip = fromData.Vip;
            #region 私聊
            if (type == (int) eChatChannel.Whisper)
            {
                var arr = new Uint64Array();
                arr.Items.Add(toData.Id);
                var toOnlineResult = ChatServer.Instance.SceneAgent.SBCheckCharacterOnline(proxy.CharacterId, arr);
                yield return toOnlineResult.SendAndWaitUntilDone(coroutine);
                if (toOnlineResult.State != MessageState.Reply)
                {
                    msg.Reply((int) ErrorCodes.Unknow);
                    yield break;
                }
                if (toOnlineResult.ErrorCode != (int) ErrorCodes.OK)
                {
                    msg.Reply(toOnlineResult.ErrorCode);
                    yield break;
                }
                if (toOnlineResult.Response.Items[0] == 0)
                {
                    var saveMessage = ChatServer.Instance.ChatAgent.CacheChatMessage(toData.Id, type,
                        msg.CharacterId, fromData.Name, newContentSound);
                    yield return saveMessage.SendAndWaitUntilDone(coroutine);

                    //返回给自己的信息 [-][C0C0C0][不在线]
                    newContentSound.Content = newContent;
                    ChatManager.ToClinetMessage(msg.CharacterId, (int) eChatChannel.MyWhisper, toData.Id, toData.Name,
                        newContentSound);

                    proxy.Character.PushChat(toData);
                    msg.Reply((int) ErrorCodes.Error_CharacterOutLine);
                    yield break;
                }

                var logicFlagData = ChatServer.Instance.LogicAgent.SSGetFlagOrCondition(toData.Id, toData.Id,
                    481, -1);
                yield return logicFlagData.SendAndWaitUntilDone(coroutine);

                if (logicFlagData.State != MessageState.Reply)
                {
                    msg.Reply((int) ErrorCodes.Unknow);
                    yield break;
                }
                if (logicFlagData.ErrorCode != (int) ErrorCodes.OK)
                {
                    msg.Reply(logicFlagData.ErrorCode);
                    yield break;
                }
                if (logicFlagData.Response == 1)
                {
                    msg.Reply((int) ErrorCodes.Error_SetRefuseWhisper);
                    yield break;
                }

                var logicShieldData = ChatServer.Instance.LogicAgent.SSIsShield(msg.CharacterId, msg.CharacterId,
                    toData.Id);
                yield return logicShieldData.SendAndWaitUntilDone(coroutine);

                if (logicShieldData.State != MessageState.Reply)
                {
                    msg.Reply((int) ErrorCodes.Unknow);
                    yield break;
                }
                if (logicShieldData.ErrorCode != (int) ErrorCodes.OK)
                {
                    msg.Reply(logicShieldData.ErrorCode);
                    yield break;
                }
                if (logicShieldData.Response == 1)
                {
                    //屏蔽了目标
                    msg.Reply((int) ErrorCodes.Error_SetYouShield);
                    yield break;
                }

                var logicShieldData2 = ChatServer.Instance.LogicAgent.SSIsShield(toData.Id, toData.Id,
                    msg.CharacterId);
                yield return logicShieldData2.SendAndWaitUntilDone(coroutine);

                if (logicShieldData2.State != MessageState.Reply)
                {
                    msg.Reply((int) ErrorCodes.Unknow);
                    yield break;
                }
                if (logicShieldData2.ErrorCode != (int) ErrorCodes.OK)
                {
                    msg.Reply(logicShieldData2.ErrorCode);
                    yield break;
                }
                if (logicShieldData2.Response == 1)
                {
                    //目标屏蔽了你
                    msg.Reply((int) ErrorCodes.Error_SetShieldYou);
                    yield break;
                }
                ChatManager.ToClinetMessage(msg.CharacterId, (int) eChatChannel.MyWhisper, toData.Id, toData.Name,
                    newContentSound);
                proxy.Character.PushChat(toData);
                var list = new List<ulong> {toData.Id};
                ChatServer.Instance.ChatAgent.ChatNotify(list, (int) eChatChannel.Whisper, msg.CharacterId,
                    fromData.Name, newContentSound);
                var charController = tochar;
                if (charController != null)
                {
                    charController.PushChat(fromData);
                }
                else
                {
                    var addRecent = ChatServer.Instance.ChatAgent.AddRecentcontacts(toData.Id,
                        fromData);
                    yield return addRecent.SendAndWaitUntilDone(coroutine);
                }
                msg.Reply();

                try
                {
                    string v = string.Format("admin_chatlog#{0}|{1}|{2}|{3}|{4}",
                        proxy.CharacterId,
                        fromData.Name,
                        type,
                        content.Content,
                        DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")); // 时间
                    kafaLogger.Info(v);
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                }
                yield break;
            }
            #endregion
            #region 世界
            if (type == (int) eChatChannel.World)
            {
                //世界聊天
                if (string.IsNullOrEmpty(proxy.Character.Name))
                {
                    var dbLoginSimple = ChatServer.Instance.LoginAgent.GetLoginSimpleData(proxy.ClientId,
                        proxy.CharacterId);
                    yield return dbLoginSimple.SendAndWaitUntilDone(coroutine);
                    if (dbLoginSimple.State != MessageState.Reply)
                    {
                        msg.Reply((int) ErrorCodes.Unknow);
                        yield break;
                    }
                    if (dbLoginSimple.ErrorCode != (int) ErrorCodes.OK)
                    {
                        msg.Reply(dbLoginSimple.ErrorCode);
                        yield break;
                    }
                    proxy.Character.Name = dbLoginSimple.Response.Name;
                }
                ChatManager.BroadcastServerIdMessage((uint) proxy.Character.ServerId, type,
                    msg.CharacterId, proxy.Character.Name, content);
                msg.Reply();

                try
                {
                    //其他服世界聊天消息发送给主播
                    var curAnchor = AnchorManager.Instance.GetCurrentAnchor();
                    if (curAnchor != 0)
                    {
                        var anchorCharacter = CharacterManager.Instance.GetCharacterControllerFromMemroy(curAnchor);
                        if (null != anchorCharacter)
                        {
                            if (anchorCharacter.Proxy.Character.ServerId != proxy.Character.ServerId)
                            {
                                anchorCharacter.Proxy.SyncChatMessage((int)eChatChannel.World, msg.CharacterId,
                                    proxy.Character.Name, content);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                }


                try
                {
                    string v = string.Format("admin_chatlog#{0}|{1}|{2}|{3}|{4}",
                        proxy.CharacterId,
                        proxy.Character.Name, //serverID
                        type,
                        content.Content,
                        DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")); // 时间
                    kafaLogger.Info(v);
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                }
                yield break;
            }
            #endregion
            #region 主播
            if (type == (int) eChatChannel.Anchor)
            { // 主播
                if (string.IsNullOrEmpty(proxy.Character.Name))
                {
                    var dbLoginSimple = ChatServer.Instance.LoginAgent.GetLoginSimpleData(proxy.ClientId,
                        proxy.CharacterId);
                    yield return dbLoginSimple.SendAndWaitUntilDone(coroutine);
                    if (dbLoginSimple.State != MessageState.Reply)
                    {
                        msg.Reply((int)ErrorCodes.Unknow);
                        yield break;
                    }
                    if (dbLoginSimple.ErrorCode != (int)ErrorCodes.OK)
                    {
                        msg.Reply(dbLoginSimple.ErrorCode);
                        yield break;
                    }
                    proxy.Character.Name = dbLoginSimple.Response.Name;
                }
                ChatManager.BroadcastAllServerMessage(type, msg.CharacterId, proxy.Character.Name, content);

                msg.Reply();


                try
                {
                    string v = string.Format("admin_chatlog#{0}|{1}|{2}|{3}|{4}",
                        proxy.CharacterId,
                        proxy.Character.Name, //serverID
                        type,
                        content.Content,
                        DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")); // 时间
                    kafaLogger.Info(v);
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                }
                yield break;
            }
            #endregion
            #region 城市
            if (type == (int) eChatChannel.City)
            {
//同城聊天
                var cId = proxy.Character.ChannelGuid;
                var c = CityChatManager.GetChannel(cId);
                if (c == null)
                {
                    msg.Reply((int) ErrorCodes.Error_ChatChannel);
                    yield break;
                }
                var uint64Array = new Uint64Array();
                foreach (var character in c.Characters)
                {
                    uint64Array.Items.Add(character.Key);
                }
                if (string.IsNullOrEmpty(proxy.Character.Name))
                {
                    var dbLoginSimple = ChatServer.Instance.LoginAgent.GetLoginSimpleData(proxy.ClientId,
                        proxy.CharacterId);
                    yield return dbLoginSimple.SendAndWaitUntilDone(coroutine);
                    if (dbLoginSimple.State != MessageState.Reply)
                    {
                        msg.Reply((int) ErrorCodes.Unknow);
                        yield break;
                    }
                    proxy.Character.Name = dbLoginSimple.Response.Name;
                }
                ChatServer.Instance.ChatAgent.SyncToListCityChatMessage(uint64Array.Items, type,
                    msg.CharacterId, proxy.Character.Name, content, c.ChannelName);

                msg.Reply();

                try
                {
                    string v = string.Format("admin_chatlog#{0}|{1}|{2}|{3}|{4}",
                        proxy.CharacterId,
                        proxy.Character.Name, //serverID
                        type,
                        content.Content,
                        DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")); // 时间
                    kafaLogger.Info(v);
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                }
                yield break;
            }
            #endregion
            #region 组队(跨服)
            if (type == (int)eChatChannel.TeamWorld)
            { // 组队(跨服)
                if (string.IsNullOrEmpty(proxy.Character.Name))
                {
                    var dbLoginSimple = ChatServer.Instance.LoginAgent.GetLoginSimpleData(proxy.ClientId,
                        proxy.CharacterId);
                    yield return dbLoginSimple.SendAndWaitUntilDone(coroutine);
                    if (dbLoginSimple.State != MessageState.Reply)
                    {
                        msg.Reply((int)ErrorCodes.Unknow);
                        yield break;
                    }
                    if (dbLoginSimple.ErrorCode != (int)ErrorCodes.OK)
                    {
                        msg.Reply(dbLoginSimple.ErrorCode);
                        yield break;
                    }
                    proxy.Character.Name = dbLoginSimple.Response.Name;
                }
                ChatManager.BroadcastAllServerMessage(type, msg.CharacterId, proxy.Character.Name, content);

                msg.Reply();


                try
                {
                    string v = string.Format("admin_chatlog#{0}|{1}|{2}|{3}|{4}",
                        proxy.CharacterId,
                        proxy.Character.Name, //serverID
                        type,
                        content.Content,
                        DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")); // 时间
                    kafaLogger.Info(v);
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                }
                yield break;
            }
            #endregion
            msg.Reply((int) ErrorCodes.Error_ChatChannel);
        }

        public IEnumerator SendHornMessage(Coroutine coroutine, ChatCharacterProxy _this, SendHornMessageInMessage msg)
        {
            var proxy = (ChatProxy)_this;
            if (proxy.Character == null)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (proxy.Character.mDbData.BannedToPost != 0)
            {
                //被禁言了
                msg.Reply((int)ErrorCodes.Error_BannedToPost);
                yield break;
            }
            #region 37监测
            {
                if (proxy.Character.moniterData.channel.Equals("37"))
                {
                    #region cm3
                    {//CM3
                        var unixTimer = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
                        var dic = new Dictionary<string, string>();
                        dic.Add("time", Math.Ceiling(unixTimer).ToString());
                        dic.Add("uid", proxy.Character.moniterData.uid);
                        dic.Add("gid", proxy.Character.moniterData.gid.ToString());
                        dic.Add("dsid", proxy.Character.ServerId.ToString());
                        var md5Key = "Ob7mD7HqInhGxTNt";
                        dic.Add("type", "7");
                        var strsign = string.Format("{0}{1}{2}{3}{4}{5}", md5Key, proxy.Character.moniterData.uid, proxy.Character.moniterData.gid, proxy.Character.ServerId, Math.Ceiling(unixTimer), dic["type"]);//md5Key + "}{" + proxy.Character.moniterData.uid + "}{""}{" + proxy.Character.ServerId.ToString()+ "}{" + unixTimer + "}{" + dic["type"] + "}");
                        var sign = RequestManager.Encrypt_MD5_UTF8(strsign);
                        dic.Add("sign", sign);
                        dic.Add("actor_name", HttpUtility.UrlEncode(proxy.Character.Name));
                        dic.Add("actor_id", proxy.Character.mGuid.ToString());
                        dic.Add("chat_time", Math.Ceiling(unixTimer).ToString());
                        dic.Add("content", HttpUtility.UrlEncode(msg.Request.Content.Content));


                        var url = @"http://cm3.api.37.com.cn/Content/_checkContent";
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
                    }
                    #endregion
                    #region CM2
                    {//CM2
                        var unixTimer = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
                        var strTimer = Math.Ceiling(unixTimer).ToString();
                        var chatType = 7;
                        List<KeyValuePair<string, string>> param = new List<KeyValuePair<string, string>>();
                        param.Add(new KeyValuePair<string, string>("time", strTimer));
                        param.Add(new KeyValuePair<string, string>("uid", proxy.Character.moniterData.uid));
                        param.Add(new KeyValuePair<string, string>("gid", proxy.Character.moniterData.gid.ToString()));
                        param.Add(new KeyValuePair<string, string>("dsid", HttpUtility.UrlEncode(proxy.Character.ServerId.ToString())));
                        param.Add(new KeyValuePair<string, string>("type", chatType.ToString()));
                        param.Add(new KeyValuePair<string, string>("actor_name", HttpUtility.UrlEncode(proxy.Character.Name)));
                        param.Add(new KeyValuePair<string, string>("actor_id", proxy.Character.mGuid.ToString()));
                      
                        param.Add(new KeyValuePair<string, string>("to_uid", ""));
                        param.Add(new KeyValuePair<string, string>("to_actor_name", ""));
                       
                        param.Add(new KeyValuePair<string, string>("content", HttpUtility.UrlEncode(msg.Request.Content.Content)));
                        param.Add(new KeyValuePair<string, string>("chat_time", strTimer));
                        param.Add(new KeyValuePair<string, string>("user_ip", proxy.Character.moniterData.ip));
                        param.Add(new KeyValuePair<string, string>("idfa", ""));
                        param.Add(new KeyValuePair<string, string>("idfv", ""));
                        param.Add(new KeyValuePair<string, string>("mac", ""));
                        param.Add(new KeyValuePair<string, string>("imei", ""));

                        const string cm2Url = "http://cm2.api.37.com.cn";
                        // const string cm2Url = "http://127.0.0.1:8888";
                        GetPageSizeAsync(cm2Url, param);
                    }
                    #endregion

                }
            }
            #endregion


            var logicMsg = ChatServer.Instance.LogicAgent.DeleteItem(msg.CharacterId, 21900, 1, (int)eDeleteItemType.HornMessage);
            yield return logicMsg.SendAndWaitUntilDone(coroutine);

            if (logicMsg.State != MessageState.Reply)
            {
                Logger.Error("DeleteItem failed! logicMsg.State = {0}", logicMsg.State);
                msg.Reply((int) ErrorCodes.Unknow);
                yield break;
            }
            if (logicMsg.ErrorCode != (int) ErrorCodes.OK)
            {
                Logger.Error("DeleteItem failed! logicMsg.ErrorCode = {0}", logicMsg.ErrorCode);
                msg.Reply(logicMsg.ErrorCode);
                yield break;
            }

            var msg1 = ChatServer.Instance.LogicAgent.GetItemCount(_this.CharacterId, (int)eResourcesType.VipLevel);
            yield return msg1.SendAndWaitUntilDone(coroutine);
            if (msg1.State == MessageState.Reply && msg1.ErrorCode == (int)ErrorCodes.OK)
            {
                msg.Request.Content.Vip = msg1.Response;
            }

            ChatManager.PushHornMessage(msg.Request.ServerId, msg.Request.ChatType, msg.Request.CharacterId,
                msg.Request.CharacterName, msg.Request.Content);
            msg.Reply();

            try
            {
                string v = string.Format("admin_chatlog#{0}|{1}|{2}|{3}|{4}",
                    msg.Request.CharacterId,
                    msg.Request.CharacterName,
                    msg.Request.ChatType,
                    msg.Request.Content.Content,
                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")); // 时间
                kafaLogger.Info(v);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        public IEnumerator GetRecentcontacts(Coroutine coroutine,
                                             ChatCharacterProxy charProxy,
                                             GetRecentcontactsInMessage msg)
        {
            var proxy = (ChatProxy) charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Chat----------GetRecentcontacts----------:");

            if (proxy.Character.mDbData.NearChats != null)

            {
                foreach (var chat in proxy.Character.mDbData.NearChats.Characters)
                {
                    msg.Response.Characters.Add(chat);
                }
            }
            msg.Reply();
            yield break;
        }

        public IEnumerator DeleteRecentcontacts(Coroutine coroutine,
                                                ChatCharacterProxy charProxy,
                                                DeleteRecentcontactsInMessage msg)
        {
            var proxy = (ChatProxy) charProxy;
            var chatacterid = msg.Request.CharacterId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Chat----------DeleteRecentcontacts----------:{0}",
                chatacterid);
            if (proxy.Character.mDbData.NearChats != null)
            {
                foreach (var chat in proxy.Character.mDbData.NearChats.Characters)
                {
                    if (chat.CharacterId == chatacterid)
                    {
                        proxy.Character.mDbData.NearChats.Characters.Remove(chat);
                        break;
                    }
                }
            }
            msg.Reply();
            yield break;
        }

        //进入频道
        public IEnumerator EnterChannel(Coroutine coroutine, ChatCharacterProxy charProxy, EnterChannelInMessage msg)
        {
            var proxy = (ChatProxy) charProxy;
            var channeled = msg.Request.ChannelId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Chat----------EnterChannel----------:{0},{1}", channeled,
                msg.Request.Password);
            var result = CityChatManager.EnterCity((int) channeled, proxy.Character);
            msg.Reply((int) result);
            yield break;
        }

        //离开频道
        public IEnumerator LeaveChannel(Coroutine coroutine, ChatCharacterProxy charProxy, LeaveChannelInMessage msg)
        {
            var proxy = (ChatProxy) charProxy;
            var channeled = msg.Request.ChannelId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Chat----------LeaveChannel----------:{0},{1}", channeled);
            var result = CityChatManager.LeaveChannel(channeled, proxy.Character);
            msg.Reply((int) result);
            yield break;
        }

		public IEnumerator ApplyAnchorRoomInfo(Coroutine coroutine, ChatCharacterProxy charProxy, ApplyAnchorRoomInfoInMessage msg)
	    {
			var proxy = (ChatProxy)charProxy;

			var config = AnchorManager.Instance.MyConfig;
			msg.Response.Open = config.Open;
			msg.Response.AnchorRoom = config.ServerName;
            msg.Response.AnchorName.AddRange(AnchorManager.Instance.GetAnchorNameList());
            msg.Response.BeginTime.AddRange(AnchorManager.Instance.GetAnchorBeginTimeList());
            msg.Response.EndTime.AddRange(AnchorManager.Instance.GetAnchorEndTimeList());
			msg.Response.GuildSpeekLevel = config.GuildSpeekLevel;
		    msg.Response.OnlineAnchorName = AnchorManager.Instance.GetCurrentAnchorName();
			msg.Reply((int)ErrorCodes.OK);
			yield break;
	    }



	    public IEnumerator OnConnected(Coroutine coroutine, ChatCharacterProxy charProxy, uint packId)
        {
//             ChatProxy proxy = (ChatProxy)charProxy;
//             Logger.Info("[{0}] has enter connected", proxy.CharacterId);
// 
//             //var notifyConnectedMsg = ChatServer.Instance.LoginAgent.NotifyConnected(proxy.ClientId, proxy.CharacterId,
//             //    (int)ServiceType.Chat, (int)ErrorCodes.OK);
//             //yield return notifyConnectedMsg.SendAndWaitUntilDone(coroutine);
// 
//             var characterId = proxy.CharacterId;
// 
//             ChatCharacterController obj = CharacterManager.Instance.GetCharacterControllerFromMemroy(proxy.CharacterId);
//             if (obj == null)
//             {
//                 Logger.Fatal("onConnected CharacterId={0}", proxy.CharacterId);
//                 yield break;
//             }
//             //var result = AsyncReturnValue<ChatCharacterController>.Create();
//             //var co = CoroutineFactory.NewSubroutine(CharacterManager.Instance.GetOrCreateCharacterController, coroutine,
//             //    characterId, new object[] { }, false, result);
//             //if (co.MoveNext())
//             //{
//             //    yield return co;
//             //}
//             //var obj = result.Value;
//             //result.Dispose();
// 
//             var dbLoginSimple = ChatServer.Instance.LoginAgent.GetLoginSimpleData(proxy.ClientId, characterId);
//             yield return dbLoginSimple.SendAndWaitUntilDone(coroutine);
//             if (dbLoginSimple.State != MessageState.Reply || dbLoginSimple.ErrorCode != (int) ErrorCodes.OK)
//             {
//                 yield break;
//             }
//             proxy.Character = obj;
//             obj.ServerId = dbLoginSimple.Response.ServerId;
//             obj.Proxy = proxy;
//             proxy.Connected = true;
// 
//             //foreach (var waitingCheckConnectedInMessages in proxy.WaitingCheckConnectedInMessages)
//             //{
//             //    waitingCheckConnectedInMessages.Reply();
//             //}
//             //proxy.WaitingCheckConnectedInMessages.Clear();
// 
//             proxy.Character.State = CharacterState.Connected;
//             
//             CharacterManager.Instance.UpdateSimpleData(proxy.CharacterId);
//             CityChatManager.EnterChannel(1, obj);
//             obj.mChat.Online();
            yield break;
        }

        public IEnumerator OnLost(Coroutine coroutine, ChatCharacterProxy charProxy, uint packId)
        {
//             ChatProxy proxy = (ChatProxy)charProxy;
//             ChatManager.OnLost(proxy.CharacterId);
//             if (proxy.Character != null)
//             {
//                 proxy.Character.Proxy = null;    
//             }
//             var co = CoroutineFactory.NewSubroutine(CharacterManager.Instance.RemoveCharacter, coroutine, proxy.CharacterId);
//             if (co.MoveNext())
//             {
//                 yield return co;
//             }
//             proxy.Connected = false;
            //foreach (var waitingCheckLostInMessage in proxy.WaitingCheckLostInMessages)
            //{
            //    waitingCheckLostInMessage.Reply();
            //}
            //proxy.WaitingCheckLostInMessages.Clear();
            yield break;
        }

        public bool OnSyncRequested(ChatCharacterProxy charProxy, ulong characterId, uint syncId)
        {
            return false;
        }
    }

    public class ChatProxy : ChatCharacterProxy
    {
        //public List<CheckConnectedInMessage> WaitingCheckConnectedInMessages = new List<CheckConnectedInMessage>();
        //public List<CheckLostInMessage> WaitingCheckLostInMessages = new List<CheckLostInMessage>();
        public ChatProxy(ChatService service, ulong characterId, ulong clientId)
            : base(service, characterId, clientId)
        {
        }

        public ChatCharacterController Character { get; set; }
        public bool Connected { get; set; }
    }
}