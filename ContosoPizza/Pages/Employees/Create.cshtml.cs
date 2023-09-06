using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ContosoPizza.Data;
using ContosoPizza.Models.Generated;
using System.Data;
using Microsoft.AspNetCore.Hosting;

namespace ContosoPizza.Pages.Employees
{
    public class CreateModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CreateModel(ContosoPizza.Data.ContosoPizzaContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public SelectList Roles { get;set; }

        [BindProperty]
        public string ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            List<SelectListItem> ItemRoles = new List<SelectListItem>
            {
                new SelectListItem { Value = "Admin", Text = "Admin" },
                new SelectListItem { Value = "Employee", Text = "Employee" },
                new SelectListItem { Value = "Manager Employee", Text = "Manager Employee" },
            };
            Roles = new SelectList(ItemRoles, "Value", "Text");
            return Page();
        }

        [BindProperty]
        public Employee Employee { get; set; } = default!;        
        public SelectList Role { get; set; }

        public async Task<IActionResult> OnPostAsync(IFormFile imageFile)
        {
            var exist_employee_email = _context.Employees.Where(e=>e.Email==Employee.Email).Select(e=>e.Email).FirstOrDefault();
            if(exist_employee_email!=null) 
            {
                ErrorMessage = "This email has already been used to register";
                return OnGet();
            }
            bool IsValidImageFile(IFormFile imageFile)
            {
                if (imageFile == null || imageFile.Length == 0)
                {
                    return false;
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                return allowedExtensions.Contains(extension);
            }

            if (!IsValidImageFile(imageFile))
            {
                ErrorMessage = "Can't add your image because it is not in the correct format";
                return OnGet();
            }
            if (_context.Employees == null || Employee == null)
            {
                return Page();
            }

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "EmployeePictures");
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            // Lưu đường dẫn tới file ảnh vào trường ImagePath của model Image
            Employee.Image = "/EmployeePictures/" + uniqueFileName;

            _context.Employees.Add(Employee);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
