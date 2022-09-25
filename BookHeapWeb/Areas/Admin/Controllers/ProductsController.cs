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
        return View();
    }

    [HttpGet]
    public IActionResult Upsert(int? productId)
    {
        ProductViewModel productVM = new()
        {
            Product = new(),
            // Used to populate HTML select tag with existing categories/cover types as options
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

        //if (productId == null || productId == 0)
        //{
        //    //ViewBag.Categories = CategoryList;
        //    //ViewBag.CoverTypes = CoverTypeList;
        //    return View(productVM);
        //}
        //else
        //{
        //    productVM.Product = _db.Products.GetFirstOrDefault(p => p.Id == productId);
        //    // update product
        //}
        if (productId > 0 || productId != null)
            // Populates the productVM.product with product info that has given Id
            productVM.Product = _db.Products.GetFirstOrDefault(p => p.Id == productId);

        return View(productVM);
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

                // Checks if ImageUrl already exists on the product and deletes it if so
                if (productVM.Product.ImageUrl != null)
                {
                    var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                // Opens new FileStream with the image upload file path and copies the given image file to that stream
                using (var fileStream = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
                // Update Product ImageURL with created file path
                productVM.Product.ImageUrl = @"\images\products\" + fileName + extension;
            }
        }
        // Creates product if it's new, updates product if it already exists
        if (productVM.Product.Id == 0)
        {
            _db.Products.Add(productVM.Product);
            TempData["Success"] = "Product created successfully";
        }
        else
        {
            _db.Products.Update(productVM.Product);
            TempData["Success"] = "Product updated successfully";
        }
        _db.Save();
        return RedirectToAction("Index");
    }

    #region API CALLS
    [HttpGet]
    public IActionResult GetAll()
    {
        // Get all products and include Category and CoverType
        var productList = _db.Products.GetAll(null, "Category,CoverType");
        return Json(new { data = productList });
    }

    [HttpDelete]
    public IActionResult Delete(int productId)
    {
        Product dbProduct = _db.Products.GetFirstOrDefault(p => p.Id == productId);

        if (dbProduct == null)
            return Json(new { success = false, message = "Error while deleting" });

        // Remove product image if it exists
        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, dbProduct.ImageUrl.TrimStart('\\'));
        if (System.IO.File.Exists(oldImagePath))
            System.IO.File.Delete(oldImagePath);

        _db.Products.Remove(dbProduct);
        _db.Save();
        return Json(new { success = true, message = "Product deleted successfully" });
    }
    #endregion
}
