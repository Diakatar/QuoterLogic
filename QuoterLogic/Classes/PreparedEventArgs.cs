using QuoterLogic.Helpers;
using System;

namespace QuoterLogic.Classes
{
    public class PreparedEventArgs : EventArgs
    {
        public PreparedEventArgs(Order order, ProcessState modificationType)
        {
            Order = order;
            ModificationType = modificationType;
        }
        public Order Order { get; }
        public ProcessState ModificationType { get; }
    }
}