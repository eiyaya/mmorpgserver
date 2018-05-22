namespace ScorpionMonitor
{
	public interface IScorpionMeter
	{
		void Mark();
		void Mark(string item);
		void Mark(long count);
		void Mark(string item, long count);
	}
}
