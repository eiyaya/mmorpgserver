#region using

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Database;
using DataContract;
using DataTable;
using Mono.GameMath;
using Scorpion;
using NLog;
using SceneServerService;

using Shared;

#endregion

namespace Scene
{
    public class SceneServerControlDefaultImpl : ISceneService, IStaticSceneServerControl, ITickable
    {
        //获得数据接口
        [Updateable("scene")]
        public static List<CharacterSimpleData> findlist = new List<CharacterSimpleData>();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Logger kafaLogger = LogManager.GetLogger(Shared.LoggerName.KafkaLog);

        public void NotifyGateClientState(SceneService _this, ulong clientId, ulong characterId)
        {
            var gateDesc = new ServiceDesc();
            gateDesc.Type = (int) MessageType.CharacterConnetServer;
            gateDesc.CharacterId = characterId;
            gateDesc.ServiceType = (int) ServiceType.Scene;
            gateDesc.ClientId = clientId;
            //SceneServerControl.
            _this.Send(gateDesc);
        }

      
        public IEnumerator OnConnected(Coroutine coroutine, SceneCharacterProxy charProxy, AsyncReturnValue<bool> ret)
        {
            ret.Value = false;
            var proxy = (SceneProxy) charProxy;
            //ConnectLostLogger.Info("character {0} - {1} Scene OnConnected 1", proxy.CharacterId, proxy.ClientId);
            Logger.Info("[{0}] has enter connected", proxy.CharacterId);
            PlayerLog.WriteLog(proxy.CharacterId, "-----Scene-----OnConnected----------{0}", proxy.CharacterId);
            Logger.Info("Enter Game {0} - OnConnected - 1 - {1}", proxy.CharacterId,
                TimeManager.Timer.ElapsedMilliseconds);
            PlayerLog.WriteLog(888, "OnConnected characterId={0},clientId={1}", proxy.CharacterId, proxy.ClientId);

            var result = AsyncReturnValue<ObjPlayer>.Create();
            var co = CoroutineFactory.NewSubroutine(CharacterManager.Instance.GetOrCreateCharacterController, coroutine,
                proxy.CharacterId, new object[] {}, false, result);
            if (co.MoveNext())
            {
                yield return co;
            }

            Logger.Info("Enter Game {0} - OnConnected - 2 - {1}", proxy.CharacterId,
                TimeManager.Timer.ElapsedMilliseconds);

            var obj = result.Value;
            result.Dispose();
//             ObjPlayer obj = CharacterManager.Instance.GetOrCreateCharacterController(proxy.CharacterId);
            if (obj == null)
            {
                Logger.Fatal("onConnected CharacterId={0}", proxy.CharacterId);
                yield break;
            }
            proxy.Character = obj;

            obj.Proxy = proxy;

            //同步名字
            var dbLoginSimple = SceneServer.Instance.LoginAgent.GetLoginSimpleData(proxy.ClientId, proxy.CharacterId);
            yield return dbLoginSimple.SendAndWaitUntilDone(coroutine);
            if (dbLoginSimple.State != MessageState.Reply)
            {
                yield break;
            }
            if (dbLoginSimple.ErrorCode != (int) ErrorCodes.OK)
            {
                yield break;
            }
            if (obj.Proxy == null)
            {
                Logger.Warn("Scene OnConnected obj.Proxy is null! type=1, objId={0}", obj.ObjId);
                yield break;
            }
            obj.SetName(dbLoginSimple.Response.Name);
            CharacterManager.playerName.Push(obj.GetName(), obj);
            //同步技能数据
            var skills = SceneServer.Instance.LogicAgent.LogicGetSkillData(proxy.CharacterId, proxy.CharacterId);
            yield return skills.SendAndWaitUntilDone(coroutine);
            if (skills.State != MessageState.Reply)
            {
                yield break;
            }
            if (skills.ErrorCode != (int) ErrorCodes.OK)
            {
                yield break;
            }

            if (obj.Proxy == null)
            {
                Logger.Warn("Scene OnConnected obj.Proxy is null! type=2, objId={0}", obj.ObjId);
                yield break;
            }

            Logger.Info("Enter Game {0} - OnConnected - 3 - {1}", proxy.CharacterId,
                TimeManager.Timer.ElapsedMilliseconds);

            obj.ApplySkill(skills.Response.Data);
            obj.ChangeOutLineTime();
            //同步装备数据
            var equips = SceneServer.Instance.LogicAgent.LogicGetEquipList(proxy.CharacterId, proxy.CharacterId);
            yield return equips.SendAndWaitUntilDone(coroutine);
            if (equips.State != MessageState.Reply)
            {
                yield break;
            }
            if (equips.ErrorCode != (int) ErrorCodes.OK)
            {
                yield break;
            }
            if (obj.Proxy == null)
            {
                Logger.Warn("Scene OnConnected obj.Proxy is null! type=3, objId={0}", obj.ObjId);
                yield break;
            }
            obj.ApplyEquip(equips.Response);

            Logger.Info("Enter Game {0} - OnConnected - 4 - {1}", proxy.CharacterId,
                TimeManager.Timer.ElapsedMilliseconds);

            //同步天赋数据
            var talents = SceneServer.Instance.LogicAgent.LogicGetTalentData(proxy.CharacterId, proxy.CharacterId);
            yield return talents.SendAndWaitUntilDone(coroutine);
            if (talents.State != MessageState.Reply)
            {
                yield break;
            }
            if (talents.ErrorCode != (int) ErrorCodes.OK)
            {
                yield break;
            }
            if (obj.Proxy == null)
            {
                Logger.Warn("Scene OnConnected obj.Proxy is null! type=4, objId={0}", obj.ObjId);
                yield break;
            }
            obj.ApplyTalent(talents.Response.Data);

            Logger.Info("Enter Game {0} - OnConnected - 5 - {1}", proxy.CharacterId,
                TimeManager.Timer.ElapsedMilliseconds);

            //同步图鉴数据
            var books = SceneServer.Instance.LogicAgent.LogicGetBookAttrData(proxy.CharacterId, proxy.CharacterId);
            yield return books.SendAndWaitUntilDone(coroutine);
            if (books.State != MessageState.Reply)
            {
                yield break;
            }
            if (books.ErrorCode != (int) ErrorCodes.OK)
            {
                yield break;
            }
            if (obj.Proxy == null)
            {
                Logger.Warn("Scene OnConnected obj.Proxy is null! type=5, objId={0}", obj.ObjId);
                yield break;
            }
            obj.ApplyBookAttr(books.Response.bookAttrs,books.Response.monsterAttrs);
	        obj.SetBookMonsterId(books.Response.fightId);

            Logger.Info("Enter Game {0} - OnConnected - 6 - {1}", proxy.CharacterId,
                TimeManager.Timer.ElapsedMilliseconds);

            //同步称号数据
            var titles = SceneServer.Instance.LogicAgent.LogicGetTitleList(proxy.CharacterId, 0);
            yield return titles.SendAndWaitUntilDone(coroutine);
            if (titles.State != MessageState.Reply)
            {
                yield break;
            }
            if (titles.ErrorCode != (int) ErrorCodes.OK)
            {
                yield break;
            }
            if (obj.Proxy == null)
            {
                Logger.Warn("Scene OnConnected obj.Proxy is null! type=6, objId={0}", obj.ObjId);
                yield break;
            }
            obj.ApplyTitles(titles.Response.EquipedTitles.Items, 0);
            obj.ApplyTitles(titles.Response.Titles.Items, 1);

            Logger.Info("Enter Game {0} - OnConnected - 7 - {1}", proxy.CharacterId,
                TimeManager.Timer.ElapsedMilliseconds);

            //同步队伍数据
            var team = SceneServer.Instance.TeamAgent.SSGetTeamData(proxy.CharacterId, proxy.CharacterId);
            yield return team.SendAndWaitUntilDone(coroutine);
            if (team.State == MessageState.Reply)
            {
                if (team.ErrorCode == (int) ErrorCodes.OK)
                {
                    obj.SetTeamId(team.Response.TeamId, team.Response.State);
                }
            }
            if (obj.Proxy == null)
            {
                Logger.Warn("Scene OnConnected obj.Proxy is null! type=7, objId={0}", obj.ObjId);
                yield break;
            }
            Logger.Info("Enter Game {0} - OnConnected - 8 - {1}", proxy.CharacterId,
                TimeManager.Timer.ElapsedMilliseconds);

            //获取viplevel
            var vip = SceneServer.Instance.LogicAgent.GetItemCount(proxy.CharacterId, (int) eResourcesType.VipLevel);
            yield return vip.SendAndWaitUntilDone(coroutine);
            if (vip.State != MessageState.Reply)
            {
                yield break;
            }
            if (vip.ErrorCode != (int) ErrorCodes.OK)
            {
                yield break;
            }
            if (obj.Proxy == null)
            {
                Logger.Warn("Scene OnConnected obj.Proxy is null! type=8, objId={0}", obj.ObjId);
                yield break;
            }
            obj.SetItemCount((int) eResourcesType.VipLevel, vip.Response);


            Logger.Info("Enter Game {0} - OnConnected - 9 - {1}", proxy.CharacterId,
                TimeManager.Timer.ElapsedMilliseconds);

            //同步战盟数据
            var alliance = SceneServer.Instance.TeamAgent.SSGetAllianceData(proxy.CharacterId, obj.ServerId);
            yield return alliance.SendAndWaitUntilDone(coroutine);
            if (obj.Proxy == null)
            {
                Logger.Warn("Scene OnConnected obj.Proxy is null! type=9, objId={0}", obj.ObjId);
                yield break;
            }
            if (alliance.State == MessageState.Reply)
            {
                if (alliance.ErrorCode == (int) ErrorCodes.OK)
                {
                    var response = alliance.Response;
                    obj.SetAllianceInfo(response.AllianceId, response.Ladder, response.Name);
                }
            }

            Logger.Info("Enter Game {0} - OnConnected - 10 - {1}", proxy.CharacterId,
                TimeManager.Timer.ElapsedMilliseconds);

            //同步战盟Buff数据
            var allianceBuff = SceneServer.Instance.LogicAgent.SSGetAllianceBuff(proxy.CharacterId, 0);
            yield return allianceBuff.SendAndWaitUntilDone(coroutine);
            if (obj.Proxy == null)
            {
                Logger.Warn("Scene OnConnected obj.Proxy is null! type=10, objId={0}", obj.ObjId);
                yield break;
            }
            if (allianceBuff.State == MessageState.Reply)
            {
                if (allianceBuff.ErrorCode == (int) ErrorCodes.OK)
                {
                    foreach (var buffId in allianceBuff.Response.Items)
                    {
                        if (buffId <= 0)
                        {
                            continue;
                        }
                        var buff = Table.GetGuildBuff(buffId);
                        if (buff == null)
                        {
                            continue;
                        }
                        obj.AddBuff(buff.BuffID, buff.BuffLevel, obj);
                    }
                }
            }

            obj.Attr.mFlag.ReSetAllFlag(true);
            obj.Attr.EquipRefresh();
            obj.Attr.InitAttributesAll();
            obj.Attr.SetFightPointFlag();

            Logger.Info("Enter Game {0} - OnConnected - 11 - {1}", proxy.CharacterId,
                TimeManager.Timer.ElapsedMilliseconds);

            //同步好友数据
            var friend = SceneServer.Instance.LogicAgent.SSGetFriendList(proxy.CharacterId, 0);
            yield return friend.SendAndWaitUntilDone(coroutine);
            if (obj.Proxy == null)
            {
                Logger.Warn("Scene OnConnected obj.Proxy is null! type=11, objId={0}", obj.ObjId);
                yield break;
            }
            if (friend.State == MessageState.Reply)
            {
                if (friend.ErrorCode == (int) ErrorCodes.OK)
                {
                    if (friend.Response.Data.Count > 0)
                    {
                        var fl = new Uint64Array();
                        foreach (var dic in friend.Response.Data)
                        {
                            foreach (var i in dic.Value.Items)
                            {
                                fl.Items.Add(i);
                            }
                        }
                        var Sbfriend = SceneServer.Instance.SceneAgent.SBCheckCharacterOnline(proxy.CharacterId, fl);
                        yield return Sbfriend.SendAndWaitUntilDone(coroutine);
                        if (Sbfriend.State == MessageState.Reply)
                        {
                            if (Sbfriend.ErrorCode == (int) ErrorCodes.OK)
                            {
                                var index = 0;
                                foreach (var dic in friend.Response.Data)
                                {
                                    var friendType = dic.Key;
                                    foreach (var i in dic.Value.Items)
                                    {
                                        if (Sbfriend.Response.Items[index] == 1)
                                        {
                                            obj.PushFriend(friendType, i);
                                        }
                                        index++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            SceneManager.Instance.CheckAvgLevelBuff(obj);

            {//判断终生卡状态
                var lifeCardMsg = SceneServer.Instance.LogicAgent.SSGetFlagOrCondition(proxy.CharacterId, proxy.CharacterId,
                   2682, -1);
                yield return lifeCardMsg.SendAndWaitUntilDone(coroutine);

                if (lifeCardMsg.State != MessageState.Reply)
                {
                    Logger.Info("SSGetFlagOrCondition   msg.State != MessageState.Reply");
                    yield break;
                }

                if (lifeCardMsg.ErrorCode != (int)ErrorCodes.OK)
                {
                    Logger.Info("SSGetFlagOrCondition  msg.ErrorCode != (int)ErrorCodes.OK");
                    yield break;
                }
                var lifeCardFlag = lifeCardMsg.Response;
                if (lifeCardFlag == 1)
                {
                    SceneManager.Instance.CheckAddLifeCardBuff(obj);
                }
            }

            Logger.Info("Enter Game {0} - OnConnected - 12 - {1}", proxy.CharacterId,
                TimeManager.Timer.ElapsedMilliseconds);

            if (obj.mDbData.Hp < 1)
            {
                var AutoReliveTime = DateTime.FromBinary(obj.mDbData.AutoRelive);
                if (AutoReliveTime < DateTime.Now)
                {
                    //需要复活
                    Logger.Info("[{0}] Need AutoRelive", proxy.CharacterId);
                    obj.Attr.SetDataValue(eAttributeType.HpNow, obj.mDbData.Hp);
                    obj.Attr.SetDataValue(eAttributeType.MpNow, obj.mDbData.Mp);
                }
                else
                {
                    obj.Attr.SetDataValue(eAttributeType.HpNow, obj.mDbData.Hp);
                    obj.Attr.SetDataValue(eAttributeType.MpNow, obj.mDbData.Mp);
                    obj.OnlineDie();
                }
            }
            else
            {
                obj.Attr.SetDataValue(eAttributeType.HpNow, obj.mDbData.Hp);
                if (obj.mDbData.Mp < 1)
                {
                    obj.Attr.SetDataValue(eAttributeType.MpNow, obj.GetAttribute(eAttributeType.MpMax));
                }
                else
                {
                    obj.Attr.SetDataValue(eAttributeType.MpNow, obj.mDbData.Mp);
                }
            }
            {
                Logger.Info("Enter Game {0} - OnConnected - 13 - {1}", proxy.CharacterId,
                    TimeManager.Timer.ElapsedMilliseconds);

                Logger.Info("[{0}] has connected", proxy.CharacterId);

                proxy.Connected = true;
                if (proxy.Character != null)
                {
                    proxy.Character.State = CharacterState.Connected;
                }
                CharacterManager.Instance.UpdateSimpleData(proxy.CharacterId);
                Logger.Info("Enter Game {0} - OnConnected - 14 - {1}", proxy.CharacterId,
                    TimeManager.Timer.ElapsedMilliseconds);

                //同步精灵数据
                var elf = SceneServer.Instance.LogicAgent.LogicGetElfData(proxy.CharacterId, proxy.CharacterId);
                yield return elf.SendAndWaitUntilDone(coroutine);
                if (elf.State != MessageState.Reply)
                {
                    yield break;
                }
                if (elf.ErrorCode != (int)ErrorCodes.OK)
                {
                    yield break;
                }

                if (obj.Proxy == null)
                {
                    Logger.Warn("Scene OnConnected obj.Proxy is null! type=2, objId={0}", obj.ObjId);
                    yield break;
                }

                Logger.Info("Enter Game {0} - OnConnected - 15 - {1}", proxy.CharacterId, TimeManager.Timer.ElapsedMilliseconds);
                obj.ApplyElf(elf.Response);
                
                {//同步坐骑数据
                    var mount = SceneServer.Instance.LogicAgent.LogicGetMountData(proxy.CharacterId, proxy.CharacterId);
                    yield return mount.SendAndWaitUntilDone(coroutine);
                    if (mount.State != MessageState.Reply)
                    {
                        yield break;
                    }
                    if (mount.ErrorCode != (int)ErrorCodes.OK)
                    {
                        yield break;
                    }

                    if (obj.Proxy == null)
                    {
                        Logger.Warn("Scene OnConnected obj.Proxy is null! type=2, objId={0}", obj.ObjId);
                        yield break;
                    }
                    obj.ApplyMountData(mount.Response);

                }
                Logger.Info("Enter Game {0} - OnConnected - 15 - {1}", proxy.CharacterId, TimeManager.Timer.ElapsedMilliseconds);

                ret.Value = true;

                var msg1 = SceneServer.Instance.GameMasterAgent.CharacterConnected(proxy.CharacterId, proxy.CharacterId,
                    (int) ServiceType.Scene);
                yield return msg1.SendAndWaitUntilDone(coroutine);
            }
        }

        private void OnEnterScenenCharacter(ObjPlayer character,
                                            ulong sceneGuid,
                                            ulong applyGuid,
                                            int changeType,
                                            SceneParam param = null)
        {
            Logger.Info("Enter Game {0} guid:{1} applyGuid:{2} changeType:{3} - OnEnterScenenCharacter - 1 - {4}",
                character.ObjId, sceneGuid, applyGuid, changeType, TimeManager.Timer.ElapsedMilliseconds);
            Logger.Info("Enter the scene in this server. character:{0} , scene guid:{1}", character.ObjId, sceneGuid);

            var scene = SceneManager.Instance.GetScene(sceneGuid);
            if (scene == null)
            {
                if (SceneManager.Instance.Scenes.Count > 0)
                {
                    scene = SceneManager.Instance.Scenes.First().Value;
                }
                else
                {
                    SceneServer.Instance.SceneAgent.NotifySceneNotExist(character.ObjId, sceneGuid, character.ObjId);
                    Logger.Error("Can not find scene: " + sceneGuid + " serverid:" + SceneServer.Instance.Id);
                    return;
                }
            }
            var eChangeType = (eScnenChangeType) changeType;
            if (eChangeType != eScnenChangeType.Login
                && eChangeType != eScnenChangeType.LoginRelive
                && eChangeType != eScnenChangeType.ExitCity
                && eChangeType != eScnenChangeType.ExitDungeon)
            {
                //登录游戏，退出副本之类的切换场景不需要记录改上个场景信息
                character.SaveBeforeScene();

                Logger.Info("Enter Game {0} pos:{1} - OnEnterScenenCharacter - 2 - {2}", character.ObjId,
                    character.GetPosition(), TimeManager.Timer.ElapsedMilliseconds);
            }
            var postionType = eScnenChangePostion.Db;
            switch (eChangeType)
            {
                case eScnenChangeType.Login:
                {
                    if (applyGuid == sceneGuid)
                    {
                        postionType = eScnenChangePostion.Db;
                    }
                    else
                    {
                        if (scene.TypeId == (int) character.GetData().SceneId)
                        {
                            postionType = eScnenChangePostion.Db;
                        }
                        else
                        {
                            postionType = eScnenChangePostion.Former;
                        }
                    }
                }
                    break;
                case eScnenChangeType.Normal:
                case eScnenChangeType.EnterCity:
                case eScnenChangeType.LoginRelive:
                case eScnenChangeType.EnterDungeon:
                {
                    postionType = eScnenChangePostion.Table;
                    if (scene.TableSceneData.FubenId != -1)
                    {
                        var tbFuben = Table.GetFuben(scene.TableSceneData.FubenId);
                        if (tbFuben.MainType == (int) eDungeonMainType.PhaseFuben)
                        {
                            postionType = eScnenChangePostion.FormerNear;
                        }
                    }
                }
                    break;
                case eScnenChangeType.ExitCity:
                case eScnenChangeType.ExitDungeon:
                {
                    postionType = eScnenChangePostion.Former;
                }
                    break;
                case eScnenChangeType.TeamDungeon:
                {
                    if (applyGuid != sceneGuid)
                    {
                        postionType = eScnenChangePostion.Table;
                    }
                    else
                    {
                        postionType = eScnenChangePostion.Db;
                    }
                }
                    break;
                case eScnenChangeType.Transfer:
                {
                    postionType = eScnenChangePostion.Transfer;
                }
                    break;
                case eScnenChangeType.Position:
                {
                    postionType = eScnenChangePostion.Position;
                }
                    break;
                case eScnenChangeType.None:
                {
                    postionType = eScnenChangePostion.None;
                }
                    break;
                default:
                {
                    postionType = eScnenChangePostion.Table;
                }
                    break;
            }


            Logger.Info("Enter Game {0} posType:{1} - OnEnterScenenCharacter - 3 - {2}", character.ObjId, postionType,
                TimeManager.Timer.ElapsedMilliseconds);

            switch (postionType)
            {
                case eScnenChangePostion.Db:
                {
                    var db = character.GetData();
                    character.SetPosition(db.Postion.X, db.Postion.Y);
                }
                    break;
                case eScnenChangePostion.Former:
                {
                    var db = character.GetData();
                    character.SetPosition(db.FormerPostion.X, db.FormerPostion.Y);
                }
                    break;
                case eScnenChangePostion.Table:
                {
                    var tbScene = Table.GetScene(scene.TypeId);
                    var pos = new Vector2((float)tbScene.Entry_x, (float)tbScene.Entry_z);
                    if (tbScene.Type == (int) eSceneType.BossHome)
                    {
                        var tbFuben = Table.GetFuben(tbScene.FubenId);
                        if (tbFuben != null)
                        {
                            var idx = MyRandom.Random(tbFuben.lParam1.Count);
                            var tbPos = Table.GetRandomCoordinate(tbFuben.lParam1[idx]);
                            if (tbPos != null)
                            {
                                pos = new Vector2(tbPos.PosX, tbPos.PosY);
                            }
                        }
                    }
                    else
                    {
                        pos = Utility.RandomMieShiEntryPosition((eSceneType) tbScene.Type, scene.TypeId, pos);
                    }
                    character.SetPosition(pos);
                }
                    break;
                case eScnenChangePostion.Transfer:
                {
                    var transferId = param.Param[0];
                    var table = Table.GetTransfer(transferId);
                    if (null == table)
                    {
                        var tbScene = Table.GetScene(scene.TypeId);
                        var pos = new Vector2((float) tbScene.Entry_x, (float) tbScene.Entry_z);
                        pos = Utility.RandomMieShiEntryPosition((eSceneType) tbScene.Type, scene.TypeId, pos);
                        character.SetPosition(pos);
                        Logger.Fatal("ERROR:Table.GetScene({0})==nul", transferId);
                    }
                    else
                    {
                        character.SetPosition(table.ToX, table.ToY);
                    }
                }
                    break;
                case eScnenChangePostion.Position:
                {
                    character.SetPosition(param.Param[0], param.Param[1]);
                }
                    break;
                case eScnenChangePostion.FormerNear:
                {
                    var db = character.GetData();
                    var tbFromScene = Table.GetScene((int) db.FormerSceneId);
                    if (tbFromScene == null)
                    {
                        character.SetPosition((float) scene.TableSceneData.Entry_x, (float) scene.TableSceneData.Entry_z);
                        break;
                    }
                    if (tbFromScene.ResName != scene.TableSceneData.ResName)
                    {
                        character.SetPosition((float) scene.TableSceneData.Entry_x, (float) scene.TableSceneData.Entry_z);
                        break;
                    }
                    if (Math.Abs(scene.TableSceneData.Entry_x - db.FormerPostion.X) > 10 ||
                        Math.Abs(scene.TableSceneData.Entry_z - db.FormerPostion.Y) > 10)
                    {
                        character.SetPosition((float) scene.TableSceneData.Entry_x, (float) scene.TableSceneData.Entry_z);
                    }
                    else
                    {
                        character.SetPosition(db.FormerPostion.X, db.FormerPostion.Y);
                    }
                }
                    break;
                case eScnenChangePostion.None:
                {
                }
                    break;
                default:
                    break;
            }

            Logger.Info("Enter Game {0} pos:{1} - OnEnterScenenCharacter - 4 - {2}", character.ObjId,
                character.GetPosition(), TimeManager.Timer.ElapsedMilliseconds);

            character.UpdataSceneInfoData((uint) scene.TypeId, scene.Guid);
            if (scene.TableSceneData.IsCanRide == 0)
            {
                character.SetMountId(0);
            }
            if (character.Proxy != null)
            {

                var data = new PlayerData();
                data.CharacterId = character.ObjId;
                data.SceneId = scene.TypeId;
                data.Camp = character.GetCamp();
                data.Level = character.Attr.GetDataValue(eAttributeType.Level);
                data.Name = character.GetName();
                data.RoleId = character.TypeId;

                var pos = character.GetPosition();
                if (!scene.ValidPosition(pos))
                {
                    var temp = scene.FindNearestValidPosition(pos);
                    if (temp == null)
                    {
                        character.SetPosition((float) scene.TableSceneData.Safe_x, (float) scene.TableSceneData.Safe_z);
                    }
                    else
                    {
                        character.SetPosition(temp.Value);
                    }
                    pos = character.GetPosition();
                }

                data.X = pos.X;
                data.Y = pos.Y;
                data.IsDead = character.IsDead() ? 1 : 0;
                data.HpMax = character.Attr.GetDataValue(eAttributeType.HpMax);
                data.HpNow = character.Attr.GetDataValue(eAttributeType.HpNow);
                data.MpMax = character.Attr.GetDataValue(eAttributeType.MpMax);
                data.MpMow = character.Attr.GetDataValue(eAttributeType.MpNow);

                data.AreaState = (int) character.UpdateAreaState(scene, false);
                data.MoveSpeed = character.GetMoveSpeed();
                data.SceneGuid = scene.Guid;
                data.ModelId = character.ModelId;
                character.GetEquipsModel(data.EquipsModel);
                data.MountId = character.GetMountId();

                PlayerLog.WriteLog(888, "ReplyChangeScene characterId={0},sceneGuid={1}", character.ObjId, sceneGuid);
                character.Proxy.ReplyChangeScene(data);

                Logger.Info("Enter Game {0} scene:{1} guid:{2} pos:{3}- OnEnterScenenCharacter - 5 - {4}",
                    character.ObjId, scene.TypeId, scene.Guid, pos, TimeManager.Timer.ElapsedMilliseconds);
                SceneManager.Instance.EnterScene(character, sceneGuid);

                Logger.Info("Enter Game {0} scene:{1} guid:{2} pos:{3}- OnEnterScenenCharacter - 6 - {4}",
                    character.ObjId, scene.TypeId, scene.Guid, pos, TimeManager.Timer.ElapsedMilliseconds);

                SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(30), () =>
                {
                    if (!character.Active)
                    {
                        if (character.Proxy != null)
                        {
                            SceneServer.Instance.LoginAgent.KickCharacter(character.Proxy.ClientId, 0);
                        }
                    }
                });
            }
            else
            {
                Logger.Info("Enter Game {0} - OnEnterScenenCharacter - 7 - {1}", character.ObjId,
                    TimeManager.Timer.ElapsedMilliseconds);
                Logger.Fatal("OnEnterScenenCharacter Proxy is null");
            }
        }

        public IEnumerator OnLost(Coroutine coroutine, SceneCharacterProxy charProxy)
        {
            var proxy = (SceneProxy) charProxy;
            //ConnectLostLogger.Info("character {0} - {1} Scene OnLost 1", proxy.CharacterId, proxy.ClientId);
            Logger.Info("Enter Game {0} - OnLost - 1 - {1}", proxy.CharacterId, TimeManager.Timer.ElapsedMilliseconds);
            PlayerLog.WriteLog(proxy.CharacterId, "----------Scene----------OnLost----------");

            var player = proxy.Character;
            if (player == null)
            {
//如果引用为空就就在CharacterManager里找
                player = CharacterManager.Instance.GetCharacterControllerFromMemroy(proxy.CharacterId);
                if (null == player)
                {
                    Logger.Error("OnLost null == player[{0}]", proxy.CharacterId);
                    yield break;
                }
            }

            try
            {
                player.BuffList.OnLost();
                player.FriendsLostTriggerTimeOver();

                if (null != player.Scene)
                {
                    player.Scene.LeaveScene(player);
                }
            }
            catch (Exception e)
            {
                Logger.Error("OnLost: " + e.Message);
            }

            Logger.Info("[" + proxy.CharacterId + "] has lost connection");
            //TODO
            //掉线移出该玩家，暂时不考虑重连的情况
            CharacterManager.playerName.Remove(player.GetName(), player);
            var co = CoroutineFactory.NewSubroutine(CharacterManager.Instance.RemoveCharacter, coroutine,
                proxy.CharacterId);
            if (co.MoveNext())
            {
                yield return co;
            }

            //Logger.Info("Enter Game {0} - OnLost - 2 - {1} {2} {3}", proxy.CharacterId, proxy.Character.Scene != null ? proxy.Character.Scene.TypeId : -1, proxy.Character.mDbData.SceneId, TimeManager.Timer.ElapsedMilliseconds);

            //互相引用清除掉
            player.Proxy = null;
            proxy.Character = null;

            proxy.Connected = false;
        }

        public IEnumerator UpdateServer(Coroutine coroutine, SceneService _this, UpdateServerInMessage msg)
        {
            SceneServer.Instance.UpdateManager.Update();
            return null;
        }

        public IEnumerator GMCommand(Coroutine coroutine, SceneService _this, GMCommandInMessage msg)
        {
            var request = msg.Request;
            var characterId = msg.CharacterId;
            var commands = request.Commonds.Items;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (character == null)
            {
                Logger.Error("In GMCommand character == null, id = {0}", characterId);
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }
            var errs = msg.Response.Items;
            foreach (var command in commands)
            {
                errs.Add((int) character.GmCommand(command));
            }
            msg.Reply();
        }
        public IEnumerator NodifyModifyPlayerName(Coroutine coroutine, SceneService _this, NodifyModifyPlayerNameInMessage msg)
        {
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.Request.CharacterId);
            if (character == null)
            {
                Logger.Error("In GMCommand character == null, id = {0}", msg.CharacterId);
                //msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }
            
            character.SetName(msg.Request.ModifyName);
            CharacterManager.DataItem dataItem;
            if (CharacterManager.Instance.mDictionary.TryGetValue(msg.Request.CharacterId, out dataItem))
            {
                dataItem.SimpleData.Name = msg.Request.ModifyName;
            }
            character.FriendsDirty = true;
            character.MarkDirty();

            if (character.GetTeamId() > 0)
            {
                var msg_team = SceneServer.Instance.TeamAgent.NodifyModifyPlayerName(character.ObjId, character.ServerId, msg.Request.CharacterId, character.GetTeamId(), character.GetName());
                yield return msg_team.SendAndWaitUntilDone(coroutine);
            }
            
            
        }

        public IEnumerator NotifyPlayerPickUpFubenReward(Coroutine coroutine, SceneService _this, NotifyPlayerPickUpFubenRewardInMessage msg)
        {
             var serverControl = (SceneServerControl) _this;
            var characterId = msg.CharacterId;

            var player = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (player == null)
                yield break;
            if(player.Scene == null)
                yield break;
            player.Scene.dicGetRewardPlayers[characterId] = 0;
        }


        public IEnumerator CloneCharacterDbById(Coroutine coroutine, SceneService _this, CloneCharacterDbByIdInMessage msg)
        {

            var fromId = msg.Request.FromId;
            var toId = msg.Request.ToId;
            var result = AsyncReturnValue<ObjPlayer>.Create();
            var co = CoroutineFactory.NewSubroutine(CharacterManager.Instance.GetOrCreateCharacterController, coroutine,
            fromId, new object[] { }, false, result);

            if (co.MoveNext())
            {
                yield return co;
            }

            if (result.Value == null)
            {
                msg.Reply((int)ErrorCodes.Error_PrepareEnterGameFailed);
                yield break;
            }
            result.Dispose();
            var controller = result.Value;
            var id = controller.mDbData.Id;
            controller.mDbData.Id = toId;
            var dataItem = new CharacterManager.DataItem();
            dataItem.Controller = controller;
            dataItem.SimpleData = result.Value.GetSimpleData();

            var co3 = CoroutineFactory.NewSubroutine(CharacterManager.Instance.SaveDataForClone, coroutine, toId, dataItem, true);
            if (co3.MoveNext())
            {
                yield return co3;
            }

            dataItem.Controller.mDbData.Id = id;

            msg.Reply((int)ErrorCodes.OK);
            yield break;
        }

        public IEnumerator OnServerStart(Coroutine coroutine, SceneService _this)
        {
            //Thread.Sleep(GlobalVariable.WaitToConnectTimespan);
            SceneServer.Instance.Start(_this);
            CharacterManager.Instance.Init(SceneServer.Instance.DB, DataCategory.SceneCharacter);
            StaticVariable.Init();
            InitStoreTypeMap();
            ReloadSpecilDropTable();
            SceneServerControl.TaskManager.Init(SceneServer.Instance.DB, CharacterManager.Instance, DataCategory.Scene,
                (int) SceneServer.Instance.Id, Logger, i => { });

            SceneServer.Instance.IsReadyToEnter = true;
            _this.TickDuration = SceneServerControl.Performance;
            ((SceneServerControl) _this).Watch.Start();

            _this.Started = true;
            SceneManager.WebRequestManager = new RequestManager(_this);

            Console.WriteLine("SceneServer startOver. [{0}]", SceneServer.Instance.Id);
            yield break;
        }

        public IEnumerator Tick(Coroutine co, ServerAgentBase _this)
        {
            var __this = ((SceneServerControl) _this);

            __this.TickCount++;
            __this.TickCountPerSecond++;

            var deltaTime = (__this.Watch.ElapsedMilliseconds - __this.LastTime)/1000.0f;
            __this.LastTime = __this.Watch.ElapsedMilliseconds;

            try
            {
                SceneServerControl.Timer.Update(200*SceneManager.Instance.Scenes.Count);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Tick error.");
            }

            var s1 = __this.Watch.ElapsedMilliseconds;

            try
            {
                SceneManager.Instance.Tick(deltaTime);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Tick error.");
            }

            var s2 = __this.Watch.ElapsedMilliseconds;

            try
            {
                CharacterManager.Instance.Tick();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Tick error.");
            }

            try
            {
                // 每秒Tick一次
                if (__this.TickCount%20 == 0)
                {
                    SceneServerControl.TaskManager.Tick();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "TimedTaskManager tick error");
            }

            var s3 = __this.Watch.ElapsedMilliseconds;

            var cost = (__this.Watch.ElapsedMilliseconds - __this.LastTime)/1000.0f;
            if (cost > SceneServerControl.Performance)
            {
                Logger.Warn("Scene Tick too slow...... {0} ms, {1}, {2}, {3}", cost, s1 - __this.LastTime, s2 - s1,
                    s3 - s2);
            }

            try
            {
                if (__this.TickCount % 1200 == 0)
                {
                    Dictionary<int,int> sPlayerCountStatistic = new Dictionary<int, int>();

                    foreach (var kv in sPlayerCountStatistic)
                    {
                        sPlayerCountStatistic[kv.Key] = 0;
                    }
                    var scenes = SceneManager.Instance.Scenes;

                    foreach (var kv in scenes)
                    {
                        if (!sPlayerCountStatistic.ContainsKey(kv.Value.ServerId))
                        {
                            sPlayerCountStatistic.Add(kv.Value.ServerId, 0);
                        }
                        sPlayerCountStatistic[kv.Value.ServerId] += kv.Value.PlayerCount;
                    }
                    var m = SceneServer.Instance.Id;
                    foreach (var kv in sPlayerCountStatistic)
                    {
                        string v = string.Format("server_online#{0}|{1}|{2}|{3}",
                                m.ToString(), //-机器ID
                                kv.Key.ToString(), //serverID
                                DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), // 时间
                                kv.Value); //在线数量
                        kafaLogger.Info(v);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            ((SceneServerControl) _this).LastFrameTime = deltaTime;

            return null;
        }

        public IEnumerator OnServerStop(Coroutine coroutine, SceneService _this)
        {
            var co = CoroutineFactory.NewSubroutine(CharacterManager.Instance.SaveAllCharacter, coroutine,
                default(TimeSpan));
            if (co.MoveNext())
            {
                yield return co;
            }

            SceneServer.Instance.DB.Dispose();
        }

        public IEnumerator GetSceneSimpleData(Coroutine coroutine, SceneService _this, GetSceneSimpleDataInMessage msg)
        {
            var characterId = msg.CharacterId;
            msg.Response.Id = characterId;
            CharacterManager.Instance.GetSimpeData(characterId, simple =>
            {
                if (simple == null)
                {
                    msg.Reply((int) ErrorCodes.Unknow);
                    return;
                }
                msg.Response.Id = simple.Id;
                msg.Response.TypeId = simple.TypeId;
                msg.Response.Name = simple.Name;
                msg.Response.SceneId = simple.SceneId;
                msg.Response.FightPoint = simple.FightPoint;
                msg.Response.Level = simple.Level;
                msg.Response.Ladder = simple.Ladder;
                msg.Response.ServerId = simple.ServerId;
                msg.Response.CheckAttr.AddRange(simple.CheckAttr);
                msg.Response.AttrList.AddRange(simple.AttrList);
                msg.Response.LastTime = simple.LastTime;
                msg.Response.Vip = simple.Vip;
                msg.Response.StarNum = simple.StarNum;

                var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
                if (character == null)
                {
                    msg.Response.Online = 0;
                }
                else
                {
                    msg.Response.Online = character.Proxy == null ? 0 : 1;
                }
                msg.Reply();
            });
            return null;
        }

        public IEnumerator CheckConnected(Coroutine coroutine, SceneService _this, CheckConnectedInMessage msg)
        {
            Logger.Error("Scene CheckConnected, {0}", msg.CharacterId);

            //SceneCharacterProxy proxy = null;
            //if (_this.Proxys.TryGetValue(msg.CharacterId, out proxy))
            //{
            //    if ((proxy as SceneProxy).Connected)
            //    {
            //        msg.Response = 1;
            //        msg.Reply();
            //        return null;
            //    }

            //    //(proxy as SceneProxy).WaitingCheckConnectedInMessages.Add(msg);
            //}
            msg.Reply((int) ErrorCodes.Unline);

            return null;
        }

        public IEnumerator CheckLost(Coroutine coroutine, SceneService _this, CheckLostInMessage msg)
        {
            Logger.Error("Scene CheckLost, {0}", msg.CharacterId);

            //SceneCharacterProxy proxy = null;
            //if (!_this.Proxys.TryGetValue(msg.CharacterId, out proxy))
            //{
            //    msg.Reply();
            //}
            //else
            //{
            //    if ((proxy as SceneProxy).Connected)
            //    {
            //        (proxy as SceneProxy).WaitingCheckLostInMessages.Add(msg);
            //    }
            //    else
            //    {
            //        msg.Reply();
            //    }
            //}

            return null;
        }

        public IEnumerator QueryStatus(Coroutine coroutine, SceneService _this, QueryStatusInMessage msg)
        {
            var common = new ServerCommonStatus();
            common.Id = SceneServer.Instance.Id;
            common.ByteReceivedPerSecond = _this.ByteReceivedPerSecond;
            common.ByteSendPerSecond = _this.ByteSendPerSecond;
            common.MessageReceivedPerSecond = _this.MessageReceivedPerSecond;
            common.MessageSendPerSecond = _this.MessageSendPerSecond;
            common.ConnectionCount = _this.ConnectionCount;

            msg.Response.CommonStatus = common;

            msg.Response.ConnectionInfo.AddRange(SceneServer.Instance.Agents.Select(kv =>
            {
                var conn = new ConnectionStatus();
                var item = kv.Value;
                conn.ByteReceivedPerSecond = item.ByteReceivedPerSecond;
                conn.ByteSendPerSecond = item.ByteSendPerSecond;
                conn.MessageReceivedPerSecond = item.MessageReceivedPerSecond;
                conn.MessageSendPerSecond = item.MessageSendPerSecond;
                conn.Target = item.Id;
                conn.Latency = item.Latency;

                return conn;
            }));

            msg.Reply();

            yield break;
        }

        public IEnumerator SceneEquipModelStateChange(Coroutine coroutine, SceneService _this, SceneEquipModelStateChangeInMessage msg)
        {
            var characterId = msg.CharacterId;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (character == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }
            character.EquipModelStateChange(msg.Request.Part, msg.Request.State, msg.Request.Equip);
            msg.Reply();
        }

        //装备发生了变化 nType=变化规则{0删除，1新增，2修改} nPart=部位   Equip=新装备数据
        public IEnumerator SceneEquipChange(Coroutine coroutine, SceneService _this, SceneEquipChangeInMessage msg)
        {
            var characterId = msg.CharacterId;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (character == null)
            {
                Logger.Error("SceneEquipChange Enter characterId = {0} null", characterId);
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }
            character.EquipChange(msg.Request.Type, msg.Request.Part, msg.Request.Equip);
            msg.Response = 1;
            msg.Reply();
        }
        public IEnumerator SyncSceneMount(Coroutine coroutine, SceneService _this, SyncSceneMountInMessage msg)
        {
            var characterId = msg.CharacterId;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (character == null)
            {
                Logger.Error("SceneEquipChange Enter characterId = {0} null", characterId);
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }
            var tbScene = Table.GetScene((int) character.CurrentSceneTypeId);
            if (tbScene != null)
            {
                if (tbScene.IsCanRide == 1)
                {
                    character.RideMount(msg.Request.MountId);
                    character.SetMountId(msg.Request.MountId);
                }
            }
            yield break;
        }
        public IEnumerator SSSceneElfChange(Coroutine coroutine, SceneService _this, SSSceneElfChangeInMessage msg)
        {
            var characterId = msg.CharacterId;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (character == null)
            {
                Logger.Error("SSSceneElfChange Enter characterId = {0} null", characterId);
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }
            character.ElfChange(msg.Request.RemoveBuff.Items, msg.Request.AddBuff.Data, msg.Request.FightPoint);
            msg.Response = 1;
            msg.Reply();
        }
        public IEnumerator NotifyRefreshLodeTimer(Coroutine coroutine, SceneService _this, NotifyRefreshLodeTimerInMessage msg)
        {
            SceneManager.Instance.FreshLodeTimer(msg.Request.ServerId,msg.Request.Ids.Items);            
            yield break;
        }

        //技能发生了变化 nType=变化规则{0删除，1新增，2修改} nId=技能ID   nLevel=技能等级
        public IEnumerator SceneSkillChange(Coroutine coroutine, SceneService _this, SceneSkillChangeInMessage msg)
        {
            bool bSyncTeam = msg.Request.Id == -1 || msg.Request.Id == -2;
            var count = msg.Request.Level;
            var characterId = msg.CharacterId;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (character == null)
            {
                Logger.Error("SceneSkillChange Enter characterId = {0} null", characterId);
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }
            if (msg.Request.Id == -5)//潜规则终身卡
            {
                character.Attr.LifeCardFlag = count;
                SceneManager.Instance.CheckAddLifeCardBuff(character);
                Logger.Warn("SceneSkillChange flag : {0}", character.Attr.LifeCardFlag);
            }
            if (msg.Request.Id == -4)  //vip级别
            {
                character.Attr.Vip = count;
            }
            else if (msg.Request.Id == -3) //潜规则玩家军衔
            {
                character.Attr.Honor = count;
            }
            else if (msg.Request.Id == -2) //潜规则玩家转职次数
            {
                character.Attr.Ladder = count;
                character.Attr.OnPropertyChanged((uint) eSceneSyncId.SyncReborn);
                character.FriendsDirty = true;
            }
            else if (msg.Request.Id == -1) //潜规则玩家等级
            {
                var oldLevel = character.Attr.GetDataValue(eAttributeType.Level);

                var attrData = new LevelUpAttrData();
                attrData.OldAttr.Add((int) eAttributeType.Level, character.Attr.GetDataValue(eAttributeType.Level));
                attrData.OldAttr.Add((int) eAttributeType.PhyPowerMax,
                    character.Attr.GetDataValue(eAttributeType.PhyPowerMax));
                attrData.OldAttr.Add((int) eAttributeType.MagPowerMax,
                    character.Attr.GetDataValue(eAttributeType.MagPowerMax));
                attrData.OldAttr.Add((int) eAttributeType.PhyArmor, character.Attr.GetDataValue(eAttributeType.PhyArmor));
                attrData.OldAttr.Add((int) eAttributeType.MagArmor, character.Attr.GetDataValue(eAttributeType.MagArmor));
                attrData.OldAttr.Add((int) eAttributeType.HpMax, character.Attr.GetDataValue(eAttributeType.HpMax));
                character.Attr.SetFightPointFlag();
                character.Attr.SetDataValue(eAttributeType.Level, count);

                attrData.NewAttr.Add((int) eAttributeType.Level, character.Attr.GetDataValue(eAttributeType.Level));
                attrData.NewAttr.Add((int) eAttributeType.PhyPowerMax,
                    character.Attr.GetDataValue(eAttributeType.PhyPowerMax));
                attrData.NewAttr.Add((int) eAttributeType.MagPowerMax,
                    character.Attr.GetDataValue(eAttributeType.MagPowerMax));
                attrData.NewAttr.Add((int) eAttributeType.PhyArmor, character.Attr.GetDataValue(eAttributeType.PhyArmor));
                attrData.NewAttr.Add((int) eAttributeType.MagArmor, character.Attr.GetDataValue(eAttributeType.MagArmor));
                attrData.NewAttr.Add((int) eAttributeType.HpMax, character.Attr.GetDataValue(eAttributeType.HpMax));

                character.Proxy.SyncLevelChange(attrData);
                SceneManager.Instance.CheckAvgLevelBuff(character);

                if (count > oldLevel)
                {
                    character.Attr.SetDataValue(eAttributeType.HpNow, character.Attr.GetDataValue(eAttributeType.HpMax));
                    character.Attr.SetDataValue(eAttributeType.MpNow, character.Attr.GetDataValue(eAttributeType.MpMax));
                }
                character.FriendsDirty = true;
                PlayerLog.AttrLog(character.ObjId, character.Attr.GetStringData());
                PlayerLog.AttrLog(character.ObjId, "name = {0}, type = {1}, level = {2}, fightpoint = {3}",
                    character.GetName(), character.TypeId, count, character.Attr.GetFightPoint().ToString());
            }
            else
            {
                character.SkillChange(msg.Request.Type, msg.Request.Id, count);
            }
            msg.Response = 1;
            msg.Reply();



            if(bSyncTeam == true&&character.GetTeamId()>0)
            {
                var msgTeam = SceneServer.Instance.TeamAgent.SSSyncTeamMemberLevelChange(character.ObjId,character.ServerId ,characterId, character.GetTeamId(), character.Attr.Ladder, character.Attr.GetDataValue(eAttributeType.Level));
                yield return msgTeam.SendAndWaitUntilDone(coroutine);
            }
        }

        //同步装备的技能
        public IEnumerator SceneEquipSkill(Coroutine coroutine, SceneService _this, SceneEquipSkillInMessage msg)
        {
            var characterId = msg.CharacterId;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (character == null)
            {
                Logger.Error("SceneSkillChange Enter characterId = {0} null", characterId);
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }
            character.EquipSkill(msg.Request.DelSkills.Items, msg.Request.SkillIds.Items, msg.Request.SkillLevels.Items);
            msg.Reply();
        }

        //天赋发生了变化 nType=变化规则{0删除，1新增，2修改} nId=天赋ID   nLevel=天赋层数
        public IEnumerator SceneInnateChange(Coroutine coroutine, SceneService _this, SceneInnateChangeInMessage msg)
        {
            var characterId = msg.CharacterId;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (character == null)
            {
                Logger.Error("SceneInnateChange Enter characterId = {0} null", characterId);
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }
            character.TalentChange(msg.Request.Type, msg.Request.Id, msg.Request.Level);
            msg.Response = 1;
            msg.Reply();
        }

        //同步图鉴数据
        public IEnumerator SceneBookAttrChange(Coroutine coroutine, SceneService _this, SceneBookAttrChangeInMessage msg)
        {
            var characterId = msg.CharacterId;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (character == null)
            {
                Logger.Error("SceneBookAttrChange Enter characterId = {0} null", characterId);
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }
            character.Attr.BookRefresh(msg.Request.Attrs.Data,msg.Request.MonsterAttrs.Data);
            msg.Response = 1;
            msg.Reply();
        }

        public IEnumerator SceneTitleChange(Coroutine coroutine, SceneService _this, SceneTitleChangeInMessage msg)
        {
            var characterId = msg.CharacterId;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (character == null)
            {
                Logger.Error("SceneTitleChange Enter characterId = {0} null", characterId);
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }
            var request = msg.Request;
            character.Attr.TitleRefresh(request.Titles.Items, request.Type);
        }

        public IEnumerator NotifyItemCount(Coroutine co, SceneService _this, NotifyItemCountInMessage msg)
        {
            var characterId = msg.CharacterId;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (character == null)
            {
                Logger.Error("SceneTitleChange Enter characterId = {0} null", characterId);
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }
            var request = msg.Request;
            var itemId = request.ItemId;
            var count = request.Count;
            character.SetItemCount(itemId, count);
        }

        public IEnumerator MotifyServerAvgLevel(Coroutine coroutine,
                                                SceneService _this,
                                                MotifyServerAvgLevelInMessage msg)
        {
            SceneManager.Instance.PushAvgLevel(msg.Request.ServerAvgLevel.Data);
            yield break;
        }

        //增加动态活动调整
        public IEnumerator AddAutoActvity(Coroutine coroutine, SceneService _this, AddAutoActvityInMessage msg)
        {
            var endTime = DateTime.FromBinary(msg.Request.EndTime);
            if (endTime < DateTime.Now)
            {
                AutoActivityManager.DeleteActivity(msg.Request.FubenId);
            }
            else
            {
                AutoActivityManager.PushActivity(msg.Request.FubenId, DateTime.FromBinary(msg.Request.StartTime),
                    endTime, msg.Request.Count);
            }
            yield break;
        }

        //给Buff
        public IEnumerator SSAddBuff(Coroutine coroutine, SceneService _this, SSAddBuffInMessage msg)
        {
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.Request.CharacterId);
            if (null == character)
            {
                yield break;
            }
            character.AddBuff(msg.Request.BuffId, msg.Request.BuffLevel, character);
        }
        public IEnumerator SyncFlagData(Coroutine coroutine, SceneService _this, SyncFlagDataInMessage msg)
        {
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (null == character)
            {
                yield break;
            }
            foreach (var v in msg.Request.Changes.Data)
            {
                character.SetFlag(v.Key,v.Value>0);
            }
            yield break;
        }
        #region 灭世之战

        public IEnumerator SSApplyNpcHP(Coroutine coroutine, SceneService _this, SSApplyNpcHPInMessage msg)
        {
            var sceneDic = SceneManager.Instance.ScenesDic.GetValue(msg.Request.ServerId);
            var sceneList = sceneDic.GetValue(msg.Request.SceneId);
            if (sceneList != null && sceneList.Count > 0)
            {
                foreach (var item in sceneList)
                {
                    var scene = item.Value;
                    ObjBase obj;
                    if (scene.mObjDict.TryGetValue(msg.Request.NpcGuid, out obj))
                    {
                        ObjNPC npc = (ObjNPC)obj;
                        msg.Response = npc.Attr.GetDataValue(eAttributeType.HpNow);
                        msg.Reply((int)ErrorCodes.OK);
                        yield break;
                    }
                }
            }
            else
            {
                msg.Response = 0;
                msg.Reply((int)ErrorCodes.Error_NoScene);
            }
            msg.Response = 0;
            msg.Reply((int)ErrorCodes.OK);
            
            
            yield break;
        }
        #endregion

        public IEnumerator SSNotifyCharacterOnConnet(Coroutine coroutine,
                                                     SceneService _this,
                                                     SSNotifyCharacterOnConnetInMessage msg)
        {
            var characterId = msg.Request.CharacterId;
            var clientId = msg.Request.ClientId;
            var proxy = new SceneProxy(_this, characterId, clientId);
            _this.Proxys[characterId] = proxy;

            PlayerLog.WriteLog(888, "SSNotifyCharacter OnConnet characterId={0},clientId={1}", characterId, clientId);
            var ret = AsyncReturnValue<bool>.Create();
            var subCo = CoroutineFactory.NewSubroutine(OnConnected, coroutine, proxy, ret);
            if (subCo.MoveNext())
            {
                yield return subCo;
            }
            var isOk = ret.Value;
            ret.Dispose();
            if (isOk)
            {
                NotifyGateClientState(_this, clientId, characterId);
                msg.Reply((int) ErrorCodes.OK);
            }
            else
            {
                msg.Reply((int) ErrorCodes.ConnectFail);
            }
        }

        public IEnumerator BSNotifyCharacterOnLost(Coroutine coroutine,
                                                   SceneService _this,
                                                   BSNotifyCharacterOnLostInMessage msg)
        {
            var characterId = msg.Request.CharacterId;
            SceneCharacterProxy charProxy;
            if (!_this.Proxys.TryGetValue(characterId, out charProxy))
            {
                yield break;
            }
            var proxy = (SceneProxy) charProxy;

            var co = CoroutineFactory.NewSubroutine(OnLost, coroutine, proxy);
            if (co.MoveNext())
            {
                yield return co;
            }
        }

        //组队消息的响应
        public IEnumerator SceneTeamMessage(Coroutine coroutine, SceneService _this, SceneTeamMessageInMessage msg)
        {
            var characterId = msg.CharacterId;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (character == null)
            {
                msg.Reply((int) ErrorCodes.Unline);
                //Logger.Error("SceneTeamMessage not find characterId = {0} null", characterId);
                yield break;
            }
            var type = msg.Request.Type;
            var teamId = msg.Request.TeamId;
            var state = msg.Request.State;
            PlayerLog.WriteLog((int) LogType.TeamMessage,
                "SS->SceneTeamMessage characterId={0}, type={1}, teamId={2}, name={3}", characterId, type, teamId,
                character.GetName());
            switch (type)
            {
                case 0:
                {
                    //创建队伍
                    character.SetTeamId(teamId, state);
                }
                    break;
                case 1:
                {
                    //加入队伍
                    character.SetTeamId(teamId, state);
                }
                    break;
                case 2:
                {
                    //离开队伍
                    character.SetTeamId(0, 0);
                }
                    break;
            }
            msg.Reply();
        }

        //使用道具技能
        public IEnumerator UseSkillItem(Coroutine coroutine, SceneService _this, UseSkillItemInMessage msg)
        {
            var characterId = msg.CharacterId;

            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (character == null)
            {
                Logger.Error("UseSkillItem Enter characterId = {0} null", characterId);
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }
            var tbItem = Table.GetItemBase(msg.Request.ItemId);
            switch (tbItem.Type)
            {
                case 24000: //红蓝药类（技能实现CD）
                {
                    switch (tbItem.Exdata[2])
                    {
                        case 0: //红药
                        {
                            if (character.GetAttribute(eAttributeType.HpMax) <=
                                character.GetAttribute(eAttributeType.HpNow))
                            {
                                msg.Reply((int) ErrorCodes.Error_HpMax);
                                yield break;
                            }
                        }
                            break;
                        case 1: //蓝药
                        {
                            if (character.GetAttribute(eAttributeType.MpMax) <=
                                character.GetAttribute(eAttributeType.MpNow))
                            {
                                msg.Reply((int) ErrorCodes.Error_MpMax);
                                yield break;
                            }
                        }
                            break;
                    }

	                var level = tbItem.Exdata[1];
	                if (null != character.Skill.GetSkill(999))
	                {
						var param = Math.Max((character.GetLevel() + 1) / 50 - 1, 0);
						level = Math.Max(tbItem.Exdata[1] + param * param * tbItem.Exdata[3] + param * tbItem.Exdata[3], 1);    
	                }
	                

					var result = character.Skill.CheckDoSkill(tbItem.Exdata[0], level);
                    if (result == ErrorCodes.OK)
                    {
                        var msg2 = SceneServer.Instance.LogicAgent.SSDeleteItemByIndex(characterId, msg.Request.BagId,
                            msg.Request.BagIndex, 1);
                        yield return msg2.SendAndWaitUntilDone(coroutine);
                        if (msg2.State != MessageState.Reply)
                        {
                            msg.Reply();
                            yield break;
                        }
                        if ((ErrorCodes) msg2.ErrorCode != ErrorCodes.OK)
                        {
                            msg.Reply(msg2.ErrorCode);
                            yield break;
                        }
						character.Skill.DoSkill(tbItem.Exdata[0], level);
                    }
                    msg.Reply((int) result);
                }
                    break;
                case 24500: //加BUFF药
                {
                    var result = character.CheckAddBuff(tbItem.Exdata[0], tbItem.Exdata[1], character);
                    if (result != ErrorCodes.OK)
                    {
                        msg.Reply((int) ErrorCodes.Error_BuffLevelTooLow);
                    }
                    var msg2 = SceneServer.Instance.LogicAgent.SSDeleteItemByIndex(characterId, msg.Request.BagId,
                        msg.Request.BagIndex, 1);
                    yield return msg2.SendAndWaitUntilDone(coroutine);
                    if (msg2.State != MessageState.Reply)
                    {
                        msg.Reply();
                        yield break;
                    }
                    if ((ErrorCodes) msg2.ErrorCode != ErrorCodes.OK)
                    {
                        msg.Reply(msg2.ErrorCode);
                        yield break;
                    }
                    var buff = character.AddBuff(tbItem.Exdata[0], tbItem.Exdata[1], character);
                    if (buff == null)
                    {
                        msg.Reply((int) ErrorCodes.Error_BuffLevelTooLow);
                    }
                    else
                    {
                        msg.Reply();
                    }
                }
                    break;
                case 24900: //修改杀气药
                {
                    var kv = character.KillerValue;
                    var changeValue = tbItem.Exdata[0];
                    if (changeValue > 0)
                    {
                        if (kv >= 500)
                        {
                            msg.Reply((int) ErrorCodes.Error_KillerValue);
                            yield break;
                        }
                    }
                    else if (kv < 1)
                    {
                        msg.Reply((int) ErrorCodes.Error_KillerValue);
                        yield break;
                    }
                    var msg2 = SceneServer.Instance.LogicAgent.SSDeleteItemByIndex(characterId, msg.Request.BagId,
                        msg.Request.BagIndex, 1);
                    yield return msg2.SendAndWaitUntilDone(coroutine);
                    if (msg2.State != MessageState.Reply)
                    {
                        msg.Reply();
                        yield break;
                    }
                    if ((ErrorCodes) msg2.ErrorCode != ErrorCodes.OK)
                    {
                        msg.Reply(msg2.ErrorCode);
                        yield break;
                    }
                    kv += changeValue;
                    character.KillerValue = kv;
                    msg.Reply();
                }
                    break;
                case 24950: //设置杀气药
                {
                    var kv = character.KillerValue;
                    var setValue = tbItem.Exdata[0];
                    if (kv == setValue)
                    {
                        msg.Reply((int) ErrorCodes.Error_KillerValue);
                        yield break;
                    }
                    var msg2 = SceneServer.Instance.LogicAgent.SSDeleteItemByIndex(characterId, msg.Request.BagId,
                        msg.Request.BagIndex, 1);
                    yield return msg2.SendAndWaitUntilDone(coroutine);
                    if (msg2.State != MessageState.Reply)
                    {
                        msg.Reply();
                        yield break;
                    }
                    if ((ErrorCodes) msg2.ErrorCode != ErrorCodes.OK)
                    {
                        msg.Reply(msg2.ErrorCode);
                        yield break;
                    }
                    character.KillerValue = setValue;
                    msg.Reply();
                }
                    break;
                default:
                    Logger.Warn("UseSkillItem characterId={2} Id={0},Type={1}", msg.Request.ItemId, tbItem.Type,
                        characterId);
                    msg.Response = tbItem.Type;
                    msg.Reply((int) ErrorCodes.Unknow);
                    break;
            }
        }

        //进入副本
        public IEnumerator AskEnterDungeon(Coroutine coroutine, SceneService _this, AskEnterDungeonInMessage msg)
        {
            var serverControl = (SceneServerControl) _this;
            Logger.Info("AskEnterDungeon id={0}", msg.CharacterId);
            PlayerLog.WriteLog(888, "AskEnterDungeon characterId={0},ServerId={1},SceneId={2},SceneGuid={3}",
                msg.CharacterId, msg.Request.ServerId, msg.Request.SceneId, msg.Request.Guid);
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (character == null)
            {
                Logger.Error("AskEnterDungeon Enter characterId = {0} null", msg.CharacterId);
                yield break;
            }

            if (character.IsChangingScene())
            {
                Logger.Error("ChangeSceneRequest proxy.Character.IsChangingScene() characterId = {0}", msg.CharacterId);
                msg.Reply((int) ErrorCodes.StateError);
                yield break;
            }

            var scenechgType = msg.Request.SceneId != -1 ? eScnenChangeType.EnterDungeon : eScnenChangeType.TeamDungeon;

            //请求进入副本时，根据合服ID进行
            var serverLogicId = SceneExtension.GetServerLogicId(msg.Request.ServerId);
            var co1 = CoroutineFactory.NewSubroutine(serverControl.CreateAndEnterScene,
                coroutine,
                msg.CharacterId,
                serverLogicId,
                msg.Request.SceneId,
                msg.Request.Guid,
                scenechgType,
                msg.Request.Param);
            if (co1.MoveNext())
            {
                yield return co1;
            }

            var tbScene = Table.GetScene(msg.Request.SceneId);
            if (tbScene != null)
            {
                var tbFuben = Table.GetFuben(tbScene.FubenId);
                if (tbFuben != null)
                {
                    //后台统计
                    try
                    {
                        var v = string.Format("fuben#{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                            msg.CharacterId,
                            tbScene.FubenId,
                            tbFuben.Name,
                            tbFuben.AssistType,
                            0,  // 0  进入   1 完成   2 退出
                            DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                            character.GetLevel(),
                            character.Attr.GetFightPoint()
                            ); // 时间
                        PlayerLog.Kafka(v);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.Message);
                    }                    
                }
            }
        }

        public IEnumerator LoginEnterScene(Coroutine coroutine, SceneService _this, LoginEnterSceneInMessage msg)
        {
            var characterId = msg.CharacterId;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (character == null)
            {
                Logger.Error("LoginEnterScene GetCharacter characterId = {0} null", characterId);
                yield break;
            }

            var db = character.GetData();
            var chgType = eScnenChangeType.Login;
            var sceneGuid = db.SceneGuid;
            var serverId = msg.Request.ServerId;
            var logout = DateTime.FromBinary(msg.Request.Logout);
            var isneed = false;
            var sceneId = SceneExtension.GetWillScene((int) db.SceneId, (int) db.FormerSceneId, db.Hp,
                DateTime.FromBinary(db.AutoRelive), logout, ref sceneGuid, ref isneed);
            if (isneed)
            {
                chgType = eScnenChangeType.LoginRelive;
                character.Attr.SetDataValue(eAttributeType.HpNow, character.Attr.GetDataValue(eAttributeType.HpMax));
                character.Attr.SetDataValue(eAttributeType.MpNow, character.Attr.GetDataValue(eAttributeType.MpMax));
            }

            //Login时，根据合服ID进行
            var serverLogicId = SceneExtension.GetServerLogicId(serverId);
            var co = CoroutineFactory.NewSubroutine(CreateAndEnterScene,
                coroutine,
                _this,
                characterId,
                serverLogicId,
                sceneId,
                sceneGuid,
                chgType,
                new SceneParam());
            if (co.MoveNext())
            {
                yield return co;
            }
        }

        //重新读表
		public IEnumerator ServerGMCommand(Coroutine coroutine, SceneService _this, ServerGMCommandInMessage msg)
        {
//             var serverControl = (SceneServerControl) _this;
//             Logger.Info("----------SceneReloadTable----------{0}", msg.Request.TableName);
//             if (msg.Request.TableName == "CM")
//             {
//                 Logger.Info("NetCount={0},CharacterCount={1}", serverControl.GetPlayerCount(),
//                     CharacterManager.Instance.CharacterCount());
//                 serverControl.LookProxy();
//                 CharacterManager.Instance.LookCharacters();
//                 yield break;
//             }
			var cmd = msg.Request.Cmd;
			var param = msg.Request.Param;

			Logger.Info("Rank----------ServerGMCommand----------cmd={0}|param={1}", cmd, param);

			try
			{
				if ("ReloadTable" == cmd)
				{
					Table.ReloadTable(param);
					InitStoreTypeMap();

					// 重新加载特殊掉落表
					ReloadSpecilDropTable();
				}
			}
			catch (Exception e)
			{
				Logger.Error("Rank----------ServerGMCommand----------error={0}", e.Message);
			}
			finally
			{

			}
			yield break;
        }

        private void ReloadSpecilDropTable()
        {
            Drop.SpecialDropForNewCharacter.Clear();
            Table.ForeachPlayerDrop(item =>
            {
                Drop.SpecialDropForNewCharacter.Add(item.MonsterId,
                    new List<int> {item.Id, item.job1Drop, item.job2Drop, item.job3Drop});

                if (item.Id >= 64)
                {
                    Logger.Error("Player Drop table id must less than 64.");
                    return false;
                }

                return true;
            });
        }

        // 根据商店类型建立索引
        public static void InitStoreTypeMap()
        {
            SceneServerControl.StoreTypeItems.Clear();

            Table.ForeachStore(record =>
            { // 不区分Group了
                List<StoreRecord> tempList;
                if (!SceneServerControl.StoreTypeItems.TryGetValue(record.Type, out tempList))
                {
                    tempList = new List<StoreRecord>();
                    SceneServerControl.StoreTypeItems[record.Type] = tempList;
                }
                tempList.Add(record);

                return true;
            });
        }

        public void GetBlackStoreItems(List<StoreRecord> tempList, Dictionary<int, int> mapCounts, List<StoneItem> outList)
        {
            var blackStoreItems = new List<StoreRecord>();
            for (int i = 0; i < tempList.Count; i++)
            {
                blackStoreItems.Add(tempList[i]);
            }
            var limitCount = Table.GetServerConfig(1204).ToInt();
            for (int i = 0; i < limitCount; i++)
            {
                var item = GetItemByWeight(blackStoreItems);
                blackStoreItems.Remove(item);
                var tempItem = new StoneItem();
                tempItem.itemid = item.Id;
                var count = 0;
                if (!mapCounts.TryGetValue(item.Id, out count))
                {
                    count = item.FuBenCount;
                    mapCounts[item.Id] = item.FuBenCount;
                }
                tempItem.itemcount = count;
                outList.Add(tempItem);
            }
        }

        public StoreRecord GetItemByWeight(List<StoreRecord> tempList)
        {
            var totalWeightSum = 0;
            foreach (var item in tempList)
            {
                totalWeightSum += item.Weight + 1;
            }
            var ranWeight = 0;
            var curWeightSum = 0;
            foreach (var item in tempList)
            {
                ranWeight = MyRandom.Random(1, totalWeightSum);
                curWeightSum += item.Weight;
                if (curWeightSum >= ranWeight)
                {
                    return item;
                }
            }
            return null;
        }

        public void GetFubenStoreItems(int type, Dictionary<int, int> mapCounts, List<StoneItem> outList)
        {
            List<StoreRecord> tempList;
            if (SceneServerControl.StoreTypeItems.TryGetValue(type, out tempList))
            {
                GetBlackStoreItems(tempList, mapCounts, outList);
                //if (type == 1100)//黑市商人
                //{
                //    GetBlackStoreItems(tempList, mapCounts, outList);
                //}
                //else
                //{
                //    foreach (var item in tempList)
                //    {
                //        var tempItem = new StoneItem();
                //        tempItem.itemid = item.Id;
                //        var count = 0;
                //        if (!mapCounts.TryGetValue(item.Id, out count))
                //        {
                //            count = item.FuBenCount;
                //            mapCounts[item.Id] = item.FuBenCount;
                //        }
                //        tempItem.itemcount = count;
                //        outList.Add(tempItem);
                //    }
                //}
            }
        }

        public void InitFubenStoreCounts(int type, Dictionary<int, int> outMap)
        {
            List<StoreRecord> tempList;
            if (SceneServerControl.StoreTypeItems.TryGetValue(type, out tempList))
            {
                foreach (var item in tempList)
                {
                    var count = 0;
                    if (!outMap.TryGetValue(item.Id, out count))
                    {
                        outMap = new Dictionary<int, int>();
                        outMap[item.Id] = item.FuBenCount;
                    }
                    else
                    {
                        outMap[item.Id] += item.FuBenCount;
                    }
                }
            }
        }

        //查询玩家名字
        public IEnumerator FindCharacterName(Coroutine coroutine, SceneService _this, FindCharacterNameInMessage msg)
        {
            var likeName = msg.Request.LikeName;
            var lists = CharacterManager.playerName.Find(likeName);
            if (lists == null)
            {
                msg.Reply();
                yield break;
            }
            foreach (var list in lists)
            {
                var temp = list.Key;
                var simple2 = temp.GetSimpleData();
                var simple = new CharacterSimpleData
                {
                    Id = simple2.Id,
                    TypeId = simple2.TypeId,
                    Name = simple2.Name,
                    SceneId = simple2.SceneId,
                    FightPoint = simple2.FightPoint,
                    Level = simple2.Level,
                    Ladder = simple2.Ladder,
                    ServerId = simple2.ServerId,
                    Online = 1,
                    StarNum = simple2.StarNum
                };
                msg.Response.Datas.Add(temp.ObjId, simple);
            }
            //CharacterManager.Instance.ForeachCharacter(character =>
            //{
            //    var temp = character as ObjPlayer;
            //    if (temp == null)
            //    {
            //        return true;
            //    }
            //    if (temp.GetName().IndexOf(likeName, 0, StringComparison.Ordinal) != -1)
            //    {
            //        //msg.Response = 1;
            //        var simple2 = temp.GetSimpleData();
            //        var simple = new CharacterSimpleData
            //        {
            //            Id = simple2.Id,
            //            TypeId = simple2.TypeId,
            //            Name = simple2.Name,
            //            SceneId = simple2.SceneId,
            //            FightPoint = simple2.FightPoint,
            //            Level = simple2.Level,
            //            Ladder = simple2.Ladder,
            //            ServerId = simple2.ServerId,
            //            Online = 1
            //        };
            //        msg.Response.Datas.Add(temp.ObjId, simple);
            //    }
            //    //if (character.)
            //    return true;
            //});
            msg.Reply();
        }

        public IEnumerator FindCharacterFriend(Coroutine coroutine, SceneService _this, FindCharacterFriendInMessage msg)
        {
            var serverId = msg.Request.ServerId;
            //查询服务器好友时，根据合服ID进行
            var serverLogicId = SceneExtension.GetServerLogicId(serverId);
            var level = msg.Request.Level;
            var maxCount = 20;
            if (CharacterManager.Instance.CharacterCount() < maxCount)
            {
                CharacterManager.Instance.ForeachCharacter(character =>
                {
                    var temp = character as ObjPlayer;
                    if (SceneExtension.GetServerLogicId(temp.ServerId) == serverLogicId)
                    {
                        var simple2 = temp.GetSimpleData();
                        var simple = new CharacterSimpleData
                        {
                            Id = simple2.Id,
                            TypeId = simple2.TypeId,
                            Name = simple2.Name,
                            SceneId = simple2.SceneId,
                            FightPoint = simple2.FightPoint,
                            Level = simple2.Level,
                            Ladder = simple2.Ladder,
                            ServerId = simple2.ServerId,
                            Online = 1,
                            StarNum = simple2.StarNum
                        };
                        msg.Response.Datas.Add(temp.ObjId, simple);
                    }
                    return true;
                });
                msg.Reply();
                yield break;
            }
            findlist.Clear();
            CharacterManager.Instance.ForeachCharacter(character =>
            {
                var temp = character as ObjPlayer;
                if (temp.ServerId == serverId)
                {
                    if (Math.Abs(temp.GetLevel() - level) < 50)
                    {
                        var simple2 = temp.GetSimpleData();
                        var simple = new CharacterSimpleData
                        {
                            Id = simple2.Id,
                            TypeId = simple2.TypeId,
                            Name = simple2.Name,
                            SceneId = simple2.SceneId,
                            FightPoint = simple2.FightPoint,
                            Level = simple2.Level,
                            Ladder = simple2.Ladder,
                            ServerId = simple2.ServerId,
                            Online = 1,
                            StarNum = simple2.StarNum
                        };
                        findlist.Add(simple);
                        //msg.Response.Datas.Add(temp.ObjId, simple);
                    }
                }
                return true;
            });
            findlist = findlist.RandRange(0, maxCount);
            foreach (var data in findlist)
            {
                msg.Response.Datas.Add(data.Id, data);
            }
            msg.Reply();
        }

        public IEnumerator BSCreateScene(Coroutine coroutine, SceneService _this, BSCreateSceneInMessage msg)
        {
            var scene = SceneManager.Instance.CreateScene(msg.Request.ServerId, msg.Request.SceneId, msg.Request.Guid,
                msg.Request.SceneParam);
            if (scene != null)
            {
                msg.Reply();


                if (scene.TableSceneData != null && scene.TableSceneData.Type == (int)eSceneType.BossHome)
                {//boss之家刷新
                    var bossHome = scene as BossHome;
                    if(bossHome == null)
                        yield break;
                    var bossData = SceneServer.Instance.ActivityAgent.QueryCreateMonsterData(1, scene.ServerId,scene.TypeId);
                    yield return bossData.SendAndWaitUntilDone(coroutine);
                    if (bossData.State != MessageState.Reply || bossData.ErrorCode != (int)ErrorCodes.OK)
                    {
                        Logger.Error("QueryCreateMonsterData() failed in BSCreateScene()  BossHome!!!");
                        yield break;
                    }
                    foreach (var v in bossData.Response.Items)
                    {
                        bossHome.RefreshBoss(v);
                    }
                    yield break;
                }

                if (scene.TypeId <= 0 || scene.TableSceneData == null || scene.TableSceneData.Type != 0)
                    //不是野外，则不需要刷特殊怪物
                {
                    yield break;
                }

                //问问Activity，有没有需要造的boss
                var createMonsterData = SceneServer.Instance.ActivityAgent.QueryCreateMonsterData(0, scene.ServerId,
                    scene.TypeId);
                yield return createMonsterData.SendAndWaitUntilDone(coroutine);
                if (createMonsterData.State != MessageState.Reply || createMonsterData.ErrorCode != (int) ErrorCodes.OK)
                {
                    Logger.Error("QueryCreateMonsterData() failed in BSCreateScene()!!!");
                    yield break;
                }

                scene.CreateSpeMonsters(createMonsterData.Response.Items);
            }
            else
            {
                Logger.Error("Create scene failed, serverid:{0}, sceneid:{1}, guid:{2}", msg.Request.ServerId,
                    msg.Request.SceneId, msg.Request.Guid);
            }
            yield return null;
        }

        public IEnumerator BossDie(Coroutine coroutine, SceneService _this, BossDieInMessage msg)
        {
            var scenes = SceneManager.Instance.GetScenes(msg.Request.ServerId, 6000);
            if (scenes == null)
            {
                yield return null;
            }
            foreach (var scene in scenes)
            {
                scene.DealWith("BossDie", null);
            }
        }

        //玩家离开改sceneServer
        public IEnumerator UnloadData(Coroutine coroutine, SceneService _this, UnloadDataInMessage msg)
        {
            PlayerLog.WriteLog(888, "SceneServer UnloadData characterId={0},clientId={1}", msg.Request.CharacterId,
                msg.ClientId);
            Logger.Info("Enter Game {0} UnloadData - 1 - {1}", msg.CharacterId, TimeManager.Timer.ElapsedMilliseconds);
            SceneCharacterProxy proxy = null;
            if (_this.Proxys.TryGetValue(msg.CharacterId, out proxy))
            {
                Logger.Info("Enter Game {0} UnloadData - 2 - {1}", msg.CharacterId,
                    TimeManager.Timer.ElapsedMilliseconds);
                var co = CoroutineFactory.NewSubroutine(OnLost, coroutine, proxy);
                if (co.MoveNext())
                {
                    yield return co;
                }
                Logger.Info("Enter Game {0} UnloadData - 3 - {1}", msg.CharacterId,
                    TimeManager.Timer.ElapsedMilliseconds);
                _this.Proxys.Remove(msg.CharacterId);
            }
            msg.Reply();
        }

        public IEnumerator PrepareDataForEnterGame(Coroutine coroutine,
                                                   SceneService _this,
                                                   PrepareDataForEnterGameInMessage msg)
        {
            var characterId = msg.CharacterId;
            var result = AsyncReturnValue<ObjPlayer>.Create();
            var res = result;
            result.Dispose();
            PlayerLog.WriteLog(888, "SceneServer PrepareDataForEnterGame characterId={0},clientId={1}", characterId,
                msg.ClientId);
            PlayerLog.WriteLog(characterId, "-----Scene-----PrepareDataForEnterGame----------{0}", characterId);
            var co =
                CoroutineFactory.NewSubroutine(CharacterManager.Instance.GetOrCreateCharacterController, coroutine,
                    characterId, new object[] {}, false, res);
            if (co.MoveNext())
            {
                yield return co;
            }
            res.Value.ServerId = msg.Request.ServerId;

            Logger.Info("Enter Game {0} - PrepareDataForEnterGame - 1 - {1}", characterId,
                TimeManager.Timer.ElapsedMilliseconds);

            msg.Reply();
        }

        public IEnumerator PrepareDataForCreateCharacter(Coroutine coroutine,
                                                         SceneService _this,
                                                         PrepareDataForCreateCharacterInMessage msg)
        {
            PlayerLog.WriteLog(msg.CharacterId, "----------PrepareDataForCreateCharacter----------{0}", msg.CharacterId);

            var result = AsyncReturnValue<ObjPlayer>.Create();
            var res = result;
            result.Dispose();
            var co = CoroutineFactory.NewSubroutine(CharacterManager.Instance.CreateCharacterController, coroutine,
                msg.CharacterId, result,
                new object[] {msg.Request.Type});
            if (co.MoveNext())
            {
                yield return co;
            }

            if (res.Value == null)
            {
                msg.Reply((int) ErrorCodes.Error_PrepareEnterGameFailed);
                yield break;
            }
            msg.Reply();
        }

        public IEnumerator PrepareDataForCommonUse(Coroutine coroutine,
                                                   SceneService _this,
                                                   PrepareDataForCommonUseInMessage msg)
        {
            msg.Reply();
            return null;
        }

        public IEnumerator PrepareDataForLogout(Coroutine coroutine,
                                                SceneService _this,
                                                PrepareDataForLogoutInMessage msg)
        {
            SceneCharacterProxy proxy;
            if (_this.Proxys.TryGetValue(msg.CharacterId, out proxy))
            {
                var co = CoroutineFactory.NewSubroutine(OnLost, coroutine, proxy);
                if (co.MoveNext())
                {
                    yield return co;
                }
                _this.Proxys.Remove(msg.CharacterId);
            }
            msg.Reply();
        }

        public IEnumerator CreateCharacter(Coroutine coroutine, SceneService _this, CreateCharacterInMessage msg)
        {
            var characterId = msg.CharacterId;
            PlayerLog.WriteLog(msg.CharacterId, "----------PrepareDataForCreateCharacter----------{0}", msg.CharacterId);
            var result = AsyncReturnValue<ObjPlayer>.Create();
            var res = result;
            result.Dispose();
            var co = CoroutineFactory.NewSubroutine(CharacterManager.Instance.CreateCharacterController, coroutine,
                msg.CharacterId, result,
				new object[] { msg.Request.Type, msg.Request.IsGM });
            if (co.MoveNext())
            {
                yield return co;
            }

            if (res.Value == null)
            {
                PlayerLog.WriteLog(msg.CharacterId,
                    "----------PrepareDataForCreateCharacter-----ErrorCodes.Error_PrepareEnterGameFailed-----{0}",
                    msg.CharacterId);
                msg.Reply((int) ErrorCodes.Error_PrepareEnterGameFailed);
                yield break;
            }

            var co1 = CoroutineFactory.NewSubroutine(CharacterManager.Instance.RemoveCharacter, coroutine, characterId);
            if (co1.MoveNext())
            {
                yield return co1;
            }

            msg.Reply();
        }

        public IEnumerator DelectCharacter(Coroutine coroutine, SceneService _this, DelectCharacterInMessage msg)
        {
            var characterId = msg.CharacterId;
            PlayerLog.WriteLog(characterId, "-----Scene-----DelectCharacter----------{0}", characterId);
            var co1 = CoroutineFactory.NewSubroutine(CharacterManager.Instance.DeleteCharacter, coroutine, characterId);
            if (co1.MoveNext())
            {
                yield return co1;
            }
            msg.Reply();
        }

        public IEnumerator SSEnterScene(Coroutine coroutine, SceneService _this, SSEnterSceneInMessage msg)
        {
            var serverControl = (SceneServerControl) _this;
            var characterId = msg.CharacterId;

            Logger.Info("Enter Game {0} guid:{1} - SSEnterScene - 1 - {2}", characterId, msg.Request.Guid,
                TimeManager.Timer.ElapsedMilliseconds);

            var newSceneGuid = msg.Request.Guid;
            //Logger.Fatal("SSEnterScene newSceneGuid ={0},ApplyGuid={1}", newSceneGuid, msg.Request.ApplyGuid);

            var newCharacter = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (newCharacter != null)
            {
                Logger.Info("Enter Game {0} - SSEnterScene - 2 - {1}", characterId,
                    TimeManager.Timer.ElapsedMilliseconds);
                if (newCharacter.Scene != null)
                {
                    PlayerLog.WriteLog(888, "SSEnterScene findCharacter!findOldScene! characterId={0},newScene={1}",
                        characterId, newSceneGuid);
                    Logger.Info("Enter Game {0} {1} {2}- SSEnterScene - 3 - {3}", characterId, newCharacter.Scene.Guid,
                        msg.Request.Guid, TimeManager.Timer.ElapsedMilliseconds);
                    if (newCharacter.Scene.Guid != msg.Request.Guid)
                    {
                        Logger.Info("Enter Game {0} - SSEnterScene - 4 - {1}", characterId,
                            TimeManager.Timer.ElapsedMilliseconds);
                        SceneManager.Instance.LevelScene(newCharacter);
                        Logger.Info("Enter the scene in this server. character:{0} , scene guid:{1}", characterId,
                            newSceneGuid);
                        var sceneGuid = msg.Request.Guid;
                        var applyGuid = msg.Request.ApplyGuid;
                        var changeType = msg.Request.ChangeType;
                        var param = msg.Request.SceneParam;
                        OnEnterScenenCharacter(newCharacter, sceneGuid, applyGuid, changeType, param);
                    }
                }
                else
                {
                    PlayerLog.WriteLog(888, "SSEnterScene findCharacter!not findOldScene! characterId={0},newScene={1}",
                        characterId, newSceneGuid);
                    Logger.Info("Enter Game {0} - SSEnterScene - 5 - {1}", characterId,
                        TimeManager.Timer.ElapsedMilliseconds);
                    newCharacter = CharacterManager.Instance.GetCharacterControllerFromMemroy(newCharacter.ObjId);
                    var sceneGuid = msg.Request.Guid;
                    var applyGuid = msg.Request.ApplyGuid;
                    var changeType = msg.Request.ChangeType;
                    var param = msg.Request.SceneParam;
                    OnEnterScenenCharacter(newCharacter, sceneGuid, applyGuid, changeType, param);
                }
                yield break;
            }
            Logger.Info("Enter Game {0} - SSEnterScene - 6 - {1}", characterId, TimeManager.Timer.ElapsedMilliseconds);
            {
                var result = AsyncReturnValue<ObjPlayer>.Create();
                var res = result;
                result.Dispose();
                var co =
                    CoroutineFactory.NewSubroutine(CharacterManager.Instance.GetOrCreateCharacterController, coroutine,
                        characterId, new object[] {}, false, res);
                if (co.MoveNext())
                {
                    yield return co;
                }

                var character = res.Value;
                PlayerLog.WriteLog(888,
                    "SSEnterScene not Character!not OldScene! characterId={0},newScene={1},dbScene={2}", characterId,
                    newSceneGuid, character.mDbData.SceneGuid);
                var sceneGuid = msg.Request.Guid;
                var applyGuid = msg.Request.ApplyGuid;
                var changeType = msg.Request.ChangeType;
                var param = msg.Request.SceneParam;
                OnEnterScenenCharacter(character, sceneGuid, applyGuid, changeType, param);

                // After send this message to gate, gate will send all message to this server from now on.
                if (character.Proxy != null)
                {
                    Logger.Info("Enter Game {0} - SSEnterScene - 7 - {1}", characterId,
                        TimeManager.Timer.ElapsedMilliseconds);
                    serverControl.NotifyGateCharacterChangeToThisServer(character.Proxy.CharacterId,
                        character.Proxy.ClientId);
                }
            }
        }

        //同步家园数据到场景服务器
        public IEnumerator NotifyScenePlayerCityData(Coroutine coroutine,
                                                     SceneService _this,
                                                     NotifyScenePlayerCityDataInMessage msg)
        {
            var sceneGuid = msg.Request.SceneGuid;

            var scene = SceneManager.Instance.GetScene(sceneGuid);
            if (null == scene)
            {
                yield break;
            }

            var cityScene = scene as SceneCity;
            if (null == cityScene)
            {
                Logger.Fatal("scene is not cityScene");
                yield break;
            }

            cityScene.SyncCityData(msg.Request.Data.Data);
        }

        //Logic玩家希望进入天梯场景
        public IEnumerator SSGoToSceneAndPvP(Coroutine coroutine, SceneService _this, SSGoToSceneAndPvPInMessage msg)
        {
            var newCharacter = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (newCharacter == null)
            {
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }

            if (newCharacter.IsChangingScene())
            {
                msg.Reply((int) ErrorCodes.Unknow);
                Logger.Warn("SSGoToSceneAndPvP newCharacter.IsChangingScene()      Character={0}", newCharacter.ObjId);
                yield break;
            }

            var pvpId = msg.Request.PvPcharacterId;
            newCharacter.mDbData.P1vP1CharacterId = pvpId;
            newCharacter.MarkDbDirty();
            var scene = Table.GetScene(msg.Request.SceneId);

            var error = AsyncReturnValue<ErrorCodes>.Create();
            var result = error;
            error.Dispose();
            var gmCo = CoroutineFactory.NewSubroutine(GameMaster.NotifyCreateChangeSceneCoroutine, coroutine,
                newCharacter, msg.Request.SceneId, (int) scene.Entry_x, (int) scene.Entry_z, result);
            if (gmCo.MoveNext())
            {
                yield return gmCo;
            }
            msg.Reply((int) result.Value);

            if (result.Value == ErrorCodes.OK && scene.FubenId >= 0)
            {
                try
                {
                    var tbFuben = Table.GetFuben(scene.FubenId);
                    if (tbFuben != null)
                    {
                        var v = string.Format("fuben#{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                        msg.CharacterId,
                        scene.FubenId,
                        tbFuben.Name,
                        tbFuben.AssistType,
                        0,  // 0  进入   1 完成   2 退出
                        DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                        newCharacter.GetLevel(),
                        newCharacter.Attr.GetFightPoint()
                        ); // 时间
                        PlayerLog.Kafka(v);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                }
            }
        }

        //设置PvP的场景阵营
        public IEnumerator SSPvPSceneCampSet(Coroutine coroutine, SceneService _this, SSPvPSceneCampSetInMessage msg)
        {
            var newCharacter = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (newCharacter == null)
            {
                //msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }
            newCharacter.mDbData.P1vP1CharacterId = (ulong) msg.Request.Type;
            newCharacter.MarkDbDirty();
        }

        public IEnumerator SSGetFubenStoreItems(Coroutine coroutine, SceneService _this, SSGetFubenStoreItemsInMessage msg)
        {
            var charId = msg.CharacterId;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(charId);
            if (character == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }

            var scene = SceneManager.Instance.GetScene(character.mDbData.SceneGuid);
            var dungeonScene = scene as DungeonScene;
            if (dungeonScene == null)
            {
                msg.Reply((int)ErrorCodes.Error_NoScene);
                yield break;
            }

            Dictionary<int, int> mapCounts;
            if (!dungeonScene.MapShopItems.TryGetValue(msg.Request.ShopType, out mapCounts))
            {
                msg.Reply((int)ErrorCodes.Error_NoDungeonShopItems);
                yield break;
            }

            var tempList = new List<StoneItem>();
            var ids = new Int32Array();
            ids.Items.Add((int)eExdataDefine.e752);
            var msg1 = SceneServer.Instance.LogicAgent.SSFetchExdata(charId, ids);
            yield return msg1.SendAndWaitUntilDone(coroutine);
            if (msg1.State != MessageState.Reply)
            {
                Logger.Error("SSFetchExdata return with state = {0}", msg1.State);
                yield break;
            }
            if (msg1.ErrorCode != (int)ErrorCodes.OK)
            {
                Logger.Error("SSFetchExdata return with err = {0}", msg1.ErrorCode);
                yield break;
            }
            var refreshFlag = msg1.Response.Items[0];
            if (refreshFlag == 1)//今日已刷新
            {
                if (SceneServerControl.BlackStoreItems != null && SceneServerControl.BlackStoreItems.Count != 0)
                {
                    tempList = SceneServerControl.BlackStoreItems;
                }
                else
                {
                    GetFubenStoreItems(msg.Request.ShopType, mapCounts, tempList);
                    SceneServerControl.BlackStoreItems = tempList;
                }
            }
            else//今日未刷新
            {
                GetFubenStoreItems(msg.Request.ShopType, mapCounts, tempList);
                SceneServerControl.BlackStoreItems = tempList;
                Dict_int_int_Data change = new Dict_int_int_Data();
                change.Data.Add((int)eExdataDefine.e752, 1);
                var msg2 = SceneServer.Instance.LogicAgent.SSSetExdata(charId, change);
                yield return msg2.SendAndWaitUntilDone(coroutine);
            }
            if (tempList.Count == 0)
            {
                msg.Reply((int)ErrorCodes.Error_NoDungeonShopItems);
                yield break;
            }
            msg.Response.items.AddRange(tempList);
            msg.Reply((int)ErrorCodes.OK);
        }

        public IEnumerator SSGetFubenStoreItemCount(Coroutine coroutine, SceneService _this,
            SSGetFubenStoreItemCountInMessage msg)
        {
            var charId = msg.CharacterId;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(charId);
            if (character == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }

            var scene = SceneManager.Instance.GetScene(character.mDbData.SceneGuid);
            var dungeonScene = scene as DungeonScene;
            if (dungeonScene == null)
            {
                msg.Reply((int)ErrorCodes.Error_NoScene);
                yield break;
            }

            Dictionary<int, int> countMap;
            if (!dungeonScene.MapShopItems.TryGetValue(msg.Request.ShopType, out countMap))
            {
                msg.Reply((int)ErrorCodes.Error_NoDungeonShopItems);
                yield break;
            }

            var itemCount = 0;
            if (!countMap.TryGetValue(msg.Request.Id, out itemCount))
            {
            }

            msg.Response = itemCount;
            msg.Reply((int)ErrorCodes.OK);
        }

        public IEnumerator SSChangeFubenStoreItem(Coroutine coroutine, SceneService _this,
            SSChangeFubenStoreItemInMessage msg)
        {
            var charId = msg.CharacterId;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(charId);
            if (character == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }

            var scene = SceneManager.Instance.GetScene(character.mDbData.SceneGuid);
            var dungeonScene = scene as DungeonScene;
            if (dungeonScene == null)
            {
                msg.Reply((int)ErrorCodes.Error_NoScene);
                yield break;
            }

            Dictionary<int, int> countMap;
            if (!dungeonScene.MapShopItems.TryGetValue(msg.Request.ShopType, out countMap))
            {
                msg.Reply((int)ErrorCodes.Error_NoDungeonShopItems);
                yield break;
            }

            var itemId = msg.Request.Id;
            var itemCount = 0;
            if (!countMap.TryGetValue(itemId, out itemCount))
            {
                msg.Reply((int)ErrorCodes.Error_NoDungeonShopItems);
                yield break;
            }

            if (itemCount >= msg.Request.Num)
            {
                countMap[itemId] -= msg.Request.Num;

                var notify = new StoneItems();
                var storeRecord = Table.GetStore(itemId);
                if (storeRecord == null)
                {
                    msg.Response = false;
                    msg.Reply((int)ErrorCodes.Error_GoodId_Not_Exist);
                    yield break;
                }

                var item = new StoneItem();
                item.itemid = itemId;
                item.itemcount = countMap[itemId];
                notify.items.Add(item);
                dungeonScene.PushActionToAllPlayer(p =>
                {
                    if (p.Proxy == null)
                        return;
                    p.Proxy.SyncFuBenStore(notify, storeRecord.Type);
                });

                msg.Response = true;
                msg.Reply((int)ErrorCodes.OK);
                yield break;
            }
  
            msg.Response = false;
            msg.Reply((int)ErrorCodes.Error_DungeonShopItemsNotEnough);
        }

	    public IEnumerator SSBookFightingMonsterId(Coroutine coroutine, SceneService _this, SSBookFightingMonsterIdInMessage msg)
	    {
			var charId = msg.CharacterId;
			var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(charId);
			if (character == null)
			{
				yield break;
			}
			character.SetBookMonsterId(msg.Request.HandbookId);
			character.SummonBookMonster();
            yield break;
	    }

        public IEnumerator UpdateHoldId(Coroutine coroutine, SceneService _this,
            UpdateHoldIdInMessage msg)
        {
            {//修改对应场景的占领公会Id
                //aljdkl;ja;fkjakl;ga;jfdgfdhgjkgkl
                
            }
            yield break;
        }
        public IEnumerator MissionChangeSceneRequest(Coroutine coroutine, SceneService _this, MissionChangeSceneRequestInMessage msg)
        {
            var charId = msg.CharacterId;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(charId);
            if (character == null)
            {
                yield break;
            }
            var transId = msg.Request.TransId;

            if (null == character.Scene)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                Logger.Warn("MissionChangeSceneRequest null==Scene      Character{0}", charId);
                yield break;
            }

            if (character.IsChangingScene())
            {
                msg.Reply((int)ErrorCodes.StateError);
                Logger.Warn("SendTeleportRequest proxy.Character.IsChangingScene()      Character{0}", charId);
                yield break;
            }

            var tableData = Table.GetTransfer(transId);
            if (null == tableData)
            {
                Logger.Info("DataTable.Table.GetTransfer({0})==null", transId);
                msg.Reply((int)ErrorCodes.Error_NoTransfer);
                yield break;
            }

            var tableScene = Table.GetScene(tableData.ToSceneId);
            if (null == tableScene)
            {
                Logger.Info("DataTable.Table.GetScene(tableData.ToSceneId)==null", tableData.ToSceneId);
                msg.Reply((int)ErrorCodes.Error_NoTransfer);
                yield break;
            }

            if (character.GetLevel() < tableScene.LevelLimit)
            {
                Logger.Info("DataTable.Table.GetTransfer({0})==null", transId); //正常现象
                msg.Reply((int)ErrorCodes.Error_LevelNoEnough);
                yield break;
            }

            var param = new SceneParam();
            param.Param.Add(transId);

            var co = CoroutineFactory.NewSubroutine(SceneServer.Instance.ServerControl.CreateAndEnterScene,
                coroutine,
                msg.CharacterId, character.ServerId,
                tableData.ToSceneId,
                0ul,
                eScnenChangeType.Transfer,
                param);
            if (co.MoveNext())
            {
                yield return co;
            }
            msg.Reply();
        }

	    //通知好友上线了，并且取SimpleData
        public IEnumerator GetFriendSceneSimpleData(Coroutine coroutine,
                                                    SceneService _this,
                                                    GetFriendSceneSimpleDataInMessage msg)
        {
            var GetId = msg.Request.GetId;
            CharacterManager.Instance.GetSimpeData(GetId, simple =>
            {
                if (simple == null)
                {
                    msg.Reply((int) ErrorCodes.Unknow);
                    return;
                }
                msg.Response.Id = simple.Id;
                msg.Response.TypeId = simple.TypeId;
                msg.Response.Name = simple.Name;
                msg.Response.SceneId = simple.SceneId;
                msg.Response.FightPoint = simple.FightPoint;
                msg.Response.Level = simple.Level;
                msg.Response.Ladder = simple.Ladder;
                msg.Response.ServerId = simple.ServerId;
                msg.Response.CheckAttr.AddRange(simple.CheckAttr);
                msg.Response.AttrList.AddRange(simple.AttrList);
                msg.Response.Vip = simple.Vip;
                msg.Response.StarNum = simple.StarNum;
                var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(GetId);
                if (character != null)
                {
                    character.PushFriend(msg.Request.Type, msg.Request.HaveId);
                }
                msg.Reply();
            });
            return null;
        }

        //通知添加好友了
        public IEnumerator SendAddFriend(Coroutine coroutine, SceneService _this, SendAddFriendInMessage msg)
        {
            var GetId = msg.Request.GetId;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(GetId);
            if (character != null)
            {
                character.PushFriend(msg.Request.Type, msg.Request.HaveId);
            }
            yield break;
        }

        //通知删除好友了
        public IEnumerator SendDeleteFriend(Coroutine coroutine, SceneService _this, SendDeleteFriendInMessage msg)
        {
            var GetId = msg.Request.GetId;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(GetId);
            if (character != null)
            {
                character.RemoveFriend(msg.Request.Type, msg.Request.HaveId);
            }
            yield break;
        }

        //通知好友下线了
        public IEnumerator SendOutLineFriend(Coroutine coroutine, SceneService _this, SendOutLineFriendInMessage msg)
        {
            var GetId = msg.Request.GetId;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(GetId);
            if (character != null)
            {
                character.RemoveFriend(msg.Request.Type, msg.Request.HaveId);
            }
            yield break;
        }

        public IEnumerator SSAllianceBuffDataChange(Coroutine coroutine,
                                                    SceneService _this,
                                                    SSAllianceBuffDataChangeInMessage msg)
        {
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (character != null)
            {
                var buff = Table.GetGuildBuff(msg.Request.BuffId);
                if (buff != null)
                {
                    character.AddBuff(buff.BuffID, buff.BuffLevel, character);
                    character.Attr.SetFightPointFlag();
                }
                //foreach (int BuffId in msg.Request.BuffList.Items)
                //{
                //    var buff = Table.GetGuildBuff(BuffId);
                //    if (buff != null)
                //    {
                //        character.AddBuff(buff.BuffID, buff.BuffLevel, character);
                //    }
                //}
            }
            yield break;
        }
        public IEnumerator SyncTowerSkillLevel(Coroutine coroutine, SceneService _this, SyncTowerSkillLevelInMessage msg)
        {
            MieShiWar scenes = SceneManager.Instance.GetScene(msg.Request.SceneGuid) as MieShiWar;
            if (null == scenes)
            {
                yield break;
            }
            scenes.SetMonsterSkillLevel(msg.Request.TowerId, msg.Request.Level);
        }
        public IEnumerator SyncPlayerMieshiContribution(Coroutine coroutine, SceneService _this, SyncPlayerMieshiContributionInMessage msg)
        {
            MieShiWar scenes = SceneManager.Instance.GetScene(msg.Request.SceneGuid) as MieShiWar;
            if (null == scenes)
            {
                yield break;
            }
            scenes.UpdatePlayerContribution(msg.Request.CharacterId, msg.Request.Contribution,msg.Request.Name,msg.Request.Rate);
        }

        
        //战盟发生变化
        public IEnumerator SSAllianceDataChange(Coroutine coroutine,
                                                SceneService _this,
                                                SSAllianceDataChangeInMessage msg)
        {
            var obj = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (obj != null)
            {
                if (msg.Request.Type == 0)
                {
                    var alliance = SceneServer.Instance.TeamAgent.SSGetAllianceData(obj.ObjId, obj.ServerId);
                    yield return alliance.SendAndWaitUntilDone(coroutine);
                    if (alliance.State == MessageState.Reply)
                    {
                        if (alliance.ErrorCode == (int) ErrorCodes.OK)
                        {
                            var response = alliance.Response;
                            obj.SetAllianceInfo(response.AllianceId, response.Ladder, response.Name);
                        }
                    }
                }
                else
                {
                    obj.SetAllianceInfo(0, 0, "");
                }
            }
        }

        public IEnumerator GetCharacterData(Coroutine co, SceneService _this, GetCharacterDataInMessage msg)
        {
            var charController = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.Request.Id);
            if (charController != null)
            {
                var charDetailInfo = new GMCharacterDetailInfo();
                charDetailInfo.Name = charController.GetName();
                charDetailInfo.SceneId = (int) charController.CurrentSceneTypeId;
                charDetailInfo.SceneGuid = charController.Scene.Guid;
                charDetailInfo.X = charController.GetPosition().X;
                charDetailInfo.Y = charController.GetPosition().Y;
                charDetailInfo.fight_value = charController.Attr.GetFightPoint();//统计需要新增角色战斗力
                charDetailInfo.AttrList.Clear();
                if (charController.Attr != null && charController.Attr.mData != null)
                {
                    charDetailInfo.AttrList.AddRange(charController.Attr.mData);
                }

                msg.Response = charDetailInfo;
                msg.Reply();
            }
            else
            {
                var data = SceneServer.Instance.DB.Get<DBCharacterScene>(co, DataCategory.SceneCharacter,
                    msg.CharacterId);
                yield return data;

                // can not get data from db
                if (data.Data == null)
                {
                    Logger.Error("can not load character {0} 's data from db.", msg.CharacterId);
                    msg.Reply((int) ErrorCodes.Error_CharacterId_Not_Exist);
                    yield break;
                }
                var sdata = SceneServer.Instance.DB.Get<DBCharacterSceneSimple>(co, DataCategory.SceneCharacter,
                    "__s_:" + msg.CharacterId);
                yield return sdata;

                // can not get data from db
                if (sdata.Data == null)
                {
                    Logger.Error("can not load character {0} 's data from db.", msg.CharacterId);
                    msg.Reply((int) ErrorCodes.Error_CharacterId_Not_Exist);
                    yield break;
                }

                var charDetailInfo = new GMCharacterDetailInfo();
                charDetailInfo.Name = sdata.Data.Name;
                charDetailInfo.fight_value = sdata.Data.FightPoint;//统计需要新增角色战斗力
                charDetailInfo.SceneId = (int) data.Data.SceneId;
                charDetailInfo.SceneGuid = data.Data.SceneGuid;
                charDetailInfo.X = data.Data.Postion.X;
                charDetailInfo.Y = data.Data.Postion.Y;
                msg.Response = charDetailInfo;
                msg.Reply();
            }
        }

        public IEnumerator NotifyCreateSpeMonster(Coroutine coroutine,
                                                  SceneService _this,
                                                  NotifyCreateSpeMonsterInMessage msg)
        {
            SceneManager.Instance.CreateSpeMonsters(msg.Request.Ids.Items);
			Logger.Info("NotifyCreateSpeMonster[{0}]", msg.Request.Ids.Items);
            yield return null;
        }
        public IEnumerator NotifyRefreshBossHome(Coroutine coroutine,
                                                  SceneService _this,
                                                  NotifyRefreshBossHomeInMessage msg)
        {
            SceneManager.Instance.FreshBossHome(msg.Request.Ids.Items);
            Logger.Info("NotifyCreateSpeMonster[{0}]", msg.Request.Ids.Items);
            yield return null;
        }


        public IEnumerator NotifyBossHomeKill(Coroutine coroutine,
                                                  SceneService _this,
                                                  NotifyBossHomeKillInMessage msg)
        {
            SceneManager.Instance.KillAllBoss();
            yield return null;
        }

        public IEnumerator CheckCanAcceptChallenge(Coroutine coroutine, SceneService _this,
            CheckCanAcceptChallengeInMessage msg)
        {
            // 校验被邀请决斗的玩家状态
            var characterId = msg.CharacterId;
            var player = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);

            //玩家不在线
            if (null == player)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }

            //玩家在副本里
            if (-1 != player.Scene.TableSceneData.FubenId)
            {
                msg.Reply((int) ErrorCodes.Error_Cannot_Challenge);
                yield break;
            }

            msg.Reply((int)ErrorCodes.OK);
        }

        public IEnumerator SSGetCharacterSceneData(Coroutine coroutine,
                                                   SceneService _this,
                                                   SSGetCharacterSceneDataInMessage msg)
        {
            var charController = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.Request.CharacterId);
            if (charController != null)
            {
                if (charController.Scene == null)
                {
                    msg.Reply((int) ErrorCodes.Error_NoScene);
                    yield break;
                }
                msg.Response.SceneGuid = charController.Scene.Guid;
                msg.Response.SceneId = charController.Scene.TypeId;
                msg.Response.ServerId = charController.ServerId;
                msg.Response.ObjId = msg.Request.CharacterId;
                if (msg.Response.Pos == null)
                {
                    msg.Response.Pos = new PositionData();
                }
                if (msg.Response.Pos.Pos == null)
                {
                    msg.Response.Pos.Pos = new Vector2Int32();
                }
                msg.Response.Pos.Pos.x = (int) (charController.GetPosition().X*100);
                msg.Response.Pos.Pos.y = (int) (charController.GetPosition().Y*100);
                msg.Reply();
                yield break;
            }
            msg.Reply((int) ErrorCodes.Error_Character_Data_Not_Exist_In_Memory);
        }

        public IEnumerator ReadyToEnter(Coroutine coroutine, SceneService _this, ReadyToEnterInMessage msg)
        {
            if (SceneServer.Instance.IsReadyToEnter && SceneServer.Instance.AllAgentConnected())
            {
                msg.Response = 1;
            }
            else
            {
                msg.Response = 0;
            }

            msg.Reply();

            return null;
        }

        public IEnumerator CreateAndEnterScene(Coroutine coroutine,
                                               SceneService _this,
                                               ulong characterId,
                                               int serverId,
                                               int sceneId,
                                               ulong sceneGuid,
                                               eScnenChangeType changeType,
                                               SceneParam sceneParam)
        {
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (character == null)
            {
                yield break;
            }

            character.BeginChangeScene();

	        var tbScene = Table.GetScene(sceneId);
			if (null != tbScene)
	        {
		        if (0 == tbScene.SwapLine)
		        {//不能开多线的场景
			        var msgRequestSceneInfo = SceneServer.Instance.SceneAgent.RequestSceneInfo(characterId, serverId, sceneId);
					yield return msgRequestSceneInfo.SendAndWaitUntilDone(coroutine);

					if (msgRequestSceneInfo.State != MessageState.Reply)
					{
						character.EndChangeScene();
						yield break;
					}

			        if (msgRequestSceneInfo.HasReturnValue)
			        {
				        var infos = msgRequestSceneInfo.Response.Info;
				        if (infos.Count > 0)
				        {//如果人数大于场景人数上线了，就不让进了
					        if (infos[0].PlayerCount >= tbScene.PlayersMaxB)
					        {
						        character.Proxy.NotifyMessage((int) eSceneNotifyType.Dictionary, 200005035.ToString(), 1);
						        character.EndChangeScene();
						        yield break;
					        }
					        else
					        {
						        sceneGuid = infos[0].Guid;
					        }
				        }
#if DEBUG
				        if (infos.Count > 1)
				        {
							Logger.Fatal("tbScene[{0}]   infos.Count={1}",tbScene.Id, infos.Count);
				        }
#endif
			        }
		        }
				
	        }
	        

            var targetServerId = serverId;
            //if (targetServerId == -1)
            //{
            //    targetServerId = character.ServerId;
            //}

            var sceneDB = character.GetData();
            var targetSceneId = sceneId;
            if (targetSceneId == -1)
            {
                var orginSceneId = (int) sceneDB.SceneId;
                if (!SceneExtension.IsNormalScene(sceneId))
                {
                    targetSceneId = (int) sceneDB.FormerSceneId;
                }
                else
                {
                    targetSceneId = orginSceneId;
                }
            }

            Logger.Info("Start SBChangeScene character:{0} to server:{1}, scene:{2}", character.ObjId,
                targetServerId, targetSceneId);

            if (character.GetTeamId() != 0 && SceneExtension.IsNormalScene(sceneId))
            {
                var dbTeamPos = SceneServer.Instance.TeamAgent.SSGetTeamSceneData(characterId, characterId);
                yield return dbTeamPos.SendAndWaitUntilDone(coroutine);
                if (dbTeamPos.State == MessageState.Reply)
                {
                    if (dbTeamPos.ErrorCode == (int) ErrorCodes.OK)
                    {
                        foreach (var sceneData in dbTeamPos.Response.Objs)
                        {
                            if (sceneData.SceneId == targetSceneId && serverId == sceneData.ServerId)
                            {
                                sceneGuid = sceneData.SceneGuid;
                                var csi = new ChangeSceneInfo();
                                csi.Guids.Add(characterId);
                                csi.SceneGuid = sceneGuid;
                                csi.SceneId = targetSceneId;

                                //切换场景时，根据合服ID进行
                                var serverLogicId2 = SceneExtension.GetServerLogicId(targetServerId);
                                csi.ServerId = serverLogicId2;
                                csi.Type = (int) changeType;
                                csi.Pos = sceneParam;

                                PlayerLog.WriteLog(888,
                                    "SBChangeScene characterId={0},ServerId={1},SceneId={2},SceneGuid={3}", characterId,
                                    targetServerId, targetSceneId, sceneGuid);

                                var toTeamchangeSceneMsg =
                                    SceneServer.Instance.SceneAgent.SBChangeSceneByTeam(character.ObjId, csi);
                                yield return toTeamchangeSceneMsg.SendAndWaitUntilDone(coroutine);

                                yield break;
                            }
                        }
                    }
                }
            }

            PlayerLog.WriteLog(888, "SBChangeScene characterId={0},ServerId={1},SceneId={2},SceneGuid={3}", characterId,
                serverId, sceneId, sceneGuid);

            //切换场景时，根据合服ID进行
            var serverLogicId = SceneExtension.GetServerLogicId(targetServerId);
            var changeSceneMsg = SceneServer.Instance.SceneAgent.SBChangeScene(character.ObjId, character.ObjId,
                serverLogicId, targetSceneId, sceneGuid, (int) changeType, sceneParam);

            yield return changeSceneMsg.SendAndWaitUntilDone(coroutine);

            if (changeSceneMsg.State != MessageState.Reply)
            {
                character.EndChangeScene();
                yield break;
            }

            Logger.Info("Start SBChangeScene character:{0} to scene guid:{1}", character.ObjId,
                changeSceneMsg.Response);
        }

        public IEnumerator SSExitDungeon(Coroutine co, SceneService _this, SSExitDungeonInMessage msg)
        {
            var characterId = msg.CharacterId;
            var player = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (player == null)
            {
                msg.Reply((int) ErrorCodes.Unline);
                yield break;
            }
            var co1 = CoroutineFactory.NewSubroutine(player.ExitDungeon, co);
            if (co1.MoveNext())
            {
                yield return co1;
            }
            msg.Reply();
        }

        public IEnumerator Silence(Coroutine co, SceneService _this, SilenceInMessage msg)
        {
            var id = msg.CharacterId;
            var mask = msg.Request.Mask;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(id);
            if (character == null)
            {
                var dbCharacter = SceneServer.Instance.DB.Get<DBCharacterScene>(co, DataCategory.SceneCharacter,
                    id);
                yield return dbCharacter;
                dbCharacter.Data.BannedToPost = mask;
                var dbSet = SceneServer.Instance.DB.Set(co, DataCategory.SceneCharacter, id, dbCharacter.Data);
                yield return dbSet;
            }
            else
            {
                character.mDbData.BannedToPost = mask;
                character.MarkDbDirty();
            }
            msg.Reply();
        }

        public IEnumerator SyncExData(Coroutine coroutine, SceneService _this, SyncExDataInMessage msg)
        {
            var characterId = msg.CharacterId;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (character == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }

            var t1 = msg.Request.Changes;
            foreach (var data in t1.Data)
            {
                character.OnExDataChanged(data.Key, data.Value);
            }

            msg.Reply();
        }
    }

    public interface IStaticSceneServerControl
    {
        IEnumerator CreateAndEnterScene(Coroutine coroutine,
                                        SceneService _this,
                                        ulong characterId,
                                        int serverId,
                                        int sceneId,
                                        ulong sceneGuid,
                                        eScnenChangeType changeType,
                                        SceneParam sceneParam);
    }

    public class SceneServerControl : SceneService
    {
        //心跳频率
        public const float Frequence = 20.0f;
        //每帧时长
        public const float Performance = 1/Frequence;
        public static TimedTaskManager TaskManager = new TimedTaskManager();
        public static TimeManager Timer = new TimeManager();
        public static Dictionary<int, List<StoreRecord>> StoreTypeItems = new Dictionary<int, List<StoreRecord>>();
        public static List<StoneItem> BlackStoreItems = new List<StoneItem>();
        public SceneServerControl()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (SceneServerControl),
                typeof (SceneServerControlDefaultImpl),
                o => { SetServiceImpl((ISceneService) o); });
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (SceneProxy), typeof (SceneProxyDefaultImpl),
                o => { SetProxyImpl((ISceneCharacterProxy) o); });
            
        }

        public float LastFrameTime;
        public float LastTime;
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public int PathFindingCacheHit;
        public int PathFindingCount;
        public Stopwatch PathFindingTimer = new Stopwatch();
        public int TickCount;
        public int TickCountPerSecond;
        public Stopwatch Watch = new Stopwatch();
        private long tickTime = 0;

        public bool CheckLost(ulong characterId)
        {
            return !Proxys.ContainsKey(characterId);
        }

        //进或者造场景
        public IEnumerator CreateAndEnterScene(Coroutine coroutine,
                                               ulong characterId,
                                               int serverId,
                                               int sceneId,
                                               ulong sceneGuid,
                                               eScnenChangeType changeType,
                                               SceneParam sceneParam)
        {
            PlayerLog.WriteLog(888, "CreateAndEnterScene characterId={0},ServerId={1},SceneId={2},SceneGuid={3}",
                characterId, serverId, sceneId, sceneGuid);
            return ((IStaticSceneServerControl) mImpl).CreateAndEnterScene(coroutine, this, characterId, serverId,
                sceneId,
                sceneGuid, changeType, sceneParam);
        }

        public int GetPlayerCount()
        {
            return Proxys.Count;
        }

        public void LookProxy()
        {
            foreach (var proxy in Proxys)
            {
                Logger.Info("proxy={0}", proxy.Key);
            }
        }

        public override SceneCharacterProxy NewCharacterIn(ulong characterId, ulong clientId)
        {
            return new SceneProxy(this, characterId, clientId);
        }

        public override void OnException(Exception ex)
        {
            Logger.Error(ex, "Network error");
        }

        public override IEnumerator OnServerStart(Coroutine coroutine)
        {
            return mImpl.OnServerStart(coroutine, this);
        }

        public override IEnumerator OnServerStop(Coroutine coroutine)
        {
            return mImpl.OnServerStop(coroutine, this);
        }

        public override IEnumerator PerformenceTest(Coroutine coroutine, ServerClient client, ServiceDesc desc)
        {
            client.SendMessage(desc);
            yield break;
        }

        public IEnumerator SendUseSkillRequest(Coroutine coroutine,
                                               SceneCharacterProxy charProxy,
                                               SendUseSkillRequestInMessage msg)
        {
            return mProxyImpl.SendUseSkillRequest(coroutine, charProxy, msg);
        }

        public void GetFubenStoreItems(int type, Dictionary<int, int> mapCounts, List<StoneItem> outList)
        {
            var o = mImpl as SceneServerControlDefaultImpl;
            if (o != null)
            {
                o.GetFubenStoreItems(type, mapCounts, outList);
            }
        }

        public void InitFubenStoreCounts(int type, Dictionary<int, int> outMap)
        {
            var o = mImpl as SceneServerControlDefaultImpl;
            if (o != null)
            {
                o.InitFubenStoreCounts(type, outMap);
            }
        }
        public void Status(ConcurrentDictionary<string, string> dict)
        {
            try
            {
                dict.TryAdd("_Listening", Listening.ToString());
                dict.TryAdd("Started", Started.ToString());
                dict.TryAdd("TickTime", tickTime.ToString());
                //dict.TryAdd("ByteReceivedPerSecond", ByteReceivedPerSecond.ToString());
                //dict.TryAdd("ByteSendPerSecond", ByteSendPerSecond.ToString());
                //dict.TryAdd("MessageReceivedPerSecond", MessageReceivedPerSecond.ToString());
                //dict.TryAdd("MessageSendPerSecond", MessageSendPerSecond.ToString());
                //dict.TryAdd("ConnectionCount", ConnectionCount.ToString());
                ////dict.TryAdd("WaitingSendMessageCount", WaitingSendMessageCount.ToString());
                //dict.TryAdd("CharacterCount", CharacterManager.Instance.CharacterCount().ToString());
                //dict.TryAdd("BuffCount", Zone.totalBuff.ToString());
                //dict.TryAdd("BuffSendCount", Zone.totalBuffSend.ToString());
                //dict.TryAdd("PathFindingCount", PathFindingCount.ToString());
                //dict.TryAdd("PathFindingCacheHit", PathFindingCacheHit.ToString());
                //dict.TryAdd("PathFindingTime(ms)", PathFindingTimer.ElapsedMilliseconds.ToString());
                //dict.TryAdd("WaitingReplyMessage", OutMessage.WaitingMessageCount.ToString());
                //dict.TryAdd("TickCountPerSecond", TickCountPerSecond.ToString());

                //TickCountPerSecond = 0;
                //PathFindingCount = 0;
                //PathFindingCacheHit = 0;
                //PathFindingTimer.Reset();

                //Zone.totalBuff = 0;
                //Zone.totalBuffSend = 0;

                //foreach (var agent in SceneServer.Instance.Agents.ToArray())
                //{
                //    dict.TryAdd(agent.Key + " Latency", agent.Value.Latency.ToString());
                //    dict.TryAdd(agent.Key + " ByteReceivedPerSecond", agent.Value.ByteReceivedPerSecond.ToString());
                //    dict.TryAdd(agent.Key + " ByteSendPerSecond", agent.Value.ByteSendPerSecond.ToString());
                //    dict.TryAdd(agent.Key + " MessageReceivedPerSecond", agent.Value.MessageReceivedPerSecond.ToString());
                //    dict.TryAdd(agent.Key + " MessageSendPerSecond", agent.Value.MessageSendPerSecond.ToString());
                //}
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "SceneServerControl Status Error!{0}");
            }
        }

        public override IEnumerator Tick(Coroutine coroutine)
        {
            tickTime++;
            return ((ITickable) mImpl).Tick(coroutine, this);
        }
    }
}