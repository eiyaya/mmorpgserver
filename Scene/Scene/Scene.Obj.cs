#region using

using System;
using System.Collections.Generic;
using DataContract;
using DataTable;
using Mono.GameMath;
using Scene.Character;
using Shared;

#endregion

namespace Scene
{
    public partial class SceneDefaultImpl
    {
        public virtual bool EnterScene(Scene _this, ObjBase obj)
        {
            if (_this.mTicking)
            {
                _this.mNeedToAddObjDict.Add(obj.ObjId, obj);
                return true;
            }

            _this.OnObjBeforeEnterScene(obj);

            var zoneId = _this.Pos2ZoneId(obj.GetPosition().X, obj.GetPosition().Y);
            if (zoneId < 0 || zoneId >= _this.mZoneList.Count)
            {
                //这里保证一下Character进入的位置必须得是个有效的位置
                if (obj.IsCharacter())
                {
                    var character = obj as ObjCharacter;
                    var validPos = _this.FindNearestValidPosition(character.GetPosition());
                    if (null != validPos)
                    {
                        character.SetPosition(validPos.Value);
                        Logger.Warn("[{0}] set valid pos[{1},{2}]", character.GetName(), character.GetPosition().X,
                            character.GetPosition().Y);
                    }
                    else
                    {
                        character.SetPosition((float) _this.TableSceneData.Entry_x, (float) _this.TableSceneData.Entry_z);
                        Logger.Warn("[{0}] set entry pos[{1},{2}]", character.GetName(), character.GetPosition().X,
                            character.GetPosition().Y);
                    }
                }

                zoneId = _this.Pos2ZoneId(obj.GetPosition().X, obj.GetPosition().Y);
                if (zoneId < 0 || zoneId >= _this.mZoneList.Count)
                {
                    Logger.Fatal("zone = null[x={0},y={1}]", obj.GetPosition().X, obj.GetPosition().Y);
                    return false;
                }
            }

            if (_this.mObjDict.ContainsKey(obj.ObjId))
            {
                if (_this.mObjDict[obj.ObjId].Equals(obj))
                {
                    Logger.Fatal("!ObjDic.ContainsKey({0})  mObjDict[obj.ObjId].Equals(obj)", obj.ObjId);
                    return false;
                }
                _this.RemoveObj(obj);
                Logger.Fatal("!ObjDic.ContainsKey({0})", obj.ObjId);
            }

            _this.AddObj(obj);

            obj.EnterScene(_this);

            var zone = _this.mZoneList[zoneId];
            zone.AddObj(obj);
            obj.SetZone(zone);

            //player 一上来时不时Active状态，不会走这里，只有player掉了EnterSceneOver之后才是active状态，
            //EnterSceneOver里自己会处理可见和飞可见的玩家
            if (obj.Active)
            {
                var canSee = true;
                if (obj.IsCharacter())
                {
                    var character = obj as ObjCharacter;
                    if (character.IsDead())
                    {
                        canSee = false;
                    }
                }
                if (canSee)
                {
                    var bornType = _this.GetBornVisibleType();
                    obj.BroadcastCreateMe(bornType);
                }
            }

            if (obj.GetObjType() == ObjType.PLAYER)
            {
                var player = obj as ObjPlayer;
                foreach (var pair in _this.mAreaDict)
                {
                    pair.Value.AdjustPlayer(player);
                }
            }


            return true;
        }

        public virtual bool LeaveScene(Scene _this, ObjBase obj, ReasonType reason = ReasonType.VisibilityChanged)
        {
            if (_this.mTicking)
            {
                _this.mNeedToRemoveObjDict.Add(obj.ObjId, obj);
                obj.Active = false;
                return true;
            }

            var zone = obj.Zone;
            if (null != zone)
            {
                obj.BroadcastDestroyMe(reason);
                zone.RemoveObj(obj);
                //Uint64Array array = new Uint64Array();
                //array.Items.Add(obj.ObjId);
                //SceneServer.Instance.ServerControl.DeleteObj(zone.EnumAllVisiblePlayerIdExclude(obj.ObjId), array, (uint)reason);
            }
            obj.SetZone(null);
            if (obj.IsCharacter())
            {
                var character = obj as ObjCharacter;
                character.ClearRetinue();
                foreach (var pair in _this.mAreaDict)
                {
                    if (pair.Value.Cantains(character))
                    {
                        pair.Value.Remove(character);
                    }
                }

	            if (obj.GetObjType() == ObjType.PLAYER)
	            {
					_this.OnPlayerLeave(obj as ObjPlayer);
	            }
				else if (obj.GetObjType() == ObjType.NPC)
				{
					if (_this.SpeMonsterToWorldBossId.ContainsKey(obj))
					{
						Log(_this, Logger, "Special monster killed!Type = {0}", obj.TypeId);
						var worldBossId = _this.SpeMonsterToWorldBossId[obj];
						--_this.SpeMonsterAlwaysRecords[worldBossId];
						_this.SpeMonsterToWorldBossId.Remove(obj);
					}
				}
            }


            obj.LeavelScene();
            _this.RemoveObj(obj);

            return true;
        }

        public virtual bool InitNPC(Scene _this)
        {
            List<SceneNpcRecord> tempList;
            if (!SceneManager.SceneNpcs.TryGetValue(_this.TypeId, out tempList))
            {
                return true;
            }
            foreach (var record in tempList)
            {
                if (record.InitActivate == 0)
                {
                    continue;
                }

                var dataId = record.DataID;
                var pos = new Vector2((float) record.PosX, (float) record.PosZ);
                var dir = new Vector2((float) Math.Cos(record.FaceDirection), (float) Math.Sin(record.FaceDirection));
                try
                {
                    var npc = _this.CreateNpc(record, dataId, pos, dir);
                    if (null != npc)
                    {
                        npc.CanRelive = true; //只有场景初始化时造的怪才可以重生
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("InitNPC Error, SceneId={0},NpcId={1},dump={2}", record.SceneID, record.DataID, ex);
                }
            }
            return true;
        }

        public void InitMapNPC(Scene _this)
        {
            if (!SceneManager.SceneMapNpcs.TryGetValue(_this.TypeId, out _this.MapNpcRecords))
            {
                _this.MapNpcRecords = new List<MapTransferRecord>();
                return;
            }
            var datas = _this.MapNpcInfo.Data;
            datas.Clear();
            foreach (var record in _this.MapNpcRecords)
            {
                datas.Add(new MapNpcInfo
                {
                    TableId = record.Id,
                    Alive = true 
                });
            }
        }

        public AutoPlayer CreateAutoPlayer(Scene _this,
                                           LogicSimpleData logic,
                                           SceneSimpleData scene,
                                           Vector2 pos,
                                           Vector2 dir)
        {
            var autoPlayer = new AutoPlayer();
            autoPlayer.Init(_this.GenerateNextId(), logic, scene);

            if (!_this.ValidPosition(pos))
            {
                Logger.Error("CreateAutoPlayer postion is invalid. PlayerId={2},name={0}, pos={1}", autoPlayer.GetName(),
                    pos, logic.Id);
            }

            autoPlayer.SetPosition(pos.X, pos.Y);
            autoPlayer.SetDirection(dir);

            _this.EnterScene(autoPlayer);
            return autoPlayer;
        }

        public AutoPlayer CreateAutoPlayer(Scene _this, int RobotId, Vector2 pos, Vector2 dir)
        {
            var autoPlayer = new AutoPlayer();
            autoPlayer.InitByRobot(_this.GenerateNextId(), RobotId);

            if (!_this.ValidPosition(pos))
            {
                Logger.Error("Npc postion is invalid. RobotId={2},name={0}, pos={1}", autoPlayer.GetName(), pos, RobotId);
            }

            autoPlayer.SetPosition(pos.X, pos.Y);
            autoPlayer.SetDirection(dir);

            _this.EnterScene(autoPlayer);
            return autoPlayer;
        }

        public ObjNPC CreateNpc(Scene _this, SceneNpcRecord sceneNpcRecord, int dataId, Vector2 pos, Vector2 dir, string name = "", int level = -1)
        {
            if (Math.Abs(pos.X+1) <0.0001f && Math.Abs(pos.Y+1) <0.0001f)
            {
                return null;
            }
            if (dataId != 999 && dataId != 998)
            {
                if (!_this.ValidPosition(pos))
                {
                    Logger.Warn("Npc postion is invalid. DataId={0},pos={1},scene={2}", dataId, pos, _this.TypeId);

                    var temp = _this.FindNearestValidPosition(pos, 10);
                    if (null != temp)
                    {
                        pos = temp.Value;
                    }
                    else
                    {
                        Logger.Error("null==FindNearestValidPosition. DataId={0},pos={1},scene={2}", dataId, pos,_this.TypeId);
                        return null;
                    }
                }
            }

            var npc = new ObjNPC();
            npc.tbSceneNpc = sceneNpcRecord;
            npc.Init(_this.GenerateNextId(), dataId, level);
            npc.mBornTime = DateTime.Now;

            if (!string.IsNullOrEmpty(name))
            {
                npc.SetName(name);
            }
            //if (level > 0)840824

            //{
            //    npc.SetLevel(level);
            //}

            npc.SetPosition(pos.X, pos.Y);
            npc.SetDirection(dir);
            npc.BornPosition = pos;
            npc.BornDirection = dir;
            _this.EnterScene(npc);
            return npc;
        }

        //刷新怪物
        public ObjNPC CreateSceneNpc(Scene _this, int sceneNpcId, Vector2 pos = default(Vector2), int level = -1)
        {
            var record = Table.GetSceneNpc(sceneNpcId);
            var dataId = record.DataID;
            if (pos == default(Vector2))
            {
                if (Math.Abs(record.PosX - (-1)) > Double.Epsilon && Math.Abs(record.PosZ - (-1)) > Double.Epsilon)
                {
                    pos = new Vector2((float) record.PosX, (float) record.PosZ);
                }
                else
                {
                    pos = Utility.GetRandomPosFromTable(record.RandomStartID, record.RandomEndID);
                }
            }
            var dir = new Vector2((float) Math.Cos(record.FaceDirection), (float) Math.Sin(record.FaceDirection));
			var npc = _this.CreateNpc(record, dataId, pos, dir, "", level);
            return npc;
        }

        //创建一个 来自WorldBOSS表 的 特殊怪物
        public ObjNPC CreateSpeMonster(Scene _this, WorldBOSSRecord record)
        {
            var alwaysRecords = _this.SpeMonsterAlwaysRecords;
            var afterDieRecords = _this.SpeMonsterAfterDieRecordIds;
            if (record.RefleshRole == (int) eSpeMonsterRefreshType.OnTime)
            {
//如果是一直刷新，那么要判断一下当前存活的数量
                if (alwaysRecords.ContainsKey(record.Id) && alwaysRecords[record.Id] >= record.MaxCount)
                {
                    return null;
                }
                alwaysRecords.modifyValue(record.Id, 1);
            }
            else if (record.RefleshRole == (int) eSpeMonsterRefreshType.AfterDie)
            {
//死后自动刷新的怪，所以如果该场景已经有造过该record id，就不再重造了
                if (afterDieRecords.Contains(record.Id))
                {
                    return null;
                }
                afterDieRecords.Add(record.Id);
            }

            var obj = _this.CreateSceneNpc(record.SceneNpc);
	        if (obj == null)
	        {
		        PlayerLog.WriteLog((ulong) LogType.SpeMonster, "In CreateSpeMonster obj == null,scene npc id = {0}",
			        record.SceneNpc);
	        }
	        else
	        {
				Logger.Info("ObjNPC CreateSpeMonster({0})", record.SceneNpc);
	        }
            if (record.RefleshRole == (int) eSpeMonsterRefreshType.OnTime)
            {
//如果是一直刷新，则要保存一下这个怪物
                Log(_this, Logger, "OnTime monster created!SceneNpc = {0}", record.SceneNpc);
                _this.SpeMonsterToWorldBossId.Add(obj, record.Id);
            }

            return obj;
        }

        public void CreateSpeMonsters(Scene _this, List<int> worldBossIds)
        {
            foreach (var id in worldBossIds)
            {
                var record = Table.GetWorldBOSS(id);
                if (record == null)
                {
                    continue;
                }

                var tbSceneNpc = Table.GetSceneNpc(record.SceneNpc);
                if (tbSceneNpc.SceneID != _this.TypeId)
                {
                    continue;
                }

                _this.CreateSpeMonster(record);
            }
        }

        public ObjBoss CreateBoss(Scene _this, int dataId, Vector2 pos, Vector2 dir, string name = "", int level = -1)
        {
            var npc = new ObjBoss();
            npc.Init(_this.GenerateNextId(), dataId, level);

            if (!string.IsNullOrEmpty(name))
            {
                npc.SetName(name);
            }
            //if (level > 0)
            //{
            //    npc.SetLevel(level);
            //}

            if (!_this.ValidPosition(pos))
            {
                Logger.Warn("Npc postion is invalid. DataId={2},name={0},pos={1},scene={3}", npc.GetName(), pos, dataId,
                    _this.TypeId);
            }

            npc.SetPosition(pos.X, pos.Y);
            npc.SetDirection(dir);
            npc.BornPosition = pos;
            npc.BornDirection = dir;
            _this.EnterScene(npc);
            return npc;
        }

        //刷新Boss
        public ObjBoss CreateSceneBoss(Scene _this, int sceneNpcId, Vector2 pos = default(Vector2))
        {
            var record = Table.GetSceneNpc(sceneNpcId);
            var dataId = record.DataID;
            if (pos == default(Vector2))
            {
                if (Math.Abs(record.PosX - (-1)) > Double.Epsilon && Math.Abs(record.PosZ - (-1)) > Double.Epsilon)
                {
                    pos = new Vector2((float) record.PosX, (float) record.PosZ);
                }
                else
                {
                    pos = Utility.GetRandomPosFromTable(record.RandomStartID, record.RandomEndID);
                }
            }
            var dir = new Vector2((float) Math.Cos(record.FaceDirection), (float) Math.Sin(record.FaceDirection));
            var npc = _this.CreateBoss(dataId, pos, dir);
            return npc;
        }

        public ObjRetinue CreateRetinue(Scene _this,
                                        int dataId,
                                        ObjCharacter owner,
                                        Vector2 pos,
                                        Vector2 dir,
                                        int camp,
                                        int level = -1)
        {
            if (Table.GetNpcBase(dataId) == null)
            {
                Logger.Error("CreateRetinue retinue = {0},objId={1}", dataId, owner.ObjId);
                return null;
            }
            var npc = new ObjRetinue();
            npc.Init(_this.GenerateNextId(), dataId, owner);
            npc.SetPosition(pos.X, pos.Y);
            npc.SetDirection(dir);
            if (level > 0)
            {
                npc.SetLevel(level);
                npc.InitData(level);
            }
            else
            {
                npc.InitData(level);
            }
            npc.SetCamp(camp);
            _this.EnterScene(npc);
            return npc;
        }

        public ObjDropItem CreateDropItem(Scene _this,
                                          int type,
                                          List<ulong> ownerList,
                                          ulong teamId,
                                          int itemId,
                                          int count,
                                          Vector2 pos)
        {
	        if (count <= 0)
	        {
		        return null;
	        }
            var item = new ObjDropItem(type, ownerList, teamId, itemId, count);
            item.InitBase(_this.GenerateNextId(), itemId);

            PlayerLog.WriteLog((int) LogType.DropItem, "CreateDropItem  Id ={0} SceneId={1} pos={2},{3}", item.ObjId,
                _this.TypeId, pos.X, pos.Y);
            var randomRadian = MyRandom.Random()*2*Math.PI;

            const float MaxDistance = 10f;
            const float MinDistance = 6f;

            var distance = MyRandom.Random((int) MinDistance, (int) MaxDistance)/10.0f;
            var targetPos = pos +
                            new Vector2(distance*(float) Math.Cos(randomRadian), distance*(float) Math.Sin(randomRadian));
            if (SceneObstacle.ObstacleValue.Obstacle == _this.GetObstacleValue(targetPos.X, targetPos.Y))
            {
                item.SetPosition(pos);
            }
            else
            {
                item.SetPosition(targetPos);
            }
            item.OrginPos = pos;
            _this.EnterScene(item);

            return item;
        }
    }

    public partial class Scene
    {
        public AutoPlayer CreateAutoPlayer(LogicSimpleData logic, SceneSimpleData scene, Vector2 pos, Vector2 dir)
        {
            return mImpl.CreateAutoPlayer(this, logic, scene, pos, dir);
        }

        public AutoPlayer CreateAutoPlayer(int RobotId, Vector2 pos, Vector2 dir)
        {
            return mImpl.CreateAutoPlayer(this, RobotId, pos, dir);
        }

        public ObjBoss CreateBoss(int dataId, Vector2 pos, Vector2 dir, string name = "", int level = -1)
        {
            return mImpl.CreateBoss(this, dataId, pos, dir, name, level);
        }

        public ObjDropItem CreateDropItem(int type,
                                          List<ulong> ownerList,
                                          ulong teamId,
                                          int itemId,
                                          int count,
                                          Vector2 pos)
        {
            return mImpl.CreateDropItem(this, type, ownerList, teamId, itemId, count, pos);
        }

        public ObjNPC CreateNpc(SceneNpcRecord sceneNpcRecord, int dataId, Vector2 pos, Vector2 dir, string name = "", int level = -1)
        {
            return mImpl.CreateNpc(this, sceneNpcRecord, dataId, pos, dir, name, level);
        }

        public ObjRetinue CreateRetinue(int dataId,
                                        ObjCharacter owner,
                                        Vector2 pos,
                                        Vector2 dir,
                                        int camp,
                                        int level = -1)
        {
            return mImpl.CreateRetinue(this, dataId, owner, pos, dir, camp, level);
        }

        //刷新Boss
        public ObjBoss CreateSceneBoss(int sceneNpcId, Vector2 pos = default(Vector2))
        {
            return mImpl.CreateSceneBoss(this, sceneNpcId, pos);
        }

        //刷新怪物
        public ObjNPC CreateSceneNpc(int sceneNpcId, Vector2 pos = default(Vector2), int level = -1)
        {
            return mImpl.CreateSceneNpc(this, sceneNpcId, pos, level);
        }

        //创建一个 来自WorldBOSS表 的 特殊怪物
        public ObjNPC CreateSpeMonster(WorldBOSSRecord record)
        {
            return mImpl.CreateSpeMonster(this, record);
        }

        public void CreateSpeMonsters(List<int> worldBossIds)
        {
            mImpl.CreateSpeMonsters(this, worldBossIds);
        }

        public virtual bool EnterScene(ObjBase obj)
        {
            return mImpl.EnterScene(this, obj);
        }

        public void InitMapNPC()
        {
            mImpl.InitMapNPC(this);
        }

        public virtual bool InitNPC()
        {
            return mImpl.InitNPC(this);
        }

        public virtual bool LeaveScene(ObjBase obj, ReasonType reason = ReasonType.VisibilityChanged)
        {
            return mImpl.LeaveScene(this, obj, reason);
        }
    }
}