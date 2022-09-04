using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookHeap.Models;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Title { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    public string ISBN { get; set; }

    [Required]
    public string Author { get; set; }

    [Required]
    [Range(1, 10000)]
    public double ListPrice { get; set; }

    [Required]
    [Range(1, 10000)]
    public double Price { get; set; }

    [Required]
    [Range(1, 10000)]
    public double Price50 { get; set; }

    [Required]
    [Range(1, 10000)]
    public double Price100 { get; set; }

    [ValidateNever]
    public string ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [Required]
    public int CategoryId { get; set; }
    [ValidateNever]
    public Category Category { get; set; }

    [Required]
    public int CoverTypeId { get; set; }
    [ValidateNever]
    public CoverType CoverType { get; set; }
}
