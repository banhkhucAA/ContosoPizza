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
using ContosoPizza.Models;
using ContosoPizza.Pages.Products;
using System.Text.Json;
using MailKit.Security;
using MimeKit.Text;
using MimeKit;
using MailKit.Net.Smtp;

namespace ContosoPizza.Pages.Orders
{
    public class CreateModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;

        public CreateModel(ContosoPizza.Data.ContosoPizzaContext context,IHttpContextAccessor httpContextAccessor,IConfiguration config)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _config = config;
        }

        [BindProperty]// cứ http request thì là cần
        public Order Order { get; set; } = default!;
        [BindProperty]
        public Customer Customer { get; set; }
        public string ErrorMessage { get; set; }
        public List<OrderDetailDto> OrderDetails_Show { get; set; }
        public string emailstring;
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
            
            var coupons = await _context.Coupons.ToListAsync();
            var selectCouponItems = new List<SelectListItem>()
            {
                new SelectListItem{Value=null,Text="None"}
            };
            selectCouponItems.AddRange(coupons.Select(cp => new SelectListItem { Value = cp.Id.ToString(), Text = cp.CouponCode }));
            ViewData["CouponId"] = selectCouponItems;
            if (_httpContextAccessor.HttpContext.Session.GetString("UserRole") == "Employee")///employee
            {
                ViewData["DeliveryMethodId"] = new SelectList(_context.DeliveryMethods, "Id", "Method");
                var customers = await _context.Customers.ToListAsync();
                var selectCustomerItems = new List<SelectListItem>()
                {
                    new SelectListItem{Value=null,Text="None"}
                };
                selectCustomerItems.AddRange(customers.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Id.ToString() }));
                ViewData["CustomerId"] = selectCustomerItems;
            }////customer khi chưa đăng nhập chỉ có thể set listCustomers = rỗng, và ko hiển thị nó lên để mặc định nó là null.
            ////customer đã đăng nhập thì set listCustomers = đúng Id của nó
            else
            {
                var deliverymethods = await _context.DeliveryMethods.Where(dm => dm.Method != "Restaurant").ToListAsync();
                ViewData["DeliveryMethodId"] = new SelectList(deliverymethods, "Id", "Method");               
            }
            
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
            if (_httpContextAccessor.HttpContext.Session.GetString("UserRole") == "Employee")////employee
                return Redirect($"/OrderDetails/Create?orderId={Order.Id}");
            else
            {
                TempData["ErrorMessage"] = "Create your order successfully.";
                var orderDetailsJson = HttpContext.Session.GetString("OrderDetail_Show");
                if (!string.IsNullOrEmpty(orderDetailsJson))
                {
                    var OrderDetails_Show = JsonSerializer.Deserialize<List<OrderDetailDto>>(orderDetailsJson);
                    foreach(var eachorderDetails in OrderDetails_Show)
                    {
                        var orderDetails = new OrderDetail
                        {
                            OrderId = Order.Id,
                            ProductId = eachorderDetails.ProductId,
                            Quantity = eachorderDetails.Quantity,
                            TotalPrice = eachorderDetails.TotalPrice,
                        };
                        _context.OrderDetails.Add(orderDetails);
                        Order.BillPrice += (float)orderDetails.TotalPrice;
                        _context.Orders.Update(Order);                       
                    }
                    await _context.SaveChangesAsync();
                    HttpContext.Session.SetInt32("OrderId", Order.Id);
                    await OnGetNewOrders(Order);
                    HttpContext.Session.Remove("OrderDetail_Show");
                    return Redirect("/Cart");
                }
                else
                {
                    var OrderDetails_Show = new List<OrderDetailDto>();
                    Console.WriteLine("Still nothing");
                }

                return Page();
            } 
                
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

        public async Task<IActionResult> OnGetNewOrders(Order myorder)
        {
            if (!_httpContextAccessor.HttpContext.Session.Keys.Contains("OrderId"))
            {
                return NotFound("You have no new orders.");
            }
            else
            {
                Order = await _context.Orders.FirstOrDefaultAsync(or => or.Id == _httpContextAccessor.HttpContext.Session.GetInt32("OrderId"));
            }

            var orderDetailsJson = _httpContextAccessor.HttpContext.Session.GetString("OrderDetail_Show");
            if (!string.IsNullOrEmpty(orderDetailsJson))
            {
                OrderDetails_Show = JsonSerializer.Deserialize<List<OrderDetailDto>>(orderDetailsJson);
            }
            else
            {
                OrderDetails_Show = new List<OrderDetailDto>();
            }

            var email = new MimeMessage();
            
            email.From.Add(MailboxAddress.Parse("nda2932002@gmail.com"));
            if (_httpContextAccessor.HttpContext.Session.Keys.Contains("UserRole"))
            {
                if (_httpContextAccessor.HttpContext.Session.GetString("UserRole") == "Customer")//thành viên
                {
                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == _httpContextAccessor.HttpContext.Session.GetInt32("UserId"));
                    if (customer != null)
                    {
                        email.To.Add(MailboxAddress.Parse(customer.Email));
                        emailstring = customer.Email;
                    }
                    else
                    {
                        ErrorMessage = "Your email doesn't exist";
                    }
                }
            } else////không là thành viên
            {
                email.To.Add(MailboxAddress.Parse(myorder.NoneCustomerMemberEmailAddress));
                emailstring = myorder.NoneCustomerMemberEmailAddress;
            }
            email.Subject = "Order successfully";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = GetEmailBody(myorder),
            };

            try
            {
                using (var smtp = new SmtpClient())
                {
                    smtp.Connect(_config.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
                    smtp.Authenticate(_config.GetSection("EmailUserName").Value, _config.GetSection("EmailPassword").Value);
                    smtp.Send(email);
                    smtp.Disconnect(true);

                    // Thông báo gửi email thành công
                    ErrorMessage = "Email sent successfully to "+ emailstring + " !";
                }
            }
            catch (Exception ex)
            {
                // Xử lý khi gửi email thất bại
                ErrorMessage = "Failed to send email: " + ex.Message;
            }


            return await OnGet();
        }

        public string GetEmailBody(Order myorder)
        {
            double bill = 0;

            string emailBody = "<div class=\"container\">" +
                "<div class=\"row\">" +
                "<div class=\"col-md-2\"></div>" +
                "<div class=\"col-md-8\">" +
                "<h1>Here are your order details, please contact us if there is any problem.</h1>" +
                "<hr />" +
                "</div>" +
                "</div>" +
                "<div class=\"row\">" +
                "<div class=\"col-md-2\"></div>" +
                "<div class=\"col-md-8\">" +
                "<table class=\"table\">" +
                "<thead>" +
                "<tr>" +
                "<th>Product Name</th>" +
                "<th>&nbsp;&nbsp;&nbsp;&nbsp;Quantity&nbsp;&nbsp;&nbsp;&nbsp;</th>" +
                "<th>&nbsp;&nbsp;&nbsp;&nbsp;Total Price&nbsp;&nbsp;&nbsp;&nbsp;</th>" +
                "<th></th>" +
                "</tr>" +
                "</thead>" +
                "<tbody>";

            foreach (var item in OrderDetails_Show)
            {
                emailBody += "<tr>" +
                    "<td>" + item.ProductName + "</td>" +
                    "<td>&nbsp;&nbsp;&nbsp;&nbsp;" + item.Quantity + "</td>" +
                    "<td>&nbsp;&nbsp;&nbsp;&nbsp;" + item.TotalPrice + "</td>" +
                    "</tr>";
                bill += (float)item.TotalPrice;
            }

            emailBody += "</tbody>" +
                "</table>" +
                "</div>" +
                "</div>" +
                "<br>";

            if (myorder != null)
            {
                var abc = _context.DeliveryMethods.Where(de => de.Id == myorder.DeliveryMethodId).Select(p=>p.Price).FirstOrDefault();
                var xyz = _context.PaymentMethods.Where(pm => pm.Id == myorder.PaymentMethodId).Select(p => p.Price).FirstOrDefault();

                emailBody += "<div class=\"row\">" +
                    "<div class=\"col-md-2\"></div>" +
                    "<div class=\"col-md-8\">" +
                    "<div>DeliveryMethod Price: " + abc + " $</div>" +
                    "<div>PaymentMethod Price: " + xyz + " $</div>";

                if (myorder.Coupon == null)
                {
                    emailBody += "<div>Total Discount: 0 $</div>" +
                        "<div>Final Price: " + (Math.Round(bill + abc + xyz, 2)) + " $</div>";
                }
                else
                {
                    var discount = _context.Coupons.Where(c => c.Id == myorder.CouponId).Select(c => c.DiscountAmount).FirstOrDefault();
                    emailBody += "<div>Total Discount: " + (Math.Round((float)bill * discount / 100, 2)) + " $</div>";
                    var totalPrice = Math.Round(bill + abc + xyz - (float)(bill * discount / 100), 2);
                    emailBody += "<div>Final Price: " + (Math.Round(totalPrice, 2)) + " $</div>";

                    if (_httpContextAccessor.HttpContext.Session.GetString("UserRole") == "Customer")
                    {
                        emailBody += "<hr /><div>Order placed at: " + myorder.OrderPlacedAt + "</div>";
                    }
                }

                emailBody += "</div>" +
                    "</div>";
            }

            emailBody += "</div>" +
                "</div>";

            return emailBody;
        }


    }
}

