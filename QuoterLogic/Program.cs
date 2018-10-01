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
            Console.WriteLine("--------------------------------");
            quoter.PlaceOrder(1, 100, 10);
            Console.WriteLine("--------------------------------");
            quoter.PlaceOrder(2, 100, -10);
            Console.WriteLine("--------------------------------");
            quoter.PlaceOrder(3, 200, -10);
            Console.WriteLine("--------------------------------");
            quoter.PlaceOrder(4, 300, -10);
            Console.WriteLine("--------------------------------");
            quoter.MoveOrder(4, 50);
            Console.WriteLine("--------------------------------");
            quoter.MoveOrder(4, 300);
            Console.WriteLine("--------------------------------");
            quoter.MoveOrder(4, 300);
            Console.WriteLine("--------------------------------");
            quoter.MoveOrder(4, 50);
            Console.WriteLine("--------------------------------");
            quoter.MoveOrder(4, 300);
            Console.WriteLine("--------------------------------");
            quoter.CancelOrder(4);
            Console.WriteLine("--------------------------------");
            Console.ReadLine();
        }
    }
}
