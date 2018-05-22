#region using

using System;
using System.Collections.Generic;
using DataContract;
using NLog;
using Shared;
using DataTable;
using EventSystem;

#endregion

namespace Chat
{
    public interface IChatCharacterController
    {
        void ApplyEvent(ChatCharacterController _this, int eventId, string evt, int count);
        void ApplySimpleData(ChatCharacterController _this, DBCharacterChatSimple simpleData);
        void ApplyVolatileData(ChatCharacterController _this, DBCharacterChatVolatile data);
        DBCharacterChat GetData(ChatCharacterController _this);
        bool GetOnline(ChatCharacterController _this);
        DBCharacterChatSimple GetSimpleData(ChatCharacterController _this);
        List<TimedTaskItem> GetTimedTasks(ChatCharacterController _this);
        DBCharacterChat InitByBase(ChatCharacterController _this, ulong characterId, object[] args = null);
        bool InitByDb(ChatCharacterController _this, ulong characterId, DBCharacterChat dbData);
        void InitChatCharacterController(ChatCharacterController _this);
        void LoadFinished(ChatCharacterController _this);
        void OnDestroy(ChatCharacterController _this);
        void OnSaveData(ChatCharacterController _this, DBCharacterChat data, DBCharacterChatSimple simpleData);
        void Tick(ChatCharacterController _this);
        void SetMoniterData(ChatCharacterController _this, MsgChatMoniterData data);
    }

    public class ChatCharacterControllerDefaultImpl : IChatCharacterController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region 初始化

        public void InitChatCharacterController(ChatCharacterController _this)
        {
            _this.mChat = new ChatManager();
            _this.childs[0] = _this.mChat;
        }

        public bool GetOnline(ChatCharacterController _this)
        {
            return _this.Proxy != null && _this.Proxy.Connected;
        }

        public List<TimedTaskItem> GetTimedTasks(ChatCharacterController _this)
        {
            return null;
        }

        public void ApplyEvent(ChatCharacterController _this, int eventId, string evt, int count)
        {
        }

        public bool InitByDb(ChatCharacterController _this, ulong characterId, DBCharacterChat dbData)
        {
            //PlayerLog.WriteLog(characterId, "----------Chat--------------------InitByDb--------------------{0}", data.SaveCount);
            _this.mDbData = dbData;
            _this.mChat.InitByDB(_this, _this.mDbData);
            return true;
        }

        public DBCharacterChat InitByBase(ChatCharacterController _this, ulong characterId, object[] args = null)
        {
            _this.mDbData = new DBCharacterChat
            {
                Id = characterId
            };
            _this.mChat.InitByBase(_this, _this.mDbData);
            return _this.mDbData;
        }

        #endregion

        #region 继承方法

        //离线数据读取
        public void ApplyVolatileData(ChatCharacterController _this, DBCharacterChatVolatile data)
        {
            foreach (var msg in data.Chats)
            {
                _this.mDbData.Chats.Add(msg);
            }
        }

        public void LoadFinished(ChatCharacterController _this)
        {
        }

        public void ApplySimpleData(ChatCharacterController _this, DBCharacterChatSimple simpleData)
        {
        }

        public DBCharacterChatSimple GetSimpleData(ChatCharacterController _this)
        {
            DBCharacterChatSimple dbSimple;
            CharacterManager<ChatCharacterController, DBCharacterChat, DBCharacterChatSimple, DBCharacterChatVolatile>.
                DataItem data;
            var dic = CharacterManager.Instance.mDictionary;
            if (dic.TryGetValue(_this.mGuid, out data))
            {
                dbSimple = data.SimpleData;
            }
            else
            {
                Logger.Info("GetSimpleData return null, id = {0}", _this.mGuid);
                dbSimple = new DBCharacterChatSimple();
            }
            dbSimple.Id = _this.mGuid;
            return dbSimple;
        }

        public DBCharacterChat GetData(ChatCharacterController _this)
        {
            return _this.mDbData;
        }

        public void Tick(ChatCharacterController _this)
        {
            if (_this.NetDirty)
            {
                foreach (var child in _this.Children)
                {
                    if (child.NetDirty)
                    {
                        child.NetDirtyHandle();
                    }
                }
                _this.CleanNetDirty();
            }
        }

        public void SetMoniterData(ChatCharacterController _this, MsgChatMoniterData data)
        {
            _this.moniterData = data;
        }
        public void OnDestroy(ChatCharacterController _this)
        {
        }

        public void OnSaveData(ChatCharacterController _this, DBCharacterChat data, DBCharacterChatSimple simpleData)
        {
            //PlayerLog.WriteLog(CharacterId, "----------Chat--------------------OnSaveData--------------------{0}", data.SaveCount);
        }

        #endregion
    }

    public class ChatCharacterController : NodeBase,
                                           ICharacterControllerBase
                                               <DBCharacterChat, DBCharacterChatSimple, DBCharacterChatVolatile>
    {
        //存储私聊对象
        public void PushChat(LogicSimpleData logicSimple)
        {
            if (mDbData.NearChats == null)
            {
                mDbData.NearChats = new PlayerHeadInfoMsgList();
            }
            var index = 0;
            foreach (var msg in mDbData.NearChats.Characters)
            {
                if (msg.CharacterId == logicSimple.Id)
                {
                    //var temp = msg;
                    mDbData.NearChats.Characters.RemoveAt(index);                  
                    //mDbData.NearChats.Characters.Add(msg);
                    MarkDbDirty();
                    //return;
                    break;
                }
                index++;
            }
            var t = new PlayerHeadInfoMsg();
            t.CharacterId = logicSimple.Id;
            t.Name = logicSimple.Name;
            t.RoleId = logicSimple.TypeId;
            t.Level = logicSimple.Level;
            t.Ladder = logicSimple.Ladder;
            mDbData.NearChats.Characters.Add(t);
            if (mDbData.NearChats.Characters.Count > 20)
            {
                mDbData.NearChats.Characters.RemoveAt(0);
            }
            MarkDbDirty();
            //uint64		 CharacterId	= 1;
            //string		 Name			= 2;
            //int32		 RoleId			= 3;
            //int32		 Level			= 4;	
            //int32 		 Ladder			= 5;
            //int32 		 FightValue		= 6;
        }
     
        #region 数据结构

        public DBCharacterChat mDbData { get; set; }
        public ChatProxy Proxy { get; set; }
        public string Name { get; set; }
        public ulong ChannelGuid { get; set; }
        public MsgChatMoniterData moniterData { get; set; }

        public ulong mGuid
        {
            get { return mDbData.Id; }
        }

        public int ServerId { get; set; }
        public ChatManager mChat;
        public NodeBase[] childs = new NodeBase[1];
        private static IChatCharacterController mImpl;

        static ChatCharacterController()
        {
            ChatServer.Instance.UpdateManager.InitStaticImpl(typeof (ChatCharacterController),
                typeof (ChatCharacterControllerDefaultImpl),
                o => { mImpl = (IChatCharacterController) o; });
        }

        #endregion

        #region 初始化

        //构造
        public ChatCharacterController()
        {
            mImpl.InitChatCharacterController(this);
        }

        public override IEnumerable<NodeBase> Children
        {
            get { return childs; }
        }

        public bool Online
        {
            get { return mImpl.GetOnline(this); }
        }

        public List<TimedTaskItem> GetTimedTasks()
        {
            return mImpl.GetTimedTasks(this);
        }

        public void ApplyEvent(int eventId, string evt, int count)
        {
            mImpl.ApplyEvent(this, eventId, evt, count);
        }

        public CharacterState State { get; set; }

        public bool InitByDb(ulong characterId, DBCharacterChat dbData)
        {
            return mImpl.InitByDb(this, characterId, dbData);
        }

        public DBCharacterChat InitByBase(ulong characterId, object[] args = null)
        {
            return mImpl.InitByBase(this, characterId, args);
        }

        #endregion

        #region 继承方法

        //离线数据读取
        public void ApplyVolatileData(DBCharacterChatVolatile data)
        {
            mImpl.ApplyVolatileData(this, data);
        }

        public void LoadFinished()
        {
            mImpl.LoadFinished(this);
        }

        public void ApplySimpleData(DBCharacterChatSimple simpleData)
        {
            mImpl.ApplySimpleData(this, simpleData);
        }

        public DBCharacterChatSimple GetSimpleData()
        {
            return mImpl.GetSimpleData(this);
        }

        public DBCharacterChat GetData()
        {
            return mImpl.GetData(this);
        }

        public void Tick()
        {
            mImpl.Tick(this);
        }

        public void OnDestroy()
        {
            mImpl.OnDestroy(this);
        }

        public void OnSaveData(DBCharacterChat data, DBCharacterChatSimple simpleData)
        {
            mImpl.OnSaveData(this, data, simpleData);
        }

        public void SetMoniterData(MsgChatMoniterData data)
        {
            mImpl.SetMoniterData(this,data);
        }
        #endregion
    }
}