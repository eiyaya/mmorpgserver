using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataContract;
using Scorpion;
using NLog;
using SceneClientService;

namespace Protocol
{
    public class WatchDogAgentControl : SceneAgent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public WatchDogAgentControl(string addr, uint id)
            : base(addr)
        {
            Id = id;
        }

        public override IEnumerator OnServerLost(Coroutine arg, string target)
        {
            throw new NotImplementedException();
        }

        public override IEnumerator OnServerConnected(Coroutine arg, string target)
        {
            throw new NotImplementedException();            
        }

        public override void OnException(Exception ex)
        {
            Logger.Error(ex, "Network error");
        }
    }
}
