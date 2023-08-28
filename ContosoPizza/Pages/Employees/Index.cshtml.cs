using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ContosoPizza.Data;
using ContosoPizza.Models.Generated;
using Microsoft.AspNetCore.Mvc.Rendering;
using Castle.Core.Internal;

namespace ContosoPizza.Pages.Employees
{
    public class IndexModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;

        public IndexModel(ContosoPizza.Data.ContosoPizzaContext context)
        {
            _context = context;
        }
        [BindProperty(SupportsGet = true)]
        public string SearchName { get; set; }

        public IList<Employee> Employee { get;set; } = default!;
        public async Task OnGetAsync()
        {
            if (_context.Employees != null)
            {
                Employee = await _context.Employees.ToListAsync();
            }

            if(!SearchName.IsNullOrEmpty()) 
            {
                Employee = await _context.Employees
                        .Where(employee => employee.FirstName.Contains(SearchName) || employee.LastName.Contains(SearchName))
                        .ToListAsync();
            }
        }
    }
}
