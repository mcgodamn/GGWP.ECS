using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace GGWP.Memory
{
    public class ObjectPool<T> where T : class
    {
        private const int DEFAULT_CAPACITY = 8;
        private const BindingFlags CTOR_FLAGS = BindingFlags.Public | BindingFlags.Instance;

        private readonly Func<T> constructor;
        private T[] buffer;
        private int count;

        public int Capacity => buffer.Length;

        public ObjectPool(Func<T> ctor = null, int capacity = DEFAULT_CAPACITY, bool isPrefilled = true)
        {
            if (ctor == null)
            {
                var ctorInfo = typeof(T).GetConstructor(CTOR_FLAGS, null, Type.EmptyTypes, null);
                var newExpr = Expression.New(ctorInfo, (IEnumerable<Expression>)null);
                constructor = Expression.Lambda<Func<T>>(newExpr).Compile();
            }
            else
            {
                constructor = ctor;
            }

            if (capacity <= 0)
            {
                capacity = DEFAULT_CAPACITY;
            }
            buffer = new T[capacity];

            if (isPrefilled)
            {
                while (count < capacity)
                {
                    buffer[count++] = constructor();
                }
            }
        }

        public void Fill()
        {
            while (count < buffer.Length)
            {
                buffer[count++] = constructor();
            }
        }

        public void Clear()
        {
            Array.Clear(buffer, 0, count);
            count = 0;
        }

        public T Pop()
        {
            return count > 0 ? buffer[--count] : null;
        }

        public T ForcePop()
        {
            return count > 0 ? buffer[--count] : constructor();
        }

        public void Push(T obj)
        {
            if (count >= buffer.Length)
            {
                return;
            }
            buffer[count++] = obj;
        }

        public void ForcePush(T obj)
        {
            if (count >= buffer.Length)
            {
                var newBuf = new T[count * 2];
                Array.Copy(buffer, newBuf, count);
                buffer = newBuf;
            }
            buffer[count++] = obj;
        }
    }
}