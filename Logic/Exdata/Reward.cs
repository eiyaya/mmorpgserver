#region using

using System;
using System.Collections.Generic;
using DataTable;
using NLog;
using Shared;

#endregion

namespace Logic
{
    public interface IReward
    {
        ErrorCodes CheckGift(CharacterController character, eActivationRewardType type, int giftId);
        ArenaRewardRecord GetArenaReward(int id);
        int GetMaxLadder(int oldRank, int newRank);
        ErrorCodes Gift(CharacterController character, eActivationRewardType type, int giftId);
        ErrorCodes GiveGift(CharacterController character, eActivationRewardType type, int giftId);
        void GiveP1vP1Lost(CharacterController character, int rank, Dictionary<int, int> items, int level = 0);
        void GiveP1vP1MaxLadder(CharacterController character, int rank);
        void GiveP1vP1Win(CharacterController character, int rank, Dictionary<int, int> items, int level = 0);
    }

    public class RewardDefaultImpl : IReward
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ErrorCodes Gift(CharacterController character, eActivationRewardType type, int giftId)
        {
            var result = CheckGift(character, type, giftId);
            if (result != ErrorCodes.OK)
            {
                return result;
            }
            return GiveGift(character, type, giftId);
        }

        //检查奖励
        public ErrorCodes CheckGift(CharacterController character, eActivationRewardType type, int giftId)
        {
            switch (type)
            {
                case eActivationRewardType.TableGift:
                {
                    var tbGift = Table.GetGift(giftId);
                    if (tbGift == null)
                    {
                        return ErrorCodes.Error_GiftID;
                    }
                    if (character.GetFlag(tbGift.Flag))
                    {
                        return ErrorCodes.Error_GiftAlreadyReceive;
                    }

                    switch ((eRewardType) tbGift.Type)
                    {
                        //固定礼包
                        case eRewardType.GiftBag:
                        {
                            var items = new Dictionary<int, int>();
                            for (var i = 0; i != 4; ++i)
                            {
                                if (tbGift.Param[i*2] != -1)
                                {
                                    items.modifyValue(tbGift.Param[i*2], tbGift.Param[i*2 + 1]);
                                }
                            }
                            return BagManager.CheckAddItemList(character.mBag, items);
                        }
                        //在线时长
                        case eRewardType.OnlineReward:
                        {
                            var times = character.TodayTimes + DateTime.Now.GetDiffSeconds(character.OnlineTime);
                            if (times >= tbGift.Param[0])
                            {
                                return character.mBag.CheckAddItem(tbGift.Param[1], tbGift.Param[2]);
                            }
                            return ErrorCodes.Error_GiftTimeNotEnough;
                        }
                        //等级奖励
                        case eRewardType.LevelReward:
                        {
                            if (character.GetLevel() >= tbGift.Param[0])
                            {
                                var items = new Dictionary<int, int>();
                                for (var i = 1; i <=5; ++i)
                                {
                                    var count = tbGift.Param[i + 5];
                                    if (tbGift.Param[i] != -1)
                                    {
                                        items.modifyValue(tbGift.Param[i], count);
                                    }
                                }
                                return BagManager.CheckAddItemList(character.mBag, items);
                            }
                            return ErrorCodes.Error_LevelNoEnough;
                        }
                        //连续登陆
                        case eRewardType.ContinuesLoginReward:
                        {
                            var days = character.GetExData(17);
                            if (days < 1)
                            {
                                return ErrorCodes.Error_GiftTimeNotEnough;
                            }
                            if (days > 5)
                            {
                                days = 5;
                            }
                            var items = new Dictionary<int, int>();
                            for (var i = 0; i < days; ++i)
                            {
                                var tbGift2 = Table.GetGift(13 + i);
                                if (character.GetFlag(tbGift.Flag))
                                {
                                    continue;
                                }
                                items.modifyValue(tbGift2.Param[1], tbGift2.Param[2]);
                            }
                            return BagManager.CheckAddItemList(character.mBag, items);
                        }
                        //累计登陆
                        case eRewardType.MonthCheckinReward:
                        {
                            var months = DateTime.Now.Month;
                            if (tbGift.Param[0] != 999 && months != tbGift.Param[0])
                            {
                                return ErrorCodes.Error_GiftTimeNotEnough;
                            }
                            var days = DateTime.Now.Day;
                            var exdataValue = character.GetExData(tbGift.Exdata);
                            if (exdataValue + 1 != tbGift.Param[1])
                            {
                                return ErrorCodes.Unknow;
                            }
                            if (days >= tbGift.Param[1])
                            {
                                if (character.GetFlag(466))
                                {
//补签
                                    var SupplementCount = character.GetExData(18);
                                    var needGold =
                                        (int)
                                            (tbGift.Param[4]*
                                             SkillExtension.Pow(tbGift.Param[5]/10000.0f, SupplementCount));
                                    needGold = needGold - needGold%5;
                                    if (character.mBag.GetRes(eResourcesType.DiamondRes) < needGold)
                                    {
                                        return ErrorCodes.MoneyNotEnough;
                                    }
                                    return character.mBag.CheckAddItem(tbGift.Param[2], tbGift.Param[3]);
                                }
                                //签到
                                return character.mBag.CheckAddItem(tbGift.Param[2], tbGift.Param[3]);
                            }
                            return ErrorCodes.Error_GiftTimeNotEnough;
                        }
                        //每日活跃度
                        case eRewardType.DailyActivity:
                        {
                            if (character.GetExData(tbGift.Exdata) >= 0)
                            {
                                if (character.GetExData(tbGift.Exdata) < tbGift.Param[1])
                                {
                                    return ErrorCodes.Error_GiftCountNotEnough;
                                }
                            }
                        }
                            break;
                        //每日活跃奖励
                        case eRewardType.DailyActivityReward:
                        {
                            if (character.GetExData(15) >= tbGift.Param[2])
                            {
                                return character.mBag.CheckAddItem(tbGift.Param[0], tbGift.Param[1]);
                            }
                            return ErrorCodes.Error_ActivityPointNotEnough;
                        }
                        //七天登录奖励
                        case eRewardType.SevenDayReward:
                        {
                            if (character.GetExData((int) eExdataDefine.e94) < tbGift.Param[0])
                            {
                                return ErrorCodes.Error_GiftTimeNotEnough;
                            }
                            if (character.GetFlag(tbGift.Flag))
                            {
                                return ErrorCodes.Error_GiftAlreadyReceive;
                            }
                            var items = new Dictionary<int, int>();
                            for (var i = 0; i != 3; i++)
                            {
                                items.modifyValue(tbGift.Param[i*2 + 1], tbGift.Param[i*2 + 2]);
                            }
                            items.modifyValue(tbGift.Param[7], tbGift.Param[8]);

                            return BagManager.CheckAddItemList(character.mBag, items);
                        }
                        default:
                            Logger.Warn("Gift[{0}] type is overflow", giftId);
                            break;
                    }
                }
                    break;
                case eActivationRewardType.DailyVipGift:
                {
                    var vipLevel = character.mBag.GetRes(eResourcesType.VipLevel);
                    var tbVip = Table.GetVIP(vipLevel);

                    if (tbVip == null)
                    {
                        return ErrorCodes.Error_NoVipGift;
                    }
                    Dictionary<int,int> dic = new Dictionary<int, int>();
                    Utils.GetVipReward(vipLevel,ref dic);
                    if (dic.Count == 0)
                    {
                        return ErrorCodes.Error_NoVipGift;
                    }
                    var tbDA = Table.GetDailyActivity(giftId);
                    if (character.GetFlag(tbDA.CommonParam[0]))
                    {
                        return ErrorCodes.Error_VipGiftGained;
                    }
                    return BagManager.CheckAddItemList(character.mBag, dic);
                }
                case eActivationRewardType.MonthCard:
                {
                    var now = DateTime.Now;
                    var date = character.lExdata64.GetTime(Exdata64TimeType.MonthCardExpirationDate);
                    if (date < now)
                    {
                        return ErrorCodes.Error_NoMonthCard;
                    }
                    var tbDA = Table.GetDailyActivity(giftId);
                    var flag = tbDA.CommonParam[0];
                    if (character.GetFlag(flag))
                    {
                        return ErrorCodes.Error_MonthCardGained;
                    }
                }
                    break;
                case eActivationRewardType.LifeCard:
                {
                    if (!character.GetFlag(2682))//终生卡标记
                    {
                        return ErrorCodes.Error_NoLifeCard;
                    }
                    var tbRecharge = Table.GetRecharge(giftId);
                    if (null == tbRecharge)
                    {
                        return ErrorCodes.Error_GiftID;
                    }
                    var flagId = tbRecharge.Param[2];
                    if (character.GetFlag(flagId))
                    {
                        return ErrorCodes.Error_LifeCardGained;
                    }
                }
                    break;
                case eActivationRewardType.WeekCard:
                    {
                        var now = DateTime.Now;
                        var date = character.lExdata64.GetTime(Exdata64TimeType.WeekCardExpirationDate);
                        if (date < now)
                        {
                            return ErrorCodes.Error_NoWeekCard;
                        }
                        var tbDA = Table.GetDailyActivity(giftId);
                        var flag = tbDA.CommonParam[0];
                        if (character.GetFlag(flag))
                        {
                            return ErrorCodes.Error_WeekCardGained;
                        }
                    }
                    break;
            }
            return ErrorCodes.OK;
        }

        //给予奖励
        public ErrorCodes GiveGift(CharacterController character, eActivationRewardType type, int giftId)
        {
            var result = ErrorCodes.OK;
            switch (type)
            {
                case eActivationRewardType.TableGift:
                {
                    var tbGift = Table.GetGift(giftId);
                    if (tbGift == null)
                    {
                        return ErrorCodes.Error_GiftID;
                    }
                    if (character.GetFlag(tbGift.Flag))
                    {
                        return ErrorCodes.Error_GiftAlreadyReceive;
                    }

                    //执行领取
                    switch ((eRewardType) tbGift.Type)
                    {
                        //固定礼包
                        case eRewardType.GiftBag:
                        {
                            //Dictionary<int, int> items = new Dictionary<int, int>();
                            for (var i = 0; i != 4; ++i)
                            {
                                if (tbGift.Param[i*2] != -1)
                                {
                                    character.mBag.AddItem(tbGift.Param[i*2], tbGift.Param[i*2 + 1],
                                        eCreateItemType.Gift);
                                }
                            }
                        }
                            break;
                        //在线时长
                        case eRewardType.OnlineReward:
                        {
                            result = character.mBag.AddItem(tbGift.Param[1], tbGift.Param[2], eCreateItemType.Online);
                        }
                            break;
                        //等级奖励
                        case eRewardType.LevelReward:
                        {
                            for (var i = 1; i <= 5; ++i)
                            {
                                var count = tbGift.Param[i + 5];
                                if (tbGift.Param[i] != -1)
                                {
                                    character.mBag.AddItem(tbGift.Param[i], count, eCreateItemType.LevelUp);
                                }
                            }
                        }
                            break;
                        //连续登陆
                        case eRewardType.ContinuesLoginReward:
                        {
                            //result = character.mBag.AddItem(tbGift.Param[1], tbGift.Param[2]);

                            var days = character.GetExData(17);
                            if (days > 5)
                            {
                                days = 5;
                            }
                            for (var i = 0; i < days; ++i)
                            {
                                var tbGift2 = Table.GetGift(13 + i);
                                if (character.GetFlag(tbGift2.Flag))
                                {
                                    continue;
                                }
                                character.mBag.AddItem(tbGift2.Param[1], tbGift2.Param[2], eCreateItemType.ContinueDay);
                                character.SetFlag(tbGift2.Flag);
                            }
                            return ErrorCodes.OK;
                        }
                        //签到
                        case eRewardType.MonthCheckinReward:
                        {
                            if (character.GetFlag(466))
                            {
//补签
                                var SupplementCount = character.GetExData(18);
                                var needGold =
                                    (int)
                                        (tbGift.Param[4]*SkillExtension.Pow(tbGift.Param[5]/10000.0f, SupplementCount));
                                needGold = needGold - needGold%5;
                                character.mBag.DeleteItem((int) eResourcesType.DiamondRes, needGold,
                                    eDeleteItemType.ReSign);
                                character.SetExData(18, SupplementCount + 1);
                                character.AddExData(tbGift.Exdata, 1);
                                result = character.mBag.AddItem(tbGift.Param[2], tbGift.Param[3], eCreateItemType.ReSign);
                            }
                            else
                            {
//签到
                                character.SetFlag(466);
                                character.AddExData(tbGift.Exdata, 1);
                                result = character.mBag.AddItem(tbGift.Param[2], tbGift.Param[3], eCreateItemType.Sign);
                            }
                        }
                            break;
                        ////每日活跃度
                        //case eRewardType.DailyActivity:
                        //    {
                        //        character.AddExData(tbGift.Param[3], tbGift.Param[0]);
                        //    }
                        //    break;
                        //每日活跃奖励
                        case eRewardType.DailyActivityReward:
                        {
                            result = character.mBag.AddItem(tbGift.Param[0], tbGift.Param[1], eCreateItemType.Activity);
                        }
                            break;
                        //七天登录奖励
                        case eRewardType.SevenDayReward:
                        {
                            for (var i = 0; i != 3; i++)
                            {
                                character.mBag.AddItem(tbGift.Param[i*2 + 1], tbGift.Param[i*2 + 2],
                                    eCreateItemType.SevenDayReward);
                            }
                            character.mBag.AddItem(tbGift.Param[7], tbGift.Param[8], eCreateItemType.SevenDayReward);
                            character.SetFlag(tbGift.Flag);
                        }
                            break;
                        default:
                            Logger.Warn("Gift[{0}] type is overflow", giftId);
                            break;
                    }
                    if (result == ErrorCodes.OK)
                    {
                        character.SetFlag(tbGift.Flag);
                    }
                }
                    break;
                case eActivationRewardType.DailyVipGift:
                {
                    var vipLevel = character.mBag.GetRes(eResourcesType.VipLevel);
                    var tbVip = Table.GetVIP(vipLevel);
                    Dictionary<int,int> dic = new Dictionary<int, int>();
                    Utils.GetVipReward(vipLevel,ref dic);



                    if (dic.Count == 0)
                    {
                        return ErrorCodes.Error_NoVipGift;
                    }
                    var tbDA = Table.GetDailyActivity(giftId);
                    var flag = tbDA.CommonParam[0];
                    if (character.GetFlag(flag))
                    {
                        return ErrorCodes.Error_VipGiftGained;
                    }
                    result = BagManager.CheckAddItemList(character.mBag, dic);
                    if (result != ErrorCodes.OK)
                    {
                        return result;
                    }
                    character.SetFlag(flag);
                    
                    result = character.mBag.AddItems(dic, eCreateItemType.DailyVipGift);
                }
                    break;
                case eActivationRewardType.MonthCard:
                {
                    var diaCount = Table.GetServerConfig(419).ToInt();
                    result = character.mBag.AddRes(eResourcesType.DiamondRes, diaCount, eCreateItemType.MonthCard);
                    var tbDA = Table.GetDailyActivity(giftId);
                    var flag = tbDA.CommonParam[0];
                    if (character.GetFlag(flag))
                    {
                        return ErrorCodes.Error_MonthCardGained;
                    }
                    character.SetFlag(flag);
                    character.SetFlag(tbDA.CommonParam[0]);
                    character.AddExData((int)eExdataDefine.e778, 1);
                }
                    break;
                case eActivationRewardType.WeekCard:
                    {
                        var tbRecharge = Table.GetRecharge(43);
                        var diaCount = tbRecharge.Param[0];//策划配置到此列
                        result = character.mBag.AddRes(eResourcesType.DiamondRes, diaCount, eCreateItemType.WeekCard);
                        var tbDA = Table.GetDailyActivity(giftId);
                        var flag = tbDA.CommonParam[0];
                        if (character.GetFlag(flag))
                        {
                            return ErrorCodes.Error_WeekCardGained;
                        }
                        character.SetFlag(flag);
                        character.AddExData((int)eExdataDefine.e779, 1);
                    }
                    break;
                case eActivationRewardType.LifeCard:
                {
                    var tbRecharge = Table.GetRecharge(giftId);
                    if (null == tbRecharge)
                    {
                        return ErrorCodes.Error_GiftID;
                    }
                    var flagId = tbRecharge.Param[2];
                    if (character.GetFlag(flagId))
                    {
                        return ErrorCodes.Error_LifeCardGained;
                    }
                    var diaCount = tbRecharge.Param[0];
                    result = character.mBag.AddRes(eResourcesType.DiamondRes, diaCount, eCreateItemType.LifeCard);
                    if (result == ErrorCodes.OK)
                    {
                        character.SetFlag(flagId);
                    }
                    else
                    {
                        return result;
                    }
                }
                    break;
            }
            return result;
        }

        //给予P1vP1胜利奖励
        public void GiveP1vP1Win(CharacterController character, int rank, Dictionary<int, int> items, int level = 0)
        {
            if (character != null)
            {
                level = character.GetLevel();
            }
            ArenaLevelRecord tbAl = null;
            Table.ForeachArenaLevel(record =>
            {
                if (level <= record.Id)
                {
                    tbAl = record;
                    return false;
                }
                return true;
            });
            if (tbAl == null)
            {
                Logger.Error("GiveP1vP1Win not find ArenaLevel rank={0}", rank);
                return;
            }
            if (character != null)
            {
                character.mBag.AddExp(tbAl.SuccessExp, eCreateItemType.P1vP1Win);
                character.mBag.AddRes(eResourcesType.GoldRes, tbAl.SuccessMoney, eCreateItemType.P1vP1Win);
                character.mBag.AddItem(tbAl.SuccessItemID, tbAl.SuccessCount, eCreateItemType.P1vP1Win);
                character.AddExData((int) eExdataDefine.e49, 1);
                //character.mCity.CityAddExp(tbAl.SuccessGetExp);
            }
            items[(int) eResourcesType.ExpRes] = tbAl.SuccessExp;
            items[(int) eResourcesType.GoldRes] = tbAl.SuccessMoney;
            //items[(int) eResourcesType.HomeExp] = tbAl.SuccessGetExp;
            items[tbAl.SuccessItemID] = tbAl.SuccessCount;
        }

        //计算最高奖励的量
        public int GetMaxLadder(int oldRank, int newRank)
        {
            if (oldRank <= newRank)
            {
                return 0;
            }
            var giveCount = 0;
            var firstFindNew = false;
            ArenaRewardRecord last = null;
            Table.ForeachArenaReward(record =>
            {
                if (firstFindNew)
                {
                    if (oldRank <= record.Id)
                    {
                        giveCount = giveCount + (oldRank - last.Id - 1)*record.MaxDiamond;
                        return false;
                    }
                    giveCount = giveCount + (record.Id - last.Id)*record.MaxDiamond;
                    last = record;
                }
                else if (newRank <= record.Id)
                {
                    firstFindNew = true;
                    if (oldRank <= record.Id)
                    {
                        giveCount = giveCount + (oldRank - newRank)*record.MaxDiamond;
                        return false;
                    }
                    giveCount = giveCount + (record.Id - newRank + 1)*record.MaxDiamond;
                    last = record;
                }
                return true;
            });
            return giveCount;
        }

        //计算天梯名次
        public ArenaRewardRecord GetArenaReward(int id)
        {
            ArenaRewardRecord tbAr = null;
            Table.ForeachArenaReward(record =>
            {
                if (id <= record.Id)
                {
                    tbAr = record;
                    return false;
                }
                return true;
            });
            return tbAr;
        }

        //给予P1vP1失败奖励
        public void GiveP1vP1Lost(CharacterController character, int rank, Dictionary<int, int> items, int level = 0)
        {
            if (character != null)
            {
                level = character.GetLevel();
            }
            ArenaLevelRecord tbAl = null;
            Table.ForeachArenaLevel(record =>
            {
                if (level <= record.Id)
                {
                    tbAl = record;
                    return false;
                }
                return true;
            });
            if (tbAl == null)
            {
                Logger.Error("GiveP1vP1Lost not find ArenaLevel rank={0}", rank);
                return;
            }
            if (character != null)
            {
                character.mBag.AddExp(tbAl.FailedExp, eCreateItemType.P1vP1Lost);
                character.mBag.AddRes(eResourcesType.GoldRes, tbAl.FailedMoney, eCreateItemType.P1vP1Lost);
                character.mBag.AddItem(tbAl.FailedItemID, tbAl.FailedCount, eCreateItemType.P1vP1Lost);
                //character.mCity.CityAddExp(tbAl.FailedGetExp);
            }
            items[(int) eResourcesType.ExpRes] = tbAl.FailedExp;
            items[(int) eResourcesType.GoldRes] = tbAl.FailedMoney;
            //items[(int) eResourcesType.HomeExp] = tbAl.FailedGetExp;
            items[tbAl.FailedItemID] = tbAl.FailedCount;
        }

        //给予P1vP1最高名次奖励
        public void GiveP1vP1MaxLadder(CharacterController character, int rank)
        {
        }
    }

    public static class Reward
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static IReward mImpl;

        static Reward()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (Reward), typeof (RewardDefaultImpl),
                o => { mImpl = (IReward) o; });
        }

        //计算天梯名次
        public static ArenaRewardRecord GetArenaReward(int id)
        {
            return mImpl.GetArenaReward(id);
        }

        //计算最高奖励的量
        public static int GetMaxLadder(int oldRank, int newRank)
        {
            return mImpl.GetMaxLadder(oldRank, newRank);
        }

        public static ErrorCodes Gift(this CharacterController character, eActivationRewardType type, int giftId)
        {
            return mImpl.Gift(character, type, giftId);
        }

        //给予P1vP1失败奖励
        public static void GiveP1vP1Lost(this CharacterController character,
                                         int rank,
                                         Dictionary<int, int> items,
                                         int level = 0)
        {
            mImpl.GiveP1vP1Lost(character, rank, items, level);
        }

        //给予P1vP1最高名次奖励
        public static void GiveP1vP1MaxLadder(this CharacterController character, int rank)
        {
            mImpl.GiveP1vP1MaxLadder(character, rank);
        }

        //给予P1vP1胜利奖励
        public static void GiveP1vP1Win(this CharacterController character,
                                        int rank,
                                        Dictionary<int, int> items,
                                        int level = 0)
        {
            mImpl.GiveP1vP1Win(character, rank, items, level);
        }
    }
}