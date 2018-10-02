using QuoterLogic.Helpers;

namespace QuoterLogic.Classes
{
    public class PlacerTask
    {
        public PlacerTask(Order order, PlacerState state)
        {
            Order = order;
            State = state;
        }

        public Order Order { get; }
        public PlacerState State { get; }
    }
}
