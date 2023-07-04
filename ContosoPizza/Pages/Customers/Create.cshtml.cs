using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ContosoPizza.Data;
using ContosoPizza.Models.Generated;
using Microsoft.EntityFrameworkCore;

namespace ContosoPizza.Pages.Customers
{
    public class CreateModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;

        public CreateModel(ContosoPizza.Data.ContosoPizzaContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGet()
        {
            skippedCount = await _context.Customers.CountAsync();
            HttpContext.Session.SetInt32("SkippedCount", skippedCount);
            return Page();
        }

        [BindProperty]
        public Customer Customer { get; set; } = default!;
        [BindProperty]
        public string ErrorMessage { get; set; }

        public int skippedCount = 0;
        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {  
            if (_context.Customers == null || Customer == null)
            {
                return Page();
            }
            var ExistEmail = await _context.Customers.FirstOrDefaultAsync(c=>c.Email==Customer.Email);
            if(ExistEmail!=null)
            {
                ErrorMessage = "This email has already been used";
                return await OnGet();
            }    

            var skippedCount = HttpContext.Session.GetInt32("SkippedCount");
            var curentPage = (skippedCount+1) / 5 + 1;

            _context.Customers.Add(Customer);
            await _context.SaveChangesAsync();

            return Redirect($"./Index?Page={curentPage}");

        }
    }
}
