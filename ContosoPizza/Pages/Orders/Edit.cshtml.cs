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

namespace ContosoPizza.Pages.Orders
{
    public class EditModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;
        private readonly IHttpContextAccessor _httpContext;

        public EditModel(ContosoPizza.Data.ContosoPizzaContext context, IHttpContextAccessor httpContext)
        {
            _context = context;
            _httpContext = httpContext;
        }
        [BindProperty]
        public string ErrorMessage{get;set;}
        [BindProperty]
        public Order Order { get; set; } = default!;

        [BindProperty]
        public OrderDetail OrderDetail { get; set; }
        [BindProperty]
        public Customer Customer { get; set; }
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order =  await _context.Orders.FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            Order = order;

            var coupons = await _context.Coupons.ToListAsync();
            var selectCouponItems = new List<SelectListItem>()
            {
                new SelectListItem{Value=null,Text="None"}
            };
            selectCouponItems.AddRange(coupons.Select(cp => new SelectListItem { Value = cp.Id.ToString(), Text = cp.CouponCode }));
            ViewData["CouponId"] = selectCouponItems;

            var customers = await _context.Customers.ToListAsync();
            var selectCustomerItems = new List<SelectListItem>()
            {
                new SelectListItem{Value=null,Text="None"}
            };
            selectCustomerItems.AddRange(customers.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Id.ToString() }));
            ViewData["CustomerId"] = selectCustomerItems;
            ViewData["DeliveryMethodId"] = new SelectList(_context.DeliveryMethods, "Id", "Method");
            ViewData["OrderStatusId"] = new SelectList(_context.OrderStatuses, "Id", "StatusName");
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods, "Id", "Method");
            var isManagerEmployee = _context.Employees.Any(e => e.Role == "Manager Employee" && e.Id == _httpContext.HttpContext.Session.GetInt32("UserId"));
            if (!isManagerEmployee)
            {
                var employeeData = GetCurrentEmployeeData();
                ViewData["EmployeeId"] = new SelectList(new List<EmployeeData> { employeeData }, "Id", "FullName");
            }else if(isManagerEmployee)
            {
                var EmployeeRecentData = _context.Employees.Where(em => em.Id == Order.EmployeeId)
                    .Select(emp => new EmployeeData
                    {
                        Id = emp.Id,
                        FullName = emp.FirstName + " " + emp.LastName
                    }).FirstOrDefault();

                if(EmployeeRecentData != null)
                ViewData["EmployeeId"] = new SelectList(new List<EmployeeData> { EmployeeRecentData }, "Id", "FullName");
                else
                {
                    var employeeData = GetCurrentEmployeeData();
                    ViewData["EmployeeId"] = new SelectList(new List<EmployeeData> { employeeData }, "Id", "FullName");
                }    
            }  
            return Page();
        }

        private EmployeeData GetCurrentEmployeeData()
        {
            var employeeData = _context.Employees
                                        .Where(emp => emp.Id == _httpContext.HttpContext.Session.GetInt32("UserId"))
                                        .Select(emp => new EmployeeData
                                        {
                                            Id = emp.Id,
                                            FullName = emp.FirstName + " " + emp.LastName
                                        })
                                        .FirstOrDefault();
            return employeeData;
        }    

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!string.IsNullOrEmpty(Request.Form["saveButton"]))
            {
                Order.BillPrice = 0;
                if (Order.CustomerId == null) Order.CouponId = null;
                else
                {
                    var already_used_Coupon = _context.Orders.Where(or => or.CouponId == Order.CouponId && or.CustomerId == Order.CustomerId && or.Id != Order.Id && Order.CouponId != null).FirstOrDefault();
                    if (already_used_Coupon != null)
                    {
                        ErrorMessage = "This coupon has already been used, please try another one.";
                        Console.WriteLine("couponId: " + Order.CouponId + " customerId:" + Order.CustomerId);
                        return await OnGetAsync(Order.Id);
                    }
                    var expire_coupon_id = _context.Coupons.Where(cou => cou.ExpireDate < DateTime.Now).Select(cou => cou.Id).ToList();
                    foreach (var item in expire_coupon_id)
                    {
                        if (item == Order.CouponId)
                        {
                            ErrorMessage = "This coupon has expired or no longer existed";
                            return await OnGetAsync(Order.Id);
                        }
                    }
                }
                var orders_detail = _context.OrderDetails.Where(p => p.OrderId == Order.Id);
                var DeliveryPrice = (float)_context.DeliveryMethods.Where(p => p.Id == Order.DeliveryMethodId).Select(p => p.Price).FirstOrDefault();
                var PaymentPrice = (float)_context.PaymentMethods.Where(p => p.Id == Order.PaymentMethodId).Select(p => p.Price).FirstOrDefault();
                float discountPrice = 0;
                if (Order.CouponId != null)
                {
                    discountPrice = (float)_context.Coupons.Where(p => p.Id == Order.CouponId).Select(p => p.DiscountAmount).FirstOrDefault();
                }
                if (orders_detail != null)
                {
                    foreach (var item in orders_detail)
                    {

                        Order.BillPrice = (float)(Order.BillPrice + item.TotalPrice);
                    }
                }

                Order.BillPrice = (float?)Math.Round((float)(Order.BillPrice + DeliveryPrice + PaymentPrice - (float)(discountPrice * Order.BillPrice / 100)), 2);
                Order.BillPrice = (float)Math.Round((float)Order.BillPrice, 2);

                var findOrderStatusId = await _context.OrderStatuses.FirstOrDefaultAsync(or => or.Id == Order.OrderStatusId);
                var findOrder = await _context.Orders.FirstOrDefaultAsync(or => or.Id == Order.Id);

                if (findOrderStatusId != null && findOrder != null)
                {
                    if (findOrderStatusId.StatusName == "Making" && findOrder.OrderStatus.StatusName!="Making")
                    {
                        Order.UpdatedMakingAt = DateTime.Now;
                    }
                    else if (findOrderStatusId.StatusName == "Delivering" && findOrder.OrderStatus.StatusName != "Delivering")
                    {
                        Order.UpdatedDeliveringAt = DateTime.Now;
                    }
                    else if (findOrderStatusId.StatusName == "Delivered" && findOrder.OrderStatus.StatusName != "Delivered")
                    {
                        Order.UpdatedDeliveredAt = DateTime.Now;
                    }
                    else if (findOrderStatusId.StatusName == "Canceled" && findOrder.OrderStatus.StatusName != "Canceled")
                    {
                        Order.UpdatedCancelledAt = DateTime.Now;
                    }
                    else if (findOrderStatusId.StatusName == "Returned" && findOrder.OrderStatus.StatusName != "Returned")
                    {
                        Order.UpdatedReturnedAt = DateTime.Now;
                    }
                    else if (findOrderStatusId.StatusName == "Waiting" && findOrder.OrderStatus.StatusName != "Waiting")
                    {
                        Order.UpdatedWaitingAt = DateTime.Now;
                    }
                    else if (findOrderStatusId.StatusName == "DeActive" && findOrder.OrderStatus.StatusName != "DeActive")
                    {
                        Order.UpdatedDeActiveAt = DateTime.Now;
                    }
                    _context.Entry(findOrder).State = EntityState.Detached; 
                    _context.Orders.Update(Order);
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(Order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Redirect($"/OrderDetails/Create?orderId={Order.Id}");
            }else if(!string.IsNullOrEmpty(Request.Form["findButton"]))
            {
                var cus = _context.Customers.Where(cus => cus.Email == Customer.Email).FirstOrDefault();
                if (cus != null)
                    Customer = cus;
                else ErrorMessage = "This email doesn't exist";
                return await OnGetAsync(Order.Id);
            }else
            {
                ErrorMessage = "No buttons are hit";
                return await OnGetAsync(Order.Id);
            }    
        }

        private bool OrderExists(int id)
        {
          return (_context.Orders?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
