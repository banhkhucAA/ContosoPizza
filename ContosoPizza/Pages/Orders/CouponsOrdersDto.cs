using ContosoPizza.Models.Generated;

namespace ContosoPizza.Pages.Orders
{
    public class CouponsOrdersDto
    {
        public DateTime? CouponUsedAt { get; set; }
        public string? CouponCode { get; set; }
    }
}
