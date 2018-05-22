#region using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DataTable;
using NLog;

#endregion

namespace Scene
{
    public interface ISceneRegister
    {
        Scene CreateScene(int typeId, int serverId, ulong guid);
    }

    public class SceneRegisterDefaultImpl : ISceneRegister
    {
        public Scene CreateScene(int typeId, int serverId, ulong guid)
        {

            var tbScene = Table.GetScene(typeId);
// 			if (serverId == -1 && 0==tbScene.CanCrossServer)
// 			{
// 				try
// 				{
// 					var logger = LogManager.GetCurrentClassLogger();
// 					if (logger != null)
// 					{
// 						StackTrace st = new StackTrace();
// 						logger.Error("serverid error!!!!  serverid = {0}, typeId = {1}, stack trace is :{2}", serverId, typeId, st.ToString());
// 					}
// 				}
// 				catch (Exception)
// 				{
// 
// 				}
// 			}

            var scriptId = tbScene.ScriptId;
            if (scriptId != -1)
            {
                var t = string.Empty;
                if (SceneRegister.sSceneType.TryGetValue(scriptId, out t))
                {
                    var scene = (Scene) Activator.CreateInstance(SceneServer.Instance.UpdateManager.GetNewType(t));
                    scene.TypeId = typeId;
                    scene.ServerId = serverId;
                    scene.Guid = guid;
                    return scene;
                }
            }

            return new Scene {TypeId = typeId, ServerId = serverId, Guid = guid};
        }
    }

    public class SceneRegister
    {
        private static ISceneRegister mImpl;

        public static Dictionary<int, string> sSceneType = new Dictionary<int, string>
        {
            {2200, "Scene.UnionBattle"},  // 跨服盟战
            {10000, "Scene.UniformSingleDungeon"}, //单人副本通用脚本
            {11000, "Scene.UniformTeamDungeon"}, //组队副本通用脚本
            {12000, "Scene.UniformMultyDungeon"}, //多人活动副本通用脚本
            {13000, "Scene.UniformHellAndGhostDungeon"}, //单人或组队副本脚本(亡灵城堡和地狱监牢)
            {101000, "Scene.SceneCity"},
            {110000, "Scene.P1vP1Scene110000"},
            {110001, "Scene.DuelDungeon"}, //角斗副本
            {120000, "Scene.CityActivity120000"}, //家园活动1
             {1200010, "Scene.CityActivity1200010"}, //家园活动2 
//             {1200011, "Scene.CityActivity1200011"}, //家园活动2 二阶
//             {1200012, "Scene.CityActivity1200012"}, //家园活动2 三阶
//             {1200013, "Scene.CityActivity1200013"}, //家园活动2 四阶
            {120002, "Scene.CityActivity120002"}, //家园活动3

            {200000, "Scene.BattleScene200000"}, //战场
            {200001, "Scene.BattleScene200001"}, //战场
            {200002, "Scene.BattleScene200002"}, //战场
            {200003, "Scene.BattleScene200003"}, //战场
            {200004, "Scene.BattleScene200004"}, //战场

            {210000, "Scene.BattleScene210000"}, //寒霜据点战场

            {210001, "Scene.CastleCraft"}, //古堡争霸战场

            {210100, "Scene.AllianceWar"}, //攻城战

			{220000, "Scene.KillZone"}, //经验岛
            {230000, "Scene.PetIsland"}, //灵兽岛
			{240000, "Scene.AcientBattleField"}, //古域战场
            {250000, "Scene.XpSkillDungeon"}, //Xp技能
            {260000, "Scene.MaYaWeaponDungeon"}, //玛雅武器引导副本
			{270000, "Scene.BossHome"}, //BossHome
            {280000, "Scene.JewellWars"}, //吃鸡峡谷
			
            {300000, "Scene.BloodCastle300000"}, //血色城堡 难度0
            {300001, "Scene.BloodCastle300001"}, //血色城堡 难度1
            {300002, "Scene.BloodCastle300002"}, //血色城堡 难度2
            {300003, "Scene.BloodCastle300003"}, //血色城堡 难度3
            {300004, "Scene.BloodCastle300004"}, //血色城堡 难度4
            {300005, "Scene.BloodCastle300005"}, //血色城堡 难度5
            {300006, "Scene.BloodCastle300006"}, //血色城堡 难度6
            {300007, "Scene.BloodCastle300007"}, //血色城堡 难度7

            {300100, "Scene.DevilSquare300100"}, //恶魔广场 难度0
            {300101, "Scene.DevilSquare300101"}, //恶魔广场 难度1
            {300102, "Scene.DevilSquare300102"}, //恶魔广场 难度2
            {300103, "Scene.DevilSquare300103"}, //恶魔广场 难度3
            {300104, "Scene.DevilSquare300104"}, //恶魔广场 难度4
            {300105, "Scene.DevilSquare300105"}, //恶魔广场 难度5
            {300106, "Scene.DevilSquare300106"}, //恶魔广场 难度6
            {300107, "Scene.DevilSquare300107"}, //恶魔广场 难度7

            
            {400000, "Scene.WorldBoss"}, //世界boss
            
            {500000, "Scene.ExpBattleField"}, //古战场

            {600000, "Scene.MeetGoldcs"}, //接金币

            {700000, "Scene.FrozenThrone"}, //冰封王座
            {810000, "Scene.MieShiWar"}, //灭世之战-先锋部队
            {810001, "Scene.MieShiWar"}, //灭世之战-兵临城下
            {810002, "Scene.MieShiWar"}, //灭世之战-大决战
            {900000,"Scene.ClimbingTower"}, //爬塔
        };

        static SceneRegister()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (SceneRegister), typeof (SceneRegisterDefaultImpl),
                o => { mImpl = (ISceneRegister) o; });
        }

        public static Scene CreateScene(int typeId, int serverId, ulong guid)
        {
            return mImpl.CreateScene(typeId, serverId, guid);
        }
    }
}