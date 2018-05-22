#region using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Scorpion;
using NLog;

#endregion

namespace Shared
{
    public class RequestManager
    {
        private readonly Logger Logger = LogManager.GetLogger("LoginServerControl");
        private ServerAgentBase mAgentBase;

        public object DoRequest(Coroutine co, string url, Dictionary<string, string> postDictionary, AsyncReturnValue<string> result)
        {
            Task.Run(async () =>
            {
                HttpWebRequest request = null;
                WebResponse response = null;
                try
                {
                    request = (HttpWebRequest) WebRequest.Create(url);
                    request.Proxy = null;
                    request.KeepAlive = false;

                    request.Method = "POST";
                    var boundary = "--abcdef";
                    request.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);
                    request.Headers.Add("cache-control", "no-cache");
                    var sendStream = await request.GetRequestStreamAsync();
                    foreach (var pair in postDictionary)
                    {
                        AddContent(sendStream, pair.Key, pair.Value, boundary);
                    }
                    var sp = string.Format("--{0}--\r\n", boundary);
                    var data = Encoding.UTF8.GetBytes(sp);
                    sendStream.Write(data, 0, data.Length);
                    sendStream.Close();
                    response = await request.GetResponseAsync();
                    var stream = response.GetResponseStream();

                    if (stream != null)
                    {
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            result.Value = reader.ReadToEnd();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
                finally
                {
                    try
                    {
                        if (response != null) response.Close();
                        if (request != null) request.Abort();
                    }
                    catch(Exception ex)
                    {
                        Logger.Error(ex.ToString());
                    }

                    mAgentBase.mWaitingEvents.Add(new ContinueEvent(co));
                }
            });

            return null;
        }

        public RequestManager(ServerAgentBase agentBase)
        {
            mAgentBase = agentBase;

            ServicePointManager.DefaultConnectionLimit = 50;
            ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;
        }

        public void Stop()
        {
        }

        public static string Encrypt_MD5_UTF8(string appKey)
        {
            MD5 MD5 = new MD5CryptoServiceProvider();
            var datSource = Encoding.GetEncoding("utf-8").GetBytes(appKey);
            var newSource = MD5.ComputeHash(datSource);
            var sb = new StringBuilder(32);
            for (var i = 0; i < newSource.Length; i++)
            {
                sb.Append(newSource[i].ToString("x").PadLeft(2, '0'));
            }
            var crypt = sb.ToString();
            return crypt;
        }

        public static void AddContent(Stream stream, string name, string value, string boundary)
        {
            var sp = string.Format("--{0}\r\n", boundary);
            sp += string.Format(
                "Content-Disposition: form-data; name=\"{0}\"; \r\n\r\n{1}\r\n",
                name,
                value);
            var data = Encoding.UTF8.GetBytes(sp);
            stream.Write(data, 0, data.Length);
        }


    }
}