using BookHeap.DataAccess;
using BookHeap.DataAccess.Repository.IRepository;
using BookHeap.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;

namespace BookHeapWeb.Areas.Admin.Controllers;
[Area("Admin")]

public class ProductsController : Controller
{
    private readonly IUnitOfWork _db;

    public ProductsController(IUnitOfWork db)
    {
        _db = db;
    }
    [HttpGet]
    public IActionResult Index()
    {
        IEnumerable<CoverType> allCoverTypes = _db.CoverTypes.GetAll();
        return View(allCoverTypes);
    }

    [HttpGet]
    public IActionResult Upsert(int productId)
    {
        Product product = new();
        IEnumerable<SelectListItem> CategoryList = _db.Categories.GetAll().Select(
            c => new SelectListItem
            {
                Text = c.Name,
                Value = c.CategoryId.ToString()
            });
        IEnumerable<SelectListItem> CoverTypeList = _db.CoverTypes.GetAll().Select(
            c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            });
        if (productId == null || productId == 0)
        {
            // create product
            ViewBag.Categories = CategoryList;
            ViewBag.CoverTypes = CoverTypeList;
            return View(product);
        }
        else
        {
            // update product
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Upsert(CoverType updatedCoverType)
    {
        if (!ModelState.IsValid || updatedCoverType.Id == 0)
            return View("Edit", updatedCoverType);

        _db.CoverTypes.Update(updatedCoverType);
        _db.Save();
        TempData["Success"] = "Cover Type updated successfully";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Delete(int coverTypeId)
    {
        CoverType? dbCoverType = _db.CoverTypes.GetFirstOrDefault(c => c.Id == coverTypeId);
        if (dbCoverType == null)
            RedirectToAction("Index");
        return View(dbCoverType);
    }

    [HttpPost, ActionName("Delete")]
    [AutoValidateAntiforgeryToken]
    public IActionResult DeletePOST(int coverTypeId)
    {
        CoverType? dbCoverType = _db.CoverTypes.GetFirstOrDefault(c => c.Id == coverTypeId);
        if (dbCoverType != null)
        {
            _db.CoverTypes.Remove(dbCoverType);
            _db.Save();
            TempData["Success"] = "Cover Type deleted successfully";
        }
        return RedirectToAction("Index");
    }
}
