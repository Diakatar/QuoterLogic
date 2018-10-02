using System.Collections.Generic;
using System.Linq;
using QuoterLogic.Helpers;
using QuoterLogic.Interfaces;
using System;

namespace QuoterLogic.Classes
{

    public class QuoterLogic: IQuoterLogic
    {
        #region Private declarations
        private readonly IOrderPlacer _placer;
        private readonly IOrderNotificator _notificator;
        private readonly OrderCollection _buyPortfolio;
        private readonly OrderCollection _sellPortfolio;
        private readonly Queue<PlacerTask> placerQueue = new Queue<PlacerTask>();
        #endregion

        #region Ctor
        public QuoterLogic(IOrderPlacer placer, IOrderNotificator notificator, int poolSize)
        {
            _placer = placer;
            _notificator = notificator;
            _buyPortfolio = new OrderCollection(poolSize);
            _buyPortfolio.Prepared += OnCollectionPrepared;
            _sellPortfolio = new OrderCollection(poolSize);
            _sellPortfolio.Prepared += OnCollectionPrepared;
        }
        #endregion
       
        #region IOrderPlacer implementation
        public void PlaceOrder(int orderId, decimal price, int size)
        {
            if (size == 0) throw new Exception("Size could not be 0");
            Order order = new Order(orderId, price, size);
            (order.Size > 0 ? _buyPortfolio : _sellPortfolio).Add(order);
        }


        public void MoveOrder(int orderId, decimal newPrice)
        {
            var order = _buyPortfolio[orderId] ?? _sellPortfolio[orderId] ?? throw new KeyNotFoundException("Order not found");
            (order.Size > 0 ? _buyPortfolio : _sellPortfolio).ChangePrice(order, newPrice);
        }

        public void CancelOrder(int orderId)
        {
            var order = _buyPortfolio[orderId] ?? _sellPortfolio[orderId] ?? throw new KeyNotFoundException("Order not found");
            (order.Size > 0 ? _buyPortfolio : _sellPortfolio).Remove(order); 
        }
        #endregion

        #region IOrderNotificator implementation
        public void OrderPlaced(int orderId)
        {
            var order = _buyPortfolio[orderId] ?? _sellPortfolio[orderId] ?? throw new KeyNotFoundException("Order not found");
            if (order.ExpectedState != OrderState.Undefined) placerQueue.Enqueue(new PlacerTask(order, PlacerState.Unmodified));
            if (placerQueue.Count > 0) CallPlacer(); else CallNotificator(order);
            //order.ExpectedState = OrderState.Undefined;
        }

        public void OrderMoved(int orderId)
        {
            var order = _buyPortfolio[orderId] ?? _sellPortfolio[orderId] ?? throw new KeyNotFoundException("Order not found");
            if (order.ExpectedState != OrderState.Undefined) placerQueue.Enqueue(new PlacerTask(order, PlacerState.Unmodified));
            if (placerQueue.Count > 0) CallPlacer(); else CallNotificator(order);
            //order.ExpectedState = OrderState.Undefined;
        }

        public void OrderCanceled(int orderId)
        {
            var order = _buyPortfolio[orderId] ?? _sellPortfolio[orderId];
            if (order != null && order.ExpectedState != OrderState.Undefined) placerQueue.Enqueue(new PlacerTask(order, PlacerState.Unmodified));
            if (placerQueue.Count > 0) CallPlacer(); else CallNotificator(orderId);
            //if (order != null) order.ExpectedState = OrderState.Undefined;

        }

        public void OrderFilled(int orderId, int size, decimal price)
        {
            if ((_buyPortfolio[orderId] ?? _sellPortfolio[orderId]) != null) _notificator.OrderFilled(orderId, size, price);
        }
        #endregion

        #region Event handlers
        private void OnCollectionPrepared(object sender, PreparedEventArgs e)
        {
            if (!(sender is OrderCollection collection)) return;
            if (placerQueue.Count > 0) throw new Exception("Still not-notified operatons in queue, queue will be purged");
            if (placerQueue.Count > 0) placerQueue.Clear();

            foreach (var order in collection.Modified)
                placerQueue.Enqueue(new PlacerTask(order, order.PlacerState));
                
            if (placerQueue.Count > 0) CallPlacer();
            switch (e.ModificationType)
            {
                case ModificationType.Added:
                    if (e.Order.PlacerState == PlacerState.Unmodified) //заказ не вошел в пул
                    {
                        _notificator.OrderPlaced(e.Order.Id);
                        e.Order.ExpectedState = OrderState.Undefined;
                    }
                    else e.Order.ExpectedState = OrderState.Placed;
                    break;
                case ModificationType.Changed:
                case ModificationType.Unmodified:
                    switch (e.Order.PlacerState)
                    {
                        case PlacerState.PendingMovement:    // двигается по пулу
                        case PlacerState.PendingCancelation: // вылетел из пула
                        case PlacerState.PendingPlacing:     // вошел  в пул
                            e.Order.ExpectedState = OrderState.Moved;
                            break;
                        case PlacerState.Unmodified:         // не изменился
                            _notificator.OrderMoved(e.Order.Id);
                            e.Order.ExpectedState = OrderState.Undefined;
                            break;
                    }
                    break;
                case ModificationType.Deleted:
                    switch (e.Order.PlacerState)
                    {
                        case PlacerState.Unmodified:         // не изменился (то есть нет необходимости его отменять)
                            _notificator.OrderCanceled(e.Order.Id);
                            e.Order.ExpectedState = OrderState.Undefined;
                            break;
                        case PlacerState.PendingCancelation:
                            e.Order.ExpectedState = OrderState.Canceled;
                            break;
                    }
                    break;
            }
            collection.Flush();
        }
        #endregion

        #region Private methods
        private void CallNotificator(Order order)
        {
            switch (order?.ExpectedState)
            {
                case OrderState.Placed:
                    _notificator.OrderPlaced(order.Id);
                    break;
                case OrderState.Moved:
                    _notificator.OrderMoved(order.Id);
                    break;
                case OrderState.Canceled:
                case null:
                    _notificator.OrderCanceled(order.Id);
                    break;
            }
            order.ExpectedState = OrderState.Undefined;
        }

        private void CallNotificator(int orderId)
        {
            _notificator.OrderCanceled(orderId);
        }

        private void CallPlacer()
        {
            if (placerQueue.Count == 0) return;
            var data = placerQueue.Dequeue();
            switch (data.State)
            {
                case PlacerState.PendingCancelation:
                    _placer.CancelOrder(data.Order.Id);
                    break;
                case PlacerState.PendingMovement:
                    _placer.MoveOrder(data.Order.Id, data.Order.Price);
                    break;
                case PlacerState.PendingPlacing:
                    _placer.PlaceOrder(data.Order.Id, data.Order.Price, data.Order.Size);
                    break;
                case PlacerState.Unmodified:
                    CallNotificator(data.Order);
                    break;
            }
        }
        #endregion
    }
}
