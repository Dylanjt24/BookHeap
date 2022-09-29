using BookHeap.DataAccess.Repository.IRepository;
using BookHeap.Models;
using BookHeap.Models.ViewModels;
using BookHeap.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace BookHeapWeb.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize]
public class CartController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    [BindProperty]
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
            OrderHeader = new()
        };
        // Set price for each shopping cart in CartList and update TotalPrice
        foreach(ShoppingCart cart in ShoppingCartVM.CartList)
        {
            cart.Price = GetPriceFromQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
            ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
        }

        return View(ShoppingCartVM);
    }

    // Increase cart's count by 1
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
        // Delete cart if Count will reach 0
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

    public IActionResult Summary()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        ShoppingCartVM = new ShoppingCartVM()
        {
            CartList = _unitOfWork.ShoppingCarts.GetAll(c => c.ApplicationUserId == claim.Value, "Product"),
            OrderHeader = new()
        };
        // Set ApplicationUser to logged in user and populate OrderHeader with user's info
        ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUsers.GetFirstOrDefault(u => u.Id == claim.Value);

        ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
        ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
        ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
        ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
        ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
        ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

        foreach (ShoppingCart cart in ShoppingCartVM.CartList)
        {
            cart.Price = GetPriceFromQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
            ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
        }
        return View(ShoppingCartVM);
    }

    [HttpPost]
    [ActionName("Summary")]
    [ValidateAntiForgeryToken]
    public IActionResult SummaryPOST()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        ShoppingCartVM.CartList = _unitOfWork.ShoppingCarts.GetAll(c => c.ApplicationUserId == claim.Value, "Product");

        ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
        ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
        ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
        ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;

        foreach (ShoppingCart cart in ShoppingCartVM.CartList)
        {
            cart.Price = GetPriceFromQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
            ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
        }

        _unitOfWork.OrderHeaders.Add(ShoppingCartVM.OrderHeader);
        _unitOfWork.Save();

        // Create new OrderDetail for each cart in CartList
        foreach (ShoppingCart cart in ShoppingCartVM.CartList)
        {
            OrderDetail orderDetail = new()
            {
                ProductId = cart.ProductId,
                OrderId = ShoppingCartVM.OrderHeader.OrderHeaderId,
                Count = cart.Count,
                Price = cart.Price
            };
            _unitOfWork.OrderDetails.Add(orderDetail);
            _unitOfWork.Save();
        }

        // Stripe settings
        var domain = "https://localhost:44316/";
        var options = new SessionCreateOptions
        {
            LineItems = new List<SessionLineItemOptions>(),
            Mode = "payment",
            SuccessUrl = domain + $"Customer/Cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.OrderHeaderId}",
            CancelUrl = domain + "Customer/Cart/Index",
        };

        // Create a LineItem for each product in CartList and add it to LineItems
        foreach (ShoppingCart cart in ShoppingCartVM.CartList)
        {
            var sessionLineItem = new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    // Multiply by 100 to convert from dollars to cents
                    UnitAmount = (long)(cart.Count * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = cart.Product.Title,
                    },

                },
                Quantity = cart.Count,
            };
            options.LineItems.Add(sessionLineItem);
        }

        var service = new SessionService();
        Session session = service.Create(options);
        // Update OrderHeader properties with info from Stripe session
        _unitOfWork.OrderHeaders.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.OrderHeaderId, session.Id, session.PaymentIntentId);
        _unitOfWork.Save();

        Response.Headers.Add("Location", session.Url);
        return new StatusCodeResult(303);

        //_unitOfWork.ShoppingCarts.RemoveRange(ShoppingCartVM.CartList);
        //_unitOfWork.Save();

        //return RedirectToAction("Index", "Home");
    }

    public IActionResult OrderConfirmation(int orderId)
    {
        OrderHeader orderHeader = _unitOfWork.OrderHeaders.GetFirstOrDefault(o => o.OrderHeaderId == orderId);
        var service = new SessionService();
        Session session = service.Get(orderHeader.SessionId);
        // Check Stripe payment status is approved
        if(session.PaymentStatus.ToLower() == "paid")
        {
            _unitOfWork.OrderHeaders.UpdateStatus(orderId, SD.StatusApproved, SD.PaymentStatusApproved);
            _unitOfWork.Save();
        }
        // Clear shopping cart
        List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCarts.GetAll(c => c.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
        _unitOfWork.ShoppingCarts.RemoveRange(shoppingCarts);
        _unitOfWork.Save();

        return View(orderId);
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
