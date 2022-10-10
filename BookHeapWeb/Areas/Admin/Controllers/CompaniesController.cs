using BookHeap.DataAccess.Repository.IRepository;
using BookHeap.Models;
using BookHeap.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.Design;

namespace BookHeapWeb.Areas.Admin.Controllers;
[Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]
public class CompaniesController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public CompaniesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Upsert(int? companyId)
    {
        Company company = new();

        if (companyId != null || companyId > 0)
            company = _unitOfWork.Companies.GetFirstOrDefault(c => c.CompanyId == companyId);

        return View(company);
    }

    [HttpPost]
    public IActionResult Upsert(Company updatedCompany)
    {
        if (ModelState.IsValid)
        {
            if (updatedCompany.CompanyId == 0)
            {
                _unitOfWork.Companies.Add(updatedCompany);
                TempData["Success"] = "Company created successfully";
            }
            else
            {
                _unitOfWork.Companies.Update(updatedCompany);
                TempData["Success"] = "Company updated successfully";
            }
            _unitOfWork.Save();
        }
        return RedirectToAction("Index");
    }

    #region API CALLS
    [HttpGet]
    public IActionResult GetAll()
    {
        var companyList = _unitOfWork.Companies.GetAll();
        return Json(new { data = companyList });
    }

    [HttpDelete]
    public IActionResult Delete(int companyId)
    {
        Company dbCompany = _unitOfWork.Companies.GetFirstOrDefault(c => c.CompanyId == companyId);

        if (dbCompany == null)
            return Json(new { success = false, message = "Error while deleting company" });

        _unitOfWork.Companies.Remove(dbCompany);
        _unitOfWork.Save();
        return Json(new { success = true, message = "Company deleted successfully" });
    }
    #endregion
}
