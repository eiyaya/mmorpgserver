#region using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using NLog;
using ServiceStack.Text;
using ThreadState = System.Threading.ThreadState;
using Scorpion;

#endregion

namespace ServerHolder
{
    internal class ServerInfo : IDisposable
    {
        public ServerInfo(ServiceInfo service, dynamic config, dynamic db)
        {
            mConfig = config;
            mDb = db;
            mService = service;

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

        public string Status
        {
            get
            {
                if (mService.Config.Type == "dll")
                {
                    mStatus.Clear();
                    mServer.Status(mStatus);
                    return mStatus.Dump();
                }
                return mProcess != null && mProcess.HasExited ? "Stopped" : "Started";
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
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex, "dump stack error for {0}", Name);
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
    }
}