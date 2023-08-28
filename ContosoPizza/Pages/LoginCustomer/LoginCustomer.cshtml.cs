using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ContosoPizza.Pages.LoginCustomer
{
    public class LoginCustomer : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;

        public LoginCustomer(ContosoPizza.Data.ContosoPizzaContext context)
        {
            _context = context;
        }
        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [BindProperty]
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync(string email, string password)
        {
            // Kiểm tra thông tin đăng nhập và xử lý logic tại đây
            var customer = await _context.Customers.FirstOrDefaultAsync(m => m.Email == email && m.Pass == password);
            if(customer == null) 
            { 
                TempData["ErrorMessage"] = "Login failed. Your email or password is incorrect";
                return Page();
            };
            var userRole = "Customer";
            var userID = customer.Id;
            var userName = customer.FirstName + " " + customer.LastName;
            HttpContext.Session.SetString("UserRole", userRole);
            HttpContext.Session.SetInt32("UserId", userID);
            HttpContext.Session.SetString("UserName", userName);
            return RedirectToPage("/Home/Home");
        }
    }
}
