#region using

using System;
using System.Collections.Generic;
using System.Linq;
using DataContract;
using Mono.GameMath;
using NLog;
using Shared;

#endregion

namespace Scene
{
    public interface IObjBase
    {
        void BroadcastCreateMe(ObjBase _this, ReasonType reason = ReasonType.VisibilityChanged);
        void BroadcastDestroyMe(ObjBase _this, ReasonType reason = ReasonType.VisibilityChanged);
        void Dispose(ObjBase _this);
        ObjData DumpObjData(ObjBase _this, ReasonType reason);
        void EnterScene(ObjBase _this, Scene scene);
        IEnumerable<ulong> EnumAllVisiblePlayerIdExclude(ObjBase _this, ulong excludeId = TypeDefine.INVALID_ULONG);
        Vector2 GetDirection(ObjBase _this);
        Vector2 GetPosition(ObjBase _this);
        void InitBase(ObjBase _this, ulong characterId, int dataId);
        bool IsVisibleTo(ObjBase _this, ObjBase obj);
        void LeavelScene(ObjBase _this);
        void NotifyCharactersToStopSyncMe(ObjBase _this, IEnumerable<ulong> players);
        void NotifyCharactersToSyncMe(ObjBase _this, IEnumerable<ulong> players);
        void OnEnterScene(ObjBase _this);
        void OnLeaveScene(ObjBase _this);
        void RegisterCharacterSyncData(ObjBase _this, ObjBase obj);
        void RemoveCharacterSyncData(ObjBase _this, ObjBase obj);
        void SetDirection(ObjBase _this, Vector2 dir);
        void SetDirection(ObjBase _this, float x, float y);
        void SetPosition(ObjBase _this, Vector2 p);
        void SetPosition(ObjBase _this, float x, float y);
        void SetZone(ObjBase _this, Zone zone);
        void Tick(ObjBase _this, float delta);
    }

    public class ObjBaseDefaultImpl : IObjBase
    {
        public static Logger Logger = LogManager.GetCurrentClassLogger();

        #region 构造函数

        public void InitBase(ObjBase _this, ulong characterId, int dataId)
        {
            _this.mObjId = characterId;
            _this.mTypeId = dataId;
            _this.mDirection = new Vector2(1, 0);
        }

        #endregion

        public ObjData DumpObjData(ObjBase _this, ReasonType reason)
        {
            var data = new ObjData
            {
                ObjId = _this.ObjId,
                Type = (int) _this.GetObjType(),
                DataId = _this.TypeId,
                Pos = Utility.MakePositionDataByPosAndDir(_this.GetPosition(), _this.GetDirection()),
                Reason = (uint) reason
            };
            return data;
        }

        //我是否能被其他obj看见
        public virtual bool IsVisibleTo(ObjBase _this, ObjBase obj)
        {
            return true;
        }

        public virtual void Dispose(ObjBase _this)
        {
        }

        public virtual void RegisterCharacterSyncData(ObjBase _this, ObjBase obj)
        {
        }

        public virtual void RemoveCharacterSyncData(ObjBase _this, ObjBase obj)
        {
        }

        public void NotifyCharactersToSyncMe(ObjBase _this, IEnumerable<ulong> players)
        {
            //             if (Scene == null)
            //             {
            //                 return;
            //             }
            //             foreach (var id in players)
            //             {
            //                 var player = Scene.FindPlayer(id);
            //                 if (player != null)
            //                 {
            //                     player.RegisterCharacterSyncData(this);
            //                 }
            //             }
        }

        public void NotifyCharactersToStopSyncMe(ObjBase _this, IEnumerable<ulong> players)
        {
            //             if (Scene == null)
            //             {
            //                 return;
            //             }
            //             foreach (var id in players)
            //             {
            //                 var player = Scene.FindPlayer(id);
            //                 if (player != null)
            //                 {
            //                     player.RemoveCharacterSyncData(this);
            //                 }
            //             }
        }

        public void Tick(ObjBase _this, float delta)
        {
        }

        #region 基本数据的方法

        //设置坐标
        public void SetPosition(ObjBase _this, Vector2 p)
        {
            //if (float.IsNaN(p.X))
            //{
            //    Logger.Error("SetPosition ");
            //    throw new Exception("SetPosition ");
            //}
            _this.mPosition = p;
        }

        public void SetPosition(ObjBase _this, float x, float y)
        {
            _this.SetPosition(new Vector2(x, y));
        }


        public Vector2 GetPosition(ObjBase _this)
        {
            return _this.mPosition;
        }

        //设置朝向
        public void SetDirection(ObjBase _this, Vector2 dir)
        {
            dir.Normalize();
            _this.mDirection = dir;
        }

        public void SetDirection(ObjBase _this, float x, float y)
        {
            _this.SetDirection(new Vector2(x, y));
        }

        public Vector2 GetDirection(ObjBase _this)
        {
            return _this.mDirection;
        }

        #endregion

        #region 场景和区域

        //进入场景
        public void EnterScene(ObjBase _this, Scene scene)
        {
            _this.Scene = scene;
            _this.OnEnterScene();
        }

        //离开场景
        public void LeavelScene(ObjBase _this)
        {
            _this.OnLeaveScene();
            _this.Scene = null;
        }

        public void OnEnterScene(ObjBase _this)
        {
            _this.Active = true;
        }

        public void OnLeaveScene(ObjBase _this)
        {
            _this.Active = false;
        }

        //进去区域
        public void SetZone(ObjBase _this, Zone zone)
        {
            _this.Zone = zone;
        }

        #endregion

        #region //广播相关

        public IEnumerable<ulong> EnumAllVisiblePlayerIdExclude(ObjBase _this,
                                                                ulong excludeId = TypeDefine.INVALID_ULONG)
        {
            if (_this.Zone == null)
            {
                Logger.Warn("EnumAllVisiblePlayerIdExclude has null zone.");
                return ObjBase.sEmtpyIdList;
            }

            if (TypeDefine.INVALID_ULONG == excludeId)
            {
                return _this.Zone.EnumAllVisiblePlayerId();
            }
            return _this.Zone.EnumAllVisiblePlayerIdExclude(excludeId);
        }

        //角色增加广播
        public virtual void BroadcastCreateMe(ObjBase _this, ReasonType reason = ReasonType.VisibilityChanged)
        {
            if (null == _this.Zone)
            {
                return;
            }
            var data = _this.DumpObjData(reason);
            if (ObjBase.BroadcastCreateObjType == 1)
            {
                _this.Zone.PushCreateObj(data);
                return;
            }
            var msg2Other = new CreateObjMsg();
            msg2Other.Data.Add(data);
            var list = _this.EnumAllVisiblePlayerIdExclude(_this.ObjId).ToArray();
            SceneServer.Instance.ServerControl.CreateObj(list, msg2Other);
            _this.NotifyCharactersToSyncMe(list);
        }

        //角色删除广播
        public virtual void BroadcastDestroyMe(ObjBase _this, ReasonType reason = ReasonType.VisibilityChanged)
        {
            if (null == _this.Zone)
            {
                return;
            }
            if (ObjBase.BroadcastDeleteObjType == 1)
            {
                _this.Zone.PushDeleteObj(new DeleteObjMsg
                {
                    ObjId = _this.ObjId,
                    reason = (int) reason
                });
                return;
            }
            var array = new Uint64Array();
            array.Items.Add(_this.ObjId);
            var list = _this.EnumAllVisiblePlayerIdExclude(_this.ObjId).ToArray();
            _this.NotifyCharactersToStopSyncMe(list);
            SceneServer.Instance.ServerControl.DeleteObj(list, array, (uint) reason);
        }

        #endregion
    }

    public class ObjBase : NodeBase, IDisposable
    {
        public static int BroadcastCreateObjType = 0;
        public static int BroadcastDeleteObjType = 0;
        public static Logger Logger = LogManager.GetCurrentClassLogger();
        private static IObjBase mImpl;

        static ObjBase()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (ObjBase), typeof (ObjBaseDefaultImpl),
                o => { mImpl = (IObjBase) o; });
        }

        public override IEnumerable<NodeBase> Children
        {
            get { throw new NotImplementedException(); }
        }

        public virtual ObjData DumpObjData(ReasonType reason)
        {
            return mImpl.DumpObjData(this, reason);
        }

        public static IObjBase GetImpl()
        {
            return mImpl;
        }

        //Obj类型
        public virtual ObjType GetObjType()
        {
            return ObjType.INVALID;
        }

        #region 构造函数

        public void InitBase(ulong characterId, int dataId)
        {
            mImpl.InitBase(this, characterId, dataId);
        }

        #endregion

        public virtual bool IsCharacter()
        {
            return false;
        }

        //我是否能被其他obj看见
        public virtual bool IsVisibleTo(ObjBase obj)
        {
            return mImpl.IsVisibleTo(this, obj);
        }

        public void NotifyCharactersToStopSyncMe(IEnumerable<ulong> players)
        {
            mImpl.NotifyCharactersToStopSyncMe(this, players);
        }

        public void NotifyCharactersToSyncMe(IEnumerable<ulong> players)
        {
            mImpl.NotifyCharactersToSyncMe(this, players);
        }

        public virtual void RegisterCharacterSyncData(ObjBase obj)
        {
            mImpl.RegisterCharacterSyncData(this, obj);
        }

        public virtual void RemoveCharacterSyncData(ObjBase obj)
        {
            mImpl.RemoveCharacterSyncData(this, obj);
        }

        public virtual void Dispose()
        {
            mImpl.Dispose(this);
        }

        #region Obj基本属性

        //Id
        public ulong mObjId;

        //激活状态
        public bool mActive;

        //Type Id
        public int mTypeId;

        //所在服务器Id
        public virtual int ServerId { get; set; }

        //所在分线Id
        public int LineId { get; set; }

        //位置
        public Vector2 mPosition = Vector2.Zero;

        //朝向 
        public Vector2 mDirection = Vector2.UnitX;

        //是否移动过
        public bool mPositionChanged = false;

        //所在场景
        private Scene mScene;

        public Scene Scene
        {
            get { return mScene; }
            set
            {
                mScene = value;
                MarkDbDirty();
            }
        }

        private Zone mZone;
        //所在区域
        public Zone Zone
        {
            get
            {
                if (mPositionChanged)
                {
                    var obj = this as ObjCharacter;
                    if (obj != null)
                    {
                        obj.ProcessPositionChanged();
                    }
                }

                return mZone;
            }
            set { mZone = value; }
        }

        #endregion

        #region 固定方法

        public virtual void Tick(float delta)
        {
            mImpl.Tick(this, delta);
        }

        public virtual void Reset()
        {
        }

        public virtual void Destroy()
        {
        }

        #endregion

        #region 基本数据的方法

        public int TypeId
        {
            get { return mTypeId; }
        }

        public virtual ulong ObjId
        {
            get { return mObjId; }
        }

        public virtual bool Active
        {
            set { mActive = value; }
            get { return mActive; }
        }

        //设置坐标
        public virtual void SetPosition(Vector2 p)
        {
            mImpl.SetPosition(this, p);
        }

        public virtual void SetPosition(float x, float y)
        {
            mImpl.SetPosition(this, x, y);
        }

        public virtual Vector2 GetPosition()
        {
            return mImpl.GetPosition(this);
        }

        //设置朝向
        public virtual void SetDirection(Vector2 dir)
        {
            mImpl.SetDirection(this, dir);
        }

        public virtual void SetDirection(float x, float y)
        {
            mImpl.SetDirection(this, x, y);
        }

        public virtual Vector2 GetDirection()
        {
            return mImpl.GetDirection(this);
        }

        #endregion

        #region 场景和区域

        //进入场景
        public void EnterScene(Scene scene)
        {
            mImpl.EnterScene(this, scene);
        }

        //离开场景
        public void LeavelScene()
        {
            mImpl.LeavelScene(this);
        }

        public virtual void OnEnterScene()
        {
            mImpl.OnEnterScene(this);
        }

        public virtual void OnLeaveScene()
        {
            mImpl.OnLeaveScene(this);
        }

        //进去区域
        public virtual void SetZone(Zone zone)
        {
            mImpl.SetZone(this, zone);
        }

        #endregion

        #region //广播相关

        public static List<ulong> sEmtpyIdList = new List<ulong>();

        public IEnumerable<ulong> EnumAllVisiblePlayerIdExclude(ulong excludeId = TypeDefine.INVALID_ULONG)
        {
            return mImpl.EnumAllVisiblePlayerIdExclude(this, excludeId);
        }

        //角色增加广播
        public virtual void BroadcastCreateMe(ReasonType reason = ReasonType.VisibilityChanged)
        {
            mImpl.BroadcastCreateMe(this, reason);
        }

        //角色删除广播
        public virtual void BroadcastDestroyMe(ReasonType reason = ReasonType.VisibilityChanged)
        {
            mImpl.BroadcastDestroyMe(this, reason);
        }

        #endregion
    }
}