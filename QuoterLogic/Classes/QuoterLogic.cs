using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using QuoterLogic.Interfaces;

namespace QuoterLogic.Classes
{
    public class QuoterLogic: IQuoterLogic
    {
        private readonly IOrderPlacer _placer;
        private readonly IOrderNotificator _notificator;
        private readonly int _poolSize;
        private readonly ObservableSortedList<Order> _buyPortfolio = new ObservableSortedList<Order>();
        private readonly ObservableSortedList<Order> _sellPortfolio = new ObservableSortedList<Order>();

        public QuoterLogic(IOrderPlacer placer, IOrderNotificator notificator, int poolSize)
        {
            _placer = placer;
            _notificator = notificator;
            _poolSize = poolSize;
            _buyPortfolio.CollectionChanged += OnCollectionChanged;
            _sellPortfolio.CollectionChanged += OnCollectionChanged;
        }

        private void Manage(NotifyCollectionChangedAction action, IList affected, Order order, int from, int to)
        {
            // отменить удаленный заказ, в этом случае ответ придет через Notificator
            if (from < _poolSize && action == NotifyCollectionChangedAction.Remove && affected.Contains(order.Id))
                _placer.CancelOrder(order.Id);
            // отменить удаленный заказ, в этом случае ответ придет сразу
            if (from >= _poolSize && action == NotifyCollectionChangedAction.Remove && affected.Contains(order.Id))
            {
                _notificator.OrderCanceled(order.Id); // сообщаем клиенту
                order.AwaitNotification = false; // не ждем нотификации по этому заказу
            }

            if (from == to && affected.Contains(order.Id) && action == NotifyCollectionChangedAction.Replace)
            {
                _notificator.OrderMoved(order.Id); // клианту собщаем что заказ изменен
                order.AwaitNotification = false;
            }

            // заказ из списка соседей (включая модифицированный) вышел из пула после изменений
            if (from < _poolSize && to >= _poolSize)
            {
                // для тех заказов которые были только что добалены но не вошли в пул
                if (affected.Contains(order.Id) && action == NotifyCollectionChangedAction.Add)
                {
                    _notificator.OrderPlaced(order.Id); // клианту собщаем что заказ размещен
                    order.AwaitNotification = false;
                }
                else if (affected.Contains(order.Id) && action == NotifyCollectionChangedAction.Replace)
                {
                    _notificator.OrderMoved(order.Id); // клианту собщаем что заказ изменен
                    order.AwaitNotification = false;
                }
                
                // переместились и не вошли в пул (новые добавленные заказы не могут быть отменены т.к. не опубликованы)
                if (!affected.Contains(order.Id) || action != NotifyCollectionChangedAction.Add)
                    _placer.CancelOrder(order.Id);
            }

            //заказ из списка соседей (включая модифицированный) попал в пул после изменений
            if (from >= _poolSize && to < _poolSize && to > -1)
            {
                // для тех заказов, которые были изменены и вошли в пул (удаленного заказа тут быть не может)
                if (affected.Contains(order.Id) && action == NotifyCollectionChangedAction.Replace)
                {
                    _notificator.OrderMoved(order.Id);
                    order.AwaitNotification = false;
                }
                // публиковать заказ из пула вне нависимости от того производится действие над ним или над соседями
                _placer.PlaceOrder(order.Id, order.Price, order.Size);
            }
            order.Index = to;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!(sender is IObservableSortedList<Order> collection)) return;
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems.Cast<Order>())
                {
                    Manage(e.Action, new List<int> {item.Id}, item, item.Index, -1);
                }
            }

            // перебрать коллекцию там где изменился индекс
            foreach (var mod in collection
                //.Where(p => p.Index != collection.IndexOf(p))
                .OrderByDescending(i => collection.IndexOf(i))
                .Select(o => new {Order = o, OldIndex = o.Index, NewIndex = collection.IndexOf(o)}))
                // вызвать менеджер для обработки изменившихся элементов
                Manage(e.Action, e.OldItems??e.NewItems, mod.Order, mod.OldIndex, mod.NewIndex);
        }

        public void PlaceOrder(int orderId, decimal price, int size)
        {
            if (size == 0) return;
            Order order = new Order(orderId, price, size);
            if (size > 0) _buyPortfolio.Add(order);
            if (size < 0) _sellPortfolio.Add(order);
        }

     
        public void MoveOrder(int orderId, decimal newPrice)
        {
            var order = _buyPortfolio[orderId] ?? _sellPortfolio[orderId] ?? throw new KeyNotFoundException();
            order.AwaitNotification = true;
            order.Price = newPrice;
        }

        public void CancelOrder(int orderId)
        {
            var order = _buyPortfolio[orderId] ?? _sellPortfolio[orderId] ?? throw new KeyNotFoundException();
            order.AwaitNotification = true;
            order.Cancel();
        }

        public void OrderPlaced(int orderId)
        {
            var order = _buyPortfolio[orderId] ?? _sellPortfolio[orderId] ?? throw new KeyNotFoundException();
            if (order.AwaitNotification) _notificator.OrderPlaced(orderId);
        }

        public void OrderMoved(int orderId)
        {
            var order = _buyPortfolio[orderId] ?? _sellPortfolio[orderId] ?? throw new KeyNotFoundException();
            if (order.AwaitNotification) _notificator.OrderMoved(orderId);
        }

        public void OrderCanceled(int orderId)
        {
            var order = _buyPortfolio[orderId] ?? _sellPortfolio[orderId] ?? throw new KeyNotFoundException();
            if (order.AwaitNotification) _notificator.OrderCanceled(orderId);
        }

        public void OrderFilled(int orderId, int size, decimal price)
        {
            if ((_buyPortfolio[orderId] ?? _sellPortfolio[orderId]) != null) _notificator.OrderFilled(orderId, size, price);
        }
    }
}
