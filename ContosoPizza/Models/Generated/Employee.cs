using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoPizza.Models.Generated
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }
        [StringLength(10)]
        [Unicode(false)]
        public string FirstName { get; set; } = null!;
        [StringLength(10)]
        [Unicode(false)]
        public string LastName { get; set; } = null!;
        [StringLength(30)]
        [Unicode(false)]
        public string Address { get; set; } = null!;
        [StringLength(10)]
        [Unicode(false)]
        public string Phone { get; set; } = null!;
        [StringLength(30)]
        [Unicode(false)]
        public string Email { get; set; } = null!;
        [StringLength(10)]
        [Unicode(false)]
        public string Pass { get; set; } = null!;
        [StringLength(30)]
        [Unicode(false)]
        public string Role { get; set; } = null!;
        public string? Image { get; set; }
        [InverseProperty("Employee")]
        public virtual ICollection<Order> Orders { get;set; }
    }
}
