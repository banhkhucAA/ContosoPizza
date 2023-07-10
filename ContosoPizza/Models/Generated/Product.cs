using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ContosoPizza.Models.Generated
{
    public class Product
    {
        public Product()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        [Key]
        public int Id { get; set; }
        [StringLength(20)]
        [Unicode(false)]
        public string ProductName { get; set; } = null!;
        public string Description { get; set; }
        public string Materials { get;set; } = null!;
        public double UnitPrice { get; set; }

        [Column("CategoryID")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        [InverseProperty("Products")]
        public virtual Category? Category { get; set; }
        //
        [InverseProperty("Product")]
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public string Image { get; set; }
        //
    }
}