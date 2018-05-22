#region using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataContract;
using Scorpion;
using NLog;
using ProtoBuf;

using Shared;

#endregion

namespace Broker
{
    public partial class CommonBroker : IBrokerBase
    {
        protected static readonly Logger ConnectLostLogger = LogManager.GetLogger("ConnectLost");

        public CommonBroker()
        {
            PackageCount = 0;
        }

        protected Logger Logger;
        //连接相应的逻辑服务器列表
        public readonly List<SocketClient> mBackEnds = new List<SocketClient>();
        //private readonly ConcurrentDictionary<ulong, CharacterInfo> mCharacterInfoManager = new ConcurrentDictionary<ulong, CharacterInfo>(); //key CharacterId value 对应的逻辑服务器

        public readonly ConcurrentDictionary<ulong, CharacterInfo> mCharacterInfoManager =
            new ConcurrentDictionary<ulong, CharacterInfo>(); //key CharacterId value 对应的逻辑服务器

        //protected readonly ConcurrentDictionary<ulong, GateProxy> mFromCharacterId2Gate = new ConcurrentDictionary<ulong, GateProxy>();//key CharacterId value 对应的Gate

        //protected readonly ConcurrentDictionary<ulong, ulong> mFromCharacterId2ClientId = new ConcurrentDictionary<ulong, ulong>();//key CharacterId value 对应的ClientId

        protected readonly ConcurrentDictionary<ulong, ulong> mFromClientId2CharacterId =
            new ConcurrentDictionary<ulong, ulong>();

        //（记录每个逻辑服务器的玩家人数） 第二个int没有什么用，因为C#没有线程安全的HashSet，所以用Dictionary代替，只使用了key
        private readonly ConcurrentDictionary<uint, ConcurrentDictionary<ulong, int>> mFromServerId2CharacterId =
            new ConcurrentDictionary<uint, ConcurrentDictionary<ulong, int>>();

        //监听连接
        protected SocketListener mFrontEndServer;
        private readonly ConcurrentDictionary<int, GateProxy> mGates = new ConcurrentDictionary<int, GateProxy>();
        protected List<ulong> mLinkCount = new List<ulong>();
        protected int mPort;
        protected uint mServiceFunc;
        protected string mServiceName;
        protected ServiceType mServiceType;
        //-------------------------------------------------流程----

        //-------------------------------------------------线程无关-----

        //-------------------------------------------------多线程----

        //---------------------SocketListener--------

        public virtual CharacterInfo CreateCharacter(ulong CharacterId)
        {
            var character = new CharacterInfo();
            character.CharacterId = CharacterId;
            return character;
        }

        //         private readonly ConcurrentDictionary<ulong, ServerClient> mFromClientIdToClient = 
        //             new ConcurrentDictionary<ulong, ServerClient>();
        public CharacterInfo GetCharacterInfo(ulong characterId)
        {
            CharacterInfo c = null;
            mCharacterInfoManager.TryGetValue(characterId, out c);
            return c;
        }

        //---------------------SocketClient--------

        public int GetServerIndex(SocketClient socket)
        {
            var ret = 0;
            foreach (var client in mBackEnds)
            {
                if (socket == client)
                {
                    return ret;
                }
                ret ++;
            }
            return -1;
        }

        public virtual void OnSocketListenerMessageReceiveCleanEx(ServiceDesc desc)
        {
            using (var ms = new MemoryStream(desc.Data))
            {
                var msg =
                    Serializer
                        .Deserialize<__RPC_Scene_SBCleanClientCharacterData_ARG_uint64_clientId_uint64_characterId__>(ms);
                var characterId = msg.CharacterId;
                var clientId = msg.ClientId;
                PlayerLog.WriteLog(characterId, "{0}Broker Onlost Character={1},ClientId={2}", mServiceName, characterId,
                    clientId);
                //通知broker下的逻辑服务器断开连接
                var character = GetCharacterInfo(characterId);
                if (character == null)
                {
                    Logger.Warn("{0} Broker Can not find character: {1}", mServiceName, characterId);
                    return;
                }
                try
                {
                    var notifyLost = new ServiceDesc();
                    notifyLost.CharacterId = character.CharacterId;
                    notifyLost.ClientId = character.ClientId;
                    notifyLost.PacketId = 0;
                    notifyLost.FuncId = mServiceFunc + 503;

                    var __data__ = new __RPC_Logic_BSNotifyCharacterOnLost_ARG_uint64_clientId_uint64_characterId__();
                    __data__.CharacterId = character.CharacterId;
                    __data__.ClientId = character.ClientId;

                    using (var ms1 = new MemoryStream())
                    {
                        Serializer.Serialize(ms1, __data__);
                        notifyLost.Data = ms1.ToArray();
                    }

                    notifyLost.ServiceType = (int) mServiceType;
                    notifyLost.Type = (int) MessageType.BS;

                    character.Server.SendMessage(notifyLost);
                    ConnectLostLogger.Info("client {0} CommonBroker OnSocketListenerMessageReceiveClean 5", clientId);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Send message failed. 2");
                }
                //清理相关的数据clientid和characterid的数据
                RemoveClientIdCharacterId(clientId, characterId);
            }
        }

        public void OnSocketListenerMessageReceiveConnectEx(ServerClient client, ServiceDesc desc)
        {
            var clientId = desc.ClientId;
            var characterId = desc.CharacterId;
            //ClientRouting server;
            if (desc.ServiceType == 2)
            {
//连接的是Gate
                var gateGuid = (int) clientId;
                client.UserData = gateGuid;
                GateProxy gateProxy;
                if (mGates.TryGetValue(gateGuid, out gateProxy))
                {
                    gateProxy.Gate = client;
                }
                else
                {
                    mGates[gateGuid] = new GateProxy {Gate = client};
                }
            }
        }

        public virtual CharacterInfo OnSocketListenerMessageReceivePrepareDataEx(ServerClient client,
                                                                                 ServiceDesc desc,
                                                                                 bool isNeedSendReply = true)
        {
            //var clientId = desc.ClientId;
            //ClientRouting server;

            using (var ms = new MemoryStream(desc.Data))
            {
                var msg = Serializer.Deserialize<PrepareDataMessage>(ms);
                var characterId = msg.CharacterId;
                var clientId = msg.ClientId;
                GateProxy gate = null;
                if (client.UserData == null)
                {
                    if (!mGates.TryGetValue((int) (msg.ClientId >> 48), out gate))
                    {
                        Logger.Error(
                            "OnSocketListenerMessageReceivePrepareDataEx -------   mFromCharacterId2Server.TryGetValue Has -- {0}",
                            msg.ClientId);
                        return null;
                    }
                }
                else if (!mGates.TryGetValue((int) client.UserData, out gate))
                {
                    Logger.Error(
                        "OnSocketListenerMessageReceivePrepareDataEx -------   mFromCharacterId2Server.TryGetValue Has -- {0}",
                        msg.ClientId);
                    return null;
                }
                else
                {
                    if (clientId != gate.Gate.ClientId)
                    {
                        Logger.Error(
                            "OnSocketListenerMessageReceivePrepareDataEx -------   ClientId not same! old={0},new={1}",
                            characterId, clientId);
                    }
                }
                //创建角色管理
                var character = CreateCharacter(characterId);
                character.Server = SelectServerForCharacter(characterId);
                character.ServerId = msg.ServerId;
                character.Gate = gate;
                character.ClientId = clientId;
                mCharacterInfoManager.AddOrUpdate(characterId, character, (arg1, arg2) =>
                {
                    Logger.Error(
                        "OnSocketListenerMessageReceivePrepareDataEx -------   mFromCharacterId2Server.TryGetValue Has -- {0}",
                        characterId);
                    return character;
                });

                //统计服务器区的角色
                ConcurrentDictionary<ulong, int> bag = null;
                var ServerId = (uint) msg.ServerId;
                mFromServerId2CharacterId.AddOrUpdate(ServerId, key =>
                {
                    bag = new ConcurrentDictionary<ulong, int>();
                    return bag;
                }, (key, oldValue) =>
                {
                    bag = oldValue;
                    return oldValue;
                });

                bag.AddOrUpdate(msg.CharacterId, 0, (arg1, arg2) =>
                {
                    Logger.Error(
                        "OnSocketListenerMessageReceivePrepareDataEx -------   mFromServerId2CharacterIdbag Add Has,s={0},c={1}",
                        ServerId, characterId);
                    return 0;
                });

                PlayerLog.WriteLog(10003, "AddOrUpdate ClientId={0},characterId={1}", msg.ClientId, msg.CharacterId);

                //维护ClientId -> Character（考虑删除）
                mFromClientId2CharacterId.AddOrUpdate(msg.ClientId, msg.CharacterId, (arg1, arg2) =>
                {
                    Logger.Error("OnSocketListenerMessageReceivePrepareDataEx oldCharacterId={0},newCharacterId={1}",
                        arg2, characterId);
                    return msg.CharacterId;
                });
                if (isNeedSendReply)
                {
                    client.SendMessage(desc);
                }
                Logger.Info("Reply PrepareData {0} {1}", mServiceName, desc.CharacterId);
                return character;
            }
        }

        public void RemoveCharacterId(ulong clientId, ulong characterId)
        {
            CharacterInfo clientRouting;
            if (!mCharacterInfoManager.TryRemove(characterId, out clientRouting))
            {
                Logger.Error("Can not find character id for client 3: {0}", clientId);
                return;
            }
            PlayerLog.WriteLog(characterId, "RemoveCharacterId characterId={0}", characterId);
            var oldclientId = clientRouting.ClientId;
            if (oldclientId != clientId)
            {
                RemoveClientId(oldclientId, characterId);
            }
            //ConnectLostLogger.Info("client {0} - {1} CommonBroker Lost 3", clientId, characterId);
            ConcurrentDictionary<ulong, int> bag;
            if (mFromServerId2CharacterId.TryGetValue((uint) clientRouting.ServerId, out bag))
            {
                int tmp;
                bag.TryRemove(characterId, out tmp);
            }
            else
            {
                ConnectLostLogger.Info("client {0} - {1} CommonBroker Lost 3 - 2", clientId, characterId);
            }

            //desc.CharacterId = characterId;

            //告诉对应的逻辑服务器掉线
            //try
            //{
            //    if (clientRouting != null && clientRouting.Server != null)
            //    {
            //        clientRouting.Server.SendMessage(desc);
            //        ConnectLostLogger.Info("client {0} - {1} CommonBroker Lost 4", clientId, characterId);
            //    }
            //    else
            //    {
            //        ConnectLostLogger.Info("client {0} - {1} CommonBroker Lost 5", clientId, characterId);
            //    }
            //    ConnectLostLogger.Info("client {0} - {1} CommonBroker Lost 5", clientId, characterId);
            //}
            //catch (Exception ex)
            //{
            //    Logger.WarnException("Send message failed. 3", ex);
            //}
        }

        public void RemoveClientId(ulong clientId, ulong characterId)
        {
            ulong oldcharacterId;
            if (!mFromClientId2CharacterId.TryRemove(clientId, out oldcharacterId))
            {
                Logger.Error("Can not find clientId id for client 3: {0}", clientId);
                return;
            }
            if (oldcharacterId != characterId)
            {
                CharacterInfo clientRouting;
                PlayerLog.WriteLog(characterId, "RemoveClientId oldcharacterId={0},characterId={1}", oldcharacterId,
                    characterId);
                mCharacterInfoManager.TryRemove(oldcharacterId, out clientRouting);
            }
        }

        public void RemoveClientIdCharacterId(ulong clientId, ulong characterId)
        {
            RemoveClientId(clientId, characterId);
            RemoveCharacterId(clientId, characterId);
        }

        public int ConnectionCount
        {
            get { return mCharacterInfoManager.Count; }
        }

        public bool Connected
        {
            get { return mBackEnds.All(client => client.IsConnected); }
        }

        public long PackageCount { get; private set; }

        public class CharacterInfo
        {
            public CharacterInfo()
            {
                CharacterId = 0;
            }

            private ulong mClientId; //ClientId
            public ulong CharacterId { get; set; }

            public ulong ClientId
            {
                get { return mClientId; }
                set
                {
                    if (value == 0)
                    {
                        return;
                    }
                    mClientId = value;
                }
            }

            public GateProxy Gate { get; set; }
            public SocketClient Server { get; set; }
            public int ServerId { get; set; }
        }

        public class GateProxy
        {
            public ServerClient Gate;
        }
    }
}