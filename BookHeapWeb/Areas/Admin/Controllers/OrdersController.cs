using BookHeap.DataAccess.Repository.IRepository;
using BookHeap.Models;
using BookHeap.Models.ViewModels;
using BookHeap.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace BookHeapWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class OrdersController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    [BindProperty]
    public OrderVM OrderVM { get; set; }
    public OrdersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Details(int orderId)
    {
        OrderVM = new()
        {
            OrderHeader = _unitOfWork.OrderHeaders.GetFirstOrDefault(o => o.OrderHeaderId == orderId, "ApplicationUser"),
            OrderDetails = _unitOfWork.OrderDetails.GetAll(o => o.OrderId == orderId, "Product")
        };

        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        // If order is null or the user isn't the one who placed the order and is not admin/employee
        if (OrderVM.OrderHeader == null || (OrderVM.OrderHeader.ApplicationUserId != claim.Value && !(User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))))
            return RedirectToAction("Index");
        return View(OrderVM);
    }

    [ActionName("Details")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult PayNow()
    {
        OrderVM.OrderHeader = _unitOfWork.OrderHeaders.GetFirstOrDefault(o => o.OrderHeaderId == OrderVM.OrderHeader.OrderHeaderId, "ApplicationUser");
        OrderVM.OrderDetails = _unitOfWork.OrderDetails.GetAll(o => o.OrderId == OrderVM.OrderHeader.OrderHeaderId, "Product");

        // Stripe settings
        var domain = "https://localhost:44316/";
        var options = new SessionCreateOptions
        {
            LineItems = new List<SessionLineItemOptions>(),
            Mode = "payment",
            SuccessUrl = domain + $"Admin/Orders/PaymentConfirmation?orderHeaderId={OrderVM.OrderHeader.OrderHeaderId}",
            CancelUrl = domain + $"Admin/Order/Details?orderId={OrderVM.OrderHeader.OrderHeaderId}",
        };

        // Create a LineItem for each product in CartList and add it to LineItems
        foreach (OrderDetail detail in OrderVM.OrderDetails)
        {
            var sessionLineItem = new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    // Multiply by 100 to convert from dollars to cents
                    UnitAmount = (long)(detail.Price * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = detail.Product.Title,
                    },

                },
                Quantity = detail.Count,
            };
            options.LineItems.Add(sessionLineItem);
        }

        var service = new SessionService();
        Session session = service.Create(options);
        // Update OrderHeader properties with info from Stripe session
        _unitOfWork.OrderHeaders.UpdateStripePaymentId(OrderVM.OrderHeader.OrderHeaderId, session.Id, session.PaymentIntentId);
        _unitOfWork.Save();

        Response.Headers.Add("Location", session.Url);
        return new StatusCodeResult(303);
    }

    public IActionResult PaymentConfirmation(int orderHeaderId)
    {
        OrderHeader orderHeader = _unitOfWork.OrderHeaders.GetFirstOrDefault(o => o.OrderHeaderId == orderHeaderId);

        // Check Stripe payment status if user not approved for delayed payment
        if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
        {
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);
            // Check if Stripe payment status is approved
            if (session.PaymentStatus.ToLower() == "paid")
            {
                // Assign PaymentIntentId after payment has been made
                _unitOfWork.OrderHeaders.UpdateStripePaymentId(orderHeaderId, orderHeader.SessionId, session.PaymentIntentId);
                _unitOfWork.OrderHeaders.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                _unitOfWork.Save();
            }
        }

        return View(orderHeaderId);
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    [ValidateAntiForgeryToken]
    public IActionResult Update()
    {
        // OrderVM coming from BindProperty at the top of controller
        _unitOfWork.OrderHeaders.Update(OrderVM.OrderHeader);
        _unitOfWork.Save();
        TempData["Success"] = "Order details updated successfully";
        return RedirectToAction("Details", "Orders", new { orderId = OrderVM.OrderHeader.OrderHeaderId });
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    [ValidateAntiForgeryToken]
    public IActionResult StartProcessing()
    {
        _unitOfWork.OrderHeaders.UpdateStatus(OrderVM.OrderHeader.OrderHeaderId, SD.StatusProcessing);
        _unitOfWork.Save();
        TempData["Success"] = "Order status updated successfully";
        return RedirectToAction("Details", "Orders", new { orderId = OrderVM.OrderHeader.OrderHeaderId });
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    [ValidateAntiForgeryToken]
    public IActionResult ShipOrder()
    {
        OrderHeader orderHeader = _unitOfWork.OrderHeaders.GetFirstOrDefault(o => o.OrderHeaderId == OrderVM.OrderHeader.OrderHeaderId);
        orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
        orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
        orderHeader.OrderStatus = SD.StatusShipped;
        orderHeader.ShippingDate = DateTime.Now;
        if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
        _unitOfWork.OrderHeaders.Update(orderHeader);
        _unitOfWork.Save();
        TempData["Success"] = "Order shipped successfully";
        return RedirectToAction("Details", "Orders", new { orderId = OrderVM.OrderHeader.OrderHeaderId });

    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    [ValidateAntiForgeryToken]
    public IActionResult CancelOrder()
    {
        OrderHeader orderHeader = _unitOfWork.OrderHeaders.GetFirstOrDefault(o => o.OrderHeaderId == OrderVM.OrderHeader.OrderHeaderId);
        // Create Stripe refund if payment has already been received
        if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
        {
            var options = new RefundCreateOptions
            {
                PaymentIntent = OrderVM.OrderHeader.PaymentIntentId,
                Reason = RefundReasons.RequestedByCustomer
            };
            var service = new RefundService();
            service.Create(options);
            _unitOfWork.OrderHeaders.UpdateStatus(OrderVM.OrderHeader.OrderHeaderId, SD.StatusCancelled, SD.StatusRefunded);
        }
        // If payment hasn't been made yet, just cancel the order
        else
        {
            _unitOfWork.OrderHeaders.UpdateStatus(OrderVM.OrderHeader.OrderHeaderId, SD.StatusCancelled, SD.StatusCancelled);
        }
        _unitOfWork.Save();
        TempData["Success"] = "Order cancelled successfully";
        return RedirectToAction("Details", "Orders", new { orderId = OrderVM.OrderHeader.OrderHeaderId });

    }

    #region API CALLS
    [HttpGet]
    public IActionResult GetAll(string status)
    {
        IEnumerable<OrderHeader> orderHeaders;
        // If logged in user is Admin or Employee, get all OrderHeaders and include ApplicationUser for getting email
        // Else only get logged in user's orders
        if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            orderHeaders = _unitOfWork.OrderHeaders.GetAll(null, "ApplicationUser");
        else
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            orderHeaders = _unitOfWork.OrderHeaders.GetAll(o => o.ApplicationUserId == claim.Value, "ApplicationUser");
        }

        // Filter orders based on selected status
        switch (status)
        {
            case "processing":
                orderHeaders = orderHeaders.Where(o => o.PaymentStatus == SD.PaymentStatusDelayedPayment);
                break;

            case "pending":
                orderHeaders = orderHeaders.Where(o => o.OrderStatus == SD.StatusPending);
                break;

            case "completed":
                orderHeaders = orderHeaders.Where(o => o.OrderStatus == SD.StatusShipped);
                break;

            case "approved":
                orderHeaders = orderHeaders.Where(o => o.OrderStatus == SD.StatusApproved);
                break;

            default:
                break;
        }
        return Json(new { data = orderHeaders });
    }
    #endregion
}
