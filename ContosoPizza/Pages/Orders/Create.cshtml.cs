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
using NuGet.Packaging;

namespace ContosoPizza.Pages.Orders
{
    public class CreateModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateModel(ContosoPizza.Data.ContosoPizzaContext context,IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        [BindProperty]// cứ http request thì là cần
        public Order Order { get; set; } = default!;
        [BindProperty]
        public Customer Customer { get; set; }
        public string ErrorMessage { get; set; }


        public async Task<IActionResult> OnGet()
        {
            var orderStatuses = _context.OrderStatuses.ToList();

            // Tìm và lưu trữ bản ghi có Id là 6
            var orderStatusId6 = orderStatuses.FirstOrDefault(status => status.Id == 6);

            // Nếu tìm thấy bản ghi có Id là 6
            if (orderStatusId6 != null)
            {
                // Xóa bản ghi có Id là 6 khỏi danh sách OrderStatuses
                orderStatuses.Remove(orderStatusId6);

                // Thêm bản ghi có Id là 6 vào đầu danh sách OrderStatuses
                orderStatuses.Insert(0, orderStatusId6);
            }

            // Đặt SelectList với danh sách đã chỉnh sửa
            ViewData["OrderStatusId"] = new SelectList(orderStatuses, "Id", "StatusName");
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Id");
            var coupons = await _context.Coupons.ToListAsync();
            var selectCouponItems = new List<SelectListItem>()
            {
                new SelectListItem{Value=null,Text="None"}
            };
            selectCouponItems.AddRange(coupons.Select(cp => new SelectListItem { Value = cp.Id.ToString(), Text = cp.CouponCode }));
            ViewData["CouponId"] = selectCouponItems;
            ViewData["DeliveryMethodId"] = new SelectList(_context.DeliveryMethods, "Id", "Method");       
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods, "Id", "Method");
            var employeeData = _context.Employees
                                    .Where(emp => emp.Id == _httpContextAccessor.HttpContext.Session.GetInt32("UserId"))
                                    .Select(emp => new EmployeeData
                                    {
                                        Id = emp.Id,
                                        FullName = emp.FirstName + " " + emp.LastName
                                    })
                                    .FirstOrDefault();
            ViewData["EmployeeId"] = new SelectList(new List<EmployeeData> { employeeData }, "Id", "FullName");

            return Page();
        }


        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnCreateAsync()
        {
          if (_context.Orders == null || Order == null)
            {
                return Page();
            }
            if (Order.IsCustomerMember==false && Order.CustomerId == null) Order.CouponId = null;
            else 
            {
                var already_used_Coupon = _context.Orders.Where(or => or.CouponId == Order.CouponId && or.CustomerId==Order.CustomerId &&or.Id!=Order.Id && Order.CouponId!=null).FirstOrDefault();
                if(already_used_Coupon!=null)
                {
                    ErrorMessage = "This coupon has already been used, please try another one.";
                    Console.WriteLine("couponId: " + Order.CouponId + " customerId:" + Order.CustomerId);
                    return await OnGet();
                }
                var expire_coupon_id = _context.Coupons.Where(cou=> cou.ExpireDate < DateTime.UtcNow).Select(cou=>cou.Id).ToList();
                foreach(var item in expire_coupon_id)
                {
                    if(item==Order.CouponId)
                    {
                        ErrorMessage = "This coupon has expired or no longer existed";
                        return await OnGet();
                    }    
                }    
            } 
                
            _context.Orders.Add(Order);
            await _context.SaveChangesAsync();
            return Redirect($"/OrderDetails/Create?orderId={Order.Id}");
        }

        public async Task<IActionResult> OnFindAsync()
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == Customer.Email);
            if (customer==null)
            {
                return NotFound("This customer doesn' exist");
            }

            Customer = customer;
            return await OnGet();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!string.IsNullOrEmpty(Request.Form["createButton"]))
            {
                if (_context.Orders == null || Order == null)
                {
                    return NotFound("There are some problems with your Order Model");
                }

                return await OnCreateAsync();
            }
            else if (!string.IsNullOrEmpty(Request.Form["findButton"]))
            {
                if(Customer.Phone=="") { return NotFound("This customer doesn' exist"); };
                return await OnFindAsync();
            }
            else
            {
                Console.WriteLine("Khong co nut nao duoc bam");
                return Page();
            }
        }
    }
}
