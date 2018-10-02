using System.Collections.Generic;
using System.Collections.Specialized;

namespace QuoterLogic.Interfaces
{
    public interface IObservableSortedList<T> : ICollection<T>, INotifyCollectionChanged
    {
        /// <summary>
        /// Get index of item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        int IndexOf(T item);

        /// <summary>
        /// recreate single item
        /// </summary>
        /// <param name="item"></param>
        void Update(T item);

        /// <summary>
        /// Update all of items
        /// </summary>
        void Update(); 
    }
}