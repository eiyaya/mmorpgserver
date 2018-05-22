using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hik.Collections;
using Hik.Communication.Scs.Client;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Communication.Protocols.ProtobufSerialization;
using Hik.Communication.Scs.Server;
using JsonConfig;
using NLog;
using ProtoBuf;
using ServiceBase;
using Shared;

namespace CommonBroker
{
    class Broker
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private string mServiceName;
        private int mId;
        private int mPort;

        private IScsServer mFrontEndServer;
        private ConcurrentDictionary<int, IScsServerClient> mFrontEndMapping;
        
        private List<ClientReConnecter> mReConnecters = new List<ClientReConnecter>();
        private readonly List<IScsClient> mBackEnds = new List<IScsClient>();

        class ClientRouting
        {
            public DateTime LastMessageTime;
            public IScsClient Server;
        }

        private readonly ConcurrentDictionary<ulong, ClientRouting> mFromCharacterId2Server = new ConcurrentDictionary<ulong, ClientRouting>();
        private readonly ConcurrentDictionary<ulong, ulong> mFromClientId2CharacterId = new ConcurrentDictionary<ulong, ulong>(); 

        public int ConnectionCount
        {
            get { return mFromCharacterId2Server.Count; }
        }

        private long mPackageCount = 0;
        public long PackageCount
        {
            get { return mPackageCount; }
        }

        public void Start()
        {
            mId = Config.User.Id;
            mServiceName = Config.User.ServiceName;
            mPort = Config.User.Port;

            foreach (var server in Config.User.Servers)
            {
                try
                {
                    using (var b = ScsClientFactory.CreateClient(new ScsTcpEndPoint(server.Ip, server.Port)))
                    {
                        b.Connected += ServerOnConnected;
                        b.Disconnected += ServerOnDisconnected;
                        b.MessageReceived += ServerOnMessageReceived;

                        b.ConnectTimeout = 2000;
                        b.Connect();
                        mBackEnds.Add(b);

                        mReConnecters.Add(new ClientReConnecter(b));
                    }
                }
                catch (Exception ex)
                {
                    Logger.FatalException("Server " + server.ServiceName + " at " + server.Ip + ":" + server.Port + " can not reached.", ex);
                    return;
                }
            }

            try
            {
                mFrontEndServer = ScsServerFactory.CreateServer(new ScsTcpEndPoint(mPort));
                mFrontEndServer.WireProtocolFactory = new ProtobufSerializationProtocolFactory();

                mFrontEndServer.ClientConnected += ClientConnected;
                mFrontEndServer.ClientDisconnected += ClientDisconnected;

                mFrontEndServer.Start();
            }
            catch (Exception ex)
            {
                Logger.FatalException("Server start failed.", ex);
            }
        }

        private int mIndex = 0;
        private List<ulong> mClearList;
        public void ClearLostClient()
        {
            if (mIndex >= mClearList.Count)
            {
                mIndex = 0;
                mClearList = mFromCharacterId2Server.Keys.ToList();
            }

            var key = mClearList[mIndex];
            ClientRouting clientRouting;
            if (mFromCharacterId2Server.TryGetValue(key, out clientRouting))
            {
                if ((DateTime.Now - clientRouting.LastMessageTime).TotalHours > 1)
                {
                    mFromCharacterId2Server.TryRemove(key, out clientRouting);
                }
            }

            mIndex++;
        }

        /// <summary>
        /// This function will be called in multithread, so THREAD SAFE is very important.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerOnMessageReceived(object sender, MessageEventArgs e)
        {
            var message = e.Message as ProtobufMessage;

            if (message.Desc.Routing.Count == 0)
            {
                return;
            }

            var clientId = message.Desc.Routing[message.Desc.Routing.Count - 1];
            message.Desc.Routing.RemoveAt(message.Desc.Routing.Count - 1);

            IScsServerClient client;
            if (mFrontEndMapping.TryGetValue((int)clientId, out client))
            {
                client.SendMessage(message);
            }
            else
            {
                Logger.Error("Message can not routed. " + message.Desc.ServiceName);
            }
        }

        /// <summary>
        /// This function will be called in multithread, so THREAD SAFE is very important.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void ServerOnDisconnected(object sender, EventArgs eventArgs)
        {
            Logger.Info("Server " + eventArgs);
        }


        private void ServerOnConnected(object sender, EventArgs eventArgs)
        {
            Logger.Info("Server " + eventArgs);
        }

        /// <summary>
        /// This function will be called in multithread, so THREAD SAFE is very important.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClientDisconnected(object sender, ServerClientEventArgs e)
        {
            Logger.Info("Gate: " + e.Client.ClientId + " disconnected.");
        }

        private void ClientConnected(object sender, ServerClientEventArgs e)
        {
            Logger.Info("Gate: " + e.Client.ClientId + " connected.");
            e.Client.MessageReceived += ClientMessageReceived;
        }

        private IScsClient SelectServerForCharacter(ulong characterId)
        {
            return mFromCharacterId2Server[characterId].Server;
        }

        /// <summary>
        /// This function will be called in multithread, so THREAD SAFE is very important.
        /// Called when gate pass a message, or other server pass a message.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClientMessageReceived(object sender, MessageEventArgs e)
        {
            var message = e.Message as ProtobufMessage;

            if (message == null)
            {
                return;
            }

            var gateOrServer = sender as IScsServerClient;
            var key = message.Desc.ClientId;
            var client = sender as IScsServerClient;

            ClientRouting clientRouting;
            ulong characterId;

            if (message.Desc.Type == (int)MessageType.CS || message.Desc.Type == (int)MessageType.SS)
            {
                if (!mFromClientId2CharacterId.TryGetValue(key, out characterId))
                {
                    Logger.Warn("Can not find character id for client: " + key);
                }

                if (!mFromCharacterId2Server.TryGetValue(characterId, out clientRouting))
                {
                    Logger.Warn("Can not find server for character: " + characterId);
                }

                message.Desc.Routing.Add((ulong)gateOrServer.ClientId);
                clientRouting.Server.SendMessage(message);
                clientRouting.LastMessageTime = DateTime.Now;


                Interlocked.Increment(ref mPackageCount);
            }
            else if (message.Desc.Type == (int)ServerMessageType.PrepareData)
            {
                using (var ms = new MemoryStream(message.Desc.Data))
                {
                    var info = Serializer.Deserialize<PrepareDataMessage>(ms);
                    mFromCharacterId2Server[info.CharacterId] = new ClientRouting
                    {
                        Server = SelectServerForCharacter(info.CharacterId),
                        LastMessageTime = DateTime.Now
                    };

                    mFromClientId2CharacterId[info.ClientId] = info.CharacterId;
                }

                message.Desc.Type = (int)ServerMessageType.PrepareDataOk;
                client.SendMessage(new ProtobufMessage(message.Desc));
            }
            else if (message.Desc.Type == (int)MessageType.Hi)
            {
                mFrontEndMapping.AddOrUpdate((int) message.Desc.ClientId, client, (i, serverClient) => serverClient);

                message.Desc.ClientId = (ulong)mId;

                if (client != null) 
                    client.SendMessage(new ProtobufMessage(message.Desc));
            }
            else if (message.Desc.Type == (int)MessageType.Lost)
            {
                

                if (!mFromClientId2CharacterId.TryRemove(key, out characterId))
                {
                    Logger.Warn("Can not find character id for client: " + key);
                }

                if (!mFromCharacterId2Server.TryRemove(characterId, out clientRouting))
                {
                    Logger.Warn("Can not find server for character: " + characterId);
                }

                clientRouting.Server.SendMessage(e.Message);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var broker = new Broker();
            broker.Start();

            while (true)
            {
                Console.Write("Client connections: " + broker.ConnectionCount);
                Console.WriteLine("\t\tPackage transmit: " + broker.PackageCount);

                broker.ClearLostClient();
            }

        }
    }
}
