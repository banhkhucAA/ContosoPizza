using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ContosoPizza.Data;
using ContosoPizza.Models.Generated;
using Microsoft.EntityFrameworkCore;

namespace ContosoPizza.Pages.Coupons
{
    public class CreateModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;

        public CreateModel(ContosoPizza.Data.ContosoPizzaContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Coupon Coupon { get; set; } = default!;
        [BindProperty]
        public string ErrorMessage { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
          if ( _context.Coupons == null || Coupon == null)
            {
                return Page();
            }
            var existCouponName = await _context.Coupons.Where(c => c.CouponCode == Coupon.CouponCode).FirstOrDefaultAsync();
            if(existCouponName!=null)
            {
                ErrorMessage = "This Coupon Code: "+ Coupon.CouponCode +" has already existed";
                return Page();
            }    

            _context.Coupons.Add(Coupon);   
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
