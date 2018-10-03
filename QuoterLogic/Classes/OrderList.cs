using QuoterLogic.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace QuoterLogic.Classes
{
    public class OrderCollection: ICollection<Order>
    {

        #region Events
        public event EventHandler<PreparedEventArgs> Prepared;

        protected void OnPrepared(Order order, PlacerState modificationType)
        {
            Prepared?.Invoke(this, new PreparedEventArgs(order, modificationType));
        }
        #endregion

        #region Private declarations
        private readonly SortedList<Order, int> _innerCollection = new SortedList<Order, int>();
        private readonly int _poolSize;
        #endregion

        #region Ctor
        public OrderCollection(int poolSize)
        { _poolSize = poolSize; }
        #endregion

        #region ICollection implementation
        public IEnumerator<Order> GetEnumerator()
        {
            return _innerCollection.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Order item)
        {
            if (item == null) throw new ArgumentNullException();
            if (_innerCollection.ContainsValue(item.Id)) throw new ArgumentException();
            _innerCollection.Add(item, item.Id);
            Prepare(item, PlacerState.PendingPlacing);
        }
        [Obsolete]
        public void Clear()
        {
            _innerCollection.Clear();
        }

        public bool Contains(Order item)
        {
            return item != null && _innerCollection.ContainsValue(item.Id);
        }

        public void CopyTo(Order[] array, int arrayIndex)
        {
            _innerCollection.Keys.CopyTo(array, arrayIndex);
        }

        public bool Remove(Order item)
        {
            if (item == null) return false;
            bool result = _innerCollection.Remove(item);
            Prepare(item, PlacerState.PendingCancelation);
            return result;
        }

       

        public int Count => _innerCollection.Count;

        public bool IsReadOnly => ((ICollection<Order>)_innerCollection).IsReadOnly;

        #endregion

        #region Public methods
        public void ChangePrice(Order item, decimal price)
        {
            if (item.Price != price)
            {
                item.Price = price;
                _innerCollection.RemoveAt(IndexOf(item));
                _innerCollection.Add(item, item.Id);
                Prepare(item, PlacerState.PendingMovement);
            }
            else
                Prepare(item, PlacerState.Unmodified);

        }

        public int IndexOf(Order item)
        {
            if (!_innerCollection.ContainsValue(item.Id)) return -1;
            return _innerCollection.IndexOfValue(item.Id);
        }

        public int? IndexOf(int id)
        {
            if (!_innerCollection.ContainsValue(id)) return null;
            return _innerCollection.IndexOfValue(id);
        }


        public Order this[int id]
        {
            get
            {
                int? index = null;
                return (index = IndexOf(id)) == null ? null : _innerCollection.Keys[index.Value];
            }
        }

        public IEnumerable<Order> Modified
        {
            get
            {
                return this.Where(i => i.PlacerState != PlacerState.Unmodified).OrderBy(p=>p.PlacerState);
            }
        }

        public void Flush()
        {
            foreach (var o in Modified)
                o.PlacerState = PlacerState.Unmodified;
        }
        #endregion

        #region Private methods
        private void Prepare(Order order, PlacerState modificationType)
        {
            foreach (var o in this)
            {
                int newIndex = IndexOf(o);
                bool activeChanged = (modificationType == PlacerState.PendingMovement && o.Id == order.Id);
                if (o.Index == newIndex)
                {
                    o.PlacerState = activeChanged ? PlacerState.PendingMovement : PlacerState.Unmodified;
                }
                else if (o.Index < _poolSize)
                {
                    if (newIndex < 0)
                        o.PlacerState = PlacerState.PendingCancelation;
                    else if (newIndex >= _poolSize)
                        o.PlacerState = PlacerState.PendingCancelation;
                    else
                        o.PlacerState = activeChanged ? PlacerState.PendingMovement : PlacerState.Unmodified;
                }
                else
                    o.PlacerState = newIndex < _poolSize ? PlacerState.PendingPlacing : PlacerState.Unmodified;
                o.Index = newIndex;
            }
            if (modificationType == PlacerState.PendingCancelation && order.Index < _poolSize)
                order.PlacerState = PlacerState.PendingCancelation;
            
            OnPrepared(order, modificationType);
        }
        #endregion
    }
}