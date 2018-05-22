#region using

using System;
using System.Collections;
using DataContract;
using DataTable;
using EventSystem;
using Scorpion;
using Mono.GameMath;
using Shared;

#endregion

namespace Scene
{
    public interface IPositionInfo
    {
        Vector2 GetDiff(float direction, float distance);

        /// <summary>
        ///     设置朝向
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="direction"></param>
        void SetDirection(PositionInfo _this, Vector2 direction);

        void SetPosition(PositionInfo _this, float fPosX, float fPoxY);
    }

    public class PositionInfoDefaultImpl : IPositionInfo
    {
        //设置坐标
        public void SetPosition(PositionInfo _this, float fPosX, float fPoxY)
        {
            _this.mPos = new Vector2(fPosX, fPoxY);
        }

        // 设置朝向
        /// <summary>
        ///     设置朝向
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="direction"></param>
        public void SetDirection(PositionInfo _this, Vector2 direction)
        {
            _this.mDirection = direction;
        }

        /*  运算函数  */

        /// <summary>
        ///     根据方向和距离计算X和Y的值
        /// </summary>
        /// <param name="direction">方向</param>
        /// <param name="distance">距离</param>
        /// <returns></returns>
        public Vector2 GetDiff(float direction, float distance)
        {
            var diffX = (float) (distance*Math.Cos(direction));
            var diffY = (float) (distance*Math.Sin(direction));
            return new Vector2(diffX, diffY);
        }
    }

    //位置信息
    public class PositionInfo
    {
        private static IPositionInfo mImpl;

        static PositionInfo()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (PositionInfo), typeof (PositionInfoDefaultImpl),
                o => { mImpl = (IPositionInfo) o; });
        }

        public Vector2 mDirection;
        public Vector2 mPos;

        public Vector2 Direction
        {
            get { return mDirection; }
            set { mDirection = value; }
        }

        public Vector2 Position
        {
            get { return mPos; }
            set { mPos = value; }
        }

        /*  运算函数  */

        /// <summary>
        ///     根据方向和距离计算X和Y的值
        /// </summary>
        /// <param name="direction">方向</param>
        /// <param name="distance">距离</param>
        /// <returns></returns>
        public static Vector2 GetDiff(float direction, float distance)
        {
            return mImpl.GetDiff(direction, distance);
        }

        // 设置朝向
        /// <summary>
        ///     设置朝向
        /// </summary>
        /// <param name="direction"></param>
        public void SetDirection(Vector2 direction)
        {
            mImpl.SetDirection(this, direction);
        }

        //设置坐标
        public void SetPosition(float fPosX, float fPoxY)
        {
            mImpl.SetPosition(this, fPosX, fPoxY);
        }
    }

    //工具函数

    public interface IUtility
    {
        float DividePrecision(int value);
        void GetItem(ulong characterId, int itemId, int count);
        Vector2 GetRandomPosFromTable(int minId, int maxId);
        Vector2 GetRandomPostion();
        PositionData MakePositionData(float x, float y, float dirx, float diry);
        PositionData MakePositionDataByPosAndDir(Vector2 pos, Vector2 dir);
        Vector2 MakeVectorDividePrecision(int x, int y);
        Vector2Int32 MakeVectorMultiplyPrecision(float x, float y);
        int MultiplyPrecision(float value);
        /// <summary>
        /// 随机灭世副本的进入点坐标
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sceneId"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        Vector2 RandomMieShiEntryPosition(eSceneType type, int sceneId, Vector2 pos);
    }

    public class UtilityDefaultImpl : IUtility
    {
        private IEnumerator GetItemCoroutine(Coroutine co, ulong characterId, int itemId, int count)
        {
            var result = SceneServer.Instance.LogicAgent.GiveItem(characterId, itemId, count,-1);
            yield return result.SendAndWaitUntilDone(co);
        }

        public int MultiplyPrecision(float value)
        {
            return (int) (value*Utility.FLOAT2INT_PRECISION);
        }

        public float DividePrecision(int value)
        {
            return value*Utility.INT2FLOAT_PRECISION;
        }

        //make a PositionData with parameter
        public PositionData MakePositionData(float x, float y, float dirx, float diry)
        {
            var Pos = new PositionData
            {
                Pos = new Vector2Int32
                {
                    x = MultiplyPrecision(x),
                    y = MultiplyPrecision(y)
                },
                Dir = new Vector2Int32
                {
                    x = MultiplyPrecision(dirx),
                    y = MultiplyPrecision(diry)
                }
            };
            return Pos;
        }

        public PositionData MakePositionDataByPosAndDir(Vector2 pos, Vector2 dir)
        {
            var Pos = new PositionData
            {
                Pos = new Vector2Int32
                {
                    x = MultiplyPrecision(pos.X),
                    y = MultiplyPrecision(pos.Y)
                },
                Dir = new Vector2Int32
                {
                    x = MultiplyPrecision(dir.X),
                    y = MultiplyPrecision(dir.Y)
                }
            };
            return Pos;
        }

        public Vector2 MakeVectorDividePrecision(int x, int y)
        {
            return new Vector2(DividePrecision(x), DividePrecision(y));
        }

        public Vector2Int32 MakeVectorMultiplyPrecision(float x, float y)
        {
            return new Vector2Int32 {x = MultiplyPrecision(x), y = MultiplyPrecision(y)};
        }

        public Vector2 GetRandomPosFromTable(int minId, int maxId)
        {
            var randPosId = MyRandom.Random(minId, maxId);
            var tbRandomPos = Table.GetRandomCoordinate(randPosId);
            return new Vector2(tbRandomPos.PosX, tbRandomPos.PosY);
        }

        public Vector2 GetRandomPostion()
        {
            var diffX = MyRandom.Random(-1.0f, 1.0f);
            if (diffX < 0)
            {
                diffX -= 1.5f;
            }
            else
            {
                diffX += 1.5f;
            }
            var diffY = MyRandom.Random(-1.0f, 1.0f);
            if (diffY < 0)
            {
                diffY -= 1.5f;
            }
            else
            {
                diffY += 1.5f;
            }
            var ret = new Vector2(diffX, diffY);
            return ret;
        }

        public void GetItem(ulong characterId, int itemId, int count)
        {
            CoroutineFactory.NewCoroutine(GetItemCoroutine, characterId, itemId, count).MoveNext();
        }

        public Vector2 RandomMieShiEntryPosition(eSceneType type, int sceneId, Vector2 pos)
        {
            if (eSceneType.Fuben != type)
                return pos;

            var NewPos = pos;
            Table.ForeachMieShi(record =>
            {
                if (record.FuBenID == sceneId)
                {
                    var index = MyRandom.Random(0, 4);
                    NewPos = new Vector2(record.Entry_x[index],record.Entry_z[index]);

                    return false;
                }
                return true;
            });

            return NewPos;
        }
    }

    public static class Utility
    {
        //float to int 精度
        public const float FLOAT2INT_PRECISION = 100.0f;
        //int to float 精度
        public const float INT2FLOAT_PRECISION = (1/FLOAT2INT_PRECISION);
        private static IUtility mImpl;

        static Utility()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (Utility), typeof (UtilityDefaultImpl),
                o => { mImpl = (IUtility) o; });
        }

        public static float DividePrecision(int value)
        {
            return mImpl.DividePrecision(value);
        }

        public static void GetItem(ulong characterId, int itemId, int count)
        {
            mImpl.GetItem(characterId, itemId, count);
        }

        public static Vector2 GetRandomPosFromTable(int minId, int maxId)
        {
            return mImpl.GetRandomPosFromTable(minId, maxId);
        }

        public static Vector2 GetRandomPostion()
        {
            return mImpl.GetRandomPostion();
        }

        //make a PositionData with parameter
        public static PositionData MakePositionData(float x, float y, float dirx, float diry)
        {
            return mImpl.MakePositionData(x, y, dirx, diry);
        }

        public static PositionData MakePositionDataByPosAndDir(Vector2 pos, Vector2 dir)
        {
            return mImpl.MakePositionDataByPosAndDir(pos, dir);
        }

        public static Vector2 MakeVectorDividePrecision(int x, int y)
        {
            return mImpl.MakeVectorDividePrecision(x, y);
        }

        public static Vector2Int32 MakeVectorMultiplyPrecision(float x, float y)
        {
            return mImpl.MakeVectorMultiplyPrecision(x, y);
        }

        public static int MultiplyPrecision(float value)
        {
            return mImpl.MultiplyPrecision(value);
        }

        public static Vector2 RandomMieShiEntryPosition(eSceneType type, int sceneId, Vector2 pos)
        {
            return mImpl.RandomMieShiEntryPosition(type, sceneId, pos);
        }
    }

    //数学函数库
    public interface IMyMath
    {
        float Angle2Radian(float angle);
        Vector2 Angle2Vector(float angle);

        /// <summary>
        ///     判断目标点是否在我朝向的扇形区域内
        /// </summary>
        /// <param name="myPos">我的位置</param>
        /// <param name="myDir">我的朝向</param>
        /// <param name="targetPos">目标位置</param>
        /// <param name="distance">距离</param>
        /// <param name="radian">角度弧度</param>
        /// /
        /// /
        /// /
        /// / )radian
        /// o------------>my dir
        /// \ )radian
        /// \
        /// \
        /// \
        /// (pos)
        bool IsInSector(Vector2 myPos, Vector2 myDir, Vector2 targetPos, float distance, float radian = MyMath.PI_4);

        float Radian2Angle(float radian);
    }

    public class MyMathDefaultImpl : IMyMath
    {
        //弧度转换成角度
        public float Radian2Angle(float radian)
        {
            return radian*MyMath.ONE_DIVIDE_PI*180.0f;
        }

        //角度转弧度
        public float Angle2Radian(float angle)
        {
            return angle*MyMath.PI_DEVIDE_180;
        }

        //角度转向量
        public Vector2 Angle2Vector(float angle)
        {
            var radian = Angle2Radian(angle);
            return new Vector2((float) Math.Cos(radian), (float) Math.Sin(radian));
        }

        /// <summary>
        ///     判断目标点是否在我朝向的扇形区域内
        /// </summary>
        /// <param name="myPos">我的位置</param>
        /// <param name="myDir">我的朝向</param>
        /// <param name="targetPos">目标位置</param>
        /// <param name="distance">距离</param>
        /// <param name="radian">角度弧度</param>
        /// /
        /// /
        /// /
        /// / )radian
        /// o------------>my dir
        /// \ )radian
        /// \
        /// \
        /// \
        /// (pos)
        public bool IsInSector(Vector2 myPos,
                               Vector2 myDir,
                               Vector2 targetPos,
                               float distance,
                               float radian = MyMath.PI_4)
        {
            var dif = targetPos - myPos;
            if (dif.LengthSquared() > distance*distance)
            {
                return false;
            }
            dif.Normalize();
            if (Vector2.Dot(myDir, dif) < Math.Cos(radian))
            {
                return false;
            }
            return true;
        }
    }

    public static class MyMath
    {
        private static IMyMath mImpl;
        //1比PI
        public const float ONE_DIVIDE_PI = 0.3183f;
        //pi
        public const float PI = 3.1416f;
        //2分之pi
        public const float PI_2 = 1.5707f;
        //4分之pi
        public const float PI_4 = 0.7853f;
        public const float PI_DEVIDE_180 = 0.0174f;
        //2倍pi
        public const float TWO_PI = 6.283f;

        static MyMath()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (MyMath), typeof (MyMathDefaultImpl),
                o => { mImpl = (IMyMath) o; });
        }

        //角度转弧度
        public static float Angle2Radian(float angle)
        {
            return mImpl.Angle2Radian(angle);
        }

        //角度转向量
        public static Vector2 Angle2Vector(float angle)
        {
            return mImpl.Angle2Vector(angle);
        }

        /// <summary>
        ///     判断目标点是否在我朝向的扇形区域内
        /// </summary>
        /// <param name="myPos">我的位置</param>
        /// <param name="myDir">我的朝向</param>
        /// <param name="targetPos">目标位置</param>
        /// <param name="distance">距离</param>
        /// <param name="radian">角度弧度</param>
        /// /
        /// /
        /// /
        /// / )radian
        /// o------------>my dir
        /// \ )radian
        /// \
        /// \
        /// \
        /// (pos)
        public static bool IsInSector(Vector2 myPos,
                                      Vector2 myDir,
                                      Vector2 targetPos,
                                      float distance,
                                      float radian = PI_4)
        {
            return mImpl.IsInSector(myPos, myDir, targetPos, distance, radian);
        }

        //弧度转换成角度
        public static float Radian2Angle(float radian)
        {
            return mImpl.Radian2Angle(radian);
        }
    }

    public interface IStaticVariable
    {
        void Init();
    }

    public class StaticVariableDefaultImpl : IStaticVariable
    {
        private void ReloadTable(IEvent ievent)
        {
            var e = ievent as ReloadTableEvent;
            if (e.tableName == "ServerConfig")
            {
                ResetServerConfigValue();
            }
        }

        private void ResetServerConfigValue()
        {
            StaticVariable.PlayerContribution = Table.GetServerConfig(904).ToInt();
            StaticVariable.GuardContribution = Table.GetServerConfig(905).ToInt();
            StaticVariable.TowerContribution = Table.GetServerConfig(906).ToInt();
            StaticVariable.VictoryTime = Table.GetServerConfig(907).ToInt();
            StaticVariable.ReliveTimeInit = Table.GetServerConfig(908).ToInt();
            StaticVariable.ReliveTimeAdd = Table.GetServerConfig(909).ToInt();
            StaticVariable.ReliveTimeMax = Table.GetServerConfig(910).ToInt();
            StaticVariable.AllianceWarGuardRespawnMaxCount = Table.GetServerConfig(911).ToInt();
            StaticVariable.IsTestPosition = Table.GetServerConfig(595).ToInt() > 0;
        }

        public void Init()
        {
            EventDispatcher.Instance.AddEventListener(ReloadTableEvent.EVENT_TYPE, ReloadTable);

            var tbExdata = Table.GetExdata((int) eExdataDefine.e428);
            StaticVariable.ExpBattleFieldResetEventStr = "Hour" + tbExdata.RefreshTime;
            switch (tbExdata.RefreshRule)
            {
                case 1:
                    StaticVariable.ExpBattleFieldResetEventStr += "Day";
                    break;
                case 2:
                    StaticVariable.ExpBattleFieldResetEventStr += "Week";
                    break;
                case 3:
                    StaticVariable.ExpBattleFieldResetEventStr += "Month";
                    break;
            }

            ResetServerConfigValue();
        }
    }

    public static class StaticVariable
    {
        //攻城战相关
        public static int AllianceWarGuardRespawnMaxCount;
        public static string ExpBattleFieldResetEventStr;
        public static int GuardContribution;
        public static int GuardReliveCountMax;
        private static IStaticVariable mImpl;
        public static int PlayerContribution;
        public static int ReliveTimeAdd;
        public static int ReliveTimeInit;
        public static int ReliveTimeMax;
        public static int TowerContribution;
        public static int VictoryTime;
        public static bool IsTestPosition;

        static StaticVariable()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (StaticVariable),
                typeof (StaticVariableDefaultImpl),
                o => { mImpl = (IStaticVariable) o; });
        }

        public static void Init()
        {
            mImpl.Init();
        }
    }
}