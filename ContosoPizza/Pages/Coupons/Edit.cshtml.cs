using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContosoPizza.Data;
using ContosoPizza.Models.Generated;

namespace ContosoPizza.Pages.Coupons
{
    public class EditModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;

        public EditModel(ContosoPizza.Data.ContosoPizzaContext context)
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

            var coupon =  await _context.Coupons.FirstOrDefaultAsync(m => m.Id == id);
            if (coupon == null)
            {
                return NotFound();
            }
            Coupon = coupon;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            var existCouponName = await _context.Coupons.Where(c => c.CouponCode == Coupon.CouponCode&&c.Id!=Coupon.Id).FirstOrDefaultAsync();
            if (existCouponName != null)
            {
                ErrorMessage = "This Coupon Code: " + Coupon.CouponCode + " has already existed";
                return Page();
            }
            _context.Attach(Coupon).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CouponExists(Coupon.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool CouponExists(int id)
        {
          return (_context.Coupons?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
