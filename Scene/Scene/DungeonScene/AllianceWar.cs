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
    public class AllianceWar : UniformDungeon
    {
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region 数据结构

        public class StrongPoint
        {
            public StrongPoint(int idx)
            {
                Index = idx;
            }

            public int Index;
            public int OccupiedCamp = DefenderCampId;
            public int OccupyingCamp = -1;
            public eStrongpointState State = eStrongpointState.Occupied;
            public Trigger Timer;
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

        public const int ExtraTimeMin = 10;

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

        #endregion

        #region 旗子相关数据

        //三方的旗子SceneNpcId
        public static readonly int[][] FlagIds =
        {
            new[] {21011, 21014, 21017, 21020, 21023},
            new[] {21012, 21015, 21018, 21021, 21024},
            new[] {21013, 21016, 21019, 21022, 21025}
        };

        public ObjNPC[] Flags = new ObjNPC[5];

        #endregion

        #region 数据

        private readonly StrongPoint[] StrongPoints = new StrongPoint[5];

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

        //是否是加时
        private bool IsExtraTime;

        //两个塔
        private readonly List<ObjNPC> Towers = new List<ObjNPC>();

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

            var hour = param[0];
            var min = param[1];
            for (var i = 0; i < 3; i++)
            {
                var allianceId = param[i + 2];
                AllianceIds.Add(allianceId);
                var c = Camps[DefenderCampId + i];
                c.AllianceId = allianceId;
                if (allianceId != -1)
                {
                    GetAllianceName(c);
                }
            }

            //初始化守城方的旗子
            var ids = FlagIds[0];
            for (int i = 0, imax = Flags.Length; i < imax; ++i)
            {
                Flags[i] = CreateSceneNpc(ids[i]);
            }

            //设置副本开启和结束时间
            var now = DateTime.Now;
            startTime =
                new DateTime(now.Year, now.Month, now.Day, hour, min, 0, DateTimeKind.Local).AddMinutes(
                    mFubenRecord.OpenLastMinutes);
            StartTimer(eDungeonTimerType.WaitStart, startTime, TimeOverStart);
            var extraTime = startTime.AddMinutes(mFubenRecord.TimeLimitMinutes - ExtraTimeMin);
            StartTimer(eDungeonTimerType.WaitExtraTimeStart, extraTime, EnterExtraTime);
            var endTime = extraTime.AddMinutes(ExtraTimeMin);
            StartTimer(eDungeonTimerType.WaitEnd, endTime, TimeOverEnd);

            //副本提示 正式开始前，攻城战每1分钟提示一次
            for (var i = mFubenRecord.OpenLastMinutes; i > 0; --i)
            {
                var time = startTime.AddMinutes(-i);
                if (now <= time)
                {
                    var content = Utils.WrapDictionaryId(300922, new List<string> { i.ToString() });
                    CreateTimer(time,
                        () =>
                        {
                            PushActionToAllPlayer(player =>
                            {
                                player.Proxy.NotifyBattleReminder(14, content, 1);
                            });
                        });
                }
            }

            //副本提示 距离结束提示
            int[] timeNoticeList = { 10, 5, 1 }; // 距离副本结束多长时间提示 (300928)
            for (var i = 0; i < timeNoticeList.Length; ++i)
            {
                var timeNotice = timeNoticeList[i];
                var time = extraTime.AddMinutes(-timeNotice);
                if (time < startTime)
                    continue;

                var dictId = 300927;
                if (i == timeNoticeList.Length) // 最后一次提示特殊处理
                    dictId = 300928;
                var content = Utils.WrapDictionaryId(dictId, new List<string> { timeNotice.ToString() });
                CreateTimer(time,
                    () => { PushActionToAllPlayer(player => { player.Proxy.NotifyBattleReminder(14, content, 1); }); });
            }

            //副本提示 加时距离结束提示
            int[] extraTimeNoticeList = { 5, 3, 1 }; // 距离副本结束多长时间提示 (300935)
            for (var i = 0; i < extraTimeNoticeList.Length; ++i)
            {
                var extraTimeNotice = extraTimeNoticeList[i];
                var time = endTime.AddMinutes(-extraTimeNotice);
                if (time < extraTime)
                    continue;
                var dictId = 300934;
                if (i == extraTimeNoticeList.Length) // 最后一次提示特殊处理
                    dictId = 300935;
                var content = Utils.WrapDictionaryId(dictId, new List<string> { extraTimeNotice.ToString() });
                CreateTimer(time,
                    () => { PushActionToAllPlayer(player => { player.Proxy.NotifyBattleReminder(14, content, 1); }); });
            }

            //初始化StrongPoints
            for (int i = 0, imax = 5; i < imax; i++)
            {
                StrongPoints[i] = new StrongPoint(i);
            }

            //初始化出生点
            var camp = Camps[DefenderCampId];
            camp.AppearPos = new Vector2((float) TableSceneData.Entry_x, (float) TableSceneData.Entry_z);
            camp.RelivePos = new Vector2((float) TableSceneData.Safe_x, (float) TableSceneData.Safe_z);

            camp = Camps[Offensiver1CampId];
            var tbPos = Table.GetRandomCoordinate(401);
            camp.AppearPos = new Vector2(tbPos.PosX, tbPos.PosY);
            tbPos = Table.GetRandomCoordinate(402);
            camp.RelivePos = new Vector2(tbPos.PosX, tbPos.PosY);

            camp = Camps[Offensiver2CampId];
            tbPos = Table.GetRandomCoordinate(404);
            camp.AppearPos = new Vector2(tbPos.PosX, tbPos.PosY);
            tbPos = Table.GetRandomCoordinate(405);
            camp.RelivePos = new Vector2(tbPos.PosX, tbPos.PosY);

            //初始化副本信息
            mFubenInfoMsg.Units[0].Params[0] = AllianceIds[0];
            mFubenInfoMsg.Units[0].Params[1] = 5;
            mFubenInfoMsg.Units[1].Params[0] = AllianceIds[1];
            mFubenInfoMsg.Units[2].Params[0] = AllianceIds[2];
            mFubenInfoMsg.Units[3].Params[0] = (int) eAllianceWarState.WaitStart;
            mIsFubenInfoDirty = true;

            //每3秒广播一次位置
            CreateTimer(now.AddSeconds(3), ScenePlayerInfos, 3000);

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

            //创建塔
            for (var i = 0; i < 2; i++)
            {
                var tower = CreateSceneNpc(i + TowerSceneNpcId0, Vector2.Zero, NpcLevel);
                Towers.Add(tower);
            }

            //创建守卫
            for (var i = 0; i < 4; i++)
            {
                var guard = CreateSceneNpc(i + GuardSceneNpcId0, Vector2.Zero, NpcLevel);
                Guards.Add(guard);
                GuardState.Items.Add(0);
            }
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
                foreach (var sp in StrongPoints)
                {
                    p.Proxy.NotifyStrongpointStateChanged(sp.OccupiedCamp, sp.Index, (int) sp.State, 0);
                }
            });
        }

        public void EnterExtraTime()
        {
            State = eDungeonState.ExtraTime;
            //通知客户端更新倒计时显示
            NotifyDungeonTime(eDungeonTimerType.WaitEnd);

            for (var i = DefenderCampId; i <= Offensiver2CampId; i++)
            {
                var count = StrongPoints.Count(sp => sp.OccupiedCamp == i);
                if (count >= 3)
                {
                    BattleOver(i);
                    return;
                }
            }

            //进入加时赛
            IsExtraTime = true;
            var unit = mFubenInfoMsg.Units[3];
            unit.Params[0] = (int) eAllianceWarState.ExtraTime;
            var dueTime = DateTime.Now.AddMinutes(mFubenRecord.TimeLimitMinutes - 30);
            unit.Params[1] = dueTime.Hour*10000 + dueTime.Minute*100 + dueTime.Second;
            mIsFubenInfoDirty = true;

            var content = Utils.WrapDictionaryId(41020);
            PushActionToAllPlayer(p => { p.Proxy.NotifyBattleReminder(14, content, 1); });
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

            if (player.GetCamp() == -1)
            {
                return;
            }

            NotifyAllianceWarNpcData(new List<ulong> {player.ObjId});

            CoroutineFactory.NewCoroutine(PlayerEnter, player).MoveNext();
        }

        public override void OnPlayerEnterOver(ObjPlayer player)
        {
            base.OnPlayerEnterOver(player);

            if (player.GetCamp() == -1)
            {
                KickPlayer(player.ObjId);
                return;
            }

            //如果是守城方，加buff
            if (player.GetCamp() == DefenderCampId)
            {
                foreach (var buffId in DefenderBuffs)
                {
                    for (int i = 0, imax = buffId.Value; i < imax; i++)
                    {
                        player.AddBuff(buffId.Key, 1, player);
                    }
                }
            }

//             if (State == eDungeonState.WillStart)
//             {
//                 var t = GetTriggerTime(eDungeonTimerType.WaitStart);
//                 var param = (int) (t - DateTime.Now).TotalSeconds;
//                 param = param << 1;
//                 param += 1;
//                 player.Proxy.NotifyBattleReminder(19, Utils.WrapDictionaryId(41015), param);
//             }

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

        private void StartTimer(StrongPoint sp, DateTime target, Action act)
        {
            CloseTimer(sp);
            sp.Timer = CreateTimer(target, act);
        }

        private void CloseTimer(StrongPoint sp)
        {
            if (sp.Timer != null)
            {
                DeleteTimer(sp.Timer);
                sp.Timer = null;
            }
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

            var now = DateTime.Now;
            var idx = tbTriggerArea.Param[0];
            var sp = StrongPoints[idx];
            var count = camps.Count;

            switch (sp.State)
            {
                case eStrongpointState.Idle:
                    if (count == 1)
                    {
                        var camp = camps.First(c => c.Value == 1).Key;
                        if (sp.OccupiedCamp == camp)
                        {
                            return;
                        }
                        sp.OccupyingCamp = camp;
                        StartTimer(sp, now.AddSeconds(OccupyTime), () =>
                        {
                            SetStrongpointState(sp, eStrongpointState.Occupied, camp);
                            SetStrongpointCamp(sp, sp.OccupyingCamp);
                            sp.OccupyingCamp = -1;
                        });
                        SetStrongpointState(sp, eStrongpointState.Occupying, camp);
                    }
                    break;
                case eStrongpointState.Occupied:
                    if (count == 1)
                    {
                        var camp = camps.First(c => c.Value == 1).Key;
                        if (sp.OccupiedCamp == camp)
                        {
                            return;
                        }
                        sp.OccupyingCamp = camp;
                        StartTimer(sp, now.AddSeconds(OccupyTime), () =>
                        {
                            SetStrongpointState(sp, eStrongpointState.Occupied, camp);
                            SetStrongpointCamp(sp, sp.OccupyingCamp);
                            sp.OccupyingCamp = -1;
                        });
                        SetStrongpointState(sp, eStrongpointState.Occupying, camp);
                    }
                    else if (count == 2)
                    {
                        if (sp.OccupiedCamp != -1 && camps.ContainsKey(sp.OccupiedCamp))
                        {
                            return;
                        }
                        StartTimer(sp, now.AddSeconds(OccupyTime), () =>
                        {
                            SetStrongpointState(sp, eStrongpointState.Idle, sp.OccupiedCamp);
                            SetStrongpointCamp(sp, -1);
                        });
                        SetStrongpointState(sp, eStrongpointState.Contending, sp.OccupiedCamp);
                    }
                    break;
                case eStrongpointState.Occupying:
                    if (count == 0 || count == 3)
                    {
                        sp.OccupyingCamp = -1;
                        CloseTimer(sp);
                        SetStrongpointState(sp,
                            sp.OccupiedCamp == -1 ? eStrongpointState.Idle : eStrongpointState.Occupied, sp.OccupiedCamp);
                    }
                    else if (count == 2)
                    {
                        if (sp.OccupiedCamp != -1)
                        {
                            if (camps.ContainsKey(sp.OccupiedCamp))
                            {
                                sp.OccupyingCamp = -1;
                                CloseTimer(sp);
                                SetStrongpointState(sp, eStrongpointState.Occupied, sp.OccupiedCamp);
                            }
                            else
                            {
                                sp.OccupyingCamp = -1;
                                StartTimer(sp, now.AddSeconds(OccupyTime), () =>
                                {
                                    SetStrongpointState(sp, eStrongpointState.Idle, -1);
                                    SetStrongpointCamp(sp, -1);
                                });
                                SetStrongpointState(sp, eStrongpointState.Contending, sp.OccupiedCamp);
                            }
                        }
                        else
                        {
                            CloseTimer(sp);
                            SetStrongpointState(sp, eStrongpointState.Idle, sp.OccupiedCamp);
                        }
                    }
                    break;
                case eStrongpointState.Contending:
                    if (count == 0 || count == 3)
                    {
                        CloseTimer(sp);
                        SetStrongpointState(sp,
                            sp.OccupiedCamp == -1 ? eStrongpointState.Idle : eStrongpointState.Occupied, sp.OccupiedCamp);
                    }
                    else if (count == 1)
                    {
                        var camp = camps.First(c => c.Value == 1).Key;
                        if (camp == sp.OccupiedCamp)
                        {
                            CloseTimer(sp);
                            SetStrongpointState(sp, eStrongpointState.Occupied, camp);
                        }
                        else
                        {
                            sp.OccupyingCamp = camp;
                            StartTimer(sp, now.AddSeconds(OccupyTime), () =>
                            {
                                SetStrongpointState(sp, eStrongpointState.Occupied, camp);
                                SetStrongpointCamp(sp, sp.OccupyingCamp);
                                sp.OccupyingCamp = -1;
                            });
                            SetStrongpointState(sp, eStrongpointState.Occupying, camp);
                        }
                    }
                    else if (count == 2)
                    {
                    }
                    break;
            }
        }

        private void SetStrongpointCamp(StrongPoint sp, int campId)
        {
            if (sp.OccupiedCamp == campId)
            {
                return;
            }

            var oldCampId = sp.OccupiedCamp;
            sp.OccupiedCamp = campId;
            if (oldCampId != -1)
            {
                var camp = Camps[oldCampId];
                var count = --camp.SpCount;
                if (count == 2)
                {
//不足3个据点
                    if (camp.Timer != null)
                    {
                        DeleteTimer(camp.Timer);
                        camp.Timer = null;
                    }
                }
                //删除旗子
                var flag = Flags[sp.Index];
                Flags[sp.Index] = null;
                LeaveScene(flag);
                //修改副本信息
                mIsFubenInfoDirty = true;
                var infoIdx = oldCampId - DefenderCampId;
                var unit = mFubenInfoMsg.Units[infoIdx];
                unit.Params[1] = count;
            }
            if (campId != -1)
            {
                var infoIdx = campId - DefenderCampId;
                var unit = mFubenInfoMsg.Units[infoIdx];

                var camp = Camps[campId];
                var count = ++camp.SpCount;
                if (count >= 3)
                {
                    if (count == 3)
                    {
//达到三个据点
                        if (IsExtraTime)
                        {
                            BattleOver(campId);
                        }
                        else if (campId != DefenderCampId)
                        {
                            var dueTime = DateTime.Now.AddSeconds(StaticVariable.VictoryTime);
                            camp.Timer = CreateTimer(dueTime, () => { BattleOver(campId); });
                            unit.Params[2] = dueTime.Hour*10000 + dueTime.Minute*100 + dueTime.Second;
                        }
                    }

                    if (!IsExtraTime && campId != DefenderCampId)
                    {
                        var sec = 0;
                        if (camp.Timer != null && camp.Timer.T != null)
                        {
                            sec = (int) (camp.Timer.Time - DateTime.Now).TotalSeconds;
                        }
                        var args = new List<string>();
                        args.Add(camp.AllianceName);
                        args.Add(count.ToString());
                        args.Add(sec.ToString());
                        var content = Utils.WrapDictionaryId(300936, args);
                        PushActionToAllPlayer(p => { p.Proxy.NotifyBattleReminder(14, content, 1); });
                    }
                }
                //创建旗子
                Flags[sp.Index] = CreateSceneNpc(FlagIds[campId - DefenderCampId][sp.Index]);
                //修改副本信息
                mFubenInfoMsg.Units[campId - DefenderCampId].Params[1] = count;
                mIsFubenInfoDirty = true;
            }

            //发据点变化提示
            {
                var dictId = -1;
                var args = new List<string>();
                if (oldCampId == -1 || Camps[oldCampId].AllianceName == null)
                {
                    if (campId != -1)
                    {
                        dictId = 300937;
                        args.Add(Camps[campId].AllianceName);
                    }
                }
                else
                {
                    if (campId == -1)
                    {
                        dictId = 300939;
                        for (var i = DefenderCampId; i <= Offensiver2CampId; i++)
                        {
                            if (i != oldCampId)
                            {
                                args.Add(Camps[i].AllianceName);
                            }
                        }
                    }
                    else
                    {
                        dictId = 300938;
                        args.Add(Camps[campId].AllianceName);
                        args.Add(Camps[oldCampId].AllianceName);
                    }
                }
                if (dictId != -1)
                {
                    var content = Utils.WrapDictionaryId(dictId, args);
                    PushActionToAllPlayer(p => { p.Proxy.NotifyBattleReminder(14, content, 1); });
                }
            }
        }

        private void SetStrongpointState(StrongPoint sp, eStrongpointState state, int camp)
        {
            sp.State = state;
            var now = DateTime.Now;
            var time = sp.Timer == null ? 0f : (OccupyTime - (float) (sp.Timer.Time - now).TotalSeconds);
            PushActionToAllPlayer(p => { p.Proxy.NotifyStrongpointStateChanged(camp, sp.Index, (int) state, time); });
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
            foreach (var sp in StrongPoints)
            {
                DeleteTimer(sp.Timer);
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