using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ContosoPizza.Data;
using ContosoPizza.Models.Generated;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ContosoPizza.Pages.Coupons
{
    public class IndexModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;

        public IndexModel(ContosoPizza.Data.ContosoPizzaContext context)
        {
            _context = context;
        }
        [BindProperty(SupportsGet = true)]
        public string Expire { get; set; }
        public IList<Coupon> Coupon { get;set; } = default!;

        public async Task OnGetAsync()  
        {
            if (_context.Coupons != null)
            {
                if(HttpContext.Session.GetString("UserRole")=="Admin")
                {                    
                    if(!string.IsNullOrEmpty(Expire)) 
                    {
                        if(Expire=="Expired") Coupon = await _context.Coupons.Where(c=>c.ExpireDate<DateTime.UtcNow).ToListAsync();
                        else if(Expire=="Available") Coupon = await _context.Coupons.Where(c => c.ExpireDate > DateTime.UtcNow || c.ExpireDate == null).ToListAsync();
                    }
                    else Coupon = await _context.Coupons.ToListAsync();
                }
                else    
                Coupon = await _context.Coupons.Where(c=>c.ExpireDate>DateTime.UtcNow||c.ExpireDate==null).ToListAsync();
            }
        }
    }
}
