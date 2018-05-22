using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Scorpion;
using DataContract;
using ProtoBuf;

#pragma warning disable 0162,0108
namespace ChatClientService
{

    public abstract class ChatAgent : ClientAgentBase
    {
        public ChatAgent(string addr)
            : base(addr)
        {
        }

        public ChatAgent(ServerInfo broker, ServerInfo[] directConnect, Func<ulong, int> characterId2ServerId)
            : base(broker, directConnect, characterId2ServerId)
        {
        }

        /// <summary>
        /// </summary>
        public PrepareDataForEnterGameOutMessage PrepareDataForEnterGame(ulong __characterId__, int serverId)
        {
            return new PrepareDataForEnterGameOutMessage(this, __characterId__, serverId);
        }

        /// <summary>
        /// </summary>
        public PrepareDataForCreateCharacterOutMessage PrepareDataForCreateCharacter(ulong __characterId__, int type)
        {
            return new PrepareDataForCreateCharacterOutMessage(this, __characterId__, type);
        }

        /// <summary>
        /// </summary>
        public PrepareDataForCommonUseOutMessage PrepareDataForCommonUse(ulong __characterId__, uint placeholder)
        {
            return new PrepareDataForCommonUseOutMessage(this, __characterId__, placeholder);
        }

        /// <summary>
        /// </summary>
        public PrepareDataForLogoutOutMessage PrepareDataForLogout(ulong __characterId__, uint placeholder)
        {
            return new PrepareDataForLogoutOutMessage(this, __characterId__, placeholder);
        }

        /// <summary>
        /// </summary>
        public CreateCharacterOutMessage CreateCharacter(ulong __characterId__, int type)
        {
            return new CreateCharacterOutMessage(this, __characterId__, type);
        }

        /// <summary>
        /// </summary>
        public DelectCharacterOutMessage DelectCharacter(ulong __characterId__, int type)
        {
            return new DelectCharacterOutMessage(this, __characterId__, type);
        }

        /// <summary>
        /// 获得某个逻辑服务器的所有在线CharacterId
        /// </summary>
        public SBGetAllOnlineCharacterInServerOutMessage SBGetAllOnlineCharacterInServer(ulong __characterId__, int serverId)
        {
            return new SBGetAllOnlineCharacterInServerOutMessage(this, __characterId__, serverId);
        }

        /// <summary>
        /// 检查客户端是否已经连接到对应服务器
        /// </summary>
        public CheckConnectedOutMessage CheckConnected(ulong __characterId__, ulong characterId)
        {
            return new CheckConnectedOutMessage(this, __characterId__, characterId);
        }

        /// <summary>
        /// 检查相应客户端连接是否已经断开
        /// </summary>
        public CheckLostOutMessage CheckLost(ulong __characterId__, ulong characterId)
        {
            return new CheckLostOutMessage(this, __characterId__, characterId);
        }

        /// <summary>
        /// 查询服务器状态
        /// </summary>
        public QueryStatusOutMessage QueryStatus(ulong __characterId__, uint placeholder)
        {
            return new QueryStatusOutMessage(this, __characterId__, placeholder);
        }

        /// <summary>
        /// Broker查询
        /// </summary>
        public QueryBrokerStatusOutMessage QueryBrokerStatus(ulong __characterId__, uint placeholder)
        {
            return new QueryBrokerStatusOutMessage(this, __characterId__, placeholder);
        }

        /// <summary>
        /// 广播表格重载
        /// </summary>
        public ServerGMCommandOutMessage ServerGMCommand(string cmd, string param)
        {
            return new ServerGMCommandOutMessage(this, 0, cmd, param);
        }

        /// <summary>
        /// 查询服务器状态，是否可以进入
        /// </summary>
        public ReadyToEnterOutMessage ReadyToEnter(int placeholder)
        {
            return new ReadyToEnterOutMessage(this, 0, placeholder);
        }

        /// <summary>
        /// 更新服务器
        /// </summary>
        public UpdateServerOutMessage UpdateServer(int placeholder)
        {
            return new UpdateServerOutMessage(this, 0, placeholder);
        }

        /// <summary>
        /// 增加最近联系人
        /// </summary>
        public AddRecentcontactsOutMessage AddRecentcontacts(ulong __characterId__, LogicSimpleData simpleData)
        {
            return new AddRecentcontactsOutMessage(this, __characterId__, simpleData);
        }

        /// <summary>
        /// 缓存私聊
        /// </summary>
        public CacheChatMessageOutMessage CacheChatMessage(ulong __characterId__, int chatType, ulong characterId, string characterName, ChatMessageContent content)
        {
            return new CacheChatMessageOutMessage(this, __characterId__, chatType, characterId, characterName, content);
        }

        /// <summary>
        /// 清楚某个Character和Client的相关数据，并转发到目前链接的下层服务器
        /// </summary>
        public SBCleanClientCharacterDataOutMessage SBCleanClientCharacterData(ulong __characterId__, ulong clientId, ulong characterId)
        {
            return new SBCleanClientCharacterDataOutMessage(this, __characterId__, clientId, characterId);
        }

        /// <summary>
        /// 通知某个角色开始 链接各游戏服务器了
        /// </summary>
        public SSNotifyCharacterOnConnetOutMessage SSNotifyCharacterOnConnet(ulong __characterId__, ulong clientId, ulong characterId)
        {
            return new SSNotifyCharacterOnConnetOutMessage(this, __characterId__, clientId, characterId);
        }

        /// <summary>
        /// 玩家禁言
        /// </summary>
        public SilenceOutMessage Silence(ulong __characterId__, uint mask)
        {
            return new SilenceOutMessage(this, __characterId__, mask);
        }

        /// <summary>
        /// 
        /// </summary>
        public GetSilenceStateOutMessage GetSilenceState(ulong __characterId__, int placeholder)
        {
            return new GetSilenceStateOutMessage(this, __characterId__, placeholder);
        }

        /// <summary>
        ///  广播送花信息
        /// </summary>
        public SSBroadcastAllServerMsgOutMessage SSBroadcastAllServerMsg(ulong __characterId__, int chatType, string charName, ChatMessageContent content)
        {
            return new SSBroadcastAllServerMsgOutMessage(this, __characterId__, chatType, charName, content);
        }

        /// <summary>
        /// </summary>
        public SSGetCurrentAnchorOutMessage SSGetCurrentAnchor(ulong __characterId__, int placeholder)
        {
            return new SSGetCurrentAnchorOutMessage(this, __characterId__, placeholder);
        }

        /// <summary>
        /// GM相关 begin  逻辑包放GM工具前
        /// </summary>
        public GMCommandOutMessage GMCommand(ulong __characterId__, StringArray commonds)
        {
            return new GMCommandOutMessage(this, __characterId__, commonds);
        }

        /// <summary>
        /// 拷贝一个角色数据到另一个角色id
        /// </summary>
        public CloneCharacterDbByIdOutMessage CloneCharacterDbById(ulong __characterId__, ulong fromId, ulong toId)
        {
            return new CloneCharacterDbByIdOutMessage(this, __characterId__, fromId, toId);
        }

        /// <summary>
        /// GM相关 end
        /// </summary>
        public NotifyPlayerEnterGameOutMessage NotifyPlayerEnterGame(ulong __characterId__, MsgChatMoniterData data)
        {
            return new NotifyPlayerEnterGameOutMessage(this, __characterId__, data);
        }

        /// <summary>
        /// </summary>
        public AddPlayerToChatMonitorOutMessage AddPlayerToChatMonitor(ulong __characterId__, int placeholder)
        {
            return new AddPlayerToChatMonitorOutMessage(this, __characterId__, placeholder);
        }

        /// <summary>
        /// </summary>
        public GetAnchorIsInRoomOutMessage GetAnchorIsInRoom(ulong __characterId__, int placeholder)
        {
            return new GetAnchorIsInRoomOutMessage(this, __characterId__, placeholder);
        }

        /// <summary>
        /// 通知修改玩家名字
        /// </summary>
        public NodifyModifyPlayerNameOutMessage NodifyModifyPlayerName(ulong characterId, string modifyName)
        {
            return new NodifyModifyPlayerNameOutMessage(this, 0, characterId, modifyName);
        }

        protected override object GetPublishData(uint p, byte[] list)
        {
            switch (p)
            {
                case 5000:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_PrepareDataForEnterGame_ARG_int32_serverId__>(ms);
                    }
                    break;
                case 5001:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_PrepareDataForCreateCharacter_ARG_int32_type__>(ms);
                    }
                    break;
                case 5002:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_PrepareDataForCommonUse_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 5003:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_PrepareDataForLogout_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 5015:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_CreateCharacter_ARG_int32_type__>(ms);
                    }
                    break;
                case 5016:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_DelectCharacter_ARG_int32_type__>(ms);
                    }
                    break;
                case 5030:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_SBGetAllOnlineCharacterInServer_ARG_int32_serverId__>(ms);
                    }
                    break;
                case 5031:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_CheckConnected_ARG_uint64_characterId__>(ms);
                    }
                    break;
                case 5032:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_CheckLost_ARG_uint64_characterId__>(ms);
                    }
                    break;
                case 5033:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_QueryStatus_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 5040:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_QueryBrokerStatus_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 5048:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_ServerGMCommand_ARG_string_cmd_string_param__>(ms);
                    }
                    break;
                case 5049:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_ReadyToEnter_ARG_int32_placeholder__>(ms);
                    }
                    break;
                case 5050:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_UpdateServer_ARG_int32_placeholder__>(ms);
                    }
                    break;
                case 5053:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_AddRecentcontacts_ARG_LogicSimpleData_simpleData__>(ms);
                    }
                    break;
                case 5057:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_CacheChatMessage_ARG_int32_chatType_uint64_characterId_string_characterName_ChatMessageContent_content__>(ms);
                    }
                    break;
                case 5500:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_SBCleanClientCharacterData_ARG_uint64_clientId_uint64_characterId__>(ms);
                    }
                    break;
                case 5501:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_SSNotifyCharacterOnConnet_ARG_uint64_clientId_uint64_characterId__>(ms);
                    }
                    break;
                case 5504:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_Silence_ARG_uint32_mask__>(ms);
                    }
                    break;
                case 5505:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_GetSilenceState_ARG_int32_placeholder__>(ms);
                    }
                    break;
                case 5507:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_SSBroadcastAllServerMsg_ARG_int32_chatType_string_charName_ChatMessageContent_content__>(ms);
                    }
                    break;
                case 5508:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_SSGetCurrentAnchor_ARG_int32_placeholder__>(ms);
                    }
                    break;
                case 5900:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_GMCommand_ARG_StringArray_commonds__>(ms);
                    }
                    break;
                case 5901:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_CloneCharacterDbById_ARG_uint64_fromId_uint64_toId__>(ms);
                    }
                    break;
                case 5902:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_NotifyPlayerEnterGame_ARG_MsgChatMoniterData_data__>(ms);
                    }
                    break;
                case 5903:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_AddPlayerToChatMonitor_ARG_int32_placeholder__>(ms);
                    }
                    break;
                case 5904:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_GetAnchorIsInRoom_ARG_int32_placeholder__>(ms);
                    }
                    break;
                case 5905:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Chat_NodifyModifyPlayerName_ARG_uint64_characterId_string_modifyName__>(ms);
                    }
                    break;
                default:
                    break;
            }

            return null;
        }


        protected override void DispatchPublishMessage(PublishMessageRecievedEvent evt)
        {
        }
        /// <summary>
        /// 接受聊天数据
        /// </summary>
        public object ChatNotify(IEnumerable<ulong> __characterIds__, int chatType, ulong characterId, string characterName, ChatMessageContent content)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 5043;
            desc.PacketId = 0;
            desc.Routing.AddRange(__characterIds__);
            if(desc.Routing.Count == 0) return null;
            desc.Type = (int)MessageType.SCList;
            desc.ServiceType = (int) ServiceType.Chat;

            var __data__ = new __RPC_Chat_ChatNotify_ARG_int32_chatType_uint64_characterId_string_characterName_ChatMessageContent_content__();
            __data__.ChatType=chatType;
            __data__.CharacterId=characterId;
            __data__.CharacterName=characterName;
            __data__.Content=content;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 聊天广播
        /// </summary>
        public object SyncChatMessage(ulong __characterId__, ulong __clientId__, int chatType, ulong characterId, string characterName, ChatMessageContent content)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 5046;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Chat;

            var __data__ = new __RPC_Chat_SyncChatMessage_ARG_int32_chatType_uint64_characterId_string_characterName_ChatMessageContent_content__();
            __data__.ChatType=chatType;
            __data__.CharacterId=characterId;
            __data__.CharacterName=characterName;
            __data__.Content=content;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 广播服务器数据
        /// </summary>
        public object BroadcastWorldMessage(uint __serverId__, int chatType, ulong characterId, string characterName, ChatMessageContent content)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 5047;
            desc.PacketId = __serverId__;
            desc.Type = (int)MessageType.SCServer;
            desc.ServiceType = (int) ServiceType.Chat;

            var __data__ = new __RPC_Chat_BroadcastWorldMessage_ARG_int32_chatType_uint64_characterId_string_characterName_ChatMessageContent_content__();
            __data__.ChatType=chatType;
            __data__.CharacterId=characterId;
            __data__.CharacterName=characterName;
            __data__.Content=content;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 同城频道的广播
        /// </summary>
        public object SyncToListCityChatMessage(IEnumerable<ulong> __characterIds__, int chatType, ulong characterId, string characterName, ChatMessageContent Content, string ChannelName)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 5056;
            desc.PacketId = 0;
            desc.Routing.AddRange(__characterIds__);
            if(desc.Routing.Count == 0) return null;
            desc.Type = (int)MessageType.SCList;
            desc.ServiceType = (int) ServiceType.Chat;

            var __data__ = new __RPC_Chat_SyncToListCityChatMessage_ARG_int32_chatType_uint64_characterId_string_characterName_ChatMessageContent_Content_string_ChannelName__();
            __data__.ChatType=chatType;
            __data__.CharacterId=characterId;
            __data__.CharacterName=characterName;
            __data__.Content=Content;
            __data__.ChannelName=ChannelName;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// </summary>
        public object BroadcastAnchorOnline(uint __serverId__, string charName, int online)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 5510;
            desc.PacketId = __serverId__;
            desc.Type = (int)MessageType.SCServer;
            desc.ServiceType = (int) ServiceType.Chat;

            var __data__ = new __RPC_Chat_BroadcastAnchorOnline_ARG_string_charName_int32_online__();
            __data__.CharName=charName;
            __data__.Online=online;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知所有在线客户端播放玫瑰特效
        /// </summary>
        public object NotifyChatRoseEffectChange(int chatType)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 5511;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SCAll;
            desc.ServiceType = (int) ServiceType.Chat;

            var __data__ = new __RPC_Chat_NotifyChatRoseEffectChange_ARG_int32_chatType__();
            __data__.ChatType=chatType;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 同步客户端主播已进入房间
        /// </summary>
        public object BroadcastAnchorEnterRoom(string charName)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 5513;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SCAll;
            desc.ServiceType = (int) ServiceType.Chat;

            var __data__ = new __RPC_Chat_BroadcastAnchorEnterRoom_ARG_string_charName__();
            __data__.CharName=charName;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
    }

    public class PrepareDataForEnterGameOutMessage : OutMessage
    {
        public PrepareDataForEnterGameOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId)
            : base(sender, ServiceType.Chat, 5000, (int)MessageType.SS)
        {
            Request = new __RPC_Chat_PrepareDataForEnterGame_ARG_int32_serverId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;

        }

        public __RPC_Chat_PrepareDataForEnterGame_ARG_int32_serverId__ Request { get; private set; }

            private __RPC_Chat_PrepareDataForEnterGame_RET_int32__ mResponse;
            public int Response { get { return mResponse.ReturnValue; } }

        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
            if (data != null)
            {
                var ms = new MemoryStream(data, false);
                mResponse = Serializer.Deserialize<__RPC_Chat_PrepareDataForEnterGame_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class PrepareDataForCreateCharacterOutMessage : OutMessage
    {
        public PrepareDataForCreateCharacterOutMessage(ClientAgentBase sender, ulong __characterId__, int type)
            : base(sender, ServiceType.Chat, 5001, (int)MessageType.SS)
        {
            Request = new __RPC_Chat_PrepareDataForCreateCharacter_ARG_int32_type__();
            mMessage.CharacterId = __characterId__;
            Request.Type=type;

        }

        public __RPC_Chat_PrepareDataForCreateCharacter_ARG_int32_type__ Request { get; private set; }

            private __RPC_Chat_PrepareDataForCreateCharacter_RET_int32__ mResponse;
            public int Response { get { return mResponse.ReturnValue; } }

        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
            if (data != null)
            {
                var ms = new MemoryStream(data, false);
                mResponse = Serializer.Deserialize<__RPC_Chat_PrepareDataForCreateCharacter_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class PrepareDataForCommonUseOutMessage : OutMessage
    {
        public PrepareDataForCommonUseOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Chat, 5002, (int)MessageType.SS)
        {
            Request = new __RPC_Chat_PrepareDataForCommonUse_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Chat_PrepareDataForCommonUse_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Chat_PrepareDataForCommonUse_RET_int32__ mResponse;
            public int Response { get { return mResponse.ReturnValue; } }

        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
            if (data != null)
            {
                var ms = new MemoryStream(data, false);
                mResponse = Serializer.Deserialize<__RPC_Chat_PrepareDataForCommonUse_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class PrepareDataForLogoutOutMessage : OutMessage
    {
        public PrepareDataForLogoutOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Chat, 5003, (int)MessageType.SS)
        {
            Request = new __RPC_Chat_PrepareDataForLogout_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Chat_PrepareDataForLogout_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Chat_PrepareDataForLogout_RET_int32__ mResponse;
            public int Response { get { return mResponse.ReturnValue; } }

        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
            if (data != null)
            {
                var ms = new MemoryStream(data, false);
                mResponse = Serializer.Deserialize<__RPC_Chat_PrepareDataForLogout_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class CreateCharacterOutMessage : OutMessage
    {
        public CreateCharacterOutMessage(ClientAgentBase sender, ulong __characterId__, int type)
            : base(sender, ServiceType.Chat, 5015, (int)MessageType.SS)
        {
            Request = new __RPC_Chat_CreateCharacter_ARG_int32_type__();
            mMessage.CharacterId = __characterId__;
            Request.Type=type;

        }

        public __RPC_Chat_CreateCharacter_ARG_int32_type__ Request { get; private set; }

            private __RPC_Chat_CreateCharacter_RET_int32__ mResponse;
            public int Response { get { return mResponse.ReturnValue; } }

        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
            if (data != null)
            {
                var ms = new MemoryStream(data, false);
                mResponse = Serializer.Deserialize<__RPC_Chat_CreateCharacter_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class DelectCharacterOutMessage : OutMessage
    {
        public DelectCharacterOutMessage(ClientAgentBase sender, ulong __characterId__, int type)
            : base(sender, ServiceType.Chat, 5016, (int)MessageType.SS)
        {
            Request = new __RPC_Chat_DelectCharacter_ARG_int32_type__();
            mMessage.CharacterId = __characterId__;
            Request.Type=type;

        }

        public __RPC_Chat_DelectCharacter_ARG_int32_type__ Request { get; private set; }

            private __RPC_Chat_DelectCharacter_RET_int32__ mResponse;
            public int Response { get { return mResponse.ReturnValue; } }

        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
            if (data != null)
            {
                var ms = new MemoryStream(data, false);
                mResponse = Serializer.Deserialize<__RPC_Chat_DelectCharacter_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SBGetAllOnlineCharacterInServerOutMessage : OutMessage
    {
        public SBGetAllOnlineCharacterInServerOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId)
            : base(sender, ServiceType.Chat, 5030, (int)MessageType.SB)
        {
            Request = new __RPC_Chat_SBGetAllOnlineCharacterInServer_ARG_int32_serverId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;

        }

        public __RPC_Chat_SBGetAllOnlineCharacterInServer_ARG_int32_serverId__ Request { get; private set; }

            private __RPC_Chat_SBGetAllOnlineCharacterInServer_RET_Uint64Array__ mResponse;
            public Uint64Array Response { get { return mResponse.ReturnValue; } }

        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
            if (data != null)
            {
                var ms = new MemoryStream(data, false);
                mResponse = Serializer.Deserialize<__RPC_Chat_SBGetAllOnlineCharacterInServer_RET_Uint64Array__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class CheckConnectedOutMessage : OutMessage
    {
        public CheckConnectedOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId)
            : base(sender, ServiceType.Chat, 5031, (int)MessageType.SS)
        {
            Request = new __RPC_Chat_CheckConnected_ARG_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;

        }

        public __RPC_Chat_CheckConnected_ARG_uint64_characterId__ Request { get; private set; }

            private __RPC_Chat_CheckConnected_RET_int32__ mResponse;
            public int Response { get { return mResponse.ReturnValue; } }

        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
            if (data != null)
            {
                var ms = new MemoryStream(data, false);
                mResponse = Serializer.Deserialize<__RPC_Chat_CheckConnected_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class CheckLostOutMessage : OutMessage
    {
        public CheckLostOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId)
            : base(sender, ServiceType.Chat, 5032, (int)MessageType.SS)
        {
            Request = new __RPC_Chat_CheckLost_ARG_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;

        }

        public __RPC_Chat_CheckLost_ARG_uint64_characterId__ Request { get; private set; }

            private __RPC_Chat_CheckLost_RET_int32__ mResponse;
            public int Response { get { return mResponse.ReturnValue; } }

        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
            if (data != null)
            {
                var ms = new MemoryStream(data, false);
                mResponse = Serializer.Deserialize<__RPC_Chat_CheckLost_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class QueryStatusOutMessage : OutMessage
    {
        public QueryStatusOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Chat, 5033, (int)MessageType.SS)
        {
            Request = new __RPC_Chat_QueryStatus_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Chat_QueryStatus_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Chat_QueryStatus_RET_ChatServerStatus__ mResponse;
            public ChatServerStatus Response { get { return mResponse.ReturnValue; } }

        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
            if (data != null)
            {
                var ms = new MemoryStream(data, false);
                mResponse = Serializer.Deserialize<__RPC_Chat_QueryStatus_RET_ChatServerStatus__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class QueryBrokerStatusOutMessage : OutMessage
    {
        public QueryBrokerStatusOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Chat, 5040, (int)MessageType.SB)
        {
            Request = new __RPC_Chat_QueryBrokerStatus_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Chat_QueryBrokerStatus_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Chat_QueryBrokerStatus_RET_CommonBrokerStatus__ mResponse;
            public CommonBrokerStatus Response { get { return mResponse.ReturnValue; } }

        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
            if (data != null)
            {
                var ms = new MemoryStream(data, false);
                mResponse = Serializer.Deserialize<__RPC_Chat_QueryBrokerStatus_RET_CommonBrokerStatus__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class ServerGMCommandOutMessage : OutMessage
    {
        public ServerGMCommandOutMessage(ClientAgentBase sender, ulong __characterId__, string cmd, string param)
            : base(sender, ServiceType.Chat, 5048, (int)MessageType.SAS)
        {
            Request = new __RPC_Chat_ServerGMCommand_ARG_string_cmd_string_param__();
            mMessage.CharacterId = __characterId__;
            Request.Cmd=cmd;
            Request.Param=param;

        }

        public __RPC_Chat_ServerGMCommand_ARG_string_cmd_string_param__ Request { get; private set; }


        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
        }
        public override bool HasReturnValue { get { return false; } }
    }

    public class ReadyToEnterOutMessage : OutMessage
    {
        public ReadyToEnterOutMessage(ClientAgentBase sender, ulong __characterId__, int placeholder)
            : base(sender, ServiceType.Chat, 5049, (int)MessageType.SAS)
        {
            Request = new __RPC_Chat_ReadyToEnter_ARG_int32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Chat_ReadyToEnter_ARG_int32_placeholder__ Request { get; private set; }

            public List<int> Response { get; private set; }

        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
            if (data != null)
            {
                var ms = new MemoryStream(data, false);
                if(Response == null) Response = new List<int>();
                Response.Add(Serializer.Deserialize<__RPC_Chat_ReadyToEnter_RET_int32__>(ms).ReturnValue);
            }
            State = MessageState.Reply;
            if(ErrorCode == 0) ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class UpdateServerOutMessage : OutMessage
    {
        public UpdateServerOutMessage(ClientAgentBase sender, ulong __characterId__, int placeholder)
            : base(sender, ServiceType.Chat, 5050, (int)MessageType.SAS)
        {
            Request = new __RPC_Chat_UpdateServer_ARG_int32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Chat_UpdateServer_ARG_int32_placeholder__ Request { get; private set; }


        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
        }
        public override bool HasReturnValue { get { return false; } }
    }

    public class AddRecentcontactsOutMessage : OutMessage
    {
        public AddRecentcontactsOutMessage(ClientAgentBase sender, ulong __characterId__, LogicSimpleData simpleData)
            : base(sender, ServiceType.Chat, 5053, (int)MessageType.SS)
        {
            Request = new __RPC_Chat_AddRecentcontacts_ARG_LogicSimpleData_simpleData__();
            mMessage.CharacterId = __characterId__;
            Request.SimpleData=simpleData;

        }

        public __RPC_Chat_AddRecentcontacts_ARG_LogicSimpleData_simpleData__ Request { get; private set; }


        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
        }
        public override bool HasReturnValue { get { return false; } }
    }

    public class CacheChatMessageOutMessage : OutMessage
    {
        public CacheChatMessageOutMessage(ClientAgentBase sender, ulong __characterId__, int chatType, ulong characterId, string characterName, ChatMessageContent content)
            : base(sender, ServiceType.Chat, 5057, (int)MessageType.SS)
        {
            Request = new __RPC_Chat_CacheChatMessage_ARG_int32_chatType_uint64_characterId_string_characterName_ChatMessageContent_content__();
            mMessage.CharacterId = __characterId__;
            Request.ChatType=chatType;
            Request.CharacterId=characterId;
            Request.CharacterName=characterName;
            Request.Content=content;

        }

        public __RPC_Chat_CacheChatMessage_ARG_int32_chatType_uint64_characterId_string_characterName_ChatMessageContent_content__ Request { get; private set; }


        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
        }
        public override bool HasReturnValue { get { return false; } }
    }

    public class SBCleanClientCharacterDataOutMessage : OutMessage
    {
        public SBCleanClientCharacterDataOutMessage(ClientAgentBase sender, ulong __characterId__, ulong clientId, ulong characterId)
            : base(sender, ServiceType.Chat, 5500, (int)MessageType.SB)
        {
            Request = new __RPC_Chat_SBCleanClientCharacterData_ARG_uint64_clientId_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.ClientId=clientId;
            Request.CharacterId=characterId;

        }

        public __RPC_Chat_SBCleanClientCharacterData_ARG_uint64_clientId_uint64_characterId__ Request { get; private set; }


        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
        }
        public override bool HasReturnValue { get { return false; } }
    }

    public class SSNotifyCharacterOnConnetOutMessage : OutMessage
    {
        public SSNotifyCharacterOnConnetOutMessage(ClientAgentBase sender, ulong __characterId__, ulong clientId, ulong characterId)
            : base(sender, ServiceType.Chat, 5501, (int)MessageType.SS)
        {
            Request = new __RPC_Chat_SSNotifyCharacterOnConnet_ARG_uint64_clientId_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.ClientId=clientId;
            Request.CharacterId=characterId;

        }

        public __RPC_Chat_SSNotifyCharacterOnConnet_ARG_uint64_clientId_uint64_characterId__ Request { get; private set; }

            private __RPC_Chat_SSNotifyCharacterOnConnet_RET_int32__ mResponse;
            public int Response { get { return mResponse.ReturnValue; } }

        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
            if (data != null)
            {
                var ms = new MemoryStream(data, false);
                mResponse = Serializer.Deserialize<__RPC_Chat_SSNotifyCharacterOnConnet_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SilenceOutMessage : OutMessage
    {
        public SilenceOutMessage(ClientAgentBase sender, ulong __characterId__, uint mask)
            : base(sender, ServiceType.Chat, 5504, (int)MessageType.SS)
        {
            Request = new __RPC_Chat_Silence_ARG_uint32_mask__();
            mMessage.CharacterId = __characterId__;
            Request.Mask=mask;

        }

        public __RPC_Chat_Silence_ARG_uint32_mask__ Request { get; private set; }

            private __RPC_Chat_Silence_RET_int32__ mResponse;
            public int Response { get { return mResponse.ReturnValue; } }

        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
            if (data != null)
            {
                var ms = new MemoryStream(data, false);
                mResponse = Serializer.Deserialize<__RPC_Chat_Silence_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetSilenceStateOutMessage : OutMessage
    {
        public GetSilenceStateOutMessage(ClientAgentBase sender, ulong __characterId__, int placeholder)
            : base(sender, ServiceType.Chat, 5505, (int)MessageType.SS)
        {
            Request = new __RPC_Chat_GetSilenceState_ARG_int32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Chat_GetSilenceState_ARG_int32_placeholder__ Request { get; private set; }

            private __RPC_Chat_GetSilenceState_RET_uint32__ mResponse;
            public uint Response { get { return mResponse.ReturnValue; } }

        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
            if (data != null)
            {
                var ms = new MemoryStream(data, false);
                mResponse = Serializer.Deserialize<__RPC_Chat_GetSilenceState_RET_uint32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSBroadcastAllServerMsgOutMessage : OutMessage
    {
        public SSBroadcastAllServerMsgOutMessage(ClientAgentBase sender, ulong __characterId__, int chatType, string charName, ChatMessageContent content)
            : base(sender, ServiceType.Chat, 5507, (int)MessageType.SS)
        {
            Request = new __RPC_Chat_SSBroadcastAllServerMsg_ARG_int32_chatType_string_charName_ChatMessageContent_content__();
            mMessage.CharacterId = __characterId__;
            Request.ChatType=chatType;
            Request.CharName=charName;
            Request.Content=content;

        }

        public __RPC_Chat_SSBroadcastAllServerMsg_ARG_int32_chatType_string_charName_ChatMessageContent_content__ Request { get; private set; }

            private __RPC_Chat_SSBroadcastAllServerMsg_RET_int32__ mResponse;
            public int Response { get { return mResponse.ReturnValue; } }

        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
            if (data != null)
            {
                var ms = new MemoryStream(data, false);
                mResponse = Serializer.Deserialize<__RPC_Chat_SSBroadcastAllServerMsg_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSGetCurrentAnchorOutMessage : OutMessage
    {
        public SSGetCurrentAnchorOutMessage(ClientAgentBase sender, ulong __characterId__, int placeholder)
            : base(sender, ServiceType.Chat, 5508, (int)MessageType.SS)
        {
            Request = new __RPC_Chat_SSGetCurrentAnchor_ARG_int32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Chat_SSGetCurrentAnchor_ARG_int32_placeholder__ Request { get; private set; }

            private __RPC_Chat_SSGetCurrentAnchor_RET_uint64__ mResponse;
            public ulong Response { get { return mResponse.ReturnValue; } }

        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
            if (data != null)
            {
                var ms = new MemoryStream(data, false);
                mResponse = Serializer.Deserialize<__RPC_Chat_SSGetCurrentAnchor_RET_uint64__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GMCommandOutMessage : OutMessage
    {
        public GMCommandOutMessage(ClientAgentBase sender, ulong __characterId__, StringArray commonds)
            : base(sender, ServiceType.Chat, 5900, (int)MessageType.SS)
        {
            Request = new __RPC_Chat_GMCommand_ARG_StringArray_commonds__();
            mMessage.CharacterId = __characterId__;
            Request.Commonds=commonds;

        }

        public __RPC_Chat_GMCommand_ARG_StringArray_commonds__ Request { get; private set; }

            private __RPC_Chat_GMCommand_RET_Int32Array__ mResponse;
            public Int32Array Response { get { return mResponse.ReturnValue; } }

        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
            if (data != null)
            {
                var ms = new MemoryStream(data, false);
                mResponse = Serializer.Deserialize<__RPC_Chat_GMCommand_RET_Int32Array__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class CloneCharacterDbByIdOutMessage : OutMessage
    {
        public CloneCharacterDbByIdOutMessage(ClientAgentBase sender, ulong __characterId__, ulong fromId, ulong toId)
            : base(sender, ServiceType.Chat, 5901, (int)MessageType.SS)
        {
            Request = new __RPC_Chat_CloneCharacterDbById_ARG_uint64_fromId_uint64_toId__();
            mMessage.CharacterId = __characterId__;
            Request.FromId=fromId;
            Request.ToId=toId;

        }

        public __RPC_Chat_CloneCharacterDbById_ARG_uint64_fromId_uint64_toId__ Request { get; private set; }

            private __RPC_Chat_CloneCharacterDbById_RET_int32__ mResponse;
            public int Response { get { return mResponse.ReturnValue; } }

        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
            if (data != null)
            {
                var ms = new MemoryStream(data, false);
                mResponse = Serializer.Deserialize<__RPC_Chat_CloneCharacterDbById_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class NotifyPlayerEnterGameOutMessage : OutMessage
    {
        public NotifyPlayerEnterGameOutMessage(ClientAgentBase sender, ulong __characterId__, MsgChatMoniterData data)
            : base(sender, ServiceType.Chat, 5902, (int)MessageType.SS)
        {
            Request = new __RPC_Chat_NotifyPlayerEnterGame_ARG_MsgChatMoniterData_data__();
            mMessage.CharacterId = __characterId__;
            Request.Data=data;

        }

        public __RPC_Chat_NotifyPlayerEnterGame_ARG_MsgChatMoniterData_data__ Request { get; private set; }


        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
        }
        public override bool HasReturnValue { get { return false; } }
    }

    public class AddPlayerToChatMonitorOutMessage : OutMessage
    {
        public AddPlayerToChatMonitorOutMessage(ClientAgentBase sender, ulong __characterId__, int placeholder)
            : base(sender, ServiceType.Chat, 5903, (int)MessageType.SS)
        {
            Request = new __RPC_Chat_AddPlayerToChatMonitor_ARG_int32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Chat_AddPlayerToChatMonitor_ARG_int32_placeholder__ Request { get; private set; }


        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
        }
        public override bool HasReturnValue { get { return false; } }
    }

    public class GetAnchorIsInRoomOutMessage : OutMessage
    {
        public GetAnchorIsInRoomOutMessage(ClientAgentBase sender, ulong __characterId__, int placeholder)
            : base(sender, ServiceType.Chat, 5904, (int)MessageType.SS)
        {
            Request = new __RPC_Chat_GetAnchorIsInRoom_ARG_int32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Chat_GetAnchorIsInRoom_ARG_int32_placeholder__ Request { get; private set; }

            private __RPC_Chat_GetAnchorIsInRoom_RET_int32__ mResponse;
            public int Response { get { return mResponse.ReturnValue; } }

        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
            if (data != null)
            {
                var ms = new MemoryStream(data, false);
                mResponse = Serializer.Deserialize<__RPC_Chat_GetAnchorIsInRoom_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class NodifyModifyPlayerNameOutMessage : OutMessage
    {
        public NodifyModifyPlayerNameOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId, string modifyName)
            : base(sender, ServiceType.Chat, 5905, (int)MessageType.SAS)
        {
            Request = new __RPC_Chat_NodifyModifyPlayerName_ARG_uint64_characterId_string_modifyName__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;
            Request.ModifyName=modifyName;

        }

        public __RPC_Chat_NodifyModifyPlayerName_ARG_uint64_characterId_string_modifyName__ Request { get; private set; }


        protected override byte[] Serialize(MemoryStream s)
        {
            Serializer.Serialize(s, Request);
            return s.ToArray();
        }

        public override void SetResponse(uint error, byte[] data)
        {
        }
        public override bool HasReturnValue { get { return false; } }
    }

    public class AddFunctionNameChat
    {
        public static void AddFunctionName(IDictionary<int, string> dict)
        {
            dict[5000] = "PrepareDataForEnterGame";
            dict[5001] = "PrepareDataForCreateCharacter";
            dict[5002] = "PrepareDataForCommonUse";
            dict[5003] = "PrepareDataForLogout";
            dict[5015] = "CreateCharacter";
            dict[5016] = "DelectCharacter";
            dict[5030] = "SBGetAllOnlineCharacterInServer";
            dict[5031] = "CheckConnected";
            dict[5032] = "CheckLost";
            dict[5033] = "QueryStatus";
            dict[5040] = "QueryBrokerStatus";
            dict[5041] = "GMChat";
            dict[5042] = "ChatChatMessage";
            dict[5043] = "ChatNotify";
            dict[5044] = "SendHornMessage";
            dict[5046] = "SyncChatMessage";
            dict[5047] = "BroadcastWorldMessage";
            dict[5048] = "ServerGMCommand";
            dict[5049] = "ReadyToEnter";
            dict[5050] = "UpdateServer";
            dict[5051] = "GetRecentcontacts";
            dict[5052] = "DeleteRecentcontacts";
            dict[5053] = "AddRecentcontacts";
            dict[5054] = "EnterChannel";
            dict[5055] = "LeaveChannel";
            dict[5056] = "SyncToListCityChatMessage";
            dict[5057] = "CacheChatMessage";
            dict[5500] = "SBCleanClientCharacterData";
            dict[5501] = "SSNotifyCharacterOnConnet";
            dict[5503] = "BSNotifyCharacterOnLost";
            dict[5504] = "Silence";
            dict[5505] = "GetSilenceState";
            dict[5506] = "ApplyAnchorRoomInfo";
            dict[5507] = "SSBroadcastAllServerMsg";
            dict[5508] = "SSGetCurrentAnchor";
            dict[5509] = "PresentGift";
            dict[5510] = "BroadcastAnchorOnline";
            dict[5511] = "NotifyChatRoseEffectChange";
            dict[5512] = "NotifyAnchorEnterRoomChange";
            dict[5513] = "BroadcastAnchorEnterRoom";
            dict[5514] = "AnchorExitRoom";
            dict[5900] = "GMCommand";
            dict[5901] = "CloneCharacterDbById";
            dict[5902] = "NotifyPlayerEnterGame";
            dict[5903] = "AddPlayerToChatMonitor";
            dict[5904] = "GetAnchorIsInRoom";
            dict[5905] = "NodifyModifyPlayerName";
        }
        public static void AddCSFunctionName(IDictionary<int, string> dict)
        {
            dict[5041] = "GMChat";
            dict[5042] = "ChatChatMessage";
            dict[5044] = "SendHornMessage";
            dict[5051] = "GetRecentcontacts";
            dict[5052] = "DeleteRecentcontacts";
            dict[5054] = "EnterChannel";
            dict[5055] = "LeaveChannel";
            dict[5506] = "ApplyAnchorRoomInfo";
            dict[5509] = "PresentGift";
            dict[5512] = "NotifyAnchorEnterRoomChange";
            dict[5514] = "AnchorExitRoom";
        }
    }
}
