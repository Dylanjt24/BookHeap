using BookHeap.DataAccess;
using BookHeap.DataAccess.Repository.IRepository;
using BookHeap.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookHeapWeb.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ICategoryRepository _db;

        public CategoriesController(ICategoryRepository db)
        {
            _db = db;
        }
        [HttpGet("/categories")]
        public IActionResult Index()
        {
            IEnumerable<Category> allCategories = _db.GetAll();
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

            _db.Add(newCategory);
            _db.Save();
            TempData["Success"] = "Category created successfully";
            return RedirectToAction("Index");
        }

        [HttpGet("/categories/{categoryId:int}/edit")]
        public IActionResult Edit(int categoryId)
        {
            Category? dbCategory = _db.GetFirstOrDefault(c => c.CategoryId == categoryId);
            if (dbCategory == null)
                return RedirectToAction("Index");
            return View(dbCategory);
        }

        //[HttpPost("/categories/{categoryId:int}/update")]
        //[ValidateAntiForgeryToken]
        //public IActionResult Update(Category updatedCategory, int categoryId)
        //{
        //    if (!ModelState.IsValid)
        //        return View("Edit", updatedCategory);

        //    Category? dbCategory = _db.Categories.Find(categoryId);
        //    if (dbCategory == null)
        //        return RedirectToAction("Index");

        //    dbCategory.Name = updatedCategory.Name;
        //    dbCategory.DisplayOrder = updatedCategory.DisplayOrder;
        //    dbCategory.UpdatedAt = DateTime.Now;
        //    _db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        [HttpPost("/categories/{categoryId:int}/edit")]
        [ValidateAntiForgeryToken]
        public IActionResult Update(Category updatedCategory)
        {
            if (!ModelState.IsValid || updatedCategory.CategoryId == 0)
                return View("Edit", updatedCategory);


            updatedCategory.UpdatedAt = DateTime.Now;
            _db.Update(updatedCategory);
            _db.Save();
            TempData["Success"] = "Category updated successfully";
            return RedirectToAction("Index");
        }

        [HttpGet("/categories/{categoryId:int}/delete")]
        public IActionResult Delete(int categoryId)
        {
            Category? dbCategory = _db.GetFirstOrDefault(c => c.CategoryId == categoryId);
            if (dbCategory == null)
                RedirectToAction("Index");
            return View(dbCategory);
        }

        [HttpPost("/categories/{categoryId:int}/delete"), ActionName("Delete")]
        [AutoValidateAntiforgeryToken]
        public IActionResult DeletePOST(int categoryId)
        {
            Category? dbCategory = _db.GetFirstOrDefault(c => c.CategoryId == categoryId);
            if (dbCategory != null)
            {
                _db.Remove(dbCategory);
                _db.Save();
                TempData["Success"] = "Category deleted successfully";
            }
            return RedirectToAction("Index");
        }
    }
}
