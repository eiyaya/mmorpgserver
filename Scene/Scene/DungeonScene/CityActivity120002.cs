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
    //单人经验副本
    public class CityActivity120002 : DungeonScene
    {
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region 副本常量

        //地点名称
        /*
        	"矿洞洞口",
			"议事厅后面",
			"战争大厅附近",
			"恶魔回廊附近",
			"正门墙角",
			"正门附近",
			"港口附近",
			"合成屋附近"
         * */

        public readonly string[] AreaName =
        {
            "#301022",
            "#301023",
            "#301024",
            "#301025",
            "#301026",
            "#301027",
            "#301028",
            "#301029"
        };

        //出现了入侵的怪物！
        public readonly string Desc = "#301021";

        //副本难度数据
        public readonly int[] MonsterGroup_1 =
        {
            90053, 1300098, 90061, 1300098, 90069, 1300098,
            90054, 1300099, 90062, 1300099, 90070, 1300099,
            90055, 1300100, 90063, 1300100, 90071, 1300100,
            90056, 1300101, 90064, 1300101, 90072, 1300101,
            90057, 1300102, 90065, 1300102, 90073, 1300102,
            90058, 1300103, 90066, 1300103, 90074, 1300103,
            90059, 1300104, 90067, 1300104, 90075, 1300104,
            90060, 1300105, 90068, 1300105, 90076, 1300105
        };

        public class GenerateMonsterData
        {
            public int AreaId1;
            public int AreaId2;
            public List<int> SceneNpcList1 = new List<int>();
            public List<int> SceneNpcList2 = new List<int>();
        }

        //刷怪波数
        private const int MaxWaveNumber = 3;

        //地点数
        private const int AreaNumber = 8;

        //每次刷几个地点
        private const int AreaPerTime = 2;

        //每波刷几次
        private const int MonsterTimes = AreaNumber/AreaPerTime;

        //开始前等待时间
        private const int WaiteSeconds = 5;

        //每波怪物之间的等待时间
        private const int WaveWaiteSeconds = 10;

        //每波4个地点刷怪时间间隔
        private const int WaveAreaIntervalMillSeconds = 5*1000;

        #endregion

        #region 副本逻辑数据

        //是否已经开始
        //private bool HasStarted = false;

        //是否已经同步随从数据 
        private bool mSyncBuildingPet;

        //刷怪等级
        private int mMonsterLevel = -1;

        //副本开始时间
        private readonly DateTime mStartTime = DateTime.Now;

        //总共击杀的怪数
        //private int TotalMonster = 10;

        //难度数据
        private int[] mMonsterData;

        //第几波
        private int mWaveIndex;

        //要刷的怪
        //private List<GenerateMonsterData> mMonsterGrop = new List<GenerateMonsterData>();

        //当前还有多少怪
       // private int mMonsterCount;

        //刷特殊怪Timer
        private readonly Trigger[] mTimer = {null, null, null, null};

        //boss
        private Trigger mBossTimer;

        //boss Id
        private ulong mBossId = TypeDefine.INVALID_ULONG;

        //boss个数
        private int mBossCount;

        //是否显示boss个数
        private bool bShowBossCount;

        //刷boss点
        private readonly int[] mBossArea = new int[MaxWaveNumber];

        //msg
        private bool mDataDirty = true;
        private readonly FubenInfoMsg mMsg = new FubenInfoMsg();
        private readonly FubenInfoUnit mInfo1 = new FubenInfoUnit();
        private readonly FubenInfoUnit mInfo2 = new FubenInfoUnit();
        private readonly FubenInfoUnit mInfo3 = new FubenInfoUnit();

        //队长
        private string mLeaderName;

        //队长是否领取过副本奖励
        private int nLeaderGainedReward = -1;

        //许愿女神的npcId
        private const int HolderId = 223104;

        //许愿女神
        private ObjNPC mHolder;

        #endregion

        #region 重写Scene方法

        public override void OnCreate()
        {
            base.OnCreate();
            mMonsterData = MonsterGroup_1;
            var time = DateTime.Now.AddSeconds(WaiteSeconds);
            StartTimer(eDungeonTimerType.WaitStart, time, TimeOverStart);
            StartTimer(eDungeonTimerType.WaitEnd, time.AddMinutes(mFubenRecord.TimeLimitMinutes), TimeOverEnd);

            mMsg.LogicId = -1;

            mInfo1.Index = 40042;
            mInfo1.Params.Add(0);
            mInfo1.Params.Add(MaxWaveNumber);
            mMsg.Units.Add(mInfo1);

            mInfo2.Index = 40063;
            mInfo2.Params.Add(80);
            mInfo2.Params.Add(1);
            mMsg.Units.Add(mInfo2);

            mInfo3.Index = 40044;
            mInfo3.Params.Add(100);
            mMsg.Units.Add(mInfo3);

            //刷新副本信息的定时器
            CreateTimer(DateTime.Now, SendDungeonInfo, 1000);

            GenerateBossEventArea();
        }

        public override void OnNpcEnter(ObjNPC npc)
        {
            List<Vector2> destList = null;
            //cast ai
            var script = npc.Script as NPCScript200004;
            if (null != script)
            {
                destList = script.ListDestination;
            }
            else
            {
                var s1 = npc.Script as NPCScript200006;
                if (null != s1)
                {
                    destList = s1.ListDestination;
                }
            }

            if (destList != null)
            { //添加路径点
                var val = npc.TableNpc.ServerParam[0].Split(',');
#if DEBUG
                if (0 != val.Length % 2 || val.Length < 2)
                {
                    Logger.Error("npc.TableNpc.ServerParam {0}，format must be px,py", npc.TableNpc.Id);
                    return;
                }
#endif

                destList.Clear();
                for (var i = 0; i < val.Length; )
                {
                    var p = new Vector2(float.Parse(val[i++]), float.Parse(val[i++]));
                    destList.Add(p);
                }

                if (destList.Count <= 0 && npc.Script != null)
                {
                    Logger.Error("{0} Destination is invalid,assign a destination CCC", npc.Script.ToString());
                }
            }

            //
            if (npc.TypeId == HolderId)
            {
                mHolder = npc;
                mHolder.OnDamageCallback = OnHolderDamage;
            }

            base.OnNpcEnter(npc);
        }

        public override void OnPlayerEnter(ObjPlayer player)
        {
            base.OnPlayerEnter(player);
            this.addExp.Clear();
            ResetMonsterLevel();

            if (mSyncBuildingPet)
            {
                return;
            }

            if (null != Param)
            {
                if (Param.ObjId == player.ObjId)
                {
                    mSyncBuildingPet = true;

                    mLeaderName = player.GetName();
                    var characterId = player.ObjId;
                    CoroutineFactory.NewCoroutine(RequestPlayCount, characterId).MoveNext();
                    CoroutineFactory.NewCoroutine(RequestBuildingPets, characterId).MoveNext();
                }
            }
        }

        public override void OnPlayerEnterOver(ObjPlayer player)
        {
            base.OnPlayerEnterOver(player);

            player.Proxy.NotifyFubenInfo(mMsg);
        }

        public override void OnNpcDie(ObjNPC npc, ulong characterId = 0)
        {
            base.OnNpcDie(npc, characterId);

            if (State > eDungeonState.Start)
            {
                return;
            }

            mDataDirty = true;

            if (HolderId == npc.TypeId)
            {
                var result = new FubenResult();
                result.CompleteType = (int)eDungeonCompleteType.Failed;
                CompleteToAll(result);
                //死的是许愿女神
                EnterAutoClose(10);
                PushActionToAllPlayer(
                    player => { player.Proxy.NotifyBattleReminder(14, Utils.WrapDictionaryId(729), 1); });
            }
            else if (mBossId == npc.ObjId)
            {
//死的是boss
                mBossCount = Math.Max(mBossCount - 1, 0);
            }
            else
            {
                var script = npc.Script as NPCScript200004;
                if (null != script)
                {
                    mMonsterCount = Math.Max(mMonsterCount - 1, 0);
                }
            }
            
            if (0 == mMonsterCount && 0 == mBossCount)
            {
                mWaveIndex++;
                if (mWaveIndex >= MaxWaveNumber)
                {
                    SendDungeonInfo();

                    var result = new FubenResult();
                    result.CompleteType = (int) eDungeonCompleteType.Success;
                    CompleteToAll(result);
                }
                else
                {
                    NextWaveMonster();
                }
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            for (var i = 0; i < mTimer.Length; i++)
            {
                if (null != mTimer[i])
                {
                    SceneServerControl.Timer.DeleteTrigger(mTimer[i]);
                    mTimer[i] = null;
                }
            }

            if (null != mBossTimer)
            {
                SceneServerControl.Timer.DeleteTrigger(mBossTimer);
                mBossTimer = null;
            }
        }

        #endregion

        #region 重写DungeonScene方法

        //副本开始
        public override void StartDungeon()
        {
            base.StartDungeon();

            NextWaveMonster();

            var players = EnumAllPlayer();
            foreach (var player in players)
            {
                player.Proxy.NotifyCountdown((ulong) DateTime.Now.AddSeconds(WaiteSeconds).ToBinary(), 0);
            }
        }

        //副本结束
        public override void CompleteToAll(FubenResult result, int seconds = 20)
        {
            //rank
            var rank = 0;
            var span = (DateTime.Now - mStartTime).TotalSeconds;
            if (result.CompleteType == (int) eDungeonCompleteType.Success)
            {
                if (span <= 60*5)
                {
                    rank = 3;
                }
                else if (span <= 60*7)
                {
                    rank = 2;
                }
                else
                {
                    rank = 1;
                }
            }

            var args = result.Args;
            args.Add(rank);
            args.Add(nLeaderGainedReward);

            //队长名字
            result.Strs.Add(mLeaderName);

            foreach (var objPlayer in EnumAllPlayer())
            {
                result.SceneAddExp = 0;
                for (int i = 0; i < this.addExp.Count; i++)
                {
                    if (this.addExp[i].characterId == objPlayer.ObjId)
                    {
                        result.SceneAddExp += (ulong)this.addExp[i].exp;
                    }
                }
                
                Complete(objPlayer.ObjId, result);
            }

            EnterAutoClose(seconds);
        }

        #endregion

        #region 副本逻辑

        private void GenerateBossEventArea()
        {
            var area = new int[MonsterTimes];
            for (var i = 0; i < MonsterTimes; i++)
            {
                area[i] = i;
            }

            for (var i = 0; i < area.Length; i++)
            {
                var r = MyRandom.Random(0, area.Length - 1);
                if (r == i)
                {
                    continue;
                }

                var temp = area[i];
                area[i] = area[r];
                area[r] = temp;
            }

            for (var i = 0; i < MaxWaveNumber; i++)
            {
                var temp = area[i];
                mBossArea[i] = temp*2 + MyRandom.Random(0, 1);
            }
        }

        private void ResetMonsterLevel()
        {
            ///////
            var totles2 = 0;
            var maxNow = 0;
            foreach (var player in mPlayerDict.Values)
            {
                var l = player.GetLevel();
                totles2 += l*l;
            }
            maxNow = (int) Math.Sqrt(totles2/mPlayerDict.Count);

            if (mMonsterLevel != maxNow)
            {
                mMonsterLevel = maxNow;
                mHolder.SetToLevel(mMonsterLevel);
            }
        }

        //处理刷怪事件
        private void NextWaveMonster(bool notify = true)
        {
            for (var i = 0; i < mTimer.Length; i++)
            {
                if (mTimer[i] != null)
                {
                    SceneServerControl.Timer.DeleteTrigger(mTimer[i]);
                    mTimer[i] = null;
                }
            }
             

            //随机区域
            var area = new int[AreaNumber];
            for (var i = 0; i < AreaNumber; i++)
            {
                area[i] = i;
            }

            for (var i = 0; i < area.Length; i++)
            {
                var r = MyRandom.Random(0, area.Length - 1);
                if (r == i)
                {
                    continue;
                }

                var temp = area[i];
                area[i] = area[r];
                area[r] = temp;
            }

            //清理怪列表
            mMonsterCount = 0;

            for (var i = 0; i < MonsterTimes; i++)
            {
                var monsterGroup = new GenerateMonsterData();
                var area1 = area[i*2];
                var area2 = area[i*2 + 1];

                monsterGroup.AreaId1 = area1;
                monsterGroup.AreaId2 = area2;

                var idx1 = area1*MaxWaveNumber*2 + mWaveIndex*2;
                var idx2 = area2*MaxWaveNumber*2 + mWaveIndex*2;

                var tableMonsterGroup = Table.GetSkillUpgrading(mMonsterData[idx1]);
                if (null != tableMonsterGroup)
                {
                    for (var j = 0; j < tableMonsterGroup.Values.Count; j++)
                    {
                        monsterGroup.SceneNpcList1.Add(tableMonsterGroup.GetSkillUpgradingValue(j));
                        mMonsterCount++;
                    }
                }

                tableMonsterGroup = Table.GetSkillUpgrading(mMonsterData[idx2]);
                if (null != tableMonsterGroup)
                {
                    for (var j = 0; j < tableMonsterGroup.Values.Count; j++)
                    {
                        monsterGroup.SceneNpcList2.Add(tableMonsterGroup.GetSkillUpgradingValue(j));
                        mMonsterCount++;
                    }
                }

                var time = DateTime.Now.AddSeconds(WaiteSeconds);

                var idx = i;
                mTimer[idx] = SceneServerControl.Timer.CreateTrigger(time.AddSeconds(WaveWaiteSeconds*i), () =>
                {
                    var list1 = monsterGroup.SceneNpcList1;
                    var list2 = monsterGroup.SceneNpcList2;

                    var has = false;
                    if (list1.Count > 0)
                    {
                        has = true;
                        var sceneNpc = list1[0];

                        CreateSceneNpc(sceneNpc, default(Vector2), mMonsterLevel);
                        list1.RemoveAt(0);
                    }

                    if (list2.Count > 0)
                    {
                        has = true;
                        var sceneNpc = list2[0];

                        CreateSceneNpc(sceneNpc, default(Vector2), mMonsterLevel);
                        list2.RemoveAt(0);
                    }

                    if (!has)
                    {
                        SceneServerControl.Timer.DeleteTrigger(mTimer[idx]);
                        mTimer[idx] = null;
                    }
                }, WaveAreaIntervalMillSeconds);
            }
            BossEvent();

            if (notify)
            {
                //5秒后进入第{0}阶段
                var msg = string.Format("^301001^{0}", mWaveIndex + 1);
                SendNotify2AllPlayer(msg);
            }

            mDataDirty = true;
        }

        //刷boss
        private void BossEvent()
        {
            bShowBossCount = false;
            mBossTimer = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(45), () =>
            {
                var r = mBossArea[mWaveIndex];
                var idx = r*MaxWaveNumber*2 + mWaveIndex*2 + 1;
                var sceneBoss = mMonsterData[idx];
                var boss = CreateSceneNpc(sceneBoss, default(Vector2), mMonsterLevel);
                mBossId = boss.ObjId;
                mBossCount = 1;
                bShowBossCount = true;
                //出现了BOSS
                var msg = AreaName[r] + "#301020";
                SendNotify2AllPlayer(msg);
                mDataDirty = true;
                mBossTimer = null;
            });
        }

        //发送文字信息
        private void SendNotify2AllPlayer(string msg)
        {
            foreach (var player in EnumAllPlayer())
            {
                player.Proxy.NotifyBattleReminder(14, msg, 1);
            }
        }

        //刷新副本信息
        private void RefreshDungeonInfo()
        {
            mInfo1.Params[0] = Math.Min(MaxWaveNumber, mWaveIndex + 1);

            if (bShowBossCount)
            {
                mInfo2.Index = 40063;
                mInfo2.Params[0] = mMonsterCount;
                mInfo2.Params[1] = mBossCount;
            }
            else
            {
                mInfo2.Index = 40043;
                mInfo2.Params[0] = mMonsterCount;
            }
        }

        //
        private void SendDungeonInfo()
        {
            if (!mDataDirty)
            {
                return;
            }
            mDataDirty = false;

            RefreshDungeonInfo();

            PushActionToAllPlayer(player => { player.Proxy.NotifyFubenInfo(mMsg); });
        }

        //获得队长的副本完成次数
        public IEnumerator RequestPlayCount(Coroutine coroutine, ulong characterId)
        {
            var msg = SceneServer.Instance.LogicAgent.SSGetFlagOrCondition(characterId, characterId,
                mFubenRecord.ScriptId, -1);
            yield return msg.SendAndWaitUntilDone(coroutine);

            if (msg.State != MessageState.Reply)
            {
                Logger.Info("SSFetchExdata   msg.State != MessageState.Reply");
                yield break;
            }

            if (msg.ErrorCode != (int) ErrorCodes.OK)
            {
                Logger.Info("SSFetchExdata  msg.ErrorCode != (int)ErrorCodes.OK");
                yield break;
            }

            nLeaderGainedReward = msg.Response;
        }

        //请求家园随从，并造出来
        public IEnumerator RequestBuildingPets(Coroutine coroutine, ulong characterId)
        {
            var msg = SceneServer.Instance.LogicAgent.SSRequestCityBuidlingPetData(characterId, characterId);
            yield return msg.SendAndWaitUntilDone(coroutine);

            if (msg.State != MessageState.Reply)
            {
                Logger.Info("GetCity   msg.State != MessageState.Reply");
                yield break;
            }

            if (msg.ErrorCode != (int) ErrorCodes.OK)
            {
                Logger.Info("GetCity  msg.ErrorCode != (int)ErrorCodes.OK");
                yield break;
            }

            if (null == msg.Response)
            {
                Logger.Info("null == msg.Response");
                yield break;
            }

            var pets = msg.Response.Pets;


            //造随从Npc
            foreach (var pet in pets)
            {
                var petId = pet.PetId;
                var level = pet.Level;
                var areaId = pet.AreaId;

                var tableArea = Table.GetHomeSence(areaId);
                if (null == tableArea)
                {
                    continue;
                }

                var tablePet = Table.GetPet(petId);
                if (null == tablePet)
                {
                    continue;
                }

                var pos = new Vector2(tableArea.RetinuePosX, tableArea.RetinuePosY);
                var dir = new Vector2((float) Math.Cos(tableArea.FaceCorrection),
                    (float) Math.Sin(tableArea.FaceCorrection));
                CreateNpc(null, tablePet.CharacterID, pos, dir, "", level);
            }
        }

        //女神受到伤害
        private void OnHolderDamage(ObjNPC npc, ObjCharacter caster, int damage)
        {
            mDataDirty = true;

            var hpPercent = (int) (100.0*npc.GetAttribute(eAttributeType.HpNow)/npc.GetAttribute(eAttributeType.HpMax));
            mInfo3.Params[0] = hpPercent;
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