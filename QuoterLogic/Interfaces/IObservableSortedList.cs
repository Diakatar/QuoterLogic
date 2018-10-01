using System.Collections.Generic;
using System.Collections.Specialized;
using QuoterLogic.Interfaces;

namespace QuoterLogic.Interfaces
{
    public interface IObservableSortedList<T> : ICollection<T>, INotifyCollectionChanged where T : ILinked<T>
    {
        int IndexOf(T item);

        void Update(T item);
    }
}