namespace QuoterLogic.Interfaces
{
    public interface IOrderNotificator
    {
        void OrderPlaced(int orderId);
        void OrderMoved(int orderId);
        void OrderCanceled(int orderId);

        void OrderFilled(int orderId, int size, decimal price);
    }
}