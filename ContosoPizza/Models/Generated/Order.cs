using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoPizza.Models.Generated
{
    public class Order
    {
        public Order()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? OrderPlacedAt { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? OrderFullFilledAt { get; set; }
        [Column("CustomerID")]
        public int? CustomerId { get; set; }

        // Thêm thuộc tính CouponId để lưu khóa ngoại đến Coupon
        [Column("CouponID")]
        public int? CouponId { get; set; }

        [Column("OrderStatusID")]
        public int OrderStatusId { get; set; }

        [Column("DeliveryMethodID")]
        public int DeliveryMethodId { get; set; }

        [Column("PaymentMethodID")]
        public int PaymentMethodId { get; set; }
        [Column("EmployeeId")]
        public int? EmployeeId { get; set; }

        [ForeignKey("CouponId")]
        [InverseProperty("Orders")]
        public virtual Coupon? Coupon { get; set; }

        [ForeignKey("CustomerId")]
        [InverseProperty("Orders")]
        public virtual Customer? Customer { get; set; }

        //tại 1 thời điểm chỉ có thể có 1 Order Status, ví dụ như Order Id = 1 có trạng thái Chờ, nó không thể có thêm 1 dòng trạng thái khác mà chỉ có thể sửa thôi
        [ForeignKey("OrderStatusId")]
        [InverseProperty("Orders")]
        public virtual OrderStatus OrderStatus { get; set; }

        [ForeignKey("DeliveryMethodId")]
        [InverseProperty("Orders")]
        public virtual DeliveryMethod DeliveryMethod { get; set; }

        [ForeignKey("PaymentMethodId")]
        [InverseProperty("Orders")]
        public virtual PaymentMethod PaymentMethod { get; set; }

        [ForeignKey("EmployeeId")]
        [InverseProperty("Orders")]
        public virtual Employee? Employee { get; set; }

        [InverseProperty("Order")]
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public string PlacedAddress { get; set; }
        public float? BillPrice { get; set; } = 0;
        public bool IsCustomerMember { get; set; }
        public string? NoneCustomerMemberPhoneNumber { get; set; }
        public string? NoneCustomerMemberEmailAddress { get; set; }

    }
}