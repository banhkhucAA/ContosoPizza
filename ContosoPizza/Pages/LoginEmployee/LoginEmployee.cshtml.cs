using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ContosoPizza.Pages.LoginEmployee
{
    public class LoginEmployeeModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;

        public LoginEmployeeModel(ContosoPizza.Data.ContosoPizzaContext context)
        {
            _context = context;
        }
        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        [DataType(DataType.Password)]
        public string Password { get; set; }


        public async Task<IActionResult>OnGetAsync()
        {
            TempData["ErrorMessage"] = HttpContext.Session.GetString("ErrorMessage");
            HttpContext.Session.Remove("ErrorMessage");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string email, string password)
        {           
            // Kiểm tra thông tin đăng nhập và xử lý logic tại đây
            var employee = await _context.Employees.FirstOrDefaultAsync(m => m.Email == email && m.Pass == password);
            if(employee == null) 
            {
                TempData["ErrorMessage"] = "Login failed. Your email or password is incorrect";
                return Page();
            };
            var userRole = employee.Role;
            var userID = employee.Id;
            var userName = employee.FirstName + " "+employee.LastName;
            var userImage = employee.Image;
            if (employee.Role == "Admin")
            {
                HttpContext.Session.SetString("UserRole", userRole);
                HttpContext.Session.SetString("UserName", userName);
                HttpContext.Session.SetInt32("UserId", userID);
                HttpContext.Session.SetString("UserImage", userImage);
                return RedirectToPage("/Employees/Index");
            }
            else if (employee.Role == "Employee")
            {
                HttpContext.Session.SetString("UserRole", userRole);
                HttpContext.Session.SetString("UserName", userName);
                HttpContext.Session.SetInt32("UserId", userID);
                HttpContext.Session.SetString("UserImage", userImage);
                return RedirectToPage("/Orders/Index");
            }
            else if (employee.Role == "Manager Employee")
            {
                userRole = "Employee";
                HttpContext.Session.SetString("UserRole", userRole);
                HttpContext.Session.SetString("UserName", userName);
                HttpContext.Session.SetInt32("UserId", userID);
                HttpContext.Session.SetString("UserImage", userImage);
                return RedirectToPage("/Orders/Index");
            }    
            else
            {
                Console.WriteLine("Khong co gi");
                return Page();
            }
        }
    }
}
