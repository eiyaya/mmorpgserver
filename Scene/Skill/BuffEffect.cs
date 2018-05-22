#region using

using System;
using System.Collections.Generic;
using DataContract;
using DataTable;
using Mono.GameMath;
using NLog;
using Shared;

#endregion

namespace Scene
{
    public interface IBuffEffect
    {
        void BEAR_REF_DAMAGE(ObjCharacter caster, ObjCharacter target, ref int damage, int damage_type);
        void BEAR_REF_HEALTH(ObjCharacter caster, ObjCharacter target, ref int health, int health_type);
        void CASTER_REF_DAMAGE(ObjCharacter caster, ObjCharacter target, ref int damage, int damage_type);
        void CASTER_REF_HEALTH(ObjCharacter caster, ObjCharacter target, ref int health, int health_type);
        void CREATER_MONSTER(eEffectEventType eventType, Scene scene, ObjCharacter caster, BuffData buff, int index);
        void CREATER_MONSTER1TYPE(Scene scene, ObjCharacter caster, BuffData buff, int index);

        void DAMAGE_HEALTH(Scene scene,
                           ObjCharacter caster,
                           int damage,
                           BuffData buff,
                           int index,
                           Dictionary<int, int> totlehealth = null);

        void DispelBuff(eEffectEventType eventType, Scene scene, ObjCharacter caster, BuffData buff, int index);
        int AbsorbInjury(ObjCharacter target, int damageType, ref int damage);

        void DO_DAMAGE(Dictionary<int, int> totledamage,
                       Scene scene,
                       ObjCharacter obj,
                       BuffData buff,
                       int index,
                       bool IsRestHitType) //场景，承受者，buff，index
            ;

        void DO_HEALTH(Dictionary<int, int> totlehealth,
                       Scene scene,
                       ObjCharacter obj,
                       BuffData buff,
                       int index,
                       bool IsRestHitType);

        void DO_MANA(Dictionary<int, int> totlehealth,
                     Scene scene,
                     ObjCharacter obj,
                     BuffData buff,
                     int index,
                     bool IsRestHitType);

        void EXP_MODIFY(eEffectEventType eventType, Scene scene, ObjCharacter obj, BuffData buff, int index);
        void EXP_MODIFY2(eEffectEventType eventType, Scene scene, ObjCharacter obj, BuffData buff, int index);
        void EXP_ADD(eEffectEventType eventType, Scene scene, ObjCharacter obj, BuffData buff, int index);
        Vector2 GetPosDiffPos(Scene scene, Vector2 oldpos, int jiaodu, float dis);
        void Kill_SELF(Scene scene, ObjCharacter caster, BuffData buff, int index);
        void NO_MODIFYBUFF(eEffectEventType eventType, Scene scene, ObjCharacter caster, BuffData buff, int index);
        void NO_MODIFYMODEL(eEffectEventType eventType, ObjCharacter caster, BuffData buff);
        void NO_MODIFYSKILL(eEffectEventType eventType, Scene scene, ObjCharacter caster, BuffData buff, int index);
        void POSITION_CHANGE(Scene scene, ObjCharacter obj, BuffData buff, int index);
        void PRO_ADD_BUFF(Scene scene, ObjCharacter obj, BuffData buff, int index);
        void REF_ATTR(Scene scene, ObjCharacter obj, BuffData buff, int index);
        void SPEIALSTATE(eEffectEventType eventType, Scene scene, ObjCharacter caster, BuffData buff, int index);
        void DoSkill(Scene scene, ObjCharacter obj, ObjCharacter target, BuffData buff, int index);
		void HP_TRIGGER_ADDBUFF(Scene scene, ObjCharacter obj, BuffData buff, int index);
    }

    public class BuffEffectDefaultImpl : IBuffEffect
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region     //伤害效果 = 0

        private int CalcDamage(int attrId, ObjCharacter caster, ObjCharacter bear, eDamageType damageType)
        {
            if (caster == null)
                return 0;

            var attrs = new List<int>();
            if (attrId >= (int)eAttributeType.AttrCount)
            {
                if (attrId == (int) eAttributeType.Attack)
                {
                    var typeId = caster.TypeId;
                    if (typeId == 0 || typeId == 2)
                    {
                        attrs.Add((int)eAttributeType.PhyPowerMax);
                    }
                    else if (typeId == 1)
                    {
                        attrs.Add((int)eAttributeType.MagPowerMax);                        
                    }
                    else
                    {
                        Logger.Error("CalcDamage have not implement typeId({0})", typeId);
                        return 0;
                    }
                }
                else
                {
                    Logger.Error("CalcDamage have not implement attrId({0})", attrId);
                    return 0;
                }
            }
            else
            {
                attrs.Add(attrId);
            }

            var damage = 0;
            for (var i = 0; i < attrs.Count; ++i)
            {
                damage += caster.Attr.GetDamageValue(bear, attrs[i], damageType);
            }

            return damage;
        }

        public void DO_DAMAGE(Dictionary<int, int> totledamage,
                              Scene scene,
                              ObjCharacter obj,
                              BuffData buff,
                              int index,
                              bool IsRestHitType) //场景，承受者，buff，index
        {
            if (buff.GetBear().IsInvisible())
            {
                return;
            }
            var caster = buff.GetCaster();
            //if (caster is ObjPlayer)
            //{
            //    Logger.Info("----------DO_DAMAGE----------BuffId={0},Index={1}", buff.GetBuffId(), index);
            //    PlayerLog.WriteLog(caster.ObjId, "----------DO_DAMAGE----------BuffId={0},Index={1}", buff.GetBuffId(), index);
            //}
            var tbBuff = buff.mBuff;
            var param = tbBuff.effectparam;
            double damage = MyRandom.Random(param[index, 4], param[index, 5]);
            var damageType = (eDamageType) param[index, 0];
            switch (damageType)
            {
                case eDamageType.Physical:
                case eDamageType.Magic:
                case eDamageType.IgnoreArmor:
                {
                    var bilizhi = param[index, 3];
                    if (bilizhi > 0)
                    {
                        var tempObj = buff.GetCaster(); //释放者
                        if (param[index, 1] == 1)
                        {
                            tempObj = obj; //承受者
                        }

                        var attrId = param[index, 2];
                        damage += (double)CalcDamage(attrId, tempObj, obj, damageType) * bilizhi / 10000;

                        //if ((eAttributeType) param[index, 2] == eAttributeType.PhyPowerMax)
                        //{
                        //    damage += (double)caster.Attr.GetDamageValue(obj, param[index, 2], (eDamageType)damageType) * bilizhi / 10000;
                        //}
                        //else if ((eAttributeType) param[index, 2] == eAttributeType.MagPowerMax)
                        //{
                        //    damage += (double)caster.Attr.GetDamageValue(obj, param[index, 2], (eDamageType)damageType) * bilizhi / 10000;
                        //}
                        //else
                        //{
                        //    damage += (double)tempObj.Attr.GetDataValue((eAttributeType)param[index, 2]) * bilizhi / 10000;
                        //}
                    }
                    else if (bilizhi < 0)
                    {
                        Logger.Warn("buff={0},index={1},param={2}, Less than 0 ", tbBuff.Id, index, 3);
                    }
                    //命中类型
                    var hitType = buff.m_HitType;
                    if (IsRestHitType)
                    {
                        hitType = caster.Attr.GetHitResult(obj, eSkillHitType.Normal);
                    }
                    if (hitType == eHitType.Excellent)
                    {
                        damage = (damage*buff.m_fModify*caster.Attr.GetDataValue(eAttributeType.ExcellentDamage)/10000);
                    }
                    else if (buff.m_HitType == eHitType.Lucky)
                    {
                        damage = (damage*buff.m_fModify*caster.Attr.GetDataValue(eAttributeType.LuckyDamage)/10000);
                    }
                    else
                    {
                        damage = (damage*buff.m_fModify);
                    }
                    //buff层数
                    damage = damage*buff.GetLayer();
                    //属性伤害附加(比例)
                    damage = damage*
                             (10000 + caster.Attr.GetDataValue(eAttributeType.DamageAddPro) -
                              obj.Attr.GetDataValue(eAttributeType.DamageResPro))/10000;
                    //属性伤害附加(固定)
                    damage = damage + caster.Attr.GetDataValue(eAttributeType.AddPower) -
                             obj.Attr.GetDataValue(eAttributeType.DamageResistance);
                }
                    break;
                case eDamageType.Ice:
                {
                    var bilizhi = param[index, 3];
                    if (bilizhi > 0 && param[index, 2] != -1)
                    {
                        var tempObj = buff.GetCaster(); //释放者
                        if (tempObj != null)
                        {
                            if (param[index, 1] == 1)
                            {
                                tempObj = obj; //承受者
                            }
                            damage += (double) tempObj.Attr.GetDataValue((eAttributeType) param[index, 2])*bilizhi/10000;
                        }
                    }
                    else if (bilizhi < 0)
                    {
                        Logger.Warn("buff={0},index={1},param={2}, Less than 0 ", tbBuff.Id, index, 3);
                    }
                    totledamage.modifyValue((int) damageType, (int) damage);
                    return;
                }
                case eDamageType.Blood:
                {
                    totledamage.modifyValue((int) damageType, (int) damage);
                    return;
                }
                case eDamageType.Rebound:
                    break;
                default:
                    damage = 0;
                    Logger.Warn("BuffEffect DO_DAMAGE buffId={0},damageType={1}", buff.GetBuffId(), damageType);
                    break;
            }


            //汇总伤害
            if (damage > 0)
            {
                totledamage.modifyValue((int) damageType, (int) damage);
            }
            else
            {
                totledamage.modifyValue((int) damageType, MyRandom.Random(1, caster.GetLevel()));
            }
        }

        #endregion

        #region     //治疗效果 = 1

        public void DO_HEALTH(Dictionary<int, int> totlehealth,
                              Scene scene,
                              ObjCharacter obj,
                              BuffData buff,
                              int index,
                              bool IsRestHitType)
        {
            PlayerLog.WriteLog(obj.ObjId, "----------DO_HEALTH----------BuffId={0},Index={1}", buff.GetBuffId(), index);
            var tbBuff = buff.mBuff;
            var param = tbBuff.effectparam;
            double health = MyRandom.Random(param[index, 4], param[index, 5]);
            var healthType = param[index, 0];
            var bilizhi = param[index, 3];
            var tempObj = buff.GetCaster(); //释放者
            if (bilizhi > 0)
            {
                if (tempObj != null)
                {
                    if (param[index, 1] == 1)
                    {
//承受者
                        tempObj = obj;
                    }
                    health += (ulong)(tempObj.Attr.GetDataValue((eAttributeType)param[index, 2])) * (ulong)(bilizhi) / 10000;
                }
            }

            var hitType = buff.m_HitType;
            if (IsRestHitType)
            {
                if (tempObj != null)
                {
                    hitType = tempObj.Attr.GetHitResult(obj, eSkillHitType.Normal);
                }
                else
                {
                    hitType = eHitType.Hit;
                }
            }
            if (tempObj != null)
            {
                if (hitType == eHitType.Excellent)
                {
                    health =
                        (int) (health*buff.m_fModify*tempObj.Attr.GetDataValue(eAttributeType.ExcellentDamage)/10000);
                }
                else if (hitType == eHitType.Lucky)
                {
                    health = (int) (health*buff.m_fModify*tempObj.Attr.GetDataValue(eAttributeType.LuckyDamage)/10000);
                }
                else
                {
                    health = (int) (health*buff.m_fModify);
                }
            }
            else
            {
                health = (int) (health*buff.m_fModify);
            }
            totlehealth.modifyValue(healthType, (int) (health*buff.GetLayer()));

            //if (totlehealth.ContainsKey(healthType))
            //{
            //    totlehealth[healthType] += (int)(health * buff.GetLayer());
            //}
            //else
            //{
            //    totlehealth[healthType] = (int)(health * buff.GetLayer());
            //}
        }

        #endregion

        #region     //修改属性 = 2

        public void REF_ATTR(Scene scene, ObjCharacter obj, BuffData buff, int index)
        {
            //PlayerLog.WriteLog(obj.ObjId, "----------REF_ATTR----------BuffId={0},Index={1}", buff.GetBuffId(), index);
            var tbBuff = buff.mBuff;
            var param = tbBuff.effectparam;
            //这里仅仅设置脏标记,下次获得时将会考虑重新计算生效
            obj.Attr.SetFlag((eAttributeType) param[index, 0]);
        }

        #endregion

        #region     //释放Buff = 4

        public void PRO_ADD_BUFF(Scene scene, ObjCharacter obj, BuffData buff, int index)
        {
//             if (obj is ObjPlayer)
//             {
//                 PlayerLog.WriteLog(obj.ObjId, "----------PRO_ADD_BUFF----------BuffId={0},Index={1}", buff.GetBuffId(), index);
//             }
            var tb_buff = buff.mBuff;
            var param = tb_buff.effectparam;
            var caster = buff.GetCaster();
            //BuffResult buffResult = new BuffResult()
            //{
            //    HeroIndex = hero.ID,
            //    BuffId = buff.DataID,
            //    hitType = HitType.HT_NEWBUFF,
            //    Damage = 0
            //};
            //buff_results.Add(buffResult);
            var tbArea = Table.GetSkillArea(param[index, 1]);
            if (tbArea == null)
            {
                return;
            }

            var targetlist = SkillManager.GetTargetByCondition(caster, obj, (eTargetType) tbArea.TargetType,
                tbArea.TargetParam, (eCampType) tbArea.CampType, -1);

            foreach (var character in targetlist)
            {
                var random = MyRandom.Random(10000);
                if (random <= param[index, 0])
                {
                    character.AddBuff(param[index, 2], buff.m_nLevel, caster, 0, buff.m_HitType);
                }
                else
                {
                    character.AddBuff(param[index, 3], buff.m_nLevel, caster, 0, buff.m_HitType);
                }
            }

            //eTargetType tT = (eTargetType) param[index, 1];
            //switch (tT)
            //{
            //    case eTargetType.Self:
            //        {
            //            int random = MyRandom.Random(10000);
            //            if (random <= param[index, 0])
            //            {
            //                caster.AddBuff(param[index, 2], buff.m_nLevel, caster);
            //            }
            //            else
            //            {
            //                caster.AddBuff(param[index, 3], buff.m_nLevel, caster);
            //            }
            //        }
            //        break;
            //    case eTargetType.Target:
            //        {
            //            int random = MyRandom.Random(10000);
            //            if (random <= param[index, 0])
            //            {
            //                obj.AddBuff(param[index, 2], buff.m_nLevel, caster);
            //            }
            //            else
            //            {
            //                obj.AddBuff(param[index, 3], buff.m_nLevel, caster);
            //            }
            //        }
            //        break;
            //    default:
            //    {

            //    }
            //        break;
            //}
        }

        #endregion

        #region     //释放方治疗万分比修正 = 5

        public void CASTER_REF_HEALTH(ObjCharacter caster, ObjCharacter target, ref int health, int health_type)
        {
            double fBili = 1.0f;
            foreach (var i in caster.BuffList.mData)
            {
                if (i.GetActive() == false)
                {
                    continue;
                }
                var tb_buff = i.mBuff;
                for (var j = 0; j != tb_buff.effectid.Length; ++j)
                {
                    if (tb_buff.effectid[j] == (int) eEffectType.CasterRefHealth)
                    {
                        if (tb_buff.effectparam[j, 0] != -1)
                        {
                            if (!BitFlag.GetLow(tb_buff.effectparam[j, 0], health_type))
                            {
                                continue;
                            }
                        }
                        fBili = fBili*(10000 + tb_buff.effectparam[j, 1]*i.GetLayer())/10000;
                    }
                }
            }
            health = (int) (fBili*health);
        }

        #endregion

        #region     //承受方治疗万分比修正 = 6

        public void BEAR_REF_HEALTH(ObjCharacter caster, ObjCharacter target, ref int health, int health_type)
        {
            double fBili = 1.0f;
            foreach (var i in target.BuffList.mData)
            {
                if (i.GetActive() == false)
                {
                    continue;
                }
                var tb_buff = i.mBuff;
                for (var j = 0; j != tb_buff.effectid.Length; ++j)
                {
                    if (tb_buff.effectid[j] == (int) eEffectType.BearRefHealth)
                    {
                        if (tb_buff.effectparam[j, 0] != -1)
                        {
                            //if (BitFlag.GetAnd(tb_buff.effectparam[j, 0], health_type) == 0)
                            //{
                            //    continue;
                            //}
                            if (!BitFlag.GetLow(tb_buff.effectparam[j, 0], health_type))
                            {
                                continue;
                            }
                        }
                        fBili = fBili*(10000 + tb_buff.effectparam[j, 1]*i.GetLayer())/10000;
                    }
                }
            }
            health = (int) (fBili*health);
        }

        #endregion

        #region     //释放方伤害万分比修正 = 7

        public void CASTER_REF_DAMAGE(ObjCharacter caster, ObjCharacter target, ref int damage, int damage_type)
        {
            double fBili = 1.0f;
            foreach (var i in caster.BuffList.mData)
            {
                if (i.GetActive() == false)
                {
                    continue;
                }
                var tb_buff = i.mBuff;
                for (var j = 0; j != tb_buff.effectid.Length; ++j)
                {
                    if (tb_buff.effectid[j] == (int) eEffectType.CasterRefDamage)
                    {
                        if (tb_buff.effectparam[j, 0] != -1)
                        {
                            //if (BitFlag.GetAnd(tb_buff.effectparam[j, 0], damage_type)==0)
                            //{
                            //    continue;
                            //}
                            if (!BitFlag.GetLow(tb_buff.effectparam[j, 0], damage_type))
                            {
                                continue;
                            }
                        }
                        fBili = fBili*(10000 + tb_buff.effectparam[j, 1]*i.GetLayer())/10000;
                    }
                }
            }
            damage = (int) (fBili*damage);
        }

        #endregion

        #region     //承受方伤害万分比修正 = 8

        public void BEAR_REF_DAMAGE(ObjCharacter caster, ObjCharacter target, ref int damage, int damage_type)
        {
            double fBili = 1.0f;
            foreach (var i in target.BuffList.mData)
            {
                if (i.GetActive() == false)
                {
                    continue;
                }
                var tb_buff = i.mBuff;
                for (var j = 0; j != tb_buff.effectid.Length; ++j)
                {
                    if (tb_buff.effectid[j] == (int) eEffectType.BearRefDamage)
                    {
                        if (tb_buff.effectparam[j, 0] != -1)
                        {
                            //if (BitFlag.GetAnd(tb_buff.effectparam[j, 0], damage_type) == 0)
                            //{
                            //    continue;
                            //}
                            if (!BitFlag.GetLow(tb_buff.effectparam[j, 0], damage_type))
                            {
                                continue;
                            }
                        }
                        fBili = fBili*(10000 + tb_buff.effectparam[j, 1]*i.GetLayer())/10000;
                    }
                }
            }
            damage = (int) (fBili*damage);
        }

        #endregion

        #region     //特殊状态 = 9

        public void SPEIALSTATE(eEffectEventType eventType, Scene scene, ObjCharacter caster, BuffData buff, int index)
        {
            if (eventType == eEffectEventType.GetBuff)
            {
                var tbBuff = buff.mBuff;
                var param = tbBuff.effectparam;
                var nSpecialState = param[index, 0]; //特定状态
                if (BitFlag.GetLow(nSpecialState, 0)) //被控制了
                {
                    //PlayerLog.WriteLog(buff.GetBear().ObjId, "SPEIALSTATE GetBuff {0}", buff.m_nBuffId);
					buff.GetBear().BuffList.SetSpecialStateNoMove(caster);
                }
            }
            else if (eventType == eEffectEventType.MissBuff)
            {
                var tbBuff = buff.mBuff;
                var param = tbBuff.effectparam;
                var nSpecialState = param[index, 0]; //特定状态
                if (BitFlag.GetLow(nSpecialState, 0)) //被控制了
                {
                    //PlayerLog.WriteLog(buff.GetBear().ObjId, "SPEIALSTATE MissBuff {0}", buff.m_nBuffId);
                    buff.GetBear().BuffList.SetSpecialStateFlag();
                }
            }
        }

        #endregion

        #region     //驱散BUFF = 11

        public void DispelBuff(eEffectEventType eventType, Scene scene, ObjCharacter caster, BuffData buff, int index)
        {
            var tbBuff = buff.mBuff;
            var param = tbBuff.effectparam;
            var nSpecialState = param[index, 0]; //Buff类型复选参数
            var self = buff.GetBear();
            var buffs = self.BuffList.mData;
            foreach (var data in buffs)
            {
                if (!data.GetActive())
                {
                    continue;
                }
                if (BitFlag.GetLow(nSpecialState, data.mBuff.Type))
                {
                    MissBuff.DoEffect(scene, self, data);
                    self.DeleteBuff(data, eCleanBuffType.Clear);
                }
            }
        }

        #endregion

        #region     // 吸收伤害 = 12

        public int AbsorbInjury(ObjCharacter target, int damageType, ref int damage)
        {
            var allAbsorbValue = 0;
            foreach (var buff in target.BuffList.mData)
            {
                var tbBuff = buff.mBuff;
                if (buff.GetActive() == false || tbBuff == null)
                    continue;

                var needDeleteBuff = 0;
                for (var j = 0; j != tbBuff.effectid.Length; ++j)
                {
                    var effectId = tbBuff.effectid[j];
                    if (effectId == -1)
                        continue;

                    if (effectId == (int)eEffectType.AbsorbInjury)
                    {
                        if (damage <= 0)
                        { // 不需要吸收了,看看是否还能吸收，能吸收的话，就不删除了
                            var value = 0;
                            if (buff.RemainAbsorbDict.TryGetValue((byte) j, out value))
                            {
                                if (value > 0)
                                    needDeleteBuff = -1;
                            }
                            continue;
                        }

                        var absorbType = tbBuff.effectparam[j, 0];
                        if (absorbType == -1)
                            continue;
                        
                        if (!BitFlag.GetLow(absorbType, damageType))
                            continue;

                        var remainValue = 0;
                        var absorbValue = buff.AbsorbDamage(damage, (byte)j, ref remainValue);
                        if (absorbValue <= 0) // 不能吸收伤害
                            continue;

                        if (remainValue <= 0)
                        {
                            if (needDeleteBuff == 0)
                            { // 需要删除buff
                                needDeleteBuff = 1;
                            }
                        }
                        else
                        { // 还能继续吸收，不能删除
                            needDeleteBuff = -1;
                        }

                        allAbsorbValue += absorbValue;
                        damage -= absorbValue;
                    }
                    else
                    { // 有其它效果，一定不要删除
                        needDeleteBuff = -1;
                    }
                }

                if (needDeleteBuff > 0)
                {
                    target.DeleteBuff(buff, eCleanBuffType.AbsorbOver);
                }

                if (damage <= 0)
                    break;
            }

            return allAbsorbValue;
        }

        #endregion

        #region     //修改技能的效果 = 14

        public void NO_MODIFYSKILL(eEffectEventType eventType,
                                   Scene scene,
                                   ObjCharacter caster,
                                   BuffData buff,
                                   int index)
        {
            if (eventType == eEffectEventType.GetBuff)
            {
                var tbBuff = buff.mBuff;
                var param = tbBuff.effectparam;
                var skillId = param[index, 0];
                var nFieldType = param[index, 1];
                var nModifyType = param[index, 2];
                var nModifyValue = param[index, 3];
                buff.GetBear().Skill.PushSkillModify(skillId, nFieldType, nModifyType, nModifyValue);
            }
            else if (eventType == eEffectEventType.MissBuff)
            {
                var tbBuff = buff.mBuff;
                var param = tbBuff.effectparam;
                var skillId = param[index, 0];
                var nFieldType = param[index, 1];
                var nModifyType = param[index, 2];
                var nModifyValue = param[index, 3];
                buff.GetBear().Skill.CleanSkillModify(skillId, nFieldType, nModifyType, nModifyValue);
            }
        }

        #endregion

        #region     //修改Buff的效果 = 15

        public void NO_MODIFYBUFF(eEffectEventType eventType, Scene scene, ObjCharacter caster, BuffData buff, int index)
        {
            if (eventType == eEffectEventType.GetBuff)
            {
                var tbBuff = buff.mBuff;
                var param = tbBuff.effectparam;
                var buffId = param[index, 0];
                var nFieldType = param[index, 1];
                var nModifyType = param[index, 2];
                var nModifyValue = param[index, 3];
                buff.GetBear().Skill.PushBuffModify(buffId, nFieldType, nModifyType, nModifyValue);
            }
            else if (eventType == eEffectEventType.MissBuff)
            {
                var tbBuff = buff.mBuff;
                var param = tbBuff.effectparam;
                var buffId = param[index, 0];
                var nFieldType = param[index, 1];
                var nModifyType = param[index, 2];
                var nModifyValue = param[index, 3];
                buff.GetBear().Skill.CleanBuffModify(buffId, nFieldType, nModifyType, nModifyValue);
            }
        }

        #endregion

        #region     //修改模型 = 16

        public void NO_MODIFYMODEL(eEffectEventType eventType, ObjCharacter caster, BuffData buff)
        {
            if (!(caster is ObjPlayer))
            {
                return;
            }
            var player = caster as ObjPlayer;
            if (player.Proxy == null)
            {
                return;
            }
            if (eventType == eEffectEventType.GetBuff)
            {
                var tbBuff = buff.mBuff;
                var param = tbBuff.effectparam;
                player.ModelId = param[0, 0];
                player.Proxy.SyncModelId(player.ModelId);
            }
            else if (eventType == eEffectEventType.MissBuff)
            {
                player.ModelId = -1;
                player.Proxy.SyncModelId(player.ModelId);
            }
        }

        #endregion

        #region     //召唤随从 = 17

        public void CREATER_MONSTER(eEffectEventType eventType,
                                    Scene scene,
                                    ObjCharacter caster,
                                    BuffData buff,
                                    int index)
        {
            if (eventType == eEffectEventType.GetBuff)
            {
                var tbBuff = buff.mBuff;
                var param = tbBuff.effectparam;
                var obj = buff.GetBear();
                if (obj.IsHaveRetinue(buff.Retinue))
                {
                    obj.RemoveRetinue(buff.Retinue);
                }
                var temp = obj.CreateRetinue(param[index, 0], buff.m_nLevel, obj.GetPosition(), obj.GetDirection(),
                    obj.GetCamp());
                if (temp == null)
                {
                    return;
                }
                buff.Retinue = temp;
                temp.Buff = buff;
                buff.SetCaster(temp);
            }
            else if (eventType == eEffectEventType.MissBuff)
            {
                if (buff.GetCaster() is ObjNPC)
                {
                    var obj = buff.GetBear();
                    if (buff.Retinue != null)
                    {
                        obj.RemoveRetinue(buff.Retinue);
                    }
                    //if (obj.IsHaveRetinue(buff.GetCaster()))
                    //{
                    //    scene.LeaveScene(buff.GetCaster());
                    //}
                }
            }
        }

        #endregion

        #region     //吸血 = 18

        public void DAMAGE_HEALTH(Scene scene,
                                  ObjCharacter caster,
                                  int damage,
                                  BuffData buff,
                                  int index,
                                  Dictionary<int, int> totlehealth = null)
        {
            //PlayerLog.WriteLog(caster.ObjId, "----------DAMAGE_HEALTH----------BuffId={0},Index={1}", buff.GetBuffId(), index);
            var tb_buff = buff.mBuff;
            var param = tb_buff.effectparam;
            if (param[index, 2] <= 0 || damage <= 0)
            {
                return;
            }
            var health = (int) (damage/10000.0f*param[index, 2]);
            if (health <= 0)
            {
                health = 1;
            }
            var targetType = (eTargetType) param[index, 1];
            switch (targetType)
            {
                case eTargetType.Self: //释放者
                {
                    if (totlehealth == null)
                    {
                        caster.DoHealth(eHitType.Hit, buff, caster, ref health, param[index, 0], true);
                    }
                    else
                    {
                        var healthType = tb_buff.effectparam[index, 0];
                        if (totlehealth.ContainsKey(healthType))
                        {
                            totlehealth[healthType] += health;
                        }
                        else
                        {
                            totlehealth[healthType] = health;
                        }
                    }
                }
                    break;
                case eTargetType.Target: //承受者
                {
                    buff.GetBear().DoHealth(eHitType.Hit, buff, buff.GetBear(), ref health, param[index, 0], true);
                }
                    break;
                case eTargetType.Around:
                    break;
                case eTargetType.Fan:
                    break;
                case eTargetType.Rect:
                    break;
                case eTargetType.TargetAround:
                    break;
                case eTargetType.TargetRect:
                    break;
                case eTargetType.TargetFan:
                    break;
                case eTargetType.Ejection:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region     //经验修正 = 23

        public void EXP_MODIFY(eEffectEventType eventType, Scene scene, ObjCharacter obj, BuffData buff, int index)
        {
            var tbBuff = buff.mBuff;
            var param = tbBuff.effectparam;
            var biliModify = param[index, 0];
            if (biliModify == 0)
            {
                return;
            }
            if (eventType == eEffectEventType.GetBuff)
            {
                foreach (var data in obj.BuffList.mData)
                {
                    if (!data.GetActive())
                    {
                        continue;
                    }
                    var tb_buff = data.mBuff;
                    for (var j = 0; j != tb_buff.effectid.Length; ++j)
                    {
                        if (tb_buff.effectid[j] == (int) eEffectType.ExpModify)
                        {
                            obj.Attr.ExpRef = tb_buff.effectparam[j, 0];
                        }
                    }
                }
            }
            else if (eventType == eEffectEventType.MissBuff)
            {
                obj.Attr.ExpRef = obj.Attr.ExpRef*10000/biliModify;
                if (obj.Attr.ExpRef > 9995 && obj.Attr.ExpRef < 10005)
                {
                    obj.Attr.ExpRef = 10000;
                }
            }
            //int FixModify = param[index, 1];
        }

        #endregion

        #region     //回蓝 = 24

        public void DO_MANA(Dictionary<int, int> totlehealth,
                            Scene scene,
                            ObjCharacter obj,
                            BuffData buff,
                            int index,
                            bool IsRestHitType)
        {
            //PlayerLog.WriteLog(obj.ObjId, "----------DO_HEALTH----------BuffId={0},Index={1}", buff.GetBuffId(), index);
            var tbBuff = buff.mBuff;
            var param = tbBuff.effectparam;
            var health = MyRandom.Random(param[index, 4], param[index, 5]);
            var healthType = param[index, 0];
            var bilizhi = param[index, 3];
            var tempObj = buff.GetCaster(); //释放者
            if (bilizhi > 0)
            {
                if (param[index, 1] == 1)
                {
//承受者
                    tempObj = obj;
                }
                if (tempObj != null)
                {
                    health += tempObj.Attr.GetDataValue((eAttributeType) param[index, 2])*bilizhi/10000;
                }
            }


            var hitType = buff.m_HitType;
            if (IsRestHitType)
            {
                if (tempObj != null)
                {
                    hitType = tempObj.Attr.GetHitResult(obj, eSkillHitType.Normal);
                }
                else
                {
                    hitType = eHitType.Hit;
                }
            }
            if (tempObj != null)
            {
                if (hitType == eHitType.Excellent && null != buff.GetCaster())
                {
                    health = (int) (health * buff.m_fModify * buff.GetCaster().Attr.GetDataValue(eAttributeType.ExcellentDamage) / 10000);
                }
                else if (hitType == eHitType.Lucky && null != buff.GetCaster())
                {
                    health = (int) (health * buff.m_fModify * buff.GetCaster().Attr.GetDataValue(eAttributeType.LuckyDamage) / 10000);
                }
                else
                {
                    health = (int) (health * buff.m_fModify);
                }
            }
            else
            {
                health = (int) (health*buff.m_fModify);
            }

            if (totlehealth.ContainsKey(healthType))
            {
                totlehealth[healthType] += health*buff.GetLayer();
            }
            else
            {
                totlehealth[healthType] = health*buff.GetLayer();
            }
        }

        #endregion

        #region     //自杀= 25

        public void Kill_SELF(Scene scene, ObjCharacter caster, BuffData buff, int index)
        {
            var obj = buff.GetBear();
            if (obj == null)
            {
                return;
            }
            if (obj is ObjPlayer)
            {
                obj.Die(caster.ObjId, 0, 0);
            }
            else
            {
                obj.Die(caster.ObjId, 0, 0);
                //scene.LeaveScene(obj);
            }
        }

        #endregion

        #region     //经验修正 = 26 

        public void EXP_MODIFY2(eEffectEventType eventType, Scene scene, ObjCharacter obj, BuffData buff, int index)
        {
            var tbBuff = buff.mBuff;
            var param = tbBuff.effectparam;
            var biliModify = param[index, 0];
            if (biliModify == 0)
            {
                return;
            }
            if (eventType == eEffectEventType.GetBuff)
            {
                if (biliModify > 100000)
                {
                    biliModify = 100000;
                }
                obj.Attr.ExpRef2 = biliModify;
            }
            else if (eventType == eEffectEventType.MissBuff)
            {
                if (biliModify > 100000)
                {
                    biliModify = 100000;
                }
                obj.Attr.ExpRef2 = obj.Attr.ExpRef2*10000/biliModify;
                if (obj.Attr.ExpRef2 > 9995 && obj.Attr.ExpRef2 < 10005)
                {
                    obj.Attr.ExpRef2 = 10000;
                }
            }
        }

        #endregion
        #region         //外围经验加成
        public void EXP_ADD(eEffectEventType eventType, Scene scene, ObjCharacter obj, BuffData buff, int index)
        {
            var tbBuff = buff.mBuff;
            var param = tbBuff.effectparam;
            var biliModify = param[index, 0];
            if (biliModify == 0)
            {
                return;
            }
            var lv = buff.m_nLevel;
            var skillup = Table.GetSkillUpgrading(biliModify);
            if (skillup == null)
                return;
            var v = skillup.GetSkillUpgradingValue(lv-1);
            if (eventType == eEffectEventType.GetBuff)
            {
                obj.Attr.ExpAdd += v;
            }
            else if (eventType == eEffectEventType.MissBuff)
            {
                obj.Attr.ExpAdd -= v;
            }
        }
        #endregion
        #region     //位置修正 = 3

        public void POSITION_CHANGE(Scene scene, ObjCharacter obj, BuffData buff, int index)
        {
            //PlayerLog.WriteLog(obj.ObjId, "----------POSITION_CHANGE----------BuffId={0},Index={1}", buff.GetBuffId(),index);
            var tbBuff = buff.mBuff;
            var param = tbBuff.effectparam;

            if (param[index, 5] == 1)
            {
                if (obj.BuffList.GetEffectParam_Bin(eEffectType.NoBuffType, 1, 3)) //&& buff.GetCaster() != obj)
                {
                    return;
                }
            }
            var dt = (eDirectionType) param[index, 0];
            var distType = param[index, 1];
            if (distType == 1)
            {
                if (obj != null && obj.Skill != null && obj.Skill.CurrentSkill != null &&
                    obj.Skill.CurrentSkill.mTarget != null)
                {
                    var p = obj.Skill.CurrentSkill.mTarget.GetPosition();
                    SendPackage(obj, buff, index, p, param);
                    return;
                }
            }

            switch (dt)
            {
                case eDirectionType.Caster: //施法者朝向(闪现)
                {
                    var tempObj = buff.GetCaster(); //释放者
                    if (tempObj != null)
                    {
                        var p = obj.GetPosition() + tempObj.GetDirection()*param[index, 2];
                        SendPackage(obj, buff, index, p, param);
                    }
                }
                    break;
                case eDirectionType.ToCaster: //朝向施法者(屠夫钩)
                {
                    var tempObj = buff.GetCaster(); //释放者
                    if (tempObj != null)
                    {
                        var p = tempObj.GetPosition() +
                                Vector2.Normalize(obj.GetPosition() - tempObj.GetPosition())*param[index, 2];
                        SendPackage(obj, buff, index, p, param);
                    }
                }
                    break;
                case eDirectionType.Bear: //承受者朝向(推推棒)
                {
                    var p = obj.GetPosition() + obj.GetDirection()*param[index, 2];
                    SendPackage(obj, buff, index, p, param);
                }
                    break;
                case eDirectionType.ToBear: //朝向承受者(地精钩)
                {
                    var tempObj = buff.GetCaster(); //释放者
                    if (tempObj != null)
                    {
                        var p = obj.GetPosition() +
                                Vector2.Normalize(obj.GetPosition() - tempObj.GetPosition())*param[index, 2];
                        SendPackage(obj, buff, index, p, param);
                    }
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SendPackage(ObjCharacter obj, BuffData buff, int index, Vector2 p, int[,] param)
        {
            var replyMsg = new BuffResultMsg();
            replyMsg.buff.Add(new BuffResult
            {
                SkillObjId = buff.mCasterId,
                TargetObjId = obj.ObjId,
                BuffTypeId = buff.GetBuffId(),
                Type = BuffType.HT_MOVE,
                Param = {(int) (p.X*1000), (int) (p.Y*1000), param[index, 3], param[index, 4]},
                ViewTime = Extension.AddTimeDiffToNet(0)
            });
            obj.BroadcastBuffList(replyMsg);

            if (param[index, 4] > 0)
            {
                SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddMilliseconds(param[index, 4]),
                    () => obj.ForceMoveTo(p, param[index, 3]));
            }
            else
            {
                obj.ForceMoveTo(p, param[index, 3]);
            }
        }

        #endregion

        #region     //BOSS召唤怪物 = 13

        public void CREATER_MONSTER1TYPE(Scene scene, ObjCharacter caster, BuffData buff, int index)
        {
            var tbBuff = buff.mBuff;
            var param = tbBuff.effectparam;
            var obj = buff.GetBear();
            if (obj.IsHaveRetinue(buff.Retinue))
            {
                obj.RemoveRetinue(buff.Retinue);
            }
            var monsterId = param[index, 0];
            var count = param[index, 1];
            var camp = buff.GetCaster().GetCamp();
            if (param[index, 2] == 0)
            {
                float minDis = param[index, 3];
                float maxDis = param[index, 4];
                var dis = MyRandom.Random(minDis, maxDis);
                var selfpos = obj.GetPosition();
                var jiaodu = MyRandom.Random(360);
                var jiaoduDiff = param[index, 5];
                for (var i = 0; i < count; i++)
                {
                    var newpos = GetPosDiffPos(scene, selfpos, jiaodu, dis);
                    obj.CreateRetinue(monsterId, buff.m_nLevel, newpos, obj.GetDirection(), camp, false);
                    //objretinue.Buff = buff;
                    jiaodu += jiaoduDiff;
                }
            }
        }

        public Vector2 GetPosDiffPos(Scene scene, Vector2 oldpos, int jiaodu, float dis)
        {
            var cosX = (float) Math.Cos(jiaodu);
            var sinX = (float) Math.Sin(jiaodu);
            while (true)
            {
                var dir = new Vector2(cosX, sinX);
                var newpos = oldpos + dir*dis;
                if (scene.GetObstacleValue(newpos.X, newpos.Y) != SceneObstacle.ObstacleValue.Obstacle)
                {
                    return newpos;
                }
                dis = dis - 0.5f;
                if (dis < 0)
                {
                    break;
                }
            }
            return oldpos;
        }

        #endregion

        #region     //施放技能 = 27

        public void DoSkill(Scene scene, ObjCharacter obj, ObjCharacter target, BuffData buff, int index)
        {
            if (obj == null || obj.NormalAttr.AreaState == eAreaState.City)
            { // 安全区
                return;
            }

            var tbBuff = buff.mBuff;
            var param = tbBuff.effectparam;  
            var skillId = param[index, 0];

            var skill = obj.Skill.GetSkill(skillId);
            if (skill == null)
            {
                skill = obj.Skill.AddSkill(skillId, buff.m_nLevel, eAddskillType.EquipAddSkill);
            }
            if (skill.mLevel != buff.m_nLevel)//bug 4720 【bug】9级心眼和1级心眼实际获得的加成属性一样多
            {
                obj.Skill.ResetSkill(skillId, buff.m_nLevel);
            }
            ObjCharacter skillTarget = null;
            if (skill != null && skill.mTable != null)
            {
                if (obj.IsRiding() && skill.IsPassiveSkill())
                { // 不在战斗中（骑马中），不放强制技能
                    return;
                }

                switch ((eTargetType)skill.mTable.TargetType)
                {
                    case eTargetType.Self:
                        skillTarget = obj;
                        break;
                    case eTargetType.Target:
                    case eTargetType.TargetAround:
                    case eTargetType.TargetRect:
                    case eTargetType.TargetFan:
                        { // 需要目标
                            var camp = (eCampType)skill.mTable.CampType;
                            if (camp == eCampType.Enemy)
                            { // 敌人
                                skillTarget = target;
                                if (target == null || target.GetCamp() == obj.GetCamp())
                                { // 默认选中目标当前敌人
                                    var currentSkill = obj.Skill.CurrentSkill;
                                    if (currentSkill != null && currentSkill.mTable != null)
                                    {
                                        skillTarget = obj.Skill.EnemyTarget;
                                    }
                                }
                            }
                            else
                            { // 其它阵营暂未处理
                                skillTarget = target;
                            }
                        }
                        break;
                }
            }

            obj.UseSkill(ref skillId, skillTarget);
        }
        #endregion

		#region     //致死一击 = 28

		public void HP_TRIGGER_ADDBUFF(Scene scene, ObjCharacter obj, BuffData buff, int index)
		{
			var hp = obj.GetAttribute(eAttributeType.HpNow);
			var hpMax = obj.GetAttribute(eAttributeType.HpMax);

			var tbBuff = buff.mBuff;
            var param = tbBuff.effectparam;
			var hpPercent = hp*1.0f/hpMax;
            var min = param[index, 0]*0.0001f;
			var max = param[index, 1] * 0.0001f;
			var targetType = param[index, 2];
			var buffidAdd = param[index, 3];

			if (hpPercent>=min && hpPercent<=max)
			{
				
				var tbArea = Table.GetSkillArea(targetType);
				if (tbArea == null)
				{
					return;
				}
				var caster = buff.GetCaster();
				var targetlist = SkillManager.GetTargetByCondition(caster, obj, (eTargetType)tbArea.TargetType,
					tbArea.TargetParam, (eCampType)tbArea.CampType, -1);

				foreach (var character in targetlist)
				{
					character.AddBuff(buffidAdd, buff.m_nLevel, caster, 0, buff.m_HitType);
				}

			}

		}
		#endregion
		
		#region     //未来效果

		#endregion
	}

    public static class BuffEffect
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static IBuffEffect mImpl;

        static BuffEffect()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (BuffEffect), typeof (BuffEffectDefaultImpl),
                o => { mImpl = (IBuffEffect) o; });
        }

        #region     //承受方伤害万分比修正 = 8

        public static void BEAR_REF_DAMAGE(ObjCharacter caster, ObjCharacter target, ref int damage, int damage_type)
        {
            mImpl.BEAR_REF_DAMAGE(caster, target, ref damage, damage_type);
        }

        #endregion

        #region     //承受方治疗万分比修正 = 6

        public static void BEAR_REF_HEALTH(ObjCharacter caster, ObjCharacter target, ref int health, int health_type)
        {
            mImpl.BEAR_REF_HEALTH(caster, target, ref health, health_type);
        }

        #endregion

        #region     //释放方伤害万分比修正 = 7

        public static void CASTER_REF_DAMAGE(ObjCharacter caster, ObjCharacter target, ref int damage, int damage_type)
        {
            mImpl.CASTER_REF_DAMAGE(caster, target, ref damage, damage_type);
        }

        #endregion

        #region     //释放方治疗万分比修正 = 5

        public static void CASTER_REF_HEALTH(ObjCharacter caster, ObjCharacter target, ref int health, int health_type)
        {
            mImpl.CASTER_REF_HEALTH(caster, target, ref health, health_type);
        }

        #endregion

        #region     //召唤随从 = 17

        public static void CREATER_MONSTER(eEffectEventType eventType,
                                           Scene scene,
                                           ObjCharacter caster,
                                           BuffData buff,
                                           int index)
        {
            mImpl.CREATER_MONSTER(eventType, scene, caster, buff, index);
        }

        #endregion

        #region     //吸血 = 18

        public static void DAMAGE_HEALTH(Scene scene,
                                         ObjCharacter caster,
                                         int damage,
                                         BuffData buff,
                                         int index,
                                         Dictionary<int, int> totlehealth = null)
        {
            mImpl.DAMAGE_HEALTH(scene, caster, damage, buff, index, totlehealth);
        }

        #endregion

        #region     //驱散BUFF = 11

        public static void DispelBuff(eEffectEventType eventType,
                                      Scene scene,
                                      ObjCharacter caster,
                                      BuffData buff,
                                      int index)
        {
            mImpl.DispelBuff(eventType, scene, caster, buff, index);
        }

        #endregion

        #region // 吸收伤害BUFF = 12

        public static int AbsorbInjury(ObjCharacter target, int damageType, ref int damage)
        {
            return mImpl.AbsorbInjury(target, damageType, ref damage);
        }
        
        #endregion

        #region     //伤害效果 = 0

        public static void DO_DAMAGE(Dictionary<int, int> totledamage,
                                     Scene scene,
                                     ObjCharacter obj,
                                     BuffData buff,
                                     int index,
                                     bool IsRestHitType) //场景，承受者，buff，index
        {
            mImpl.DO_DAMAGE(totledamage, scene, obj, buff, index, IsRestHitType);
        }

        #endregion

        #region     //治疗效果 = 1

        public static void DO_HEALTH(Dictionary<int, int> totlehealth,
                                     Scene scene,
                                     ObjCharacter obj,
                                     BuffData buff,
                                     int index,
                                     bool IsRestHitType)
        {
            mImpl.DO_HEALTH(totlehealth, scene, obj, buff, index, IsRestHitType);
        }

        #endregion

        #region     //回蓝 = 24

        public static void DO_MANA(Dictionary<int, int> totlehealth,
                                   Scene scene,
                                   ObjCharacter obj,
                                   BuffData buff,
                                   int index,
                                   bool IsRestHitType)
        {
            mImpl.DO_MANA(totlehealth, scene, obj, buff, index, IsRestHitType);
        }

        #endregion

        #region     //经验修正 = 23

        public static void EXP_MODIFY(eEffectEventType eventType,
                                      Scene scene,
                                      ObjCharacter obj,
                                      BuffData buff,
                                      int index)
        {
            mImpl.EXP_MODIFY(eventType, scene, obj, buff, index);
        }

        #endregion
        #region //外围经验修正 29

        public static void EXP_ADD(eEffectEventType eventType,
            Scene scene,
            ObjCharacter obj,
            BuffData buff,
            int index)
        {
            mImpl.EXP_ADD(eventType, scene, obj, buff, index);
        }
        #endregion
        #region     //经验修正 = 26

        public static void EXP_MODIFY2(eEffectEventType eventType,
                                       Scene scene,
                                       ObjCharacter obj,
                                       BuffData buff,
                                       int index)
        {
            mImpl.EXP_MODIFY2(eventType, scene, obj, buff, index);
        }

        #endregion

        #region     //自杀= 25

        public static void Kill_SELF(Scene scene, ObjCharacter caster, BuffData buff, int index)
        {
            mImpl.Kill_SELF(scene, caster, buff, index);
        }

        #endregion

        #region     //修改Buff的效果 = 15

        public static void NO_MODIFYBUFF(eEffectEventType eventType,
                                         Scene scene,
                                         ObjCharacter caster,
                                         BuffData buff,
                                         int index)
        {
            mImpl.NO_MODIFYBUFF(eventType, scene, caster, buff, index);
        }

        #endregion

        #region     //修改模型 = 16

        public static void NO_MODIFYMODEL(eEffectEventType eventType, ObjCharacter caster, BuffData buff)
        {
            mImpl.NO_MODIFYMODEL(eventType, caster, buff);
        }

        #endregion

        #region     //修改技能的效果 = 14

        public static void NO_MODIFYSKILL(eEffectEventType eventType,
                                          Scene scene,
                                          ObjCharacter caster,
                                          BuffData buff,
                                          int index)
        {
            mImpl.NO_MODIFYSKILL(eventType, scene, caster, buff, index);
        }

        #endregion

        #region     //位置修正 = 3

        public static void POSITION_CHANGE(Scene scene, ObjCharacter obj, BuffData buff, int index)
        {
            mImpl.POSITION_CHANGE(scene, obj, buff, index);
        }

        #endregion

        #region     //释放Buff = 4

        public static void PRO_ADD_BUFF(Scene scene, ObjCharacter obj, BuffData buff, int index)
        {
            mImpl.PRO_ADD_BUFF(scene, obj, buff, index);
        }

        #endregion

        #region     //修改属性 = 2

        public static void REF_ATTR(Scene scene, ObjCharacter obj, BuffData buff, int index)
        {
            mImpl.REF_ATTR(scene, obj, buff, index);
        }

        #endregion

        #region     //特殊状态 = 9

        public static void SPEIALSTATE(eEffectEventType eventType,
                                       Scene scene,
                                       ObjCharacter caster,
                                       BuffData buff,
                                       int index)
        {
            mImpl.SPEIALSTATE(eventType, scene, caster, buff, index);
        }

        #endregion

        #region     //BOSS召唤怪物 = 13

        public static void CREATER_MONSTER1TYPE(Scene scene, ObjCharacter caster, BuffData buff, int index)
        {
            mImpl.CREATER_MONSTER1TYPE(scene, caster, buff, index);
        }

        public static Vector2 GetPosDiffPos(Scene scene, Vector2 oldpos, int jiaodu, float dis)
        {
            return mImpl.GetPosDiffPos(scene, oldpos, jiaodu, dis);
        }

        #endregion

        #region // 施放技能 = 27

        public static void DoSkill(Scene scene, ObjCharacter caster, ObjCharacter target, BuffData buff, int index)
        {
            mImpl.DoSkill(scene, caster, target, buff, index);
        }
        
        #endregion

		#region // 血量在一个范围内获得一个效果 = 28

		public static void HP_TRIGGER_ADDBUFF(Scene scene, ObjCharacter obj, BuffData buff, int index)
		{
			mImpl.HP_TRIGGER_ADDBUFF(scene,
                        obj,
                        buff,
                        index);
		}

		#endregion

        #region     //未来效果

        #endregion
    }
}