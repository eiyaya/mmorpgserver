using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Scorpion;
using DataContract;
using ProtoBuf;

#pragma warning disable 0162,0108
namespace LoginClientService
{

    public abstract class LoginAgent : ClientAgentBase
    {
        public LoginAgent(string addr)
            : base(addr)
        {
        }

        public LoginAgent(ServerInfo broker, ServerInfo[] directConnect, Func<ulong, int> characterId2ServerId)
            : base(broker, directConnect, characterId2ServerId)
        {
        }

        /// <summary>
        /// 通知Login连接成功
        /// </summary>
        public NotifyConnectedOutMessage NotifyConnected(ulong __characterId__, ulong characterId, int servictType, int err)
        {
            return new NotifyConnectedOutMessage(this, __characterId__, characterId, servictType, err);
        }

        /// <summary>
        /// 查询服务器状态
        /// </summary>
        public QueryStatusOutMessage QueryStatus(ulong __characterId__, uint placeholder)
        {
            return new QueryStatusOutMessage(this, __characterId__, placeholder);
        }

        /// <summary>
        /// 获得Login的Simple数据
        /// </summary>
        public GetLoginSimpleDataOutMessage GetLoginSimpleData(ulong __characterId__, ulong characterId)
        {
            return new GetLoginSimpleDataOutMessage(this, __characterId__, characterId);
        }

        /// <summary>
        /// 获得某个角色今天的在线时间
        /// </summary>
        public GetTodayOnlineSecondsOutMessage GetTodayOnlineSeconds(ulong __characterId__, ulong characterId)
        {
            return new GetTodayOnlineSecondsOutMessage(this, __characterId__, characterId);
        }

        /// <summary>
        /// 判断是否有此玩家
        /// </summary>
        public CheckIsHaveCharacterOutMessage CheckIsHaveCharacter(ulong __characterId__, ulong characterId)
        {
            return new CheckIsHaveCharacterOutMessage(this, __characterId__, characterId);
        }

        /// <summary>
        /// 获取某个名字玩家的ID
        /// </summary>
        public GetCharacterIdByNameOutMessage GetCharacterIdByName(ulong __characterId__, string name)
        {
            return new GetCharacterIdByNameOutMessage(this, __characterId__, name);
        }

        /// <summary>
        /// 强制下线
        /// </summary>
        public KickCharacterOutMessage KickCharacter(ulong __characterId__, int placeholder)
        {
            return new KickCharacterOutMessage(this, __characterId__, placeholder);
        }

        /// <summary>
        /// 查询服务器状态，是否可以进入
        /// </summary>
        public ReadyToEnterOutMessage ReadyToEnter(int placeholder)
        {
            return new ReadyToEnterOutMessage(this, 0, placeholder);
        }

        /// <summary>
        /// 获得角色的总在线时间
        /// </summary>
        public GetTotleOnlineSecondsOutMessage GetTotleOnlineSeconds(ulong __characterId__, ulong characterId)
        {
            return new GetTotleOnlineSecondsOutMessage(this, __characterId__, characterId);
        }

        /// <summary>
        /// 获取某个账号对应的账号ID
        /// </summary>
        public GetPlayerIdByAccountOutMessage GetPlayerIdByAccount(ulong __characterId__, string account)
        {
            return new GetPlayerIdByAccountOutMessage(this, __characterId__, account);
        }

        /// <summary>
        /// 广播表格重载
        /// </summary>
        public ServerGMCommandOutMessage ServerGMCommand(string cmd, string param)
        {
            return new ServerGMCommandOutMessage(this, 0, cmd, param);
        }

        /// <summary>
        /// 请求各服务器人数
        /// </summary>
        public GetServerCharacterCountOutMessage GetServerCharacterCount(ulong __characterId__, int result)
        {
            return new GetServerCharacterCountOutMessage(this, __characterId__, result);
        }

        /// <summary>
        ///  获取playerid
        /// </summary>
        public GetUserIdOutMessage GetUserId(ulong __characterId__, ulong clientId)
        {
            return new GetUserIdOutMessage(this, __characterId__, clientId);
        }

        /// <summary>
        /// GM相关 begin
        /// 通过角色名称查找角色信息
        /// </summary>
        public GetPlayerDataOutMessage GetPlayerData(ulong __characterId__, ulong playerId, ulong charId)
        {
            return new GetPlayerDataOutMessage(this, __characterId__, playerId, charId);
        }

        /// <summary>
        /// 封账号
        /// </summary>
        public LockAccountOutMessage LockAccount(ulong __characterId__, ulong playerId, long endTime)
        {
            return new LockAccountOutMessage(this, __characterId__, playerId, endTime);
        }

        /// <summary>
        /// </summary>
        public GMKickCharacterOutMessage GMKickCharacter(ulong __characterId__, ulong charId, string name)
        {
            return new GMKickCharacterOutMessage(this, __characterId__, charId, name);
        }

        /// <summary>
        /// </summary>
        public UpdateServerOutMessage UpdateServer(int placeholder)
        {
            return new UpdateServerOutMessage(this, 0, placeholder);
        }

        /// <summary>
        /// 角色转服
        /// </summary>
        public ChangeServerOutMessage ChangeServer(ulong __characterId__, ulong characterId, int serverId)
        {
            return new ChangeServerOutMessage(this, __characterId__, characterId, serverId);
        }

        /// <summary>
        /// </summary>
        public NotiffyGMAccountOutMessage NotiffyGMAccount(ulong __characterId__, LoginAllAccounts acc)
        {
            return new NotiffyGMAccountOutMessage(this, __characterId__, acc);
        }

        /// <summary>
        /// 制作快照
        /// 创建一个新账号和角色为存储快照
        /// </summary>
        public CreateCharacterByAccountNameOutMessage CreateCharacterByAccountName(ulong __characterId__, string accName)
        {
            return new CreateCharacterByAccountNameOutMessage(this, __characterId__, accName);
        }

        /// <summary>
        /// 拷贝一个角色数据到另一个角色id
        /// </summary>
        public CloneCharacterDbByIdOutMessage CloneCharacterDbById(ulong __characterId__, ulong fromId, ulong toId)
        {
            return new CloneCharacterDbByIdOutMessage(this, __characterId__, fromId, toId);
        }

        /// <summary>
        /// 查询用户名下的角色id
        /// </summary>
        public GetCharacterIdByAccountNameOutMessage GetCharacterIdByAccountName(ulong __characterId__, string accName)
        {
            return new GetCharacterIdByAccountNameOutMessage(this, __characterId__, accName);
        }

        /// <summary>
        /// GM相关 end
        /// Logic通知修改玩家名字
        /// </summary>
        public TryModifyPlayerNameOutMessage TryModifyPlayerName(ulong __characterId__, string modifyName)
        {
            return new TryModifyPlayerNameOutMessage(this, __characterId__, modifyName);
        }

        protected override object GetPublishData(uint p, byte[] list)
        {
            switch (p)
            {
                case 2016:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Login_NotifyConnected_ARG_uint64_characterId_int32_servictType_int32_err__>(ms);
                    }
                    break;
                case 2017:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Login_QueryStatus_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 2018:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Login_GetLoginSimpleData_ARG_uint64_characterId__>(ms);
                    }
                    break;
                case 2019:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Login_GetTodayOnlineSeconds_ARG_uint64_characterId__>(ms);
                    }
                    break;
                case 2020:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Login_CheckIsHaveCharacter_ARG_uint64_characterId__>(ms);
                    }
                    break;
                case 2023:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Login_GetCharacterIdByName_ARG_string_name__>(ms);
                    }
                    break;
                case 2024:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Login_KickCharacter_ARG_int32_placeholder__>(ms);
                    }
                    break;
                case 2027:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Login_ReadyToEnter_ARG_int32_placeholder__>(ms);
                    }
                    break;
                case 2028:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Login_GetTotleOnlineSeconds_ARG_uint64_characterId__>(ms);
                    }
                    break;
                case 2029:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Login_GetPlayerIdByAccount_ARG_string_account__>(ms);
                    }
                    break;
                case 2032:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Login_ServerGMCommand_ARG_string_cmd_string_param__>(ms);
                    }
                    break;
                case 2036:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Login_GetServerCharacterCount_ARG_int32_result__>(ms);
                    }
                    break;
                case 2038:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Login_GetUserId_ARG_uint64_clientId__>(ms);
                    }
                    break;
                case 2900:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Login_GetPlayerData_ARG_uint64_playerId_uint64_charId__>(ms);
                    }
                    break;
                case 2901:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Login_LockAccount_ARG_uint64_playerId_int64_endTime__>(ms);
                    }
                    break;
                case 2903:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Login_GMKickCharacter_ARG_uint64_charId_string_name__>(ms);
                    }
                    break;
                case 2904:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Login_UpdateServer_ARG_int32_placeholder__>(ms);
                    }
                    break;
                case 2905:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Login_ChangeServer_ARG_uint64_characterId_int32_serverId__>(ms);
                    }
                    break;
                case 2906:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Login_NotiffyGMAccount_ARG_LoginAllAccounts_acc__>(ms);
                    }
                    break;
                case 2097:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Login_CreateCharacterByAccountName_ARG_string_accName__>(ms);
                    }
                    break;
                case 2098:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Login_CloneCharacterDbById_ARG_uint64_fromId_uint64_toId__>(ms);
                    }
                    break;
                case 2099:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Login_GetCharacterIdByAccountName_ARG_string_accName__>(ms);
                    }
                    break;
                case 2100:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Login_TryModifyPlayerName_ARG_string_modifyName__>(ms);
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
        /// </summary>
        public object Kick(ulong __characterId__, ulong __clientId__, int type)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 2010;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Login;

            var __data__ = new __RPC_Login_Kick_ARG_int32_type__();
            __data__.Type=type;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        ///  通知其他服务器，这个character已经退出了，调用了这个函数以后，Broker和其他服务器中，关于这个Character的数据都会被清理，相当于这个character下线了
        /// </summary>
        public object Logout(ulong __characterId__, ulong __clientId__, ulong characterId)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 2011;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Login;

            var __data__ = new __RPC_Login_Logout_ARG_uint64_characterId__();
            __data__.CharacterId=characterId;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知排队名次
        /// </summary>
        public object NotifyQueueIndex(ulong __characterId__, ulong __clientId__, int index)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 2021;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Login;

            var __data__ = new __RPC_Login_NotifyQueueIndex_ARG_int32_index__();
            __data__.Index=index;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知登录排队成功
        /// </summary>
        public object Discard0(ulong __characterId__, ulong __clientId__, PlayerLoginData plData)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 2022;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Login;

            var __data__ = new __RPC_Login_Discard0_ARG_PlayerLoginData_plData__();
            __data__.PlData=plData;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通用，通知排队成功
        /// </summary>
        public object NotifyQueueSuccess(ulong __characterId__, ulong __clientId__, QueueSuccessData data)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 2030;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Login;

            var __data__ = new __RPC_Login_NotifyQueueSuccess_ARG_QueueSuccessData_data__();
            __data__.Data=data;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知重连
        /// </summary>
        public object NotifyReConnet(ulong __characterId__, ulong __clientId__, int result)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 2035;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Login;

            var __data__ = new __RPC_Login_NotifyReConnet_ARG_int32_result__();
            __data__.Result=result;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
    }

    public class NotifyConnectedOutMessage : OutMessage
    {
        public NotifyConnectedOutMessage(ClientAgentBase sender, ulong __clientId__, ulong characterId, int servictType, int err)
            : base(sender, ServiceType.Login, 2016, (int)MessageType.SS)
        {
            Request = new __RPC_Login_NotifyConnected_ARG_uint64_characterId_int32_servictType_int32_err__();
            mMessage.ClientId = __clientId__;
            Request.CharacterId=characterId;
            Request.ServictType=servictType;
            Request.Err=err;

        }

        public __RPC_Login_NotifyConnected_ARG_uint64_characterId_int32_servictType_int32_err__ Request { get; private set; }


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

    public class QueryStatusOutMessage : OutMessage
    {
        public QueryStatusOutMessage(ClientAgentBase sender, ulong __clientId__, uint placeholder)
            : base(sender, ServiceType.Login, 2017, (int)MessageType.SS)
        {
            Request = new __RPC_Login_QueryStatus_ARG_uint32_placeholder__();
            mMessage.ClientId = __clientId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Login_QueryStatus_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Login_QueryStatus_RET_LoginServerStatus__ mResponse;
            public LoginServerStatus Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Login_QueryStatus_RET_LoginServerStatus__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetLoginSimpleDataOutMessage : OutMessage
    {
        public GetLoginSimpleDataOutMessage(ClientAgentBase sender, ulong __clientId__, ulong characterId)
            : base(sender, ServiceType.Login, 2018, (int)MessageType.SS)
        {
            Request = new __RPC_Login_GetLoginSimpleData_ARG_uint64_characterId__();
            mMessage.ClientId = __clientId__;
            Request.CharacterId=characterId;

        }

        public __RPC_Login_GetLoginSimpleData_ARG_uint64_characterId__ Request { get; private set; }

            private __RPC_Login_GetLoginSimpleData_RET_DBCharacterLoginSimple__ mResponse;
            public DBCharacterLoginSimple Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Login_GetLoginSimpleData_RET_DBCharacterLoginSimple__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetTodayOnlineSecondsOutMessage : OutMessage
    {
        public GetTodayOnlineSecondsOutMessage(ClientAgentBase sender, ulong __clientId__, ulong characterId)
            : base(sender, ServiceType.Login, 2019, (int)MessageType.SS)
        {
            Request = new __RPC_Login_GetTodayOnlineSeconds_ARG_uint64_characterId__();
            mMessage.ClientId = __clientId__;
            Request.CharacterId=characterId;

        }

        public __RPC_Login_GetTodayOnlineSeconds_ARG_uint64_characterId__ Request { get; private set; }

            private __RPC_Login_GetTodayOnlineSeconds_RET_int64__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Login_GetTodayOnlineSeconds_RET_int64__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class CheckIsHaveCharacterOutMessage : OutMessage
    {
        public CheckIsHaveCharacterOutMessage(ClientAgentBase sender, ulong __clientId__, ulong characterId)
            : base(sender, ServiceType.Login, 2020, (int)MessageType.SS)
        {
            Request = new __RPC_Login_CheckIsHaveCharacter_ARG_uint64_characterId__();
            mMessage.ClientId = __clientId__;
            Request.CharacterId=characterId;

        }

        public __RPC_Login_CheckIsHaveCharacter_ARG_uint64_characterId__ Request { get; private set; }

            private __RPC_Login_CheckIsHaveCharacter_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Login_CheckIsHaveCharacter_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetCharacterIdByNameOutMessage : OutMessage
    {
        public GetCharacterIdByNameOutMessage(ClientAgentBase sender, ulong __clientId__, string name)
            : base(sender, ServiceType.Login, 2023, (int)MessageType.SS)
        {
            Request = new __RPC_Login_GetCharacterIdByName_ARG_string_name__();
            mMessage.ClientId = __clientId__;
            Request.Name=name;

        }

        public __RPC_Login_GetCharacterIdByName_ARG_string_name__ Request { get; private set; }

            private __RPC_Login_GetCharacterIdByName_RET_uint64__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Login_GetCharacterIdByName_RET_uint64__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class KickCharacterOutMessage : OutMessage
    {
        public KickCharacterOutMessage(ClientAgentBase sender, ulong __clientId__, int placeholder)
            : base(sender, ServiceType.Login, 2024, (int)MessageType.SS)
        {
            Request = new __RPC_Login_KickCharacter_ARG_int32_placeholder__();
            mMessage.ClientId = __clientId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Login_KickCharacter_ARG_int32_placeholder__ Request { get; private set; }

            private __RPC_Login_KickCharacter_RET_int64__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Login_KickCharacter_RET_int64__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class ReadyToEnterOutMessage : OutMessage
    {
        public ReadyToEnterOutMessage(ClientAgentBase sender, ulong __clientId__, int placeholder)
            : base(sender, ServiceType.Login, 2027, (int)MessageType.SAS)
        {
            Request = new __RPC_Login_ReadyToEnter_ARG_int32_placeholder__();
            mMessage.ClientId = __clientId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Login_ReadyToEnter_ARG_int32_placeholder__ Request { get; private set; }

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
                Response.Add(Serializer.Deserialize<__RPC_Login_ReadyToEnter_RET_int32__>(ms).ReturnValue);
            }
            State = MessageState.Reply;
            if(ErrorCode == 0) ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetTotleOnlineSecondsOutMessage : OutMessage
    {
        public GetTotleOnlineSecondsOutMessage(ClientAgentBase sender, ulong __clientId__, ulong characterId)
            : base(sender, ServiceType.Login, 2028, (int)MessageType.SS)
        {
            Request = new __RPC_Login_GetTotleOnlineSeconds_ARG_uint64_characterId__();
            mMessage.ClientId = __clientId__;
            Request.CharacterId=characterId;

        }

        public __RPC_Login_GetTotleOnlineSeconds_ARG_uint64_characterId__ Request { get; private set; }

            private __RPC_Login_GetTotleOnlineSeconds_RET_int64__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Login_GetTotleOnlineSeconds_RET_int64__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetPlayerIdByAccountOutMessage : OutMessage
    {
        public GetPlayerIdByAccountOutMessage(ClientAgentBase sender, ulong __clientId__, string account)
            : base(sender, ServiceType.Login, 2029, (int)MessageType.SS)
        {
            Request = new __RPC_Login_GetPlayerIdByAccount_ARG_string_account__();
            mMessage.ClientId = __clientId__;
            Request.Account=account;

        }

        public __RPC_Login_GetPlayerIdByAccount_ARG_string_account__ Request { get; private set; }

            private __RPC_Login_GetPlayerIdByAccount_RET_uint64__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Login_GetPlayerIdByAccount_RET_uint64__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class ServerGMCommandOutMessage : OutMessage
    {
        public ServerGMCommandOutMessage(ClientAgentBase sender, ulong __clientId__, string cmd, string param)
            : base(sender, ServiceType.Login, 2032, (int)MessageType.SAS)
        {
            Request = new __RPC_Login_ServerGMCommand_ARG_string_cmd_string_param__();
            mMessage.ClientId = __clientId__;
            Request.Cmd=cmd;
            Request.Param=param;

        }

        public __RPC_Login_ServerGMCommand_ARG_string_cmd_string_param__ Request { get; private set; }


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

    public class GetServerCharacterCountOutMessage : OutMessage
    {
        public GetServerCharacterCountOutMessage(ClientAgentBase sender, ulong __clientId__, int result)
            : base(sender, ServiceType.Login, 2036, (int)MessageType.SS)
        {
            Request = new __RPC_Login_GetServerCharacterCount_ARG_int32_result__();
            mMessage.ClientId = __clientId__;
            Request.Result=result;

        }

        public __RPC_Login_GetServerCharacterCount_ARG_int32_result__ Request { get; private set; }

            private __RPC_Login_GetServerCharacterCount_RET_Dict_int_int_Data__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Login_GetServerCharacterCount_RET_Dict_int_int_Data__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetUserIdOutMessage : OutMessage
    {
        public GetUserIdOutMessage(ClientAgentBase sender, ulong __clientId__, ulong clientId)
            : base(sender, ServiceType.Login, 2038, (int)MessageType.SS)
        {
            Request = new __RPC_Login_GetUserId_ARG_uint64_clientId__();
            mMessage.ClientId = __clientId__;
            Request.ClientId=clientId;

        }

        public __RPC_Login_GetUserId_ARG_uint64_clientId__ Request { get; private set; }

            private __RPC_Login_GetUserId_RET_string__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Login_GetUserId_RET_string__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetPlayerDataOutMessage : OutMessage
    {
        public GetPlayerDataOutMessage(ClientAgentBase sender, ulong __clientId__, ulong playerId, ulong charId)
            : base(sender, ServiceType.Login, 2900, (int)MessageType.SS)
        {
            Request = new __RPC_Login_GetPlayerData_ARG_uint64_playerId_uint64_charId__();
            mMessage.ClientId = __clientId__;
            Request.PlayerId=playerId;
            Request.CharId=charId;

        }

        public __RPC_Login_GetPlayerData_ARG_uint64_playerId_uint64_charId__ Request { get; private set; }

            private __RPC_Login_GetPlayerData_RET_GMPlayerInfoMsg__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Login_GetPlayerData_RET_GMPlayerInfoMsg__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class LockAccountOutMessage : OutMessage
    {
        public LockAccountOutMessage(ClientAgentBase sender, ulong __clientId__, ulong playerId, long endTime)
            : base(sender, ServiceType.Login, 2901, (int)MessageType.SS)
        {
            Request = new __RPC_Login_LockAccount_ARG_uint64_playerId_int64_endTime__();
            mMessage.ClientId = __clientId__;
            Request.PlayerId=playerId;
            Request.EndTime=endTime;

        }

        public __RPC_Login_LockAccount_ARG_uint64_playerId_int64_endTime__ Request { get; private set; }

            private __RPC_Login_LockAccount_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Login_LockAccount_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GMKickCharacterOutMessage : OutMessage
    {
        public GMKickCharacterOutMessage(ClientAgentBase sender, ulong __clientId__, ulong charId, string name)
            : base(sender, ServiceType.Login, 2903, (int)MessageType.SS)
        {
            Request = new __RPC_Login_GMKickCharacter_ARG_uint64_charId_string_name__();
            mMessage.ClientId = __clientId__;
            Request.CharId=charId;
            Request.Name=name;

        }

        public __RPC_Login_GMKickCharacter_ARG_uint64_charId_string_name__ Request { get; private set; }

            private __RPC_Login_GMKickCharacter_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Login_GMKickCharacter_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class UpdateServerOutMessage : OutMessage
    {
        public UpdateServerOutMessage(ClientAgentBase sender, ulong __clientId__, int placeholder)
            : base(sender, ServiceType.Login, 2904, (int)MessageType.SAS)
        {
            Request = new __RPC_Login_UpdateServer_ARG_int32_placeholder__();
            mMessage.ClientId = __clientId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Login_UpdateServer_ARG_int32_placeholder__ Request { get; private set; }


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

    public class ChangeServerOutMessage : OutMessage
    {
        public ChangeServerOutMessage(ClientAgentBase sender, ulong __clientId__, ulong characterId, int serverId)
            : base(sender, ServiceType.Login, 2905, (int)MessageType.SS)
        {
            Request = new __RPC_Login_ChangeServer_ARG_uint64_characterId_int32_serverId__();
            mMessage.ClientId = __clientId__;
            Request.CharacterId=characterId;
            Request.ServerId=serverId;

        }

        public __RPC_Login_ChangeServer_ARG_uint64_characterId_int32_serverId__ Request { get; private set; }

            private __RPC_Login_ChangeServer_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Login_ChangeServer_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class NotiffyGMAccountOutMessage : OutMessage
    {
        public NotiffyGMAccountOutMessage(ClientAgentBase sender, ulong __clientId__, LoginAllAccounts acc)
            : base(sender, ServiceType.Login, 2906, (int)MessageType.SS)
        {
            Request = new __RPC_Login_NotiffyGMAccount_ARG_LoginAllAccounts_acc__();
            mMessage.ClientId = __clientId__;
            Request.Acc=acc;

        }

        public __RPC_Login_NotiffyGMAccount_ARG_LoginAllAccounts_acc__ Request { get; private set; }


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

    public class CreateCharacterByAccountNameOutMessage : OutMessage
    {
        public CreateCharacterByAccountNameOutMessage(ClientAgentBase sender, ulong __clientId__, string accName)
            : base(sender, ServiceType.Login, 2097, (int)MessageType.SS)
        {
            Request = new __RPC_Login_CreateCharacterByAccountName_ARG_string_accName__();
            mMessage.ClientId = __clientId__;
            Request.AccName=accName;

        }

        public __RPC_Login_CreateCharacterByAccountName_ARG_string_accName__ Request { get; private set; }

            private __RPC_Login_CreateCharacterByAccountName_RET_uint64__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Login_CreateCharacterByAccountName_RET_uint64__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class CloneCharacterDbByIdOutMessage : OutMessage
    {
        public CloneCharacterDbByIdOutMessage(ClientAgentBase sender, ulong __clientId__, ulong fromId, ulong toId)
            : base(sender, ServiceType.Login, 2098, (int)MessageType.SS)
        {
            Request = new __RPC_Login_CloneCharacterDbById_ARG_uint64_fromId_uint64_toId__();
            mMessage.ClientId = __clientId__;
            Request.FromId=fromId;
            Request.ToId=toId;

        }

        public __RPC_Login_CloneCharacterDbById_ARG_uint64_fromId_uint64_toId__ Request { get; private set; }

            private __RPC_Login_CloneCharacterDbById_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Login_CloneCharacterDbById_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetCharacterIdByAccountNameOutMessage : OutMessage
    {
        public GetCharacterIdByAccountNameOutMessage(ClientAgentBase sender, ulong __clientId__, string accName)
            : base(sender, ServiceType.Login, 2099, (int)MessageType.SS)
        {
            Request = new __RPC_Login_GetCharacterIdByAccountName_ARG_string_accName__();
            mMessage.ClientId = __clientId__;
            Request.AccName=accName;

        }

        public __RPC_Login_GetCharacterIdByAccountName_ARG_string_accName__ Request { get; private set; }

            private __RPC_Login_GetCharacterIdByAccountName_RET_uint64__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Login_GetCharacterIdByAccountName_RET_uint64__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class TryModifyPlayerNameOutMessage : OutMessage
    {
        public TryModifyPlayerNameOutMessage(ClientAgentBase sender, ulong __clientId__, string modifyName)
            : base(sender, ServiceType.Login, 2100, (int)MessageType.SS)
        {
            Request = new __RPC_Login_TryModifyPlayerName_ARG_string_modifyName__();
            mMessage.ClientId = __clientId__;
            Request.ModifyName=modifyName;

        }

        public __RPC_Login_TryModifyPlayerName_ARG_string_modifyName__ Request { get; private set; }

            private __RPC_Login_TryModifyPlayerName_RET_string__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Login_TryModifyPlayerName_RET_string__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class AddFunctionNameLogin
    {
        public static void AddFunctionName(IDictionary<int, string> dict)
        {
            dict[2001] = "PlayerLoginByUserNamePassword";
            dict[2002] = "PlayerLoginByThirdKey";
            dict[2003] = "PlayerSelectServerId";
            dict[2010] = "Kick";
            dict[2011] = "Logout";
            dict[2012] = "CreateCharacter";
            dict[2013] = "EnterGame";
            dict[2014] = "SyncTime";
            dict[2015] = "GetServerList";
            dict[2016] = "NotifyConnected";
            dict[2017] = "QueryStatus";
            dict[2018] = "GetLoginSimpleData";
            dict[2019] = "GetTodayOnlineSeconds";
            dict[2020] = "CheckIsHaveCharacter";
            dict[2021] = "NotifyQueueIndex";
            dict[2022] = "Discard0";
            dict[2023] = "GetCharacterIdByName";
            dict[2024] = "KickCharacter";
            dict[2025] = "ExitLogin";
            dict[2026] = "ExitSelectCharacter";
            dict[2027] = "ReadyToEnter";
            dict[2028] = "GetTotleOnlineSeconds";
            dict[2029] = "GetPlayerIdByAccount";
            dict[2030] = "NotifyQueueSuccess";
            dict[2031] = "QueryServerTimezone";
            dict[2032] = "ServerGMCommand";
            dict[2033] = "GateDisconnect";
            dict[2034] = "ReConnet";
            dict[2035] = "NotifyReConnet";
            dict[2036] = "GetServerCharacterCount";
            dict[2037] = "SendDeviceUdid";
            dict[2038] = "GetUserId";
            dict[2039] = "GetAllCharactersLoginInfo";
            dict[2040] = "GetAnchorIsInRoom";
            dict[2900] = "GetPlayerData";
            dict[2901] = "LockAccount";
            dict[2903] = "GMKickCharacter";
            dict[2904] = "UpdateServer";
            dict[2905] = "ChangeServer";
            dict[2906] = "NotiffyGMAccount";
            dict[2097] = "CreateCharacterByAccountName";
            dict[2098] = "CloneCharacterDbById";
            dict[2099] = "GetCharacterIdByAccountName";
            dict[2100] = "TryModifyPlayerName";
        }
        public static void AddCSFunctionName(IDictionary<int, string> dict)
        {
            dict[2001] = "PlayerLoginByUserNamePassword";
            dict[2002] = "PlayerLoginByThirdKey";
            dict[2003] = "PlayerSelectServerId";
            dict[2012] = "CreateCharacter";
            dict[2013] = "EnterGame";
            dict[2014] = "SyncTime";
            dict[2015] = "GetServerList";
            dict[2025] = "ExitLogin";
            dict[2026] = "ExitSelectCharacter";
            dict[2031] = "QueryServerTimezone";
            dict[2033] = "GateDisconnect";
            dict[2034] = "ReConnet";
            dict[2037] = "SendDeviceUdid";
            dict[2039] = "GetAllCharactersLoginInfo";
            dict[2040] = "GetAnchorIsInRoom";
        }
    }
}
