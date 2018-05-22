#region using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DataContract;
using DataTable;
using Scorpion;
using NLog;

using Shared;

#endregion

namespace Broker
{
    public static class SceneServerManager
    {
        public static Logger Logger = LogManager.GetLogger("Broker.SceneBroker");

        public static ConcurrentDictionary<SocketClient, int> SceneServerPressure =
            new ConcurrentDictionary<SocketClient, int>();

        public static void CreateScene(SocketClient client, int value)
        {
            SceneServerPressure.AddOrUpdate(client, key => value, (key, oldvalue) => oldvalue + value);
        }

        public static SocketClient GetFreeSceneServer()
        {
            SocketClient nowServer = null;
            var nowValue = 0;
            foreach (var i in SceneServerPressure)
            {
                if (nowServer == null)
                {
                    nowServer = i.Key;
                    nowValue = i.Value;
                }
                else if (nowValue > i.Value)
                {
                    nowServer = i.Key;
                    nowValue = i.Value;
                }
            }
            return nowServer;
        }

        public static void PushServer(SocketClient client, int initValue)
        {
            SceneServerPressure.AddOrUpdate(client, key => initValue, (key, oldvalue) =>
            {
                Logger.Warn("SceneServerManager PushServer key is have");
                return oldvalue + initValue;
            });
        }

        public static void RemoveScene(SocketClient client, int value)
        {
            SceneServerPressure.AddOrUpdate(client, key => 0, (key, oldvalue) => oldvalue - value);
        }
    }

    public class SceneManagerBroker
    {
        #region 数据结构

        private static readonly Logger mLogger;
        private readonly SceneBroker mBroker;
        //SceneGuid -> sceneInfo
        public ConcurrentDictionary<ulong, SceneInfo> mFromSceneGuid2Server =
            new ConcurrentDictionary<ulong, SceneInfo>();

        //key = severId + SceneId ,value = List<SceneInfo>
        //public ConcurrentDictionary<ulong, List<SceneInfo>> mFromServerIdAndSceneId2Guid = new ConcurrentDictionary<ulong, List<SceneInfo>>();

        public ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, SceneInfo>> mFromServerIdAndSceneId2Guid =
            new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, SceneInfo>>();

        //key=ServerId value = 这个ServerId下的所有场景
        //public Dictionary<int, List<SceneInfo>> mFromServerId2SceneInfo = new Dictionary<int, List<SceneInfo>>();
        public SceneManagerBroker(SceneBroker broker, string ServiceName)
        {
            //Logger = LogManager.GetLogger("Broker." + ServiceName + "Broker");
            mBroker = broker;
        }

        static SceneManagerBroker()
        {
            mLogger = LogManager.GetLogger("Broker.SceneBroker");
        }

        #endregion

        #region 获取数据接口

        //获得 服务器 + 场景 的自定义ID  之后简称SSid
        //private static ulong mServerSceneParam = 1000000ul;
        public static ulong CalcServerSceneId(int serverId, int sceneId)
        {
            var server = ((ulong) serverId << 32);
            return server + (ulong) sceneId;
        }

        //根据SSid  获取  服务器ID
        public static int GetServerId(ulong serverSceneId)
        {
            return (int) (serverSceneId >> 32);
        }

        //根据SSid  获取  场景ID
        public static int GetSceneId(ulong serverSceneId)
        {
            return (int) (serverSceneId & 0xFFFFFFFF);
        }

        //SelectOldScene
        public SceneInfo SelectOldScene(ulong sceneGuid,
                                        int serverId,
                                        int sceneId,
                                        ulong characterId,
                                        bool checkFull = true)
        {
            var scene = SelectServerForScene(sceneGuid, characterId, checkFull);
            if (scene == null)
            {
                scene = SelectServerForScene(serverId, sceneId, characterId);
            }
            return scene;
        }

        public SceneInfo SelectServerForScene(ulong sceneGuid, ulong characterId, bool checkFull = true)
        {
            mLogger.Info("SelectServerForScene {0}", sceneGuid);

            SceneInfo info;
            if (mFromSceneGuid2Server.TryGetValue(sceneGuid, out info))
            {
                if (info.Status == SceneStatus.Finished || info.Status == SceneStatus.Crashed)
                {
                    return null;
                }

                if (checkFull)
                {
                    if (info.CharacterIds.Count >= info.MaxCharacterCount)
                    {
                        if (info.CharacterIds.Contains(characterId))
                        {
                            return info;
                        }

                        return null;
                    }
                }
                if (characterId != 0)
                {
                    info.CharacterIds.Add(characterId);
                }
                return info;
            }
            return null;
        }

        //根据Guid获取服务器

        public SocketClient GetSceneSocket(ulong guid)
        {
            return SceneServerManager.GetFreeSceneServer();
        }


        /// <summary>
        ///     为sceneId的场景选择服务器
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="sceneId"></param>
        /// <param name="characterId"></param>
        /// <returns></returns>
        public SceneInfo SelectServerForScene(int serverId, int sceneId, ulong characterId)
        {
            mLogger.Info("SelectServerForScene {0}, {1}", serverId, sceneId);

            if (sceneId == -1)
            {
                mLogger.Info("serverId:{0}, sceneId:{1} inside SelectSceneForCharacter.", serverId, sceneId);
                return null;
            }

            var scene = Table.GetScene(sceneId);
            if (scene == null)
            {
                return null;
            }
            if (IsNormalScene(scene) || scene.CreateSceneRule == 1)
            {
                return SelectNormalSceneForCharacter(serverId, sceneId, characterId);
            }

            return null;
        }

        //根据服务器ID 和  场景ID  ---->  查询出相应的场景
        //如果带有正确的 characterId,有能力找到character所在的场景
        private SceneInfo SelectNormalSceneForCharacter(int serverId, int sceneId, ulong characterId = 0)
        {
            var key = CalcServerSceneId(serverId, sceneId);
            ConcurrentDictionary<ulong, SceneInfo> infos;
            if (mFromServerIdAndSceneId2Guid.TryGetValue(key, out infos))
            {
                foreach (var scene in infos)
                {
                    var sceneInfo = scene.Value;
                    if (sceneInfo.Status == SceneStatus.Finished || sceneInfo.Status == SceneStatus.Crashed)
                    {
                        continue;
                    }

                    if (sceneInfo.CharacterIds.Count >= sceneInfo.MaxCharacterCount)
                    {
                        if (sceneInfo.CharacterIds.Contains(characterId))
                        {
                            return sceneInfo;
                        }
                    }
                    else
                    {
                        if (characterId != 0)
                        {
                            sceneInfo.CharacterIds.Add(characterId);
                        }
                        return sceneInfo;
                    }
                }
            }

            return null;
        }

        //判断是否一般场景
        public static bool IsNormalScene(SceneRecord scene)
        {
            return scene.Type == 0 || scene.Type == 1;
        }

        #endregion

        #region 功能函数(增删改合）

        public SceneInfo CreateNewSceneInfo(int serverId, int sceneId, ulong guid)
        {
            mBroker.DebugCounter[16]++;

            if (guid == 0)
            {
                guid = mBroker.GetUniqueId();
            }
            else
            {
                if (sceneId != 1000)
                {
                    guid = mBroker.GetUniqueId();
                }
            }

            var server = GetSceneSocket(guid);
            var info = new SceneInfo(serverId, sceneId, guid, server, Table.GetScene(sceneId).PlayersMaxA);
            ((SceneServerUserData) server.UserData).Scenes.Add(info);

            mLogger.Info("CreateNewSceneInfo serverid:{0}, sceneid:{1}, scene guid:{2}, scene serverid:{3}", serverId,
                sceneId, info.SceneGuid, info.Server.RemoteEndPoint);

            return info;
        }

        //创造新场景
        public void CreateNewScene(SceneInfo sceneInfo, SceneParam param = null)
        {
            var content =
                new __RPC_Scene_BSCreateScene_ARG_int32_serverId_int32_sceneId_uint64_guid_SceneParam_sceneParam__();
            content.ServerId = sceneInfo.ServerId;
            content.SceneId = sceneInfo.SceneId;
            content.Guid = sceneInfo.SceneGuid;
            content.SceneParam = param ?? new SceneParam();

            var message = new ServiceDesc();
            message.FuncId = 3010;
            message.ServiceType = (int) ServiceType.Scene;
            message.PacketId = mBroker.GetUniquePacketId();
            message.Data = ProtocolExtension.Serialize(content);
            message.Type = (int) MessageType.BS;

            mLogger.Info("Notify Scene server CreateNewScene {0}, {1}, {2}", sceneInfo.ServerId, sceneInfo.SceneId,
                sceneInfo.SceneGuid);

            ConcurrentDictionary<ulong, SceneInfo> infos;
            var serverSceneId = CalcServerSceneId(sceneInfo.ServerId, sceneInfo.SceneId);
            if (mFromServerIdAndSceneId2Guid.TryGetValue(serverSceneId, out infos))
            {
                infos.TryAdd(sceneInfo.SceneGuid, sceneInfo);
            }
            else
            {
                var temp = new ConcurrentDictionary<ulong, SceneInfo>();
                temp.TryAdd(sceneInfo.SceneGuid, sceneInfo);
                mFromServerIdAndSceneId2Guid.TryAdd(serverSceneId, temp);
            }

            mBroker.DebugCounter[17]++;
            mFromSceneGuid2Server.TryAdd(sceneInfo.SceneGuid, sceneInfo);

            SceneServerManager.CreateScene(sceneInfo.Server, 1);
            var act = new Action<bool, ServiceDesc>((b, item) =>
            {
                if (b)
                {
                    if (item.Error == 0)
                    {
                        mLogger.Info("Scene server CreateNewScene replied {0}, {1}, {2}", sceneInfo.ServerId,
                            sceneInfo.SceneId, sceneInfo.SceneGuid);

                        sceneInfo.Status = SceneStatus.ReadyToEnter;

                        foreach (var action in sceneInfo.WaitingActions)
                        {
                            try
                            {
                                action();
                            }
                            catch (Exception ex)
                            {
                                mLogger.Error(ex, "Create new scene callback error.");
                            }
                        }

                        sceneInfo.WaitingActions.Clear();
                    }
                    else
                    {
                        mLogger.Error("CreateNewScene failed {0}....", item.Error);
                    }
                }
                else
                {
                    mLogger.Error("CreateNewScene timeout....");
                }
            });

            mBroker.RegisterCallback(message.PacketId, act);

            sceneInfo.Server.SendMessage(message);
        }

        public void RemoveScene(ulong sceneGuid)
        {
            SceneInfo sceneInfo;
            if (mFromSceneGuid2Server.TryRemove(sceneGuid, out sceneInfo))
            {
                if (sceneInfo == null)
                {
                    mLogger.Error("SceneBroker RemoveScene Guid = {0}", sceneGuid);
                    return;
                }
                SceneServerManager.RemoveScene(sceneInfo.Server, 1);
                ConcurrentDictionary<ulong, SceneInfo> scenes;
                if (mFromServerIdAndSceneId2Guid.TryGetValue(CalcServerSceneId(sceneInfo.ServerId, sceneInfo.SceneId),
                    out scenes))
                {
                    scenes.TryRemove(sceneGuid, out sceneInfo);
                    //scenes.RemoveAll(item => item.SceneGuid == sceneGuid);
                }
                if (sceneInfo.CharacterIds.Count > 0)
                {
                    mLogger.Error("SceneBroker RemoveScene CharacterIds = {0}", sceneInfo.CharacterIds.GetDataString());
                }
                sceneInfo.CharacterIds.Clear();
            }
        }

        /// <summary>
        ///     找到两个场景人数都低于最大人数30%的场景，把一个场景的人合并到另一个场景中
        /// </summary>
        private readonly Dictionary<int, SceneInfo> firstScenes = new Dictionary<int, SceneInfo>();

        public void MergeSceneImpl()
        {
            foreach (var pair in mFromServerIdAndSceneId2Guid)
            {
                firstScenes.Clear();
                var sceneId = GetSceneId(pair.Key);
                var tbScene = Table.GetScene(sceneId);
                if (tbScene == null)
                {
                    mLogger.Error("MergeSceneImpl ={0},sceneId={1}", pair.Key, sceneId);
                    continue;
                }
                if (!IsNormalScene(Table.GetScene(sceneId)))
                {
                    continue;
                }
                SceneInfo firstInfo = null;
                SceneInfo secondInfo = null;
                foreach (var scene in pair.Value)
                {
                    var sceneInfo = scene.Value;
                    if (sceneInfo.Status == SceneStatus.WaitingToCreate)
                    {
                        continue;
                    }
                    if (sceneInfo.CharacterIds.Count >= sceneInfo.MaxCharacterCount*0.3f)
                    {
                        continue;
                    }
                    SceneInfo findInfo = null;
                    if (firstScenes.TryGetValue(sceneInfo.SceneId, out findInfo))
                    {
                        firstInfo = findInfo;
                        secondInfo = sceneInfo;
                        break;
                    }
                    firstScenes[sceneInfo.SceneId] = sceneInfo;
                    //if (firstInfo == null)
                    //{
                    //    firstInfo = sceneInfo;
                    //}
                    //else
                    //{
                    //    secondInfo = sceneInfo;
                    //    break;
                    //}
                }
                if (firstInfo != null)
                {
                    var ids = secondInfo.CharacterIds.ToArray();
                    foreach (var characterId in ids)
                    {
                        mBroker.ChangeScene(characterId, firstInfo);
                    }
                }
            }


            // 按服务器合并
            //foreach (var sceneInfos in mFromServerId2SceneInfo)
            //{
            //    SceneInfo firstInfo = null;
            //    SceneInfo secondInfo = null;

            //    foreach (var sceneInfo in sceneInfos.Value)
            //    {
            //        // 只合并非副本场景
            //        if (!SceneManagerBroker.IsNormalScene(sceneInfo.SceneRecord))
            //        {
            //            continue;
            //        }

            //        if (sceneInfo.CharacterIds.Count <= sceneInfo.MaxCharacterCount * 0.3f)
            //        {
            //            if (firstInfo == null && sceneInfo.CharacterIds.Count > 0)
            //            {
            //                firstInfo = sceneInfo;
            //                continue;
            //            }

            //            // 按场景合并
            //            if (sceneInfo.CharacterIds.Count > 0 && firstInfo != null && sceneInfo.SceneId == firstInfo.SceneId)
            //            {
            //                secondInfo = sceneInfo;
            //                break;
            //            }
            //        }
            //    }

            //    if (firstInfo == null || secondInfo == null || firstInfo.Status == SceneStatus.WaitingToCreate)
            //    {
            //        return;
            //    }

            //    // 把场景2的人移动到场景1
            //    var ids = secondInfo.CharacterIds.ToArray();
            //    foreach (var characterId in ids)
            //    {
            //        mBroker.ChangeScene(characterId, firstInfo);
            //    }
            //}
        }

        //通知进入场景
        public void NotifyEnterScene(CharacterSceneInfo info, int changeType, SceneParam sp)
        {
            PlayerLog.WriteLog(888, "NotifyEnterScene characterId={0},SceneId={1},newScene={2}", info.CharacterId,
                info.SceneInfo.SceneId, info.SceneInfo.SceneGuid);
            mLogger.Info("Enter Game {0} - NotifyEnterScene - 1 - {1}", info.CharacterId,
                TimeManager.Timer.ElapsedMilliseconds);
            mLogger.Info("NotifyEnterScene {0}, {1}.", info.CharacterId, info.SceneInfo.Server.RemoteEndPoint);

            var content =
                new __RPC_Scene_SSEnterScene_ARG_uint64_characterId_uint64_guid_uint64_applyGuid_int32_changeType_SceneParam_sceneParam__
                    ();
            content.CharacterId = info.CharacterId;
            content.Guid = info.SceneInfo.SceneGuid;
            content.ChangeType = changeType;
            content.SceneParam = sp;
            var message = new ServiceDesc();
            message.FuncId = 3055;
            message.ServiceType = (int) ServiceType.Scene;
            message.PacketId = mBroker.GetUniquePacketId();
            message.Data = ProtocolExtension.Serialize(content);
            message.Type = (int) MessageType.SS;
            message.CharacterId = info.CharacterId;


            info.SceneInfo.Server.SendMessage(message);
        }

        #endregion

        #region 网络包处理

        //desc.FuncId == 3001   销毁场景
        public void DestroyScene(ServerClient client, ServiceDesc desc)
        {
            var msg = ProtocolExtension.Deserialize<__RPC_Scene_SBDestroyScene_ARG_uint64_guid__>(desc.Data);
            var ret = new __RPC_Scene_SBDestroyScene_RET_uint64__();
            SceneInfo info;
            if (mFromSceneGuid2Server.TryGetValue(msg.Guid, out info))
            {
                if (info.CharacterIds.Count == 0)
                {
                    SceneServerManager.RemoveScene(info.Server, 1);
                    ConcurrentDictionary<ulong, SceneInfo> scenes;
                    if (mFromServerIdAndSceneId2Guid.TryGetValue(CalcServerSceneId(info.ServerId, info.SceneId),
                        out scenes))
                    {
                        SceneInfo info2;
                        scenes.TryRemove(msg.Guid, out info2);
                        //scenes.RemoveAll(item => item.SceneGuid == msg.Guid);
                        mFromSceneGuid2Server.TryRemove(msg.Guid, out info);
                        ret.ReturnValue = 1;
                    }
                    else
                    {
                        // 这种情况不应该发生
                        mLogger.Error(
                            "mFromSceneGuid2Server and mFromServerIdAndSceneId2Guid not consistent, {0},{1},{2}",
                            info.ServerId, info.SceneId, info.SceneGuid);
                        mFromSceneGuid2Server.TryRemove(msg.Guid, out info);
                        ret.ReturnValue = 1;
                    }
                }
                else
                {
                    ret.ReturnValue = 0;
                }
            }
            else
            {
                ret.ReturnValue = 0;
            }

            desc.Data = ProtocolExtension.Serialize(ret);
            client.SendMessage(desc);
        }


        //desc.FuncId == 3002   此场景已经不存在
        public void NoHaveScene(ServerClient client, ServiceDesc desc)
        {
            var msg =
                ProtocolExtension.Deserialize<__RPC_Scene_NotifySceneNotExist_ARG_uint64_sceneId_uint64_characterId__>(
                    desc.Data);
            var characterId = msg.CharacterId;
            mLogger.Info("Enter Game {0} - NotifySceneNotExist - 1 - {1}", characterId,
                TimeManager.Timer.ElapsedMilliseconds);

            var info = mBroker.GetCharacter(characterId);
            if (info == null)
            {
                return;
            }
            var serverId = info.SceneInfo.ServerId;
            var sceneId = info.SceneInfo.SceneId;

            mLogger.Info("Enter Game {0} - NotifySceneNotExist - 2 - {1}", characterId,
                TimeManager.Timer.ElapsedMilliseconds);
            RemoveScene(msg.SceneId);
            mLogger.Info("Enter Game {0} - NotifySceneNotExist - 3 - {1}", characterId,
                TimeManager.Timer.ElapsedMilliseconds);

            var newScene = SelectServerForScene(serverId, sceneId, msg.CharacterId);
            if (newScene == null)
            {
                mLogger.Info("Enter Game {0} - NotifySceneNotExist - 4 - {1}", characterId,
                    TimeManager.Timer.ElapsedMilliseconds);
                newScene = CreateNewSceneInfo(serverId, sceneId, 0);
                newScene.PushCharacter(msg.CharacterId);
                //DebugCounter[1]++;
                var param = new SceneParam();
                param.ObjId = desc.CharacterId;

                CreateNewScene(newScene, param);
            }

            if (newScene.Status == SceneStatus.ReadyToEnter)
            {
                mBroker.ChangeScene(msg.CharacterId, newScene);
                mLogger.Info("Enter Game {0} - NotifySceneNotExist - 5 - {1}", characterId,
                    TimeManager.Timer.ElapsedMilliseconds);
            }
            else
            {
                newScene.WaitingActions.Add(() =>
                {
                    mLogger.Info("Enter Game {0} - NotifySceneNotExist - 6 - {1}", characterId,
                        TimeManager.Timer.ElapsedMilliseconds);
                    mBroker.ChangeScene(msg.CharacterId, newScene);
                });
            }
        }

        //funId = 3071 
        public void MergeSceneByTeam(ServerClient client, ServiceDesc desc)
        {
            var msg = ProtocolExtension.Deserialize<__RPC_Scene_MergeSceneByTeam_ARG_IdList_ids__>(desc.Data);

            // 先按场景Id分组
            //var dict = msg.Ids.Ids.Select(i => mBroker.mFromCharacterId2Server[i]).ToLookup(k => k.SceneInfo.SceneId, v => v);

            var dict = new Dictionary<int, List<CharacterSceneInfo>>();
            foreach (var id in msg.Ids.Ids)
            {
                var c = mBroker.GetCharacter(id);
                if (c == null)
                {
                    continue;
                }
                List<CharacterSceneInfo> list;
                if (!dict.TryGetValue(c.SceneInfo.SceneId, out list))
                {
                    list = new List<CharacterSceneInfo>();
                    dict[c.SceneInfo.SceneId] = list;
                }
                list.Add(c);
            }


            foreach (var k in dict)
            {
                // 只合并非副本场景
                if (!IsNormalScene(Table.GetScene(k.Key)))
                {
                    continue;
                }

                var v = dict[k.Key];

                // 找到人最少的Server
                SceneInfo minCharacterSceneInfo = null;
                foreach (var info in v)
                {
                    if (minCharacterSceneInfo == null)
                    {
                        minCharacterSceneInfo = info.SceneInfo;
                        continue;
                    }

                    if (info.SceneInfo.CharacterIds.Count < minCharacterSceneInfo.CharacterIds.Count)
                    {
                        minCharacterSceneInfo = info.SceneInfo;
                    }
                }

                if (minCharacterSceneInfo == null)
                {
                    continue;
                }

                // 把其他人都移过去
                foreach (var info in v)
                {
                    // 如果可以跨服或者服务器Id相同，才可以合并
                    if (info.SceneInfo != minCharacterSceneInfo && (info.SceneInfo.SceneRecord.CanCrossServer == 1 || info.SceneInfo.ServerId == minCharacterSceneInfo.ServerId))
                    {
                        mBroker.ChangeScene(info.CharacterId, minCharacterSceneInfo);
                    }
                }
            }
        }

        //func = 3499
        public void SeekSceneIndexByCharacterId(ServerClient client, ServiceDesc desc)
        {
            var msg =
                ProtocolExtension
                    .Deserialize
                    <__RPC_Scene_SBReconnectNotifyScene_ARG_uint64_oldclientId_uint64_newclientId_uint64_characterId__>(
                        desc.Data);

            var characterId = msg.CharacterId;
            var clientId = msg.NewclientId;
            var oldClientId = msg.OldclientId;
            CommonBroker.CharacterInfo characterCommon;

            if (mBroker.mCharacterInfoManager.TryGetValue(characterId, out characterCommon))
            {
                var character = characterCommon as CharacterSceneInfo;
                if (character == null)
                {
                    return;
                }
                var index = mBroker.GetServerIndex(character.Server);
                if (index == -1)
                {
                    return;
                }

                var gateDesc = new ServiceDesc();
                gateDesc.Type = (int) MessageType.ReConnetServerToGate;
                gateDesc.CharacterId = characterId;
                gateDesc.ServiceType = (int) ServiceType.Scene;
                gateDesc.ClientId = clientId;

                gateDesc.Routing.Add(oldClientId);
                gateDesc.Routing.Add((ulong) index);

                character.Gate.Gate.SendMessage(gateDesc);
            }
        }

        //func = 3072
        public void IsSceneExist(ServerClient client, ServiceDesc desc)
        {
            var msg = ProtocolExtension.Deserialize<__RPC_Scene_IsSceneExist_ARG_uint64_sceneGuid__>(desc.Data);
            var sceneGuid = msg.SceneGuid;
            var exist = mFromSceneGuid2Server.ContainsKey(sceneGuid);
            desc.Data = ProtocolExtension.Serialize(exist);
            client.SendMessage(desc);
        }

		//func = 3605
		public void RequestSceneInfo(ServerClient client, ServiceDesc desc)
		{
			var msg = ProtocolExtension.Deserialize<__RPC_Scene_RequestSceneInfo_ARG_int32_serverId_int32_sceneTypeId__>(desc.Data);
			var serverId = msg.ServerId;
			var sceneId = msg.SceneTypeId;
			var key = CalcServerSceneId(serverId, sceneId);

			var retMsg = new __RPC_Scene_RequestSceneInfo_RET_MsgScenesInfo__();
			retMsg.ReturnValue = new MsgScenesInfo();

			ConcurrentDictionary<ulong, SceneInfo> infos;
			if (mFromServerIdAndSceneId2Guid.TryGetValue(key, out infos))
			{
				if (null != infos && !infos.IsEmpty)
				{
					foreach (var info in infos)
					{
						var sceneInfo = new MsgSceneInfo();
						sceneInfo.Guid = info.Value.SceneGuid;
						sceneInfo.PlayerCount = info.Value.CharacterIds.Count;
						retMsg.ReturnValue.Info.Add(sceneInfo);
					}
				}
			}
			desc.Data = ProtocolExtension.Serialize(retMsg);
			client.SendMessage(desc);
		}
        #endregion
    }
}