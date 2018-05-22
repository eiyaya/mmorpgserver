
public static class Constants
{
    public const int ServerCount = 20;
    public const int LevelMax = 400;
    public const int VipMax = 12;

    public const int AllianceWarSceneId = 9000;
    public const int AllianceWarDungeonId = 9000;

    public const long RankLevelFactor = 4000000000;
    
    public const int AllianceMaxPlayer = 50;
}

//活动副本相关常量
public static class ActivityDungeonConstants
{
    //血色恶魔奖励存档相关
    public const int DungeonTimeStartIdx = 0;
    public const int DungeonTimeRange = 14;
    public const uint DungeonTimeMask = 0x3fffu;

    public const int CompleteTypeStartIdx = DungeonTimeStartIdx + DungeonTimeRange;
    public const int CompleteTypeRange = 2;
    public const uint CompleteTypeMask = 0x3u;

    public const int PlayerLevelStartIdx = CompleteTypeStartIdx + CompleteTypeRange;
    public const int PlayerLevelRange = 12;
    public const uint PlayerLevelMask = 0xfffu;
    public const int MaxExpTimes = 2;   // 多倍经验倍数
}

//GM服务器相关指令
public static class GmCommands
{
    //血色恶魔奖励存档相关
    public const string NewGiftCodes = "NewGiftCodes";
}
