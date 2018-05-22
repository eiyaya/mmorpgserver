#region using

using System;
using System.Collections;
using System.Collections.Generic;
using ChatServerService;
using Database;
using DataContract;
using DataTable;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Chat
{
    public class HornMessage
    {
        public ulong CharacterId;
        public string CharacterName;
        public int ChatType;
        public ChatMessageContent Content;
        public uint ServerId;
    }

    public interface IChatManager
    {
        void BroadcastServerIdMessage(uint ServerId,
                                      int chatType,
                                      ulong CharacterId,
                                      string characterName,
                                      ChatMessageContent content);

        ErrorCodes BroadcastAllServerMessage(int chatType, ulong characterId, string characterName,
            ChatMessageContent content);

        void BroadcastAnchorOnlineMessage(uint serverId, string name, int online);
        void BroadcastAllAnchorOnlineMessage(string name, int online);
        void CacheWorldMessage(uint serverId,
                               int chatType,
                               ulong characterId,
                               string characterName,
                               ChatMessageContent content);

        void CreateHornTrigger();
        void DeleteHornTrigger();
        string GetChatName(string Content, out string NewContent);
        void HornTick();
        void Init();
        void InitByBase(ChatManager _this, ChatCharacterController character, DBCharacterChat dbplayer);
        void InitByDB(ChatManager _this, ChatCharacterController character, DBCharacterChat dbplayer);
        bool IsRobot(string name);
        void Online(ChatManager _this);
        void OnLost(ulong characterId);

        void PushHornMessage(uint serverId,
                             int chatType,
                             ulong characterId,
                             string characterName,
                             ChatMessageContent content);

        IEnumerator SaveMessage(Coroutine coroutine,
                                ChatManager _this,
                                int chatType,
                                ulong fromCharacterId,
                                string characterName,
                                ChatMessageContent content,
                                ulong ToCharacterId);

        bool ToClinetMessage(ulong toCharacterId,
                             int chatType,
                             ulong fromCharacterId,
                             string characterName,
                             ChatMessageContent content);
    }

    public class ChatManagerDefaultImpl : IChatManager
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public string GetChatName(string Content, out string NewContent)
        {
            var nend1 = Content.IndexOf("/", 0, StringComparison.Ordinal);
            if (nend1 != 0)
            {
                NewContent = "";
                return "";
            }
            var nend2 = Content.IndexOf(" ", 0, StringComparison.Ordinal);
            if (nend2 == -1)
            {
                NewContent = "";
                return "";
            }
            if (nend2 < nend1)
            {
                NewContent = "";
                return "";
            }
            NewContent = Content.Substring(nend2 + 1, Content.Length - nend2 - 1);
            return Content.Substring(nend1 + 1, nend2 - nend1 - 1);
        }

        #region 初始化

        //静态数据初始化
        public void Init()
        {
            Table.ForeachJJCRoot(record =>
            {
                ChatManager.RobotNames.Add(record.Name, 1);
                return true;
            });

            //UpdateOpenedServers();
            CreateServerListTrigger();
        }

        public bool IsRobot(string name)
        {
            return ChatManager.RobotNames.ContainsKey(name);
        }

        //初始化（按初始配置）
        public void InitByBase(ChatManager _this, ChatCharacterController character, DBCharacterChat dbplayer)
        {
            _this.mCharacter = character;
            _this.mData = dbplayer.Chats;
            _this.MarkDirty();
        }

        //初始化（按数据库配置）
        public void InitByDB(ChatManager _this, ChatCharacterController character, DBCharacterChat dbplayer)
        {
            _this.mCharacter = character;
            _this.mData = dbplayer.Chats;
        }

        #endregion

        #region 广播相关

        //发消息给玩家
        public bool ToClinetMessage(ulong toCharacterId,
                                    int chatType,
                                    ulong fromCharacterId,
                                    string characterName,
                                    ChatMessageContent content)
        {
            ChatCharacterProxy toCharacterProxy;
            if (ChatServer.Instance.ServerControl.Proxys.TryGetValue(toCharacterId, out toCharacterProxy))
            {
                var chattoCharacterProxy = toCharacterProxy as ChatProxy;
                if (chattoCharacterProxy != null)
                {
                    chattoCharacterProxy.SyncChatMessage(chatType, fromCharacterId, characterName, content);
                }
                return true;
            }
            return false;
        }

        //存储私聊信息
        public IEnumerator SaveMessage(Coroutine coroutine,
                                       ChatManager _this,
                                       int chatType,
                                       ulong fromCharacterId,
                                       string characterName,
                                       ChatMessageContent content,
                                       ulong ToCharacterId)
        {
            var msg = new DBChatMsg
            {
                FromId = fromCharacterId,
                Type = chatType,
                Content = content.Content,
                ToId = ToCharacterId,
                Name = characterName,
                SoundData = content.SoundData,
                Vip = content.Vip
            };
            //缓存私聊
            CharacterManager.Instance.ModifyVolatileData(ToCharacterId, DataCategory.ChatCharacter, oldData =>
            {
                oldData.Chats.Add(msg);
                if (oldData.Chats.Count > 50)
                {
                    oldData.Chats.RemoveAt(0);
                }
                return oldData;
            });
            yield break;
        }

        //缓存世界频道内容
        public void CacheWorldMessage(uint serverId,
                                      int chatType,
                                      ulong characterId,
                                      string characterName,
                                      ChatMessageContent content)
        {
            var Horn = new HornMessage
            {
                ServerId = serverId,
                ChatType = chatType,
                CharacterId = characterId,
                CharacterName = characterName,
                Content = content
            };
            List<HornMessage> tempList;
            if (ChatManager.mCacheWorlds.TryGetValue(serverId, out tempList))
            {
                if (tempList.Count > 20)
                {
                    tempList.RemoveAt(0);
                }
                tempList.Add(Horn);
            }
            else
            {
                tempList = new List<HornMessage>();
                ChatManager.mCacheWorlds[serverId] = tempList;
                tempList.Add(Horn);
            }
        }

        //世界聊天广播
        public void BroadcastServerIdMessage(uint serverId,
                                             int chatType,
                                             ulong characterId,
                                             string characterName,
                                             ChatMessageContent content)
        {
            var id = (int)serverId;
            var tb = Table.GetServerName(id);
            if (tb != null)
            {
                id = tb.LogicID;
            }

            ChatServer.Instance.ServerControl.BroadcastWorldMessage((uint)id, chatType, characterId, characterName,
                content);
        }

        public ErrorCodes BroadcastAllServerMessage(int chatType, ulong characterId, string characterName,
            ChatMessageContent content)
        {
            foreach (var serverId in ChatManager.OpenedServerIdList)
            {
                ChatManager.BroadcastServerIdMessage((uint)serverId, chatType,
                    characterId, characterName, content);
            }

            return ErrorCodes.OK;
        }

        public void BroadcastAnchorOnlineMessage(uint serverId, string name, int online)
        {
            ChatServer.Instance.ServerControl.BroadcastAnchorOnline(serverId, name, online);            
        }

        public void BroadcastAllAnchorOnlineMessage(string name, int online)
        {
            foreach (var serverId in ChatManager.OpenedServerIdList)
            {
                BroadcastAnchorOnlineMessage((uint)serverId, name, online);
            }
        }

        #endregion

        #region 上下线 相关

        //上线
        public void Online(ChatManager _this)
        {
            //Todo
            if (_this.mData == null || _this.mData.Count == 0)
            {
            }
            else
            {
                foreach (var chatMsg in _this.mData)
                {
                    ToClinetMessage(chatMsg.ToId, chatMsg.Type, chatMsg.FromId, chatMsg.Name, new ChatMessageContent
                    {
                        Content = chatMsg.Content,
                        SoundData = chatMsg.SoundData,
                        Vip = chatMsg.Vip
                    });
                }
                _this.mData.Clear();
                _this.MarkDbDirty();
            }
        }

        //下线
        public void OnLost(ulong characterId)
        {
        }

        #endregion

        #region 喇叭心跳

        //喇叭心跳
        public void HornTick()
        {
            if (ChatManager.Horns.Count < 1)
            {
                DeleteHornTrigger();
            }
            else
            {
                var Horn = ChatManager.Horns[0];
                BroadcastServerIdMessage(Horn.ServerId, Horn.ChatType, Horn.CharacterId, Horn.CharacterName,
                    Horn.Content);
                ChatManager.Horns.RemoveAt(0);
            }
        }

        public void PushHornMessage(uint serverId,
                                    int chatType,
                                    ulong characterId,
                                    string characterName,
                                    ChatMessageContent content)
        {
            if (ChatManager.HornTrigger == null)
            {
                BroadcastServerIdMessage(serverId, chatType, characterId, characterName, content);
                CreateHornTrigger();
            }
            else
            {
                var Horn = new HornMessage
                {
                    ServerId = serverId,
                    ChatType = chatType,
                    CharacterId = characterId,
                    CharacterName = characterName,
                    Content = content
                };
                ChatManager.Horns.Add(Horn);
            }
        }

        public void CreateHornTrigger()
        {
            ChatManager.HornTrigger =
                ChatServerControl.tm.CreateTrigger(DateTime.Now.AddMilliseconds(ChatManager.HornTickTime), HornTick,
                    ChatManager.HornTickTime);
        }

        public void DeleteHornTrigger()
        {
            if (ChatManager.HornTrigger != null)
            {
                ChatServerControl.tm.DeleteTrigger(ChatManager.HornTrigger);
                ChatManager.HornTrigger = null;
            }
        }

        public void ServerListTick()
        {
            UpdateOpenedServers();
        }


        public void CreateServerListTrigger()
        {
            ChatManager.ServerListTrigger =
                ChatServerControl.tm.CreateTrigger(DateTime.Now, ServerListTick, ChatManager.ServerListTickTime);
        }

        public void DeleteServerListTrigger()
        {
            if (ChatManager.ServerListTrigger != null)
            {
                ChatServerControl.tm.DeleteTrigger(ChatManager.ServerListTrigger);
                ChatManager.ServerListTrigger = null;
            }
        }

        public void UpdateOpenedServers()
        {
            ChatManager.OpenedServerIdList.Clear();
            Table.ForeachServerName(record =>
            {
                if (record.LogicID == -1 || record.LogicID != record.Id)
                {
                    return true;
                }
                if (record.IsClientDisplay == 0)
                {
                    return true;
                }
                else if (record.IsClientDisplay == 2)
                { // 维护
                    return true;
                }

                var startTime = DateTime.Parse(record.OpenTime);
                if (DateTime.Now > startTime)
                { // 开启
                    ChatManager.OpenedServerIdList.Add(record.Id);
                    //Logger.Error("UpdateOpenedServers LogicID={0},ID={1}",record.LogicID,record.Id);
                }

                return true;
            });

        }

        #endregion
    }

    public class ChatManager : NodeBase
    {
        public static List<HornMessage> Horns = new List<HornMessage>();
        //喇叭的发送频率,喇叭广播时间间隔（毫秒）,15000
        public static int HornTickTime = Table.GetServerConfig(339).ToInt();
        public static Trigger HornTrigger; //喇叭定时器
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static List<int> OpenedServerIdList = new List<int>(4);
        public static Trigger ServerListTrigger;
        public static int ServerListTickTime = 60000;
        public static Dictionary<uint, List<HornMessage>> mCacheWorlds = new Dictionary<uint, List<HornMessage>>();
            //key[serverId]

        private static IChatManager mImpl;
        public static Dictionary<string, int> RobotNames = new Dictionary<string, int>();

        public static RequestManager WebRequestManager = null;

        static ChatManager()
        {
            ChatServer.Instance.UpdateManager.InitStaticImpl(typeof (ChatManager), typeof (ChatManagerDefaultImpl),
                o => { mImpl = (IChatManager) o; });
        }

        public ChatCharacterController mCharacter;
        public List<DBChatMsg> mData;

        public override IEnumerable<NodeBase> Children
        {
            get { return null; }
        }

        public static string GetChatName(string Content, out string NewContent)
        {
            return mImpl.GetChatName(Content, out NewContent);
        }

        #region 初始化

        //静态数据初始化
        public static void Init()
        {
            mImpl.Init();
        }

        public static bool IsRobot(string name)
        {
            return mImpl.IsRobot(name);
        }

        //初始化（按初始配置）
        public void InitByBase(ChatCharacterController character, DBCharacterChat dbplayer)
        {
            mImpl.InitByBase(this, character, dbplayer);
        }

        //初始化（按数据库配置）
        public void InitByDB(ChatCharacterController character, DBCharacterChat dbplayer)
        {
            mImpl.InitByDB(this, character, dbplayer);
        }

        #endregion

        #region 广播相关

        public static bool ToClinetMessage(ulong toCharacterId,
                                           int chatType,
                                           ulong fromCharacterId,
                                           string characterName,
                                           ChatMessageContent content)
        {
            return mImpl.ToClinetMessage(toCharacterId, chatType, fromCharacterId, characterName, content);
        }

        //存储私聊信息
        public IEnumerator SaveMessage(Coroutine coroutine,
                                       int chatType,
                                       ulong fromCharacterId,
                                       string characterName,
                                       ChatMessageContent content,
                                       ulong ToCharacterId)
        {
            return mImpl.SaveMessage(coroutine, this, chatType, fromCharacterId, characterName, content, ToCharacterId);
        }

        //缓存世界频道内容
        public static void CacheWorldMessage(uint serverId,
                                             int chatType,
                                             ulong characterId,
                                             string characterName,
                                             ChatMessageContent content)
        {
            mImpl.CacheWorldMessage(serverId, chatType, characterId, characterName, content);
        }

        //世界聊天广播
        public static void BroadcastServerIdMessage(uint ServerId,
                                                    int chatType,
                                                    ulong CharacterId,
                                                    string characterName,
                                                    ChatMessageContent content)
        {
            mImpl.BroadcastServerIdMessage(ServerId, chatType, CharacterId, characterName, content);
        }

        public static ErrorCodes BroadcastAllServerMessage(int chatType, ulong characterId, string characterName,
            ChatMessageContent content)
        {
            return mImpl.BroadcastAllServerMessage(chatType, characterId, characterName, content);
        }

        public static void BroadcastAnchorOnlineMessage(uint serverId, string name, int online)
        {
            mImpl.BroadcastAnchorOnlineMessage(serverId, name, online);
        }

        public static void BroadcastAllAnchorOnlineMessage(string name, int online)
        {
            mImpl.BroadcastAllAnchorOnlineMessage(name, online);
        }

        #endregion

        #region 上下线 相关

        //上线
        public void Online()
        {
            mImpl.Online(this);
        }

        //下线
        public static void OnLost(ulong characterId)
        {
            mImpl.OnLost(characterId);
        }

        #endregion

        #region 喇叭心跳

        //喇叭心跳
        public static void HornTick()
        {
            mImpl.HornTick();
        }

        public static void PushHornMessage(uint serverId,
                                           int chatType,
                                           ulong characterId,
                                           string characterName,
                                           ChatMessageContent content)
        {
            mImpl.PushHornMessage(serverId, chatType, characterId, characterName, content);
        }

        public static void CreateHornTrigger()
        {
            mImpl.CreateHornTrigger();
        }

        public static void DeleteHornTrigger()
        {
            mImpl.DeleteHornTrigger();
        }

        #endregion
    }
}