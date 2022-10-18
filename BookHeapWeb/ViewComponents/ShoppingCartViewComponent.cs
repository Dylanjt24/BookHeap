using BookHeap.DataAccess.Repository.IRepository;
using BookHeap.Utilities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookHeapWeb.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            // If user is logged in
            if (claim != null)
            {
                // Return session cart count if session != null
                if (HttpContext.Session.GetInt32(SD.SessionCart) != null)
                    return View(HttpContext.Session.GetInt32(SD.SessionCart));
                // Get cart count from db and set it in session
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCarts.GetAll(c => c.ApplicationUserId == claim.Value).ToList().Count);
                return View(HttpContext.Session.GetInt32(SD.SessionCart));
            }

            HttpContext.Session.Clear();
            return View(0);
        }
    }
}
