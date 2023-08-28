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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.Net.WebSockets;
using System.Data;

namespace ContosoPizza.Pages.Employees
{
    public class EditModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EditModel(ContosoPizza.Data.ContosoPizzaContext context, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _webHostEnvironment = webHostEnvironment;
        }

        [BindProperty]
        public Employee Employee { get; set; } = default!;
        [BindProperty]
        public string ErrorMessage { get; set; } = default!;
        public SelectList Roles { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.Employees == null)
            {
                return NotFound();
            }
            List<SelectListItem> ItemRoles = new List<SelectListItem>
            {
                new SelectListItem { Value = "Admin", Text = "Admin" },
                new SelectListItem { Value = "Employee", Text = "Employee" },
                new SelectListItem { Value = "Manager Employee", Text = "Manager Employee" },
            };
            Roles = new SelectList(ItemRoles, "Value", "Text");
            var employee = await _context.Employees.FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }
            Employee = employee;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync(IFormFile? imageFile, int? id)
        {
            //email_employee có id gốc, nếu Email ng dùng nhập trùng với email của bản ghi đang edit thì vẫn cho phép sửa
            var existingEmployee = await _context.Employees.FindAsync(id);

            var email_employee = await _context.Employees.Where(e=>e.Email==Employee.Email).FirstOrDefaultAsync();
            var not_email_employee = await _context.Employees.Where(e => e.Email != Employee.Email).ToListAsync();
            if (email_employee != null)
            {
                foreach(var item in not_email_employee)
                {
                    if(Employee.Id==item.Id) 
                    {
                        ErrorMessage = "This email " + email_employee.Email + " has already been used, can not edit";
                        return await OnGetAsync(id);
                    }
                }
            }

            if (existingEmployee == null)
            {
                return NotFound();
            }

            if (imageFile != null)
            {
                if (!IsValidImageFile(imageFile))
                {
                    ErrorMessage = "Can't add your image because it is not in the correct format of .png, .jpg or .jpeg";
                    return await OnGetAsync(id);
                }
                if (!string.IsNullOrEmpty(existingEmployee.Image))
                {
                    string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingEmployee.Image.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }else
                    {
                        Console.WriteLine("No Path, just add");
                    }    
                }
                // Lưu trữ tệp tin ảnh mới trong thư mục uploads
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "EmployeePictures");
                string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // Cập nhật đường dẫn tới tệp hình ảnh mới
                existingEmployee.Image = $"/EmployeePictures/{uniqueFileName}";
                Employee.Image = existingEmployee.Image;
            }
            else
            {
                if (string.IsNullOrEmpty(existingEmployee.Image))
                {
                    if (_httpContextAccessor.HttpContext.Session.GetString("UserRole") == "Admin")
                        return NotFound("You have to add your employess photos");
                    else
                        return NotFound("You have to add your photos, please contact to your Admin");
                }
                else
                {
                    // Giữ nguyên giá trị của trường Image
                    Employee.Image = existingEmployee.Image;
                }
            }
            _context.Entry(existingEmployee).State = EntityState.Detached;
            _context.Attach(Employee).State = EntityState.Modified;
            try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(Employee.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            if (_httpContextAccessor.HttpContext.Session.GetString("UserRole") == "Admin")
                return RedirectToPage("./Index");
            else if (_httpContextAccessor.HttpContext.Session.GetString("UserRole") == "Employee")
            {
                var userId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId").ToString();
                Console.WriteLine(userId);
            return Redirect($"/Employees/Edit?id={userId}");
            }
            else return NotFound("You are not allowed to access this");
            }

            private bool EmployeeExists(int id)
            {
                return (_context.Employees?.Any(e => e.Id == id)).GetValueOrDefault();
            }

            private bool IsValidImageFile(IFormFile imageFile)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                return allowedExtensions.Contains(extension);
            }
    }
}
