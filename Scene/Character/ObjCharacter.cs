#region using

using System;
using System.Collections.Generic;
using System.IO;
using DataContract;
using DataTable;
using Scorpion;
using Mono.GameMath;
using NLog;
using ProtoBuf;

using Shared;

#endregion

namespace Scene
{
    public class NormalAttr : ObjCharacter.INotifyPropertyChanged
    {
        private eAreaState mAreaState = eAreaState.City;
		private ulong mTargetCharacter = ulong.MaxValue;
        public eAreaState AreaState
        {
            get { return mAreaState; }
            set
            {
                if (mAreaState == value)
                {
                    return;
                }

                mAreaState = value;
                OnPropertyChanged((uint) eSceneSyncId.SyncAreaState);
            }
        }

		public ulong TargetCharacter
		{
			get { return mTargetCharacter; }
			set
			{
				if (mTargetCharacter == value)
				{
					return;
				}

				mTargetCharacter = value;
				OnPropertyChanged((uint)eSceneSyncId.SyncTargetCharacterId);
			}
		}
        public virtual void OnPropertyChanged(uint propertyId)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new ObjCharacter.PropertyChangedEventArgs(propertyId));
            }
        }

        public event ObjCharacter.PropertyChangedEventHandler PropertyChanged;
    }


    public interface IObjCharacter
    {
        BuffData AddBuff(ObjCharacter _this, BuffRecord tbBuff, int bufflevel, ObjCharacter casterHero);

        /// 获得Buff时
        /// <summary>
        ///     增加Buff
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="buffid">BuffId</param>
        /// <param name="bufflevel"></param>
        /// <param name="casterHero">施放者</param>
        /// <param name="DelayView"></param>
        /// <param name="hitType">命中类型</param>
        /// <param name="fBili">Buff的修正比例</param>
        /// <param name="buffLastTime">Buff持续时间 部分buff的持续时间不读buff表</param>
        /// <returns></returns>
        BuffData AddBuff(ObjCharacter _this,
                         int buffid,
                         int bufflevel,
                         ObjCharacter casterHero,
                         int DelayView = 0,
                         eHitType hitType = eHitType.Hit,
                         float fBili = 1.0f,
                         double buffLastTime = 0
            );

        void AddEnemy(ObjCharacter _this, ulong objId);
        void AddEquip(ObjCharacter _this, int nPart, ItemEquip2 equip);
        void AddExdata(ObjCharacter _this, int eId, int value);

        void AddSyncData(ObjCharacter objCharacter,
                         uint id,
                         ObjCharacter.INotifyPropertyChanged holder,
                         Func<byte[]> getter);

        void BroadcastBuffList(ObjCharacter _this, BuffResultMsg msg);
        void BroadcastChangeEquipModel(ObjCharacter _this, ulong casterId, int nPart, int EquipId);
        void BroadcastDirection(ObjCharacter _this);
        void BroadcastMoveTo(ObjCharacter _this);
        void BroadcastSelfPostion(ObjCharacter _this);
        void BroadcastShootBullet(ObjCharacter _this, int bulletId, ulong casterId, ulong targetId, int delayView);
        void BroadcastSpeak(ObjCharacter _this, int dictId, string content);
        void BroadcastStopMove(ObjCharacter _this);
        void BroadcastUseSkill(ObjCharacter _this, int skillId, ObjCharacter obj);
        bool CanMove(ObjCharacter _this);
        bool CanSkill(ObjCharacter _this);
        ErrorCodes CheckAddBuff(ObjCharacter _this, int buffid, int bufflevel, ObjCharacter casterHero);

        /// <summary>
        ///     死亡检测
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="buff">源于Buff</param>
        /// <param name="caster">造成者</param>
        /// <param name="delayView"></param>
        /// <param name="damage">当次伤害</param>
        /// <returns>真的死亡了没</returns>
        bool CheckDie(ObjCharacter _this, BuffData buff, ObjCharacter caster, int delayView, int damage);

        ErrorCodes CheckUseSkill(ObjCharacter _this, ref int skillId, ObjCharacter target = null);
        void CleanBullet(ObjCharacter _this);
        void ClearEnemy(ObjCharacter _this);

        ObjRetinue CreateRetinue(ObjCharacter _this,
                                 int dataId,
                                 int level,
                                 Vector2 pos,
                                 Vector2 dir,
                                 int camp,
                                 bool isNeedAdd = true);

        void DelEquip(ObjCharacter _this, int nPart, bool refreshAttr = true);

        void AddEquipBuff(ObjCharacter _this, ItemEquip2 equip);
        void RemoveEquipBuff(ObjCharacter _this, ItemEquip2 equip);
        void RemoveCantMoveBuff(ObjCharacter _this);

        /// 删除Buff
        /// <summary>
        ///     删除Buff (知道Buff实例)
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="deletebuff">Buff实例</param>
        /// <param name="type">删除的原因类型</param>
        void DeleteBuff(ObjCharacter _this, BuffData deletebuff, eCleanBuffType type);

        /// 删除Buff
        /// <summary>
        ///     删除Buff (知道BuffId)
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="nBuffId">BuffId</param>
        /// <param name="type">删除的原因类型</param>
        void DeleteBuff(ObjCharacter _this, int nBuffId, eCleanBuffType type);

        void Destroy(ObjCharacter _this);
        void Die(ObjCharacter _this, ulong characterId, int delayView, int damage = 0);

        /// <summary>
        ///     造成伤害
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="hitType">命中类型</param>
        /// <param name="buff">来源Buff</param>
        /// <param name="caster">造成者</param>
        /// <param name="delayView"></param>
        /// <param name="damage">伤害量</param>
        /// <param name="damageType">伤害类型</param>
        /// <param name="absorbDamage">吸收伤害</param>
        void DoDamage(ObjCharacter _this,
                      eHitType hitType,
                      BuffData buff,
                      ObjCharacter caster,
                      int delayView,
                      ref int damage,
                      int damageType,
                      ref int absorbDamage);

        /// <summary>
        ///     造成治疗
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="hitType">命中类型</param>
        /// <param name="buff">来源Buff</param>
        /// <param name="caster">造成者</param>
        /// <param name="health">治疗量</param>
        /// <param name="healthType">治疗类型</param>
        /// <param name="IsToClient"></param>
        void DoHealth(ObjCharacter _this,
                      eHitType hitType,
                      BuffData buff,
                      ObjCharacter caster,
                      ref int health,
                      int healthType,
                      bool IsToClient = false);

        /// <summary>
        ///     造成回蓝
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="hitType">命中类型</param>
        /// <param name="buff">来源Buff</param>
        /// <param name="caster">造成者</param>
        /// <param name="mana">治疗量</param>
        /// <param name="healthType">治疗类型</param>
        /// <param name="IsToClient"></param>
        void DoMana(ObjCharacter _this,
                    eHitType hitType,
                    BuffData buff,
                    ObjCharacter caster,
                    ref int mana,
                    int healthType,
                    bool IsToClient = false);

        /// <summary>
        ///     造成伤害
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="hitType">命中类型</param>
        /// <param name="buff">来源Buff</param>
        /// <param name="caster">造成者</param>
        /// <param name="delayView"></param>
        /// <param name="damage">伤害量</param>
        /// <param name="damageType">伤害类型</param>
        void DoRealDamage(ObjCharacter _this,
                          eHitType hitType,
                          BuffData buff,
                          ObjCharacter caster,
                          int delayView,
                          ref int damage,
                          int damageType);

        ObjData DumpObjData(ObjCharacter _this, ReasonType reason);

        void EquipModelStateChange(ObjCharacter _this, int nPart, int nState, ItemBaseData equip);
        void EquipChange(ObjCharacter _this, int nType, int nPart, ItemBaseData equip);
        void EquipSkill(ObjCharacter _this, List<int> dels, List<int> adds, List<int> lvls);

        /// <summary>
        ///     强制移动到目标点
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="pos">目标位置</param>
        /// <param name="speed">移动速度</param>
        void ForceMoveTo(ObjCharacter _this, Vector2 pos, float speed);

        int GetAttribute(ObjCharacter _this, eAttributeType nIndex);
        int GetBroadcastCd();
        int GetCamp(ObjCharacter _this);
        ItemEquip2 GetEquip(ObjCharacter _this, int nPart);
        void GetEquipsModel(ObjCharacter _this, Dictionary<int, int> equipsModel);
        int GetExdata(ObjCharacter _this, int eId);
        ulong GetLastEnemyId(ObjCharacter _this);
        int GetLevel(ObjCharacter _this);
        float GetMoveSpeed(ObjCharacter _this);
        string GetName(ObjCharacter _this);
        List<ObjRetinue> GetRetinueList(ObjCharacter _this);
        ObjCharacter GetRewardOwner(ObjCharacter _this);
        ulong GetTeamId(ObjCharacter _this);
        void Init(ObjCharacter _this, ulong characterId, int dataId, int level);
        bool InitAttr(ObjCharacter _this, int level);
        bool InitBuff(ObjCharacter _this, int level);
        bool InitEquip(ObjCharacter _this, int level);
        bool InitSkill(ObjCharacter _this, int level);
        int InitTableData(ObjCharacter _this, int level);
        bool IsDead(ObjCharacter _this);
        bool IsHaveRetinue(ObjCharacter _this, ObjRetinue obj);
        bool IsInvisible(ObjCharacter _this);
        bool IsMoving(ObjCharacter _this);
        bool IsRiding(ObjCharacter _this);
        bool IsMyEnemy(ObjCharacter _this, ObjCharacter character);
        bool IsVisibleTo(ObjCharacter _this, ObjBase obj);
        bool Move(ObjCharacter _this, List<Vector2> path);

        /// <summary>
        ///     移动角色（寻路是异步的）
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="pos">目标位置</param>
        /// <param name="offset">目标位置偏移</param>
        /// <param name="searchPath">是否需要寻路，false表示直线移动到目的地</param>
        /// <param name="pushLastPos">是否需要精确移动到目标点，因为寻路是以格子中心点为坐标的，最后可能会产生半格的误差</param>
        /// <param name="callback">寻路结果callback</param>
        /// <returns>能走返回true，不能走返回false</returns>
        MoveResult MoveTo(ObjCharacter _this,
                          Vector2 pos,
                          float offset = 0.05f,
                          bool searchPath = true,
                          bool pushLastPos = false,
                          Action<List<Vector2>> callback = null);

        ErrorCodes MustSkill(ObjCharacter _this, ref int skillId, ObjCharacter target = null);
        void OnDamage(ObjCharacter _this, ObjCharacter enemy, int damage);
		void OnTrapped(ObjCharacter _this, ObjCharacter enemy);
        void OnDie(ObjCharacter _this, ulong characterId, int delayView, int damage = 0);
        void OnEnemyDie(ObjCharacter _this, ObjCharacter obj);
        void OnEnterScene(ObjCharacter _this);
        void OnLeaveScene(ObjCharacter _this);
        void OnlineDie(ObjCharacter _this);
        void OnMakeDamageTo(ObjCharacter _this, ObjCharacter target, int damage);
        void OnMoveBegin(ObjCharacter _this);
        void OnMoveEnd(ObjCharacter _this);
        bool OnSyncRequested(ObjCharacter _this, ulong characterId, uint syncId);
        void OnUseSkill(ObjCharacter _this, int skillId, ObjCharacter target);
        void OverTime(ObjCharacter _this, int eId);
        void ProcessPositionChanged(ObjCharacter _this);
        void PushBullet(ObjCharacter _this, Bullet b);
        void RegisterAllSyncData(ObjCharacter _this);
        void Relive(ObjCharacter _this,bool byItem = false);
        void RemoveAllSyncData(ObjCharacter _this);
        void RemoveBullet(ObjCharacter _this, Bullet b);
        void RemoveEnemy(ObjCharacter _this, ulong objId);
        void RemoveMeFromOtherEnemyList(ObjCharacter _this);
        void RemoveRetinue(ObjCharacter _this, ObjRetinue retinue);
        ErrorCodes RequestUseSkill(ObjCharacter _this, int skillId, List<int> skillIds, ObjCharacter target = null);
        void Reset(ObjCharacter _this);
        void ResetAttribute(ObjCharacter _this);
        void ResetEquip(ObjCharacter _this, int nPart, ItemEquip2 equip);
        void RetinueAttack(ObjCharacter _this);
        void ClearRetinue(ObjCharacter _this);
        void SetCamp(ObjCharacter _this, int camp);
        void SetCheckPostion(ObjCharacter _this, Vector2 pos);
        void SetExdata(ObjCharacter _this, int eId, int value);
        void SetFlag(ObjCharacter _this, int idx, bool b);
        bool GetFlag(ObjCharacter _this, int idx);

        /// <summary>
        ///     设置等级
        /// </summary>
        /// <returns></returns>
        void SetLevel(ObjCharacter _this, int nValue);

        void SetMove(ObjCharacter _this, int MoveTime);
        void SetName(ObjCharacter _this, string name);
        void SetPosition(ObjCharacter _this, Vector2 p);
        void SetPosition(ObjCharacter _this, float x, float y);
        void SetSkill(ObjCharacter _this, int SkillTime);
        void SetTeamId(ObjCharacter _this, ulong teamId, int state);
        void SetToLevel(ObjCharacter _this, int lv);
        void SkillChange(ObjCharacter _this, int nType, int nId, int nLevel);
        void StartTime(ObjCharacter _this, int eId);
        void StopMove(ObjCharacter _this, bool broadcast = true);
        void SyncCharacterPostion(ObjCharacter _this);
        void TalentChange(ObjCharacter _this, int nType, int nId, int nLayer);
        void Tick(ObjCharacter _this, float delta);
        void Tick_Move(ObjCharacter _this, float delta);
        void TurnFaceTo(ObjCharacter _this, Vector2 pos);
        void UpdateDbAttribute(ObjCharacter _this, eAttributeType type);
        void UpdateTriggerArea(ObjCharacter _this);
        void UpdateZone(ObjCharacter _this);
        ErrorCodes UseSkill(ObjCharacter _this, ref int skillId, ObjCharacter target = null);
        void ElfChange(ObjCharacter _this, List<int> removeBuff, Dictionary<int, int> addBuff, int fightPoint);
		int GetElfSkillFightPoint(ObjCharacter _this);
        void AddElfBuff(ObjCharacter _this, int buffId, int buffLevel);
        void RemoveElfBuff(ObjCharacter _this, int buffId);
		ulong GetTargetCharacterId(ObjCharacter _this);
		void SetTargetCharacterId(ObjCharacter _this, ulong id);

        void RideMount(ObjCharacter _this, int mountId);

		bool CanBeAttacked(ObjCharacter _this);
    }

    public partial class ObjCharacterDefaultImpl : IObjCharacter
    {
        public static Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     强制移动到目标点
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="pos">目标位置</param>
        /// <param name="speed">移动速度</param>
        public void ForceMoveTo(ObjCharacter _this, Vector2 pos, float speed)
        {
            if (_this.Scene == null)
            {
                return;
            }

            _this.Scene.Raycast(_this, pos, p =>
            {
                var dir = p - _this.GetPosition();
                _this.mForceMovingTime = dir.Length()/speed;
                dir.Normalize();
                _this.mForceMoveSpeed = dir*speed;
                _this.mIsForceMoving = true;
            });
        }

        //输出成一个objdata用于客户端创建
        public ObjData DumpObjData(ObjCharacter _this, ReasonType reason)
        {
            var data = ObjBase.GetImpl().DumpObjData(_this, reason);
            data.Name = _this.GetName();
            data.IsDead = _this.IsDead();
            data.Camp = _this.GetCamp();
            data.Movspeed = _this.GetMoveSpeed();
            data.IsMoving = _this.IsMoving();
            data.Level = _this.Attr.GetDataValue(eAttributeType.Level);
            data.MpMax = _this.Attr.GetDataValue(eAttributeType.MpMax);
            data.MpMow = _this.Attr.GetDataValue(eAttributeType.MpNow);

            var hpMax = _this.Attr.GetDataValue(eAttributeType.HpMax);
            var hpNow = _this.Attr.GetDataValue(eAttributeType.HpNow);
            if (_this.Scene != null && _this.Scene.isNeedDamageModify)
            {
                if (Scene.IsNeedChangeHp(_this) != null)
                {
                    hpNow = (int) (hpNow/_this.Scene.BeDamageModify);
                    hpMax = (int) (hpMax/_this.Scene.BeDamageModify);
                }
            }
            data.HpMax = hpMax;
            data.HpNow = hpNow;
            foreach (var pos in _this.mTargetPos)
            {
                data.TargetPos.Add(new Vector2Int32
                {
                    x = Utility.MultiplyPrecision(pos.X),
                    y = Utility.MultiplyPrecision(pos.Y)
                });
            }
            data.ModelId = _this.ModelId;

            if (_this.GetTargetCharacterId() != ulong.MaxValue && null != _this.Scene)
			{
				var player = _this.Scene.FindPlayer(_this.GetTargetCharacterId());
				if (null != player)
				{
                    data.TargetInfo = player.GetName() + "|" + player.TypeId + "|" + player.Attr.Ladder;//51.转生ID
				}
			}
            return data;
        }

        //停止移动
        public virtual void StopMove(ObjCharacter _this, bool broadcast = true)
        {
            if (_this.mIsMoving)
            {
                _this.mIsMoving = false;
                _this.mWaitingToMove = false;
                _this.mTargetPos.Clear();

                _this.OnMoveEnd();

                if (broadcast)
                {
                    _this.BroadcastStopMove();
                }
            }
        }

        //是否是我的敌方
        public virtual bool IsMyEnemy(ObjCharacter _this, ObjCharacter character)
        {
            if (null == _this.TableCamp)
            {
                return false;
            }
			if (!character.CanBeAttacked())
			{
				return false;	
			}
            if (character.GetCamp() < 0 || character.GetCamp() >= _this.TableCamp.Camp.Length)
            {
                return false;
            }

            return 1 == _this.TableCamp.Camp[character.GetCamp()];
        }

        //是否正在移动
        public virtual bool IsMoving(ObjCharacter _this)
        {
            return _this.mIsMoving || _this.mWaitingToMove;
        }

        public virtual bool IsRiding(ObjCharacter _this)
        {
            return false;
        }

        public virtual void UpdateDbAttribute(ObjCharacter _this, eAttributeType type)
        {
        }

        //是否无敌免疫Buff
        public virtual bool IsInvisible(ObjCharacter _this)
        {
            return false;
        }

        //我是否能被其他obj看见
        public bool IsVisibleTo(ObjCharacter _this, ObjBase obj)
        {
            if (_this.IsDead() || obj.GetObjType() != ObjType.PLAYER)
            {
                return false;
            }
            return true;
        }

        public void Destroy(ObjCharacter _this)
        {
        }

        #region 属性技能数据

        public void OverTime(ObjCharacter _this, int eId)
        {
            Trigger tr;
            if (_this.SkillExdataReset.TryGetValue(eId, out tr))
            {
                SceneServerControl.Timer.DeleteTrigger(tr);
                _this.SkillExdataReset.Remove(eId);
            }
        }

        public void StartTime(ObjCharacter _this, int eId)
        {
            Trigger tr;
            if (_this.SkillExdataReset.TryGetValue(eId, out tr))
            {
                SceneServerControl.Timer.ChangeTime(ref tr, DateTime.Now.AddMilliseconds(1500));
            }
            else
            {
                _this.SkillExdataReset[eId] = SceneServerControl.Timer.CreateTrigger(
                    DateTime.Now.AddMilliseconds(1500), () =>
                    {
                        _this.OverTime(eId);
                        _this.SkillExdata[eId] = 0;
                    });
            }
        }

        public int GetExdata(ObjCharacter _this, int eId)
        {
            int v;
            if (_this.SkillExdata.TryGetValue(eId, out v))
            {
                return v;
            }
            return 0;
        }

        public void SetExdata(ObjCharacter _this, int eId, int value)
        {
            if (value <= 0)
            {
                _this.SkillExdata[eId] = 0;
                _this.OverTime(eId);
                return;
            }
            _this.SkillExdata[eId] = value;
            _this.StartTime(eId);
        }

        public void SetFlag(ObjCharacter _this, int idx, bool b)
        {
            if (_this.FlagData.ContainsKey(idx))
            {
                _this.FlagData[idx] = b;
            }
            else
            {
                _this.FlagData.Add(idx,b);
            }
        }

        public bool GetFlag(ObjCharacter _this, int idx)
        {
            bool b = false;
            _this.FlagData.TryGetValue(idx, out b);
            return b;
        }

        public void AddExdata(ObjCharacter _this, int eId, int value)
        {
            int v;
            if (_this.SkillExdata.TryGetValue(eId, out v))
            {
                _this.SetExdata(eId, v + value);
            }
            else
            {
                _this.SetExdata(eId, value);
            }
        }

        public void GetEquipsModel(ObjCharacter _this, Dictionary<int, int> equipsModel)
        {
            foreach (var bagid in EquipExtension.EquipModelBagId)
            {
                ItemEquip2 equip;
                if (_this.Equip.TryGetValue(bagid * 10, out equip))
                {
                    if (equip.GetId() != -1)
                    {
                        if (bagid == (int)eBagType.Wing)
                        {
                            equipsModel.Add(bagid, equip.GetId() * 100 + equip.GetExdata(11));
                        }
                        else if (bagid == (int)eBagType.WeaponShiZhuang ||
                                 bagid == (int)eBagType.EquipShiZhuang ||
                                 bagid == (int)eBagType.WingShiZhuang)
                        {
                            equipsModel.Add(bagid, equip.GetId() * 100 + equip.GetExdata(31));
                        }
                        else
                        {
                            equipsModel.Add(bagid, equip.GetId() * 100 + equip.GetExdata(0));
                        }
                    }
                }
            }
        }

        //获得属性
        public int GetAttribute(ObjCharacter _this, eAttributeType nIndex)
        {
            return _this.Attr.GetDataValue(nIndex);
        }

        #endregion

        #region 初始化

        // 构造函数

        public void Init(ObjCharacter _this, ulong characterId, int dataId, int level)
        {
            ObjBase.GetImpl().InitBase(_this, characterId, dataId);
            _this.BuffList = new BuffList();
            _this.BuffList.InitByBase(_this);
            _this.Attr = new FightAttr(_this);
            _this.Skill = new SkillManager(_this);
            level = _this.InitTableData(level);
            _this.InitEquip(level);
            _this.InitSkill(level);
            _this.InitBuff(level);
            _this.InitAttr(level);
        }

        //初始化表格数据
        public virtual int InitTableData(ObjCharacter _this, int level)
        {
            _this.TableCharacter = Table.GetCharacterBase(_this.TypeId);
            return level;
        }

        #endregion

        #region 基本属性方法

        //名字
        public virtual void SetName(ObjCharacter _this, string name)
        {
            _this.mName = name;

        }

        public virtual string GetName(ObjCharacter _this)
        {
            return _this.mName;
        }

        //获得等级
        public virtual int GetLevel(ObjCharacter _this)
        {
            return _this.Attr.GetDataValue(eAttributeType.Level);
        }

        /// <summary>
        ///     设置等级
        /// </summary>
        /// <returns></returns>
        public void SetLevel(ObjCharacter _this, int nValue)
        {
            if (nValue < 1)
            {
                nValue = 1;
            }
            _this.Attr.SetDataValue(eAttributeType.Level, nValue);
        }

        public void SetToLevel(ObjCharacter _this, int lv)
        {
            foreach (var i in _this.TableCharacter.InitSkill)
            {
                _this.Skill.ResetSkill(i, lv);
            }
            SetLevel(_this, lv);
            _this.Attr.InitAttributesAll();
        }

        //阵营
        public virtual void SetCamp(ObjCharacter _this, int camp)
        {
            _this.mCamp = camp;
            if (null == _this.TableCamp || (null != _this.TableCamp && _this.TableCamp.Id != camp))
            {
                _this.TableCamp = Table.GetCamp(camp);
            }
            foreach (var retinue in _this.mRetinues)
            {
                if (null != retinue && retinue.Active)
                {
                    retinue.SetCamp(camp);
                }
            }
        }

        public virtual int GetCamp(ObjCharacter _this)
        {
            return _this.mCamp;
        }

        //队伍
        public virtual void SetTeamId(ObjCharacter _this, ulong teamId, int state)
        {
            _this.mTeamId = teamId;
        }

        public virtual ulong GetTeamId(ObjCharacter _this)
        {
            return _this.mTeamId;
        }

        public virtual float GetMoveSpeed(ObjCharacter _this)
        {
            return _this.Attr.GetDataValue(eAttributeType.MoveSpeed)*ObjCharacter.MOVESPEED_RATE;
        }

        #endregion

        #region 逻辑数据方法

        //死亡状态
        public virtual bool IsDead(ObjCharacter _this)
        {
            return _this.mIsDead;
        }

        //当前目标
        private void SetSelectTarget(ObjCharacter _this, ulong id)
        {
            _this.mSelectTarget = id;
        }

        private ulong GetSelectTarget(ObjCharacter _this)
        {
            return _this.mSelectTarget;
        }

        //朝向
        public void TurnFaceTo(ObjCharacter _this, Vector2 pos)
        {
            var dif = pos - _this.GetPosition();
            dif.Normalize();
            _this.SetDirection(dif);
        }

        #endregion

        #region 精灵

        public int GetElfSkillFightPoint(ObjCharacter _this)
        {
            return _this.ElfFightPoint;
        }

        public void ElfChange(ObjCharacter _this, List<int> removeBuff, Dictionary<int, int> addBuff, int fightPoint)
        {
            if (_this.ElfFightPoint != fightPoint)
            {
                _this.ElfFightPoint = fightPoint;
                _this.Attr.SetFightPointFlag();
            }
            foreach (var buffId in removeBuff)
            {
                RemoveElfBuff(_this, buffId);
            }
            if (removeBuff.Count > 0)
                _this.BuffList.Do_Del_Buff();

            foreach (var buff in addBuff)
            {
                AddElfBuff(_this, buff.Key, buff.Value);
            }
        }

        // 添加精灵被动buff
        public void AddElfBuff(ObjCharacter _this, int buffId, int buffLevel)
        {
            if (buffId >= 0)
            {
                if (_this.CheckAddBuff(buffId, buffLevel, _this) == ErrorCodes.OK)
                {
                    _this.AddBuff(buffId, buffLevel, _this, 1);
                    _this.ElfBuffDict[buffId] = buffLevel;
                }
                else
                {
                    Logger.Error("AddElfBuff Error!  elfId={0}!", buffId);
                }
            }
        }

        public void RemoveElfBuff(ObjCharacter _this, int buffId)
        {
            if (buffId >= 0)
            {
                _this.DeleteBuff(buffId, eCleanBuffType.RemoveElf);
                _this.ElfBuffDict.Remove(buffId);
            }
        }

		public ulong GetTargetCharacterId(ObjCharacter _this)
		{
			return _this.NormalAttr.TargetCharacter;
		}

		public void SetTargetCharacterId(ObjCharacter _this,ulong id)
	    {
			_this.NormalAttr.TargetCharacter = id;
	    }
        #endregion

        #region 装备数据(增删改查)

        public void EquipModelStateChange(ObjCharacter _this, int nPart, int nState, ItemBaseData equip)
        {
            nPart = nPart / 10;
            if (EquipExtension.EquipModelBagId.Contains(nPart))
            {
                _this.BroadcastChangeEquipModel(_this.ObjId, nPart, nState == 0 ? -1 : equip.ItemId * 100);
            }
        }

        //装备发生了变化 nType=变化规则{0删除，1新增，2修改} nPart=部位   Equip=新装备数据
        public void EquipChange(ObjCharacter _this, int nType, int nPart, ItemBaseData equip)
        {
            switch (nType)
            {
                case 0:
                {
                    _this.DelEquip(nPart);
                }
                    break;
                case 1:
                {
                    var ib = new ItemEquip2();
                    ib.SetId(equip.ItemId);
                    ib.SetCount(equip.Count);
                    ib.CopyFrom(equip.Exdata);
                    ib.CheckTrialEquip();
                    _this.AddEquip(nPart, ib);
                }
                    break;
                case 2:
                {
                    var ib = new ItemEquip2();
                    ib.SetId(equip.ItemId);
                    ib.SetCount(equip.Count);
                    ib.CopyFrom(equip.Exdata);
                    ib.CheckTrialEquip();
                    _this.ResetEquip(nPart, ib);
                }
                    break;
            }
        }

        public void AddEquipBuff(ObjCharacter _this, ItemEquip2 equip)
        {
            if (equip == null)
                return;

            var tbItem = Table.GetItemBase(equip.GetId());
            if (tbItem != null)
            {
                if (tbItem.InitInBag == (int) eBagType.Wing)
                    return;
                if (tbItem.InitInBag >= (int)eBagType.EquipShiZhuangBag && tbItem.InitInBag <= (int)eBagType.WeaponShiZhuangBag)
                    return;
            }

            // 添加武器被动buff
            if (equip.IsTrialEnd())
                return;

            var addBuffId = equip.GetBuffId(0);
            if (addBuffId >= 0)
            {
                var skillLevel = equip.GetBuffLevel(0);
                if (_this.CheckAddBuff(addBuffId, skillLevel, _this) == ErrorCodes.OK)
                {
                    _this.AddBuff(addBuffId, skillLevel, _this, 1);
                }
                else
                {
                    Logger.Error("AddEquip AddBuff Error!  equipId={0}!", equip.GetId());
                }
            }     
        }

        public void RemoveEquipBuff(ObjCharacter _this, ItemEquip2 equip)
        {
            if (equip == null)
                return;
            var tbItem = Table.GetItemBase(equip.GetId());
            if (tbItem != null)
            {
                if (tbItem.InitInBag == (int)eBagType.Wing)
                    return;
                if (tbItem.InitInBag >= (int)eBagType.EquipShiZhuangBag && tbItem.InitInBag <= (int)eBagType.WeaponShiZhuangBag)
                    return;
            }

            // 删除武器带的buff
            var buffId = equip.GetBuffId(0);
            if (buffId >= 0)
            {
                _this.DeleteBuff(buffId, eCleanBuffType.DeleteEquip);
            }                        
        }

        public void RemoveCantMoveBuff(ObjCharacter _this)
        {
            foreach (var buff in _this.BuffList.mData)
            {
                if (buff.mBuff.Type == 6 || buff.mBuff.Type == 7)
                {
                    MissBuff.DoEffect(_this.Scene, _this, buff);
                    DeleteBuff(_this, buff, eCleanBuffType.GoHome);
                }
            }
        }
        
        //增加装备（穿上装备）
        public void AddEquip(ObjCharacter _this, int nPart, ItemEquip2 equip)
        {
            var oldEquip = _this.GetEquip(nPart);
            if (oldEquip != null)
            {
                Logger.Warn("AddEquip Error!  EquipPart={0} Is Have!", nPart); //之后refresh skilldata
                RemoveEquipBuff(_this, oldEquip);
            }
            _this.Equip[nPart] = equip;

            AddEquipBuff(_this, equip);

            nPart = nPart/10;
            if (EquipExtension.EquipModelBagId.Contains(nPart))
            {
                if (nPart == (int)eBagType.Wing)
                {
                    _this.BroadcastChangeEquipModel(_this.ObjId, nPart, equip.GetId() * 100 + equip.GetExdata(11));
                }
                else if (nPart == (int)eBagType.WeaponShiZhuang ||
                         nPart == (int)eBagType.EquipShiZhuang ||
                         nPart == (int)eBagType.WingShiZhuang)
                {
                    _this.BroadcastChangeEquipModel(_this.ObjId, nPart, equip.GetId() * 100 + equip.GetExdata(31));
                }
                else
                {
                    _this.BroadcastChangeEquipModel(_this.ObjId, nPart, equip.GetId() * 100 + equip.GetExdata(0));
                }
            }
            _this.Attr.EquipRefresh();
        }

        //删除装备（脱下装备）
        public void DelEquip(ObjCharacter _this, int nPart, bool refreshAttr = true)
        {
            var oldEquip = _this.GetEquip(nPart);
            if (oldEquip == null)
            {
                Logger.Warn("DelEquip not Find! EquipPart=[{0}] ", nPart);
                return;
            }
            _this.Equip.Remove(nPart);

            RemoveEquipBuff(_this, oldEquip);

            nPart = nPart/10;
            if (refreshAttr)
            {
                if (EquipExtension.EquipModelBagId.Contains(nPart))
                {
                    _this.BroadcastChangeEquipModel(_this.ObjId, nPart, -1);
                }
                _this.Attr.EquipRefresh();
            }
        }

        //重置装备
        public void ResetEquip(ObjCharacter _this, int nPart, ItemEquip2 equip)
        {
            var OldEquip = _this.GetEquip(nPart);
            if (OldEquip == null)
            {
                Logger.Warn("ResetEquip not Find! Part=[{0}] ItemID=[{1}] ", nPart, equip.GetId());
            }
            else
            {
                _this.DelEquip(nPart, false);
            }
            _this.AddEquip(nPart, equip);
            //Attr.EquipRefresh();
        }

        //获得装备数据
        public ItemEquip2 GetEquip(ObjCharacter _this, int nPart)
        {
            ItemEquip2 equip;
            _this.Equip.TryGetValue(nPart, out equip);
            return equip;
        }

        #endregion

        #region 同步数据

        public void AddSyncData(ObjCharacter _this,
                                uint id,
                                ObjCharacter.INotifyPropertyChanged holder,
                                Func<byte[]> getter)
        {
            Dictionary<uint, ObjCharacter.SourceBinding> dict;
            if (!_this.mDirtyFlag.TryGetValue(holder, out dict))
            {
                holder.PropertyChanged += _this.PropertyChangedHandler;
                dict = new Dictionary<uint, ObjCharacter.SourceBinding>();
                _this.mDirtyFlag[holder] = dict;
            }

            ObjCharacter.SourceBinding binding;
            if (!dict.TryGetValue(id, out binding))
            {
                dict[id] = new ObjCharacter.SourceBinding
                {
                    Getter = getter
                };
            }
        }

        public void RemoveAllSyncData(ObjCharacter _this)
        {
            PlayerLog.WriteLog(_this.mObjId, "----------Scene----------RemoveAllSyncData----------");
            foreach (var flag in _this.mDirtyFlag)
            {
                flag.Value.Clear();
                flag.Key.PropertyChanged -= _this.PropertyChangedHandler;
            }

            _this.mDirtyFlag.Clear();
        }

        public void RegisterAllSyncData(ObjCharacter _this)
        {
            if (_this is ObjNPC)
            {
                _this.OnSyncRequested(_this.mObjId, (int) eSceneSyncId.SyncHpMax);
                _this.OnSyncRequested(_this.mObjId, (int) eSceneSyncId.SyncMpMax);
                _this.OnSyncRequested(_this.mObjId, (int) eSceneSyncId.SyncHpNow);
                _this.OnSyncRequested(_this.mObjId, (int) eSceneSyncId.SyncMpNow);
                _this.OnSyncRequested(_this.mObjId, (int) eSceneSyncId.SyncMoveSpeed);
				_this.OnSyncRequested(_this.mObjId, (int)eSceneSyncId.SyncTargetCharacterId);
            }
			else if (_this is ObjPlayer)
            {
                _this.OnSyncRequested(_this.mObjId, (int)eSceneSyncId.SyncPlayerName);

				_this.OnSyncRequested(_this.mObjId, (int)eSceneSyncId.SyncHpMax);
				_this.OnSyncRequested(_this.mObjId, (int)eSceneSyncId.SyncMpMax);
				_this.OnSyncRequested(_this.mObjId, (int)eSceneSyncId.SyncHpNow);
				_this.OnSyncRequested(_this.mObjId, (int)eSceneSyncId.SyncMpNow);
				_this.OnSyncRequested(_this.mObjId, (int)eSceneSyncId.SyncMoveSpeed);

                _this.OnSyncRequested(_this.mObjId, (int) eSceneSyncId.SyncLevel);
                _this.OnSyncRequested(_this.mObjId, (int) eSceneSyncId.SyncAreaState);
                _this.OnSyncRequested(_this.mObjId, (int) eSceneSyncId.SyncFightValue);
                _this.OnSyncRequested(_this.mObjId, (int) eSceneSyncId.SyncPkModel);
                _this.OnSyncRequested(_this.mObjId, (int) eSceneSyncId.SyncPkValue);
                _this.OnSyncRequested(_this.mObjId, (int) eSceneSyncId.SyncReborn);
                _this.OnSyncRequested(_this.mObjId, (int) eSceneSyncId.SyncAllianceName);
                _this.OnSyncRequested(_this.mObjId, (int) eSceneSyncId.SyncTitle0);
                _this.OnSyncRequested(_this.mObjId, (int) eSceneSyncId.SyncTitle1);
                _this.OnSyncRequested(_this.mObjId, (int) eSceneSyncId.SyncTitle2);
                _this.OnSyncRequested(_this.mObjId, (int) eSceneSyncId.SyncTitle3);
                _this.OnSyncRequested(_this.mObjId, (int)eSceneSyncId.SyncTitle4);
            }
        }

        public bool OnSyncRequested(ObjCharacter _this, ulong characterId, uint syncId)
        {
            var syncType = (eSceneSyncId) syncId;
            switch (syncType)
            {
                case eSceneSyncId.SyncLevel:
                    _this.AddSyncData(syncId, _this.Attr, () => _this.Attr.GetDataValue(eAttributeType.Level));
                    break;
                case eSceneSyncId.SyncStrength:
                    _this.AddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.Strength));
                    break;
                case eSceneSyncId.SyncAgility:
                    _this.AddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.Agility));
                    break;
                case eSceneSyncId.SyncIntelligence:
                    _this.AddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.Intelligence));
                    break;
                case eSceneSyncId.SyncEndurance:
                    _this.AddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.Endurance));
                    break;
                case eSceneSyncId.SyncPhyPowerMin:
                    _this.AddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.PhyPowerMin));
                    break;
                case eSceneSyncId.SyncPhyPowerMax:
                    _this.AddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.PhyPowerMax));
                    break;
                case eSceneSyncId.SyncMagPowerMin:
                    _this.AddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.MagPowerMin));
                    break;
                case eSceneSyncId.SyncMagPowerMax:
                    _this.AddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.MagPowerMax));
                    break;
                case eSceneSyncId.SyncAddPower:
                    _this.AddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.AddPower));
                    break;
                case eSceneSyncId.SyncPhyArmor:
                    _this.AddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.PhyArmor));
                    break;
                case eSceneSyncId.SyncMagArmor:
                    _this.AddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.MagArmor));
                    break;
                case eSceneSyncId.SyncDamageResistance:
                    _this.AddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.DamageResistance));
                    break;
                case eSceneSyncId.SyncHpMax:
                    _this.AddSyncData(syncId, _this.Attr, () =>
                    {
                        var hpMax = _this.Attr.GetDataValue(eAttributeType.HpMax);
                        if (_this.Scene != null && _this.Scene.isNeedDamageModify)
                        {
                            if (Scene.IsNeedChangeHp(_this) != null)
                            {
                                hpMax = (int) (hpMax/_this.Scene.BeDamageModify);
                            }
                        }

                        return hpMax;
                    });
                    break;
                case eSceneSyncId.SyncMpMax:
                    _this.AddSyncData(syncId, _this.Attr, () =>
                    {
                        var mpMax = _this.Attr.GetDataValue(eAttributeType.MpMax);
                        return mpMax;
                    });
                    break;
                case eSceneSyncId.SyncLuckyPro:
                    _this.AddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.LuckyPro));
                    break;
                case eSceneSyncId.SyncLuckyDamage:
                    _this.AddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.LuckyDamage));
                    break;
                case eSceneSyncId.SyncExcellentPro:
                    _this.AddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.ExcellentPro));
                    break;
                case eSceneSyncId.SyncExcellentDamage:
                    _this.AddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.ExcellentDamage));
                    break;
                case eSceneSyncId.SyncHit:
                    _this.AddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.Hit));
                    break;
                case eSceneSyncId.SyncDodge:
                    _this.AddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.Dodge));
                    break;
                case eSceneSyncId.SyncDamageAddPro:
                    _this.AddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.DamageAddPro));
                    break;
                case eSceneSyncId.SyncDamageResPro:
                    _this.AddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.DamageResPro));
                    break;
                case eSceneSyncId.SyncDamageReboundPro:
                    _this.AddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.DamageReboundPro));
                    break;
                case eSceneSyncId.SyncIgnoreArmorPro:
                    _this.AddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.IgnoreArmorPro));
                    break;
                case eSceneSyncId.SyncMoveSpeed:
                    _this.AddSyncData(syncId, _this.Attr,
                        () => { return _this.Attr.GetDataValue(eAttributeType.MoveSpeed); });
                    break;
                case eSceneSyncId.SyncHitRecovery:
                    _this.AddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.HitRecovery));
                    break;
                case eSceneSyncId.SyncHpNow:
                    _this.AddSyncData(syncId, _this.Attr, () =>
                    {
                        var hpNow = _this.Attr.GetDataValue(eAttributeType.HpNow);
                        if (_this.Scene != null && _this.Scene.isNeedDamageModify)
                        {
                            if (Scene.IsNeedChangeHp(_this) != null)
                            {
                                hpNow = (int) (hpNow/_this.Scene.BeDamageModify);
                            }
                        }
                        return hpNow;
                    });

                    break;
                case eSceneSyncId.SyncMpNow:
                    _this.AddSyncData(syncId, _this.Attr,
                        () => _this.Attr.GetDataValue(eAttributeType.MpNow));
                    break;
                case eSceneSyncId.SyncFightValue:
                    _this.AddSyncData(syncId, _this.Attr,
                        () => { return _this.Attr.GetFightPoint(); });
                    break;
                case eSceneSyncId.SyncAreaState:
                    _this.AddSyncData(syncId, _this.NormalAttr,
                        () => { return ((int) (_this.NormalAttr.AreaState)); });
                    break;
                case eSceneSyncId.SyncPkModel:
                {
                    var player = _this as ObjPlayer;
                    if (player != null)
                    {
                        _this.AddSyncData(syncId, player, () => player.PkModel);
                    }
                }
                    break;
                case eSceneSyncId.SyncPkValue:
                {
                    var player = _this as ObjPlayer;
                    if (player != null)
                    {
                        _this.AddSyncData(syncId, player,
                            () =>
                            {
                                var ret = player.KillerValue;
                                if (player.KillerValue < 100)
                                {
                                    ret += player.PkTime;
                                }
                                return ret;
                            });
                    }
                }
                    break;
                case eSceneSyncId.SyncReborn:
                {
                    _this.AddSyncData(syncId, _this.Attr,
                        () => _this.Attr.Ladder);
                }
                    break;
                case eSceneSyncId.SyncTitle0:
                {
                    _this.AddSyncData(syncId, _this.Attr, () => _this.Attr.mEquipedTitles[0]);
                }
                    break;
                case eSceneSyncId.SyncTitle1:
                {
                    _this.AddSyncData(syncId, _this.Attr, () => _this.Attr.mEquipedTitles[1]);
                }
                    break;
                case eSceneSyncId.SyncTitle2:
                {
                    _this.AddSyncData(syncId, _this.Attr, () => _this.Attr.mEquipedTitles[2]);
                }
                    break;
                case eSceneSyncId.SyncTitle3:
                {
                    _this.AddSyncData(syncId, _this.Attr, () => _this.Attr.mEquipedTitles[3]);
                }
                    break;
                case eSceneSyncId.SyncTitle4:
                    {
                        _this.AddSyncData(syncId, _this.Attr, () => _this.Attr.mEquipedTitles[4]);
                    }
                    break;
                case eSceneSyncId.SyncAllianceName:
                {
                    var player = _this as ObjPlayer;
                    if (player != null)
                    {
                        _this.AddSyncData(syncId, player, () => { return player.AllianceName; });
                    }
                }
                    break;
                case eSceneSyncId.SyncFireAttack:
                {
                    _this.AddSyncData(syncId, _this.Attr,
                       () => _this.Attr.GetDataValue(eAttributeType.FireAttack));                   
                }
                    break;

                case eSceneSyncId.SyncIceAttack:
                    {
                        _this.AddSyncData(syncId, _this.Attr,
                           () => _this.Attr.GetDataValue(eAttributeType.IceAttack));
                    }
                    break;
                case eSceneSyncId.SyncPoisonAttack:
                    {
                        _this.AddSyncData(syncId, _this.Attr,
                           () => _this.Attr.GetDataValue(eAttributeType.PoisonAttack));
                    }
                    break;
                case eSceneSyncId.SyncFireResistance:
                    {
                        _this.AddSyncData(syncId, _this.Attr,
                           () => _this.Attr.GetDataValue(eAttributeType.FireResistance));
                    }
                    break;
                case eSceneSyncId.SyncIceResistance:
                    {
                        _this.AddSyncData(syncId, _this.Attr,
                           () => _this.Attr.GetDataValue(eAttributeType.IceResistance));
                    }
                    break;
                case eSceneSyncId.SyncPoisonResistance:
                    {
                        _this.AddSyncData(syncId, _this.Attr,
                           () => _this.Attr.GetDataValue(eAttributeType.PoisonResistance));
                    }
                    break;
				case eSceneSyncId.SyncTargetCharacterId:
	            {
					_this.AddSyncData(syncId, _this.NormalAttr,
						() =>
						{
							if (_this.GetTargetCharacterId() != ulong.MaxValue && null != _this.Scene)
							{
								var player = _this.Scene.FindPlayer(_this.GetTargetCharacterId());
								if (null != player)
								{
                                    return player.GetName() + "|" + player.TypeId + "|" + player.Attr.Ladder;//51.转生ID
								}
							}
							return "";
						});
	            }
				break;
                case eSceneSyncId.SyncPlayerName:
                {
                    var player = _this as ObjPlayer;
                    if (player != null)
                    {
                        _this.AddSyncData(syncId, player, () => { return player.GetName(); });
                    }
                }
                break;
            }

            return true;
        }

        #endregion

        #region 角色移动相关

        /// <summary>
        ///     移动角色（寻路是异步的）
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="pos">目标位置</param>
        /// <param name="offset">目标位置偏移</param>
        /// <param name="searchPath">是否需要寻路，false表示直线移动到目的地</param>
        /// <param name="pushLastPos">是否需要精确移动到目标点，因为寻路是以格子中心点为坐标的，最后可能会产生半格的误差</param>
        /// <param name="callback">寻路结果callback</param>
        /// <returns>能走返回true，不能走返回false</returns>
        public virtual MoveResult MoveTo(ObjCharacter _this,
                                         Vector2 pos,
                                         float offset = 0.05f,
                                         bool searchPath = true,
                                         bool pushLastPos = false,
                                         Action<List<Vector2>> callback = null)
        {
            if (!_this.CanMove())
            {
                return MoveResult.CannotMoveByBuff;
            }
            if ((pos - _this.GetPosition()).Length() <= offset)
            {
                return MoveResult.AlreadyThere;
            }

            var path = SceneObstacle.EmptyPath;
            if (searchPath)
            {
                var hasPath = _this.Scene.FindPathTo(_this, pos, l =>
                {
                    // 因为离开场景，Reset等情况下，不用再等待寻路结束了
                    if (_this.mWaitingToMove == false)
                    {
                        return;
                    }

                    _this.mWaitingToMove = false;

                    if (l.Count == 0)
                    {
                        if (_this.GetObjType() != ObjType.NPC)
                            return;

                        Logger.Error("NPC Find Path Error scene={0},name={1},pos={2}", _this.Scene.TypeId, _this.GetName(), pos);

                        // 寻路失败，可能进入了一个无效的位置，无视阻挡
                        pushLastPos = true;
                    }

                    if (pushLastPos)
                    {
                        if (_this.Scene.ValidPosition(pos))
                        {
                            l.Add(pos);
                        }
                        else
                        {
                            var nearest = _this.Scene.FindNearestValidPosition(pos);
                            if (null != nearest)
                            {
                                l.Add(nearest.Value);
                            }
                        }
                    }

                    // 修正之后还是没有路径的话
                    if (l.Count == 0)
                    {
                        if (_this.GetObjType() == ObjType.NPC)
                        {
                            // 寻路失败，可能进入了一个无效的位置，直接把NPC拽到出生点
                            var npc = (_this as ObjNPC);
                            npc.SetPosition(npc.BornPosition);
                            npc.EnterState(BehaviorState.Idle);
                        }
                        return;
                    }

                    _this.mIsMoving = true;
                    _this.mTargetPos = l;
                    if (callback != null)
                    {
                        try
                        {
                            callback(l);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "find path call back error.");
                        }
                    }

                    if (offset > 0.1f)
                    {
                        PathBackward(_this.GetPosition(), offset, l);
                    }

                    _this.OnMoveBegin();
                });

                if (hasPath)
                {
                    _this.mWaitingToMove = true;
                    return MoveResult.Ok;
                }
                return MoveResult.CannotReach;
            }
            path.Add(pos - Vector2.Normalize(pos - _this.GetPosition())*offset);


            if (path.Count == 1)
            {
                path[0] = path[0] - Vector2.Normalize(path[0] - _this.GetPosition())*offset;
            }
            else if (path.Count > 1)
            {
                path[path.Count - 1] = path[path.Count - 1] -
                                       Vector2.Normalize(path[path.Count - 1] - path[path.Count - 2])*offset;
            }
            else
            {
                return MoveResult.CannotReach;
            }
            _this.mIsMoving = true;

            _this.mTargetPos = path;
            _this.OnMoveBegin();

            return MoveResult.Ok;
        }

        protected bool PathBackward(Vector2 pos, float offset, List<Vector2> vec)
        {
            while (offset > 0 && vec.Count > 1)
            {
                var l = (vec[vec.Count - 1] - vec[vec.Count - 2]).Length();
                if (l < offset)
                {
                    vec.RemoveAt(vec.Count - 1);
                    offset -= l;
                }
                else
                {
                    var p = vec[vec.Count - 1];
                    vec[vec.Count - 1] -= Vector2.Normalize(p - vec[vec.Count - 2])*offset;
                    offset = -1;
                    break;
                }
            }

            if (offset > 0)
            {
                if (vec.Count == 1)
                {
                    if (Vector2.Distance(vec[0], pos) < offset + 0.01f)
                    {
                        return true;
                    }
                    vec[0] -= Vector2.Normalize(vec[0] - pos)*offset;
                }
                else // == 0
                {
                    return true;
                }
            }

            return false;
        }

        public void SetCheckPostion(ObjCharacter _this, Vector2 pos)
        {
            if (_this.Scene.ValidPosition(pos))
            {
                _this.SetPosition(pos);
            }
            else
            {
                var nearest = _this.Scene.FindNearestValidPosition(pos);
                if (null != nearest)
                {
                    _this.SetPosition(nearest.Value);
                }
                else
                {
                    _this.SetPosition(pos);
                }
            }
        }

        public virtual bool Move(ObjCharacter _this, List<Vector2> path)
        {
            _this.mIsMoving = true;
            _this.mTargetPos = path;

            _this.OnMoveBegin();
            return true;
        }

        public virtual void OnMoveBegin(ObjCharacter _this)
        {
            _this.mIsMoving = true;

            _this.TurnFaceTo(_this.mTargetPos[0]);

            _this.Skill.EventToSkill(eSkillEventType.Move);

            _this.BroadcastMoveTo();
        }

        //当移动停止时
        public virtual void OnMoveEnd(ObjCharacter _this)
        {
        }

        public virtual void Tick_Move(ObjCharacter _this, float delta)
        {
            if (_this.mIsMoving)
            {
                if (_this.mTargetPos.Count > 0)
                {
                    var pos = _this.mTargetPos[0];
                    _this.TurnFaceTo(pos);
                    var stepLenth = _this.GetMoveSpeed()*delta;
                    var dif = (pos - _this.GetPosition()).Length();
                    if (stepLenth > dif)
                    {
                        var v = stepLenth - dif;
                        pos = _this.mTargetPos[0];
                        _this.mTargetPos.RemoveAt(0);
                        while (v > 0 && _this.mTargetPos.Count > 0)
                        {
                            var dir = (_this.mTargetPos[0] - pos);
                            var l = dir.Length();

                            if (l < v)
                            {
                                v -= l;
                                pos = _this.mTargetPos[0];
                                _this.mTargetPos.RemoveAt(0);
                            }
                            else
                            {
                                dir.Normalize();
                                _this.SetPosition(pos + dir*v);
                                break;
                            }
                        }

                        if (_this.mTargetPos.Count == 0)
                        {
                            _this.SetPosition(pos);
                            _this.StopMove(false);
                        }
                    }
                    else
                    {
                        _this.SetPosition(_this.GetPosition() + _this.GetDirection()*stepLenth);
                    }
                }
            }
        }

        public void Tick(ObjCharacter _this, float delta)
        {
            if (!_this.Active)
            {
                return;
            }
            _this.BuffList.DeleteBuff();
            ObjBase.GetImpl().Tick(_this, delta);

            if (_this.mIsForceMoving)
            {
                if (_this.mForceMovingTime - delta < 0)
                {
                    var target = _this.GetPosition() + _this.mForceMoveSpeed*_this.mForceMovingTime;
                    if (_this.Scene.GetObstacleValue(target.X, target.Y) != SceneObstacle.ObstacleValue.Obstacle)
                    {
                        _this.SetPosition(target);
                    }

                    _this.mForceMovingTime = 0;
                    _this.mIsForceMoving = false;
                }
                else
                {
                    var target = _this.GetPosition() + _this.mForceMoveSpeed*delta;
                    if (_this.Scene.GetObstacleValue(target.X, target.Y) != SceneObstacle.ObstacleValue.Obstacle)
                    {
                        _this.SetPosition(target);
                        _this.mForceMovingTime -= delta;
                    }
                    else
                    {
                        _this.mForceMovingTime = 0;
                        _this.mIsForceMoving = false;
                    }
                }
            }
            else
            {
                // lower move frequency.
                if (_this.mLogicTickCount%2 == 0)
                {
                    _this.Tick_Move(delta + SceneServer.Instance.ServerControl.LastFrameTime);
                }
            }
            _this.mLogicTickCount++;
            _this.ProcessPositionChanged();
#if DEBUG && DEBUGPOS

			if (null == _this.Zone) return;
			SceneServer.Instance.ServerControl.DebugObjPosition(_this.EnumAllVisiblePlayerIdExclude(), _this.ObjId, Utility.MakePositionDataByPosAndDir(_this.GetPosition(), _this.GetDirection()));
#endif
        }

        //设置坐标
        public void SetPosition(ObjCharacter _this, Vector2 p)
        {
            ObjBase.GetImpl().SetPosition(_this, p);
            _this.mPositionChanged = true;
            //_this.ProcessPositionChanged();
        }

        public void SetPosition(ObjCharacter _this, float x, float y)
        {
            //base.SetPosition(x, y);
            _this.SetPosition(new Vector2(x, y));
        }

        public virtual void ProcessPositionChanged(ObjCharacter _this)
        {
            if (!_this.mPositionChanged)
            {
                return;
            }

            _this.mPositionChanged = false;

            try
            {
                _this.UpdateZone();
                _this.UpdateTriggerArea();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public void UpdateZone(ObjCharacter _this)
        {
            //安全检查
            if (null == _this.Scene || null == _this.Zone)
            {
                return;
            }

            //获得我的zone id
            var pos = _this.GetPosition();
#if DEBUG
            if (pos.X < 0 || pos.X >= _this.Scene.mSceneWidth || pos.Y < 0 || pos.Y >= _this.Scene.mSceneHeight)
            {
                Logger.Error("out of scene[{0}] obj[{1}] [{2}{3}]", _this.Scene.TypeId, _this.ObjId, pos.X, pos.Y);
            }
#endif
            var zoneId = _this.Scene.Pos2ZoneId(pos.X, pos.Y);

            var newZone = _this.Scene.GetZone(zoneId);

            //不站在一个有效的位置上
            if (null == newZone)
            {
                Logger.Error("[{0}] out of scene[{1},{2}]", _this.GetName(), pos.X, pos.Y);

                var validPos = _this.Scene.FindNearestValidPosition(pos);
                if (null != validPos)
                {
                    pos = validPos.Value;
                    Logger.Warn("[{0}] set valid pos[{1},{2}]", _this.GetName(), pos.X, pos.Y);
                }
                else
                {
                    pos = new Vector2((float)_this.Scene.TableSceneData.Entry_x,
                        (float)_this.Scene.TableSceneData.Entry_z);
                    Logger.Warn("[{0}] set entry pos[{1},{2}]", _this.GetName(), pos.X, pos.Y);
                }

                _this.SetPosition(pos);
                zoneId = _this.Scene.Pos2ZoneId(pos.X, pos.Y);
                newZone = _this.Scene.GetZone(zoneId);

                if (null == newZone)
                {
                    _this.Active = false;
                    Logger.Fatal("[{0}] disabled pos[{1},{2}]", _this.GetName(), pos.X, pos.Y);
                    return;
                }
            }

            //如果我的zone没变化
            if (zoneId == _this.Zone.Id)
            {
                return;
            }

            var listMe = new List<ulong> {_this.ObjId};

            //比较计算应该创建我的zone
            var createMeZone = new List<Zone>();
            foreach (var zone in newZone.VisibleZoneList)
            {
                if (!_this.Zone.VisibleZoneList.Contains(zone))
                {
                    createMeZone.Add(zone);
                }
            }

            //对该创建我的zone进行处理
            if (createMeZone.Count > 0)
            {
                //我能看见的Obj 列表
                var listICanSeeObj = new List<ObjBase>();

                //能看见我的player
                var listCanSeeMePlayerId = new List<ulong>();

                foreach (var zone in createMeZone)
                {
                    foreach (var pair in zone.ObjDict)
                    {
                        var obj = pair.Value;

                        //过滤自己
                        if (obj.ObjId == _this.ObjId)
                        {
                            continue;
                        }

                        //还没有被激活
                        if (!obj.Active)
                        {
                            continue;
                        }

                        //判断这个obj相对于我是否能看见
                        if (obj.IsVisibleTo(_this))
                        {
                            listICanSeeObj.Add(obj);
                        }

                        //player
                        if (obj.GetObjType() == ObjType.PLAYER)
                        {
                            //这个player能看见我
                            if (_this.IsVisibleTo(obj))
                            {
                                listCanSeeMePlayerId.Add(obj.ObjId);
                            }
                        }
                    }
                }

                //通知我创建周围Obj
                if (listICanSeeObj.Count > 0 && _this.GetObjType() == ObjType.PLAYER)
                {
                    var msg2Me = new CreateObjMsg();
                    foreach (var obj in listICanSeeObj)
                    {
                        msg2Me.Data.Add(obj.DumpObjData(ReasonType.VisibilityChanged));
                    }

                    //_this.NotifyCharactersToSyncMe(listMe);
                    SceneServer.Instance.ServerControl.CreateObj(listMe, msg2Me);
                    foreach (var obj in listICanSeeObj)
                    {
                        _this.RegisterCharacterSyncData(obj);
                    }
                }

                //通知周围人创建我
                if (listCanSeeMePlayerId.Count > 0)
                {
                    var msg2Other = new CreateObjMsg();
                    var data = _this.DumpObjData(ReasonType.VisibilityChanged);
                    msg2Other.Data.Add(data);

                    SceneServer.Instance.ServerControl.CreateObj(listCanSeeMePlayerId, msg2Other);
                    _this.NotifyCharactersToSyncMe(listCanSeeMePlayerId);
                }
            }


            //比较计算应该删除我的zone
            var deleteMeZone = new List<Zone>();
            foreach (var zone in _this.Zone.VisibleZoneList)
            {
                if (!newZone.VisibleZoneList.Contains(zone))
                {
                    deleteMeZone.Add(zone);
                }
            }

            //对该删除我的zone处理
            if (deleteMeZone.Count > 0)
            {
                var listICantSeeObj = new Uint64Array();
                var listPlayrCantSeeMe = new List<ulong>();
                foreach (var zone in deleteMeZone)
                {
                    foreach (var pair in zone.ObjDict)
                    {
                        var obj = pair.Value;

                        //过滤自己
                        if (obj.ObjId == _this.ObjId)
                        {
                            continue;
                        }

                        //该告诉我删除这个obj
                        listICantSeeObj.Items.Add(obj.ObjId);

                        //如果是玩家
                        if (obj.GetObjType() == ObjType.PLAYER && obj.Active)
                        {
                            listPlayrCantSeeMe.Add(obj.ObjId);
                        }
                    }
                }

                //通知我删除的周围人
                if (listICantSeeObj.Items.Count > 0 && _this.GetObjType() == ObjType.PLAYER)
                {
                    foreach (var id in listICantSeeObj.Items)
                    {
                        if (_this.Scene != null)
                        {
                            var o = _this.Scene.FindObj(id);
                            _this.RemoveCharacterSyncData(o);
                        }
                    }
                    SceneServer.Instance.ServerControl.DeleteObj(listMe, listICantSeeObj,
                        (int) ReasonType.VisibilityChanged);
                }

                //通知周围的人删除我
                if (listPlayrCantSeeMe.Count > 0)
                {
                    var array = new Uint64Array();
                    array.Items.Add(_this.ObjId);
                    _this.NotifyCharactersToStopSyncMe(listPlayrCantSeeMe);
                    SceneServer.Instance.ServerControl.DeleteObj(listPlayrCantSeeMe, array,
                        (int) ReasonType.VisibilityChanged);
                }
            }

            //旧zone删除我
            _this.Zone.RemoveObj(_this);

            //新zone加入我
            newZone.AddObj(_this);

            //设置我当前zone
            _this.SetZone(newZone);
        }

        public virtual void UpdateTriggerArea(ObjCharacter _this)
        {
            if (null == _this.Scene)
            {
                return;
            }
            foreach (var pair in _this.Scene.AreaDict)
            {
                pair.Value.AdjustPlayer(_this);
            }
        }

        #endregion

        #region 场景相关

        public void OnEnterScene(ObjCharacter _this)
        {
            _this.Active = true;
            _this.RegisterAllSyncData();
        }

        public void OnLeaveScene(ObjCharacter _this)
        {
            _this.Active = false;
            _this.RemoveMeFromOtherEnemyList();
            _this.ClearEnemy();
            _this.CleanBullet();
            _this.RemoveAllSyncData();
            _this.ModelId = -1;
        }

        #endregion

        #region 效果相关

        public virtual void OnDamage(ObjCharacter _this, ObjCharacter enemy, int damage)
        {
            if (enemy.ObjId == _this.ObjId)
            {
                return;
            }

            //通知每个召唤物主人被伤害了
            foreach (var retinue in _this.mRetinues)
            {
                if (null != _this.mRetinues && retinue.Active && !retinue.IsDead())
                {
                    retinue.OnOwnerReceiveDamage(enemy, damage);
                }
            }

			if (enemy.CanBeAttacked())
	        {
				_this.AddEnemy(enemy.ObjId);    
	        }

			if (_this.CanBeAttacked())
	        {
				enemy.AddEnemy(_this.ObjId);    
	        }
            
            enemy.OnMakeDamageTo(_this, damage);
        }

	    public virtual void OnTrapped(ObjCharacter _this, ObjCharacter enemy)
	    {
		    CharacterTrappedEvent.DoEffect(_this.Scene, _this, enemy);
	    }
        public virtual void OnMakeDamageTo(ObjCharacter _this, ObjCharacter target, int damage)
        {
            if (null == target)
            {
                return;
            }

            if (target.ObjId == _this.ObjId)
            {
                return;
            }

            //通知每个召唤物主人对别人造成伤害了
            foreach (var retinue in _this.mRetinues)
            {
                if (retinue.Active && !retinue.IsDead())
                {
                    retinue.OnOwnerMakeDamage(target, damage);
                }
            }
        }

        public virtual void Die(ObjCharacter _this, ulong characterId, int delayView, int damage = 0)
        {
            if (!_this.Active || _this.IsDead())
            {
                return;
            }
            _this.mIsDead = true;
            _this.StopMove(false);
            Logger.Info("characterId={0} is die", characterId);
            _this.OnDie(characterId, delayView, damage);
        }

        public void OnlineDie(ObjCharacter _this)
        {
            _this.mIsDead = true;
            _this.OnDie(_this.ObjId, 0, 0);
        }

        public virtual void OnDie(ObjCharacter _this, ulong characterId, int delayView, int damage = 0)
        {
            var replyMsg = new BuffResultMsg();
            replyMsg.buff.Add(new BuffResult
            {
                SkillObjId = characterId,
                TargetObjId = _this.ObjId,
                Type = BuffType.HT_DIE,
                ViewTime = Extension.AddTimeDiffToNet(delayView)
            });
            _this.BroadcastBuffList(replyMsg);
            _this.Skill.StopCurrentSkill();
            _this.RemoveMeFromOtherEnemyList();
            if (null != _this.Scene)
            {
                foreach (var enemyId in _this.EnemyList)
                {
                    var character = _this.Scene.FindCharacter(enemyId);
                    if (null != character)
                    {
                        character.OnEnemyDie(_this);
                    }
                }
            }

            _this.ClearEnemy();
            _this.CleanBullet();
            _this.BuffList.DelBuffByOnDie();
        }

        public virtual void OnEnemyDie(ObjCharacter _this, ObjCharacter obj)
        {
        }

        //重置
        public void Reset(ObjCharacter _this)
        {
            _this.BuffList.OnDestroy();
            //_this.Skill.mData.Clear();
            foreach (var data in _this.Skill.mData)
            {
                var skillData = data.Value;
                if (skillData.mTable.CastType == 3)
                {
                    skillData.mBuff = _this.AddBuff(skillData.mTable.CastParam[0], _this.GetLevel(), _this);
                }
            }
            //InitSkill(_this, _this.GetLevel());
            _this.ResetAttribute();
            _this.mIsMoving = false;
            _this.mIsForceMoving = false;
            _this.mWaitingToMove = false;
            _this.mTargetPos.Clear();
            _this.Skill.Reset();
            //EnemyList.Clear();
            _this.ClearEnemy();
            _this.CleanBullet();
        }

        public virtual void ResetAttribute(ObjCharacter _this)
        {
            _this.Attr.InitAttributesAll();
        }

        public void SyncCharacterPostion(ObjCharacter _this)
        {
            if (!_this.mActive)
            {
                return;
            }
            if (null == _this.Zone)
            {
                return;
            }
            var pos = Utility.MakePositionDataByPosAndDir(_this.GetPosition(), _this.GetDirection());
            SceneServer.Instance.ServerControl.SyncCharacterPostion(_this.EnumAllVisiblePlayerIdExclude(), _this.ObjId,
                pos);
        }

        //重生
		public virtual void Relive(ObjCharacter _this, bool byItem = false)
        {
            if (!_this.mIsDead)
            {
                Logger.Warn("I'm not dead![{0}]", _this.ObjId);
                return;
            }

            _this.mIsDead = false;

            _this.Reset();
            _this.Attr.SetDataValue(eAttributeType.HpNow, _this.Attr.GetDataValue(eAttributeType.HpMax));
            _this.Attr.SetDataValue(eAttributeType.MpNow, _this.Attr.GetDataValue(eAttributeType.MpMax));
            _this.BroadcastCreateMe();
            // 处理位置改变，并向客户端同步自己的位置
            _this.SyncCharacterPostion();

            var msg = new BuffResultMsg();
            msg.buff.Add(new BuffResult
            {
                SkillObjId = TypeDefine.INVALID_ULONG,
                TargetObjId = _this.ObjId,
                Type = BuffType.HT_RELIVE
            });
            _this.BroadcastBuffList(msg);
        }

        public void RideMount(ObjCharacter _this, int mountId)
        {
            if (mountId > 0)
            {
                _this.BroadcastChangeEquipModel(_this.ObjId, (int) eBagType.Mount, mountId);
            }
            else
            {
                _this.BroadcastChangeEquipModel(_this.ObjId, (int)eBagType.Mount, 0);
            }
            
        }

		public bool CanBeAttacked(ObjCharacter _this)
	    {
		    if (0 != _this.TableCharacter.IsBeAttack)
		    {
				return true;
		    }
		    return false;
	    }
        #endregion

        #region Enemy

        //添加obj到我的敌人列表
        public virtual void AddEnemy(ObjCharacter _this, ulong objId)
        {
            if (objId == _this.ObjId)
            {
                return;
            }

            if (!_this.EnemyList.Contains(objId))
            {
                _this.EnemyList.Add(objId);
            }
        }

        //移除敌人
        public virtual void RemoveEnemy(ObjCharacter _this, ulong objId)
        {
            if (_this.EnemyList.Contains(objId))
            {
                _this.EnemyList.Remove(objId);
            }
        }

        //把自己从别人的敌人列表里清除
        public virtual void RemoveMeFromOtherEnemyList(ObjCharacter _this)
        {
            foreach (var id in _this.EnemyList)
            {
                var obj = _this.Scene.FindCharacter(id);
                if (null != obj)
                {
                    obj.RemoveEnemy(_this.ObjId);
                }
            }
        }

        //清除敌人列表
        public virtual void ClearEnemy(ObjCharacter _this)
        {
            _this.EnemyList.Clear();
        }

        //获取奖励的归属
        public ObjCharacter GetRewardOwner(ObjCharacter _this)
        {
	        if (_this.GetObjType() == ObjType.RETINUE)
	        {
		        return (_this as ObjRetinue).Owner;
	        }
            return _this;
        }

        #endregion
    }

    public partial class ObjCharacter : ObjBase
    {
        private static IObjCharacter mImpl;
        public const float MOVESPEED_RATE = 0.01f;

        static ObjCharacter()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (ObjCharacter), typeof (ObjCharacterDefaultImpl),
                o => { mImpl = (IObjCharacter) o; });
        }

        public override void Destroy()
        {
            mImpl.Destroy(this);
        }

        //输出成一个objdata用于客户端创建
        public override ObjData DumpObjData(ReasonType reason)
        {
            return mImpl.DumpObjData(this, reason);
        }

        /// <summary>
        ///     强制移动到目标点
        /// </summary>
        /// <param name="pos">目标位置</param>
        /// <param name="speed">移动速度</param>
        public void ForceMoveTo(Vector2 pos, float speed)
        {
            mImpl.ForceMoveTo(this, pos, speed);
        }

        public new static IObjCharacter GetImpl()
        {
            return mImpl;
        }

        //Obj类型
        public override bool IsCharacter()
        {
            return true;
        }

        //是否无敌免疫Buff
        public virtual bool IsInvisible()
        {
            return mImpl.IsInvisible(this);
        }

        //是否正在移动
        public virtual bool IsMoving()
        {
            return mImpl.IsMoving(this);
        }

        //是否骑马中
        public virtual bool IsRiding()
        {
            return mImpl.IsRiding(this);
        }

        //是否是我的敌方
        public virtual bool IsMyEnemy(ObjCharacter character)
        {
            return mImpl.IsMyEnemy(this, character);
        }

        //我是否能被其他obj看见
        public override bool IsVisibleTo(ObjBase obj)
        {
            return mImpl.IsVisibleTo(this, obj);
        }

        //停止移动
        public virtual void StopMove(bool broadcast = true)
        {
            mImpl.StopMove(this, broadcast);
        }

        public virtual void UpdateDbAttribute(eAttributeType type)
        {
            mImpl.UpdateDbAttribute(this, type);
        }


        #region 基本属性

        //名字
        public string mName;

        //阵营
        public int mCamp = -1;

        //队伍
        public ulong mTeamId;


        //普通属性，不存数据库的
        public NormalAttr NormalAttr = new NormalAttr();

        #endregion

        #region 逻辑数据

        public ulong mLogicTickCount;

        //死亡状态
        public bool mIsDead;

        //选择目标
        public ulong mSelectTarget;

        //是否正在移动
        public bool mWaitingToMove;
        public bool mIsMoving;

        //目标点
        public List<Vector2> mTargetPos = new List<Vector2>();

        //是否在强制移动
        public bool mIsForceMoving;

        //强制移动目标点（用于击飞，击退等被控制的情况）
        public Vector2 mForceMoveSpeed;

        //强制移动的剩余时间
        public float mForceMovingTime;

        //敌人列表
        public List<ulong> EnemyList = new List<ulong>();

        //模型ID，不为-1就造这个模型，否则就用表里的
        private int mModelId = -1;

        public int ModelId
        {
            get { return mModelId; }
            set { mModelId = value; }
        }

        #endregion

        #region 表格数据

        public CharacterBaseRecord TableCharacter { get; set; }

        //阵营表
        public CampRecord TableCamp { get; set; }

        #endregion

        #region 属性技能数据

        //基本属性
        public FightAttr Attr;

        //Buff列表
        public BuffList BuffList;

        //技能管理器
        public SkillManager Skill;
        public Dictionary<int, int> SkillExdata = new Dictionary<int, int>(); //技能扩展数据记录，用于记录不同ID的技能连击
        public Dictionary<int, Trigger> SkillExdataReset = new Dictionary<int, Trigger>(); //技能连击的重置倒计时
        public Dictionary<int,bool> FlagData = new Dictionary<int, bool>();

        public bool GetFlag(int idx)
        {
            return mImpl.GetFlag(this,idx);
        }

        public void SetFlag(int idx, bool b)
        {
            mImpl.SetFlag(this,idx,b);
        }
        public void OverTime(int eId)
        {
            mImpl.OverTime(this, eId);
        }

        public void StartTime(int eId)
        {
            mImpl.StartTime(this, eId);
        }

        public int GetExdata(int eId)
        {
            return mImpl.GetExdata(this, eId);
        }

        public void SetExdata(int eId, int value)
        {
            mImpl.SetExdata(this, eId, value);
        }

        public void AddExdata(int eId, int value)
        {
            mImpl.AddExdata(this, eId, value);
        }

        //玩家的装备数据
        public Dictionary<int, ItemEquip2> Equip = new Dictionary<int, ItemEquip2>();

        public void GetEquipsModel(Dictionary<int, int> equipsModel)
        {
            mImpl.GetEquipsModel(this, equipsModel);
        }

        //获得属性
        public int GetAttribute(eAttributeType nIndex)
        {
            return mImpl.GetAttribute(this, nIndex);
        }

        #endregion

        #region 初始化

        // 构造函数

        public virtual void Init(ulong characterId, int dataId, int level)
        {
            mImpl.Init(this, characterId, dataId, level);
        }

        //初始化表格数据
        public virtual int InitTableData(int level)
        {
            return mImpl.InitTableData(this, level);
        }

        #endregion

        #region 基本属性方法

        //名字
        public virtual void SetName(string name)
        {
            mImpl.SetName(this, name);
        }

        public virtual string GetName()
        {
            return mImpl.GetName(this);
        }

        //获得等级
        public virtual int GetLevel()
        {
            return mImpl.GetLevel(this);
        }

        public void SyncCharacterPostion()
        {
            mImpl.SyncCharacterPostion(this);
        }

        /// <summary>
        ///     设置等级
        /// </summary>
        /// <returns></returns>
        public void SetLevel(int nValue)
        {
            mImpl.SetLevel(this, nValue);
        }

        // 把某个怪的等级和属性都调整到对应的level
        public void SetToLevel(int lv)
        {
            mImpl.SetToLevel(this, lv);
        }

        //阵营
        public virtual void SetCamp(int camp)
        {
            mImpl.SetCamp(this, camp);
        }

        public virtual int GetCamp()
        {
            return mImpl.GetCamp(this);
        }

        //队伍
        public virtual void SetTeamId(ulong teamId, int state)
        {
            mImpl.SetTeamId(this, teamId, state);
        }

        public virtual ulong GetTeamId()
        {
            return mImpl.GetTeamId(this);
        }

        public virtual float GetMoveSpeed()
        {
            return mImpl.GetMoveSpeed(this);
        }

        #endregion

        #region 逻辑数据方法

        //死亡状态
        public virtual bool IsDead()
        {
            return mImpl.IsDead(this);
        }

        //朝向
        public void TurnFaceTo(Vector2 pos)
        {
            mImpl.TurnFaceTo(this, pos);
        }

        #endregion

        #region 装备数据(增删改查)

        public void EquipModelStateChange(int nPart, int nState, ItemBaseData equip)
        {
            mImpl.EquipModelStateChange(this, nPart, nState, equip);
        }

        //装备发生了变化 nType=变化规则{0删除，1新增，2修改} nPart=部位   Equip=新装备数据
        public void EquipChange(int nType, int nPart, ItemBaseData equip)
        {
            mImpl.EquipChange(this, nType, nPart, equip);
        }

        //增加装备（穿上装备）
        public void AddEquip(int nPart, ItemEquip2 equip)
        {
            mImpl.AddEquip(this, nPart, equip);
        }

        //删除装备（脱下装备）
        public void DelEquip(int nPart, bool refreshAttr = true)
        {
            mImpl.DelEquip(this, nPart, refreshAttr);
        }

        public void AddEquipBuff(ItemEquip2 equip)
        {
            mImpl.AddEquipBuff(this, equip);            
        }

        public void RemoveEquipBuff(ItemEquip2 equip)
        {
            mImpl.RemoveEquipBuff(this, equip);
        }

        public void RemoveCantMoveBuff()
        {
            mImpl.RemoveCantMoveBuff(this);
        }

        //重置装备
        public void ResetEquip(int nPart, ItemEquip2 equip)
        {
            mImpl.ResetEquip(this, nPart, equip);
        }

        //获得装备数据
        public ItemEquip2 GetEquip(int nPart)
        {
            return mImpl.GetEquip(this, nPart);
        }

        #endregion

        #region elf
        public Dictionary<int, int> ElfBuffDict = new Dictionary<int, int>();
        public int ElfFightPoint = 0;

        public void ElfChange(List<int> removeBuff, Dictionary<int, int> addBuff, int fightPoint)
        {
            mImpl.ElfChange(this, removeBuff, addBuff, fightPoint);
        }

        public int GetElfSkillFightPoint()
        {
            return mImpl.GetElfSkillFightPoint(this);
        }

        public void AddElfBuff(int buffId, int buffLevel)
        {
            mImpl.AddElfBuff(this, buffId, buffLevel);
        }
        public void RemoveElfBuff(int buffId)
        {
            mImpl.RemoveElfBuff(this, buffId);
        }

	    public ulong GetTargetCharacterId()
	    {
			return mImpl.GetTargetCharacterId(this);
	    }

	    public void SetTargetCharacterId(ulong id)
	    {
			mImpl.SetTargetCharacterId(this,id);
	    }

        public void RideMount(int mountId)
        {
            mImpl.RideMount(this, mountId);
        }

		public virtual bool CanBeAttacked()
		{
			return mImpl.CanBeAttacked(this);
		}
        #endregion

        #region 同步数据

        public class SourceBinding
        {
            public bool Dirty = true;
            public Func<byte[]> Getter;
        }

        public bool mSyncDirtyFlag = true;

        public Dictionary<INotifyPropertyChanged, Dictionary<uint, SourceBinding>> mDirtyFlag =
            new Dictionary<INotifyPropertyChanged, Dictionary<uint, SourceBinding>>();

        public delegate void PropertyChangedEventHandler(INotifyPropertyChanged sender, PropertyChangedEventArgs args);

        public interface INotifyPropertyChanged
        {
            event PropertyChangedEventHandler PropertyChanged;
        }

        public class PropertyChangedEventArgs
        {
            public PropertyChangedEventArgs(uint id)
            {
                Id = id;
            }

            public uint Id;
        }

        public static List<SceneSyncDataItem> sEmptySyncData = new List<SceneSyncDataItem>();
        public List<SceneSyncDataItem> mSyncData = new List<SceneSyncDataItem>();

        public List<SceneSyncDataItem> GetSyncData(int c = 0)
        {
            if (!mSyncDirtyFlag)
            {
                return sEmptySyncData;
            }

            mSyncData.Clear();
            foreach (var s in mDirtyFlag)
            {
                foreach (var sync in s.Value)
                {
                    if (sync.Value.Dirty)
                    {
                        mSyncData.Add(new SceneSyncDataItem
                        {
                            Data = sync.Value.Getter(),
                            CharacterId = mObjId,
                            Id = sync.Key
                        });

                        sync.Value.Dirty = false;
                    }
                }
            }

            mSyncDirtyFlag = false;

            return mSyncData;
        }

        public void PropertyChangedHandler(INotifyPropertyChanged obj, PropertyChangedEventArgs args)
        {
            Dictionary<uint, SourceBinding> dict;
            SourceBinding binding;
            if (mDirtyFlag.TryGetValue(obj, out dict))
            {
                if (dict.TryGetValue(args.Id, out binding))
                {
                    binding.Dirty = true;
                    mSyncDirtyFlag = true;
                }
            }
        }

        public void AddSyncData(uint id, INotifyPropertyChanged holder, Func<int> getter)
        {
            AddSyncData(id, holder, () =>
            {
                var buffer = new byte[4];
                SerializerUtility.WriteInt(buffer, getter());
                return buffer;
            });
        }

        public void AddSyncData(uint id, INotifyPropertyChanged holder, Func<byte[]> getter)
        {
            mImpl.AddSyncData(this, id, holder, getter);
        }

        public void AddSyncData(uint id, INotifyPropertyChanged holder, Func<string> getter)
        {
            AddSyncData(id, holder, () =>
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, new TString {Data = getter()});
                    return ms.ToArray();
                }
            });
        }


        public void RemoveAllSyncData()
        {
            mImpl.RemoveAllSyncData(this);
        }

        public void RegisterAllSyncData()
        {
            mImpl.RegisterAllSyncData(this);
        }

        public bool OnSyncRequested(ulong characterId, uint syncId)
        {
            return mImpl.OnSyncRequested(this, characterId, syncId);
        }

        #endregion

        #region 角色移动相关

        /// <summary>
        ///     移动角色（寻路是异步的）
        /// </summary>
        /// <param name="pos">目标位置</param>
        /// <param name="offset">目标位置偏移</param>
        /// <param name="searchPath">是否需要寻路，false表示直线移动到目的地</param>
        /// <param name="pushLastPos">是否需要精确移动到目标点，因为寻路是以格子中心点为坐标的，最后可能会产生半格的误差</param>
        /// <param name="callback">寻路结果callback</param>
        /// <returns>能走返回true，不能走返回false</returns>
        public virtual MoveResult MoveTo(Vector2 pos,
                                         float offset = 0.05f,
                                         bool searchPath = true,
                                         bool pushLastPos = false,
                                         Action<List<Vector2>> callback = null)
        {
            return mImpl.MoveTo(this, pos, offset, searchPath, pushLastPos, callback);
        }

        public void SetCheckPostion(Vector2 pos)
        {
            mImpl.SetCheckPostion(this, pos);
        }

        public virtual bool Move(List<Vector2> path)
        {
            return mImpl.Move(this, path);
        }


        public virtual void OnMoveBegin()
        {
            mImpl.OnMoveBegin(this);
        }

        //当移动停止时
        public virtual void OnMoveEnd()
        {
            mImpl.OnMoveEnd(this);
        }

        public virtual void Tick_Move(float delta)
        {
            mImpl.Tick_Move(this, delta);
        }

        public override void Tick(float delta)
        {
            mImpl.Tick(this, delta);
        }

        //设置坐标
        public override void SetPosition(Vector2 p)
        {
            mImpl.SetPosition(this, p);
        }

        public override void SetPosition(float x, float y)
        {
            mImpl.SetPosition(this, x, y);
        }

        public virtual void ProcessPositionChanged()
        {
            mImpl.ProcessPositionChanged(this);
        }

        public void UpdateZone()
        {
            mImpl.UpdateZone(this);
        }

        public virtual void UpdateTriggerArea()
        {
            mImpl.UpdateTriggerArea(this);
        }

        #endregion

        #region 场景相关

        public override void OnEnterScene()
        {
            mImpl.OnEnterScene(this);
        }

        public override void OnLeaveScene()
        {
            mImpl.OnLeaveScene(this);
        }

        #endregion

        #region 效果相关

        public virtual void OnDamage(ObjCharacter enemy, int damage)
        {
            mImpl.OnDamage(this, enemy, damage);
        }

		public virtual void OnTrapped(ObjCharacter enemy)
		{
			mImpl.OnTrapped(this, enemy);
		}
        public virtual void OnMakeDamageTo(ObjCharacter target, int damage)
        {
            mImpl.OnMakeDamageTo(this, target, damage);
        }

        public virtual void Die(ulong characterId, int delayView, int damage = 0)
        {
            mImpl.Die(this, characterId, delayView, damage);
        }

        public void OnlineDie()
        {
            mImpl.OnlineDie(this);
        }

        public virtual void OnDie(ulong characterId, int delayView, int damage = 0)
        {
            mImpl.OnDie(this, characterId, delayView, damage);
        }

        public virtual void OnEnemyDie(ObjCharacter obj)
        {
            mImpl.OnEnemyDie(this, obj);
        }

        //重置
        public override void Reset()
        {
            mImpl.Reset(this);
        }

        public virtual void ResetAttribute()
        {
            mImpl.ResetAttribute(this);
        }

        //重生
        public virtual void Relive(bool byItem = false)
        {
			mImpl.Relive(this, byItem);
        }

        #endregion

        #region Enemy

        //添加obj到我的敌人列表
        public virtual void AddEnemy(ulong objId)
        {
            mImpl.AddEnemy(this, objId);
        }

        //移除敌人
        public virtual void RemoveEnemy(ulong objId)
        {
            mImpl.RemoveEnemy(this, objId);
        }

        //把自己从别人的敌人列表里清除
        public virtual void RemoveMeFromOtherEnemyList()
        {
            mImpl.RemoveMeFromOtherEnemyList(this);
        }

        //清除敌人列表
        public virtual void ClearEnemy()
        {
            mImpl.ClearEnemy(this);
        }

        //获取奖励的归属
        public ObjCharacter GetRewardOwner()
        {
            return mImpl.GetRewardOwner(this);
        }

        #endregion
    }
}