using BookHeap.DataAccess.Repository.IRepository;
using BookHeap.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookHeap.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Product product)
        {
            Product dbProduct = _db.Products.FirstOrDefault(p => p.Id == product.Id);
            if (dbProduct != null)
            {
                dbProduct.Title = product.Title;
                dbProduct.Description = product.Description;
                dbProduct.ISBN = product.ISBN;
                dbProduct.Author = product.Author;
                dbProduct.ListPrice = product.ListPrice;
                dbProduct.Price = product.Price;
                dbProduct.Price50 = product.Price50;
                dbProduct.Price100 = product.Price100;
                dbProduct.UpdatedAt = DateTime.Now;
                if (dbProduct.ImageUrl != null)
                    dbProduct.ImageUrl = product.ImageUrl;
            }
        }
    }
}
