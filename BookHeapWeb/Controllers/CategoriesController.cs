using BookHeapWeb.Data;
using BookHeapWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookHeapWeb.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CategoriesController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            IEnumerable<Category> allCategories = _db.Categories;
            return View(allCategories);
        }
    }
}
