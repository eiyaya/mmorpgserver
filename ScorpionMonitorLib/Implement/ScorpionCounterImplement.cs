using Metrics;

namespace ScorpionMonitor
{
	internal class ScorpionCounterImplement:IScorpionCounter
	{
		public Counter counter;
		public ScorpionCounterImplement(MetricsContext context,string name, SMUnit unit)
		{
			counter = context.Counter(name, new Unit(unit.Name));
		}
		
		public void Increment()
		{
			counter.Increment();
		}

		public void Increment(string item)
		{
			counter.Increment(item);
		}

		public void Increment(long amount)
		{
			counter.Increment(amount);
		}

		public void Increment(string item, long amount)
		{
			counter.Increment(item, amount);
		}

		public void Decrement()
		{
			counter.Decrement();
		}

		public void Decrement(string item)
		{
			counter.Decrement(item);
		}
		public void Decrement(long amount)
		{
			counter.Decrement(amount);
		}

		public void Decrement(string item, long amount)
		{
			counter.Decrement(item, amount);
		}
	}
}
