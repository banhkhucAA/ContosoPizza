using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ContosoPizza.Models.Generated;
using Microsoft.EntityFrameworkCore;
using ContosoPizza.Pages.Products;
using System.Text.Json;

namespace ContosoPizza.Pages.Home
{

    public class HomeModel : PageModel
    {

        private readonly ContosoPizza.Data.ContosoPizzaContext _context;
        public HomeModel(ContosoPizza.Data.ContosoPizzaContext context)
        {
            _context = context;
        }
        public IList<Product> ProductsList { get; set; }
        public IList<Product> MostFavourablePizzas { get; set; }
        public IList<Product> MostFavourableDrinks { get; set; }
        public IList<Product> MostFavourableBurgers { get; set; }
        public IList<Product> MostFavourablePastas { get; set; }
        public async Task<IActionResult> OnGet()
        {
            ProductsList = await _context.Products.Where(p=>p.Id==25|| p.Id == 26 || p.Id == 27 || p.Id == 28 || p.Id == 29 || p.Id == 30).ToListAsync();
            MostFavourablePizzas = ProductsList.Take(3).ToList();
            MostFavourableDrinks = await _context.Products.Where(p => p.Id == 31 || p.Id == 32 || p.Id == 33).ToListAsync();
            MostFavourableBurgers = await _context.Products.Where(p => p.Id == 34 || p.Id == 35 || p.Id == 36).ToListAsync();
            MostFavourablePastas =  await _context.Products.Where(p => p.Id == 37 || p.Id == 38 || p.Id == 39).ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            Console.WriteLine("call on post");
            var findProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (findProduct != null)
            {
                var orderDetailsJson = HttpContext.Session.GetString("OrderDetail_Show");
                List<OrderDetailDto> orderDetails;
                if (!string.IsNullOrEmpty(orderDetailsJson))
                {
                    orderDetails = JsonSerializer.Deserialize<List<OrderDetailDto>>(orderDetailsJson);
                }
                else
                {
                    orderDetails = new List<OrderDetailDto>();
                }

                var existingOrderDetail = orderDetails.FirstOrDefault(o => o.ProductId == findProduct.Id);
                if (existingOrderDetail != null)
                {
                    // Sản phẩm đã tồn tại trong giỏ hàng, tăng số lượng
                    existingOrderDetail.Quantity++;
                    existingOrderDetail.TotalPrice = existingOrderDetail.Quantity * findProduct.UnitPrice;
                }
                else
                {
                    // Sản phẩm chưa tồn tại trong giỏ hàng, thêm mới
                    var orderDetail = new OrderDetailDto
                    {
                        ProductId = findProduct.Id,
                        ProductName = findProduct.ProductName,
                        Quantity = 1,
                        TotalPrice = findProduct.UnitPrice,
                        Image = findProduct.Image
                    };
                    orderDetails.Add(orderDetail);
                }

                orderDetailsJson = JsonSerializer.Serialize(orderDetails);
                HttpContext.Session.SetString("OrderDetail_Show", orderDetailsJson);
            }

            return Redirect("/Cart");
        }
    }

}
