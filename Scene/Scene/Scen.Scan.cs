#region using

using System;

#endregion

namespace Scene
{
    public partial class SceneDefaultImpl
    {
        //按照形状拿到所有角色来执行Action
        public void SceneShapeAction(Scene _this, Shape shape, Action<ObjCharacter> action)
        {
            var currentZone = GetZone(_this, Pos2ZoneId(_this, shape.Pos.X, shape.Pos.Y));
            if (null == currentZone)
                return;
            foreach (var zone in currentZone.VisibleZoneList)
            {
                if (!shape.IsColliderZone(zone))
                {
                    continue;
                }
                foreach (var character in zone.ObjDict)
                {
                    if (character.Value.IsCharacter() && shape.IsColliderObj(character.Value))
                    {
                        action(character.Value as ObjCharacter);
                    }
                }
            }
        }
    }

    public partial class Scene
    {
        //按照形状拿到所有角色来执行Action
        public void SceneShapeAction(Shape shape, Action<ObjCharacter> action)
        {
            mImpl.SceneShapeAction(this, shape, action);
        }
    }
}