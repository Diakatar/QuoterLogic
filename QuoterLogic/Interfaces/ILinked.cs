using System;

namespace QuoterLogic.Interfaces
{
    public interface ILinked<T>: IComparable<T>, IEquatable<T> where T: ILinked<T>
    {
        int Id { get; }

        IObservableSortedList<T> EntireCollection { get; set; }
    }
}