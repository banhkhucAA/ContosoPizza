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

                if (HttpContext.Session.GetString("UserRole") == "Admin" || HttpContext.Session.GetString("UserRole") == "Employee")
                {
                    if (!string.IsNullOrEmpty(Expire))
                    {
                        if (Expire == "Expired") Coupon = await _context.Coupons.Where(c => c.ExpireDate < DateTime.Now).ToListAsync();
                        else if (Expire == "Available") Coupon = await _context.Coupons.Where(c => c.ExpireDate > DateTime.Now || c.ExpireDate == null).ToListAsync();
                        else Coupon = await _context.Coupons.ToListAsync();
                    }
                    else Coupon = await _context.Coupons.ToListAsync();
                }
                else
                {
                    var used_coupons_in_orders = await _context.Orders
                        .Where(o => o.CustomerId == HttpContext.Session.GetInt32("UserId"))
                        .Where(o => o.CouponId != null)
                        .Select(o => o.CouponId)
                        .ToListAsync();

                    Coupon = await _context.Coupons
                            .Where(c => c.ExpireDate > DateTime.Now || c.ExpireDate == null)
                            .Where(c => !used_coupons_in_orders.Contains(c.Id))
                            .ToListAsync();
                }

                Coupon = Coupon
                    .Skip(offset)
                    .Take(pageSize)
                    .ToList();
                Console.WriteLine(Coupon);
            }
        }
    }
}
