using BookHeap.DataAccess;
using BookHeap.DataAccess.Repository.IRepository;
using BookHeap.Models;
using BookHeap.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;

namespace BookHeapWeb.Areas.Admin.Controllers;
[Area("Admin")]

public class ProductsController : Controller
{
    private readonly IUnitOfWork _db;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProductsController(IUnitOfWork db, IWebHostEnvironment webHostEnvironment)
    {
        _db = db;
        _webHostEnvironment = webHostEnvironment;
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
        ProductViewModel productViewModel = new()
        {
            Product = new(),
            CategoryList = _db.Categories.GetAll().Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.CategoryId.ToString()
            }),
            CoverTypeList = _db.CoverTypes.GetAll().Select(ct => new SelectListItem
            {
                Text = ct.Name,
                Value = ct.Id.ToString()
            })
        };

        if (productId == null || productId == 0)
        {
            //ViewBag.Categories = CategoryList;
            //ViewBag.CoverTypes = CoverTypeList;
            return View(productViewModel);
        }
        else
        {
            // update product
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Upsert(ProductViewModel productVM, IFormFile? file)
    {
        if (ModelState.IsValid)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString();
                var uploads = Path.Combine(wwwRootPath, @"images\products");
                var extension = Path.GetExtension(file.FileName);

                using (var fileStream = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
                productVM.Product.ImageUrl = @"\images\products\" + fileName + extension;
            }
        }

        _db.Products.Add(productVM.Product);
        _db.Save();
        TempData["Success"] = "Product created successfully";
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
