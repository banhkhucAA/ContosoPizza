using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ContosoPizza.Data;
using ContosoPizza.Models.Generated;

namespace ContosoPizza.Pages.Products
{
    public class DetailsModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;

        public DetailsModel(ContosoPizza.Data.ContosoPizzaContext context)
        {
            _context = context;
        }

      public Product Product { get; set; } = default!;
        public int skippedcount = 0;
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            else 
            {
                skippedcount = await _context.Products.CountAsync(p => p.Id < id);
                HttpContext.Session.SetInt32("SkippedCount", skippedcount);
                Product = product;
            }
            return Page();
        }

        /*public async Task<IActionResult> OnPostAsync()
        {
            var skippedCount = HttpContext.Session.GetInt32("SkippedCount");
            Console.WriteLine("Số bản ghi đã skip: " + skippedcount);
            var currentPage = skippedCount / 5 + 1;
            if (!ModelState.IsValid)
            {
                return Page();
            }
            return RedirectToPage("./Index", new { page = currentPage });
        }*/
    }
}
