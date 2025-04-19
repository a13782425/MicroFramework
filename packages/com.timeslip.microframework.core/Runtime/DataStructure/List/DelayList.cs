using System.Collections;
using System.Collections.Generic;

namespace MFramework.Core
{

    /// <summary>
    /// 延时集合
    /// 增加,删除,清空时候不会真正删除数据，需要执行Push方法才会更改原始数据
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class DelayList<T> : IEnumerable<T>
    {
        private List<T> _list = new List<T>();
        private HashSet<T> _waitRemoveSet = new HashSet<T>();
        private HashSet<T> _waitAddSet = new HashSet<T>();

        /// <summary>
        /// 是否有脏数据
        /// </summary>
        private bool _isDirty = false;
        /// <summary>
        /// 是否清空
        /// </summary>
        private bool _isClear = false;
        /// <summary>
        /// 获取一个数据（在缓存中的数据无法通过该方法获取）
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public T this[int index]
        {
            get
            {
                return _list[index];
            }
            set
            {
                _list[index] = value;
            }
        }
        public int Count => _list.Count + _waitAddSet.Count - _waitRemoveSet.Count;
        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            if (_waitRemoveSet.Contains(item)) return false;
            return _list.Contains(item) || _waitAddSet.Contains(item);
        }

        /// <summary>
        /// 删除一个数据
        /// </summary>
        public void Remove(T item)
        {
            if (!_isDirty)
                Push();
            _isDirty = true;
            if (_waitAddSet.Contains(item))
                _waitAddSet.Remove(item);
            else if (_list.Contains(item))
                _waitRemoveSet.Add(item);
        }

        /// <summary>
        /// 增加一个数据(如果增加Key相同的则更新数据)
        /// </summary>
        public void Add(T item)
        {
            if (!_isDirty)
                Push();
            _isDirty = true;
            if (_waitRemoveSet.Contains(item))
                _waitRemoveSet.Remove(item);
            _waitAddSet.Add(item);
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            if (!_isDirty)
                Push();
            _isDirty = true;
            _isClear = true;
            _waitAddSet.Clear();
            _waitRemoveSet.Clear();
        }
        /// <summary>
        /// 将缓存数据推到正式数据
        /// </summary>
        /// <returns>存在变化</returns>
        public void Push()
        {
            if (!_isDirty) return;
            _isDirty = false;
            if (_isClear)
                _list.Clear();
            if (_waitRemoveSet.Count > 0)
            {
                foreach (var item in _waitRemoveSet)
                {
                    _list.Remove(item);
                }
                _waitRemoveSet.Clear();
            }
            if (_waitAddSet.Count > 0)
            {
                foreach (var item in _waitAddSet)
                {
                    _list.Add(item);
                }
                _waitAddSet.Clear();
            }

        }
        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
    }
}
