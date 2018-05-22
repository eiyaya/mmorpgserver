#region using

using System;
using System.Collections.Generic;
using DataTable;
using EventSystem;
using NLog;
using Shared;

#endregion

namespace Logic
{
    public interface IEquipEnchance2
    {
        ErrorCodes ConfirmResetExcellentEquip(CharacterController character, int bagId, int bagIndex, int isOk);
        ErrorCodes SaveSuperExcellentEquip(CharacterController character, int bagId, int bagIndex, int isOk);
        ErrorCodes EnchanceEquip(CharacterController character,
                                 int bagId,
                                 int bagIndex,
                                 int blessing,
                                 int upRate,
                                 int costGoleBless,
                                 ref int Nextlevel);

        ErrorCodes EquipAdditionalEquip(CharacterController character, int bagId, int bagIndex, ref int NextValue);
        int GetAdditionalTable1(EquipAdditional1Record tbAdditional, int Value);
        ErrorCodes RecoveryEquip(CharacterController character, int Type, List<int> indexList);
        ErrorCodes ResetExcellentEquip(CharacterController character, int bagId, int bagIndex, List<int> attrList);

        ErrorCodes SmritiEquip(CharacterController character,
                               int smritiType,
                               int moneyType,
                               int fromBagType,
                               int fromBagIndex,
                               int toBagType,
                               int toBagIndex,
                               ref int appendCount);

        ErrorCodes SuperExcellentEquip(CharacterController character,
                                       int bagId,
                                       int bagIndex,
                                       List<int> lockList,
                                       List<int> attrId,
                                       List<int> attrValue);

        ErrorCodes RandEquipSkill(CharacterController character,
            int bagId,
            int bagIndex,
            int itemId,
            ref int buffId);
        ErrorCodes UseEquipSkill(CharacterController character,
            int bagId,
            int bagIndex,
            int type,
            ref int buffId);
    }

    public class EquipEnchance2DefaultImpl : IEquipEnchance2
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //检查传承的物品类型条件
        private ErrorCodes CheckSmritEquip(ItemBaseRecord tbFromItem, ItemBaseRecord tbToItem)
        {
            if (tbFromItem.Type == tbToItem.Type)
            {
                return ErrorCodes.OK;
            }
            // 10010:主手
            // 10098:单手
            // 10099:双手
            if (tbFromItem.Type == 10010 || tbFromItem.Type == 10098 || tbFromItem.Type == 10099)
            {
                if (tbToItem.Type == 10010 || tbToItem.Type == 10098 || tbToItem.Type == 10099)
                {
                    return ErrorCodes.Error_EquipSmritTypeNotSame;
                }
            }
            return ErrorCodes.Error_EquipTypeNotSame;
        }

        //根据两件装备，反推追加值需求
        private int EquipAdditionalEquipValue(EquipRecord tbToEquip, int fromRes)
        {
            var newValue = MyRandom.Random(tbToEquip.AddAttrUpMinValue, tbToEquip.AddAttrUpMaxValue);
            var tbAdditional = Table.GetEquipAdditional1(tbToEquip.AddIndexID);
            if (tbAdditional == null)
            {
                return -1;
            }
            var loopCheck = true;
            while (loopCheck)
            {
                var AddLevel = GetAdditionalTable1(tbAdditional, newValue);
                //材料检查
                var needItemCount = Table.GetSkillUpgrading(tbAdditional.MaterialCount).GetSkillUpgradingValue(AddLevel);
                //增加值
                var minUp = Table.GetSkillUpgrading(tbAdditional.MinSection).GetSkillUpgradingValue(AddLevel);
                var maxUp = Table.GetSkillUpgrading(tbAdditional.MaxSection).GetSkillUpgradingValue(AddLevel);
                newValue = newValue + MyRandom.Random(minUp, maxUp);
                if (newValue > tbToEquip.AddAttrMaxValue)
                {
                    newValue = tbToEquip.AddAttrMaxValue;
                    loopCheck = false;
                }
                if (fromRes < needItemCount)
                {
                    loopCheck = false;
                }
                fromRes -= needItemCount;
            }
            return newValue;
        }

        //强化装备
        public ErrorCodes EnchanceEquip(CharacterController character,
                                        int bagId,
                                        int bagIndex,
                                        int blessing,
                                        int upRate,
                                        int costGoleBless,
                                        ref int Nextlevel)
        {
            //条件整理
            var theEquip = character.GetItemByBagByIndex(bagId, bagIndex);
            if (theEquip == null)
            {
                Logger.Warn("DoEquipEnchance bagId({0})[{1}] is Empty!", bagId, bagIndex);
                return ErrorCodes.Error_ItemNotFind;
            }
            //等级限制
            var theEquip2 = theEquip as ItemEquip2;
            if (theEquip2 == null)
            {
                return ErrorCodes.Error_ItemIsNoEquip;
            }
            var tbItem = Table.GetItemBase(theEquip.GetId());
            if (tbItem == null)
            {
                return ErrorCodes.Error_ItemID;
            }
            var tbEquip = Table.GetEquip(tbItem.Exdata[0]);
            if (tbEquip == null)
            {
                return ErrorCodes.Error_EquipID;
            }
            var nLevel = theEquip.GetExdata(0);
            if (nLevel >= tbEquip.MaxLevel)
            {
                Logger.Warn("DoEquipEnchance  itemid={0} is Level={1}!", tbEquip.Id, nLevel);
                return ErrorCodes.Error_EquipLevelMax; //装备已经到最大的强化等级了
            }
            //强化参数表
            var tbBlessing = Table.GetEquipBlessing(nLevel);
            if (tbBlessing == null)
            {
                return ErrorCodes.Error_EquipBlessingID;
            }
            if (upRate == 1 && tbBlessing.SpecialId != -1)
            {
                tbBlessing = Table.GetEquipBlessing(tbBlessing.SpecialId);
                if (tbBlessing == null)
                {
                    return ErrorCodes.Error_DataOverflow;
                }
            }
            //金钱检查
            if (character.mBag.GetRes(eResourcesType.GoldRes) < tbBlessing.NeedMoney)
            {
                return ErrorCodes.MoneyNotEnough;
            }
            //材料检查
            for (var i = 0; i != 3; ++i)
            {
                if (tbBlessing.NeedItemId[i] < 0)
                {
                    break;
                }
                if (character.mBag.GetItemCount(tbBlessing.NeedItemId[i]) < tbBlessing.NeedItemCount[i])
                {
                    return ErrorCodes.ItemNotEnough;
                }
            }
            if (blessing == 1)
            {
                if (character.mBag.GetItemCount(tbBlessing.WarrantItemId) <= 0)
                {
                    return ErrorCodes.ItemNotEnough;
                }
                //if (character.mBag.GetItemCount(tbBlessing.WarrantItemId) < tbBlessing.WarrantItemCount)
                //{
                //    return ErrorCodes.ItemNotEnough;
                //}
            }
            //消耗材料
            character.mBag.DeleteItem((int) eResourcesType.GoldRes, tbBlessing.NeedMoney, eDeleteItemType.EnchanceEquip);
            for (var i = 0; i != 3; ++i)
            {
                if (tbBlessing.NeedItemId[i] < 0)
                {
                    break;
                }
                character.mBag.DeleteItem(tbBlessing.NeedItemId[i], tbBlessing.NeedItemCount[i],
                    eDeleteItemType.EnchanceEquip);
            }
            var warrantNum = 0;
            if (blessing == 1)
            {
                //if (warrantNum >= tbBlessing.WarrantItemCount)
                //{
                //    warrantNum = tbBlessing.WarrantItemCount;
                //}          
                if (costGoleBless >= tbBlessing.WarrantItemCount)
                {
                    costGoleBless = tbBlessing.WarrantItemCount;
                }
                warrantNum = costGoleBless;
                character.mBag.DeleteItem(tbBlessing.WarrantItemId, costGoleBless, eDeleteItemType.EnchanceEquip);
            }
            //vip增加成功率
            var vipLevel = character.mBag.GetRes(eResourcesType.VipLevel);
            var tbVip = Table.GetVIP(vipLevel);
            
            //随机成功率
            if (MyRandom.Random(10000) < tbBlessing.Probability + warrantNum * tbBlessing.MoreChance + tbVip.EnhanceRatio * 100)
            {
                Nextlevel = nLevel + 1;
                character.AddExData((int)eExdataDefine.e47, 1);
            }
            else
            {
                character.AddExData((int)eExdataDefine.e85, 1);              
                //if (blessing == 1)
                //{
                //    //使用神佑宝石失败不掉等级
                //    Nextlevel = nLevel;
                //}
                //else
                //{
                //    Nextlevel = tbBlessing.FalseLevel;
                //}

                Nextlevel = tbBlessing.FalseLevel;
            }
            if (Nextlevel != nLevel)
            {
                theEquip.SetExdata(0, Nextlevel);
                if (bagId != (int) eBagType.Equip)
                {
                    {
                        int pos = Utils.GetEquipLevelExPos(bagId, bagIndex);
                        character.SetExData(pos, Nextlevel);
                    }
                    character.EquipChange(2, bagId, bagIndex, theEquip);

                }
                theEquip.MarkDbDirty();
               
                
            }
            var e = new EnhanceEquipEvent(character, tbEquip.Part);
            EventDispatcher.Instance.DispatchEvent(e);
            character.AddExData((int) eExdataDefine.e28, 1);
            character.AddExData((int) eExdataDefine.e43, 1);

            if (bagId != (int) eBagType.Equip && Nextlevel >= 7)
            {
//如果是穿在身上的装备，则要计算下装备的称号
                character.CheckEquipEnhanceTitle();
            }

            if (Nextlevel > nLevel && Nextlevel >= 9)
            {
//添加公告，恭喜玩家{0}鸿运当头，将{1}强化至+{2}！实力大增！
                var args = new List<string>
                {
                    Utils.AddCharacter(character.mGuid, character.GetName()),
                    Utils.AddItemId(tbItem.Id),
                    Nextlevel.ToString()
                };
                var exData = new List<int>();
                theEquip.CopyTo(exData);
                character.SendSystemNoticeInfo(215000, args, exData);
            }
            theEquip2.SetBinding();

            try
            {
                var klog = string.Format("equip_enhance#{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}",
                      character.mGuid,
                      character.GetLevel(),
                      character.serverId,
                      theEquip.GetId(),
                      nLevel, // 强化等级
                      Nextlevel,
                      tbBlessing.NeedMoney,
                      tbBlessing.NeedItemCount[0],// 22000
                      DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                PlayerLog.Kafka(klog, 2);
            }
            catch (Exception ee)
            {
                Logger.Error(ee.Message);
            }

            return ErrorCodes.OK;
        }

        public ErrorCodes RandEquipSkill(CharacterController character,
                                int bagId,
                                int bagIndex,
                                int itemId,
                                ref int buffId)
        {
            var theEquip = character.GetItemByBagByIndex(bagId, bagIndex);
            if (theEquip == null)
            {
                Logger.Warn("Do RandEquipSkill bagId({0})[{1}] is Empty!", bagId, bagIndex);
                return ErrorCodes.Error_ItemNotFind;
            }
            var theEquip2 = theEquip as ItemEquip2;
            if (theEquip2 == null)
            {
                return ErrorCodes.Error_ItemIsNoEquip;
            }
            var tbItem = Table.GetItemBase(theEquip.GetId());
            if (tbItem == null)
            {
                return ErrorCodes.Error_ItemID;
            }
            var tbEquip = Table.GetEquip(tbItem.Exdata[0]);
            if (tbEquip == null)
            {
                return ErrorCodes.Error_EquipID;
            }

            //材料检查
            if (character.mBag.GetItemCount(itemId) < 1)
            {
                return ErrorCodes.ItemNotEnough;
            }

            //消耗材料
            character.mBag.DeleteItem(itemId, 1, eDeleteItemType.RandEquipSkill);

            buffId = theEquip2.RandBuff(itemId);

            return ErrorCodes.OK;
        }

        public ErrorCodes UseEquipSkill(CharacterController character,
                        int bagId,
                        int bagIndex,
                        int type,
                        ref int buffId)
        {
            var theEquip = character.GetItemByBagByIndex(bagId, bagIndex);
            if (theEquip == null)
            {
                Logger.Warn("Do RandEquipSkill bagId({0})[{1}] is Empty!", bagId, bagIndex);
                return ErrorCodes.Error_ItemNotFind;
            }
            var theEquip2 = theEquip as ItemEquip2;
            if (theEquip2 == null)
            {
                return ErrorCodes.Error_ItemIsNoEquip;
            }
            var tbItem = Table.GetItemBase(theEquip.GetId());
            if (tbItem == null)
            {
                return ErrorCodes.Error_ItemID;
            }
            var tbEquip = Table.GetEquip(tbItem.Exdata[0]);
            if (tbEquip == null)
            {
                return ErrorCodes.Error_EquipID;
            }

            if (type == 1)
            {
                theEquip2.UseRandBuff();
                character.EquipChange(2, bagId, bagIndex, theEquip);
                theEquip.MarkDbDirty();
            }
            else
            {
                theEquip2.CancleRandBuff();
            }
            buffId = theEquip2.GetBuffId(0);

            return ErrorCodes.OK;
        }

        //追加装备
        public ErrorCodes EquipAdditionalEquip(CharacterController character, int bagId, int bagIndex, ref int NextValue)
        {
            //条件整理
            var theEquip = character.GetItemByBagByIndex(bagId, bagIndex);
            if (theEquip == null)
            {
                Logger.Warn("EquipAdditionalEquip bagId({0})[{1}] is Empty!", bagId, bagIndex);
                return ErrorCodes.Error_ItemNotFind;
            }
            //等级限制
            var theEquip2 = theEquip as ItemEquip2;
            if (theEquip2 == null)
            {
                return ErrorCodes.Error_ItemIsNoEquip;
            }
            var tbItem = Table.GetItemBase(theEquip.GetId());
            if (tbItem == null)
            {
                return ErrorCodes.Error_ItemID;
            }
            var tbEquip = Table.GetEquip(tbItem.Exdata[0]);
            if (tbEquip == null)
            {
                return ErrorCodes.Error_EquipID;
            }
            var nValue = theEquip.GetExdata(1);
            if (nValue >= tbEquip.AddAttrMaxValue)
            {
                Logger.Warn("EquipAdditionalEquip  itemid={0} is Value={1}!", tbEquip.Id, nValue);
                return ErrorCodes.Error_EquipAdditionalMax; //装备已经到最大的强化等级了
            }
            var nAttrId = tbEquip.AddAttrId;
            var tbAdditional = Table.GetEquipAdditional1(tbEquip.AddIndexID);
            //EquipAdditionalRecord tbAdditional = GetAdditionalTable(tbEquip, nValue);
            if (tbAdditional == null)
            {
                Logger.Error("EquipAdditionalEquip  itemid={0} is  nAttrId={1} Value={2}!", tbEquip.Id, nAttrId, nValue);
                return ErrorCodes.Error_EquipAdditionalID;
            }
            var AddLevel = GetAdditionalTable1(tbAdditional, nValue);
            //金钱检查
            var needMoney = Table.GetSkillUpgrading(tbAdditional.Money).GetSkillUpgradingValue(AddLevel);
            if (character.mBag.GetRes(eResourcesType.GoldRes) < needMoney)
            {
                return ErrorCodes.MoneyNotEnough;
            }

            //材料检查
            var needItemCount = Table.GetSkillUpgrading(tbAdditional.MaterialCount).GetSkillUpgradingValue(AddLevel);
            if (character.mBag.GetItemCount(tbAdditional.MaterialID) < needItemCount)
            {
                return ErrorCodes.ItemNotEnough;
            }
            //消耗材料
            character.mBag.DeleteItem((int) eResourcesType.GoldRes, needMoney, eDeleteItemType.EquipAdditionalEquip);
            character.mBag.DeleteItem(tbAdditional.MaterialID, needItemCount, eDeleteItemType.EquipAdditionalEquip);
            var oldItemUsed = theEquip2.GetExdata(25);
            if (oldItemUsed < 0)
            {
                oldItemUsed = 0;
            }
            theEquip2.SetExdata(25, oldItemUsed + needItemCount);
            //增加值
            var minUp = Table.GetSkillUpgrading(tbAdditional.MinSection).GetSkillUpgradingValue(AddLevel);
            var maxUp = Table.GetSkillUpgrading(tbAdditional.MaxSection).GetSkillUpgradingValue(AddLevel);
            NextValue = nValue + MyRandom.Random(minUp, maxUp);
            if (NextValue > tbEquip.AddAttrMaxValue)
            {
                NextValue = tbEquip.AddAttrMaxValue;
            }
            theEquip.SetExdata(1, NextValue);
            theEquip.MarkDbDirty();
            //theEquip.MarkDirty();
            //theEquip.CleanNetDirty();
            character.AddExData((int) eExdataDefine.e29, 1);
            character.AddExData((int) eExdataDefine.e57, 1);
            var e = new AdditionalEquipEvent(character, tbEquip.Part);
            EventDispatcher.Instance.DispatchEvent(e);
            if (bagId != (int) eBagType.Equip)
            {
                {
                    int pos = Utils.GetEquipAddtionalExPos(bagId, bagIndex);
                    character.SetExData(pos, NextValue);
                }
                character.EquipChange(2, bagId, bagIndex, theEquip);

            }

            if (NextValue == tbEquip.AddAttrMaxValue)
            {
//添加公告，恭喜玩家{0}奋发图强，将{1}追加至上限！实力大增！
                var args = new List<string>
                {
                    Utils.AddCharacter(character.mGuid, character.GetName()),
                    Utils.AddItemId(tbItem.Id)
                };
                var exData = new List<int>();
                theEquip.CopyTo(exData);
                character.SendSystemNoticeInfo(215001, args, exData);
            }
            theEquip2.SetBinding();

            //后台统计            
            try
            {               
                string v = string.Format("equip_refine#{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                    character.serverId,
                    character.mGuid,
                    character.GetLevel(),
                    tbEquip.Id,
                    needMoney,
                    needItemCount,
                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                PlayerLog.Kafka(v);
            }
            catch (Exception)
            {
            }
            return ErrorCodes.OK;
        }

        //洗炼绿色属性
        public ErrorCodes ResetExcellentEquip(CharacterController character, int bagId, int bagIndex, List<int> attrList)
        {
            //attrList.Clear();
            //参数整理
            var theItem = character.GetItemByBagByIndex(bagId, bagIndex);
            if (theItem == null)
            {
                Logger.Warn("ResetExcellentEquip bagId({0})[{1}] is Empty!", bagId, bagIndex);
                return ErrorCodes.Error_ItemNotFind;
            }
            var theEquip = theItem as ItemEquip2;
            if (theEquip == null)
            {
                Logger.Warn("ResetExcellentEquip bagId({0})[{1}] is not Equip[{2}]!", bagId, bagIndex, theItem.GetId());
                return ErrorCodes.Error_ItemIsNoEquip;
            }
            var tbItem = Table.GetItemBase(theEquip.GetId());
            var tbEquip = Table.GetEquip(tbItem.Exdata[0]);
            var tbExcellent = Table.GetEquipExcellent(tbEquip.Ladder);
            if (tbExcellent == null)
            {
                return ErrorCodes.Error_EquipExcellentID;
            }
            //条件限制

            if (character.mBag.GetRes(eResourcesType.GoldRes) < tbExcellent.GreenMoney)
            {
                return ErrorCodes.MoneyNotEnough;
            }
            if (character.mBag.GetItemCount(tbExcellent.GreenItemId) < tbExcellent.GreenItemCount)
            {
                return ErrorCodes.ItemNotEnough;
            }
            //消耗
            character.mBag.DeleteItem((int) eResourcesType.GoldRes, tbExcellent.GreenMoney,
                eDeleteItemType.ExcellentEquip);
            character.mBag.DeleteItem(tbExcellent.GreenItemId, tbExcellent.GreenItemCount,
                eDeleteItemType.ExcellentEquip);
            //执行
            character.AddExData((int) eExdataDefine.e303, 1);
            theEquip.RandNewGreenAttr(tbEquip, attrList);

			try
			{
				character.mOperActivity.OnExcellentEquip();
			}
			catch (Exception exc)
			{
				Logger.Fatal("OnSuperExcellentEquip {0}\n{1}", exc.Message, exc.StackTrace);
			}

            //后台统计            
            try
            {                
                string v = string.Format("equip_bap#{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                    character.serverId,
                    character.mGuid,
                    character.GetLevel(),
                    tbEquip.Id,
                    tbExcellent.GreenMoney,
                    tbExcellent.GreenItemCount,
                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                PlayerLog.Kafka(v);                  
            }
            catch (Exception )
            {
            }


            return ErrorCodes.OK;
        }

        //确定使用绿色属性
        public ErrorCodes ConfirmResetExcellentEquip(CharacterController character, int bagId, int bagIndex, int isOk)
        {
            //参数整理
            var theItem = character.GetItemByBagByIndex(bagId, bagIndex);
            if (theItem == null)
            {
                Logger.Warn("ResetExcellentEquip bagId({0})[{1}] is Empty!", bagId, bagIndex);
                return ErrorCodes.Error_ItemNotFind;
            }
            var theEquip = theItem as ItemEquip2;
            if (theEquip == null)
            {
                Logger.Warn("ResetExcellentEquip bagId({0})[{1}] is not Equip[{2}]!", bagId, bagIndex, theItem.GetId());
                return ErrorCodes.Error_ItemIsNoEquip;
            }
            //执行
            var result = theEquip.UseNewGreenAttr(isOk);
            if (result == ErrorCodes.OK)
            {
                if (bagId != (int) eBagType.Equip)
                {
                    character.EquipChange(2, bagId, bagIndex, theEquip);
                }
            }
            theEquip.SetBinding();
            theEquip.MarkDirty();
            return result;
        }
        //确定使用星级属性
        public ErrorCodes SaveSuperExcellentEquip(CharacterController character, int bagId, int bagIndex, int isOk)
        {
            var theItem = character.GetItemByBagByIndex(bagId, bagIndex);
            if (theItem == null)
            {
                Logger.Warn("SaveSuperExcellentEquip bagId({0})[{1}] is Empty!", bagId, bagIndex);
                return ErrorCodes.Error_ItemNotFind;
            }
            var theEquip = theItem as ItemEquip2;
            if (theEquip == null)
            {
                Logger.Warn("SaveSuperExcellentEquip bagId({0})[{1}] is not Equip[{2}]!", bagId, bagIndex, theItem.GetId());
                return ErrorCodes.Error_ItemIsNoEquip;
            }
            var result = theEquip.UseNewSuperExcellentAttr(isOk); 
            if (result == ErrorCodes.OK)
            {
                if (bagId != (int)eBagType.Equip)
                {
                    character.EquipChange(2, bagId, bagIndex, theEquip);
                }
            }
            theEquip.SetBinding();
            theEquip.MarkDirty();
            return result;
        }

        //洗灵魂属性
        public ErrorCodes SuperExcellentEquip(CharacterController character,
                                              int bagId,
                                              int bagIndex,
                                              List<int> lockList,
                                              List<int> attrId,
                                              List<int> attrValue)
        {
            //参数检查
            if (lockList.Count != 6)
            {
                return ErrorCodes.Error_DataOverflow;
            }
            //参数整理
            var theItem = character.GetItemByBagByIndex(bagId, bagIndex);
            if (theItem == null)
            {
                Logger.Warn("SuperExcellentEquip bagId({0})[{1}] is Empty!", bagId, bagIndex);
                return ErrorCodes.Error_ItemNotFind;
            }
            var theEquip = theItem as ItemEquip2;
            if (theEquip == null)
            {
                Logger.Warn("SuperExcellentEquip bagId({0})[{1}] is not Equip[{2}]!", bagId, bagIndex, theItem.GetId());
                return ErrorCodes.Error_ItemIsNoEquip;
            }
            var tbItem = Table.GetItemBase(theEquip.GetId());
            var tbEquip = Table.GetEquip(tbItem.Exdata[0]);
            var tbExcellent = Table.GetEquipExcellent(tbEquip.Ladder);
            if (tbExcellent == null)
            {
                return ErrorCodes.Error_EquipExcellentID;
            }
            if (tbEquip.RandomAttrCount == -1 || tbEquip.NewRandomAttrCount == -1)//如果是-1的是不能随灵的，包括物品类型是23700的装备(这类装备有多条紫色属性)
            {
                return ErrorCodes.Error_EquipLevelTooHigh;
            }

            //锁定条数
            var BlockCount = lockList.GetValueCount(1);
            //当前紫色附加属性条数
            var PurpleAttrCount = theEquip.GetPurpleAttrCount();
            if (BlockCount >= PurpleAttrCount)
            {
                return ErrorCodes.Error_EquipLockMax;
            }

            //条件限制
            if (character.mBag.GetRes(eResourcesType.GoldRes) < tbExcellent.Money[BlockCount])
            {
                return ErrorCodes.MoneyNotEnough;
            }
            if (character.mBag.GetItemCount(tbExcellent.ItemId) < tbExcellent.ItemCount)
            {
                return ErrorCodes.ItemNotEnough;
            }
            if (BlockCount > 0)
            {
                var itemCount = tbExcellent.LockCount[BlockCount - 1];
                if (character.mBag.GetItemCount(tbExcellent.LockId) < itemCount)
                {
                    return ErrorCodes.ItemNotEnough;
                }
            }
            //消耗道具
            character.mBag.DeleteItem((int) eResourcesType.GoldRes, tbExcellent.Money[BlockCount], eDeleteItemType.SuperExcellentEquip);
            character.mBag.DeleteItem(tbExcellent.ItemId, tbExcellent.ItemCount, eDeleteItemType.SuperExcellentEquip);
            if (BlockCount > 0)
            {
                character.mBag.DeleteItem(tbExcellent.LockId, tbExcellent.LockCount[BlockCount - 1],eDeleteItemType.SuperExcellentEquip);
            }
            //执行
            character.AddExData((int) eExdataDefine.e306, 1);

            theEquip.RandNewPurpleAttr(tbEquip, tbEquip.NewRandomAttrCount, lockList, attrId, attrValue);
            theEquip.SetBinding();

            //if (bagId != (int)eBagType.Equip)
            //{
            //    character.EquipChange(2, bagId, bagIndex, theEquip);
            //}

            try
            {
                character.mOperActivity.OnSuperExcellentEquip();
            }
            catch (Exception e)
            {
                Logger.Fatal("OnSuperExcellentEquip {0}\n{1}", e.Message, e.StackTrace);
            }

            return ErrorCodes.OK;
        }

        //传承装备属性
        public ErrorCodes SmritiEquip(CharacterController character,
                                      int smritiType,
                                      int moneyType,
                                      int fromBagType,
                                      int fromBagIndex,
                                      int toBagType,
                                      int toBagIndex,
                                      ref int appendCount)
        {
            //参数整理1
            var FromItem = character.GetItemByBagByIndex(fromBagType, fromBagIndex);
            if (FromItem == null)
            {
                Logger.Warn("SmritiEquip form bagId({0})[{1}] is Empty!", fromBagType, fromBagIndex);
                return ErrorCodes.Error_ItemNotFind;
            }
            var FromEquip = FromItem as ItemEquip2;
            if (FromEquip == null)
            {
                Logger.Warn("SmritiEquip form bagId({0})[{1}] is not Equip[{2}]!", fromBagType, fromBagIndex,
                    FromItem.GetId());
                return ErrorCodes.Error_ItemIsNoEquip;
            }
            //参数整理2
            var ToItem = character.GetItemByBagByIndex(toBagType, toBagIndex);
            if (ToItem == null)
            {
                Logger.Warn("SmritiEquip to bagId({0})[{1}] is Empty!", toBagType, toBagIndex);
                return ErrorCodes.Error_ItemNotFind;
            }
            var ToEquip = ToItem as ItemEquip2;
            if (ToEquip == null)
            {
                Logger.Warn("SmritiEquip to bagId({0})[{1}] is not Equip[{2}]!", toBagType, toBagIndex, ToItem.GetId());
                return ErrorCodes.Error_ItemIsNoEquip;
            }
            var tbFromItem = Table.GetItemBase(FromItem.GetId());
            var tbToItem = Table.GetItemBase(ToItem.GetId());

            var smritResult = CheckSmritEquip(tbFromItem, tbToItem);
            if (smritResult == ErrorCodes.Error_EquipTypeNotSame)
            {
                return ErrorCodes.Error_EquipTypeNotSame;
            }
            switch (smritiType)
            {
                case 0:
                {
//强化传承
                    var FromLevel = FromEquip.GetExdata(0);
                    var tbBlessing = Table.GetEquipBlessing(FromLevel);
                    if (tbBlessing == null)
                    {
                        return ErrorCodes.Error_EquipBlessingID;
                    }
                    var ToLevel = ToEquip.GetExdata(0);
                    if (FromLevel <= ToLevel)
                    {
                        return ErrorCodes.Error_EquipLevelTooHigh;
                    }
                    var tbToEquip = Table.GetEquip(tbToItem.Exdata[0]);
                    if (ToLevel >= tbToEquip.MaxLevel)
                    {
                        return ErrorCodes.Error_EquipLevelTooHigh;
                    }
                    if (moneyType == 0)
                    {
                        if (character.mBag.GetRes(eResourcesType.GoldRes) < tbBlessing.SmritiMoney)
                        {
                            return ErrorCodes.MoneyNotEnough;
                        }
                        character.mBag.DeleteItem((int) eResourcesType.GoldRes, tbBlessing.SmritiMoney,
                            eDeleteItemType.SmritiEquip);
                    }
                    else
                    {
                        if (character.mBag.GetRes(eResourcesType.DiamondRes) < tbBlessing.SmritiGold)
                        {
                            return ErrorCodes.DiamondNotEnough;
                        }
                        character.mBag.DeleteItem((int) eResourcesType.DiamondRes, tbBlessing.SmritiGold,
                            eDeleteItemType.SmritiEquip);
                    }
                    if (FromLevel >= tbToEquip.MaxLevel)
                    {
                        FromLevel = tbToEquip.MaxLevel;
                    }
                    ToEquip.SetExdata(0, FromLevel);
                    FromEquip.SetExdata(0, 0);
                    appendCount = FromLevel;
                    if (toBagType != (int) eBagType.Equip && FromLevel >= 7)
                    {
                        character.CheckEquipEnhanceTitle();
                    }
                }
                    break;
                case 1:
                {
//追加传承
                    if (smritResult == ErrorCodes.OK)
                    {
                        var fromPoint = FromEquip.GetExdata(1);
                        var toPoint = ToEquip.GetExdata(1);
                        if (fromPoint <= toPoint)
                        {
                            return ErrorCodes.Error_EquipAdditionalTooHigh;
                        }
                        var tbToEquip = Table.GetEquip(tbToItem.Id);
                        if (toPoint >= tbToEquip.AddAttrMaxValue)
                        {
                            return ErrorCodes.Error_EquipAdditionalTooHigh;
                        }
                        var tbFromEquip = Table.GetEquip(tbFromItem.Id);
                        if (tbToEquip.AddAttrId != tbFromEquip.AddAttrId)
                        {
                            Logger.Error(
                                "SmritiEquip Error_EquipAdditionalTypeNotSame! fromitemid={0} toitemid={1}",
                                tbToEquip.Id, tbFromEquip.Id);
                            return ErrorCodes.Error_EquipAdditionalTypeNotSame;
                        }
                        var tbAdditional = Table.GetEquipAdditional1(tbFromEquip.AddIndexID);
                        if (tbAdditional == null)
                        {
                            Logger.Error(
                                "SmritiEquip Error_EquipAdditionalID! itemid={0} is  nAttrId={1} Value={2}!",
                                tbToEquip.Id, tbToEquip.AddAttrId, fromPoint);
                            return ErrorCodes.Error_EquipAdditionalID;
                        }
                        var addLevel = GetAdditionalTable1(tbAdditional, fromPoint);
                        if (moneyType == 0)
                        {
                            var needMoney =
                                Table.GetSkillUpgrading(tbAdditional.SmritiMoney).GetSkillUpgradingValue(addLevel);
                            if (character.mBag.GetRes(eResourcesType.GoldRes) < needMoney)
                            {
                                return ErrorCodes.MoneyNotEnough;
                            }
                            character.mBag.DeleteItem((int) eResourcesType.GoldRes, needMoney,
                                eDeleteItemType.SmritiEquip);
                        }
                        else
                        {
                            var needDiamond =
                                Table.GetSkillUpgrading(tbAdditional.SmritiDiamond).GetSkillUpgradingValue(addLevel);
                            if (character.mBag.GetRes(eResourcesType.DiamondRes) < needDiamond)
                            {
                                return ErrorCodes.DiamondNotEnough;
                            }
                            character.mBag.DeleteItem((int) eResourcesType.DiamondRes, needDiamond,
                                eDeleteItemType.SmritiEquip);
                        }
                        if (fromPoint > tbToEquip.AddAttrMaxValue)
                        {
                            fromPoint = tbToEquip.AddAttrMaxValue;
                        }
                        ToEquip.SetExdata(1, fromPoint);
                        ToEquip.SetExdata(25, FromEquip.GetExdata(25));
                        FromEquip.SetExdata(1, 0);
                        FromEquip.SetExdata(25, 0);
                        appendCount = fromPoint;
                    }
                    else
                    {
                        var fromPoint = FromEquip.GetExdata(1);
                        var toPoint = ToEquip.GetExdata(1);
                        var oldCount = FromEquip.GetExdata(25);
                        if (oldCount < 1)
                        {
                            //说明该装备没有追加过
                            return ErrorCodes.Error_EquipNoAdditionalNoSmrit;
                        }
                        var toCount = ToEquip.GetExdata(25);
                        if (toCount < 0)
                        {
                            toCount = 0;
                        }
                        if (oldCount <= toCount)
                        {
                            return ErrorCodes.Error_EquipAdditionalTooHigh;
                        }
                        var tbToEquip = Table.GetEquip(tbToItem.Id);
                        if (toPoint >= tbToEquip.AddAttrMaxValue)
                        {
                            return ErrorCodes.Error_EquipAdditionalTooHigh;
                        }
                        var tbFromEquip = Table.GetEquip(tbFromItem.Id);
                        if (tbToEquip.AddAttrId != tbFromEquip.AddAttrId)
                        {
                            Logger.Error(
                                "SmritiEquip Error_EquipAdditionalTypeNotSame! fromitemid={0} toitemid={1}",
                                tbToEquip.Id, tbFromEquip.Id);
                            return ErrorCodes.Error_EquipAdditionalTypeNotSame;
                        }
                        var tbAdditional = Table.GetEquipAdditional1(tbFromEquip.AddIndexID);
                        if (tbAdditional == null)
                        {
                            Logger.Error(
                                "SmritiEquip Error_EquipAdditionalID! itemid={0} is  nAttrId={1} Value={2}!",
                                tbToEquip.Id, tbToEquip.AddAttrId, fromPoint);
                            return ErrorCodes.Error_EquipAdditionalID;
                        }
                        var addLevel = GetAdditionalTable1(tbAdditional, fromPoint);
                        if (moneyType == 0)
                        {
                            var needMoney =
                                Table.GetSkillUpgrading(tbAdditional.SmritiMoney).GetSkillUpgradingValue(addLevel);
                            if (character.mBag.GetRes(eResourcesType.GoldRes) < needMoney)
                            {
                                return ErrorCodes.MoneyNotEnough;
                            }
                            character.mBag.DeleteItem((int) eResourcesType.GoldRes, needMoney,
                                eDeleteItemType.SmritiEquip);
                        }
                        else
                        {
                            var needDiamond =
                                Table.GetSkillUpgrading(tbAdditional.SmritiDiamond).GetSkillUpgradingValue(addLevel);
                            if (character.mBag.GetRes(eResourcesType.DiamondRes) < needDiamond)
                            {
                                return ErrorCodes.DiamondNotEnough;
                            }
                            character.mBag.DeleteItem((int) eResourcesType.DiamondRes, needDiamond,
                                eDeleteItemType.SmritiEquip);
                        }

                        //if (FromPoint > tbToEquip.AddAttrMaxValue)
                        //{
                        //    FromPoint = tbToEquip.AddAttrMaxValue;
                        //}
                        var newValue = EquipAdditionalEquipValue(tbToEquip, oldCount);
                        ToEquip.SetExdata(1, newValue);
                        ToEquip.SetExdata(25, oldCount);
                        FromEquip.SetExdata(1, 0);
                        FromEquip.SetExdata(25, 0);
                        appendCount = newValue;
                    }
                }
                    break;
                //case 2:
                //    {//绿色卓越传承
                //        //int FromMinCount = 0;
                //        int FromMaxCount = 0;
                //        for (int i = 0; i != 4; ++i)
                //        {
                //            int FromPoint = FromEquip.GetExdata(2 + i);
                //            int ToPoint = ToEquip.GetExdata(2 + i);
                //            if (FromPoint > ToPoint)
                //            {
                //                FromMaxCount++;
                //            }
                //            //if (FromPoint <= ToPoint && ToPoint!=0)
                //            //{
                //            //    FromMinCount++;
                //            //}
                //        }
                //        if (FromMaxCount == 0)
                //        {
                //            return ErrorCodes.Error_EquipSmritiExcellentMax;
                //        }
                //        var tbFromEquip = Table.GetEquip(tbFromItem.Exdata[0]);
                //        var tbToEquip = Table.GetEquip(tbToItem.Exdata[0]);
                //        EquipEnchantRecord tbEnchant = Table.GetEquipEnchant(tbToEquip.ExcellentAttrValue);
                //        if (tbEnchant == null)
                //        {
                //            return ErrorCodes.Unknow;
                //        }

                //        EquipExcellentRecord tbExcellent = Table.GetEquipExcellent(tbFromEquip.Ladder);
                //        if (tbExcellent == null)
                //        {
                //            return ErrorCodes.Error_EquipExcellentID;
                //        }
                //        if (moneyType == 0)
                //        {
                //            if (character.mBag.GetRes(eResourcesType.GoldRes) < tbExcellent.SmritiMoney)
                //            {
                //                return ErrorCodes.MoneyNotEnough;
                //            }
                //            character.mBag.DeleteItem((int)eResourcesType.GoldRes, tbExcellent.SmritiMoney, eDeleteItemType.SmritiEquip);
                //        }
                //        else
                //        {
                //            if (character.mBag.GetRes(eResourcesType.DiamondRes) < tbExcellent.SmritiGold)
                //            {
                //                return ErrorCodes.DiamondNotEnough;
                //            }
                //            character.mBag.DeleteItem((int)eResourcesType.DiamondRes, tbExcellent.SmritiGold, eDeleteItemType.SmritiEquip);
                //        }

                //        for (int i = 0; i != 4; ++i)
                //        {
                //            int FromPoint = FromEquip.GetExdata(2 + i);
                //            FromEquip.SetExdata(2 + i, 0);
                //            if (FromPoint > 0)
                //            {
                //                if (tbFromEquip.ExcellentAttrId[i] != tbToEquip.ExcellentAttrId[i])
                //                {
                //                    Logger.Error("Equip[{0}]-[{1}] ExcellentAttrId not Same", tbFromItem.Exdata[0], tbToItem.Exdata[0]);
                //                    continue;
                //                }
                //                int maxValue = ItemEquip2.GetExcellentMaxValue(tbEnchant, tbFromEquip.ExcellentAttrId[i]) * tbToEquip.AddAttrUpMaxValue / 100;
                //                if (FromPoint > maxValue)
                //                {
                //                    FromPoint = maxValue;
                //                }
                //                ToEquip.SetExdata(2 + i, FromPoint);
                //            }
                //        }
                //    }
                //    break;
                default:
                    return ErrorCodes.Unknow;
            }

            character.AddExData((int) eExdataDefine.e307, 1);
            if (fromBagType != (int) eBagType.Equip)
            {
                character.EquipChange(2, fromBagType, fromBagIndex, FromItem);
            }

            if (toBagType != (int) eBagType.Equip)
            {
                character.EquipChange(2, toBagType, toBagIndex, ToItem);
            }
            ToEquip.SetBinding();
            return ErrorCodes.OK;
        }

        //回收装备
        public ErrorCodes RecoveryEquip(CharacterController character, int Type, List<int> indexList)
        {
            var bag = character.GetBag(0);
            var result = new Dictionary<int, int>();
            var getItem = new Dictionary<int, int>();
            foreach (var index in indexList)
            {
                var item = bag.GetItemByIndex(index);
                if (item == null)
                {
                    return ErrorCodes.Error_ItemNotFind;
                }
                var tbItem = Table.GetItemBase(item.GetId());
                if (tbItem == null)
                {
                    return ErrorCodes.Error_ItemID;
                }
                if (Type == 0) // 出售装备
                {
                    var sell = tbItem.Sell;
                    if (sell > 0)
                    {
                        result.modifyValue(2, sell);
                    }
                    else
                    {
                        return ErrorCodes.Error_ItemNotSell;
                    }
                }
                else //回收装备
                {
                    if (tbItem.CallBackType <= 0 || tbItem.CallBackPrice <= 0)
                    {
                        return ErrorCodes.Error_NotCallBack;
                    }
                    result.modifyValue(tbItem.CallBackType, tbItem.CallBackPrice);
                }

                //添加物品强化列表
                if (item.GetExdata(0) > 0 && item.GetExdata(0) < 15)
                {
                    var tbBlessing = Table.GetEquipBlessing(item.GetExdata(0));
                    for (var i = 0; i < tbBlessing.CallBackItem.Length; i++)
                    {
                        if (tbBlessing.CallBackItem[i] == -1)
                        {
                            continue;
                        }
                        var isExist = false;
                        foreach (var j in getItem)
                        {
                            if (j.Key == tbBlessing.CallBackItem[i])
                            {
                                getItem[j.Key] += tbBlessing.CallBackCount[i];
                                isExist = true;
                                break;
                            }
                        }
                        if (!isExist)
                        {
                            getItem.modifyValue(tbBlessing.CallBackItem[i], tbBlessing.CallBackCount[i]);
                        }
                    }
                }

                //var tbItemBase = Table.GetItemBase(item.GetId());
                if (tbItem.Exdata[0] == -1)
                {
                    Logger.Error("equipId={0},itemId={1}", tbItem.Exdata[0], tbItem.Id);
                    return ErrorCodes.Error_EquipID;
                }
                var tbEquipBase = Table.GetEquip(tbItem.Exdata[0]);
                if (tbEquipBase == null)
                {
                    Logger.Error("equipId={0},itemId={1}", tbItem.Exdata[0], tbItem.Id);
                    return ErrorCodes.Error_EquipID;
                }
                //添加追加道具
                if (item.GetExdata(1) > tbEquipBase.AddAttrUpMaxValue)
                {
                    var tbEquipAdd = Table.GetEquipAdditional1(tbEquipBase.AddIndexID);
                    var Value = item.GetExdata(1);
                    var countIndex = GetAdditionalTable1(tbEquipAdd, Value);
                    var tbSkillUpdate2 = Table.GetSkillUpgrading(tbEquipAdd.CallBackCount);

                    var isExist = false;
                    foreach (var j in getItem)
                    {
                        if (j.Key == tbEquipAdd.CallBackItem)
                        {
                            getItem[j.Key] += tbSkillUpdate2.GetSkillUpgradingValue(countIndex);
                            isExist = true;
                            break;
                        }
                    }
                    if (!isExist)
                    {
                        getItem.modifyValue(tbEquipAdd.CallBackItem, tbSkillUpdate2.GetSkillUpgradingValue(countIndex));
                    }
                }
            }

            foreach (var index in indexList)
            {
                bag.ReduceCountByIndex(index, 1, eDeleteItemType.RecoveryEquip);
                //bag.CleanItemByIndex(index);
            }

            character.mBag.AddItemOrMail(111, getItem, null, eCreateItemType.RecoveryEquip);
            foreach (var i in result)
            {
                character.mBag.AddItem(i.Key, i.Value, eCreateItemType.RecoveryEquip);
            }
            character.SetFlag(2800);
            return ErrorCodes.OK;
        }

        public int GetAdditionalTable1(EquipAdditional1Record tbAdditional, int Value)
        {
            var tbskillup = Table.GetSkillUpgrading(tbAdditional.AddPropArea);
            var level = 0;
            var lValue = tbskillup.GetSkillUpgradingValue(level);
            while (lValue < Value)
            {
                level++;
                var newValue = tbskillup.GetSkillUpgradingValue(level);
                if (newValue == lValue)
                {
                    break;
                }
                lValue = newValue;
            }
            return level;
        }
    }

    public static class EquipEnchance2
    {
        private static IEquipEnchance2 mStaticImpl;

        static EquipEnchance2()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (EquipEnchance2),
                typeof (EquipEnchance2DefaultImpl),
                o => { mStaticImpl = (IEquipEnchance2) o; });
        }

        //确定使用绿色属性
        public static ErrorCodes ConfirmResetExcellentEquip(this CharacterController character,
                                                            int bagId,
                                                            int bagIndex,
                                                            int isOk)
        {
            return mStaticImpl.ConfirmResetExcellentEquip(character, bagId, bagIndex, isOk);
        }

        //确定使用星级属性
        public static ErrorCodes SaveSuperExcellentEquip(this CharacterController character,
                                                            int bagId,
                                                            int bagIndex,
                                                            int isOk)
        {
            return mStaticImpl.SaveSuperExcellentEquip(character, bagId, bagIndex, isOk);
        }
        //强化装备
        public static ErrorCodes EnchanceEquip(this CharacterController character,
                                               int bagId,
                                               int bagIndex,
                                               int blessing,
                                               int upRate,
                                               int costGoleBless,
                                               ref int Nextlevel)
        {
            return mStaticImpl.EnchanceEquip(character, bagId, bagIndex, blessing, upRate, costGoleBless, ref Nextlevel);
        }

        //追加装备
        public static ErrorCodes EquipAdditionalEquip(this CharacterController character,
                                                      int bagId,
                                                      int bagIndex,
                                                      ref int NextValue)
        {
            return mStaticImpl.EquipAdditionalEquip(character, bagId, bagIndex, ref NextValue);
        }

        //回收装备
        public static ErrorCodes RecoveryEquip(this CharacterController character, int Type, List<int> indexList)
        {
            return mStaticImpl.RecoveryEquip(character, Type, indexList);
        }

        //洗炼绿色属性
        public static ErrorCodes ResetExcellentEquip(this CharacterController character,
                                                     int bagId,
                                                     int bagIndex,
                                                     List<int> attrList)
        {
            return mStaticImpl.ResetExcellentEquip(character, bagId, bagIndex, attrList);
        }

        //传承装备属性
        public static ErrorCodes SmritiEquip(this CharacterController character,
                                             int smritiType,
                                             int moneyType,
                                             int fromBagType,
                                             int fromBagIndex,
                                             int toBagType,
                                             int toBagIndex,
                                             ref int appendCount)
        {
            return mStaticImpl.SmritiEquip(character, smritiType, moneyType, fromBagType, fromBagIndex, toBagType,
                toBagIndex, ref appendCount);
        }

        //洗灵魂属性
        public static ErrorCodes SuperExcellentEquip(this CharacterController character,
                                                     int bagId,
                                                     int bagIndex,
                                                     List<int> lockList,
                                                     List<int> attrId,
                                                     List<int> attrValue)
        {
            return mStaticImpl.SuperExcellentEquip(character, bagId, bagIndex, lockList, attrId, attrValue);
        }

        public static ErrorCodes RandEquipSkill(this CharacterController character,
                                       int bagId,
                                       int bagIndex,
                                       int itemId,
                                       ref int buffId)
        {
            return mStaticImpl.RandEquipSkill(character, bagId, bagIndex, itemId, ref buffId);
        }

        public static ErrorCodes UseEquipSkill(this CharacterController character,
                                       int bagId,
                                       int bagIndex,
                                       int type,
                                       ref int buffId)
        {
            return mStaticImpl.UseEquipSkill(character, bagId, bagIndex, type, ref buffId);
        }
    }
}