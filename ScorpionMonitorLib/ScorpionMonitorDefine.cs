namespace ScorpionMonitor
{
	internal class SMConfig
	{
		public bool ConsoleReport = false;
		public float ConsoleFreq = 30;
		public bool DBReport = false;
		public float DBFreq = 5;
		public string Ip = "127.0.0.1";
		public int Port = 8086;
		public string DBName = "metrics";
		public string User ="root";
		public string Password = "root";
		public bool SSL = false;
	}

	public enum SMTimeUnit
	{
		Nanoseconds = 0,
		Microseconds = 1,
		Milliseconds = 2,
		Seconds = 3,
		Minutes = 4,
		Hours = 5,
		Days = 6,
	}

	public struct SMUnit
	{
		public readonly string Name;

		public static readonly SMUnit None = new SMUnit(string.Empty);
		public static readonly SMUnit Requests = new SMUnit("Requests");
		public static readonly SMUnit Commands = new SMUnit("Commands");
		public static readonly SMUnit Calls = new SMUnit("Calls");
		public static readonly SMUnit Events = new SMUnit("Events");
		public static readonly SMUnit Errors = new SMUnit("Errors");
		public static readonly SMUnit Results = new SMUnit("Results");
		public static readonly SMUnit Items = new SMUnit("Items");
		public static readonly SMUnit MegaBytes = new SMUnit("Mb");
		public static readonly SMUnit KiloBytes = new SMUnit("Kb");
		public static readonly SMUnit Bytes = new SMUnit("bytes");
		public static readonly SMUnit Percent = new SMUnit("%");
		public static readonly SMUnit Threads = new SMUnit("Threads");

		public static SMUnit Custom(string name)
		{
			return new SMUnit(name);
		}

		public static implicit operator SMUnit(string name)
		{
			return SMUnit.Custom(name);
		}

		private SMUnit(string name)
		{
			if (name == null)
			{
				this.Name = "No Name";
			}

			this.Name = name;
		}
	}

}
