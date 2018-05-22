#region using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JsonConfig;
using NLog;
using Shared;
using Timer = System.Windows.Forms.Timer;
using Scorpion;

#endregion

namespace ServerHolder
{
    internal class Program
    {
        public static Process Gate;
        public static List<ServiceInfo> Infos = new List<ServiceInfo>();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static Process LogWindow;
        public static String MachineName = "a";
        public static Process Memcached;
        public static dynamic ServerConfig;
        public static List<ServerInfo> ServerInfos = new List<ServerInfo>();
        private static MainWindow window;

        private static void Main(string[] args)
        {
            Utility.InitEventLoop();

            Utility.FunctionCallLogger = new ScorpionLogger(LogManager.GetLogger("FunctionCallLogger"));
            Utility.Logger = new ScorpionLogger(LogManager.GetLogger("Network"));

            Application.ThreadException +=
                (sender, eventArgs) => { Logger.Fatal(eventArgs.Exception, "Application ThreadException. "); };

            AppDomain.CurrentDomain.UnhandledException +=
                (sender, eventArgs) =>
                {
                    Logger.Fatal(eventArgs.ExceptionObject as Exception, "CurrentDomain UnhandledException. ");
                };

            TaskScheduler.UnobservedTaskException +=
                (sender, eventArgs) =>
                {
                    Logger.Fatal(eventArgs.Exception, "TaskScheduler UnobservedTaskException. ");
                };

            if (args.Length > 0)
            {
                MachineName = args[0];
            }

            var localByName = Process.GetProcessesByName("TailBlazer");
            if (localByName.Length == 0)
            {
                var startInfo = new ProcessStartInfo("../Dependency/TailBlazer.exe");
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                startInfo.WorkingDirectory = "../Dependency/";
                try
                {
                    File.Delete("../logs/server.log");
                    File.Delete("../Dependency/logs/logger.log");
                }
                catch
                {
                    // do nothing.
                }
                startInfo.Arguments = "../logs/server.log";
                LogWindow = new Process();
                LogWindow.StartInfo = startInfo;
                LogWindow.Start();
                //Thread.Sleep(1000);
            }

            localByName = Process.GetProcessesByName("memcached");
            foreach (var process in localByName)
            {
                process.Kill();
                process.WaitForExit();
            }

            var t = new Timer();
            t.Interval = 1000;
            t.Tick += t_Tick;
            t.Start();

            Config.SetDefaultConfig(Config.ApplyJsonFromPath("../Config/default.conf"));
            ServerConfig = Config.ApplyJsonFromPath(Config.Default.ConfigFile.ToString());
            window = new MainWindow();
            window.ShowDialog();
        }

        public static void Start()
        {
            foreach (var config in Config.Default.Services)
            {
                var info = new ServiceInfo(config, ServerConfig);

                dynamic[] ServersConfig = null;

                if (info.Name == "Broker")
                {
                    var list = new List<dynamic>();
                    foreach (var name in GlobalVariable.ServerNames)
                    {
                        if (ServerConfig.BrokerServer.ContainsKey(name))
                        {
                            list.Add(ServerConfig.BrokerServer[name]);
                        }
                    }
                    ServersConfig = list.ToArray();
                }
                else if (info.Name == "Login")
                {
                    ServersConfig = ServerConfig.LoginServer;
                }
                else if (info.Name == "Activity")
                {
                    ServersConfig = ServerConfig.ActivityServer;
                }
                else if (info.Name == "Logic")
                {
                    ServersConfig = ServerConfig.LogicServer;
                }
                else if (info.Name == "Rank")
                {
                    ServersConfig = ServerConfig.RankServer;
                }
                else if (info.Name == "Scene")
                {
                    ServersConfig = ServerConfig.SceneServer;
                }
                else if (info.Name == "Chat")
                {
                    ServersConfig = ServerConfig.ChatServer;
                }
                else if (info.Name == "Team")
                {
                    ServersConfig = ServerConfig.TeamServer;
                }
                else if (info.Name == "Gate")
                {
                    ServersConfig = ServerConfig.GateServer;
                }
                else if (info.Name == "Memcached")
                {
                    ServersConfig = ServerConfig.Memcache;
                }
                else if (info.Name == "WatchDog")
                {
                    ServersConfig = ServerConfig.WatchDogServer;
                }
                else if (info.Name == "GameMaster")
                {
                    ServersConfig = ServerConfig.GameMasterServer;
                }

                dynamic dbConfig = ServerConfig.DBConfig;

                foreach (var server in ServersConfig)
                {
                    if (server.MachineName != MachineName)
                    {
                        continue;
                    }
                    var serverInfo = new ServerInfo(info, server, dbConfig);
                    ServerInfos.Add(serverInfo);
                    serverInfo.Start();

                    //Thread.Sleep(500);
                }
            }
            string title = "Server";
            title = ServerConfig.Name;
            window.Text = title;
            window.SetDataBinding(ServerInfos);
        }

        public static void Stop()
        {
            foreach (var info in ServerInfos)
            {
                info.Stop();
            }
        }

        private static void t_Tick(object sender, EventArgs e)
        {
            window.Info = "Fatal:" + LogListener.FatalCount + "    Error:" + LogListener.ErrorCount + "    Warning:" +
                          LogListener.WarningCount;

            if (LogListener.WarningCount > 0)
            {
                window.HeadColor = Color.Yellow;
            }

            if (LogListener.ErrorCount > 0)
            {
                window.HeadColor = Color.Red;
            }

            if (LogListener.FatalCount > 0)
            {
                window.HeadColor = Color.DarkRed;
            }
        }
    }
}