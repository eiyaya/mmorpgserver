#region using

using System.Collections.Generic;
using DataTable;
using NLog;
using Shared;

#endregion

namespace Logic
{
    public interface IEquipEnchance
    {
        /// <summary>
        ///     装备强化
        /// </summary>
        /// <param name="character">角色</param>
        /// <param name="EquipPoint">装备点</param>
        /// <param name="UseList">使用的装备在包裹的索引列表</param>
        /// <param name="IndexList">对应到需求材料第几个的列表</param>
        /// <param name="MoneyList">第几个是用钱买的</param>
        ItemBase DoEquipEnchance(CharacterController character,
                                 int EquipPoint,
                                 List<int> UseList,
                                 List<int> IndexList,
                                 List<int> MoneyList);

        void GetEquipEnchance(ItemEquip2 itemequip, CharacterController character, bool isMust = false);
        int GetEquipGiveExp(ItemBase itemBase, int maxExp);
        int GetEquipGiveExp(int nLevel, int nColor, int maxExp);
        int GetEquipNeedExp(int nLevel, int nColor);
        void ResetEquipLevel(CharacterController character, ItemEquip2 oldItem, ItemEquip2 newItem);
    }

    public class EquipEnchanceDefaultImpl : IEquipEnchance
    {
        private static readonly List<int> EquipTypes = new List<int> {7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18};
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region 常用对外接口

        //装备强化
        /// <summary>
        ///     装备强化
        /// </summary>
        /// <param name="character">角色</param>
        /// <param name="EquipPoint">装备点</param>
        /// <param name="UseList">使用的装备在包裹的索引列表</param>
        /// <param name="IndexList">对应到需求材料第几个的列表</param>
        /// <param name="MoneyList">第几个是用钱买的</param>
        public ItemBase DoEquipEnchance(CharacterController character,
                                        int EquipPoint,
                                        List<int> UseList,
                                        List<int> IndexList,
                                        List<int> MoneyList)
        {
            if (UseList.Count != IndexList.Count)
            {
                Logger.Warn("DoEquipEnchance UseList{0}!=IndexList{1}", UseList.Count, IndexList.Count);
                return null;
            }
            if (UseList.Count == 0 && MoneyList.Count == 0)
            {
                Logger.Warn("DoEquipEnchance UseList={0},MoneyList={1}", UseList.Count, MoneyList.Count);
                return null;
            }
            //条件整理
            var EnchanceEquip = character.GetItemByBagByIndex(EquipPoint, 0);
            if (EnchanceEquip == null)
            {
                Logger.Warn("DoEquipEnchance EquipPoint={0} is Empty!", EquipPoint);
                return null;
            }
            var tbItem = Table.GetItemBase(EnchanceEquip.GetId());
            var tbEquip = Table.GetEquip(tbItem.Exdata[0]);
            var nLevel = EnchanceEquip.GetExdata(0);
            if (nLevel >= tbEquip.MaxLevel)
            {
                Logger.Warn("DoEquipEnchance EquipPoint={0} is MaxLevel={1}!", EquipPoint, nLevel);
                return null; //装备已经到最大的强化等级了
            }
            var ItemList = new List<ItemBase>();
            foreach (var i in UseList)
            {
                var useitem = character.GetItemByBagByIndex((int) eItemType.Equip, i);
                if (useitem == null)
                {
                    Logger.Warn("DoEquipEnchance UseList={0} is Empty!", i);
                    return null;
                }
                if (useitem.GetId() == -1)
                {
                    Logger.Warn("DoEquipEnchance UseList={0} is Empty,ItemId={1}!", i, useitem.GetId());
                    return null;
                }
                ItemList.Add(useitem);
            }
            //消耗道具
            var nTotleValue = 0;
            var nIndex = 0;
            var nMaxExp = GetEquipNeedExp(nLevel, tbItem.Quality)/2;
            foreach (var itemBase in ItemList)
            {
                var tbitem = Table.GetItemBase(itemBase.GetId());
                if (tbitem == null)
                {
                    Logger.Warn("DoEquipEnchance UseList={0} is Empty,ItemId={1}!", nIndex, itemBase.GetId());
                    return null;
                }
                var nPart = tbitem.Type - 9993;
                var nUseLevel = tbitem.UseLevel;
                var nColor = tbitem.Quality;
                var nValue = GetEnchanceValue(nPart, nUseLevel, nColor);
                var nEquipValue = EnchanceEquip.GetExdata(22 + IndexList[nIndex]);
                if (nValue != nEquipValue)
                {
                    Logger.Warn("DoEquipEnchance UseList IndexList  Index={0},ItemId={1},EquipValue={2},Value={3}!",
                        nIndex, itemBase.GetId(), nEquipValue, nValue);
                    return null;
                }
                nTotleValue += GetEquipGiveExp(itemBase, nMaxExp);
                //character.mBag.mBags[(int)eItemType.Equip].CleanItemByIndex(UseList[nIndex]);
                character.mBag.mBags[(int) eItemType.Equip].ReduceCountByIndex(UseList[nIndex], 1,
                    eDeleteItemType.EnchanceEquip);
                nIndex++;
            }
            //消耗金钱
            foreach (var i in MoneyList)
            {
                var nEquipValue = EnchanceEquip.GetExdata(22 + i);
                var nPart = nEquipValue/10000;
                var nUseLevel = (nEquipValue%10000)/10;
                var nColor = nEquipValue%10;
                nTotleValue += GetEquipGiveExp(nUseLevel, nColor, nMaxExp);
                var ItemId = GetDefaultEquip(character, nPart, nUseLevel, nColor);
                var tbDefaultItem = Table.GetItemBase(ItemId);
                if (tbDefaultItem == null)
                {
                    Logger.Warn("DoEquipEnchance MoneyList not find ItemId={0}!", ItemId);
                    return null;
                }
                if (tbDefaultItem.BuyNeedType >= 0)
                {
                    character.mBag.DeleteItem(tbDefaultItem.BuyNeedType, tbDefaultItem.BuyNeedCount,
                        eDeleteItemType.EnchanceEquip);
                }
            }
            //计算概率
            var nTotleGiveValue = 0;
            var nLevelUp = 0;
            CalculationEnchance(ref nLevelUp, ref nTotleGiveValue, nTotleValue, nLevel, tbItem, tbEquip.MaxLevel);
            Logger.Info(
                "EquipEnchance is Successful! ItemId={0},OldLevel={1},NewLevel={2},nLevelUp={3},nTotleGiveValue={4}",
                tbItem.Id, nLevel, nLevel + nLevelUp, nLevelUp, nTotleGiveValue);
            //计算结果
            EnchanceEquip.SetExdata(0, nLevel + nLevelUp);
            EnchanceEquip.MarkDirty();
            AddEquipTypeEnchanceValue(character, tbItem.Type, nTotleGiveValue);
            //装备等级发生改变了
            if (nLevelUp > 0)
            {
                character.EquipChange(2, EquipPoint, 0, EnchanceEquip);
            }
            GetEquipEnchance((ItemEquip2) EnchanceEquip, character, true);
            return EnchanceEquip;
        }

        //更换装备时新的等级计算
        public void ResetEquipLevel(CharacterController character, ItemEquip2 oldItem, ItemEquip2 newItem)
        {
            var tbOldItem = ItemBase.GetTableItem(oldItem.GetId());
            if (tbOldItem == null)
            {
                Logger.Warn("ResetEquipLevel OldEquip[{0}] not find by Table", oldItem.GetId());
                return;
            }
            var tbNewItem = ItemBase.GetTableItem(newItem.GetId());
            if (tbNewItem == null)
            {
                Logger.Warn("ResetEquipLevel NewEquip[{0}] not find by Table", newItem.GetId());
                return;
            }
            if (tbOldItem.Type != tbNewItem.Type)
            {
                Logger.Warn("ResetEquipLevel OldEquip[{0}] NewEquip[{1}] Type is Different", oldItem.GetId(),
                    newItem.GetId());
                return;
            }
            var nValue = GetEquipAddValue(character, tbOldItem.Type);


            //计算概率
            var nTotleGiveValue = 0;
            var nLevelUp = 0;

            var tbNewEquip = ItemEquip2.GetTableEquip(newItem.GetId());
            if (tbNewEquip == null)
            {
                Logger.Warn("ResetEquipLevel NewEquip[{0}] not find by Table", newItem.GetId());
                return;
            }
            CalculationEnchance(ref nLevelUp, ref nTotleGiveValue, nValue, 0, tbNewItem, tbNewEquip.MaxLevel);

            //--todo  临时取消了
            //oldItem.SetLevel(0);
            //newItem.SetLevel(nLevelUp);
        }

        #endregion

        #region 价值相关

        //获得某个类型的装备强化继承价值
        private int GetEquipAddValue(CharacterController character, int nType)
        {
            var nIndex = nType - 9900;
            return character.GetExData(nIndex);
        }

        //增加玩家装备位置上的强化继承价值
        private void AddEquipTypeEnchanceValue(CharacterController character, int nType, int nValue)
        {
            var nIndex = nType - 9900;
            character.AddExData(nIndex, nValue);
            //switch (nType)
            //{
            //    case 10000:
            //    case 10001:
            //    case 10002:
            //    case 10003:
            //    case 10004:
            //    case 10005:
            //    case 10006:
            //    case 10007:
            //    case 10008:
            //    case 10009:
            //    case 10010:
            //    case 10011:
            //    case 10012:
            //    case 10013:
            //    {
            //    }
            //        break;
            //    default:
            //        break;
            //}
        }

        #endregion

        #region 材料相关

        //初始化强化需求材料
        public void GetEquipEnchance(ItemEquip2 itemequip, CharacterController character, bool isMust = false)
        {
            var tbItem = Table.GetItemBase(itemequip.GetId());
            if (!isMust)
            {
                if (itemequip.GetExdata(22) != -1)
                {
                    return;
                }
                itemequip.MarkDirty();
            }
            Logger.Info("---GetEquipEnchance---Type={0},ItemId={1},Level={2}", tbItem.Type, itemequip.GetId(),
                itemequip.GetExdata(0));
            //var tbEquip = Table.GetEquip(tbItem.Exdata[0]);
            var nNeedLevel = tbItem.UseLevel;
            //int nColor = tbItem.Color;
            var nLevel = itemequip.GetExdata(0);
            var tbenchance = Table.GetEquipEnchance(nLevel);
            var tbladder = Table.GetEquipEnchance(nNeedLevel);


            for (var i = 0; i != 6; ++i)
            {
                var nNewLevel = nNeedLevel + (RandomEquipLevel(tbladder) - 3)*5;
                var nNewType = RandomEquipType(tbenchance);
                var nNewColor = RandomEquipColor(tbenchance);
                //参数构造： 装备类型*10000 + 装备等级 * 10 + 颜色
                var nValue = GetEnchanceValue(nNewType, nNewLevel, nNewColor);
                itemequip.SetExdata(22 + i, nValue);
            }
        }

        //获得装备强化材料的参数
        private int GetEnchanceValue(int nType, int nUseLevel, int nColor)
        {
            return nType*10000 + nUseLevel*10 + nColor;
        }

        #endregion

        #region 经验相关

        //获得装备的提供强化经验
        public int GetEquipGiveExp(ItemBase itemBase, int maxExp)
        {
            var tbitem = Table.GetItemBase(itemBase.GetId());
            var nNeedLevel = tbitem.UseLevel;
            var tbenchance = Table.GetEquipEnchance(nNeedLevel);
            if (tbenchance == null)
            {
                Logger.Warn("GetEquipGiveExp nNeedLevel={0} not find!", nNeedLevel);
                return 0;
            }
            var nColor = tbitem.Quality;
            if (nColor < 0 || nColor >= tbenchance.Value.Length)
            {
                Logger.Warn("GetEquipGiveExp ItemId={0} GiveExp Error!", itemBase.GetId());
                return 0;
            }
            return tbenchance.Value[nColor] > maxExp ? maxExp : tbenchance.Value[nColor];
        }


        //获得装备的提供强化经验
        public int GetEquipGiveExp(int nLevel, int nColor, int maxExp)
        {
            var tbenchance = Table.GetEquipEnchance(nLevel);
            if (tbenchance == null)
            {
                Logger.Warn("GetEquipGiveExp nNeedLevel={0} not find!", nLevel);
                return 0;
            }
            if (nColor < 0 || nColor >= tbenchance.Value.Length)
            {
                Logger.Warn("GetEquipGiveExp Color={0} GiveExp Error!", nColor);
                return 0;
            }
            return tbenchance.Value[nColor] > maxExp ? maxExp : tbenchance.Value[nColor];
        }

        //获得装备的强化需求经验
        public int GetEquipNeedExp(int nLevel, int nColor)
        {
            var tbenchance = Table.GetEquipEnchance(nLevel);
            if (tbenchance == null)
            {
                Logger.Warn("GetEquipNeedExp nLevel={0} not find!", nLevel);
                return 99999;
            }
            if (nColor < 0 || nColor >= tbenchance.Need.Length)
            {
                Logger.Warn("GetEquipNeedExp nLevel={0} , nColor ={1} Index is out!", nLevel, nColor);
                return 99999;
            }
            return tbenchance.Need[nColor];
        }

        #endregion

        #region 私有接口

        //随机装备类型
        private int RandomEquipType(EquipEnchanceRecord tbenchance)
        {
            //int result = MyRandom.Random(13);
            var nType = EquipTypes.Range();
            if (nType == 0)
            {
                Logger.Error("RandomEquipType is error type={0}", nType);
                return 7;
            }
            if (!EquipTypes.Contains(nType))
            {
                Logger.Error("RandomEquipType is error type={0}", nType);
                return 8;
            }
            return nType;
        }

        //随机颜色
        private int RandomEquipColor(EquipEnchanceRecord tbenchance)
        {
            var nRnd = MyRandom.Random(100);
            var nTotle = 0;
            for (var i = 0; i != tbenchance.Color.Length; ++i)
            {
                nTotle += tbenchance.Color[i];
                if (nRnd < nTotle)
                {
                    return i;
                }
            }
            return 0;
        }

        //随机等级
        private int RandomEquipLevel(EquipEnchanceRecord tbladder)
        {
            var nRnd = MyRandom.Random(100);
            var nTotle = 0;
            for (var i = 0; i != tbladder.Level.Length; ++i)
            {
                nTotle += tbladder.Level[i];
                if (nRnd < nTotle)
                {
                    return i;
                }
            }
            return 0;
        }

        //强化计算要到多少级
        /// <summary>
        ///     强化计算要到多少级
        /// </summary>
        /// <param name="nLevelUp">应该升多少级</param>
        /// <param name="nTotleGiveValue">继承的总经验</param>
        /// <param name="nAddValue">本次的装备价值</param>
        /// <param name="nLevel">当前的强化等级</param>
        /// <param name="tbItem">物品表</param>
        /// <param name="nMaxLevel">当前的最大等级</param>
        private void CalculationEnchance(ref int nLevelUp,
                                         ref int nTotleGiveValue,
                                         int nAddValue,
                                         int nLevel,
                                         ItemBaseRecord tbItem,
                                         int nMaxLevel)
        {
            var nNeedValue = GetEquipNeedExp(nLevel, tbItem.Quality);
            while (nAddValue > nNeedValue)
            {
                nAddValue -= nNeedValue;
                nTotleGiveValue += nNeedValue;
                nLevelUp++;
                if (nLevel + nLevelUp >= nMaxLevel)
                {
                    return;
                }
                nNeedValue = GetEquipNeedExp(nLevel + 1, tbItem.Quality);
            }
            var nRnd = MyRandom.Random(nNeedValue);
            Logger.Info("EnchancePro {0:F2}%", (nLevelUp + ((float) nAddValue)/nNeedValue)*100);
            if (nRnd < nAddValue)
            {
                nLevelUp++;
                nTotleGiveValue += nNeedValue;
            }
        }

        //根据值来获得一个默认的装备ID
        private int GetDefaultEquip(CharacterController character, int nType, int nUseLevel, int nColor)
        {
            //nType = nType - 9993;
            var ItemId = 100000;
            switch (nType)
            {
                //  不区分职业
                case 8:
                case 9:
                case 13:
                case 17:
                {
                    ItemId = ItemId + nType*1000 + nColor*100 + nUseLevel/5;
                }
                    break;
                //  区分职业
                case 7:
                case 10:
                case 11:
                case 12:
                case 14:
                case 15:
                case 16:
                case 18:
                case 19:
                case 20:
                {
                    var RoleId = character.GetRole();
                    ItemId = ItemId + (RoleId + 1)*100000 + nType*1000 + nColor*100 + nUseLevel/5;
                }
                    break;
                default:
                    break;
            }
            //--todo
            {
                var tbItem = Table.GetItemBase(ItemId);
                if (tbItem == null)
                {
                    Logger.Warn("GetItemBase not find!ItemId={0},nType={1},nUseLevel={2}, nColor={3},RoleId={4}", ItemId,
                        nType, nUseLevel, nColor, character.GetRole());
                }
            }
            return ItemId;
        }

        #endregion
    }

    public static class EquipEnchance
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static IEquipEnchance mImpl;

        static EquipEnchance()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (EquipEnchance), typeof (EquipEnchanceDefaultImpl),
                o => { mImpl = (IEquipEnchance) o; });
        }

        #region 材料相关

        //初始化强化需求材料
        public static void GetEquipEnchance(ItemEquip2 itemequip, CharacterController character, bool isMust = false)
        {
            mImpl.GetEquipEnchance(itemequip, character, isMust);
        }

        #endregion

        #region 常用对外接口

        //装备强化
        /// <summary>
        ///     装备强化
        /// </summary>
        /// <param name="character">角色</param>
        /// <param name="EquipPoint">装备点</param>
        /// <param name="UseList">使用的装备在包裹的索引列表</param>
        /// <param name="IndexList">对应到需求材料第几个的列表</param>
        /// <param name="MoneyList">第几个是用钱买的</param>
        public static ItemBase DoEquipEnchance(this CharacterController character,
                                               int EquipPoint,
                                               List<int> UseList,
                                               List<int> IndexList,
                                               List<int> MoneyList)
        {
            return mImpl.DoEquipEnchance(character, EquipPoint, UseList, IndexList, MoneyList);
        }

        //更换装备时新的等级计算
        public static void ResetEquipLevel(this CharacterController character, ItemEquip2 oldItem, ItemEquip2 newItem)
        {
            mImpl.ResetEquipLevel(character, oldItem, newItem);
        }

        #endregion

        #region 经验相关

        //获得装备的提供强化经验
        public static int GetEquipGiveExp(ItemBase itemBase, int maxExp)
        {
            return mImpl.GetEquipGiveExp(itemBase, maxExp);
        }

        //获得装备的提供强化经验
        public static int GetEquipGiveExp(int nLevel, int nColor, int maxExp)
        {
            return mImpl.GetEquipGiveExp(nLevel, nColor, maxExp);
        }

        //获得装备的强化需求经验
        public static int GetEquipNeedExp(int nLevel, int nColor)
        {
            return mImpl.GetEquipNeedExp(nLevel, nColor);
        }

        #endregion
    }
}