#region using

using DataContract;

#endregion

namespace Scene
{
    public interface IObjBoss
    {
        DamageList CollectDamageList(ObjBoss _this);
        void InitObjBoss(ObjBoss _this);
        void OnDamage(ObjBoss _this, ObjCharacter enemy, int damage);
    }

    public class ObjBossDefaultImpl : IObjBoss
    {
        //构造函数
        public void InitObjBoss(ObjBoss _this)
        {
            _this.mDropOnDie = false;
        }

        public DamageList CollectDamageList(ObjBoss _this)
        {
            var ret = _this.damageList;
            _this.damageList = new DamageList();
            return ret;
        }

        public void OnDamage(ObjBoss _this, ObjCharacter enemy, int damage)
        {
            ObjNPC.GetImpl().OnDamage(_this, enemy, damage);
            enemy = enemy.GetRewardOwner();
            if (enemy.GetObjType() == ObjType.PLAYER)
            {
                var unit = new DamageUnit();
                unit.CharacterId = enemy.ObjId;
                unit.Damage = damage;
                _this.damageList.Data.Add(unit);
            }
        }
    }

    public class ObjBoss : ObjNPC
    {
        private static IObjBoss mImpl;

        static ObjBoss()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (ObjBoss), typeof (ObjBossDefaultImpl),
                o => { mImpl = (IObjBoss) o; });
        }

        //构造函数
        public ObjBoss()
        {
            mImpl.InitObjBoss(this);
        }

        public DamageList damageList = new DamageList();

        public DamageList CollectDamageList()
        {
            return mImpl.CollectDamageList(this);
        }

        public override void OnDamage(ObjCharacter enemy, int damage)
        {
            mImpl.OnDamage(this, enemy, damage);
        }
    }
}