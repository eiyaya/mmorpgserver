#region using

using DataTable;

#endregion

namespace Login
{
    public interface IServerQueue
    {
        PlayerQueueType CheckQueueState(ServerQueue _this, bool strick);
        void Construct(ServerQueue _this, int serverId);
        void UpdataNoWait(ServerQueue _this);
    }

    public class ServerQueueDefaultImpl : IServerQueue
    {
        public void Construct(ServerQueue _this, int serverId)
        {
            _this.SNRecord = Table.GetServerName(serverId);
        }

        public PlayerQueueType CheckQueueState(ServerQueue _this, bool strick)
        {
            var record = _this.SNRecord;
            var playerCount = QueueManager.PlayerCount[record.Id];
            var max = record.MaxLiveCount;
            if (max != -1 && playerCount >= max)
            {
                return PlayerQueueType.Wait;
            }
            return PlayerQueueType.NoWait;
        }

        public void UpdataNoWait(ServerQueue _this)
        {
            while (CheckQueueState(_this, false) == PlayerQueueType.NoWait)
            {
                var first = _this.GetFirstWaitPlayer();
                if (first == null)
                {
                    return;
                }
                if (first.State == ConnectState.WaitOffLine)
                {
                    continue;
                }
                if (first.Player == null)
                {
                    continue;
                }
                var dbPlayer = first.Player.DbData;
                first.SendClientQueueSuccess(QueueType.EnterGame, dbPlayer.SelectChar);
                QueueManager.PlayerEnterGameSuccess(dbPlayer.Id);
            }
        }
    }

    public class ServerQueue : QueueBase
    {
        private static IServerQueue mImpl;

        static ServerQueue()
        {
            LoginServer.Instance.UpdateManager.InitStaticImpl(typeof (ServerQueue), typeof (ServerQueueDefaultImpl),
                o => { mImpl = (IServerQueue) o; });
        }

        public ServerQueue(int serverId)
        {
            mImpl.Construct(this, serverId);
        }

        public ServerNameRecord SNRecord;

        public override PlayerQueueType CheckQueueState(bool strick)
        {
            return mImpl.CheckQueueState(this, strick);
        }

        public override void UpdataNoWait()
        {
            mImpl.UpdataNoWait(this);
        }
    }
}