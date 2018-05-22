#region using

using System;
using System.Collections.Concurrent;

#endregion

namespace Shared
{
    public sealed class AsyncReturnValue<T> : IDisposable
    {
        private static readonly ConcurrentStack<AsyncReturnValue<T>> sStack = new ConcurrentStack<AsyncReturnValue<T>>();
        public T Value { get; set; }

        public static AsyncReturnValue<T> Create()
        {
            AsyncReturnValue<T> value;
            if (!sStack.TryPop(out value))
            {
                value = new AsyncReturnValue<T>();
            }
            value.Value = default(T);
            return value;
        }

        public static AsyncReturnValue<T> Create(T t)
        {
            AsyncReturnValue<T> value;
            if (!sStack.TryPop(out value))
            {
                value = new AsyncReturnValue<T>();
            }
            value.Value = t;
            return value;
        }

        public void Dispose()
        {
            sStack.Push(this);
        }
    }

    public class AsyncReturnValue<T1, T2>
    {
        private static readonly ConcurrentStack<AsyncReturnValue<T1, T2>> sStack =
            new ConcurrentStack<AsyncReturnValue<T1, T2>>();

        public T1 Value1 { get; set; }
        public T2 Value2 { get; set; }

        public static AsyncReturnValue<T1, T2> Create()
        {
            AsyncReturnValue<T1, T2> value;
            if (!sStack.TryPop(out value))
            {
                value = new AsyncReturnValue<T1, T2>();
            }
            value.Value1 = default(T1);
            value.Value2 = default(T2);
            return value;

            //if (sStack.IsEmpty)
            //{
            //    var value = new AsyncReturnValue<T1, T2>();
            //    return value;
            //}
            //else
            //{
            //    AsyncReturnValue<T1, T2> value;
            //    sStack.TryPop(out value);
            //    value.Value1 = default(T1);
            //    value.Value2 = default(T2);
            //    return value;
            //}
        }

        public static AsyncReturnValue<T1, T2> Create(T1 t)
        {
            AsyncReturnValue<T1, T2> value;
            if (!sStack.TryPop(out value))
            {
                value = new AsyncReturnValue<T1, T2>();
            }
            value.Value1 = t;
            value.Value2 = default(T2);
            return value;

            //if (sStack.IsEmpty)
            //{
            //    var value = new AsyncReturnValue<T1, T2>();
            //    value.Value1 = t;
            //    return value;
            //}
            //else
            //{
            //    AsyncReturnValue<T1, T2> value;
            //    sStack.TryPop(out value);
            //    value.Value1 = t;
            //    value.Value2 = default(T2);
            //    return value;
            //}
        }

        public static AsyncReturnValue<T1, T2> Create(T1 t1, T2 t2)
        {
            AsyncReturnValue<T1, T2> value;
            if (!sStack.TryPop(out value))
            {
                value = new AsyncReturnValue<T1, T2>();
            }
            value.Value1 = default(T1);
            value.Value2 = default(T2);
            return value;
            //if (sStack.IsEmpty)
            //{
            //    var value = new AsyncReturnValue<T1, T2>();
            //    value.Value1 = t1;
            //    value.Value2 = t2;
            //    return value;
            //}
            //else
            //{
            //    AsyncReturnValue<T1, T2> value;
            //    sStack.TryPop(out value);
            //    value.Value1 = t1;
            //    value.Value2 = t2;
            //    return value;
            //}
        }

        public void Dispose()
        {
            sStack.Push(this);
        }
    }

    public class AsyncReturnValue<T1, T2, T3>
    {
        public T1 Value1 { get; set; }
        public T2 Value2 { get; set; }
        public T3 Value3 { get; set; }

        private static ConcurrentStack<AsyncReturnValue<T1, T2, T3>> sStack = new ConcurrentStack<AsyncReturnValue<T1, T2, T3>>();
        public static AsyncReturnValue<T1, T2, T3> Create()
        {
            AsyncReturnValue<T1, T2, T3> value;
            if (!sStack.TryPop(out value))
            {
                value = new AsyncReturnValue<T1, T2, T3>();
            }
            value.Value1 = default(T1);
            value.Value2 = default(T2);
            value.Value3 = default(T3);
            return value;
        }

        public static AsyncReturnValue<T1, T2, T3> Create(T1 t)
        {
            if (sStack.IsEmpty)
            {
                var value = new AsyncReturnValue<T1, T2, T3>();
                value.Value1 = t;
                return value;
            }
            else
            {
                AsyncReturnValue<T1, T2, T3> value;
                sStack.TryPop(out value);
                return value;
            }
        }

        public static AsyncReturnValue<T1, T2, T3> Create(T1 t1, T2 t2)
        {
            if (sStack.IsEmpty)
            {
                var value = new AsyncReturnValue<T1, T2, T3>();
                value.Value1 = t1;
                value.Value2 = t2;
                return value;
            }
            else
            {
                AsyncReturnValue<T1, T2, T3> value;
                sStack.TryPop(out value);
                return value;
            }
        }

        public static AsyncReturnValue<T1, T2, T3> Create(T1 t1, T2 t2, T3 t3)
        {
            if (sStack.IsEmpty)
            {
                var value = new AsyncReturnValue<T1, T2, T3>();
                value.Value1 = t1;
                value.Value2 = t2;
                value.Value3 = t3;
                return value;
            }
            else
            {
                AsyncReturnValue<T1, T2, T3> value;
                sStack.TryPop(out value);
                return value;
            }
        }

        public void Dispose()
        {
            sStack.Push(this);
        }
    }
}