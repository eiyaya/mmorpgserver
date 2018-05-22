#region using

using System;
using System.Collections.Generic;
using System.Linq;
using DataContract;
using DataTable;
using NLog;
using Shared;

#endregion

namespace Scene
{
    public interface IBuffData
    {
        void AddDuration(BuffData _this, float fDuration);
        ObjCharacter GetCaster(BuffData _this);
        int GetLastSeconds(BuffData _this);
        void Init(BuffData _this);
        void OnDestroy(BuffData _this);

        void Reset(BuffData _this,
                   uint id,
                   int dataid,
                   int bufflevel,
                   ObjCharacter caster,
                   BuffRecord tbbuff,
                   ObjCharacter bear,
                   eHitType hitType,
                   float fBili);

        int AbsorbDamage(BuffData _this, int damage, byte effectIndex, ref int remainValue);
        void SetActive(BuffData _this, bool bActive);
        void SetCaster(BuffData _this, ObjCharacter caster);
        void SetDuration(BuffData _this, float fDuration, bool bNewTrigger = false);
    }

    public class BuffDataDefaultImpl : IBuffData
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region  数据结构

        public void Reset(BuffData _this,
                          uint id,
                          int dataid,
                          int bufflevel,
                          ObjCharacter caster,
                          BuffRecord tbbuff,
                          ObjCharacter bear,
                          eHitType hitType,
                          float fBili)
        {
            _this.mId = id;
            _this.m_nBuffId = dataid;
            _this.m_nLevel = bufflevel;
            SetCaster(_this, caster);
            _this.m_fDuration = (float) tbbuff.Duration/1000;
            _this.m_Bear = bear;
            _this.m_nLayer = 1;
            _this.m_nHuchi = tbbuff.HuchiId;
            _this.m_HitType = hitType;
            _this.m_fModify = fBili;
            _this.m_bActive = true;
            _this.m_nUpdataCount = 0;
            _this.m_Flag.CleanFlag(0);
	        _this.mCoolDownTime = DateTime.MinValue;
            _this.RemainAbsorbDict.Clear();
            for (var i = 0; i < tbbuff.effectid.Length; ++i)
            {
                var effectId = tbbuff.effectid[i];
                var absorbMax = tbbuff.effectparam[i, 2];
                if (effectId == (int)eEffectType.AbsorbInjury && absorbMax > 0)
                {
                    _this.RemainAbsorbDict[(byte)i] = absorbMax;
                }
            }
        }

        #endregion

        #region  初始化

        public void Init(BuffData _this)
        {
            var tbBuff = _this.mBuff;
            if (tbBuff.Duration != 0)
            {
                foreach (var i in tbBuff.effectpoint)
                {
                    //Buff不需要心跳
                    if (i == -1)
                    {
                        continue;
                    }
                    if (BitFlag.GetAnd(i, 14336) == 0)
                    {
                        continue;
                    }
                    StarUpdata(_this);
                    return;
                }
                if (tbBuff.Duration > 0)
                {
                    StarTrigger(_this);
                }
            }
        }

        #endregion

        #region  基础方法

        public ObjCharacter GetCaster(BuffData _this)
        {
            ObjCharacter result;
            _this.m_Caster.TryGetTarget(out result);
            return result;
        }

        public void SetCaster(BuffData _this, ObjCharacter caster)
        {
            if (_this.m_Caster == null)
            {
                _this.m_Caster = new WeakReference<ObjCharacter>(caster);
            }
            else
            {
                _this.m_Caster.SetTarget(caster);
            }
            if (caster != null)
            {
                _this.mCasterId = caster.ObjId;
            }
        }

        public void SetActive(BuffData _this, bool bActive)
        {
            _this.m_bActive = bActive;
            if (!bActive)
            {
                if (_this.m_UpdataTrigger != null)
                {
                    SceneServerControl.Timer.DeleteTrigger(_this.m_UpdataTrigger);
                    _this.m_UpdataTrigger = null;
                }
                if (_this.mOverTrigger != null)
                {
                    SceneServerControl.Timer.DeleteTrigger(_this.mOverTrigger);
                    _this.mOverTrigger = null;
                }
            }
        }

        public void SetDuration(BuffData _this, float fDuration, bool bNewTrigger = false)
        {
            //float diffitem = fDuration - m_fDuration;
            _this.m_fDuration = fDuration;
            if (!bNewTrigger)
            {
                return;
            }
            if (_this.mOverTrigger != null)
            {
                SceneServerControl.Timer.ChangeTime(ref _this.mOverTrigger, DateTime.Now.AddSeconds(fDuration));
            }
            if (_this.m_UpdataTrigger != null)
            {
                _this.m_nUpdataCount = 0;
                //SceneServerControl.Timer.ChangeTime(m_UpdataTrigger, DateTime.Now.AddSeconds(1));
            }
        }

        public void AddDuration(BuffData _this, float fDuration)
        {
            _this.m_fDuration += fDuration;
            if (_this.mOverTrigger != null)
            {
                var nextTime = SceneServerControl.Timer.GetNextTime(_this.mOverTrigger);
                SceneServerControl.Timer.ChangeTime(ref _this.mOverTrigger, nextTime.AddSeconds(fDuration));
            }
        }

        public int GetLastSeconds(BuffData _this)
        {
            if (_this.m_fDuration < 0.01)
            {
                return -1;
            }
            if (_this.mOverTrigger != null)
            {
                var overTime = SceneServerControl.Timer.GetNextTime(_this.mOverTrigger);
                if (overTime > DateTime.Now)
                {
                    return (int) (overTime - DateTime.Now).TotalSeconds;
                }
                return -1;
            }
            if (_this.m_nUpdataCount >= _this.m_fDuration*1000)
            {
                return -1;
            }
            return (int) _this.m_fDuration - _this.m_nUpdataCount/1000;
        }

        public void OnDestroy(BuffData _this)
        {
            if (_this.Retinue != null && _this.GetBear() != null)
            {
                _this.GetBear().RemoveRetinue(_this.Retinue);
                _this.Retinue = null;
            }
            SetCaster(_this, null);
            _this.SetBear(null);
            if (_this.mOverTrigger != null)
            {
                SceneServerControl.Timer.DeleteTrigger(_this.mOverTrigger);
                _this.mOverTrigger = null;
            }
            if (_this.m_UpdataTrigger != null)
            {
                SceneServerControl.Timer.DeleteTrigger(_this.m_UpdataTrigger);
                _this.m_UpdataTrigger = null;
            }
            _this.mId = 0;
            ObjectPool<BuffData>.Release(_this);
            //ObjectPool<BuffData>.Release(this);
	        _this.mCoolDownTime = DateTime.MinValue;
        }

        public int GetCanAbsorb(BuffData _this, int effectIndex, int damage)
        {
            var absorbPercent = _this.mBuff.effectparam[effectIndex, 1];   // 吸收万分比
            var canAbsorbValue = (int)((long)damage * absorbPercent / 10000); // 可以吸收的值(不考虑GetLayer())
            return canAbsorbValue;
        }

        public int AbsorbDamage(BuffData _this, int damage, byte effectIndex, ref int remainValue)
        {
            if (!_this.RemainAbsorbDict.TryGetValue(effectIndex, out remainValue))
                return 0;

            if (remainValue <= 0)
                return 0;

            var canAbsorbValue = GetCanAbsorb(_this, effectIndex, damage);
            var absorbValue = Math.Min(canAbsorbValue, remainValue); // 实际吸收值            

            remainValue = _this.RemainAbsorbDict.modifyValue(effectIndex, -absorbValue);    // 修改buff
            return absorbValue;
        }

        #endregion

        #region  持续相关

        private void StarTrigger(BuffData _this)
        {
            //Logger.Info("StarTrigger by Buffid={0}",m_nBuffId);
            _this.mOverTrigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(_this.m_fDuration),
                () =>
                {
                    _this.mOverTrigger = null;
                    TimeOver(_this);
                });
        }

        private void TimeOver(BuffData _this)
        {
            if (_this.Retinue != null)
            {
                if (_this.GetBear() != null)
                {
                    _this.GetBear().RemoveRetinue(_this.Retinue);
                }
                _this.Retinue = null;
            }
            //if (m_Bear is ObjPlayer)
            //{
            //    Logger.Info("TimeOver   by Buffid={0}", m_nBuffId);
            //}
            MissBuff.DoEffect(_this.m_Bear.Scene, _this.m_Bear, _this);
            //if (m_Bear is ObjPlayer)
            //{
            //    Logger.Info("MissBuff   by Buffid={0}", m_nBuffId);
            //}
            if (_this.m_Bear != null)
            {
                _this.m_Bear.DeleteBuff(_this, eCleanBuffType.TimeOver);
                _this.m_Bear.BuffList.Do_Del_Buff();
            }
            //if (m_Bear is ObjPlayer)
            //{
            //    Logger.Info("DeleteBuff by Buffid={0}", m_nBuffId);
            //}
            //if ((ulong)m_UpdataTrigger > 0)
            //{
            //    SceneServerControl.Timer.DeleteTrigger(m_UpdataTrigger);
            //}
            _this.mOverTrigger = null;
            _this.m_UpdataTrigger = null;
        }

        #endregion

        #region  心跳相关

        private void StarUpdata(BuffData _this)
        {
            Logger.Info("StarUpdata by Buffid={0}", _this.m_nBuffId);
            _this.m_UpdataTrigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(1),
                () => { OnEventTime(_this); }, BuffData.UpdataMillisecond);
        }

        private void OnEventTime(BuffData _this)
        {
            if (_this.m_bActive == false)
            {
                return;
            }
            if (_this.m_Bear == null)
            {
                Logger.Warn("buffdata OnEventTime Bear is null!! guid={1},buffid={0}", _this.m_nBuffId, _this.mId);
                if (_this.m_UpdataTrigger != null)
                {
                    SceneServerControl.Timer.DeleteTrigger(_this.m_UpdataTrigger);
                    _this.m_UpdataTrigger = null;
                }
            }
            _this.m_nUpdataCount += BuffData.UpdataMillisecond;
            if (_this.m_nUpdataCount%1000 == 0)
            {
                if (_this.m_Bear != null)
                {
                    SecondOne.DoEffect(_this.m_Bear.Scene, _this.m_Bear, _this);
                }
            }
            if (_this.m_nUpdataCount%3000 == 0)
            {
                if (_this.m_Bear != null)
                {
                    SecondThree.DoEffect(_this.m_Bear.Scene, _this.m_Bear, _this);
                }
            }
            if (_this.m_nUpdataCount%5000 == 0)
            {
                if (_this.m_Bear != null)
                {
                    SecondFive.DoEffect(_this.m_Bear.Scene, _this.m_Bear, _this);
                }
            }
            //m_UpdataTrigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(1), OnEventTime);
            if (_this.mBuff.Duration != -1 && _this.m_nUpdataCount >= _this.m_fDuration*1000)
            {
                if (_this.m_UpdataTrigger != null)
                {
                    SceneServerControl.Timer.DeleteTrigger(_this.m_UpdataTrigger);
                    TimeOver(_this);
                }
                else
                {
                    TimeOver(_this);
                }
                //m_UpdataTrigger = null;
            }
        }

        #endregion
    }

    public class BuffData
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger(); //
        private static IBuffData mImpl;
        public static int UpdataMillisecond = 1000;

        static BuffData()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (BuffData), typeof (BuffDataDefaultImpl),
                o => { mImpl = (IBuffData) o; });
        }

        #region  初始化

        public void Init()
        {
            mImpl.Init(this);
        }

        #endregion

        #region  数据结构

        //唯一ID
        public uint mId;
        //配置ID
        public int m_nBuffId { get; set; }
        //等级
        public int m_nLevel { get; set; }
        //持续时间(秒数)
        public float m_fDuration { get; set; }
        //释放者
        public WeakReference<ObjCharacter> m_Caster;
        public ulong mCasterId;
        //承受者
        public ObjCharacter m_Bear { get; set; }
        //层数
        public int m_nLayer = 1;
        //是否失效
        public bool m_bActive = true;
        //事件是否生效过
        public BitFlag m_Flag = new BitFlag((int) eEffectEventType.EVENT_COUNT, 0);
        //结束的触发器Id
        public Trigger mOverTrigger;
        //心跳的触发器Id
        public Trigger m_UpdataTrigger;
        //Buff的已经持续心跳次数
        public int m_nUpdataCount { get; set; }
        //对伤害或治疗的强制影响系数
        public float m_fModify = 1.0f;
        //对伤害或治疗的命中结果影响系数
        public eHitType m_HitType = eHitType.Hit;
        //互斥ID
        public int m_nHuchi = -1;
        //表格ID
        public BuffRecord mBuff;
        public ObjRetinue Retinue;
        public Dictionary<byte, int> RemainAbsorbDict = new Dictionary<byte, int>();   // 吸收伤害值(效果索引，上限)
	    public DateTime mCoolDownTime = DateTime.MinValue;
        public void Reset(uint id,
                          int dataid,
                          int bufflevel,
                          ObjCharacter caster,
                          BuffRecord tbbuff,
                          ObjCharacter bear,
                          eHitType hitType,
                          float fBili)
        {
            mImpl.Reset(this, id, dataid, bufflevel, caster, tbbuff, bear, hitType, fBili);
        }

        #endregion

        #region  基础方法

        public int GetBuffId()
        {
            return m_nBuffId;
        }

        public void SetBuffId(int nBuffId)
        {
            m_nBuffId = nBuffId;
        }

        public int GetLayer()
        {
            return m_nLayer;
        }

        public void SetLayer(int nLayer)
        {
            m_nLayer = nLayer;
        }

        public ObjCharacter GetCaster()
        {
            return mImpl.GetCaster(this);
        }

        public void SetCaster(ObjCharacter caster)
        {
            mImpl.SetCaster(this, caster);
        }

        public ObjCharacter GetBear()
        {
            return m_Bear;
        }

        public void SetBear(ObjCharacter bear)
        {
            m_Bear = bear;
        }

        public bool GetActive()
        {
            return m_bActive;
        }

		public bool IsCoolDown()
		{
			return DateTime.Now>=mCoolDownTime;
		}

	    public void AddCoolDownTime()
	    {
		    if (null!=mBuff && mBuff.CoolDownTime > 0)
		    {
			    mCoolDownTime = DateTime.Now.AddMilliseconds(mBuff.CoolDownTime);
		    }
	    }

        public void SetActive(bool bActive)
        {
            mImpl.SetActive(this, bActive);
        }

        public float GetDuration()
        {
            return m_fDuration;
        }

        public void SetDuration(float fDuration, bool bNewTrigger = false)
        {
            mImpl.SetDuration(this, fDuration, bNewTrigger);
        }

        public void AddDuration(float fDuration)
        {
            mImpl.AddDuration(this, fDuration);
        }

        public int GetLastSeconds()
        {
            return mImpl.GetLastSeconds(this);
        }

        public void OnDestroy()
        {
            mImpl.OnDestroy(this);
        }

        public int AbsorbDamage(int damage, byte effectIndex, ref int remainValue)
        {
            return mImpl.AbsorbDamage(this, damage, effectIndex, ref remainValue);
        }

        #endregion
    }

    public interface IBuffList
    {
        BuffData AddBuff(BuffList _this,
                         int buffId,
                         int bufflevel,
                         ObjCharacter caster,
                         ObjCharacter bear,
                         float fBili,
                         eHitType hitType);

        int Calculate(BuffList _this, ObjCharacter obj, eAttributeType type, ref double dBili);
        List<BuffData> CopyBuff(BuffList _this);
        bool DelBuff(BuffList _this, BuffData thisbuff);
        void DelBuffByOnDie(BuffList _this);
        void DeleteBuff(BuffList _this);
        void Do_Del_Buff(BuffList _this);
        BuffData Get_Event_Buff(BuffList _this, eEffectEventType ebuffeventtype);
        BuffData Get_Same_Caster_Huchi_Buff(BuffList _this, int huchiid, ObjCharacter sameCaster = null);
        BuffData Get_Same_Caster_Same_Buff(BuffList _this, int buffId, ObjCharacter sameCaster);
        List<BuffData> GetBuffById(BuffList _this, int buffid);
        BuffData GetCasterBuff(BuffList _this, int buffid, ObjCharacter caster);
        bool GetEffectParam_And(BuffList _this, eEffectType effectid, int paramid, int value);
        bool GetEffectParam_Bin(BuffList _this, eEffectType effectid, int paramid, int value);
        uint GetNextUniqueId(BuffList _this);
        bool IsHaveBuffById(BuffList _this, int buffid);
        bool IsHaveEffectId(BuffList _this, eEffectType effectid);
        bool IsNoMove(BuffList _this);
        void ModifyBuffTableData(ObjCharacter casterHero, BuffData buff, int buffId, int buffLevel);
        void OnDestroy(BuffList _this);
        void OnEnterScene(BuffList _this);
        void OnLeaveScene(BuffList _this);
        void OnLost(BuffList _this);
        void SetSpecialStateFlag(BuffList _this);
		void SetSpecialStateNoMove(BuffList _this, ObjCharacter caster);
        void UpdataBuff_Flag(BuffList _this, BuffData buff);
    }

    public class BuffListDefaultImpl : IBuffList
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region  属性计算相关

        //计算属性
        public int Calculate(BuffList _this, ObjCharacter obj, eAttributeType type, ref double dBili)
        {
            var value = 0;
            foreach (var i in _this.mData)
            {
                if (i.GetActive() == false)
                {
                    continue;
                }
                var tbBuff = i.mBuff;
                var effectCount = tbBuff.effectid.Length;
                for (var j = 0; j != effectCount; ++j)
                {
                    var param = tbBuff.effectparam;
                    if (tbBuff.effectid[j] != (int) eEffectType.RefAttr)
                    {
                        continue;
                    }
                    if (type != (eAttributeType) param[j, 0])
                    {
                        continue;
                    }
                    value += param[j, 3]*i.GetLayer();
                    if (param[j, 1] == -1)
                    {
                        continue;
                    }
                    if (param[j, 1] != param[j, 0])
                    {
                        value +=
                            (int)
                                (obj.Attr.GetDataValue((eAttributeType) param[j, 1])*
                                 ((double) param[j, 2]*i.GetLayer()/10000));
                    }
                    else
                    {
                        dBili = dBili*(10000 + param[j, 2]*i.GetLayer())/10000;
                    }
                }
            }
            return value;
        }

        #endregion

        #region  删除相关

        public void OnEnterScene(BuffList _this)
        {
            if (_this.mRetinueBuff != null)
            {
                var obj = _this.mRetinueBuff.GetBear();
                GetBuff.DoEffect(obj.Scene, obj, _this.mRetinueBuff, 0);
                _this.mRetinueBuff = null;
            }
        }

        public void OnLeaveScene(BuffList _this)
        {
            //_this.mRemoveBuff.Clear();
            _this.mRetinueBuff = null;
            foreach (var data in _this.mData)
            {
                if (data.Retinue != null && data.GetBear() != null)
                {
                    _this.mRetinueBuff = data;
                    data.GetBear().RemoveRetinue(data.Retinue);
                    data.Retinue = null;
                }
                if (data.mBuff.SceneDisappear == 1 && data.GetActive())
                {
                    MissBuff.DoEffect(_this.mObj.Scene, _this.mObj, data);
                    data.SetActive(false);
                }
            }
            Do_Del_Buff(_this);
            DeleteBuff(_this);
        }

        public void OnLost(BuffList _this)
        {
            //_this.mRemoveBuff.Clear();
            _this.mRetinueBuff = null;
            foreach (var data in _this.mData)
            {
                if (data.Retinue != null && data.GetBear() != null)
                {
                    _this.mRetinueBuff = data;
                    data.GetBear().RemoveRetinue(data.Retinue);
                    data.Retinue = null;
                }
                if (data.mBuff.DownLine == 1 && data.GetActive())
                {
                    MissBuff.DoEffect(_this.mObj.Scene, _this.mObj, data);
                    data.SetActive(false);
                }
            }
            Do_Del_Buff(_this);
            DeleteBuff(_this);
        }

        //析构
        public void OnDestroy(BuffList _this)
        {
            DeleteBuff(_this);
            foreach (var buff in _this.mData)
            {
                try
                {
                    buff.OnDestroy();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
            }
            _this.mData.Clear();
        }

        //删除已经添加到移除列表，但是还没有还给池的
        public void DeleteBuff(BuffList _this)
        {
            foreach (var buffData in _this.mRemoveBuff)
            {
                try
                {
                    buffData.OnDestroy();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
            }
            _this.mRemoveBuff.Clear();
        }

        //删除打了标记的Buff
        public void Do_Del_Buff(BuffList _this)
        {
            foreach (var buffData in _this.mData)
            {
                if (!buffData.GetActive())
                {
                    _this.mRemoveBuff.Add(buffData);
                    //buffData.OnDestroy();
                }
            }
            _this.mData.RemoveAll(h => h.GetActive() == false);
            _this.mObj.MarkDbDirty();
        }

        //删除buff
        public bool DelBuff(BuffList _this, BuffData thisbuff)
        {
            if (thisbuff == null)
            {
                return false;
            }
            thisbuff.SetActive(false);
            return true;
        }

        //死亡时时取消Buff
        public void DelBuffByOnDie(BuffList _this)
        {
            foreach (var data in _this.mData)
            {
                if (data.mBuff.DieDisappear == 1)
                {
                    MissBuff.DoEffect(_this.mObj.Scene, _this.mObj, data);
                    _this.mObj.DeleteBuff(data, eCleanBuffType.Die);
                }
            }
            //Do_Del_Buff(_this);
            //DeleteBuff(_this);
        }

        #endregion

        #region  标记相关

        //获得一个某事件是干净标记的Buff
        public List<BuffData> CopyBuff(BuffList _this)
        {
            var bufflist = new List<BuffData>();
            foreach (var buff in _this.mData)
            {
                if (buff.GetActive())
                {
                    bufflist.Add(buff);
                }
            }
            return bufflist;
        }

        //获得一个某事件是干净标记的Buff
        public BuffData Get_Event_Buff(BuffList _this, eEffectEventType ebuffeventtype)
        {
            foreach (var buff in _this.mData)
            {
                if (buff.m_Flag.GetFlag((int) ebuffeventtype) == 0)
                {
                    buff.m_Flag.SetFlag((int) ebuffeventtype);
                    return buff;
                }
            }
            return null;
        }

        //刷新Buff的事件触发标记
        public void UpdataBuff_Flag(BuffList _this, BuffData buff)
        {
            buff.m_Flag.ReSetAllFlag();
        }

        //判断是否可以移动
        public bool IsNoMove(BuffList _this)
        {
            if (!_this.mSpecialStateFlag)
            {
                return _this._mNoMove;
            }
            _this.mSpecialStateFlag = false;
            _this._mNoMove = GetEffectParam_Bin(_this, eEffectType.SpecialState, 0, 0);
            return _this._mNoMove;
        }

        //设置特殊状态改变标记
		public void SetSpecialStateNoMove(BuffList _this, ObjCharacter caster)
        {
            _this.mSpecialStateFlag = false;
            _this._mNoMove = true;
            if (_this.mObj.IsMoving())
            {
                _this.mObj.StopMove();
	            if (_this.mObj.GetObjType() == ObjType.PLAYER)
	            {
		            var player = _this.mObj as ObjPlayer;
					player.SendForceStopMove();
	            }
            }
			_this.mObj.OnTrapped(caster);
        }

        //设置特殊状态改变标记
        public void SetSpecialStateFlag(BuffList _this)
        {
            _this.mSpecialStateFlag = true;
        }

        #endregion

        #region  buff相关

        //获取UniqueId
        public uint GetNextUniqueId(BuffList _this)
        {
            return BuffList.mUniqueId++;
        }

        //增加buff
        public BuffData AddBuff(BuffList _this,
                                int buffId,
                                int bufflevel,
                                ObjCharacter caster,
                                ObjCharacter bear,
                                float fBili,
                                eHitType hitType)
        {
#if DEBUG
            if (caster is ObjPlayer)
            {
                Logger.Info("id={0}给{1}释放了{2}的Buff", caster.ObjId, bear.ObjId, buffId);
            }
#endif
            var tbbuff = Table.GetBuff(buffId);

            var buff = ObjectPool<BuffData>.NewObject();
            //var buff = ObjectPool<BuffData>.NewObject();
            buff.mBuff = caster.Skill.ModifyBuff(tbbuff, bufflevel);
            buff.Reset(GetNextUniqueId(_this), buffId, bufflevel, caster, buff.mBuff, bear, hitType, fBili);
            //mBuff = tbbuff,
            buff.Init();
            _this.mData.Add(buff);
            _this.mObj.MarkDbDirty();
            return buff;
        }

        //修改Buff表格数据
        public void ModifyBuffTableData(ObjCharacter casterHero, BuffData buff, int buffId, int buffLevel)
        {
            buff.m_nLevel = buffLevel;
            var tbbuff = Table.GetBuff(buffId);
            buff.mBuff = casterHero.Skill.ModifyBuff(tbbuff, buffLevel);
        }

        //获得某BuffId的Buff
        public List<BuffData> GetBuffById(BuffList _this, int buffid)
        {
            var buffs = new List<BuffData>();
            foreach (var buff in _this.mData)
            {
                if (buff.GetBuffId() == buffid)
                {
                    buffs.Add(buff);
                }
            }
            return buffs;
        }

        //获得某BuffId的Buff
        public bool IsHaveBuffById(BuffList _this, int buffid)
        {
            var buffs = new List<BuffData>();
            foreach (var buff in _this.mData)
            {
                if (buff.GetBuffId() == buffid)
                {
                    return true;
                }
            }
            return false;
        }

        //查询相同释放者的某BuffID的Buff
        public BuffData GetCasterBuff(BuffList _this, int buffid, ObjCharacter caster)
        {
            foreach (var buff in _this.mData)
            {
                if (buff.GetActive() == false)
                {
                    continue;
                }
                if (buff.GetBuffId() == buffid && buff.mCasterId == caster.ObjId)
                {
                    return buff;
                }
            }
            return null;
        }

        //查询相同释放者的互斥ID
        public BuffData Get_Same_Caster_Huchi_Buff(BuffList _this, int huchiid, ObjCharacter sameCaster = null)
        {
            if (sameCaster == null)
            {
                foreach (var buff in _this.mData)
                {
                    if (buff.GetActive() == false)
                    {
                        continue;
                    }
                    if (buff.m_nHuchi == huchiid)
                    {
                        return buff;
                    }
                }
                return null;
            }
            foreach (var buff in _this.mData)
            {
                if (buff.GetActive() == false)
                {
                    continue;
                }
                if (buff.m_nHuchi == huchiid && buff.mCasterId == sameCaster.ObjId)
                {
                    return buff;
                }
            }
            return null;
        }

        //查询相同释放者的同BuffId
        public BuffData Get_Same_Caster_Same_Buff(BuffList _this, int buffId, ObjCharacter sameCaster)
        {
            foreach (var buff in _this.mData)
            {
                if (buff.GetActive() == false)
                {
                    continue;
                }
                if (buff.m_nBuffId == buffId && buff.mCasterId == sameCaster.ObjId)
                {
                    return buff;
                }
            }
            return null;
        }

        //查询是否有某个效果ID
        public bool IsHaveEffectId(BuffList _this, eEffectType effectid)
        {
            foreach (var i in _this.mData)
            {
                if (i.GetActive() == false)
                {
                    continue;
                }
                var tbBuff = i.mBuff;
                for (var j = 0; j != tbBuff.effectid.Length; ++j)
                {
                    if (tbBuff.effectid[j] == (int) effectid)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        //获得BUFF中某个效果ID的某个参数进行(与操作)
        public bool GetEffectParam_And(BuffList _this, eEffectType effectid, int paramid, int value)
        {
            foreach (var i in _this.mData)
            {
                if (i.GetActive() == false)
                {
                    continue;
                }
                var tbBuff = i.mBuff;
                for (var j = 0; j != tbBuff.effectid.Length; ++j)
                {
                    if (tbBuff.effectid[j] == (int) effectid)
                    {
                        if (BitFlag.GetAnd(tbBuff.effectparam[j, paramid], value) > 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        //获得BUFF中某个效果ID的某个参数(二进制检查)
        public bool GetEffectParam_Bin(BuffList _this, eEffectType effectid, int paramid, int value)
        {
            foreach (var i in _this.mData)
            {
                if (i.GetActive() == false)
                {
                    continue;
                }
                var tbBuff = i.mBuff;
                for (var j = 0; j != tbBuff.effectid.Length; ++j)
                {
                    if (tbBuff.effectid[j] == (int) effectid)
                    {
                        if (BitFlag.GetLow(tbBuff.effectparam[j, paramid], value))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        #endregion
    }

    //玩家身上的Buff结构
    public class BuffList
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger(); //
        private static IBuffList mImpl;
        public static uint mUniqueId;

        static BuffList()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (BuffList), typeof (BuffListDefaultImpl),
                o => { mImpl = (IBuffList) o; });
        }

        public bool _mNoMove;
        public List<BuffData> mData = new List<BuffData>();
        public ObjCharacter mObj;
        public List<BuffData> mRemoveBuff = new List<BuffData>();
        public bool mSpecialStateFlag;

        #region  属性计算相关

        //计算属性
        public int Calculate(ObjCharacter obj, eAttributeType type, ref double dBili)
        {
            return mImpl.Calculate(this, obj, type, ref dBili);
        }

        #endregion

        #region  存储

        public void SaveDB()
        {
        }

        #endregion

        public override string ToString()
        {
            return string.Join(",", mData.Select(i => i.m_nBuffId));
        }

        #region  初始化

        //用第一次创建
        public void InitByBase(ObjCharacter character)
        {
            mObj = character;
        }

        //用数据库数据
        public void InitByDB( /*DBSkill BuffData*/)
        {
        }

        #endregion

        #region  删除相关

        public void OnEnterScene()
        {
            mImpl.OnEnterScene(this);
        }

        public BuffData mRetinueBuff;

        public void OnLeaveScene()
        {
            mImpl.OnLeaveScene(this);
        }

        public void OnLost()
        {
            mImpl.OnLost(this);
        }

        //析构
        public void OnDestroy()
        {
            mImpl.OnDestroy(this);
        }

        //删除已经添加到移除列表，但是还没有还给池的
        public void DeleteBuff()
        {
            mImpl.DeleteBuff(this);
        }

        //删除打了标记的Buff
        public void Do_Del_Buff()
        {
            mImpl.Do_Del_Buff(this);
        }

        //删除buff
        public bool DelBuff(BuffData thisbuff)
        {
            return mImpl.DelBuff(this, thisbuff);
        }

        //死亡时时取消Buff
        public void DelBuffByOnDie()
        {
            mImpl.DelBuffByOnDie(this);
        }

        #endregion

        #region  标记相关

        //获得一个某事件是干净标记的Buff
        public List<BuffData> CopyBuff()
        {
            return mImpl.CopyBuff(this);
        }

        //获得一个某事件是干净标记的Buff
        public BuffData Get_Event_Buff(eEffectEventType ebuffeventtype)
        {
            return mImpl.Get_Event_Buff(this, ebuffeventtype);
        }

        //刷新Buff的事件触发标记
        public void UpdataBuff_Flag(BuffData buff)
        {
            mImpl.UpdataBuff_Flag(this, buff);
        }

        //判断是否可以移动
        public bool IsNoMove()
        {
            return mImpl.IsNoMove(this);
        }

        //设置特殊状态改变标记
        public void SetSpecialStateNoMove(ObjCharacter caster)
        {
			mImpl.SetSpecialStateNoMove(this, caster);
        }

        //设置特殊状态改变标记
        public void SetSpecialStateFlag()
        {
            mImpl.SetSpecialStateFlag(this);
        }

        #endregion

        #region  buff相关

        //获取UniqueId
        public uint GetNextUniqueId()
        {
            return mImpl.GetNextUniqueId(this);
        }

        //增加buff
        public BuffData AddBuff(int buffId,
                                int bufflevel,
                                ObjCharacter caster,
                                ObjCharacter bear,
                                float fBili,
                                eHitType hitType)
        {
            return mImpl.AddBuff(this, buffId, bufflevel, caster, bear, fBili, hitType);
        }

        //修改Buff表格数据
        public static void ModifyBuffTableData(ObjCharacter casterHero, BuffData buff, int buffId, int buffLevel)
        {
            mImpl.ModifyBuffTableData(casterHero, buff, buffId, buffLevel);
        }

        //获得某BuffId的Buff
        public List<BuffData> GetBuffById(int buffid)
        {
            return mImpl.GetBuffById(this, buffid);
        }

        //获得某BuffId的Buff
        public bool IsHaveBuffById(int buffid)
        {
            return mImpl.IsHaveBuffById(this, buffid);
        }

        //查询相同释放者的某BuffID的Buff
        public BuffData GetCasterBuff(int buffid, ObjCharacter caster)
        {
            return mImpl.GetCasterBuff(this, buffid, caster);
        }

        //查询相同释放者的互斥ID
        public BuffData Get_Same_Caster_Huchi_Buff(int huchiid, ObjCharacter sameCaster = null)
        {
            return mImpl.Get_Same_Caster_Huchi_Buff(this, huchiid, sameCaster);
        }

        //查询相同释放者的同BuffId
        public BuffData Get_Same_Caster_Same_Buff(int buffId, ObjCharacter sameCaster)
        {
            return mImpl.Get_Same_Caster_Same_Buff(this, buffId, sameCaster);
        }

        //查询是否有某个效果ID
        public bool IsHaveEffectId(eEffectType effectid)
        {
            return mImpl.IsHaveEffectId(this, effectid);
        }

        //获得BUFF中某个效果ID的某个参数进行(与操作)
        public bool GetEffectParam_And(eEffectType effectid, int paramid, int value)
        {
            return mImpl.GetEffectParam_And(this, effectid, paramid, value);
        }

        //获得BUFF中某个效果ID的某个参数(二进制检查)
        public bool GetEffectParam_Bin(eEffectType effectid, int paramid, int value)
        {
            return mImpl.GetEffectParam_Bin(this, effectid, paramid, value);
        }

        #endregion
    }
}