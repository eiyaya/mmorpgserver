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
	public class OperationActivityReward
	{
		public int Id;
		public int Count;
	}
	public class OperationActivityItem
	{
		public int Id { get; set; }
		public int Type { get; set; }
		public string Name { get; set; }
		public int Condition { get; set; }
		public int OpenDelayDays { get; set; }
		public int LastDays { get; set; }
		public int RewardOpenDelay { get; set; }
		public int[] Params { get; set; }
		public List<int> StrParams { get; set; }
		public int TotalTimes { get; set; }
		public List<OperationActivityReward>[] Rewards = new List<OperationActivityReward>[3] { null, null, null };
	}

	public class OperationActivity
	{
		public enum ActivityStatus
		{
			Waiting,
			Started,
			Ended,
		}

		public int Id { get; set; }

	    public virtual OperationActivityType Type
	    {
			get { return OperationActivityType.Invalid; }
	    }

		public DateTime StartTime;
		public DateTime EndTime;
		public DateTime ScoreTime;
		public int DelayHours = 0;
		public int LastHours = 0;
		public int ScoreDelayHours = 0;
		public eRechargeActivityOpenRule OpenRule = eRechargeActivityOpenRule.None;
		public bool IsTitle = false;
		protected ActivityStatus Status = ActivityStatus.Waiting;

		public List<OperationActivityItem> ItemList = new List<OperationActivityItem>();
		public List<int> ServerList = new List<int>();
		public int[] Param;
		public bool IsActive
		{
			get { return ActivityStatus.Started == Status; }
		}
		public virtual void Init()
		{
			if (eRechargeActivityOpenRule.LimitTime == OpenRule)
			{
				if (DateTime.Now > StartTime && DateTime.Now < EndTime)
				{
					Status = ActivityStatus.Started;
				}
			}
			else
			{
				Status = ActivityStatus.Started;
			}
		}

		public void Start()
		{
			Status = ActivityStatus.Started;
			OnStart();
		}
		protected virtual void OnStart()
		{

		}

		public void End()
		{
			Status = ActivityStatus.Ended;
			OnEnd();
		}
		protected virtual void OnEnd()
		{

		}

		public virtual void Update()
        {
			if (eRechargeActivityOpenRule.LimitTime != OpenRule)
			{
				return;
			}
			if (Status == ActivityStatus.Waiting)
			{
				if (DateTime.Now > StartTime && DateTime.Now < EndTime)
				{
					Status = ActivityStatus.Started;
					OperationActivityManager.Instance.MarkAllCharacterDirty(ServerList);
					OnStart();
				}	
			}
			else if (Status == ActivityStatus.Started)
			{
				if (DateTime.Now > EndTime)
				{
					Status = ActivityStatus.Ended;
					OperationActivityManager.Instance.MarkAllCharacterDirty(ServerList);
					OnEnd();
				}
			}
// 			else if (Status == ActivityStatus.Started)
// 			{
// 				if (DateTime.Now > EndTime)
// 				{
// 					Status = ActivityStatus.Ended;
// 					OnEnd();
// 				}
// 			}

        }

		public void Destroy()
		{
			OnDestroy();
		}

		protected virtual void OnDestroy()
		{
			
		}
	}

}