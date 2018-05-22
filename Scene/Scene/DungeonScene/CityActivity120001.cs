#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
    //单人经验副本
    public abstract class CityActivitySingleExpBase : DungeonScene
    {
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //刷boss
        private void BossEvent()
        {
            var r = MyRandom.Random(0, AreaNumber - 1);
            var idx = r*AreaNumber + 3;
            var sceneBoss = mMonsterGroup[idx];
            var boss = CreateSceneNpc(sceneBoss);
            mBossId = boss.ObjId;
            //出现了BOSS
            var msg = AreaName[r] + "#301020";
            SendNotify2AllPlayer(msg);
            mDataDirty = true;
            SendDungeonInfo();
        }

        //刷怪
        private void GenerateMonster()
        {
            if (null != mTimer)
            {
                SceneServerControl.Timer.DeleteTrigger(mTimer);
                mTimer = null;
            }

            mTimer = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(WaiteSeconds), () =>
            {
                if (mMonsterGrop.Count() <= 0)
                {
                    SceneServerControl.Timer.DeleteTrigger(mTimer);
                    mTimer = null;
                    return;
                }

                var data = mMonsterGrop[0];
                var list = data.SceneNpcList;
                if (list.Count <= 0)
                {
                    SceneServerControl.Timer.DeleteTrigger(mTimer);
                    mTimer = null;
                    return;
                }

                var sceneNpc = list[0];

                CreateSceneNpc(sceneNpc);

                if (5 == list.Count)
                {
                    var msg = AreaName[data.AreaId] + Desc;
                    SendNotify2AllPlayer(msg);
                }

                list.RemoveAt(0);
                if (list.Count <= 0)
                {
                    mMonsterGrop.RemoveAt(0);
                }
            }, WaveAreaIntervalMillSeconds);
        }

        //处理刷怪事件
        private void NextWaveMonster(bool notify = true)
        {
            if (mWaveIndex >= MaxWaveNumber)
            {
                BossEvent();
                return;
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
            mMonsterGrop.Clear();
            mMonsterCount = 0;
            //计算该刷哪些怪
            for (var i = 0; i < AreaNumber; i++)
            {
                var areaId = area[i];
                var idx = areaId*4 + mWaveIndex;

                var data = new GenerateMonsterData();
                mMonsterGrop.Add(data);

                data.AreaId = areaId;

                var table = Table.GetSkillUpgrading(mMonsterGroup[idx]);
                if (null == table)
                {
                    continue;
                }

                foreach (var value in table.Values)
                {
                    data.SceneNpcList.Add(value);
                    mMonsterCount++;
                }
            }

            //刷怪
            GenerateMonster();

            if (notify)
            {
                //5秒后进入第{0}阶段
                var msg = string.Format("^301001^{0}", mWaveIndex + 1);
                SendNotify2AllPlayer(msg);
            }

            SendDungeonInfo();
        }

        //刷新副本信息
        private void RefreshDungeonInfo()
        {
            if (!mDataDirty)
            {
                return;
            }
            if (mWaveIndex < MaxWaveNumber)
            {
                mInfo1.Index = 40042;
                mInfo1.Params[0] = Math.Min(MaxWaveNumber, mWaveIndex + 1);
                mInfo1.Params[1] = MaxWaveNumber;

                mInfo2.Index = 40043;
                mInfo2.Params[0] = mMonsterCount;
            }
            else
            {
                mInfo1.Index = 40070;
                mInfo1.Params[0] = mBossCount;

                mInfo2.Index = -1;
            }

            mDataDirty = true;
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

        private void SendDungeonInfo()
        {
            RefreshDungeonInfo();

            PushActionToAllPlayer(player => { player.Proxy.NotifyFubenInfo(mMsg); });
        }

        //发送文字信息
        private void SendNotify2AllPlayer(string msg)
        {
            foreach (var player in EnumAllPlayer())
            {
                player.Proxy.NotifyBattleReminder(14, msg, 1);
            }
        }

        #region 副本常量

        //"矿洞",
        //"恶魔回廊",
        //"正门",
        //"勇士港"
        //地点名称
        public readonly string[] AreaName =
        {
            "#301016",
            "#301017",
            "#301018",
            "#301019"
        };

        //出现了入侵的怪物！
        public readonly string Desc = "#301021";

        public class GenerateMonsterData
        {
            public int AreaId;
            public List<int> SceneNpcList = new List<int>();
        }

        //刷怪波数
        private readonly int MaxWaveNumber = 3;

        //地点数
        private readonly int AreaNumber = 4;

        //开始前等待时间
        private readonly int WaiteSeconds = 5;

        //每波4个地点刷怪时间间隔
        private readonly int WaveAreaIntervalMillSeconds = 2*1000;

        #endregion

        #region 副本逻辑数据

        //是否已经开始
        private bool HasStarted;

        //副本开始时间
        private readonly DateTime mStartTime = DateTime.Now;

        //总共击杀的怪数
        //private int TotalMonster = 10;

        //难度数据
        protected int[] mMonsterGroup;

        //第几波
        private int mWaveIndex;

        //要刷的怪
        private readonly List<GenerateMonsterData> mMonsterGrop = new List<GenerateMonsterData>();

        //当前还有多少怪
       // private int mMonsterCount;

        //刷特殊怪Timer
        private Trigger mTimer;

        //boss Id
        private ulong mBossId = TypeDefine.INVALID_ULONG;

        //boss个数
        private int mBossCount = 1;

        //msg
        private bool mDataDirty = true;
        private readonly FubenInfoMsg mMsg = new FubenInfoMsg();
        private readonly FubenInfoUnit mInfo1 = new FubenInfoUnit();
        private readonly FubenInfoUnit mInfo2 = new FubenInfoUnit();

        #endregion

        #region 重写父类函数

        public override void OnCreate()
        {
            base.OnCreate();

            var time = DateTime.Now.AddSeconds(WaiteSeconds);
            StartTimer(eDungeonTimerType.WaitEnd, time.AddMinutes(mFubenRecord.TimeLimitMinutes), TimeOverEnd);

            mMsg.LogicId = -1;

            mInfo1.Index = 40042;
            mInfo1.Params.Add(0);
            mInfo1.Params.Add(MaxWaveNumber);
            mMsg.Units.Add(mInfo1);

            mInfo2.Index = 40043;
            mInfo2.Params.Add(20);
            mMsg.Units.Add(mInfo2);

// 			mInfo3.Index = 40044;
// 			mInfo3.Params.Add(0);
// 			mInfo3.Params.Add(1);
// 			mMsg.Units.Add(mInfo3);


	        var tbScene = Table.GetScene(TypeId);
	        var tbFuben = Table.GetFuben(tbScene.FubenId);
	       

	        if (null == tbFuben)
			{

				string str = string.Format("tbFuben==null [{0}]", tbScene.FubenId);
#if DEBUG
				Debug.Assert(false, str);
#endif			
				Logger.Fatal(str);
	        }		
	
			//SkillUpdateId1,SkillUpdateId2,SkillUpdateId3,SceneNpcId
			//SkillUpdateId1,SkillUpdateId2,SkillUpdateId3,SceneNpcId
			//SkillUpdateId1,SkillUpdateId2,SkillUpdateId3,SceneNpcId
			//SkillUpdateId1,SkillUpdateId2,SkillUpdateId3,SceneNpcId

			/*
            90005, 90006, 90007, 1300018,
            90008, 90009, 90010, 1300019,
            90011, 90012, 90013, 1300020,
            90014, 90015, 90016, 1300021
			*/

#if DEBUG
				Debug.Assert(tbFuben.lParam1.Count == 4 * 4);
#endif
		        mMonsterGroup = tbFuben.lParam1.ToArray();

			

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
                    Logger.Error("{0} Destination is invalid,assign a destination BBC", npc.Script.ToString());
                }

            }

            base.OnNpcEnter(npc);
        }

        public override void OnPlayerEnterOver(ObjPlayer player)
        {
            base.OnPlayerEnterOver(player);
            this.addExp.Clear();

            if (!HasStarted)
            {
                HasStarted = true;

                var time = DateTime.Now.AddSeconds(WaiteSeconds);
                StartTimer(eDungeonTimerType.WaitStart, time, TimeOverStart);

                player.Proxy.NotifyCountdown((ulong) DateTime.Now.AddSeconds(WaiteSeconds).ToBinary(), 0);
                NextWaveMonster();

                var characterId = player.ObjId;
                CoroutineFactory.NewCoroutine(RequestBuildingPets, characterId).MoveNext();
            }

            player.Proxy.NotifyFubenInfo(mMsg);
        }

        private Dictionary<int, int> mDicBoss = new Dictionary<int, int>()
        {
            {1291000, 0},
            {1300070, 0},
            {1300071, 0},
            {1300072, 0},
            {1300073, 0},
            {1300106, 0}
        };
        public override void OnNpcDie(ObjNPC npc, ulong characterId = 0)
        {
            base.OnNpcDie(npc, characterId);

            //死的是boss
            if (mBossId == npc.ObjId)
            {
                mBossCount--;
                mDataDirty = true;
                SendDungeonInfo();

                var result = new FubenResult();
                result.CompleteType = (int) eDungeonCompleteType.Success;
                CompleteToAll(result);
                //结束副本
                return;
            }
            else if (mDicBoss.ContainsKey(npc.TableNpc.Id))
            {
                var result = new FubenResult();
                result.CompleteType = (int)eDungeonCompleteType.Failed;
                CompleteToAll(result);
                return;
            }

            var script = npc.Script as NPCScript200004;
            if (null != script)
            {
                mMonsterCount--;
            }

            if (0 == mMonsterCount && 0 == mMonsterGrop.Count)
            {
                mWaveIndex++;
                NextWaveMonster();
            }
            mDataDirty = true;
            SendDungeonInfo();
        }

        //副本结束
        public override void CompleteToAll(FubenResult result, int seconds = 20)
        {
            if (State > eDungeonState.Start)
            {
                return;
            }

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

        public override void ExitDungeon(ObjPlayer player)
        {
            base.ExitDungeon(player);

            var result = new FubenResult();
            result.CompleteType = (int) eDungeonCompleteType.Quit;
            CompleteToAll(result);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (null != mTimer)
            {
                SceneServerControl.Timer.DeleteTrigger(mTimer);
                mTimer = null;
            }
        }

        public override void OnPlayerPickUp(ulong objId, int itemId, int count)
        {
            DungeonSceneExpItem item = new DungeonSceneExpItem();
            item.characterId = objId;
            item.exp = count;

            this.addExp.Add(item);
        }
        #endregion
    }

	public class CityActivity1200010 : CityActivitySingleExpBase
	{
		
	}
	/*
    public class CityActivity1200010 : CityActivitySingleExpBase
    {
        //副本难度数据
        public static readonly int[] MonsterGroup =
        {
            90005, 90006, 90007, 1300018,
            90008, 90009, 90010, 1300019,
            90011, 90012, 90013, 1300020,
            90014, 90015, 90016, 1300021
        };

        public override void OnCreate()
        {
            mMonsterGroup = MonsterGroup;
            base.OnCreate();
        }

    }

	
    public class CityActivity1200011 : CityActivitySingleExpBase
    {
        //副本难度数据
        public static readonly int[] MonsterGroup =
        {
            90017, 90018, 90019, 1300034,
            90020, 90021, 90022, 1300035,
            90023, 90024, 90025, 1300036,
            90026, 90027, 90028, 1300037
        };

        public override void OnCreate()
        {
            mMonsterGroup = MonsterGroup;
            base.OnCreate();
        }
    }

    public class CityActivity1200012 : CityActivitySingleExpBase
    {
        //副本难度数据
        public static readonly int[] MonsterGroup =
        {
            90029, 90030, 90031, 1300050,
            90032, 90033, 90034, 1300051,
            90035, 90036, 90037, 1300052,
            90038, 90039, 90040, 1300053
        };

        public override void OnCreate()
        {
            mMonsterGroup = MonsterGroup;
            base.OnCreate();
        }
    }

    public class CityActivity1200013 : CityActivitySingleExpBase
    {
        //副本难度数据
        public static readonly int[] MonsterGroup =
        {
            90041, 90042, 90043, 1300066,
            90044, 90045, 90046, 1300067,
            90047, 90048, 90049, 1300068,
            90050, 90051, 90052, 1300069
        };

        public override void OnCreate()
        {
            mMonsterGroup = MonsterGroup;
            base.OnCreate();
        }
    }
	*/
}