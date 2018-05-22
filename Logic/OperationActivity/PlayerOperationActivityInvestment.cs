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
	public class PlayerOperationActivityInvestment : PlayerOperationActivity
    {
		public override OperationActivityType Type
		{
			get { return OperationActivityType.Investment; }
		}

		public override void OnStart()
		{

		}

		public override void OnEnd()
		{
			
		}


	}


}