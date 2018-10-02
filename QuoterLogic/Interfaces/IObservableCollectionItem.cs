using System;

namespace QuoterLogic.Interfaces
{
    public interface IObservableCollectionItem<T>: IComparable<T>, IEquatable<T>
    {
        int Id { get; }

        bool ContentModified { get; set; }

        int Index { get; set; }

        int NewIndex { get; }

        IObservableSortedList<T> EntireCollection { get; set; }

        void Update();
    }
}