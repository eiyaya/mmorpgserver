#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataContract;
using Scorpion;
using Mono.GameMath;
using NLog;
using Shared;

#endregion

namespace Scene
{
    public class FrozenThrone : UniformDungeon
    {
        #region 变量

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private int mSeconds;
        private Trigger mTrigger;
        private int maxLevel;
        private int LadderCount;
        private int mKillCount;
        private ObjCharacter leftTower;
        private ObjCharacter middleTower;
        private ObjCharacter rightTower;
        private ObjCharacter mainTower;
        private int RankLadder;
        private readonly Dictionary<ulong, ObjPlayer> mDropPlayers = new Dictionary<ulong, ObjPlayer>();
        private readonly Dictionary<ulong, ObjPlayer> mQuitPlayers = new Dictionary<ulong, ObjPlayer>();

        #endregion

        #region 常量

        private static readonly List<int> mIdList = new List<int>
        {
            220000,
            220001,
            220002,
            220003,
            220004,
            220005,
            220006,
            220007,
            220008,
            220009,
            220010,
            220011,
            220012,
            220013,
            220014
        };

        private static readonly List<int> mIdCount = new List<int>
        {
            10,
            10,
            10,
            10,
            1,
            10,
            10,
            10,
            10,
            1,
            10,
            10,
            10,
            10,
            1
        };

        private static readonly List<Vector2> RefreshPos = new List<Vector2>
        {
            new Vector2(9.58f, 58.92f),
            new Vector2(33.26f, 66.11f),
            new Vector2(59.15f, 57.74f)
        };

        #endregion

        #region 重写父类函数

        public override void OnCreate()
        {
            base.OnCreate();

            mSeconds = 0;
            mFubenInfoMsg.Units[0].Params[0] = 20;
            mTrigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(1), TimeTick, 1000);

            var startTime = DateTime.Now.AddSeconds(20);
            StartTimer(eDungeonTimerType.WaitStart, startTime, TimeOverStart);
            StartTimer(eDungeonTimerType.WaitEnd, startTime.AddSeconds(600), TimeOverEnd);

            foreach (var pair in mObjDict)
            {
                var t = pair.Value as ObjNPC;
                if (t == null)
                {
                    continue;
                }
                if (t.IsDead() || !t.Active)
                {
                    continue;
                }
                if (t.TypeId == 220020)
                {
                    leftTower = t;
                    t.OnDamageCallback = LeftOndamage;
                }
                else if (t.TypeId == 220021)
                {
                    middleTower = t;
                    t.OnDamageCallback = MiddleOndamage;
                }
                else if (t.TypeId == 220022)
                {
                    rightTower = t;
                    t.OnDamageCallback = RightOndamage;
                }
                else if (t.TypeId == 220023)
                {
                    mainTower = t;
                    t.OnDamageCallback = MainTowerOndamage;
                }
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (mTrigger != null)
            {
                SceneServerControl.Timer.DeleteTrigger(mTrigger);
                mTrigger = null;
            }
        }

        public override void OnNpcDie(ObjNPC npc, ulong characterId = 0)
        {
            if (State > eDungeonState.Start)
            {
                return;
            }

            if (npc.TypeId == 220023)
            {
                //失败了
                Onlost();
            }
            else if (npc.TypeId < 220020)
            {
                mKillCount++;
                if (mKillCount >= 369)
                {
                    //胜利了
                    OnWin();
                }
            }
        }

        public override void OnPlayerEnter(ObjPlayer player)
        {
            base.OnPlayerEnter(player);
            this.addExp.Clear();
            ResetMonsterLevel();
            if (State == eDungeonState.Start)
            {
                mDropPlayers.Remove(player.ObjId);
            }
        }

        public override void OnPlayerLeave(ObjPlayer player)
        {
            if (State == eDungeonState.Start)
            {
                if (!mQuitPlayers.ContainsKey(player.ObjId))
                {
                    mDropPlayers.Add(player.ObjId, player);
                }
            }
            base.OnPlayerLeave(player);
        }

        public override void ExitDungeon(ObjPlayer player)
        {
            if (State <= eDungeonState.Start)
            {
                mQuitPlayers.Add(player.ObjId, player);
            }
        }

        public override void CompleteToAll(FubenResult result, int seconds = 20)
        {
            var args = result.Args;
            args.Add(RankLadder);

            SendFubenInfo();
            DealWithQuitPlayers(result);

            base.CompleteToAll(result, seconds);
        }

        #endregion

        #region 内部逻辑

        private void DealWithQuitPlayers(FubenResult result)
        {
            Log(Logger, "DealWithQuitPlayers");

            foreach (var player in mDropPlayers)
            {
                result.SceneAddExp = 0;
                for (int i = 0; i < this.addExp.Count; i++)
                {
                    if (this.addExp[i].characterId == player.Value.ObjId)
                    {
                        result.SceneAddExp += (ulong)this.addExp[i].exp;
                    }
                }

                Complete(player.Value.ObjId, result);
            }
            mDropPlayers.Clear();
            mQuitPlayers.Clear();
        }

        //通知点事情
        private IEnumerator NoditySome(Coroutine coroutine, string content)
        {
            if (mPlayerDict.Count == 0)
            {
                yield break;
            }
            var fromCharId = mPlayerDict.Keys.First();
            var t = mPlayerDict.Keys.ToList();
            SceneServer.Instance.ChatAgent.ChatNotify(t, (int) eChatChannel.System, fromCharId, string.Empty,
                new ChatMessageContent {Content = content});
        }

        //失败了
        private void Onlost()
        {
            var result = new FubenResult();
            result.CompleteType = (int)eDungeonCompleteType.Failed;
            CompleteToAll(result);
            //失败了
          //  var str = Utils.WrapDictionaryId(301030);
          //  CoroutineFactory.NewCoroutine(NoditySome, str).MoveNext();
            SendFubenInfo();
            EnterAutoClose();
        }

        //胜利了
        private void OnWin()
        {
            if (State > eDungeonState.Start)
            {
                return;
            }
            //胜利了
         //   var str = Utils.WrapDictionaryId(301031);
         //   CoroutineFactory.NewCoroutine(NoditySome, str).MoveNext();
            var nowHp = mainTower.GetAttribute(eAttributeType.HpNow);
            var maxHp = mainTower.GetAttribute(eAttributeType.HpMax);
            if (nowHp > maxHp*90/100)
            {
                RankLadder = 3;
            }
            else if (nowHp > maxHp*60/100)
            {
                RankLadder = 2;
            }
            else
            {
                RankLadder = 1;
            }

            var result = new FubenResult();
            result.CompleteType = (int) eDungeonCompleteType.Success;
            CompleteToAll(result);
        }

        private void TimeTick()
        {
            if (mPlayerDict.Count == 0)
            {
                return;
            }
            mSeconds = mSeconds + 1;
            var diff = mSeconds - 20;
            if (diff < 0)
            {
                return;
            }
            var residue = diff%20;
            if (residue == 0)
            {
                NextLadder();
            }
        }

        private void LeftOndamage(ObjNPC npc, ObjCharacter caster, int damage)
        {
            mIsFubenInfoDirty = true;
            var unit = mFubenInfoMsg.Units[1];
            unit.Params[0] = npc.GetAttribute(eAttributeType.HpNow)*100/npc.GetAttribute(eAttributeType.HpMax);
        }

        private void MiddleOndamage(ObjNPC npc, ObjCharacter caster, int damage)
        {
            mIsFubenInfoDirty = true;
            var unit = mFubenInfoMsg.Units[1];
            unit.Params[1] = npc.GetAttribute(eAttributeType.HpNow)*100/npc.GetAttribute(eAttributeType.HpMax);
        }

        private void RightOndamage(ObjNPC npc, ObjCharacter caster, int damage)
        {
            mIsFubenInfoDirty = true;
            var unit = mFubenInfoMsg.Units[1];
            unit.Params[2] = npc.GetAttribute(eAttributeType.HpNow)*100/npc.GetAttribute(eAttributeType.HpMax);
        }

        private void MainTowerOndamage(ObjNPC npc, ObjCharacter caster, int damage)
        {
            mIsFubenInfoDirty = true;
            var unit2 = mFubenInfoMsg.Units[2];
            if (npc.IsDead() || !npc.Active)
            {
                unit2.Params[0] = 0;
            }
            else
            {
                unit2.Params[0] = npc.GetAttribute(eAttributeType.HpNow)*100/npc.GetAttribute(eAttributeType.HpMax);
            }
        }

        private void ResetTowerHp()
        {
            mIsFubenInfoDirty = true;
            var unit = mFubenInfoMsg.Units[1];
            if (leftTower.IsDead() || !leftTower.Active)
            {
                unit.Params[0] = 0;
            }
            else
            {
                unit.Params[0] = leftTower.GetAttribute(eAttributeType.HpNow)*100/
                                 leftTower.GetAttribute(eAttributeType.HpMax);
            }

            if (middleTower.IsDead() || !middleTower.Active)
            {
                unit.Params[1] = 0;
            }
            else
            {
                unit.Params[1] = middleTower.GetAttribute(eAttributeType.HpNow)*100/
                                 middleTower.GetAttribute(eAttributeType.HpMax);
            }

            if (rightTower.IsDead() || !rightTower.Active)
            {
                unit.Params[2] = 0;
            }
            else
            {
                unit.Params[2] = rightTower.GetAttribute(eAttributeType.HpNow)*100/
                                 rightTower.GetAttribute(eAttributeType.HpMax);
            }

            var unit2 = mFubenInfoMsg.Units[2];
            if (mainTower.IsDead() || !mainTower.Active)
            {
                unit2.Params[0] = 0;
            }
            else
            {
                unit2.Params[0] = mainTower.GetAttribute(eAttributeType.HpNow)*100/
                                  mainTower.GetAttribute(eAttributeType.HpMax);
            }
        }

        //重新计算怪物最大等级
        private void ResetMonsterLevel()
        {
            var totles2 = 0;
            var maxNow = 0;
            var players = mPlayerDict.Values;
            foreach (var player in players)
            {
                var l = player.GetLevel();
                totles2 += l*l;
            }
            maxNow = (int) Math.Sqrt(totles2/mPlayerDict.Count);

            if (maxLevel != maxNow)
            {
                maxLevel = maxNow;
                leftTower.SetToLevel(maxLevel);
                middleTower.SetToLevel(maxLevel);
                rightTower.SetToLevel(maxLevel);
                mainTower.SetToLevel(maxLevel);
            }
        }

        private void NextLadder()
        {
            if (LadderCount >= 15)
            {
                //结束了，不再刷新怪物了
                SceneServerControl.Timer.DeleteTrigger(mTrigger);
                mTrigger = null;
                return;
            }
            EnterNextPhase();
            ResetTowerHp();

            var mId = mIdList[LadderCount];
            var mCount = mIdCount[LadderCount];
            var i = 0;
            StartTimer(eDungeonTimerType.CreateMonster, DateTime.Now.AddSeconds(0.5f), () =>
            {
                if (i == mCount - 1)
                {
                    EnterNextPhase();
                    ResetTowerHp();
                    CloseTimer(eDungeonTimerType.CreateMonster);
                    mFubenInfoMsg.Units[0].Params[0] = (20 - (mSeconds%20));
                }
                foreach (var vector in RefreshPos)
                {
                    CreateNpc(mId, vector);
                }
                ++i;
            }, 1500);
            LadderCount = LadderCount + 1;
        }

        //造Npc
        private void CreateNpc(int dataId, Vector2 pos)
        {
            var npc = CreateNpc(null, dataId, pos, Vector2.UnitX, "", maxLevel);
            if (null != npc)
            {
                npc.CanRelive = false;
                foreach (var i in npc.TableCharacter.InitSkill)
                {
                    npc.Skill.ResetSkill(i, maxLevel);
                }
                npc.Attr.InitAttributesAll();
            }
        }

        #endregion
        public override void OnPlayerPickUp(ulong objId, int itemId, int count)
        {
            DungeonSceneExpItem item = new DungeonSceneExpItem();
            item.characterId = objId;
            item.exp = count;

            this.addExp.Add(item);
        }
    }
}