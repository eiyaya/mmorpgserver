#region using

using System;
using System.Collections.Concurrent;
using System.IO;
using NLog;

#endregion

namespace Shared
{
    public enum BackDataType
    {
        PayData = 1, //收入数据
        NewPlayer = 2, //新增用户
        NewDevice = 3, //新增设备
        NewCharacter = 4, //新增角色
        PlayerOnline = 5, //账号上线数据
        OnLineCount = 6, //在线人数
        VipData = 7, //Vip人数
        CharacterOnline = 8, //角色上线数据
        LevelData = 9 //等级相关
    }

    public enum LogType
    {
        MailConfigError = 260, //邮件配置错误 tbMail Flag is -1
        CharacterManager = 280, //角色管理器
        PlayerMoveTo = 285, //角色管理器
        RefreshGem = 290, //宝石没找到 GetGemGroup not find
        SceneManager = 300, //场景管理器
        UniformDungeon = 301, //UniformDungeon
        Error_Login_AlreadyLogin = 440, //登陆状态：已经登陆了
        Error_PLayerLoginMore = 441, //排队太多了
        Error_Login_NotLogin = 442, //创建完账号后，玩家已离线
        Error_PLayerLoginWait = 447, //玩家需要排队
        Error_PLayerLoginNoWait = 448, //玩家不需要排队
        SendClientQueueSuccess = 449, //排队成功了
        EnterGame = 550, //点击进入游戏时
        PrepareData = 551, //整理数据完成
        EnterGameSuccess = 552, //进入游戏成功
        Connected = 553, //链接其他服务器成功
        Error_NAME_IN_USE = 554, //创建名字失败
        DataBase = 555, //数据库异常
        OnClientLost = 556, //离线
        RemoveScaleModify = 557, //ModifyData.RemoveScaleModify()中的异常
        Test = 558, //Test
        LoginPlayerQueueCountError = 559, //在线玩家统计出错
        SpeMonster = 560, //特殊怪物刷新（地图统领，黄金部队，精英怪）
        QueueMessage = 600, //Queue排队相关
        QueueLog = 601, //QueueLog
        BattleLog = 602, //Battle
        AllianceWar = 603, //攻城战
        AllianceWarError = 604, //攻城战错误
        TeamMessage = 650, //组队信息
        DictionaryPro = 651, //错误的概率随机
        TeamServerMessage = 680, //
        SyncAllianceMessage = 700, //战盟消息
        TimeTest = 800, //timeTest
        SaveLodeData = 980, //战盟占矿存储
        GetLodeData = 981, //战盟占矿读取
        SaveAllianceData = 990, //战盟存储
        GetAllianceData = 991, //战盟读取
        AddRanking = 997, //排行相关
        GetRanking = 998,
        SaveRanking = 999,
        GroupShop = 1000, //许愿树
        GroupShopOldDatasKeySame = 1001,
        ReturnDungeonCostMail = 1002, //退还副本材料的邮件
        TeamEnterDungeonDelItems = 1003, //队伍进副本时，删除材料
        OfflineRecharge = 1004, //离线充值
        LoginLog = 1005, //记录登录流程相关
        PlayerConnect = 1006, //记录PlayerConnect
        LodeSave = 1007, //抢矿相关save
        MysteryStoreSave = 1008,//珍宝商店save
        BlackStoreSave = 1009,//黑市商店
        SaveCheckenData = 1010, //吃鸡数据存储
        GetCheckenData = 1011, //吃鸡数据读取
        PlayerNameError = 2000, //PLayerNameError
        SceneInfo = 3000, //场景
        TestDrop = 9999, //掉落物品相关
        DropItem = 10000, //掉落物品相关
        ClientError = 99999 //客户端报错
    }

    public static class PlayerLog
    {
        private static readonly ConcurrentDictionary<ulong, Logger> mAttrLoggers =
            new ConcurrentDictionary<ulong, Logger>();

        private static readonly ConcurrentDictionary<ulong, Logger> mDataLoggers =
            new ConcurrentDictionary<ulong, Logger>();

        private static readonly ConcurrentDictionary<ulong, Logger> mLoggers = new ConcurrentDictionary<ulong, Logger>();

        private static readonly ConcurrentDictionary<ulong, Logger> mPlayerLoggers =
            new ConcurrentDictionary<ulong, Logger>();

        private static readonly Logger Statistics = LogManager.GetLogger("Statistics");
        private static readonly Logger kafaLogger = LogManager.GetLogger(LoggerName.KafkaLog);

        //写文件  
        public static void AttrLog(ulong playerId, String format, params object[] args)
        {
            GetAttrLogger(playerId).Info(format, args);
        }

        //写文件  
        public static void DataLog(ulong playerId, String format, params object[] args)
        {
            //把Operation发送到Kafka集中处理
            PlayerLog.Kafka("Operation#" + playerId + "#" + string.Format(format, args));
            GetDataLogger(playerId).Info(format, args);
        }

        private static Logger GetAttrLogger(ulong id)
        {
            Logger logger = null;
            mAttrLoggers.AddOrUpdate(id, key =>
            {
                logger = LogManager.GetLogger("Attribute." + id);
                return logger;
            }, (key, oldvalue) =>
            {
                logger = oldvalue;
                return oldvalue;
            });
            return logger;
            //Logger logger;
            //if (mAttrLoggers.TryGetValue(id, out logger))
            //{
            //    return logger;
            //}
            //else
            //{
            //    logger = LogManager.GetLogger("Attribute." + id);
            //    mAttrLoggers.Add(id, logger);
            //    return logger;
            //}
        }

        private static Logger GetDataLogger(ulong id)
        {
            Logger logger = null;
            mDataLoggers.AddOrUpdate(id, key =>
            {
                logger = LogManager.GetLogger("Operation." + id);
                return logger;
            }, (key, oldvalue) =>
            {
                logger = oldvalue;
                return oldvalue;
            });
            return logger;
            //Logger logger;
            //if (mDataLoggers.TryGetValue(id, out logger))
            //{
            //    return logger;
            //}
            //else
            //{
            //    logger = LogManager.GetLogger("Operation." + id);
            //    mDataLoggers.Add(id, logger);
            //    return logger;
            //}
        }

        private static Logger GetLogger(ulong id)
        {
            Logger logger = null;
            mLoggers.AddOrUpdate(id, key =>
            {
                logger = LogManager.GetLogger("Character." + id);
                return logger;
            }, (key, oldvalue) =>
            {
                logger = oldvalue;
                return oldvalue;
            });
            return logger;
        }

        //获得文件路径
        public static string GetPath()
        {
            // 检测并创建Log文件夹
            var strLogDir = Directory.GetCurrentDirectory() + "\\Log";
            if (!Directory.Exists(strLogDir))
            {
                Directory.CreateDirectory(strLogDir);
            }
            // 检测并创建日期文件夹
            var strLogDayDir = strLogDir + "\\" + DateTime.Now.ToString("yyyy-MM-dd");
            if (!Directory.Exists(strLogDayDir))
            {
                Directory.CreateDirectory(strLogDayDir);
            }
            // 检测并创建独立玩家文件夹
            var strLogPlayerDir = strLogDayDir + "\\player";
            if (!Directory.Exists(strLogPlayerDir))
            {
                Directory.CreateDirectory(strLogPlayerDir);
            }
            return strLogPlayerDir;
        }

        private static Logger GetPlayerLogger(ulong id)
        {
            Logger logger = null;
            mPlayerLoggers.AddOrUpdate(id, key =>
            {
                logger = LogManager.GetLogger("Player." + id);
                return logger;
            }, (key, oldvalue) =>
            {
                logger = oldvalue;
                return oldvalue;
            });
            return logger;
        }

        // 初始化
        public static void Init()
        {
            //GetPath();
        }

        //写文件  
        public static void PlayerLogger(ulong playerId, String format, params object[] args)
        {
            GetPlayerLogger(playerId).Info(format, args);
        }

        public static void StatisticsLogger(String format, params object[] args)
        {
            Statistics.Info(format, args);
        }

        //写文件  
        public static void WriteLog(ulong playerId, String format, params object[] args)
        {
            GetLogger(playerId).Info(format, args);
            //try
            //{
            //    FileStream fs = new FileStream(GetPath() + "\\" + playerId.ToString() + ".log", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            //    String content = DateTime.Now.ToString() + ": " + String.Format(format, args) + "\r\n";
            //    var buffer = Encoding.Default.GetBytes(content);
            //    fs.Position = fs.Length;
            //    fs.Write(buffer, 0, buffer.Length);
            //    fs.Flush();
            //    fs.Close();
            //}
            //catch (System.Exception ex)
            //{
            //    Console.WriteLine("玩家日志文件{0}打开/写入失败{1}", GetPath() + "\\" + playerId.ToString() + ".log", ex.ToString());
            //}
        }

        #region 后台统计

        private static Logger GetBackDataLogger(ulong id)
        {
            Logger logger = null;
            mPlayerLoggers.AddOrUpdate(id, key =>
            {
                logger = LogManager.GetLogger("BackData." + id);
                return logger;
            }, (key, oldvalue) =>
            {
                logger = oldvalue;
                return oldvalue;
            });
            return logger;
        }

        //写文件  
        public static void BackDataLogger(ulong playerId, String format, params object[] args)
        {
            GetBackDataLogger(playerId).Info(format, args);
        }

        #endregion

        public static void Kafka(string log, int level = 0)
        {
            if (kafaLogger != null)
            {
                kafaLogger.Info(log);
            }
        }
    }

	public static class LoggerName
	{
		public const string Statistic = "Statistic";
        public const string KafkaLog = "KafkaLog";
	}
}