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
	public class PlayerOperationActivityItem : NodeBase
    {
        public int Id;
		public string Desc;
        public int Condition = -1;
		public DateTime Begin;
		public DateTime RewardBegin;
		public DateTime End;
		public DBOperActivityItem mDBData;
		public CharacterController Controller;
		public List<OperationActivityReward> Rewards;
		public int[] Params;
		public List<int> StrParams;
		public ulong Need { get; set; }
		public int TotalTimes { get; set; }


		public virtual bool HasAquired
		{
			get
			{
				return AquiredTimes >= TotalTimes;
			}
		}
		public virtual int AquiredTimes
		{
			get
			{
				return mDBData.Aquired;
			}
		}
        public virtual bool CanAquire
        {
			get
			{
				return !HasAquired && Counter >= Need;
			}
        }

		public void Aquire()
		{
			mDBData.Aquired++;
			MarkDirty();
		}
        public ulong Counter
        {
            get { return mDBData.Status; }
	        set
	        {
		        mDBData.Status = value;
				MarkDirty();
	        }
        }

		public bool IsActive
		{
			get
			{
				return DateTime.Now >= Begin && DateTime.Now < End;	
			}
			
		}
        public override IEnumerable<NodeBase> Children
        {
            get { return null; }
        }

    }

}