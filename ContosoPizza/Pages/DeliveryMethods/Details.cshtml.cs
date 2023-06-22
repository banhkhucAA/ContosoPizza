using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ContosoPizza.Data;
using ContosoPizza.Models;

namespace ContosoPizza.Pages.DeliveryMethods
{
    public class DetailsModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;

        public DetailsModel(ContosoPizza.Data.ContosoPizzaContext context)
        {
            _context = context;
        }

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
    }
}
