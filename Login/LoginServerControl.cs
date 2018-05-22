#region using

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Database;
using DataContract;
using DataTable;
using LoginServerService;
using Scorpion;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using SceneClientService;

using Shared;

#endregion

namespace Login
{
    public class ThirdLoginResult
    {
        public string userid;
        public string userName;
        public string spid;
        public MsgChatMoniterData moniterData;
       
    }

    public partial class LoginServerControlDefaultImpl : ILoginService, ITickable, IStaticLoginServerControl
    {
        [Updateable("Login")] 
        public static int Diff = 0;//(int) Math.Round((DateTime.UtcNow - DateTime.Now).TotalMinutes);
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Logger ThirdLoginLogger = LogManager.GetLogger("LoginByThird");
        //private bool isZhengdianTickLog = false;
        //private bool isEveryMinTickLog = false;

        public IEnumerator NotiffyGMAccount(Coroutine coroutine, LoginService _this, NotiffyGMAccountInMessage msg)
        {
            LoginAllAccounts acc = msg.Request.Acc;
            
//             foreach (var ac in acc.acc)
//             {
//                 if (!LoginServer.Instance.ServerControl.GMAccounts.Contains(ac))
//                 {
//                     LoginServer.Instance.ServerControl.GMAccounts.Add(ac);
//                 }
//             }
            msg.Reply();
            return null;
        }
        public static void AddKuaiFaKeyValuePair(ref StringBuilder sb,
                                                 ref Dictionary<string, string> dictionary,
                                                 string key,
                                                 string value)
        {
            dictionary.Add(key, value);
            sb.Append(string.Format("&{0}={1}", key, HttpUtility.UrlEncode(value, Encoding.UTF8)));
        }

        private IEnumerator CheckLost(Coroutine coroutine, ulong characterId, AsyncReturnValue<bool> result)
        {
            PlayerLog.WriteLog(characterId, "----------Login----------CheckLost----------{0}", characterId);

            Logger.Info("Enter Game {0} CheckLost - 1 - {1}", characterId, TimeManager.Timer.ElapsedMilliseconds);
            var c1 = LoginServer.Instance.SceneAgent.CheckLost(characterId, characterId);
            yield return c1.SendAndWaitUntilDone(coroutine);
            if (c1.State != MessageState.Reply)
            {
                result.Value = false;
            }

            Logger.Info("Enter Game {0} CheckLost - 2 - {1}", characterId, TimeManager.Timer.ElapsedMilliseconds);
            var c2 = LoginServer.Instance.ChatAgent.CheckLost(characterId, characterId);
            yield return c2.SendAndWaitUntilDone(coroutine);
            if (c2.State != MessageState.Reply)
            {
                result.Value = false;
            }

            Logger.Info("Enter Game {0} CheckLost - 3 - {1}", characterId, TimeManager.Timer.ElapsedMilliseconds);
            var c3 = LoginServer.Instance.ActivityAgent.CheckLost(characterId, characterId);
            yield return c3.SendAndWaitUntilDone(coroutine);
            if (c3.State != MessageState.Reply)
            {
                result.Value = false;
            }

            Logger.Info("Enter Game {0} CheckLost - 4 - {1}", characterId, TimeManager.Timer.ElapsedMilliseconds);
            var c4 = LoginServer.Instance.RankAgent.CheckLost(characterId, characterId);
            yield return c4.SendAndWaitUntilDone(coroutine);
            if (c4.State != MessageState.Reply)
            {
                result.Value = false;
            }

            Logger.Info("Enter Game {0} CheckLost - 5 - {1}", characterId, TimeManager.Timer.ElapsedMilliseconds);
            var c5 = LoginServer.Instance.TeamAgent.CheckLost(characterId, characterId);
            yield return c5.SendAndWaitUntilDone(coroutine);
            if (c5.State != MessageState.Reply)
            {
                result.Value = false;
            }

            Logger.Info("Enter Game {0} CheckLost - 6 - {1}", characterId, TimeManager.Timer.ElapsedMilliseconds);
            var c6 = LoginServer.Instance.LogicAgent.CheckLost(characterId, characterId);
            yield return c6.SendAndWaitUntilDone(coroutine);
            if (c6.State != MessageState.Reply)
            {
                result.Value = false;
            }

            Logger.Info("Enter Game {0} CheckLost - 7 - {1}", characterId, TimeManager.Timer.ElapsedMilliseconds);
            result.Value = true;
        }

        private void GetServerListData(LoginService _this, ServerListData serverList, PlayerController player, bool isGM = false)
        {
            if (player != null)
            {
                foreach (var server in player.DbData.ServersPlayers)
                {
                    serverList.PlayerData[server.Key] = server.Value;
                }
            }
            
            if (isGM)
            {
                Table.ForeachServerName(record =>
                {
                    var server = new ServerState();
                    server.ServerId = record.Id;
                    server.Name = string.Format("{0}-{1}-{2}", record.Id, record.OpenTime, player.LoginChannel);
                    server.IsNew = 0;
                    server.State = (int) ServerStateType.Fine;
                    serverList.Data.Add(server);
                    return true;
                });
            }
            else
            {
                ChannelServerInfo servers;
                if (StaticParam.ServerListWithPid.TryGetValue(player.LoginChannel, out servers))
                {
                    serverList.Data.AddRange(servers.AllServerStates);
                }

                ChannelServerInfo serversAll;
                if (StaticParam.ServerListWithPid.TryGetValue("all", out serversAll))
                {
                    foreach (var data in serversAll.AllServerStates)
                    {
                        if (!serverList.Data.Contains(data))
                        {
                            serverList.Data.Add(data);
                        } 
                    }
                }
                serverList.Config.AddRange(LoginServer.Instance.s_SpecialAccount.ClientConfig);
      
                serverList.WaitSec = 60;
            }
          
        }

        private void RefreshServerListForAllChannels()
        {
            //var notifyMsg = LoginServer.Instance.ActivityAgent.SSApplyActiResultList(0, 0);
            //yield return notifyMsg.SendAndWaitUntilDone(coroutine);
            //if (notifyMsg.State != MessageState.Reply || notifyMsg.ErrorCode != (int)ErrorCodes.OK)
            //{
            //    Logger.Error("SSApplyActiResultList failed in GetServerList()!!!");
            //    yield break;
            //}

            try
            {
                //var resultList = notifyMsg.Response;
                var playerCount = QueueManager.PlayerCount;
                var now = DateTime.Now;
                foreach (var kv in StaticParam.ServerListWithPid)
                {
                    var serverRecords = new List<ServerNameRecord>();
                    var maxWeightNewServer = -1;
                    var maxWeightPrepareServer = -1;
                    var servers = kv.Value;
                    servers.AllServerStates.Clear();
                    servers.NewServers.Clear();
                    servers.PrepareServers.Clear();
                    foreach (var record in servers.AllSevers)
                    {
                        if (record.Id == 0)
                        {
                            Logger.Error("serverid must not be zero!!!check serverName Table!!!");
                            continue;
                        }

                        if (record.LogicID == -1)
                        {
                            continue;
                        }
                        if (record.IsClientDisplay == 0)
                        {
                            continue;
                        }

                        var startTime = DateTime.Parse(record.OpenTime);

                        var tempServer = new ServerState();
                        tempServer.ServerId = record.Id;
                        tempServer.Name = record.Name;
                        tempServer.IsNew = 0;

                        int pc;
                        if (startTime > now)
                        {
                            tempServer.State = (int) ServerStateType.Prepare;
                        }
                        else if (playerCount.TryGetValue(record.Id, out pc))
                        {
                            if (pc >= record.FullCount)
                            {
                                tempServer.State = (int) ServerStateType.Full;
                            }
                            else if (pc >= record.CrowdCount)
                            {
                                tempServer.State = (int) ServerStateType.Crowded;
                            }
                            else if (pc >= 1)
                            {
                                tempServer.State = (int) ServerStateType.Busy;
                            }
                            else
                            {
                                tempServer.State = (int) ServerStateType.Fine;
                            }
                        }
                        else
                        {
                            tempServer.State = (int) ServerStateType.Fine;
                        }

                        //var unit = resultList.Datas.Find(d => d.serverId == record.Id);
                        //if (null != unit)
                        {
                            tempServer.actiResult = -99;
                        }

                        servers.AllServerStates.Add(tempServer);
                        serverRecords.Add(record);

                        if (record.Weights > maxWeightNewServer
                            && tempServer.State != (int) ServerStateType.Repair
                            && tempServer.State != (int) ServerStateType.Prepare)
                        {
                            maxWeightNewServer = record.Weights; 
                        }
                            
                        if (record.Weights > maxWeightPrepareServer
                            && tempServer.State != (int)ServerStateType.Repair)
                        {
                            maxWeightPrepareServer = record.Weights;
                        }
                    }

                    try
                    {
                        servers.NewServers.Clear();

                        var index = 0;
                        foreach (var v in servers.AllServerStates)
                        {
                            if (maxWeightNewServer > 0 && v.State != (int) ServerStateType.Repair &&
                                v.State != (int) ServerStateType.Prepare)
                            {
                                if (serverRecords[index].Weights >= maxWeightNewServer)
                                {
                                    v.IsNew = 1;
                                    servers.NewServers.Add(serverRecords[index]);
                                }
                            }

                            if (maxWeightPrepareServer > 0 && v.State == (int) ServerStateType.Prepare)
                            {
                                servers.PrepareServers.Add(serverRecords[index]);
                            }

                            ++index;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error("Refresh Server List 1" + e.Message);
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Error("Refresh Server List 2" + ex.Message);
            }
        }

        private IEnumerator NotifyBrokerPrepareData(Coroutine coroutine,
                                                    ulong characterId,
                                                    ulong clientId,
                                                    int serverId,
                                                    int sceneId,
                                                    ulong sceneGuid,
                                                    AsyncReturnValue<int> status,
                                                    AsyncReturnValue<ulong> scenedata)
        {
            PlayerLog.WriteLog(characterId, "----------Login----------NotifyBrokerPrepareData----------{0},{1},{2},{3}",
                clientId, sceneId, sceneGuid, serverId);
            status.Value = 0;
            var msg = new OutMessage[6];
            serverId = SceneExtension.GetServerLogicId(serverId);
            msg[0] = LoginServer.Instance.SceneAgent.NotifyBroker(coroutine, characterId, clientId, serverId, sceneId,
                sceneGuid, (int) eScnenChangePostion.Login);
            msg[0].SendAndWaitUntilDone(coroutine);
            msg[1] = LoginServer.Instance.ChatAgent.NotifyBroker(coroutine, characterId, clientId, serverId, sceneId,
                sceneGuid);
            msg[1].SendAndWaitUntilDone(coroutine);
            msg[2] = LoginServer.Instance.ActivityAgent.NotifyBroker(coroutine, characterId, clientId, serverId, sceneId,
                sceneGuid);
            msg[2].SendAndWaitUntilDone(coroutine);
            msg[3] = LoginServer.Instance.LogicAgent.NotifyBroker(coroutine, characterId, clientId, serverId, sceneId,
                sceneGuid);
            msg[3].SendAndWaitUntilDone(coroutine);
            msg[4] = LoginServer.Instance.RankAgent.NotifyBroker(coroutine, characterId, clientId, serverId, sceneId,
                sceneGuid);
            msg[4].SendAndWaitUntilDone(coroutine);
            msg[5] = LoginServer.Instance.TeamAgent.NotifyBroker(coroutine, characterId, clientId, serverId, sceneId,
                sceneGuid);
            msg[5].SendAndWaitUntilDone(coroutine);

            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;


            for (var i = 0; i < msg.Length; i++)
            {
                var outMessage = msg[i];
                if (outMessage.State != MessageState.Reply)
                {
                    Logger.Error("NotifyBrokerPrepareData ..{1}..NotifyBroker....MessageState.....: {0} ",
                        outMessage.State, i);
                    status.Value = 1;
                    yield break;
                }
            }

            Logger.Info("Enter Game {0} - EnterGame - NotifyBrokerPrepareData - 1 - {1}", characterId,
                TimeManager.Timer.ElapsedMilliseconds);

            PlayerLog.WriteLog(characterId, "----------Login----------PrepareDataForEnterGame----------{0},{1},{2},{3}",
                clientId, sceneId, sceneGuid, serverId);
            msg[0] = LoginServer.Instance.SceneAgent.PrepareDataForEnterGame(characterId, serverId);
            msg[0].SendAndWaitUntilDone(coroutine);
            msg[1] = LoginServer.Instance.ChatAgent.PrepareDataForEnterGame(characterId, serverId);
            msg[1].SendAndWaitUntilDone(coroutine);
            msg[2] = LoginServer.Instance.ActivityAgent.PrepareDataForEnterGame(characterId, serverId);
            msg[2].SendAndWaitUntilDone(coroutine);
            msg[3] = LoginServer.Instance.LogicAgent.PrepareDataForEnterGame(characterId, serverId, sceneGuid);
            msg[3].SendAndWaitUntilDone(coroutine);
            msg[4] = LoginServer.Instance.RankAgent.PrepareDataForEnterGame(characterId, serverId);
            msg[4].SendAndWaitUntilDone(coroutine);
            msg[5] = LoginServer.Instance.TeamAgent.PrepareDataForEnterGame(characterId, serverId);
            msg[5].SendAndWaitUntilDone(coroutine);
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            if (msg[0].State == MessageState.Reply)
            {
                var t = (PrepareDataForEnterGameOutMessage) msg[0];
                //t.Response
                var newSceneGuid = t.Response;
                scenedata.Value = newSceneGuid;
                //Logger.Fatal("NotifyBrokerPrepareData  sceneGuid = {0}", newSceneGuid);
            }

            foreach (var outMessage in msg)
            {
                if (outMessage.State != MessageState.Reply)
                {
                    status.Value = 1;
                    Logger.Error("NotifyBrokerPrepareData ..{1}....PrepareDataForEnterGame....MessageState.....: {0} ",
                        outMessage.State, (ServiceType) outMessage.mMessage.ServiceType);
                    yield break;
                }
            }

            Logger.Info("Enter Game {0} - EnterGame - NotifyBrokerPrepareData - 2 - {1}", characterId,
                TimeManager.Timer.ElapsedMilliseconds);
        }


        public IEnumerator OnClientConnected(Coroutine coroutine,
                                             LoginService _this,
                                             string target,
                                             ulong clientid,
                                             ulong characterId,
                                             uint packId)
        {
            var __this = (LoginServerControl) _this;
//             if (!__this.CurrentConnectedClients.Add(clientid))
//                 yield break;

            Logger.Debug("{0} {1}", target, clientid);
            yield break;
        }

        /// <summary>
        ///     DO NOT use characterId in this function, it's always zero.
        /// </summary>
        /// <param name="coroutine"></param>
        /// <param name="target"></param>
        /// <param name="clientid"></param>
        /// <param name="characterId"></param>
        /// <returns></returns>
        public IEnumerator OnClientLost(Coroutine coroutine,
                                        LoginService _this,
                                        string target,
                                        ulong clientid,
                                        ulong characterId,
                                        uint packId)
        {
            yield break;
            //LoginServerControl __this = (LoginServerControl)_this;
            //if (target != "client")
            //{
            //    yield break;
            //}

            //PlayerLog.WriteLog((int)LogType.OnClientLost, "clientid lost ={0}", clientid);

            //var co = CoroutineFactory.NewSubroutine(PlayerLogout, coroutine, __this, clientid);
            //if (co.MoveNext())
            //    yield return co;
        }

        public IEnumerator UpdateServer(Coroutine coroutine, LoginService _this, UpdateServerInMessage msg)
        {
            LoginServer.Instance.UpdateManager.Update();
            return null;
        }

        public IEnumerator ChangeServer(Coroutine coroutine, LoginService _this, ChangeServerInMessage msg)
        {
            var __this = (LoginServerControl) _this;
            var characterId = msg.Request.CharacterId;
            var newServerId = msg.Request.ServerId;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            var isChracterOnline = character != null;
            if (!isChracterOnline)
            {
                var characterRet = AsyncReturnValue<CharacterController>.Create();
                var co1 = CoroutineFactory.NewSubroutine(CharacterManager.Instance.GetOrCreateCharacterController,
                    coroutine,
                    characterId, new object[] {}, false, characterRet);
                if (co1.MoveNext())
                {
                    yield return co1;
                }
                character = characterRet.Value;
                characterRet.Dispose();
            }
            if (character == null)
            {
                msg.Reply((int) ErrorCodes.Error_CharacterNotFind);
                yield break;
            }
            //角色已拿到
            var clientId = character.ClientId;
            var characterDb = character.GetData();
            var playerId = characterDb.PlayerId;
            var oldServerId = characterDb.ServerId;
            if (oldServerId == newServerId)
            {
                msg.Reply((int) ErrorCodes.Error_AlreadyOnThisSever);
                yield break;
            }
            //已满足修改条件
            PlayerController player = null;
            if (isChracterOnline)
            {
                player = __this.PlayerManager.GetPlayerController(clientId);
                if (player == null)
                {
                    Logger.Error("ChangeServer player unLine! playId={0},characterId={1},clientId={2}", playerId,
                        characterId, clientId);
                    CleanClientData(__this, clientId, characterId, KickClientType.ChangeServer);
                    character.LostLine();
                    CharacterManager.PopServerPlayer(oldServerId); //转服如果玩家在线，需要踢出玩家，服务器人数减
                    var co1 = CoroutineFactory.NewSubroutine(RemoveCharacterData, coroutine, characterId);
                    if (co1.MoveNext())
                    {
                        yield return co1;
                    }
                    msg.Reply((int) ErrorCodes.Error_PlayerId_Not_Exist);
                    yield break;
                }
            }
            else
            {
                player = __this.PlayerManager.GetPlayerControllerByPlayerId(playerId);
                if (player == null)
                {
                    var result = AsyncReturnValue<PlayerController>.Create();
                    var co1 = CoroutineFactory.NewSubroutine(__this.PlayerManager.LoadPlayer, coroutine, playerId,
                        result);
                    if (co1.MoveNext())
                    {
                        yield return co1;
                    }
                    player = result.Value;
                    result.Dispose();

                    if (player == null)
                    {
                        Logger.Error("ChangeServer player not read db! playId={0},characterId={1},clientId={2}",
                            playerId, characterId, clientId);
                        msg.Reply((int) ErrorCodes.Error_PlayerId_Not_Exist);
                        yield break;
                    }
                }
            }
            //Player已拿到
            var playerDb = player.DbData;
            var serversPlayers = playerDb.ServersPlayers;

            Uint64Array oldServerData;
            if (!serversPlayers.TryGetValue(oldServerId, out oldServerData))
            {
                //角色数据不在OldServer
                Logger.Error(
                    "ChangeServer character ServerId Error! playId={0},characterId={1},clientId={2},oldServerId={3}",
                    playerId, characterId, clientId, oldServerId);
                msg.Reply((int) ErrorCodes.Error_PlayerId_Not_Exist);
                yield break;
            }


            Uint64Array newServerData;
            if (serversPlayers.TryGetValue(newServerId, out newServerData))
            {
                if (newServerData.Items.Count >= 4)
                {
                    msg.Reply((int) ErrorCodes.CharacterFull);
                    yield break;
                }
            }
            else
            {
                newServerData = new Uint64Array();
                serversPlayers[newServerId] = newServerData;
            }

            //修改数据
            newServerData.Items.Add(characterId);
            newServerData.Items.Sort();
            oldServerData.Items.Remove(characterId);
            if (oldServerData.Items.Count == 0)
            {
                serversPlayers.Remove(oldServerId);
            }
            characterDb.ServerId = newServerId;
            character.MarkDbDirty();

            //保存Player数据

            var result1 = AsyncReturnValue<int>.Create();
            var co = CoroutineFactory.NewSubroutine(player.SaveDb, coroutine, result1);
            if (co.MoveNext())
            {
                yield return co;
            }
            result1.Dispose();

            if (isChracterOnline)
            {
//原本在线，踢下线
                CleanClientData(__this, clientId, characterId, KickClientType.ChangeServerOK);
                var co4 = CoroutineFactory.NewSubroutine(PlayerLogout, coroutine, __this, clientId);
                if (co4.MoveNext())
                {
                    yield return co4;
                }
            }
            else
            {
                //保存Character数据
                var co3 = CoroutineFactory.NewSubroutine(CharacterManager.Instance.RemoveCharacter, coroutine,
                    characterId);
                if (co3.MoveNext())
                {
                    yield return co3;
                }
            }
            msg.Reply();
        }
        //<summary>
        //修改玩家名字
        //</summary>
        //<param name="coroutine"></param>
        //<param name="_this"></param>
        //<param name="msg"></param>
        //<returns></returns>
        public IEnumerator TryModifyPlayerName(Coroutine coroutine, LoginService _this, TryModifyPlayerNameInMessage msg)
        {
            var __this = (LoginServerControl)_this;
            var clientId = msg.ClientId;
            var name = msg.Request.ModifyName;
            var playerController = __this.PlayerManager.GetPlayerController(clientId);
            if (playerController == null)
            {
                msg.Reply((int)ErrorCodes.Error_Login_NotLogin);
                yield break;
            }
            if (PlayerManager.IsRobot(name))
            {
                msg.Reply((int)ErrorCodes.Error_NAME_IN_USE);
                yield break;
            }
            if (!SensitiveWord.CheckString(name))
            {
                Logger.Error("CreateCharacter client need check name={0}", name);
                msg.Reply((int)ErrorCodes.Error_NAME_Sensitive);
                yield break;
            }

            var cc = CharacterManager.Instance.GetCharacterControllerFromMemroy(playerController.DbData.SelectChar);
            if (cc == null)
            {
                msg.Reply((int)ErrorCodes.Error_Login_NotLogin);
                yield break;
            }
            
            //如果没人用这个名字
            var retDbId = LoginServer.Instance.DB.Set(coroutine, DataCategory.LoginCharacterName, name, playerController.DbData.SelectChar.ToDBUlong(), SetOption.SetIfNotExist);

            yield return retDbId;

            if (retDbId.Status == DataStatus.DatabaseError)
            {
                msg.Reply((int)ErrorCodes.DataBase);
                PlayerLog.WriteLog((int)LogType.DataBase, "CreateCharacter DataBase Error! name={0}", name);
                yield break;
            }
            if (retDbId.Status != DataStatus.Ok)
            {
                msg.Reply((int)ErrorCodes.Error_NAME_IN_USE);
                PlayerLog.WriteLog((int)LogType.Error_NAME_IN_USE, "CreateCharacter Error_NAME_IN_USE name={0}", name);
                yield break;
            }
            
            
            
            CharacterManager<CharacterController, DBCharacterLogin, DBCharacterLoginSimple>.DataItem data;
            if (CharacterManager.Instance.mDictionary.TryGetValue(cc.mDbData.Id, out data))
            {
                data.SimpleData.Name = name;
                cc.mDbData.Name = name;
                cc.MarkDirty();
                msg.Response = name;
                msg.Reply((int)ErrorCodes.OK);
            }
            else
            {
                msg.Reply((int)ErrorCodes.Unknow);
            }
        } 
        public IEnumerator OnServerStart(Coroutine coroutine, LoginService _this)
        {
            var __this = (LoginServerControl) _this;
            //Thread.Sleep(GlobalVariable.WaitToConnectTimespan);
            LoginServer.Instance.Start(__this);
            __this.WebRequestManager = new RequestManager(__this);
            CharacterManager.Instance.Init(LoginServer.Instance.DB, DataCategory.LoginCharacter);
            PlayerManager.Init();
            LoginServer.Instance.IsReadyToEnter = true;
            _this.TickDuration = 0.1f;
            var targetTime = DateTime.Now.AddDays(1).Date;
            __this.DayChangeTrigger = LoginServerControl.Timer.CreateTrigger(targetTime,
                () => { OnDayChangeEvent(_this); }, 24*3600000);
            Console.WriteLine("LoginServer startOver. [{0}]", LoginServer.Instance.Id);

            _this.Started = true;

//             try
//             {
//                 ServerListData serverList = new ServerListData();
//                 GetServerListData(__this, serverList, null);
//                 foreach (var data in serverList.Data)
//                 {
//                     string v = string.Format("gameserver#{0}|{1}|{2}",
//                         data.ServerId, //serverid
//                         data.Name, //服务器名字
//                         DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")); // 时间
//                     kafaLogger.Info(v);
//                 }
//             }
//             catch (Exception)
//             {
//             }


            yield break;
        }

        public void OnDayChangeEvent(LoginService _this)
        {
            var __this = (LoginServerControl) _this;
            if (__this.PlayerManager != null)
            {
                foreach (var controller in __this.PlayerManager.PlayersByPlayerId)
                {
                    //PlayerLog.StatisticsLogger("eg,{0},{1}", controller.Value.DbData.SelectChar, controller.Key);
                    PlayerLog.BackDataLogger((int) BackDataType.PlayerOnline, "{0}", controller.Key);
                }
            }

            //LoginServerControl.Timer.ChangeTime(ref __this.DayChangeTrigger, DateTime.Now.AddDays(1).Date);
        }

        public IEnumerator Tick(Coroutine co, ServerAgentBase server)
        {
            var _this = (LoginServerControl) server;

            try
            {
                LoginServerControl.Timer.Update();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Tick error.");
            }

            try
            {
                QueueManager.Update();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Tick error.");
            }

            var now = DateTime.Now;
            if (now > _this.NextGetServerCharacterCountTime)
            {
                _this.NextGetServerCharacterCountTime = now.AddMinutes(10);

                var msg = LoginServer.Instance.SceneAgent.SBGetServerCharacterCount(0, 0);
                yield return msg.SendAndWaitUntilDone(co);

                if (msg.State == MessageState.Reply)
                {
                    _this.ServerCharacterCount = msg.Response.Data;
                }
            }

            //TickCountLog(now);

            try
            {
                LoginServerMonitor.TickRate.Mark();
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
            }

            try
            {
                if (now > _this.NextRefreshServerListTime)
                {
                    _this.NextRefreshServerListTime = now.AddMinutes(1);
                    RefreshServerListForAllChannels();
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
            }
        }

//         public void TickCountLog(DateTime now)
//         {
//             try
//             {
//                 // 每小时记录在线人数
//                 if (now.Minute == 0 && isZhengdianTickLog == false)
//                 {
//                     isZhengdianTickLog = true;
//                     foreach (var i in CharacterManager.ServerCount)
//                     {
//                         if (now.Hour == 0)
//                         {
//                             // 0点插入
//                             DateTime dt = DateTime.Now;
//                             dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, 0);
// 
//                             string v = string.Format("gamecount#{0}|{1}|{2}",
//                                 i.Key, //serverid
//                                 i.Value.nowCount, //在线数量
//                                 dt.ToString("yyyy/MM/dd HH:mm:ss")); // 时间
//                             kafaLogger.Info(v);
//                         }
//                         else
//                         {
//                             // 非0点update
//                             DateTime dt = DateTime.Now;
//                             dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour - 1, 0, 0, 0);
//                             DateTime dt2 = DateTime.Now;
//                             dt2 = new DateTime(dt2.Year, dt2.Month, dt2.Day, dt2.Hour, 0, 0, 0);
// 
//                             var str = "gamecount_h_up" + now.Hour.ToString();
//                             string v = string.Format("{0}#{1}|{2}|{3}|{4}",
//                                 str,
//                                 i.Key, //serverid
//                                 i.Value.nowCount, //在线数量
//                                 dt.ToString("yyyy/MM/dd HH:mm:ss"), // 上次时间
//                                 dt2 // 这次的时间
//                                 );
//                             kafaLogger.Info(v);
//                         }
//                     }
//                 }
//                 else if (now.Minute != 0)
//                 {
//                     isZhengdianTickLog = false;
//                 }

                // 每分钟记录在线人数
//                 if (now.Second == 0 && isEveryMinTickLog == false)
//                 {
//                     isEveryMinTickLog = true;
//                     //Dictionary<int, int> dic = new Dictionary<int, int>();
//                     string str = string.Empty;
//                     int counter = 0;
//                     foreach (var i in CharacterManager.ServerCount)
//                     {
//                         ++counter;
//                         if (counter == CharacterManager.ServerCount.Count)
//                         {
//                             str += i.Value.serverId + ":" + i.Value.nowCount;
//                         }
//                         else
//                         {
//                             str += i.Value.serverId + ":" + i.Value.nowCount + "&";
//                         }
//                        
//                     }
//                     if (CharacterManager.ServerCount.Count > 0)
//                     {       
//                         DateTime dt = DateTime.Now;
//                         dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, 0);
// 
//                         string v = string.Format("timecharactercount#{0}|{1}",
//                             dt.ToString("yyyy/MM/dd HH:mm:ss"), // 时间
//                             str); //在线数量
//                         kafaLogger.Info(v);
//                     }
//                 }
//                 else if (now.Second != 0)
//                 {
//                     isEveryMinTickLog = false;
//                 }
//             }
//             catch (Exception)
//             {
//             }
//        }

        public IEnumerator OnServerStop(Coroutine coroutine, LoginService _this)
        {
//            CharacterManager.Instance.ForeachCharacter(item =>
//             {
//                 var c = (CharacterController) item;
//                 c.LostLine();
//                 return true;
//             });

            var dict = CharacterManager.Instance.mDictionary;
            if (dict != null)
            {
                foreach (var data in dict)
                {
                    try
                    {
                        if (data.Value == null)
                        {
                            continue;
                        }

                        if (data.Value.Controller == null)
                        {
                            //Logger.Error("ForeachCharacter is null characterId={0}", dataItem.Key);
                            continue;
                        }

                        if (!data.Value.Controller.Online)
                        {
                            continue;
                        }

                        data.Value.Controller.LostLine();
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, "Login OnServerStop got a exception.");
                    }
                }
            }

            var __this = (LoginServerControl) _this;
            __this.WebRequestManager.Stop();

            var co = CoroutineFactory.NewSubroutine(CharacterManager.Instance.SaveAllCharacter, coroutine,
                default(TimeSpan));
            if (co.MoveNext())
            {
                yield return co;
            }

            LoginServer.Instance.DB.Dispose();
        }

        public IEnumerator PlayerLoginByUserNamePassword(Coroutine coroutine,
                                                         LoginService _this,
                                                         PlayerLoginByUserNamePasswordInMessage msg)
        {
			
			try
			{
				bool isGM = false;
				string pwd = "";
				if (LoginServer.Instance.s_SpecialAccount.GMAccounts.TryGetValue(msg.Request.Username, out pwd))
				{
					if (0 != msg.Request.Password.CompareTo(pwd))
					{
						Logger.Info("!!PlayerLoginByUserNamePassword [{0}] pwd error", msg.Request.Username);
						msg.Reply((int) ErrorCodes.PasswordIncorrect);
						yield break;
					}

					Logger.Warn("PlayerLoginByUserNamePassword [{0}] Login successed!", msg.Request.Username);
					isGM = true;
				}

				if (!isGM)
				{
					if (!LoginServer.Instance.s_SpecialAccount.CanLoginByUserName)
					{
						if (!LoginServer.Instance.s_SpecialAccount.WhiteAccounts.Contains(msg.Request.Username))
						{
							msg.Reply((int)ErrorCodes.ClientIdNoPower);
							yield break;
						}
					}
				}
			}
	        catch (Exception e)
	        {
		        Logger.Fatal(e.Message);
	        }

			var co = CoroutineFactory.NewSubroutine(LoginEx,
				coroutine,
				_this,
                new List<string>() { msg.Request.Username, msg.Request.Password, "", "-1", "uborm" }, msg, new MsgChatMoniterData() { ip = "127.0.0.1", channel = "test", uid = "123" });

			if (co.MoveNext())
            {
                yield return co;
            }
        }

        //第三方登录
        public IEnumerator PlayerLoginByThirdKey(Coroutine coroutine,
            LoginService _this,
            PlayerLoginByThirdKeyInMessage msg)
        {
            var __this = (LoginServerControl) _this;
            //ulong clientId = msg.ClientId;
            //string platfrom = msg.Request.Platform;
            var channel = msg.Request.Channel;
            var userName = msg.Request.UserId;
            var accessToken = msg.Request.AccessToken;
            var uid = string.Empty;

            var spid = channel;
            MsgChatMoniterData moniterData;
            //服务器校验
            if (!channel.Equals("BaiDu")) //百度暂时不需要验证token
            {
                using (var reslut = AsyncReturnValue<ThirdLoginResult>.Create())
                {
                    var coCheck = CoroutineFactory.NewSubroutine(CheckAccessToken, coroutine, __this, channel,
                        accessToken, userName, reslut);
                    if (coCheck.MoveNext())
                    {
                        yield return coCheck;
                    }

                    if (null == reslut.Value)
                    {
                        Logger.Error("loginThird checkAccessToken error, channel:{0},token:{1}", channel,
                            accessToken);
                        msg.Reply((int) ErrorCodes.Error_loginThird_CheckTokenError);
                        yield break;
                    }
                    uid = reslut.Value.userid;
                    userName = reslut.Value.userName;
                    spid = reslut.Value.spid;
                    moniterData = reslut.Value.moniterData;

                }
                var co = CoroutineFactory.NewSubroutine(LoginEx, coroutine, _this,
                    new List<string>() { userName, accessToken, uid, spid, msg.Request.Platform }, (InMessage)msg, moniterData);
                //校验结束
                //var co = CoroutineFactory.NewSubroutine(Login, coroutine, _this, userName, accessToken, (InMessage)msg, (Action)(() =>
                //{
                //    if (!string.IsNullOrEmpty(uid))
                //    {
                //        msg.Response.Uid = uid;
                //    }
                //}));

                if (co.MoveNext())
                {
                    yield return co;
                }
            }
        }

        //选择服务器
            public IEnumerator PlayerSelectServerId(Coroutine coroutine,
                                                LoginService _this,
                                                PlayerSelectServerIdInMessage msg)
        {
            var __this = (LoginServerControl) _this;
            var clientId = msg.ClientId;
            var serverId = msg.Request.ServerId;
            //检查server id的有效性
            if (serverId < 0)
            {
                msg.Reply((int) ErrorCodes.ServerID);
                yield break;
            }
            var playerController = __this.PlayerManager.GetPlayerController(clientId);
            if (playerController == null)
            {
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }
            playerController.DbData.LastServerId = serverId;
            PlayerLog.PlayerLogger(playerController.DbData.Id,
                "----------Login----------PlayerSelectServerId----------{0}", clientId);
            Uint64Array characters;
            if (playerController.DbData.ServersPlayers.TryGetValue(serverId, out characters))
            {
                var cs = characters.Items.ToList();
                foreach (var characterId in cs)
                {
                    var dbLogicSimple = LoginServer.Instance.LogicAgent.GetLogicSimpleData(characterId, 0);
                    yield return dbLogicSimple.SendAndWaitUntilDone(coroutine);
                    if (dbLogicSimple.State != MessageState.Reply)
                    {
                        continue;
                    }
                    if (dbLogicSimple.ErrorCode != 0)
                    {
                        continue;
                    }
                    var dbLoginSimple = LoginServer.Instance.LoginAgent.GetLoginSimpleData(clientId, characterId);
                    yield return dbLoginSimple.SendAndWaitUntilDone(coroutine);
                    if (dbLoginSimple.State != MessageState.Reply)
                    {
                        continue;
                    }
                    if (dbLoginSimple.ErrorCode != 0)
                    {
                        continue;
                    }
                    var loginsimple = AsyncReturnValue<DBCharacterLoginSimple>.Create();
                    var co = CharacterManager.Instance.GetSimpeData(coroutine, characterId, loginsimple);
                    if (co.MoveNext())
                    {
                        yield return co;
                    }
                    if (loginsimple.Value == null)
                    {
                        continue;
                    }

                    var elfid = 0;
                    var elfStar = -1;
                    if (dbLogicSimple.Response.Equips.ItemsChange.ContainsKey((int)eBagType.Elf))
                    {
                        var elfItem = dbLogicSimple.Response.Equips.ItemsChange[(int)eBagType.Elf];
                        elfid = elfItem.ItemId;
                        if ((int)ElfExdataDefine.StarLevel < elfItem.Exdata.Count)
                            elfStar = elfItem.Exdata[(int)ElfExdataDefine.StarLevel];
                    }

                    var info = new CharacterSimpleInfo
                    {
                        CharacterId = loginsimple.Value.Id,
                        Name = loginsimple.Value.Name,
                        RoleId = loginsimple.Value.TypeId,
                        Level = dbLogicSimple.Response.Level,
                        Ladder = dbLogicSimple.Response.Ladder,
                        FightElfId = elfid,
                        FightElfStar = elfStar
                    };
                    info.EquipsModel.AddRange(dbLogicSimple.Response.EquipsModel);
                    msg.Response.Info.Add(info);
                }
            }
            var serverName = Table.GetServerName(serverId);
            if (serverName != null)
            {
                msg.Response.LogicServerId = serverName.LogicID;
            }
            else
            {
                msg.Response.LogicServerId = serverId;
            }

            if (msg.Response.Info.Count > 0 && playerController.DbData.SelectChar == 0)
            {
                msg.Response.SelectId = msg.Response.Info[msg.Response.Info.Count - 1].CharacterId;
            }
            else
            {
                msg.Response.SelectId = playerController.DbData.SelectChar;
            }

            msg.Reply();
        }

        public IEnumerator SyncTime(Coroutine coroutine, LoginService _this, SyncTimeInMessage msg)
        {
            msg.Response = (ulong) DateTime.Now.ToBinary();
            msg.Reply();
            return null;
        }

        //获得服务器列表
        public IEnumerator GetServerList(Coroutine coroutine, LoginService _this, GetServerListInMessage msg)
        {
            var __this = (LoginServerControl) _this;
            var clientId = msg.ClientId;
            var playerController = __this.PlayerManager.GetPlayerController(clientId);
            if (playerController != null)
            {
                PlayerLog.PlayerLogger(playerController.DbData.Id, "----------Login----------GetServerList----------{0}",
                    clientId);
            }

            bool isGM = false;
			isGM = ISGMAccount(playerController.DbData.Name);
            GetServerListData(_this, msg.Response, playerController, isGM);
            msg.Reply();
            yield break;
        }

        public IEnumerator NotifyConnected(Coroutine coroutine, LoginService _this, NotifyConnectedInMessage msg)
        {
            var __this = (LoginServerControl) _this;
            Action<ServiceType, ErrorCodes> call;
            if (__this.NotifyConnectedCallbacks.TryGetValue(msg.Request.CharacterId, out call))
            {
                var request = msg.Request;
                var type = (ServiceType) request.ServictType;
                var err = (ErrorCodes) request.Err;
                call(type, err);
            }

            return null;
        }

        public IEnumerator GetLoginSimpleData(Coroutine coroutine, LoginService _this, GetLoginSimpleDataInMessage msg)
        {
            msg.Response.Id = msg.Request.CharacterId;
            CharacterManager.Instance.GetSimpeData(msg.Request.CharacterId, simple =>
            {
                if (simple == null)
                {
                    msg.Reply((int) ErrorCodes.Unknow);
                    return;
                }
                msg.Response.TypeId = simple.TypeId;
                msg.Response.Id = simple.Id;
                msg.Response.Name = simple.Name;
                msg.Response.ServerId = simple.ServerId;
                msg.Response.LoginOut = simple.LoginOut;
                msg.Reply();
            });
            yield break;
        }

        public IEnumerator GetTodayOnlineSeconds(Coroutine coroutine,
                                                 LoginService _this,
                                                 GetTodayOnlineSecondsInMessage msg)
        {
            var cId = msg.Request.CharacterId;
            PlayerLog.WriteLog(cId, "----------Login----------GetTodayOnlineSeconds----------{0}", cId);
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(cId);
            if (character == null)
            {
                Logger.Error("GetTodayOnlineSeconds Error characterId = {0} null", cId);
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }
            msg.Response = character.GetTodayOnlineTime();
            msg.Reply();
        }

        //是否有该Id的角色
        public IEnumerator CheckIsHaveCharacter(Coroutine coroutine,
                                                LoginService _this,
                                                CheckIsHaveCharacterInMessage msg)
        {
            var cId = msg.Request.CharacterId;
            //PlayerLog.WriteLog(cId, "----------Login----------CheckIsHaveCharacter----------{0}", cId);
            var retDbId = LoginServer.Instance.DB.Get<DBCharacterLogin>(coroutine, DataCategory.LoginCharacter,
                cId);
            yield return null;
            if (retDbId.Status == DataStatus.Ok)
            {
                if (retDbId.Data != null)
                {
                    msg.Response = 1;
                    msg.Reply();
                    yield break;
                }
            }
            msg.Reply((int) ErrorCodes.Error_LoginDB_NoCharacter);
        }

        //名字获得玩家ID
        public IEnumerator GetCharacterIdByName(Coroutine coroutine,
                                                LoginService _this,
                                                GetCharacterIdByNameInMessage msg)
        {
            var retDbId = LoginServer.Instance.DB.Get<DBUlong>(coroutine, DataCategory.LoginCharacterName,
                msg.Request.Name);
            yield return null;
            if (retDbId.Status == DataStatus.Ok)
            {
                if (retDbId.Data != null)
                {
                    msg.Response = retDbId.Data.Value;
                    PlayerLog.WriteLog(msg.Response, "----------Login----------GetCharacterIdByName----------{0}",
                        msg.Request.Name);
                    msg.Reply();
                    yield break;
                }
            }
            msg.Reply((int) ErrorCodes.NameNotFindCharacter);
        }

        public IEnumerator GetTotleOnlineSeconds(Coroutine coroutine,
                                                 LoginService _this,
                                                 GetTotleOnlineSecondsInMessage msg)
        {
            var charId = msg.Request.CharacterId;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(charId);
            if (character == null)
            {
                CharacterManager.Instance.GetSimpeData(charId, simple =>
                {
                    msg.Response = simple.TotleOlineTime;
                    msg.Reply();
                });
                yield break;
            }
            msg.Response = character.GetData().TotleOlineTime;
            msg.Reply();
        }

        public IEnumerator GetPlayerIdByAccount(Coroutine coroutine,
                                                LoginService _this,
                                                GetPlayerIdByAccountInMessage msg)
        {
            var account = msg.Request.Account;
            var dbAccoutGuid = LoginServer.Instance.DB.Get<DBUlong>(coroutine, DataCategory.LoginPlayerName,
                account);
            yield return dbAccoutGuid;
            if (dbAccoutGuid.Data == null)
            {
                msg.Reply((int) ErrorCodes.Error_NoAccount);
                yield break;
            }
            msg.Response = dbAccoutGuid.Data.Value;
            msg.Reply();
        }

        public IEnumerator QueryServerTimezone(Coroutine coroutine, LoginService _this, QueryServerTimezoneInMessage msg)
        {
            msg.Response = Diff;
            msg.Reply();
            yield break;
        }

		public IEnumerator ServerGMCommand(Coroutine coroutine, LoginService _this, ServerGMCommandInMessage msg)
        {
			var cmd = msg.Request.Cmd;
			var param = msg.Request.Param;

			Logger.Info("Login----------ServerGMCommand----------cmd={0}|param={1}", cmd, param);

			try
			{
				if ("ReloadTable" == cmd)
				{
					Table.ReloadTable(param);
				}
				else if ("ReloadAccountList" == cmd)
				{
					LoginServer.Instance.s_SpecialAccount.LoadConfig();
				}
			}
			catch (Exception e)
			{
				Logger.Error("Login----------ServerGMCommand----------error={0}", e.Message);
			}
			finally
			{
				
			}
			yield break;
        }

        public IEnumerator ReadyToEnter(Coroutine coroutine, LoginService _this, ReadyToEnterInMessage msg)
        {
            if (LoginServer.Instance.IsReadyToEnter && LoginServer.Instance.AllAgentConnected())
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

        public IEnumerator GetPlayerData(Coroutine coroutine, LoginService _this, GetPlayerDataInMessage msg)
        {
            var __this = (LoginServerControl) _this;
            var playerId = msg.Request.PlayerId;
            var charId = msg.Request.CharId;

            PlayerController player = null;
            if (charId > 0)
            {
                var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(charId);
                if (character != null)
                {
                    playerId = character.GetData().PlayerId;
                }
            }
            if (playerId > 0)
            {
                player = __this.PlayerManager.Players.Values.FirstOrDefault(p => p.DbData.Id == playerId);
            }

            DBPlayerLogin playerDb = null;
            if (player != null)
            {
                playerDb = player.DbData;
            }
            else
            {
                if (playerId == 0)
                {
                    var retDbId = LoginServer.Instance.DB.Get<DBCharacterLogin>(coroutine,
                        DataCategory.LoginCharacter, msg.Request.CharId);
                    yield return retDbId;
                    if (retDbId.Status != DataStatus.Ok || retDbId.Data == null)
                    {
                        msg.Reply((int) ErrorCodes.Error_CharacterId_Not_Exist);
                        yield break;
                    }
                    playerId = retDbId.Data.PlayerId;
                }

                var dbAccout = LoginServer.Instance.DB.Get<DBPlayerLogin>(coroutine, DataCategory.LoginPlayer,
                    playerId);
                yield return dbAccout;
                if (dbAccout.Status != DataStatus.Ok || dbAccout.Data == null)
                {
                    msg.Reply((int) ErrorCodes.Error_PlayerId_Not_Exist);
                    yield break;
                }
                playerDb = dbAccout.Data;
            }

            if (playerDb != null)
            {
                msg.Response = new GMPlayerInfoMsg
                {
                    Id = playerDb.Id,
                    Name = playerDb.Name,
                    Type = playerDb.Type,
                    FoundTime = playerDb.FoundTime,
                    LoginDay = playerDb.LoginDay,
                    LoginTotal = playerDb.LoginTotal,
                    LastTime = playerDb.LastTime,
                    BindPhone = playerDb.BindPhone,
                    BindEmail = playerDb.BindEmail,
                    LockTime = playerDb.LockTime
                };
                foreach (var serversPlayer in playerDb.ServersPlayers)
                {
                    var charactersOnServer = new GMCharactersServers();
                    charactersOnServer.ServerId = serversPlayer.Key;

                    var checkCharList = new List<GMCharacterInfo>();

                    foreach (var characterId in serversPlayer.Value.Items)
                    {
                        var characterData = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);

                        if (characterData != null)
                        {
                            var charInfo = new GMCharacterInfo();
                            charInfo.CharacterId = characterId;
                            charInfo.Name = characterData.GetData().Name;
                            charInfo.Type = characterData.GetData().TypeId;

                            charactersOnServer.Characters.Add(charInfo);

                            checkCharList.Add(charInfo);
                        }
                        else
                        {
                            var dbCharacter = LoginServer.Instance.DB.Get<DBCharacterLogin>(coroutine,
                                DataCategory.LoginCharacter, characterId);
                            yield return null;
                            if (dbCharacter.Status != DataStatus.Ok || dbCharacter.Data == null)
                            {
                                msg.Reply((int) ErrorCodes.Error_CharacterId_Not_Exist);
                                yield break;
                            }

                            var charInfo = new GMCharacterInfo();
                            charInfo.CharacterId = characterId;
                            charInfo.Name = dbCharacter.Data.Name;
                            charInfo.Type = dbCharacter.Data.TypeId;

                            charactersOnServer.Characters.Add(charInfo);
                        }
                    }

                    // 检查是否在线
                    var idList = new Uint64Array();
                    idList.Items.AddRange(checkCharList.Select(characterData => characterData.CharacterId));
                    var checkOnlineData = LoginServer.Instance.SceneAgent.SBCheckCharacterOnline(0, idList);
                    yield return checkOnlineData.SendAndWaitUntilDone(coroutine);

                    if (checkOnlineData.State == MessageState.Reply && checkOnlineData.ErrorCode == (int) ErrorCodes.OK)
                    {
                        for (var i = 0; i < checkCharList.Count; i++)
                        {
                            checkCharList[i].IsOnline = checkOnlineData.Response.Items[i] != 0;
                        }
                    }

                    msg.Response.CharactersServers.Add(charactersOnServer);
                }
                msg.Reply();
                yield break;
            }
            msg.Reply((int) ErrorCodes.Error_Player_Not_On_This_Server);
        }

        public IEnumerator KickCharacter(Coroutine coroutine, LoginService _this, KickCharacterInMessage msg)
        {
            var __this = (LoginServerControl) _this;
            var clientId = msg.ClientId;
            var playerController = __this.PlayerManager.GetPlayerController(clientId);
            if (playerController != null)
            {
                PlayerLog.PlayerLogger(playerController.DbData.Id, "----------Login----------KickCharacter----------{0}",
                    clientId);
                playerController.Kick(clientId, KickClientType.OtherLogin);
            }
            yield break;
        }

        public IEnumerator GMKickCharacter(Coroutine coroutine, LoginService _this, GMKickCharacterInMessage msg)
        {
            var __this = (LoginServerControl) _this;
            var charId = msg.Request.CharId;
            if (charId == 0)
            {
                var retDbId = LoginServer.Instance.DB.Get<DBUlong>(coroutine, DataCategory.LoginCharacterName,
                    msg.Request.Name);
                yield return null;
                if (retDbId.Status == DataStatus.Ok && retDbId.Data != null)
                {
                    charId = retDbId.Data.Value;
                }
                else
                {
                    msg.Reply((int) ErrorCodes.NameNotFindCharacter);
                    yield break;
                }
            }
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(charId);
            if (character != null)
            {
                var player = __this.PlayerManager.GetPlayerControllerByPlayerId(character.GetData().PlayerId);
                if (player != null && player.Connect != null)
                {
                    player.Kick(player.Connect.ClientId, KickClientType.GmKick);
                    msg.Reply();
                    yield break;
                }
            }
            else
            {
                // 如果传进来的ID > 100000000,就把它变成playerID
                if (charId > 1100000000)
                {
                    var realId = charId - 1000000000;
                    var player = __this.PlayerManager.GetPlayerControllerByPlayerId(realId);
                    if (player != null && player.Connect != null)
                    {
                        player.Kick(player.Connect.ClientId, KickClientType.GmKick);
                    }
                    else
                    {
                        __this.PlayerManager.PlayersByPlayerId.Remove(realId);
                        __this.PlayerManager.Players.Remove(realId);
                    }

                    msg.Reply();
                    yield break;
                }
            }
            msg.Reply((int) ErrorCodes.Unline);
        }

        public IEnumerator ExitLogin(Coroutine coroutine, LoginService _this, ExitLoginInMessage msg)
        {
            var __this = (LoginServerControl) _this;

            var co = CoroutineFactory.NewSubroutine(LoginoutEx, coroutine, __this, msg);
            if (co.MoveNext())
            {
                yield return co;
            }
        }

        public IEnumerator ExitSelectCharacter(Coroutine coroutine, LoginService _this, ExitSelectCharacterInMessage msg)
        {
            var __this = (LoginServerControl) _this;

            var clientId = msg.ClientId;
            var player = __this.PlayerManager.GetPlayerController(clientId);
            if (player == null || player.Connect == null)
            {
                QueueManager.ClinetIdLost(clientId);
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }
            var stateCheck = false;
            switch (player.Connect.State)
            {
                case ConnectState.EnterGame:
                case ConnectState.InGame:
                {
                    stateCheck = true;
                }
                    break;
                case ConnectState.NotFind:
                    break;
                case ConnectState.Wait:
                    break;
                case ConnectState.Landing:
                    break;
                case ConnectState.OffLine:
                    break;
                case ConnectState.WaitOffLine:
                    break;
                default:
                    break;
            }

            if (stateCheck == false)
            {
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }


//             __this.Logout(clientId, msg.Request.CharacterId);
// 
//             using (var result = AsyncReturnValue<bool>.Create())
//             {
//                 var subroutine = CoroutineFactory.NewSubroutine(CheckLost, coroutine, msg.Request.CharacterId, result);
//                 while (subroutine.MoveNext())
//                     yield return subroutine;
//             }
// 
//             var playerController = __this.PlayerManager.GetPlayerController(clientId);
//             if (playerController == null)
//             {
//                 msg.Reply((int)ErrorCodes.Error_Login_NotLogin);
//                 yield break;
//             }
//             PlayerLog.PlayerLogger(playerController.DbData.Id, "----------Login----------ExitSelectCharacter----------{0}", clientId);

//             //------------------------------------------------------------------------------------------
            var characterId = msg.Request.CharacterId;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (null == character)
            {
                msg.Reply((int) ErrorCodes.Error_Not_Login_find);
                yield break;
            }
            if (player.Connect == null)
            {
                msg.Reply((int) ErrorCodes.Error_Connect_find);
                yield break;
            }
            var serverId = character.GetData().ServerId;


            //如果是从游戏中退出，同步等级和转生数据到login
            if (player.Connect.State == ConnectState.InGame)
            {
                var subCo = CoroutineFactory.NewSubroutine(character.SyncCharacterLevelData, coroutine);
                if (subCo.MoveNext())
                {
                    yield return subCo;
                }
            }

//             character.LostLine();
//             var co1 = CoroutineFactory.NewSubroutine(CharacterManager.Instance.RemoveCharacter, coroutine, character.CharacterId);
//             if (co1.MoveNext())
//                 yield return co1;
            NotifyGateClientState(__this, clientId, 0, GateClientState.Login);
            CleanCharacterData(clientId, characterId);
            //清理玩家账号数据
            var co = CoroutineFactory.NewSubroutine(PlayerLogoutEx, coroutine, __this, clientId);
            if (co.MoveNext())
            {
                yield return co;
            }
            //QueueManager.PushLoginState(player.Connect);

            Uint64Array characters;
            if (player.DbData.ServersPlayers.TryGetValue(serverId, out characters))
            {
                foreach (var id in characters.Items)
                {
                    var dbLogicSimple = LoginServer.Instance.LogicAgent.GetLogicSimpleData(id, 0);
                    yield return dbLogicSimple.SendAndWaitUntilDone(coroutine);
                    if (dbLogicSimple.State != MessageState.Reply)
                    {
                        continue;
                    }
                    var dbLoginSimple = LoginServer.Instance.LoginAgent.GetLoginSimpleData(clientId, id);
                    yield return dbLoginSimple.SendAndWaitUntilDone(coroutine);
                    if (dbLoginSimple.State != MessageState.Reply)
                    {
                        continue;
                    }

                    var elfid = 0;
                    var elfStar = -1;
                    if (dbLogicSimple.Response.Equips.ItemsChange.ContainsKey((int) eBagType.Elf))
                    {
                        var elfItem = dbLogicSimple.Response.Equips.ItemsChange[(int)eBagType.Elf];
                        elfid = elfItem.ItemId;
                        if ((int)ElfExdataDefine.StarLevel < elfItem.Exdata.Count)
                            elfStar = elfItem.Exdata[(int)ElfExdataDefine.StarLevel];
                    }

                    var info = new CharacterSimpleInfo
                    {
                        CharacterId = id,
                        Name = dbLoginSimple.Response.Name,
                        RoleId = dbLoginSimple.Response.TypeId,
                        Level = dbLogicSimple.Response.Level,
                        Ladder = dbLogicSimple.Response.Ladder,
                        FightElfId = elfid,
                        FightElfStar = elfStar
                    };
                    info.EquipsModel.AddRange(dbLogicSimple.Response.EquipsModel);
                    msg.Response.Info.Add(info);
                }
            }
            //------------------------------------------------------------------------------------------
            if (player.DbData.SelectChar == 0)
            {
                msg.Response.SelectId = msg.Response.Info[msg.Response.Info.Count - 1].CharacterId;
            }
            else
            {
                msg.Response.SelectId = player.DbData.SelectChar;
            }
            msg.Reply();
        }

        public IEnumerator QueryStatus(Coroutine coroutine, LoginService _this, QueryStatusInMessage msg)
        {
            yield break;
        }

        //第三方登录验证
        public IEnumerator CheckAccessToken(Coroutine co,
                                            LoginServerControl _this,
                                            string channel,
                                            string token,
                                            string uid,
                                            AsyncReturnValue<ThirdLoginResult> returnValue)
        {
            var sw = Stopwatch.StartNew();
            ThirdLoginLogger.Info("CheckAccessToken Channel :  {0} time:{1} Step - 1 - {2}", channel,
                sw.ElapsedMilliseconds, TimeManager.Timer.ElapsedMilliseconds);
            returnValue.Value = null;

            if (channel.Equals("moe") || channel.Equals("moetest"))
            {
                #region typeSDK
               // const string apikey = "a6f3c0428a23b5e65cf19a122bc852a7";
               // const string apikey = "82ef9bd77183365b6bb78bc513254647";
                const string apikey = "mayakey";
                var info = Regex.Split(uid, "@_@");
//                 if (info.Length < 2)
//                 {
//                     Logger.Error("CheckAccessToken typeSDK error!! client send info size < 3 info:{0}", uid);
//                     yield break;
//                 }
                
//                 var cpid = "1004";//info[0];
//                 var channel_id = "100";//info[1];
//                 var id = info[0];


                var cpid = info[0];
                var channel_id = info[1];
                var id = info[2];
                
                var data = "";
                var sb = new StringBuilder();
                if (null != id)
                {
                    sb.Append(id);
                }
                else
                {
                    sb.Append("");
                }
                sb.Append('|');
                sb.Append(token);
                sb.Append('|');
                sb.Append(data);
                sb.Append('|');
                sb.Append(apikey);

                var sign = RequestManager.Encrypt_MD5_UTF8(sb.ToString());
                var dic = new Dictionary<string, string>();
                dic.Add("id", id);
                dic.Add("token", token);
                dic.Add("data", data);
                dic.Add("sign", sign);

                string url = string.Format("http://sdk.uborm.com:40000/{0}/{1}/Login/", cpid, channel_id);
                //var url = @"http://sdk.uborm.com:40000/1004/100/Login/";

                var result = AsyncReturnValue<string>.Create();
                yield return _this.WebRequestManager.DoRequest(co, url, dic, result);

                if (string.IsNullOrEmpty(result.Value))
                {
                    Logger.Error("CheckAccessToken get webResponse is null.channel:{0},url{1}", channel, url);
                    yield break;
                }

                var jsonObj = (JObject)JsonConvert.DeserializeObject(result.Value);
                var resultCode = jsonObj["code"].ToString();
                var openid = jsonObj["id"].ToString();
                var resultDesc = jsonObj["msg"].ToString();


                if (!resultCode.Equals("0"))
                {
                    Logger.Error("CheckAccessToken typeSDK web response return error. code :{0}, errordesc:{1}",
                        resultCode, resultDesc);
                    yield break;
                }

                returnValue.Value = new ThirdLoginResult
                {
                    userid = openid,
                    userName = "LG"+openid,
                    spid = channel_id
                };

            }
                #endregion
            else if (channel.Equals("37"))
            {
                #region 37

                const string url = @"http://vt.api.m.37.com/verify/token/";

                var info = (JObject) JsonConvert.DeserializeObject(token);
                var pid = info["pid"].ToString();
                var gid = info["gid"].ToString();
                var gameid = info["appid"].ToString();
                var channelid =  info["chhid"].ToString();
                var accessToken = info["token"].ToString();
                var timestamp = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

                var dic = new Dictionary<string, string>();
                dic.Add("pid", pid);
                dic.Add("gid", gid);
                dic.Add("time", timestamp.ToString());
                dic.Add("sign", Get37Sign(dic));
                dic.Add("token", accessToken);

                var result = AsyncReturnValue<string>.Create();
                yield return _this.WebRequestManager.DoRequest(co, url, dic, result);

                if (string.IsNullOrEmpty(result.Value))
                {
                    Logger.Error("CheckAccessToken 37 get webResponse is null.channel:{0},url{1}", channel, url);
                    yield break;
                }

                var jsonObj = (JObject)JsonConvert.DeserializeObject(result.Value);
                var resultCode = (int)jsonObj["state"];
                var data = (JObject)jsonObj["data"];

                if (resultCode != 1)
                {
                    Logger.Error("CheckAccessToken 37 web response return error. code :{0}",
                        resultCode);
                    yield break;
                }

                var userId = data["uid"].ToString();

                returnValue.Value = new ThirdLoginResult
                {
                    userid = userId,
                    userName = "SQ" + userId,
                    spid = pid
                };

                returnValue.Value.moniterData = new MsgChatMoniterData();

                int _gid;
                int.TryParse(gid, out _gid);
                returnValue.Value.moniterData.gid = _gid;
                returnValue.Value.moniterData.uid = userId;
                returnValue.Value.moniterData.channel = channel;
                returnValue.Value.moniterData.pid = pid;
            }

                #endregion

        }

        public static void UrlEncodeDictionary(ref Dictionary<string, string> paramsDic)
        {
            var keys = paramsDic.Keys;
            var newDic = new Dictionary<string, string>(paramsDic.Count);
            foreach (var key in keys)
            {
               var encodeValue = HttpUtility.UrlEncode(paramsDic[key]);
               newDic.Add(key, encodeValue);
            }
            paramsDic = newDic;
        }

        public static string GetQianHuanSign(Dictionary<string, string> paramsDic)
        {
            const string secretKey = "b01895af049699f35c40294901c1fe9d";
            var newDic = new Dictionary<string, string>(paramsDic) {{"server_secret", secretKey}};
            var array = newDic.Keys.ToArray();
            var sorted = array.OrderBy(x => x);
            var sb = new StringBuilder();
            foreach (var key in sorted)
            {
                var value = newDic[key];
                if (string.IsNullOrEmpty(value) || value.Equals("0"))
                {
                    continue;
                }
                sb.Append(key);
                sb.Append("=");
                sb.Append(value);
            }

            return RequestManager.Encrypt_MD5_UTF8(sb.ToString()).ToLower();
        }

        public static string Get37Sign(Dictionary<string, string> paramsDic)
        {
            const string key = @"X&+tA13asLcm5j7gzxSwFJ96BMZlYNDE";
            var sb = new StringBuilder();
            sb.Append(paramsDic["gid"]);
            sb.Append(paramsDic["time"]);
            sb.Append(key);
            return RequestManager.Encrypt_MD5_UTF8(sb.ToString()).ToLower();
        }


        public static string GetStarJoysSign(string appid, string chhid, string token,string time)
        {
            const string appkey = "rRFDnGK1Nip9Wb6";
            var paramsDic = new Dictionary<string, string>();
            paramsDic.Add(arrayFields[0], appid);
            paramsDic.Add(arrayFields[1], chhid);
            paramsDic.Add(arrayFields[2], token);
            paramsDic.Add(arrayFields[3], time);

            var array = paramsDic.Keys.ToArray();
            var sorted = array.OrderBy(x => x);
            var sb = new StringBuilder();

            int count = 0;
            foreach (var key in sorted)
            {
                var value = paramsDic[key];
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }
                sb.Append(key);
                sb.Append("=");
                var encodevalue = HttpUtility.UrlEncode(value);
                sb.Append(encodevalue);
                count++;
                if (count != sorted.Count())
                {
                    sb.Append("&");
                }
                else
                {
                    sb.Append(appkey);
                }

            }
            return RequestManager.Encrypt_MD5_UTF8(sb.ToString());
        }
        [Updateable("LoginServer")]
        private static readonly string[] arrayFields =
        {
            "app_id",
            "cch_id",
            "access_token", 
            "tm",
        };    
    }




    public interface IStaticLoginServerControl
    {
        IEnumerator CheckAccessToken(Coroutine co,
                                     LoginServerControl _this,
                                     string channel,
                                     string token,
                                     string uid,
                                     AsyncReturnValue<ThirdLoginResult> returnValue);

        void OnDayChangeEvent(LoginService _this);
        IEnumerator PlayerLogout(Coroutine coroutine, LoginServerControl __this, ulong clientid);
        IEnumerator PlayerLogoutEx(Coroutine coroutine, LoginServerControl __this, ulong clientid);
    }

    public class LoginServerControl : LoginService
    {
        public static TimeManager Timer = new TimeManager();

        public LoginServerControl()
        {
            LoginServer.Instance.UpdateManager.InitStaticImpl(typeof (LoginServerControl),
                typeof (LoginServerControlDefaultImpl),
                o => { SetServiceImpl((ILoginService) o); });
        }

        //public HashSet<ulong> CurrentConnectedClients = new HashSet<ulong>();
        public Trigger DayChangeTrigger;
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public DateTime NextGetServerCharacterCountTime;
        public DateTime NextRefreshServerListTime;

        public readonly Dictionary<ulong, Action<ServiceType, ErrorCodes>> NotifyConnectedCallbacks =
            new Dictionary<ulong, Action<ServiceType, ErrorCodes>>();

        public PlayerManager PlayerManager = new PlayerManager();
        public Dictionary<int, int> ServerCharacterCount;
        public RequestManager WebRequestManager = null;
        private long tickTime = 0;

        public IStaticLoginServerControl GetImpl()
        {
            return (IStaticLoginServerControl) mImpl;
        }

        /// <summary>
        ///     DO NOT use characterId in this function, it's always zero.
        /// </summary>
        /// <param name="coroutine"></param>
        /// <param name="target"></param>
        /// <param name="clientid"></param>
        /// <param name="characterId"></param>
        /// <returns></returns>
        public override IEnumerator OnClientConnected(Coroutine coroutine,
                                                      string target,
                                                      ulong clientid,
                                                      ulong characterId,
                                                      uint packId)
        {
            return mImpl.OnClientConnected(coroutine, this, target, clientid, characterId, packId);
        }

        /// <summary>
        ///     DO NOT use characterId in this function, it's always zero.
        /// </summary>
        /// <param name="coroutine"></param>
        /// <param name="target"></param>
        /// <param name="clientid"></param>
        /// <param name="characterId"></param>
        /// <returns></returns>
        public override IEnumerator OnClientLost(Coroutine coroutine,
                                                 string target,
                                                 ulong clientid,
                                                 ulong characterId,
                                                 uint packId)
        {
            return mImpl.OnClientLost(coroutine, this, target, clientid, characterId, packId);
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

        public IEnumerator PlayerLogout(Coroutine coroutine, ulong clientid)
        {
            var t = mImpl as IStaticLoginServerControl;
            return t.PlayerLogout(coroutine, this, clientid);
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

                //foreach (var agent in LoginServer.Instance.Agents.ToArray())
                //{
                //    dict.TryAdd(agent.Key + " Latency", agent.Value.Latency.ToString());
                //    dict.TryAdd(agent.Key + " ByteReceivedPerSecond", agent.Value.ByteReceivedPerSecond.ToString());
                //    dict.TryAdd(agent.Key + " ByteSendPerSecond", agent.Value.ByteSendPerSecond.ToString());
                //    dict.TryAdd(agent.Key + " MessageReceivedPerSecond", agent.Value.MessageReceivedPerSecond.ToString());
                //    dict.TryAdd(agent.Key + " MessageSendPerSecond", agent.Value.MessageSendPerSecond.ToString());
                //}

                //var playerCount = QueueManager.PlayerCount;
                //foreach (var i in playerCount)
                //{
                //    dict.TryAdd("Server " + i.Key + " Player count", i.Value.ToString());
                //}
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "LoginServerControl Status Error!{0}");
            }
        }

        public override IEnumerator Tick(Coroutine coroutine)
        {
            tickTime++;
            return ((ITickable) mImpl).Tick(coroutine, this);
        }
    }
}