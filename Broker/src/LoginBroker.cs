#region using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Scorpion;
using NLog;


#endregion

namespace Broker
{
    internal class LoginBroker : IBrokerBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly List<SocketClient> mBackEnds = new List<SocketClient>();

        private readonly ConcurrentDictionary<ulong, ServerClient> mFromClientId2Gate =
            new ConcurrentDictionary<ulong, ServerClient>();

        private readonly ConcurrentDictionary<ulong, ClientRouting> mFromClientId2Login =
            new ConcurrentDictionary<ulong, ClientRouting>();

        private SocketListener mFrontEndServer;
        private long mPackageCount;
        private int mPort;
        private readonly Random mRandomNumber = new Random();
        private string mServiceName;
        //---------------------SocketClient--------
        public void OnSocketClientConnected(SocketClient b)
        {
            //Logger.Debug("Server {0} at {1}:{2} Connected.", type, serverItem.Ip, serverItem.Port);
            var desc = new ServiceDesc();
            desc.Type = (int) MessageType.Connect;
            desc.ServiceType = 1;
            b.SendMessage(desc);
        }

        public void OnSocketClientDisconnected()
        {
            //Logger.Debug("Server {0} at {1}:{2} Disconnected.", mServiceName, serverItem.Ip, serverItem.Port);
        }

        public void OnSocketClientException(Exception exception)
        {
            Logger.Error(exception, "Connect to server error {0}", mServiceName);
        }

        //---------------------SocketListener--------
        public void OnSocketListenerException(Exception exception)
        {
            Logger.Error(exception, "Start listen Error");
        }

        public void OnSocketListenerMessageReceiveConnect(ServerClient client, ServiceDesc desc)
        {
            //var clientId = desc.ClientId;
            //var characterId = desc.CharacterId;
            ClientRouting clientRouting = null;
            if (desc.ServiceType == 2)
            {
                client.UserData = desc.ClientId;
                return;
            }

            if (!mFromClientId2Login.TryGetValue(desc.ClientId, out clientRouting))
            {
                var index = mRandomNumber.Next(mBackEnds.Count);
                var server = mBackEnds[index];

                clientRouting = new ClientRouting
                {
                    Server = server,
                    LastMessageTime = DateTime.Now
                };

                mFromClientId2Login[desc.ClientId] = clientRouting;
            }

            mFromClientId2Gate[desc.ClientId] = client;
            desc.Routing.Add((ulong) client.UserData);

            try
            {
                clientRouting.Server.SendMessage(desc);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Send message failed. 5");
            }
        }

        public void OnSocketListenerMessageReceiveLost(ServerClient client, ServiceDesc desc)
        {
            var clientId = desc.ClientId;
            var characterId = desc.CharacterId;

            ClientRouting clientRouting = null;
            if (!mFromClientId2Login.TryRemove(clientId, out clientRouting))
            {
                Logger.Warn("Can not find server for client: " + clientId);
                return;
            }

            try
            {
                clientRouting.Server.SendMessage(desc);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Send message failed. 6");
            }
        }

        //-------------------------------------------------流程----

        public void Start(int id, int nPort, string type, dynamic[] serverList)
        {
            //dynamic User = Config.ApplyJsonFromPath(config);
            //mId = User.Id;
            mServiceName = type;
            mPort = nPort;

            try
            {
                Logger.Debug("Start listen at:{0}", mPort);

                var settings = new SocketSettings(100, 100, new IPEndPoint(IPAddress.Any, mPort));
                mFrontEndServer = new SocketListener(settings);

                mFrontEndServer.ClientConnected += ClientConnected;
                mFrontEndServer.ClientDisconnected += ClientDisconnected;
                mFrontEndServer.OnException += OnSocketListenerException;

                mFrontEndServer.StartListen();
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "Server start failed.");
            }

            //Thread.Sleep(1000);

            foreach (var server in serverList)
            {
                try
                {
                    Logger.Debug("Connect to server {0} {1}:{2}", mServiceName, server.Ip, server.Port);
                    var serverItem = server;
                    var settings = new SocketClientSettings(new IPEndPoint(IPAddress.Parse(server.Ip), server.Port), 1024*64);
                    var b = new SocketClient(settings);
                    b.Connected += () => { OnSocketClientConnected(b); };
                    b.Disconnected += OnSocketClientDisconnected;
                    b.OnException += OnSocketClientException;
                    b.MessageReceived += ServerOnMessageReceived;

                    mBackEnds.Add(b);
                    b.StartConnect();
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex, 
                        "Server {0} at {1}:{2} can not reached.", type, server.Ip, server.Port);
                }
            }
        }

        public void Stop()
        {
            mFrontEndServer.Stop();
            foreach (var client in mBackEnds)
            {
                client.Stop();
            }
        }

        public void Status(ConcurrentDictionary<string, string> dict)
        {
            try
            {
                if(mFrontEndServer != null)
                    dict.TryAdd("_Listening", mFrontEndServer.IsListening.ToString());
            //    dict.TryAdd("ByteReceivedPerSecond", mFrontEndServer.ByteReceivedPerSecond.ToString());
            //    dict.TryAdd("ByteSendPerSecond", mFrontEndServer.ByteSendPerSecond.ToString());
            //    dict.TryAdd("MessageReceivedPerSecond", mFrontEndServer.MessageReceivedPerSecond.ToString());
            //    dict.TryAdd("MessageSendPerSecond", mFrontEndServer.MessageSendPerSecond.ToString());
            //    dict.TryAdd("ConnectionCount", mFrontEndServer.ConnectionCount.ToString());

            //    for (var i = 0; i < mBackEnds.Count; i++)
            //    {
            //        var backEnd = mBackEnds[i];
            //        dict.TryAdd("Server" + i + " Latency", backEnd.Latency.ToString());
            //        dict.TryAdd("Server" + i + " ByteReceivedPerSecond", backEnd.ByteReceivedPerSecond.ToString());
            //        dict.TryAdd("Server" + i + " ByteSendPerSecond", backEnd.ByteSendPerSecond.ToString());
            //        dict.TryAdd("Server" + i + " MessageReceivedPerSecond", backEnd.MessageReceivedPerSecond.ToString());
            //        dict.TryAdd("Server" + i + " MessageSendPerSecond", backEnd.MessageSendPerSecond.ToString());
            //    }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "LoginBroker Status Error!{0}");
            }
        }

        /// <summary>
        ///     This function will be called in multithread, so THREAD SAFE is very important.
        /// </summary>
        /// <param name="client"></param>
        public void ClientDisconnected(ServerClient client)
        {
            Logger.Debug("Gate: " + client.ClientId + " disconnected.");
            //mFromClientIdToClient.TryRemove(client.ClientId, out client);
        }

        public void ClientConnected(ServerClient client)
        {
            Logger.Debug("Gate: " + client.ClientId + " connected.");
            client.MessageReceived += ClientMessageReceived;
            //mFromClientIdToClient.AddOrUpdate(client.ClientId, client, (arg1, serverClient) => client);
        }

        /// <summary>
        ///     This function will be called in multithread, so THREAD SAFE is very important.
        ///     Called when gate pass a message, or other server pass a message.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="desc"></param>
        public void ClientMessageReceived(ServerClient client, ServiceDesc desc)
        {
            try
            {
                var clientId = desc.ClientId;
                var characterId = desc.CharacterId;

                Logger.Debug("Recieved a message from client {0}", clientId);

                ClientRouting clientRouting = null;

                if (desc.Type == (int) MessageType.CS || desc.Type == (int) MessageType.Sync)
                {
                    if (!mFromClientId2Login.TryGetValue(desc.ClientId, out clientRouting))
                    {
                        Logger.Warn("Can not find client {0}. ", desc.ClientId);
                        return;
                    }

                    try
                    {
                        // 只要消息经过这里，就给Gate回复一个这个消息，后面的消息就不应该到这里了，Gate会直接发送到Server上。
                        var gateDesc = new ServiceDesc();
                        gateDesc.Type = 20;
                        gateDesc.CharacterId = characterId;
                        gateDesc.ServiceType = desc.ServiceType;
                        gateDesc.ClientId = desc.ClientId;
                        gateDesc.Routing.Add((ulong) client.UserData);
                        clientRouting.Server.SendMessage(gateDesc);

                        clientRouting.Server.SendMessage(desc);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, "Send message failed. 4");
                    }

                    clientRouting.LastMessageTime = DateTime.Now;

                    Interlocked.Increment(ref mPackageCount);
                }
                else if (desc.Type == (int) MessageType.SS)
                {
                    ClientRouting server;
                    if (mFromClientId2Login.TryGetValue(clientId, out server))
                    {
                        desc.Routing.Add(client.ClientId);
                        server.Server.SendMessage(desc);
                        return;
                    }

                    if (clientId == 0)
                    {
                        var index = mRandomNumber.Next(mBackEnds.Count);
                        var backend = mBackEnds[index];
                        desc.Routing.Add(client.ClientId);
                        backend.SendMessage(desc);
                        return;
                    }

                    Logger.Warn("Can not find client id for ss message. {0}", clientId);
                }
                else if (desc.Type == (int) MessageType.SAS)
                {
                    desc.Routing.Add(client.ClientId);
                    var reply = new ServiceDesc();
                    reply.Type = (int) MessageType.SASReply;
                    reply.CharacterId = desc.CharacterId;
                    reply.ServiceType = desc.ServiceType;
                    reply.ClientId = desc.ClientId;
                    reply.PacketId = desc.PacketId;
                    reply.FuncId = desc.FuncId;
                    ulong i = 0;
                    foreach (var socketClient in mBackEnds)
                    {
                        socketClient.SendMessage(desc);
                        i++;
                    }
                    reply.Routing.Add(i);
                    client.SendMessage(reply);
                }
                else if (desc.Type == (int) MessageType.SCAll)
                {
                    var gates = mFromClientId2Gate.Values.Distinct();
                    foreach (var gate in gates)
                    {
                        gate.SendMessage(desc);
                    }
                }
                else if (desc.Type == (int) MessageType.SCServer)
                {
                    Logger.Error("B2S is not supported in login server.");
                }
                else if (desc.Type == (int) MessageType.SCList)
                {
                    var clients = desc.Routing;

                    var dictionary = new Dictionary<ServerClient, List<ulong>>();
                    foreach (var v in clients)
                    {
                        var gate = mFromClientId2Gate[v];
                        List<ulong> c;
                        if (!dictionary.TryGetValue(gate, out c))
                        {
                            c = new List<ulong>();
                            dictionary.Add(gate, c);
                        }

                        c.Add(v);
                    }

                    desc.Routing.Clear();
                    foreach (var gate in dictionary)
                    {
                        var msg = new ServiceDesc();
                        msg.Data = desc.Data;
                        msg.ClientId = desc.ClientId;
                        msg.PacketId = desc.PacketId;
                        msg.FuncId = desc.FuncId;
                        msg.ServiceType = desc.ServiceType;
                        msg.Error = desc.Error;
                        msg.Type = desc.Type;
                        msg.CharacterId = desc.CharacterId;
                        msg.Routing.AddRange(gate.Value);
                        gate.Key.SendMessage(msg);
                    }
                }
                else if (desc.Type == (int) MessageType.Connect)
                {
                    OnSocketListenerMessageReceiveConnect(client, desc);
                }
                else if (desc.Type == (int) MessageType.Lost)
                {
                    OnSocketListenerMessageReceiveLost(client, desc);
                }
//                 else if (desc.ServiceType == 99999)
//                 {
//                     desc.Routing.Add(client.ClientId);
//                     foreach (var end in mBackEnds)
//                     {
//                         end.SendMessage(desc);
//                     }
//                 }
                else
                {
                    Logger.Warn("Login broker can not deal with this message type: " + desc.Type);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Some error inside ClientMessageReceived ");
            }
        }

        /// <summary>
        ///     This function will be called in multithread, so THREAD SAFE is very important.
        /// </summary>
        /// <param name="desc"></param>
        public void ServerOnMessageReceived(ServiceDesc desc)
        {
            try
            {
                Logger.Debug("Recieved a message from server ");

//                 if (desc.ServiceType == 99999)
//                 {
//                     ServerClient c;
//                     if (mFromClientIdToClient.TryGetValue(desc.Routing[desc.Routing.Count - 1], out c))
//                     {
//                         desc.Routing.RemoveAt(desc.Routing.Count - 1);
//                         c.SendMessage(desc);
//                     }
// 
//                     return;
//                 }
                if (desc.Type == (int) MessageType.SS || desc.Type == (int) MessageType.SAS)
                {
                    mFrontEndServer.Clients[desc.Routing[desc.Routing.Count - 1]].SendMessage(desc);
                    return;
                }
                if (desc.Type == (int) MessageType.SCAll)
                {
                    var gates = mFromClientId2Gate.Values.Distinct();
                    foreach (var gate in gates)
                    {
                        gate.SendMessage(desc);
                    }
                    return;
                }
                if (desc.Type == (int) MessageType.SCServer)
                {
                    Logger.Error("B2S is not supported in login server.");
                    return;
                }
                if (desc.Type == (int) MessageType.SCList)
                {
                    var clients = desc.Routing;

                    var dictionary = new Dictionary<ServerClient, List<ulong>>();
                    foreach (var v in clients)
                    {
                        var gate = mFromClientId2Gate[v];
                        List<ulong> c;
                        if (!dictionary.TryGetValue(gate, out c))
                        {
                            c = new List<ulong>();
                            dictionary.Add(gate, c);
                        }

                        c.Add(v);
                    }

                    desc.Routing.Clear();
                    foreach (var gate in dictionary)
                    {
                        var msg = new ServiceDesc();
                        msg.Data = desc.Data;
                        msg.ClientId = desc.ClientId;
                        msg.PacketId = desc.PacketId;
                        msg.FuncId = desc.FuncId;
                        msg.ServiceType = desc.ServiceType;
                        msg.Error = desc.Error;
                        msg.Type = desc.Type;
                        msg.CharacterId = desc.CharacterId;
                        msg.Routing.AddRange(gate.Value);
                        gate.Key.SendMessage(msg);
                    }
                    return;
                }

                ServerClient client;
                if (mFromClientId2Gate.TryGetValue(desc.ClientId, out client))
                {
                    client.SendMessage(desc);
                }
                else
                {
                    Logger.Warn("Can find client for message routing id: {0}", desc.ClientId);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Some error inside ClientMessageReceived ");
            }
        }

        //-------------------------------------------------无用------
        public void ServerOnDisconnected()
        {
            throw new NotImplementedException();
        }

        public void ServerOnConnected()
        {
            throw new NotImplementedException();
        }

        public void ClearLostClient()
        {
//             if (mClearList.Count == 0)
//             {
//                 return;
//             }
// 
//             if (mIndex >= mClearList.Count)
//             {
//                 mIndex = 0;
//                 mClearList = mFromClientId2Login.Keys.ToList();
// 
//                 // clear operation should finished in one minute.
//                 mItemPerTick = (int)Math.Ceiling(mClearList.Count / 60.0f);
//             }
// 
//             var count = 0;
//             while (count < mItemPerTick && mIndex < mClearList.Count)
//             {
//                 var key = mClearList[mIndex];
//                 var client = mFromClientId2Login[key];
//                 if ((DateTime.Now - client.LastMessageTime).TotalHours > 1)
//                 {
//                     mFromClientId2Login.TryRemove(key, out client);
//                 }
// 
//                 mIndex++;
//                 count++;
//             }
        }

        //private readonly ConcurrentDictionary<ulong, ServerClient> mFromClientIdToClient = new ConcurrentDictionary<ulong, ServerClient>();

        public int ConnectionCount
        {
            get { return mFromClientId2Login.Count; }
        }

        public bool Connected
        {
            get { return mBackEnds.All(client => client.IsConnected); }
        }

        public long PackageCount
        {
            get { return mPackageCount; }
        }

        private class ClientRouting
        {
            public DateTime LastMessageTime;
            public SocketClient Server;
        }
    }
}