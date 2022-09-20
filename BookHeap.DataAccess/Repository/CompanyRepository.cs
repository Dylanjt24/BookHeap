using BookHeap.DataAccess.Repository.IRepository;
using BookHeap.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookHeap.DataAccess.Repository;
public class CompanyRepository : Repository<Company>, ICompanyRepository
{
    private ApplicationDbContext _db;
    public CompanyRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public void Update(Company company)
    {
        company.UpdatedAt = DateTime.Now;
        _db.Companies.Update(company);
    }
}