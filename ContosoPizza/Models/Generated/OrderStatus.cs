using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ContosoPizza.Models.Generated
{
    public class OrderStatus
    {
        public OrderStatus()
        {
            Orders = new HashSet<Order>();
        }

        [Key]
        public int Id { get; set; }
        [StringLength(10)]
        [Unicode(false)]
        public string StatusName { get; set; } = null!;
        public bool IsActive { get; set; }

        [InverseProperty("OrderStatus")]
        public virtual ICollection<Order> Orders { get; set; }
    }
}
