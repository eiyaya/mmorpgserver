#region using

using System;
using System.Collections.Generic;
using DataContract;
using DataTable;
using NLog;
using Shared;

#endregion

namespace Logic
{
    public interface IMedalEnchance
    {

        ErrorCodes EnchanceMedal(CharacterController character,
                                 int bagId,
                                 int bagIndex);

        ErrorCodes PickUpMedal(CharacterController character, int bagIndex,int flag);
        ErrorCodes RemoveMedal(CharacterController character, int bagId, int bagIndex, ref int putIndex);
        ErrorCodes UseMedal(CharacterController character, int bagId, int bagIndex, ref int putIndex);
        ErrorCodes SplitMedal(CharacterController character, int bagId,int bagIndex, int flag);
        
    }

    public class MedalEnchanceDefaultImpl : IMedalEnchance
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Logger kafaLogger = LogManager.GetLogger(Shared.LoggerName.KafkaLog);
        //勋章升级
        public ErrorCodes EnchanceMedal(CharacterController character, int bagId, int bagIndex)
        {
            var medal = character.GetItemByBagByIndex(bagId, bagIndex) as MedalItem;
            if (medal == null)
            {
                return ErrorCodes.Error_ItemNotFind;
            }

            var expTotle = character.mBag.GetRes(eResourcesType.HomeExp);
            var tb = Table.GetMedal(medal.GetId());
            if(tb == null)
                return ErrorCodes.Error_ItemNotFind;
            int lv = medal.GetExdata(0);
            if(lv >= tb.MaxLevel)
                return ErrorCodes.Error_EquipLevelMax;

            int curL = character.GetExData((int)eExdataDefine.e621);//无尽幻境历史最高等级
            if (lv >= curL)
                return ErrorCodes.Error_EquipRuneLevelMax;

            var needExp = Table.GetSkillUpgrading(tb.LevelUpExp).GetSkillUpgradingValue(medal.GetExdata(0));
            var exp = medal.GetExdata(1);
            if (expTotle + exp < needExp)
            {
                return ErrorCodes.Error_ExpNotEnough;
            }

            medal.AddExp(needExp - exp);
            character.mBag.DelRes(eResourcesType.HomeExp, needExp - exp, eDeleteItemType.EnchanceMedal);
            if (bagId == (int) eBagType.MedalUsed)
            {
                character.mBag.RefreshMedal();
                character.BooksChange();
            }
            var CurrentLevel = medal.GetExdata(0);
            var MaxLevel = character.GetExData((int)eExdataDefine.e346);
            if (MaxLevel < CurrentLevel)
            {
                character.SetExData((int)eExdataDefine.e346,CurrentLevel);
            }
            if (lv < CurrentLevel)
            {
                try
                {
                    string v = string.Format("FuWenLevelUp_info#{0}|{1}|{2}|{3}|{4}",
                                       character.serverId,
                                       character.mGuid,
                                       character.GetLevel(),
                                       CurrentLevel,
                                       DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")); // 时间
                    PlayerLog.Kafka(v);
                }
                catch (Exception)
                {
                }
            }
            
            
            return ErrorCodes.OK;
        }


        //脱勋章
        public ErrorCodes RemoveMedal(CharacterController character, int bagId, int bagIndex,ref int putIndex)
        {
            var medal = character.GetItemByBagByIndex(bagId, bagIndex) as MedalItem;
            if (medal == null)
            {
                return ErrorCodes.Error_ItemNotFind;
            }
            var bag = character.mBag.GetBag((int) eBagType.MedalBag);
            var freeIndex = bag.GetFirstFreeIndex();
            if (freeIndex == -1)
            {
                return ErrorCodes.Error_BagIndexOverflow;
            }
            var result = character.mBag.MoveItem(bagId, bagIndex, (int) eBagType.MedalBag, freeIndex, 1);
            if (result != ErrorCodes.OK)
            {
                Logger.Error("DropMedal MoveItem Faild! result={0}", result);
                return result;
            }
            character.mBag.RefreshMedal();
            character.BooksChange();
            putIndex = freeIndex;
            return ErrorCodes.OK;
        }

        public ErrorCodes SplitMedal(CharacterController character, int bagId,int bagIndex, int flag)
        {

            bool isHave = false;
            var bag = character.mBag.GetBag(bagId);
            int sumExp = 0;
            if (bagIndex >= 0)
            {
                var item = bag.GetItemByIndex(bagIndex);
                if (item == null)
                {
                    return ErrorCodes.Error_MedalMaterialNotFind;
                }
                if (item.GetId() == -1)
                {
                    return ErrorCodes.Error_MedalMaterialNotFind;
                }
                var medalTemp = item as MedalItem;
                if (medalTemp == null)
                {
                    return ErrorCodes.Error_MedalMaterialNotFind;
                }
                var tbItem = Table.GetMedal(item.GetId());
                if (tbItem == null)
                {
                    return ErrorCodes.Error_MedalID;
                }
                sumExp += medalTemp.GetGiveExp();
                bag.ReduceCountByIndex(medalTemp.GetIndex(), 1, eDeleteItemType.SplitMedal);
                if (bagId == (int) eBagType.MedalUsed)
                {
                    character.mBag.RefreshMedal();
                    character.BooksChange();
                }
            }
            else
            {
                foreach (var itemBase in bag.mLogics)
                {
                    if (itemBase.GetId() == -1)
                    {
                        continue;
                    }
                    var medalTemp = itemBase as MedalItem;
                    if (medalTemp == null)
                    {
                        return ErrorCodes.Error_MedalMaterialNotFind;
                    }
                    var tbItem = Table.GetMedal(itemBase.GetId());
                    if (tbItem == null)
                    {
                        return ErrorCodes.Error_MedalID;
                    }
                    //int paixu = 100000 ;
                    //  0(是否可装备)00(品质)00(等级)00(经验)00(索引)

                    if (0 == (flag & (1 << tbItem.Quality)))
                        continue;
                    isHave = true;
                    sumExp += medalTemp.GetGiveExp() * itemBase.GetCount();
                    bag.ReduceCountByIndex(medalTemp.GetIndex(), itemBase.GetCount(), eDeleteItemType.SplitMedal);
                }
                if (!isHave)
                { 
                    // to show tips ...
                    return ErrorCodes.Error_Runr_Not_Resolve;
                }
            }
            
            character.mBag.AddRes(eResourcesType.HomeExp, sumExp, eCreateItemType.SplitMedal);
            return ErrorCodes.OK;
        }

        //装备勋章
        public ErrorCodes UseMedal(CharacterController character, int bagId, int bagIndex, ref int putIndex)
        {
            if (bagId == (int) eBagType.MedalUsed)
            {
                return character.RemoveMedal(bagId, bagIndex,ref putIndex);
            }
            if (bagId != (int) eBagType.MedalBag)
            {
                return ErrorCodes.Error_BagIndexNoItem;
            }
            var medal = character.GetItemByBagByIndex(bagId, bagIndex) as MedalItem;
            if (medal == null)
            {
                return ErrorCodes.Error_ItemNotFind;
            }
            var bag = character.mBag.GetBag((int) eBagType.MedalUsed);
            //if (useIndex < 0 || useIndex >= bag.GetNowCount())
            //{
            //    return ErrorCodes.Error_DataOverflow;
            //}
            var tbMedal = Table.GetMedal(medal.GetId());
            if (tbMedal == null)
            {
                return ErrorCodes.Error_MedalID;
            }
            if (tbMedal.CanEquipment != 1)
            {
                return ErrorCodes.Error_MedalNotEquip;
            }

            var level = character.mBag.GetRes(eResourcesType.LevelRes);

            //类型检查
            var SameIndex = -1;
            var FreeIndex = -1;
            for (var i = 0; i < bag.GetNowCount(); i++)
            {
                var limit = Table.GetServerConfig(1006 + i).ToInt();
                if (level < limit)
                {
                    break;
                }
                var item = bag.GetItemByIndex(i);
                if (item == null)
                {
                    if (FreeIndex == -1)
                    {
                        FreeIndex = i;
                    }
                    continue;
                }
                if (item.GetId() == -1)
                {
                    if (FreeIndex == -1)
                    {
                        FreeIndex = i;
                    }
                    continue;
                }
                var medalTemp = Table.GetMedal(item.GetId());
                if (medalTemp == null)
                {
                    return ErrorCodes.Error_MedalMaterialNotFind;
                }
                if (medalTemp.MedalType == tbMedal.MedalType)
                {
                    SameIndex = i;
                }
            }
            //移动包裹
            var moveIndex = -1;
            if (SameIndex != -1)
            {
                moveIndex = SameIndex;
            }
            else if (FreeIndex != -1)
            {
                moveIndex = FreeIndex;
            }
            else
            {
                return ErrorCodes.Error_MedalEquipFull;
            }
            //执行
            var result = character.mBag.MoveItem(bagId, bagIndex, (int) eBagType.MedalUsed, moveIndex, 1);
            if (result != ErrorCodes.OK)
            {
                return result;
            }
            character.mBag.RefreshMedal();
            character.BooksChange();
            putIndex = moveIndex;
            return ErrorCodes.OK;
        }

        //拾取勋章
        public ErrorCodes PickUpMedal(CharacterController character, int bagIndex, int flag)
        {
            var bag = character.mBag.GetBag((int) eBagType.MedalTemp);
            if (bagIndex != -1)
            {
                var medal = bag.GetItemByIndex(bagIndex) as MedalItem;
                if (medal == null)
                {
                    return ErrorCodes.Error_ItemNotFind;
                }
                var tbMedal = Table.GetMedal(medal.GetId());
                if (tbMedal == null)
                {
                    return ErrorCodes.Error_MedalID;
                }
                if (tbMedal.MedalType == 12)
                {
                    return ErrorCodes.Error_ItemNoInBag;
                }
                var freeIndex = character.mBag.GetBag((int) eBagType.MedalBag).GetFirstFreeIndex();
                if (freeIndex == -1)
                {
                    return ErrorCodes.Error_ItemNoInBag_All;
                }
                var result = character.mBag.MoveItem((int) eBagType.MedalTemp, bagIndex, (int) eBagType.MedalBag,
                    freeIndex, 1);
                return result;
            }
            ErrorCodes err = ErrorCodes.OK;
            int Exp = 0;
            //一起拾取
            foreach (var itemBase in bag.mLogics)
            {
                if (itemBase.GetId() == -1)
                {
                    continue;
                }
                var medalTemp = itemBase as MedalItem;
                if (medalTemp == null)
                {
                    
                    err = ErrorCodes.Error_MedalMaterialNotFind;
                    break;
                }
                var tbItem = Table.GetMedal(itemBase.GetId());
                if (tbItem == null)
                {
                    err = ErrorCodes.Error_MedalID;
                    break;
                }
                if (0 == (flag & (1 << tbItem.Quality)))
                {
                    var freeIndex = character.mBag.GetBag((int) eBagType.MedalBag).GetFirstFreeIndex();
                    if (freeIndex == -1)
                    {
                        err = ErrorCodes.Error_ItemNoInBag_All;
                        break;
                    }
                    var result = character.mBag.MoveItem((int) eBagType.MedalTemp, medalTemp.GetIndex(),
                        (int) eBagType.MedalBag,
                        freeIndex, 1);
                    if (result != ErrorCodes.OK)
                    {
                        err = result;
                        break;
                    }
                        
                }
                else
                {
                    Exp += medalTemp.GetGiveExp();
                    bag.ReduceCountByIndex(medalTemp.GetIndex(), 1, eDeleteItemType.SplitMedal);
                }

            }
            if (err == ErrorCodes.OK)
            {
                character.mBag.AddRes(eResourcesType.HomeExp, Exp, eCreateItemType.SplitMedal);
            }
            return err;
        }
    }

    public static class MedalEnchance
    {
        private static IMedalEnchance mStaticImpl;

        static MedalEnchance()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (MedalEnchance), typeof (MedalEnchanceDefaultImpl),
                o => { mStaticImpl = (IMedalEnchance) o; });
        }

        //勋章升级
        public static ErrorCodes EnchanceMedal(this CharacterController character,
                                               int bagId,
                                               int bagIndex)
        {
            return mStaticImpl.EnchanceMedal(character, bagId, bagIndex);
        }


        //拾取勋章
        public static ErrorCodes PickUpMedal(this CharacterController character, int bagIndex,int flag)
        {
            return mStaticImpl.PickUpMedal(character, bagIndex,flag);
        }

        //脱勋章
        public static ErrorCodes RemoveMedal(this CharacterController character, int bagId, int bagIndex,ref int putIndex)
        {
            return mStaticImpl.RemoveMedal(character, bagId, bagIndex,ref putIndex);
        }

        //装备勋章
        public static ErrorCodes UseMedal(this CharacterController character, int bagId, int bagIndex, ref int putIndex)
        {
            return mStaticImpl.UseMedal(character, bagId, bagIndex, ref putIndex);
        }
        public static ErrorCodes SplitMedal(this CharacterController character, int bagId,int bagIndex, int flag)
        {
            return mStaticImpl.SplitMedal(character, bagId, bagIndex,flag);
        }
    }
}