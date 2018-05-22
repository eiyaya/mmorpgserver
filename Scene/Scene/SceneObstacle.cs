#region using

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Mono.GameMath;
using NLog;

#endregion

namespace Scene
{
    public unsafe class NavmeshFinder : IDisposable
    {
        private static readonly List<Vector3> sEmptyPath = new List<Vector3>();

        public NavmeshFinder(string file)
        {
            mFileName = Marshal.StringToHGlobalAnsi(file).ToPointer();
            mDetour = CreateNavMesh(mFileName, 1);
        }

        private void* mDetour;
        private void* mFileName;

        [DllImport("DetourDll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint AddObstacle(void* p, void* s, float radius, float height);

        public uint AddObstacle(float[] s, float radius, float height)
        {
            if (mDetour == null)
            {
                return 0;
            }

            fixed (void* ps = &s[0])
            {
                return AddObstacle(mDetour, ps, radius, height);
            }
        }

        [DllImport("DetourDll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void* CreateNavMesh(void* file, int type);

        [DllImport("DetourDll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int FindPath(void* p, void* s, void* e, void* way, int size);

        public List<Vector3> FindPath(float[] s, float[] e)
        {
            if (mDetour == null)
            {
                return sEmptyPath;
            }

            if (s.Length != 3)
            {
                return sEmptyPath;
            }

            if (e.Length != 3)
            {
                return sEmptyPath;
            }

            var way = new float[128*3];

            fixed (void* ps = &s[0])
            {
                fixed (void* pe = &e[0])
                {
                    fixed (void* pway = &way[0])
                    {
                        var wp = FindPath(mDetour, ps, pe, pway, 128);
                        if (wp > 0)
                        {
                            var l = new List<Vector3>();
                            var p = wp*3;
                            for (var i = 0; i < p;)
                            {
                                var v = new Vector3();
                                v.X = way[i++];
                                v.Y = way[i++];
                                v.Z = way[i++];

                                l.Add(v);
                            }

                            return l;
                        }
                        return sEmptyPath;
                    }
                }
            }
        }

        [DllImport("DetourDll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Raycast(void* p, void* s, void* e, void* hitPoint);

        public bool Raycast(float[] s, float[] e, out Vector2 hit)
        {
            if (mDetour == null)
            {
                hit = new Vector2(0, 0);
                return false;
            }

            var hitPoint = new float[3];

            fixed (void* ps = &s[0])
            {
                fixed (void* pe = &e[0])
                {
                    fixed (void* phitPoint = &hitPoint[0])
                    {
                        var b = Raycast(mDetour, ps, pe, phitPoint) != 0;
                        hit = new Vector2(hitPoint[0], hitPoint[2]);
                        return b;
                    }
                }
            }
        }

        [DllImport("DetourDll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void ReleaseNavMesh(void* p);

        [DllImport("DetourDll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void RemoveObstacle(void* p, uint ob);

        public void RemoveObstacle(uint ob)
        {
            if (mDetour == null)
            {
                return;
            }

            RemoveObstacle(mDetour, ob);
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal((IntPtr) mFileName);
            ReleaseNavMesh(mDetour);
            mDetour = null;
            mFileName = null;
        }
    }

    public class SceneObstacle
    {
        public static readonly List<Vector2> EmptyPath = new List<Vector2>();

        private static readonly Dictionary<string, SceneMatrixCacheData> mSceneMatrixCache =
            new Dictionary<string, SceneMatrixCacheData>();

        private static readonly int[] sPowerOfTwo = {2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096};

        public SceneObstacle(string filename)
        {
            try
            {
                if (!Load(filename))
                {
                    throw new NullReferenceException(filename);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "load server obstacle error, {0}", filename);
            }
        }

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private byte[,] mMatrix;
        private NavmeshFinder mNavmeshPathFinder;

        public enum ObstacleValue
        {
            Obstacle = 0, //不可行走
            Runable = 1, //可跑
            Walkable = 2 //可行走
        }

        public int Height { get; set; }
        public int Width { get; set; }

        /// <summary>
        ///     DO NOT CALL THIS FUNCTION
        /// </summary>
        /// <param name="p"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public uint AddCollider(Vector2 p, float radius)
        {
            return mNavmeshPathFinder.AddObstacle(new[] {p.X, 0.0f, p.Y}, radius, 100);
        }

        private static ObstacleItem Byte2Struct(byte[] arr)
        {
            var structSize = Marshal.SizeOf(typeof (ObstacleItem));
            var ptemp = Marshal.AllocHGlobal(structSize);
            Marshal.Copy(arr, 0, ptemp, structSize);
            var rs = (ObstacleItem) Marshal.PtrToStructure(ptemp, typeof (ObstacleItem));
            Marshal.FreeHGlobal(ptemp);
            return rs;
        }

        /// <summary>
        ///     DO NOT CALL THIS FUNCTION
        /// </summary>
        public void Close()
        {
            mNavmeshPathFinder.Dispose();
        }

        /// <summary>
        ///     DO NOT CALL THIS FUNCTION
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<Vector2> FindPath(Vector2 start, Vector2 end)
        {
            return
                mNavmeshPathFinder.FindPath(new[] {start.X, 0, start.Y}, new[] {end.X, 0, end.Y}).Skip(1)
                    .Select(i => new Vector2(i.X, i.Z))
                    .ToList();
        }

        private int GetNearestPowerOfTwo(int x)
        {
            for (var i = 0; i < sPowerOfTwo.Length; i++)
            {
                if (x <= sPowerOfTwo[i])
                {
                    return sPowerOfTwo[i];
                }
            }

            throw new Exception("too big.");
        }

        public ObstacleValue GetObstacleValue(float fx, float fy)
        {
            if (fx < 0 || fx >= Width || fy < 0 || fy >= Height)
            {
                return ObstacleValue.Obstacle;
            }
            return (ObstacleValue) mMatrix[(int) (fx*2), (int) (fy*2)];
        }

        private bool Load(string fileName)
        {
            if (!File.Exists(fileName))
            {
                Logger.Error("path file do not exit. {0}", fileName);
                return false;
            }

            // 在Scene之间共享Matrix，节省内存
            SceneMatrixCacheData cache;
            if (!mSceneMatrixCache.TryGetValue(fileName, out cache))
            {
                using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    var nSeek = 0;

                    //先从文件头读取地图的长和宽
                    var byteLen = new byte[Marshal.SizeOf(typeof (int))];
                    fileStream.Seek(nSeek, SeekOrigin.Begin);
                    fileStream.Read(byteLen, 0, byteLen.Length);
                    Height = BitConverter.ToInt32(byteLen, 0);
                    nSeek += byteLen.Length;

                    var byteWid = new byte[Marshal.SizeOf(typeof (int))];
                    fileStream.Seek(nSeek, SeekOrigin.Begin);
                    fileStream.Read(byteWid, 0, byteWid.Length);
                    Width = BitConverter.ToInt32(byteWid, 0);
                    nSeek += byteWid.Length;

                    var nReadLen = Marshal.SizeOf(typeof (ObstacleItem));
                    var read = new byte[nReadLen];

                    var w = GetNearestPowerOfTwo(Width*2);
                    var h = GetNearestPowerOfTwo(Height*2);
                    var x = Math.Max(w, h);

                    mMatrix = new byte[x, x];
                    for (var i = 0; i <= (Width*2); ++i)
                    {
                        for (var j = 0; j <= (Height*2); j++)
                        {
                            fileStream.Seek(nSeek, SeekOrigin.Begin);
                            fileStream.Read(read, 0, nReadLen);
                            nSeek += nReadLen;
                            var info = Byte2Struct(read);

                            mMatrix[(int) (info.Fx*2), (int) (info.Fy*2)] = info.Value;
                        }
                    }
                }
                cache = new SceneMatrixCacheData
                {
                    Height = Height,
                    Width = Width,
                    Matrix = mMatrix
                };
                mSceneMatrixCache[fileName] = cache;
            }

            Height = cache.Height;
            Width = cache.Width;
            mMatrix = cache.Matrix;

            mNavmeshPathFinder = new NavmeshFinder(fileName + ".nav");

            return true;
        }

        /// <summary>
        ///     DO NOT CALL THIS FUNCTION
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// <param name="hit"></param>
        /// <returns></returns>
        public bool Raycast(Vector2 s, Vector2 e, out Vector2 hit)
        {
            return mNavmeshPathFinder.Raycast(new[] {s.X, 0, s.Y}, new[] {e.X, 0, e.Y}, out hit);
        }

        /// <summary>
        ///     DO NOT CALL THIS FUNCTION
        /// </summary>
        /// <param name="ob"></param>
        public void RemoveCollider(uint ob)
        {
            mNavmeshPathFinder.RemoveObstacle(ob);
        }

        public static Vector2 ToObstacleCoordinate(float x, float y)
        {
            return new Vector2(x*2, y*2);
        }

        public static Vector2 ToSceneCoordinate(float x, float y)
        {
            return new Vector2(x/2.0f, y/2.0f);
        }

        public class SceneMatrixCacheData
        {
            public int Height;
            public byte[,] Matrix;
            public int Width;
        }

        public struct ObstacleItem
        {
            public float Fx { get; set; }
            public float Fy { get; set; }
            public byte Value { get; set; }
        }
    }
}