using BookHeap.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookHeap.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Categories = new CategoryRepository(_db);
        }
        public ICategoryRepository Categories { get; private set; }
        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
