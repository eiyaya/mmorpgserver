using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataTable;
using EventSystem;

namespace Scene
{
    public static class StaticParam
    {
         //黄金部队 地图统领  世界boss表
        public static Dictionary<int, WorldBOSSRecord> BossDict = new Dictionary<int, WorldBOSSRecord>();
        static StaticParam()
        {
            EventDispatcher.Instance.AddEventListener(ReloadTableEvent.EVENT_TYPE, ReloadTable);
            Init();
        }

        private static void Init()
        {
            ReLoadBossDict();
        }

        private static void ReloadTable(IEvent ievent)
        {
            var v = ievent as ReloadTableEvent;
            if (v.tableName == "WorldBOSS")
            {
                ReLoadBossDict();
            }
        }

        public static void ReLoadBossDict()
        {
            BossDict.Clear();
            Table.ForeachWorldBOSS(o =>
            {
                var sceneNpc = Table.GetSceneNpc(o.SceneNpc);
                if (sceneNpc != null)
                {
                    var chaBase = Table.GetCharacterBase(sceneNpc.DataID);
                    if (chaBase != null)
                    {
                        var npcBase = Table.GetNpcBase(chaBase.ExdataId);
                        if (npcBase != null)
                        {
                            if (!BossDict.ContainsKey(npcBase.Id))
                            {
                                BossDict.Add(npcBase.Id, o);
                            }
                        }
                    }
                }
                return true;
            });
        }
    }
}
