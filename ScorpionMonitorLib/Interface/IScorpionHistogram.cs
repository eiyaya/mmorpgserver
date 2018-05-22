
namespace ScorpionMonitor
{
	public interface IScorpionHistogram
    {
       void Update(long value, string userValue = null);
    }
}
