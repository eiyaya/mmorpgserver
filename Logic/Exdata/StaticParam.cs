#region using

using System;
using System.Collections.Generic;
using System.Linq;
using DataContract;
using DataTable;
using EventSystem;
using NLog;
using Shared;

#endregion

namespace Logic
{
    public class SkillTitleParam
    {
        public int Count;
        public int Level;
    }

    public class EquipParam
    {
        public int BagId;
        public int BagIdx;
    }

    //RechargeActivity循环数数据
    public class RACircleData
    {
        public int CircleCount;
        public int ExdataId;
        public int ExdataInit;
        public List<int> FlagFalse = new List<int>();
        public int FlagTrue = -1;
    }

    public static class StaticParam
    {
        //攻城战信息
        //serverid => info
        public static Dictionary<int, AllianceWarInfo> AllianceWarInfo = new Dictionary<int, AllianceWarInfo>();
        //拍卖行最低的放置价格
        public static int AuctionMinValue;
        public static int AuctionTime;
        //战场家园经验每次最大次数
        public static int BFCExpMaxCount;
        //战场失败给多少家园经验
        public static int BFCFailExp;
        //战场胜利给多少家园经验
        public static int BFCSuccessExp;
        //许愿池购买幸运签 给予要塞经验
        public static int BuyGroupShopExp;
        public static int CompensationDiamond;
        //补偿系统的 系数
        public static int CompensationExp;
        public static int CompensationGold;
        public static int CompensationGoldRef;
        public static int CumulativeConsumeExdataId;
        public static int CumulativeRechargeDaysExdataId;
        //累计充值相关exdataid
        public static int CumulativeRechargeEverydayExdataId;
        public static int CumulativeRechargeExdataId;
        //连续充值最小额度
        public static int CumulativeRechargeMinDiamonds;
        //装备槽列表
        public static List<EquipParam> EquipList = new List<EquipParam>
        {
            new EquipParam {BagId = (int) eBagType.Equip01, BagIdx = 0},
            new EquipParam {BagId = (int) eBagType.Equip02, BagIdx = 0},
            new EquipParam {BagId = (int) eBagType.Equip05, BagIdx = 0},
            new EquipParam {BagId = (int) eBagType.Equip07, BagIdx = 0},
            new EquipParam {BagId = (int) eBagType.Equip07, BagIdx = 1},
            new EquipParam {BagId = (int) eBagType.Equip08, BagIdx = 0},
            new EquipParam {BagId = (int) eBagType.Equip09, BagIdx = 0},
            new EquipParam {BagId = (int) eBagType.Equip10, BagIdx = 0},
            new EquipParam {BagId = (int) eBagType.Equip11, BagIdx = 0},
            new EquipParam {BagId = (int) eBagType.Equip12, BagIdx = 0}
        };

        //装备强化相关的称号
        public static List<KeyValuePair<int, int>> EquipTitles = new List<KeyValuePair<int, int>>
        {
            new KeyValuePair<int, int>(7, 2428),
            new KeyValuePair<int, int>(9, 2429),
            new KeyValuePair<int, int>(11, 2430),
            new KeyValuePair<int, int>(13, 2431),
            new KeyValuePair<int, int>(15, 2432)
        };

        //交易吆喝给予要塞经验
        public static int ExchangeExp;
        //古战场每天可玩的时间（秒数）
        public static int ExpBattleFieldMaxPlayTimeSec;
        public static int HaploidDia;   // 双倍经验消耗钻石
        //投资相关exdataid
        public static int[] InvestmentLoginExdataId = new int[2];
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static int MaxMailCount = 50; //最大邮件数量
        public static int mEnemyMax;
        //好友系统的人数上限
        public static int mFriendMax;
        public static int mShieldMax;
        //家园宠物复训所需满足的condition id
        public static int PetRetrainConditionId;
        //家园宠物复训所需物品Count
        public static int PetRetrainItemCount;
        //家园宠物复训所需物品id
        public static int PetRetrainItemId;
        //RechargeActivity活动
        //exdata id => count
        public static Dictionary<int, RACircleData> RechargeActivityCircleIds = new Dictionary<int, RACircleData>();
        //RechargeActivityData
        public static RechargeActivityData RechargeActivityData = new RechargeActivityData();
        //RechargeActivityReward flags
        //type => flags
        public static Dictionary<int, List<int>> RechargeActivityInvestReward = new Dictionary<int, List<int>>();
        //充值相关的缓存
        //platform => pay type => price => recharge table id
        public static Dictionary<string, Dictionary<int, Dictionary<int, int>>> RechargeData =
            new Dictionary<string, Dictionary<int, Dictionary<int, int>>>();

        //充值专用的logger
        public static readonly Logger RLogger = LogManager.GetLogger("RechargeLogger");
        //技能相关的称号参数
        public static Dictionary<int, SkillTitleParam> SkillTitleParams = new Dictionary<int, SkillTitleParam>();
        //扫荡券ID
        public static int SweepCouponId;
        //背包钥匙ID
        public static int OpenBagKeyId;
        //修炼点数的计算系数
        public static int TalentPointBeginLevel;
        public static int TalentPointBeginPoint;
        public static int TalentPointEveryLevel;
        //称号相关的Flags, flag id=>NameTitel表ID
        public static Dictionary<int, int> TitleFlags = new Dictionary<int, int>();
        public static int TitlesMaxCount = 5;
        private static Trigger trigger;
        //Key=扩展计数ID   Value=影响的DailyActivityRecord列表
        public static Dictionary<int, List<DailyActivityRecord>> TriggerActivity =
            new Dictionary<int, List<DailyActivityRecord>>();

        //许愿池单抽给的要塞经验
        public static int WishingExp;
        public static int WorshipCoinCost;
        public static int WorshipCoinExpId;
        public static int WorshipCoinHonor;
        public static int WorshipConditionId;
        //膜拜最大次数
        public static int WorshipCountMax;
        public static int WorshipDiamondCost;
        public static int WorshipDiamondExpId;
        public static int WorshipDiamondHonor;

        public static Dictionary<int, MayaBaseRecord> EraMissionDict = new Dictionary<int, MayaBaseRecord>();
        public static Dictionary<int, MayaBaseRecord> EraFubenDict = new Dictionary<int, MayaBaseRecord>();
        public static int MissionIdToOpenBook;

        // 刷新狩猎任务所需物品
        public static int RereshHunterItemId = -1;

        public static int RereshHunterItemCount = -1;

        static StaticParam()
        {
            EventDispatcher.Instance.AddEventListener(ReloadTableEvent.EVENT_TYPE, ReloadTable);
            Init();
        }

        private static void Init()
        {
            ResetServerConfigValues();
            ResetNameTitleValues();
            ResetRechargeValues();
            ResetDailyActivityValues();

            ResetRechargeActiveNotice();
            ResetRechargeActiveCumulative();
            ResetRechargeActiveCumulativeReward();
            ResetRechargeActiveInvestment();
            ResetRechargeActiveInvestmentReward();
            RefreshRechargeActivityData();
            ReLoadEra();

            SkillTitleParams.Add(3100, new SkillTitleParam
            {
                Count = 5,
                Level = 20
            });
            SkillTitleParams.Add(3101, new SkillTitleParam
            {
                Count = 6,
                Level = 35
            });
            SkillTitleParams.Add(3102, new SkillTitleParam
            {
                Count = 7,
                Level = 50
            });
            SkillTitleParams.Add(3103, new SkillTitleParam
            {
                Count = 8,
                Level = 75
            });
        }

        public static void RefreshRechargeActiveData(int serverId)
        {
            var t = new RechargeActiveTable();
            Table.ForeachRechargeActive(r =>
            {
                if (!r.ServerIds.Contains(-1) && !r.ServerIds.Contains(serverId))
                {
                    return true;
                }
                var rr = new RechargeActiveEntry();
                rr.Id = r.Id;
                rr.Type = r.Type;
                rr.SonType = r.SonType;
                rr.LabelText = r.LabelText;
                rr.Icon = r.Icon;
                if (rr.OpenRule == (int) eRechargeActivityOpenRule.NewServerAuto)
                {
                    rr.ServerIds.Add(-1);
                }
                else
                {
                    rr.ServerIds.Add(serverId);
                }
                rr.StartTime = r.StartTime;
                rr.EndTime = r.EndTime;
                rr.ExdataId = r.ExtraId;
                rr.ExtraCount = r.ExtraCount;
                rr.OpenRule = r.OpenRule;
                t.Records.Add(r.Id, rr);
                return true;
            });
            RechargeActivityData.RechargeActiveTable = t;
        }

        private static void RefreshRechargeActivityData()
        {
            trigger = null;

            RechargeActivityInvestReward.Clear();
            Table.ForeachRechargeActiveInvestmentReward(r =>
            {
                List<int> flags;
                if (!RechargeActivityInvestReward.TryGetValue(r.Type, out flags))
                {
                    flags = new List<int>();
                    RechargeActivityInvestReward.Add(r.Type, flags);
                }
                flags.Add(r.Flag);
                return true;
            });

            RechargeActivityCircleIds.Clear();
            Table.ForeachRechargeActiveCumulative(r =>
            {
                var tbRA = Table.GetRechargeActive(r.ActivityId);
                if (tbRA != null && tbRA.ExtraId >= 0 && !RechargeActivityCircleIds.ContainsKey(tbRA.ExtraId))
                {
                    var data = new RACircleData();
                    data.CircleCount = tbRA.ExtraCount;
                    data.ExdataId = r.ExtraId;
                    data.ExdataInit = r.ResetCount;
                    data.FlagTrue = r.FlagTrueId;
                    data.FlagFalse.AddRange(r.FlagFalseId.Where(i => i != -1));
                    RechargeActivityCircleIds.Add(tbRA.ExtraId, data);
                }
                return true;
            });
            Table.ForeachRechargeActiveInvestment(r =>
            {
                var tbRA = Table.GetRechargeActive(r.ActivityId);
                if (tbRA != null && tbRA.ExtraId >= 0 && !RechargeActivityCircleIds.ContainsKey(tbRA.ExtraId))
                {
                    var data = new RACircleData();
                    data.CircleCount = tbRA.ExtraCount;
                    data.ExdataId = r.ExtraId;
                    data.ExdataInit = r.ResetCount;
                    List<int> flags;
                    if (RechargeActivityInvestReward.TryGetValue(r.Id, out flags))
                    {
                        data.FlagFalse.AddRange(flags);
                    }
                    RechargeActivityCircleIds.Add(tbRA.ExtraId, data);
                }
                return true;
            });
        }

        private static void ReloadTable(IEvent ievent)
        {
            var v = ievent as ReloadTableEvent;
            if (v.tableName == "ServerConfig")
            {
                ResetServerConfigValues();
            }
            else if (v.tableName == "NameTitle")
            {
                ResetNameTitleValues();
            }
            else if (v.tableName == "Recharge")
            {
                ResetRechargeValues();
            }
            else if (v.tableName == "DailyActivity")
            {
                ResetDailyActivityValues();
            }
            else if (v.tableName == "RechargeActive")
            {
                if (trigger == null)
                {
                    trigger = LogicServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(5),
                        RefreshRechargeActivityData);
                }
            }
            else if (v.tableName == "RechargeActiveNotice")
            {
                ResetRechargeActiveNotice();
            }
            else if (v.tableName == "RechargeActiveCumulative")
            {
                ResetRechargeActiveCumulative();
                if (trigger == null)
                {
                    trigger = LogicServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(5),
                        RefreshRechargeActivityData);
                }
            }
            else if (v.tableName == "RechargeActiveCumulativeReward")
            {
                ResetRechargeActiveCumulativeReward();
                if (trigger == null)
                {
                    trigger = LogicServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(5),
                        RefreshRechargeActivityData);
                }
            }
            else if (v.tableName == "RechargeActiveInvestment")
            {
                ResetRechargeActiveInvestment();
                if (trigger == null)
                {
                    trigger = LogicServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(5),
                        RefreshRechargeActivityData);
                }
            }
            else if (v.tableName == "RechargeActiveInvestmentReward")
            {
                ResetRechargeActiveInvestmentReward();
                if (trigger == null)
                {
                    trigger = LogicServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(5),
                        RefreshRechargeActivityData);
                }
            }
            else if (v.tableName == "MayaBase")
            {
                ReLoadEra();
            }
        }

        private static void ResetDailyActivityValues()
        {
            //初始化每日活跃度
            TriggerActivity.Clear();
            Table.ForeachDailyActivity(record =>
            {
                if (record.ExDataId == -1)
                {
                    return true;
                }

                List<DailyActivityRecord> temp;
                if (TriggerActivity.TryGetValue(record.ExDataId, out temp))
                {
                    temp.Add(record);
                }
                else
                {
                    temp = new List<DailyActivityRecord> {record};
                    TriggerActivity[record.ExDataId] = temp;
                }
                return true;
            });
        }

        private static void ResetNameTitleValues()
        {
            TitleFlags.Clear();
            Table.ForeachNameTitle(record =>
            {
                if (record.FlagId < 0)
                {
                    return true;
                }

                if (TitleFlags.ContainsKey(record.FlagId))
                {
                    Logger.Error("In Table.ForeachNameTitle. TitleFlags.ContainsKey(record.FlagId) == true!! Id = {0}",
                        record.Id);
                }
                else
                {
                    TitleFlags.Add(record.FlagId, record.Id);
                }
                return true;
            });
        }

        private static void ResetRechargeActiveCumulative()
        {
            // 登陆投资初始化标记位
            InvestmentLoginExdataId[0] = Table.GetRechargeActiveCumulative(2).ExtraId;
            InvestmentLoginExdataId[1] = Table.GetRechargeActiveCumulative(3).ExtraId;

            var t = new RechargeActiveCumulativeTable();
            Table.ForeachRechargeActiveCumulative(r =>
            {
                var rr = new RechargeActiveCumulativeEntry();
                rr.Id = r.Id;
                rr.TypeStr = r.typeStr;
                rr.BuyConditionText = r.BuyConditionText;
                rr.ActivityId = r.ActivityId;
                rr.BgIconId = r.BgIconId;
                rr.ConditionId = r.ConditionId;
                rr.NeedItemId = r.NeedItemId;
                rr.NeedItemCount = r.NeedItemCount;
                rr.ExtraId = r.ExtraId;
                rr.ResetCount = r.ResetCount;
                rr.FlagTrueId = r.FlagTrueId;
                rr.FlagFalseId.AddRange(r.FlagFalseId);
                rr.CanRepeatBuy = r.CanRepeatBuy;
                rr.ChargeID = r.ChargeID;
                t.Records.Add(r.Id, rr);
                return true;
            });
            RechargeActivityData.RechargeActiveCumulativeTable = t;
        }

        private static void ResetRechargeActiveCumulativeReward()
        {
            var t = new RechargeActiveCumulativeRewardTable();
            Table.ForeachRechargeActiveCumulativeReward(r =>
            {
                var rr = new RechargeActiveCumulativeRewardEntry();
                rr.Id = r.Id;
                rr.Type = r.Type;
                rr.Desc1 = r.Desc1;
                rr.Desc2 = r.Desc2;
                rr.ConditionId = r.ConditionId;
                rr.ItemId = r.ItemId;
                rr.ItemCount = r.ItemCount;
                rr.Flag = r.Flag;
                t.Records.Add(r.Id, rr);
                return true;
            });
            RechargeActivityData.RechargeActiveCumulativeRewardTable = t;
        }

        private static void ResetRechargeActiveInvestment()
        {
            CumulativeRechargeEverydayExdataId = Table.GetRechargeActiveInvestment(0).ExtraId;
            CumulativeRechargeExdataId = 561;//Table.GetRechargeActiveInvestment(1).ExtraId;
            CumulativeConsumeExdataId = Table.GetRechargeActiveInvestment(2).ExtraId;
            CumulativeRechargeDaysExdataId = Table.GetRechargeActiveInvestment(3).ExtraId;

            var t = new RechargeActiveInvestmentTable();
            Table.ForeachRechargeActiveInvestment(r =>
            {
                var rr = new RechargeActiveInvestmentEntry();
                rr.Id = r.Id;
                rr.ActivityId = r.ActivityId;
                rr.Type = r.Type;
                rr.Tips = r.Tips;
                rr.ConditionText = r.ConditionText;
                rr.BtnText = r.BtnText;
                rr.GoToUI = r.GoToUI;
                rr.GoToUITab = r.GoToUITab;
                rr.BgIconId = r.BgIconId;
                rr.ExtraId = r.ExtraId;
                rr.ResetCount = r.ResetCount;
                t.Records.Add(r.Id, rr);
                return true;
            });
            RechargeActivityData.RechargeActiveInvestmentTable = t;
        }

        private static void ResetRechargeActiveInvestmentReward()
        {
            var t = new RechargeActiveInvestmentRewardTable();
            Table.ForeachRechargeActiveInvestmentReward(r =>
            {
                var rr = new RechargeActiveInvestmentRewardEntry();
                rr.Id = r.Id;
                rr.Type = r.Type;
                rr.ConditionId = r.ConditionId;
                rr.DiaNeedCount = r.DiaNeedCount;
                rr.ItemId.AddRange(r.ItemId);
                rr.ItemCount.AddRange(r.ItemCount);
                rr.Flag = r.Flag;
                t.Records.Add(r.Id, rr);
                return true;
            });
            RechargeActivityData.RechargeActiveInvestmentRewardTable = t;
        }

        private static void ResetRechargeActiveNotice()
        {
            var t = new RechargeActiveNoticeTable();
            Table.ForeachRechargeActiveNotice(r =>
            {
                var rr = new RechargeActiveNoticeEntry();
                rr.Id = r.Id;
                rr.ActivityId = r.ActivityId;
                rr.Desc = r.Desc;
                rr.IsBtnShow = r.IsBtnShow;
                rr.BtnText = r.BtnText;
                rr.GotoUiId = r.GotoUiId;
                rr.GotoUiTab = r.GotoUiTab;
                rr.ConditionId = r.ConditionId;
                rr.ItemId.AddRange(r.ItemId);
                rr.ItemCount.AddRange(r.ItemCount);
                t.Records.Add(r.Id, rr);
                return true;
            });
            RechargeActivityData.RechargeActiveNoticeTable = t;
        }

        private static void ResetRechargeValues()
        {
            RechargeData.Clear();
            Table.ForeachRecharge(record =>
            {
                Dictionary<int, Dictionary<int, int>> types;
                if (!RechargeData.TryGetValue(record.Platfrom, out types))
                {
                    types = new Dictionary<int, Dictionary<int, int>>();
                    RechargeData.Add(record.Platfrom, types);
                }
                Dictionary<int, int> prices;
                if (!types.TryGetValue(record.Type, out prices))
                {
                    prices = new Dictionary<int, int>();
                    types.Add(record.Type, prices);
                }
                prices.Add(record.Price, record.Id);
                return true;
            });
        }


        public static void ReLoadEra()
        {
            EraMissionDict.Clear();
            EraFubenDict.Clear();
            Table.ForeachMayaBase(record =>
            {
                if (record.ActiveType == (int)EraActiveType.Mission)
                {
                    var missionId = record.ActiveParam[0];
                    if (missionId >= 0)
                    {
                        EraMissionDict[missionId] = record;
                    }

                    if (record.FunBenId >= 0)
                    {
                        EraFubenDict[record.FunBenId] = record;
                    }
                }
                return true;
            });
        }

        private static void ResetServerConfigValues()
        {
            CompensationExp = Table.GetServerConfig(584).ToInt();
            CompensationGold = Table.GetServerConfig(585).ToInt();
            CompensationDiamond = Table.GetServerConfig(586).ToInt();
            CompensationGoldRef = Table.GetServerConfig(587).ToInt();

            TalentPointBeginLevel = Table.GetServerConfig(208).ToInt();
            TalentPointBeginPoint = Table.GetServerConfig(209).ToInt();
            TalentPointEveryLevel = Table.GetServerConfig(210).ToInt();

            SweepCouponId = Table.GetServerConfig(385).ToInt();
            OpenBagKeyId = Table.GetServerConfig(509).ToInt();
            WishingExp = Table.GetServerConfig(312).ToInt();
            BuyGroupShopExp = Table.GetServerConfig(313).ToInt();

            BFCSuccessExp = Table.GetServerConfig(306).ToInt();
            BFCFailExp = Table.GetServerConfig(307).ToInt();
            BFCExpMaxCount = Table.GetServerConfig(308).ToInt();

            PetRetrainConditionId = Table.GetServerConfig(211).ToInt();
            PetRetrainItemId = Table.GetServerConfig(233).ToInt();
            PetRetrainItemCount = Table.GetServerConfig(234).ToInt();
            ExchangeExp = Table.GetServerConfig(303).ToInt();
            ExpBattleFieldMaxPlayTimeSec = Table.GetServerConfig(6).ToInt();

            MaxMailCount = Table.GetServerConfig(284).ToInt();

            mFriendMax = Table.GetServerConfig(320).ToInt();

            mEnemyMax = Table.GetServerConfig(321).ToInt();
            mShieldMax = Table.GetServerConfig(322).ToInt();

            WorshipCountMax = Table.GetServerConfig(390).ToInt();
            WorshipCoinCost = Table.GetServerConfig(391).ToInt();
            WorshipCoinExpId = Table.GetServerConfig(392).ToInt();
            WorshipCoinHonor = Table.GetServerConfig(393).ToInt();
            WorshipDiamondCost = Table.GetServerConfig(394).ToInt();
            WorshipDiamondExpId = Table.GetServerConfig(395).ToInt();
            WorshipDiamondHonor = Table.GetServerConfig(396).ToInt();
            WorshipConditionId = Table.GetServerConfig(397).ToInt();

            AuctionMinValue = Table.GetServerConfig(610).ToInt();
            AuctionTime = Table.GetServerConfig(611).ToInt();
            CumulativeRechargeMinDiamonds = Table.GetServerConfig(1201).ToInt();
            HaploidDia = Table.GetServerConfig(7).ToInt();
            MissionIdToOpenBook = Table.GetServerConfig(110).ToInt();

            var tbServerConfig = Table.GetServerConfig(800);
            if (tbServerConfig != null)
            {
                var paramList = tbServerConfig.Value.Split('|');
                if (paramList.Length > 0)
                    RereshHunterItemId = int.Parse(paramList[0]);
                if (paramList.Length > 1)
                    RereshHunterItemCount = int.Parse(paramList[1]);
            }
        }
    }
}