namespace ContosoPizza.Pages.StatisticalReport
{
    public class ReportEmployeeDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get;set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public float TotalMoneyOrders { get; set; }
        public string Image { get; set; }
        public int DoneOrders { get; set; }
        public int FailedOrders { get; set; }
        public float Discrepancy { get; set; }
        public float SuccessfullMoneyOrders { get; set; }
    }
}
