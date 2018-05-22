#region using

using System;
using System.Collections.Generic;
using System.IO;
using DataContract;
using DataTable;
using Scorpion;
using ProtoBuf;
using Shared;

#endregion

namespace Broker
{
    public partial class SceneBroker : CommonBroker
    {
        #region 服务器的消息返回  ( 依赖消息类型 ）

        public override void OnSocketListenerMessageReceiveSB(ServerClient client, ServiceDesc desc)
        {
            // 换场景
            if (desc.FuncId == 3000)
            {
                if (!mWaitingEvents.IsAddingCompleted)
                {
                    mWaitingEvents.Add(() => { ChangeScene(client, desc); });
                }
            }
            // 销毁场景
            else if (desc.FuncId == 3001)
            {
                if (!mWaitingEvents.IsAddingCompleted)
                {
                    mWaitingEvents.Add(() => { mSceneManager.DestroyScene(client, desc); });
                }
            }
            //查询scene是否存在
            else if (desc.FuncId == 3002)
            {
                if (!mWaitingEvents.IsAddingCompleted)
                {
                    mWaitingEvents.Add(() => { mSceneManager.NoHaveScene(client, desc); });
                }
            }
            // 检查是否在线
            else if (desc.FuncId == 3039)
            {
                CheckCharacterOnline(client, desc);
            } //广播表格重载
            else if (desc.FuncId == 3050)
            {
                if (!mWaitingEvents.IsAddingCompleted)
                {
                    var info = ProtocolExtension.Deserialize<__RPC_Scene_SBReloadTable_ARG_string_tableName__>(desc.Data);
                    mWaitingEvents.Add(() => { Table.ReloadTable(info.TableName); });
                }
            }
            else if (desc.FuncId == 3061)
            {
                OnNotifyConnected(desc);
            }
            else if (desc.FuncId == 3062)
            {
                OnNotifyLost(desc);
            }
            else if (desc.FuncId == 3063)
            {
                if (!mWaitingEvents.IsAddingCompleted)
                {
                    mWaitingEvents.Add(() => { OnSceneFinished(desc); });
                }
            }
            else if (desc.FuncId == 3069)
            {
                if (!mWaitingEvents.IsAddingCompleted)
                {
                    mWaitingEvents.Add(() => { ChangeSceneByTeam(client, desc); });
                }
            }
            else if (desc.FuncId == 3071)
            {
                if (!mWaitingEvents.IsAddingCompleted)
                {
                    mWaitingEvents.Add(() => { mSceneManager.MergeSceneByTeam(client, desc); });
                }
            }
            else if (desc.FuncId == 3072)
            {
                if (!mWaitingEvents.IsAddingCompleted)
                {
                    mWaitingEvents.Add(() => { mSceneManager.IsSceneExist(client, desc); });
                }
            }
            else if (desc.FuncId == 3499)
            {
                if (!mWaitingEvents.IsAddingCompleted)
                {
                    mWaitingEvents.Add(() => { mSceneManager.SeekSceneIndexByCharacterId(client, desc); });
                }
            }
			else if (desc.FuncId == 3605)
			{
				if (!mWaitingEvents.IsAddingCompleted)
				{
					mWaitingEvents.Add(() => { mSceneManager.RequestSceneInfo(client, desc); });
				}
			}
            else
            {
                base.OnSocketListenerMessageReceiveSB(client, desc);
            }
        }

        //desc.Type == (int)MessageType.BS (多线程）
        public override void OnSocketListenerMessageReceiveBS(ServiceDesc desc)
        {
            var id = desc.PacketId;
            CallbackItem item;
            if (mCallback.TryRemove(id, out item))
            {
                if (!mWaitingEvents.IsAddingCompleted)
                {
                    mWaitingEvents.Add(() =>
                    {
                        mTimeDispatcher.UnregisterTimedEvent(item.TimeHandle);
                        item.Callback(true, desc);
                    });
                }
            }
            else
            {
                Logger.Error(
                    "OnSocketListenerMessageReceiveBS mServiceName={0},FuncId ={1},characterId={2},ServiceType={3}",
                    mServiceName, desc.FuncId, desc.CharacterId, desc.ServiceType);
            }
        }

        //desc.Type == (int)MessageType.SS
        public void SSfunc(ServerClient client, ServiceDesc desc)
        {
            var characterId = desc.CharacterId;
            var character = GetCharacter(characterId);
            if (character != null)
            {
                desc.Routing.Add(client.ClientId);
                if (null != character.SceneInfo && null != character.SceneInfo.Server)
                {
                    Logger.Info("Enter Game {0} - SS - 1 - {1} - {2} - {3} - {4}", characterId, desc.ServiceType,
                        desc.FuncId, character.SceneInfo.Server.RemoteEndPoint, TimeManager.Timer.ElapsedMilliseconds);
                    character.SceneInfo.Server.SendMessage(desc);
                }
                else
                {
                    // 异常日志
                    Logger.Error("SSfunc SceneInfo or Server is null id {0} - ss - 1 - {1} - {2}", characterId,
                        desc.ServiceType, desc.FuncId);
                }
            }
            else
            {
                if (!mWaitingEvents.IsAddingCompleted)
                {
                    mWaitingEvents.Add(() =>
                    {
                        character = GetCharacter(characterId);
                        if (character != null)
                        {
                            Logger.Info("Enter Game {0} - SS - 1 - {1} - {2} - {3} - {4}", characterId, desc.ServiceType,
                                desc.FuncId, character.SceneInfo.Server.RemoteEndPoint,
                                TimeManager.Timer.ElapsedMilliseconds);
                            desc.Routing.Add(client.ClientId);
                            character.SceneInfo.Server.SendMessage(desc);
                        }

                        var serverInfo = SelectServerForCharacter(characterId);
                        //character = new CharacterSceneInfo
                        //{
                        //    CharacterId = characterId,
                        //    SceneInfo = sceneInfo,
                        //    State = CharacterInfoState.PreparedData
                        //};

                        //mFromCharacterId2Server.TryAdd(characterId, character);
                        //Logger.Info("ss to scene: {0}, {1}", characterId, server.SceneInfo.Server.RemoteEndPoint);

                        Logger.Info("SS Not find Character  {0} - SS - 3 - {1} - {2} - {3} - {4}", characterId,
                            desc.ServiceType, desc.FuncId, serverInfo.RemoteEndPoint,
                            TimeManager.Timer.ElapsedMilliseconds);
                        desc.Routing.Add(client.ClientId);
                        serverInfo.SendMessage(desc);
                    });
                }
            }
        }

        #endregion

        #region 服务器的消息返回  ( 依赖FunctionId ）

        //desc.FuncId == 3000
        //单人切换场景
        public void ChangeScene(ServerClient client, ServiceDesc desc)
        {
            var characterId = desc.CharacterId;
            PlayerLog.WriteLog(888, "ChangeScene characterId={0}", characterId);
            Logger.Info("Character {0} chanage scene.", characterId);

            var info = GetCharacter(characterId);
            if (info == null)
            {
                // after prepare data, info must exist.
                Logger.Error("Can not find character info, {0}.", characterId);
                desc.Data = ProtocolExtension.Serialize(new __RPC_Scene_SBChangeScene_RET_uint64__
                {
                    ReturnValue = 0
                });
                client.SendMessage(desc);
                return;
            }

            Action changeScene = () =>
            {
                DebugCounter[0]++;
                Logger.Info("Enter Game {0} - ChangeScene - 1 - {1}", characterId, TimeManager.Timer.ElapsedMilliseconds);
                using (var ms = new MemoryStream(desc.Data, false))
                {
                    var msg =
                        Serializer
                            .Deserialize
                            <
                                __RPC_Scene_SBChangeScene_ARG_uint64_characterId_int32_serverId_int32_sceneId_uint64_guid_int32_changeType_SceneParam_sceneParam__
                                >(ms);
                    var param = msg.SceneParam;
                    var changeType = msg.ChangeType;
                    var sceneInfo = mSceneManager.SelectOldScene(msg.Guid, msg.ServerId, msg.SceneId, msg.CharacterId);

                    if (sceneInfo == null && msg.SceneId == -1)
                    {
                        PlayerLog.WriteLog(888, "ChangeScene characterId={0},ServerId={1},SceneId={2},SceneGuid={3}",
                            msg.CharacterId, msg.ServerId, msg.SceneId, msg.Guid);
                        desc.Data = ProtocolExtension.Serialize(new __RPC_Scene_SBChangeScene_RET_uint64__
                        {
                            ReturnValue = 0
                        });
                        client.SendMessage(desc);
                        return;
                    }

                    if (sceneInfo == null)
                    {
                        Logger.Info("Enter Game {0} - ChangeScene - 2 - {1}", characterId,
                            TimeManager.Timer.ElapsedMilliseconds);
                        sceneInfo = mSceneManager.CreateNewSceneInfo(msg.ServerId, msg.SceneId, msg.Guid);
                        sceneInfo.PushCharacter(msg.CharacterId);
                        var oldSceneInfo = info.SceneInfo;
                        oldSceneInfo.CharacterIds.Remove(msg.CharacterId);
                        info.SceneInfo = sceneInfo;
                        Logger.Info("change scene 1: {0}, {1}, {2}", characterId,
                            oldSceneInfo.Server.RemoteEndPoint, sceneInfo.Server.RemoteEndPoint);

                        PlayerLog.WriteLog(888, "ChangeScene characterId={0},ServerId={1},SceneId={2},SceneGuid={3}",
                            msg.CharacterId, msg.ServerId, msg.SceneId, msg.Guid);
                        DebugCounter[1]++;
                        param.ObjId = desc.CharacterId;
                        sceneInfo.WaitingActions.Add(() =>
                        {
                            DebugCounter[2]++;
                            Logger.Info("Enter Game {0} - ChangeScene - 3 - {1}", characterId,
                                TimeManager.Timer.ElapsedMilliseconds);
                            ChangeSceneOver(info, oldSceneInfo, sceneInfo, b =>
                            {
                                PlayerLog.WriteLog(888, "ChangeSceneOver characterId={0},oldScene={1},newScene={2}",
                                    desc.CharacterId, oldSceneInfo.SceneGuid, sceneInfo.SceneGuid);
                                DebugCounter[3]++;
                                desc.Data =
                                    ProtocolExtension.Serialize(new __RPC_Scene_SBChangeScene_RET_uint64__
                                    {
                                        ReturnValue = sceneInfo.SceneGuid
                                    });
                                client.SendMessage(desc);
                                mSceneManager.NotifyEnterScene(info, changeType, param);

                                info.State = CharacterInfoState.Connected;
                                if (info.WaitingChangeSceneAction != null)
                                {
                                    var call = info.WaitingChangeSceneAction;
                                    info.WaitingChangeSceneAction = null;
                                    call();
                                }
                            });
                        });
                        mSceneManager.CreateNewScene(sceneInfo, param);
                    }
                    else
                    {
                        var oldSceneInfo = info.SceneInfo;
                        if (oldSceneInfo != sceneInfo)
                        {
                            oldSceneInfo.CharacterIds.Remove(msg.CharacterId);
                            info.SceneInfo = sceneInfo;
                        }
                        Logger.Info("change scene 2: {0}, {1}, {2}", characterId,
                            oldSceneInfo.Server.RemoteEndPoint, sceneInfo.Server.RemoteEndPoint);

                        DebugCounter[4]++;
                        var act = new Action(() =>
                        {
                            Logger.Info("Enter Game {0} - ChangeScene - 4 - {1}", characterId,
                                TimeManager.Timer.ElapsedMilliseconds);
                            ChangeSceneOver(info, oldSceneInfo, sceneInfo, b =>
                            {
                                DebugCounter[5]++;
                                desc.Data =
                                    ProtocolExtension.Serialize(new __RPC_Scene_SBChangeScene_RET_uint64__
                                    {
                                        ReturnValue = sceneInfo.SceneGuid
                                    });
                                client.SendMessage(desc);
                                mSceneManager.NotifyEnterScene(info, changeType, param);

                                info.State = CharacterInfoState.Connected;
                                if (info.WaitingChangeSceneAction != null)
                                {
                                    var call = info.WaitingChangeSceneAction;
                                    info.WaitingChangeSceneAction = null;
                                    call();
                                }
                            });
                        });

                        if (sceneInfo.Status == SceneStatus.ReadyToEnter)
                        {
                            DebugCounter[6]++;
                            act();
                        }
                        else
                        {
                            DebugCounter[7]++;
                            sceneInfo.WaitingActions.Add(act);
                        }
                    }
                }
            };

            if (info.State == CharacterInfoState.Transfer)
            {
                info.WaitingChangeSceneAction = changeScene;
            }
            else
            {
                info.State = CharacterInfoState.Transfer;
                changeScene();
            }
        }

        //desc.FuncId == 3039
        public void CheckCharacterOnline(ServerClient client, ServiceDesc desc)
        {
            var request =
                ProtocolExtension.Deserialize<__RPC_Scene_SBCheckCharacterOnline_ARG_Uint64Array_toList__>(desc.Data);
            var reply = new __RPC_Scene_SBCheckCharacterOnline_RET_Int32Array__();
            reply.ReturnValue = new Int32Array();
            foreach (var id in request.ToList.Items)
            {
                reply.ReturnValue.Items.Add(mCharacterInfoManager.ContainsKey(id) ? 1 : 0);
            }
            desc.Data = ProtocolExtension.Serialize(reply);
            client.SendMessage(desc);
        }

        //desc.FuncId == 3061
        private void OnNotifyConnected(ServiceDesc desc)
        {
            OnSocketListenerMessageReceiveBS(desc);
        }

        //desc.FuncId == 3062
        private void OnNotifyLost(ServiceDesc desc)
        {
            OnSocketListenerMessageReceiveBS(desc);
        }

        //desc.FuncId == 3069  一堆人同时进一个场景
        private void ChangeSceneByTeam(ServerClient client, ServiceDesc desc)
        {
            var msg =
                ProtocolExtension.Deserialize<__RPC_Scene_SBChangeSceneByTeam_ARG_ChangeSceneInfo_changeSceneData__>(
                    desc.Data);

            var sceneInfo = mSceneManager.SelectOldScene(msg.ChangeSceneData.SceneGuid, msg.ChangeSceneData.ServerId,
                msg.ChangeSceneData.SceneId, 0, msg.ChangeSceneData.CheckFull);

            PlayerLog.WriteLog(888, "ChangeSceneByTeam characterId={0},ServerId={1},SceneId={2},SceneGuid={3}",
                msg.ChangeSceneData.Guids.GetDataString(), msg.ChangeSceneData.ServerId, msg.ChangeSceneData.SceneId,
                msg.ChangeSceneData.SceneGuid);
            // 如果场景不存在，不用造新的
            if (sceneInfo == null && msg.ChangeSceneData.SceneId == -1)
            {
                PlayerLog.WriteLog(888, "ChangeSceneByTeam not find and not new,guids={0}",
                    msg.ChangeSceneData.Guids.GetDataString());
                desc.Data = ProtocolExtension.Serialize(new __RPC_Scene_SBChangeSceneByTeam_RET_uint64__
                {
                    ReturnValue = 0
                });
                client.SendMessage(desc);
                return;
            }

            var param = msg.ChangeSceneData.Pos;
            if (param == null)
            {
                param = new SceneParam();
            }

            param.ObjId = desc.CharacterId;

            if (sceneInfo == null)
            {
                sceneInfo = mSceneManager.CreateNewSceneInfo(msg.ChangeSceneData.ServerId, msg.ChangeSceneData.SceneId,
                    msg.ChangeSceneData.SceneGuid);
                mSceneManager.CreateNewScene(sceneInfo, param);
                //sceneInfo.PushCharacter(desc.CharacterId);
            }

            desc.Data =
                ProtocolExtension.Serialize(new __RPC_Scene_SBChangeSceneByTeam_RET_uint64__
                {
                    ReturnValue = sceneInfo.SceneGuid
                });
            client.SendMessage(desc);

            var changeSceneInfos = new List<ChangeSceneInfo>();
            foreach (var characterId in msg.ChangeSceneData.Guids)
            {
                sceneInfo.CharacterIds.Add(characterId);
                var info = GetCharacter(characterId);
                if (info == null)
                {
                    continue;
                }
                var oldSceneInfo = info.SceneInfo;
                if (oldSceneInfo != sceneInfo)
                {
                    oldSceneInfo.CharacterIds.Remove(characterId);
                    info.SceneInfo = sceneInfo;
                }

                changeSceneInfos.Add(new ChangeSceneInfo
                {
                    Info = info,
                    OldSceneInfo = oldSceneInfo,
                    NewSceneInfo = sceneInfo
                });
            }

            var type = msg.ChangeSceneData.Type;

            var act = new Action(() =>
            {
                PlayerLog.WriteLog(888, "ChangeSceneByTeam not find! new scene over guids={0}",
                    msg.ChangeSceneData.Guids.GetDataString());
                foreach (var changeSceneInfo in changeSceneInfos)
                {
                    Logger.Info("Enter Game {0} {1} {2} - ChangeSceneByTeam - 2 - {3}", changeSceneInfo.Info.CharacterId,
                        changeSceneInfo.OldSceneInfo.SceneGuid, changeSceneInfo.NewSceneInfo.SceneGuid,
                        TimeManager.Timer.ElapsedMilliseconds);
                    ChangeSceneOver(changeSceneInfo.Info, changeSceneInfo.OldSceneInfo, changeSceneInfo.NewSceneInfo,
                        b => { mSceneManager.NotifyEnterScene(changeSceneInfo.Info, type, param); });
                }
            });

            if (sceneInfo.Status == SceneStatus.ReadyToEnter)
            {
                act();
            }
            else
            {
                sceneInfo.WaitingActions.Add(act);
            }
        }

        //desc.FuncId == 3500
        public override void OnSocketListenerMessageReceiveCleanEx(ServiceDesc desc)
        {
            var character = GetCharacter(desc.CharacterId);
            if (character == null)
            {
                Logger.Warn("Can not find server for character: {0}", desc.CharacterId);
                return;
            }
            //场景
            if (character.SceneInfo != null)
            {
                character.SceneInfo.RemoveCharacter(character.CharacterId);
                character.SceneInfo = null;
            }
            //释放角色
            base.OnSocketListenerMessageReceiveCleanEx(desc);
        }

        #endregion
    }
}