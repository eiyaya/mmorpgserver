#region using

using System.Collections.Generic;
using System.IO;
using NLog;
using ServiceStack.Text;

#endregion

namespace GameMaster
{
    public class LogReceiverConfig
    {
        public string CharacterPrefix;
        public List<string> LogPath;
        public string LogSurfix;
        public string SafeLog;
        public string TranslateConfig;
    }

    public static class LogSettings
    {
        private static LogReceiverConfig Config;
        private static readonly string ConfigFile = "../Config/LogReceiver.config";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static string GetFilePath(string logPath, ulong characterId, string date)
        {
            return logPath + date + "/" + Config.CharacterPrefix + characterId + Config.LogSurfix;
        }

        public static void Init()
        {
            var fStream = new FileStream(ConfigFile, FileMode.Open, FileAccess.Read);
            Config = XmlSerializer.DeserializeFromStream<LogReceiverConfig>(fStream);
            fStream.Close();
        }

        public static ErrorCodes ReadBlock(ulong characterId, string date, out string data)
        {
            var path = string.Empty;
            foreach (var dir in Config.LogPath)
            {
                var p = GetFilePath(dir, characterId, date);
                if (File.Exists(p))
                {
                    path = p;
                    break;
                }
            }
            if (string.IsNullOrEmpty(path))
            {
                Logger.Error("In ReadBlock(). Error_FileNotFind! path = {0}", path);
                data = string.Empty;
                return ErrorCodes.Error_FileNotFind;
            }
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var sr = new StreamReader(fs);
                data = sr.ReadToEnd();
            }
            return ErrorCodes.OK;
        }
    }
}