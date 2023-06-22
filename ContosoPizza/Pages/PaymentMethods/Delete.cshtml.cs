﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ContosoPizza.Data;
using ContosoPizza.Models.Generated;

namespace ContosoPizza.Pages.PaymentMethods
{
    public class DeleteModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;

        public DeleteModel(ContosoPizza.Data.ContosoPizzaContext context)
        {
            _context = context;
        }

        [BindProperty]
      public PaymentMethod PaymentMethod { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.PaymentMethods == null)
            {
                return NotFound();
            }

            var paymentmethod = await _context.PaymentMethods.FirstOrDefaultAsync(m => m.Id == id);

            if (paymentmethod == null)
            {
                return NotFound();
            }
            else 
            {
                PaymentMethod = paymentmethod;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null || _context.PaymentMethods == null)
            {
                return NotFound();
            }
            var paymentmethod = await _context.PaymentMethods.FindAsync(id);

            if (paymentmethod != null)
            {
                PaymentMethod = paymentmethod;
                _context.PaymentMethods.Remove(PaymentMethod);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
