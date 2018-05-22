namespace Scene
{
    /*
    public class ZoneManager
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public Dictionary<int, Zone> ZoneDic;
        public static int ZoneWidth = 10;
        public static int ZoneHeight = 10;
        public static int ZoneHorizontal = 10;
        public static int ZoneVertical = 10;
        public Scene SceneInfo { get; set; }
        public bool Init()
        {
            ZoneDic = new Dictionary<int, Zone>();

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    int id = i*10 + j;
                    //var z = new Zone(id,10,10);
                    //ZoneDic.Add(id,z);
                }
            }

            foreach (var zone in ZoneDic)
            {
				int id = zone.Value.Id;
                int nRow = id / ZoneHorizontal;
                int nCol = id % ZoneHorizontal;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        int nTempRow = nRow + (i - 1);
                        int nTempCol = nCol + (j - 1);
                        if (nTempRow < 0 || nTempRow >= ZoneHorizontal)
                        {
                            continue;
                        }
                        if (nTempCol < 0 || nTempCol >= ZoneVertical)
                        {
                            continue;
                        }
                        int iId = nTempRow * ZoneVertical + nTempCol;
                        zone.Value.SeenZoneList.Add(GetZone(iId));
                    }
                }
            }
            

            return true;
        }
        public Zone GetZone(int id)
        {
            if (ZoneDic.ContainsKey(id))
            {
                return ZoneDic[id];
            }            
            return null;
        }

        /// <summary>
        /// 角色进入到zone
        /// 第一次进入，目的zoneid为空根据保存的位置计算出id
        /// 告诉新id的zone的可见列表，去创建角色
        /// 进入zone的enter逻辑
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="id">目的zone</param>
        /// /// <param name="oldId">原本zone</param>
        public void EnterZone(ObjBase obj, int id = -1,int oldId = -1)
        {
            List<Zone> oldSeen = null;
            if (id == -1)
            {
                id = ConvertPostion(obj.GetPosition());
            }
            else
            {
                if (oldId != -1)
                {
                    Zone oldZone = null;
                    if (ZoneDic.TryGetValue(oldId,out oldZone))
                    {
                        oldSeen = oldZone.SeenZoneList;        
                    }
                }
            }
            Zone zone = GetZone(id);

            List<Zone> newSeen = zone.SeenZoneList;


            CreateObjMsg msg2Other = new CreateObjMsg();
            InitObjData data = obj.DumpObjData();
            msg2Other.Data.Add(data);

            foreach (var z in newSeen)
            {
                if (oldSeen == null || !oldSeen.Contains(z))
                {
                    z.PushAction2AllPlayer((player) =>
                    {
                        player.Proxy.CreateObj(msg2Other);
                    }, obj.ObjId);

                    CreateObjMsg msg2Me = new CreateObjMsg();
                    foreach (var pair in z.ObjDict)
                    {
                        if (pair.Key == obj.ObjId)
                        {
                            continue;
                        }
                        ObjBase o = pair.Value;

                        InitObjData d = o.DumpObjData();
                        msg2Me.Data.Add(d);
                    }
                     ObjPlayer p = obj as ObjPlayer;
                    if (p != null)
                    {
                        p.Proxy.CreateObj(msg2Me);
                    }
                }
            }
            //zone.EnterZone(obj);
        }
        /// <summary>
        /// 角色移出zone
        /// 当玩家掉线退出时，可能没有目标zone
        /// 广播告诉旧的zone删除移出角色
        /// 告诉自己删除旧的zone看见自己
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="id">目标zoneid</param>
        /// 
        public void LeveZone(ObjBase obj, int id = -1)
        {
            Logger.Info("LeaveZone obj={0},zone={1}" , obj.ObjId,id);
            List<Zone> newSeen = null;
            if (id != -1)
            {
                Zone zone = GetZone(id);
                newSeen = zone.SeenZoneList;
            }

            List<Zone> oldSeen = obj.Zone.SeenZoneList;

            foreach (var z in oldSeen)
            {
//                 if (newSeen == null)
//                 {
//                     Logger.Info("LeaveZone Miss Me obj={0},zone={1}", obj.ObjId, z.mZoneId);
//                     
//                     z.PushAction2AllPlayer((player) =>
//                     {
//                         player.Proxy.DeleteCharacter(obj.ObjId);
//                     }, obj.ObjId);
//                 }
//                 else if (!newSeen.Contains(z))
//                 {
//                     Logger.Info("LeaveZone Miss Me obj={0},zone={1}", obj.ObjId, z.mZoneId);
//                     z.PushAction2AllPlayer((playerOther) =>
//                     {
//                         playerOther.Proxy.DeleteCharacter(obj.ObjId);
//                     }, obj.ObjId);
// 
// 
//                     Logger.Info("LeaveZone Miss Other obj={0},zone={1}", obj.ObjId, z.mZoneId);
//                     ObjPlayer playerSelf = obj as ObjPlayer;
//                     if (playerSelf != null)
//                     {
//                         foreach (var objBase in z.ObjDict)
//                         {
//                             playerSelf.Proxy.DeleteCharacter(objBase.Key);
//                         }
//                     }
//                 }

                if (newSeen == null || !newSeen.Contains(z))
                {
                    Logger.Info("LeaveZone Miss Me obj={0},zone={1}", obj.ObjId, z.mZoneId);
                    z.PushAction2AllPlayer((player) =>
                    {
                        player.Proxy.DeleteCharacter(obj.ObjId);
                    }, obj.ObjId);
                }

                if (newSeen != null && !newSeen.Contains(z))
                {
                    Logger.Info("LeaveZone Miss Other obj={0},zone={1}", obj.ObjId, z.mZoneId);
                    ObjPlayer player = obj as ObjPlayer;
                    if (player != null)
                    {
                        foreach (var objBase in z.ObjDict)
                        {
                            player.Proxy.DeleteCharacter(objBase.Key);
                        }
                    }
                }
            }

			//obj.Zone.LevelZone(obj);
        }
        public void UpdatePostion(Vector2 v2Old,ObjBase obj)
        {
            int nZone = ConvertPostion(obj.GetPosition());
			int oldId = obj.Zone.Id;
            if (nZone != oldId)
            {
                LeveZone(obj, nZone);
                EnterZone(obj, nZone, oldId);
            }
        }
        public int ConvertPostion(Vector2 v2Pos)
        {
            return (int)(v2Pos.X / ZoneWidth) + (int)(v2Pos.Y / ZoneHeight) * ZoneHorizontal;
        }
        public void DoSeenZone(int id, Action<Zone> act)
        {
            int nRow = id / ZoneHorizontal;
            int nCol = id % ZoneHorizontal;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int nTempRow = nRow + (i - 1);
                    int nTempCol = nCol + (j - 1);
                    if (nTempRow < 0 || nTempRow >= ZoneHorizontal)
                    {
                        continue;
                    }
                    if (nTempCol < 0 || nTempCol >= ZoneVertical)
                    {
                        continue;
                    }
                    int iId = nTempRow * ZoneVertical + nTempCol;
                    var z = ZoneDic[iId];
                    act(z);
                }
            }
        }
    }
	 */
}