#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataTable;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Scene
{
    //寒霜据点战场
    public class BattleScene210000 : UniformDungeon
    {
        //阵营数据
        public class Camp
        {
            public Dictionary<ulong, ObjPlayer> Players = new Dictionary<ulong, ObjPlayer>();
            public int Score;
            public ulong UUID = ulong.MaxValue;
        }

        //据点数据
        public class Strongpoint
        {
            public int OwnerSide = -1;
            public int FightingSide = -1;
            public float OccupyTimer;
            public float ScoreTimer;
            public eStrongpointState State = eStrongpointState.Idle;
        }

        #region 数据

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        //阵营数量
        public const int CampCount = 2;
        public const int CampBegin = 4;
        //据点数量
        public const int StrongpointCount = 3;
        //占据据点的时间
        public const float OccupyTime = 5f;
        //占据据点获得的分数
        public const int OccupyScoreAdd = 20;
        //每10秒加一次分
        public const int ScoreAddDuration = 10;
        //每次加2分
        public const int ScoreAdd = 2;
        //多少分赢得比赛
        public const int VictoryScore = 150;
        //一半积分
        public const int VictoryScoreHalf = VictoryScore/2;
        //多少分发即将胜利的公告
        public const int WillVictoryScore = VictoryScore - 20;
        //据点被占领的通知id
        private static readonly int[] OccupyTipId = {224003, 224004, 224005};
        //据点旗帜的id
        private static readonly int[][] FlagIds =
        {
            new[] {520103, 520100},
            new[] {520104, 520101},
            new[] {520105, 520102}
        };

        private BattleState mBattleState;
        private readonly List<ulong> mQuitPlayers = new List<ulong>();
        private readonly Dictionary<ulong, Trigger> mLeaveTrigger = new Dictionary<ulong, Trigger>();
        private readonly Dictionary<ulong, Trigger> mReliveTrigger = new Dictionary<ulong, Trigger>();

        //阵营数据
        private readonly Camp[] Camps = new Camp[CampCount];

        //据点数据
        private readonly Strongpoint[] Strongpoints = new Strongpoint[StrongpointCount];

        //
        private Trigger mTipTrigger;

        #endregion

        #region 重载函数

        public override void OnCreate()
        {
            base.OnCreate();

            for (var i = 0; i < StrongpointCount; i++)
            {
                Strongpoints[i] = new Strongpoint();
            }
            for (var i = 0; i < CampCount; i++)
            {
                Camps[i] = new Camp();
            }

            var startTime = DateTime.Now.AddSeconds(20.0f);
            StartTimer(eDungeonTimerType.WaitStart, startTime, TimeOverStart);
            StartTimer(eDungeonTimerType.WaitEnd, startTime.AddMinutes(mFubenRecord.TimeLimitMinutes), TimeOverEnd);
            mTipTrigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(15f), OnTriggerStartTip);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var o in mReliveTrigger)
            {
                SceneServerControl.Timer.DeleteTrigger(o.Value);
            }
            mReliveTrigger.Clear();
        }

        public override void StartDungeon()
        {
            base.StartDungeon();
            mTipTrigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(2f), OnTriggerGoodLuckTip);
            RemoveObj(999);

            PushActionToAllPlayer(p =>
            {
                //"战斗开始！"
                p.Proxy.NotifyMessage((int) eSceneNotifyType.Dictionary, 224001.ToString(), 1);
            });
        }

        public override void EndDungeon()
        {
            if (mBattleState == BattleState.None)
            {
                mBattleState = BattleState.Dogfall;
                ResultOver();
            }

            base.EndDungeon();
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);

            if (State != eDungeonState.Start)
            {
                return;
            }

            for (var i = 0; i < Strongpoints.Length; i++)
            {
                var strongpoint = Strongpoints[i];

                if (strongpoint.OwnerSide != -1)
                {
//占领中，持续加分
                    strongpoint.ScoreTimer += delta;
                    var camp = Camps[strongpoint.OwnerSide];
                    if (strongpoint.ScoreTimer >= ScoreAddDuration)
                    {
                        strongpoint.ScoreTimer -= ScoreAddDuration;
                        AddCampScore(camp, ScoreAdd, strongpoint.OwnerSide);
                    }
                }

                switch (strongpoint.State)
                {
                    case eStrongpointState.Occupying:
                    {
                        strongpoint.OccupyTimer += delta;
                        if (strongpoint.OccupyTimer >= OccupyTime)
                        {
                            strongpoint.OccupyTimer = OccupyTime;
                            SetStrongpointSide(i);
                        }
                    }
                        break;
                }
            }
        }

        public override void OnObjBeforeEnterScene(ObjBase obj)
        {
            var player = obj as ObjPlayer;
            if (player == null)
            {
                return;
            }

            var campId = player.mDbData.P1vP1CharacterId;
            var flag = -1;
            for (var i = 0; i < CampCount; i++)
            {
                var c = Camps[i];
                if (c.UUID == ulong.MaxValue)
                {
                    flag = i;
                    c.UUID = campId;
                    break;
                }
                if (c.UUID == campId)
                {
                    flag = i;
                    break;
                }
            }

            if (flag == -1)
            {
                Logger.Error("OnPlayerEnter Error CharacterId = {0}, CampId = {1}", player.ObjId, campId);
                return;
            }

            //设置阵营
            var newCamp = CampBegin + flag;
            player.SetCamp(newCamp);

            var camp = Camps[flag];
            var teamPlayers = camp.Players;
            if (teamPlayers.Keys.Contains(player.ObjId))
            {
                teamPlayers[player.ObjId] = player;
            }
            else
            {
                teamPlayers.Add(player.ObjId, player);
                FixPostion(player);

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

            var position = player.GetPosition();
            var pos = Utility.MakeVectorMultiplyPrecision(position.X, position.Y);
            player.Proxy.NotifyCampChange(newCamp, pos);
        }

        public override void OnPlayerEnter(ObjPlayer player)
        {
            PlayerLog.WriteLog(player.ObjId, "----HSJDKBattle------OnPlayerEnter----------");

            if (mLeaveTrigger.ContainsKey(player.ObjId))
            {
                var trigger = mLeaveTrigger[player.ObjId];
                SceneServerControl.Timer.DeleteTrigger(trigger);
                mLeaveTrigger.Remove(player.ObjId);
            }
            else
            {
                CoroutineFactory.NewCoroutine(PlayerEnter, player).MoveNext();
            }

            base.OnPlayerEnter(player);
        }
        public override void AfterPlayerEnterOver(ObjPlayer player)
        {
            player.AddBuff(3010, 1, player);
        }
        public override void OnPlayerLeave(ObjPlayer player)
        {
            base.OnPlayerLeave(player);
            PlayerLog.WriteLog(player.ObjId, "----HSJDKBattle------OnPlayerLeave----------");

            if (mBattleState == BattleState.None)
            {
                if (mQuitPlayers.Contains(player.ObjId))
                {
                    mQuitPlayers.Remove(player.ObjId);
                    CoroutineFactory.NewCoroutine(PlayerLeave, player).MoveNext();
                }
                else
                {
                    var leave = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(60.0f), () =>
                    {
                        CoroutineFactory.NewCoroutine(PlayerLeave, player).MoveNext();
                        mLeaveTrigger.Remove(player.ObjId);
                    });
                    mLeaveTrigger.Add(player.ObjId, leave);
                }
            }
            else
            {
                ResetPlayer(player);
            }
        }

        public override void ExitDungeon(ObjPlayer player)
        {
            mQuitPlayers.Add(player.ObjId);

            CoroutineFactory.NewCoroutine(player.ExitDungeon).MoveNext();
        }

        public override void OnPlayerDie(ObjPlayer player, ulong characterId = 0)
        {
            var side = GetPlayerSide(player);
            if (side == -1)
            {
                Logger.Error("OnPlayerDie GetPlayerSide  ObjOd{0}", player.ObjId);
                return;
            }
            PlayerLog.WriteLog(player.ObjId, "----HSJDKBattle------PlayerDie----------KillId={0}", characterId);
            //死人了，重置一下trigger area的状态
            var playerPos = player.GetPosition();
            var area = mAreaDict.Values.FirstOrDefault(a => a.IsInArea(playerPos));
            if (area != null)
            {
                OnTriggerAreaPlayersChanged(area);
            }

            var reliveObj = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(10.0f), () =>
            {
                if (mLeaveTrigger.ContainsKey(player.ObjId)) return;
                mReliveTrigger.Remove(player.ObjId);
                FixPostion(player);
                player.Relive();
                PlayerLog.WriteLog(player.ObjId, "----HSJDKBattle------PlayerRelive----------Time Over");
            });
            mReliveTrigger.Add(player.ObjId, reliveObj);

            var caster = FindCharacter(characterId);
            if (caster == null)
            {
                return;
            }
            caster = caster.GetRewardOwner();
            var strColor = "";
            if (side == 0)
            {
                strColor = "FF0000";//红色
            }
            else if (side == 1)
            {
                strColor = "2866E1";//蓝色
            }
            //
            var args = new List<string>();
            args.Add(strColor);
            args.Add(caster.GetName());
            var content = Utils.WrapDictionaryId(224008, args);
            
            player.Proxy.NotifyCountdown((ulong) DateTime.Now.AddSeconds(10f).ToBinary(),
                (int) eCountdownType.BattleRelive);

            player.Proxy.NotifyBattleReminder(14, content, 1);
            args.Clear();
            args.Add(caster.GetName());//此处 A 击杀了 B   player是被击杀者
            args.Add(player.GetName());
            string text = side == 1 ? Utils.WrapDictionaryId(61000, args) : Utils.WrapDictionaryId(61001, args);

            PushActionToAllPlayer(p =>
            {
                p.Proxy.NotifyBattleReminder(14, text, 1);
            });

        }

        public override void OnCharacterEnterArea(int areaId, ObjCharacter character)
        {
            OnTriggerAreaPlayersChanged(AreaDict[areaId]);
        }

        public override void OnCharacterLeaveArea(int areaId, ObjCharacter character)
        {
            OnTriggerAreaPlayersChanged(AreaDict[areaId]);
        }
        public override void OnPlayerEnterOver(ObjPlayer player)
        {
            if (mReliveTrigger.ContainsKey(player.ObjId))
            {
                var trigger = mReliveTrigger[player.ObjId];
                mReliveTrigger.Remove(player.ObjId);
                SceneServerControl.Timer.DeleteTrigger(trigger);
                if (DateTime.Now > trigger.Time)
                {
                    FixPostion(player);
                    player.Relive();


                    PlayerLog.WriteLog(player.ObjId, "----HLKBattle------PlayerRelive----------Time Over");
                }
                else
                {
                    var reliveObj = SceneServerControl.Timer.CreateTrigger(trigger.Time, () =>
                    {

                        if (mLeaveTrigger.ContainsKey(player.ObjId)) return;
                        FixPostion(player);
                        player.Relive();
                        mReliveTrigger.Remove(player.ObjId);

                        PlayerLog.WriteLog(player.ObjId, "----HLKBattle------PlayerRelive----------Time Over");
                    });
                    mReliveTrigger.Add(player.ObjId, reliveObj);
                    //通知死者，复活倒计时
                    player.Proxy.NotifyCountdown((ulong)trigger.Time.ToBinary(), (int)eCountdownType.BattleRelive);
                }
            }
        }
        #endregion

        #region 私有方法

        private void GetCampNameAndColor(int side, ref string strColor, ref string name)
        {
            if (side == 0)
            {
                strColor = "2866E1";
                name = Utils.AddDictionaryId(224300);
            }
            else
            {
                strColor = "FF0000";
                name = Utils.AddDictionaryId(224301);
            }
        }

        private void OnTriggerStartTip()
        {
            mTipTrigger = null;

            PushActionToAllPlayer(p =>
            {
                //"战斗将在5秒后开始！"
                p.Proxy.NotifyMessage((int) eSceneNotifyType.Dictionary, 224000.ToString(), 1);
                p.Proxy.NotifyCountdown((ulong) DateTime.Now.AddSeconds(5.0f).ToBinary(),
                    (int) eCountdownType.BattleFight);
            });
        }

        private void OnTriggerGoodLuckTip()
        {
            mTipTrigger = null;

            PushActionToAllPlayer(p =>
            {
                //"占领据点，获取积分，赢得战斗胜利！祝你好运！"
                p.Proxy.NotifyMessage((int) eSceneNotifyType.Dictionary, 224002.ToString(), 1);
            });
        }

        public void BroadcastSceneSide(int side, Action<ObjPlayer> action)
        {
            var player = Camps[side].Players;
            if (player != null)
            {
                foreach (var objPlayer in player)
                {
                    if (null == objPlayer.Value || null == objPlayer.Value.Proxy)
                        continue;
                    action(objPlayer.Value);
                }
            }
        }

        private IEnumerator CharacterResultOver(Coroutine coroutine, ObjPlayer player, int state)
        {
            if (player.Proxy != null)
            {
                if (state == 1)
                {
                    player.Proxy.NotifyBattleReminder(14, Utils.WrapDictionaryId(224200), 1);
                }
                else
                {
                    player.Proxy.NotifyBattleReminder(14, Utils.WrapDictionaryId(224201), 1);
                }
            }
            var msg = SceneServer.Instance.LogicAgent.SSBattleResult(player.ObjId, mFubenRecord.Id, state);
            yield return msg.SendAndWaitUntilDone(coroutine);
        }

        private void ResultOver()
        {
            CoroutineFactory.NewCoroutine(ResultOverCoroutine).MoveNext();
        }

        private IEnumerator ResultOverCoroutine(Coroutine co)
        {
            EnterAutoClose(10);

            if (mTipTrigger != null)
            {
                SceneServerControl.Timer.DeleteTrigger(mTipTrigger);
                mTipTrigger = null;
            }

            if (mBattleState == BattleState.WinA)
            {
                BroadcastSceneSide(0,
                    player => { CoroutineFactory.NewCoroutine(CharacterResultOver, player, 1).MoveNext(); });
                BroadcastSceneSide(1,
                    player => { CoroutineFactory.NewCoroutine(CharacterResultOver, player, 0).MoveNext(); });
            }
            else if (mBattleState == BattleState.WinB)
            {
                BroadcastSceneSide(0,
                    player => { CoroutineFactory.NewCoroutine(CharacterResultOver, player, 0).MoveNext(); });
                BroadcastSceneSide(1,
                    player => { CoroutineFactory.NewCoroutine(CharacterResultOver, player, 1).MoveNext(); });
            }
            else if (mBattleState == BattleState.Dogfall)
            {
                PushActionToAllPlayer(
                    player => { CoroutineFactory.NewCoroutine(CharacterResultOver, player, 0).MoveNext(); });
            }

            ObjPlayer onePlayer = null;
            foreach (var camp in Camps)
            {
                foreach (var player in camp.Players.Values)
                {
                    onePlayer = player;
                    break;
                }
                if (onePlayer != null)
                {
                    break;
                }
            }
            if (onePlayer != null)
            {
                PlayerLog.WriteLog(onePlayer.ObjId, "----HSJDKBattle------SSBattleEnd----------");
                var msg = SceneServer.Instance.TeamAgent.SSBattleEnd(onePlayer.ObjId, Guid);
                yield return msg.SendAndWaitUntilDone(co);
            }
            else
            {
                Logger.Error("send SSBattleEnd error onePlayer == null");
            }
        }

        private IEnumerator PlayerEnter(Coroutine coroutine, ObjPlayer player)
        {
            var msg = SceneServer.Instance.TeamAgent.SSCharacterEnterBattle(player.ObjId, mFubenRecord.Id, Guid,
                player.ObjId);
            yield return msg.SendAndWaitUntilDone(coroutine);
        }

        private IEnumerator PlayerLeave(Coroutine coroutine, ObjPlayer player)
        {
            var side = GetPlayerSide(player);

            ResetPlayer(player);

            if (side == -1)
            {
                Logger.Error("OnPlayerLeave GetPlayerSide  ObjOd{0}", player.ObjId);
                yield break;
            }
            var playerList = Camps[side].Players;
            playerList.Remove(player.ObjId);
            if (mReliveTrigger.ContainsKey(player.ObjId))
            {
                mReliveTrigger.Remove(player.ObjId);
                player.Proxy.NotifyCountdown((ulong)DateTime.Now.ToBinary(), (int)eCountdownType.BattleRelive);//离开就不倒计时了

            }
            if (playerList.Count == 0 && mBattleState == BattleState.None)
            {
                if (side == 0)
                {
                    mBattleState = BattleState.WinB;
                }
                else
                {
                    mBattleState = BattleState.WinA;
                }
                ResultOver();
            }

            var msg1 = SceneServer.Instance.TeamAgent.SSCharacterLeaveBattle(player.ObjId, mFubenRecord.Id, Guid,
                player.ObjId);
            yield return msg1.SendAndWaitUntilDone(coroutine);

            var msg2 = SceneServer.Instance.LogicAgent.SSBattleResult(player.ObjId, mFubenRecord.Id, 0);
            yield return msg2.SendAndWaitUntilDone(coroutine);
        }

        private void ResetPlayer(ObjPlayer player)
        {
            player.SetCamp(0);
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

        private int GetPlayerSide(ObjPlayer player)
        {
            var side = player.GetCamp() - CampBegin;
            return side >= 0 ? side : -1;
        }

        private void FixPostion(ObjPlayer player)
        {
            var side = GetPlayerSide(player);
            if (side == 1)
            {
                player.SetPosition((float) TableSceneData.Entry_x, (float) TableSceneData.Entry_z);
            }
            else
            {
                player.SetPosition((float) TableSceneData.PVPPosX, (float) TableSceneData.PVPPosZ);
            }
            PlayerLog.WriteLog(player.ObjId, "----HSJDKBattle------FixPostion----------{0}", player.GetPosition());
        }

        private void OnTriggerAreaPlayersChanged(TriggerArea area)
        {
            var tbTriggerArea = area.TableTriggerArea;
            if (tbTriggerArea.AreaType != (int) eTriggerAreaType.Strongpoint)
            {
                return;
            }

            int[] sides = {0, 0};
            var characters = area.mCharacterList;
            foreach (var character in characters)
            {
                var player = character as ObjPlayer;
                if (player != null && !player.IsDead())
                {
                    var s = GetPlayerSide(player);
                    if (s >= 0 && s <= 1)
                    {
                        sides[s] = 1;
                    }
                    else
                    {
                        Logger.Error("side = {0}, error! player id = {1}, name = {2}", s, player.ObjId, player.GetName());
                    }
                }
            }

            var idx = tbTriggerArea.Param[0];
            var strongpoint = Strongpoints[idx];
            var count = sides.Count(i => i == 1);

            if (count != 1)
            {
                switch (strongpoint.State)
                {
                    case eStrongpointState.Idle:
                    case eStrongpointState.Occupied:
                        break;
                    case eStrongpointState.Occupying:
                        if (count == 0)
                        {
                            strongpoint.FightingSide = -1;
                            strongpoint.OccupyTimer = 0;
                            SetStrongpointState(idx, eStrongpointState.Idle);
                        }
                        else
                        {
                            SetStrongpointState(idx, eStrongpointState.OccupyWait);
                        }
                        break;
                    case eStrongpointState.OccupyWait:
                        if (count == 0)
                        {
                            strongpoint.FightingSide = -1;
                            strongpoint.OccupyTimer = 0;
                            SetStrongpointState(idx, eStrongpointState.Idle);
                        }
                        break;
                }
                return;
            }

            //如果只有一波人在据点内，则继续检查
            var side = sides[0] == 1 ? 0 : 1;
            if (strongpoint.OwnerSide == side)
            {
                if (strongpoint.FightingSide != -1)
                {
                    strongpoint.FightingSide = -1;
                    strongpoint.OccupyTimer = 0;
                    SetStrongpointState(idx, eStrongpointState.Idle);
                }
                return;
            }
            if (strongpoint.FightingSide == side && strongpoint.State == eStrongpointState.Occupying)
            {
                return;
            }

            if (strongpoint.FightingSide != side)
            {
                strongpoint.FightingSide = side;
                strongpoint.OccupyTimer = 0;
            }
            SetStrongpointState(idx, eStrongpointState.Occupying);
        }

        private void NotifyMessageWraped(int dicId, params string[] strs)
        {
            var tip = Utils.WrapDictionaryId(dicId, strs.ToList());
            PushActionToAllPlayer(p => { p.Proxy.NotifyMessage((int) eSceneNotifyType.DictionaryWrap, tip, 1); });
        }

        private void AddCampScore(Camp camp, int addScore, int side)
        {
            var oldScore = camp.Score;
            camp.Score += addScore;
            if (oldScore < VictoryScoreHalf && camp.Score >= VictoryScoreHalf)
            {
                var strColor = String.Empty;
                var name = string.Empty;
                GetCampNameAndColor(side, ref strColor, ref name);
                NotifyMessageWraped(224006, strColor, name);
            }
            else if (oldScore < WillVictoryScore && camp.Score >= WillVictoryScore)
            {
                var strColor = String.Empty;
                var name = string.Empty;
                GetCampNameAndColor(side, ref strColor, ref name);
                NotifyMessageWraped(224007, strColor, name);
            }
            else if (camp.Score >= VictoryScore)
            {
                camp.Score = VictoryScore;
                if (side == 0)
                {
                    mBattleState = BattleState.WinA;
                }
                else
                {
                    mBattleState = BattleState.WinB;
                }
                ResultOver();
            }
            mIsFubenInfoDirty = true;
            var unit = mFubenInfoMsg.Units[side];
            unit.Params[0] = camp.Score;
        }

        private void SetStrongpointState(int index, eStrongpointState state)
        {
            var point = Strongpoints[index];
            point.State = state;
            //如果是占领中则归属设置为正在占领方  否则设置成当前占领方

            var camp = state == eStrongpointState.Occupying ? point.FightingSide : point.OwnerSide;
            camp += CampBegin;
            PushActionToAllPlayer(
                p => { p.Proxy.NotifyStrongpointStateChanged(camp, index, (int) state, point.OccupyTimer); });
        }

        private void SetStrongpointSide(int index)
        {
            var point = Strongpoints[index];
            point.ScoreTimer = 0;

            var side = point.FightingSide;
            if (side < 0 || side > 1)
            {
                Logger.Error("In SetStrongpointSide(), side = {0}", side);
                return;
            }

            //加20分
            point.OwnerSide = side;
            AddCampScore(Camps[side], OccupyScoreAdd, side);

            //state
            SetStrongpointState(index, eStrongpointState.Occupied);

            //发通知
            var strColor = String.Empty;
            var name = string.Empty;
            GetCampNameAndColor(side, ref strColor, ref name);
            NotifyMessageWraped(OccupyTipId[index], strColor, name, name);

            //改旗帜
            var flags = FlagIds[index];
            var oldFlagSceneNpcId = flags[1 - side];
            var tbSceneNpc = Table.GetSceneNpc(oldFlagSceneNpcId);
            RemoveObj(tbSceneNpc.DataID);
            CreateSceneNpc(flags[side]);

            //通知客户端据点易主了
            mIsFubenInfoDirty = true;
            var unit = mFubenInfoMsg.Units[2];
            unit.Params[index] = side;
        }

        #endregion
    }
}