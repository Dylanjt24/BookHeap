using Microsoft.AspNetCore.Mvc;

namespace BookHeapWeb.Areas.Customer.Controllers;

[Area("Customer")]
public class CartController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
