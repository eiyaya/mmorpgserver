namespace ScorpionMonitor
{
	interface IScorpionMonitor
	{
		IScorpionCounter Conter(string name, SMUnit unit);
		IScorpionHistogram Histogram(string name, SMUnit unit);
		IScorpionMeter Meter(string name, SMUnit unit, SMTimeUnit rateUnit = SMTimeUnit.Seconds);
	}
}
