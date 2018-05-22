using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataContract;
using DataTable;
using EventSystem;
using NLog;
using Shared;
using System.Diagnostics;



namespace Logic
{
	public class OperationActivityManager
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


		private static OperationActivityManager s_Instance = null;

		public static OperationActivityManager Instance
		{
			get
			{
				if (null == s_Instance)
				{
					s_Instance = new OperationActivityManager();
				}
				return s_Instance;
			}
		}

		public Dictionary<int, OperationActivity> DictActivity = new Dictionary<int, OperationActivity>();

		public bool Init()
		{
			InitEventListener();
			return Load();
		}
		public bool Load()
		{
			Cleanup();
			Table.ForeachOperationActivity((tb) =>
			{
				var openRule = (eRechargeActivityOpenRule)tb.openTimeRule;
				if (eRechargeActivityOpenRule.None == openRule)
				{
					return true;
				}

				OperationActivity act = null;
				var type = (OperationActivityType) tb.activityType;
				if (OperationActivityType.Guide == type)
				{
					act = new OperationActivityGuide();
				}
				else if (OperationActivityType.Recharge == type)
				{
					act = new OperationActivityRecharge();
				}
				else if (OperationActivityType.SpecialEvent == type)
				{
					act = new OperationActivitySpecialEvent();
				}
				else if (OperationActivityType.Investment == type)
				{
					act = new OperationActivityInvestment();
				}
				else if (OperationActivityType.Rank == type)
				{
					var rankAct = new OperationActivityRank();
					rankAct.RankType = (RankType)tb.activitySonType;
					act = rankAct;
				}
				else if (OperationActivityType.Lottery == type)
				{
					var actLottery = new OperationActivityLottery();
					actLottery.ResetNeedMoney = tb.Param[0];
					actLottery.DrawLotteryLadder = tb.StrParam;
					act = actLottery;
				}
				else
				{
					Logger.Fatal("unknown type tb.activityType");
					return true;
				}

				if ((OperationActivityUIType)tb.UIType == OperationActivityUIType.Table)
				{
					act.IsTitle = true;
				}

				Table.ForeachYunYing((tbYunYing) =>
				{
					if (1 != tbYunYing.IsOpen || tb.Id != tbYunYing.ParentType)
					{
						return true;	
					}

					var item = new OperationActivityItem();
					item.Id = tbYunYing.Id;
					item.Name = tbYunYing.Name;
					//item.Desc = tbYunYing.Desc;
					item.Params = tbYunYing.Param;
					item.StrParams = tbYunYing.StrParam;
					item.Condition = tbYunYing.ConditionId;
					//item.Begin = DateTime.MinValue;
					//item.End = DateTime.MaxValue;
					item.OpenDelayDays = tbYunYing.XiangDuiOpenServerTime;
					item.RewardOpenDelay = tbYunYing.RewardOpenTime;
					item.LastDays = tbYunYing.LastTime;
					item.TotalTimes = tbYunYing.LingQuTimes;

					item.Rewards[0] = ParseRewardItem(tbYunYing.JiangLiJob1);
					item.Rewards[1] = ParseRewardItem(tbYunYing.JiangLiJob2);
					item.Rewards[2] = ParseRewardItem(tbYunYing.JiangLiJob3);
					
					act.ItemList.Add(item);

					return true;
				});

				if (0 == act.ItemList.Count && !act.IsTitle)
				{
					return true;
				}

				act.Id = tb.Id;
				act.OpenRule = openRule;
				if (eRechargeActivityOpenRule.None == act.OpenRule)
				{//关闭
					return true;
				}
				else if (eRechargeActivityOpenRule.Last == act.OpenRule)
				{//持续活动
					act.StartTime = DateTime.Now;
					act.EndTime = DateTime.MaxValue;
					if (OperationActivityType.Rank == type)
					{
						act.ScoreTime = DateTime.Parse(tb.LastTime);	
					}
					
				}
				else if (eRechargeActivityOpenRule.LimitTime == act.OpenRule)
				{//固定时间活动
					act.StartTime = DateTime.Parse(tb.openTime);
					act.EndTime = DateTime.Parse(tb.closeTime);
					if (OperationActivityType.Rank == type)
					{
						act.ScoreTime = DateTime.Parse(tb.LastTime);
					}

				}
				else if (eRechargeActivityOpenRule.NewServerAuto == act.OpenRule)
				{//新开服
					act.DelayHours = int.Parse(tb.openTime);
					act.LastHours = int.Parse(tb.closeTime);
					act.ScoreDelayHours = int.Parse(tb.LastTime);
				}

				//act.CleanUpTime = DateTime.Parse(tb.CleanUpTime);
				act.ServerList = new List<int>();
				act.ServerList.AddRange(tb.activityServer);
				act.Param = tb.Param;
				

				act.Init();
				DictActivity.Add(act.Id, act);
				return true;
			});

			return true;
		}

		public void Tick()
		{
			foreach (var kv in DictActivity)
			{
				kv.Value.Update();
			}
		}

		public void Reload()
		{
            //重新加载其中任何一个表都会触发运营活动重新加载
		    //Table.ReloadTable("ServerName");
            //Table.ReloadTable("YunYing");
            //Table.ReloadTable("OperationActivity");

            bool dirty = false;
			try
			{
				Load();

				dirty = true;

			}
			catch (Exception e)
			{
#if DEBUG
				Debug.Assert(false, e.Message);
#endif
				Logger.Fatal("PlayerOperationActivityManager.Reload" + e.Message);

			}

			if (dirty)
			{
				MarkAllCharacterDirty();
			}
		}

		public void Cleanup()
		{
			foreach (var kv in DictActivity)
			{
				kv.Value.Destroy();
			}
			DictActivity.Clear();
		}
		public void MarkAllCharacterDirty(List<int> serverList=null)
		{
			if (null != CharacterManager.Instance.mDictionary)
			{
				foreach (var kv in CharacterManager.Instance.mDictionary)
				{
					try
					{
						if (null != kv.Value && null != kv.Value.Controller && null != kv.Value.Controller.mOperActivity)
						{
							if (null != serverList && serverList.Count > 0 && !serverList.Contains(kv.Value.Controller.serverId))
							{
								continue;
							}
							kv.Value.Controller.mOperActivity.NeedReset = true;
						}
					}
					catch (Exception e)
					{
						Logger.Fatal("PlayerOperationActivityManager.Reload dirty" + e.Message);
					}
				}
			}
		}
		public static void CalculateOpenServerTime(out DateTime t1, out DateTime t2, string s1, string s2, int serverId)
		{
			var tbServer = Table.GetServerName(serverId);
			var openTime = DateTime.Parse(tbServer.OpenTime);
			int startHour, endHour;
			if (int.TryParse(s1, out startHour) && int.TryParse(s2, out endHour))
			{
				t1 = openTime.AddHours(startHour);
				t2 = openTime.AddHours(endHour);
			}
			else
			{
				t1 = openTime;
				t2 = DateTime.MaxValue;
			}
		}

		public static List<OperationActivityReward> ParseRewardItem(string str)
		{
			var list = new List<OperationActivityReward>();
			var strList = str.Split('|');
			for (int i = 0; i < strList.Count(); i++)
			{
				var ret = strList[i].Split(',');
				if (2==ret.Count())
				{
					var reward = new OperationActivityReward
					{
						Id = int.Parse(ret[0]),
						Count = int.Parse(ret[1])
					};
					list.Add(reward);
				}
			}

			return list;
		}
		private void InitEventListener()
		{
			//EventDispatcher.Instance.AddEventListener(CharacterRechargeSuccessEvent.EVENT_TYPE, OnCharacterRechargeSuccessEvent);
			EventDispatcher.Instance.AddEventListener(ItemChange.EVENT_TYPE, OnItemChange);
			EventDispatcher.Instance.AddEventListener(KillMonster.EVENT_TYPE, OnKillMonster);
			EventDispatcher.Instance.AddEventListener(EnterArea.EVENT_TYPE, OnEnterArea);
			EventDispatcher.Instance.AddEventListener(TollgateFinish.EVENT_TYPE, OnTollgateFinish);
			EventDispatcher.Instance.AddEventListener(ChacacterFlagTrue.EVENT_TYPE, OnChacacterFlagTrue);
			EventDispatcher.Instance.AddEventListener(ChacacterFlagFalse.EVENT_TYPE, OnChacacterFlagFalse);
			EventDispatcher.Instance.AddEventListener(BuyItemEvent.EVENT_TYPE, OnBuyItemEvent);
			EventDispatcher.Instance.AddEventListener(EnhanceEquipEvent.EVENT_TYPE, OnEnhanceEquipEvent);
			EventDispatcher.Instance.AddEventListener(AdditionalEquipEvent.EVENT_TYPE, OnAdditionalEquipEvent);
			EventDispatcher.Instance.AddEventListener(UpgradeSkillEvent.EVENT_TYPE, OnUpgradeSkillEvent);
			EventDispatcher.Instance.AddEventListener(NpcServeEvent.EVENT_TYPE, OnNpcServeEvent);
			EventDispatcher.Instance.AddEventListener(AddFriendEvent.EVENT_TYPE, OnAddFriendEvent);
			EventDispatcher.Instance.AddEventListener(ComposeItemEvent.EVENT_TYPE, OnComposeItemEvent);
			EventDispatcher.Instance.AddEventListener(CharacterExdataAddEvent.EVENT_TYPE, OnCharacterExdataAddEvent);
			EventDispatcher.Instance.AddEventListener(AddTalentEvent.EVENT_TYPE, OnAddTalentEvent);
			EventDispatcher.Instance.AddEventListener(CharacterExdataChange.EVENT_TYPE, OnCharacterExdataChangeEvent);
		
			
		}

		#region event
// 		public void OnCharacterRechargeSuccessEvent(IEvent evt)
// 		{
// 			var e = (CharacterRechargeSuccessEvent)evt;
// 			if (null == e.character)
// 			{
// 				return;
// 			}
// 			e.character.mOperActivity.OnRechargeSuccess(e.Num);
// 		}

		public void OnItemChange(IEvent evt)
		{
			var e = (ItemChange)evt;
			var character = e.character;
			var item = e.mItemId;
			var count = e.mItemCount;
			if (null == e.character)
			{
				return;
			}
			character.mOperActivity.OnItemChange(item, count);
		}
		public void OnKillMonster(IEvent evt)
		{
			var e = (KillMonster)evt;
			var character = e.character;
			var monsterId = e.mMonsterId;
			if (null == e.character)
			{
				return;
			}
			character.mOperActivity.OnKillMonster(monsterId);
		}
		public void OnEnterArea(IEvent evt)
		{
			var e = (EnterArea)evt;
			if (null == e.character)
			{
				return;
			} 
			e.character.mOperActivity.OnEnterArea(e.mAreaId, e.mIsEnter);
			
		}
		public void OnTollgateFinish(IEvent evt)
		{
			var e = (TollgateFinish)evt;
			if (null == e.character)
			{
				return;
			}
			e.character.mOperActivity.OnTollgateFinish(e.TollgateId);
		}
		public void OnChacacterFlagTrue(IEvent evt)
		{
			var e = (ChacacterFlagTrue)evt;
			if (null == e.character)
			{
				return;
			}
			e.character.mOperActivity.OnChacacterFlagTrue(e.FlagId);
		}
		public void OnChacacterFlagFalse(IEvent evt)
		{
			var e = (ChacacterFlagFalse)evt;
			if (null == e.character)
			{
				return;
			}
			e.character.mOperActivity.OnChacacterFlagFalse(e.FlagId);
		}

		public void OnBuyItemEvent(IEvent evt)
		{
			var e = (BuyItemEvent)evt;
			if (null == e.character)
			{
				return;
			}
            e.character.mOperActivity.OnBuyItemEvent(e.ItemId, e.ItemCount, e.character);
		}

		public void OnEnhanceEquipEvent(IEvent evt)
		{
			var e = (EnhanceEquipEvent)evt;
			if (null == e.character)
			{
				return;
			}
			e.character.mOperActivity.OnEnhanceEquipEvent(e.EquipPart);
		}
		public void OnAdditionalEquipEvent(IEvent evt)
		{
			var e = (AdditionalEquipEvent)evt;
			if (null == e.character)
			{
				return;
			}
			e.character.mOperActivity.OnAdditionalEquipEvent(e.EquipPart);
		}
		public void OnUpgradeSkillEvent(IEvent evt)
		{
			var e = (UpgradeSkillEvent)evt;
			if (null == e.character)
			{
				return;
			}
			e.character.mOperActivity.OnUpgradeSkillEvent(e.SkillID);
		}
		public void OnNpcServeEvent(IEvent evt)
		{
			var e = (NpcServeEvent)evt;
			if (null == e.character)
			{
				return;
			}
			e.character.mOperActivity.OnNpcServeEvent(e.NpcServeID);
		}

		public void OnAddFriendEvent(IEvent evt)
		{
			var e = (AddFriendEvent)evt;
			if (null == e.character)
			{
				return;
			}
			e.character.mOperActivity.OnAddFriendEvent();
		}
		public void OnComposeItemEvent(IEvent evt)
		{
			var e = (ComposeItemEvent)evt;
			if (null == e.character)
			{
				return;
			}
			e.character.mOperActivity.OnComposeItemEvent(e.ComposeId,e.ItemId);
		}
		public void OnCharacterExdataAddEvent(IEvent evt)
		{
			var e = (CharacterExdataAddEvent)evt;
			if (null == e.character)
			{
				return;
			}
            
			e.character.mOperActivity.OnCharacterExdataAddEvent(e.ExdataId,e.AddValue);
		}
		public void OnCharacterExdataChangeEvent(IEvent evt)
		{
			var e = (CharacterExdataChange)evt;
			if (null == e.character)
			{
				return;
			}
            
			e.character.mOperActivity.OnCharacterExdataChangeEvent(e.ExdataId, e.ExdataValue);
		}

		public void OnAddTalentEvent(IEvent evt)
		{
			var e = (AddTalentEvent)evt;
			if (null == e.character)
			{
				return;
			}
			e.character.mOperActivity.OnAddTalentEvent(e.Id);
		}


		#endregion
	}

}
