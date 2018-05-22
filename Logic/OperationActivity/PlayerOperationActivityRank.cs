#region using

using System;
using System.Collections;
using System.Collections.Generic;
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
	public class PlayerOperationActivityRank : PlayerOperationActivity
    {
		public override OperationActivityType Type
		{
			get { return OperationActivityType.Rank; }
		}
		public RankType RankType { get; set; }
		public override void RefreshActivity(OperationActivity data)
		{
			base.RefreshActivity(data);
			var actRank = data as OperationActivityRank;

			if (eRechargeActivityOpenRule.NewServerAuto == data.OpenRule)
			{
                ScoreTime = DateTime.Parse(Table.GetServerName(Controller.serverId).OpenTime).AddHours(data.ScoreDelayHours).AddSeconds(30);
				
			}
			else if (eRechargeActivityOpenRule.LimitTime == data.OpenRule
				|| eRechargeActivityOpenRule.Last == data.OpenRule)
			{
				ScoreTime = actRank.ScoreTime;
			}
			
			if (null == actRank)
			{
				return;
			}
			foreach (var actItem in Items)
			{
				actItem.Need = 1;
                actItem.Desc = "【" + Table.GetDictionary(100002286).Desc[0] + "】";
				actItem.mDBData.Status = 0;
			}

			if (RankType.Level == RankType)
			{
				OnRankDataChange(RankType.Level,(long)Controller.mBag.GetRes(eResourcesType.LevelRes));
			}
			else if (RankType.Money == RankType)
			{
				OnRankDataChange(RankType.Money,
					(long)Controller.mBag.GetRes(eResourcesType.GoldRes));
			}


			if (actRank.HasResult)
			{
				MsgRankList list = null;
				if (!actRank.DictRankData.TryGetValue(Controller.serverId, out list))
				{
					return;
				}
				
				foreach (var actItem in Items)
				{
					actItem.mDBData.Status = 0;

					var needRankIdx = actItem.Params[0];
					if (-1 == needRankIdx)
					{
						if (mDBData.Param >= actItem.Params[1] &&
								-2 == Controller.CheckCondition(actItem.Condition))
						{
							actItem.mDBData.Status = 1;
						}
						else
						{
							actItem.mDBData.Status = 0;
						}
					}
					else
					{
						if (needRankIdx >= 0 && needRankIdx < list.Items.Count)
						{
							var rankData = list.Items[needRankIdx];
							actItem.Desc = string.Format("【{0}】",rankData.Name);

							if (rankData.CharacterId == Controller.mGuid &&
							    rankData.Value >= actItem.Params[1] &&
							    -2 == Controller.CheckCondition(actItem.Condition))
							{
								actItem.mDBData.Status = 1;
							}
							else
							{
								actItem.mDBData.Status = 0;
							}
						}
					}
				}
			}

		}

		public void OnRankDataChange(RankType type, long value)
		{
			if (RankType != type)
			{
				return;
			}

			if (value > mDBData.LongParam)
			{
				mDBData.LongParam = value;
				foreach (var actItem in Items)
				{
					if (-1 == actItem.Params[0] && mDBData.LongParam >= actItem.Params[1])
					{
						actItem.mDBData.Status = 1;
						actItem.MarkDirty();
					}
				}
				MarkDbDirty();	
			}
		}
    }


}