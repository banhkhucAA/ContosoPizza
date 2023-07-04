using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContosoPizza.Data;
using ContosoPizza.Models;
using ContosoPizza.Models.Generated;

namespace ContosoPizza.Pages.DeliveryMethods
{
    public class EditModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;

        public EditModel(ContosoPizza.Data.ContosoPizzaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public DeliveryMethod DeliveryMethod { get; set; } = default!;
        [BindProperty]
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.DeliveryMethods == null)
            {
                return NotFound();
            }

            var deliverymethod =  await _context.DeliveryMethods.FirstOrDefaultAsync(m => m.Id == id);
            if (deliverymethod == null)
            {
                return NotFound();
            }
            DeliveryMethod = deliverymethod;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            var existDelimethod = await _context.DeliveryMethods.Where(c => c.Method == DeliveryMethod.Method && c.Id != DeliveryMethod.Id).FirstOrDefaultAsync();
            if (existDelimethod != null)
            {
                ErrorMessage = "This delivery name: " + DeliveryMethod.Method + " has already existed";
                return await OnGetAsync(DeliveryMethod.Id);
            }
            _context.Attach(DeliveryMethod).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeliveryMethodExists(DeliveryMethod.Id))
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

        private bool DeliveryMethodExists(int id)
        {
          return (_context.DeliveryMethods?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
