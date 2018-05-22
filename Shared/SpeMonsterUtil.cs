#region using

using System;
using System.Collections.Generic;
using DataTable;
using NLog;

#endregion

namespace Shared
{
    public class MonsterConfig
    {
        public MonsterConfig(List<DateTime> times, List<WorldBOSSRecord> records, int timeIndex)
        {
            Times = times;
            Records = records;
            TimeIndex = timeIndex;
        }

        public readonly List<WorldBOSSRecord> Records;
        public int TimeIndex;
        public readonly List<DateTime> Times;

        public DateTime UseNextTime()
        {
            var t = Times[TimeIndex];
            Times[TimeIndex] = Times[TimeIndex].AddDays(1);
            TimeIndex = (TimeIndex + 1)%Times.Count;
            return t;
        }
    }

    public abstract class CreateSpeMonsterBase
    {
        public CreateSpeMonsterBase(MonsterConfig config, eSpeMonsterType type)
        {
            Config = config;
            Type = type;
        }

        public MonsterConfig Config;
        public eSpeMonsterType Type;
    }

    public class SpeMonsterUtil
    {
        private static SpeMonsterUtil _instance;
        public static int AvailableSeconds = 120;

        private SpeMonsterUtil()
        {
            AvailableSeconds = int.Parse(Table.GetServerConfig(380).Value);

            var tmpDic = new[]
            {
                new Dictionary<string, List<WorldBOSSRecord>>(),
                new Dictionary<string, List<WorldBOSSRecord>>(),
                new Dictionary<string, List<WorldBOSSRecord>>()
            };
            Table.ForeachWorldBOSS(record =>
            {
                var dic = tmpDic[record.Type];
                var timeStr = record.RefleshTime;
                List<WorldBOSSRecord> records;
                if (!dic.TryGetValue(timeStr, out records))
                {
                    records = new List<WorldBOSSRecord>();
                    dic.Add(timeStr, records);
                }
                records.Add(record);
                return true;
            });
            var now = DateTime.Now.AddSeconds(-AvailableSeconds);
            for (int i = 0, imax = tmpDic.Length; i < imax; i++)
            {
                foreach (var dicItem in tmpDic[i])
                {
                    var timeIndex = 0;
                    var times = new List<DateTime>();
                    var timeStrs = dicItem.Key.Split('|');
                    foreach (var timeStr in timeStrs)
                    {
                        var dayTimes = timeStr.Split(':');
                        var hour = int.Parse(dayTimes[0]);
                        var min = int.Parse(dayTimes[1]);
                        var targetTime = new DateTime(now.Year, now.Month, now.Day, hour, min, 0, DateTimeKind.Local);
                        if (targetTime < now)
                        {
                            ++timeIndex;
                            targetTime = targetTime.AddDays(1);
                        }
                        times.Add(targetTime);
                    }
                    if (timeIndex >= timeStrs.Length)
                    {
                        timeIndex = 0;
                    }
                    SpeMonsterConfigs[i].Add(new MonsterConfig(times, dicItem.Value, timeIndex));
                }
            }
        }

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public List<MonsterConfig>[] SpeMonsterConfigs =
        {
            new List<MonsterConfig>(),
            new List<MonsterConfig>(),
            new List<MonsterConfig>()
        };

        public static SpeMonsterUtil Instance
        {
            get { return _instance ?? (_instance = new SpeMonsterUtil()); }
        }
    }
}