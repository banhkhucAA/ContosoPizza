using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ContosoPizza.Data;
using ContosoPizza.Models.Generated;

namespace ContosoPizza.Pages.Employees
{
    public class DeleteModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;
        private readonly IWebHostEnvironment _webHost;
        public DeleteModel(ContosoPizza.Data.ContosoPizzaContext context, IWebHostEnvironment webHost)
        {
            _context = context;
            _webHost = webHost;
        }

        [BindProperty]
      public Employee Employee { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.Employees == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null)
            {
                return NotFound();
            }
            else 
            {
                Employee = employee;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null || _context.Employees == null)
            {
                return NotFound();
            }
            var employee = await _context.Employees.FindAsync(id);
            Console.WriteLine("Web root path: " + _webHost.WebRootPath);
            Console.WriteLine("employee.Image: " + employee.Image);
            string oldFilePath = Path.Combine(_webHost.WebRootPath, employee.Image.TrimStart('/'));

            if (employee != null && System.IO.File.Exists(oldFilePath))
            {
                Employee = employee;
                var order_createdby_deleteEmps = _context.Orders.Where(or => or.EmployeeId == id).ToList();
                foreach(var item in order_createdby_deleteEmps)
                {
                    item.EmployeeId = null;
                    _context.Orders.Update(item);
                }    
                _context.Employees.Remove(Employee);
                System.IO.File.Delete(oldFilePath);
                await _context.SaveChangesAsync();
            }else
            {

                Console.WriteLine("Picture Errors, current oldFilePath: "+oldFilePath);
            }    

            return RedirectToPage("./Index");
        }
    }
}
