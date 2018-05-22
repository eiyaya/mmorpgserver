#region using

using System.Collections.Generic;
using DataContract;
using NLog;
using Shared;

#endregion

namespace Scene
{
    public interface IBuffEvent
    {
        bool CheckEventResetHitType(eEffectEventType eventType);

        void DoBuff(Scene scene,
                    ObjCharacter obj,
                    BuffData buff,
                    int delayView,
                    eEffectEventType eventType,
                    int Param = 0,
                    ObjCharacter otherObj = null,
                    int checkParam = 0);
    }

    public class BuffEventDefaultImpl : IBuffEvent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); //

        public bool CheckEffectOk(BuffData buff, ObjCharacter bearObj, eEffectEventType eventType, int effectIndex, int param)
        {
            if (buff == null)
            {
                return false;                
            }
            
            var tbBuff = buff.mBuff;
            if (tbBuff.effectid[effectIndex] == -1 || tbBuff.effectpoint[effectIndex] == -1)
            {
                return false;
            }
            if (!BitFlag.GetLow(tbBuff.effectpoint[effectIndex], (int)eventType))
            {
                return false;
            }

            switch (eventType)
            {
                case eEffectEventType.HpLessPercent:
                {
                    var nowHp = bearObj.Attr.GetDataValue(eAttributeType.HpNow);
                    var maxHp = bearObj.Attr.GetDataValue(eAttributeType.HpMax);
                    if (nowHp * 10000L > maxHp * tbBuff.EffectPointParam[effectIndex]) // 是否血量百分比触发
                    {
                        return false;
                    }
                }
                    break;
                case eEffectEventType.Critical:
                case eEffectEventType.WasCrit:
                {
                    var condition = tbBuff.EffectPointParam[effectIndex];
                    var hitType = param;
                    if (!BitFlag.GetLow(condition, hitType))
                    {
                        return false;
                    }
                }
                    break;
            }

            return true;
        }

        //Buff所在场景,承受者,Buff实例,触发点,特定参数(伤害值,治疗值),另一个个Obj参数（释放者，击杀者等等不一定）
        public void DoBuff(Scene scene,
                           ObjCharacter obj,
                           BuffData buff,
                           int delayView,
                           eEffectEventType eventType,
                           int Param = 0,
                           ObjCharacter otherObj = null,
                           int checkParam = 0)
        {
            if (null == buff)
                return;

            if (!buff.GetActive())
            {
                return; //没有激活的Buff不生效
            }
	        if (!buff.IsCoolDown())
	        {
		        return;
	        }
            //该BUFF产生的伤害统计
            var thisbuffdamage = new Dictionary<int, int>();
            //该BuFF产生的治疗统计
            var thisbuffhealth = new Dictionary<int, int>();
            //该BuFF产生的回蓝统计
            var thisbuffmana = new Dictionary<int, int>();
            var tb_buff = buff.mBuff;
            for (var j = 0; j != tb_buff.effectid.Length; ++j)
            {
                if (!CheckEffectOk(buff, obj, eventType, j, checkParam))
                {
                    continue;                    
                }

                //执行BUFF效果需要
                switch ((eEffectType) tb_buff.effectid[j])
                {
                    case eEffectType.DoDamage:
                    {
                        BuffEffect.DO_DAMAGE(thisbuffdamage, scene, obj, buff, j, CheckEventResetHitType(eventType));
                    }
                        break;
                    case eEffectType.DoHealth:
                    {
                        BuffEffect.DO_HEALTH(thisbuffhealth, scene, obj, buff, j, CheckEventResetHitType(eventType));
                    }
                        break;
                    case eEffectType.RefAttr:
                        BuffEffect.REF_ATTR(scene, obj, buff, j);
                        break;
                    case eEffectType.PositionChange:
                        BuffEffect.POSITION_CHANGE(scene, obj, buff, j);
                        break;
                    case eEffectType.ProAddBuff:
                        BuffEffect.PRO_ADD_BUFF(scene, obj, buff, j);
                        break;
                    case eEffectType.DamageHealth:
                    {
                        if (Param > 0)
                        {
                            BuffEffect.DAMAGE_HEALTH(scene, obj, Param, buff, j);
                        }
                    }
                        break;
                    case eEffectType.DispelBuff:
                    {
                        BuffEffect.DispelBuff(eventType, scene, obj, buff, j);
                    }
                        break;
                    case eEffectType.SpecialState:
                    {
                        BuffEffect.SPEIALSTATE(eventType, scene, obj, buff, j);
                    }
                        break;
                    case eEffectType.CreateMonsterType1:
                    {
                        BuffEffect.CREATER_MONSTER1TYPE(scene, obj, buff, j);
                    }
                        break;
                    case eEffectType.ModifySkill:
                    {
                        BuffEffect.NO_MODIFYSKILL(eventType, scene, obj, buff, j);
                    }
                        break;
                    case eEffectType.ModifyBuff:
                    {
                        BuffEffect.NO_MODIFYBUFF(eventType, scene, obj, buff, j);
                    }
                        break;
                    case eEffectType.ModifyModel:
                    {
                        BuffEffect.NO_MODIFYMODEL(eventType, obj, buff);
                    }
                        break;
                    case eEffectType.CreateMonster:
                    {
                        BuffEffect.CREATER_MONSTER(eventType, scene, obj, buff, j);
                    }
                        break;
                    case eEffectType.ExpModify:
                    {
                        BuffEffect.EXP_MODIFY(eventType, scene, obj, buff, j);
                    }
                        break;
                    case eEffectType.ExpModify2:
                    {
                        BuffEffect.EXP_MODIFY2(eventType, scene, obj, buff, j);
                    }
                        break;
                    case eEffectType.DoMana:
                    {
                        BuffEffect.DO_MANA(thisbuffmana, scene, obj, buff, j, CheckEventResetHitType(eventType));
                    }
                        break;
                    case eEffectType.KillSelf:
                    {
                        BuffEffect.Kill_SELF(scene, obj, buff, j);
                    }
                        break;
                    case eEffectType.DoSkill:
                    {
                        BuffEffect.DoSkill(scene, obj, otherObj, buff, j);
                    }
                        break;
					case eEffectType.HpInRangeTriggerBuff:
						{
							BuffEffect.HP_TRIGGER_ADDBUFF(scene, obj, buff, j);
						}
						break;
                    case eEffectType.AddExp:
                    {
                        BuffEffect.EXP_ADD(eventType,scene, obj, buff, j);
                        break; 
                    }
                }

	            buff.AddCoolDownTime();
            }

            CalcAttrDamage(thisbuffdamage, buff);

            if (thisbuffdamage.Count > 0 || thisbuffhealth.Count > 0 || thisbuffmana.Count > 0)
            {
                var caster = buff.GetCaster();
                if (caster != null)
                {
                    var replyMsg = new BuffResultMsg();
                    //发送伤害
                    foreach (var thisdamage in thisbuffdamage)
                    {
                        var damagevalue = thisdamage.Value;
                        var absorbDamage = 0;
                        obj.DoDamage(eHitType.Hit, buff, caster, delayView, ref damagevalue, thisdamage.Key, ref absorbDamage);

                        //if (absorbDamage > 0)
                        //{ // 吸收
                        //    replyMsg.buff.Add(new BuffResult
                        //    {
                        //        SkillObjId = caster.ObjId,
                        //        TargetObjId = obj.ObjId,
                        //        BuffTypeId = buff.GetBuffId(),
                        //        Type = BuffType.HT_NODAMAGE,
                        //        Damage = damagevalue,
                        //        ViewTime = Extension.AddTimeDiffToNet(delayView)
                        //    });
                        //}

                        if (damagevalue > 0)
                        {
                            if (thisdamage.Key == (int)eDamageType.FireAttr)
                            {
                                var result = NetBuffResult(scene, buff, thisdamage.Key, damagevalue, BuffType.HT_Fire_DAMAGE, delayView);
                                replyMsg.buff.Add(result);
                                continue;
                            }
                            else if (thisdamage.Key == (int)eDamageType.IceAttr)
                            {
                                var result = NetBuffResult(scene, buff, thisdamage.Key, damagevalue, BuffType.HT_Ice_DAMAGE, delayView);
                                replyMsg.buff.Add(result);
                                continue;
                            }
                            else if (thisdamage.Key == (int)eDamageType.PoisonAttr)
                            {
                                var result = NetBuffResult(scene, buff, thisdamage.Key, damagevalue, BuffType.HT_Poison_DAMAGE, delayView);
                                replyMsg.buff.Add(result);
                                continue;
                            }

                            CauseCrit.DoEffect(scene, caster, buff, damagevalue);
                            WasCrit.DoEffect(scene, obj, buff, damagevalue);
                            //Logger.Info("伤害类型:{0},伤害值:{1}", thisdamage.Key, damagevalue);
                            if (buff.m_HitType == eHitType.Hit)
                            {
                                var result = NetBuffResult(scene, buff, thisdamage.Key, damagevalue, BuffType.HT_NORMAL, delayView);
                                replyMsg.buff.Add(result);
                            }
                            else if (buff.m_HitType == eHitType.Lucky)
                            {
                                var result = NetBuffResult(scene, buff, thisdamage.Key, damagevalue, BuffType.HT_CRITICAL, delayView);
                                replyMsg.buff.Add(result);
                            }
                            else if (buff.m_HitType == eHitType.Excellent)
                            {
                                var result = NetBuffResult(scene, buff, thisdamage.Key, damagevalue, BuffType.HT_EXCELLENT, delayView);
                                replyMsg.buff.Add(result);
                            }
                            //击中回复生效
                            var HitRecoveryValue = caster.GetAttribute(eAttributeType.HitRecovery);
                            if (HitRecoveryValue > 0)
                            {
                                var healthvalue = HitRecoveryValue;
                                caster.DoHealth(eHitType.Hit, buff, caster, ref healthvalue, 8);
                                //Logger.Info("治疗类型:{0},治疗值:{1}",8, healthvalue);
                                var nowHp = caster.GetAttribute(eAttributeType.HpNow);
                                if (scene != null && scene.isNeedDamageModify)
                                {
                                    if (Scene.IsNeedChangeHp(caster) != null)
                                    {
                                        nowHp = (int) (nowHp/scene.BeDamageModify);
                                    }
                                }
                                replyMsg.buff.Add(new BuffResult
                                {
                                    SkillObjId = caster.ObjId,
                                    TargetObjId = caster.ObjId,
                                    BuffTypeId = buff.GetBuffId(),
                                    Type = BuffType.HT_HEALTH,
                                    Damage = healthvalue,
                                    ViewTime = Extension.AddTimeDiffToNet(delayView),
                                    Param = {0, nowHp}
                                });
                            }
                            //本Buff的吸血效果（给释放方的)
                            for (var j = 0; j != tb_buff.effectid.Length; ++j)
                            {
                                if (tb_buff.effectid[j] == 18 && tb_buff.effectparam[j, 1] == 0) //有吸血效果，并且是释放者
                                {
                                    BuffEffect.DAMAGE_HEALTH(scene, obj, damagevalue, buff, j, thisbuffhealth);
                                }
                            }
                            //伤害反弹
                            if (thisdamage.Key != (int) eDamageType.Rebound && thisdamage.Key != (int) eDamageType.Blood)
                            {
                                var DamageReboundValue = obj.GetAttribute(eAttributeType.DamageReboundPro);
                                if (DamageReboundValue > 0)
                                {
                                    var damageReboundValue = damagevalue*DamageReboundValue/10000;
                                    if (damageReboundValue > 0)
                                    {
                                        //caster.DoHealth(eHitType.Hit, buff, caster, ref healthvalue, 8);
                                        caster.DoRealDamage(eHitType.Hit, buff, obj, delayView, ref damageReboundValue,
                                            (int) eDamageType.Rebound);
                                        //Logger.Info("伤害类型:{0},伤害值:{1}", eDamageType.Rebound, damageReboundValue);
                                        var nowHp = caster.GetAttribute(eAttributeType.HpNow);
                                        if (scene != null && scene.isNeedDamageModify)
                                        {
                                            if (Scene.IsNeedChangeHp(caster) != null)
                                            {
                                                nowHp = (int) (nowHp/scene.BeDamageModify);
                                            }
                                        }
                                        replyMsg.buff.Add(new BuffResult
                                        {
                                            SkillObjId = obj.ObjId,
                                            TargetObjId = caster.ObjId,
                                            BuffTypeId = buff.GetBuffId(),
                                            Type = BuffType.HT_REBOUND,
                                            Damage = damageReboundValue,
                                            ViewTime = Extension.AddTimeDiffToNet(delayView),
                                            Param = { 0, nowHp }
                                        });
                                    }
                                }
                            }
                        }
                        else if (absorbDamage <= 0)
                        {
//免疫
                            replyMsg.buff.Add(new BuffResult
                            {
                                SkillObjId = caster.ObjId,
                                TargetObjId = obj.ObjId,
                                BuffTypeId = buff.GetBuffId(),
                                Type = BuffType.HT_NODAMAGE,
                                Damage = damagevalue,
                                ViewTime = Extension.AddTimeDiffToNet(delayView)
                            });
                        }
                    }
                    //发送治疗
                    foreach (var thishealth in thisbuffhealth)
                    {
                        if (obj.GetAttribute(eAttributeType.HpNow) == obj.GetAttribute(eAttributeType.HpMax))
                        {
                            continue;
                        }
                        var healthvalue = thishealth.Value;
                        obj.DoHealth(eHitType.Hit, buff, caster, ref healthvalue, thishealth.Key);
                        //Logger.Info("治疗类型:{0},治疗值:{1}", thishealth.Key, healthvalue);
                        var nowHp = obj.GetAttribute(eAttributeType.HpNow);
                        if (scene != null && scene.isNeedDamageModify)
                        {
                            if (Scene.IsNeedChangeHp(obj) != null)
                            {
                                nowHp = (int) (nowHp/scene.BeDamageModify);
                            }
                        }
                        replyMsg.buff.Add(new BuffResult
                        {
                            SkillObjId = caster.ObjId,
                            TargetObjId = obj.ObjId,
                            BuffTypeId = buff.GetBuffId(),
                            Type = BuffType.HT_HEALTH,
                            Damage = healthvalue,
                            ViewTime = Extension.AddTimeDiffToNet(delayView),
                            Param = {0, nowHp}
                        });
                    }

                    //发送回蓝
                    foreach (var thishealth in thisbuffmana)
                    {
                        var healthvalue = thishealth.Value;
                        obj.DoMana(eHitType.Hit, buff, caster, ref healthvalue, thishealth.Key);
                        //Logger.Info("回蓝类型:{0},回蓝值:{1}", thishealth.Key, healthvalue);
                        replyMsg.buff.Add(new BuffResult
                        {
                            SkillObjId = caster.ObjId,
                            TargetObjId = obj.ObjId,
                            BuffTypeId = buff.GetBuffId(),
                            Type = BuffType.HT_MANA,
                            Damage = healthvalue,
                            ViewTime = Extension.AddTimeDiffToNet(delayView)
                        });
                    }
                    obj.BroadcastBuffList(replyMsg);
                }
            }
        }

        public void CalcAttrDamage(Dictionary<int, int> totalDamage, BuffData buff)
        {
            if (totalDamage.Count > 0)
            { // 有伤害数
                var caster = buff.GetCaster();
                var bear = buff.GetBear();

                var damage = caster.Attr.GetAttrDamageValue(bear, eAttributeType.FireAttack);
                if (damage > 0)
                {
                    totalDamage[(int)eDamageType.FireAttr] = damage;
                }
                damage = caster.Attr.GetAttrDamageValue(bear, eAttributeType.IceAttack);
                if (damage > 0)
                {
                    totalDamage[(int)eDamageType.IceAttr] = damage;
                }
                damage = caster.Attr.GetAttrDamageValue(bear, eAttributeType.PoisonAttack);
                if (damage > 0)
                {
                    totalDamage[(int)eDamageType.PoisonAttr] = damage;
                }
            }            
        }

        public BuffResult NetBuffResult(Scene scene, BuffData buff, int damageType, int damageValue, BuffType type, int delayView)
        {
            var caster = buff.GetCaster();
            var bear = buff.GetBear();
            var nowHp = bear.GetAttribute(eAttributeType.HpNow);
            if (scene != null && scene.isNeedDamageModify)
            {
                if (Scene.IsNeedChangeHp(bear) != null)
                {
                    nowHp = (int)(nowHp / scene.BeDamageModify);
                }
            }

            var result = new BuffResult
            {
                SkillObjId = caster.ObjId,
                TargetObjId = bear.ObjId,
                BuffTypeId = buff.GetBuffId(),
                Type = type,
                Damage = damageValue,
                ViewTime = Extension.AddTimeDiffToNet(delayView),
                Param = { damageType, nowHp }
            };

            return result;
        }

        //看事件点是否需要重置Buff的命中
        public bool CheckEventResetHitType(eEffectEventType eventType)
        {
            if (eventType == eEffectEventType.SecondOne || eventType == eEffectEventType.SecondThree ||
                eventType == eEffectEventType.SecondFive)
            {
                return true;
            }
            return false;
        }
    }

    #region     技能事件统一中间处理层，触发各种效果

    public static class BuffEvent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static IBuffEvent mImpl;

        static BuffEvent()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (BuffEvent), typeof (BuffEventDefaultImpl),
                o => { mImpl = (IBuffEvent) o; });
        }

        //看事件点是否需要重置Buff的命中
        public static bool CheckEventResetHitType(eEffectEventType eventType)
        {
            return mImpl.CheckEventResetHitType(eventType);
        }

        //Buff所在场景,承受者,Buff实例,触发点,特定参数(伤害值,治疗值),另一个个Obj参数（释放者，击杀者等等不一定）
        public static void DoBuff(Scene scene,
                                  ObjCharacter obj,
                                  BuffData buff,
                                  int delayView,
                                  eEffectEventType eventType,
                                  int Param = 0,
                                  ObjCharacter otherObj = null,
                                  int checkParam = 0)
        {
            mImpl.DoBuff(scene, obj, buff, delayView, eventType, Param, otherObj, checkParam);
        }
    }

    #endregion

    #region     //获得时 = 0

    public static class GetBuff
    {
        public static eEffectEventType m_sType = eEffectEventType.GetBuff;

        public static void DoEffect(Scene scene, ObjCharacter target, BuffData buff, int DelayView)
        {
            BuffEvent.DoBuff(scene, target, buff, DelayView, m_sType);
        }
    }

    #endregion

    #region     //消失时 = 1

    public static class MissBuff
    {
        public static eEffectEventType m_sType = eEffectEventType.MissBuff;

        public static void DoEffect(Scene scene, ObjCharacter target, BuffData buff)
        {
            BuffEvent.DoBuff(scene, target, buff, 0, m_sType);
        }
    }

    #endregion

    #region     //将要死亡时 = 2

    public static class WillDie
    {
        public static eEffectEventType m_sType = eEffectEventType.WillDie;

        public static void DoEffect(Scene scene, ObjCharacter target, BuffData buff, ObjCharacter caster, int damage)
        {
            var tempList = target.BuffList.CopyBuff();
            foreach (var dobuff in tempList)
            {
                if (dobuff.GetActive())
                {
                    BuffEvent.DoBuff(scene, target, dobuff, 0, m_sType, damage, caster);
                }
            }
        }
    }

    #endregion

    #region     //真正死亡时 = 3

    public static class RealDie
    {
        public static eEffectEventType m_sType = eEffectEventType.RealDie;

        public static void DoEffect(Scene scene, ObjCharacter target, BuffData buff, ObjCharacter caster, int damage)
        {
            var tempList = target.BuffList.CopyBuff();
            foreach (var dobuff in tempList)
            {
                if (dobuff.GetActive())
                {
                    BuffEvent.DoBuff(scene, target, dobuff, 0, m_sType, damage, caster);
                }
            }
        }
    }

    #endregion

    #region     //造成死亡时 = 4

    public static class CauseDie
    {
        public static eEffectEventType m_sType = eEffectEventType.CauseDie;

        public static void DoEffect(Scene scene, ObjCharacter target, BuffData buff, ObjCharacter caster, int damage)
        {
            var tempList = target.BuffList.CopyBuff();
            foreach (var dobuff in tempList)
            {
                if (dobuff.GetActive())
                {
                    BuffEvent.DoBuff(scene, caster, dobuff, 0, m_sType, damage, target);
                }
            }
        }
    }

    #endregion

    #region     //暴击时 = 5

    public static class CauseCrit
    {
        public static eEffectEventType m_sType = eEffectEventType.Critical;

        public static void DoEffect(Scene scene, ObjCharacter caster, BuffData buff, int damage)
        {
            if (buff.m_HitType < eHitType.Hit)
            {
                var tempList = caster.BuffList.CopyBuff();
                foreach (var dobuff in tempList)
                {
                    if (dobuff.mId != buff.mId)
                    {
                        BuffEvent.DoBuff(scene, caster, dobuff, 0, m_sType, damage, caster, (int)buff.m_HitType);
                    }
                }                
            }
        }
    }

    #endregion

    #region     //被暴击时 = 6

    public static class WasCrit
    {
        public static eEffectEventType m_sType = eEffectEventType.WasCrit;

        public static void DoEffect(Scene scene, ObjCharacter bear, BuffData buff, int damage)
        {
            if (buff.m_HitType < eHitType.Hit)
            {
                var tempList = bear.BuffList.CopyBuff();
                foreach (var dobuff in tempList)
                {
                    BuffEvent.DoBuff(scene, bear, dobuff, 0, m_sType, damage, bear, (int)buff.m_HitType);
                }
            }
        }
    }

    #endregion

    #region     //造成伤害时 = 7

    public static class CauseDamage
    {
        public static eEffectEventType m_sType = eEffectEventType.CauseDamage;

        public static void DoEffect(Scene scene, ObjCharacter target, ObjCharacter caster, int damage)
        {
            var tempList = caster.BuffList.CopyBuff();
            foreach (var dobuff in tempList)
            {
                if (dobuff.GetActive())
                {
                    BuffEvent.DoBuff(scene, caster, dobuff, 0, m_sType, damage, target);
                }
            }
        }
    }

    #endregion

    #region     //受到伤害时 = 8

    public static class BearDamage
    {
        public static eEffectEventType m_sType = eEffectEventType.BearDamage;

        public static void DoEffect(Scene scene, ObjCharacter target, ObjCharacter caster, int damage)
        {
            var tempList = target.BuffList.CopyBuff();
            foreach (var dobuff in tempList)
            {
                if (dobuff.GetActive())
                {
                    BuffEvent.DoBuff(scene, target, dobuff, 0, m_sType, damage, caster);
                }
            }
        }
    }

    #endregion

    #region     //造成治疗时 = 9

    public static class CauseHealth
    {
        public static eEffectEventType m_sType = eEffectEventType.CauseHealth;

        public static void DoEffect(Scene scene, ObjCharacter target, ObjCharacter caster, int health)
        {
            var tempList = target.BuffList.CopyBuff();
            foreach (var dobuff in tempList)
            {
                if (dobuff.GetActive())
                {
                    BuffEvent.DoBuff(scene, caster, dobuff, 0, m_sType, health, target);
                }
            }
        }
    }

    #endregion

    #region     //受到治疗时 = 10

    public static class BearHealth
    {
        public static eEffectEventType m_sType = eEffectEventType.BearHealth;

        public static void DoEffect(Scene scene, ObjCharacter target, ObjCharacter caster, int health)
        {
            var tempList = target.BuffList.CopyBuff();
            foreach (var dobuff in tempList)
            {
                if (dobuff.GetActive())
                {
                    BuffEvent.DoBuff(scene, target, dobuff, 0, m_sType, health, caster);
                }
            }
        }
    }

    #endregion

    #region     //每1秒钟 = 11

    public static class SecondOne
    {
        public static eEffectEventType m_sType = eEffectEventType.SecondOne;

        public static void DoEffect(Scene scene, ObjCharacter target, BuffData buff)
        {
            BuffEvent.DoBuff(scene, target, buff, 0, m_sType);
        }
    }

    #endregion

    #region     //每3秒钟 = 12

    public static class SecondThree
    {
        public static eEffectEventType m_sType = eEffectEventType.SecondThree;

        public static void DoEffect(Scene scene, ObjCharacter target, BuffData buff)
        {
            BuffEvent.DoBuff(scene, target, buff, 0, m_sType);
        }
    }

    #endregion

    #region     //每5秒钟 = 13

    public static class SecondFive
    {
        public static eEffectEventType m_sType = eEffectEventType.SecondFive;

        public static void DoEffect(Scene scene, ObjCharacter target, BuffData buff)
        {
            BuffEvent.DoBuff(scene, target, buff, 0, m_sType);
        }
    }

    #endregion

    #region     // 血量少于X%时 = 14

    public static class HpLessThanCentainPercent
    {
        public static eEffectEventType m_sType = eEffectEventType.HpLessPercent;

        public static void DoEffect(Scene scene, ObjCharacter target)
        {
            var tempList = target.BuffList.CopyBuff();
            foreach (var buff in tempList)
            {
                BuffEvent.DoBuff(scene, target, buff, 0, m_sType);
            }
        }
    }


	public static class CharacterTrappedEvent
	{
		public static eEffectEventType m_sType = eEffectEventType.OnTrapped;

		public static void DoEffect(Scene scene, ObjCharacter target, ObjCharacter caster)
		{
			var tempList = target.BuffList.CopyBuff();
			foreach (var buff in tempList)
			{
				BuffEvent.DoBuff(scene, target, buff, 0, m_sType);
			}
		}
	}
    #endregion
}