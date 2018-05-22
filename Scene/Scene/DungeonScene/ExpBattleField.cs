#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using C5;
using DataContract;
using EventSystem;
using Scorpion;
using NLog;
using Shared;
using Logger = NLog.Logger;

#endregion

namespace Scene
{
    public class ExpBattleField : DungeonScene
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //需要去获取的exData idList
        private static readonly Int32Array mIdList = new Int32Array();
        //每天最多打多长时间（秒）
        private static int mMaxTimeOneDaySec;
        private bool mIsResetTimerEventHandled;
        //排序用的，用来找出最快用完时间的玩家，以便将他踢出副本
        private readonly IntervalHeap<QueuePlayer> mKickQueue = new IntervalHeap<QueuePlayer>();
        //踢人的Timer，如果玩家副本时间结束了，需要把他踢出去
        private Trigger mKickTimer;
        //用来缓存最早到时间的那个用户
        private QueuePlayer mMinQueuePlayer;
        //用来找到ulong的玩家对应的QueuePlayer
        private readonly Dictionary<ulong, QueuePlayer> mQueuePlayerMap = new Dictionary<ulong, QueuePlayer>();
        //记录每个玩家的进入时间
        private readonly Dictionary<ulong, DateTime> PlayerEnterTimes = new Dictionary<ulong, DateTime>();

        private void ChangePlayedTime(ObjPlayer player)
        {
            CoroutineFactory.NewCoroutine(ChangePlayedTimeCoroutine, player).MoveNext();
        }

        private IEnumerator ChangePlayedTimeCoroutine(Coroutine co, ObjPlayer player)
        {
            var enterTime = PlayerEnterTimes[player.ObjId];
            PlayerEnterTimes.Remove(player.ObjId);
            var playedTimeSec = (int) (DateTime.Now - enterTime).TotalSeconds;
            var data = new Dict_int_int_Data();
            
            if (Math.Abs(mMaxTimeOneDaySec - playedTimeSec) < 10 )
            {
                data.Data.Add((int)eExdataDefine.e428, mMaxTimeOneDaySec);
            }
            else
            {
                data.Data.Add((int)eExdataDefine.e428, playedTimeSec);
            }
            var msg = SceneServer.Instance.LogicAgent.SSChangeExdata(player.ObjId, data);
            yield return msg.SendAndWaitUntilDone(co);
        }

        private void DeleteQueuePlayer(QueuePlayer queuePlayer, bool bChangeTimer = true)
        {
            mKickQueue.Delete(queuePlayer.Handle);
            mQueuePlayerMap.Remove(queuePlayer.Player.ObjId);
            if (!bChangeTimer)
            {
                return;
            }
            if (mKickQueue.Count > 0)
            {
                mMinQueuePlayer = mKickQueue.FindMin();
                SceneServerControl.Timer.ChangeTime(ref mKickTimer, mMinQueuePlayer.DueTime);
            }
            else
            {
                SceneServerControl.Timer.DeleteTrigger(mKickTimer);
                mKickTimer = null;
            }
        }

        private void FetchPlayerExData(ObjPlayer player)
        {
            CoroutineFactory.NewCoroutine(FetchPlayerExDataCoroutine, player).MoveNext();
        }

        //获取古战场今日游戏时间，并设置定时器（时间到了，要把玩家踢出副本）
        private IEnumerator FetchPlayerExDataCoroutine(Coroutine co, ObjPlayer player)
        {
            var msg = SceneServer.Instance.LogicAgent.SSFetchExdata(player.ObjId, mIdList);
            yield return msg.SendAndWaitUntilDone(co);

            if (msg.State != MessageState.Reply)
            {
                Logger.Error("In FetchPlayerExDataCoroutine(), SSFetchExdata() not replied! msg.State = {0}", msg.State);
                player.ExitDungeon();
                yield break;
            }
            if (msg.ErrorCode != (int) ErrorCodes.OK)
            {
                Logger.Error("In FetchPlayerExDataCoroutine(), SSFetchExdata() reply with errCode = {0}", msg.ErrorCode);
                player.ExitDungeon();
                yield break;
            }

            var exData = msg.Response.Items;
            var playedTimeSec = exData[0];
            var enterTime = PlayerEnterTimes[player.ObjId];
            var dueTime = enterTime.AddSeconds(mMaxTimeOneDaySec - playedTimeSec);

            //添加queuePlayer
            var queuePlayer = new QueuePlayer();
            queuePlayer.DueTime = dueTime;
            queuePlayer.Player = player;
            mKickQueue.Add(ref queuePlayer.Handle, queuePlayer);
            mQueuePlayerMap.Add(player.ObjId, queuePlayer);

            //检查并修改踢人的定时器
            mMinQueuePlayer = mKickQueue.FindMin();
            if (mKickTimer == null || mKickTimer.T == null)
            {
                mKickTimer = SceneServerControl.Timer.CreateTrigger(mMinQueuePlayer.DueTime, KickPlayers);
            }
            else
            {
                var trigger = mKickTimer;
                if (trigger.Time != mMinQueuePlayer.DueTime)
                {
                    SceneServerControl.Timer.ChangeTime(ref mKickTimer, mMinQueuePlayer.DueTime);
                }
            }

            //通知客户端倒计时
            player.Proxy.NotifyDungeonTime((int) State, (ulong) dueTime.ToBinary());
        }

        private void InitStaticVariable()
        {
            if (mMaxTimeOneDaySec != 0)
            {
                return;
            }
            mMaxTimeOneDaySec = (int)(mFubenRecord.TimeLimitMinutes*60);
            mIdList.Items.Add((int) eExdataDefine.e428);
        }

        //由于副本时间用完了，要Kick Players
        private void KickPlayers()
        {
            mMinQueuePlayer.Player.ExitDungeon();
            DeleteQueuePlayer(mMinQueuePlayer);
        }

        public override void OnCreate()
        {
            base.OnCreate();
            CloseTimer(eDungeonTimerType.SafeClose);

            State = eDungeonState.Start;
            InitStaticVariable();
            EventDispatcher.Instance.AddEventListener(ExpBattleFieldPlayTimeResetEvent.EVENT_TYPE,
                OnExpBattleFieldPlayTimeReset);
        }

        public override void OnDestroy()
        {
            EventDispatcher.Instance.RemoveEventListener(ExpBattleFieldPlayTimeResetEvent.EVENT_TYPE,
                OnExpBattleFieldPlayTimeReset);
            base.OnDestroy();
        }

        //
        private void OnExpBattleFieldPlayTimeReset(IEvent ievent)
        {
            if (mIsResetTimerEventHandled)
            {
                return;
            }
            mIsResetTimerEventHandled = true;
            SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddMinutes(1), () => mIsResetTimerEventHandled = false);

            var now = DateTime.Now;
            var list = PlayerEnterTimes.Keys.ToList();
            foreach (var key in list)
            {
                PlayerEnterTimes[key] = now;
            }

            if (mKickQueue.Count > 0)
            {
                foreach (var queuePlayer in mKickQueue)
                {
                    queuePlayer.DueTime = now.AddSeconds(mMaxTimeOneDaySec);

                    //通知客户端更新倒计时
                    if (null != queuePlayer.Player && null != queuePlayer.Player.Proxy)
                    {
                        queuePlayer.Player.Proxy.NotifyDungeonTime((int) State, (ulong) queuePlayer.DueTime.ToBinary());
                    }
                }

                mMinQueuePlayer = mKickQueue.FindMin();
                SceneServerControl.Timer.ChangeTime(ref mKickTimer, mMinQueuePlayer.DueTime);
            }
        }

        public override void OnPlayerEnter(ObjPlayer player)
        {
            base.OnPlayerEnter(player);

            PlayerEnterTimes[player.ObjId] = DateTime.Now;
            FetchPlayerExData(player);
        }

        public override void OnPlayerLeave(ObjPlayer player)
        {
            base.OnPlayerLeave(player);

            ChangePlayedTime(player);

            QueuePlayer queuePlayer;
            if (mQueuePlayerMap.TryGetValue(player.ObjId, out queuePlayer))
            {
//如果能找到，说明是主动退出的，而不是时间用光了，被踢掉的
                DeleteQueuePlayer(queuePlayer, queuePlayer == mMinQueuePlayer);
            }
        }

        //QueuePlayer，用来给玩家排序的数据结构，以便找出最早应该被踢出去的玩家
        private class QueuePlayer : IComparable<QueuePlayer>
        {
            public DateTime DueTime;
            public IPriorityQueueHandle<QueuePlayer> Handle;
            public ObjPlayer Player;

            public int CompareTo(QueuePlayer other)
            {
                if (DueTime < other.DueTime)
                {
                    return -1;
                }
                if (DueTime == other.DueTime)
                {
                    return 0;
                }
                return 1;
            }
        }
    }
}