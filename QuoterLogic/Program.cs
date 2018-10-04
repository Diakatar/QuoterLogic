using System;
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
            Console.WriteLine(" 1]------------PlaceOrder(1)------------");
            Console.Write("14> "); quoter.PlaceOrder(1, 100, 10);
            Console.Write("15> "); quoter.OrderPlaced(1);
            Console.WriteLine(" 2]------------PlaceOrder(2)------------");
            Console.Write("17> "); quoter.PlaceOrder(2, 200, 10);
            Console.Write("18> "); quoter.OrderPlaced(2);
            Console.WriteLine(" 3]------------PlaceOrder(3)------------");
            Console.Write("20> "); quoter.PlaceOrder(3, 300, 10);
            Console.WriteLine(" 4]------------MoveOrder(3,2)-----------");
            Console.Write("22> "); quoter.MoveOrder(3, 150);
            Console.Write("23> "); quoter.MoveOrder(2, 50);
            Console.Write("24> "); quoter.OrderCanceled(2);
            Console.Write("25> "); quoter.OrderPlaced(3);
            Console.Write("26> "); quoter.OrderCanceled(3);
            Console.Write("27> "); quoter.OrderPlaced(2);
            Console.WriteLine(" 5]------------PlaceOrder(4)------------");
            Console.Write("29> "); quoter.PlaceOrder(4, 300, 10);
            Console.WriteLine(" 6]------------MoveOrder(4)-------------");
            Console.Write("31> "); quoter.MoveOrder(4, 50);
            Console.Write("32> "); quoter.OrderCanceled(1);
            Console.Write("33> "); quoter.OrderPlaced(4);
            Console.WriteLine(" 7]------------MoveOrder(4)-------------");
            Console.Write("35> "); quoter.MoveOrder(4, 300);
            Console.Write("36> "); quoter.OrderCanceled(4);
            Console.Write("37> "); quoter.OrderPlaced(1);
            Console.WriteLine(" 8]------------MoveOrder(4)-------------");
            Console.Write("39> "); quoter.MoveOrder(4, 300);
            Console.WriteLine(" 9]------------MoveOrder(4)-------------");
            Console.Write("41> "); quoter.MoveOrder(4, 50);
            Console.Write("42> "); quoter.OrderCanceled(1);
            Console.Write("43> "); quoter.OrderPlaced(4);
            Console.WriteLine("10]-----------MoveOrder(4)-------------");
            Console.Write("45> "); quoter.MoveOrder(4, 60);
            Console.Write("46> "); quoter.OrderMoved(4);
            Console.WriteLine("11]-----------CancelOrder(4)-----------");
            Console.Write("48> "); quoter.CancelOrder(4);
            Console.Write("49> "); quoter.OrderCanceled(4);
            Console.Write("50> "); quoter.OrderPlaced(1);
            Console.WriteLine("12]-----------CancelOrder(3)-----------");
            Console.Write("52> "); quoter.CancelOrder(3);
            Console.WriteLine("13]------------PlaceOrder(5)------------");
            Console.Write("54> "); quoter.PlaceOrder(5, 500, 10);
            Console.WriteLine("14]-----------OrderFilled(1)-----------");
            Console.Write("56> "); quoter.OrderFilled(1, 100, 10);
            Console.Write("57> "); quoter.OrderPlaced(5);
            Console.WriteLine("---------------------------------------");
            Console.ReadLine();
        }
    }
}
