using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NLog;

namespace LogReceiver
{
    [Serializable]
    public class TcpReceiver
    {
        #region Logger
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region Port Property

        int _port = 4505;
        [Category("Configuration")]
        [DisplayName("TCP Port Number")]
        [DefaultValue(4505)]
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        #endregion

        #region IpV6 Property

        bool _ipv6;
        [Category("Configuration")]
        [DisplayName("Use IPv6 Addresses")]
        [DefaultValue(false)]
        public bool IpV6
        {
            get { return _ipv6; }
            set { _ipv6 = value; }
        }

        private int _bufferSize = 10000;
        [Category("Configuration")]
        [DisplayName("Receive Buffer Size")]
        [DefaultValue(10000)]
        public int BufferSize
        {
            get { return _bufferSize; }
            set { _bufferSize = value; }
        }

        #endregion

        #region IReceiver Members

        [Browsable(false)]
        public string SampleClientConfig
        {
            get
            {
                return
                    "Configuration for NLog:" + Environment.NewLine +
                    "<target name=\"TcpOutlet\" xsi:type=\"NLogViewer\" address=\"tcp://localhost:4505\"/>";
            }
        }

        [NonSerialized]
        Socket _socket;

        private const char ElementSeperator = ',';

        public void Initialize()
        {
            if (_socket != null) return;

            _socket = new Socket(_ipv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.ExclusiveAddressUse = true;
            _socket.Bind(new IPEndPoint(_ipv6 ? IPAddress.IPv6Any : IPAddress.Any, _port));
            _socket.Listen(100);
            _socket.ReceiveBufferSize = _bufferSize;

            var args = new SocketAsyncEventArgs();
            args.Completed += AcceptAsyncCompleted;

            _socket.AcceptAsync(args);
        }

        void AcceptAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            Logger.Warn("AcceptAsyncCompleted new thread:{0}", e.LastOperation);
            if (_socket == null || e.SocketError != SocketError.Success) return;

            if (e.LastOperation == SocketAsyncOperation.Accept)
            {
                var thread = new Thread(Start) { IsBackground = true };
                thread.Start(e.AcceptSocket);
                e.AcceptSocket = null;
                _socket.AcceptAsync(e);
            }
        }

        void Start(object newSocket)
        {
            try
            {
                using (var socket = (Socket)newSocket)
                using (var ns = new NetworkStream(socket, FileAccess.Read, false))
                using (var reader = new StreamReader(ns))
                {
                    while (_socket != null)
                    {
                        var entry = reader.ReadLine();
                        if (string.IsNullOrEmpty(entry)) continue;

                        var elements = entry.Split(ElementSeperator);
                        var dateBinaryStr = elements[0];
                        var date = DateTime.FromBinary(long.Parse(dateBinaryStr));
                        var logName = elements[1];
                        var level = elements[2];
                        var message = entry.Substring(dateBinaryStr.Length + logName.Length + level.Length + 3);
                        var content = string.Format("{0} {1}", date.ToString("HH:mm:ss"), message);
                        var writer = LogWriters.GetWriter(logName, date);
                        writer.WriteLine(content);
                        writer.Flush();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Logger.Error(e);
            }
        }

        public void Terminate()
        {
            if (_socket == null) return;

            _socket.Close();
            _socket = null;
        }

        #endregion
    }
}
