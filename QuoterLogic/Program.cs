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
            Console.Write("18> "); quoter.PlaceOrder(1, 100, 10);
            Console.Write("19> "); quoter.OrderPlaced(1);
            Console.WriteLine("2]------------PlaceOrder(2)------------");
            Console.Write("21> "); quoter.PlaceOrder(2, 200, 10);
            Console.Write("22> "); quoter.OrderPlaced(2);
            Console.WriteLine("3]------------PlaceOrder(3)------------");
            Console.Write("24> "); quoter.PlaceOrder(3, 300, 10);
            Console.WriteLine("4]------------MoveOrder(3)-------------");
            Console.Write("26> "); quoter.MoveOrder(3, 150);
            Console.Write("26X "); quoter.MoveOrder(2, 250);
            Console.Write("27> "); quoter.OrderCanceled(2);
            Console.Write("28> "); quoter.OrderPlaced(3);
            Console.Write("28> "); quoter.OrderCanceled(3);
            Console.Write("28> "); quoter.OrderPlaced(2);
            Console.WriteLine("5]------------PlaceOrder(4)------------");
            Console.Write("30> "); quoter.PlaceOrder(4, 300, 10);
            Console.WriteLine("6]------------MoveOrder(4)-------------");
            Console.Write("32> "); quoter.MoveOrder(4, 50);
            Console.Write("33> "); quoter.OrderCanceled(3);
            Console.Write("34> "); quoter.OrderPlaced(4);
            Console.WriteLine("7]------------MoveOrder(4)-------------");
            Console.Write("36> "); quoter.MoveOrder(4, 300);
            Console.Write("37> "); quoter.OrderCanceled(4);
            Console.Write("38> "); quoter.OrderPlaced(3);
            Console.WriteLine("8]------------MoveOrder(4)-------------");
            Console.Write("40> "); quoter.MoveOrder(4, 300);
            Console.WriteLine("9]------------MoveOrder(4)-------------");
            Console.Write("42> "); quoter.MoveOrder(4, 50);
            Console.Write("43> "); quoter.OrderCanceled(3);
            Console.Write("44> "); quoter.OrderPlaced(4);
            Console.WriteLine("10]-----------MoveOrder(4)-------------");
            Console.Write("46> "); quoter.MoveOrder(4, 60);
            Console.Write("47> "); quoter.OrderMoved(4);
            Console.WriteLine("11]-----------CancelOrder(4)-----------");
            Console.Write("49> "); quoter.CancelOrder(4);
            Console.Write("50> "); quoter.OrderCanceled(4);
            Console.Write("50> "); quoter.OrderPlaced(3);
            Console.WriteLine("11]-----------CancelOrder(2)-----------");
            Console.Write("52> "); quoter.CancelOrder(2);
            Console.WriteLine("---------------------------------------");
            Console.ReadLine();
        }
    }
}
