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
    public enum ActivityStatus
    {
        Waiting,
        Started,
        Ended,
    }

    public class PlayerOperationActivity : NodeBase
    {
	    public int Id { get; set; }

	    public virtual OperationActivityType Type
	    {
			get { return OperationActivityType.Invalid; }
	    }

        public DateTime StartTime;
        public DateTime EndTime;
	    public DateTime ScoreTime;
		public List<PlayerOperationActivityItem> Items = new List<PlayerOperationActivityItem>();
		public DBOperationActivityData mDBData;
	    public CharacterController Controller;
	    public int[] Param;
	    public bool IsActive
	    {
		    get
		    {
				return DateTime.Now >= StartTime && DateTime.Now < EndTime;
		    }
	    }

	    public virtual bool Init(CharacterController controller,OperationActivity data, DBOperationActivityData db,DateTime begin,DateTime end)
		{
			Controller = controller;
			mDBData = db;
		    Id = data.Id;
		    mDBData.Id = Id;
			StartTime = begin;
			EndTime = end;
		    Param = data.Param;
			RefreshActivity(data);
			return true;
		}

		public virtual void OnStart()
        {


        }

		public virtual void OnEnd()
        {
        }

		public virtual void CleanUp()
        {
            
        }




        public override IEnumerable<NodeBase> Children
        {
            get { return Items; }
        }

		public override void NetDirtyHandle()
		{
			if (null != Controller)
			{
				if (null != Controller.Proxy)
				{
					if (NetDirty)
					{
						Controller.Proxy.SyncOperationActivityTerm(Id,mDBData.Param);
					}
					var msg = new MsgOperActivtyItemList();
					foreach (var item in Items)
					{
						if (item.NetDirty)
						{
							var msgItem = new MsgOperActivtyItem();
							msgItem.Id = item.Id;
							msgItem.Count = item.Counter;
							msgItem.AquiredTimes = item.mDBData.Aquired;
							msg.Items.Add(msgItem);
						}
					}
					if (msg.Items.Count > 0)
					{
						Controller.Proxy.SyncOperationActivityItem(msg);	
					}
				}
			}
		}



		public virtual ErrorCodes AquireReward(int id,out int result)
		{
			result = -1;
			if (!IsActive)
			{
				return ErrorCodes.Error_Operation_NotInActivity;
			}
			

			for (int i = 0; i < Items.Count; i++)
			{
				var item = Items[i];
				if (id == item.Id)
				{
					if (!item.IsActive)
					{
						return ErrorCodes.Error_Operation_NotInActivity;
					}
					if (!item.CanAquire)
					{
						return ErrorCodes.Error_Operation_CannotAquire;
					}
					if (null == Controller)
					{
						return ErrorCodes.Unknow;
					}

					var tb = Table.GetYunYing(item.Id);
					if(null==tb)
					{
						return ErrorCodes.Error_Operation_NotInActivity;
					}

					if (-1 != tb.ConditionId)
					{
						if (-2 != Controller.CheckCondition(tb.ConditionId))
						{
							return ErrorCodes.Error_Condition;	
						}
					}

					if (-1 != tb.NeetItem && tb.NeetItemCount>0)
					{
						var nowCount = Controller.mBag.GetItemCount(tb.NeetItem);
						if (nowCount < tb.NeetItemCount)
						{
							return ErrorCodes.ItemNotEnough;
						}
                        var ret = Controller.mBag.DeleteItem(tb.NeetItem, tb.NeetItemCount, eDeleteItemType.OperationActivity, tb.Name);
						if (ErrorCodes.OK != ret)
						{
							return ret;
						}
					}

					item.Aquire();

					if (null != item.Rewards)
					{
						var items = new Dictionary<int, int>();
						for (int j = 0; j < item.Rewards.Count; j++)
						{
							var reward = item.Rewards[j];
							if (-1 == reward.Id || reward.Count <= 0)
							{
								continue;
							}
							items.Add(reward.Id, reward.Count);
                            var tbItem = Table.GetItemBase(reward.Id);
                            if (null != tbItem)
                            {
                                if (10500 == tbItem.Type)//时装
                                {
                                    var tbYunYing = Table.GetYunYing(item.Id);
                                    if (null != tbYunYing)
                                    {
                                        //开服活动购买时装后，修改时装的扩展计数，刷新时装商城
                                        Controller.SetExData(tbYunYing.Param[2], 0);
                                    }
                                }
                                else
                                {
                                    Controller.BroadCastGetEquip(reward.Id, 100002166); 
                                }
                            }
						}
						if (items.Count > 0)
						{
							Controller.mBag.AddItemOrMail(200, items, null, eCreateItemType.OperationActivity,tb.Name);
						}
					}
					return ErrorCodes.OK;
				}
			}
			return ErrorCodes.Error_Operation_NotFound;
		}

		public virtual void RefreshActivity(OperationActivity data)
		{
			Items.Clear();
			bool dirty = false;
			var list = data.ItemList;
			for (int i = 0; i < list.Count; i++)
			{
				var item = list[i];
				DBOperActivityItem db = null;
				if (i >= mDBData.Items.Count)
				{
					db = new DBOperActivityItem();
					mDBData.Items.Add(db);
					dirty = true;
				}
				else
				{
					db = mDBData.Items[i];
				}
				var actItem = MakeItem(item, db, StartTime);

				Items.Add(actItem);
				AddChild(actItem);
			}

			if (dirty)
			{
				MarkDbDirty();
			}
		}

		private PlayerOperationActivityItem MakeItem(OperationActivityItem item, DBOperActivityItem db, DateTime time)
		{
			var ret = new PlayerOperationActivityItem();

			ret.mDBData = db;
			ret.Id = item.Id;
			ret.Need = (ulong)item.Params[0];
			ret.TotalTimes = item.TotalTimes;
			ret.Condition = item.Condition;
			if (-1 == item.OpenDelayDays)
			{
				ret.Begin = DateTime.MinValue;
				ret.RewardBegin = ret.Begin;
				ret.End = DateTime.MaxValue;	
			}
			else
			{
				ret.Begin = time.Date.AddDays(item.OpenDelayDays);
				ret.RewardBegin = time.Date.AddDays(item.RewardOpenDelay);
				if (ret.Begin > ret.RewardBegin)
				{
					ret.RewardBegin = ret.Begin;
				}
				ret.End = ret.Begin.AddDays(item.LastDays);	
			}
			
			ret.Params = item.Params;
			ret.StrParams = item.StrParams;
			var idx = Controller.GetRole();
			if (idx >= 0 && idx < item.Rewards.Count())
			{
				ret.Rewards = item.Rewards[idx];
			}


			return ret;
		}

    }

}