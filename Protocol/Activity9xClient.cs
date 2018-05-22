using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Scorpion;
using DataContract;
using ProtoBuf;

#pragma warning disable 0162,0108
namespace ActivityClientService
{

    public abstract class ActivityAgent : ClientAgentBase
    {
        public ActivityAgent(string addr)
            : base(addr)
        {
        }

        public ActivityAgent(ServerInfo broker, ServerInfo[] directConnect, Func<ulong, int> characterId2ServerId)
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
        /// 广播表格重载
        /// </summary>
        public ServerGMCommandOutMessage ServerGMCommand(string cmd, string param)
        {
            return new ServerGMCommandOutMessage(this, 0, cmd, param);
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
        /// 检查相应客户端连接是否已经断开
        /// </summary>
        public QueryCreateMonsterDataOutMessage QueryCreateMonsterData(ulong __characterId__, int serverId, int sceneId)
        {
            return new QueryCreateMonsterDataOutMessage(this, __characterId__, serverId, sceneId);
        }

        /// <summary>
        /// 通知Activity，某只怪物的伤害列表
        /// </summary>
        public NotifyDamageListOutMessage NotifyDamageList(ulong __characterId__, int serverId, ulong sceneGuid, DamageList list)
        {
            return new NotifyDamageListOutMessage(this, __characterId__, serverId, sceneGuid, list);
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
        /// 活动是否可以进入
        /// </summary>
        public SSApplyActivityStateOutMessage SSApplyActivityState(ulong __characterId__, int activityId, int serverId)
        {
            return new SSApplyActivityStateOutMessage(this, __characterId__, activityId, serverId);
        }

        /// <summary>
        /// 提升炮台血量
        /// </summary>
        public SSApplyPromoteHPOutMessage SSApplyPromoteHP(ulong __characterId__, int serverId, int activityId, int batteryId, int promoteType, ulong characterId, string name)
        {
            return new SSApplyPromoteHPOutMessage(this, __characterId__, serverId, activityId, batteryId, promoteType, characterId, name);
        }

        /// <summary>
        /// 提升炮台技能等级
        /// </summary>
        public SSApplyPromoteSkillOutMessage SSApplyPromoteSkill(ulong __characterId__, int serverId, int activityId, int batteryId, int promoteType, ulong characterId, string name)
        {
            return new SSApplyPromoteSkillOutMessage(this, __characterId__, serverId, activityId, batteryId, promoteType, characterId, name);
        }

        /// <summary>
        /// 请求报名活动
        /// </summary>
        public SSApplyJoinActivityOutMessage SSApplyJoinActivity(ulong __characterId__, int serverId, int activityId)
        {
            return new SSApplyJoinActivityOutMessage(this, __characterId__, serverId, activityId);
        }

        /// <summary>
        /// 同步灭世活动的数据
        /// </summary>
        public SSSyncMieShiDataOutMessage SSSyncMieShiData(ulong __characterId__, int serverId, int activityId, MieShiSceneData list)
        {
            return new SSSyncMieShiDataOutMessage(this, __characterId__, serverId, activityId, list);
        }

        /// <summary>
        /// 获取炮台贡献百分比列表
        /// </summary>
        public SSApplyContributeRateOutMessage SSApplyContributeRate(ulong __characterId__, int serverId, int activityId)
        {
            return new SSApplyContributeRateOutMessage(this, __characterId__, serverId, activityId);
        }

        /// <summary>
        /// 保存活动的结果
        /// </summary>
        public SSSaveActivityResultOutMessage SSSaveActivityResult(ulong __characterId__, int serverId, int activityId, int result)
        {
            return new SSSaveActivityResultOutMessage(this, __characterId__, serverId, activityId, result);
        }

        /// <summary>
        /// 获取活动状态列表
        /// </summary>
        public SSApplyActiResultListOutMessage SSApplyActiResultList(ulong __characterId__, int serverId)
        {
            return new SSApplyActiResultListOutMessage(this, __characterId__, serverId);
        }

        /// <summary>
        /// 同步炮台的guid
        /// 返回值活动数据
        /// </summary>
        public SSSetAndGetActivityDataOutMessage SSSetAndGetActivityData(ulong __characterId__, ulong sceneGuid, int serverId, int activityId, MieShiBatteryGuid guidList)
        {
            return new SSSetAndGetActivityDataOutMessage(this, __characterId__, sceneGuid, serverId, activityId, guidList);
        }

        /// <summary>
        /// 灭世活动全部玩家退出
        /// </summary>
        public SyncActivityAllPlayerExitOutMessage SyncActivityAllPlayerExit(ulong __characterId__, int serverId, int activityId)
        {
            return new SyncActivityAllPlayerExitOutMessage(this, __characterId__, serverId, activityId);
        }

        /// <summary>
        /// 古域战场同步死亡boss
        /// </summary>
        public SSAcientBattleSceneRequestOutMessage SSAcientBattleSceneRequest(ulong __characterId__, int serverId, int sceneid, int npcId, int isDie)
        {
            return new SSAcientBattleSceneRequestOutMessage(this, __characterId__, serverId, sceneid, npcId, isDie);
        }

        /// <summary>
        /// 请求灭世活动是否可进入
        /// </summary>
        public SSApplyMieShiCanInOutMessage SSApplyMieShiCanIn(ulong __characterId__, int serverId, int activityId)
        {
            return new SSApplyMieShiCanInOutMessage(this, __characterId__, serverId, activityId);
        }

        /// <summary>
        /// 同步灭世Boss宝箱可以领取
        /// </summary>
        public SSSyncMieShiBoxCanPickUpOutMessage SSSyncMieShiBoxCanPickUp(ulong __characterId__, int serverId, int activityId, int npcId)
        {
            return new SSSyncMieShiBoxCanPickUpOutMessage(this, __characterId__, serverId, activityId, npcId);
        }

        /// <summary>
        /// 请求领取Boss宝箱
        /// </summary>
        public SSApplyPickUpBoxOutMessage SSApplyPickUpBox(ulong __characterId__, int serverId, int activityId, int npcId)
        {
            return new SSApplyPickUpBoxOutMessage(this, __characterId__, serverId, activityId, npcId);
        }

        /// <summary>
        /// 请求领取奖励
        /// </summary>
        public SSApplyPortraitAwardOutMessage SSApplyPortraitAward(ulong __characterId__, int serverId)
        {
            return new SSApplyPortraitAwardOutMessage(this, __characterId__, serverId);
        }

        /// <summary>
        /// 保存炮台被摧毁的数据
        /// </summary>
        public SSSaveBatteryDestroyOutMessage SSSaveBatteryDestroy(ulong __characterId__, int serverId, int activityId, ulong batteryGuid)
        {
            return new SSSaveBatteryDestroyOutMessage(this, __characterId__, serverId, activityId, batteryGuid);
        }

        /// <summary>
        ///  请求上次灭世结果
        /// </summary>
        public SSApplyLastResultOutMessage SSApplyLastResult(ulong __characterId__, int serverId)
        {
            return new SSApplyLastResultOutMessage(this, __characterId__, serverId);
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
        /// 请求灭世炮台提升数量
        /// </summary>
        public SSAskMieshiTowerUpTimesOutMessage SSAskMieshiTowerUpTimes(ulong __characterId__, int serverId, int activityId, ulong characterId)
        {
            return new SSAskMieshiTowerUpTimesOutMessage(this, __characterId__, serverId, activityId, characterId);
        }

        /// <summary>
        /// 请求领取炮台升级奖励
        /// </summary>
        public SSAskMieshiTowerRewardOutMessage SSAskMieshiTowerReward(ulong __characterId__, int serverId, int activityId, ulong characterId, int idx)
        {
            return new SSAskMieshiTowerRewardOutMessage(this, __characterId__, serverId, activityId, characterId, idx);
        }

        /// <summary>
        /// 请求Boss之家Boss状态数据
        /// </summary>
        public BossHomeSceneRequestOutMessage BossHomeSceneRequest(ulong __characterId__, int serverId, int sceneid, int npcId, int isDie)
        {
            return new BossHomeSceneRequestOutMessage(this, __characterId__, serverId, sceneid, npcId, isDie);
        }

        /// <summary>
        /// 请求珍宝商店数据
        /// </summary>
        public SSGetTreasureShopItemsOutMessage SSGetTreasureShopItems(ulong __characterId__, int serverId)
        {
            return new SSGetTreasureShopItemsOutMessage(this, __characterId__, serverId);
        }

        /// <summary>
        /// 请求珍宝商店物品数量
        /// </summary>
        public SSGetTreasureShopItemCountOutMessage SSGetTreasureShopItemCount(ulong __characterId__, int serverId, int storeId)
        {
            return new SSGetTreasureShopItemCountOutMessage(this, __characterId__, serverId, storeId);
        }

        /// <summary>
        /// 消耗珍宝商店物品
        /// </summary>
        public SSConsumeTreasureShopItemOutMessage SSConsumeTreasureShopItem(ulong __characterId__, int serverId, int storeId, int consumeCount)
        {
            return new SSConsumeTreasureShopItemOutMessage(this, __characterId__, serverId, storeId, consumeCount);
        }

        /// <summary>
        /// 请求黑市商店数据
        /// </summary>
        public SSGetBlackStoreItemsOutMessage SSGetBlackStoreItems(ulong __characterId__, int serverId)
        {
            return new SSGetBlackStoreItemsOutMessage(this, __characterId__, serverId);
        }

        /// <summary>
        /// </summary>
        public SSSyncChickenScoreOutMessage SSSyncChickenScore(ulong __characterId__, ChickenRankData rankData)
        {
            return new SSSyncChickenScoreOutMessage(this, __characterId__, rankData);
        }

        protected override object GetPublishData(uint p, byte[] list)
        {
            switch (p)
            {
                case 4000:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_PrepareDataForEnterGame_ARG_int32_serverId__>(ms);
                    }
                    break;
                case 4001:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_PrepareDataForCreateCharacter_ARG_int32_type__>(ms);
                    }
                    break;
                case 4002:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_PrepareDataForCommonUse_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 4003:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_PrepareDataForLogout_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 4004:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_ServerGMCommand_ARG_string_cmd_string_param__>(ms);
                    }
                    break;
                case 4030:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_SBGetAllOnlineCharacterInServer_ARG_int32_serverId__>(ms);
                    }
                    break;
                case 4031:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_CheckConnected_ARG_uint64_characterId__>(ms);
                    }
                    break;
                case 4032:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_CheckLost_ARG_uint64_characterId__>(ms);
                    }
                    break;
                case 4033:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_QueryCreateMonsterData_ARG_int32_serverId_int32_sceneId__>(ms);
                    }
                    break;
                case 4034:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_NotifyDamageList_ARG_int32_serverId_uint64_sceneGuid_DamageList_list__>(ms);
                    }
                    break;
                case 4035:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_QueryStatus_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 4040:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_QueryBrokerStatus_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 4041:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_ReadyToEnter_ARG_int32_placeholder__>(ms);
                    }
                    break;
                case 4042:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_UpdateServer_ARG_int32_placeholder__>(ms);
                    }
                    break;
                case 4043:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_SSApplyActivityState_ARG_int32_activityId_int32_serverId__>(ms);
                    }
                    break;
                case 4102:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_SSApplyPromoteHP_ARG_int32_serverId_int32_activityId_int32_batteryId_int32_promoteType_uint64_characterId_string_name__>(ms);
                    }
                    break;
                case 4103:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_SSApplyPromoteSkill_ARG_int32_serverId_int32_activityId_int32_batteryId_int32_promoteType_uint64_characterId_string_name__>(ms);
                    }
                    break;
                case 4106:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_SSApplyJoinActivity_ARG_int32_serverId_int32_activityId__>(ms);
                    }
                    break;
                case 4110:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_SSSyncMieShiData_ARG_int32_serverId_int32_activityId_MieShiSceneData_list__>(ms);
                    }
                    break;
                case 4111:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_SSApplyContributeRate_ARG_int32_serverId_int32_activityId__>(ms);
                    }
                    break;
                case 4112:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_SSSaveActivityResult_ARG_int32_serverId_int32_activityId_int32_result__>(ms);
                    }
                    break;
                case 4113:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_SSApplyActiResultList_ARG_int32_serverId__>(ms);
                    }
                    break;
                case 4114:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_SSSetAndGetActivityData_ARG_uint64_sceneGuid_int32_serverId_int32_activityId_MieShiBatteryGuid_guidList__>(ms);
                    }
                    break;
                case 4116:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_SyncActivityAllPlayerExit_ARG_int32_serverId_int32_activityId__>(ms);
                    }
                    break;
                case 4117:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_SSAcientBattleSceneRequest_ARG_int32_serverId_int32_sceneid_int32_npcId_int32_isDie__>(ms);
                    }
                    break;
                case 4015:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_SSApplyMieShiCanIn_ARG_int32_serverId_int32_activityId__>(ms);
                    }
                    break;
                case 4016:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_SSSyncMieShiBoxCanPickUp_ARG_int32_serverId_int32_activityId_int32_npcId__>(ms);
                    }
                    break;
                case 4017:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_SSApplyPickUpBox_ARG_int32_serverId_int32_activityId_int32_npcId__>(ms);
                    }
                    break;
                case 4019:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_SSApplyPortraitAward_ARG_int32_serverId__>(ms);
                    }
                    break;
                case 4020:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_SSSaveBatteryDestroy_ARG_int32_serverId_int32_activityId_uint64_batteryGuid__>(ms);
                    }
                    break;
                case 4021:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_SSApplyLastResult_ARG_int32_serverId__>(ms);
                    }
                    break;
                case 4500:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_SBCleanClientCharacterData_ARG_uint64_clientId_uint64_characterId__>(ms);
                    }
                    break;
                case 4501:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_SSNotifyCharacterOnConnet_ARG_uint64_clientId_uint64_characterId__>(ms);
                    }
                    break;
                case 4022:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_SSAskMieshiTowerUpTimes_ARG_int32_serverId_int32_activityId_uint64_characterId__>(ms);
                    }
                    break;
                case 4023:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_SSAskMieshiTowerReward_ARG_int32_serverId_int32_activityId_uint64_characterId_int32_idx__>(ms);
                    }
                    break;
                case 4024:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_BossHomeSceneRequest_ARG_int32_serverId_int32_sceneid_int32_npcId_int32_isDie__>(ms);
                    }
                    break;
                case 4026:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_SSGetTreasureShopItems_ARG_int32_serverId__>(ms);
                    }
                    break;
                case 4027:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_SSGetTreasureShopItemCount_ARG_int32_serverId_int32_storeId__>(ms);
                    }
                    break;
                case 4028:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_SSConsumeTreasureShopItem_ARG_int32_serverId_int32_storeId_int32_consumeCount__>(ms);
                    }
                    break;
                case 4029:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_SSGetBlackStoreItems_ARG_int32_serverId__>(ms);
                    }
                    break;
                case 4505:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Activity_SSSyncChickenScore_ARG_ChickenRankData_rankData__>(ms);
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
        /// 通知全服，某个活动的状态
        /// </summary>
        public object NotifyActivityState(uint __serverId__, int activityId, int state)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 4045;
            desc.PacketId = __serverId__;
            desc.Type = (int)MessageType.SCServer;
            desc.ServiceType = (int) ServiceType.Activity;

            var __data__ = new __RPC_Activity_NotifyActivityState_ARG_int32_activityId_int32_state__();
            __data__.ActivityId=activityId;
            __data__.State=state;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知所有在线客户端某些表格刷新了
        /// </summary>
        public object NotifyTableChange(int flag)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 4047;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SCAll;
            desc.ServiceType = (int) ServiceType.Activity;

            var __data__ = new __RPC_Activity_NotifyTableChange_ARG_int32_flag__();
            __data__.Flag=flag;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知全服，某个活动某个炮台的数据
        /// </summary>
        public object NotifyBatteryData(uint __serverId__, int activityId, ActivityBatteryOne battery)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 4107;
            desc.PacketId = __serverId__;
            desc.Type = (int)MessageType.SCServer;
            desc.ServiceType = (int) ServiceType.Activity;

            var __data__ = new __RPC_Activity_NotifyBatteryData_ARG_int32_activityId_ActivityBatteryOne_battery__();
            __data__.ActivityId=activityId;
            __data__.Battery=battery;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知全服灭世活动的状态
        /// </summary>
        public object NotifyMieShiActivityState(uint __serverId__, int activityId, int state)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 4108;
            desc.PacketId = __serverId__;
            desc.Type = (int)MessageType.SCServer;
            desc.ServiceType = (int) ServiceType.Activity;

            var __data__ = new __RPC_Activity_NotifyMieShiActivityState_ARG_int32_activityId_int32_state__();
            __data__.ActivityId=activityId;
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
        public object NotifyMieShiActivityInfo(uint __serverId__, CommonActivityData msg)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 4115;
            desc.PacketId = __serverId__;
            desc.Type = (int)MessageType.SCServer;
            desc.ServiceType = (int) ServiceType.Activity;

            var __data__ = new __RPC_Activity_NotifyMieShiActivityInfo_ARG_CommonActivityData_msg__();
            __data__.Msg=msg;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知客户端某个炮台数据更新
        /// SC ActivityBatteryOne         NotifyBatteryDataOne(int32 activityId, ActivityBatteryOne battery) = 4108;
        /// 通知报名的玩家可以进入
        /// </summary>
        public object NotifyPlayerCanIn(IEnumerable<ulong> __characterIds__, int fubenId, long canInEndTime)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 4109;
            desc.PacketId = 0;
            desc.Routing.AddRange(__characterIds__);
            if(desc.Routing.Count == 0) return null;
            desc.Type = (int)MessageType.SCList;
            desc.ServiceType = (int) ServiceType.Activity;

            var __data__ = new __RPC_Activity_NotifyPlayerCanIn_ARG_int32_fubenId_int64_canInEndTime__();
            __data__.FubenId=fubenId;
            __data__.CanInEndTime=canInEndTime;


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
            : base(sender, ServiceType.Activity, 4000, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_PrepareDataForEnterGame_ARG_int32_serverId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;

        }

        public __RPC_Activity_PrepareDataForEnterGame_ARG_int32_serverId__ Request { get; private set; }

            private __RPC_Activity_PrepareDataForEnterGame_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Activity_PrepareDataForEnterGame_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class PrepareDataForCreateCharacterOutMessage : OutMessage
    {
        public PrepareDataForCreateCharacterOutMessage(ClientAgentBase sender, ulong __characterId__, int type)
            : base(sender, ServiceType.Activity, 4001, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_PrepareDataForCreateCharacter_ARG_int32_type__();
            mMessage.CharacterId = __characterId__;
            Request.Type=type;

        }

        public __RPC_Activity_PrepareDataForCreateCharacter_ARG_int32_type__ Request { get; private set; }

            private __RPC_Activity_PrepareDataForCreateCharacter_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Activity_PrepareDataForCreateCharacter_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class PrepareDataForCommonUseOutMessage : OutMessage
    {
        public PrepareDataForCommonUseOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Activity, 4002, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_PrepareDataForCommonUse_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Activity_PrepareDataForCommonUse_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Activity_PrepareDataForCommonUse_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Activity_PrepareDataForCommonUse_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class PrepareDataForLogoutOutMessage : OutMessage
    {
        public PrepareDataForLogoutOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Activity, 4003, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_PrepareDataForLogout_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Activity_PrepareDataForLogout_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Activity_PrepareDataForLogout_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Activity_PrepareDataForLogout_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class ServerGMCommandOutMessage : OutMessage
    {
        public ServerGMCommandOutMessage(ClientAgentBase sender, ulong __characterId__, string cmd, string param)
            : base(sender, ServiceType.Activity, 4004, (int)MessageType.SAS)
        {
            Request = new __RPC_Activity_ServerGMCommand_ARG_string_cmd_string_param__();
            mMessage.CharacterId = __characterId__;
            Request.Cmd=cmd;
            Request.Param=param;

        }

        public __RPC_Activity_ServerGMCommand_ARG_string_cmd_string_param__ Request { get; private set; }


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

    public class SBGetAllOnlineCharacterInServerOutMessage : OutMessage
    {
        public SBGetAllOnlineCharacterInServerOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId)
            : base(sender, ServiceType.Activity, 4030, (int)MessageType.SB)
        {
            Request = new __RPC_Activity_SBGetAllOnlineCharacterInServer_ARG_int32_serverId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;

        }

        public __RPC_Activity_SBGetAllOnlineCharacterInServer_ARG_int32_serverId__ Request { get; private set; }

            private __RPC_Activity_SBGetAllOnlineCharacterInServer_RET_Uint64Array__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Activity_SBGetAllOnlineCharacterInServer_RET_Uint64Array__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class CheckConnectedOutMessage : OutMessage
    {
        public CheckConnectedOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId)
            : base(sender, ServiceType.Activity, 4031, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_CheckConnected_ARG_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;

        }

        public __RPC_Activity_CheckConnected_ARG_uint64_characterId__ Request { get; private set; }

            private __RPC_Activity_CheckConnected_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Activity_CheckConnected_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class CheckLostOutMessage : OutMessage
    {
        public CheckLostOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId)
            : base(sender, ServiceType.Activity, 4032, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_CheckLost_ARG_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;

        }

        public __RPC_Activity_CheckLost_ARG_uint64_characterId__ Request { get; private set; }

            private __RPC_Activity_CheckLost_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Activity_CheckLost_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class QueryCreateMonsterDataOutMessage : OutMessage
    {
        public QueryCreateMonsterDataOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int sceneId)
            : base(sender, ServiceType.Activity, 4033, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_QueryCreateMonsterData_ARG_int32_serverId_int32_sceneId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.SceneId=sceneId;

        }

        public __RPC_Activity_QueryCreateMonsterData_ARG_int32_serverId_int32_sceneId__ Request { get; private set; }

            private __RPC_Activity_QueryCreateMonsterData_RET_Int32Array__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Activity_QueryCreateMonsterData_RET_Int32Array__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class NotifyDamageListOutMessage : OutMessage
    {
        public NotifyDamageListOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, ulong sceneGuid, DamageList list)
            : base(sender, ServiceType.Activity, 4034, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_NotifyDamageList_ARG_int32_serverId_uint64_sceneGuid_DamageList_list__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.SceneGuid=sceneGuid;
            Request.List=list;

        }

        public __RPC_Activity_NotifyDamageList_ARG_int32_serverId_uint64_sceneGuid_DamageList_list__ Request { get; private set; }

            private __RPC_Activity_NotifyDamageList_RET_DamageListForServer__ mResponse;
            public DamageListForServer Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Activity_NotifyDamageList_RET_DamageListForServer__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class QueryStatusOutMessage : OutMessage
    {
        public QueryStatusOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Activity, 4035, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_QueryStatus_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Activity_QueryStatus_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Activity_QueryStatus_RET_ActivityServerStatus__ mResponse;
            public ActivityServerStatus Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Activity_QueryStatus_RET_ActivityServerStatus__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class QueryBrokerStatusOutMessage : OutMessage
    {
        public QueryBrokerStatusOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Activity, 4040, (int)MessageType.SB)
        {
            Request = new __RPC_Activity_QueryBrokerStatus_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Activity_QueryBrokerStatus_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Activity_QueryBrokerStatus_RET_CommonBrokerStatus__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Activity_QueryBrokerStatus_RET_CommonBrokerStatus__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class ReadyToEnterOutMessage : OutMessage
    {
        public ReadyToEnterOutMessage(ClientAgentBase sender, ulong __characterId__, int placeholder)
            : base(sender, ServiceType.Activity, 4041, (int)MessageType.SAS)
        {
            Request = new __RPC_Activity_ReadyToEnter_ARG_int32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Activity_ReadyToEnter_ARG_int32_placeholder__ Request { get; private set; }

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
                Response.Add(Serializer.Deserialize<__RPC_Activity_ReadyToEnter_RET_int32__>(ms).ReturnValue);
            }
            State = MessageState.Reply;
            if(ErrorCode == 0) ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class UpdateServerOutMessage : OutMessage
    {
        public UpdateServerOutMessage(ClientAgentBase sender, ulong __characterId__, int placeholder)
            : base(sender, ServiceType.Activity, 4042, (int)MessageType.SAS)
        {
            Request = new __RPC_Activity_UpdateServer_ARG_int32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Activity_UpdateServer_ARG_int32_placeholder__ Request { get; private set; }


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

    public class SSApplyActivityStateOutMessage : OutMessage
    {
        public SSApplyActivityStateOutMessage(ClientAgentBase sender, ulong __characterId__, int activityId, int serverId)
            : base(sender, ServiceType.Activity, 4043, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_SSApplyActivityState_ARG_int32_activityId_int32_serverId__();
            mMessage.CharacterId = __characterId__;
            Request.ActivityId=activityId;
            Request.ServerId=serverId;

        }

        public __RPC_Activity_SSApplyActivityState_ARG_int32_activityId_int32_serverId__ Request { get; private set; }

            private __RPC_Activity_SSApplyActivityState_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Activity_SSApplyActivityState_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSApplyPromoteHPOutMessage : OutMessage
    {
        public SSApplyPromoteHPOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int activityId, int batteryId, int promoteType, ulong characterId, string name)
            : base(sender, ServiceType.Activity, 4102, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_SSApplyPromoteHP_ARG_int32_serverId_int32_activityId_int32_batteryId_int32_promoteType_uint64_characterId_string_name__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.ActivityId=activityId;
            Request.BatteryId=batteryId;
            Request.PromoteType=promoteType;
            Request.CharacterId=characterId;
            Request.Name=name;

        }

        public __RPC_Activity_SSApplyPromoteHP_ARG_int32_serverId_int32_activityId_int32_batteryId_int32_promoteType_uint64_characterId_string_name__ Request { get; private set; }

            private __RPC_Activity_SSApplyPromoteHP_RET_BatteryUpdateData__ mResponse;
            public BatteryUpdateData Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Activity_SSApplyPromoteHP_RET_BatteryUpdateData__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSApplyPromoteSkillOutMessage : OutMessage
    {
        public SSApplyPromoteSkillOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int activityId, int batteryId, int promoteType, ulong characterId, string name)
            : base(sender, ServiceType.Activity, 4103, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_SSApplyPromoteSkill_ARG_int32_serverId_int32_activityId_int32_batteryId_int32_promoteType_uint64_characterId_string_name__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.ActivityId=activityId;
            Request.BatteryId=batteryId;
            Request.PromoteType=promoteType;
            Request.CharacterId=characterId;
            Request.Name=name;

        }

        public __RPC_Activity_SSApplyPromoteSkill_ARG_int32_serverId_int32_activityId_int32_batteryId_int32_promoteType_uint64_characterId_string_name__ Request { get; private set; }

            private __RPC_Activity_SSApplyPromoteSkill_RET_BatteryUpdateData__ mResponse;
            public BatteryUpdateData Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Activity_SSApplyPromoteSkill_RET_BatteryUpdateData__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSApplyJoinActivityOutMessage : OutMessage
    {
        public SSApplyJoinActivityOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int activityId)
            : base(sender, ServiceType.Activity, 4106, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_SSApplyJoinActivity_ARG_int32_serverId_int32_activityId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.ActivityId=activityId;

        }

        public __RPC_Activity_SSApplyJoinActivity_ARG_int32_serverId_int32_activityId__ Request { get; private set; }

            private __RPC_Activity_SSApplyJoinActivity_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Activity_SSApplyJoinActivity_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSSyncMieShiDataOutMessage : OutMessage
    {
        public SSSyncMieShiDataOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int activityId, MieShiSceneData list)
            : base(sender, ServiceType.Activity, 4110, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_SSSyncMieShiData_ARG_int32_serverId_int32_activityId_MieShiSceneData_list__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.ActivityId=activityId;
            Request.List=list;

        }

        public __RPC_Activity_SSSyncMieShiData_ARG_int32_serverId_int32_activityId_MieShiSceneData_list__ Request { get; private set; }

            private __RPC_Activity_SSSyncMieShiData_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Activity_SSSyncMieShiData_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSApplyContributeRateOutMessage : OutMessage
    {
        public SSApplyContributeRateOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int activityId)
            : base(sender, ServiceType.Activity, 4111, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_SSApplyContributeRate_ARG_int32_serverId_int32_activityId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.ActivityId=activityId;

        }

        public __RPC_Activity_SSApplyContributeRate_ARG_int32_serverId_int32_activityId__ Request { get; private set; }

            private __RPC_Activity_SSApplyContributeRate_RET_ContriRateList__ mResponse;
            public ContriRateList Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Activity_SSApplyContributeRate_RET_ContriRateList__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSSaveActivityResultOutMessage : OutMessage
    {
        public SSSaveActivityResultOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int activityId, int result)
            : base(sender, ServiceType.Activity, 4112, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_SSSaveActivityResult_ARG_int32_serverId_int32_activityId_int32_result__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.ActivityId=activityId;
            Request.Result=result;

        }

        public __RPC_Activity_SSSaveActivityResult_ARG_int32_serverId_int32_activityId_int32_result__ Request { get; private set; }

            private __RPC_Activity_SSSaveActivityResult_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Activity_SSSaveActivityResult_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSApplyActiResultListOutMessage : OutMessage
    {
        public SSApplyActiResultListOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId)
            : base(sender, ServiceType.Activity, 4113, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_SSApplyActiResultList_ARG_int32_serverId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;

        }

        public __RPC_Activity_SSApplyActiResultList_ARG_int32_serverId__ Request { get; private set; }

            private __RPC_Activity_SSApplyActiResultList_RET_MieShiActivityResultList__ mResponse;
            public MieShiActivityResultList Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Activity_SSApplyActiResultList_RET_MieShiActivityResultList__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSSetAndGetActivityDataOutMessage : OutMessage
    {
        public SSSetAndGetActivityDataOutMessage(ClientAgentBase sender, ulong __characterId__, ulong sceneGuid, int serverId, int activityId, MieShiBatteryGuid guidList)
            : base(sender, ServiceType.Activity, 4114, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_SSSetAndGetActivityData_ARG_uint64_sceneGuid_int32_serverId_int32_activityId_MieShiBatteryGuid_guidList__();
            mMessage.CharacterId = __characterId__;
            Request.SceneGuid=sceneGuid;
            Request.ServerId=serverId;
            Request.ActivityId=activityId;
            Request.GuidList=guidList;

        }

        public __RPC_Activity_SSSetAndGetActivityData_ARG_uint64_sceneGuid_int32_serverId_int32_activityId_MieShiBatteryGuid_guidList__ Request { get; private set; }

            private __RPC_Activity_SSSetAndGetActivityData_RET_CommonActivityInfo__ mResponse;
            public CommonActivityInfo Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Activity_SSSetAndGetActivityData_RET_CommonActivityInfo__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SyncActivityAllPlayerExitOutMessage : OutMessage
    {
        public SyncActivityAllPlayerExitOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int activityId)
            : base(sender, ServiceType.Activity, 4116, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_SyncActivityAllPlayerExit_ARG_int32_serverId_int32_activityId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.ActivityId=activityId;

        }

        public __RPC_Activity_SyncActivityAllPlayerExit_ARG_int32_serverId_int32_activityId__ Request { get; private set; }


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

    public class SSAcientBattleSceneRequestOutMessage : OutMessage
    {
        public SSAcientBattleSceneRequestOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int sceneid, int npcId, int isDie)
            : base(sender, ServiceType.Activity, 4117, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_SSAcientBattleSceneRequest_ARG_int32_serverId_int32_sceneid_int32_npcId_int32_isDie__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.Sceneid=sceneid;
            Request.NpcId=npcId;
            Request.IsDie=isDie;

        }

        public __RPC_Activity_SSAcientBattleSceneRequest_ARG_int32_serverId_int32_sceneid_int32_npcId_int32_isDie__ Request { get; private set; }

            private __RPC_Activity_SSAcientBattleSceneRequest_RET_Dict_int_int_Data__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Activity_SSAcientBattleSceneRequest_RET_Dict_int_int_Data__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSApplyMieShiCanInOutMessage : OutMessage
    {
        public SSApplyMieShiCanInOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int activityId)
            : base(sender, ServiceType.Activity, 4015, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_SSApplyMieShiCanIn_ARG_int32_serverId_int32_activityId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.ActivityId=activityId;

        }

        public __RPC_Activity_SSApplyMieShiCanIn_ARG_int32_serverId_int32_activityId__ Request { get; private set; }

            private __RPC_Activity_SSApplyMieShiCanIn_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Activity_SSApplyMieShiCanIn_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSSyncMieShiBoxCanPickUpOutMessage : OutMessage
    {
        public SSSyncMieShiBoxCanPickUpOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int activityId, int npcId)
            : base(sender, ServiceType.Activity, 4016, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_SSSyncMieShiBoxCanPickUp_ARG_int32_serverId_int32_activityId_int32_npcId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.ActivityId=activityId;
            Request.NpcId=npcId;

        }

        public __RPC_Activity_SSSyncMieShiBoxCanPickUp_ARG_int32_serverId_int32_activityId_int32_npcId__ Request { get; private set; }

            private __RPC_Activity_SSSyncMieShiBoxCanPickUp_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Activity_SSSyncMieShiBoxCanPickUp_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSApplyPickUpBoxOutMessage : OutMessage
    {
        public SSApplyPickUpBoxOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int activityId, int npcId)
            : base(sender, ServiceType.Activity, 4017, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_SSApplyPickUpBox_ARG_int32_serverId_int32_activityId_int32_npcId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.ActivityId=activityId;
            Request.NpcId=npcId;

        }

        public __RPC_Activity_SSApplyPickUpBox_ARG_int32_serverId_int32_activityId_int32_npcId__ Request { get; private set; }

            private __RPC_Activity_SSApplyPickUpBox_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Activity_SSApplyPickUpBox_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSApplyPortraitAwardOutMessage : OutMessage
    {
        public SSApplyPortraitAwardOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId)
            : base(sender, ServiceType.Activity, 4019, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_SSApplyPortraitAward_ARG_int32_serverId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;

        }

        public __RPC_Activity_SSApplyPortraitAward_ARG_int32_serverId__ Request { get; private set; }

            private __RPC_Activity_SSApplyPortraitAward_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Activity_SSApplyPortraitAward_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSSaveBatteryDestroyOutMessage : OutMessage
    {
        public SSSaveBatteryDestroyOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int activityId, ulong batteryGuid)
            : base(sender, ServiceType.Activity, 4020, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_SSSaveBatteryDestroy_ARG_int32_serverId_int32_activityId_uint64_batteryGuid__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.ActivityId=activityId;
            Request.BatteryGuid=batteryGuid;

        }

        public __RPC_Activity_SSSaveBatteryDestroy_ARG_int32_serverId_int32_activityId_uint64_batteryGuid__ Request { get; private set; }

            private __RPC_Activity_SSSaveBatteryDestroy_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Activity_SSSaveBatteryDestroy_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSApplyLastResultOutMessage : OutMessage
    {
        public SSApplyLastResultOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId)
            : base(sender, ServiceType.Activity, 4021, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_SSApplyLastResult_ARG_int32_serverId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;

        }

        public __RPC_Activity_SSApplyLastResult_ARG_int32_serverId__ Request { get; private set; }

            private __RPC_Activity_SSApplyLastResult_RET_MieshiLastResult__ mResponse;
            public MieshiLastResult Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Activity_SSApplyLastResult_RET_MieshiLastResult__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SBCleanClientCharacterDataOutMessage : OutMessage
    {
        public SBCleanClientCharacterDataOutMessage(ClientAgentBase sender, ulong __characterId__, ulong clientId, ulong characterId)
            : base(sender, ServiceType.Activity, 4500, (int)MessageType.SB)
        {
            Request = new __RPC_Activity_SBCleanClientCharacterData_ARG_uint64_clientId_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.ClientId=clientId;
            Request.CharacterId=characterId;

        }

        public __RPC_Activity_SBCleanClientCharacterData_ARG_uint64_clientId_uint64_characterId__ Request { get; private set; }


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
            : base(sender, ServiceType.Activity, 4501, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_SSNotifyCharacterOnConnet_ARG_uint64_clientId_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.ClientId=clientId;
            Request.CharacterId=characterId;

        }

        public __RPC_Activity_SSNotifyCharacterOnConnet_ARG_uint64_clientId_uint64_characterId__ Request { get; private set; }

            private __RPC_Activity_SSNotifyCharacterOnConnet_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Activity_SSNotifyCharacterOnConnet_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSAskMieshiTowerUpTimesOutMessage : OutMessage
    {
        public SSAskMieshiTowerUpTimesOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int activityId, ulong characterId)
            : base(sender, ServiceType.Activity, 4022, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_SSAskMieshiTowerUpTimes_ARG_int32_serverId_int32_activityId_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.ActivityId=activityId;
            Request.CharacterId=characterId;

        }

        public __RPC_Activity_SSAskMieshiTowerUpTimes_ARG_int32_serverId_int32_activityId_uint64_characterId__ Request { get; private set; }

            private __RPC_Activity_SSAskMieshiTowerUpTimes_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Activity_SSAskMieshiTowerUpTimes_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSAskMieshiTowerRewardOutMessage : OutMessage
    {
        public SSAskMieshiTowerRewardOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int activityId, ulong characterId, int idx)
            : base(sender, ServiceType.Activity, 4023, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_SSAskMieshiTowerReward_ARG_int32_serverId_int32_activityId_uint64_characterId_int32_idx__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.ActivityId=activityId;
            Request.CharacterId=characterId;
            Request.Idx=idx;

        }

        public __RPC_Activity_SSAskMieshiTowerReward_ARG_int32_serverId_int32_activityId_uint64_characterId_int32_idx__ Request { get; private set; }

            private __RPC_Activity_SSAskMieshiTowerReward_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Activity_SSAskMieshiTowerReward_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class BossHomeSceneRequestOutMessage : OutMessage
    {
        public BossHomeSceneRequestOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int sceneid, int npcId, int isDie)
            : base(sender, ServiceType.Activity, 4024, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_BossHomeSceneRequest_ARG_int32_serverId_int32_sceneid_int32_npcId_int32_isDie__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.Sceneid=sceneid;
            Request.NpcId=npcId;
            Request.IsDie=isDie;

        }

        public __RPC_Activity_BossHomeSceneRequest_ARG_int32_serverId_int32_sceneid_int32_npcId_int32_isDie__ Request { get; private set; }

            private __RPC_Activity_BossHomeSceneRequest_RET_Dict_int_int_Data__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Activity_BossHomeSceneRequest_RET_Dict_int_int_Data__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSGetTreasureShopItemsOutMessage : OutMessage
    {
        public SSGetTreasureShopItemsOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId)
            : base(sender, ServiceType.Activity, 4026, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_SSGetTreasureShopItems_ARG_int32_serverId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;

        }

        public __RPC_Activity_SSGetTreasureShopItems_ARG_int32_serverId__ Request { get; private set; }

            private __RPC_Activity_SSGetTreasureShopItems_RET_StoneItems__ mResponse;
            public StoneItems Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Activity_SSGetTreasureShopItems_RET_StoneItems__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSGetTreasureShopItemCountOutMessage : OutMessage
    {
        public SSGetTreasureShopItemCountOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int storeId)
            : base(sender, ServiceType.Activity, 4027, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_SSGetTreasureShopItemCount_ARG_int32_serverId_int32_storeId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.StoreId=storeId;

        }

        public __RPC_Activity_SSGetTreasureShopItemCount_ARG_int32_serverId_int32_storeId__ Request { get; private set; }

            private __RPC_Activity_SSGetTreasureShopItemCount_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Activity_SSGetTreasureShopItemCount_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSConsumeTreasureShopItemOutMessage : OutMessage
    {
        public SSConsumeTreasureShopItemOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int storeId, int consumeCount)
            : base(sender, ServiceType.Activity, 4028, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_SSConsumeTreasureShopItem_ARG_int32_serverId_int32_storeId_int32_consumeCount__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.StoreId=storeId;
            Request.ConsumeCount=consumeCount;

        }

        public __RPC_Activity_SSConsumeTreasureShopItem_ARG_int32_serverId_int32_storeId_int32_consumeCount__ Request { get; private set; }

            private __RPC_Activity_SSConsumeTreasureShopItem_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Activity_SSConsumeTreasureShopItem_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSGetBlackStoreItemsOutMessage : OutMessage
    {
        public SSGetBlackStoreItemsOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId)
            : base(sender, ServiceType.Activity, 4029, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_SSGetBlackStoreItems_ARG_int32_serverId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;

        }

        public __RPC_Activity_SSGetBlackStoreItems_ARG_int32_serverId__ Request { get; private set; }

            private __RPC_Activity_SSGetBlackStoreItems_RET_StoneItems__ mResponse;
            public StoneItems Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Activity_SSGetBlackStoreItems_RET_StoneItems__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSSyncChickenScoreOutMessage : OutMessage
    {
        public SSSyncChickenScoreOutMessage(ClientAgentBase sender, ulong __characterId__, ChickenRankData rankData)
            : base(sender, ServiceType.Activity, 4505, (int)MessageType.SS)
        {
            Request = new __RPC_Activity_SSSyncChickenScore_ARG_ChickenRankData_rankData__();
            mMessage.CharacterId = __characterId__;
            Request.RankData=rankData;

        }

        public __RPC_Activity_SSSyncChickenScore_ARG_ChickenRankData_rankData__ Request { get; private set; }


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

    public class AddFunctionNameActivity
    {
        public static void AddFunctionName(IDictionary<int, string> dict)
        {
            dict[4000] = "PrepareDataForEnterGame";
            dict[4001] = "PrepareDataForCreateCharacter";
            dict[4002] = "PrepareDataForCommonUse";
            dict[4003] = "PrepareDataForLogout";
            dict[4004] = "ServerGMCommand";
            dict[4030] = "SBGetAllOnlineCharacterInServer";
            dict[4031] = "CheckConnected";
            dict[4032] = "CheckLost";
            dict[4033] = "QueryCreateMonsterData";
            dict[4034] = "NotifyDamageList";
            dict[4035] = "QueryStatus";
            dict[4040] = "QueryBrokerStatus";
            dict[4041] = "ReadyToEnter";
            dict[4042] = "UpdateServer";
            dict[4043] = "SSApplyActivityState";
            dict[4044] = "ApplyActivityState";
            dict[4045] = "NotifyActivityState";
            dict[4046] = "ApplyOrderSerial";
            dict[4047] = "NotifyTableChange";
            dict[4100] = "ApplyMieShiData";
            dict[4099] = "ApplyMieshiHeroLogData";
            dict[4101] = "ApplyBatteryData";
            dict[4102] = "SSApplyPromoteHP";
            dict[4103] = "SSApplyPromoteSkill";
            dict[4104] = "ApplyContriRankingData";
            dict[4105] = "ApplyPointRankingData";
            dict[4106] = "SSApplyJoinActivity";
            dict[4107] = "NotifyBatteryData";
            dict[4108] = "NotifyMieShiActivityState";
            dict[4115] = "NotifyMieShiActivityInfo";
            dict[4109] = "NotifyPlayerCanIn";
            dict[4110] = "SSSyncMieShiData";
            dict[4111] = "SSApplyContributeRate";
            dict[4112] = "SSSaveActivityResult";
            dict[4113] = "SSApplyActiResultList";
            dict[4114] = "SSSetAndGetActivityData";
            dict[4116] = "SyncActivityAllPlayerExit";
            dict[4117] = "SSAcientBattleSceneRequest";
            dict[4118] = "ApplyAcientBattle";
            dict[4015] = "SSApplyMieShiCanIn";
            dict[4016] = "SSSyncMieShiBoxCanPickUp";
            dict[4017] = "SSApplyPickUpBox";
            dict[4018] = "ApplyPortraitData";
            dict[4019] = "SSApplyPortraitAward";
            dict[4020] = "SSSaveBatteryDestroy";
            dict[4021] = "SSApplyLastResult";
            dict[4500] = "SBCleanClientCharacterData";
            dict[4501] = "SSNotifyCharacterOnConnet";
            dict[4503] = "BSNotifyCharacterOnLost";
            dict[4022] = "SSAskMieshiTowerUpTimes";
            dict[4023] = "SSAskMieshiTowerReward";
            dict[4024] = "BossHomeSceneRequest";
            dict[4025] = "ApplyBossHome";
            dict[4026] = "SSGetTreasureShopItems";
            dict[4027] = "SSGetTreasureShopItemCount";
            dict[4028] = "SSConsumeTreasureShopItem";
            dict[4029] = "SSGetBlackStoreItems";
            dict[4505] = "SSSyncChickenScore";
            dict[4506] = "ApplyChickenRankData";
        }
        public static void AddCSFunctionName(IDictionary<int, string> dict)
        {
            dict[4044] = "ApplyActivityState";
            dict[4046] = "ApplyOrderSerial";
            dict[4100] = "ApplyMieShiData";
            dict[4099] = "ApplyMieshiHeroLogData";
            dict[4101] = "ApplyBatteryData";
            dict[4104] = "ApplyContriRankingData";
            dict[4105] = "ApplyPointRankingData";
            dict[4118] = "ApplyAcientBattle";
            dict[4018] = "ApplyPortraitData";
            dict[4025] = "ApplyBossHome";
            dict[4506] = "ApplyChickenRankData";
        }
    }
}
