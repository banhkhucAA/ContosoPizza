using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ContosoPizza.Data;
using ContosoPizza.Models.Generated;

namespace ContosoPizza.Pages.Categories
{
    public class DeleteModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;

        public DeleteModel(ContosoPizza.Data.ContosoPizzaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Category Category { get; set; } = default!;
        [BindProperty]
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FirstOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return NotFound();
            }
            else 
            {
                Category = category;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }
            var category = await _context.Categories.FindAsync(id);

            if (category != null)
            {
                var find_ProductsId = _context.Products.Where(p => p.CategoryId == id).Select(p => p.Id).ToList();
                foreach (var item in find_ProductsId)
                {
                    var find_ordersId = _context.OrderDetails.Where(or => or.ProductId == item).Select(p => p.OrderId).ToList();
                    foreach (var ordersid in find_ordersId)
                    {
                        var find_orders = _context.Orders.Where(or => or.Id == ordersid && or.OrderStatus.IsActive==true).ToList();
                        if (find_orders.Any())
                        {
                            ErrorMessage = "Can't delete. This category has some products which have been used in active orders";
                            return await OnGetAsync(id);
                        }
                    }    

                }
                Category = category;
                _context.Categories.Remove(Category);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
