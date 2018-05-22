#region using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using ActivityClientService;
using ChatClientService;
using DataContract;
using JsonConfig;
using LogicClientService;
using LoginClientService;
using Scorpion;
using NLog;
using ProtoBuf;
using RankClientService;
using SceneClientService;

using Shared;
using TeamClientService;

#endregion

namespace Gate
{
    internal enum ClientAuthorizeState
    {
        NotAuthorized = 0,
        Authorized
    }

    //class CharacterInfo
    //{
    //    public ConcurrentDictionary<int, SocketClient> Servers = 
    //        new ConcurrentDictionary<int, SocketClient>();
    //    public ulong CharacterId = 0;
    //    public ulong ClientId = 0;
    //    public ClientAuthorizeState Authorize = ClientAuthorizeState.NotAuthorized;
    //    public ulong Key = 0;
    //    public ConcurrentQueue<ServiceDesc> Messages = new ConcurrentQueue<ServiceDesc>();
    //}

    public partial class GateServer
    {

		private static GateServer _instance;

        private static readonly Logger ConnectLostLogger = LogManager.GetLogger("ConnectLost");
        private const ulong GateDatePrefix = 0xFFFFFFFF0000UL;
        private const ulong GateIdPrefix = 0xFFFF000000000000UL;
        private static readonly Scorpion.ILogger Logger = new ScorpionLogger(LogManager.GetCurrentClassLogger());
        //private readonly ConcurrentDictionary<int, SocketClient> mFromId2Servers = new ConcurrentDictionary<int, SocketClient>(); // key 逻辑服务器id  value 到逻辑服务器的连接

        //key serviseType, list all servers
        public static ConcurrentDictionary<ServiceType, List<SocketClient>> mFromId2Servers =
            new ConcurrentDictionary<ServiceType, List<SocketClient>>();

        public static ConcurrentDictionary<IPAddress, int> BlackList = new ConcurrentDictionary<IPAddress, int>(); // value 没有用

        public GateServer()
        {
            PackageCount = 0;
	        _instance = this;
        }

		public static GateServer Instance
		{
			get { return _instance ?? (_instance = new GateServer()); }
		}


        private readonly ConcurrentDictionary<uint, int> mCSReceivedPacket = new ConcurrentDictionary<uint, int>();
        private readonly ConcurrentDictionary<uint, int> mCSRepliedPacket = new ConcurrentDictionary<uint, int>();

        private readonly ConcurrentDictionary<ulong, ServerClient> mFromClientId2Client
            = new ConcurrentDictionary<ulong, ServerClient>(); // key 客户端的连接id  value 到客户端器的连接

        private readonly ConcurrentDictionary<int, string> mFromFunctionId2Name =
            new ConcurrentDictionary<int, string>();


        private readonly ConcurrentDictionary<int, string> mCSFunctionId2Name =
            new ConcurrentDictionary<int, string>();

        private readonly ConcurrentDictionary<int, SocketClient> mFromServiceType2Brokers =
            new ConcurrentDictionary<int, SocketClient>(); // key broker的类型 value 到broker的连接

        private int mId;
        private readonly ConcurrentDictionary<uint, int> mMessageCount = new ConcurrentDictionary<uint, int>();
        private readonly ConcurrentDictionary<uint, float> mMessageTime = new ConcurrentDictionary<uint, float>();
        private int mPort;
        private SocketListener mServer;
        private TimeDispatcher mTimeDispatcher;
        private readonly DateTime OriginTime = DateTime.Parse("1970-1-1");

	    public int Id
	    {
			get
			{
				return mId;
			}
	    }

        public int ConnectionCount
        {
            get
            {
                if (mServer == null)
                {
                    return 0;
                }

                return mServer.Clients.Count;
            }
        }

        public ConcurrentDictionary<int, string> FromFunctionId2Name
        {
            get { return mFromFunctionId2Name; }
        }

        public ConcurrentDictionary<uint, int> MessageCount
        {
            get { return mMessageCount; }
        }

        public ConcurrentDictionary<uint, float> MessageTime
        {
            get { return mMessageTime; }
        }

        public long PackageCount { get; private set; }

        public ConcurrentDictionary<uint, int> ReceivedCSMessage
        {
            get { return mCSReceivedPacket; }
        }

        public ConcurrentDictionary<uint, int> RepliedCSMessage
        {
            get { return mCSRepliedPacket; }
        }

        private void AddWaitingMessage(ServiceDesc desc)
        {
            mMessageCount.AddOrUpdate(desc.FuncId, 1, (k, v) => v + 1);

            if (desc.PacketId == 0)
            {
                return;
            }

            if (desc.Type == (int) MessageType.CS)
            {
                mCSReceivedPacket.AddOrUpdate(desc.FuncId, 1, (k, v) => v + 1);
            }

            //             ConcurrentDictionary<uint, WaitingPacket> dict;
            //             if (!mWaitingPacket.TryGetValue(desc.ClientId, out dict))
            //             {
            //                 dict = new ConcurrentDictionary<uint, WaitingPacket>();
            //                 mWaitingPacket.TryAdd(desc.ClientId, dict);
            //             }
            // 
            //             dict.TryAdd(desc.PacketId, new WaitingPacket
            //             {
            //                 Desc = desc,
            //                 Watch = Stopwatch.StartNew()
            //             });
        }

        private void ConnectToBroker(List<dynamic> list)
        {
            foreach (var broker in list)
            {
                if (broker.Type == "GameMaster")
                {
                    continue;
                }

                try
                {
                    var settings = new SocketClientSettings(new IPEndPoint(IPAddress.Parse(broker.Ip), broker.Port), 1024*64);
                    var b = new SocketClient(settings);
                    {
                        b.Connected += () => { OnSocketClientConnectedBroker(b); };

                        b.Disconnected += OnSocketClientDisconnectedBroker;
                        b.MessageReceived += desc => { OnSocketClienMessageReceivedBroker(b, desc); };

                        string service = broker.Type;
                        {
                            ServiceType tempseServiceType;
                            if (!Enum.TryParse(service, out tempseServiceType))
                            {
                                Logger.Error("ServiceType convet faild!! service={0} ", service);
                            }
                            mFromServiceType2Brokers[(int) tempseServiceType] = b;
                        }
                        b.StartConnect();
                    }
                }
                catch (Exception ex)
                {
                    Logger.FatalException(
                        "Broker " + broker.Type + " at " + broker.Ip + ":" + broker.Port + " can not reached.", ex);
                }
            }
        }

        private void ConnectToServer(dynamic configObject)
        {
            foreach (var co in configObject)
            {
                dynamic[] serverList;
                ServiceType serviceType;
                var key = co.Key as string;
                switch (key)
                {
                    case "LoginServer":
                        serverList = configObject.LoginServer;
                        serviceType = ServiceType.Login;
                        break;
                    case "SceneServer":
                        serverList = configObject.SceneServer;
                        serviceType = ServiceType.Scene;
                        break;
                    case "ActivityServer":
                        serverList = configObject.ActivityServer;
                        serviceType = ServiceType.Activity;
                        break;
                    case "LogicServer":
                        serverList = configObject.LogicServer;
                        serviceType = ServiceType.Logic;
                        break;
                    case "RankServer":
                        serverList = configObject.RankServer;
                        serviceType = ServiceType.Rank;
                        break;
                    case "ChatServer":
                        serverList = configObject.ChatServer;
                        serviceType = ServiceType.Chat;
                        break;
                    case "TeamServer":
                        serverList = configObject.TeamServer;
                        serviceType = ServiceType.Team;
                        break;
//                     case "GameMasterServer":
//                         serverList = configObject.GameMasterServer;
//                         break;
                    default:
                        continue;
                }


                foreach (var server in serverList)
                {
                    try
                    {
                        Logger.Debug("Connect to server {0} {1}:{2}", co.Key, server.Ip, server.Port);

                        var settings = new SocketClientSettings(new IPEndPoint(IPAddress.Parse(server.Ip), server.Port), 1024*64);
                        var b = new SocketClient(settings);
                        {
                            b.Connected += () => { OnSocketClientConnectedServer(b); };
                            b.Disconnected += OnSocketClientDisconnectedServer;
                            b.MessageReceived += desc => { OnSocketClientMessageReceivedServer(b, desc); };
                            b.OnException += OnSocketClientExceptionServer;

                            mFromId2Servers.AddOrUpdate(serviceType, arg1 => { return new List<SocketClient> {b}; },
                                (argKey, oldValue) =>
                                {
                                    oldValue.Add(b);
                                    return oldValue;
                                });
                            //mFromId2Servers.TryAdd(server.Id, b);

                            b.StartConnect();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.FatalException(
                            "Server " + server.ServiceName + " at " + server.Ip + ":" + server.Port +
                            " can not reached.",
                            ex);
                    }
                }
            }
        }

        public static SocketClient GetSocketClientByServiceTypeAndCharacter(ServiceType serviceType, ulong characterId)
        {
            List<SocketClient> clients;
            if (mFromId2Servers.TryGetValue(serviceType, out clients))
            {
                var index = (int) (characterId%(ulong) clients.Count);
                return clients[index];
            }
            return null;
        }

        public static SocketClient GetSocketClientByServiceTypeAndIndex(ServiceType serviceType, int index)
        {
            List<SocketClient> clients;
            if (mFromId2Servers.TryGetValue(serviceType, out clients))
            {
                var count = clients.Count;
                if (index >= 0 && index < count)
                {
                    return clients[index];
                }
            }
            return null;
        }

        private ulong GetUniqueClientId(ulong gateId, ulong clientId)
        {
            return (GateIdPrefix & (gateId << 48)) +
                   (GateDatePrefix & ((ulong) ((DateTime.Now - OriginTime).TotalSeconds) << 16)) + (clientId & 0xFFFF);
        }

        private void LogMessage(ulong clientId, ServiceDesc desc, bool client2Server)
        {
#if LOG_MESSAGE
            try
            {
                Logger logger;
                if (!mFromClientId2Logger.TryGetValue(clientId, out logger))
                {
                    logger = LogManager.GetLogger("Gate." + desc.ClientId);
                    mFromClientId2Logger.AddOrUpdate(clientId, logger, (arg1, logger1) => logger1);
                }

                string funcName;
                if (!mFromFunctionId2Name.TryGetValue((int)desc.FuncId, out funcName))
                {
                    funcName = "Other";
                }

                logger.Info("Dir:{5} Service:{0} Func:{1} PacketId:{2} Size:{3} Type:{6} Content:{4}",
                    (ServiceType)desc.ServiceType, funcName,
                    desc.PacketId, desc.Data.Length, Convert.ToBase64String(ProtocolExtension.Serialize(desc)),
                    client2Server ? "In" : "Out", (MessageType)desc.Type);
            }
            catch
            {
            }
#endif
        }

//         private void RemoveAndCleanupClient(ServiceDesc desc, CharacterInfo characterInfo, bool notifyLogin = false)
//         {
// 
//             ConnectLostLogger.Info("client {0} Gate RemoveAndCleanupClient - 1", desc.ClientId);
// 
//             LogMessage(desc.ClientId, desc, false);
//             mMessageCount.AddOrUpdate(desc.FuncId, 1, (k, v) => v + 1);
// 
//             if (characterInfo != null)
//             {
//                 characterInfo.Key = 0;
//                 characterInfo.CharacterId = 0;
//                 characterInfo.Servers.Clear();
//             }
// 
//             NotifyBrokerCharacterLost(desc.ClientId, notifyLogin);
//         }
        private void NotifyBrokerCharacterLost(ulong clientId, bool notifyLogin = true)
        {
            ConnectLostLogger.Info("client {0} Gate NotifyBrokerCharacterLost 1", clientId);

            foreach (var name2Broker in mFromServiceType2Brokers)
            {
                if (name2Broker.Key == (int) ServiceType.Login && !notifyLogin)
                {
                    continue;
                }

                var desc = new ServiceDesc {ServiceType = name2Broker.Key, ClientId = clientId};
                desc.Type = (int) MessageType.Lost;

                try
                {
                    ConnectLostLogger.Info("client {0} - {1} Gate NotifyBrokerCharacterLost 2", desc.ClientId,
                        name2Broker.Key);

                    name2Broker.Value.SendMessage(desc);
                }
                catch (Exception ex)
                {
                    Logger.WarnException("Send message to " + name2Broker.Key + " failed.", ex);
                }
            }
        }

        private void NotifyLoginLost(CharacterInfoEx characterInfo)
        {
            var desc = new ServiceDesc();
            desc.ServiceType = (int) ServiceType.Login;
            desc.ClientId = characterInfo.ClientId;
            desc.CharacterId = characterInfo.CharacterId;
            desc.FuncId = 2033;

            var msgData = new __RPC_Login_GateDisconnect_ARG_uint64_clientId_uint64_characterId__();
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, msgData);
                desc.Data = ms.ToArray();
            }

            desc.PacketId = 0;


            List<SocketClient> serverClient;
            if (mFromId2Servers.TryGetValue(ServiceType.Login, out serverClient))
            {
                serverClient[0].SendMessage(desc);
            }
        }

        private void OnSocketClienMessageReceived20(SocketClient client, ServiceDesc desc)
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

                characterInfo.CharacterId = desc.CharacterId;
                characterInfo.Servers.AddOrUpdate(desc.ServiceType, client, (i, socketClient) => client);

                if (desc.PacketId != 0)
                {
                    SocketClient broker;
                    if (mFromServiceType2Brokers.TryGetValue(desc.ServiceType, out broker))
                    {
                        broker.SendMessage(desc);
                    }
                    else
                    {
                        Logger.Warn("Can not find Broker for {0} {1}. ", desc.ServiceType, key);
                    }
                }
            }
            else
            {
                Logger.Warn("Client {0} is not connected. ", key);
            }
        }

        //直接连接 （broker服务器）收到的消息
        private void OnSocketClienMessageReceivedBroker(SocketClient client, ServiceDesc desc)
        {
            Logger.Debug("Received a message from Broker.");

            //OnSocketClienMessageReceived(client, desc);
            OnSocketClienMessageReceivedEx(client, desc);
        }

        //-----------------------------------------------------SocketClient------------------------
        private void OnSocketClienMessageReceivedSCAll(SocketClient client, ServiceDesc desc)
        {
            desc.Type = (int) MessageType.SC;
            desc.ClientId = 0;
            desc.CharacterId = 0;
            desc.Routing.Clear();
            foreach (var c in mFromClientId2Client)
            {
                LogMessage(c.Key, desc, false);
                mMessageCount.AddOrUpdate(desc.FuncId, 1, (k, v) => v + 1);
                c.Value.SendMessage(desc);
            }
        }

        private void OnSocketClienMessageReceivedSCServerSCList(SocketClient client, ServiceDesc desc)
        {
            desc.Type = (int) MessageType.SC;
            var l = new ulong[desc.Routing.Count];
            desc.Routing.CopyTo(l);
            desc.ClientId = 0;
            desc.CharacterId = 0;
            desc.Routing.Clear();
            foreach (var c in l)
            {
                ServerClient fei;
                if (mFromClientId2Client.TryGetValue(c, out fei))
                {
                    LogMessage(c, desc, false);
                    mMessageCount.AddOrUpdate(desc.FuncId, 1, (k, v) => v + 1);

                    fei.SendMessage(desc);
                }
            }
        }

        private void OnSocketClienMessageReceivedSingleClient(SocketClient client, ServiceDesc desc)
        {
        }

        public void OnSocketClientConnectedBroker(SocketClient b)
        {
            //Logger.Debug("Broker {0} at {1}:{2} Connected.", broker.Type, broker.Ip, broker.Port);
            var desc = new ServiceDesc();
            desc.Type = (int) MessageType.Connect;
            desc.ServiceType = 2;
            desc.ClientId = (ulong) mId;
            b.SendMessage(desc);
        }

        public void OnSocketClientConnectedServer(SocketClient b)
        {
            //Logger.Debug("Server {0} at {1}:{2} Connected.", co.Key, server.Ip, server.Port);
            var desc = new ServiceDesc();
            desc.Type = (int) MessageType.Connect;
            desc.ServiceType = 2;
            desc.ClientId = (ulong) mId;
            b.SendMessage(desc);
        }

        //-----------------------------------------------------SocketClient------------------Broker------
        public void OnSocketClientDisconnectedBroker()
        {
            //Logger.Debug("Broker {0} at {1}:{2} Disconnected.", broker.Type, broker.Ip, broker.Port);
        }

//         private void OnSocketClienMessageReceivedSingle(SocketClient client, ServiceDesc desc)
//         {
//             var key = desc.ClientId;
//             CharacterInfo characterInfo;
//             ServerClient info;
//             if (mFromClientId2Client.TryGetValue(desc.ClientId, out info))
//             {
//                 if (info.Closed != 0)
//                 {
//                     RemoveClient(desc.ClientId);
//                     return;
//                 }
// 
//                 characterInfo = info.UserData as CharacterInfo;
//                 if (characterInfo == null)
//                 {
//                     return;
//                 }
// 
//                 // 这种情况不应该发生，但是发生了。。。
//                 if (characterInfo.ClientId != desc.ClientId)
//                 {
//                     ServerClient sc;
//                     mFromClientId2Client.TryRemove(desc.ClientId, out sc);
//                     NotifyBrokerCharacterLost(desc.ClientId, true);
//                 }
// 
//                if (desc.Type == (int)ServerMessageType.Authorize)
//                 {
//                     Logger.Debug("Authorize client {0}", desc.ClientId);
// 
//                     ConnectLostLogger.Info("client {0} - {1} Gate auth 1", desc.ClientId, characterInfo.CharacterId);
// 
//                     characterInfo.Authorize = ClientAuthorizeState.Authorized;
// 
//                     // notify all brokers except login
//                     foreach (var brokerClient in mFromServiceType2Brokers)
//                     {
//                         if (brokerClient.Key != (int)ServiceType.Login)
//                         {
//                             desc = new ServiceDesc();
//                             desc.Type = (int)MessageType.Connect;
//                             desc.ClientId = key;
//                             try
//                             {
//                                 LogMessage(key, desc, false);
//                                 mMessageCount.AddOrUpdate(desc.FuncId, 1, (k, v) => v + 1);
// 
//                                 ConnectLostLogger.Info("client {0} - {1} - {2} Gate auth 2", desc.ClientId, characterInfo.CharacterId, brokerClient.Key);
// 
//                                 brokerClient.Value.SendMessage(desc);
//                             }
//                             catch (Exception ex)
//                             {
//                                 Logger.WarnException("Send message failed. 10", ex);
//                             }
//                         }
//                     }
// 
//                     ServerClient c;
//                     if (mFromClientId2Client.TryGetValue(desc.ClientId, out c))
//                     {
//                         characterInfo.Key = ((ulong)MyRandom.Random(int.MaxValue) << 32) + (ulong)MyRandom.Random(int.MaxValue);
//                         desc = new ServiceDesc();
//                         desc.ClientId = key;
//                         desc.CharacterId = characterInfo.Key;
//                         desc.Type = (int)ServerMessageType.Authorize;
// 
//                         ConnectLostLogger.Info("client {0} - {1} Gate auth 3", key, characterInfo.CharacterId);
// 
//                         c.SendMessage(desc);
//                     }
// 
//                     // 这个客户端不能再重连回来了
//                     if (mReconnectClients.TryRemove(desc.ClientId, out characterInfo))
//                     {
//                         Logger.Info("Remove Client: " + desc.ClientId + " from reconnecting [1].");
//                     }
//                 }
//                 // 服务器主动要求断开连接
//                 else if (desc.Type == (int)MessageType.SC && 
//                             desc.ServiceType == (int)ServiceType.Login &&
//                             desc.FuncId == 2010) // Kick
//                 {
//                     try
//                     {
//                         info.SendMessage(desc);
// 
//                         // 不能让客户端重连回来了，已经要踢掉了
//                         CharacterInfo temp;
//                         if (mReconnectClients.TryRemove(desc.ClientId, out temp))
//                         {
//                             Logger.Info("Remove Client: " + desc.ClientId + " from reconnecting [2].");
//                         }
// 
//                         // 通知Broker掉线
//                         Logger.Info("Disconnect Client:{0} characterId:{1} by server.", desc.ClientId, desc.CharacterId);
//                         RemoveAndCleanupClient(desc, characterInfo, true);
// 
//                         // 如果直接断开连接，会导致客户端收不到包
//                         mTimeDispatcher.RegisterTimedEvent(TimeSpan.FromSeconds(1), () =>
//                         {
//                             ServerClient sc;
// 
//                             Logger.Info("Disconnect Client: " + desc.ClientId + " by server after 1 sec.");
// 
//                             if (mFromClientId2Client.TryRemove(desc.ClientId, out sc))
//                             {
//                                 sc.UserData = null;
//                             }
// 
//                             info.Disconnect();
//                         });
//                     }
//                     catch (Exception ex)
//                     {
//                         Logger.WarnException("Send kick message failed. ", ex);
//                     }
//                 }
//                 // 角色退出
//                 else if (desc.Type == (int)MessageType.SC && desc.ServiceType == (int)ServiceType.Login &&
//                          desc.FuncId == 2011) // Logout
//                 {
//                     try
//                     {
//                         // 不能让客户端重连回来了，已经要踢掉了
//                         CharacterInfo temp;
//                         if (mReconnectClients.TryRemove(desc.ClientId, out temp))
//                         {
//                             Logger.Info("Remove Client: " + desc.ClientId + " from reconnecting [3].");
//                         }
// 
//                         // 通知Broker掉线
//                         Logger.Info("Disconnect Client:{0} characterId:{1} by server.", desc.ClientId, desc.CharacterId);
//                         RemoveAndCleanupClient(desc, characterInfo);
//                     }
//                     catch (Exception ex)
//                     {
//                         Logger.WarnException("Send kick message failed. ", ex);
//                     }
//                 }
//                 // 正常的CS的回复
//                 else
//                 {
//                     try
//                     {
//                         LogMessage(key, desc, false);
//                         RemoveWaitingMessage(desc);
// 
//                         desc.ClientId = 0;
//                         desc.CharacterId = 0;
//                         desc.Routing.Clear();
// 
//                         info.SendMessage(desc);
//                     }
//                     catch (Exception ex)
//                     {
//                         Logger.WarnException("Send message failed. 11", ex);
//                     }
//                 }
//             }
//             else if (mReconnectClients.TryGetValue(desc.ClientId, out characterInfo))
//             {
//                 if (desc.Type == (int)ServerMessageType.Authorize)
//                 {
//                     Logger.Error("should not here.");
//                 }
//                 else if (desc.Type == (int)MessageType.SC && desc.ServiceType == (int)ServiceType.Login &&
//                          desc.FuncId == 2010) // Kick
//                 {
//                     Logger.Info("Remove Client: " + desc.ClientId + " from reconnecting 2010.");
//                     mReconnectClients.TryRemove(desc.ClientId, out characterInfo);
//                     RemoveAndCleanupClient(desc, characterInfo, true);
//                 }
//                 else if (desc.Type == (int)MessageType.SC && desc.ServiceType == (int)ServiceType.Login &&
//                          desc.FuncId == 2011) // Logout
//                 {
//                     Logger.Info("Remove Client: " + desc.ClientId + " from reconnecting 2011.");
//                     mReconnectClients.TryRemove(desc.ClientId, out characterInfo);
//                     RemoveAndCleanupClient(desc, characterInfo, true);
//                 }
//                 else
//                 {
//                     if (desc.ServiceType != (int)ServiceType.Scene)
//                     {
//                         characterInfo.Messages.Enqueue(desc);
//                     }
//                 }
//             }
//             // 如果这个客户端不存在，则向服务器发起断开连接
//             else
//             {
//                 //RemoveAndCleanupClient(desc, characterInfo, true);
//             }
//         }
//        private void OnSocketClienMessageReceived(SocketClient client, ServiceDesc desc)
//        {

////             if (desc.ServiceType == 99999)
////             {
////                 ServerClient fei;
////                 if (mFromClientId2Client.TryGetValue(desc.Routing[desc.Routing.Count - 1], out fei))
////                 {
////                     LogMessage(key, desc, false);
////                     mMessageCount.AddOrUpdate(desc.FuncId, 1, (k, v) => v + 1);
//// 
////                     desc.Routing.RemoveAt(desc.Routing.Count - 1);
//// 
////                     fei.SendMessage(desc);
////                 }
////                 return;
////             }
//            if (desc.Type == 20)
//            {
//                OnSocketClienMessageReceived20(client, desc); 
//                return;
//            }
//            else if (desc.Type == (int)MessageType.SCAll)
//            {
//                OnSocketClienMessageReceivedSCAll(client, desc);
//                return;
//            }
//            else if (desc.Type == (int)MessageType.SCServer || desc.Type == (int)MessageType.SCList)
//            {
//                OnSocketClienMessageReceivedSCServerSCList(client, desc);
//                return;
//            }
//            else
//            {
//                OnSocketClienMessageReceivedSingle(client, desc);    
//            }

//        }

        //-----------------------------------------------------SocketClient------------------Server------------
        public void OnSocketClientDisconnectedServer()
        {
            //Logger.Debug("Server {0} at {1}:{2} Disconnected.", co.Key, server.Ip, server.Port);
        }

        private void OnSocketClientExceptionServer(Exception exception)
        {
            //Logger.Error("Connect to server error " + co.Key, exception);
        }

        //直接连接 （子服务器）收到的消息
        private void OnSocketClientMessageReceivedServer(SocketClient client, ServiceDesc desc)
        {
            Logger.Debug("Received a message from Server.");

            //OnSocketClienMessageReceived(client, desc);
            OnSocketClienMessageReceivedEx(client, desc);

	        
		    GateServerMonitor.SendPacketNumber.Mark();
			if (null != desc && null != desc.Data)
	        {
				GateServerMonitor.SendPacketSize.Mark(desc.Data.Length);
	        }
        }

        private void OnSocketClientOnReconnectStartBroker(int count)
        {
            //             Logger.Warn("Start reconnect to server {0} {1}:{2} {3} times.", co.Key, server.Ip,
            //                  server.Port, count);
        }

        private void OnSocketClientOnReconnectStartServer(int count)
        {
//             Logger.Warn("Start reconnect to server {0} {1}:{2} {3} times.", co.Key, server.Ip,
//                  server.Port, count);
        }

        //-----------------------------------------------------SocketListener------------------------

//         private void OnSocketListenerConnected(ServerClient sender)
//         {
//             sender.MessageReceived += OnSocketListenerMessageReceived;
// 
//             ulong key = GetUniqueClientId((uint) mId, sender.ClientId);
// 
//             ConnectLostLogger.Info("client {0} {1} Gate ClientConnected - 1", key, sender.RemoteEndPoint);
// 
//             var characterInfo = new CharacterInfo();
//             characterInfo.Authorize = ClientAuthorizeState.NotAuthorized;
//             characterInfo.ClientId = key;
// 
//             Logger.Info("Client: " + key + " connected.");
// 
//             sender.UserData = characterInfo;
//             mFromClientId2Client.AddOrUpdate(characterInfo.ClientId, sender, (l, arg2) => sender);
// 
//             // Notify Login
//             SocketClient client;
//             if (mFromServiceType2Brokers.TryGetValue((int)ServiceType.Login, out client))
//             {
//                 var desc = new ServiceDesc { ServiceType = (int)ServiceType.Login, ClientId = key };
//                 desc.Type = (int)MessageType.Connect;
// 
//                 try
//                 {
// 
//                     ConnectLostLogger.Info("client {0} Gate ClientConnected - 2", key);
//                     client.SendMessage(desc);
//                 }
//                 catch (Exception ex)
//                 {
//                     Logger.WarnException("Send message to Login failed.", ex);
//                 }
//             }
//             else
//             {
//                 ConnectLostLogger.Info("client {0} Gate ClientConnected - 3", key);
//                 Logger.Fatal("Login service can not found.");
//             }
// 
//             ConnectLostLogger.Info("client {0} Gate ClientConnected - 4", key);
//         }
        /// <summary>
        ///     This function will be called in multithread, so THREAD SAFE is very important.
        /// </summary>
        /// <param name="sender"></param>
        private void OnSocketListenerDisconnected(ServerClient sender)
        {
            if (sender == null)
            {
                Logger.Debug("ClientDisconnected: sender is null");
                return;
            }

            var characterInfo = (CharacterInfoEx) sender.UserData;
            if (characterInfo == null)
            {
                Logger.Debug("ClientDisconnected: sender.UserData is null");
                return;
            }

            var clientId = characterInfo.ClientId;

            //Logger.Fatal("Gate Disconnect ------ 1---{0} -- {1}", DateTime.Now.ToBinary(), clientId);

            ConnectLostLogger.Info("client {0} Gate Lost 1", clientId);
            Logger.Info("Client: {0} disconnected.", clientId);

            ServerClient oldSender = null;

            if (mFromClientId2Client.TryGetValue(clientId, out oldSender))
            {
//老clientId找到了，需要判断链接是否已经被重连替换成新的
                if (oldSender != sender)
                {
//新的告诉login，老clientId需要 忽视断线的变量 --
                    //NotifyLoginLost(characterInfo);
                    //Logger.Fatal("Gate Disconnect ------ 2---{0} -- {1}", DateTime.Now.ToBinary(), clientId);
                    Logger.Info("Client: {0} has resconnected.", clientId);
                }
                else
                {
                    //老的，那么需要移除ClientId
                    //告诉login切换状态
                    RemoveClient(clientId);
                    //Logger.Fatal("Gate Disconnect ------ 3---{0} -- {1}", DateTime.Now.ToBinary(), clientId);
                    NotifyLoginLost(characterInfo);
                }
            }
        }

        private void OnSocketListenerMessageReceivedReconnect(ServerClient client, ServiceDesc desc)
        {
        }

//         private void NotifyLoginCharacterLost(ulong clientId)
//         {
//             SocketClient client;
//             if (mFromServiceType2Brokers.TryGetValue((int) ServiceType.Login, out client))
//             {
//                 var desc = new ServiceDesc {ServiceType = (int) ServiceType.Login, ClientId = clientId};
//                 desc.Type = (int) MessageType.Lost;
// 
//                 try
//                 {
//                     client.SendMessage(desc);
//                 }
//                 catch (Exception ex)
//                 {
//                     Logger.WarnException("Send message to " + ServiceType.Login + " failed.", ex);
//                 }
//             }
//         }

        private void RemoveClient(ulong clientId)
        {
            ServerClient info;
            if (mFromClientId2Client.TryRemove(clientId, out info))
            {
                if (info.UserData == null)
                {
                    Logger.Error("RemoveClient not find uesr! clientId={0}", clientId);
                    return;
                }

                var characterInfo = (CharacterInfoEx) info.UserData;
                if (characterInfo == null)
                {
                    Logger.Error("RemoveClient  uesrdata faild! clientId={0}", clientId);
                    return;
                }
                info.UserData = null;

                ConnectLostLogger.Info("client {0} Gate Lost 2", clientId);
                if (characterInfo.mState == GateClientState.NotAuthorized)
                {
                    return;
                }
                ConnectLostLogger.Info("client {0} Gate Lost 6", clientId);
                Logger.Info("Notify broker Client: " + clientId + " disconnected.");
                ConnectLostLogger.Info("client {0} Gate Lost 7", clientId);
            }
            ConnectLostLogger.Info("client {0} Gate Lost 8", clientId);
        }

        private void RemoveWaitingMessage(ServiceDesc desc)
        {
            mMessageCount.AddOrUpdate(desc.FuncId, 1, (k, v) => v + 1);

            if (desc.PacketId == 0)
            {
                return;
            }

            if (desc.Type == (int) MessageType.CS)
            {
                mCSRepliedPacket.AddOrUpdate(desc.FuncId, 1, (k, v) => v + 1);
            }

            // 
            //             ConcurrentDictionary<uint, WaitingPacket> dict;
            //             if (!mWaitingPacket.TryGetValue(desc.ClientId, out dict))
            //             {
            //                 return;
            //             }
            // 
            //             WaitingPacket packet;
            //             if (dict.TryRemove(desc.PacketId, out packet))
            //             {
            //                 mMessageTime.AddOrUpdate(desc.FuncId, packet.Watch.ElapsedMilliseconds, (k, v) => v + packet.Watch.ElapsedMilliseconds);
            //             }
        }

        //-----------------------------------------------------Base------------------------
        public void Start(string[] args)
        {
            if (!int.TryParse(args[0], out mId))
            {
                Logger.Warn(@"Gate server Start() Faild! args[0]={0}", args[0]);
                return;
            }

            if (!int.TryParse(args[1], out mPort))
            {
                Logger.Warn(@"Gate server Start() Faild! args[1]={0}", args[1]);
                return;
            }

            Utility.IsGate = true;

            dynamic User = Config.ApplyJson(args[3]);

            var list = new List<dynamic>();
            foreach (var name in GlobalVariable.ServerNames)
            {
                if (User.ContainsKey(name))
                {
                    list.Add(User[name]);
                }
            }

            AddFunctionNameActivity.AddFunctionName(mFromFunctionId2Name);
            AddFunctionNameChat.AddFunctionName(mFromFunctionId2Name);
            AddFunctionNameLogic.AddFunctionName(mFromFunctionId2Name);
            AddFunctionNameLogin.AddFunctionName(mFromFunctionId2Name);
            AddFunctionNameRank.AddFunctionName(mFromFunctionId2Name);
            AddFunctionNameScene.AddFunctionName(mFromFunctionId2Name);
            AddFunctionNameTeam.AddFunctionName(mFromFunctionId2Name);

            AddFunctionNameActivity.AddCSFunctionName(mCSFunctionId2Name);
            AddFunctionNameChat.AddCSFunctionName(mCSFunctionId2Name);
            AddFunctionNameLogic.AddCSFunctionName(mCSFunctionId2Name);
            AddFunctionNameLogin.AddCSFunctionName(mCSFunctionId2Name);
            AddFunctionNameRank.AddCSFunctionName(mCSFunctionId2Name);
            AddFunctionNameScene.AddCSFunctionName(mCSFunctionId2Name);
            AddFunctionNameTeam.AddCSFunctionName(mCSFunctionId2Name);

            mTimeDispatcher = new TimeDispatcher("GateServerTimer");
            mTimeDispatcher.Start();

            try
            {
                var settings = new SocketSettings(5000, 200, new IPEndPoint(IPAddress.Any, mPort));

                settings.Compress = true;
                mServer = new SocketListener(settings);

                //mServer.ClientConnected += OnSocketListenerConnected;
                mServer.ClientConnected += OnSocketListenerConnectedEx;
                mServer.ClientDisconnected += OnSocketListenerDisconnected;

                mServer.StartListen();
            }
            catch (Exception ex)
            {
                Logger.FatalException("Server start failed.", ex);
            }

            //Thread.Sleep(5000);

            ConnectToServer(Config.ApplyJson(args[5]));

            ConnectToBroker(list);

            Console.WriteLine("GateServer startOver.");
        }

        public void Status(ConcurrentDictionary<string, string> dict)
        {
            if (mServer == null)
            {
                return;
            }
            try
            {
                dict.TryAdd("_Listening", mServer.IsListening.ToString());
                //    dict.TryAdd("ByteReceivedPerSecond", mServer.ByteReceivedPerSecond.ToString());
                //    dict.TryAdd("ByteSendPerSecond", mServer.ByteSendPerSecond.ToString());
                //    dict.TryAdd("MessageReceivedPerSecond", mServer.MessageReceivedPerSecond.ToString());
                //    dict.TryAdd("MessageSendPerSecond", mServer.MessageSendPerSecond.ToString());

                //    dict.TryAdd("MaxByteReceivedPerSecond", mServer.MaxByteReceivedPerSecond.ToString());
                //    dict.TryAdd("MaxByteSendPerSecond", mServer.MaxByteSendPerSecond.ToString());
                //    dict.TryAdd("MaxMessageReceivedPerSecond", mServer.MaxMessageReceivedPerSecond.ToString());
                //    dict.TryAdd("MaxMessageSendPerSecond", mServer.MaxMessageSendPerSecond.ToString());

                //    dict.TryAdd("ConnectionCount", mServer.ConnectionCount.ToString());
                //    dict.TryAdd("AcceptPoolCount", mServer.AcceptPoolCount.ToString());
                //    dict.TryAdd("SendRecvPoolCount", mServer.SendRecvPoolCount.ToString());
                //    dict.TryAdd("WaitingSendMessageCount", mServer.WaitingSendMessageCount.ToString());

                //    dict.TryAdd("mFromServiceType2Brokers", mFromServiceType2Brokers.Count.ToString());
                //    dict.TryAdd("mFromId2Servers", mFromId2Servers.Count.ToString());
                //    dict.TryAdd("mFromClientId2Client", mFromClientId2Client.Count.ToString());
                //    dict.TryAdd("mMessageCount", mMessageCount.Count.ToString());
                //    dict.TryAdd("mMessageTime", mMessageTime.Count.ToString());
                //    dict.TryAdd("mCSReceivedPacket", mCSReceivedPacket.Count.ToString());
                //    dict.TryAdd("mCSRepliedPacket", mCSRepliedPacket.Count.ToString());
                //    dict.TryAdd("mFromFunctionId2Name", mFromFunctionId2Name.Count.ToString());
                //    dict.TryAdd("TotalUsedMemory", mServer.TotalUsedMemory.ToString());

                //    foreach (var client in mFromClientId2Client)
                //    {
                //        dict.TryAdd("Client Send - " + client.Key, client.Value.SendUseMemory.ToString());
                //        dict.TryAdd("Client Receive - " + client.Key, client.Value.ReceiveUseMemory.ToString());
                //        //dict.TryAdd("Client Serialize - " + client.Key, client.Value.SerializeUseMemory.ToString());
                //    }

                //    var index = 0;
                //    foreach (var agents in mFromId2Servers)
                //    {
                //        foreach (var agent in agents.Value)
                //        {
                //            dict.TryAdd(agents.Key + " Latency", agent.Latency.ToString());
                //            dict.TryAdd(agents.Key + " ByteReceivedPerSecond", agent.ByteReceivedPerSecond.ToString());
                //            dict.TryAdd(agents.Key + " ByteSendPerSecond", agent.ByteSendPerSecond.ToString());
                //            dict.TryAdd(agents.Key + " MessageReceivedPerSecond", agent.MessageReceivedPerSecond.ToString());
                //            dict.TryAdd(agents.Key + " MessageSendPerSecond", agent.MessageSendPerSecond.ToString());
                //        }
                //        index++;
                //    }
            }
            catch (Exception ex)
            {
                Logger.Error("Gate Status Error!{0}", ex);
            }


            //             for (int i = 0; i < 55; i++)
            //             {
            //                 sb.AppendLine("Debug-" + i + ":" + mServer.DebugStatus[i]);
            //             }
        }

        public void Stop()
        {
            mTimeDispatcher.Stop();
            mServer.Stop();

            foreach (var client in mFromServiceType2Brokers)
            {
                client.Value.Stop();
            }
        }
    }

    public class Gate : IServer
    {
        private List<string> args;
        //private string mConfigFile;

        private DateTime LastSaveStat;
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly GateServer mServer = new GateServer();

        public void Init(string[] args)
        {
            this.args = new List<string>(args);
            LastSaveStat = DateTime.Now;
        }

        public void Start()
        {
            mServer.Start(args.ToArray());
        }

        public void Rescue()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            mServer.Stop();
        }

        public void Status(ConcurrentDictionary<string, string> dict)
        {
            foreach (var v in mServer.MessageCount)
            {
                string funcName;
                if (!mServer.FromFunctionId2Name.TryGetValue((int) v.Key, out funcName))
                {
                    funcName = v.Key.ToString();
                }

                KeyValuePair<int, float> k;
                var funcTime = 0.0f;
                if (InMessage.FunctionTime.TryGetValue(v.Key, out k))
                {
                    funcTime = k.Value/k.Key;
                }

                var recv = -1;
                var repl = -1;
                var strRecv = string.Empty;
                var strRepl = string.Empty;
                if (mServer.ReceivedCSMessage.TryGetValue(v.Key, out recv))
                {
                    strRecv = "Recv:" + recv;
                }
                if (mServer.RepliedCSMessage.TryGetValue(v.Key, out repl))
                {
                    strRepl = "Repl:" + repl;
                }

                float time;
                if (mServer.MessageTime.TryGetValue(v.Key, out time))
                {
                    dict.TryAdd(string.Format("Func {0} Count", funcName), v.Value.ToString());
                    dict.TryAdd(string.Format("Func {0} AvgTime", funcName), (time/v.Value).ToString());
                    dict.TryAdd(string.Format("Func {0} FuncTime", funcName), funcTime.ToString());
                }
                else
                {
                    dict.TryAdd(string.Format("Func {0} Count", funcName), v.Value.ToString());
                }
            }

            if ((DateTime.Now - LastSaveStat).TotalMinutes > 1)
            {
                LastSaveStat = DateTime.Now;

                foreach (var mc in mServer.MessageCount)
                {
                    mServer.MessageCount[mc.Key] = 0;
                }

                foreach (var mt in mServer.MessageTime)
                {
                    mServer.MessageTime[mt.Key] = 0;
                }
            }

            mServer.Status(dict);
        }
    }
}