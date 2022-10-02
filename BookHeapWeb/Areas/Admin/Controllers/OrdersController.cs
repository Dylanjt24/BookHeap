using BookHeap.DataAccess.Repository.IRepository;
using BookHeap.Models;
using BookHeap.Utilities;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookHeapWeb.Areas.Admin.Controllers;

[Area("Admin")]
public class OrdersController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    public OrdersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        return View();
    }

    #region API CALLS
    [HttpGet]
    public IActionResult GetAll(string status)
    {
        // Get all OrderHeaders and include ApplicationUser
        IEnumerable<OrderHeader> orderHeaders = _unitOfWork.OrderHeaders.GetAll(null, "ApplicationUser");

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
