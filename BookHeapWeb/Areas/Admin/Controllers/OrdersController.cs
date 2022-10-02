using BookHeap.DataAccess.Repository.IRepository;
using BookHeap.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookHeapWeb.Areas.Admin.Controllers;

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
    public IActionResult GetAll()
    {
        // Get all OrderHeaders and include ApplicationUser
        IEnumerable<OrderHeader> orderHeaders = _unitOfWork.OrderHeaders.GetAll(null, "ApplicationUser");
        return Json(new { data = orderHeaders });
    }
    #endregion
}
