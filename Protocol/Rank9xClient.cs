using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Scorpion;
using DataContract;
using ProtoBuf;

#pragma warning disable 0162,0108
namespace RankClientService
{

    public abstract class RankAgent : ClientAgentBase
    {
        public RankAgent(string addr)
            : base(addr)
        {
        }

        public RankAgent(ServerInfo broker, ServerInfo[] directConnect, Func<ulong, int> characterId2ServerId)
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
        /// 修改等级数据
        /// </summary>
        public CharacterChangeLevelOutMessage CharacterChangeLevel(ulong __characterId__, int serverId, ulong guid, string name, int level, int exp)
        {
            return new CharacterChangeLevelOutMessage(this, __characterId__, serverId, guid, name, level, exp);
        }

        /// <summary>
        /// 修改其他数据
        /// </summary>
        public CharacterChangeDataOutMessage CharacterChangeData(ulong __characterId__, int rankType, int serverId, ulong guid, string name, long value)
        {
            return new CharacterChangeDataOutMessage(this, __characterId__, rankType, serverId, guid, name, value);
        }

        /// <summary>
        /// 获取某个玩家的天梯对手
        /// </summary>
        public Rank_GetP1vP1ListOutMessage Rank_GetP1vP1List(ulong __characterId__, int serverId, ulong characterId, string name)
        {
            return new Rank_GetP1vP1ListOutMessage(this, __characterId__, serverId, characterId, name);
        }

        /// <summary>
        /// 查询一个玩家的名次是否改变
        /// </summary>
        public CompareRankOutMessage CompareRank(ulong __characterId__, int serverId, ulong characterId, int rank)
        {
            return new CompareRankOutMessage(this, __characterId__, serverId, characterId, rank);
        }

        /// <summary>
        /// 天梯结果有改变
        /// </summary>
        public RankP1vP1FightOverOutMessage RankP1vP1FightOver(ulong __characterId__, int serverId, ulong characterId, ulong pvpCharacterId, int result, string name, string pvpName)
        {
            return new RankP1vP1FightOverOutMessage(this, __characterId__, serverId, characterId, pvpCharacterId, result, name, pvpName);
        }

        /// <summary>
        /// 广博GM命令
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
        /// 玩家的排行榜数据修改
        /// </summary>
        public SSCharacterChangeDataListOutMessage SSCharacterChangeDataList(ulong __characterId__, RankChangeDataList changes)
        {
            return new SSCharacterChangeDataListOutMessage(this, __characterId__, changes);
        }

        /// <summary>
        /// 更新服务器
        /// </summary>
        public UpdateServerOutMessage UpdateServer(int placeholder)
        {
            return new UpdateServerOutMessage(this, 0, placeholder);
        }

        /// <summary>
        /// 获取rank数据
        /// </summary>
        public GetRankValueOutMessage GetRankValue(ulong __characterId__, int serverId, int rankType, int idx)
        {
            return new GetRankValueOutMessage(this, __characterId__, serverId, rankType, idx);
        }

        /// <summary>
        /// 获取排行榜数据
        /// </summary>
        public SSGetServerRankDataOutMessage SSGetServerRankData(ulong __characterId__, int serverId, int ranktype)
        {
            return new SSGetServerRankDataOutMessage(this, __characterId__, serverId, ranktype);
        }

        /// <summary>
        /// 运营活动获取排行榜数据
        /// </summary>
        public SSGetRankDataByServerIdOutMessage SSGetRankDataByServerId(ulong __characterId__, Int32Array serverList, long time, int ranktype)
        {
            return new SSGetRankDataByServerIdOutMessage(this, __characterId__, serverList, time, ranktype);
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
        /// GM相关 begin  逻辑包放GM工具前
        /// </summary>
        public GMCommandOutMessage GMCommand(ulong __characterId__, StringArray commonds)
        {
            return new GMCommandOutMessage(this, __characterId__, commonds);
        }

        /// <summary>
        /// GM相关 end
        /// 通知修改玩家名字
        /// </summary>
        public NodifyModifyPlayerNameOutMessage NodifyModifyPlayerName(ulong __characterId__, int serverId, ulong guid, string modifyName)
        {
            return new NodifyModifyPlayerNameOutMessage(this, __characterId__, serverId, guid, modifyName);
        }

        protected override object GetPublishData(uint p, byte[] list)
        {
            switch (p)
            {
                case 6000:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Rank_PrepareDataForEnterGame_ARG_int32_serverId__>(ms);
                    }
                    break;
                case 6001:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Rank_PrepareDataForCreateCharacter_ARG_int32_type__>(ms);
                    }
                    break;
                case 6002:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Rank_PrepareDataForCommonUse_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 6003:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Rank_PrepareDataForLogout_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 6030:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Rank_SBGetAllOnlineCharacterInServer_ARG_int32_serverId__>(ms);
                    }
                    break;
                case 6031:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Rank_CheckConnected_ARG_uint64_characterId__>(ms);
                    }
                    break;
                case 6032:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Rank_CheckLost_ARG_uint64_characterId__>(ms);
                    }
                    break;
                case 6033:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Rank_QueryStatus_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 6040:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Rank_QueryBrokerStatus_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 6041:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Rank_CharacterChangeLevel_ARG_int32_serverId_uint64_guid_string_name_int32_level_int32_exp__>(ms);
                    }
                    break;
                case 6042:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Rank_CharacterChangeData_ARG_int32_rankType_int32_serverId_uint64_guid_string_name_int64_value__>(ms);
                    }
                    break;
                case 6044:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Rank_Rank_GetP1vP1List_ARG_int32_serverId_uint64_characterId_string_name__>(ms);
                    }
                    break;
                case 6045:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Rank_CompareRank_ARG_int32_serverId_uint64_characterId_int32_rank__>(ms);
                    }
                    break;
                case 6046:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Rank_RankP1vP1FightOver_ARG_int32_serverId_uint64_characterId_uint64_pvpCharacterId_int32_result_string_name_string_pvpName__>(ms);
                    }
                    break;
                case 6048:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Rank_ServerGMCommand_ARG_string_cmd_string_param__>(ms);
                    }
                    break;
                case 6049:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Rank_ReadyToEnter_ARG_int32_placeholder__>(ms);
                    }
                    break;
                case 6050:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Rank_SSCharacterChangeDataList_ARG_RankChangeDataList_changes__>(ms);
                    }
                    break;
                case 6051:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Rank_UpdateServer_ARG_int32_placeholder__>(ms);
                    }
                    break;
                case 6052:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Rank_GetRankValue_ARG_int32_serverId_int32_rankType_int32_idx__>(ms);
                    }
                    break;
                case 6054:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Rank_SSGetServerRankData_ARG_int32_serverId_int32_ranktype__>(ms);
                    }
                    break;
                case 6055:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Rank_SSGetRankDataByServerId_ARG_Int32Array_serverList_int64_time_int32_ranktype__>(ms);
                    }
                    break;
                case 6500:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Rank_SBCleanClientCharacterData_ARG_uint64_clientId_uint64_characterId__>(ms);
                    }
                    break;
                case 6501:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Rank_SSNotifyCharacterOnConnet_ARG_uint64_clientId_uint64_characterId__>(ms);
                    }
                    break;
                case 6900:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Rank_GMCommand_ARG_StringArray_commonds__>(ms);
                    }
                    break;
                case 6901:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Rank_NodifyModifyPlayerName_ARG_int32_serverId_uint64_guid_string_modifyName__>(ms);
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
    }

    public class PrepareDataForEnterGameOutMessage : OutMessage
    {
        public PrepareDataForEnterGameOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId)
            : base(sender, ServiceType.Rank, 6000, (int)MessageType.SS)
        {
            Request = new __RPC_Rank_PrepareDataForEnterGame_ARG_int32_serverId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;

        }

        public __RPC_Rank_PrepareDataForEnterGame_ARG_int32_serverId__ Request { get; private set; }

            private __RPC_Rank_PrepareDataForEnterGame_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Rank_PrepareDataForEnterGame_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class PrepareDataForCreateCharacterOutMessage : OutMessage
    {
        public PrepareDataForCreateCharacterOutMessage(ClientAgentBase sender, ulong __characterId__, int type)
            : base(sender, ServiceType.Rank, 6001, (int)MessageType.SS)
        {
            Request = new __RPC_Rank_PrepareDataForCreateCharacter_ARG_int32_type__();
            mMessage.CharacterId = __characterId__;
            Request.Type=type;

        }

        public __RPC_Rank_PrepareDataForCreateCharacter_ARG_int32_type__ Request { get; private set; }

            private __RPC_Rank_PrepareDataForCreateCharacter_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Rank_PrepareDataForCreateCharacter_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class PrepareDataForCommonUseOutMessage : OutMessage
    {
        public PrepareDataForCommonUseOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Rank, 6002, (int)MessageType.SS)
        {
            Request = new __RPC_Rank_PrepareDataForCommonUse_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Rank_PrepareDataForCommonUse_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Rank_PrepareDataForCommonUse_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Rank_PrepareDataForCommonUse_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class PrepareDataForLogoutOutMessage : OutMessage
    {
        public PrepareDataForLogoutOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Rank, 6003, (int)MessageType.SS)
        {
            Request = new __RPC_Rank_PrepareDataForLogout_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Rank_PrepareDataForLogout_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Rank_PrepareDataForLogout_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Rank_PrepareDataForLogout_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SBGetAllOnlineCharacterInServerOutMessage : OutMessage
    {
        public SBGetAllOnlineCharacterInServerOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId)
            : base(sender, ServiceType.Rank, 6030, (int)MessageType.SB)
        {
            Request = new __RPC_Rank_SBGetAllOnlineCharacterInServer_ARG_int32_serverId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;

        }

        public __RPC_Rank_SBGetAllOnlineCharacterInServer_ARG_int32_serverId__ Request { get; private set; }

            private __RPC_Rank_SBGetAllOnlineCharacterInServer_RET_Uint64Array__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Rank_SBGetAllOnlineCharacterInServer_RET_Uint64Array__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class CheckConnectedOutMessage : OutMessage
    {
        public CheckConnectedOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId)
            : base(sender, ServiceType.Rank, 6031, (int)MessageType.SS)
        {
            Request = new __RPC_Rank_CheckConnected_ARG_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;

        }

        public __RPC_Rank_CheckConnected_ARG_uint64_characterId__ Request { get; private set; }

            private __RPC_Rank_CheckConnected_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Rank_CheckConnected_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class CheckLostOutMessage : OutMessage
    {
        public CheckLostOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId)
            : base(sender, ServiceType.Rank, 6032, (int)MessageType.SS)
        {
            Request = new __RPC_Rank_CheckLost_ARG_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;

        }

        public __RPC_Rank_CheckLost_ARG_uint64_characterId__ Request { get; private set; }

            private __RPC_Rank_CheckLost_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Rank_CheckLost_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class QueryStatusOutMessage : OutMessage
    {
        public QueryStatusOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Rank, 6033, (int)MessageType.SS)
        {
            Request = new __RPC_Rank_QueryStatus_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Rank_QueryStatus_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Rank_QueryStatus_RET_RankServerStatus__ mResponse;
            public RankServerStatus Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Rank_QueryStatus_RET_RankServerStatus__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class QueryBrokerStatusOutMessage : OutMessage
    {
        public QueryBrokerStatusOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Rank, 6040, (int)MessageType.SB)
        {
            Request = new __RPC_Rank_QueryBrokerStatus_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Rank_QueryBrokerStatus_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Rank_QueryBrokerStatus_RET_CommonBrokerStatus__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Rank_QueryBrokerStatus_RET_CommonBrokerStatus__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class CharacterChangeLevelOutMessage : OutMessage
    {
        public CharacterChangeLevelOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, ulong guid, string name, int level, int exp)
            : base(sender, ServiceType.Rank, 6041, (int)MessageType.SS)
        {
            Request = new __RPC_Rank_CharacterChangeLevel_ARG_int32_serverId_uint64_guid_string_name_int32_level_int32_exp__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.Guid=guid;
            Request.Name=name;
            Request.Level=level;
            Request.Exp=exp;

        }

        public __RPC_Rank_CharacterChangeLevel_ARG_int32_serverId_uint64_guid_string_name_int32_level_int32_exp__ Request { get; private set; }


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

    public class CharacterChangeDataOutMessage : OutMessage
    {
        public CharacterChangeDataOutMessage(ClientAgentBase sender, ulong __characterId__, int rankType, int serverId, ulong guid, string name, long value)
            : base(sender, ServiceType.Rank, 6042, (int)MessageType.SS)
        {
            Request = new __RPC_Rank_CharacterChangeData_ARG_int32_rankType_int32_serverId_uint64_guid_string_name_int64_value__();
            mMessage.CharacterId = __characterId__;
            Request.RankType=rankType;
            Request.ServerId=serverId;
            Request.Guid=guid;
            Request.Name=name;
            Request.Value=value;

        }

        public __RPC_Rank_CharacterChangeData_ARG_int32_rankType_int32_serverId_uint64_guid_string_name_int64_value__ Request { get; private set; }


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

    public class Rank_GetP1vP1ListOutMessage : OutMessage
    {
        public Rank_GetP1vP1ListOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, ulong characterId, string name)
            : base(sender, ServiceType.Rank, 6044, (int)MessageType.SS)
        {
            Request = new __RPC_Rank_Rank_GetP1vP1List_ARG_int32_serverId_uint64_characterId_string_name__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.CharacterId=characterId;
            Request.Name=name;

        }

        public __RPC_Rank_Rank_GetP1vP1List_ARG_int32_serverId_uint64_characterId_string_name__ Request { get; private set; }

            private __RPC_Rank_Rank_GetP1vP1List_RET_CharacterLadderDataInfo__ mResponse;
            public CharacterLadderDataInfo Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Rank_Rank_GetP1vP1List_RET_CharacterLadderDataInfo__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class CompareRankOutMessage : OutMessage
    {
        public CompareRankOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, ulong characterId, int rank)
            : base(sender, ServiceType.Rank, 6045, (int)MessageType.SS)
        {
            Request = new __RPC_Rank_CompareRank_ARG_int32_serverId_uint64_characterId_int32_rank__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.CharacterId=characterId;
            Request.Rank=rank;

        }

        public __RPC_Rank_CompareRank_ARG_int32_serverId_uint64_characterId_int32_rank__ Request { get; private set; }

            private __RPC_Rank_CompareRank_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Rank_CompareRank_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class RankP1vP1FightOverOutMessage : OutMessage
    {
        public RankP1vP1FightOverOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, ulong characterId, ulong pvpCharacterId, int result, string name, string pvpName)
            : base(sender, ServiceType.Rank, 6046, (int)MessageType.SS)
        {
            Request = new __RPC_Rank_RankP1vP1FightOver_ARG_int32_serverId_uint64_characterId_uint64_pvpCharacterId_int32_result_string_name_string_pvpName__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.CharacterId=characterId;
            Request.PvpCharacterId=pvpCharacterId;
            Request.Result=result;
            Request.Name=name;
            Request.PvpName=pvpName;

        }

        public __RPC_Rank_RankP1vP1FightOver_ARG_int32_serverId_uint64_characterId_uint64_pvpCharacterId_int32_result_string_name_string_pvpName__ Request { get; private set; }


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

    public class ServerGMCommandOutMessage : OutMessage
    {
        public ServerGMCommandOutMessage(ClientAgentBase sender, ulong __characterId__, string cmd, string param)
            : base(sender, ServiceType.Rank, 6048, (int)MessageType.SAS)
        {
            Request = new __RPC_Rank_ServerGMCommand_ARG_string_cmd_string_param__();
            mMessage.CharacterId = __characterId__;
            Request.Cmd=cmd;
            Request.Param=param;

        }

        public __RPC_Rank_ServerGMCommand_ARG_string_cmd_string_param__ Request { get; private set; }


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
            : base(sender, ServiceType.Rank, 6049, (int)MessageType.SAS)
        {
            Request = new __RPC_Rank_ReadyToEnter_ARG_int32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Rank_ReadyToEnter_ARG_int32_placeholder__ Request { get; private set; }

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
                Response.Add(Serializer.Deserialize<__RPC_Rank_ReadyToEnter_RET_int32__>(ms).ReturnValue);
            }
            State = MessageState.Reply;
            if(ErrorCode == 0) ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSCharacterChangeDataListOutMessage : OutMessage
    {
        public SSCharacterChangeDataListOutMessage(ClientAgentBase sender, ulong __characterId__, RankChangeDataList changes)
            : base(sender, ServiceType.Rank, 6050, (int)MessageType.SS)
        {
            Request = new __RPC_Rank_SSCharacterChangeDataList_ARG_RankChangeDataList_changes__();
            mMessage.CharacterId = __characterId__;
            Request.Changes=changes;

        }

        public __RPC_Rank_SSCharacterChangeDataList_ARG_RankChangeDataList_changes__ Request { get; private set; }


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

    public class UpdateServerOutMessage : OutMessage
    {
        public UpdateServerOutMessage(ClientAgentBase sender, ulong __characterId__, int placeholder)
            : base(sender, ServiceType.Rank, 6051, (int)MessageType.SAS)
        {
            Request = new __RPC_Rank_UpdateServer_ARG_int32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Rank_UpdateServer_ARG_int32_placeholder__ Request { get; private set; }


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

    public class GetRankValueOutMessage : OutMessage
    {
        public GetRankValueOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int rankType, int idx)
            : base(sender, ServiceType.Rank, 6052, (int)MessageType.SS)
        {
            Request = new __RPC_Rank_GetRankValue_ARG_int32_serverId_int32_rankType_int32_idx__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.RankType=rankType;
            Request.Idx=idx;

        }

        public __RPC_Rank_GetRankValue_ARG_int32_serverId_int32_rankType_int32_idx__ Request { get; private set; }

            private __RPC_Rank_GetRankValue_RET_int64__ mResponse;
            public long Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Rank_GetRankValue_RET_int64__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSGetServerRankDataOutMessage : OutMessage
    {
        public SSGetServerRankDataOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int ranktype)
            : base(sender, ServiceType.Rank, 6054, (int)MessageType.SS)
        {
            Request = new __RPC_Rank_SSGetServerRankData_ARG_int32_serverId_int32_ranktype__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.Ranktype=ranktype;

        }

        public __RPC_Rank_SSGetServerRankData_ARG_int32_serverId_int32_ranktype__ Request { get; private set; }

            private __RPC_Rank_SSGetServerRankData_RET_RankList__ mResponse;
            public RankList Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Rank_SSGetServerRankData_RET_RankList__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSGetRankDataByServerIdOutMessage : OutMessage
    {
        public SSGetRankDataByServerIdOutMessage(ClientAgentBase sender, ulong __characterId__, Int32Array serverList, long time, int ranktype)
            : base(sender, ServiceType.Rank, 6055, (int)MessageType.SS)
        {
            Request = new __RPC_Rank_SSGetRankDataByServerId_ARG_Int32Array_serverList_int64_time_int32_ranktype__();
            mMessage.CharacterId = __characterId__;
            Request.ServerList=serverList;
            Request.Time=time;
            Request.Ranktype=ranktype;

        }

        public __RPC_Rank_SSGetRankDataByServerId_ARG_Int32Array_serverList_int64_time_int32_ranktype__ Request { get; private set; }

            private __RPC_Rank_SSGetRankDataByServerId_RET_MsgRankData__ mResponse;
            public MsgRankData Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Rank_SSGetRankDataByServerId_RET_MsgRankData__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SBCleanClientCharacterDataOutMessage : OutMessage
    {
        public SBCleanClientCharacterDataOutMessage(ClientAgentBase sender, ulong __characterId__, ulong clientId, ulong characterId)
            : base(sender, ServiceType.Rank, 6500, (int)MessageType.SB)
        {
            Request = new __RPC_Rank_SBCleanClientCharacterData_ARG_uint64_clientId_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.ClientId=clientId;
            Request.CharacterId=characterId;

        }

        public __RPC_Rank_SBCleanClientCharacterData_ARG_uint64_clientId_uint64_characterId__ Request { get; private set; }


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
            : base(sender, ServiceType.Rank, 6501, (int)MessageType.SS)
        {
            Request = new __RPC_Rank_SSNotifyCharacterOnConnet_ARG_uint64_clientId_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.ClientId=clientId;
            Request.CharacterId=characterId;

        }

        public __RPC_Rank_SSNotifyCharacterOnConnet_ARG_uint64_clientId_uint64_characterId__ Request { get; private set; }

            private __RPC_Rank_SSNotifyCharacterOnConnet_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Rank_SSNotifyCharacterOnConnet_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GMCommandOutMessage : OutMessage
    {
        public GMCommandOutMessage(ClientAgentBase sender, ulong __characterId__, StringArray commonds)
            : base(sender, ServiceType.Rank, 6900, (int)MessageType.SS)
        {
            Request = new __RPC_Rank_GMCommand_ARG_StringArray_commonds__();
            mMessage.CharacterId = __characterId__;
            Request.Commonds=commonds;

        }

        public __RPC_Rank_GMCommand_ARG_StringArray_commonds__ Request { get; private set; }

            private __RPC_Rank_GMCommand_RET_Int32Array__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Rank_GMCommand_RET_Int32Array__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class NodifyModifyPlayerNameOutMessage : OutMessage
    {
        public NodifyModifyPlayerNameOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, ulong guid, string modifyName)
            : base(sender, ServiceType.Rank, 6901, (int)MessageType.SS)
        {
            Request = new __RPC_Rank_NodifyModifyPlayerName_ARG_int32_serverId_uint64_guid_string_modifyName__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.Guid=guid;
            Request.ModifyName=modifyName;

        }

        public __RPC_Rank_NodifyModifyPlayerName_ARG_int32_serverId_uint64_guid_string_modifyName__ Request { get; private set; }


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

    public class AddFunctionNameRank
    {
        public static void AddFunctionName(IDictionary<int, string> dict)
        {
            dict[6000] = "PrepareDataForEnterGame";
            dict[6001] = "PrepareDataForCreateCharacter";
            dict[6002] = "PrepareDataForCommonUse";
            dict[6003] = "PrepareDataForLogout";
            dict[6030] = "SBGetAllOnlineCharacterInServer";
            dict[6031] = "CheckConnected";
            dict[6032] = "CheckLost";
            dict[6033] = "QueryStatus";
            dict[6040] = "QueryBrokerStatus";
            dict[6041] = "CharacterChangeLevel";
            dict[6042] = "CharacterChangeData";
            dict[6043] = "GetRankList";
            dict[6044] = "Rank_GetP1vP1List";
            dict[6045] = "CompareRank";
            dict[6046] = "RankP1vP1FightOver";
            dict[6047] = "GMRank";
            dict[6048] = "ServerGMCommand";
            dict[6049] = "ReadyToEnter";
            dict[6050] = "SSCharacterChangeDataList";
            dict[6051] = "UpdateServer";
            dict[6052] = "GetRankValue";
            dict[6053] = "ApplyServerActivityData";
            dict[6054] = "SSGetServerRankData";
            dict[6055] = "SSGetRankDataByServerId";
            dict[6500] = "SBCleanClientCharacterData";
            dict[6501] = "SSNotifyCharacterOnConnet";
            dict[6503] = "BSNotifyCharacterOnLost";
            dict[6504] = "GetFightRankList";
            dict[6900] = "GMCommand";
            dict[6901] = "NodifyModifyPlayerName";
        }
        public static void AddCSFunctionName(IDictionary<int, string> dict)
        {
            dict[6043] = "GetRankList";
            dict[6047] = "GMRank";
            dict[6053] = "ApplyServerActivityData";
            dict[6504] = "GetFightRankList";
        }
    }
}
