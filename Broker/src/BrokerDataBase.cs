#region using

using System;
using System.Collections.Generic;
using DataTable;
using Scorpion;

#endregion

namespace Broker
{
    //玩家状态
    public enum CharacterState
    {
        SimpleData,
        PreparedData,
        Connected,
        Transfer
    }

    //场景状态
    public enum SceneStatus
    {
        WaitingToCreate = 0, //等待创建
        ReadyToEnter = 1, //创建完毕
        Finished = 2, //场景已满
        Crashed = 3 //正在删除
    }

    //Gate链接
    public class GateProxy
    {
        public ServerClient Gate;
    }

    //场景数据
    public class SceneInfo
    {
        public SceneInfo(int serverId, int sceneId, ulong sceneGuid, SocketClient server, int maxCharacterCount)
        {
            ServerId = serverId;
            SceneId = sceneId;
            SceneGuid = sceneGuid;
            Server = server;
            MaxCharacterCount = maxCharacterCount;
            SceneRecord = Table.GetScene(sceneId);
        }

        public HashSet<ulong> CharacterIds = new HashSet<ulong>();
        public SceneRecord SceneRecord;
        public SceneStatus Status = SceneStatus.WaitingToCreate;
        public List<Action> WaitingActions = new List<Action>();
        public int MaxCharacterCount { get; private set; }
        public ulong SceneGuid { get; private set; }
        public int SceneId { get; private set; }
        public SocketClient Server { get; private set; }
        public int ServerId { get; private set; }

        public void PushCharacter(ulong characterId)
        {
            CharacterIds.Add(characterId);
        }

        public void RemoveCharacter(ulong characterId)
        {
            CharacterIds.Remove(characterId);
        }
    }

    //玩家状态
    public enum CharacterInfoState
    {
        PreparedData = 0, //第一次预加载
        Connected, //链接进来
        Transfer //传送
    }

    //玩家数据
    public class CharacterSceneInfo : CommonBroker.CharacterInfo
    {
        public CharacterSceneInfo()
        {
        }

        public CharacterSceneInfo(ulong characterId)
        {
            CharacterId = characterId;
            State = CharacterInfoState.PreparedData;
        }

        private SceneInfo mSceneInfo;
        public Action WaitingChangeSceneAction;

        public SceneInfo SceneInfo
        {
            get { return mSceneInfo; }
            set
            {
                mSceneInfo = value;
                if (value != null)
                {
                    Server = value.Server;
                }
            }
        }

        public CharacterInfoState State { get; set; }
    }

    public class UserData
    {
        public int Id;
    }


    public class SceneServerUserData : UserData
    {
        public HashSet<SceneInfo> Scenes = new HashSet<SceneInfo>();
    }

    public class CallbackItem
    {
        public Action<bool, ServiceDesc> Callback;
        public object TimeHandle;
    }

    public static class ExtendFunction
    {
        public static ulong GetRating(this ServiceDesc desc)
        {
            if (desc.Routing.Count > 0)
            {
                return desc.Routing[desc.Routing.Count - 1];
            }
            return ulong.MaxValue;
        }
    }
}