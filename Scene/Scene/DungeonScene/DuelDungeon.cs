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

// 角斗副本

namespace Scene
{
    public class DuelDungeon : DungeonScene
    {
        // 日志
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        // 玩家缓存
        private Dictionary<ulong, string> mPlayerInfo = new Dictionary<ulong, string>();

        private List<ObjPlayer> mPlayers = new List<ObjPlayer>();
        // 是否结束
        private bool mIsEnd;

        private Trigger mDeadWaitExitTrigger;
        public override void EndDungeon()
        {
            // 限制buff
            foreach (var p in mPlayers)
            {
                if (null != p)
                {
                    p.AddBuff(3001, 1, p);
                }
            }

            // 平局？？
            CoroutineFactory.NewCoroutine(ResultOver, mPlayers[0], true).MoveNext();

            // 3秒后退出
            EnterAutoClose(3);
        }

        public override void ExitDungeon(ObjPlayer player)
        {
            CoroutineFactory.NewCoroutine(ResultOver, player, false).MoveNext();

            // 玩家手动退出副本?
            player.ExitDungeon();
        }

        public override void OnPlayerDie(ObjPlayer player, ulong characterId = 0)
        {
            CoroutineFactory.NewCoroutine(ResultOver, player, false).MoveNext();

            // 是否需要延时几秒再退出场景？？
            // 离开副本
            mDeadWaitExitTrigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(2.0f), () => { OnDeadExit(player);});
        }

        private void OnDeadExit(ObjPlayer player)
        {
            mDeadWaitExitTrigger = null;
            player.ExitDungeon();
        }

        private void FixPostion(ObjPlayer player)
        {
            if (player.ObjId == Param.ObjId)
            {
                player.SetPosition((float)TableSceneData.Entry_x, (float)TableSceneData.Entry_z);
            }
            else
            {
                player.SetPosition((float)TableSceneData.PVPPosX, (float)TableSceneData.PVPPosZ);
            }
        }

        public override void OnObjBeforeEnterScene(ObjBase obj)
        {
            if (ObjType.PLAYER != obj.GetObjType())
                return;

            var player = obj as ObjPlayer;

            FixPostion(player);
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

            if (mPlayerInfo.ContainsKey(player.ObjId))
                return;

            mPlayerInfo.Add(player.ObjId, player.GetName());
            mPlayers.Add(player);

            // 限制buff
            player.AddBuff(3001, 1, player);
            // 同步位置
            player.SyncCharacterPostion();

            if (Trggers[(int) eDungeonTimerType.WaitStart] == null)
            {
                StartTimer(eDungeonTimerType.WaitStart, DateTime.Now.AddSeconds(5), TimeOverStart);
            }
            NotifyDungeonTime(player, eDungeonTimerType.WaitStart);

            OnTriggerStartWarn(player);
        }

        public override void OnPlayerLeave(ObjPlayer player)
        {
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
        }

        public IEnumerator ResultOver(Coroutine co, ObjPlayer player, bool end = false)
        {
            if (mIsEnd)
            {// 已经结算过了
                yield break;
            }
            mIsEnd = true;

            ulong winnerId = 0;
            foreach (var objPlayer in mPlayerInfo)
            {
                if (objPlayer.Key != player.ObjId)
                {
                    winnerId = objPlayer.Key;
                    break;
                }
            }
            if (0 == winnerId)
            {//异常了？？
                yield break;
            }

            // 世界广播
            string content;
            var args = new List<string>();

            if (end)
            {
                //平手
                args.Add(mPlayerInfo[winnerId]);
                args.Add(player.GetName());
                content = Utils.WrapDictionaryId(100003326, args);
            }
            else
            {
                if (player.IsDead())
                {
                    //胜负
                    args.Add(mPlayerInfo[winnerId]);
                    args.Add(player.GetName());
                    content = Utils.WrapDictionaryId(100003327, args);
                }
                else
                {
                    //放弃
                    args.Add(player.GetName());
                    args.Add(mPlayerInfo[winnerId]);
                    content = Utils.WrapDictionaryId(100003325, args);
                }
            }

            var msg2 = SceneServer.Instance.ChatAgent.SSBroadcastAllServerMsg(player.ObjId, (int)eChatChannel.SystemScroll, player.GetName(), new ChatMessageContent { Content = content });
            yield return msg2.SendAndWaitUntilDone(co);

            // 5秒后自动关闭
            if (false == end)
            {
                EnterAutoClose(5);
            }
        }

        public override void StartDungeon()
        {
            StartTimer(eDungeonTimerType.WaitEnd, DateTime.Now.AddMinutes(mFubenRecord.TimeLimitMinutes), TimeOverEnd);
            Exdata = 0;

            base.StartDungeon();
        }

        private void OnTriggerStartWarn(ObjPlayer player)
        {
            if (null == player)
                return;

            //"战斗将在5秒后开始！"
            player.Proxy.NotifyMessage((int)eSceneNotifyType.Dictionary, 220417.ToString(), 1);
            player.Proxy.NotifyCountdown((ulong)DateTime.Now.AddSeconds(5.0f).ToBinary(),
                (int)eCountdownType.BattleFight);
        }
    }
}