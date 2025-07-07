using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

namespace MFramework.Core
{
    public class MicroPool<T> : IEnumerable<T>, IDisposable where T : class
    {
        /// <summary>
        /// 对象池现在的数量
        /// </summary>
        public int Count { get { return _elements.Count; } }
        private int _maxCount = 1000;
        /// <summary>
        /// 对象池最大上限
        /// </summary>
        public int MaxCount { get => _maxCount; set => _maxCount = value; }
        private readonly List<T> _elements = new List<T>();
        private readonly Func<T> _onCreate;
        private readonly Action<T> _onRecover;
        private readonly Action<T> _onDestroy;
        private readonly bool _collectionCheck;
        public MicroPool(Func<T> onCreate, Action<T> onRecover, Action<T> onDestroy, bool collectionCheck)
        {
            this._onCreate = onCreate;
            this._onRecover = onRecover;
            this._onDestroy = onDestroy;
            this._collectionCheck = collectionCheck;
            _elements = new List<T>(8);
        }
        public MicroPool(Func<T> onCreate, Action<T> onRecover, Action<T> onDestroy) : this(onCreate, onRecover, onDestroy, true) { }
        public MicroPool(Func<T> onCreate, Action<T> onRecover, bool collectionCheck) : this(onCreate, onRecover, null, collectionCheck) { }
        public MicroPool(Func<T> onCreate, Action<T> onRecover) : this(onCreate, onRecover, null, true) { }
        public MicroPool(Func<T> onCreate, bool collectionCheck) : this(onCreate, null, null, collectionCheck) { }
        public MicroPool(Func<T> onCreate) : this(onCreate, null, null, true) { }
        public MicroPool(bool collectionCheck) : this(null, null, null, collectionCheck) { }
        public MicroPool() : this(null, null, null, true) { }

        public T Get()
        {
            if (_elements.Count > 0)
            {
                int index = _elements.Count - 1;
                var obj = _elements[index];
                _elements.RemoveAt(index);
                return obj;
            }
            else
            {
                var element = _onCreate?.Invoke();
                return element;
            }
        }
        public void Recover(T element)
        {
            if (_collectionCheck && _elements.Count > 0)
            {
                if (_elements.Contains(element))
                {
                    throw new InvalidOperationException("Trying to release an object that has already been released to the pool.");
                }
            }

            _onRecover?.Invoke(element);
            if (_elements.Count < _maxCount)
            {
                _elements.Add(element);
                return;
            }
            _onDestroy?.Invoke(element);
        }
        /// <summary>
        /// 是否为空
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty() => _elements.Count == 0;
        /// <summary>
        /// 清空所有对象
        /// </summary>
        public void Clear()
        {
            if (_onDestroy != null)
            {
                for (int i = _elements.Count; i >= 0; i--)
                {
                    _onDestroy(_elements[i]);
                }
            }
            _elements.Clear();
        }
        public IEnumerator<T> GetEnumerator()
        {
            return _elements.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
