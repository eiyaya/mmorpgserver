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

	public class OperationActivityInvestment : OperationActivity
	{
		public override OperationActivityType Type
		{
			get { return OperationActivityType.Investment; }
		}

		protected override void OnStart()
		{
			
		}

		protected override void OnEnd()
		{
		}
	}

}