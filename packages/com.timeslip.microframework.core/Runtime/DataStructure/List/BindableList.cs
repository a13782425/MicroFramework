using System;
using System.Collections;
using System.Collections.Generic;

namespace MFramework.Core
{
    /// <summary>
    /// 数据绑定List
    /// </summary>
    [Ignore]
    public sealed class BindableList<T> : IList<T>, IBindable
    {
        private BindableHandle _classHandler = new BindableHandle();
        private List<T> _list = new List<T>();
        private bool _childIsBindable = false;
        public T this[int index]
        {
            get => _list[index];
            set
            {
                T oldValue = default;
                if (index < _list.Count)
                {
                    oldValue = _list[index];
                }
                _list[index] = value;
                m_publish(index, oldValue, value, ContainerBindableState.Replace);
                if (_childIsBindable)
                {
                    if (oldValue is IBindable old)
                    {
                        old.SetParent(null);
                    }
                    if (value is IBindable @new)
                    {
                        @new.SetParent(this);
                    }
                }
            }
        }

        public int Count => _list.Count;

        public bool IsReadOnly => false;
        public BindableList()
        {
            _classHandler.Init(this, BindableUtils.BINDABLE_ALL_KEY, null);
            _childIsBindable = typeof(IBindable).IsAssignableFrom(typeof(T));
        }

        /// <summary>
        /// 等价于索引器的Set
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void RawSet(int index, T value)
        {
            T oldValue = default;
            if (index < _list.Count)
            {
                oldValue = _list[index];
            }
            _list[index] = value;
            if (_childIsBindable)
            {
                if (oldValue is IBindable old)
                {
                    old.SetParent(null);
                }
                if (value is IBindable @new)
                {
                    @new.SetParent(this);
                }
            }
        }
        public void Add(T item)
        {
            _list.Add(item);
            m_publish(_list.Count - 1, default, item, ContainerBindableState.Add);
            if (_childIsBindable)
            {
                if (item is IBindable bindable)
                {
                    bindable.SetParent(this);
                }
            }
        }
        public void RawAdd(T item)
        {
            _list.Add(item);
            if (_childIsBindable)
            {
                if (item is IBindable bindable)
                {
                    bindable.SetParent(this);
                }
            }
        }
        public void Clear()
        {
            if (_childIsBindable)
            {
                foreach (var item in _list)
                {
                    if (item is IBindable bindable)
                    {
                        bindable.SetParent(null);
                    }
                }
            }
            _list.Clear();
            _classHandler.Publish(new ContainerBindableEventArgs(this, BindableUtils.BINDABLE_ALL_KEY, ContainerBindableState.Clear), true);
        }
        public void RawClear()
        {
            if (_childIsBindable)
            {
                foreach (var item in _list)
                {
                    if (item is IBindable bindable)
                    {
                        bindable.SetParent(null);
                    }
                }
            }
            _list.Clear();
        }
        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }
        public bool Exists(Predicate<T> match)
        {
            return _list.Exists(match);
        }
        public T Find(Predicate<T> match)
        {
            return _list.Find(match);
        }
        public int FindIndex(Predicate<T> match)
        {
            return _list.FindIndex(match);
        }
        public List<T> FindAll(Predicate<T> match)
        {
            return _list.FindAll(match);
        }
        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
            m_publish(index, default, item, ContainerBindableState.Add);
            if (_childIsBindable)
            {
                if (item is IBindable bindable)
                {
                    bindable.SetParent(this);
                }
            }
        }
        public void RawInsert(int index, T item)
        {
            _list.Insert(index, item);
            if (_childIsBindable)
            {
                if (item is IBindable bindable)
                {
                    bindable.SetParent(this);
                }
            }
        }
        public bool Remove(T item)
        {
            int num = IndexOf(item);
            if (num >= 0)
            {
                RemoveAt(num);
                return true;
            }
            return false;
        }
        public bool RawRemove(T item)
        {
            int num = IndexOf(item);
            if (num >= 0)
            {
                RawRemoveAt(num);
                return true;
            }
            return false;
        }
        public void RemoveAt(int index)
        {
            T value = _list[index];
            _list.RemoveAt(index);
            m_publish(index, value, default, ContainerBindableState.Remove);
            if (_childIsBindable)
            {
                if (value is IBindable bindable)
                {
                    bindable.SetParent(null);
                }
            }
        }
        public void RawRemoveAt(int index)
        {
            T value = _list[index];
            _list.RemoveAt(index);
            if (_childIsBindable)
            {
                if (value is IBindable bindable)
                {
                    bindable.SetParent(null);
                }
            }
        }
        public void RemoveRange(int index, int count)
        {
            int _size = _list.Count;
            if (count > 0)
            {
                int size = _size;
                _size -= count;
                if (index < _size)
                {
                    T[] arr = new T[count];
                    _list.CopyTo(index, arr, 0, count);
                    _list.RemoveRange(index, count);
                    if (_childIsBindable)
                    {
                        foreach (var item in arr)
                        {
                            if (item is IBindable bindable)
                            {
                                bindable.SetParent(null);
                            }
                        }
                    }
                    m_publish(-1, default, default, ContainerBindableState.RemoveRange);
                    return;
                }
                Clear();
            }
        }
        public void RawRemoveRange(int index, int count)
        {
            int _size = _list.Count;
            if (count > 0)
            {
                int size = _size;
                _size -= count;
                if (index < _size)
                {
                    T[] arr = new T[count];
                    _list.CopyTo(index, arr, 0, count);
                    _list.RemoveRange(index, count);
                    if (_childIsBindable)
                    {
                        foreach (var item in arr)
                        {
                            if (item is IBindable bindable)
                            {
                                bindable.SetParent(null);
                            }
                        }
                    }
                    return;
                }
                RawClear();
            }
        }
        public void AddRange(IEnumerable<T> collection)
        {
            _list.AddRange(collection);
            _classHandler.Publish(new ContainerBindableEventArgs(this, BindableUtils.BINDABLE_ALL_KEY, ContainerBindableState.AddRange), true);
            if (_childIsBindable)
            {
                foreach (var item in collection)
                {
                    if (item is IBindable bindable)
                    {
                        bindable.SetParent(this);
                    }
                }
            }
        }
        public void RawAddRange(IEnumerable<T> collection)
        {
            _list.AddRange(collection);
            if (_childIsBindable)
            {
                foreach (var item in collection)
                {
                    if (item is IBindable bindable)
                    {
                        bindable.SetParent(this);
                    }
                }
            }
        }
        public void Sort()
        {
            _list.Sort();
        }
        public void Sort(IComparer<T> comparer)
        {
            _list.Sort(comparer);
        }

        public void Sort(int index, int count, IComparer<T> comparer)
        {
            _list.Sort(index, count, comparer);
        }

        public void Sort(Comparison<T> comparison)
        {
            _list.Sort(comparison);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Publish(int key)
        {
            T value = default;
            if (key < _list.Count)
            {
                value = _list[key];
            }
            m_publish(key, value, value, ContainerBindableState.Publish);
        }

        private void m_publish(int key, T oldValue, T newValue, ContainerBindableState state)
        {
            ContainerBindableEventArgs<T> args = new ContainerBindableEventArgs<T>(this, key, oldValue, newValue, state);
            _classHandler.Publish(args, true);
        }

        #region IBindable实现
        void IBindable.SetParent(IBindable parent)
        {
            if (parent == null)
            {
                _classHandler.SetParent(null);
                return;
            }
            _classHandler.SetParent(parent.GetHandle());
        }
        IBindableObserver IBindable.Subscribe(BindableDelegate callback, bool firstNotify, bool notifyAnyway)
        {
            return _classHandler.Subscribe(callback, firstNotify, notifyAnyway);
        }
        void IBindable.Unsubscribe(BindableDelegate callback)
        {
            _classHandler.Unsubscribe(callback);
        }
        void IBindable.UnsubscribeAll()
        {
            _classHandler.Clear();
        }
        void IBindable.Publish()
        {
            _classHandler.Publish();
        }
        void IBindable.PublishAll()
        {
            _classHandler.Publish();
        }
        void IBindable.SetNotify(bool notify, bool isPublish)
        {
            _classHandler.SetNotify(notify, isPublish);
            if (_childIsBindable)
            {
                foreach (var item in _list)
                {
                    if (item is IBindable bindable)
                    {
                        bindable.SetNotify(notify, isPublish);
                    }
                }
            }
        }
        BindableHandle IBindable.GetHandle(object memberName = null)
        {
            return _classHandler;
        }
        List<BindableHandle> IBindable.GetHandles()
        {
            return new List<BindableHandle> { _classHandler };
        }
        #endregion
    }
}
