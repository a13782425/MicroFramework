using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SocialPlatforms;

namespace MFramework.Core
{
    /// <summary>
    /// Object对象池
    /// <para>不会创建空的游戏物体，只存在于内存</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPool<T> where T : class
    {
        private Pool<T> _pool = default;
        public event Func<T> onCreate;
        public event Action<T> onRecover;

        public ObjectPool()
        {
            _pool = new Pool<T>(() =>
            {
                if (onCreate != null)
                    return onCreate();
                return default(T);
            }, (item) => { onRecover?.Invoke(item); });
        }

        public void Clear()
        {
            _pool.Clear();
        }

        public T Get()
        {
            return _pool.Get();
        }

        public bool IsEmpty() => _pool.IsEmpty();

        public void Recover(T t)
        {
            _pool.Recover(t);
        }
    }
}
