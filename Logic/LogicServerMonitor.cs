using ScorpionMonitor;

namespace Logic
{
	public static class LogicServerMonitor
	{
		public static readonly IScorpionMeter TickRate;
		public static readonly IScorpionCounter DiamondNumber;
		static LogicServerMonitor()
		{
			var contextName = "Logic[" + LogicServer.Instance.Id + "]";
			var impl = new ScorpionServerMonitor(contextName);

			TickRate = impl.Meter("TickRate", SMUnit.Calls);
			DiamondNumber = impl.Conter("DiamondNumber", SMUnit.Items);
		}
		

	}
}
