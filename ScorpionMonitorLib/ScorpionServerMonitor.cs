using JsonConfig;

namespace ScorpionMonitor
{
	public class ScorpionServerMonitor
	{
		private static SMConfig MyConfig = new SMConfig();

		private IScorpionMonitor Monitor;

		static ScorpionServerMonitor()
		{
			LoadConfig();
		}

		public ScorpionServerMonitor(string contextName)
		{
			Monitor = new ScorpionMonitorImplement(contextName, MyConfig);
		}

		internal static void LoadConfig()
		{

			dynamic ServerConfig = Config.ApplyJsonFromPath("../Config/monitor.config");

			MyConfig.ConsoleReport = (bool)ServerConfig.ConsoleReport;
			MyConfig.ConsoleFreq = (float)ServerConfig.ConsoleFreq;
			MyConfig.DBReport = (bool)ServerConfig.DBReport;
			MyConfig.DBFreq = (float)ServerConfig.DBFreq;
			MyConfig.Ip = (string)ServerConfig.Ip;
			MyConfig.Port = (int)ServerConfig.Port;
			MyConfig.DBName = (string)ServerConfig.DBName;
			MyConfig.User = (string)ServerConfig.User;
			MyConfig.Password = (string)ServerConfig.Password;
			MyConfig.SSL = (bool)ServerConfig.SSL;
		}

		public IScorpionCounter Conter(string name, SMUnit unit)
		{
			return Monitor.Conter(name,unit);
		}

		public IScorpionHistogram Histogram(string name, SMUnit unit)
		{
			return Monitor.Histogram(name, unit);
		}

		public IScorpionMeter Meter(string name, SMUnit unit, SMTimeUnit rateUnit = SMTimeUnit.Seconds)
		{
			return Monitor.Meter(name, unit);
		}
	}
}
