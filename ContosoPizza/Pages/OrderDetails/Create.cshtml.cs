using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ContosoPizza.Data;
using ContosoPizza.Models.Generated;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using MailKit.Security;
using MimeKit.Text;
using MimeKit;
using static Org.BouncyCastle.Math.EC.ECCurve;
using MailKit.Net.Smtp;
using ContosoPizza.Pages.Cart;
using MailKit.Search;

namespace ContosoPizza.Pages.OrderDetails
{
    public class CreateModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;

        public CreateModel(ContosoPizza.Data.ContosoPizzaContext context, IHttpContextAccessor httpContextAccessor, IConfiguration config)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _config = config;
        }


        [BindProperty]
        public OrderDetail OrderDetail { get; set; } = default!;
        [BindProperty]
        public Order Order { get; set; }
        [BindProperty]
        public List<OrderDetail> OrderDetails_Show { get; set; } = default!;
        public string emailstring;
        public IActionResult OnGet(int? orderId)
        {
            var order = _context.Orders
                .FirstOrDefault(x => x.Id == orderId);

            var orderDetails = _context.OrderDetails.Where(x => x.OrderId == orderId).Include(p=>p.Product).ToList();

            if (order == null)
            {
                return NotFound();
            }

            Order = order;

            if (orderDetails!=null) 
            {
                OrderDetails_Show = orderDetails;
            }

            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id");
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "ProductName");
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FirstName");
            Console.WriteLine("This is the Id: " + Order.Id);
            return Page();
        }
        [BindProperty]
        public float DiscountAmountAdd { get; set; }
        [BindProperty]
        public float DeliveryPriceAdd { get; set; }
        [BindProperty]
        public float PaymentMethodPriceAdd { get; set; }
        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (!string.IsNullOrEmpty(Request.Form["createButton"]))
            {
                await OnCreate();
                return Redirect($"/OrderDetails/Create?orderId={Order.Id}");
            }
            else if (!string.IsNullOrEmpty(Request.Form["SendEmail"]))
            {
                await OnSendEmail(Order.Id);
                return Redirect($"/OrderDetails/Create?orderId={Order.Id}");
            }
            else if (!string.IsNullOrEmpty(Request.Form["UpdateQuantity"]))
            {
                await OnUpdateQuantity();
                return Redirect($"/OrderDetails/Create?orderId={OrderDetails_Show[0].OrderId}");
            }
            else if (!string.IsNullOrEmpty(Request.Form["DeleteProduct"]))
            {
                await OnDeleteProduct(id);
                return Redirect($"/OrderDetails/Create?orderId={OrderDetails_Show[0].OrderId}");
            }
            else
            {
                return NotFound();
            }    
 
        }

        private async Task<IActionResult> OnDeleteProduct(int? id)
        {
            foreach (var item in OrderDetails_Show)
            {
                var findProduct = await _context.OrderDetails.Where(p => p.Id == id).FirstOrDefaultAsync();
                if (findProduct != null)
                {
                    _context.OrderDetails.Remove(findProduct);
                }
            }
            var order = _context.Orders
                .FirstOrDefault(x => x.Id == OrderDetails_Show[0].OrderId);
            var DeliveryPrice = _context.DeliveryMethods.Where(p => p.Id == order.DeliveryMethodId).Select(p => p.Price).FirstOrDefault();
            DeliveryPriceAdd = (float)DeliveryPrice;
            var PaymentPrice = _context.PaymentMethods.Where(p => p.Id == order.PaymentMethodId).Select(p => p.Price).FirstOrDefault();
            PaymentMethodPriceAdd = (float)PaymentPrice;

            if (order.CouponId == null)
            {
                DiscountAmountAdd = 0;
            }
            else
            {
                var DiscountPrice = _context.Coupons.Where(p => p.Id == order.CouponId).Select(p => p.DiscountAmount).FirstOrDefault();
                DiscountAmountAdd = (float)DiscountPrice;
            }

            var newBillPrice = OrderDetails_Show.Sum(x => x.TotalPrice);

            var newOrderBillPrice = newBillPrice;


            order.BillPrice = (float?)Math.Round((float)(newBillPrice + DeliveryPrice + PaymentPrice - (float)(newBillPrice * DiscountAmountAdd / 100)), 2);
            order.BillPrice = (float)Math.Round((float)order.BillPrice, 2);

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return Page();
        }

        private async Task<IActionResult> OnUpdateQuantity()
        {
            foreach (var item in OrderDetails_Show)
            {
                var productPrice = await _context.Products.Where(p => p.Id == item.ProductId).Select(p => p.UnitPrice).FirstOrDefaultAsync();
                var cartItem = OrderDetails_Show.FirstOrDefault(c => c.ProductId == item.ProductId);
                if (cartItem != null)
                {
                    item.Quantity = cartItem.Quantity;
                    item.TotalPrice = cartItem.Quantity * productPrice;
                }
                _context.OrderDetails.Update(item);
            }
            var order = _context.Orders
                .FirstOrDefault(x => x.Id == OrderDetails_Show[0].OrderId);
            var DeliveryPrice = _context.DeliveryMethods.Where(p => p.Id == order.DeliveryMethodId).Select(p => p.Price).FirstOrDefault();
            DeliveryPriceAdd = (float)DeliveryPrice;
            var PaymentPrice = _context.PaymentMethods.Where(p => p.Id == order.PaymentMethodId).Select(p => p.Price).FirstOrDefault();
            PaymentMethodPriceAdd = (float)PaymentPrice;

            if (order.CouponId == null)
            {
                DiscountAmountAdd = 0;
            }
            else
            {
                var DiscountPrice = _context.Coupons.Where(p => p.Id == order.CouponId).Select(p => p.DiscountAmount).FirstOrDefault();
                DiscountAmountAdd = (float)DiscountPrice;
            }

            var newBillPrice = OrderDetails_Show.Sum(x => x.TotalPrice);

            var newOrderBillPrice = newBillPrice;
            

            order.BillPrice = (float?)Math.Round((float)(newBillPrice + DeliveryPrice + PaymentPrice - (float)(newBillPrice * DiscountAmountAdd / 100)), 2);
            order.BillPrice = (float)Math.Round((float)order.BillPrice, 2);

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return Page();
        }

        private async Task<IActionResult> OnSendEmail(int id)
        {
            var order = await _context.Orders.Where(or => or.Id == id).FirstOrDefaultAsync();
            var email = new MimeMessage();

            email.From.Add(MailboxAddress.Parse("nda2932002@gmail.com"));
            if (_httpContextAccessor.HttpContext.Session.Keys.Contains("UserRole"))
            {
                if (order.CustomerId!=null)//thành viên
                {
                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == order.Customer.Id);
                    if (customer != null)
                    {
                        email.To.Add(MailboxAddress.Parse(customer.Email));
                        emailstring = customer.Email;
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Your email doesn't exist";
                    }
                }
                else////không là thành viên
                {
                    email.To.Add(MailboxAddress.Parse(order.NoneCustomerMemberEmailAddress));
                    emailstring = order.NoneCustomerMemberEmailAddress;
                }
            }
            
            email.Subject = "Order successfully";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = GetEmailBody(order),
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
                    TempData["ErrorMessage"] = "Email sent successfully to " + emailstring + " !";
                }
            }
            catch (Exception ex)
            {
                // Xử lý khi gửi email thất bại
                TempData["ErrorMessage"] = "Failed to send email: " + ex.Message;
            }


            return OnGet(id);
        }

        private async Task<IActionResult> OnCreate()
        {
            var ProductPrice = _context.Products.Where(p => p.Id == OrderDetail.ProductId).Select(p => p.UnitPrice).FirstOrDefault();
            var DeliveryPrice = _context.DeliveryMethods.Where(p => p.Id == Order.DeliveryMethodId).Select(p => p.Price).FirstOrDefault();
            DeliveryPriceAdd = (float)DeliveryPrice;
            var PaymentPrice = _context.PaymentMethods.Where(p => p.Id == Order.PaymentMethodId).Select(p => p.Price).FirstOrDefault();
            PaymentMethodPriceAdd = (float)PaymentPrice;

            OrderDetail.TotalPrice = ProductPrice * OrderDetail.Quantity;

            var Orders_detail = _context.OrderDetails.Where(o => o.OrderId == Order.Id);

            if (!Orders_detail.Any())
            {
                Order.BillPrice = (float)OrderDetail.TotalPrice;
            }
            else
            {
                Order.BillPrice = (float)Orders_detail.Sum(o => o.TotalPrice) + (float)OrderDetail.TotalPrice;
            }

            if (Order.CouponId == null)
            {
                DiscountAmountAdd = 0;
            }
            else
            {
                var DiscountPrice = _context.Coupons.Where(p => p.Id == Order.CouponId).Select(p => p.DiscountAmount).FirstOrDefault();
                DiscountAmountAdd = (float)DiscountPrice;
            }
            /*float Discount_part = (float)(Order.BillPrice * DiscountAmountAdd / 100.0);*/
            Order.BillPrice = (float?)Math.Round((float)(Order.BillPrice + DeliveryPrice + PaymentPrice - (float)(Order.BillPrice * DiscountAmountAdd / 100)), 2);
            DiscountAmountAdd = (float)Math.Round((float)(Order.BillPrice * DiscountAmountAdd / 100), 2);
            Order.BillPrice = (float)Math.Round((float)Order.BillPrice, 2);

            if (_context.OrderDetails == null || OrderDetail == null)
            {
                return Page();
            }

            var check_if_product_exist_in_the_created_order = _context.OrderDetails.Where(p => p.OrderId == Order.Id && p.ProductId == OrderDetail.ProductId).FirstOrDefault();
            if (check_if_product_exist_in_the_created_order != null)
            {
                check_if_product_exist_in_the_created_order.Quantity += OrderDetail.Quantity;
                check_if_product_exist_in_the_created_order.TotalPrice = ProductPrice * check_if_product_exist_in_the_created_order.Quantity;
                _context.OrderDetails.Attach(check_if_product_exist_in_the_created_order).State = EntityState.Modified;
            }
            else
            {
                _context.OrderDetails.Add(OrderDetail);
            }

            _context.Orders.Attach(Order).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return Redirect($"/OrderDetails/Create?orderId={Order.Id}");

        }

        public string GetEmailBody(Order myorder)
        {
            double bill = 0;
            var orderDetails = _context.OrderDetails.Where(ordt => ordt.OrderId == myorder.Id).ToList();

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

            foreach (var item in orderDetails)
            {
                emailBody += "<tr>" +
                    "<td>" + item.Product.ProductName + "</td>" +
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
                var abc = _context.DeliveryMethods.Where(de => de.Id == myorder.DeliveryMethodId).Select(p => p.Price).FirstOrDefault();
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

                    if (_httpContextAccessor.HttpContext.Session.GetString("UserRole") == "Employee")
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
