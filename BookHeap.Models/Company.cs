using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookHeap.Models;

public class Company
{
    [Key]
    public int CompanyId { get; set; }

    [Required]
    public string Name { get; set; }

    [Display(Name = "Street Address")]
    public string? StreetAddress { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    [Display(Name = "Postal Code")]
    public string? PostalCode { get; set; }

    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
