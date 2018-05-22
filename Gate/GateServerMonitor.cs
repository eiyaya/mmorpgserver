using ScorpionMonitor;

namespace Gate
{
    public static class GateServerMonitor
    {
		public static readonly IScorpionCounter ConnectionNumber;
		public static readonly IScorpionMeter ReceivePacketSize;
		public static readonly IScorpionMeter ReceivePacketNumber;
		public static readonly IScorpionMeter SendPacketSize;
		public static readonly IScorpionMeter SendPacketNumber;

		static GateServerMonitor()
		{
			var contextName = string.Format("Gate[{0}]", GateServer.Instance.Id);
			var impl = new ScorpionServerMonitor(contextName);

			ReceivePacketSize = impl.Meter("ReceivePacketSize", SMUnit.Bytes);
			ReceivePacketNumber = impl.Meter("ReceivePacketNumber", SMUnit.Requests);
			SendPacketSize = impl.Meter("SendPacketSize", SMUnit.Bytes);
			SendPacketNumber = impl.Meter("SendPacketNumber", SMUnit.Requests);
			ConnectionNumber = impl.Conter("ConnectionNumber", SMUnit.Items);
		}
		

	}
}
