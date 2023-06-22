using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ContosoPizza.Models.Generated
{
    public class Coupon
    {
        [Key]
        public int Id { get; set; }
        [StringLength(10)]
        [Unicode(false)]
        public string CouponCode { get; set; } = null!;
        public double DiscountAmount { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? ExpireDate { get; set; }

        [InverseProperty("Coupon")]
        public virtual ICollection<Order> Orders { get; set; }
    }
}
