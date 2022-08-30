using BookHeap.DataAccess;
using BookHeap.DataAccess.Repository.IRepository;
using BookHeap.Models;
using Microsoft.AspNetCore.Mvc;
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
    public IActionResult New()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CoverType newCoverType)
    {
        if (!ModelState.IsValid)
            return View("New");

        _db.CoverTypes.Add(newCoverType);
        _db.Save();
        TempData["Success"] = "Cover Type created successfully";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Edit(int coverTypeId)
    {
        CoverType? dbCoverType = _db.CoverTypes.GetFirstOrDefault(c => c.Id == coverTypeId);
        if (dbCoverType == null)
            return RedirectToAction("Index");
        return View(dbCoverType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Update(CoverType updatedCoverType)
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
