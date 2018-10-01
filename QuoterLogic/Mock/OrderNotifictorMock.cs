using System;
using QuoterLogic.Interfaces;

namespace QuoterLogic.Mock
{
    public class OrderNotifictorMock : IOrderNotificator
    {
        public void OrderPlaced(int orderId)
        {
            Console.WriteLine($"Notificator: placed: {orderId}");
        }

        public void OrderMoved(int orderId)
        {
            Console.WriteLine($"Notificator: moved: {orderId}");
        }

        public void OrderCanceled(int orderId)
        {
            Console.WriteLine($"Notificator: canceled: {orderId}");
        }

        public void OrderFilled(int orderId, int size, decimal price)
        {
            Console.WriteLine($"Notificator: filled: {orderId} {size} {price}");
        }
    }
}