﻿using BookHeap.DataAccess.Repository.IRepository;
using BookHeap.Models;
using BookHeap.Models.ViewModels;
using BookHeap.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        return View(OrderVM);
    }

    #region API CALLS
    [HttpGet]
    public IActionResult GetAll(string status)
    {
        IEnumerable<OrderHeader> orderHeaders;
        // Get all OrderHeaders and include ApplicationUser for getting email
        if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            orderHeaders = _unitOfWork.OrderHeaders.GetAll(null, "ApplicationUser");
        else
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            orderHeaders = _unitOfWork.OrderHeaders.GetAll(o => o.ApplicationUserId == claim.Value, "ApplicationUser");
        }


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
