//----------------------------------------------
//--------------以下需要自动生成代码-------------
//----------------------------------------------
using NLog;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using EventSystem;
namespace DataTable
{
    public static class Table
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static Dictionary<int, ConditionTableRecord> ConditionTable = new Dictionary<int, ConditionTableRecord>();
        private static Dictionary<int, FlagRecord> Flag = new Dictionary<int, FlagRecord>();
        private static Dictionary<int, ExdataRecord> Exdata = new Dictionary<int, ExdataRecord>();
        private static Dictionary<int, DictionaryRecord> Dictionary = new Dictionary<int, DictionaryRecord>();
        private static Dictionary<int, BagBaseRecord> BagBase = new Dictionary<int, BagBaseRecord>();
        private static Dictionary<int, ItemBaseRecord> ItemBase = new Dictionary<int, ItemBaseRecord>();
        private static Dictionary<int, ItemTypeRecord> ItemType = new Dictionary<int, ItemTypeRecord>();
        private static Dictionary<int, ColorBaseRecord> ColorBase = new Dictionary<int, ColorBaseRecord>();
        private static Dictionary<int, BuffRecord> Buff = new Dictionary<int, BuffRecord>();
        private static Dictionary<int, MissionRecord> Mission = new Dictionary<int, MissionRecord>();
        private static Dictionary<int, CharacterBaseRecord> CharacterBase = new Dictionary<int, CharacterBaseRecord>();
        private static Dictionary<int, ActorRecord> Actor = new Dictionary<int, ActorRecord>();
        private static Dictionary<int, AttrRefRecord> AttrRef = new Dictionary<int, AttrRefRecord>();
        private static Dictionary<int, EquipRecord> Equip = new Dictionary<int, EquipRecord>();
        private static Dictionary<int, EquipRelateRecord> EquipRelate = new Dictionary<int, EquipRelateRecord>();
        private static Dictionary<int, EquipEnchantRecord> EquipEnchant = new Dictionary<int, EquipEnchantRecord>();
        private static Dictionary<int, EquipEnchantChanceRecord> EquipEnchantChance = new Dictionary<int, EquipEnchantChanceRecord>();
        private static Dictionary<int, TitleRecord> Title = new Dictionary<int, TitleRecord>();
        private static Dictionary<int, EquipEnchanceRecord> EquipEnchance = new Dictionary<int, EquipEnchanceRecord>();
        private static Dictionary<int, LevelDataRecord> LevelData = new Dictionary<int, LevelDataRecord>();
        private static Dictionary<int, SceneNpcRecord> SceneNpc = new Dictionary<int, SceneNpcRecord>();
        private static Dictionary<int, SkillRecord> Skill = new Dictionary<int, SkillRecord>();
        private static Dictionary<int, BulletRecord> Bullet = new Dictionary<int, BulletRecord>();
        private static Dictionary<int, SceneRecord> Scene = new Dictionary<int, SceneRecord>();
        private static Dictionary<int, TalentRecord> Talent = new Dictionary<int, TalentRecord>();
        private static Dictionary<int, NpcBaseRecord> NpcBase = new Dictionary<int, NpcBaseRecord>();
        private static Dictionary<int, SkillUpgradingRecord> SkillUpgrading = new Dictionary<int, SkillUpgradingRecord>();
        private static Dictionary<int, AchievementRecord> Achievement = new Dictionary<int, AchievementRecord>();
        private static Dictionary<int, ScriptRecord> Script = new Dictionary<int, ScriptRecord>();
        private static Dictionary<int, DropMotherRecord> DropMother = new Dictionary<int, DropMotherRecord>();
        private static Dictionary<int, DropSonRecord> DropSon = new Dictionary<int, DropSonRecord>();
        private static Dictionary<int, EquipTieRecord> EquipTie = new Dictionary<int, EquipTieRecord>();
        private static Dictionary<int, TransferRecord> Transfer = new Dictionary<int, TransferRecord>();
        private static Dictionary<int, ServerConfigRecord> ServerConfig = new Dictionary<int, ServerConfigRecord>();
        private static Dictionary<int, AIRecord> AI = new Dictionary<int, AIRecord>();
        private static Dictionary<int, EventRecord> Event = new Dictionary<int, EventRecord>();
        private static Dictionary<int, DropConfigRecord> DropConfig = new Dictionary<int, DropConfigRecord>();
        private static Dictionary<int, GiftRecord> Gift = new Dictionary<int, GiftRecord>();
        private static Dictionary<int, EquipBlessingRecord> EquipBlessing = new Dictionary<int, EquipBlessingRecord>();
        private static Dictionary<int, EquipAdditionalRecord> EquipAdditional = new Dictionary<int, EquipAdditionalRecord>();
        private static Dictionary<int, EquipExcellentRecord> EquipExcellent = new Dictionary<int, EquipExcellentRecord>();
        private static Dictionary<int, HandBookRecord> HandBook = new Dictionary<int, HandBookRecord>();
        private static Dictionary<int, BookGroupRecord> BookGroup = new Dictionary<int, BookGroupRecord>();
        private static Dictionary<int, ItemComposeRecord> ItemCompose = new Dictionary<int, ItemComposeRecord>();
        private static Dictionary<int, CampRecord> Camp = new Dictionary<int, CampRecord>();
        private static Dictionary<int, FubenRecord> Fuben = new Dictionary<int, FubenRecord>();
        private static Dictionary<int, SkillAreaRecord> SkillArea = new Dictionary<int, SkillAreaRecord>();
        private static Dictionary<int, StatsRecord> Stats = new Dictionary<int, StatsRecord>();
        private static Dictionary<int, StoreRecord> Store = new Dictionary<int, StoreRecord>();
        private static Dictionary<int, InitItemRecord> InitItem = new Dictionary<int, InitItemRecord>();
        private static Dictionary<int, TriggerAreaRecord> TriggerArea = new Dictionary<int, TriggerAreaRecord>();
        private static Dictionary<int, MailRecord> Mail = new Dictionary<int, MailRecord>();
        private static Dictionary<int, BuildingRecord> Building = new Dictionary<int, BuildingRecord>();
        private static Dictionary<int, BuildingRuleRecord> BuildingRule = new Dictionary<int, BuildingRuleRecord>();
        private static Dictionary<int, BuildingServiceRecord> BuildingService = new Dictionary<int, BuildingServiceRecord>();
        private static Dictionary<int, HomeSenceRecord> HomeSence = new Dictionary<int, HomeSenceRecord>();
        private static Dictionary<int, PetRecord> Pet = new Dictionary<int, PetRecord>();
        private static Dictionary<int, PetSkillRecord> PetSkill = new Dictionary<int, PetSkillRecord>();
        private static Dictionary<int, ServiceRecord> Service = new Dictionary<int, ServiceRecord>();
        private static Dictionary<int, StoreTypeRecord> StoreType = new Dictionary<int, StoreTypeRecord>();
        private static Dictionary<int, PetSkillBaseRecord> PetSkillBase = new Dictionary<int, PetSkillBaseRecord>();
        private static Dictionary<int, ElfRecord> Elf = new Dictionary<int, ElfRecord>();
        private static Dictionary<int, ElfGroupRecord> ElfGroup = new Dictionary<int, ElfGroupRecord>();
        private static Dictionary<int, QueueRecord> Queue = new Dictionary<int, QueueRecord>();
        private static Dictionary<int, DrawRecord> Draw = new Dictionary<int, DrawRecord>();
        private static Dictionary<int, PlantRecord> Plant = new Dictionary<int, PlantRecord>();
        private static Dictionary<int, MedalRecord> Medal = new Dictionary<int, MedalRecord>();
        private static Dictionary<int, SailingRecord> Sailing = new Dictionary<int, SailingRecord>();
        private static Dictionary<int, WingTrainRecord> WingTrain = new Dictionary<int, WingTrainRecord>();
        private static Dictionary<int, WingQualityRecord> WingQuality = new Dictionary<int, WingQualityRecord>();
        private static Dictionary<int, PVPRuleRecord> PVPRule = new Dictionary<int, PVPRuleRecord>();
        private static Dictionary<int, ArenaRewardRecord> ArenaReward = new Dictionary<int, ArenaRewardRecord>();
        private static Dictionary<int, ArenaLevelRecord> ArenaLevel = new Dictionary<int, ArenaLevelRecord>();
        private static Dictionary<int, HonorRecord> Honor = new Dictionary<int, HonorRecord>();
        private static Dictionary<int, JJCRootRecord> JJCRoot = new Dictionary<int, JJCRootRecord>();
        private static Dictionary<int, StatueRecord> Statue = new Dictionary<int, StatueRecord>();
        private static Dictionary<int, EquipAdditional1Record> EquipAdditional1 = new Dictionary<int, EquipAdditional1Record>();
        private static Dictionary<int, GuildRecord> Guild = new Dictionary<int, GuildRecord>();
        private static Dictionary<int, GuildBuffRecord> GuildBuff = new Dictionary<int, GuildBuffRecord>();
        private static Dictionary<int, GuildBossRecord> GuildBoss = new Dictionary<int, GuildBossRecord>();
        private static Dictionary<int, GuildAccessRecord> GuildAccess = new Dictionary<int, GuildAccessRecord>();
        private static Dictionary<int, ExpInfoRecord> ExpInfo = new Dictionary<int, ExpInfoRecord>();
        private static Dictionary<int, GroupShopRecord> GroupShop = new Dictionary<int, GroupShopRecord>();
        private static Dictionary<int, GroupShopUpdateRecord> GroupShopUpdate = new Dictionary<int, GroupShopUpdateRecord>();
        private static Dictionary<int, PKModeRecord> PKMode = new Dictionary<int, PKModeRecord>();
        private static Dictionary<int, forgedRecord> forged = new Dictionary<int, forgedRecord>();
        private static Dictionary<int, EquipUpdateRecord> EquipUpdate = new Dictionary<int, EquipUpdateRecord>();
        private static Dictionary<int, GuildMissionRecord> GuildMission = new Dictionary<int, GuildMissionRecord>();
        private static Dictionary<int, OrderFormRecord> OrderForm = new Dictionary<int, OrderFormRecord>();
        private static Dictionary<int, OrderUpdateRecord> OrderUpdate = new Dictionary<int, OrderUpdateRecord>();
        private static Dictionary<int, TradeRecord> Trade = new Dictionary<int, TradeRecord>();
        private static Dictionary<int, GemRecord> Gem = new Dictionary<int, GemRecord>();
        private static Dictionary<int, GemGroupRecord> GemGroup = new Dictionary<int, GemGroupRecord>();
        private static Dictionary<int, SensitiveWordRecord> SensitiveWord = new Dictionary<int, SensitiveWordRecord>();
        private static Dictionary<int, GuidanceRecord> Guidance = new Dictionary<int, GuidanceRecord>();
        private static Dictionary<int, MapTransferRecord> MapTransfer = new Dictionary<int, MapTransferRecord>();
        private static Dictionary<int, RandomCoordinateRecord> RandomCoordinate = new Dictionary<int, RandomCoordinateRecord>();
        private static Dictionary<int, StepByStepRecord> StepByStep = new Dictionary<int, StepByStepRecord>();
        private static Dictionary<int, WorldBOSSRecord> WorldBOSS = new Dictionary<int, WorldBOSSRecord>();
        private static Dictionary<int, WorldBOSSAwardRecord> WorldBOSSAward = new Dictionary<int, WorldBOSSAwardRecord>();
        private static Dictionary<int, PKValueRecord> PKValue = new Dictionary<int, PKValueRecord>();
        private static Dictionary<int, TransmigrationRecord> Transmigration = new Dictionary<int, TransmigrationRecord>();
        private static Dictionary<int, FubenInfoRecord> FubenInfo = new Dictionary<int, FubenInfoRecord>();
        private static Dictionary<int, FubenLogicRecord> FubenLogic = new Dictionary<int, FubenLogicRecord>();
        private static Dictionary<int, ServerNameRecord> ServerName = new Dictionary<int, ServerNameRecord>();
        private static Dictionary<int, GetMissionLevelRecord> GetMissionLevel = new Dictionary<int, GetMissionLevelRecord>();
        private static Dictionary<int, GetMissionTypeRecord> GetMissionType = new Dictionary<int, GetMissionTypeRecord>();
        private static Dictionary<int, GetMissionQulityRecord> GetMissionQulity = new Dictionary<int, GetMissionQulityRecord>();
        private static Dictionary<int, GetPetCountRecord> GetPetCount = new Dictionary<int, GetPetCountRecord>();
        private static Dictionary<int, GetMissionInfoRecord> GetMissionInfo = new Dictionary<int, GetMissionInfoRecord>();
        private static Dictionary<int, GetMissionPlaceRecord> GetMissionPlace = new Dictionary<int, GetMissionPlaceRecord>();
        private static Dictionary<int, GetMissionWeatherRecord> GetMissionWeather = new Dictionary<int, GetMissionWeatherRecord>();
        private static Dictionary<int, GetMissionTimeLevelRecord> GetMissionTimeLevel = new Dictionary<int, GetMissionTimeLevelRecord>();
        private static Dictionary<int, MissionConditionInfoRecord> MissionConditionInfo = new Dictionary<int, MissionConditionInfoRecord>();
        private static Dictionary<int, GetMissionRewardRecord> GetMissionReward = new Dictionary<int, GetMissionRewardRecord>();
        private static Dictionary<int, GetPetTypeRecord> GetPetType = new Dictionary<int, GetPetTypeRecord>();
        private static Dictionary<int, SubjectRecord> Subject = new Dictionary<int, SubjectRecord>();
        private static Dictionary<int, GetMissionNameRecord> GetMissionName = new Dictionary<int, GetMissionNameRecord>();
        private static Dictionary<int, DynamicActivityRecord> DynamicActivity = new Dictionary<int, DynamicActivityRecord>();
        private static Dictionary<int, CompensationRecord> Compensation = new Dictionary<int, CompensationRecord>();
        private static Dictionary<int, CityTalkRecord> CityTalk = new Dictionary<int, CityTalkRecord>();
        private static Dictionary<int, DailyActivityRecord> DailyActivity = new Dictionary<int, DailyActivityRecord>();
        private static Dictionary<int, RechargeRecord> Recharge = new Dictionary<int, RechargeRecord>();
        private static Dictionary<int, NameTitleRecord> NameTitle = new Dictionary<int, NameTitleRecord>();
        private static Dictionary<int, VIPRecord> VIP = new Dictionary<int, VIPRecord>();
        private static Dictionary<int, RechargeActiveRecord> RechargeActive = new Dictionary<int, RechargeActiveRecord>();
        private static Dictionary<int, RechargeActiveNoticeRecord> RechargeActiveNotice = new Dictionary<int, RechargeActiveNoticeRecord>();
        private static Dictionary<int, RechargeActiveInvestmentRecord> RechargeActiveInvestment = new Dictionary<int, RechargeActiveInvestmentRecord>();
        private static Dictionary<int, RechargeActiveInvestmentRewardRecord> RechargeActiveInvestmentReward = new Dictionary<int, RechargeActiveInvestmentRewardRecord>();
        private static Dictionary<int, RechargeActiveCumulativeRecord> RechargeActiveCumulative = new Dictionary<int, RechargeActiveCumulativeRecord>();
        private static Dictionary<int, RechargeActiveCumulativeRewardRecord> RechargeActiveCumulativeReward = new Dictionary<int, RechargeActiveCumulativeRewardRecord>();
        private static Dictionary<int, GiftCodeRecord> GiftCode = new Dictionary<int, GiftCodeRecord>();
        private static Dictionary<int, GMCommandRecord> GMCommand = new Dictionary<int, GMCommandRecord>();
        private static Dictionary<int, AuctionType1Record> AuctionType1 = new Dictionary<int, AuctionType1Record>();
        private static Dictionary<int, AuctionType2Record> AuctionType2 = new Dictionary<int, AuctionType2Record>();
        private static Dictionary<int, AuctionType3Record> AuctionType3 = new Dictionary<int, AuctionType3Record>();
        private static Dictionary<int, YunYingRecord> YunYing = new Dictionary<int, YunYingRecord>();
        private static Dictionary<int, OperationActivityRecord> OperationActivity = new Dictionary<int, OperationActivityRecord>();
        private static Dictionary<int, FirstRechargeRecord> FirstRecharge = new Dictionary<int, FirstRechargeRecord>();
        private static Dictionary<int, MieShiRecord> MieShi = new Dictionary<int, MieShiRecord>();
        private static Dictionary<int, MieShiPublicRecord> MieShiPublic = new Dictionary<int, MieShiPublicRecord>();
        private static Dictionary<int, DefendCityRewardRecord> DefendCityReward = new Dictionary<int, DefendCityRewardRecord>();
        private static Dictionary<int, DefendCityDevoteRewardRecord> DefendCityDevoteReward = new Dictionary<int, DefendCityDevoteRewardRecord>();
        private static Dictionary<int, BatteryLevelRecord> BatteryLevel = new Dictionary<int, BatteryLevelRecord>();
        private static Dictionary<int, BatteryBaseRecord> BatteryBase = new Dictionary<int, BatteryBaseRecord>();
        private static Dictionary<int, PlayerDropRecord> PlayerDrop = new Dictionary<int, PlayerDropRecord>();
        private static Dictionary<int, BuffGroupRecord> BuffGroup = new Dictionary<int, BuffGroupRecord>();
        private static Dictionary<int, BangBuffRecord> BangBuff = new Dictionary<int, BangBuffRecord>();
        private static Dictionary<int, MieshiTowerRewardRecord> MieshiTowerReward = new Dictionary<int, MieshiTowerRewardRecord>();
        private static Dictionary<int, ClimbingTowerRecord> ClimbingTower = new Dictionary<int, ClimbingTowerRecord>();
        private static Dictionary<int, AcientBattleFieldRecord> AcientBattleField = new Dictionary<int, AcientBattleFieldRecord>();
        private static Dictionary<int, ConsumArrayRecord> ConsumArray = new Dictionary<int, ConsumArrayRecord>();
        private static Dictionary<int, BookBaseRecord> BookBase = new Dictionary<int, BookBaseRecord>();
        private static Dictionary<int, MountRecord> Mount = new Dictionary<int, MountRecord>();
        private static Dictionary<int, MountSkillRecord> MountSkill = new Dictionary<int, MountSkillRecord>();
        private static Dictionary<int, MountFeedRecord> MountFeed = new Dictionary<int, MountFeedRecord>();
        private static Dictionary<int, MayaBaseRecord> MayaBase = new Dictionary<int, MayaBaseRecord>();
        private static Dictionary<int, OfflineExperienceRecord> OfflineExperience = new Dictionary<int, OfflineExperienceRecord>();
        private static Dictionary<int, SurveyRecord> Survey = new Dictionary<int, SurveyRecord>();
        private static Dictionary<int, PreferentialRecord> Preferential = new Dictionary<int, PreferentialRecord>();
        private static Dictionary<int, KaiFuRecord> KaiFu = new Dictionary<int, KaiFuRecord>();
        private static Dictionary<int, BossHomeRecord> BossHome = new Dictionary<int, BossHomeRecord>();
        private static Dictionary<int, WarFlagRecord> WarFlag = new Dictionary<int, WarFlagRecord>();
        private static Dictionary<int, LodeRecord> Lode = new Dictionary<int, LodeRecord>();
        private static Dictionary<int, MainActivityRecord> MainActivity = new Dictionary<int, MainActivityRecord>();
        private static Dictionary<int, ObjectTableRecord> ObjectTable = new Dictionary<int, ObjectTableRecord>();
        private static Dictionary<int, BatteryBaseNewRecord> BatteryBaseNew = new Dictionary<int, BatteryBaseNewRecord>();
        private static Dictionary<int, CheckenRecord> Checken = new Dictionary<int, CheckenRecord>();
        private static Dictionary<int, CheckenLvRecord> CheckenLv = new Dictionary<int, CheckenLvRecord>();
        private static Dictionary<int, CheckenRewardRecord> CheckenReward = new Dictionary<int, CheckenRewardRecord>();
        private static Dictionary<int, CheckenFinalRewardRecord> CheckenFinalReward = new Dictionary<int, CheckenFinalRewardRecord>();
        private static Dictionary<int, SuperVipRecord> SuperVip = new Dictionary<int, SuperVipRecord>();
        private static Dictionary<int, IosMutiplePlatformRecord> IosMutiplePlatform = new Dictionary<int, IosMutiplePlatformRecord>();
        private static Dictionary<int, BattleCorrectRecord> BattleCorrect = new Dictionary<int, BattleCorrectRecord>();
        private static Dictionary<int, FashionTitleRecord> FashionTitle = new Dictionary<int, FashionTitleRecord>();
        static Table()
        {
            TableInit<ConditionTableRecord>.Table_Init(TalbeHelper.GetLoadPath("ConditionTable"), ConditionTable);
            TableInit<FlagRecord>.Table_Init(TalbeHelper.GetLoadPath("Flag"), Flag);
            TableInit<ExdataRecord>.Table_Init(TalbeHelper.GetLoadPath("Exdata"), Exdata);
            TableInit<DictionaryRecord>.Table_Init(TalbeHelper.GetLoadPath("Dictionary"), Dictionary);
            TableInit<BagBaseRecord>.Table_Init(TalbeHelper.GetLoadPath("BagBase"), BagBase);
            TableInit<ItemBaseRecord>.Table_Init(TalbeHelper.GetLoadPath("ItemBase"), ItemBase);
            TableInit<ItemTypeRecord>.Table_Init(TalbeHelper.GetLoadPath("ItemType"), ItemType);
            TableInit<ColorBaseRecord>.Table_Init(TalbeHelper.GetLoadPath("ColorBase"), ColorBase);
            TableInit<BuffRecord>.Table_Init(TalbeHelper.GetLoadPath("Buff"), Buff);
            TableInit<MissionRecord>.Table_Init(TalbeHelper.GetLoadPath("MissionBase"), Mission);
            TableInit<CharacterBaseRecord>.Table_Init(TalbeHelper.GetLoadPath("CharacterBase"), CharacterBase);
            TableInit<ActorRecord>.Table_Init(TalbeHelper.GetLoadPath("Actor"), Actor);
            TableInit<AttrRefRecord>.Table_Init(TalbeHelper.GetLoadPath("AttrRef"), AttrRef);
            TableInit<EquipRecord>.Table_Init(TalbeHelper.GetLoadPath("EquipBase"), Equip);
            TableInit<EquipRelateRecord>.Table_Init(TalbeHelper.GetLoadPath("EquipRelate"), EquipRelate);
            TableInit<EquipEnchantRecord>.Table_Init(TalbeHelper.GetLoadPath("EquipEnchant"), EquipEnchant);
            TableInit<EquipEnchantChanceRecord>.Table_Init(TalbeHelper.GetLoadPath("EquipEnchantChance"), EquipEnchantChance);
            TableInit<TitleRecord>.Table_Init(TalbeHelper.GetLoadPath("Title"), Title);
            TableInit<EquipEnchanceRecord>.Table_Init(TalbeHelper.GetLoadPath("EquipEnchance"), EquipEnchance);
            TableInit<LevelDataRecord>.Table_Init(TalbeHelper.GetLoadPath("LevelData"), LevelData);
            TableInit<SceneNpcRecord>.Table_Init(TalbeHelper.GetLoadPath("SceneNpc"), SceneNpc);
            TableInit<SkillRecord>.Table_Init(TalbeHelper.GetLoadPath("Skill"), Skill);
            TableInit<BulletRecord>.Table_Init(TalbeHelper.GetLoadPath("Bullet"), Bullet);
            TableInit<SceneRecord>.Table_Init(TalbeHelper.GetLoadPath("Scene"), Scene);
            TableInit<TalentRecord>.Table_Init(TalbeHelper.GetLoadPath("Talent"), Talent);
            TableInit<NpcBaseRecord>.Table_Init(TalbeHelper.GetLoadPath("NpcBase"), NpcBase);
            TableInit<SkillUpgradingRecord>.Table_Init(TalbeHelper.GetLoadPath("SkillUpgrading"), SkillUpgrading);
            TableInit<AchievementRecord>.Table_Init(TalbeHelper.GetLoadPath("Achievement"), Achievement);
            TableInit<ScriptRecord>.Table_Init(TalbeHelper.GetLoadPath("Script"), Script);
            TableInit<DropMotherRecord>.Table_Init(TalbeHelper.GetLoadPath("DropMother"), DropMother);
            TableInit<DropSonRecord>.Table_Init(TalbeHelper.GetLoadPath("DropSon"), DropSon);
            TableInit<EquipTieRecord>.Table_Init(TalbeHelper.GetLoadPath("EquipTie"), EquipTie);
            TableInit<TransferRecord>.Table_Init(TalbeHelper.GetLoadPath("Transfer"), Transfer);
            TableInit<ServerConfigRecord>.Table_Init(TalbeHelper.GetLoadPath("ServerConfig"), ServerConfig);
            TableInit<AIRecord>.Table_Init(TalbeHelper.GetLoadPath("AI"), AI);
            TableInit<EventRecord>.Table_Init(TalbeHelper.GetLoadPath("Event"), Event);
            TableInit<DropConfigRecord>.Table_Init(TalbeHelper.GetLoadPath("DropConfig"), DropConfig);
            TableInit<GiftRecord>.Table_Init(TalbeHelper.GetLoadPath("Gift"), Gift);
            TableInit<EquipBlessingRecord>.Table_Init(TalbeHelper.GetLoadPath("EquipBlessing"), EquipBlessing);
            TableInit<EquipAdditionalRecord>.Table_Init(TalbeHelper.GetLoadPath("EquipAdditional"), EquipAdditional);
            TableInit<EquipExcellentRecord>.Table_Init(TalbeHelper.GetLoadPath("EquipExcellent"), EquipExcellent);
            TableInit<HandBookRecord>.Table_Init(TalbeHelper.GetLoadPath("HandBook"), HandBook);
            TableInit<BookGroupRecord>.Table_Init(TalbeHelper.GetLoadPath("BookGroup"), BookGroup);
            TableInit<ItemComposeRecord>.Table_Init(TalbeHelper.GetLoadPath("ItemCompose"), ItemCompose);
            TableInit<CampRecord>.Table_Init(TalbeHelper.GetLoadPath("Camp"), Camp);
            TableInit<FubenRecord>.Table_Init(TalbeHelper.GetLoadPath("Fuben"), Fuben);
            TableInit<SkillAreaRecord>.Table_Init(TalbeHelper.GetLoadPath("SkillArea"), SkillArea);
            TableInit<StatsRecord>.Table_Init(TalbeHelper.GetLoadPath("Stats"), Stats);
            TableInit<StoreRecord>.Table_Init(TalbeHelper.GetLoadPath("Store"), Store);
            TableInit<InitItemRecord>.Table_Init(TalbeHelper.GetLoadPath("InitItem"), InitItem);
            TableInit<TriggerAreaRecord>.Table_Init(TalbeHelper.GetLoadPath("TriggerArea"), TriggerArea);
            TableInit<MailRecord>.Table_Init(TalbeHelper.GetLoadPath("Mail"), Mail);
            TableInit<BuildingRecord>.Table_Init(TalbeHelper.GetLoadPath("Building"), Building);
            TableInit<BuildingRuleRecord>.Table_Init(TalbeHelper.GetLoadPath("BuildingRule"), BuildingRule);
            TableInit<BuildingServiceRecord>.Table_Init(TalbeHelper.GetLoadPath("BuildingService"), BuildingService);
            TableInit<HomeSenceRecord>.Table_Init(TalbeHelper.GetLoadPath("HomeSence"), HomeSence);
            TableInit<PetRecord>.Table_Init(TalbeHelper.GetLoadPath("Pet"), Pet);
            TableInit<PetSkillRecord>.Table_Init(TalbeHelper.GetLoadPath("PetSkill"), PetSkill);
            TableInit<ServiceRecord>.Table_Init(TalbeHelper.GetLoadPath("Service"), Service);
            TableInit<StoreTypeRecord>.Table_Init(TalbeHelper.GetLoadPath("StoreType"), StoreType);
            TableInit<PetSkillBaseRecord>.Table_Init(TalbeHelper.GetLoadPath("PetSkillBase"), PetSkillBase);
            TableInit<ElfRecord>.Table_Init(TalbeHelper.GetLoadPath("Elf"), Elf);
            TableInit<ElfGroupRecord>.Table_Init(TalbeHelper.GetLoadPath("ElfGroup"), ElfGroup);
            TableInit<QueueRecord>.Table_Init(TalbeHelper.GetLoadPath("Queue"), Queue);
            TableInit<DrawRecord>.Table_Init(TalbeHelper.GetLoadPath("Draw"), Draw);
            TableInit<PlantRecord>.Table_Init(TalbeHelper.GetLoadPath("Plant"), Plant);
            TableInit<MedalRecord>.Table_Init(TalbeHelper.GetLoadPath("Medal"), Medal);
            TableInit<SailingRecord>.Table_Init(TalbeHelper.GetLoadPath("Sailing"), Sailing);
            TableInit<WingTrainRecord>.Table_Init(TalbeHelper.GetLoadPath("WingTrain"), WingTrain);
            TableInit<WingQualityRecord>.Table_Init(TalbeHelper.GetLoadPath("WingQuality"), WingQuality);
            TableInit<PVPRuleRecord>.Table_Init(TalbeHelper.GetLoadPath("PVPRule"), PVPRule);
            TableInit<ArenaRewardRecord>.Table_Init(TalbeHelper.GetLoadPath("ArenaReward"), ArenaReward);
            TableInit<ArenaLevelRecord>.Table_Init(TalbeHelper.GetLoadPath("ArenaLevel"), ArenaLevel);
            TableInit<HonorRecord>.Table_Init(TalbeHelper.GetLoadPath("Honor"), Honor);
            TableInit<JJCRootRecord>.Table_Init(TalbeHelper.GetLoadPath("JJCRoot"), JJCRoot);
            TableInit<StatueRecord>.Table_Init(TalbeHelper.GetLoadPath("Statue"), Statue);
            TableInit<EquipAdditional1Record>.Table_Init(TalbeHelper.GetLoadPath("EquipAdditional1"), EquipAdditional1);
            TableInit<GuildRecord>.Table_Init(TalbeHelper.GetLoadPath("Guild"), Guild);
            TableInit<GuildBuffRecord>.Table_Init(TalbeHelper.GetLoadPath("GuildBuff"), GuildBuff);
            TableInit<GuildBossRecord>.Table_Init(TalbeHelper.GetLoadPath("GuildBoss"), GuildBoss);
            TableInit<GuildAccessRecord>.Table_Init(TalbeHelper.GetLoadPath("GuildAccess"), GuildAccess);
            TableInit<ExpInfoRecord>.Table_Init(TalbeHelper.GetLoadPath("ExpInfo"), ExpInfo);
            TableInit<GroupShopRecord>.Table_Init(TalbeHelper.GetLoadPath("GroupShop"), GroupShop);
            TableInit<GroupShopUpdateRecord>.Table_Init(TalbeHelper.GetLoadPath("GroupShopUpdate"), GroupShopUpdate);
            TableInit<PKModeRecord>.Table_Init(TalbeHelper.GetLoadPath("PKMode"), PKMode);
            TableInit<forgedRecord>.Table_Init(TalbeHelper.GetLoadPath("forged"), forged);
            TableInit<EquipUpdateRecord>.Table_Init(TalbeHelper.GetLoadPath("EquipUpdate"), EquipUpdate);
            TableInit<GuildMissionRecord>.Table_Init(TalbeHelper.GetLoadPath("GuildMission"), GuildMission);
            TableInit<OrderFormRecord>.Table_Init(TalbeHelper.GetLoadPath("OrderForm"), OrderForm);
            TableInit<OrderUpdateRecord>.Table_Init(TalbeHelper.GetLoadPath("OrderUpdate"), OrderUpdate);
            TableInit<TradeRecord>.Table_Init(TalbeHelper.GetLoadPath("Trade"), Trade);
            TableInit<GemRecord>.Table_Init(TalbeHelper.GetLoadPath("Gem"), Gem);
            TableInit<GemGroupRecord>.Table_Init(TalbeHelper.GetLoadPath("GemGroup"), GemGroup);
            TableInit<SensitiveWordRecord>.Table_Init(TalbeHelper.GetLoadPath("SensitiveWord"), SensitiveWord);
            TableInit<GuidanceRecord>.Table_Init(TalbeHelper.GetLoadPath("Guidance"), Guidance);
            TableInit<MapTransferRecord>.Table_Init(TalbeHelper.GetLoadPath("MapTransfer"), MapTransfer);
            TableInit<RandomCoordinateRecord>.Table_Init(TalbeHelper.GetLoadPath("RandomCoordinate"), RandomCoordinate);
            TableInit<StepByStepRecord>.Table_Init(TalbeHelper.GetLoadPath("StepByStep"), StepByStep);
            TableInit<WorldBOSSRecord>.Table_Init(TalbeHelper.GetLoadPath("WorldBOSS"), WorldBOSS);
            TableInit<WorldBOSSAwardRecord>.Table_Init(TalbeHelper.GetLoadPath("WorldBOSSAward"), WorldBOSSAward);
            TableInit<PKValueRecord>.Table_Init(TalbeHelper.GetLoadPath("PKValue"), PKValue);
            TableInit<TransmigrationRecord>.Table_Init(TalbeHelper.GetLoadPath("Transmigration"), Transmigration);
            TableInit<FubenInfoRecord>.Table_Init(TalbeHelper.GetLoadPath("FubenInfo"), FubenInfo);
            TableInit<FubenLogicRecord>.Table_Init(TalbeHelper.GetLoadPath("FubenLogic"), FubenLogic);
            TableInit<ServerNameRecord>.Table_Init(TalbeHelper.GetLoadPath("ServerName"), ServerName);
            TableInit<GetMissionLevelRecord>.Table_Init(TalbeHelper.GetLoadPath("GetMissionLevel"), GetMissionLevel);
            TableInit<GetMissionTypeRecord>.Table_Init(TalbeHelper.GetLoadPath("GetMissionType"), GetMissionType);
            TableInit<GetMissionQulityRecord>.Table_Init(TalbeHelper.GetLoadPath("GetMissionQulity"), GetMissionQulity);
            TableInit<GetPetCountRecord>.Table_Init(TalbeHelper.GetLoadPath("GetPetCount"), GetPetCount);
            TableInit<GetMissionInfoRecord>.Table_Init(TalbeHelper.GetLoadPath("GetMissionInfo"), GetMissionInfo);
            TableInit<GetMissionPlaceRecord>.Table_Init(TalbeHelper.GetLoadPath("GetMissionPlace"), GetMissionPlace);
            TableInit<GetMissionWeatherRecord>.Table_Init(TalbeHelper.GetLoadPath("GetMissionWeather"), GetMissionWeather);
            TableInit<GetMissionTimeLevelRecord>.Table_Init(TalbeHelper.GetLoadPath("GetMissionTimeLevel"), GetMissionTimeLevel);
            TableInit<MissionConditionInfoRecord>.Table_Init(TalbeHelper.GetLoadPath("MissionConditionInfo"), MissionConditionInfo);
            TableInit<GetMissionRewardRecord>.Table_Init(TalbeHelper.GetLoadPath("GetMissionReward"), GetMissionReward);
            TableInit<GetPetTypeRecord>.Table_Init(TalbeHelper.GetLoadPath("GetPetType"), GetPetType);
            TableInit<SubjectRecord>.Table_Init(TalbeHelper.GetLoadPath("Subject"), Subject);
            TableInit<GetMissionNameRecord>.Table_Init(TalbeHelper.GetLoadPath("GetMissionName"), GetMissionName);
            TableInit<DynamicActivityRecord>.Table_Init(TalbeHelper.GetLoadPath("DynamicActivity"), DynamicActivity);
            TableInit<CompensationRecord>.Table_Init(TalbeHelper.GetLoadPath("Compensation"), Compensation);
            TableInit<CityTalkRecord>.Table_Init(TalbeHelper.GetLoadPath("CityTalk"), CityTalk);
            TableInit<DailyActivityRecord>.Table_Init(TalbeHelper.GetLoadPath("DailyActivity"), DailyActivity);
            TableInit<RechargeRecord>.Table_Init(TalbeHelper.GetLoadPath("Recharge"), Recharge);
            TableInit<NameTitleRecord>.Table_Init(TalbeHelper.GetLoadPath("NameTitle"), NameTitle);
            TableInit<VIPRecord>.Table_Init(TalbeHelper.GetLoadPath("VIP"), VIP);
            TableInit<RechargeActiveRecord>.Table_Init(TalbeHelper.GetLoadPath("RechargeActive"), RechargeActive);
            TableInit<RechargeActiveNoticeRecord>.Table_Init(TalbeHelper.GetLoadPath("RechargeActiveNotice"), RechargeActiveNotice);
            TableInit<RechargeActiveInvestmentRecord>.Table_Init(TalbeHelper.GetLoadPath("RechargeActiveInvestment"), RechargeActiveInvestment);
            TableInit<RechargeActiveInvestmentRewardRecord>.Table_Init(TalbeHelper.GetLoadPath("RechargeActiveInvestmentReward"), RechargeActiveInvestmentReward);
            TableInit<RechargeActiveCumulativeRecord>.Table_Init(TalbeHelper.GetLoadPath("RechargeActiveCumulative"), RechargeActiveCumulative);
            TableInit<RechargeActiveCumulativeRewardRecord>.Table_Init(TalbeHelper.GetLoadPath("RechargeActiveCumulativeReward"), RechargeActiveCumulativeReward);
            TableInit<GiftCodeRecord>.Table_Init(TalbeHelper.GetLoadPath("GiftCode"), GiftCode);
            TableInit<GMCommandRecord>.Table_Init(TalbeHelper.GetLoadPath("GMCommand"), GMCommand);
            TableInit<AuctionType1Record>.Table_Init(TalbeHelper.GetLoadPath("AuctionType1"), AuctionType1);
            TableInit<AuctionType2Record>.Table_Init(TalbeHelper.GetLoadPath("AuctionType2"), AuctionType2);
            TableInit<AuctionType3Record>.Table_Init(TalbeHelper.GetLoadPath("AuctionType3"), AuctionType3);
            TableInit<YunYingRecord>.Table_Init(TalbeHelper.GetLoadPath("YunYing"), YunYing);
            TableInit<OperationActivityRecord>.Table_Init(TalbeHelper.GetLoadPath("OperationActivity"), OperationActivity);
            TableInit<FirstRechargeRecord>.Table_Init(TalbeHelper.GetLoadPath("FirstRecharge"), FirstRecharge);
            TableInit<MieShiRecord>.Table_Init(TalbeHelper.GetLoadPath("MieShi"), MieShi);
            TableInit<MieShiPublicRecord>.Table_Init(TalbeHelper.GetLoadPath("MieShiPublic"), MieShiPublic);
            TableInit<DefendCityRewardRecord>.Table_Init(TalbeHelper.GetLoadPath("DefendCityReward"), DefendCityReward);
            TableInit<DefendCityDevoteRewardRecord>.Table_Init(TalbeHelper.GetLoadPath("DefendCityDevoteReward"), DefendCityDevoteReward);
            TableInit<BatteryLevelRecord>.Table_Init(TalbeHelper.GetLoadPath("BatteryLevel"), BatteryLevel);
            TableInit<BatteryBaseRecord>.Table_Init(TalbeHelper.GetLoadPath("BatteryBase"), BatteryBase);
            TableInit<PlayerDropRecord>.Table_Init(TalbeHelper.GetLoadPath("PlayerDrop"), PlayerDrop);
            TableInit<BuffGroupRecord>.Table_Init(TalbeHelper.GetLoadPath("BuffGroup"), BuffGroup);
            TableInit<BangBuffRecord>.Table_Init(TalbeHelper.GetLoadPath("BangBuff"), BangBuff);
            TableInit<MieshiTowerRewardRecord>.Table_Init(TalbeHelper.GetLoadPath("MieshiTowerReward"), MieshiTowerReward);
            TableInit<ClimbingTowerRecord>.Table_Init(TalbeHelper.GetLoadPath("ClimbingTower"), ClimbingTower);
            TableInit<AcientBattleFieldRecord>.Table_Init(TalbeHelper.GetLoadPath("AcientBattleField"), AcientBattleField);
            TableInit<ConsumArrayRecord>.Table_Init(TalbeHelper.GetLoadPath("ConsumArray"), ConsumArray);
            TableInit<BookBaseRecord>.Table_Init(TalbeHelper.GetLoadPath("BookBase"), BookBase);
            TableInit<MountRecord>.Table_Init(TalbeHelper.GetLoadPath("Mount"), Mount);
            TableInit<MountSkillRecord>.Table_Init(TalbeHelper.GetLoadPath("MountSkill"), MountSkill);
            TableInit<MountFeedRecord>.Table_Init(TalbeHelper.GetLoadPath("MountFeed"), MountFeed);
            TableInit<MayaBaseRecord>.Table_Init(TalbeHelper.GetLoadPath("MayaBase"), MayaBase);
            TableInit<OfflineExperienceRecord>.Table_Init(TalbeHelper.GetLoadPath("OfflineExperience"), OfflineExperience);
            TableInit<SurveyRecord>.Table_Init(TalbeHelper.GetLoadPath("Survey"), Survey);
            TableInit<PreferentialRecord>.Table_Init(TalbeHelper.GetLoadPath("Preferential"), Preferential);
            TableInit<KaiFuRecord>.Table_Init(TalbeHelper.GetLoadPath("KaiFu"), KaiFu);
            TableInit<BossHomeRecord>.Table_Init(TalbeHelper.GetLoadPath("BossHome"), BossHome);
            TableInit<WarFlagRecord>.Table_Init(TalbeHelper.GetLoadPath("WarFlag"), WarFlag);
            TableInit<LodeRecord>.Table_Init(TalbeHelper.GetLoadPath("Lode"), Lode);
            TableInit<MainActivityRecord>.Table_Init(TalbeHelper.GetLoadPath("MainActivity"), MainActivity);
            TableInit<ObjectTableRecord>.Table_Init(TalbeHelper.GetLoadPath("ObjectTable"), ObjectTable);
            TableInit<BatteryBaseNewRecord>.Table_Init(TalbeHelper.GetLoadPath("BatteryBaseNew"), BatteryBaseNew);
            TableInit<CheckenRecord>.Table_Init(TalbeHelper.GetLoadPath("Checken"), Checken);
            TableInit<CheckenLvRecord>.Table_Init(TalbeHelper.GetLoadPath("CheckenLv"), CheckenLv);
            TableInit<CheckenRewardRecord>.Table_Init(TalbeHelper.GetLoadPath("CheckenReward"), CheckenReward);
            TableInit<CheckenFinalRewardRecord>.Table_Init(TalbeHelper.GetLoadPath("CheckenFinalReward"), CheckenFinalReward);
            TableInit<SuperVipRecord>.Table_Init(TalbeHelper.GetLoadPath("SuperVip"), SuperVip);
            TableInit<IosMutiplePlatformRecord>.Table_Init(TalbeHelper.GetLoadPath("IosMutiplePlatform"), IosMutiplePlatform);
            TableInit<BattleCorrectRecord>.Table_Init(TalbeHelper.GetLoadPath("BattleCorrect"), BattleCorrect);
            TableInit<FashionTitleRecord>.Table_Init(TalbeHelper.GetLoadPath("FashionTitle"), FashionTitle);
        }
        public static void ReloadTable(string tableName)
        {
            if (tableName == "all")
            {
                    TableInit<ConditionTableRecord>.Table_Reload(TalbeHelper.GetLoadPath("ConditionTable"), ConditionTable);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ConditionTable"));
                    TableInit<FlagRecord>.Table_Reload(TalbeHelper.GetLoadPath("Flag"), Flag);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Flag"));
                    TableInit<ExdataRecord>.Table_Reload(TalbeHelper.GetLoadPath("Exdata"), Exdata);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Exdata"));
                    TableInit<DictionaryRecord>.Table_Reload(TalbeHelper.GetLoadPath("Dictionary"), Dictionary);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Dictionary"));
                    TableInit<BagBaseRecord>.Table_Reload(TalbeHelper.GetLoadPath("BagBase"), BagBase);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("BagBase"));
                    TableInit<ItemBaseRecord>.Table_Reload(TalbeHelper.GetLoadPath("ItemBase"), ItemBase);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ItemBase"));
                    TableInit<ItemTypeRecord>.Table_Reload(TalbeHelper.GetLoadPath("ItemType"), ItemType);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ItemType"));
                    TableInit<ColorBaseRecord>.Table_Reload(TalbeHelper.GetLoadPath("ColorBase"), ColorBase);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ColorBase"));
                    TableInit<BuffRecord>.Table_Reload(TalbeHelper.GetLoadPath("Buff"), Buff);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Buff"));
                    TableInit<MissionRecord>.Table_Reload(TalbeHelper.GetLoadPath("MissionBase"), Mission);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("MissionBase"));
                    TableInit<CharacterBaseRecord>.Table_Reload(TalbeHelper.GetLoadPath("CharacterBase"), CharacterBase);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("CharacterBase"));
                    TableInit<ActorRecord>.Table_Reload(TalbeHelper.GetLoadPath("Actor"), Actor);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Actor"));
                    TableInit<AttrRefRecord>.Table_Reload(TalbeHelper.GetLoadPath("AttrRef"), AttrRef);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("AttrRef"));
                    TableInit<EquipRecord>.Table_Reload(TalbeHelper.GetLoadPath("EquipBase"), Equip);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("EquipBase"));
                    TableInit<EquipRelateRecord>.Table_Reload(TalbeHelper.GetLoadPath("EquipRelate"), EquipRelate);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("EquipRelate"));
                    TableInit<EquipEnchantRecord>.Table_Reload(TalbeHelper.GetLoadPath("EquipEnchant"), EquipEnchant);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("EquipEnchant"));
                    TableInit<EquipEnchantChanceRecord>.Table_Reload(TalbeHelper.GetLoadPath("EquipEnchantChance"), EquipEnchantChance);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("EquipEnchantChance"));
                    TableInit<TitleRecord>.Table_Reload(TalbeHelper.GetLoadPath("Title"), Title);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Title"));
                    TableInit<EquipEnchanceRecord>.Table_Reload(TalbeHelper.GetLoadPath("EquipEnchance"), EquipEnchance);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("EquipEnchance"));
                    TableInit<LevelDataRecord>.Table_Reload(TalbeHelper.GetLoadPath("LevelData"), LevelData);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("LevelData"));
                    TableInit<SceneNpcRecord>.Table_Reload(TalbeHelper.GetLoadPath("SceneNpc"), SceneNpc);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("SceneNpc"));
                    TableInit<SkillRecord>.Table_Reload(TalbeHelper.GetLoadPath("Skill"), Skill);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Skill"));
                    TableInit<BulletRecord>.Table_Reload(TalbeHelper.GetLoadPath("Bullet"), Bullet);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Bullet"));
                    TableInit<SceneRecord>.Table_Reload(TalbeHelper.GetLoadPath("Scene"), Scene);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Scene"));
                    TableInit<TalentRecord>.Table_Reload(TalbeHelper.GetLoadPath("Talent"), Talent);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Talent"));
                    TableInit<NpcBaseRecord>.Table_Reload(TalbeHelper.GetLoadPath("NpcBase"), NpcBase);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("NpcBase"));
                    TableInit<SkillUpgradingRecord>.Table_Reload(TalbeHelper.GetLoadPath("SkillUpgrading"), SkillUpgrading);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("SkillUpgrading"));
                    TableInit<AchievementRecord>.Table_Reload(TalbeHelper.GetLoadPath("Achievement"), Achievement);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Achievement"));
                    TableInit<ScriptRecord>.Table_Reload(TalbeHelper.GetLoadPath("Script"), Script);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Script"));
                    TableInit<DropMotherRecord>.Table_Reload(TalbeHelper.GetLoadPath("DropMother"), DropMother);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("DropMother"));
                    TableInit<DropSonRecord>.Table_Reload(TalbeHelper.GetLoadPath("DropSon"), DropSon);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("DropSon"));
                    TableInit<EquipTieRecord>.Table_Reload(TalbeHelper.GetLoadPath("EquipTie"), EquipTie);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("EquipTie"));
                    TableInit<TransferRecord>.Table_Reload(TalbeHelper.GetLoadPath("Transfer"), Transfer);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Transfer"));
                    TableInit<ServerConfigRecord>.Table_Reload(TalbeHelper.GetLoadPath("ServerConfig"), ServerConfig);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ServerConfig"));
                    TableInit<AIRecord>.Table_Reload(TalbeHelper.GetLoadPath("AI"), AI);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("AI"));
                    TableInit<EventRecord>.Table_Reload(TalbeHelper.GetLoadPath("Event"), Event);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Event"));
                    TableInit<DropConfigRecord>.Table_Reload(TalbeHelper.GetLoadPath("DropConfig"), DropConfig);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("DropConfig"));
                    TableInit<GiftRecord>.Table_Reload(TalbeHelper.GetLoadPath("Gift"), Gift);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Gift"));
                    TableInit<EquipBlessingRecord>.Table_Reload(TalbeHelper.GetLoadPath("EquipBlessing"), EquipBlessing);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("EquipBlessing"));
                    TableInit<EquipAdditionalRecord>.Table_Reload(TalbeHelper.GetLoadPath("EquipAdditional"), EquipAdditional);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("EquipAdditional"));
                    TableInit<EquipExcellentRecord>.Table_Reload(TalbeHelper.GetLoadPath("EquipExcellent"), EquipExcellent);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("EquipExcellent"));
                    TableInit<HandBookRecord>.Table_Reload(TalbeHelper.GetLoadPath("HandBook"), HandBook);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("HandBook"));
                    TableInit<BookGroupRecord>.Table_Reload(TalbeHelper.GetLoadPath("BookGroup"), BookGroup);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("BookGroup"));
                    TableInit<ItemComposeRecord>.Table_Reload(TalbeHelper.GetLoadPath("ItemCompose"), ItemCompose);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ItemCompose"));
                    TableInit<CampRecord>.Table_Reload(TalbeHelper.GetLoadPath("Camp"), Camp);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Camp"));
                    TableInit<FubenRecord>.Table_Reload(TalbeHelper.GetLoadPath("Fuben"), Fuben);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Fuben"));
                    TableInit<SkillAreaRecord>.Table_Reload(TalbeHelper.GetLoadPath("SkillArea"), SkillArea);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("SkillArea"));
                    TableInit<StatsRecord>.Table_Reload(TalbeHelper.GetLoadPath("Stats"), Stats);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Stats"));
                    TableInit<StoreRecord>.Table_Reload(TalbeHelper.GetLoadPath("Store"), Store);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Store"));
                    TableInit<InitItemRecord>.Table_Reload(TalbeHelper.GetLoadPath("InitItem"), InitItem);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("InitItem"));
                    TableInit<TriggerAreaRecord>.Table_Reload(TalbeHelper.GetLoadPath("TriggerArea"), TriggerArea);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("TriggerArea"));
                    TableInit<MailRecord>.Table_Reload(TalbeHelper.GetLoadPath("Mail"), Mail);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Mail"));
                    TableInit<BuildingRecord>.Table_Reload(TalbeHelper.GetLoadPath("Building"), Building);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Building"));
                    TableInit<BuildingRuleRecord>.Table_Reload(TalbeHelper.GetLoadPath("BuildingRule"), BuildingRule);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("BuildingRule"));
                    TableInit<BuildingServiceRecord>.Table_Reload(TalbeHelper.GetLoadPath("BuildingService"), BuildingService);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("BuildingService"));
                    TableInit<HomeSenceRecord>.Table_Reload(TalbeHelper.GetLoadPath("HomeSence"), HomeSence);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("HomeSence"));
                    TableInit<PetRecord>.Table_Reload(TalbeHelper.GetLoadPath("Pet"), Pet);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Pet"));
                    TableInit<PetSkillRecord>.Table_Reload(TalbeHelper.GetLoadPath("PetSkill"), PetSkill);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("PetSkill"));
                    TableInit<ServiceRecord>.Table_Reload(TalbeHelper.GetLoadPath("Service"), Service);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Service"));
                    TableInit<StoreTypeRecord>.Table_Reload(TalbeHelper.GetLoadPath("StoreType"), StoreType);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("StoreType"));
                    TableInit<PetSkillBaseRecord>.Table_Reload(TalbeHelper.GetLoadPath("PetSkillBase"), PetSkillBase);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("PetSkillBase"));
                    TableInit<ElfRecord>.Table_Reload(TalbeHelper.GetLoadPath("Elf"), Elf);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Elf"));
                    TableInit<ElfGroupRecord>.Table_Reload(TalbeHelper.GetLoadPath("ElfGroup"), ElfGroup);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ElfGroup"));
                    TableInit<QueueRecord>.Table_Reload(TalbeHelper.GetLoadPath("Queue"), Queue);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Queue"));
                    TableInit<DrawRecord>.Table_Reload(TalbeHelper.GetLoadPath("Draw"), Draw);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Draw"));
                    TableInit<PlantRecord>.Table_Reload(TalbeHelper.GetLoadPath("Plant"), Plant);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Plant"));
                    TableInit<MedalRecord>.Table_Reload(TalbeHelper.GetLoadPath("Medal"), Medal);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Medal"));
                    TableInit<SailingRecord>.Table_Reload(TalbeHelper.GetLoadPath("Sailing"), Sailing);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Sailing"));
                    TableInit<WingTrainRecord>.Table_Reload(TalbeHelper.GetLoadPath("WingTrain"), WingTrain);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("WingTrain"));
                    TableInit<WingQualityRecord>.Table_Reload(TalbeHelper.GetLoadPath("WingQuality"), WingQuality);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("WingQuality"));
                    TableInit<PVPRuleRecord>.Table_Reload(TalbeHelper.GetLoadPath("PVPRule"), PVPRule);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("PVPRule"));
                    TableInit<ArenaRewardRecord>.Table_Reload(TalbeHelper.GetLoadPath("ArenaReward"), ArenaReward);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ArenaReward"));
                    TableInit<ArenaLevelRecord>.Table_Reload(TalbeHelper.GetLoadPath("ArenaLevel"), ArenaLevel);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ArenaLevel"));
                    TableInit<HonorRecord>.Table_Reload(TalbeHelper.GetLoadPath("Honor"), Honor);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Honor"));
                    TableInit<JJCRootRecord>.Table_Reload(TalbeHelper.GetLoadPath("JJCRoot"), JJCRoot);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("JJCRoot"));
                    TableInit<StatueRecord>.Table_Reload(TalbeHelper.GetLoadPath("Statue"), Statue);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Statue"));
                    TableInit<EquipAdditional1Record>.Table_Reload(TalbeHelper.GetLoadPath("EquipAdditional1"), EquipAdditional1);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("EquipAdditional1"));
                    TableInit<GuildRecord>.Table_Reload(TalbeHelper.GetLoadPath("Guild"), Guild);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Guild"));
                    TableInit<GuildBuffRecord>.Table_Reload(TalbeHelper.GetLoadPath("GuildBuff"), GuildBuff);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GuildBuff"));
                    TableInit<GuildBossRecord>.Table_Reload(TalbeHelper.GetLoadPath("GuildBoss"), GuildBoss);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GuildBoss"));
                    TableInit<GuildAccessRecord>.Table_Reload(TalbeHelper.GetLoadPath("GuildAccess"), GuildAccess);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GuildAccess"));
                    TableInit<ExpInfoRecord>.Table_Reload(TalbeHelper.GetLoadPath("ExpInfo"), ExpInfo);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ExpInfo"));
                    TableInit<GroupShopRecord>.Table_Reload(TalbeHelper.GetLoadPath("GroupShop"), GroupShop);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GroupShop"));
                    TableInit<GroupShopUpdateRecord>.Table_Reload(TalbeHelper.GetLoadPath("GroupShopUpdate"), GroupShopUpdate);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GroupShopUpdate"));
                    TableInit<PKModeRecord>.Table_Reload(TalbeHelper.GetLoadPath("PKMode"), PKMode);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("PKMode"));
                    TableInit<forgedRecord>.Table_Reload(TalbeHelper.GetLoadPath("forged"), forged);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("forged"));
                    TableInit<EquipUpdateRecord>.Table_Reload(TalbeHelper.GetLoadPath("EquipUpdate"), EquipUpdate);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("EquipUpdate"));
                    TableInit<GuildMissionRecord>.Table_Reload(TalbeHelper.GetLoadPath("GuildMission"), GuildMission);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GuildMission"));
                    TableInit<OrderFormRecord>.Table_Reload(TalbeHelper.GetLoadPath("OrderForm"), OrderForm);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("OrderForm"));
                    TableInit<OrderUpdateRecord>.Table_Reload(TalbeHelper.GetLoadPath("OrderUpdate"), OrderUpdate);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("OrderUpdate"));
                    TableInit<TradeRecord>.Table_Reload(TalbeHelper.GetLoadPath("Trade"), Trade);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Trade"));
                    TableInit<GemRecord>.Table_Reload(TalbeHelper.GetLoadPath("Gem"), Gem);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Gem"));
                    TableInit<GemGroupRecord>.Table_Reload(TalbeHelper.GetLoadPath("GemGroup"), GemGroup);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GemGroup"));
                    TableInit<SensitiveWordRecord>.Table_Reload(TalbeHelper.GetLoadPath("SensitiveWord"), SensitiveWord);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("SensitiveWord"));
                    TableInit<GuidanceRecord>.Table_Reload(TalbeHelper.GetLoadPath("Guidance"), Guidance);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Guidance"));
                    TableInit<MapTransferRecord>.Table_Reload(TalbeHelper.GetLoadPath("MapTransfer"), MapTransfer);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("MapTransfer"));
                    TableInit<RandomCoordinateRecord>.Table_Reload(TalbeHelper.GetLoadPath("RandomCoordinate"), RandomCoordinate);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("RandomCoordinate"));
                    TableInit<StepByStepRecord>.Table_Reload(TalbeHelper.GetLoadPath("StepByStep"), StepByStep);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("StepByStep"));
                    TableInit<WorldBOSSRecord>.Table_Reload(TalbeHelper.GetLoadPath("WorldBOSS"), WorldBOSS);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("WorldBOSS"));
                    TableInit<WorldBOSSAwardRecord>.Table_Reload(TalbeHelper.GetLoadPath("WorldBOSSAward"), WorldBOSSAward);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("WorldBOSSAward"));
                    TableInit<PKValueRecord>.Table_Reload(TalbeHelper.GetLoadPath("PKValue"), PKValue);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("PKValue"));
                    TableInit<TransmigrationRecord>.Table_Reload(TalbeHelper.GetLoadPath("Transmigration"), Transmigration);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Transmigration"));
                    TableInit<FubenInfoRecord>.Table_Reload(TalbeHelper.GetLoadPath("FubenInfo"), FubenInfo);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("FubenInfo"));
                    TableInit<FubenLogicRecord>.Table_Reload(TalbeHelper.GetLoadPath("FubenLogic"), FubenLogic);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("FubenLogic"));
                    TableInit<ServerNameRecord>.Table_Reload(TalbeHelper.GetLoadPath("ServerName"), ServerName);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ServerName"));
                    TableInit<GetMissionLevelRecord>.Table_Reload(TalbeHelper.GetLoadPath("GetMissionLevel"), GetMissionLevel);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GetMissionLevel"));
                    TableInit<GetMissionTypeRecord>.Table_Reload(TalbeHelper.GetLoadPath("GetMissionType"), GetMissionType);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GetMissionType"));
                    TableInit<GetMissionQulityRecord>.Table_Reload(TalbeHelper.GetLoadPath("GetMissionQulity"), GetMissionQulity);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GetMissionQulity"));
                    TableInit<GetPetCountRecord>.Table_Reload(TalbeHelper.GetLoadPath("GetPetCount"), GetPetCount);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GetPetCount"));
                    TableInit<GetMissionInfoRecord>.Table_Reload(TalbeHelper.GetLoadPath("GetMissionInfo"), GetMissionInfo);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GetMissionInfo"));
                    TableInit<GetMissionPlaceRecord>.Table_Reload(TalbeHelper.GetLoadPath("GetMissionPlace"), GetMissionPlace);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GetMissionPlace"));
                    TableInit<GetMissionWeatherRecord>.Table_Reload(TalbeHelper.GetLoadPath("GetMissionWeather"), GetMissionWeather);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GetMissionWeather"));
                    TableInit<GetMissionTimeLevelRecord>.Table_Reload(TalbeHelper.GetLoadPath("GetMissionTimeLevel"), GetMissionTimeLevel);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GetMissionTimeLevel"));
                    TableInit<MissionConditionInfoRecord>.Table_Reload(TalbeHelper.GetLoadPath("MissionConditionInfo"), MissionConditionInfo);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("MissionConditionInfo"));
                    TableInit<GetMissionRewardRecord>.Table_Reload(TalbeHelper.GetLoadPath("GetMissionReward"), GetMissionReward);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GetMissionReward"));
                    TableInit<GetPetTypeRecord>.Table_Reload(TalbeHelper.GetLoadPath("GetPetType"), GetPetType);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GetPetType"));
                    TableInit<SubjectRecord>.Table_Reload(TalbeHelper.GetLoadPath("Subject"), Subject);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Subject"));
                    TableInit<GetMissionNameRecord>.Table_Reload(TalbeHelper.GetLoadPath("GetMissionName"), GetMissionName);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GetMissionName"));
                    TableInit<DynamicActivityRecord>.Table_Reload(TalbeHelper.GetLoadPath("DynamicActivity"), DynamicActivity);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("DynamicActivity"));
                    TableInit<CompensationRecord>.Table_Reload(TalbeHelper.GetLoadPath("Compensation"), Compensation);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Compensation"));
                    TableInit<CityTalkRecord>.Table_Reload(TalbeHelper.GetLoadPath("CityTalk"), CityTalk);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("CityTalk"));
                    TableInit<DailyActivityRecord>.Table_Reload(TalbeHelper.GetLoadPath("DailyActivity"), DailyActivity);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("DailyActivity"));
                    TableInit<RechargeRecord>.Table_Reload(TalbeHelper.GetLoadPath("Recharge"), Recharge);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Recharge"));
                    TableInit<NameTitleRecord>.Table_Reload(TalbeHelper.GetLoadPath("NameTitle"), NameTitle);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("NameTitle"));
                    TableInit<VIPRecord>.Table_Reload(TalbeHelper.GetLoadPath("VIP"), VIP);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("VIP"));
                    TableInit<RechargeActiveRecord>.Table_Reload(TalbeHelper.GetLoadPath("RechargeActive"), RechargeActive);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("RechargeActive"));
                    TableInit<RechargeActiveNoticeRecord>.Table_Reload(TalbeHelper.GetLoadPath("RechargeActiveNotice"), RechargeActiveNotice);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("RechargeActiveNotice"));
                    TableInit<RechargeActiveInvestmentRecord>.Table_Reload(TalbeHelper.GetLoadPath("RechargeActiveInvestment"), RechargeActiveInvestment);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("RechargeActiveInvestment"));
                    TableInit<RechargeActiveInvestmentRewardRecord>.Table_Reload(TalbeHelper.GetLoadPath("RechargeActiveInvestmentReward"), RechargeActiveInvestmentReward);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("RechargeActiveInvestmentReward"));
                    TableInit<RechargeActiveCumulativeRecord>.Table_Reload(TalbeHelper.GetLoadPath("RechargeActiveCumulative"), RechargeActiveCumulative);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("RechargeActiveCumulative"));
                    TableInit<RechargeActiveCumulativeRewardRecord>.Table_Reload(TalbeHelper.GetLoadPath("RechargeActiveCumulativeReward"), RechargeActiveCumulativeReward);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("RechargeActiveCumulativeReward"));
                    TableInit<GiftCodeRecord>.Table_Reload(TalbeHelper.GetLoadPath("GiftCode"), GiftCode);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GiftCode"));
                    TableInit<GMCommandRecord>.Table_Reload(TalbeHelper.GetLoadPath("GMCommand"), GMCommand);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GMCommand"));
                    TableInit<AuctionType1Record>.Table_Reload(TalbeHelper.GetLoadPath("AuctionType1"), AuctionType1);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("AuctionType1"));
                    TableInit<AuctionType2Record>.Table_Reload(TalbeHelper.GetLoadPath("AuctionType2"), AuctionType2);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("AuctionType2"));
                    TableInit<AuctionType3Record>.Table_Reload(TalbeHelper.GetLoadPath("AuctionType3"), AuctionType3);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("AuctionType3"));
                    TableInit<YunYingRecord>.Table_Reload(TalbeHelper.GetLoadPath("YunYing"), YunYing);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("YunYing"));
                    TableInit<OperationActivityRecord>.Table_Reload(TalbeHelper.GetLoadPath("OperationActivity"), OperationActivity);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("OperationActivity"));
                    TableInit<FirstRechargeRecord>.Table_Reload(TalbeHelper.GetLoadPath("FirstRecharge"), FirstRecharge);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("FirstRecharge"));
                    TableInit<MieShiRecord>.Table_Reload(TalbeHelper.GetLoadPath("MieShi"), MieShi);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("MieShi"));
                    TableInit<MieShiPublicRecord>.Table_Reload(TalbeHelper.GetLoadPath("MieShiPublic"), MieShiPublic);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("MieShiPublic"));
                    TableInit<DefendCityRewardRecord>.Table_Reload(TalbeHelper.GetLoadPath("DefendCityReward"), DefendCityReward);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("DefendCityReward"));
                    TableInit<DefendCityDevoteRewardRecord>.Table_Reload(TalbeHelper.GetLoadPath("DefendCityDevoteReward"), DefendCityDevoteReward);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("DefendCityDevoteReward"));
                    TableInit<BatteryLevelRecord>.Table_Reload(TalbeHelper.GetLoadPath("BatteryLevel"), BatteryLevel);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("BatteryLevel"));
                    TableInit<BatteryBaseRecord>.Table_Reload(TalbeHelper.GetLoadPath("BatteryBase"), BatteryBase);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("BatteryBase"));
                    TableInit<PlayerDropRecord>.Table_Reload(TalbeHelper.GetLoadPath("PlayerDrop"), PlayerDrop);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("PlayerDrop"));
                    TableInit<BuffGroupRecord>.Table_Reload(TalbeHelper.GetLoadPath("BuffGroup"), BuffGroup);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("BuffGroup"));
                    TableInit<BangBuffRecord>.Table_Reload(TalbeHelper.GetLoadPath("BangBuff"), BangBuff);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("BangBuff"));
                    TableInit<MieshiTowerRewardRecord>.Table_Reload(TalbeHelper.GetLoadPath("MieshiTowerReward"), MieshiTowerReward);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("MieshiTowerReward"));
                    TableInit<ClimbingTowerRecord>.Table_Reload(TalbeHelper.GetLoadPath("ClimbingTower"), ClimbingTower);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ClimbingTower"));
                    TableInit<AcientBattleFieldRecord>.Table_Reload(TalbeHelper.GetLoadPath("AcientBattleField"), AcientBattleField);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("AcientBattleField"));
                    TableInit<ConsumArrayRecord>.Table_Reload(TalbeHelper.GetLoadPath("ConsumArray"), ConsumArray);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ConsumArray"));
                    TableInit<BookBaseRecord>.Table_Reload(TalbeHelper.GetLoadPath("BookBase"), BookBase);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("BookBase"));
                    TableInit<MountRecord>.Table_Reload(TalbeHelper.GetLoadPath("Mount"), Mount);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Mount"));
                    TableInit<MountSkillRecord>.Table_Reload(TalbeHelper.GetLoadPath("MountSkill"), MountSkill);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("MountSkill"));
                    TableInit<MountFeedRecord>.Table_Reload(TalbeHelper.GetLoadPath("MountFeed"), MountFeed);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("MountFeed"));
                    TableInit<MayaBaseRecord>.Table_Reload(TalbeHelper.GetLoadPath("MayaBase"), MayaBase);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("MayaBase"));
                    TableInit<OfflineExperienceRecord>.Table_Reload(TalbeHelper.GetLoadPath("OfflineExperience"), OfflineExperience);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("OfflineExperience"));
                    TableInit<SurveyRecord>.Table_Reload(TalbeHelper.GetLoadPath("Survey"), Survey);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Survey"));
                    TableInit<PreferentialRecord>.Table_Reload(TalbeHelper.GetLoadPath("Preferential"), Preferential);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Preferential"));
                    TableInit<KaiFuRecord>.Table_Reload(TalbeHelper.GetLoadPath("KaiFu"), KaiFu);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("KaiFu"));
                    TableInit<BossHomeRecord>.Table_Reload(TalbeHelper.GetLoadPath("BossHome"), BossHome);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("BossHome"));
                    TableInit<WarFlagRecord>.Table_Reload(TalbeHelper.GetLoadPath("WarFlag"), WarFlag);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("WarFlag"));
                    TableInit<LodeRecord>.Table_Reload(TalbeHelper.GetLoadPath("Lode"), Lode);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Lode"));
                    TableInit<MainActivityRecord>.Table_Reload(TalbeHelper.GetLoadPath("MainActivity"), MainActivity);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("MainActivity"));
                    TableInit<ObjectTableRecord>.Table_Reload(TalbeHelper.GetLoadPath("ObjectTable"), ObjectTable);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ObjectTable"));
                    TableInit<BatteryBaseNewRecord>.Table_Reload(TalbeHelper.GetLoadPath("BatteryBaseNew"), BatteryBaseNew);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("BatteryBaseNew"));
                    TableInit<CheckenRecord>.Table_Reload(TalbeHelper.GetLoadPath("Checken"), Checken);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Checken"));
                    TableInit<CheckenLvRecord>.Table_Reload(TalbeHelper.GetLoadPath("CheckenLv"), CheckenLv);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("CheckenLv"));
                    TableInit<CheckenRewardRecord>.Table_Reload(TalbeHelper.GetLoadPath("CheckenReward"), CheckenReward);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("CheckenReward"));
                    TableInit<CheckenFinalRewardRecord>.Table_Reload(TalbeHelper.GetLoadPath("CheckenFinalReward"), CheckenFinalReward);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("CheckenFinalReward"));
                    TableInit<SuperVipRecord>.Table_Reload(TalbeHelper.GetLoadPath("SuperVip"), SuperVip);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("SuperVip"));
                    TableInit<IosMutiplePlatformRecord>.Table_Reload(TalbeHelper.GetLoadPath("IosMutiplePlatform"), IosMutiplePlatform);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("IosMutiplePlatform"));
                    TableInit<BattleCorrectRecord>.Table_Reload(TalbeHelper.GetLoadPath("BattleCorrect"), BattleCorrect);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("BattleCorrect"));
                    TableInit<FashionTitleRecord>.Table_Reload(TalbeHelper.GetLoadPath("FashionTitle"), FashionTitle);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("FashionTitle"));
                return;
            }
            switch (tableName)
            {
                case "ConditionTable":
                    TableInit<ConditionTableRecord>.Table_Reload(TalbeHelper.GetLoadPath("ConditionTable"), ConditionTable);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ConditionTable"));
                    break;
                case "Flag":
                    TableInit<FlagRecord>.Table_Reload(TalbeHelper.GetLoadPath("Flag"), Flag);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Flag"));
                    break;
                case "Exdata":
                    TableInit<ExdataRecord>.Table_Reload(TalbeHelper.GetLoadPath("Exdata"), Exdata);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Exdata"));
                    break;
                case "Dictionary":
                    TableInit<DictionaryRecord>.Table_Reload(TalbeHelper.GetLoadPath("Dictionary"), Dictionary);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Dictionary"));
                    break;
                case "BagBase":
                    TableInit<BagBaseRecord>.Table_Reload(TalbeHelper.GetLoadPath("BagBase"), BagBase);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("BagBase"));
                    break;
                case "ItemBase":
                    TableInit<ItemBaseRecord>.Table_Reload(TalbeHelper.GetLoadPath("ItemBase"), ItemBase);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ItemBase"));
                    break;
                case "ItemType":
                    TableInit<ItemTypeRecord>.Table_Reload(TalbeHelper.GetLoadPath("ItemType"), ItemType);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ItemType"));
                    break;
                case "ColorBase":
                    TableInit<ColorBaseRecord>.Table_Reload(TalbeHelper.GetLoadPath("ColorBase"), ColorBase);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ColorBase"));
                    break;
                case "Buff":
                    TableInit<BuffRecord>.Table_Reload(TalbeHelper.GetLoadPath("Buff"), Buff);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Buff"));
                    break;
                case "MissionBase":
                    TableInit<MissionRecord>.Table_Reload(TalbeHelper.GetLoadPath("MissionBase"), Mission);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("MissionBase"));
                    break;
                case "CharacterBase":
                    TableInit<CharacterBaseRecord>.Table_Reload(TalbeHelper.GetLoadPath("CharacterBase"), CharacterBase);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("CharacterBase"));
                    break;
                case "Actor":
                    TableInit<ActorRecord>.Table_Reload(TalbeHelper.GetLoadPath("Actor"), Actor);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Actor"));
                    break;
                case "AttrRef":
                    TableInit<AttrRefRecord>.Table_Reload(TalbeHelper.GetLoadPath("AttrRef"), AttrRef);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("AttrRef"));
                    break;
                case "EquipBase":
                    TableInit<EquipRecord>.Table_Reload(TalbeHelper.GetLoadPath("EquipBase"), Equip);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("EquipBase"));
                    break;
                case "EquipRelate":
                    TableInit<EquipRelateRecord>.Table_Reload(TalbeHelper.GetLoadPath("EquipRelate"), EquipRelate);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("EquipRelate"));
                    break;
                case "EquipEnchant":
                    TableInit<EquipEnchantRecord>.Table_Reload(TalbeHelper.GetLoadPath("EquipEnchant"), EquipEnchant);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("EquipEnchant"));
                    break;
                case "EquipEnchantChance":
                    TableInit<EquipEnchantChanceRecord>.Table_Reload(TalbeHelper.GetLoadPath("EquipEnchantChance"), EquipEnchantChance);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("EquipEnchantChance"));
                    break;
                case "Title":
                    TableInit<TitleRecord>.Table_Reload(TalbeHelper.GetLoadPath("Title"), Title);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Title"));
                    break;
                case "EquipEnchance":
                    TableInit<EquipEnchanceRecord>.Table_Reload(TalbeHelper.GetLoadPath("EquipEnchance"), EquipEnchance);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("EquipEnchance"));
                    break;
                case "LevelData":
                    TableInit<LevelDataRecord>.Table_Reload(TalbeHelper.GetLoadPath("LevelData"), LevelData);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("LevelData"));
                    break;
                case "SceneNpc":
                    TableInit<SceneNpcRecord>.Table_Reload(TalbeHelper.GetLoadPath("SceneNpc"), SceneNpc);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("SceneNpc"));
                    break;
                case "Skill":
                    TableInit<SkillRecord>.Table_Reload(TalbeHelper.GetLoadPath("Skill"), Skill);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Skill"));
                    break;
                case "Bullet":
                    TableInit<BulletRecord>.Table_Reload(TalbeHelper.GetLoadPath("Bullet"), Bullet);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Bullet"));
                    break;
                case "Scene":
                    TableInit<SceneRecord>.Table_Reload(TalbeHelper.GetLoadPath("Scene"), Scene);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Scene"));
                    break;
                case "Talent":
                    TableInit<TalentRecord>.Table_Reload(TalbeHelper.GetLoadPath("Talent"), Talent);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Talent"));
                    break;
                case "NpcBase":
                    TableInit<NpcBaseRecord>.Table_Reload(TalbeHelper.GetLoadPath("NpcBase"), NpcBase);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("NpcBase"));
                    break;
                case "SkillUpgrading":
                    TableInit<SkillUpgradingRecord>.Table_Reload(TalbeHelper.GetLoadPath("SkillUpgrading"), SkillUpgrading);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("SkillUpgrading"));
                    break;
                case "Achievement":
                    TableInit<AchievementRecord>.Table_Reload(TalbeHelper.GetLoadPath("Achievement"), Achievement);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Achievement"));
                    break;
                case "Script":
                    TableInit<ScriptRecord>.Table_Reload(TalbeHelper.GetLoadPath("Script"), Script);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Script"));
                    break;
                case "DropMother":
                    TableInit<DropMotherRecord>.Table_Reload(TalbeHelper.GetLoadPath("DropMother"), DropMother);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("DropMother"));
                    break;
                case "DropSon":
                    TableInit<DropSonRecord>.Table_Reload(TalbeHelper.GetLoadPath("DropSon"), DropSon);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("DropSon"));
                    break;
                case "EquipTie":
                    TableInit<EquipTieRecord>.Table_Reload(TalbeHelper.GetLoadPath("EquipTie"), EquipTie);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("EquipTie"));
                    break;
                case "Transfer":
                    TableInit<TransferRecord>.Table_Reload(TalbeHelper.GetLoadPath("Transfer"), Transfer);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Transfer"));
                    break;
                case "ServerConfig":
                    TableInit<ServerConfigRecord>.Table_Reload(TalbeHelper.GetLoadPath("ServerConfig"), ServerConfig);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ServerConfig"));
                    break;
                case "AI":
                    TableInit<AIRecord>.Table_Reload(TalbeHelper.GetLoadPath("AI"), AI);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("AI"));
                    break;
                case "Event":
                    TableInit<EventRecord>.Table_Reload(TalbeHelper.GetLoadPath("Event"), Event);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Event"));
                    break;
                case "DropConfig":
                    TableInit<DropConfigRecord>.Table_Reload(TalbeHelper.GetLoadPath("DropConfig"), DropConfig);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("DropConfig"));
                    break;
                case "Gift":
                    TableInit<GiftRecord>.Table_Reload(TalbeHelper.GetLoadPath("Gift"), Gift);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Gift"));
                    break;
                case "EquipBlessing":
                    TableInit<EquipBlessingRecord>.Table_Reload(TalbeHelper.GetLoadPath("EquipBlessing"), EquipBlessing);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("EquipBlessing"));
                    break;
                case "EquipAdditional":
                    TableInit<EquipAdditionalRecord>.Table_Reload(TalbeHelper.GetLoadPath("EquipAdditional"), EquipAdditional);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("EquipAdditional"));
                    break;
                case "EquipExcellent":
                    TableInit<EquipExcellentRecord>.Table_Reload(TalbeHelper.GetLoadPath("EquipExcellent"), EquipExcellent);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("EquipExcellent"));
                    break;
                case "HandBook":
                    TableInit<HandBookRecord>.Table_Reload(TalbeHelper.GetLoadPath("HandBook"), HandBook);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("HandBook"));
                    break;
                case "BookGroup":
                    TableInit<BookGroupRecord>.Table_Reload(TalbeHelper.GetLoadPath("BookGroup"), BookGroup);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("BookGroup"));
                    break;
                case "ItemCompose":
                    TableInit<ItemComposeRecord>.Table_Reload(TalbeHelper.GetLoadPath("ItemCompose"), ItemCompose);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ItemCompose"));
                    break;
                case "Camp":
                    TableInit<CampRecord>.Table_Reload(TalbeHelper.GetLoadPath("Camp"), Camp);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Camp"));
                    break;
                case "Fuben":
                    TableInit<FubenRecord>.Table_Reload(TalbeHelper.GetLoadPath("Fuben"), Fuben);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Fuben"));
                    break;
                case "SkillArea":
                    TableInit<SkillAreaRecord>.Table_Reload(TalbeHelper.GetLoadPath("SkillArea"), SkillArea);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("SkillArea"));
                    break;
                case "Stats":
                    TableInit<StatsRecord>.Table_Reload(TalbeHelper.GetLoadPath("Stats"), Stats);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Stats"));
                    break;
                case "Store":
                    TableInit<StoreRecord>.Table_Reload(TalbeHelper.GetLoadPath("Store"), Store);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Store"));
                    break;
                case "InitItem":
                    TableInit<InitItemRecord>.Table_Reload(TalbeHelper.GetLoadPath("InitItem"), InitItem);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("InitItem"));
                    break;
                case "TriggerArea":
                    TableInit<TriggerAreaRecord>.Table_Reload(TalbeHelper.GetLoadPath("TriggerArea"), TriggerArea);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("TriggerArea"));
                    break;
                case "Mail":
                    TableInit<MailRecord>.Table_Reload(TalbeHelper.GetLoadPath("Mail"), Mail);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Mail"));
                    break;
                case "Building":
                    TableInit<BuildingRecord>.Table_Reload(TalbeHelper.GetLoadPath("Building"), Building);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Building"));
                    break;
                case "BuildingRule":
                    TableInit<BuildingRuleRecord>.Table_Reload(TalbeHelper.GetLoadPath("BuildingRule"), BuildingRule);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("BuildingRule"));
                    break;
                case "BuildingService":
                    TableInit<BuildingServiceRecord>.Table_Reload(TalbeHelper.GetLoadPath("BuildingService"), BuildingService);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("BuildingService"));
                    break;
                case "HomeSence":
                    TableInit<HomeSenceRecord>.Table_Reload(TalbeHelper.GetLoadPath("HomeSence"), HomeSence);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("HomeSence"));
                    break;
                case "Pet":
                    TableInit<PetRecord>.Table_Reload(TalbeHelper.GetLoadPath("Pet"), Pet);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Pet"));
                    break;
                case "PetSkill":
                    TableInit<PetSkillRecord>.Table_Reload(TalbeHelper.GetLoadPath("PetSkill"), PetSkill);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("PetSkill"));
                    break;
                case "Service":
                    TableInit<ServiceRecord>.Table_Reload(TalbeHelper.GetLoadPath("Service"), Service);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Service"));
                    break;
                case "StoreType":
                    TableInit<StoreTypeRecord>.Table_Reload(TalbeHelper.GetLoadPath("StoreType"), StoreType);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("StoreType"));
                    break;
                case "PetSkillBase":
                    TableInit<PetSkillBaseRecord>.Table_Reload(TalbeHelper.GetLoadPath("PetSkillBase"), PetSkillBase);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("PetSkillBase"));
                    break;
                case "Elf":
                    TableInit<ElfRecord>.Table_Reload(TalbeHelper.GetLoadPath("Elf"), Elf);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Elf"));
                    break;
                case "ElfGroup":
                    TableInit<ElfGroupRecord>.Table_Reload(TalbeHelper.GetLoadPath("ElfGroup"), ElfGroup);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ElfGroup"));
                    break;
                case "Queue":
                    TableInit<QueueRecord>.Table_Reload(TalbeHelper.GetLoadPath("Queue"), Queue);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Queue"));
                    break;
                case "Draw":
                    TableInit<DrawRecord>.Table_Reload(TalbeHelper.GetLoadPath("Draw"), Draw);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Draw"));
                    break;
                case "Plant":
                    TableInit<PlantRecord>.Table_Reload(TalbeHelper.GetLoadPath("Plant"), Plant);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Plant"));
                    break;
                case "Medal":
                    TableInit<MedalRecord>.Table_Reload(TalbeHelper.GetLoadPath("Medal"), Medal);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Medal"));
                    break;
                case "Sailing":
                    TableInit<SailingRecord>.Table_Reload(TalbeHelper.GetLoadPath("Sailing"), Sailing);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Sailing"));
                    break;
                case "WingTrain":
                    TableInit<WingTrainRecord>.Table_Reload(TalbeHelper.GetLoadPath("WingTrain"), WingTrain);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("WingTrain"));
                    break;
                case "WingQuality":
                    TableInit<WingQualityRecord>.Table_Reload(TalbeHelper.GetLoadPath("WingQuality"), WingQuality);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("WingQuality"));
                    break;
                case "PVPRule":
                    TableInit<PVPRuleRecord>.Table_Reload(TalbeHelper.GetLoadPath("PVPRule"), PVPRule);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("PVPRule"));
                    break;
                case "ArenaReward":
                    TableInit<ArenaRewardRecord>.Table_Reload(TalbeHelper.GetLoadPath("ArenaReward"), ArenaReward);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ArenaReward"));
                    break;
                case "ArenaLevel":
                    TableInit<ArenaLevelRecord>.Table_Reload(TalbeHelper.GetLoadPath("ArenaLevel"), ArenaLevel);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ArenaLevel"));
                    break;
                case "Honor":
                    TableInit<HonorRecord>.Table_Reload(TalbeHelper.GetLoadPath("Honor"), Honor);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Honor"));
                    break;
                case "JJCRoot":
                    TableInit<JJCRootRecord>.Table_Reload(TalbeHelper.GetLoadPath("JJCRoot"), JJCRoot);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("JJCRoot"));
                    break;
                case "Statue":
                    TableInit<StatueRecord>.Table_Reload(TalbeHelper.GetLoadPath("Statue"), Statue);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Statue"));
                    break;
                case "EquipAdditional1":
                    TableInit<EquipAdditional1Record>.Table_Reload(TalbeHelper.GetLoadPath("EquipAdditional1"), EquipAdditional1);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("EquipAdditional1"));
                    break;
                case "Guild":
                    TableInit<GuildRecord>.Table_Reload(TalbeHelper.GetLoadPath("Guild"), Guild);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Guild"));
                    break;
                case "GuildBuff":
                    TableInit<GuildBuffRecord>.Table_Reload(TalbeHelper.GetLoadPath("GuildBuff"), GuildBuff);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GuildBuff"));
                    break;
                case "GuildBoss":
                    TableInit<GuildBossRecord>.Table_Reload(TalbeHelper.GetLoadPath("GuildBoss"), GuildBoss);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GuildBoss"));
                    break;
                case "GuildAccess":
                    TableInit<GuildAccessRecord>.Table_Reload(TalbeHelper.GetLoadPath("GuildAccess"), GuildAccess);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GuildAccess"));
                    break;
                case "ExpInfo":
                    TableInit<ExpInfoRecord>.Table_Reload(TalbeHelper.GetLoadPath("ExpInfo"), ExpInfo);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ExpInfo"));
                    break;
                case "GroupShop":
                    TableInit<GroupShopRecord>.Table_Reload(TalbeHelper.GetLoadPath("GroupShop"), GroupShop);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GroupShop"));
                    break;
                case "GroupShopUpdate":
                    TableInit<GroupShopUpdateRecord>.Table_Reload(TalbeHelper.GetLoadPath("GroupShopUpdate"), GroupShopUpdate);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GroupShopUpdate"));
                    break;
                case "PKMode":
                    TableInit<PKModeRecord>.Table_Reload(TalbeHelper.GetLoadPath("PKMode"), PKMode);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("PKMode"));
                    break;
                case "forged":
                    TableInit<forgedRecord>.Table_Reload(TalbeHelper.GetLoadPath("forged"), forged);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("forged"));
                    break;
                case "EquipUpdate":
                    TableInit<EquipUpdateRecord>.Table_Reload(TalbeHelper.GetLoadPath("EquipUpdate"), EquipUpdate);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("EquipUpdate"));
                    break;
                case "GuildMission":
                    TableInit<GuildMissionRecord>.Table_Reload(TalbeHelper.GetLoadPath("GuildMission"), GuildMission);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GuildMission"));
                    break;
                case "OrderForm":
                    TableInit<OrderFormRecord>.Table_Reload(TalbeHelper.GetLoadPath("OrderForm"), OrderForm);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("OrderForm"));
                    break;
                case "OrderUpdate":
                    TableInit<OrderUpdateRecord>.Table_Reload(TalbeHelper.GetLoadPath("OrderUpdate"), OrderUpdate);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("OrderUpdate"));
                    break;
                case "Trade":
                    TableInit<TradeRecord>.Table_Reload(TalbeHelper.GetLoadPath("Trade"), Trade);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Trade"));
                    break;
                case "Gem":
                    TableInit<GemRecord>.Table_Reload(TalbeHelper.GetLoadPath("Gem"), Gem);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Gem"));
                    break;
                case "GemGroup":
                    TableInit<GemGroupRecord>.Table_Reload(TalbeHelper.GetLoadPath("GemGroup"), GemGroup);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GemGroup"));
                    break;
                case "SensitiveWord":
                    TableInit<SensitiveWordRecord>.Table_Reload(TalbeHelper.GetLoadPath("SensitiveWord"), SensitiveWord);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("SensitiveWord"));
                    break;
                case "Guidance":
                    TableInit<GuidanceRecord>.Table_Reload(TalbeHelper.GetLoadPath("Guidance"), Guidance);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Guidance"));
                    break;
                case "MapTransfer":
                    TableInit<MapTransferRecord>.Table_Reload(TalbeHelper.GetLoadPath("MapTransfer"), MapTransfer);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("MapTransfer"));
                    break;
                case "RandomCoordinate":
                    TableInit<RandomCoordinateRecord>.Table_Reload(TalbeHelper.GetLoadPath("RandomCoordinate"), RandomCoordinate);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("RandomCoordinate"));
                    break;
                case "StepByStep":
                    TableInit<StepByStepRecord>.Table_Reload(TalbeHelper.GetLoadPath("StepByStep"), StepByStep);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("StepByStep"));
                    break;
                case "WorldBOSS":
                    TableInit<WorldBOSSRecord>.Table_Reload(TalbeHelper.GetLoadPath("WorldBOSS"), WorldBOSS);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("WorldBOSS"));
                    break;
                case "WorldBOSSAward":
                    TableInit<WorldBOSSAwardRecord>.Table_Reload(TalbeHelper.GetLoadPath("WorldBOSSAward"), WorldBOSSAward);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("WorldBOSSAward"));
                    break;
                case "PKValue":
                    TableInit<PKValueRecord>.Table_Reload(TalbeHelper.GetLoadPath("PKValue"), PKValue);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("PKValue"));
                    break;
                case "Transmigration":
                    TableInit<TransmigrationRecord>.Table_Reload(TalbeHelper.GetLoadPath("Transmigration"), Transmigration);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Transmigration"));
                    break;
                case "FubenInfo":
                    TableInit<FubenInfoRecord>.Table_Reload(TalbeHelper.GetLoadPath("FubenInfo"), FubenInfo);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("FubenInfo"));
                    break;
                case "FubenLogic":
                    TableInit<FubenLogicRecord>.Table_Reload(TalbeHelper.GetLoadPath("FubenLogic"), FubenLogic);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("FubenLogic"));
                    break;
                case "ServerName":
                    TableInit<ServerNameRecord>.Table_Reload(TalbeHelper.GetLoadPath("ServerName"), ServerName);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ServerName"));
                    break;
                case "GetMissionLevel":
                    TableInit<GetMissionLevelRecord>.Table_Reload(TalbeHelper.GetLoadPath("GetMissionLevel"), GetMissionLevel);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GetMissionLevel"));
                    break;
                case "GetMissionType":
                    TableInit<GetMissionTypeRecord>.Table_Reload(TalbeHelper.GetLoadPath("GetMissionType"), GetMissionType);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GetMissionType"));
                    break;
                case "GetMissionQulity":
                    TableInit<GetMissionQulityRecord>.Table_Reload(TalbeHelper.GetLoadPath("GetMissionQulity"), GetMissionQulity);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GetMissionQulity"));
                    break;
                case "GetPetCount":
                    TableInit<GetPetCountRecord>.Table_Reload(TalbeHelper.GetLoadPath("GetPetCount"), GetPetCount);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GetPetCount"));
                    break;
                case "GetMissionInfo":
                    TableInit<GetMissionInfoRecord>.Table_Reload(TalbeHelper.GetLoadPath("GetMissionInfo"), GetMissionInfo);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GetMissionInfo"));
                    break;
                case "GetMissionPlace":
                    TableInit<GetMissionPlaceRecord>.Table_Reload(TalbeHelper.GetLoadPath("GetMissionPlace"), GetMissionPlace);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GetMissionPlace"));
                    break;
                case "GetMissionWeather":
                    TableInit<GetMissionWeatherRecord>.Table_Reload(TalbeHelper.GetLoadPath("GetMissionWeather"), GetMissionWeather);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GetMissionWeather"));
                    break;
                case "GetMissionTimeLevel":
                    TableInit<GetMissionTimeLevelRecord>.Table_Reload(TalbeHelper.GetLoadPath("GetMissionTimeLevel"), GetMissionTimeLevel);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GetMissionTimeLevel"));
                    break;
                case "MissionConditionInfo":
                    TableInit<MissionConditionInfoRecord>.Table_Reload(TalbeHelper.GetLoadPath("MissionConditionInfo"), MissionConditionInfo);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("MissionConditionInfo"));
                    break;
                case "GetMissionReward":
                    TableInit<GetMissionRewardRecord>.Table_Reload(TalbeHelper.GetLoadPath("GetMissionReward"), GetMissionReward);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GetMissionReward"));
                    break;
                case "GetPetType":
                    TableInit<GetPetTypeRecord>.Table_Reload(TalbeHelper.GetLoadPath("GetPetType"), GetPetType);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GetPetType"));
                    break;
                case "Subject":
                    TableInit<SubjectRecord>.Table_Reload(TalbeHelper.GetLoadPath("Subject"), Subject);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Subject"));
                    break;
                case "GetMissionName":
                    TableInit<GetMissionNameRecord>.Table_Reload(TalbeHelper.GetLoadPath("GetMissionName"), GetMissionName);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GetMissionName"));
                    break;
                case "DynamicActivity":
                    TableInit<DynamicActivityRecord>.Table_Reload(TalbeHelper.GetLoadPath("DynamicActivity"), DynamicActivity);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("DynamicActivity"));
                    break;
                case "Compensation":
                    TableInit<CompensationRecord>.Table_Reload(TalbeHelper.GetLoadPath("Compensation"), Compensation);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Compensation"));
                    break;
                case "CityTalk":
                    TableInit<CityTalkRecord>.Table_Reload(TalbeHelper.GetLoadPath("CityTalk"), CityTalk);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("CityTalk"));
                    break;
                case "DailyActivity":
                    TableInit<DailyActivityRecord>.Table_Reload(TalbeHelper.GetLoadPath("DailyActivity"), DailyActivity);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("DailyActivity"));
                    break;
                case "Recharge":
                    TableInit<RechargeRecord>.Table_Reload(TalbeHelper.GetLoadPath("Recharge"), Recharge);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Recharge"));
                    break;
                case "NameTitle":
                    TableInit<NameTitleRecord>.Table_Reload(TalbeHelper.GetLoadPath("NameTitle"), NameTitle);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("NameTitle"));
                    break;
                case "VIP":
                    TableInit<VIPRecord>.Table_Reload(TalbeHelper.GetLoadPath("VIP"), VIP);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("VIP"));
                    break;
                case "RechargeActive":
                    TableInit<RechargeActiveRecord>.Table_Reload(TalbeHelper.GetLoadPath("RechargeActive"), RechargeActive);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("RechargeActive"));
                    break;
                case "RechargeActiveNotice":
                    TableInit<RechargeActiveNoticeRecord>.Table_Reload(TalbeHelper.GetLoadPath("RechargeActiveNotice"), RechargeActiveNotice);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("RechargeActiveNotice"));
                    break;
                case "RechargeActiveInvestment":
                    TableInit<RechargeActiveInvestmentRecord>.Table_Reload(TalbeHelper.GetLoadPath("RechargeActiveInvestment"), RechargeActiveInvestment);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("RechargeActiveInvestment"));
                    break;
                case "RechargeActiveInvestmentReward":
                    TableInit<RechargeActiveInvestmentRewardRecord>.Table_Reload(TalbeHelper.GetLoadPath("RechargeActiveInvestmentReward"), RechargeActiveInvestmentReward);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("RechargeActiveInvestmentReward"));
                    break;
                case "RechargeActiveCumulative":
                    TableInit<RechargeActiveCumulativeRecord>.Table_Reload(TalbeHelper.GetLoadPath("RechargeActiveCumulative"), RechargeActiveCumulative);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("RechargeActiveCumulative"));
                    break;
                case "RechargeActiveCumulativeReward":
                    TableInit<RechargeActiveCumulativeRewardRecord>.Table_Reload(TalbeHelper.GetLoadPath("RechargeActiveCumulativeReward"), RechargeActiveCumulativeReward);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("RechargeActiveCumulativeReward"));
                    break;
                case "GiftCode":
                    TableInit<GiftCodeRecord>.Table_Reload(TalbeHelper.GetLoadPath("GiftCode"), GiftCode);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GiftCode"));
                    break;
                case "GMCommand":
                    TableInit<GMCommandRecord>.Table_Reload(TalbeHelper.GetLoadPath("GMCommand"), GMCommand);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("GMCommand"));
                    break;
                case "AuctionType1":
                    TableInit<AuctionType1Record>.Table_Reload(TalbeHelper.GetLoadPath("AuctionType1"), AuctionType1);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("AuctionType1"));
                    break;
                case "AuctionType2":
                    TableInit<AuctionType2Record>.Table_Reload(TalbeHelper.GetLoadPath("AuctionType2"), AuctionType2);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("AuctionType2"));
                    break;
                case "AuctionType3":
                    TableInit<AuctionType3Record>.Table_Reload(TalbeHelper.GetLoadPath("AuctionType3"), AuctionType3);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("AuctionType3"));
                    break;
                case "YunYing":
                    TableInit<YunYingRecord>.Table_Reload(TalbeHelper.GetLoadPath("YunYing"), YunYing);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("YunYing"));
                    break;
                case "OperationActivity":
                    TableInit<OperationActivityRecord>.Table_Reload(TalbeHelper.GetLoadPath("OperationActivity"), OperationActivity);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("OperationActivity"));
                    break;
                case "FirstRecharge":
                    TableInit<FirstRechargeRecord>.Table_Reload(TalbeHelper.GetLoadPath("FirstRecharge"), FirstRecharge);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("FirstRecharge"));
                    break;
                case "MieShi":
                    TableInit<MieShiRecord>.Table_Reload(TalbeHelper.GetLoadPath("MieShi"), MieShi);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("MieShi"));
                    break;
                case "MieShiPublic":
                    TableInit<MieShiPublicRecord>.Table_Reload(TalbeHelper.GetLoadPath("MieShiPublic"), MieShiPublic);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("MieShiPublic"));
                    break;
                case "DefendCityReward":
                    TableInit<DefendCityRewardRecord>.Table_Reload(TalbeHelper.GetLoadPath("DefendCityReward"), DefendCityReward);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("DefendCityReward"));
                    break;
                case "DefendCityDevoteReward":
                    TableInit<DefendCityDevoteRewardRecord>.Table_Reload(TalbeHelper.GetLoadPath("DefendCityDevoteReward"), DefendCityDevoteReward);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("DefendCityDevoteReward"));
                    break;
                case "BatteryLevel":
                    TableInit<BatteryLevelRecord>.Table_Reload(TalbeHelper.GetLoadPath("BatteryLevel"), BatteryLevel);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("BatteryLevel"));
                    break;
                case "BatteryBase":
                    TableInit<BatteryBaseRecord>.Table_Reload(TalbeHelper.GetLoadPath("BatteryBase"), BatteryBase);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("BatteryBase"));
                    break;
                case "PlayerDrop":
                    TableInit<PlayerDropRecord>.Table_Reload(TalbeHelper.GetLoadPath("PlayerDrop"), PlayerDrop);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("PlayerDrop"));
                    break;
                case "BuffGroup":
                    TableInit<BuffGroupRecord>.Table_Reload(TalbeHelper.GetLoadPath("BuffGroup"), BuffGroup);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("BuffGroup"));
                    break;
                case "BangBuff":
                    TableInit<BangBuffRecord>.Table_Reload(TalbeHelper.GetLoadPath("BangBuff"), BangBuff);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("BangBuff"));
                    break;
                case "MieshiTowerReward":
                    TableInit<MieshiTowerRewardRecord>.Table_Reload(TalbeHelper.GetLoadPath("MieshiTowerReward"), MieshiTowerReward);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("MieshiTowerReward"));
                    break;
                case "ClimbingTower":
                    TableInit<ClimbingTowerRecord>.Table_Reload(TalbeHelper.GetLoadPath("ClimbingTower"), ClimbingTower);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ClimbingTower"));
                    break;
                case "AcientBattleField":
                    TableInit<AcientBattleFieldRecord>.Table_Reload(TalbeHelper.GetLoadPath("AcientBattleField"), AcientBattleField);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("AcientBattleField"));
                    break;
                case "ConsumArray":
                    TableInit<ConsumArrayRecord>.Table_Reload(TalbeHelper.GetLoadPath("ConsumArray"), ConsumArray);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ConsumArray"));
                    break;
                case "BookBase":
                    TableInit<BookBaseRecord>.Table_Reload(TalbeHelper.GetLoadPath("BookBase"), BookBase);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("BookBase"));
                    break;
                case "Mount":
                    TableInit<MountRecord>.Table_Reload(TalbeHelper.GetLoadPath("Mount"), Mount);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Mount"));
                    break;
                case "MountSkill":
                    TableInit<MountSkillRecord>.Table_Reload(TalbeHelper.GetLoadPath("MountSkill"), MountSkill);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("MountSkill"));
                    break;
                case "MountFeed":
                    TableInit<MountFeedRecord>.Table_Reload(TalbeHelper.GetLoadPath("MountFeed"), MountFeed);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("MountFeed"));
                    break;
                case "MayaBase":
                    TableInit<MayaBaseRecord>.Table_Reload(TalbeHelper.GetLoadPath("MayaBase"), MayaBase);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("MayaBase"));
                    break;
                case "OfflineExperience":
                    TableInit<OfflineExperienceRecord>.Table_Reload(TalbeHelper.GetLoadPath("OfflineExperience"), OfflineExperience);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("OfflineExperience"));
                    break;
                case "Survey":
                    TableInit<SurveyRecord>.Table_Reload(TalbeHelper.GetLoadPath("Survey"), Survey);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Survey"));
                    break;
                case "Preferential":
                    TableInit<PreferentialRecord>.Table_Reload(TalbeHelper.GetLoadPath("Preferential"), Preferential);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Preferential"));
                    break;
                case "KaiFu":
                    TableInit<KaiFuRecord>.Table_Reload(TalbeHelper.GetLoadPath("KaiFu"), KaiFu);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("KaiFu"));
                    break;
                case "BossHome":
                    TableInit<BossHomeRecord>.Table_Reload(TalbeHelper.GetLoadPath("BossHome"), BossHome);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("BossHome"));
                    break;
                case "WarFlag":
                    TableInit<WarFlagRecord>.Table_Reload(TalbeHelper.GetLoadPath("WarFlag"), WarFlag);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("WarFlag"));
                    break;
                case "Lode":
                    TableInit<LodeRecord>.Table_Reload(TalbeHelper.GetLoadPath("Lode"), Lode);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Lode"));
                    break;
                case "MainActivity":
                    TableInit<MainActivityRecord>.Table_Reload(TalbeHelper.GetLoadPath("MainActivity"), MainActivity);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("MainActivity"));
                    break;
                case "ObjectTable":
                    TableInit<ObjectTableRecord>.Table_Reload(TalbeHelper.GetLoadPath("ObjectTable"), ObjectTable);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("ObjectTable"));
                    break;
                case "BatteryBaseNew":
                    TableInit<BatteryBaseNewRecord>.Table_Reload(TalbeHelper.GetLoadPath("BatteryBaseNew"), BatteryBaseNew);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("BatteryBaseNew"));
                    break;
                case "Checken":
                    TableInit<CheckenRecord>.Table_Reload(TalbeHelper.GetLoadPath("Checken"), Checken);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("Checken"));
                    break;
                case "CheckenLv":
                    TableInit<CheckenLvRecord>.Table_Reload(TalbeHelper.GetLoadPath("CheckenLv"), CheckenLv);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("CheckenLv"));
                    break;
                case "CheckenReward":
                    TableInit<CheckenRewardRecord>.Table_Reload(TalbeHelper.GetLoadPath("CheckenReward"), CheckenReward);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("CheckenReward"));
                    break;
                case "CheckenFinalReward":
                    TableInit<CheckenFinalRewardRecord>.Table_Reload(TalbeHelper.GetLoadPath("CheckenFinalReward"), CheckenFinalReward);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("CheckenFinalReward"));
                    break;
                case "SuperVip":
                    TableInit<SuperVipRecord>.Table_Reload(TalbeHelper.GetLoadPath("SuperVip"), SuperVip);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("SuperVip"));
                    break;
                case "IosMutiplePlatform":
                    TableInit<IosMutiplePlatformRecord>.Table_Reload(TalbeHelper.GetLoadPath("IosMutiplePlatform"), IosMutiplePlatform);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("IosMutiplePlatform"));
                    break;
                case "BattleCorrect":
                    TableInit<BattleCorrectRecord>.Table_Reload(TalbeHelper.GetLoadPath("BattleCorrect"), BattleCorrect);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("BattleCorrect"));
                    break;
                case "FashionTitle":
                    TableInit<FashionTitleRecord>.Table_Reload(TalbeHelper.GetLoadPath("FashionTitle"), FashionTitle);
                    EventDispatcher.Instance.DispatchEvent(new ReloadTableEvent("FashionTitle"));
                    break;
            }
        }
        public static void ForeachConditionTable(Func<ConditionTableRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach ConditionTable act is null");
                return;
            }
            foreach (var tempRecord in ConditionTable)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static ConditionTableRecord GetConditionTable(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            ConditionTableRecord tbConditionTable;
            if (!ConditionTable.TryGetValue(nId, out tbConditionTable))
            {
                Logger.Error("ConditionTable[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbConditionTable;
        }
        public static void ForeachFlag(Func<FlagRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Flag act is null");
                return;
            }
            foreach (var tempRecord in Flag)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static FlagRecord GetFlag(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            FlagRecord tbFlag;
            if (!Flag.TryGetValue(nId, out tbFlag))
            {
                Logger.Error("Flag[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbFlag;
        }
        public static void ForeachExdata(Func<ExdataRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Exdata act is null");
                return;
            }
            foreach (var tempRecord in Exdata)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static ExdataRecord GetExdata(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            ExdataRecord tbExdata;
            if (!Exdata.TryGetValue(nId, out tbExdata))
            {
                Logger.Error("Exdata[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbExdata;
        }
        public static void ForeachDictionary(Func<DictionaryRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Dictionary act is null");
                return;
            }
            foreach (var tempRecord in Dictionary)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static DictionaryRecord GetDictionary(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            DictionaryRecord tbDictionary;
            if (!Dictionary.TryGetValue(nId, out tbDictionary))
            {
                Logger.Error("Dictionary[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbDictionary;
        }
        public static void ForeachBagBase(Func<BagBaseRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach BagBase act is null");
                return;
            }
            foreach (var tempRecord in BagBase)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static BagBaseRecord GetBagBase(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            BagBaseRecord tbBagBase;
            if (!BagBase.TryGetValue(nId, out tbBagBase))
            {
                Logger.Error("BagBase[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbBagBase;
        }
        public static void ForeachItemBase(Func<ItemBaseRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach ItemBase act is null");
                return;
            }
            foreach (var tempRecord in ItemBase)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static ItemBaseRecord GetItemBase(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            ItemBaseRecord tbItemBase;
            if (!ItemBase.TryGetValue(nId, out tbItemBase))
            {
                Logger.Error("ItemBase[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbItemBase;
        }
        public static void ForeachItemType(Func<ItemTypeRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach ItemType act is null");
                return;
            }
            foreach (var tempRecord in ItemType)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static ItemTypeRecord GetItemType(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            ItemTypeRecord tbItemType;
            if (!ItemType.TryGetValue(nId, out tbItemType))
            {
                Logger.Error("ItemType[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbItemType;
        }
        public static void ForeachColorBase(Func<ColorBaseRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach ColorBase act is null");
                return;
            }
            foreach (var tempRecord in ColorBase)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static ColorBaseRecord GetColorBase(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            ColorBaseRecord tbColorBase;
            if (!ColorBase.TryGetValue(nId, out tbColorBase))
            {
                Logger.Error("ColorBase[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbColorBase;
        }
        public static void ForeachBuff(Func<BuffRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Buff act is null");
                return;
            }
            foreach (var tempRecord in Buff)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static BuffRecord GetBuff(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            BuffRecord tbBuff;
            if (!Buff.TryGetValue(nId, out tbBuff))
            {
                Logger.Error("Buff[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbBuff;
        }
        public static void ForeachMission(Func<MissionRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Mission act is null");
                return;
            }
            foreach (var tempRecord in Mission)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static MissionRecord GetMission(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            MissionRecord tbMission;
            if (!Mission.TryGetValue(nId, out tbMission))
            {
                Logger.Error("Mission[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbMission;
        }
        public static void ForeachCharacterBase(Func<CharacterBaseRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach CharacterBase act is null");
                return;
            }
            foreach (var tempRecord in CharacterBase)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static CharacterBaseRecord GetCharacterBase(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            CharacterBaseRecord tbCharacterBase;
            if (!CharacterBase.TryGetValue(nId, out tbCharacterBase))
            {
                Logger.Error("CharacterBase[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbCharacterBase;
        }
        public static void ForeachActor(Func<ActorRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Actor act is null");
                return;
            }
            foreach (var tempRecord in Actor)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static ActorRecord GetActor(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            ActorRecord tbActor;
            if (!Actor.TryGetValue(nId, out tbActor))
            {
                Logger.Error("Actor[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbActor;
        }
        public static void ForeachAttrRef(Func<AttrRefRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach AttrRef act is null");
                return;
            }
            foreach (var tempRecord in AttrRef)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static AttrRefRecord GetAttrRef(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            AttrRefRecord tbAttrRef;
            if (!AttrRef.TryGetValue(nId, out tbAttrRef))
            {
                Logger.Error("AttrRef[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbAttrRef;
        }
        public static void ForeachEquip(Func<EquipRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Equip act is null");
                return;
            }
            foreach (var tempRecord in Equip)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static EquipRecord GetEquip(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            EquipRecord tbEquip;
            if (!Equip.TryGetValue(nId, out tbEquip))
            {
                Logger.Error("Equip[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbEquip;
        }
        public static void ForeachEquipRelate(Func<EquipRelateRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach EquipRelate act is null");
                return;
            }
            foreach (var tempRecord in EquipRelate)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static EquipRelateRecord GetEquipRelate(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            EquipRelateRecord tbEquipRelate;
            if (!EquipRelate.TryGetValue(nId, out tbEquipRelate))
            {
                Logger.Error("EquipRelate[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbEquipRelate;
        }
        public static void ForeachEquipEnchant(Func<EquipEnchantRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach EquipEnchant act is null");
                return;
            }
            foreach (var tempRecord in EquipEnchant)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static EquipEnchantRecord GetEquipEnchant(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            EquipEnchantRecord tbEquipEnchant;
            if (!EquipEnchant.TryGetValue(nId, out tbEquipEnchant))
            {
                Logger.Error("EquipEnchant[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbEquipEnchant;
        }
        public static void ForeachEquipEnchantChance(Func<EquipEnchantChanceRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach EquipEnchantChance act is null");
                return;
            }
            foreach (var tempRecord in EquipEnchantChance)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static EquipEnchantChanceRecord GetEquipEnchantChance(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            EquipEnchantChanceRecord tbEquipEnchantChance;
            if (!EquipEnchantChance.TryGetValue(nId, out tbEquipEnchantChance))
            {
                Logger.Error("EquipEnchantChance[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbEquipEnchantChance;
        }
        public static void ForeachTitle(Func<TitleRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Title act is null");
                return;
            }
            foreach (var tempRecord in Title)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static TitleRecord GetTitle(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            TitleRecord tbTitle;
            if (!Title.TryGetValue(nId, out tbTitle))
            {
                Logger.Error("Title[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbTitle;
        }
        public static void ForeachEquipEnchance(Func<EquipEnchanceRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach EquipEnchance act is null");
                return;
            }
            foreach (var tempRecord in EquipEnchance)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static EquipEnchanceRecord GetEquipEnchance(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            EquipEnchanceRecord tbEquipEnchance;
            if (!EquipEnchance.TryGetValue(nId, out tbEquipEnchance))
            {
                Logger.Error("EquipEnchance[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbEquipEnchance;
        }
        public static void ForeachLevelData(Func<LevelDataRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach LevelData act is null");
                return;
            }
            foreach (var tempRecord in LevelData)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static LevelDataRecord GetLevelData(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            LevelDataRecord tbLevelData;
            if (!LevelData.TryGetValue(nId, out tbLevelData))
            {
                Logger.Error("LevelData[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbLevelData;
        }
        public static void ForeachSceneNpc(Func<SceneNpcRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach SceneNpc act is null");
                return;
            }
            foreach (var tempRecord in SceneNpc)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static SceneNpcRecord GetSceneNpc(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            SceneNpcRecord tbSceneNpc;
            if (!SceneNpc.TryGetValue(nId, out tbSceneNpc))
            {
                Logger.Error("SceneNpc[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbSceneNpc;
        }
        public static void ForeachSkill(Func<SkillRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Skill act is null");
                return;
            }
            foreach (var tempRecord in Skill)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static SkillRecord GetSkill(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            SkillRecord tbSkill;
            if (!Skill.TryGetValue(nId, out tbSkill))
            {
                Logger.Error("Skill[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbSkill;
        }
        public static void ForeachBullet(Func<BulletRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Bullet act is null");
                return;
            }
            foreach (var tempRecord in Bullet)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static BulletRecord GetBullet(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            BulletRecord tbBullet;
            if (!Bullet.TryGetValue(nId, out tbBullet))
            {
                Logger.Error("Bullet[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbBullet;
        }
        public static void ForeachScene(Func<SceneRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Scene act is null");
                return;
            }
            foreach (var tempRecord in Scene)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static SceneRecord GetScene(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            SceneRecord tbScene;
            if (!Scene.TryGetValue(nId, out tbScene))
            {
                Logger.Error("Scene[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbScene;
        }
        public static void ForeachTalent(Func<TalentRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Talent act is null");
                return;
            }
            foreach (var tempRecord in Talent)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static TalentRecord GetTalent(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            TalentRecord tbTalent;
            if (!Talent.TryGetValue(nId, out tbTalent))
            {
                Logger.Error("Talent[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbTalent;
        }
        public static void ForeachNpcBase(Func<NpcBaseRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach NpcBase act is null");
                return;
            }
            foreach (var tempRecord in NpcBase)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static NpcBaseRecord GetNpcBase(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            NpcBaseRecord tbNpcBase;
            if (!NpcBase.TryGetValue(nId, out tbNpcBase))
            {
                Logger.Error("NpcBase[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbNpcBase;
        }
        public static void ForeachSkillUpgrading(Func<SkillUpgradingRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach SkillUpgrading act is null");
                return;
            }
            foreach (var tempRecord in SkillUpgrading)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static SkillUpgradingRecord GetSkillUpgrading(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            SkillUpgradingRecord tbSkillUpgrading;
            if (!SkillUpgrading.TryGetValue(nId, out tbSkillUpgrading))
            {
                Logger.Error("SkillUpgrading[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbSkillUpgrading;
        }
        public static void ForeachAchievement(Func<AchievementRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Achievement act is null");
                return;
            }
            foreach (var tempRecord in Achievement)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static AchievementRecord GetAchievement(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            AchievementRecord tbAchievement;
            if (!Achievement.TryGetValue(nId, out tbAchievement))
            {
                Logger.Error("Achievement[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbAchievement;
        }
        public static void ForeachScript(Func<ScriptRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Script act is null");
                return;
            }
            foreach (var tempRecord in Script)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static ScriptRecord GetScript(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            ScriptRecord tbScript;
            if (!Script.TryGetValue(nId, out tbScript))
            {
                Logger.Error("Script[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbScript;
        }
        public static void ForeachDropMother(Func<DropMotherRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach DropMother act is null");
                return;
            }
            foreach (var tempRecord in DropMother)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static DropMotherRecord GetDropMother(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            DropMotherRecord tbDropMother;
            if (!DropMother.TryGetValue(nId, out tbDropMother))
            {
                Logger.Error("DropMother[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbDropMother;
        }
        public static void ForeachDropSon(Func<DropSonRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach DropSon act is null");
                return;
            }
            foreach (var tempRecord in DropSon)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static DropSonRecord GetDropSon(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            DropSonRecord tbDropSon;
            if (!DropSon.TryGetValue(nId, out tbDropSon))
            {
                Logger.Error("DropSon[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbDropSon;
        }
        public static void ForeachEquipTie(Func<EquipTieRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach EquipTie act is null");
                return;
            }
            foreach (var tempRecord in EquipTie)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static EquipTieRecord GetEquipTie(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            EquipTieRecord tbEquipTie;
            if (!EquipTie.TryGetValue(nId, out tbEquipTie))
            {
                Logger.Error("EquipTie[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbEquipTie;
        }
        public static void ForeachTransfer(Func<TransferRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Transfer act is null");
                return;
            }
            foreach (var tempRecord in Transfer)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static TransferRecord GetTransfer(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            TransferRecord tbTransfer;
            if (!Transfer.TryGetValue(nId, out tbTransfer))
            {
                Logger.Error("Transfer[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbTransfer;
        }
        public static void ForeachServerConfig(Func<ServerConfigRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach ServerConfig act is null");
                return;
            }
            foreach (var tempRecord in ServerConfig)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static ServerConfigRecord GetServerConfig(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            ServerConfigRecord tbServerConfig;
            if (!ServerConfig.TryGetValue(nId, out tbServerConfig))
            {
                Logger.Error("ServerConfig[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbServerConfig;
        }
        public static void ForeachAI(Func<AIRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach AI act is null");
                return;
            }
            foreach (var tempRecord in AI)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static AIRecord GetAI(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            AIRecord tbAI;
            if (!AI.TryGetValue(nId, out tbAI))
            {
                Logger.Error("AI[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbAI;
        }
        public static void ForeachEvent(Func<EventRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Event act is null");
                return;
            }
            foreach (var tempRecord in Event)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static EventRecord GetEvent(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            EventRecord tbEvent;
            if (!Event.TryGetValue(nId, out tbEvent))
            {
                Logger.Error("Event[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbEvent;
        }
        public static void ForeachDropConfig(Func<DropConfigRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach DropConfig act is null");
                return;
            }
            foreach (var tempRecord in DropConfig)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static DropConfigRecord GetDropConfig(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            DropConfigRecord tbDropConfig;
            if (!DropConfig.TryGetValue(nId, out tbDropConfig))
            {
                Logger.Error("DropConfig[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbDropConfig;
        }
        public static void ForeachGift(Func<GiftRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Gift act is null");
                return;
            }
            foreach (var tempRecord in Gift)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static GiftRecord GetGift(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            GiftRecord tbGift;
            if (!Gift.TryGetValue(nId, out tbGift))
            {
                Logger.Error("Gift[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbGift;
        }
        public static void ForeachEquipBlessing(Func<EquipBlessingRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach EquipBlessing act is null");
                return;
            }
            foreach (var tempRecord in EquipBlessing)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static EquipBlessingRecord GetEquipBlessing(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            EquipBlessingRecord tbEquipBlessing;
            if (!EquipBlessing.TryGetValue(nId, out tbEquipBlessing))
            {
                Logger.Error("EquipBlessing[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbEquipBlessing;
        }
        public static void ForeachEquipAdditional(Func<EquipAdditionalRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach EquipAdditional act is null");
                return;
            }
            foreach (var tempRecord in EquipAdditional)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static EquipAdditionalRecord GetEquipAdditional(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            EquipAdditionalRecord tbEquipAdditional;
            if (!EquipAdditional.TryGetValue(nId, out tbEquipAdditional))
            {
                Logger.Error("EquipAdditional[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbEquipAdditional;
        }
        public static void ForeachEquipExcellent(Func<EquipExcellentRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach EquipExcellent act is null");
                return;
            }
            foreach (var tempRecord in EquipExcellent)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static EquipExcellentRecord GetEquipExcellent(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            EquipExcellentRecord tbEquipExcellent;
            if (!EquipExcellent.TryGetValue(nId, out tbEquipExcellent))
            {
                Logger.Error("EquipExcellent[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbEquipExcellent;
        }
        public static void ForeachHandBook(Func<HandBookRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach HandBook act is null");
                return;
            }
            foreach (var tempRecord in HandBook)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static HandBookRecord GetHandBook(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            HandBookRecord tbHandBook;
            if (!HandBook.TryGetValue(nId, out tbHandBook))
            {
                Logger.Error("HandBook[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbHandBook;
        }
        public static void ForeachBookGroup(Func<BookGroupRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach BookGroup act is null");
                return;
            }
            foreach (var tempRecord in BookGroup)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static BookGroupRecord GetBookGroup(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            BookGroupRecord tbBookGroup;
            if (!BookGroup.TryGetValue(nId, out tbBookGroup))
            {
                Logger.Error("BookGroup[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbBookGroup;
        }
        public static void ForeachItemCompose(Func<ItemComposeRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach ItemCompose act is null");
                return;
            }
            foreach (var tempRecord in ItemCompose)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static ItemComposeRecord GetItemCompose(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            ItemComposeRecord tbItemCompose;
            if (!ItemCompose.TryGetValue(nId, out tbItemCompose))
            {
                Logger.Error("ItemCompose[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbItemCompose;
        }
        public static void ForeachCamp(Func<CampRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Camp act is null");
                return;
            }
            foreach (var tempRecord in Camp)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static CampRecord GetCamp(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            CampRecord tbCamp;
            if (!Camp.TryGetValue(nId, out tbCamp))
            {
                Logger.Error("Camp[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbCamp;
        }
        public static void ForeachFuben(Func<FubenRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Fuben act is null");
                return;
            }
            foreach (var tempRecord in Fuben)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static FubenRecord GetFuben(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            FubenRecord tbFuben;
            if (!Fuben.TryGetValue(nId, out tbFuben))
            {
                Logger.Error("Fuben[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbFuben;
        }
        public static void ForeachSkillArea(Func<SkillAreaRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach SkillArea act is null");
                return;
            }
            foreach (var tempRecord in SkillArea)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static SkillAreaRecord GetSkillArea(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            SkillAreaRecord tbSkillArea;
            if (!SkillArea.TryGetValue(nId, out tbSkillArea))
            {
                Logger.Error("SkillArea[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbSkillArea;
        }
        public static void ForeachStats(Func<StatsRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Stats act is null");
                return;
            }
            foreach (var tempRecord in Stats)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static StatsRecord GetStats(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            StatsRecord tbStats;
            if (!Stats.TryGetValue(nId, out tbStats))
            {
                Logger.Error("Stats[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbStats;
        }
        public static void ForeachStore(Func<StoreRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Store act is null");
                return;
            }
            foreach (var tempRecord in Store)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static StoreRecord GetStore(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            StoreRecord tbStore;
            if (!Store.TryGetValue(nId, out tbStore))
            {
                Logger.Error("Store[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbStore;
        }
        public static void ForeachInitItem(Func<InitItemRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach InitItem act is null");
                return;
            }
            foreach (var tempRecord in InitItem)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static InitItemRecord GetInitItem(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            InitItemRecord tbInitItem;
            if (!InitItem.TryGetValue(nId, out tbInitItem))
            {
                Logger.Error("InitItem[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbInitItem;
        }
        public static void ForeachTriggerArea(Func<TriggerAreaRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach TriggerArea act is null");
                return;
            }
            foreach (var tempRecord in TriggerArea)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static TriggerAreaRecord GetTriggerArea(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            TriggerAreaRecord tbTriggerArea;
            if (!TriggerArea.TryGetValue(nId, out tbTriggerArea))
            {
                Logger.Error("TriggerArea[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbTriggerArea;
        }
        public static void ForeachMail(Func<MailRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Mail act is null");
                return;
            }
            foreach (var tempRecord in Mail)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static MailRecord GetMail(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            MailRecord tbMail;
            if (!Mail.TryGetValue(nId, out tbMail))
            {
                Logger.Error("Mail[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbMail;
        }
        public static void ForeachBuilding(Func<BuildingRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Building act is null");
                return;
            }
            foreach (var tempRecord in Building)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static BuildingRecord GetBuilding(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            BuildingRecord tbBuilding;
            if (!Building.TryGetValue(nId, out tbBuilding))
            {
                Logger.Error("Building[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbBuilding;
        }
        public static void ForeachBuildingRule(Func<BuildingRuleRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach BuildingRule act is null");
                return;
            }
            foreach (var tempRecord in BuildingRule)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static BuildingRuleRecord GetBuildingRule(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            BuildingRuleRecord tbBuildingRule;
            if (!BuildingRule.TryGetValue(nId, out tbBuildingRule))
            {
                Logger.Error("BuildingRule[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbBuildingRule;
        }
        public static void ForeachBuildingService(Func<BuildingServiceRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach BuildingService act is null");
                return;
            }
            foreach (var tempRecord in BuildingService)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static BuildingServiceRecord GetBuildingService(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            BuildingServiceRecord tbBuildingService;
            if (!BuildingService.TryGetValue(nId, out tbBuildingService))
            {
                Logger.Error("BuildingService[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbBuildingService;
        }
        public static void ForeachHomeSence(Func<HomeSenceRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach HomeSence act is null");
                return;
            }
            foreach (var tempRecord in HomeSence)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static HomeSenceRecord GetHomeSence(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            HomeSenceRecord tbHomeSence;
            if (!HomeSence.TryGetValue(nId, out tbHomeSence))
            {
                Logger.Error("HomeSence[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbHomeSence;
        }
        public static void ForeachPet(Func<PetRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Pet act is null");
                return;
            }
            foreach (var tempRecord in Pet)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static PetRecord GetPet(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            PetRecord tbPet;
            if (!Pet.TryGetValue(nId, out tbPet))
            {
                Logger.Error("Pet[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbPet;
        }
        public static void ForeachPetSkill(Func<PetSkillRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach PetSkill act is null");
                return;
            }
            foreach (var tempRecord in PetSkill)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static PetSkillRecord GetPetSkill(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            PetSkillRecord tbPetSkill;
            if (!PetSkill.TryGetValue(nId, out tbPetSkill))
            {
                Logger.Error("PetSkill[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbPetSkill;
        }
        public static void ForeachService(Func<ServiceRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Service act is null");
                return;
            }
            foreach (var tempRecord in Service)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static ServiceRecord GetService(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            ServiceRecord tbService;
            if (!Service.TryGetValue(nId, out tbService))
            {
                Logger.Error("Service[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbService;
        }
        public static void ForeachStoreType(Func<StoreTypeRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach StoreType act is null");
                return;
            }
            foreach (var tempRecord in StoreType)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static StoreTypeRecord GetStoreType(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            StoreTypeRecord tbStoreType;
            if (!StoreType.TryGetValue(nId, out tbStoreType))
            {
                Logger.Error("StoreType[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbStoreType;
        }
        public static void ForeachPetSkillBase(Func<PetSkillBaseRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach PetSkillBase act is null");
                return;
            }
            foreach (var tempRecord in PetSkillBase)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static PetSkillBaseRecord GetPetSkillBase(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            PetSkillBaseRecord tbPetSkillBase;
            if (!PetSkillBase.TryGetValue(nId, out tbPetSkillBase))
            {
                Logger.Error("PetSkillBase[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbPetSkillBase;
        }
        public static void ForeachElf(Func<ElfRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Elf act is null");
                return;
            }
            foreach (var tempRecord in Elf)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static ElfRecord GetElf(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            ElfRecord tbElf;
            if (!Elf.TryGetValue(nId, out tbElf))
            {
                Logger.Error("Elf[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbElf;
        }
        public static void ForeachElfGroup(Func<ElfGroupRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach ElfGroup act is null");
                return;
            }
            foreach (var tempRecord in ElfGroup)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static ElfGroupRecord GetElfGroup(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            ElfGroupRecord tbElfGroup;
            if (!ElfGroup.TryGetValue(nId, out tbElfGroup))
            {
                Logger.Error("ElfGroup[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbElfGroup;
        }
        public static void ForeachQueue(Func<QueueRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Queue act is null");
                return;
            }
            foreach (var tempRecord in Queue)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static QueueRecord GetQueue(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            QueueRecord tbQueue;
            if (!Queue.TryGetValue(nId, out tbQueue))
            {
                Logger.Error("Queue[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbQueue;
        }
        public static void ForeachDraw(Func<DrawRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Draw act is null");
                return;
            }
            foreach (var tempRecord in Draw)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static DrawRecord GetDraw(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            DrawRecord tbDraw;
            if (!Draw.TryGetValue(nId, out tbDraw))
            {
                Logger.Error("Draw[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbDraw;
        }
        public static void ForeachPlant(Func<PlantRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Plant act is null");
                return;
            }
            foreach (var tempRecord in Plant)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static PlantRecord GetPlant(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            PlantRecord tbPlant;
            if (!Plant.TryGetValue(nId, out tbPlant))
            {
                Logger.Error("Plant[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbPlant;
        }
        public static void ForeachMedal(Func<MedalRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Medal act is null");
                return;
            }
            foreach (var tempRecord in Medal)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static MedalRecord GetMedal(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            MedalRecord tbMedal;
            if (!Medal.TryGetValue(nId, out tbMedal))
            {
                Logger.Error("Medal[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbMedal;
        }
        public static void ForeachSailing(Func<SailingRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Sailing act is null");
                return;
            }
            foreach (var tempRecord in Sailing)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static SailingRecord GetSailing(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            SailingRecord tbSailing;
            if (!Sailing.TryGetValue(nId, out tbSailing))
            {
                Logger.Error("Sailing[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbSailing;
        }
        public static void ForeachWingTrain(Func<WingTrainRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach WingTrain act is null");
                return;
            }
            foreach (var tempRecord in WingTrain)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static WingTrainRecord GetWingTrain(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            WingTrainRecord tbWingTrain;
            if (!WingTrain.TryGetValue(nId, out tbWingTrain))
            {
                Logger.Error("WingTrain[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbWingTrain;
        }
        public static void ForeachWingQuality(Func<WingQualityRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach WingQuality act is null");
                return;
            }
            foreach (var tempRecord in WingQuality)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static WingQualityRecord GetWingQuality(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            WingQualityRecord tbWingQuality;
            if (!WingQuality.TryGetValue(nId, out tbWingQuality))
            {
                Logger.Error("WingQuality[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbWingQuality;
        }
        public static void ForeachPVPRule(Func<PVPRuleRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach PVPRule act is null");
                return;
            }
            foreach (var tempRecord in PVPRule)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static PVPRuleRecord GetPVPRule(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            PVPRuleRecord tbPVPRule;
            if (!PVPRule.TryGetValue(nId, out tbPVPRule))
            {
                Logger.Error("PVPRule[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbPVPRule;
        }
        public static void ForeachArenaReward(Func<ArenaRewardRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach ArenaReward act is null");
                return;
            }
            foreach (var tempRecord in ArenaReward)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static ArenaRewardRecord GetArenaReward(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            ArenaRewardRecord tbArenaReward;
            if (!ArenaReward.TryGetValue(nId, out tbArenaReward))
            {
                Logger.Error("ArenaReward[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbArenaReward;
        }
        public static void ForeachArenaLevel(Func<ArenaLevelRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach ArenaLevel act is null");
                return;
            }
            foreach (var tempRecord in ArenaLevel)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static ArenaLevelRecord GetArenaLevel(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            ArenaLevelRecord tbArenaLevel;
            if (!ArenaLevel.TryGetValue(nId, out tbArenaLevel))
            {
                Logger.Error("ArenaLevel[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbArenaLevel;
        }
        public static void ForeachHonor(Func<HonorRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Honor act is null");
                return;
            }
            foreach (var tempRecord in Honor)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static HonorRecord GetHonor(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            HonorRecord tbHonor;
            if (!Honor.TryGetValue(nId, out tbHonor))
            {
                Logger.Error("Honor[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbHonor;
        }
        public static void ForeachJJCRoot(Func<JJCRootRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach JJCRoot act is null");
                return;
            }
            foreach (var tempRecord in JJCRoot)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static JJCRootRecord GetJJCRoot(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            JJCRootRecord tbJJCRoot;
            if (!JJCRoot.TryGetValue(nId, out tbJJCRoot))
            {
                Logger.Error("JJCRoot[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbJJCRoot;
        }
        public static void ForeachStatue(Func<StatueRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Statue act is null");
                return;
            }
            foreach (var tempRecord in Statue)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static StatueRecord GetStatue(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            StatueRecord tbStatue;
            if (!Statue.TryGetValue(nId, out tbStatue))
            {
                Logger.Error("Statue[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbStatue;
        }
        public static void ForeachEquipAdditional1(Func<EquipAdditional1Record, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach EquipAdditional1 act is null");
                return;
            }
            foreach (var tempRecord in EquipAdditional1)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static EquipAdditional1Record GetEquipAdditional1(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            EquipAdditional1Record tbEquipAdditional1;
            if (!EquipAdditional1.TryGetValue(nId, out tbEquipAdditional1))
            {
                Logger.Error("EquipAdditional1[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbEquipAdditional1;
        }
        public static void ForeachGuild(Func<GuildRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Guild act is null");
                return;
            }
            foreach (var tempRecord in Guild)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static GuildRecord GetGuild(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            GuildRecord tbGuild;
            if (!Guild.TryGetValue(nId, out tbGuild))
            {
                Logger.Error("Guild[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbGuild;
        }
        public static void ForeachGuildBuff(Func<GuildBuffRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach GuildBuff act is null");
                return;
            }
            foreach (var tempRecord in GuildBuff)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static GuildBuffRecord GetGuildBuff(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            GuildBuffRecord tbGuildBuff;
            if (!GuildBuff.TryGetValue(nId, out tbGuildBuff))
            {
                Logger.Error("GuildBuff[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbGuildBuff;
        }
        public static void ForeachGuildBoss(Func<GuildBossRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach GuildBoss act is null");
                return;
            }
            foreach (var tempRecord in GuildBoss)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static GuildBossRecord GetGuildBoss(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            GuildBossRecord tbGuildBoss;
            if (!GuildBoss.TryGetValue(nId, out tbGuildBoss))
            {
                Logger.Error("GuildBoss[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbGuildBoss;
        }
        public static void ForeachGuildAccess(Func<GuildAccessRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach GuildAccess act is null");
                return;
            }
            foreach (var tempRecord in GuildAccess)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static GuildAccessRecord GetGuildAccess(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            GuildAccessRecord tbGuildAccess;
            if (!GuildAccess.TryGetValue(nId, out tbGuildAccess))
            {
                Logger.Error("GuildAccess[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbGuildAccess;
        }
        public static void ForeachExpInfo(Func<ExpInfoRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach ExpInfo act is null");
                return;
            }
            foreach (var tempRecord in ExpInfo)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static ExpInfoRecord GetExpInfo(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            ExpInfoRecord tbExpInfo;
            if (!ExpInfo.TryGetValue(nId, out tbExpInfo))
            {
                Logger.Error("ExpInfo[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbExpInfo;
        }
        public static void ForeachGroupShop(Func<GroupShopRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach GroupShop act is null");
                return;
            }
            foreach (var tempRecord in GroupShop)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static GroupShopRecord GetGroupShop(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            GroupShopRecord tbGroupShop;
            if (!GroupShop.TryGetValue(nId, out tbGroupShop))
            {
                Logger.Error("GroupShop[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbGroupShop;
        }
        public static void ForeachGroupShopUpdate(Func<GroupShopUpdateRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach GroupShopUpdate act is null");
                return;
            }
            foreach (var tempRecord in GroupShopUpdate)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static GroupShopUpdateRecord GetGroupShopUpdate(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            GroupShopUpdateRecord tbGroupShopUpdate;
            if (!GroupShopUpdate.TryGetValue(nId, out tbGroupShopUpdate))
            {
                Logger.Error("GroupShopUpdate[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbGroupShopUpdate;
        }
        public static void ForeachPKMode(Func<PKModeRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach PKMode act is null");
                return;
            }
            foreach (var tempRecord in PKMode)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static PKModeRecord GetPKMode(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            PKModeRecord tbPKMode;
            if (!PKMode.TryGetValue(nId, out tbPKMode))
            {
                Logger.Error("PKMode[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbPKMode;
        }
        public static void Foreachforged(Func<forgedRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach forged act is null");
                return;
            }
            foreach (var tempRecord in forged)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static forgedRecord Getforged(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            forgedRecord tbforged;
            if (!forged.TryGetValue(nId, out tbforged))
            {
                Logger.Error("forged[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbforged;
        }
        public static void ForeachEquipUpdate(Func<EquipUpdateRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach EquipUpdate act is null");
                return;
            }
            foreach (var tempRecord in EquipUpdate)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static EquipUpdateRecord GetEquipUpdate(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            EquipUpdateRecord tbEquipUpdate;
            if (!EquipUpdate.TryGetValue(nId, out tbEquipUpdate))
            {
                Logger.Error("EquipUpdate[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbEquipUpdate;
        }
        public static void ForeachGuildMission(Func<GuildMissionRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach GuildMission act is null");
                return;
            }
            foreach (var tempRecord in GuildMission)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static GuildMissionRecord GetGuildMission(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            GuildMissionRecord tbGuildMission;
            if (!GuildMission.TryGetValue(nId, out tbGuildMission))
            {
                Logger.Error("GuildMission[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbGuildMission;
        }
        public static void ForeachOrderForm(Func<OrderFormRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach OrderForm act is null");
                return;
            }
            foreach (var tempRecord in OrderForm)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static OrderFormRecord GetOrderForm(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            OrderFormRecord tbOrderForm;
            if (!OrderForm.TryGetValue(nId, out tbOrderForm))
            {
                Logger.Error("OrderForm[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbOrderForm;
        }
        public static void ForeachOrderUpdate(Func<OrderUpdateRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach OrderUpdate act is null");
                return;
            }
            foreach (var tempRecord in OrderUpdate)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static OrderUpdateRecord GetOrderUpdate(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            OrderUpdateRecord tbOrderUpdate;
            if (!OrderUpdate.TryGetValue(nId, out tbOrderUpdate))
            {
                Logger.Error("OrderUpdate[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbOrderUpdate;
        }
        public static void ForeachTrade(Func<TradeRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Trade act is null");
                return;
            }
            foreach (var tempRecord in Trade)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static TradeRecord GetTrade(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            TradeRecord tbTrade;
            if (!Trade.TryGetValue(nId, out tbTrade))
            {
                Logger.Error("Trade[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbTrade;
        }
        public static void ForeachGem(Func<GemRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Gem act is null");
                return;
            }
            foreach (var tempRecord in Gem)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static GemRecord GetGem(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            GemRecord tbGem;
            if (!Gem.TryGetValue(nId, out tbGem))
            {
                Logger.Error("Gem[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbGem;
        }
        public static void ForeachGemGroup(Func<GemGroupRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach GemGroup act is null");
                return;
            }
            foreach (var tempRecord in GemGroup)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static GemGroupRecord GetGemGroup(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            GemGroupRecord tbGemGroup;
            if (!GemGroup.TryGetValue(nId, out tbGemGroup))
            {
                Logger.Error("GemGroup[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbGemGroup;
        }
        public static void ForeachSensitiveWord(Func<SensitiveWordRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach SensitiveWord act is null");
                return;
            }
            foreach (var tempRecord in SensitiveWord)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static SensitiveWordRecord GetSensitiveWord(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            SensitiveWordRecord tbSensitiveWord;
            if (!SensitiveWord.TryGetValue(nId, out tbSensitiveWord))
            {
                Logger.Error("SensitiveWord[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbSensitiveWord;
        }
        public static void ForeachGuidance(Func<GuidanceRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Guidance act is null");
                return;
            }
            foreach (var tempRecord in Guidance)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static GuidanceRecord GetGuidance(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            GuidanceRecord tbGuidance;
            if (!Guidance.TryGetValue(nId, out tbGuidance))
            {
                Logger.Error("Guidance[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbGuidance;
        }
        public static void ForeachMapTransfer(Func<MapTransferRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach MapTransfer act is null");
                return;
            }
            foreach (var tempRecord in MapTransfer)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static MapTransferRecord GetMapTransfer(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            MapTransferRecord tbMapTransfer;
            if (!MapTransfer.TryGetValue(nId, out tbMapTransfer))
            {
                Logger.Error("MapTransfer[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbMapTransfer;
        }
        public static void ForeachRandomCoordinate(Func<RandomCoordinateRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach RandomCoordinate act is null");
                return;
            }
            foreach (var tempRecord in RandomCoordinate)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static RandomCoordinateRecord GetRandomCoordinate(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            RandomCoordinateRecord tbRandomCoordinate;
            if (!RandomCoordinate.TryGetValue(nId, out tbRandomCoordinate))
            {
                Logger.Error("RandomCoordinate[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbRandomCoordinate;
        }
        public static void ForeachStepByStep(Func<StepByStepRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach StepByStep act is null");
                return;
            }
            foreach (var tempRecord in StepByStep)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static StepByStepRecord GetStepByStep(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            StepByStepRecord tbStepByStep;
            if (!StepByStep.TryGetValue(nId, out tbStepByStep))
            {
                Logger.Error("StepByStep[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbStepByStep;
        }
        public static void ForeachWorldBOSS(Func<WorldBOSSRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach WorldBOSS act is null");
                return;
            }
            foreach (var tempRecord in WorldBOSS)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static WorldBOSSRecord GetWorldBOSS(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            WorldBOSSRecord tbWorldBOSS;
            if (!WorldBOSS.TryGetValue(nId, out tbWorldBOSS))
            {
                Logger.Error("WorldBOSS[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbWorldBOSS;
        }
        public static void ForeachWorldBOSSAward(Func<WorldBOSSAwardRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach WorldBOSSAward act is null");
                return;
            }
            foreach (var tempRecord in WorldBOSSAward)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static WorldBOSSAwardRecord GetWorldBOSSAward(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            WorldBOSSAwardRecord tbWorldBOSSAward;
            if (!WorldBOSSAward.TryGetValue(nId, out tbWorldBOSSAward))
            {
                Logger.Error("WorldBOSSAward[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbWorldBOSSAward;
        }
        public static void ForeachPKValue(Func<PKValueRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach PKValue act is null");
                return;
            }
            foreach (var tempRecord in PKValue)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static PKValueRecord GetPKValue(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            PKValueRecord tbPKValue;
            if (!PKValue.TryGetValue(nId, out tbPKValue))
            {
                Logger.Error("PKValue[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbPKValue;
        }
        public static void ForeachTransmigration(Func<TransmigrationRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Transmigration act is null");
                return;
            }
            foreach (var tempRecord in Transmigration)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static TransmigrationRecord GetTransmigration(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            TransmigrationRecord tbTransmigration;
            if (!Transmigration.TryGetValue(nId, out tbTransmigration))
            {
                Logger.Error("Transmigration[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbTransmigration;
        }
        public static void ForeachFubenInfo(Func<FubenInfoRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach FubenInfo act is null");
                return;
            }
            foreach (var tempRecord in FubenInfo)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static FubenInfoRecord GetFubenInfo(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            FubenInfoRecord tbFubenInfo;
            if (!FubenInfo.TryGetValue(nId, out tbFubenInfo))
            {
                Logger.Error("FubenInfo[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbFubenInfo;
        }
        public static void ForeachFubenLogic(Func<FubenLogicRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach FubenLogic act is null");
                return;
            }
            foreach (var tempRecord in FubenLogic)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static FubenLogicRecord GetFubenLogic(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            FubenLogicRecord tbFubenLogic;
            if (!FubenLogic.TryGetValue(nId, out tbFubenLogic))
            {
                Logger.Error("FubenLogic[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbFubenLogic;
        }
        public static void ForeachServerName(Func<ServerNameRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach ServerName act is null");
                return;
            }
            foreach (var tempRecord in ServerName)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static ServerNameRecord GetServerName(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            ServerNameRecord tbServerName;
            if (!ServerName.TryGetValue(nId, out tbServerName))
            {
                Logger.Error("ServerName[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbServerName;
        }
        public static void ForeachGetMissionLevel(Func<GetMissionLevelRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach GetMissionLevel act is null");
                return;
            }
            foreach (var tempRecord in GetMissionLevel)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static GetMissionLevelRecord GetGetMissionLevel(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            GetMissionLevelRecord tbGetMissionLevel;
            if (!GetMissionLevel.TryGetValue(nId, out tbGetMissionLevel))
            {
                Logger.Error("GetMissionLevel[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbGetMissionLevel;
        }
        public static void ForeachGetMissionType(Func<GetMissionTypeRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach GetMissionType act is null");
                return;
            }
            foreach (var tempRecord in GetMissionType)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static GetMissionTypeRecord GetGetMissionType(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            GetMissionTypeRecord tbGetMissionType;
            if (!GetMissionType.TryGetValue(nId, out tbGetMissionType))
            {
                Logger.Error("GetMissionType[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbGetMissionType;
        }
        public static void ForeachGetMissionQulity(Func<GetMissionQulityRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach GetMissionQulity act is null");
                return;
            }
            foreach (var tempRecord in GetMissionQulity)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static GetMissionQulityRecord GetGetMissionQulity(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            GetMissionQulityRecord tbGetMissionQulity;
            if (!GetMissionQulity.TryGetValue(nId, out tbGetMissionQulity))
            {
                Logger.Error("GetMissionQulity[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbGetMissionQulity;
        }
        public static void ForeachGetPetCount(Func<GetPetCountRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach GetPetCount act is null");
                return;
            }
            foreach (var tempRecord in GetPetCount)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static GetPetCountRecord GetGetPetCount(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            GetPetCountRecord tbGetPetCount;
            if (!GetPetCount.TryGetValue(nId, out tbGetPetCount))
            {
                Logger.Error("GetPetCount[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbGetPetCount;
        }
        public static void ForeachGetMissionInfo(Func<GetMissionInfoRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach GetMissionInfo act is null");
                return;
            }
            foreach (var tempRecord in GetMissionInfo)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static GetMissionInfoRecord GetGetMissionInfo(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            GetMissionInfoRecord tbGetMissionInfo;
            if (!GetMissionInfo.TryGetValue(nId, out tbGetMissionInfo))
            {
                Logger.Error("GetMissionInfo[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbGetMissionInfo;
        }
        public static void ForeachGetMissionPlace(Func<GetMissionPlaceRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach GetMissionPlace act is null");
                return;
            }
            foreach (var tempRecord in GetMissionPlace)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static GetMissionPlaceRecord GetGetMissionPlace(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            GetMissionPlaceRecord tbGetMissionPlace;
            if (!GetMissionPlace.TryGetValue(nId, out tbGetMissionPlace))
            {
                Logger.Error("GetMissionPlace[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbGetMissionPlace;
        }
        public static void ForeachGetMissionWeather(Func<GetMissionWeatherRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach GetMissionWeather act is null");
                return;
            }
            foreach (var tempRecord in GetMissionWeather)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static GetMissionWeatherRecord GetGetMissionWeather(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            GetMissionWeatherRecord tbGetMissionWeather;
            if (!GetMissionWeather.TryGetValue(nId, out tbGetMissionWeather))
            {
                Logger.Error("GetMissionWeather[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbGetMissionWeather;
        }
        public static void ForeachGetMissionTimeLevel(Func<GetMissionTimeLevelRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach GetMissionTimeLevel act is null");
                return;
            }
            foreach (var tempRecord in GetMissionTimeLevel)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static GetMissionTimeLevelRecord GetGetMissionTimeLevel(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            GetMissionTimeLevelRecord tbGetMissionTimeLevel;
            if (!GetMissionTimeLevel.TryGetValue(nId, out tbGetMissionTimeLevel))
            {
                Logger.Error("GetMissionTimeLevel[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbGetMissionTimeLevel;
        }
        public static void ForeachMissionConditionInfo(Func<MissionConditionInfoRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach MissionConditionInfo act is null");
                return;
            }
            foreach (var tempRecord in MissionConditionInfo)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static MissionConditionInfoRecord GetMissionConditionInfo(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            MissionConditionInfoRecord tbMissionConditionInfo;
            if (!MissionConditionInfo.TryGetValue(nId, out tbMissionConditionInfo))
            {
                Logger.Error("MissionConditionInfo[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbMissionConditionInfo;
        }
        public static void ForeachGetMissionReward(Func<GetMissionRewardRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach GetMissionReward act is null");
                return;
            }
            foreach (var tempRecord in GetMissionReward)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static GetMissionRewardRecord GetGetMissionReward(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            GetMissionRewardRecord tbGetMissionReward;
            if (!GetMissionReward.TryGetValue(nId, out tbGetMissionReward))
            {
                Logger.Error("GetMissionReward[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbGetMissionReward;
        }
        public static void ForeachGetPetType(Func<GetPetTypeRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach GetPetType act is null");
                return;
            }
            foreach (var tempRecord in GetPetType)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static GetPetTypeRecord GetGetPetType(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            GetPetTypeRecord tbGetPetType;
            if (!GetPetType.TryGetValue(nId, out tbGetPetType))
            {
                Logger.Error("GetPetType[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbGetPetType;
        }
        public static void ForeachSubject(Func<SubjectRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Subject act is null");
                return;
            }
            foreach (var tempRecord in Subject)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static SubjectRecord GetSubject(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            SubjectRecord tbSubject;
            if (!Subject.TryGetValue(nId, out tbSubject))
            {
                Logger.Error("Subject[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbSubject;
        }
        public static void ForeachGetMissionName(Func<GetMissionNameRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach GetMissionName act is null");
                return;
            }
            foreach (var tempRecord in GetMissionName)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static GetMissionNameRecord GetGetMissionName(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            GetMissionNameRecord tbGetMissionName;
            if (!GetMissionName.TryGetValue(nId, out tbGetMissionName))
            {
                Logger.Error("GetMissionName[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbGetMissionName;
        }
        public static void ForeachDynamicActivity(Func<DynamicActivityRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach DynamicActivity act is null");
                return;
            }
            foreach (var tempRecord in DynamicActivity)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static DynamicActivityRecord GetDynamicActivity(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            DynamicActivityRecord tbDynamicActivity;
            if (!DynamicActivity.TryGetValue(nId, out tbDynamicActivity))
            {
                Logger.Error("DynamicActivity[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbDynamicActivity;
        }
        public static void ForeachCompensation(Func<CompensationRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Compensation act is null");
                return;
            }
            foreach (var tempRecord in Compensation)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static CompensationRecord GetCompensation(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            CompensationRecord tbCompensation;
            if (!Compensation.TryGetValue(nId, out tbCompensation))
            {
                Logger.Error("Compensation[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbCompensation;
        }
        public static void ForeachCityTalk(Func<CityTalkRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach CityTalk act is null");
                return;
            }
            foreach (var tempRecord in CityTalk)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static CityTalkRecord GetCityTalk(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            CityTalkRecord tbCityTalk;
            if (!CityTalk.TryGetValue(nId, out tbCityTalk))
            {
                Logger.Error("CityTalk[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbCityTalk;
        }
        public static void ForeachDailyActivity(Func<DailyActivityRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach DailyActivity act is null");
                return;
            }
            foreach (var tempRecord in DailyActivity)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static DailyActivityRecord GetDailyActivity(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            DailyActivityRecord tbDailyActivity;
            if (!DailyActivity.TryGetValue(nId, out tbDailyActivity))
            {
                Logger.Error("DailyActivity[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbDailyActivity;
        }
        public static void ForeachRecharge(Func<RechargeRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Recharge act is null");
                return;
            }
            foreach (var tempRecord in Recharge)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static RechargeRecord GetRecharge(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            RechargeRecord tbRecharge;
            if (!Recharge.TryGetValue(nId, out tbRecharge))
            {
                Logger.Error("Recharge[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbRecharge;
        }
        public static void ForeachNameTitle(Func<NameTitleRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach NameTitle act is null");
                return;
            }
            foreach (var tempRecord in NameTitle)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static NameTitleRecord GetNameTitle(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            NameTitleRecord tbNameTitle;
            if (!NameTitle.TryGetValue(nId, out tbNameTitle))
            {
                Logger.Error("NameTitle[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbNameTitle;
        }
        public static void ForeachVIP(Func<VIPRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach VIP act is null");
                return;
            }
            foreach (var tempRecord in VIP)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static VIPRecord GetVIP(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            VIPRecord tbVIP;
            if (!VIP.TryGetValue(nId, out tbVIP))
            {
                Logger.Error("VIP[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbVIP;
        }
        public static void ForeachRechargeActive(Func<RechargeActiveRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach RechargeActive act is null");
                return;
            }
            foreach (var tempRecord in RechargeActive)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static RechargeActiveRecord GetRechargeActive(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            RechargeActiveRecord tbRechargeActive;
            if (!RechargeActive.TryGetValue(nId, out tbRechargeActive))
            {
                Logger.Error("RechargeActive[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbRechargeActive;
        }
        public static void ForeachRechargeActiveNotice(Func<RechargeActiveNoticeRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach RechargeActiveNotice act is null");
                return;
            }
            foreach (var tempRecord in RechargeActiveNotice)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static RechargeActiveNoticeRecord GetRechargeActiveNotice(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            RechargeActiveNoticeRecord tbRechargeActiveNotice;
            if (!RechargeActiveNotice.TryGetValue(nId, out tbRechargeActiveNotice))
            {
                Logger.Error("RechargeActiveNotice[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbRechargeActiveNotice;
        }
        public static void ForeachRechargeActiveInvestment(Func<RechargeActiveInvestmentRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach RechargeActiveInvestment act is null");
                return;
            }
            foreach (var tempRecord in RechargeActiveInvestment)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static RechargeActiveInvestmentRecord GetRechargeActiveInvestment(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            RechargeActiveInvestmentRecord tbRechargeActiveInvestment;
            if (!RechargeActiveInvestment.TryGetValue(nId, out tbRechargeActiveInvestment))
            {
                Logger.Error("RechargeActiveInvestment[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbRechargeActiveInvestment;
        }
        public static void ForeachRechargeActiveInvestmentReward(Func<RechargeActiveInvestmentRewardRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach RechargeActiveInvestmentReward act is null");
                return;
            }
            foreach (var tempRecord in RechargeActiveInvestmentReward)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static RechargeActiveInvestmentRewardRecord GetRechargeActiveInvestmentReward(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            RechargeActiveInvestmentRewardRecord tbRechargeActiveInvestmentReward;
            if (!RechargeActiveInvestmentReward.TryGetValue(nId, out tbRechargeActiveInvestmentReward))
            {
                Logger.Error("RechargeActiveInvestmentReward[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbRechargeActiveInvestmentReward;
        }
        public static void ForeachRechargeActiveCumulative(Func<RechargeActiveCumulativeRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach RechargeActiveCumulative act is null");
                return;
            }
            foreach (var tempRecord in RechargeActiveCumulative)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static RechargeActiveCumulativeRecord GetRechargeActiveCumulative(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            RechargeActiveCumulativeRecord tbRechargeActiveCumulative;
            if (!RechargeActiveCumulative.TryGetValue(nId, out tbRechargeActiveCumulative))
            {
                Logger.Error("RechargeActiveCumulative[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbRechargeActiveCumulative;
        }
        public static void ForeachRechargeActiveCumulativeReward(Func<RechargeActiveCumulativeRewardRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach RechargeActiveCumulativeReward act is null");
                return;
            }
            foreach (var tempRecord in RechargeActiveCumulativeReward)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static RechargeActiveCumulativeRewardRecord GetRechargeActiveCumulativeReward(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            RechargeActiveCumulativeRewardRecord tbRechargeActiveCumulativeReward;
            if (!RechargeActiveCumulativeReward.TryGetValue(nId, out tbRechargeActiveCumulativeReward))
            {
                Logger.Error("RechargeActiveCumulativeReward[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbRechargeActiveCumulativeReward;
        }
        public static void ForeachGiftCode(Func<GiftCodeRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach GiftCode act is null");
                return;
            }
            foreach (var tempRecord in GiftCode)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static GiftCodeRecord GetGiftCode(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            GiftCodeRecord tbGiftCode;
            if (!GiftCode.TryGetValue(nId, out tbGiftCode))
            {
                Logger.Error("GiftCode[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbGiftCode;
        }
        public static void ForeachGMCommand(Func<GMCommandRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach GMCommand act is null");
                return;
            }
            foreach (var tempRecord in GMCommand)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static GMCommandRecord GetGMCommand(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            GMCommandRecord tbGMCommand;
            if (!GMCommand.TryGetValue(nId, out tbGMCommand))
            {
                Logger.Error("GMCommand[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbGMCommand;
        }
        public static void ForeachAuctionType1(Func<AuctionType1Record, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach AuctionType1 act is null");
                return;
            }
            foreach (var tempRecord in AuctionType1)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static AuctionType1Record GetAuctionType1(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            AuctionType1Record tbAuctionType1;
            if (!AuctionType1.TryGetValue(nId, out tbAuctionType1))
            {
                Logger.Error("AuctionType1[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbAuctionType1;
        }
        public static void ForeachAuctionType2(Func<AuctionType2Record, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach AuctionType2 act is null");
                return;
            }
            foreach (var tempRecord in AuctionType2)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static AuctionType2Record GetAuctionType2(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            AuctionType2Record tbAuctionType2;
            if (!AuctionType2.TryGetValue(nId, out tbAuctionType2))
            {
                Logger.Error("AuctionType2[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbAuctionType2;
        }
        public static void ForeachAuctionType3(Func<AuctionType3Record, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach AuctionType3 act is null");
                return;
            }
            foreach (var tempRecord in AuctionType3)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static AuctionType3Record GetAuctionType3(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            AuctionType3Record tbAuctionType3;
            if (!AuctionType3.TryGetValue(nId, out tbAuctionType3))
            {
                Logger.Error("AuctionType3[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbAuctionType3;
        }
        public static void ForeachYunYing(Func<YunYingRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach YunYing act is null");
                return;
            }
            foreach (var tempRecord in YunYing)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static YunYingRecord GetYunYing(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            YunYingRecord tbYunYing;
            if (!YunYing.TryGetValue(nId, out tbYunYing))
            {
                Logger.Error("YunYing[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbYunYing;
        }
        public static void ForeachOperationActivity(Func<OperationActivityRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach OperationActivity act is null");
                return;
            }
            foreach (var tempRecord in OperationActivity)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static OperationActivityRecord GetOperationActivity(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            OperationActivityRecord tbOperationActivity;
            if (!OperationActivity.TryGetValue(nId, out tbOperationActivity))
            {
                Logger.Error("OperationActivity[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbOperationActivity;
        }
        public static void ForeachFirstRecharge(Func<FirstRechargeRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach FirstRecharge act is null");
                return;
            }
            foreach (var tempRecord in FirstRecharge)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static FirstRechargeRecord GetFirstRecharge(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            FirstRechargeRecord tbFirstRecharge;
            if (!FirstRecharge.TryGetValue(nId, out tbFirstRecharge))
            {
                Logger.Error("FirstRecharge[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbFirstRecharge;
        }
        public static void ForeachMieShi(Func<MieShiRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach MieShi act is null");
                return;
            }
            foreach (var tempRecord in MieShi)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static MieShiRecord GetMieShi(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            MieShiRecord tbMieShi;
            if (!MieShi.TryGetValue(nId, out tbMieShi))
            {
                Logger.Error("MieShi[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbMieShi;
        }
        public static void ForeachMieShiPublic(Func<MieShiPublicRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach MieShiPublic act is null");
                return;
            }
            foreach (var tempRecord in MieShiPublic)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static MieShiPublicRecord GetMieShiPublic(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            MieShiPublicRecord tbMieShiPublic;
            if (!MieShiPublic.TryGetValue(nId, out tbMieShiPublic))
            {
                Logger.Error("MieShiPublic[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbMieShiPublic;
        }
        public static void ForeachDefendCityReward(Func<DefendCityRewardRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach DefendCityReward act is null");
                return;
            }
            foreach (var tempRecord in DefendCityReward)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static DefendCityRewardRecord GetDefendCityReward(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            DefendCityRewardRecord tbDefendCityReward;
            if (!DefendCityReward.TryGetValue(nId, out tbDefendCityReward))
            {
                Logger.Error("DefendCityReward[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbDefendCityReward;
        }
        public static void ForeachDefendCityDevoteReward(Func<DefendCityDevoteRewardRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach DefendCityDevoteReward act is null");
                return;
            }
            foreach (var tempRecord in DefendCityDevoteReward)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static DefendCityDevoteRewardRecord GetDefendCityDevoteReward(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            DefendCityDevoteRewardRecord tbDefendCityDevoteReward;
            if (!DefendCityDevoteReward.TryGetValue(nId, out tbDefendCityDevoteReward))
            {
                Logger.Error("DefendCityDevoteReward[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbDefendCityDevoteReward;
        }
        public static void ForeachBatteryLevel(Func<BatteryLevelRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach BatteryLevel act is null");
                return;
            }
            foreach (var tempRecord in BatteryLevel)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static BatteryLevelRecord GetBatteryLevel(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            BatteryLevelRecord tbBatteryLevel;
            if (!BatteryLevel.TryGetValue(nId, out tbBatteryLevel))
            {
                Logger.Error("BatteryLevel[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbBatteryLevel;
        }
        public static void ForeachBatteryBase(Func<BatteryBaseRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach BatteryBase act is null");
                return;
            }
            foreach (var tempRecord in BatteryBase)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static BatteryBaseRecord GetBatteryBase(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            BatteryBaseRecord tbBatteryBase;
            if (!BatteryBase.TryGetValue(nId, out tbBatteryBase))
            {
                Logger.Error("BatteryBase[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbBatteryBase;
        }
        public static void ForeachPlayerDrop(Func<PlayerDropRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach PlayerDrop act is null");
                return;
            }
            foreach (var tempRecord in PlayerDrop)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static PlayerDropRecord GetPlayerDrop(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            PlayerDropRecord tbPlayerDrop;
            if (!PlayerDrop.TryGetValue(nId, out tbPlayerDrop))
            {
                Logger.Error("PlayerDrop[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbPlayerDrop;
        }
        public static void ForeachBuffGroup(Func<BuffGroupRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach BuffGroup act is null");
                return;
            }
            foreach (var tempRecord in BuffGroup)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static BuffGroupRecord GetBuffGroup(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            BuffGroupRecord tbBuffGroup;
            if (!BuffGroup.TryGetValue(nId, out tbBuffGroup))
            {
                Logger.Error("BuffGroup[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbBuffGroup;
        }
        public static void ForeachBangBuff(Func<BangBuffRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach BangBuff act is null");
                return;
            }
            foreach (var tempRecord in BangBuff)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static BangBuffRecord GetBangBuff(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            BangBuffRecord tbBangBuff;
            if (!BangBuff.TryGetValue(nId, out tbBangBuff))
            {
                Logger.Error("BangBuff[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbBangBuff;
        }
        public static void ForeachMieshiTowerReward(Func<MieshiTowerRewardRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach MieshiTowerReward act is null");
                return;
            }
            foreach (var tempRecord in MieshiTowerReward)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static MieshiTowerRewardRecord GetMieshiTowerReward(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            MieshiTowerRewardRecord tbMieshiTowerReward;
            if (!MieshiTowerReward.TryGetValue(nId, out tbMieshiTowerReward))
            {
                Logger.Error("MieshiTowerReward[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbMieshiTowerReward;
        }
        public static void ForeachClimbingTower(Func<ClimbingTowerRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach ClimbingTower act is null");
                return;
            }
            foreach (var tempRecord in ClimbingTower)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static ClimbingTowerRecord GetClimbingTower(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            ClimbingTowerRecord tbClimbingTower;
            if (!ClimbingTower.TryGetValue(nId, out tbClimbingTower))
            {
                Logger.Error("ClimbingTower[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbClimbingTower;
        }
        public static void ForeachAcientBattleField(Func<AcientBattleFieldRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach AcientBattleField act is null");
                return;
            }
            foreach (var tempRecord in AcientBattleField)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static AcientBattleFieldRecord GetAcientBattleField(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            AcientBattleFieldRecord tbAcientBattleField;
            if (!AcientBattleField.TryGetValue(nId, out tbAcientBattleField))
            {
                Logger.Error("AcientBattleField[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbAcientBattleField;
        }
        public static void ForeachConsumArray(Func<ConsumArrayRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach ConsumArray act is null");
                return;
            }
            foreach (var tempRecord in ConsumArray)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static ConsumArrayRecord GetConsumArray(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            ConsumArrayRecord tbConsumArray;
            if (!ConsumArray.TryGetValue(nId, out tbConsumArray))
            {
                Logger.Error("ConsumArray[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbConsumArray;
        }
        public static void ForeachBookBase(Func<BookBaseRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach BookBase act is null");
                return;
            }
            foreach (var tempRecord in BookBase)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static BookBaseRecord GetBookBase(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            BookBaseRecord tbBookBase;
            if (!BookBase.TryGetValue(nId, out tbBookBase))
            {
                Logger.Error("BookBase[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbBookBase;
        }
        public static void ForeachMount(Func<MountRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Mount act is null");
                return;
            }
            foreach (var tempRecord in Mount)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static MountRecord GetMount(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            MountRecord tbMount;
            if (!Mount.TryGetValue(nId, out tbMount))
            {
                Logger.Error("Mount[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbMount;
        }
        public static void ForeachMountSkill(Func<MountSkillRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach MountSkill act is null");
                return;
            }
            foreach (var tempRecord in MountSkill)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static MountSkillRecord GetMountSkill(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            MountSkillRecord tbMountSkill;
            if (!MountSkill.TryGetValue(nId, out tbMountSkill))
            {
                Logger.Error("MountSkill[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbMountSkill;
        }
        public static void ForeachMountFeed(Func<MountFeedRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach MountFeed act is null");
                return;
            }
            foreach (var tempRecord in MountFeed)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static MountFeedRecord GetMountFeed(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            MountFeedRecord tbMountFeed;
            if (!MountFeed.TryGetValue(nId, out tbMountFeed))
            {
                Logger.Error("MountFeed[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbMountFeed;
        }
        public static void ForeachMayaBase(Func<MayaBaseRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach MayaBase act is null");
                return;
            }
            foreach (var tempRecord in MayaBase)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static MayaBaseRecord GetMayaBase(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            MayaBaseRecord tbMayaBase;
            if (!MayaBase.TryGetValue(nId, out tbMayaBase))
            {
                Logger.Error("MayaBase[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbMayaBase;
        }
        public static void ForeachOfflineExperience(Func<OfflineExperienceRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach OfflineExperience act is null");
                return;
            }
            foreach (var tempRecord in OfflineExperience)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static OfflineExperienceRecord GetOfflineExperience(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            OfflineExperienceRecord tbOfflineExperience;
            if (!OfflineExperience.TryGetValue(nId, out tbOfflineExperience))
            {
                Logger.Error("OfflineExperience[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbOfflineExperience;
        }
        public static void ForeachSurvey(Func<SurveyRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Survey act is null");
                return;
            }
            foreach (var tempRecord in Survey)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static SurveyRecord GetSurvey(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            SurveyRecord tbSurvey;
            if (!Survey.TryGetValue(nId, out tbSurvey))
            {
                Logger.Error("Survey[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbSurvey;
        }
        public static void ForeachPreferential(Func<PreferentialRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Preferential act is null");
                return;
            }
            foreach (var tempRecord in Preferential)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static PreferentialRecord GetPreferential(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            PreferentialRecord tbPreferential;
            if (!Preferential.TryGetValue(nId, out tbPreferential))
            {
                Logger.Error("Preferential[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbPreferential;
        }
        public static void ForeachKaiFu(Func<KaiFuRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach KaiFu act is null");
                return;
            }
            foreach (var tempRecord in KaiFu)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static KaiFuRecord GetKaiFu(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            KaiFuRecord tbKaiFu;
            if (!KaiFu.TryGetValue(nId, out tbKaiFu))
            {
                Logger.Error("KaiFu[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbKaiFu;
        }
        public static void ForeachBossHome(Func<BossHomeRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach BossHome act is null");
                return;
            }
            foreach (var tempRecord in BossHome)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static BossHomeRecord GetBossHome(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            BossHomeRecord tbBossHome;
            if (!BossHome.TryGetValue(nId, out tbBossHome))
            {
                Logger.Error("BossHome[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbBossHome;
        }
        public static void ForeachWarFlag(Func<WarFlagRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach WarFlag act is null");
                return;
            }
            foreach (var tempRecord in WarFlag)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static WarFlagRecord GetWarFlag(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            WarFlagRecord tbWarFlag;
            if (!WarFlag.TryGetValue(nId, out tbWarFlag))
            {
                Logger.Error("WarFlag[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbWarFlag;
        }
        public static void ForeachLode(Func<LodeRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Lode act is null");
                return;
            }
            foreach (var tempRecord in Lode)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static LodeRecord GetLode(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            LodeRecord tbLode;
            if (!Lode.TryGetValue(nId, out tbLode))
            {
                Logger.Error("Lode[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbLode;
        }
        public static void ForeachMainActivity(Func<MainActivityRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach MainActivity act is null");
                return;
            }
            foreach (var tempRecord in MainActivity)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static MainActivityRecord GetMainActivity(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            MainActivityRecord tbMainActivity;
            if (!MainActivity.TryGetValue(nId, out tbMainActivity))
            {
                Logger.Error("MainActivity[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbMainActivity;
        }
        public static void ForeachObjectTable(Func<ObjectTableRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach ObjectTable act is null");
                return;
            }
            foreach (var tempRecord in ObjectTable)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static ObjectTableRecord GetObjectTable(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            ObjectTableRecord tbObjectTable;
            if (!ObjectTable.TryGetValue(nId, out tbObjectTable))
            {
                Logger.Error("ObjectTable[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbObjectTable;
        }
        public static void ForeachBatteryBaseNew(Func<BatteryBaseNewRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach BatteryBaseNew act is null");
                return;
            }
            foreach (var tempRecord in BatteryBaseNew)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static BatteryBaseNewRecord GetBatteryBaseNew(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            BatteryBaseNewRecord tbBatteryBaseNew;
            if (!BatteryBaseNew.TryGetValue(nId, out tbBatteryBaseNew))
            {
                Logger.Error("BatteryBaseNew[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbBatteryBaseNew;
        }
        public static void ForeachChecken(Func<CheckenRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach Checken act is null");
                return;
            }
            foreach (var tempRecord in Checken)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static CheckenRecord GetChecken(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            CheckenRecord tbChecken;
            if (!Checken.TryGetValue(nId, out tbChecken))
            {
                Logger.Error("Checken[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbChecken;
        }
        public static void ForeachCheckenLv(Func<CheckenLvRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach CheckenLv act is null");
                return;
            }
            foreach (var tempRecord in CheckenLv)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static CheckenLvRecord GetCheckenLv(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            CheckenLvRecord tbCheckenLv;
            if (!CheckenLv.TryGetValue(nId, out tbCheckenLv))
            {
                Logger.Error("CheckenLv[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbCheckenLv;
        }
        public static void ForeachCheckenReward(Func<CheckenRewardRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach CheckenReward act is null");
                return;
            }
            foreach (var tempRecord in CheckenReward)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static CheckenRewardRecord GetCheckenReward(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            CheckenRewardRecord tbCheckenReward;
            if (!CheckenReward.TryGetValue(nId, out tbCheckenReward))
            {
                Logger.Error("CheckenReward[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbCheckenReward;
        }
        public static void ForeachCheckenFinalReward(Func<CheckenFinalRewardRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach CheckenFinalReward act is null");
                return;
            }
            foreach (var tempRecord in CheckenFinalReward)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static CheckenFinalRewardRecord GetCheckenFinalReward(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            CheckenFinalRewardRecord tbCheckenFinalReward;
            if (!CheckenFinalReward.TryGetValue(nId, out tbCheckenFinalReward))
            {
                Logger.Error("CheckenFinalReward[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbCheckenFinalReward;
        }
        public static void ForeachSuperVip(Func<SuperVipRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach SuperVip act is null");
                return;
            }
            foreach (var tempRecord in SuperVip)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static SuperVipRecord GetSuperVip(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            SuperVipRecord tbSuperVip;
            if (!SuperVip.TryGetValue(nId, out tbSuperVip))
            {
                Logger.Error("SuperVip[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbSuperVip;
        }
        public static void ForeachIosMutiplePlatform(Func<IosMutiplePlatformRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach IosMutiplePlatform act is null");
                return;
            }
            foreach (var tempRecord in IosMutiplePlatform)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static IosMutiplePlatformRecord GetIosMutiplePlatform(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            IosMutiplePlatformRecord tbIosMutiplePlatform;
            if (!IosMutiplePlatform.TryGetValue(nId, out tbIosMutiplePlatform))
            {
                Logger.Error("IosMutiplePlatform[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbIosMutiplePlatform;
        }
        public static void ForeachBattleCorrect(Func<BattleCorrectRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach BattleCorrect act is null");
                return;
            }
            foreach (var tempRecord in BattleCorrect)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static BattleCorrectRecord GetBattleCorrect(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            BattleCorrectRecord tbBattleCorrect;
            if (!BattleCorrect.TryGetValue(nId, out tbBattleCorrect))
            {
                Logger.Error("BattleCorrect[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbBattleCorrect;
        }
        public static void ForeachFashionTitle(Func<FashionTitleRecord, bool> act)
        {
            if (act == null)
            {
                Logger.Error("Foreach FashionTitle act is null");
                return;
            }
            foreach (var tempRecord in FashionTitle)
            {
                try
                {
                    if (!act(tempRecord.Value))
                        break;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public static FashionTitleRecord GetFashionTitle(int nId,[CallerFilePath] string filename = "", [CallerMemberName] string member =  "", [CallerLineNumber] int line = 0)
        {
            FashionTitleRecord tbFashionTitle;
            if (!FashionTitle.TryGetValue(nId, out tbFashionTitle))
            {
                Logger.Error("FashionTitle[{0}] not find by Table ,filename={1}:{3},member={2}", nId, filename, member, line);
                return null;
            }
            return tbFashionTitle;
        }
    }
    public class ConditionTableRecord :IRecord
    {
        public static string __TableName = "ConditionTable.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] TrueFlag = new int[1];
        public int FlagTrueDict { get;        set; }
        public int[] FalseFlag = new int[1];
        public int FlagFalseDict { get;        set; }
        public int[] ExdataId = new int[4];
        public int[] ExdataMin = new int[4];
        public int[] ExdataMax = new int[4];
        public int[] ExdataDict = new int[4];
        public int[] ItemId = new int[4];
        public int[] ItemCountMin = new int[4];
        public int[] ItemCountMax = new int[4];
        public int[] ItemDict = new int[4];
        public int Role { get;        set; }
        public int RoleDict { get;        set; }
        public int[] OpenTime = new int[2];
        public int[] OpenTimeDict = new int[2];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                TrueFlag[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                FlagTrueDict = Table_Tamplet.Convert_Int(temp[__column++]);
                FalseFlag[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                FlagFalseDict = Table_Tamplet.Convert_Int(temp[__column++]);
                ExdataId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ExdataMin[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ExdataMax[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ExdataDict[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ExdataId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ExdataMin[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ExdataMax[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ExdataDict[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ExdataId[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ExdataMin[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ExdataMax[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ExdataDict[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ExdataId[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ExdataMin[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ExdataMax[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ExdataDict[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCountMin[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCountMax[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemDict[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCountMin[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCountMax[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemDict[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCountMin[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCountMax[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemDict[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCountMin[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCountMax[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemDict[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Role = Table_Tamplet.Convert_Int(temp[__column++]);
                RoleDict = Table_Tamplet.Convert_Int(temp[__column++]);
                OpenTime[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                OpenTimeDict[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                OpenTime[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                OpenTimeDict[1] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class FlagRecord :IRecord
    {
        public static string __TableName = "Flag.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int ToClient { get;        set; }
        public int RefreshType { get;        set; }
        public int RefreshTime { get;        set; }
        public int RefreshValue { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                ToClient = Table_Tamplet.Convert_Int(temp[__column++]);
                RefreshType = Table_Tamplet.Convert_Int(temp[__column++]);
                RefreshTime = Table_Tamplet.Convert_Int(temp[__column++]);
                RefreshValue = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class ExdataRecord :IRecord
    {
        public static string __TableName = "Exdata.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int InitValue { get;        set; }
        public int Change { get;        set; }
        public int RefreshRule { get;        set; }
        public int RefreshTime { get;        set; }
        public int[] RefreshValue = new int[2];
        public int IsRefresh { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                InitValue = Table_Tamplet.Convert_Int(temp[__column++]);
                Change = Table_Tamplet.Convert_Int(temp[__column++]);
                RefreshRule = Table_Tamplet.Convert_Int(temp[__column++]);
                RefreshTime = Table_Tamplet.Convert_Int(temp[__column++]);
                RefreshValue[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                RefreshValue[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                IsRefresh = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class DictionaryRecord :IRecord
    {
        public static string __TableName = "Dictionary.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string[] Desc = new string[2];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Desc[0]  = temp[__column++];
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class BagBaseRecord :IRecord
    {
        public static string __TableName = "BagBase.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int InitCapacity { get;        set; }
        public int MaxCapacity { get;        set; }
        public int ChangeBagCount { get;        set; }
        public int TimeMult { get;        set; }
        public int Expression { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                InitCapacity = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCapacity = Table_Tamplet.Convert_Int(temp[__column++]);
                ChangeBagCount = Table_Tamplet.Convert_Int(temp[__column++]);
                TimeMult = Table_Tamplet.Convert_Int(temp[__column++]);
                Expression = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class ItemBaseRecord :IRecord
    {
        public static string __TableName = "ItemBase.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string Name { get;        set; }
        public int Icon { get;        set; }
        public int InitInBag { get;        set; }
        public double CanInBag { get;        set; }
        public int MaxCount { get;        set; }
        public int Type { get;        set; }
        public int Quality { get;        set; }
        public int UseLevel { get;        set; }
        public int OccupationLimit { get;        set; }
        public int BuyNeedType { get;        set; }
        public int BuyNeedCount { get;        set; }
        public int CallBackType { get;        set; }
        public int CallBackPrice { get;        set; }
        public int Sell { get;        set; }
        public int[] Exdata = new int[4];
        public int SortLadder { get;        set; }
        public int CanTrade { get;        set; }
        public int TradeMaxCount { get;        set; }
        public int TradeMin { get;        set; }
        public int TradeMax { get;        set; }
        public int LevelLimit { get;        set; }
        public int ItemValue { get;        set; }
        public int IsDigNotic { get;        set; }
        public int[] AuctionType = new int[3];
        public int DependItemId { get;        set; }
        public int DependItemNum { get;        set; }
        public int ShowAfterUse { get;        set; }
        public int StoreID { get;        set; }
        public int AutoUse { get;        set; }
        public int TimeLimit { get;        set; }
        public int DonatePrice { get;        set; }
        public int TakeoutPrice { get;        set; }
        public int WishBroadcast { get;        set; }
        public int GuildPionts { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Name = temp[__column++];
                Icon = Table_Tamplet.Convert_Int(temp[__column++]);
                InitInBag = Table_Tamplet.Convert_Int(temp[__column++]);
                CanInBag = Table_Tamplet.Convert_Double(temp[__column++]);
                MaxCount = Table_Tamplet.Convert_Int(temp[__column++]);
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                Quality = Table_Tamplet.Convert_Int(temp[__column++]);
                UseLevel = Table_Tamplet.Convert_Int(temp[__column++]);
                OccupationLimit = Table_Tamplet.Convert_Int(temp[__column++]);
                BuyNeedType = Table_Tamplet.Convert_Int(temp[__column++]);
                BuyNeedCount = Table_Tamplet.Convert_Int(temp[__column++]);
                CallBackType = Table_Tamplet.Convert_Int(temp[__column++]);
                CallBackPrice = Table_Tamplet.Convert_Int(temp[__column++]);
                Sell = Table_Tamplet.Convert_Int(temp[__column++]);
                Exdata[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Exdata[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Exdata[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Exdata[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                SortLadder = Table_Tamplet.Convert_Int(temp[__column++]);
                CanTrade = Table_Tamplet.Convert_Int(temp[__column++]);
                TradeMaxCount = Table_Tamplet.Convert_Int(temp[__column++]);
                TradeMin = Table_Tamplet.Convert_Int(temp[__column++]);
                TradeMax = Table_Tamplet.Convert_Int(temp[__column++]);
                LevelLimit = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemValue = Table_Tamplet.Convert_Int(temp[__column++]);
                IsDigNotic = Table_Tamplet.Convert_Int(temp[__column++]);
                AuctionType[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                AuctionType[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                AuctionType[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                DependItemId = Table_Tamplet.Convert_Int(temp[__column++]);
                DependItemNum = Table_Tamplet.Convert_Int(temp[__column++]);
                ShowAfterUse = Table_Tamplet.Convert_Int(temp[__column++]);
                StoreID = Table_Tamplet.Convert_Int(temp[__column++]);
                AutoUse = Table_Tamplet.Convert_Int(temp[__column++]);
                TimeLimit = Table_Tamplet.Convert_Int(temp[__column++]);
                DonatePrice = Table_Tamplet.Convert_Int(temp[__column++]);
                TakeoutPrice = Table_Tamplet.Convert_Int(temp[__column++]);
                WishBroadcast = Table_Tamplet.Convert_Int(temp[__column++]);
                GuildPionts = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class ItemTypeRecord :IRecord
    {
        public static string __TableName = "ItemType.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int LogicType { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                LogicType = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class ColorBaseRecord :IRecord
    {
        public static string __TableName = "ColorBase.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int Red { get;        set; }
        public int Green { get;        set; }
        public int Blue { get;        set; }
        public int Alpha { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Red = Table_Tamplet.Convert_Int(temp[__column++]);
                Green = Table_Tamplet.Convert_Int(temp[__column++]);
                Blue = Table_Tamplet.Convert_Int(temp[__column++]);
                Alpha = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class BuffRecord :IRecord
    {
        public static string __TableName = "Buff.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int IsView { get;        set; }
        public int[] Effect = new int[2];
        public int Duration { get;        set; }
        public int RefleshRule { get;        set; }
        public int Type { get;        set; }
        public int DownLine { get;        set; }
        public int Die { get;        set; }
        public int SceneDisappear { get;        set; }
        public int DieDisappear { get;        set; }
        public int HuchiId { get;        set; }
        public int TihuanId { get;        set; }
        public int PriorityId { get;        set; }
        public int BearMax { get;        set; }
        public int LayerMax { get;        set; }
        public int[] effectid = new int[4];
        public int[] effectpoint = new int[4];
        public int[] EffectPointParam = new int[4];
        public int[,] effectparam = new int[4,6];
        public List<int> ElfSkillUp = new List<int>();
        public int SkillType { get;        set; }
        public int FightPoint { get;        set; }
        public int CoolDownTime { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                IsView = Table_Tamplet.Convert_Int(temp[__column++]);
                Effect[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Effect[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Duration = Table_Tamplet.Convert_Int(temp[__column++]);
                RefleshRule = Table_Tamplet.Convert_Int(temp[__column++]);
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                DownLine = Table_Tamplet.Convert_Int(temp[__column++]);
                Die = Table_Tamplet.Convert_Int(temp[__column++]);
                SceneDisappear = Table_Tamplet.Convert_Int(temp[__column++]);
                DieDisappear = Table_Tamplet.Convert_Int(temp[__column++]);
                HuchiId = Table_Tamplet.Convert_Int(temp[__column++]);
                TihuanId = Table_Tamplet.Convert_Int(temp[__column++]);
                PriorityId = Table_Tamplet.Convert_Int(temp[__column++]);
                BearMax = Table_Tamplet.Convert_Int(temp[__column++]);
                LayerMax = Table_Tamplet.Convert_Int(temp[__column++]);
                effectid[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectpoint[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                EffectPointParam[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectparam[0,0] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectparam[0,1] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectparam[0,2] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectparam[0,3] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectparam[0,4] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectparam[0,5] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectid[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectpoint[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                EffectPointParam[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectparam[1,0] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectparam[1,1] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectparam[1,2] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectparam[1,3] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectparam[1,4] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectparam[1,5] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectid[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectpoint[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                EffectPointParam[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectparam[2,0] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectparam[2,1] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectparam[2,2] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectparam[2,3] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectparam[2,4] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectparam[2,5] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectid[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectpoint[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                EffectPointParam[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectparam[3,0] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectparam[3,1] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectparam[3,2] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectparam[3,3] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectparam[3,4] = Table_Tamplet.Convert_Int(temp[__column++]);
                effectparam[3,5] = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(ElfSkillUp,temp[__column++]);
                SkillType = Table_Tamplet.Convert_Int(temp[__column++]);
                FightPoint = Table_Tamplet.Convert_Int(temp[__column++]);
                CoolDownTime = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class MissionRecord :IRecord
    {
        public static string __TableName = "MissionBase.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string Name { get;        set; }
        public int ViewType { get;        set; }
        public int Condition { get;        set; }
        public int CanDrop { get;        set; }
        public int TimeLimit { get;        set; }
        public int NpcStart { get;        set; }
        public int NpcScene { get;        set; }
        public float PosX { get;        set; }
        public float PosY { get;        set; }
        public int TrackType { get;        set; }
        public int[] TrackParam = new int[4];
        public int FinishCondition { get;        set; }
        public int[] FinishParam = new int[3];
        public int FinishNpcId { get;        set; }
        public int FinishSceneId { get;        set; }
        public float FinishPosX { get;        set; }
        public float FinishPosY { get;        set; }
        public int NextMission { get;        set; }
        public int JbId { get;        set; }
        public int[] RewardItem = new int[3];
        public int[] RewardItemCount = new int[3];
        public int[] DropId = new int[2];
        public int[] DropPro = new int[2];
        public int[] DropMonsterId = new int[2];
        public int TriggerActive { get;        set; }
        public int TriggerClose { get;        set; }
        public int FlagId { get;        set; }
        public int BuffAdd { get;        set; }
        public int BuffClean { get;        set; }
        public int RewardTitle { get;        set; }
        public int ExdataId { get;        set; }
        public int ExdataValue { get;        set; }
        public int[,] RoleRewardId = new int[3,2];
        public int[,] RoleRewardCount = new int[3,2];
        public List<int> GetSkill = new List<int>();
        public int IsDynamicExp { get;        set; }
        public int DynamicExpRatio { get;        set; }
        public int RandomTaskID { get;        set; }
        public int MutexID { get;        set; }
        public List<int> DeleteItem = new List<int>();
        public int SceneTransferId { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Name = temp[__column++];
                ViewType = Table_Tamplet.Convert_Int(temp[__column++]);
                Condition = Table_Tamplet.Convert_Int(temp[__column++]);
                CanDrop = Table_Tamplet.Convert_Int(temp[__column++]);
                TimeLimit = Table_Tamplet.Convert_Int(temp[__column++]);
                NpcStart = Table_Tamplet.Convert_Int(temp[__column++]);
                NpcScene = Table_Tamplet.Convert_Int(temp[__column++]);
                PosX = Table_Tamplet.Convert_Float(temp[__column++]);
                PosY = Table_Tamplet.Convert_Float(temp[__column++]);
                TrackType = Table_Tamplet.Convert_Int(temp[__column++]);
                TrackParam[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                TrackParam[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                TrackParam[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                TrackParam[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                FinishCondition = Table_Tamplet.Convert_Int(temp[__column++]);
                FinishParam[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                FinishParam[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                FinishParam[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                FinishNpcId = Table_Tamplet.Convert_Int(temp[__column++]);
                FinishSceneId = Table_Tamplet.Convert_Int(temp[__column++]);
                FinishPosX = Table_Tamplet.Convert_Float(temp[__column++]);
                FinishPosY = Table_Tamplet.Convert_Float(temp[__column++]);
                NextMission = Table_Tamplet.Convert_Int(temp[__column++]);
                JbId = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardItem[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardItemCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardItem[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardItemCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardItem[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardItemCount[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropPro[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropMonsterId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropPro[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropMonsterId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                TriggerActive = Table_Tamplet.Convert_Int(temp[__column++]);
                TriggerClose = Table_Tamplet.Convert_Int(temp[__column++]);
                FlagId = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffAdd = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffClean = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardTitle = Table_Tamplet.Convert_Int(temp[__column++]);
                ExdataId = Table_Tamplet.Convert_Int(temp[__column++]);
                ExdataValue = Table_Tamplet.Convert_Int(temp[__column++]);
                RoleRewardId[0,0] = Table_Tamplet.Convert_Int(temp[__column++]);
                RoleRewardCount[0,0] = Table_Tamplet.Convert_Int(temp[__column++]);
                RoleRewardId[0,1] = Table_Tamplet.Convert_Int(temp[__column++]);
                RoleRewardCount[0,1] = Table_Tamplet.Convert_Int(temp[__column++]);
                RoleRewardId[1,0] = Table_Tamplet.Convert_Int(temp[__column++]);
                RoleRewardCount[1,0] = Table_Tamplet.Convert_Int(temp[__column++]);
                RoleRewardId[1,1] = Table_Tamplet.Convert_Int(temp[__column++]);
                RoleRewardCount[1,1] = Table_Tamplet.Convert_Int(temp[__column++]);
                RoleRewardId[2,0] = Table_Tamplet.Convert_Int(temp[__column++]);
                RoleRewardCount[2,0] = Table_Tamplet.Convert_Int(temp[__column++]);
                RoleRewardId[2,1] = Table_Tamplet.Convert_Int(temp[__column++]);
                RoleRewardCount[2,1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(GetSkill,temp[__column++]);
                IsDynamicExp = Table_Tamplet.Convert_Int(temp[__column++]);
                DynamicExpRatio = Table_Tamplet.Convert_Int(temp[__column++]);
                RandomTaskID = Table_Tamplet.Convert_Int(temp[__column++]);
                MutexID = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(DeleteItem,temp[__column++]);
                SceneTransferId = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class CharacterBaseRecord :IRecord
    {
        public static string __TableName = "CharacterBase.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string Name { get;        set; }
        public int Type { get;        set; }
        public int ExdataId { get;        set; }
        public int Sex { get;        set; }
        public int Camp { get;        set; }
        public int[] Attr = new int[33];
        public int[] InitSkill = new int[20];
        public int IsBeAttack { get;        set; }
        public string PassiveSkillGroup { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Name = temp[__column++];
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                ExdataId = Table_Tamplet.Convert_Int(temp[__column++]);
                Sex = Table_Tamplet.Convert_Int(temp[__column++]);
                Camp = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[10] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[11] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[12] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[13] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[14] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[15] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[16] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[17] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[18] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[19] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[20] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[21] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[22] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[23] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[24] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[25] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[26] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[27] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[28] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[29] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[30] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[31] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[32] = Table_Tamplet.Convert_Int(temp[__column++]);
                InitSkill[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                InitSkill[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                InitSkill[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                InitSkill[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                InitSkill[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                InitSkill[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                InitSkill[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                InitSkill[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                InitSkill[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                InitSkill[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                InitSkill[10] = Table_Tamplet.Convert_Int(temp[__column++]);
                InitSkill[11] = Table_Tamplet.Convert_Int(temp[__column++]);
                InitSkill[12] = Table_Tamplet.Convert_Int(temp[__column++]);
                InitSkill[13] = Table_Tamplet.Convert_Int(temp[__column++]);
                InitSkill[14] = Table_Tamplet.Convert_Int(temp[__column++]);
                InitSkill[15] = Table_Tamplet.Convert_Int(temp[__column++]);
                InitSkill[16] = Table_Tamplet.Convert_Int(temp[__column++]);
                InitSkill[17] = Table_Tamplet.Convert_Int(temp[__column++]);
                InitSkill[18] = Table_Tamplet.Convert_Int(temp[__column++]);
                InitSkill[19] = Table_Tamplet.Convert_Int(temp[__column++]);
                IsBeAttack = Table_Tamplet.Convert_Int(temp[__column++]);
                PassiveSkillGroup = temp[__column++];
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class ActorRecord :IRecord
    {
        public static string __TableName = "Actor.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string Name { get;        set; }
        public int Class { get;        set; }
        public int Sex { get;        set; }
        public int[] FreePoint = new int[5];
        public int[] LadderLevel = new int[5];
        public int[] AutoAddAttr = new int[4];
        public int BirthScene { get;        set; }
        public float BirthPosX { get;        set; }
        public float BirthPosY { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Name = temp[__column++];
                Class = Table_Tamplet.Convert_Int(temp[__column++]);
                Sex = Table_Tamplet.Convert_Int(temp[__column++]);
                FreePoint[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                FreePoint[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                FreePoint[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                FreePoint[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                FreePoint[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                LadderLevel[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                LadderLevel[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                LadderLevel[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                LadderLevel[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                LadderLevel[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                AutoAddAttr[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                AutoAddAttr[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                AutoAddAttr[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                AutoAddAttr[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                BirthScene = Table_Tamplet.Convert_Int(temp[__column++]);
                BirthPosX = Table_Tamplet.Convert_Float(temp[__column++]);
                BirthPosY = Table_Tamplet.Convert_Float(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class AttrRefRecord :IRecord
    {
        public static string __TableName = "AttrRef.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int CharacterId { get;        set; }
        public int AttrId { get;        set; }
        public string Desc { get;        set; }
        public int[] Attr = new int[14];
        public int[] PropPercent = new int[12];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                CharacterId = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrId = Table_Tamplet.Convert_Int(temp[__column++]);
                Desc = temp[__column++];
                Attr[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[10] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[11] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[12] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[13] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropPercent[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropPercent[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropPercent[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropPercent[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropPercent[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropPercent[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropPercent[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropPercent[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropPercent[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropPercent[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropPercent[10] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropPercent[11] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class EquipRecord :IRecord
    {
        public static string __TableName = "EquipBase.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int Ladder { get;        set; }
        public int Occupation { get;        set; }
        public int Part { get;        set; }
        public int DurableType { get;        set; }
        public int Durability { get;        set; }
        public int DurableMoney { get;        set; }
        public int TieId { get;        set; }
        public int TieIndex { get;        set; }
        public int[] NeedAttrId = new int[2];
        public int[] NeedAttrValue = new int[2];
        public int[] BaseAttr = new int[4];
        public int[] BaseValue = new int[4];
        public int MaxLevel { get;        set; }
        public int[] BaseFixedAttrId = new int[2];
        public int[] BaseFixedAttrValue = new int[2];
        public int[] BaseFixedAttrValueMax = new int[2];
        public int[] ExcellentAttrId = new int[4];
        public int ExcellentAttrCount { get;        set; }
        public int ExcellentAttrValue { get;        set; }
        public int ExcellentAttrInterval { get;        set; }
        public int ExcellentValueMin { get;        set; }
        public int ExcellentValueMax { get;        set; }
        public int AddAttrId { get;        set; }
        public int AddAttrUpMinValue { get;        set; }
        public int AddAttrUpMaxValue { get;        set; }
        public int AddAttrMaxValue { get;        set; }
        public int AddIndexID { get;        set; }
        public int RandomAttrCount { get;        set; }
        public int RandomAttrPro { get;        set; }
        public int RandomAttrValue { get;        set; }
        public int RandomAttrInterval { get;        set; }
        public int RandomValueMin { get;        set; }
        public int RandomValueMax { get;        set; }
        public int RandomSlotCount { get;        set; }
        public int EquipUpdateLogic { get;        set; }
        public int UpdateEquipID { get;        set; }
        public int BuffGroupId { get;        set; }
        public int AddBuffSkillLevel { get;        set; }
        public int NeedRebornLevel { get;        set; }
        public int NewRandomAttrCount { get;        set; }
        public int NewRandomAttrPro { get;        set; }
        public int NewRandomAttrValue { get;        set; }
        public int NewRandomAttrInterval { get;        set; }
        public int NewRandomValueMin { get;        set; }
        public int NewRandomValueMax { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Ladder = Table_Tamplet.Convert_Int(temp[__column++]);
                Occupation = Table_Tamplet.Convert_Int(temp[__column++]);
                Part = Table_Tamplet.Convert_Int(temp[__column++]);
                DurableType = Table_Tamplet.Convert_Int(temp[__column++]);
                Durability = Table_Tamplet.Convert_Int(temp[__column++]);
                DurableMoney = Table_Tamplet.Convert_Int(temp[__column++]);
                TieId = Table_Tamplet.Convert_Int(temp[__column++]);
                TieIndex = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedAttrId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedAttrValue[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedAttrId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedAttrValue[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                BaseAttr[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                BaseValue[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                BaseAttr[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                BaseValue[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                BaseAttr[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                BaseValue[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                BaseAttr[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                BaseValue[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxLevel = Table_Tamplet.Convert_Int(temp[__column++]);
                BaseFixedAttrId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                BaseFixedAttrValue[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                BaseFixedAttrValueMax[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                BaseFixedAttrId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                BaseFixedAttrValue[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                BaseFixedAttrValueMax[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ExcellentAttrId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ExcellentAttrId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ExcellentAttrId[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ExcellentAttrId[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ExcellentAttrCount = Table_Tamplet.Convert_Int(temp[__column++]);
                ExcellentAttrValue = Table_Tamplet.Convert_Int(temp[__column++]);
                ExcellentAttrInterval = Table_Tamplet.Convert_Int(temp[__column++]);
                ExcellentValueMin = Table_Tamplet.Convert_Int(temp[__column++]);
                ExcellentValueMax = Table_Tamplet.Convert_Int(temp[__column++]);
                AddAttrId = Table_Tamplet.Convert_Int(temp[__column++]);
                AddAttrUpMinValue = Table_Tamplet.Convert_Int(temp[__column++]);
                AddAttrUpMaxValue = Table_Tamplet.Convert_Int(temp[__column++]);
                AddAttrMaxValue = Table_Tamplet.Convert_Int(temp[__column++]);
                AddIndexID = Table_Tamplet.Convert_Int(temp[__column++]);
                RandomAttrCount = Table_Tamplet.Convert_Int(temp[__column++]);
                RandomAttrPro = Table_Tamplet.Convert_Int(temp[__column++]);
                RandomAttrValue = Table_Tamplet.Convert_Int(temp[__column++]);
                RandomAttrInterval = Table_Tamplet.Convert_Int(temp[__column++]);
                RandomValueMin = Table_Tamplet.Convert_Int(temp[__column++]);
                RandomValueMax = Table_Tamplet.Convert_Int(temp[__column++]);
                RandomSlotCount = Table_Tamplet.Convert_Int(temp[__column++]);
                EquipUpdateLogic = Table_Tamplet.Convert_Int(temp[__column++]);
                UpdateEquipID = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffGroupId = Table_Tamplet.Convert_Int(temp[__column++]);
                AddBuffSkillLevel = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedRebornLevel = Table_Tamplet.Convert_Int(temp[__column++]);
                NewRandomAttrCount = Table_Tamplet.Convert_Int(temp[__column++]);
                NewRandomAttrPro = Table_Tamplet.Convert_Int(temp[__column++]);
                NewRandomAttrValue = Table_Tamplet.Convert_Int(temp[__column++]);
                NewRandomAttrInterval = Table_Tamplet.Convert_Int(temp[__column++]);
                NewRandomValueMin = Table_Tamplet.Convert_Int(temp[__column++]);
                NewRandomValueMax = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class EquipRelateRecord :IRecord
    {
        public static string __TableName = "EquipRelate.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] AttrCount = new int[7];
        public int[] Value = new int[4];
        public int[] Slot = new int[5];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrCount[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrCount[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrCount[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrCount[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrCount[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                Value[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Value[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Value[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Value[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Slot[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Slot[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Slot[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Slot[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Slot[4] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class EquipEnchantRecord :IRecord
    {
        public static string __TableName = "EquipEnchant.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] Attr = new int[23];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[10] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[11] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[12] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[13] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[14] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[15] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[16] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[17] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[18] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[19] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[20] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[21] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[22] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class EquipEnchantChanceRecord :IRecord
    {
        public static string __TableName = "EquipEnchantChance.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] Attr = new int[23];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[10] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[11] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[12] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[13] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[14] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[15] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[16] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[17] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[18] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[19] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[20] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[21] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[22] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class TitleRecord :IRecord
    {
        public static string __TableName = "Title.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int NameType { get;        set; }
        public int Mutex { get;        set; }
        public int Level { get;        set; }
        public int[] Buff = new int[2];
        public int Time { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                NameType = Table_Tamplet.Convert_Int(temp[__column++]);
                Mutex = Table_Tamplet.Convert_Int(temp[__column++]);
                Level = Table_Tamplet.Convert_Int(temp[__column++]);
                Buff[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Buff[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Time = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class EquipEnchanceRecord :IRecord
    {
        public static string __TableName = "EquipEnchance.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] Count = new int[5];
        public int[] Color = new int[5];
        public int[] Level = new int[4];
        public int[] Value = new int[5];
        public int[] Need = new int[5];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Color[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Color[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Color[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Color[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Color[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Level[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Level[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Level[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Level[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Value[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Value[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Value[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Value[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Value[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Need[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Need[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Need[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Need[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Need[4] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class LevelDataRecord :IRecord
    {
        public static string __TableName = "LevelData.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int NeedExp { get;        set; }
        public int ExpMax { get;        set; }
        public double Dodge { get;        set; }
        public double Hit { get;        set; }
        public int PhyPowerMinScale { get;        set; }
        public int PhyPowerMinFix { get;        set; }
        public int PhyPowerMaxScale { get;        set; }
        public int PhyPowerMaxFix { get;        set; }
        public int MagPowerMinScale { get;        set; }
        public int MagPowerMinFix { get;        set; }
        public int MagPowerMaxScale { get;        set; }
        public int MagPowerMaxFix { get;        set; }
        public int PhyArmorScale { get;        set; }
        public int PhyArmorFix { get;        set; }
        public int MagArmorScale { get;        set; }
        public int MagArmorFix { get;        set; }
        public int HpMaxScale { get;        set; }
        public int HpMaxFix { get;        set; }
        public int PowerFightPoint { get;        set; }
        public int ArmorFightPoint { get;        set; }
        public int HpFightPoint { get;        set; }
        public int MpFightPoint { get;        set; }
        public int HitFightPoint { get;        set; }
        public int DodgeFightPoint { get;        set; }
        public int IgnoreArmorProFightPoint { get;        set; }
        public int DamageAddProFightPoint { get;        set; }
        public int DamageResProFightPoint { get;        set; }
        public int ExcellentProFightPoint { get;        set; }
        public int LuckyProFightPoint { get;        set; }
        public int DamageReboundProFightPoint { get;        set; }
        public int LeaveExpBase { get;        set; }
        public int FightingWayExp { get;        set; }
        public int FightingWayIncome { get;        set; }
        public int ElfExp { get;        set; }
        public int ElfResolveValue { get;        set; }
        public int WorshipExp { get;        set; }
        public int BeWorshipedExp { get;        set; }
        public int BeWorshipedMoney { get;        set; }
        public int[] Exp = new int[5];
        public int FixedGorw { get;        set; }
        public int PercentGrow { get;        set; }
        public int UpNeedExp { get;        set; }
        public int[] FruitLimit = new int[4];
        public int DynamicExp { get;        set; }
        public int PetPointRatio { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedExp = Table_Tamplet.Convert_Int(temp[__column++]);
                ExpMax = Table_Tamplet.Convert_Int(temp[__column++]);
                Dodge = Table_Tamplet.Convert_Double(temp[__column++]);
                Hit = Table_Tamplet.Convert_Double(temp[__column++]);
                PhyPowerMinScale = Table_Tamplet.Convert_Int(temp[__column++]);
                PhyPowerMinFix = Table_Tamplet.Convert_Int(temp[__column++]);
                PhyPowerMaxScale = Table_Tamplet.Convert_Int(temp[__column++]);
                PhyPowerMaxFix = Table_Tamplet.Convert_Int(temp[__column++]);
                MagPowerMinScale = Table_Tamplet.Convert_Int(temp[__column++]);
                MagPowerMinFix = Table_Tamplet.Convert_Int(temp[__column++]);
                MagPowerMaxScale = Table_Tamplet.Convert_Int(temp[__column++]);
                MagPowerMaxFix = Table_Tamplet.Convert_Int(temp[__column++]);
                PhyArmorScale = Table_Tamplet.Convert_Int(temp[__column++]);
                PhyArmorFix = Table_Tamplet.Convert_Int(temp[__column++]);
                MagArmorScale = Table_Tamplet.Convert_Int(temp[__column++]);
                MagArmorFix = Table_Tamplet.Convert_Int(temp[__column++]);
                HpMaxScale = Table_Tamplet.Convert_Int(temp[__column++]);
                HpMaxFix = Table_Tamplet.Convert_Int(temp[__column++]);
                PowerFightPoint = Table_Tamplet.Convert_Int(temp[__column++]);
                ArmorFightPoint = Table_Tamplet.Convert_Int(temp[__column++]);
                HpFightPoint = Table_Tamplet.Convert_Int(temp[__column++]);
                MpFightPoint = Table_Tamplet.Convert_Int(temp[__column++]);
                HitFightPoint = Table_Tamplet.Convert_Int(temp[__column++]);
                DodgeFightPoint = Table_Tamplet.Convert_Int(temp[__column++]);
                IgnoreArmorProFightPoint = Table_Tamplet.Convert_Int(temp[__column++]);
                DamageAddProFightPoint = Table_Tamplet.Convert_Int(temp[__column++]);
                DamageResProFightPoint = Table_Tamplet.Convert_Int(temp[__column++]);
                ExcellentProFightPoint = Table_Tamplet.Convert_Int(temp[__column++]);
                LuckyProFightPoint = Table_Tamplet.Convert_Int(temp[__column++]);
                DamageReboundProFightPoint = Table_Tamplet.Convert_Int(temp[__column++]);
                LeaveExpBase = Table_Tamplet.Convert_Int(temp[__column++]);
                FightingWayExp = Table_Tamplet.Convert_Int(temp[__column++]);
                FightingWayIncome = Table_Tamplet.Convert_Int(temp[__column++]);
                ElfExp = Table_Tamplet.Convert_Int(temp[__column++]);
                ElfResolveValue = Table_Tamplet.Convert_Int(temp[__column++]);
                WorshipExp = Table_Tamplet.Convert_Int(temp[__column++]);
                BeWorshipedExp = Table_Tamplet.Convert_Int(temp[__column++]);
                BeWorshipedMoney = Table_Tamplet.Convert_Int(temp[__column++]);
                Exp[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Exp[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Exp[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Exp[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Exp[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                FixedGorw = Table_Tamplet.Convert_Int(temp[__column++]);
                PercentGrow = Table_Tamplet.Convert_Int(temp[__column++]);
                UpNeedExp = Table_Tamplet.Convert_Int(temp[__column++]);
                FruitLimit[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                FruitLimit[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                FruitLimit[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                FruitLimit[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                DynamicExp = Table_Tamplet.Convert_Int(temp[__column++]);
                PetPointRatio = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class SceneNpcRecord :IRecord
    {
        public static string __TableName = "SceneNpc.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int DataID { get;        set; }
        public int SceneID { get;        set; }
        public double PosX { get;        set; }
        public double PosZ { get;        set; }
        public double FaceDirection { get;        set; }
        public int InitActivate { get;        set; }
        public int RandomStartID { get;        set; }
        public int RandomEndID { get;        set; }
        public int ChouHenGroupId { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                DataID = Table_Tamplet.Convert_Int(temp[__column++]);
                SceneID = Table_Tamplet.Convert_Int(temp[__column++]);
                PosX = Table_Tamplet.Convert_Double(temp[__column++]);
                PosZ = Table_Tamplet.Convert_Double(temp[__column++]);
                FaceDirection = Table_Tamplet.Convert_Double(temp[__column++]);
                InitActivate = Table_Tamplet.Convert_Int(temp[__column++]);
                RandomStartID = Table_Tamplet.Convert_Int(temp[__column++]);
                RandomEndID = Table_Tamplet.Convert_Int(temp[__column++]);
                ChouHenGroupId = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class SkillRecord :IRecord
    {
        public static string __TableName = "Skill.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int Type { get;        set; }
        public int NeedHp { get;        set; }
        public int NeedMp { get;        set; }
        public int NeedAnger { get;        set; }
        public int Cd { get;        set; }
        public int Layer { get;        set; }
        public int CommonCd { get;        set; }
        public int BulletId { get;        set; }
        public int ActionId { get;        set; }
        public int NoMove { get;        set; }
        public int CastType { get;        set; }
        public int[] CastParam = new int[4];
        public int ControlType { get;        set; }
        public int[] BeforeBuff = new int[2];
        public int TargetType { get;        set; }
        public int[] TargetParam = new int[6];
        public int CampType { get;        set; }
        public int DelayTarget { get;        set; }
        public int DelayView { get;        set; }
        public int TargetCount { get;        set; }
        public int[] AfterBuff = new int[2];
        public int[] MainTarget = new int[4];
        public int[] OtherTarget = new int[2];
        public int ExdataChange { get;        set; }
        public int HitType { get;        set; }
        public int Effect { get;        set; }
        public int NeedMoney { get;        set; }
        public int TalentMax { get;        set; }
        public int FightPoint { get;        set; }
        public int SkillID { get;        set; }
        public int ResetCount { get;        set; }
        public int IsEquipCanUse { get;        set; }
        public int TargetObjType { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedHp = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedMp = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedAnger = Table_Tamplet.Convert_Int(temp[__column++]);
                Cd = Table_Tamplet.Convert_Int(temp[__column++]);
                Layer = Table_Tamplet.Convert_Int(temp[__column++]);
                CommonCd = Table_Tamplet.Convert_Int(temp[__column++]);
                BulletId = Table_Tamplet.Convert_Int(temp[__column++]);
                ActionId = Table_Tamplet.Convert_Int(temp[__column++]);
                NoMove = Table_Tamplet.Convert_Int(temp[__column++]);
                CastType = Table_Tamplet.Convert_Int(temp[__column++]);
                CastParam[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                CastParam[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                CastParam[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                CastParam[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ControlType = Table_Tamplet.Convert_Int(temp[__column++]);
                BeforeBuff[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                BeforeBuff[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                TargetType = Table_Tamplet.Convert_Int(temp[__column++]);
                TargetParam[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                TargetParam[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                TargetParam[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                TargetParam[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                TargetParam[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                TargetParam[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                CampType = Table_Tamplet.Convert_Int(temp[__column++]);
                DelayTarget = Table_Tamplet.Convert_Int(temp[__column++]);
                DelayView = Table_Tamplet.Convert_Int(temp[__column++]);
                TargetCount = Table_Tamplet.Convert_Int(temp[__column++]);
                AfterBuff[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                AfterBuff[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                MainTarget[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                MainTarget[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                MainTarget[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                MainTarget[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                OtherTarget[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                OtherTarget[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ExdataChange = Table_Tamplet.Convert_Int(temp[__column++]);
                HitType = Table_Tamplet.Convert_Int(temp[__column++]);
                Effect = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedMoney = Table_Tamplet.Convert_Int(temp[__column++]);
                TalentMax = Table_Tamplet.Convert_Int(temp[__column++]);
                FightPoint = Table_Tamplet.Convert_Int(temp[__column++]);
                SkillID = Table_Tamplet.Convert_Int(temp[__column++]);
                ResetCount = Table_Tamplet.Convert_Int(temp[__column++]);
                IsEquipCanUse = Table_Tamplet.Convert_Int(temp[__column++]);
                TargetObjType = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class BulletRecord :IRecord
    {
        public static string __TableName = "Bullet.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public float Speed { get;        set; }
        public int[] Buff = new int[2];
        public int[] AttackMonsterID = new int[3];
        public int ShotDelay { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Speed = Table_Tamplet.Convert_Float(temp[__column++]);
                Buff[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Buff[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttackMonsterID[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttackMonsterID[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttackMonsterID[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ShotDelay = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class SceneRecord :IRecord
    {
        public static string __TableName = "Scene.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string Name { get;        set; }
        public string ResName { get;        set; }
        public int Type { get;        set; }
        public string Stem { get;        set; }
        public int Sound { get;        set; }
        public int[] ReliveType = new int[3];
        public int CityId { get;        set; }
        public double Entry_x { get;        set; }
        public double Entry_z { get;        set; }
        public double Safe_x { get;        set; }
        public double Safe_z { get;        set; }
        public int PvPRule { get;        set; }
        public int SwapLine { get;        set; }
        public int PlayersMaxA { get;        set; }
        public int PlayersMaxB { get;        set; }
        public int ScriptId { get;        set; }
        public int TerrainHeightMapWidth { get;        set; }
        public int TerrainHeightMapLength { get;        set; }
        public int FubenId { get;        set; }
        public int SeeArea { get;        set; }
        public double PVPPosX { get;        set; }
        public double PVPPosZ { get;        set; }
        public int IsPublic { get;        set; }
        public int LevelLimit { get;        set; }
        public int ConsumeMoney { get;        set; }
        public int CreateSceneRule { get;        set; }
        public int ReturnSceneID { get;        set; }
        public int CanSummonMonster { get;        set; }
        public int CanCrossServer  { get;        set; }
        public int[] AddBuff = new int[3];
        public int SafeReliveCD { get;        set; }
        public int IsCanRide { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Name = temp[__column++];
                ResName = temp[__column++];
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                Stem = temp[__column++];
                Sound = Table_Tamplet.Convert_Int(temp[__column++]);
                ReliveType[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ReliveType[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                CityId = Table_Tamplet.Convert_Int(temp[__column++]);
                ReliveType[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Entry_x = Table_Tamplet.Convert_Double(temp[__column++]);
                Entry_z = Table_Tamplet.Convert_Double(temp[__column++]);
                Safe_x = Table_Tamplet.Convert_Double(temp[__column++]);
                Safe_z = Table_Tamplet.Convert_Double(temp[__column++]);
                PvPRule = Table_Tamplet.Convert_Int(temp[__column++]);
                SwapLine = Table_Tamplet.Convert_Int(temp[__column++]);
                PlayersMaxA = Table_Tamplet.Convert_Int(temp[__column++]);
                PlayersMaxB = Table_Tamplet.Convert_Int(temp[__column++]);
                ScriptId = Table_Tamplet.Convert_Int(temp[__column++]);
                TerrainHeightMapWidth = Table_Tamplet.Convert_Int(temp[__column++]);
                TerrainHeightMapLength = Table_Tamplet.Convert_Int(temp[__column++]);
                FubenId = Table_Tamplet.Convert_Int(temp[__column++]);
                SeeArea = Table_Tamplet.Convert_Int(temp[__column++]);
                PVPPosX = Table_Tamplet.Convert_Double(temp[__column++]);
                PVPPosZ = Table_Tamplet.Convert_Double(temp[__column++]);
                IsPublic = Table_Tamplet.Convert_Int(temp[__column++]);
                LevelLimit = Table_Tamplet.Convert_Int(temp[__column++]);
                ConsumeMoney = Table_Tamplet.Convert_Int(temp[__column++]);
                CreateSceneRule = Table_Tamplet.Convert_Int(temp[__column++]);
                ReturnSceneID = Table_Tamplet.Convert_Int(temp[__column++]);
                CanSummonMonster = Table_Tamplet.Convert_Int(temp[__column++]);
                CanCrossServer  = Table_Tamplet.Convert_Int(temp[__column++]);
                AddBuff[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddBuff[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddBuff[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                SafeReliveCD = Table_Tamplet.Convert_Int(temp[__column++]);
                IsCanRide = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class TalentRecord :IRecord
    {
        public static string __TableName = "Talent.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int ActiveSkillId { get;        set; }
        public int ForgetSkillId { get;        set; }
        public int Icon { get;        set; }
        public int BeforeId { get;        set; }
        public int BeforeLayer { get;        set; }
        public int AttrId { get;        set; }
        public int SkillupgradingId { get;        set; }
        public int MaxLayer { get;        set; }
        public int HuchiId { get;        set; }
        public int ModifySkill { get;        set; }
        public int FightPointBySkillUpgrading { get;        set; }
        public int[] BuffId = new int[10];
        public int CastItemId { get;        set; }
        public int CastItemCount { get;        set; }
        public int NeedLevel { get;        set; }
        public int SkillItem { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                ActiveSkillId = Table_Tamplet.Convert_Int(temp[__column++]);
                ForgetSkillId = Table_Tamplet.Convert_Int(temp[__column++]);
                Icon = Table_Tamplet.Convert_Int(temp[__column++]);
                BeforeId = Table_Tamplet.Convert_Int(temp[__column++]);
                BeforeLayer = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrId = Table_Tamplet.Convert_Int(temp[__column++]);
                SkillupgradingId = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxLayer = Table_Tamplet.Convert_Int(temp[__column++]);
                HuchiId = Table_Tamplet.Convert_Int(temp[__column++]);
                ModifySkill = Table_Tamplet.Convert_Int(temp[__column++]);
                FightPointBySkillUpgrading = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffId[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffId[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffId[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffId[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffId[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffId[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffId[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffId[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                CastItemId = Table_Tamplet.Convert_Int(temp[__column++]);
                CastItemCount = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedLevel = Table_Tamplet.Convert_Int(temp[__column++]);
                SkillItem = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class NpcBaseRecord :IRecord
    {
        public static string __TableName = "NpcBase.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string Name { get;        set; }
        public int mAI { get;        set; }
        public int NpcType { get;        set; }
        public int Level { get;        set; }
        public int DialogRadius { get;        set; }
        public int Patrol { get;        set; }
        public double PatrolRadius { get;        set; }
        public double ViewDistance { get;        set; }
        public double MaxCombatDistance { get;        set; }
        public int BornEffctID { get;        set; }
        public int DieEffectID { get;        set; }
        public int IsAttackFly { get;        set; }
        public int CorpseTime { get;        set; }
        public int IsReviveTime { get;        set; }
        public int ReviveTime { get;        set; }
        public List<int> RefreshTime = new List<int>();
        public int Exp { get;        set; }
        public int AIID { get;        set; }
        public int BelongType { get;        set; }
        public int DropId { get;        set; }
        public int Interactive { get;        set; }
        public int[] Service = new int[2];
        public int Spare { get;        set; }
        public float NPCStopRadius { get;        set; }
        public int HeartRate { get;        set; }
        public int IsDynamicExp { get;        set; }
        public int DynamicExpRatio { get;        set; }
        public int ExpMultiple { get;        set; }
        public int IsWorldExpAdd { get;        set; }
        public string[] ServerParam = new string[2];
        public int LeaveFightAddBlood { get;        set; }
        public int KillExpendType { get;        set; }
        public int KillExpendValue { get;        set; }
        public int WorldBroadCastDic { get;        set; }
        public int IsShowItemOwnerIcon { get;        set; }
        public int LimitFlag { get;        set; }
        public int LimitTimes { get;        set; }
        public List<int> SpecialDrops = new List<int>();
        public List<int> YunYingIds = new List<int>();
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Name = temp[__column++];
                mAI = Table_Tamplet.Convert_Int(temp[__column++]);
                NpcType = Table_Tamplet.Convert_Int(temp[__column++]);
                Level = Table_Tamplet.Convert_Int(temp[__column++]);
                DialogRadius = Table_Tamplet.Convert_Int(temp[__column++]);
                Patrol = Table_Tamplet.Convert_Int(temp[__column++]);
                PatrolRadius = Table_Tamplet.Convert_Double(temp[__column++]);
                ViewDistance = Table_Tamplet.Convert_Double(temp[__column++]);
                MaxCombatDistance = Table_Tamplet.Convert_Double(temp[__column++]);
                BornEffctID = Table_Tamplet.Convert_Int(temp[__column++]);
                DieEffectID = Table_Tamplet.Convert_Int(temp[__column++]);
                IsAttackFly = Table_Tamplet.Convert_Int(temp[__column++]);
                CorpseTime = Table_Tamplet.Convert_Int(temp[__column++]);
                IsReviveTime = Table_Tamplet.Convert_Int(temp[__column++]);
                ReviveTime = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(RefreshTime,temp[__column++]);
                Exp = Table_Tamplet.Convert_Int(temp[__column++]);
                AIID = Table_Tamplet.Convert_Int(temp[__column++]);
                BelongType = Table_Tamplet.Convert_Int(temp[__column++]);
                DropId = Table_Tamplet.Convert_Int(temp[__column++]);
                Interactive = Table_Tamplet.Convert_Int(temp[__column++]);
                Service[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Service[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Spare = Table_Tamplet.Convert_Int(temp[__column++]);
                NPCStopRadius = Table_Tamplet.Convert_Float(temp[__column++]);
                HeartRate = Table_Tamplet.Convert_Int(temp[__column++]);
                IsDynamicExp = Table_Tamplet.Convert_Int(temp[__column++]);
                DynamicExpRatio = Table_Tamplet.Convert_Int(temp[__column++]);
                ExpMultiple = Table_Tamplet.Convert_Int(temp[__column++]);
                IsWorldExpAdd = Table_Tamplet.Convert_Int(temp[__column++]);
                ServerParam[0]  = temp[__column++];
                ServerParam[1]  = temp[__column++];
                LeaveFightAddBlood = Table_Tamplet.Convert_Int(temp[__column++]);
                KillExpendType = Table_Tamplet.Convert_Int(temp[__column++]);
                KillExpendValue = Table_Tamplet.Convert_Int(temp[__column++]);
                WorldBroadCastDic = Table_Tamplet.Convert_Int(temp[__column++]);
                IsShowItemOwnerIcon = Table_Tamplet.Convert_Int(temp[__column++]);
                LimitFlag = Table_Tamplet.Convert_Int(temp[__column++]);
                LimitTimes = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(SpecialDrops,temp[__column++]);
                Table_Tamplet.Convert_Value(YunYingIds,temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class SkillUpgradingRecord :IRecord
    {
        public static string __TableName = "SkillUpgrading.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int Type { get;        set; }
        public List<int> Values = new List<int>();
        public int[] Param = new int[5];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(Values,temp[__column++]);
                Param[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[4] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class AchievementRecord :IRecord
    {
        public static string __TableName = "Achievement.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int Type { get;        set; }
        public string Name { get;        set; }
        public int ViewLevel { get;        set; }
        public int Exdata { get;        set; }
        public int ExdataCount { get;        set; }
        public List<int> FlagList = new List<int>();
        public int AchievementPoint { get;        set; }
        public int[] ItemId = new int[3];
        public int[] ItemCount = new int[3];
        public int RewardFlagId { get;        set; }
        public int FinishFlagId { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                Name = temp[__column++];
                ViewLevel = Table_Tamplet.Convert_Int(temp[__column++]);
                Exdata = Table_Tamplet.Convert_Int(temp[__column++]);
                ExdataCount = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(FlagList,temp[__column++]);
                AchievementPoint = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardFlagId = Table_Tamplet.Convert_Int(temp[__column++]);
                FinishFlagId = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class ScriptRecord :IRecord
    {
        public static string __TableName = "Script.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string Path { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Path = temp[__column++];
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class DropMotherRecord :IRecord
    {
        public static string __TableName = "DropMother.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int SurpriseDrop { get;        set; }
        public int SurprisePro { get;        set; }
        public int ExdataId { get;        set; }
        public int ExdataCount { get;        set; }
        public int ExdataDropId { get;        set; }
        public int[] Group = new int[10];
        public int[] Pro = new int[10];
        public int[] DropSon = new int[10];
        public int[] DropMin = new int[10];
        public int[] DropMax = new int[10];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                SurpriseDrop = Table_Tamplet.Convert_Int(temp[__column++]);
                SurprisePro = Table_Tamplet.Convert_Int(temp[__column++]);
                ExdataId = Table_Tamplet.Convert_Int(temp[__column++]);
                ExdataCount = Table_Tamplet.Convert_Int(temp[__column++]);
                ExdataDropId = Table_Tamplet.Convert_Int(temp[__column++]);
                Group[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropSon[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropMin[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropMax[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Group[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropSon[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropMin[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropMax[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Group[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropSon[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropMin[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropMax[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Group[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropSon[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropMin[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropMax[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Group[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropSon[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropMin[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropMax[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Group[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropSon[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropMin[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropMax[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                Group[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropSon[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropMin[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropMax[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                Group[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropSon[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropMin[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropMax[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                Group[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropSon[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropMin[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropMax[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                Group[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropSon[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropMin[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropMax[9] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class DropSonRecord :IRecord
    {
        public static string __TableName = "DropSon.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int DropType { get;        set; }
        public int TotlePro { get;        set; }
        public int[] Item = new int[48];
        public int[] Count = new int[48];
        public int[] MaxCount = new int[48];
        public int[] Pro = new int[48];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                DropType = Table_Tamplet.Convert_Int(temp[__column++]);
                TotlePro = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[10] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[10] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[10] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[10] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[11] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[11] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[11] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[11] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[12] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[12] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[12] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[12] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[13] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[13] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[13] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[13] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[14] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[14] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[14] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[14] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[15] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[15] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[15] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[15] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[16] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[16] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[16] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[16] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[17] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[17] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[17] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[17] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[18] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[18] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[18] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[18] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[19] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[19] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[19] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[19] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[20] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[20] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[20] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[20] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[21] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[21] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[21] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[21] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[22] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[22] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[22] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[22] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[23] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[23] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[23] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[23] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[24] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[24] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[24] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[24] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[25] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[25] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[25] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[25] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[26] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[26] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[26] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[26] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[27] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[27] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[27] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[27] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[28] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[28] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[28] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[28] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[29] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[29] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[29] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[29] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[30] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[30] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[30] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[30] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[31] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[31] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[31] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[31] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[32] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[32] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[32] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[32] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[33] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[33] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[33] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[33] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[34] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[34] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[34] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[34] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[35] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[35] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[35] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[35] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[36] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[36] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[36] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[36] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[37] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[37] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[37] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[37] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[38] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[38] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[38] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[38] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[39] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[39] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[39] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[39] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[40] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[40] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[40] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[40] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[41] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[41] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[41] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[41] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[42] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[42] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[42] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[42] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[43] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[43] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[43] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[43] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[44] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[44] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[44] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[44] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[45] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[45] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[45] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[45] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[46] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[46] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[46] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[46] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[47] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[47] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount[47] = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro[47] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class EquipTieRecord :IRecord
    {
        public static string __TableName = "EquipTie.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string Name { get;        set; }
        public int AllCount { get;        set; }
        public int[] NeedCount = new int[4];
        public int[] Attr1Id = new int[4];
        public int[] Attr1Value = new int[4];
        public int[] Attr2Id = new int[4];
        public int[] Attr2Value = new int[4];
        public int[] BuffId = new int[4];
        public int[] FightPoint = new int[4];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Name = temp[__column++];
                AllCount = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr1Id[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr1Value[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr2Id[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr2Value[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                FightPoint[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr1Id[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr1Value[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr2Id[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr2Value[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                FightPoint[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedCount[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr1Id[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr1Value[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr2Id[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr2Value[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffId[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                FightPoint[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedCount[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr1Id[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr1Value[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr2Id[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr2Value[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffId[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                FightPoint[3] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class TransferRecord :IRecord
    {
        public static string __TableName = "Transfer.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int FromSceneId { get;        set; }
        public float FromX { get;        set; }
        public float FromY { get;        set; }
        public int NeedTime { get;        set; }
        public int ToSceneId { get;        set; }
        public float ToX { get;        set; }
        public float ToY { get;        set; }
        public float TransferRadius { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                FromSceneId = Table_Tamplet.Convert_Int(temp[__column++]);
                FromX = Table_Tamplet.Convert_Float(temp[__column++]);
                FromY = Table_Tamplet.Convert_Float(temp[__column++]);
                NeedTime = Table_Tamplet.Convert_Int(temp[__column++]);
                ToSceneId = Table_Tamplet.Convert_Int(temp[__column++]);
                ToX = Table_Tamplet.Convert_Float(temp[__column++]);
                ToY = Table_Tamplet.Convert_Float(temp[__column++]);
                TransferRadius = Table_Tamplet.Convert_Float(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class ServerConfigRecord :IRecord
    {
        public static string __TableName = "ServerConfig.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string Value { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Value = temp[__column++];
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class AIRecord :IRecord
    {
        public static string __TableName = "AI.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int CommonSkill { get;        set; }
        public int[] SpecialSkill = new int[4];
        public int[] Cd = new int[4];
        public int[] InitCd = new int[4];
        public int HatreType { get;        set; }
        public int SortType { get;        set; }
        public int NextAI { get;        set; }
        public int NextAICondition { get;        set; }
        public int Type { get;        set; }
        public int Param { get;        set; }
        public int EnterSpeak { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                CommonSkill = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialSkill[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Cd[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                InitCd[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialSkill[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Cd[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                InitCd[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialSkill[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Cd[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                InitCd[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialSkill[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Cd[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                InitCd[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                HatreType = Table_Tamplet.Convert_Int(temp[__column++]);
                SortType = Table_Tamplet.Convert_Int(temp[__column++]);
                NextAI = Table_Tamplet.Convert_Int(temp[__column++]);
                NextAICondition = Table_Tamplet.Convert_Int(temp[__column++]);
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                Param = Table_Tamplet.Convert_Int(temp[__column++]);
                EnterSpeak = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class EventRecord :IRecord
    {
        public static string __TableName = "Event.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string Name { get;        set; }
        public int Enable { get;        set; }
        public string TriggerTime { get;        set; }
        public int DurationType { get;        set; }
        public int DurationParam { get;        set; }
        public int CacheCount { get;        set; }
        public string Action { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Name = temp[__column++];
                Enable = Table_Tamplet.Convert_Int(temp[__column++]);
                TriggerTime = temp[__column++];
                DurationType = Table_Tamplet.Convert_Int(temp[__column++]);
                DurationParam = Table_Tamplet.Convert_Int(temp[__column++]);
                CacheCount = Table_Tamplet.Convert_Int(temp[__column++]);
                Action = temp[__column++];
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class DropConfigRecord :IRecord
    {
        public static string __TableName = "DropConfig.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] Param = new int[12];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[10] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[11] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class GiftRecord :IRecord
    {
        public static string __TableName = "Gift.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int Type { get;        set; }
        public int Flag { get;        set; }
        public int Exdata { get;        set; }
        public int[] Param = new int[12];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                Flag = Table_Tamplet.Convert_Int(temp[__column++]);
                Exdata = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[10] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[11] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class EquipBlessingRecord :IRecord
    {
        public static string __TableName = "EquipBlessing.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int Probability { get;        set; }
        public int[] NeedItemId = new int[3];
        public int[] NeedItemCount = new int[3];
        public int NeedMoney { get;        set; }
        public int WarrantItemId { get;        set; }
        public int MoreChance { get;        set; }
        public int WarrantItemCount { get;        set; }
        public int FalseLevel { get;        set; }
        public int SpecialId { get;        set; }
        public int SmritiMoney { get;        set; }
        public int SmritiGold { get;        set; }
        public int[] CallBackItem = new int[3];
        public int[] CallBackCount = new int[3];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Probability = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemId[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemCount[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedMoney = Table_Tamplet.Convert_Int(temp[__column++]);
                WarrantItemId = Table_Tamplet.Convert_Int(temp[__column++]);
                MoreChance = Table_Tamplet.Convert_Int(temp[__column++]);
                WarrantItemCount = Table_Tamplet.Convert_Int(temp[__column++]);
                FalseLevel = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialId = Table_Tamplet.Convert_Int(temp[__column++]);
                SmritiMoney = Table_Tamplet.Convert_Int(temp[__column++]);
                SmritiGold = Table_Tamplet.Convert_Int(temp[__column++]);
                CallBackItem[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                CallBackCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                CallBackItem[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                CallBackCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                CallBackItem[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                CallBackCount[2] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class EquipAdditionalRecord :IRecord
    {
        public static string __TableName = "EquipAdditional.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int HpMax { get;        set; }
        public int Power { get;        set; }
        public int NeedItemId { get;        set; }
        public int NeedItemCount { get;        set; }
        public int NeedMoney { get;        set; }
        public int SmritiMoney { get;        set; }
        public int SmritiGold { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                HpMax = Table_Tamplet.Convert_Int(temp[__column++]);
                Power = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemId = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemCount = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedMoney = Table_Tamplet.Convert_Int(temp[__column++]);
                SmritiMoney = Table_Tamplet.Convert_Int(temp[__column++]);
                SmritiGold = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class EquipExcellentRecord :IRecord
    {
        public static string __TableName = "EquipExcellent.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int GreenItemId { get;        set; }
        public int GreenItemCount { get;        set; }
        public int GreenMoney { get;        set; }
        public int ItemId { get;        set; }
        public int ItemCount { get;        set; }
        public int LockId { get;        set; }
        public int[] LockCount = new int[5];
        public int[] Money = new int[6];
        public int SmritiMoney { get;        set; }
        public int SmritiGold { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                GreenItemId = Table_Tamplet.Convert_Int(temp[__column++]);
                GreenItemCount = Table_Tamplet.Convert_Int(temp[__column++]);
                GreenMoney = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount = Table_Tamplet.Convert_Int(temp[__column++]);
                LockId = Table_Tamplet.Convert_Int(temp[__column++]);
                LockCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                LockCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                LockCount[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                LockCount[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                LockCount[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Money[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Money[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Money[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Money[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Money[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Money[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                SmritiMoney = Table_Tamplet.Convert_Int(temp[__column++]);
                SmritiGold = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class HandBookRecord :IRecord
    {
        public static string __TableName = "HandBook.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string Name { get;        set; }
        public int PieceId { get;        set; }
        public int Count { get;        set; }
        public int AttrId { get;        set; }
        public int AttrValue { get;        set; }
        public int Money { get;        set; }
        public int NpcId { get;        set; }
        public int SkillId { get;        set; }
        public List<int> ListCost = new List<int>();
        public int MaxLevel { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Name = temp[__column++];
                PieceId = Table_Tamplet.Convert_Int(temp[__column++]);
                Count = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrId = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrValue = Table_Tamplet.Convert_Int(temp[__column++]);
                Money = Table_Tamplet.Convert_Int(temp[__column++]);
                NpcId = Table_Tamplet.Convert_Int(temp[__column++]);
                SkillId = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(ListCost,temp[__column++]);
                MaxLevel = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class BookGroupRecord :IRecord
    {
        public static string __TableName = "BookGroup.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int GroupId { get;        set; }
        public int Level { get;        set; }
        public int[] ItemId = new int[6];
        public int[] AttrId = new int[6];
        public int[] AttrValue = new int[6];
        public int[] GroupAttrId = new int[4];
        public int[] GroupAttrValue = new int[4];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                GroupId = Table_Tamplet.Convert_Int(temp[__column++]);
                Level = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrValue[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrValue[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrId[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrValue[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrId[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrValue[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrId[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrValue[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrId[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrValue[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                GroupAttrId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                GroupAttrValue[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                GroupAttrId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                GroupAttrValue[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                GroupAttrId[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                GroupAttrValue[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                GroupAttrId[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                GroupAttrValue[3] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class ItemComposeRecord :IRecord
    {
        public static string __TableName = "ItemCompose.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int Type { get;        set; }
        public int[] ComposeId = new int[8];
        public int[] ProId = new int[8];
        public int[] NeedId = new int[4];
        public int[] NeedCount = new int[4];
        public int NeedRes { get;        set; }
        public int NeedValue { get;        set; }
        public int Pro { get;        set; }
        public int SortByCareer { get;        set; }
        public int ComposeOpenLevel { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                ComposeId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ProId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ComposeId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ProId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ComposeId[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ProId[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ComposeId[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ProId[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ComposeId[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                ProId[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                ComposeId[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                ProId[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                ComposeId[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                ProId[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                ComposeId[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                ProId[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedId[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedCount[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedId[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedCount[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedRes = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedValue = Table_Tamplet.Convert_Int(temp[__column++]);
                Pro = Table_Tamplet.Convert_Int(temp[__column++]);
                SortByCareer = Table_Tamplet.Convert_Int(temp[__column++]);
                ComposeOpenLevel = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class CampRecord :IRecord
    {
        public static string __TableName = "Camp.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] Camp = new int[10];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Camp[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Camp[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Camp[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Camp[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Camp[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Camp[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                Camp[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                Camp[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                Camp[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                Camp[9] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class FubenRecord :IRecord
    {
        public static string __TableName = "Fuben.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string Name { get;        set; }
        public int MainType { get;        set; }
        public int AssistType { get;        set; }
        public int Difficulty { get;        set; }
        public int BeforeStoryId { get;        set; }
        public int AfterStoryId { get;        set; }
        public int TodayCount { get;        set; }
        public int TodayBuyCount { get;        set; }
        public int[] NeedItemId = new int[2];
        public int[] NeedItemCount = new int[2];
        public int ResetItemId { get;        set; }
        public int ResetItemCount { get;        set; }
        public int ViewConditionId { get;        set; }
        public int EnterConditionId { get;        set; }
        public List<int> OpenTime = new List<int>();
        public int CanEnterTime { get;        set; }
        public int OpenLastMinutes { get;        set; }
        public float TimeLimitMinutes { get;        set; }
        public int SweepLimitMinutes { get;        set; }
        public int FightPoint { get;        set; }
        public string Desc { get;        set; }
        public int[] RewardId = new int[4];
        public int[] RewardCount = new int[4];
        public int ScriptId { get;        set; }
        public int FlagId { get;        set; }
        public int TodayCountExdata { get;        set; }
        public int ResetExdata { get;        set; }
        public int TotleExdata { get;        set; }
        public int SceneId { get;        set; }
        public int TimeExdata { get;        set; }
        public int QueueParam { get;        set; }
        public int DrawReward { get;        set; }
        public int ScanExp { get;        set; }
        public int ScanGold { get;        set; }
        public int[] ScanReward = new int[2];
        public int FubenCountNode { get;        set; }
        public int FubenInfoParam { get;        set; }
        public int FubenLogicID { get;        set; }
        public int MainHouseGetExp { get;        set; }
        public int IsDynamicExp { get;        set; }
        public int DynamicExpRatio { get;        set; }
        public int ScanDynamicExpRatio { get;        set; }
        public int CanInspire { get;        set; }
        public int CanGroupEnter { get;        set; }
        public int IsDyncDifficulty { get;        set; }
        public int IsDyncReward { get;        set; }
        public int IsStarReward { get;        set; }
        public int BusinessManSceneId { get;        set; }
        public int BusinessManPR { get;        set; }
        public int StarRewardProb { get;        set; }
        public string MieshiScoreDesc { get;        set; }
        public string MieshiRewardDesc { get;        set; }
        public int iParam1 { get;        set; }
        public List<int> lParam1 = new List<int>();
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Name = temp[__column++];
                MainType = Table_Tamplet.Convert_Int(temp[__column++]);
                AssistType = Table_Tamplet.Convert_Int(temp[__column++]);
                Difficulty = Table_Tamplet.Convert_Int(temp[__column++]);
                BeforeStoryId = Table_Tamplet.Convert_Int(temp[__column++]);
                AfterStoryId = Table_Tamplet.Convert_Int(temp[__column++]);
                TodayCount = Table_Tamplet.Convert_Int(temp[__column++]);
                TodayBuyCount = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ResetItemId = Table_Tamplet.Convert_Int(temp[__column++]);
                ResetItemCount = Table_Tamplet.Convert_Int(temp[__column++]);
                ViewConditionId = Table_Tamplet.Convert_Int(temp[__column++]);
                EnterConditionId = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(OpenTime,temp[__column++]);
                CanEnterTime = Table_Tamplet.Convert_Int(temp[__column++]);
                OpenLastMinutes = Table_Tamplet.Convert_Int(temp[__column++]);
                TimeLimitMinutes = Table_Tamplet.Convert_Float(temp[__column++]);
                SweepLimitMinutes = Table_Tamplet.Convert_Int(temp[__column++]);
                FightPoint = Table_Tamplet.Convert_Int(temp[__column++]);
                Desc = temp[__column++];
                RewardId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardId[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardCount[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardId[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardCount[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ScriptId = Table_Tamplet.Convert_Int(temp[__column++]);
                FlagId = Table_Tamplet.Convert_Int(temp[__column++]);
                TodayCountExdata = Table_Tamplet.Convert_Int(temp[__column++]);
                ResetExdata = Table_Tamplet.Convert_Int(temp[__column++]);
                TotleExdata = Table_Tamplet.Convert_Int(temp[__column++]);
                SceneId = Table_Tamplet.Convert_Int(temp[__column++]);
                TimeExdata = Table_Tamplet.Convert_Int(temp[__column++]);
                QueueParam = Table_Tamplet.Convert_Int(temp[__column++]);
                DrawReward = Table_Tamplet.Convert_Int(temp[__column++]);
                ScanExp = Table_Tamplet.Convert_Int(temp[__column++]);
                ScanGold = Table_Tamplet.Convert_Int(temp[__column++]);
                ScanReward[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ScanReward[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                FubenCountNode = Table_Tamplet.Convert_Int(temp[__column++]);
                FubenInfoParam = Table_Tamplet.Convert_Int(temp[__column++]);
                FubenLogicID = Table_Tamplet.Convert_Int(temp[__column++]);
                MainHouseGetExp = Table_Tamplet.Convert_Int(temp[__column++]);
                IsDynamicExp = Table_Tamplet.Convert_Int(temp[__column++]);
                DynamicExpRatio = Table_Tamplet.Convert_Int(temp[__column++]);
                ScanDynamicExpRatio = Table_Tamplet.Convert_Int(temp[__column++]);
                CanInspire = Table_Tamplet.Convert_Int(temp[__column++]);
                CanGroupEnter = Table_Tamplet.Convert_Int(temp[__column++]);
                IsDyncDifficulty = Table_Tamplet.Convert_Int(temp[__column++]);
                IsDyncReward = Table_Tamplet.Convert_Int(temp[__column++]);
                IsStarReward = Table_Tamplet.Convert_Int(temp[__column++]);
                BusinessManSceneId = Table_Tamplet.Convert_Int(temp[__column++]);
                BusinessManPR = Table_Tamplet.Convert_Int(temp[__column++]);
                StarRewardProb = Table_Tamplet.Convert_Int(temp[__column++]);
                MieshiScoreDesc = temp[__column++];
                MieshiRewardDesc = temp[__column++];
                iParam1 = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(lParam1,temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class SkillAreaRecord :IRecord
    {
        public static string __TableName = "SkillArea.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int TargetType { get;        set; }
        public int[] TargetParam = new int[6];
        public int CampType { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                TargetType = Table_Tamplet.Convert_Int(temp[__column++]);
                TargetParam[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                TargetParam[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                TargetParam[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                TargetParam[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                TargetParam[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                TargetParam[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                CampType = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class StatsRecord :IRecord
    {
        public static string __TableName = "Stats.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] FightPoint = new int[3];
        public int PetFight { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                FightPoint[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                FightPoint[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                FightPoint[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                PetFight = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class StoreRecord :IRecord
    {
        public static string __TableName = "Store.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int ItemId { get;        set; }
        public int ItemCount { get;        set; }
        public int Type { get;        set; }
        public int NeedType { get;        set; }
        public int NeedValue { get;        set; }
        public int DayCount { get;        set; }
        public int WeekCount { get;        set; }
        public int MonthCount { get;        set; }
        public int FuBenCount { get;        set; }
        public int GroupId { get;        set; }
        public int EventId { get;        set; }
        public int NeedItem { get;        set; }
        public int NeedCount { get;        set; }
        public int SeeCharacterID { get;        set; }
        public int BugSign { get;        set; }
        public int DisplayCondition { get;        set; }
        public int BuyCondition { get;        set; }
        public int WaveValue { get;        set; }
        public int BlessGrow { get;        set; }
        public int BlessUp { get;        set; }
        public int BlessupBuy { get;        set; }
        public int Order { get;        set; }
        public int Weight { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount = Table_Tamplet.Convert_Int(temp[__column++]);
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedType = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedValue = Table_Tamplet.Convert_Int(temp[__column++]);
                DayCount = Table_Tamplet.Convert_Int(temp[__column++]);
                WeekCount = Table_Tamplet.Convert_Int(temp[__column++]);
                MonthCount = Table_Tamplet.Convert_Int(temp[__column++]);
                FuBenCount = Table_Tamplet.Convert_Int(temp[__column++]);
                GroupId = Table_Tamplet.Convert_Int(temp[__column++]);
                EventId = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItem = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedCount = Table_Tamplet.Convert_Int(temp[__column++]);
                SeeCharacterID = Table_Tamplet.Convert_Int(temp[__column++]);
                BugSign = Table_Tamplet.Convert_Int(temp[__column++]);
                DisplayCondition = Table_Tamplet.Convert_Int(temp[__column++]);
                BuyCondition = Table_Tamplet.Convert_Int(temp[__column++]);
                WaveValue = Table_Tamplet.Convert_Int(temp[__column++]);
                BlessGrow = Table_Tamplet.Convert_Int(temp[__column++]);
                BlessUp = Table_Tamplet.Convert_Int(temp[__column++]);
                BlessupBuy = Table_Tamplet.Convert_Int(temp[__column++]);
                Order = Table_Tamplet.Convert_Int(temp[__column++]);
                Weight = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class InitItemRecord :IRecord
    {
        public static string __TableName = "InitItem.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int ItemId { get;        set; }
        public int ItemCount { get;        set; }
        public int Type { get;        set; }
        public int BagId { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount = Table_Tamplet.Convert_Int(temp[__column++]);
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                BagId = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class TriggerAreaRecord :IRecord
    {
        public static string __TableName = "TriggerArea.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int SceneId { get;        set; }
        public float PosX { get;        set; }
        public float PosZ { get;        set; }
        public float Radius { get;        set; }
        public int ClientAnimation { get;        set; }
        public int OffLineTrigger { get;        set; }
        public int AreaType { get;        set; }
        public int[] Param = new int[4];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                SceneId = Table_Tamplet.Convert_Int(temp[__column++]);
                PosX = Table_Tamplet.Convert_Float(temp[__column++]);
                PosZ = Table_Tamplet.Convert_Float(temp[__column++]);
                Radius = Table_Tamplet.Convert_Float(temp[__column++]);
                ClientAnimation = Table_Tamplet.Convert_Int(temp[__column++]);
                OffLineTrigger = Table_Tamplet.Convert_Int(temp[__column++]);
                AreaType = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[3] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class MailRecord :IRecord
    {
        public static string __TableName = "Mail.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string Sender { get;        set; }
        public string Title { get;        set; }
        public string Text { get;        set; }
        public int Condition { get;        set; }
        public int[] ItemId = new int[5];
        public int[] ItemCount = new int[5];
        public int Flag { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Sender = temp[__column++];
                Title = temp[__column++];
                Text = temp[__column++];
                Condition = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Flag = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class BuildingRecord :IRecord
    {
        public static string __TableName = "Building.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int PetCount { get;        set; }
        public int Type { get;        set; }
        public int AreaType { get;        set; }
        public int Level { get;        set; }
        public int BuildMaxLevel { get;        set; }
        public int NextId { get;        set; }
        public int GetMainHouseExp { get;        set; }
        public int ServerGetExpFix { get;        set; }
        public int NeedHomeLevel { get;        set; }
        public int[] NeedItemId = new int[2];
        public int[] NeedItemCount = new int[2];
        public int NeedMinutes { get;        set; }
        public int CanRemove { get;        set; }
        public int RemoveNeedCityLevel { get;        set; }
        public int RemoveNeedRes { get;        set; }
        public int RemoveNeedCount { get;        set; }
        public int RemovedBuildID { get;        set; }
        public int ServiceId { get;        set; }
        public int FlagId { get;        set; }
        public int OrderRefleshRule { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                PetCount = Table_Tamplet.Convert_Int(temp[__column++]);
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                AreaType = Table_Tamplet.Convert_Int(temp[__column++]);
                Level = Table_Tamplet.Convert_Int(temp[__column++]);
                BuildMaxLevel = Table_Tamplet.Convert_Int(temp[__column++]);
                NextId = Table_Tamplet.Convert_Int(temp[__column++]);
                GetMainHouseExp = Table_Tamplet.Convert_Int(temp[__column++]);
                ServerGetExpFix = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedHomeLevel = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedMinutes = Table_Tamplet.Convert_Int(temp[__column++]);
                CanRemove = Table_Tamplet.Convert_Int(temp[__column++]);
                RemoveNeedCityLevel = Table_Tamplet.Convert_Int(temp[__column++]);
                RemoveNeedRes = Table_Tamplet.Convert_Int(temp[__column++]);
                RemoveNeedCount = Table_Tamplet.Convert_Int(temp[__column++]);
                RemovedBuildID = Table_Tamplet.Convert_Int(temp[__column++]);
                ServiceId = Table_Tamplet.Convert_Int(temp[__column++]);
                FlagId = Table_Tamplet.Convert_Int(temp[__column++]);
                OrderRefleshRule = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class BuildingRuleRecord :IRecord
    {
        public static string __TableName = "BuildingRule.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] CityLevel = new int[10];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                CityLevel[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                CityLevel[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                CityLevel[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                CityLevel[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                CityLevel[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                CityLevel[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                CityLevel[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                CityLevel[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                CityLevel[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                CityLevel[9] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class BuildingServiceRecord :IRecord
    {
        public static string __TableName = "BuildingService.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int RewardParam { get;        set; }
        public int RewardRule { get;        set; }
        public int[] RewardID = new int[3];
        public int[] RewardCountMin = new int[3];
        public int[] RewardCountMax = new int[3];
        public int BuildingType { get;        set; }
        public int[] Param = new int[8];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardParam = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardRule = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardID[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardCountMin[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardCountMax[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardID[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardCountMin[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardCountMax[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardID[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardCountMin[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardCountMax[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuildingType = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[7] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class HomeSenceRecord :IRecord
    {
        public static string __TableName = "HomeSence.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int BuildId { get;        set; }
        public float RetinuePosX { get;        set; }
        public float RetinuePosY { get;        set; }
        public int BuildType { get;        set; }
        public float FaceCorrection { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                BuildId = Table_Tamplet.Convert_Int(temp[__column++]);
                RetinuePosX = Table_Tamplet.Convert_Float(temp[__column++]);
                RetinuePosY = Table_Tamplet.Convert_Float(temp[__column++]);
                BuildType = Table_Tamplet.Convert_Int(temp[__column++]);
                FaceCorrection = Table_Tamplet.Convert_Float(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class PetRecord :IRecord
    {
        public static string __TableName = "Pet.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int CharacterID { get;        set; }
        public int Type { get;        set; }
        public int Ladder { get;        set; }
        public int MaxLadder { get;        set; }
        public int NeedItemId { get;        set; }
        public int NeedItemCount { get;        set; }
        public int NeedTime { get;        set; }
        public int NextId { get;        set; }
        public int[] Skill = new int[4];
        public int[] ActiveLadder = new int[4];
        public int[] Speciality = new int[3];
        public int NeedExp { get;        set; }
        public int AttrRef { get;        set; }
        public int[] SpecialityLibrary = new int[3];
        public int ResolvePartCount { get;        set; }
        public int IsCanView { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                CharacterID = Table_Tamplet.Convert_Int(temp[__column++]);
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                Ladder = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxLadder = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemId = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemCount = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedTime = Table_Tamplet.Convert_Int(temp[__column++]);
                NextId = Table_Tamplet.Convert_Int(temp[__column++]);
                Skill[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ActiveLadder[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Skill[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ActiveLadder[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Skill[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ActiveLadder[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Skill[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ActiveLadder[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Speciality[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Speciality[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Speciality[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedExp = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrRef = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialityLibrary[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialityLibrary[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialityLibrary[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ResolvePartCount = Table_Tamplet.Convert_Int(temp[__column++]);
                IsCanView = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class PetSkillRecord :IRecord
    {
        public static string __TableName = "PetSkill.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int SkillType { get;        set; }
        public int MatchType { get;        set; }
        public int EffectId { get;        set; }
        public int[] Param = new int[6];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                SkillType = Table_Tamplet.Convert_Int(temp[__column++]);
                MatchType = Table_Tamplet.Convert_Int(temp[__column++]);
                EffectId = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[5] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class ServiceRecord :IRecord
    {
        public static string __TableName = "Service.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int Type { get;        set; }
        public int[] Param = new int[4];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[3] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class StoreTypeRecord :IRecord
    {
        public static string __TableName = "StoreType.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class PetSkillBaseRecord :IRecord
    {
        public static string __TableName = "PetSkillBase.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] Speciality = new int[20];
        public int[] SpecialityPro = new int[20];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Speciality[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialityPro[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Speciality[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialityPro[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Speciality[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialityPro[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Speciality[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialityPro[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Speciality[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialityPro[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Speciality[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialityPro[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                Speciality[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialityPro[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                Speciality[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialityPro[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                Speciality[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialityPro[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                Speciality[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialityPro[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                Speciality[10] = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialityPro[10] = Table_Tamplet.Convert_Int(temp[__column++]);
                Speciality[11] = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialityPro[11] = Table_Tamplet.Convert_Int(temp[__column++]);
                Speciality[12] = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialityPro[12] = Table_Tamplet.Convert_Int(temp[__column++]);
                Speciality[13] = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialityPro[13] = Table_Tamplet.Convert_Int(temp[__column++]);
                Speciality[14] = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialityPro[14] = Table_Tamplet.Convert_Int(temp[__column++]);
                Speciality[15] = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialityPro[15] = Table_Tamplet.Convert_Int(temp[__column++]);
                Speciality[16] = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialityPro[16] = Table_Tamplet.Convert_Int(temp[__column++]);
                Speciality[17] = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialityPro[17] = Table_Tamplet.Convert_Int(temp[__column++]);
                Speciality[18] = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialityPro[18] = Table_Tamplet.Convert_Int(temp[__column++]);
                Speciality[19] = Table_Tamplet.Convert_Int(temp[__column++]);
                SpecialityPro[19] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class ElfRecord :IRecord
    {
        public static string __TableName = "Elf.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string ElfName { get;        set; }
        public int ElfModel { get;        set; }
        public int ElfType { get;        set; }
        public int[] ElfInitProp = new int[6];
        public int[] ElfProp = new int[6];
        public int[] GrowAddValue = new int[6];
        public int RandomPropCount { get;        set; }
        public int RandomPropPro { get;        set; }
        public int RandomPropValue { get;        set; }
        public int[] BelongGroup = new int[3];
        public int MaxLevel { get;        set; }
        public int[] ResolveCoef = new int[2];
        public List<int> ElfStarUp = new List<int>();
        public int[] StarAttrId = new int[5];
        public int[] StarAttrValue = new int[5];
        public int BuffGroupId { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                ElfName = temp[__column++];
                ElfModel = Table_Tamplet.Convert_Int(temp[__column++]);
                ElfType = Table_Tamplet.Convert_Int(temp[__column++]);
                ElfInitProp[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ElfProp[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                GrowAddValue[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ElfInitProp[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ElfProp[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                GrowAddValue[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ElfInitProp[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ElfProp[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                GrowAddValue[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ElfInitProp[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ElfProp[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                GrowAddValue[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ElfInitProp[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                ElfProp[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                GrowAddValue[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                ElfInitProp[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                ElfProp[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                GrowAddValue[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                RandomPropCount = Table_Tamplet.Convert_Int(temp[__column++]);
                RandomPropPro = Table_Tamplet.Convert_Int(temp[__column++]);
                RandomPropValue = Table_Tamplet.Convert_Int(temp[__column++]);
                BelongGroup[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                BelongGroup[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                BelongGroup[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxLevel = Table_Tamplet.Convert_Int(temp[__column++]);
                ResolveCoef[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ResolveCoef[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(ElfStarUp,temp[__column++]);
                StarAttrId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                StarAttrValue[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                StarAttrId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                StarAttrValue[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                StarAttrId[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                StarAttrValue[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                StarAttrId[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                StarAttrValue[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                StarAttrId[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                StarAttrValue[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffGroupId = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class ElfGroupRecord :IRecord
    {
        public static string __TableName = "ElfGroup.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string GroupName { get;        set; }
        public int[] ElfID = new int[3];
        public int[] GroupPorp = new int[6];
        public int[] PropValue = new int[6];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                GroupName = temp[__column++];
                ElfID[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ElfID[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ElfID[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                GroupPorp[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                GroupPorp[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                GroupPorp[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                GroupPorp[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                GroupPorp[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                GroupPorp[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue[5] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class QueueRecord :IRecord
    {
        public static string __TableName = "Queue.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int CountLimit { get;        set; }
        public int AppType { get;        set; }
        public int Param { get;        set; }
        public int PreId { get;        set; }
        public int WaitTime { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                CountLimit = Table_Tamplet.Convert_Int(temp[__column++]);
                AppType = Table_Tamplet.Convert_Int(temp[__column++]);
                Param = Table_Tamplet.Convert_Int(temp[__column++]);
                PreId = Table_Tamplet.Convert_Int(temp[__column++]);
                WaitTime = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class DrawRecord :IRecord
    {
        public static string __TableName = "Draw.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] DropItem = new int[4];
        public int[] Count = new int[4];
        public int[] Probability = new int[4];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                DropItem[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Probability[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropItem[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Probability[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropItem[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Probability[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropItem[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Probability[3] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class PlantRecord :IRecord
    {
        public static string __TableName = "Plant.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int PlantItemID { get;        set; }
        public int PlantLevel { get;        set; }
        public int CanRemove { get;        set; }
        public int MatureCycle { get;        set; }
        public int HarvestItemID { get;        set; }
        public int[] HarvestCount = new int[2];
        public int RetinueGettingExp { get;        set; }
        public int ExtraRandomDrop { get;        set; }
        public int PlantType { get;        set; }
        public int GetHomeExp { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                PlantItemID = Table_Tamplet.Convert_Int(temp[__column++]);
                PlantLevel = Table_Tamplet.Convert_Int(temp[__column++]);
                CanRemove = Table_Tamplet.Convert_Int(temp[__column++]);
                MatureCycle = Table_Tamplet.Convert_Int(temp[__column++]);
                HarvestItemID = Table_Tamplet.Convert_Int(temp[__column++]);
                HarvestCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                HarvestCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                RetinueGettingExp = Table_Tamplet.Convert_Int(temp[__column++]);
                ExtraRandomDrop = Table_Tamplet.Convert_Int(temp[__column++]);
                PlantType = Table_Tamplet.Convert_Int(temp[__column++]);
                GetHomeExp = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class MedalRecord :IRecord
    {
        public static string __TableName = "Medal.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int CanEquipment { get;        set; }
        public int InitExp { get;        set; }
        public int MedalType { get;        set; }
        public int Quality { get;        set; }
        public int LevelUpExp { get;        set; }
        public int MaxLevel { get;        set; }
        public int[] AddPropID = new int[2];
        public int[] PropValue = new int[2];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                CanEquipment = Table_Tamplet.Convert_Int(temp[__column++]);
                InitExp = Table_Tamplet.Convert_Int(temp[__column++]);
                MedalType = Table_Tamplet.Convert_Int(temp[__column++]);
                Quality = Table_Tamplet.Convert_Int(temp[__column++]);
                LevelUpExp = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxLevel = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropID[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropID[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue[1] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class SailingRecord :IRecord
    {
        public static string __TableName = "Sailing.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int SuccessProb { get;        set; }
        public int SuccessID { get;        set; }
        public int FailedID { get;        set; }
        public int CanCall { get;        set; }
        public int NeedType { get;        set; }
        public int ItemCount { get;        set; }
        public int SuccessDrop { get;        set; }
        public int FailedDrop { get;        set; }
        public int CostType { get;        set; }
        public int CostCount { get;        set; }
        public int ConsumeType { get;        set; }
        public int SuccessGetExp { get;        set; }
        public int FailedGetExp { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                SuccessProb = Table_Tamplet.Convert_Int(temp[__column++]);
                SuccessID = Table_Tamplet.Convert_Int(temp[__column++]);
                FailedID = Table_Tamplet.Convert_Int(temp[__column++]);
                CanCall = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedType = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount = Table_Tamplet.Convert_Int(temp[__column++]);
                SuccessDrop = Table_Tamplet.Convert_Int(temp[__column++]);
                FailedDrop = Table_Tamplet.Convert_Int(temp[__column++]);
                CostType = Table_Tamplet.Convert_Int(temp[__column++]);
                CostCount = Table_Tamplet.Convert_Int(temp[__column++]);
                ConsumeType = Table_Tamplet.Convert_Int(temp[__column++]);
                SuccessGetExp = Table_Tamplet.Convert_Int(temp[__column++]);
                FailedGetExp = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class WingTrainRecord :IRecord
    {
        public static string __TableName = "WingTrain.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int WingPostion { get;        set; }
        public int TrainCount { get;        set; }
        public int TrainStar { get;        set; }
        public int Condition { get;        set; }
        public int MaterialID { get;        set; }
        public int MaterialCount { get;        set; }
        public int UsedMoney { get;        set; }
        public int AddExp { get;        set; }
        public int CommonProb { get;        set; }
        public int[] CritAddExp = new int[3];
        public int[] CritProb = new int[3];
        public int ExpLimit { get;        set; }
        public int[] AddPropID = new int[10];
        public int[] AddPropValue = new int[10];
        public int UpStarID { get;        set; }
        public int PosX { get;        set; }
        public int PoxY { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                WingPostion = Table_Tamplet.Convert_Int(temp[__column++]);
                TrainCount = Table_Tamplet.Convert_Int(temp[__column++]);
                TrainStar = Table_Tamplet.Convert_Int(temp[__column++]);
                Condition = Table_Tamplet.Convert_Int(temp[__column++]);
                MaterialID = Table_Tamplet.Convert_Int(temp[__column++]);
                MaterialCount = Table_Tamplet.Convert_Int(temp[__column++]);
                UsedMoney = Table_Tamplet.Convert_Int(temp[__column++]);
                AddExp = Table_Tamplet.Convert_Int(temp[__column++]);
                CommonProb = Table_Tamplet.Convert_Int(temp[__column++]);
                CritAddExp[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                CritProb[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                CritAddExp[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                CritProb[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                CritAddExp[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                CritProb[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ExpLimit = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropID[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropValue[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropID[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropValue[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropID[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropValue[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropID[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropValue[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropID[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropValue[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropID[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropValue[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropID[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropValue[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropID[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropValue[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropID[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropValue[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropID[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropValue[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                UpStarID = Table_Tamplet.Convert_Int(temp[__column++]);
                PosX = Table_Tamplet.Convert_Int(temp[__column++]);
                PoxY = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class WingQualityRecord :IRecord
    {
        public static string __TableName = "WingQuality.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int Career { get;        set; }
        public int Segment { get;        set; }
        public int MaterialNeed { get;        set; }
        public int MaterialCount { get;        set; }
        public int UsedMoney { get;        set; }
        public int GrowAddValue { get;        set; }
        public int MinValue { get;        set; }
        public int MaxValue { get;        set; }
        public int ValueLimit { get;        set; }
        public int LevelLimit { get;        set; }
        public int[] AddPropID = new int[11];
        public int[] AddPropValue = new int[11];
        public int BreakNeedItem { get;        set; }
        public int BreakNeedCount { get;        set; }
        public int BreakNeedMoney { get;        set; }
        public int GrowProgress { get;        set; }
        public int[] GrowPropID = new int[6];
        public int[] GrowMinProp = new int[6];
        public int[] GrowMaxProp = new int[6];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Career = Table_Tamplet.Convert_Int(temp[__column++]);
                Segment = Table_Tamplet.Convert_Int(temp[__column++]);
                MaterialNeed = Table_Tamplet.Convert_Int(temp[__column++]);
                MaterialCount = Table_Tamplet.Convert_Int(temp[__column++]);
                UsedMoney = Table_Tamplet.Convert_Int(temp[__column++]);
                GrowAddValue = Table_Tamplet.Convert_Int(temp[__column++]);
                MinValue = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxValue = Table_Tamplet.Convert_Int(temp[__column++]);
                ValueLimit = Table_Tamplet.Convert_Int(temp[__column++]);
                LevelLimit = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropID[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropValue[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropID[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropValue[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropID[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropValue[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropID[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropValue[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropID[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropValue[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropID[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropValue[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropID[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropValue[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropID[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropValue[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropID[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropValue[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropID[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropValue[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropID[10] = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropValue[10] = Table_Tamplet.Convert_Int(temp[__column++]);
                BreakNeedItem = Table_Tamplet.Convert_Int(temp[__column++]);
                BreakNeedCount = Table_Tamplet.Convert_Int(temp[__column++]);
                BreakNeedMoney = Table_Tamplet.Convert_Int(temp[__column++]);
                GrowProgress = Table_Tamplet.Convert_Int(temp[__column++]);
                GrowPropID[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                GrowMinProp[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                GrowMaxProp[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                GrowPropID[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                GrowMinProp[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                GrowMaxProp[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                GrowPropID[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                GrowMinProp[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                GrowMaxProp[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                GrowPropID[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                GrowMinProp[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                GrowMaxProp[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                GrowPropID[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                GrowMinProp[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                GrowMaxProp[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                GrowPropID[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                GrowMinProp[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                GrowMaxProp[5] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class PVPRuleRecord :IRecord
    {
        public static string __TableName = "PVPRule.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int CanPK { get;        set; }
        public int ProtectLevel { get;        set; }
        public int IsKillAdd { get;        set; }
        public int KillAddValue { get;        set; }
        public int IntervalTime { get;        set; }
        public int IsPunished { get;        set; }
        public int LostExp { get;        set; }
        public int IsAutoAddEnemy { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                CanPK = Table_Tamplet.Convert_Int(temp[__column++]);
                ProtectLevel = Table_Tamplet.Convert_Int(temp[__column++]);
                IsKillAdd = Table_Tamplet.Convert_Int(temp[__column++]);
                KillAddValue = Table_Tamplet.Convert_Int(temp[__column++]);
                IntervalTime = Table_Tamplet.Convert_Int(temp[__column++]);
                IsPunished = Table_Tamplet.Convert_Int(temp[__column++]);
                LostExp = Table_Tamplet.Convert_Int(temp[__column++]);
                IsAutoAddEnemy = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class ArenaRewardRecord :IRecord
    {
        public static string __TableName = "ArenaReward.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int MaxDiamond { get;        set; }
        public int DayMoney { get;        set; }
        public int DayDiamond { get;        set; }
        public int[] DayItemID = new int[3];
        public int[] DayItemCount = new int[3];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxDiamond = Table_Tamplet.Convert_Int(temp[__column++]);
                DayMoney = Table_Tamplet.Convert_Int(temp[__column++]);
                DayDiamond = Table_Tamplet.Convert_Int(temp[__column++]);
                DayItemID[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                DayItemCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                DayItemID[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                DayItemCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                DayItemID[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                DayItemCount[2] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class ArenaLevelRecord :IRecord
    {
        public static string __TableName = "ArenaLevel.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int SuccessExp { get;        set; }
        public int SuccessMoney { get;        set; }
        public int SuccessItemID { get;        set; }
        public int SuccessCount { get;        set; }
        public int FailedExp { get;        set; }
        public int FailedMoney { get;        set; }
        public int FailedItemID { get;        set; }
        public int FailedCount { get;        set; }
        public int SuccessGetExp { get;        set; }
        public int FailedGetExp { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                SuccessExp = Table_Tamplet.Convert_Int(temp[__column++]);
                SuccessMoney = Table_Tamplet.Convert_Int(temp[__column++]);
                SuccessItemID = Table_Tamplet.Convert_Int(temp[__column++]);
                SuccessCount = Table_Tamplet.Convert_Int(temp[__column++]);
                FailedExp = Table_Tamplet.Convert_Int(temp[__column++]);
                FailedMoney = Table_Tamplet.Convert_Int(temp[__column++]);
                FailedItemID = Table_Tamplet.Convert_Int(temp[__column++]);
                FailedCount = Table_Tamplet.Convert_Int(temp[__column++]);
                SuccessGetExp = Table_Tamplet.Convert_Int(temp[__column++]);
                FailedGetExp = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class HonorRecord :IRecord
    {
        public static string __TableName = "Honor.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int TitleId { get;        set; }
        public int NextRank { get;        set; }
        public int NeedHonor { get;        set; }
        public int[] PropID = new int[10];
        public int[] PropValue = new int[10];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                TitleId = Table_Tamplet.Convert_Int(temp[__column++]);
                NextRank = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedHonor = Table_Tamplet.Convert_Int(temp[__column++]);
                PropID[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropID[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropID[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropID[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropID[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropID[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropID[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropID[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropID[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropID[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue[9] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class JJCRootRecord :IRecord
    {
        public static string __TableName = "JJCRoot.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string Name { get;        set; }
        public int Level { get;        set; }
        public int Career { get;        set; }
        public int CombatValue { get;        set; }
        public int EquipHand { get;        set; }
        public int EquipHead { get;        set; }
        public int EquipChest { get;        set; }
        public int EquipGlove { get;        set; }
        public int EquipTrouser { get;        set; }
        public int EquipShoes { get;        set; }
        public int EquipRange { get;        set; }
        public int EquipNecklace { get;        set; }
        public int EquipLevel { get;        set; }
        public int WingID { get;        set; }
        public int[] Skill = new int[4];
        public int Power { get;        set; }
        public int Agility { get;        set; }
        public int Intelligence { get;        set; }
        public int physical { get;        set; }
        public int AttackMin { get;        set; }
        public int AttackMax { get;        set; }
        public int LifeLimit { get;        set; }
        public int MagicLimit { get;        set; }
        public int PhysicsDefense { get;        set; }
        public int MagicDefense { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Name = temp[__column++];
                Level = Table_Tamplet.Convert_Int(temp[__column++]);
                Career = Table_Tamplet.Convert_Int(temp[__column++]);
                CombatValue = Table_Tamplet.Convert_Int(temp[__column++]);
                EquipHand = Table_Tamplet.Convert_Int(temp[__column++]);
                EquipHead = Table_Tamplet.Convert_Int(temp[__column++]);
                EquipChest = Table_Tamplet.Convert_Int(temp[__column++]);
                EquipGlove = Table_Tamplet.Convert_Int(temp[__column++]);
                EquipTrouser = Table_Tamplet.Convert_Int(temp[__column++]);
                EquipShoes = Table_Tamplet.Convert_Int(temp[__column++]);
                EquipRange = Table_Tamplet.Convert_Int(temp[__column++]);
                EquipNecklace = Table_Tamplet.Convert_Int(temp[__column++]);
                EquipLevel = Table_Tamplet.Convert_Int(temp[__column++]);
                WingID = Table_Tamplet.Convert_Int(temp[__column++]);
                Skill[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Skill[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Skill[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Skill[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Power = Table_Tamplet.Convert_Int(temp[__column++]);
                Agility = Table_Tamplet.Convert_Int(temp[__column++]);
                Intelligence = Table_Tamplet.Convert_Int(temp[__column++]);
                physical = Table_Tamplet.Convert_Int(temp[__column++]);
                AttackMin = Table_Tamplet.Convert_Int(temp[__column++]);
                AttackMax = Table_Tamplet.Convert_Int(temp[__column++]);
                LifeLimit = Table_Tamplet.Convert_Int(temp[__column++]);
                MagicLimit = Table_Tamplet.Convert_Int(temp[__column++]);
                PhysicsDefense = Table_Tamplet.Convert_Int(temp[__column++]);
                MagicDefense = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class StatueRecord :IRecord
    {
        public static string __TableName = "Statue.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string Name { get;        set; }
        public int Level { get;        set; }
        public int MaxLevel { get;        set; }
        public int NextLevelID { get;        set; }
        public int Type { get;        set; }
        public int LevelUpExp { get;        set; }
        public int[] PropID = new int[3];
        public int[] propValue = new int[3];
        public int[] FuseID = new int[3];
        public int[] FuseValue = new int[3];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Name = temp[__column++];
                Level = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxLevel = Table_Tamplet.Convert_Int(temp[__column++]);
                NextLevelID = Table_Tamplet.Convert_Int(temp[__column++]);
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                LevelUpExp = Table_Tamplet.Convert_Int(temp[__column++]);
                PropID[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                propValue[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropID[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                propValue[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropID[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                propValue[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                FuseID[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                FuseValue[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                FuseID[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                FuseValue[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                FuseID[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                FuseValue[2] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class EquipAdditional1Record :IRecord
    {
        public static string __TableName = "EquipAdditional1.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int AddPropArea { get;        set; }
        public int MaterialID { get;        set; }
        public int MaterialCount { get;        set; }
        public int Money { get;        set; }
        public int SmritiMoney { get;        set; }
        public int SmritiDiamond { get;        set; }
        public int CallBackItem { get;        set; }
        public int CallBackCount { get;        set; }
        public int MinSection { get;        set; }
        public int MaxSection { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                AddPropArea = Table_Tamplet.Convert_Int(temp[__column++]);
                MaterialID = Table_Tamplet.Convert_Int(temp[__column++]);
                MaterialCount = Table_Tamplet.Convert_Int(temp[__column++]);
                Money = Table_Tamplet.Convert_Int(temp[__column++]);
                SmritiMoney = Table_Tamplet.Convert_Int(temp[__column++]);
                SmritiDiamond = Table_Tamplet.Convert_Int(temp[__column++]);
                CallBackItem = Table_Tamplet.Convert_Int(temp[__column++]);
                CallBackCount = Table_Tamplet.Convert_Int(temp[__column++]);
                MinSection = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxSection = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class GuildRecord :IRecord
    {
        public static string __TableName = "Guild.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int MaxCount { get;        set; }
        public int MaintainMoney { get;        set; }
        public int IsJoinCityWar { get;        set; }
        public int StoreParam { get;        set; }
        public int moneyCountLimit { get;        set; }
        public int LessGetGongji { get;        set; }
        public int LessNeedCount { get;        set; }
        public int LessUnionMoney { get;        set; }
        public int LessUnionDonation { get;        set; }
        public int MoreGetGongji { get;        set; }
        public int MoreNeedCount { get;        set; }
        public int MoreUnionMoney { get;        set; }
        public int MoreUnionDonation { get;        set; }
        public int DiamondGetGongji { get;        set; }
        public int DiaNeedCount { get;        set; }
        public int DiaUnionMoney { get;        set; }
        public int DiaUnionDonation { get;        set; }
        public int TaskRefresh { get;        set; }
        public int ConsumeUnionMoney { get;        set; }
        public int DepotCapacity { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount = Table_Tamplet.Convert_Int(temp[__column++]);
                MaintainMoney = Table_Tamplet.Convert_Int(temp[__column++]);
                IsJoinCityWar = Table_Tamplet.Convert_Int(temp[__column++]);
                StoreParam = Table_Tamplet.Convert_Int(temp[__column++]);
                moneyCountLimit = Table_Tamplet.Convert_Int(temp[__column++]);
                LessGetGongji = Table_Tamplet.Convert_Int(temp[__column++]);
                LessNeedCount = Table_Tamplet.Convert_Int(temp[__column++]);
                LessUnionMoney = Table_Tamplet.Convert_Int(temp[__column++]);
                LessUnionDonation = Table_Tamplet.Convert_Int(temp[__column++]);
                MoreGetGongji = Table_Tamplet.Convert_Int(temp[__column++]);
                MoreNeedCount = Table_Tamplet.Convert_Int(temp[__column++]);
                MoreUnionMoney = Table_Tamplet.Convert_Int(temp[__column++]);
                MoreUnionDonation = Table_Tamplet.Convert_Int(temp[__column++]);
                DiamondGetGongji = Table_Tamplet.Convert_Int(temp[__column++]);
                DiaNeedCount = Table_Tamplet.Convert_Int(temp[__column++]);
                DiaUnionMoney = Table_Tamplet.Convert_Int(temp[__column++]);
                DiaUnionDonation = Table_Tamplet.Convert_Int(temp[__column++]);
                TaskRefresh = Table_Tamplet.Convert_Int(temp[__column++]);
                ConsumeUnionMoney = Table_Tamplet.Convert_Int(temp[__column++]);
                DepotCapacity = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class GuildBuffRecord :IRecord
    {
        public static string __TableName = "GuildBuff.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int BuffLevel { get;        set; }
        public int NextLevel { get;        set; }
        public int NeedUnionLevel { get;        set; }
        public int BuffID { get;        set; }
        public int LevelLimit { get;        set; }
        public int UpConsumeGongji { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffLevel = Table_Tamplet.Convert_Int(temp[__column++]);
                NextLevel = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedUnionLevel = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffID = Table_Tamplet.Convert_Int(temp[__column++]);
                LevelLimit = Table_Tamplet.Convert_Int(temp[__column++]);
                UpConsumeGongji = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class GuildBossRecord :IRecord
    {
        public static string __TableName = "GuildBoss.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int MonsterID { get;        set; }
        public int RewardMone { get;        set; }
        public int KillMoney { get;        set; }
        public int KillZhangong { get;        set; }
        public int CycleZhangong { get;        set; }
        public int ActiveBossID { get;        set; }
        public int ChallengeCount { get;        set; }
        public int ChallengeTime { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                MonsterID = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardMone = Table_Tamplet.Convert_Int(temp[__column++]);
                KillMoney = Table_Tamplet.Convert_Int(temp[__column++]);
                KillZhangong = Table_Tamplet.Convert_Int(temp[__column++]);
                CycleZhangong = Table_Tamplet.Convert_Int(temp[__column++]);
                ActiveBossID = Table_Tamplet.Convert_Int(temp[__column++]);
                ChallengeCount = Table_Tamplet.Convert_Int(temp[__column++]);
                ChallengeTime = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class GuildAccessRecord :IRecord
    {
        public static string __TableName = "GuildAccess.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int MaxCount { get;        set; }
        public int CanAddMember { get;        set; }
        public int CanLevelBuff { get;        set; }
        public int CanOperation { get;        set; }
        public int CanModifyNotice { get;        set; }
        public int CanModifyAttackCity { get;        set; }
        public int CanRebornGuard { get;        set; }
        public int MailId { get;        set; }
        public int LodeResRatio { get;        set; }
        public int LodeMailId { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount = Table_Tamplet.Convert_Int(temp[__column++]);
                CanAddMember = Table_Tamplet.Convert_Int(temp[__column++]);
                CanLevelBuff = Table_Tamplet.Convert_Int(temp[__column++]);
                CanOperation = Table_Tamplet.Convert_Int(temp[__column++]);
                CanModifyNotice = Table_Tamplet.Convert_Int(temp[__column++]);
                CanModifyAttackCity = Table_Tamplet.Convert_Int(temp[__column++]);
                CanRebornGuard = Table_Tamplet.Convert_Int(temp[__column++]);
                MailId = Table_Tamplet.Convert_Int(temp[__column++]);
                LodeResRatio = Table_Tamplet.Convert_Int(temp[__column++]);
                LodeMailId = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class ExpInfoRecord :IRecord
    {
        public static string __TableName = "ExpInfo.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int LevelDiff { get;        set; }
        public int ExpZoom { get;        set; }
        public int CountExpProp { get;        set; }
        public int TeamCountExpProp { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                LevelDiff = Table_Tamplet.Convert_Int(temp[__column++]);
                ExpZoom = Table_Tamplet.Convert_Int(temp[__column++]);
                CountExpProp = Table_Tamplet.Convert_Int(temp[__column++]);
                TeamCountExpProp = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class GroupShopRecord :IRecord
    {
        public static string __TableName = "GroupShop.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int ItemID { get;        set; }
        public int MotherID { get;        set; }
        public int LuckNumber { get;        set; }
        public int SaleType { get;        set; }
        public int SaleCount { get;        set; }
        public int BuyLimit { get;        set; }
        public int ExistTime { get;        set; }
        public int RefleshCount { get;        set; }
        public int SuccessGetExp { get;        set; }
        public int LimitMinCount { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemID = Table_Tamplet.Convert_Int(temp[__column++]);
                MotherID = Table_Tamplet.Convert_Int(temp[__column++]);
                LuckNumber = Table_Tamplet.Convert_Int(temp[__column++]);
                SaleType = Table_Tamplet.Convert_Int(temp[__column++]);
                SaleCount = Table_Tamplet.Convert_Int(temp[__column++]);
                BuyLimit = Table_Tamplet.Convert_Int(temp[__column++]);
                ExistTime = Table_Tamplet.Convert_Int(temp[__column++]);
                RefleshCount = Table_Tamplet.Convert_Int(temp[__column++]);
                SuccessGetExp = Table_Tamplet.Convert_Int(temp[__column++]);
                LimitMinCount = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class GroupShopUpdateRecord :IRecord
    {
        public static string __TableName = "GroupShopUpdate.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int ListID { get;        set; }
        public int[] Goods = new int[10];
        public int[] Count = new int[10];
        public int[] GroupID = new int[10];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                ListID = Table_Tamplet.Convert_Int(temp[__column++]);
                Goods[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                GroupID[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Goods[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                GroupID[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Goods[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                GroupID[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Goods[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                GroupID[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Goods[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                GroupID[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Goods[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                GroupID[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                Goods[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                GroupID[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                Goods[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                GroupID[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                Goods[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                GroupID[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                Goods[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                Count[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                GroupID[9] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class PKModeRecord :IRecord
    {
        public static string __TableName = "PKMode.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string Mode { get;        set; }
        public int NomalTeam { get;        set; }
        public int NomalUnion { get;        set; }
        public int NomalState { get;        set; }
        public int RedTeam { get;        set; }
        public int RedUnion { get;        set; }
        public int RedState { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Mode = temp[__column++];
                NomalTeam = Table_Tamplet.Convert_Int(temp[__column++]);
                NomalUnion = Table_Tamplet.Convert_Int(temp[__column++]);
                NomalState = Table_Tamplet.Convert_Int(temp[__column++]);
                RedTeam = Table_Tamplet.Convert_Int(temp[__column++]);
                RedUnion = Table_Tamplet.Convert_Int(temp[__column++]);
                RedState = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class forgedRecord :IRecord
    {
        public static string __TableName = "forged.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int Type { get;        set; }
        public int NeedLevel { get;        set; }
        public int NeedTime { get;        set; }
        public int ProductID { get;        set; }
        public int[] NeedItemID = new int[5];
        public int[] NeedItemCount = new int[5];
        public int[] NeedResID = new int[3];
        public int[] NeedResCount = new int[3];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedLevel = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedTime = Table_Tamplet.Convert_Int(temp[__column++]);
                ProductID = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemID[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemID[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemID[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemCount[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemID[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemCount[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemID[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemCount[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedResID[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedResCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedResID[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedResCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedResID[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedResCount[2] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class EquipUpdateRecord :IRecord
    {
        public static string __TableName = "EquipUpdate.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] NeedItemID = new int[4];
        public int[] NeedItemCount = new int[4];
        public int[] NeedResID = new int[3];
        public int[] NeedResCount = new int[3];
        public int NeedEquipCount { get;        set; }
        public int SuccessGetExp { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemID[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemID[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemID[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemCount[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemID[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemCount[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedResID[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedResCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedResID[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedResCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedResID[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedResCount[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedEquipCount = Table_Tamplet.Convert_Int(temp[__column++]);
                SuccessGetExp = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class GuildMissionRecord :IRecord
    {
        public static string __TableName = "GuildMission.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int ItemID { get;        set; }
        public int MinCount { get;        set; }
        public int MaxCount { get;        set; }
        public int MinLevel { get;        set; }
        public int MaxLevel { get;        set; }
        public int GetGongJi { get;        set; }
        public int GetDonation { get;        set; }
        public int GetMoney { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemID = Table_Tamplet.Convert_Int(temp[__column++]);
                MinCount = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount = Table_Tamplet.Convert_Int(temp[__column++]);
                MinLevel = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxLevel = Table_Tamplet.Convert_Int(temp[__column++]);
                GetGongJi = Table_Tamplet.Convert_Int(temp[__column++]);
                GetDonation = Table_Tamplet.Convert_Int(temp[__column++]);
                GetMoney = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class OrderFormRecord :IRecord
    {
        public static string __TableName = "OrderForm.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int Item { get;        set; }
        public int RewardLess100 { get;        set; }
        public int RewardMore100 { get;        set; }
        public int ExtraRewardValue { get;        set; }
        public int ExtraRewardProb { get;        set; }
        public int ExtraDropID { get;        set; }
        public float MoneyValue { get;        set; }
        public float ExpValue { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Item = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardLess100 = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardMore100 = Table_Tamplet.Convert_Int(temp[__column++]);
                ExtraRewardValue = Table_Tamplet.Convert_Int(temp[__column++]);
                ExtraRewardProb = Table_Tamplet.Convert_Int(temp[__column++]);
                ExtraDropID = Table_Tamplet.Convert_Int(temp[__column++]);
                MoneyValue = Table_Tamplet.Convert_Float(temp[__column++]);
                ExpValue = Table_Tamplet.Convert_Float(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class OrderUpdateRecord :IRecord
    {
        public static string __TableName = "OrderUpdate.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int TotalValue { get;        set; }
        public int RefleshTaskCount { get;        set; }
        public int CDRefleshCount { get;        set; }
        public int RefleshCD { get;        set; }
        public int[] OrderID = new int[25];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                TotalValue = Table_Tamplet.Convert_Int(temp[__column++]);
                RefleshTaskCount = Table_Tamplet.Convert_Int(temp[__column++]);
                CDRefleshCount = Table_Tamplet.Convert_Int(temp[__column++]);
                RefleshCD = Table_Tamplet.Convert_Int(temp[__column++]);
                OrderID[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                OrderID[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                OrderID[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                OrderID[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                OrderID[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                OrderID[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                OrderID[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                OrderID[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                OrderID[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                OrderID[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                OrderID[10] = Table_Tamplet.Convert_Int(temp[__column++]);
                OrderID[11] = Table_Tamplet.Convert_Int(temp[__column++]);
                OrderID[12] = Table_Tamplet.Convert_Int(temp[__column++]);
                OrderID[13] = Table_Tamplet.Convert_Int(temp[__column++]);
                OrderID[14] = Table_Tamplet.Convert_Int(temp[__column++]);
                OrderID[15] = Table_Tamplet.Convert_Int(temp[__column++]);
                OrderID[16] = Table_Tamplet.Convert_Int(temp[__column++]);
                OrderID[17] = Table_Tamplet.Convert_Int(temp[__column++]);
                OrderID[18] = Table_Tamplet.Convert_Int(temp[__column++]);
                OrderID[19] = Table_Tamplet.Convert_Int(temp[__column++]);
                OrderID[20] = Table_Tamplet.Convert_Int(temp[__column++]);
                OrderID[21] = Table_Tamplet.Convert_Int(temp[__column++]);
                OrderID[22] = Table_Tamplet.Convert_Int(temp[__column++]);
                OrderID[23] = Table_Tamplet.Convert_Int(temp[__column++]);
                OrderID[24] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class TradeRecord :IRecord
    {
        public static string __TableName = "Trade.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int ItemID { get;        set; }
        public int Count { get;        set; }
        public int MoneyType { get;        set; }
        public int Price { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemID = Table_Tamplet.Convert_Int(temp[__column++]);
                Count = Table_Tamplet.Convert_Int(temp[__column++]);
                MoneyType = Table_Tamplet.Convert_Int(temp[__column++]);
                Price = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class GemRecord :IRecord
    {
        public static string __TableName = "Gem.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int Type { get;        set; }
        public int Combination { get;        set; }
        public int Quality { get;        set; }
        public int InitExp { get;        set; }
        public int MaxLevel { get;        set; }
        public int[] ActiveCondition = new int[6];
        public int[] Param = new int[6];
        public int[] Prop1 = new int[6];
        public int[] PropValue1 = new int[6];
        public int[] Prop2 = new int[6];
        public int[] PropValue2 = new int[6];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                Combination = Table_Tamplet.Convert_Int(temp[__column++]);
                Quality = Table_Tamplet.Convert_Int(temp[__column++]);
                InitExp = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxLevel = Table_Tamplet.Convert_Int(temp[__column++]);
                ActiveCondition[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Prop1[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue1[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Prop2[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue2[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ActiveCondition[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Prop1[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue1[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Prop2[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue2[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ActiveCondition[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Prop1[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue1[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Prop2[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue2[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ActiveCondition[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Prop1[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue1[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Prop2[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue2[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ActiveCondition[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Prop1[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue1[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Prop2[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue2[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                ActiveCondition[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                Prop1[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue1[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                Prop2[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue2[5] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class GemGroupRecord :IRecord
    {
        public static string __TableName = "GemGroup.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int DiaID { get;        set; }
        public int CrystalID { get;        set; }
        public int AgateID { get;        set; }
        public int[] Towprop = new int[2];
        public int[] TowValue = new int[2];
        public int[] Threeprop = new int[2];
        public int[] ThreeValue = new int[2];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                DiaID = Table_Tamplet.Convert_Int(temp[__column++]);
                CrystalID = Table_Tamplet.Convert_Int(temp[__column++]);
                AgateID = Table_Tamplet.Convert_Int(temp[__column++]);
                Towprop[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                TowValue[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Towprop[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                TowValue[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Threeprop[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ThreeValue[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Threeprop[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ThreeValue[1] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class SensitiveWordRecord :IRecord
    {
        public static string __TableName = "SensitiveWord.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string Name { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Name = temp[__column++];
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class GuidanceRecord :IRecord
    {
        public static string __TableName = "Guidance.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string Desc { get;        set; }
        public string Name { get;        set; }
        public int TaskID { get;        set; }
        public int State { get;        set; }
        public int FlagPrepose { get;        set; }
        public int FlagPreposeFalse { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Desc = temp[__column++];
                Name = temp[__column++];
                TaskID = Table_Tamplet.Convert_Int(temp[__column++]);
                State = Table_Tamplet.Convert_Int(temp[__column++]);
                FlagPrepose = Table_Tamplet.Convert_Int(temp[__column++]);
                FlagPreposeFalse = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class MapTransferRecord :IRecord
    {
        public static string __TableName = "MapTransfer.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int SceneID { get;        set; }
        public int NpcID { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                SceneID = Table_Tamplet.Convert_Int(temp[__column++]);
                NpcID = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class RandomCoordinateRecord :IRecord
    {
        public static string __TableName = "RandomCoordinate.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public float PosX { get;        set; }
        public float PosY { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                PosX = Table_Tamplet.Convert_Float(temp[__column++]);
                PosY = Table_Tamplet.Convert_Float(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class StepByStepRecord :IRecord
    {
        public static string __TableName = "StepByStep.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int PosMark { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                PosMark = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class WorldBOSSRecord :IRecord
    {
        public static string __TableName = "WorldBOSS.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int Type { get;        set; }
        public int SceneNpc { get;        set; }
        public string RefleshTime { get;        set; }
        public int RefleshRole { get;        set; }
        public int MaxCount { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                SceneNpc = Table_Tamplet.Convert_Int(temp[__column++]);
                RefleshTime = temp[__column++];
                RefleshRole = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class WorldBOSSAwardRecord :IRecord
    {
        public static string __TableName = "WorldBOSSAward.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int MinRanking { get;        set; }
        public int MaxRanking { get;        set; }
        public int PostIndex { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                MinRanking = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxRanking = Table_Tamplet.Convert_Int(temp[__column++]);
                PostIndex = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class PKValueRecord :IRecord
    {
        public static string __TableName = "PKValue.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int BuffId { get;        set; }
        public int IsDeadDouble { get;        set; }
        public int IsKilledAddValue { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffId = Table_Tamplet.Convert_Int(temp[__column++]);
                IsDeadDouble = Table_Tamplet.Convert_Int(temp[__column++]);
                IsKilledAddValue = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class TransmigrationRecord :IRecord
    {
        public static string __TableName = "Transmigration.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int ConditionCount { get;        set; }
        public int TransLevel { get;        set; }
        public int PropPoint { get;        set; }
        public int NeedMoney { get;        set; }
        public int NeedDust { get;        set; }
        public int AttackAdd { get;        set; }
        public int PhyDefAdd { get;        set; }
        public int MagicDefAdd { get;        set; }
        public int HitAdd { get;        set; }
        public int DodgeAdd { get;        set; }
        public int LifeAdd { get;        set; }
        public List<int> zsRebornSkill = new List<int>();
        public List<int> fsRebornSkill = new List<int>();
        public List<int> gsRebornSkill = new List<int>();
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                ConditionCount = Table_Tamplet.Convert_Int(temp[__column++]);
                TransLevel = Table_Tamplet.Convert_Int(temp[__column++]);
                PropPoint = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedMoney = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedDust = Table_Tamplet.Convert_Int(temp[__column++]);
                AttackAdd = Table_Tamplet.Convert_Int(temp[__column++]);
                PhyDefAdd = Table_Tamplet.Convert_Int(temp[__column++]);
                MagicDefAdd = Table_Tamplet.Convert_Int(temp[__column++]);
                HitAdd = Table_Tamplet.Convert_Int(temp[__column++]);
                DodgeAdd = Table_Tamplet.Convert_Int(temp[__column++]);
                LifeAdd = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(zsRebornSkill,temp[__column++]);
                Table_Tamplet.Convert_Value(fsRebornSkill,temp[__column++]);
                Table_Tamplet.Convert_Value(gsRebornSkill,temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class FubenInfoRecord :IRecord
    {
        public static string __TableName = "FubenInfo.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int Type { get;        set; }
        public int FubenParam { get;        set; }
        public int[] Param = new int[6];
        public int Stage { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                FubenParam = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                Stage = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class FubenLogicRecord :IRecord
    {
        public static string __TableName = "FubenLogic.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] EnterStage = new int[4];
        public int[] EnterParam1 = new int[4];
        public int[] EnterParam2 = new int[4];
        public int[] SwitchState = new int[4];
        public int[] SwitchParam1 = new int[4];
        public int[] SwitchParam2 = new int[4];
        public int[] SwitchInfoPa = new int[4];
        public int EnterStateID { get;        set; }
        public int IsClearCount { get;        set; }
        public int[] FubenInfo = new int[4];
        public int[] FubenParam1 = new int[4];
        public int[] FubenParam2 = new int[4];
        public int DelayToNextState { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                EnterStage[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                EnterParam1[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                EnterParam2[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                EnterStage[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                EnterParam1[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                EnterParam2[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                EnterStage[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                EnterParam1[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                EnterParam2[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                EnterStage[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                EnterParam1[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                EnterParam2[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                SwitchState[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                SwitchParam1[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                SwitchParam2[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                SwitchInfoPa[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                SwitchState[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                SwitchParam1[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                SwitchParam2[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                SwitchInfoPa[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                SwitchState[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                SwitchParam1[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                SwitchParam2[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                SwitchInfoPa[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                SwitchState[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                SwitchParam1[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                SwitchParam2[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                SwitchInfoPa[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                EnterStateID = Table_Tamplet.Convert_Int(temp[__column++]);
                IsClearCount = Table_Tamplet.Convert_Int(temp[__column++]);
                FubenInfo[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                FubenParam1[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                FubenParam2[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                FubenInfo[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                FubenParam1[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                FubenParam2[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                FubenInfo[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                FubenParam1[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                FubenParam2[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                FubenInfo[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                FubenParam1[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                FubenParam2[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                DelayToNextState = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class ServerNameRecord :IRecord
    {
        public static string __TableName = "ServerName.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string Name { get;        set; }
        public int LogicID { get;        set; }
        public int IsClientDisplay { get;        set; }
        public string Channels { get;        set; }
        public int CrowdCount { get;        set; }
        public int FullCount { get;        set; }
        public string OpenTime { get;        set; }
        public int MaxLiveCount { get;        set; }
        public int AuctionId { get;        set; }
        public int Weights { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Name = temp[__column++];
                LogicID = Table_Tamplet.Convert_Int(temp[__column++]);
                IsClientDisplay = Table_Tamplet.Convert_Int(temp[__column++]);
                Channels = temp[__column++];
                CrowdCount = Table_Tamplet.Convert_Int(temp[__column++]);
                FullCount = Table_Tamplet.Convert_Int(temp[__column++]);
                OpenTime = temp[__column++];
                MaxLiveCount = Table_Tamplet.Convert_Int(temp[__column++]);
                AuctionId = Table_Tamplet.Convert_Int(temp[__column++]);
                Weights = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class GetMissionLevelRecord :IRecord
    {
        public static string __TableName = "GetMissionLevel.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] LevelProb = new int[10];
        public int LevelUpExp { get;        set; }
        public int RefleshCount { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                LevelProb[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                LevelProb[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                LevelProb[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                LevelProb[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                LevelProb[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                LevelProb[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                LevelProb[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                LevelProb[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                LevelProb[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                LevelProb[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                LevelUpExp = Table_Tamplet.Convert_Int(temp[__column++]);
                RefleshCount = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class GetMissionTypeRecord :IRecord
    {
        public static string __TableName = "GetMissionType.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] TypeProb = new int[7];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                TypeProb[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                TypeProb[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                TypeProb[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                TypeProb[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                TypeProb[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                TypeProb[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                TypeProb[6] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class GetMissionQulityRecord :IRecord
    {
        public static string __TableName = "GetMissionQulity.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] DiffProb = new int[3];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                DiffProb[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                DiffProb[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                DiffProb[2] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class GetPetCountRecord :IRecord
    {
        public static string __TableName = "GetPetCount.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] PersonProb = new int[3];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                PersonProb[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                PersonProb[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                PersonProb[2] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class GetMissionInfoRecord :IRecord
    {
        public static string __TableName = "GetMissionInfo.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] FightNeed = new int[3];
        public int[] StarNeed = new int[3];
        public int[] TaskTime = new int[3];
        public int[] HomeExp = new int[3];
        public int PetGetExp { get;        set; }
        public int RandomShowCount { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                FightNeed[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                FightNeed[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                FightNeed[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                StarNeed[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                StarNeed[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                StarNeed[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                TaskTime[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                TaskTime[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                TaskTime[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                HomeExp[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                HomeExp[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                HomeExp[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                PetGetExp = Table_Tamplet.Convert_Int(temp[__column++]);
                RandomShowCount = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class GetMissionPlaceRecord :IRecord
    {
        public static string __TableName = "GetMissionPlace.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] LandProb = new int[8];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                LandProb[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                LandProb[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                LandProb[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                LandProb[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                LandProb[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                LandProb[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                LandProb[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                LandProb[7] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class GetMissionWeatherRecord :IRecord
    {
        public static string __TableName = "GetMissionWeather.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] WeatherProb = new int[6];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                WeatherProb[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                WeatherProb[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                WeatherProb[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                WeatherProb[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                WeatherProb[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                WeatherProb[5] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class GetMissionTimeLevelRecord :IRecord
    {
        public static string __TableName = "GetMissionTimeLevel.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] TimeTask = new int[4];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                TimeTask[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                TimeTask[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                TimeTask[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                TimeTask[3] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class MissionConditionInfoRecord :IRecord
    {
        public static string __TableName = "MissionConditionInfo.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int Param { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Param = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class GetMissionRewardRecord :IRecord
    {
        public static string __TableName = "GetMissionReward.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int RewardType { get;        set; }
        public int RewardCount { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardType = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardCount = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class GetPetTypeRecord :IRecord
    {
        public static string __TableName = "GetPetType.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] MonsterProb = new int[6];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                MonsterProb[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                MonsterProb[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                MonsterProb[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                MonsterProb[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                MonsterProb[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                MonsterProb[5] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class SubjectRecord :IRecord
    {
        public static string __TableName = "Subject.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string Title { get;        set; }
        public string RightKey { get;        set; }
        public string[] Wrong = new string[4];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Title = temp[__column++];
                RightKey = temp[__column++];
                Wrong[0]  = temp[__column++];
                Wrong[1]  = temp[__column++];
                Wrong[2]  = temp[__column++];
                Wrong[3]  = temp[__column++];
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class GetMissionNameRecord :IRecord
    {
        public static string __TableName = "GetMissionName.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int RandomNameCount { get;        set; }
        public string[] Name = new string[10];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                RandomNameCount = Table_Tamplet.Convert_Int(temp[__column++]);
                Name[0]  = temp[__column++];
                Name[1]  = temp[__column++];
                Name[2]  = temp[__column++];
                Name[3]  = temp[__column++];
                Name[4]  = temp[__column++];
                Name[5]  = temp[__column++];
                Name[6]  = temp[__column++];
                Name[7]  = temp[__column++];
                Name[8]  = temp[__column++];
                Name[9]  = temp[__column++];
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class DynamicActivityRecord :IRecord
    {
        public static string __TableName = "DynamicActivity.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] FuBenID = new int[7];
        public List<int> WeekLoop = new List<int>();
        public int IsOpenTeam { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                FuBenID[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                FuBenID[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                FuBenID[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                FuBenID[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                FuBenID[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                FuBenID[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                FuBenID[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(WeekLoop,temp[__column++]);
                IsOpenTeam = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class CompensationRecord :IRecord
    {
        public static string __TableName = "Compensation.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string Name { get;        set; }
        public int Type { get;        set; }
        public int Sign { get;        set; }
        public int ExtraData { get;        set; }
        public int MaxCount { get;        set; }
        public int ExpType { get;        set; }
        public int UnitExp { get;        set; }
        public int GoldType { get;        set; }
        public int UnitGold { get;        set; }
        public int[] ItemYype = new int[4];
        public int[] UnitItem = new int[4];
        public int[] ItemCount = new int[4];
        public int ConditionId { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Name = temp[__column++];
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                Sign = Table_Tamplet.Convert_Int(temp[__column++]);
                ExtraData = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount = Table_Tamplet.Convert_Int(temp[__column++]);
                ExpType = Table_Tamplet.Convert_Int(temp[__column++]);
                UnitExp = Table_Tamplet.Convert_Int(temp[__column++]);
                GoldType = Table_Tamplet.Convert_Int(temp[__column++]);
                UnitGold = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemYype[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                UnitItem[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemYype[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                UnitItem[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemYype[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                UnitItem[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemYype[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                UnitItem[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ConditionId = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class CityTalkRecord :IRecord
    {
        public static string __TableName = "CityTalk.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string Name { get;        set; }
        public int IsParent { get;        set; }
        public int Param { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Name = temp[__column++];
                IsParent = Table_Tamplet.Convert_Int(temp[__column++]);
                Param = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class DailyActivityRecord :IRecord
    {
        public static string __TableName = "DailyActivity.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int Type { get;        set; }
        public int DetailType { get;        set; }
        public int OpenCondition { get;        set; }
        public int WillOpenCondition { get;        set; }
        public int ActivityValue { get;        set; }
        public int ActivityCount { get;        set; }
        public int FinishCanJoin { get;        set; }
        public int[] CommonParam = new int[2];
        public int ExDataId { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                DetailType = Table_Tamplet.Convert_Int(temp[__column++]);
                OpenCondition = Table_Tamplet.Convert_Int(temp[__column++]);
                WillOpenCondition = Table_Tamplet.Convert_Int(temp[__column++]);
                ActivityValue = Table_Tamplet.Convert_Int(temp[__column++]);
                ActivityCount = Table_Tamplet.Convert_Int(temp[__column++]);
                FinishCanJoin = Table_Tamplet.Convert_Int(temp[__column++]);
                CommonParam[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                CommonParam[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ExDataId = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class RechargeRecord :IRecord
    {
        public static string __TableName = "Recharge.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string Platfrom { get;        set; }
        public string GoodsId { get;        set; }
        public string Name { get;        set; }
        public int ItemId { get;        set; }
        public int Visible { get;        set; }
        public int Type { get;        set; }
        public string ExDesc { get;        set; }
        public string Desc { get;        set; }
        public int Price { get;        set; }
        public int Diamond { get;        set; }
        public int VipExp { get;        set; }
        public int ExTimes { get;        set; }
        public int ExdataId { get;        set; }
        public int ExDiamond { get;        set; }
        public int NormalDiamond { get;        set; }
        public int[] Param = new int[6];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Platfrom = temp[__column++];
                GoodsId = temp[__column++];
                Name = temp[__column++];
                ItemId = Table_Tamplet.Convert_Int(temp[__column++]);
                Visible = Table_Tamplet.Convert_Int(temp[__column++]);
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                ExDesc = temp[__column++];
                Desc = temp[__column++];
                Price = Table_Tamplet.Convert_Int(temp[__column++]);
                Diamond = Table_Tamplet.Convert_Int(temp[__column++]);
                VipExp = Table_Tamplet.Convert_Int(temp[__column++]);
                ExTimes = Table_Tamplet.Convert_Int(temp[__column++]);
                ExdataId = Table_Tamplet.Convert_Int(temp[__column++]);
                ExDiamond = Table_Tamplet.Convert_Int(temp[__column++]);
                NormalDiamond = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[5] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class NameTitleRecord :IRecord
    {
        public static string __TableName = "NameTitle.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string Name { get;        set; }
        public int Pos { get;        set; }
        public int PeriodEndSort { get;        set; }
        public int PropAdd { get;        set; }
        public int[] PropId = new int[9];
        public int[] PropValue = new int[9];
        public int ValidityPeriod { get;        set; }
        public int ConditionID { get;        set; }
        public int FlagId { get;        set; }
        public int FrontId { get;        set; }
        public int PostId { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Name = temp[__column++];
                Pos = Table_Tamplet.Convert_Int(temp[__column++]);
                PeriodEndSort = Table_Tamplet.Convert_Int(temp[__column++]);
                PropAdd = Table_Tamplet.Convert_Int(temp[__column++]);
                PropId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropId[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropId[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropId[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropId[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropId[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropId[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropId[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                PropValue[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                ValidityPeriod = Table_Tamplet.Convert_Int(temp[__column++]);
                ConditionID = Table_Tamplet.Convert_Int(temp[__column++]);
                FlagId = Table_Tamplet.Convert_Int(temp[__column++]);
                FrontId = Table_Tamplet.Convert_Int(temp[__column++]);
                PostId = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class VIPRecord :IRecord
    {
        public static string __TableName = "VIP.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int NeedVipExp { get;        set; }
        public int[] PackItemParam = new int[3];
        public int GetItem { get;        set; }
        public int GetBuff { get;        set; }
        public int GetTitle { get;        set; }
        public string PackageId { get;        set; }
        public int[] BuyItemId = new int[10];
        public int[] BuyItemCount = new int[10];
        public int PKBuyCount { get;        set; }
        public int PKChallengeCD { get;        set; }
        public int Depot { get;        set; }
        public int Repair { get;        set; }
        public int WingAdvanced { get;        set; }
        public int Muse2Reward { get;        set; }
        public int Muse4Reward { get;        set; }
        public int StatueAddCount { get;        set; }
        public int FarmAddRefleshCount { get;        set; }
        public int SceneBossTrans { get;        set; }
        public int EnhanceRatio { get;        set; }
        public int PlotFubenResetCount { get;        set; }
        public int AreaLimitTrans { get;        set; }
        public int DevilBuyCount { get;        set; }
        public int BloodBuyCount { get;        set; }
        public int VipGoldTemple { get;        set; }
        public int VipBossTemple { get;        set; }
        public int SailScanCount { get;        set; }
        public int WishPoolFilterNum { get;        set; }
        public int SentTimes { get;        set; }
        public int PetIslandBuyTimes { get;        set; }
        public int TowerSweepTimes { get;        set; }
        public int OfflineTimeMax { get;        set; }
        public int ItemId { get;        set; }
        public int BuyFlag { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedVipExp = Table_Tamplet.Convert_Int(temp[__column++]);
                PackItemParam[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                PackItemParam[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                PackItemParam[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                GetItem = Table_Tamplet.Convert_Int(temp[__column++]);
                GetBuff = Table_Tamplet.Convert_Int(temp[__column++]);
                GetTitle = Table_Tamplet.Convert_Int(temp[__column++]);
                PackageId = temp[__column++];
                BuyItemId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuyItemCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuyItemId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuyItemCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuyItemId[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuyItemCount[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuyItemId[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuyItemCount[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuyItemId[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuyItemCount[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuyItemId[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuyItemCount[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuyItemId[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuyItemCount[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuyItemId[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuyItemCount[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuyItemId[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuyItemCount[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuyItemId[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuyItemCount[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                PKBuyCount = Table_Tamplet.Convert_Int(temp[__column++]);
                PKChallengeCD = Table_Tamplet.Convert_Int(temp[__column++]);
                Depot = Table_Tamplet.Convert_Int(temp[__column++]);
                Repair = Table_Tamplet.Convert_Int(temp[__column++]);
                WingAdvanced = Table_Tamplet.Convert_Int(temp[__column++]);
                Muse2Reward = Table_Tamplet.Convert_Int(temp[__column++]);
                Muse4Reward = Table_Tamplet.Convert_Int(temp[__column++]);
                StatueAddCount = Table_Tamplet.Convert_Int(temp[__column++]);
                FarmAddRefleshCount = Table_Tamplet.Convert_Int(temp[__column++]);
                SceneBossTrans = Table_Tamplet.Convert_Int(temp[__column++]);
                EnhanceRatio = Table_Tamplet.Convert_Int(temp[__column++]);
                PlotFubenResetCount = Table_Tamplet.Convert_Int(temp[__column++]);
                AreaLimitTrans = Table_Tamplet.Convert_Int(temp[__column++]);
                DevilBuyCount = Table_Tamplet.Convert_Int(temp[__column++]);
                BloodBuyCount = Table_Tamplet.Convert_Int(temp[__column++]);
                VipGoldTemple = Table_Tamplet.Convert_Int(temp[__column++]);
                VipBossTemple = Table_Tamplet.Convert_Int(temp[__column++]);
                SailScanCount = Table_Tamplet.Convert_Int(temp[__column++]);
                WishPoolFilterNum = Table_Tamplet.Convert_Int(temp[__column++]);
                SentTimes = Table_Tamplet.Convert_Int(temp[__column++]);
                PetIslandBuyTimes = Table_Tamplet.Convert_Int(temp[__column++]);
                TowerSweepTimes = Table_Tamplet.Convert_Int(temp[__column++]);
                OfflineTimeMax = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId = Table_Tamplet.Convert_Int(temp[__column++]);
                BuyFlag = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class RechargeActiveRecord :IRecord
    {
        public static string __TableName = "RechargeActive.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int Type { get;        set; }
        public int SonType { get;        set; }
        public string LabelText { get;        set; }
        public int Icon { get;        set; }
        public List<int> ServerIds = new List<int>();
        public int OpenRule { get;        set; }
        public string StartTime { get;        set; }
        public string EndTime { get;        set; }
        public int ExtraId { get;        set; }
        public int ExtraCount { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                SonType = Table_Tamplet.Convert_Int(temp[__column++]);
                LabelText = temp[__column++];
                Icon = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(ServerIds,temp[__column++]);
                OpenRule = Table_Tamplet.Convert_Int(temp[__column++]);
                StartTime = temp[__column++];
                EndTime = temp[__column++];
                ExtraId = Table_Tamplet.Convert_Int(temp[__column++]);
                ExtraCount = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class RechargeActiveNoticeRecord :IRecord
    {
        public static string __TableName = "RechargeActiveNotice.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int ActivityId { get;        set; }
        public string Desc { get;        set; }
        public int IsBtnShow { get;        set; }
        public string BtnText { get;        set; }
        public int GotoUiId { get;        set; }
        public int GotoUiTab { get;        set; }
        public int ConditionId { get;        set; }
        public int[] ItemId = new int[6];
        public int[] ItemCount = new int[6];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                ActivityId = Table_Tamplet.Convert_Int(temp[__column++]);
                Desc = temp[__column++];
                IsBtnShow = Table_Tamplet.Convert_Int(temp[__column++]);
                BtnText = temp[__column++];
                GotoUiId = Table_Tamplet.Convert_Int(temp[__column++]);
                GotoUiTab = Table_Tamplet.Convert_Int(temp[__column++]);
                ConditionId = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[5] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class RechargeActiveInvestmentRecord :IRecord
    {
        public static string __TableName = "RechargeActiveInvestment.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int ActivityId { get;        set; }
        public string Type { get;        set; }
        public string Tips { get;        set; }
        public string ConditionText { get;        set; }
        public string BtnText { get;        set; }
        public int GoToUI { get;        set; }
        public int GoToUITab { get;        set; }
        public int BgIconId { get;        set; }
        public int ExtraId { get;        set; }
        public int ResetCount { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                ActivityId = Table_Tamplet.Convert_Int(temp[__column++]);
                Type = temp[__column++];
                Tips = temp[__column++];
                ConditionText = temp[__column++];
                BtnText = temp[__column++];
                GoToUI = Table_Tamplet.Convert_Int(temp[__column++]);
                GoToUITab = Table_Tamplet.Convert_Int(temp[__column++]);
                BgIconId = Table_Tamplet.Convert_Int(temp[__column++]);
                ExtraId = Table_Tamplet.Convert_Int(temp[__column++]);
                ResetCount = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class RechargeActiveInvestmentRewardRecord :IRecord
    {
        public static string __TableName = "RechargeActiveInvestmentReward.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int Type { get;        set; }
        public int ConditionId { get;        set; }
        public int DiaNeedCount { get;        set; }
        public int[] ItemId = new int[5];
        public int[] ItemCount = new int[5];
        public int Flag { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                ConditionId = Table_Tamplet.Convert_Int(temp[__column++]);
                DiaNeedCount = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Flag = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class RechargeActiveCumulativeRecord :IRecord
    {
        public static string __TableName = "RechargeActiveCumulative.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string typeStr { get;        set; }
        public string BuyConditionText { get;        set; }
        public int ActivityId { get;        set; }
        public int BgIconId { get;        set; }
        public int ConditionId { get;        set; }
        public int NeedItemId { get;        set; }
        public int NeedItemCount { get;        set; }
        public int ExtraId { get;        set; }
        public int ResetCount { get;        set; }
        public int FlagTrueId { get;        set; }
        public List<int> FlagFalseId = new List<int>();
        public int CanRepeatBuy { get;        set; }
        public string ChargeID { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                typeStr = temp[__column++];
                BuyConditionText = temp[__column++];
                ActivityId = Table_Tamplet.Convert_Int(temp[__column++]);
                BgIconId = Table_Tamplet.Convert_Int(temp[__column++]);
                ConditionId = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemId = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItemCount = Table_Tamplet.Convert_Int(temp[__column++]);
                ExtraId = Table_Tamplet.Convert_Int(temp[__column++]);
                ResetCount = Table_Tamplet.Convert_Int(temp[__column++]);
                FlagTrueId = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(FlagFalseId,temp[__column++]);
                CanRepeatBuy = Table_Tamplet.Convert_Int(temp[__column++]);
                ChargeID = temp[__column++];
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class RechargeActiveCumulativeRewardRecord :IRecord
    {
        public static string __TableName = "RechargeActiveCumulativeReward.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int Type { get;        set; }
        public string Desc1 { get;        set; }
        public string Desc2 { get;        set; }
        public int ConditionId { get;        set; }
        public int ItemId { get;        set; }
        public int ItemCount { get;        set; }
        public int Flag { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                Desc1 = temp[__column++];
                Desc2 = temp[__column++];
                ConditionId = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount = Table_Tamplet.Convert_Int(temp[__column++]);
                Flag = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class GiftCodeRecord :IRecord
    {
        public static string __TableName = "GiftCode.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int DropId { get;        set; }
        public int FlagId { get;        set; }
        public string StartTime { get;        set; }
        public string EndTime { get;        set; }
        public string Drop1Id { get;        set; }
        public string Drop2Id { get;        set; }
        public string Drop3Id { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                DropId = Table_Tamplet.Convert_Int(temp[__column++]);
                FlagId = Table_Tamplet.Convert_Int(temp[__column++]);
                StartTime = temp[__column++];
                EndTime = temp[__column++];
                Drop1Id = temp[__column++];
                Drop2Id = temp[__column++];
                Drop3Id = temp[__column++];
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class GMCommandRecord :IRecord
    {
        public static string __TableName = "GMCommand.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string Command { get;        set; }
        public int Type { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Command = temp[__column++];
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class AuctionType1Record :IRecord
    {
        public static string __TableName = "AuctionType1.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public List<int> SonList = new List<int>();
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(SonList,temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class AuctionType2Record :IRecord
    {
        public static string __TableName = "AuctionType2.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public List<int> SonList = new List<int>();
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(SonList,temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class AuctionType3Record :IRecord
    {
        public static string __TableName = "AuctionType3.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class YunYingRecord :IRecord
    {
        public static string __TableName = "YunYing.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string Name { get;        set; }
        public string Desc { get;        set; }
        public int IsOpen { get;        set; }
        public int BtnIcon { get;        set; }
        public int showYouXianJi { get;        set; }
        public int ParentType { get;        set; }
        public int Subtype { get;        set; }
        public int ConditionId { get;        set; }
        public int UIRank { get;        set; }
        public int NeetItem { get;        set; }
        public int NeetItemCount { get;        set; }
        public int XiangDuiOpenServerTime { get;        set; }
        public int RewardOpenTime { get;        set; }
        public int LastTime { get;        set; }
        public int GuideOperationActivityId { get;        set; }
        public int GuideUI { get;        set; }
        public int NeedCompleteId { get;        set; }
        public List<int> StrParam = new List<int>();
        public int[] Param = new int[4];
        public int LingQuTimes { get;        set; }
        public string JiangLiJob1 { get;        set; }
        public string JiangLiJob2 { get;        set; }
        public string JiangLiJob3 { get;        set; }
        public int SuoYingUi { get;        set; }
        public int ModelShow { get;        set; }
        public int ArtTitle { get;        set; }
        public int ArtPicture { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Name = temp[__column++];
                Desc = temp[__column++];
                IsOpen = Table_Tamplet.Convert_Int(temp[__column++]);
                BtnIcon = Table_Tamplet.Convert_Int(temp[__column++]);
                showYouXianJi = Table_Tamplet.Convert_Int(temp[__column++]);
                ParentType = Table_Tamplet.Convert_Int(temp[__column++]);
                Subtype = Table_Tamplet.Convert_Int(temp[__column++]);
                ConditionId = Table_Tamplet.Convert_Int(temp[__column++]);
                UIRank = Table_Tamplet.Convert_Int(temp[__column++]);
                NeetItem = Table_Tamplet.Convert_Int(temp[__column++]);
                NeetItemCount = Table_Tamplet.Convert_Int(temp[__column++]);
                XiangDuiOpenServerTime = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardOpenTime = Table_Tamplet.Convert_Int(temp[__column++]);
                LastTime = Table_Tamplet.Convert_Int(temp[__column++]);
                GuideOperationActivityId = Table_Tamplet.Convert_Int(temp[__column++]);
                GuideUI = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedCompleteId = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(StrParam,temp[__column++]);
                Param[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                LingQuTimes = Table_Tamplet.Convert_Int(temp[__column++]);
                JiangLiJob1 = temp[__column++];
                JiangLiJob2 = temp[__column++];
                JiangLiJob3 = temp[__column++];
                SuoYingUi = Table_Tamplet.Convert_Int(temp[__column++]);
                ModelShow = Table_Tamplet.Convert_Int(temp[__column++]);
                ArtTitle = Table_Tamplet.Convert_Int(temp[__column++]);
                ArtPicture = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class OperationActivityRecord :IRecord
    {
        public static string __TableName = "OperationActivity.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int activityType { get;        set; }
        public int activitySonType { get;        set; }
        public string Name { get;        set; }
        public int BkgIconId { get;        set; }
        public List<int> activityServer = new List<int>();
        public int openTimeRule { get;        set; }
        public string openTime { get;        set; }
        public string closeTime { get;        set; }
        public string LastTime { get;        set; }
        public int ParentTypeId { get;        set; }
        public int UIType { get;        set; }
        public string Desc { get;        set; }
        public int SmallIcon { get;        set; }
        public List<int> ModelPath = new List<int>();
        public int[] Param = new int[2];
        public List<int> StrParam = new List<int>();
        public int RankWeight { get;        set; }
        public int Categorytags { get;        set; }
        public int RedDotShows { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                activityType = Table_Tamplet.Convert_Int(temp[__column++]);
                activitySonType = Table_Tamplet.Convert_Int(temp[__column++]);
                Name = temp[__column++];
                BkgIconId = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(activityServer,temp[__column++]);
                openTimeRule = Table_Tamplet.Convert_Int(temp[__column++]);
                openTime = temp[__column++];
                closeTime = temp[__column++];
                LastTime = temp[__column++];
                ParentTypeId = Table_Tamplet.Convert_Int(temp[__column++]);
                UIType = Table_Tamplet.Convert_Int(temp[__column++]);
                Desc = temp[__column++];
                SmallIcon = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(ModelPath,temp[__column++]);
                Param[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Param[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(StrParam,temp[__column++]);
                RankWeight = Table_Tamplet.Convert_Int(temp[__column++]);
                Categorytags = Table_Tamplet.Convert_Int(temp[__column++]);
                RedDotShows = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class FirstRechargeRecord :IRecord
    {
        public static string __TableName = "FirstRecharge.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int diamond { get;        set; }
        public string job1Items { get;        set; }
        public string job2Items { get;        set; }
        public string job3Items { get;        set; }
        public string yuliu { get;        set; }
        public int label { get;        set; }
        public int desc { get;        set; }
        public int flag { get;        set; }
        public string job1Path { get;        set; }
        public string job2Path { get;        set; }
        public string job3Path { get;        set; }
        public string Announcement { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                diamond = Table_Tamplet.Convert_Int(temp[__column++]);
                job1Items = temp[__column++];
                job2Items = temp[__column++];
                job3Items = temp[__column++];
                yuliu = temp[__column++];
                label = Table_Tamplet.Convert_Int(temp[__column++]);
                desc = Table_Tamplet.Convert_Int(temp[__column++]);
                flag = Table_Tamplet.Convert_Int(temp[__column++]);
                job1Path = temp[__column++];
                job2Path = temp[__column++];
                job3Path = temp[__column++];
                Announcement = temp[__column++];
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class MieShiRecord :IRecord
    {
        public static string __TableName = "MieShi.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int ReviveCostDiamond { get;        set; }
        public int ReviveCostItemId { get;        set; }
        public int ReviveCostItemNum { get;        set; }
        public int ReviveTime { get;        set; }
        public int ReviveAddTime { get;        set; }
        public int MaxReviveTime { get;        set; }
        public int PriorityEnterCount { get;        set; }
        public int MaxActivityCount { get;        set; }
        public int FirstMobTime { get;        set; }
        public int MobIntervalTime { get;        set; }
        public List<int> Monster1IdList = new List<int>();
        public List<int> Monster1NumList = new List<int>();
        public List<int> Monster2IdList = new List<int>();
        public List<int> Monster2NumList = new List<int>();
        public List<int> Monster3IdList = new List<int>();
        public List<int> Monster3NumList = new List<int>();
        public List<int> Monster4IdList = new List<int>();
        public List<int> Monster4NumList = new List<int>();
        public int Monst1TimeMin { get;        set; }
        public int Monst1TimeMax { get;        set; }
        public List<int> Monst1RandomId = new List<int>();
        public List<int> Monst1Num = new List<int>();
        public int Monst2TimeMin { get;        set; }
        public int Monst2TimeMax { get;        set; }
        public List<int> Monst2RandomId = new List<int>();
        public List<int> Monst2Num = new List<int>();
        public int Monst3TimeMin { get;        set; }
        public int Monst3TimeMax { get;        set; }
        public List<int> Monst3RandomId = new List<int>();
        public List<int> Monst3Num = new List<int>();
        public int OpenDay { get;        set; }
        public int OpenIntervalDay { get;        set; }
        public int FuBenID { get;        set; }
        public int BossCgID { get;        set; }
        public int BossDropBoxId { get;        set; }
        public int BoxAwardId { get;        set; }
        public int[] Entry_x = new int[5];
        public int[] Entry_z = new int[5];
        public int[] Safe_x = new int[5];
        public int[] Safe_z = new int[5];
        public float Difficult { get;        set; }
        public string OpenTime { get;        set; }
        public int ScoreLevel { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                ReviveCostDiamond = Table_Tamplet.Convert_Int(temp[__column++]);
                ReviveCostItemId = Table_Tamplet.Convert_Int(temp[__column++]);
                ReviveCostItemNum = Table_Tamplet.Convert_Int(temp[__column++]);
                ReviveTime = Table_Tamplet.Convert_Int(temp[__column++]);
                ReviveAddTime = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxReviveTime = Table_Tamplet.Convert_Int(temp[__column++]);
                PriorityEnterCount = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxActivityCount = Table_Tamplet.Convert_Int(temp[__column++]);
                FirstMobTime = Table_Tamplet.Convert_Int(temp[__column++]);
                MobIntervalTime = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(Monster1IdList,temp[__column++]);
                Table_Tamplet.Convert_Value(Monster1NumList,temp[__column++]);
                Table_Tamplet.Convert_Value(Monster2IdList,temp[__column++]);
                Table_Tamplet.Convert_Value(Monster2NumList,temp[__column++]);
                Table_Tamplet.Convert_Value(Monster3IdList,temp[__column++]);
                Table_Tamplet.Convert_Value(Monster3NumList,temp[__column++]);
                Table_Tamplet.Convert_Value(Monster4IdList,temp[__column++]);
                Table_Tamplet.Convert_Value(Monster4NumList,temp[__column++]);
                Monst1TimeMin = Table_Tamplet.Convert_Int(temp[__column++]);
                Monst1TimeMax = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(Monst1RandomId,temp[__column++]);
                Table_Tamplet.Convert_Value(Monst1Num,temp[__column++]);
                Monst2TimeMin = Table_Tamplet.Convert_Int(temp[__column++]);
                Monst2TimeMax = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(Monst2RandomId,temp[__column++]);
                Table_Tamplet.Convert_Value(Monst2Num,temp[__column++]);
                Monst3TimeMin = Table_Tamplet.Convert_Int(temp[__column++]);
                Monst3TimeMax = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(Monst3RandomId,temp[__column++]);
                Table_Tamplet.Convert_Value(Monst3Num,temp[__column++]);
                OpenDay = Table_Tamplet.Convert_Int(temp[__column++]);
                OpenIntervalDay = Table_Tamplet.Convert_Int(temp[__column++]);
                FuBenID = Table_Tamplet.Convert_Int(temp[__column++]);
                BossCgID = Table_Tamplet.Convert_Int(temp[__column++]);
                BossDropBoxId = Table_Tamplet.Convert_Int(temp[__column++]);
                BoxAwardId = Table_Tamplet.Convert_Int(temp[__column++]);
                Entry_x[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Entry_z[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Entry_x[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Entry_z[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Entry_x[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Entry_z[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Entry_x[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Entry_z[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Entry_x[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Entry_z[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Safe_x[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Safe_z[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Safe_x[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Safe_z[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Safe_x[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Safe_z[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Safe_x[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Safe_z[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Safe_x[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Safe_z[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Difficult = Table_Tamplet.Convert_Float(temp[__column++]);
                OpenTime = temp[__column++];
                ScoreLevel = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class MieShiPublicRecord :IRecord
    {
        public static string __TableName = "MieShiPublic.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int CostType { get;        set; }
        public int CostNum { get;        set; }
        public int ItemId { get;        set; }
        public int ItemNum { get;        set; }
        public int RaiseHP { get;        set; }
        public int GainContribute { get;        set; }
        public int LevelKeepTime { get;        set; }
        public int MineId { get;        set; }
        public int MaxMineNum { get;        set; }
        public int BoxId { get;        set; }
        public int MaxBoxNum { get;        set; }
        public int NormalDamageScore { get;        set; }
        public int EliteDamageScore { get;        set; }
        public int BossDamageScore { get;        set; }
        public int BigBossDamageScore { get;        set; }
        public int MineGainProb { get;        set; }
        public int[] MineScore = new int[2];
        public int MaxRaiseHP { get;        set; }
        public int MaxBatteryLevel { get;        set; }
        public int CanApplyTime { get;        set; }
        public int BatteryPromoteTime { get;        set; }
        public int BuffId { get;        set; }
        public int BuffLevel { get;        set; }
        public int PromoteAwardId { get;        set; }
        public int WorshipItemId { get;        set; }
        public int WorshipItemNum { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                CostType = Table_Tamplet.Convert_Int(temp[__column++]);
                CostNum = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemNum = Table_Tamplet.Convert_Int(temp[__column++]);
                RaiseHP = Table_Tamplet.Convert_Int(temp[__column++]);
                GainContribute = Table_Tamplet.Convert_Int(temp[__column++]);
                LevelKeepTime = Table_Tamplet.Convert_Int(temp[__column++]);
                MineId = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxMineNum = Table_Tamplet.Convert_Int(temp[__column++]);
                BoxId = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxBoxNum = Table_Tamplet.Convert_Int(temp[__column++]);
                NormalDamageScore = Table_Tamplet.Convert_Int(temp[__column++]);
                EliteDamageScore = Table_Tamplet.Convert_Int(temp[__column++]);
                BossDamageScore = Table_Tamplet.Convert_Int(temp[__column++]);
                BigBossDamageScore = Table_Tamplet.Convert_Int(temp[__column++]);
                MineGainProb = Table_Tamplet.Convert_Int(temp[__column++]);
                MineScore[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                MineScore[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxRaiseHP = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxBatteryLevel = Table_Tamplet.Convert_Int(temp[__column++]);
                CanApplyTime = Table_Tamplet.Convert_Int(temp[__column++]);
                BatteryPromoteTime = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffId = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffLevel = Table_Tamplet.Convert_Int(temp[__column++]);
                PromoteAwardId = Table_Tamplet.Convert_Int(temp[__column++]);
                WorshipItemId = Table_Tamplet.Convert_Int(temp[__column++]);
                WorshipItemNum = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class DefendCityRewardRecord :IRecord
    {
        public static string __TableName = "DefendCityReward.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int ActivityId { get;        set; }
        public List<int> Rank = new List<int>();
        public int ActivateTitle { get;        set; }
        public int[] RankItemID = new int[4];
        public int[] RankItemCount = new int[4];
        public int MailId { get;        set; }
        public int MailId2 { get;        set; }
        public int MailId3 { get;        set; }
        public int MailId4 { get;        set; }
        public int MailId5 { get;        set; }
        public int MailId6 { get;        set; }
        public string RankIcon { get;        set; }
        public List<int> Rate = new List<int>();
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                ActivityId = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(Rank,temp[__column++]);
                ActivateTitle = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemID[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemID[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemID[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemCount[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemID[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemCount[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                MailId = Table_Tamplet.Convert_Int(temp[__column++]);
                MailId2 = Table_Tamplet.Convert_Int(temp[__column++]);
                MailId3 = Table_Tamplet.Convert_Int(temp[__column++]);
                MailId4 = Table_Tamplet.Convert_Int(temp[__column++]);
                MailId5 = Table_Tamplet.Convert_Int(temp[__column++]);
                MailId6 = Table_Tamplet.Convert_Int(temp[__column++]);
                RankIcon = temp[__column++];
                Table_Tamplet.Convert_Value(Rate,temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class DefendCityDevoteRewardRecord :IRecord
    {
        public static string __TableName = "DefendCityDevoteReward.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public List<int> Rank = new List<int>();
        public int[] RankItemID = new int[4];
        public int[] RankItemCount = new int[4];
        public int MailId { get;        set; }
        public int MailId2 { get;        set; }
        public int MailId3 { get;        set; }
        public int MailId4 { get;        set; }
        public int MailId5 { get;        set; }
        public int MailId6 { get;        set; }
        public string ContributionIcon { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(Rank,temp[__column++]);
                RankItemID[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemID[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemID[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemCount[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemID[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemCount[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                MailId = Table_Tamplet.Convert_Int(temp[__column++]);
                MailId2 = Table_Tamplet.Convert_Int(temp[__column++]);
                MailId3 = Table_Tamplet.Convert_Int(temp[__column++]);
                MailId4 = Table_Tamplet.Convert_Int(temp[__column++]);
                MailId5 = Table_Tamplet.Convert_Int(temp[__column++]);
                MailId6 = Table_Tamplet.Convert_Int(temp[__column++]);
                ContributionIcon = temp[__column++];
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class BatteryLevelRecord :IRecord
    {
        public static string __TableName = "BatteryLevel.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int BatterySkillId { get;        set; }
        public int BatterySkillDesc { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                BatterySkillId = Table_Tamplet.Convert_Int(temp[__column++]);
                BatterySkillDesc = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class BatteryBaseRecord :IRecord
    {
        public static string __TableName = "BatteryBase.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] BatteryNpcId = new int[3];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                BatteryNpcId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                BatteryNpcId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                BatteryNpcId[2] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class PlayerDropRecord :IRecord
    {
        public static string __TableName = "PlayerDrop.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int OnlineTime { get;        set; }
        public int MonsterId { get;        set; }
        public int job1Drop { get;        set; }
        public int job2Drop { get;        set; }
        public int job3Drop { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                OnlineTime = Table_Tamplet.Convert_Int(temp[__column++]);
                MonsterId = Table_Tamplet.Convert_Int(temp[__column++]);
                job1Drop = Table_Tamplet.Convert_Int(temp[__column++]);
                job2Drop = Table_Tamplet.Convert_Int(temp[__column++]);
                job3Drop = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class BuffGroupRecord :IRecord
    {
        public static string __TableName = "BuffGroup.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public List<int> BuffID = new List<int>();
        public List<int> QuanZhong = new List<int>();
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(BuffID,temp[__column++]);
                Table_Tamplet.Convert_Value(QuanZhong,temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class BangBuffRecord :IRecord
    {
        public static string __TableName = "BangBuff.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] BuffGoldId = new int[5];
        public int[] BuffGoldPrice = new int[5];
        public int[] BuffDiamodId = new int[5];
        public int[] BuffDiamodPrice = new int[5];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffGoldId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffGoldPrice[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffDiamodId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffDiamodPrice[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffGoldId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffGoldPrice[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffDiamodId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffDiamodPrice[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffGoldId[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffGoldPrice[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffDiamodId[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffDiamodPrice[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffGoldId[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffGoldPrice[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffDiamodId[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffDiamodPrice[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffGoldId[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffGoldPrice[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffDiamodId[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffDiamodPrice[4] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class MieshiTowerRewardRecord :IRecord
    {
        public static string __TableName = "MieshiTowerReward.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public List<int> TimesStep = new List<int>();
        public int DiamondCost { get;        set; }
        public int StepReward { get;        set; }
        public int OnceReward { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(TimesStep,temp[__column++]);
                DiamondCost = Table_Tamplet.Convert_Int(temp[__column++]);
                StepReward = Table_Tamplet.Convert_Int(temp[__column++]);
                OnceReward = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class ClimbingTowerRecord :IRecord
    {
        public static string __TableName = "ClimbingTower.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int FubenId { get;        set; }
        public List<int> RewardList = new List<int>();
        public List<int> NumList = new List<int>();
        public List<int> OnceRewardList = new List<int>();
        public List<int> OnceNumList = new List<int>();
        public int SweepFloor { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                FubenId = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(RewardList,temp[__column++]);
                Table_Tamplet.Convert_Value(NumList,temp[__column++]);
                Table_Tamplet.Convert_Value(OnceRewardList,temp[__column++]);
                Table_Tamplet.Convert_Value(OnceNumList,temp[__column++]);
                SweepFloor = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class AcientBattleFieldRecord :IRecord
    {
        public static string __TableName = "AcientBattleField.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int CharacterBaseId { get;        set; }
        public int CostEnergy { get;        set; }
        public int SpawnTimeHour { get;        set; }
        public int PosX { get;        set; }
        public int PosY { get;        set; }
        public int[] Item = new int[4];
        public int[] ItemCount = new int[4];
        public int[] ItemDropWight = new int[4];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                CharacterBaseId = Table_Tamplet.Convert_Int(temp[__column++]);
                CostEnergy = Table_Tamplet.Convert_Int(temp[__column++]);
                SpawnTimeHour = Table_Tamplet.Convert_Int(temp[__column++]);
                PosX = Table_Tamplet.Convert_Int(temp[__column++]);
                PosY = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemDropWight[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemDropWight[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemDropWight[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemDropWight[3] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class ConsumArrayRecord :IRecord
    {
        public static string __TableName = "ConsumArray.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] ItemId = new int[10];
        public int[] ItemCount = new int[10];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[9] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class BookBaseRecord :IRecord
    {
        public static string __TableName = "BookBase.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] AttrList = new int[33];
        public int SummonAttr { get;        set; }
        public int AddAttr { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrList[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrList[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrList[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrList[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrList[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrList[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrList[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrList[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrList[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrList[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrList[10] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrList[11] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrList[12] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrList[13] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrList[14] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrList[15] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrList[16] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrList[17] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrList[18] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrList[19] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrList[20] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrList[21] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrList[22] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrList[23] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrList[24] = Table_Tamplet.Convert_Int(temp[__column++]);
                AttrList[25] = Table_Tamplet.Convert_Int(temp[__column++]);
                SummonAttr = Table_Tamplet.Convert_Int(temp[__column++]);
                AddAttr = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class MountRecord :IRecord
    {
        public static string __TableName = "Mount.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int ItemId { get;        set; }
        public int Step { get;        set; }
        public int NextId { get;        set; }
        public int Level { get;        set; }
        public int NeedExp { get;        set; }
        public int NeedItem { get;        set; }
        public int GetExp { get;        set; }
        public int[] Attr = new int[9];
        public int[] Value = new int[9];
        public int SkillId { get;        set; }
        public int Special { get;        set; }
        public int IsOpen { get;        set; }
        public float speed { get;        set; }
        public int IsPermanent { get;        set; }
        public float ValidityData { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId = Table_Tamplet.Convert_Int(temp[__column++]);
                Step = Table_Tamplet.Convert_Int(temp[__column++]);
                NextId = Table_Tamplet.Convert_Int(temp[__column++]);
                Level = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedExp = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedItem = Table_Tamplet.Convert_Int(temp[__column++]);
                GetExp = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Value[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Value[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Value[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Value[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Value[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                Value[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                Value[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                Value[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                Value[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                SkillId = Table_Tamplet.Convert_Int(temp[__column++]);
                Special = Table_Tamplet.Convert_Int(temp[__column++]);
                IsOpen = Table_Tamplet.Convert_Int(temp[__column++]);
                speed = Table_Tamplet.Convert_Float(temp[__column++]);
                IsPermanent = Table_Tamplet.Convert_Int(temp[__column++]);
                ValidityData = Table_Tamplet.Convert_Float(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class MountSkillRecord :IRecord
    {
        public static string __TableName = "MountSkill.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int MaxLevel { get;        set; }
        public List<int> CostList = new List<int>();
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxLevel = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(CostList,temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class MountFeedRecord :IRecord
    {
        public static string __TableName = "MountFeed.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int UseLimit { get;        set; }
        public int MaxCount { get;        set; }
        public int[] Attr = new int[5];
        public int[] Value = new int[5];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                UseLimit = Table_Tamplet.Convert_Int(temp[__column++]);
                MaxCount = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Value[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Value[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Value[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Value[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Attr[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Value[4] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class MayaBaseRecord :IRecord
    {
        public static string __TableName = "MayaBase.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int Type { get;        set; }
        public int ActiveType { get;        set; }
        public List<int> ActiveParam = new List<int>();
        public int FunBenId { get;        set; }
        public List<int> SkillIds = new List<int>();
        public int FlagId { get;        set; }
        public int NextId { get;        set; }
        public int FinishFlagId { get;        set; }
        public List<int> Award = new List<int>();
        public int GotAward { get;        set; }
        public int TitleId { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                ActiveType = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(ActiveParam,temp[__column++]);
                FunBenId = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(SkillIds,temp[__column++]);
                FlagId = Table_Tamplet.Convert_Int(temp[__column++]);
                NextId = Table_Tamplet.Convert_Int(temp[__column++]);
                FinishFlagId = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(Award,temp[__column++]);
                GotAward = Table_Tamplet.Convert_Int(temp[__column++]);
                TitleId = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class OfflineExperienceRecord :IRecord
    {
        public static string __TableName = "OfflineExperience.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int levelMin { get;        set; }
        public int levelMax { get;        set; }
        public int[] DropId = new int[3];
        public int[] DropCD = new int[3];
        public int Money { get;        set; }
        public int Exp { get;        set; }
        public int Time { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                levelMin = Table_Tamplet.Convert_Int(temp[__column++]);
                levelMax = Table_Tamplet.Convert_Int(temp[__column++]);
                DropId[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropCD[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropId[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropCD[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropId[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                DropCD[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Money = Table_Tamplet.Convert_Int(temp[__column++]);
                Exp = Table_Tamplet.Convert_Int(temp[__column++]);
                Time = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class SurveyRecord :IRecord
    {
        public static string __TableName = "Survey.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int type { get;        set; }
        public int param { get;        set; }
        public int flagHad { get;        set; }
        public int flagCan { get;        set; }
        public int reward { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                type = Table_Tamplet.Convert_Int(temp[__column++]);
                param = Table_Tamplet.Convert_Int(temp[__column++]);
                flagHad = Table_Tamplet.Convert_Int(temp[__column++]);
                flagCan = Table_Tamplet.Convert_Int(temp[__column++]);
                reward = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class PreferentialRecord :IRecord
    {
        public static string __TableName = "Preferential.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int Type { get;        set; }
        public int DelaDay { get;        set; }
        public int Exdata { get;        set; }
        public int ItemId { get;        set; }
        public int Count { get;        set; }
        public int OldPrice { get;        set; }
        public int NowPrice { get;        set; }
        public int IconId { get;        set; }
        public int More1 { get;        set; }
        public int More2 { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Type = Table_Tamplet.Convert_Int(temp[__column++]);
                DelaDay = Table_Tamplet.Convert_Int(temp[__column++]);
                Exdata = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemId = Table_Tamplet.Convert_Int(temp[__column++]);
                Count = Table_Tamplet.Convert_Int(temp[__column++]);
                OldPrice = Table_Tamplet.Convert_Int(temp[__column++]);
                NowPrice = Table_Tamplet.Convert_Int(temp[__column++]);
                IconId = Table_Tamplet.Convert_Int(temp[__column++]);
                More1 = Table_Tamplet.Convert_Int(temp[__column++]);
                More2 = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class KaiFuRecord :IRecord
    {
        public static string __TableName = "KaiFu.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] Week = new int[7];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Week[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Week[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Week[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Week[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Week[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Week[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                Week[6] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class BossHomeRecord :IRecord
    {
        public static string __TableName = "BossHome.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int CharacterBaseId { get;        set; }
        public string RefreshTime { get;        set; }
        public int PosX { get;        set; }
        public int PosY { get;        set; }
        public int[] Item = new int[4];
        public int[] ItemCount = new int[4];
        public int[] ItemDropWight = new int[4];
        public int Scene { get;        set; }
        public int VipLimit { get;        set; }
        public int CostType { get;        set; }
        public int CostNum { get;        set; }
        public int SceneNpcId { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                CharacterBaseId = Table_Tamplet.Convert_Int(temp[__column++]);
                RefreshTime = temp[__column++];
                PosX = Table_Tamplet.Convert_Int(temp[__column++]);
                PosY = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemDropWight[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemDropWight[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemDropWight[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Item[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemCount[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemDropWight[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Scene = Table_Tamplet.Convert_Int(temp[__column++]);
                VipLimit = Table_Tamplet.Convert_Int(temp[__column++]);
                CostType = Table_Tamplet.Convert_Int(temp[__column++]);
                CostNum = Table_Tamplet.Convert_Int(temp[__column++]);
                SceneNpcId = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class WarFlagRecord :IRecord
    {
        public static string __TableName = "WarFlag.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int Icon { get;        set; }
        public int FlagModel { get;        set; }
        public int FlagInMap { get;        set; }
        public int OccupyFlag { get;        set; }
        public int CollectTime { get;        set; }
        public List<int> BelongToTime = new List<int>();
        public int EXPAdd { get;        set; }
        public int MiningAdd { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Icon = Table_Tamplet.Convert_Int(temp[__column++]);
                FlagModel = Table_Tamplet.Convert_Int(temp[__column++]);
                FlagInMap = Table_Tamplet.Convert_Int(temp[__column++]);
                OccupyFlag = Table_Tamplet.Convert_Int(temp[__column++]);
                CollectTime = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(BelongToTime,temp[__column++]);
                EXPAdd = Table_Tamplet.Convert_Int(temp[__column++]);
                MiningAdd = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class LodeRecord :IRecord
    {
        public static string __TableName = "Lode.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int Icon { get;        set; }
        public int NpcId { get;        set; }
        public int LodeInMap { get;        set; }
        public int OccupyLode { get;        set; }
        public int CanCollectNum { get;        set; }
        public int LodeRefreshTime { get;        set; }
        public int LodeX { get;        set; }
        public int LodeY { get;        set; }
        public int[] LodeOutput = new int[2];
        public int[] OutputShow = new int[4];
        public int Addition { get;        set; }
        public List<int> ActiveTime1 = new List<int>();
        public List<int> ActiveTime2 = new List<int>();
        public int AllianceRes { get;        set; }
        public int[] AllianceOutput = new int[3];
        public int[] AllianceOutputNum = new int[3];
        public int ExDataId { get;        set; }
        public int WorshipConsumeItemId { get;        set; }
        public int WorshipConsumeItemCount { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Icon = Table_Tamplet.Convert_Int(temp[__column++]);
                NpcId = Table_Tamplet.Convert_Int(temp[__column++]);
                LodeInMap = Table_Tamplet.Convert_Int(temp[__column++]);
                OccupyLode = Table_Tamplet.Convert_Int(temp[__column++]);
                CanCollectNum = Table_Tamplet.Convert_Int(temp[__column++]);
                LodeRefreshTime = Table_Tamplet.Convert_Int(temp[__column++]);
                LodeX = Table_Tamplet.Convert_Int(temp[__column++]);
                LodeY = Table_Tamplet.Convert_Int(temp[__column++]);
                LodeOutput[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                LodeOutput[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                OutputShow[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                OutputShow[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                OutputShow[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                OutputShow[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Addition = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(ActiveTime1,temp[__column++]);
                Table_Tamplet.Convert_Value(ActiveTime2,temp[__column++]);
                AllianceRes = Table_Tamplet.Convert_Int(temp[__column++]);
                AllianceOutput[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                AllianceOutputNum[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                AllianceOutput[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                AllianceOutputNum[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                AllianceOutput[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                AllianceOutputNum[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ExDataId = Table_Tamplet.Convert_Int(temp[__column++]);
                WorshipConsumeItemId = Table_Tamplet.Convert_Int(temp[__column++]);
                WorshipConsumeItemCount = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class MainActivityRecord :IRecord
    {
        public static string __TableName = "MainActivity.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] Week = new int[7];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Week[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Week[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Week[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Week[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Week[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Week[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                Week[6] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class ObjectTableRecord :IRecord
    {
        public static string __TableName = "ObjectTable.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int TaskType { get;        set; }
        public int EventType { get;        set; }
        public int ExData { get;        set; }
        public int NeedCount { get;        set; }
        public int[] Reward = new int[3];
        public int[] RewardNum = new int[3];
        public int IsGet { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                TaskType = Table_Tamplet.Convert_Int(temp[__column++]);
                EventType = Table_Tamplet.Convert_Int(temp[__column++]);
                ExData = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedCount = Table_Tamplet.Convert_Int(temp[__column++]);
                Reward[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardNum[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Reward[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardNum[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Reward[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                RewardNum[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                IsGet = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class BatteryBaseNewRecord :IRecord
    {
        public static string __TableName = "BatteryBaseNew.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string BatteryNpcId { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                BatteryNpcId = temp[__column++];
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class CheckenRecord :IRecord
    {
        public static string __TableName = "Checken.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] ItemID = new int[5];
        public int[] Num = new int[5];
        public int ExData1 { get;        set; }
        public int ExData2 { get;        set; }
        public int BuffId { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemID[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                Num[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemID[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                Num[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemID[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                Num[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemID[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                Num[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                ItemID[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                Num[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                ExData1 = Table_Tamplet.Convert_Int(temp[__column++]);
                ExData2 = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffId = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class CheckenLvRecord :IRecord
    {
        public static string __TableName = "CheckenLv.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int NeedExp { get;        set; }
        public int BaseBuff { get;        set; }
        public int BuffGroup { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                NeedExp = Table_Tamplet.Convert_Int(temp[__column++]);
                BaseBuff = Table_Tamplet.Convert_Int(temp[__column++]);
                BuffGroup = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class CheckenRewardRecord :IRecord
    {
        public static string __TableName = "CheckenReward.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int Rank { get;        set; }
        public int[] RankItemID = new int[4];
        public int[] RankItemCount = new int[4];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Rank = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemID[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemID[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemID[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemCount[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemID[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemCount[3] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class CheckenFinalRewardRecord :IRecord
    {
        public static string __TableName = "CheckenFinalReward.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string Rank { get;        set; }
        public int[] RankItemID = new int[4];
        public int[] RankItemCount = new int[4];
        public int RankMail { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Rank = temp[__column++];
                RankItemID[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemCount[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemID[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemCount[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemID[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemCount[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemID[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankItemCount[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                RankMail = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class SuperVipRecord :IRecord
    {
        public static string __TableName = "SuperVip.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int ServerID { get;        set; }
        public string ServerName { get;        set; }
        public int DayRechargeNum { get;        set; }
        public int MonthRechargeNum { get;        set; }
        public string HeadUrl { get;        set; }
        public string QQ { get;        set; }
        public int IsShowIcon { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                ServerID = Table_Tamplet.Convert_Int(temp[__column++]);
                ServerName = temp[__column++];
                DayRechargeNum = Table_Tamplet.Convert_Int(temp[__column++]);
                MonthRechargeNum = Table_Tamplet.Convert_Int(temp[__column++]);
                HeadUrl = temp[__column++];
                QQ = temp[__column++];
                IsShowIcon = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class IosMutiplePlatformRecord :IRecord
    {
        public static string __TableName = "IosMutiplePlatform.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public string channel { get;        set; }
        public string display { get;        set; }
        public int red { get;        set; }
        public int green { get;        set; }
        public int blue { get;        set; }
        public int posX { get;        set; }
        public int posY { get;        set; }
        public int job { get;        set; }
        public string Spid { get;        set; }
        public string BundleID { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                channel = temp[__column++];
                display = temp[__column++];
                red = Table_Tamplet.Convert_Int(temp[__column++]);
                green = Table_Tamplet.Convert_Int(temp[__column++]);
                blue = Table_Tamplet.Convert_Int(temp[__column++]);
                posX = Table_Tamplet.Convert_Int(temp[__column++]);
                posY = Table_Tamplet.Convert_Int(temp[__column++]);
                job = Table_Tamplet.Convert_Int(temp[__column++]);
                Spid = temp[__column++];
                BundleID = temp[__column++];
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class BattleCorrectRecord :IRecord
    {
        public static string __TableName = "BattleCorrect.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public int[] MyFight = new int[59];
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[0] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[1] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[2] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[3] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[4] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[5] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[6] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[7] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[8] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[9] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[10] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[11] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[12] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[13] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[14] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[15] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[16] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[17] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[18] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[19] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[20] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[21] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[22] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[23] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[24] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[25] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[26] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[27] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[28] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[29] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[30] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[31] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[32] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[33] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[34] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[35] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[36] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[37] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[38] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[39] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[40] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[41] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[42] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[43] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[44] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[45] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[46] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[47] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[48] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[49] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[50] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[51] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[52] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[53] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[54] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[55] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[56] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[57] = Table_Tamplet.Convert_Int(temp[__column++]);
                MyFight[58] = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
    public class FashionTitleRecord :IRecord
    {
        public static string __TableName = "FashionTitle.txt";
        private static object __WriteLock = new object();
        public int Id { get;        set; }
        public List<int> ExList = new List<int>();
        public List<int> ValList = new List<int>();
        public int Flag { get;        set; }
        public int Exdata { get;        set; }
        public void __Init__(string[] temp)
        {
            int __column = 0;
            try
            {
               lock (__WriteLock)
               {
                Id = Table_Tamplet.Convert_Int(temp[__column++]);
                Table_Tamplet.Convert_Value(ExList,temp[__column++]);
                Table_Tamplet.Convert_Value(ValList,temp[__column++]);
                Flag = Table_Tamplet.Convert_Int(temp[__column++]);
                Exdata = Table_Tamplet.Convert_Int(temp[__column++]);
               }
            }
            catch (Exception ex)
            {
                string s = string.Format("ERROR:Load table[{0}] id=[{1}] column=[{2}]", __TableName, temp[0], __column);
                Table.Logger.Log(LogLevel.Fatal, ex, s);
                throw;
            }
        }
    }
}
