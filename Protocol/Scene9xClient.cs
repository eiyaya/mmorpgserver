using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Scorpion;
using DataContract;
using ProtoBuf;

#pragma warning disable 0162,0108
namespace SceneClientService
{

    public abstract class SceneAgent : ClientAgentBase
    {
        public SceneAgent(string addr)
            : base(addr)
        {
        }

        public SceneAgent(ServerInfo broker, ServerInfo[] directConnect, Func<ulong, int> characterId2ServerId)
            : base(broker, directConnect, characterId2ServerId)
        {
        }

        /// <summary>
        /// 通知Broker换场景
        /// </summary>
        public SBChangeSceneOutMessage SBChangeScene(ulong __characterId__, ulong characterId, int serverId, int sceneId, ulong guid, int changeType, SceneParam sceneParam)
        {
            return new SBChangeSceneOutMessage(this, __characterId__, characterId, serverId, sceneId, guid, changeType, sceneParam);
        }

        /// <summary>
        /// </summary>
        public SBDestroySceneOutMessage SBDestroyScene(ulong __characterId__, ulong guid)
        {
            return new SBDestroySceneOutMessage(this, __characterId__, guid);
        }

        /// <summary>
        /// 通知Broker 这个id的场景不存在，character需要重新切换场景
        /// </summary>
        public NotifySceneNotExistOutMessage NotifySceneNotExist(ulong __characterId__, ulong sceneId, ulong characterId)
        {
            return new NotifySceneNotExistOutMessage(this, __characterId__, sceneId, characterId);
        }

        /// <summary>
        /// 获得某个逻辑服务器的所有在线CharacterId
        /// </summary>
        public SBGetAllOnlineCharacterInServerOutMessage SBGetAllOnlineCharacterInServer(ulong __characterId__, int serverId)
        {
            return new SBGetAllOnlineCharacterInServerOutMessage(this, __characterId__, serverId);
        }

        /// <summary>
        /// 查询这些玩家是否在线
        /// </summary>
        public SBCheckCharacterOnlineOutMessage SBCheckCharacterOnline(ulong __characterId__, Uint64Array toList)
        {
            return new SBCheckCharacterOnlineOutMessage(this, __characterId__, toList);
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
        public SBReloadTableOutMessage SBReloadTable(ulong __characterId__, string tableName)
        {
            return new SBReloadTableOutMessage(this, __characterId__, tableName);
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
        public CreateCharacterOutMessage CreateCharacter(ulong __characterId__, int type, bool isGM)
        {
            return new CreateCharacterOutMessage(this, __characterId__, type, isGM);
        }

        /// <summary>
        /// </summary>
        public DelectCharacterOutMessage DelectCharacter(ulong __characterId__, int type)
        {
            return new DelectCharacterOutMessage(this, __characterId__, type);
        }

        /// <summary>
        /// 
        /// </summary>
        public SSEnterSceneOutMessage SSEnterScene(ulong __characterId__, ulong characterId, ulong guid, ulong applyGuid, int changeType, SceneParam sceneParam)
        {
            return new SSEnterSceneOutMessage(this, __characterId__, characterId, guid, applyGuid, changeType, sceneParam);
        }

        /// <summary>
        /// 获得Scene的Simple数据
        /// </summary>
        public GetSceneSimpleDataOutMessage GetSceneSimpleData(ulong __characterId__, uint placeholder)
        {
            return new GetSceneSimpleDataOutMessage(this, __characterId__, placeholder);
        }

        /// <summary>
        /// 检查客户端是否已经连接到对应服务器，如果对方没有Connected，这个函数会卡住直到相应的客户端Connected
        /// </summary>
        public CheckConnectedOutMessage CheckConnected(ulong __characterId__, ulong characterId)
        {
            return new CheckConnectedOutMessage(this, __characterId__, characterId);
        }

        /// <summary>
        /// 检查相应客户端连接是否已经断开，如果对方没有Lost，这个函数会卡住知道对方Lost
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
        /// 通知Broker进入Connected
        /// </summary>
        public NotifyConnectedOutMessage NotifyConnected(ulong __characterId__, uint placeholder)
        {
            return new NotifyConnectedOutMessage(this, __characterId__, placeholder);
        }

        /// <summary>
        /// 通知Broker进入Lost
        /// </summary>
        public NotifyLostOutMessage NotifyLost(ulong __characterId__, uint placeholder)
        {
            return new NotifyLostOutMessage(this, __characterId__, placeholder);
        }

        /// <summary>
        /// 通知Broker这个场景不能再进人了
        /// </summary>
        public NotifySceneFinishedOutMessage NotifySceneFinished(ulong __characterId__, ulong guid)
        {
            return new NotifySceneFinishedOutMessage(this, __characterId__, guid);
        }

        /// <summary>
        /// 请求进入副本
        /// </summary>
        public AskEnterDungeonOutMessage AskEnterDungeon(ulong __characterId__, int serverId, int sceneId, ulong guid, SceneParam param)
        {
            return new AskEnterDungeonOutMessage(this, __characterId__, serverId, sceneId, guid, param);
        }

        /// <summary>
        /// </summary>
        public NotifyPlayerPickUpFubenRewardOutMessage NotifyPlayerPickUpFubenReward(ulong __characterId__, ulong characterId)
        {
            return new NotifyPlayerPickUpFubenRewardOutMessage(this, __characterId__, characterId);
        }

        /// <summary>
        /// 请求进入与队伍某人相同的场景
        /// </summary>
        public SBChangeSceneByTeamOutMessage SBChangeSceneByTeam(ulong __characterId__, ChangeSceneInfo changeSceneData)
        {
            return new SBChangeSceneByTeamOutMessage(this, __characterId__, changeSceneData);
        }

        /// <summary>
        /// 请求进入副本(不要用这个接口了)
        /// SB void					AskEnterDungeonByTeam(ChangeSceneInfo changeSceneData) = 3070;
        /// 组队后把相同场景id的人合并到同一个场景中
        /// </summary>
        public MergeSceneByTeamOutMessage MergeSceneByTeam(ulong __characterId__, IdList ids)
        {
            return new MergeSceneByTeamOutMessage(this, __characterId__, ids);
        }

        /// <summary>
        /// 问Broker这个场景在不在
        /// </summary>
        public IsSceneExistOutMessage IsSceneExist(ulong __characterId__, ulong sceneGuid)
        {
            return new IsSceneExistOutMessage(this, __characterId__, sceneGuid);
        }

        /// <summary>
        /// login请求准备数据完毕请求scene进入场景
        /// </summary>
        public LoginEnterSceneOutMessage LoginEnterScene(ulong __characterId__, int serverId, long logout)
        {
            return new LoginEnterSceneOutMessage(this, __characterId__, serverId, logout);
        }

        /// <summary>
        /// 同步装备变化
        /// </summary>
        public SceneEquipChangeOutMessage SceneEquipChange(ulong __characterId__, int type, int part, ItemBaseData equip)
        {
            return new SceneEquipChangeOutMessage(this, __characterId__, type, part, equip);
        }

        /// <summary>
        /// 同步技能变化
        /// </summary>
        public SceneSkillChangeOutMessage SceneSkillChange(ulong __characterId__, int type, int id, int level)
        {
            return new SceneSkillChangeOutMessage(this, __characterId__, type, id, level);
        }

        /// <summary>
        /// 同步装备的技能
        /// </summary>
        public SceneEquipSkillOutMessage SceneEquipSkill(ulong __characterId__, Int32Array delSkills, Int32Array SkillIds, Int32Array SkillLevels)
        {
            return new SceneEquipSkillOutMessage(this, __characterId__, delSkills, SkillIds, SkillLevels);
        }

        /// <summary>
        /// 同步天赋变化
        /// </summary>
        public SceneInnateChangeOutMessage SceneInnateChange(ulong __characterId__, int type, int id, int level)
        {
            return new SceneInnateChangeOutMessage(this, __characterId__, type, id, level);
        }

        /// <summary>
        /// 同步图鉴属性
        /// </summary>
        public SceneBookAttrChangeOutMessage SceneBookAttrChange(ulong __characterId__, Dict_int_int_Data attrs, Dict_int_int_Data monsterAttrs)
        {
            return new SceneBookAttrChangeOutMessage(this, __characterId__, attrs, monsterAttrs);
        }

        /// <summary>
        /// 组队消息
        /// </summary>
        public SceneTeamMessageOutMessage SceneTeamMessage(ulong __characterId__, ulong characterId, int type, ulong teamId, int state)
        {
            return new SceneTeamMessageOutMessage(this, __characterId__, characterId, type, teamId, state);
        }

        /// <summary>
        /// 使用技能道具
        /// </summary>
        public UseSkillItemOutMessage UseSkillItem(ulong __characterId__, int itemId, int count, int bagId, int bagIndex)
        {
            return new UseSkillItemOutMessage(this, __characterId__, itemId, count, bagId, bagIndex);
        }

        /// <summary>
        /// 广播表格重载
        /// </summary>
        public ServerGMCommandOutMessage ServerGMCommand(string cmd, string param)
        {
            return new ServerGMCommandOutMessage(this, 0, cmd, param);
        }

        /// <summary>
        /// 获得数据接口
        /// </summary>
        public FindCharacterNameOutMessage FindCharacterName(string likeName)
        {
            return new FindCharacterNameOutMessage(this, 0, likeName);
        }

        /// <summary>
        /// 获得数据接口
        /// </summary>
        public FindCharacterFriendOutMessage FindCharacterFriend(int serverId, int level)
        {
            return new FindCharacterFriendOutMessage(this, 0, serverId, level);
        }

        /// <summary>
        /// 同步家园数据到场景服务器
        /// </summary>
        public NotifyScenePlayerCityDataOutMessage NotifyScenePlayerCityData(ulong sceneGuid, BuildingList data)
        {
            return new NotifyScenePlayerCityDataOutMessage(this, 0, sceneGuid, data);
        }

        /// <summary>
        /// Logic玩家希望进入天梯场景
        /// </summary>
        public SSGoToSceneAndPvPOutMessage SSGoToSceneAndPvP(ulong __characterId__, int sceneId, ulong PvPcharacterId)
        {
            return new SSGoToSceneAndPvPOutMessage(this, __characterId__, sceneId, PvPcharacterId);
        }

        /// <summary>
        /// 通知场景这些人的坐标归属
        /// </summary>
        public SSPvPSceneCampSetOutMessage SSPvPSceneCampSet(ulong __characterId__, int type)
        {
            return new SSPvPSceneCampSetOutMessage(this, __characterId__, type);
        }

        /// <summary>
        /// 通知各Scene服务器
        /// </summary>
        public NotifyCreateSpeMonsterOutMessage NotifyCreateSpeMonster(Int32Array ids)
        {
            return new NotifyCreateSpeMonsterOutMessage(this, 0, ids);
        }

        /// <summary>
        /// 获得某人的SceneData
        /// </summary>
        public SSGetCharacterSceneDataOutMessage SSGetCharacterSceneData(ulong __characterId__, ulong characterId)
        {
            return new SSGetCharacterSceneDataOutMessage(this, __characterId__, characterId);
        }

        /// <summary>
        /// 通知各个Scene，世界boss死了
        /// </summary>
        public BossDieOutMessage BossDie(uint __serverId__, int serverId)
        {
            return new BossDieOutMessage(this, __serverId__, serverId);
        }

        /// <summary>
        /// 查询服务器状态，是否可以进入
        /// </summary>
        public ReadyToEnterOutMessage ReadyToEnter(int placeholder)
        {
            return new ReadyToEnterOutMessage(this, 0, placeholder);
        }

        /// <summary>
        /// 通知好友上线了，并且取SimpleData
        /// </summary>
        public GetFriendSceneSimpleDataOutMessage GetFriendSceneSimpleData(ulong __characterId__, int type, ulong haveId, ulong getId)
        {
            return new GetFriendSceneSimpleDataOutMessage(this, __characterId__, type, haveId, getId);
        }

        /// <summary>
        /// 通知添加好友了
        /// </summary>
        public SendAddFriendOutMessage SendAddFriend(ulong __characterId__, int type, ulong haveId, ulong getId)
        {
            return new SendAddFriendOutMessage(this, __characterId__, type, haveId, getId);
        }

        /// <summary>
        /// 通知删除好友了
        /// </summary>
        public SendDeleteFriendOutMessage SendDeleteFriend(ulong __characterId__, int type, ulong haveId, ulong getId)
        {
            return new SendDeleteFriendOutMessage(this, __characterId__, type, haveId, getId);
        }

        /// <summary>
        /// 通知好友下线了
        /// </summary>
        public SendOutLineFriendOutMessage SendOutLineFriend(ulong __characterId__, int type, ulong haveId, ulong getId)
        {
            return new SendOutLineFriendOutMessage(this, __characterId__, type, haveId, getId);
        }

        /// <summary>
        /// SS退出副本
        /// </summary>
        public SSExitDungeonOutMessage SSExitDungeon(ulong __characterId__, int placeholder)
        {
            return new SSExitDungeonOutMessage(this, __characterId__, placeholder);
        }

        /// <summary>
        /// 战盟Buff发生变化
        /// </summary>
        public SSAllianceBuffDataChangeOutMessage SSAllianceBuffDataChange(ulong __characterId__, int buffId)
        {
            return new SSAllianceBuffDataChangeOutMessage(this, __characterId__, buffId);
        }

        /// <summary>
        /// 战盟Buff发生变化
        /// </summary>
        public SSAllianceDataChangeOutMessage SSAllianceDataChange(ulong __characterId__, int alianceId, int type, string name)
        {
            return new SSAllianceDataChangeOutMessage(this, __characterId__, alianceId, type, name);
        }

        /// <summary>
        /// 同步称号属性(type = 0表示设置当前使用的称号，type = 1表示刷新已获得的称号列表)
        /// </summary>
        public SceneTitleChangeOutMessage SceneTitleChange(ulong __characterId__, Int32Array titles, int type)
        {
            return new SceneTitleChangeOutMessage(this, __characterId__, titles, type);
        }

        /// <summary>
        /// 通知Scene，玩家的某个物品数量变了
        /// </summary>
        public NotifyItemCountOutMessage NotifyItemCount(ulong __characterId__, int itemId, int count)
        {
            return new NotifyItemCountOutMessage(this, __characterId__, itemId, count);
        }

        /// <summary>
        /// 获得同步服务器平均玩家等级
        /// </summary>
        public MotifyServerAvgLevelOutMessage MotifyServerAvgLevel(Dict_int_int_Data ServerAvgLevel)
        {
            return new MotifyServerAvgLevelOutMessage(this, 0, ServerAvgLevel);
        }

        /// <summary>
        /// 动态激活活动
        /// </summary>
        public AddAutoActvityOutMessage AddAutoActvity(int fubenId, long startTime, long endTime, int count)
        {
            return new AddAutoActvityOutMessage(this, 0, fubenId, startTime, endTime, count);
        }

        /// <summary>
        /// 给某玩家上个buff
        /// </summary>
        public SSAddBuffOutMessage SSAddBuff(ulong __characterId__, ulong characterId, int buffId, int buffLevel)
        {
            return new SSAddBuffOutMessage(this, __characterId__, characterId, buffId, buffLevel);
        }

        /// <summary>
        /// 玩家禁言
        /// </summary>
        public SilenceOutMessage Silence(ulong __characterId__, uint mask)
        {
            return new SilenceOutMessage(this, __characterId__, mask);
        }

        /// <summary>
        /// 查询某个对象剩余血量
        /// </summary>
        public SSApplyNpcHPOutMessage SSApplyNpcHP(ulong __characterId__, int serverId, int sceneId, ulong npcGuid)
        {
            return new SSApplyNpcHPOutMessage(this, __characterId__, serverId, sceneId, npcGuid);
        }

        /// <summary>
        /// 灵兽数据变化
        /// </summary>
        public SSSceneElfChangeOutMessage SSSceneElfChange(ulong __characterId__, Int32Array removeBuff, Dict_int_int_Data addBuff, int fightPoint)
        {
            return new SSSceneElfChangeOutMessage(this, __characterId__, removeBuff, addBuff, fightPoint);
        }

        /// <summary>
        /// 
        /// </summary>
        public SBReconnectNotifySceneOutMessage SBReconnectNotifyScene(ulong __characterId__, ulong oldclientId, ulong newclientId, ulong characterId)
        {
            return new SBReconnectNotifySceneOutMessage(this, __characterId__, oldclientId, newclientId, characterId);
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
        /// 查询各个服务器人数
        /// </summary>
        public SBGetServerCharacterCountOutMessage SBGetServerCharacterCount(ulong __characterId__, uint placeholder)
        {
            return new SBGetServerCharacterCountOutMessage(this, __characterId__, placeholder);
        }

        /// <summary>
        /// 灭世
        /// </summary>
        public SyncTowerSkillLevelOutMessage SyncTowerSkillLevel(ulong sceneGuid, int towerId, int level)
        {
            return new SyncTowerSkillLevelOutMessage(this, 0, sceneGuid, towerId, level);
        }

        /// <summary>
        /// </summary>
        public SyncPlayerMieshiContributionOutMessage SyncPlayerMieshiContribution(ulong sceneGuid, ulong characterId, int Contribution, string name, float rate)
        {
            return new SyncPlayerMieshiContributionOutMessage(this, 0, sceneGuid, characterId, Contribution, name, rate);
        }

        /// <summary>
        /// 问Broker这个类型场景的信息
        /// </summary>
        public RequestSceneInfoOutMessage RequestSceneInfo(ulong __characterId__, int serverId, int sceneTypeId)
        {
            return new RequestSceneInfoOutMessage(this, __characterId__, serverId, sceneTypeId);
        }

        /// <summary>
        /// 通知scene 刷新扩展数据
        /// </summary>
        public SyncExDataOutMessage SyncExData(ulong __characterId__, Dict_int_int_Data changes)
        {
            return new SyncExDataOutMessage(this, __characterId__, changes);
        }

        /// <summary>
        /// </summary>
        public SyncSceneMountOutMessage SyncSceneMount(ulong __characterId__, int MountId)
        {
            return new SyncSceneMountOutMessage(this, __characterId__, MountId);
        }

        /// <summary>
        /// 请求副本商店数据
        /// </summary>
        public SSGetFubenStoreItemsOutMessage SSGetFubenStoreItems(ulong __characterId__, int shopType)
        {
            return new SSGetFubenStoreItemsOutMessage(this, __characterId__, shopType);
        }

        /// <summary>
        /// </summary>
        public SSGetFubenStoreItemCountOutMessage SSGetFubenStoreItemCount(ulong __characterId__, int shopType, int id)
        {
            return new SSGetFubenStoreItemCountOutMessage(this, __characterId__, shopType, id);
        }

        /// <summary>
        /// </summary>
        public SSChangeFubenStoreItemOutMessage SSChangeFubenStoreItem(ulong __characterId__, int shopType, int id, int num)
        {
            return new SSChangeFubenStoreItemOutMessage(this, __characterId__, shopType, id, num);
        }

        /// <summary>
        /// 同步魔物出站ID
        /// </summary>
        public SSBookFightingMonsterIdOutMessage SSBookFightingMonsterId(ulong __characterId__, int handbookId)
        {
            return new SSBookFightingMonsterIdOutMessage(this, __characterId__, handbookId);
        }

        /// <summary>
        /// 任务切换场景
        /// </summary>
        public MissionChangeSceneRequestOutMessage MissionChangeSceneRequest(ulong __characterId__, int transId)
        {
            return new MissionChangeSceneRequestOutMessage(this, __characterId__, transId);
        }

        /// <summary>
        /// ----------------------------------------------------------------------------GM相关放在最后
        /// GM相关 begin
        /// </summary>
        public GetCharacterDataOutMessage GetCharacterData(ulong __characterId__, ulong id)
        {
            return new GetCharacterDataOutMessage(this, __characterId__, id);
        }

        /// <summary>
        /// 更新服务器
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
        /// 拷贝一个角色数据到另一个角色id
        /// </summary>
        public CloneCharacterDbByIdOutMessage CloneCharacterDbById(ulong __characterId__, ulong fromId, ulong toId)
        {
            return new CloneCharacterDbByIdOutMessage(this, __characterId__, fromId, toId);
        }

        /// <summary>
        /// 修改战盟占领属性
        /// </summary>
        public UpdateHoldIdOutMessage UpdateHoldId(ulong __characterId__, int GuildId)
        {
            return new UpdateHoldIdOutMessage(this, __characterId__, GuildId);
        }

        /// <summary>
        /// 通知scene 刷新扩展数据
        /// </summary>
        public NotifyRefreshLodeTimerOutMessage NotifyRefreshLodeTimer(int ServerId, Int32Array ids)
        {
            return new NotifyRefreshLodeTimerOutMessage(this, 0, ServerId, ids);
        }

        /// <summary>
        /// </summary>
        public SyncFlagDataOutMessage SyncFlagData(ulong __characterId__, Dict_int_int_Data changes)
        {
            return new SyncFlagDataOutMessage(this, __characterId__, changes);
        }

        /// <summary>
        /// 同步时装状态
        /// </summary>
        public SceneEquipModelStateChangeOutMessage SceneEquipModelStateChange(ulong __characterId__, int part, int state, ItemBaseData equip)
        {
            return new SceneEquipModelStateChangeOutMessage(this, __characterId__, part, state, equip);
        }

        /// <summary>
        /// 通知修改玩家名字
        /// </summary>
        public NodifyModifyPlayerNameOutMessage NodifyModifyPlayerName(ulong characterId, string modifyName)
        {
            return new NodifyModifyPlayerNameOutMessage(this, 0, characterId, modifyName);
        }

        /// <summary>
        /// 通知各Scene服务器
        /// </summary>
        public NotifyRefreshBossHomeOutMessage NotifyRefreshBossHome(Int32Array ids)
        {
            return new NotifyRefreshBossHomeOutMessage(this, 0, ids);
        }

        /// <summary>
        /// </summary>
        public NotifyBossHomeKillOutMessage NotifyBossHomeKill(int placeholder)
        {
            return new NotifyBossHomeKillOutMessage(this, 0, placeholder);
        }

        /// <summary>
        /// 通知scene检查character是否能够接受决斗
        /// </summary>
        public CheckCanAcceptChallengeOutMessage CheckCanAcceptChallenge(ulong __characterId__, int placeholder)
        {
            return new CheckCanAcceptChallengeOutMessage(this, __characterId__, placeholder);
        }

        protected override object GetPublishData(uint p, byte[] list)
        {
            switch (p)
            {
                case 3000:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SBChangeScene_ARG_uint64_characterId_int32_serverId_int32_sceneId_uint64_guid_int32_changeType_SceneParam_sceneParam__>(ms);
                    }
                    break;
                case 3001:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SBDestroyScene_ARG_uint64_guid__>(ms);
                    }
                    break;
                case 3002:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_NotifySceneNotExist_ARG_uint64_sceneId_uint64_characterId__>(ms);
                    }
                    break;
                case 3030:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SBGetAllOnlineCharacterInServer_ARG_int32_serverId__>(ms);
                    }
                    break;
                case 3039:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SBCheckCharacterOnline_ARG_Uint64Array_toList__>(ms);
                    }
                    break;
                case 3040:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_QueryBrokerStatus_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 3050:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SBReloadTable_ARG_string_tableName__>(ms);
                    }
                    break;
                case 3051:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_PrepareDataForEnterGame_ARG_int32_serverId__>(ms);
                    }
                    break;
                case 3052:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_PrepareDataForCreateCharacter_ARG_int32_type__>(ms);
                    }
                    break;
                case 3053:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_PrepareDataForCommonUse_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 3054:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_PrepareDataForLogout_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 3015:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_CreateCharacter_ARG_int32_type_bool_isGM__>(ms);
                    }
                    break;
                case 3016:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_DelectCharacter_ARG_int32_type__>(ms);
                    }
                    break;
                case 3055:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SSEnterScene_ARG_uint64_characterId_uint64_guid_uint64_applyGuid_int32_changeType_SceneParam_sceneParam__>(ms);
                    }
                    break;
                case 3056:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_GetSceneSimpleData_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 3057:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_CheckConnected_ARG_uint64_characterId__>(ms);
                    }
                    break;
                case 3058:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_CheckLost_ARG_uint64_characterId__>(ms);
                    }
                    break;
                case 3059:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_QueryStatus_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 3061:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_NotifyConnected_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 3062:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_NotifyLost_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 3063:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_NotifySceneFinished_ARG_uint64_guid__>(ms);
                    }
                    break;
                case 3064:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_AskEnterDungeon_ARG_int32_serverId_int32_sceneId_uint64_guid_SceneParam_param__>(ms);
                    }
                    break;
                case 3066:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_NotifyPlayerPickUpFubenReward_ARG_uint64_characterId__>(ms);
                    }
                    break;
                case 3069:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SBChangeSceneByTeam_ARG_ChangeSceneInfo_changeSceneData__>(ms);
                    }
                    break;
                case 3071:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_MergeSceneByTeam_ARG_IdList_ids__>(ms);
                    }
                    break;
                case 3072:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_IsSceneExist_ARG_uint64_sceneGuid__>(ms);
                    }
                    break;
                case 3073:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_LoginEnterScene_ARG_int32_serverId_int64_logout__>(ms);
                    }
                    break;
                case 3092:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SceneEquipChange_ARG_int32_type_int32_part_ItemBaseData_equip__>(ms);
                    }
                    break;
                case 3093:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SceneSkillChange_ARG_int32_type_int32_id_int32_level__>(ms);
                    }
                    break;
                case 3094:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SceneEquipSkill_ARG_Int32Array_delSkills_Int32Array_SkillIds_Int32Array_SkillLevels__>(ms);
                    }
                    break;
                case 3095:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SceneInnateChange_ARG_int32_type_int32_id_int32_level__>(ms);
                    }
                    break;
                case 3096:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SceneBookAttrChange_ARG_Dict_int_int_Data_attrs_Dict_int_int_Data_monsterAttrs__>(ms);
                    }
                    break;
                case 3103:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SceneTeamMessage_ARG_uint64_characterId_int32_type_uint64_teamId_int32_state__>(ms);
                    }
                    break;
                case 3107:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_UseSkillItem_ARG_int32_itemId_int32_count_int32_bagId_int32_bagIndex__>(ms);
                    }
                    break;
                case 3108:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_ServerGMCommand_ARG_string_cmd_string_param__>(ms);
                    }
                    break;
                case 3109:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_FindCharacterName_ARG_string_likeName__>(ms);
                    }
                    break;
                case 3110:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_FindCharacterFriend_ARG_int32_serverId_int32_level__>(ms);
                    }
                    break;
                case 3119:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_NotifyScenePlayerCityData_ARG_uint64_sceneGuid_BuildingList_data__>(ms);
                    }
                    break;
                case 3123:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SSGoToSceneAndPvP_ARG_int32_sceneId_uint64_PvPcharacterId__>(ms);
                    }
                    break;
                case 3129:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SSPvPSceneCampSet_ARG_int32_type__>(ms);
                    }
                    break;
                case 3134:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_NotifyCreateSpeMonster_ARG_Int32Array_ids__>(ms);
                    }
                    break;
                case 3136:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SSGetCharacterSceneData_ARG_uint64_characterId__>(ms);
                    }
                    break;
                case 3139:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_BossDie_ARG_int32_serverId__>(ms);
                    }
                    break;
                case 3140:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_ReadyToEnter_ARG_int32_placeholder__>(ms);
                    }
                    break;
                case 3145:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_GetFriendSceneSimpleData_ARG_int32_type_uint64_haveId_uint64_getId__>(ms);
                    }
                    break;
                case 3146:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SendAddFriend_ARG_int32_type_uint64_haveId_uint64_getId__>(ms);
                    }
                    break;
                case 3147:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SendDeleteFriend_ARG_int32_type_uint64_haveId_uint64_getId__>(ms);
                    }
                    break;
                case 3148:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SendOutLineFriend_ARG_int32_type_uint64_haveId_uint64_getId__>(ms);
                    }
                    break;
                case 3152:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SSExitDungeon_ARG_int32_placeholder__>(ms);
                    }
                    break;
                case 3157:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SSAllianceBuffDataChange_ARG_int32_buffId__>(ms);
                    }
                    break;
                case 3158:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SSAllianceDataChange_ARG_int32_alianceId_int32_type_string_name__>(ms);
                    }
                    break;
                case 3161:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SceneTitleChange_ARG_Int32Array_titles_int32_type__>(ms);
                    }
                    break;
                case 3162:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_NotifyItemCount_ARG_int32_itemId_int32_count__>(ms);
                    }
                    break;
                case 3165:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_MotifyServerAvgLevel_ARG_Dict_int_int_Data_ServerAvgLevel__>(ms);
                    }
                    break;
                case 3169:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_AddAutoActvity_ARG_int32_fubenId_int64_startTime_int64_endTime_int32_count__>(ms);
                    }
                    break;
                case 3170:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SSAddBuff_ARG_uint64_characterId_int32_buffId_int32_buffLevel__>(ms);
                    }
                    break;
                case 3171:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_Silence_ARG_uint32_mask__>(ms);
                    }
                    break;
                case 3172:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SSApplyNpcHP_ARG_int32_serverId_int32_sceneId_uint64_npcGuid__>(ms);
                    }
                    break;
                case 3175:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SSSceneElfChange_ARG_Int32Array_removeBuff_Dict_int_int_Data_addBuff_int32_fightPoint__>(ms);
                    }
                    break;
                case 3499:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SBReconnectNotifyScene_ARG_uint64_oldclientId_uint64_newclientId_uint64_characterId__>(ms);
                    }
                    break;
                case 3500:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SBCleanClientCharacterData_ARG_uint64_clientId_uint64_characterId__>(ms);
                    }
                    break;
                case 3501:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SSNotifyCharacterOnConnet_ARG_uint64_clientId_uint64_characterId__>(ms);
                    }
                    break;
                case 3502:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SBGetServerCharacterCount_ARG_uint32_placeholder__>(ms);
                    }
                    break;
                case 3600:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SyncTowerSkillLevel_ARG_uint64_sceneGuid_int32_towerId_int32_level__>(ms);
                    }
                    break;
                case 3603:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SyncPlayerMieshiContribution_ARG_uint64_sceneGuid_uint64_characterId_int32_Contribution_string_name_float_rate__>(ms);
                    }
                    break;
                case 3605:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_RequestSceneInfo_ARG_int32_serverId_int32_sceneTypeId__>(ms);
                    }
                    break;
                case 3606:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SyncExData_ARG_Dict_int_int_Data_changes__>(ms);
                    }
                    break;
                case 3612:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SyncSceneMount_ARG_int32_MountId__>(ms);
                    }
                    break;
                case 3613:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SSGetFubenStoreItems_ARG_int32_shopType__>(ms);
                    }
                    break;
                case 3614:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SSGetFubenStoreItemCount_ARG_int32_shopType_int32_id__>(ms);
                    }
                    break;
                case 3616:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SSChangeFubenStoreItem_ARG_int32_shopType_int32_id_int32_num__>(ms);
                    }
                    break;
                case 3619:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SSBookFightingMonsterId_ARG_int32_handbookId__>(ms);
                    }
                    break;
                case 3704:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_MissionChangeSceneRequest_ARG_int32_transId__>(ms);
                    }
                    break;
                case 3990:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_GetCharacterData_ARG_uint64_id__>(ms);
                    }
                    break;
                case 3991:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_UpdateServer_ARG_int32_placeholder__>(ms);
                    }
                    break;
                case 3993:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_GMCommand_ARG_StringArray_commonds__>(ms);
                    }
                    break;
                case 3994:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_CloneCharacterDbById_ARG_uint64_fromId_uint64_toId__>(ms);
                    }
                    break;
                case 3996:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_UpdateHoldId_ARG_int32_GuildId__>(ms);
                    }
                    break;
                case 3713:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_NotifyRefreshLodeTimer_ARG_int32_ServerId_Int32Array_ids__>(ms);
                    }
                    break;
                case 3999:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SyncFlagData_ARG_Dict_int_int_Data_changes__>(ms);
                    }
                    break;
                case 4000:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_SceneEquipModelStateChange_ARG_int32_part_int32_state_ItemBaseData_equip__>(ms);
                    }
                    break;
                case 3712:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_NodifyModifyPlayerName_ARG_uint64_characterId_string_modifyName__>(ms);
                    }
                    break;
                case 3711:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_NotifyRefreshBossHome_ARG_Int32Array_ids__>(ms);
                    }
                    break;
                case 3714:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_NotifyBossHomeKill_ARG_int32_placeholder__>(ms);
                    }
                    break;
                case 3715:
                    using (var ms = new MemoryStream(list, false))
                    {
                        return Serializer.Deserialize<__RPC_Scene_CheckCanAcceptChallenge_ARG_int32_placeholder__>(ms);
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
        /// 服务端返回进入场景结果(服务端可主动发，让玩家强制进入某个场景)
        /// </summary>
        public object ReplyChangeScene(ulong __characterId__, ulong __clientId__, PlayerData data)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3074;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_ReplyChangeScene_ARG_PlayerData_data__();
            __data__.Data=data;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知创建角色
        /// </summary>
        public object CreateObj(IEnumerable<ulong> __characterIds__, CreateObjMsg msg)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3078;
            desc.PacketId = 0;
            desc.Routing.AddRange(__characterIds__);
            if(desc.Routing.Count == 0) return null;
            desc.Type = (int)MessageType.SCList;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_CreateObj_ARG_CreateObjMsg_msg__();
            __data__.Msg=msg;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 广播删除角色
        /// 创建OBJ的原因，2种，0不可见 1死亡移除
        /// </summary>
        public object DeleteObj(IEnumerable<ulong> __characterIds__, Uint64Array objs, uint reason)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3079;
            desc.PacketId = 0;
            desc.Routing.AddRange(__characterIds__);
            if(desc.Routing.Count == 0) return null;
            desc.Type = (int)MessageType.SCList;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_DeleteObj_ARG_Uint64Array_objs_uint32_reason__();
            __data__.Objs=objs;
            __data__.Reason=reason;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// </summary>
        public object DeleteObjList(IEnumerable<ulong> __characterIds__, DeleteObjMsgList dels)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3080;
            desc.PacketId = 0;
            desc.Routing.AddRange(__characterIds__);
            if(desc.Routing.Count == 0) return null;
            desc.Type = (int)MessageType.SCList;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_DeleteObjList_ARG_DeleteObjMsgList_dels__();
            __data__.Dels=dels;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 同步移动
        /// </summary>
        public object SyncMoveTo(IEnumerable<ulong> __characterIds__, CharacterMoveMsg msg)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3082;
            desc.PacketId = 0;
            desc.Routing.AddRange(__characterIds__);
            if(desc.Routing.Count == 0) return null;
            desc.Type = (int)MessageType.SCList;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_SyncMoveTo_ARG_CharacterMoveMsg_msg__();
            __data__.Msg=msg;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 同步移动
        /// </summary>
        public object SyncMoveToList(IEnumerable<ulong> __characterIds__, CharacterMoveMsgList msg)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3083;
            desc.PacketId = 0;
            desc.Routing.AddRange(__characterIds__);
            if(desc.Routing.Count == 0) return null;
            desc.Type = (int)MessageType.SCList;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_SyncMoveToList_ARG_CharacterMoveMsgList_msg__();
            __data__.Msg=msg;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 同步停止移动
        /// </summary>
        public object SyncStopMove(IEnumerable<ulong> __characterIds__, SyncPostionMsg msg)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3085;
            desc.PacketId = 0;
            desc.Routing.AddRange(__characterIds__);
            if(desc.Routing.Count == 0) return null;
            desc.Type = (int)MessageType.SCList;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_SyncStopMove_ARG_SyncPostionMsg_msg__();
            __data__.Msg=msg;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 同步主角
        /// </summary>
        public object SyncDirection(IEnumerable<ulong> __characterIds__, ulong characterId, int dirX, int dirZ)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3087;
            desc.PacketId = 0;
            desc.Routing.AddRange(__characterIds__);
            if(desc.Routing.Count == 0) return null;
            desc.Type = (int)MessageType.SCList;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_SyncDirection_ARG_uint64_characterId_int32_dirX_int32_dirZ__();
            __data__.CharacterId=characterId;
            __data__.DirX=dirX;
            __data__.DirZ=dirZ;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 广播使用技能
        /// </summary>
        public object NotifyUseSkill(IEnumerable<ulong> __characterIds__, CharacterUseSkillMsg msg)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3089;
            desc.PacketId = 0;
            desc.Routing.AddRange(__characterIds__);
            if(desc.Routing.Count == 0) return null;
            desc.Type = (int)MessageType.SCList;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_NotifyUseSkill_ARG_CharacterUseSkillMsg_msg__();
            __data__.Msg=msg;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 广播使用一组技能
        /// </summary>
        public object NotifyUseSkillList(IEnumerable<ulong> __characterIds__, CharacterUseSkillMsgList msg)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3090;
            desc.PacketId = 0;
            desc.Routing.AddRange(__characterIds__);
            if(desc.Routing.Count == 0) return null;
            desc.Type = (int)MessageType.SCList;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_NotifyUseSkillList_ARG_CharacterUseSkillMsgList_msg__();
            __data__.Msg=msg;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 广播Buff结果
        /// </summary>
        public object SyncBuff(IEnumerable<ulong> __characterIds__, BuffResultMsg msg)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3091;
            desc.PacketId = 0;
            desc.Routing.AddRange(__characterIds__);
            if(desc.Routing.Count == 0) return null;
            desc.Type = (int)MessageType.SCList;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_SyncBuff_ARG_BuffResultMsg_msg__();
            __data__.Msg=msg;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 广播释放子弹
        /// </summary>
        public object NotifyShootBullet(IEnumerable<ulong> __characterIds__, BulletMsg msg)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3098;
            desc.PacketId = 0;
            desc.Routing.AddRange(__characterIds__);
            if(desc.Routing.Count == 0) return null;
            desc.Type = (int)MessageType.SCList;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_NotifyShootBullet_ARG_BulletMsg_msg__();
            __data__.Msg=msg;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 广播释放子弹
        /// </summary>
        public object NotifyShootBulletList(IEnumerable<ulong> __characterIds__, BulletMsgList msg)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3099;
            desc.PacketId = 0;
            desc.Routing.AddRange(__characterIds__);
            if(desc.Routing.Count == 0) return null;
            desc.Type = (int)MessageType.SCList;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_NotifyShootBulletList_ARG_BulletMsgList_msg__();
            __data__.Msg=msg;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知客户端修改装备模型
        /// </summary>
        public object NotifyEquipChanged(IEnumerable<ulong> __characterIds__, ulong characterId, int part, int ItemId)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3102;
            desc.PacketId = 0;
            desc.Routing.AddRange(__characterIds__);
            if(desc.Routing.Count == 0) return null;
            desc.Type = (int)MessageType.SCList;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_NotifyEquipChanged_ARG_uint64_characterId_int32_part_int32_ItemId__();
            __data__.CharacterId=characterId;
            __data__.Part=part;
            __data__.ItemId=ItemId;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 拾取某个物品
        /// </summary>
        public object PickUpItemSuccess(ulong __characterId__, ulong __clientId__, ulong dropItemId)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3112;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_PickUpItemSuccess_ARG_uint64_dropItemId__();
            __data__.DropItemId=dropItemId;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知客户端场景动画
        /// </summary>
        public object NotifySceneAction(IEnumerable<ulong> __characterIds__, int ActionId)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3113;
            desc.PacketId = 0;
            desc.Routing.AddRange(__characterIds__);
            if(desc.Routing.Count == 0) return null;
            desc.Type = (int)MessageType.SCList;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_NotifySceneAction_ARG_int32_ActionId__();
            __data__.ActionId=ActionId;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 包裹已满的提示
        /// </summary>
        public object BagisFull(ulong __characterId__, ulong __clientId__, ulong dropItemId, int itemId, int itemCount)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3114;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_BagisFull_ARG_uint64_dropItemId_int32_itemId_int32_itemCount__();
            __data__.DropItemId=dropItemId;
            __data__.ItemId=itemId;
            __data__.ItemCount=itemCount;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 同步副本时间
        /// </summary>
        public object NotifyDungeonTime(ulong __characterId__, ulong __clientId__, int state, ulong time)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3118;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_NotifyDungeonTime_ARG_int32_state_uint64_time__();
            __data__.State=state;
            __data__.Time=time;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 同步家园场景数据
        /// </summary>
        public object SyncSceneBuilding(IEnumerable<ulong> __characterIds__, BuildingList data)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3120;
            desc.PacketId = 0;
            desc.Routing.AddRange(__characterIds__);
            if(desc.Routing.Count == 0) return null;
            desc.Type = (int)MessageType.SCList;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_SyncSceneBuilding_ARG_BuildingList_data__();
            __data__.Data=data;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// Debug模式下客户端坐标
        /// </summary>
        public object DebugObjPosition(IEnumerable<ulong> __characterIds__, ulong characterId, PositionData pos)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3122;
            desc.PacketId = 0;
            desc.Routing.AddRange(__characterIds__);
            if(desc.Routing.Count == 0) return null;
            desc.Type = (int)MessageType.SCList;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_DebugObjPosition_ARG_uint64_characterId_PositionData_pos__();
            __data__.CharacterId=characterId;
            __data__.Pos=pos;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 强制客户端改变坐标
        /// </summary>
        public object SyncCharacterPostion(IEnumerable<ulong> __characterIds__, ulong characterId, PositionData pos)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3124;
            desc.PacketId = 0;
            desc.Routing.AddRange(__characterIds__);
            if(desc.Routing.Count == 0) return null;
            desc.Type = (int)MessageType.SCList;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_SyncCharacterPostion_ARG_uint64_characterId_PositionData_pos__();
            __data__.CharacterId=characterId;
            __data__.Pos=pos;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 广播战场提示信息
        /// </summary>
        public object NotifyBattleReminder(ulong __characterId__, ulong __clientId__, int type, string info, int param)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3132;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_NotifyBattleReminder_ARG_int32_type_string_info_int32_param__();
            __data__.Type=type;
            __data__.Info=info;
            __data__.Param=param;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 同步一个倒计时
        /// </summary>
        public object NotifyCountdown(ulong __characterId__, ulong __clientId__, ulong time, int type)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3133;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_NotifyCountdown_ARG_uint64_time_int32_type__();
            __data__.Time=time;
            __data__.Type=type;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知客户端，某只怪物的伤害列表
        /// </summary>
        public object NotifyDamageList(ulong __characterId__, ulong __clientId__, DamageList list)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3137;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_NotifyDamageList_ARG_DamageList_list__();
            __data__.List=list;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知客户端，任务进度
        /// </summary>
        public object NotifyFubenInfo(ulong __characterId__, ulong __clientId__, FubenInfoMsg info)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3138;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_NotifyFubenInfo_ARG_FubenInfoMsg_info__();
            __data__.Info=info;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知一些消息 type:0死亡
        /// </summary>
        public object NotifyMessage(ulong __characterId__, ulong __clientId__, int type, string info, int addChat)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3141;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_NotifyMessage_ARG_int32_type_string_info_int32_addChat__();
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
        /// 通知客户端自己的阵营发生变化
        /// </summary>
        public object NotifyCampChange(ulong __characterId__, ulong __clientId__, int campId, Vector2Int32 pos)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3142;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_NotifyCampChange_ARG_int32_campId_Vector2Int32_pos__();
            __data__.CampId=campId;
            __data__.Pos=pos;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        ///  同步客户端数据
        /// </summary>
        public object SyncDataToClient(IEnumerable<ulong> __characterIds__, SceneSyncData data)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3150;
            desc.PacketId = 0;
            desc.Routing.AddRange(__characterIds__);
            if(desc.Routing.Count == 0) return null;
            desc.Type = (int)MessageType.SCList;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_SyncDataToClient_ARG_SceneSyncData_data__();
            __data__.Data=data;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        ///  同步自己客户端数据
        /// </summary>
        public object SyncMyDataToClient(ulong __characterId__, ulong __clientId__, SceneSyncData data)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3151;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_SyncMyDataToClient_ARG_SceneSyncData_data__();
            __data__.Data=data;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知客户端，据点状态改变了
        /// </summary>
        public object NotifyStrongpointStateChanged(ulong __characterId__, ulong __clientId__, int camp, int index, int state, float time)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3153;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_NotifyStrongpointStateChanged_ARG_int32_camp_int32_index_int32_state_float_time__();
            __data__.Camp=camp;
            __data__.Index=index;
            __data__.State=state;
            __data__.Time=time;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 同步NPC位置
        /// </summary>
        public object SyncObjPosition(IEnumerable<ulong> __characterIds__, SyncPathPosMsg msg)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3156;
            desc.PacketId = 0;
            desc.Routing.AddRange(__characterIds__);
            if(desc.Routing.Count == 0) return null;
            desc.Type = (int)MessageType.SCList;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_SyncObjPosition_ARG_SyncPathPosMsg_msg__();
            __data__.Msg=msg;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 服务器通知客户端，某个id的obj说了一句话,如果字典id不为空，就说字典，如果为空，就说字符串
        /// </summary>
        public object ObjSpeak(IEnumerable<ulong> __characterIds__, ulong id, int dictId, string content)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3160;
            desc.PacketId = 0;
            desc.Routing.AddRange(__characterIds__);
            if(desc.Routing.Count == 0) return null;
            desc.Type = (int)MessageType.SCList;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_ObjSpeak_ARG_uint64_id_int32_dictId_string_content__();
            __data__.Id=id;
            __data__.DictId=dictId;
            __data__.Content=content;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 玩家等级变化同步属性变化值
        /// </summary>
        public object SyncLevelChange(ulong __characterId__, ulong __clientId__, LevelUpAttrData Attr)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3163;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_SyncLevelChange_ARG_LevelUpAttrData_Attr__();
            __data__.Attr=Attr;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 向玩家通知攻城战npc信息
        /// </summary>
        public object NotifyAllianceWarNpcData(IEnumerable<ulong> __characterIds__, int reliveCount, Int32Array data)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3166;
            desc.PacketId = 0;
            desc.Routing.AddRange(__characterIds__);
            if(desc.Routing.Count == 0) return null;
            desc.Type = (int)MessageType.SCList;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_NotifyAllianceWarNpcData_ARG_int32_reliveCount_Int32Array_data__();
            __data__.ReliveCount=reliveCount;
            __data__.Data=data;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 向玩家通知场景内玩家的信息，主要是位置信息
        /// </summary>
        public object NotifyScenePlayerInfos(IEnumerable<ulong> __characterIds__, ScenePlayerInfos info)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3167;
            desc.PacketId = 0;
            desc.Routing.AddRange(__characterIds__);
            if(desc.Routing.Count == 0) return null;
            desc.Type = (int)MessageType.SCList;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_NotifyScenePlayerInfos_ARG_ScenePlayerInfos_info__();
            __data__.Info=info;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 向玩家通知小地图怪物的存活状态
        /// </summary>
        public object NotifyNpcStatus(ulong __characterId__, ulong __clientId__, MapNpcInfos infos)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3168;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_NotifyNpcStatus_ARG_MapNpcInfos_infos__();
            __data__.Infos=infos;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知客户端，积分列表
        /// </summary>
        public object NotifyPointList(ulong __characterId__, ulong __clientId__, PointList list)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3173;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_NotifyPointList_ARG_PointList_list__();
            __data__.List=list;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知客户端开始预警
        /// </summary>
        public object NotifyStartWarning(ulong __characterId__, ulong __clientId__, ulong timeOut)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3174;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_NotifyStartWarning_ARG_uint64_timeOut__();
            __data__.TimeOut=timeOut;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// </summary>
        public object SendMieshiResult(ulong __characterId__, ulong __clientId__, MieshiResultMsg msg)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3602;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_SendMieshiResult_ARG_MieshiResultMsg_msg__();
            __data__.Msg=msg;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知客户端，刷新副本信息
        /// </summary>
        public object NotifyRefreshDungeonInfo(ulong __characterId__, ulong __clientId__, DungeonInfo info)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3604;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_NotifyRefreshDungeonInfo_ARG_DungeonInfo_info__();
            __data__.Info=info;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知客户端，开启xp技能引导
        /// </summary>
        public object NotifyStartXpSkillGuide(ulong __characterId__, ulong __clientId__, int placeholder)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3607;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_NotifyStartXpSkillGuide_ARG_int32_placeholder__();
            __data__.Placeholder=placeholder;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 通知客户端，开启玛雅武器副本引导
        /// </summary>
        public object NotifyStartMaYaFuBenGuide(ulong __characterId__, ulong __clientId__, int type)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3608;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_NotifyStartMaYaFuBenGuide_ARG_int32_type__();
            __data__.Type=type;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 服务端广播爬塔怪物个数
        /// </summary>
        public object BroadcastSceneMonsterCount(ulong __characterId__, ulong __clientId__, int count)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3610;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_BroadcastSceneMonsterCount_ARG_int32_count__();
            __data__.Count=count;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 场景内广播消息，dictId不为-1时，优先用字典内容， 字符串可以用"|"分开进行格式化
        /// </summary>
        public object BroadcastSceneChat(IEnumerable<ulong> __characterIds__, string content, int dictId)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3618;
            desc.PacketId = 0;
            desc.Routing.AddRange(__characterIds__);
            if(desc.Routing.Count == 0) return null;
            desc.Type = (int)MessageType.SCList;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_BroadcastSceneChat_ARG_string_content_int32_dictId__();
            __data__.Content=content;
            __data__.DictId=dictId;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 刷新副本商店购买数据
        /// </summary>
        public object SyncFuBenStore(ulong __characterId__, ulong __clientId__, StoneItems itemlst, int storeType)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3700;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_SyncFuBenStore_ARG_StoneItems_itemlst_int32_storeType__();
            __data__.Itemlst=itemlst;
            __data__.StoreType=storeType;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 队伍：有队员场景typeId变化时，同步队员数据
        /// </summary>
        public object NotifyTeamMemberScene(ulong __characterId__, ulong __clientId__, ulong characterId, ulong changeCharacterId, ulong sceneId)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3701;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_NotifyTeamMemberScene_ARG_uint64_characterId_uint64_changeCharacterId_uint64_sceneId__();
            __data__.CharacterId=characterId;
            __data__.ChangeCharacterId=changeCharacterId;
            __data__.SceneId=sceneId;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 强制停止移动
        /// </summary>
        public object ForceStopMove(ulong __characterId__, ulong __clientId__, PositionData pos)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3702;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_ForceStopMove_ARG_PositionData_pos__();
            __data__.Pos=pos;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 同步倒计时
        /// </summary>
        public object NotifyCommonCountdown(ulong __characterId__, ulong __clientId__, int time)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3703;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_NotifyCommonCountdown_ARG_int32_time__();
            __data__.Time=time;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 吃鸡玩法通知客户端升级
        /// </summary>
        public object CK_NotifyClientLevelup(ulong __characterId__, ulong __clientId__, ulong objId, int lv, int exp, Dict_int_int_Data addBuff)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3705;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_CK_NotifyClientLevelup_ARG_uint64_objId_int32_lv_int32_exp_Dict_int_int_Data_addBuff__();
            __data__.ObjId=objId;
            __data__.Lv=lv;
            __data__.Exp=exp;
            __data__.AddBuff=addBuff;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 发送吃鸡地图信息
        /// </summary>
        public object CK_NotifyCheckenSceneInfo(ulong __characterId__, ulong __clientId__, MsgCheckenSceneInfo info)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3707;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_CK_NotifyCheckenSceneInfo_ARG_MsgCheckenSceneInfo_info__();
            __data__.Info=info;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 发送收到地图伤害
        /// </summary>
        public object CK_NotifyHurt(ulong __characterId__, ulong __clientId__, int placeholder)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3708;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_CK_NotifyHurt_ARG_int32_placeholder__();
            __data__.Placeholder=placeholder;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// </summary>
        public object CK_NotifyRankList(ulong __characterId__, ulong __clientId__, MsgCheckenRankList rank)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3709;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_CK_NotifyRankList_ARG_MsgCheckenRankList_rank__();
            __data__.Rank=rank;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// 同步模型Id
        /// </summary>
        public object SyncModelId(ulong __characterId__, ulong __clientId__, int model)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3992;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_SyncModelId_ARG_int32_model__();
            __data__.Model=model;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// </summary>
        public object NotifyLodeInfo(ulong __characterId__, ulong __clientId__, MsgSceneLode info)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 3998;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_NotifyLodeInfo_ARG_MsgSceneLode_info__();
            __data__.Info=info;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
        /// <summary>
        /// </summary>
        public object NotifyPlayEffect(ulong __characterId__, ulong __clientId__, int effectId)
        {
            var desc = new ServiceDesc();
            desc.FuncId = 4002;
            desc.CharacterId = __characterId__;
            desc.ClientId = __clientId__;
            desc.PacketId = 0;
            desc.Type = (int)MessageType.SC;
            desc.ServiceType = (int) ServiceType.Scene;

            var __data__ = new __RPC_Scene_NotifyPlayEffect_ARG_int32_effectId__();
            __data__.EffectId=effectId;


            var __s__ = MemoryStream;
            Serializer.Serialize(__s__, __data__);
            desc.Data = __s__.ToArray();
            Utility.FunctionCallLogger.Info("Func [{0}] Service [{1}] Type [{2}] called by server.", desc.FuncId, desc.ServiceType, desc.Type);
            mBroker.SendMessage(desc);

            return null;
        }
    }

    public class SBChangeSceneOutMessage : OutMessage
    {
        public SBChangeSceneOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId, int serverId, int sceneId, ulong guid, int changeType, SceneParam sceneParam)
            : base(sender, ServiceType.Scene, 3000, (int)MessageType.SB)
        {
            Request = new __RPC_Scene_SBChangeScene_ARG_uint64_characterId_int32_serverId_int32_sceneId_uint64_guid_int32_changeType_SceneParam_sceneParam__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;
            Request.ServerId=serverId;
            Request.SceneId=sceneId;
            Request.Guid=guid;
            Request.ChangeType=changeType;
            Request.SceneParam=sceneParam;

        }

        public __RPC_Scene_SBChangeScene_ARG_uint64_characterId_int32_serverId_int32_sceneId_uint64_guid_int32_changeType_SceneParam_sceneParam__ Request { get; private set; }

            private __RPC_Scene_SBChangeScene_RET_uint64__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_SBChangeScene_RET_uint64__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SBDestroySceneOutMessage : OutMessage
    {
        public SBDestroySceneOutMessage(ClientAgentBase sender, ulong __characterId__, ulong guid)
            : base(sender, ServiceType.Scene, 3001, (int)MessageType.SB)
        {
            Request = new __RPC_Scene_SBDestroyScene_ARG_uint64_guid__();
            mMessage.CharacterId = __characterId__;
            Request.Guid=guid;

        }

        public __RPC_Scene_SBDestroyScene_ARG_uint64_guid__ Request { get; private set; }

            private __RPC_Scene_SBDestroyScene_RET_uint64__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_SBDestroyScene_RET_uint64__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class NotifySceneNotExistOutMessage : OutMessage
    {
        public NotifySceneNotExistOutMessage(ClientAgentBase sender, ulong __characterId__, ulong sceneId, ulong characterId)
            : base(sender, ServiceType.Scene, 3002, (int)MessageType.SB)
        {
            Request = new __RPC_Scene_NotifySceneNotExist_ARG_uint64_sceneId_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.SceneId=sceneId;
            Request.CharacterId=characterId;

        }

        public __RPC_Scene_NotifySceneNotExist_ARG_uint64_sceneId_uint64_characterId__ Request { get; private set; }


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
            : base(sender, ServiceType.Scene, 3030, (int)MessageType.SB)
        {
            Request = new __RPC_Scene_SBGetAllOnlineCharacterInServer_ARG_int32_serverId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;

        }

        public __RPC_Scene_SBGetAllOnlineCharacterInServer_ARG_int32_serverId__ Request { get; private set; }

            private __RPC_Scene_SBGetAllOnlineCharacterInServer_RET_Uint64Array__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_SBGetAllOnlineCharacterInServer_RET_Uint64Array__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SBCheckCharacterOnlineOutMessage : OutMessage
    {
        public SBCheckCharacterOnlineOutMessage(ClientAgentBase sender, ulong __characterId__, Uint64Array toList)
            : base(sender, ServiceType.Scene, 3039, (int)MessageType.SB)
        {
            Request = new __RPC_Scene_SBCheckCharacterOnline_ARG_Uint64Array_toList__();
            mMessage.CharacterId = __characterId__;
            Request.ToList=toList;

        }

        public __RPC_Scene_SBCheckCharacterOnline_ARG_Uint64Array_toList__ Request { get; private set; }

            private __RPC_Scene_SBCheckCharacterOnline_RET_Int32Array__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_SBCheckCharacterOnline_RET_Int32Array__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class QueryBrokerStatusOutMessage : OutMessage
    {
        public QueryBrokerStatusOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Scene, 3040, (int)MessageType.SB)
        {
            Request = new __RPC_Scene_QueryBrokerStatus_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Scene_QueryBrokerStatus_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Scene_QueryBrokerStatus_RET_CommonBrokerStatus__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_QueryBrokerStatus_RET_CommonBrokerStatus__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SBReloadTableOutMessage : OutMessage
    {
        public SBReloadTableOutMessage(ClientAgentBase sender, ulong __characterId__, string tableName)
            : base(sender, ServiceType.Scene, 3050, (int)MessageType.SB)
        {
            Request = new __RPC_Scene_SBReloadTable_ARG_string_tableName__();
            mMessage.CharacterId = __characterId__;
            Request.TableName=tableName;

        }

        public __RPC_Scene_SBReloadTable_ARG_string_tableName__ Request { get; private set; }


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

    public class PrepareDataForEnterGameOutMessage : OutMessage
    {
        public PrepareDataForEnterGameOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId)
            : base(sender, ServiceType.Scene, 3051, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_PrepareDataForEnterGame_ARG_int32_serverId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;

        }

        public __RPC_Scene_PrepareDataForEnterGame_ARG_int32_serverId__ Request { get; private set; }

            private __RPC_Scene_PrepareDataForEnterGame_RET_uint64__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_PrepareDataForEnterGame_RET_uint64__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class PrepareDataForCreateCharacterOutMessage : OutMessage
    {
        public PrepareDataForCreateCharacterOutMessage(ClientAgentBase sender, ulong __characterId__, int type)
            : base(sender, ServiceType.Scene, 3052, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_PrepareDataForCreateCharacter_ARG_int32_type__();
            mMessage.CharacterId = __characterId__;
            Request.Type=type;

        }

        public __RPC_Scene_PrepareDataForCreateCharacter_ARG_int32_type__ Request { get; private set; }

            private __RPC_Scene_PrepareDataForCreateCharacter_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_PrepareDataForCreateCharacter_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class PrepareDataForCommonUseOutMessage : OutMessage
    {
        public PrepareDataForCommonUseOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Scene, 3053, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_PrepareDataForCommonUse_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Scene_PrepareDataForCommonUse_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Scene_PrepareDataForCommonUse_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_PrepareDataForCommonUse_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class PrepareDataForLogoutOutMessage : OutMessage
    {
        public PrepareDataForLogoutOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Scene, 3054, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_PrepareDataForLogout_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Scene_PrepareDataForLogout_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Scene_PrepareDataForLogout_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_PrepareDataForLogout_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class CreateCharacterOutMessage : OutMessage
    {
        public CreateCharacterOutMessage(ClientAgentBase sender, ulong __characterId__, int type, bool isGM)
            : base(sender, ServiceType.Scene, 3015, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_CreateCharacter_ARG_int32_type_bool_isGM__();
            mMessage.CharacterId = __characterId__;
            Request.Type=type;
            Request.IsGM=isGM;

        }

        public __RPC_Scene_CreateCharacter_ARG_int32_type_bool_isGM__ Request { get; private set; }

            private __RPC_Scene_CreateCharacter_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_CreateCharacter_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class DelectCharacterOutMessage : OutMessage
    {
        public DelectCharacterOutMessage(ClientAgentBase sender, ulong __characterId__, int type)
            : base(sender, ServiceType.Scene, 3016, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_DelectCharacter_ARG_int32_type__();
            mMessage.CharacterId = __characterId__;
            Request.Type=type;

        }

        public __RPC_Scene_DelectCharacter_ARG_int32_type__ Request { get; private set; }

            private __RPC_Scene_DelectCharacter_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_DelectCharacter_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSEnterSceneOutMessage : OutMessage
    {
        public SSEnterSceneOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId, ulong guid, ulong applyGuid, int changeType, SceneParam sceneParam)
            : base(sender, ServiceType.Scene, 3055, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SSEnterScene_ARG_uint64_characterId_uint64_guid_uint64_applyGuid_int32_changeType_SceneParam_sceneParam__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;
            Request.Guid=guid;
            Request.ApplyGuid=applyGuid;
            Request.ChangeType=changeType;
            Request.SceneParam=sceneParam;

        }

        public __RPC_Scene_SSEnterScene_ARG_uint64_characterId_uint64_guid_uint64_applyGuid_int32_changeType_SceneParam_sceneParam__ Request { get; private set; }


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

    public class GetSceneSimpleDataOutMessage : OutMessage
    {
        public GetSceneSimpleDataOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Scene, 3056, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_GetSceneSimpleData_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Scene_GetSceneSimpleData_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Scene_GetSceneSimpleData_RET_SceneSimpleData__ mResponse;
            public SceneSimpleData Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Scene_GetSceneSimpleData_RET_SceneSimpleData__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class CheckConnectedOutMessage : OutMessage
    {
        public CheckConnectedOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId)
            : base(sender, ServiceType.Scene, 3057, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_CheckConnected_ARG_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;

        }

        public __RPC_Scene_CheckConnected_ARG_uint64_characterId__ Request { get; private set; }

            private __RPC_Scene_CheckConnected_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_CheckConnected_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class CheckLostOutMessage : OutMessage
    {
        public CheckLostOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId)
            : base(sender, ServiceType.Scene, 3058, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_CheckLost_ARG_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;

        }

        public __RPC_Scene_CheckLost_ARG_uint64_characterId__ Request { get; private set; }

            private __RPC_Scene_CheckLost_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_CheckLost_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class QueryStatusOutMessage : OutMessage
    {
        public QueryStatusOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Scene, 3059, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_QueryStatus_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Scene_QueryStatus_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Scene_QueryStatus_RET_SceneServerStatus__ mResponse;
            public SceneServerStatus Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Scene_QueryStatus_RET_SceneServerStatus__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class NotifyConnectedOutMessage : OutMessage
    {
        public NotifyConnectedOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Scene, 3061, (int)MessageType.SB)
        {
            Request = new __RPC_Scene_NotifyConnected_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Scene_NotifyConnected_ARG_uint32_placeholder__ Request { get; private set; }


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

    public class NotifyLostOutMessage : OutMessage
    {
        public NotifyLostOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Scene, 3062, (int)MessageType.SB)
        {
            Request = new __RPC_Scene_NotifyLost_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Scene_NotifyLost_ARG_uint32_placeholder__ Request { get; private set; }


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

    public class NotifySceneFinishedOutMessage : OutMessage
    {
        public NotifySceneFinishedOutMessage(ClientAgentBase sender, ulong __characterId__, ulong guid)
            : base(sender, ServiceType.Scene, 3063, (int)MessageType.SB)
        {
            Request = new __RPC_Scene_NotifySceneFinished_ARG_uint64_guid__();
            mMessage.CharacterId = __characterId__;
            Request.Guid=guid;

        }

        public __RPC_Scene_NotifySceneFinished_ARG_uint64_guid__ Request { get; private set; }


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

    public class AskEnterDungeonOutMessage : OutMessage
    {
        public AskEnterDungeonOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int sceneId, ulong guid, SceneParam param)
            : base(sender, ServiceType.Scene, 3064, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_AskEnterDungeon_ARG_int32_serverId_int32_sceneId_uint64_guid_SceneParam_param__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.SceneId=sceneId;
            Request.Guid=guid;
            Request.Param=param;

        }

        public __RPC_Scene_AskEnterDungeon_ARG_int32_serverId_int32_sceneId_uint64_guid_SceneParam_param__ Request { get; private set; }


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

    public class NotifyPlayerPickUpFubenRewardOutMessage : OutMessage
    {
        public NotifyPlayerPickUpFubenRewardOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId)
            : base(sender, ServiceType.Scene, 3066, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_NotifyPlayerPickUpFubenReward_ARG_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;

        }

        public __RPC_Scene_NotifyPlayerPickUpFubenReward_ARG_uint64_characterId__ Request { get; private set; }


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

    public class SBChangeSceneByTeamOutMessage : OutMessage
    {
        public SBChangeSceneByTeamOutMessage(ClientAgentBase sender, ulong __characterId__, ChangeSceneInfo changeSceneData)
            : base(sender, ServiceType.Scene, 3069, (int)MessageType.SB)
        {
            Request = new __RPC_Scene_SBChangeSceneByTeam_ARG_ChangeSceneInfo_changeSceneData__();
            mMessage.CharacterId = __characterId__;
            Request.ChangeSceneData=changeSceneData;

        }

        public __RPC_Scene_SBChangeSceneByTeam_ARG_ChangeSceneInfo_changeSceneData__ Request { get; private set; }

            private __RPC_Scene_SBChangeSceneByTeam_RET_uint64__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_SBChangeSceneByTeam_RET_uint64__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class MergeSceneByTeamOutMessage : OutMessage
    {
        public MergeSceneByTeamOutMessage(ClientAgentBase sender, ulong __characterId__, IdList ids)
            : base(sender, ServiceType.Scene, 3071, (int)MessageType.SB)
        {
            Request = new __RPC_Scene_MergeSceneByTeam_ARG_IdList_ids__();
            mMessage.CharacterId = __characterId__;
            Request.Ids=ids;

        }

        public __RPC_Scene_MergeSceneByTeam_ARG_IdList_ids__ Request { get; private set; }


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

    public class IsSceneExistOutMessage : OutMessage
    {
        public IsSceneExistOutMessage(ClientAgentBase sender, ulong __characterId__, ulong sceneGuid)
            : base(sender, ServiceType.Scene, 3072, (int)MessageType.SB)
        {
            Request = new __RPC_Scene_IsSceneExist_ARG_uint64_sceneGuid__();
            mMessage.CharacterId = __characterId__;
            Request.SceneGuid=sceneGuid;

        }

        public __RPC_Scene_IsSceneExist_ARG_uint64_sceneGuid__ Request { get; private set; }

            private __RPC_Scene_IsSceneExist_RET_bool__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_IsSceneExist_RET_bool__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class LoginEnterSceneOutMessage : OutMessage
    {
        public LoginEnterSceneOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, long logout)
            : base(sender, ServiceType.Scene, 3073, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_LoginEnterScene_ARG_int32_serverId_int64_logout__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.Logout=logout;

        }

        public __RPC_Scene_LoginEnterScene_ARG_int32_serverId_int64_logout__ Request { get; private set; }


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

    public class SceneEquipChangeOutMessage : OutMessage
    {
        public SceneEquipChangeOutMessage(ClientAgentBase sender, ulong __characterId__, int type, int part, ItemBaseData equip)
            : base(sender, ServiceType.Scene, 3092, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SceneEquipChange_ARG_int32_type_int32_part_ItemBaseData_equip__();
            mMessage.CharacterId = __characterId__;
            Request.Type=type;
            Request.Part=part;
            Request.Equip=equip;

        }

        public __RPC_Scene_SceneEquipChange_ARG_int32_type_int32_part_ItemBaseData_equip__ Request { get; private set; }

            private __RPC_Scene_SceneEquipChange_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_SceneEquipChange_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SceneSkillChangeOutMessage : OutMessage
    {
        public SceneSkillChangeOutMessage(ClientAgentBase sender, ulong __characterId__, int type, int id, int level)
            : base(sender, ServiceType.Scene, 3093, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SceneSkillChange_ARG_int32_type_int32_id_int32_level__();
            mMessage.CharacterId = __characterId__;
            Request.Type=type;
            Request.Id=id;
            Request.Level=level;

        }

        public __RPC_Scene_SceneSkillChange_ARG_int32_type_int32_id_int32_level__ Request { get; private set; }

            private __RPC_Scene_SceneSkillChange_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_SceneSkillChange_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SceneEquipSkillOutMessage : OutMessage
    {
        public SceneEquipSkillOutMessage(ClientAgentBase sender, ulong __characterId__, Int32Array delSkills, Int32Array SkillIds, Int32Array SkillLevels)
            : base(sender, ServiceType.Scene, 3094, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SceneEquipSkill_ARG_Int32Array_delSkills_Int32Array_SkillIds_Int32Array_SkillLevels__();
            mMessage.CharacterId = __characterId__;
            Request.DelSkills=delSkills;
            Request.SkillIds=SkillIds;
            Request.SkillLevels=SkillLevels;

        }

        public __RPC_Scene_SceneEquipSkill_ARG_Int32Array_delSkills_Int32Array_SkillIds_Int32Array_SkillLevels__ Request { get; private set; }

            private __RPC_Scene_SceneEquipSkill_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_SceneEquipSkill_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SceneInnateChangeOutMessage : OutMessage
    {
        public SceneInnateChangeOutMessage(ClientAgentBase sender, ulong __characterId__, int type, int id, int level)
            : base(sender, ServiceType.Scene, 3095, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SceneInnateChange_ARG_int32_type_int32_id_int32_level__();
            mMessage.CharacterId = __characterId__;
            Request.Type=type;
            Request.Id=id;
            Request.Level=level;

        }

        public __RPC_Scene_SceneInnateChange_ARG_int32_type_int32_id_int32_level__ Request { get; private set; }

            private __RPC_Scene_SceneInnateChange_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_SceneInnateChange_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SceneBookAttrChangeOutMessage : OutMessage
    {
        public SceneBookAttrChangeOutMessage(ClientAgentBase sender, ulong __characterId__, Dict_int_int_Data attrs, Dict_int_int_Data monsterAttrs)
            : base(sender, ServiceType.Scene, 3096, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SceneBookAttrChange_ARG_Dict_int_int_Data_attrs_Dict_int_int_Data_monsterAttrs__();
            mMessage.CharacterId = __characterId__;
            Request.Attrs=attrs;
            Request.MonsterAttrs=monsterAttrs;

        }

        public __RPC_Scene_SceneBookAttrChange_ARG_Dict_int_int_Data_attrs_Dict_int_int_Data_monsterAttrs__ Request { get; private set; }

            private __RPC_Scene_SceneBookAttrChange_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_SceneBookAttrChange_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SceneTeamMessageOutMessage : OutMessage
    {
        public SceneTeamMessageOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId, int type, ulong teamId, int state)
            : base(sender, ServiceType.Scene, 3103, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SceneTeamMessage_ARG_uint64_characterId_int32_type_uint64_teamId_int32_state__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;
            Request.Type=type;
            Request.TeamId=teamId;
            Request.State=state;

        }

        public __RPC_Scene_SceneTeamMessage_ARG_uint64_characterId_int32_type_uint64_teamId_int32_state__ Request { get; private set; }

            private __RPC_Scene_SceneTeamMessage_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_SceneTeamMessage_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class UseSkillItemOutMessage : OutMessage
    {
        public UseSkillItemOutMessage(ClientAgentBase sender, ulong __characterId__, int itemId, int count, int bagId, int bagIndex)
            : base(sender, ServiceType.Scene, 3107, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_UseSkillItem_ARG_int32_itemId_int32_count_int32_bagId_int32_bagIndex__();
            mMessage.CharacterId = __characterId__;
            Request.ItemId=itemId;
            Request.Count=count;
            Request.BagId=bagId;
            Request.BagIndex=bagIndex;

        }

        public __RPC_Scene_UseSkillItem_ARG_int32_itemId_int32_count_int32_bagId_int32_bagIndex__ Request { get; private set; }

            private __RPC_Scene_UseSkillItem_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_UseSkillItem_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class ServerGMCommandOutMessage : OutMessage
    {
        public ServerGMCommandOutMessage(ClientAgentBase sender, ulong __characterId__, string cmd, string param)
            : base(sender, ServiceType.Scene, 3108, (int)MessageType.SAS)
        {
            Request = new __RPC_Scene_ServerGMCommand_ARG_string_cmd_string_param__();
            mMessage.CharacterId = __characterId__;
            Request.Cmd=cmd;
            Request.Param=param;

        }

        public __RPC_Scene_ServerGMCommand_ARG_string_cmd_string_param__ Request { get; private set; }


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

    public class FindCharacterNameOutMessage : OutMessage
    {
        public FindCharacterNameOutMessage(ClientAgentBase sender, ulong __characterId__, string likeName)
            : base(sender, ServiceType.Scene, 3109, (int)MessageType.SAS)
        {
            Request = new __RPC_Scene_FindCharacterName_ARG_string_likeName__();
            mMessage.CharacterId = __characterId__;
            Request.LikeName=likeName;

        }

        public __RPC_Scene_FindCharacterName_ARG_string_likeName__ Request { get; private set; }

            public List<CharacterSimpleDatas> Response { get; private set; }

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
                if(Response == null) Response = new List<CharacterSimpleDatas>();
                Response.Add(Serializer.Deserialize<__RPC_Scene_FindCharacterName_RET_CharacterSimpleDatas__>(ms).ReturnValue);
            }
            State = MessageState.Reply;
            if(ErrorCode == 0) ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class FindCharacterFriendOutMessage : OutMessage
    {
        public FindCharacterFriendOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int level)
            : base(sender, ServiceType.Scene, 3110, (int)MessageType.SAS)
        {
            Request = new __RPC_Scene_FindCharacterFriend_ARG_int32_serverId_int32_level__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.Level=level;

        }

        public __RPC_Scene_FindCharacterFriend_ARG_int32_serverId_int32_level__ Request { get; private set; }

            public List<CharacterSimpleDatas> Response { get; private set; }

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
                if(Response == null) Response = new List<CharacterSimpleDatas>();
                Response.Add(Serializer.Deserialize<__RPC_Scene_FindCharacterFriend_RET_CharacterSimpleDatas__>(ms).ReturnValue);
            }
            State = MessageState.Reply;
            if(ErrorCode == 0) ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class NotifyScenePlayerCityDataOutMessage : OutMessage
    {
        public NotifyScenePlayerCityDataOutMessage(ClientAgentBase sender, ulong __characterId__, ulong sceneGuid, BuildingList data)
            : base(sender, ServiceType.Scene, 3119, (int)MessageType.SAS)
        {
            Request = new __RPC_Scene_NotifyScenePlayerCityData_ARG_uint64_sceneGuid_BuildingList_data__();
            mMessage.CharacterId = __characterId__;
            Request.SceneGuid=sceneGuid;
            Request.Data=data;

        }

        public __RPC_Scene_NotifyScenePlayerCityData_ARG_uint64_sceneGuid_BuildingList_data__ Request { get; private set; }


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

    public class SSGoToSceneAndPvPOutMessage : OutMessage
    {
        public SSGoToSceneAndPvPOutMessage(ClientAgentBase sender, ulong __characterId__, int sceneId, ulong PvPcharacterId)
            : base(sender, ServiceType.Scene, 3123, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SSGoToSceneAndPvP_ARG_int32_sceneId_uint64_PvPcharacterId__();
            mMessage.CharacterId = __characterId__;
            Request.SceneId=sceneId;
            Request.PvPcharacterId=PvPcharacterId;

        }

        public __RPC_Scene_SSGoToSceneAndPvP_ARG_int32_sceneId_uint64_PvPcharacterId__ Request { get; private set; }

            private __RPC_Scene_SSGoToSceneAndPvP_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_SSGoToSceneAndPvP_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSPvPSceneCampSetOutMessage : OutMessage
    {
        public SSPvPSceneCampSetOutMessage(ClientAgentBase sender, ulong __characterId__, int type)
            : base(sender, ServiceType.Scene, 3129, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SSPvPSceneCampSet_ARG_int32_type__();
            mMessage.CharacterId = __characterId__;
            Request.Type=type;

        }

        public __RPC_Scene_SSPvPSceneCampSet_ARG_int32_type__ Request { get; private set; }


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

    public class NotifyCreateSpeMonsterOutMessage : OutMessage
    {
        public NotifyCreateSpeMonsterOutMessage(ClientAgentBase sender, ulong __characterId__, Int32Array ids)
            : base(sender, ServiceType.Scene, 3134, (int)MessageType.SAS)
        {
            Request = new __RPC_Scene_NotifyCreateSpeMonster_ARG_Int32Array_ids__();
            mMessage.CharacterId = __characterId__;
            Request.Ids=ids;

        }

        public __RPC_Scene_NotifyCreateSpeMonster_ARG_Int32Array_ids__ Request { get; private set; }


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

    public class SSGetCharacterSceneDataOutMessage : OutMessage
    {
        public SSGetCharacterSceneDataOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId)
            : base(sender, ServiceType.Scene, 3136, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SSGetCharacterSceneData_ARG_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;

        }

        public __RPC_Scene_SSGetCharacterSceneData_ARG_uint64_characterId__ Request { get; private set; }

            private __RPC_Scene_SSGetCharacterSceneData_RET_ObjSceneData__ mResponse;
            public ObjSceneData Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Scene_SSGetCharacterSceneData_RET_ObjSceneData__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class BossDieOutMessage : OutMessage
    {
        public BossDieOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId)
            : base(sender, ServiceType.Scene, 3139, (int)MessageType.SAS)
        {
            Request = new __RPC_Scene_BossDie_ARG_int32_serverId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;

        }

        public __RPC_Scene_BossDie_ARG_int32_serverId__ Request { get; private set; }


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
            : base(sender, ServiceType.Scene, 3140, (int)MessageType.SAS)
        {
            Request = new __RPC_Scene_ReadyToEnter_ARG_int32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Scene_ReadyToEnter_ARG_int32_placeholder__ Request { get; private set; }

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
                Response.Add(Serializer.Deserialize<__RPC_Scene_ReadyToEnter_RET_int32__>(ms).ReturnValue);
            }
            State = MessageState.Reply;
            if(ErrorCode == 0) ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetFriendSceneSimpleDataOutMessage : OutMessage
    {
        public GetFriendSceneSimpleDataOutMessage(ClientAgentBase sender, ulong __characterId__, int type, ulong haveId, ulong getId)
            : base(sender, ServiceType.Scene, 3145, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_GetFriendSceneSimpleData_ARG_int32_type_uint64_haveId_uint64_getId__();
            mMessage.CharacterId = __characterId__;
            Request.Type=type;
            Request.HaveId=haveId;
            Request.GetId=getId;

        }

        public __RPC_Scene_GetFriendSceneSimpleData_ARG_int32_type_uint64_haveId_uint64_getId__ Request { get; private set; }

            private __RPC_Scene_GetFriendSceneSimpleData_RET_SceneSimpleData__ mResponse;
            public SceneSimpleData Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Scene_GetFriendSceneSimpleData_RET_SceneSimpleData__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SendAddFriendOutMessage : OutMessage
    {
        public SendAddFriendOutMessage(ClientAgentBase sender, ulong __characterId__, int type, ulong haveId, ulong getId)
            : base(sender, ServiceType.Scene, 3146, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SendAddFriend_ARG_int32_type_uint64_haveId_uint64_getId__();
            mMessage.CharacterId = __characterId__;
            Request.Type=type;
            Request.HaveId=haveId;
            Request.GetId=getId;

        }

        public __RPC_Scene_SendAddFriend_ARG_int32_type_uint64_haveId_uint64_getId__ Request { get; private set; }


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

    public class SendDeleteFriendOutMessage : OutMessage
    {
        public SendDeleteFriendOutMessage(ClientAgentBase sender, ulong __characterId__, int type, ulong haveId, ulong getId)
            : base(sender, ServiceType.Scene, 3147, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SendDeleteFriend_ARG_int32_type_uint64_haveId_uint64_getId__();
            mMessage.CharacterId = __characterId__;
            Request.Type=type;
            Request.HaveId=haveId;
            Request.GetId=getId;

        }

        public __RPC_Scene_SendDeleteFriend_ARG_int32_type_uint64_haveId_uint64_getId__ Request { get; private set; }


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

    public class SendOutLineFriendOutMessage : OutMessage
    {
        public SendOutLineFriendOutMessage(ClientAgentBase sender, ulong __characterId__, int type, ulong haveId, ulong getId)
            : base(sender, ServiceType.Scene, 3148, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SendOutLineFriend_ARG_int32_type_uint64_haveId_uint64_getId__();
            mMessage.CharacterId = __characterId__;
            Request.Type=type;
            Request.HaveId=haveId;
            Request.GetId=getId;

        }

        public __RPC_Scene_SendOutLineFriend_ARG_int32_type_uint64_haveId_uint64_getId__ Request { get; private set; }


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

    public class SSExitDungeonOutMessage : OutMessage
    {
        public SSExitDungeonOutMessage(ClientAgentBase sender, ulong __characterId__, int placeholder)
            : base(sender, ServiceType.Scene, 3152, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SSExitDungeon_ARG_int32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Scene_SSExitDungeon_ARG_int32_placeholder__ Request { get; private set; }

            private __RPC_Scene_SSExitDungeon_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_SSExitDungeon_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSAllianceBuffDataChangeOutMessage : OutMessage
    {
        public SSAllianceBuffDataChangeOutMessage(ClientAgentBase sender, ulong __characterId__, int buffId)
            : base(sender, ServiceType.Scene, 3157, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SSAllianceBuffDataChange_ARG_int32_buffId__();
            mMessage.CharacterId = __characterId__;
            Request.BuffId=buffId;

        }

        public __RPC_Scene_SSAllianceBuffDataChange_ARG_int32_buffId__ Request { get; private set; }


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

    public class SSAllianceDataChangeOutMessage : OutMessage
    {
        public SSAllianceDataChangeOutMessage(ClientAgentBase sender, ulong __characterId__, int alianceId, int type, string name)
            : base(sender, ServiceType.Scene, 3158, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SSAllianceDataChange_ARG_int32_alianceId_int32_type_string_name__();
            mMessage.CharacterId = __characterId__;
            Request.AlianceId=alianceId;
            Request.Type=type;
            Request.Name=name;

        }

        public __RPC_Scene_SSAllianceDataChange_ARG_int32_alianceId_int32_type_string_name__ Request { get; private set; }


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

    public class SceneTitleChangeOutMessage : OutMessage
    {
        public SceneTitleChangeOutMessage(ClientAgentBase sender, ulong __characterId__, Int32Array titles, int type)
            : base(sender, ServiceType.Scene, 3161, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SceneTitleChange_ARG_Int32Array_titles_int32_type__();
            mMessage.CharacterId = __characterId__;
            Request.Titles=titles;
            Request.Type=type;

        }

        public __RPC_Scene_SceneTitleChange_ARG_Int32Array_titles_int32_type__ Request { get; private set; }


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

    public class NotifyItemCountOutMessage : OutMessage
    {
        public NotifyItemCountOutMessage(ClientAgentBase sender, ulong __characterId__, int itemId, int count)
            : base(sender, ServiceType.Scene, 3162, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_NotifyItemCount_ARG_int32_itemId_int32_count__();
            mMessage.CharacterId = __characterId__;
            Request.ItemId=itemId;
            Request.Count=count;

        }

        public __RPC_Scene_NotifyItemCount_ARG_int32_itemId_int32_count__ Request { get; private set; }


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

    public class MotifyServerAvgLevelOutMessage : OutMessage
    {
        public MotifyServerAvgLevelOutMessage(ClientAgentBase sender, ulong __characterId__, Dict_int_int_Data ServerAvgLevel)
            : base(sender, ServiceType.Scene, 3165, (int)MessageType.SAS)
        {
            Request = new __RPC_Scene_MotifyServerAvgLevel_ARG_Dict_int_int_Data_ServerAvgLevel__();
            mMessage.CharacterId = __characterId__;
            Request.ServerAvgLevel=ServerAvgLevel;

        }

        public __RPC_Scene_MotifyServerAvgLevel_ARG_Dict_int_int_Data_ServerAvgLevel__ Request { get; private set; }


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

    public class AddAutoActvityOutMessage : OutMessage
    {
        public AddAutoActvityOutMessage(ClientAgentBase sender, ulong __characterId__, int fubenId, long startTime, long endTime, int count)
            : base(sender, ServiceType.Scene, 3169, (int)MessageType.SAS)
        {
            Request = new __RPC_Scene_AddAutoActvity_ARG_int32_fubenId_int64_startTime_int64_endTime_int32_count__();
            mMessage.CharacterId = __characterId__;
            Request.FubenId=fubenId;
            Request.StartTime=startTime;
            Request.EndTime=endTime;
            Request.Count=count;

        }

        public __RPC_Scene_AddAutoActvity_ARG_int32_fubenId_int64_startTime_int64_endTime_int32_count__ Request { get; private set; }


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

    public class SSAddBuffOutMessage : OutMessage
    {
        public SSAddBuffOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId, int buffId, int buffLevel)
            : base(sender, ServiceType.Scene, 3170, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SSAddBuff_ARG_uint64_characterId_int32_buffId_int32_buffLevel__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;
            Request.BuffId=buffId;
            Request.BuffLevel=buffLevel;

        }

        public __RPC_Scene_SSAddBuff_ARG_uint64_characterId_int32_buffId_int32_buffLevel__ Request { get; private set; }


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

    public class SilenceOutMessage : OutMessage
    {
        public SilenceOutMessage(ClientAgentBase sender, ulong __characterId__, uint mask)
            : base(sender, ServiceType.Scene, 3171, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_Silence_ARG_uint32_mask__();
            mMessage.CharacterId = __characterId__;
            Request.Mask=mask;

        }

        public __RPC_Scene_Silence_ARG_uint32_mask__ Request { get; private set; }

            private __RPC_Scene_Silence_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_Silence_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSApplyNpcHPOutMessage : OutMessage
    {
        public SSApplyNpcHPOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int sceneId, ulong npcGuid)
            : base(sender, ServiceType.Scene, 3172, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SSApplyNpcHP_ARG_int32_serverId_int32_sceneId_uint64_npcGuid__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.SceneId=sceneId;
            Request.NpcGuid=npcGuid;

        }

        public __RPC_Scene_SSApplyNpcHP_ARG_int32_serverId_int32_sceneId_uint64_npcGuid__ Request { get; private set; }

            private __RPC_Scene_SSApplyNpcHP_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_SSApplyNpcHP_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSSceneElfChangeOutMessage : OutMessage
    {
        public SSSceneElfChangeOutMessage(ClientAgentBase sender, ulong __characterId__, Int32Array removeBuff, Dict_int_int_Data addBuff, int fightPoint)
            : base(sender, ServiceType.Scene, 3175, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SSSceneElfChange_ARG_Int32Array_removeBuff_Dict_int_int_Data_addBuff_int32_fightPoint__();
            mMessage.CharacterId = __characterId__;
            Request.RemoveBuff=removeBuff;
            Request.AddBuff=addBuff;
            Request.FightPoint=fightPoint;

        }

        public __RPC_Scene_SSSceneElfChange_ARG_Int32Array_removeBuff_Dict_int_int_Data_addBuff_int32_fightPoint__ Request { get; private set; }

            private __RPC_Scene_SSSceneElfChange_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_SSSceneElfChange_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SBReconnectNotifySceneOutMessage : OutMessage
    {
        public SBReconnectNotifySceneOutMessage(ClientAgentBase sender, ulong __characterId__, ulong oldclientId, ulong newclientId, ulong characterId)
            : base(sender, ServiceType.Scene, 3499, (int)MessageType.SB)
        {
            Request = new __RPC_Scene_SBReconnectNotifyScene_ARG_uint64_oldclientId_uint64_newclientId_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.OldclientId=oldclientId;
            Request.NewclientId=newclientId;
            Request.CharacterId=characterId;

        }

        public __RPC_Scene_SBReconnectNotifyScene_ARG_uint64_oldclientId_uint64_newclientId_uint64_characterId__ Request { get; private set; }


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
            : base(sender, ServiceType.Scene, 3500, (int)MessageType.SB)
        {
            Request = new __RPC_Scene_SBCleanClientCharacterData_ARG_uint64_clientId_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.ClientId=clientId;
            Request.CharacterId=characterId;

        }

        public __RPC_Scene_SBCleanClientCharacterData_ARG_uint64_clientId_uint64_characterId__ Request { get; private set; }


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
            : base(sender, ServiceType.Scene, 3501, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SSNotifyCharacterOnConnet_ARG_uint64_clientId_uint64_characterId__();
            mMessage.CharacterId = __characterId__;
            Request.ClientId=clientId;
            Request.CharacterId=characterId;

        }

        public __RPC_Scene_SSNotifyCharacterOnConnet_ARG_uint64_clientId_uint64_characterId__ Request { get; private set; }

            private __RPC_Scene_SSNotifyCharacterOnConnet_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_SSNotifyCharacterOnConnet_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SBGetServerCharacterCountOutMessage : OutMessage
    {
        public SBGetServerCharacterCountOutMessage(ClientAgentBase sender, ulong __characterId__, uint placeholder)
            : base(sender, ServiceType.Scene, 3502, (int)MessageType.SB)
        {
            Request = new __RPC_Scene_SBGetServerCharacterCount_ARG_uint32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Scene_SBGetServerCharacterCount_ARG_uint32_placeholder__ Request { get; private set; }

            private __RPC_Scene_SBGetServerCharacterCount_RET_Dict_int_int_Data__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_SBGetServerCharacterCount_RET_Dict_int_int_Data__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SyncTowerSkillLevelOutMessage : OutMessage
    {
        public SyncTowerSkillLevelOutMessage(ClientAgentBase sender, ulong __characterId__, ulong sceneGuid, int towerId, int level)
            : base(sender, ServiceType.Scene, 3600, (int)MessageType.SAS)
        {
            Request = new __RPC_Scene_SyncTowerSkillLevel_ARG_uint64_sceneGuid_int32_towerId_int32_level__();
            mMessage.CharacterId = __characterId__;
            Request.SceneGuid=sceneGuid;
            Request.TowerId=towerId;
            Request.Level=level;

        }

        public __RPC_Scene_SyncTowerSkillLevel_ARG_uint64_sceneGuid_int32_towerId_int32_level__ Request { get; private set; }


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

    public class SyncPlayerMieshiContributionOutMessage : OutMessage
    {
        public SyncPlayerMieshiContributionOutMessage(ClientAgentBase sender, ulong __characterId__, ulong sceneGuid, ulong characterId, int Contribution, string name, float rate)
            : base(sender, ServiceType.Scene, 3603, (int)MessageType.SAS)
        {
            Request = new __RPC_Scene_SyncPlayerMieshiContribution_ARG_uint64_sceneGuid_uint64_characterId_int32_Contribution_string_name_float_rate__();
            mMessage.CharacterId = __characterId__;
            Request.SceneGuid=sceneGuid;
            Request.CharacterId=characterId;
            Request.Contribution=Contribution;
            Request.Name=name;
            Request.Rate=rate;

        }

        public __RPC_Scene_SyncPlayerMieshiContribution_ARG_uint64_sceneGuid_uint64_characterId_int32_Contribution_string_name_float_rate__ Request { get; private set; }


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

    public class RequestSceneInfoOutMessage : OutMessage
    {
        public RequestSceneInfoOutMessage(ClientAgentBase sender, ulong __characterId__, int serverId, int sceneTypeId)
            : base(sender, ServiceType.Scene, 3605, (int)MessageType.SB)
        {
            Request = new __RPC_Scene_RequestSceneInfo_ARG_int32_serverId_int32_sceneTypeId__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=serverId;
            Request.SceneTypeId=sceneTypeId;

        }

        public __RPC_Scene_RequestSceneInfo_ARG_int32_serverId_int32_sceneTypeId__ Request { get; private set; }

            private __RPC_Scene_RequestSceneInfo_RET_MsgScenesInfo__ mResponse;
            public MsgScenesInfo Response { get { return mResponse.ReturnValue; } }

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
                mResponse = Serializer.Deserialize<__RPC_Scene_RequestSceneInfo_RET_MsgScenesInfo__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SyncExDataOutMessage : OutMessage
    {
        public SyncExDataOutMessage(ClientAgentBase sender, ulong __characterId__, Dict_int_int_Data changes)
            : base(sender, ServiceType.Scene, 3606, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SyncExData_ARG_Dict_int_int_Data_changes__();
            mMessage.CharacterId = __characterId__;
            Request.Changes=changes;

        }

        public __RPC_Scene_SyncExData_ARG_Dict_int_int_Data_changes__ Request { get; private set; }

            private __RPC_Scene_SyncExData_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_SyncExData_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SyncSceneMountOutMessage : OutMessage
    {
        public SyncSceneMountOutMessage(ClientAgentBase sender, ulong __characterId__, int MountId)
            : base(sender, ServiceType.Scene, 3612, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SyncSceneMount_ARG_int32_MountId__();
            mMessage.CharacterId = __characterId__;
            Request.MountId=MountId;

        }

        public __RPC_Scene_SyncSceneMount_ARG_int32_MountId__ Request { get; private set; }


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

    public class SSGetFubenStoreItemsOutMessage : OutMessage
    {
        public SSGetFubenStoreItemsOutMessage(ClientAgentBase sender, ulong __characterId__, int shopType)
            : base(sender, ServiceType.Scene, 3613, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SSGetFubenStoreItems_ARG_int32_shopType__();
            mMessage.CharacterId = __characterId__;
            Request.ShopType=shopType;

        }

        public __RPC_Scene_SSGetFubenStoreItems_ARG_int32_shopType__ Request { get; private set; }

            private __RPC_Scene_SSGetFubenStoreItems_RET_StoneItems__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_SSGetFubenStoreItems_RET_StoneItems__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSGetFubenStoreItemCountOutMessage : OutMessage
    {
        public SSGetFubenStoreItemCountOutMessage(ClientAgentBase sender, ulong __characterId__, int shopType, int id)
            : base(sender, ServiceType.Scene, 3614, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SSGetFubenStoreItemCount_ARG_int32_shopType_int32_id__();
            mMessage.CharacterId = __characterId__;
            Request.ShopType=shopType;
            Request.Id=id;

        }

        public __RPC_Scene_SSGetFubenStoreItemCount_ARG_int32_shopType_int32_id__ Request { get; private set; }

            private __RPC_Scene_SSGetFubenStoreItemCount_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_SSGetFubenStoreItemCount_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSChangeFubenStoreItemOutMessage : OutMessage
    {
        public SSChangeFubenStoreItemOutMessage(ClientAgentBase sender, ulong __characterId__, int shopType, int id, int num)
            : base(sender, ServiceType.Scene, 3616, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SSChangeFubenStoreItem_ARG_int32_shopType_int32_id_int32_num__();
            mMessage.CharacterId = __characterId__;
            Request.ShopType=shopType;
            Request.Id=id;
            Request.Num=num;

        }

        public __RPC_Scene_SSChangeFubenStoreItem_ARG_int32_shopType_int32_id_int32_num__ Request { get; private set; }

            private __RPC_Scene_SSChangeFubenStoreItem_RET_bool__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_SSChangeFubenStoreItem_RET_bool__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SSBookFightingMonsterIdOutMessage : OutMessage
    {
        public SSBookFightingMonsterIdOutMessage(ClientAgentBase sender, ulong __characterId__, int handbookId)
            : base(sender, ServiceType.Scene, 3619, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SSBookFightingMonsterId_ARG_int32_handbookId__();
            mMessage.CharacterId = __characterId__;
            Request.HandbookId=handbookId;

        }

        public __RPC_Scene_SSBookFightingMonsterId_ARG_int32_handbookId__ Request { get; private set; }


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

    public class MissionChangeSceneRequestOutMessage : OutMessage
    {
        public MissionChangeSceneRequestOutMessage(ClientAgentBase sender, ulong __characterId__, int transId)
            : base(sender, ServiceType.Scene, 3704, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_MissionChangeSceneRequest_ARG_int32_transId__();
            mMessage.CharacterId = __characterId__;
            Request.TransId=transId;

        }

        public __RPC_Scene_MissionChangeSceneRequest_ARG_int32_transId__ Request { get; private set; }

            private __RPC_Scene_MissionChangeSceneRequest_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_MissionChangeSceneRequest_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class GetCharacterDataOutMessage : OutMessage
    {
        public GetCharacterDataOutMessage(ClientAgentBase sender, ulong __characterId__, ulong id)
            : base(sender, ServiceType.Scene, 3990, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_GetCharacterData_ARG_uint64_id__();
            mMessage.CharacterId = __characterId__;
            Request.Id=id;

        }

        public __RPC_Scene_GetCharacterData_ARG_uint64_id__ Request { get; private set; }

            private __RPC_Scene_GetCharacterData_RET_GMCharacterDetailInfo__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_GetCharacterData_RET_GMCharacterDetailInfo__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class UpdateServerOutMessage : OutMessage
    {
        public UpdateServerOutMessage(ClientAgentBase sender, ulong __characterId__, int placeholder)
            : base(sender, ServiceType.Scene, 3991, (int)MessageType.SAS)
        {
            Request = new __RPC_Scene_UpdateServer_ARG_int32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Scene_UpdateServer_ARG_int32_placeholder__ Request { get; private set; }


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
            : base(sender, ServiceType.Scene, 3993, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_GMCommand_ARG_StringArray_commonds__();
            mMessage.CharacterId = __characterId__;
            Request.Commonds=commonds;

        }

        public __RPC_Scene_GMCommand_ARG_StringArray_commonds__ Request { get; private set; }

            private __RPC_Scene_GMCommand_RET_Int32Array__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_GMCommand_RET_Int32Array__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class CloneCharacterDbByIdOutMessage : OutMessage
    {
        public CloneCharacterDbByIdOutMessage(ClientAgentBase sender, ulong __characterId__, ulong fromId, ulong toId)
            : base(sender, ServiceType.Scene, 3994, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_CloneCharacterDbById_ARG_uint64_fromId_uint64_toId__();
            mMessage.CharacterId = __characterId__;
            Request.FromId=fromId;
            Request.ToId=toId;

        }

        public __RPC_Scene_CloneCharacterDbById_ARG_uint64_fromId_uint64_toId__ Request { get; private set; }

            private __RPC_Scene_CloneCharacterDbById_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_CloneCharacterDbById_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class UpdateHoldIdOutMessage : OutMessage
    {
        public UpdateHoldIdOutMessage(ClientAgentBase sender, ulong __characterId__, int GuildId)
            : base(sender, ServiceType.Scene, 3996, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_UpdateHoldId_ARG_int32_GuildId__();
            mMessage.CharacterId = __characterId__;
            Request.GuildId=GuildId;

        }

        public __RPC_Scene_UpdateHoldId_ARG_int32_GuildId__ Request { get; private set; }

            private __RPC_Scene_UpdateHoldId_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_UpdateHoldId_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class NotifyRefreshLodeTimerOutMessage : OutMessage
    {
        public NotifyRefreshLodeTimerOutMessage(ClientAgentBase sender, ulong __characterId__, int ServerId, Int32Array ids)
            : base(sender, ServiceType.Scene, 3713, (int)MessageType.SAS)
        {
            Request = new __RPC_Scene_NotifyRefreshLodeTimer_ARG_int32_ServerId_Int32Array_ids__();
            mMessage.CharacterId = __characterId__;
            Request.ServerId=ServerId;
            Request.Ids=ids;

        }

        public __RPC_Scene_NotifyRefreshLodeTimer_ARG_int32_ServerId_Int32Array_ids__ Request { get; private set; }


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

    public class SyncFlagDataOutMessage : OutMessage
    {
        public SyncFlagDataOutMessage(ClientAgentBase sender, ulong __characterId__, Dict_int_int_Data changes)
            : base(sender, ServiceType.Scene, 3999, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SyncFlagData_ARG_Dict_int_int_Data_changes__();
            mMessage.CharacterId = __characterId__;
            Request.Changes=changes;

        }

        public __RPC_Scene_SyncFlagData_ARG_Dict_int_int_Data_changes__ Request { get; private set; }

            private __RPC_Scene_SyncFlagData_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_SyncFlagData_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class SceneEquipModelStateChangeOutMessage : OutMessage
    {
        public SceneEquipModelStateChangeOutMessage(ClientAgentBase sender, ulong __characterId__, int part, int state, ItemBaseData equip)
            : base(sender, ServiceType.Scene, 4000, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_SceneEquipModelStateChange_ARG_int32_part_int32_state_ItemBaseData_equip__();
            mMessage.CharacterId = __characterId__;
            Request.Part=part;
            Request.State=state;
            Request.Equip=equip;

        }

        public __RPC_Scene_SceneEquipModelStateChange_ARG_int32_part_int32_state_ItemBaseData_equip__ Request { get; private set; }

            private __RPC_Scene_SceneEquipModelStateChange_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_SceneEquipModelStateChange_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class NodifyModifyPlayerNameOutMessage : OutMessage
    {
        public NodifyModifyPlayerNameOutMessage(ClientAgentBase sender, ulong __characterId__, ulong characterId, string modifyName)
            : base(sender, ServiceType.Scene, 3712, (int)MessageType.SAS)
        {
            Request = new __RPC_Scene_NodifyModifyPlayerName_ARG_uint64_characterId_string_modifyName__();
            mMessage.CharacterId = __characterId__;
            Request.CharacterId=characterId;
            Request.ModifyName=modifyName;

        }

        public __RPC_Scene_NodifyModifyPlayerName_ARG_uint64_characterId_string_modifyName__ Request { get; private set; }


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

    public class NotifyRefreshBossHomeOutMessage : OutMessage
    {
        public NotifyRefreshBossHomeOutMessage(ClientAgentBase sender, ulong __characterId__, Int32Array ids)
            : base(sender, ServiceType.Scene, 3711, (int)MessageType.SAS)
        {
            Request = new __RPC_Scene_NotifyRefreshBossHome_ARG_Int32Array_ids__();
            mMessage.CharacterId = __characterId__;
            Request.Ids=ids;

        }

        public __RPC_Scene_NotifyRefreshBossHome_ARG_Int32Array_ids__ Request { get; private set; }


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

    public class NotifyBossHomeKillOutMessage : OutMessage
    {
        public NotifyBossHomeKillOutMessage(ClientAgentBase sender, ulong __characterId__, int placeholder)
            : base(sender, ServiceType.Scene, 3714, (int)MessageType.SAS)
        {
            Request = new __RPC_Scene_NotifyBossHomeKill_ARG_int32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Scene_NotifyBossHomeKill_ARG_int32_placeholder__ Request { get; private set; }


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

    public class CheckCanAcceptChallengeOutMessage : OutMessage
    {
        public CheckCanAcceptChallengeOutMessage(ClientAgentBase sender, ulong __characterId__, int placeholder)
            : base(sender, ServiceType.Scene, 3715, (int)MessageType.SS)
        {
            Request = new __RPC_Scene_CheckCanAcceptChallenge_ARG_int32_placeholder__();
            mMessage.CharacterId = __characterId__;
            Request.Placeholder=placeholder;

        }

        public __RPC_Scene_CheckCanAcceptChallenge_ARG_int32_placeholder__ Request { get; private set; }

            private __RPC_Scene_CheckCanAcceptChallenge_RET_int32__ mResponse;
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
                mResponse = Serializer.Deserialize<__RPC_Scene_CheckCanAcceptChallenge_RET_int32__>(ms);
            }
            State = MessageState.Reply;
            ErrorCode = (int) error;
        }
        public override bool HasReturnValue { get { return true; } }
    }

    public class AddFunctionNameScene
    {
        public static void AddFunctionName(IDictionary<int, string> dict)
        {
            dict[3000] = "SBChangeScene";
            dict[3001] = "SBDestroyScene";
            dict[3002] = "NotifySceneNotExist";
            dict[3010] = "BSCreateScene";
            dict[3020] = "UnloadData";
            dict[3030] = "SBGetAllOnlineCharacterInServer";
            dict[3039] = "SBCheckCharacterOnline";
            dict[3040] = "QueryBrokerStatus";
            dict[3050] = "SBReloadTable";
            dict[3051] = "PrepareDataForEnterGame";
            dict[3052] = "PrepareDataForCreateCharacter";
            dict[3053] = "PrepareDataForCommonUse";
            dict[3054] = "PrepareDataForLogout";
            dict[3015] = "CreateCharacter";
            dict[3016] = "DelectCharacter";
            dict[3055] = "SSEnterScene";
            dict[3056] = "GetSceneSimpleData";
            dict[3057] = "CheckConnected";
            dict[3058] = "CheckLost";
            dict[3059] = "QueryStatus";
            dict[3061] = "NotifyConnected";
            dict[3062] = "NotifyLost";
            dict[3063] = "NotifySceneFinished";
            dict[3064] = "AskEnterDungeon";
            dict[3065] = "CreateObjAround";
            dict[3066] = "NotifyPlayerPickUpFubenReward";
            dict[3069] = "SBChangeSceneByTeam";
            dict[3071] = "MergeSceneByTeam";
            dict[3072] = "IsSceneExist";
            dict[3073] = "LoginEnterScene";
            dict[3074] = "ReplyChangeScene";
            dict[3075] = "ApplyPlayerData";
            dict[3076] = "ChangeSceneOver";
            dict[3077] = "ApplyAttribute";
            dict[3078] = "CreateObj";
            dict[3079] = "DeleteObj";
            dict[3080] = "DeleteObjList";
            dict[3081] = "MoveTo";
            dict[3082] = "SyncMoveTo";
            dict[3083] = "SyncMoveToList";
            dict[3084] = "StopMove";
            dict[3085] = "SyncStopMove";
            dict[3086] = "DirectTo";
            dict[3087] = "SyncDirection";
            dict[3088] = "SendUseSkillRequest";
            dict[3089] = "NotifyUseSkill";
            dict[3090] = "NotifyUseSkillList";
            dict[3091] = "SyncBuff";
            dict[3092] = "SceneEquipChange";
            dict[3093] = "SceneSkillChange";
            dict[3094] = "SceneEquipSkill";
            dict[3095] = "SceneInnateChange";
            dict[3096] = "SceneBookAttrChange";
            dict[3097] = "GMScene";
            dict[3098] = "NotifyShootBullet";
            dict[3099] = "NotifyShootBulletList";
            dict[3100] = "SendTeleportRequest";
            dict[3101] = "ChangeSceneRequest";
            dict[3102] = "NotifyEquipChanged";
            dict[3103] = "SceneTeamMessage";
            dict[3104] = "ApplySceneObj";
            dict[3105] = "PickUpItem";
            dict[3106] = "SceneChatMessage";
            dict[3107] = "UseSkillItem";
            dict[3108] = "ServerGMCommand";
            dict[3109] = "FindCharacterName";
            dict[3110] = "FindCharacterFriend";
            dict[3111] = "ExitDungeon";
            dict[3112] = "PickUpItemSuccess";
            dict[3113] = "NotifySceneAction";
            dict[3114] = "BagisFull";
            dict[3115] = "NotifySomeClientMessage";
            dict[3116] = "MoveToRobot";
            dict[3117] = "NpcService";
            dict[3118] = "NotifyDungeonTime";
            dict[3119] = "NotifyScenePlayerCityData";
            dict[3120] = "SyncSceneBuilding";
            dict[3121] = "ReliveType";
            dict[3122] = "DebugObjPosition";
            dict[3123] = "SSGoToSceneAndPvP";
            dict[3124] = "SyncCharacterPostion";
            dict[3126] = "ChangePKModel";
            dict[3128] = "FlyTo";
            dict[3129] = "SSPvPSceneCampSet";
            dict[3132] = "NotifyBattleReminder";
            dict[3133] = "NotifyCountdown";
            dict[3134] = "NotifyCreateSpeMonster";
            dict[3135] = "ApplySceneTeamLeaderObj";
            dict[3136] = "SSGetCharacterSceneData";
            dict[3137] = "NotifyDamageList";
            dict[3138] = "NotifyFubenInfo";
            dict[3139] = "BossDie";
            dict[3140] = "ReadyToEnter";
            dict[3141] = "NotifyMessage";
            dict[3142] = "NotifyCampChange";
            dict[3143] = "GetLeaveExp";
            dict[3144] = "ApplyLeaveExp";
            dict[3145] = "GetFriendSceneSimpleData";
            dict[3146] = "SendAddFriend";
            dict[3147] = "SendDeleteFriend";
            dict[3148] = "SendOutLineFriend";
            dict[3150] = "SyncDataToClient";
            dict[3151] = "SyncMyDataToClient";
            dict[3152] = "SSExitDungeon";
            dict[3153] = "NotifyStrongpointStateChanged";
            dict[3154] = "ChangeSceneRequestByMission";
            dict[3155] = "ApplyPlayerPostionList";
            dict[3156] = "SyncObjPosition";
            dict[3157] = "SSAllianceBuffDataChange";
            dict[3158] = "SSAllianceDataChange";
            dict[3159] = "Inspire";
            dict[3160] = "ObjSpeak";
            dict[3161] = "SceneTitleChange";
            dict[3162] = "NotifyItemCount";
            dict[3163] = "SyncLevelChange";
            dict[3164] = "AllianceWarRespawnGuard";
            dict[3165] = "MotifyServerAvgLevel";
            dict[3166] = "NotifyAllianceWarNpcData";
            dict[3167] = "NotifyScenePlayerInfos";
            dict[3168] = "NotifyNpcStatus";
            dict[3169] = "AddAutoActvity";
            dict[3170] = "SSAddBuff";
            dict[3171] = "Silence";
            dict[3172] = "SSApplyNpcHP";
            dict[3173] = "NotifyPointList";
            dict[3174] = "NotifyStartWarning";
            dict[3175] = "SSSceneElfChange";
            dict[3499] = "SBReconnectNotifyScene";
            dict[3500] = "SBCleanClientCharacterData";
            dict[3501] = "SSNotifyCharacterOnConnet";
            dict[3502] = "SBGetServerCharacterCount";
            dict[3503] = "BSNotifyCharacterOnLost";
            dict[3600] = "SyncTowerSkillLevel";
            dict[3601] = "GetSceneNpcPos";
            dict[3602] = "SendMieshiResult";
            dict[3603] = "SyncPlayerMieshiContribution";
            dict[3604] = "NotifyRefreshDungeonInfo";
            dict[3605] = "RequestSceneInfo";
            dict[3606] = "SyncExData";
            dict[3607] = "NotifyStartXpSkillGuide";
            dict[3608] = "NotifyStartMaYaFuBenGuide";
            dict[3610] = "BroadcastSceneMonsterCount";
            dict[3611] = "SummonMonster";
            dict[3612] = "SyncSceneMount";
            dict[3613] = "SSGetFubenStoreItems";
            dict[3614] = "SSGetFubenStoreItemCount";
            dict[3616] = "SSChangeFubenStoreItem";
            dict[3617] = "FastReach";
            dict[3618] = "BroadcastSceneChat";
            dict[3619] = "SSBookFightingMonsterId";
            dict[3700] = "SyncFuBenStore";
            dict[3701] = "NotifyTeamMemberScene";
            dict[3702] = "ForceStopMove";
            dict[3703] = "NotifyCommonCountdown";
            dict[3704] = "MissionChangeSceneRequest";
            dict[3705] = "CK_NotifyClientLevelup";
            dict[3706] = "CK_ApplyLevelupBuff";
            dict[3707] = "CK_NotifyCheckenSceneInfo";
            dict[3708] = "CK_NotifyHurt";
            dict[3709] = "CK_NotifyRankList";
            dict[3710] = "ApplySolveStuck";
            dict[3990] = "GetCharacterData";
            dict[3991] = "UpdateServer";
            dict[3992] = "SyncModelId";
            dict[3993] = "GMCommand";
            dict[3994] = "CloneCharacterDbById";
            dict[3995] = "CollectLode";
            dict[3996] = "UpdateHoldId";
            dict[3997] = "HoldLode";
            dict[3998] = "NotifyLodeInfo";
            dict[3713] = "NotifyRefreshLodeTimer";
            dict[3999] = "SyncFlagData";
            dict[4000] = "SceneEquipModelStateChange";
            dict[3712] = "NodifyModifyPlayerName";
            dict[4002] = "NotifyPlayEffect";
            dict[3711] = "NotifyRefreshBossHome";
            dict[3714] = "NotifyBossHomeKill";
            dict[3715] = "CheckCanAcceptChallenge";
        }
        public static void AddCSFunctionName(IDictionary<int, string> dict)
        {
            dict[3065] = "CreateObjAround";
            dict[3075] = "ApplyPlayerData";
            dict[3076] = "ChangeSceneOver";
            dict[3077] = "ApplyAttribute";
            dict[3081] = "MoveTo";
            dict[3084] = "StopMove";
            dict[3086] = "DirectTo";
            dict[3088] = "SendUseSkillRequest";
            dict[3097] = "GMScene";
            dict[3100] = "SendTeleportRequest";
            dict[3101] = "ChangeSceneRequest";
            dict[3104] = "ApplySceneObj";
            dict[3105] = "PickUpItem";
            dict[3106] = "SceneChatMessage";
            dict[3111] = "ExitDungeon";
            dict[3115] = "NotifySomeClientMessage";
            dict[3116] = "MoveToRobot";
            dict[3117] = "NpcService";
            dict[3121] = "ReliveType";
            dict[3126] = "ChangePKModel";
            dict[3128] = "FlyTo";
            dict[3135] = "ApplySceneTeamLeaderObj";
            dict[3143] = "GetLeaveExp";
            dict[3144] = "ApplyLeaveExp";
            dict[3154] = "ChangeSceneRequestByMission";
            dict[3155] = "ApplyPlayerPostionList";
            dict[3159] = "Inspire";
            dict[3164] = "AllianceWarRespawnGuard";
            dict[3601] = "GetSceneNpcPos";
            dict[3611] = "SummonMonster";
            dict[3617] = "FastReach";
            dict[3706] = "CK_ApplyLevelupBuff";
            dict[3710] = "ApplySolveStuck";
            dict[3995] = "CollectLode";
            dict[3997] = "HoldLode";
        }
    }
}
