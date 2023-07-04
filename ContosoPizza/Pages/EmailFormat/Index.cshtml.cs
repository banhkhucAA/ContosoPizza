using ContosoPizza.Models.Generated;
using ContosoPizza.Pages.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using Microsoft.EntityFrameworkCore;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContosoPizza.Pages.EmailFormat
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

        [BindProperty]
        public List<OrderDetailDto> OrderDetails_Show { get; set; }

        [BindProperty]
        public double UnitPrice { get; set; }

        [BindProperty]
        public Order Order { get; set; }

        [BindProperty]
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGet()
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
            email.To.Add(MailboxAddress.Parse("nda2932002@gmail.com"));
            email.Subject = "Order successfully";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = GetEmailBody(),
            };

            using (var smtp = new SmtpClient())
            {
                smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                smtp.Authenticate("nda2932002@gmail.com", "oihcfhpohmfuzrua");
                smtp.Send(email);
                smtp.Disconnect(true);
            }

            return Redirect("/Products");
        }

        private string GetEmailBody()
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

            if (Order != null)
                {
                    emailBody += "<div class=\"row\">" +
                        "<div class=\"col-md-2\"></div>" +
                        "<div class=\"col-md-8\">" +
                        "<div>DeliveryMethod Price: " + Order.DeliveryMethod.Price + " $</div>" +
                        "<div>PaymentMethod Price: " + Order.PaymentMethod.Price + " $</div>";

                    if (Order.Coupon == null)
                    {
                        emailBody += "<div>Total Discount: 0 $</div>" +
                            "<div>Final Price: " + (Math.Round(bill + Order.DeliveryMethod.Price + Order.PaymentMethod.Price, 2)) + " $</div>";
                    }
                    else
                    {
                        emailBody += "<div>Total Discount: " + (Math.Round((float)bill * Order.Coupon.DiscountAmount / 100, 2)) + " $</div>";
                        var totalPrice = Math.Round(bill + Order.DeliveryMethod.Price + Order.PaymentMethod.Price - (float)(bill * Order.Coupon.DiscountAmount / 100), 2);
                        emailBody += "<div>Final Price: " + (Math.Round(totalPrice, 2)) + " $</div>";

                        if (_httpContextAccessor.HttpContext.Session.GetString("UserRole") == "Customer")
                        {
                            emailBody += "<hr /><div>Order placed at: " + Order.OrderPlacedAt + "</div>";
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