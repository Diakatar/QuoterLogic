using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuoterLogic.Mock;

namespace QuoterLogic
{
    class Program
    {
        static void Main(string[] args)
        {
            var placer = new OrderPlacerMock();
            var notificator = new OrderNotifictorMock();
            Classes.QuoterLogic quoter = new Classes.QuoterLogic(placer, notificator, 2);
            Console.WriteLine("1]------------PlaceOrder(1)------------");
            quoter.PlaceOrder(1, 100, 10);
            quoter.OrderPlaced(1);
            Console.WriteLine("2]------------PlaceOrder(2)------------");
            quoter.PlaceOrder(2, 200, 10);
            quoter.OrderPlaced(2);
            Console.WriteLine("3]------------PlaceOrder(3)------------");
            quoter.PlaceOrder(3, 300, 10);
            Console.WriteLine("4]------------MoveOrder(3)-------------");
            quoter.MoveOrder(3, 150);
            quoter.OrderCanceled(2);
            quoter.OrderPlaced(3);
            Console.WriteLine("5]------------PlaceOrder(4)------------");
            quoter.PlaceOrder(4, 300, 10);
            Console.WriteLine("6]------------MoveOrder(4)-------------");
            quoter.MoveOrder(4, 50);
            quoter.OrderCanceled(3);
            quoter.OrderPlaced(4);
            Console.WriteLine("7]------------MoveOrder(4)-------------");
            quoter.MoveOrder(4, 300);
            quoter.OrderCanceled(4);
            quoter.OrderPlaced(3);
            Console.WriteLine("8]------------MoveOrder(4)-------------");
            quoter.MoveOrder(4, 300);
            Console.WriteLine("9]------------MoveOrder(4)-------------");
            quoter.MoveOrder(4, 50);
            quoter.OrderCanceled(3);
            quoter.OrderPlaced(4);
            Console.WriteLine("10]-----------MoveOrder(4)-------------");
            quoter.MoveOrder(4, 60);
            quoter.OrderMoved(4);
            Console.WriteLine("--------------------------------");
            quoter.MoveOrder(4, 300);
            quoter.OrderCanceled(4);
            quoter.OrderPlaced(3);
            Console.WriteLine("--------------------------------");
            quoter.CancelOrder(4);
            quoter.OrderCanceled(4);
            Console.WriteLine("--------------------------------");
            Console.ReadLine();
        }
    }
}
