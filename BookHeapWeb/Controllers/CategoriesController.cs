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
        [HttpGet("/categories")]
        public IActionResult Index()
        {
            IEnumerable<Category> allCategories = _db.Categories;
            return View(allCategories);
        }
        [HttpGet("/categories/new")]
        public IActionResult New()
        {
            return View();
        }

        [HttpPost("/categories/create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category newCategory)
        {
            if (!ModelState.IsValid)
                return View("New");

            _db.Categories.Add(newCategory);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet("/categories/{categoryId:int}/edit")]
        public IActionResult Edit(int categoryId)
        {
            Category? dbCategory = _db.Categories.Find(categoryId);
            if (dbCategory == null)
                RedirectToAction("Index");
            return View(dbCategory);
        }
    }
}
