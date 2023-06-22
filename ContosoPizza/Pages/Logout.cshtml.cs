using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ContosoPizza.Pages
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnPostAsync()
        {
            // Đăng xuất người dùng và xóa thông tin xác thực từ cookie
            HttpContext.Session.Remove("UserRole");
            HttpContext.Session.Clear();
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Chuyển hướng đến trang khác sau khi đăng xuất thành công
            return Redirect("/Products");
        }
    }
}
