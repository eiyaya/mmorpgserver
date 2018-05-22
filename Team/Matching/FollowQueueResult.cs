#region using

using System.Collections.Generic;
using NLog;

#endregion

namespace Team
{
    public interface IFollowQueueResult
    {
        //排队结果，所有人正常同意
        void AllOK(FollowQueueResult _this);
        ErrorCodes MatchingBack(FollowQueueResult _this, ulong guid, bool agree = false);
        void PushOneCharacter(FollowQueueResult _this, QueueCharacter queue);
        void RemoveCharacterList(FollowQueueResult _this, List<QueueCharacter> cs);
        void RemoveCharacterOne(FollowQueueResult _this, QueueCharacter character);
        void TimeOver(FollowQueueResult _this);
    }

    public class FollowQueueResultDefaultImpl : IFollowQueueResult
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void PushOneCharacter(FollowQueueResult _this, QueueCharacter queue)
        {
            QueueResultBase.GetImpl().PushOneCharacter(_this, queue);
            _this.Character = queue;
        }

        public void RemoveCharacterList(FollowQueueResult _this, List<QueueCharacter> cs)
        {
            RemoveCharacterOne(_this, cs[0]);
        }

        public void TimeOver(FollowQueueResult _this)
        {
            var character = _this.Character;
            var scene = QueueSceneManager.GetCharacterScene(character.mDatas[0].Id);
            if (scene == null)
            {
                return;
            }
            scene.FollowResult(character, 1);
            QueueManager.Pop(character.mDatas[0].Id, eLeaveMatchingType.TimeOut);
        }

        public void RemoveCharacterOne(FollowQueueResult _this, QueueCharacter character)
        {
            QueueResultBase.GetImpl().RemoveCharacterOne(_this, character);
            var scene = QueueSceneManager.GetCharacterScene(_this.Character.mDatas[0].Id);
            if (scene == null)
            {
                Logger.Error("In AllOK scene == null!!!");
                return;
            }
            scene.RemoveFollowCharacter(character);
        }

        public void AllOK(FollowQueueResult _this)
        {
            _this.mQueue.OnAllOk(_this);
            var scene = QueueSceneManager.GetCharacterScene(_this.Character.mDatas[0].Id);
            if (scene == null)
            {
                Logger.Error("In AllOK scene == null!!!");
                return;
            }
            scene.FollowResult(_this.Character, 1);
            QueueManager.Remove(_this.Character.mDatas[0].Id, eLeaveMatchingType.Success);
            _this.Character.result = null;
        }

        public ErrorCodes MatchingBack(FollowQueueResult _this, ulong guid, bool agree = false)
        {
            var err = QueueResultBase.GetImpl().MatchingBack(_this, guid, agree);
            if (!agree)
            {
                var scene = QueueSceneManager.GetCharacterScene(guid);
                if (scene == null)
                {
                    return err;
                }
                scene.FollowResult(_this.Character, 0);
                QueueManager.Pop(guid, eLeaveMatchingType.Refuse);
            }
            return err;
        }
    }

    //续排的QueueResult
    public class FollowQueueResult : QueueResultBase
    {
        private static IFollowQueueResult mImpl;

        static FollowQueueResult()
        {
            TeamServer.Instance.UpdateManager.InitStaticImpl(typeof (FollowQueueResult),
                typeof (FollowQueueResultDefaultImpl), o => { mImpl = (IFollowQueueResult) o; });
        }

        public FollowQueueResult(QueueLogic q) : base(q)
        {
        }

        public QueueCharacter Character;
        //排队结果，所有人正常同意
        public override void AllOK()
        {
            mImpl.AllOK(this);
        }

        //确认消息返回
        public override ErrorCodes MatchingBack(ulong guid, bool agree = false)
        {
            return mImpl.MatchingBack(this, guid, agree);
        }

        public override void PushOneCharacter(QueueCharacter queue)
        {
            mImpl.PushOneCharacter(this, queue);
        }

        //排队结果，移除一堆人
        public override void RemoveCharacterList(List<QueueCharacter> cs)
        {
            mImpl.RemoveCharacterList(this, cs);
        }

        //排队结果，移除一个人
        public override void RemoveCharacterOne(QueueCharacter character)
        {
            mImpl.RemoveCharacterOne(this, character);
        }

        //排队结果，添加一个队伍的人，目前只有活动副本的预约要用这个函数
        public override void TimeOver()
        {
            mImpl.TimeOver(this);
        }
    }
}