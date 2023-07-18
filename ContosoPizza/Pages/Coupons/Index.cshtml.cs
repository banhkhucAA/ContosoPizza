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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
        public int myPage { get; set; }
        public async Task OnGetAsync()  
        {
            int pageSize = 4;

            if (Request.Query.ContainsKey("Page"))
            {
                int.TryParse(Request.Query["Page"], out int page);
                myPage = Math.Max(page, 1); // Đảm bảo giá trị trang không nhỏ hơn 1
            }
            else
            {
                myPage = 1; // Trang mặc định là 1
            }

            if (_context.Coupons != null)
            {
                int offset = Math.Max((myPage - 1) * pageSize, 0);
                
                if (HttpContext.Session.GetString("UserRole")=="Admin"|| (HttpContext.Session.GetString("UserRole") == "Employee"))
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

                Coupon = Coupon
                    .Skip(offset)
                    .Take(pageSize)
                    .ToList();
                Console.WriteLine(Coupon);
            }
        }
    }
}
