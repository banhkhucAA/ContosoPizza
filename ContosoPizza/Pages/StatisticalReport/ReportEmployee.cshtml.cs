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
        [BindProperty(SupportsGet = true)]
        public DateTime FromDate { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime ToDate { get; set; }


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
                                 TotalMoneyOrders = (float)Math.Round((decimal)eo.Where(o => o.EmployeeId == e.Id).Sum(o => o.BillPrice), 2),
                                 SuccessfullMoneyOrders = (float)Math.Round((decimal)eo.Where(o => o.EmployeeId == e.Id && o.OrderStatus.StatusName == "Delivered").Sum(o => o.BillPrice), 2),
                                 Discrepancy = (float)Math.Round((decimal)(eo.Where(o => o.EmployeeId == e.Id && o.OrderStatus.StatusName == "Delivered").Sum(o => o.BillPrice) - eo.Where(o => o.EmployeeId == e.Id).Sum(o => o.BillPrice)), 2),
                                 Image = e.Image,
                             }).ToList();

            if (string.IsNullOrEmpty(OrderBy) || OrderBy == "OrderByTotalMoneyOrders")
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

            if (Request.Query.ContainsKey("fromDate") && !Request.Query.ContainsKey("toDate"))
            {              
                var employees_fromdate = (from e in _context.Employees.ToList()
                                          join o in _context.Orders.ToList() on e.Id equals o.EmployeeId into eo
                                          select new ReportEmployeeDto
                                          {
                                              Id = e.Id,
                                              FullName = e.FirstName + " " + e.LastName,
                                              Role = e.Role,
                                              DoneOrders = eo.Count(o => o.EmployeeId == e.Id && o.OrderStatus.StatusName == "Delivered" && o.OrderPlacedAt >= FromDate),
                                              FailedOrders = eo.Count(o => o.EmployeeId == e.Id && (o.OrderStatus.StatusName == "Canceled" || o.OrderStatus.StatusName == "Returned" || o.OrderStatus.StatusName == "DeActive") && o.OrderPlacedAt >= FromDate),
                                              TotalMoneyOrders = (float)Math.Round((decimal)eo.Where(o => o.EmployeeId == e.Id && o.OrderPlacedAt >= FromDate).Sum(o => o.BillPrice), 2),
                                              SuccessfullMoneyOrders = (float)Math.Round((decimal)eo.Where(o => o.EmployeeId == e.Id && o.OrderStatus.StatusName == "Delivered" && o.OrderPlacedAt >= FromDate).Sum(o => o.BillPrice), 2),
                                              Discrepancy = (float)Math.Round((decimal)(eo.Where(o => o.EmployeeId == e.Id && o.OrderStatus.StatusName == "Delivered" && o.OrderPlacedAt >= FromDate).Sum(o => o.BillPrice) - eo.Where(o => o.EmployeeId == e.Id && o.OrderPlacedAt >= FromDate).Sum(o => o.BillPrice)), 2),
                                              Image = e.Image,
                                          }).ToList();

                if (string.IsNullOrEmpty(OrderBy) || OrderBy == "OrderByTotalMoneyOrders")
                {
                    employees_fromdate = employees_fromdate.OrderByDescending(e => e.TotalMoneyOrders).ToList();
                }
                else if (OrderBy == "OrderByDoneOrders")
                {
                    employees_fromdate = employees_fromdate.OrderByDescending(e => e.DoneOrders).ToList();
                }
                else if (OrderBy == "OrderByFailedOrders")
                {
                    employees_fromdate = employees_fromdate.OrderByDescending(e => e.FailedOrders).ToList();
                }
                else if (OrderBy == "OrderBySuccessfullMoneyOrders")
                {
                    employees_fromdate = employees_fromdate.OrderByDescending(e => e.SuccessfullMoneyOrders).ToList();
                }
                else if (OrderBy == "OrderByDiscrepancy")
                {
                    employees_fromdate = employees_fromdate.OrderByDescending(e => e.Discrepancy).ToList();
                }
                reportEmployeeDtos = employees_fromdate;
            }

            if (!Request.Query.ContainsKey("fromDate") && Request.Query.ContainsKey("toDate"))
            {
                var employees_todate = (from e in _context.Employees.ToList()
                                        join o in _context.Orders.ToList() on e.Id equals o.EmployeeId into eo
                                        select new ReportEmployeeDto
                                        {
                                            Id = e.Id,
                                            FullName = e.FirstName + " " + e.LastName,
                                            Role = e.Role,
                                            DoneOrders = eo.Count(o => o.EmployeeId == e.Id && o.OrderStatus.StatusName == "Delivered" && o.OrderPlacedAt <= ToDate),
                                            FailedOrders = eo.Count(o => o.EmployeeId == e.Id && (o.OrderStatus.StatusName == "Canceled" || o.OrderStatus.StatusName == "Returned" || o.OrderStatus.StatusName == "DeActive") && o.OrderPlacedAt <= ToDate),
                                            TotalMoneyOrders = (float)Math.Round((decimal)eo.Where(o => o.EmployeeId == e.Id && o.OrderPlacedAt <= ToDate).Sum(o => o.BillPrice), 2),
                                            SuccessfullMoneyOrders = (float)Math.Round((decimal)eo.Where(o => o.EmployeeId == e.Id && o.OrderStatus.StatusName == "Delivered" && o.OrderPlacedAt <= ToDate).Sum(o => o.BillPrice), 2),
                                            Discrepancy = (float)Math.Round((decimal)(eo.Where(o => o.EmployeeId == e.Id && o.OrderStatus.StatusName == "Delivered" && o.OrderPlacedAt <= ToDate).Sum(o => o.BillPrice) - eo.Where(o => o.EmployeeId == e.Id && o.OrderPlacedAt <= ToDate).Sum(o => o.BillPrice)), 2),
                                            Image = e.Image,
                                        }).ToList();


                if (string.IsNullOrEmpty(OrderBy) || OrderBy == "OrderByTotalMoneyOrders")
                {
                    employees_todate = employees_todate.OrderByDescending(e => e.TotalMoneyOrders).ToList();
                }
                else if (OrderBy == "OrderByDoneOrders")
                {
                    employees_todate = employees_todate.OrderByDescending(e => e.DoneOrders).ToList();
                }
                else if (OrderBy == "OrderByFailedOrders")
                {
                    employees_todate = employees_todate.OrderByDescending(e => e.FailedOrders).ToList();
                }
                else if (OrderBy == "OrderBySuccessfullMoneyOrders")
                {
                    employees_todate = employees_todate.OrderByDescending(e => e.SuccessfullMoneyOrders).ToList();
                }
                else if (OrderBy == "OrderByDiscrepancy")
                {
                    employees_todate = employees_todate.OrderByDescending(e => e.Discrepancy).ToList();
                }
                reportEmployeeDtos = employees_todate;
            }

            if (Request.Query.ContainsKey("fromDate") && Request.Query.ContainsKey("toDate"))
            {
                var employees_fromdate_todate = (from e in _context.Employees.ToList()
                                                 join o in _context.Orders.ToList() on e.Id equals o.EmployeeId into eo
                                                 select new ReportEmployeeDto
                                                 {
                                                     Id = e.Id,
                                                     FullName = e.FirstName + " " + e.LastName,
                                                     Role = e.Role,
                                                     DoneOrders = eo.Count(o => o.EmployeeId == e.Id && o.OrderStatus.StatusName == "Delivered" && o.OrderPlacedAt >= FromDate && o.OrderPlacedAt <= ToDate),
                                                     FailedOrders = eo.Count(o => o.EmployeeId == e.Id && (o.OrderStatus.StatusName == "Canceled" || o.OrderStatus.StatusName == "Returned" || o.OrderStatus.StatusName == "DeActive") && o.OrderPlacedAt >= FromDate && o.OrderPlacedAt <= ToDate),
                                                     TotalMoneyOrders = (float)Math.Round((decimal)eo.Where(o => o.EmployeeId == e.Id && o.OrderPlacedAt >= FromDate && o.OrderPlacedAt <= ToDate).Sum(o => o.BillPrice), 2),
                                                     SuccessfullMoneyOrders = (float)Math.Round((decimal)eo.Where(o => o.EmployeeId == e.Id && o.OrderStatus.StatusName == "Delivered" && o.OrderPlacedAt >= FromDate && o.OrderPlacedAt <= ToDate).Sum(o => o.BillPrice), 2),
                                                     Discrepancy = (float)Math.Round((decimal)(eo.Where(o => o.EmployeeId == e.Id && o.OrderStatus.StatusName == "Delivered" && o.OrderPlacedAt >= FromDate && o.OrderPlacedAt <= ToDate).Sum(o => o.BillPrice) - eo.Where(o => o.EmployeeId == e.Id && o.OrderPlacedAt >= FromDate && o.OrderPlacedAt <= ToDate).Sum(o => o.BillPrice)), 2),
                                                     Image = e.Image,
                                                 }).ToList();
                if (string.IsNullOrEmpty(OrderBy) || OrderBy == "OrderByTotalMoneyOrders")
                {
                    employees_fromdate_todate = employees_fromdate_todate.OrderByDescending(e => e.TotalMoneyOrders).ToList();
                }
                else if (OrderBy == "OrderByDoneOrders")
                {
                    employees_fromdate_todate = employees_fromdate_todate.OrderByDescending(e => e.DoneOrders).ToList();
                }
                else if (OrderBy == "OrderByFailedOrders")
                {
                    employees_fromdate_todate = employees_fromdate_todate.OrderByDescending(e => e.FailedOrders).ToList();
                }
                else if (OrderBy == "OrderBySuccessfullMoneyOrders")
                {
                    employees_fromdate_todate = employees_fromdate_todate.OrderByDescending(e => e.SuccessfullMoneyOrders).ToList();
                }
                else if (OrderBy == "OrderByDiscrepancy")
                {
                    employees_fromdate_todate = employees_fromdate_todate.OrderByDescending(e => e.Discrepancy).ToList();
                }
                reportEmployeeDtos = employees_fromdate_todate;
            }
            return Page();
        }
    }
}
