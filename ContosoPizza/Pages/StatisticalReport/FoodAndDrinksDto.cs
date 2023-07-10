namespace ContosoPizza.Pages.StatisticalReport
{
    public class FoodAndDrinksDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float UnitPrice { get; set; }
        public int OrderedNumber { get; set; }
        public int OrderIncludeProduct { get; set; }
        public float TotalMoney { get; set; }
        public float RealMoneyEarn { get; set; }
        public string Image { get; set; }
    }
}
