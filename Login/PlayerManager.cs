#region using

using System;
using System.Collections;
using System.Collections.Generic;
using Database;
using DataContract;
using DataTable;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Login
{
    public interface IPlayerController
    {
        void CreateCharacter(int serverId, ulong characterId, PlayerController _this);
        ConnectState GetPlayerState(PlayerController _this);
        void Kick(ulong clientId, KickClientType type);
        IEnumerator SaveAccountDb(Coroutine coroutine, AsyncReturnValue<int> ret, PlayerController _this);
        IEnumerator SaveDb(Coroutine coroutine, AsyncReturnValue<int> ret, PlayerController _this);
        void SaveLoginTime(PlayerController _this);
    }

    public class PlayerControllerDefaultImpl : IPlayerController
    {
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IEnumerator SaveLoginTimeCoroutine(Coroutine coroutine, PlayerController _this)
        {
            var retSet = LoginServer.Instance.DB.Set(coroutine, DataCategory.LoginPlayerLastLoginTime, _this.DbData.Name,
                DateTime.Now.ToBinary().ToDBUlong());
            yield return retSet;
            if (retSet.Status == DataStatus.Ok)
            {
                Logger.Info("SaveLoginTime Player DB OK" + _this.DbData.Name);
            }
        }

        public void Kick(ulong clientId, KickClientType type)
        {
            LoginServerControlDefaultImpl.NotifyGateClientLost(LoginServer.Instance.ServerControl, clientId, type);
            //LoginServer.Instance.ServerControl.Kick(clientId, 0);
        }

        public IEnumerator SaveDb(Coroutine coroutine, AsyncReturnValue<int> ret, PlayerController _this)
        {
            ret.Value = 1;
            var retSet = LoginServer.Instance.DB.Set(coroutine, DataCategory.LoginPlayer, _this.DbData.Id, _this.DbData);
            yield return retSet;
            if (retSet.Status == DataStatus.Ok)
            {
                Logger.Info("Save Player DB OK" + _this.DbData.Name);
            }
            ret.Value = 0;
        }

        public IEnumerator SaveAccountDb(Coroutine coroutine, AsyncReturnValue<int> ret, PlayerController _this)
        {
            ret.Value = 1;
            var retSet = LoginServer.Instance.DB.Set(coroutine, DataCategory.LoginPlayerName,
                _this.DbData.Name, _this.DbData.Id.ToDBUlong());
            yield return retSet;
            if (retSet.Status == DataStatus.Ok)
            {
                Logger.Info("SaveAccountDb Player DB OK" + _this.DbData.Name);
            }
            ret.Value = 0;
        }

        public void SaveLoginTime(PlayerController _this)
        {
            CoroutineFactory.NewCoroutine(SaveLoginTimeCoroutine, _this).MoveNext();
        }

        public void CreateCharacter(int serverId, ulong characterId, PlayerController _this)
        {
            Uint64Array tempList;
            if (!_this.DbData.ServersPlayers.TryGetValue(serverId, out tempList))
            {
                tempList = new Uint64Array();
                _this.DbData.ServersPlayers[serverId] = tempList;
            }
            tempList.Items.Add(characterId);
        }

        public ConnectState GetPlayerState(PlayerController _this)
        {
            if (_this.Connect == null)
            {
                return ConnectState.NotFind;
            }
            return _this.Connect.State;
        }
    }

    public class PlayerController
    {
        private static IPlayerController mImpl;
        public string Platform { get; set; }
        public string LoginChannel { get; set; }     

        public string RemoteIpAddress { get; set; }
        
        static PlayerController()
        {
            LoginServer.Instance.UpdateManager.InitStaticImpl(typeof (PlayerController),
                typeof (PlayerControllerDefaultImpl),
                o => { mImpl = (IPlayerController) o; });
        }

        public PlayerConnect Connect { get; set; }
        public DBPlayerLogin DbData { get; set; }

        public void CreateCharacter(int serverId, ulong characterId)
        {
            mImpl.CreateCharacter(serverId, characterId, this);
        }

        public ConnectState GetPlayerState()
        {
            return mImpl.GetPlayerState(this);
        }

        public void Kick(ulong clientId, KickClientType type)
        {
            mImpl.Kick(clientId, type);
        }

        public IEnumerator SaveAccountDb(Coroutine coroutine, AsyncReturnValue<int> ret)
        {
            return mImpl.SaveAccountDb(coroutine, ret, this);
        }

        public IEnumerator SaveDb(Coroutine coroutine, AsyncReturnValue<int> ret)
        {
            return mImpl.SaveDb(coroutine, ret, this);
        }

        public void SaveLoginTime()
        {
            mImpl.SaveLoginTime(this);
        }
    }

    public interface IPlayerManager
    {
        int AddPlayer(ulong clientId, PlayerController player, PlayerManager _this);

        IEnumerator CreateCharacter(Coroutine coroutine,
                                    ulong clientId,
                                    int serverId,
                                    int type,
                                    string name,
                                    AsyncReturnValue<int, ulong> asyValue,
                                    PlayerManager _this);

        IEnumerator CreatePlayer(Coroutine coroutine,
                                 ulong clientId,
                                 string name,
                                 string psw,
            					 string channelPid,
                                 AsyncReturnValue<PlayerController> status,
                                 PlayerManager _this);

        PlayerController GetPlayerController(ulong clientId, PlayerManager _this);
        PlayerController GetPlayerControllerByPlayerId(ulong playerId, PlayerManager _this);
        ConnectState GetPlayerState(PlayerManager _this, ulong playerId);

        IEnumerator LoadPlayer(Coroutine coroutine,
                               ulong clientId,
                               string name,
                               AsyncReturnValue<PlayerController> ret,
                               PlayerManager _this);

        IEnumerator LoadPlayer(Coroutine coroutine, ulong playerId, AsyncReturnValue<PlayerController> ret);
        int ModifyClientId(ulong playerId, ulong newClientId, PlayerManager _this);
        void TryDelPlayer(PlayerManager _this, ulong clientId, out PlayerController player);
    }


    public class PlayerManagerDefaultImpl : IPlayerManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public PlayerController GetPlayerController(ulong clientId, PlayerManager _this)
        {
            PlayerController player = null;
            if (_this.Players.TryGetValue(clientId, out player))
            {
                return player;
            }
            return null;
        }

        public PlayerController GetPlayerControllerByPlayerId(ulong playerId, PlayerManager _this)
        {
            PlayerController player = null;
            if (_this.PlayersByPlayerId.TryGetValue(playerId, out player))
            {
                return player;
            }
            return null;
        }

        public int AddPlayer(ulong clientId, PlayerController player, PlayerManager _this)
        {
            if (_this.GetPlayerController(clientId) != null)
            {
                return -1; //client已重复
            }
            var oldPlayer = GetPlayerControllerByPlayerId(player.DbData.Id, _this);
            if (oldPlayer != null)
            {
                if (oldPlayer.Connect.ClientId != clientId)
                {
                    //player已经有其他client连进来了
                    return -2;
                }
                return -3; //按说不应该发生这个情况
            }
            _this.Players[clientId] = player;
            _this.PlayersByPlayerId[player.DbData.Id] = player;
            return 0;
        }

        public void TryDelPlayer(PlayerManager _this, ulong clientId, out PlayerController player)
        {
            if (_this.Players.TryGetValue(clientId, out player))
            {
                _this.Players.Remove(clientId);
                _this.PlayersByPlayerId.Remove(player.DbData.Id);
            }
        }

        public ConnectState GetPlayerState(PlayerManager _this, ulong playerId)
        {
            var player = GetPlayerControllerByPlayerId(playerId, _this);
            if (player == null)
            {
                return ConnectState.NotFind;
            }
            return player.GetPlayerState();
        }

        public int ModifyClientId(ulong playerId, ulong newClientId, PlayerManager _this)
        {
            var player = _this.GetPlayerControllerByPlayerId(playerId);
            if (player == null)
            {
                return -1;
            }
            if (player.Connect == null)
            {
                return -2;
            }
            var oldClientId = player.Connect.ClientId;
            player.Connect.ClientId = newClientId;
            _this.Players.Remove(oldClientId);
            _this.Players[newClientId] = player;
            return 0;
        }

        public IEnumerator LoadPlayer(Coroutine coroutine,
                                      ulong clientId,
                                      string name,
                                      AsyncReturnValue<PlayerController> ret,
                                      PlayerManager _this)
        {
            ret.Value = null;
            var dbAccoutGuid = LoginServer.Instance.DB.Get<DBUlong>(coroutine, DataCategory.LoginPlayerName,
                name);
            yield return dbAccoutGuid;
            if (dbAccoutGuid.Data == null)
            {
                yield break;
            }
            var dbAccout = LoginServer.Instance.DB.Get<DBPlayerLogin>(coroutine, DataCategory.LoginPlayer,
                dbAccoutGuid.Data.Value);
            yield return dbAccout;
            if (dbAccout.Data == null)
            {
                yield break;
            }
            var playerController = new PlayerController();
            playerController.DbData = dbAccout.Data;
            ret.Value = playerController;
        }

        public IEnumerator LoadPlayer(Coroutine coroutine, ulong playerId, AsyncReturnValue<PlayerController> ret)
        {
            ret.Value = null;
            var dbAccout = LoginServer.Instance.DB.Get<DBPlayerLogin>(coroutine, DataCategory.LoginPlayer, playerId);
            yield return dbAccout;
            if (dbAccout.Data == null)
            {
                yield break;
            }
            var playerController = new PlayerController();
            playerController.DbData = dbAccout.Data;
            ret.Value = playerController;
        }

        public IEnumerator CreatePlayer(Coroutine coroutine,
                                        ulong clientId,
                                        string name,
                                        string psw,
            							string channelPid,
                                        AsyncReturnValue<PlayerController> status,
                                        PlayerManager _this)
        {
            status.Value = null;
            var playerGuid = LoginServer.Instance.DB.GetNextId(coroutine, (int) DataCategory.LoginPlayer);
            yield return playerGuid;
            if (playerGuid.Status != DataStatus.Ok)
            {
                yield break;
            }

            int lastServerId = Table.GetServerConfig(2000).ToInt();
            ChannelServerInfo info;
            if (StaticParam.ServerListWithPid.TryGetValue(channelPid, out info))
            {
                if (info.NewServers.Count > 0)
                {
                    var index = MyRandom.Random(0, info.NewServers.Count - 1);
                    lastServerId = info.NewServers[index].Id;
                }
                else if(info.PrepareServers.Count > 0)
                {
                    var index = MyRandom.Random(0, info.PrepareServers.Count - 1);
                    lastServerId = info.PrepareServers[index].Id;
                }
            }
            if (StaticParam.ServerListWithPid.TryGetValue("all", out info))
            {
                if (info.NewServers.Count > 0)
                {
                    var index = MyRandom.Random(0, info.NewServers.Count - 1);
                    lastServerId = info.NewServers[index].Id;
                }
                else if (info.PrepareServers.Count > 0)
                {
                    var index = MyRandom.Random(0, info.PrepareServers.Count - 1);
                    lastServerId = info.PrepareServers[index].Id;
                }
            }

            var player = new PlayerController
            {
                DbData = new DBPlayerLogin
                {
                    Id = playerGuid.Data,
                    Name = name,
                    Pwd = psw,
                    SelectChar = 0,
                    FoundTime = DateTime.Now.ToBinary(),
                    LoginDay = 0,
                    LoginTotal = 0,
                    IsLock = 0,
                    LastServerId = lastServerId
                }
            };

            var returnValue = AsyncReturnValue<int>.Create();
            var co = CoroutineFactory.NewSubroutine(player.SaveDb, coroutine, returnValue);
            if (co.MoveNext())
            {
                yield return co;
            }
            returnValue.Dispose();
            var accountValue = AsyncReturnValue<int>.Create();
            co = CoroutineFactory.NewSubroutine(player.SaveAccountDb, coroutine, accountValue);
            if (co.MoveNext())
            {
                yield return co;
            }
            accountValue.Dispose();
            status.Value = player;
            //_this.AddPlayer(clientId, player);
            //创建账户
            //PlayerLog.StatisticsLogger("na,{0}", player.DbData.Id);
            PlayerLog.BackDataLogger((int) BackDataType.NewPlayer, "{0}", player.DbData.Id);
        }

        public IEnumerator CreateCharacter(Coroutine coroutine,
                                           ulong clientId,
                                           int serverId,
                                           int type,
                                           string name,
                                           AsyncReturnValue<int, ulong> asyValue,
                                           PlayerManager _this)
        {
            asyValue.Value1 = (int) ErrorCodes.Error_Login_DBCreate;
            asyValue.Value2 = 0;

            var player = _this.GetPlayerController(clientId);

            var retDbId = LoginServer.Instance.DB.Set(coroutine, DataCategory.LoginCharacterName, name, 0ul.ToDBUlong(),
                SetOption.SetIfNotExist);

            yield return null;

            if (retDbId.Status == DataStatus.DatabaseError)
            {
                asyValue.Value1 = (int) ErrorCodes.DataBase;
                PlayerLog.WriteLog((int) LogType.DataBase, "CreateCharacter DataBase Error! name={0}", name);
                yield break;
            }
            if (retDbId.Status != DataStatus.Ok)
            {
                asyValue.Value1 = (int) ErrorCodes.Error_NAME_IN_USE;
                PlayerLog.WriteLog((int) LogType.Error_NAME_IN_USE, "CreateCharacter Error_NAME_IN_USE name={0}", name);
                yield break;
            }

            var retNextId = LoginServer.Instance.DB.GetNextId(coroutine, (int) DataCategory.LoginCharacter);
            yield return retNextId;

            if (retNextId.Status != DataStatus.Ok)
            {
                var cleanName = LoginServer.Instance.DB.Delete(coroutine, DataCategory.LoginCharacterName, name);
                yield return null;
                if (cleanName.Status != DataStatus.Ok)
                {
                    Logger.Fatal("CreateCharacter Failed delete name={0}", name);
                }

                asyValue.Value1 = (int) ErrorCodes.Error_CreateControllerFailed;
                yield break;
            }
            var nextId = retNextId.Data;
            asyValue.Value2 = nextId;

            var result = AsyncReturnValue<CharacterController>.Create();
            var co = CoroutineFactory.NewSubroutine(CharacterManager.Instance.GetOrCreateCharacterController, coroutine,
                nextId, new object[] {nextId, name, type, serverId, player.DbData.Id}, true, result);
            if (co.MoveNext())
            {
                yield return co;
            }

            if (result.Value == null)
            {
                Logger.Error("CreateCharacter Failed id={0} name={1}", nextId, name);
                asyValue.Value1 = (int) ErrorCodes.Error_CreateControllerFailed;

                var cleanName = LoginServer.Instance.DB.Delete(coroutine, DataCategory.LoginCharacterName, name);
                yield return null;
                if (cleanName.Status != DataStatus.Ok)
                {
                    Logger.Fatal("CreateCharacter Failed delete name={0}", name);
                }

                yield break;
            }
            result.Dispose();
            result.Value.SetLoginDays(1);
            result.Value.GetTodayOnlineTime();
            player.DbData.Players.Add(nextId);
            player.CreateCharacter(serverId, nextId);

            var ret = LoginServer.Instance.DB.Set(coroutine, DataCategory.LoginCharacterName, name, nextId.ToDBUlong());
            yield return ret;
            if (ret.Status != DataStatus.Ok)
            {
                asyValue.Value1 = (int) ErrorCodes.Error_LoginCreateSetNameDB;
                Logger.Fatal("CreateCharacter ReSet Faild ,name={0}", name);
                yield break;
            }

            asyValue.Value1 = 0;

            //新增角色 
            //PlayerLog.StatisticsLogger("nc,{0},{1}", nextId, player.DbData.Id);
            PlayerLog.BackDataLogger((int) BackDataType.NewCharacter, "{0}|{1}|{2}", player.DbData.Id, nextId, serverId);

            try
            {
                var charCtrl = result.Value;
                if (charCtrl != null && charCtrl.GetData() != null && player != null && player.DbData != null)
                {
                    string gameuserlog = string.Format("gameusers#{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}",
                        charCtrl.GetData().Id,// charid
                        player.DbData.Name, //acount
                        charCtrl.GetData().ServerId, // serverid
                        DateTime.FromBinary(charCtrl.GetData().FoundTime).ToString("yyyy/MM/dd HH:mm:ss"),   // createtime
                        player.Platform,// createplatform
                        player.LoginChannel,   // createspid
                        DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),   // logintime
                        DateTime.MinValue,   // logouttime
                        player.Platform, // loginplatfomr
                        charCtrl.GetLoginDays() // dayback
                        );
                    Logger kafaLogger = LogManager.GetLogger(Shared.LoggerName.KafkaLog);
                    kafaLogger.Info(gameuserlog);

                    string userlevel = string.Format("userlevel#{0}|{1}|{2}",
                        charCtrl.GetData().Id,// charid
                        1, //level
                        DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
                        );
                    kafaLogger.Info(userlevel);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            } 
        }
    }

    public class PlayerManager
    {
        private static IPlayerManager mImpl;
        private static readonly Dictionary<string, int> RobotNames = new Dictionary<string, int>();

        static PlayerManager()
        {
            LoginServer.Instance.UpdateManager.InitStaticImpl(typeof (PlayerManager), typeof (PlayerManagerDefaultImpl),
                o => { mImpl = (IPlayerManager) o; });
        }

        //key = loginType + PlayerName
        public Dictionary<string, PlayerController> NameKey2Player = new Dictionary<string, PlayerController>();
        //key = ClientId
        public Dictionary<ulong, PlayerController> Players = new Dictionary<ulong, PlayerController>();
        //key = PlayerId
        public Dictionary<ulong, PlayerController> PlayersByPlayerId = new Dictionary<ulong, PlayerController>();

        public int AddPlayer(ulong clientId, PlayerController player)
        {
            return mImpl.AddPlayer(clientId, player, this);
        }

        public IEnumerator CreateCharacter(Coroutine coroutine,
                                           ulong clientId,
                                           int serverId,
                                           int type,
                                           string name,
                                           AsyncReturnValue<int, ulong> asyValue)
        {
            return mImpl.CreateCharacter(coroutine, clientId, serverId, type, name, asyValue, this);
        }

        public IEnumerator CreatePlayer(Coroutine coroutine,
                                        ulong clientId,
                                        string name,
                                        string psw,
            							string channelPid,
                                        AsyncReturnValue<PlayerController> status)
        {
            return mImpl.CreatePlayer(coroutine, clientId, name, psw, channelPid, status, this);
        }

        public static IPlayerManager GetImpl()
        {
            return mImpl;
        }

        public PlayerController GetPlayerController(ulong clientId)
        {
            return mImpl.GetPlayerController(clientId, this);
        }

        public PlayerController GetPlayerControllerByPlayerId(ulong playerId)
        {
            return mImpl.GetPlayerControllerByPlayerId(playerId, this);
        }

        //静态数据初始化
        public static void Init()
        {
            Table.ForeachJJCRoot(record =>
            {
                RobotNames.Add(record.Name, 1);
                return true;
            });
        }

        public static bool IsRobot(string name)
        {
            return RobotNames.ContainsKey(name);
        }

        public IEnumerator LoadPlayer(Coroutine coroutine,
                                      ulong clientId,
                                      string name,
                                      AsyncReturnValue<PlayerController> ret)
        {
            return mImpl.LoadPlayer(coroutine, clientId, name, ret, this);
        }

        public IEnumerator LoadPlayer(Coroutine coroutine, ulong playerId, AsyncReturnValue<PlayerController> ret)
        {
            return mImpl.LoadPlayer(coroutine, playerId, ret);
        }

        public int ModifyClientId(ulong playerId, ulong newClientId)
        {
            return mImpl.ModifyClientId(playerId, newClientId, this);
        }

        public void TryDelPlayer(ulong clientId, out PlayerController player)
        {
            mImpl.TryDelPlayer(this, clientId, out player);
        }
    }
}