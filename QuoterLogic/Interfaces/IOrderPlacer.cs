namespace QuoterLogic.Interfaces
{
    public interface IOrderPlacer
    {
        void PlaceOrder(int orderId, decimal price, int size);
        void MoveOrder(int orderId, decimal newPrice);
        void CancelOrder(int orderId);
    }
}