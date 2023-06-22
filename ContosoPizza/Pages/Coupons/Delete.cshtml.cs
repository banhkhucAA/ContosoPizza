using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ContosoPizza.Data;
using ContosoPizza.Models.Generated;

namespace ContosoPizza.Pages.Coupons
{
    public class DeleteModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;

        public DeleteModel(ContosoPizza.Data.ContosoPizzaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Coupon Coupon { get; set; } = default!;

        [BindProperty]
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.Coupons == null)
            {
                return NotFound();
            }

            var coupon = await _context.Coupons.FirstOrDefaultAsync(m => m.Id == id);

            if (coupon == null)
            {
                return NotFound();
            }
            else 
            {
                Coupon = coupon;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null || _context.Coupons == null)
            {
                return NotFound();
            }
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon != null)
            {
                var used_coupon_in_active_orders = await _context.Orders.Include(ors => ors.OrderStatus).Where(or => or.CouponId == coupon.Id).ToListAsync();
                if (used_coupon_in_active_orders.Count()!=0)
                {
                    foreach(var item in used_coupon_in_active_orders)
                    {
                        Console.WriteLine(item.Id);
                        if (item.OrderStatus.IsActive==true)
                        {
                            ErrorMessage = "Can't delete because this coupon is used in active orders. Example with the order placed at: " + item.OrderPlacedAt;
                            Console.WriteLine(ErrorMessage);
                            return await OnGetAsync(id);
                        }
                    }
                }
                else 
                {
                    Coupon = coupon;
                    _context.Coupons.Remove(Coupon);
                    await _context.SaveChangesAsync();
                }
            }               
            return RedirectToPage("./Index");
        }
    }
}
