#region using

using System;
using System.Collections.Generic;
using DataTable;
using Mono.GameMath;
using NLog;
using Shared;

#endregion

namespace Scene
{
    public interface IBullet
    {
        DirectionBullet CreateBullet(ObjCharacter caster,
                                     int BulletId,
                                     int nLevel,
                                     eSkillHitType hitType,
                                     Vector2 direction);

        TargetBullet CreateBullet(ObjCharacter caster,
                                  int BulletId,
                                  int nLevel,
                                  eSkillHitType hitType,
                                  ObjCharacter target);

        void DelayArrive(Bullet _this);
        void OnDestroy(Bullet _this);
    }

    public class BulletDefaultImpl : IBullet
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region  生效

        //延迟到击中时
        public virtual void DelayArrive(Bullet _this)
        {
            _this.Target.RemoveBullet(_this);
            var hitResult = _this.Caster.Attr.GetHitResult(_this.Target, _this.HitType);
            foreach (var i in _this.Buff)
            {
                _this.Target.AddBuff(i.Key, i.Value, _this.Caster, 0, hitResult, _this.Modify);
            }
        }

        #endregion

        #region  删除

        public void OnDestroy(Bullet _this)
        {
            if (_this.ArriveTrigger != null)
            {
                SceneServerControl.Timer.DeleteTrigger(_this.ArriveTrigger);
                _this.ArriveTrigger = null;
            }
        }

        #endregion

        #region  创建

        //创建子弹(靠方向)
        public DirectionBullet CreateBullet(ObjCharacter caster,
                                            int BulletId,
                                            int nLevel,
                                            eSkillHitType hitType,
                                            Vector2 direction)
        {
            var tbbullet = Table.GetBullet(BulletId);
            if (tbbullet == null)
            {
                return null;
            }
            return new DirectionBullet(caster, tbbullet, nLevel, hitType, direction);
        }

        //创建子弹(靠目标)
        public TargetBullet CreateBullet(ObjCharacter caster,
                                         int BulletId,
                                         int nLevel,
                                         eSkillHitType hitType,
                                         ObjCharacter target)
        {
            var tbbullet = Table.GetBullet(BulletId);
            if (tbbullet == null)
            {
                return null;
            }
            if (target == null)
            {
                return null;
            }
            if (tbbullet.AttackMonsterID[0] != -1)
            {
                foreach (var i in tbbullet.AttackMonsterID)
                {
                    if (target.TypeId == i)
                    {
                        return new TargetBullet(caster, tbbullet, nLevel, hitType, target);
                    }
                }
                return null;
            }
            return new TargetBullet(caster, tbbullet, nLevel, hitType, target);
        }

        #endregion
    }

    public class Bullet //:ObjBase
    {
        private static IBullet mImpl;

        static Bullet()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (Bullet), typeof (BulletDefaultImpl),
                o => { mImpl = (IBullet) o; });
        }

        #region  生效

        //延迟到击中时
        public virtual void DelayArrive()
        {
            mImpl.DelayArrive(this);
        }

        #endregion

        #region  删除

        public void OnDestroy()
        {
            mImpl.OnDestroy(this);
        }

        #endregion

        #region  数据结构

        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public int Type { get; set; } //0是有目标的追踪   1是没目标的碰撞
        public ObjCharacter Caster { get; set; }
        public ObjCharacter Target { get; set; }
        public Vector2 Direction { get; set; }
        public Dictionary<int, int> Buff = new Dictionary<int, int>();
        public BulletRecord TableBullet { get; set; }
        public eSkillHitType HitType { get; set; }
        public float Modify = 1.0f;
        public Trigger ArriveTrigger;

        #endregion

        #region  创建

        //创建子弹(靠方向)
        public static DirectionBullet CreateBullet(ObjCharacter caster,
                                                   int BulletId,
                                                   int nLevel,
                                                   eSkillHitType hitType,
                                                   Vector2 direction)
        {
            return mImpl.CreateBullet(caster, BulletId, nLevel, hitType, direction);
        }

        //创建子弹(靠目标)
        public static TargetBullet CreateBullet(ObjCharacter caster,
                                                int BulletId,
                                                int nLevel,
                                                eSkillHitType hitType,
                                                ObjCharacter target)
        {
            return mImpl.CreateBullet(caster, BulletId, nLevel, hitType, target);
        }

        #endregion
    }


    public interface ITargetBullet
    {
        void InitTargetBullet(TargetBullet _this,
                              ObjCharacter caster,
                              BulletRecord tbbullet,
                              int nLevel,
                              eSkillHitType hitType,
                              ObjCharacter target);
    }

    public class TargetBulletDefaultImpl : ITargetBullet
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void InitTargetBullet(TargetBullet _this,
                                     ObjCharacter caster,
                                     BulletRecord tbbullet,
                                     int nLevel,
                                     eSkillHitType hitType,
                                     ObjCharacter target)
        {
            _this.Type = 0;
            _this.Caster = caster;
            _this.Target = target;
            _this.TableBullet = tbbullet;
            _this.HitType = hitType;
            foreach (var i in tbbullet.Buff)
            {
                _this.Buff[i] = nLevel;
            }
            double delaytime = (_this.Caster.GetPosition() - _this.Target.GetPosition()).Length()/
                               _this.TableBullet.Speed;
            var DelaySecond = (int) (delaytime*1000);
            DelaySecond = DelaySecond - 100; //减去100毫秒的延迟
            DelaySecond = DelaySecond + tbbullet.ShotDelay;
            if (DelaySecond > 300)
            {
                _this.ArriveTrigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddMilliseconds(DelaySecond),
                    () => { _this.DelayArrive(); });
                target.PushBullet(_this);
                //CoroutineFactory.NewCoroutine(TimeTrigger).MoveNext();
                Logger.Info("TargetBullet From={0} To={1} Bullet={2}", _this.Caster.ObjId, target.ObjId, tbbullet.Id);
                caster.BroadcastShootBullet(tbbullet.Id, caster.ObjId, target.ObjId, tbbullet.ShotDelay);
            }
            else
            {
                caster.BroadcastShootBullet(tbbullet.Id, caster.ObjId, target.ObjId, tbbullet.ShotDelay);
                var hitResult = _this.Caster.Attr.GetHitResult(_this.Target, _this.HitType);
                if (DelaySecond > 0)
                {
                    foreach (var i in _this.Buff)
                    {
                        _this.Target.AddBuff(i.Key, i.Value, _this.Caster, DelaySecond, hitResult, _this.Modify);
                    }
                }
                else
                {
                    foreach (var i in _this.Buff)
                    {
                        _this.Target.AddBuff(i.Key, i.Value, _this.Caster, 0, hitResult, _this.Modify);
                    }
                }
            }
        }
    }

    //对目标的子弹
    public class TargetBullet : Bullet
    {
        private static ITargetBullet mImpl;

        static TargetBullet()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (TargetBullet), typeof (TargetBulletDefaultImpl),
                o => { mImpl = (ITargetBullet) o; });
        }

        public TargetBullet(ObjCharacter caster,
                            BulletRecord tbbullet,
                            int nLevel,
                            eSkillHitType hitType,
                            ObjCharacter target)
        {
            mImpl.InitTargetBullet(this, caster, tbbullet, nLevel, hitType, target);
        }
    }


    public interface IDirectionBullet
    {
        void InitDirectionBullet(DirectionBullet _this,
                                 ObjCharacter caster,
                                 BulletRecord tbbullet,
                                 int nLevel,
                                 eSkillHitType hitType,
                                 Vector2 direction);
    }

    public class DirectionBulletDefaultImpl : IDirectionBullet
    {
        public void InitDirectionBullet(DirectionBullet _this,
                                        ObjCharacter caster,
                                        BulletRecord tbbullet,
                                        int nLevel,
                                        eSkillHitType hitType,
                                        Vector2 direction)
        {
            _this.Type = 1;
            _this.Caster = caster;
            _this.Direction = direction;
            _this.TableBullet = tbbullet;
            _this.HitType = hitType;
            foreach (var i in tbbullet.Buff)
            {
                _this.Buff[i] = nLevel;
            }
            //需要飞行计算
        }
    }

    //对方向的子弹
    public class DirectionBullet : Bullet
    {
        private static IDirectionBullet mImpl;

        static DirectionBullet()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (DirectionBullet),
                typeof (DirectionBulletDefaultImpl),
                o => { mImpl = (IDirectionBullet) o; });
        }

        public DirectionBullet(ObjCharacter caster,
                               BulletRecord tbbullet,
                               int nLevel,
                               eSkillHitType hitType,
                               Vector2 direction)
        {
            mImpl.InitDirectionBullet(this, caster, tbbullet, nLevel, hitType, direction);
        }
    }
}