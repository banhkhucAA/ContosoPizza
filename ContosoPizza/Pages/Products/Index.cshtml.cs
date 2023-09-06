using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ContosoPizza.Data;
using ContosoPizza.Models.Generated;
using System.Drawing.Printing;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace ContosoPizza.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;

        public IndexModel(ContosoPizza.Data.ContosoPizzaContext context)
        {
            _context = context;
        }

        public IList<Product> Product { get;set; } = default!;
        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }
        public SelectList? Categories { get; set; }
        public SelectList? FoodType { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? Food_Type { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? Food_Category { get; set; }
        [BindProperty]
        public List<OrderDetailDto> OrderDetail_Show { get; set; }
        [BindProperty]
        public string ErrorMessage { get; set; }
        

        public int Page { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(SearchString))
            {
                query = query.Where(x => x.ProductName.Contains(SearchString) || x.Materials.Contains(SearchString));
            }

            if (!string.IsNullOrEmpty(Food_Type))
            {               
                if (Food_Type == "CarbonatedDrink")
                    query = query.Where(f => f.ProductName.Contains("Carbonated") || f.Materials.Contains("Carbonated"));
                else if (Food_Type == "FreshJuice")
                    query = query.Where(f => f.ProductName.Contains("Juice") || f.Materials.Contains("Juice"));
                else
                    query = query.Where(f => f.ProductName.Contains(Food_Type) || f.Materials.Contains(Food_Type));

            }

            if (!string.IsNullOrEmpty(Food_Category))
            {
                var category_id = await _context.Categories
                    .Where(x => x.CategoryName == Food_Category)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync();

                query = query.Where(f => f.CategoryId == category_id);
            }

            var category_list = await _context.Categories.Select(cat=>cat.CategoryName).ToListAsync();

            Categories = new SelectList(category_list);

            int pageSize = 6;

            if (Request.Query.ContainsKey("Page"))
            {
                int.TryParse(Request.Query["Page"], out int page);
                Page = Math.Max(page, 1); // Đảm bảo giá trị trang không nhỏ hơn 1
            }
            else
            {
                Page = 1; // Trang mặc định là 1
            }

            if (_context.Products != null)
            {
                int offset = Math.Max((Page - 1) * pageSize, 0);
                Product = await query
                    .Skip(offset)
                    .Take(pageSize)
                    .ToListAsync();
            }
            TempData["ErrorMessage"] = HttpContext.Session.GetString("ErrorMessage");
            HttpContext.Session.Remove("ErrorMessage");
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
