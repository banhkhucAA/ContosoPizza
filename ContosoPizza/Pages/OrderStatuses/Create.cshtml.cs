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

namespace ContosoPizza.Pages.OrderStatuses
{
    public class CreateModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;

        public CreateModel(ContosoPizza.Data.ContosoPizzaContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public OrderStatus OrderStatus { get; set; } = default!;
        [BindProperty]
        public string ErrorMessage { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
          if (_context.OrderStatuses == null || OrderStatus == null)
            {
                return Page();
            }

            var existStatusName = await _context.OrderStatuses.Where(c => c.StatusName == OrderStatus.StatusName).FirstOrDefaultAsync();
            if (existStatusName != null)
            {
                ErrorMessage = "This Category Name: " + OrderStatus.StatusName + " has already existed";
                return Page();
            }

            _context.OrderStatuses.Add(OrderStatus);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
