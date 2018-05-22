#region using

using System.Collections.Generic;
using DataTable;
using EventSystem;
using NLog;
using Shared;

#endregion

namespace Logic
{
    public interface IItemCompose
    {
        ErrorCodes ComposeItem(CharacterController character, int id, int count, ref int rewardId);
    }

    public class ItemComposeDefaultImpl : IItemCompose
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ErrorCodes ComposeItem(CharacterController character, int id, int count, ref int rewardId)
        {
            var tbCompose = Table.GetItemCompose(id);
            if (tbCompose == null)
            {
                return ErrorCodes.Error_ItemComposeID;
            }
            //黄金检查
            if (tbCompose.NeedRes > 0 && tbCompose.NeedRes < 10)
            {
                if (character.mBag.GetRes((eResourcesType) tbCompose.NeedRes) < tbCompose.NeedValue*count)
                {
                    return ErrorCodes.MoneyNotEnough;
                }
            }

            //物品数量检查
            for (var i = 0; i != tbCompose.NeedId.Length; ++i)
            {
                var itemId = tbCompose.NeedId[i];
                var itemCount = tbCompose.NeedCount[i]*count;
                if (itemId == -1)
                {
                    continue;
                }
                if (character.mBag.GetItemCount(itemId) < itemCount)
                {
                    return ErrorCodes.ItemNotEnough;
                }
            }
            //奖励物品
            var items = new Dictionary<int, int>();
            var RewardId = -1;
            for (var i = 0; i != count; ++i)
            {
                var r = MyRandom.Random(10000);
                var tr = 0;
                for (var j = 0; j != 8; ++j)
                {
                    tr += tbCompose.ProId[j];
                    if (r < tr)
                    {
                        RewardId = tbCompose.ComposeId[j];
                        items.Add(tbCompose.ComposeId[j], count);
                        break;
                    }
                }
            }
            //包裹空位是否足够

            //ErrorCodes result= BagManager.CheckAddItemList(character.mBag, items);
            var result = character.mBag.CheckAddItem(RewardId, count);
            if (result != ErrorCodes.OK)
            {
                return result;
            }

            //执行
            if (tbCompose.NeedRes > 0 && tbCompose.NeedRes < 10)
            {
                character.mBag.DeleteItem(tbCompose.NeedRes, tbCompose.NeedValue*count, eDeleteItemType.Compose);
            }

            for (var i = 0; i != tbCompose.NeedId.Length; ++i)
            {
                var itemId = tbCompose.NeedId[i];
                var itemCount = tbCompose.NeedCount[i]*count;
                if (itemId == -1)
                {
                    continue;
                }
                character.mBag.DeleteItem(itemId, itemCount, eDeleteItemType.Compose);
            }
            var isTrue = false;
            if (character.GetExData((int) eExdataDefine.e342) == 0)
            {
                isTrue = true;
            }
            else
            {
                var pro = tbCompose.Pro /*+ character.mCity.GetComposePro()*/;
                if (MyRandom.Random(10000) < pro)
                {
                    isTrue = true;
                }
            }
            if (isTrue)
            {
                character.mBag.AddItem(RewardId, count, eCreateItemType.Compose);
                rewardId = RewardId;
                var e = new ComposeItemEvent(character, id, RewardId);
                EventDispatcher.Instance.DispatchEvent(e);

                character.BroadCastGetEquip(RewardId, 100002167);
				/*
                var build = character.mCity.GetBuildByType((int) BuildingType.CompositeHouse);
                if (build != null)
                {
                    build.GiveReward(407, 1, build.TbBs, 121, eCreateItemType.Compose);
                    var addexp = build.TbBs.Param[2];
                    var okCount = character.GetExData((int) eExdataDefine.e556);
                    if (okCount < build.TbBs.Param[3])
                    {
                        character.mCity.CityAddExp(addexp);
                        character.AddExData((int) eExdataDefine.e556, 1);
                    }
                }
				 * */
            }
            //var build=character.mCity.GetBuildByType((int) BuildingType.CompositeHouse);

            character.AddExData((int) eExdataDefine.e342, 1);
            character.AddExData((int) eExdataDefine.e415, 1);
            return ErrorCodes.OK;
        }
    }

    public static class ItemCompose
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static IItemCompose mImpl;

        static ItemCompose()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (ItemCompose), typeof (ItemComposeDefaultImpl),
                o => { mImpl = (IItemCompose) o; });
        }

        public static ErrorCodes ComposeItem(this CharacterController character, int id, int count, ref int rewardId)
        {
            return mImpl.ComposeItem(character, id, count, ref rewardId);
        }
    }
}