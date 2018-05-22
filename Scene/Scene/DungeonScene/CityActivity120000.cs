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
    public class CityActivity120000 : UniformDungeon
    {
        //建筑数据
        protected List<BuildingData> BuildingList = new List<BuildingData>();
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public override void CompleteToAll(FubenResult result, int seconds = 20)
        {
            if (State > eDungeonState.Start)
            {
                return;
            }

            var args = result.Args;

            //statistic
            foreach (var pair in mStatistic)
            {
                args.Add(pair.Value.x);
                args.Add(pair.Value.y);
            }

            foreach (var objPlayer in EnumAllPlayer())
            {
                Complete(objPlayer.ObjId, result);
            }

            EnterAutoClose(seconds);
        }

        protected override void DoIntermittentCreateMonster(int id, int interval)
        {
            var tbSkillUpgrading = Table.GetSkillUpgrading(id);
            var ids = tbSkillUpgrading.Values.ToList();
            var randomIdx = MyRandom.Random(ids.Count);
            var idx = 0;
            StartTimer(eDungeonTimerType.CreateMonster, DateTime.Now, () =>
            {
                if (idx < ids.Count)
                {
                    if (randomIdx == idx)
                    {
                        CreateSceneNpc(SpeSceneNpcId);
                    }
                    CreateSceneNpc(ids[idx++]);
                }
                else
                {
                    CloseTimer(eDungeonTimerType.CreateMonster);
                }
            }, interval);
        }

        public override void ExitDungeon(ObjPlayer player)
        {
            base.ExitDungeon(player);
            Log(Logger, "ExitDungeon:player id = {0}, name = {1}", player.ObjId, player.GetName());

            var result = new FubenResult();
            result.CompleteType = (int) eDungeonCompleteType.Quit;
            CompleteToAll(result);
        }

        public override void OnCreate()
        {
            base.OnCreate();

            mIsFubenInfoDirty = true;
            var unit = mFubenInfoMsg.Units[2];
            unit.Params[0] = 100;

            foreach (var monsterId in MonsterIds)
            {
                mStatistic.Add(monsterId, new Vector2Int32());
            }
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

        public override void OnNpcDie(ObjNPC npc, ulong characterId = 0)
        {
            base.OnNpcDie(npc, characterId);

            if (npc.TypeId == MonsterSpecial)
            {
                mIsFubenInfoDirty = true;

                var unit = mFubenInfoMsg.Units[1];
                var count = ++unit.Params[0];

                unit = mFubenInfoMsg.Units[2];
                unit.Params[0] = 100 + 20*count;
            }

            Vector2Int32 pair;
            if (!mStatistic.TryGetValue(npc.TypeId, out pair))
            {
                return;
            }

            pair.x++;
        }

        public override void OnNpcEnter(ObjNPC npc)
        {
            base.OnNpcEnter(npc);

            //cast ai
            var script = npc.Script as NPCScript200003;
            if (null == script)
            {
                return;
            }

            if (MonsterTyp1 == npc.TypeId ||
                MonsterTyp2 == npc.TypeId ||
                MonsterTyp3 == npc.TypeId ||
                MonsterTyp4 == npc.TypeId ||
                MonsterTyp5 == npc.TypeId ||
                MonsterSpecial == npc.TypeId)
            {
                //添加路径点
                var val = npc.TableNpc.ServerParam[0].Split(',');
#if DEBUG
                if (0 != val.Length % 2 || val.Length < 2)
                {
                    Logger.Error("ABC npc.TableNpc.ServerParam {0}，format must be px,py", npc.TableNpc.Id);
                    return;
                }
#endif

                script.ListDestination.Clear();
                for (var i = 0; i < val.Length; )
                {
                    var p = new Vector2(float.Parse(val[i++]), float.Parse(val[i++]));
                    script.ListDestination.Add(p);
                }

                if (script.ListDestination.Count <= 0 && npc.Script != null)
                {
                    Logger.Error("{0} Destination is invalid,assign a destination ABC", npc.Script.ToString());
                }

                if (MonsterSpecial == npc.TypeId)
                { // 特殊哥布林
                    script.WaitTime = WaitTime;
                    //提示
                    PushActionToAllPlayer(p => { p.Proxy.NotifyBattleReminder(14, Utils.WrapDictionaryId(300902), 1); });
                }
            }

            Vector2Int32 pair;
            if (mStatistic.TryGetValue(npc.TypeId, out pair))
            {
                pair.y++;
            }
        }

        public override void OnPlayerEnterOver(ObjPlayer player)
        {
            base.OnPlayerEnterOver(player);

            if (!mHasStarted)
            {
                mHasStarted = true;
                var characterId = player.ObjId; //这里潜规则一下，家园场景的Guid就是家园拥有者的CharacterId
                CoroutineFactory.NewCoroutine(RequestBuildingPets, characterId).MoveNext();
            }
        }

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

            //造Npc
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

        public void SyncCityData(List<BuildingData> buildings)
        {
            var msg = new BuildingList();
            msg.Data.AddRange(buildings);
            SceneServer.Instance.ServerControl.SyncSceneBuilding(EnumAllPlayerId(), msg);
        }

        #region 副本常量

        ////普通哥布林的路径点
        //private readonly Vector2 NormalDestination = new Vector2(60, 11);

        ////特殊哥布林的路径点
        //private readonly Vector2[] ListDestination =
        //{
        //    new Vector2(18.4f, 35.1f),
        //    new Vector2(26.6f, 47.1f),
        //    new Vector2(40f, 45f),
        //    new Vector2(38f, 32.6f),
        //    new Vector2(37.7f, 21.2f),
        //    new Vector2(48.5f, 29.2f),
        //    new Vector2(57.7f, 26.5f)
        //};

        //每个路径点等待时间
        private readonly float WaitTime = 3;

        private const int MonsterTyp1 = 65000;
        private const int MonsterTyp2 = 65001;
        private const int MonsterTyp3 = 65002;
        private const int MonsterTyp4 = 65003;
        private const int MonsterTyp5 = 65004;
        private const int MonsterSpecial = 65005;

        private const int SpeSceneNpcId = 1300005;

        private readonly int[] MonsterIds =
        {
            MonsterTyp1,
            MonsterTyp2,
            MonsterTyp3,
            MonsterTyp4,
            MonsterTyp5,
            MonsterSpecial
        };

        #endregion

        #region 副本逻辑数据

        //刷特殊怪Timer
        private Trigger mTimer;

        //统计
        private readonly Dictionary<int, Vector2Int32> mStatistic = new Dictionary<int, Vector2Int32>();

        //是否开始
        private bool mHasStarted;

        #endregion
    }
}