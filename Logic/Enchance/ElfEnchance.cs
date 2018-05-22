#region using

using System;
using System.Collections.Generic;
using System.Linq;
using DataTable;
using EventSystem;
using NLog;
using Shared;

#endregion

namespace Logic
{
    public interface IStaticElfEnchance
    {
        ErrorCodes BattleElf(CharacterController character, int index, int targetIndex);
        ErrorCodes BattleMainElf(CharacterController character, int index);
        int CheckElfType(CharacterController character, ElfRecord tbElf, int targetIndex);
        ErrorCodes DisBattleElf(CharacterController character, int index);
        void CheckBattleElf(CharacterController character);
        ErrorCodes EnchanceElf(CharacterController character, int bagIndex, ref int Nextlevel);
        ErrorCodes EnchanceFormation(CharacterController character, ref int Nextlevel);
        int GetElfFightCount(CharacterController character);

        ErrorCodes ResolveElf(CharacterController character,
                              int bagIndex,
                              ref ulong resolveValue);

        ErrorCodes ResolveElfList(CharacterController character, List<int> bagIndexList, ref int resolveGet);
        ErrorCodes EnchanceElfStar(CharacterController character,
            int bagIndex,
            ref int nextStar);

        ErrorCodes EnchanceElfSkill(CharacterController character,
            int bagIndex,
            int exId,
            ref int nextLevel);

        ErrorCodes ReplaceElfSkill(CharacterController character,
            int elfBagIndex,
            int exBuffIdx,
            int itemBagId,
            int itemBagindex,
            ref int nextBuffId);
    }

    public class ElfEnchanceDefaultImpl : IStaticElfEnchance
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        [Updateable("ElfEnchance")]
        public static readonly int MaxElfFightCount = 3;
        [Updateable("ElfEnchance")]
        public static int SecondCondition = Table.GetServerConfig(105).ToInt();
        [Updateable("ElfEnchance")]
        public static int ThirdCondition = Table.GetServerConfig(106).ToInt();

        public int GetElfFightCount(CharacterController character)
        {
            if (character.CheckCondition(SecondCondition) != -2)
            {
                return 1;
            }
            if (character.CheckCondition(ThirdCondition) != -2)
            {
                return 2;
            }
            return 3;
        }

        //阵法强化
        public ErrorCodes EnchanceFormation(CharacterController character, ref int Nextlevel)
        {
            var oldLevel = character.GetExData(82);
            var tbLevel = Table.GetLevelData(oldLevel);
            if (tbLevel.FightingWayExp <= 0)
            {
                return ErrorCodes.Error_FormationLevelMax;
            }
            var res = character.mBag.GetRes(eResourcesType.ElfPiece);
            if (res < tbLevel.FightingWayExp)
            {
                return ErrorCodes.Error_FormationExpNotEnough;
            }
            //character.mBag.SetRes(eResourcesType.ElfPiece, res - tbLevel.FightingWayExp);
            character.mBag.DelRes(eResourcesType.ElfPiece, tbLevel.FightingWayExp, eDeleteItemType.EnchanceFormation);
            Nextlevel = oldLevel + 1;
            character.SetExData(82, Nextlevel);
            character.mBag.RefreshElfAttr();
            character.BooksChange();
            return ErrorCodes.OK;
        }

        //精灵强化
        public ErrorCodes EnchanceElf(CharacterController character, int bagIndex, ref int Nextlevel)
        {
            var item = character.GetItemByBagByIndex((int) eBagType.Elf, bagIndex) as ElfItem;
            if (item == null)
            {
                return ErrorCodes.Error_ItemNotFind;
            }
            var tbElf = Table.GetElf(item.GetId());
            if (tbElf == null)
            {
                return ErrorCodes.Error_ElfNotFind;
            }
            var oldLevel = item.GetExdata(0);
            if ((oldLevel >= tbElf.MaxLevel&&tbElf.MaxLevel>0) || (tbElf.MaxLevel == -1 && oldLevel >= character.GetLevel()))
            {
                return ErrorCodes.Error_ElfLevelMax;
            }
            var tbLevel = Table.GetLevelData(oldLevel);
            var needExp = tbLevel.ElfExp*tbElf.ResolveCoef[0]/100;
            var oldExp = character.mBag.GetRes(eResourcesType.ElfPiece);
            if (needExp > oldExp)
            {
                return ErrorCodes.Error_FormationExpNotEnough;
            }
            //character.mBag.SetRes(eResourcesType.ElfPiece, oldExp - needExp);
            character.mBag.DelRes(eResourcesType.ElfPiece, needExp, eDeleteItemType.EnchanceElf);
            Nextlevel = oldLevel + 1;
            item.SetExdata(0, Nextlevel);
            var oldmaxLevel = character.GetExData((int) eExdataDefine.e328);
            if (Nextlevel > oldmaxLevel)
            {
                character.SetExData((int) eExdataDefine.e328, Nextlevel, true);
                character.Proxy.SyncExdata((int)eExdataDefine.e328, Nextlevel);//同步客户端扩展数据
            }
            //item.MarkDbDirty();//客户端自行增加等级
            if (bagIndex < 3)
            {
                if (bagIndex == 0)
                {
                    character.SetRankFlag(RankType.PetFight);
                }
                character.mBag.RefreshElfAttr();
                character.BooksChange();
            }  
            //删除灵兽公告
            //if (Nextlevel >= 10 && Nextlevel%5 == 0)
            //{
            //    //添加公告，恭喜玩家{0}将精灵{1}升至{2}级！实力大增！
            //    var args = new List<string>
            //    {
            //        Utils.AddCharacter(character.mGuid, character.GetName()),
            //        Utils.AddItemId(item.GetId()),
            //        Nextlevel.ToString()
            //    };
            //    var exData = new List<int>();
            //    item.CopyTo(exData);
            //    character.SendSystemNoticeInfo(215002, args, exData);
            //}

            try
            {
                var klog = string.Format("jinglingshengji#{0}|{1}|{2}|{3}|{4}|{5}",
                    character.mGuid,
                    character.GetLevel(),
                    character.serverId,
                    item.GetId(),
                    oldLevel,    // 升级前等级
                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                PlayerLog.Kafka(klog, 2);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            return ErrorCodes.OK;
        }

        private void ResolveElfLog(CharacterController character, int count)
        {
            try
            {
                var klog = string.Format("jinglingfenjie#{0}|{1}|{2}|{3}|{4}",
                    character.mGuid,
                    character.GetLevel(),
                    character.serverId,
                    count,
                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                PlayerLog.Kafka(klog, 2);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        private int CalcResolveElf(int elfId, int level, int starLevel)
        {
            var tbElf = Table.GetElf(elfId);
            if (tbElf == null)
                return 0;

            var tbLevel = Table.GetLevelData(level);
            if (tbLevel == null)
                return 0;

            starLevel = Math.Max(starLevel, 0);
            var getValue = (tbLevel.ElfResolveValue / 100.0f * tbElf.ResolveCoef[0]) + tbElf.ResolveCoef[1] * (1 + starLevel);

            return (int)getValue;
        }

        //精灵分解
        public ErrorCodes ResolveElf(CharacterController character,
                                     int bagIndex,
                                     ref ulong resolveValue)
        {
            var item = character.GetItemByBagByIndex((int) eBagType.Elf, bagIndex) as ElfItem;
            if (item == null)
            {
                return ErrorCodes.Error_ItemNotFind;
            }
            var tbElf = Table.GetElf(item.GetId());
            if (tbElf == null)
            {
                return ErrorCodes.Error_ElfNotFind;
            }
            if (item.GetIndex() < MaxElfFightCount)
            {
                return ErrorCodes.Error_ElfAlreadyBattle;
            }
            var oldLevel = item.GetExdata(0);
            resolveValue = (ulong)CalcResolveElf(tbElf.Id, oldLevel, item.GetStarLevel());
            character.mBag.AddRes(eResourcesType.ElfPiece, (int) resolveValue, eCreateItemType.ResolveElf);
            character.mBag.mBags[(int) eBagType.Elf].ReduceCountByIndex(bagIndex, 1, eDeleteItemType.ResolveElf);
            character.AddExData((int) eExdataDefine.e327, 1);
            ResolveElfLog(character, 1);
            return ErrorCodes.OK;
        }

        public ErrorCodes ResolveElfList(CharacterController character, List<int> bagIndexList, ref int resolveGet)
        {
            var error = ErrorCodes.OK;
            var count = 0;
            var resolveValue = 0;
            for (var i = 0; i < bagIndexList.Count; ++i)
            {
                var bagIndex = bagIndexList[i];
                if (bagIndex < 3)
                    continue;
                var item = character.GetItemByBagByIndex((int)eBagType.Elf, bagIndex) as ElfItem;
                if (item == null)
                {
                    error = ErrorCodes.Error_ItemNotFind;
                    break;
                }
                var tbElf = Table.GetElf(item.GetId());
                if (tbElf == null)
                {
                    error = ErrorCodes.Error_ElfNotFind;
                    break;
                }
                if (item.GetIndex() < MaxElfFightCount)
                {
                    error = ErrorCodes.Error_ElfAlreadyBattle;
                    break;
                }
                var oldLevel = item.GetExdata(0);
                resolveValue += CalcResolveElf(tbElf.Id, oldLevel, item.GetStarLevel());
                character.mBag.mBags[(int)eBagType.Elf].ReduceCountByIndex(bagIndex, 1, eDeleteItemType.ResolveElf);
                ++count;
            }

            if (count > 0)
            {
                character.mBag.AddRes(eResourcesType.ElfPiece, resolveValue, eCreateItemType.ResolveElf);
                character.AddExData((int)eExdataDefine.e327, count);
                ResolveElfLog(character, count);
            }
            resolveGet = resolveValue;

            return error;
        }

        // 精灵升星
        public ErrorCodes EnchanceElfStar(CharacterController character,
            int bagIndex,
            ref int nextStar)
        {
            var item = character.GetItemByBagByIndex((int)eBagType.Elf, bagIndex) as ElfItem;
            if (item == null)
            {
                return ErrorCodes.Error_ItemNotFind;
            }
            var tbElf = Table.GetElf(item.GetId());
            if (tbElf == null)
            {
                return ErrorCodes.Error_ElfNotFind;
            }
            var starLevel = item.GetStarLevel();
            if (starLevel >= tbElf.ElfStarUp.Count)
            {
                return ErrorCodes.Error_ElfStarMax;
            }
            if (starLevel < 0)
                starLevel = 0;

            // 检查物品
            var tbConsume = Table.GetConsumArray(tbElf.ElfStarUp[starLevel]);
            if (tbConsume == null)
            {
                return ErrorCodes.Error_ElfConsumeArrayNotFound;
            }

            var elfBag = character.mBag.GetBag((int)eBagType.Elf);
            if (elfBag == null)
            {
                return ErrorCodes.Unknow;
            }

            // 排除出战的宠物跟自己
            var elfCount = new Dictionary<int, int>();
            for (var i = 3; i < elfBag.mLogics.Count; ++i)
            {
                var itemBase = elfBag.mLogics[i];
                if (itemBase.GetId() < 0)
                {
                    continue;
                }

                if (itemBase.GetIndex() == bagIndex)
                    continue;

                elfCount.modifyValue(itemBase.GetId(), itemBase.GetCount());
            }

            for (var i = 0; i < tbConsume.ItemId.Length; i++)
            {
                var itemId = tbConsume.ItemId[i];
                var needCount = tbConsume.ItemCount[i];
                if (itemId == -1)
                {
                    continue;
                }
                var tbItem = Table.GetItemBase(itemId);
                if (tbItem == null)
                {
                    return ErrorCodes.Error_ItemNotFind;
                }
                if (itemId < (int) eResourcesType.CountRes)
                {
                    var res = character.mBag.GetRes((eResourcesType)itemId);
                    if (res < needCount)
                    {
                        return ErrorCodes.ItemNotEnough;
                    }                    
                }
                else if (tbItem.InitInBag == (int)eBagType.Elf)
                { // 消耗灵兽
                    int haveCount;
                    if (elfCount.TryGetValue(itemId, out haveCount))
                    {
                        if (haveCount < needCount)
                        {
                            return ErrorCodes.ItemNotEnough;
                        }
                    }
                    else
                    {
                        return ErrorCodes.ItemNotEnough;
                    }
                }
                else
                { // 消耗物品
                    if (character.mBag.GetItemCount(itemId) < needCount)
                    {
                        return ErrorCodes.ItemNotEnough;
                    }
                }
            }

            // 删除物品
            for (var i = 0; i < tbConsume.ItemId.Length; i++)
            {
                var itemId = tbConsume.ItemId[i];
                if (itemId == -1)
                {
                    continue;
                }

                var deleteItemType = eDeleteItemType.ElfStar;
                var needCount = tbConsume.ItemCount[i];
                if (itemId < (int)eResourcesType.CountRes)
                { // 资源型数据
                    character.mBag.DelRes((eResourcesType)itemId, needCount, deleteItemType);
                    continue;
                }

                var tbItem = Table.GetItemBase(itemId);
                if (tbItem.InitInBag != (int) eBagType.Elf)
                {
                    character.mBag.DeleteItem(itemId, needCount, deleteItemType);
                    continue;
                }

                var delList = new List<ItemBase>();  // 不考虑叠加
                for (var ii = elfBag.mLogics.Count - 1; ii >= 3; ii--)
                {
                    var bagItem = elfBag.mLogics[ii];
                    if (bagItem.GetId() != itemId)
                        continue;

                    if (bagIndex == ii)
                        continue;

                    delList.Add(bagItem);
                }
                delList.Sort((l, r) =>
                {
                    var lstar = l.GetExdata((int)ElfExdataDefine.StarLevel);
                    var rstar = r.GetExdata((int)ElfExdataDefine.StarLevel);
                    return lstar < rstar ? -1 : 1;
                });

                for (var j = 0; j < needCount; ++j)
                {
                    var delItem = delList[j];
                    PlayerLog.DataLog(character.mGuid, "elf star levelup id,{0},{1},{2},{3}", item.GetId(), item.GetStarLevel(),
                        delItem.GetId(), delItem.GetExdata((int)ElfExdataDefine.StarLevel));
                    elfBag.CleanItemByIndex(delItem.GetIndex());
                }
            }

            // 升星
            nextStar = starLevel + 1;
            item.SetStarLevel(nextStar);
            var itemba = Table.GetItemBase(item.GetId());
            if (nextStar == 5)
            {
                var args = new List<string>
                        {
                        Utils.AddCharacter(character.mGuid,character.GetName()),
                        string.Format("[{0}]{1}[-]",Utils.GetTableColorString(itemba.Quality),itemba.Name),    
                        };
                var exExdata = new List<int>();
                character.SendSystemNoticeInfo(291008, args, exExdata);
            }
            if (bagIndex < 3)
            {
                if (bagIndex == 0)
                {
                    character.SetRankFlag(RankType.PetFight);
                }
                character.mBag.RefreshElfAttr();
                character.BooksChange();
            }

            try
            {
                var klog = string.Format("jinglingshengxing#{0}|{1}|{2}|{3}|{4}|{5}",
                    character.mGuid,
                    character.GetLevel(),
                    character.serverId,
                    item.GetId(),
                    starLevel,
                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                PlayerLog.Kafka(klog, 2);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            return ErrorCodes.OK;
        }

        public ErrorCodes EnchanceElfSkill(CharacterController character,
            int bagIndex,
            int exId,
            ref int nextLevel)
        {
            var item = character.GetItemByBagByIndex((int)eBagType.Elf, bagIndex) as ElfItem;
            if (item == null)
            {
                return ErrorCodes.Error_ItemNotFind;
            }
            var tbElf = Table.GetElf(item.GetId());
            if (tbElf == null)
            {
                return ErrorCodes.Error_ElfNotFind;
            }

            var buffId = item.GetBuffId(exId);
            if (buffId < 0)
            {
                return ErrorCodes.Error_BuffID;
            }

            var tbBuff = Table.GetBuff(buffId);
            if (tbBuff == null)
            {
                return ErrorCodes.Error_BuffID;                
            }

            var skillLevel = item.GetBuffLevel(exId);
            if (skillLevel < 0 || skillLevel > tbBuff.ElfSkillUp.Count)
            {
                return ErrorCodes.Error_ElfMaxSkill;
            }

            // 检查物品
            var tbConsume = Table.GetConsumArray(tbBuff.ElfSkillUp[skillLevel - 1]);
            if (tbConsume == null)
            {
                return ErrorCodes.Error_ElfConsumeArrayNotFound;
            }

            for (var i = 0; i < tbConsume.ItemId.Length; i++)
            {
                var itemId = tbConsume.ItemId[i];
                var needCount = tbConsume.ItemCount[i];
                if (itemId == -1)
                {
                    continue;
                }

                if (itemId < (int)eResourcesType.CountRes)
                {
                    var res = character.mBag.GetRes((eResourcesType)itemId);
                    if (res < needCount)
                    {
                        return ErrorCodes.ItemNotEnough;
                    }
                }
                else
                {
                    var haveCount = character.mBag.GetItemCount(itemId);
                    if (haveCount < needCount)
                    {
                        return ErrorCodes.ItemNotEnough;
                    }
                }
            }

            // 删除物品
            for (var i = 0; i < tbConsume.ItemId.Length; i++)
            {
                var itemId = tbConsume.ItemId[i];
                if (itemId == -1)
                {
                    continue;
                }

                var deleteItemType = eDeleteItemType.ElfStar;
                var needCount = tbConsume.ItemCount[i];
                if (itemId < (int)eResourcesType.CountRes)
                { // 资源型数据
                    character.mBag.DelRes((eResourcesType)itemId, needCount, deleteItemType);
                }
                else
                {
                    character.mBag.DeleteItem(itemId, needCount, deleteItemType);
                }
            }

            // 升级
            nextLevel = skillLevel + 1;
            item.SetBuffLevel(exId, nextLevel);

            if (bagIndex < 3)
            {
                if (bagIndex == 0)
                {
                    character.SetRankFlag(RankType.PetFight);
                }

                var removeBuff = new List<int>();
                var addBuff = new Dictionary<int, int>();
                removeBuff.Add(buffId);
                addBuff[buffId] = nextLevel;
                character.ElfChange(removeBuff, addBuff);
            }

            try
            {
                var klog = string.Format("jinglingjinengshengji#{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                    character.mGuid,
                    character.GetLevel(),
                    character.serverId,
                    item.GetId(),
                    buffId, // 技能ID
                    skillLevel,
                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                PlayerLog.Kafka(klog, 2);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            return ErrorCodes.OK;            
        }

        public ErrorCodes ReplaceElfSkill(CharacterController character,
            int elfBagIndex,
            int exBuffIdx,
            int itemBagId,
            int itemBagindex,
            ref int nextBuffId)
        {
            var item = character.GetItemByBagByIndex((int)eBagType.Elf, elfBagIndex) as ElfItem;
            if (item == null)
            {
                return ErrorCodes.Error_ItemNotFind;
            }
            var tbElf = Table.GetElf(item.GetId());
            if (tbElf == null)
            {
                return ErrorCodes.Error_ElfNotFind;
            }

            // 检查物品
            var costItem = character.GetItemByBagByIndex(itemBagId, itemBagindex);
            if (costItem == null)
            {
                return ErrorCodes.Error_ItemNoInBag;
            }

            var tbItem = Table.GetItemBase(costItem.GetId());
            if (tbItem == null)
            {
                return ErrorCodes.Error_ItemID;
            }

            if (tbItem.Type != 26900)
            {
                return ErrorCodes.Error_ELfItemType;
            }

            // 物品类型
            var buffType = tbItem.Exdata[1];
            if (buffType != exBuffIdx)
            {
                return ErrorCodes.Error_ELfItemType;
            }

            var oldBuff = item.GetBuffId(exBuffIdx);
            var oldBuffLevel = item.GetBuffLevel(exBuffIdx);

            // 返还升级所需要的物品
            var recycleDict = new Dictionary<int, int>();
            var tbOldBuff = Table.GetBuff(oldBuff);
            if (tbOldBuff != null)
            {
                // 返还升级到oldBuffLevel等级所需要的物品
                for (var i = 0; i < oldBuffLevel; ++i)
                {
                    var index = Math.Max(0, i - 1);  // 因为ElfSkillUp[0]是1升2级的，所以都是i-1，当i == 0时，设置为1级的消耗
                    
                    if (index >= tbOldBuff.ElfSkillUp.Count)
                        continue;

                    var tbConsume = Table.GetConsumArray(tbOldBuff.ElfSkillUp[index]);
                    if (tbConsume == null)
                        continue;

                    for (var j = 0; j < tbConsume.ItemId.Length; ++j)
                    {
                        var itemId = tbConsume.ItemId[j];
                        if (itemId < 0)
                            continue;
                        if (itemId == (int)eResourcesType.ElfPiece)     // 不返还精魄
                            continue;
                        var itemCount = tbConsume.ItemCount[j];
                        recycleDict.modifyValue(itemId, itemCount);
                    }
                }
                var result = BagManager.CheckAddItemList(character.mBag, recycleDict);
                if (result != ErrorCodes.OK)
                { // 不能增加返还物品
                    return result;
                }
            }

            // 换技能
            var buffId = item.ReplaceBuff(tbItem.Exdata[0], exBuffIdx);
            if (buffId < 0)
            {
                return ErrorCodes.Error_ELfItemType;
            }
            int buffLevel = 1;
            item.SetBuffLevel(exBuffIdx, buffLevel);

            // 删除物品
            character.mBag.GetBag(itemBagId).ReduceCountByIndex(itemBagindex, 1, eDeleteItemType.ELfSkillReplace);
            // 增加返还物品
            if (recycleDict.Count > 0)
                character.mBag.AddItems(recycleDict, eCreateItemType.ElfReplaceBuff);

            if (elfBagIndex < 3)
            {
                if (elfBagIndex == 0)
                {
                    character.SetRankFlag(RankType.PetFight);
                }

                var allBuff = new Dictionary<int, int>();
                for (var i = 0; i < 3; ++i)
                {
                    var tempItem = character.GetItemByBagByIndex((int) eBagType.Elf, i) as ElfItem;
                    if (tempItem == null)
                        continue;
                    tempItem.FillAllBuff(allBuff);
                }

                var removeBuff = new List<int>();
                var addBuff = new Dictionary<int, int>();
                int tempBuffLevel;
                if (oldBuff != -1)
                {
                    if (allBuff.TryGetValue(oldBuff, out tempBuffLevel))
                    {
                        if (tempBuffLevel != oldBuffLevel)
                        {
                            removeBuff.Add(oldBuff);
                            addBuff[oldBuff] = tempBuffLevel;
                        }
                    }
                    else
                    {
                        removeBuff.Add(oldBuff);
                    }                    
                }

                if (allBuff.TryGetValue(buffId, out tempBuffLevel))
                {
                    addBuff[buffId] = tempBuffLevel;
                }

                character.ElfChange(removeBuff, addBuff);
            }

            nextBuffId = buffId;

            try
            {
                var klog = string.Format("jinglingjinengtihuan#{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                    character.mGuid,
                    character.GetLevel(),
                    character.serverId,
                    item.GetId(),
                    oldBuff,
                    nextBuffId,  // 替换为的技能id
                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                PlayerLog.Kafka(klog, 2);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            return ErrorCodes.OK;                
        }

        public int CheckElfType(CharacterController character, ElfRecord tbElf, int targetIndex)
        {
            var sameIndex = 0;
            var elfBag = character.GetBag((int) eBagType.Elf);
            var nowFightCount = character.GetElfFightCount();
            if (targetIndex > nowFightCount)
            {
//检查尚未开启的精灵栏，返回不可用
                return 0;
            }
            for (var i = 0; i != nowFightCount; ++i)
            {
                if (i == targetIndex)
                {
                    continue;
                }
                var tempItem = elfBag.GetItemByIndex(i);
                if (tempItem == null)
                {
                    continue;
                }
                var tbTempElf = Table.GetElf(tempItem.GetId());
                if (tbTempElf == null)
                {
                    continue;
                }
                if (tbTempElf.ElfType == tbElf.ElfType)
                {
                    return sameIndex;
                }
            }
            return -1;
        }

        //出战精灵
        public ErrorCodes BattleElf(CharacterController character, int index, int targetIndex)
        {
            var nowFightCount = character.GetElfFightCount();

            if (targetIndex < 1 || targetIndex >= nowFightCount)
            {
                return ErrorCodes.Error_DataOverflow;
            }
            if (index == 0 && targetIndex > 3)
            {
                return ErrorCodes.Error_ElfAlreadyBattle;
            }
            if (index == targetIndex)
            {
                return ErrorCodes.Error_ElfAlreadyBattle;
            }
            if (index > 0 && index < nowFightCount)
            {
                character.mBag.MoveItem((int) eBagType.Elf, index, (int) eBagType.Elf, targetIndex, 1);
                return ErrorCodes.OK;
            }
            var elfBag = character.GetBag((int) eBagType.Elf);
            var item = elfBag.GetItemByIndex(index);
            if (item == null || item.GetId() < 0)
            {
                return ErrorCodes.Error_ElfNotFind;
            }
            var tbElf = Table.GetElf(item.GetId());
            if (tbElf == null)
            {
                return ErrorCodes.Error_ElfNotFind;
            }

            if (item.GetIndex() >= MaxElfFightCount)
            {
                if (character.CheckElfType(tbElf, targetIndex) != -1)
                {
                    return ErrorCodes.Error_ElfTypeSame;
                }
            }
            character.mBag.MoveItem((int) eBagType.Elf, index, (int) eBagType.Elf, targetIndex, 1);
            character.mBag.RefreshElfAttr();
            character.BooksChange();
            return ErrorCodes.OK;
        }

        public void CheckBattleElf(CharacterController character)
        {
            var nowFightCount = character.GetElfFightCount();
            bool bRefresh = false;
            for (int i = 0; i < 3; i++)
            {
                if (i >= nowFightCount)
                {
                    var item = character.GetItemByBagByIndex((int)eBagType.Elf, i);
                    if (item == null || item.GetId() < 0)
                    {
                        continue ;
                    }
                    var freeindex = character.mBag.mBags[(int)eBagType.Elf].GetFirstFreeIndex(MaxElfFightCount);
                    if (freeindex != -1)
                    {
                        bRefresh = true;
                        character.mBag.MoveItem((int)eBagType.Elf, i, (int)eBagType.Elf, freeindex, 1);
                    }                    
                }
            }
            if (bRefresh)
            {
                character.mBag.RefreshElfAttr();
                character.BooksChange();
            }
        }
        //休息精灵
        public ErrorCodes DisBattleElf(CharacterController character, int index)
        {
            var nowFightCount = character.GetElfFightCount();

            if (index < 0 && index >= 3)
            {
                return ErrorCodes.Error_ElfNotBattle;
            }
            var item = character.GetItemByBagByIndex((int) eBagType.Elf, index);
            if (item == null || item.GetId() < 0)
            {
                return ErrorCodes.Error_ElfNotFind;
            }
            var freeindex = character.mBag.mBags[(int) eBagType.Elf].GetFirstFreeIndex(MaxElfFightCount);
            if (freeindex != -1)
            {
                character.mBag.MoveItem((int) eBagType.Elf, index, (int) eBagType.Elf, freeindex, 1);
                character.mBag.RefreshElfAttr();
                character.BooksChange();
                return ErrorCodes.OK;
            }
            return ErrorCodes.Error_ItemNoInBag_All;
        }

        //展示精灵
        public ErrorCodes BattleMainElf(CharacterController character, int index)
        {
            if (index <= 0)
            {
                return ErrorCodes.Error_ElfNotBattle;
            }
            var item = character.GetItemByBagByIndex((int) eBagType.Elf, index);
            if (item == null || item.GetId() < 0)
            {
                return ErrorCodes.Error_ElfNotFind;
            }

            var tbElf = Table.GetElf(item.GetId());
            if (tbElf == null)
            {
                return ErrorCodes.Error_ElfNotFind;
            }
            if (item.GetIndex() >= MaxElfFightCount)
            {
                if (character.CheckElfType(tbElf, 0) != -1)
                {
                    return ErrorCodes.Error_ElfTypeSame;
                }
            }
            var nowFightCount = character.GetElfFightCount();

            if (item.GetIndex() >= MaxElfFightCount) //如果原来是休息的,需要重新为老的尝试找位置
            {
                var tempItem = character.GetItemByBagByIndex((int) eBagType.Elf, 0);
                if (tempItem != null && tempItem.GetId() != -1)
                {
                    var tbtempElf = Table.GetElf(tempItem.GetId());
                    if (tbtempElf != null && tbtempElf.ElfType != tbElf.ElfType) //类型冲突就不找了 直接休息吧
                    {
                        for (var i = 1; i != nowFightCount; ++i)
                        {
                            var tempbatItem = character.GetItemByBagByIndex((int) eBagType.Elf, i);
                            if (tempbatItem == null || tempbatItem.GetId() == -1)
                            {
                                character.mBag.MoveItem((int) eBagType.Elf, 0, (int) eBagType.Elf, i, 1);
                                character.SetRankFlag(RankType.PetFight);
                                break;
                            }
                        }
                    }
                }
            }
            character.mBag.MoveItem((int) eBagType.Elf, index, (int) eBagType.Elf, 0, 1);
            //出战
            //item.MarkDbDirty();
            character.mBag.RefreshElfAttr();
            character.BooksChange();
            character.SetRankFlag(RankType.PetFight);
            return ErrorCodes.OK;
        }
    }

    public static class ElfEnchance
    {
        private static IStaticElfEnchance mStaticImpl;

        static ElfEnchance()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (ElfEnchance), typeof (ElfEnchanceDefaultImpl),
                o => { mStaticImpl = (IStaticElfEnchance) o; });
        }

        //出战精灵
        public static ErrorCodes BattleElf(this CharacterController character, int index, int targetIndex)
        {
            return mStaticImpl.BattleElf(character, index, targetIndex);
        }

        //展示精灵
        public static ErrorCodes BattleMainElf(this CharacterController character, int index)
        {
            return mStaticImpl.BattleMainElf(character, index);
        }

        public static int CheckElfType(this CharacterController character, ElfRecord tbElf, int targetIndex)
        {
            return mStaticImpl.CheckElfType(character, tbElf, targetIndex);
        }

        //休息精灵
        public static ErrorCodes DisBattleElf(this CharacterController character, int index)
        {
            return mStaticImpl.DisBattleElf(character, index);
        }

        public static void CheckBattleElf(this CharacterController character)
        {
            mStaticImpl.CheckBattleElf(character);
        }

        //精灵强化
        public static ErrorCodes EnchanceElf(this CharacterController character, int bagIndex, ref int nextLevel)
        {
            return mStaticImpl.EnchanceElf(character, bagIndex, ref nextLevel);
        }

        //阵法强化
        public static ErrorCodes EnchanceFormation(this CharacterController character, ref int nextLevel)
        {
            return mStaticImpl.EnchanceFormation(character, ref nextLevel);
        }

        public static int GetElfFightCount(this CharacterController character)
        {
            return mStaticImpl.GetElfFightCount(character);
        }

        //精灵分解
        public static ErrorCodes ResolveElf(this CharacterController character,
                                            int bagIndex,
                                            ref ulong resolveValue)
        {
            return mStaticImpl.ResolveElf(character, bagIndex, ref resolveValue);
        }

        public static ErrorCodes ResolveElfList(this CharacterController character,
                                    List<int> bagIndexList,
                                    ref int resolveGet)
        {
            return mStaticImpl.ResolveElfList(character, bagIndexList, ref resolveGet);
        }

        public static ErrorCodes EnchanceElfStar(this CharacterController character,
                                    int bagIndex,
                                    ref int nextStar)
        {
            return mStaticImpl.EnchanceElfStar(character, bagIndex, ref nextStar);
        }
        public static ErrorCodes EnchanceElfSkill(this CharacterController character,
                            int bagIndex,
                            int exId,
                            ref int nextLevel)
        {
            return mStaticImpl.EnchanceElfSkill(character, bagIndex, exId, ref nextLevel);
        }
        public static ErrorCodes ReplaceElfSkill(this CharacterController character,
                            int elfBagIndex,
                            int exBuffIdx,
                            int itemBagId,
                            int itemBagindex,
                            ref int nextBuffId)
        {
            return mStaticImpl.ReplaceElfSkill(character, elfBagIndex, exBuffIdx,
                itemBagId, itemBagindex, ref nextBuffId);
        }
    }
}