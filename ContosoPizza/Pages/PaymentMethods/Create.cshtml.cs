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

namespace ContosoPizza.Pages.PaymentMethods
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
        public PaymentMethod PaymentMethod { get; set; } = default!;
        [BindProperty]
        public string ErrorMessage { get; set; }


        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
          if ( _context.PaymentMethods == null || PaymentMethod == null)
            {
                return Page();
            }
            var existPaymentName = await _context.PaymentMethods.Where(c => c.Method == PaymentMethod.Method).FirstOrDefaultAsync();
            if (existPaymentName != null)
            {
                ErrorMessage = "This payment method name: " + PaymentMethod.Method + " has already existed";
                return Page();
            }
            _context.PaymentMethods.Add(PaymentMethod);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}

