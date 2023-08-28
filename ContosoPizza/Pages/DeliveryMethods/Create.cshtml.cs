using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ContosoPizza.Data;
using ContosoPizza.Models;
using ContosoPizza.Models.Generated;
using Microsoft.EntityFrameworkCore;

namespace ContosoPizza.Pages.DeliveryMethods
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
        public DeliveryMethod DeliveryMethod { get; set; } = default!;
        [BindProperty]
        public string ErrorMessage { get; set; }


        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
          if (_context.DeliveryMethods == null || DeliveryMethod == null)
            {
                return Page();
            }
            var existDeliveryName = await _context.DeliveryMethods.Where(c => c.Method == DeliveryMethod.Method).FirstOrDefaultAsync();
            if (existDeliveryName != null)
            {
                ErrorMessage = "This delivery method name: " + DeliveryMethod.Method + " has already existed";
                return Page();
            }
            _context.DeliveryMethods.Add(DeliveryMethod);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
