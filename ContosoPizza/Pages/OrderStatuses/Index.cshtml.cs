﻿using System;
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
    public class IndexModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;

        public IndexModel(ContosoPizza.Data.ContosoPizzaContext context)
        {
            _context = context;
        }

        public IList<OrderStatus> OrderStatus { get;set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.OrderStatuses != null)
            {
                OrderStatus = await _context.OrderStatuses.ToListAsync();
            }
        }
    }
}
