using Metrics;

namespace ScorpionMonitor
{
	internal class ScorpionMeterImplement : IScorpionMeter
	{
		private Meter meter = null;
		public ScorpionMeterImplement(MetricsContext context, string name, SMUnit unit,SMTimeUnit rateUnit)
		{
			meter = context.Meter(name, new Unit(unit.Name), (TimeUnit)rateUnit);
		}
		public void Mark()
		{
			meter.Mark();
		}

		public void Mark(string item)
		{
			meter.Mark(item);
		}

		public void Mark(long count)
		{
			meter.Mark(count);
		}

		public void Mark(string item, long count)
		{
			meter.Mark(item,count);
		}
	}
}
