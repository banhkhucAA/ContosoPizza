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

            if (category == null)
            {
                return NotFound();
            }

            var find_ProductsId = _context.Products.Where(p => p.CategoryId == id).Select(p => p.Id).ToList();
            foreach (var productId in find_ProductsId)
            {
                var find_orders = _context.OrderDetails
                    .Where(od => od.ProductId == productId && od.Order.OrderStatus.IsActive == true)
                    .Select(od => od.Order)
                    .ToList();

                if (find_orders.Any())
                {
                    ErrorMessage = "Can't delete. This category has some products which have been used in active orders.";
                    return await OnGetAsync(id);
                }
            }

            // If there are no active orders associated with the products in this category, delete the orders and the category.
            foreach (var productId in find_ProductsId)
            {
                var orderDetails = await _context.OrderDetails
                    .Where(od => od.ProductId == productId)
                    .ToListAsync();

                foreach (var orderDetail in orderDetails)
                {
                    var order = await _context.Orders.FindAsync(orderDetail.OrderId);
                    if (order != null)
                    {
                        _context.Orders.Remove(order);
                    }
                }

                var product = await _context.Products.FindAsync(productId);
                if (product != null)
                {
                    _context.Products.Remove(product);
                }
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

    }
}
