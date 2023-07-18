using ContosoPizza.Models.Generated;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace ContosoPizza.Pages.StatisticalReport
{
    public class ReportFoodAndDrinksModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ReportFoodAndDrinksModel(ContosoPizza.Data.ContosoPizzaContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public IList<FoodAndDrinksDto> ReportFoodAndDrinks { get; set; }
        public int Page { get; set; }
        public int pageSize = 5;
        [BindProperty(SupportsGet = true)]
        public DateTime FromDate { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime ToDate { get; set; }
        [BindProperty(SupportsGet = true)]
        public string orderBy { get; set; }
        public List<FoodAndDrinksDto> Products { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if(!Request.Query.ContainsKey("fromdate") && !Request.Query.ContainsKey("todate"))
            {
                Products = (from p in _context.Products.ToList()
                            join ordt in _context.OrderDetails.ToList() on p.Id equals ordt.ProductId into pordt
                            select new FoodAndDrinksDto
                            {
                                Id = p.Id,
                                Name = p.ProductName,
                                UnitPrice = (float)Math.Round(p.UnitPrice, 2),
                                OrderedNumber = pordt.Sum(p => p.Quantity),
                                OrderIncludeProduct = pordt.Count(),
                                TotalMoney = (float)Math.Round((decimal)pordt.Sum(p => p.TotalPrice), 2),
                                RealMoneyEarn = (float)Math.Round((decimal)pordt.Where(ordt => ordt.Order.OrderStatus.StatusName == "Delivered").Sum(p => p.TotalPrice), 2),
                                Image = p.Image
                            }).ToList();

            }

            if (Request.Query.ContainsKey("fromdate")&& !Request.Query.ContainsKey("todate"))
            {
                var selectedOrderDetails = _context.OrderDetails.Where(o => o.Order.OrderPlacedAt >= FromDate).Select(o => o.ProductId).Distinct().ToList();
                List<Product> ProductList = new List<Product>();
                foreach (var item in selectedOrderDetails)
                {
                    var findProducts = await _context.Products.Where(p => p.Id == item).FirstOrDefaultAsync();
                    if (findProducts != null)
                    {
                        ProductList.Add(findProducts);
                    }
                }

                Products = (from p in ProductList.ToList()
                            join ordt in _context.OrderDetails.ToList() on p.Id equals ordt.ProductId into pordt
                            select new FoodAndDrinksDto
                            {
                                Id = p.Id,
                                Name = p.ProductName,
                                UnitPrice = (float)p.UnitPrice,
                                OrderedNumber = pordt.Sum(p => p.Quantity),
                                OrderIncludeProduct = pordt.Count(),
                                TotalMoney = (float)Math.Round((decimal)pordt.Sum(p => p.TotalPrice), 2),
                                RealMoneyEarn = (float)Math.Round((decimal)pordt.Where(ordt => ordt.Order.OrderStatus.StatusName == "Delivered").Sum(p => p.TotalPrice), 2),
                                Image = p.Image
                            }).ToList();

            }

            if (!Request.Query.ContainsKey("fromdate") && Request.Query.ContainsKey("todate"))
            {
                var selectedOrderDetails = _context.OrderDetails.Where(o => o.Order.OrderPlacedAt <= ToDate).Select(o=>o.ProductId).Distinct().ToList();
                List<Product> ProductList = new List<Product>();
                foreach(var item in selectedOrderDetails) 
                {
                    var findProducts = await _context.Products.Where(p => p.Id == item).FirstOrDefaultAsync();
                    if (findProducts != null) 
                    {
                        ProductList.Add(findProducts);
                    }
                }

                Products = (from p in ProductList.ToList()
                            join ordt in _context.OrderDetails.ToList() on p.Id equals ordt.ProductId into pordt
                            select new FoodAndDrinksDto
                            {
                                Id = p.Id,
                                Name = p.ProductName,
                                UnitPrice = (float)p.UnitPrice,
                                OrderedNumber = pordt.Sum(p => p.Quantity),
                                OrderIncludeProduct = pordt.Count(),
                                TotalMoney = (float)Math.Round((decimal)pordt.Sum(p => p.TotalPrice), 2),
                                RealMoneyEarn = (float)Math.Round((decimal)pordt.Where(ordt => ordt.Order.OrderStatus.StatusName == "Delivered").Sum(p => p.TotalPrice), 2),
                                Image = p.Image
                            }).ToList();

            }

            if (Request.Query.ContainsKey("fromdate") && Request.Query.ContainsKey("todate"))
            {
                var selectedOrderDetails = _context.OrderDetails.Where(o => o.Order.OrderPlacedAt >= FromDate && o.Order.OrderPlacedAt <= ToDate).Select(o => o.ProductId).Distinct().ToList();
                List<Product> ProductList = new List<Product>();
                foreach (var item in selectedOrderDetails)
                {
                    var findProducts = await _context.Products.Where(p => p.Id == item).FirstOrDefaultAsync();
                    if (findProducts != null)
                    {
                        ProductList.Add(findProducts);
                    }
                }

                Products = (from p in ProductList.ToList()
                            join ordt in _context.OrderDetails.ToList() on p.Id equals ordt.ProductId into pordt
                            select new FoodAndDrinksDto
                            {
                                Id = p.Id,
                                Name = p.ProductName,
                                UnitPrice = (float)p.UnitPrice,
                                OrderedNumber = pordt.Sum(p => p.Quantity),
                                OrderIncludeProduct = pordt.Count(),
                                TotalMoney = (float)Math.Round((decimal)pordt.Sum(p => p.TotalPrice), 2),
                                RealMoneyEarn = (float)Math.Round((decimal)pordt.Where(ordt => ordt.Order.OrderStatus.StatusName == "Delivered").Sum(p => p.TotalPrice), 2),
                                Image = p.Image
                            }).ToList();

            }

            if (Request.Query.ContainsKey("page"))
            {
                int.TryParse(Request.Query["page"], out int page);
                Page = Math.Max(page, 1); // Đảm bảo giá trị trang không nhỏ hơn 1
            }
            else
            {
                Page = 1; // Trang mặc định là 1
            }

            if (Request.Query.ContainsKey("orderBy"))
            {
                if (orderBy == "UnitPrice")
                    Products = Products.OrderByDescending(p => p.UnitPrice).ToList();
                if (orderBy == "OrderedNumber")
                    Products = Products.OrderByDescending(p => p.OrderedNumber).ToList();
                if (orderBy == "OrderIncludeProduct")
                    Products = Products.OrderByDescending(p => p.OrderIncludeProduct).ToList();
                if (orderBy == "TotalMoney")
                    Products = Products.OrderByDescending(p => p.TotalMoney).ToList();
                if (orderBy == "RealMoneyEarn")
                    Products = Products.OrderByDescending(p => p.RealMoneyEarn).ToList();
            }

            if (_context.Orders != null)
            {
                int offset = Math.Max((Page - 1) * pageSize, 0);
                ReportFoodAndDrinks = Products
                .Skip(offset)
                    .Take(pageSize)
                    .ToList();
            }


            return Page();
        }
    }
}
