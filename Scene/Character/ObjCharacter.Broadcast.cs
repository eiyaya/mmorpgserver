#region using

using DataContract;
using Shared;

#endregion

namespace Scene
{
    public partial class ObjCharacterDefaultImpl
    {
        public int GetBroadcastCd()
        {
            return 100;
        }

        //使用技能广播
        public void BroadcastUseSkill(ObjCharacter _this, int skillId, ObjCharacter obj)
        {
            if (!_this.mActive)
            {
                return;
            }
            if (null == _this.Zone)
            {
                return;
            }
            ulong targetId = 0;
            if (obj != null)
            {
                targetId = obj.ObjId;
            }
            var msg = new CharacterUseSkillMsg();
            msg.CharacterId = _this.ObjId;
            msg.SkillId = skillId;
            msg.TargetObjId.Add(targetId);
            msg.Pos = new PositionData
            {
                Pos = new Vector2Int32
                {
                    x = Utility.MultiplyPrecision(_this.GetPosition().X),
                    y = Utility.MultiplyPrecision(_this.GetPosition().Y)
                },
                Dir = new Vector2Int32
                {
                    x = Utility.MultiplyPrecision(_this.GetDirection().X),
                    y = Utility.MultiplyPrecision(_this.GetDirection().Y)
                }
            };

            if (ObjCharacter.BroadcastType == 1)
            {
                _this.Zone.PushSkillMsg(msg);
                return;
            }
            SceneServer.Instance.ServerControl.NotifyUseSkill(_this.EnumAllVisiblePlayerIdExclude(), msg);
        }

        //广播移动
        public void BroadcastMoveTo(ObjCharacter _this)
        {
            if (!_this.mActive)
            {
                return;
            }
            if (null == _this.Zone)
            {
                return;
            }
            var msg = new CharacterMoveMsg
            {
                ObjId = _this.ObjId
            };

            foreach (var pos in _this.mTargetPos)
            {
                msg.TargetPos.Add(new Vector2Int32
                {
                    x = Utility.MultiplyPrecision(pos.X),
                    y = Utility.MultiplyPrecision(pos.Y)
                });
            }
            if (ObjCharacter.BroadcastMoveToType == 1)
            {
                _this.Zone.PushMoveToMsg(msg);
                return;
            }
            SceneServer.Instance.ServerControl.SyncMoveTo(_this.EnumAllVisiblePlayerIdExclude(_this.ObjId), msg);
        }

        //广播泡泡说话，如果字典id不为空，就说字典，如果为空，就说字符串
        public void BroadcastSpeak(ObjCharacter _this, int dictId, string content)
        {
            if (!_this.mActive)
            {
                return;
            }
            if (null == _this.Zone)
            {
                return;
            }

            SceneServer.Instance.ServerControl.ObjSpeak(_this.EnumAllVisiblePlayerIdExclude(), _this.ObjId, dictId,
                content);
        }

        //广播移动
        public void BroadcastStopMove(ObjCharacter _this)
        {
            if (!_this.mActive)
            {
                return;
            }
            if (null == _this.Zone)
            {
                return;
            }
            var msg = new SyncPostionMsg();
            msg.ObjId = _this.ObjId;
            msg.Pos = new PositionData
            {
                Pos = new Vector2Int32
                {
                    x = Utility.MultiplyPrecision(_this.GetPosition().X),
                    y = Utility.MultiplyPrecision(_this.GetPosition().Y)
                },
                Dir = new Vector2Int32
                {
                    x = Utility.MultiplyPrecision(_this.GetDirection().X),
                    y = Utility.MultiplyPrecision(_this.GetDirection().Y)
                }
            };
            SceneServer.Instance.ServerControl.SyncStopMove(_this.EnumAllVisiblePlayerIdExclude(_this.ObjId), msg);
        }

        //广播自己位置
        public void BroadcastSelfPostion(ObjCharacter _this)
        {
            if (!_this.mActive)
            {
                return;
            }
            if (null == _this.Zone)
            {
                return;
            }
            var msg = new SyncPostionMsg();
            msg.ObjId = _this.ObjId;
            msg.Pos = new PositionData
            {
                Pos = new Vector2Int32
                {
                    x = Utility.MultiplyPrecision(_this.GetPosition().X),
                    y = Utility.MultiplyPrecision(_this.GetPosition().Y)
                },
                Dir = new Vector2Int32
                {
                    x = Utility.MultiplyPrecision(_this.GetDirection().X),
                    y = Utility.MultiplyPrecision(_this.GetDirection().Y)
                }
            };

            SceneServer.Instance.ServerControl.SyncStopMove(_this.EnumAllVisiblePlayerIdExclude(), msg);
        }

        //Buff效果广播
        public void BroadcastBuffList(ObjCharacter _this, BuffResultMsg msg)
        {
            if (!_this.mActive)
            {
                return;
            }
            if (null == _this.Zone)
            {
                return;
            }
			if(msg.buff.Count<=0)
			{
				return;
			}
            if (ObjCharacter.BroadcastType == 1)
            {
                _this.Zone.PushBuffMsg(msg);
                return;
            }
            SceneServer.Instance.ServerControl.SyncBuff(_this.EnumAllVisiblePlayerIdExclude(), msg);
        }

        //子弹广播
        public void BroadcastShootBullet(ObjCharacter _this, int bulletId, ulong casterId, ulong targetId, int delayView)
        {
            if (!_this.mActive)
            {
                return;
            }
            if (null == _this.Zone)
            {
                return;
            }
            var msg = new BulletMsg
            {
                BulletId = bulletId,
                CasterId = casterId
            };
            if (delayView > 0)
            {
                msg.ViewTime = Extension.AddTimeDiffToNet(delayView);
            }
            msg.TargetObjId.Add(targetId);
            if (ObjCharacter.BroadcastType == 1)
            {
                _this.Zone.PushBulletMsg(msg);
                return;
            }
            SceneServer.Instance.ServerControl.NotifyShootBullet(_this.EnumAllVisiblePlayerIdExclude(), msg);
        }

        //广播装备模型修改
        public void BroadcastChangeEquipModel(ObjCharacter _this, ulong casterId, int nPart, int EquipId)
        {
            if (!_this.mActive)
            {
                return;
            }
            if (null == _this.Zone)
            {
                return;
            }
            SceneServer.Instance.ServerControl.NotifyEquipChanged(_this.EnumAllVisiblePlayerIdExclude(), casterId, nPart,
                EquipId);
        }

        //广播方向
        public void BroadcastDirection(ObjCharacter _this)
        {
            if (!_this.mActive)
            {
                return;
            }
            if (null == _this.Zone)
            {
                return;
            }
            SceneServer.Instance.ServerControl.SyncDirection(_this.EnumAllVisiblePlayerIdExclude(_this.ObjId),
                _this.ObjId, Utility.MultiplyPrecision(_this.GetDirection().X),
                Utility.MultiplyPrecision(_this.GetDirection().Y));
        }
    }

    public partial class ObjCharacter : ObjBase
    {
        public static int BroadcastMoveToType = 0;
        public static int BroadcastType = 1;

        public static int BroadcastCd
        {
            get { return mImpl.GetBroadcastCd(); }
        }

        //Buff效果广播
        public virtual void BroadcastBuffList(BuffResultMsg msg)
        {
            mImpl.BroadcastBuffList(this, msg);
        }

        //广播装备模型修改
        public virtual void BroadcastChangeEquipModel(ulong casterId, int nPart, int EquipId)
        {
            mImpl.BroadcastChangeEquipModel(this, casterId, nPart, EquipId);
        }

        //广播方向
        public virtual void BroadcastDirection()
        {
            mImpl.BroadcastDirection(this);
        }

        //广播移动
        public virtual void BroadcastMoveTo()
        {
            mImpl.BroadcastMoveTo(this);
        }

        //广播自己位置
        public virtual void BroadcastSelfPostion()
        {
            mImpl.BroadcastSelfPostion(this);
        }

        //子弹广播
        public virtual void BroadcastShootBullet(int bulletId, ulong casterId, ulong targetId, int delayView)
        {
            mImpl.BroadcastShootBullet(this, bulletId, casterId, targetId, delayView);
        }

        //广播泡泡说话，如果字典id不为空，就说字典，如果为空，就说字符串
        public void BroadcastSpeak(int dictId, string content)
        {
            mImpl.BroadcastSpeak(this, dictId, content);
        }

        //广播移动
        public virtual void BroadcastStopMove()
        {
            mImpl.BroadcastStopMove(this);
        }

        //使用技能广播
        public virtual void BroadcastUseSkill(int skillId, ObjCharacter obj)
        {
            mImpl.BroadcastUseSkill(this, skillId, obj);
        }
    }
}