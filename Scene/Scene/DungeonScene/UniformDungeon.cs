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
    //统一单人副本
    public class UniformSingleDungeon : UniformDungeon
    {
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public override void ExitDungeon(ObjPlayer player)
        {
//玩家强退，1s之后关闭副本
            Log(Logger, "ExitDungeon:player id = {0}, name = {1}", player.ObjId, player.GetName());

            EnterAutoClose(30);
        }

        private void GetDungeonTotleCount(ObjPlayer player)
        {
            CoroutineFactory.NewCoroutine(GetDungeonTotleCountCoroutine, player).MoveNext();
        }

        private IEnumerator GetDungeonTotleCountCoroutine(Coroutine co, ObjPlayer player)
        {
            var ids = new Int32Array();
            ids.Items.Add(mFubenRecord.TotleExdata);
            var msg = SceneServer.Instance.LogicAgent.SSFetchExdata(player.ObjId, ids);
            yield return msg.SendAndWaitUntilDone(co);
            if (msg.State != MessageState.Reply)
            {
                Logger.Error("SSFetchExdata return with state = {0}", msg.State);
                yield break;
            }
            if (msg.ErrorCode != (int) ErrorCodes.OK)
            {
                Logger.Error("SSFetchExdata return with err = {0}", msg.ErrorCode);
                yield break;
            }
            if (msg.Response.Items[0] == 0)
            {
                SpecialDrop = player.TypeId;
            }
        }

        public override void OnCreate()
        {
            Log(Logger, "OnCreate");

            base.OnCreate();
            State = eDungeonState.Start;
            var waitMin = mFubenRecord == null ? 10 : mFubenRecord.TimeLimitMinutes;
            StartTimer(eDungeonTimerType.WaitEnd, DateTime.Now.AddMinutes(waitMin), TimeOverEnd);
        }

        public override void OnPlayerEnter(ObjPlayer player)
        {
            base.OnPlayerEnter(player);

            GetDungeonTotleCount(player);
        }
    }

    //统一多人副本
    public class UniformTeamDungeon : UniformDungeon
    {
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<ulong, Trigger> mLeaveTrigger = new Dictionary<ulong, Trigger>();

        public override void CompleteToAll(FubenResult result, int seconds = 20)
        {
            DealWithQuitPlayers(result);

            base.CompleteToAll(result, seconds);
        }

        private void DealWithQuitPlayers(FubenResult result)
        {
            foreach (var o in mLeaveTrigger)
            {
                SceneServerControl.Timer.DeleteTrigger(o.Value);

                Complete(o.Key, result);

                var player = FindPlayer(o.Key);
                if (player != null)
                {
                    CoroutineFactory.NewCoroutine(PlayerLeave, player).MoveNext();
                }
            }
            mLeaveTrigger.Clear();
        }

        public override void EndDungeon()
        {
            Log(Logger, "EndDungeon");

            base.EndDungeon();

            var result = new FubenResult();
            result.CompleteType = (int) eDungeonCompleteType.Failed;
            CompleteToAll(result);
            EnterAutoClose(0);
        }

        public override void OnCreate()
        {
            Log(Logger, "OnCreate");

            base.OnCreate();
            State = eDungeonState.Start;
            var waitMin = mFubenRecord == null ? 10 : mFubenRecord.TimeLimitMinutes;
            StartTimer(eDungeonTimerType.WaitEnd, DateTime.Now.AddMinutes(waitMin), TimeOverEnd);
        }

        public override void OnPlayerEnter(ObjPlayer player)
        {
            Log(Logger, "OnPlayerEnter:player id = {0}, name = {1}", player.ObjId, player.GetName());

            base.OnPlayerEnter(player);

            if (mLeaveTrigger.ContainsKey(player.ObjId))
            {
                var trigger = mLeaveTrigger[player.ObjId];
                SceneServerControl.Timer.DeleteTrigger(trigger);
                mLeaveTrigger.Remove(player.ObjId);
            }
        }

        public override void OnPlayerLeave(ObjPlayer player)
        {
            Log(Logger, "OnPlayerLeave:player id = {0}, name = {1}, State = {2}", player.ObjId, player.GetName(), State);

            if (State >= eDungeonState.WillClose)
            {
                CoroutineFactory.NewCoroutine(PlayerLeave, player).MoveNext();
            }
            else
            {
                var leave = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddMinutes(1), () =>
                {
                    CoroutineFactory.NewCoroutine(PlayerLeave, player).MoveNext();
                    mLeaveTrigger.Remove(player.ObjId);
                });
                mLeaveTrigger.Add(player.ObjId, leave);
            }
            base.OnPlayerLeave(player);
        }

        private IEnumerator PlayerLeave(Coroutine coroutine, ObjPlayer player)
        {
            var msg = SceneServer.Instance.TeamAgent.SSCharacterLeaveBattle(player.ObjId, mFubenRecord.Id, Guid,
                player.ObjId);
            yield return msg.SendAndWaitUntilDone(coroutine);
        }
    }

    //统一活动副本
    public class UniformMultyDungeon : UniformDungeon
    {
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        protected Dictionary<ulong, ObjPlayer> mDropPlayers = new Dictionary<ulong, ObjPlayer>();
        protected Dictionary<ulong, int> mPlayers = new Dictionary<ulong, int>();
        protected Dictionary<ulong, ObjPlayer> mQuitPlayers = new Dictionary<ulong, ObjPlayer>();

        public override IEnumerator CloseDungeon(Coroutine co)
        {
            Log(Logger, "CloseDungeon");

            var playerIds = new Uint64Array();
            playerIds.Items.AddRange(mPlayers.Keys);
            if (this.dicGetRewardPlayers.ContainsKey(playerIds.Items[0]) == false)
            {
                var msg = SceneServer.Instance.LogicAgent.NotifyDungeonClose(playerIds.Items[0], TypeId, playerIds);
                yield return msg.SendAndWaitUntilDone(co);
            }
            var co1 = CoroutineFactory.NewSubroutine(base.CloseDungeon, co);
            if (co1.MoveNext())
            {
                yield return co1;
            }
        }

        public override void CompleteToAll(FubenResult result, int seconds = 20)
        {
            DealWithQuitPlayers();
            base.CompleteToAll(result, seconds);
        }

        private void DealWithQuitPlayers()
        {
            Log(Logger, "DealWithQuitPlayers");

            var result = new FubenResult();
            result.CompleteType = (int) eDungeonCompleteType.Quit;
            foreach (var player in mDropPlayers)
            {
                Complete(player.Value.ObjId, result);
            }
            mDropPlayers.Clear();
            mQuitPlayers.Clear();
        }

        public override void EndDungeon()
        {
            Log(Logger, "EndDungeon");

            var result = new FubenResult();
            result.CompleteType = (int) eDungeonCompleteType.Failed;
            CompleteToAll(result);
        }

        public override void ExitDungeon(ObjPlayer player)
        {
//强退的玩家，直接副本结束
            Log(Logger, "ExitDungeon:State = {0}", State);

            if (State <= eDungeonState.Start)
            {
	            if (mQuitPlayers.ContainsKey(player.ObjId))
	            {
					Logger.Error("ExitDungeon Error mQuitPlayers.ContainsKey({0})", player.ObjId);
		            return;
	            }
                mQuitPlayers.Add(player.ObjId, player);

                var result = new FubenResult();
                result.CompleteType = (int) eDungeonCompleteType.Quit;
                Complete(player.ObjId, result);

                if (PlayerCount == 1 && mDropPlayers.Count == 0)
                {
//没人掉线，且强退副本，30s后关闭副本
                    EnterAutoClose(30);
                }
            }
        }

        public override void OnCreate()
        {
            Log(Logger, "OnCreate");

            base.OnCreate();

            if (Param.Param.Count > 1)
            {
                State = eDungeonState.WillStart;

                //设置副本开启结束时间
                var now = DateTime.Now;
                var hour = Param.Param[0];
                var min = Param.Param[1];
                var startTime = new DateTime(now.Year, now.Month, now.Day, hour, min, 0, DateTimeKind.Local);
                StartTimer(eDungeonTimerType.WaitStart, startTime, TimeOverStart);
                StartTimer(eDungeonTimerType.WaitEnd, startTime.AddMinutes(mFubenRecord.TimeLimitMinutes), TimeOverEnd);
            }
            else
            {
                State = eDungeonState.Start;
                StartTimer(eDungeonTimerType.WaitEnd, DateTime.Now.AddMinutes(mFubenRecord.TimeLimitMinutes),
                    TimeOverEnd);
            }
        }

        public override void OnPlayerEnter(ObjPlayer player)
        {
            Log(Logger, "OnPlayerEnter:player id = {0}, name = {1}", player.ObjId, player.GetName());

            base.OnPlayerEnter(player);
            mPlayers.modifyValue(player.ObjId, 0);
            if (State == eDungeonState.Start)
            {
                mDropPlayers.Remove(player.ObjId);
            }
        }

        public override void OnPlayerLeave(ObjPlayer player)
        {
            Log(Logger, "OnPlayerLeave:player id = {0}, name = {1}, State = {2}", player.ObjId, player.GetName(), State);

            if (State == eDungeonState.Start)
            {
                if (!mQuitPlayers.ContainsKey(player.ObjId))
                {
                    mDropPlayers.Add(player.ObjId, player);
                }
            }
            base.OnPlayerLeave(player);
        }

        public override void StartDungeon()
        {
            Log(Logger, "StartDungeon");

            base.StartDungeon();
            DetectPhaseEnd();
        }
    }

    public abstract class UniformDungeon : DungeonScene
    {
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region 虚函数

        protected virtual void DoIntermittentCreateMonster(int id, int interval)
        {
            var tbSkillUpgrading = Table.GetSkillUpgrading(id);
            var ids = tbSkillUpgrading.Values;
            var idx = 0;
            StartTimer(eDungeonTimerType.CreateMonster, DateTime.Now, () =>
            {
                if (idx < ids.Count)
                {
                    CreateSceneNpc(ids[idx++]);
                }
                else
                {
                    CloseTimer(eDungeonTimerType.CreateMonster);
                }
            }, interval);
        }

        #endregion

        #region 常量

        public enum EnterPhaseOperateType
        {
            CreateMonster, //刷怪
            KillMonster, //杀怪
            OpenGate, //开门
            SwitchState, //切换阶段计时
            NotifyCountDown, //屏幕中间的倒计时
            NotifyBattleReminder, //屏幕公告
            RemoveBuff, //移除buff
            IntermittentCreateMonster, //间歇刷怪
            WaitTime, //阶段开始后计时(毫秒)
            NotifyStartWarning //开始预警
        }

        public enum EnterPhaseRequireType
        {
            KillMonster, //杀怪
            EnterRegion, //进入区域
            KillMonsterGroup, //杀怪组
            Wait, //此阶段已经过的时间(毫秒)
            BattleFieldBossHpPercent, //战场BOSS血量百分比
            AllMonsterDie //敌对阵营怪物死光
        }

        #endregion

        #region 数据

        private class EnterPhaseRequireStruct
        {
            public EnterPhaseRequireStruct(int p1, int p2, int infoIdx)
            {
                Param1 = p1;
                Param2 = p2;
                InfoIdx = infoIdx;
            }

            public int Counter;
            public readonly int InfoIdx;
            public int Param1;
            public readonly int Param2;

            public void AddCount(int add, FubenInfoMsg info)
            {
                Counter += add;
                if (InfoIdx >= 0 && InfoIdx < info.Units.Count)
                {
                    var unit = info.Units[InfoIdx];
                    unit.Params[0] += add;
                }
            }
        }

        private FubenLogicRecord mFubenLogicRecord;

        protected FubenLogicRecord FubenLogicRecord
        {
            get { return mFubenLogicRecord; }
            set
            {
                if (mFubenLogicRecord == value)
                {
                    return;
                }
                mFubenLogicRecord = value;
                if (mFubenLogicRecord != null)
                {
                    EnterPhase();
                }
            }
        }

        //EnterPhaseRequireType => param1 => EnterPhaseRequireStruct
        private readonly Dictionary<EnterPhaseRequireType, Dictionary<int, EnterPhaseRequireStruct>>
            mPhaseRequirementDic =
                new Dictionary<EnterPhaseRequireType, Dictionary<int, EnterPhaseRequireStruct>>();

        private readonly List<EnterPhaseRequireStruct> mPhaseRequirements = new List<EnterPhaseRequireStruct>();

        protected FubenInfoMsg mFubenInfoMsg;
        protected bool mIsFubenInfoDirty;

        #endregion

        #region 重写父类方法

        public override void OnCreate()
        {
            Log(Logger, "OnCreate");

            base.OnCreate();
            PrepareFubenLogicData();
        }

        public override void OnPlayerEnter(ObjPlayer player)
        {
            Log(Logger, "OnPlayerEnter:player id = {0}, name = {1}", player.ObjId, player.GetName());

            base.OnPlayerEnter(player);
            RefreshInfoPlayerCount();
            SendFubenInfo(player);
        }

        public override void OnPlayerLeave(ObjPlayer player)
        {
            Log(Logger, "OnPlayerLeave:player id = {0}, name = {1}", player.ObjId, player.GetName());

            base.OnPlayerLeave(player);
            RefreshInfoPlayerCount();
        }

        public override void OnCharacterEnterArea(int areaId, ObjCharacter character)
        {
            AccomplishPhaseRequire(EnterPhaseRequireType.EnterRegion, areaId);
        }

        public override void OnNpcDie(ObjNPC npc, ulong characterId = 0)
        {
            base.OnNpcDie(npc, characterId);
            AccomplishPhaseRequire(EnterPhaseRequireType.KillMonster, npc.TypeId);
        }

        public override bool LeaveScene(ObjBase obj, ReasonType reason = ReasonType.VisibilityChanged)
        {
            if (mPhaseRequirementDic.ContainsKey(EnterPhaseRequireType.AllMonsterDie) && obj.GetObjType() == ObjType.NPC &&
                mPlayerDict.Count > 0)
            {
                var p = mPlayerDict.First().Value;
                var camp = p.GetCamp();
                var tbCamp = Table.GetCamp(camp);
                var count = mObjDict.Values.Count(o =>
                {
                    var npc = o as ObjNPC;
                    return npc != null && tbCamp.Camp[npc.GetCamp()] == 1;
                });
                if (count == 1)
                {
                    AccomplishPhaseRequire(EnterPhaseRequireType.AllMonsterDie, 0);
                }
            }

            return base.LeaveScene(obj, reason);
        }

        #endregion

        #region 内部逻辑方法

        private void RefreshInfoPlayerCount()
        {
            if (mFubenInfoMsg == null)
            {
                PlayerLog.WriteLog((ulong) LogType.UniformDungeon,
                    "RefreshInfoPlayerCount() mFubenInfoMsg == null! Scene id = {0}", TypeId);
                return;
            }
            var units = mFubenInfoMsg.Units;
            var unit = units.Find(u => FubenLogicRecord.FubenInfo[u.Index] == (int) eFubenInfoType.PlayerCount);
            if (unit != null)
            {
                unit.Params[0] = PlayerCount;
                mIsFubenInfoDirty = true;
            }
        }

        protected void SendFubenInfo(ObjPlayer player)
        {
            if (mFubenInfoMsg == null)
            {
                return;
            }
            player.Proxy.NotifyFubenInfo(mFubenInfoMsg);
        }

        protected void SendFubenInfo()
        {
            if (!mIsFubenInfoDirty)
            {
                return;
            }
            mIsFubenInfoDirty = false;
            PushActionToAllPlayer(player => { player.Proxy.NotifyFubenInfo(mFubenInfoMsg); });
        }

        protected void PrepareFubenLogicData()
        {
            if (mFubenRecord.FubenLogicID == -1)
            {
                return;
            }

            mFubenInfoMsg = new FubenInfoMsg();
            mFubenInfoMsg.LogicId = -1;

            FubenLogicRecord = Table.GetFubenLogic(mFubenRecord.FubenLogicID);
            if (FubenLogicRecord == null)
            {
                return;
            }

            //刷新副本信息的定时器
            CreateTimer(DateTime.Now, SendFubenInfo, 1000);
        }

        protected void EnterNextPhase()
        {
            if (State >= eDungeonState.WillClose)
            {
                return;
            }

            if (FubenLogicRecord.EnterStateID == -1)
            {
                return;
            }

            if (FubenLogicRecord.DelayToNextState > 0)
            {
                CreateTimer(DateTime.Now.AddMilliseconds(FubenLogicRecord.DelayToNextState),
                    () => { FubenLogicRecord = Table.GetFubenLogic(FubenLogicRecord.EnterStateID); });
            }
            else
            {
                FubenLogicRecord = Table.GetFubenLogic(FubenLogicRecord.EnterStateID);
            }
        }

        private void EnterPhase()
        {
            Log(Logger, "EnterPhase:LogidId = {0}", mFubenLogicRecord.Id);

            //把该阶段的完成条件组织一下
            mPhaseRequirementDic.Clear();
            mPhaseRequirements.Clear();
            for (int i = 0, imax = FubenLogicRecord.SwitchState.Length; i < imax; ++i)
            {
                var type = (EnterPhaseRequireType) FubenLogicRecord.SwitchState[i];
                var param1 = FubenLogicRecord.SwitchParam1[i];
                var param2 = FubenLogicRecord.SwitchParam2[i];
                var infoIdx = FubenLogicRecord.SwitchInfoPa[i];
                if ((int) type == -1)
                {
                    break;
                }

                var realType = type == EnterPhaseRequireType.KillMonsterGroup ? EnterPhaseRequireType.KillMonster : type;
                Dictionary<int, EnterPhaseRequireStruct> requireDic;
                if (!mPhaseRequirementDic.TryGetValue(realType, out requireDic))
                {
                    requireDic = new Dictionary<int, EnterPhaseRequireStruct>();
                    mPhaseRequirementDic.Add(realType, requireDic);
                }
                var require = new EnterPhaseRequireStruct(param1, param2, infoIdx);
                mPhaseRequirements.Add(require);
                switch (type)
                {
                    case EnterPhaseRequireType.KillMonsterGroup:
                    {
                        var tbSkillUpgrading = Table.GetSkillUpgrading(param1);
                        foreach (var monsterId in tbSkillUpgrading.Values)
                        {
                            requireDic.Add(monsterId, require);
                        }
                    }
                        break;
                    default:
                    {
                        requireDic.Add(param1, require);
                    }
                        break;
                }
            }

            //执行进入该阶段的操作
            for (int i = 0, imax = FubenLogicRecord.EnterStage.Length; i < imax; ++i)
            {
                var type = FubenLogicRecord.EnterStage[i];
                var param1 = FubenLogicRecord.EnterParam1[i];
                var param2 = FubenLogicRecord.EnterParam2[i];
                if (type == -1)
                {
                    break;
                }
                switch ((EnterPhaseOperateType) type)
                {
                    case EnterPhaseOperateType.CreateMonster:
                    {
                        var tbSkillUpgrading = Table.GetSkillUpgrading(param1);
                        foreach (var sceneNpcId in tbSkillUpgrading.Values)
                        {
                            CreateMonsters(sceneNpcId, param2);
                        }
                    }
                        break;
                    case EnterPhaseOperateType.IntermittentCreateMonster:
                    {
                        DoIntermittentCreateMonster(param1, param2);
                    }
                        break;
                    case EnterPhaseOperateType.KillMonster:
                    {
                        if (param2 > 0)
                        {
                            CreateTimer(DateTime.Now.AddMilliseconds(param2), () => { RemoveObj(param1); });
                        }
                        else
                        {
                            RemoveObj(param1);
                        }
                    }
                        break;
                    case EnterPhaseOperateType.OpenGate:
                    {
                        var tbArea = Table.GetTriggerArea(param1);
                        if (tbArea == null)
                        {
                            break;
                        }
                        BroadcastSceneAction(tbArea);
                        if (tbArea.OffLineTrigger != -1)
                        {
                            Exdata = BitFlag.IntSetFlag(Exdata, tbArea.OffLineTrigger);
                        }
                    }
                        break;
                    case EnterPhaseOperateType.NotifyCountDown:
                    {
                        PushActionToAllPlayer(
                            player =>
                            {
                                player.Proxy.NotifyCountdown((ulong) DateTime.Now.AddSeconds(param1).ToBinary(), param2);
                            });
                    }
                        break;
                    case EnterPhaseOperateType.NotifyBattleReminder:
                    {
//通知客户端显示相关信息
                        PushActionToAllPlayer(
                            player => { player.Proxy.NotifyBattleReminder(19, Utils.WrapDictionaryId(param1), param2); });
                    }
                        break;
                    case EnterPhaseOperateType.RemoveBuff:
                    {
                        foreach (
                            var character in mObjDict.Values.Where(obj => obj.TypeId == param1).Cast<ObjCharacter>())
                        {
                            var datas = character.BuffList.GetBuffById(param2);
                            foreach (var data in datas)
                            {
                                MissBuff.DoEffect(this, character, data);
                                character.DeleteBuff(data, eCleanBuffType.EffectOver);
                            }
                        }
                    }
                        break;
                    case EnterPhaseOperateType.WaitTime:
                    {
                        SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddMilliseconds(param1),
                            () => { AccomplishPhaseRequire(EnterPhaseRequireType.Wait, param1); });
                    }
                        break;
                    case EnterPhaseOperateType.NotifyStartWarning:
                    {
                        PushActionToAllPlayer(
                            player =>
                            {
                                player.Proxy.NotifyStartWarning((ulong)DateTime.Now.AddSeconds(param1).ToBinary());
                            });
                    }
                        break;
                    case EnterPhaseOperateType.SwitchState:
                    {
                        var now = DateTime.Now;
                        var targetState = (eDungeonState) param1;
                        switch (targetState)
                        {
                            case eDungeonState.Start:
                            {
                                var startTime = now.AddSeconds(param2);
                                StartTimer(eDungeonTimerType.WaitStart, startTime, TimeOverStart);
                                StartTimer(eDungeonTimerType.WaitEnd,
                                    startTime.AddMinutes(mFubenRecord.TimeLimitMinutes), TimeOverEnd);
                            }
                                break;
                            case eDungeonState.WillClose:
                                break;
                            case eDungeonState.Closing:
                            {
                                var result = new FubenResult();
                                result.CompleteType = (int) eDungeonCompleteType.Success;
                                CompleteToAll(result, param2);

                                SendFubenInfo();
                            }
                                break;
							default:
	                        {
		                        Debug.Assert(false,"Type Error:"+targetState.ToString());
	                        }
		                        break;
                        }
                    }
                        break;
                }
            }

            //准备一下要发给客户端的数据结构
            FubenLogicRecord oldRecord = null;
            if (mFubenInfoMsg.LogicId != -1)
            {
                oldRecord = Table.GetFubenLogic(mFubenInfoMsg.LogicId);
            }
            var units = mFubenInfoMsg.Units;
            mFubenInfoMsg.LogicId = FubenLogicRecord.Id;
            mIsFubenInfoDirty = true;
            for (int i = 0, imax = FubenLogicRecord.FubenInfo.Length; i < imax; i++)
            {
                var type = (eFubenInfoType) FubenLogicRecord.FubenInfo[i];
                if ((int) type == -1)
                {
                    units.RemoveRange(i, units.Count - i);
                    break;
                }

                var infoValue = 0;
                if (type == eFubenInfoType.Percent)
                {
                    infoValue = 100;
                }
                if (i < units.Count)
                {
                    var unit = units[i];
                    if (FubenLogicRecord.IsClearCount == 0 && oldRecord != null && (int) type == oldRecord.FubenInfo[i])
                    {
//如果上一阶段的type和这一阶段的type一样，且IsClearCount == 0，不清计数
                    }
                    else
                    {
//否则，清计数           
                        if (unit.Params.Count > 0)
                            unit.Params[0] = infoValue;
                    }
                }
                else
                {
                    var unit = new FubenInfoUnit();
                    unit.Index = i;
                    unit.Params.Add(infoValue);
                    units.Add(unit);

                    switch (type)
                    {
                        case eFubenInfoType.Percent:
                        {
                            var pars = unit.Params;
                            if (pars.Count < 2)
                            {
                                pars.Add(0);
                            }
                        }
                            break;
                        case eFubenInfoType.StrongpointInfo:
                        {
                            var pars = unit.Params;
                            for (var j = 0; j < 3; ++j)
                            {
                                if (j < pars.Count)
                                {
                                    pars[j] = -1;
                                }
                                else
                                {
                                    pars.Add(-1);
                                }
                            }
                        }
                            break;
                        case eFubenInfoType.ShowDictionary2:
                        {
                            var pars = unit.Params;
                            for (var j = pars.Count; j < 2; ++j)
                            {
                                pars.Add(0);
                            }
                        }
                            break;
                        case eFubenInfoType.ShowDictionary3:
                        {
                            var pars = unit.Params;
                            for (var j = pars.Count; j < 3; ++j)
                            {
                                pars.Add(0);
                            }
                        }
                            break;
                        case eFubenInfoType.AllianceWarInfo:
                        {
                            var pars = unit.Params;
                            for (var j = pars.Count; j < 3; ++j)
                            {
                                pars.Add(0);
                            }
                        }
                            break;
                        case eFubenInfoType.AllianceWarState:
                        {
                            var pars = unit.Params;
                            for (var j = pars.Count; j < 2; ++j)
                            {
                                pars.Add(0);
                            }
                        }
                            break;
                    }
                }
            }
        }

        protected void AccomplishPhaseRequire(EnterPhaseRequireType type, int param)
        {
            Dictionary<int, EnterPhaseRequireStruct> requireDic;
            if (!mPhaseRequirementDic.TryGetValue(type, out requireDic))
            {
                return;
            }

            //特殊处理，如果杀怪类型中有-1，则表示，无论杀什么怪，都要计算数量
            if (type == EnterPhaseRequireType.KillMonster && requireDic.ContainsKey(-1))
            {
                param = -1;
            }

            EnterPhaseRequireStruct require;
            if (!requireDic.TryGetValue(param, out require))
            {
                return;
            }

            if (require.Counter >= require.Param2)
            {
//该条件已经完成了，不再计数
                return;
            }

            mIsFubenInfoDirty = true;
            require.AddCount(1, mFubenInfoMsg);

            if (require.Counter >= require.Param2)
            {
//完成了一个条件
                DetectPhaseEnd();
            }
        }

        //检查是否可以进入下一阶段了
        protected void DetectPhaseEnd()
        {
            var complete = mPhaseRequirements.All(requirement => requirement.Counter >= requirement.Param2);
            if (complete)
            {
                EnterNextPhase();
            }
        }

        protected void CreateMonsters(int npcId, int count)
        {
            CoroutineFactory.NewCoroutine(CreateMonstersCoroutine, npcId, count).MoveNext();
        }

        private IEnumerator CreateMonstersCoroutine(Coroutine co, int npcId, int count)
        {
            while (count-- > 0)
            {
                CreateSceneNpc(npcId);
                yield return SceneServer.Instance.ServerControl.Wait(co, new TimeSpan(50));
            }
        }

        #endregion
    }
}