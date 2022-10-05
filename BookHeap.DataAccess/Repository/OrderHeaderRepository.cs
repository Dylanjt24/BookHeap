using BookHeap.DataAccess.Repository.IRepository;
using BookHeap.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookHeap.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderHeader orderHeader)
        {
            _db.OrderHeaders.Update(orderHeader);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            OrderHeader dbOrder = _db.OrderHeaders.FirstOrDefault(o => o.OrderHeaderId == id);
            if (dbOrder != null)
            {
                dbOrder.OrderStatus = orderStatus;
                dbOrder.UpdatedAt = DateTime.Now;
                if (paymentStatus != null)
                    dbOrder.PaymentStatus = paymentStatus;
            }
        }

        public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
        {
            OrderHeader dbOrder = _db.OrderHeaders.FirstOrDefault(o => o.OrderHeaderId == id);
            if (dbOrder != null)
            {
                dbOrder.PaymentDate = DateTime.Now;
                dbOrder.SessionId = sessionId;
                dbOrder.PaymentIntentId = paymentIntentId;
                dbOrder.UpdatedAt = DateTime.Now;
            }
        }
    }
}
