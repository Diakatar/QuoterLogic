using QuoterLogic.Helpers;

namespace QuoterLogic.Classes
{
    public class ProcessorTask
    {
        public ProcessorTask(Order order, ProcessState state, bool isNotification = false)
        {
            Order = order;
            State = state;
            IsNotification = isNotification;
        }

        public Order Order { get; }
        public ProcessState State { get; }

        public bool IsNotification { get; }
    }
}
