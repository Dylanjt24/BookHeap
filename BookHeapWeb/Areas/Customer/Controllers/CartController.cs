using BookHeap.DataAccess.Repository.IRepository;
using BookHeap.Models;
using BookHeap.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookHeapWeb.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize]
public class CartController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    public ShoppingCartVM ShoppingCartVM { get; set; }
    public CartController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        ShoppingCartVM = new ShoppingCartVM()
        {
            // Get all shopping carts with the logged in user id and include Products
            CartList = _unitOfWork.ShoppingCarts.GetAll(c => c.ApplicationUserId == claim.Value, "Product"),
            TotalPrice = 0
        };
        // Set price for each shopping cart in CartList and update TotalPrice
        foreach(ShoppingCart cart in ShoppingCartVM.CartList)
        {
            cart.Price = GetPriceFromQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
            ShoppingCartVM.TotalPrice += cart.Price * cart.Count;
        }

        return View(ShoppingCartVM);
    }

    public IActionResult IncreaseCount(int cartId)
    {
        ShoppingCart dbCart = _unitOfWork.ShoppingCarts.GetFirstOrDefault(c => c.ShoppingCartId == cartId);
        if (dbCart == null)
            return RedirectToAction("Index", "Home");
        _unitOfWork.ShoppingCarts.IncrementCount(dbCart, 1);
        _unitOfWork.Save();
        return RedirectToAction("Index");
    }

    public IActionResult DecreaseCount(int cartId)
    {
        ShoppingCart dbCart = _unitOfWork.ShoppingCarts.GetFirstOrDefault(c => c.ShoppingCartId == cartId);
        if (dbCart.Count <= 1)
            _unitOfWork.ShoppingCarts.Remove(dbCart);
        else
            _unitOfWork.ShoppingCarts.DecrementCount(dbCart, 1);
        _unitOfWork.Save();
        return RedirectToAction("Index");
    }

    public IActionResult Delete(int cartId)
    {
        ShoppingCart dbCart = _unitOfWork.ShoppingCarts.GetFirstOrDefault(c => c.ShoppingCartId == cartId);
        _unitOfWork.ShoppingCarts.Remove(dbCart);
        _unitOfWork.Save();
        return RedirectToAction("Index");
    }

    // Adjusts price based on quantity of products in the cart
    private double GetPriceFromQuantity(double quantity, double price, double price50, double price100)
    {
        if (quantity > 99)
            return price100;
        if (quantity > 49)
            return price50;
        return price;
    }
}
