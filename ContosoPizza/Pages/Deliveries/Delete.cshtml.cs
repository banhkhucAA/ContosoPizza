using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ContosoPizza.Data;
using ContosoPizza.Models;

namespace ContosoPizza.Pages.NewFolder
{
    public class DeleteModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;

        public DeleteModel(ContosoPizza.Data.ContosoPizzaContext context)
        {
            _context = context;
        }

        [BindProperty]
      public DeliveryMethod DeliveryMethod { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.DeliveryMethods == null)
            {
                return NotFound();
            }

            var deliverymethod = await _context.DeliveryMethods.FirstOrDefaultAsync(m => m.Id == id);

            if (deliverymethod == null)
            {
                return NotFound();
            }
            else 
            {
                DeliveryMethod = deliverymethod;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null || _context.DeliveryMethods == null)
            {
                return NotFound();
            }
            var deliverymethod = await _context.DeliveryMethods.FindAsync(id);

            if (deliverymethod != null)
            {
                DeliveryMethod = deliverymethod;
                _context.DeliveryMethods.Remove(DeliveryMethod);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
