using System;

#region     //角色属性列表
public enum eAttributeType
{
    Level	=	0	,//等级
	Strength	=	1	,//力量
	Agility	=	2	,		//敏捷
	Intelligence	=	3	,//智力
	Endurance	=	4	,//体力
	PhyPowerMin	=	5	,//物攻最小值
	PhyPowerMax	=	6	,//物攻最大值
	MagPowerMin	=	7	,//魔攻最小值
	MagPowerMax	=	8	,//魔攻最大值
	AddPower	=	9	,//附加伤害
	PhyArmor	=	10	,//物理防御
	MagArmor	=	11	,//魔法防御
	DamageResistance	=	12	,//伤害抵挡
	HpMax	=	13	,//生命上限
	MpMax	=	14	,//魔法上限
	LuckyPro	=	15	,//幸运一击率
	LuckyDamage	=	16	,//幸运一击伤害率
	ExcellentPro	=	17	,//卓越一击率
	ExcellentDamage	=	18	,//卓越一击伤害率
	Hit	=	19	,//命中
	Dodge	=	20	,//闪避
	DamageAddPro	=	21	,//伤害加成率
	DamageResPro	=	22	,//伤害减少率
	DamageReboundPro	=	23	,//伤害反弹率
	IgnoreArmorPro	=	24	,//无视防御率
	MoveSpeed	=	25	,//移动速度
	HitRecovery	=	26	,//击中回复
    FireAttack = 27, // 火属性攻击
    IceAttack = 28, // 冰属性攻击
    PoisonAttack = 29, // 毒属性攻击
    FireResistance = 30, // 火属性抗性
    IceResistance = 31, // 冰属性抗性
    PoisonResistance = 32, // 毒属性抗性

    Count   ,//

    Reserved33 = 33,
    Reserved34,
    Reserved35,
    Reserved36,
    Reserved37,
    Reserved38,
    Reserved39,
    Reserved40,
    Reserved41,
    Reserved42,
    Reserved43,
    Reserved44,
    Reserved45,
    Reserved46,
    Reserved47,
    Reserved48,
    Reserved49,
    Reserved50,

    CountNext = 51,
	HpNow = 51,//生命
	MpNow = 52,//魔法
    AttrCount,
    Attack = 105, // 攻击(跟角色有关)
}
#endregion

#region     //包裹类型
public enum eBagType
{
    Error = -1,                 //错误
    Equip = 0,                  //装备包裹
    BaseItem = 1,               //基础包裹
    Piece = 2,                  //碎片包裹
    Depot = 3,                  //仓库包裹
    Elf = 4,                    //精灵包裹
    Pet = 5,                    //宠物包裹
    MedalBag = 6,				//勋章包裹
    Equip01 = 7,                //头盔
    Equip02 = 8,                //项链
    Mount = 9,                  //坐骑
    Equip04 = 10,               //精灵（暂无）
    Equip05 = 11,               //胸甲
    Wing = 12,               	//翅膀
    Equip07 = 13,               //戒指
    Equip08 = 14,               //手套
    Equip09 = 15,               //裤子
    Equip10 = 16,               //靴子
    Equip11 = 17,               //主手
    Equip12 = 18,               //副手
    MedalUsed = 19,             //（使用的勋章）
    MedalTemp = 20,             //（临时的勋章）       
    FarmDepot = 21,             //农场仓库    
    WishingPool = 22,           //许愿池仓库
    GemBag = 23,             	//宝石仓库
    GemEquip = 24,             	//宝石装备
	MountBag = 25,              //坐骑背包
    EquipShiZhuangBag = 26,     //装备时装背包
    WingShiZhuangBag = 27,      //翅膀时装背包
    WeaponShiZhuangBag = 28,    //武器时装背包
    EquipShiZhuang = 29,     //装备时装
    WingShiZhuang = 30,      //翅膀时装
    WeaponShiZhuang = 31,    //武器时装
}

#endregion

#region     //物品大类型
public enum eItemType
{
    Error = -2,          //错误
    Resources = -1,      //资源
    Equip = 0,           //装备
    BaseItem = 1,        //基础道具
    Piece = 2,           //碎片道具
    Mission = 3,         //任务道具
    Elf = 4,             //精灵
    Pet = 5,             //宠物
    Wing = 6,            //翅膀
    Astrology = 7,       //占星宝石
    Medal = 8,           //勋章
    TreasureMap = 9,     //藏宝图
}
#endregion

//物品具体类型
public enum eItemSpeType
{
    Resources = 0,          //资源
    TreasureMap = 26300,    //藏宝图
}

public enum eEquipType
{
    Begin = 0,
    Helmet = Begin,                //头盔
    EquipType0 = Helmet,
    Necklace = 1,                //项链
    EquipType1 = Necklace,
    Chest = 2,               //胸甲
    EquipType2 = Chest,
    RingL = 3,               //戒指
    EquipType3 = RingL,
    RingR = 4,               //戒指
    EquipType4 = RingR,
    Hand = 5,               //手套
    EquipType5 = Hand,
    Leg = 6,               //裤子
    EquipType6 = Leg,
    Foot = 7,               //靴子
    EquipType7 = Foot,
    WeaponMain = 8,               //主手
    EquipType8 = WeaponMain,
    WeaponScend = 9,               //副手
    EquipType9 = WeaponScend,
    EquipShiZhuang = 10,     //装备时装
    EquipType10 = EquipShiZhuang,
    WingShiZhuang = 11,      //翅膀时装
    EquipType11 = WingShiZhuang,
    WeaponShiZhuang = 12,    //武器时装
    EquipType12 = WeaponShiZhuang,
    Count = WeaponShiZhuang + 1,
}

#region //事件列表
public enum eEventType
{
    Trueflag = 0,           //标记位变化为True     p0=标记位ID
    Falseflag = 1,          //标记位变化为False    p0=标记位ID
    ExDataChange = 2,       //扩展数据变化         p0=扩展计数ID    p1=数量 value
    ItemChange = 3,         //资源变化            P0=资源ID        p1=数量 !=0
    KillMonster = 4,        //怪物击杀            P0=怪物ID         p1=数量 >0
    EnterArea = 5,          //区域事件            P0=区域ID         p1={1进入,0离开}
    Tollgate = 6,           //关卡事件            P0=关卡ID         p1={1胜利,0失败}
    //LevelUp = 3,          //升级                p0=升级后等级
}
#endregion

#region //任务目标
public enum eMissionType
{
    Finish = 0,                     //直接完成
    KillMonster = 1,                //杀怪            	0=怪物ID      	1=怪物数量
    AcceptProgressBar = 2,          //接受时读条     	0=读条时间   	1=读条内容
    AreaProgressBar = 3,            //进区域读条     	0=读条时间  	1=读条内容  	2=区域坐标
    CheckItem = 4,                  //收集道具      	0=物品ID      	1=物品数量
    AcceptStroy = 5,                //接受时播放剧情   	0=剧情ID
    AreaStroy = 6,                  //进区域播放剧情   	0=剧情ID      	1=区域坐标
    Tollgate = 7,                   //通关副本      	0=副本ID      	1=次数
    BuyItem = 8,                    //购买物品  		0=物品ID（-1代表所有）			1=数量	     
    EquipItem = 9,                  //装备物品  		0=装备部位复选（-1代表所有）	
    EnhanceEquip = 10,              //强化装备  		0=装备部位复选（-1代表所有）	1=次数
    AdditionalEquip = 11,           //追加装备  		0=装备部位复选（-1代表所有） 	1=次数
    UpgradeSkill = 12,              //升级技能  		0=技能ID（-1代表所有）  		1=次数
    NpcServe = 13,					//使用NPC服务 		0=服务ID 		1=次数
    Arena = 14,						//参与竞技场		0=次数
    AddFriend = 15,					//添加好友			0=次数
    ComposeItem = 16,				//合成				0=类型(-1代表所有类型) 	1=物品ID(-1代表所有物品) 2=次数
    ExdataAdd = 17,					//相应扩展计数增加	0=扩展计数ID  1=增加目标数值
    ExDataChange = 18,				//相应扩展计数改变	0=扩展计数ID  1=相应值
    LearnSkill = 19,				//学会技能			0=技能ID（-1代表所有） 
    GetSkillTalant = 20,            //获得技能天赋  	0=技能ID（-1代表所有）  		1=次数
    Dungeon = 21,                   //副本              0=要进的副本id				1=次数
    DepotTakeOut = 22,                   //从仓库取出物品    0=物品ID				    1=数量
	FlagTrue  = 23,                   //标记位为真      标记位ID
    ObtainItem  = 24,               //累计获得物品
}
#endregion



//Obj类型
public enum ObjType
{
    INVALID = -1,           //无效
    PLAYER = 0,             //玩家
    NPC = 1,                //NPC和怪
    RETINUE = 2,			//宠物
    AUTOPLAYER = 3,			//自动玩家
    DROPITEM = 100,         //掉落
	DROPBUFF = 101,         //掉落Buff
}

//阵营定义(表格)
public enum CharacterCamp
{
    NOCAMP = -1,            //无阵营
    FRIEND = 0,             //友好
    NEUTRALITY = 1,         //中立
    ENEMY = 2,              //敌方
}

#region 技能
//技能释放类型
public enum SkillType
{
    Immediate = 0,          // 瞬发
    Launch = 1,             // 吟唱技能
    Channel = 2,            // 引导技能
    Passive = 3,            // 被动
    DiffTimeSpan = 4,       // 不规则引导
    FixedPoint = 5,         // 定点引导(目标为目标点)
    Trace = 6,              // 追踪引导
    FixedPointSelf = 7,     // 定点引导(自己为坐标点)
}

//技能目标类型
public enum SkillTargetType
{
    SELF = 0,               //自己
    SINGLE = 1,             //单体
    CIRCLE = 2,             //自己周围的圆形
    SECTOR = 3,             //自己扇形
    RECT = 4,               //矩形
    TARGET_CIRCLE = 5,      //目标圆形
    TARGET_RECT = 6,        //目标矩形
    TARGET_SECTOR = 7,      //目标扇形
}

//技能参数类型
public enum eModifySkillType
{
    Icon = 0,                   //技能图标
    Skill = 1,                  //技能系ID
    NeedHp = 2,                 //生命值消耗
    NeedMp = 3,                 //法力消耗
    NeedAnger = 4,              //怒气消耗
    Cd = 5,                     //技能冷却（毫秒）
    Layer = 6,                  //最大冲能层数
    CommonCd = 7,               //公共冷却时间（毫秒）
    TargetCount = 8,           //目标数量
    ControlType = 9,               //行动类型
    BeforeBuf1 = 10,            //释放前自身获得BUFFID1
    BeforeBuf2 = 11,            //释放前自身获得BUFFID2
    AfterBuff1 = 12,            //释放后自身获得BUFFID1
    AfterBuff2 = 13,            //释放后自身获得BUFFID2
    MainTarget3 = 14,           //所选目标BUFFID3
    MainTarget4 = 15,           //所选目标BUFFID4
    OtherTarget1 = 16,          //其他目标BUFFID1
    OtherTarget2 = 17,          //其他目标BUFFID2
    TargetType = 18,            //目标选择类型
    TargetParam1 = 19,          //参数1
    TargetParam2 = 20,          //参数2
    TargetParam3 = 21,          //参数3
    TargetParam4 = 22,          //参数4
    TargetParam5 = 23,          //参数5
    TargetParam6 = 24,          //参数6
    CastType = 25,              //释放类型
    CastTypeParam1 = 26,          //参数1
    CastTypeParam2 = 27,          //参数2
    CastTypeParam3 = 28,          //参数3
    CastTypeParam4 = 29,          //参数4
    HitType = 30,               //命中类型
    MainTarget1 = 31,           //所选目标BUFFID1
    MainTarget2 = 32,           //所选目标BUFFID2
    BulletId = 33,               //子弹ID
    BroadcastSkillId = 34,       //广播时的ID
}

#endregion


//Scene 数据同步ID
public enum eSceneSyncId
{
    SyncLevel = 0,
    SyncStrength = 1,
    SyncAgility = 2,
    SyncIntelligence = 3,
    SyncEndurance = 4,
    SyncPhyPowerMin = 5,
    SyncPhyPowerMax = 6,
    SyncMagPowerMin = 7,
    SyncMagPowerMax = 8,
    SyncAddPower = 9,
    SyncPhyArmor = 10,
    SyncMagArmor = 11,
    SyncDamageResistance = 12,
    SyncHpMax = 13,
    SyncMpMax = 14,
    SyncLuckyPro = 15,
    SyncLuckyDamage = 16,
    SyncExcellentPro = 17,
    SyncExcellentDamage = 18,
    SyncHit = 19,
    SyncDodge = 20,
    SyncDamageAddPro = 21,
    SyncDamageResPro = 22,
    SyncDamageReboundPro = 23,
    SyncIgnoreArmorPro = 24,
    SyncMoveSpeed = 25,
    SyncHitRecovery = 26,
    SyncFireAttack = 27, // 火属性攻击
    SyncIceAttack = 28, // 冰属性攻击
    SyncPoisonAttack = 29, // 毒属性攻击
    SyncFireResistance = 30, // 火属性抗性
    SyncIceResistance = 31, // 冰属性抗性
    SyncPoisonResistance = 32, // 毒属性抗性

    Count   ,//
    SyncReserved33 = 33,
    SyncReserved34,
    SyncReserved35,
    SyncReserved36,
    SyncReserved37,
    SyncReserved38,
    SyncReserved39,
    SyncReserved40,
    SyncReserved41,
    SyncReserved42,
    SyncReserved43,
    SyncReserved44,
    SyncReserved45,
    SyncReserved46,
    SyncReserved47,
    SyncReserved48,
    SyncReserved49,
    SyncReserved50,
    SyncCountNext = 51,


    SyncHpNow = 51,
    SyncMpNow = 52,

    SyncTitle0 = 53,
    SyncTitle1 = 54,
    SyncTitle2 = 55,
    SyncTitle3 = 56,
    SyncTitle4 = 57,

    SyncFightValue = 58,
    SyncAreaState = 59,
    SyncPkModel = 60,
    SyncPkValue = 61,
    SyncReborn = 62,
    SyncAllianceName = 63,  //战盟名字
    SyncTargetCharacterId = 64,  //战盟名字
    SyncPlayerName=65,      //玩家名字
    SyncMax,
}

public enum eResourcesType
{
	CityWood = -2,   //家园木资源(废弃)
    InvalidRes = -1,
    LevelRes = 0,    //角色等级
    ExpRes = 1,      //角色经验
    GoldRes = 2,     //游戏币
    DiamondRes = 3,  //充值币
    VipExpRes = 4,   //Vip经验
    Spar = 5,        //晶石
    HomeLevel = 6,   //家园等级
    ElfPiece = 7,    //精灵碎片
    MieshiScore = 8, //家园木资源
    AchievementScore = 9,   // 成就点
    MagicDust = 10,  //魔尘
    Honor = 11,  	 //荣誉
    HomeExp = 12,    //家园经验
    Contribution = 13,  //战盟贡献
    PetSoul = 14,    //随从魂魄
    VipLevel = 15,   //Vip等级
    Other16 = 16,    //交易所使用的货币
    DiamondBind = 17,   // 绑钻
	Alchemy = 18,    //炼金币
    CountRes,
}



//任务状态
public enum eMissionState
{
    Unfinished = 0,         //未完成
    Finished = 1,           //已完成
    Failed = 2,             //失败
    Acceptable = 3          //可接
}

//任务类型
public enum eMissionMainType
{
	MainStoryLine = 0,		//主线任务
	SubStoryLine,			//支线任务
	Daily,					//每日任务
	Gang,					//帮派任务
    Farm,                   //农场任务
    Base,                   //母任务
    Circle                  //环任务
}

//奖励类型
public enum eActivationRewardType
{
    TableGift=0,                //gift表里的
    DailyVipGift=1,             //vip每日奖励
    MonthCard=2,                //月卡
    NewServerActive=3,          //开服活动
    LifeCard = 4,               //终生卡
    WeekCard = 5,               //周卡
}

//奖励类型
public enum eRewardType
{
    Invalid = -1,
    GiftBag = 0,                //礼包
    OnlineReward=1,             //在线奖励
    LevelReward=2,              //等级奖励
    ContinuesLoginReward=3,     //连续登录奖励
    MonthCheckinReward=4,       //每月累计签到
    DailyActivity=6,            //每日活跃任务
    DailyActivityReward=7,      //每日活跃奖励
    SevenDayReward=9,      //七天登录奖励
}

//奖励状态
public enum eRewardState
{
    HasGot = 0,             //是否已经获得
    CanGet = 1,             //现在可以获得
    CannotGet = 2,          //现在不能获得
}

//在线奖励参数
public static class OnLineRewardParamterIndx
{
    public const int Minutes = 0;
    public const int ItemId = 1;
    public const int ItemCount = 2;
}

//等级奖励参数
public static class LevelRewardParamterIndx
{
    public const int Level = 0;
    public const int ItemId_1 = 1;
    public const int ItemId_Max = 5;
}

//连续登录奖励参数
public static class ContinuesLoginRewardParamterIndx
{
    public const int Days = 0;
    public const int ItemId = 1;
    public const int ItemCount = 2;
}

//每月签到奖励参数
public static class MonthCheckinRewardParamterIndx
{
    public const int Month = 0;
    public const int Day = 1;
    public const int ItemId = 2;
    public const int ItemCount = 3;
    public const int CostDiamond = 4;
}

//每日活动参数
public static class ActivityParamterIndx
{
    public const int Score = 0;
    public const int NeedTimes = 1;
    public const int UIId = 2;
    public const int ExtDataIdx = 3;
    public const int DescId = 4;
}

//每日活动奖励参数
public static class ActivityRewardParamterIndx
{
    public const int ItemId = 0;
    public const int Count = 1;
    public const int NeedScore = 2;
}

//聊天频道
public enum eChatChannel
{
    System = 0,            		//系统  S[?]C 			各服务器触发
    World = 1,                  //世界	CS[?]C 			逻辑服务器
    City = 2,                   //同城  
    Scene = 3,                  //场景  CS[scene]C     	场景服务器
    Guild = 4,                  //公会  CS[?]C			逻辑服务器
    Team = 5,                   //队伍  CS[chat]C	    组队服务器
    Whisper = 6,  	            //私聊  CS[chat]C		聊天服务器
    Horn = 7,                 	//喇叭
    WishingDraw=8,              //许愿池抽奖
    WishingGroup=9,             //许愿池团购
    Anchor=10,					//主播
    TeamWorld=11,				//世界队伍，跨服
    Count,
    SystemScroll = 98,                 //系统中间滚屏 
    MyWhisper = 99,  	        //自己发送的私聊
    Help = 100,  	        	//特殊提示
}

public enum eChatShowChannel
{
    Total = 0,
    City,
    Guild,
    Team,
    Custom,
    System,
    Horn,
    Wishing,
    SystemScroll,
    TeamWorld,              //世界队伍，跨服
    Count,
}


//聊天频道
public enum eChatParamType
{
    BossCreate,
    BossKilled,
}

//副本类型
public enum eDungeonType
{
    Invalid = -1,
    Main = 0,                   //剧情
    Exp = 1,                    //经验
    Team = 2,                   //组队
    Gold = 3,                   //金币
    Vip = 8                     //vip
}

//副本主要类型
public enum eDungeonMainType
{
    Invalid = -1,
    Fuben,              //副本
    Activity,           //活动
    Vip,                //vip
    Pvp,                //pvp
    ScrollActivity,     //滚动活动
    PhaseFuben,         //相位副本
}

//副本具体（辅助）类型
public enum eDungeonAssistType
{
    Invalid = -1,
    Story,              //剧情副本
    Daily,              //日常副本
    Team,               //组队副本
    Tower,              //通天塔
    DevilSquare,        //恶魔广场
    BloodCastle,        //血色城堡
    WorldBoss,          //世界BOSS
    AncientBattlefield, //古战场
    Vip,                //VIP专属
    Battlefield1,       //火龙窟1阶
    Battlefield2,       //火龙窟2阶
    Battlefield3,       //火龙窟3阶
    Battlefield4,       //火龙窟4阶
    Battlefield5,       //火龙窟5阶
    Pvp1v1,             //1v1竞技场
    FrostBF1,           //寒霜据点1阶
    FrostBF2,           //寒霜据点2阶
    FrostBF3,           //寒霜据点3阶
    FrostBF4,           //寒霜据点4阶
    FrostBF5,           //寒霜据点5阶
    FrozenThrone,       //寒冰王座
    OrganRoom,          //机关房
    PhaseDungeon,       //相位副本
    CityExpMulty,       //家园多人经验活动
    CityGoldSingle,     //家园单人金币
    CityExpSingle,      //家园单人经验
    CastleCraft1,       //古堡1阶
    CastleCraft2,       //古堡2阶
    CastleCraft3,       //古堡3阶
    CastleCraft4,       //古堡4阶
    CastleCraft5,       //古堡5阶
    CastleCraft6,       //古堡6阶
    AllianceWar,        //攻城战
    MieShiWar,          //灭世之战
    KillZone,           //经验岛
	ElfWar,             //灵兽岛
	ClimbingTower,      //爬塔
	Chiji,              //吃鸡
}

//副本计数结算点
public enum eDungeonSettlementNode
{
    None = -1,
    Start,
    End,
}

public enum eDungeonState
{
    WillStart,
    Start,
    ExtraTime,
    WillClose,
    Closing,
    Closed,
}

public enum eActivity
{
    DevilSquare,
    BloodCastle,
    WorldBoss,
}

public enum eActivityState
{
    WaitNext,
    WillStart,
    Start,
    WillEnd,
    End,
}

public enum eMieShiState
{
    WaitNext = 0,   //等待开放
    Open,       //可以报名了
    CanIn,      //可以进入了，副本准备阶段
    Start,      //已经开始了
    WillEnd,    //即将结束，不能进入了
    End,        //结束
}

public enum eNpcType
{
    Common,
    Elite,
    SceneBoss,
    GoldArmy,
    WorldBoss,
    PickUpNpc,
}

public enum eScnenChangeType
{
    Login = 0,              //登录
    Normal = 1,             //普通
    EnterDungeon = 2,       //进入副本(sceneParam:  0:hour 1:min)
    ExitDungeon = 3,        //退出副本
    TeamDungeon = 4,        //队员被动进副本
    Transfer = 5,           //传送门
    EnterCity = 6,          //进入家园
    ExitCity = 7,           //退出家园
    LoginRelive = 8,        //复活登录
    Position = 9,            //指定了位置
    None = 10,            //不处理位置
}

public enum eScnenChangePostion
{
    Db = 0,         //读取数据库
    Former = 1,     //读取数据库副本备份
    Table = 2,      //读取表格配置
    Transfer = 3,   //传送门
    Position = 4,    //目标传送点
    EnterCity = 5, //进家园    
    Login = 6,       //进入游戏
    None = 10,      //不处理位置
    FormerNear = 11,	//相位副本
    CreateCharacter =999,    //创建角色
}

public enum eComposeType
{
    Invalid = -1,
    Handbook = 0,   //图鉴
    Ticket = 1,     //门票
    Rune = 2,       //属性符文
	MountSkill = 3,  //坐骑魂丹
    SkillBook = 4,  //技能书
    SkillPiece = 5,
    MayaShenQi = 6, // 玛雅神器
    Count,
}

public enum eSmithyCastType
{
    Invalid = -1,
    Iron = 0,           //铸造神铁
    Conversion = 1,     //转化
    Refine = 2,         //精炼
}

// 创建，删除OBJ的原因
public enum ReasonType
{
    VisibilityChanged,  //可见性发生变化
    Born,               //出生
    Dead,               //死亡
    Other,              
}

//排行榜类型
public enum RankType
{
    FightValue = 0,         //战斗力
    Level = 1,              //等级
    Money = 2,              //财富
    Arena = 3,              //竞技场
    CityLevel = 4,          //家园等级
    WingsFight = 5,         //翅膀战力
    PetFight = 6,           //精灵战力
    RechargeTotal = 7,      //充值总量
    DailyGift = 8,          //每日送礼
    WeeklyGift = 9,         //每周送礼
    TotalGift = 10,         //送礼总榜
    Mount = 11,             //坐骑
    //RechargeDaily,          //每日充值(目前没用)
    TypeCount,
}

//建筑状态
public enum BuildStateType
{
    Building = 0,       //建造中
    Upgrading = 1,      //升级中
    Using = 2,          //使用中
    Idle = 3,           //休闲中
}

//邮件状态
public enum MailStateType
{
    NewMail = 0,       //新邮件
    OldMail = 1,       //已看的
    Receive = 2,       //已领取
    NewMailHave = 100,       	//新邮件有物品
    OldMailHave = 101,       	//已看的有物品
    NewMailNothing = 200,       //新邮件无物品
    OldMailNothing  = 201,     	//已看的无物品
}

//服务类型
public enum NpcServeType
{
    Shop = 0,           //商店
    Repair = 1,         //修理
    Treat = 2,          //治疗
    Warehouse = 3,      //仓库
    OpenUI = 4,         //打开UI
    EquipShop = 5,      //装备兑换
    HeiShiShop = 6,     // 黑市商人
    TreasureShop = 7,   // 珍宝商人
    TalentShop = 8,     // 天赋商人
} 


//宠物任务类型
public enum PetMissionType
{
    KillMonster = 0,           	//杀怪
    Patrol  = 1,         		//巡逻
    Treasure = 2,          		//寻宝
	Plunder = 3,      			//掠夺
    Recruit = 4,      			//招募
}

//宠物任务状态
public enum PetMissionStateType
{
    New = 0,           			//新增的
    Do  = 1,         			//正在做的
    Idle = 2,          			//空闲的
    Finish = 3,          		//完成的
    Delete = 4,          		//删除
    Finish2 = 5,                //点完完成任务等待可领取奖励，也可以开始
}

//宠物状态
public enum PetStateType
{
    Piece = 0,          		//碎片
    NoEmploy  = 1,				//未雇佣
    Idle = 2,          			//已雇佣，目前空闲的
    Mission = 3,          		//正在做任务
    Building = 4,          		//正在建筑中
    Hatch = 5,          		//进阶孵化
}

public enum BuildingType
{
	BaseCamp = 0,				//议事厅
	Mine =1,					//矿洞
	Farm=2,						//农场
    DemonCave=3,				//占星台
    DemonCloister=4,			//恶魔回廊
    MercenaryCamp=5,			//佣兵营地
    ArenaTemple=6,				//角斗圣殿
    WarHall=7,				    //战争大厅
    BlacksmithShop=8,			//铁匠屋
    Exchange=9,					//交易所
    CompositeHouse=10,			//合成屋
    WishingPool=11,				//许愿池
    BraveHarbor=12,				//勇士港
    LogPlace = 13,				//伐木场
    Broken=98,					//破损的
    Debris=99,					//杂物
    Space=100,					//空地
}

//家园操作类型
public enum CityOperationType
{
    BUILD                               = 0,  //建造
    UPGRADE                             = 1,  //升级
    DESTROY                             = 2,  //销毁
    ASSIGNPET                           = 3,  //指派随从
    ASSIGNPETINDEX                      = 4,  //指派制定位置随从
    SPEEDUP                             = 5,  //加速
}

public static class PetItemExtDataIdx
{
    public const int Level = 0;
    public const int Exp = 1;
    public const int StarLevel = 2;
    public const int State = 3;
    public const int FragmentNum = 4;
    public const int SpecialSkill_Begin = 5;
    public const int SpecialSkill_Num = 3;
    public const int SpecialSkill_End = SpecialSkill_Begin+SpecialSkill_Num;
}

//家园操作类型
public enum PetOperationType
{
    EMPLOY                              = 0,  //雇佣
    FIRE                                = 1,  //解雇
    RECYCLESOUL                         = 2,  //回收魂魄
}

//随从任务操作
public enum PetMissionOpt
{
    START                               = 0,    //开始
    COMPLETE                            = 1,    //完成
    COMMIT                              = 2,    //提交
    DELETE                              = 3,    //删除
    BUYTIMES                            = 4,    //购买次数
}

//服务器状态
public enum ServerStateType
{
    Prepare = -1,               //准备
	Repair = 0,					//维护中
	Fine  = 1,					//空闲
	Busy = 2,					//繁忙
	Crowded = 3,				//拥挤
    Full = 4,                   //爆满
}


public enum eExdataDefine
{
    e0 = 0,//实现普攻连击规则计数
    e1 = 1,//完成剧情任务总数
    e2 = 2,//完成日常任务总数
    e3 = 3,//查看新邮件总数
    e4 = 4,//日常活跃度
    e5 = 5,//角色力量升级分配总点数
    e6 = 6,//角色敏捷升级分配总点数
    e7 = 7,//角色智力升级分配总点数
    e8 = 8,//角色体力升级分配总点数
    e9 = 9,//角色力量吃符附加点数
    e10 = 10,//角色敏捷吃符附加点数
    e11 = 11,//角色智力吃符附加点数
    e12 = 12,//角色体力吃符附加点数
    e13 = 13,//角色战功值
    e14 = 14,//角色荣誉值
    e15 = 15,//每日活跃度
    e16 = 16,//月度累计奖励计数
    e17 = 17,//连续登陆天数
    e18 = 18,//累计签到补签次数
    e19 = 19,//每日商城消费计数
    e20 = 20,//每日活跃完成1次日常任务
    e21 = 21,//每日通关普通副本次数
    e22 = 22,//每日通关精英副本次数
    e23 = 23,//每日通关炼狱副本次数
    e24 = 24,//每日参与血色城堡次数
    e25 = 25,//每日参与恶魔广场次数
    e26 = 26,//每日参与竞技场次数
    e27 = 27,//每日参与世界BOSS次数
    e28 = 28,//每日强化装备次数
    e29 = 29,//每日追加装备次数
    e30 = 30,//每日击杀怪物总个数
    e31 = 31,//每日在线时间
    e32 = 32,//每日使用炼金次数
    e33 = 33,//每日进行精英挑战次数
    e34 = 34,//每日强化随从次数
    e35 = 35,//每日强化宠物次数
    e36 = 36,//每日参与组队副本次数
    e37 = 37,//每日进行抽奖次数
    e38 = 38,//每日进行战盟捐赠次数
    e39 = 39,//每日进行家园建设次数
    e40 = 40,//每日拜访好友家园次数
    e41 = 41,//每日添加好友次数
    e42 = 42,//每日通关副本次数
    e43 = 43,//总强化次数
    e44 = 44,//总添加好友次数
    e45 = 45,//总杀怪只数
    e46 = 46,//级别数
    e47 = 47,//总强化成功次数
    e48 = 48,//累计金币数
    e49 = 49,//总竞技场获胜数
    e50 = 50,//总成就点数
    e51 = 51,//转生次数
    e52 = 52,//可分配属性点数
    e53 = 53,//总普通副本次数
    e54 = 54,//总精英副本次数
    e55 = 55,//总炼狱副本次数
    e56 = 56,//总所有副本次数
    e57 = 57,//总追加次数
    e58 = 58,//击杀小哥布林总数成就用
    e59 = 59,//挂机记录生命值吃药
    e60 = 60,//挂机记录法力值吃药
    e61 = 61,//挂机范围
    e62 = 62,//任务总等级经验值
    e63 = 63,//任务总等级
    e64 = 64,//杀怪任务专精经验值
    e65 = 65,//杀怪任务专精等级
    e66 = 66,//巡逻任务专精经验值
    e67 = 67,//巡逻任务专精等级
    e68 = 68,//寻宝任务专精经验值
    e69 = 69,//总共充值次数！！！！（原来的寻宝任务专精等级已经废弃）
    e70 = 70,//家园随从最高等级
    e71 = 71,//勇士港每日扫荡海里
    e72 = 72,//战盟城战城主每日膜拜次数
    e78_TotalRechargeDiamond = 78,//充值总量
    e82 = 82,//精灵阵法等级   
    e83 = 83,//累计消费金币数  
    e84 = 84,//累计消费钻石数  
    e85 = 85,//强化失败总次数  
    e86 = 86,//已分配天赋总点数 
    e87 = 87,//拥有精灵总个数  
    e88 = 88,//激活怪物图鉴总个数
    e89 = 89,//升级技能总次数  
    e90 = 90,//孵化随从蛋总次数 
    e91 = 91,//已分配修炼点数  
    e92 = 92,//免费抽宠物蛋次数
    e93 = 93,//竞技场历史最高排名
    e94 = 94,//累计登陆总天数  
    e95 = 95,//累计获得钻石数量 
    e96 = 96,//农场商店每日限购1次
    e97 = 97,//农场商店每日限购5次
    e98 = 98,//玩家当日可挑战次数
    e99 = 99,//玩家当日可购买挑战次数
    e250 = 250,//军衔值
    e251 = 251,//许愿池每日免费次数
    e252 = 252,//建造建筑完成总次数
    e253 = 253,//升级建筑完成总次数
    e254 = 254,//拆除杂物次数
    e255 = 255,//拜访好友家园次数
    e256 = 256,//家园被拜访次数
    e257 = 257,//抵御怪物入侵次数
    e258 = 258,//采集石料总数
    e259 = 259,//采集木料总数
    e260 = 260,//种植作物次数
    e261 = 261,//施肥次数
    e262 = 262,//收获作物次数
    e263 = 263,//铲除作物次数
    e264 = 264,//购买种子次数
    e265 = 265,//购买肥料次数
    e266 = 266,//孵化室单抽次数
    e267 = 267,//孵化室稀有单抽次数
    e268 = 268,//随从进阶完成次数
    e269 = 269,//随从碎片合成次数
    e270 = 270,//许愿池单抽次数
    e271 = 271,//许愿池团购次数
    e272 = 272,//许愿池团购中奖次数
    e273 = 273,//随从任务完成次数
    e274 = 274,//杀怪任务完成次数
    e275 = 275,//巡逻任务完成次数
    e276 = 276,//寻宝任务完成次数
    e277 = 277,//掠夺任务完成次数
    e278 = 278,//重置天赋次数
    e279 = 279,//使用技能书次数
    e282 =282,//玩家战盟ID
    e283 = 283,//玩家在战盟中获得的总功绩
    e284 = 284,//玩家每日捐献道具获得功绩
    e285 = 285,//玩家每日捐献资金或钻石次数
    e286 = 286,//玩家已申请的战盟ID1
    e287 = 287,//玩家已申请的战盟ID2
    e288 = 288,//玩家已申请的战盟ID3
    e289 = 289,//玩家每日挑战Boss1次数
    e290 = 290,//玩家每日挑战Boss2次数
    e291 = 291,//玩家每日挑战Boss3次数
    e292 = 292,//玩家每日挑战Boss4次数
    e293 = 293,//玩家每日挑战Boss5次数
    e294 = 294,//玩家每日挑战Boss6次数
    e295 = 295,//玩家每日挑战Boss7次数
    e296 = 296,//玩家每日挑战Boss8次数
    e297 = 297,//玩家每日挑战Boss9次数
    e298 = 298,//玩家每日挑战Boss10次数
    e299 = 299,//今日血色次数
    e300 = 300,//今日恶魔次数
    e301 = 301,//总计血色次数
    e302 = 302,//总计恶魔次数
    e303 = 303,//装备洗炼总次数
    e304 = 304,//购买包裹格数
    e305 = 305,//开启包裹格数
    e306 = 306,//装备随灵总次数
    e307 = 307,//装备传承总次数
    e308 = 308,//翅膀总阶数   
    e312 = 312,//玩家每日可崇拜次数
    e326 = 326,//翅膀培养次数
    e327 = 327,//精灵分解次数
    e328 = 328,//精灵最大等级
    e329 = 329,//战盟总捐献次数
    e330 = 330,//随从数量
    e331 = 331,//随从总等级
    e332 = 332,//技能总数
    e333 = 333,//道具包裹开启格数
    e334 = 334,//装备包裹开启格数
    e335 = 335,//完成农场任务数量
    e336 = 336,//神像总等级
    e337 = 337,//装备进阶总次数
    e338 = 338,//交易所道具上架次数
    e339 = 339,//交易所道具购买次数    
    e340 = 340,//金币占星每日次数
    e341 = 341,//矿石占星每日次数
    e342 = 342,//道具合成次数
    e343 = 343,//众筹总购买签数
    e344 = 344,//勇士出航总数
    e345 = 345,//加速航行次数
    e346 = 346,//勋章最大等级
    e347 = 347,//图鉴组合激活齐的数量
    e400 = 400,//神像修理次数
    e408 = 408,//恶魔广场，奖励未领取标志位：上次的通关时间(秒)，如果为0，说明领过；如果小于0，说明是中途退出副本的
    e409 = 409,//血色城堡，奖励未领取标志位：上次的通关时间(秒)，如果为0，说明领过；如果小于0，说明是中途退出副本的
    e410 = 410,//精灵抽奖次数
    e411 = 411,//精灵每日免费
    e412 = 412,//每日完成农场订单次数
    e413 = 413,//每日收获农场作物次数
    e414 = 414,//每日完成随从任务次数
    e415 = 415,//每日合成道具次数
    e416 = 416,//每日勇士港出海次数
    // e417 = 417,//每日战盟捐物资获得功绩
    e418 = 418,//每日交易所摆摊次数
    e419 = 419,//每日翅膀培养次数
    e420 = 420,//每日排行榜点赞次数
    e421 = 421,//副本未完成标记
    e428 = 428,//每日火龙窟游戏时间exdata id
    e435 = 435,//用来记录每天古战场获得的经验
	e443 = 443,//日常任务计数
    e479 = 479,//许愿池抽奖次数 
    e530 = 530,//每日完成战场的次数
    e545 = 545,//每日火龙窟完成次数
    e556 = 556,//合成屋记录给经验的次数
	e561 = 561,//累计充值金额
    e563 = 563,//连续充值天数
    e564 = 564,//昨日充钻数
    e590 = 590, //是否已经领取首冲奖励
    e591 = 591, //每日血色累计单倍经验
    e592 = 592, //每日恶魔累计单倍经验
    e593 = 593, //每日血色领取消耗钻石
    e594 = 594, //每日恶魔领取消耗钻石
    e595 = 595, //每日送花榜
    e596 = 596, //每周送花榜
    e597 = 597, //总送花榜
    e598 = 598, //个人累计送花数 
	e621 = 621, //无尽幻境历史最好成绩
	e622 = 622, //无尽幻境可扫荡次数
	e623 = 623, //无尽幻境当前层数
	e624 = 624, //无尽幻境已经购买扫荡次数
    e625 = 625, //累计充值现金（人民币等）数量
    e626 = 626, //累积收到的鲜花榜
    e628 = 628, //挂机设置自动反击
	e630 = 630, //精灵岛每日重置体力
	e631 = 631, //精灵岛体力购买数
    e632 = 632, //古域战场每日重置体力
    e633 = 633, //古域战场体力购买数
    e648 = 648, //副本标记信息
    e652 = 652, //每次充值金额（不累计）
    e653 = 653, //每日累计充值数
    e654 = 654, //每月累计充值数
    e666 = 666, //副本当前鼓舞次数
	e672 = 672, //免费的灭世升级塔次数
	e678 = 678, //未领取奖励的离线时间
	e679 = 679, //翅膀段数
    e680 = 680, //玩家每天采矿次数
    e686 = 686, //历史最大成就点数 
    e688 = 688, //星级
	e700 = 700, //免费的传送次数
	e701 = 701, //今天已经免费传送的次数
	e702 = 702, //VIP已经传送的次数
    e703 = 703, //使用飞鞋券传送的次数
    e714 = 714, //狩猎任务随机种子
	e715 = 715, //战盟日常任务计数
	e716 = 716, //日常任务随机数
	e717 = 717, //战盟日常随机数
    e719 = 719, //狩猎任务计数
    e710 = 710, //当前进行的玛雅纪元的id
    e711 = 711, //需要展示的玛雅纪元ID
	e720 = 720, //装备强化级别
	e721 = 721, //装备强化级别
	e722 = 722, //装备强化级别
	e723 = 723, //装备强化级别
	e724 = 724, //装备强化级别
	e725 = 725, //装备强化级别
	e726 = 726, //装备强化级别
	e727 = 727, //装备强化级别
	e728 = 728, //装备强化级别
	e729 = 729, //装备强化级别
	e730 = 730, //装备精炼
	e731 = 731, //装备精炼
	e732 = 732, //装备精炼
	e733 = 733, //装备精炼
	e734 = 734, //装备精炼
	e735 = 735, //装备精炼
	e736 = 736, //装备精炼
	e737 = 737, //装备精炼
	e738 = 738, //装备精炼
	e739 = 739, //装备精炼
	e740 = 740, //坐骑进阶次数
	e742 = 742, //挂机奖励时间当前值
    e750 = 750, //邮件每日反馈次数
    e752 = 752, //黑市商人每日刷新
    e753 = 753, //玩家超级vip显示情况
    e770 = 770, //蛮荒孤岛每天击杀数
	e778 = 778, //月卡领取次数
    e779 = 779, //周卡领取次数
    e799 = 799, //每日参加爬塔次数
}

public enum eAcitivityFuben
{
    DevilSquare,    //恶魔广场
    BloodCastle,    //血色城堡
}

public enum eAreaState
{
    Wild = 0,
    City = 1,
}

public enum eBagItemType
{
    UnLock = 0,
    Lock = 1,
    AutoUnLock = 2,
    FightUp = 3,
    FightDown = 4,
    No = 5,
    NoFightUp = 6,
    NoFightDown = 7,
    FreeLock = 8,
    TrialEnd = 9,
}

public enum eEquipLimit
{
    OK = 0,
    Occupation = 1,
    Attribute = 2,
    Level = 3,
    Reborn = 4,
}

public enum eEquipBtnShow
{
    BagPack = 0,          // Operate  Equip   Recyle   Sell  Share
    EquipPack = 1,        // Operate  Share
    OperateBag = 2,       // Equip   Share
    Share = 3,            // Share
    None = 4,             // 
    Input = 5,
    Donate = 6,            //战盟仓库捐赠
    TakeOut = 7,           //战盟仓库取出
    ChiefOperate = 8       //战盟盟主操作
}


public enum EquipExdataDefine
{
    SkillLevel = 26,
    BuffId,
    RandBuffId,
    TrialTime,  // 试用时间
    TrialEndTime,   // 试用结束时间（1970到现在的秒）
    Count
}

public enum ElfExdataDefine
{
    StarLevel = 14,         // 星级
    BuffId1 = 15,           // 攻击
    BuffLevel1 = 16,
    BuffId2 = 17,           // 防御
    BuffLevel2 = 18,
    BuffId3 = 19,           // 辅助
    BuffLevel3 = 20,
    Count
}

public enum eGmPriority
{
    Admin = 0,              //admin
    MidLevel = 1,           //中层管理员
    Normal = 2,             //普通GM
    Log = 3,                //只能查log的GM
}

public enum battleAccess
{
    People0=0,
    People1=1,
    AssistantChief=2,
    Chief=3,
}

public enum eSceneType
{
    Normal = 0,
    City = 1,
    Fuben = 2,
    Pvp = 3,
    Home = 4,
    BossHome = 6,
}

public enum ePkModel
{
    Peace = 0,          //和平
    Team = 1,        	//组队
    Alliance = 2,       //战盟
    GoodEvil = 3,       //善恶
}

public enum CityMissionState
{
    Wait = 0,       //等待刷新
    Normal = 1,     //未完成
    Lock = 2,       //未开启
}

public enum StoreItemType
{
    Normal = 0,       //等待购买
    Buyed = 1,     		//已被买
    Free = 2,     		//闲置的空格
}

public enum NpcService
{
    NsNone = -1,
    NsShop = 0,  // 商店
    NsRepair = 1,  // 修理
    NsDoctor = 2,  // 治疗
    NsStoreHouse = 3,  // 仓库
    NsCompose = 4,  // 合成
    NsExchange = 5,  // 装备兑换
    NsBlackMarket = 6,  // 黑市商人
    NsTreasureShop = 7, // 珍宝商人
}

//离开组队匹配的原因
public enum eLeaveMatchingType
{
    Unknow = -1,    //位置错误
    TimeOut = 0,    //超时未响应(超时排队确认时间)  
    TeamOther = 1,  //队伍其他人取消(排队有玩家未确认)
    Refuse = 2,     //拒绝了该匹配(我自己拒绝)
    TeamRefuse = 3, //队伍其他人拒绝
    Cannel = 4,     //取消了排队 
    TeamCannel = 5,     //队伍其他人取消
    TeamChange = 8,     //队伍发生改变
    InTemp = 10,     //临时进队
    Onlost = 11,     //掉线了
    LeaderLeave = 12,    //队长离队
    LeaderLost = 13,     //队长掉线
    MemberLeave = 14,    //队员离队
    MemberLost = 15,     //队员掉线
    PushCannel = 16,     //因为重排，取消了之前的排队
    MatchingBackCannel = 17,     //因为拒绝进入，取消了之前的排队
    MoveDown = 18,               //因为等待时间太长，把这个队列的人都降级
    OtherRefuse = 40,   //有其他人拒绝，进入排队首
    SceneOver = 50,   //场景已结束，进入排队首
    Success = 99,    //排队成功     
}

public enum eChatLinkType
{
    Text        = 0,
    Face        = 1,       //表情
    Equip       = 2,      //装备
    Dictionary  = 3, //字典
    Postion     = 4,    //位置
    Character   = 5,  //角色
    Voice       = 6,  //语音
	Team        = 7,  //组队
	NewTeam     = 8,
}

public enum eCountdownType
{
    BattleFight = 0,
    BattleRelive = 1,
}

public enum eSpeMonsterType
{
    SceneBoss,
    GoldArmy,
    Elite,
    WorldBoss,
}

public enum eSpeMonsterRefreshType
{
    OnTime,         //到点刷新
    AfterDie,       //死后刷新
}

public enum eDungeonCompleteType
{
    Success,
    Failed,
    Quit,
}

public enum eFubenInfoType
{
    KillMonster,        //杀怪信息
    Percent,            //完成度百分比
    Score,              //积分信息
    PlayerCount,        //玩家数
    BattleFieldScore,   //寒霜据点战场积分
    StrongpointInfo,    //据点信息
    ShowDictionary0,	//字典0参数
    ShowDictionary1,	//字典1参数
    ShowDictionary2,	//字典2参数
    ShowDictionary3,	//字典3参数
    Timer,				//倒计时
    AllianceWarInfo,    //攻城战信息
    AllianceWarState,   //攻城战状态
    ShowDictionary6,    //字典6参数
    Timer2,             //倒计时2
}

public enum eFubenPhaseOpType
{
    None = -1,      //啥也不做
    PlayAnimation,  //播放动画
    PlayBgMusic,    //播放背景音乐
}

public enum Exdata64TimeType
{
    P1vP1CoolDown = 0,              //下次冷却时间
    FreeWishingTime = 1,            //下次免费许愿的时间
    FirstOnlineTime = 2,            //今天的第一次登陆时间
    AstrologyMoneyTime= 3,          //金币抽奖间隔时间
    AstrologyResTime = 4,           //资源抽奖间隔时间
    StatueCdTime = 5,               //神像打扫的冷却时间
    MonthCardExpirationDate = 6,    //充值月卡到期时间
    LastOutlineTime = 7,            //最后下线的时间
    CreateTime = 8,                 //创建时间
    LastOnlineTime = 9,             //最后上线的时间
    LastRechargeTime = 10,          //最后一次充值的时间
    ServerStartTime = 11,          //开服时间
	LastTowerCheckTime = 12,        //最后一次爬塔数据验证时间
	LastGetOfflineExpTime = 13,	//最后一次领取离线奖励时间
    FreeElfTime = 14,            //下次免费灵兽抽奖的时间
    LifeCardExpirationDate = 15, //终生卡到期时间
	MonthCardStartDate = 16,     //月卡开始时间 //  未生效
    WeekCardStartDate = 17,     //周卡开始时间
    WeekCardExpirationDate = 18, //周卡到期时间
}


public enum eSceneNotifyType
{
    Dictionary,     //词典    
    DictionaryWrap, //带参数的词典
    Die,            //死亡
}

public enum eLogicNotifyType
{
    Dictionary,     //词典    
    BagFull,        //背包太满
}

//团购商品的状态
public enum eGroupShopItemState
{
    OnSell,         //正在出售
    WaitResult,     //等待揭晓结果
    Sold,           //已售出
    Abandon,        //没人买，被废弃了
}

//团购商品的状态
public enum eQueueType
{
    Dungeon,         //普通副本
    BattleField,     //战场
    ActivityDungeon, //活动副本
}


//进入游戏，申请玩家数据的类型
public enum eLoginApplyType
{
    Bag = 0,
    Skill,
    Mission,
    Flag,
    Exdata,
    Exdata64,
    Mail,
    Book,
    Quene,
    City,
	Mount,
    Trade,

}


public enum eItemInfoType
{
    Item = 0,
    Equip = 1,
    Wing = 2,
    Elf = 3,
}

//TriggerArea Type
public enum eTriggerAreaType
{
    None = -1,      //无
    Trap,           //陷阱
    OpenDoor,       //开门
    Strongpoint,    //据点
}

//据点状态，寒霜据点 战场使用
public enum eStrongpointState
{
    Idle,           //空闲
    Occupying,      //占据中
    OccupyWait,     //占据到一半，停下来了(进度条停止，但光圈还显示)
    Occupied,       //被占据了
    Contending,     //争夺中
}

//副本信息类型
public enum eDungeonInfoType
{
    PlayCount,      //次数
    RestTime,       //剩余时间
    AnswerCount,    //答题数
    QuestionTime,   //头脑风暴的时间
    Count,          //数量
}

//活动副本类型
public enum eDynamicActivityType
{
    Dungon,         //副本类
    Question,       //头脑风暴
	Tower,          //爬塔类
}

 public enum  NoticeState
    {
        WishFreeDraw = 0,   //许愿池抽奖
        ElfFreeDraw = 1,   //精灵抽奖
        RewardLevel = 2,   //福利——等级奖励 可领取奖励
        RewardTime = 3,   //福利——在线时长
        RewardContinuity = 4,   //福利——连续登陆
        RewardSign = 5,   //福利——累计签到
        RewardCompensate = 6,   //福利——每日补偿
        EmailNoRead = 7,   //有未读邮件
        Achievement = 8,   // 成就 
        HatchFinish = 9,   //蛋完成
        PetTaskFinish = 10,   //随从任务完成
        FarmGain = 11,   //农场作物成熟
        MineGain = 12,   //矿物累计40%未收取
        WoodGain = 13,   //木材累计40%未收取
        SailArrive = 14,   //勇士港出海  已到达
        RankClickLike = 15,   //可点赞
        TaskFinish = 16,   //任务完成
        Count,
    }

//PlayFrame 活动的具体类型
public enum eActivityType
{
    DailyTask,                  //日常任务
    StoryDungeon,               //剧情副本
    Arena,                      //竞技场
    NormalCount,                //正常计次数
    Infinity,                   //无限次
    AcientBF,                   //古战场
    Question,                   //答题
    VipAddCount,                //vip可加次数
    IgnoreCount,                //不计次数
    VipDailyGift,               //vip每日礼包
    MonthCard,                  //月卡
}

//服务器状态
public enum QueueType
{
    Login,                      //登录排队
    EnterGame,                  //进入游戏排队
}

//竞标类型
public enum eBidType
{
    AllianceWar,                //攻城战
}

//战盟成员级别
public enum eAllianceLadder
{
    Member,                    //成员
    Elder,                     //长老
    ViceChairman,              //副会长
    Chairman,                  //会长
}

//攻城战状态
public enum eAllianceWarState
{
    WaitBid,                    //等待竞标
    Bid,                        //竞标阶段
    WaitEnter,                  //等待进入战场
    WaitStart,                  //等待战斗开始
    Fight,                      //战斗开始
    ExtraTime,                  //加时赛
}

//充值活动类型
public enum eReChargeRewardType
{
    Notice,                    //公告
    Recharge,                  //累计充值
    Investment,                //累计投资
    FirstRecharge,             //首充
    DaoHang,                   //导航
}

//充值活动类型
public enum eNewServerActivityType
{
    Level,                      //冲级活动
    FightPoint,                 //战力排名
    RechargeDaily,              //充值榜
    Wing,                       //翅膀返利
    Count
}

//Login踢人的类型
public enum KickClientType
{
    OtherLogin = 0, //有其他人登陆
    ChangeServer = 1,//转服时，发现角色在线，但是账号不在线，需要清除角色
    ChangeServerOK = 2,//转服成功，此角色在线，需要踢出
    BacktoLogin = 3, //返回登录
    GmKick  = 4, //Gm踢下线
    LostLine = 5, //掉线
    LoginTimeOut = 6, //登陆时间超时
}

//NotifyTableChange的标记位
public enum eNotifyTableChangeFlag
{
    RechargeTables = 1,         //充值活动相关的一系列表
    Count
}

//充值活动表的OpenRule
public enum eRechargeActivityOpenRule
{
    None = -1,
    Last,
    LimitTime,
    NewServerAuto,
}

//gm命令类型
public enum eGmCommandType
{
    GMLocal,
    GMLogic,
    GMScene,
    GMChat,
    GMRank,
    GMTeam,
    GMAll,
}


public enum eWingExDefine
{
    eGrowValue = 0,    // 成长值
    eGrowMax = 10,      // 1-10 已经占用了
    eReserved11 = 11,   // 保留
    eReserved12 = 12,   // 保留
    eReserved13 = 13,   // 保留
    eReserved14 = 14,   // 保留
    eReserved15 = 15,   // 保留
    eGrowProperty = 16, // 成长属性
    // Warnning： >= 16 为成长属性，个数不定，尽量不要用，使用保留
}

public enum OperationActivityType
{
    Invalid = -1,
    Guide = 0,              //引导
    Recharge = 1,           //充值活动
    SpecialEvent = 2,       //特殊事件
    Investment = 3,         //投资
    Rank = 4,               //排行榜
    Lottery = 5,            //抽奖
}

public enum TimeRule
{
    Invalid = -1,       
    Foreaver = 0,        //永久
    ExactTime = 1,      //准确时间
    ServerOpenTime = 2,  //开服相对时间
}

public enum OperationActivityUIType
{
    Invalid = -1,
    Guide = 0,
    Normal,
    Charge,
    Table,
    Rank,
    Discount,
    ShowModel,
    Horizontal,
    Lottery,
    Holiday,
}

public enum eFakeCharacterTypeDefine
{
    MieShiFakeCharacterType = 1,    //灭世雕像
}

public enum eTitleType
{
	Strength=1,   //强化
    Rank=2,   //军衔
    Level=3,   //等级    
    Wings=4,   //翅膀	
	WingCulture = 5, //翅膀培养	
    Beast=6,   //灵兽
    Skill=7,  //技能
    Collect = 8,   //玛雅技能书
	EquipFashion = 11,   //装备时装
	WeaponFashion = 12,   //武器时装
	WingFashion = 13,   //翅膀时装
    Count,
}

public enum EraActiveType
{
    Mission = 0,
    Condition,
	Achievement
}

public enum eSnapShotResult
{
    Success = 0,
    SnapShotExist = 1,
    SnapShotNotExist = 2,
    NeedReload = 3,
}
public enum Star
{
    Star0 = 0,
    Star1 = 1,
    Star2 = 2,
    Star3 = 3,
    Star4 = 4,
    Star5 = 5,
}
public static class eSkillTargetObjType
{
    public const int Invalid = 0x00;    //无
    public const int Player = 0x01;     //玩家
    public const int Monster = 0x02;    //怪
    public const int Boss = 0x04;        //Boss
    public const int Retinue = 0x08;    //召唤物

    public const int SkillTargetObjType_All = Player|Monster|Boss;
}
//邮件类型
public enum SendToCharacterMailType
{
    Normal = 0,            //正常邮件
    RechargeRet = 1,       //充值返还
    BeKillInfo = 2,       //被人击杀
}

