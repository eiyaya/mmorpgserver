#region using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using DataContract;
using Scorpion;
using NLog;
using ProtoBuf;
using Shared;
using System.Net;
using System.Text;

#endregion

namespace Gate
{
    internal class CharacterInfoEx
    {
        protected static readonly Logger mLogger = LogManager.GetCurrentClassLogger();
        public ulong ClientId;
        public ulong Key = 0;
        public ulong mCharacterId;
        public ConcurrentQueue<ServiceDesc> Messages = new ConcurrentQueue<ServiceDesc>();
        public GateClientState mState = GateClientState.NotAuthorized;
        public ConcurrentDictionary<int, SocketClient> Servers = new ConcurrentDictionary<int, SocketClient>();
        public DateTime Last1000Message;
        public int Last1000MessageCount = 0;
        public ulong CharacterId
        {
            get { return mCharacterId; }
            set
            {
                if (CharacterId != value)
                {
                    mCharacterId = value;
                    if (value != 0)
                    {
                        var logic = GateServer.GetSocketClientByServiceTypeAndCharacter(ServiceType.Logic, value);
                        Servers.AddOrUpdate((int) ServiceType.Logic, key => { return logic; }, (key, oldvalue) =>
                        {
                            mLogger.Error(" CharacterId old={0},new={1}", oldvalue, value);
                            return logic;
                        });

                        var team = GateServer.GetSocketClientByServiceTypeAndCharacter(ServiceType.Team, value);
                        Servers.AddOrUpdate((int) ServiceType.Team, key => { return team; }, (key, oldvalue) =>
                        {
                            mLogger.Error(" CharacterId old={0},new={1}", oldvalue, value);
                            return team;
                        });

                        var chat = GateServer.GetSocketClientByServiceTypeAndCharacter(ServiceType.Chat, value);
                        Servers.AddOrUpdate((int) ServiceType.Chat, key => { return chat; }, (key, oldvalue) =>
                        {
                            mLogger.Error(" CharacterId old={0},new={1}", oldvalue, value);
                            return chat;
                        });

                        var rank = GateServer.GetSocketClientByServiceTypeAndCharacter(ServiceType.Rank, value);
                        Servers.AddOrUpdate((int) ServiceType.Rank, key => { return rank; }, (key, oldvalue) =>
                        {
                            mLogger.Error(" CharacterId old={0},new={1}", oldvalue, value);
                            return rank;
                        });
                        var activity = GateServer.GetSocketClientByServiceTypeAndCharacter(ServiceType.Activity, value);
                        Servers.AddOrUpdate((int) ServiceType.Activity, key => { return activity; }, (key, oldvalue) =>
                        {
                            mLogger.Error(" CharacterId old={0},new={1}", oldvalue, value);
                            return activity;
                        });
                        //Servers.TryAdd((int) ServiceType.Logic,GateServer.GetSocketClientByServiceTypeAndCharacter(ServiceType.Logic, value));
                        //Servers.TryAdd((int)ServiceType.Team, GateServer.GetSocketClientByServiceTypeAndCharacter(ServiceType.Team, value));
                        //Servers.TryAdd((int)ServiceType.Rank, GateServer.GetSocketClientByServiceTypeAndCharacter(ServiceType.Rank, value));
                        //Servers.TryAdd((int)ServiceType.Chat, GateServer.GetSocketClientByServiceTypeAndCharacter(ServiceType.Chat, value));
                        //Servers.TryAdd((int)ServiceType.Activity, GateServer.GetSocketClientByServiceTypeAndCharacter(ServiceType.Activity, value));
                    }
                    else
                    {
                        SocketClient server;
                        Servers.TryRemove((int) ServiceType.Logic, out server);
                        Servers.TryRemove((int) ServiceType.Team, out server);
                        Servers.TryRemove((int) ServiceType.Rank, out server);
                        Servers.TryRemove((int) ServiceType.Chat, out server);
                        Servers.TryRemove((int) ServiceType.Activity, out server);
                        Servers.TryRemove((int) ServiceType.Scene, out server);
                    }
                }
            }
        }
    }

    public partial class GateServer
    {
        private void OnSocketClienMessageReceived196(SocketClient client, ServiceDesc desc)
        {
            var newClientId = desc.ClientId;
            var oldClientId = desc.Routing[0];
            var sceneIndex = (int) desc.Routing[1];
            //Logger.Fatal("Gate Re Connect ------ 1---{0} -- {1}", DateTime.Now.ToBinary(), oldClientId);
            CharacterInfoEx characterInfo;
            ServerClient serverClient;
            if (mFromClientId2Client.TryRemove(newClientId, out serverClient))
            {
                //serverClient.UserData;
                mFromClientId2Client.AddOrUpdate(oldClientId, serverClient, (arg1, arg2) => serverClient);
                characterInfo = serverClient.UserData as CharacterInfoEx;
                if (characterInfo == null)
                {
                    return;
                }
                characterInfo.ClientId = oldClientId;
                characterInfo.mState = GateClientState.GamePlay;
                characterInfo.CharacterId = desc.CharacterId;

                var server = GetSocketClientByServiceTypeAndIndex(ServiceType.Scene, sceneIndex);
                characterInfo.Servers[(int) ServiceType.Scene] = server;


                var descReply = new ServiceDesc();
                descReply.FuncId = 2035;
                descReply.ServiceType = (int) ServiceType.Login;
                descReply.Type = (int) MessageType.SC;
                using (var ms = new MemoryStream())
                {
                    var ret = new __RPC_Login_NotifyReConnet_ARG_int32_result__();
                    ret.Result = 0;
                    Serializer.Serialize(ms, ret);
                    descReply.Data = ms.ToArray();
                }
                serverClient.SendMessage(descReply);
            }
            else
            {
                Logger.Warn("Client {0} is not connected. ", newClientId);
            }
        }

        private void OnSocketClienMessageReceived197(SocketClient client, ServiceDesc desc)
        {
            var key = desc.ClientId;
            CharacterInfoEx characterInfo;
            ServerClient serverClient;
            if (mFromClientId2Client.TryGetValue(key, out serverClient))
            {
                characterInfo = serverClient.UserData as CharacterInfoEx;
                if (characterInfo == null)
                {
                    return;
                }
                characterInfo.Servers[desc.ServiceType] = client;
            }
            else
            {
                Logger.Warn("Client {0} is not connected. ", key);
            }
        }

        private void OnSocketClienMessageReceived198(SocketClient client, ServiceDesc desc)
        {
            var key = desc.ClientId;
            var clientId = desc.ClientId;
            var characterId = desc.CharacterId;


            //CharacterInfoEx characterInfo;
            ServerClient serverClient;
            if (mFromClientId2Client.TryRemove(desc.ClientId, out serverClient))
            {
                var characterInfo = serverClient.UserData as CharacterInfoEx;
                if (characterInfo == null)
                {
                    return;
                }
                characterInfo.CharacterId = 0ul;
                characterInfo.mState = GateClientState.NotAuthorized;
                var type = (KickClientType) desc.FuncId;
                switch (type)
                {
                    case KickClientType.OtherLogin:
                    case KickClientType.ChangeServer:
                    case KickClientType.ChangeServerOK:
                    case KickClientType.GmKick:
                    case KickClientType.LoginTimeOut:
                    {
                        var descReply = new ServiceDesc();
                        descReply.FuncId = 2010;
                        descReply.ServiceType = (int) ServiceType.Login;
                        descReply.Type = (int) MessageType.SC;
                        using (var ms = new MemoryStream())
                        {
                            var ret = new __RPC_Login_Kick_ARG_int32_type__();
                            ret.Type = (int) type;
                            Serializer.Serialize(ms, ret);
                            descReply.Data = ms.ToArray();
                        }
                        serverClient.SendMessage(descReply);
                    }
                        break;
                    case KickClientType.LostLine:
                    case KickClientType.BacktoLogin:
                    default:
                        break;
                }
            }
        }

        //Login通知Gate修改某个Client的链接状态
        private void OnSocketClienMessageReceived199(SocketClient client, ServiceDesc desc)
        {
            var clientId = desc.ClientId;
            var characterId = desc.CharacterId;

            CharacterInfoEx characterInfo;
            ServerClient serverClient;
            var state = (GateClientState) desc.FuncId;
            if (mFromClientId2Client.TryGetValue(clientId, out serverClient))
            {
                characterInfo = serverClient.UserData as CharacterInfoEx;
                if (characterInfo == null)
                {
                    return;
                }
                //ChangeState(state);
                //var oldState = characterInfo.mState;
                characterInfo.mState = state;
                if (state == GateClientState.GamePlay)
                {
                    characterInfo.CharacterId = characterId;
                }
                else if (state == GateClientState.Login)
                {
                    characterInfo.CharacterId = 0;
                    characterInfo.Servers[(int) ServiceType.Login] =
                        GetSocketClientByServiceTypeAndCharacter(ServiceType.Login, 0);
                }
                else if (state == GateClientState.NotAuthorized)
                {
                    characterInfo.Servers.Clear();
                }
            }
            else
            {
                Logger.Warn("Client {0} is not connected. ", clientId);
            }
        }

        //Broker 或  Server 有消息过来时
        private void OnSocketClienMessageReceivedEx(SocketClient client, ServiceDesc desc)
        {
            switch (desc.Type)
            {
                case (int) MessageType.ReConnetServerToGate:
                    //修改Client的链接状态
                    OnSocketClienMessageReceived196(client, desc);
                    return;
                case (int) MessageType.ChangeState:
                    //修改Client的链接状态
                    OnSocketClienMessageReceived199(client, desc);
                    return;
                case (int) MessageType.Kick:
                    //需要踢掉的Client
                    OnSocketClienMessageReceived198(client, desc);
                    return;
                case (int) MessageType.CharacterConnetServer:
                    //需要踢掉的Client
                    OnSocketClienMessageReceived197(client, desc);
                    return;
                case 20: //某服务器需要添加至 Client管理中的直接发包逻辑
                    Logger.Error("OnSocketClienMessageReceivedEx type = 20,funcId={0}", desc.FuncId);
                    //OnSocketClienMessageReceived20(client, desc);
                    return;
                case (int) MessageType.SCAll:
                    OnSocketClienMessageReceivedSCAll(client, desc);
                    return;
                case (int) MessageType.SCServer:
                case (int) MessageType.SCList:
                    OnSocketClienMessageReceivedSCServerSCList(client, desc);
                    return;
                default:
                    OnSocketClienMessageReceivedSingleEx(client, desc);
                    return;
            }
        }

        private void OnSocketClienMessageReceivedSingleEx(SocketClient client, ServiceDesc desc)
        {
            //var key = desc.ClientId;
            var clientId = desc.ClientId;
            CharacterInfoEx characterInfo;
            ServerClient info;
            if (mFromClientId2Client.TryGetValue(clientId, out info))
            {
                if (!info.IsConnected)
                {
                    RemoveClient(clientId);
                    return;
                }

                characterInfo = info.UserData as CharacterInfoEx;
                if (characterInfo == null)
                {
                    return;
                }

                // 这种情况不应该发生，但是发生了。。。
                if (characterInfo.ClientId != clientId)
                {
                    ServerClient sc;
                    mFromClientId2Client.TryRemove(clientId, out sc);
                    NotifyBrokerCharacterLost(clientId, true);

                    Logger.Error(
                        "OnSocketClienMessageReceivedSingleEx characterInfo.ClientId = {0},desc.ClientId = {1}",
                        characterInfo.ClientId, clientId);
                }

                {
                    try
                    {
                        LogMessage(clientId, desc, false);
                        RemoveWaitingMessage(desc);

                        desc.ClientId = 0;
                        desc.CharacterId = 0;
                        desc.Routing.Clear();

                        info.SendMessage(desc);
                    }
                    catch (Exception ex)
                    {
                        Logger.WarnException("Send message failed. 11", ex);
                    }
                }
            }
        }

        public static long IpToInt(string ip)
        {
            ip = ip.Substring(ip.LastIndexOf(':') + 1);
            char[] separator = new char[] { '.' };
            string[] items = ip.Split(separator);
            return long.Parse(items[0]) << 24
                    | long.Parse(items[1]) << 16
                    | long.Parse(items[2]) << 8
                    | long.Parse(items[3]);
        }

        //-----------------------------------------------------SocketListener------------------------
        //客户端链接进来时
        private void OnSocketListenerConnectedEx(ServerClient sender)
        {
            int i = 0;
            if(BlackList.TryGetValue(((IPEndPoint)sender.RemoteEndPoint).Address, out i))
            {
                //sender.SendMessage(new ServiceDesc{Type = (byte)MessageType.DROP, ClientId = 30});
                sender.Disconnect();
                return;
            }

            sender.MessageReceived += OnSocketListenerMessageReceivedEx;
            var clientId = GetUniqueClientId((uint) mId, sender.ClientId);
            //Logger.Fatal("Gate Connect ------ 1---{0} -- {1}", DateTime.Now.ToBinary(), clientId);
            var characterInfo = new CharacterInfoEx();
            characterInfo.mState = GateClientState.NotAuthorized;
            characterInfo.ClientId = clientId;

            Logger.Info("Client: " + clientId + " connected.");

            sender.UserData = characterInfo;
            mFromClientId2Client.AddOrUpdate(characterInfo.ClientId, sender, (l, arg2) => sender);
        }

        //客户端有消息发过来时
        private void OnSocketListenerMessageReceivedEx(ServerClient client, ServiceDesc desc)
        {
			GateServerMonitor.ReceivePacketNumber.Mark();
			if (null != desc && null != desc.Data)
			{
				GateServerMonitor.ReceivePacketSize.Mark(desc.Data.Length);
			}

            if (desc.Type == (int) MessageType.Ping)
            {
                client.SendMessage(desc);
                return;
            }

            Logger.Debug("Received a message from Client {0}.", client.ClientId);
            var characterInfo = client.UserData as CharacterInfoEx;
            if (characterInfo == null)
            {
                Logger.Error("Internal error for ClientMessageReceived: " + desc.ServiceType);
                return;
            }

            if(characterInfo.Last1000MessageCount == 0)
            {
                characterInfo.Last1000Message = DateTime.Now;
            }

            characterInfo.Last1000MessageCount++;

            if (characterInfo.Last1000MessageCount == 1000)
            {
                characterInfo.Last1000MessageCount = 0;
                var duration = (DateTime.Now - characterInfo.Last1000Message).TotalMilliseconds;
                if (duration < 1000)
                {
                    Logger.Error("Add {0} to black list.", ((IPEndPoint)client.RemoteEndPoint).Address);
                    BlackList.TryAdd(((IPEndPoint)client.RemoteEndPoint).Address, 0);
                    //client.SendMessage(new ServiceDesc { Type = (byte)MessageType.DROP, ClientId = 30 });
                    client.Disconnect();
                }
            }

            if (!mCSFunctionId2Name.ContainsKey((int)desc.FuncId))
            {
                characterInfo.Last1000MessageCount = 0;
                Logger.Error("Add {0} to black list because attack by funcid {1}.", ((IPEndPoint)client.RemoteEndPoint).Address, desc.FuncId);
                BlackList.TryAdd(((IPEndPoint)client.RemoteEndPoint).Address, 0);
                client.Disconnect();
                //client.SendMessage(new ServiceDesc { Type = (byte)MessageType.DROP, ClientId = 30 });
                return;
            }

            switch (characterInfo.mState)
            {
                case GateClientState.NotAuthorized:
                {
                    //只要验证账号密码函数通过
                    if (desc.FuncId != 2001 //PlayerLoginByUserNamePassword
                        && desc.FuncId != 2002 //PlayerLoginByThirdKey
                        && desc.FuncId != 2031 //QueryServerTimezone
                        && desc.FuncId != 2014 //SyncTime
                        && desc.FuncId != 2034 //ReConnet
                        && desc.FuncId != 2037 // SendDeviceUdid
                        )
                    {
                        Logger.Info("OnSocketListenerMessageReceivedEx NotAuthorized FuncId: {0}", desc.FuncId);
                        return;
                    }

                    List<SocketClient> serverClient;
                    if (mFromId2Servers.TryGetValue(ServiceType.Login, out serverClient))
                    {
                        try
                        {
                            //characterInfo.Servers[(int)ServiceType.Login] = serverClient[0];
                            desc.ClientId = characterInfo.ClientId;
                                try
                                {
                                    desc.Routing.Add((ulong)IpToInt(((IPEndPoint)(client.RemoteEndPoint)).Address.ToString()));
                                }
                                catch { }
                            serverClient[0].SendMessage(desc);
                        }
                        catch (Exception ex)
                        {
                            Logger.WarnException("OnSocketListenerMessageReceivedEx NotAuthorized WarnException", ex);
                        }
                    }
                    else
                    {
                        Logger.Info("OnSocketListenerMessageReceivedEx NotAuthorized LoginNotConnet");
                    }
                }
                    break;
                case GateClientState.Login:
                {
                    if (desc.ServiceType != (int) ServiceType.Login)
                    {
                        Logger.Error("OnSocketListenerMessageReceivedEx Login FuncId: {0}", desc.FuncId);
                        return;
                    }
                    SocketClient serverClient;
                    if (characterInfo.Servers.TryGetValue((int) ServiceType.Login, out serverClient))
                    {
                        try
                        {
                            desc.ClientId = characterInfo.ClientId;
                            serverClient.SendMessage(desc);
                        }
                        catch (Exception ex)
                        {
                            Logger.WarnException("OnSocketListenerMessageReceivedEx Login WarnException", ex);
                        }
                    }
                    else
                    {
                        Logger.Info("OnSocketListenerMessageReceivedEx Login LoginNotConnet");
                    }
                }
                    break;
                case GateClientState.GamePlay:
                {
                    SocketClient serverClient;
                    if (characterInfo.Servers.TryGetValue(desc.ServiceType, out serverClient))
                    {
                        try
                        {
                            desc.ClientId = characterInfo.ClientId;
                            desc.CharacterId = characterInfo.CharacterId;
                            serverClient.SendMessage(desc);
                        }
                        catch (Exception ex)
                        {
                            Logger.WarnException("OnSocketListenerMessageReceivedEx GamePlay WarnException", ex);
                        }
                    }
                    else
                    {
                        Logger.Info("OnSocketListenerMessageReceivedEx GamePlay LoginNotConnet");
                    }
                }
                    break;
            }
        }
    }
}