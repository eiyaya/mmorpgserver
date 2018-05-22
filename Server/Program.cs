#region using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AdysTech.InfluxDB.Client.Net;
using CommandLine;
using CommandLine.Text;
using JsonConfig;
using NLog;
using ServiceStack.Text;
using Shared;
using ThreadState = System.Threading.ThreadState;
using Scorpion;

#endregion

namespace Server
{
    internal class ServiceInfo
    {
        public ServiceInfo(dynamic config, dynamic serverConfig)
        {
            Config = config;
            ServerConfig = serverConfig;

            if (Config.Type == "dll")
            {
                var file = Path.Combine(Environment.CurrentDirectory, config.DLLName);
                Assembly ass = Assembly.LoadFile(file);
                ServiceType = ass.GetType(config.EntryType);
            }
            else if (Config.Type == "exe")
            {
            }
        }

        public dynamic Config { get; private set; }

        public string Name
        {
            get { return Config.ServiceName; }
        }

        public dynamic ServerConfig { get; private set; }
        public Type ServiceType { get; private set; }
    }

    internal class ServerInfo : IDisposable
    {
        public ServerInfo(ServiceInfo service, dynamic config, dynamic db, int id)
        {
            mConfig = config;
            mDb = db;
            mService = service;
            Id = id;

            if (mService.Config.Type == "dll")
            {
                mServer = (IServer) Activator.CreateInstance(mService.ServiceType);
            }
        }

        private readonly dynamic mConfig;
        private readonly dynamic mDb;
        private Process mProcess;
        private readonly IServer mServer;
        private readonly ServiceInfo mService;
        private bool mStarted;
        private readonly ConcurrentDictionary<string, string> mStatus = new ConcurrentDictionary<string, string>();
        private Thread mThread;
        private long mTick;
        private long mLastTick;
        public int mNoTickTime = 0;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public int Id { get; private set; }

        public string Ip
        {
            get { return mConfig.Ip.ToString(); }
        }

        public string Name
        {
            get
            {
                if (mService.Config.ServiceName.ToString() == "Broker")
                {
                    return mConfig.Type.ToString() + "Broker";
                }
                return mService.Config.ServiceName.ToString() + mConfig.Id.ToString();
            }
        }

        public string Port
        {
            get { return mConfig.Port.ToString(); }
        }

        public bool Alive
        {
            get { return mNoTickTime < 10; }
        }

        public ConcurrentDictionary<string, string> Status
        {
            get
            {
                if (mService.Config.Type == "dll")
                {
                    mStatus.Clear();
                    mServer.Status(mStatus);

                    string tick;
                    if (mStatus.TryGetValue("TickTime", out tick))
                    {
                        var t = long.Parse(tick);
                        if (t > 0)
                        {
                            if ((t - mLastTick) == 0)
                                mNoTickTime++;
                            else
                                mNoTickTime = 0;

                            mLastTick = mTick;
                            mTick = t;
                        }
                    }

                    return mStatus;
                }
                mStatus.Clear();
                mStatus.TryAdd("Status", mProcess != null && mProcess.HasExited ? "Stopped" : "Started");
                return mStatus;
            }
        }

        public string GetStackTrace()
        {
            try
            {
                if (mThread.ThreadState != ThreadState.Stopped &&
                    mThread.ThreadState != ThreadState.Aborted &&
                    mThread.ThreadState != ThreadState.Unstarted &&
                    mThread.ThreadState != ThreadState.Suspended)
                {
                    mThread.Suspend();
                }

                var s = new StackTrace(mThread, true);

                var sb = new StringBuilder();
                sb.AppendLine("Thread " + Name);
                sb.AppendLine("Stack:");
                for (var i = 0; i < s.FrameCount; i++)
                {
                    var f = s.GetFrame(i);
                    sb.Append("\t");
                    sb.Append(f.GetType());
                    sb.Append(".");
                    sb.Append(f.GetMethod());

                    if (!string.IsNullOrEmpty(f.GetFileName()))
                    {
                        sb.Append("\t\t");
                        sb.Append(f.GetFileName());
                        sb.Append(":");
                        sb.Append(f.GetFileLineNumber());
                    }
                    sb.AppendLine();
                }

                return sb.ToString();
            }
            catch
            {
            }
            finally
            {
                if (mThread.ThreadState == ThreadState.Suspended)
                {
                    mThread.Resume();
                }
            }

            return string.Empty;
        }

        public void Start()
        {
            if (mService.Config.Type == "dll")
            {
                if (mStarted)
                {
                    return;
                }

                if (mService.Config.ServiceName == "Broker")
                {
                    mThread = new Thread(() =>
                    {
                        var arg = new List<string>();
                        arg.Add(mConfig.Type.ToString());
                        arg.Add(mConfig.Port.ToString());
                        arg.Add(mConfig.Id.ToString());
                        arg.Add(mService.ServerConfig.ToString());

                        mServer.Init(arg.ToArray());
                        mServer.Start();
                    });
                }
                else
                {
                    mThread = new Thread(() =>
                    {
                        var arg = new List<string>();
                        arg.Add(mConfig.Id.ToString());
                        arg.Add(mConfig.Port.ToString());
                        arg.Add(mService.ServerConfig.Auth);
                        arg.Add(mService.ServerConfig.BrokerServer.ToString());
                        arg.Add(mDb.ToString());
                        arg.Add(mService.ServerConfig.ToString());
                        mServer.Init(arg.ToArray());
                        mServer.Start();
                    });
                }


                mThread.Name = Name;

                mThread.Priority = ThreadPriority.Highest;
                mThread.Start();

                mStarted = true;
            }
            else
            {
                Stop();

                var startInfo = new ProcessStartInfo(mService.Config.ExePath);
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                startInfo.WorkingDirectory = mService.Config.WorkingDirectory;
                startInfo.Arguments = mConfig.Port.ToString();
                mProcess = new Process();
                mProcess.StartInfo = startInfo;
                mProcess.Start();
            }
        }

        public void Stop()
        {
            if (mService.Config.Type == "dll")
            {
                if (!mStarted)
                {
                    return;
                }

                if (mThread == null)
                {
                    return;
                }

                if (mServer == null)
                {
                    return;
                }

                mServer.Stop();
                mThread.Join();
                mStarted = false;
            }
            else
            {
                var name = Path.GetFileNameWithoutExtension(mService.Config.ExePath);
                Process[] localByName = Process.GetProcessesByName(name);
                if (localByName.Length > 0)
                {
                    foreach (var process in localByName)
                    {
                        process.Kill();
                        process.WaitForExit();
                    }
                }
            }
        }

        public void Dispose()
        {
            mProcess.Dispose();
        }

        internal void Rescue()
        {
            if (mService.Config.Type == "dll")
            {
                Console.WriteLine("Start rescue " + Name);

                if (!mStarted)
                {
                    return;
                }

                if (mThread != null)
                {
                    int tryCount = 3;
                    while (mThread.ThreadState != ThreadState.Stopped && tryCount > 0)
                    {
                        try
                        {
                            mThread.Abort();
                        }
                        catch (ThreadStateException ex1)
                        {
                            Logger.Error(ex1, "Rescue error 1.");
                            try
                            {
#pragma warning disable CS0618 // 类型或成员已过时
                                mThread.Resume();
#pragma warning restore CS0618 // 类型或成员已过时
                            }
                            catch (ThreadStateException ex2)
                            {
                                Logger.Error(ex2, "Rescue error 2.");
                            }
                        }

                        tryCount--;
                        Thread.Sleep(1000);
                    }
                }

                mThread = new Thread(() =>
                {
                    mServer.Rescue();
                });

                mThread.Name = Name;

                mThread.Priority = ThreadPriority.Highest;
                mThread.Start();
            }
            else
            {
                Stop();

                var startInfo = new ProcessStartInfo(mService.Config.ExePath);
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                startInfo.WorkingDirectory = mService.Config.WorkingDirectory;
                startInfo.Arguments = mConfig.Port.ToString();
                mProcess = new Process();
                mProcess.StartInfo = startInfo;
                mProcess.Start();
            }
        }
    }

    internal class Program
    {
        private static InfluxDBClient client;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Options sOptions = new Options();

        private static void Main(string[] args)
        {
            if (!Parser.Default.ParseArguments(args, sOptions))
            {
                Console.WriteLine(sOptions.GetUsage());
                Logger.Error(sOptions.GetUsage());
                return;
            }

            Utility.FunctionCallLogger = new ScorpionLogger(LogManager.GetLogger("FunctionCallLogger"));
            Utility.Logger = new ScorpionLogger(LogManager.GetLogger("Network"));

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

            var machine = sOptions.MachineName;
            var id = -1;
            Int32.TryParse(sOptions.ServerId, out id);

            Logger.Info("server id [{0}] is starting, machine [{1}]", id, machine);

            Config.SetDefaultConfig(Config.ApplyJsonFromPath("../Config/default.conf"));
            var ServerConfig = Config.ApplyJsonFromPath(Config.Default.ConfigFile.ToString());
            var dbConfig = ServerConfig.DBConfig;

            var serverInfos = new List<ServerInfo>();

            var started = false;

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

                foreach (var server in ServersConfig)
                {
                    if (id > 0 && server.Id != id)
                    {
                        continue;
                    }

                    if (machine != server.MachineName)
                    {
                        continue;
                    }

                    Logger.Info("server id [{0}] is starting as {1} server at {2}:{3}", server.Id, info.Name, server.Ip,
                        server.Port);
                    var serverInfo = new ServerInfo(info, server, dbConfig, server.Id);
                    serverInfos.Add(serverInfo);
                    serverInfo.Start();
					string log = string.Format("{0,-20}started.  {1,-10} [{2}:{3}] {4,-20}", 
						"[" + serverInfo.Name + "]",
						"[" + serverInfo.Id + "]", 
						server.Ip, server.Port,
						"[" + machine + "]");
					Console.WriteLine(log);

                    started = true;
                }
            }

            if (!started)
            {
                Logger.Error("Can not find a server " + sOptions.ServerId + " at " + sOptions.MachineName +
                             " in configuration.");
                return;
            }

            try
            {
                if (!string.IsNullOrEmpty(ServerConfig.InfluxDb.Uri.ToString()))
                {
                    client = new InfluxDBClient(ServerConfig.InfluxDb.Uri, ServerConfig.InfluxDb.UserName,
                        ServerConfig.InfluxDb.Password);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }

            while (!File.Exists("exit"))
            {
                Thread.Sleep(1000);

                if (!Directory.Exists("../status"))
                {
                    Directory.CreateDirectory("../status");
                }

                if (File.Exists("stack"))
                {
                    var sb = new StringBuilder();

                    foreach (var info in serverInfos)
                    {
                        sb.AppendLine(info.GetStackTrace());
                    }

                    File.AppendAllText("../status/stack.txt", sb.ToString());
                    File.Delete("stack");
                }

                foreach (var info in serverInfos)
                {
                    try
                    {
                        var status = info.Status;
                        File.WriteAllText("../status/" + info.Name + ".txt", status.Dump());

                        if (client != null)
                        {
                            var p = new InfluxDatapoint<string>();
                            p.MeasurementName = info.Name.Trim();
                            p.UtcTimestamp = DateTime.Now;
                            p.Precision = TimePrecision.Seconds;
                            foreach (var s in status)
                            {
                                p.Fields.Add(s.Key, s.Value);
                            }

                            Task.WaitAll(client.PostPointAsync("server", p));
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Send state error.");
                    }
                }
#if !DEBUG
                foreach (var info in serverInfos)
                {
                    try
                    {
                        if (!info.Alive)
                        {
                            var sb = new StringBuilder();
                            sb.AppendLine(info.GetStackTrace());
                            File.AppendAllText("../status/stack.txt", sb.ToString());

                            info.Rescue();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Send state error.");
                    }
                }

#endif
			}

			Console.WriteLine("------------------------------------");
			Console.WriteLine("Please hold on,program is quiting...");
			Console.WriteLine("------------------------------------");
            foreach (var info in serverInfos)
            {
                info.Stop();
            }
        }

        private class Options
        {
            [Option('m', "machine-name", Required = true, HelpText = "Set machine name of this server.")]
            public string MachineName { get; set; }

            [Option('s', "server-id", DefaultValue = "*", HelpText = "Set server id of this server, * means all server."
                )]
            public string ServerId { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild(this,
                    (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
            }
        }
    }
}