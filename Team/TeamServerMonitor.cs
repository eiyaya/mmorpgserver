using ScorpionMonitor;

namespace Team
{
    public static class TeamServerMonitor
    {
		public static readonly IScorpionMeter TickRate;
		static TeamServerMonitor()
		{
			var contextName = "Team[" + TeamServer.Instance.Id + "]";
			var impl = new ScorpionServerMonitor(contextName);

			TickRate = impl.Meter("TickRate", SMUnit.Calls);
		}


	}
}
