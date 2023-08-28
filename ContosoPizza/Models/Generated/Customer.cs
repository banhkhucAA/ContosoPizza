using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ContosoPizza.Models.Generated
{
    public class Customer
    {
        public Customer()
        {
            Orders = new HashSet<Order>();
        }

        [Key]
        public int Id { get; set; }
        [StringLength(10)]
        [Unicode(false)]
        public string FirstName { get; set; } = null!;
        [StringLength(10)]
        [Unicode(false)]
        public string LastName { get; set; } = null!;
        [Unicode(false)]
        public string Address { get; set; } = null!;
        [StringLength(10)]
        [Unicode(false)]
        public string Phone { get; set; } = null!;
        [Unicode(false)]
        public string Email { get; set; } = null!;
        [StringLength(10)]
        [Unicode(false)]
        public string Pass { get; set; } = null!;

        [InverseProperty("Customer")]
        public virtual ICollection<Order> Orders { get; set; }
    }
}
