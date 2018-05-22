#region using

using System;
using System.Collections;
using System.Collections.Generic;
using Database;
using DataContract;
using DataTable;
using Scorpion;
using Shared;

#endregion

namespace Scene
{
    //世界boss的脚本
    public class WorldBoss : MultiplayerDungeon
    {
        public const string DbKey = "WBoss:";
        private int BossMaxHp;
        private int BossSceneNpcId = -1;
        private ObjBoss mBoss;
        private int mBossLevel;
        private DamageListForServer mDamageList = new DamageListForServer();
        private readonly List<ulong> mQuitPlayerId = new List<ulong>();
        private readonly Dictionary<ulong, ObjPlayer> NewPlayers = new Dictionary<ulong, ObjPlayer>();
        private int nSendDamageListFailedCount;
        private bool Running = true;

        private int BossLevel
        {
            get { return mBossLevel; }
            set
            {
                mBossLevel = value;
                BossSceneNpcId = 900000 + BossLevel;
            }
        }

        public override void CompleteToAll(FubenResult result, int seconds = 20)
        {
            SendQuitNotify();

            var datas = mDamageList.Data;
            PushActionToAllPlayer(player =>
            {
                if (player != null)
                {
                    var myData = datas.Find(data => data.CharacterId == player.ObjId);
                    result.Args.Clear();
                    result.Args.Add(myData.Rank);
                    Complete(player.ObjId, result);
                }
            });

            EnterAutoClose(seconds);
        }

        private void CreateBoss()
        {
            if (mBoss == null && BossSceneNpcId != -1)
            {
                mBoss = CreateSceneBoss(BossSceneNpcId);
                BossMaxHp = mBoss.GetAttribute(eAttributeType.HpMax);
                StartGetDamageList();
                this.IsOnlineDamage = true;
            }
        }

        public override void DealWith(string name, object param)
        {
            if (name == "BossDie")
            {
                if (Running)
                {
                    //Running = false;
//                    mBoss.OnlineDie();
                    this.IsOnlineDamage = false;
                    //SendLastDamageList();
                }
            }
        }

        public override void EndDungeon()
        {
            Running = false;
            //时间到了，把boss变成无敌的
            mBoss.AddBuff(1011, 1, mBoss);

            base.EndDungeon();
        }

        private IEnumerator GetDamageListCoroutine(Coroutine co)
        {
            while (Running)
            {
                yield return SceneServer.Instance.ServerControl.Wait(co, TimeSpan.FromSeconds(1));

                var co1 = CoroutineFactory.NewSubroutine(SendDamageListCoroutine, co);
                if (co1.MoveNext())
                {
                    yield return co1;
                }
            }
        }

        protected void GetDbBossLevel()
        {
            CoroutineFactory.NewCoroutine(GetDbBossLevelCoroutine).MoveNext();
        }

        protected IEnumerator GetDbBossLevelCoroutine(Coroutine co)
        {
            var dbBossLevel = SceneServer.Instance.DB.Get<DBInt>(co, DataCategory.SceneWorldBoss,
                DbKey + ServerId);
            yield return dbBossLevel;
            if (dbBossLevel.Status != DataStatus.Ok)
            {
                Logger.Fatal("GetDbBossLevel get data from db faild!");
                var subco = CoroutineFactory.NewSubroutine(CloseDungeon, co);
                if (subco.MoveNext())
                {
                    yield return subco;
                }
                yield break;
            }
            if (dbBossLevel.Data != null)
            {
                BossLevel = dbBossLevel.Data.Value;
            }
            else
            {
                BossLevel = 0;
            }
            if (State == eDungeonState.Start)
            {
                CreateBoss();
            }
        }

        public override void OnCreate()
        {
            base.OnCreate();
            GetDbBossLevel();
        }
        //public override void OnNpcDamage(ObjNPC obj, int damage, ObjBase enemy)
        //{
        //    base.OnNpcDamage(obj, damage, enemy);
        //    var hpNow = mBoss.Attr.GetDataValue(eAttributeType.HpNow);
        //    if (hpNow < BossMaxHp * 0.3)
        //        return;
        //    mBoss.Attr.SetDataValue(eAttributeType.HpNow, (int)(hpNow + BossMaxHp / 100 * MyRandom.Random(10, 30)));
        //}
        public override void OnNpcDie(ObjNPC npc, ulong characterId = 0)
        {
            base.OnNpcDie(npc, characterId);

            if (State != eDungeonState.Start)
            {
                return;
            }

            if (npc.GetObjType() == ObjType.NPC)
            {
                Running = false;
                SendLastDamageList();
                //结束公告
                PushActionToAllPlayer(
                    player => { player.Proxy.NotifyBattleReminder(14, Utils.WrapDictionaryId(220445), 0); });

                var result = new FubenResult();
                result.CompleteType = (int) eDungeonCompleteType.Success;
                CompleteToAll(result);
            }
        }

        public override void OnPlayerEnter(ObjPlayer player)
        {
            base.OnPlayerEnter(player);
            NewPlayers[player.ObjId] = player;
            mQuitPlayerId.Remove(player.ObjId);
        }

        public override void OnPlayerLeave(ObjPlayer player)
        {
            //修改副本难度
            ChangeDifficulty(PlayerCount);

            if (State == eDungeonState.Start)
            {
                mQuitPlayerId.Add(player.ObjId);
            }
        }

        private IEnumerator SendDamageListCoroutine(Coroutine co)
        {
            var damageList = mBoss.CollectDamageList();
            var hpOld = mBoss.Attr.GetDataValue(eAttributeType.HpNow);
            //把新进来的人加入damageList
            var datas = damageList.Data;
            foreach (var player in NewPlayers.Values)
            {
                var unit = datas.Find(d => d.CharacterId == player.ObjId);
                if (unit != null)
                {
                    unit.Name = player.GetName();
                }
                else
                {
                    unit = new DamageUnit();
                    unit.CharacterId = player.ObjId;
                    unit.Name = player.GetName();
                    datas.Add(unit);
                }
            }
            NewPlayers.Clear();

            var notifyMsg = SceneServer.Instance.ActivityAgent.NotifyDamageList(0, ServerId, Guid, damageList);
            yield return notifyMsg.SendAndWaitUntilDone(co);
            if (notifyMsg.State != MessageState.Reply || notifyMsg.ErrorCode != (int) ErrorCodes.OK)
            {
                Logger.Error("NotifyDamageList failed in GetDamageListCoroutine()!!!");
                //如果失败了，重试一次
                if (++nSendDamageListFailedCount < 1)
                {
                    yield return SceneServer.Instance.ServerControl.Wait(co, TimeSpan.FromMilliseconds(20));
                    SendLastDamageList();
                }
                else
                {
                    nSendDamageListFailedCount = 0;
                }
                yield break;
            }
            mDamageList = notifyMsg.Response;
            //此场景中所有人的伤害列表
            var damageDatas = mDamageList.Data;
            //整场战斗的前五名
            var topPlayers = mDamageList.TopPlayers;
            //通知客户端刷新伤害列表
            PushActionToAllPlayer(player =>
            {
                var myDamageList = new DamageList();
                myDamageList.TopPlayers.AddRange(topPlayers);
                var myUnit = damageDatas.Find(d => d.CharacterId == player.ObjId);
                if (myUnit == null)
                {
                    return;
                }
                myDamageList.Data.Add(myUnit);
                myDamageList.NpcMaxHp = BossMaxHp;
                player.Proxy.NotifyDamageList(myDamageList);
            });
            //修正boss血量
            var hpNow = BossMaxHp - mDamageList.TotalDamage;
            mBoss.Attr.SetDataValue(eAttributeType.HpNow, hpNow);
            if (hpNow > hpOld)
            {
                PushActionToAllPlayer(player =>
                {
                    player.Proxy.NotifyBattleReminder(27, Utils.WrapDictionaryId(6018), 1);
                });  
            }
            //给最后一击的玩家发奖励
            if (mDamageList.LastPlayer != 0)
            {
                SendLastHitReward(mDamageList.LastPlayer);
            }
        }

        private void SendLastDamageList()
        {
            CoroutineFactory.NewCoroutine(SendDamageListCoroutine).MoveNext();
        }

        private void SendLastHitReward(ulong lastHitPlayerId)
        {
            var player = FindPlayer(lastHitPlayerId);
            if (player == null)
            {
                return;
            }

            var tbNpc = Table.GetNpcBase(mBoss.TypeId);
            if (tbNpc.DropId == -1)
            {
                return;
            }

            var dropItems = new Dictionary<int, int>();
            Drop.DropMother(tbNpc.DropId, dropItems);

            var ownerList = new List<ulong>();
            ownerList.Add(lastHitPlayerId);
            foreach (var i in dropItems)
            {
                CreateDropItem(tbNpc.BelongType, ownerList, 0, i.Key, i.Value, mBoss.GetPosition());
            }
        }

        private void SendQuitNotify()
        {
            //伤害列表
            var datas = mDamageList.Data;
            var playerCount = datas.Count;
            if (playerCount == 0)
            {
                return;
            }

            var result = new FubenResult();
            result.CompleteType = (int) eDungeonCompleteType.Quit;
            foreach (var playerId in mQuitPlayerId)
            {
                var player = FindPlayer(playerId);
                if (player == null)
                {
                    //发系统消息
                    var message = Utils.WrapDictionaryId(220445);
                    var toList = new List<ulong>();
                    toList.Add(playerId);
                    SceneServer.Instance.ChatAgent.ChatNotify(toList, (int) eChatChannel.System, 0, string.Empty,
                        new ChatMessageContent {Content = message});

                    var myData = datas.Find(data => data.CharacterId == playerId);
                    result.Args.Clear();
                    result.Args.Add(myData.Rank);
                    Complete(playerId, result);
                }
            }
        }

        public override void StartDungeon()
        {
            base.StartDungeon();

            CreateBoss();
        }

        private void StartGetDamageList()
        {
            CoroutineFactory.NewCoroutine(GetDamageListCoroutine).MoveNext();
        }
    }
}