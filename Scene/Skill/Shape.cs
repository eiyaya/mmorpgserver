#region using

using System;
using Mono.GameMath;

#endregion

namespace Scene
{
    public interface IShape
    {
        bool CheckZoneDistance(Shape _this, Zone zone, float distance);
        bool CheckZoneDistance(Zone zone, Vector2 ps, float distance);
        float Clamp(float value, float min, float max);
        void InitShape(Shape _this, Vector2 pos);
    }

    public class ShapeDefaultImpl : IShape
    {
        public void InitShape(Shape _this, Vector2 pos)
        {
            _this.Pos = pos;
        }

        #region     扩展方法

        //获得某个值，在最小最大值之间，最接近的值
        public float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                return min;
            }
            if (value > max)
            {
                return max;
            }
            return value;
        }

        //检查到Zode的距离
        public bool CheckZoneDistance(Shape _this, Zone zone, float distance)
        {
            var initpos = zone.GetInitPos();
            var closestX = Clamp(_this.Pos.X, initpos.X, initpos.X + zone.Width);
            var closestY = Clamp(_this.Pos.Y, initpos.Y, initpos.Y + zone.Width);
            var distanceX = _this.Pos.X - closestX;
            var distanceY = _this.Pos.Y - closestY;
            if (distanceX*distanceX + distanceY*distanceY <= distance*distance)
            {
                return true;
            }
            return false;
        }

        //检查某个点（ps）到Zone的距离
        public bool CheckZoneDistance(Zone zone, Vector2 ps, float distance)
        {
            var initpos = zone.GetInitPos();
            var closestX = Clamp(ps.X, initpos.X, initpos.X + zone.Width);
            var closestY = Clamp(ps.Y, initpos.Y, initpos.Y + zone.Width);
            var distanceX = ps.X - closestX;
            var distanceY = ps.Y - closestY;
            if (distanceX*distanceX + distanceY*distanceY <= distance)
            {
                return true;
            }
            return false;
        }

        #endregion
    }

    #region     形状基类

    public abstract class Shape
    {
        private static IShape mImpl;

        static Shape()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (Shape), typeof (ShapeDefaultImpl),
                o => { mImpl = (IShape) o; });
        }

        public Shape(Vector2 pos)
        {
            Pos = pos;
        }

        public Vector2 Pos { get; set; }

        public static IShape GetImpl()
        {
            return mImpl;
        }

        #region     筛选方法

        //Zone级的筛选
        public abstract bool IsColliderZone(Zone zone);

        //Obj级的筛选
        public abstract bool IsColliderObj(ObjBase obj);

        #endregion

        #region     扩展方法

        //获得某个值，在最小最大值之间，最接近的值
        public static float Clamp(float value, float min, float max)
        {
            return mImpl.Clamp(value, min, max);
        }

        //检查到Zode的距离
        public bool CheckZoneDistance(Zone zone, float distance)
        {
            return mImpl.CheckZoneDistance(this, zone, distance);
        }

        //检查某个点（ps）到Zone的距离
        public static bool CheckZoneDistance(Zone zone, Vector2 ps, float distance)
        {
            return mImpl.CheckZoneDistance(zone, ps, distance);
        }

        #endregion
    }

    #endregion

    #region     圆形

    public interface ICircleShape
    {
        bool IsColliderObj(CircleShape _this, ObjBase obj);
        bool IsColliderZone(CircleShape _this, Zone zone);
    }

    public class CircleShapeDefaultImpl : ICircleShape
    {
        public bool IsColliderZone(CircleShape _this, Zone zone)
        {
            return _this.CheckZoneDistance(zone, _this.mRadius);
        }

        public bool IsColliderObj(CircleShape _this, ObjBase obj)
        {
            var dif = _this.Pos - obj.GetPosition();
            if (dif.LengthSquared() > _this.mRadius*_this.mRadius)
            {
                return false;
            }
            return true;
        }
    }

    public class CircleShape : Shape
    {
        private static ICircleShape mImpl;

        static CircleShape()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (CircleShape), typeof (CircleShapeDefaultImpl),
                o => { mImpl = (ICircleShape) o; });
        }

        public CircleShape(Vector2 Pos, float Radius)
            : base(Pos)
        {
            mRadius = Radius;
        }

        public float mRadius;

        public override bool IsColliderObj(ObjBase obj)
        {
            return mImpl.IsColliderObj(this, obj);
        }

        public override bool IsColliderZone(Zone zone)
        {
            return mImpl.IsColliderZone(this, zone);
        }
    }

    #endregion

    #region     扇形

    public interface IFanShape
    {
        void InitFanShape(FanShape _this, Vector2 pos, Vector2 dir, float radius, int degree);
        bool IsColliderObj(FanShape _this, ObjBase obj);
        bool IsColliderZone(FanShape _this, Zone zone);
    }

    public class FanShapeDefaultImpl : IFanShape
    {
        public void InitFanShape(FanShape _this, Vector2 pos, Vector2 dir, float radius, int degree)
        {
            _this.mRadius = radius;
            _this.mRegree = degree;
            _this.mDir = dir;
            _this.mDir.Normalize();
        }

        public bool IsColliderZone(FanShape _this, Zone zone)
        {
            return _this.CheckZoneDistance(zone, _this.mRadius);
        }

        public bool IsColliderObj(FanShape _this, ObjBase obj)
        {
            var dif = obj.GetPosition() - _this.Pos;

            var l = dif.LengthSquared();
            if (l > _this.mRadius*_this.mRadius)
            {
                return false;
            }

            // 如果两个点基本重合，也算命中了
            if (l < 4.0f)
            {
                return true;
            }

            dif.Normalize();
            if (Vector2.Dot(_this.mDir, dif) < Math.Cos((float) Math.PI*_this.mRegree/2/180))
            {
                return false;
            }
            return true;
        }
    }

    public class FanShape : Shape
    {
        private static IFanShape mImpl;

        static FanShape()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (FanShape), typeof (FanShapeDefaultImpl),
                o => { mImpl = (IFanShape) o; });
        }

        public FanShape(Vector2 pos, Vector2 dir, float radius, int degree)
            : base(pos)
        {
            mImpl.InitFanShape(this, pos, dir, radius, degree);
        }

        public Vector2 mDir;
        public float mRadius;
        public int mRegree;

        public override bool IsColliderObj(ObjBase obj)
        {
            return mImpl.IsColliderObj(this, obj);
        }

        public override bool IsColliderZone(Zone zone)
        {
            return mImpl.IsColliderZone(this, zone);
        }
    }

    #endregion

    #region     矩形

    public interface IOrientedBox
    {
        void InitOrientedBox(OrientedBox _this, Vector2 pos, float w, float l, Vector2 dir);
        bool IsColliderObj(OrientedBox _this, ObjBase obj);
        bool IsColliderZone(OrientedBox _this, Zone zone);
    }

    public class OrientedBoxDefaultImpl : IOrientedBox
    {
        public void InitOrientedBox(OrientedBox _this, Vector2 pos, float w, float l, Vector2 dir)
        {
            _this.mHalfWidth = w*0.5f;
            _this.mHalfLength = l*0.5f + 1.0f;
            _this.mDir = dir;
            _this.mCenter = pos + dir*l*0.5f - dir*1.0f; // 向后稍微一点，能打到跟自己重合的人
        }

        public bool IsColliderZone(OrientedBox _this, Zone zone)
        {
            return Shape.CheckZoneDistance(zone, _this.mCenter,
                _this.mHalfWidth*_this.mHalfWidth + _this.mHalfLength*_this.mHalfLength);
        }

        public bool IsColliderObj(OrientedBox _this, ObjBase obj)
        {
            var newx = _this.mDir.Y*(obj.GetPosition().X - _this.mCenter.X) -
                       _this.mDir.X*(obj.GetPosition().Y - _this.mCenter.Y);
            var newy = _this.mDir.X*(obj.GetPosition().X - _this.mCenter.X) +
                       _this.mDir.Y*(obj.GetPosition().Y - _this.mCenter.Y);

            return (newy > -_this.mHalfLength) && (newy < _this.mHalfLength)
                   && (newx > -_this.mHalfWidth) && (newx < _this.mHalfWidth);
        }
    }

    public class OrientedBox : Shape
    {
        private static IOrientedBox mImpl;

        static OrientedBox()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (OrientedBox), typeof (OrientedBoxDefaultImpl),
                o => { mImpl = (IOrientedBox) o; });
        }

        public OrientedBox(Vector2 pos, float w, float l, Vector2 dir)
            : base(pos)
        {
            mImpl.InitOrientedBox(this, pos, w, l, dir);
        }

        public Vector2 mCenter;
        public Vector2 mDir;
        public float mHalfLength;
        public float mHalfWidth;

        public override bool IsColliderObj(ObjBase obj)
        {
            var newx = mDir.Y*(obj.GetPosition().X - mCenter.X) - mDir.X*(obj.GetPosition().Y - mCenter.Y);
            var newy = mDir.X*(obj.GetPosition().X - mCenter.X) + mDir.Y*(obj.GetPosition().Y - mCenter.Y);

            return (newy > -mHalfLength) && (newy < mHalfLength)
                   && (newx > -mHalfWidth) && (newx < mHalfWidth);
        }

        public override bool IsColliderZone(Zone zone)
        {
            return CheckZoneDistance(zone, mCenter, mHalfWidth*mHalfWidth + mHalfLength*mHalfLength);
        }
    }

    #endregion
}