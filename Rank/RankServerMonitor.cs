using ScorpionMonitor;

namespace Rank
{
    public static class RankServerMonitor
    {
		public static readonly IScorpionMeter TickRate;
		static RankServerMonitor()
		{
			var contextName = "Rank[" + RankServer.Instance.Id + "]";
			var impl = new ScorpionServerMonitor(contextName);

			TickRate = impl.Meter("TickRate", SMUnit.Calls);
		}


	}
}
