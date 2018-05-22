#region using

using System.Collections.Generic;
using DataTable;
using Shared;

#endregion

namespace Logic.Enchance
{
    public interface IStaticAstrologyEnchance
    {
        ErrorCodes AstrologyEquipOff(CharacterController character, int astrologyId, int Index);
        ErrorCodes AstrologyEquipOn(CharacterController character, int bagIndex, int astrologyId, int Index);
        ErrorCodes AstrologyLevelUp(CharacterController character, int bagId, int bagIndex, List<int> needList);
    }


    public class AstrologyEnchanceDefaultImpl : IStaticAstrologyEnchance
    {
        public ErrorCodes AstrologyLevelUp(CharacterController character, int bagId, int bagIndex, List<int> needList)
        {
            var bag = character.mBag.GetBag(bagId);
            if (bag == null)
            {
                return ErrorCodes.Error_BagID;
            }
            var item = bag.GetItemByIndex(bagIndex);
            if (item == null || item.GetId() == -1)
            {
                return ErrorCodes.Error_ItemNotFind;
            }
            var tbGem = Table.GetGem(item.GetId());
            if (tbGem == null)
            {
                return ErrorCodes.Error_GemID;
            }
            //升级操作
            var oldLevel = item.GetExdata(0);
            var oldExp = item.GetExdata(1);
            var tbLevel = Table.GetLevelData(oldLevel);
            var color = tbGem.Quality;
            var needExp = tbLevel.Exp[color];
            if (needExp == -1)
            {
                return ErrorCodes.Error_GemLevelMax;
            }
            var totleExp = 0;
            //材料检查
            var needBag = bag;
            if (bagId == (int) eBagType.GemBag)
            {
                if (needList.Contains(bagIndex))
                {
                    return ErrorCodes.Error_NotSelectUpgrade;
                }
            }
            else
            {
                needBag = character.mBag.GetBag((int) eBagType.GemBag);
            }
            foreach (var i in needList)
            {
                var needItem = needBag.GetItemByIndex(i);
                if (needItem == null || needItem.GetId() == -1)
                {
                    return ErrorCodes.Error_ItemNotFind;
                }
                var tbGemNeed = Table.GetGem(needItem.GetId());
                if (tbGemNeed == null)
                {
                    continue;
                }
                var needLevel = needItem.GetExdata(0);
                var Exp = needItem.GetExdata(1) + tbGemNeed.InitExp;
                for (var l = 1; l < needLevel; ++l)
                {
                    Exp += Table.GetLevelData(l).Exp[tbGemNeed.Quality];
                }
                totleExp += Exp;
            }
            //升级操作
            oldExp = oldExp + totleExp;
            var newLevel = oldLevel;
            var newExp = oldExp;
            while (newExp >= needExp)
            {
                newLevel++;
                newExp -= needExp;
                tbLevel = Table.GetLevelData(newLevel);
                needExp = tbLevel.Exp[color];
                if (needExp == -1)
                {
                    break;
                }
            }
            //消耗材料
            foreach (var i in needList)
            {
                needBag.ReduceCountByIndex(i, 1, eDeleteItemType.AstrologyLevelUp);
                //needBag.CleanItemByIndex(i);
            }
            item.SetExdata(0, newLevel);
            item.SetExdata(1, newExp);
            item.MarkDirty();
            if (bagId == (int) eBagType.GemEquip && newLevel > oldLevel)
            {
                character.mBag.RefreshGemAttr();
                character.BooksChange();
            }
            return ErrorCodes.OK;
        }

        public ErrorCodes AstrologyEquipOn(CharacterController character, int bagIndex, int astrologyId, int Index)
        {
            if (astrologyId < 0 || astrologyId > 11)
            {
                return ErrorCodes.Error_DataOverflow;
            }
            if (Index < 0 || Index > 2)
            {
                return ErrorCodes.Error_DataOverflow;
            }
            var bag = character.mBag.GetBag((int) eBagType.GemBag);
            if (bag == null)
            {
                return ErrorCodes.Error_BagID;
            }
            var equipItem = bag.GetItemByIndex(bagIndex);
            if (equipItem == null || equipItem.GetId() == -1)
            {
                return ErrorCodes.Error_ItemNotFind;
            }
            var tbGem = Table.GetGem(equipItem.GetId());
            if (tbGem == null)
            {
                return ErrorCodes.Error_GemID;
            }
            if (tbGem.Type == 50000 && Index == 0)
            {
            }
            else if (tbGem.Type == 51000 && Index == 1)
            {
            }
            else if (tbGem.Type == 52000 && Index == 2)
            {
            }
            else
            {
                return ErrorCodes.Error_GemID;
            }
            //oldItem
            for (var i = 0; i < 3; ++i)
            {
                if (Index == i)
                {
                    continue;
                }
                var tempItem = character.GetItemByBagByIndex((int) eBagType.GemEquip, i*12 + astrologyId);
                if (tempItem == null || tempItem.GetId() == -1)
                {
                    continue;
                }
                var tbTempGem = Table.GetGem(tempItem.GetId());
                if (tbTempGem.Type == tbGem.Type)
                {
                    return ErrorCodes.Error_GemTypeSame;
                }
            }
            var equipIndex = Index*12 + astrologyId;
            //var oldItem = character.GetItemByBagByIndex((int) eBagType.GemEquip, equipIndex);
            var result = character.mBag.MoveItem((int) eBagType.GemBag, bagIndex, (int) eBagType.GemEquip, equipIndex, 1);
            if (result == ErrorCodes.OK)
            {
                character.mBag.RefreshGemAttr();
                character.BooksChange();
            }
            return result;
        }

        public ErrorCodes AstrologyEquipOff(CharacterController character, int astrologyId, int Index)
        {
            if (astrologyId < 0 || astrologyId > 11)
            {
                return ErrorCodes.Error_DataOverflow;
            }
            if (Index < 0 || Index > 2)
            {
                return ErrorCodes.Error_DataOverflow;
            }
            //物品是否存在
            var equipIndex = Index*12 + astrologyId;
            var equipItem = character.GetItemByBagByIndex((int) eBagType.GemEquip, equipIndex);
            if (equipItem == null || equipItem.GetId() == -1)
            {
                return ErrorCodes.Error_ItemNotFind;
            }
            //包裹是否有空格脱下
            var freeIndex = character.mBag.GetBag((int) eBagType.GemBag).GetFirstFreeIndex();
            if (freeIndex == -1)
            {
                return ErrorCodes.Error_ItemNoInBag_All;
            }
            //移动物品
            var result = character.mBag.MoveItem((int) eBagType.GemEquip, equipIndex, (int) eBagType.GemBag, freeIndex,
                1);
            if (result == ErrorCodes.OK)
            {
                character.mBag.RefreshGemAttr();
                character.BooksChange();
            }
            return result;
        }
    }


    /*很重要的一个东西就是宝石的附加属性条数
     * 0、等级
     * 1、经验
     * 2、
     * 3、
     */

    //包裹排序	星座ID	星座宝石索引
    //0	        0	        0
    //1	        1	        0
    //2	        2	        0
    //3	        3	        0
    //4	        4	        0
    //5	        5	        0
    //6	        6	        0
    //7	        7	        0
    //8	        8	        0
    //9	        9	        0
    //10	    10	        0
    //11	    11	        0
    //12	    0	        1
    //13	    1	        1
    //14	    2	        1
    //15	    3	        1
    //16	    4	        1
    //17	    5	        1
    //18	    6	        1
    //19	    7	        1
    //20	    8	        1
    //21	    9	        1
    //22	    10	        1
    //23	    11	        1
    //24	    0	        2
    //25	    1	        2
    //26	    2	        2
    //27	    3	        2
    //28	    4	        2
    //29	    5	        2
    //30	    6	        2
    //31	    7	        2
    //32	    8	        2
    //33	    9	        2
    //34	    10	        2
    //35	    11	        2
    public static class AstrologyEnchance
    {
        private static IStaticAstrologyEnchance mStaticImpl;

        static AstrologyEnchance()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (AstrologyEnchance),
                typeof (AstrologyEnchanceDefaultImpl),
                o => { mStaticImpl = (IStaticAstrologyEnchance) o; });
        }

        //占星脱下
        public static ErrorCodes AstrologyEquipOff(this CharacterController character, int astrologyId, int index)
        {
            return mStaticImpl.AstrologyEquipOff(character, astrologyId, index);
        }

        //占星穿戴
        public static ErrorCodes AstrologyEquipOn(this CharacterController character,
                                                  int bagIndex,
                                                  int astrologyId,
                                                  int index)
        {
            return mStaticImpl.AstrologyEquipOn(character, bagIndex, astrologyId, index);
        }

        //占星升级
        public static ErrorCodes AstrologyLevelUp(this CharacterController character,
                                                  int bagId,
                                                  int bagIndex,
                                                  List<int> needList)
        {
            return mStaticImpl.AstrologyLevelUp(character, bagId, bagIndex, needList);
        }
    }
}