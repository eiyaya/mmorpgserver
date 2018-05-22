using ScorpionMonitor;

namespace Login
{
	public static class LoginServerMonitor
	{
		public static readonly IScorpionMeter TickRate;
		public static readonly IScorpionMeter LoginRate;

		static LoginServerMonitor()
		{
			var contextName = "Login[" + LoginServer.Instance.Id + "]";
			var impl = new ScorpionServerMonitor(contextName);

			TickRate = impl.Meter("TickRate", SMUnit.Calls);
			LoginRate = impl.Meter("LoginRate", SMUnit.Items);
		}
		
	}
}
