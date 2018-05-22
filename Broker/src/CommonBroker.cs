#region using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using DataContract;
using Scorpion;
using NLog;

using Shared;

#endregion

namespace Broker
{
    public partial class CommonBroker : IBrokerBase
    {
        public virtual void BackEndsOnConnect(SocketClient client, int index)
        {
        }

        //-------------------------------------------------流程----
        public virtual UserData CreateUserData(int serverId)
        {
            return new UserData {Id = serverId};
        }

        //获得Routing
        public ulong GetRouting(ServiceDesc desc)
        {
            if (desc.Routing.Count > 0)
            {
                return desc.Routing[desc.Routing.Count - 1];
            }
            if (desc.FuncId != 3501)
            {
                Logger.Error("Broker GetRouting faild!! ClientId={0},CharacterId={1},FuncId={2},ServiceType={3}",
                    desc.ClientId, desc.CharacterId, desc.FuncId, desc.ServiceType);
            }
            return ulong.MaxValue;
        }

        //-------------------------------------------------多线程-----
        public void OnReconnectStart(int count)
        {
            //Logger.Warn("Start reconnect to server {0} {1}:{2} {3} times.", mServiceName, serverItem.Ip, serverItem.Port, count);
        }

//         public void OnSocketListenerMessageReceiveSS(ServerClient client, ServiceDesc desc)
//         {
//             var clientId = desc.ClientId;
//             var characterId = desc.CharacterId;
//             SocketClientInfo server;
//             // logic rank team chat activity CheckLost
//             if (desc.FuncId == 1033 || desc.FuncId == 6032 || desc.FuncId == 7032 || desc.FuncId == 5032 || desc.FuncId == 4032)
//             {
//                 if (!mFromCharacterId2Server.TryGetValue(characterId, out server))
//                 {
//                     client.SendMessage(desc);
//                     return;
//                 }
//             }
// 
//             if (!mFromCharacterId2Server.TryGetValue(characterId, out server))
//             {
//                 server = new SocketClientInfo
//                 {
//                     Server = SelectServerForCharacter(characterId),
//                     //LastMessageTime = DateTime.Now
//                 };
//                 mFromCharacterId2Server.TryAdd(characterId, server);
//             }
// 
//             desc.Routing.Add(client.ClientId);
//             server.Server.SendMessage(desc);
// 
//             Logger.Info("Transfer SS Func {0} char:{1} func:{2}", mServiceName, desc.CharacterId, desc.FuncId);
//         }
        //public void OnSocketListenerMessageReceiveCsSync(ServerClient client, ServiceDesc desc)
        //{
        //    var clientId = desc.ClientId;
        //    var characterId = desc.CharacterId;
        //    CharacterInfo clientRouting;
        //    if (!mFromClientId2CharacterId.TryGetValue(clientId, out characterId))
        //    {
        //        Logger.Warn("Can not find character id for client 1: " + clientId);
        //        return;
        //    }

        //    if (!mCharacterInfoManager.TryGetValue(characterId, out clientRouting))
        //    {
        //        Logger.Warn("Can not find server for character: " + characterId);
        //        return;
        //    }
        //    if (desc.CharacterId != characterId)
        //    {
        //        Logger.Error("OnSocketListenerMessageReceiveCsSync oldCharacterId={0},newCharacterId={1}", desc.CharacterId,characterId);
        //        desc.CharacterId = characterId;
        //    }

        //    try
        //    {
        //        // 只要消息经过这里，就给Gate回复一个这个消息，后面的消息就不应该到这里了，Gate会直接发送到Server上。
        //        ServiceDesc gateDesc = new ServiceDesc();
        //        gateDesc.Type = 20;
        //        gateDesc.CharacterId = characterId;
        //        gateDesc.ServiceType = desc.ServiceType;
        //        gateDesc.ClientId = desc.ClientId;
        //        gateDesc.Routing.Add((ulong)(int)client.UserData);
        //        clientRouting.Server.SendMessage(gateDesc);

        //        clientRouting.Server.SendMessage(desc);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.WarnException("Send message failed. 1", ex);
        //    }

        //    //clientRouting.LastMessageTime = DateTime.Now;

        //    Interlocked.Increment(ref mPackageCount);
        //}

        //---------------------SocketClient--------
        public void OnSocketClientConnected(SocketClient b)
        {
            //Logger.Debug("Server {0} at {1}:{2} Connected.", mServiceName, serverItem.Ip, serverItem.Port);
            var desc = new ServiceDesc();
            desc.Type = (int) MessageType.Connect;
            desc.ServiceType = 1;
            b.SendMessage(desc);
        }

        public virtual void OnSocketClientDisconnected(SocketClient b)
        {
            //Logger.Debug("Server {0} at {1}:{2} Disconnected.", mServiceName, serverItem.Ip, serverItem.Port);
        }

        public void OnSocketClientException(Exception exception)
        {
            Logger.Error(exception, "Connect to server error {0}", mServiceName);
        }

        public void OnSocketClientMessageReceivedSCList(ServiceDesc desc)
        {
            var clients = desc.Routing;
            SplitMessageToGate(clients, desc);
        }

        public void OnSocketClientMessageReceivedSCServer(ServiceDesc desc)
        {
            ConcurrentDictionary<ulong, int> characters;
            var serverId = desc.PacketId;
            if (!mFromServerId2CharacterId.TryGetValue(serverId, out characters))
            {
                //广播给的服务器上没有玩家
                Logger.Info("OnSocketClientMessageReceivedSCServer  funcId={0},ServiceType={1},ServerId ={2}",
                    desc.FuncId, desc.ServiceType, serverId);
                return;
            }
            var list = characters.Keys.ToList();
            SplitMessageToGate(list, desc);
        }

        //---------------------SocketListener--------
        public void OnSocketListenerException(Exception exception)
        {
            Logger.Error(exception, "Start listen Error");
        }

        public virtual void OnSocketListenerMessageReceiveBS(ServiceDesc desc)
        {
            Logger.Error("{1} ClientMessageReceived MessageType.Lost is Error! FuncId={0}", desc.FuncId, mServiceName);
        }

        //public void OnSocketListenerMessageReceiveSCAll(ServerClient client, ServiceDesc desc)
        //{
        //    var gates = mFromCharacterId2Gate.Values.Distinct();
        //    foreach (var gate in gates)
        //    {
        //        try
        //        {
        //            gate.Gate.SendMessage(desc);
        //        }
        //        catch
        //        {
        //            // do not affect next one.
        //        }
        //    }
        //}
        public void OnSocketListenerMessageReceiveSAS(ServerClient client, ServiceDesc desc)
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
                try
                {
                    socketClient.SendMessage(desc);
                    i++;
                }
                catch
                {
                    // do not affect next one.
                }
            }
            reply.Routing.Add(i);
            client.SendMessage(reply);
        }

        public virtual void OnSocketListenerMessageReceiveSB(ServerClient client, ServiceDesc desc)
        {
            if (desc.FuncId%1000 == 30)
            {
//SBGetAllOnlineCharacterInServer
                var info =
                    ProtocolExtension.Deserialize<__RPC_Logic_SBGetAllOnlineCharacterInServer_ARG_int32_serverId__>(
                        desc.Data);
                ConcurrentDictionary<ulong, int> bag;
                if (mFromServerId2CharacterId.TryGetValue((uint) info.ServerId, out bag))
                {
                    var ret =
                        new __RPC_Logic_SBGetAllOnlineCharacterInServer_RET_Uint64Array__();
                    ret.ReturnValue.Items.AddRange(bag.Keys);

                    desc.Data = ProtocolExtension.Serialize(ret);
                    client.SendMessage(desc);
                }
            }
            else if (desc.FuncId%1000 == 40)
            {
                // QueryBrokerStatus
                //var ret = new __RPC_Logic_QueryBrokerStatus_RET_CommonBrokerStatus__();
                //var status = ret.ReturnValue = new CommonBrokerStatus();
                //status.CommonStatus = new ServerCommonStatus();
                //status.CommonStatus.ByteReceivedPerSecond = (uint) mFrontEndServer.ByteReceivedPerSecond;
                //status.CommonStatus.ByteSendPerSecond = (uint) mFrontEndServer.ByteSendPerSecond;
                //status.CommonStatus.MessageReceivedPerSecond = (uint) mFrontEndServer.MessageReceivedPerSecond;
                //status.CommonStatus.MessageSendPerSecond = (uint) mFrontEndServer.MessageSendPerSecond;
                //status.CommonStatus.ConnectionCount = (uint) mFrontEndServer.ConnectionCount;

                //status.ConnectionInfo.AddRange(mBackEnds.Select(item =>
                //{
                //    var conn = new ConnectionStatus();
                //    conn.ByteReceivedPerSecond = (uint) item.ByteReceivedPerSecond;
                //    conn.ByteSendPerSecond = (uint) item.ByteSendPerSecond;
                //    conn.MessageReceivedPerSecond = (uint) item.MessageReceivedPerSecond;
                //    conn.MessageSendPerSecond = (uint) item.MessageSendPerSecond;
                //    conn.Target = (uint) ((UserData) item.UserData).Id;
                //    conn.Latency = (float) item.Latency.TotalMilliseconds;

                //    return conn;
                //}));

                //desc.Data = ProtocolExtension.Serialize(ret);
                //client.SendMessage(desc);
            }
            else if (desc.FuncId%1000 == 500)
            {
//收到login断线清理
                OnSocketListenerMessageReceiveCleanEx(desc);
            }
            else if (desc.FuncId%1000 == 502)
            {
                var ret = new __RPC_Scene_SBGetServerCharacterCount_RET_Dict_int_int_Data__();
                ret.ReturnValue = new Dict_int_int_Data();
                foreach (var s in mFromServerId2CharacterId)
                {
                    ret.ReturnValue.Data.Add((int) s.Key, s.Value.Count);
                }
                desc.Data = ProtocolExtension.Serialize(ret);
                client.SendMessage(desc);
            }
            else if (desc.FuncId%1000 == 503)
            {
//                 var info = ProtocolExtension.Deserialize<__RPC_Scene_SBModifyCharacterClientId_ARG_uint64_oldClientId_uint64_newClientId_uint64_characterId__>(desc.Data);
//                 var character = GetCharacterInfo(info.CharacterId);
//                 if (character == null)
//                 {
//                     client.SendMessage(desc);
//                     return;
//                 }
//                 if (character.ClientId != info.OldClientId)
//                 {
//                     Logger.Error("funID%1000=503,brokerClientId={0},oldClientId={1},newClientId={2}", character.ClientId, info.OldClientId, info.NewClientId);
//                     client.SendMessage(desc);
//                     return;
//                 }
//                 
//                 //执行
//                 ulong brokerId;
//                 mFromClientId2CharacterId.TryRemove(character.ClientId, out brokerId);
//                 mFromClientId2CharacterId.TryAdd(character.ClientId, info.CharacterId);
//                 character.ClientId = info.NewClientId;
//                 var ret = new __RPC_Scene_SBModifyCharacterClientId_RET_int32__();
//                 ret.ReturnValue = 1;
//                 desc.Data = ProtocolExtension.Serialize(ret);
//                 client.SendMessage(desc);
            }
            else
            {
                Logger.Error(
                    "OnSocketListenerMessageReceiveSB faild!! ClientId={0},CharacterId={1},FuncId={2},ServiceType={3}",
                    desc.ClientId, desc.CharacterId, desc.FuncId, desc.ServiceType);
            }
        }

//         public void OnSocketListenerMessageReceivePrepareData(ServerClient client, ServiceDesc desc)
//         {
//             //var clientId = desc.ClientId;
//             //var characterId = desc.CharacterId;
//             //ClientRouting server;
//             using (var ms = new MemoryStream(desc.Data))
//             {
//                 var info = Serializer.Deserialize<PrepareDataMessage>(ms);
//                 SocketClientInfo routing;
//                 // 如果这个Character已经被分配过，则不需要重新分配
//                 if (!mFromCharacterId2Server.TryGetValue(info.CharacterId, out routing))
//                 {
//                     routing = new SocketClientInfo
//                     {
//                         Server = SelectServerForCharacter(info.CharacterId),
//                         //LastMessageTime = DateTime.Now,
//                         Data = info
//                     };
// 
//                     mFromCharacterId2Server.AddOrUpdate(info.CharacterId, routing, (arg1, arg2) => routing);
//                 }
//                 else
//                 {
//                     
//                 }
//                 ConcurrentDictionary<ulong, int> bag;
//                 if (!mFromServerId2CharacterId.TryGetValue((uint)info.ServerId, out bag))
//                 {
//                     bag = new ConcurrentDictionary<ulong, int>();
//                     mFromServerId2CharacterId.TryAdd((uint)info.ServerId, bag);
//                 }
// 
//                 bag.TryAdd(info.CharacterId, 0);
// 
//                 mFromClientId2CharacterId.AddOrUpdate(info.ClientId, info.CharacterId, (arg1, arg2) => info.CharacterId);
// 
//                 client.SendMessage(desc);
//                 Logger.Info("Reply PrepareData {0} {1}", mServiceName, desc.CharacterId);
//             }
//         }
        public void OnSocketListenerMessageReceiveSCList(ServerClient client, ServiceDesc desc)
        {
            OnSocketClientMessageReceivedSCList(desc);
        }

        public void OnSocketListenerMessageReceiveSCServer(ServerClient client, ServiceDesc desc)
        {
            OnSocketClientMessageReceivedSCServer(desc);
        }

        //-------------------------------------------------线程无关-----
        private SocketClient SelectServerForCharacter(ulong characterId)
        {
            var index = characterId%(ulong) mBackEnds.Count;
            return mBackEnds[(int) index];
        }

        public void SplitMessageToGate(List<ulong> characters, ServiceDesc desc)
        {
            var dictionary = new Dictionary<ServerClient, List<ulong>>();
            foreach (var v in characters)
            {
                try
                {
                    var character = GetCharacterInfo(v);
                    if (character == null)
                    {
                        continue;
                    }
                    var gate = character.Gate;
                    List<ulong> c;
                    if (!dictionary.TryGetValue(gate.Gate, out c))
                    {
                        c = new List<ulong>();
                        dictionary.Add(gate.Gate, c);
                    }

                    c.Add(character.ClientId);
                }
                catch
                {
                    // do not affect next one.
                }
            }
            foreach (var gate in dictionary)
            {
                try
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
                catch
                {
                    // do not affect next one.
                }
            }
            desc.Routing.Clear();
        }

        public virtual void Start(int id, int nPort, string type, dynamic[] serverList)
        {
            mServiceName = type;
            mPort = nPort;

            if (mServiceName == "Login")
            {
                mServiceFunc = 2000;
                mServiceType = ServiceType.Login;
            }
            else if (mServiceName == "Activity")
            {
                mServiceFunc = 4000;
                mServiceType = ServiceType.Activity;
            }
            else if (mServiceName == "Logic")
            {
                mServiceFunc = 1000;
                mServiceType = ServiceType.Logic;
            }
            else if (mServiceName == "Rank")
            {
                mServiceFunc = 6000;
                mServiceType = ServiceType.Rank;
            }
            else if (mServiceName == "Scene")
            {
                mServiceFunc = 3000;
                mServiceType = ServiceType.Scene;
            }
            else if (mServiceName == "Chat")
            {
                mServiceFunc = 5000;
                mServiceType = ServiceType.Chat;
            }
            else if (mServiceName == "Team")
            {
                mServiceFunc = 7000;
                mServiceType = ServiceType.Team;
            }
            else if (mServiceName == "GameMaster")
            {
                mServiceFunc = 9000;
                mServiceType = ServiceType.GameMaster;
            }

            Logger = LogManager.GetLogger("Broker." + mServiceName + "Broker");
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
            var index = 0;
            foreach (var server in serverList)
            {
                try
                {
                    Logger.Debug("Connect to server {0} {1}:{2}", mServiceName, server.Ip, server.Port);

                    //var serverItem = server;
                    var settings = new SocketClientSettings(new IPEndPoint(IPAddress.Parse(server.Ip), server.Port),
                        1024*1024*1024);
                    var b = new SocketClient(settings);
                    {
                        b.UserData = CreateUserData((int) server.Id);
                        b.Connected += () => { OnSocketClientConnected(b); };
                        b.Disconnected += () => { OnSocketClientDisconnected(b); };
                        b.MessageReceived += ServerOnMessageReceived;
                        b.OnException += OnSocketClientException;

                        mBackEnds.Add(b);
                        BackEndsOnConnect(b, index);
                        b.StartConnect();
                    }
                }
                catch (Exception ex)
                {
                    Logger.FatalException(
                        "Server " + server.ServiceName + " at " + server.Ip + ":" + server.Port + " can not reached.",
                        ex);
                }
                finally
                {
                    index++;
                }
            }

            Console.WriteLine("{1} startOver. [{0}]", id, mServiceName);
        }

        public virtual void Stop()
        {
            mFrontEndServer.Stop();
            lock (mBackEnds)
            {
                foreach (var client in mBackEnds)
                {
                    client.Stop();
                }
            }
        }

        public virtual void Status(ConcurrentDictionary<string, string> dict)
        {
            if (mFrontEndServer == null)
            {
                return;
            }
            try
            {
                dict.TryAdd("_Listening", mFrontEndServer.IsListening.ToString());
                //dict.TryAdd("ByteReceivedPerSecond", mFrontEndServer.ByteReceivedPerSecond.ToString());
                //dict.TryAdd("ByteSendPerSecond", mFrontEndServer.ByteSendPerSecond.ToString());
                //dict.TryAdd("MessageReceivedPerSecond", mFrontEndServer.MessageReceivedPerSecond.ToString());
                //dict.TryAdd("MessageSendPerSecond", mFrontEndServer.MessageSendPerSecond.ToString());
                //dict.TryAdd("ConnectionCount", mFrontEndServer.ConnectionCount.ToString());

                //for (var i = 0; i < mBackEnds.Count; i++)
                //{
                //    var backEnd = mBackEnds[i];
                //    dict.TryAdd("Server" + i + " Latency", backEnd.Latency.ToString());
                //    dict.TryAdd("Server" + i + " ByteReceivedPerSecond", backEnd.ByteReceivedPerSecond.ToString());
                //    dict.TryAdd("Server" + i + " ByteSendPerSecond", backEnd.ByteSendPerSecond.ToString());
                //    dict.TryAdd("Server" + i + " MessageReceivedPerSecond", backEnd.MessageReceivedPerSecond.ToString());
                //    dict.TryAdd("Server" + i + " MessageSendPerSecond", backEnd.MessageSendPerSecond.ToString());
                //}
            }
            catch (Exception ex)
            {
                Logger.Error("CommonBroker Status Error!{0},mServiceName={1}", ex, mServiceName);
            }
        }

        public void ClientConnected(ServerClient client)
        {
            Logger.Debug("Gate: " + client.ClientId + " connected.");
            client.MessageReceived += ClientMessageReceived;
            //mFromClientIdToClient.AddOrUpdate(client.ClientId, client, (arg1, serverClient) => client);
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

        //服务器网络异步函数（Gate,  其他所有服务器[包含子服务器]）
        public virtual void ClientMessageReceived(ServerClient client, ServiceDesc desc)
        {
            try
            {
                var type = (MessageType) desc.Type;
                Logger.Debug("ClientMessageReceived ,type={0},FuncId={1}", type, desc.FuncId);
                switch (type)
                {
                    case MessageType.CS:
                        Logger.Error("CommenBroker ClientMessageReceived MessageType.CS is Error! FuncId={0}",
                            desc.FuncId);
                        //OnSocketListenerMessageReceiveCsSync(client, desc);
                        break;
                    case MessageType.SC:
                        Logger.Error("CommenBroker ClientMessageReceived MessageType.SC is Error! FuncId={0}",
                            desc.FuncId);
                        break;
                    case MessageType.SS:
                        Logger.Error("CommenBroker ClientMessageReceived MessageType.SS is Error! FuncId={0}",
                            desc.FuncId);
                        break;
                    case MessageType.Connect:
                        OnSocketListenerMessageReceiveConnectEx(client, desc);
                        break;
                    case MessageType.Lost:
                        Logger.Error("CommenBroker ClientMessageReceived MessageType.Lost is Error! FuncId={0}",
                            desc.FuncId);
                        break;
                    case MessageType.Sync:
                        Logger.Error("CommenBroker ClientMessageReceived MessageType.Sync is Error! FuncId={0}",
                            desc.FuncId);
                        //OnSocketListenerMessageReceiveCsSync(client, desc);
                        break;
                    case MessageType.Ping:
                        break;
                    case MessageType.SB:
                        OnSocketListenerMessageReceiveSB(client, desc);
                        break;
                    case MessageType.BS:
                        Logger.Error("CommenBroker ClientMessageReceived MessageType.BS is Error! FuncId={0}",
                            desc.FuncId);
                        break;
                    case MessageType.SCAll:
                        Logger.Error("CommenBroker ClientMessageReceived MessageType.SCAll is Error!");
                        break;
                    case MessageType.SCServer:
                        OnSocketListenerMessageReceiveSCServer(client, desc);
                        break;
                    case MessageType.SCList:
                        OnSocketListenerMessageReceiveSCList(client, desc);
                        break;
                    case MessageType.SAS:
                        OnSocketListenerMessageReceiveSAS(client, desc);
                        break;
                    case MessageType.PrepareData:
                        OnSocketListenerMessageReceivePrepareDataEx(client, desc);
                        break;
                    case MessageType.SASReply:
                        Logger.Error("CommenBroker ClientMessageReceived MessageType.SASReply is Error!");
                        break;
                    default:
                        Logger.Error("CommenBroker ClientMessageReceived is Error!type={0}", type);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Some error inside ClientMessageReceived ");
            }
        }

        //客户端网络异步函数（他管理的子服务器）
        public virtual void ServerOnMessageReceived(ServiceDesc desc)
        {
            try
            {
                var type = (MessageType) desc.Type;
                Logger.Debug("ServerOnMessageReceived ,type={0},FuncId={1}", type, desc.FuncId);
                switch (type)
                {
                    case MessageType.CS:
                        Logger.Error("CommenBroker ServerOnMessageReceived MessageType.CS is Error! FuncId={0}",
                            desc.FuncId);
                        break;
                    case MessageType.SC:
                        Logger.Error("CommenBroker ServerOnMessageReceived MessageType.SC is Error! FuncId={0}",
                            desc.FuncId);
                        break;
                    case MessageType.SS:
                        Logger.Error("CommenBroker ServerOnMessageReceived MessageType.SS is Error! FuncId={0}",
                            desc.FuncId);
                        //mFrontEndServer.Clients[desc.Routing[desc.Routing.Count - 1]].SendMessage(desc);
                        return;
                    case MessageType.Connect:
                        Logger.Error("CommenBroker ServerOnMessageReceived MessageType.Connect is Error! FuncId={0}",
                            desc.FuncId);
                        break;
                    case MessageType.Lost:
                        Logger.Error("CommenBroker ServerOnMessageReceived MessageType.Lost is Error! FuncId={0}",
                            desc.FuncId);
                        break;
                    case MessageType.Sync:
                        Logger.Error("CommenBroker ServerOnMessageReceived MessageType.Sync is Error! FuncId={0}",
                            desc.FuncId);
                        break;
                    case MessageType.Ping:
                        break;
                    case MessageType.SB:
                        Logger.Error("CommenBroker ServerOnMessageReceived MessageType.SB is Error! FuncId={0}",
                            desc.FuncId);
                        break;
                    case MessageType.BS:
                        OnSocketListenerMessageReceiveBS(desc);
                        break;
                    case MessageType.SCAll:
                        Logger.Error("CommenBroker ServerOnMessageReceived MessageType.SCAll is Error!");
                        break;
                    case MessageType.SCServer:
                        OnSocketClientMessageReceivedSCServer(desc);
                        return;
                    case MessageType.SCList:
                        OnSocketClientMessageReceivedSCList(desc);
                        return;
                    case MessageType.SAS:
                    case MessageType.PrepareData:
                        var routing = GetRouting(desc);
                        if (routing == ulong.MaxValue)
                        {
                            return;
                        }
                        mFrontEndServer.Clients[routing].SendMessage(desc);
                        return;
                    case MessageType.SASReply:
                        Logger.Error("CommenBroker ServerOnMessageReceived MessageType.SASReply is Error! FuncId={0}",
                            desc.FuncId);
                        break;
                    default:
                        Logger.Error("CommenBroker ServerOnMessageReceived MessageType.SB is Error! FuncId={0}",
                            desc.FuncId);
                        return;
                }
                var character = GetCharacterInfo(desc.CharacterId);
                if (character == null)
                {
                    Logger.Error(
                        "CommenBroker ServerOnMessageReceived character = null desc.CharacterId :{0} ,funcId={1},ServiceType={2},clientId={3}",
                        desc.CharacterId, desc.FuncId, desc.ServiceType, desc.ClientId);
                    return;
                }
                if (character.Gate == null)
                {
                    Logger.Error("CommenBroker ServerOnMessageReceived 9 = null desc.CharacterI0d :{0} ",
                        desc.CharacterId);
                    return;
                }
                //desc.ClientId = character.ClientId;
                character.Gate.Gate.SendMessage(desc);
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
            //                 mClearList = mFromClientId2CharacterId.Keys.ToList();
            // 
            //                 // clear operation should finished in one minute.
            //                 mItemPerTick = (int)Math.Ceiling(mClearList.Count / 60.0f);
            //             }
            // 
            //             var count = 0;
            //             while (count < mItemPerTick && mIndex < mClearList.Count)
            //             {
            //                 var key = mClearList[mIndex];
            //                 ulong characterId = mFromClientId2CharacterId[key];
            //                 ClientRouting clientRouting = mFromCharacterId2Server[characterId];
            //                 if ((DateTime.Now - clientRouting.LastMessageTime).TotalHours > 1)
            //                 {
            //                     mFromClientId2CharacterId.TryRemove(key, out characterId);
            //                     mFromCharacterId2Server.TryRemove(characterId, out clientRouting);
            //                 }
            // 
            //                 mIndex++;
            //                 count++;
            //             }
        }
    }
}