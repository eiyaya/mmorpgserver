#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DataContract;
using DataTable;
using EventSystem;
using LogicServerService;
using NLog;
using Shared;

#endregion

namespace Logic
{
	public class PlayerOperationActivityLottery : PlayerOperationActivity
    {
		public override OperationActivityType Type
		{
			get { return OperationActivityType.Lottery; }
		}

		public readonly eResourcesType NeedItem = eResourcesType.DiamondRes;//钻石

		public int ResetNeedMoney = 1;
		public List<int> DrawLotteryLadder = new List<int>();

		private Random Rand = new Random();

		public override void RefreshActivity(OperationActivity data)
		{
			base.RefreshActivity(data);
			var lottery = data as OperationActivityLottery;
			ResetNeedMoney = lottery.ResetNeedMoney;
			DrawLotteryLadder = lottery.DrawLotteryLadder;
#if DEBUG
			Debug.Assert(DrawLotteryLadder.Count<Items.Count);
#endif
		}

		public override ErrorCodes AquireReward(int id, out int result)
		{
			result = -1;
			if (null == Controller)
			{
				return ErrorCodes.Unknow;
			}

			if (!IsActive)
			{
				return ErrorCodes.Error_Operation_NotInActivity;
			}

			if (0 == id)
			{//重置次数
				var current = Controller.mBag.GetRes(NeedItem);
				if (current < ResetNeedMoney)
				{
					return ErrorCodes.DiamondNotEnough;
				}

				var ret = Controller.mBag.DeleteItem((int)NeedItem, ResetNeedMoney, eDeleteItemType.OperationActivity, "LotteryReSet");
				if (ErrorCodes.OK != ret)
				{
					return ret;
				}

				mDBData.Param = 0;
				for (int i = 0; i < Items.Count; i++)
				{
					var item = Items[i];
					item.mDBData.Aquired = 0;
					item.MarkDirty();
				}
				MarkDirty();
			}
			else
			{//抽奖
				if (mDBData.Param >= DrawLotteryLadder.Count)
				{
					return ErrorCodes.Unknow;
				}

				var cost = DrawLotteryLadder[mDBData.Param];

				var current = Controller.mBag.GetRes(NeedItem);
				if (current < cost)
				{
					return ErrorCodes.DiamondNotEnough;
				}

				var ret = Controller.mBag.DeleteItem((int)NeedItem, cost, eDeleteItemType.OperationActivity, "LotterySpend");
				if (ErrorCodes.OK != ret)
				{
					return ret;
				}
				
				mDBData.Param++;
				MarkDirty();

				int totalRate = 0;
				for (int i = 0; i < Items.Count; i++)
				{
					var item = Items[i];
					if (item.AquiredTimes > 0)
					{
						continue;
					}

					totalRate += item.Params[0];
				}

				var r = Rand.Next(0, totalRate);
				int count = 0;

				var broadcastItemId = -1;
				for (int i = 0; i < Items.Count; i++)
				{
					var item = Items[i];
					if (item.AquiredTimes > 0)
					{
						continue;
					}

					count += item.Params[0];
					if (r > count)
					{
						continue;
					}

					var tb = Table.GetYunYing(item.Id);
					if (null == tb)
					{
						return ErrorCodes.Error_Operation_NotInActivity;
					}

					item.Aquire();

					var items = new Dictionary<int, int>();
					for (int j = 0; j < item.Rewards.Count; j++)
					{
						var reward = item.Rewards[j];
						if (-1 == reward.Id || reward.Count <= 0)
						{
							continue;
						}
						items.Add(reward.Id, reward.Count);
						if ( 1==item.Params[1] && -1 == broadcastItemId)
						{
							broadcastItemId = reward.Id;
						}
					}
					if (items.Count > 0)
					{
						Controller.mBag.AddItemOrMail(200, items, null, eCreateItemType.OperationActivity, tb.Name);
					}
					result = item.Id;
					break;
				}

				if (-1 != broadcastItemId)
				{
					var strs = new List<string>
					{
						Controller.GetName(),
						Utils.AddItemId(broadcastItemId)
					};
					var exData = new List<int>();
					var content = Utils.WrapDictionaryId(300000134, strs, exData);
					var chatAgent = LogicServer.Instance.ChatAgent;
					chatAgent.BroadcastWorldMessage((uint)Controller.serverId, (int)eChatChannel.WishingGroup, 0, string.Empty,
						new ChatMessageContent { Content = content });
				}
			}

			return ErrorCodes.OK;

		}
    }


}