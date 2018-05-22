#region using

using System;
using System.Collections.Generic;
using System.Linq;
using Shared;

#endregion

namespace Team
{
    internal interface IFubenQueue
    {
        void Construct(FubenQueue _this, int queueId);
        void MatchOver(FubenQueue _this);
        void Tick(FubenQueue _this);
    }

    internal class FubenQueueDefaultImpl : IFubenQueue
    {
        public void Construct(FubenQueue _this, int queueId)
        {
        }

        public void Tick(FubenQueue _this)
        {
            var needCount = _this.tbQueue.CountLimit;
            _this.tempTeam.Clear();
            var isCanOver = false;
            foreach (var mCharacter in _this.mCharacters)
            {
                if (mCharacter.mDatas.Count <= needCount)
                {
                    _this.tempTeam.Add(mCharacter);
                    needCount -= mCharacter.mDatas.Count;
                    if (needCount == 0)
                    {
                        isCanOver = true;
                        break;
                    }
                }
            }
            if (isCanOver)
            {
                _this.MatchOver();
            }
        }

        //通知匹配成功
        public void MatchOver(FubenQueue _this)
        {
            foreach (var character in _this.tempTeam)
            {
                PlayerLog.WriteLog((int) LogType.QueueMessage, "MatchOver c={0} ids={1}", character.Guid,
                    character.mDatas.Select(d => d.Id).GetDataString());
                _this.mCharacters.Remove(character);
                _this.PushSuccessTime((int) DateTime.Now.GetDiffSeconds(character.StartTime));
            }
            var result = new QueueResult(_this);
            result.PushCharacter(_this.tempTeam);
            result.StartTrigger();
            _this.tempTeam = new List<QueueCharacter>();
        }
    }

    internal class FubenQueue : QueueLogic
    {
        private static IFubenQueue mImpl;

        static FubenQueue()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (FubenQueue), typeof (FubenQueueDefaultImpl),
                o => { mImpl = (IFubenQueue) o; });
        }

        public FubenQueue(int queueId)
            : base(queueId)
        {
            mImpl.Construct(this, queueId);
        }

        //心跳
        public List<QueueCharacter> tempTeam = new List<QueueCharacter>();
        //通知匹配成功
        public override void MatchOver()
        {
            mImpl.MatchOver(this);
        }

        public override void Tick()
        {
            mImpl.Tick(this);
        }
    }
}