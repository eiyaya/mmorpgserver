#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DataContract;
using DataTable;
using Scorpion;
using Mono.GameMath;
using NLog;
using Scene.Character;
using Shared;

#endregion

namespace Scene
{
    public class P1vP1Scene110000 : DungeonScene
    {
        // 缓存对战数据
        struct P1vP1Result
        {
            public string Name;
            public ulong Id;
            public int ServerId;
            public string NpcName;
        };
        private P1vP1Result mResult;

        private int FightFinally;
        private bool FightFinallyEnd;
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private AutoPlayer mAutoPlayer;
        public Dictionary<int, int> mDieDictionary = new Dictionary<int, int>();
        private bool mHasHold = true;
        private ObjPlayer mPlayer;
        private ulong pvpId;

        public override void EndDungeon()
        {
            Debug.Assert(!FightFinallyEnd);
            FightFinally = 0;
            FightFinallyEnd = true;
            CoroutineFactory.NewCoroutine(ResultOver).MoveNext();

            var result = new FubenResult();
            result.CompleteType = (int) eDungeonCompleteType.Failed;
            CompleteToAll(result, 10);
        }

        public override void ExitDungeon(ObjPlayer player)
        {
            if (!FightFinallyEnd)
            {
                FightFinallyEnd = true;
                if (mAutoPlayer != null)
                {
                    mAutoPlayer.EnterState(BehaviorState.Invalid); 
                }
                FightFinally = 0;
                CoroutineFactory.NewCoroutine(ResultOver).MoveNext();

                var result = new FubenResult();
                result.CompleteType = (int) eDungeonCompleteType.Failed;
                CompleteToAll(result, 10);
            }
        }

        public IEnumerator GetAutoPlayerInScene(Coroutine coroutine, ulong guid)
        {
            var msg = SceneServer.Instance.LogicAgent.GetLogicSimpleData(guid, 0);
            yield return msg.SendAndWaitUntilDone(coroutine);
            if (msg.State != MessageState.Reply)
            {
                Logger.Warn("GetAutoPlayerInScene GetLogicSimpleData False! guid={0}", guid);
                yield break;
            }

            var msg2 = SceneServer.Instance.SceneAgent.GetSceneSimpleData(guid, 0);
            yield return msg2.SendAndWaitUntilDone(coroutine);
            if (msg2.State != MessageState.Reply)
            {
                Logger.Warn("GetAutoPlayerInScene GetSceneSimpleData False! guid={0}", guid);
                yield break;
            }
            var tbScene = Table.GetScene(TypeId);
            if (string.IsNullOrEmpty(msg2.Response.Name))
            {
                Logger.Error("GetAutoPlayerInScene GetSceneSimpleData Name is null! guid={0}", guid);
            }
            mAutoPlayer = CreateAutoPlayer(msg.Response, msg2.Response,
                new Vector2((float) tbScene.PVPPosX, (float) tbScene.PVPPosZ), new Vector2(1, 0));
            mAutoPlayer.AddBuff(3001, 1, mAutoPlayer);
        }

        public override void OnNpcDie(ObjNPC npc, ulong characterId = 0)
        {
            if (State != eDungeonState.Start)
            {
                return;
            }

            if (!FightFinallyEnd && mAutoPlayer == npc)
            {
                FightFinally = 1;
                FightFinallyEnd = true;
                CoroutineFactory.NewCoroutine(ResultOver).MoveNext();

                var result = new FubenResult();
                result.CompleteType = (int) eDungeonCompleteType.Success;
                CompleteToAll(result, 10);
            }
        }

        public override void OnPlayerDie(ObjPlayer player, ulong characterId = 0)
        {
            if (!FightFinallyEnd)
            {
                FightFinally = 0;
                FightFinallyEnd = true;
                CoroutineFactory.NewCoroutine(ResultOver).MoveNext();

                var result = new FubenResult();
                result.CompleteType = (int) eDungeonCompleteType.Failed;
                CompleteToAll(result, 10);
            }
        }

        public override void AfterPlayerEnterOver(ObjPlayer player)
        {
            if (player.IsDead())
            {
                player.Relive();
            }
            else
            {
                player.Attr.SetDataValue(eAttributeType.HpNow, player.Attr.GetDataValue(eAttributeType.HpMax));
                player.Attr.SetDataValue(eAttributeType.MpNow, player.Attr.GetDataValue(eAttributeType.MpMax));
            }

            ChangeDifficulty(PlayerCount);

            mPlayer = player;
            pvpId = player.mDbData.P1vP1CharacterId;
            if (mAutoPlayer == null)
            {
                if (mHasHold)
                {
                    if (Trggers[(int) eDungeonTimerType.WaitStart] == null)
                    {
                        StartTimer(eDungeonTimerType.WaitStart, DateTime.Now.AddSeconds(5), TimeOverStart);
                    }
                    NotifyDungeonTime(player, eDungeonTimerType.WaitStart);
                    Exdata = 1;
                }
                else
                {
                    StartDungeon();
                    Exdata = 0;
                }

                player.AddBuff(3001, 1, player);
                if (StaticData.IsRobot(pvpId))
                {
                    var tbScene = Table.GetScene(TypeId);
                    mAutoPlayer = CreateAutoPlayer((int) pvpId,
                        new Vector2((float) tbScene.PVPPosX, (float) tbScene.PVPPosZ), new Vector2(1, 0));
                    mAutoPlayer.AddBuff(3001, 1, mAutoPlayer);
                }
                else
                {
                    CoroutineFactory.NewCoroutine(GetAutoPlayerInScene, pvpId).MoveNext();
                }
            }
        }
        public override void OnPlayerLeave(ObjPlayer player)
        {
            //修改副本难度
            ChangeDifficulty(PlayerCount);

            if (State <= eDungeonState.Start)
            {
                var result = new FubenResult();
                result.CompleteType = (int) eDungeonCompleteType.Quit;
                Complete(player.ObjId, result);
            }
            if (player.IsDead())
            {
                player.Relive();
            }
            else
            {
                player.Attr.SetDataValue(eAttributeType.HpNow, player.Attr.GetDataValue(eAttributeType.HpMax));
                player.Attr.SetDataValue(eAttributeType.MpNow, player.Attr.GetDataValue(eAttributeType.MpMax));
            }
            PlayerLog.WriteLog(player.ObjId, "CloseDungeon character is full！ c={0}", player.ObjId);
            CoroutineFactory.NewCoroutine(CloseDungeon).MoveNext();
        }

        public IEnumerator ResultOver(Coroutine co)
        {
            // 此处出现无效对象的引用，可能是异步执行的时候mPlayer或mAutoPlayer数据已被清空
            // 解决办法：增加mResult来缓存数据
            //PlayerLog.WriteLog(mPlayer.ObjId, "c={0},s={1},p={2},e={3},n1={4},n2={5}", mPlayer.ObjId, mPlayer.ServerId,
            //    pvpId, FightFinally, mPlayer.GetName(), mAutoPlayer.GetName());
            //var msgToRank = SceneServer.Instance.RankAgent.RankP1vP1FightOver(mPlayer.ObjId, mPlayer.ServerId,
            //    mPlayer.ObjId, pvpId, FightFinally, mPlayer.GetName(), mAutoPlayer.GetName());
            PlayerLog.WriteLog(mResult.Id, "c={0},s={1},p={2},e={3},n1={4},n2={5}", mResult.Id, mResult.ServerId,
                pvpId, FightFinally, mResult.Name, mResult.NpcName);
            var msgToRank = SceneServer.Instance.RankAgent.RankP1vP1FightOver(mResult.Id, mResult.ServerId,
                mResult.Id, pvpId, FightFinally, mResult.Name, mResult.NpcName);
            yield return msgToRank.SendAndWaitUntilDone(co);
        }

        public override void StartDungeon()
        {
            StartTimer(eDungeonTimerType.WaitEnd, DateTime.Now.AddMinutes(5), TimeOverEnd);
            mHasHold = false;
            Exdata = 0;

            base.StartDungeon();

            // 保存数据
            mResult.Name = mPlayer.GetName();
            mResult.Id = mPlayer.ObjId;
            mResult.ServerId = mPlayer.ServerId;
            mResult.NpcName = mAutoPlayer.GetName();
        }
    }
}