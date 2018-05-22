#region using

using System;
using DataContract;
using NLog;

#endregion

namespace Scene
{
    //多人本基类的脚本
    public abstract class MultiplayerDungeon : DungeonScene
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public override void EndDungeon()
        {
            var result = new FubenResult();
            result.CompleteType = (int) eDungeonCompleteType.Failed;
            CompleteToAll(result);
        }

        public override void OnCreate()
        {
            base.OnCreate();

            State = eDungeonState.WillStart;

            //设置副本开启结束时间
            var now = DateTime.Now;
            var hour = Param.Param[0];
            var min = Param.Param[1];
            var startTime = new DateTime(now.Year, now.Month, now.Day, hour, min, 0, DateTimeKind.Local);
            StartTimer(eDungeonTimerType.WaitStart, startTime, TimeOverStart);
            StartTimer(eDungeonTimerType.WaitEnd, startTime.AddMinutes(mFubenRecord.TimeLimitMinutes), TimeOverEnd);
        }

        public override void OnPlayerLeave(ObjPlayer player)
        {
            if (State == eDungeonState.Start)
            {
                var result = new FubenResult();
                result.CompleteType = (int) eDungeonCompleteType.Quit;
                Complete(player.ObjId, result);
            }
            base.OnPlayerLeave(player);
        }
    }
}