#region using

using DataContract;
using DataTable;

#endregion

namespace Shared
{
    public static class ShareItemFactory
    {
        //初始化（按初始配置）
        /// <summary>
        ///     创造物品
        /// </summary>
        /// <param name="nId">物品Id</param>
        /// <returns></returns>
        public static ItemBase Create(int nId, ItemBaseData Dbdata ,int Count = 1)
        {
            if (Table.GetItemBase(nId) == null)
            {
                return null;
            }
            var it = CheckGeneral.GetItemType(nId);
            switch (it)
            {
                case eItemType.Error:
                    return null;
                case eItemType.Resources:
                    return new ItemBase(nId, Dbdata);
                case eItemType.Equip:
                    return new ItemEquip2(nId, Dbdata);
                case eItemType.BaseItem:
                    return new ItemBase(nId, Dbdata , Count);
                case eItemType.Piece:
                    return new ItemBase(nId, Dbdata);
                case eItemType.Mission:
                    return new ItemBase(nId, Dbdata);
                case eItemType.Elf: //精灵
                    return new ElfItem(nId, Dbdata);
                case eItemType.Pet: //宠物
                    return new PetItem(nId, Dbdata);
                case eItemType.Wing: //翅膀
                    return new WingItem(nId, Dbdata);
                case eItemType.Astrology: //占星宝石
                    return new Astrology(nId, Dbdata);
                case eItemType.Medal: //勋章
                    return new MedalItem(nId, Dbdata);
                case eItemType.TreasureMap: //藏宝图
                    return new TreasureMap(nId, Dbdata);
                default:
                    return null;
            }
        }

        //初始化（用某块DB初始一个空的类型）
        public static ItemBase CreateByDb(ItemBaseData Dbdata)
        {
            //if (Table.GetItemBase(Dbdata.ItemId) == null)
            //{
            //    return null;
            //}
            var it = CheckGeneral.GetItemType(Dbdata.ItemId);
            switch (it)
            {
                case eItemType.Error:
                    return new ItemBase(Dbdata, false);
                case eItemType.Resources:
                    return null;
                case eItemType.Equip:
                    return new ItemEquip2(Dbdata, false);
                case eItemType.BaseItem:
                    return new ItemBase(Dbdata, false);
                case eItemType.Piece:
                    return new ItemBase(Dbdata, false);
                case eItemType.Mission:
                    return new ItemBase(Dbdata, false);
                case eItemType.Elf: //精灵
                    return new ElfItem(Dbdata, false);
                case eItemType.Pet: //宠物
                    return new PetItem(Dbdata, false);
                case eItemType.Wing: //翅膀
                    return new WingItem(Dbdata, false);
                case eItemType.Astrology: //占星宝石
                    return new Astrology(Dbdata, false);
                case eItemType.Medal: //勋章
                    return new MedalItem(Dbdata, false);
                case eItemType.TreasureMap: //藏宝图
                    return new TreasureMap(Dbdata, false);
                default:
                    return new ItemBase(Dbdata, false);
            }
        }

        //初始化（用某块DB初始一个空的类型）
        public static ItemBase CreateNull(int nId, ItemBaseData Dbdata)
        {
            if (Table.GetItemBase(nId) == null)
            {
                return null;
            }
            var it = CheckGeneral.GetItemType(nId);
            switch (it)
            {
                case eItemType.Error:
                    return null;
                case eItemType.Resources:
                    return null;
                case eItemType.Equip:
                    return new ItemEquip2(Dbdata);
                case eItemType.BaseItem:
                    return new ItemBase(Dbdata);
                case eItemType.Piece:
                    return new ItemBase(Dbdata);
                case eItemType.Mission:
                    return new ItemBase(Dbdata);
                case eItemType.Elf: //精灵
                    return new ElfItem(Dbdata);
                case eItemType.Pet: //宠物
                    return new PetItem(Dbdata);
                case eItemType.Wing: //翅膀
                    return new WingItem(Dbdata);
                case eItemType.Astrology: //占星宝石
                    return new Astrology(Dbdata);
                case eItemType.Medal: //勋章
                    return new MedalItem(Dbdata);
                case eItemType.TreasureMap: //藏宝图
                    return new TreasureMap(Dbdata);
                default:
                    return new ItemBase(Dbdata);
            }
        }
    }
}