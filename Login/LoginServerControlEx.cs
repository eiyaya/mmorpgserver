#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Database;
using DataContract;
using DataTable;
using LoginServerService;
using Scorpion;

using Shared;
using NLog;
using System.Text;
//using Chat;

#endregion

namespace Login
{
    public partial class LoginServerControlDefaultImpl : ILoginService, ITickable, IStaticLoginServerControl
    {
        private static readonly Logger kafaLogger = LogManager.GetLogger(Shared.LoggerName.KafkaLog);

        public string IntToIp(long ipInt)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append((ipInt >> 24) & 0xFF).Append(".");
            sb.Append((ipInt >> 16) & 0xFF).Append(".");
            sb.Append((ipInt >> 8) & 0xFF).Append(".");
            sb.Append(ipInt & 0xFF);
            return sb.ToString();
        }

        //登录
        public IEnumerator LoginEx(Coroutine coroutine,
                                   LoginService _this,
                                   List<string> l,
                                   InMessage msg,
                                    MsgChatMoniterData moniterData)
        {
            var __this = (LoginServerControl)_this;
            var clientId = msg.ClientId;
            string userName = l[0];
            string userPsw = l[1];
            string uid = l[2];
            string loginChannel = l[3];
            string platform = l[4];
            var s = Stopwatch.StartNew();
            Logger.Info("Enter Game {0} Login - 1 - {1}", userName, TimeManager.Timer.ElapsedMilliseconds);
            s.Reset();

            try
            {
                LoginServerMonitor.LoginRate.Mark();
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
            }

            try
            {
                if (LoginServer.Instance.s_SpecialAccount.BlackAccounts.Contains(userName))
                {
                    msg.Reply((int)ErrorCodes.PlayerAccountIsLock);
                    Logger.Warn("LoginEx BlackAccounts [{0}]", userName);
                    yield break;
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
            }

            #region 此链接 是否已登陆

            var playerController = __this.PlayerManager.GetPlayerController(clientId);
            if (playerController != null)
            {
                if (playerController.Connect != null)
                {
                    PlayerLog.WriteLog((int)LogType.Error_Login_AlreadyLogin,
                        "PlayerLoginByUserNamePassword 1, name = {0} ,state={1}", userName,
                        playerController.Connect.State);
                }
                else
                {
                    PlayerLog.WriteLog((int)LogType.Error_Login_AlreadyLogin,
                        "PlayerLoginByUserNamePassword 1, name = {0} ", userName);
                }
                msg.Reply((int)ErrorCodes.Error_Login_AlreadyLogin);
                yield break;
            }

            #endregion

            #region //查看排队缓存

            var queue = QueueManager.MainQueue;
            var connet = QueueManager.IsCache(loginChannel, clientId, userName);
            if (connet == null)
            {
                //没有缓存的账号
                var result = queue.CheckQueueState();
                if (result == PlayerQueueType.More)
                {
                    PlayerLog.WriteLog((int)LogType.Error_PLayerLoginMore,
                        "PlayerLoginByUserNamePassword name = {0},state={1}", userName, result);
                    msg.Reply((int)ErrorCodes.Error_PLayerLoginMore);
                    yield break;
                }
            }

            #endregion

            connet = null;

            #region 判断账号是否已登陆

            var key = PlayerConnect.GetLandingKey(loginChannel, userName);
            if (__this.PlayerManager.NameKey2Player.TryGetValue(key, out playerController))
            {
                connet = playerController.Connect;

                //提掉角色
                if (connet != null)
                {
                    var state = connet.State;
                    var oldClinetId = connet.ClientId;
                    PlayerLog.WriteLog((int)LogType.LoginLog,
                        "--LoginEx---clientId:{0}---state:{1}----oldClinetId--{2}",
                        clientId, state, oldClinetId);
                    switch (state)
                    {
                        case ConnectState.Wait:
                            {
                                //说明之前有人正在排队，这次ClientId是新的，需要断开之前的ClientId
                                NotifyGateClientLost(__this, oldClinetId, KickClientType.OtherLogin);
                                msg.Reply((int)ErrorCodes.Error_PLayerLoginWait);
                                yield break;
                            }
                        //break;
                        case ConnectState.Landing:
                            {
                                NotifyGateClientLost(__this, oldClinetId, KickClientType.OtherLogin);
                            }
                            break;
                        case ConnectState.EnterGame:
                            {
                                msg.Reply((int)ErrorCodes.PlayerEnterGamming);
                                yield break;
                            }
                        case ConnectState.InGame:
                            {
                                if (connet.Player == null)
                                {
                                    Logger.Warn(
                                        "KickClientType.OtherLogin CleanClientData player not find,oldClinetId={0}",
                                        oldClinetId);
                                    CleanClientData(__this, oldClinetId, 0, KickClientType.OtherLogin);
                                }
                                else
                                {
                                    var selectCharacterID = connet.Player.DbData.SelectChar;
                                    CleanClientData(__this, oldClinetId, selectCharacterID, KickClientType.OtherLogin);
                                    var character =
                                        CharacterManager.Instance.GetCharacterControllerFromMemroy(selectCharacterID);
                                    if (character != null)
                                    {
                                        character.LostLine();
                                        CharacterManager.PopServerPlayer(connet.Player.DbData.LastServerId); //登陆失败
                                        var removeCo = CoroutineFactory.NewSubroutine(RemoveCharacterData, coroutine,
                                            selectCharacterID);
                                        if (removeCo.MoveNext())
                                        {
                                            yield return removeCo;
                                        }
                                    }
                                }

                                PlayerConnect oldPlayerConnect;
                                if (QueueManager.InGamePlayerList.TryGetValue(playerController.DbData.Id,
                                    out oldPlayerConnect))
                                {
                                    QueueManager.InGamePlayerList.Remove(playerController.DbData.Id);
                                    if (oldPlayerConnect != connet)
                                    {
                                        Logger.Warn(
                                            "oldPlayerConnect !=connet oldPlayerConnect.ClienId={0} connet.ClientId={1}",
                                            oldPlayerConnect.ClientId, connet.ClientId);
                                    }
                                }

                                QueueManager.LandingPlayerList.TryAdd(playerController.DbData.Id, connet);
                                connet.State = ConnectState.Landing;
                            }
                            break;
                        case ConnectState.OffLine:
                            {
                                QueueManager.CacheLost.Remove(key);
                                var checkState = QueueManager.MainQueue.CheckQueueState();
                                QueueManager.TotalList.TryAdd(clientId, connet);
                                if (checkState == PlayerQueueType.NoWait)
                                {
                                    connet.State = ConnectState.Landing;
                                    QueueManager.LandingPlayerList.TryAdd(playerController.DbData.Id, connet);
                                }
                                else
                                {
                                    connet.State = ConnectState.Wait;
                                    QueueManager.MainQueue.WaitPlayerList.AddFirst(connet);
                                }
                            }
                            break;
                        case ConnectState.WaitOffLine:
                            {
                                QueueManager.CacheLost.Remove(key);
                                var checkState = QueueManager.MainQueue.CheckQueueState();
                                QueueManager.TotalList.TryAdd(clientId, connet);
                                if (checkState == PlayerQueueType.NoWait)
                                {
                                    connet.State = ConnectState.Landing;
                                    QueueManager.LandingPlayerList.TryAdd(playerController.DbData.Id, connet);
                                }
                                else
                                {
                                    connet.State = ConnectState.Wait;
                                }
                            }
                            break;
                        case ConnectState.WaitReConnet:
                            {
                                if (connet.Player == null || connet.Player.DbData == null)
                                {
                                    Logger.Error("This should be never happen !!! connet.Player == null || connet.Player.DbData == null  ID={0}", playerController.DbData.Id);
                                    __this.PlayerManager.PlayersByPlayerId.Remove(playerController.DbData.Id);
                                    __this.PlayerManager.Players.Remove(playerController.DbData.Id);
                                    connet.State = ConnectState.OffLine;
                                    break;
                                }
                               
                                CleanCharacterData(oldClinetId, connet.Player.DbData.SelectChar);
                                PlayerConnect oldPlayerConnect;
                                if (QueueManager.InGamePlayerList.TryGetValue(playerController.DbData.Id,
                                    out oldPlayerConnect))
                                {
                                    QueueManager.InGamePlayerList.Remove(playerController.DbData.Id);
                                    if (oldPlayerConnect != connet)
                                    {
                                        Logger.Warn(
                                            "oldPlayerConnect !=connet oldPlayerConnect.ClienId={0} connet.ClientId={1}",
                                            oldPlayerConnect.ClientId, connet.ClientId);
                                    }
                                }
                                QueueManager.TotalList.TryAdd(clientId, connet);
                                QueueManager.LandingPlayerList.Add(playerController.DbData.Id, connet);
                                connet.State = ConnectState.Landing;
                            }
                            break;
                    }
                    __this.PlayerManager.ModifyClientId(playerController.DbData.Id, clientId);
                }
                else
                {
                    Logger.Warn("LoginEx oldPlayer.Connet is null");
                }
            }

            #endregion

            #region // 重新加载账号相关信息

            if (playerController == null)
            {
                var loadPlayerRet = AsyncReturnValue<PlayerController>.Create();
                var co = CoroutineFactory.NewSubroutine(__this.PlayerManager.LoadPlayer, coroutine, clientId, userName,
                    loadPlayerRet);
                if (co.MoveNext())
                {
                    yield return co;
                }
                playerController = loadPlayerRet.Value;
                loadPlayerRet.Dispose();
                // loadPlayerRet.Value == 1  DB没有找到，需要Create
                if (playerController == null)
                {
                    //检查密码
                    if (Runtime.Mono && msg is PlayerLoginByUserNamePasswordInMessage)
                    {
                        if (userPsw != "123")
                        {
                            Logger.Warn(
                                "LoginEx ErrorCodes.PlayerCreateFaild userName = {0},uid ={1}.....PasswordIncorrect",
                                userName, uid);
                            msg.Reply((int)ErrorCodes.PasswordIncorrect);
                            yield break;
                        }
                    }
                    //检查密码通过
                    var createPlayerRet = AsyncReturnValue<PlayerController>.Create();
                    co = CoroutineFactory.NewSubroutine(__this.PlayerManager.CreatePlayer, coroutine, clientId, userName,
                        userPsw, loginChannel, createPlayerRet);
                    if (co.MoveNext())
                    {
                        yield return co;
                    }
                    playerController = createPlayerRet.Value;
                    createPlayerRet.Dispose();
                }
                //2次都没造出来（第一次是老账号，第二次是新账号）
                if (playerController == null)
                {
                    Logger.Error("LoginEx ErrorCodes.PlayerCreateFaild userName = {0},uid ={1} ", userName, uid);
                    msg.Reply((int)ErrorCodes.PlayerCreateFaild);
                    yield break;
                }

                if (DateTime.FromBinary(playerController.DbData.LockTime) > DateTime.Now)
                {
                    //被锁了
                    Logger.Warn("LoginEx ErrorCodes.PlayerCreateFaild userName = {0},uid ={1}.....PlayerAccountIsLock",
                        userName, uid);
                    msg.Reply((int)ErrorCodes.PlayerAccountIsLock);
                    yield break;
                }
                var addResult = __this.PlayerManager.AddPlayer(clientId, playerController);
                switch (addResult)
                {
                    case 0:
                        break;
                    case -1:
                        Logger.Error("LoginEx PlayerManager.AddPlayer! clientId={0},playerId={1},addResult={2}",
                            clientId, playerController.DbData.Id, addResult);
                        msg.Reply((int)ErrorCodes.ClientLoginMore);
                        yield break;
                    case -2:
                        Logger.Warn("LoginEx PlayerManager.AddPlayer! clientId={0},playerId={1},addResult={2}", clientId,
                            playerController.DbData.Id, addResult);
                        msg.Reply((int)ErrorCodes.PlayerLoginning);
                        yield break;
                    case -3:
                        Logger.Error("LoginEx PlayerManager.AddPlayer! clientId={0},playerId={1},addResult={2}",
                            clientId, playerController.DbData.Id, addResult);
                        msg.Reply((int)ErrorCodes.Unknow);
                        yield break;
                }
                __this.PlayerManager.NameKey2Player[key] = playerController;

            }
            else
            {
                if (DateTime.FromBinary(playerController.DbData.LockTime) > DateTime.Now)
                {
                    //被锁了
                    Logger.Warn("LoginEx ErrorCodes.PlayerCreateFaild userName = {0},uid ={1}.....PlayerAccountIsLock",
                        userName, uid);
                    msg.Reply((int)ErrorCodes.PlayerAccountIsLock);
                    yield break;
                }
            }

            Logger.Info("Enter Game {0} time:{1} Login - 4 - {2}", userName, s.ElapsedMilliseconds,
                TimeManager.Timer.ElapsedMilliseconds);
            s.Reset();

            #endregion

            PlayerLog.BackDataLogger((int)BackDataType.PlayerOnline, "{0}", playerController.DbData.Id);
            playerController.Platform = platform;
            if (loginChannel != null)
            {
                playerController.LoginChannel = loginChannel;
            }
            else
            {
                playerController.LoginChannel = "error";
            }

            try
            {
                // 先insert
                string playerLog = string.Format("player#{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}",
                    playerController.DbData.Id,  // playerid
                    DateTime.FromBinary(playerController.DbData.FoundTime).ToString("yyyy/MM/dd HH:mm:ss"), // createtime
                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), // lasttime
                    0,                   // lastcharacter
                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), // characterlasttime
                    0,      // onlinecount
                    playerController.DbData.LastServerId, // serverid
                    playerController.Platform, // createplatform
                    playerController.LoginChannel    // spid
                );
                kafaLogger.Info(playerLog);
            }
            catch (Exception)
            {
            }

            //---------加入进入游戏过程，有可能需要排队-------------
            if (playerController.Connect == null)
            {
                var check = queue.PushConnect(loginChannel, clientId, userName, playerController);
                if (check != PlayerQueueType.NoWait)
                {
                    PlayerLog.WriteLog((int)LogType.Error_PLayerLoginWait,
                        "PlayerLoginByUserNamePassword name = {0},state={1}", userName, check);
                    msg.Reply((int)ErrorCodes.Error_PLayerLoginWait);
                    yield break;
                }
                PlayerLog.WriteLog((int)LogType.Error_PLayerLoginNoWait,
                    "PlayerLoginByUserNamePassword name = {0},state={1}", userName, check);
            }

            //---------------------------------------------------
            PlayerLog.PlayerLogger(playerController.DbData.Id,
                "----------Login----------PlayerLoginByUserNamePassword----------{0}", clientId);
            PlayerLog.WriteLog((int)LogType.LoginLog, "--LoginEx---clientId:{0}------Ok", clientId);
            playerController.DbData.LoginDay++;
            playerController.DbData.LoginTotal++;

            try
            {
                if (msg.RouterInfo.Count > 0)
                {
                    playerController.RemoteIpAddress = IntToIp((long)msg.RouterInfo[0]);
                }
            }
            catch
            {
                // 获取客户端IP失败，也没什么。
            }

            //playerController.SaveLoginTime();
            if (msg is PlayerLoginByUserNamePasswordInMessage)
            {
                var message = msg as PlayerLoginByUserNamePasswordInMessage;
                message.Response.LastServerId = playerController.DbData.LastServerId;
            }
            else if (msg is PlayerLoginByThirdKeyInMessage)
            {
                var message = msg as PlayerLoginByThirdKeyInMessage;
                message.Response.LastServerId = playerController.DbData.LastServerId;
                message.Response.Uid = uid;
            }
            playerController.DbData.moniterData = moniterData == null ? new MsgChatMoniterData() { ip = "127.0.0.1", channel = "test", uid = "123" } : moniterData;
            NotifyGateClientState(__this, clientId, 0, GateClientState.Login);
            msg.Reply();

            Logger.Info("[" + userName + "]" + " has logined!");
            Logger.Info("Enter Game {0} time:{1} Login - 5 - {2}", userName, s.ElapsedMilliseconds,
                TimeManager.Timer.ElapsedMilliseconds);


        }

        //登出
        public IEnumerator LoginoutEx(Coroutine coroutine, LoginServerControl __this, ExitLoginInMessage msg)
        {
            var clientId = msg.ClientId;
            var player = __this.PlayerManager.GetPlayerController(clientId);
            if (player == null)
            {
                Logger.Error("LoginoutEx clientId={0}......player == null", clientId);

                QueueManager.ClinetIdLost(clientId);
                yield break;
            }
            var connect = player.Connect;
            var db = player.DbData;
            if (connect != null)
            {
                var state = connect.State;

                PlayerLog.WriteLog((int)LogType.LoginLog, "--LoginoutEx---clientId:{0}---state:{1}----",
                    clientId, state);
                switch (state)
                {
                    case ConnectState.Wait:
                    case ConnectState.Landing:
                        {
                            NotifyGateClientState(__this, clientId, 0, GateClientState.NotAuthorized);
                            //清理玩家账号数据
                            var co = CoroutineFactory.NewSubroutine(PlayerLogout, coroutine, __this, clientId);
                            if (co.MoveNext())
                            {
                                yield return co;
                            }
                        }
                        break;
                    case ConnectState.EnterGame:
                    case ConnectState.InGame:
                        {

                            //如果是从游戏中退出，同步等级和转生数据到login
                            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(db.SelectChar);
                            if (character != null)
                            {
                                var subCo = CoroutineFactory.NewSubroutine(character.SyncCharacterLevelData, coroutine);
                                if (subCo.MoveNext())
                                {
                                    yield return subCo;
                                }

                            }

                            CleanCharacterData(clientId, db.SelectChar);

                            //返回登录
                            NotifyGateClientState(__this, clientId, 0, GateClientState.NotAuthorized);
                            //清理玩家账号数据
                            var co = CoroutineFactory.NewSubroutine(PlayerLogout, coroutine, __this, clientId);
                            if (co.MoveNext())
                            {
                                yield return co;
                            }
                        }
                        break;
                    case ConnectState.OffLine:
                        break;
                    case ConnectState.WaitOffLine:
                        break;
                }
            }
            else
            {
                Logger.Error("LoginoutEx clientId={0}......player.Connect == null", clientId);
            }
        }

        #region connet所有服务器

        private IEnumerator NotifyCharacterOnConne(Coroutine coroutine,
                                                   ulong characterId,
                                                   ulong clientId,
                                                   AsyncReturnValue<int> status)
        {
            PlayerLog.WriteLog(characterId, "----------Login----------NotifyCharacterOnConne----------{0},{1}", clientId,
                characterId);
            status.Value = 0;
            var msg = new OutMessage[6];
            msg[0] = LoginServer.Instance.SceneAgent.SSNotifyCharacterOnConnet(characterId, clientId, characterId);
            msg[0].SendAndWaitUntilDone(coroutine);
            msg[1] = LoginServer.Instance.ChatAgent.SSNotifyCharacterOnConnet(characterId, clientId, characterId);
            msg[1].SendAndWaitUntilDone(coroutine);
            msg[2] = LoginServer.Instance.ActivityAgent.SSNotifyCharacterOnConnet(characterId, clientId, characterId);
            msg[2].SendAndWaitUntilDone(coroutine);
            msg[3] = LoginServer.Instance.LogicAgent.SSNotifyCharacterOnConnet(characterId, clientId, characterId);
            msg[3].SendAndWaitUntilDone(coroutine);
            msg[4] = LoginServer.Instance.RankAgent.SSNotifyCharacterOnConnet(characterId, clientId, characterId);
            msg[4].SendAndWaitUntilDone(coroutine);
            msg[5] = LoginServer.Instance.TeamAgent.SSNotifyCharacterOnConnet(characterId, clientId, characterId);
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
                    Logger.Error("CharacterOnConnet ..{1}..NotifyBroker....MessageState.....: {0} ", outMessage.State, i);
                    status.Value = 1;
                    yield break;
                }
                if (outMessage.ErrorCode != (int)ErrorCodes.OK)
                {
                    Logger.Error("CharacterOnConnet ..{1}..NotifyBroker....MessageState.....: {0},ErrorCode={2} ",
                        outMessage.State, i, outMessage.ErrorCode);
                    status.Value = 1;
                    yield break;
                }
            }
        }

        #endregion

        private IEnumerator NotifyCreateCharacter(Coroutine coroutine,
                                                  ulong characterId,
                                                  ulong clientId,
                                                  int type,
                                                  int serverId,
                                                  int sceneId,
            //ulong sceneGuid,
                                                  bool isGM,
                                                  AsyncReturnValue<int> status)
        {
            status.Value = 0;
            var msg1 = LoginServer.Instance.SceneAgent.CreateCharacter(characterId, type, isGM);
            yield return msg1.SendAndWaitUntilDone(coroutine);

            if (msg1.State == MessageState.Error)
            {
                Logger.Error("Login NotifyBrokerCreateCharacter Scene Create DB faild  ....MessageState.Error:{0}",
                    characterId);
                status.Value = (int)ErrorCodes.Error_LoginCreatePrepareDataScene;
                yield break;
            }
            if (msg1.State == MessageState.Reply)
            {
                if (msg1.ErrorCode != (int)ErrorCodes.OK)
                {
                    Logger.Error("Login NotifyBrokerCreateCharacter Scene Create DB faild,  {0},characterId:{1}",
                        msg1.ErrorCode, characterId);
                    status.Value = (int)ErrorCodes.Error_LoginCreatePrepareDataScene;
                    yield break;
                }
            }
            else if (msg1.State == MessageState.Timeout)
            {
                Logger.Error("Login NotifyBrokerCreateCharacter Scene Create DB faild  ....MessageState.Timeout,{0}",
                    characterId);
                status.Value = (int)ErrorCodes.Error_LoginCreatePrepareDataScene;
                yield break;
            }


            var msg4 = LoginServer.Instance.LogicAgent.CreateCharacter(characterId, type, serverId, isGM);
            yield return msg4.SendAndWaitUntilDone(coroutine);


            if (msg4.State == MessageState.Error)
            {
                Logger.Error("Login NotifyBrokerCreateCharacter Logic Create DB faild  ....MessageState.Error:{0}",
                    characterId);
                status.Value = (int)ErrorCodes.Error_LoginCreatePrepareDataLogic;
                yield break;
            }
            if (msg4.State == MessageState.Reply)
            {
                if (msg4.ErrorCode != (int)ErrorCodes.OK)
                {
                    Logger.Error("Login NotifyBrokerCreateCharacter Logic Create DB faild,  {0},characterId:{1}",
                        msg4.ErrorCode, characterId);
                    status.Value = (int)ErrorCodes.Error_LoginCreatePrepareDataLogic;
                    yield break;
                }
            }
            else if (msg4.State == MessageState.Timeout)
            {
                Logger.Error("Login NotifyBrokerCreateCharacter Logic Create DB faild  ....MessageState.Timeout:{0}",
                    characterId);
                status.Value = (int)ErrorCodes.Error_LoginCreatePrepareDataLogic;
                yield break;
            }


            var msg2 = LoginServer.Instance.ChatAgent.CreateCharacter(characterId, type);
            yield return msg2.SendAndWaitUntilDone(coroutine);
            if (msg2.State == MessageState.Error)
            {
                Logger.Error("Login NotifyBrokerCreateCharacter Logic Create DB faild  ....MessageState.Error,{0}",
                    characterId);
                status.Value = (int)ErrorCodes.Error_LoginCreatePrepareDataChat;
            }
            else if (msg2.State == MessageState.Reply)
            {
                if (msg2.ErrorCode != (int)ErrorCodes.OK)
                {
                    Logger.Error("Login NotifyBrokerCreateCharacter Logic Create DB faild,  {0} characterId:{1}",
                        msg4.ErrorCode, characterId);
                    status.Value = (int)ErrorCodes.Error_LoginCreatePrepareDataChat;
                }
            }
            else if (msg2.State == MessageState.Timeout)
            {
                Logger.Error("Login NotifyBrokerCreateCharacter Logic Create DB faild  ....MessageState.Timeout:{0}",
                    characterId);
                status.Value = (int)ErrorCodes.Error_LoginCreatePrepareDataChat;
            }
        }

        //通知Gate的这个Client的状态
        public static void NotifyGateClientState(LoginServerControl _this,
                                                 ulong clientId,
                                                 ulong characterId,
                                                 GateClientState clientState)
        {
            var gateDesc = new ServiceDesc();
            gateDesc.Type = 199;
            gateDesc.CharacterId = characterId;
            gateDesc.ServiceType = (int)ServiceType.Login;
            gateDesc.ClientId = clientId;
            gateDesc.FuncId = (uint)clientState;
            _this.Send(gateDesc);
        }

        //清除某个角色的数据
        public IEnumerator RemoveCharacterData(Coroutine coroutine, ulong characterId)
        {
            var removeCo = CoroutineFactory.NewSubroutine(CharacterManager.Instance.RemoveCharacter, coroutine,
                characterId);
            if (removeCo.MoveNext())
            {
                yield return removeCo;
            }
        }

        //Gate检测到用户掉线 gate - > login
        public IEnumerator GateDisconnect(Coroutine coroutine, LoginService _this, GateDisconnectInMessage msg)
        {
            var clientId = msg.ClientId;
            var __this = (LoginServerControl)_this;
            var playerController = __this.PlayerManager.GetPlayerController(clientId);
            if (playerController == null)
            {
                PlayerLog.WriteLog((int)LogType.LoginLog, "--GateDisconnect---clientId:{0}---playerController == null",
                    clientId);
                yield break;
            }
            if (playerController.Connect == null)
            {
                __this.PlayerManager.TryDelPlayer(clientId, out playerController);
                Logger.Error("GateDisconnect not find Connect playerId={0},character={1}",
                    playerController.DbData.Id, 0);
                yield break;
            }
            var state = playerController.Connect.State;
            var db = playerController.DbData;
            PlayerLog.WriteLog((int)LogType.LoginLog, "--GateDisconnect---clientId:{0}--state:{1}",
                clientId, state);
            switch (state)
            {
                case ConnectState.Wait:
                case ConnectState.Landing:
                    {
                        var co = CoroutineFactory.NewSubroutine(PlayerLogout, coroutine, __this, clientId);
                        if (co.MoveNext())
                        {
                            yield return co;
                        }
                        yield break;
                    }
                case ConnectState.EnterGame:
                    {
                        CleanClientData(__this, clientId, db.SelectChar, KickClientType.LostLine);
                        var co = CoroutineFactory.NewSubroutine(PlayerLogout, coroutine, __this, clientId);
                        if (co.MoveNext())
                        {
                            yield return co;
                        }
                        yield break;
                    }
                case ConnectState.InGame:
                    //游戏中断线把等级和转生同步给login
                    var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(db.SelectChar);
                    if (character != null)
                    {
                        playerController.Connect.State = ConnectState.WaitReConnet;
                        var co = CoroutineFactory.NewSubroutine(character.SyncCharacterLevelData, coroutine);
                        if (co.MoveNext())
                        {
                            yield return co;
                        }
                    }
                    break;
                case ConnectState.WaitReConnet:
                case ConnectState.OffLine:
                case ConnectState.WaitOffLine:
                    Logger.Error("GateDisconnect Player State={0}", state);
                    yield break;
            }
            playerController.Connect.State = ConnectState.WaitReConnet;
            PlayerLog.PlayerLogger(db.Id, "----------Login----------GateDisconnect----------{0}", clientId);
        }

        //客户端尝试重连
        public IEnumerator ReConnet(Coroutine coroutine, LoginService _this, ReConnetInMessage msg)
        {
            var newClientId = msg.ClientId;
            var characterId = msg.Request.CharacterId;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (character == null)
            {
                //已经不在线了，可能是超时重连了
                PlayerLog.WriteLog((int)LogType.LoginLog, "--ReConnet---clientId:{0}---character == null", newClientId);
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }

            var __this = (LoginServerControl)_this;
            var playerId = character.mDbData.PlayerId;
            var player = __this.PlayerManager.GetPlayerControllerByPlayerId(playerId); // (clientId);
            if (player == null)
            {
                //角色在线，玩家不在线，理论上不可能的
                Logger.Error("ReConnet player not find! player={0},character={1}", playerId, characterId);
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }
            if (player.Connect == null)
            {
                //玩家在线，排队无信息，理论上不可能的
                Logger.Error("ReConnet Connect not find! player={0},character={1}", playerId, characterId);
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }
            var state = player.Connect.State;
            if (state != ConnectState.WaitReConnet && state != ConnectState.InGame)
            {
                //说明其他设备已经登录了，此设备重连失效
                //ConnectState.InGame说明reConncet比DisConnect要快
                msg.Reply((int)ErrorCodes.PlayerEnterGamming);
                PlayerLog.WriteLog((int)LogType.LoginLog, "--ReConnet---clientId:{0}---state:{1}----state error",
                    newClientId, state);
                yield break;
            }

            //开始重连过程！
            var oldClineId = player.Connect.ClientId;
            if (oldClineId >> 48 != newClientId >> 48)
            {
                Logger.Error("ReConnet ------ 4---{0}----oldClineId:{1}----newClientId:{2}",
                    DateTime.Now.ToBinary(), oldClineId, newClientId);
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            PlayerLog.WriteLog((int)LogType.LoginLog, "--ReConnet---clientId:{0}---state:{1}----Ok--",
                newClientId, state);

            player.Connect.State = ConnectState.InGame;

            msg.Reply();

            //通知SS->Scene这个characterId在哪个服务器
            var msgChgScene = LoginServer.Instance.SceneAgent.SBReconnectNotifyScene(characterId, oldClineId,
                newClientId, characterId);
            yield return msgChgScene.SendAndWaitUntilDone(coroutine);
        }

        //清理Login （PLAYER CHARACTER)
        public IEnumerator PlayerLogout(Coroutine coroutine, LoginServerControl __this, ulong clientId)
        {
            //清理玩家账号数据
            PlayerController playerController = null;
            __this.PlayerManager.TryDelPlayer(clientId, out playerController);
            if (playerController == null)
            {
                Logger.Error("PlayerLogout clientId={0}......player == null", clientId);

                QueueManager.ClinetIdLost(clientId);
                yield break;
            }
            if (playerController.Connect == null)
            {
                Logger.Error("PlayerLogout clientId={0}......player.Connect == null", clientId);
                QueueManager.ClinetIdLost(clientId);
            }
            else
            {
                playerController.Connect.IsOnline = false;
                playerController.Connect.OnLost();
                // 如果玩家主动断开连接，这时，已经执行了RemoveCharacter
                if (playerController.Connect != null)
                {
                    var name = playerController.Connect.GetKey();
                    __this.PlayerManager.NameKey2Player.Remove(name);
                }
                else
                {
                    Logger.Warn("PlayerLogout playerController.Connect is nullclientid={0},playerId={1}", clientId,
                        playerController.DbData.Id);
                }
            }
            PlayerLog.WriteLog((int)LogType.LoginLog, "--LoginEx---PlayerLogout:{0}---Ok", clientId);

            var character =
                CharacterManager.Instance.GetCharacterControllerFromMemroy(playerController.DbData.SelectChar);
            if (character != null)
            {
                character.LostLine();
                CharacterManager.PopServerPlayer(character.mDbData.ServerId); //玩家角色登出
                var co = CoroutineFactory.NewSubroutine(RemoveCharacterData, coroutine,
                    playerController.DbData.SelectChar);
                if (co.MoveNext())
                {
                    yield return co;
                }
            }
            //__this.PlayerName2ClientId.Remove(playerController.DbData.Name);

            try
            {
                long totleOnlineTime = 0;
                if (character != null)
                {
                    totleOnlineTime = character.mDbData.TotleOlineTime;
                }
                // 先insert
                string playerLog = string.Format("player#{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}",
                    playerController.DbData.Id,  // playerid
                    DateTime.FromBinary(playerController.DbData.FoundTime).ToString("yyyy/MM/dd HH:mm:ss"), // createtime
                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), // lasttime
                    playerController.DbData.SelectChar,                   // lastcharacter
                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), // characterlasttime
                    totleOnlineTime,      // onlinecount
                    playerController.DbData.LastServerId, // serverid
                    playerController.Platform, // createplatform
                    playerController.LoginChannel    // spid
                );
                kafaLogger.Info(playerLog);

                UpdateUserLog(playerController, character);
            }
            catch (Exception)
            {
            }
        }

        //清理Login （回退登陆到Login状态 CHARACTER)
        public IEnumerator PlayerLogoutEx(Coroutine coroutine, LoginServerControl __this, ulong clientId)
        {
            //清理玩家账号数据
            var playerController = __this.PlayerManager.GetPlayerController(clientId);
            if (playerController == null)
            {
                QueueManager.ClinetIdLost(clientId);
                Logger.Warn("PlayerLogoutEx not find clientid={0}", clientId);
                yield break;
            }
            var playerId = playerController.DbData.Id;
            //
            if (playerController.Connect == null)
            {
                Logger.Warn("PlayerLogoutEx Controller.Connect == null clientid={0}", clientId);
                QueueManager.ClinetIdLost(clientId);
            }
            else
            {
                switch (playerController.Connect.State)
                {
                    case ConnectState.InGame:
                        QueueManager.InGamePlayerList.Remove(playerId);
                        QueueManager.LandingPlayerList.TryAdd(playerId, playerController.Connect);
                        QueueManager.TotalList.TryAdd(clientId, playerController.Connect);
                        playerController.Connect.State = ConnectState.Landing;
                        break;
                    default:
                        Logger.Warn("PlayerLogoutEx State Error !  clientid={0},playerId={1},state={2}", clientId,
                            playerId, playerController.Connect.State);
                        break;
                }
            }
            PlayerLog.WriteLog((int)LogType.LoginLog, "--PlayerLogoutEx---PlayerLogout:{0}---Ok", clientId);

            var character =
                CharacterManager.Instance.GetCharacterControllerFromMemroy(playerController.DbData.SelectChar);
            if (character != null)
            {
                character.LostLine();
                CharacterManager.PopServerPlayer(character.mDbData.ServerId); //玩家角色登出
                var co = CoroutineFactory.NewSubroutine(RemoveCharacterData, coroutine,
                    playerController.DbData.SelectChar);
                if (co.MoveNext())
                {
                    yield return co;
                }
                UpdateUserLog(playerController, character);
            }
        }


        public IEnumerator GetAnchorIsInRoom(Coroutine coroutine, LoginService _this, GetAnchorIsInRoomInMessage msg)
        {
            var msg1 = LoginServer.Instance.ChatAgent.GetAnchorIsInRoom(msg.Request.CharacterId,0);
            yield return msg1.SendAndWaitUntilDone(coroutine);
            if (msg1.State == MessageState.Reply)
            {
                msg.Response = msg1.Response;
                msg.Reply(msg1.ErrorCode);
            }
           // msg.Response = AnchorManager.IsInAnchorRooml;
            yield break;
        }

        //EnterGame
        public IEnumerator EnterGame(Coroutine coroutine, LoginService _this, EnterGameInMessage msg)
        {
            var __this = (LoginServerControl)_this;
            //检查ClientId登陆权限
            var clientId = msg.ClientId;
            var playerController = __this.PlayerManager.GetPlayerController(clientId);
            if (playerController == null)
            {
                msg.Reply((int)ErrorCodes.ClientIdNoPower);
                yield break;
            }
            var playerId = playerController.DbData.Id;
            var foundTime = playerController.DbData.FoundTime;
            if (playerController.Connect == null)
            {
                Logger.Error("EnterGameEx LoginStateError client={0},player={1} ", clientId, playerId);
                msg.Reply((int)ErrorCodes.LoginStateError);
                yield break;
            }
            //登陆状态不符
            if (playerController.Connect.State != ConnectState.Landing)
            {
                Logger.Error("EnterGameEx LoginStateError client={0},player={1},state={2} ", clientId, playerId,
                    playerController.Connect.State);
                msg.Reply((int)ErrorCodes.LoginStateError);
                yield break;
            }
            //查询选择的服务器下是否有角色
            Uint64Array characters;
            var characterId = msg.Request.CharacterId;
            var selectServerId = playerController.DbData.LastServerId;
            if (!playerController.DbData.ServersPlayers.TryGetValue(selectServerId, out characters))
            {
                Logger.Error("EnterGameEx Can not find server client={0},player={1},ServerId={2} characterId={3}",
                    clientId, playerId, selectServerId, characterId);
                msg.Reply((int)ErrorCodes.SelectServerNoCharacter);
                yield break;
            }
            //查询要登陆的Character是否在选择的服务器下
            if (!characters.Items.Contains(characterId))
            {
                Logger.Error("EnterGameEx Can not find character client={0},player={1},ServerId={2} characterId={3}",
                    clientId, playerId, selectServerId, characterId);
                msg.Reply((int)ErrorCodes.SelectServerNoThisCharacter);
                yield break;
            }

            //服务器级别的排队
            //             var queue = QueueManager.GetQueue(selectServerId);
            //             if (queue == null)
            //             {
            //                 Logger.Error("EnterGameEx Can not find queue client={0},player={1},ServerId={2} characterId={3}", clientId, playerId, selectServerId, characterId);
            //                 msg.Reply((int)ErrorCodes.Unknow);
            //                 yield break;
            //             }

            //             var check = queue.PushConnect("", clientId, characterId.ToString(), playerController);
            //             if (check != PlayerQueueType.NoWait)
            //             {
            //                 PlayerLog.WriteLog((int)LogType.Error_PLayerLoginWait, "EnterGame character id = {0}, state={1}", characterId, check);
            //                 msg.Reply((int)ErrorCodes.Error_EnterGameWait);
            //                 yield break;
            //             }

            //账号状态进入开始游戏状态
            QueueManager.PlayerEnterGameStart(playerId);
            //加载角色数据
            var characterRet = AsyncReturnValue<CharacterController>.Create();
            var loginCo = CoroutineFactory.NewSubroutine(CharacterManager.Instance.GetOrCreateCharacterController,
                coroutine, characterId, new object[] { }, false, characterRet);
            if (loginCo.MoveNext())
            {
                yield return loginCo;
            }
            var character = characterRet.Value;
            characterRet.Dispose();
            if (character == null)
            {
                Logger.Error("EnterGameEx LoadCharacter Error characterId = {0} null", characterId);
                msg.Reply((int)ErrorCodes.Error_CharacterNotFind);
                yield break;
            }
            //加载完角色后的状态判断
            var playerState = PlayerManager.GetImpl().GetPlayerState(__this.PlayerManager, playerId);
            if (playerState != ConnectState.EnterGame || clientId != playerController.Connect.ClientId)
            {
                //如果玩家在这个过程中被T，有可能会不是之前的EnterGame状态
                var removeCo = CoroutineFactory.NewSubroutine(RemoveCharacterData, coroutine, characterId);
                if (removeCo.MoveNext())
                {
                    yield return removeCo;
                }

                Logger.Error("EnterGameEx LoginStateError 1  client={0},playerClient={3},player={1},state={2},  ",
                    clientId, playerId, playerController.Connect.State, playerController.Connect.ClientId);
                msg.Reply((int)ErrorCodes.LoginStateError);
                yield break;
            }
            CharacterManager.PushServerPlayer(character.mDbData.ServerId);
            //整理角色在线时间
            character.LostLine(true);
            character.OnLine();
            //拿到Scene的Db参数 ，为了尽量能一次性的分配对玩家的Scene服务器 和 SceneId
            var sceneId = 0;
            ulong sceneGuid = 0;
            var dbData = LoginServer.Instance.DB.Get<DBCharacterScene>(coroutine, DataCategory.SceneCharacter,
                characterId);
            yield return dbData;

            //加载完角色后的状态判断
            playerState = PlayerManager.GetImpl().GetPlayerState(__this.PlayerManager, playerId);
            if (playerState != ConnectState.EnterGame || clientId != playerController.Connect.ClientId)
            {
                //如果玩家在这个过程中被T，有可能会不是之前的EnterGame状态
                character.LostLine();
                CharacterManager.PopServerPlayer(character.mDbData.ServerId); //登陆失败
                var removeCo = CoroutineFactory.NewSubroutine(RemoveCharacterData, coroutine, characterId);
                if (removeCo.MoveNext())
                {
                    yield return removeCo;
                }
                Logger.Error("EnterGameEx LoginStateError 2  client={0},playerClient={3},player={1},state={2},  ",
                    clientId, playerId, playerController.Connect.State, playerController.Connect.ClientId);
                msg.Reply((int)ErrorCodes.LoginStateError);
                yield break;
            }
            //整理场景数据
            if (dbData.Data != null)
            {
                sceneId = (int)dbData.Data.SceneId;
                sceneGuid = dbData.Data.SceneGuid;
                var isneed = false;
                sceneId = SceneExtension.GetWillScene(sceneId, (int)dbData.Data.FormerSceneId, dbData.Data.Hp,
                    DateTime.FromBinary(dbData.Data.AutoRelive), DateTime.FromBinary(character.GetData().LoginOut),
                    ref sceneGuid, ref isneed);
            }
            else
            {
                sceneId = int.Parse(Table.GetServerConfig(0).Value);
                sceneGuid = 0;
            }
            ulong newSceneGuid = 0;
            //通知各Broker，次玩家要进入
            {
                var status = AsyncReturnValue<int>.Create();
                var sceneData = AsyncReturnValue<ulong>.Create();
                var co = CoroutineFactory.NewSubroutine(NotifyBrokerPrepareData, coroutine, characterId, clientId,
                    selectServerId, sceneId, sceneGuid, status, sceneData);
                if (co.MoveNext())
                {
                    yield return co;
                }
                var prepareDataResult = status.Value;
                newSceneGuid = sceneData.Value;
                status.Dispose();
                sceneData.Dispose();
                if (prepareDataResult != 0)
                {
                    CleanCharacterData(clientId, characterId);
                    msg.Reply((int)ErrorCodes.Error_PrepareEnterGameFailed);
                    character.State = CharacterState.LoadData;
                    character.LostLine();
                    CharacterManager.PopServerPlayer(character.mDbData.ServerId); //登陆失败
                    var removeCo = CoroutineFactory.NewSubroutine(RemoveCharacterData, coroutine, characterId);
                    if (removeCo.MoveNext())
                    {
                        yield return removeCo;
                    }
                    PlayerLog.WriteLog((int)LogType.LoginLog, "--EnterGameEx---clientId:{0}--prepareDataResult error-",
                        clientId);
                    yield break;
                }
            }
            if (newSceneGuid == 0)
            {
                CharacterManager.PopServerPlayer(character.mDbData.ServerId); //登陆失败
                var removeCo = CoroutineFactory.NewSubroutine(RemoveCharacterData, coroutine, characterId);
                if (removeCo.MoveNext())
                {
                    yield return removeCo;
                }
                PlayerLog.WriteLog((int)LogType.LoginLog, "--EnterGameEx---clientId:{0}--newSceneGuid == 0", clientId);
                msg.Reply((int)ErrorCodes.Error_PrepareEnterGameFailed);
                yield break;
            }
            //加载完角色后的状态判断
            playerState = PlayerManager.GetImpl().GetPlayerState(__this.PlayerManager, playerId);
            if (playerState != ConnectState.EnterGame || clientId != playerController.Connect.ClientId)
            {
                CleanCharacterData(clientId, characterId);
                //如果玩家在这个过程中被T，有可能会不是之前的EnterGame状态
                character.LostLine();
                CharacterManager.PopServerPlayer(character.mDbData.ServerId); //登陆失败
                var removeCo = CoroutineFactory.NewSubroutine(RemoveCharacterData, coroutine, characterId);
                if (removeCo.MoveNext())
                {
                    yield return removeCo;
                }
                Logger.Error("EnterGameEx LoginStateError 3 client={0},playerClient={3},player={1},state={2},  ",
                    clientId, playerId, playerController.Connect.State, playerController.Connect.ClientId);
                msg.Reply((int)ErrorCodes.LoginStateError);
                yield break;
            }
            //Onconnet
            {
                var status = AsyncReturnValue<int>.Create();
                var co = CoroutineFactory.NewSubroutine(NotifyCharacterOnConne, coroutine, characterId, clientId, status);
                if (co.MoveNext())
                {
                    yield return co;
                }
                var prepareDataResult = status.Value;
                status.Dispose();
                if (prepareDataResult != 0)
                {
                    character.LostLine();
                    CharacterManager.PopServerPlayer(character.mDbData.ServerId); //登陆失败
                    var removeCo = CoroutineFactory.NewSubroutine(RemoveCharacterData, coroutine, characterId);
                    if (removeCo.MoveNext())
                    {
                        yield return removeCo;
                    }
                    CleanCharacterData(clientId, characterId);
                    msg.Reply((int)ErrorCodes.Error_PrepareEnterGameFailed);

                    PlayerLog.WriteLog((int)LogType.LoginLog,
                        "--EnterGameEx---clientId:{0}--ErrorCodes.Error_PrepareEnterGameFailed", clientId);

                    character.State = CharacterState.LoadData;
                    yield break;
                }
            }

            QueueManager.PlayerEnterGameSuccess(playerId);
            NotifyGateClientState(__this, clientId, characterId, GateClientState.GamePlay);
            //character.NotifyEnterScene();

            //首服开服ID
            //  var config = Table.GetServerConfig(150);
            //  var serverInfo = Table.GetServerName(int.Parse(config.Value));
            //  DateTime ServerOpenDate = DateTime.Parse(serverInfo.OpenTime);
            //   var diffTime = DateTime.Now - ServerOpenDate;
            //  if (character.FirstLanding && diffTime.TotalSeconds < 15 * 24 * 3600)//开服15天
            if (character.FirstLanding)
            {
                character.FirstOnlineMsg(clientId);
            }
            msg.Response.Continuedays = character.GetContinueday();
            msg.Response.ServerId = selectServerId;

            //客户端转换成utc了，但是暂时不能更新客户端，所以发给客户端时候额外减少8小时让客户端正确
            var fixedTime = DateTime.FromBinary(character.mDbData.FoundTime) - TimeSpan.FromHours(8);
            var fixedTimelong = fixedTime.ToBinary();
            msg.Response.FoundTime = fixedTimelong;
            // msg.Response.FoundTime = character.mDbData.FoundTime;

            msg.Reply();
            //写账号数据
            if (characterId != playerController.DbData.SelectChar)
            {
                playerController.DbData.SelectChar = characterId;
                playerController.DbData.LastServerId = selectServerId;
                var returnValue = AsyncReturnValue<int>.Create();
                var co = CoroutineFactory.NewSubroutine(playerController.SaveDb, coroutine, returnValue);
                if (co.MoveNext())
                {
                    yield return co;
                }
                returnValue.Dispose();
            }

            PlayerLog.WriteLog((int)LogType.LoginLog,
                "--EnterGameEx---clientId:{0}--characterId:{1}--playerId -- {2}----Ok",
                clientId, characterId, playerController.DbData.Id);

            //PlayerLog.StatisticsLogger("eg,{0},{1}", characterId, playerController.DbData.Id);
            PlayerLog.BackDataLogger((int)BackDataType.CharacterOnline, "{0}|{1}", playerController.DbData.Id,
                characterId);

            try
            {
                string v = string.Format("characters#{0}|{1}|{2}|{3}|{4}|{5}",
                            characterId,
                            playerController.DbData.Id,
                            DateTime.FromBinary(character.GetData().FoundTime).ToString("yyyy/MM/dd HH:mm:ss"),
                            DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                            playerController.DbData.LastServerId,
                            character.GetData().TypeId);
                kafaLogger.Info(v);

                string log = string.Format("gameloginlog#{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}",
                    playerController.DbData.LastServerId,  // 服务器id
                    playerController.DbData.Name,   // 账号
                    playerController.LoginChannel,   // 运营商id
                    character.GetData().Name,   // 角色名
                    playerController.Platform, // 平台（ios Android)
                    playerController.RemoteIpAddress, // ip地址
                    "1.0", // version
                    1,
                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")); // 时间
                kafaLogger.Info(log);

                //                 string vv2 = string.Format("adminplayerlogintime#{0}|{1}|{2}|{3}|{4}|{5}",
                //                     playerController.DbData.LastServerId,
                //                     playerController.DbData.Id,
                //                     characterId,
                //                     DateTime.FromBinary(character.GetData().LoginIn).ToString("yyyy/MM/dd HH:mm:ss"),// login time
                //                     DateTime.Now.Date.ToString("yyyy/MM/dd HH:mm:ss"),  // logout time
                //                     1);
                //                 kafaLogger.Info(vv2);

                UpdateUserLog(playerController, character);
            }
            catch (Exception e)
            {
                Logger.Error(e, "kafaLogger gameloginlog error.");
            }
            //防止出现data还没传给聊天和logic服务器 玩家已经进入游戏
            //同步给聊天服  玩家的基本信息 LoginServer.Instance.ChatAgent
            var msg3 = LoginServer.Instance.ChatAgent.NotifyPlayerEnterGame(characterId, playerController.DbData.moniterData);
            yield return msg3.SendAndWaitUntilDone(coroutine);
            var msgLogic = LoginServer.Instance.LogicAgent.NotifyPlayerMoniterData(characterId, playerController.DbData.moniterData);
            yield return msgLogic.SendAndWaitUntilDone(coroutine);
            

            var enterScene = LoginServer.Instance.SceneAgent.SSEnterScene(characterId, characterId, newSceneGuid,
                sceneGuid,
                (int)eScnenChangeType.Login, new SceneParam());
            yield return enterScene.SendAndWaitUntilDone(coroutine);
        }

        public void UpdateUserLog(PlayerController playerController, CharacterController character)
        {
            if (playerController == null || character == null)
                return;

            try
            {
                var logintime = DateTime.FromBinary(character.mDbData.LoginIn);
                var logouttime = DateTime.FromBinary(character.mDbData.LoginOut);

                var gameuserlog = string.Format("gameusers#{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}",
                      character.GetData().Id, // charid
                      playerController.DbData.Name, //acount
                      character.GetData().ServerId, // serverid
                      DateTime.FromBinary(character.GetData().FoundTime).ToString("yyyy/MM/dd HH:mm:ss"),   // createtime
                      playerController.Platform,// createplatform
                      playerController.LoginChannel,   // createspid
                      logintime,   // logintime
                      logouttime,   // logouttime
                      playerController.Platform, // loginplatform
                      character.GetLoginDays() // dayback
                  );

                kafaLogger.Info(gameuserlog);
            }
            catch (Exception e)
            {
                Logger.Error(e, "kafaLogger gameusers_up error.");
            }
        }

        //获取设备ID
        public IEnumerator SendDeviceUdid(Coroutine coroutine, LoginService _this, SendDeviceUdidInMessage msg)
        {
            PlayerLog.BackDataLogger((int)BackDataType.NewDevice, "{0}", msg.Request.DeviceUdid);
            yield break;
        }

        public IEnumerator CreateCharacter(Coroutine coroutine, LoginService _this, CreateCharacterInMessage msg)
        {
            var __this = (LoginServerControl)_this;
            var clientId = msg.ClientId;
            var name = msg.Request.Name;
            var serverId = msg.Request.ServerId;
            var type = msg.Request.Type;
            //检查server id的有效性
            if (serverId < 0)
            {
                msg.Reply((int)ErrorCodes.ServerID);
                yield break;
            }
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
            PlayerLog.PlayerLogger(playerController.DbData.Id, "----------Login----------CreateCharacter----------{0}",
                clientId);
            Uint64Array characters;
            if (playerController.DbData.ServersPlayers.TryGetValue(serverId, out characters))
            {
                if (characters.Items.Count >= 4)
                {
                    msg.Reply((int)ErrorCodes.CharacterFull);
                    yield break;
                }
            }
            var asyValue = AsyncReturnValue<int, ulong>.Create();
            var co = CoroutineFactory.NewSubroutine(__this.PlayerManager.CreateCharacter, coroutine, clientId, serverId,
                type, name,
                asyValue);
            if (co.MoveNext())
            {
                yield return co;
            }

            Logger.Info("Login CreateCharacter {0}", asyValue.Value1);

            var asyValue1 = asyValue.Value1;
            var characterId = asyValue.Value2;
            asyValue.Dispose();
            if (asyValue1 != 0)
            {
                msg.Reply(asyValue1);
                yield break;
            }

            var sceneId = 0;
            //ulong sceneGuid = 0;

            var characterControl = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (characterControl == null)
            {
                Logger.Error("CreateCharacter Error characterId = {0} null", characterId);
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }

            if (type < 0 || type > 2)
            {
                Logger.Error("CreateCharacter Error characterId = {0}, bad type id {1}", characterId, type);
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            sceneId = Table.GetActor(type).BirthScene;
            //sceneGuid = 0;

            var status = AsyncReturnValue<int>.Create();
            bool isGM = ISGMAccount(playerController.DbData.Name);
            var anotherCo = CoroutineFactory.NewSubroutine(NotifyCreateCharacter, coroutine, characterId, clientId,
                type, serverId, sceneId/*, sceneGuid*/, isGM, status);
            if (anotherCo.MoveNext())
            {
                yield return anotherCo;
            }

            if (status.Value != 0)
            {
                if (status.Value == (int)ErrorCodes.Error_LoginCreatePrepareDataScene)
                {
                }
                else if (status.Value == (int)ErrorCodes.Error_LoginCreatePrepareDataLogic)
                {
                    var msg1 = LoginServer.Instance.SceneAgent.DelectCharacter(characterId, type);
                    yield return msg1.SendAndWaitUntilDone(coroutine);
                }
                else if (status.Value == (int)ErrorCodes.Error_LoginCreatePrepareDataChat)
                {
                    var msg1 = LoginServer.Instance.SceneAgent.DelectCharacter(characterId, type);
                    yield return msg1.SendAndWaitUntilDone(coroutine);
                    var msg2 = LoginServer.Instance.LogicAgent.DelectCharacter(characterId, type);
                    yield return msg2.SendAndWaitUntilDone(coroutine);
                }

                Logger.Error("Login CreateCharacter->NotifyBrokerCreateCharacter Faild,  {0}", status.Value);
                msg.Reply(status.Value);
                yield break;
            }
            status.Dispose();

            if (playerController.DbData.ServersPlayers.TryGetValue(serverId, out characters))
            {
                foreach (var id in characters.Items)
                {
                    var dbLogicSimple = LoginServer.Instance.LogicAgent.GetLogicSimpleData(id, 0);
                    yield return dbLogicSimple.SendAndWaitUntilDone(coroutine);
                    if (dbLogicSimple.State != MessageState.Reply)
                    {
                        continue;
                    }
                    if (dbLogicSimple.ErrorCode != 0)
                    {
                        Logger.Error("In CreateCharacter().dbLogicSimple.ErrorCode = {0}, id = {1}",
                            dbLogicSimple.ErrorCode, id);
                        continue;
                    }
                    var dbLoginSimple = LoginServer.Instance.LoginAgent.GetLoginSimpleData(clientId, id);
                    yield return dbLoginSimple.SendAndWaitUntilDone(coroutine);
                    if (dbLoginSimple.State != MessageState.Reply)
                    {
                        continue;
                    }
                    if (dbLoginSimple.ErrorCode != 0)
                    {
                        Logger.Error("In CreateCharacter().dbLoginSimple.ErrorCode = {0}, id = {1}",
                            dbLoginSimple.ErrorCode, id);
                        continue;
                    }
                    var info = new CharacterSimpleInfo
                    {
                        CharacterId = id,
                        Name = dbLoginSimple.Response.Name,
                        RoleId = dbLoginSimple.Response.TypeId,
                        Level = dbLogicSimple.Response.Level,
                        Ladder = dbLogicSimple.Response.Ladder
                    };
                    info.EquipsModel.AddRange(dbLogicSimple.Response.EquipsModel);
                    msg.Response.Info.Add(info);
                }
            }

            if (msg.Response.Info.Count > 0)
            {
                msg.Response.SelectId = msg.Response.Info[msg.Response.Info.Count - 1].CharacterId;
            }


            //Save Login Character DB
            var returnValue = AsyncReturnValue<int>.Create();
            co = CoroutineFactory.NewSubroutine(playerController.SaveDb, coroutine, returnValue);
            if (co.MoveNext())
            {
                yield return co;
            }

            returnValue.Dispose();
            if (returnValue.Value != 0)
            {
                msg.Reply((int)ErrorCodes.Error_Login_DBCreate);
                yield break;
            }

            //客户端转换成utc了，但是暂时不能更新客户端，所以发给客户端时候额外减少8小时让客户端正确
            var fixedTime = DateTime.FromBinary(characterControl.mDbData.FoundTime) - TimeSpan.FromHours(8);
            var fixedTimelong = fixedTime.ToBinary();
            msg.Response.CharacterFoundTime = fixedTimelong; //playerController.DbData.FoundTime;
            msg.Reply();
            Logger.Info("[" + name + "]" + " Create OK!");
        }

        public IEnumerator GetServerCharacterCount(Coroutine coroutine,
                                                   LoginService _this,
                                                   GetServerCharacterCountInMessage msg)
        {
            CharacterManager.AddtoRange(msg.Response.Data);
            //msg.Response.Data.AddRange(CharacterManager.ServerCount);
            msg.Reply();
            yield return null;
        }

        public IEnumerator LockAccount(Coroutine co, LoginService _this, LockAccountInMessage msg)
        {
            var __this = (LoginServerControl)_this;
            var playerId = msg.Request.PlayerId;
            var endTime = msg.Request.EndTime;
            DBPlayerLogin playerDb = null;
            var player = __this.PlayerManager.GetPlayerControllerByPlayerId(playerId);
            if (player != null)
            {
                playerDb = player.DbData;
                if (player.Connect != null)
                {
                    player.Kick(player.Connect.ClientId, KickClientType.GmKick);
                }
            }
            if (null == playerDb)
            {
                var dbPlayer = LoginServer.Instance.DB.Get<DBPlayerLogin>(co, DataCategory.LoginPlayer, playerId);
                yield return dbPlayer;
                playerDb = dbPlayer.Data;
            }
            if (null == playerDb)
            {
                // 错误的id会来到这里
                msg.Reply((int)ErrorCodes.Error_NoAccount);
                yield break;
            }
            playerDb.LockTime = endTime;
            var dbSave = LoginServer.Instance.DB.Set(co, DataCategory.LoginPlayer, playerId, playerDb);
            yield return dbSave;
            msg.Reply();
        }

        public IEnumerator GetUserId(Coroutine coroutine, LoginService _this, GetUserIdInMessage msg)
        {
            var __this = (LoginServerControl)_this;
            var player = __this.PlayerManager.GetPlayerController(msg.Request.ClientId);
            if (player != null)
            {
                msg.Response = player.DbData.Name;
            }
            msg.Reply();
            yield return null;
        }

        #region 踢某个玩家（ClientId,CharacterId)

        //踢Gate的这个Client
        public static void NotifyGateClientLost(LoginServerControl _this,
                                                ulong clientId,
                                                KickClientType result = KickClientType.OtherLogin)
        {
            var gateDesc = new ServiceDesc();
            gateDesc.Type = 198;
            gateDesc.CharacterId = 0;
            gateDesc.ServiceType = (int)ServiceType.Login;
            gateDesc.ClientId = clientId;
            gateDesc.FuncId = (uint)result;

            _this.Send(gateDesc);
        }

        //踢这个Client链接的所有数据
        public void CleanClientData(LoginServerControl _this,
                                    ulong clientId,
                                    ulong characterId,
                                    KickClientType result = KickClientType.OtherLogin)
        {
            NotifyGateClientLost(_this, clientId, result);
            CleanCharacterData(clientId, characterId);
        }

        //清除某个角色的相关内容
        public static void CleanCharacterData(ulong clientId, ulong characterId)
        {
            PlayerLog.WriteLog(10004, "CleanCharacterData ClientId={0},characterId={1}", clientId, characterId);
            CoroutineFactory.NewCoroutine(SceneCleanClientData, clientId, characterId).MoveNext();
            CoroutineFactory.NewCoroutine(LogicCleanClientData, clientId, characterId).MoveNext();
            CoroutineFactory.NewCoroutine(RankCleanClientData, clientId, characterId).MoveNext();
            CoroutineFactory.NewCoroutine(TeamCleanClientData, clientId, characterId).MoveNext();
            CoroutineFactory.NewCoroutine(ChatCleanClientData, clientId, characterId).MoveNext();
            CoroutineFactory.NewCoroutine(ActivityCleanClientData, clientId, characterId).MoveNext();
        }


        //清除SceneBroker及scene的 此ClientId和CharacterId的数据
        private static IEnumerator SceneCleanClientData(Coroutine co, ulong clientId, ulong characterId)
        {
            var msgChgScene = LoginServer.Instance.SceneAgent.SBCleanClientCharacterData(characterId, clientId,
                characterId);
            yield return msgChgScene.SendAndWaitUntilDone(co);
        }

        //清除LogicBroker及Logic的 此ClientId和CharacterId的数据
        private static IEnumerator LogicCleanClientData(Coroutine co, ulong clientId, ulong characterId)
        {
            var msgChgScene = LoginServer.Instance.LogicAgent.SBCleanClientCharacterData(characterId, clientId,
                characterId);
            yield return msgChgScene.SendAndWaitUntilDone(co);
        }

        //清除RankBroker及Rank的 此ClientId和CharacterId的数据
        private static IEnumerator RankCleanClientData(Coroutine co, ulong clientId, ulong characterId)
        {
            var msgChgScene = LoginServer.Instance.RankAgent.SBCleanClientCharacterData(characterId, clientId,
                characterId);
            yield return msgChgScene.SendAndWaitUntilDone(co);
        }

        //清除 TeamBroker及 Team的 此ClientId和CharacterId的数据
        private static IEnumerator TeamCleanClientData(Coroutine co, ulong clientId, ulong characterId)
        {
            var msgChgScene = LoginServer.Instance.TeamAgent.SBCleanClientCharacterData(characterId, clientId,
                characterId);
            yield return msgChgScene.SendAndWaitUntilDone(co);
        }

        //清除ChatBroker及Chat的 此ClientId和CharacterId的数据
        private static IEnumerator ChatCleanClientData(Coroutine co, ulong clientId, ulong characterId)
        {
            var msgChgScene = LoginServer.Instance.ChatAgent.SBCleanClientCharacterData(characterId, clientId,
                characterId);
            yield return msgChgScene.SendAndWaitUntilDone(co);
        }

        //清除ActivityBroker及Activity的 此ClientId和CharacterId的数据
        private static IEnumerator ActivityCleanClientData(Coroutine co, ulong clientId, ulong characterId)
        {
            var msgChgScene = LoginServer.Instance.ActivityAgent.SBCleanClientCharacterData(characterId, clientId,
                characterId);
            yield return msgChgScene.SendAndWaitUntilDone(co);
        }

        private static bool ISGMAccount(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }


            return LoginServer.Instance.s_SpecialAccount.GMAccounts.ContainsKey(name);

        }

        //请求服务器列表总所有角色信息
        public IEnumerator GetAllCharactersLoginInfo(Coroutine coroutine, LoginService _this,
            GetAllCharactersLoginInfoInMessage msg)
        {
            if (msg.Request.ServerId != -1)
            {
                msg.Reply((int)ErrorCodes.ParamError);
            }

            var __this = (LoginServerControl)_this;
            var playerController = __this.PlayerManager.GetPlayerController(msg.ClientId);
            if (playerController == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }

            var allServer = playerController.DbData.ServersPlayers;


            var counter = 0;
            foreach (var oneServer in allServer)
            {
                var characters = oneServer.Value.Items.ToList();
                foreach (var characterId in characters)
                {
                    DBCharacterLogin dbLogin;
                    var controller = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
                    if (controller != null)
                    {
                        dbLogin = controller.mDbData;
                    }
                    else
                    {
                        var dbLoginResult = LoginServer.Instance.DB.Get<DBCharacterLogin>(coroutine,
                            DataCategory.LoginCharacter,
                            characterId);
                        yield return dbLoginResult;
                        if (dbLoginResult.Data == null)
                        {
                            msg.Reply((int)ErrorCodes.Error_CharacterId_Not_Exist);
                            yield break;
                        }

                        dbLogin = dbLoginResult.Data;
                    }


                    var info = new CharacterLoginInfo()
                    {
                        ServerId = dbLogin.ServerId,
                        Level = dbLogin.Level,
                        Name = dbLogin.Name,
                        RebornTimes = dbLogin.Ladder,
                        TypeId = dbLogin.TypeId,
                        LastTime = dbLogin.LoginIn
                    };
                    msg.Response.CharacterInfos.Add(info);
                    counter++;
                    if (counter >= 20)
                    {
                        break;
                    }
                }

                //最多返回20个人物信息
                if (counter >= 20)
                {
                    var str = string.Format("this player create character to much! client id:{0} , charactername:{1}",
                        msg.ClientId, msg.Response.CharacterInfos[0].Name);
                    Logger.Info(str);
                    break;
                }
            }
            msg.Reply();
        }

        public IEnumerator CreateCharacterByAccountName(Coroutine coroutine, LoginService _this, CreateCharacterByAccountNameInMessage msg)
        {
            var __this = (LoginServerControl)_this;
            var name = msg.Request.AccName;
            var loadPlayerRet = AsyncReturnValue<PlayerController>.Create();
            var co = CoroutineFactory.NewSubroutine(__this.PlayerManager.LoadPlayer, coroutine, 0ul, name,
                loadPlayerRet);
            if (co.MoveNext())
            {
                yield return co;
            }
            var playerController = loadPlayerRet.Value;
            loadPlayerRet.Dispose();

            //没有账号先创建
            if (playerController == null)
            {//这里只有clone快照的时候会走到如果正常流程到这里就跪了
                var createPlayerRet = AsyncReturnValue<PlayerController>.Create();

                co = CoroutineFactory.NewSubroutine(__this.PlayerManager.CreatePlayer, coroutine, 0ul, name,
                    "123", "-2", loadPlayerRet);
                if (co.MoveNext())
                {
                    yield return co;
                }
                playerController = createPlayerRet.Value;
                createPlayerRet.Dispose();
            }

            //没造出来
            if (playerController == null)
            {
                msg.Reply((int)ErrorCodes.PlayerCreateFaild);
                yield break;
            }

            //如有已经有角色返回第一个角色
            ulong characterId;
            if (playerController.DbData.ServersPlayers.Count != 0)
            {
                var cs = playerController.DbData.ServersPlayers.First();
                characterId = cs.Value.Items.First();
                msg.Response = characterId;
                msg.Reply((int)ErrorCodes.OK);
                yield break;
            }

            //创建一个新角色
            var retNextId = LoginServer.Instance.DB.GetNextId(coroutine, (int)DataCategory.LoginCharacter);
            yield return retNextId;

            if (retNextId.Status != DataStatus.Ok)
            {
                var cleanName = LoginServer.Instance.DB.Delete(coroutine, DataCategory.LoginCharacterName, name);
                yield return null;
                if (cleanName.Status != DataStatus.Ok)
                {
                    Logger.Fatal("CreateCharacter Failed delete name={0}", name);
                }

                msg.Reply((int)ErrorCodes.Error_CreateControllerFailed);
                yield break;
            }
            var nextId = retNextId.Data;
            const int serverId = 1;
            const int type = 0;
            var result = AsyncReturnValue<CharacterController>.Create();
            var createCo = CoroutineFactory.NewSubroutine(CharacterManager.Instance.GetOrCreateCharacterController, coroutine,
                nextId, new object[] { nextId, name, type, serverId, playerController.DbData.Id }, true, result);
            if (createCo.MoveNext())
            {
                yield return createCo;
            }
            if (result.Value == null)
            {
                var cleanName = LoginServer.Instance.DB.Delete(coroutine, DataCategory.LoginCharacterName, name);
                yield return null;
                if (cleanName.Status != DataStatus.Ok)
                {
                    Logger.Fatal("CreateCharacter Failed delete name={0}", name);
                }
                msg.Reply((int)ErrorCodes.Error_CreateControllerFailed);
                yield break;
            }
            result.Dispose();
            playerController.DbData.Players.Add(nextId);
            playerController.CreateCharacter(serverId, nextId);

            var ret = LoginServer.Instance.DB.Set(coroutine, DataCategory.LoginCharacterName, name, nextId.ToDBUlong());
            yield return ret;
            if (ret.Status != DataStatus.Ok)
            {
                msg.Reply((int)ErrorCodes.Error_CharacterId_Not_Exist);
                Logger.Fatal("CreateCharacter ReSet Faild ,name={0}", name);
                yield break;
            }

            characterId = nextId;
            var characterControl = result.Value;
            if (characterControl == null)
            {
                Logger.Error("CreateCharacter Error characterId = {0} null", characterId);
                msg.Reply((int)ErrorCodes.Error_CharacterId_Not_Exist);
                yield break;
            }
            var sceneId = 0;
            sceneId = Table.GetActor(type).BirthScene;

            var status = AsyncReturnValue<int>.Create();
            var isGm = ISGMAccount(playerController.DbData.Name);
            var anotherCo = CoroutineFactory.NewSubroutine(NotifyCreateCharacter, coroutine, characterId, 0ul,
                type, serverId, sceneId, isGm, status);
            if (anotherCo.MoveNext())
            {
                yield return anotherCo;
            }

            var returnValue = AsyncReturnValue<int>.Create();
            co = CoroutineFactory.NewSubroutine(playerController.SaveDb, coroutine, returnValue);
            if (co.MoveNext())
            {
                yield return co;
            }

            returnValue.Dispose();
            if (returnValue.Value != 0)
            {
                msg.Reply((int)ErrorCodes.Error_Login_DBCreate);
                yield break;
            }
            msg.Response = characterId;
            msg.Reply((int)ErrorCodes.OK);
        }

        public IEnumerator CloneCharacterDbById(Coroutine coroutine, LoginService _this, CloneCharacterDbByIdInMessage msg)
        {

            var fromId = msg.Request.FromId;
            var toId = msg.Request.ToId;

            //如果拷贝到的账号在线， 先踢下线否知拷贝完了下线时候又把数据复写回去了
            var __this = (LoginServerControl)_this;
            var ct = CharacterManager.Instance.GetCharacterControllerFromMemroy(toId);
            if (ct != null)
            {
                //                 var logoutCo = CoroutineFactory.NewSubroutine(PlayerLogout, coroutine, __this, toId);
                //                 if (logoutCo.MoveNext())
                //                 {
                //                     yield return logoutCo;
                //                 }
            }

            var result = AsyncReturnValue<CharacterController>.Create();
            var co = CoroutineFactory.NewSubroutine(CharacterManager.Instance.GetOrCreateCharacterController, coroutine,
            fromId, new object[] { }, false, result);

            if (co.MoveNext())
            {
                yield return co;
            }
            if (result.Value == null)
            {
                msg.Reply((int)ErrorCodes.Error_PrepareEnterGameFailed);
                yield break;
            }
            var fromDb = result.Value.mDbData;
            result.Dispose();

            var result2 = AsyncReturnValue<CharacterController>.Create();
            var co2 = CoroutineFactory.NewSubroutine(CharacterManager.Instance.GetOrCreateCharacterController, coroutine,
                toId, new object[] { }, false, result2);

            if (co2.MoveNext())
            {
                yield return co2;
            }

            if (result2.Value == null)
            {
                msg.Reply((int)ErrorCodes.Error_PrepareEnterGameFailed);
                yield break;
            }
            var controller = result2.Value;
            result2.Dispose();

            var dataItem = new CharacterManager.DataItem();
            var dbData = controller.mDbData;
            //dbData.Id = toId;
            dbData.Ladder = fromDb.Ladder;
            dbData.LandCount = fromDb.LandCount;
            dbData.Level = fromDb.Level;
            dbData.LoginDays = fromDb.LoginDays;
            dbData.LoginIn = fromDb.LoginIn;
            dbData.LoginOut = fromDb.LoginOut;
            dbData.Name = fromDb.Name;
            dbData.SaveCount = fromDb.SaveCount;
            dbData.SaveState = fromDb.SaveState;
            dbData.TodayOlineTime = fromDb.TodayOlineTime;
            dbData.TypeId = fromDb.TypeId;
            dbData.Continuedays = fromDb.Continuedays;
            dbData.FoundTime = fromDb.FoundTime;
            dataItem.Controller = controller;
            var co3 = CoroutineFactory.NewSubroutine(CharacterManager.Instance.SaveDataForClone, coroutine, toId, dataItem, true);
            if (co3.MoveNext())
            {
                yield return co3;
            }
            msg.Reply((int)ErrorCodes.OK);
            yield break;
        }

        public IEnumerator GetCharacterIdByAccountName(Coroutine coroutine, LoginService _this,
            GetCharacterIdByAccountNameInMessage msg)
        {
            msg.Response = 0ul;
            var result = LoginServer.Instance.DB.Get<DBUlong>(coroutine, DataCategory.LoginPlayerName,
                msg.Request.AccName);
            yield return result;
            if (result.Data != null)
            {
                var dbAccout = LoginServer.Instance.DB.Get<DBPlayerLogin>(coroutine, DataCategory.LoginPlayer,
                    result.Data.Value);
                yield return dbAccout;
                if (dbAccout.Data != null)
                {
                    var oneServerCharacters = dbAccout.Data.ServersPlayers;
                    if (oneServerCharacters.Count > 0)
                    {
                        var one = oneServerCharacters.First();
                        if (one.Value.Items.Count > 0)
                        {
                            msg.Response = one.Value.Items.First();
                        }
                    }
                }
            }

            msg.Reply();
            yield break;
        }


        #endregion
    }
}