using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using QuoterLogic.Interfaces;

namespace QuoterLogic.Classes
{
    public class ObservableSortedList<T> : IObservableSortedList<T> where T : ILinked<T>
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        private readonly SortedList<T, int> _innerCollection = new SortedList<T, int>();

        public int IndexOf(T item)
        {
            if (!_innerCollection.ContainsValue(item.Id)) return -1;
            return _innerCollection.IndexOfValue(item.Id);
        }

        public int? IndexOf(int id)
        {
            if (!_innerCollection.ContainsValue(id)) return null;
            return _innerCollection.IndexOfValue(id);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _innerCollection.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T this[int id]
        {
            get
            {
                int? index = null;
                return (index = IndexOf(id)) == null ? default(T) : _innerCollection.Keys[index.Value];
            }
        }

        public void Add(T item)
        {
            if (item == null) throw new ArgumentNullException();
            if (_innerCollection.ContainsValue(item.Id)) throw new ArgumentException();
            _innerCollection.Add(item, item.Id);
            item.EntireCollection = this;
            OnCollectionChanged(NotifyCollectionChangedAction.Add, new List<int> {item.Id});
        }

        public void Clear()
        {
            var ids = new List<T>(_innerCollection.Keys);
            _innerCollection.Clear();
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, ids);
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
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, new List<T> {item});
            return result;
        }

        public int Count => _innerCollection.Count;

        public bool IsReadOnly => ((ICollection<T>)_innerCollection).IsReadOnly;

        public void Update(T item)
        {
            // price changing workaround
            _innerCollection.RemoveAt(IndexOf(item)); 
            _innerCollection.Add(item, item.Id);
            //
            OnCollectionChanged(NotifyCollectionChangedAction.Replace, new List<int> {item.Id});
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, IList changesList)
        {
            switch (action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, changesList));
                    break;
                case NotifyCollectionChangedAction.Replace:
                    CollectionChanged?.Invoke(this,
                        new NotifyCollectionChangedEventArgs(action, changesList, changesList));
                    break;
            }
        }

        
    }
}