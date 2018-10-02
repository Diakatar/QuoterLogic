using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using QuoterLogic.Interfaces;

namespace QuoterLogic.Classes
{
    public class ObservableSortedList<T> : IObservableSortedList<T> where T : IObservableCollectionItem<T>
    {
        #region Events
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        #endregion

        #region Private declarations
        private readonly SortedList<T, int> _innerCollection = new SortedList<T, int>();
        #endregion

        #region IObservableSortedList implementation
        public int IndexOf(T item)
        {
            if (!_innerCollection.ContainsValue(item.Id)) return -1;
            return _innerCollection.IndexOfValue(item.Id);
        }

        public void Update(T item)
        {
            // price changing workaround
            _innerCollection.RemoveAt(IndexOf(item));
            _innerCollection.Add(item, item.Id);
            //
            OnCollectionChanged(NotifyCollectionChangedAction.Replace, new List<T> { item });
        }

        public void Update()
        {
            foreach (var order in _innerCollection.Keys)
                order.Update();
        }
        #endregion

        #region ICollection implementation
        public IEnumerator<T> GetEnumerator()
        {
            return _innerCollection.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            if (item == null) throw new ArgumentNullException();
            if (_innerCollection.ContainsValue(item.Id)) throw new ArgumentException();
            _innerCollection.Add(item, item.Id);
            item.EntireCollection = this;
            OnCollectionChanged(NotifyCollectionChangedAction.Add, new List<T> { item });
        }

        public void Clear()
        {
            var list = new List<T>(_innerCollection.Keys);
            _innerCollection.Clear();
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, list);
        }

        public bool Contains(T item)
        {
            return item != null && _innerCollection.ContainsValue(item.Id);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _innerCollection.Keys.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            if (item == null) return false;
            bool result = _innerCollection.Remove(item);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, new List<T> { item });
            return result;
        }

        public int Count => _innerCollection.Count;

        public bool IsReadOnly => ((ICollection<T>)_innerCollection).IsReadOnly;

        #endregion

        #region INotifyCollectionChanged implementation
        private void OnCollectionChanged(NotifyCollectionChangedAction action, IList changesList)
        {
            switch (action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, changesList));
                    break;
                case NotifyCollectionChangedAction.Replace:
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, changesList, changesList));
                    break;
            }
        }
        #endregion

        #region Public methods
        public int? IndexOf(int id)
        {
            if (!_innerCollection.ContainsValue(id)) return null;
            return _innerCollection.IndexOfValue(id);
        }


        public T this[int id]
        {
            get
            {
                int? index = null;
                return (index = IndexOf(id)) == null ? default(T) : _innerCollection.Keys[index.Value];
            }
        }
        #endregion

    }
}