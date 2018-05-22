using ScorpionMonitor;

namespace Activity
{
    public static class ActivityServerMonitor
    {
		public static readonly IScorpionMeter TickRate;
		static ActivityServerMonitor()
		{
			var contextName = "Activity[" + ActivityServer.Instance.Id + "]";
			var impl = new ScorpionServerMonitor(contextName);

			TickRate = impl.Meter("TickRate", SMUnit.Calls);
		}


	}
}
