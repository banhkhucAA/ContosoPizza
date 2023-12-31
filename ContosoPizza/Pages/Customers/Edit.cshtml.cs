﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContosoPizza.Data;
using ContosoPizza.Models.Generated;

namespace ContosoPizza.Pages.Customers
{
    public class EditModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;

        public EditModel(ContosoPizza.Data.ContosoPizzaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Customer Customer { get; set; } = default!;
        [BindProperty]
        public string ErrorMessage { get; set; }
        public int skippedcount = 0;
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.Customers == null)
            {
                return NotFound();
            }

            var customer =  await _context.Customers.FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null)
            {
                return NotFound();
            }
            skippedcount = await _context.Products.CountAsync(p => p.Id < id);
            HttpContext.Session.SetInt32("SkippedCount", skippedcount);
            Customer = customer;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            var skippedCount = HttpContext.Session.GetInt32("SkippedCount");
            var curentPage = skippedCount / 5 + 1;

            var ExistEmail = await _context.Customers.FirstOrDefaultAsync(c => c.Email == Customer.Email&&c.Id!=Customer.Id);
            if (ExistEmail != null)
            {
                ErrorMessage = "This email has already been used";
                return await OnGetAsync(Customer.Id);
            }

            _context.Attach(Customer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(Customer.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            if (HttpContext.Session.GetString("UserRole") == "Admin")
                return Redirect($"./Index?Page={curentPage}");
            else
            {
                ErrorMessage = "Update your information successfully";
                return Page();
            }
        }

        private bool CustomerExists(int id)
        {
          return (_context.Customers?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
