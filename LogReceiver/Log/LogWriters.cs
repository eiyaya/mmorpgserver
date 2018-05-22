// Decompiled with JetBrains decompiler
// Type: Log2Console.Settings.FieldType
// Assembly: Log2Console, Version=9.9.9.9, Culture=neutral, PublicKeyToken=null
// MVID: 44D35A25-C349-4FB9-B272-3AC90AA136EE
// Assembly location: C:\Users\rwahl\Desktop\Log2Console.exe

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using ServiceStack.Text;
using Shared;

namespace LogReceiver
{
    public class WriterData
    {
        public string Name;
        public StreamWriter Writer;
        public DateTime Time;
    }

    public static class LogWriters
    {
        public static readonly string ConfigFile = "../Config/LogReceiver.config";
        public static StreamWriter DefaultLog;
        public static LogReceiverConfig LogConfig;
        public static string TimeStampFormatString = "yyyy-MM-dd";
        public static bool Runing = true;

        public static ConcurrentDictionary<string, WriterData> Writers = new ConcurrentDictionary<string, WriterData>();

        public const int WriterLifeTime = 180000;

        public static void Init()
        {
            var fStream = new FileStream(ConfigFile, FileMode.Open, FileAccess.Read);
            LogConfig = XmlSerializer.DeserializeFromStream<LogReceiverConfig>(fStream);
            fStream.Close();

            DefaultLog = CreateWriter(LogConfig.SafeLog, DateTime.Now);

            Task.Run(() =>
            {
                while (Runing)
                {
                    var writers = Writers.Values.ToList();
                    foreach (var writer in writers)
                    {
                        var threshold = DateTime.Now.AddMilliseconds(-WriterLifeTime);
                        if (writer.Time < threshold)
                        {
                            RemoveWriter(writer);
                        }
                    }
                    Thread.Sleep(3000);
                }
            });
        }

        public static void Destory()
        {
            Runing = false;

            DefaultLog.Close();
            DefaultLog = null;

            foreach (var writer in Writers)
            {
                RemoveWriter(writer.Value);
            }
            Writers.Clear();
        }

        private static void RemoveWriter(WriterData data)
        {
            data.Writer.Close();
            data.Writer = null;
            Writers.TryRemove(data.Name, out data);
        }

        private static StreamWriter CreateWriter(string file, DateTime date)
        {
            var filePath = LogConfig.LogPath + date.ToString(TimeStampFormatString) + "/" + file;
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }
            return new StreamWriter(filePath, true);
        }

        public static StreamWriter GetWriter(string name, DateTime date)
        {
            if (Writers.ContainsKey(name))
            {
                var writer = Writers[name];
                writer.Time = date;
                return writer.Writer;
            }
            if (!name.Contains(LogConfig.CharacterPrefix)) return DefaultLog;

            var w = CreateWriter(name, date);
            var wd = new WriterData()
            {
                Name = name,
                Time = date,
                Writer = w,
            };
            Writers.GetOrAdd(name, wd);
            return w;
        }
    }
}
