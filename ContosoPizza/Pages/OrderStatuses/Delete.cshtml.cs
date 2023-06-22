using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ContosoPizza.Data;
using ContosoPizza.Models.Generated;

namespace ContosoPizza.Pages.OrderStatuses
{
    public class DeleteModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;

        public DeleteModel(ContosoPizza.Data.ContosoPizzaContext context)
        {
            _context = context;
        }

        [BindProperty]
      public OrderStatus OrderStatus { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.OrderStatuses == null)
            {
                return NotFound();
            }

            var orderstatus = await _context.OrderStatuses.FirstOrDefaultAsync(m => m.Id == id);

            if (orderstatus == null)
            {
                return NotFound();
            }
            else 
            {
                OrderStatus = orderstatus;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null || _context.OrderStatuses == null)
            {
                return NotFound();
            }
            var orderstatus = await _context.OrderStatuses.FindAsync(id);

            if (orderstatus != null)
            {
                OrderStatus = orderstatus;
                _context.OrderStatuses.Remove(OrderStatus);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
