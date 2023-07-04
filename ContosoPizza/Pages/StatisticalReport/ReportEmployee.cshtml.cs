using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ContosoPizza.Pages.StatisticalReport
{
    public class ReportEmployeeModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ReportEmployeeModel(ContosoPizza.Data.ContosoPizzaContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        /*[BindProperty(SupportsGet = true)]
        public string OrderBy { get; set; }*/
        public IList<ReportEmployeeDto> reportEmployeeDtos { get; set; }

        public async Task<IActionResult> OnGetAsync(string OrderBy)
        {
            var employees = (from e in _context.Employees.ToList()
                             join o in _context.Orders.ToList() on e.Id equals o.EmployeeId into eo
                             select new ReportEmployeeDto
                             {
                                 Id = e.Id,
                                 FullName = e.FirstName + " " + e.LastName,
                                 Role = e.Role,
                                 DoneOrders = eo.Count(o => o.EmployeeId == e.Id && o.OrderStatus.StatusName == "Delivered"),
                                 FailedOrders = eo.Count(o => o.EmployeeId == e.Id && (o.OrderStatus.StatusName == "Canceled" || o.OrderStatus.StatusName == "Returned" || o.OrderStatus.StatusName == "DeActive")),
                                 TotalMoneyOrders = (float)eo.Where(o => o.EmployeeId == e.Id).Sum(o => o.BillPrice),
                                 SuccessfullMoneyOrders = (float)eo.Where(o => o.EmployeeId == e.Id && o.OrderStatus.StatusName == "Delivered").Sum(o => o.BillPrice),
                                 Discrepancy = (float)eo.Where(o => o.EmployeeId == e.Id && o.OrderStatus.StatusName == "Delivered").Sum(o => o.BillPrice) - (float)eo.Where(o => o.EmployeeId == e.Id).Sum(o => o.BillPrice),
                                 Image = e.Image,                                                               
                             }).ToList();

            if (string.IsNullOrEmpty(OrderBy)|| OrderBy == "OrderByTotalMoneyOrders")
            {
                employees = employees.OrderByDescending(e => e.TotalMoneyOrders).ToList();
            }
            else if (OrderBy == "OrderByDoneOrders")
            {
                employees = employees.OrderByDescending(e => e.DoneOrders).ToList();
            }
            else if (OrderBy == "OrderByFailedOrders")
            {
                employees = employees.OrderByDescending(e => e.FailedOrders).ToList();
            }
            else if (OrderBy == "OrderBySuccessfullMoneyOrders")
            {
                employees = employees.OrderByDescending(e => e.SuccessfullMoneyOrders).ToList();
            }
            else if (OrderBy == "OrderByDiscrepancy")
            {
                employees = employees.OrderByDescending(e => e.Discrepancy).ToList();
            }
            reportEmployeeDtos = employees;
            return Page();
        }
    }
}
