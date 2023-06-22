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

namespace ContosoPizza.Pages.OrderDetails
{
    public class CreateModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;

        public CreateModel(ContosoPizza.Data.ContosoPizzaContext context)
        {
            _context = context;
        }


        [BindProperty]
        public OrderDetail OrderDetail { get; set; } = default!;
        [BindProperty]
        public Order Order { get; set; }
        [BindProperty]
        public IList<OrderDetail> OrderDetails_Show { get; set; } = default!;
        public IActionResult OnGet(int? orderId)
        {
            var order = _context.Orders
                .Include(o => o.Coupon)
                .Include(o => o.Customer)
                .Include(o => o.DeliveryMethod)
                .Include(o => o.OrderStatus)
                .Include(o => o.PaymentMethod)
                .Include(o => o.Employee)
                .FirstOrDefault(x => x.Id == orderId);

            var orderDetails = _context.OrderDetails.Where(x => x.OrderId == orderId).ToList();

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
        public async Task<IActionResult> OnPostAsync()
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
    }
}
