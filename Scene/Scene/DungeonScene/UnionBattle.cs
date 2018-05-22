// 盟战
#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataContract;
using DataTable;
using Scorpion;
using Mono.GameMath;
using NLog;
using Shared;

#endregion

namespace Scene
{
    public class UnionBattle : UniformDungeon
    {
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region 数据结构
        // 场景npc配置
        public class NpcIds
        {
            public int AltarId = 99001; // 祭坛
        }

        // npc
        public class BattleNpc
        {
            public ObjNPC AltarNpc; // 祭坛npc
        }

 
        public class Camp
        {
            public int AllianceId = -1;
            public string AllianceName;
            public Vector2 AppearPos;
            public List<ObjPlayer> Players = new List<ObjPlayer>();
            public Vector2 RelivePos;
            public int SpCount;
            public int Step;
            public Trigger Timer;
        }

        #endregion

        #region 常量

        public const int GuardTypeId1 = 99001;
        public const int GuardTypeId2 = 99009;
        public const int GuardTypeId3 = 99010;
        public const int GuardTypeId4 = 99011;
        public const int Tower1TypeId = 99002;
        public const int Tower2TypeId = 99003;

        public const int GuardSceneNpcId0 = 21005;
        public const int TowerSceneNpcId0 = 21009;

        public const int DefenderCampId = 7;
        public const int Offensiver1CampId = 8;
        public const int Offensiver2CampId = 9;

        //占据据点的时间
        public const float OccupyTime = 5f;

        public static NpcIds RedNpcIds = new NpcIds();
        public static NpcIds BlueNpcIds = new NpcIds();

        #endregion

        #region 旗子相关数据

        static UnionBattle()
        {
            RedNpcIds.AltarId = 99001;
            BlueNpcIds.AltarId = 99009;
        }

        #endregion

        #region 数据

        //守城方的公会id
        private readonly List<int> AllianceIds = new List<int>();

        //
        private readonly Dictionary<int, Camp> Camps = new Dictionary<int, Camp>
        {
            {DefenderCampId, new Camp {SpCount = 5, Step = 1}},
            {Offensiver1CampId, new Camp()},
            {Offensiver2CampId, new Camp()}
        };

        //buff id => 层数
        private readonly Dictionary<int, int> DefenderBuffs = new Dictionary<int, int>();

        //每个玩家的复活时间
        private readonly Dictionary<ulong, int> PlayerReliveTime = new Dictionary<ulong, int>();

        public readonly BattleNpc RedBattleNpc = new BattleNpc();
        public readonly BattleNpc BlueBattleNpc = new BattleNpc();

        //四个守卫
        private readonly List<ObjNPC> Guards = new List<ObjNPC>();

        //守卫是死是活的状态
        private readonly Int32Array GuardState = new Int32Array();

        //动态难度的npc的level
        private int NpcLevel;

        private DateTime startTime = DateTime.Now.AddMinutes(10);
        #endregion

        #region 重写父类方法

        public override void OnCreate()
        {
            base.OnCreate();

            var param = Param.Param;
            if (param.Count < 5)
            {
                Logger.Error("In OnCreate(). Param count < 5");
                return;
            }

            //设置副本开启和结束时间
            var hour = param[0];
            var min = param[1];
            var now = DateTime.Now;
            startTime =
                new DateTime(now.Year, now.Month, now.Day, hour, min, 0, DateTimeKind.Local).AddMinutes(
                    mFubenRecord.OpenLastMinutes);
            StartTimer(eDungeonTimerType.WaitStart, startTime, TimeOverStart);
            var extraTime = startTime.AddMinutes(mFubenRecord.TimeLimitMinutes);
            var endTime = extraTime.AddMinutes(0);
            StartTimer(eDungeonTimerType.WaitEnd, endTime, TimeOverEnd);

            //for (var i = 0; i < 3; i++)
            //{
            //    var allianceId = param[i + 2];
            //    AllianceIds.Add(allianceId);
            //    var c = Camps[DefenderCampId + i];
            //    c.AllianceId = allianceId;
            //    if (allianceId != -1)
            //    {
            //        GetAllianceName(c);
            //    }
            //}

            CreateDynamicNpc();
        }

        private void CreateDynamicNpc()
        {
            CoroutineFactory.NewCoroutine(CreateDynamicNpcCoroutine).MoveNext();
        }

        private IEnumerator CreateDynamicNpcCoroutine(Coroutine co)
        {
            var msg = SceneServer.Instance.RankAgent.GetRankValue(0, ServerId, (int) RankType.Level, 50);
            yield return msg.SendAndWaitUntilDone(co);

            if (msg.State != MessageState.Reply)
            {
                Logger.Error("In CreateDynamicNpcCoroutine(). GetRankValue Failed with state = {0}", msg.State);
            }

            NpcLevel = (int) (msg.Response/Constants.RankLevelFactor);
            NpcLevel = Math.Max(NpcLevel, 100);
            NpcLevel = Math.Min(NpcLevel, AttrBaseManager.LevelMax);
            NpcLevel = (NpcLevel + 9)/10*10;

            CreateNpc(RedBattleNpc, RedNpcIds);
            CreateNpc(BlueBattleNpc, BlueNpcIds);
        }

        private void CreateNpc(BattleNpc npc, NpcIds npcIds)
        {
            npc.AltarNpc = CreateSceneNpc(npcIds.AltarId);
        }

        public override void StartDungeon()
        {
            //移除阻挡
            RemoveObj(999);

            State = eDungeonState.Start;
            NotifyDungeonTime(eDungeonTimerType.WaitExtraTimeStart);

            mFubenInfoMsg.Units[3].Params[0] = (int) eAllianceWarState.Fight;
            mIsFubenInfoDirty = true;

            var content = Utils.WrapDictionaryId(41019);
            PushActionToAllPlayer(p =>
            {
                p.Proxy.NotifyBattleReminder(14, content, 1);
            });
        }

        public override void EndDungeon()
        {
            BattleOver(DefenderCampId);
            base.EndDungeon();
        }

        public override void OnObjBeforeEnterScene(ObjBase obj)
        {
            var type = obj.GetObjType();
            if (type != ObjType.PLAYER)
            {
                return;
            }

            var player = obj as ObjPlayer;
	        if (null == player)
	        {
		        return;
	        }

            //设置camp
            var allianceId = player.GetAllianceId();
            var idx = AllianceIds.IndexOf(allianceId);
            var campId = DefenderCampId + idx;
	        if (!Camps.ContainsKey(campId))
	        {
				//潜规则camp = -1，表示要把他踢掉，人已满
				player.SetCamp(-1);
				Logger.Fatal("OnObjBeforeEnterScene  Name[{0}] CampId=[{1}]", player.GetName(),campId);
				return;
	        }
	        
			var camp = Camps[campId];
			if (camp.Players.Count >= Constants.AllianceMaxPlayer)
			{
				//潜规则camp = -1，表示要把他踢掉，人已满
				player.SetCamp(-1);
				return;
			}    
	        
            
            player.SetCamp(campId);
            player.SetPosition(camp.AppearPos);
            camp.Players.Add(player);

            var position = player.GetPosition();
            var pos = Utility.MakeVectorMultiplyPrecision(position.X, position.Y);
            player.Proxy.NotifyCampChange(campId, pos);

            //恢复满血满蓝
            if (player.IsDead())
            {
                player.Relive();
            }
            else
            {
                player.Attr.SetDataValue(eAttributeType.HpNow, player.Attr.GetDataValue(eAttributeType.HpMax));
                player.Attr.SetDataValue(eAttributeType.MpNow, player.Attr.GetDataValue(eAttributeType.MpMax));
            }

            if (!PlayerReliveTime.ContainsKey(player.ObjId))
            {
                PlayerReliveTime.Add(player.ObjId, StaticVariable.ReliveTimeInit);
            }
        }

        public override void OnNpcEnter(ObjNPC npc)
        {
            base.OnNpcEnter(npc);

            var buffId = -1;
            var typeId = npc.TypeId;
            switch (typeId)
            {
                case GuardTypeId1:
                case GuardTypeId2:
                case GuardTypeId3:
                case GuardTypeId4:
                    buffId = 1306;
                    break;
                case Tower1TypeId:
                case Tower2TypeId:
                    buffId = 1307;
                    break;
                default:
                    return;
            }
            if (AllianceIds.Count == 0 || AllianceIds[2] != -1)
            {
//这会还没有玩家进入游戏，所以不用刷新buff
                DefenderBuffs.modifyValue(buffId, 1);
                RefreshDefenderBuff(buffId, true);
            }
        }

        public override void OnPlayerEnter(ObjPlayer player)
        {
            base.OnPlayerEnter(player);

            NotifyAllianceWarNpcData(new List<ulong> {player.ObjId});

            CoroutineFactory.NewCoroutine(PlayerEnter, player).MoveNext();
        }

        public override void OnPlayerEnterOver(ObjPlayer player)
        {
            base.OnPlayerEnterOver(player);

            var delta = startTime - DateTime.Now;
            if (delta.TotalSeconds > 0)
            {
                player.Proxy.NotifyCommonCountdown((int)delta.TotalSeconds);
            }
        }

        public override void OnPlayerLeave(ObjPlayer player)
        {
            base.OnPlayerLeave(player);

            //恢复满血满蓝
            if (player.IsDead())
            {
                player.Relive();
            }
            else
            {
                player.Attr.SetDataValue(eAttributeType.HpNow, player.Attr.GetDataValue(eAttributeType.HpMax));
                player.Attr.SetDataValue(eAttributeType.MpNow, player.Attr.GetDataValue(eAttributeType.MpMax));
            }

            var campId = player.GetCamp();
            if (campId == -1)
            {
                player.SetCamp(0);
                return;
            }
            var camp = Camps[campId];
            camp.Players.Remove(player);
            player.SetCamp(0);

            CoroutineFactory.NewCoroutine(PlayerLeave, player).MoveNext();
        }

        public override void ExitDungeon(ObjPlayer player)
        {
            player.ExitDungeon();
        }

        public override void OnPlayerDie(ObjPlayer player, ulong characterId = 0)
        {
            //死人了，重置一下trigger area的状态
            var playerPos = player.GetPosition();
            var area = mAreaDict.Values.FirstOrDefault(a => a.IsInArea(playerPos));
            if (area != null)
            {
                OnTriggerAreaPlayersChanged(area);
            }

            //重生
            var waitSec = PlayerReliveTime[player.ObjId];
            if (waitSec < StaticVariable.ReliveTimeMax)
            {
                PlayerReliveTime[player.ObjId] = Math.Min(waitSec + StaticVariable.ReliveTimeAdd,
                    StaticVariable.ReliveTimeMax);
            }
            var reliveTime = DateTime.Now.AddSeconds(waitSec);
            CreateTimer(reliveTime, () => { PlayerRelive(player); });

            //通知死者，复活倒计时
            player.Proxy.NotifyBattleReminder(14,
                Utils.WrapDictionaryId(220516, new List<string> {waitSec.ToString()}), 1);
            player.Proxy.NotifyCountdown((ulong) reliveTime.ToBinary(), (int) eCountdownType.BattleRelive);

            //找出击杀者
            var caster = FindCharacter(characterId);
            if (caster == null)
            {
                return;
            }
            caster = caster.GetRewardOwner();
            var casterPlayer = caster as ObjPlayer;
            if (casterPlayer == null)
            {
                return;
            }
            //加公会贡献
            Utility.GetItem(casterPlayer.ObjId, (int) eResourcesType.Contribution, StaticVariable.PlayerContribution);
        }

        public override void OnNpcDie(ObjNPC npc, ulong characterId = 0)
        {
            base.OnNpcDie(npc, characterId);

            int buffId;
            int dictId;
            var campId = -1;
            int contribution;
            var typeId = npc.TypeId;
            switch (typeId)
            {
                case GuardTypeId1:
                case GuardTypeId2:
                case GuardTypeId3:
                case GuardTypeId4:
                {
                    buffId = 1306;
                    dictId = 300929;
                    contribution = StaticVariable.GuardContribution;

                    //
                    var idx = Guards.IndexOf(npc);
                    if (idx == -1)
                    {
                        Logger.Error("In OnNpcDie(). idx == -1");
                        break;
                    }
                    GuardState.Items[idx] = 1;
                    NotifyAllianceWarNpcDataToAllPlayer();
                }
                    break;
                case Tower1TypeId:
                {
                    buffId = 1307;
                    dictId = 300931;
                    contribution = StaticVariable.TowerContribution;

                    var tbPos = Table.GetRandomCoordinate(403);
                    campId = Offensiver1CampId;
                    var camp = Camps[campId];
                    camp.RelivePos = new Vector2(tbPos.PosX, tbPos.PosY);
                    camp.Step = 1;
                }
                    break;
                case Tower2TypeId:
                {
                    buffId = 1307;
                    dictId = 300931;
                    contribution = StaticVariable.TowerContribution;

                    var tbPos = Table.GetRandomCoordinate(406);
                    campId = Offensiver2CampId;
                    var camp = Camps[campId];
                    camp.RelivePos = new Vector2(tbPos.PosX, tbPos.PosY);
                    camp.Step = 1;
                }
                    break;
                default:
                    return;
            }

            //刷新buff
            if (AllianceIds[2] != -1)
            {
//如果只有攻城公会只有一个，就不加buff
                DefenderBuffs.modifyValue(buffId, -1);
                RefreshDefenderBuff(buffId, false);
            }

            //找出击杀者
            var caster = FindCharacter(characterId);
            if (caster == null)
            {
                return;
            }
            caster = caster.GetRewardOwner();
            var casterPlayer = caster as ObjPlayer;
            if (casterPlayer == null)
            {
                return;
            }

            //加贡献
            var players = Camps[casterPlayer.GetCamp()].Players;
            foreach (var player in players)
            {
                Utility.GetItem(player.ObjId, (int) eResourcesType.Contribution, contribution);
            }

            //发通告
            var args = new List<string>();
            args.Add(casterPlayer.AllianceName);
            args.Add(caster.GetName());
            if (dictId == 300931)
            {
                if (Camps[campId].AllianceName != null)
                {
                    args.Add(Camps[campId].AllianceName);
                }
                else
                {
                    dictId = 300941;
                }
            }
            var content = Utils.WrapDictionaryId(dictId, args);
            PushActionToAllPlayer(p => { p.Proxy.NotifyBattleReminder(14, content, 1); });
        }

        public override void OnCharacterEnterArea(int areaId, ObjCharacter character)
        {
            if (character.GetObjType() != ObjType.PLAYER)
            {
                return;
            }
            OnTriggerAreaPlayersChanged(AreaDict[areaId]);
        }

        public override void OnCharacterLeaveArea(int areaId, ObjCharacter character)
        {
            if (character.GetObjType() != ObjType.PLAYER)
            {
                return;
            }
            OnTriggerAreaPlayersChanged(AreaDict[areaId]);
        }

        #endregion

        #region 重置守卫

        //复活守卫的次数
        private int GuardReliveCount;

        public IEnumerator RespawnGuard(Coroutine co, ObjPlayer player, int index, AsyncReturnValue<ErrorCodes> err)
        {
            if (player.GetCamp() != DefenderCampId)
            {
                err.Value = ErrorCodes.Error_JurisdictionNotEnough;
                yield break;
            }
            if (GuardReliveCount >= StaticVariable.AllianceWarGuardRespawnMaxCount)
            {
                err.Value = ErrorCodes.Error_GuardRespawnExceed;
                yield break;
            }
            var guard = Guards[index];
            if (!guard.IsDead())
            {
                NotifyAllianceWarNpcData(new List<ulong> {player.ObjId});
                err.Value = ErrorCodes.Error_GuardNotDie;
                yield break;
            }
            //扣钱
            var cost = Table.GetSkillUpgrading(73000).GetSkillUpgradingValue(GuardReliveCount);
            var msg = SceneServer.Instance.LogicAgent.DeleteItem(player.ObjId, (int)eResourcesType.DiamondRes, cost, (int)eDeleteItemType.FuHuoShouWei);
            yield return msg.SendAndWaitUntilDone(co);
            if (msg.State != MessageState.Reply)
            {
                err.Value = ErrorCodes.Unknow;
                yield break;
            }
            var errCode = (ErrorCodes) msg.ErrorCode;
            if (errCode != ErrorCodes.OK)
            {
                err.Value = errCode;
                yield break;
            }
            err.Value = ErrorCodes.OK;
            var npc = CreateSceneNpc(index + GuardSceneNpcId0, Vector2.Zero, NpcLevel);
            Guards[index] = npc;
            ++GuardReliveCount;
            GuardState.Items[index] = 0;
            NotifyAllianceWarNpcDataToAllPlayer();

            //
            var idx = MapNpcRecords.FindIndex(r => r.NpcID == npc.TypeId);
            if (idx != -1)
            {
                var info = MapNpcInfo.Data[idx];
                info.Alive = true;
            }

            //发通告
            var args = new List<string>();
            args.Add(player.GetName());
            var content = Utils.WrapDictionaryId(300930, args);
            PushActionToAllPlayer(p => { p.Proxy.NotifyBattleReminder(14, content, 1); });
        }

        #endregion

        #region 内部逻辑

        private void GetAllianceName(Camp camp)
        {
            CoroutineFactory.NewCoroutine(GetAllianceNameCoroutine, camp).MoveNext();
        }

        private IEnumerator GetAllianceNameCoroutine(Coroutine co, Camp camp)
        {
            for (var i = 0; i < 3; i++)
            {
                var msg = SceneServer.Instance.TeamAgent.SSGetAllianceName(0, camp.AllianceId);
                yield return msg.SendAndWaitUntilDone(co);
                if (msg.State == MessageState.Reply && msg.ErrorCode == (int) ErrorCodes.OK)
                {
                    camp.AllianceName = msg.Response;
                    yield break;
                }
                yield return SceneServer.Instance.ServerControl.Wait(co, TimeSpan.FromMilliseconds(100));
            }
            Logger.Fatal("GetAllianceNameCoroutine failed for alliance id = {0}", camp.AllianceId);
        }

        private void OnTriggerAreaPlayersChanged(TriggerArea area)
        {
            if (State != eDungeonState.Start && State != eDungeonState.ExtraTime)
            {
                return;
            }

            var tbTriggerArea = area.TableTriggerArea;
            if (tbTriggerArea.AreaType != (int) eTriggerAreaType.Strongpoint)
            {
                return;
            }

            var camps = new Dictionary<int, int>();
            var characters = area.mCharacterList;
            var tip = Utils.WrapDictionaryId(300940);
            foreach (var character in characters)
            {
                var player = character as ObjPlayer;
                if (player != null && !player.IsDead())
                {
                    var campId = player.GetCamp();
                    if (Camps[campId].Step != 1)
                    {
                        //打掉防御塔后获得据点争夺权
                        player.Proxy.NotifyBattleReminder(14, tip, 1);
                        continue;
                    }
                    if (campId >= DefenderCampId && campId <= Offensiver2CampId)
                    {
                        camps[campId] = 1;
                    }
                    else
                    {
                        Logger.Error("side = {0}, error! player id = {1}, name = {2}", campId, player.ObjId,
                            player.GetName());
                    }
                }
            }
        }

        //刷新守城方的buff
        private void RefreshDefenderBuff(int buffId, bool isAdd)
        {
            var players = Camps[DefenderCampId].Players;
            if (isAdd)
            {
                foreach (var player in players)
                {
                    player.AddBuff(buffId, 1, player);
                }
            }
            else
            {
                foreach (var player in players)
                {
                    var buffs = player.BuffList.GetBuffById(buffId);
                    if (buffs.Count > 0)
                    {
                        var buff = buffs[0];
                        var layer = buff.GetLayer();
                        if (layer > 1)
                        {
                            buff.SetLayer(layer - 1);
                            var replyMsg = new BuffResultMsg();
                            var buffResult = new BuffResult
                            {
                                SkillObjId = buff.mCasterId,
                                TargetObjId = player.ObjId,
                                BuffTypeId = buffId,
                                BuffId = buff.mId,
                                Type = BuffType.HT_EFFECT,
                                ViewTime = Extension.AddTimeDiffToNet(0)
                            };
                            if (buff.mBuff.IsView == 1)
                            {
                                buffResult.Param.Add(buff.GetLastSeconds());
                                buffResult.Param.Add(buff.GetLayer());
                                buffResult.Param.Add(buff.m_nLevel);
                            }
                            replyMsg.buff.Add(buffResult);
                            player.BroadcastBuffList(replyMsg);
                            GetBuff.DoEffect(this, player, buff, 0);
                        }
                        else
                        {
                            MissBuff.DoEffect(this, player, buff);
                            player.DeleteBuff(buff, eCleanBuffType.Clear);
                        }
                    }
                    else
                    {
                        Logger.Error("In RefreshDefenderBuff(). DeleteBuff error! buffs.Count == 0! player name = {0}",
                            player.GetName());
                    }
                }
            }
        }

        //战斗结束
        private void BattleOver(int campId)
        {
            //关闭所有的timer
            foreach (var c in Camps)
            {
                DeleteTimer(c.Value.Timer);
            }

            var winCamp = Camps[campId];
            var result = new FubenResult();
            result.Args.Add(winCamp.AllianceId);
            PushActionToAllPlayer(p =>
            {
                result.CompleteType = (int) (p.GetCamp() == campId
                    ? eDungeonCompleteType.Success
                    : eDungeonCompleteType.Failed);
                Complete(p.ObjId, result);
            });

            EnterAutoClose();

            CoroutineFactory.NewCoroutine(NotifyResultToTeamServer, winCamp.AllianceId).MoveNext();
        }

        private IEnumerator NotifyResultToTeamServer(Coroutine co, int winAlliance)
        {
            var result = new AllianceWarResult();
            result.ServerId = ServerId;
            result.Winner = winAlliance;
            var teamAgent = SceneServer.Instance.TeamAgent;
            for (var i = 0; i < 5; i++)
            {
                var msg = teamAgent.NotifyAllianceWarResult(0, result);
                yield return msg.SendAndWaitUntilDone(co);
                if (msg.State != MessageState.Reply)
                {
                    PlayerLog.WriteLog((ulong) LogType.AllianceWarError, "NotifyAllianceWarResult not reply!");
                }
                else if (msg.ErrorCode != (int) ErrorCodes.OK)
                {
                    PlayerLog.WriteLog((ulong) LogType.AllianceWarError, "NotifyAllianceWarResult err = {0}!",
                        msg.ErrorCode);
                }
                else
                {
                    yield break;
                }
            }
            PlayerLog.WriteLog((ulong) LogType.AllianceWarError, "Fatal error!!!! NotifyAllianceWarResult Failed!!!");
        }

        private void PlayerRelive(ObjPlayer player)
        {
            var campId = player.GetCamp();
            player.SetPosition(Camps[campId].RelivePos);
            player.Relive();
        }

        private IEnumerator PlayerEnter(Coroutine coroutine, ObjPlayer player)
        {
            var msg = SceneServer.Instance.TeamAgent.SSCharacterEnterBattle(player.ObjId, mFubenRecord.Id, Guid,
                player.ObjId);
            yield return msg.SendAndWaitUntilDone(coroutine);
        }

        private IEnumerator PlayerLeave(Coroutine coroutine, ObjPlayer player)
        {
            var msg1 = SceneServer.Instance.TeamAgent.SSCharacterLeaveBattle(player.ObjId, mFubenRecord.Id, Guid,
                player.ObjId);
            yield return msg1.SendAndWaitUntilDone(coroutine);
        }

        private void NotifyAllianceWarNpcDataToAllPlayer()
        {
            var players = EnumAllPlayer();
            NotifyAllianceWarNpcData(players.Select(p => p.ObjId));
        }

        //向客户端通知守卫的状态
        private void NotifyAllianceWarNpcData(IEnumerable<ulong> playerIds)
        {
            SceneServer.Instance.SceneAgent.NotifyAllianceWarNpcData(playerIds, GuardReliveCount, GuardState);
        }

        //通知场景里所有玩家大家的位置信息
        private void ScenePlayerInfos()
        {
            var players = EnumAllPlayer();
            var info = new ScenePlayerInfos();
            foreach (var player in players)
            {
                var pos = player.GetPosition();
                info.Data.Add(new ScenePlayerInfo
                {
                    Id = player.ObjId,
                    Pos = Utility.MakeVectorMultiplyPrecision(pos.X, pos.Y),
                    Camp = player.GetCamp()
                });
            }
            //players.First().Proxy.NotifyScenePlayerInfos(players.Select(p => p.ObjId), info);
            SceneServer.Instance.SceneAgent.NotifyScenePlayerInfos(players.Select(p => p.ObjId), info);
        }

        #endregion
    }
}