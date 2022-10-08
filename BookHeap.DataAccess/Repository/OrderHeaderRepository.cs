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
            OrderHeader dbOrderHeader = _db.OrderHeaders.FirstOrDefault(o => o.OrderHeaderId == orderHeader.OrderHeaderId);
            if (dbOrderHeader != null)
            {
                dbOrderHeader.Name = orderHeader.Name;
                dbOrderHeader.PhoneNumber = orderHeader.PhoneNumber;
                dbOrderHeader.StreetAddress = orderHeader.StreetAddress;
                dbOrderHeader.City = orderHeader.City;
                dbOrderHeader.State = orderHeader.State;
                dbOrderHeader.PostalCode = orderHeader.PostalCode;
                dbOrderHeader.UpdatedAt = DateTime.Now;
                if (orderHeader.Carrier != null)
                    dbOrderHeader.Carrier = orderHeader.Carrier;
                if (orderHeader.TrackingNumber != null)
                    dbOrderHeader.TrackingNumber = orderHeader.TrackingNumber;
            }

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
