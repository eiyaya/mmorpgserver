using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Scorpion;
using DataContract;
using ProtoBuf;

#pragma warning disable 0162,0108
namespace GameMasterClientService
{

    public abstract class GameMasterAgent : ClientAgentBase
    {
        public GameMasterAgent(string addr)
            : base(addr)
        {
        }

        public GameMasterAgent(ServerInfo broker, ServerInfo[] directConnect, Func<ulong, int> characterId2ServerId)
            : base(broker, directConnect, characterId2ServerId)
        {
        }

        /// <summary>
        /// 检查客户端是否已经连接到对应服务器
        /// </summary>
        public CheckConnectedOutMessage CheckConnected(ulong __characterId__, ulong characterId)
        {
            return new CheckConnectedOutMessage(this, __characterId__, characterId);
        }

        /// <summary>
        /// </summary>
        public LoginOutMessage Login(ulong __characterId__, string name, string password)
        {
            return new LoginOutMessage(this, __characterId__, name, password);
        }

        /// <summary>
        /// 通过角色名称查找角色信息
        /// </summary>
        public GetPlayerDataByCharacterNameOutMessage GetPlayerDataByCharacterName(ulong __characterId__, string name)
        {
            return new GetPlayerDataByCharacterNameOutMessage(this, __characterId__, name);
        }

        /// <summary>
        /// </summary>
        public GetPlayerDataByCharacterIdOutMessage GetPlayerDataByCharacterId(ulong __characterId__, ulong id)
        {
            return new GetPlayerDataByCharacterIdOutMessage(this, __characterId__, id);
        }

        /// <summary>
        /// </summary>
        public GetPlayerDataByPlayerNameOutMessage GetPlayerDataByPlayerName(ulong __characterId__, string name)
        {
            return new GetPlayerDataByPlayerNameOutMessage(this, __characterId__, name);
        }

        /// <summary>
        /// </summary>
        public GetPlayerDataByPlayerIdOutMessage GetPlayerDataByPlayerId(ulong __characterId__, ulong id)
        {
            return new GetPlayerDataByPlayerIdOutMessage(this, __characterId__, id);
        }

        /// <summary>
        /// </summary>
        public GetCharacterDataByIdOutMessage GetCharacterDataById(ulong __characterId__, ulong characterId)
        {
            return new GetCharacterDataByIdOutMessage(this, __characterId__, characterId);
        }

        /// <summary>
        /// </summary>
        public SendMailsByIdOutMessage SendMailsById(ulong __characterId__, Uint64Array ids, string title, string content, Dict_int_int_Data items, long time)
        {
            return new SendMailsByIdOutMessage(this, __characterId__, ids, title, content, items, time);
        }

        /// <summary>
        /// </summary>
        public SendMailsByNameOutMessage SendMailsByName(ulong __characterId__, StringArray names, string title, string content, Dict_int_int_Data items, long time)
        {
            return new SendMailsByNameOutMessage(this, __characterId__, names, title, content, items, time);
        }

        /// <summary>
        /// </summary>
        public SendMailsToServersOutMessage SendMailsToServers(ulong __characterId__, Uint32Array servers, string title, string content, Dict_int_int_Data items, long time)
        {
            return new SendMailsToServersOutMessage(this, __characterId__, servers, title, content, items, time);
        }

        /// <summary>
        /// </summary>
        public GetWaitingMailsOutMessage GetWaitingMails(ulong __characterId__, int placeholder)
        {
            return new GetWaitingMailsOutMessage(this, __characterId__, placeholder);
        }

        /// <summary>
        /// </summary>
        public DelWaitingMailsOutMessage DelWaitingMails(ulong __characterId__, Uint64Array ids)
        {
            return new DelWaitingMailsOutMessage(this, __characterId__, ids);
        }

        /// <summary>
        /// </summary>
        public GetWaitingBroadcastsOutMessage GetWaitingBroadcasts(ulong __characterId__, int placeholder)
        {
            return new GetWaitingBroadcastsOutMessage(this, __characterId__, placeholder);
        }

        /// <summary>
        /// </summary>
        public DelWaitingBroadcastsOutMessage DelWaitingBroadcasts(ulong __characterId__, Uint64Array ids)
        {
            return new DelWaitingBroadcastsOutMessage(this, __characterId__, ids);
        }

        /// <summary>
        /// </summary>
        public KickCharacterOutMessage KickCharacter(ulong __characterId__, ulong characterId, string name)
        {
            return new KickCharacterOutMessage(this, __characterId__, characterId, name);
        }

        /// <summary>
        /// </summary>
        public BroadcastOutMessage Broadcast(ulong __characterId__, string content, Uint32Array servers, long time)
        {
            return new BroadcastOutMessage(this, __characterId__, content, servers, time);
        }

        /// <summary>
        /// </summary>
        public CreateGmAccountOutMessage CreateGmAccount(ulong __characterId__, string name, string pwd, int priority)
        {
            return new CreateGmAccountOutMessage(this, __characterId__, name, pwd, priority);
        }

        /// <summary>
        /// </summary>
        public UpdateServerOutMessage UpdateServer(ulong __characterId__, int placeholder)
        {
            return new UpdateServerOutMessage(this, __characterId__, placeholder);
        }

        /// <summary>
        /// </summary>
        public UpdateServerAllOutMessage UpdateServerAll(ulong __characterId__, int placeholder)
        {
            return new UpdateServerAllOutMessage(this, __characterId__, placeholder);
        }

        /// <summary>
        /// </summary>
        public ReloadTableOutMessage ReloadTable(ulong __characterId__, string tableName)
        {
            return new ReloadTableOutMessage(this, __characterId__, tableName);
        }

        /// <summary>
        /// 修改角色的Flag
        /// </summary>
        public SetFlagOutMessage SetFlag(ulong __characterId__, ulong characterId, Dict_int_int_Data changes)
        {
            return new SetFlagOutMessage(this, __characterId__, characterId, changes);
        }

        /// <summary>
        /// 修改角色的Exdata
        /// </summary>
        public SetExdataOutMessage SetExdata(ulong __characterId__, ulong characterId, Dict_int_int_Data changes)
        {
            return new SetExdataOutMessage(this, __characterId__, characterId, changes);
        }

        /// <summary>
        /// 获得某角色某天的日志，date format "yyyy-MM-dd"
        /// </summary>
        public GetLogOutMessage GetLog(ulong __characterId__, ulong characterId, string date)
        {
            return new GetLogOutMessage(this, __characterId__, characterId, date);
        }

        /// <summary>
        /// 角色转服
        /// </summary>
        public ChangeServerOutMessage ChangeServer(ulong __characterId__, ulong characterId, int serverId)
        {
            return new ChangeServerOutMessage(this, __characterId__, characterId, serverId);
        }

        /// <summary>
        /// gm命令
        /// </summary>
        public GMCommandOutMessage GMCommand(ulong __characterId__, ulong id, string command)
        {
            return new GMCommandOutMessage(this, __characterId__, id, command);
        }

        /// <summary>
        /// </summary>
        public CharacterConnectedOutMessage CharacterConnected(ulong __characterId__, ulong id, int serverType)
        {
            return new CharacterConnectedOutMessage(this, __characterId__, id, serverType);
        }

        /// <summary>
        /// gm server接受命令的接口
        /// </summary>
        public GenGiftCodeOutMessage GenGiftCode(ulong __characterId__, int type, int count, int channelId)
        {
            return new GenGiftCodeOutMessage(this, __characterId__, type, count, channelId);
        }

        /// <summary>
        /// </summary>
        public GetServerCharacterCountOutMessage GetServerCharacterCount(ulong __characterId__, ulong id, int serverType)
        {
            return new GetServerCharacterCountOutMessage(this, __characterId__, id, serverType);
        }

        /// <summary>
        /// </summary>
        public GetCharacterLogicDbInfoOutMessage GetCharacterLogicDbInfo(ulong __characterId__, ulong characterId)
        {
            return new GetCharacterLogicDbInfoOutMessage(this, __characterId__, characterId);
        }

        /// <summary>
        /// 动态激活活动
        /// </summary>
        public AddAutoActvityOutMessage AddAutoActvity(ulong __characterId__, int fubenId, long startTime, long endTime, int count)
        {
            return new AddAutoActvityOutMessage(this, __characterId__, fubenId, startTime, endTime, count);
        }

        /// <summary>
        /// 获取排行榜数据
        /// </summary>
        public GetServerRankDataOutMessage GetServerRankData(ulong __characterId__, int serverId, int ranktype)
        {
            return new GetServerRankDataOutMessage(this, __characterId__, serverId, ranktype);
        }

        /// <summary>
        /// 获取玩家副本次数
        /// </summary>
        public GetTodayFunbenCountOutMessage GetTodayFunbenCount(ulong __characterId__, int serverId, ulong characterId, int selecttype)
        {
            return new GetTodayFunbenCountOutMessage(this, __characterId__, serverId, characterId, selecttype);
        }

        /// <summary>
        /// 玩家禁言
        /// </summary>
        public SilenceOutMessage Silence(ulong __characterId__, ulong characterId, uint mask)
        {
            return new SilenceOutMessage(this, __characterId__, characterId, mask);
        }

        /// <summary>
        /// 封账号
        /// </summary>
        public LockAccountOutMessage LockAccount(ulong __characterId__, ulong playerId, long endTime)
        {
            return new LockAccountOutMessage(this, __characterId__, playerId, endTime);
        }

        /// <summary>
        /// 查找账号下的角色
        /// </summary>
        public GetServerCharacterOutMessage GetServerCharacter(ulong __characterId__, string accoutName)
        {
            return new GetServerCharacterOutMessage(this, __characterId__, accoutName);
        }

        /// <summary>
        /// </summary>
        public SendQuestionOutMessage SendQuestion(ulong __characterId__, MailQuestion mail)
        {
            return new SendQuestionOutMessage(this, __characterId__, mail);
        }

        /// <summary>
        /// </summary>
        public TakeOldPlayerRewardOutMessage TakeOldPlayerReward(ulong __characterId__, ulong clientId)
        {
            return new TakeOldPlayerRewardOutMessage(this, __characterId__, clientId);
        }

        /// <summary>
        /// </summary>
        public UseGiftCodeOutMessage UseGiftCode(ulong __characterId__, string code, int channelId)
        {
            return new UseGiftCodeOutMessage(this, __characterId__, code, channelId);
        }

        protected override object GetPublishData(uint p, byte[] list)
        {
            switch (p)
            {
                case 9000:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_CheckConnected_ARG_uint64_characterId__>(ms);
                    }
                    break;
                case 9001:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_Login_ARG_string_name_string_password__>(ms);
                    }
                    break;
                case 9002:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_GetPlayerDataByCharacterName_ARG_string_name__>(ms);
                    }
                    break;
                case 9003:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_GetPlayerDataByCharacterId_ARG_uint64_id__>(ms);
                    }
                    break;
                case 9004:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_GetPlayerDataByPlayerName_ARG_string_name__>(ms);
                    }
                    break;
                case 9005:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_GetPlayerDataByPlayerId_ARG_uint64_id__>(ms);
                    }
                    break;
                case 9006:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_GetCharacterDataById_ARG_uint64_characterId__>(ms);
                    }
                    break;
                case 9008:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_SendMailsById_ARG_Uint64Array_ids_string_title_string_content_Dict_int_int_Data_items_int64_time__>(ms);
                    }
                    break;
                case 9009:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_SendMailsByName_ARG_StringArray_names_string_title_string_content_Dict_int_int_Data_items_int64_time__>(ms);
                    }
                    break;
                case 9010:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_SendMailsToServers_ARG_Uint32Array_servers_string_title_string_content_Dict_int_int_Data_items_int64_time__>(ms);
                    }
                    break;
                case 9011:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_GetWaitingMails_ARG_int32_placeholder__>(ms);
                    }
                    break;
                case 9012:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_DelWaitingMails_ARG_Uint64Array_ids__>(ms);
                    }
                    break;
                case 9013:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_GetWaitingBroadcasts_ARG_int32_placeholder__>(ms);
                    }
                    break;
                case 9014:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_DelWaitingBroadcasts_ARG_Uint64Array_ids__>(ms);
                    }
                    break;
                case 9015:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_KickCharacter_ARG_uint64_characterId_string_name__>(ms);
                    }
                    break;
                case 9016:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_Broadcast_ARG_string_content_Uint32Array_servers_int64_time__>(ms);
                    }
                    break;
                case 9017:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_CreateGmAccount_ARG_string_name_string_pwd_int32_priority__>(ms);
                    }
                    break;
                case 9018:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_UpdateServer_ARG_int32_placeholder__>(ms);
                    }
                    break;
                case 9019:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_UpdateServerAll_ARG_int32_placeholder__>(ms);
                    }
                    break;
                case 9020:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_ReloadTable_ARG_string_tableName__>(ms);
                    }
                    break;
                case 9021:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_SetFlag_ARG_uint64_characterId_Dict_int_int_Data_changes__>(ms);
                    }
                    break;
                case 9022:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_SetExdata_ARG_uint64_characterId_Dict_int_int_Data_changes__>(ms);
                    }
                    break;
                case 9023:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_GetLog_ARG_uint64_characterId_string_date__>(ms);
                    }
                    break;
                case 9024:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_ChangeServer_ARG_uint64_characterId_int32_serverId__>(ms);
                    }
                    break;
                case 9026:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_GMCommand_ARG_uint64_id_string_command__>(ms);
                    }
                    break;
                case 9027:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_CharacterConnected_ARG_uint64_id_int32_serverType__>(ms);
                    }
                    break;
                case 9028:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_GenGiftCode_ARG_int32_type_int32_count_int32_channelId__>(ms);
                    }
                    break;
                case 9029:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_GetServerCharacterCount_ARG_uint64_id_int32_serverType__>(ms);
                    }
                    break;
                case 9030:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_GetCharacterLogicDbInfo_ARG_uint64_characterId__>(ms);
                    }
                    break;
                case 9031:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_AddAutoActvity_ARG_int32_fubenId_int64_startTime_int64_endTime_int32_count__>(ms);
                    }
                    break;
                case 9032:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_GetServerRankData_ARG_int32_serverId_int32_ranktype__>(ms);
                    }
                    break;
                case 9033:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_GetTodayFunbenCount_ARG_int32_serverId_uint64_characterId_int32_selecttype__>(ms);
                    }
                    break;
                case 9034:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_Silence_ARG_uint64_characterId_uint32_mask__>(ms);
                    }
                    break;
                case 9035:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_LockAccount_ARG_uint64_playerId_int64_endTime__>(ms);
                    }
                    break;
                case 9036:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_GetServerCharacter_ARG_string_accoutName__>(ms);
                    }
                    break;
                case 9037:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_SendQuestion_ARG_MailQuestion_mail__>(ms);
                    }
                    break;
                case 9038:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_TakeOldPlayerReward_ARG_uint64_clientId__>(ms);
                    }
                    break;
                case 9039:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_GameMaster_UseGiftCode_ARG_string_code_int32_channelId__>(ms);
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

    public class CheckConnectedOutMessage : OutMessage
    {
        public CheckConnectedOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId)
            : base(sender, ServiceType.GameMaster, 9000, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_CheckConnected_ARG_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;

        }

        public __RPC_GameMaster_CheckConnected_ARG_uint64_characterId__ Request { get; private set; }

            private __RPC_GameMaster_CheckConnected_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_CheckConnected_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class LoginOutMessage : OutMessage
    {
        public LoginOutMessage(ClientAgentBase sender, ulong __characterId__, string name, string password)
            : base(sender, ServiceType.GameMaster, 9001, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_Login_ARG_string_name_string_password__();
            mMessage.CharacterId = __characterId__;
            Request.Name=name;
            Request.Password=password;

        }

        public __RPC_GameMaster_Login_ARG_string_name_string_password__ Request { get; private set; }

            private __RPC_GameMaster_Login_RET_GMAccount__ mResponse;
            public GMAccount Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_Login_RET_GMAccount__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetPlayerDataByCharacterNameOutMessage : OutMessage
    {
        public GetPlayerDataByCharacterNameOutMessage(ClientAgentBase sender, ulong __characterId__, string name)
            : base(sender, ServiceType.GameMaster, 9002, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_GetPlayerDataByCharacterName_ARG_string_name__();
            mMessage.CharacterId = __characterId__;
            Request.Name=name;

        }

        public __RPC_GameMaster_GetPlayerDataByCharacterName_ARG_string_name__ Request { get; private set; }

            private __RPC_GameMaster_GetPlayerDataByCharacterName_RET_GMPlayerInfoMsg__ mResponse;
            public GMPlayerInfoMsg Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_GetPlayerDataByCharacterName_RET_GMPlayerInfoMsg__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetPlayerDataByCharacterIdOutMessage : OutMessage
    {
        public GetPlayerDataByCharacterIdOutMessage(ClientAgentBase sender, ulong __characterId__, ulong id)
            : base(sender, ServiceType.GameMaster, 9003, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_GetPlayerDataByCharacterId_ARG_uint64_id__();
            mMessage.CharacterId = __characterId__;
            Request.Id=id;

        }

        public __RPC_GameMaster_GetPlayerDataByCharacterId_ARG_uint64_id__ Request { get; private set; }

            private __RPC_GameMaster_GetPlayerDataByCharacterId_RET_GMPlayerInfoMsg__ mResponse;
            public GMPlayerInfoMsg Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_GetPlayerDataByCharacterId_RET_GMPlayerInfoMsg__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetPlayerDataByPlayerNameOutMessage : OutMessage
    {
        public GetPlayerDataByPlayerNameOutMessage(ClientAgentBase sender, ulong __characterId__, string name)
            : base(sender, ServiceType.GameMaster, 9004, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_GetPlayerDataByPlayerName_ARG_string_name__();
            mMessage.CharacterId = __characterId__;
            Request.Name=name;

        }

        public __RPC_GameMaster_GetPlayerDataByPlayerName_ARG_string_name__ Request { get; private set; }

            private __RPC_GameMaster_GetPlayerDataByPlayerName_RET_GMPlayerInfoMsg__ mResponse;
            public GMPlayerInfoMsg Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_GetPlayerDataByPlayerName_RET_GMPlayerInfoMsg__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetPlayerDataByPlayerIdOutMessage : OutMessage
    {
        public GetPlayerDataByPlayerIdOutMessage(ClientAgentBase sender, ulong __characterId__, ulong id)
            : base(sender, ServiceType.GameMaster, 9005, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_GetPlayerDataByPlayerId_ARG_uint64_id__();
            mMessage.CharacterId = __characterId__;
            Request.Id=id;

        }

        public __RPC_GameMaster_GetPlayerDataByPlayerId_ARG_uint64_id__ Request { get; private set; }

            private __RPC_GameMaster_GetPlayerDataByPlayerId_RET_GMPlayerInfoMsg__ mResponse;
            public GMPlayerInfoMsg Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_GetPlayerDataByPlayerId_RET_GMPlayerInfoMsg__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetCharacterDataByIdOutMessage : OutMessage
    {
        public GetCharacterDataByIdOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId)
            : base(sender, ServiceType.GameMaster, 9006, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_GetCharacterDataById_ARG_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;

        }

        public __RPC_GameMaster_GetCharacterDataById_ARG_uint64_characterId__ Request { get; private set; }

            private __RPC_GameMaster_GetCharacterDataById_RET_GMCharacterDetailInfo__ mResponse;
            public GMCharacterDetailInfo Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_GetCharacterDataById_RET_GMCharacterDetailInfo__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SendMailsByIdOutMessage : OutMessage
    {
        public SendMailsByIdOutMessage(ClientAgentBase sender, ulong __characterId__, Uint64Array ids, string title, string content, Dict_int_int_Data items, long time)
            : base(sender, ServiceType.GameMaster, 9008, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_SendMailsById_ARG_Uint64Array_ids_string_title_string_content_Dict_int_int_Data_items_int64_time__();
            mMessage.CharacterId = __characterId__;
            Request.Ids=ids;
            Request.Title=title;
            Request.Content=content;
            Request.Items=items;
            Request.Time=time;

        }

        public __RPC_GameMaster_SendMailsById_ARG_Uint64Array_ids_string_title_string_content_Dict_int_int_Data_items_int64_time__ Request { get; private set; }

            private __RPC_GameMaster_SendMailsById_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_SendMailsById_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SendMailsByNameOutMessage : OutMessage
    {
        public SendMailsByNameOutMessage(ClientAgentBase sender, ulong __characterId__, StringArray names, string title, string content, Dict_int_int_Data items, long time)
            : base(sender, ServiceType.GameMaster, 9009, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_SendMailsByName_ARG_StringArray_names_string_title_string_content_Dict_int_int_Data_items_int64_time__();
            mMessage.CharacterId = __characterId__;
            Request.Names=names;
            Request.Title=title;
            Request.Content=content;
            Request.Items=items;
            Request.Time=time;

        }

        public __RPC_GameMaster_SendMailsByName_ARG_StringArray_names_string_title_string_content_Dict_int_int_Data_items_int64_time__ Request { get; private set; }

            private __RPC_GameMaster_SendMailsByName_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_SendMailsByName_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SendMailsToServersOutMessage : OutMessage
    {
        public SendMailsToServersOutMessage(ClientAgentBase sender, ulong __characterId__, Uint32Array servers, string title, string content, Dict_int_int_Data items, long time)
            : base(sender, ServiceType.GameMaster, 9010, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_SendMailsToServers_ARG_Uint32Array_servers_string_title_string_content_Dict_int_int_Data_items_int64_time__();
            mMessage.CharacterId = __characterId__;
            Request.Servers=servers;
            Request.Title=title;
            Request.Content=content;
            Request.Items=items;
            Request.Time=time;

        }

        public __RPC_GameMaster_SendMailsToServers_ARG_Uint32Array_servers_string_title_string_content_Dict_int_int_Data_items_int64_time__ Request { get; private set; }

            private __RPC_GameMaster_SendMailsToServers_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_SendMailsToServers_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetWaitingMailsOutMessage : OutMessage
    {
        public GetWaitingMailsOutMessage(ClientAgentBase sender, ulong __characterId__, int placeholder)
            : base(sender, ServiceType.GameMaster, 9011, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_GetWaitingMails_ARG_int32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_GameMaster_GetWaitingMails_ARG_int32_placeholder__ Request { get; private set; }

            private __RPC_GameMaster_GetWaitingMails_RET_GmMailList__ mResponse;
            public GmMailList Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_GetWaitingMails_RET_GmMailList__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class DelWaitingMailsOutMessage : OutMessage
    {
        public DelWaitingMailsOutMessage(ClientAgentBase sender, ulong __characterId__, Uint64Array ids)
            : base(sender, ServiceType.GameMaster, 9012, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_DelWaitingMails_ARG_Uint64Array_ids__();
            mMessage.CharacterId = __characterId__;
            Request.Ids=ids;

        }

        public __RPC_GameMaster_DelWaitingMails_ARG_Uint64Array_ids__ Request { get; private set; }

            private __RPC_GameMaster_DelWaitingMails_RET_GmMailList__ mResponse;
            public GmMailList Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_DelWaitingMails_RET_GmMailList__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetWaitingBroadcastsOutMessage : OutMessage
    {
        public GetWaitingBroadcastsOutMessage(ClientAgentBase sender, ulong __characterId__, int placeholder)
            : base(sender, ServiceType.GameMaster, 9013, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_GetWaitingBroadcasts_ARG_int32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_GameMaster_GetWaitingBroadcasts_ARG_int32_placeholder__ Request { get; private set; }

            private __RPC_GameMaster_GetWaitingBroadcasts_RET_GmBroadcastList__ mResponse;
            public GmBroadcastList Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_GetWaitingBroadcasts_RET_GmBroadcastList__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class DelWaitingBroadcastsOutMessage : OutMessage
    {
        public DelWaitingBroadcastsOutMessage(ClientAgentBase sender, ulong __characterId__, Uint64Array ids)
            : base(sender, ServiceType.GameMaster, 9014, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_DelWaitingBroadcasts_ARG_Uint64Array_ids__();
            mMessage.CharacterId = __characterId__;
            Request.Ids=ids;

        }

        public __RPC_GameMaster_DelWaitingBroadcasts_ARG_Uint64Array_ids__ Request { get; private set; }

            private __RPC_GameMaster_DelWaitingBroadcasts_RET_GmBroadcastList__ mResponse;
            public GmBroadcastList Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_DelWaitingBroadcasts_RET_GmBroadcastList__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class KickCharacterOutMessage : OutMessage
    {
        public KickCharacterOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId, string name)
            : base(sender, ServiceType.GameMaster, 9015, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_KickCharacter_ARG_uint64_characterId_string_name__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;
            Request.Name=name;

        }

        public __RPC_GameMaster_KickCharacter_ARG_uint64_characterId_string_name__ Request { get; private set; }

            private __RPC_GameMaster_KickCharacter_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_KickCharacter_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class BroadcastOutMessage : OutMessage
    {
        public BroadcastOutMessage(ClientAgentBase sender, ulong __characterId__, string content, Uint32Array servers, long time)
            : base(sender, ServiceType.GameMaster, 9016, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_Broadcast_ARG_string_content_Uint32Array_servers_int64_time__();
            mMessage.CharacterId = __characterId__;
            Request.Content=content;
            Request.Servers=servers;
            Request.Time=time;

        }

        public __RPC_GameMaster_Broadcast_ARG_string_content_Uint32Array_servers_int64_time__ Request { get; private set; }

            private __RPC_GameMaster_Broadcast_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_Broadcast_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class CreateGmAccountOutMessage : OutMessage
    {
        public CreateGmAccountOutMessage(ClientAgentBase sender, ulong __characterId__, string name, string pwd, int priority)
            : base(sender, ServiceType.GameMaster, 9017, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_CreateGmAccount_ARG_string_name_string_pwd_int32_priority__();
            mMessage.CharacterId = __characterId__;
            Request.Name=name;
            Request.Pwd=pwd;
            Request.Priority=priority;

        }

        public __RPC_GameMaster_CreateGmAccount_ARG_string_name_string_pwd_int32_priority__ Request { get; private set; }

            private __RPC_GameMaster_CreateGmAccount_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_CreateGmAccount_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class UpdateServerOutMessage : OutMessage
    {
        public UpdateServerOutMessage(ClientAgentBase sender, ulong __characterId__, int placeholder)
            : base(sender, ServiceType.GameMaster, 9018, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_UpdateServer_ARG_int32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_GameMaster_UpdateServer_ARG_int32_placeholder__ Request { get; private set; }


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

    public class UpdateServerAllOutMessage : OutMessage
    {
        public UpdateServerAllOutMessage(ClientAgentBase sender, ulong __characterId__, int placeholder)
            : base(sender, ServiceType.GameMaster, 9019, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_UpdateServerAll_ARG_int32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_GameMaster_UpdateServerAll_ARG_int32_placeholder__ Request { get; private set; }

            private __RPC_GameMaster_UpdateServerAll_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_UpdateServerAll_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class ReloadTableOutMessage : OutMessage
    {
        public ReloadTableOutMessage(ClientAgentBase sender, ulong __characterId__, string tableName)
            : base(sender, ServiceType.GameMaster, 9020, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_ReloadTable_ARG_string_tableName__();
            mMessage.CharacterId = __characterId__;
            Request.TableName=tableName;

        }

        public __RPC_GameMaster_ReloadTable_ARG_string_tableName__ Request { get; private set; }

            private __RPC_GameMaster_ReloadTable_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_ReloadTable_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SetFlagOutMessage : OutMessage
    {
        public SetFlagOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId, Dict_int_int_Data changes)
            : base(sender, ServiceType.GameMaster, 9021, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_SetFlag_ARG_uint64_characterId_Dict_int_int_Data_changes__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;
            Request.Changes=changes;

        }

        public __RPC_GameMaster_SetFlag_ARG_uint64_characterId_Dict_int_int_Data_changes__ Request { get; private set; }

            private __RPC_GameMaster_SetFlag_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_SetFlag_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SetExdataOutMessage : OutMessage
    {
        public SetExdataOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId, Dict_int_int_Data changes)
            : base(sender, ServiceType.GameMaster, 9022, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_SetExdata_ARG_uint64_characterId_Dict_int_int_Data_changes__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;
            Request.Changes=changes;

        }

        public __RPC_GameMaster_SetExdata_ARG_uint64_characterId_Dict_int_int_Data_changes__ Request { get; private set; }

            private __RPC_GameMaster_SetExdata_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_SetExdata_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetLogOutMessage : OutMessage
    {
        public GetLogOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId, string date)
            : base(sender, ServiceType.GameMaster, 9023, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_GetLog_ARG_uint64_characterId_string_date__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;
            Request.Date=date;

        }

        public __RPC_GameMaster_GetLog_ARG_uint64_characterId_string_date__ Request { get; private set; }

            private __RPC_GameMaster_GetLog_RET_string__ mResponse;
            public string Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_GetLog_RET_string__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class ChangeServerOutMessage : OutMessage
    {
        public ChangeServerOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId, int serverId)
            : base(sender, ServiceType.GameMaster, 9024, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_ChangeServer_ARG_uint64_characterId_int32_serverId__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;
            Request.ServerId=serverId;

        }

        public __RPC_GameMaster_ChangeServer_ARG_uint64_characterId_int32_serverId__ Request { get; private set; }

            private __RPC_GameMaster_ChangeServer_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_ChangeServer_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GMCommandOutMessage : OutMessage
    {
        public GMCommandOutMessage(ClientAgentBase sender, ulong __characterId__, ulong id, string command)
            : base(sender, ServiceType.GameMaster, 9026, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_GMCommand_ARG_uint64_id_string_command__();
            mMessage.CharacterId = __characterId__;
            Request.Id=id;
            Request.Command=command;

        }

        public __RPC_GameMaster_GMCommand_ARG_uint64_id_string_command__ Request { get; private set; }

            private __RPC_GameMaster_GMCommand_RET_string__ mResponse;
            public string Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_GMCommand_RET_string__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class CharacterConnectedOutMessage : OutMessage
    {
        public CharacterConnectedOutMessage(ClientAgentBase sender, ulong __characterId__, ulong id, int serverType)
            : base(sender, ServiceType.GameMaster, 9027, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_CharacterConnected_ARG_uint64_id_int32_serverType__();
            mMessage.CharacterId = __characterId__;
            Request.Id=id;
            Request.ServerType=serverType;

        }

        public __RPC_GameMaster_CharacterConnected_ARG_uint64_id_int32_serverType__ Request { get; private set; }


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

    public class GenGiftCodeOutMessage : OutMessage
    {
        public GenGiftCodeOutMessage(ClientAgentBase sender, ulong __characterId__, int type, int count, int channelId)
            : base(sender, ServiceType.GameMaster, 9028, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_GenGiftCode_ARG_int32_type_int32_count_int32_channelId__();
            mMessage.CharacterId = __characterId__;
            Request.Type=type;
            Request.Count=count;
            Request.ChannelId=channelId;

        }

        public __RPC_GameMaster_GenGiftCode_ARG_int32_type_int32_count_int32_channelId__ Request { get; private set; }

            private __RPC_GameMaster_GenGiftCode_RET_string__ mResponse;
            public string Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_GenGiftCode_RET_string__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetServerCharacterCountOutMessage : OutMessage
    {
        public GetServerCharacterCountOutMessage(ClientAgentBase sender, ulong __characterId__, ulong id, int serverType)
            : base(sender, ServiceType.GameMaster, 9029, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_GetServerCharacterCount_ARG_uint64_id_int32_serverType__();
            mMessage.CharacterId = __characterId__;
            Request.Id=id;
            Request.ServerType=serverType;

        }

        public __RPC_GameMaster_GetServerCharacterCount_ARG_uint64_id_int32_serverType__ Request { get; private set; }

            private __RPC_GameMaster_GetServerCharacterCount_RET_Dict_int_int_Data__ mResponse;
            public Dict_int_int_Data Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_GetServerCharacterCount_RET_Dict_int_int_Data__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetCharacterLogicDbInfoOutMessage : OutMessage
    {
        public GetCharacterLogicDbInfoOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId)
            : base(sender, ServiceType.GameMaster, 9030, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_GetCharacterLogicDbInfo_ARG_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;

        }

        public __RPC_GameMaster_GetCharacterLogicDbInfo_ARG_uint64_characterId__ Request { get; private set; }

            private __RPC_GameMaster_GetCharacterLogicDbInfo_RET_GMCharacterLogicDbInfo__ mResponse;
            public GMCharacterLogicDbInfo Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_GetCharacterLogicDbInfo_RET_GMCharacterLogicDbInfo__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class AddAutoActvityOutMessage : OutMessage
    {
        public AddAutoActvityOutMessage(ClientAgentBase sender, ulong __characterId__, int fubenId, long startTime, long endTime, int count)
            : base(sender, ServiceType.GameMaster, 9031, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_AddAutoActvity_ARG_int32_fubenId_int64_startTime_int64_endTime_int32_count__();
            mMessage.CharacterId = __characterId__;
            Request.FubenId=fubenId;
            Request.StartTime=startTime;
            Request.EndTime=endTime;
            Request.Count=count;

        }

        public __RPC_GameMaster_AddAutoActvity_ARG_int32_fubenId_int64_startTime_int64_endTime_int32_count__ Request { get; private set; }

            private __RPC_GameMaster_AddAutoActvity_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_AddAutoActvity_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetServerRankDataOutMessage : OutMessage
    {
        public GetServerRankDataOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int ranktype)
            : base(sender, ServiceType.GameMaster, 9032, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_GetServerRankData_ARG_int32_serverId_int32_ranktype__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.Ranktype=ranktype;

        }

        public __RPC_GameMaster_GetServerRankData_ARG_int32_serverId_int32_ranktype__ Request { get; private set; }

            private __RPC_GameMaster_GetServerRankData_RET_RankList__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_GetServerRankData_RET_RankList__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetTodayFunbenCountOutMessage : OutMessage
    {
        public GetTodayFunbenCountOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, ulong characterId, int selecttype)
            : base(sender, ServiceType.GameMaster, 9033, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_GetTodayFunbenCount_ARG_int32_serverId_uint64_characterId_int32_selecttype__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.CharacterId=characterId;
            Request.Selecttype=selecttype;

        }

        public __RPC_GameMaster_GetTodayFunbenCount_ARG_int32_serverId_uint64_characterId_int32_selecttype__ Request { get; private set; }

            private __RPC_GameMaster_GetTodayFunbenCount_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_GetTodayFunbenCount_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SilenceOutMessage : OutMessage
    {
        public SilenceOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId, uint mask)
            : base(sender, ServiceType.GameMaster, 9034, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_Silence_ARG_uint64_characterId_uint32_mask__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;
            Request.Mask=mask;

        }

        public __RPC_GameMaster_Silence_ARG_uint64_characterId_uint32_mask__ Request { get; private set; }

            private __RPC_GameMaster_Silence_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_Silence_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class LockAccountOutMessage : OutMessage
    {
        public LockAccountOutMessage(ClientAgentBase sender, ulong __characterId__, ulong playerId, long endTime)
            : base(sender, ServiceType.GameMaster, 9035, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_LockAccount_ARG_uint64_playerId_int64_endTime__();
            mMessage.CharacterId = __characterId__;
            Request.PlayerId=playerId;
            Request.EndTime=endTime;

        }

        public __RPC_GameMaster_LockAccount_ARG_uint64_playerId_int64_endTime__ Request { get; private set; }

            private __RPC_GameMaster_LockAccount_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_LockAccount_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetServerCharacterOutMessage : OutMessage
    {
        public GetServerCharacterOutMessage(ClientAgentBase sender, ulong __characterId__, string accoutName)
            : base(sender, ServiceType.GameMaster, 9036, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_GetServerCharacter_ARG_string_accoutName__();
            mMessage.CharacterId = __characterId__;
            Request.AccoutName=accoutName;

        }

        public __RPC_GameMaster_GetServerCharacter_ARG_string_accoutName__ Request { get; private set; }

            private __RPC_GameMaster_GetServerCharacter_RET_DictIntUint64Array__ mResponse;
            public DictIntUint64Array Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_GetServerCharacter_RET_DictIntUint64Array__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SendQuestionOutMessage : OutMessage
    {
        public SendQuestionOutMessage(ClientAgentBase sender, ulong __characterId__, MailQuestion mail)
            : base(sender, ServiceType.GameMaster, 9037, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_SendQuestion_ARG_MailQuestion_mail__();
            mMessage.CharacterId = __characterId__;
            Request.Mail=mail;

        }

        public __RPC_GameMaster_SendQuestion_ARG_MailQuestion_mail__ Request { get; private set; }


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

    public class TakeOldPlayerRewardOutMessage : OutMessage
    {
        public TakeOldPlayerRewardOutMessage(ClientAgentBase sender, ulong __characterId__, ulong clientId)
            : base(sender, ServiceType.GameMaster, 9038, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_TakeOldPlayerReward_ARG_uint64_clientId__();
            mMessage.CharacterId = __characterId__;
            Request.ClientId=clientId;

        }

        public __RPC_GameMaster_TakeOldPlayerReward_ARG_uint64_clientId__ Request { get; private set; }


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

    public class UseGiftCodeOutMessage : OutMessage
    {
        public UseGiftCodeOutMessage(ClientAgentBase sender, ulong __characterId__, string code, int channelId)
            : base(sender, ServiceType.GameMaster, 9039, (int)MessageType.SS)
        {
            Request = new __RPC_GameMaster_UseGiftCode_ARG_string_code_int32_channelId__();
            mMessage.CharacterId = __characterId__;
            Request.Code=code;
            Request.ChannelId=channelId;

        }

        public __RPC_GameMaster_UseGiftCode_ARG_string_code_int32_channelId__ Request { get; private set; }

            private __RPC_GameMaster_UseGiftCode_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_GameMaster_UseGiftCode_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class AddFunctionNameGameMaster
    {
        public static void AddFunctionName(IDictionary<int, string> dict)
        {
            dict[9000] = "CheckConnected";
            dict[9001] = "Login";
            dict[9002] = "GetPlayerDataByCharacterName";
            dict[9003] = "GetPlayerDataByCharacterId";
            dict[9004] = "GetPlayerDataByPlayerName";
            dict[9005] = "GetPlayerDataByPlayerId";
            dict[9006] = "GetCharacterDataById";
            dict[9008] = "SendMailsById";
            dict[9009] = "SendMailsByName";
            dict[9010] = "SendMailsToServers";
            dict[9011] = "GetWaitingMails";
            dict[9012] = "DelWaitingMails";
            dict[9013] = "GetWaitingBroadcasts";
            dict[9014] = "DelWaitingBroadcasts";
            dict[9015] = "KickCharacter";
            dict[9016] = "Broadcast";
            dict[9017] = "CreateGmAccount";
            dict[9018] = "UpdateServer";
            dict[9019] = "UpdateServerAll";
            dict[9020] = "ReloadTable";
            dict[9021] = "SetFlag";
            dict[9022] = "SetExdata";
            dict[9023] = "GetLog";
            dict[9024] = "ChangeServer";
            dict[9026] = "GMCommand";
            dict[9027] = "CharacterConnected";
            dict[9028] = "GenGiftCode";
            dict[9029] = "GetServerCharacterCount";
            dict[9030] = "GetCharacterLogicDbInfo";
            dict[9031] = "AddAutoActvity";
            dict[9032] = "GetServerRankData";
            dict[9033] = "GetTodayFunbenCount";
            dict[9034] = "Silence";
            dict[9035] = "LockAccount";
            dict[9036] = "GetServerCharacter";
            dict[9037] = "SendQuestion";
            dict[9038] = "TakeOldPlayerReward";
            dict[9039] = "UseGiftCode";
        }
        public static void AddCSFunctionName(IDictionary<int, string> dict)
        {
        }
    }
}
