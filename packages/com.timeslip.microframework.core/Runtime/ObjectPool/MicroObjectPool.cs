using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFramework.Core
{
    internal static class MicroObjectPool<T> where T : class, new()
    {
        public static event Func<T> onCreate;
        public static event Action<T> onRecover;

        private static Pool<T> _pool = new Pool<T>(() => onCreate?.Invoke() ?? new T(), (t) => onRecover?.Invoke(t));
        public static T Get() => _pool.Get();

        public static void Clear() => _pool.Clear();


        public static bool IsEmpty() => _pool.IsEmpty();

        public static void Recover(T t) => _pool.Recover(t);
    }
}
