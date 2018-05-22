#region using

using System;
using DataContract;
using Mono.GameMath;

#endregion

namespace Scene
{
    public interface IObjRetinue
    {
        void AddEnemy(ObjRetinue _this, ulong objId);
        void Destroy(ObjRetinue _this);

        /// <summary>
        ///     消失
        /// </summary>
        void Disapeare(ObjRetinue _this);

        /// <summary>
        ///     输出成一个objdata用于客户端创建
        /// </summary>
        /// <returns></returns>
        ObjData DumpObjData(ObjRetinue _this, ReasonType reason);

        /// <summary>
        ///     获得等级，NPC不走属性那套，直接用表格的
        /// </summary>
        /// <returns></returns>
        int GetLevel(ObjRetinue _this);

        /// <summary>
        ///     初始化
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="id"></param>
        /// <param name="dataId"></param>
        /// <param name="owner"></param>
        void Init(ObjRetinue _this, ulong id, int dataId, ObjCharacter owner);

        bool InitAttr(ObjRetinue _this, int level);
        void InitData(ObjRetinue _this, int level);
        bool InitSkill(ObjRetinue _this, int level);

        /// <summary>
        ///     死亡时
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="characterId"></param>
        /// <param name="viewTime"></param>
        /// <param name="damage"></param>
        void OnDie(ObjRetinue _this, ulong characterId, int viewTime, int damage = 0);

        void OnOwnerMakeDamage(ObjRetinue _this, ObjCharacter target, int damage);
        void OnOwnerReceiveDamage(ObjRetinue _this, ObjCharacter enemy, int damage);

        /// <summary>
        ///     复活
        /// </summary>
		void Relive(ObjRetinue _this, bool byItem = false);

        /// <summary>
        ///     重新刷出(注意和复活的区别)
        /// </summary>
        void Respawn(ObjRetinue _this);

        /// <summary>
        /// Tick
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="delta"></param>
        void Tick(ObjRetinue _this, float delta);
    }

    public class ObjRetinueDefaultImpl : IObjRetinue
    {
        /// <summary>
        ///     初始化
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="id"></param>
        /// <param name="dataId"></param>
        /// <param name="owner"></param>
        public void Init(ObjRetinue _this, ulong id, int dataId, ObjCharacter owner)
        {
            _this.mObjId = id;
            _this.mTypeId = dataId;
            _this.Owner = owner;
            _this.mDirection = new Vector2(1, 0);
            _this.BuffList = new BuffList();
            _this.BuffList.InitByBase(_this);
            _this.Attr = new FightAttr(_this);
            _this.Skill = new SkillManager(_this);
            //InitTableData();
            //InitEquip();
            //InitSkill();
            //InitBuff();
            //InitAttr();
            //base.Init(id, dataId);
        }

        public void InitData(ObjRetinue _this, int level)
        {
            _this.InitTableData(level);
            _this.InitEquip(level);
            _this.InitSkill(level);
            _this.InitBuff(level);
            _this.InitAttr(level);

            _this.Script = NPCScriptRegister.CreateScriptInstance(_this.TableNpc.AIID);

            //mNextAction = DateTime.Now.AddSeconds(MyRandom.Random(MIN_PATROL_TIME, MAX_PATROL_TIME));

            if (null != _this.TableNpc && _this.TableNpc.HeartRate > 0)
            {
                _this.mAiTickSeconds = Math.Max(ObjNPC.MIN_AI_TICK_SECOND, _this.TableNpc.HeartRate*0.001f);
                _this.m_AITimer = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(_this.mAiTickSeconds),
                    () => { _this.Tick_AI(); }, (int) (_this.mAiTickSeconds*1000));
            }
        }

        public bool InitAttr(ObjRetinue _this, int level)
        {
            _this.Attr.InitAttributesAll();
            _this.Attr.InitAttributessAllEx(_this.Owner.Attr.mMonsterAttr);
            return true;
        }

        public bool InitSkill(ObjRetinue _this, int level)
        {
            foreach (var skillId in _this.TableCharacter.InitSkill)
            {
                if (-1 != skillId)
                {
                    _this.Skill.AddSkill(skillId, level, eAddskillType.InitSkillObjRetinue);
                }
            }
            return true;
        }

        /// <summary>
        ///     获得等级，NPC不走属性那套，直接用表格的
        /// </summary>
        /// <returns></returns>
        public int GetLevel(ObjRetinue _this)
        {
            return _this.Attr.GetDataValue(eAttributeType.Level);
        }

        /// <summary>
        ///     死亡时
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="characterId"></param>
        /// <param name="viewTime"></param>
        /// <param name="damage"></param>
        public void OnDie(ObjRetinue _this, ulong characterId, int viewTime, int damage = 0)
        {
            _this.IsSendDie = true;
            ObjNPC.GetImpl().OnDie(_this, characterId, viewTime, damage);
            if (_this.Buff != null && _this.Owner != null)
            {
                _this.Owner.DeleteBuff(_this.Buff, eCleanBuffType.RetinueDie);
            }
        }

        /// <summary>
        ///     消失
        /// </summary>
        public void Disapeare(ObjRetinue _this)
        {
            if (null != _this.Owner)
            {
                _this.Owner.mRetinues.Remove(_this);
            }
            ObjNPC.GetImpl().Disapeare(_this);
        }

        /// <summary>
        ///     重新刷出(注意和复活的区别)
        /// </summary>
        public void Respawn(ObjRetinue _this)
        {
            ObjNPC.GetImpl().Respawn(_this);
        }

        /// <summary>
        ///     复活
        /// </summary>
		public void Relive(ObjRetinue _this, bool byItem = false)
        {
            ObjNPC.GetImpl().Relive(_this,byItem);
        }

        /// <summary>
        ///     输出成一个objdata用于客户端创建
        /// </summary>
        /// <returns></returns>
        public ObjData DumpObjData(ObjRetinue _this, ReasonType reason)
        {
            var data = ObjCharacter.GetImpl().DumpObjData(_this, reason);
            data.Owner = new Uint64Array();
            data.Owner.Items.Add(_this.Owner.ObjId);
            return data;
        }

        public virtual void OnOwnerReceiveDamage(ObjRetinue _this, ObjCharacter enemy, int damage)
        {
            if (null == enemy)
            {
                return;
            }

            if (enemy.ObjId == _this.ObjId || enemy.ObjId == _this.Owner.ObjId)
            {
                return;
            }

            _this.AddEnemy(enemy.ObjId);
            _this.EnterState(BehaviorState.Combat);
        }

        public void AddEnemy(ObjRetinue _this, ulong objId)
        {
            if (objId == _this.ObjId || objId == _this.Owner.ObjId)
            {
                return;
            }

            if (!_this.EnemyList.Contains(objId))
            {
                _this.EnemyList.Add(objId);
            }
        }

        public virtual void OnOwnerMakeDamage(ObjRetinue _this, ObjCharacter target, int damage)
        {
            if (null == target)
            {
                return;
            }

            if (target.ObjId == _this.ObjId || target.ObjId == _this.Owner.ObjId)
            {
                return;
            }

            _this.AddEnemy(target.ObjId);
            _this.EnterState(BehaviorState.Combat);
        }

        public void Destroy(ObjRetinue _this)
        {
            if (null != _this.Owner)
            {
                _this.Owner.mRetinues.Remove(_this);
            }
            ObjNPC.GetImpl().Destroy(_this);
        }

        public void Tick(ObjRetinue _this, float delta)
        {
            ObjCharacter.GetImpl().Tick(_this, delta);

            if (0 == _this.mLogicTickCount % 10)
            {
                if (_this.Owner == null)
                {
                    _this.Scene.LeaveScene(_this);
                    return;
                }
                var rePos = _this.GetPosition();
                var ownerPos = _this.Owner.GetPosition();
                var distance = (ownerPos - rePos).LengthSquared();
                if (distance > 64)
                {
                    _this.EnterState(BehaviorState.Idle);
                    _this.SetPosition(ownerPos);
                    _this.SyncCharacterPostion();
                }
            }
        }

    }

    public class ObjRetinue : ObjNPC
    {
        private static IObjRetinue mImpl;

        static ObjRetinue()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (ObjRetinue), typeof (ObjRetinueDefaultImpl),
                o => { mImpl = (IObjRetinue) o; });
        }

        public override void Tick(float delta)
        {
            mImpl.Tick(this, delta);
        }

        public override void AddEnemy(ulong objId)
        {
            mImpl.AddEnemy(this, objId);
        }

        public override void Destroy()
        {
            mImpl.Destroy(this);
        }

        /// <summary>
        ///     消失
        /// </summary>
        public override void Disapeare()
        {
            mImpl.Disapeare(this);
        }

        /// <summary>
        ///     输出成一个objdata用于客户端创建
        /// </summary>
        /// <returns></returns>
        public override ObjData DumpObjData(ReasonType reason)
        {
            return mImpl.DumpObjData(this, reason);
        }

        /// <summary>
        ///     获得等级，NPC不走属性那套，直接用表格的
        /// </summary>
        /// <returns></returns>
        public override int GetLevel()
        {
            return mImpl.GetLevel(this);
        }

        //Obj类型
        public override ObjType GetObjType()
        {
            return ObjType.RETINUE;
        }

        //构造函数

        /// <summary>
        ///     初始化
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dataId"></param>
        /// <param name="owner"></param>
        public void Init(ulong id, int dataId, ObjCharacter owner)
        {
            mImpl.Init(this, id, dataId, owner);
        }

        public override bool InitAttr(int level)
        {
            return mImpl.InitAttr(this, level);
        }

        public void InitData(int level)
        {
            mImpl.InitData(this, level);
        }

        public override bool InitSkill(int level)
        {
            return mImpl.InitSkill(this, level);
        }

        /// <summary>
        ///     死亡时
        /// </summary>
        /// <param name="characterId"></param>
        /// <param name="viewTime"></param>
        /// <param name="damage"></param>
        public override void OnDie(ulong characterId, int viewTime, int damage = 0)
        {
            mImpl.OnDie(this, characterId, viewTime, damage);
        }

        public virtual void OnOwnerMakeDamage(ObjCharacter target, int damage)
        {
            mImpl.OnOwnerMakeDamage(this, target, damage);
        }

        public virtual void OnOwnerReceiveDamage(ObjCharacter enemy, int damage)
        {
            mImpl.OnOwnerReceiveDamage(this, enemy, damage);
        }

        /// <summary>
        ///     复活
        /// </summary>
		public override void Relive(bool byItem = false)
        {
            mImpl.Relive(this,byItem);
        }

        /// <summary>
        ///     重新刷出(注意和复活的区别)
        /// </summary>
        public override void Respawn()
        {
            mImpl.Respawn(this);
        }

        #region 逻辑数据

        public BuffData Buff;
        public ObjCharacter Owner;
        public bool IsSendDie;

        #endregion
    }
}