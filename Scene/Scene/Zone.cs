#region using

using System;
using System.Collections.Generic;
using System.Linq;
using DataContract;
using Mono.GameMath;
using NLog;

#endregion

namespace Scene
{
    public interface IZone
    {
        void AddObj(Zone _this, ObjBase obj);
        void AddVisibleZone(Zone _this, Zone zone);
        void BuffSend(Zone _this);
        void BulletSend(Zone _this);
        void CreateObjSend(Zone _this);
        void DeleteObjSend(Zone _this);
        IEnumerable<ObjBase> EnumAllActiveObj(Zone _this);
        IEnumerable<ObjBase> EnumAllVisibleObj(Zone _this);
        IEnumerable<ObjPlayer> EnumAllVisiblePlayer(Zone _this);
        IEnumerable<ObjPlayer> EnumAllVisiblePlayerExclude(Zone _this, ulong excludeId = TypeDefine.INVALID_ULONG);
        IEnumerable<ulong> EnumAllVisiblePlayerId(Zone _this);
        IEnumerable<ulong> EnumAllVisiblePlayerIdExclude(Zone _this, ulong excludeId = TypeDefine.INVALID_ULONG);
        Vector2 GetCenterPos(Zone _this);
        Vector2 GetInitPos(Zone _this);
        void InitZone(Zone _this, Scene scene, int id, float x, float y, float w, float h);
        void MarkDirty(Zone _this);
        void MoveToSend(Zone _this);
        void PushBuffMsg(Zone _this, BuffResultMsg temp);
        void PushBulletMsg(Zone _this, BulletMsg temp);
        void PushCreateObj(Zone _this, ObjData temp);
        void PushDeleteObj(Zone _this, DeleteObjMsg temp);
        void PushMoveToMsg(Zone _this, CharacterMoveMsg temp);
        void PushSkillMsg(Zone _this, CharacterUseSkillMsg temp);
        void RemoveObj(Zone _this, ObjBase obj);
        void Reset(Zone _this);
        void SkillSend(Zone _this);
    }

    public class ZoneDefaultImpl : IZone
    {
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Reset(Zone _this)
        {
            _this.Reset();
        }

        public void InitZone(Zone _this, Scene scene, int id, float x, float y, float w, float h)
        {
            _this.mScene = scene;
            _this.mId = id;
            _this.mOrginalX = x;
            _this.mOrginalY = y;
            _this.mWidth = w;
            _this.mHeight = h;
        }

        public void AddVisibleZone(Zone _this, Zone zone)
        {
            _this.VisibleZoneList.Add(zone);
        }

        public Vector2 GetInitPos(Zone _this)
        {
            return new Vector2(_this.mOrginalX, _this.mOrginalY);
        }

        public Vector2 GetCenterPos(Zone _this)
        {
            return new Vector2(_this.mOrginalX + 0.5f*_this.mWidth, _this.mOrginalY + 0.5f*_this.mHeight);
        }

        public void MarkDirty(Zone _this)
        {
            foreach (var zone in _this.VisibleZoneList)
            {
                zone.mDirty = true;
            }
        }

        public void AddObj(Zone _this, ObjBase obj)
        {
            if (!_this.mObjDict.ContainsKey(obj.ObjId))
            {
                _this.mObjDict.Add(obj.ObjId, obj);
                if (obj.GetObjType() == ObjType.PLAYER)
                {
                    MarkDirty(_this);
                }
            }
            else
            {
                Logger.Fatal("mObjDict has same key[0]", obj.ObjId);
            }
        }

        public void RemoveObj(Zone _this, ObjBase obj)
        {
            if (_this.mObjDict.ContainsKey(obj.ObjId))
            {
                _this.mObjDict.Remove(obj.ObjId);
                if (obj.GetObjType() == ObjType.PLAYER)
                {
                    MarkDirty(_this);
                }
            }
        }

#if ENUM_MODE
		public IEnumerable<ObjBase> EnumAllActiveObj()
		{
			foreach (var pair in ObjDict)
			{
				if(pair.Value.Active)
					yield return pair.Value;
			}
		}

		public IEnumerable<ObjBase> EnumAllVisibleObj()
		{
			foreach(var zone in VisibleZoneList)
			{
				foreach(var obj in zone.EnumAllActiveObj())
				{
					yield return obj;
				}
			}
		}		

		public IEnumerable<ObjPlayer> EnumAllVisiblePlayer()
		{
			foreach (var zone in VisibleZoneList)
			{
				foreach (var pair in zone.ObjDict)
				{
					if (!pair.Value.Active)
					{
						continue;
					}

					if (pair.Value.GetObjType() != ObjType.PLAYER)
					{
						continue;
					}

					yield return pair.Value as ObjPlayer;
				}
			}
		}

		public IEnumerable<ulong> EnumAllVisiblePlayerId()
		{
			foreach (var player in EnumAllVisiblePlayer())
			{
				yield return player.ObjId;
			}
		}

		public IEnumerable<ObjPlayer> EnumAllVisiblePlayerExclude(ulong excludeId = TypeDefine.INVALID_ULONG)
		{
			foreach(var player in EnumAllVisiblePlayer())
			{
				if (player.ObjId != excludeId)
				{
					yield return player;
				}
			}
		}

		public IEnumerable<ulong> EnumAllVisiblePlayerIdExclude(ulong excludeId = TypeDefine.INVALID_ULONG)
		{
			foreach (var player in EnumAllVisiblePlayerExclude(excludeId))
			{
				yield return player.ObjId;
			}
        }
#else
        public IEnumerable<ObjBase> EnumAllActiveObj(Zone _this)
        {
            foreach (var pair in _this.ObjDict)
            {
                if (pair.Value.Active)
                {
                    yield return pair.Value;
                }
            }
        }

        public IEnumerable<ObjBase> EnumAllVisibleObj(Zone _this)
        {
            foreach (var zone in _this.VisibleZoneList)
            {
                foreach (var pair in zone.ObjDict)
                {
                    if (pair.Value.Active)
                    {
                        yield return pair.Value;
                    }
                }
            }
        }

        public IEnumerable<ObjPlayer> EnumAllVisiblePlayer(Zone _this)
        {
            foreach (var zone in _this.VisibleZoneList)
            {
                foreach (var pair in zone.ObjDict)
                {
                    if (!pair.Value.Active)
                    {
                        continue;
                    }

                    if (pair.Value.GetObjType() != ObjType.PLAYER)
                    {
                        continue;
                    }

                    yield return pair.Value as ObjPlayer;
                }
            }
        }

        public IEnumerable<ulong> EnumAllVisiblePlayerId(Zone _this)
        {
            foreach (var zone in _this.VisibleZoneList)
            {
                foreach (var pair in zone.ObjDict)
                {
                    if (!pair.Value.Active)
                    {
                        continue;
                    }

                    if (pair.Value.GetObjType() != ObjType.PLAYER)
                    {
                        continue;
                    }

                    yield return pair.Key;
                }
            }
        }

        public IEnumerable<ObjPlayer> EnumAllVisiblePlayerExclude(Zone _this, ulong excludeId = TypeDefine.INVALID_ULONG)
        {
            foreach (var zone in _this.VisibleZoneList)
            {
                foreach (var pair in zone.ObjDict)
                {
                    if (!pair.Value.Active)
                    {
                        continue;
                    }

                    if (pair.Value.GetObjType() != ObjType.PLAYER)
                    {
                        continue;
                    }

                    if (pair.Key == excludeId)
                    {
                        continue;
                    }

                    yield return pair.Value as ObjPlayer;
                }
            }
        }

        public IEnumerable<ulong> EnumAllVisiblePlayerIdExclude(Zone _this, ulong excludeId = TypeDefine.INVALID_ULONG)
        {
            foreach (var zone in _this.VisibleZoneList)
            {
                foreach (var pair in zone.ObjDict)
                {
                    if (!pair.Value.Active)
                    {
                        continue;
                    }

                    if (pair.Value.GetObjType() != ObjType.PLAYER)
                    {
                        continue;
                    }

                    if (pair.Key == excludeId)
                    {
                        continue;
                    }

                    yield return pair.Key;
                }
            }
        }
#endif

        #region Buff缓存相关

        public void PushBuffMsg(Zone _this, BuffResultMsg temp)
        {
            if (_this.buffList.buff.Count < 1)
            {
                _this.buffList = temp;
            }
            else
            {
                foreach (var i in temp.buff)
                {
                    _this.buffList.buff.Add(i);
                }
            }
            if (_this.BuffTrigger == null)
            {
                _this.BuffTrigger =
                    SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddMilliseconds(ObjCharacter.BroadcastCd),
                        () => { BuffSend(_this); });
            }
        }

        public void BuffSend(Zone _this)
        {
            _this.BuffTrigger = null;
            Zone.totalBuff += _this.buffList.buff.Count;
            Zone.totalBuffSend += 1;
            SceneServer.Instance.ServerControl.SyncBuff(EnumAllVisiblePlayerIdExclude(_this), _this.buffList);
            _this.buffList.buff.Clear();
        }

        #endregion

        #region Skill缓存相关

        public void PushSkillMsg(Zone _this, CharacterUseSkillMsg temp)
        {
            _this.skillList.Skills.Add(temp);
            if (_this.SkillTrigger == null)
            {
                _this.SkillTrigger =
                    SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddMilliseconds(ObjCharacter.BroadcastCd),
                        () => { SkillSend(_this); });
            }
        }

        public void SkillSend(Zone _this)
        {
            _this.SkillTrigger = null;
            Zone.totalBuff += _this.skillList.Skills.Count;
            Zone.totalBuffSend += 1;
            SceneServer.Instance.ServerControl.NotifyUseSkillList(EnumAllVisiblePlayerIdExclude(_this), _this.skillList);
            _this.skillList.Skills.Clear();
        }

        #endregion

        #region Bullet缓存相关

        public void PushBulletMsg(Zone _this, BulletMsg temp)
        {
            _this.BulleList.Bullets.Add(temp);
            if (_this.BulletTrigger == null)
            {
                _this.BulletTrigger =
                    SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddMilliseconds(ObjCharacter.BroadcastCd),
                        () => { BulletSend(_this); });
            }
        }

        public void BulletSend(Zone _this)
        {
            _this.BulletTrigger = null;
            SceneServer.Instance.ServerControl.NotifyShootBulletList(EnumAllVisiblePlayerIdExclude(_this),
                _this.BulleList);
            _this.BulleList.Bullets.Clear();
        }

        #endregion

        #region MoveTo缓存相关

        public void PushMoveToMsg(Zone _this, CharacterMoveMsg temp)
        {
            _this.movetoList.Moves.Add(temp);
            if (_this.movetoTrigger == null)
            {
                _this.movetoTrigger =
                    SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddMilliseconds(ObjCharacter.BroadcastCd),
                        () => { MoveToSend(_this); });
            }
        }

        public void MoveToSend(Zone _this)
        {
            _this.movetoTrigger = null;
            SceneServer.Instance.ServerControl.SyncMoveToList(EnumAllVisiblePlayerIdExclude(_this), _this.movetoList);
            _this.movetoList.Moves.Clear();
        }

        #endregion

        #region CreateObj缓存相关

        public void PushCreateObj(Zone _this, ObjData temp)
        {
            _this.CreateObjList.Data.Add(temp);
            if (_this.CreateObjTrigger == null)
            {
                _this.CreateObjTrigger =
                    SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddMilliseconds(ObjCharacter.BroadcastCd),
                        () => { CreateObjSend(_this); });
            }
        }

        public void CreateObjSend(Zone _this)
        {
            _this.CreateObjTrigger = null;
            var list = EnumAllVisiblePlayerIdExclude(_this).ToArray();
            SceneServer.Instance.ServerControl.CreateObj(list, _this.CreateObjList);
            _this.CreateObjList.Data.Clear();
        }

        #endregion

        #region DeleteObj缓存相关

        public void PushDeleteObj(Zone _this, DeleteObjMsg temp)
        {
            _this.DeleteObjList.Datas.Add(temp);
            if (_this.DeleteObjTrigger == null)
            {
                _this.DeleteObjTrigger =
                    SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddMilliseconds(ObjCharacter.BroadcastCd),
                        () => { DeleteObjSend(_this); });
            }
        }

        public void DeleteObjSend(Zone _this)
        {
            _this.DeleteObjTrigger = null;
            var list = EnumAllVisiblePlayerIdExclude(_this).ToArray();
            SceneServer.Instance.ServerControl.DeleteObjList(list, _this.DeleteObjList);
            _this.DeleteObjList.Datas.Clear();
        }

        #endregion
    }

    public class Zone
    {
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static IZone mImpl;

        static Zone()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (Zone), typeof (ZoneDefaultImpl),
                o => { mImpl = (IZone) o; });
        }

        public int mId = -1;
        public float mOrginalX;
        public float mOrginalY;
        public float mWidth;
        public float mHeight;

        public Scene mScene;

        public int Id
        {
            get { return mId; }
        }

        public float Width
        {
            get { return mWidth; }
        }

        public float Height
        {
            get { return mHeight; }
        }

        public Dictionary<ulong, ObjBase> mObjDict = new Dictionary<ulong, ObjBase>();

        // public List<int>  SeenZoneList = new List<int>();
        public List<Zone> VisibleZoneList = new List<Zone>();

        public Dictionary<ulong, ObjBase> ObjDict
        {
            get { return mObjDict; }
            set { mObjDict = value; }
        }

        public bool mDirty = true;
        public bool mHasPlayer;

        public bool HasPlayerInAllVisibleZone
        {
            get
            {
                if (mDirty)
                {
                    mDirty = false;
                    mHasPlayer = false;
                    foreach (var player in EnumAllVisiblePlayer())
                    {
                        mHasPlayer = true;
                        break;
                    }
                }
                return mHasPlayer;
            }
        }

        public Zone(Scene scene, int id, float x, float y, float w, float h)
        {
            mScene = scene;
            mId = id;
            mOrginalX = x;
            mOrginalY = y;
            mWidth = w;
            mHeight = h;
        }

        public void Reset()
        {
            mObjDict.Clear();
        }

        public void AddVisibleZone(Zone zone)
        {
            mImpl.AddVisibleZone(this, zone);
        }

        public Vector2 GetInitPos()
        {
            return mImpl.GetInitPos(this);
        }

        public Vector2 GetCenterPos()
        {
            return mImpl.GetCenterPos(this);
        }

        public void MarkDirty()
        {
            mImpl.MarkDirty(this);
        }

        public void AddObj(ObjBase obj)
        {
            mImpl.AddObj(this, obj);
        }

        public void RemoveObj(ObjBase obj)
        {
            mImpl.RemoveObj(this, obj);
        }

#if ENUM_MODE
		public IEnumerable<ObjBase> EnumAllActiveObj()
		{
			foreach (var pair in ObjDict)
			{
				if(pair.Value.Active)
					yield return pair.Value;
			}
		}

		public IEnumerable<ObjBase> EnumAllVisibleObj()
		{
			foreach(var zone in VisibleZoneList)
			{
				foreach(var obj in zone.EnumAllActiveObj())
				{
					yield return obj;
				}
			}
		}		

		public IEnumerable<ObjPlayer> EnumAllVisiblePlayer()
		{
			foreach (var zone in VisibleZoneList)
			{
				foreach (var pair in zone.ObjDict)
				{
					if (!pair.Value.Active)
					{
						continue;
					}

					if (pair.Value.GetObjType() != ObjType.PLAYER)
					{
						continue;
					}

					yield return pair.Value as ObjPlayer;
				}
			}
		}

		public IEnumerable<ulong> EnumAllVisiblePlayerId()
		{
			foreach (var player in EnumAllVisiblePlayer())
			{
				yield return player.ObjId;
			}
		}

		public IEnumerable<ObjPlayer> EnumAllVisiblePlayerExclude(ulong excludeId = TypeDefine.INVALID_ULONG)
		{
			foreach(var player in EnumAllVisiblePlayer())
			{
				if (player.ObjId != excludeId)
				{
					yield return player;
				}
			}
		}

		public IEnumerable<ulong> EnumAllVisiblePlayerIdExclude(ulong excludeId = TypeDefine.INVALID_ULONG)
		{
			foreach (var player in EnumAllVisiblePlayerExclude(excludeId))
			{
				yield return player.ObjId;
			}
        }
#else
        public IEnumerable<ObjBase> EnumAllActiveObj()
        {
            return mImpl.EnumAllActiveObj(this);
        }

        public IEnumerable<ObjBase> EnumAllVisibleObj()
        {
            return mImpl.EnumAllVisibleObj(this);
        }

        public IEnumerable<ObjPlayer> EnumAllVisiblePlayer()
        {
            return mImpl.EnumAllVisiblePlayer(this);
        }

        public IEnumerable<ulong> EnumAllVisiblePlayerId()
        {
            return mImpl.EnumAllVisiblePlayerId(this);
        }

        public IEnumerable<ObjPlayer> EnumAllVisiblePlayerExclude(ulong excludeId = TypeDefine.INVALID_ULONG)
        {
            return mImpl.EnumAllVisiblePlayerExclude(this, excludeId);
        }

        public IEnumerable<ulong> EnumAllVisiblePlayerIdExclude(ulong excludeId = TypeDefine.INVALID_ULONG)
        {
            return mImpl.EnumAllVisiblePlayerIdExclude(this, excludeId);
        }
#endif

        #region Buff缓存相关

        public BuffResultMsg buffList = new BuffResultMsg();
        public object BuffTrigger;

        public void PushBuffMsg(BuffResultMsg temp)
        {
            mImpl.PushBuffMsg(this, temp);
        }

        public static int totalBuff;
        public static int totalBuffSend;

        public void BuffSend()
        {
            mImpl.BuffSend(this);
        }

        #endregion

        #region Skill缓存相关

        public CharacterUseSkillMsgList skillList = new CharacterUseSkillMsgList();
        public object SkillTrigger;

        public void PushSkillMsg(CharacterUseSkillMsg temp)
        {
            mImpl.PushSkillMsg(this, temp);
        }

        public void SkillSend()
        {
            mImpl.SkillSend(this);
        }

        #endregion

        #region Bullet缓存相关

        public BulletMsgList BulleList = new BulletMsgList();
        public object BulletTrigger;

        public void PushBulletMsg(BulletMsg temp)
        {
            mImpl.PushBulletMsg(this, temp);
        }

        public void BulletSend()
        {
            mImpl.BulletSend(this);
        }

        #endregion

        #region MoveTo缓存相关

        public CharacterMoveMsgList movetoList = new CharacterMoveMsgList();
        public object movetoTrigger;

        public void PushMoveToMsg(CharacterMoveMsg temp)
        {
            mImpl.PushMoveToMsg(this, temp);
        }

        public void MoveToSend()
        {
            mImpl.MoveToSend(this);
        }

        #endregion

        #region CreateObj缓存相关

        public CreateObjMsg CreateObjList = new CreateObjMsg();
        public object CreateObjTrigger;

        public void PushCreateObj(ObjData temp)
        {
            mImpl.PushCreateObj(this, temp);
        }

        public void CreateObjSend()
        {
            mImpl.CreateObjSend(this);
        }

        #endregion

        #region DeleteObj缓存相关

        public DeleteObjMsgList DeleteObjList = new DeleteObjMsgList();
        public object DeleteObjTrigger;

        public void PushDeleteObj(DeleteObjMsg temp)
        {
            mImpl.PushDeleteObj(this, temp);
        }

        public void DeleteObjSend()
        {
            mImpl.DeleteObjSend(this);
        }

        #endregion
    }
}