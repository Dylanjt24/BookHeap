using BookHeap.DataAccess.Repository.IRepository;
using BookHeap.Models;
using BookHeap.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookHeapWeb.Areas.Customer.Controllers;
[Area("Customer")]

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        IEnumerable<Product> allProducts = _unitOfWork.Products.GetAll("Category,CoverType");
        return View(allProducts);
    }

    public IActionResult Details(int productId)
    {
        ShoppingCart shoppingCart = new()
        {
            Product = _unitOfWork.Products.GetFirstOrDefault(p => p.Id == productId, "Category,CoverType"),
            Count = 1
        };

        if (shoppingCart.Product == null)
            return RedirectToAction("Index");
        return View(shoppingCart);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}