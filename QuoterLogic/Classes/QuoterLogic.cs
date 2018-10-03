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
        private readonly Queue<PlacerTask> _placerQueue = new Queue<PlacerTask>();
        private readonly Dictionary<Order, Queue<PlacerState>> _pendingOrders =
            new Dictionary<Order, Queue<PlacerState>>();
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
            if (!_pendingOrders.ContainsKey(order) || _pendingOrders[order].Count == 0 || _pendingOrders[order].Peek() != PlacerState.PendingPlacing) throw new Exception("Notificator returned unexpected result.");
            //if (order.ExpectedState != OrderState.Undefined) _placerQueue.Enqueue(new PlacerTask(order, PlacerState.Unmodified));
            _pendingOrders[order].Dequeue();
            if (_placerQueue.Count > 0) CallPlacer(); //; else CallNotificator(order);
        }

        public void OrderMoved(int orderId)
        {
            var order = _buyPortfolio[orderId] ?? _sellPortfolio[orderId] ?? throw new KeyNotFoundException("Order not found");
            if (!_pendingOrders.ContainsKey(order) || _pendingOrders[order].Count == 0 || _pendingOrders[order].Peek() != PlacerState.PendingMovement) throw new Exception("Notificator returned unexpected result.");
            if (order.ExpectedState != OrderState.Undefined) _placerQueue.Enqueue(new PlacerTask(order, PlacerState.Unmodified));
            _pendingOrders[order].Dequeue();
            if (_placerQueue.Count > 0) CallPlacer(); //; else CallNotificator(order);
        }

        public void OrderCanceled(int orderId)
        {
            var order = _buyPortfolio[orderId] ?? _sellPortfolio[orderId] ?? _pendingOrders.Keys.FirstOrDefault(o=>o.Id == orderId) ?? throw new KeyNotFoundException("Order not found");
            if (!_pendingOrders.ContainsKey(order) || _pendingOrders[order].Count == 0 || _pendingOrders[order].Peek() != PlacerState.PendingCancelation) throw new Exception("Notificator returned unexpected result.");
            if (order.ExpectedState != OrderState.Undefined) _placerQueue.Enqueue(new PlacerTask(order, PlacerState.Unmodified));
            _pendingOrders[order].Dequeue();
            if (_placerQueue.Count > 0) CallPlacer(); //; else CallNotificator(orderId);
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
            /*if (_placerQueue.Count > 0) throw new Exception("Still not-notified operatons in queue, queue will be purged");
            if (_placerQueue.Count > 0) _placerQueue.Clear();*/

            // выполнить удаление сначала
            if (e.ModificationType == e.Order.PlacerState && e.ModificationType == PlacerState.PendingCancelation)
                _placerQueue.Enqueue(new PlacerTask(e.Order, e.Order.PlacerState));

            // выполнить удаление сначала, затем остальные действия
            foreach (var order in collection.Modified)
                _placerQueue.Enqueue(new PlacerTask(order, order.PlacerState));

            _placerQueue.Enqueue(new PlacerTask(e.Order, e.ModificationType, true)); 

            collection.Flush();

            if (_placerQueue.Count > 0) CallPlacer();
            /*switch (e.ModificationType)
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
            }*/
            
        }
        #endregion

        #region Private methods
        private void CallNotificator(Order order)
        {
            switch (order.ExpectedState)
            {
                case OrderState.Placed:
                    _notificator.OrderPlaced(order.Id);
                    break;
                case OrderState.Moved:
                    _notificator.OrderMoved(order.Id);
                    break;
                case OrderState.Canceled:
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
            if (_placerQueue.Count == 0) return;
            var data = _placerQueue.Dequeue();
            switch (data.State)
            {
                case PlacerState.PendingCancelation:
                    _placer.CancelOrder(data.Order.Id);
                    AddPending(data.Order, PlacerState.PendingCancelation);
                    break;
                case PlacerState.PendingMovement:
                    _placer.MoveOrder(data.Order.Id, data.Order.Price);
                    AddPending(data.Order, PlacerState.PendingMovement);
                    break;
                case PlacerState.PendingPlacing:
                    _placer.PlaceOrder(data.Order.Id, data.Order.Price, data.Order.Size);
                    AddPending(data.Order, PlacerState.PendingPlacing);
                    break;
                case PlacerState.Unmodified:
                    CallNotificator(data.Order);
                    _pendingOrders.Remove(data.Order);
                    break;
            }
        }

        private void AddPending(Order order, PlacerState state)
        {
            if (!_pendingOrders.ContainsKey(order)) _pendingOrders.Add(order, new Queue<PlacerState>());
            _pendingOrders[order].Enqueue(state);
        }

        #endregion
    }
}
