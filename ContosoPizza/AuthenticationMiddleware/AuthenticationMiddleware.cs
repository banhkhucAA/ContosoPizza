using ContosoPizza.Models.Generated;
using ContosoPizza.Pages.LoginCustomer;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, ContosoPizza.Data.ContosoPizzaContext _context)
    {
        //chưa đăng nhập thì có 3 loại n dùng: Admin, Employee, Customer
        // Kiểm tra xem người dùng đã đăng nhập hay chưa
        if (!context.Session.Keys.Contains("UserRole") && !context.Request.Path.StartsWithSegments("/LoginEmployee") && !context.Request.Path.StartsWithSegments("/Products")&&!context.Request.Path.StartsWithSegments("/LoginCustomer") && !context.Request.Path.StartsWithSegments("/Cart")&&!context.Request.Path.StartsWithSegments("/Orders/Create")&&!context.Request.Path.StartsWithSegments("/Customers/Create") && !context.Request.Path.StartsWithSegments("/EmailFormat") && !context.Request.Path.StartsWithSegments("/StatisticalReport") && !context.Request.Path.StartsWithSegments("/Home"))
        {
            // Người dùng chưa đăng nhập, điều hướng đến trang đăng nhập
            context.Response.Redirect("/LoginEmployee");
            return;
        }   
        else if(context.Session.Keys.Contains("UserRole"))
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
                }else if(context.Request.Path.StartsWithSegments("/LoginEmployee"))
                {
                    string errorMessage = "You have already logged in ";
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
                else if (context.Request.Path.StartsWithSegments("/LoginEmployee") || context.Request.Path.StartsWithSegments("/LoginCustomer"))
                {
                    string errorMessage = "You have already logged in as an employee";
                    context.Session.SetString("ErrorMessage", errorMessage);
                    context.Response.Redirect("/Customers/Index");
                    return;
                }
            }
            else if (userRole == "Customer")
            {
                string errorMessage;               
                if (context.Request.Path.StartsWithSegments("/Customers/Details") || context.Request.Path.StartsWithSegments("/Customers/Delete") || context.Request.Path.StartsWithSegments("/DeliveryMethods") || context.Request.Path.StartsWithSegments("/PaymentMethods") || context.Request.Path.StartsWithSegments("/OrderStatuses") || context.Request.Path.StartsWithSegments("/Categories") || context.Request.Path.StartsWithSegments("/Employees") || context.Request.Path.StartsWithSegments("/Orders/Details") || context.Request.Path.StartsWithSegments("/Orders/Edit"))
                 {                   
                    errorMessage = "You are not allowed to get access to this function";
                    context.Session.SetString("ErrorMessage", errorMessage);
                    context.Response.Redirect("/Products/Index");
                    return;
                 }
                 else if (context.Request.Path.StartsWithSegments("/LoginCustomer"))
                 {
                    errorMessage = "You have already logged in ";
                    context.Session.SetString("ErrorMessage", errorMessage);
                    context.Response.Redirect("/Products/Index");
                    return;
                 }

                var Ids = await _context.Customers.Where(c => c.Id != context.Session.GetInt32("UserId")).ToListAsync();
                foreach (var item in Ids)
                {
                    string url = $"/Customers/Edit?id={item.Id}";

                    if (context.Request.Path + context.Request.QueryString == url)
                    {
                        errorMessage = "You are not allowed to get access to other customers' information";
                        context.Session.SetString("ErrorMessage", errorMessage);
                        context.Response.Redirect("/Products/Index");
                        return;
                    }
                }

                var OrdersById = await _context.Orders.Where(or=>or.CustomerId!=context.Session.GetInt32("UserId")).ToListAsync();
                foreach(var item in OrdersById)
                {
                    string url = "/OrderDetails/Create?orderId=" + item.Id;
                    if (context.Request.Path + context.Request.QueryString == url)
                    {
                        errorMessage = "You are not allowed to get access to other customers' orders";
                        context.Session.SetString("ErrorMessage", errorMessage);
                        context.Response.Redirect("/Products/Index");
                        return;
                    }
                }    
            }
        }

        // Người dùng đã đăng nhập, tiếp tục xử lý request
        await _next(context);
    }
}
