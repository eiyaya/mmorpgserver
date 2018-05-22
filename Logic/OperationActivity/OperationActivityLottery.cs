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
	public class OperationActivityLottery : OperationActivity
	{
		public int ResetNeedMoney = 1;
		public List<int> DrawLotteryLadder = new List<int>();
		public override OperationActivityType Type
		{
			get { return OperationActivityType.Lottery; }
		}

		protected override void OnStart()
		{

		}

		protected override void OnEnd()
		{
		}
	}
}