using System;
using System.Collections;
using System.Collections.Generic;

namespace MFramework.Core
{
    public class Pool<T> : IEnumerable<T> where T : class
    {
        public int Count { get { return objects.Count; } }
        readonly Queue<T> objects = new Queue<T>();
        readonly Func<T> onCreate;
        readonly Action<T> onRecover;

        public Pool(Func<T> onCreate, Action<T> onRecover)
        {
            this.onCreate = onCreate;
            this.onRecover = onRecover;
        }
        public Pool(Func<T> onCreate) : this(onCreate, null) { }
        public Pool() : this(null, null) { }

        public T Get()
        {
            if (objects.Count > 0)
            {
                var obj = objects.Dequeue();
                return obj;
            }
            else
            {
                var obj = onCreate();
                return obj;
            }
        }
        public void Recover(T obj)
        {
            if (!objects.Contains(obj))
            {
                onRecover?.Invoke(obj);
                objects.Enqueue(obj);
            }
        }
        /// <summary>
        /// 是否为空
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty() => Count == 0;
        /// <summary>
        /// 清空所有对象
        /// </summary>
        public void Clear()
        {
            objects.Clear();
        }
        public IEnumerator<T> GetEnumerator()
        {
            return objects.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
