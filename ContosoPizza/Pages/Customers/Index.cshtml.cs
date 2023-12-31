﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ContosoPizza.Data;
using ContosoPizza.Models.Generated;
using System.Drawing.Printing;
using Microsoft.AspNetCore.Mvc.Rendering;
using Castle.Core.Internal;

namespace ContosoPizza.Pages.Customers
{
    public class IndexModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;
        private readonly IHttpContextAccessor _contextAccessor;

        public IndexModel(ContosoPizza.Data.ContosoPizzaContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;
        }

        public IList<Customer> Customer { get; set; } = default!;
        public string ErrorMessage { get; set; } = default !;
        [BindProperty(SupportsGet = true)]
        public string Email { get; set; }
        public int Page { get;set; }

        public int pageSize = 10;

        public async Task OnGetAsync()
        {

            if (Request.Query.ContainsKey("page"))
            {
                int.TryParse(Request.Query["page"], out int page);
                Page = Math.Max(page, 1); // Đảm bảo giá trị trang không nhỏ hơn 1
            }
            else
            {
                Page = 1; // Trang mặc định là 1
            }

            Customer = _context.Customers.ToList();

            if (!Email.IsNullOrEmpty())
            {
                Customer = Customer
                    .Where(e => e.Email.Contains(Email))
                    .ToList();
            }    

            if (_context.Customers != null)
            {
                int offset = Math.Max((Page - 1) * pageSize, 0);
                Customer = Customer
                .Skip(offset)
                    .Take(pageSize)
                    .ToList();
            }

            ErrorMessage = _contextAccessor.HttpContext.Session.GetString("ErrorMessage");
            Console.WriteLine("Error Message currently is: " + ErrorMessage);
            _contextAccessor.HttpContext.Session.Remove("ErrorMessage");
        }
    }
}
