#region using

using System;
using System.Collections.Generic;
using System.IO;
using DataContract;
using DataTable;
using LZ4;
using NLog;
using ProtoBuf;

#endregion

namespace Shared
{
    public enum eCreateItemType
    {
        None = -1, //未知
        Init = 0, //初始化
        PickUp = 1, //拾取掉落
        Mail = 2, //收邮件
        StoreBuy = 3, //商店购买
        Sell = 4, //出售
        Recycle = 5, //回收
        UseItem = 6, //使用礼包
        Piece = 7, //碎片合成
        Farm = 8, //农场收获
        Hatch = 9, //孵化室查收
        BraveHarbor = 10, //航海查收；
        BlacksmithShop = 11, //铁匠铺合成
        Mine = 12, //矿洞收获
        PetMissionComplete = 13, //随从任务完成
        PetMissionSubmit = 14, //随从任务提交
        RecoveryEquip = 15, //回收装备
        Compose = 16, //道具合成
        Gift = 17, //固定礼包
        Online = 18, //在线时长
        LevelUp = 19, //等级奖励
        ContinueDay = 20, //连续登陆
        Sign = 21, //签到
        ReSign = 22, //补签
        Activity = 23, //活跃奖励
        P1vP1Win = 24, //1v1天梯胜利
        P1vP1Lost = 25, //1v1天梯失败
        Achievement = 26, //成就奖励
        MissionSubmit = 27, //提交任务
        MissionActivate = 28, //任务激活掉落
        ExchangeBuy = 29, //交易所购买
        ExchangeCancel = 30, //交易所取消
        Draw = 31, //抽奖
        DrawPetEgg = 32, //抽宠物蛋
        DrawWishingPool = 33, //许愿池抽奖
        Battle = 34, //战场奖励
        PassFuben = 35, //扫荡副本
        CityMission = 36, //家园任务奖励
        ResolveElf = 37, //精灵分解
        ActivateBook = 38, //激活图鉴
        SuccessDonation = 39, //捐献成功
        Worship = 40, //崇拜奖励
        Casting3 = 41, //进阶装备
        ExchangeHarvest = 42, //交易所收获
        ExchangeSwap = 43, //交易所兑换
        AstrologyDraw = 44, //占星台抽奖
        Fuben = 45, //副本奖励
        Wood = 46, //伐木场
        ArenaTemple = 47, //角斗圣殿
        DrawElf = 48, //精灵抽奖
        ReturnDungeonRequire = 49, //退还副本材料
        TreasureMap = 50, //藏宝图
        PetSoul = 51, //魂魄兑换
        AnswerQuestion = 52, //答题奖励
        Compensation = 53, //领取补偿
        Recharge = 54, //充值
        Vip = 55, //到达Vip级别的奖励
        DailyVipGift = 56, //Vip每日礼包
        MonthCard = 57, //月卡
        SevenDayReward = 58, //七天登录奖励
        CumulativeRecharge = 59, //累计充值奖励
        Investment = 60, //投资奖励
        ShareSuccess = 61, //分享奖励
        WorshipLord = 62, //膜拜城主
        GiftCode = 63, //礼品码
        FirstCharge = 64, // 首冲奖励
		OperationActivity = 65,//运营活动
        PromoteBatteryAward = 66, //提升炮台奖励
        MieShiBossAward = 67, //灭世Boss宝箱奖励
        MieShiPortraitAward = 68, //灭世雕像奖励
        OldPlayer = 69, // 老玩家
        SplitMedal = 70, //分解符文
        Offline = 71, // 离线奖励
        Survey = 72, //问卷
        EraAward = 73,  // 玛雅纪元
        FieldActive = 74, //挖矿活动
        ElfReplaceBuff = 75,    // 精灵技能替换
        GMAdd = 99, //GM
        HiddenRules = 100, //潜规则
        BuyKaiFuTeHui = 101, //开服特惠
        AllianceDepotTakeOut = 102, //战盟仓库取出
        AllianceDepotDonate = 103, //战盟仓库捐赠
        CollectLode = 104,
        LifeCard = 105, //终生卡
        CheckenPick = 106, //吃鸡拾取
        WorshipMonument = 107, //祭拜墓碑
        WeekCard = 108, //周卡
    }

    public enum eDeleteItemType
    {
        None = -1, //未知
        StoreBuy = 0, //商店购买
        ExchangeBroadcast = 1, //交易所购买广播
        ResetSkillTalent = 2, //洗天赋
        BagCellBuy = 3, //买包裹格子
        Casting0 = 4, //铸造
        Casting3 = 5, //进阶装备
        CreateAlliance = 6, //创造战盟
        RefreshCityMission = 7, //刷新家园任务
        ExchangeBuy = 8, //交易所购买
        WingFormation = 9, //翅膀进阶
        Compose = 10, //道具合成
        EnchanceEquip = 11, //装备强化
        WingTrain = 12, //翅膀培养
        ActivateBook = 13, //激活图鉴
        EquipAdditionalEquip = 14, //装备追加
        ReSign = 15, //补签
        UpgradeSkill = 16, //升级技能
        ExcellentEquip = 17, //洗炼装备
        ClearInnate = 18, //重置天赋
        SuperExcellentEquip = 19, //装备洗灵魂属性
        SmritiEquip = 20, //装备传承
        MissionSubmit = 21, //提交任务
        Chat = 22, //聊天消耗
        Piece = 23, //碎片合成
        Plant0 = 24, //农场种植
        Plant2 = 25, //农场施肥
        Hatch0 = 26, //孵化室孵化
        DrawWishingPool = 27, //许愿池抽奖
        FubenCountBuy = 28, //购买副本次数
        CreateBuild = 29, //建造建筑
        UpgradeBuild = 30, //升级建筑
        EnterFuben = 31, //进入副本
        CityMission = 32, //家园任务
        RepairEquip = 33, //修理
        HatchSpeed = 34, //孵化加速
        BraveHarbor = 35, //勇士港加速
        SpeedBuild = 36, //加速建筑
        EnchanceFormation = 37, //阵法强化
        BuyP1vP1Count = 38, //购买天梯次数
        EnchanceElf = 39, //精灵强化
        BuyP1vP1CD = 40, //购买天梯CD
        EnchanceMedal = 41, //勋章升级
        ExchangePush = 42, //放入交易所
        Sell = 43, //出售
        Recycle = 44, //回收
        UseItem = 45, //使用道具
        AstrologyLevelUp = 46, //占星升级
        ResolveElf = 47, //精灵分解
        RecoveryEquip = 48, //回收装备
        ExchangeSwap = 49, //交易所兑换
        ExchangeRefresh = 50, //交易所刷新
        AstrologyDraw = 51, //占星台抽奖
        PetExp = 52, //给随从吃经验
        Reincarnation = 53, //转生消耗
        MultyExp = 54, //经验翻倍消耗
        StatueExp = 55, //竞技场神像经验
        UpgradeHonor = 56, //升级军衔
        DrawElf = 57, //精灵抽奖
        DestroyBuild = 58, //摧毁建筑
        AnswerQuestion = 59, //答题
        Compensation = 60, //领取补偿
        StatueBuyCooling = 61, //竞技场神像冷却
        AllianceBuff = 62, //升级战盟Buff
        Investment = 63, //投资
        WorshipLord = 64, //膜拜城主
        AuctionPush = 65, //放入拍卖行
		OperationActivity = 67,//运营活动消耗
        PromoteBattery = 68, //提升炮台消耗
        InviteChallenge = 69, //邀请决斗消耗
        GMDel = 99, //GM
        CheckAdd             = 100, //尝试添加
        FuHuo                = 101, //复活
        GetLeaveExp          = 102, //获得离线经验
        FuHuoShouWei         = 103, //复活守卫
        ChangeScene          = 104, //切换场景
        Inspire              = 105, //鼓舞
        HornMessage          = 106, //喇叭发言
        EnterFuBen           = 107, //进入副本扣除材料
        ZhanMenJuanXian      = 108, //战盟捐献
        XuYuanShuTuanGou     = 109,  //许愿树团购
        WingBuy              = 110, //翅膀界面购买
        SailingAccess        = 111, //航海直达
        RandEquipSkill       = 112, //随机武器技能
        FeiXie               = 113, //飞鞋
        BuyTiliPet           = 114, //灵兽岛购买体力
        ElfStar              = 115, //灵兽升星
        TowerSweepBuy        = 116, //购买爬塔扫荡次数
        ELfSkillReplace      = 117, //灵兽换技能
        SplitMedal = 118, //分解符文
        PresentGift = 119, //主播送礼
        BuyTiliGuYuZhanChang = 120, //古域战场购买体力
        MountLevelup         = 121, //坐骑升级
        MountSkillUp         = 122, //坐骑技能升级
        MountFeed            = 123, //坐骑喂养
        BuyKaiFuTeHui        = 124, //开服特惠购买物品
        AllianceDepotTakeOut = 125, //战盟仓库取出
        AllianceDepotDonate  = 126, //战盟仓库捐赠
        MountSkinAdd         = 127, //添加坐骑皮肤
        WorshipMonument      = 128, //祭拜墓碑
        ModifyName           = 129, //修改名字
        RefreshHunterMission = 130, //刷新狩猎任务
    }

    public class ItemBase : NodeBase
    {
        public static Logger Logger = LogManager.GetCurrentClassLogger();
        //构造方法
        public ItemBase()
        {
            mDbData = new ItemBaseData {ItemId = -1, Count = 0};
        }

        public ItemBase(int nItemId, ItemBaseData dbData,int count = 1)
        {
            mDbData = dbData;
            mDbData.ItemId = nItemId;
            mDbData.Count = count;
        }

        public ItemBase(ItemBaseData Dbdata, bool IsNull = true)
        {
            mDbData = Dbdata;
            if (IsNull)
            {
                mDbData.ItemId = -1;
                mDbData.Count = 0;
            }
        }

        //private int m_nId;                               //物品ID    //Table
        //private int m_nCount;                            //物品数量
        private int m_nBagId; //包裹ID         
        //private int m_nIndex;                            //包裹索引  //key
        //private List<int> m_Exdata = new List<int>();    //物品扩展数据
        public ItemBaseData mDbData;
        protected bool isTrialEquip = false;
        protected DateTime thisTrialTime = DateTime.Now;


        public override IEnumerable<NodeBase> Children
        {
            get { return null; }
        }

        //增加扩展数据
        public void AddExdata(int nValue)
        {
            mDbData.Exdata.Add(nValue);
        }

        //检查是否有某个扩展数据
        public bool CheckExdata(int nIndex)
        {
            var nNowCount = mDbData.Exdata.Count;
            if (nNowCount <= nIndex)
            {
                return false;
            }
            return true;
        }

        //清空所有扩展数据
        public void CleanExdata()
        {
            mDbData.Exdata.Clear();
        }

        public void CopyFrom(List<int> l)
        {
            mDbData.Exdata.Clear();
            mDbData.Exdata.AddRange(l);
        }

        public void CopyFrom(ItemBase Item)
        {
            SetId(Item.GetId());
            SetCount(Item.GetCount());
            //SetIndex(Item.GetIndex());
            //SetBagId(Item.GetBagId());
            isTrialEquip = Item.isTrialEquip;
            thisTrialTime = Item.thisTrialTime;

            mDbData.Exdata.Clear();
            mDbData.Exdata.AddRange(Item.mDbData.Exdata);
        }

        //扩展数据相关方法
        public void CopyTo(List<int> l)
        {
            l.AddRange(mDbData.Exdata);
        }

        public int GetBagId()
        {
            return m_nBagId;
        }

        public int GetCount()
        {
            return mDbData.Count;
        }

        //获得物品扩展数据
        /// <summary>
        ///     获得物品扩展数据
        /// </summary>
        /// <param name="nIndex">索引</param>
        /// <returns></returns>
        public int GetExdata(int nIndex)
        {
            if (mDbData.Exdata.Count <= nIndex)
            {
                return -1;
            }
            return mDbData.Exdata[nIndex];
        }

        //基础方法
        public int GetId()
        {
            return mDbData.ItemId;
        }

        public int GetIndex()
        {
            return mDbData.Index;
        }

        //获得物品
        public static ItemBaseRecord GetTableItem(int nId)
        {
            var tbitem = Table.GetItemBase(nId);
            if (tbitem == null)
            {
                Logger.Info("ItemId[{0}] not find by Table", nId);
                return null;
            }
            return tbitem;
        }

        //物品封装
        public static string ItemWrap(ItemBaseData item)
        {
            var data = new ChatInfoNodeData();
            data.Type = (int) eChatLinkType.Equip;
            data.Id = item.ItemId;
            var nowItemExdataCount0 = item.Exdata.Count;
            for (var i = 0; i < nowItemExdataCount0; i++)
            {
                data.ExData.Add(item.Exdata[i]);
            }
            var str = "";
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, data);
                var wrap = LZ4Codec.Encode(ms.GetBuffer(), 0, (int) ms.Length);
                str = Convert.ToBase64String(wrap);
            }
            str = SpecialCode.ChatBegin + str + SpecialCode.ChatEnd;
            return str;
        }

        public void SetBagId(int nBagId)
        {
            m_nBagId = nBagId;
        }

        public void SetCount(int nCount)
        {
            mDbData.Count = nCount;
        }

        //设置物品扩展数据
        /// <summary>
        ///     设置物品扩展数据
        /// </summary>
        /// <param name="nIndex">索引</param>
        /// <param name="nValue">设置值</param>
        /// <returns></returns>
        public void SetExdata(int nIndex, int nValue)
        {
            var nNowCount = mDbData.Exdata.Count;
            if (nNowCount <= nIndex)
            {
                Logger.Log(LogLevel.Warn,
                    string.Format("SetExdata ItemId={0},NowCount={1},SetIndex={2},Value={3}", GetId(), nNowCount, nIndex,
                        nValue));
                //要设置的数据位数不足，用-1补足
                for (var i = nNowCount; i <= nIndex; ++i)
                {
                    AddExdata(-1);
                }
            }
            mDbData.Exdata[nIndex] = nValue;
        }

        public void SetId(int nId)
        {
            mDbData.ItemId = nId;
        }

        public void SetIndex(int nIndex)
        {
            mDbData.Index = nIndex;
        }

        public virtual int GetBuffId(int index)
        {
            return -1;
        }
        public virtual int GetBuffLevel(int index)
        {
            return -1;
        }

        public virtual void ReCalcBuff()
        {
            
        }

      
        public int RandBuffId(int buffGroupId = -1)
        {
            if (buffGroupId == -1)
            {
                var tbItem = Table.GetItemBase(GetId());
                if (tbItem == null)
                {
                    return -1;
                }
                var equipId = tbItem.Exdata[0];
                var tbEquip = Table.GetEquip(equipId);
                if (tbEquip != null && tbEquip.BuffGroupId != -1)
                {
                    buffGroupId = tbEquip.BuffGroupId;
                }
                else
                {
                    return -1;
                }
            }

            var tbBuffGroup = Table.GetBuffGroup(buffGroupId);
            if (tbBuffGroup != null)
            {
                if (tbBuffGroup.BuffID.Count != tbBuffGroup.QuanZhong.Count)
                {
                    Logger.Error("RandBuffId equipId {0} addbuffgroup {1} size not eaqual!", GetId(), buffGroupId);
                    return -1;
                }

                var curBuffId = GetBuffId(0);
                var buffList = new List<int>();
                var propList = new List<int>();
                var index = 0;
                var max = 0;
                foreach (var buffId in tbBuffGroup.BuffID)
                {
                    //if (curBuffId != buffId)
                    {
                        max += tbBuffGroup.QuanZhong[index];
                        buffList.Add(buffId);
                        propList.Add(tbBuffGroup.QuanZhong[index]);
                    }
                    ++index;
                }

                index = 0;
                var rnd = MyRandom.Random(0, max - 1);
                foreach (var prop in propList)
                {
                    if (rnd < prop)
                    {
                        return buffList[index];
                    }
                    rnd -= prop;
                    ++index;
                }
            }

            return -1;
        }

        public virtual bool TrialTimeCost()
        {
            return false;
        }

        public virtual bool IsTrialEnd()
        {
            return false;
        }
    }
}