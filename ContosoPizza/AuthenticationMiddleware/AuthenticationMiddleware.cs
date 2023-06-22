public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        // Kiểm tra xem người dùng đã đăng nhập hay chưa
        if (!context.Session.Keys.Contains("UserRole") && !context.Request.Path.StartsWithSegments("/LoginEmployee") && !context.Request.Path.StartsWithSegments("/Products")&&!context.Request.Path.StartsWithSegments("/LoginCustomer") && !context.Request.Path.StartsWithSegments("/Cart")&&!context.Request.Path.StartsWithSegments("/Orders/Create"))
        {
            // Người dùng chưa đăng nhập, điều hướng đến trang đăng nhập
            context.Response.Redirect("/LoginEmployee");
            return;
        }   
        else
        {
            var userRole = context.Session.GetString("UserRole");

            if (userRole == "Employee")
            {
                // Kiểm tra truy cập vào trang của Admin
                if (context.Request.Path.StartsWithSegments("/Customers")|| context.Request.Path.StartsWithSegments("/DeliveryMethods") || context.Request.Path.StartsWithSegments("/PaymentMethods") || context.Request.Path.StartsWithSegments("/OrderStatuses"))
                {
                    string errorMessage = "You are not allowed to get access to this function";
                    context.Session.SetString("ErrorMessage", errorMessage);
                    context.Response.Redirect("/Orders/Index");
                    return;
                }
            }
            else if (userRole == "Admin")
            {
                // Kiểm tra truy cập vào trang của Employee
                if (context.Request.Path.StartsWithSegments("/Orders"))
                {
                    string errorMessage = "Create an employee account to see this function";
                    context.Session.SetString("ErrorMessage", errorMessage);
                    context.Response.Redirect("/Customers/Index"); // Điều hướng đến trang truy cập bị từ chối
                    return;
                }
            }
        }

        // Người dùng đã đăng nhập, tiếp tục xử lý request
        await _next(context);
    }
}
