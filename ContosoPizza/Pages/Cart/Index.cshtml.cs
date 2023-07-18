using ContosoPizza.Models.Generated;
using ContosoPizza.Pages.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ContosoPizza.Pages.Cart
{
    public class IndexModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;

        public IndexModel(ContosoPizza.Data.ContosoPizzaContext context)
        {
            _context = context;
        }
        [BindProperty]
        public List<OrderDetailDto> OrderDetails_Show { get; set; }
        [BindProperty]
        public List<OrderDetailDto> OrderDetails_DisplayQuantityPrice { get; set; }
        [BindProperty]
        public double UnitPrice { get;set; }
        [BindProperty]
        public Order Order { get; set; }
        [BindProperty]
        public string ErrorMessage { get; set; }
        public int Page { get; set; }
        public IActionResult OnGet()
        {
            var orderDetailsJson = HttpContext.Session.GetString("OrderDetail_Show");
            if (!string.IsNullOrEmpty(orderDetailsJson))
            {
                OrderDetails_Show = JsonSerializer.Deserialize<List<OrderDetailDto>>(orderDetailsJson);
            }
            else
            {
                OrderDetails_Show = new List<OrderDetailDto>();
                Console.WriteLine("Still nothing");
            }

            int pageSize = 3;

            if (Request.Query.ContainsKey("Page"))
            {
                int.TryParse(Request.Query["Page"], out int page);
                Page = Math.Max(page, 1); // Đảm bảo giá trị trang không nhỏ hơn 1
            }
            else
            {
                Page = 1; // Trang mặc định là 1
            }

            if (_context.Products != null && OrderDetails_Show!=null)
            {
                int offset = Math.Max((Page - 1) * pageSize, 0);
                OrderDetails_DisplayQuantityPrice = OrderDetails_Show;
                OrderDetails_Show = OrderDetails_Show
                    .Skip(offset)
                    .Take(pageSize)
                    .ToList();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(List<CartItemModel> cartItems,int? id)
        {
            if (!string.IsNullOrEmpty(Request.Form["UpdateQuantity"]))
            {
                var orderDetailsJson = HttpContext.Session.GetString("OrderDetail_Show");
                if (!string.IsNullOrEmpty(orderDetailsJson))
                {
                    OrderDetails_Show = JsonSerializer.Deserialize<List<OrderDetailDto>>(orderDetailsJson);
                    OrderDetails_DisplayQuantityPrice = OrderDetails_Show;
                    foreach (var item in OrderDetails_Show)
                    {
                        var productPrice = await _context.Products.Where(p => p.Id == item.ProductId).Select(p => p.UnitPrice).FirstOrDefaultAsync();
                        var cartItem = cartItems.FirstOrDefault(c => c.ProductId == item.ProductId);
                        if (cartItem != null)
                        {
                            item.Quantity = cartItem.Quantity;
                            item.TotalPrice = cartItem.Quantity * productPrice;
                        }
                    }
                }
                orderDetailsJson = JsonSerializer.Serialize(OrderDetails_Show);
                HttpContext.Session.SetString("OrderDetail_Show", orderDetailsJson);
            }
            else if(!string.IsNullOrEmpty(Request.Form["Delete"]))
            {
                Console.WriteLine("Call successfully");
                var orderDetailsJson = HttpContext.Session.GetString("OrderDetail_Show");
                if (!string.IsNullOrEmpty(orderDetailsJson))
                {
                    OrderDetails_Show = JsonSerializer.Deserialize<List<OrderDetailDto>>(orderDetailsJson);
                    OrderDetails_DisplayQuantityPrice = OrderDetails_Show;
                    var deleteItem = OrderDetails_Show.FirstOrDefault(p=>p.ProductId==id);
                    OrderDetails_Show.Remove(deleteItem);                                     
                }
                orderDetailsJson = JsonSerializer.Serialize(OrderDetails_Show);
                HttpContext.Session.SetString("OrderDetail_Show", orderDetailsJson);
            }    
            
            return Page();
        }

        


    }
}
