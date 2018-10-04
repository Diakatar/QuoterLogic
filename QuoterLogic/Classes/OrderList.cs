using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QuoterLogic.Helpers;

namespace QuoterLogic.Classes
{
    public class OrderCollection: IEnumerable<Order>
    {
        #region Events and callers
        public event EventHandler<PreparedEventArgs> Prepared;

        protected void OnPrepared(Order order, ProcessState modificationType)
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

        #region IEnumerable implementation
        public IEnumerator<Order> GetEnumerator()
        {
            return _innerCollection.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region Public properties
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
                return this.Where(i => i.PlacerState != ProcessState.Unmodified).OrderBy(p => p.PlacerState);
            }
        }
        #endregion

        #region Public methods
        public void Add(Order item)
        {
            if (item == null) throw new ArgumentNullException();
            if (_innerCollection.ContainsValue(item.Id)) throw new ArgumentException();
            _innerCollection.Add(item, item.Id);
            Prepare(item, ProcessState.Placing);
        }

        public bool Remove(Order item)
        {
            if (item == null) return false;
            bool result = _innerCollection.Remove(item);
            Prepare(item, ProcessState.Cancelation);
            return result;
        }

        public void Fill(Order item)
        {
            if (item == null) throw new ArgumentNullException();
            _innerCollection.RemoveAt(IndexOf(item));
            Prepare(item, ProcessState.Filled);
        }

        public void ChangePrice(Order item, decimal price)
        {
            if (item.Price != price)
            {
                item.Price = price;
                _innerCollection.RemoveAt(IndexOf(item));
                _innerCollection.Add(item, item.Id);
            }
            Prepare(item, ProcessState.Movement);
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
        
        public void Fixate()
        {
            foreach (var o in Modified)
                o.PlacerState = ProcessState.Unmodified;
        }
        #endregion

        #region Private methods
        private void Prepare(Order order, ProcessState modificationType)
        {
            foreach (var o in this)
            {
                int newIndex = IndexOf(o);
                bool activeChanged = (modificationType == ProcessState.Movement && o.Id == order.Id);
                if (o.Index < _poolSize)
                {
                    if (newIndex < 0)
                        o.PlacerState = ProcessState.Cancelation;
                    else if (newIndex >= _poolSize)
                        o.PlacerState = ProcessState.Cancelation;
                    else
                        o.PlacerState = activeChanged ? ProcessState.Movement : ProcessState.Unmodified;
                }
                else
                    o.PlacerState = newIndex < _poolSize ? ProcessState.Placing : ProcessState.Unmodified;
                o.Index = newIndex;
            }
            if (modificationType == ProcessState.Cancelation && order.Index < _poolSize)
                order.PlacerState = ProcessState.Cancelation;
            OnPrepared(order, modificationType);
        }
        #endregion
    }
}