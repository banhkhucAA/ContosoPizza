using ContosoPizza.Data;
using Microsoft.EntityFrameworkCore;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();

        builder.Services.AddDbContext<ContosoPizzaContext>(options =>
        options
        .UseLazyLoadingProxies()
        .UseSqlServer(builder.Configuration.GetConnectionString("ContosoPizza")));

        builder.Services.AddSession();
        builder.Services.AddHttpContextAccessor();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapRazorPages();

        app.UseSession();
        app.UseMiddleware<AuthenticationMiddleware>();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
        });

        app.Run();
    }
}