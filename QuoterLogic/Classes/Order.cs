using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using QuoterLogic.Interfaces;

namespace QuoterLogic.Classes
{
    public class Order: ILinked<Order> 
    {
        private decimal _price;
        public IObservableSortedList<Order> EntireCollection { get; set; }
        public bool AwaitNotification { get; set; } = true;
        internal int Index { get; set; } = Int32.MaxValue;

        public Order(int orderId, decimal price, int size)
        {
            Id = orderId;
            Size = size;
            _price = price;
        }

        public int Id { get; }

        public int Size { get; }

        public decimal Price
        {
            get => _price;
            set
            {
                _price = value;
                EntireCollection.Update(this);
            }
        }

        public void Cancel()
        {
            EntireCollection.Remove(this);
        }

 #region IF Impl
        public bool Equals(Order other)
        {
            return (other != null && Id == other.Id);
        }

        public int CompareTo(Order other)
        {
            int cmp = Price.CompareTo(other.Price);
            if (cmp == 0) cmp = Id.CompareTo(other.Id);
            return  cmp * -1;
        }

        public override string ToString()
        {
            return Id.ToString();
        }
#endregion
    }
}