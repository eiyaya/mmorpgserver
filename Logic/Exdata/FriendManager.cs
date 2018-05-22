#region using

using System;
using System.Collections;
using System.Collections.Generic;
using DataContract;
using EventSystem;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Logic
{
    public interface IFriendData
    {
        void FriendData(FriendData _this, CharacterController character, ulong nId, int type = 0);
        void PushInfoData(FriendData _this, CharacterSimpleData info);
        void PushSimpleData(FriendData _this, SceneSimpleData result, bool isOnline);
    }

    public class FriendDataDefaultImpl : IFriendData
    {
        public void FriendData(FriendData _this, CharacterController character, ulong nId, int type = 0)
        {
            _this.mDbData = new DBFriend();
            _this.Guid = nId;
            _this.mDbData.Time = DateTime.Now.ToBinary();
            _this.mDbData.Type = type;
        }

        public void PushSimpleData(FriendData _this, SceneSimpleData result, bool isOnline)
        {
            var temp = new CharacterSimpleData
            {
                Id = result.Id,
                TypeId = result.TypeId,
                Name = result.Name,
                SceneId = result.SceneId,
                FightPoint = result.FightPoint,
                Level = result.Level,
                Ladder = result.Ladder,
                ServerId = result.ServerId
            };
            if (isOnline)
            {
                temp.Online = 1;
            }
            else
            {
                temp.Online = 0;
            }
            _this.InfoData = temp;
        }

        public void PushInfoData(FriendData _this, CharacterSimpleData info)
        {
            _this.InfoData = info;
        }
    }

    public class FriendData
    {
        private static IFriendData mImpl;

        static FriendData()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (FriendData), typeof (FriendDataDefaultImpl),
                o => { mImpl = (IFriendData) o; });
        }

        //构造邮件
        public FriendData(DBFriend dbdata)
        {
            mDbData = dbdata;
        }

        public FriendData(CharacterController character, ulong nId, int type = 0)
        {
            mImpl.FriendData(this, character, nId, type);
        }

        public CharacterSimpleData InfoData;
        //public CharacterSimpleData SimpleData;
        public DBFriend mDbData;

        public ulong Guid
        {
            get { return mDbData.Guid; }
            set { mDbData.Guid = value; }
        }

        public void PushInfoData(CharacterSimpleData info)
        {
            mImpl.PushInfoData(this, info);
        }

        public void PushSimpleData(SceneSimpleData result, bool isOnline)
        {
            mImpl.PushSimpleData(this, result, isOnline);
        }
    }

    public interface IFriendManager
    {
        ErrorCodes AddEnemy(FriendManager _this, ulong addId, int type);
        ErrorCodes AddFriend(FriendManager _this, ulong addId);
        ErrorCodes AddShield(FriendManager _this, ulong addId);
        ErrorCodes CheckAddEnemy(FriendManager _this, ulong id);
        ErrorCodes CheckAddFriend(FriendManager _this, ulong id);
        ErrorCodes CheckAddShield(FriendManager _this, ulong id);
        ErrorCodes DelEnemy(FriendManager _this, ulong delId);
        ErrorCodes DelFriend(FriendManager _this, ulong delId);
        ErrorCodes DelShield(FriendManager _this, ulong delId);
        FriendData GetEnemy(FriendManager _this, ulong id);
        FriendData GetFirstAutoEnemy(FriendManager _this);
        FriendData GetFriend(FriendManager _this, ulong id);
        FriendData GetShield(FriendManager _this, ulong id);
        DBFriends InitByBase(FriendManager _this, CharacterController character);
        void InitByDB(FriendManager _this, CharacterController character, DBFriends friendsData);
        void PushDataChange(FriendManager _this, ulong uId, CharacterSimpleData info);
        void SendChanges(FriendManager _this);
        void SetBehaveData(FriendManager _this, int type, ulong targetId, int operate);
        void UpdateBeHaveData(FriendManager _this, int type, ulong targetId, int operate);

        IEnumerator UpdateBeHaveDataCoroutine(Coroutine coroutine,
                                              FriendManager _this,
                                              int type,
                                              ulong targetId,
                                              int operate);
    }

    public class FriendManagerDefaultImpl : IFriendManager
    {
        public void SetBehaveData(FriendManager _this, int type, ulong targetId, int operate)
        {
            var db = _this.mCharacter.mFriend.mDbData;
            switch (type)
            {
                case 0:
                {
                    if (operate == 1)
                    {
                        if (!db.BeHaveFriends.Contains(targetId))
                        {
                            db.BeHaveFriends.Add(targetId);
                        }
                    }
                    else
                    {
                        if (db.BeHaveFriends.Contains(targetId))
                        {
                            db.BeHaveFriends.Remove(targetId);
                        }
                    }
                }
                    break;
                case 1:
                {
                    if (operate == 1)
                    {
                        if (!db.BeHaveEnemys.Contains(targetId))
                        {
                            db.BeHaveEnemys.Add(targetId);
                        }
                    }
                    else
                    {
                        if (db.BeHaveEnemys.Contains(targetId))
                        {
                            db.BeHaveEnemys.Remove(targetId);
                        }
                    }
                }
                    break;
                case 2:
                {
                    if (operate == 1)
                    {
                        if (!db.BeHaveShield.Contains(targetId))
                        {
                            db.BeHaveShield.Add(targetId);
                        }
                    }
                    else
                    {
                        if (db.BeHaveShield.Contains(targetId))
                        {
                            db.BeHaveShield.Remove(targetId);
                        }
                    }
                }
                    break;
            }
        }

        //更新目标的数据，operate，0：删除，1增加
        public void UpdateBeHaveData(FriendManager _this, int type, ulong targetId, int operate)
        {
//通过targetid找到character，改变character的好友相关数据
            var selfCharacterId = _this.mCharacter.mGuid;
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(targetId);
            if (character != null)
            {
                character.mFriend.SetBehaveData(type, selfCharacterId, operate);
            }
            else
            {
                CoroutineFactory.NewCoroutine(UpdateBeHaveDataCoroutine, _this, type, targetId, operate).MoveNext();
            }
        }

        //如果发现不在一个Server上，则通过ss网络包，通知其他Server服务器
        public IEnumerator UpdateBeHaveDataCoroutine(Coroutine coroutine,
                                                     FriendManager _this,
                                                     int type,
                                                     ulong targetId,
                                                     int operate)
        {
//通过targetid发送网络，改变其的好友相关数据
            var msg = LogicServer.Instance.LogicAgent.SSFriendpPssiveChange(targetId, type, _this.mCharacter.mGuid,
                operate);
            yield return msg.SendAndWaitUntilDone(coroutine);
        }

        #region 初始化

        //用第一次创建
        public DBFriends InitByBase(FriendManager _this, CharacterController character)
        {
            _this.mDbData = new DBFriends();
            _this.mCharacter = character;
            _this.MarkDirty();
            return _this.mDbData;
        }

        //用数据库数据
        public void InitByDB(FriendManager _this, CharacterController character, DBFriends friendsData)
        {
            _this.mCharacter = character;
            _this.mDbData = friendsData;

            foreach (var dbDatas in _this.mDbData.Friends)
            {
                var friend = new FriendData(dbDatas.Value);
                _this.mDataFriend.Add(dbDatas.Key, friend);
            }
            foreach (var dbDatas in _this.mDbData.Enemys)
            {
                var friend = new FriendData(dbDatas.Value);
                _this.mDataEnemy.Add(dbDatas.Key, friend);
            }
            foreach (var dbDatas in _this.mDbData.Shield)
            {
                var friend = new FriendData(dbDatas.Value);
                _this.mDataShield.Add(dbDatas.Key, friend);
            }
        }

        //通知客户端变化量
        public void SendChanges(FriendManager _this)
        {
            if (_this.mCharacter.Proxy != null)
            {
                _this.mCharacter.Proxy.SyncFriendDataChange(_this.Changes);
                _this.Changes.Characters.Clear();
            }
            _this.ChangesTrigger = null;
        }

        //数据变化
        public void PushDataChange(FriendManager _this, ulong uId, CharacterSimpleData info)
        {
            var isFind = false;
            var f = GetFriend(_this, uId);
            if (f != null)
            {
                f.PushInfoData(info);
                isFind = true;
            }
            f = GetEnemy(_this, uId);
            if (f != null)
            {
                f.PushInfoData(info);
                isFind = true;
            }
            f = GetShield(_this, uId);
            if (f != null)
            {
                f.PushInfoData(info);
                isFind = true;
            }
            if (isFind)
            {
                _this.Changes.Characters.Add(info);
                if (_this.ChangesTrigger == null)
                {
                    _this.ChangesTrigger = LogicServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(3),
                        () => { SendChanges(_this); });
                }
            }
        }

        #endregion

        #region  好友方法

        //获得一个好友
        public FriendData GetFriend(FriendManager _this, ulong id)
        {
            FriendData first;
            if (_this.mDataFriend.TryGetValue(id, out first))
            {
                return first;
            }
            return null;
        }

        //是否已有好友
        public ErrorCodes CheckAddFriend(FriendManager _this, ulong id)
        {
            if (_this.mCharacter.mGuid == id)
            {
                return ErrorCodes.Error_NotAddSelf;
            }
            if (_this.mDataFriend.ContainsKey(id))
            {
                return ErrorCodes.Error_FriendIsHave;
            }
            if (_this.mDataFriend.Count >= FriendManager.mFriendMax)
            {
                return ErrorCodes.Error_FriendIsMore;
            }
            return ErrorCodes.OK;
        }

        //添加好友
        public ErrorCodes AddFriend(FriendManager _this, ulong addId)
        {
            //是否已经是好友
            if (_this.mDataFriend.ContainsKey(addId))
            {
                return ErrorCodes.Error_FriendIsHave;
            }
            UpdateBeHaveData(_this, 0, addId, 1);
            //是否在线

            //添加玩家
            var tempf = new FriendData(_this.mCharacter, addId);
            _this.mDataFriend.Add(addId, tempf);
            _this.mDbData.Friends.Add(addId, tempf.mDbData);
            _this.mCharacter.AddExData((int) eExdataDefine.e41, 1);
            var oldCount = _this.mCharacter.GetExData((int) eExdataDefine.e44);
            if (_this.mDataFriend.Count > oldCount)
            {
                _this.mCharacter.SetExData((int) eExdataDefine.e44, _this.mDataFriend.Count);
            }
            var e = new AddFriendEvent(_this.mCharacter);
            EventDispatcher.Instance.DispatchEvent(e);
            return ErrorCodes.OK;
        }

        //删除好友
        public ErrorCodes DelFriend(FriendManager _this, ulong delId)
        {
            //是否已经是好友
            if (!_this.mDataFriend.ContainsKey(delId))
            {
                return ErrorCodes.Error_NoThisSelf;
            }
            UpdateBeHaveData(_this, 0, delId, 0);
            //删除
            _this.mDataFriend.Remove(delId);
            _this.mDbData.Friends.Remove(delId);
            return ErrorCodes.OK;
        }

        #endregion

        #region  仇人方法

        //获得一个仇人
        public FriendData GetEnemy(FriendManager _this, ulong id)
        {
            FriendData first;
            if (_this.mDataEnemy.TryGetValue(id, out first))
            {
                return first;
            }
            return null;
        }

        //是否已有仇人
        public ErrorCodes CheckAddEnemy(FriendManager _this, ulong id)
        {
            if (_this.mCharacter.mGuid == id)
            {
                return ErrorCodes.Error_NotAddSelf;
            }
            if (_this.mDataEnemy.ContainsKey(id))
            {
                return ErrorCodes.Error_FriendIsHave;
            }
            if (_this.mDataEnemy.Count >= FriendManager.mEnemyMax)
            {
                return ErrorCodes.Error_EnemyIsMore;
            }
            return ErrorCodes.OK;
        }

        //添加仇人
        public ErrorCodes AddEnemy(FriendManager _this, ulong addId, int type)
        {
            //是否已经是仇人
            if (_this.mDataEnemy.ContainsKey(addId))
            {
                return ErrorCodes.Unknow;
            }

            //主动添加仇人时，需要移除好友
            //if (type == 1)
            //{
            //    DelFriend(_this, addId);
            //}
            UpdateBeHaveData(_this, 1, addId, 1);
            //添加玩家
            var tempf = new FriendData(_this.mCharacter, addId, type);
            _this.mDataEnemy.Add(addId, tempf);
            _this.mDbData.Enemys.Add(addId, tempf.mDbData);
            return ErrorCodes.OK;
        }

        //获得一个自动添加并且时间最早的一个
        public FriendData GetFirstAutoEnemy(FriendManager _this)
        {
            FriendData first = null;
            foreach (var data in _this.mDataEnemy)
            {
                if (data.Value.mDbData.Type == 1)
                {
                    continue;
                }
                if (first == null)
                {
                    first = data.Value;
                    continue;
                }
                if (DateTime.FromBinary(first.mDbData.Time) > DateTime.FromBinary(data.Value.mDbData.Time))
                {
                    first = data.Value;
                }
            }
            return first;
        }

        //删除仇人
        public ErrorCodes DelEnemy(FriendManager _this, ulong delId)
        {
            //是否已经是好友
            if (!_this.mDataEnemy.ContainsKey(delId))
            {
                return ErrorCodes.Unknow;
            }
            UpdateBeHaveData(_this, 1, delId, 0);
            //删除
            _this.mDataEnemy.Remove(delId);
            _this.mDbData.Enemys.Remove(delId);
            return ErrorCodes.OK;
        }

        #endregion

        #region  屏蔽方法

        //获得一个屏蔽
        public FriendData GetShield(FriendManager _this, ulong id)
        {
            FriendData first;
            if (_this.mDataShield.TryGetValue(id, out first))
            {
                return first;
            }
            return null;
        }

        //是否已有屏蔽
        public ErrorCodes CheckAddShield(FriendManager _this, ulong id)
        {
            if (_this.mCharacter.mGuid == id)
            {
                return ErrorCodes.Error_NotAddSelf;
            }
            if (_this.mDataShield.ContainsKey(id))
            {
                return ErrorCodes.Error_FriendIsHave;
            }
            if (_this.mDataShield.Count >= FriendManager.mShieldMax)
            {
                return ErrorCodes.Error_ShieldIsMore;
            }
            return ErrorCodes.OK;
        }

        //添加屏蔽
        public ErrorCodes AddShield(FriendManager _this, ulong addId)
        {
            //是否已经是好友
            if (_this.mDataShield.ContainsKey(addId))
            {
                return ErrorCodes.Unknow;
            }

            UpdateBeHaveData(_this, 2, addId, 1);
            //DelFriend(_this, addId);

            //添加玩家
            var tempf = new FriendData(_this.mCharacter, addId);
            _this.mDataShield.Add(addId, tempf);
            _this.mDbData.Shield.Add(addId, tempf.mDbData);
            return ErrorCodes.OK;
        }

        //删除屏蔽
        public ErrorCodes DelShield(FriendManager _this, ulong delId)
        {
            //是否已经是好友
            if (!_this.mDataShield.ContainsKey(delId))
            {
                return ErrorCodes.Unknow;
            }
            UpdateBeHaveData(_this, 2, delId, 0);
            //删除
            _this.mDataShield.Remove(delId);
            _this.mDbData.Shield.Remove(delId);
            return ErrorCodes.OK;
        }

        #endregion
    }

    public class FriendManager : NodeBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static IFriendManager mImpl;

        static FriendManager()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (FriendManager), typeof (FriendManagerDefaultImpl),
                o => { mImpl = (IFriendManager) o; });
        }

        public CharacterSimpleDataList Changes = new CharacterSimpleDataList();
        public object ChangesTrigger;
        public DateTime EnemyNextUpdateTime = DateTime.Now;
        public DateTime FriendNextUpdateTime = DateTime.Now;
        public CharacterController mCharacter; //角色
        public Dictionary<ulong, FriendData> mDataEnemy = new Dictionary<ulong, FriendData>();
        //数据结构
        public Dictionary<ulong, FriendData> mDataFriend = new Dictionary<ulong, FriendData>();
        public Dictionary<ulong, FriendData> mDataShield = new Dictionary<ulong, FriendData>();
        public DBFriends mDbData;
        public DateTime SeekCharacterNameNextUpdateTime = DateTime.Now;
        public DateTime SeekFriendNextUpdateTime = DateTime.Now;
        public DateTime ShieldNextUpdateTime = DateTime.Now;

        public override IEnumerable<NodeBase> Children
        {
            get { return null; }
        }

        public static int mEnemyMax
        {
            get { return StaticParam.mEnemyMax; }
        }

        public static int mFriendMax
        {
            get { return StaticParam.mFriendMax; }
        }

        public static int mShieldMax
        {
            get { return StaticParam.mShieldMax; }
        }

        public void SetBehaveData(int type, ulong targetId, int operate)
        {
            mImpl.SetBehaveData(this, type, targetId, operate);
        }

        //更新目标的数据，operate，0：删除，1增加
        public void UpdateBeHaveData(int type, ulong targetId, int operate)
        {
            mImpl.UpdateBeHaveData(this, type, targetId, operate);
        }

        //如果发现不在一个Server上，则通过ss网络包，通知其他Server服务器
        public IEnumerator UpdateBeHaveDataCoroutine(Coroutine coroutine, int type, ulong targetId, int operate)
        {
            return mImpl.UpdateBeHaveDataCoroutine(coroutine, this, type, targetId, operate);
        }

        #region 初始化

        //用第一次创建
        public DBFriends InitByBase(CharacterController character)
        {
            return mImpl.InitByBase(this, character);
        }

        //用数据库数据
        public void InitByDB(CharacterController character, DBFriends friendsData)
        {
            mImpl.InitByDB(this, character, friendsData);
        }

        //通知客户端变化量
        public void SendChanges()
        {
            mImpl.SendChanges(this);
        }

        //数据变化
        public void PushDataChange(ulong uId, CharacterSimpleData info)
        {
            mImpl.PushDataChange(this, uId, info);
        }

        #endregion

        #region  好友方法

        //获得一个好友
        public FriendData GetFriend(ulong id)
        {
            return mImpl.GetFriend(this, id);
        }

        //是否已有好友
        public ErrorCodes CheckAddFriend(ulong id)
        {
            return mImpl.CheckAddFriend(this, id);
        }

        //添加好友
        public ErrorCodes AddFriend(ulong addId)
        {
            return mImpl.AddFriend(this, addId);
        }

        //删除好友
        public ErrorCodes DelFriend(ulong delId)
        {
            return mImpl.DelFriend(this, delId);
        }

        #endregion

        #region  仇人方法

        //获得一个仇人
        public FriendData GetEnemy(ulong id)
        {
            return mImpl.GetEnemy(this, id);
        }

        //是否已有仇人
        public ErrorCodes CheckAddEnemy(ulong id)
        {
            return mImpl.CheckAddEnemy(this, id);
        }

        //添加仇人
        public ErrorCodes AddEnemy(ulong addId, int type)
        {
            return mImpl.AddEnemy(this, addId, type);
        }

        //获得一个自动添加并且时间最早的一个
        public FriendData GetFirstAutoEnemy()
        {
            return mImpl.GetFirstAutoEnemy(this);
        }

        //删除仇人
        public ErrorCodes DelEnemy(ulong delId)
        {
            return mImpl.DelEnemy(this, delId);
        }

        #endregion

        #region  屏蔽方法

        //获得一个屏蔽
        public FriendData GetShield(ulong id)
        {
            return mImpl.GetShield(this, id);
        }

        //是否已有屏蔽
        public ErrorCodes CheckAddShield(ulong id)
        {
            return mImpl.CheckAddShield(this, id);
        }

        //添加屏蔽
        public ErrorCodes AddShield(ulong addId)
        {
            return mImpl.AddShield(this, addId);
        }

        //删除屏蔽
        public ErrorCodes DelShield(ulong delId)
        {
            return mImpl.DelShield(this, delId);
        }

        #endregion
    }
}