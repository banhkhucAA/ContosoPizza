using ContosoPizza.Models.Generated;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Drawing.Printing;
using System.Linq;

namespace ContosoPizza.Pages.StatisticalReport
{
    public class ReportOrderModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ReportOrderModel(ContosoPizza.Data.ContosoPizzaContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        /*[BindProperty(SupportsGet = true)]
        public string OrderBy { get; set; }*/
        public IList<ReportEmployeeDto> reportEmployeeDtos { get; set; }
        public IList<ReportOrderDto> ReportOrderDtos { get; set; }
        public int TotalOrders { get; set; }
        public float TotalMoneyOrders { get; set; }
        public int TotalSuccessOrders { get; set; }
        public int TotalFailedOrders { get; set; }
        public float SuccesfullOrdersMoney { get; set; }
        public float FailedOrdersMoney { get; set; }
        public int DeliveredSuccessfullyOrders { get; set; }
        public int DeliveredCanceled_FailedOrders { get; set; }
        public int Page { get; set; }
        public int pageSize = 5;
        [BindProperty(SupportsGet =true)]
        public DateTime FromDate { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime ToDate { get; set; }
        public async Task<IActionResult> OnGetAsync(string OrderBy)
        {
            var orders = _context.Orders.ToList();
            var ordersDto = (from o in _context.Orders.ToList()
                             select new ReportOrderDto
                             {
                                 Id = o.Id,
                                 OrderPlacedAt = o.OrderPlacedAt,
                                 OrderFullfilledAt = o.OrderFullFilledAt,
                                 PriceOrder = (float)o.BillPrice,
                                 DiscountAmount = (float)((o.CustomerId == null || o.CouponId == null) ? 0 : Math.Round((float)o.BillPrice * o.Coupon.DiscountAmount / 100, 2))
                             }).ToList();

            if (Request.Query.ContainsKey("FromDate") && !Request.Query.ContainsKey("ToDate"))
            {
                ordersDto = ordersDto.Where(or => or.OrderPlacedAt >= FromDate).ToList();
                TotalOrders = orders.Count(or => or.OrderPlacedAt >= FromDate);
                TotalMoneyOrders = (float)Math.Round((decimal)orders.Where(or => or.OrderPlacedAt >= FromDate).Sum(b => b.BillPrice), 2);
                TotalSuccessOrders = orders.Count(b => b.OrderStatus.StatusName == "Delivered" && b.OrderPlacedAt >= FromDate);
                TotalFailedOrders = orders.Count(b => (b.OrderStatus.StatusName == "Canceled" || b.OrderStatus.StatusName == "Returned" || b.OrderStatus.StatusName == "DeActive") && b.OrderPlacedAt >= FromDate);
                SuccesfullOrdersMoney = (float)Math.Round((decimal)orders.Where(b => b.OrderStatus.StatusName == "Delivered" && b.OrderPlacedAt >= FromDate).Sum(b => b.BillPrice), 2);
                FailedOrdersMoney = (float)Math.Round((decimal)orders.Where(b => (b.OrderStatus.StatusName == "Canceled" || b.OrderStatus.StatusName == "Returned" || b.OrderStatus.StatusName == "DeActive") && b.OrderPlacedAt >= FromDate).Sum(b => b.BillPrice), 2);
                DeliveredSuccessfullyOrders = orders.Where(b => b.OrderStatus.StatusName == "Delivered" && b.DeliveryMethod.Method == "Shipping" && b.OrderPlacedAt >= FromDate).Count();
                DeliveredCanceled_FailedOrders = orders.Where(b => (b.OrderStatus.StatusName == "Canceled" || b.OrderStatus.StatusName == "Returned" || b.OrderStatus.StatusName == "DeActive") && b.DeliveryMethod.Method == "Shipping" && b.OrderPlacedAt >= FromDate).Count();
            }

            if (Request.Query.ContainsKey("ToDate") && !Request.Query.ContainsKey("FromDate"))
            {
                ordersDto = ordersDto.Where(or => or.OrderPlacedAt <= ToDate).ToList();
                TotalOrders = orders.Count(or => or.OrderPlacedAt <= ToDate);
                TotalMoneyOrders = (float)Math.Round((decimal)orders.Where(or => or.OrderPlacedAt <= ToDate).Sum(b => b.BillPrice), 2);
                TotalSuccessOrders = orders.Count(b => b.OrderStatus.StatusName == "Delivered" && b.OrderPlacedAt <= ToDate);
                TotalFailedOrders = orders.Count(b => (b.OrderStatus.StatusName == "Canceled" || b.OrderStatus.StatusName == "Returned" || b.OrderStatus.StatusName == "DeActive") && b.OrderPlacedAt <= ToDate);
                SuccesfullOrdersMoney = (float)Math.Round((decimal)orders.Where(b => b.OrderStatus.StatusName == "Delivered" && b.OrderPlacedAt <= ToDate).Sum(b => b.BillPrice), 2);
                FailedOrdersMoney = (float)Math.Round((decimal)orders.Where(b => (b.OrderStatus.StatusName == "Canceled" || b.OrderStatus.StatusName == "Returned" || b.OrderStatus.StatusName == "DeActive") && b.OrderPlacedAt <= ToDate).Sum(b => b.BillPrice), 2);
                DeliveredSuccessfullyOrders = orders.Where(b => b.OrderStatus.StatusName == "Delivered" && b.DeliveryMethod.Method == "Shipping" && b.OrderPlacedAt <= ToDate).Count();
                DeliveredCanceled_FailedOrders = orders.Where(b => (b.OrderStatus.StatusName == "Canceled" || b.OrderStatus.StatusName == "Returned" || b.OrderStatus.StatusName == "DeActive") && b.DeliveryMethod.Method == "Shipping" && b.OrderPlacedAt <= ToDate).Count();
            }

            if (Request.Query.ContainsKey("ToDate") && Request.Query.ContainsKey("FromDate"))
            {
                ordersDto = ordersDto.Where(or => or.OrderPlacedAt >= FromDate && or.OrderPlacedAt <= ToDate).ToList();
                TotalOrders = orders.Count(or => or.OrderPlacedAt >= FromDate && or.OrderPlacedAt <= ToDate);
                TotalMoneyOrders = (float)Math.Round((decimal)orders.Where(or => or.OrderPlacedAt >= FromDate && or.OrderPlacedAt <= ToDate).Sum(b => b.BillPrice), 2);
                TotalSuccessOrders = orders.Count(b => b.OrderStatus.StatusName == "Delivered" && b.OrderPlacedAt >= FromDate && b.OrderPlacedAt <= ToDate);
                TotalFailedOrders = orders.Count(b => (b.OrderStatus.StatusName == "Canceled" || b.OrderStatus.StatusName == "Returned" || b.OrderStatus.StatusName == "DeActive") && b.OrderPlacedAt >= FromDate && b.OrderPlacedAt <= ToDate);
                SuccesfullOrdersMoney = (float)Math.Round((decimal)orders.Where(b => b.OrderStatus.StatusName == "Delivered" && b.OrderPlacedAt >= FromDate && b.OrderPlacedAt <= ToDate).Sum(b => b.BillPrice), 2);
                FailedOrdersMoney = (float)Math.Round((decimal)orders.Where(b => (b.OrderStatus.StatusName == "Canceled" || b.OrderStatus.StatusName == "Returned" || b.OrderStatus.StatusName == "DeActive") && b.OrderPlacedAt >= FromDate && b.OrderPlacedAt <= ToDate).Sum(b => b.BillPrice), 2);
                DeliveredSuccessfullyOrders = orders.Where(b => b.OrderStatus.StatusName == "Delivered" && b.DeliveryMethod.Method == "Shipping" && b.OrderPlacedAt >= FromDate && b.OrderPlacedAt <= ToDate).Count();
                DeliveredCanceled_FailedOrders = orders.Where(b => (b.OrderStatus.StatusName == "Canceled" || b.OrderStatus.StatusName == "Returned" || b.OrderStatus.StatusName == "DeActive") && b.DeliveryMethod.Method == "Shipping" && b.OrderPlacedAt >= FromDate && b.OrderPlacedAt <= ToDate).Count();
            }

            if (!Request.Query.ContainsKey("FromDate") && !Request.Query.ContainsKey("ToDate"))
            {
                TotalOrders = orders.Count();
                TotalMoneyOrders = (float)Math.Round((decimal)orders.Sum(b => b.BillPrice), 2);
                TotalSuccessOrders = orders.Count(b => b.OrderStatus.StatusName == "Delivered");
                TotalFailedOrders = orders.Count(b => b.OrderStatus.StatusName == "Canceled" || b.OrderStatus.StatusName == "Returned" || b.OrderStatus.StatusName == "DeActive");
                SuccesfullOrdersMoney = (float)Math.Round((decimal)orders.Where(b => b.OrderStatus.StatusName == "Delivered").Sum(b => b.BillPrice), 2);
                FailedOrdersMoney = (float)Math.Round((decimal)orders.Where(b => b.OrderStatus.StatusName == "Canceled" || b.OrderStatus.StatusName == "Returned" || b.OrderStatus.StatusName == "DeActive").Sum(b => b.BillPrice), 2);
                DeliveredSuccessfullyOrders = orders.Where(b => b.OrderStatus.StatusName == "Delivered" && b.DeliveryMethod.Method == "Shipping").Count();
                DeliveredCanceled_FailedOrders = orders.Where(b => (b.OrderStatus.StatusName == "Canceled" || b.OrderStatus.StatusName == "Returned" || b.OrderStatus.StatusName == "DeActive") && b.DeliveryMethod.Method == "Shipping").Count();
            }


            if (OrderBy == "OrderByBillPrice")
            {
                ordersDto = ordersDto.OrderByDescending(e => e.PriceOrder).ToList();
            }
            else if (OrderBy == "OrderByDiscountAmount")
            {
                ordersDto = ordersDto.OrderByDescending(e => e.DiscountAmount).ToList();
            }
            
            //order lại
            if (Request.Query.ContainsKey("page"))
            {
                int.TryParse(Request.Query["page"], out int page);
                Page = Math.Max(page, 1); // Đảm bảo giá trị trang không nhỏ hơn 1
            }
            else
            {
                Page = 1; // Trang mặc định là 1
            }

            if (_context.Orders != null)
            {
                int offset = Math.Max((Page - 1) * pageSize, 0);
                ReportOrderDtos = ordersDto
                .Skip(offset)
                    .Take(pageSize)
                    .ToList();
            }
            return Page();
        }
    }
}
