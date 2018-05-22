#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataContract;
using DataTable;
using Scorpion;
using NLog;
using Shared;
using System.Diagnostics;

#endregion

namespace Scene
{
    //单人或组队副本(亡灵城堡和地狱监牢)
    public class UniformHellAndGhostDungeon : UniformTeamDungeon
    {

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        public override void OnCreate()
        {
            Log(Logger, "OnCreate");

            base.OnCreate();
            State = eDungeonState.WillStart;
            var GetFubenLogic = Table.GetFubenLogic(mFubenRecord.FubenLogicID);
            if (GetFubenLogic.FubenParam2.Length > 0)
            {
                var seconds = GetFubenLogic.FubenParam2[0];
                mFubenInfoMsg.Units[0].Params[0] = seconds;
                var startTime = DateTime.Now.AddSeconds(seconds);
                StartTimer(eDungeonTimerType.WaitStart, startTime, TimeOverStart);
                StartTimer(eDungeonTimerType.WaitEnd, startTime.AddMinutes(mFubenRecord.TimeLimitMinutes), TimeOverEnd);
            }
        }

        public override void StartDungeon()
        {
            Log(Logger, "StartDungeon");

            base.StartDungeon();
            DetectPhaseEnd();
        }
    }
}
