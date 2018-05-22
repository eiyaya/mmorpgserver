
using Metrics;

namespace ScorpionMonitor
{
	internal class ScorpionHistogram : IScorpionHistogram
	{
		private Histogram histogram;
		public ScorpionHistogram (MetricsContext context, string name, SMUnit unit)
		{
			histogram = context.Histogram(name, new Unit(unit.Name));
		}

		public void Update(long value, string userValue = null)
		{
			histogram.Update(value,userValue);
		}
    }
}
