#region using

using System;
using DataTable;
using Shared;

#endregion

namespace Logic
{
    public interface IBuySome
    {
        ErrorCodes BuyP1vP1CD(CharacterController character);
        ErrorCodes BuyP1vP1Count(CharacterController character);
    }

    public class BuySomeDefaultImpl : IBuySome
    {
        //购买P1vP1次数
        public ErrorCodes BuyP1vP1Count(CharacterController character)
        {
            var nVipLevel = character.mBag.GetRes(eResourcesType.VipLevel);
            var tbVip = Table.GetVIP(nVipLevel);
            var buyCount = character.GetExData((int) eExdataDefine.e99);
            if (buyCount + tbVip.PKBuyCount < 1)
            {
                return ErrorCodes.Error_CountNotEnough;
            }
            var oldCount = character.GetExData((int) eExdataDefine.e98);
            if (oldCount > 0)
            {
                return ErrorCodes.Unknow;
            }
            var oldRes = character.mBag.GetRes(eResourcesType.DiamondRes);
            var tbUpgrade = Table.GetSkillUpgrading(19999);
            var price = tbUpgrade.GetSkillUpgradingValue(BuySome.MaxBuyCount - buyCount);
            if (oldRes < price)
            {
                return ErrorCodes.DiamondNotEnough;
            }
            //消耗
            character.mBag.DelRes(eResourcesType.DiamondRes, price, eDeleteItemType.BuyP1vP1Count);
            //character.mBag.SetRes(eResourcesType.DiamondRes, oldRes - price);
            //执行
            character.SetExData(98, oldCount + 1);
            character.SetExData(99, buyCount - 1);
            return ErrorCodes.OK;
        }

        //购买天梯CD
        public ErrorCodes BuyP1vP1CD(CharacterController character)
        {
            var oldRes = character.mBag.GetRes(eResourcesType.DiamondRes);
            //var tbUpgrade = Table.GetSkillUpgrading(19999);
            var tbServerConfig = Table.GetServerConfig(203);
            var price = 0;
            int.TryParse(tbServerConfig.Value, out price);
            if (oldRes < price)
            {
                return ErrorCodes.DiamondNotEnough;
            }
            //消耗
            //character.mBag.SetRes(eResourcesType.DiamondRes, oldRes - price);
            character.mBag.DelRes(eResourcesType.DiamondRes, price, eDeleteItemType.BuyP1vP1CD);
            character.lExdata64.SetTime(Exdata64TimeType.P1vP1CoolDown, DateTime.Now);
            return ErrorCodes.OK;
        }
    }

    public static class BuySome
    {
        private static readonly ExdataRecord mExdataRecord = Table.GetExdata(99);
        private static IBuySome mStaticImpl;

        static BuySome()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (BuySome), typeof (BuySomeDefaultImpl),
                o => { mStaticImpl = (IBuySome) o; });
        }

        public static int MaxBuyCount
        {
            get { return mExdataRecord.InitValue; }
        }

        //购买天梯CD
        public static ErrorCodes BuyP1vP1CD(this CharacterController character)
        {
            return mStaticImpl.BuyP1vP1CD(character);
        }

        //购买P1vP1次数
        public static ErrorCodes BuyP1vP1Count(this CharacterController character)
        {
            return mStaticImpl.BuyP1vP1Count(character);
        }
    }
}