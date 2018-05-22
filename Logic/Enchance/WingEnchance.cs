#region using

using System;
using System.Collections;
using System.Collections.Generic;
using DataTable;
using NLog;
using Shared;

#endregion

namespace Logic
{
    public interface IStaticWingEnchance
    {
        ErrorCodes WingFormation(CharacterController character, ref int result);
        ErrorCodes WingTrain(CharacterController character, int type, ref int result);
        ErrorCodes WingForceFormation(CharacterController character, int targetWingId);
    }

    public class WingEnchanceDefaultImpl : IStaticWingEnchance
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        [Updateable("WingEnchance")]
        public static int WingQualityMax = Table.GetServerConfig(499).ToInt();
        [Updateable("WingEnchance")]
        static WeightRandom wRandom = new WeightRandom();//加权随机函数
        //翅膀升阶(成长和突破)
        public ErrorCodes WingFormation(CharacterController character, ref int result)
        {
            var wing = character.GetWing();
            if (wing == null)
            {
                return ErrorCodes.Error_WingNotFind;
            }
            var tbWing = Table.GetWingQuality(wing.GetId());
            if (tbWing == null)
            {
                return ErrorCodes.Error_WingID;
            }

            //进阶最大等级
            if (tbWing.Segment == WingQualityMax)
            {
                return ErrorCodes.Error_WingLevelMax;
            }
            //玩家等级
            if (character.GetLevel() < tbWing.LevelLimit)
            {
                return ErrorCodes.Error_LevelNoEnough;
            }

            var needGold = 0;
            var needItemId = -1;
            var needItemCount = 0;
            var isBreak = (wing.GetGrowValue() >= tbWing.GrowProgress);
            if (isBreak)
            { // 突破
                if (tbWing.Segment + 1 >= 4)
                {
                    var item = Table.GetItemBase(wing.GetId()+1);
                    if (null == item)
                    {
                        return ErrorCodes.Error_WingID;
                    }
                    var args = new List<string>
                {
                    Utils.AddCharacter(character.mGuid,character.GetName()),
                    (tbWing.Segment+1).ToString(),
                    item.Name,   
                };
                    var exExdata = new List<int>();
                    character.SendSystemNoticeInfo(291000, args, exExdata);
                }
                
                needItemId = tbWing.BreakNeedItem;
                needItemCount = tbWing.BreakNeedCount;
                needGold = tbWing.BreakNeedMoney;
            }
            else
            { // 成长
                needItemId = tbWing.MaterialNeed;
                needItemCount = tbWing.MaterialCount;
                needGold = tbWing.UsedMoney;
            }
            //材料判断
            if (character.mBag.GetItemCount(needItemId) < needItemCount)
            {
                return ErrorCodes.ItemNotEnough;
            }
            if (character.mBag.GetRes(eResourcesType.GoldRes) < needGold)
            {
                return ErrorCodes.MoneyNotEnough;
            }
            //消耗
            character.mBag.DeleteItem(needItemId, needItemCount, eDeleteItemType.WingFormation);
            character.mBag.DeleteItem((int) eResourcesType.GoldRes, needGold, eDeleteItemType.WingFormation);

            if (isBreak)
            { // 突破
                character.SetRankFlag(RankType.WingsFight);
                wing.SetId(tbWing.Id + 1);
                wing.SetGrowValue(0);
                wing.ClearGrowProperty();
                wing.MarkDirty();
                character.EquipChange(2, (int) eBagType.Wing, 0, wing);
                character.AddExData((int) eExdataDefine.e308, 1);
                result = 1;
            }
            else
            { // 成长属性
                character.SetRankFlag(RankType.WingsFight);
                var curValue = System.Math.Min(wing.GetGrowValue() + tbWing.GrowAddValue, tbWing.GrowProgress);
                wing.SetGrowValue(curValue);
                for (var i = 0; i < tbWing.GrowPropID.Length; ++i)
                {
                    var attrId = tbWing.GrowPropID[i];
                    var minProp = tbWing.GrowMinProp[i];
                    var maxProp = tbWing.GrowMaxProp[i];
                    if (attrId > 0 && minProp > 0 && maxProp > 0)
                    {
                        var prop = MyRandom.Random(minProp, maxProp);
                        wing.SetGrowProperty(attrId, prop);
                    }
                }
                character.EquipChange(2, (int)eBagType.Wing, 0, wing);
                wing.MarkDirty();
                result = 0;
            }

	        try
	        {
				character.mOperActivity.OnWingFormation();
	        }
	        catch (Exception e)
	        {
				Logger.Fatal("OnWingFormation {0}\n{1}",e,e.Message);
		        throw;
	        }

            try
            {
                var klog = string.Format("wingadvance#{0}|{1}|{2}|{3}|{4}|{5}",
                    character.mGuid,
                    character.GetLevel(),
                    character.serverId,
                    tbWing.Segment,
                    isBreak ? (tbWing.Segment + 1) : tbWing.Segment,
                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                PlayerLog.Kafka(klog, 2);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            return ErrorCodes.OK;
        }

        //翅膀培养
        public ErrorCodes WingTrain(CharacterController character, int _type, ref int result)
        {
            int type = 0;//永远都用部位0
            //数据溢出
            if (type < 0 || type >= 5)
            {
                return ErrorCodes.Error_DataOverflow;
            }
            //查询翅膀
            var wing = character.GetWing();
            if (wing == null)
            {
                return ErrorCodes.Error_WingNotFind;
            }
            //表格取值
            var tbWing = Table.GetWingQuality(wing.GetId());
            if (tbWing == null)
            {
                return ErrorCodes.Error_WingID;
            }
            //获取类型ID
            var typeId = wing.GetTypeId(type);
            var tbWingTrain = Table.GetWingTrain(typeId);
            if (tbWingTrain == null)
            {
                Logger.Error("EnchanceFormation Id={0}", typeId);
                return ErrorCodes.Unknow;
            }
            //这个类型的培养是否到头了
            if (tbWingTrain.UpStarID == -1)
            {
                return ErrorCodes.Error_WingTypeLevelMax;
            }
            //需求翅膀阶段等级
            if (tbWingTrain.Condition > tbWing.Segment)
            {
                return ErrorCodes.Error_NeedWingLevelMore;
            }
            //材料判断
            if (character.mBag.GetItemCount(tbWingTrain.MaterialID) < tbWingTrain.MaterialCount)
            {
                return ErrorCodes.ItemNotEnough;
            }
            if (character.mBag.GetRes(eResourcesType.GoldRes) < tbWingTrain.UsedMoney)
            {
                return ErrorCodes.MoneyNotEnough;
            }
            //消耗
            character.mBag.DeleteItem(tbWingTrain.MaterialID, tbWingTrain.MaterialCount, eDeleteItemType.WingTrain);
            character.mBag.DeleteItem((int) eResourcesType.GoldRes, tbWingTrain.UsedMoney, eDeleteItemType.WingTrain);
            //经验计算
            var NowExp = wing.GetExp(type);
            wRandom.Clear();

            wRandom.AddTargets(new WeightRandomItem()
            {
                Index=-1,
                Target = tbWingTrain.AddExp,
                Weight = tbWingTrain.CommonProb
            });
            for (int i = 0; i < tbWingTrain.CritAddExp.Length;i++)
            {
                wRandom.AddTargets(new WeightRandomItem()
                {
                    Index=i,
                    Target = tbWingTrain.CritAddExp[i],
                    Weight = tbWingTrain.CritProb[i]
                });
            }
            wRandom.Ready();
            WeightRandomItem resWR=wRandom.Random();
            NowExp +=Convert.ToInt32(resWR.Target);
            result = resWR.Index + 1;//0.普通，1.暴击1,2.暴击2,3.暴击3
            //if (MyRandom.Random(10000) < tbWingTrain.CritProb)
            //{
            //    NowExp += tbWingTrain.CritAddExp;
            //    result = 1;
            //}
            //else
            //{
            //    NowExp += tbWingTrain.AddExp;
            //    result = 0;
            //}
            //升级计算
            var oldId = tbWingTrain.Id;
            var oldTrainCount = tbWingTrain.TrainCount;
            var newId = -1;
            var levelup = 0;
            while (NowExp >= tbWingTrain.ExpLimit)
            {
                levelup++;
                if (tbWingTrain.UpStarID == -1)
                {
                    NowExp = 0;
                    break;
                }
                NowExp -= tbWingTrain.ExpLimit;
                tbWingTrain = Table.GetWingTrain(tbWingTrain.UpStarID);
                if (tbWingTrain == null)
                {
                    break;
                }
            }
            if (tbWingTrain != null)
                newId = tbWingTrain.Id;
            //计算升级
            if (levelup > 0)
            {
                character.SetRankFlag(RankType.WingsFight);
                wing.SetTypeId(type, tbWingTrain.Id);
                character.EquipChange(2, (int) eBagType.Wing, 0, wing);

                var newTrainCount = tbWingTrain.TrainCount;
                for (int upCount = oldTrainCount; upCount < newTrainCount; upCount++)
                {
                    character.AddExData((int)eExdataDefine.e679, 1);
                }
            }
            character.AddExData((int) eExdataDefine.e326, 1);
            character.AddExData((int) eExdataDefine.e419, 1);
            wing.SetExp(type, NowExp);
            wing.MarkDbDirty();

			try
			{
				character.mOperActivity.OnWingTrainEvent();
			}
			catch (Exception e)
			{
				Logger.Fatal("OnWingFormation {0}\n{1}", e, e.Message);
				throw;
			}

            try
            {
                var klog = string.Format("wingtrain#{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                    character.mGuid,
                    character.GetLevel(),
                    character.serverId,
                    type,
                    oldId,
                    newId,
                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                PlayerLog.Kafka(klog, 2);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            return ErrorCodes.OK;
        }

        public ErrorCodes WingForceFormation(CharacterController character,int targetWingId)
        {
            var wing = character.GetWing();
            if (null == wing)
            {
                return ErrorCodes.Error_WingNotFind;
            }
            var tbWing = Table.GetWingQuality(targetWingId);
            if (null == tbWing)
            {
                return ErrorCodes.Error_WingID;
            }
            var wingSegment = tbWing.Segment;
            character.SetRankFlag(RankType.WingsFight);
            wing.SetId(targetWingId);
            wing.SetGrowValue(0);
            wing.ClearGrowProperty();
            wing.MarkDirty();
            character.EquipChange(2, (int)eBagType.Wing, 0, wing);
            character.SetExData((int) eExdataDefine.e308, wingSegment);
            return ErrorCodes.OK;
        }
    }


    public static class WingEnchance
    {
        private static IStaticWingEnchance mStaticImpl;
        
        static WingEnchance()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (WingEnchance), typeof (WingEnchanceDefaultImpl),
                o => { mStaticImpl = (IStaticWingEnchance) o; });
        }

        //翅膀升阶
        public static ErrorCodes WingFormation(this CharacterController character, ref int result)
        {
            return mStaticImpl.WingFormation(character, ref result);
        }

        //翅膀培养
        public static ErrorCodes WingTrain(this CharacterController character, int type, ref int result)
        {
            return mStaticImpl.WingTrain(character, type, ref result);
        }

        public static ErrorCodes WingForceFormation(this CharacterController character,int targetWingId)
        {
            return mStaticImpl.WingForceFormation(character, targetWingId);
        }
    }

}