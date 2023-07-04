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

namespace ContosoPizza.Pages.OrderStatuses
{
    public class EditModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;

        public EditModel(ContosoPizza.Data.ContosoPizzaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public OrderStatus OrderStatus { get; set; } = default!;
        [BindProperty]
        public string ErrorMessage { get; set; }
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.OrderStatuses == null)
            {
                return NotFound();
            }

            var orderstatus =  await _context.OrderStatuses.FirstOrDefaultAsync(m => m.Id == id);
            if (orderstatus == null)
            {
                return NotFound();
            }
            OrderStatus = orderstatus;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            var existOrderStatName = await _context.OrderStatuses.Where(c => c.StatusName == OrderStatus.StatusName && c.Id != OrderStatus.Id).FirstOrDefaultAsync();
            if (existOrderStatName != null)
            {
                ErrorMessage = "This order status: " + OrderStatus.StatusName + " has already existed";
                return await OnGetAsync(OrderStatus.Id);
            }
            _context.Attach(OrderStatus).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderStatusExists(OrderStatus.Id))
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

        private bool OrderStatusExists(int id)
        {
          return (_context.OrderStatuses?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
