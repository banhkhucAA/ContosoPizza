using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ContosoPizza.Data;
using ContosoPizza.Models.Generated;

namespace ContosoPizza.Pages.Products
{
    public class DeleteModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;

        public DeleteModel(ContosoPizza.Data.ContosoPizzaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Product Product { get; set; } = default!;
        [BindProperty]
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }
            else 
            {
                Product = product;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }
            var product = await _context.Products.FindAsync(id);

            if(product!=null)
            {
                var find_OrdersId =  _context.OrderDetails.Where(ord=>ord.ProductId==id && ord.Order.OrderStatus.IsActive == true).ToList();
                if (find_OrdersId.Count != 0)
                {
                    foreach (var item in find_OrdersId)
                    {
                        var find_order_status = _context.Orders.Include(ors => ors.OrderStatus).Where(or => or.Id == item.OrderId);
                        if (find_order_status.Any())
                        {
                            ErrorMessage = "Can't delete. This product has been used in active orders";
                            return await OnGetAsync(id);
                        }
                    }
                }
                else
                {
                    Product = product;
                    _context.Products.Remove(Product);
                    var findDeleteOrder = _context.OrderDetails.Where(ord => ord.ProductId == id).ToList();
                    if(findDeleteOrder.Count != 0) 
                    {
                        foreach(var item in findDeleteOrder)
                        {
                            var deleteOrder = _context.Orders.FirstOrDefault(or=>or.Id==item.OrderId);
                            if(deleteOrder != null)
                            _context.Orders.Remove(deleteOrder);
                        }    
                    }
                    await _context.SaveChangesAsync();
                }
            }    

            return RedirectToPage("./Index");
        }
    }
}
