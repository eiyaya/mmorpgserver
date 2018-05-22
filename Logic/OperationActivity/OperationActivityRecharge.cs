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
	public class OperationActivityRecharge : OperationActivity
	{
		public override OperationActivityType Type
		{
			get { return OperationActivityType.Recharge; }
		}

		protected override void OnStart()
		{
		}

		protected override void OnEnd()
		{
		}

		protected override void OnDestroy()
		{
		}


	}

}