#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Scene
{
    public enum BattleState
    {
        None = 0,
        Dogfall = 1,
        WinA = 2,
        WinB = 3
    }

    public class BattleScene200000 : UniformDungeon
    {
        #region 初始化

        private bool Init()
        {
            mBattleState = BattleState.None;
            StartTimer(eDungeonTimerType.WaitStart, DateTime.Now.AddSeconds(20.0f), TimeOverStart);
            mStartWarnTrigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(15.0f),
                OnTriggerStartWarn);
            return true;
        }

        #endregion

        #region 数据

        //A 0 天使  B 1 火龙
        public int[] NpcIds = {50000, 50001, 50002, 50015, 50016, 50017, 50030, 50031, 500006, 500007};
        private BattleState mBattleState = BattleState.None;
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public Dictionary<int, int> mDieDictionary = new Dictionary<int, int>();
        private readonly Dictionary<ulong, ObjPlayer>[] mPlayers = new Dictionary<ulong, ObjPlayer>[2];
        private readonly Dictionary<ulong, Trigger> mLeaveTrigger = new Dictionary<ulong, Trigger>();
        private readonly List<ulong> mQuitPlayers = new List<ulong>();
        private readonly ulong[] mTeams = {ulong.MaxValue, ulong.MaxValue};
        private readonly ulong[] mDamagTotal = {0, 0};
        private readonly int[] mBuffLayer = {0, 0};
        private readonly List<ObjNPC>[] mNpcs = new List<ObjNPC>[2];
        private readonly bool[] mBossHpWarn = {false, false};
        private bool mIsInit;
        //Trigger
        private Trigger mStartFightTrigger;
        private Trigger mFightWarnTrigger;
        private Trigger mStartWarnTrigger;
        private Trigger mNpc5003Trigger;
        private Trigger mPlayerBuffTrigger;
        private Trigger mBossBuffTrigger;
        private readonly Dictionary<ulong, Trigger> mReliveTrigger = new Dictionary<ulong, Trigger>();
        private bool mIs50030;
        private ObjNPC mNpcLeader50000;  // 天使领袖
        private ObjNPC mNpc50001;        // 天使前锋
        private ObjNPC mNpc50002;        // 天使守卫塔
        private ObjNPC mNpcLeader50015;  // 火龙王
        private ObjNPC mNpc50016;        // 火龙前锋
        private ObjNPC mNpc50017;        // 火龙守卫塔
        private ObjNPC mNpc50030;
        private List<ulong> BattleObjIdList = new List<ulong>();
        #endregion

        #region 重载函数

        public override void StartDungeon()
        {
            StartTimer(eDungeonTimerType.WaitEnd, DateTime.Now.AddMinutes(mFubenRecord.TimeLimitMinutes), TimeOverEnd);
            base.StartDungeon();
            var npcList = new List<ObjNPC>();

            //移出阻挡npc
            foreach (var objBase in mObjDict)
            {
                if (objBase.Value.GetObjType() == ObjType.NPC)
                {
                    var objNpc = objBase.Value as ObjNPC;
                    var npcId = objNpc.TableNpc.Id;
                    if (npcId == 999)
                    {
                        npcList.Add(objNpc);
                    }
                    BattleObjIdList.Add(objNpc.ObjId);
                }
            }
            foreach (var npc in npcList)
            {
                LeaveScene(npc);
            }

            if (mFightWarnTrigger == null)
            {
                mFightWarnTrigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(3.0f),
                    OnTriggerFightWarn);
            }

            if (mNpc5003Trigger == null)
            {
                mNpc5003Trigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(60.0f),
                    OnTriggerNpc5003Born);
            }

            if (mPlayerBuffTrigger == null)
            {
                mPlayerBuffTrigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(15.0f),
                    OnTriggerPlayerBuff, 15*1000);
            }
            if (mBossBuffTrigger == null)
            {
                mBossBuffTrigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(6.0f),
                    OnTriggerBossBuff, 6*1000);
            }
            mNpcLeader50000.AddBuff(3002, 1, mNpcLeader50000);
            mNpcLeader50015.AddBuff(3002, 1, mNpcLeader50015);
            SetInvincible(mNpcLeader50000);
            SetInvincible(mNpc50001);
            SetInvincible(mNpcLeader50015);
            SetInvincible(mNpc50016);

            mStartFightTrigger = null;
        }
        public override void AfterPlayerEnterOver(ObjPlayer player)
        {
            player.AddBuff(3010, 1, player);
        }

        // 设置无敌
        private void SetInvincible(ObjCharacter obj)
        {
            if (obj != null)
            {
                obj.AddBuff(1501, 1, obj);
            }
        }

        // 取消无敌
        private void CancelInvincible(ObjCharacter obj)
        {
            if (obj != null)
            {
                obj.DeleteBuff(1501, eCleanBuffType.Clear);
            }
        }

        public override void OnObjBeforeEnterScene(ObjBase obj) 
        {
            var player = obj as ObjPlayer;
            if (player == null)
            {
                return;
            }

            if (mIsInit == false)
            {
                Init();
                mIsInit = true;
            }

            var teamId = player.mDbData.P1vP1CharacterId;
            var flag = -1;
            for (var i = 0; i < 2; i++)
            {
                var team = mTeams[i];
                if (team == ulong.MaxValue)
                {
                    flag = i;
                    mTeams[i] = teamId;
                    break;
                }
                if (team == teamId)
                {
                    flag = i;
                    break;
                }
            }

            if (flag == -1)
            {
                Logger.Error("OnPlayerEnter Error CharacterId = {0},TeamId = {1}", player.ObjId, teamId);
                return;
            }

            var teamPlayers = mPlayers[flag];
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
            var newCamp = 4 + flag;
            player.SetCamp(newCamp);
            var pos = Utility.MakeVectorMultiplyPrecision(player.GetPosition().X, player.GetPosition().Y);
            player.Proxy.NotifyCampChange(newCamp, pos);
        }

        public override void OnPlayerEnter(ObjPlayer player)
        {
            PlayerLog.WriteLog(player.ObjId, "----HLKBattle------OnPlayerEnter----------");
            //bool isLeave = false;
            if (mLeaveTrigger.ContainsKey(player.ObjId))
            {
                var trigger = mLeaveTrigger[player.ObjId];
                SceneServerControl.Timer.DeleteTrigger(trigger);
                mLeaveTrigger.Remove(player.ObjId);
                //isLeave = true; 
            }
            else
            {
                CoroutineFactory.NewCoroutine(PlayerEnter, player).MoveNext();
            }

            base.OnPlayerEnter(player);
        }

        public override void OnPlayerDie(ObjPlayer player, ulong characterId = 0)
        {
            var side = GetPlayerSide(player);
            if (side == -1)
            {
                Logger.Error("OnPlayerDie GetPlayerSide  ObjOd{0}", player.ObjId);
                return;
            }
            var bossName = "";
            var caster = FindCharacter(characterId);
            if (caster == null)
            {
                return;
            }
            caster = caster.GetRewardOwner();
            var strColor1 = "";
            var strColor2 = "";
            if (side == 0)
            {
                bossName = Utils.AddDictionaryId(220451);
                strColor1 = Utils.AddDictionaryId(220453);
                strColor2 = Utils.AddDictionaryId(220454);
            }
            else if (side == 1)
            {
                bossName = Utils.AddDictionaryId(220452);
                strColor1 = Utils.AddDictionaryId(220454);
                strColor2 = Utils.AddDictionaryId(220453);
            }
            //所有获得2层BUFF-
            Add3003Buff(side, 2, true);

            var args = new List<string>();
            args.Add(strColor1);
            args.Add(caster.GetName());
            args.Add(strColor2);
            args.Add(player.GetName());
            args.Add(bossName);
            args.Add(mBuffLayer[side].ToString());

            var reliveTime = DateTime.Now.AddSeconds(10);
            var reliveObj = SceneServerControl.Timer.CreateTrigger(reliveTime, () =>
            {
               
                if (mLeaveTrigger.ContainsKey(player.ObjId)) return;
                FixPostion(player);
                player.Relive();
                mReliveTrigger.Remove(player.ObjId);

                PlayerLog.WriteLog(player.ObjId, "----HLKBattle------PlayerRelive----------Time Over");
            });
            mReliveTrigger.Add(player.ObjId, reliveObj);

            PlayerLog.WriteLog(player.ObjId, "----HLKBattle------PlayerDie----------KillId={0}", caster.ObjId);

            //通告，某人被某人杀了
            //var info = Utils.WrapDictionaryId(220421, args);
            //BroadcastScene(
            //    objPlayer =>
            //    {
            //        if (objPlayer != null && objPlayer.Proxy != null)
            //        {
            //            objPlayer.Proxy.NotifyBattleReminder(14, info, 1);
            //        }
            //    });


            args.Clear();
            args.Add(caster.GetName());//此处 A 击杀了 B   player是被击杀者
            args.Add(player.GetName());
            string text = side == 1 ? Utils.WrapDictionaryId(61000, args) : Utils.WrapDictionaryId(61001, args);

            PushActionToAllPlayer(p =>
            {
                 p.Proxy.NotifyBattleReminder(14, text, 1);
            });



            //通知死者，复活倒计时
            player.Proxy.NotifyBattleReminder(14, Utils.WrapDictionaryId(220450), 1);
            player.Proxy.NotifyCountdown((ulong) reliveTime.ToBinary(), (int) eCountdownType.BattleRelive);
        }

        public override void OnPlayerLeave(ObjPlayer player)
        {
            base.OnPlayerLeave(player);

            PlayerLog.WriteLog(player.ObjId, "----HLKBattle------OnPlayerLeave----------");

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

        public override void OnNpcDie(ObjNPC npc, ulong characterId = 0)
        {
            base.OnNpcDie(npc, characterId);

            if (State != eDungeonState.Start)
            {
                return;
            }

            if (npc == mNpc50030)
            {
                OnDie50030(npc);
            }
            else if (npc == mNpc50001)
            {
                Add3003Buff(0, 10, true);
                BroadcastSceneSide(0, player =>
                {
                    //天使领袖：我们的前锋倒下了！我将直面敌人！你们必须打起精神来了！
                    player.Proxy.NotifyMessage((int) eSceneNotifyType.Dictionary, 220424.ToString(), 1);
                });
                CancelInvincible(mNpcLeader50000);
            }
            else if (npc == mNpc50002)
            {
                Add3003Buff(0, 10, true);
                BroadcastSceneSide(0, player =>
                {
                    //天使领袖：我们第一层守卫倒下了！我受到的伤害再次加深！不要灰心，坚持就是胜利！
                    player.Proxy.NotifyMessage((int) eSceneNotifyType.Dictionary, 220422.ToString(), 1);
                });
                CancelInvincible(mNpc50001);
            }
            else if (npc == mNpc50016)
            {
                Add3003Buff(1, 10, true);
                BroadcastSceneSide(1, player =>
                {
                    //火龙王：我们的前锋倒下了！我将直面敌人！你们必须打起精神来了！
                    player.Proxy.NotifyMessage((int) eSceneNotifyType.Dictionary, 220425.ToString(), 1);
                });
                CancelInvincible(mNpcLeader50015);
            }
            else if (npc == mNpc50017)
            {
                Add3003Buff(1, 10, true);
                BroadcastSceneSide(1, player =>
                {
                    //火龙王：我们第一层守卫倒下了！我受到的伤害再次加深了！不要灰心，坚持就是胜利！
                    player.Proxy.NotifyMessage((int) eSceneNotifyType.Dictionary, 220423.ToString(), 1);
                });
                CancelInvincible(mNpc50016);
            }
            else if (npc == mNpcLeader50000)
            {
                if (mBattleState == BattleState.None)
                {
                    mBattleState = BattleState.WinB;
                    ResultOver();
                }
            }
            else if (npc == mNpcLeader50015)
            {
                if (mBattleState == BattleState.None)
                {
                    mBattleState = BattleState.WinA;
                    ResultOver();
                }
            }
            BattleObjIdList.Remove(npc.ObjId);
        }

        public override void OnCreate()
        {
            base.OnCreate();
            for (var i = 0; i < 2; i++)
            {
                mBossHpWarn[i] = false;
                mDamagTotal[i] = 0ul;
                mNpcs[i] = new List<ObjNPC>();
            }
            foreach (var objBase in mObjDict)
            {
                if (objBase.Value.GetObjType() == ObjType.NPC)
                {
                    var objNpc = objBase.Value as ObjNPC;
                    var npcId = objNpc.TableNpc.Id;
                    if (npcId == NpcIds[0])
                    {
                        mNpcLeader50000 = objNpc;
                        mNpcLeader50000.OnDamageCallback = OnDamage50000;
                        mNpcs[0].Add(mNpcLeader50000);
                    }
                    else if (npcId == NpcIds[1])
                    {
                        mNpc50001 = objNpc;
                        mNpc50001.OnDamageCallback = OnDamage50001;
                        mNpcs[0].Add(mNpc50001);
                    }
                    else if (npcId == NpcIds[2])
                    {
                        mNpc50002 = objNpc;
                        mNpcs[0].Add(mNpc50002);
                    }
                    else if (npcId == NpcIds[3])
                    {
                        mNpcLeader50015 = objNpc;
                        mNpcLeader50015.OnDamageCallback = OnDamage50015;
                        mNpcs[1].Add(mNpcLeader50015);
                    }
                    else if (npcId == NpcIds[4])
                    {
                        mNpc50016 = objNpc;
                        mNpc50016.OnDamageCallback = OnDamage50016;
                        mNpcs[1].Add(mNpc50016);
                    }
                    else if (npcId == NpcIds[5])
                    {
                        mNpc50017 = objNpc;
                        mNpcs[1].Add(mNpc50017);
                    }
                    else if (npcId == NpcIds[6])
                    {
                        mNpc50030 = objNpc;
                    }
                }
            }
            for (var i = 0; i < 2; i++)
            {
                mPlayers[i] = new Dictionary<ulong, ObjPlayer>();
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (mStartWarnTrigger != null)
            {
                SceneServerControl.Timer.DeleteTrigger(mStartWarnTrigger);
                mStartWarnTrigger = null;
            }
            if (mStartFightTrigger != null)
            {
                SceneServerControl.Timer.DeleteTrigger(mStartFightTrigger);
                mStartFightTrigger = null;
            }
            if (mFightWarnTrigger != null)
            {
                SceneServerControl.Timer.DeleteTrigger(mFightWarnTrigger);
                mFightWarnTrigger = null;
            }
            foreach (var o in mLeaveTrigger)
            {
                SceneServerControl.Timer.DeleteTrigger(o.Value);
            }
            mLeaveTrigger.Clear();
            foreach (var o in mReliveTrigger)
            {
                SceneServerControl.Timer.DeleteTrigger(o.Value);
            }
            mReliveTrigger.Clear();

            for (var i = 0; i < 2; i++)
            {
                mTeams[i] = 0;
            }
        }

        public override void EndDungeon()
        {
            if (mBattleState == BattleState.None)
            {
                if (mNpcLeader50000.Attr.GetDataValue(eAttributeType.HpNow) >
                    mNpcLeader50015.Attr.GetDataValue(eAttributeType.HpNow))
                {
                    mBattleState = BattleState.WinA;
                }
                else if (mNpcLeader50000.Attr.GetDataValue(eAttributeType.HpNow) <
                         mNpcLeader50015.Attr.GetDataValue(eAttributeType.HpNow))
                {
                    mBattleState = BattleState.WinB;
                }
                else
                {
                    mBattleState = BattleState.Dogfall;
                }
                ResultOver();
            }
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

        private void OnTriggerStartWarn()
        {
            mStartWarnTrigger = null;

            foreach (var player in mPlayers)
            {
                foreach (var objPlayer in player)
                {
                    //"战斗将在5秒后开始！"
                    objPlayer.Value.Proxy.NotifyMessage((int) eSceneNotifyType.Dictionary, 220417.ToString(), 1);
                    objPlayer.Value.Proxy.NotifyCountdown((ulong) DateTime.Now.AddSeconds(5.0f).ToBinary(),
                        (int) eCountdownType.BattleFight);
                }
            }
        }

        private void OnTriggerNpc5003Born()
        {
            if (mNpc50030 != null)
            {
                return;
            }
            //int npcId = 500006;
            var npcId = NpcIds[8];
            var dicId = -1;
            if (mIs50030 == false)
            {
                //npcId = 500007;
                mIs50030 = true;
                dicId = 220426;
                //"战场中央刷新了神秘水晶，击碎它的人可以获得水晶的赐福！";
            }
            else
            {
                mIs50030 = false;
                npcId = NpcIds[9];
                dicId = 220428;
                //“战场中央刷新了古神之手，击碎它的人可以直接给对方造成易伤效果！”
            }
            mNpc50030 = CreateSceneNpc(npcId);
            mDamagTotal[0] = 0ul;
            mDamagTotal[1] = 0ul;
            mNpc50030.OnDamageCallback = OnDamage50030;
            BroadcastScene(player =>
            {
                if (player.Proxy != null)
                {
                    player.Proxy.NotifyMessage((int) eSceneNotifyType.Dictionary, dicId.ToString(), 1);
                }
            });
            mNpc5003Trigger = null;
        }

        private void OnTriggerPlayerBuff()
        {
            BroadcastScene(player => { player.AddBuff(3009, 1, player); });
        }

        private void OnTriggerBossBuff()
        {
            for (var i = 0; i < 2; i++)
            {
                Add3003Buff(i, 1, false);
            }
        }

        private void OnTriggerFightWarn()
        {
            for (var i = 0; i < 2; i++)
            {
                var dicId = -1;
                if (i == 0)
                {
                    //天使领袖：我们的时间不多，在火龙窟中我会持续受到伤害加成效果！
                    dicId = 220419;
                }
                else
                {
                    //火龙王：我们的时间不多，在天使的威胁下我会持续受到易伤效果！
                    dicId = 220420;
                }
                var playerList = mPlayers[i];
                foreach (var objPlayer in playerList)
                {
                    if (objPlayer.Value!=null)
                        objPlayer.Value.Proxy.NotifyMessage((int) eSceneNotifyType.Dictionary, dicId.ToString(), 1);
                }
            }
            mFightWarnTrigger = null;
        }

        private void Add3003Buff(int side, int layer, bool isAll)
        {
            if (side != 0 && side != 1)
            {
                return;
            }
            if (isAll)
            {
                foreach (var npc in mNpcs[side])
                {
                    npc.AddBuff(3003, layer, npc);
                }
                mBuffLayer[side] += layer;
            }
            else
            {
                mNpcs[side][0].AddBuff(3003, layer, mNpcs[side][0]);
                ;
                mBuffLayer[side] += layer;
            }
            SetFubenInfo(side, 1, mBuffLayer[side]);
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
            PlayerLog.WriteLog(player.ObjId, "----HLKBattle------FixPostion----------{0}", player.GetPosition());
        }

        private int GetPlayerSide(ObjPlayer player)
        {
            var index = 0;
            foreach (var dictionary in mPlayers)
            {
                if (dictionary.ContainsKey(player.ObjId))
                {
                    return index;
                }
                index++;
            }
            //for (int i = 0; i < 2; i++)
            //{
            //    if (mTeams[i] == player.GetTeamId())
            //    {
            //        return i;
            //    }
            //}
            return -1;
        }

        public void BroadcastScene(Action<ObjPlayer> action)
        {
            foreach (var player in mPlayers)
            {
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
        }

        public void BroadcastSceneSide(int side, Action<ObjPlayer> action)
        {
            var player = mPlayers[side];
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

        public IEnumerator ResultOverCoroutine(Coroutine coroutine)
        {
            EnterAutoClose(10);

            if (mNpc5003Trigger != null)
            {
                SceneServerControl.Timer.DeleteTrigger(mNpc5003Trigger);
                mNpc5003Trigger = null;
            }
            if (mPlayerBuffTrigger != null)
            {
                SceneServerControl.Timer.DeleteTrigger(mPlayerBuffTrigger);
                mPlayerBuffTrigger = null;
            }

            if (mBossBuffTrigger != null)
            {
                SceneServerControl.Timer.DeleteTrigger(mBossBuffTrigger);
                mBossBuffTrigger = null;
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
                BroadcastScene(player => { CoroutineFactory.NewCoroutine(CharacterResultOver, player, 0).MoveNext(); });
            }

            ObjPlayer onePlayer = null;
            foreach (var dictionary in mPlayers)
            {
                foreach (var value in dictionary.Values)
                {
                    onePlayer = value;
                    break;
                }
                if (onePlayer != null)
                {
                    break;
                }
            }
            if (onePlayer != null)
            {
                PlayerLog.WriteLog(onePlayer.ObjId, "----HLKBattle------SSBattleEnd----------");

                var msg = SceneServer.Instance.TeamAgent.SSBattleEnd(onePlayer.ObjId, Guid);
                yield return msg.SendAndWaitUntilDone(coroutine);
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
            var playerList = mPlayers[side];
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

        public override void ExitDungeon(ObjPlayer player)
        {
            mQuitPlayers.Add(player.ObjId);

            CoroutineFactory.NewCoroutine(player.ExitDungeon).MoveNext();
        }

        private void OnDie50030(ObjNPC npc)
        {
            if (npc == null)
            {
                return;
            }
            if (npc.TableNpc == null)
            {
                return;
            }
            var npcId = npc.TableNpc.Id;
            var side = 0;
            if (mDamagTotal[1] > mDamagTotal[0])
            {
                side = 1;
            }
            if (npcId == 50030)
            {
                var str = "";
                if (side == 0)
                {
                    str = Utils.AddDictionaryId(220451);
                }
                else
                {
                    str = Utils.AddDictionaryId(220452);
                }
                var args = new List<string>();
                args.Add(str);

                BroadcastSceneSide(side, player => { player.AddBuff(3008, 1, player); });
                BroadcastScene(
                    player => { player.Proxy.NotifyBattleReminder(14, Utils.WrapDictionaryId(220427, args), 1); });
            }
            else if (npcId == 50031)
            {
                var str = "";
                var targetStr = "";
                if (side == 0)
                {
                    str = Utils.AddDictionaryId(220451);
                    targetStr = Utils.AddDictionaryId(220452);
                    Add3003Buff(1, 5, true);
                }
                else
                {
                    str = Utils.AddDictionaryId(220452);
                    targetStr = Utils.AddDictionaryId(220451);
                    Add3003Buff(0, 5, true);
                }
                var args = new List<string>();
                args.Add(str);
                args.Add(targetStr);
                BroadcastScene(
                    player => { player.Proxy.NotifyBattleReminder(14, Utils.WrapDictionaryId(220429, args), 1); });
            }
            if (mNpc5003Trigger == null)
            {
                mNpc5003Trigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(60.0f),
                    OnTriggerNpc5003Born);
            }
            mNpc50030 = null;
        }

        private void OnDamage50030(ObjNPC npc, ObjCharacter caster, int damage)
        {
            var player = caster as ObjPlayer;
            if (player == null)
            {
                return;
            }
            var side = GetPlayerSide(player);
            if (side == -1)
            {
                return;
            }
            mDamagTotal[side] += (ulong) damage;
        }

        private void OnDamage50001(ObjNPC npc, ObjCharacter caster, int damage)
        {
            if (BattleObjIdList.Contains(mNpc50002.ObjId))
            {
                BroadcastScene(player => { player.Proxy.NotifyBattleReminder(14, Utils.WrapDictionaryId(100001503), 1); });
            }
        }
        private void OnDamage50000(ObjNPC npc, ObjCharacter caster, int damage)
        {
            if (BattleObjIdList.Contains(mNpc50002.ObjId) || BattleObjIdList.Contains(mNpc50001.ObjId))
            {
                BroadcastScene(player => { player.Proxy.NotifyBattleReminder(14, Utils.WrapDictionaryId(100001504), 1); });
            }
            //副本信息
            var infoIdx = FubenLogicRecord.SwitchInfoPa[1];
            var hpPercent = 100.0*npc.GetAttribute(eAttributeType.HpNow)/npc.GetAttribute(eAttributeType.HpMax);
            SetFubenInfo(infoIdx, 0, (int) hpPercent);

            if (mBossHpWarn[0] == false && hpPercent <= 10)
            {
                mBossHpWarn[0] = true;
                BroadcastSceneSide(0, player =>
                {
                    //我支撑不了太久了！胜利的希望在哪里？
                    player.Proxy.NotifyMessage((int) eSceneNotifyType.Dictionary, 220430.ToString(), 1);
                });
            }
        }

        private void OnDamage50015(ObjNPC npc, ObjCharacter caster, int damage)
        {
            if (BattleObjIdList.Contains(mNpc50017.ObjId) || BattleObjIdList.Contains(mNpc50016.ObjId))
            {
                BroadcastScene(player => { player.Proxy.NotifyBattleReminder(14, Utils.WrapDictionaryId(100001506), 1); });
            }
            //副本信息
            var infoIdx = FubenLogicRecord.SwitchInfoPa[0];
            var hpPercent = 100.0*npc.GetAttribute(eAttributeType.HpNow)/npc.GetAttribute(eAttributeType.HpMax);
            SetFubenInfo(infoIdx, 0, (int) hpPercent);

            if (mBossHpWarn[1] == false && hpPercent <= 10)
            {
                mBossHpWarn[1] = true;
                BroadcastSceneSide(1, player =>
                {
                    //我支撑不了太久了！胜利的希望在哪里？
                    player.Proxy.NotifyMessage((int) eSceneNotifyType.Dictionary, 220430.ToString(), 1);
                });
            }
        }
        private void OnDamage50016(ObjNPC npc, ObjCharacter caster, int damage)
        {
            if (BattleObjIdList.Contains(mNpc50017.ObjId))
            {
                BroadcastScene(player => { player.Proxy.NotifyBattleReminder(14, Utils.WrapDictionaryId(100001505), 1); });
            }
        }

        private void SetFubenInfo(int infoIdx, int paramIdx, int count)
        {
            mIsFubenInfoDirty = true;
            var pars = mFubenInfoMsg.Units[infoIdx].Params;
            pars[paramIdx] = count;
        }

        #endregion
    }

    public class BattleScene200001 : BattleScene200000
    {
        public override void OnCreate()
        {
            NpcIds = new[] {50003, 50004, 50005, 50018, 50019, 50020, 50032, 50033, 500006, 500007};
            base.OnCreate();
        }
    }

    public class BattleScene200002 : BattleScene200000
    {
        public override void OnCreate()
        {
            NpcIds = new[] {50006, 50007, 50008, 50021, 50022, 50023, 50034, 50035, 500006, 500007};
            base.OnCreate();
        }
    }

    public class BattleScene200003 : BattleScene200000
    {
        public override void OnCreate()
        {
            NpcIds = new[] {50009, 50010, 50011, 50024, 50025, 50026, 50036, 50037, 500006, 500007};
            base.OnCreate();
        }
    }

    public class BattleScene200004 : BattleScene200000
    {
        public override void OnCreate()
        {
            NpcIds = new[] {50012, 50013, 50014, 50027, 50028, 50029, 50038, 50039, 500006, 500007};
            base.OnCreate();
        }
    }
}