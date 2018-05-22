#region using

using System;
using System.Collections.Generic;
using NLog;
using Shared;

#endregion

namespace Login
{
    public enum PlayerQueueType
    {
        NoWait = 0, //不需要排队
        Wait = 1, //需要排队
        More = 2 //压力太大了，队都别排了
    }

    public interface IQueueBase
    {
        //取出最早的一个排队,通知进入
        PlayerConnect GetFirstWaitPlayer(QueueBase _this);
        //增加排队
        PlayerQueueType PushConnect(QueueBase _this,
                                    string type,
                                    ulong clientId,
                                    string name,
                                    PlayerController playerController);

        void Update(QueueBase _this);
    }

    public class QueueBaseDefaultImpl : IQueueBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private void UpdataIndex(QueueBase _this)
        {
            var index = 0;
            foreach (var connet in _this.WaitPlayerList)
            {
                if (index >= 50)
                {
                    break;
                }
                index++;
                connet.SendClientQueueIndex(index);
            }
        }

        public PlayerQueueType PushConnect(QueueBase _this,
                                           string type,
                                           ulong clientId,
                                           string name,
                                           PlayerController playerController)
        {
            //是否曾经在排队过程中
            PlayerConnect findConnect;
            var key = PlayerConnect.GetLandingKey(type, name);
            if (QueueManager.CacheLost.TryGetValue(key, out findConnect))
            {
                findConnect.IsOnline = true;
                QueueManager.CacheLost.Remove(key);
                findConnect.ClientId = clientId;
                playerController.Connect = findConnect;
                switch (findConnect.State)
                {
                    case ConnectState.Wait:
                    //QueueManager.TotalList.TryAdd(clientId, findConnect);
                    //break;
                    case ConnectState.Landing:
                    case ConnectState.EnterGame:
                    case ConnectState.InGame:
                        Logger.Warn("PushConnect StateERROR clientId={0},name={1},State={2}", clientId, name,
                            findConnect.State);
                        break;
                    case ConnectState.OffLine:
                    {
                        var checkState = _this.CheckQueueState();
                        QueueManager.TotalList.TryAdd(clientId, findConnect);
                        findConnect.Player = playerController;
                        if (checkState == PlayerQueueType.NoWait)
                        {
                            findConnect.State = ConnectState.Landing;
                            QueueManager.LandingPlayerList.TryAdd(playerController.DbData.Id, findConnect);
                            return PlayerQueueType.NoWait;
                        }
                        findConnect.State = ConnectState.Wait;
                        _this.WaitPlayerList.AddFirst(findConnect);
                    }
                        break;
                    case ConnectState.WaitOffLine:
                    {
                        var checkState = _this.CheckQueueState();
                        QueueManager.TotalList.TryAdd(clientId, findConnect);
                        findConnect.Player = playerController;
                        if (checkState == PlayerQueueType.NoWait)
                        {
                            findConnect.State = ConnectState.Landing;
                            QueueManager.LandingPlayerList.TryAdd(playerController.DbData.Id, findConnect);
                            return PlayerQueueType.NoWait;
                        }
                        findConnect.State = ConnectState.Wait;
                    }
                        break;
                }
                return PlayerQueueType.Wait;
            }
            //增加到新的排队
            QueueManager.TotalList.TryGetValue(clientId, out findConnect);
            var check = _this.CheckQueueState();
            switch (check)
            {
                case PlayerQueueType.NoWait:
                {
                    if (findConnect == null)
                    {
                        findConnect = new PlayerConnect(type, clientId, name, ConnectState.Landing);
                        QueueManager.TotalList.TryAdd(clientId, findConnect);
                    }
                    playerController.Connect = findConnect;
                    findConnect.Player = playerController;
                    QueueManager.LandingPlayerList.TryAdd(findConnect.Player.DbData.Id, findConnect);
                }
                    break;
                case PlayerQueueType.Wait:
                {
                    if (findConnect == null)
                    {
                        findConnect = new PlayerConnect(type, clientId, name);
                        QueueManager.TotalList.TryAdd(clientId, findConnect);
                    }
                    playerController.Connect = findConnect;
                    findConnect.Player = playerController;
                    _this.WaitPlayerList.AddLast(findConnect);
                }
                    break;
                case PlayerQueueType.More:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return check;
        }

        public PlayerConnect GetFirstWaitPlayer(QueueBase _this)
        {
            if (_this.WaitPlayerCount > 0)
            {
                var temp = _this.WaitPlayerList.First.Value;
                _this.WaitPlayerList.RemoveFirst();
                if (temp.State == ConnectState.WaitOffLine)
                {
                    QueueManager.CacheLost.Remove(temp.GetKey());
                }
                else
                {
                    temp.State = ConnectState.Landing;
                }
                return temp;
            }
            return null;
        }

        public void Update(QueueBase _this)
        {
            var tick = QueueManager.Tick;
            if (tick%2 == 0)
            {
                _this.UpdataNoWait();
            }
            if (tick >= 30)
            {
                UpdataIndex(_this);
            }
        }
    }


    public abstract class QueueBase
    {
        static QueueBase()
        {
            LoginServer.Instance.UpdateManager.InitStaticImpl(typeof (QueueBase), typeof (QueueBaseDefaultImpl),
                o => { mImpl = (IQueueBase) o; });
        }

        #region 读取数据

        //取出最早的一个排队,通知进入
        public PlayerConnect GetFirstWaitPlayer()
        {
            return mImpl.GetFirstWaitPlayer(this);
        }

        #endregion

        #region 数据

        private static IQueueBase mImpl;

        public int WaitPlayerCount
        {
            get { return WaitPlayerList.Count; }
        } //正在排队的玩家

        //排队的玩家
        public LinkedList<PlayerConnect> WaitPlayerList = new LinkedList<PlayerConnect>();

        #endregion

        #region 接口

        //是否需要排队的判断
        public abstract PlayerQueueType CheckQueueState(bool strick = true);

        //增加排队
        public PlayerQueueType PushConnect(string type, ulong clientId, string name, PlayerController playerController)
        {
            return mImpl.PushConnect(this, type, clientId, name, playerController);
        }

        #endregion

        #region 心跳相关

        public void Update()
        {
            mImpl.Update(this);
        }

        public abstract void UpdataNoWait();

        #endregion
    }
}