#region using

using System;
using System.Collections;
using System.Collections.Generic;
using DataContract;
using DataTable;
using Scorpion;
using Mono.GameMath;
using NLog;
using Shared;

#endregion

namespace Scene
{
    public class CastleCraft : DungeonScene
    {
        #region 刷新表格

        static CastleCraft()
        {
            var tbSU = Table.GetSkillUpgrading(330);
            foreach (var value in tbSU.Values)
            {
                var tbPos = Table.GetRandomCoordinate(value);
                var pos = new Vector2(tbPos.PosX, tbPos.PosY);
                RelivePos.Add(pos);
            }
        }

        #endregion

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region 数据

        public static List<Vector2> RelivePos = new List<Vector2>();

        public DamageList RankData = new DamageList();

        //玩家积分
        //id => score
        public Dictionary<ulong, DamageUnit> PlayerScore = new Dictionary<ulong, DamageUnit>();

        public DamageUnitComparer DuComparer = new DamageUnitComparer();

        //箱子给的buff
        public static readonly int[] BoxBuffId = {1202, 1203, 1204};
        //死亡给的buff
        public static readonly int[] DieBuffId = {1200, 1201};
        //无敌buff
        public const int InvincibleBuffId = 1205;

        private bool _running;

        #endregion

        #region 重写父类方法

        public override void OnCreate()
        {
            base.OnCreate();

            var hour = 0;
            var min = 0;
            if (Param.Param.Count > 1)
            {
                hour = Param.Param[0];
                min = Param.Param[1];
            }
            else
            {
                if (mFubenRecord.OpenTime.Count > 1)
                {
                    Logger.Error("Fuben Table Error!! id = {0}, OpenTime Count > 1", mFubenRecord.Id);
                    return;
                }

                var time = mFubenRecord.OpenTime[0];
                hour = time/100;
                min = time%100;
            }

            //设置副本开启和结束时间
            var now = DateTime.Now;
            var startTime = new DateTime(now.Year, now.Month, now.Day, hour, min, 0, DateTimeKind.Local);
            StartTimer(eDungeonTimerType.WaitStart, startTime, TimeOverStart);
            var endTime = startTime.AddMinutes(mFubenRecord.TimeLimitMinutes);
            StartTimer(eDungeonTimerType.WaitEnd, endTime, TimeOverEnd);

            //副本提示2
            var tipTime = startTime.AddSeconds(-5);
            {
                var content = Utils.WrapDictionaryId(42001);
                CreateTimer(tipTime, () =>
                {
                    var countDown = (ulong) DateTime.Now.AddSeconds(5).ToBinary();
                    var type = (int) eCountdownType.BattleFight;
                    PushActionToAllPlayer(player =>
                    {
                        player.Proxy.NotifyBattleReminder(14, content, 1);
                        player.Proxy.NotifyCountdown(countDown, type);
                    });
                });
            }

            //副本提示3
            tipTime = startTime.AddSeconds(570);
            {
                var content = Utils.WrapDictionaryId(42002);
                CreateTimer(tipTime,
                    () => { PushActionToAllPlayer(player => { player.Proxy.NotifyBattleReminder(14, content, 1); }); });
            }

            //副本提示4
            tipTime = tipTime.AddSeconds(25);
            {
                var content = Utils.WrapDictionaryId(42003);
                CreateTimer(tipTime,
                    () => { PushActionToAllPlayer(player => { player.Proxy.NotifyBattleReminder(14, content, 1); }); });
            }
        }

        public override bool CanPk()
        {
            return state == eDungeonState.Start;
        }

        public override void StartDungeon()
        {
            base.StartDungeon();

            //开始发送积分排行榜
            StartToSendRankList();

            //造怪
            List<SceneNpcRecord> tempList;
            if (!SceneManager.SceneNpcs.TryGetValue(TypeId, out tempList))
            {
                return;
            }
            foreach (var record in tempList)
            {
                var dataId = record.DataID;
                var pos = new Vector2((float) record.PosX, (float) record.PosZ);
                var dir = new Vector2((float) Math.Cos(record.FaceDirection), (float) Math.Sin(record.FaceDirection));
                try
                {
                    var npc = CreateNpc(null, dataId, pos, dir);
                    if (null != npc)
                    {
                        npc.CanRelive = true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("InitNPC Error, SceneId={0},NpcId={1},dump={2}", record.SceneID, record.DataID, ex);
                }
            }
        }

        public override void EndDungeon()
        {
            _running = false;

            var result = new FubenResult();
            result.CompleteType = (int) eDungeonCompleteType.Success;

            //排名次
            RefreshScoreRank();

            //
            PushActionToAllPlayer(player =>
            {
                if (player != null)
                {
                    var unit = PlayerScore[player.ObjId];
                    result.Args.Clear();
                    result.Args.Add(unit.Rank);
                    result.Args.Add(unit.Damage);
                    Complete(player.ObjId, result);
                }
            });

            EnterAutoClose(10);
        }

        //当Obj进入场景前，(Obj还没有进入场景，他的Scene是空)这里只可以写场景逻辑(例如改个坐标)，不可以同步数据
        public override void OnObjBeforeEnterScene(ObjBase obj)
        {
            //变身
            if (obj.GetObjType() == ObjType.PLAYER)
            {
                var player = obj as ObjPlayer;
                player.AddBuff(1206 + player.TypeId, 1, player);
            }
        }

        public override void OnPlayerEnter(ObjPlayer player)
        {
            base.OnPlayerEnter(player);

            //恢复满血满蓝
            ResetPlayer(player);

            //初始化玩家积分
            if (!PlayerScore.ContainsKey(player.ObjId))
            {
                var unit = new DamageUnit();
                unit.CharacterId = player.ObjId;
                unit.Name = player.GetName();
                PlayerScore.Add(player.ObjId, unit);
                RankData.Data.Add(unit);
            }
        }

        public override void OnPlayerEnterOver(ObjPlayer player)
        {
            base.OnPlayerEnterOver(player);

            if (State == eDungeonState.WillStart)
            {
                var t = GetTriggerTime(eDungeonTimerType.WaitStart);
                var param = (int) (t - DateTime.Now).TotalSeconds;
                param = param << 1;
                param += 1;
                player.Proxy.NotifyBattleReminder(19, Utils.WrapDictionaryId(42000), param);
            }
        }

        public override void OnPlayerLeave(ObjPlayer player)
        {
            ResetPlayer(player);

            base.OnPlayerLeave(player);
        }

        public override void ExitDungeon(ObjPlayer player)
        {
            player.ExitDungeon();
        }

        public override void OnPlayerDie(ObjPlayer player, ulong characterId = 0)
        {
            //重生
            var reliveTime = DateTime.Now.AddSeconds(5);
            CreateTimer(reliveTime, () => { PlayerRelive(player); });

            //通知死者，复活倒计时
            player.Proxy.NotifyBattleReminder(14, Utils.WrapDictionaryId(220515), 1);
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

            //移除死亡给叠加的buff
            foreach (var buffId in DieBuffId)
            {
                var datas = casterPlayer.BuffList.GetBuffById(buffId);
                foreach (var data in datas)
                {
                    MissBuff.DoEffect(this, casterPlayer, data);
                    casterPlayer.DeleteBuff(data, eCleanBuffType.EffectOver);
                }
            }

            if (state == eDungeonState.Start)
            {
                //加积分
                var unit = PlayerScore[casterPlayer.ObjId];
                unit.Damage += 5;
            }
        }

        public override void OnNpcDie(ObjNPC npc, ulong characterId = 0)
        {
            base.OnNpcDie(npc, characterId);

            if (state >= eDungeonState.WillClose)
            {
                return;
            }

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
            int score;
            if (npc.TypeId == 58999)
            {
//箱子
                score = 2;
                casterPlayer.AddBuff(BoxBuffId[MyRandom.Random(BoxBuffId.Length)], 1, casterPlayer);
            }
            else
            {
                score = 3;
            }

            if (state == eDungeonState.Start)
            {
                //加积分
                var unit = PlayerScore[casterPlayer.ObjId];
                unit.Damage += score;
            }
        }

        #endregion

        #region 内部逻辑

        private void ResetPlayer(ObjPlayer player)
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
        }

        private void PlayerRelive(ObjPlayer player)
        {
            //随机位置重生
            player.SetPosition(RelivePos.Range());
            foreach (var buffId in DieBuffId)
            {
                player.AddBuff(buffId, 1, player);
            }
            player.Relive();
            player.AddBuff(InvincibleBuffId, 1, player);
        }

        private void StartToSendRankList()
        {
            _running = true;
            CoroutineFactory.NewCoroutine(SendRankListLoop).MoveNext();
        }

        private IEnumerator SendRankListLoop(Coroutine co)
        {
            while (_running)
            {
                SendRankList();

                yield return SceneServer.Instance.ServerControl.Wait(co, TimeSpan.FromSeconds(1));
            }
        }

        private void SendRankList()
        {
            RefreshScoreRank();

            var datas = RankData.Data;
            var imax = Math.Min(5, datas.Count);
            //通知客户端积分排行榜
            PushActionToAllPlayer(player =>
            {
                var myDamageList = new DamageList();
                for (var i = 0; i < imax; i++)
                {
                    myDamageList.TopPlayers.Add(datas[i]);
                }
                var myUnit = datas.Find(d => d.CharacterId == player.ObjId);
                if (myUnit == null)
                {
                    return;
                }
                myDamageList.Data.Add(myUnit);
                player.Proxy.NotifyDamageList(myDamageList);
            });
        }

        private void RefreshScoreRank()
        {
            var datas = RankData.Data;
            datas.Sort(DuComparer);
            var idx = 1;
            foreach (var unit in datas)
            {
                unit.Rank = idx++;
            }
        }

        #endregion
    }
}