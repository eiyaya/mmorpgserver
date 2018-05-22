#region using

using Shared;

#endregion

namespace Login
{
    //整个服务器的queue
    public interface IGameQueue
    {
        PlayerQueueType CheckQueueState(GameQueue _this, bool strick);
        void UpdataNoWait(GameQueue _this);
    }

    public class GameQueueDefaultImpl : IGameQueue
    {
        public PlayerQueueType CheckQueueState(GameQueue _this, bool strick)
        {
            if (strick && _this.WaitPlayerCount >= StaticParam.WaitPlayerCountMax)
            {
                PlayerLog.WriteLog((int) LogType.Error_PLayerLoginMore, "have");
                return PlayerQueueType.More;
            }
            if (QueueManager.GamePlayerCount >= StaticParam.GamePlayerCountMax ||
                QueueManager.LandingPlayerCount >= StaticParam.LandingPlayerCountMax ||
                QueueManager.EnterGamePlayerCount >= StaticParam.EnterGamePlayerCountMax)
            {
                PlayerLog.WriteLog((int) LogType.Error_PLayerLoginWait, "have");
                return PlayerQueueType.Wait;
            }
            return PlayerQueueType.NoWait;
        }

        public void UpdataNoWait(GameQueue _this)
        {
            var index = 0;
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
                first.SendClientQueueSuccess(QueueType.Login, 0);
                QueueManager.LandingPlayerList.TryAdd(first.Player.DbData.Id, first);
                index++;
            }
        }
    }

    public class GameQueue : QueueBase
    {
        private static IGameQueue mImpl;

        static GameQueue()
        {
            LoginServer.Instance.UpdateManager.InitStaticImpl(typeof (GameQueue), typeof (GameQueueDefaultImpl),
                o => { mImpl = (IGameQueue) o; });
        }

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