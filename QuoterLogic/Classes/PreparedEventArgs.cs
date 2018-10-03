using QuoterLogic.Helpers;
using System;

namespace QuoterLogic.Classes
{
    public class PreparedEventArgs : EventArgs
    {
        public PreparedEventArgs(Order order, PlacerState modificationType)
        {
            Order = order;
            ModificationType = modificationType;
        }
        public Order Order { get; }
        public PlacerState ModificationType { get; }
    }
}