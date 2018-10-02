using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using QuoterLogic.Helpers;
using QuoterLogic.Interfaces;

namespace QuoterLogic.Classes
{
    public class QuoterLogic: IQuoterLogic
    {
        #region Private declarations
        private readonly IOrderPlacer _placer;
        private readonly IOrderNotificator _notificator;
        private readonly int _poolSize;
        private readonly ObservableSortedList<Order> _buyPortfolio = new ObservableSortedList<Order>();
        private readonly ObservableSortedList<Order> _sellPortfolio = new ObservableSortedList<Order>();
        #endregion

        #region Ctor
        public QuoterLogic(IOrderPlacer placer, IOrderNotificator notificator, int poolSize)
        {
            _placer = placer;
            _notificator = notificator;
            _poolSize = poolSize;
            _buyPortfolio.CollectionChanged += OnCollectionChanged;
            _sellPortfolio.CollectionChanged += OnCollectionChanged;
        }
        #endregion

        #region IOrderPlacer implementation
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
            order.Price = newPrice;
        }

        public void CancelOrder(int orderId)
        {
            var order = _buyPortfolio[orderId] ?? _sellPortfolio[orderId] ?? throw new KeyNotFoundException();
            order.Cancel();
        }
        #endregion

        #region IOrderNotificator implementation
        public void OrderPlaced(int orderId)
        {
            var order = _buyPortfolio[orderId] ?? _sellPortfolio[orderId] ?? throw new KeyNotFoundException();
            if (!order.AwaitNotification) return;
            order.AwaitNotification = false;
            order.State = OrderState.Placed;
            _notificator.OrderPlaced(orderId);
        }

        public void OrderMoved(int orderId)
        {
            var order = _buyPortfolio[orderId] ?? _sellPortfolio[orderId] ?? throw new KeyNotFoundException();
            if (!order.AwaitNotification) return;
            order.AwaitNotification = false;
            order.State = OrderState.Moved;
            _notificator.OrderMoved(orderId);
        }

        public void OrderCanceled(int orderId)
        {
            var order = _buyPortfolio[orderId] ?? _sellPortfolio[orderId];
            if (order != null) return;
            _notificator.OrderCanceled(orderId);
        }

        public void OrderFilled(int orderId, int size, decimal price)
        {
            if ((_buyPortfolio[orderId] ?? _sellPortfolio[orderId]) != null) _notificator.OrderFilled(orderId, size, price);
        }
        #endregion

        #region Event handlers
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!(sender is IObservableSortedList<Order> collection)) return;

            foreach (var dim in
                collection
                    .Select(o => new { order = o, state = o.GetPlacerState(_poolSize) })
                    .Where(i => i.state != PlacerState.Unmodified)
                    .OrderBy(m => m.state))
            {
                switch (dim.state)
                {
                    case PlacerState.PendingCancelation:
                        _placer.CancelOrder(dim.order.Id);
                        break;
                    case PlacerState.PendingMovement:
                        _placer.MoveOrder(dim.order.Id, dim.order.Price);
                        break;
                    case PlacerState.PendingPlacing:
                        _placer.PlaceOrder(dim.order.Id, dim.order.Price, dim.order.Size);
                        break;
                }
            }

            foreach (var order in (e.OldItems ?? e.NewItems).Cast<Order>())
            {
                order.AwaitNotification = false;
                switch (order.State)
                {
                    case OrderState.Undefined:
                    case OrderState.Canceled: // а нужно ли ???
                        switch (e.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                                if (order.GetPlacerState(_poolSize) == PlacerState.Unmodified) //заказ не вошел в пул
                                {
                                    _notificator.OrderPlaced(order.Id);
                                    order.State = OrderState.Placed;
                                }
                                else order.AwaitNotification = true;
                                break;
                            default: // изменить то, чего не было (вырожденная ситуация)
                                break;
                        }
                        break;
                    case OrderState.Placed:
                    case OrderState.Moved:
                        switch (e.Action)
                        {
                            case NotifyCollectionChangedAction.Replace:
                                switch (order.GetPlacerState(_poolSize))
                                {

                                    case PlacerState.PendingMovement:   // двигается по пулу
                                        order.AwaitNotification = true;
                                        break;
                                    case PlacerState.PendingCancelation: // вылетел из пула
                                    case PlacerState.PendingPlacing:     // вошел  в пул
                                    case PlacerState.Unmodified:         // не изменился
                                        _notificator.OrderMoved(order.Id);
                                        order.State = OrderState.Moved;
                                        break;
                                }
                                break;
                            case NotifyCollectionChangedAction.Remove:
                                switch (order.GetPlacerState(_poolSize))
                                {
                                    case PlacerState.Unmodified:         // не изменился (то есть нет необходимости его отменять)
                                        _notificator.OrderCanceled(order.Id);
                                        order.State = OrderState.Canceled;
                                        break;
                                    case PlacerState.PendingCancelation:
                                        order.AwaitNotification = true;
                                        break;
                                }
                                break;
                        }
                        break;
                }
            }
            collection.Update();
        }
        #endregion
    }
}
