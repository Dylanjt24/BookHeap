using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookHeap.Models.ViewModels;

public class ShoppingCartVM
{
    public IEnumerable<ShoppingCart> CartList { get; set; }
    public double TotalPrice { get; set; }
}
