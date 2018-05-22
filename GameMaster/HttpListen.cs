using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataTable;

namespace GameMaster
{
    public interface IHttpResponser
    {
        void ProcessRequestAsync(HttpListenerContext context);
    }

    class HttpListen
    {
        private readonly HttpListener _listener; 
        private IHttpResponser _responser;
        private Thread _thread;
        private bool mRunning = true;

        public void Stop()
        {
            mRunning = false;
            if (_thread != null)
            {
                _thread.Join();
            }
        }

        public HttpListen(IHttpResponser resp, int port)
        {
            _responser = resp;
            string uri = string.Format(@"http://+:{0}/", port);
            _listener = new HttpListener();
            _listener.Prefixes.Add(uri);
            _listener.Start();
        }

        public void Start()
        {
            _thread = new Thread(async () =>
            {
                while (mRunning)
                {
                    var content = await _listener.GetContextAsync();
                    _responser.ProcessRequestAsync(content);
                }
            });
            _thread.Start();
        }
    }
}
