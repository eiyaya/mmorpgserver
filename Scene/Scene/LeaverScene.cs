#region using

using DataContract;
using DataTable;

#endregion

namespace Scene
{
    public interface ILeaverScene
    {
        bool EnterScene(LeaverScene _this, ObjBase obj);
        bool Init(LeaverScene _this, SceneParam param);
        bool LeaveScene(LeaverScene _this, ObjBase obj, ReasonType reason = ReasonType.VisibilityChanged);
    }

    public class LeaverSceneDefaultImpl : ILeaverScene
    {
        public bool Init(LeaverScene _this, SceneParam param)
        {
            //Obstacle.Load();
            _this.Active = false;
            _this.TableSceneData = Table.GetScene(_this.TypeId);
            //Param = param;
            return true;
        }

        public bool EnterScene(LeaverScene _this, ObjBase obj)
        {
            _this.AddObj(obj);
            obj.EnterScene(_this);
            return true;
        }

        public bool LeaveScene(LeaverScene _this, ObjBase obj, ReasonType reason = ReasonType.VisibilityChanged)
        {
            obj.LeavelScene();
            _this.RemoveObj(obj);
            return true;
        }
    }

    //可以回到最近的非阻挡点 Scene.FindNearestValidPosition()
    public class LeaverScene : Scene
    {
        private static ILeaverScene mImpl;

        static LeaverScene()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (LeaverScene), typeof (LeaverSceneDefaultImpl),
                o => { mImpl = (ILeaverScene) o; });
        }

        public override bool EnterScene(ObjBase obj)
        {
            return mImpl.EnterScene(this, obj);
        }

        public override bool Init(SceneParam param)
        {
            return mImpl.Init(this, param);
        }

        public override bool LeaveScene(ObjBase obj, ReasonType reason = ReasonType.VisibilityChanged)
        {
            return mImpl.LeaveScene(this, obj, reason);
        }

        public override void OnCreate()
        {
        }

        public override void OnNpcDie(ObjNPC npc, ulong characterId = 0)
        {
        }

        public override void OnPlayerDie(ObjPlayer player, ulong characterId = 0)
        {
        }

        //这里只可以写场景逻辑，不可以同步数据，因为这时候客户端有可能还没加载好
        public override void OnPlayerEnter(ObjPlayer player)
        {
        }

        //这里可以同步一些数据，这时候客户端已经都ok了
        public override void OnPlayerEnterOver(ObjPlayer player)
        {
        }
    }
}