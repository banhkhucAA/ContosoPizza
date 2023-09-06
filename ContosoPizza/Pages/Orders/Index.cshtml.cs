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
using System.Drawing.Printing;
using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.Asn1;

namespace ContosoPizza.Pages.Orders
{
    public class IndexModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public IndexModel(ContosoPizza.Data.ContosoPizzaContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public IList<Order> Order { get;set; } = default!;
        [BindProperty]
        public string ErrorMessage { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? searchOrderStatus { get; set; }
        public SelectList? searchOrderStatuses { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? searchDeliveryMethod { get; set; }
        public SelectList? searchDeliveryMethods { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? searchPaymentMethod { get; set; }
        public SelectList? searchPaymentMethods { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }
        public int Page { get; set; }
        public int pageSize = 5;
        public async Task<IActionResult> OnGetAsync()
        {
            if (_context.Orders != null && _httpContextAccessor.HttpContext.Session.GetString("UserRole")=="Employee")
            {
                var employee = _context.Employees.Any(e => e.Id == _httpContextAccessor.HttpContext.Session.GetInt32("UserId") && e.Role == "Manager Employee");
                if (employee) Order = await _context.Orders
                    .ToListAsync();
                else
                Order = await _context.Orders
                    .Where(o => o.EmployeeId == _httpContextAccessor.HttpContext.Session.GetInt32("UserId") || o.EmployeeId==null)
                    .ToListAsync();
            }
            else if(_context.Orders != null && _httpContextAccessor.HttpContext.Session.GetString("UserRole") == "Customer")
            {
                Order = await _context.Orders                    
                    .Where(o=>o.CustomerId== _httpContextAccessor.HttpContext.Session.GetInt32("UserId"))
                    .ToListAsync();
            }

            CheckOverTime(Order);
            await _context.SaveChangesAsync();

            if (searchOrderStatus==null)
            {
                Order = Order.Where(or => or.OrderStatus.IsActive == true).ToList();
            }else
            {
                if(searchOrderStatus!="All")
                {
                    Order = Order.Where(or => or.OrderStatus.StatusName == searchOrderStatus).ToList();
                }               
            }
            if (!string.IsNullOrEmpty(searchDeliveryMethod))
            {
                Order = Order.Where(or => or.DeliveryMethod.Method == searchDeliveryMethod).ToList();
            }
            if (!string.IsNullOrEmpty(searchPaymentMethod))
            {
                Order = Order.Where(or => or.PaymentMethod.Method == searchPaymentMethod).ToList();
            }
            if (FromDate.HasValue&&!ToDate.HasValue)
            {
                Order = Order.Where(or => or.OrderPlacedAt >= FromDate).ToList();
            }else if(!FromDate.HasValue && ToDate.HasValue)
            {
                Order = Order.Where(or => or.OrderPlacedAt <= ToDate).ToList();
            }else if(FromDate.HasValue && ToDate.HasValue)
            {
                Order = Order.Where(or => or.OrderPlacedAt <= ToDate && or.OrderPlacedAt >= FromDate).ToList();
            }
            
            
            await _context.SaveChangesAsync();
            var select_items = _context.OrderStatuses.Select(ors => ors.StatusName);
            searchOrderStatuses = new SelectList(select_items);

            var deliMethods = _context.DeliveryMethods.Select(dlv => dlv.Method);
            searchDeliveryMethods = new SelectList(deliMethods);

            var paymentMethods = _context.PaymentMethods.Select(pmt => pmt.Method);
            searchPaymentMethods = new SelectList(paymentMethods);

            if (Request.Query.ContainsKey("page"))
            {
                int.TryParse(Request.Query["page"], out int page);
                Page = Math.Max(page, 1); // Đảm bảo giá trị trang không nhỏ hơn 1
            }
            else
            {
                Page = 1; // Trang mặc định là 1
            }

            if (_context.Orders != null && _httpContextAccessor.HttpContext.Session.GetString("UserRole") == "Employee")
            {
                int offset = Math.Max((Page - 1) * pageSize, 0);
                Order = Order
                    .Skip(offset)
                    .Take(pageSize)
                    .ToList();
            }else 
            {
                int offset = Math.Max((Page - 1) * pageSize, 0);
                Order = Order
                    .Skip(offset)
                    .Take(2)
                    .ToList();
            }

            TempData["ErrorMessage"] = _httpContextAccessor.HttpContext.Session.GetString("ErrorMessage");
            _httpContextAccessor.HttpContext.Session.Remove("ErrorMessage");

            return Page();
        }

        private void CheckOverTime(IList<Order> Order)
        {
            foreach (var item in Order)
            {
                if (item.OrderStatus.StatusName == "Waiting")
                {
                    if (DateTime.Now - item.UpdatedWaitingAt >= TimeSpan.FromMinutes(10))
                    {
                        item.IsOverWaitingTime = true;
                        item.IsOverDeliveringTime = false;
                        item.IsOverMakingTime = false;
                        item.OrderNotification = "This order has been waiting for more than 10 minutes \n.The specific time is: "+ (DateTime.Now - item.UpdatedWaitingAt).ToString();
                    }else
                    {
                        item.IsOverWaitingTime = false;
                        item.IsOverDeliveringTime = false;
                        item.IsOverMakingTime = false;
                        item.OrderNotification = "No Note";
                    }    
                }

                if (item.OrderStatus.StatusName == "Making")
                {
                    if (DateTime.Now - item.UpdatedMakingAt >= TimeSpan.FromMinutes(15))
                    {
                        item.IsOverMakingTime = true;
                        item.IsOverWaitingTime = false;
                        item.IsOverDeliveringTime = false;
                        item.OrderNotification = "This order has been making for more than 15 minutes \n.The specific time is: "+ (DateTime.Now - item.UpdatedMakingAt).ToString();
                    }else
                    {
                        item.IsOverWaitingTime = false;
                        item.IsOverDeliveringTime = false;
                        item.IsOverMakingTime = false;
                        item.OrderNotification = "No Note";
                    }    
                }

                if (item.OrderStatus.StatusName == "Delivering")
                {
                    if (DateTime.Now - item.UpdatedDeliveringAt >= TimeSpan.FromMinutes(15))
                    {
                        item.IsOverDeliveringTime = true;
                        item.IsOverMakingTime = false;
                        item.IsOverWaitingTime=false;
                        item.OrderNotification = "This order has been delivering for more than 15 minutes \n.The specific time is: "+ (DateTime.Now - item.UpdatedDeliveringAt).ToString();
                    }else
                    {
                        item.IsOverWaitingTime = false;
                        item.IsOverDeliveringTime = false;
                        item.IsOverMakingTime = false;
                        item.OrderNotification = "No Note";
                    }    
                }

                _context.Update(item);

            }
        }


        public async Task<IActionResult> OnPostAsync(int id)
        {
            var userId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                var user = _context.Employees.Where(us => us.Id == userId).FirstOrDefault();
                if (user != null&& user.Role == "Employee")
                {
                    if (!string.IsNullOrEmpty(Request.Form["Delete"]))
                    {
                        var softDelOrder = _context.Orders.Where(or => or.Id == id).FirstOrDefault();
                        if (softDelOrder != null)
                        {
                            softDelOrder.OrderStatusId = 7;
                            softDelOrder.EmployeeId = userId;
                            softDelOrder.IsOverDeliveringTime=false;
                            softDelOrder.IsOverMakingTime = false;
                            softDelOrder.IsOverWaitingTime=false;
                            _context.Orders.Attach(softDelOrder).State = EntityState.Modified;
                            await _context.SaveChangesAsync();                         
                        }
                        HttpContext.Session.SetString("ErrorMessage","Soft delete successfully.") ;
                        return await OnGetAsync();
                    }
                    else return NotFound();
                }
                else if (user != null && user.Role == "Manager Employee" && !string.IsNullOrEmpty(Request.Form["Delete"]))
                {
                    var hardDelOrder = _context.Orders.Where(or => or.Id == id).Include(ors=>ors.OrderStatus).FirstOrDefault();
                    if (hardDelOrder != null && hardDelOrder.OrderStatus.IsActive==false)
                    {
                        _context.Orders.Remove(hardDelOrder);
                        await _context.SaveChangesAsync();
                        HttpContext.Session.SetString("ErrorMessage", "Delete order successfully.");
                    }
                    else if (hardDelOrder != null && hardDelOrder.OrderStatus.IsActive == true)
                        HttpContext.Session.SetString("ErrorMessage", "This order is active, cant delete.");
                    else return NotFound();
                    return await OnGetAsync();
                }
                else return NotFound();
            }
            else return NotFound();
        }
    }
}
