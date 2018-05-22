#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DataContract;
using DataTable;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scorpion;
using NLog;
using Shared;
using TeamServerService;

#endregion

namespace Team
{
    public class TeamProxyDefaultImpl : ITeamCharacterProxy
    {
        [Updateable("TeamProxy")]
        private static readonly List<int> ErrPriorityList = new List<int>
        {
            (int) ErrorCodes.Error_FubenID,
            (int) ErrorCodes.Error_LevelNoEnough,
            (int) ErrorCodes.Error_FubenCountNotEnough,
            (int) ErrorCodes.ItemNotEnough,
            (int) ErrorCodes.Error_FubenRewardNotReceived,
            (int) ErrorCodes.Error_AlreadyInThisDungeon,
            (int) ErrorCodes.Unline
        };

        [Updateable("LogicProxy")]
        //开始排队
        public static bool isNewQueue = true;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Logger kafaLogger = LogManager.GetLogger(Shared.LoggerName.KafkaLog);

        public IEnumerator GMTeam(Coroutine co, TeamCharacterProxy charProxy, GMTeamInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            var gmCommond = msg.Request.Commond;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------GMTeam----------:{0}", gmCommond);

            var err = new AsyncReturnValue<ErrorCodes>();
            var co1 = CoroutineFactory.NewSubroutine(Utility.GmCommand, co, proxy.CharacterId, gmCommond, err);
            if (co1.MoveNext())
            {
                yield return co1;
            }
            msg.Reply((int)err.Value);
            err.Dispose();
        }

        public IEnumerator ClientApplyHoldLode(Coroutine coroutine,
            TeamCharacterProxy _this,
            ClientApplyHoldLodeInMessage msg)
        {//客户端请求

            MsgSceneLodeList tmp = new MsgSceneLodeList();
            ServerLodeManagerManager.ApplyHoldLode(SceneExtension.GetServerLogicId(msg.Request.ServerId), ref tmp);
            msg.Response = tmp;
            msg.Reply();


            yield break;
        }

        //拍卖行：查询道具
        public IEnumerator CSSelectItemAuction(Coroutine coroutine,
                                               TeamCharacterProxy _this,
                                               CSSelectItemAuctionInMessage msg)
        {
            var serverId = msg.Request.ServerId;
            var findType = msg.Request.Type;
            var auc = ServerAuctionManager.instance.GetAuction(serverId);
            if (auc == null)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            var t = auc.SelectItem(findType);
            if (t == null)
            {
                msg.Reply();
                yield break;
            }
            foreach (var value in t.Values)
            {
                msg.Response.Items.Add(value.dbData);
                //msg.Response.Items.Add(new AuctionItemOne()
                //{
                //    Id = value.dbData.Id,
                //    ItemData = value.dbData.ItemData,
                //    NeedCount = value.dbData.NeedCount,
                //    NeedType = value.dbData.NeedType,
                //    OverTime = value.dbData.OverTime
                //});
            }
            msg.Reply();
        }

        public IEnumerator OnConnected(Coroutine coroutine, TeamCharacterProxy charProxy, uint packId)
        {
            //             TeamProxy proxy = (TeamProxy)charProxy;
            //             TeamManager.OnLine(proxy.CharacterId);
            //             QueueManager.OnLine(proxy.CharacterId);
            // 
            //             ((TeamProxy)charProxy).Connected = true;
            //             //foreach (var waitingCheckConnectedInMessage in proxy.WaitingCheckConnectedInMessages)
            //             //{
            //             //    waitingCheckConnectedInMessage.Reply();
            //             //}
            //             //proxy.WaitingCheckConnectedInMessages.Clear();
            //             //var notifyConnectedMsg = TeamServer.Instance.LoginAgent.NotifyConnected(proxy.ClientId, proxy.CharacterId,
            //             //    (int)ServiceType.Team, (int)ErrorCodes.OK);
            //             //yield return notifyConnectedMsg.SendAndWaitUntilDone(coroutine);
            // 
            //             proxy.SendGroupMessage(GroupShop.DbData.Notifys);
            yield break;
        }

        public IEnumerator OnLost(Coroutine coroutine, TeamCharacterProxy charProxy, uint packId)
        {
            //             TeamProxy proxy = (TeamProxy)charProxy;
            //             TeamManager.OnLost(proxy.CharacterId);
            //             QueueManager.OnLost(proxy.CharacterId);
            //             //MatchingManager.OnLost(CharacterId);
            //             ServerAllianceManager.OnLost(proxy.CharacterId);
            //             proxy.Connected = false; 
            //             //foreach (var waitingCheckLostInMessage in proxy.WaitingCheckLostInMessages)
            //             //{
            //             //    waitingCheckLostInMessage.Reply();
            //             //}
            //             //proxy.WaitingCheckLostInMessages.Clear();
            //             return null;
            yield break;
        }

        //组队聊天
        public IEnumerator TeamChatMessage(Coroutine coroutine,
                                           TeamCharacterProxy charProxy,
                                           TeamChatMessageInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            var request = msg.Request;
            var content = request.Content;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------TeamChatMessage----------:{0},{1},{2}",
                request, content, msg.CharacterId);
            if (!StringExtension.CheckChatStr(content.Content))
            {
                msg.Reply((int)ErrorCodes.Error_ChatLengthMax);
                yield break;
            }
            var dbLoginSimple = TeamServer.Instance.LoginAgent.GetLoginSimpleData(proxy.ClientId, msg.CharacterId);
            yield return dbLoginSimple.SendAndWaitUntilDone(coroutine);
           
            var msg1 = TeamServer.Instance.LogicAgent.GetItemCount(proxy.CharacterId, (int)eResourcesType.VipLevel);
            yield return msg1.SendAndWaitUntilDone(coroutine);
            if (msg1.State == MessageState.Reply && msg1.ErrorCode == (int)ErrorCodes.OK)
            {
                content.Vip = msg1.Response;
            }
            var msg2 = TeamServer.Instance.ChatAgent.GetSilenceState(proxy.CharacterId, 0);
            yield return msg2.SendAndWaitUntilDone(coroutine);
            if (msg2.State == MessageState.Reply && msg2.ErrorCode == (int)ErrorCodes.OK)
            {
                if (msg2.Response != 0)
                {
                    //被禁言了
                    msg.Reply((int)ErrorCodes.Error_BannedToPost);
                    yield break;
                }
            }
            var msgMonData =  TeamServer.Instance.LogicAgent.GetPlayerMoniterData(proxy.CharacterId, 0);
            yield return msgMonData.SendAndWaitUntilDone(coroutine);
            if (msgMonData == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }
            var moniterData = msgMonData.Response;
            if (moniterData == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }
            bool bNormal = content.Content.IndexOf("{!") == -1 && content.Content.IndexOf("!}") == -1;
        

            if (request.ChatType == (int)eChatChannel.Team)
            {
                var characterTeam = TeamManager.GetCharacterTeam(msg.CharacterId);
                if (characterTeam == null)
                {
                    msg.Reply((int)ErrorCodes.Error_CharacterNoTeam);
                    yield break;
                }
                #region 37监测
                if (moniterData.channel.Equals("37") && bNormal == true)
                {
                    #region cm3
                    {//CM3
                        var unixTimer = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
                        var dic = new Dictionary<string, string>();
                        dic.Add("time", Math.Ceiling(unixTimer).ToString());
                        dic.Add("uid", moniterData.uid);
                        dic.Add("gid", moniterData.gid.ToString());
                        dic.Add("dsid", dbLoginSimple.Response.ServerId.ToString());
                        var md5Key = "Ob7mD7HqInhGxTNt";
                        {
                            switch (request.ChatType)
                            {
                                case (int)eChatChannel.Team:
                                    dic.Add("type", "3");
                                    break;
                                case (int)eChatChannel.Guild:
                                    dic.Add("type", "2");
                                    break;
                                default:
                                    dic.Add("type", "8");
                                    break;
                            }
                        }
                        var strsign = string.Format("{0}{1}{2}{3}{4}{5}", md5Key, moniterData.uid, moniterData.gid, dbLoginSimple.Response.ServerId, Math.Ceiling(unixTimer), dic["type"]);//md5Key + "}{" + chatChar.moniterData.uid + "}{""}{" + chatChar.ServerId.ToString()+ "}{" + unixTimer + "}{" + dic["type"] + "}");
                        var sign = RequestManager.Encrypt_MD5_UTF8(strsign);
                        dic.Add("sign", sign);
                        dic.Add("actor_name", HttpUtility.UrlEncode(dbLoginSimple.Response.Name));
                        dic.Add("actor_id", dbLoginSimple.Response.Id.ToString());//chatChar.mGuid.ToString()
                        dic.Add("chat_time", Math.Ceiling(unixTimer).ToString());
                        dic.Add("content", HttpUtility.UrlEncode(content.Content));

                        var url = @"http://cm3.api.37.com.cn/Content/_checkContent";
                        //var url = @"http://183.60.124.198/Content/_checkContent"; //"183.60.124.198"; 
                        var result = AsyncReturnValue<string>.Create();
                        yield return TeamManager.WebRequestManager.DoRequest(coroutine, url, dic, result);

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
                        var chatType = 8;
                        switch (request.ChatType)
                        {

                            case (int)eChatChannel.Team:
                                //dic.Add("type", "3");
                                chatType = 3;
                                break;
                            case (int)eChatChannel.Guild:
                                //dic.Add("type", "2");
                                chatType = 2;
                                break;
                            default:
                                chatType = 8;
                                //dic.Add("type", "8");
                                break;
                        }
                        List<KeyValuePair<string, string>> param = new List<KeyValuePair<string, string>>();
                        param.Add(new KeyValuePair<string, string>("time", strTimer));
                        param.Add(new KeyValuePair<string, string>("uid", moniterData.uid));
                        param.Add(new KeyValuePair<string, string>("gid", moniterData.gid.ToString()));
                        param.Add(new KeyValuePair<string, string>("dsid", HttpUtility.UrlEncode(dbLoginSimple.Response.ServerId.ToString())));
                        param.Add(new KeyValuePair<string, string>("type", chatType.ToString()));
                        param.Add(new KeyValuePair<string, string>("actor_name", HttpUtility.UrlEncode(dbLoginSimple.Response.Name)));
                        param.Add(new KeyValuePair<string, string>("actor_id", dbLoginSimple.Response.Id.ToString()));
                        //组队和工会没有目标角色Id
                        param.Add(new KeyValuePair<string, string>("to_uid", ""));
                        param.Add(new KeyValuePair<string, string>("to_actor_name", ""));
                        param.Add(new KeyValuePair<string, string>("content", HttpUtility.UrlEncode(content.Content)));
                        param.Add(new KeyValuePair<string, string>("chat_time", strTimer));
                        param.Add(new KeyValuePair<string, string>("user_ip", moniterData.ip));
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

                var ids = characterTeam.team.TeamList.ToList();
                TeamServer.Instance.ChatAgent.ChatNotify(ids, request.ChatType, msg.CharacterId,
                    dbLoginSimple.Response.Name, content);
                msg.Reply();

                try
                {
                    string v = string.Format("admin_chatlog#{0}|{1}|{2}|{3}|{4}",
                        proxy.CharacterId,
                        dbLoginSimple.Response.Name, //serverID
                        request.ChatType,
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

            if (request.ChatType == (int)eChatChannel.Guild)
            {
                var serverAlliance = ServerAllianceManager.GetAllianceByServer(dbLoginSimple.Response.ServerId);
                if (serverAlliance == null)
                {
                    msg.Reply((int)ErrorCodes.ServerID);
                    yield break;
                }
                var requestCharacter = serverAlliance.GetCharacterData(msg.CharacterId);
                if (requestCharacter == null)
                {
                    Logger.Warn("TeamChatMessage is not apply proxy alliance[{0}]", msg.CharacterId);
                    msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                    yield break;
                }
                var alliance = ServerAllianceManager.GetAllianceById(requestCharacter.AllianceId);
                if (alliance == null)
                {
                    msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                    yield break;
                }
                #region 37监测
                if (moniterData.channel.Equals("37") && bNormal == true)
                {
                    #region cm3
                    {//CM3
                        var unixTimer = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
                        var dic = new Dictionary<string, string>();
                        dic.Add("time", Math.Ceiling(unixTimer).ToString());
                        dic.Add("uid", moniterData.uid);
                        dic.Add("gid", moniterData.gid.ToString());
                        dic.Add("dsid", dbLoginSimple.Response.ServerId.ToString());
                        var md5Key = "Ob7mD7HqInhGxTNt";
                        {
                            switch (request.ChatType)
                            {
                                case (int)eChatChannel.Team:
                                    dic.Add("type", "3");
                                    break;
                                case (int)eChatChannel.Guild:
                                    dic.Add("type", "2");
                                    break;
                                default:
                                    dic.Add("type", "8");
                                    break;
                            }
                        }
                        var strsign = string.Format("{0}{1}{2}{3}{4}{5}", md5Key, moniterData.uid, moniterData.gid, dbLoginSimple.Response.ServerId, Math.Ceiling(unixTimer), dic["type"]);//md5Key + "}{" + chatChar.moniterData.uid + "}{""}{" + chatChar.ServerId.ToString()+ "}{" + unixTimer + "}{" + dic["type"] + "}");
                        var sign = RequestManager.Encrypt_MD5_UTF8(strsign);
                        dic.Add("sign", sign);
                        dic.Add("actor_name", HttpUtility.UrlEncode(dbLoginSimple.Response.Name));
                        dic.Add("actor_id", dbLoginSimple.Response.Id.ToString());//chatChar.mGuid.ToString()
                        dic.Add("chat_time", Math.Ceiling(unixTimer).ToString());
                        dic.Add("content", HttpUtility.UrlEncode(content.Content));

                        var url = @"http://cm3.api.37.com.cn/Content/_checkContent";
                        //var url = @"http://183.60.124.198/Content/_checkContent"; //"183.60.124.198"; 
                        var result = AsyncReturnValue<string>.Create();
                        yield return TeamManager.WebRequestManager.DoRequest(coroutine, url, dic, result);

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
                        var chatType = 8;
                        switch (request.ChatType)
                        {

                            case (int)eChatChannel.Team:
                                //dic.Add("type", "3");
                                chatType = 3;
                                break;
                            case (int)eChatChannel.Guild:
                                //dic.Add("type", "2");
                                chatType = 2;
                                break;
                            default:
                                chatType = 8;
                                //dic.Add("type", "8");
                                break;
                        }
                        List<KeyValuePair<string, string>> param = new List<KeyValuePair<string, string>>();
                        param.Add(new KeyValuePair<string, string>("time", strTimer));
                        param.Add(new KeyValuePair<string, string>("uid", moniterData.uid));
                        param.Add(new KeyValuePair<string, string>("gid", moniterData.gid.ToString()));
                        param.Add(new KeyValuePair<string, string>("dsid", HttpUtility.UrlEncode(dbLoginSimple.Response.ServerId.ToString())));
                        param.Add(new KeyValuePair<string, string>("type", chatType.ToString()));
                        param.Add(new KeyValuePair<string, string>("actor_name", HttpUtility.UrlEncode(dbLoginSimple.Response.Name)));
                        param.Add(new KeyValuePair<string, string>("actor_id", dbLoginSimple.Response.Id.ToString()));
                        //组队和工会没有目标角色Id
                        param.Add(new KeyValuePair<string, string>("to_uid", ""));
                        param.Add(new KeyValuePair<string, string>("to_actor_name", ""));
                        param.Add(new KeyValuePair<string, string>("content", HttpUtility.UrlEncode(content.Content)));
                        param.Add(new KeyValuePair<string, string>("chat_time", strTimer));
                        param.Add(new KeyValuePair<string, string>("user_ip", moniterData.ip));
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
                var ids = alliance.mDBData.Members.ToList();
                TeamServer.Instance.ChatAgent.ChatNotify(ids, request.ChatType, msg.CharacterId,
                    dbLoginSimple.Response.Name, content);
                msg.Reply();

                try
                {
                    string v = string.Format("admin_chatlog#{0}|{1}|{2}|{3}|{4}",
                        proxy.CharacterId,
                        dbLoginSimple.Response.Name, //serverID
                        request.ChatType,
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
            msg.Reply();
        }
   
        [Updateable("TeamProxy")]
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
        //排队
        public IEnumerator TeamDungeonLineUp(Coroutine coroutine,
                                             TeamCharacterProxy charProxy,
                                             TeamDungeonLineUpInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------TeamDungeonLineUp----------");
            msg.Reply();
            yield break;
        }
        //排队
        public IEnumerator ClientApplyActiveInfo(Coroutine coroutine,
                                             TeamCharacterProxy charProxy,
                                             ClientApplyActiveInfoInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------TeamDungeonLineUp----------");

            LodeManager mgr;
            if (ServerLodeManagerManager.Servers.TryGetValue(SceneExtension.GetServerLogicId(msg.Request.ServerId), out mgr) == false)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            mgr.OnAllianceEvent(msg.Request.AllianceId, -1);
            msg.Response = mgr.mDbData.ActivityInfo;
            msg.Reply();
            yield break;
        }


        public IEnumerator MatchingStart(Coroutine coroutine, TeamCharacterProxy charProxy, MatchingStartInMessage msg)
        {
            var characterId = msg.CharacterId;
            var proxy = (TeamProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------MatchingStart----------c={0},s={1}",
                proxy.CharacterId, msg.Request.QueueId);
            var ids = new List<ulong>();
            var CharacterTeam = TeamManager.GetCharacterTeam(characterId);
            if (CharacterTeam == null)
            {
                ids.Add(characterId); //没有队伍
            }
            else
            {
                if (CharacterTeam.TeamState != TeamState.Leader)
                {
                    msg.Reply((int)ErrorCodes.Error_CharacterNotLeader, true);
                    yield break;
                }
                ids.AddRange(CharacterTeam.team.TeamList); //有队伍
            }
            //在线检查
            foreach (var id in ids)
            {
                if (!TeamServer.Instance.ServerControl.Proxys.ContainsKey(id))
                {
                    msg.Response.CharacterId.Add(id);
                }
            }
            if (msg.Response.CharacterId.Count > 0)
            {
                msg.Reply((int)ErrorCodes.Unline, true);
                yield break;
            }
            //检查进入条件
            var tbQueue = Table.GetQueue(msg.Request.QueueId);
            if (tbQueue == null)
            {
                msg.Reply((int)ErrorCodes.Error_FubenID, true);
                yield break;
            }
            //确定每个玩家的进入条件满足
            var errs = new Dictionary<ulong, int>();
            if (CharacterTeam != null)
            {
                foreach (var id in CharacterTeam.team.TeamList)
                {
                    var logicResult = TeamServer.Instance.LogicAgent.CheckCharacterInFuben(id, tbQueue.Param);
                    yield return logicResult.SendAndWaitUntilDone(coroutine);
                    if (logicResult.State == MessageState.Reply)
                    {
                        if (logicResult.ErrorCode != (int)ErrorCodes.OK)
                        {
                            errs.Add(id, logicResult.ErrorCode);
                        }
                    }
                    else
                    {
                        errs.Add(id, (int)ErrorCodes.Error_TimeOut);
                    }
                }
                if (errs.Count > 0)
                {
                    try
                    {
                        foreach (var err in ErrPriorityList)
                        {
                            if (!errs.Values.Contains(err))
                            {
                                continue;
                            }
                            var errs1 = errs.Where(e => e.Value == err);
                            foreach (var e in errs1)
                            {
                                msg.Response.CharacterId.Add(e.Key);
                            }
                            msg.Reply(err, true);
                            yield break;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Logger.Error(ex);
                    }
                    {
                        var err = errs[0];
                        var errs1 = errs.Where(e => e.Value == err);
                        foreach (var e in errs1)
                        {
                            msg.Response.CharacterId.Add(e.Key);
                        }
                        msg.Reply(err, true);
                        yield break;
                    }
                }
            }
            else
            {
                //没有队伍，只检查我自己
                var logicResult = TeamServer.Instance.LogicAgent.CheckCharacterInFuben(characterId, tbQueue.Param);
                yield return logicResult.SendAndWaitUntilDone(coroutine);
                if (logicResult.State == MessageState.Reply)
                {
                    if (logicResult.ErrorCode != (int)ErrorCodes.OK)
                    {
                        msg.Response.CharacterId.Add(characterId);
                        msg.Reply(logicResult.ErrorCode, true);
                        yield break;
                    }
                }
                else
                {
                    msg.Response.CharacterId.Add(characterId);
                    msg.Reply((int)ErrorCodes.Error_TimeOut, true);
                    yield break;
                }
            }
            //获得角色信息
            var simpleDatas = new List<CharacterSimpleData>();
            foreach (var id in ids)
            {
                var dbSceneSimple = TeamServer.Instance.SceneAgent.GetSceneSimpleData(id, 0);
                yield return dbSceneSimple.SendAndWaitUntilDone(coroutine);

                //如果当前已经在战场里了，则不能继续排战场
                var sceneData = dbSceneSimple.Response;
                var tbScene = Table.GetScene(sceneData.SceneId);
                if (tbScene.FubenId != -1)
                {
                    var tbFuben = Table.GetFuben(tbScene.FubenId);
                    if (tbFuben != null && tbFuben.QueueParam != -1)
                    {
                        msg.Response.CharacterId.Add(id);
                        msg.Reply((int)ErrorCodes.Error_CharacterCantQueue, true);
                        yield break;
                    }
                }

                var temp = new CharacterSimpleData
                {
                    Id = sceneData.Id,
                    TypeId = sceneData.TypeId,
                    Name = sceneData.Name,
                    SceneId = sceneData.SceneId,
                    FightPoint = sceneData.FightPoint,
                    Level = sceneData.Level,
                    Ladder = sceneData.Ladder,
                    ServerId = sceneData.ServerId
                };
                simpleDatas.Add(temp);
            }
            if (!isNewQueue)
            {
                msg.Reply();
            }
            else
            {
                var result = QueueManager.Push(msg.Request.QueueId, simpleDatas, msg.Response);
                if (result != ErrorCodes.OK)
                {
                    msg.Reply((int)result, true);
                    yield break;
                }
                var character = QueueManager.GetQueueInfo(proxy.CharacterId);
                if (character == null)
                {
                    msg.Reply((int)ErrorCodes.Unknow, true);
                    yield break;
                }
                msg.Response.Info = new QueueInfo();
                msg.Response.Info.QueueId = character.mLogic.mQueueId;
                msg.Response.Info.NeedSeconds = character.mLogic.GetAverageTime();
                msg.Response.Info.StartTime = character.StartTime.ToBinary();
                msg.Reply();
            }
        }

        public void NoticeTeamMemberError(string _name, int ErrorCode, ulong toCharacterId)
        {
            var result = "";
            if (!string.IsNullOrEmpty(_name))
            {
                if (ErrorCode == (int)ErrorCodes.Error_FubenCountNotEnough)
                {
                    //{0}副本次数不够
                    result = Table.GetDictionary(466).Desc[0];
                    result = string.Format(result, _name);
                }
                else if (ErrorCode == (int)ErrorCodes.ItemNotEnough)
                {
                    //{{0}道具不足
                    result = Table.GetDictionary(467).Desc[0];
                    result = string.Format(result, _name);
                }
                else if (ErrorCode == (int)ErrorCodes.Error_LevelNoEnough)
                {
                    //{{0}等级不足
                    result = Table.GetDictionary(100001474).Desc[0];
                    result = string.Format(result, _name);
                }
                else if (ErrorCode == (int)ErrorCodes.Error_FubenRewardNotReceived)
                {
                    //{{0}没有领取奖励
                    result = Table.GetDictionary(497).Desc[0];
                    result = string.Format(result, _name);
                }
                else if (ErrorCode == (int)ErrorCodes.Unline)
                {
                    //有队友不在线
                    result = Table.GetDictionary(498).Desc[0];
                    result = string.Format(result, _name);
                }
                else if (ErrorCode == (int)ErrorCodes.Error_CharacterOutLine)
                {
                    //有队友不在线
                    result = Table.GetDictionary(498).Desc[0];
                    result = string.Format(result, _name);
                }
                else if (ErrorCode == (int)ErrorCodes.Error_AlreadyInThisDungeon)
                {
                    //{{0}有队员在副本中
                    result = Table.GetDictionary(493).Desc[0];
                    result = string.Format(result, _name);
                }
                else if (ErrorCode == (int)ErrorCodes.Error_CharacterCantQueue)
                {
                    //{{0}有队员在副本中
                    result = Table.GetDictionary(544).Desc[0];
                    result = string.Format(result, _name);
                }
                else
                {
                    //{{0}不符合副本条件
                    result = Table.GetDictionary(468).Desc[0];
                    result = string.Format(result, _name);
                }
            }
            else
            {
                result = ErrorCode.ToString();
            }
            TeamCharacterProxy toCharacterProxy;
            if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(toCharacterId, out toCharacterProxy))
            {
                var teamProxy = toCharacterProxy as TeamProxy;
                teamProxy.NoticeTeamMemberError(result);
            }
        }
        public void NoticeTeamMemberError(string errorConetn, ulong toCharacterId)
        {
            TeamCharacterProxy toCharacterProxy;
            if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(toCharacterId, out toCharacterProxy))
            {
                var teamProxy = toCharacterProxy as TeamProxy;
                teamProxy.NoticeTeamMemberError(errorConetn);
            }
        }
        //取消排队
        public IEnumerator MatchingCancel(Coroutine coroutine, TeamCharacterProxy charProxy, MatchingCancelInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------MatchingCancel----------c={0}",
                proxy.CharacterId);
            if (!isNewQueue)
            {
                //MatchingManager.Pop(CharacterId, eLeaveMatchingType.Cannel);
            }
            else
            {
                QueueManager.Pop(proxy.CharacterId, eLeaveMatchingType.Cannel);
            }
            msg.Reply();
            yield break;
        }

        //排队结果反馈
        public IEnumerator MatchingBack(Coroutine coroutine, TeamCharacterProxy charProxy, MatchingBackInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            var result = msg.Request.Result;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------MatchingBack----------{0}", result);
            if (!isNewQueue)
            {
            }
            else
            {
                QueueManager.MatchingBack(proxy.CharacterId, msg.Request.Result);
            }
            yield break;
        }

        //请求排队信息
        public IEnumerator ApplyQueueData(Coroutine coroutine, TeamCharacterProxy charProxy, ApplyQueueDataInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------ApplyQueueData----------");
            if (!isNewQueue)
            {
                msg.Reply();
            }
            else
            {
                var character = QueueManager.GetQueueInfo(proxy.CharacterId);
                if (character == null)
                {
                    msg.Reply((int)ErrorCodes.Unknow);
                    yield break;
                }
                msg.Response.QueueId = character.mLogic.mQueueId;
                msg.Response.NeedSeconds = character.mLogic.GetAverageTime();
                msg.Response.StartTime = character.StartTime.ToBinary();
                msg.Reply();
                if (character.result != null)
                {
                    int state;
                    if (character.result.CharacterState.TryGetValue(proxy.CharacterId, out state))
                    {
                        if (state == 0)
                        {
                            TeamCharacterProxy toCharacterProxy;
                            if (TeamServer.Instance.ServerControl.Proxys.TryGetValue(proxy.CharacterId,
                                out toCharacterProxy))
                            {
                                var ChattoCharacterProxy = toCharacterProxy as TeamProxy;
                                ChattoCharacterProxy.MatchingSuccess(character.mLogic.mQueueId);
                            }
                        }
                    }
                }
            }
        }
        //队伍消息
        public IEnumerator TeamMessage(Coroutine coroutine, TeamCharacterProxy charProxy, TeamMessageInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            var characterId = msg.CharacterId;
            var Type = msg.Request.Type;
            var teamId = msg.Request.TeamId;
            var TocharacterId = msg.Request.OtherId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------TeamMessage----------{0},{1}", Type,
                TocharacterId);
            PlayerLog.WriteLog((int)LogType.TeamMessage,
                "SC->TeamMessage characterId={0}, type={1}, teamId={2}, TocharacterId={3}", characterId, Type, teamId,
                TocharacterId);

            switch (Type)
            {
                case 0:
                    {
                        //创建队伍

                        {
                            ulong id = 0;
                            var err = TeamManager.CreateTeamEx(characterId, ref id);
                            msg.Response = id;
                            msg.Reply((int)err);
                            AutoMatchManager.changeTeamTarget(msg.CharacterId, 0, 0, 1, 400, 1);//附近的默认值
                            // team = CreateTeam(characterId);
                            // teamId = team.TeamId;
                            // characterMember = GetCharacterTeam(characterId);
                            // TeamChange(TeamChangedType.Request, team, characterId);
                        }
                        //{
                        //    var team = TeamManager.CreateTeam(characterId);
                        //    //ChatServer.Instance.SceneAgent.SceneTeamMessage(characterId, characterId, 0, team.TeamId, 0);
                        //    if (team == null)
                        //    {
                        //        msg.Reply((int)ErrorCodes.Error_CharacterHaveTeam);
                        //        yield break;
                        //    }
                        //    msg.Reply();                        
                        //}

                        yield break;
                    }
                case 1:
                    {
                        {
                            bool onlineState = false;
                            var dbLogicSimple = TeamServer.Instance.LogicAgent.GetLogicSimpleData(TocharacterId, 0);
                            yield return dbLogicSimple.SendAndWaitUntilDone(coroutine);
                            if (dbLogicSimple.State != MessageState.Reply)
                            {
                                yield break;
                            }

                            if (dbLogicSimple.ErrorCode == (int)ErrorCodes.OK)
                            {
                                if (dbLogicSimple.Response.Online == 1)
                                {
                                    onlineState = true;
                                }
                            }

                            if (!onlineState)
                            {
                                msg.Reply((int)ErrorCodes.Unline);
                                yield break;
                            }
                        }

                        //邀请组队
                        if (!TeamServer.Instance.ServerControl.Proxys.ContainsKey(TocharacterId))
                        {
                            msg.Reply((int)ErrorCodes.Unline);
                            yield break;
                        }
                        var conditionid = Table.GetServerConfig(219).ToInt();
                        var logicFlagData = TeamServer.Instance.LogicAgent.SSGetFlagOrCondition(TocharacterId, TocharacterId,
                            480, conditionid);
                        yield return logicFlagData.SendAndWaitUntilDone(coroutine);
                        if (logicFlagData.State == MessageState.Reply)
                        {
                            if (logicFlagData.ErrorCode == (int)ErrorCodes.OK)
                            {
                                if (logicFlagData.Response == 1)
                                {
                                    msg.Reply((int)ErrorCodes.Error_SetRefuseTeam);
                                    yield break;
                                }
                                if (logicFlagData.Response == 2)
                                {
                                    msg.Reply((int)ErrorCodes.Error_TeamFunctionNotOpen);
                                    yield break;
                                }
                            }
                        }

                        //                         var logicCondition = TeamServer.Instance.LogicAgent.SSGetCondition(TocharacterId, TocharacterId, conditionid);
                        //                         yield return logicCondition.SendAndWaitUntilDone(coroutine);
                        //                         if (logicFlagData.State == MessageState.Reply)
                        //                         {
                        //                             if (logicFlagData.ErrorCode == (int)ErrorCodes.OK)
                        //                             {
                        //                                 if (logicFlagData.Response == 0)
                        //                                 {
                        //                                     msg.Reply((int)ErrorCodes.Error_TeamFunctionNotOpen);
                        //                                     yield break;
                        //                                 }
                        //                             }
                        //                         }
                        //todo 判断此characterId玩家在不在不副本  还得判断TocharacterId是否能进入该副本
                        var errorCodes = TeamManager.Request(characterId, ref teamId, TocharacterId);
                        msg.Reply((int)errorCodes);
                        yield break;
                    }
                case 2:
                    {
                        //同意组队邀请
                        var errorCodes = TeamManager.AcceptRequest(characterId, teamId);
                        //if (errorCodes == ErrorCodes.OK)
                        //{
                        //    ChatServer.Instance.SceneAgent.SceneTeamMessage(characterId, characterId, 1, teamId, 0);
                        //}
                        msg.Reply((int)errorCodes);
                        yield break;
                    }
                case 3:
                    {
                        //申请加入
                        if (!TeamServer.Instance.ServerControl.Proxys.ContainsKey(TocharacterId))
                        {
                            msg.Reply((int)ErrorCodes.Unline);
                            yield break;
                        }
                        var errorCodes = TeamManager.ApplyJoin(characterId, TocharacterId);
                        msg.Reply((int)errorCodes);
                        yield break;
                    }
                case 4:
                    {
                        //同意申请
                        var errorCodes = TeamManager.AcceptJoin(characterId, teamId, TocharacterId);
                        //if (errorCodes == ErrorCodes.OK)
                        //{
                        //    ChatServer.Instance.SceneAgent.SceneTeamMessage(TocharacterId, TocharacterId, 1, teamId, 0);
                        //}
                        msg.Reply((int)errorCodes);
                        yield break;
                    }
                case 5:
                    {
                        //离开队伍
                        var errorCodes = TeamManager.Leave(characterId, teamId);
                        msg.Reply((int)errorCodes);
                        yield break;
                    }
                case 6:
                    {
                        //更换队长
                        var errorCodes = TeamManager.SwapLeader(characterId, TocharacterId);
                        msg.Reply((int)errorCodes);
                        yield break;
                    }
                case 7:
                    {
                        //解散队伍
                        var errorCodes = TeamManager.Disband(characterId, teamId);
                        msg.Reply((int)errorCodes);
                        yield break;
                    }
                case 8:
                    {
                        //踢出队伍
                        var errorCodes = TeamManager.Kick(characterId, teamId, TocharacterId);
                        msg.Reply((int)errorCodes);
                        yield break;
                    }
                case 9:
                    {
                        //拒绝邀请
                        var errorCodes = TeamManager.RefuseInvite(teamId, characterId);
                        msg.Reply((int)errorCodes);
                        yield break;
                    }
                case 10:
                    {
                        //拒绝申请
                        var errorCodes = TeamManager.RefuseJoin(characterId, TocharacterId);
                        msg.Reply((int)errorCodes);
                        yield break;
                    }
            }
            msg.Reply((int)ErrorCodes.Unknow);
        }

        //请求进入多人副本
        public IEnumerator TeamEnterFuben(Coroutine coroutine, TeamCharacterProxy charProxy, TeamEnterFubenInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            var characterId = proxy.CharacterId;
            var fubenId = msg.Request.FubenId;
            var serverId = msg.Request.ServerId;
            PlayerLog.WriteLog(characterId, "----------Team----------TeamEnterFuben----------{0},{1}", fubenId, serverId);
            var characterTeam = TeamManager.GetCharacterTeam(characterId);
            if (characterTeam == null)
            {
                msg.Reply((int)ErrorCodes.Error_CharacterNoTeam, true);
                yield break;
            }
            if (characterTeam.TeamState != TeamState.Leader)
            {
                msg.Reply((int)ErrorCodes.Error_CharacterNotLeader, true);
                yield break;
            }
            //副本是否存在
            var tbFuben = Table.GetFuben(fubenId);
            if (tbFuben == null)
            {
                msg.Reply((int)ErrorCodes.Error_FubenID, true);
                yield break;
            }
            //是否组队副本
            if (tbFuben.CanGroupEnter != 1)
            {
                msg.Reply((int)ErrorCodes.Unknow, true);
                yield break;
            }
            //确定玩家都在队伍,并且都在线
            var team = characterTeam.team;
            foreach (var id in team.TeamList)
            {
                var tempTeam = TeamManager.GetCharacterTeam(id);
                if (tempTeam == null)
                {
                    Logger.Error("TeamEnterFuben tempTeam is null! auto Disband");
                    msg.Response.Items.Add(id);
                    msg.Reply((int)ErrorCodes.Error_TeamNotFind, true);
                    team.Disband();
                    yield break;
                }
                if (tempTeam.team.TeamId != team.TeamId)
                {
                    Logger.Error("TeamEnterFuben tempTeam not same! auto Disband");
                    msg.Response.Items.Add(id);
                    msg.Reply((int)ErrorCodes.Error_TeamNotSame, true);
                    team.Disband();
                    yield break;
                }
                if (tempTeam.TeamState == TeamState.Leaver)
                {
                    msg.Response.Items.Add(id);
                    msg.Reply((int)ErrorCodes.Error_CharacterOutLine, true);
                    yield break;
                }
            }

            var errs = new Dictionary<ulong, int>();
            var TeamNames = new Dictionary<ulong, string>();
            team.TeamDatas.Clear();
           
            foreach (var id in team.TeamList)
            {
                var dbSceneSimple = TeamServer.Instance.SceneAgent.GetSceneSimpleData(id, 0);
                yield return dbSceneSimple.SendAndWaitUntilDone(coroutine);

                if (dbSceneSimple.State != MessageState.Reply)
                {
                    Logger.Error("In TeamEnterFuben(), GetSceneSimpleData return with dbSceneSimple.State = {0}",
                        dbSceneSimple.State);
                    errs.Add(id, (int)ErrorCodes.Unknow);
                    continue;
                }
                if (dbSceneSimple.ErrorCode != (int)ErrorCodes.OK)
                {
                    Logger.Error("In TeamEnterFuben(), GetSceneSimpleData return with dbSceneSimple.ErrorCode = {0}",
                        dbSceneSimple.ErrorCode);
                    errs.Add(id, dbSceneSimple.ErrorCode);
                    continue;
                }

                var sceneData = dbSceneSimple.Response;
                team.TeamDatas.Add(sceneData);
                TeamNames[id] = sceneData.Name;
                var tbScene = Table.GetScene(sceneData.SceneId);
                if (tbScene != null && tbScene.Type != (int)eSceneType.Normal && tbScene.Type != (int)eSceneType.City)
                {//已经在此副本内
                    errs.Add(id, (int)ErrorCodes.Error_AlreadyInThisDungeon);
                }
            }
            if (errs.Count > 0)
            {
                foreach (var err in ErrPriorityList)
                {
                    if (!errs.Values.Contains(err))
                    {
                        continue;
                    }
                    var errs1 = errs.Where(e => e.Value == err);
                    foreach (var e in errs1)
                    {
                        msg.Response.Items.Add(e.Key);
                    }
                    msg.Reply(err, true);

                    var dbSceneSimple = TeamServer.Instance.SceneAgent.GetSceneSimpleData(msg.Response.Items[0], 0);
                    yield return dbSceneSimple.SendAndWaitUntilDone(coroutine);
                    foreach (var id in team.TeamList)
                    {
                        if (id != characterId)
                            NoticeTeamMemberError(dbSceneSimple.Response.Name, err, id);
                    }
                    yield break;
                }
                {
                    var err = errs.First().Value;
                    var errs1 = errs.Where(e => e.Value == err);
                    foreach (var e in errs1)
                    {
                        msg.Response.Items.Add(e.Key);
                    }
                    msg.Reply(err, true);
                    yield break;
                }
            }

            int finalFubenId = fubenId;
            List<int> listFubenId = new List<int>();
            foreach (var id in team.TeamList)
            {
                listFubenId.Add(fubenId);
            }

            //地狱监牢 队伍进入需要特殊处理
            try
            {
                bool needProcess = false;
                DynamicActivityRecord tbDynamic = null;
                do
                {
                    tbDynamic = Table.GetDynamicActivity(11);
                    if (null != tbDynamic && tbDynamic.FuBenID.Contains(fubenId))
                    {
                        needProcess = true;
                        break;
                    }
                    tbDynamic = Table.GetDynamicActivity(12);
                    if (null != tbDynamic && tbDynamic.FuBenID.Contains(fubenId))
                    {
                        needProcess = true;
                        break;
                    }
                } while (false);


                if (needProcess && null != tbDynamic)
                {
                    //收集副本场景表数据
                    Dictionary<int, SceneRecord> dictSceneRecord = new Dictionary<int, SceneRecord>();
                    for (int j = tbDynamic.FuBenID.Length - 1; j >= 0; j--)
                    {
                        var tempFubenId = tbDynamic.FuBenID[j];
                        if (-1 == tempFubenId)
                        {
                            continue;
                        }
                        var tbTempFuben = Table.GetFuben(tempFubenId);
                        if (null == tbTempFuben)
                        {
                            continue;
                        }
                        var tbTempScene = Table.GetScene(tbTempFuben.SceneId);
                        if (null == tbTempScene)
                        {
                            continue;
                        }
                        dictSceneRecord.Add(tempFubenId, tbTempScene);
                    }

                    //确定每个人该去哪个场景
                    int totalLevel = 0;
                    for (int i = 0; i < team.TeamDatas.Count; i++)
                    {
                        var level = team.TeamDatas[i].Level;
                        int tempFubenId = finalFubenId;
                        int maxLevel = 0;
                        foreach (var kv in dictSceneRecord)
                        {
                            if (level >= kv.Value.LevelLimit && kv.Value.LevelLimit > maxLevel)
                            {
                                tempFubenId = kv.Key;
                                maxLevel = kv.Value.LevelLimit;
                            }
                        }
                        listFubenId[i] = tempFubenId;
                        totalLevel += level;
                    }

                    //计算最终该去的场景
                    int maxTempLevel = 0;
                    var averageLevel = Math.Ceiling(totalLevel * 1.0f / team.TeamDatas.Count);
                    foreach (var kv in dictSceneRecord)
                    {
                        if (averageLevel >= kv.Value.LevelLimit && kv.Value.LevelLimit > maxTempLevel)
                        {
                            finalFubenId = kv.Key;
                            maxTempLevel = kv.Value.LevelLimit;
                        }
                    }

                }
            }
            catch (Exception)
            {
                listFubenId.Clear();
                foreach (var id in team.TeamList)
                {
                    listFubenId.Add(fubenId);
                }
            }
            var errsDetail = new Dictionary<ulong, int>();
            
            //确定每个玩家的进入条件满足
            for (int i = 0; i < team.TeamList.Count; i++)
            {
                var id = team.TeamList[i];
                var tempFubenId = listFubenId[i];
                var logicResult = TeamServer.Instance.LogicAgent.CheckCharacterInFuben(id, tempFubenId);
                yield return logicResult.SendAndWaitUntilDone(coroutine);
                if (logicResult.State == MessageState.Reply)
                {
                    if (logicResult.ErrorCode != (int)ErrorCodes.OK)
                    {
                        errs.Add(id, logicResult.ErrorCode);
                        errsDetail[id] = logicResult.Response;
                    }
                }
                else
                {
                    errs.Add(id, (int)ErrorCodes.Unknow);
                    errsDetail[id] = logicResult.Response;
                }
            }
            if (errs.Count > 0)
            {
                foreach (var err in ErrPriorityList)
                {
                   
                    if (!errs.Values.Contains(err))
                    {
                        continue;
                    }

                    var errs1 = errs.Where(e => e.Value == err);

                    var eName = "";//次错误所有人名字
                    int eValue = 0;//次错误值
                    ulong eKey = 0;//次错误Code
                    foreach (var e in errs1)
                    {
                        msg.Response.Items.Add(e.Key);
                        eKey = e.Key;
                        eValue = e.Value;
                        if (TeamNames.ContainsKey(e.Key))
                        {
                            eName += TeamNames[e.Key]+",";//有问题人的名字     
                        }       
                    }
                    msg.Reply(err, true);

                    if (eName.Length > 0)
                    {
                        eName = eName.Substring(0, eName.Length - 1);
                    }
                    

                    for (int i = 0; i < team.TeamDatas.Count; i++)
                    {
                        if (team.TeamDatas[i].Id != characterId)           //不是请求者自己
                        {
                            if (eValue == (int)ErrorCodes.Error_LevelNoEnough && errsDetail.ContainsKey(eKey) && errsDetail[eKey] != 0)  //如果是等级不足，且未打过副本
                            {
                                var _content = string.Format(Table.GetDictionary(errsDetail[eKey]).Desc[0], eName);
                                NoticeTeamMemberError(_content, team.TeamDatas[i].Id);
                            }
                            else//其他道具不足情况
                            {
                                NoticeTeamMemberError(eName, err, team.TeamDatas[i].Id);
                            }
                        }
                    }
                    //var dbSceneSimple = TeamServer.Instance.SceneAgent.GetSceneSimpleData(msg.Response.Items[0], 0);
                    //yield return dbSceneSimple.SendAndWaitUntilDone(coroutine);
                    
                    yield break;
                }
                {
                    var err = errs.First().Value;
                    var errs1 = errs.Where(e => e.Value == err);
                    foreach (var e in errs1)
                    {
                        msg.Response.Items.Add(e.Key);
                    }
                    msg.Reply(err, true);
                    yield break;
                }
            }
            //
            /*
        team.TeamDatas.Clear();
        foreach (var id in team.TeamList)
        {
            var dbSceneSimple = TeamServer.Instance.SceneAgent.GetSceneSimpleData(id, 0);
            yield return dbSceneSimple.SendAndWaitUntilDone(coroutine);

            if (dbSceneSimple.State != MessageState.Reply)
            {
                Logger.Error("In TeamEnterFuben(), GetSceneSimpleData return with dbSceneSimple.State = {0}",
                    dbSceneSimple.State);
                errs.Add(id, (int)ErrorCodes.Unknow);
                continue;
            }
            if (dbSceneSimple.ErrorCode != (int)ErrorCodes.OK)
            {
                Logger.Error("In TeamEnterFuben(), GetSceneSimpleData return with dbSceneSimple.ErrorCode = {0}",
                    dbSceneSimple.ErrorCode);
                errs.Add(id, dbSceneSimple.ErrorCode);
                continue;
            }

            var sceneData = dbSceneSimple.Response;
            team.TeamDatas.Add(sceneData);
            var tbScene = Table.GetScene(sceneData.SceneId);
            if (tbScene != null && tbScene.Type != (int)eSceneType.Normal && tbScene.Type != (int)eSceneType.City)
            {//已经在此副本内
                errs.Add(id, (int)ErrorCodes.Error_AlreadyInThisDungeon);
            }
        }
        if (errs.Count > 0)
        {
            foreach (var err in ErrPriorityList)
            {
                if (!errs.Values.Contains(err))
                {
                    continue;
                }
                var errs1 = errs.Where(e => e.Value == err);
                foreach (var e in errs1)
                {
                    msg.Response.Items.Add(e.Key);
                }
                msg.Reply(err, true);
                yield break;
            }
            {
                var err = errs.First().Value;
                var errs1 = errs.Where(e => e.Value == err);
                foreach (var e in errs1)
                {
                    msg.Response.Items.Add(e.Key);
                }
                msg.Reply(err, true);
                yield break;
            }
        }
*/
            //通知大家进入
            team.TeamEnterFuben(finalFubenId, serverId);
            Team.GetImpl().NotifyQueueMessage(characterId, -1, team.TeamDatas, team.mFubenResult);
            msg.Reply();

            //后台统计
            try
            {
                foreach (var uId in team.TeamList)
                {
                    string v = string.Format("fuben#{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                        uId,
                        finalFubenId,
                        tbFuben.Name,
                        tbFuben.AssistType,
                        0,  // 0  进入   1 完成   2 退出
                        DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                        -1,
                        -1); // 时间
                    PlayerLog.Kafka(v);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        //进副本的结果
        public IEnumerator ResultTeamEnterFuben(Coroutine coroutine,
                                                TeamCharacterProxy charProxy,
                                                ResultTeamEnterFubenInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            var fubenId = msg.Request.FubenID;
            var isOK = msg.Request.IsOk;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------ResultTeamEnterFuben----------{0},{1}",
                fubenId, isOK);
            var CharacterTeam = TeamManager.GetCharacterTeam(proxy.CharacterId);
            if (CharacterTeam == null)
            {
                //msg.Reply((int)ErrorCodes.Error_CharacterNoTeam);
                yield break;
            }
            CharacterTeam.team.TeamEnterFubenResult(proxy.CharacterId, fubenId, isOK);
        }

        //请求队伍数据
        public IEnumerator ApplyTeam(Coroutine coroutine, TeamCharacterProxy charProxy, ApplyTeamInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            var cId = msg.CharacterId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------ApplyTeam----------{0}", cId);
            var chracter = TeamManager.GetCharacterTeam(cId);
            if (chracter == null)
            {
                msg.Reply();
                yield break;
            }
            msg.Response.TeamId = chracter.team.TeamId;
            var tempList = chracter.team.TeamList.ToList();
            foreach (var uId in tempList)
            {
                var chracter2 = TeamManager.GetCharacterTeam(uId);
                if (chracter2 == null)
                {
                    Logger.Warn("ApplyTeam not find chracter={0}", uId);
                    continue;
                }
                //TODO
                var dbSceneSimple = TeamServer.Instance.SceneAgent.GetSceneSimpleData(uId, 0);
                yield return dbSceneSimple.SendAndWaitUntilDone(coroutine);
                if (dbSceneSimple.State != MessageState.Reply)
                {
                    continue;
                }
                if (dbSceneSimple.ErrorCode != (int)ErrorCodes.OK)
                {
                    continue;
                }
                var dbLogicSimple = TeamServer.Instance.LogicAgent.GetLogicSimpleData(uId, 0);
                yield return dbLogicSimple.SendAndWaitUntilDone(coroutine);
                if (dbLogicSimple.State != MessageState.Reply)
                {
                    continue;
                }
                if (dbLogicSimple.ErrorCode != (int)ErrorCodes.OK)
                {
                    continue;
                }

                var star = 0;
                dbLogicSimple.Response.Exdatas.TryGetValue((int)eExdataDefine.e688, out star);
                var cs = new CharacterSimpleInfo
                {
                    CharacterId = uId,
                    Name = dbSceneSimple.Response.Name,
                    Level = dbLogicSimple.Response.Level,
                    Type = dbLogicSimple.Response.TypeId,
                    RoleId = dbLogicSimple.Response.TypeId,
                    FightValue = dbSceneSimple.Response.FightPoint,
                    Ladder = dbLogicSimple.Response.Ladder,
                    OnLine = chracter2.TeamState != TeamState.Leaver,
                    Star = star,
                    SceneId = dbSceneSimple.Response.SceneId,
                };

                cs.EquipsModel.AddRange(dbLogicSimple.Response.EquipsModel);

                msg.Response.Teams.Add(cs);
            }
            msg.Reply();
        }

        public bool OnSyncRequested(TeamCharacterProxy proxy, ulong characterId, uint syncId)
        {
            return false;
        }

        //请求战盟信息
        public IEnumerator ApplyAllianceData(Coroutine coroutine,
                                             TeamCharacterProxy charProxy,
                                             ApplyAllianceDataInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            var aId = msg.Request.AllianceId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------ApplyAllianceData----------{0}", aId);
            var alliance = ServerAllianceManager.GetAllianceById(aId);
            if (alliance == null)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            if (alliance.State != TeamAllianceState.Already)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            var serverAlliance = ServerAllianceManager.GetAllianceByServer(alliance.ServerId);
            if (serverAlliance == null)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            var requestCharacter = serverAlliance.GetCharacterData(msg.CharacterId);
            if (requestCharacter == null)
            {
                Logger.Warn("ApplyAllianceData is not have proxy alliance[{0}]", aId);
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }
            alliance.GetAllianceData(msg.Response);

            var templist = new Uint64Array();
            foreach (var id in alliance.mDBData.Members)
            {
                templist.Items.Add(id);
            }
            if (templist.Items.Count < 1)
            {
                msg.Reply();
                yield break;
            }
            var isOnlineList = TeamServer.Instance.SceneAgent.SBCheckCharacterOnline(proxy.CharacterId, templist);
            yield return isOnlineList.SendAndWaitUntilDone(coroutine);
            if (isOnlineList.State != MessageState.Reply)
            {
                msg.Reply();
                yield break;
            }
            var index = 0;
            var OnlineList = new List<ulong>();
            if (isOnlineList.Response != null)
            {
                foreach (var i in isOnlineList.Response.Items)
                {
                    if (i == 1)
                    {
                        OnlineList.Add(templist.Items[index]);
                    }
                    index++;
                }
            }
            foreach (var id in alliance.mDBData.Members)
            {
                var member = serverAlliance.GetCharacterData(id);
                if (member == null)
                {
                    Logger.Error("alliance not find Character a={0},c={1}", alliance.AllianceId, id);
                    continue;
                }
                SceneSimpleData SceneSimple = null;
                if (string.IsNullOrEmpty(member.Name))
                {
                    var dbSceneSimple = TeamServer.Instance.SceneAgent.GetSceneSimpleData(id, 0);
                    yield return dbSceneSimple.SendAndWaitUntilDone(coroutine);
                    if (dbSceneSimple.State != MessageState.Reply)
                    {
                        continue;
                    }
                    if (dbSceneSimple.ErrorCode != (int)ErrorCodes.OK)
                    {
                        continue;
                    }
                    SceneSimple = dbSceneSimple.Response;
                }

                var tempNew = new AllianceMemberData();
                alliance.GetMemberData(tempNew, member, SceneSimple);
                tempNew.Online = OnlineList.Contains(id) ? 1 : 0;
                msg.Response.Members.Add(tempNew);
            }
            //List<ulong> delList =new List<ulong>();
            //foreach (var id in alliance.mDBData.Applys)
            //{
            //    var member = serverAlliance.GetCharacterData(id);
            //    if (member != null)
            //    {
            //        Logger.Error(" Character  have alliance a={0},c={1}", alliance.AllianceId, id);
            //        continue;
            //    }
            //    var dbSceneSimple = TeamServer.Instance.SceneAgent.GetSceneSimpleData(id, id);
            //    yield return dbSceneSimple.SendAndWaitUntilDone(coroutine);
            //    if (dbSceneSimple.State != MessageState.Reply)
            //    {
            //        continue;
            //    }
            //    AllianceMemberData tempNew = new AllianceMemberData();
            //    tempNew.Guid = id;
            //    tempNew.Name = dbSceneSimple.Response.Name;
            //    tempNew.Level = dbSceneSimple.Response.Level;
            //    tempNew.TypeId = dbSceneSimple.Response.TypeId;
            //    msg.Response.Applys.Add(tempNew);
            //}
            //foreach (ulong id in delList)
            //{
            //    Logger.Warn("have alliance character[{0}] in apply", id);
            //    alliance.mDBData.Applys.Remove(id);
            //}
            msg.Reply();
        }

        //获取战盟信息申请列表
        public IEnumerator ApplyAllianceEnjoyList(Coroutine coroutine,
                                                  TeamCharacterProxy charProxy,
                                                  ApplyAllianceEnjoyListInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            var aId = msg.Request.AllianceId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------ApplyAllianceMissionData----------{0}", aId);
            var alliance = ServerAllianceManager.GetAllianceById(aId);
            if (alliance == null)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            if (alliance.State != TeamAllianceState.Already)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            var serverAlliance = ServerAllianceManager.GetAllianceByServer(alliance.ServerId);
            if (serverAlliance == null)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            var requestCharacter = serverAlliance.GetCharacterData(msg.CharacterId);
            if (requestCharacter == null)
            {
                Logger.Warn("ApplyAllianceData is not have proxy alliance[{0}]", aId);
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }
            foreach (var id in alliance.mDBData.Applys)
            {
                var member = serverAlliance.GetCharacterData(id);
                if (member != null)
                {
                    Logger.Info(" Character  have alliance a={0},c={1}", alliance.AllianceId, id);
                    continue;
                }
                var dbSceneSimple = TeamServer.Instance.SceneAgent.GetSceneSimpleData(id, 0);
                yield return dbSceneSimple.SendAndWaitUntilDone(coroutine);
                if (dbSceneSimple.State != MessageState.Reply)
                {
                    continue;
                }
                var tempNew = new AllianceMemberData();
                tempNew.Guid = id;
                tempNew.Name = dbSceneSimple.Response.Name;
                tempNew.Level = dbSceneSimple.Response.Level;
                tempNew.TypeId = dbSceneSimple.Response.TypeId;
                tempNew.FightPoint = dbSceneSimple.Response.FightPoint;
                tempNew.SceneId = dbSceneSimple.Response.SceneId;
                tempNew.Online = dbSceneSimple.Response.Online;
                tempNew.LostTime = dbSceneSimple.Response.LastTime;
                msg.Response.Applys.Add(tempNew);
            }
            msg.Reply();
        }

        public IEnumerator BattleUnionDepotArrange(Coroutine coroutine,
                                                   TeamCharacterProxy charProxy,
                                                   BattleUnionDepotArrangeInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            var allianceId = msg.Request.AllianceId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------BattleUnionDepotArrange----------{0}", allianceId);
            var alliance = ServerAllianceManager.GetAllianceById(allianceId);
            if (alliance == null)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            if (alliance.State != TeamAllianceState.Already)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            var serverAlliance = ServerAllianceManager.GetAllianceByServer(alliance.ServerId);
            if (serverAlliance == null)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            var requestCharacter = serverAlliance.GetCharacterData(msg.CharacterId);
            if (requestCharacter == null)
            {
                Logger.Warn("ApplyAllianceData is not have proxy alliance[{0}]", allianceId);
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }
            alliance.DepotArrange(requestCharacter.Name);
            msg.Reply((int)ErrorCodes.OK);
        }

        //获取战盟捐献记录
        public IEnumerator ApplyAllianceDonationList(Coroutine coroutine,
                                                     TeamCharacterProxy charProxy,
                                                     ApplyAllianceDonationListInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            var aId = msg.Request.AllianceId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------ApplyAllianceMissionData----------{0}", aId);
            var alliance = ServerAllianceManager.GetAllianceById(aId);
            if (alliance == null)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            if (alliance.State != TeamAllianceState.Already)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            var serverAlliance = ServerAllianceManager.GetAllianceByServer(alliance.ServerId);
            if (serverAlliance == null)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            var requestCharacter = serverAlliance.GetCharacterData(msg.CharacterId);
            if (requestCharacter == null)
            {
                Logger.Warn("ApplyAllianceData is not have proxy alliance[{0}]", aId);
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }

            alliance.GetAllianceDonationData(msg.Response);
            msg.Reply();
        }

        public IEnumerator ApplyAllianceDepotLogList(Coroutine coroutine,
                                                     TeamCharacterProxy charProxy,
                                                     ApplyAllianceDepotLogListInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            var aId = msg.Request.AllianceId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------ApplyAllianceDepotLogList----------{0}", aId);
            var alliance = ServerAllianceManager.GetAllianceById(aId);
            if (alliance == null)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            if (alliance.State != TeamAllianceState.Already)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            var serverAlliance = ServerAllianceManager.GetAllianceByServer(alliance.ServerId);
            if (serverAlliance == null)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            var requestCharacter = serverAlliance.GetCharacterData(msg.CharacterId);
            if (requestCharacter == null)
            {
                Logger.Warn("ApplyAllianceDepotLogList is not have alliance[{0}]", aId);
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }

            alliance.GetAllianceDepotLogData(msg.Response);
            msg.Reply();
        }

        public IEnumerator ApplyAllianceDepotData(Coroutine coroutine,
                                             TeamCharacterProxy charProxy,
                                             ApplyAllianceDepotDataInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            var aId = msg.Request.AllianceId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------ApplyAllianceMissionData----------{0}", aId);
            var alliance = ServerAllianceManager.GetAllianceById(aId);
            if (alliance == null)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            if (alliance.State != TeamAllianceState.Already)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            var serverAlliance = ServerAllianceManager.GetAllianceByServer(alliance.ServerId);
            if (serverAlliance == null)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            var requestCharacter = serverAlliance.GetCharacterData(msg.CharacterId);
            if (requestCharacter == null)
            {
                Logger.Warn("ApplyAllianceData is not have proxy alliance[{0}]", aId);
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }

            alliance.GetAllianceDepotData(msg.Response);
            msg.Reply();
        }

        public IEnumerator BattleUnionDepotClearUp(Coroutine coroutine,
                                                   TeamCharacterProxy charProxy,
                                                   BattleUnionDepotClearUpInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            var aId = msg.Request.AllianceId;
            var info = msg.Request.Info;
            var characterId = msg.CharacterId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------BattleUnionDepotClearUp----------{0}", aId);
            var alliance = ServerAllianceManager.GetAllianceById(aId);
            if (alliance == null)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            if (alliance.State != TeamAllianceState.Already)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            var serverAlliance = ServerAllianceManager.GetAllianceByServer(alliance.ServerId);
            if (serverAlliance == null)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            var requestCharacter = serverAlliance.GetCharacterData(msg.CharacterId);
            if (requestCharacter == null)
            {
                Logger.Warn("ApplyAllianceData is not have proxy alliance[{0}]", aId);
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }
            var result = alliance.DepotClearUp(characterId, requestCharacter.Name, info);
            if (result != (int)ErrorCodes.OK)
            {
                msg.Reply((int)result);
                yield break;
            }
            msg.Reply();
        }

        public IEnumerator BattleUnionRemoveDepotItem(Coroutine coroutine,
                                                      TeamCharacterProxy charProxy,
                                                      BattleUnionRemoveDepotItemInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            var aId = msg.Request.AllianceId;
            var itemId = msg.Request.ItemId;
            var bagIndex = msg.Request.BagIndex;
            var characterId = msg.CharacterId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------BattleUnionRemoveDepotItem----------{0}", aId);
            var alliance = ServerAllianceManager.GetAllianceById(aId);
            if (alliance == null)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            if (alliance.State != TeamAllianceState.Already)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            var serverAlliance = ServerAllianceManager.GetAllianceByServer(alliance.ServerId);
            if (serverAlliance == null)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            var requestCharacter = serverAlliance.GetCharacterData(msg.CharacterId);
            if (requestCharacter == null)
            {
                Logger.Warn("ApplyAllianceData is not have proxy alliance[{0}]", aId);
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }
            var result = alliance.DepotItemRemove(characterId, requestCharacter.Name, bagIndex, itemId);
            if (result != (int)ErrorCodes.OK)
            {
                msg.Reply((int)result);
                yield break;
            }
            msg.Reply();
        }

        //请求战盟信息任务数据
        public IEnumerator ApplyAllianceMissionData(Coroutine coroutine,
                                                    TeamCharacterProxy charProxy,
                                                    ApplyAllianceMissionDataInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            var aId = msg.Request.AllianceId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------ApplyAllianceMissionData----------{0}", aId);
            var alliance = ServerAllianceManager.GetAllianceById(aId);
            if (alliance == null)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            if (alliance.State != TeamAllianceState.Already)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            var serverAlliance = ServerAllianceManager.GetAllianceByServer(alliance.ServerId);
            if (serverAlliance == null)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            var requestCharacter = serverAlliance.GetCharacterData(msg.CharacterId);
            if (requestCharacter == null)
            {
                Logger.Warn("ApplyAllianceData is not have proxy alliance[{0}]", aId);
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }
            alliance.GetAllianceMissionData(msg.Response.Missions);
            msg.Reply();
        }

        //请求战盟信息
        public IEnumerator ApplyAllianceDataByServerId(Coroutine coroutine,
                                                       TeamCharacterProxy charProxy,
                                                       ApplyAllianceDataByServerIdInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            var sId = msg.Request.ServerId;
            var type = msg.Request.Type;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------ApplyAllianceDataByServerId----------{0},{1}",
                sId, type);
            var serverAlliance = ServerAllianceManager.GetAllianceByServer(sId);
            if (serverAlliance == null)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            var requestCharacter = serverAlliance.GetCharacterData(msg.CharacterId);
            if (requestCharacter == null)
            {
                Logger.Warn("ApplyAllianceDataByServerId is not apply proxy alliance[{0}]", msg.CharacterId);
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }
            var alliance = ServerAllianceManager.GetAllianceById(requestCharacter.AllianceId);
            if (alliance == null)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            if (alliance.State != TeamAllianceState.Already)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            alliance.GetAllianceData(msg.Response);
            //0战盟详细数据   1战盟简单数据
            if (type == 1)
            {
                msg.Reply((int)ErrorCodes.OK);
                yield break;
            }

            var templist = new Uint64Array();
            bool bSure = false;
            var characterId = msg.CharacterId;
            Dictionary<ulong, int> dic = new Dictionary<ulong, int>();

            foreach (var id in alliance.mDBData.Members)
            {
                dic.modifyValue(id, 1);
                templist.Items.Add(id);
                if (id == characterId)
                    bSure = true;
            }
            {//这里又是一个补丁,去重用的
                foreach (var v in dic)
                {
                    if (v.Value > 1)
                    {
                        alliance.mDBData.Members.RemoveAll(o => o == v.Key);
                        alliance.mDBData.Members.Add(v.Key);
                        alliance.SetFlag();
                    }
                }
            }
            if (bSure == false)
            {
                //修复流程(在成员列表中找不到这个id)
                //1. 遍历这个服务器的所有战盟,看这个人到底在哪里
                //2. 把这个人身上的战盟数据改正确
                bool allianceFound = false;
                foreach (var alliance1 in alliance.Dad.Alliances)
                {
                    if (alliance1.Value.mDBData.Members.Contains(characterId))
                    {
                        allianceFound = true;
                        requestCharacter.AllianceId = alliance1.Key;
                    }
                }

                if (!allianceFound)
                {
                    serverAlliance.RemoveCharacterData(characterId);
                }

                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }

            if (templist.Items.Count < 1)
            {
                msg.Reply();
                yield break;
            }
            var isOnlineList = TeamServer.Instance.SceneAgent.SBCheckCharacterOnline(proxy.CharacterId, templist);
            yield return isOnlineList.SendAndWaitUntilDone(coroutine);
            if (isOnlineList.State != MessageState.Reply)
            {
                msg.Reply();
                yield break;
            }
            var index = 0;
            //请求申请数据
            var OnlineList = new List<ulong>();
            if (isOnlineList.Response != null)
            {
                foreach (var i in isOnlineList.Response.Items)
                {
                    if (i == 1)
                    {
                        OnlineList.Add(templist.Items[index]);
                    }
                    index++;
                }
            }
            foreach (var id in alliance.mDBData.Members)
            {
                var member = serverAlliance.GetCharacterData(id);
                if (member == null)
                {
                    Logger.Error(" Character  not have alliance a={0},c={1}", alliance.AllianceId, id);
                    continue;
                }

                SceneSimpleData SceneSimple = null;
                if (string.IsNullOrEmpty(member.Name))
                {
                    var dbSceneSimple = TeamServer.Instance.SceneAgent.GetSceneSimpleData(id, 0);
                    yield return dbSceneSimple.SendAndWaitUntilDone(coroutine);
                    if (dbSceneSimple.State != MessageState.Reply)
                    {
                        continue;
                    }
                    if (dbSceneSimple.ErrorCode != (int)ErrorCodes.OK)
                    {
                        continue;
                    }
                    SceneSimple = dbSceneSimple.Response;
                }

                var tempNew = new AllianceMemberData();
                alliance.GetMemberData(tempNew, member, SceneSimple);
                {// 修正玩家转生等级ladder
                    var simpleData = TeamServer.Instance.SceneAgent.GetSceneSimpleData(id, 0);
                    yield return simpleData.SendAndWaitUntilDone(coroutine);
                    if (simpleData.State != MessageState.Reply)
                    {
                        continue;
                    }
                    if (simpleData.ErrorCode == (int)ErrorCodes.OK)
                    {
                        tempNew.RebornLadder = simpleData.Response.Ladder;
                    }
                }

                tempNew.Online = OnlineList.Contains(id) ? 1 : 0;
                msg.Response.Members.Add(tempNew);
            }
            //请求申请数据
            //List<ulong> delList = new List<ulong>();
            //foreach (var id in alliance.mDBData.Applys)
            //{
            //    var member = serverAlliance.GetCharacterData(id);
            //    if (member != null)
            //    {
            //        Logger.Error(" Character  have alliance a={0},c={1}", alliance.AllianceId, id);
            //        continue;
            //    }
            //    var dbSceneSimple = TeamServer.Instance.SceneAgent.GetSceneSimpleData(id, id);
            //    yield return dbSceneSimple.SendAndWaitUntilDone(coroutine);
            //    if (dbSceneSimple.State != MessageState.Reply)
            //    {
            //        continue;
            //    }
            //    AllianceMemberData tempNew = new AllianceMemberData();
            //    tempNew.Guid = id;
            //    tempNew.Name = dbSceneSimple.Response.Name;
            //    tempNew.Level = dbSceneSimple.Response.Level;
            //    tempNew.TypeId = dbSceneSimple.Response.TypeId;
            //    msg.Response.Applys.Add(tempNew);
            //}
            //foreach (ulong id in delList)
            //{
            //    Logger.Warn("have alliance character[{0}] in apply", id);
            //    alliance.mDBData.Applys.Remove(id);
            //}
            msg.Reply();
        }

        //修改战盟公告
        public IEnumerator ChangeAllianceNotice(Coroutine coroutine,
                                                TeamCharacterProxy charProxy,
                                                ChangeAllianceNoticeInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            var aId = msg.Request.AllianceId;
            var content = msg.Request.Content;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------ChangeAllianceNotice----------{0},{1}", aId,
                content);
            var alliance = ServerAllianceManager.GetAllianceById(aId);
            if (alliance == null)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            if (alliance.State != TeamAllianceState.Already)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            var serverAlliance = ServerAllianceManager.GetAllianceByServer(alliance.ServerId);
            if (serverAlliance == null)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            var requestCharacter = serverAlliance.GetCharacterData(msg.CharacterId);
            if (requestCharacter == null)
            {
                Logger.Warn("ChangeAllianceNotice is not apply proxy alliance[{0}]", aId);
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }

            var tbGA = Table.GetGuildAccess(requestCharacter.Ladder);
            if (tbGA == null)
            {
                //查表错误，数据溢出
                msg.Reply((int)ErrorCodes.Error_DataOverflow);
                yield break;
            }
            if (tbGA.CanModifyNotice == 0)
            {
                msg.Reply((int)ErrorCodes.Error_JurisdictionNotEnough);
                yield break;
            }

            bool bNormal = content.IndexOf("{!") == -1 && content.IndexOf("!}") == -1;

            var monitorData = TeamServer.Instance.LogicAgent.GetPlayerMoniterData(msg.CharacterId, msg.CharacterId);
            yield return monitorData.SendAndWaitUntilDone(coroutine);
            if (monitorData == null || monitorData.Response == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }
            var dbLoginSimple = TeamServer.Instance.LoginAgent.GetLoginSimpleData(proxy.ClientId, msg.CharacterId);
            yield return dbLoginSimple.SendAndWaitUntilDone(coroutine);
            #region 37监测
            if (monitorData.Response.channel.Equals("37") && bNormal == true)
            {
                #region cm3
                {//CM3
                    var unixTimer = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
                    var dic = new Dictionary<string, string>();
                    dic.Add("time", Math.Ceiling(unixTimer).ToString());
                    dic.Add("uid", monitorData.Response.uid);
                    dic.Add("gid", monitorData.Response.gid.ToString());
                    dic.Add("dsid", dbLoginSimple.Response.ServerId.ToString());
                    
                    var md5Key = "Ob7mD7HqInhGxTNt";
                    dic.Add("type", "8");
                       
                    var strsign = string.Format("{0}{1}{2}{3}{4}{5}", md5Key, monitorData.Response.uid, monitorData.Response.gid, dbLoginSimple.Response.ServerId, Math.Ceiling(unixTimer), dic["type"]);//md5Key + "}{" + proxy.Character.moniterData.uid + "}{""}{" + proxy.Character.ServerId.ToString()+ "}{" + unixTimer + "}{" + dic["type"] + "}");
                    var sign = RequestManager.Encrypt_MD5_UTF8(strsign);
                    dic.Add("sign", sign);
                    dic.Add("actor_name", HttpUtility.UrlEncode(dbLoginSimple.Response.Name));
                    dic.Add("actor_id", dbLoginSimple.Response.Id.ToString());
                    dic.Add("chat_time", Math.Ceiling(unixTimer).ToString());
                    dic.Add("content", HttpUtility.UrlEncode(content));
                    var url = @"http://cm3.api.37.com.cn/Content/_checkContent";
                    var result = AsyncReturnValue<string>.Create();
                    yield return TeamManager.WebRequestManager.DoRequest(coroutine, url, dic, result);

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
                    var chatType = 8;
                    chatType = 8;
                          
                    List<KeyValuePair<string, string>> param = new List<KeyValuePair<string, string>>();
                    param.Add(new KeyValuePair<string, string>("time", strTimer));
                    param.Add(new KeyValuePair<string, string>("uid", monitorData.Response.uid));
                    param.Add(new KeyValuePair<string, string>("gid", monitorData.Response.gid.ToString()));
                    param.Add(new KeyValuePair<string, string>("dsid", HttpUtility.UrlEncode(dbLoginSimple.Response.ServerId.ToString())));
                    param.Add(new KeyValuePair<string, string>("type", chatType.ToString()));
                    param.Add(new KeyValuePair<string, string>("actor_name", HttpUtility.UrlEncode(dbLoginSimple.Response.Name)));
                    param.Add(new KeyValuePair<string, string>("actor_id", dbLoginSimple.Response.Id.ToString()));

                    param.Add(new KeyValuePair<string, string>("to_uid", ""));
                    param.Add(new KeyValuePair<string, string>("to_actor_name", ""));
                    param.Add(new KeyValuePair<string, string>("content", HttpUtility.UrlEncode(content)));
                    param.Add(new KeyValuePair<string, string>("chat_time", strTimer));
                    param.Add(new KeyValuePair<string, string>("user_ip", monitorData.Response.ip));
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


            alliance.SetNotice(content);
            msg.Reply();
        }

        //获得本服务器的战盟
        public IEnumerator GetServerAlliance(Coroutine coroutine,
                                             TeamCharacterProxy charProxy,
                                             GetServerAllianceInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            var sId = msg.Request.ServerId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------GetServerAlliance----------{0}", sId);
            var serverAlliance = ServerAllianceManager.GetAllianceByServer(sId);
            if (serverAlliance == null)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            foreach (var alliance in serverAlliance.Alliances)
            {
                if (alliance.Value.State != TeamAllianceState.Already)
                {
                    continue;
                }

                var dbSceneSimple = TeamServer.Instance.SceneAgent.GetSceneSimpleData(alliance.Value.Leader, 0);
                yield return dbSceneSimple.SendAndWaitUntilDone(coroutine);
                if (dbSceneSimple.State != MessageState.Reply)
                {
                    continue;
                }
                if (dbSceneSimple.ErrorCode != (int)ErrorCodes.OK)
                {
                    continue;
                }
                //if (dbSceneSimple.Response == null)
                //{
                //    continue;
                //}
                var temp = new AllianceSimpleData();
                temp.Id = alliance.Value.AllianceId;
                temp.Name = alliance.Value.Name;
                temp.Leader = alliance.Value.Leader;
                temp.LeaderName = dbSceneSimple.Response.Name;
                temp.Level = alliance.Value.Level;
                temp.NowCount = alliance.Value.GetMemberCount();
                temp.MaxCount = alliance.Value.GetMemberMaxCount();
                temp.FightPoint = alliance.Value.GetTotleFightPoint();
                temp.AutoAgree = alliance.Value.AutoAgree;
                msg.Response.Alliances.Add(temp);
            }

            msg.Reply();
        }

        //修改角色权限
        public IEnumerator ChangeJurisdiction(Coroutine coroutine,
                                              TeamCharacterProxy charProxy,
                                              ChangeJurisdictionInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            var aId = msg.Request.AllianceId;
            var guid = msg.Request.Guid;
            var type = msg.Request.Type;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------ChangeJurisdiction----------{0},{1},{2}", aId,
                guid, type);
            var alliance = ServerAllianceManager.GetAllianceById(aId);
            if (alliance == null)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            if (alliance.State != TeamAllianceState.Already)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            var serverAlliance = ServerAllianceManager.GetAllianceByServer(alliance.ServerId);
            if (serverAlliance == null)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            var requestCharacter = serverAlliance.GetCharacterData(msg.CharacterId);
            if (requestCharacter == null)
            {
                Logger.Warn("ChangeJurisdiction requestCharacter is not have proxy alliance[{0}]", aId);
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }

            var operationCharacter = serverAlliance.GetCharacterData(guid);
            if (operationCharacter == null)
            {
                Logger.Warn("ChangeJurisdiction operationCharacter is not have proxy alliance[{0}]", guid);
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }
            if (requestCharacter.AllianceId != aId)
            {
                Logger.Warn("ChangeJurisdiction requestCharacter is not same! requestAlliance={0},proxyAlliance={1}",
                    requestCharacter.AllianceId, aId);
                msg.Reply((int)ErrorCodes.Error_AllianceIsNotSame);
                yield break;
            }
            if (operationCharacter.AllianceId != aId)
            {
                Logger.Warn("ChangeJurisdiction requestCharacter is not same! operationAlliance={0},proxyAlliance={1}",
                    operationCharacter.AllianceId, aId);
                msg.Reply((int)ErrorCodes.Error_AllianceIsNotSame);
                yield break;
            }
            var err = alliance.ChangeJurisdiction(requestCharacter, operationCharacter, type);
            msg.Reply((int)err);
        }

        //修改战盟设置为是否自动同意申请
        public IEnumerator ChangeAllianceAutoJoin(Coroutine coroutine,
                                                  TeamCharacterProxy charProxy,
                                                  ChangeAllianceAutoJoinInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            var aId = msg.Request.AllianceId;
            var value = msg.Request.Value;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------ChangeAllianceAutoJoin----------{0},{1}", aId,
                value);
            var alliance = ServerAllianceManager.GetAllianceById(aId);
            if (alliance == null)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            if (alliance.State != TeamAllianceState.Already)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            var serverAlliance = ServerAllianceManager.GetAllianceByServer(alliance.ServerId);
            if (serverAlliance == null)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            var requestCharacter = serverAlliance.GetCharacterData(msg.CharacterId);
            if (requestCharacter == null)
            {
                Logger.Warn("ChangeJurisdiction requestCharacter is not have proxy alliance[{0}]", aId);
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }

            if (requestCharacter.AllianceId != aId)
            {
                Logger.Warn("ChangeJurisdiction requestCharacter is not same! requestAlliance={0},proxyAlliance={1}",
                    requestCharacter.AllianceId, aId);
                msg.Reply((int)ErrorCodes.Error_AllianceIsNotSame);
                yield break;
            }
            alliance.ChangeAllianceAutoJoin(requestCharacter, value);
            msg.Reply();
        }

        //批量同意的战盟申请
        public IEnumerator AllianceAgreeApplyList(Coroutine coroutine,
                                                  TeamCharacterProxy charProxy,
                                                  AllianceAgreeApplyListInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            var aId = msg.Request.AllianceId;
            var type = msg.Request.Type;
            var guid = msg.Request.Guids;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------AllianceAgreeApplyList----------{0}", aId);
            foreach (var item in guid.Items)
            {
                PlayerLog.WriteLog(proxy.CharacterId,
                    "----------Team----------AllianceAgreeApplyList----------guids:{0}", item);
            }
            var alliance = ServerAllianceManager.GetAllianceById(aId);
            if (alliance == null)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            if (alliance.State != TeamAllianceState.Already)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            var serverAlliance = ServerAllianceManager.GetAllianceByServer(alliance.ServerId);
            if (serverAlliance == null)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            var requestCharacter = serverAlliance.GetCharacterData(msg.CharacterId);
            if (requestCharacter == null)
            {
                Logger.Warn("ChangeJurisdiction requestCharacter is not have proxy alliance[{0}]", aId);
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }

            if (requestCharacter.AllianceId != aId)
            {
                Logger.Warn("ChangeJurisdiction requestCharacter is not same! requestAlliance={0},proxyAlliance={1}",
                    requestCharacter.AllianceId, aId);
                msg.Reply((int)ErrorCodes.Error_AllianceIsNotSame);
                yield break;
            }
            ErrorCodes result;
            if (type == 0)
            {
                result = alliance.AllianceAgreeApplyList(requestCharacter, guid.Items);
            }
            else
            {
                result = alliance.AllianceRefuseApplyList(requestCharacter, guid.Items);
            }
            msg.Reply((int)result);
        }
        public IEnumerator ClearAllianceApplyList(Coroutine coroutine,
                                                  TeamCharacterProxy charProxy,
                                                  ClearAllianceApplyListInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            var aId = msg.Request.AllianceId;
            var guid = msg.Request.Guids;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------ClearAllianceApplyList----------{0}", aId);
            foreach (var item in guid.Items)
            {
                PlayerLog.WriteLog(proxy.CharacterId,
                    "----------Team----------ClearAllianceApplyList----------guids:{0}", item);
            }
            var alliance = ServerAllianceManager.GetAllianceById(aId);
            if (alliance == null)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            if (alliance.State != TeamAllianceState.Already)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }
            var serverAlliance = ServerAllianceManager.GetAllianceByServer(alliance.ServerId);
            if (serverAlliance == null)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            var requestCharacter = serverAlliance.GetCharacterData(msg.CharacterId);
            if (requestCharacter == null)
            {
                Logger.Warn("ChangeJurisdiction requestCharacter is not have proxy alliance[{0}]", aId);
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }

            if (requestCharacter.AllianceId != aId)
            {
                Logger.Warn("ChangeJurisdiction requestCharacter is not same! requestAlliance={0},proxyAlliance={1}",
                    requestCharacter.AllianceId, aId);
                msg.Reply((int)ErrorCodes.Error_AllianceIsNotSame);
                yield break;
            }
            ErrorCodes result;
            result = alliance.ClearAllianceApplyList(requestCharacter,guid.Items);
            msg.Reply((int)result);
        }

        ////升级联盟Buff
        //public IEnumerator UpgradeAllianceBuff(Coroutine coroutine, TeamCharacterProxy charProxy,  UpgradeAllianceBuffInMessage msg)
        //{
        //TeamProxy proxy = (TeamProxy)charProxy;
        //int aId = msg.Request.AllianceId;
        //PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------UpgradeAllianceBuff----------{0}", aId);
        //var a = ServerAllianceManager.GetAllianceById(aId);
        //if (a == null)
        //{
        //    msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
        //    yield break;
        //}

        //int buffId = msg.Request.BuffId;
        //ErrorCodes result = a.UpgradeBuff(msg.CharacterId, buffId);
        //msg.Reply((int)result);
        //    yield break;
        //}

        //升级战盟等级
        public IEnumerator UpgradeAllianceLevel(Coroutine coroutine,
                                                TeamCharacterProxy charProxy,
                                                UpgradeAllianceLevelInMessage msg)
        {
            var proxy = (TeamProxy)charProxy;
            var aId = msg.Request.AllianceId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------UpgradeAllianceLevel----------{0}", aId);
            var a = ServerAllianceManager.GetAllianceById(aId);
            if (a == null)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }

            var result = a.UpgradeAllianceLevel(msg.CharacterId);
            msg.Reply((int)result);
        }

        public IEnumerator ApplyAllianceWarOccupantData(Coroutine coroutine,
                                                        TeamCharacterProxy _this,
                                                        ApplyAllianceWarOccupantDataInMessage msg)
        {
            var proxy = (TeamProxy)_this;
            var response = msg.Response;
            var serverId = msg.Request.ServerId;
            serverId = SceneExtension.GetServerLogicId(serverId);
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------ApplyAllianceWarOccupantData----------{0}",
                serverId);
            var allianceManager = ServerAllianceManager.GetAllianceByServer(serverId);
            if (allianceManager == null)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            var dbAllianceWar = allianceManager.GetServerData(serverId);
            if (dbAllianceWar == null)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            var occupantId = dbAllianceWar.Occupant;
            var occupant = ServerAllianceManager.GetAllianceById(occupantId);
            if (occupant != null)
            {
                //有工会占领王城
                response.OccupantId = occupantId;
                response.OccupantName = occupant.Name;
            }
            msg.Reply();
        }

        public IEnumerator ApplyAllianceWarChallengerData(Coroutine coroutine,
                                                          TeamCharacterProxy _this,
                                                          ApplyAllianceWarChallengerDataInMessage msg)
        {
            var proxy = (TeamProxy)_this;
            if (proxy == null)
            {
                Logger.Error("----Team----ApplyAllianceWarChallengerData-----proxy == null--characterId:{0}",
                    msg.CharacterId);
                yield break;
            }
            var response = msg.Response;
            var serverId = msg.Request.ServerId;
            serverId = SceneExtension.GetServerLogicId(serverId);
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------ApplyAllianceWarChallengerData----------{0}",
                serverId);
            var allianceManager = ServerAllianceManager.GetAllianceByServer(serverId);
            if (allianceManager == null)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            var dbAllianceWar = allianceManager.GetServerData(serverId);
            if (dbAllianceWar == null)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            foreach (var id in dbAllianceWar.Challengers)
            {
                var a = ServerAllianceManager.GetAllianceById(id);
                if (a == null)
                {
                    continue;
                }
                response.ChallengerId.Add(id);
                response.ChallengerName.Add(a.Name);
            }
            msg.Reply();
        }

        public IEnumerator ApplyAllianceWarData(Coroutine coroutine,
                                                TeamCharacterProxy _this,
                                                ApplyAllianceWarDataInMessage msg)
        {
            var response = msg.Response;
            var characterId = msg.CharacterId;
            var serverId = msg.Request.ServerId;
            serverId = SceneExtension.GetServerLogicId(serverId);
            PlayerLog.WriteLog((ulong)LogType.AllianceWar,
                "In ApplyAllianceWarData() - 0, characterId = {0}, serverId = {1}", characterId, serverId);
            var allianceManager = ServerAllianceManager.GetAllianceByServer(serverId);
            if (allianceManager == null)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            var dbAllianceWar = allianceManager.GetServerData(serverId);
            if (dbAllianceWar == null)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            var occupantId = dbAllianceWar.Occupant;
            var alliance = ServerAllianceManager.GetAllianceById(occupantId);
            if (alliance != null)
            {
                //有工会占领王城
                PlayerLog.WriteLog((ulong)LogType.AllianceWar,
                    "In ApplyAllianceWarData() - 1, characterId = {0}, serverId = {1}, occupantId = {2}", characterId,
                    serverId, occupantId);
                var serverAlliance = alliance.Dad;
                foreach (var id in alliance.mDBData.Members)
                {
                    var member = serverAlliance.GetCharacterData(id);
                    if (member == null)
                    {
                        Logger.Error("alliance not find Character a={0},c={1}", alliance.AllianceId, id);
                        continue;
                    }
                    if (member.Ladder >= (int)eAllianceLadder.Elder)
                    {
                        response.Members.Add(new AllianceMemberSimpleData
                        {
                            Guid = member.Guid,
                            Ladder = member.Ladder,
                            Name = member.Name
                        });
                    }
                }
            }
            response.SignUpCount = dbAllianceWar.BidDatas.Count;
            response.State = AllianceWarManager.WarDatas[serverId].GetStatus();
            response.OpenTime = dbAllianceWar.OpenTime;
            msg.Reply();
        }

        public IEnumerator BidAllianceWar(Coroutine coroutine, TeamCharacterProxy _this, BidAllianceWarInMessage msg)
        {
            var proxy = (TeamProxy)_this;
            var characterId = msg.CharacterId;
            var alliance = ServerAllianceManager.GetAllianceByCharacterId(characterId);
            var myprice = msg.Request.Price;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Team----------ApplyAllianceWarChallengerData----------{0}",
                myprice);
            if (alliance == null)
            {
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }
            var allianceId = alliance.AllianceId;
            var serverId = alliance.ServerId;
            serverId = SceneExtension.GetServerLogicId(serverId);
            var allianceManager = ServerAllianceManager.GetAllianceByServer(serverId);
            if (allianceManager == null)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }

            var dbAllianceNew = allianceManager.GetServerData(serverId);
            if (dbAllianceNew == null)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
            if (allianceId == dbAllianceNew.Occupant)
            {
                msg.Reply((int)ErrorCodes.OK);
                yield break;
            }
            var bidDatas = dbAllianceNew.BidDatas;
            var now = DateTime.Now;
            var weekDay = (int)now.DayOfWeek;
            /*
            weekDay = (weekDay + 1) % 7;
            var flag = AllianceWarManager.FightWeekDay.GetFlag(weekDay);
            if (flag != 1)
            {
                //当前不是竞标日
                msg.Reply((int)ErrorCodes.Error_NotBidTime);
                yield break;
            }
            */
            //AllianceWarManager.WarDatas[serverId]
            AllianceWar war = null;
            if (!AllianceWarManager.WarDatas.TryGetValue(serverId, out war))
            {
                //当前不是竞标日
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            if (war.GetStatus() != (int)eAllianceWarState.Bid)
            {
                //当前不是竞标日
                msg.Reply((int)ErrorCodes.Error_NotBidTime);
                yield break;
            }

            //检查权限
            var one = alliance.Dad.GetCharacterData(msg.CharacterId);
            var tbGA = Table.GetGuildAccess(one.Ladder);
            if (tbGA == null)
            {
                //查表错误，数据溢出
                msg.Reply((int)ErrorCodes.Error_DataOverflow);
                yield break;
            }
            if (tbGA.CanModifyAttackCity != 1)
            {
                //权限不足
                msg.Reply((int)ErrorCodes.Error_JurisdictionNotEnough);
                yield break;
            }

            int price;
            bidDatas.TryGetValue(allianceId, out price);
            var addPrice = myprice;
            if (addPrice == 0)
            {
                msg.Response = price;
                msg.Reply();
                yield break;
            }
            var tbGuild = Table.GetGuild(alliance.Level);
            if (tbGuild == null)
            {
                msg.Reply((int)ErrorCodes.Error_DataOverflow);
                yield break;
            }
            if (alliance.Money - addPrice < tbGuild.MaintainMoney)
            {
                //战盟资金不足
                msg.Reply((int)ErrorCodes.Error_AllianceMoneyNotEnough);
                yield break;
            }
            price += addPrice;
            if (price < AllianceWarManager.BidMin)
            {
                //未达到竞标门槛
                msg.Reply((int)ErrorCodes.Error_BidThreshold);
                yield break;
            }
            //扣钱
            alliance.Money -= addPrice;
            alliance.SetFlag();
            bidDatas[allianceId] = price;
            msg.Response = price;
            msg.Reply();

            PlayerLog.WriteLog((ulong)LogType.AllianceWar,
                "In BidAllianceWar(), characterId = {0}, ladder = {1}, addPrice = {2}, totalPrice = {3}, allLeftMoney = {4}",
                characterId,
                one.Ladder,
                addPrice, price, alliance.Money);
        }

        public IEnumerator EnterAllianceWar(Coroutine co, TeamCharacterProxy _this, EnterAllianceWarInMessage msg)
        {
            var characterId = msg.CharacterId;
            var alliance = ServerAllianceManager.GetAllianceByCharacterId(characterId);
            if (alliance == null)
            {
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }
            var allianceId = alliance.AllianceId;
            var serverId = alliance.ServerId;
            serverId = SceneExtension.GetServerLogicId(serverId);
            var war = AllianceWarManager.WarDatas[serverId];
            var err = war.CheckPlayerEnter(serverId, characterId);
            if (err != ErrorCodes.OK)
            {
                msg.Reply((int)err);
                yield break;
            }
            var asyncErr = AsyncReturnValue<ErrorCodes>.Create();
            var co1 = CoroutineFactory.NewSubroutine(war.PlayerEnter, co, characterId, allianceId, asyncErr);
            if (co1.MoveNext())
            {
                yield return co1;
            }
            var retValue = asyncErr.Value;
            asyncErr.Dispose();
            msg.Reply((int)retValue);

            PlayerLog.WriteLog((ulong)LogType.AllianceWar,
                "In EnterAllianceWar(), characterId = {0}, allianceName = {1}, err = {2}", characterId, alliance.Name,
                asyncErr.Value);
        }

        /// <summary>
        /// end of攻城战相关
        /// 队伍：自动匹配  开启自动匹配
        /// </summary>
        public IEnumerator AutoMatchBegin(Coroutine coroutine, TeamCharacterProxy _this, AutoMatchBeginInMessage msg)
        {
            TeamProxy proxy = (TeamProxy)_this;
            ulong charactetId = msg.CharacterId;
            //Logger.Error("TeamWorkRefrerrence AutoMatchBegin characterId=" + msg.CharacterId + " prox chraracterid = " + proxy.CharacterId);
            bool isHaveTeam = msg.Request.IsHaveTeam == 1;

            TeamTargetType type = (TeamTargetType)msg.Request.Type;
            int targetId = msg.Request.TargetID;
            TeamMatchItem item;
            if (isHaveTeam)
            {
                TeamTarget target = new TeamTarget();
                target.type = type;
                target.teamTargetID = targetId;

                item = new TeamMatchItem() { characterID = (int)charactetId, teamTarget = target, };
                item.chracter = TeamManager.GetCharacterTeam(charactetId);
                if (AutoMatchManager.teamMatchDic.ContainsKey(charactetId))
                {
                    AutoMatchManager.teamMatchDic[charactetId] = null;
                    AutoMatchManager.teamMatchDic[charactetId] = item;
                }
                else
                {
                    AutoMatchManager.teamMatchDic.Add(charactetId, item);
                }
            }
            else
            {
                TeamTarget target = new TeamTarget();
                target.type = (TeamTargetType)type;//TeamTargetType.NOTTARGET;
                target.teamTargetID = targetId;
                item = new TeamMatchItem() { characterID = (int)charactetId, teamTarget = target, };
                if (AutoMatchManager.nullTeamMatchDic.ContainsKey(charactetId))
                {
                    AutoMatchManager.nullTeamMatchDic[charactetId] = null;
                    AutoMatchManager.nullTeamMatchDic[charactetId] = item;
                }
                else
                {
                    AutoMatchManager.nullTeamMatchDic.Add(charactetId, item);
                }
            }
            proxy.AutoMatchStateChange(1);
            AutoMatchManager.beginAotuMatch(isHaveTeam, charactetId, item);

            return null;
        }
        /// <summary>
        /// 队伍：自动匹配  取消自动匹配
        /// </summary>
        public IEnumerator AutoMatchCancel(Coroutine coroutine, TeamCharacterProxy _this, AutoMatchCancelInMessage msg)
        {
            TeamProxy proxy = (TeamProxy)_this;
            var charactetId = msg.CharacterId;
            if (AutoMatchManager.teamMatchDic.ContainsKey(charactetId))
            {
                AutoMatchManager.teamMatchDic.Remove(charactetId);
            }

            if (AutoMatchManager.nullTeamMatchDic.ContainsKey(charactetId))
            {
                AutoMatchManager.nullTeamMatchDic.Remove(charactetId);
            }
            proxy.AutoMatchStateChange(0);
            return null;
        }

        public IEnumerator ChangetTeamTarget(Coroutine coroutine, TeamCharacterProxy _this, ChangetTeamTargetInMessage msg)
        {
            TeamProxy proxy = (TeamProxy)_this;
            var team = TeamManager.GetCharacterTeam(msg.CharacterId);
            if (null == team)
            {
                msg.Reply((int)ErrorCodes.Error_ChangeTeamTargetFail_001);
                return null;
            }
            AutoMatchManager.changeTeamTarget(msg.CharacterId, msg.Request.Type, msg.Request.TargetID, msg.Request.LevelMini, msg.Request.LevelMax, msg.Request.ReadTableId);
            return null;
        }

        public IEnumerator SearchTeamList(Coroutine coroutine, TeamCharacterProxy _this, SearchTeamListInMessage msg)
        {
            int groupType = msg.Request.GroupType;
            int targetId = msg.Request.TargetID;
            TeamProxy proxy = (TeamProxy)_this;
            if (groupType == 0)
                targetId = 0;

            List<TeamSearchItem> tmpList = new List<TeamSearchItem>();
            foreach (var item in TeamManager.GetTeamsList().SearchList)
            {
                if (item.TeamGroupType == groupType && item.TargetId == targetId && item.LevelMini <= msg.Request.Level && item.LevelMax >= msg.Request.Level)
                {
                    ulong uId = (ulong)item.CharacterId;
                    var dbLogicSimple = TeamServer.Instance.LogicAgent.GetLogicSimpleData(uId, 0);
                    yield return dbLogicSimple.SendAndWaitUntilDone(coroutine);
                    if (dbLogicSimple.State != MessageState.Reply)
                    {
                        continue;
                    }
                    if (dbLogicSimple.ErrorCode != (int)ErrorCodes.OK)
                    {
                        continue;
                    }
                    item.Profession = dbLogicSimple.Response.TypeId;
                    item.Name = dbLogicSimple.Response.Name;
                    item.CharacterId = item.CharacterId;
                    item.RoleLevel = dbLogicSimple.Response.Level;

                    var team = TeamManager.GetCharacterTeam(uId);
                    if (null != team)
                    {
                        item.TeamID = team.team.TeamList.Count;
                    }
                    item.Ladder = dbLogicSimple.Response.Ladder;
                    item.StarNum = dbLogicSimple.Response.StarNum;
                    tmpList.Add(item);
                }

            }

            var count = tmpList.Count;
            List<int> calList = new List<int>();
            if (count > 0)
            {
                if (count <= 4)
                {
                    for (int i = 0; calList.Count < count; i++)
                    {
                        int index = MyRandom.Random(0, count - 1);
                        if (!calList.Contains(index))
                        {
                            calList.Add(index);
                            msg.Response.SearchList.Add(tmpList[index]);
                        }
                    }
                }
                else
                {
                    for (int i = 0; calList.Count < 4; i++)
                    {
                        int index = MyRandom.Random(0, count - 1);
                        if (!calList.Contains(index))
                        {
                            calList.Add(index);
                            msg.Response.SearchList.Add(tmpList[index]);
                        }
                    }
                }

            }
            msg.Reply();
            yield break;
        }

        public IEnumerator TeamSearchApplyList(Coroutine coroutine, TeamCharacterProxy _this, TeamSearchApplyListInMessage msg)
        {
            TeamProxy proxy = (TeamProxy)_this;
            var characterId = msg.CharacterId;

            var team = TeamManager.GetCharacterTeam(characterId);
            if (null == team)
            {
                msg.Reply((int)ErrorCodes.Error_SearchApplyListFail_001);
                yield break;
            }
            foreach (var item in team.team.ApplyList)
            {
                var dbLogicSimple = TeamServer.Instance.LogicAgent.GetLogicSimpleData(item, 0);
                yield return dbLogicSimple.SendAndWaitUntilDone(coroutine);
                if (dbLogicSimple.State != MessageState.Reply)
                {
                    continue;
                }
                if (dbLogicSimple.ErrorCode != (int)ErrorCodes.OK)
                {
                    continue;
                }
                var dbSceneSimple = TeamServer.Instance.SceneAgent.GetSceneSimpleData(item, 0);
                yield return dbSceneSimple.SendAndWaitUntilDone(coroutine);
                if (dbSceneSimple.State != MessageState.Reply)
                {
                    continue;
                }
                if (dbSceneSimple.ErrorCode != (int)ErrorCodes.OK)
                {
                    continue;
                }

                TeamSearchItem ite = new TeamSearchItem();
                ite.Profession = dbLogicSimple.Response.TypeId;
                ite.Name = dbLogicSimple.Response.Name;
                ite.RoleLevel = dbSceneSimple.Response.Level;
                ite.CharacterId = (int)item;
                ite.Ladder = dbSceneSimple.Response.Ladder;
                ite.StarNum = dbLogicSimple.Response.StarNum;
                msg.Response.SearchList.Add(ite);

            }
            msg.Reply();
            yield break;
        }

        public IEnumerator TeamApplyListClear(Coroutine coroutine, TeamCharacterProxy _this, TeamApplyListClearInMessage msg)
        {
            TeamProxy proxy = (TeamProxy)_this;
            var characterId = msg.CharacterId;

            ErrorCodes cod = TeamManager.ClearApplyList(characterId);

            yield break;
        }

        // 请求盟战开启时间
        public IEnumerator CSGetUnionBattleInfo(Coroutine coroutine, TeamCharacterProxy _this,
            CSGetUnionBattleInfoInMessage msg)
        {
            var proxy = (TeamProxy)_this;
            if (proxy == null)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            var aId = -1;
            var alliance = ServerAllianceManager.GetAllianceByCharacterId(proxy.CharacterId);
            if (alliance != null)
            {
                aId = alliance.AllianceId;
            }

            UnionBattleManager.FillBattleInfo(msg.Response, aId);
            msg.Reply((int)ErrorCodes.OK);
            yield break;
        }
        

        public IEnumerator CSEnrollUnionBattle(Coroutine coroutine, TeamCharacterProxy _this,
            CSEnrollUnionBattleInMessage msg)
        {
            var proxy = (TeamProxy)_this;
            if (proxy == null)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            var aId = msg.Request.AllianceId;
            var alliance = ServerAllianceManager.GetAllianceById(aId);
            if (alliance == null)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }

            if (alliance.Level < 2)
            { // 战盟等级不够
                msg.Reply((int)ErrorCodes.Error_UnionBattleLowLevel);
                yield break;
            }

            var serverAlliance = ServerAllianceManager.GetAllianceByServer(alliance.ServerId);
            if (serverAlliance == null)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }

            var requestCharacter = serverAlliance.GetCharacterData(msg.CharacterId);
            if (requestCharacter == null)
            {
                Logger.Warn("ChangeAllianceNotice is not apply proxy alliance[{0}]", aId);
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }

            var tbGA = Table.GetGuildAccess(requestCharacter.Ladder);
            if (tbGA == null)
            { //查表错误，数据溢出
                msg.Reply((int)ErrorCodes.Error_DataOverflow);
                yield break;
            }

            if (requestCharacter.Ladder != (int)eAllianceLadder.ViceChairman
                && requestCharacter.Ladder != (int)eAllianceLadder.Chairman)
            {
                msg.Reply((int)ErrorCodes.Error_UnionBattleLessPower);
                yield break;
            }

            var error = UnionBattleManager.Enroll(aId);
            msg.Reply((int)error);
        }

        public IEnumerator CSEnterUnionBattle(Coroutine coroutine, TeamCharacterProxy _this, CSEnterUnionBattleInMessage msg)
        {
            var proxy = (TeamProxy)_this;
            if (proxy == null)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            var aId = msg.Request.AllianceId;
            var alliance = ServerAllianceManager.GetAllianceById(aId);
            if (alliance == null)
            {
                msg.Reply((int)ErrorCodes.Error_AllianceNotFind);
                yield break;
            }

            var serverAlliance = ServerAllianceManager.GetAllianceByServer(alliance.ServerId);
            if (serverAlliance == null)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }

            var requestCharacter = serverAlliance.GetCharacterData(msg.CharacterId);
            if (requestCharacter == null)
            {
                Logger.Warn("ChangeAllianceNotice is not apply proxy alliance[{0}]", aId);
                msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                yield break;
            }

            if (requestCharacter.Level < 180)
            {
                msg.Reply((int)ErrorCodes.Error_LevelNoEnough);
                yield break;
            }

            var error = UnionBattleManager.CanEnter(aId);
            if (error != ErrorCodes.OK)
            {
                yield break;
            }

            UnionBattleManager.EnterFight(coroutine, msg.CharacterId, aId).MoveNext();
        }

        public IEnumerator CSGetUnionBattleMathInfo(Coroutine coroutine, TeamCharacterProxy _this,
            CSGetUnionBattleMathInfoInMessage msg)
        {
            var proxy = (TeamProxy)_this;
            if (proxy == null)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            var aId = -1;
            var alliance = ServerAllianceManager.GetAllianceByCharacterId(proxy.CharacterId);
            if (alliance != null)
            {
                aId = alliance.AllianceId;
            }

            UnionBattleManager.FillMathInfo(msg.Response, aId);
            msg.Reply((int)ErrorCodes.OK);
        }
    }

    public class TeamProxy : TeamCharacterProxy
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public TeamProxy(TeamService service, ulong characterId, ulong clientId)
            : base(service, characterId, clientId)
        {
        }

        //public List<CheckConnectedInMessage> WaitingCheckConnectedInMessages = new List<CheckConnectedInMessage>();
        //public List<CheckLostInMessage> WaitingCheckLostInMessages = new List<CheckLostInMessage>();
        public bool Connected { get; set; }
    }
}