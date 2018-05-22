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
	public class PlayerOperationActivitySpecialEvent : PlayerOperationActivity
	{
		public Dictionary<string, PlayerOperationActivityItem> EventDict = new Dictionary<string, PlayerOperationActivityItem>();

		enum SpecialEventType
		{
			Invalid = 0,
			ItemChange = 1,
			KillMonster = 2,
			EnterArea = 3,
			TollgateFinish = 4,
			ChacacterFlagTrue = 5,
			ChacacterFlagFalse = 6,
			BuyItemEvent = 7,
			EnhanceEquipEvent = 8,
			AdditionalEquipEvent = 9,
			UpgradeSkillEvent = 10,
			NpcServeEvent = 11,
			AddFriendEvent = 12,
			ComposeItemEvent = 13,
			CharacterExdataAddEvent = 14,
			SkillPointChangeEvent = 15,
			UseDiamondEvent = 16,
			TotalSkillsLevel = 17, //同技能等级
			EnterFuben = 18, //进入副本
			PlayerLevel = 19,		//玩家等级
			CommitMissionEvent = 20,	//提交任务数
			WingFormationEvent = 21, //翅膀升阶
			WingTrainEvent = 22, //翅膀培养
			ExcellentEquip = 23, //装备洗练
			SuperExcellentEquip = 24, //装备随灵
			ExtDataEvent = 25, //扩展计数
		}
		public override OperationActivityType Type
		{
			get { return OperationActivityType.SpecialEvent; }
		}

		public override void OnStart()
		{
 
		}

		public override void OnEnd()
		{
			
		}

		public override void RefreshActivity(OperationActivity data)
		{
			base.RefreshActivity(data);
			OnSkillLevelup(Controller.mSkill.GetSkillTotalLevel());
			OnPlayerLevelup(Controller.GetLevel());
			foreach (var item in Items)
			{
				if (!item.IsActive)
				{
					continue;
				}
				if (SpecialEventType.TotalSkillsLevel == (SpecialEventType)item.Params[1])
				{
					item.Counter = (ulong)Controller.mSkill.GetSkillTotalLevel();
				}
				else if (SpecialEventType.PlayerLevel == (SpecialEventType) item.Params[1])
				{
					item.Counter = (ulong)Controller.GetLevel();
				}
				else if (SpecialEventType.ExtDataEvent == (SpecialEventType)item.Params[1])
				{
					item.Counter = (ulong)Controller.GetExData(item.Params[2]);
				}
			}
			
		}

		#region event
		public void OnItemChange(int id, int count)
		{
			foreach (var item in Items)
			{
				if (!item.IsActive)
				{
					continue;
				}
				if (SpecialEventType.ItemChange != (SpecialEventType)item.Params[1])
				{
					continue;
				}
				if (item.StrParams.Count > 0 && !item.StrParams.Contains(id))
				{
					continue;
				}
				item.Counter += (ulong)count;
			}
		}

		public void OnKillMonster(int monsterId)
		{
			foreach (var item in Items)
			{
				if (!item.IsActive)
				{
					continue;
				}
				if (SpecialEventType.KillMonster != (SpecialEventType)item.Params[1])
				{
					continue;
				}
				if (item.StrParams.Count > 0 && !item.StrParams.Contains(monsterId))
				{
					continue;
				}
				item.Counter += 1;
			}
		}

		public void OnEnterArea(int areaId, bool isEnter)
		{
			if (!isEnter)
			{
				return;
			}
			foreach (var item in Items)
			{
				if (!item.IsActive)
				{
					continue;
				}
				if (SpecialEventType.EnterArea != (SpecialEventType)item.Params[1])
				{
					continue;
				}
				if (item.StrParams.Count > 0 && !item.StrParams.Contains(areaId))
				{
					continue;
				}
				item.Counter += 1;
			}
		}
		public void OnEnterFuben(int tollgateId)
		{
			foreach (var item in Items)
			{
				if (!item.IsActive)
				{
					continue;
				}
				if (SpecialEventType.EnterFuben != (SpecialEventType)item.Params[1])
				{
					continue;
				}
				if (item.StrParams.Count > 0 && !item.StrParams.Contains(tollgateId))
				{
					continue;
				}
				item.Counter += 1;
			}
		}
		public void OnTollgateFinish(int tollgateId)
		{
			foreach (var item in Items)
			{
				if (!item.IsActive)
				{
					continue;
				}
				if (SpecialEventType.TollgateFinish != (SpecialEventType)item.Params[1])
				{
					continue;
				}
				if (item.StrParams.Count > 0 && !item.StrParams.Contains(tollgateId))
				{
					continue;
				}
				item.Counter += 1;
			}
		}

		public void OnChacacterFlagTrue(int flagId)
		{
			foreach (var item in Items)
			{
				if (!item.IsActive)
				{
					continue;
				}
				if (SpecialEventType.ChacacterFlagTrue != (SpecialEventType)item.Params[1])
				{
					continue;
				}
				if (item.StrParams.Count > 0 && !item.StrParams.Contains(flagId))
				{
					continue;
				}
				item.Counter += 1;
			}
		}
		public void OnChacacterFlagFalse(int flagId)
		{
			foreach (var item in Items)
			{
				if (!item.IsActive)
				{
					continue;
				}
				if (SpecialEventType.ChacacterFlagFalse != (SpecialEventType)item.Params[1])
				{
					continue;
				}
				if (item.StrParams.Count > 0 && !item.StrParams.Contains(flagId))
				{
					continue;
				}
				item.Counter += 1;
			}
		}

        public void OnBuyItemEvent(int id, int count, CharacterController character)
		{
			foreach (var item in Items)
			{
                if (201001 == item.Id)
                {
                    //刷开服活动时装是否领取状态
                    ShiZhuangActivity(id,item,character);
                }
				if (!item.IsActive)
				{
					continue;
				}
				if (SpecialEventType.BuyItemEvent != (SpecialEventType)item.Params[1])
				{
					continue;
				}
				if (item.StrParams.Count > 0 && !item.StrParams.Contains(id))
				{
					continue;
				}
				item.Counter += (ulong)count;
			}
		}
        private void ShiZhuangActivity(int id,PlayerOperationActivityItem item,CharacterController character)
        {
            //时装特殊处理
            var tbItem = Table.GetItemBase(id);
            if (null != tbItem)
            {
                if (10500 == tbItem.Type)//时装
                {
                    var tbStore = Table.GetStore(tbItem.StoreID);
                    if (null != tbStore)
                    {
                        var exdata = character.GetExData(tbStore.DayCount);
                        if (1 == exdata)
                        {
                            item.Aquire();
                        }
                    }
                }
            } 
        }

		public void OnEnhanceEquipEvent(int part)
		{
			foreach (var item in Items)
			{
				if (!item.IsActive)
				{
					continue;
				}
				if (SpecialEventType.EnhanceEquipEvent != (SpecialEventType)item.Params[1])
				{
					continue;
				}
				if (item.StrParams.Count > 0 && !item.StrParams.Contains(part))
				{
					continue;
				}
				item.Counter += 1;
			}
		}
		public void OnAdditionalEquipEvent(int part)
		{
			foreach (var item in Items)
			{
				if (!item.IsActive)
				{
					continue;
				}
				if (SpecialEventType.AdditionalEquipEvent != (SpecialEventType)item.Params[1])
				{
					continue;
				}
				if (item.StrParams.Count > 0 && !item.StrParams.Contains(part))
				{
					continue;
				}
				item.Counter += 1;
			}
		}
		public void OnUpgradeSkillEvent(int skillId)
		{
			foreach (var item in Items)
			{
				if (!item.IsActive)
				{
					continue;
				}
				if (SpecialEventType.UpgradeSkillEvent != (SpecialEventType)item.Params[1])
				{
					continue;
				}
				if (item.StrParams.Count > 0 && !item.StrParams.Contains(skillId))
				{
					continue;
				}
				item.Counter += 1;
			}
		}
		public void OnNpcServeEvent(int serviceId)
		{
			//0商店
            //1修理
            //2治疗
			//3仓库

			foreach (var item in Items)
			{
				if (!item.IsActive)
				{
					continue;
				}
				if (SpecialEventType.NpcServeEvent != (SpecialEventType)item.Params[1])
				{
					continue;
				}
				if (item.StrParams.Count > 0 && !item.StrParams.Contains(serviceId))
				{
					continue;
				}
				item.Counter += 1;
			}
		}

		public void OnAddFriendEvent()
		{
			foreach (var item in Items)
			{
				if (!item.IsActive)
				{
					continue;
				}
				if (SpecialEventType.AddFriendEvent != (SpecialEventType)item.Params[1])
				{
					continue;
				}
				item.Counter += 1;
			}
		}
		public void OnComposeItemEvent(int composeId, int itemId)
		{
			foreach (var item in Items)
			{
				if (!item.IsActive)
				{
					continue;
				}
				if (SpecialEventType.ComposeItemEvent != (SpecialEventType)item.Params[1])
				{
					continue;
				}
				if (item.StrParams.Count > 0 && !item.StrParams.Contains(composeId))
				{
					continue;
				}
				item.Counter += 1;
			}
		}
		public void OnCharacterExdataAddEvent(int exdataId, int add)
		{
			foreach (var item in Items)
			{
				if (!item.IsActive)
				{
					continue;
				}
				if (SpecialEventType.CharacterExdataAddEvent == (SpecialEventType)item.Params[1])
				{
					if (item.StrParams.Count > 0 && !item.StrParams.Contains(exdataId))
					{
						continue;
					}
					item.Counter += (ulong)add;
				}
			}
		}

		public void OnCharacterExdataChangeEvent(int exdataId, int val)
		{
			foreach (var item in Items)
			{
				if (!item.IsActive)
				{
					continue;
				}
				if (SpecialEventType.ExtDataEvent == (SpecialEventType)item.Params[1])
				{
					if (exdataId != item.Params[2])
					{
						continue;
					}
					item.Counter = (ulong)val;
				}
			}
		}
		public void OnAddTalenEvent(int id)
		{
			foreach (var item in Items)
			{
				if (!item.IsActive)
				{
					continue;
				}
				if (SpecialEventType.SkillPointChangeEvent != (SpecialEventType)item.Params[1])
				{
					continue;
				}
				if (item.StrParams.Count > 0 && !item.StrParams.Contains(id))
				{
					continue;
				}
				item.Counter += 1;
			}
		}

		public void OnUseDiamondEvent(int num)
		{
			foreach (var item in Items)
			{
				if (!item.IsActive)
				{
					continue;
				}
				if (SpecialEventType.UseDiamondEvent != (SpecialEventType)item.Params[1])
				{
					continue;
				}

				item.Counter += (ulong)num;
			}
		}

		public void OnSkillLevelup(int totalLevel)
		{
			foreach (var item in Items)
			{
				if (!item.IsActive)
				{
					continue;
				}
				if (SpecialEventType.TotalSkillsLevel != (SpecialEventType)item.Params[1])
				{
					continue;
				}
				item.Counter = (ulong)totalLevel;
			}
			
		}
		public void OnPlayerLevelup(int level)
		{
			foreach (var item in Items)
			{
				if (!item.IsActive)
				{
					continue;
				}
				if (SpecialEventType.PlayerLevel != (SpecialEventType)item.Params[1])
				{
					continue;
				}
				item.Counter = (ulong)level;
			}

		}
		public void OnCommitMission(int type)
		{
			foreach (var item in Items)
			{
				if (!item.IsActive)
				{
					continue;
				}
				if (SpecialEventType.CommitMissionEvent != (SpecialEventType)item.Params[1])
				{
					continue;
				}
				if (item.StrParams.Count > 0 && !item.StrParams.Contains(type))
				{
					continue;
				}
				item.Counter += 1;
			}
		}

		public void OnWingFormation()
		{
			foreach (var item in Items)
			{
				if (!item.IsActive)
				{
					continue;
				}
				if (SpecialEventType.WingFormationEvent != (SpecialEventType)item.Params[1])
				{
					continue;
				}

				item.Counter += 1;
			}
		}

		public void OnWingTrainEvent()
		{
			foreach (var item in Items)
			{
				if (!item.IsActive)
				{
					continue;
				}
				if (SpecialEventType.WingTrainEvent != (SpecialEventType)item.Params[1])
				{
					continue;
				}

				item.Counter += 1;
			}
		}
		public void OnExcellentEquipEvent()
		{
			foreach (var item in Items)
			{
				if (!item.IsActive)
				{
					continue;
				}
				if (SpecialEventType.ExcellentEquip != (SpecialEventType)item.Params[1])
				{
					continue;
				}

				item.Counter += 1;
			}
		}
		public void OnSuperExcellentEquipEvent()
		{
			foreach (var item in Items)
			{
				if (!item.IsActive)
				{
					continue;
				}
				if (SpecialEventType.SuperExcellentEquip != (SpecialEventType)item.Params[1])
				{
					continue;
				}

				item.Counter += 1;
			}
		}
		#endregion

	}

}