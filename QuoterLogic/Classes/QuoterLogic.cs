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
        private readonly Queue<ProcessorTask> _placerQueue = new Queue<ProcessorTask>();
        private readonly Dictionary<Order, Queue<ProcessState>> _pendingOrders =
            new Dictionary<Order, Queue<ProcessState>>();
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
            if ((_buyPortfolio[orderId] ?? _sellPortfolio[orderId]) != null)
                throw new Exception("Order already exists.");
            Order order = new Order(orderId, price, size);
            (order.Size > 0 ? _buyPortfolio : _sellPortfolio).Add(order);
        }

        public void MoveOrder(int orderId, decimal newPrice)
        {
            var order = _buyPortfolio[orderId] ?? _sellPortfolio[orderId] ?? throw new KeyNotFoundException("Order not found");
            if (_placerQueue.Any(t => t.Order == order && t.IsNotification))
                throw new InvalidOperationException("Order already processing");
            (order.Size > 0 ? _buyPortfolio : _sellPortfolio).ChangePrice(order, newPrice);
        }

        public void CancelOrder(int orderId)
        {
            var order = _buyPortfolio[orderId] ?? _sellPortfolio[orderId] ?? throw new KeyNotFoundException("Order not found");
            if (_placerQueue.Any(t => t.Order == order && t.IsNotification))
                throw new InvalidOperationException("Order already processing");
            (order.Size > 0 ? _buyPortfolio : _sellPortfolio).Remove(order); 
        }
        #endregion

        #region IOrderNotificator implementation
        public void OrderPlaced(int orderId)
        {
            var order = _buyPortfolio[orderId] 
                ?? _sellPortfolio[orderId] 
                ?? throw new KeyNotFoundException("Order not found");
            if (!_pendingOrders.ContainsKey(order) || _pendingOrders[order].Count == 0 || _pendingOrders[order].Peek() != ProcessState.Placing)
                throw new Exception("Notificator returned unexpected result.");
            _pendingOrders[order].Dequeue();
            if (_placerQueue.Count > 0) Process();
        }

        public void OrderMoved(int orderId)
        {
            var order = _buyPortfolio[orderId] 
                ?? _sellPortfolio[orderId] 
                ?? throw new KeyNotFoundException("Order not found");
            if (!_pendingOrders.ContainsKey(order) 
                || _pendingOrders[order].Count == 0 
                || _pendingOrders[order].Peek() != ProcessState.Movement)
                throw new Exception("Notificator returned unexpected result.");
            _pendingOrders[order].Dequeue();
            if (_placerQueue.Count > 0) Process();
        }

        public void OrderCanceled(int orderId)
        {
            var order = _buyPortfolio[orderId] 
                ?? _sellPortfolio[orderId] 
                ?? _pendingOrders.Keys.FirstOrDefault(o=>o.Id == orderId) 
                ?? throw new KeyNotFoundException("Order not found");
            if (!_pendingOrders.ContainsKey(order) 
                || _pendingOrders[order].Count == 0 
                || _pendingOrders[order].Peek() != ProcessState.Cancelation)
                throw new Exception("Notificator returned unexpected result.");
            _pendingOrders[order].Dequeue();
            if (_placerQueue.Count > 0) Process();
        }

        public void OrderFilled(int orderId, int size, decimal price)
        {
            var order = _buyPortfolio[orderId]
                        ?? _sellPortfolio[orderId]
                        ?? _pendingOrders.Keys.FirstOrDefault(o => o.Id == orderId)
                        ?? throw new KeyNotFoundException("Order not found");
            (order.Size > 0 ? _buyPortfolio : _sellPortfolio).Fill(order);
        }
        #endregion

        #region Event handlers
        private void OnCollectionPrepared(object sender, PreparedEventArgs e)
        {
            if (!(sender is OrderCollection collection)) return;
            // выполнить удаление сначала
            if (e.ModificationType == e.Order.PlacerState && e.ModificationType == ProcessState.Cancelation)
                _placerQueue.Enqueue(new ProcessorTask(e.Order, e.Order.PlacerState));
            // выполнить удаление сначала, затем остальные действия
            foreach (var order in collection.Modified)
                _placerQueue.Enqueue(new ProcessorTask(order, order.PlacerState));
            _placerQueue.Enqueue(new ProcessorTask(e.Order, e.ModificationType, true)); 
            collection.Fixate(); // зафиксировать изменения
            Process(); // вызвать процессор
        }
        #endregion

        #region Private methods
        private void Process()
        {
            if (_placerQueue.Count == 0) return;
            var data = _placerQueue.Dequeue();
            if (data.IsNotification)
                switch (data.State)
                {
                    case ProcessState.Cancelation:
                        _notificator.OrderCanceled(data.Order.Id);
                        break;
                    case ProcessState.Movement:
                        _notificator.OrderMoved(data.Order.Id);
                        break;
                    case ProcessState.Placing:
                        _notificator.OrderPlaced(data.Order.Id);
                        break;
                    case ProcessState.Filled:
                        _notificator.OrderFilled(data.Order.Id, data.Order.Size, data.Order.Price);
                        break;
                }
            else
                switch (data.State)
                {
                    case ProcessState.Cancelation:
                        _placer.CancelOrder(data.Order.Id);
                        AddPending(data.Order, ProcessState.Cancelation);
                        break;
                    case ProcessState.Movement:
                        _placer.MoveOrder(data.Order.Id, data.Order.Price);
                        AddPending(data.Order, ProcessState.Movement);
                        break;
                    case ProcessState.Placing:
                        _placer.PlaceOrder(data.Order.Id, data.Order.Price, data.Order.Size);
                        AddPending(data.Order, ProcessState.Placing);
                        break;
                }
        }

        private void AddPending(Order order, ProcessState state)
        {
            if (!_pendingOrders.ContainsKey(order)) _pendingOrders.Add(order, new Queue<ProcessState>());
            _pendingOrders[order].Enqueue(state);
        }
        #endregion
    }
}
