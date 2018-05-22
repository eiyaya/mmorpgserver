#region using

using System.Collections.Concurrent;
using Scorpion;

#endregion

namespace Broker
{
    internal interface IBrokerBase
    {
        bool Connected { get; }
        int ConnectionCount { get; }
        long PackageCount { get; }
        void ClearLostClient();
        void ClientConnected(ServerClient client);

        /// <summary>
        ///     This function will be called in multithread, so THREAD SAFE is very important.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ClientDisconnected(ServerClient client);

        /// <summary>
        ///     This function will be called in multithread, so THREAD SAFE is very important.
        ///     Called when gate pass a message, or other server pass a message.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ClientMessageReceived(ServerClient client, ServiceDesc desc);

        void ServerOnConnected();

        /// <summary>
        ///     This function will be called in multithread, so THREAD SAFE is very important.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        void ServerOnDisconnected();

        /// <summary>
        ///     This function will be called in multithread, so THREAD SAFE is very important.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ServerOnMessageReceived(ServiceDesc desc);

        void Start(int id, int nPort, string type, dynamic[] serverList);
        void Status(ConcurrentDictionary<string, string> dict);
        void Stop();
    }
}