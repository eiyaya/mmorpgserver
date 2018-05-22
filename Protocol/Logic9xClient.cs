using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Scorpion;
using DataContract;
using ProtoBuf;

#pragma warning disable 0162,0108
namespace LogicClientService
{

    public abstract class LogicAgent : ClientAgentBase
    {
        public LogicAgent(string addr)
            : base(addr)
        {
        }

        public LogicAgent(ServerInfo broker, ServerInfo[] directConnect, Func<ulong, int> characterId2ServerId)
            : base(broker, directConnect, characterId2ServerId)
        {
        }

        /// <summary>
        /// </summary>
        public PrepareDataForEnterGameOutMessage PrepareDataForEnterGame(ulong __characterId__, int serverId, ulong sceneGuid)
        {
            return new PrepareDataForEnterGameOutMessage(this, __characterId__, serverId, sceneGuid);
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
        public CreateCharacterOutMessage CreateCharacter(ulong __characterId__, int type, int serverId, bool isGM)
        {
            return new CreateCharacterOutMessage(this, __characterId__, type, serverId, isGM);
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
        /// 获得Logic的Simple数据
        /// </summary>
        public GetLogicSimpleDataOutMessage GetLogicSimpleData(ulong __characterId__, uint placeholder)
        {
            return new GetLogicSimpleDataOutMessage(this, __characterId__, placeholder);
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
        /// 请求装备数据
        /// </summary>
        public LogicGetEquipListOutMessage LogicGetEquipList(ulong __characterId__, ulong chararcterId)
        {
            return new LogicGetEquipListOutMessage(this, __characterId__, chararcterId);
        }

        /// <summary>
        /// 请求技能数据
        /// </summary>
        public LogicGetSkillDataOutMessage LogicGetSkillData(ulong __characterId__, ulong chararcterId)
        {
            return new LogicGetSkillDataOutMessage(this, __characterId__, chararcterId);
        }

        /// <summary>
        /// 请求天赋数据
        /// </summary>
        public LogicGetTalentDataOutMessage LogicGetTalentData(ulong __characterId__, ulong chararcterId)
        {
            return new LogicGetTalentDataOutMessage(this, __characterId__, chararcterId);
        }

        /// <summary>
        /// 请求图鉴属性数据
        /// </summary>
        public LogicGetBookAttrDataOutMessage LogicGetBookAttrData(ulong __characterId__, ulong chararcterId)
        {
            return new LogicGetBookAttrDataOutMessage(this, __characterId__, chararcterId);
        }

        /// <summary>
        /// 杀怪Id
        /// </summary>
        public LogicKillMonsterOutMessage LogicKillMonster(ulong __characterId__, int MonsterId, int addExp, int sceneId)
        {
            return new LogicKillMonsterOutMessage(this, __characterId__, MonsterId, addExp, sceneId);
        }

        /// <summary>
        /// 获得物品
        /// </summary>
        public GiveItemOutMessage GiveItem(ulong __characterId__, int itemId, int itemCount, int from)
        {
            return new GiveItemOutMessage(this, __characterId__, itemId, itemCount, from);
        }

        /// <summary>
        /// 扣除道具
        /// </summary>
        public DeleteItemOutMessage DeleteItem(ulong __characterId__, int itemId, int itemCount, int deleteType)
        {
            return new DeleteItemOutMessage(this, __characterId__, itemId, itemCount, deleteType);
        }

        /// <summary>
        /// 每天的首次登陆(连续登陆天数)
        /// </summary>
        public FirstOnlineOutMessage FirstOnline(ulong __characterId__, ulong clientId, ulong chararcterId, int continuedLanding)
        {
            return new FirstOnlineOutMessage(this, __characterId__, clientId, chararcterId, continuedLanding);
        }

        /// <summary>
        /// 其他服务器要求添加好友
        /// </summary>
        public SSAddFriendByIdOutMessage SSAddFriendById(ulong __characterId__, ulong characterId, int type)
        {
            return new SSAddFriendByIdOutMessage(this, __characterId__, characterId, type);
        }

        /// <summary>
        /// 广播表格重载
        /// </summary>
        public ServerGMCommandOutMessage ServerGMCommand(string cmd, string param)
        {
            return new ServerGMCommandOutMessage(this, 0, cmd, param);
        }

        /// <summary>
        /// 完成副本
        /// </summary>
        public CompleteFubenOutMessage CompleteFuben(ulong __characterId__, FubenResult result)
        {
            return new CompleteFubenOutMessage(this, __characterId__, result);
        }

        /// <summary>
        /// 耐久度下降
        /// </summary>
        public DurableDownOutMessage DurableDown(ulong __characterId__, Dict_int_int_Data bagidList)
        {
            return new DurableDownOutMessage(this, __characterId__, bagidList);
        }

        /// <summary>
        /// 场景服务器请求使用NPC服务
        /// </summary>
        public NpcServiceOutMessage NpcService(ulong __characterId__, int serviceId)
        {
            return new NpcServiceOutMessage(this, __characterId__, serviceId);
        }

        /// <summary>
        /// 触发翻牌
        /// </summary>
        public PushDrawOutMessage PushDraw(ulong __characterId__, int drawId)
        {
            return new PushDrawOutMessage(this, __characterId__, drawId);
        }

        /// <summary>
        /// 检查玩家是否满足进入副本的需求
        /// </summary>
        public CheckCharacterInFubenOutMessage CheckCharacterInFuben(ulong __characterId__, int fubenId)
        {
            return new CheckCharacterInFubenOutMessage(this, __characterId__, fubenId);
        }

        /// <summary>
        /// 天梯结果有改变
        /// </summary>
        public LogicP1vP1FightOverOutMessage LogicP1vP1FightOver(ulong __characterId__, ulong characterId, int result, int rank)
        {
            return new LogicP1vP1FightOverOutMessage(this, __characterId__, characterId, result, rank);
        }

        /// <summary>
        /// 天梯名次有所前进
        /// </summary>
        public LogicP1vP1LadderAdvanceOutMessage LogicP1vP1LadderAdvance(ulong __characterId__, int rank)
        {
            return new LogicP1vP1LadderAdvanceOutMessage(this, __characterId__, rank);
        }

        /// <summary>
        /// 记录天梯变化
        /// </summary>
        public PushP1vP1LadderChangeOutMessage PushP1vP1LadderChange(ulong __characterId__, int type, string name, int result, int oldRank, int newRank)
        {
            return new PushP1vP1LadderChangeOutMessage(this, __characterId__, type, name, result, oldRank, newRank);
        }

        /// <summary>
        /// 支持其他服务器获取标记位和条件表
        /// </summary>
        public SSGetFlagOrConditionOutMessage SSGetFlagOrCondition(ulong __characterId__, ulong guid, int flagId, int conditionid)
        {
            return new SSGetFlagOrConditionOutMessage(this, __characterId__, guid, flagId, conditionid);
        }

        /// <summary>
        /// 查询玩家是否屏蔽了另一个玩家
        /// </summary>
        public SSIsShieldOutMessage SSIsShield(ulong __characterId__, ulong guid, ulong Shield)
        {
            return new SSIsShieldOutMessage(this, __characterId__, guid, Shield);
        }

        /// <summary>
        /// 修改扩展数据
        /// </summary>
        public SSChangeExdataOutMessage SSChangeExdata(ulong __characterId__, Dict_int_int_Data changes)
        {
            return new SSChangeExdataOutMessage(this, __characterId__, changes);
        }

        /// <summary>
        /// 交易系统：有A要买B的东西
        /// </summary>
        public SSStoreOperationBuyOutMessage SSStoreOperationBuy(ulong __characterId__, long storeId, ulong Aid, string name, int resType, int resCount, ItemBaseData itemdata)
        {
            return new SSStoreOperationBuyOutMessage(this, __characterId__, storeId, Aid, name, resType, resCount, itemdata);
        }

        /// <summary>
        /// 获得某人的商店道具
        /// </summary>
        public GetExchangeDataOutMessage GetExchangeData(ulong __characterId__, ulong characterId)
        {
            return new GetExchangeDataOutMessage(this, __characterId__, characterId);
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        public SendMailToCharacterByItemsOutMessage SendMailToCharacterByItems(ulong __characterId__, int mailId, ItemBaseData item, StringArray args)
        {
            return new SendMailToCharacterByItemsOutMessage(this, __characterId__, mailId, item, args);
        }

        /// <summary>
        /// 战盟信息发生变化的通知
        /// type: 0:申请被同意后，通知申请者	1:被踢出	2:被拒绝	3:战盟权限变化
        /// </summary>
        public AllianceDataChangeOutMessage AllianceDataChange(ulong __characterId__, int type, int allianceId, int ladder, string name)
        {
            return new AllianceDataChangeOutMessage(this, __characterId__, type, allianceId, ladder, name);
        }

        /// <summary>
        ///  战场结果
        /// </summary>
        public SSBattleResultOutMessage SSBattleResult(ulong __characterId__, int fubenId, int type)
        {
            return new SSBattleResultOutMessage(this, __characterId__, fubenId, type);
        }

        /// <summary>
        /// 查询服务器状态，是否可以进入
        /// </summary>
        public ReadyToEnterOutMessage ReadyToEnter(int placeholder)
        {
            return new ReadyToEnterOutMessage(this, 0, placeholder);
        }

        /// <summary>
        /// 获得所有有我的好友数据
        /// </summary>
        public SSGetFriendListOutMessage SSGetFriendList(ulong __characterId__, int typeId)
        {
            return new SSGetFriendListOutMessage(this, __characterId__, typeId);
        }

        /// <summary>
        /// 通知数据变化了
        /// </summary>
        public SSSendSimpleDataOutMessage SSSendSimpleData(ulong __characterId__, ulong haveId, CharacterSimpleData SimpleData)
        {
            return new SSSendSimpleDataOutMessage(this, __characterId__, haveId, SimpleData);
        }

        /// <summary>
        /// 通知有人加了自己好友仇人等变化了
        /// </summary>
        public SSFriendpPssiveChangeOutMessage SSFriendpPssiveChange(ulong __characterId__, int type, ulong characterId, int operate)
        {
            return new SSFriendpPssiveChangeOutMessage(this, __characterId__, type, characterId, operate);
        }

        /// <summary>
        /// 获取某人的ExData
        /// </summary>
        public SSFetchExdataOutMessage SSFetchExdata(ulong __characterId__, Int32Array idList)
        {
            return new SSFetchExdataOutMessage(this, __characterId__, idList);
        }

        /// <summary>
        /// 扣除道具
        /// </summary>
        public SSDeleteItemByIndexOutMessage SSDeleteItemByIndex(ulong __characterId__, int bagId, int bagIndex, int itemCount)
        {
            return new SSDeleteItemByIndexOutMessage(this, __characterId__, bagId, bagIndex, itemCount);
        }

        /// <summary>
        /// 请求某个任务可以相位到的场景
        /// </summary>
        public SSGetMissionEnterSceneOutMessage SSGetMissionEnterScene(ulong __characterId__, int missionId)
        {
            return new SSGetMissionEnterSceneOutMessage(this, __characterId__, missionId);
        }

        /// <summary>
        /// 场景要关闭了，通知Logic Server
        /// </summary>
        public NotifyDungeonCloseOutMessage NotifyDungeonClose(ulong __characterId__, int fubenId, Uint64Array playerIds)
        {
            return new NotifyDungeonCloseOutMessage(this, __characterId__, fubenId, playerIds);
        }

        /// <summary>
        /// 修改某角色的扩展数据
        /// </summary>
        public SSSetExdataOutMessage SSSetExdata(ulong __characterId__, Dict_int_int_Data changes)
        {
            return new SSSetExdataOutMessage(this, __characterId__, changes);
        }

        /// <summary>
        /// 修改某角色的flag数据
        /// </summary>
        public SSSetFlagOutMessage SSSetFlag(ulong __characterId__, Dict_int_int_Data changes)
        {
            return new SSSetFlagOutMessage(this, __characterId__, changes);
        }

        /// <summary>
        /// 请求称号数据
        /// </summary>
        public LogicGetTitleListOutMessage LogicGetTitleList(ulong __characterId__, uint placeholder)
        {
            return new LogicGetTitleListOutMessage(this, __characterId__, placeholder);
        }

        /// <summary>
        /// 处理充值成功消息
        /// </summary>
        public RechargeSuccessOutMessage RechargeSuccess(ulong __characterId__, string platform, int payType, float price, string orderId, string channel)
        {
            return new RechargeSuccessOutMessage(this, __characterId__, platform, payType, price, orderId, channel);
        }

        /// <summary>
        /// 获得物品数量
        /// </summary>
        public GetItemCountOutMessage GetItemCount(ulong __characterId__, int itemId)
        {
            return new GetItemCountOutMessage(this, __characterId__, itemId);
        }

        /// <summary>
        /// 同步战盟Buff
        /// </summary>
        public SSGetAllianceBuffOutMessage SSGetAllianceBuff(ulong __characterId__, int placeholder)
        {
            return new SSGetAllianceBuffOutMessage(this, __characterId__, placeholder);
        }

        /// <summary>
        /// 同步战盟Buff
        /// </summary>
        public NotifyAllianceWarInfoOutMessage NotifyAllianceWarInfo(AllianceWarInfo info)
        {
            return new NotifyAllianceWarInfoOutMessage(this, 0, info);
        }

        /// <summary>
        /// 别的服务器（team）通知logic，我进入某个副本啦，注意处理相关数据
        /// </summary>
        public NotifyEnterFubenOutMessage NotifyEnterFuben(ulong __characterId__, int fubenId)
        {
            return new NotifyEnterFubenOutMessage(this, __characterId__, fubenId);
        }

        /// <summary>
        /// 通知loigc某人要转服了，需要处理相关数据
        /// </summary>
        public ChangeServerOutMessage ChangeServer(ulong __characterId__, int newServerId)
        {
            return new ChangeServerOutMessage(this, __characterId__, newServerId);
        }

        /// <summary>
        /// 请求精灵buff数据
        /// </summary>
        public LogicGetElfDataOutMessage LogicGetElfData(ulong __characterId__, ulong chararcterId)
        {
            return new LogicGetElfDataOutMessage(this, __characterId__, chararcterId);
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
        /// 场景向Logic请求建筑随从数据
        /// </summary>
        public SSRequestCityBuidlingPetDataOutMessage SSRequestCityBuidlingPetData(ulong __characterId__, ulong guid)
        {
            return new SSRequestCityBuidlingPetDataOutMessage(this, __characterId__, guid);
        }

        /// <summary>
        /// 获取玩家副本次数
        /// </summary>
        public SSGetTodayFunbenCountOutMessage SSGetTodayFunbenCount(ulong __characterId__, int serverId, ulong characterId, int selecttype)
        {
            return new SSGetTodayFunbenCountOutMessage(this, __characterId__, serverId, characterId, selecttype);
        }

        /// <summary>
        /// 请求装备数据
        /// </summary>
        public LogicGetAnyDataOutMessage LogicGetAnyData(ulong __characterId__, ulong chararcterId)
        {
            return new LogicGetAnyDataOutMessage(this, __characterId__, chararcterId);
        }

        /// <summary>
        /// 请求获得雕像奖励
        /// </summary>
        public SSSyncCharacterFightPointOutMessage SSSyncCharacterFightPoint(ulong __characterId__, int fp)
        {
            return new SSSyncCharacterFightPointOutMessage(this, __characterId__, fp);
        }

        /// <summary>
        /// 当玩家进入场景后(Scene->Logic)
        /// </summary>
        public OnPlayerEnterSceneOverOutMessage OnPlayerEnterSceneOver(ulong __characterId__, int sceneId)
        {
            return new OnPlayerEnterSceneOverOutMessage(this, __characterId__, sceneId);
        }

        /// <summary>
        /// </summary>
        public AnchorGiftOutMessage AnchorGift(ulong __characterId__, int itemId, int count)
        {
            return new AnchorGiftOutMessage(this, __characterId__, itemId, count);
        }

        /// <summary>
        /// scene请求logic学习技能
        /// </summary>
        public SSLearnSkillOutMessage SSLearnSkill(ulong __characterId__, int skillId, int skillLevel)
        {
            return new SSLearnSkillOutMessage(this, __characterId__, skillId, skillLevel);
        }

        /// <summary>
        /// </summary>
        public GMDeleteMessageOutMessage GMDeleteMessage(ulong __characterId__, ulong id)
        {
            return new GMDeleteMessageOutMessage(this, __characterId__, id);
        }

        /// <summary>
        /// GM相关 begin  逻辑包放GM工具前
        /// </summary>
        public GetCharacterDataOutMessage GetCharacterData(ulong __characterId__, ulong id)
        {
            return new GetCharacterDataOutMessage(this, __characterId__, id);
        }

        /// <summary>
        /// </summary>
        public SendMailToCharacterOutMessage SendMailToCharacter(ulong __characterId__, string title, string content, Dict_int_int_Data items, int state)
        {
            return new SendMailToCharacterOutMessage(this, __characterId__, title, content, items, state);
        }

        /// <summary>
        /// </summary>
        public SendMailToServerOutMessage SendMailToServer(uint serverId, ulong mailId)
        {
            return new SendMailToServerOutMessage(this, 0, serverId, mailId);
        }

        /// <summary>
        /// </summary>
        public UpdateServerOutMessage UpdateServer(int placeholder)
        {
            return new UpdateServerOutMessage(this, 0, placeholder);
        }

        /// <summary>
        /// </summary>
        public GMCommandOutMessage GMCommand(ulong __characterId__, StringArray commonds)
        {
            return new GMCommandOutMessage(this, __characterId__, commonds);
        }

        /// <summary>
        /// 补充type = 0 是发送给玩家的物品 数量读表  type = 1时 物品读表 数量取count * 表里的数量
        /// </summary>
        public SendMailToCharacterByIdOutMessage SendMailToCharacterById(ulong __characterId__, int mailId, int createType, int type, Int32Array countList)
        {
            return new SendMailToCharacterByIdOutMessage(this, __characterId__, mailId, createType, type, countList);
        }

        /// <summary>
        /// </summary>
        public LogicGetMountDataOutMessage LogicGetMountData(ulong __characterId__, ulong chararcterId)
        {
            return new LogicGetMountDataOutMessage(this, __characterId__, chararcterId);
        }

        /// <summary>
        /// 拷贝一个角色数据到另一个角色id
        /// </summary>
        public CloneCharacterDbByIdOutMessage CloneCharacterDbById(ulong __characterId__, ulong fromId, ulong toId)
        {
            return new CloneCharacterDbByIdOutMessage(this, __characterId__, fromId, toId);
        }

        /// <summary>
        /// </summary>
        public GiveItemListOutMessage GiveItemList(ulong __characterId__, Dict_int_int_Data items, int from)
        {
            return new GiveItemListOutMessage(this, __characterId__, items, from);
        }

        /// <summary>
        /// </summary>
        public ApplyPlayerFlagOutMessage ApplyPlayerFlag(ulong __characterId__, Int32Array flagList)
        {
            return new ApplyPlayerFlagOutMessage(this, __characterId__, flagList);
        }

        /// <summary>
        /// </summary>
        public ApplyMayaSkillOutMessage ApplyMayaSkill(ulong __characterId__, int id)
        {
            return new ApplyMayaSkillOutMessage(this, __characterId__, id);
        }

        /// <summary>
        /// 其他服务器请求给玩家发送邮件
        /// </summary>
        public SSSendMailByIdOutMessage SSSendMailById(ulong __characterId__, int tableId, int ExtendType, string ExtendPara0, string ExtendPara1)
        {
            return new SSSendMailByIdOutMessage(this, __characterId__, tableId, ExtendType, ExtendPara0, ExtendPara1);
        }

        /// <summary>
        /// </summary>
        public NotifyPlayerMoniterDataOutMessage NotifyPlayerMoniterData(ulong __characterId__, MsgChatMoniterData data)
        {
            return new NotifyPlayerMoniterDataOutMessage(this, __characterId__, data);
        }

        /// <summary>
        /// </summary>
        public GetPlayerMoniterDataOutMessage GetPlayerMoniterData(ulong __characterId__, ulong characterId)
        {
            return new GetPlayerMoniterDataOutMessage(this, __characterId__, characterId);
        }

        /// <summary>
        /// 通知目标玩家的Logic收到角斗
        /// </summary>
        public NotifyInviteChallengeOutMessage NotifyInviteChallenge(ulong __characterId__, ulong invitorId, string invitorName, int invitorServerId)
        {
            return new NotifyInviteChallengeOutMessage(this, __characterId__, invitorId, invitorName, invitorServerId);
        }

        protected override object GetPublishData(uint p, byte[] list)
        {
            switch (p)
            {
                case 1000:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_PrepareDataForEnterGame_ARG_int32_serverId_uint64_sceneGuid__>(ms);
                    }
                    break;
                case 1001:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_PrepareDataForCreateCharacter_ARG_int32_type__>(ms);
                    }
                    break;
                case 1002:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_PrepareDataForCommonUse_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 1003:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_PrepareDataForLogout_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 1015:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_CreateCharacter_ARG_int32_type_int32_serverId_bool_isGM__>(ms);
                    }
                    break;
                case 1016:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_DelectCharacter_ARG_int32_type__>(ms);
                    }
                    break;
                case 1030:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_SBGetAllOnlineCharacterInServer_ARG_int32_serverId__>(ms);
                    }
                    break;
                case 1031:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_GetLogicSimpleData_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 1032:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_CheckConnected_ARG_uint64_characterId__>(ms);
                    }
                    break;
                case 1033:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_CheckLost_ARG_uint64_characterId__>(ms);
                    }
                    break;
                case 1034:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_QueryStatus_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 1040:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_QueryBrokerStatus_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 1042:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_LogicGetEquipList_ARG_uint64_chararcterId__>(ms);
                    }
                    break;
                case 1043:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_LogicGetSkillData_ARG_uint64_chararcterId__>(ms);
                    }
                    break;
                case 1044:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_LogicGetTalentData_ARG_uint64_chararcterId__>(ms);
                    }
                    break;
                case 1045:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_LogicGetBookAttrData_ARG_uint64_chararcterId__>(ms);
                    }
                    break;
                case 1079:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_LogicKillMonster_ARG_int32_MonsterId_int32_addExp_int32_sceneId__>(ms);
                    }
                    break;
                case 1080:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_GiveItem_ARG_int32_itemId_int32_itemCount_int32_from__>(ms);
                    }
                    break;
                case 1081:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_DeleteItem_ARG_int32_itemId_int32_itemCount_int32_deleteType__>(ms);
                    }
                    break;
                case 1083:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_FirstOnline_ARG_uint64_clientId_uint64_chararcterId_int32_continuedLanding__>(ms);
                    }
                    break;
                case 1096:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_SSAddFriendById_ARG_uint64_characterId_int32_type__>(ms);
                    }
                    break;
                case 1099:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_ServerGMCommand_ARG_string_cmd_string_param__>(ms);
                    }
                    break;
                case 1100:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_CompleteFuben_ARG_FubenResult_result__>(ms);
                    }
                    break;
                case 1117:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_DurableDown_ARG_Dict_int_int_Data_bagidList__>(ms);
                    }
                    break;
                case 1119:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_NpcService_ARG_int32_serviceId__>(ms);
                    }
                    break;
                case 1134:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_PushDraw_ARG_int32_drawId__>(ms);
                    }
                    break;
                case 1135:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_CheckCharacterInFuben_ARG_int32_fubenId__>(ms);
                    }
                    break;
                case 1148:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_LogicP1vP1FightOver_ARG_uint64_characterId_int32_result_int32_rank__>(ms);
                    }
                    break;
                case 1149:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_LogicP1vP1LadderAdvance_ARG_int32_rank__>(ms);
                    }
                    break;
                case 1150:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_PushP1vP1LadderChange_ARG_int32_type_string_name_int32_result_int32_oldRank_int32_newRank__>(ms);
                    }
                    break;
                case 1164:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_SSGetFlagOrCondition_ARG_uint64_guid_int32_flagId_int32_conditionid__>(ms);
                    }
                    break;
                case 1165:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_SSIsShield_ARG_uint64_guid_uint64_Shield__>(ms);
                    }
                    break;
                case 1167:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_SSChangeExdata_ARG_Dict_int_int_Data_changes__>(ms);
                    }
                    break;
                case 1182:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_SSStoreOperationBuy_ARG_int64_storeId_uint64_Aid_string_name_int32_resType_int32_resCount_ItemBaseData_itemdata__>(ms);
                    }
                    break;
                case 1184:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_GetExchangeData_ARG_uint64_characterId__>(ms);
                    }
                    break;
                case 1190:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_SendMailToCharacterByItems_ARG_int32_mailId_ItemBaseData_item_StringArray_args__>(ms);
                    }
                    break;
                case 1191:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_AllianceDataChange_ARG_int32_type_int32_allianceId_int32_ladder_string_name__>(ms);
                    }
                    break;
                case 1192:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_SSBattleResult_ARG_int32_fubenId_int32_type__>(ms);
                    }
                    break;
                case 1202:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_ReadyToEnter_ARG_int32_placeholder__>(ms);
                    }
                    break;
                case 1209:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_SSGetFriendList_ARG_int32_typeId__>(ms);
                    }
                    break;
                case 1212:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_SSSendSimpleData_ARG_uint64_haveId_CharacterSimpleData_SimpleData__>(ms);
                    }
                    break;
                case 1213:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_SSFriendpPssiveChange_ARG_int32_type_uint64_characterId_int32_operate__>(ms);
                    }
                    break;
                case 1214:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_SSFetchExdata_ARG_Int32Array_idList__>(ms);
                    }
                    break;
                case 1215:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_SSDeleteItemByIndex_ARG_int32_bagId_int32_bagIndex_int32_itemCount__>(ms);
                    }
                    break;
                case 1219:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_SSGetMissionEnterScene_ARG_int32_missionId__>(ms);
                    }
                    break;
                case 1223:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_NotifyDungeonClose_ARG_int32_fubenId_Uint64Array_playerIds__>(ms);
                    }
                    break;
                case 1226:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_SSSetExdata_ARG_Dict_int_int_Data_changes__>(ms);
                    }
                    break;
                case 1227:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_SSSetFlag_ARG_Dict_int_int_Data_changes__>(ms);
                    }
                    break;
                case 1229:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_LogicGetTitleList_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 1230:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_RechargeSuccess_ARG_string_platform_int32_payType_float_price_string_orderId_string_channel__>(ms);
                    }
                    break;
                case 1232:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_GetItemCount_ARG_int32_itemId__>(ms);
                    }
                    break;
                case 1235:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_SSGetAllianceBuff_ARG_int32_placeholder__>(ms);
                    }
                    break;
                case 1236:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_NotifyAllianceWarInfo_ARG_AllianceWarInfo_info__>(ms);
                    }
                    break;
                case 1239:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_NotifyEnterFuben_ARG_int32_fubenId__>(ms);
                    }
                    break;
                case 1241:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_ChangeServer_ARG_int32_newServerId__>(ms);
                    }
                    break;
                case 1249:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_LogicGetElfData_ARG_uint64_chararcterId__>(ms);
                    }
                    break;
                case 1500:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_SBCleanClientCharacterData_ARG_uint64_clientId_uint64_characterId__>(ms);
                    }
                    break;
                case 1501:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_SSNotifyCharacterOnConnet_ARG_uint64_clientId_uint64_characterId__>(ms);
                    }
                    break;
                case 1504:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_SSRequestCityBuidlingPetData_ARG_uint64_guid__>(ms);
                    }
                    break;
                case 1506:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_SSGetTodayFunbenCount_ARG_int32_serverId_uint64_characterId_int32_selecttype__>(ms);
                    }
                    break;
                case 1509:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_LogicGetAnyData_ARG_uint64_chararcterId__>(ms);
                    }
                    break;
                case 1530:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_SSSyncCharacterFightPoint_ARG_int32_fp__>(ms);
                    }
                    break;
                case 1531:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_OnPlayerEnterSceneOver_ARG_int32_sceneId__>(ms);
                    }
                    break;
                case 1534:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_AnchorGift_ARG_int32_itemId_int32_count__>(ms);
                    }
                    break;
                case 1603:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_SSLearnSkill_ARG_int32_skillId_int32_skillLevel__>(ms);
                    }
                    break;
                case 1605:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_GMDeleteMessage_ARG_uint64_id__>(ms);
                    }
                    break;
                case 1990:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_GetCharacterData_ARG_uint64_id__>(ms);
                    }
                    break;
                case 1992:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_SendMailToCharacter_ARG_string_title_string_content_Dict_int_int_Data_items_int32_state__>(ms);
                    }
                    break;
                case 1993:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_SendMailToServer_ARG_uint32_serverId_uint64_mailId__>(ms);
                    }
                    break;
                case 1994:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_UpdateServer_ARG_int32_placeholder__>(ms);
                    }
                    break;
                case 1995:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_GMCommand_ARG_StringArray_commonds__>(ms);
                    }
                    break;
                case 1996:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_SendMailToCharacterById_ARG_int32_mailId_int32_createType_int32_type_Int32Array_countList__>(ms);
                    }
                    break;
                case 1325:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_LogicGetMountData_ARG_uint64_chararcterId__>(ms);
                    }
                    break;
                case 1329:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_CloneCharacterDbById_ARG_uint64_fromId_uint64_toId__>(ms);
                    }
                    break;
                case 1333:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_GiveItemList_ARG_Dict_int_int_Data_items_int32_from__>(ms);
                    }
                    break;
                case 1339:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_ApplyPlayerFlag_ARG_Int32Array_flagList__>(ms);
                    }
                    break;
                case 1342:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_ApplyMayaSkill_ARG_int32_id__>(ms);
                    }
                    break;
                case 1344:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_SSSendMailById_ARG_int32_tableId_int32_ExtendType_string_ExtendPara0_string_ExtendPara1__>(ms);
                    }
                    break;
                case 1346:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_NotifyPlayerMoniterData_ARG_MsgChatMoniterData_data__>(ms);
                    }
                    break;
                case 1347:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_GetPlayerMoniterData_ARG_uint64_characterId__>(ms);
                    }
                    break;
                case 1355:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Logic_NotifyInviteChallenge_ARG_uint64_invitorId_string_invitorName_int32_invitorServerId__>(ms);
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
        /// 同步任务数据
        /// </summary>
        public object SyncMission(ulong __characterId__, ulong __clientId__, int missionId, int state, int param)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1060;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_SyncMission_ARG_int32_missionId_int32_state_int32_param__();
            __data__.MissionId=missionId;
            __data__.State=state;
            __data__.Param=param;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 同步标记位
        /// </summary>
        public object SyncFlag(ulong __characterId__, ulong __clientId__, int flagId, int param)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1061;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_SyncFlag_ARG_int32_flagId_int32_param__();
            __data__.FlagId=flagId;
            __data__.Param=param;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 同步标记位
        /// </summary>
        public object SyncFlagList(ulong __characterId__, ulong __clientId__, Int32Array trueList, Int32Array falseList)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1062;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_SyncFlagList_ARG_Int32Array_trueList_Int32Array_falseList__();
            __data__.TrueList=trueList;
            __data__.FalseList=falseList;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 同步扩展数据
        /// </summary>
        public object SyncExdata(ulong __characterId__, ulong __clientId__, int exdataId, int value)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1063;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_SyncExdata_ARG_int32_exdataId_int32_value__();
            __data__.ExdataId=exdataId;
            __data__.Value=value;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 同步扩展数据
        /// </summary>
        public object SyncExdataList(ulong __characterId__, ulong __clientId__, Dict_int_int_Data diff)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1064;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_SyncExdataList_ARG_Dict_int_int_Data_diff__();
            __data__.Diff=diff;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 同步64位扩展数据
        /// </summary>
        public object SyncExdata64(ulong __characterId__, ulong __clientId__, int exdataId, long value)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1065;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_SyncExdata64_ARG_int32_exdataId_int64_value__();
            __data__.ExdataId=exdataId;
            __data__.Value=value;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 同步资源数据
        /// </summary>
        public object SyncResources(ulong __characterId__, ulong __clientId__, int resId, int value)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1066;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_SyncResources_ARG_int32_resId_int32_value__();
            __data__.ResId=resId;
            __data__.Value=value;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 同步包裹变化
        /// </summary>
        public object SyncItems(ulong __characterId__, ulong __clientId__, BagsChangeData bag)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1067;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_SyncItems_ARG_BagsChangeData_bag__();
            __data__.Bag=bag;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 同步任务变化
        /// </summary>
        public object SyncMissions(ulong __characterId__, ulong __clientId__, MissionDataMessage missions)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1082;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_SyncMissions_ARG_MissionDataMessage_missions__();
            __data__.Missions=missions;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 成就完成提示
        /// </summary>
        public object FinishAchievement(ulong __characterId__, ulong __clientId__, int achievementId)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1087;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_FinishAchievement_ARG_int32_achievementId__();
            __data__.AchievementId=achievementId;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// </summary>
        public object SeekCharactersReceive(ulong __characterId__, ulong __clientId__, CharacterSimpleDataList result)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1393;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_SeekCharactersReceive_ARG_CharacterSimpleDataList_result__();
            __data__.Result=result;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// </summary>
        public object SeekFriendsReceive(ulong __characterId__, ulong __clientId__, CharacterSimpleDataList result)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1394;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_SeekFriendsReceive_ARG_CharacterSimpleDataList_result__();
            __data__.Result=result;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 副本结束，通知结果
        /// </summary>
        public object DungeonComplete(ulong __characterId__, ulong __clientId__, FubenResult result)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1101;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_DungeonComplete_ARG_FubenResult_result__();
            __data__.Result=result;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 主动更新图鉴组激活状态
        /// </summary>
        public object ActivateBookGroup(ulong __characterId__, ulong __clientId__, int groupId)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1319;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_ActivateBookGroup_ARG_int32_groupId__();
            __data__.GroupId=groupId;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 主动更新邮件
        /// </summary>
        public object SyncMails(ulong __characterId__, ulong __clientId__, MailList mails)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1116;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_SyncMails_ARG_MailList_mails__();
            __data__.Mails=mails;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 同步家园建筑数据
        /// </summary>
        public object SyncCityBuildingData(ulong __characterId__, ulong __clientId__, BuildingList data)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1126;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_SyncCityBuildingData_ARG_BuildingList_data__();
            __data__.Data=data;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 同步随从任务数据
        /// </summary>
        public object SyncPetMission(ulong __characterId__, ulong __clientId__, PetMissionList msg)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1127;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_SyncPetMission_ARG_PetMissionList_msg__();
            __data__.Msg=msg;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 有装备损坏
        /// </summary>
        public object EquipDurableBroken(ulong __characterId__, ulong __clientId__, int partId, int value)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1129;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_EquipDurableBroken_ARG_int32_partId_int32_value__();
            __data__.PartId=partId;
            __data__.Value=value;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 装备耐久第一次变化，希望客户端可以下次开界面时请求耐久
        /// </summary>
        public object EquipDurableChange(ulong __characterId__, ulong __clientId__, int placeholder)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1130;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_EquipDurableChange_ARG_int32_placeholder__();
            __data__.Placeholder=placeholder;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 删除宠物任务
        /// </summary>
        public object DeletePetMission(ulong __characterId__, ulong __clientId__, int missionId)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1144;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_DeletePetMission_ARG_int32_missionId__();
            __data__.MissionId=missionId;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 天梯的奖励界面
        /// </summary>
        public object LogicP1vP1FightResult(ulong __characterId__, ulong __clientId__, P1vP1RewardData data)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1152;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_LogicP1vP1FightResult_ARG_P1vP1RewardData_data__();
            __data__.Data=data;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 战盟信息通知 type：0=name1邀请您加入name2的战盟
        /// </summary>
        public object LogicSyncAllianceMessage(ulong __characterId__, ulong __clientId__, int type, string name1, int allianceId, string name2)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1163;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_LogicSyncAllianceMessage_ARG_int32_type_string_name1_int32_allianceId_string_name2__();
            __data__.Type=type;
            __data__.Name1=name1;
            __data__.AllianceId=allianceId;
            __data__.Name2=name2;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 精灵的抽奖结果
        /// </summary>
        public object ElfDrawOver(ulong __characterId__, ulong __clientId__, DrawItemResult Items, long getTime)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1170;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_ElfDrawOver_ARG_DrawItemResult_Items_int64_getTime__();
            __data__.Items=Items;
            __data__.GetTime=getTime;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 增加天赋数量变化的通知界面
        /// </summary>
        public object TalentCountChange(ulong __characterId__, ulong __clientId__, int talentId, int value)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1171;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_TalentCountChange_ARG_int32_talentId_int32_value__();
            __data__.TalentId=talentId;
            __data__.Value=value;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知有东西被买了
        /// </summary>
        public object NotifyStoreBuyed(ulong __characterId__, ulong __clientId__, long storeId, ulong Aid, string Aname)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1186;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_NotifyStoreBuyed_ARG_int64_storeId_uint64_Aid_string_Aname__();
            __data__.StoreId=storeId;
            __data__.Aid=Aid;
            __data__.Aname=Aname;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 占星台抽奖结果
        /// </summary>
        public object AstrologyDrawOver(ulong __characterId__, ulong __clientId__, DrawItemResult Items, long getTime)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1197;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_AstrologyDrawOver_ARG_DrawItemResult_Items_int64_getTime__();
            __data__.Items=Items;
            __data__.GetTime=getTime;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 额外增加了仇人
        /// </summary>
        public object SyncAddFriend(ulong __characterId__, ulong __clientId__, int type, CharacterSimpleData character)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1198;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_SyncAddFriend_ARG_int32_type_CharacterSimpleData_character__();
            __data__.Type=type;
            __data__.Character=character;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知一些消息 type:0提示字典
        /// </summary>
        public object LogicNotifyMessage(ulong __characterId__, ulong __clientId__, int type, string info, int addChat)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1203;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_LogicNotifyMessage_ARG_int32_type_string_info_int32_addChat__();
            __data__.Type=type;
            __data__.Info=info;
            __data__.AddChat=addChat;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知客户端获得经验
        /// </summary>
        public object NotifGainRes(ulong __characterId__, ulong __clientId__, DataChangeList changes)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1204;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_NotifGainRes_ARG_DataChangeList_changes__();
            __data__.Changes=changes;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知战场的结果界面信息
        /// </summary>
        public object BattleResult(ulong __characterId__, ulong __clientId__, int dungeonId, int resultType, int first)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1205;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_BattleResult_ARG_int32_dungeonId_int32_resultType_int32_first__();
            __data__.DungeonId=dungeonId;
            __data__.ResultType=resultType;
            __data__.First=first;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 自己被打了
        /// </summary>
        public object NotifyP1vP1Change(ulong __characterId__, ulong __clientId__, P1vP1Change_One one)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1208;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_NotifyP1vP1Change_ARG_P1vP1Change_One_one__();
            __data__.One=one;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知好友数据变化
        /// </summary>
        public object SyncFriendDataChange(ulong __characterId__, ulong __clientId__, CharacterSimpleDataList Changes)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1210;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_SyncFriendDataChange_ARG_CharacterSimpleDataList_Changes__();
            __data__.Changes=Changes;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知好友删除
        /// </summary>
        public object SyncFriendDelete(ulong __characterId__, ulong __clientId__, int type, ulong characterId)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1211;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_SyncFriendDelete_ARG_int32_type_uint64_characterId__();
            __data__.Type=type;
            __data__.CharacterId=characterId;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知客户端，充值成功
        /// </summary>
        public object NotifyRechargeSuccess(ulong __characterId__, ulong __clientId__, int rechargeId)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1231;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_NotifyRechargeSuccess_ARG_int32_rechargeId__();
            __data__.RechargeId=rechargeId;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 同步运营活动内容
        /// </summary>
        public object SyncOperationActivityItem(ulong __characterId__, ulong __clientId__, MsgOperActivtyItemList items)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1514;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_SyncOperationActivityItem_ARG_MsgOperActivtyItemList_items__();
            __data__.Items=items;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 同步运营活动内容
        /// </summary>
        public object SyncOperationActivityTerm(ulong __characterId__, ulong __clientId__, int id, int param)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1532;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_SyncOperationActivityTerm_ARG_int32_id_int32_param__();
            __data__.Id=id;
            __data__.Param=param;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// </summary>
        public object SendMountData(ulong __characterId__, ulong __clientId__, MountData data)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1327;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_SendMountData_ARG_MountData_data__();
            __data__.Data=data;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知记录角色结果
        /// </summary>
        public object NotifySnapShotResult(ulong __characterId__, ulong __clientId__, int state)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1328;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_NotifySnapShotResult_ARG_int32_state__();
            __data__.State=state;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 增加技能点
        /// </summary>
        public object AddSkillPoint(ulong __characterId__, ulong __clientId__, int skillId, int point)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1331;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_AddSkillPoint_ARG_int32_skillId_int32_point__();
            __data__.SkillId=skillId;
            __data__.Point=point;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 收到角斗申请
        /// </summary>
        public object ReceiveChallenge(ulong __characterId__, ulong __clientId__, ulong characterId, string name, string server)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 1353;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Logic;

            var __data__ = new __RPC_Logic_ReceiveChallenge_ARG_uint64_characterId_string_name_string_server__();
            __data__.CharacterId=characterId;
            __data__.Name=name;
            __data__.Server=server;


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
        public PrepareDataForEnterGameOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, ulong sceneGuid)
            : base(sender, ServiceType.Logic, 1000, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_PrepareDataForEnterGame_ARG_int32_serverId_uint64_sceneGuid__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.SceneGuid=sceneGuid;

        }

        public __RPC_Logic_PrepareDataForEnterGame_ARG_int32_serverId_uint64_sceneGuid__ Request { get; private set; }

            private __RPC_Logic_PrepareDataForEnterGame_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_PrepareDataForEnterGame_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class PrepareDataForCreateCharacterOutMessage : OutMessage
    {
        public PrepareDataForCreateCharacterOutMessage(ClientAgentBase sender, ulong __characterId__, int type)
            : base(sender, ServiceType.Logic, 1001, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_PrepareDataForCreateCharacter_ARG_int32_type__();
            mMessage.CharacterId = __characterId__;
            Request.Type=type;

        }

        public __RPC_Logic_PrepareDataForCreateCharacter_ARG_int32_type__ Request { get; private set; }

            private __RPC_Logic_PrepareDataForCreateCharacter_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_PrepareDataForCreateCharacter_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class PrepareDataForCommonUseOutMessage : OutMessage
    {
        public PrepareDataForCommonUseOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Logic, 1002, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_PrepareDataForCommonUse_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Logic_PrepareDataForCommonUse_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Logic_PrepareDataForCommonUse_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_PrepareDataForCommonUse_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class PrepareDataForLogoutOutMessage : OutMessage
    {
        public PrepareDataForLogoutOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Logic, 1003, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_PrepareDataForLogout_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Logic_PrepareDataForLogout_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Logic_PrepareDataForLogout_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_PrepareDataForLogout_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class CreateCharacterOutMessage : OutMessage
    {
        public CreateCharacterOutMessage(ClientAgentBase sender, ulong __characterId__, int type, int serverId, bool isGM)
            : base(sender, ServiceType.Logic, 1015, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_CreateCharacter_ARG_int32_type_int32_serverId_bool_isGM__();
            mMessage.CharacterId = __characterId__;
            Request.Type=type;
            Request.ServerId=serverId;
            Request.IsGM=isGM;

        }

        public __RPC_Logic_CreateCharacter_ARG_int32_type_int32_serverId_bool_isGM__ Request { get; private set; }

            private __RPC_Logic_CreateCharacter_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_CreateCharacter_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class DelectCharacterOutMessage : OutMessage
    {
        public DelectCharacterOutMessage(ClientAgentBase sender, ulong __characterId__, int type)
            : base(sender, ServiceType.Logic, 1016, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_DelectCharacter_ARG_int32_type__();
            mMessage.CharacterId = __characterId__;
            Request.Type=type;

        }

        public __RPC_Logic_DelectCharacter_ARG_int32_type__ Request { get; private set; }

            private __RPC_Logic_DelectCharacter_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_DelectCharacter_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SBGetAllOnlineCharacterInServerOutMessage : OutMessage
    {
        public SBGetAllOnlineCharacterInServerOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId)
            : base(sender, ServiceType.Logic, 1030, (int)MessageType.SB)
        {
            Request = new __RPC_Logic_SBGetAllOnlineCharacterInServer_ARG_int32_serverId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;

        }

        public __RPC_Logic_SBGetAllOnlineCharacterInServer_ARG_int32_serverId__ Request { get; private set; }

            private __RPC_Logic_SBGetAllOnlineCharacterInServer_RET_Uint64Array__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_SBGetAllOnlineCharacterInServer_RET_Uint64Array__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetLogicSimpleDataOutMessage : OutMessage
    {
        public GetLogicSimpleDataOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Logic, 1031, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_GetLogicSimpleData_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Logic_GetLogicSimpleData_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Logic_GetLogicSimpleData_RET_LogicSimpleData__ mResponse;
            public LogicSimpleData Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Logic_GetLogicSimpleData_RET_LogicSimpleData__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class CheckConnectedOutMessage : OutMessage
    {
        public CheckConnectedOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId)
            : base(sender, ServiceType.Logic, 1032, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_CheckConnected_ARG_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;

        }

        public __RPC_Logic_CheckConnected_ARG_uint64_characterId__ Request { get; private set; }

            private __RPC_Logic_CheckConnected_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_CheckConnected_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class CheckLostOutMessage : OutMessage
    {
        public CheckLostOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId)
            : base(sender, ServiceType.Logic, 1033, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_CheckLost_ARG_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;

        }

        public __RPC_Logic_CheckLost_ARG_uint64_characterId__ Request { get; private set; }

            private __RPC_Logic_CheckLost_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_CheckLost_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class QueryStatusOutMessage : OutMessage
    {
        public QueryStatusOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Logic, 1034, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_QueryStatus_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Logic_QueryStatus_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Logic_QueryStatus_RET_LogicServerStatus__ mResponse;
            public LogicServerStatus Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Logic_QueryStatus_RET_LogicServerStatus__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class QueryBrokerStatusOutMessage : OutMessage
    {
        public QueryBrokerStatusOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Logic, 1040, (int)MessageType.SB)
        {
            Request = new __RPC_Logic_QueryBrokerStatus_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Logic_QueryBrokerStatus_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Logic_QueryBrokerStatus_RET_CommonBrokerStatus__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_QueryBrokerStatus_RET_CommonBrokerStatus__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class LogicGetEquipListOutMessage : OutMessage
    {
        public LogicGetEquipListOutMessage(ClientAgentBase sender, ulong __characterId__, ulong chararcterId)
            : base(sender, ServiceType.Logic, 1042, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_LogicGetEquipList_ARG_uint64_chararcterId__();
            mMessage.CharacterId = __characterId__;
            Request.ChararcterId=chararcterId;

        }

        public __RPC_Logic_LogicGetEquipList_ARG_uint64_chararcterId__ Request { get; private set; }

            private __RPC_Logic_LogicGetEquipList_RET_BagBaseData__ mResponse;
            public BagBaseData Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Logic_LogicGetEquipList_RET_BagBaseData__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class LogicGetSkillDataOutMessage : OutMessage
    {
        public LogicGetSkillDataOutMessage(ClientAgentBase sender, ulong __characterId__, ulong chararcterId)
            : base(sender, ServiceType.Logic, 1043, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_LogicGetSkillData_ARG_uint64_chararcterId__();
            mMessage.CharacterId = __characterId__;
            Request.ChararcterId=chararcterId;

        }

        public __RPC_Logic_LogicGetSkillData_ARG_uint64_chararcterId__ Request { get; private set; }

            private __RPC_Logic_LogicGetSkillData_RET_Dict_int_int_Data__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_LogicGetSkillData_RET_Dict_int_int_Data__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class LogicGetTalentDataOutMessage : OutMessage
    {
        public LogicGetTalentDataOutMessage(ClientAgentBase sender, ulong __characterId__, ulong chararcterId)
            : base(sender, ServiceType.Logic, 1044, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_LogicGetTalentData_ARG_uint64_chararcterId__();
            mMessage.CharacterId = __characterId__;
            Request.ChararcterId=chararcterId;

        }

        public __RPC_Logic_LogicGetTalentData_ARG_uint64_chararcterId__ Request { get; private set; }

            private __RPC_Logic_LogicGetTalentData_RET_Dict_int_int_Data__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_LogicGetTalentData_RET_Dict_int_int_Data__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class LogicGetBookAttrDataOutMessage : OutMessage
    {
        public LogicGetBookAttrDataOutMessage(ClientAgentBase sender, ulong __characterId__, ulong chararcterId)
            : base(sender, ServiceType.Logic, 1045, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_LogicGetBookAttrData_ARG_uint64_chararcterId__();
            mMessage.CharacterId = __characterId__;
            Request.ChararcterId=chararcterId;

        }

        public __RPC_Logic_LogicGetBookAttrData_ARG_uint64_chararcterId__ Request { get; private set; }

            private __RPC_Logic_LogicGetBookAttrData_RET_BookAttrList__ mResponse;
            public BookAttrList Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Logic_LogicGetBookAttrData_RET_BookAttrList__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class LogicKillMonsterOutMessage : OutMessage
    {
        public LogicKillMonsterOutMessage(ClientAgentBase sender, ulong __characterId__, int MonsterId, int addExp, int sceneId)
            : base(sender, ServiceType.Logic, 1079, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_LogicKillMonster_ARG_int32_MonsterId_int32_addExp_int32_sceneId__();
            mMessage.CharacterId = __characterId__;
            Request.MonsterId=MonsterId;
            Request.AddExp=addExp;
            Request.SceneId=sceneId;

        }

        public __RPC_Logic_LogicKillMonster_ARG_int32_MonsterId_int32_addExp_int32_sceneId__ Request { get; private set; }


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

    public class GiveItemOutMessage : OutMessage
    {
        public GiveItemOutMessage(ClientAgentBase sender, ulong __characterId__, int itemId, int itemCount, int from)
            : base(sender, ServiceType.Logic, 1080, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_GiveItem_ARG_int32_itemId_int32_itemCount_int32_from__();
            mMessage.CharacterId = __characterId__;
            Request.ItemId=itemId;
            Request.ItemCount=itemCount;
            Request.From=from;

        }

        public __RPC_Logic_GiveItem_ARG_int32_itemId_int32_itemCount_int32_from__ Request { get; private set; }

            private __RPC_Logic_GiveItem_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_GiveItem_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class DeleteItemOutMessage : OutMessage
    {
        public DeleteItemOutMessage(ClientAgentBase sender, ulong __characterId__, int itemId, int itemCount, int deleteType)
            : base(sender, ServiceType.Logic, 1081, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_DeleteItem_ARG_int32_itemId_int32_itemCount_int32_deleteType__();
            mMessage.CharacterId = __characterId__;
            Request.ItemId=itemId;
            Request.ItemCount=itemCount;
            Request.DeleteType=deleteType;

        }

        public __RPC_Logic_DeleteItem_ARG_int32_itemId_int32_itemCount_int32_deleteType__ Request { get; private set; }

            private __RPC_Logic_DeleteItem_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_DeleteItem_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class FirstOnlineOutMessage : OutMessage
    {
        public FirstOnlineOutMessage(ClientAgentBase sender, ulong __characterId__, ulong clientId, ulong chararcterId, int continuedLanding)
            : base(sender, ServiceType.Logic, 1083, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_FirstOnline_ARG_uint64_clientId_uint64_chararcterId_int32_continuedLanding__();
            mMessage.CharacterId = __characterId__;
            Request.ClientId=clientId;
            Request.ChararcterId=chararcterId;
            Request.ContinuedLanding=continuedLanding;

        }

        public __RPC_Logic_FirstOnline_ARG_uint64_clientId_uint64_chararcterId_int32_continuedLanding__ Request { get; private set; }

            private __RPC_Logic_FirstOnline_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_FirstOnline_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSAddFriendByIdOutMessage : OutMessage
    {
        public SSAddFriendByIdOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId, int type)
            : base(sender, ServiceType.Logic, 1096, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_SSAddFriendById_ARG_uint64_characterId_int32_type__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;
            Request.Type=type;

        }

        public __RPC_Logic_SSAddFriendById_ARG_uint64_characterId_int32_type__ Request { get; private set; }


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
            : base(sender, ServiceType.Logic, 1099, (int)MessageType.SAS)
        {
            Request = new __RPC_Logic_ServerGMCommand_ARG_string_cmd_string_param__();
            mMessage.CharacterId = __characterId__;
            Request.Cmd=cmd;
            Request.Param=param;

        }

        public __RPC_Logic_ServerGMCommand_ARG_string_cmd_string_param__ Request { get; private set; }


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

    public class CompleteFubenOutMessage : OutMessage
    {
        public CompleteFubenOutMessage(ClientAgentBase sender, ulong __characterId__, FubenResult result)
            : base(sender, ServiceType.Logic, 1100, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_CompleteFuben_ARG_FubenResult_result__();
            mMessage.CharacterId = __characterId__;
            Request.Result=result;

        }

        public __RPC_Logic_CompleteFuben_ARG_FubenResult_result__ Request { get; private set; }


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

    public class DurableDownOutMessage : OutMessage
    {
        public DurableDownOutMessage(ClientAgentBase sender, ulong __characterId__, Dict_int_int_Data bagidList)
            : base(sender, ServiceType.Logic, 1117, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_DurableDown_ARG_Dict_int_int_Data_bagidList__();
            mMessage.CharacterId = __characterId__;
            Request.BagidList=bagidList;

        }

        public __RPC_Logic_DurableDown_ARG_Dict_int_int_Data_bagidList__ Request { get; private set; }


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

    public class NpcServiceOutMessage : OutMessage
    {
        public NpcServiceOutMessage(ClientAgentBase sender, ulong __characterId__, int serviceId)
            : base(sender, ServiceType.Logic, 1119, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_NpcService_ARG_int32_serviceId__();
            mMessage.CharacterId = __characterId__;
            Request.ServiceId=serviceId;

        }

        public __RPC_Logic_NpcService_ARG_int32_serviceId__ Request { get; private set; }

            private __RPC_Logic_NpcService_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_NpcService_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class PushDrawOutMessage : OutMessage
    {
        public PushDrawOutMessage(ClientAgentBase sender, ulong __characterId__, int drawId)
            : base(sender, ServiceType.Logic, 1134, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_PushDraw_ARG_int32_drawId__();
            mMessage.CharacterId = __characterId__;
            Request.DrawId=drawId;

        }

        public __RPC_Logic_PushDraw_ARG_int32_drawId__ Request { get; private set; }


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

    public class CheckCharacterInFubenOutMessage : OutMessage
    {
        public CheckCharacterInFubenOutMessage(ClientAgentBase sender, ulong __characterId__, int fubenId)
            : base(sender, ServiceType.Logic, 1135, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_CheckCharacterInFuben_ARG_int32_fubenId__();
            mMessage.CharacterId = __characterId__;
            Request.FubenId=fubenId;

        }

        public __RPC_Logic_CheckCharacterInFuben_ARG_int32_fubenId__ Request { get; private set; }

            private __RPC_Logic_CheckCharacterInFuben_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_CheckCharacterInFuben_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class LogicP1vP1FightOverOutMessage : OutMessage
    {
        public LogicP1vP1FightOverOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId, int result, int rank)
            : base(sender, ServiceType.Logic, 1148, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_LogicP1vP1FightOver_ARG_uint64_characterId_int32_result_int32_rank__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;
            Request.Result=result;
            Request.Rank=rank;

        }

        public __RPC_Logic_LogicP1vP1FightOver_ARG_uint64_characterId_int32_result_int32_rank__ Request { get; private set; }


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

    public class LogicP1vP1LadderAdvanceOutMessage : OutMessage
    {
        public LogicP1vP1LadderAdvanceOutMessage(ClientAgentBase sender, ulong __characterId__, int rank)
            : base(sender, ServiceType.Logic, 1149, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_LogicP1vP1LadderAdvance_ARG_int32_rank__();
            mMessage.CharacterId = __characterId__;
            Request.Rank=rank;

        }

        public __RPC_Logic_LogicP1vP1LadderAdvance_ARG_int32_rank__ Request { get; private set; }


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

    public class PushP1vP1LadderChangeOutMessage : OutMessage
    {
        public PushP1vP1LadderChangeOutMessage(ClientAgentBase sender, ulong __characterId__, int type, string name, int result, int oldRank, int newRank)
            : base(sender, ServiceType.Logic, 1150, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_PushP1vP1LadderChange_ARG_int32_type_string_name_int32_result_int32_oldRank_int32_newRank__();
            mMessage.CharacterId = __characterId__;
            Request.Type=type;
            Request.Name=name;
            Request.Result=result;
            Request.OldRank=oldRank;
            Request.NewRank=newRank;

        }

        public __RPC_Logic_PushP1vP1LadderChange_ARG_int32_type_string_name_int32_result_int32_oldRank_int32_newRank__ Request { get; private set; }


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

    public class SSGetFlagOrConditionOutMessage : OutMessage
    {
        public SSGetFlagOrConditionOutMessage(ClientAgentBase sender, ulong __characterId__, ulong guid, int flagId, int conditionid)
            : base(sender, ServiceType.Logic, 1164, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_SSGetFlagOrCondition_ARG_uint64_guid_int32_flagId_int32_conditionid__();
            mMessage.CharacterId = __characterId__;
            Request.Guid=guid;
            Request.FlagId=flagId;
            Request.Conditionid=conditionid;

        }

        public __RPC_Logic_SSGetFlagOrCondition_ARG_uint64_guid_int32_flagId_int32_conditionid__ Request { get; private set; }

            private __RPC_Logic_SSGetFlagOrCondition_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_SSGetFlagOrCondition_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSIsShieldOutMessage : OutMessage
    {
        public SSIsShieldOutMessage(ClientAgentBase sender, ulong __characterId__, ulong guid, ulong Shield)
            : base(sender, ServiceType.Logic, 1165, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_SSIsShield_ARG_uint64_guid_uint64_Shield__();
            mMessage.CharacterId = __characterId__;
            Request.Guid=guid;
            Request.Shield=Shield;

        }

        public __RPC_Logic_SSIsShield_ARG_uint64_guid_uint64_Shield__ Request { get; private set; }

            private __RPC_Logic_SSIsShield_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_SSIsShield_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSChangeExdataOutMessage : OutMessage
    {
        public SSChangeExdataOutMessage(ClientAgentBase sender, ulong __characterId__, Dict_int_int_Data changes)
            : base(sender, ServiceType.Logic, 1167, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_SSChangeExdata_ARG_Dict_int_int_Data_changes__();
            mMessage.CharacterId = __characterId__;
            Request.Changes=changes;

        }

        public __RPC_Logic_SSChangeExdata_ARG_Dict_int_int_Data_changes__ Request { get; private set; }


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

    public class SSStoreOperationBuyOutMessage : OutMessage
    {
        public SSStoreOperationBuyOutMessage(ClientAgentBase sender, ulong __characterId__, long storeId, ulong Aid, string name, int resType, int resCount, ItemBaseData itemdata)
            : base(sender, ServiceType.Logic, 1182, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_SSStoreOperationBuy_ARG_int64_storeId_uint64_Aid_string_name_int32_resType_int32_resCount_ItemBaseData_itemdata__();
            mMessage.CharacterId = __characterId__;
            Request.StoreId=storeId;
            Request.Aid=Aid;
            Request.Name=name;
            Request.ResType=resType;
            Request.ResCount=resCount;
            Request.Itemdata=itemdata;

        }

        public __RPC_Logic_SSStoreOperationBuy_ARG_int64_storeId_uint64_Aid_string_name_int32_resType_int32_resCount_ItemBaseData_itemdata__ Request { get; private set; }

            private __RPC_Logic_SSStoreOperationBuy_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_SSStoreOperationBuy_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetExchangeDataOutMessage : OutMessage
    {
        public GetExchangeDataOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId)
            : base(sender, ServiceType.Logic, 1184, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_GetExchangeData_ARG_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;

        }

        public __RPC_Logic_GetExchangeData_ARG_uint64_characterId__ Request { get; private set; }

            private __RPC_Logic_GetExchangeData_RET_OtherStoreList__ mResponse;
            public OtherStoreList Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Logic_GetExchangeData_RET_OtherStoreList__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SendMailToCharacterByItemsOutMessage : OutMessage
    {
        public SendMailToCharacterByItemsOutMessage(ClientAgentBase sender, ulong __characterId__, int mailId, ItemBaseData item, StringArray args)
            : base(sender, ServiceType.Logic, 1190, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_SendMailToCharacterByItems_ARG_int32_mailId_ItemBaseData_item_StringArray_args__();
            mMessage.CharacterId = __characterId__;
            Request.MailId=mailId;
            Request.Item=item;
            Request.Args=args;

        }

        public __RPC_Logic_SendMailToCharacterByItems_ARG_int32_mailId_ItemBaseData_item_StringArray_args__ Request { get; private set; }


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

    public class AllianceDataChangeOutMessage : OutMessage
    {
        public AllianceDataChangeOutMessage(ClientAgentBase sender, ulong __characterId__, int type, int allianceId, int ladder, string name)
            : base(sender, ServiceType.Logic, 1191, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_AllianceDataChange_ARG_int32_type_int32_allianceId_int32_ladder_string_name__();
            mMessage.CharacterId = __characterId__;
            Request.Type=type;
            Request.AllianceId=allianceId;
            Request.Ladder=ladder;
            Request.Name=name;

        }

        public __RPC_Logic_AllianceDataChange_ARG_int32_type_int32_allianceId_int32_ladder_string_name__ Request { get; private set; }


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

    public class SSBattleResultOutMessage : OutMessage
    {
        public SSBattleResultOutMessage(ClientAgentBase sender, ulong __characterId__, int fubenId, int type)
            : base(sender, ServiceType.Logic, 1192, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_SSBattleResult_ARG_int32_fubenId_int32_type__();
            mMessage.CharacterId = __characterId__;
            Request.FubenId=fubenId;
            Request.Type=type;

        }

        public __RPC_Logic_SSBattleResult_ARG_int32_fubenId_int32_type__ Request { get; private set; }

            private __RPC_Logic_SSBattleResult_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_SSBattleResult_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class ReadyToEnterOutMessage : OutMessage
    {
        public ReadyToEnterOutMessage(ClientAgentBase sender, ulong __characterId__, int placeholder)
            : base(sender, ServiceType.Logic, 1202, (int)MessageType.SAS)
        {
            Request = new __RPC_Logic_ReadyToEnter_ARG_int32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Logic_ReadyToEnter_ARG_int32_placeholder__ Request { get; private set; }

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
                Response.Add(Serializer.Deserialize<__RPC_Logic_ReadyToEnter_RET_int32__>(ms).ReturnValue);
            }
            State = MessageState.Reply;
            if(ErrorCode == 0) ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSGetFriendListOutMessage : OutMessage
    {
        public SSGetFriendListOutMessage(ClientAgentBase sender, ulong __characterId__, int typeId)
            : base(sender, ServiceType.Logic, 1209, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_SSGetFriendList_ARG_int32_typeId__();
            mMessage.CharacterId = __characterId__;
            Request.TypeId=typeId;

        }

        public __RPC_Logic_SSGetFriendList_ARG_int32_typeId__ Request { get; private set; }

            private __RPC_Logic_SSGetFriendList_RET_DictIntUint64Array__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_SSGetFriendList_RET_DictIntUint64Array__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSSendSimpleDataOutMessage : OutMessage
    {
        public SSSendSimpleDataOutMessage(ClientAgentBase sender, ulong __characterId__, ulong haveId, CharacterSimpleData SimpleData)
            : base(sender, ServiceType.Logic, 1212, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_SSSendSimpleData_ARG_uint64_haveId_CharacterSimpleData_SimpleData__();
            mMessage.CharacterId = __characterId__;
            Request.HaveId=haveId;
            Request.SimpleData=SimpleData;

        }

        public __RPC_Logic_SSSendSimpleData_ARG_uint64_haveId_CharacterSimpleData_SimpleData__ Request { get; private set; }


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

    public class SSFriendpPssiveChangeOutMessage : OutMessage
    {
        public SSFriendpPssiveChangeOutMessage(ClientAgentBase sender, ulong __characterId__, int type, ulong characterId, int operate)
            : base(sender, ServiceType.Logic, 1213, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_SSFriendpPssiveChange_ARG_int32_type_uint64_characterId_int32_operate__();
            mMessage.CharacterId = __characterId__;
            Request.Type=type;
            Request.CharacterId=characterId;
            Request.Operate=operate;

        }

        public __RPC_Logic_SSFriendpPssiveChange_ARG_int32_type_uint64_characterId_int32_operate__ Request { get; private set; }


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

    public class SSFetchExdataOutMessage : OutMessage
    {
        public SSFetchExdataOutMessage(ClientAgentBase sender, ulong __characterId__, Int32Array idList)
            : base(sender, ServiceType.Logic, 1214, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_SSFetchExdata_ARG_Int32Array_idList__();
            mMessage.CharacterId = __characterId__;
            Request.IdList=idList;

        }

        public __RPC_Logic_SSFetchExdata_ARG_Int32Array_idList__ Request { get; private set; }

            private __RPC_Logic_SSFetchExdata_RET_Int32Array__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_SSFetchExdata_RET_Int32Array__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSDeleteItemByIndexOutMessage : OutMessage
    {
        public SSDeleteItemByIndexOutMessage(ClientAgentBase sender, ulong __characterId__, int bagId, int bagIndex, int itemCount)
            : base(sender, ServiceType.Logic, 1215, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_SSDeleteItemByIndex_ARG_int32_bagId_int32_bagIndex_int32_itemCount__();
            mMessage.CharacterId = __characterId__;
            Request.BagId=bagId;
            Request.BagIndex=bagIndex;
            Request.ItemCount=itemCount;

        }

        public __RPC_Logic_SSDeleteItemByIndex_ARG_int32_bagId_int32_bagIndex_int32_itemCount__ Request { get; private set; }

            private __RPC_Logic_SSDeleteItemByIndex_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_SSDeleteItemByIndex_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSGetMissionEnterSceneOutMessage : OutMessage
    {
        public SSGetMissionEnterSceneOutMessage(ClientAgentBase sender, ulong __characterId__, int missionId)
            : base(sender, ServiceType.Logic, 1219, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_SSGetMissionEnterScene_ARG_int32_missionId__();
            mMessage.CharacterId = __characterId__;
            Request.MissionId=missionId;

        }

        public __RPC_Logic_SSGetMissionEnterScene_ARG_int32_missionId__ Request { get; private set; }

            private __RPC_Logic_SSGetMissionEnterScene_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_SSGetMissionEnterScene_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class NotifyDungeonCloseOutMessage : OutMessage
    {
        public NotifyDungeonCloseOutMessage(ClientAgentBase sender, ulong __characterId__, int fubenId, Uint64Array playerIds)
            : base(sender, ServiceType.Logic, 1223, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_NotifyDungeonClose_ARG_int32_fubenId_Uint64Array_playerIds__();
            mMessage.CharacterId = __characterId__;
            Request.FubenId=fubenId;
            Request.PlayerIds=playerIds;

        }

        public __RPC_Logic_NotifyDungeonClose_ARG_int32_fubenId_Uint64Array_playerIds__ Request { get; private set; }


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

    public class SSSetExdataOutMessage : OutMessage
    {
        public SSSetExdataOutMessage(ClientAgentBase sender, ulong __characterId__, Dict_int_int_Data changes)
            : base(sender, ServiceType.Logic, 1226, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_SSSetExdata_ARG_Dict_int_int_Data_changes__();
            mMessage.CharacterId = __characterId__;
            Request.Changes=changes;

        }

        public __RPC_Logic_SSSetExdata_ARG_Dict_int_int_Data_changes__ Request { get; private set; }

            private __RPC_Logic_SSSetExdata_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_SSSetExdata_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSSetFlagOutMessage : OutMessage
    {
        public SSSetFlagOutMessage(ClientAgentBase sender, ulong __characterId__, Dict_int_int_Data changes)
            : base(sender, ServiceType.Logic, 1227, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_SSSetFlag_ARG_Dict_int_int_Data_changes__();
            mMessage.CharacterId = __characterId__;
            Request.Changes=changes;

        }

        public __RPC_Logic_SSSetFlag_ARG_Dict_int_int_Data_changes__ Request { get; private set; }

            private __RPC_Logic_SSSetFlag_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_SSSetFlag_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class LogicGetTitleListOutMessage : OutMessage
    {
        public LogicGetTitleListOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Logic, 1229, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_LogicGetTitleList_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Logic_LogicGetTitleList_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Logic_LogicGetTitleList_RET_LogicTitleData__ mResponse;
            public LogicTitleData Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Logic_LogicGetTitleList_RET_LogicTitleData__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class RechargeSuccessOutMessage : OutMessage
    {
        public RechargeSuccessOutMessage(ClientAgentBase sender, ulong __characterId__, string platform, int payType, float price, string orderId, string channel)
            : base(sender, ServiceType.Logic, 1230, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_RechargeSuccess_ARG_string_platform_int32_payType_float_price_string_orderId_string_channel__();
            mMessage.CharacterId = __characterId__;
            Request.Platform=platform;
            Request.PayType=payType;
            Request.Price=price;
            Request.OrderId=orderId;
            Request.Channel=channel;

        }

        public __RPC_Logic_RechargeSuccess_ARG_string_platform_int32_payType_float_price_string_orderId_string_channel__ Request { get; private set; }

            private __RPC_Logic_RechargeSuccess_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_RechargeSuccess_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetItemCountOutMessage : OutMessage
    {
        public GetItemCountOutMessage(ClientAgentBase sender, ulong __characterId__, int itemId)
            : base(sender, ServiceType.Logic, 1232, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_GetItemCount_ARG_int32_itemId__();
            mMessage.CharacterId = __characterId__;
            Request.ItemId=itemId;

        }

        public __RPC_Logic_GetItemCount_ARG_int32_itemId__ Request { get; private set; }

            private __RPC_Logic_GetItemCount_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_GetItemCount_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSGetAllianceBuffOutMessage : OutMessage
    {
        public SSGetAllianceBuffOutMessage(ClientAgentBase sender, ulong __characterId__, int placeholder)
            : base(sender, ServiceType.Logic, 1235, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_SSGetAllianceBuff_ARG_int32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Logic_SSGetAllianceBuff_ARG_int32_placeholder__ Request { get; private set; }

            private __RPC_Logic_SSGetAllianceBuff_RET_Int32Array__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_SSGetAllianceBuff_RET_Int32Array__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class NotifyAllianceWarInfoOutMessage : OutMessage
    {
        public NotifyAllianceWarInfoOutMessage(ClientAgentBase sender, ulong __characterId__, AllianceWarInfo info)
            : base(sender, ServiceType.Logic, 1236, (int)MessageType.SAS)
        {
            Request = new __RPC_Logic_NotifyAllianceWarInfo_ARG_AllianceWarInfo_info__();
            mMessage.CharacterId = __characterId__;
            Request.Info=info;

        }

        public __RPC_Logic_NotifyAllianceWarInfo_ARG_AllianceWarInfo_info__ Request { get; private set; }


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

    public class NotifyEnterFubenOutMessage : OutMessage
    {
        public NotifyEnterFubenOutMessage(ClientAgentBase sender, ulong __characterId__, int fubenId)
            : base(sender, ServiceType.Logic, 1239, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_NotifyEnterFuben_ARG_int32_fubenId__();
            mMessage.CharacterId = __characterId__;
            Request.FubenId=fubenId;

        }

        public __RPC_Logic_NotifyEnterFuben_ARG_int32_fubenId__ Request { get; private set; }


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
        public ChangeServerOutMessage(ClientAgentBase sender, ulong __characterId__, int newServerId)
            : base(sender, ServiceType.Logic, 1241, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_ChangeServer_ARG_int32_newServerId__();
            mMessage.CharacterId = __characterId__;
            Request.NewServerId=newServerId;

        }

        public __RPC_Logic_ChangeServer_ARG_int32_newServerId__ Request { get; private set; }

            private __RPC_Logic_ChangeServer_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_ChangeServer_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class LogicGetElfDataOutMessage : OutMessage
    {
        public LogicGetElfDataOutMessage(ClientAgentBase sender, ulong __characterId__, ulong chararcterId)
            : base(sender, ServiceType.Logic, 1249, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_LogicGetElfData_ARG_uint64_chararcterId__();
            mMessage.CharacterId = __characterId__;
            Request.ChararcterId=chararcterId;

        }

        public __RPC_Logic_LogicGetElfData_ARG_uint64_chararcterId__ Request { get; private set; }

            private __RPC_Logic_LogicGetElfData_RET_ElfData__ mResponse;
            public ElfData Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Logic_LogicGetElfData_RET_ElfData__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SBCleanClientCharacterDataOutMessage : OutMessage
    {
        public SBCleanClientCharacterDataOutMessage(ClientAgentBase sender, ulong __characterId__, ulong clientId, ulong characterId)
            : base(sender, ServiceType.Logic, 1500, (int)MessageType.SB)
        {
            Request = new __RPC_Logic_SBCleanClientCharacterData_ARG_uint64_clientId_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.ClientId=clientId;
            Request.CharacterId=characterId;

        }

        public __RPC_Logic_SBCleanClientCharacterData_ARG_uint64_clientId_uint64_characterId__ Request { get; private set; }


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
            : base(sender, ServiceType.Logic, 1501, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_SSNotifyCharacterOnConnet_ARG_uint64_clientId_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.ClientId=clientId;
            Request.CharacterId=characterId;

        }

        public __RPC_Logic_SSNotifyCharacterOnConnet_ARG_uint64_clientId_uint64_characterId__ Request { get; private set; }

            private __RPC_Logic_SSNotifyCharacterOnConnet_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_SSNotifyCharacterOnConnet_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSRequestCityBuidlingPetDataOutMessage : OutMessage
    {
        public SSRequestCityBuidlingPetDataOutMessage(ClientAgentBase sender, ulong __characterId__, ulong guid)
            : base(sender, ServiceType.Logic, 1504, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_SSRequestCityBuidlingPetData_ARG_uint64_guid__();
            mMessage.CharacterId = __characterId__;
            Request.Guid=guid;

        }

        public __RPC_Logic_SSRequestCityBuidlingPetData_ARG_uint64_guid__ Request { get; private set; }

            private __RPC_Logic_SSRequestCityBuidlingPetData_RET_CityBuildingPetMssage__ mResponse;
            public CityBuildingPetMssage Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Logic_SSRequestCityBuidlingPetData_RET_CityBuildingPetMssage__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSGetTodayFunbenCountOutMessage : OutMessage
    {
        public SSGetTodayFunbenCountOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, ulong characterId, int selecttype)
            : base(sender, ServiceType.Logic, 1506, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_SSGetTodayFunbenCount_ARG_int32_serverId_uint64_characterId_int32_selecttype__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.CharacterId=characterId;
            Request.Selecttype=selecttype;

        }

        public __RPC_Logic_SSGetTodayFunbenCount_ARG_int32_serverId_uint64_characterId_int32_selecttype__ Request { get; private set; }

            private __RPC_Logic_SSGetTodayFunbenCount_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_SSGetTodayFunbenCount_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class LogicGetAnyDataOutMessage : OutMessage
    {
        public LogicGetAnyDataOutMessage(ClientAgentBase sender, ulong __characterId__, ulong chararcterId)
            : base(sender, ServiceType.Logic, 1509, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_LogicGetAnyData_ARG_uint64_chararcterId__();
            mMessage.CharacterId = __characterId__;
            Request.ChararcterId=chararcterId;

        }

        public __RPC_Logic_LogicGetAnyData_ARG_uint64_chararcterId__ Request { get; private set; }

            private __RPC_Logic_LogicGetAnyData_RET_LogicGetAnyData__ mResponse;
            public LogicGetAnyData Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Logic_LogicGetAnyData_RET_LogicGetAnyData__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSSyncCharacterFightPointOutMessage : OutMessage
    {
        public SSSyncCharacterFightPointOutMessage(ClientAgentBase sender, ulong __characterId__, int fp)
            : base(sender, ServiceType.Logic, 1530, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_SSSyncCharacterFightPoint_ARG_int32_fp__();
            mMessage.CharacterId = __characterId__;
            Request.Fp=fp;

        }

        public __RPC_Logic_SSSyncCharacterFightPoint_ARG_int32_fp__ Request { get; private set; }

            private __RPC_Logic_SSSyncCharacterFightPoint_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_SSSyncCharacterFightPoint_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class OnPlayerEnterSceneOverOutMessage : OutMessage
    {
        public OnPlayerEnterSceneOverOutMessage(ClientAgentBase sender, ulong __characterId__, int sceneId)
            : base(sender, ServiceType.Logic, 1531, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_OnPlayerEnterSceneOver_ARG_int32_sceneId__();
            mMessage.CharacterId = __characterId__;
            Request.SceneId=sceneId;

        }

        public __RPC_Logic_OnPlayerEnterSceneOver_ARG_int32_sceneId__ Request { get; private set; }

            private __RPC_Logic_OnPlayerEnterSceneOver_RET_ExDataAndFlagData__ mResponse;
            public ExDataAndFlagData Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Logic_OnPlayerEnterSceneOver_RET_ExDataAndFlagData__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class AnchorGiftOutMessage : OutMessage
    {
        public AnchorGiftOutMessage(ClientAgentBase sender, ulong __characterId__, int itemId, int count)
            : base(sender, ServiceType.Logic, 1534, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_AnchorGift_ARG_int32_itemId_int32_count__();
            mMessage.CharacterId = __characterId__;
            Request.ItemId=itemId;
            Request.Count=count;

        }

        public __RPC_Logic_AnchorGift_ARG_int32_itemId_int32_count__ Request { get; private set; }

            private __RPC_Logic_AnchorGift_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_AnchorGift_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSLearnSkillOutMessage : OutMessage
    {
        public SSLearnSkillOutMessage(ClientAgentBase sender, ulong __characterId__, int skillId, int skillLevel)
            : base(sender, ServiceType.Logic, 1603, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_SSLearnSkill_ARG_int32_skillId_int32_skillLevel__();
            mMessage.CharacterId = __characterId__;
            Request.SkillId=skillId;
            Request.SkillLevel=skillLevel;

        }

        public __RPC_Logic_SSLearnSkill_ARG_int32_skillId_int32_skillLevel__ Request { get; private set; }

            private __RPC_Logic_SSLearnSkill_RET_bool__ mResponse;
            public bool Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Logic_SSLearnSkill_RET_bool__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GMDeleteMessageOutMessage : OutMessage
    {
        public GMDeleteMessageOutMessage(ClientAgentBase sender, ulong __characterId__, ulong id)
            : base(sender, ServiceType.Logic, 1605, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_GMDeleteMessage_ARG_uint64_id__();
            mMessage.CharacterId = __characterId__;
            Request.Id=id;

        }

        public __RPC_Logic_GMDeleteMessage_ARG_uint64_id__ Request { get; private set; }


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

    public class GetCharacterDataOutMessage : OutMessage
    {
        public GetCharacterDataOutMessage(ClientAgentBase sender, ulong __characterId__, ulong id)
            : base(sender, ServiceType.Logic, 1990, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_GetCharacterData_ARG_uint64_id__();
            mMessage.CharacterId = __characterId__;
            Request.Id=id;

        }

        public __RPC_Logic_GetCharacterData_ARG_uint64_id__ Request { get; private set; }

            private __RPC_Logic_GetCharacterData_RET_GMCharacterDetailInfo__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_GetCharacterData_RET_GMCharacterDetailInfo__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SendMailToCharacterOutMessage : OutMessage
    {
        public SendMailToCharacterOutMessage(ClientAgentBase sender, ulong __characterId__, string title, string content, Dict_int_int_Data items, int state)
            : base(sender, ServiceType.Logic, 1992, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_SendMailToCharacter_ARG_string_title_string_content_Dict_int_int_Data_items_int32_state__();
            mMessage.CharacterId = __characterId__;
            Request.Title=title;
            Request.Content=content;
            Request.Items=items;
            Request.State=state;

        }

        public __RPC_Logic_SendMailToCharacter_ARG_string_title_string_content_Dict_int_int_Data_items_int32_state__ Request { get; private set; }


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

    public class SendMailToServerOutMessage : OutMessage
    {
        public SendMailToServerOutMessage(ClientAgentBase sender, ulong __characterId__, uint serverId, ulong mailId)
            : base(sender, ServiceType.Logic, 1993, (int)MessageType.SAS)
        {
            Request = new __RPC_Logic_SendMailToServer_ARG_uint32_serverId_uint64_mailId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.MailId=mailId;

        }

        public __RPC_Logic_SendMailToServer_ARG_uint32_serverId_uint64_mailId__ Request { get; private set; }


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
            : base(sender, ServiceType.Logic, 1994, (int)MessageType.SAS)
        {
            Request = new __RPC_Logic_UpdateServer_ARG_int32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Logic_UpdateServer_ARG_int32_placeholder__ Request { get; private set; }


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

    public class GMCommandOutMessage : OutMessage
    {
        public GMCommandOutMessage(ClientAgentBase sender, ulong __characterId__, StringArray commonds)
            : base(sender, ServiceType.Logic, 1995, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_GMCommand_ARG_StringArray_commonds__();
            mMessage.CharacterId = __characterId__;
            Request.Commonds=commonds;

        }

        public __RPC_Logic_GMCommand_ARG_StringArray_commonds__ Request { get; private set; }

            private __RPC_Logic_GMCommand_RET_Int32Array__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_GMCommand_RET_Int32Array__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SendMailToCharacterByIdOutMessage : OutMessage
    {
        public SendMailToCharacterByIdOutMessage(ClientAgentBase sender, ulong __characterId__, int mailId, int createType, int type, Int32Array countList)
            : base(sender, ServiceType.Logic, 1996, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_SendMailToCharacterById_ARG_int32_mailId_int32_createType_int32_type_Int32Array_countList__();
            mMessage.CharacterId = __characterId__;
            Request.MailId=mailId;
            Request.CreateType=createType;
            Request.Type=type;
            Request.CountList=countList;

        }

        public __RPC_Logic_SendMailToCharacterById_ARG_int32_mailId_int32_createType_int32_type_Int32Array_countList__ Request { get; private set; }


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

    public class LogicGetMountDataOutMessage : OutMessage
    {
        public LogicGetMountDataOutMessage(ClientAgentBase sender, ulong __characterId__, ulong chararcterId)
            : base(sender, ServiceType.Logic, 1325, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_LogicGetMountData_ARG_uint64_chararcterId__();
            mMessage.CharacterId = __characterId__;
            Request.ChararcterId=chararcterId;

        }

        public __RPC_Logic_LogicGetMountData_ARG_uint64_chararcterId__ Request { get; private set; }

            private __RPC_Logic_LogicGetMountData_RET_MountMsgData__ mResponse;
            public MountMsgData Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Logic_LogicGetMountData_RET_MountMsgData__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class CloneCharacterDbByIdOutMessage : OutMessage
    {
        public CloneCharacterDbByIdOutMessage(ClientAgentBase sender, ulong __characterId__, ulong fromId, ulong toId)
            : base(sender, ServiceType.Logic, 1329, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_CloneCharacterDbById_ARG_uint64_fromId_uint64_toId__();
            mMessage.CharacterId = __characterId__;
            Request.FromId=fromId;
            Request.ToId=toId;

        }

        public __RPC_Logic_CloneCharacterDbById_ARG_uint64_fromId_uint64_toId__ Request { get; private set; }

            private __RPC_Logic_CloneCharacterDbById_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_CloneCharacterDbById_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GiveItemListOutMessage : OutMessage
    {
        public GiveItemListOutMessage(ClientAgentBase sender, ulong __characterId__, Dict_int_int_Data items, int from)
            : base(sender, ServiceType.Logic, 1333, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_GiveItemList_ARG_Dict_int_int_Data_items_int32_from__();
            mMessage.CharacterId = __characterId__;
            Request.Items=items;
            Request.From=from;

        }

        public __RPC_Logic_GiveItemList_ARG_Dict_int_int_Data_items_int32_from__ Request { get; private set; }

            private __RPC_Logic_GiveItemList_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_GiveItemList_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class ApplyPlayerFlagOutMessage : OutMessage
    {
        public ApplyPlayerFlagOutMessage(ClientAgentBase sender, ulong __characterId__, Int32Array flagList)
            : base(sender, ServiceType.Logic, 1339, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_ApplyPlayerFlag_ARG_Int32Array_flagList__();
            mMessage.CharacterId = __characterId__;
            Request.FlagList=flagList;

        }

        public __RPC_Logic_ApplyPlayerFlag_ARG_Int32Array_flagList__ Request { get; private set; }

            private __RPC_Logic_ApplyPlayerFlag_RET_Dict_int_int_Data__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Logic_ApplyPlayerFlag_RET_Dict_int_int_Data__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class ApplyMayaSkillOutMessage : OutMessage
    {
        public ApplyMayaSkillOutMessage(ClientAgentBase sender, ulong __characterId__, int id)
            : base(sender, ServiceType.Logic, 1342, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_ApplyMayaSkill_ARG_int32_id__();
            mMessage.CharacterId = __characterId__;
            Request.Id=id;

        }

        public __RPC_Logic_ApplyMayaSkill_ARG_int32_id__ Request { get; private set; }


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

    public class SSSendMailByIdOutMessage : OutMessage
    {
        public SSSendMailByIdOutMessage(ClientAgentBase sender, ulong __characterId__, int tableId, int ExtendType, string ExtendPara0, string ExtendPara1)
            : base(sender, ServiceType.Logic, 1344, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_SSSendMailById_ARG_int32_tableId_int32_ExtendType_string_ExtendPara0_string_ExtendPara1__();
            mMessage.CharacterId = __characterId__;
            Request.TableId=tableId;
            Request.ExtendType=ExtendType;
            Request.ExtendPara0=ExtendPara0;
            Request.ExtendPara1=ExtendPara1;

        }

        public __RPC_Logic_SSSendMailById_ARG_int32_tableId_int32_ExtendType_string_ExtendPara0_string_ExtendPara1__ Request { get; private set; }


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

    public class NotifyPlayerMoniterDataOutMessage : OutMessage
    {
        public NotifyPlayerMoniterDataOutMessage(ClientAgentBase sender, ulong __characterId__, MsgChatMoniterData data)
            : base(sender, ServiceType.Logic, 1346, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_NotifyPlayerMoniterData_ARG_MsgChatMoniterData_data__();
            mMessage.CharacterId = __characterId__;
            Request.Data=data;

        }

        public __RPC_Logic_NotifyPlayerMoniterData_ARG_MsgChatMoniterData_data__ Request { get; private set; }


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

    public class GetPlayerMoniterDataOutMessage : OutMessage
    {
        public GetPlayerMoniterDataOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId)
            : base(sender, ServiceType.Logic, 1347, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_GetPlayerMoniterData_ARG_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;

        }

        public __RPC_Logic_GetPlayerMoniterData_ARG_uint64_characterId__ Request { get; private set; }

            private __RPC_Logic_GetPlayerMoniterData_RET_MsgChatMoniterData__ mResponse;
            public MsgChatMoniterData Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Logic_GetPlayerMoniterData_RET_MsgChatMoniterData__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class NotifyInviteChallengeOutMessage : OutMessage
    {
        public NotifyInviteChallengeOutMessage(ClientAgentBase sender, ulong __characterId__, ulong invitorId, string invitorName, int invitorServerId)
            : base(sender, ServiceType.Logic, 1355, (int)MessageType.SS)
        {
            Request = new __RPC_Logic_NotifyInviteChallenge_ARG_uint64_invitorId_string_invitorName_int32_invitorServerId__();
            mMessage.CharacterId = __characterId__;
            Request.InvitorId=invitorId;
            Request.InvitorName=invitorName;
            Request.InvitorServerId=invitorServerId;

        }

        public __RPC_Logic_NotifyInviteChallenge_ARG_uint64_invitorId_string_invitorName_int32_invitorServerId__ Request { get; private set; }


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

    public class AddFunctionNameLogic
    {
        public static void AddFunctionName(IDictionary<int, string> dict)
        {
            dict[1000] = "PrepareDataForEnterGame";
            dict[1001] = "PrepareDataForCreateCharacter";
            dict[1002] = "PrepareDataForCommonUse";
            dict[1003] = "PrepareDataForLogout";
            dict[1015] = "CreateCharacter";
            dict[1016] = "DelectCharacter";
            dict[1030] = "SBGetAllOnlineCharacterInServer";
            dict[1031] = "GetLogicSimpleData";
            dict[1032] = "CheckConnected";
            dict[1033] = "CheckLost";
            dict[1034] = "QueryStatus";
            dict[1040] = "QueryBrokerStatus";
            dict[1041] = "GMLogic";
            dict[1042] = "LogicGetEquipList";
            dict[1043] = "LogicGetSkillData";
            dict[1044] = "LogicGetTalentData";
            dict[1045] = "LogicGetBookAttrData";
            dict[1046] = "ApplySkill";
            dict[1047] = "UpgradeInnate";
            dict[1048] = "ClearInnate";
            dict[1049] = "ApplyBags";
            dict[1050] = "ApplyFlag";
            dict[1051] = "ApplyExdata";
            dict[1052] = "ApplyExdata64";
            dict[1053] = "ApplyMission";
            dict[1054] = "ApplyBooks";
            dict[1055] = "ReplaceEquip";
            dict[1056] = "AcceptMission";
            dict[1057] = "CommitMission";
            dict[1058] = "CompleteMission";
            dict[1059] = "DropMission";
            dict[1060] = "SyncMission";
            dict[1061] = "SyncFlag";
            dict[1062] = "SyncFlagList";
            dict[1063] = "SyncExdata";
            dict[1064] = "SyncExdataList";
            dict[1065] = "SyncExdata64";
            dict[1066] = "SyncResources";
            dict[1067] = "SyncItems";
            dict[1068] = "EquipSkill";
            dict[1069] = "UpgradeSkill";
            dict[1070] = "SellBagItem";
            dict[1071] = "RecycleBagItem";
            dict[1072] = "EnchanceEquip";
            dict[1073] = "AppendEquip";
            dict[1074] = "ResetExcellentEquip";
            dict[1075] = "ConfirmResetExcellentEquip";
            dict[1076] = "SuperExcellentEquip";
            dict[1077] = "SmritiEquip";
            dict[1078] = "UseItem";
            dict[1079] = "LogicKillMonster";
            dict[1080] = "GiveItem";
            dict[1081] = "DeleteItem";
            dict[1082] = "SyncMissions";
            dict[1083] = "FirstOnline";
            dict[1084] = "ActivationReward";
            dict[1086] = "ComposeItem";
            dict[1087] = "FinishAchievement";
            dict[1088] = "RewardAchievement";
            dict[1089] = "DistributionAttrPoint";
            dict[1090] = "RefreshAttrPoint";
            dict[1091] = "SetAttributeAutoAdd";
            dict[1092] = "ApplyFriends";
            dict[1093] = "SeekCharacters";
            dict[1393] = "SeekCharactersReceive";
            dict[1094] = "SeekFriends";
            dict[1394] = "SeekFriendsReceive";
            dict[1095] = "AddFriendById";
            dict[1096] = "SSAddFriendById";
            dict[1097] = "AddFriendByName";
            dict[1098] = "DelFriendById";
            dict[1099] = "ServerGMCommand";
            dict[1100] = "CompleteFuben";
            dict[1101] = "DungeonComplete";
            dict[1102] = "SelectDungeonReward";
            dict[1103] = "EnterFuben";
            dict[1104] = "ResetFuben";
            dict[1105] = "SweepFuben";
            dict[1106] = "ApplyStores";
            dict[1107] = "ActivateBook";
            dict[1319] = "ActivateBookGroup";
            dict[1108] = "SortBag";
            dict[1109] = "ApplyPlayerInfo";
            dict[1110] = "SetFlag";
            dict[1111] = "SetExData";
            dict[1112] = "ApplyMails";
            dict[1113] = "ApplyMailInfo";
            dict[1114] = "ReceiveMail";
            dict[1115] = "DeleteMail";
            dict[1116] = "SyncMails";
            dict[1117] = "DurableDown";
            dict[1118] = "RepairEquip";
            dict[1119] = "NpcService";
            dict[1120] = "DepotTakeOut";
            dict[1121] = "DepotPutIn";
            dict[1122] = "WishingPoolDepotTakeOut";
            dict[1123] = "StoreBuy";
            dict[1124] = "ApplyCityData";
            dict[1125] = "CityOperationRequest";
            dict[1126] = "SyncCityBuildingData";
            dict[1127] = "SyncPetMission";
            dict[1128] = "EnterCity";
            dict[1129] = "EquipDurableBroken";
            dict[1130] = "EquipDurableChange";
            dict[1131] = "ApplyEquipDurable";
            dict[1132] = "ElfOperate";
            dict[1133] = "ElfReplace";
            dict[1134] = "PushDraw";
            dict[1135] = "CheckCharacterInFuben";
            dict[1136] = "WingFormation";
            dict[1137] = "WingTrain";
            dict[1138] = "OperatePet";
            dict[1139] = "OperatePetMission";
            dict[1140] = "PickUpMedal";
            dict[1141] = "EnchanceMedal";
            dict[1142] = "EquipMedal";
            dict[1317] = "SplitMedal";
            dict[1143] = "BuySpaceBag";
            dict[1144] = "DeletePetMission";
            dict[1145] = "UseBuildService";
            dict[1146] = "GetP1vP1LadderPlayer";
            dict[1147] = "GetP1vP1FightPlayer";
            dict[1148] = "LogicP1vP1FightOver";
            dict[1149] = "LogicP1vP1LadderAdvance";
            dict[1150] = "PushP1vP1LadderChange";
            dict[1151] = "GetP1vP1LadderOldList";
            dict[1152] = "LogicP1vP1FightResult";
            dict[1153] = "BuyP1vP1Count";
            dict[1154] = "DrawLotteryPetEgg";
            dict[1155] = "RecoveryEquip";
            dict[1156] = "DrawWishingPool";
            dict[1157] = "ResetSkillTalent";
            dict[1158] = "RobotcFinishFuben";
            dict[1159] = "CreateAlliance";
            dict[1160] = "AllianceOperation";
            dict[1161] = "AllianceOperationCharacter";
            dict[1162] = "AllianceOperationCharacterByName";
            dict[1163] = "LogicSyncAllianceMessage";
            dict[1164] = "SSGetFlagOrCondition";
            dict[1165] = "SSIsShield";
            dict[1166] = "WorshipCharacter";
            dict[1167] = "SSChangeExdata";
            dict[1168] = "DonationAllianceItem";
            dict[1170] = "ElfDrawOver";
            dict[1171] = "TalentCountChange";
            dict[1172] = "CityMissionOperation";
            dict[1173] = "DropCityMission";
            dict[1174] = "CityRefreshMission";
            dict[1175] = "StoreOperationAdd";
            dict[1176] = "StoreOperationBroadcast";
            dict[1177] = "StoreOperationBuy";
            dict[1178] = "StoreOperationCancel";
            dict[1179] = "StoreOperationLook";
            dict[1180] = "StoreOperationLookSelf";
            dict[1181] = "StoreOperationHarvest";
            dict[1182] = "SSStoreOperationBuy";
            dict[1183] = "SSStoreOperationExchange";
            dict[1184] = "GetExchangeData";
            dict[1185] = "ApplyGroupShopItems";
            dict[1186] = "NotifyStoreBuyed";
            dict[1187] = "BuyGroupShopItem";
            dict[1188] = "GetBuyedGroupShopItems";
            dict[1189] = "GetGroupShopHistory";
            dict[1190] = "SendMailToCharacterByItems";
            dict[1191] = "AllianceDataChange";
            dict[1192] = "SSBattleResult";
            dict[1193] = "AcceptBattleAward";
            dict[1194] = "AstrologyLevelUp";
            dict[1195] = "AstrologyEquipOn";
            dict[1196] = "AstrologyEquipOff";
            dict[1197] = "AstrologyDrawOver";
            dict[1198] = "SyncAddFriend";
            dict[1199] = "UsePetExpItem";
            dict[1200] = "Reincarnation";
            dict[1201] = "UpgradeHonor";
            dict[1202] = "ReadyToEnter";
            dict[1203] = "LogicNotifyMessage";
            dict[1204] = "NotifGainRes";
            dict[1205] = "BattleResult";
            dict[1206] = "ApplyCityBuildingData";
            dict[1207] = "ApplyBagByType";
            dict[1208] = "NotifyP1vP1Change";
            dict[1209] = "SSGetFriendList";
            dict[1210] = "SyncFriendDataChange";
            dict[1211] = "SyncFriendDelete";
            dict[1212] = "SSSendSimpleData";
            dict[1213] = "SSFriendpPssiveChange";
            dict[1214] = "SSFetchExdata";
            dict[1215] = "SSDeleteItemByIndex";
            dict[1216] = "StoreBuyEquip";
            dict[1217] = "GetQuestionData";
            dict[1218] = "AnswerQuestion";
            dict[1219] = "SSGetMissionEnterScene";
            dict[1220] = "RemoveErrorAnswer";
            dict[1221] = "AnswerQuestionUseItem";
            dict[1222] = "ApplyPlayerHeadInfo";
            dict[1223] = "NotifyDungeonClose";
            dict[1224] = "GetCompensationList";
            dict[1225] = "ReceiveCompensation";
            dict[1226] = "SSSetExdata";
            dict[1227] = "SSSetFlag";
            dict[1228] = "SelectTitle";
            dict[1229] = "LogicGetTitleList";
            dict[1230] = "RechargeSuccess";
            dict[1231] = "NotifyRechargeSuccess";
            dict[1232] = "GetItemCount";
            dict[1233] = "RetrainPet";
            dict[1234] = "UpgradeAllianceBuff";
            dict[1235] = "SSGetAllianceBuff";
            dict[1236] = "NotifyAllianceWarInfo";
            dict[1237] = "Investment";
            dict[1238] = "GainReward";
            dict[1239] = "NotifyEnterFuben";
            dict[1240] = "Worship";
            dict[1241] = "ChangeServer";
            dict[1242] = "UseGiftCode";
            dict[1243] = "ApplyRechargeTables";
            dict[1244] = "ApplyFirstChargeItem";
            dict[1245] = "ApplyGetFirstChargeItem";
            dict[1246] = "TakeMultyExpAward";
            dict[1247] = "RandEquipSkill";
            dict[1248] = "UseEquipSkill";
            dict[1249] = "LogicGetElfData";
            dict[1250] = "ReplaceElfSkill";
            dict[1252] = "RecycleBagItemList";
            dict[1253] = "ResolveElfList";
            dict[1260] = "CSEnterEraById";
            dict[1261] = "EraPlayedSkill";
            dict[1262] = "EraTakeAchvAward";
            dict[1263] = "EraTakeAward";
            dict[1264] = "RefreshHunterMission";
            dict[1270] = "CSApplyOfflineExpData";
            dict[1271] = "RereshTiralTime";
            dict[1500] = "SBCleanClientCharacterData";
            dict[1501] = "SSNotifyCharacterOnConnet";
            dict[1503] = "BSNotifyCharacterOnLost";
            dict[1504] = "SSRequestCityBuidlingPetData";
            dict[1505] = "GetReviewState";
            dict[1506] = "SSGetTodayFunbenCount";
            dict[1507] = "OnItemAuction";
            dict[1508] = "BuyItemAuction";
            dict[1509] = "LogicGetAnyData";
            dict[1510] = "ApplySellHistory";
            dict[1511] = "DrawWishItem";
            dict[1512] = "ApplyOperationActivity";
            dict[1513] = "ClaimOperationReward";
            dict[1514] = "SyncOperationActivityItem";
            dict[1520] = "ApplyPromoteHP";
            dict[1521] = "ApplyPromoteSkill";
            dict[1522] = "ApplyPickUpBox";
            dict[1523] = "ApplyJoinActivity";
            dict[1524] = "ApplyPortraitAward";
            dict[1530] = "SSSyncCharacterFightPoint";
            dict[1531] = "OnPlayerEnterSceneOver";
            dict[1532] = "SyncOperationActivityTerm";
            dict[1533] = "ApplyGetTowerReward";
            dict[1534] = "AnchorGift";
            dict[1600] = "SendJsonData";
            dict[1601] = "BuyWingCharge";
            dict[1602] = "BuyEnergyByType";
            dict[1603] = "SSLearnSkill";
            dict[1605] = "GMDeleteMessage";
            dict[1606] = "ApplyKaiFuTeHuiData";
            dict[1607] = "BuyKaiFuTeHuiItem";
            dict[1608] = "BattleUnionDonateEquip";
            dict[1609] = "BattleUnionTakeOutEquip";
            dict[1990] = "GetCharacterData";
            dict[1992] = "SendMailToCharacter";
            dict[1993] = "SendMailToServer";
            dict[1994] = "UpdateServer";
            dict[1995] = "GMCommand";
            dict[1996] = "SendMailToCharacterById";
            dict[1999] = "ClientErrorMessage";
            dict[1604] = "SendQuestion";
            dict[1330] = "SendSurvey";
            dict[1314] = "TowerSweep";
            dict[1315] = "TowerBuySweepTimes";
            dict[1316] = "CheckTowerDailyInfo";
            dict[1318] = "SetHandbookFight";
            dict[1320] = "AskMountData";
            dict[1321] = "MountUp";
            dict[1322] = "RideMount";
            dict[1323] = "MountSkill";
            dict[1324] = "MountFeed";
            dict[1325] = "LogicGetMountData";
            dict[1326] = "Mount";
            dict[1327] = "SendMountData";
            dict[1328] = "NotifySnapShotResult";
            dict[1329] = "CloneCharacterDbById";
            dict[1331] = "AddSkillPoint";
            dict[1332] = "BossHomeCost";
            dict[1333] = "GiveItemList";
            dict[1336] = "AddMountSkin";
            dict[1337] = "ApplyFieldActivityReward";
            dict[1338] = "ApplyFriendListData";
            dict[1339] = "ApplyPlayerFlag";
            dict[1340] = "UseShiZhuang";
            dict[1341] = "ClickMayaTip";
            dict[1342] = "ApplyMayaSkill";
            dict[1343] = "ChangeEquipState";
            dict[1344] = "SSSendMailById";
            dict[1345] = "RefreshFashionInfo";
            dict[1346] = "NotifyPlayerMoniterData";
            dict[1347] = "GetPlayerMoniterData";
            dict[1348] = "ApplySuperVIP";
            dict[1349] = "WorshipMonument";
            dict[1350] = "SaveSuperExcellentEquip";
            dict[1351] = "ModifyPlayerName";
            dict[1352] = "InviteChallenge";
            dict[1353] = "ReceiveChallenge";
            dict[1354] = "AcceptChallenge";
            dict[1355] = "NotifyInviteChallenge";
        }
        public static void AddCSFunctionName(IDictionary<int, string> dict)
        {
            dict[1041] = "GMLogic";
            dict[1046] = "ApplySkill";
            dict[1047] = "UpgradeInnate";
            dict[1048] = "ClearInnate";
            dict[1049] = "ApplyBags";
            dict[1050] = "ApplyFlag";
            dict[1051] = "ApplyExdata";
            dict[1052] = "ApplyExdata64";
            dict[1053] = "ApplyMission";
            dict[1054] = "ApplyBooks";
            dict[1055] = "ReplaceEquip";
            dict[1056] = "AcceptMission";
            dict[1057] = "CommitMission";
            dict[1058] = "CompleteMission";
            dict[1059] = "DropMission";
            dict[1068] = "EquipSkill";
            dict[1069] = "UpgradeSkill";
            dict[1070] = "SellBagItem";
            dict[1071] = "RecycleBagItem";
            dict[1072] = "EnchanceEquip";
            dict[1073] = "AppendEquip";
            dict[1074] = "ResetExcellentEquip";
            dict[1075] = "ConfirmResetExcellentEquip";
            dict[1076] = "SuperExcellentEquip";
            dict[1077] = "SmritiEquip";
            dict[1078] = "UseItem";
            dict[1084] = "ActivationReward";
            dict[1086] = "ComposeItem";
            dict[1088] = "RewardAchievement";
            dict[1089] = "DistributionAttrPoint";
            dict[1090] = "RefreshAttrPoint";
            dict[1091] = "SetAttributeAutoAdd";
            dict[1092] = "ApplyFriends";
            dict[1093] = "SeekCharacters";
            dict[1094] = "SeekFriends";
            dict[1095] = "AddFriendById";
            dict[1097] = "AddFriendByName";
            dict[1098] = "DelFriendById";
            dict[1102] = "SelectDungeonReward";
            dict[1103] = "EnterFuben";
            dict[1104] = "ResetFuben";
            dict[1105] = "SweepFuben";
            dict[1106] = "ApplyStores";
            dict[1107] = "ActivateBook";
            dict[1108] = "SortBag";
            dict[1109] = "ApplyPlayerInfo";
            dict[1110] = "SetFlag";
            dict[1111] = "SetExData";
            dict[1112] = "ApplyMails";
            dict[1113] = "ApplyMailInfo";
            dict[1114] = "ReceiveMail";
            dict[1115] = "DeleteMail";
            dict[1118] = "RepairEquip";
            dict[1120] = "DepotTakeOut";
            dict[1121] = "DepotPutIn";
            dict[1122] = "WishingPoolDepotTakeOut";
            dict[1123] = "StoreBuy";
            dict[1124] = "ApplyCityData";
            dict[1125] = "CityOperationRequest";
            dict[1128] = "EnterCity";
            dict[1131] = "ApplyEquipDurable";
            dict[1132] = "ElfOperate";
            dict[1133] = "ElfReplace";
            dict[1136] = "WingFormation";
            dict[1137] = "WingTrain";
            dict[1138] = "OperatePet";
            dict[1139] = "OperatePetMission";
            dict[1140] = "PickUpMedal";
            dict[1141] = "EnchanceMedal";
            dict[1142] = "EquipMedal";
            dict[1317] = "SplitMedal";
            dict[1143] = "BuySpaceBag";
            dict[1145] = "UseBuildService";
            dict[1146] = "GetP1vP1LadderPlayer";
            dict[1147] = "GetP1vP1FightPlayer";
            dict[1151] = "GetP1vP1LadderOldList";
            dict[1153] = "BuyP1vP1Count";
            dict[1154] = "DrawLotteryPetEgg";
            dict[1155] = "RecoveryEquip";
            dict[1156] = "DrawWishingPool";
            dict[1157] = "ResetSkillTalent";
            dict[1158] = "RobotcFinishFuben";
            dict[1159] = "CreateAlliance";
            dict[1160] = "AllianceOperation";
            dict[1161] = "AllianceOperationCharacter";
            dict[1162] = "AllianceOperationCharacterByName";
            dict[1166] = "WorshipCharacter";
            dict[1168] = "DonationAllianceItem";
            dict[1172] = "CityMissionOperation";
            dict[1173] = "DropCityMission";
            dict[1174] = "CityRefreshMission";
            dict[1175] = "StoreOperationAdd";
            dict[1176] = "StoreOperationBroadcast";
            dict[1177] = "StoreOperationBuy";
            dict[1178] = "StoreOperationCancel";
            dict[1179] = "StoreOperationLook";
            dict[1180] = "StoreOperationLookSelf";
            dict[1181] = "StoreOperationHarvest";
            dict[1183] = "SSStoreOperationExchange";
            dict[1185] = "ApplyGroupShopItems";
            dict[1187] = "BuyGroupShopItem";
            dict[1188] = "GetBuyedGroupShopItems";
            dict[1189] = "GetGroupShopHistory";
            dict[1193] = "AcceptBattleAward";
            dict[1194] = "AstrologyLevelUp";
            dict[1195] = "AstrologyEquipOn";
            dict[1196] = "AstrologyEquipOff";
            dict[1199] = "UsePetExpItem";
            dict[1200] = "Reincarnation";
            dict[1201] = "UpgradeHonor";
            dict[1206] = "ApplyCityBuildingData";
            dict[1207] = "ApplyBagByType";
            dict[1216] = "StoreBuyEquip";
            dict[1217] = "GetQuestionData";
            dict[1218] = "AnswerQuestion";
            dict[1220] = "RemoveErrorAnswer";
            dict[1221] = "AnswerQuestionUseItem";
            dict[1222] = "ApplyPlayerHeadInfo";
            dict[1224] = "GetCompensationList";
            dict[1225] = "ReceiveCompensation";
            dict[1228] = "SelectTitle";
            dict[1233] = "RetrainPet";
            dict[1234] = "UpgradeAllianceBuff";
            dict[1237] = "Investment";
            dict[1238] = "GainReward";
            dict[1240] = "Worship";
            dict[1242] = "UseGiftCode";
            dict[1243] = "ApplyRechargeTables";
            dict[1244] = "ApplyFirstChargeItem";
            dict[1245] = "ApplyGetFirstChargeItem";
            dict[1246] = "TakeMultyExpAward";
            dict[1247] = "RandEquipSkill";
            dict[1248] = "UseEquipSkill";
            dict[1250] = "ReplaceElfSkill";
            dict[1252] = "RecycleBagItemList";
            dict[1253] = "ResolveElfList";
            dict[1260] = "CSEnterEraById";
            dict[1261] = "EraPlayedSkill";
            dict[1262] = "EraTakeAchvAward";
            dict[1263] = "EraTakeAward";
            dict[1264] = "RefreshHunterMission";
            dict[1270] = "CSApplyOfflineExpData";
            dict[1271] = "RereshTiralTime";
            dict[1505] = "GetReviewState";
            dict[1507] = "OnItemAuction";
            dict[1508] = "BuyItemAuction";
            dict[1510] = "ApplySellHistory";
            dict[1511] = "DrawWishItem";
            dict[1512] = "ApplyOperationActivity";
            dict[1513] = "ClaimOperationReward";
            dict[1520] = "ApplyPromoteHP";
            dict[1521] = "ApplyPromoteSkill";
            dict[1522] = "ApplyPickUpBox";
            dict[1523] = "ApplyJoinActivity";
            dict[1524] = "ApplyPortraitAward";
            dict[1533] = "ApplyGetTowerReward";
            dict[1600] = "SendJsonData";
            dict[1601] = "BuyWingCharge";
            dict[1602] = "BuyEnergyByType";
            dict[1606] = "ApplyKaiFuTeHuiData";
            dict[1607] = "BuyKaiFuTeHuiItem";
            dict[1608] = "BattleUnionDonateEquip";
            dict[1609] = "BattleUnionTakeOutEquip";
            dict[1999] = "ClientErrorMessage";
            dict[1604] = "SendQuestion";
            dict[1330] = "SendSurvey";
            dict[1314] = "TowerSweep";
            dict[1315] = "TowerBuySweepTimes";
            dict[1316] = "CheckTowerDailyInfo";
            dict[1318] = "SetHandbookFight";
            dict[1320] = "AskMountData";
            dict[1321] = "MountUp";
            dict[1322] = "RideMount";
            dict[1323] = "MountSkill";
            dict[1324] = "MountFeed";
            dict[1326] = "Mount";
            dict[1332] = "BossHomeCost";
            dict[1336] = "AddMountSkin";
            dict[1337] = "ApplyFieldActivityReward";
            dict[1338] = "ApplyFriendListData";
            dict[1340] = "UseShiZhuang";
            dict[1341] = "ClickMayaTip";
            dict[1343] = "ChangeEquipState";
            dict[1345] = "RefreshFashionInfo";
            dict[1348] = "ApplySuperVIP";
            dict[1349] = "WorshipMonument";
            dict[1350] = "SaveSuperExcellentEquip";
            dict[1351] = "ModifyPlayerName";
            dict[1352] = "InviteChallenge";
            dict[1354] = "AcceptChallenge";
        }
    }
}
