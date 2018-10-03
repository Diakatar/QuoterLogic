using System;
using QuoterLogic.Helpers;

namespace QuoterLogic.Classes
{
    public class Order: IEquatable<Order>, IComparable<Order>
    {
        #region Ctor
        public Order(int orderId, decimal price, int size)
        {
            Id = orderId;
            Size = size;
            Price = price;
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
        public int Id { get; }

        public int Index { get; set; } = Int32.MaxValue;

        public ProcessState PlacerState { get; set; } = ProcessState.Unmodified;

        public decimal Price { get; set; }
       
        public int Size { get; }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return Id.ToString();
        }
        #endregion
    }
}