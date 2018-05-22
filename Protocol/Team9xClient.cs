using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Scorpion;
using DataContract;
using ProtoBuf;

#pragma warning disable 0162,0108
namespace TeamClientService
{

    public abstract class TeamAgent : ClientAgentBase
    {
        public TeamAgent(string addr)
            : base(addr)
        {
        }

        public TeamAgent(ServerInfo broker, ServerInfo[] directConnect, Func<ulong, int> characterId2ServerId)
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
        /// 队长创建切换场景
        /// </summary>
        public TeamDungeonLeaderChangeSceneOutMessage TeamDungeonLeaderChangeScene(ulong __characterId__, ulong sceneGuid)
        {
            return new TeamDungeonLeaderChangeSceneOutMessage(this, __characterId__, sceneGuid);
        }

        /// <summary>
        /// 退出副本队伍了
        /// </summary>
        public LeaveDungeonOutMessage LeaveDungeon(ulong __characterId__, ulong characterId)
        {
            return new LeaveDungeonOutMessage(this, __characterId__, characterId);
        }

        /// <summary>
        /// 广播表格重载
        /// </summary>
        public ServerGMCommandOutMessage ServerGMCommand(string cmd, string param)
        {
            return new ServerGMCommandOutMessage(this, 0, cmd, param);
        }

        /// <summary>
        /// 战盟操作:创建
        /// </summary>
        public Logic2TeamCreateAllianceOutMessage Logic2TeamCreateAlliance(ulong __characterId__, int serverId, ulong guid, string name, int state)
        {
            return new Logic2TeamCreateAllianceOutMessage(this, __characterId__, serverId, guid, name, state);
        }

        /// <summary>
        /// 战盟操作:其他操作 	type：0=申请加入（value=战盟ID）  1=取消申请（value=战盟ID）  2=退出战盟   3=同意邀请（value=战盟ID）  4=拒绝邀请（value=战盟ID）
        /// </summary>
        public Logic2TeamAllianceOperationOutMessage Logic2TeamAllianceOperation(ulong __characterId__, int type, int value)
        {
            return new Logic2TeamAllianceOperationOutMessage(this, __characterId__, type, value);
        }

        /// <summary>
        /// 战盟操作:其他操作     type：0=邀请加入 1=同意申请加入 2：拒绝申请加入
        /// </summary>
        public Logic2TeamAllianceOperationCharacterOutMessage Logic2TeamAllianceOperationCharacter(ulong __characterId__, int type, string name, int allianceId, ulong guid)
        {
            return new Logic2TeamAllianceOperationCharacterOutMessage(this, __characterId__, type, name, allianceId, guid);
        }

        /// <summary>
        /// 同步一次帮派数据
        /// </summary>
        public GetAllianceCharacterDataOutMessage GetAllianceCharacterData(ulong __characterId__, int serverId, ulong guid, int level)
        {
            return new GetAllianceCharacterDataOutMessage(this, __characterId__, serverId, guid, level);
        }

        /// <summary>
        /// 请求玩家的队伍信息
        /// </summary>
        public SSGetTeamDataOutMessage SSGetTeamData(ulong __characterId__, ulong guid)
        {
            return new SSGetTeamDataOutMessage(this, __characterId__, guid);
        }

        /// <summary>
        /// 请求玩家的战盟信息
        /// </summary>
        public SSGetAllianceDataOutMessage SSGetAllianceData(ulong __characterId__, int serverId)
        {
            return new SSGetAllianceDataOutMessage(this, __characterId__, serverId);
        }

        /// <summary>
        /// 玩家捐献，这边需要记录
        /// </summary>
        public Logic2TeamDonationAllianceItemOutMessage Logic2TeamDonationAllianceItem(ulong __characterId__, int serverId, int type, string name)
        {
            return new Logic2TeamDonationAllianceItemOutMessage(this, __characterId__, serverId, type, name);
        }

        /// <summary>
        /// 获得战盟的Buff等级
        /// SS int32                SSGetAllianceBuffLevel(int32 serverId,int32 buffId)=7079;
        /// 获得战盟的名字
        /// </summary>
        public SSGetAllianceNameOutMessage SSGetAllianceName(ulong __characterId__, int allianceId)
        {
            return new SSGetAllianceNameOutMessage(this, __characterId__, allianceId);
        }

        /// <summary>
        /// GM获得一个服务器所有战盟的详细信息GM后台使用
        /// </summary>
        public SSGetAllianceOutMessage SSGetAlliance(ulong __characterId__, int serverId, int startIndex, int EndIndex, string name)
        {
            return new SSGetAllianceOutMessage(this, __characterId__, serverId, startIndex, EndIndex, name);
        }

        /// <summary>
        /// GM修改角色权限
        /// </summary>
        public GMChangeJurisdictionOutMessage GMChangeJurisdiction(ulong __characterId__, int serverId, int allianceId, ulong guid, int type)
        {
            return new GMChangeJurisdictionOutMessage(this, __characterId__, serverId, allianceId, guid, type);
        }

        /// <summary>
        /// GM修改战盟公告
        /// </summary>
        public GMChangeAllianceNoticeOutMessage GMChangeAllianceNotice(ulong __characterId__, int serverId, int allianceId, string Content)
        {
            return new GMChangeAllianceNoticeOutMessage(this, __characterId__, serverId, allianceId, Content);
        }

        /// <summary>
        /// GM解散战盟
        /// </summary>
        public GMDelAllicanceOutMessage GMDelAllicance(ulong __characterId__, int allianceId)
        {
            return new GMDelAllicanceOutMessage(this, __characterId__, allianceId);
        }

        /// <summary>
        /// 交易所：广播交易道具
        /// </summary>
        public BroadcastExchangeItemOutMessage BroadcastExchangeItem(ulong __characterId__, ulong characterId, string characterName, ItemBaseData item, int needCount, int ContinueMinutes)
        {
            return new BroadcastExchangeItemOutMessage(this, __characterId__, characterId, characterName, item, needCount, ContinueMinutes);
        }

        /// <summary>
        /// 交易所：请求可看到的交易所道具
        /// </summary>
        public GetExchangeItemOutMessage GetExchangeItem(ulong __characterId__, ulong characterId, int level, int count, int type)
        {
            return new GetExchangeItemOutMessage(this, __characterId__, characterId, level, count, type);
        }

        /// <summary>
        /// 交易所：取消广播交易道具
        /// </summary>
        public CancelExchangeItemOutMessage CancelExchangeItem(ulong __characterId__, ulong characterId, long itemGuid)
        {
            return new CancelExchangeItemOutMessage(this, __characterId__, characterId, itemGuid);
        }

        /// <summary>
        /// 获得团购申请
        /// </summary>
        public SSApplyGroupShopItemsOutMessage SSApplyGroupShopItems(ulong __characterId__, Int32Array types, Int64ArrayList items, int profession)
        {
            return new SSApplyGroupShopItemsOutMessage(this, __characterId__, types, items, profession);
        }

        /// <summary>
        /// 购买一个道具
        /// </summary>
        public SSBuyGroupShopItemOutMessage SSBuyGroupShopItem(ulong __characterId__, long guid, int count)
        {
            return new SSBuyGroupShopItemOutMessage(this, __characterId__, guid, count);
        }

        /// <summary>
        /// 获取我当前的愿望
        /// </summary>
        public SSGetBuyedGroupShopItemsOutMessage SSGetBuyedGroupShopItems(ulong __characterId__, Int64Array buyed)
        {
            return new SSGetBuyedGroupShopItemsOutMessage(this, __characterId__, buyed);
        }

        /// <summary>
        /// 获取团购历史
        /// </summary>
        public SSGetGroupShopHistoryOutMessage SSGetGroupShopHistory(ulong __characterId__, Int64Array buyed, Int64Array history)
        {
            return new SSGetGroupShopHistoryOutMessage(this, __characterId__, buyed, history);
        }

        /// <summary>
        /// 战场有人进去了
        /// </summary>
        public SSCharacterEnterBattleOutMessage SSCharacterEnterBattle(ulong __characterId__, int fubenId, ulong sceneGuid, ulong characterId)
        {
            return new SSCharacterEnterBattleOutMessage(this, __characterId__, fubenId, sceneGuid, characterId);
        }

        /// <summary>
        /// 战场有人离开了
        /// </summary>
        public SSCharacterLeaveBattleOutMessage SSCharacterLeaveBattle(ulong __characterId__, int fubenId, ulong sceneGuid, ulong characterId)
        {
            return new SSCharacterLeaveBattleOutMessage(this, __characterId__, fubenId, sceneGuid, characterId);
        }

        /// <summary>
        /// 某个战场结束了
        /// </summary>
        public SSBattleEndOutMessage SSBattleEnd(ulong __characterId__, ulong sceneGuid)
        {
            return new SSBattleEndOutMessage(this, __characterId__, sceneGuid);
        }

        /// <summary>
        /// 查询某个队伍ID的人数
        /// </summary>
        public SSGetTeamCountOutMessage SSGetTeamCount(ulong __characterId__, Uint64Array teamIds)
        {
            return new SSGetTeamCountOutMessage(this, __characterId__, teamIds);
        }

        /// <summary>
        /// 获得本队伍其他人的SceneData
        /// </summary>
        public SSGetTeamSceneDataOutMessage SSGetTeamSceneData(ulong __characterId__, ulong characterId)
        {
            return new SSGetTeamSceneDataOutMessage(this, __characterId__, characterId);
        }

        /// <summary>
        /// 获得本队伍其他人的Id
        /// </summary>
        public SSGetTeamCharactersOutMessage SSGetTeamCharacters(ulong __characterId__, ulong characterId)
        {
            return new SSGetTeamCharactersOutMessage(this, __characterId__, characterId);
        }

        /// <summary>
        /// 玩家切换场景了
        /// </summary>
        public SSNotifyPlayerChangeSceneOutMessage SSNotifyPlayerChangeScene(ulong __characterId__, int serverId, ulong guid, int sceneId, int level, int fightPoint)
        {
            return new SSNotifyPlayerChangeSceneOutMessage(this, __characterId__, serverId, guid, sceneId, level, fightPoint);
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
        /// Scene通知Team，攻城战结果
        /// </summary>
        public NotifyAllianceWarResultOutMessage NotifyAllianceWarResult(ulong __characterId__, AllianceWarResult result)
        {
            return new NotifyAllianceWarResultOutMessage(this, __characterId__, result);
        }

        /// <summary>
        /// 请求攻城战相关信息
        /// </summary>
        public QueryAllianceWarInfoOutMessage QueryAllianceWarInfo(ulong __characterId__, int placeholder)
        {
            return new QueryAllianceWarInfoOutMessage(this, __characterId__, placeholder);
        }

        /// <summary>
        /// 拍卖行：添加道具
        /// </summary>
        public SSOnItemAuctionOutMessage SSOnItemAuction(ulong __characterId__, int serverId, ulong characterId, string characterName, ItemBaseData item, int needType, int needCount, long itemGuid)
        {
            return new SSOnItemAuctionOutMessage(this, __characterId__, serverId, characterId, characterName, item, needType, needCount, itemGuid);
        }

        /// <summary>
        /// 拍卖行：取消交易道具
        /// </summary>
        public SSDownItemAuctionOutMessage SSDownItemAuction(ulong __characterId__, int serverId, ulong characterId, long itemGuid)
        {
            return new SSDownItemAuctionOutMessage(this, __characterId__, serverId, characterId, itemGuid);
        }

        /// <summary>
        /// 拍卖行：检查是否存在道具
        /// </summary>
        public SSSelectItemAuctionOutMessage SSSelectItemAuction(ulong __characterId__, int serverId, ulong characterId, long itemManagerId)
        {
            return new SSSelectItemAuctionOutMessage(this, __characterId__, serverId, characterId, itemManagerId);
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
        /// 队伍：自动匹配  logic广播玩家： 是否接受自动入队 flag标记
        /// </summary>
        public SSGetCharacterTeamFlagOutMessage SSGetCharacterTeamFlag(ulong __characterId__, ulong characterId, bool flag)
        {
            return new SSGetCharacterTeamFlagOutMessage(this, __characterId__, characterId, flag);
        }

        /// <summary>
        /// 战盟仓库捐赠
        /// </summary>
        public SSAllianceDepotDonateOutMessage SSAllianceDepotDonate(ulong __characterId__, int serverId, ulong characterId, string characterName, ItemBaseData item)
        {
            return new SSAllianceDepotDonateOutMessage(this, __characterId__, serverId, characterId, characterName, item);
        }

        /// <summary>
        /// 战盟仓库取出
        /// </summary>
        public SSAllianceDepotTakeOutOutMessage SSAllianceDepotTakeOut(ulong __characterId__, int serverId, ulong characterId, string characterName, int bagIndex, int itemId)
        {
            return new SSAllianceDepotTakeOutOutMessage(this, __characterId__, serverId, characterId, characterName, bagIndex, itemId);
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
        /// 玩家占领某个包含矿脉的scene
        /// </summary>
        public PlayerHoldLodeOutMessage PlayerHoldLode(ulong __characterId__, int serverId, int allianceId, int sceneId)
        {
            return new PlayerHoldLodeOutMessage(this, __characterId__, serverId, allianceId, sceneId);
        }

        /// <summary>
        /// 玩家挖矿
        /// </summary>
        public PlayerCollectLodeOutMessage PlayerCollectLode(ulong __characterId__, int serverId, ulong characterId, int allianceId, int sceneId, int LodeId, int addScore, FieldRankBaseData baseData, int meritPoint)
        {
            return new PlayerCollectLodeOutMessage(this, __characterId__, serverId, characterId, allianceId, sceneId, LodeId, addScore, baseData, meritPoint);
        }

        /// <summary>
        /// 请求矿脉信息
        /// </summary>
        public ApplyHoldLodeOutMessage ApplyHoldLode(ulong __characterId__, int serverId, int sceneId)
        {
            return new ApplyHoldLodeOutMessage(this, __characterId__, serverId, sceneId);
        }

        /// <summary>
        /// </summary>
        public SSApplyFieldActivityRewardOutMessage SSApplyFieldActivityReward(ulong __characterId__, int serverId, int allianceId, ulong characterId, int missionId, int score, int addScore, FieldRankBaseData data)
        {
            return new SSApplyFieldActivityRewardOutMessage(this, __characterId__, serverId, allianceId, characterId, missionId, score, addScore, data);
        }

        /// <summary>
        /// </summary>
        public SSSyncTeamMemberLevelChangeOutMessage SSSyncTeamMemberLevelChange(ulong __characterId__, int serverId, ulong characterId, ulong teamId, int reborn, int level)
        {
            return new SSSyncTeamMemberLevelChangeOutMessage(this, __characterId__, serverId, characterId, teamId, reborn, level);
        }

        /// <summary>
        /// </summary>
        public SSAddAllianceContributionOutMessage SSAddAllianceContribution(ulong __characterId__, int serverId, ulong characterId, int allianceId, int Contribution)
        {
            return new SSAddAllianceContributionOutMessage(this, __characterId__, serverId, characterId, allianceId, Contribution);
        }

        /// <summary>
        /// 通知修改玩家名字
        /// </summary>
        public NodifyModifyPlayerNameOutMessage NodifyModifyPlayerName(ulong __characterId__, int serverId, ulong characterId, ulong teamId, string modifyName)
        {
            return new NodifyModifyPlayerNameOutMessage(this, __characterId__, serverId, characterId, teamId, modifyName);
        }

        /// <summary>
        /// </summary>
        public NodifyModifyAllianceMemberNameOutMessage NodifyModifyAllianceMemberName(ulong __characterId__, int serverId, ulong characterId, int allianceId, string modifyName)
        {
            return new NodifyModifyAllianceMemberNameOutMessage(this, __characterId__, serverId, characterId, allianceId, modifyName);
        }

        /// <summary>
        /// 服务器内的组队操作
        /// </summary>
        public SSLeaveTeamOutMessage SSLeaveTeam(ulong __characterId__, int placeholder)
        {
            return new SSLeaveTeamOutMessage(this, __characterId__, placeholder);
        }

        protected override object GetPublishData(uint p, byte[] list)
        {
            switch (p)
            {
                case 7000:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_PrepareDataForEnterGame_ARG_int32_serverId__>(ms);
                    }
                    break;
                case 7001:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_PrepareDataForCreateCharacter_ARG_int32_type__>(ms);
                    }
                    break;
                case 7002:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_PrepareDataForCommonUse_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 7003:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_PrepareDataForLogout_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 7030:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SBGetAllOnlineCharacterInServer_ARG_int32_serverId__>(ms);
                    }
                    break;
                case 7031:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_CheckConnected_ARG_uint64_characterId__>(ms);
                    }
                    break;
                case 7032:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_CheckLost_ARG_uint64_characterId__>(ms);
                    }
                    break;
                case 7033:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_QueryStatus_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 7040:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_QueryBrokerStatus_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 7046:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_TeamDungeonLeaderChangeScene_ARG_uint64_sceneGuid__>(ms);
                    }
                    break;
                case 7053:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_LeaveDungeon_ARG_uint64_characterId__>(ms);
                    }
                    break;
                case 7058:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_ServerGMCommand_ARG_string_cmd_string_param__>(ms);
                    }
                    break;
                case 7061:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_Logic2TeamCreateAlliance_ARG_int32_serverId_uint64_guid_string_name_int32_state__>(ms);
                    }
                    break;
                case 7062:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_Logic2TeamAllianceOperation_ARG_int32_type_int32_value__>(ms);
                    }
                    break;
                case 7063:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_Logic2TeamAllianceOperationCharacter_ARG_int32_type_string_name_int32_allianceId_uint64_guid__>(ms);
                    }
                    break;
                case 7072:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_GetAllianceCharacterData_ARG_int32_serverId_uint64_guid_int32_level__>(ms);
                    }
                    break;
                case 7073:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SSGetTeamData_ARG_uint64_guid__>(ms);
                    }
                    break;
                case 7074:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SSGetAllianceData_ARG_int32_serverId__>(ms);
                    }
                    break;
                case 7076:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_Logic2TeamDonationAllianceItem_ARG_int32_serverId_int32_type_string_name__>(ms);
                    }
                    break;
                case 7300:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SSGetAllianceName_ARG_int32_allianceId__>(ms);
                    }
                    break;
                case 7301:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SSGetAlliance_ARG_int32_serverId_int32_startIndex_int32_EndIndex_string_name__>(ms);
                    }
                    break;
                case 7302:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_GMChangeJurisdiction_ARG_int32_serverId_int32_allianceId_uint64_guid_int32_type__>(ms);
                    }
                    break;
                case 7303:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_GMChangeAllianceNotice_ARG_int32_serverId_int32_allianceId_string_Content__>(ms);
                    }
                    break;
                case 7304:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_GMDelAllicance_ARG_int32_allianceId__>(ms);
                    }
                    break;
                case 7083:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_BroadcastExchangeItem_ARG_uint64_characterId_string_characterName_ItemBaseData_item_int32_needCount_int32_ContinueMinutes__>(ms);
                    }
                    break;
                case 7084:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_GetExchangeItem_ARG_uint64_characterId_int32_level_int32_count_int32_type__>(ms);
                    }
                    break;
                case 7085:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_CancelExchangeItem_ARG_uint64_characterId_int64_itemGuid__>(ms);
                    }
                    break;
                case 7086:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SSApplyGroupShopItems_ARG_Int32Array_types_Int64ArrayList_items_int32_profession__>(ms);
                    }
                    break;
                case 7087:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SSBuyGroupShopItem_ARG_int64_guid_int32_count__>(ms);
                    }
                    break;
                case 7088:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SSGetBuyedGroupShopItems_ARG_Int64Array_buyed__>(ms);
                    }
                    break;
                case 7089:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SSGetGroupShopHistory_ARG_Int64Array_buyed_Int64Array_history__>(ms);
                    }
                    break;
                case 7092:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SSCharacterEnterBattle_ARG_int32_fubenId_uint64_sceneGuid_uint64_characterId__>(ms);
                    }
                    break;
                case 7093:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SSCharacterLeaveBattle_ARG_int32_fubenId_uint64_sceneGuid_uint64_characterId__>(ms);
                    }
                    break;
                case 7094:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SSBattleEnd_ARG_uint64_sceneGuid__>(ms);
                    }
                    break;
                case 7095:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SSGetTeamCount_ARG_Uint64Array_teamIds__>(ms);
                    }
                    break;
                case 7096:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SSGetTeamSceneData_ARG_uint64_characterId__>(ms);
                    }
                    break;
                case 7097:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SSGetTeamCharacters_ARG_uint64_characterId__>(ms);
                    }
                    break;
                case 7098:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SSNotifyPlayerChangeScene_ARG_int32_serverId_uint64_guid_int32_sceneId_int32_level_int32_fightPoint__>(ms);
                    }
                    break;
                case 7099:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_ReadyToEnter_ARG_int32_placeholder__>(ms);
                    }
                    break;
                case 7101:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_UpdateServer_ARG_int32_placeholder__>(ms);
                    }
                    break;
                case 7111:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_NotifyAllianceWarResult_ARG_AllianceWarResult_result__>(ms);
                    }
                    break;
                case 7112:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_QueryAllianceWarInfo_ARG_int32_placeholder__>(ms);
                    }
                    break;
                case 7114:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SSOnItemAuction_ARG_int32_serverId_uint64_characterId_string_characterName_ItemBaseData_item_int32_needType_int32_needCount_int64_itemGuid__>(ms);
                    }
                    break;
                case 7115:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SSDownItemAuction_ARG_int32_serverId_uint64_characterId_int64_itemGuid__>(ms);
                    }
                    break;
                case 7117:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SSSelectItemAuction_ARG_int32_serverId_uint64_characterId_int64_itemManagerId__>(ms);
                    }
                    break;
                case 7500:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SBCleanClientCharacterData_ARG_uint64_clientId_uint64_characterId__>(ms);
                    }
                    break;
                case 7501:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SSNotifyCharacterOnConnet_ARG_uint64_clientId_uint64_characterId__>(ms);
                    }
                    break;
                case 7508:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SSGetCharacterTeamFlag_ARG_uint64_characterId_bool_flag__>(ms);
                    }
                    break;
                case 7520:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SSAllianceDepotDonate_ARG_int32_serverId_uint64_characterId_string_characterName_ItemBaseData_item__>(ms);
                    }
                    break;
                case 7523:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SSAllianceDepotTakeOut_ARG_int32_serverId_uint64_characterId_string_characterName_int32_bagIndex_int32_itemId__>(ms);
                    }
                    break;
                case 7900:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_GMCommand_ARG_StringArray_commonds__>(ms);
                    }
                    break;
                case 7901:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_PlayerHoldLode_ARG_int32_serverId_int32_allianceId_int32_sceneId__>(ms);
                    }
                    break;
                case 7902:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_PlayerCollectLode_ARG_int32_serverId_uint64_characterId_int32_allianceId_int32_sceneId_int32_LodeId_int32_addScore_FieldRankBaseData_baseData_int32_meritPoint__>(ms);
                    }
                    break;
                case 7903:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_ApplyHoldLode_ARG_int32_serverId_int32_sceneId__>(ms);
                    }
                    break;
                case 7906:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SSApplyFieldActivityReward_ARG_int32_serverId_int32_allianceId_uint64_characterId_int32_missionId_int32_score_int32_addScore_FieldRankBaseData_data__>(ms);
                    }
                    break;
                case 7907:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SSSyncTeamMemberLevelChange_ARG_int32_serverId_uint64_characterId_uint64_teamId_int32_reborn_int32_level__>(ms);
                    }
                    break;
                case 7909:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SSAddAllianceContribution_ARG_int32_serverId_uint64_characterId_int32_allianceId_int32_Contribution__>(ms);
                    }
                    break;
                case 7915:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_NodifyModifyPlayerName_ARG_int32_serverId_uint64_characterId_uint64_teamId_string_modifyName__>(ms);
                    }
                    break;
                case 7917:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_NodifyModifyAllianceMemberName_ARG_int32_serverId_uint64_characterId_int32_allianceId_string_modifyName__>(ms);
                    }
                    break;
                case 7918:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Team_SSLeaveTeam_ARG_int32_placeholder__>(ms);
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
        /// 组队信息通知
        /// </summary>
        public object NotifyTeamMessage(ulong __characterId__, ulong __clientId__, int type, ulong teamId, ulong characterId)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 7042;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Team;

            var __data__ = new __RPC_Team_NotifyTeamMessage_ARG_int32_type_uint64_teamId_uint64_characterId__();
            __data__.Type=type;
            __data__.TeamId=teamId;
            __data__.CharacterId=characterId;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知排队成功
        /// </summary>
        public object MatchingSuccess(ulong __characterId__, ulong __clientId__, int queueId)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 7049;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Team;

            var __data__ = new __RPC_Team_MatchingSuccess_ARG_int32_queueId__();
            __data__.QueueId=queueId;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知排队移出
        /// </summary>
        public object NotifyMatchingData(ulong __characterId__, ulong __clientId__, QueueInfo queueInfo)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 7050;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Team;

            var __data__ = new __RPC_Team_NotifyMatchingData_ARG_QueueInfo_queueInfo__();
            __data__.QueueInfo=queueInfo;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 匹配结果失败通知
        /// </summary>
        public object TeamServerMessage(ulong __characterId__, ulong __clientId__, int resultType, string args)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 7052;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Team;

            var __data__ = new __RPC_Team_TeamServerMessage_ARG_int32_resultType_string_args__();
            __data__.ResultType=resultType;
            __data__.Args=args;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 请求提示
        /// </summary>
        public object SyncTeamEnterFuben(ulong __characterId__, ulong __clientId__, int fubenId)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 7056;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Team;

            var __data__ = new __RPC_Team_SyncTeamEnterFuben_ARG_int32_fubenId__();
            __data__.FubenId=fubenId;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 队员希望邀请
        /// </summary>
        public object MemberWantInvite(ulong __characterId__, ulong __clientId__, int type, string memberName, int memberJob, int memberLevel, string toName, ulong toId)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 7060;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Team;

            var __data__ = new __RPC_Team_MemberWantInvite_ARG_int32_type_string_memberName_int32_memberJob_int32_memberLevel_string_toName_uint64_toId__();
            __data__.Type=type;
            __data__.MemberName=memberName;
            __data__.MemberJob=memberJob;
            __data__.MemberLevel=memberLevel;
            __data__.ToName=toName;
            __data__.ToId=toId;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 战盟信息通知 			type：0=name1邀请您加入name2的战盟
        /// </summary>
        public object SyncAllianceMessage(ulong __characterId__, ulong __clientId__, int type, string name1, int allianceId, string name2)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 7064;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Team;

            var __data__ = new __RPC_Team_SyncAllianceMessage_ARG_int32_type_string_name1_int32_allianceId_string_name2__();
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
        /// 通知玩家的排队状态
        /// </summary>
        public object SendMatchingMessage(ulong __characterId__, ulong __clientId__, int NowCount)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 7075;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Team;

            var __data__ = new __RPC_Team_SendMatchingMessage_ARG_int32_NowCount__();
            __data__.NowCount=NowCount;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 升级Buff
        /// CS int32  				UpgradeAllianceBuff(int32 allianceId,int32 buffId)=7077;
        /// 战盟信息通知 		type：0=name1邀请您加入name2的战盟 1=同意申请 2拒绝申请
        /// </summary>
        public object TeamSyncAllianceMessage(ulong __characterId__, ulong __clientId__, int type, string name1, int allianceId, string name2)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 7078;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Team;

            var __data__ = new __RPC_Team_TeamSyncAllianceMessage_ARG_int32_type_string_name1_int32_allianceId_string_name2__();
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
        /// 战盟信息改变  type = 0 说明战盟升级：param1等级 param2总资金
        /// </summary>
        public object ChangeAllianceData(ulong __characterId__, ulong __clientId__, int type, int param1, int param2)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 7080;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Team;

            var __data__ = new __RPC_Team_ChangeAllianceData_ARG_int32_type_int32_param1_int32_param2__();
            __data__.Type=type;
            __data__.Param1=param1;
            __data__.Param2=param2;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 缓存许愿池的团购信息
        /// </summary>
        public object SendGroupMessage(ulong __characterId__, ulong __clientId__, StringArray contents)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 7100;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Team;

            var __data__ = new __RPC_Team_SendGroupMessage_ARG_StringArray_contents__();
            __data__.Contents=contents;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知组队进入信息
        /// </summary>
        public object NotifyQueueMessage(ulong __characterId__, ulong __clientId__, TeamCharacterMessage tcm)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 7102;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Team;

            var __data__ = new __RPC_Team_NotifyQueueMessage_ARG_TeamCharacterMessage_tcm__();
            __data__.Tcm=tcm;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知某人进入结果
        /// </summary>
        public object NotifyQueueResult(ulong __characterId__, ulong __clientId__, ulong characterId, int result)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 7103;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Team;

            var __data__ = new __RPC_Team_NotifyQueueResult_ARG_uint64_characterId_int32_result__();
            __data__.CharacterId=characterId;
            __data__.Result=result;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 服务器主动推送城主信息
        /// </summary>
        public object NotifyAllianceWarOccupantData(uint __serverId__, AllianceWarOccupantData data)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 7105;
            desc.PacketId = __serverId__;
            desc.Type = (int)MessageType.SCServer;
            desc.ServiceType = (int) ServiceType.Team;

            var __data__ = new __RPC_Team_NotifyAllianceWarOccupantData_ARG_AllianceWarOccupantData_data__();
            __data__.Data=data;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 服务器主动推送进攻方信息
        /// </summary>
        public object NotifyAllianceWarChallengerData(uint __serverId__, AllianceWarChallengerData data)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 7107;
            desc.PacketId = __serverId__;
            desc.Type = (int)MessageType.SCServer;
            desc.ServiceType = (int) ServiceType.Team;

            var __data__ = new __RPC_Team_NotifyAllianceWarChallengerData_ARG_AllianceWarChallengerData_data__();
            __data__.Data=data;


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
        public object SyncAllianceChatMessage(ulong __characterId__, ulong __clientId__, int chatType, ulong characterId, string characterName, ChatMessageContent content)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 7113;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Team;

            var __data__ = new __RPC_Team_SyncAllianceChatMessage_ARG_int32_chatType_uint64_characterId_string_characterName_ChatMessageContent_content__();
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
        /// 队伍：自动匹配  自动匹配结束，通知client
        /// </summary>
        public object AutoMatchEnd(ulong __characterId__, ulong __clientId__, int MathchState)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 7505;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Team;

            var __data__ = new __RPC_Team_AutoMatchEnd_ARG_int32_MathchState__();
            __data__.MathchState=MathchState;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 队伍：自动匹配  自动匹配 状态发生变化
        /// </summary>
        public object AutoMatchStateChange(ulong __characterId__, ulong __clientId__, int MathchState)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 7507;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Team;

            var __data__ = new __RPC_Team_AutoMatchStateChange_ARG_int32_MathchState__();
            __data__.MathchState=MathchState;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// </summary>
        public object NotifyChangetTeamTarget(ulong __characterId__, ulong __clientId__, int type, int targetID, int levelMini, int levelMax, int readTableId)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 7510;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Team;

            var __data__ = new __RPC_Team_NotifyChangetTeamTarget_ARG_int32_type_int32_targetID_int32_levelMini_int32_levelMax_int32_readTableId__();
            __data__.Type=type;
            __data__.TargetID=targetID;
            __data__.LevelMini=levelMini;
            __data__.LevelMax=levelMax;
            __data__.ReadTableId=readTableId;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 队伍：同步队员sceneGuid
        /// </summary>
        public object NotifyTeamScenGuid(ulong __characterId__, ulong __clientId__, ulong characterId, ulong changeCharacterId, ulong sceneGuid)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 7514;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Team;

            var __data__ = new __RPC_Team_NotifyTeamScenGuid_ARG_uint64_characterId_uint64_changeCharacterId_uint64_sceneGuid__();
            __data__.CharacterId=characterId;
            __data__.ChangeCharacterId=changeCharacterId;
            __data__.SceneGuid=sceneGuid;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// </summary>
        public object TeamApplyListSync(ulong __characterId__, ulong __clientId__, ulong characterId, bool state)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 7516;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Team;

            var __data__ = new __RPC_Team_TeamApplyListSync_ARG_uint64_characterId_bool_state__();
            __data__.CharacterId=characterId;
            __data__.State=state;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// </summary>
        public object SCSyncTeamMemberLevelChange(ulong __characterId__, ulong __clientId__, ulong characterId, ulong changeCharacterId, int reborn, int level)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 7908;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Team;

            var __data__ = new __RPC_Team_SCSyncTeamMemberLevelChange_ARG_uint64_characterId_uint64_changeCharacterId_int32_reborn_int32_level__();
            __data__.CharacterId=characterId;
            __data__.ChangeCharacterId=changeCharacterId;
            __data__.Reborn=reborn;
            __data__.Level=level;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// </summary>
        public object NoticeTeamMemberError(ulong __characterId__, ulong __clientId__, string msgInfo)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 7910;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Team;

            var __data__ = new __RPC_Team_NoticeTeamMemberError_ARG_string_msgInfo__();
            __data__.MsgInfo=msgInfo;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知全服，战旗活动结算
        /// </summary>
        public object NotifyFieldFinal(uint __serverId__, MsgWarFlagInfoList msg)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 7911;
            desc.PacketId = __serverId__;
            desc.Type = (int)MessageType.SCServer;
            desc.ServiceType = (int) ServiceType.Team;

            var __data__ = new __RPC_Team_NotifyFieldFinal_ARG_MsgWarFlagInfoList_msg__();
            __data__.Msg=msg;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// </summary>
        public object SCNotifyAllianceActiveTask(ulong __characterId__, ulong __clientId__, DBActiveTask msg)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 7912;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Team;

            var __data__ = new __RPC_Team_SCNotifyAllianceActiveTask_ARG_DBActiveTask_msg__();
            __data__.Msg=msg;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// </summary>
        public object SCNotifyPlayerExitAlliance(ulong __characterId__, ulong __clientId__, ulong exitplayerid, string name, bool bChange, ulong leaderId, string leaderName)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 7914;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Team;

            var __data__ = new __RPC_Team_SCNotifyPlayerExitAlliance_ARG_uint64_exitplayerid_string_name_bool_bChange_uint64_leaderId_string_leaderName__();
            __data__.Exitplayerid=exitplayerid;
            __data__.Name=name;
            __data__.BChange=bChange;
            __data__.LeaderId=leaderId;
            __data__.LeaderName=leaderName;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// </summary>
        public object NodifyTeamMemberPlayerNameChange(ulong __characterId__, ulong __clientId__, ulong characterId, ulong changeCharacterId, string changeName)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 7916;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Team;

            var __data__ = new __RPC_Team_NodifyTeamMemberPlayerNameChange_ARG_uint64_characterId_uint64_changeCharacterId_string_changeName__();
            __data__.CharacterId=characterId;
            __data__.ChangeCharacterId=changeCharacterId;
            __data__.ChangeName=changeName;


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
            : base(sender, ServiceType.Team, 7000, (int)MessageType.SS)
        {
            Request = new __RPC_Team_PrepareDataForEnterGame_ARG_int32_serverId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;

        }

        public __RPC_Team_PrepareDataForEnterGame_ARG_int32_serverId__ Request { get; private set; }

            private __RPC_Team_PrepareDataForEnterGame_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_PrepareDataForEnterGame_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class PrepareDataForCreateCharacterOutMessage : OutMessage
    {
        public PrepareDataForCreateCharacterOutMessage(ClientAgentBase sender, ulong __characterId__, int type)
            : base(sender, ServiceType.Team, 7001, (int)MessageType.SS)
        {
            Request = new __RPC_Team_PrepareDataForCreateCharacter_ARG_int32_type__();
            mMessage.CharacterId = __characterId__;
            Request.Type=type;

        }

        public __RPC_Team_PrepareDataForCreateCharacter_ARG_int32_type__ Request { get; private set; }

            private __RPC_Team_PrepareDataForCreateCharacter_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_PrepareDataForCreateCharacter_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class PrepareDataForCommonUseOutMessage : OutMessage
    {
        public PrepareDataForCommonUseOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Team, 7002, (int)MessageType.SS)
        {
            Request = new __RPC_Team_PrepareDataForCommonUse_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Team_PrepareDataForCommonUse_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Team_PrepareDataForCommonUse_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_PrepareDataForCommonUse_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class PrepareDataForLogoutOutMessage : OutMessage
    {
        public PrepareDataForLogoutOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Team, 7003, (int)MessageType.SS)
        {
            Request = new __RPC_Team_PrepareDataForLogout_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Team_PrepareDataForLogout_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Team_PrepareDataForLogout_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_PrepareDataForLogout_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SBGetAllOnlineCharacterInServerOutMessage : OutMessage
    {
        public SBGetAllOnlineCharacterInServerOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId)
            : base(sender, ServiceType.Team, 7030, (int)MessageType.SB)
        {
            Request = new __RPC_Team_SBGetAllOnlineCharacterInServer_ARG_int32_serverId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;

        }

        public __RPC_Team_SBGetAllOnlineCharacterInServer_ARG_int32_serverId__ Request { get; private set; }

            private __RPC_Team_SBGetAllOnlineCharacterInServer_RET_Uint64Array__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_SBGetAllOnlineCharacterInServer_RET_Uint64Array__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class CheckConnectedOutMessage : OutMessage
    {
        public CheckConnectedOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId)
            : base(sender, ServiceType.Team, 7031, (int)MessageType.SS)
        {
            Request = new __RPC_Team_CheckConnected_ARG_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;

        }

        public __RPC_Team_CheckConnected_ARG_uint64_characterId__ Request { get; private set; }

            private __RPC_Team_CheckConnected_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_CheckConnected_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class CheckLostOutMessage : OutMessage
    {
        public CheckLostOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId)
            : base(sender, ServiceType.Team, 7032, (int)MessageType.SS)
        {
            Request = new __RPC_Team_CheckLost_ARG_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;

        }

        public __RPC_Team_CheckLost_ARG_uint64_characterId__ Request { get; private set; }

            private __RPC_Team_CheckLost_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_CheckLost_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class QueryStatusOutMessage : OutMessage
    {
        public QueryStatusOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Team, 7033, (int)MessageType.SS)
        {
            Request = new __RPC_Team_QueryStatus_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Team_QueryStatus_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Team_QueryStatus_RET_TeamServerStatus__ mResponse;
            public TeamServerStatus Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Team_QueryStatus_RET_TeamServerStatus__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class QueryBrokerStatusOutMessage : OutMessage
    {
        public QueryBrokerStatusOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Team, 7040, (int)MessageType.SB)
        {
            Request = new __RPC_Team_QueryBrokerStatus_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Team_QueryBrokerStatus_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Team_QueryBrokerStatus_RET_CommonBrokerStatus__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_QueryBrokerStatus_RET_CommonBrokerStatus__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class TeamDungeonLeaderChangeSceneOutMessage : OutMessage
    {
        public TeamDungeonLeaderChangeSceneOutMessage(ClientAgentBase sender, ulong __characterId__, ulong sceneGuid)
            : base(sender, ServiceType.Team, 7046, (int)MessageType.SS)
        {
            Request = new __RPC_Team_TeamDungeonLeaderChangeScene_ARG_uint64_sceneGuid__();
            mMessage.CharacterId = __characterId__;
            Request.SceneGuid=sceneGuid;

        }

        public __RPC_Team_TeamDungeonLeaderChangeScene_ARG_uint64_sceneGuid__ Request { get; private set; }

            private __RPC_Team_TeamDungeonLeaderChangeScene_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_TeamDungeonLeaderChangeScene_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class LeaveDungeonOutMessage : OutMessage
    {
        public LeaveDungeonOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId)
            : base(sender, ServiceType.Team, 7053, (int)MessageType.SS)
        {
            Request = new __RPC_Team_LeaveDungeon_ARG_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;

        }

        public __RPC_Team_LeaveDungeon_ARG_uint64_characterId__ Request { get; private set; }

            private __RPC_Team_LeaveDungeon_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_LeaveDungeon_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class ServerGMCommandOutMessage : OutMessage
    {
        public ServerGMCommandOutMessage(ClientAgentBase sender, ulong __characterId__, string cmd, string param)
            : base(sender, ServiceType.Team, 7058, (int)MessageType.SAS)
        {
            Request = new __RPC_Team_ServerGMCommand_ARG_string_cmd_string_param__();
            mMessage.CharacterId = __characterId__;
            Request.Cmd=cmd;
            Request.Param=param;

        }

        public __RPC_Team_ServerGMCommand_ARG_string_cmd_string_param__ Request { get; private set; }


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

    public class Logic2TeamCreateAllianceOutMessage : OutMessage
    {
        public Logic2TeamCreateAllianceOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, ulong guid, string name, int state)
            : base(sender, ServiceType.Team, 7061, (int)MessageType.SS)
        {
            Request = new __RPC_Team_Logic2TeamCreateAlliance_ARG_int32_serverId_uint64_guid_string_name_int32_state__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.Guid=guid;
            Request.Name=name;
            Request.State=state;

        }

        public __RPC_Team_Logic2TeamCreateAlliance_ARG_int32_serverId_uint64_guid_string_name_int32_state__ Request { get; private set; }

            private __RPC_Team_Logic2TeamCreateAlliance_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_Logic2TeamCreateAlliance_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class Logic2TeamAllianceOperationOutMessage : OutMessage
    {
        public Logic2TeamAllianceOperationOutMessage(ClientAgentBase sender, ulong __characterId__, int type, int value)
            : base(sender, ServiceType.Team, 7062, (int)MessageType.SS)
        {
            Request = new __RPC_Team_Logic2TeamAllianceOperation_ARG_int32_type_int32_value__();
            mMessage.CharacterId = __characterId__;
            Request.Type=type;
            Request.Value=value;

        }

        public __RPC_Team_Logic2TeamAllianceOperation_ARG_int32_type_int32_value__ Request { get; private set; }

            private __RPC_Team_Logic2TeamAllianceOperation_RET_string__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_Logic2TeamAllianceOperation_RET_string__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class Logic2TeamAllianceOperationCharacterOutMessage : OutMessage
    {
        public Logic2TeamAllianceOperationCharacterOutMessage(ClientAgentBase sender, ulong __characterId__, int type, string name, int allianceId, ulong guid)
            : base(sender, ServiceType.Team, 7063, (int)MessageType.SS)
        {
            Request = new __RPC_Team_Logic2TeamAllianceOperationCharacter_ARG_int32_type_string_name_int32_allianceId_uint64_guid__();
            mMessage.CharacterId = __characterId__;
            Request.Type=type;
            Request.Name=name;
            Request.AllianceId=allianceId;
            Request.Guid=guid;

        }

        public __RPC_Team_Logic2TeamAllianceOperationCharacter_ARG_int32_type_string_name_int32_allianceId_uint64_guid__ Request { get; private set; }

            private __RPC_Team_Logic2TeamAllianceOperationCharacter_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_Logic2TeamAllianceOperationCharacter_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetAllianceCharacterDataOutMessage : OutMessage
    {
        public GetAllianceCharacterDataOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, ulong guid, int level)
            : base(sender, ServiceType.Team, 7072, (int)MessageType.SS)
        {
            Request = new __RPC_Team_GetAllianceCharacterData_ARG_int32_serverId_uint64_guid_int32_level__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.Guid=guid;
            Request.Level=level;

        }

        public __RPC_Team_GetAllianceCharacterData_ARG_int32_serverId_uint64_guid_int32_level__ Request { get; private set; }

            private __RPC_Team_GetAllianceCharacterData_RET_AllianceCharacterLogicData__ mResponse;
            public AllianceCharacterLogicData Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Team_GetAllianceCharacterData_RET_AllianceCharacterLogicData__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSGetTeamDataOutMessage : OutMessage
    {
        public SSGetTeamDataOutMessage(ClientAgentBase sender, ulong __characterId__, ulong guid)
            : base(sender, ServiceType.Team, 7073, (int)MessageType.SS)
        {
            Request = new __RPC_Team_SSGetTeamData_ARG_uint64_guid__();
            mMessage.CharacterId = __characterId__;
            Request.Guid=guid;

        }

        public __RPC_Team_SSGetTeamData_ARG_uint64_guid__ Request { get; private set; }

            private __RPC_Team_SSGetTeamData_RET_MsgTeamState__ mResponse;
            public MsgTeamState Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Team_SSGetTeamData_RET_MsgTeamState__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSGetAllianceDataOutMessage : OutMessage
    {
        public SSGetAllianceDataOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId)
            : base(sender, ServiceType.Team, 7074, (int)MessageType.SS)
        {
            Request = new __RPC_Team_SSGetAllianceData_ARG_int32_serverId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;

        }

        public __RPC_Team_SSGetAllianceData_ARG_int32_serverId__ Request { get; private set; }

            private __RPC_Team_SSGetAllianceData_RET_AllianceDataToScene__ mResponse;
            public AllianceDataToScene Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Team_SSGetAllianceData_RET_AllianceDataToScene__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class Logic2TeamDonationAllianceItemOutMessage : OutMessage
    {
        public Logic2TeamDonationAllianceItemOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int type, string name)
            : base(sender, ServiceType.Team, 7076, (int)MessageType.SS)
        {
            Request = new __RPC_Team_Logic2TeamDonationAllianceItem_ARG_int32_serverId_int32_type_string_name__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.Type=type;
            Request.Name=name;

        }

        public __RPC_Team_Logic2TeamDonationAllianceItem_ARG_int32_serverId_int32_type_string_name__ Request { get; private set; }

            private __RPC_Team_Logic2TeamDonationAllianceItem_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_Logic2TeamDonationAllianceItem_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSGetAllianceNameOutMessage : OutMessage
    {
        public SSGetAllianceNameOutMessage(ClientAgentBase sender, ulong __characterId__, int allianceId)
            : base(sender, ServiceType.Team, 7300, (int)MessageType.SS)
        {
            Request = new __RPC_Team_SSGetAllianceName_ARG_int32_allianceId__();
            mMessage.CharacterId = __characterId__;
            Request.AllianceId=allianceId;

        }

        public __RPC_Team_SSGetAllianceName_ARG_int32_allianceId__ Request { get; private set; }

            private __RPC_Team_SSGetAllianceName_RET_string__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_SSGetAllianceName_RET_string__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSGetAllianceOutMessage : OutMessage
    {
        public SSGetAllianceOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int startIndex, int EndIndex, string name)
            : base(sender, ServiceType.Team, 7301, (int)MessageType.SS)
        {
            Request = new __RPC_Team_SSGetAlliance_ARG_int32_serverId_int32_startIndex_int32_EndIndex_string_name__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.StartIndex=startIndex;
            Request.EndIndex=EndIndex;
            Request.Name=name;

        }

        public __RPC_Team_SSGetAlliance_ARG_int32_serverId_int32_startIndex_int32_EndIndex_string_name__ Request { get; private set; }

            private __RPC_Team_SSGetAlliance_RET_ServerAllianceInfo__ mResponse;
            public ServerAllianceInfo Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Team_SSGetAlliance_RET_ServerAllianceInfo__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GMChangeJurisdictionOutMessage : OutMessage
    {
        public GMChangeJurisdictionOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int allianceId, ulong guid, int type)
            : base(sender, ServiceType.Team, 7302, (int)MessageType.SS)
        {
            Request = new __RPC_Team_GMChangeJurisdiction_ARG_int32_serverId_int32_allianceId_uint64_guid_int32_type__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.AllianceId=allianceId;
            Request.Guid=guid;
            Request.Type=type;

        }

        public __RPC_Team_GMChangeJurisdiction_ARG_int32_serverId_int32_allianceId_uint64_guid_int32_type__ Request { get; private set; }

            private __RPC_Team_GMChangeJurisdiction_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_GMChangeJurisdiction_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GMChangeAllianceNoticeOutMessage : OutMessage
    {
        public GMChangeAllianceNoticeOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int allianceId, string Content)
            : base(sender, ServiceType.Team, 7303, (int)MessageType.SS)
        {
            Request = new __RPC_Team_GMChangeAllianceNotice_ARG_int32_serverId_int32_allianceId_string_Content__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.AllianceId=allianceId;
            Request.Content=Content;

        }

        public __RPC_Team_GMChangeAllianceNotice_ARG_int32_serverId_int32_allianceId_string_Content__ Request { get; private set; }

            private __RPC_Team_GMChangeAllianceNotice_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_GMChangeAllianceNotice_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GMDelAllicanceOutMessage : OutMessage
    {
        public GMDelAllicanceOutMessage(ClientAgentBase sender, ulong __characterId__, int allianceId)
            : base(sender, ServiceType.Team, 7304, (int)MessageType.SS)
        {
            Request = new __RPC_Team_GMDelAllicance_ARG_int32_allianceId__();
            mMessage.CharacterId = __characterId__;
            Request.AllianceId=allianceId;

        }

        public __RPC_Team_GMDelAllicance_ARG_int32_allianceId__ Request { get; private set; }

            private __RPC_Team_GMDelAllicance_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_GMDelAllicance_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class BroadcastExchangeItemOutMessage : OutMessage
    {
        public BroadcastExchangeItemOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId, string characterName, ItemBaseData item, int needCount, int ContinueMinutes)
            : base(sender, ServiceType.Team, 7083, (int)MessageType.SS)
        {
            Request = new __RPC_Team_BroadcastExchangeItem_ARG_uint64_characterId_string_characterName_ItemBaseData_item_int32_needCount_int32_ContinueMinutes__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;
            Request.CharacterName=characterName;
            Request.Item=item;
            Request.NeedCount=needCount;
            Request.ContinueMinutes=ContinueMinutes;

        }

        public __RPC_Team_BroadcastExchangeItem_ARG_uint64_characterId_string_characterName_ItemBaseData_item_int32_needCount_int32_ContinueMinutes__ Request { get; private set; }

            private __RPC_Team_BroadcastExchangeItem_RET_int64__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_BroadcastExchangeItem_RET_int64__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetExchangeItemOutMessage : OutMessage
    {
        public GetExchangeItemOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId, int level, int count, int type)
            : base(sender, ServiceType.Team, 7084, (int)MessageType.SS)
        {
            Request = new __RPC_Team_GetExchangeItem_ARG_uint64_characterId_int32_level_int32_count_int32_type__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;
            Request.Level=level;
            Request.Count=count;
            Request.Type=type;

        }

        public __RPC_Team_GetExchangeItem_ARG_uint64_characterId_int32_level_int32_count_int32_type__ Request { get; private set; }

            private __RPC_Team_GetExchangeItem_RET_StoreBroadcastList__ mResponse;
            public StoreBroadcastList Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Team_GetExchangeItem_RET_StoreBroadcastList__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class CancelExchangeItemOutMessage : OutMessage
    {
        public CancelExchangeItemOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId, long itemGuid)
            : base(sender, ServiceType.Team, 7085, (int)MessageType.SS)
        {
            Request = new __RPC_Team_CancelExchangeItem_ARG_uint64_characterId_int64_itemGuid__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;
            Request.ItemGuid=itemGuid;

        }

        public __RPC_Team_CancelExchangeItem_ARG_uint64_characterId_int64_itemGuid__ Request { get; private set; }


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

    public class SSApplyGroupShopItemsOutMessage : OutMessage
    {
        public SSApplyGroupShopItemsOutMessage(ClientAgentBase sender, ulong __characterId__, Int32Array types, Int64ArrayList items, int profession)
            : base(sender, ServiceType.Team, 7086, (int)MessageType.SS)
        {
            Request = new __RPC_Team_SSApplyGroupShopItems_ARG_Int32Array_types_Int64ArrayList_items_int32_profession__();
            mMessage.CharacterId = __characterId__;
            Request.Types=types;
            Request.Items=items;
            Request.Profession=profession;

        }

        public __RPC_Team_SSApplyGroupShopItems_ARG_Int32Array_types_Int64ArrayList_items_int32_profession__ Request { get; private set; }

            private __RPC_Team_SSApplyGroupShopItems_RET_GroupShopItemAllForServer__ mResponse;
            public GroupShopItemAllForServer Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Team_SSApplyGroupShopItems_RET_GroupShopItemAllForServer__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSBuyGroupShopItemOutMessage : OutMessage
    {
        public SSBuyGroupShopItemOutMessage(ClientAgentBase sender, ulong __characterId__, long guid, int count)
            : base(sender, ServiceType.Team, 7087, (int)MessageType.SS)
        {
            Request = new __RPC_Team_SSBuyGroupShopItem_ARG_int64_guid_int32_count__();
            mMessage.CharacterId = __characterId__;
            Request.Guid=guid;
            Request.Count=count;

        }

        public __RPC_Team_SSBuyGroupShopItem_ARG_int64_guid_int32_count__ Request { get; private set; }

            private __RPC_Team_SSBuyGroupShopItem_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_SSBuyGroupShopItem_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSGetBuyedGroupShopItemsOutMessage : OutMessage
    {
        public SSGetBuyedGroupShopItemsOutMessage(ClientAgentBase sender, ulong __characterId__, Int64Array buyed)
            : base(sender, ServiceType.Team, 7088, (int)MessageType.SS)
        {
            Request = new __RPC_Team_SSGetBuyedGroupShopItems_ARG_Int64Array_buyed__();
            mMessage.CharacterId = __characterId__;
            Request.Buyed=buyed;

        }

        public __RPC_Team_SSGetBuyedGroupShopItems_ARG_Int64Array_buyed__ Request { get; private set; }

            private __RPC_Team_SSGetBuyedGroupShopItems_RET_GroupShopItemAll__ mResponse;
            public GroupShopItemAll Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Team_SSGetBuyedGroupShopItems_RET_GroupShopItemAll__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSGetGroupShopHistoryOutMessage : OutMessage
    {
        public SSGetGroupShopHistoryOutMessage(ClientAgentBase sender, ulong __characterId__, Int64Array buyed, Int64Array history)
            : base(sender, ServiceType.Team, 7089, (int)MessageType.SS)
        {
            Request = new __RPC_Team_SSGetGroupShopHistory_ARG_Int64Array_buyed_Int64Array_history__();
            mMessage.CharacterId = __characterId__;
            Request.Buyed=buyed;
            Request.History=history;

        }

        public __RPC_Team_SSGetGroupShopHistory_ARG_Int64Array_buyed_Int64Array_history__ Request { get; private set; }

            private __RPC_Team_SSGetGroupShopHistory_RET_GroupShopItemAllForServer__ mResponse;
            public GroupShopItemAllForServer Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Team_SSGetGroupShopHistory_RET_GroupShopItemAllForServer__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSCharacterEnterBattleOutMessage : OutMessage
    {
        public SSCharacterEnterBattleOutMessage(ClientAgentBase sender, ulong __characterId__, int fubenId, ulong sceneGuid, ulong characterId)
            : base(sender, ServiceType.Team, 7092, (int)MessageType.SS)
        {
            Request = new __RPC_Team_SSCharacterEnterBattle_ARG_int32_fubenId_uint64_sceneGuid_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.FubenId=fubenId;
            Request.SceneGuid=sceneGuid;
            Request.CharacterId=characterId;

        }

        public __RPC_Team_SSCharacterEnterBattle_ARG_int32_fubenId_uint64_sceneGuid_uint64_characterId__ Request { get; private set; }


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

    public class SSCharacterLeaveBattleOutMessage : OutMessage
    {
        public SSCharacterLeaveBattleOutMessage(ClientAgentBase sender, ulong __characterId__, int fubenId, ulong sceneGuid, ulong characterId)
            : base(sender, ServiceType.Team, 7093, (int)MessageType.SS)
        {
            Request = new __RPC_Team_SSCharacterLeaveBattle_ARG_int32_fubenId_uint64_sceneGuid_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.FubenId=fubenId;
            Request.SceneGuid=sceneGuid;
            Request.CharacterId=characterId;

        }

        public __RPC_Team_SSCharacterLeaveBattle_ARG_int32_fubenId_uint64_sceneGuid_uint64_characterId__ Request { get; private set; }


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

    public class SSBattleEndOutMessage : OutMessage
    {
        public SSBattleEndOutMessage(ClientAgentBase sender, ulong __characterId__, ulong sceneGuid)
            : base(sender, ServiceType.Team, 7094, (int)MessageType.SS)
        {
            Request = new __RPC_Team_SSBattleEnd_ARG_uint64_sceneGuid__();
            mMessage.CharacterId = __characterId__;
            Request.SceneGuid=sceneGuid;

        }

        public __RPC_Team_SSBattleEnd_ARG_uint64_sceneGuid__ Request { get; private set; }


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

    public class SSGetTeamCountOutMessage : OutMessage
    {
        public SSGetTeamCountOutMessage(ClientAgentBase sender, ulong __characterId__, Uint64Array teamIds)
            : base(sender, ServiceType.Team, 7095, (int)MessageType.SS)
        {
            Request = new __RPC_Team_SSGetTeamCount_ARG_Uint64Array_teamIds__();
            mMessage.CharacterId = __characterId__;
            Request.TeamIds=teamIds;

        }

        public __RPC_Team_SSGetTeamCount_ARG_Uint64Array_teamIds__ Request { get; private set; }

            private __RPC_Team_SSGetTeamCount_RET_Int32Array__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_SSGetTeamCount_RET_Int32Array__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSGetTeamSceneDataOutMessage : OutMessage
    {
        public SSGetTeamSceneDataOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId)
            : base(sender, ServiceType.Team, 7096, (int)MessageType.SS)
        {
            Request = new __RPC_Team_SSGetTeamSceneData_ARG_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;

        }

        public __RPC_Team_SSGetTeamSceneData_ARG_uint64_characterId__ Request { get; private set; }

            private __RPC_Team_SSGetTeamSceneData_RET_ObjSceneDataList__ mResponse;
            public ObjSceneDataList Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Team_SSGetTeamSceneData_RET_ObjSceneDataList__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSGetTeamCharactersOutMessage : OutMessage
    {
        public SSGetTeamCharactersOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId)
            : base(sender, ServiceType.Team, 7097, (int)MessageType.SS)
        {
            Request = new __RPC_Team_SSGetTeamCharacters_ARG_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;

        }

        public __RPC_Team_SSGetTeamCharacters_ARG_uint64_characterId__ Request { get; private set; }

            private __RPC_Team_SSGetTeamCharacters_RET_Uint64Array__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_SSGetTeamCharacters_RET_Uint64Array__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSNotifyPlayerChangeSceneOutMessage : OutMessage
    {
        public SSNotifyPlayerChangeSceneOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, ulong guid, int sceneId, int level, int fightPoint)
            : base(sender, ServiceType.Team, 7098, (int)MessageType.SS)
        {
            Request = new __RPC_Team_SSNotifyPlayerChangeScene_ARG_int32_serverId_uint64_guid_int32_sceneId_int32_level_int32_fightPoint__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.Guid=guid;
            Request.SceneId=sceneId;
            Request.Level=level;
            Request.FightPoint=fightPoint;

        }

        public __RPC_Team_SSNotifyPlayerChangeScene_ARG_int32_serverId_uint64_guid_int32_sceneId_int32_level_int32_fightPoint__ Request { get; private set; }


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
            : base(sender, ServiceType.Team, 7099, (int)MessageType.SAS)
        {
            Request = new __RPC_Team_ReadyToEnter_ARG_int32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Team_ReadyToEnter_ARG_int32_placeholder__ Request { get; private set; }

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
                Response.Add(Serializer.Deserialize<__RPC_Team_ReadyToEnter_RET_int32__>(ms).ReturnValue);
            }
            State = MessageState.Reply;
            if(ErrorCode == 0) ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class UpdateServerOutMessage : OutMessage
    {
        public UpdateServerOutMessage(ClientAgentBase sender, ulong __characterId__, int placeholder)
            : base(sender, ServiceType.Team, 7101, (int)MessageType.SAS)
        {
            Request = new __RPC_Team_UpdateServer_ARG_int32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Team_UpdateServer_ARG_int32_placeholder__ Request { get; private set; }


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

    public class NotifyAllianceWarResultOutMessage : OutMessage
    {
        public NotifyAllianceWarResultOutMessage(ClientAgentBase sender, ulong __characterId__, AllianceWarResult result)
            : base(sender, ServiceType.Team, 7111, (int)MessageType.SS)
        {
            Request = new __RPC_Team_NotifyAllianceWarResult_ARG_AllianceWarResult_result__();
            mMessage.CharacterId = __characterId__;
            Request.Result=result;

        }

        public __RPC_Team_NotifyAllianceWarResult_ARG_AllianceWarResult_result__ Request { get; private set; }

            private __RPC_Team_NotifyAllianceWarResult_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_NotifyAllianceWarResult_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class QueryAllianceWarInfoOutMessage : OutMessage
    {
        public QueryAllianceWarInfoOutMessage(ClientAgentBase sender, ulong __characterId__, int placeholder)
            : base(sender, ServiceType.Team, 7112, (int)MessageType.SS)
        {
            Request = new __RPC_Team_QueryAllianceWarInfo_ARG_int32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Team_QueryAllianceWarInfo_ARG_int32_placeholder__ Request { get; private set; }

            private __RPC_Team_QueryAllianceWarInfo_RET_AllianceWarInfos__ mResponse;
            public AllianceWarInfos Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Team_QueryAllianceWarInfo_RET_AllianceWarInfos__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSOnItemAuctionOutMessage : OutMessage
    {
        public SSOnItemAuctionOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, ulong characterId, string characterName, ItemBaseData item, int needType, int needCount, long itemGuid)
            : base(sender, ServiceType.Team, 7114, (int)MessageType.SS)
        {
            Request = new __RPC_Team_SSOnItemAuction_ARG_int32_serverId_uint64_characterId_string_characterName_ItemBaseData_item_int32_needType_int32_needCount_int64_itemGuid__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.CharacterId=characterId;
            Request.CharacterName=characterName;
            Request.Item=item;
            Request.NeedType=needType;
            Request.NeedCount=needCount;
            Request.ItemGuid=itemGuid;

        }

        public __RPC_Team_SSOnItemAuction_ARG_int32_serverId_uint64_characterId_string_characterName_ItemBaseData_item_int32_needType_int32_needCount_int64_itemGuid__ Request { get; private set; }

            private __RPC_Team_SSOnItemAuction_RET_int64__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_SSOnItemAuction_RET_int64__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSDownItemAuctionOutMessage : OutMessage
    {
        public SSDownItemAuctionOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, ulong characterId, long itemGuid)
            : base(sender, ServiceType.Team, 7115, (int)MessageType.SS)
        {
            Request = new __RPC_Team_SSDownItemAuction_ARG_int32_serverId_uint64_characterId_int64_itemGuid__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.CharacterId=characterId;
            Request.ItemGuid=itemGuid;

        }

        public __RPC_Team_SSDownItemAuction_ARG_int32_serverId_uint64_characterId_int64_itemGuid__ Request { get; private set; }


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

    public class SSSelectItemAuctionOutMessage : OutMessage
    {
        public SSSelectItemAuctionOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, ulong characterId, long itemManagerId)
            : base(sender, ServiceType.Team, 7117, (int)MessageType.SS)
        {
            Request = new __RPC_Team_SSSelectItemAuction_ARG_int32_serverId_uint64_characterId_int64_itemManagerId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.CharacterId=characterId;
            Request.ItemManagerId=itemManagerId;

        }

        public __RPC_Team_SSSelectItemAuction_ARG_int32_serverId_uint64_characterId_int64_itemManagerId__ Request { get; private set; }

            private __RPC_Team_SSSelectItemAuction_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_SSSelectItemAuction_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SBCleanClientCharacterDataOutMessage : OutMessage
    {
        public SBCleanClientCharacterDataOutMessage(ClientAgentBase sender, ulong __characterId__, ulong clientId, ulong characterId)
            : base(sender, ServiceType.Team, 7500, (int)MessageType.SB)
        {
            Request = new __RPC_Team_SBCleanClientCharacterData_ARG_uint64_clientId_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.ClientId=clientId;
            Request.CharacterId=characterId;

        }

        public __RPC_Team_SBCleanClientCharacterData_ARG_uint64_clientId_uint64_characterId__ Request { get; private set; }


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
            : base(sender, ServiceType.Team, 7501, (int)MessageType.SS)
        {
            Request = new __RPC_Team_SSNotifyCharacterOnConnet_ARG_uint64_clientId_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.ClientId=clientId;
            Request.CharacterId=characterId;

        }

        public __RPC_Team_SSNotifyCharacterOnConnet_ARG_uint64_clientId_uint64_characterId__ Request { get; private set; }

            private __RPC_Team_SSNotifyCharacterOnConnet_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_SSNotifyCharacterOnConnet_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSGetCharacterTeamFlagOutMessage : OutMessage
    {
        public SSGetCharacterTeamFlagOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId, bool flag)
            : base(sender, ServiceType.Team, 7508, (int)MessageType.SS)
        {
            Request = new __RPC_Team_SSGetCharacterTeamFlag_ARG_uint64_characterId_bool_flag__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;
            Request.Flag=flag;

        }

        public __RPC_Team_SSGetCharacterTeamFlag_ARG_uint64_characterId_bool_flag__ Request { get; private set; }


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

    public class SSAllianceDepotDonateOutMessage : OutMessage
    {
        public SSAllianceDepotDonateOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, ulong characterId, string characterName, ItemBaseData item)
            : base(sender, ServiceType.Team, 7520, (int)MessageType.SS)
        {
            Request = new __RPC_Team_SSAllianceDepotDonate_ARG_int32_serverId_uint64_characterId_string_characterName_ItemBaseData_item__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.CharacterId=characterId;
            Request.CharacterName=characterName;
            Request.Item=item;

        }

        public __RPC_Team_SSAllianceDepotDonate_ARG_int32_serverId_uint64_characterId_string_characterName_ItemBaseData_item__ Request { get; private set; }

            private __RPC_Team_SSAllianceDepotDonate_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_SSAllianceDepotDonate_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSAllianceDepotTakeOutOutMessage : OutMessage
    {
        public SSAllianceDepotTakeOutOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, ulong characterId, string characterName, int bagIndex, int itemId)
            : base(sender, ServiceType.Team, 7523, (int)MessageType.SS)
        {
            Request = new __RPC_Team_SSAllianceDepotTakeOut_ARG_int32_serverId_uint64_characterId_string_characterName_int32_bagIndex_int32_itemId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.CharacterId=characterId;
            Request.CharacterName=characterName;
            Request.BagIndex=bagIndex;
            Request.ItemId=itemId;

        }

        public __RPC_Team_SSAllianceDepotTakeOut_ARG_int32_serverId_uint64_characterId_string_characterName_int32_bagIndex_int32_itemId__ Request { get; private set; }

            private __RPC_Team_SSAllianceDepotTakeOut_RET_ItemBaseData__ mResponse;
            public ItemBaseData Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Team_SSAllianceDepotTakeOut_RET_ItemBaseData__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GMCommandOutMessage : OutMessage
    {
        public GMCommandOutMessage(ClientAgentBase sender, ulong __characterId__, StringArray commonds)
            : base(sender, ServiceType.Team, 7900, (int)MessageType.SS)
        {
            Request = new __RPC_Team_GMCommand_ARG_StringArray_commonds__();
            mMessage.CharacterId = __characterId__;
            Request.Commonds=commonds;

        }

        public __RPC_Team_GMCommand_ARG_StringArray_commonds__ Request { get; private set; }

            private __RPC_Team_GMCommand_RET_Int32Array__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_GMCommand_RET_Int32Array__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class PlayerHoldLodeOutMessage : OutMessage
    {
        public PlayerHoldLodeOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int allianceId, int sceneId)
            : base(sender, ServiceType.Team, 7901, (int)MessageType.SS)
        {
            Request = new __RPC_Team_PlayerHoldLode_ARG_int32_serverId_int32_allianceId_int32_sceneId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.AllianceId=allianceId;
            Request.SceneId=sceneId;

        }

        public __RPC_Team_PlayerHoldLode_ARG_int32_serverId_int32_allianceId_int32_sceneId__ Request { get; private set; }

            private __RPC_Team_PlayerHoldLode_RET_MsgSceneLode__ mResponse;
            public MsgSceneLode Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Team_PlayerHoldLode_RET_MsgSceneLode__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class PlayerCollectLodeOutMessage : OutMessage
    {
        public PlayerCollectLodeOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, ulong characterId, int allianceId, int sceneId, int LodeId, int addScore, FieldRankBaseData baseData, int meritPoint)
            : base(sender, ServiceType.Team, 7902, (int)MessageType.SS)
        {
            Request = new __RPC_Team_PlayerCollectLode_ARG_int32_serverId_uint64_characterId_int32_allianceId_int32_sceneId_int32_LodeId_int32_addScore_FieldRankBaseData_baseData_int32_meritPoint__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.CharacterId=characterId;
            Request.AllianceId=allianceId;
            Request.SceneId=sceneId;
            Request.LodeId=LodeId;
            Request.AddScore=addScore;
            Request.BaseData=baseData;
            Request.MeritPoint=meritPoint;

        }

        public __RPC_Team_PlayerCollectLode_ARG_int32_serverId_uint64_characterId_int32_allianceId_int32_sceneId_int32_LodeId_int32_addScore_FieldRankBaseData_baseData_int32_meritPoint__ Request { get; private set; }

            private __RPC_Team_PlayerCollectLode_RET_MsgSceneLode__ mResponse;
            public MsgSceneLode Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Team_PlayerCollectLode_RET_MsgSceneLode__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class ApplyHoldLodeOutMessage : OutMessage
    {
        public ApplyHoldLodeOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int sceneId)
            : base(sender, ServiceType.Team, 7903, (int)MessageType.SS)
        {
            Request = new __RPC_Team_ApplyHoldLode_ARG_int32_serverId_int32_sceneId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.SceneId=sceneId;

        }

        public __RPC_Team_ApplyHoldLode_ARG_int32_serverId_int32_sceneId__ Request { get; private set; }

            private __RPC_Team_ApplyHoldLode_RET_MsgSceneLode__ mResponse;
            public MsgSceneLode Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Team_ApplyHoldLode_RET_MsgSceneLode__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSApplyFieldActivityRewardOutMessage : OutMessage
    {
        public SSApplyFieldActivityRewardOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int allianceId, ulong characterId, int missionId, int score, int addScore, FieldRankBaseData data)
            : base(sender, ServiceType.Team, 7906, (int)MessageType.SS)
        {
            Request = new __RPC_Team_SSApplyFieldActivityReward_ARG_int32_serverId_int32_allianceId_uint64_characterId_int32_missionId_int32_score_int32_addScore_FieldRankBaseData_data__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.AllianceId=allianceId;
            Request.CharacterId=characterId;
            Request.MissionId=missionId;
            Request.Score=score;
            Request.AddScore=addScore;
            Request.Data=data;

        }

        public __RPC_Team_SSApplyFieldActivityReward_ARG_int32_serverId_int32_allianceId_uint64_characterId_int32_missionId_int32_score_int32_addScore_FieldRankBaseData_data__ Request { get; private set; }

            private __RPC_Team_SSApplyFieldActivityReward_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Team_SSApplyFieldActivityReward_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSSyncTeamMemberLevelChangeOutMessage : OutMessage
    {
        public SSSyncTeamMemberLevelChangeOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, ulong characterId, ulong teamId, int reborn, int level)
            : base(sender, ServiceType.Team, 7907, (int)MessageType.SS)
        {
            Request = new __RPC_Team_SSSyncTeamMemberLevelChange_ARG_int32_serverId_uint64_characterId_uint64_teamId_int32_reborn_int32_level__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.CharacterId=characterId;
            Request.TeamId=teamId;
            Request.Reborn=reborn;
            Request.Level=level;

        }

        public __RPC_Team_SSSyncTeamMemberLevelChange_ARG_int32_serverId_uint64_characterId_uint64_teamId_int32_reborn_int32_level__ Request { get; private set; }


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

    public class SSAddAllianceContributionOutMessage : OutMessage
    {
        public SSAddAllianceContributionOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, ulong characterId, int allianceId, int Contribution)
            : base(sender, ServiceType.Team, 7909, (int)MessageType.SS)
        {
            Request = new __RPC_Team_SSAddAllianceContribution_ARG_int32_serverId_uint64_characterId_int32_allianceId_int32_Contribution__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.CharacterId=characterId;
            Request.AllianceId=allianceId;
            Request.Contribution=Contribution;

        }

        public __RPC_Team_SSAddAllianceContribution_ARG_int32_serverId_uint64_characterId_int32_allianceId_int32_Contribution__ Request { get; private set; }


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

    public class NodifyModifyPlayerNameOutMessage : OutMessage
    {
        public NodifyModifyPlayerNameOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, ulong characterId, ulong teamId, string modifyName)
            : base(sender, ServiceType.Team, 7915, (int)MessageType.SS)
        {
            Request = new __RPC_Team_NodifyModifyPlayerName_ARG_int32_serverId_uint64_characterId_uint64_teamId_string_modifyName__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.CharacterId=characterId;
            Request.TeamId=teamId;
            Request.ModifyName=modifyName;

        }

        public __RPC_Team_NodifyModifyPlayerName_ARG_int32_serverId_uint64_characterId_uint64_teamId_string_modifyName__ Request { get; private set; }


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

    public class NodifyModifyAllianceMemberNameOutMessage : OutMessage
    {
        public NodifyModifyAllianceMemberNameOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, ulong characterId, int allianceId, string modifyName)
            : base(sender, ServiceType.Team, 7917, (int)MessageType.SS)
        {
            Request = new __RPC_Team_NodifyModifyAllianceMemberName_ARG_int32_serverId_uint64_characterId_int32_allianceId_string_modifyName__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.CharacterId=characterId;
            Request.AllianceId=allianceId;
            Request.ModifyName=modifyName;

        }

        public __RPC_Team_NodifyModifyAllianceMemberName_ARG_int32_serverId_uint64_characterId_int32_allianceId_string_modifyName__ Request { get; private set; }


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

    public class SSLeaveTeamOutMessage : OutMessage
    {
        public SSLeaveTeamOutMessage(ClientAgentBase sender, ulong __characterId__, int placeholder)
            : base(sender, ServiceType.Team, 7918, (int)MessageType.SS)
        {
            Request = new __RPC_Team_SSLeaveTeam_ARG_int32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Team_SSLeaveTeam_ARG_int32_placeholder__ Request { get; private set; }


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

    public class AddFunctionNameTeam
    {
        public static void AddFunctionName(IDictionary<int, string> dict)
        {
            dict[7000] = "PrepareDataForEnterGame";
            dict[7001] = "PrepareDataForCreateCharacter";
            dict[7002] = "PrepareDataForCommonUse";
            dict[7003] = "PrepareDataForLogout";
            dict[7030] = "SBGetAllOnlineCharacterInServer";
            dict[7031] = "CheckConnected";
            dict[7032] = "CheckLost";
            dict[7033] = "QueryStatus";
            dict[7040] = "QueryBrokerStatus";
            dict[7041] = "TeamMessage";
            dict[7042] = "NotifyTeamMessage";
            dict[7043] = "ApplyTeam";
            dict[7044] = "TeamChatMessage";
            dict[7045] = "TeamDungeonLineUp";
            dict[7046] = "TeamDungeonLeaderChangeScene";
            dict[7047] = "MatchingStart";
            dict[7048] = "MatchingCancel";
            dict[7049] = "MatchingSuccess";
            dict[7050] = "NotifyMatchingData";
            dict[7051] = "MatchingBack";
            dict[7052] = "TeamServerMessage";
            dict[7053] = "LeaveDungeon";
            dict[7054] = "ApplyQueueData";
            dict[7055] = "TeamEnterFuben";
            dict[7056] = "SyncTeamEnterFuben";
            dict[7057] = "ResultTeamEnterFuben";
            dict[7058] = "ServerGMCommand";
            dict[7059] = "GMTeam";
            dict[7060] = "MemberWantInvite";
            dict[7061] = "Logic2TeamCreateAlliance";
            dict[7062] = "Logic2TeamAllianceOperation";
            dict[7063] = "Logic2TeamAllianceOperationCharacter";
            dict[7064] = "SyncAllianceMessage";
            dict[7065] = "ApplyAllianceData";
            dict[7066] = "ApplyAllianceDataByServerId";
            dict[7067] = "ChangeAllianceNotice";
            dict[7068] = "GetServerAlliance";
            dict[7069] = "ChangeJurisdiction";
            dict[7070] = "ChangeAllianceAutoJoin";
            dict[7071] = "AllianceAgreeApplyList";
            dict[7072] = "GetAllianceCharacterData";
            dict[7073] = "SSGetTeamData";
            dict[7074] = "SSGetAllianceData";
            dict[7075] = "SendMatchingMessage";
            dict[7076] = "Logic2TeamDonationAllianceItem";
            dict[7078] = "TeamSyncAllianceMessage";
            dict[7300] = "SSGetAllianceName";
            dict[7301] = "SSGetAlliance";
            dict[7302] = "GMChangeJurisdiction";
            dict[7303] = "GMChangeAllianceNotice";
            dict[7304] = "GMDelAllicance";
            dict[7080] = "ChangeAllianceData";
            dict[7081] = "ApplyAllianceMissionData";
            dict[7082] = "UpgradeAllianceLevel";
            dict[7083] = "BroadcastExchangeItem";
            dict[7084] = "GetExchangeItem";
            dict[7085] = "CancelExchangeItem";
            dict[7086] = "SSApplyGroupShopItems";
            dict[7087] = "SSBuyGroupShopItem";
            dict[7088] = "SSGetBuyedGroupShopItems";
            dict[7089] = "SSGetGroupShopHistory";
            dict[7090] = "ApplyAllianceEnjoyList";
            dict[7091] = "ApplyAllianceDonationList";
            dict[7092] = "SSCharacterEnterBattle";
            dict[7093] = "SSCharacterLeaveBattle";
            dict[7094] = "SSBattleEnd";
            dict[7095] = "SSGetTeamCount";
            dict[7096] = "SSGetTeamSceneData";
            dict[7097] = "SSGetTeamCharacters";
            dict[7098] = "SSNotifyPlayerChangeScene";
            dict[7099] = "ReadyToEnter";
            dict[7100] = "SendGroupMessage";
            dict[7101] = "UpdateServer";
            dict[7102] = "NotifyQueueMessage";
            dict[7103] = "NotifyQueueResult";
            dict[7104] = "ApplyAllianceWarOccupantData";
            dict[7105] = "NotifyAllianceWarOccupantData";
            dict[7106] = "ApplyAllianceWarChallengerData";
            dict[7107] = "NotifyAllianceWarChallengerData";
            dict[7108] = "ApplyAllianceWarData";
            dict[7109] = "BidAllianceWar";
            dict[7110] = "EnterAllianceWar";
            dict[7111] = "NotifyAllianceWarResult";
            dict[7112] = "QueryAllianceWarInfo";
            dict[7113] = "SyncAllianceChatMessage";
            dict[7114] = "SSOnItemAuction";
            dict[7115] = "SSDownItemAuction";
            dict[7116] = "CSSelectItemAuction";
            dict[7117] = "SSSelectItemAuction";
            dict[7130] = "CSEnrollUnionBattle";
            dict[7131] = "CSEnterUnionBattle";
            dict[7132] = "CSGetUnionBattleInfo";
            dict[7133] = "CSGetUnionBattleMathInfo";
            dict[7500] = "SBCleanClientCharacterData";
            dict[7501] = "SSNotifyCharacterOnConnet";
            dict[7503] = "BSNotifyCharacterOnLost";
            dict[7504] = "AutoMatchBegin";
            dict[7505] = "AutoMatchEnd";
            dict[7506] = "AutoMatchCancel";
            dict[7507] = "AutoMatchStateChange";
            dict[7508] = "SSGetCharacterTeamFlag";
            dict[7509] = "ChangetTeamTarget";
            dict[7510] = "NotifyChangetTeamTarget";
            dict[7512] = "SearchTeamList";
            dict[7513] = "TeamSearchApplyList";
            dict[7514] = "NotifyTeamScenGuid";
            dict[7515] = "TeamApplyListClear";
            dict[7516] = "TeamApplyListSync";
            dict[7520] = "SSAllianceDepotDonate";
            dict[7521] = "ApplyAllianceDepotLogList";
            dict[7522] = "BattleUnionDepotArrange";
            dict[7523] = "SSAllianceDepotTakeOut";
            dict[7524] = "ApplyAllianceDepotData";
            dict[7525] = "BattleUnionDepotClearUp";
            dict[7526] = "BattleUnionRemoveDepotItem";
            dict[7900] = "GMCommand";
            dict[7901] = "PlayerHoldLode";
            dict[7902] = "PlayerCollectLode";
            dict[7903] = "ApplyHoldLode";
            dict[7904] = "ClientApplyHoldLode";
            dict[7905] = "ClientApplyActiveInfo";
            dict[7906] = "SSApplyFieldActivityReward";
            dict[7907] = "SSSyncTeamMemberLevelChange";
            dict[7908] = "SCSyncTeamMemberLevelChange";
            dict[7909] = "SSAddAllianceContribution";
            dict[7910] = "NoticeTeamMemberError";
            dict[7911] = "NotifyFieldFinal";
            dict[7912] = "SCNotifyAllianceActiveTask";
            dict[7913] = "ClearAllianceApplyList";
            dict[7914] = "SCNotifyPlayerExitAlliance";
            dict[7915] = "NodifyModifyPlayerName";
            dict[7916] = "NodifyTeamMemberPlayerNameChange";
            dict[7917] = "NodifyModifyAllianceMemberName";
            dict[7918] = "SSLeaveTeam";
        }
        public static void AddCSFunctionName(IDictionary<int, string> dict)
        {
            dict[7041] = "TeamMessage";
            dict[7043] = "ApplyTeam";
            dict[7044] = "TeamChatMessage";
            dict[7045] = "TeamDungeonLineUp";
            dict[7047] = "MatchingStart";
            dict[7048] = "MatchingCancel";
            dict[7051] = "MatchingBack";
            dict[7054] = "ApplyQueueData";
            dict[7055] = "TeamEnterFuben";
            dict[7057] = "ResultTeamEnterFuben";
            dict[7059] = "GMTeam";
            dict[7065] = "ApplyAllianceData";
            dict[7066] = "ApplyAllianceDataByServerId";
            dict[7067] = "ChangeAllianceNotice";
            dict[7068] = "GetServerAlliance";
            dict[7069] = "ChangeJurisdiction";
            dict[7070] = "ChangeAllianceAutoJoin";
            dict[7071] = "AllianceAgreeApplyList";
            dict[7081] = "ApplyAllianceMissionData";
            dict[7082] = "UpgradeAllianceLevel";
            dict[7090] = "ApplyAllianceEnjoyList";
            dict[7091] = "ApplyAllianceDonationList";
            dict[7104] = "ApplyAllianceWarOccupantData";
            dict[7106] = "ApplyAllianceWarChallengerData";
            dict[7108] = "ApplyAllianceWarData";
            dict[7109] = "BidAllianceWar";
            dict[7110] = "EnterAllianceWar";
            dict[7116] = "CSSelectItemAuction";
            dict[7130] = "CSEnrollUnionBattle";
            dict[7131] = "CSEnterUnionBattle";
            dict[7132] = "CSGetUnionBattleInfo";
            dict[7133] = "CSGetUnionBattleMathInfo";
            dict[7504] = "AutoMatchBegin";
            dict[7506] = "AutoMatchCancel";
            dict[7509] = "ChangetTeamTarget";
            dict[7512] = "SearchTeamList";
            dict[7513] = "TeamSearchApplyList";
            dict[7515] = "TeamApplyListClear";
            dict[7521] = "ApplyAllianceDepotLogList";
            dict[7522] = "BattleUnionDepotArrange";
            dict[7524] = "ApplyAllianceDepotData";
            dict[7525] = "BattleUnionDepotClearUp";
            dict[7526] = "BattleUnionRemoveDepotItem";
            dict[7904] = "ClientApplyHoldLode";
            dict[7905] = "ClientApplyActiveInfo";
            dict[7913] = "ClearAllianceApplyList";
        }
    }
}
