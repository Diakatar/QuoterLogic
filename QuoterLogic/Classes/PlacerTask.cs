using QuoterLogic.Helpers;

namespace QuoterLogic.Classes
{
    public class PlacerTask
    {
        public PlacerTask(Order order, PlacerState state, bool isNotification = false)
        {
            Order = order;
            State = state;
            IsNotification = isNotification;
        }

        public Order Order { get; }
        public PlacerState State { get; }

        public bool IsNotification { get; }
    }
}
