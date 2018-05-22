using ScorpionMonitor;

namespace Chat
{
    public static class ChatServerMonitor
    {
		public static readonly IScorpionMeter TickRate;
		public static readonly IScorpionMeter ChatRate;
		static ChatServerMonitor()
		{
			var contextName = "Chat[" + ChatServer.Instance.Id + "]";
			var impl = new ScorpionServerMonitor(contextName);

			TickRate = impl.Meter("TickRate", SMUnit.Calls);
			ChatRate = impl.Meter("ChatRate", SMUnit.Items);
		}
		

	}
}
