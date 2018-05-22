#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
    public class TimerState
    {
        public TimerState(int State)
        {
            state = State;
            time = DateTime.MaxValue;
            ReduceTImes = 0;
        }
        public int state { get; set; }
        public DateTime time { get; set; }
        public int ReduceTImes { get; set; }
    }

    public class PetIsland : Scene
    {
        #region 刷新表格

        static PetIsland()
        {

        }

        #endregion

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region 数据
        // 体力
        private Dictionary<ulong, int> TiliDic = new Dictionary<ulong, int>();

        // 倒计时剔出状态
        private Dictionary<ulong, TimerState> StateDic = new Dictionary<ulong, TimerState>();

        // 玩家进入副本的时间点
        private Dictionary<ulong, DateTime> EnterTimeDic = new Dictionary<ulong, DateTime>();

        private Trigger mTrigger = null;
        private const int TickSecondInterval = 2000;

        #endregion

        #region 重写父类方法
        public override void OnCreate()
        {
            base.OnCreate();
            mTrigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now, LogicTick, TickSecondInterval);
        }
        public override void ExitDungeon(ObjPlayer player)
        {
            if (player == null) return;

            player.ExitDungeon();
        }

        public override void OnPlayerDie(ObjPlayer player, ulong characterId = 0)
        {
            if (player == null) return;

            var cost = Table.GetServerConfig(936).ToInt();
            PetIslandReduceTili(player.ObjId, (int)eExdataDefine.e630, cost);
        }

        public override void OnNpcDie(ObjNPC npc, ulong characterId = 0)
        {
            if (npc == null) return;
           
            base.OnNpcDie(npc, characterId);
            var tbNpc = Table.GetNpcBase(npc.TypeId);
            if (tbNpc != null)
            {
                //var killer = FindCharacter(characterId);
                ObjCharacter killer = null;
                if (npc.TableNpc.BelongType == 1)//灵兽岛，如果是队内伤害最高，就扣他的体力因为他会得宝物
                {
                    killer = FindCharacter(npc.GetTargetCharacterId());//灵兽岛Boss，是谁获得了奖励扣谁的体力
                }
                else
                {
                    killer = FindCharacter(characterId);//灵兽岛小怪，是谁获得了奖励扣谁的体力
                }
                if (killer == null)
                {
                    return;
                }

                //var player = killer.GetRewardOwner() as ObjPlayer;
                var player = killer as ObjPlayer;
                if (null != player)
                {
                    if (tbNpc.KillExpendType == (int)eExdataDefine.e630)
                    {
                        PetIslandReduceTili(player.ObjId, tbNpc.KillExpendType, tbNpc.KillExpendValue); 
                    }
                }
            }
        }

        public override void OnPlayerEnterOver(ObjPlayer player)
        {
            if (player == null) return;

            base.OnPlayerEnterOver(player);
            CoroutineFactory.NewCoroutine(GetPlayerTili, player.ObjId).MoveNext();

            TimerState outValue = null;
            if (StateDic.TryGetValue(player.ObjId, out outValue))
            {
                StateDic[player.ObjId].state = 1;
                StateDic[player.ObjId].time = DateTime.MaxValue;
            }
            else
            {
                StateDic.Add(player.ObjId, new TimerState(1));
            }

            DateTime outData;
            if (EnterTimeDic.TryGetValue(player.ObjId, out outData))
            {
                EnterTimeDic[player.ObjId] = DateTime.Now;
            }
            else
            {
                EnterTimeDic.Add(player.ObjId, DateTime.Now);
            }
        }

        public override void OnPlayerLeave(ObjPlayer player)
        {
            if (player == null) return;

            base.OnPlayerLeave(player);
            RemoveRecord(player.ObjId);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (null != mTrigger)
            {
                SceneServerControl.Timer.DeleteTrigger(mTrigger);
                mTrigger = null;
            }
        }

        public override void OnPlayerExDataChanged(ObjPlayer obj, int idx, int val)
        {
            if (obj == null || idx != (int)eExdataDefine.e630) return;
            base.OnPlayerExDataChanged(obj, idx, val);

            if (TiliDic.ContainsKey(obj.ObjId))
            {
                TiliDic[obj.ObjId] = val;
            }

            if (val > 0)
            {
                if (StateDic.ContainsKey(obj.ObjId))
                {
                    StateDic[obj.ObjId].state = 1;
                    StateDic[obj.ObjId].time = DateTime.MaxValue;
                } 
            }
            else
            {
                // 设置剔出状态 准备剔出
                if (StateDic.ContainsKey(obj.ObjId))
                {
                    if (StateDic[obj.ObjId].state != 0)
                    {
                        StateDic[obj.ObjId].state = 0;
                        if (StateDic[obj.ObjId].time == DateTime.MaxValue)
                        {
                            StateDic[obj.ObjId].time = DateTime.Now.AddSeconds(10);
                        }
                    }
                }
            }
        }

        #endregion

        #region 内部逻辑

        private void PetIslandReduceTili(ulong playerObjId, int exdataId, int num)
        {
            CoroutineFactory.NewCoroutine(PetIslandReduceTili, playerObjId, exdataId, num).MoveNext();
        }

        private IEnumerator PetIslandReduceTili(Coroutine co, ulong playerObjId, int exdataId, int num)
        {
            if (exdataId != (int)eExdataDefine.e630)
            {
                Logger.Error("PetIslandReduceTili ExData table error not 630");
                yield break;
            }

            var refreshTili = 0;
            if (!TiliDic.TryGetValue(playerObjId, out refreshTili))
            {
                yield break;
            }

            var oldvalue = refreshTili;

            refreshTili -= num;

            if (refreshTili <= 0)
            {
                // 设置剔出状态 准备剔出
                if (StateDic.ContainsKey(playerObjId))
                {
                    if (StateDic[playerObjId].state != 0)
                    {
                        StateDic[playerObjId].state = 0;
                        StateDic[playerObjId].time = DateTime.Now.AddSeconds(10); 
                    }
                }
            }
            refreshTili = Math.Max(refreshTili, 0);
            TiliDic[playerObjId] = refreshTili;

            var data = new Dict_int_int_Data();
            if (refreshTili <= 0)
            {
                data.Data.Add((int)eExdataDefine.e630, refreshTili - oldvalue); 
            }
            else
            {
                data.Data.Add((int)eExdataDefine.e630, -num); 
            }

            var msg1 = SceneServer.Instance.LogicAgent.SSChangeExdata(playerObjId, data);
            yield return msg1.SendAndWaitUntilDone(co);
        }

        private IEnumerator GetPlayerTili(Coroutine co, ulong objId)
        {
            var ids = new Int32Array();
            ids.Items.Add((int)eExdataDefine.e630);
            var msg = SceneServer.Instance.LogicAgent.SSFetchExdata(objId, ids);
            yield return msg.SendAndWaitUntilDone(co);
            if (msg.State != MessageState.Reply)
            {
                Logger.Error("PetIslandReduceTili SSFetchExdata return with state = {0}", msg.State);
                yield break;
            }
            if (msg.ErrorCode != (int) ErrorCodes.OK)
            {
                Logger.Error("PetIslandReduceTili SSFetchExdata return with err = {0}", msg.ErrorCode);
                yield break;
            }
            if (msg.Response.Items.Count <= 0)
            {
                Logger.Error("PetIslandReduceTili SSFetchExdata return with err = {0}", msg.ErrorCode);
                yield break;
            }

            var outValue = 0;
            if (TiliDic.TryGetValue(objId, out outValue))
            {
                TiliDic[objId] = msg.Response.Items[0];
            }
            else
            {
                TiliDic.Add(objId, msg.Response.Items[0]); 
            }

            if (msg.Response.Items[0] > 0)
            {
                if (StateDic.ContainsKey(objId))
                {
                    StateDic[objId].state = 1;
                    StateDic[objId].time = DateTime.MaxValue;
                }
            }
        }

        private void RemoveRecord(ulong characterId)
        {
            if (TiliDic.ContainsKey(characterId))
            {
                TiliDic.Remove(characterId);
            }

            if (StateDic.ContainsKey(characterId))
            {
                StateDic.Remove(characterId);
            }

            if (EnterTimeDic.ContainsKey(characterId))
            {
                EnterTimeDic.Remove(characterId);
            }
        }

        private void LogicTick()
        {
            // 每5分钟消耗一点体力
            foreach (var value in EnterTimeDic)
            {
                if (DateTime.Now > EnterTimeDic[value.Key])
                {
                    TimeSpan delta = DateTime.Now - EnterTimeDic[value.Key];
                    var cost = Table.GetServerConfig(937).ToInt();
                    if (cost != 0)
                    {
                        var needReduceTimes = (int)delta.TotalSeconds / cost;
                        if (StateDic.ContainsKey(value.Key))
                        {
                            if (needReduceTimes > StateDic[value.Key].ReduceTImes)
                            {
                                PetIslandReduceTili(value.Key, (int)eExdataDefine.e630, 1);
                                StateDic[value.Key].ReduceTImes += 1;
                            }
                        }  
                    }
                }
            }

            var tempRemove = new List<ulong>();
            tempRemove.Clear();

            DynamicActivityRecord tbDynamic = Table.GetDynamicActivity(9);
            if (tbDynamic != null)
            {
                if (!Utils.CheckIsWeekLoopOk(tbDynamic.WeekLoop))
                {
                    foreach (var data in StateDic)
                    {
                        var character = FindCharacter(data.Key);
                        if (character != null)
                        {
                            //把玩家踢出去
                            var player = character as ObjPlayer;
                            if (player != null)
                            {
                                CoroutineFactory.NewCoroutine(player.ExitDungeon).MoveNext();
                            }
                        }
                    }
                    return;
                }
            }

            foreach (var data in StateDic)
            {
                if (data.Value != null && data.Value.state == 0 && data.Value.time <= DateTime.Now)
                {
                    var character = FindCharacter(data.Key);
                    if (character != null)
                    {
                        //把玩家踢出去
                        var player = character as ObjPlayer;
                        if (player != null)
                        {
                            if (TiliDic.ContainsKey(data.Key))
                            {
                                if (TiliDic[data.Key] <= 0)
                                {
                                    CoroutineFactory.NewCoroutine(player.ExitDungeon).MoveNext();
                                    tempRemove.Add(data.Key);
                                    //RemoveRecord(data.Key);  迭代器失效
                                }
                                else
                                {
                                    if (StateDic.ContainsKey(data.Key))
                                    {
                                        StateDic[data.Key].state = 1;
                                        StateDic[data.Key].time = DateTime.MaxValue;
                                    } 
                                }
                            }
                        }
                    }
                }
            }

            foreach (var data in tempRemove)
            {
                if (TiliDic.ContainsKey(data))
                {
                    RemoveRecord(data);
                }
            }
        }

        #endregion
    }
}