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
    public class DetailsModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;

        public DetailsModel(ContosoPizza.Data.ContosoPizzaContext context)
        {
            _context = context;
        }

      public Coupon Coupon { get; set; } = default!; 

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
    }
}
