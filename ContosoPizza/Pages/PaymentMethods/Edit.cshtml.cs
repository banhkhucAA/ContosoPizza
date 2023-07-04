using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContosoPizza.Data;
using ContosoPizza.Models.Generated;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;

namespace ContosoPizza.Pages.PaymentMethods
{
    public class EditModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;

        public EditModel(ContosoPizza.Data.ContosoPizzaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public PaymentMethod PaymentMethod { get; set; } = default!;
        [BindProperty]
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.PaymentMethods == null)
            {
                return NotFound();
            }

            var paymentmethod =  await _context.PaymentMethods.FirstOrDefaultAsync(m => m.Id == id);
            if (paymentmethod == null)
            {
                return NotFound();
            }
            PaymentMethod = paymentmethod;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            var existPaymentMethod = await _context.PaymentMethods.Where(c => c.Method == PaymentMethod.Method && c.Id != PaymentMethod.Id).FirstOrDefaultAsync();
            if (existPaymentMethod != null)
            {
                ErrorMessage = "This payment method: " + PaymentMethod.Method + " has already existed";
                return await OnGetAsync(PaymentMethod.Id);
            }
            _context.Attach(PaymentMethod).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaymentMethodExists(PaymentMethod.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool PaymentMethodExists(int id)
        {
          return (_context.PaymentMethods?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
