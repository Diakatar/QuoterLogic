using System;
using QuoterLogic.Helpers;
using QuoterLogic.Interfaces;

namespace QuoterLogic.Classes
{
    public class Order: IObservableCollectionItem<Order> 
    {
        #region Private declarations
        private decimal _price;
        #endregion

        #region Ctor
        public Order(int orderId, decimal price, int size)
        {
            Id = orderId;
            Size = size;
            _price = price;
        }
        #endregion

        #region IObservableCollectionItem implementation
        public int Id { get; }

        public bool ContentModified { get; set; } = true;

        public int Index { get; set; } = Int32.MaxValue;

        public int NewIndex => EntireCollection.Contains(this) ? EntireCollection.IndexOf(this) : -1;

        public IObservableSortedList<Order> EntireCollection { get; set; }

        public void Update()
        {
            ContentModified = false;
            Index = NewIndex;
        }


        #endregion

        #region IEquatable implementation
        public bool Equals(Order other)
        {
            return (other != null && Id == other.Id);
        }
        #endregion

        #region IComparable implementation
        public int CompareTo(Order other)
        {
            int cmp = Price.CompareTo(other.Price);
            if (cmp == 0) cmp = Id.CompareTo(other.Id);
            return cmp * Math.Sign(Size);
        }
        #endregion

        #region Public properties
        public bool AwaitNotification { get; set; } = true;

        public OrderState State { get; set; } = OrderState.Undefined;

        public decimal Price
        {
            get => _price;
            set
            {
                if (_price != value) ContentModified = true;
                _price = value;
                EntireCollection.Update(this);
            }
        }

        public int Size { get; }
        #endregion

        #region Internal methods
        internal PlacerState GetPlacerState(int poolSize)
        {
            if (Index == NewIndex)
                return ContentModified ? PlacerState.PendingMovement : PlacerState.Unmodified;
            if (Index < poolSize)
            {
                if (NewIndex < 0)
                    return PlacerState.PendingCancelation;
                if (NewIndex >= poolSize)
                    return PlacerState.PendingCancelation;
                return ContentModified ? PlacerState.PendingMovement : PlacerState.Unmodified;
            }
            return NewIndex < poolSize ? PlacerState.PendingPlacing : PlacerState.Unmodified;
        }


        internal void Cancel()
        {
            EntireCollection.Remove(this);
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return Id.ToString();
        }
        #endregion
    }
}