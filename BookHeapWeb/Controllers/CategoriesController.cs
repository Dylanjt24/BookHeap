﻿using BookHeapWeb.Data;
using BookHeapWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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

        [HttpPost("/categories/{categoryId:int}/update")]
        [ValidateAntiForgeryToken]
        public IActionResult Update(Category updatedCategory, int categoryId)
        {
            if (!ModelState.IsValid)
                return View("Edit", updatedCategory);

            Category? dbCategory = _db.Categories.Find(categoryId);
            if (dbCategory == null)
                return RedirectToAction("Index");

            dbCategory.Name = updatedCategory.Name;
            dbCategory.DisplayOrder = updatedCategory.DisplayOrder;
            dbCategory.UpdatedAt = DateTime.Now;
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
