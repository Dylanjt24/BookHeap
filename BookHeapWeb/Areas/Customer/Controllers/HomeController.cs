using BookHeap.DataAccess.Repository.IRepository;
using BookHeap.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

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
        IEnumerable<Product> allProducts = _unitOfWork.Products.GetAll(null, "Category,CoverType");
        return View(allProducts);
    }

    public IActionResult Details(int productId)
    {
        ShoppingCart shoppingCart = new()
        {
            Product = _unitOfWork.Products.GetFirstOrDefault(p => p.Id == productId, "Category,CoverType"),
            Count = 1,
            ProductId = productId
        };

        if (shoppingCart.Product == null)
            return RedirectToAction("Index");
        return View(shoppingCart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public IActionResult Details(ShoppingCart shoppingCart)
    {
        // Gets user's id from the NameIdentifier in the default Identity User object
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        shoppingCart.ApplicationUserId = claim.Value;

        ShoppingCart dbCart = _unitOfWork.ShoppingCarts.GetFirstOrDefault(
            c => c.ApplicationUserId == shoppingCart.ApplicationUserId && c.ProductId == shoppingCart.ProductId);

        if (dbCart == null)
            _unitOfWork.ShoppingCarts.Add(shoppingCart);
        else
            _unitOfWork.ShoppingCarts.IncrementCount(dbCart, shoppingCart.Count);

        _unitOfWork.Save();
        return RedirectToAction("Index");
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