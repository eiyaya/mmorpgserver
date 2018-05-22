#region using

using System.Collections.Generic;

#endregion

namespace Shared
{
    public static class ObjectPool<T> where T : class, new()
    {
        private static readonly Stack<T> mFreeObjects = new Stack<T>();

        public static void Clear()
        {
            mFreeObjects.Clear();
        }

        public static T NewObject()
        {
            if (mFreeObjects.Count > 0)
            {
                var go = mFreeObjects.Pop();
                return go;
            }
            else
            {
                var go = new T();
                return go;
            }
        }

        public static void Release(T go)
        {
            if (go == null)
            {
                return;
            }

            mFreeObjects.Push(go);
        }
    }
}