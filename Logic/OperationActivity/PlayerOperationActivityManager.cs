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
	public class PlayerOperationActivityManager : NodeBase
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		public Dictionary<int,PlayerOperationActivity> DictActivity = new Dictionary<int,PlayerOperationActivity>();
		
		public CharacterController mCharacter; //角色
		private DBPlayerOperationActivityData mDbData;

		public bool NeedReset = false;
		
		public override IEnumerable<NodeBase> Children
		{
			get { return DictActivity.Values; }
		}

		public override void NetDirtyHandle()
		{
			foreach (var kv in DictActivity)
			{
				if (kv.Value.NetDirty)
				{
					kv.Value.NetDirtyHandle();
				}
			}
		}


		//用第一次创建
		public DBPlayerOperationActivityData InitByBase(CharacterController character)
		{
			mDbData = new DBPlayerOperationActivityData();
			
			mCharacter = character;
			Init();
			CleanNetDirty();
			return mDbData;
		}

		//用数据库数据
		public void InitByDB(CharacterController character, DBPlayerOperationActivityData operDbData)
		{
			mCharacter = character;
			mDbData = operDbData;
			Init();
			CleanNetDirty();
		}

		public void Init()
		{
			DictActivity.Clear();
			bool dirty = false;
			int i = 0;
			var dict = OperationActivityManager.Instance.DictActivity;
			foreach (var kv in dict)
			{
				var act = kv.Value;
				if (!act.IsActive)
				{
					continue;
				}

                if (act.ServerList.Count > 0 && !act.ServerList.Contains(SceneExtension.GetServerLogicId(mCharacter.serverId)))
				{
					continue;
				}

				if (!act.IsTitle && act.ItemList.Count <= 0)
				{
					continue;
				}

				DateTime startTime = DateTime.MinValue;
				DateTime endTime = DateTime.MinValue;

				if (eRechargeActivityOpenRule.NewServerAuto == act.OpenRule)
				{
					DateTime time = DateTime.Now;
                    var table = Table.GetServerName(SceneExtension.GetServerLogicId(mCharacter.serverId));
					if (null == table)
					{
                        Logger.Fatal("PlayerOperationActivityManager.Init mCharacter.serverId={0}", SceneExtension.GetServerLogicId(mCharacter.serverId));
					}
					else
					{
						time = DateTime.Parse(table.OpenTime).Date;
					}
					startTime = time.AddHours(act.DelayHours);
					endTime = time.AddHours(act.LastHours);	
				}
				else
				{
					startTime = act.StartTime;
					endTime = act.EndTime;
				}

				if (!(DateTime.Now >= startTime && DateTime.Now < endTime))
				{
					continue;
				}

				DBOperationActivityData dbAct = null;

				for (int j = i; j < mDbData.Data.Count; j++)
				{
					var dbData = mDbData.Data[j];
					if (null != dbData && dbData.Id == act.Id)
					{
						dbAct = dbData;
						if (j != i)
						{
							var temp = mDbData.Data[j];
							mDbData.Data[j] = mDbData.Data[i];
							mDbData.Data[i] = temp;
						}
						break;
					}
				}
				if (null == dbAct)
				{
					dbAct = new DBOperationActivityData();
					mDbData.Data.Insert(i,dbAct);
					dirty = true;
				}
				

				PlayerOperationActivity playerAct = null;

				var type = (OperationActivityType) act.Type;

				if (type == OperationActivityType.Guide)
				{
					playerAct = new PlayerOperationActivityGuide();
				}
				else if (type == OperationActivityType.Recharge)
				{
					playerAct = new PlayerOperationActivityRecharge();
				}
				else if (type == OperationActivityType.SpecialEvent)
				{
					playerAct = new PlayerOperationActivitySpecialEvent();
				}
				else if (type == OperationActivityType.Investment)
				{
					playerAct = new PlayerOperationActivityInvestment();
				}
				else if (type == OperationActivityType.Rank)
				{
					var actRank = new PlayerOperationActivityRank();
					var temp = act as OperationActivityRank;
					actRank.RankType = temp.RankType;
					playerAct = actRank;
				}
				else if (type == OperationActivityType.Lottery)
				{
					playerAct = new PlayerOperationActivityLottery();
				}
				else
				{
					//Error
				}
				playerAct.Init(mCharacter,act, dbAct, startTime, endTime);

				DictActivity.Add(playerAct.Id,playerAct);
				AddChild(playerAct);
				i++;
				
			}

			var remCount = mDbData.Data.Count - i;
			if (remCount > 0)
			{
				mDbData.Data.RemoveRange(i, remCount);
				dirty = true;
			}
			if (dirty)
			{
				MarkDirty();
			}

			NeedReset = false;
		}

		public void Tick()
		{
			if (NeedReset)
			{
				if (LogicServer.Instance.ServerControl.TickCount % 40 == mCharacter.mGuid % 40)
				{
					NeedReset = false;
					try
					{
						Init();
					}
					catch (Exception e)
					{
						Logger.Fatal("PlayerOperationActivityManager  "+ e.Message);	
					}
					mCharacter.Proxy.SyncOperationActivityItem(new MsgOperActivtyItemList());
				}
			}
			
		}

		public void Destroy()
		{
			
		}

		public void OnRankDataChange(RankType type, long value)
		{
			foreach (var kv in DictActivity)
			{
				var act = kv.Value;
				if (OperationActivityType.Rank != act.Type)
				{
					continue;
				}
				if (!act.IsActive)
				{
					continue;
				}

				var opAct = act as PlayerOperationActivityRank;

				opAct.OnRankDataChange(type,value);
			}
		}

		#region event
		public void OnRechargeSuccess(int diamond)
		{
			foreach (var kv in DictActivity)
			{
				var act = kv.Value;
				if (OperationActivityType.Recharge != act.Type)
				{
					continue;
				}
				if (!act.IsActive)
				{
					continue;
				}
				var opAct = act as PlayerOperationActivityRecharge;
				opAct.OnRecharge(diamond);
			}
		}

		public void OnItemChange(int id,int count)
		{
            foreach (var kv in DictActivity)
            {
                var act = kv.Value;
                if (OperationActivityType.SpecialEvent != act.Type)
                {
                    continue;
                }
                if (!act.IsActive)
                {
                    continue;
                }
                var opAct = act as PlayerOperationActivitySpecialEvent;
                opAct.OnItemChange(id, count);
            }
		}

		public void OnKillMonster(int monsterId)
		{
			foreach (var kv in DictActivity)
			{
				var act = kv.Value;
				if (OperationActivityType.SpecialEvent != act.Type)
				{
					continue;
				}
				if (!act.IsActive)
				{
					continue;
				}
				var opAct = act as PlayerOperationActivitySpecialEvent;
				opAct.OnKillMonster(monsterId);
			}
		}

		public void OnEnterArea(int areaId, bool isEnter)
		{
			foreach (var kv in DictActivity)
			{
				var act = kv.Value;
				if (OperationActivityType.SpecialEvent != act.Type)
				{
					continue;
				}
				if (!act.IsActive)
				{
					continue;
				}
				var opAct = act as PlayerOperationActivitySpecialEvent;
				opAct.OnEnterArea(areaId, isEnter);
			}
		}

		public void OnEnterFuben(int tollgateId)
		{
			foreach (var kv in DictActivity)
			{
				var act = kv.Value;
				if (OperationActivityType.SpecialEvent != act.Type)
				{
					continue;
				}
				if (!act.IsActive)
				{
					continue;
				}
				var opAct = act as PlayerOperationActivitySpecialEvent;
				opAct.OnEnterFuben(tollgateId);
			}
		}

		public void OnTollgateFinish(int tollgateId)
		{
			foreach (var kv in DictActivity)
			{
				var act = kv.Value;
				if (OperationActivityType.SpecialEvent != act.Type)
				{
					continue;
				}
				if (!act.IsActive)
				{
					continue;
				}
				var opAct = act as PlayerOperationActivitySpecialEvent;
				opAct.OnTollgateFinish(tollgateId);
			}
		}

		public void OnChacacterFlagTrue(int flagId)
		{
			foreach (var kv in DictActivity)
			{
				var act = kv.Value;
				if (OperationActivityType.SpecialEvent != act.Type)
				{
					continue;
				}
				if (!act.IsActive)
				{
					continue;
				}
				var opAct = act as PlayerOperationActivitySpecialEvent;
				opAct.OnChacacterFlagTrue(flagId);
			}
		}
		public void OnChacacterFlagFalse(int flagId)
		{
			foreach (var kv in DictActivity)
			{
				var act = kv.Value;
				if (OperationActivityType.SpecialEvent != act.Type)
				{
					continue;
				}
				if (!act.IsActive)
				{
					continue;
				}
				var opAct = act as PlayerOperationActivitySpecialEvent;
				opAct.OnChacacterFlagFalse(flagId);
			}
		}

        public void OnBuyItemEvent(int id, int count, CharacterController character)
		{
			foreach (var kv in DictActivity)
			{
				var act = kv.Value;
				if (OperationActivityType.SpecialEvent != act.Type)
				{
					continue;
				}
				if (!act.IsActive)
				{
					continue;
				}
				var opAct = act as PlayerOperationActivitySpecialEvent;
                opAct.OnBuyItemEvent(id, count, character);
			}
		}

		public void OnEnhanceEquipEvent(int part)
		{
			foreach (var kv in DictActivity)
			{
				var act = kv.Value;
				if (OperationActivityType.SpecialEvent != act.Type)
				{
					continue;
				}
				if (!act.IsActive)
				{
					continue;
				}
				var opAct = act as PlayerOperationActivitySpecialEvent;
				opAct.OnEnhanceEquipEvent(part);
			}
		}
		public void OnAdditionalEquipEvent(int part)
		{
			foreach (var kv in DictActivity)
			{
				var act = kv.Value;
				if (OperationActivityType.SpecialEvent != act.Type)
				{
					continue;
				}
				if (!act.IsActive)
				{
					continue;
				}
				var opAct = act as PlayerOperationActivitySpecialEvent;
				opAct.OnAdditionalEquipEvent(part);
			}
		}
		public void OnUpgradeSkillEvent(int skillId)
		{
			foreach (var kv in DictActivity)
			{
				var act = kv.Value;
				if (OperationActivityType.SpecialEvent != act.Type)
				{
					continue;
				}
				if (!act.IsActive)
				{
					continue;
				}
				var opAct = act as PlayerOperationActivitySpecialEvent;
				opAct.OnUpgradeSkillEvent(skillId);
				opAct.OnSkillLevelup(mCharacter.mSkill.GetSkillTotalLevel());
			}
		}
		public void OnNpcServeEvent(int serviceId)
		{
			//0商店
			//1修理
			//2治疗
			//3仓库
			foreach (var kv in DictActivity)
			{
				var act = kv.Value;
				if (OperationActivityType.SpecialEvent != act.Type)
				{
					continue;
				}
				if (!act.IsActive)
				{
					continue;
				}
				var opAct = act as PlayerOperationActivitySpecialEvent;
				opAct.OnNpcServeEvent(serviceId);
			}
		}

		public void OnAddFriendEvent()
		{
			foreach (var kv in DictActivity)
			{
				var act = kv.Value;
				if (OperationActivityType.SpecialEvent != act.Type)
				{
					continue;
				}
				if (!act.IsActive)
				{
					continue;
				}
				var opAct = act as PlayerOperationActivitySpecialEvent;
				opAct.OnAddFriendEvent();
			}
		}
		public void OnComposeItemEvent(int composeId, int itemId)
		{
			foreach (var kv in DictActivity)
			{
				var act = kv.Value;
				if (OperationActivityType.SpecialEvent != act.Type)
				{
					continue;
				}
				if (!act.IsActive)
				{
					continue;
				}
				var opAct = act as PlayerOperationActivitySpecialEvent;
				opAct.OnComposeItemEvent(composeId, itemId);
			}
		}
		public void OnCharacterExdataAddEvent(int exdataId, int add)
		{
			foreach (var kv in DictActivity)
			{
				var act = kv.Value;
				if (OperationActivityType.SpecialEvent != act.Type)
				{
					continue;
				}
				if (!act.IsActive)
				{
					continue;
				}
				var opAct = act as PlayerOperationActivitySpecialEvent;
				opAct.OnCharacterExdataAddEvent(exdataId, add);
			}
		}

		public void OnCharacterExdataChangeEvent(int exdataId, int val)
		{

            if (exdataId == StaticParam.CumulativeRechargeEverydayExdataId && val == 0)//扩展计数560，每日充值的0点清空推送特殊处理下 ，这个值是从表里查出来的，感觉这么写挺有问题的历史遗留问题先这样吧，测试好要和其他人商量下
            {
                foreach (var kv in DictActivity)
                {
                    var act = kv.Value;
                    if (act.Id != 13) continue;
                    if (OperationActivityType.Recharge != act.Type)
                    {
                        continue;
                    }
                    if (!act.IsActive)
                    {
                        continue;
                    }
                    var opAct = act as PlayerOperationActivityRecharge;
                    opAct.OnDayRechargeRest();
                }
            }
            else
            {
                foreach (var kv in DictActivity)
                {
                    var act = kv.Value;
                    if (OperationActivityType.SpecialEvent != act.Type)
                    {
                        continue;
                    }
                    if (!act.IsActive)
                    {
                        continue;
                    }
                    var opAct = act as PlayerOperationActivitySpecialEvent;
                    opAct.OnCharacterExdataChangeEvent(exdataId, val);
                }
            }
            //foreach (var kv in DictActivity)
            //{
            //    var act = kv.Value;
            //    if (OperationActivityType.SpecialEvent != act.Type)
            //    {
            //        continue;
            //    }
            //    if (!act.IsActive)
            //    {
            //        continue;
            //    }
            //    var opAct = act as PlayerOperationActivitySpecialEvent;
            //    opAct.OnCharacterExdataChangeEvent(exdataId, val);
            //}
		}

		public void OnAddTalentEvent(int id)
		{
			foreach (var kv in DictActivity)
			{
				var act = kv.Value;
				if (OperationActivityType.SpecialEvent != act.Type)
				{
					continue;
				}
				if (!act.IsActive)
				{
					continue;
				}
				var opAct = act as PlayerOperationActivitySpecialEvent;
				opAct.OnAddTalenEvent(id);
			}
		}

		public void OnUseDiamondEvent(int num)
		{
			foreach (var kv in DictActivity)
			{
				var act = kv.Value;
				if (OperationActivityType.SpecialEvent != act.Type)
				{
					continue;
				}
				if (!act.IsActive)
				{
					continue;
				}
				var opAct = act as PlayerOperationActivitySpecialEvent;
				opAct.OnUseDiamondEvent(num);
			}
		}
		public void OnPlayerLevelUp(int level)
		{
			foreach (var kv in DictActivity)
			{
				var act = kv.Value;
				if (OperationActivityType.SpecialEvent != act.Type)
				{
					continue;
				}
				if (!act.IsActive)
				{
					continue;
				}
				var opAct = act as PlayerOperationActivitySpecialEvent;
				opAct.OnPlayerLevelup(level);
			}
		}

		public void OnCommitMission(int type)
		{
			foreach (var kv in DictActivity)
			{
				var act = kv.Value;
				if (OperationActivityType.SpecialEvent != act.Type)
				{
					continue;
				}
				if (!act.IsActive)
				{
					continue;
				}
				var opAct = act as PlayerOperationActivitySpecialEvent;
				opAct.OnCommitMission(type);
			}
		}

		public void OnWingFormation()
		{
			foreach (var kv in DictActivity)
			{
				var act = kv.Value;
				if (OperationActivityType.SpecialEvent != act.Type)
				{
					continue;
				}
				if (!act.IsActive)
				{
					continue;
				}
				var opAct = act as PlayerOperationActivitySpecialEvent;
				opAct.OnWingFormation();
			}
		}
		public void OnWingTrainEvent()
		{
			foreach (var kv in DictActivity)
			{
				var act = kv.Value;
				if (OperationActivityType.SpecialEvent != act.Type)
				{
					continue;
				}
				if (!act.IsActive)
				{
					continue;
				}
				var opAct = act as PlayerOperationActivitySpecialEvent;
				opAct.OnWingTrainEvent();
			}
		}

		public void OnExcellentEquip()
		{
			foreach (var kv in DictActivity)
			{
				var act = kv.Value;
				if (OperationActivityType.SpecialEvent != act.Type)
				{
					continue;
				}
				if (!act.IsActive)
				{
					continue;
				}
				var opAct = act as PlayerOperationActivitySpecialEvent;
				opAct.OnExcellentEquipEvent();
			}
		}

		public void OnSuperExcellentEquip()
		{
			foreach (var kv in DictActivity)
			{
				var act = kv.Value;
				if (OperationActivityType.SpecialEvent != act.Type)
				{
					continue;
				}
				if (!act.IsActive)
				{
					continue;
				}
				var opAct = act as PlayerOperationActivitySpecialEvent;
				opAct.OnSuperExcellentEquipEvent();
			}
		}
		#endregion

	}
}