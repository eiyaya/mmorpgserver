#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using DataContract;
using DataTable;
using LoginClientService;
using Scorpion;
using Mono.GameMath;
using NLog;
using SceneServerService;
using Shared;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace Scene
{
    public class SceneProxyDefaultImpl : ISceneCharacterProxy
    {
        private static readonly Logger ConnectLostLogger = LogManager.GetLogger("ConnectLost");
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Logger kafaLogger = LogManager.GetLogger(Shared.LoggerName.KafkaLog);

        public IEnumerator GMScene(Coroutine coroutine, SceneCharacterProxy charProxy, GMSceneInMessage msg)
        {
			var proxy = (SceneProxy)charProxy;
	        if (null == proxy.Character)
	        {
				yield break;
	        }

	        var level = GMCommandLevel.GetCommandLevel();
			if (GMCommandLevel.GMCommandLevelType.ALLOW == level)
			{
			}        
			else if (GMCommandLevel.GMCommandLevelType.GMALLOW == level)
			{
				if (!proxy.Character.mDbData.IsGM)
				{
					yield break;
				}
			}
			else 
			{
				yield break;
	        }

            var command = msg.Request.Commond;
            var err = proxy.Character.GmCommand(command);
            msg.Reply((int) err);
            PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------GMScene----------:{0}", command);
        }

        public IEnumerator ApplySolveStuck(Coroutine coroutine, SceneCharacterProxy charProxy, ApplySolveStuckInMessage msg)
        {
            var proxy = (SceneProxy)charProxy;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (character == null)
            {
                Logger.Error("AskEnterDungeon Enter characterId = {0} null", msg.CharacterId);
                yield break;
            }
            var sceneId = 3 == proxy.Character.Scene.TypeId ? 5 : 3;

            
            var param = new SceneParam();
            var co1 = CoroutineFactory.NewSubroutine(SceneServer.Instance.ServerControl.CreateAndEnterScene,
                coroutine,
                msg.CharacterId, proxy.Character.ServerId,
                sceneId,   //解决卡死的传送目标sceneId
                0ul,
                eScnenChangeType.EnterDungeon,
                param);
            if(co1.MoveNext())
            {
                yield return co1;
            }
            


            yield return null;
        }
        public IEnumerator HoldLode(Coroutine coroutine, SceneCharacterProxy charProxy, HoldLodeInMessage msg)
        {//玩家占领旗帜
            var proxy = (SceneProxy)charProxy;
            var scene = proxy.Character.Scene;
            {//判断玩家是否可以占领
                if (proxy.Character.GetAllianceId() <= 0)
                {
                    msg.Reply((int)ErrorCodes.Error_CharacterNoAlliance);
                    yield break;
                }
                if (scene.OwnerAllianceId == proxy.Character.GetAllianceId())
                {
                    msg.Reply((int)ErrorCodes.Error_Lode_IsOwnerAlliance);
                    yield break;
                }
             //判断是否在时间内
                //if(DateTime.Now<scene.HoldTimeBegin || DateTime.Now > scene.HoldTimeEnd)
                //{
                //    msg.Reply((int)ErrorCodes.Error_MieShi_NotCanInTime);
                //    yield break;
                //}
            }
            {//通知战盟服务器占领
                var msgTeam = SceneServer.Instance.TeamAgent.PlayerHoldLode(proxy.CharacterId, proxy.Character.ServerId, proxy.Character.GetAllianceId(), scene.TypeId);
                yield return msgTeam.SendAndWaitUntilDone(coroutine);
                if (msgTeam.State != MessageState.Reply)
                {
                    msg.Reply((int)ErrorCodes.Error_TimeOut);
                    yield break;
                }
                if (msgTeam.ErrorCode != (int)ErrorCodes.OK)
                {
                    msg.Reply(msgTeam.ErrorCode);
                    yield break;
                }
                scene.OwnerAllianceId = msgTeam.Response.TeamId;
                scene.OwnerAllianceName = msgTeam.Response.TeamName;
                scene.SetLodeInfo(msgTeam.Response);
                scene.BroadCastLodeInfo();
                msg.Response = scene.OwnerAllianceName;

                // 写日志记录占领情况
                //if (proxy.Character.GetLevel() < scene.TableSceneData.LevelLimit)
                {
                    PlayerLog.DataLog(proxy.Character.ObjId, "Player({0})Level({1}) hold lode scene={2},level={3},allianceId={4}",
                        proxy.Character.GetName(), proxy.Character.GetLevel(), scene.TypeId, scene.TableSceneData.LevelLimit, msgTeam.Response.TeamId);
                }

                {
                    var dict = new Dict_int_int_Data();
                    dict.Data.Add(956, 1);
                    proxy.Character.SendExDataChange(dict);                    
                }


            }
            msg.Reply();
            
        }

        public IEnumerator CK_ApplyLevelupBuff(Coroutine coroutine, SceneCharacterProxy charProxy, CK_ApplyLevelupBuffInMessage msg)
        {
            var proxy = (SceneProxy)charProxy;

            var scene = proxy.Character.Scene as JewellWars;
            if (scene == null)
            {
                msg.Reply((int)ErrorCodes.Error_CharacterNoScene);
                yield break;
            }
            msg.Reply(scene.OnPlayerSkillUp(proxy.Character, msg.Request.BuffId));
            yield break;
        }
        //占矿
        public IEnumerator CollectLode(Coroutine coroutine, SceneCharacterProxy charProxy, CollectLodeInMessage msg)
        {
            var proxy = (SceneProxy)charProxy;
            //玩家采矿
            //查看矿点是否靠谱
            var tb = Table.GetLode(msg.Request.LodeId);
            if (tb == null)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;                    
            }
            var score = proxy.Character.GetSycExData(949);

            var dropItems = new Dictionary<int, int>();
            var resItem = new Dictionary<int, int>();
            Drop.DropMother(tb.LodeOutput[0], dropItems);
            resItem.AddRange(dropItems);
            int addScore = 0;
            if (tb.LodeOutput[0] > 0 && proxy.Character.GetAdditionLode() > 1.0f)
            {
                foreach (var v in dropItems)
                {
                    resItem[v.Key] = (int)Math.Ceiling((float)resItem[v.Key] * proxy.Character.GetAdditionLode());
                   
                }
            }
            resItem.TryGetValue(8, out addScore);
            //功绩
            int MeritPoint = 0;
            resItem.TryGetValue(13, out MeritPoint);


            {//通知team采矿操作,计算对应的计数
                FieldRankBaseData rankData = new FieldRankBaseData();
                {

                    rankData.Guid = proxy.CharacterId;
                    rankData.Name = proxy.Character.GetName();
                    rankData.Level = proxy.Character.GetLevel();
                    rankData.Score = addScore + score;
                    rankData.TypeId = proxy.Character.GetRole();
                    CharacterManager.Instance.GetSimpeData(proxy.CharacterId, simple =>
                    {
                        rankData.FightPoint = simple.FightPoint;
                    });
                    
                }
                var dbTeamMsg = SceneServer.Instance.TeamAgent.PlayerCollectLode(proxy.CharacterId, proxy.Character.ServerId, proxy.CharacterId, proxy.Character.GetAllianceId(), proxy.Character.Scene.TypeId, msg.Request.LodeId, addScore, rankData, MeritPoint);
                yield return dbTeamMsg.SendAndWaitUntilDone(coroutine);
                if (dbTeamMsg.State != MessageState.Reply)
                {
                    msg.Reply((int)ErrorCodes.Error_TimeOut);
                    yield break;
                }
                if (dbTeamMsg.ErrorCode != (int)ErrorCodes.OK)
                {
                    msg.Reply(dbTeamMsg.ErrorCode);
                    yield break;
                }
                proxy.Character.Scene.SetLodeInfo(dbTeamMsg.Response);
                proxy.Character.Scene.BroadCastLodeInfo();
                var dict = new Dict_int_int_Data();
                if (tb.ExDataId > 0)
                    dict.Data.Add(tb.ExDataId,1);
                dict.Data.Add((int)eExdataDefine.e680, -1);
                dict.Data.Add(949, addScore);
                
                var msg1 = SceneServer.Instance.LogicAgent.SSChangeExdata(proxy.Character.ObjId,dict);
                yield return msg1.SendAndWaitUntilDone(coroutine);
            }
            {//告诉logic采矿结果,增加玩家的实际收益
               

            //设置副本开启和结束时间
                var now = DateTime.Now;
                var start1 = new DateTime(now.Year, now.Month, now.Day, tb.ActiveTime1[0] / 100, tb.ActiveTime1[0] % 100, 0, DateTimeKind.Local);
                var start2 = new DateTime(now.Year, now.Month, now.Day, tb.ActiveTime2[0] / 100, tb.ActiveTime2[0] % 100, 0, DateTimeKind.Local);
                var end1 = new DateTime(now.Year, now.Month, now.Day, tb.ActiveTime1[1] / 100, tb.ActiveTime1[1] % 100, 0, DateTimeKind.Local);
                var end2 = new DateTime(now.Year, now.Month, now.Day, tb.ActiveTime2[1] / 100, tb.ActiveTime2[1] % 100, 0, DateTimeKind.Local);
                if ((now > start1 && now < end1) || (now > start2 && now < end2))
                {
                    foreach (var v in dropItems)
                    {
                        resItem[v.Key] = resItem[v.Key]*tb.Addition;
                    }                    
                }
                
                if (tb.LodeOutput[1] > 0)
                {
                    Drop.DropMother(tb.LodeOutput[1], resItem);                    
                }
                Dict_int_int_Data data = new Dict_int_int_Data();
                data.Data.AddRange(resItem);
                var msgLogicAdd = SceneServer.Instance.LogicAgent.GiveItemList(proxy.CharacterId, data, (int)eCreateItemType.CollectLode);
                yield return msgLogicAdd.SendAndWaitUntilDone(coroutine);                    
            }
            msg.Reply();                

            yield break;
        }

        /// <summary>
        ///     请求场景内所有玩家
        /// </summary>
        /// <param name="coroutine"></param>
        /// <param name="charProxy"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        /// IEnumerator ApplySceneObj(Coroutine coroutine, SceneCharacterProxy _this, ApplySceneObjInMessage msg);
        public IEnumerator ApplySceneObj(Coroutine coroutine, SceneCharacterProxy charProxy, ApplySceneObjInMessage msg)
        {
            var proxy = (SceneProxy) charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------ApplySceneObj----------");
            var scene = proxy.Character.Scene;
            if (scene == null)
            {
                msg.Reply();
                yield break;
            }
            scene.PushActionToAllPlayer(player =>
            {
                if (player.GetTeamId() != 0)
                {
                    return;
                }
                if (msg.CharacterId == player.ObjId)
                {
                    return;
                }
                var StarNum = 0;
                player.dicFlagTemp.TryGetValue((int) eExdataDefine.e688, out StarNum);
                var characterSimple = new CharacterSimpleInfo
                {
                    CharacterId = player.ObjId,
                    Name = player.GetName(),
                    Type = player.TypeId,
                    Level = player.GetLevel(),
                    FightValue = player.Attr.GetFightPoint(),
                    Ladder = player.Attr.Ladder,
                    Serverid = player.ServerId,
                    Star = StarNum
                };
                //player.GetEquipsModel(characterSimple.EquipsModel);
                msg.Response.Data.Add(characterSimple);
            });
            msg.Reply();
        }

        public IEnumerator SceneChatMessage(Coroutine coroutine,
                                            SceneCharacterProxy charProxy,
                                            SceneChatMessageInMessage msg)
        {
            var proxy = (SceneProxy) charProxy;
            if (proxy.Character.mDbData.BannedToPost != 0)
            {
                msg.Reply((int) ErrorCodes.Error_BannedToPost);
                yield break;
            }
            var content = msg.Request.Content;
            var type = msg.Request.ChatType;
            var characterid = msg.CharacterId;
            content.Vip = proxy.Character.VipLevel;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------SceneChatMessage----------:{0},{1},{2}",
                type, content, characterid);
            if (!StringExtension.CheckChatStr(content.Content))
            {
                msg.Reply((int) ErrorCodes.Error_ChatLengthMax);
                yield break;
            }
            if ((eChatChannel) type != eChatChannel.Scene)
            {
                msg.Reply((int) ErrorCodes.Error_ChatChannel);
                yield break;
            }
            #region 37监测
            bool bNormal = content.Content.IndexOf("{!") == -1 && content.Content.IndexOf("!}") == -1;

            var monitorData = SceneServer.Instance.LogicAgent.GetPlayerMoniterData(msg.CharacterId, msg.CharacterId);
            yield return monitorData.SendAndWaitUntilDone(coroutine);
            if (monitorData == null || monitorData.Response == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }
            var dbLoginSimple = SceneServer.Instance.LoginAgent.GetLoginSimpleData(proxy.ClientId, msg.CharacterId);
            yield return dbLoginSimple.SendAndWaitUntilDone(coroutine);
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
                    {
                        switch (type)
                        {
                            case (int)eChatChannel.Scene:
                                dic.Add("type", "6");
                                break;
                            default:
                                dic.Add("type", "8");
                                break;
                        }
                    }
                    var strsign = string.Format("{0}{1}{2}{3}{4}{5}", md5Key, monitorData.Response.uid, monitorData.Response.gid, dbLoginSimple.Response.ServerId, Math.Ceiling(unixTimer), dic["type"]);//md5Key + "}{" + proxy.Character.moniterData.uid + "}{""}{" + proxy.Character.ServerId.ToString()+ "}{" + unixTimer + "}{" + dic["type"] + "}");
                    var sign = RequestManager.Encrypt_MD5_UTF8(strsign);
                    dic.Add("sign", sign);
                    dic.Add("actor_name", HttpUtility.UrlEncode(dbLoginSimple.Response.Name));
                    dic.Add("actor_id", dbLoginSimple.Response.Id.ToString());
                    dic.Add("chat_time", Math.Ceiling(unixTimer).ToString());
                    dic.Add("content", HttpUtility.UrlEncode(msg.Request.Content.Content));
                    var url = @"http://cm3.api.37.com.cn/Content/_checkContent";
                    var result = AsyncReturnValue<string>.Create();
                    yield return SceneManager.WebRequestManager.DoRequest(coroutine, url, dic, result);

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
                    switch (type)
                    {

                        case (int)eChatChannel.Scene:
                            //dic.Add("type", "6");
                            chatType = 6;
                            break;
                        default:
                            chatType = 8;
                            //dic.Add("type", "8");
                            break;
                    }
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
                    param.Add(new KeyValuePair<string, string>("content", HttpUtility.UrlEncode(msg.Request.Content.Content)));
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
            var ids = new List<ulong>();
            proxy.Character.ChatSpeek((eChatChannel) type, content.Content, ids);
            if (ids.Count > 0)
            {
                SceneServer.Instance.ChatAgent.ChatNotify(ids, type, characterid,
                    proxy.Character.GetName(), content);
            }
            msg.Reply();

            try
            {
                string v = string.Format("admin_chatlog#{0}|{1}|{2}|{3}|{4}",
                    proxy.CharacterId,
                    proxy.Character.GetName(), //serverID
                    type,
                    content.Content,
                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")); // 时间
                kafaLogger.Info(v);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }
        }
       
        [Updateable("SceneProxy")]
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
        //退出副本
        public IEnumerator ExitDungeon(Coroutine coroutine, SceneCharacterProxy charProxy, ExitDungeonInMessage msg)
        {
            var proxy = (SceneProxy) charProxy;
            var type = msg.Request.Type;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------ExitDungeon----------{0}", type);

            if (null == proxy.Character)
            {
                PlayerLog.WriteLog(proxy.CharacterId, "null==proxy.Character");
                msg.Reply((int) ErrorCodes.StateError);
                yield break;
            }

            if (proxy.Character.IsChangingScene())
            {
                PlayerLog.WriteLog(proxy.CharacterId, "proxy.Character.IsChangingScene()");
                msg.Reply((int) ErrorCodes.StateError);
                yield break;
            }

            if (proxy.Character.Scene != null)
            {
                proxy.Character.Scene.ExitDungeon(proxy.Character);
                var tbScene = Table.GetScene(proxy.Character.Scene.TypeId);
                if (tbScene.Type == 3 && type == -1)
                {
                    //Type = -1，pvp类型先放弃结果
                    proxy.Character.Attr.SetDataValue(eAttributeType.HpNow,
                        proxy.Character.Attr.GetDataValue(eAttributeType.HpMax));
                    proxy.Character.Attr.SetDataValue(eAttributeType.MpNow,
                        proxy.Character.Attr.GetDataValue(eAttributeType.MpMax));
                    msg.Reply();
                    yield break;
                }

                if (type == -10) //潜规则  -10 为退出副本并复活
                {
                    proxy.Character.StopAutoRelive();
                    proxy.Character.Relive();
                    proxy.Character.SelectReliveType = 0;
                }
            }

            msg.Reply();

            var co = CoroutineFactory.NewSubroutine(proxy.Character.ExitDungeon, coroutine);
            if (co.MoveNext())
            {
                yield return co;
            }
        }


        //通知一些事情
        public IEnumerator NotifySomeClientMessage(Coroutine coroutine,
                                                   SceneCharacterProxy charProxy,
                                                   NotifySomeClientMessageInMessage msg)
        {
            var proxy = (SceneProxy) charProxy;
            var type = msg.Request.Type;
            var value = msg.Request.Value;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------NotifySomeClientMessage----------{0}", type,
                value);
            switch (type)
            {
                case 0: //移动速度修改
                {
                    if (value == 0)
                    {
                        proxy.Character.Attr.SetDataValue(eAttributeType.MoveSpeed, 300);
                    }
                    else
                    {
                        proxy.Character.Attr.SetDataValue(eAttributeType.MoveSpeed, 600);
                    }
                }
                    break;
            }
            msg.Reply();
            yield break;
        }

        //NPC服务
        public IEnumerator NpcService(Coroutine coroutine, SceneCharacterProxy charProxy, NpcServiceInMessage msg)
        {
            var proxy = (SceneProxy) charProxy;
            var serviceid = msg.Request.ServiceId;
            var npcguid = msg.Request.NpcGuid;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------NpcService----------{0},{1}", serviceid,
                npcguid);
            if (null == proxy.Character.Scene)
            {
                msg.Reply((int) ErrorCodes.Unknow);
                Logger.Warn("NpcService null==Scene      Character{0}", proxy.Character.ObjId);
                yield break;
            }

            var result = proxy.Character.NpcService(npcguid, serviceid);
            if (result == ErrorCodes.Need_2_Logic)
            {
                var Service = SceneServer.Instance.LogicAgent.NpcService(proxy.CharacterId, serviceid);
                yield return Service.SendAndWaitUntilDone(coroutine);
                if (Service.ErrorCode == (int) ErrorCodes.OK)
                {
                    if (Table.GetService(serviceid).Type == 1) //修理服务
                    {
                        foreach (var equip in proxy.Character.Equip)
                        {
                            if (-1 == equip.Value.GetId())
                            {
                                continue;
                            }
                            var tbEquip = Table.GetEquip(equip.Value.GetId());
                            if (tbEquip.DurableType == 0)
                            {
                                continue;
                            }
                            equip.Value.SetExdata(22, tbEquip.Durability);
                        }
                    }
                }
                msg.Reply(Service.ErrorCode);
                yield break;
            }
            msg.Reply();
        }

        //复活
        public IEnumerator ReliveType(Coroutine coroutine, SceneCharacterProxy charProxy, ReliveTypeInMessage msg)
        {
            var proxy = (SceneProxy) charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------ReliveType----------{0}", msg.Request.Type);
            if (!proxy.Character.IsDead()) // 人没死
            {
                if (proxy.Character.Attr != null && proxy.Character.Attr.GetDataValue(eAttributeType.HpNow) < 1) // 但是血量为0  服务器异常了
                {
                    proxy.Character.Attr.SetDataValue(eAttributeType.HpNow, proxy.Character.Attr.GetDataValue(eAttributeType.HpMax));
                }

                msg.Reply((int)ErrorCodes.Error_CharacterNoDie);
                yield break;
            }
            if (proxy.Character.Scene != null)
            {
                if (proxy.Character.Scene.TypeId == 1001)
                {
                    msg.Reply((int) ErrorCodes.Unknow);
                    yield break;
                }
            }
            switch (msg.Request.Type)
            {
                case 0: //复活石复活
                {
                    proxy.Character.SelectReliveType = 2;
                    var result = SceneServer.Instance.LogicAgent.DeleteItem(proxy.Character.ObjId, 22019, 1, (int)eDeleteItemType.FuHuo);
                    yield return result.SendAndWaitUntilDone(coroutine);
                    if (result.State != MessageState.Reply)
                    {
                        Logger.Error("ReliveType result.State={0}", result.State);
                        proxy.Character.StopAutoRelive();
                        proxy.Character.Relive();
                        msg.Reply((int) ErrorCodes.Unknow);
                        proxy.Character.SelectReliveType = 0;
                        yield break;
                    }
                    if (result.ErrorCode == (int)ErrorCodes.OK)
                    {
                        //Logic删除道具成功
                        proxy.Character.StopAutoRelive();
                        proxy.Character.Relive(true);
                        proxy.Character.SelectReliveType = 0;
                    }
                    else
                    {
                        //if (proxy.Character.SelectReliveType == 1)
                        {
                            proxy.Character.SelectReliveType = 0;
                            //proxy.Character.StopAutoRelive();
                        }
                        msg.Reply(result.ErrorCode);
                        yield break;
                    }
                }
                    break;
                case 1: //钻石复活
                {
                    proxy.Character.SelectReliveType = 2;
                    var diamond = Table.GetServerConfig(900).ToInt();
                    if (diamond < 0)
                    {
                        diamond = 30;
                    }
                    var result = SceneServer.Instance.LogicAgent.DeleteItem(proxy.Character.ObjId, 3, diamond, (int)eDeleteItemType.FuHuo);
                    yield return result.SendAndWaitUntilDone(coroutine);
                    if (result.State != MessageState.Reply)
                    {
                        Logger.Error("ReliveType result.State={0}", result.State);
                        proxy.Character.StopAutoRelive();
                        proxy.Character.Relive();
                        msg.Reply((int) ErrorCodes.Unknow);
                        proxy.Character.SelectReliveType = 0;
                        yield break;
                    }

                    if (result.ErrorCode == (int)ErrorCodes.OK)
                    {
                        //Logic删除道具成功
                        proxy.Character.StopAutoRelive();
						proxy.Character.Relive(true);

                        proxy.Character.SelectReliveType = 0;
                    }
                    else
                    {
                        //if (proxy.Character.SelectReliveType == 1)
                        {
                            proxy.Character.SelectReliveType = 0;
                            //proxy.Character.StopAutoRelive();
                        }
                        msg.Reply(result.ErrorCode);
                        yield break;
                    }
                }
                    break;
                case 2: //安全区
                {
                    proxy.Character.AutoRelive();
                }
                    break;
            }

            msg.Reply();
        }

        //修改pk模式
        public IEnumerator ChangePKModel(Coroutine coroutine, SceneCharacterProxy charProxy, ChangePKModelInMessage msg)
        {
            var proxy = (SceneProxy) charProxy;
            var model = msg.Request.PkModel;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------ChangePKModel----------{0}", model);
            if (model < (int) ePkModel.Peace || model > (int) ePkModel.GoodEvil)
            {
                msg.Reply((int) ErrorCodes.Error_DataOverflow);
                yield break;
            }
            proxy.Character.PkModel = model;
            msg.Response = proxy.Character.mDbData.PKModel;
            //             if (Character.Scene != null)
            //             {
            //                 Character.mDbData.PKModel = msg.Request.PkModel;
            //                 msg.Response = msg.Request.PkModel;
            //             }
            //             else
            //             {
            //                 msg.Response = -1;
            //             }
            msg.Reply();
        }

        //激活Buff
        //public IEnumerator ActivationAllianceBuff(Coroutine coroutine, SceneCharacterProxy charProxy, ActivationAllianceBuffInMessage msg)
        //{
        //    SceneProxy proxy = (SceneProxy)charProxy;
        //    var buffId = msg.Request.BuffId;
        //    PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------ActivationAllianceBuff----------{0}", buffId);
        //    var tbGuidBuff = Table.GetGuildBuff(buffId);
        //    if (tbGuidBuff == null)
        //    {
        //        msg.Reply((int)ErrorCodes.Error_BuffID);
        //        yield break;
        //    }
        //    //向team请求该Buff的等级
        //    var dbTeamMsg = SceneServer.Instance.TeamAgent.SSGetAllianceBuffLevel(proxy.Character.ObjId,
        //        proxy.Character.ServerId, buffId);
        //    yield return dbTeamMsg.SendAndWaitUntilDone(coroutine);
        //    if (dbTeamMsg.State != MessageState.Reply)
        //    {
        //        msg.Reply((int)ErrorCodes.Error_TimeOut);
        //        yield break;
        //    }
        //    if (dbTeamMsg.ErrorCode != (int)ErrorCodes.OK)
        //    {
        //        msg.Reply(dbTeamMsg.ErrorCode);
        //        yield break;
        //    }
        //    var buffLevel = dbTeamMsg.Response;
        //    if (buffLevel == 0)
        //    {
        //        msg.Reply(dbTeamMsg.ErrorCode);
        //        yield break;
        //    }
        //    //向Logic请求扣除道具
        //    //var dbLogicSimple = SceneServer.Instance.LogicAgent.DeleteItem(proxy.Character.ObjId, tbGuidBuff.ActiveNeedID,
        //    //    tbGuidBuff.ActiveNeedCount);

        //    //     var dbLogicSimple = SceneServer.Instance.LogicAgent.DeleteItem(Character.ObjId, tbGuidBuff.ActiveNeedID, Table.GetSkillUpgrading(tbGuidBuff.ActiveNeedID).GetSkillUpgradingValue(buffLevel));
        //    //yield return dbLogicSimple.SendAndWaitUntilDone(coroutine);
        //    //if (dbLogicSimple.State != MessageState.Reply)
        //    //{
        //    //    msg.Reply((int)ErrorCodes.Error_TimeOut);
        //    //    yield break;
        //    //}
        //    //if (dbLogicSimple.ErrorCode != (int)ErrorCodes.OK)
        //    //{
        //    //    msg.Reply(dbLogicSimple.ErrorCode);
        //    //    yield break;
        //    //}
        //    //增加Buff
        //    proxy.Character.AddBuff(tbGuidBuff.BuffID, buffLevel, proxy.Character);
        //    msg.Reply();
        //}

        public IEnumerator FlyTo(Coroutine coroutine, SceneCharacterProxy charProxy, FlyToInMessage msg)
        {
            var proxy = (SceneProxy) charProxy;

            if (null == proxy.Character)
            {
                PlayerLog.WriteLog(proxy.CharacterId, "FlyTo null==proxy.Character");
                msg.Reply((int) ErrorCodes.StateError);
                yield break;
            }

            if (proxy.Character.IsChangingScene())
            {
                PlayerLog.WriteLog(proxy.CharacterId, "FlyTo proxy.Character.IsChangingScene()");
                msg.Reply((int) ErrorCodes.StateError);
                yield break;
            }

            var sceneId = msg.Request.SceneId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------FlyTo----------{0},{1},{2}", sceneId,
                msg.Request.Postion.x, msg.Request.Postion.y);
            if (sceneId == 0)
            {
                msg.Reply((int) ErrorCodes.Error_NoScene);
                yield break;
            }
            var tbScene = Table.GetScene(sceneId);
            if (tbScene == null)
            {
                msg.Reply((int) ErrorCodes.Error_NoScene);
                yield break;
            }
            // 检查限制条件
            // 场景是否开放
            if (tbScene.IsPublic != 1)
            {
                msg.Reply((int) ErrorCodes.Error_SceneNotPublic);
                yield break;
            }
            // 角色等级是否到达
            if (proxy.Character.GetLevel() < tbScene.LevelLimit)
            {
                msg.Reply((int) ErrorCodes.Error_LevelNoEnough);
                yield break;
            }
            PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------FlyTo----------{0}", sceneId);
            var pos = Utility.MakeVectorDividePrecision(msg.Request.Postion.x, msg.Request.Postion.y);

            if (proxy.Character.Scene.TypeId == sceneId)
            {
                if (!proxy.Character.Scene.ValidPosition(pos))
                {
                    msg.Reply((int)ErrorCodes.Error_PathInvalid);
                    yield break;
                }
                proxy.Character.StopMove();
                proxy.Character.SetPosition(pos);
                proxy.Character.SyncCharacterPostion();
                var retinues = proxy.Character.GetRetinueList();

                if (retinues != null)
                {
                    foreach (var retinue in retinues)
                    {
                        var rePos = retinue.GetPosition();
                        var distance = (pos - rePos).LengthSquared();
                        if (distance > 400)
                        {
                            retinue.SetPosition(pos);
                        }
                    }
                }

                msg.Reply();
            }
            else
            {
                //Fly命令切换场景，根据合服ID进行
                var serverLogicId = SceneExtension.GetServerLogicId(proxy.Character.ServerId);
                var sceneInfo = new ChangeSceneInfo
                {
                    SceneId = sceneId,
                    ServerId = serverLogicId,
                    SceneGuid = 0,
                    Type = (int) eScnenChangeType.Position
                };
                sceneInfo.Guids.Add(proxy.Character.ObjId);
                sceneInfo.Pos = new SceneParam();
                sceneInfo.Pos.Param.Add((int) pos.X);
                sceneInfo.Pos.Param.Add((int) pos.Y);
                var msgChgScene = SceneServer.Instance.SceneAgent.SBChangeSceneByTeam(proxy.Character.ObjId, sceneInfo);
                yield return msgChgScene.SendAndWaitUntilDone(coroutine, TimeSpan.FromSeconds(30));
                msg.Reply();
            }
        }

        public IEnumerator FastReach(Coroutine coroutine, SceneCharacterProxy charProxy, FastReachInMessage msg)
        {
            var proxy = (SceneProxy)charProxy;

            if (null == proxy.Character)
            {
                PlayerLog.WriteLog(proxy.CharacterId, "FastReach null==proxy.Character");
                msg.Reply((int)ErrorCodes.StateError);
                yield break;
            }

            if (proxy.Character.IsChangingScene())
            {
                PlayerLog.WriteLog(proxy.CharacterId, "FastReach proxy.Character.IsChangingScene()");
                msg.Reply((int)ErrorCodes.StateError);
                yield break;
            }

            var sceneId = msg.Request.SceneId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------FastReach----------{0},{1},{2}", sceneId,
                msg.Request.Postion.x, msg.Request.Postion.y);
            if (sceneId == 0)
            {
                msg.Reply((int)ErrorCodes.Error_NoScene);
                yield break;
            }
            var tbScene = Table.GetScene(sceneId);
            if (tbScene == null)
            {
                msg.Reply((int)ErrorCodes.Error_NoScene);
                yield break;
            }
            // 检查限制条件
            // 场景是否开放
            if (tbScene.IsPublic != 1)
            {
                msg.Reply((int)ErrorCodes.Error_SceneNotPublic);
                yield break;
            }
            // 角色等级是否到达
            if (proxy.Character.GetLevel() < tbScene.LevelLimit)
            {
                msg.Reply((int)ErrorCodes.Error_LevelNoEnough);
                yield break;
            }

            // 检查次数是否足够
            var ids3 = new Int32Array();
            ids3.Items.Add((int)eExdataDefine.e700);
            ids3.Items.Add((int)eExdataDefine.e701);
            ids3.Items.Add((int)eExdataDefine.e702);
            var msg3 = SceneServer.Instance.LogicAgent.SSFetchExdata(proxy.Character.ObjId, ids3);
            yield return msg3.SendAndWaitUntilDone(coroutine);

            if (msg3.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (msg3.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (msg3.Response.Items.Count < 3)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            var fastReachItemId = Table.GetServerConfig(375).ToInt();
            var msg4 = SceneServer.Instance.LogicAgent.GetItemCount(proxy.Character.ObjId, fastReachItemId);
            yield return msg4.SendAndWaitUntilDone(coroutine);

            if (msg4.State != MessageState.Reply || msg4.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            var freeTimes = msg3.Response.Items[0];
            var doneTimes = msg3.Response.Items[1];
            var vipDoneTimes = msg3.Response.Items[2];
            var tbVip = Table.GetVIP(proxy.Character.VipLevel);
            if (tbVip == null)
            {
                msg.Reply((int)ErrorCodes.VipLevelNotEnough);
                yield break;
            }
            var vipAdd = tbVip.SentTimes;
            var fastReachItemTimes = msg4.Response;
            if ((vipAdd - vipDoneTimes) + freeTimes + fastReachItemTimes <= 0)
            {
                // 改为扣5个钻石
                //msg.Reply((int)ErrorCodes.Error_NO_Times);
                var diamond = int.Parse(Table.GetServerConfig(1202).Value);
                if (diamond < 0)
                {
                    diamond = 0;
                }
                var result = SceneServer.Instance.LogicAgent.DeleteItem(proxy.CharacterId, 3, diamond, (int)eDeleteItemType.FeiXie);
                yield return result.SendAndWaitUntilDone(coroutine);
                if (result.State != MessageState.Reply)
                {
                    Logger.Error("Cost money result.State={0}", result.State);
                    msg.Reply((int)ErrorCodes.Unknow);
                    yield break;
                }

                // 扣钱出现错误，则直接返回错误码
                if (result.Response != (int)ErrorCodes.OK)
                {
                    msg.Reply(result.Response);
                    yield break;
                }

                if (result.ErrorCode != (int)ErrorCodes.OK)
                {
                    if (null == msg)
                    {
                        Logger.Error("FastReach msg is null!!!!!");
                        yield break;
                    }
                    msg.Reply(result.ErrorCode);
                    yield break;
                }
            }


            PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------FlyTo----------{0}", sceneId);
            var pos = Utility.MakeVectorDividePrecision(msg.Request.Postion.x, msg.Request.Postion.y);

            if (proxy.Character.Scene.TypeId == sceneId)
            {
                if (!proxy.Character.Scene.ValidPosition(pos))
                {
                    var nearest = proxy.Character.Scene.FindNearestValidPosition(pos, 20.0f);
                    if (null == nearest)
                    {
                        msg.Reply((int)ErrorCodes.Error_PathInvalid);
                        yield break;
                    }
                    pos = nearest.Value;
                }

                var dict = new Dict_int_int_Data();
                if (freeTimes > 0) // 有免费次数 使用免费次数
                {
                    dict.Data.Add((int)eExdataDefine.e700, -1);
                }
                else if ((vipAdd - vipDoneTimes) > 0)// 有vip次数 使用vip次数
                {
                    dict.Data.Add((int)eExdataDefine.e702, 1);
                }
                else if (fastReachItemTimes > 0)// 有道具次数 扣道具次数
                {
                    dict.Data.Add((int)eExdataDefine.e703, 1);
                    var msg5 = SceneServer.Instance.LogicAgent.DeleteItem(proxy.Character.ObjId, fastReachItemId, 1,
                        (int)eDeleteItemType.FeiXie);
                    yield return msg5.SendAndWaitUntilDone(coroutine);

                    if (msg5.State != MessageState.Reply || msg5.ErrorCode != (int)ErrorCodes.OK)
                    {
                        if (msg == null)
                        {
                            yield break;
                        }
                        msg.Reply((int)ErrorCodes.Unknow);
                        yield break;
                    }
                }

                //增加传送次数
                dict.Data.Add((int)eExdataDefine.e701, 1);
                var msg1 = SceneServer.Instance.LogicAgent.SSChangeExdata(proxy.Character.ObjId, dict);
                yield return msg1.SendAndWaitUntilDone(coroutine);

                proxy.Character.StopMove();
                proxy.Character.SetPosition(pos);
                proxy.Character.SyncCharacterPostion();
                var retinues = proxy.Character.GetRetinueList();

                if (retinues != null)
                {
                    foreach (var retinue in retinues)
                    {
                        var rePos = retinue.GetPosition();
                        var distance = (pos - rePos).LengthSquared();
                        if (distance > 400)
                        {
                            retinue.SetPosition(pos);
                        }
                    }
                }

                msg.Reply();
            }
            else
            {
                if (proxy.Character.IsChangingScene())
                {
                
                    msg.Reply((int)ErrorCodes.StateError);
                    yield break;
                }

                var dict = new Dict_int_int_Data();
                if (freeTimes > 0) // 有免费次数 使用免费次数
                {
                    dict.Data.Add((int)eExdataDefine.e700, -1);
                }
                else if ((vipAdd - vipDoneTimes) > 0)// 有vip次数 使用vip次数
                {
                    dict.Data.Add((int)eExdataDefine.e702, 1);
                }
                else if (fastReachItemTimes > 0)// 有道具次数 扣道具次数
                {
                    dict.Data.Add((int)eExdataDefine.e703, 1);
                    var msg5 = SceneServer.Instance.LogicAgent.DeleteItem(proxy.Character.ObjId, fastReachItemId, 1,
                        (int)eDeleteItemType.FeiXie);
                    yield return msg5.SendAndWaitUntilDone(coroutine);

                    if (msg5.State != MessageState.Reply || msg5.ErrorCode != (int)ErrorCodes.OK)
                    {
                        if (msg == null)
                        {
                            yield break;
                        }
                        msg.Reply((int)ErrorCodes.Unknow);
                        yield break;
                    }
                }

                //增加传送次数
                dict.Data.Add((int)eExdataDefine.e701, 1);
                var msg1 = SceneServer.Instance.LogicAgent.SSChangeExdata(proxy.Character.ObjId, dict);
                yield return msg1.SendAndWaitUntilDone(coroutine);


                //Fly命令切换场景，根据合服ID进行
                var serverLogicId = SceneExtension.GetServerLogicId(proxy.Character.ServerId);
                var sceneInfo = new ChangeSceneInfo
                {
                    SceneId = sceneId,
                    ServerId = serverLogicId,
                    SceneGuid = 0,
                    Type = (int)eScnenChangeType.Position
                };
                sceneInfo.Guids.Add(proxy.Character.ObjId);
                sceneInfo.Pos = new SceneParam();
                sceneInfo.Pos.Param.Add((int)pos.X);
                sceneInfo.Pos.Param.Add((int)pos.Y);

                proxy.Character.BeginChangeScene();
                var msgChgScene = SceneServer.Instance.SceneAgent.SBChangeSceneByTeam(proxy.Character.ObjId, sceneInfo);
                yield return msgChgScene.SendAndWaitUntilDone(coroutine, TimeSpan.FromSeconds(30));

                msg.Reply();
            }
        }

        //请求附近队伍的所有队长
        public IEnumerator ApplySceneTeamLeaderObj(Coroutine coroutine,
                                                   SceneCharacterProxy charProxy,
                                                   ApplySceneTeamLeaderObjInMessage msg)
        {
            var proxy = (SceneProxy) charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------ApplySceneTeamLeaderObj----------");
            var serverId = proxy.Character.ServerId;
            var players = new List<ObjPlayer>();
            var guids = new Uint64Array();
            CharacterManager.Instance.ForeachCharacter(controller =>
            {
                var player = controller as ObjPlayer;
                if (player == null)
                {
                    return true;
                }
                if (serverId != player.ServerId)
                {
                    return true;
                }
                if (player.GetTeamId() == 0)
                {
                    return true;
                }
                if (player.teamState != 0)
                {
                    return true;
                }
                if (msg.CharacterId == player.ObjId)
                {
                    return true;
                }
                players.Add(player);
                guids.Items.Add(player.ObjId);
                return true;
            });

            var teamMsg = SceneServer.Instance.TeamAgent.SSGetTeamCount(proxy.CharacterId, guids);
            yield return teamMsg.SendAndWaitUntilDone(coroutine);
            if (teamMsg.State != MessageState.Reply)
            {
                Logger.Error("Cost money result.State={0}", teamMsg.State);
                msg.Reply((int) ErrorCodes.Unknow);
                yield break;
            }
            var index = 0;
            var count = teamMsg.Response.Items.Count;
            foreach (var player in players)
            {
                if (index >= count)
                {
                    break;
                }
                var teamcount = teamMsg.Response.Items[index];
                if (teamcount > 0)
                {
                    var characterSimple = new CharacterSimpleInfo
                    {
                        CharacterId = player.ObjId,
                        Name = player.GetName(),
                        Type = player.TypeId,
                        RoleId = teamcount,
                        Level = player.GetLevel(),
                        FightValue = player.Attr.GetFightPoint(),
                        Ladder = player.Attr.Ladder,
                        Serverid = player.ServerId
                    };
                    msg.Response.Data.Add(characterSimple);
                }
                index++;
            }
            msg.Reply();
        }

        //获得离线经验
        public IEnumerator GetLeaveExp(Coroutine coroutine, SceneCharacterProxy charProxy, GetLeaveExpInMessage msg)
        {
            var proxy = (SceneProxy) charProxy;
            var character = proxy.Character;
            if (character.GetLevel() >= 400)
            {
                msg.Reply((int) ErrorCodes.CharacterLevelMax);
                yield break;
            }
            var maxExp = 0;
            var getExp = character.GetNowLeaveExp(ref maxExp);
            if (getExp < 1 || maxExp < 1)
            {
                msg.Reply((int) ErrorCodes.Error_TimeNotOver);
                yield break;
            }
            var type = msg.Request.Type;
            var needcount = msg.Request.NeedCount;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------GetLeaveExp----------:{0}", type);

            var tbVip = Table.GetVIP(character.VipLevel);
            if (tbVip == null)
            {
                msg.Reply((int)ErrorCodes.VipLevelNotEnough);
                yield break;
            }

            if (type == 1 && tbVip.Muse2Reward == 0)
            {
                msg.Reply((int) ErrorCodes.VipLevelNotEnough);
                yield break;
            }
            if (type == 2 && tbVip.Muse4Reward == 0)
            {
                msg.Reply((int) ErrorCodes.VipLevelNotEnough);
                yield break;
            }
            switch (type)
            {
                case 0: //正常经验
                {
                    character.OutLineExp = 0;
                    character.OutLineTime = DateTime.Now.AddMinutes(1);
                    var msgLogicAdd = SceneServer.Instance.LogicAgent.GiveItem(proxy.CharacterId, 1, getExp,-1);
                    yield return msgLogicAdd.SendAndWaitUntilDone(coroutine);
                }
                    break;
                case 1: //金币：双倍经验
                {
                    var needCount = (int) ((double) getExp*ObjPlayer.OutLineMoney/maxExp);
                    if (needCount > needcount)
                    {
                        msg.Reply((int) ErrorCodes.MoneyNotEnough);
                        yield break;
                    }
                    var msgLogic = SceneServer.Instance.LogicAgent.DeleteItem(proxy.CharacterId, 2, needCount, (int)eDeleteItemType.GetLeaveExp);
                    yield return msgLogic.SendAndWaitUntilDone(coroutine);
                    if (msgLogic.State != MessageState.Reply)
                    {
                        msg.Reply((int) ErrorCodes.Error_TimeOut);
                        yield break;
                    }
                    if (msgLogic.ErrorCode != (int) ErrorCodes.OK)
                    {
                        msg.Reply(msgLogic.ErrorCode);
                        yield break;
                    }
                    character.OutLineExp = 0;
                    character.OutLineTime = DateTime.Now.AddMinutes(1);
                    var msgLogicAdd = SceneServer.Instance.LogicAgent.GiveItem(proxy.CharacterId, 1, getExp*2,-1);
                    yield return msgLogicAdd.SendAndWaitUntilDone(coroutine);
                }
                    break;
                case 2: //钻石：4倍经验
                {
                    var gemNeed = ((double) getExp*ObjPlayer.OutLineDiamond/maxExp);
                    var needCount = getExp*ObjPlayer.OutLineDiamond/maxExp;
                    if (gemNeed > 0 && gemNeed < 1)
                    {
                        needCount = 1;
                    }
                    else
                    {
                        needCount = getExp*ObjPlayer.OutLineDiamond/maxExp;
                    }
                    if (needCount > needcount)
                    {
                        msg.Reply((int) ErrorCodes.DiamondNotEnough);
                        yield break;
                    }
                    var msgLogic = SceneServer.Instance.LogicAgent.DeleteItem(proxy.CharacterId, 3, needCount, (int)eDeleteItemType.GetLeaveExp);
                    yield return msgLogic.SendAndWaitUntilDone(coroutine);
                    if (msgLogic.State != MessageState.Reply)
                    {
                        msg.Reply((int) ErrorCodes.Error_TimeOut);
                        yield break;
                    }
                    if (msgLogic.ErrorCode != (int) ErrorCodes.OK)
                    {
                        msg.Reply(msgLogic.ErrorCode);
                        yield break;
                    }
                    character.OutLineExp = 0;
                    character.OutLineTime = DateTime.Now.AddMinutes(1);
                    var msgLogicAdd = SceneServer.Instance.LogicAgent.GiveItem(proxy.CharacterId, 1, getExp*4,-1);
                    yield return msgLogicAdd.SendAndWaitUntilDone(coroutine);
                }
                    break;
                case 3:
                {
                    if (character.OutLineTime > DateTime.Now)
                    {
                        character.OutLineTime = DateTime.Now;
                    }
                }
                    break;
            }
            msg.Reply();
        }

        //请求离线经验数据
        public IEnumerator ApplyLeaveExp(Coroutine coroutine, SceneCharacterProxy charProxy, ApplyLeaveExpInMessage msg)
        {
            var proxy = (SceneProxy) charProxy;
            msg.Response.Exp = proxy.Character.OutLineExp;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------ApplyLeaveExp----------");
            msg.Response.Time = proxy.Character.OutLineTime.ToBinary();
            msg.Reply();
            yield return null;
        }

        public IEnumerator ChangeSceneRequestByMission(Coroutine coroutine,
                                                       SceneCharacterProxy charProxy,
                                                       ChangeSceneRequestByMissionInMessage msg)
        {
            var proxy = (SceneProxy) charProxy;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (character == null)
            {
                Logger.Error("AskEnterDungeon Enter characterId = {0} null", msg.CharacterId);
                yield break;
            }
            var sceneId = msg.Request.SceneId;
            var missionid = msg.Request.MissionId;
            PlayerLog.WriteLog(proxy.CharacterId,
                "----------Scene----------ChangeSceneRequestByMission----------{0},{1}", sceneId, missionid);
            if (sceneId == 0)
            {
                Logger.Warn("DataTable.Table.GetScene({0})==null", sceneId);
                msg.Reply((int) ErrorCodes.Error_NoScene);
                yield break;
            }
            var tbScene = Table.GetScene(sceneId);
            if (null == tbScene)
            {
                Logger.Warn("DataTable.Table.GetScene({0})==null", sceneId);
                msg.Reply((int) ErrorCodes.Error_NoScene);
                yield break;
            }

            if (proxy.Character.IsChangingScene())
            {
                PlayerLog.WriteLog(proxy.CharacterId,
                    "ChangeSceneRequestByMission IsChangingScene(), time = {0}, now = {1}",
                    proxy.Character.LastChangeSceneTime, DateTime.Now);
                msg.Reply((int) ErrorCodes.StateError);
                yield break;
            }

            //硬编码:如果任务120，就给传到某个地方
            if (120 == missionid && sceneId == 5)
            {
                Logger.Debug("120 == missionid");
                msg.Reply((int) ErrorCodes.OK);
                var pos = new Vector2(111.82f, 15.13f);
                proxy.Character.StopMove();
                proxy.Character.SetPosition(pos);
                proxy.Character.SyncCharacterPostion();

                yield break;
            }

            //请求任务ID要去的场景
            var result = SceneServer.Instance.LogicAgent.SSGetMissionEnterScene(proxy.CharacterId, missionid);
            yield return result.SendAndWaitUntilDone(coroutine);
            if (result.State != MessageState.Reply)
            {
                Logger.Error("Cost money result.State={0}", result.State);
                msg.Reply((int) ErrorCodes.Unknow);
                yield break;
            }
            if (result.ErrorCode != (int) ErrorCodes.OK)
            {
                msg.Reply(result.Response);
                yield break;
            }
            if (msg.Request.SceneId != result.Response)
            {
                Logger.Warn("ChangeSceneRequestByMission m={0},cs={1},ls={2}", missionid, sceneId, result.Response);

                sceneId = result.Response;

                // 重新获取Scene配置数据
                tbScene = Table.GetScene(sceneId);
                if (null == tbScene)
                {
                    Logger.Warn("DataTable.Table.GetScene({0})==null", sceneId);
                    msg.Reply((int)ErrorCodes.Error_NoScene);
                    yield break;
                }
            }

            // 检查限制条件
            // 场景是否开放
            //if (tbScene.IsPublic != 1)
            //{
            //    Logger.Warn("Character({0}) : Scene({1}) not public!", proxy.CharacterId, sceneId);
            //    msg.Reply((int)ErrorCodes.Error_SceneNotPublic);
            //    yield break;
            //}

            // 角色等级是否到达
            if (tbScene.LevelLimit > proxy.Character.GetLevel())
            {
                Logger.Warn("Character({0}) : not enough level for transfer to Scene({1}) in mission({2})!", proxy.CharacterId, sceneId, missionid);
                msg.Reply((int)ErrorCodes.Error_LevelNoEnough);
                yield break;
            }

            //// 扣钱
            //if (tbScene.ConsumeMoney > 0)
            //{
            //    var result = SceneServer.Instance.LogicAgent.DeleteItem(proxy.CharacterId, 2, tbScene.ConsumeMoney);
            //    yield return result.SendAndWaitUntilDone(coroutine);
            //    if (result.State != MessageState.Reply)
            //    {
            //        Logger.Error("Cost money result.State={0}", result.State);
            //        msg.Reply((int)ErrorCodes.Unknow);
            //        yield break;
            //    }

            //    // 扣钱出现错误，则直接返回错误码
            //    if (result.Response != (int)ErrorCodes.OK)
            //    {
            //        msg.Reply(result.Response);
            //        yield break;
            //    }
            //}


            var param = new SceneParam();
            var co1 = CoroutineFactory.NewSubroutine(SceneServer.Instance.ServerControl.CreateAndEnterScene,
                coroutine,
                msg.CharacterId, proxy.Character.ServerId,
                sceneId,
                0ul,
                eScnenChangeType.EnterDungeon,
                param);
            if (co1.MoveNext())
            {
                yield return co1;
            }
            msg.Reply();
        }

        public IEnumerator ApplyPlayerPostionList(Coroutine coroutine,
                                                  SceneCharacterProxy _this,
                                                  ApplyPlayerPostionListInMessage msg)
        {
            var proxy = (SceneProxy) _this;
            var charIds = msg.Request.CharacterIds;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------ApplyPlayerPostionList----------:{0}",
                charIds);
            var ret = msg.Response;
            foreach (var i in charIds.Items)
            {
                var obj = CharacterManager.Instance.GetCharacterControllerFromMemroy(i);
                if (obj == null || obj.Scene == null || proxy.Character == null || proxy.Character.Scene == null ||
                    obj.Scene.Guid != proxy.Character.Scene.Guid)
                {
                    ret.List.Add(new Vector2Int32 {x = -1, y = -1});
                }
                else
                {
                    var pos = Utility.MakeVectorMultiplyPrecision(obj.GetPosition().X, obj.GetPosition().Y);
                    ret.List.Add(pos);
                }
            }
            msg.Reply();
            yield break;
        }

        public IEnumerator AllianceWarRespawnGuard(Coroutine co,
                                                   SceneCharacterProxy _this,
                                                   AllianceWarRespawnGuardInMessage msg)
        {
            var proxy = (SceneProxy) _this;
            var character = proxy.Character;
            var tbGA = Table.GetGuildAccess((int) character.Ladder);
            if (tbGA.CanRebornGuard != 1)
            {
                msg.Reply((int) ErrorCodes.Error_JurisdictionNotEnough);
                yield break;
            }
            var scene = character.Scene as AllianceWar;
            if (scene == null)
            {
                msg.Reply((int) ErrorCodes.Error_SceneIdNotMatch);
                yield break;
            }
            var idx = msg.Request.Index;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------AllianceWarRespawnGuard----------:{0}", idx);
            var err = AsyncReturnValue<ErrorCodes>.Create();
            var co1 = CoroutineFactory.NewSubroutine(scene.RespawnGuard, co, character, idx, err);
            if (co1.MoveNext())
            {
                yield return co1;
            }
            var result = err.Value;
            err.Dispose();
            msg.Reply((int) result);
        }

        public IEnumerator GetSceneNpcPos(Coroutine coroutine, SceneCharacterProxy _this, GetSceneNpcPosInMessage msg)
        {
            var proxy = (SceneProxy)_this;
            var character = proxy.Character;
            var scene = character.Scene as MieShiWar;
            if (scene == null)
            {
                msg.Reply((int)ErrorCodes.Error_SceneIdNotMatch);
                yield break;
            }
            var idx = msg.Request.Placeholder;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------GetSceneNpcPos----------:{0}", idx);
            scene.GetSceneNpcPosList(msg.Response);
            msg.Reply();            
        }

        public IEnumerator OnConnected(Coroutine coroutine, SceneCharacterProxy charProxy, uint packId)
        {
            //SceneProxy proxy = (SceneProxy)charProxy;

            //ConnectLostLogger.Info("character {0} - {1} Scene OnConnected 1", proxy.CharacterId, proxy.ClientId);

            //Logger.Info("[{0}] has enter connected", proxy.CharacterId);

            //PlayerLog.WriteLog(proxy.CharacterId, "-----Scene-----OnConnected----------{0}", proxy.CharacterId);

            //Logger.Info("Enter Game {0} - OnConnected - 1 - {1}", proxy.CharacterId, TimeManager.Timer.ElapsedMilliseconds);

            //PlayerLog.WriteLog(888, "OnConnected characterId={0},clientId={1}", proxy.CharacterId, proxy.ClientId);
            ////PlayerLog.WriteLog(proxy.CharacterId, "------Scene----OnConnected----------");
            ////var result = AsyncReturnValue<ObjPlayer>.Create();
            ////var co = CoroutineFactory.NewSubroutine(CharacterManager.Instance.GetOrCreateCharacterController, coroutine, proxy.CharacterId, new object[] { }, false, result);
            ////if (co.MoveNext())
            ////{
            ////    yield return co;
            ////}

            ////Logger.Info("Enter Game {0} - OnConnected - 2 - {1}", proxy.CharacterId, TimeManager.Timer.ElapsedMilliseconds);

            ////var obj = result.Value;
            ////result.Dispose();
            ////if (obj == null)
            ////{
            ////    yield break;
            ////}


            //ObjPlayer obj = CharacterManager.Instance.GetCharacterControllerFromMemroy(proxy.CharacterId);
            //if (obj == null)
            //{
            //    Logger.Fatal("onConnected CharacterId={0}", proxy.CharacterId);
            //    yield break;
            //}
            //proxy.Character = obj;
            //obj.Proxy = proxy;

            ////同步名字
            //var dbLoginSimple = SceneServer.Instance.LoginAgent.GetLoginSimpleData(proxy.ClientId, proxy.CharacterId);
            //yield return dbLoginSimple.SendAndWaitUntilDone(coroutine);
            //if (dbLoginSimple.State != MessageState.Reply)
            //{
            //    yield break;
            //}
            //if (dbLoginSimple.ErrorCode != (int)ErrorCodes.OK)
            //{
            //    yield break;
            //}
            //if (obj.Proxy == null)
            //{
            //    Logger.Warn("Scene OnConnected obj.Proxy is null! type=1, objId={0}", obj.ObjId);
            //    yield break;
            //}
            //obj.SetName(dbLoginSimple.Response.Name);

            ////同步技能数据
            //var skills = SceneServer.Instance.LogicAgent.LogicGetSkillData(proxy.CharacterId, proxy.CharacterId);
            //yield return skills.SendAndWaitUntilDone(coroutine);
            //if (skills.State != MessageState.Reply)
            //{
            //    yield break;
            //}
            //if (skills.ErrorCode != (int)ErrorCodes.OK)
            //{
            //    yield break;
            //}

            //if (obj.Proxy == null)
            //{
            //    Logger.Warn("Scene OnConnected obj.Proxy is null! type=2, objId={0}", obj.ObjId);
            //    yield break;
            //}

            //Logger.Info("Enter Game {0} - OnConnected - 3 - {1}", proxy.CharacterId, TimeManager.Timer.ElapsedMilliseconds);

            //obj.ApplySkill(skills.Response.Data);
            //obj.ChangeOutLineTime();
            ////同步装备数据
            //var equips = SceneServer.Instance.LogicAgent.LogicGetEquipList(proxy.CharacterId, proxy.CharacterId);
            //yield return equips.SendAndWaitUntilDone(coroutine);
            //if (equips.State != MessageState.Reply)
            //{
            //    yield break;
            //}
            //if (equips.ErrorCode != (int)ErrorCodes.OK)
            //{
            //    yield break;
            //}
            //if (obj.Proxy == null)
            //{
            //    Logger.Warn("Scene OnConnected obj.Proxy is null! type=3, objId={0}", obj.ObjId);
            //    yield break;
            //}
            //obj.ApplyEquip(equips.Response);

            //Logger.Info("Enter Game {0} - OnConnected - 4 - {1}", proxy.CharacterId, TimeManager.Timer.ElapsedMilliseconds);

            ////同步天赋数据
            //var talents = SceneServer.Instance.LogicAgent.LogicGetTalentData(proxy.CharacterId, proxy.CharacterId);
            //yield return talents.SendAndWaitUntilDone(coroutine);
            //if (talents.State != MessageState.Reply)
            //{
            //    yield break;
            //}
            //if (talents.ErrorCode != (int)ErrorCodes.OK)
            //{
            //    yield break;
            //}
            //if (obj.Proxy == null)
            //{
            //    Logger.Warn("Scene OnConnected obj.Proxy is null! type=4, objId={0}", obj.ObjId);
            //    yield break;
            //}
            //obj.ApplyTalent(talents.Response.Data);

            //Logger.Info("Enter Game {0} - OnConnected - 5 - {1}", proxy.CharacterId, TimeManager.Timer.ElapsedMilliseconds);

            ////同步图鉴数据
            //var books = SceneServer.Instance.LogicAgent.LogicGetBookAttrData(proxy.CharacterId, proxy.CharacterId);
            //yield return books.SendAndWaitUntilDone(coroutine);
            //if (books.State != MessageState.Reply)
            //{
            //    yield break;
            //}
            //if (books.ErrorCode != (int)ErrorCodes.OK)
            //{
            //    yield break;
            //}
            //if (obj.Proxy == null)
            //{
            //    Logger.Warn("Scene OnConnected obj.Proxy is null! type=5, objId={0}", obj.ObjId);
            //    yield break;
            //}
            //obj.ApplyBookAttr(books.Response.Data);


            //Logger.Info("Enter Game {0} - OnConnected - 6 - {1}", proxy.CharacterId, TimeManager.Timer.ElapsedMilliseconds);

            ////同步称号数据
            //var titles = SceneServer.Instance.LogicAgent.LogicGetTitleList(proxy.CharacterId, 0);
            //yield return titles.SendAndWaitUntilDone(coroutine);
            //if (titles.State != MessageState.Reply)
            //{
            //    yield break;
            //}
            //if (titles.ErrorCode != (int)ErrorCodes.OK)
            //{
            //    yield break;
            //}
            //if (obj.Proxy == null)
            //{
            //    Logger.Warn("Scene OnConnected obj.Proxy is null! type=6, objId={0}", obj.ObjId);
            //    yield break;
            //}
            //obj.ApplyTitles(titles.Response.EquipedTitles.Items, 0);
            //obj.ApplyTitles(titles.Response.Titles.Items, 1);

            //Logger.Info("Enter Game {0} - OnConnected - 7 - {1}", proxy.CharacterId, TimeManager.Timer.ElapsedMilliseconds);

            ////同步队伍数据
            //var team = SceneServer.Instance.TeamAgent.SSGetTeamData(proxy.CharacterId, proxy.CharacterId);
            //yield return team.SendAndWaitUntilDone(coroutine);
            //if (team.State == MessageState.Reply)
            //{
            //    if (team.ErrorCode == (int)ErrorCodes.OK)
            //    {
            //        obj.SetTeamId(team.Response.TeamId, team.Response.State);
            //    }
            //}
            //if (obj.Proxy == null)
            //{
            //    Logger.Warn("Scene OnConnected obj.Proxy is null! type=7, objId={0}", obj.ObjId);
            //    yield break;
            //}
            //Logger.Info("Enter Game {0} - OnConnected - 8 - {1}", proxy.CharacterId, TimeManager.Timer.ElapsedMilliseconds);

            ////获取viplevel
            //var vip = SceneServer.Instance.LogicAgent.GetItemCount(proxy.CharacterId, (int) eResourcesType.VipLevel);
            //yield return vip.SendAndWaitUntilDone(coroutine);
            //if (vip.State != MessageState.Reply)
            //{
            //    yield break;
            //}
            //if (vip.ErrorCode != (int)ErrorCodes.OK)
            //{
            //    yield break;
            //}
            //if (obj.Proxy == null)
            //{
            //    Logger.Warn("Scene OnConnected obj.Proxy is null! type=8, objId={0}", obj.ObjId);
            //    yield break;
            //}
            //obj.SetItemCount((int)eResourcesType.VipLevel, vip.Response);


            //Logger.Info("Enter Game {0} - OnConnected - 9 - {1}", proxy.CharacterId, TimeManager.Timer.ElapsedMilliseconds);

            ////同步战盟数据
            //var alliance = SceneServer.Instance.TeamAgent.SSGetAllianceData(proxy.CharacterId, obj.ServerId);
            //yield return alliance.SendAndWaitUntilDone(coroutine);
            //if (obj.Proxy == null)
            //{
            //    Logger.Warn("Scene OnConnected obj.Proxy is null! type=9, objId={0}", obj.ObjId);
            //    yield break;
            //}
            //if (alliance.State == MessageState.Reply)
            //{
            //    if (alliance.ErrorCode == (int)ErrorCodes.OK)
            //    {
            //        var response = alliance.Response;
            //        obj.SetAllianceInfo(response.AllianceId, response.Ladder, response.Name);
            //    }
            //}

            //Logger.Info("Enter Game {0} - OnConnected - 10 - {1}", proxy.CharacterId, TimeManager.Timer.ElapsedMilliseconds);

            ////同步战盟Buff数据
            //var allianceBuff = SceneServer.Instance.LogicAgent.SSGetAllianceBuff(proxy.CharacterId, 0);
            //yield return allianceBuff.SendAndWaitUntilDone(coroutine);
            //if (obj.Proxy == null)
            //{
            //    Logger.Warn("Scene OnConnected obj.Proxy is null! type=10, objId={0}", obj.ObjId);
            //    yield break;
            //}
            //if (allianceBuff.State == MessageState.Reply)
            //{
            //    if (allianceBuff.ErrorCode == (int)ErrorCodes.OK)
            //    {
            //        foreach (int buffId in allianceBuff.Response.Items)
            //        {
            //            if (buffId <= 0)
            //            {
            //                continue;
            //            }
            //            var buff = Table.GetGuildBuff(buffId);
            //            if (buff == null) continue;
            //            obj.AddBuff(buff.BuffID, buff.BuffLevel, obj);
            //        }
            //    }
            //}

            //obj.Attr.mFlag.ReSetAllFlag(true);
            //obj.Attr.EquipRefresh();
            //obj.Attr.InitAttributesAll();
            //obj.Attr.SetFightPointFlag();

            //Logger.Info("Enter Game {0} - OnConnected - 11 - {1}", proxy.CharacterId, TimeManager.Timer.ElapsedMilliseconds);

            ////同步好友数据
            //var friend = SceneServer.Instance.LogicAgent.SSGetFriendList(proxy.CharacterId, 0);
            //yield return friend.SendAndWaitUntilDone(coroutine);
            //if (obj.Proxy == null)
            //{
            //    Logger.Warn("Scene OnConnected obj.Proxy is null! type=11, objId={0}", obj.ObjId);
            //    yield break;
            //}
            //if (friend.State == MessageState.Reply)
            //{
            //    if (friend.ErrorCode == (int)ErrorCodes.OK)
            //    {
            //        if (friend.Response.Data.Count > 0)
            //        {
            //            var fl = new Uint64Array();
            //            foreach (var dic in friend.Response.Data)
            //            {
            //                foreach (var i in dic.Value.Items)
            //                {
            //                    fl.Items.Add(i);
            //                }
            //            }
            //            var Sbfriend = SceneServer.Instance.SceneAgent.SBCheckCharacterOnline(proxy.CharacterId, fl);
            //            yield return Sbfriend.SendAndWaitUntilDone(coroutine);
            //            if (Sbfriend.State == MessageState.Reply)
            //            {
            //                if (Sbfriend.ErrorCode == (int)ErrorCodes.OK)
            //                {
            //                    var index = 0;
            //                    foreach (var dic in friend.Response.Data)
            //                    {
            //                        var friendType = dic.Key;
            //                        foreach (var i in dic.Value.Items)
            //                        {
            //                            if (Sbfriend.Response.Items[index] == 1)
            //                            {
            //                                obj.PushFriend(friendType, i);
            //                            }
            //                            index++;
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            //SceneManager.Instance.CheckAvgLevelBuff(obj);
            //Logger.Info("Enter Game {0} - OnConnected - 12 - {1}", proxy.CharacterId, TimeManager.Timer.ElapsedMilliseconds);

            //if (obj.mDbData.Hp < 1)
            //{
            //    var AutoReliveTime = DateTime.FromBinary(obj.mDbData.AutoRelive);
            //    if (AutoReliveTime < DateTime.Now)
            //    {
            //        //需要复活
            //        Logger.Info("[{0}] Need AutoRelive", proxy.CharacterId);
            //        obj.Attr.SetDataValue(eAttributeType.HpNow, obj.mDbData.Hp);
            //        obj.Attr.SetDataValue(eAttributeType.MpNow, obj.mDbData.Mp);
            //    }
            //    else
            //    {
            //        obj.Attr.SetDataValue(eAttributeType.HpNow, obj.mDbData.Hp);
            //        obj.Attr.SetDataValue(eAttributeType.MpNow, obj.mDbData.Mp);
            //        obj.OnlineDie();
            //    }
            //}
            //else
            //{
            //    obj.Attr.SetDataValue(eAttributeType.HpNow, obj.mDbData.Hp);
            //    if (obj.mDbData.Mp < 1)
            //    {
            //        obj.Attr.SetDataValue(eAttributeType.MpNow, obj.GetAttribute(eAttributeType.MpMax));
            //    }
            //    else
            //    {
            //        obj.Attr.SetDataValue(eAttributeType.MpNow, obj.mDbData.Mp);
            //    }
            //}
            //{
            //    Logger.Info("Enter Game {0} - OnConnected - 13 - {1}", proxy.CharacterId, TimeManager.Timer.ElapsedMilliseconds);

            //    Logger.Info("[{0}] has connected", proxy.CharacterId);

            //    proxy.Connected = true;
            //    if (proxy.Character != null)
            //    {
            //        proxy.Character.State = CharacterState.Connected;
            //    }

            //    //var notifyLoginConnectedMsg = SceneServer.Instance.LoginAgent.NotifyConnected(proxy.ClientId,proxy.CharacterId, (int) ServiceType.Scene, (int) ErrorCodes.OK);
            //    //yield return notifyLoginConnectedMsg.SendAndWaitUntilDone(coroutine);

            //    //var notifyBrokerConnectedMsg = SceneServer.Instance.SceneAgent.NotifyConnected(proxy.CharacterId, 0);
            //    //notifyBrokerConnectedMsg.mMessage.PacketId = packId;
            //    //yield return notifyBrokerConnectedMsg.SendAndWaitUntilDone(coroutine);

            //    //foreach (var waitingCheckConnectedInMessage in proxy.WaitingCheckConnectedInMessages)
            //    //{
            //    //    waitingCheckConnectedInMessage.Response = 1;
            //    //    waitingCheckConnectedInMessage.Reply();
            //    //}
            //    //proxy.WaitingCheckConnectedInMessages.Clear();

            //    CharacterManager.Instance.UpdateSimpleData(proxy.CharacterId);

            //    Logger.Info("Enter Game {0} - OnConnected - 14 - {1}", proxy.CharacterId, TimeManager.Timer.ElapsedMilliseconds);

            //    var msg1 = SceneServer.Instance.GameMasterAgent.CharacterConnected(proxy.CharacterId, proxy.CharacterId, (int)ServiceType.Scene);
            //    yield return msg1.SendAndWaitUntilDone(coroutine);
            //}
            yield break;
        }

        public IEnumerator OnLost(Coroutine coroutine, SceneCharacterProxy charProxy, uint packId)
        {
//             SceneProxy proxy = (SceneProxy)charProxy;
//             if (packId == uint.MaxValue)
//             {
//                 // TODO: 这说明只是切场景的时候UnloadData，不是真正的下线
//                 PlayerLog.WriteLog(888, "SceneServer OnLost LeaveScene characterId={0}", proxy.CharacterId);
//             }
//             else
//             {
//                 PlayerLog.WriteLog(888, "SceneServer OnLost characterId={0}", proxy.CharacterId);
//             }
// 
//             ConnectLostLogger.Info("character {0} - {1} Scene OnLost 1", proxy.CharacterId, proxy.ClientId);
//             Logger.Info("Enter Game {0} - OnLost - 1 - {1}", proxy.CharacterId, TimeManager.Timer.ElapsedMilliseconds);
//             PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------OnLost----------");
// 
// 	        var player = proxy.Character;
// 			if (player == null)
// 			{//如果引用为空就就在CharacterManager里找
// 				player = CharacterManager.Instance.GetCharacterControllerFromMemroy(proxy.CharacterId);
// 	            if (null == player)
// 	            {
// 					Logger.Error("OnLost null == player[{0}]", proxy.CharacterId);
// 					yield break;    
// 	            }
//             }
// 
// 	        try
// 	        {
// 		        player.BuffList.OnLost();
// 		        player.FriendsLostTriggerTimeOver();
// 
// 		        if (null != player.Scene)
// 		        {
// 			        player.Scene.LeaveScene(player);
// 		        }
// 	        }
// 	        catch (Exception e)
// 	        {
// 		        Logger.Error("OnLost: " + e.Message);
// 	        }
// 	        
//             Logger.Info("[" + proxy.CharacterId + "] has lost connection");
//             //TODO
//             //掉线移出该玩家，暂时不考虑重连的情况
//             var co = CoroutineFactory.NewSubroutine(CharacterManager.Instance.RemoveCharacter, coroutine, proxy.CharacterId);
//             if (co.MoveNext())
//                 yield return co;
// 
//             //Logger.Info("Enter Game {0} - OnLost - 2 - {1} {2} {3}", proxy.CharacterId, proxy.Character.Scene != null ? proxy.Character.Scene.TypeId : -1, proxy.Character.mDbData.SceneId, TimeManager.Timer.ElapsedMilliseconds);
// 
// 			//互相引用清除掉
// 			player.Proxy = null;
// 			proxy.Character = null;
// 
//             proxy.Connected = false;
            //foreach (var waitingCheckLostInMessage in proxy.WaitingCheckLostInMessages)
            //{
            //    waitingCheckLostInMessage.Reply();
            //}
            //proxy.WaitingCheckLostInMessages.Clear();

            yield break;
        }

        public bool OnSyncRequested(SceneCharacterProxy _this, ulong characterId, uint syncId)
        {
            return false;
        }

        public IEnumerator ApplyAttribute(Coroutine coroutine,
                                          SceneCharacterProxy charProxy,
                                          ApplyAttributeInMessage msg)
        {
            var proxy = (SceneProxy) charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------ApplyAttribute----------");
            //msg.Response.Items.AddRange(Character.Attr.mData);
            for (var i = 0; i < (int) eAttributeType.AttrCount; i++)
            {
                msg.Response.Items.Add(proxy.Character.Attr.GetDataValue((eAttributeType) i));
            }
            msg.Reply();
            yield break;
        }

        public IEnumerator CreateObjAround(Coroutine coroutine, SceneCharacterProxy _this, CreateObjAroundInMessage msg)
        {
            var proxy = (SceneProxy) _this;
            var msg2Me = new CreateObjMsg();
            PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------CreateObjAround----------");
            var character = proxy.Character;
            foreach (var zone in character.Zone.VisibleZoneList)
            {
                foreach (var pair in zone.ObjDict)
                {
                    var obj = pair.Value;

                    if (!obj.Active)
                    {
                        continue;
                    }

                    if (obj.ObjId == character.ObjId)
                    {
                        continue;
                    }

                    //如果obj
                    if (!obj.IsVisibleTo(character))
                    {
                        continue;
                    }

                    var data = obj.DumpObjData(ReasonType.VisibilityChanged);
                    msg2Me.Data.Add(data);
                }
            }

            msg.Response = msg2Me;
            msg.Reply();

            return null;
        }

        public IEnumerator ApplyPlayerData(Coroutine coroutine,
                                           SceneCharacterProxy charProxy,
                                           ApplyPlayerDataInMessage msg)
        {
            var proxy = (SceneProxy) charProxy;
            var data = msg.Response;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------ApplyPlayerData----------");
            foreach (var buff in proxy.Character.BuffList.mData)
            {
                if (buff.mBuff.Effect[0] != -1)
                {
                    var newBuff = new BuffResult
                    {
                        TargetObjId = proxy.Character.ObjId,
                        BuffTypeId = buff.GetBuffId(),
                        BuffId = buff.mId,
                        Type = BuffType.HT_ADDBUFF
                    };
                    if (buff.mBuff.IsView == 1)
                    {
                        newBuff.Param.Add(buff.GetLastSeconds());
                        newBuff.Param.Add(buff.GetLayer());
                        newBuff.Param.Add(buff.m_nLevel);
                    }
                    data.Buff.Add(newBuff);
                }
            }
            data.MountId = proxy.Character.GetMountId();
            msg.Reply();
            yield return null;
        }

        /// <summary>
        ///     客户端加载新的场景
        ///     设置新场景的可见人
        ///     服务器给玩家设置新场景信息，刷新其他玩家
        /// </summary>
        /// <param name="coroutine"></param>
        /// <param name="charProxy"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public IEnumerator ChangeSceneOver(Coroutine coroutine,
                                           SceneCharacterProxy charProxy,
                                           ChangeSceneOverInMessage msg)
        {
            var proxy = (SceneProxy) charProxy;
            var sceneid = msg.Request.SceneId;
            var sceneguid = msg.Request.SceneGuid;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------ChangeSceneOver----------:{0},{1}", sceneid,
                sceneguid);

            var character = proxy.Character;
            if (null == character)
            {
                Logger.Warn("ChangeSceneOver null == character      Character={0}", proxy.CharacterId);
                yield break;
            }

            if (null == character.Scene)
            {
                Logger.Warn("ChangeSceneOver null == Scene      Character={0}", character.ObjId);
                yield break;
            }
//             if (msg.Request.SceneGuid != character.Scene.Guid)
//             {
//                 Logger.Warn(
//                     "ChangeSceneOver character not this scene!    Character={0},ClientScene={1},ServerScene={2}",
//                     character.ObjId, msg.Request.SceneGuid, character.Scene.Guid);
//                 yield break;
//             }
            //SceneManager.Instance.EnterScene(Character, msg.Request.SceneId);
            if (character.Active)
            {
                Logger.Warn("ChangeSceneOver character.Active == true      Character={0}", character.ObjId);
                yield break;
            }

            Logger.Info("Enter Game {0} - ChangeSceneOver - 1 - {1}", msg.CharacterId,
                TimeManager.Timer.ElapsedMilliseconds);
            character.EnterSceneOver();

			msg.Response = proxy.Character.Scene.Exdata;
			msg.Reply();
        }

        public IEnumerator StopMove(Coroutine coroutine, SceneCharacterProxy charProxy, StopMoveInMessage msg)
        {
            var proxy = (SceneProxy) charProxy;
            if (proxy == null || proxy.Character == null)
            {
                Logger.Error("----------Scene----------StopMove----proxy == null------msg.CharacterId:{0}",
                    msg.CharacterId);
                msg.Reply((int) ErrorCodes.Unknow);
            }
            var x = msg.Request.Pos.Pos.x;
            var y = msg.Request.Pos.Pos.y;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------StopMove----------:{0},{1}", x, y);
            if (null == proxy.Character.Scene || proxy.Character.Scene.TableSceneData == null)
            {
                Logger.Warn("StopMove  null == Scene      Character{0}", proxy.Character.ObjId);
                msg.Reply((int) ErrorCodes.Unknow);
                yield break;
            }
            if (proxy.Character.Scene.TableSceneData.Id != msg.Request.SceneId)
            {
                Logger.Warn("StopMove  null == Scene      Character{0}  sceneid={1}  msgSceneId={2}", proxy.Character.ObjId, proxy.Character.Scene.TableSceneData.Id, msg.Request.SceneId);
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            var p = Utility.MakeVectorDividePrecision(x, y);
            var dir = Utility.MakeVectorDividePrecision(msg.Request.Pos.Dir.x, msg.Request.Pos.Dir.y);
            if (StaticVariable.IsTestPosition == true)
            {
                const float MaxErrorDistance = 5;
                if ((proxy.Character.GetPosition() - p).Length() > MaxErrorDistance)
                {
                    Logger.Warn("StopMove Character[{0}] Error_DistanceTooMuch", proxy.Character.ObjId);
                    msg.Response = Utility.MakePositionDataByPosAndDir(proxy.Character.GetPosition(), proxy.Character.GetDirection());
                    proxy.Character.StopMove();
                    msg.Reply((int)ErrorCodes.Error_DistanceTooMuch, true);
                    yield break;
                }                
            }

            

            proxy.Character.SetPosition(p);
            proxy.Character.SetDirection(dir);
            proxy.Character.StopMove();

            msg.Reply((int)ErrorCodes.OK);
        }

        public IEnumerator MoveTo(Coroutine coroutine, SceneCharacterProxy charProxy, MoveToInMessage msg)
        {
            var proxy = (SceneProxy) charProxy;
//             PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------MoveTo----------:{0},{1}",msg.Request.Offset,msg.Request.Time);
//             foreach (var items in msg.Request.TargetList.List)
//             {
//                 PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------MoveToTargetList----------{0}", items);
//                 
//             }
            if (proxy == null)
            {
                msg.Reply((int) ErrorCodes.Error_CharacterNoScene, true);
                Logger.Warn("MoveTo  null == proxy");
                yield break;
            }

            if (proxy.Character == null)
            {
                msg.Reply((int) ErrorCodes.Error_NoObj, true);
                Logger.Warn("MoveTo  null == Character Character{0}", proxy.CharacterId);
                yield break;
            }



            

            msg.Response = Utility.MakePositionDataByPosAndDir(proxy.Character.GetPosition(),
                proxy.Character.GetDirection());
            msg.Response.Time = DateTime.Now.ToBinary();
            if (null == proxy.Character.Scene || proxy.Character.Scene.TableSceneData == null)
            {
                msg.Reply((int) ErrorCodes.Error_CharacterNoScene, true);
                Logger.Warn("MoveTo  null == Scene      Character{0}", proxy.Character.ObjId);
                yield break;
            }
            if (proxy.Character.Scene.TableSceneData.Id != msg.Request.SceneId)
            {
                msg.Reply((int)ErrorCodes.Error_CharacterNoScene, true);
                Logger.Warn("MoveTo  null == Scene      Character{0} curSceneId={1} msgSceneId={2}", proxy.Character.ObjId,proxy.Character.Scene.TableSceneData.Id,msg.Request.SceneId);
                yield break;
            }
            if (proxy.Character.IsDead())
            {
                msg.Reply((int) ErrorCodes.Error_CharacterDie, true);
                Logger.Warn("MoveTo       Character{0} has been dead", proxy.Character.ObjId);
                yield break;
            }

            if (!proxy.Character.CanMove())
            {
                var isCanmove = false;
                if (proxy.Character.lastNomoveFlag)
                {
                    if (DateTime.Now.GetDiffSeconds(proxy.Character.lastNomoveTime) > 5)
                    {
                        proxy.Character.BuffList.SetSpecialStateFlag();
                        if (proxy.Character.CanMove())
                        {
                            isCanmove = true;
                            Logger.Warn("MoveTo Character{0} !Character.CanMove(), {1}, {2}, {3}", proxy.Character.ObjId,
                                proxy.Character.BuffList, proxy.Character.BuffList.mSpecialStateFlag,
                                proxy.Character.BuffList._mNoMove);
                        }
                    }
                }
                else
                {
                    proxy.Character.lastNomoveFlag = true;
                    proxy.Character.lastNomoveTime = DateTime.Now;
                }
                if (!isCanmove)
                {
                    msg.Reply((int) ErrorCodes.Error_CannotMove, true);
                    yield break;
                }
            }
            if (StaticVariable.IsTestPosition == false && proxy.Character.Scene.TableSceneData != null && proxy.Character.Scene.TableSceneData.Id == msg.Request.SceneId)
            {
                var curPos = Utility.MakeVectorDividePrecision(msg.Request.TargetList.List[0].x,
                    msg.Request.TargetList.List[0].y);
                proxy.Character.SetPosition(curPos);
            }
            proxy.Character.lastNomoveFlag = false;
            var currentPos = Vector2.Zero;
            if (msg.Request.TargetList.List.Count > 0)
            {
                var p = msg.Request.TargetList.List[0];
                currentPos = Utility.MakeVectorDividePrecision(p.x, p.y);

                var diff = (proxy.Character.GetPosition() - currentPos).Length();

                if (diff > proxy.Character.GetMoveSpeed())
                {
                    msg.Reply((int) ErrorCodes.Error_PositionUnsync, true);
                    yield break;
                }
            }

            var list = new List<Vector2>();
            for (var i = 1; i < msg.Request.TargetList.List.Count; ++i)
            {
                var p = msg.Request.TargetList.List[i];

                var pos = Utility.MakeVectorDividePrecision(p.x, p.y);
                list.Add(pos);
            }

            //if (!proxy.Character.Scene.ValidPath(proxy.Character.GetPosition(), list))
            //{
            //    msg.Reply((int)ErrorCodes.Error_PathInvalid, true);
            //    Logger.Warn("MoveTo Character{0} !Character.Scene.ValidPath(Character.GetPosition(), list)",
            //        proxy.Character.ObjId);
            //    yield break;
            //}

            proxy.Character.MoveToTarget(list, 0.0f);
            proxy.Character.ChangeOutLineTime();
            msg.Reply();
        }

        public IEnumerator MoveToRobot(Coroutine coroutine, SceneCharacterProxy charProxy, MoveToRobotInMessage msg)
        {
            var proxy = (SceneProxy) charProxy;
            var posx = msg.Request.Postion.x;
            var posy = msg.Request.Postion.y;
            //PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------MoveToRobot----------:{0},{1}", posx, posy);
            if (proxy.Character.IsDead())
            {
                msg.Reply((int) ErrorCodes.Unknow);
                Logger.Info("MoveToRobot       Character{0} has been dead", proxy.Character.ObjId);
                yield return null;
            }

            var pos = Utility.MakeVectorDividePrecision(posx, posy);
            var posChar = proxy.Character.GetPosition();

            var dis = Vector2.Distance(posChar, pos);
            if (dis < 0.5)
            {
                msg.Reply((int) ErrorCodes.Error_Robot_PostionSame);
                yield break;
            }
            if (proxy.Character.Scene == null)
            {
                msg.Reply((int) ErrorCodes.Unknow);
                yield break;
            }
            var p = proxy.Character.Scene.FindNearestValidPosition(pos);
            if (p == null)
            {
                msg.Reply((int) ErrorCodes.Unknow);
                yield break;
            }

            var result = AsyncReturnValue<List<Vector2>>.Create();
            yield return proxy.Character.Scene.FindPathTo(coroutine, proxy.Character, p.Value, result);
            var path = result.Value;
            result.Dispose();
            if (path.Count == 0)
            {
                msg.Reply((int) ErrorCodes.Unknow);
                yield break;
            }

            var v2PosChar = new Vector2Int32
            {
                x = Utility.MultiplyPrecision(posChar.X),
                y = Utility.MultiplyPrecision(posChar.Y)
            };
            msg.Response.List.Add(v2PosChar);


            foreach (var vector2 in path)
            {
                var v2 = new Vector2Int32
                {
                    x = Utility.MultiplyPrecision(vector2.X),
                    y = Utility.MultiplyPrecision(vector2.Y)
                };
                msg.Response.List.Add(v2);
            }
            proxy.Character.MoveToTarget(path, 0.01f);
            msg.Reply();
        }

        public IEnumerator DirectTo(Coroutine coroutine, SceneCharacterProxy charProxy, DirectToInMessage msg)
        {
            var proxy = (SceneProxy) charProxy;
            var dirx = msg.Request.DirX;
            var dirz = msg.Request.DirZ;
            //PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------DirectTo----------:{0},{1}", dirx, dirz);
            if (null == proxy.Character.Scene)
            {
                Logger.Warn("DirectTo   null == Scene      Character{0}", proxy.Character.ObjId);
                yield break;
            }

            var p = Utility.MakeVectorDividePrecision(dirx, dirz);
            proxy.Character.SetDirection(p);
            proxy.Character.BroadcastDirection();
            msg.Reply();
        }

        public IEnumerator SendUseSkillRequest(Coroutine coroutine,
                                               SceneCharacterProxy charProxy,
                                               SendUseSkillRequestInMessage msg)
        {

            var proxy = (SceneProxy) charProxy;

            if (proxy.Character == null)
            {
                msg.Reply((int) ErrorCodes.Error_NoObj, true);
                Logger.Warn("SendUseSkillRequest Character is null {0}", proxy.CharacterId);
                yield break;
            }

            if (proxy.Character.Scene == null || proxy.Character.Scene.TableSceneData == null)
            {
                msg.Reply((int) ErrorCodes.Unknow);
                yield break;
            }
            if (proxy.Character.Scene.TableSceneData.Id != msg.Request.SceneId)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                Logger.Warn("SendUseSkillRequest scene is null {0} sceneid={1} msgSceneID={2}", proxy.CharacterId,proxy.Character.Scene.TableSceneData.Id,msg.Request.SceneId);
                yield break;
            }
            var skillId = msg.Request.Msg.SkillId;
            //PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------SendUseSkillRequest----------:{0}", skillId);
            var skillPos = new Vector2(Utility.DividePrecision(msg.Request.Msg.Pos.Pos.x),
                Utility.DividePrecision(msg.Request.Msg.Pos.Pos.y));
            //if (StaticVariable.IsTestPosition == false)
            {
               // if ((skillPos - proxy.Character.GetPosition()).LengthSquared() < SceneProxy.SkillErrorDistance)   //这里会引起一个Bug暂时先注释掉  会引起玩家释放完技能后立即移动,会被拉回技能释放点
                {//暂时这样处理回头得改成用sceneId判断 
                    proxy.Character.SetPosition(skillPos);
                }                
            }

            //这里把角色位置强制移到技能位置  这里风险很大暂时先改一下测一下不行就干掉想别的办法
            //proxy.Character.SetPosition(skillPos);
            
            proxy.Character.SetDirection(Utility.MakeVectorDividePrecision(msg.Request.Msg.Pos.Dir.x,
                msg.Request.Msg.Pos.Dir.y));
            var Pos = proxy.Character.GetPosition();
            var tag = proxy.Character.Scene.GetObstacleValue(Pos.X, Pos.Y);
            if (tag == SceneObstacle.ObstacleValue.Walkable)
            {
                //走
                msg.Reply((int) ErrorCodes.Error_SafeArea);
                yield break;
            }
            ObjCharacter target = null;
            if (msg.Request.Msg.TargetObjId.Count > 0)
            {
                target = proxy.Character.Scene.FindCharacter(msg.Request.Msg.TargetObjId[0]);
                Logger.Info("SendUseSkillRequest skillId={0}  objId={1}", skillId, msg.Request.Msg.TargetObjId[0]);
            }
            else
            {
                Logger.Info("SendUseSkillRequest skillId={0}", skillId);
            }
            var erroCode = proxy.Character.RequestUseSkill(skillId, msg.Response.Items, target);
            Logger.Info("SendUseSkillRequest UseSkill reusult={0}", erroCode);
            if (ErrorCodes.OK != erroCode)
            {
                msg.Reply((int) erroCode);
                Logger.Info("[{0}] use skill[{1}] failed [error={2}]", proxy.CharacterId, skillId, erroCode);
                yield break;
            }
            //msg.Response = skillId;
            proxy.Character.ChangeOutLineTime();
            msg.Reply();

            //SceneServerMonitor.UseSkillRate.Mark();

        }

        //传送
        public IEnumerator SendTeleportRequest(Coroutine coroutine,
                                               SceneCharacterProxy charProxy,
                                               SendTeleportRequestInMessage msg)
        {
            var proxy = (SceneProxy) charProxy;
            var type = msg.Request.Type;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------SendTeleportRequest----------{0}", type);
            if (proxy.Character == null)
            {
                msg.Reply((int)ErrorCodes.Error_NoObj, true);
                Logger.Warn("SendTeleportRequest Character is null {0}", proxy.CharacterId);
                yield break;
            }
            if (null == proxy.Character.Scene)
            {
                msg.Reply((int) ErrorCodes.Unknow);
                Logger.Warn("SendTeleportRequest null==Scene      Character{0}", proxy.Character.ObjId);
                yield break;
            }
            if (proxy.Character.IsChangingScene())
            {
                msg.Reply((int)ErrorCodes.StateError);
                Logger.Warn("SendTeleportRequest proxy.Character.IsChangingScene()      Character{0}",
                    proxy.Character.ObjId);
                yield break;
            }
            

            var tableData = Table.GetTransfer(type);
            if (null == tableData)
            {
                Logger.Info("DataTable.Table.GetTransfer({0})==null", type);
                msg.Reply((int) ErrorCodes.Error_NoTransfer);
                yield break;
            }

            var tableScene = Table.GetScene(tableData.ToSceneId);
            if (null == tableScene)
            {
                Logger.Info("DataTable.Table.GetScene(tableData.ToSceneId)==null", tableData.ToSceneId);
                msg.Reply((int) ErrorCodes.Error_NoTransfer);
                yield break;
            }

            if (proxy.Character.GetLevel() < tableScene.LevelLimit)
            {
                Logger.Info("DataTable.Table.GetTransfer({0})==null", type); //正常现象
                msg.Reply((int) ErrorCodes.Error_LevelNoEnough);
                yield break;
            }
            
            const float DistanceError = 5;

            var diff = proxy.Character.GetPosition() - new Vector2(tableData.FromX, tableData.FromY);
            if (diff.Length() > (tableData.TransferRadius + DistanceError))
            {
                msg.Reply((int) ErrorCodes.Error_DistanceTooMuch);
                Logger.Warn("SendTeleportRequest Character[{0}] Error_DistanceTooMuch now diff[{1}]", proxy.Character.ObjId,diff);
                yield break;
            }

            var param = new SceneParam();
            param.Param.Add(type);

            var co = CoroutineFactory.NewSubroutine(SceneServer.Instance.ServerControl.CreateAndEnterScene,
                coroutine,
                msg.CharacterId, proxy.Character.ServerId,
                tableData.ToSceneId,
                0ul,
                eScnenChangeType.Transfer,
                param);
            if (co.MoveNext())
            {
                yield return co;
            }
            msg.Reply();
        }

        //切换场景
        public IEnumerator ChangeSceneRequest(Coroutine coroutine,
                                              SceneCharacterProxy charProxy,
                                              ChangeSceneRequestInMessage msg)
        {
            var proxy = (SceneProxy) charProxy;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (character == null)
            {
                Logger.Error("AskEnterDungeon Enter characterId = {0} null", msg.CharacterId);
                yield break;
            }

            if (proxy.Character.IsChangingScene())
            {
                PlayerLog.WriteLog(proxy.CharacterId, "ChangeSceneRequest proxy.Character.IsChangingScene()");
                msg.Reply((int) ErrorCodes.StateError);
                yield break;
            }

            var sceneId = msg.Request.SceneId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------ChangeSceneRequest----------{0}", sceneId);
            if (sceneId == 0)
            {
                Logger.Warn("DataTable.Table.GetScene({0})==null", sceneId);
                msg.Reply((int) ErrorCodes.Error_NoScene);
                yield break;
            }
            var tbScene = Table.GetScene(sceneId);
            if (null == tbScene)
            {
                Logger.Warn("DataTable.Table.GetScene({0})==null", sceneId);
                msg.Reply((int) ErrorCodes.Error_NoScene);
                yield break;
            }

            // 检查限制条件
            // 场景是否开放
            if (tbScene.IsPublic != 1)
            {
                Logger.Warn("Character({0}) : Scene({1}) not public!", proxy.CharacterId, sceneId);
                msg.Reply((int) ErrorCodes.Error_SceneNotPublic);
                yield break;
            }

            // 角色等级是否到达
            if (tbScene.LevelLimit > proxy.Character.GetLevel())
            {
                Logger.Warn("Character({0}) : level not enough for transfer to Scene({1})!", proxy.CharacterId, sceneId);
                msg.Reply((int) ErrorCodes.Error_LevelNoEnough);
                yield break;
            }
            if (character.VipLevel < 4)
            {

                // 扣钱
                if (tbScene.ConsumeMoney > 0)
                {
                    var result = SceneServer.Instance.LogicAgent.DeleteItem(proxy.CharacterId, 2, tbScene.ConsumeMoney, (int)eDeleteItemType.ChangeScene);
                    yield return result.SendAndWaitUntilDone(coroutine);
                    if (result.State != MessageState.Reply)
                    {
                        Logger.Error("Cost money result.State={0}", result.State);
                        msg.Reply((int)ErrorCodes.Unknow);
                        yield break;
                    }

                    // 扣钱出现错误，则直接返回错误码
                    if (result.ErrorCode != (int)ErrorCodes.OK)
                    {
                        msg.Reply(result.Response);
                        yield break;
                    }
                }
            }


            var param = new SceneParam();
            var co1 = CoroutineFactory.NewSubroutine(SceneServer.Instance.ServerControl.CreateAndEnterScene,
                coroutine,
                msg.CharacterId, proxy.Character.ServerId,
                sceneId,
                0ul,
                eScnenChangeType.Normal,
                param);
            if (co1.MoveNext())
            {
                yield return co1;
            }
            msg.Reply();
        }

        //拾取物品
        public IEnumerator PickUpItem(Coroutine coroutine, SceneCharacterProxy charProxy, PickUpItemInMessage msg)
        {
            var proxy = (SceneProxy) charProxy;
            var dropitemid = msg.Request.DropItemId;
            //PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------PickUpItem----------:{0}", dropitemid);
            if (proxy.Character == null || null == proxy.Character.Scene)
            {
                msg.Reply((int) ErrorCodes.Unknow);
                Logger.Warn("PickUpItem Character is null or Scene is null");
                yield break;
            }

            var obj = proxy.Character.Scene.FindObj(dropitemid);
            if (null == obj)
            {
                msg.Reply((int) ErrorCodes.Error_NoObj);
                yield break;
            }

            if (obj.GetObjType() != ObjType.DROPITEM)
            {
                msg.Reply((int) ErrorCodes.Error_NoObj);
                yield break;
            }

            var item = obj as ObjDropItem;
            if (!item.Pickup(proxy.Character))
            {
                msg.Reply((int) ErrorCodes.Error_NotTheOwner);
                yield break;
            }

            var tbItem = Table.GetItemBase(item.ItemId);
            if (tbItem != null && tbItem.Type == 40000 && tbItem.Quality >= 3)//灵兽品质>=3 发公告
            {
                var CharacterName = Utils.AddCharacter(proxy.CharacterId,proxy.Character.GetName());
                var strs = new List<string>
                    {
                        CharacterName,
                        Utils.AddItemId(tbItem.Id)
                    };
                var exData = new List<int>(tbItem.Exdata);
                var content = Utils.WrapDictionaryId(274081, strs, exData);
                var chatAgent = SceneServer.Instance.ChatAgent;
                chatAgent.BroadcastWorldMessage((uint)proxy.Character.ServerId, (int)eChatChannel.SystemScroll, 0, string.Empty,
                    new ChatMessageContent { Content = content });               
            }

            if(proxy.Character.Scene !=null)
            {
                proxy.Character.Scene.OnPlayerPickUp(proxy.Character.ObjId, item.ItemId, item.Count);
            }

            msg.Reply();
        }

        //请求场景扩展数据
        public IEnumerator Inspire(Coroutine co, SceneCharacterProxy _this, InspireInMessage msg)
        {
            var proxy = (SceneProxy) _this;
            var player = proxy.Character;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------Inspire----------");

            if (player == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }

            if (player.IsDead())
            {
                msg.Reply((int)ErrorCodes.Error_Death);
                yield break;
            }

            var tbScene = Table.GetScene((int) player.CurrentSceneTypeId);
            if (tbScene == null)
            {
                Logger.Error("In Inspire(), tbScene == null");
                msg.Reply((int) ErrorCodes.Error_NoScene);
                yield break;
            }
            var tbFuben = Table.GetFuben(tbScene.FubenId);
            if (tbFuben == null || tbFuben.CanInspire == -1)
            {
                Logger.Error("In Inspire(), tbFuben == null || tbFuben.CanInspire != 1");
                msg.Reply((int) ErrorCodes.Unknow);
                yield break;
            }
            var needGold = 0;
            var type = (int) eResourcesType.GoldRes;
            var buffId = 30109;

            var tbInspire = Table.GetBangBuff(tbFuben.CanInspire);
            if (tbInspire == null)
            {
                Logger.Error("In Inspire(), tbInspire == null");
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            var ids = new Int32Array();
            ids.Items.Add((int)eExdataDefine.e666);
            var msg2 = SceneServer.Instance.LogicAgent.SSFetchExdata(player.ObjId, ids);
            yield return msg2.SendAndWaitUntilDone(co);

            if (msg2.State != MessageState.Reply)
            {
                Logger.Error("SSFetchExdata return with state = {0}", msg2.State);
                yield break;
            }
            if (msg2.ErrorCode != (int) ErrorCodes.OK)
            {
                Logger.Error("SSFetchExdata return with err = {0}", msg2.ErrorCode);
                yield break;
            }

            var curDangci = msg2.Response.Items[0];
            if (curDangci < 0)
            {
                Logger.Error("In Inspire(), curDangci < 0");
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            if (msg.Request.Placeholder == 0)
            {
                //needGold = Table.GetServerConfig(381).ToInt();
                if (curDangci < tbInspire.BuffGoldId.Length)
                {
                    buffId = tbInspire.BuffGoldId[curDangci];
                }
                else
                {
                    buffId = tbInspire.BuffGoldId[tbInspire.BuffGoldId.Length - 1];
                }

                if (curDangci < tbInspire.BuffGoldPrice.Length)
                {
                    needGold = tbInspire.BuffGoldPrice[curDangci];
                }
                else
                {
                    needGold = tbInspire.BuffGoldPrice[tbInspire.BuffGoldPrice.Length - 1];
                }

                type = (int) eResourcesType.GoldRes;
            }
            else
            {
                //needGold = Table.GetServerConfig(386).ToInt();

                if (curDangci < tbInspire.BuffDiamodId.Length)
                {
                    buffId = tbInspire.BuffDiamodId[curDangci];
                }
                else
                {
                    buffId = tbInspire.BuffDiamodId[tbInspire.BuffDiamodId.Length - 1];
                }

                if (curDangci < tbInspire.BuffDiamodPrice.Length)
                {
                    needGold = tbInspire.BuffDiamodPrice[curDangci];
                }
                else
                {
                    needGold = tbInspire.BuffDiamodPrice[tbInspire.BuffDiamodPrice.Length - 1];
                }

                type = (int) eResourcesType.DiamondRes;
            }

            if (buffId == -1)
            {
                Logger.Error("In Inspire(), buffId == -1");
                msg.Reply((int)ErrorCodes.Error_CanNot_Inspire);
                yield break;
            }

            var result = SceneServer.Instance.LogicAgent.DeleteItem(player.ObjId, type, needGold, (int)eDeleteItemType.Inspire);
            yield return result.SendAndWaitUntilDone(co);

            if (result.State != MessageState.Reply)
            {
                Logger.Error("In Inspire(), LogicAgent.DeleteItem replied with state = {0}", result.State);
                msg.Reply((int) ErrorCodes.Unknow);
                yield break;
            }
            if (result.ErrorCode != (int) ErrorCodes.OK)
            {
                Logger.Error("In Inspire(), LogicAgent.DeleteItem replied with ErrorCode = {0}", result.ErrorCode);
                msg.Reply(result.ErrorCode);
                yield break;
            }

            proxy.Character.AddBuff(buffId, 1, proxy.Character);
            //player.SetExdata((int)eExdataDefine.e666, curDangci + 1);

            var dict = new Dict_int_int_Data();

            //判断下一档是或否BuffId == -1
            var nextDangCi = curDangci + 1;
            var nextBuffId = -1;

            if (nextDangCi < tbInspire.BuffGoldId.Length)
            {
                nextBuffId = tbInspire.BuffGoldId[nextDangCi];
            }
            else
            {
                nextBuffId = tbInspire.BuffGoldId[tbInspire.BuffGoldId.Length - 1];
            }

            if (nextBuffId != -1)
            { 
                dict.Data.Add((int)eExdataDefine.e666, 1);
            }
            var msg1 = SceneServer.Instance.LogicAgent.SSChangeExdata(player.ObjId, dict);
            yield return msg1.SendAndWaitUntilDone(co);

            msg.Reply();
        }

        public IEnumerator SummonMonster(Coroutine co, SceneCharacterProxy _this, SummonMonsterInMessage msg)
        {
            var proxy = (SceneProxy) _this;
            proxy.Character.ClearRetinue();
            
            proxy.Character.CreateRetinue(msg.Request.MonsterId, 1, proxy.Character.GetPosition(), proxy.Character.GetDirection(),
                    proxy.Character.GetCamp());
            msg.Response = (int)ErrorCodes.OK;
            msg.Reply();
            yield break;
        }
    }

    public class SceneProxy : SceneCharacterProxy
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //使用技能
        public const float SkillErrorDistance = 3;//0.25f; //0.5f * 0.5f;
        //public List<CheckConnectedInMessage> WaitingCheckConnectedInMessages = new List<CheckConnectedInMessage>();
        //public List<CheckLostInMessage> WaitingCheckLostInMessages = new List<CheckLostInMessage>();
        public SceneProxy(SceneService service, ulong characterId, ulong clientId)
            : base(service, characterId, clientId)
        {
            Connected = false;
        }

        public ObjPlayer Character { get; set; }
        public bool Connected { get; set; }

        private ObjCharacter GetCharacter(ulong characterId, eSceneSyncId syncType)
        {
            if (Character.Scene == null)
            {
                return null;
            }
            return Character.Scene.FindCharacter(characterId);
        }
    }
}