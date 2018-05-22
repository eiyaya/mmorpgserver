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
using DataTable;
using System.Diagnostics;
#endregion

namespace Scene
{
    public class MieShiWar : UniformDungeon
    {
        private class PlayerBattleInfo
        {//统计的玩家战斗信息
            public int Contribution = 0;   //贡献
            public int KillCount = 0;
            public float _Rate = 0.0f;
            public bool isEverInBattle = false;
            public string name = "";
            public Dictionary<int, int> PickList = new Dictionary<int, int>();
        }
        private Dictionary<ulong, PlayerBattleInfo> dicBattleInfo = new Dictionary<ulong, PlayerBattleInfo>();
        #region 变量

        //下列参数为默认初始值,oncreate中会根据副本id重置
        public int MaxLadderCount = 4;
        public int ActivityId = 1;   //当前灭世活动ID
        public int FubenId = 6110;   //当前灭世副本ID
        public int NPCTower1 = 226000;   //炮台1
        public int NPCTower2 = 226001;   //炮台2
        public int NPCTower3 = 226002;   //炮台3
        public int NPCTower4 = 226003;   //炮台4
        public int NPCTower5 = 226004;   //炮台5
        public int NPCTower6 = 226005;   //炮台6
        public int NPCChancel = 226018;  //圣坛
        public int TowerCount = 0;
        private MieShiRecord mTbMieShiRecord;
        public Dictionary<ulong, DamageUnit> mPointList = new Dictionary<ulong, DamageUnit>();   //玩家伤害对应的积分排名列表
        public Dictionary<ulong, int> mBatteryPointList = new Dictionary<ulong, int>();   //炮台伤害对应的积分列表
        public int nBatteryTotalPoint;  //所有炮台总积分
        public DamageUnitComparer DuComparer = new DamageUnitComparer();  //积分排行比较函数
        public MieShiSceneData SyncData = new MieShiSceneData();  //需要同步给Activity服务器的数据

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private DateTime mToMobTime;  //下一次怪物刷新时间
        private int nCurRandWaveIdx;  //当前随即怪刷新波次
        private List<DateTime> mRandomMonsterTimerList = new List<DateTime>();
        private Trigger mTrigger;
        private Trigger mFubenInfoTrigger;  //副本信息同步定时器
        private Trigger mChanceDamageTrigger;  //圣坛伤害定时器
        private Trigger sceneNpcPosTrigger;  //
        private SceneNpcPosList sceneNpcPosList = new SceneNpcPosList();
        //private int nBaseWorldLevel = 1;    //世界等级
        //private int Level = 0;
        private int nCurLadderCount;     //当前已刷新波数
        private int nMobMonsterCount;    //已刷新怪物的总数量
        private int nCurKillCount;       //当前已击杀怪物的数量
        private int nSendPointListFailedCount; //同步数据失败次数
        private bool Running = false;
        private bool HadPlayCG;
        private int KillBoss;
        private Dictionary<ulong, bool> dicBoss = new Dictionary<ulong, bool>();
        private int nFubenResult = -1;
        private bool ChanceDamagePrompt;
        private bool isInitContributePointList = false;

        private int nPlayerCount;    //玩家的个数
        private Dictionary<ulong, int> mPosIndexDic;    //当前位置下标
        private List<ObjNPC> TowerObjList = new List<ObjNPC>();
        private Dictionary<int, int> DamagePromtCount = new Dictionary<int, int>();
        private ObjCharacter mainTower;
        private CommonActivityInfo mActivityInfo;
        private readonly Dictionary<ulong, ObjPlayer> mDropPlayers = new Dictionary<ulong, ObjPlayer>();
        //private readonly Dictionary<ulong, ObjPlayer> mQuitPlayers = new Dictionary<ulong, ObjPlayer>();

        private List<int> NpcIndexList = new List<int>();
        private List<int> CountList = new List<int>();
        private List<int> IterList = new List<int>();
        private List<bool> FinishList = new List<bool>();

        #endregion

        #region 常量

        private static readonly List<Vector4> RefreshArea = new List<Vector4>()
        {
            new Vector4(15.0f, 45.0f, 20.0f, 40.0f),
            new Vector4(64.0f, 86.0f, 13.0f, 37.0f),
            new Vector4(89.0f, 138.0f, 61.0f, 78.0f),
            new Vector4(100.0f, 145.0f, 94.0f, 114.0f),
            new Vector4(100.0f, 126.0f, 130.0f, 160.0f),
            new Vector4(20.0f, 39.0f, 141.0f, 164.0f),
            new Vector4(55.0f, 75.0f, 153.0f, 179.0f)
        };

        private static readonly List<int> SinglePoint = new List<int>()
        {
            5,
            7,
            10
        };

        private static readonly List<int> HpPercent = new List<int>()
        {
            80,
            60,
            40,
            20,
            0
        };

        #endregion

        #region 重写父类函数

        public override void OnCreate()
        {

            base.OnCreate();
            var tbFuben = Table.GetFuben(TableSceneData.FubenId);
            do
            {
                MaxLadderCount = 0;
                FubenId = TableSceneData.FubenId;   //当前灭世副本ID

                if (tbFuben != null)
                {
                    if (tbFuben.lParam1.Count != 7 || tbFuben.iParam1 <= 0)
                    {
#if DEBUG
                        Debug.Assert(false, "table_fuben_lParam1_count!=7 || table_fuben_iParam1<=0");
#endif
                        break;
                    }
                    ActivityId = tbFuben.iParam1;

                    NPCChancel = tbFuben.lParam1[0];
                    NPCTower1 = tbFuben.lParam1[1];   //炮台1
                    NPCTower2 = tbFuben.lParam1[2];   //炮台2
                    NPCTower3 = tbFuben.lParam1[3];   //炮台3
                    NPCTower4 = tbFuben.lParam1[4];   //炮台4
                    NPCTower5 = tbFuben.lParam1[5];   //炮台5
                    NPCTower6 = tbFuben.lParam1[6];   //炮台6

                    TowerCount = tbFuben.lParam1.Count - 1;

                    var tbMieshi = Table.GetMieShi(ActivityId);
                    if (tbMieshi != null)
                    {
                        if (tbMieshi.Monster1IdList.Count > 0)
                        {
                            MaxLadderCount++;
                        }
                        if (tbMieshi.Monster2IdList.Count > 0)
                        {
                            MaxLadderCount++;
                        }
                        if (tbMieshi.Monster3IdList.Count > 0)
                        {
                            MaxLadderCount++;
                        }
                        if (tbMieshi.Monster4IdList.Count > 0)
                        {
                            MaxLadderCount++;
                        }
                    }
                }
            } while (false);

            dicBattleInfo.Clear();
            //初始化数据
            mPointList.Clear();
            mBatteryPointList.Clear();
            nBatteryTotalPoint = 0;
            nCurLadderCount = nCurKillCount = nMobMonsterCount = 0;
            Running = false;
            HadPlayCG = false;
            KillBoss = 0;
            dicBoss.Clear();
            mPosIndexDic = new Dictionary<ulong, int>();
            mTbMieShiRecord = Table.GetMieShi(ActivityId);
            mTrigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(1), TimeTick, 1000);
            sceneNpcPosTrigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(1), UpdateSceneNpcPos, 1000);
            var firstMobTime = tbFuben.OpenLastMinutes * 60;
            //Todo 修改测试数据
            var startTime = DateTime.Now.AddSeconds(firstMobTime);
            mToMobTime = startTime;
            mFubenInfoMsg.Units[0].Params[0] = firstMobTime;
            mFubenInfoMsg.Units[3].Params[0] = firstMobTime;
            StartTimer(eDungeonTimerType.WaitStart, startTime, StartFuben);
            StartTimer(eDungeonTimerType.WaitEnd, startTime.AddSeconds(tbFuben.TimeLimitMinutes * 60), EndFuben);
            //开始定时同步副本信息
            mFubenInfoTrigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now, RefreshFubenInfo, 5000);

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
                if (t.TypeId == NPCTower1)
                {
                    TowerObjList.Add(t);
                    t.OnDamageCallback = Tower1Ondamage;
                }
                else if (t.TypeId == NPCTower2)
                {
                    TowerObjList.Add(t);
                    t.OnDamageCallback = Tower2Ondamage;
                }
                else if (t.TypeId == NPCTower3)
                {
                    TowerObjList.Add(t);
                    t.OnDamageCallback = Tower3Ondamage;
                }
                else if (t.TypeId == NPCTower4)
                {
                    TowerObjList.Add(t);
                    t.OnDamageCallback = Tower4Ondamage;
                }
                else if (t.TypeId == NPCTower5)
                {
                    TowerObjList.Add(t);
                    t.OnDamageCallback = Tower5Ondamage;
                }
                else if (t.TypeId == NPCTower6)
                {
                    TowerObjList.Add(t);
                    t.OnDamageCallback = Tower6Ondamage;
                }
                else if (t.TypeId == NPCChancel)
                {
                    mainTower = t;
                    t.OnDamageCallback = MainTowerOndamage;
                }
            }
            //初始化炮台、圣坛血量信息
            ResetTowerHp();

            //同步炮台的guid给Activity服务器
            StartSyncBatteryGuid();
            while (mFubenInfoMsg.Units.Count < 4)
            {
                mFubenInfoMsg.Units.Add(new FubenInfoUnit());
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

            if (mFubenInfoTrigger != null)
            {
                SceneServerControl.Timer.DeleteTrigger(mFubenInfoTrigger);
                mFubenInfoTrigger = null;
            }

            if (mChanceDamageTrigger != null)
            {
                SceneServerControl.Timer.DeleteTrigger(mChanceDamageTrigger);
                mChanceDamageTrigger = null;
            }

            if (sceneNpcPosTrigger != null)
            {
                SceneServerControl.Timer.DeleteTrigger(sceneNpcPosTrigger);
                sceneNpcPosTrigger = null;
            }
            NpcIndexList = null;
            CountList = null;
            IterList = null;
            FinishList = null;
        }
        public override void StartDungeon()
        {
            base.StartDungeon();

            RemoveObj(999);
        }

        public override void OnNpcDamage(ObjNPC obj, int damage, ObjBase enemy)
        {


            var npcId = obj.mTypeId;
            var tbNpcBase = Table.GetNpcBase(npcId);

            if (ChanceDamagePrompt && npcId == NPCChancel)
            {
                ChanceDamagePrompt = false;
                var args = new List<string>();
                args.Add(tbNpcBase.Name);
                var content = Utils.WrapDictionaryId(300000108, args);
                PushActionToAllPlayer(p => { p.Proxy.NotifyBattleReminder(19, content, 0); });

                var extInt = new List<int>();
                extInt.Add(FubenId);
                extInt.Add((int)obj.GetPosition().X * 100);
                extInt.Add((int)obj.GetPosition().Y * 100);

                content = Utils.WrapPositionDictionaryId(300000108, args, extInt);
                SceneServer.Instance.ChatAgent.BroadcastWorldMessage((uint)this.ServerId,
                (int)eChatChannel.Help, 0,
                string.Empty, new ChatMessageContent { Content = content });
            }

            if (tbNpcBase == null || tbNpcBase.NpcType < 0 || tbNpcBase.NpcType > 2)
            {
                return;  //非需要统计的类型
            }

            if (npcId == NPCTower1 || npcId == NPCTower2 ||
                npcId == NPCTower3 || npcId == NPCTower4 ||
                npcId == NPCTower5 || npcId == NPCTower6 ||
                npcId == NPCChancel)
            {

                return;  //炮台和圣坛收到的伤害不统计
            }

            int singleDamage = GetSingleDamage(tbNpcBase.NpcType);
            if (singleDamage == 0)
                return;



            var lv = mTbMieShiRecord.ScoreLevel;
            var nRate = Table.GetSkillUpgrading(130110).GetSkillUpgradingValue(lv);

            var fRate = (float)nRate / 100.0f;
            //计算伤害对应的积分
            int point = (int)((double)damage / (double)singleDamage * (double)SinglePoint[tbNpcBase.NpcType] / fRate);
            if (enemy.GetObjType() == ObjType.PLAYER)
            {
                DamageUnit damageUnit = mPointList.GetValue(enemy.ObjId);
                if (damageUnit == null)
                {
                    damageUnit = new DamageUnit();
                    damageUnit.CharacterId = enemy.ObjId;
                    damageUnit.Damage = point;
                    damageUnit.Name = ((ObjCharacter)enemy).mName;
                    damageUnit.Rank = 0;
                    mPointList.Add(enemy.ObjId, damageUnit);
                }
                else
                {
                    damageUnit.Damage += point;
                }
                //广播积分超越历时最佳
                if (mActivityInfo != null && mActivityInfo.lastBestInfo != null && damageUnit.Damage > mActivityInfo.lastBestInfo.value)
                {
                    var args = new List<string>();
                    args.Add(Utils.AddCharacter(damageUnit.CharacterId, damageUnit.Name));
                    args.Add(Utils.AddCharacter(mActivityInfo.lastBestInfo.characterId, mActivityInfo.lastBestInfo.name));
                    var content = Utils.WrapDictionaryId(300000080, args);
                    SceneServer.Instance.ChatAgent.BroadcastWorldMessage((uint)this.ServerId,
                        (int)eChatChannel.SystemScroll, 0,
                        string.Empty, new ChatMessageContent { Content = content });
                }
            }
            else if (enemy.mTypeId == NPCTower1 || enemy.mTypeId == NPCTower2 ||
                     enemy.mTypeId == NPCTower3 || enemy.mTypeId == NPCTower4 ||
                     enemy.mTypeId == NPCTower5 || enemy.mTypeId == NPCTower6)
            {
                int curPoint;
                if (!mBatteryPointList.TryGetValue(enemy.ObjId, out curPoint))
                {
                    mBatteryPointList.Add(enemy.ObjId, point);
                }
                else
                {
                    curPoint += point;
                    mBatteryPointList.Remove(enemy.ObjId);
                    mBatteryPointList.Add(enemy.ObjId, curPoint);
                }

                //优先初始化进入战场的带贡献值的玩家
                if (!isInitContributePointList)
                    InitContributePointList();

                AddBatteryPoint(point);
            }
        }

        void InitContributePointList()
        {
            isInitContributePointList = true;
            foreach (var player in dicBattleInfo)
            {
                if (player.Value.isEverInBattle)
                {
                    DamageUnit damageUnit = mPointList.GetValue(player.Key);
                    if (damageUnit == null)
                    {
                        damageUnit = new DamageUnit();
                        damageUnit.CharacterId = player.Key;
                        damageUnit.Damage = 0;
                        damageUnit.Name = player.Value.name;
                        damageUnit.Rank = 0;
                        mPointList.Add(player.Key, damageUnit);
                    }
                }
            }
        }
        private void AddBatteryPoint(int point)
        {
            nBatteryTotalPoint += point;

            foreach (var item in mPointList)
            {
                item.Value.Damage += (int)(item.Value.Rate * point);
            }
        }
        public override void OnNpcRespawn(ObjNPC npc)
        {
            if (null != npc)
            {
                nMobMonsterCount++;
            }

        }
        public override void OnNpcDie(ObjNPC npc, ulong characterId = 0)
        {
            if (State > eDungeonState.Start || Running == false)
            {
                return;
            }
            if (characterId > 0)
            {
                PlayerKillNpc(characterId);
            }
            if (npc.TypeId == NPCTower1 || npc.TypeId == NPCTower2 ||
                npc.TypeId == NPCTower3 || npc.TypeId == NPCTower4 ||
                npc.TypeId == NPCTower5 || npc.TypeId == NPCTower6)
            {
                //全服广播炮台被摧毁
                var tbNpcBase = Table.GetNpcBase(npc.TypeId);
                if (tbNpcBase != null)
                {
                    var args = new List<string>();
                    args.Add(Utils.GetPositionById(tbNpcBase.Id) + tbNpcBase.Name);
                    args.Add(Utils.GetPositionById(tbNpcBase.Id));
                    //tbNpcBase.Id = 
                    var content = Utils.WrapDictionaryId(300000091, args);
                    SceneServer.Instance.ChatAgent.BroadcastWorldMessage((uint)this.ServerId,
                        (int)eChatChannel.SystemScroll, 0,
                        string.Empty, new ChatMessageContent { Content = content });
                }
                //保存炮台摧毁的数据
                SaveBatteryDestroyData(npc.ObjId);
                TowerCount--;
            }
            else if (npc.TypeId == NPCChancel)
            {
                //失败了
                mainTower = null;
                Onlost();
            }
            else if (npc.GetCamp() == 2)
            {
                nCurKillCount++;
                //掉落BOSS宝箱
                if (dicBoss.ContainsKey(npc.mObjId))
                {
                    //全服广播击杀Boss
                    var damageUnit = mPointList.GetValue(characterId);
                    if (damageUnit != null)
                    {
                        var args = new List<string>();
                        args.Add(Utils.AddCharacter(characterId, damageUnit.Name));
                        args.Add(npc.GetName());
                        var content = Utils.WrapDictionaryId(300000078, args);
                        SceneServer.Instance.ChatAgent.BroadcastWorldMessage((uint)this.ServerId,
                            (int)eChatChannel.SystemScroll, 0,
                            string.Empty, new ChatMessageContent { Content = content });
                        dicBoss.Remove(npc.mObjId);
                        if (dicBoss.Count() == 0 && nCurLadderCount == MaxLadderCount)
                        {//Boss死完了
                            OnWin();
                            KillBoss = 1;
                            nCurKillCount = nMobMonsterCount;
                        }
                    }

                    //通过活动服务器Boss宝箱可以领取了
                    var tbMieShi = Table.GetMieShi(ActivityId);
                    StartSyncBossDrop(tbMieShi.BossDropBoxId);
                    CreateNpc(tbMieShi.BossDropBoxId, npc.mPosition);
                }
                else
                {//判断时间决定是否刷新
                    if (Running == true && nCurLadderCount < MaxLadderCount)
                        npc.CanRelive = DateTime.Now < npc.ReliveTimer;
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
        }

        public override void OnPlayerEnter(ObjPlayer player)
        {
            base.OnPlayerEnter(player);
            if (State >= eDungeonState.Start)
            {
                mDropPlayers.Remove(player.ObjId);
            }
            nPlayerCount++;
            AddToInfo(player.ObjId, player.mName, true);
        }

        public override void OnPlayerLeave(ObjPlayer player)
        {
            nPlayerCount--;
            if (State == eDungeonState.Start)
            {
                if (!mDropPlayers.ContainsKey(player.ObjId))
                {
                    mDropPlayers.Add(player.ObjId, player);
                }
            }
            base.OnPlayerLeave(player);
            if (nPlayerCount <= 0 && State == eDungeonState.WillClose)
            {
                CoroutineFactory.NewCoroutine(SyncActivityAllPlayerExit).MoveNext();
            }
        }

        private IEnumerator SyncActivityAllPlayerExit(Coroutine co)
        {
            var msg = SceneServer.Instance.ActivityAgent.SyncActivityAllPlayerExit(0, ServerId, ActivityId);
            yield return msg.SendAndWaitUntilDone(co);
            if (msg.State != MessageState.Reply || msg.ErrorCode != (int)ErrorCodes.OK)
            {
                Logger.Error("SyncActivityAllPlayerExit failed in SyncActivityAllPlayerExit()!!!");
                yield break;
            }
        }

        public override void ExitDungeon(ObjPlayer player)
        {
            //if (State <= eDungeonState.Start)
            //{
            //    if (!mQuitPlayers.ContainsKey(player.ObjId))
            //    {
            //        mQuitPlayers.Add(player.ObjId, player);
            //    }
            //}
        }

        #endregion

        #region 内部逻辑
        private void InitRandomMonster()
        {
            mRandomMonsterTimerList.Clear();
            nCurRandWaveIdx = 0;

            var tbMieShi = Table.GetMieShi(ActivityId);
            if (tbMieShi == null) return;
            DateTime dt;

            if (tbMieShi.Monst1TimeMin > 0)
            {//1
                int nRand = MyRandom.Random(tbMieShi.Monst1TimeMin, tbMieShi.Monst1TimeMax);
                dt = DateTime.Now.AddSeconds(nRand);
                mRandomMonsterTimerList.Add(dt);
            }
            if (tbMieShi.Monst2TimeMin > 0)
            {//2
                int nRand = MyRandom.Random(tbMieShi.Monst2TimeMin, tbMieShi.Monst2TimeMax);
                dt = DateTime.Now.AddSeconds(nRand);
                mRandomMonsterTimerList.Add(dt);
            }
            if (tbMieShi.Monst3TimeMin > 0)
            {//3
                int nRand = MyRandom.Random(tbMieShi.Monst3TimeMin, tbMieShi.Monst3TimeMax);
                dt = DateTime.Now.AddSeconds(nRand);
                mRandomMonsterTimerList.Add(dt);
            }
        }
        private void CreateRandomNpc()
        {
            if (ids.Count == 0 || nums.Count == 0 || Running == false)
                return;

            var tbSceneNpc = Table.GetSceneNpc(ids[0]);
            if (tbSceneNpc == null)
                return;
            var tbNpcBase = Table.GetNpcBase(tbSceneNpc.DataID);
            if (tbNpcBase == null)
                return;
            for (int j = 0; j < nums[0]; j++)
            {
                int randPos = MyRandom.Random(tbSceneNpc.RandomStartID, tbSceneNpc.RandomEndID);
                var tbRandomPos = Table.GetRandomCoordinate(randPos);
                if (null == tbRandomPos)
                    return;
                var x = MyRandom.Random(tbRandomPos.PosX - 5, tbRandomPos.PosX + 5);
                var y = MyRandom.Random(tbRandomPos.PosY - 5, tbRandomPos.PosY + 5);
                var vec = new Vector2(x, y);
                if (!ValidPosition(vec))
                {
                    var tmp = FindNearestValidPosition(vec, 10);
                    if (null != tmp)
                    {
                        vec = tmp.Value;
                        CreateNpc(tbNpcBase.Id, vec);
                    }
                    else
                    {
                        Logger.Warn("Npc postion is invalid. DataId={0},pos={1},scene={2}", tbNpcBase.Id, vec, TypeId);
                    }
                }
            }
            //Logger.Warn("----------------------------------Random Npc ----------------. id={0},num={1}", ids[0],nums[0]);

            if (ids.Count > 0 && nums.Count > 0)
            {
                ids.RemoveAt(0);
                nums.RemoveAt(0);

                StartTimer(eDungeonTimerType.CreateRandomMonster, DateTime.Now.AddSeconds(0.5f), () => CreateRandomNpc());
            }
        }
        List<int> ids = new List<int>();
        List<int> nums = new List<int>();
        private void CheckRandomMonster()
        {
            ids.Clear();
            nums.Clear();
            if (nCurRandWaveIdx > 2 || mRandomMonsterTimerList.Count == 0)
                return;
            var tbMieShi = Table.GetMieShi(ActivityId);


            switch (nCurRandWaveIdx)
            {
                case 0:
                    ids = tbMieShi.Monst1RandomId;
                    nums = tbMieShi.Monst1Num;
                    break;
                case 1:
                    ids = tbMieShi.Monst2RandomId;
                    nums = tbMieShi.Monst2Num;
                    break;
                case 2:
                    ids = tbMieShi.Monst3RandomId;
                    nums = tbMieShi.Monst3Num;
                    break;
            }

            {//刷怪

                StartTimer(eDungeonTimerType.CreateRandomMonster, DateTime.Now.AddSeconds(0.5f), () => CreateRandomNpc());
            }
        }
        public void StartFuben()
        {
            base.TimeOverStart();
            Running = true;
            //开始同步数据到Activity
            StartSyncPointList();
            //开始给客户端发送数据
            StartNotifyPointList();


            //随即怪刷新逻辑初始化
            InitRandomMonster();

            mChanceDamageTrigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now, SetChanceDamagePrompt, 30000);
        }

        public void EndFuben()
        {
            try
            {
                base.TimeOverEnd();
                Running = false;
                isInitContributePointList = false;
                nFubenResult = (int)eDungeonCompleteType.Quit;
                //结束副本信息同步
                if (mFubenInfoTrigger != null)
                {
                    SceneServerControl.Timer.DeleteTrigger(mFubenInfoTrigger);
                    mFubenInfoTrigger = null;
                }
            }
            finally
            {
                Onlost();
            }
        }

        public void CompleteToAll(int resultType, int seconds = 1800)
        {
            SendFubenInfo();

            DealWithQuitPlayers(resultType);

            if (DamagePromtCount != null)
            {
                DamagePromtCount.Clear();
            }

            PushActionToAllPlayer(player =>
            {
                if (player != null)
                {
                    var result = new FubenResult();
                    result.CompleteType = resultType;
                    var myUnit = mPointList.GetValue(player.ObjId);
                    if (myUnit != null)
                    {
                        result.Args.Add(myUnit.Rank);
                        result.Args.Add(myUnit.Damage);
                    }
                    else
                    {
                        result.Args.Add(mPointList.Count + 1);
                        result.Args.Add(0);
                    }
                    if (resultType == (int)eDungeonCompleteType.Success)
                    {
                        if (KillBoss > 0)
                        {
                            result.Args.Add(2);
                        }
                        else
                        {
                            result.Args.Add(1);
                        }
                    }
                    else
                    {
                        result.Args.Add(0);
                    }
                    result.Args.Add(CalculateLiveTowerCount());
                    result.ActivityId = ActivityId;
                    Complete(player.ObjId, result);
                }
            });
            EnterAutoClose(seconds);
        }
        private int CalculateLiveTowerCount()
        {
            int count = 0;
            for (int i = 0; i < TowerObjList.Count; i++)
            {
                var tower = TowerObjList[i];
                if (tower.IsDead() || !tower.Active)
                {
                    continue;
                }
                count++;
            }
            return count;
        }

        private FubenResult GetFubenResult(ulong charId, int resultType)
        {
            var result = new FubenResult();
            result.CompleteType = resultType;
            var myUnit = mPointList.GetValue(charId);
            if (myUnit != null)
            {
                result.Args.Add(myUnit.Rank);
                result.Args.Add(myUnit.Damage);
            }
            else
            {
                result.Args.Add(mPointList.Count + 1);
                result.Args.Add(0);
            }
            if (resultType == (int)eDungeonCompleteType.Success)
            {
                if (KillBoss > 0)
                {
                    result.Args.Add(2);
                }
                else
                {
                    result.Args.Add(1);
                }
            }
            else
            {
                result.Args.Add(0);
            }
            result.Args.Add(CalculateLiveTowerCount());
            result.ActivityId = ActivityId;
            return result;
        }

        private void DealWithQuitPlayers(int resultType)
        {
            Log(Logger, "DealWithQuitPlayers");

            foreach (var player in mDropPlayers)
            {
                var result = GetFubenResult(player.Key, resultType);
                result.ActivityId = ActivityId;
                Complete(player.Key, result);
            }
            mDropPlayers.Clear();
            //mQuitPlayers.Clear();
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
            SceneServer.Instance.ChatAgent.ChatNotify(t, (int)eChatChannel.System, fromCharId, string.Empty,
                new ChatMessageContent { Content = content });
        }

        //失败了
        private void Onlost()
        {
            Running = false;
            KillAllMonster();
            
            nFubenResult = (int)eDungeonCompleteType.Failed;
            CoroutineFactory.NewCoroutine(SyncActivityResult, 0).MoveNext();
            //计算并同步最后的积分数据
            CalculateFinalPointList(nFubenResult, 20);
        }

        //胜利了
        private void OnWin()
        {
            Running = false;
            CloseTimer(eDungeonTimerType.CreateRandomMonster);
            KillAllMonster();
            nFubenResult = (int)eDungeonCompleteType.Success;
            CoroutineFactory.NewCoroutine(SyncActivityResult, 1).MoveNext();
            CalculateFinalPointList(nFubenResult, 180);
        }

        private IEnumerator SyncActivityResult(Coroutine co, int result)
        {
            var msg = SceneServer.Instance.ActivityAgent.SSSaveActivityResult(0, ServerId, ActivityId, result);
            yield return msg.SendAndWaitUntilDone(co);
            if (msg.State != MessageState.Reply || msg.ErrorCode != (int)ErrorCodes.OK)
            {
                Logger.Error("SSSaveActivityResult failed in SyncActivityResult()!!!");
                yield break;
            }
        }

        private void TimeTick()
        {
            if (mRandomMonsterTimerList.Count > 0 && mRandomMonsterTimerList[0] < DateTime.Now)
            {
                mRandomMonsterTimerList.RemoveAt(0);
                CheckRandomMonster();
                nCurRandWaveIdx++;
            }
            var reachTime = DateTime.Now - mToMobTime;
            if (reachTime.TotalSeconds < 0)
            {
                return;
            }
            var tbMieShi = Table.GetMieShi(ActivityId);
            mToMobTime = mToMobTime.AddSeconds(tbMieShi.MobIntervalTime);
            NextLadder();
        }

        private bool CheckMobFinish()
        {
            if (FinishList.Count == 0)
                return false;
            for (int i = 0; i < FinishList.Count; i++)
            {
                if (false == FinishList[i])
                    return false;
            }
            return true;
        }

        private void Tower1Ondamage(ObjNPC npc, ObjCharacter caster, int damage)
        {
            mIsFubenInfoDirty = true;
            var unit = mFubenInfoMsg.Units[1];
            var curHp = npc.GetAttribute(eAttributeType.HpNow);
            var maxHp = npc.GetAttribute(eAttributeType.HpMax);
            if (curHp == 0 || maxHp == 0)
            {
                unit.Params[0] = 0;
            }
            else
            {
                unit.Params[0] = (int)((double)curHp * 100 / (double)maxHp);
            }
        }

        private void Tower2Ondamage(ObjNPC npc, ObjCharacter caster, int damage)
        {
            mIsFubenInfoDirty = true;
            var unit = mFubenInfoMsg.Units[1];
            var curHp = npc.GetAttribute(eAttributeType.HpNow);
            var maxHp = npc.GetAttribute(eAttributeType.HpMax);
            if (curHp == 0 || maxHp == 0)
            {
                unit.Params[1] = 0;
            }
            else
            {
                unit.Params[1] = (int)((double)curHp * 100 / (double)maxHp);
            }
        }

        private void Tower3Ondamage(ObjNPC npc, ObjCharacter caster, int damage)
        {
            mIsFubenInfoDirty = true;
            var unit = mFubenInfoMsg.Units[1];
            var curHp = npc.GetAttribute(eAttributeType.HpNow);
            var maxHp = npc.GetAttribute(eAttributeType.HpMax);
            if (curHp == 0 || maxHp == 0)
            {
                unit.Params[2] = 0;
            }
            else
            {
                unit.Params[2] = (int)((double)curHp * 100 / (double)maxHp);
            }
        }

        private void Tower4Ondamage(ObjNPC npc, ObjCharacter caster, int damage)
        {
            mIsFubenInfoDirty = true;
            var unit = mFubenInfoMsg.Units[1];
            var curHp = npc.GetAttribute(eAttributeType.HpNow);
            var maxHp = npc.GetAttribute(eAttributeType.HpMax);
            if (curHp == 0 || maxHp == 0)
            {
                unit.Params[3] = 0;
            }
            else
            {
                unit.Params[3] = (int)((double)curHp * 100 / (double)maxHp);
            }
        }

        private void Tower5Ondamage(ObjNPC npc, ObjCharacter caster, int damage)
        {
            mIsFubenInfoDirty = true;
            var unit = mFubenInfoMsg.Units[1];
            var curHp = npc.GetAttribute(eAttributeType.HpNow);
            var maxHp = npc.GetAttribute(eAttributeType.HpMax);
            if (curHp == 0 || maxHp == 0)
            {
                unit.Params[4] = 0;
            }
            else
            {
                unit.Params[4] = (int)((double)curHp * 100 / (double)maxHp);
            }
        }

        private void Tower6Ondamage(ObjNPC npc, ObjCharacter caster, int damage)
        {
            mIsFubenInfoDirty = true;
            var unit = mFubenInfoMsg.Units[1];
            var curHp = npc.GetAttribute(eAttributeType.HpNow);
            var maxHp = npc.GetAttribute(eAttributeType.HpMax);
            if (curHp == 0 || maxHp == 0)
            {
                unit.Params[5] = 0;
            }
            else
            {
                unit.Params[5] = (int)((double)curHp * 100 / (double)maxHp);
            }
        }

        private void MainTowerOndamage(ObjNPC npc, ObjCharacter caster, int damage)
        {
            mIsFubenInfoDirty = true;
            var unit2 = mFubenInfoMsg.Units[2];
            var curHp = npc.GetAttribute(eAttributeType.HpNow);
            var maxHp = npc.GetAttribute(eAttributeType.HpMax);
            if (curHp == 0 || maxHp == 0 || npc.IsDead() || !npc.Active)
            {
                unit2.Params[0] = 0;
            }
            else
            {
                unit2.Params[0] = (int)((double)curHp * 100 / (double)maxHp);
            }
        }

        private void ResetTowerHp()
        {
            mIsFubenInfoDirty = true;
            var unit = mFubenInfoMsg.Units[1];
            for (int i = 0; i < TowerObjList.Count; i++)
            {
                var tower = TowerObjList[i];
                if (unit.Params.Count < (i + 1))
                    unit.Params.Add(0);
                if (null == tower || tower.IsDead() || !tower.Active)
                {
                    unit.Params[i] = 0;
                }
                else
                {
                    var curHp = tower.GetAttribute(eAttributeType.HpNow);
                    var maxHp = tower.GetAttribute(eAttributeType.HpMax);
                    if (curHp == 0 || maxHp == 0)
                    {
                        unit.Params[i] = 0;
                    }
                    else
                    {
                        unit.Params[i] = (int)((double)curHp * 100 / (double)maxHp);
                    }
                }
            }

            var unit2 = mFubenInfoMsg.Units[2];
            if (unit2.Params.Count < 1)
                unit2.Params.Add(0);
            if (null != mainTower)
            {
                if (mainTower.IsDead() || !mainTower.Active)
                {
                    unit2.Params[0] = 0;
                }
                else
                {
                    var curHp = mainTower.GetAttribute(eAttributeType.HpNow);
                    var maxHp = mainTower.GetAttribute(eAttributeType.HpMax);
                    if (curHp == 0 || maxHp == 0)
                    {
                        unit2.Params[0] = 0;
                    }
                    else
                    {
                        unit2.Params[0] = (int)((double)curHp * 100 / (double)maxHp);
                    }
                }
            }
            else
            {
                unit2.Params[0] = 0;
            }
        }
        public void UpdatePlayerContribution(ulong objId, int Contribution, string name, float rate)
        {
            AddToInfo(objId, name);
            dicBattleInfo[objId].Contribution = Contribution;
            //此处修改 贡献之后人不进去也会有积分  现在修改为人进去之后才会有积分
            /** var damageUnit = new DamageUnit();
             damageUnit.CharacterId = objId;
             damageUnit.Damage = 0;
             damageUnit.Name = name;
             damageUnit.Rank = 0;
             damageUnit.Rate = rate;
             mPointList.Add(objId, damageUnit);**/
        }
        public void SetMonsterSkillLevel(int towerId, int level)
        {
            if (towerId > 0 && towerId <= TowerObjList.Count && null != TowerObjList[towerId - 1])
            {
                var tbSkill = Table.GetBatteryLevel(level);
                if (tbSkill != null)
                {
                    TowerObjList[towerId - 1].NormalSkillId = tbSkill.BatterySkillId;
                }
                DataContract.ActivityBatteryOne data = mActivityInfo.batterys[towerId - 1];
                data.skillLevel = level;
            }
        }

        //private void ResetWorldLevel()
        //{
        //    Level = nBaseWorldLevel;// + PlayerCount / 5;
        //}
        //重新计算怪物最大等级
        private void ResetMonsterLevel(CommonActivityInfo data)
        {
            if (data == null)
            {
                return;
            }
            for (int i = 0; i < TowerObjList.Count; i++)
            {
                if (null == TowerObjList[i])
                    continue;
                var battery = data.batterys.Find(d => d.batteryGuid == TowerObjList[i].ObjId);
                if (battery != null)
                {
                    TowerObjList[i].Attr.SetDataValue(eAttributeType.HpMax, battery.curMaxHP);
                    TowerObjList[i].Attr.SetDataValue(eAttributeType.HpNow, battery.curMaxHP);
                    var tbSkill = Table.GetBatteryLevel(battery.skillLevel);
                    if (tbSkill != null)
                    {
                        TowerObjList[i].NormalSkillId = tbSkill.BatterySkillId;
                    }
                }
            }
            if (null != mainTower)
            {
                var mainBattery = data.batterys.Find(d => d.batteryGuid == mainTower.ObjId);
                if (mainBattery != null)
                {
                    mainTower.Attr.SetDataValue(eAttributeType.HpMax, mainBattery.curMaxHP);
                    mainTower.Attr.SetDataValue(eAttributeType.HpNow, mainBattery.curMaxHP);
                }
            }
        }

        private void NextLadder()
        {
            if (nCurLadderCount >= MaxLadderCount)
            {
                //结束了，不再刷新怪物了
                SceneServerControl.Timer.DeleteTrigger(mTrigger);
                mTrigger = null;

                return;
            }
            if (nCurLadderCount == 0)
            {
                EnterNextPhase();
            }
            ResetTowerHp();

            var tbScene = Table.GetScene(FubenId);
            NpcIndexList.Clear(); CountList.Clear(); IterList.Clear(); FinishList.Clear();
            List<int> indexList = new List<int>();
            List<int> numList = new List<int>();
            GetMonsterInfo(nCurLadderCount, ref indexList, ref numList);
            for (int j = 0; j < indexList.Count; j++)
            {
                NpcIndexList.Add(indexList.GetIndexValue(j));
                var tbSceneNpc = Table.GetSceneNpc(indexList.GetIndexValue(j));
                var tbNpcBase = Table.GetNpcBase(tbSceneNpc.DataID);
                if (tbNpcBase.NpcType == 1)//精英怪数量根据实际玩家数进行衰减
                {
                    var count = Math.Max(Math.Round((double)nPlayerCount / tbScene.PlayersMaxA * numList.GetIndexValue(j), 0), 9);
                    CountList.Add((int)count);
                }
                else
                {
                    CountList.Add(numList.GetIndexValue(j));
                }
                IterList.Add(0);
                FinishList.Add(false);
                StartTimer(eDungeonTimerType.CreateMonster, DateTime.Now.AddSeconds(0.5f), DelayCreateNpc, 500);
            }

            nCurLadderCount = nCurLadderCount + 1;

            RefreshFubenInfo();
        }

        //刷怪逻辑
        private void DelayCreateNpc()
        {
            int temp = 0;
            for (temp = 0; temp < FinishList.Count; temp++)
            {
                if (false == FinishList[temp])
                {
                    break;
                }
            }
            if (temp == FinishList.Count)
            {
                //EnterNextPhase();
                ResetTowerHp();
                CloseTimer(eDungeonTimerType.CreateMonster);
                //计算下一波刷怪时间
                if (mFubenInfoMsg.Units[0].Params.Count < 1)
                {
                    mFubenInfoMsg.Units[0].Params.Add((int)(mToMobTime - DateTime.Now).TotalSeconds);
                }
                else
                {
                    mFubenInfoMsg.Units[0].Params[0] = (int)(mToMobTime - DateTime.Now).TotalSeconds;
                }
                return;
            }

            var tbSceneNpc = Table.GetSceneNpc(NpcIndexList[temp]);
            var tbNpcBase = Table.GetNpcBase(tbSceneNpc.DataID);
            if (tbNpcBase.NpcType == 2)  //BOSS
            {
                var vector = new Vector2((float)tbSceneNpc.PosX, (float)tbSceneNpc.PosZ);
                ulong objId = CreateNpc(tbNpcBase.Id, vector);
                Logger.Warn("Refresh Npc BOSS----------------. DataId={0},pos={1},scene={2}", tbNpcBase.Id, vector, TypeId);
                EnterBossPhase();

                ++IterList[temp];
                if (objId > 0)
                    dicBoss.Add(objId, true);
            }
            else
            {
                for (int i = tbSceneNpc.RandomStartID; i < tbSceneNpc.RandomEndID; i++)
                {
                    if (IterList[temp] == CountList[temp])
                        break;
                    var tbRandomPos = Table.GetRandomCoordinate(i);
                    if (null == tbRandomPos)
                        continue;
                    var x = MyRandom.Random(tbRandomPos.PosX - 5, tbRandomPos.PosX + 5);
                    var y = MyRandom.Random(tbRandomPos.PosY - 5, tbRandomPos.PosY + 5);
                    var vec = new Vector2(x, y);

                    if (!ValidPosition(vec))
                    {
                        var tmp = FindNearestValidPosition(vec, 10);
                        if (null != tmp)
                        {
                            vec = tmp.Value;
                        }
                        else
                        {
                            Logger.Warn("Npc postion is invalid. DataId={0},pos={1},scene={2}", tbNpcBase.Id, vec, TypeId);
                        }
                    }
                    CreateNpc(tbNpcBase.Id, vec);
                    ++IterList[temp];
                    //Logger.Warn("Refresh Npc monster----------------. DataId={0},pos={1},scene={2}", tbNpcBase.Id, vec, TypeId);
                }
            }

            if (IterList[temp] == CountList[temp])
            {
                FinishList[temp] = true;
            }
        }

        //进入刷BOSS CG动画
        private void EnterBossPhase()
        {
            if (State >= eDungeonState.WillClose)
            {
                return;
            }

            if (FubenLogicRecord.EnterStateID == -1 && HadPlayCG)
            {
                return;
            }
            HadPlayCG = true;
            var tbMieShi = Table.GetMieShi(ActivityId);
            if (null == tbMieShi)
                return;
            FubenLogicRecord = Table.GetFubenLogic(tbMieShi.BossCgID);
        }

        //造Npc
        private ulong CreateNpc(int dataId, Vector2 pos)
        {
            var npc = CreateNpc(null, dataId, pos, Vector2.UnitX); //, Vector2.UnitX, "", Level);
            if (null != npc)
            {
                //npc.CanRelive = false;
                //foreach (var i in npc.TableCharacter.InitSkill)
                //{
                //    npc.Skill.ResetSkill(i, Level);
                //}
                npc.Attr.InitAttributesAll();
                npc.ReliveTimer = mToMobTime.AddSeconds(-60);

                if (npc.GetCamp() == 2)
                    ++nMobMonsterCount;
                return npc.mObjId;
            }
            return 0;
        }

        public void UpdateSceneNpcPos()
        {
            sceneNpcPosList.NpcList.Clear();
            sceneNpcPosList.NpcIdPosList.Clear();
            foreach (var pair in mObjDict)
            {
                if (!pair.Value.Active)
                    continue;

                var objType = pair.Value.GetObjType();
                if (objType == ObjType.PLAYER)
                {
                    var player = pair.Value as ObjPlayer;
                    if (player != null)
                    {
                        var info = CreateNpcPosInfo(0, player.ObjId, player.GetPosition());
                        sceneNpcPosList.NpcList.Add(info);
                    }
                }
                else if (objType == ObjType.NPC)
                {
                    var t = pair.Value as ObjNPC;
                    if (t == null)
                    {
                        continue;
                    }
                    if (t.IsDead())
                    {
                        continue;
                    }

                    if (t.TypeId == 999)
                    {
                        continue;
                    }

                    if (mTbMieShiRecord != null && t.TypeId == mTbMieShiRecord.BossDropBoxId)
                    {
                        if (t.TableNpc != null)
                        {
                            var info = CreateNpcPosInfo(5, t.ObjId, t.GetPosition(), t.TableNpc.Id);
                            sceneNpcPosList.NpcIdPosList.Add(info);
                        }
                        continue;
                    }

                    if (t.TypeId >= NPCTower1 && t.TypeId <= NPCTower6)
                    {
                        if (t.TableNpc != null)
                        {
                            var npcPos = CreateNpcPosInfo(1, t.ObjId, t.GetPosition(), t.TableNpc.Id);
                            sceneNpcPosList.NpcIdPosList.Add(npcPos);
                        }
                    }
                    else if (t.TypeId == NPCChancel)
                    {
                        if (t.TableNpc != null)
                        {
                            var npcPos = CreateNpcPosInfo(2, t.ObjId, t.GetPosition(), t.TableNpc.Id);
                            sceneNpcPosList.NpcIdPosList.Add(npcPos);
                        }
                    }
                    else
                    {
                        if (t.TableNpc != null && CheckNpcIsBoss(t.TableNpc.Id))
                        {
                            var info = CreateNpcPosInfo(3, t.ObjId, t.GetPosition());
                            sceneNpcPosList.NpcList.Add(info);
                        }
                        else
                        {
                            var info = CreateNpcPosInfo(4, t.ObjId, t.GetPosition());
                            sceneNpcPosList.NpcList.Add(info);
                        }
                    }
                }
            }
        }

        private SceneNpcPos CreateNpcPosInfo(int type, ulong id, Vector2 pos)
        {
            var npcPos = new SceneNpcPos();
            npcPos.Id = id;
            npcPos.PosX = pos.X;
            npcPos.PosY = pos.Y;
            npcPos.type = type;

            return npcPos;
        }

        private SceneNpcIdPos CreateNpcPosInfo(int type, ulong id, Vector2 pos, int npcId)
        {
            var npcPos = new SceneNpcIdPos();
            npcPos.Id = id;
            npcPos.PosX = pos.X;
            npcPos.PosY = pos.Y;
            npcPos.npcId = npcId;
            npcPos.type = type;
            sceneNpcPosList.NpcIdPosList.Add(npcPos);

            return npcPos;
        }

        private void GetMonsterInfo(int ladderCount, ref List<int> monsterIdList, ref List<int> monsterNumList)
        {
            var tbMieShi = Table.GetMieShi(ActivityId);
            if (ladderCount == 0)
            {
                monsterIdList = tbMieShi.Monster1IdList;
                monsterNumList = tbMieShi.Monster1NumList;
            }
            else if (ladderCount == 1)
            {
                monsterIdList = tbMieShi.Monster2IdList;
                monsterNumList = tbMieShi.Monster2NumList;
            }
            else if (ladderCount == 2)
            {
                monsterIdList = tbMieShi.Monster3IdList;
                monsterNumList = tbMieShi.Monster3NumList;
            }
            else
            {
                monsterIdList = tbMieShi.Monster4IdList;
                monsterNumList = tbMieShi.Monster4NumList;
            }
        }

        private int GetSingleDamage(int type)
        {
            var tbMieShiPublic = Table.GetMieShiPublic(1);
            if (null == tbMieShiPublic)
                return 0;
            if (type == 0)
            {
                return tbMieShiPublic.NormalDamageScore;
            }
            else if (type == 1)
            {
                return tbMieShiPublic.EliteDamageScore;
            }
            else if (type == 2)
            {
                return tbMieShiPublic.BossDamageScore;
            }
            return 0;
        }

        private void StartSyncBossDrop(int npcId)
        {
            CoroutineFactory.NewCoroutine(NotifyBossDropCanPickUp, npcId).MoveNext();
        }

        private IEnumerator NotifyBossDropCanPickUp(Coroutine co, int npcId)
        {
            var notifyMsg = SceneServer.Instance.ActivityAgent.SSSyncMieShiBoxCanPickUp(0, ServerId, ActivityId, npcId);
            yield return notifyMsg.SendAndWaitUntilDone(co);
            if (notifyMsg.State != MessageState.Reply || notifyMsg.ErrorCode != (int)ErrorCodes.OK)
            {
                Logger.Error("SSSyncMieShiBoxCanPickUp failed in NotifyBossDropCanPickUp()!!!");
                yield break;
            }
            yield return co;
        }

        private void StartSyncPointList()
        {
            CoroutineFactory.NewCoroutine(SyncPointListCoroutine).MoveNext();
        }

        public void GetSceneNpcPosList(SceneNpcPosList posList)
        {
            posList.NpcList.AddRange(sceneNpcPosList.NpcList);
            posList.NpcIdPosList.AddRange(sceneNpcPosList.NpcIdPosList);
        }

        private IEnumerator SyncPointListCoroutine(Coroutine co)
        {
            while (Running)
            {
                yield return SceneServer.Instance.ServerControl.Wait(co, TimeSpan.FromSeconds(10));

                var co1 = CoroutineFactory.NewSubroutine(SendPointListCoroutine, co);
                if (co1.MoveNext())
                {
                    yield return co1;
                }
            }
        }

        private IEnumerator SendPointListCoroutine(Coroutine co)
        {
            if (mPointList.Count == 0)
                yield break;

            SyncData.playerPoints.Clear();
            foreach (var item in mPointList)
            {
                SyncData.playerPoints.Add(item.Value);
            }
            SyncData.batteryPoints.Clear();
            SyncData.batteryPoints.AddRange(mBatteryPointList);
            SyncData.characterId = 0;
            if (Running == false && nFubenResult == (int)eDungeonCompleteType.Success)
            {
                var pointList = mPointList.ToList();
                for (int i = pointList.Count - 1; i > -1; i--)
                {
                    var unit = pointList[i];
                    if (unit.Value.Rank == 1)
                    {
                        SyncData.characterId = unit.Key;
                        break;
                    }
                }
            }
            var notifyMsg = SceneServer.Instance.ActivityAgent.SSSyncMieShiData(0, ServerId, ActivityId, SyncData);
            yield return notifyMsg.SendAndWaitUntilDone(co);
            if (notifyMsg.State != MessageState.Reply || notifyMsg.ErrorCode != (int)ErrorCodes.OK)
            {
                Logger.Error("SSSyncMieShiData failed in SendPointListCoroutine()!!!");
                //如果失败了，重试一次
                if (++nSendPointListFailedCount < 1)
                {
                    yield return SceneServer.Instance.ServerControl.Wait(co, TimeSpan.FromMilliseconds(20));
                    StartOnceSyncPointList();
                }
                else
                {
                    nSendPointListFailedCount = 0;
                }
                yield break;
            }
            yield break;
        }

        private void StartOnceSyncPointList()
        {
            CoroutineFactory.NewCoroutine(SendPointListCoroutine).MoveNext();
        }

        private void StartNotifyPointList()
        {
            CoroutineFactory.NewCoroutine(NotifyPointListCoroutine).MoveNext();
        }

        private IEnumerator NotifyPointListCoroutine(Coroutine co)
        {
            while (Running)
            {
                yield return SceneServer.Instance.ServerControl.Wait(co, TimeSpan.FromSeconds(5));

                var co1 = CoroutineFactory.NewSubroutine(SendNotifyListCoroutine, co);
                if (co1.MoveNext())
                {
                    yield return co1;
                }
            }
        }

        private IEnumerator SendNotifyListCoroutine(Coroutine co)
        {
            if (mPointList.Count == 0)
                yield break;
            //排序并赋予名次
            var pointList = mPointList.OrderBy(item => item.Value.Damage).ToList();
            var topPlayers = new List<DamageUnit>();
            var idx = 1;
            for (int i = pointList.Count - 1; i > -1; i--)
            {
                var unit = pointList[i];
                unit.Value.Rank = idx++;
                if (idx <= 11)
                {
                    //积分的前10名
                    topPlayers.Add(unit.Value);
                }
                else
                {
                    break;
                }

            }
            //通知客户端刷新积分列表
            PushActionToAllPlayer(player =>
            {
                var myPointList = new PointList();
                myPointList.TopPlayers.AddRange(topPlayers);
                var myUnit = mPointList.GetValue(player.ObjId);
                myPointList.myRank = myUnit != null ? myUnit.Rank : 0;
                myPointList.myPoint = myUnit != null ? myUnit.Damage : 0;
                player.Proxy.NotifyPointList(myPointList);
            });
            yield break;
        }

        private void CalculateFinalPointList(int result, int seconds)
        {
            CoroutineFactory.NewCoroutine(SendApplyContriDataMsg, result, seconds).MoveNext();
        }

        private IEnumerator SendApplyContriDataMsg(Coroutine co, int result, int seconds)
        {
            //var notifyMsg = SceneServer.Instance.ActivityAgent.SSApplyContributeRate(0, ServerId, ActivityId);
            //yield return notifyMsg.SendAndWaitUntilDone(co);
            //if (notifyMsg.State != MessageState.Reply || notifyMsg.ErrorCode != (int)ErrorCodes.OK)
            //{
            //    Logger.Error("SSApplyContributeRate failed in SendApplyContriDataMsg()!!!");
            //    yield break;
            //}
            ////换算最终得分
            //var data = notifyMsg.Response;
            //foreach (var item in mPointList)
            //{
            //    var rankItem = mPointList.GetValue(item.Key);
            //    var myUnit = data.rateList.Find(d => d.characterId == item.Key);
            //    if (myUnit != null)
            //    {
            //        rankItem.Damage += (int) (myUnit.rate*(float) nBatteryTotalPoint);
            //    }
            //}
            //上面的积分换算改成实时的了


            //排序并赋予最终名次
            var pointList = mPointList.OrderBy(item => item.Value.Damage).ToList();
            var idx = 1;
            for (int i = pointList.Count - 1; i > -1; i--)
            {
                var unit = pointList[i];
                unit.Value.Rank = idx++;

                if (unit.Value.Rank == 1)
                {
                    if (nFubenResult == (int)eDungeonCompleteType.Success)
                    {
                        //胜利广播
                        var args = new List<string>();
                        args.Add(Utils.AddCharacter(unit.Key, unit.Value.Name));
                        var content = Utils.WrapDictionaryId(300000081, args);
                        SceneServer.Instance.ChatAgent.BroadcastWorldMessage((uint)this.ServerId,
                            (int)eChatChannel.SystemScroll, 0,
                            string.Empty, new ChatMessageContent { Content = content });

                    }
                    else if (nFubenResult == (int)eDungeonCompleteType.Failed)
                    {
                        //失败广播
                        var args = new List<string>();
                        var content = Utils.WrapDictionaryId(300000082, args);
                        SceneServer.Instance.ChatAgent.BroadcastWorldMessage((uint)this.ServerId,
                            (int)eChatChannel.SystemScroll, 0,
                            string.Empty, new ChatMessageContent { Content = content });
                    }
                }
            }
            BroadBattleInfo(nFubenResult == (int)eDungeonCompleteType.Success ? 1 : 0);
            CompleteToAll(result, seconds);

            //将结束后的积分数据同步给Activity
            CoroutineFactory.NewCoroutine(SendPointListCoroutine).MoveNext();



            yield return co;
        }

        private void StartSyncBatteryGuid()
        {
            CoroutineFactory.NewCoroutine(SendSyncBatteryGuidMsg).MoveNext();
        }

        private IEnumerator SendSyncBatteryGuidMsg(Coroutine co)
        {
            MieShiBatteryGuid list = new MieShiBatteryGuid();
            for (int i = 0; i < TowerObjList.Count; i++)
            {
                var tower = TowerObjList[i];
                BatteryGuidUnit guid = new BatteryGuidUnit();
                guid.batteryId = i + 1;
                guid.guid = tower.ObjId;
                list.data.Add(guid);
            }
            var notifyMsg = SceneServer.Instance.ActivityAgent.SSSetAndGetActivityData(0, Guid, ServerId, ActivityId, list);
            yield return notifyMsg.SendAndWaitUntilDone(co);
            if (notifyMsg.State != MessageState.Reply || notifyMsg.ErrorCode != (int)ErrorCodes.OK)
            {
                Logger.Error("SSSaveBatteryGuid failed in SendSyncBatteryGuidMsg()!!!");
                yield break;
            }
            mActivityInfo = notifyMsg.Response;
            ResetMonsterLevel(notifyMsg.Response);
        }

        private bool CheckNpcIsBoss(int npcId)
        {
            var tbMieShi = Table.GetMieShi(ActivityId);
            for (int i = 0; i < tbMieShi.Monster4IdList.Count; i++)
            {
                var tbNpcBase = Table.GetNpcBase(npcId);
                var monster = Table.GetSceneNpc(tbMieShi.Monster4IdList[i]);
                if (npcId == monster.DataID && tbNpcBase.NpcType == 2)
                    return true;
            }
            return false;
        }
        private bool CheckBossLive()
        {

            return true;
        }
        private void SetChanceDamagePrompt()
        {
            ChanceDamagePrompt = true;
        }
        private void RefreshFubenInfo()
        {
            mIsFubenInfoDirty = true;
            //更新下一波倒计时
            var unit0 = mFubenInfoMsg.Units[0];
            if (nCurLadderCount == MaxLadderCount)
            {
                unit0.Params[0] = nCurLadderCount;
            }
            else
            {
                unit0.Params[0] = (int)(mToMobTime - DateTime.Now).TotalSeconds;
            }



            //更新炮台血量
            var unit = mFubenInfoMsg.Units[1];
            while (unit.Params.Count < 7)
            {
                unit.Params.Add(0);
            }
            unit.Params[6] = nPlayerCount;

            for (int i = 0; i < TowerObjList.Count; ++i)
            {
                var tower = TowerObjList[i];
                if (null == tower || tower.IsDead() || !tower.Active)
                {
                    unit.Params[i] = 0;
                }
                else
                {
                    var curHp = tower.GetAttribute(eAttributeType.HpNow);
                    var maxHp = tower.GetAttribute(eAttributeType.HpMax);
                    if (curHp == 0 || maxHp == 0)
                    {
                        unit.Params[i] = 0;
                    }
                    else
                    {
                        unit.Params[i] = (int)((double)curHp * 100 / (double)maxHp);
                        if (DamagePromtCount != null)
                        {
                            var npcId = TowerObjList[i].mTypeId;
                            var tbNpcBase = Table.GetNpcBase(npcId);

                            int Count = GetPromptCountById(npcId);
                            if (Count < HpPercent.Count && unit.Params[i] < HpPercent[Count])
                            {
                                if (DamagePromtCount.ContainsKey(npcId))
                                {
                                    DamagePromtCount[npcId] = Count + 1;
                                }
                                else
                                {
                                    DamagePromtCount.Add(npcId, Count + 1);
                                }


                                var args = new List<string>();
                                args.Add(TowerObjList[i].GetName());
                                args.Add(unit.Params[i].ToString());


                                var content = Utils.WrapDictionaryId(300000107, args);
                                PushActionToAllPlayer(p => { p.Proxy.NotifyBattleReminder(19, content, 0); });

                                var extInt = new List<int>();
                                extInt.Add(FubenId);
                                extInt.Add((int)TowerObjList[i].GetPosition().X * 100);
                                extInt.Add((int)TowerObjList[i].GetPosition().Y * 100);

                                content = Utils.WrapPositionDictionaryId(300000107, args, extInt);
                                SceneServer.Instance.ChatAgent.BroadcastWorldMessage((uint)this.ServerId,
                                (int)eChatChannel.Help, 0,
                                string.Empty, new ChatMessageContent { Content = content });
                            }
                        }
                    }
                }
            }
            //更新圣坛血量
            var unit2 = mFubenInfoMsg.Units[2];
            if (null != mainTower)
            {
                if (mainTower.IsDead() || !mainTower.Active)
                {
                    unit2.Params[0] = 0;
                }
                else
                {
                    var curHp = mainTower.GetAttribute(eAttributeType.HpNow);
                    var maxHp = mainTower.GetAttribute(eAttributeType.HpMax);
                    if (curHp == 0 || maxHp == 0)
                    {
                        unit2.Params[0] = 0;
                    }
                    else
                    {
                        unit2.Params[0] = (int)((double)curHp * 100 / (double)maxHp);
                    }
                }
            }
            else
            {
                unit2.Params[0] = 0;
            }



            var unit3 = mFubenInfoMsg.Units[3];
            if (unit3.Params.Count < 1)
                unit3.Params.Add(0);

            int count = 0;
            PushActionToAllObj(obj =>
            {
                if (obj.GetObjType() != ObjType.NPC)
                {
                    return;
                }
                ObjNPC npc = obj as ObjNPC;
                if (npc == null || npc.IsDead() == true || npc.GetCamp() != 2)
                {
                    return;
                }
                count++;
            });

            unit3.Params[0] = count;
            SendFubenInfo();
        }

        private void SaveBatteryDestroyData(ulong objId)
        {
            CoroutineFactory.NewCoroutine(SendSaveDestroyData, objId).MoveNext();
        }

        private int GetPromptCountById(int id)
        {
            int count;
            if (!DamagePromtCount.TryGetValue(id, out count))
            {
                return 0;
            }
            return count;
        }

        private IEnumerator SendSaveDestroyData(Coroutine co, ulong objId)
        {
            var notifyMsg = SceneServer.Instance.ActivityAgent.SSSaveBatteryDestroy(0, ServerId, ActivityId, objId);
            yield return notifyMsg.SendAndWaitUntilDone(co);
            if (notifyMsg.State != MessageState.Reply || notifyMsg.ErrorCode != (int)ErrorCodes.OK)
            {
                Logger.Error("SSSaveBatteryDestroy failed in SendSaveDestroyData()!!!");
                yield break;
            }
        }
        private void KillAllMonster()
        {
            List<ObjNPC> l = new List<ObjNPC>();
            PushActionToAllObj(obj =>
            {

                if (obj.GetObjType() != ObjType.NPC)
                {
                    return;
                }

                ObjNPC npc = obj as ObjNPC;
                if (npc != null && npc.GetCamp() == 2)
                {
                    l.Add(npc);
                }
            });
            foreach (var npc in l)
            {
                npc.mDropOnDie = false;
                npc.Die(0, 0);
            }
        }
        private void PlayerKillNpc(ulong objId)
        {
            if (dicBattleInfo.ContainsKey(objId))
            {
                dicBattleInfo[objId].KillCount++;
            }
        }

        private void AddToInfo(ulong objId, string name, bool isEverInBattle = false)
        {
            if (!dicBattleInfo.ContainsKey(objId))
            {
                var playerInfo = new PlayerBattleInfo();
                playerInfo.name = name;
                playerInfo.isEverInBattle = isEverInBattle;
                dicBattleInfo.Add(objId, playerInfo);
            }
        }
        public override void OnPlayerPickUp(ulong objId, int itemId, int count)
        {
            if (dicBattleInfo.ContainsKey(objId))
            {
                var info = dicBattleInfo[objId];
                if (info.PickList.ContainsKey(itemId))
                {
                    info.PickList[itemId] += count;
                }
                else
                {
                    info.PickList.Add(itemId, count);
                }
            }
        }
        public void BroadBattleInfo(int isWin)
        {

            foreach (var objPlayer in EnumAllPlayer())
            {
                MieshiResultMsg msg = new MieshiResultMsg();
                msg.IsWin = isWin;
                msg.TowerCount = TowerCount;
                if (dicBattleInfo.ContainsKey(objPlayer.ObjId))
                {
                    var info = dicBattleInfo[objPlayer.ObjId];
                    msg.KillCount = info.KillCount;
                    msg.Contribution = info.Contribution;
                    DamageUnit damageUnit = mPointList.GetValue(objPlayer.ObjId);
                    if (damageUnit != null)
                    {//积分
                        msg.Score = damageUnit.Damage;
                        msg.Rank = damageUnit.Rank;
                    }

                    foreach (var v in info.PickList)
                    {
                        msg.ItemId.Add(v.Key);
                        msg.ItemNum.Add(v.Value);
                    }
                }
                msg.ActivityId = ActivityId;
                objPlayer.Proxy.SendMieshiResult(msg);
            }
        }
        #endregion
    }
}