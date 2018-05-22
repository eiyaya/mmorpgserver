using System;

public enum ErrorCodes
{
        OK = 0,                         //没有错误
        Unknow = 1,                     //未知错误
        Gm = 2,                         //GM命令
        Unline = 3,                     //玩家不在线
        MoneyNotEnough = 4,             //金钱不足
        ItemNotEnough = 5,              //道具不足
        DiamondNotEnough = 6,           //钻石不足
        NameNotFindCharacter = 7,       //名字不存在
        RoleIdError = 8,                //职业不符
        ParamError = 9,                 //参数不合法
        CharacterFull = 10,             //角色已满
        PasswordIncorrect = 11,         //密码错误
        CharacterLevelMax = 12,        	//玩家等级最大值
        Error_Condition = 13,           //Condition表，测试未通过
        VipLevelNotEnough = 14,         //Vip等级不足

        ClientIdNoPower = 50,			//没有权限的ClientId
		LoginStateError = 51,			//登陆状态不符
		SelectServerNoCharacter = 52,	//选择的服务器没有角色
		SelectServerNoThisCharacter = 53,	//角色不在选择的服务器中
		PlayerCreateFaild =54,				//账号创建失败
		ClientLoginMore =55,				//客户端重复登陆
		PlayerLoginning =56,				//账号已在登陆过程中
		PlayerEnterGamming =57,				//账号已在进入游戏过程中
		Error_OnConnetGameFailed =58,			//账号已在进入游戏过程中
        PlayerAccountIsLock   = 59,                            //账号已被封
        ConnectFail  = 60,                              //登录过程中，连接时又断开连接了

        DataBase  = 90,                 //数据库异常
        StateError = 91,               //玩家当前状态不正确
        ServerNotSame = 98,             //服务器ID不一致
        ServerID = 99,                  //错误的服务器ID


        //------通用   = 100 -----
        Error_DataOverflow = 100,       //数据溢出
        Error_ItemID = 101,             //非法的道具ID
        Error_BagID = 102,              //非法的包裹ID
        Error_BuffID = 103,             //非法的BuffID
        Error_Death = 104,              //死亡时无法操作
        Error_InnateID = 105,           //非法的天赋ID
        Error_GemID = 106,           	//非法的宝石ID
        Error_EquipID = 107,           	//非法的装备ID
        Error_LevelNoEnough = 108,      //玩家等级不足
        Error_MissionID = 109,          //非法的任务ID
        Error_GiftID = 110,           	//非法的奖励ID
        Error_EquipExcellentID = 111, 	//非法装备卓越玩法ID
        Error_EquipBlessingID = 112, 	//非法装备强化玩法ID
        Error_EquipAdditionalID = 113, 	//非法装备追加玩法ID
        Error_SkillID = 114,           	//非法的技能ID
        Error_BookID = 115,           	//非法的图鉴ID
        Error_BookGroupID = 116,        //非法的图鉴组ID
        Error_ItemComposeID = 117,      //非法的组合ID
        Error_AchievementID = 118,      //非法的成就ID
        Error_FubenID = 119,      	//非法的副本ID
        Error_BuildID = 120,      	//非法的建筑ID
        Error_NpcBaseID = 121,      	//非法的NPCID
        Error_ServiceID = 122,      	//非法的NPC服务ID
        Error_StoreID = 123,      	//非法的商店道具ID
        Error_BuildServiceID = 124,     //非法的建筑服务ID
        Error_PetID = 125,     		//非法的宠物ID
        Error_SailingID = 126,     	//非法的航海ID
        Error_WingID = 127,     	//非法的翅膀ID
        Error_MedalID = 128,     	//非法的勋章ID
        Error_forgedID = 129,     	//非法的铸造ID
        Error_GuildID = 130,     	//非法的战盟等级ID
        Error_EquipUpdata = 131,     	//非法的装备进阶ID
        Error_TradeID = 132,     	//非法的装备进阶ID
        Error_TransmigrationID = 133,   //非法的转生ID
        Error_HonorID = 134,     	//非法的军衔ID
        Error_HomeLevelNoEnough = 135,  //要塞等级不足
        Error_TableData = 136,          //表格配置错误
        Error_NameTitleID = 137,        //非法的NameTitle ID

        //------ Login = 500-----
        Error_PLayerLoginMore = 500,    //玩家爆满，请稍后尝试
        Error_PLayerLoginWait = 501,    //玩家登陆排队
        Error_EnterGameMore = 502,      //玩家爆满，请稍后尝试
        Error_EnterGameWait = 503,      //玩家选服排队
        Error_Login = 510,              //登录状态不对

        //------尹东斐 = 1000-----
        Error_CreateControllerFailed = 1000,    //创建角色失败
        Error_PrepareEnterGameFailed = 1001,    //进入游戏失败
        Error_EnterGameFailed = 1002,           //进入游戏失败
        Error_PathInvalid = 1003,               //移动路径非法
        Error_DataInvalid = 1004,               //收到错误的网络包

        //------                 = 2000-----
        Error_ItemNoInBag_All = 2001,   //道具无法全部放入该包裹
        Error_ResNoEnough = 2002,       //资源或道具数量不足
        Error_ItemNoInBag = 2003,       //道具无法放入相应的包裹
        Error_ResIdOverflow = 2004,     //资源ID越界
        Error_MoveItemFalse = 2005,     //道具移动位置失败
        Error_BagIndexNoItem = 2006,    //包裹索引没有道具
        Error_BagIndexOverflow = 2007,  //包裹索引越界
        Error_ItemIsNoEquip = 2008,     //该道具不是装备
        Error_ItemNotFind = 2009,   	//道具没有找到
        Error_ItemIsNoGem = 2010,     	//该道具不是宝石
        Error_GemSlotOverflow = 2011,   //宝石槽越界
        Error_SlotNoOpen = 2012,   		//宝石槽没有开
        Error_GemCountNoEnouth = 2013,  //宝石数量不足
        Error_GemIdNoSame = 2014,  		//宝石ID不一致
        Error_ItemNotSell = 2015,  		//道具无法出售
        Error_EquipPart = 2016,  		//装备点不符
        Error_EquipIndex = 2017,  		//装备索引不符
        Error_EquipLevelMax = 2018,  		//装备已经最大等级
        Error_EquipAdditionalMax = 2019,	//装备追加已经最大值
        Error_EquipLockMax = 2020,	//装备锁定超过最大属性条数
        Error_EquipLevelTooHigh = 2021,	//目标装备的强化等级过高
        Error_EquipTypeNotSame = 2022,	//装备类型不一致
        Error_EquipAdditionalTooHigh = 2023,	//目标装备的追加点数过高
        Error_EquipSmritiExcellentMax = 2024,	//承装备卓越属性已经大于等于传承装备卓越属性，无需传承
        Error_EquipAdditionalTypeNotSame = 2025,	//追加属性类型不一致
        Error_AttrNotEnough = 2026,			//属性需求不满足        
        Error_SkillTalentMax = 2027,		//技能天赋数到达上限
        Error_NotCallBack = 2028,			//装备无法回收
        Error_SkillTalentNoReset = 2029,		//技能无需重置
        Error_ItemNotUse = 2030,			//道具无法使用
        Error_EquipSmritTypeNotSame = 2031,		//装备类型不一致，但是可以继续传承
        Error_EquipNoAdditionalNoSmrit = 2032,		//装备没有追加过，无法传承

        Error_GiftAlreadyReceive = 2050,	//奖励已经领取
        Error_GiftTimeNotEnough = 2051,		//奖励时间不到
        Error_GiftCountNotEnough = 2052,	//奖励的次数不足
        Error_ActivityPointNotEnough = 2053,	//活跃度不足

        Error_BuffLevelTooLow = 2100,   //Buff优先级太低了

        Error_GroupShopCountNotEnough = 2120,   //团购次数不足
        Error_GroupShopOver = 2121,   //该团购已结束不足

        Error_NotSelectUpgrade = 2150,   //占星材料不能选取要升级的目标
        Error_GemLevelMax = 2151,   	//宝石到达最大等级
        Error_GemTypeSame = 2152,   	//已镶嵌同类型的宝石


        Error_SkillNoCD = 2200,         //技能没有CD
        Error_MpNoEnough = 2201,        //MP不足
        Error_NotHaveSkill = 2202,      //没有该技能
        Error_AngerNoEnough = 2203,     //怒气不足
        Error_HpNoEnough = 2204,        //HP不足
        Error_SkillNoTarget = 2205,    	//没有技能目标
        Error_SkillDistance = 2206,    	//技能距离不足
        Error_SkillNotUse = 2207,    	//技能被限制
        Error_InnateNoBefore = 2210,    //天赋前置不足
        Error_InnateMaxLayer = 2211,    //天赋已经达到最大层数
        Error_InnateNoPoint = 2212,    	//天赋没有剩余点数
        Error_SkillNotCast = 2213,     	//技能是被动无法释放
        Error_CharacterDie = 2214,   	//角色已经死了
        Error_SkillLevelMax = 2215,     	//技能等级到达上限
        Error_SkillNoTUpgrade = 2216,     	//该技能无法升级
        Error_CharacterNoDie = 2217,     	//角色没死
        Error_CharacterCamp  =2218,			//阵营不符
        Error_SafeArea = 2219,         	//技能处在安全区
        Error_HpMax = 2220,     			//血已满
        Error_MpMax = 2221,     			//蓝已满
        Error_AttrPointNotEnough = 2222,   //可分配点数不足
        Error_SkillNeedWeapon = 2223,           //需要武器
        Error_InnateZero = 2224,			//还没有点天赋
        Error_ExdataMax = 2225,				//扩展数据到达最大值了
        Error_ItemWaste = 2226,				//物品会浪费
        Error_KillerValue = 2227,			//杀气值不符
        Error_HonorMax = 2228,				//军衔到达最大值
        Error_Dizzy = 2229,                          //你正在眩晕状态


        Error_NpcNotFind = 2250,   			//NPC没找到
        Error_NpcNotHaveService = 2251,   			//NPC没有该服务
        Error_NpcTooFar = 2252,   			//NPC太远了
        Need_2_Logic = 2253,   			//转至Logic来使用NPC服务

        Error_WorshipCount = 2280,   		//崇拜次数不足
        Error_WorshipAlready = 2281,   		//已经崇拜过了
        Error_CharacterSame= 2282,   		//不能崇拜自己

        Error_AcceptMission = 2300,     	//任务接受失败
        Error_NotHaveMission = 2301,     	//没有该任务
        Error_ConditionNoEnough = 2302,     //任务条件不满足

        Error_CompensationNotFind = 2310,     	//补偿已领取


        Error_AchievementNotFinished = 2350,//成就未满足
        Error_RewardAlready = 2351,			//成就已领取

        Error_CharacterHaveTeam = 2400,		//玩家已经有队伍了
        Error_TeamNotFind = 2401,			//不存在的队伍ID
        Error_CharacterNotTeam = 2402,		//玩家不属于这个队伍了
        Error_TeamIsFull = 2403,			//队伍已满
        Error_CharacterOutLine = 2404,		//玩家已离线
        Error_CharacterNotLeader = 2405,	//玩家不是队长
        Error_TeamNotSame = 2406,			//队伍不一致
        Error_CharacterNotInvite = 2407,    //玩家没有被邀请
        Error_CharacterNoTeam = 2408,    	//玩家没有队伍
        Error_AlreadyToLeader = 2409,		//已向队长发送组队邀请
        Error_SetRefuseTeam = 2410,			//玩家设置了拒绝邀请
        Error_OtherHasTeam = 2411,          //对方已经有队伍了

        Error_ChatChannel = 2450,           //错误的聊天频道
        Error_ChatNone = 2451,           	//聊天信息为空
        Error_WhisperNameNone = 2452,       //私聊名字不足
        Error_SetRefuseWhisper = 2453,		//玩家设置了拒绝私聊
        Error_SetShieldYou = 2454,			//你被屏蔽了
        Error_NotWhisperSelf = 2455,		//不能私聊自己
        Error_ChatLengthMax = 2456,			//聊天太长了
        Error_SetYouShield = 2457,          //屏蔽了目标
        Error_Cannot_Challenge = 2458,      //对方不能接受角斗
        
        Error_BookActivated = 2500,                                     //图鉴已经激活了
        Error_BookNotSame = 2501,                                       //消耗图鉴与配置不同
        Error_FriendIsHave = 2550,                                      //已经有好友了
        Error_FriendIsMore = 2551,                                      //好友太多
        Error_EnemyIsMore = 2552,                                       //仇人太多
        Error_ShieldIsMore = 2553,                                      //屏蔽太多
        Error_NotAddSelf = 2554,                                        //无法添加自己
        Error_NoThisSelf = 2555,                                        //没有此好友

        Error_FubenCountNotEnough = 2600,                               //副本次数到达上限
        Error_FubenResetCountNotEnough = 2601,                          //副本重置次数到达上限
        Error_PassFubenTimeNotEnough = 2602,                            //副本时间不满足扫荡条件
        Error_FubenNoPass = 2603,                                       //副本不可扫荡

        Error_BattleNoWin = 2610,                                       //战场没有取得胜利
        Error_BattleHasAccept = 2611,                                   //已经领取了奖励

        Error_StringIsNone = 2650,                                      //字符为空
        Error_StoreBuyCountMax = 2651,                                  //限购次数不足
        Error_StoreNotHaveItem = 2652,                                  //商店没有该道具


        Error_MatchingResultError = 2700,                               //排队反馈非法
        Error_MatchingTeamNotFindCharacter = 2701,                      //排队玩家的队伍中，并没有该玩家
        Error_MatchingTeamStateNotConform = 2702,                       //排队玩家的队伍的状态不符
        Error_QueueCountMax = 2703,					//排队人数超过上限
        Error_CharacterHaveQueue = 2704,                                //队伍中有人已经在排队了
        Error_CharacterCantQueue = 2705,                                //队伍中有人不能排队



        Error_MailNotFind = 2750,                                       //邮件没有找到
        Error_MailReceiveOver = 2751,                                   //邮件已领取

        Error_ExchangeItemState = 2770,                                 //交易道具状态不符
        Error_ExchangeFreeBroadcast = 2771,                             //交易免费广播时间不到
        Error_NoBuyCharacterItem = 2772,                                //无权购买此人的道具
        Error_ItemNoExchange = 2773,                                    //道具不允许交易
        Error_ExchangeValueNotEnough = 2774,                            //交易价格不符

        Error_BuildAreaNotEmpty = 2800,                                 //建筑区域非空
        Error_BuildCountMax = 2801,                                     //建筑数量到达上限
        Error_BuildNotFind = 2802,                                      //建筑没有找到
        Error_NeedCityLevelMore = 2803,                                 //需要更高的议事厅等级
        Error_BuildNotService = 2804,                                   //该建筑不支持该服务
        Error_BuildStateError = 2805,                                   //该建筑状态不符
        Error_BuildPetMax = 2806,                                       //该建筑的宠物已满
        Error_BuildNotFindPet = 2807,                                   //该建筑的没有该宠物
        Error_BuildLevelMax = 2808,                                     //该建筑已经到达最大等级
        Error_AlreadyHaveSeed = 2820,                                   //已经有其他作物了
        Error_NotFindSeed = 2821,                                       //没有找到作物
        Error_SeedTimeNotOver = 2822,                                   //作物还没到时间
        Error_NeedFarmLevelMore = 2823,                                 //需要更高级的农场
        Error_FarmNotAddSpeed = 2824,                                   //不需要加速了
        Error_ItemNot91000 = 2825,                                      //物品不是催产剂
        Error_RouteState = 2830,                                        //航线状态不符
        Error_RouteTime = 2831,                                         //航线时间还没到
        Error_MedalMaterialNotFind = 2832,                              //勋章材料没找到
        Error_MedalNotMaterial = 2833,                                  //已使用的勋章无法作为材料
        Error_MedalNotEquip = 2834,                                     //勋章无法装备
        Error_MedalEquipFull = 2835,                                    //勋章已满
        Error_HatchTimeOver = 2836,                                     //孵化时间已到
        Error_TempleCountNotEnough = 2837,                              //神像打扫次数不足
        Error_TempleNoCD = 2838,                                        //神像打扫没冷却

        Error_PetMissionNotFind = 2850,                                 //没有可接的宠物任务ID
        Error_PetMissionState = 2851,                                   //宠物任务的状态不符
        Error_PetPartakeCount = 2852,                                   //宠物参与数量不符
        Error_PetNotFind = 2853,                                        //宠物参与数量不符
        Error_PetState = 2854,                                          //宠物的状态不符
        Error_PetIsHave = 2855,                                         //同类宠物已存在
        Error_PetStarMax = 2856,                                        //同类宠物已经进阶到最大星级了
        Error_DoPetMissionCountMax = 2857,                              //进入可完成的随从任务到达上限了

        Error_Build8CastingTimeNotOver = 2860,                          //铸造时间未到
        Error_Build8CastingCountMax = 2861,                             //同时铸造最大数量超出
        Error_Build8CastingNotFind = 2862,                              //同时铸造最大数量超出
        Error_Build8EquipNotUpdata = 2863,                              //装备无法进阶
        Error_Build8EquipNotSame = 2864,                                //装备进阶不一致
        Error_Build8CastingTimeOver = 2865,                             //铸造已结束

        Error_CityMissionNotFind = 2890,                                //家园任务没找到
        Error_CityMissionState = 2891,                                  //家园任务状态不符
        Error_CityMissionFreeCd = 2892,                                 //家园任务免费刷新次数不足
        Error_CityMissionTime = 2893,                                   //家园任务时间不符

        Error_ElfNotFind = 2900,                                        //精灵没有找到
        Error_ElfAlreadyBattle = 2901,                                  //精灵已经在场上了
        Error_ElfNotBattle = 2902,                                      //精灵没有在场上了
        Error_ElfBattleMax = 2903,                                      //精灵已经到达上限
        Error_ElfIsBattleMain = 2904,                                   //精灵已经出战
        Error_ElfNotBattleMain = 2905,                                  //精灵并没有出战
        Error_FormationLevelMax = 2906,                                 //阵法已到最大等级
        Error_FormationExpNotEnough = 2907,                             //阵法经验不足
        Error_ElfLevelMax = 2908,                                       //精灵达到最大等级        
        Error_ElfTypeSame = 2909,                                       //已经有同类型精灵出阵了
        Error_ElfStarMax = 2910,                                        //精灵达到最大星级
        Error_ElfConsumeArrayNotFound = 2911,                           //消耗表未找到
        Error_ElfStarUpFail = 2912,                                     //升星失败
        Error_ElfMaxSkill = 2913,                                       //技能达到最大等级了
        Error_ELfItemType = 2914,                                       //错误的精灵技能石类型

        Error_WingNotFind = 2920,  					//还没有翅膀
        Error_WingLevelMax = 2921,					//翅膀进阶到最大了
        Error_WingTypeLevelMax = 2922,                                  //翅膀培养到最大等级了
        Error_NeedWingLevelMore = 2923,                                 //需要更高阶的翅膀

        Error_LadderChange = 2950,                                      //天梯名次发生变化
        Error_CountNotEnough = 2951,                                    //天梯次数不足
        Error_LadderTime = 2952,					//天梯时间还没到

        Error_AllianceNameSame= 2970,					//战盟名字重复
        Error_CharacterHaveAlliance= 2971,				//已经有战盟
        Error_AllianceState	= 2972,					//战盟状态不符
        Error_AllianceNotFind	= 2973,					//战盟未找到
        Error_CharacterNoAlliance	= 2974,				//没有战盟
        Error_AlreadyApply	= 2975,					//已经申请
        Error_AllianceNoApply	= 2976,                                 //还没有申请
        Error_AllianceLeaderNotExit	= 2977,				//盟主不能退出
        Error_CharacterNotFind	= 2978,					//没有找到玩家
        Error_AllianceIsFull	= 2979,					//战盟已满
        Error_JurisdictionNotEnough	= 2980,				//成员权限不足
        Error_AllianceIsNotSame	= 2981,					//战盟不同
        Error_AllianceApplyIsFull	= 2982,                         //申请列表已满
        Error_AllianceMissionNotFind= 2983,                             //战盟没有该任务
        Error_AllianceBuffMax     = 2984,                               //战盟buff到达上限
        Error_AllianceLevelMax     = 2985,                              //战盟等级到达上限
        Error_AllianceExpNotEnough     = 2986,                          //战盟贡献不足
        Error_AllianceApplyJoinOK     = 2987,                           //申请战盟后自动已经加入了
        Error_AllianceDonationCount     = 2988,                         //战盟捐献次数不足
        Error_AllianceBuffID     = 2989,                               //战盟buffId错误
        Error_GongjiNotEnough     = 2990,                               //战盟功绩不足
        Error_CheckAllianceLevel    = 2991,                               //需要检查战盟等级
        Error_AllianceLeveNotEnough     = 2992,                              //战盟等级不足
        Error_AllianceMoneyNotEnough     = 2993,                              //战盟资金不足
        Error_Alliance_Limit_Time        =2994,                               //刚刚退出战盟

        //------白露   = 3000-----
        Error_LoginDB_NoCharacter = 3000,                               //LoginDB中无法找到这个角色
        Error_NoObj = 3001,                                             //当前场景没有这个Obj
        Error_NotTheOwner = 3002,                                       //不是拥有者
        Error_NoTransfer = 3003,                                        //传送点没找到
        Error_CannotMove = 3004,                                        //不能移动
        Error_DistanceTooMuch = 3005,                                   //还没有到达
        Error_CityCanotBuildMore = 3006,                                //不能再建造更多建筑

        //------运营活动 = 3200----
        Error_Operation_NotFound = 3200,                                 //没有找到该活动
        Error_Operation_NotInActivity = 3201,                            //当前时间活动未开启
        Error_Operation_CannotAquire = 3202,                            //不能再领取了



        //------董士哲 = 4000-----
        Error_Connect_find = 4100,                                    	//玩家登录未发现

        Error_Login_AlreadyLogin = 4101,                                //玩家已登陆
        Error_Login_NotLogin = 4102,                                    //玩家未登陆

        Error_NAME_IN_USE = 4103,                                       // 角色名已存在

        Error_Not_Login_find = 4104,                                    //玩家登录未发现

        Error_LoginCreateSetNameDB = 4105,                              //设置角色名和唯一id的映射错误

        Error_LoginCreateSetCharDB = 4106,                              //设置角色db错误

        Error_LoginCreateSetPlayerDb = 4107,                            //更新玩家账户db设置错误

        Error_LoginCreatePrepareDataScene  = 4108,                      //创建账号preparedata时Scene出错
        Error_LoginCreatePrepareDataChat  = 4109,                       //创建账号preparedata时Chat出错
        Error_LoginCreatePrepareDataActivity  = 4110,                   //创建账号preparedata时Activity出错
        Error_LoginCreatePrepareDataLogic  = 4111,                      //创建账号preparedata时Logic出错
        Error_LoginCreatePrepareDataRank  = 4112,                       //创建账号preparedata时Rank出错

        Error_PrepareDataLogicCreateDBCreated = 4113,                   //Logicprepare数据库已经创建
        Error_PrepareDataLogicCreateSetDBFail = 4114,                   //Logicprepare数据库创建失败

        Error_PrepareDataSceneCreateDBCreated = 4115,                   //Sceneprepare数据库已经创建
        Error_PrepareDataSceneCreateSetDBFail = 4116,                   //Sceneprepare数据库创建失败

        Error_NAME_Sensitive  = 4120,                                   //名字有屏蔽字

        Error_Login_DBCreate = 4122,                                    //Logic DBCreate错误
        Error_Robot_PostionSame = 4200,                                 //请求目的位置一样

        Error_Item_Use_Fruit_Limit = 4201,                              //使用果实达到等级上限        

        //------王兴 = 5000-----
        Error_TimeOut = 5000,                                           //超时
        Error_GetPlayerDataFromCache_Count = 5001,                      //GetPlayerDataFromCache调用时，返回的个数不对
        Error_Player_Not_On_This_Server = 5002,                         //GetPlayerDataFromCache被调用时，发现本服务器上没有该用户的缓存
        Error_CharacterId_Not_Exist = 5003,                             //角色Id不存在
        Error_PlayerId_Not_Exist = 5004,                                //账号Id不存在
        Error_Character_Data_Not_Exist_In_Memory = 5005,                //调用GetCharacterDataFromCache时，SceneServer发现Character Data没有被缓存
        Error_GM_Id_Not_Exist = 5006,                                   //gm id不存在
        Error_GM_Account_Exist = 5007,                                  //gm账号已经存在
        Error_Create_GM_Account = 5008,                                 //创建gm账号出错
        Error_Modify_GM_Account = 5009,                                 //修改gm账号出错

        Error_NoScene = 5010,                                           //场景id没找到
        Error_SceneNotPublic = 5011,                                    //场景未开放

        Error_FubenNotInOpenTime = 5012,                                //副本未开放
        Error_NoFubenReward = 5013,                                     //没有对应的副本奖励
        Error_FubenRewardNotReceived = 5014,                            //上次的奖励未领取

        Error_CanBuy = 5015,                                            //不能购买该物品

        Error_CharacterNoScene = 5016,                                  //角色没在一个场景中

        Error_PositionUnsync = 5017,                                    //角色的位置与服务器不同步

        Error_NoAccount = 5018,                                         //这个账号不存在

        Error_DungeonNoTime = 5019,                                     //副本时间用完了，用于古战场

        Error_SceneIdNotMatch = 5020,                                   //场景不符
        Error_PositionNotMatch = 5021,                                  //玩家位置不符

        Error_AlreadyInThisDungeon = 5022,                              //已经在该副本内了

        Error_ActivityOver = 5023,                                      //活动已结束

        Error_SweepCouponNotEnough = 5024,                              //扫荡券不足

        Error_FileNotFind = 5025,                                       //没有找到对应文件
        Error_FileNotEnd = 5026,                                        //文件传输尚未完成，用于GM客户端向服务器索取日志文件

        Error_NoVipGift = 5027,                                         //没有可领取的vip礼包
        Error_VipGiftGained = 5028,                                     //vip礼包已领取
        Error_NoMonthCard = 5029,                                       //您没有购买月卡
        Error_MonthCardGained = 5030,                                   //今日月卡奖励已领取

        Error_AlreadyOnThisSever = 5031,                                //已经在目标服务器上了

        //攻城战相关
        Error_NotBidTime = 5032,                                        //现在不能竞标(攻城战竞标ERROR)
        Error_BidThreshold = 5033,                                      //未达到竞标最低价
        Error_AllianceWarQualification = 5034,                          //你的战盟没有参赛资格
        Error_AllianceWarFull = 5035,                                   //参战人数已满
        Error_GuardRespawnExceed = 5036,                                //守卫复活次数已达上限
        Error_OccupantNoNeedBid = 5037,                                 //守城方不必竞标
        Error_AllianceWarCancel = 5038,                                 //本次城战取消
        Error_AllianceWarOver = 5039,                                   //本次城战已结束
        Error_AllianceWarFighting = 5040,                               //城战期间，相关公会禁止人员变动
        Error_GuardNotDie = 5041,                                       //该守卫没死
        Error_NoOccupant = 5042,                                        //荣耀城无人占领
        Error_BannedToPost = 5043,                                      //你已经被禁言了

        //
        Error_EnhanceTooHigh = 5100,                                    //您的装备已经超出强化范围
        Error_GiftCodeInvalid = 5101,                                   //礼品码无效（用过了/不正确）
        Error_GiftCodeExpire = 5102,                                    //礼品码过期
        Error_CantUseGiftCode = 5103,                                   //你不能使用这个礼品码

        //GM工具相关
        Error_CharacterInAlliance = 5200,                               //请让该玩家退出公会，再进行改动
        Error_GMCommandInvalid = 5201,                                  //gm命令无效

        //-------王凯 = 6000-----------
        Error_TimeNotOver =6000,                                        //抽奖时间未到
        Error_NotDrawCount =6001,                                       //无抽奖次数
        Error_AnswerNotTime =6002,                                      //活动不在指定时间内
        Error_ScanCountNotEnough =6003,                                 //扫荡次数不足
        Error_TeamFunctionNotOpen = 6004,                               //组队功能未开启

        //-------lwn = 6105--------------
        Error_NoDungeonShopItems = 6105,                                //副本中没有商店物品
        Error_DungeonShopItemsNotEnough = 6106,                         // 副本商品不足
        Error_ExpNotEnough = 6107,                                      // 经验不足
        Error_AnchorNotInRoom = 6108,                                   // 主播不在
        Error_TiralEnd = 6109,                                          // 试用结束
        Error_UnionBattleEnrollOver = 6110,                             // 盟战报名结束
        Error_UnionBattleNotTime = 6111,                                // 盟战时间未到
        Error_UnionBattleOverTimeEnter = 6112,                          // 盟战进入时间超时了
        Error_AllianceNotEnroll = 6113,                                 // 战盟未报名
        Error_AllianceNotInTime = 6114,                                 // 未到时间
        Error_UnionBattleLessPower = 6115,                              // 盟战报名权限不足
        Error_UnionBattleLowLevel = 6116,                               // 盟战等级不足

        //---------张菲菲 = 7000-----------
        Error_GoodId_Not_Exist = 7000,                                  //商品id没有找到
        Error_RechargeSuccess_ThrowException = 7001,                    //充值过程中出现异常
        Error_LoginThird_CheckTokenTimeOut = 7002,                      //第三方登录验签超时
        Error_loginThird_CheckTokenError = 7003,                        //第三方登录验签异常

        //------------灭世之战 = 8000-------------
        Error_MieShi_NoData = 8000,                                     //没有灭世数据
        Error_MieShi_NoActivity = 8001,                                 //没有该活动
        Error_MieShi_NoBattery = 8002,                                  //没有该炮塔
        Error_MieShi_MaxHP = 8003,                                      //已达到最高可提升血量
        Error_MieShi_MaxSkillLvl = 8004,                                //已达到最高可提升等级
        Error_MieShi_NoApplyTime = 8005,                                //不在报名阶段不可提升炮台
        Error_MieShi_BatteryDestory = 8006,                             //炮台已被摧毁
        Error_MieShi_Config = 8007,                                     //配置错误
        Error_MieShi_CanNotPromote = 8008,                              //不在可提升阶段
        Error_MieShi_NoBossBox = 8009,                                  //该Boss宝箱不存在
        Error_MieShi_BossHadPickUp = 8010,                              //Boss宝箱已拾取
        Error_MieShi_NotCanInTime = 8011,                               //活动未到进入时间
        Error_MieShi_WaitTime = 8012,                                   //正在排队中，请耐心等候
        Error_MieShi_PlayerFull = 8013,                                 //当前人数已满，请排队等候
        Error_MieShi_NotApply = 8014,                                   //玩家没有报名
        Error_MieShi_AlreadyGain = 8015,                                //雕像奖励已领取

        //---------------尹东奇 = 9000-------------------------
        Error_CanNot_Inspire = 9000,                                    //不能继续鼓舞
        Error_NO_Times       = 9001,                                    //次数不足
        Error_Answer_Over    = 9002,									//题目已经答完
        Error_BuyTili_NO_Times = 9003,                                  //灵兽岛体力购买次数不足
		Error_Offline_Item_Use_Failed = 9004,                           //离线挂机物品使用失败
		Error_BuyTeHui_Failed_Time_Error = 9005,                        //开服特惠购买物品 日期不对
        Error_BuyTeHui_Failed_No_Times = 9006,                          //开服特惠购买物品 购买次数不足
		
        //---------------爬塔
        Error_CanNot_Sweep_Limit       = 9100,                                //层数限制不能继续扫荡
		Error_CanNot_Sweep_Buy_Times   = 9101,                                //达到购买上限
		//--------------坐骑
		Error_Unknow_Mount             = 9111,                                //未知的坐骑
		Error_MountSkill_MAX_Level     = 9112,                                //技能级别已达上限
		Error_MountSkill_Limit         = 9113,                                //不满足开启条件
		Error_MountFeed_MAX_Count      = 9114,                                //喂养数量上限
		Error_Mount_Not_Active         = 9115,                                //未激活的坐骑
		Error_Mount_MAX_LEVEL          = 9116,                                //坐骑级别上限
        //--------------符文分解-------------------------------------
        Error_Runr_Not_Resolve = 9200,                                //没有可供分解的该类符文
		Error_Checken_Skill_Point = 9211  ,                           //鸡翅活动技能点不足
		Error_Checken_Skill_Id    = 9212  ,                           //吃鸡活动buffid错误
 		//---------------new team errorcodes = 9300- 9400-------------------------
		Error_SearchApplyListFail_001 = 9300,							// 查找玩家队伍申请列表异常：玩家没有队伍存在
		Error_ChangeTeamTargetFail_001 = 9301,							// 更改队伍目标失败，玩家没有队伍
		Error_ClearApplyListFail_001 = 9302,							// 清除申请列表失败，玩家没有队伍存在

        //--------------|LYX|9600|-------------------------------------
        Error_RebornLevelNotEnough = 9600,  //转生等级不足
		

       //--------------邮件反馈最大次数-------------------------------------
        Error_SendMailNotEnough = 9601,  //邮件反馈最大次数不足

        Error_AllianceDepotIsFull             = 9602,  //战盟仓库已满
        Error_AllianceDepotItemChanged        = 9603,  //战盟仓库道具发生变化
        Error_AllianceDepotDonateWrongQuality = 9604,  //战盟捐赠道具品质错误
        Error_AllianceDepotDonateWrongLadder  = 9605,  //战盟捐赠道具阶数错误
        Error_AllianceDepotDonateWrongBinding = 9606,  //战盟捐赠道具绑定错误
        Error_WrongItemGuildPionts            = 9607,  //战盟清理所得资金配置错误
		Error_Lode_ErrorId                    = 9700,  //错误的ID
		Error_Lode_Hold_ErrorTime             = 9701,  //错误的时间
		Error_Lode_Collect_ErrorTimes         = 9702,  //错误的次数
		Error_Lode_Hold_ErrorAlliance         = 9703,  //错误的公会
        Error_Lode_IsOwnerAlliance            = 9704,  //战旗所属战盟是当前玩家的战盟

        Error_TreasureStoreItemCountNotEnough = 9800,  //珍宝商店物品数量不足
        Error_TreasureStoreBuyFailed          = 9801,  //珍宝商店购买失败

        Error_NoLifeCard                     = 9802,  //还没有购买终生卡
        Error_LifeCardGained                 = 9803,  //今日已领取

        Error_EquipRuneLevelMax = 9804,  		//符文达到无尽幻境已经最大等级

        Error_NoWeekCard = 9805,  //还没有购买周卡
        Error_WeekCardGained = 9806,  //今日已领取

	}
