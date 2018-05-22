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
	public class PlayerOperationActivityRecharge : PlayerOperationActivity
    {
		public override OperationActivityType Type
		{
			get { return OperationActivityType.Recharge; }
		}

		public override void OnStart()
		{

		}

		public override void OnEnd()
		{
		}


		public void OnRecharge(int num)
		{
			if (!IsActive)
			{
				return;
			}
			foreach (var item in Items)
			{
				if (item.IsActive && !item.HasAquired)
				{
					item.Counter += (ulong)num;
					item.MarkDirty();	
				}
			}
		}

        public void OnDayRechargeRest()
        {
            if (!IsActive)
            {
                return;
            }
            foreach (var item in Items)
            {
                if (!item.IsActive && item.CanAquire)//把客户端昨天的设置为0.因为这个已经不激活了
                {
                    item.Counter = 0;
                    item.MarkDirty();
                }
            }
        }

        //public override void NetDirtyHandle()
        //{
        //    base.NetDirtyHandle();
        //}
	}


}