#region using

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using DataContract;
using Scorpion;
using MySql.Data.MySqlClient;
using NLog;

using Shared;

#endregion

namespace Directory
{
    internal class DirectoryServer
    {
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private MySqlConnection mConnection = null;
        private string mConnectionString;
        private readonly Dictionary<string, Content> mContents = new Dictionary<string, Content>();
        private readonly ReaderWriterLockSlim mLock = new ReaderWriterLockSlim();
        private int mPort;
        private SocketListener mServer;
        private Random random;
        private TimeDispatcher mTimer;

        private bool CheckConnection()
        {
            try
            {
                if (mConnection == null)
                {
                    mConnection = new MySqlConnection(mConnectionString);
                    mConnection.Open();
                }

                if (!mConnection.Ping())
                {
                    Logger.Error("connect.ping return fasle!!!");
                    Logger.Error("CheckConnection 1 !!! connect.state={0}", mConnection.State);
                    mConnection.Close();
                    Logger.Error("CheckConnection 2 !!! connect.state={0}", mConnection.State);
                    mConnection.Open();
                    Logger.Error("CheckConnection 3 !!! connect.state={0}", mConnection.State);
                }
                return true;
            }
            catch (Exception ex)
            {
                var errorStr = string.Format("Try to reconnect to database " + mConnectionString + " failed.", ex);
                Console.WriteLine(errorStr);
                Logger.Error(errorStr);
                mConnection = null;
                return false;
            }
        }

        private void ClientConnected(ServerClient sender)
        {
            Logger.Debug("Client: " + sender.ClientId + " connected.");
            sender.MessageReceived += ClientMessageReceived;
            mTimer.RegisterTimedEvent(TimeSpan.FromMinutes(1), sender.Disconnect);
        }

        /// <summary>
        ///     This function will be called in multithread, so THREAD SAFE is very important.
        /// </summary>
        /// <param name="sender"></param>
        private void ClientDisconnected(ServerClient sender)
        {
            Logger.Debug("Client: " + sender.ClientId + " disconnected.");
        }

        /// <summary>
        ///     This function will be called in multithread, so THREAD SAFE is very important.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="desc"></param>
        private void ClientMessageReceived(ServerClient client, ServiceDesc desc)
        {
            try
            {
                Logger.Debug("Received a message from Client {0}, {1}.", client.ClientId, ((IPEndPoint)client.RemoteEndPoint).Address.MapToIPv4());

                if (desc.ServiceType == (int) ServiceType.Directory)

                {
                    switch (desc.FuncId)
                    {
                        case 8000:
                        {
                            var info =
                                ProtocolExtension
                                    .Deserialize
                                    <
                                        __RPC_Directory_CheckVersion_ARG_string_lang_string_platform_string_channel_string_version__
                                        >(desc.Data);

                            var lang = info.Lang.ToLowerInvariant();
                            var platform = info.Platform.ToLowerInvariant();
                            var channel = info.Channel.ToLowerInvariant();
                            var big = info.Version;

                            var sb = new StringBuilder();
                            sb.Append(lang);
                            sb.Append(".");
                            sb.Append(platform);
                            sb.Append(".");
                            sb.Append(channel);
                            sb.Append(".");
                            sb.Append(big);

                            try
                            {
                                mLock.EnterReadLock();

                                Content c;
                                if (mContents.TryGetValue(sb.ToString(), out c))
                                {
                                    var ret = new __RPC_Directory_CheckVersion_RET_VersionInfo__();
                                    ret.ReturnValue = new VersionInfo();
                                    ret.ReturnValue.SmallVersion = c.SmallVersion;
                                    ret.ReturnValue.AnnoucementURL = c.AnnoucementURL;
                                    ret.ReturnValue.ResourceURL = c.ResourceURL;
                                    ret.ReturnValue.HasNewVersion = c.HasNewVersion ? 1 : 0;
                                    ret.ReturnValue.NewVersionURL = c.NewVersionURL;
                                    ret.ReturnValue.ReviewState = c.ReviewState;

                                    var length = c.GateAddress.Length;
                                    if (length > 0)
                                    {
                                        if (length == 1)
                                        {
                                            ret.ReturnValue.GateAddress = c.GateAddress[0];
                                        }
                                        else
                                        {
                                            ret.ReturnValue.GateAddress = c.GateAddress[random.Next(length)];
                                        }
                                    }

                                    desc.Data = ProtocolExtension.Serialize(ret);

                                    client.SendMessage(desc);
                                }
                                else
                                {
                                    desc.Data = null;
                                    desc.Error = 1;
                                    client.SendMessage(desc);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex, "8000 error.");
                            }
                            finally
                            {
                                mLock.ExitReadLock();
                            }
                        }
                            break;
                        case 8001:
                        {
                            var info =
                                ProtocolExtension
                                    .Deserialize
                                    <
                                        __RPC_Directory_CheckVersion_ARG_string_lang_string_platform_string_channel_string_version__
                                        >(desc.Data);

                            var lang = info.Lang.ToLowerInvariant();
                            var platform = info.Platform.ToLowerInvariant();
                            var channel = info.Channel.ToLowerInvariant();
                            var big = info.Version;

                            var sb = new StringBuilder();
                            sb.Append(lang);
                            sb.Append(".");
                            sb.Append(platform);
                            sb.Append(".");
                            sb.Append(channel);
                            sb.Append(".");
                            sb.Append(big);

                            try
                            {
                                mLock.EnterReadLock();

                                Content c;
                                if (mContents.TryGetValue(sb.ToString(), out c))
                                {
                                    var ret = new __RPC_Directory_CheckVersion_RET_VersionInfo__();
                                    ret.ReturnValue = new VersionInfo();
                                    ret.ReturnValue.SmallVersion = c.SmallVersion;
                                    ret.ReturnValue.AnnoucementURL = c.AnnoucementURL;
                                    ret.ReturnValue.ResourceURL = c.ResourceURL;
                                    ret.ReturnValue.HasNewVersion = c.HasNewVersion ? 1 : 0;
                                    ret.ReturnValue.NewVersionURL = c.NewVersionURL;
                                    ret.ReturnValue.ReviewState = c.ReviewState;

                                    ret.ReturnValue.GateAddress = c.GateAddressString;

                                    desc.Data = ProtocolExtension.Serialize(ret);

                                    client.SendMessage(desc);
                                }
                                else
                                {
                                    desc.Data = null;
                                    desc.Error = 1;
                                    client.SendMessage(desc);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex, "8001 error.");
                            }
                            finally
                            {
                                mLock.ExitReadLock();
                            }
                        }
                            break;
                        case 8002:
                        {
                            var info =
                                ProtocolExtension
                                    .Deserialize
                                    <
                                        __RPC_Directory_CheckVersion3_ARG_string_lang_string_platform_string_channel_string_version__
                                        >(desc.Data);

                            var lang = info.Lang.ToLowerInvariant();
                            var platform = info.Platform.ToLowerInvariant();
                            var channel = info.Channel.ToLowerInvariant();
                            var big = info.Version;

                            var sb = new StringBuilder();
                            sb.Append(lang);
                            sb.Append(".");
                            sb.Append(platform);
                            sb.Append(".");
                            sb.Append(channel);
                            sb.Append(".");
                            sb.Append(big);

                            try
                            {
                                mLock.EnterReadLock();

                                Content c;
                                if (mContents.TryGetValue(sb.ToString(), out c))
                                {
                                    var ret = new __RPC_Directory_CheckVersion3_RET_VersionInfo__();
                                    ret.ReturnValue = new VersionInfo();
                                    ret.ReturnValue.SmallVersion = c.SmallVersion;
                                    ret.ReturnValue.AnnoucementURL = c.AnnoucementURL;
                                    ret.ReturnValue.ResourceURL = c.ResourceURL;
                                    ret.ReturnValue.HasNewVersion = c.HasNewVersion ? 1 : 0;
                                    ret.ReturnValue.NewVersionURL = c.NewVersionURL;
                                    ret.ReturnValue.ReviewState = c.ReviewState;

                                    if (c.IpWhiteList.Contains(((IPEndPoint)client.RemoteEndPoint).Address.MapToIPv4().ToString()))
                                    {
                                        ret.ReturnValue.GateAddress = c.RetargetGateAddress;
                                    }
                                    else
                                    {
                                        ret.ReturnValue.GateAddress = c.GateAddressString;
                                    }
                                    ret.ReturnValue.ForceShowAnn = c.ForceShowAnn;
                                    ret.ReturnValue.Isbn = c.Isbn;
                                    desc.Data = ProtocolExtension.Serialize(ret);

                                    client.SendMessage(desc);
                                }
                                else
                                {
                                    desc.Data = null;
                                    desc.Error = 1;
                                    client.SendMessage(desc);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex, "8001 error.");
                            }
                            finally
                            {
                                mLock.ExitReadLock();
                            }
                        }
                            break;
                        default:
                            Logger.Error("Unknown funcion id:{0}", desc.FuncId);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "ClientMessageReceived");
            }
        }

        public static void Main(string[] args)
        {
            var server = new DirectoryServer();
            server.Start(args);
        }

        public void Start(string[] args)
        {
            random = new Random(DateTime.Now.Second);
            if (args.Length == 0)
            {
                args = File.ReadAllLines("../Config/directory.config");
            }

            if (!int.TryParse(args[0], out mPort))
            {
                Logger.Warn(@"Gate server Start() Faild! args[1]={0}", args[1]);
                return;
            }

            try
            {
                mTimer = new TimeDispatcher("Directory");
                mTimer.Start();

                var settings = new SocketSettings(1000, 100, new IPEndPoint(IPAddress.Any, mPort));
                settings.Compress = true;
                mServer = new SocketListener(settings);

                mServer.ClientConnected += ClientConnected;
                mServer.ClientDisconnected += ClientDisconnected;

                mServer.StartListen();
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "Server start failed.");
            }

            try
            {
                mConnectionString = args[1];
                while (true)
                {
                    UpdateContent();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Connect to db {0} Failed.", args[1]);
            }
        }

        public void UpdateContent()
        {
            if (!CheckConnection())
            {
                // 等10秒再刷新
                Thread.Sleep(10000);
                return;
            }

            using (var command = mConnection.CreateCommand())
            {
                command.CommandText =
                    "select v.c1, " +
                    "(select c2 from Language where c1=v.c2) as c2, " +
                    "(select c2 from Platform where c1=v.c3) as c3, " +
                    "(select c2 from Channel where c1=v.c4) as c4, v.c5, v.c6, v.c7, v.c8, v.c9, v.c10, v.c11, v.c12, v.c13, v.c14 from Version as v;";
                command.CommandType = CommandType.Text;

                MySqlDataReader reader = null;

                try
                {
                    Logger.Info("Executing SQL: " + command.CommandText);

                    var sb = new StringBuilder();

                    reader = command.ExecuteReader();

                    mLock.EnterWriteLock();
                    while (reader.Read())
                    {
                        sb.Clear();

                        var lang = reader.GetString("c2").ToLowerInvariant();
                        var platform = reader.GetString("c3").ToLowerInvariant();
                        var channel = reader.GetString("c4").ToLowerInvariant();
                        var big = reader.GetString("c5");
                        sb.Append(lang);
                        sb.Append(".");
                        sb.Append(platform);
                        sb.Append(".");
                        sb.Append(channel);
                        sb.Append(".");
                        sb.Append(big);

                        Content c;
                        if (!mContents.TryGetValue(sb.ToString(), out c))
                        {
                            c = new Content();
                            c.Language = lang;
                            c.Platform = platform;
                            c.Channel = channel;
                            c.BigVersion = big;

                            mContents[sb.ToString()] = c;
                        }

                        c.SmallVersion = reader.GetInt32("c6");
                        c.ResourceURL = reader.GetString("c7");
                        c.AnnoucementURL = reader.GetString("c8");
                        c.HasNewVersion = reader.GetInt32("c9") != 0;
                        c.NewVersionURL = reader.GetString("c10");
                        var addresses = reader.GetString("c11");
                        c.ReviewState = reader.GetInt32("c12");
                        if (string.IsNullOrEmpty(addresses))
                        {
                            continue;
                        }
                        c.GateAddressString = addresses.Trim();
                        var address = addresses.Trim().Split(';');
                        c.GateAddress = address.Where(s => !string.IsNullOrEmpty(s)).ToArray();
                        c.ForceShowAnn = reader.GetInt32("c13");
                        c.Isbn = reader.GetString("c14").Replace("\\n", "\n");
                        c.IpWhiteList = new HashSet<string>(reader.GetString("c16").Split(';'));
                        c.RetargetGateAddress = reader.GetString("c17");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Execute {0} failed.", command.CommandText);
                    return;
                }
                finally
                {
                    mLock.ExitWriteLock();

                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
            }

            // 等10秒再刷新
            Thread.Sleep(10000);
        }

        private class Content
        {
            public string AnnoucementURL;
            public string BigVersion;
            public string Channel;
            public string GateAddressString;
            public string[] GateAddress;
            public bool HasNewVersion;
            public string Language;
            public string NewVersionURL;
            public string Platform;
            public string ResourceURL;
            public int ReviewState;
            public int SmallVersion;
            public int ForceShowAnn;
            public string Isbn;
            public HashSet<string> IpWhiteList;
            public string RetargetGateAddress;
        }
    }
}