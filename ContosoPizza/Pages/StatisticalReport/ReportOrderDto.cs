namespace ContosoPizza.Pages.StatisticalReport
{
    public class ReportOrderDto
    {
        public int Id { get;set; }
        public DateTime? OrderPlacedAt { get; set; }
        public DateTime? OrderFullfilledAt { get; set; }
        public float PriceOrder { get; set; }
        public float DiscountAmount { get; set; }
    }
}
