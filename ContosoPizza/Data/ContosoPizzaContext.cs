using ContosoPizza.Models;
using ContosoPizza.Models.Generated;
using Microsoft.EntityFrameworkCore;

namespace ContosoPizza.Data
{
    public class ContosoPizzaContext:DbContext
    {
        public ContosoPizzaContext(DbContextOptions<ContosoPizzaContext> options) : base(options) { }
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Coupon> Coupons { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<DeliveryMethod> DeliveryMethods { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderDetail> OrderDetails { get; set; } = null!;
        public DbSet<OrderStatus> OrderStatuses { get; set; } = null!;
        public DbSet<PaymentMethod> PaymentMethods { get; set; } = null!;   
        public DbSet<Product> Products { get; set; } = null!;

    }
}
