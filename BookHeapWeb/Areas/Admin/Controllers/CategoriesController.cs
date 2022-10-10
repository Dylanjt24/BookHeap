using BookHeap.DataAccess;
using BookHeap.DataAccess.Repository.IRepository;
using BookHeap.Models;
using BookHeap.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookHeapWeb.Areas.Admin.Controllers;
[Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]

public class CategoriesController : Controller
{
    private readonly IUnitOfWork _db;

    public CategoriesController(IUnitOfWork db)
    {
        _db = db;
    }
    [HttpGet]
    public IActionResult Index()
    {
        IEnumerable<Category> allCategories = _db.Categories.GetAll();
        return View(allCategories);
    }
    [HttpGet]
    public IActionResult New()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Category newCategory)
    {
        if (!ModelState.IsValid)
            return View("New");

        _db.Categories.Add(newCategory);
        _db.Save();
        TempData["Success"] = "Category created successfully";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Edit(int categoryId)
    {
        Category? dbCategory = _db.Categories.GetFirstOrDefault(c => c.CategoryId == categoryId);
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Update(Category updatedCategory)
    {
        if (!ModelState.IsValid || updatedCategory.CategoryId == 0)
            return View("Edit", updatedCategory);

        _db.Categories.Update(updatedCategory);
        _db.Save();
        TempData["Success"] = "Category updated successfully";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Delete(int categoryId)
    {
        Category? dbCategory = _db.Categories.GetFirstOrDefault(c => c.CategoryId == categoryId);
        if (dbCategory == null)
            RedirectToAction("Index");
        return View(dbCategory);
    }

    [HttpPost, ActionName("Delete")]
    [AutoValidateAntiforgeryToken]
    public IActionResult DeletePOST(int categoryId)
    {
        Category? dbCategory = _db.Categories.GetFirstOrDefault(c => c.CategoryId == categoryId);
        if (dbCategory != null)
        {
            _db.Categories.Remove(dbCategory);
            _db.Save();
            TempData["Success"] = "Category deleted successfully";
        }
        return RedirectToAction("Index");
    }
}
