using System;
using Metrics;
using Metrics.NET.InfluxDB;

namespace ScorpionMonitor
{
 	internal class ScorpionMonitorImplement:IScorpionMonitor
    {

 		private MetricsContext Context = null;
		internal ScorpionMonitorImplement(string contextName, SMConfig config)
 		{
 			if (null != Context)
 			{
				Console.WriteLine("!!!!!!!!!!!!Error: null != Context");
 				return;
 			}
			Context = Metric.Context(contextName);
			var metricConfig = new MetricsConfig(Context);
			metricConfig.WithReporting(report =>
 			{
 				if (config.ConsoleReport)
 				{
 					report.WithConsoleReport(TimeSpan.FromSeconds(config.ConsoleFreq));
 				}
 				if (config.DBReport)
 				{
					report.WithInflux(config.Ip, config.Port, config.User, config.Password, config.DBName, TimeSpan.FromSeconds(config.DBFreq), new ConfigOptions
 					{
 						UseHttps = config.SSL,
 					});
 				}
 			});
 		}

 		public IScorpionCounter Conter(string name, SMUnit unit)
 		{
			var c = new ScorpionCounterImplement(Context,name,unit);
 			return c;
 		}

		public IScorpionHistogram Histogram(string name, SMUnit unit)
 		{
			var c = new ScorpionHistogram(Context, name, unit);
 			return c;
 		}

 		public IScorpionMeter Meter(string name, SMUnit unit, SMTimeUnit rateUnit = SMTimeUnit.Seconds)
 		{
			var c = new ScorpionMeterImplement(Context, name, unit, rateUnit);
			return c;
 		}

    }
}
