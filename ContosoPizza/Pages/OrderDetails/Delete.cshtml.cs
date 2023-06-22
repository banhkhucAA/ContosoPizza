using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ContosoPizza.Data;
using ContosoPizza.Models.Generated;

namespace ContosoPizza.Pages.OrderDetails
{
    public class DeleteModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;

        public DeleteModel(ContosoPizza.Data.ContosoPizzaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public OrderDetail OrderDetail { get; set; } = default!;

        [BindProperty]
        public Order Order { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.OrderDetails == null)
            {
                return NotFound();
            }

            var orderdetail = await _context.OrderDetails
                .Include(p=>p.Product)
                .Include(o=>o.Order)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (orderdetail == null)
            {
                return NotFound();
            }
            else 
            {
                OrderDetail = orderdetail;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            var updateOrder_id = _context.OrderDetails.Where(p => p.Id == id).Select(p => p.OrderId).FirstOrDefault();
            Console.WriteLine("updateOrder_id là: " + updateOrder_id);
            var updateOrder = _context.Orders.Where(p => p.Id == updateOrder_id).FirstOrDefault();
            var DeliveryPrice = (float)_context.DeliveryMethods.Where(p => p.Id == updateOrder.DeliveryMethodId).Select(p => p.Price).FirstOrDefault();
            Console.WriteLine("updateOrder_id là: " + updateOrder_id);
            var PaymentPrice = (float)_context.PaymentMethods.Where(p => p.Id == updateOrder.PaymentMethodId).Select(p => p.Price).FirstOrDefault();
            float discountPrice = 0;
            if (updateOrder.CouponId != null)
            {
                discountPrice = (float)_context.Coupons.Where(p => p.Id == updateOrder.CouponId).Select(p => p.DiscountAmount).FirstOrDefault();
            }
            var orders_detail = _context.OrderDetails.Where(p => p.OrderId == updateOrder.Id);

            if (id == null || _context.OrderDetails == null)
            {
                return NotFound();
            }
            var orderdetail = await _context.OrderDetails.FindAsync(id);

            if (orderdetail != null)
            {
                OrderDetail = orderdetail;
                _context.OrderDetails.Remove(OrderDetail);
                await _context.SaveChangesAsync();
                Console.WriteLine("Liệu bị xóa chưa: " + OrderDetail);
                //lấy id của order đã bị xóa từ OrderDetails
                updateOrder.BillPrice = 0;
                foreach (var item in orders_detail)
                {
                    updateOrder.BillPrice = (float)(updateOrder.BillPrice + item.TotalPrice);
                }

                updateOrder.BillPrice = (float?)Math.Round((float)(updateOrder.BillPrice + DeliveryPrice + PaymentPrice - (float)(discountPrice * updateOrder.BillPrice / 100)), 2);
                updateOrder.BillPrice = (float)Math.Round((float)updateOrder.BillPrice, 2);
                Order = updateOrder;
                if (OrderDetail==null)
                _context.Orders.Attach(Order).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            return Redirect($"/OrderDetails/Create?orderId={Order.Id}");
        }
    }
}
