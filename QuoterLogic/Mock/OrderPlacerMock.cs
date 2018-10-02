using System;
using QuoterLogic.Interfaces;

namespace QuoterLogic.Mock
{
    public class OrderPlacerMock: IOrderPlacer
    {
        public void PlaceOrder(int orderId, decimal price, int size)
        {
            Console.WriteLine($"Placer: place: {orderId} {price} {size}");
        }

        public void MoveOrder(int orderId, decimal newPrice)
        {
            Console.WriteLine($"Placer: move: {orderId} {newPrice}");
        }

        public void CancelOrder(int orderId)
        {
            Console.WriteLine($"Placer: cancel: {orderId}");
        }
    }
}
