using System.Net;
using Scorpion;

namespace GameMaster
{
    internal class GameMasterResponser : IHttpResponser
    {
        public void ProcessRequestAsync(HttpListenerContext context)
        {
            var waitingEvents = GameMasterServer.Instance.GameMasterAgent.mWaitingEvents;
            waitingEvents.Add(new ActionEvent(() =>
            {
                CoroutineFactory.NewCoroutine(GameMasterServerControl.Instance.ProcessRequestAsync, context).MoveNext();
            }));
        }
    }
}