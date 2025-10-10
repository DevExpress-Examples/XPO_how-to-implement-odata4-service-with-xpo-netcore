using System.Linq;
using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using ODataService.Helpers;
using ODataService.Models;

namespace ODataService.Controllers {
    public class ActionsController : ODataController {
        private UnitOfWork Session;

        public ActionsController(UnitOfWork uow) {
            this.Session = uow;
        }
        [Route("odata/InitializeDatabase")]
        public IActionResult InitializeDatabase() {
            DemoDataHelper.CleanupDatabase(Session);
            DemoDataHelper.CreateDemoData(Session);
            return Ok();
        }

        [HttpGet]
        [Route("odata/TotalSalesByYear(year={year})")]
        public IActionResult TotalSalesByYear(int year) {
            decimal result = Session.Query<Order>()
                    .Where(o => o.Date.Value.Year == year)
                    .Sum(o => o.OrderDetails.Sum(d => d.Quantity * d.UnitPrice));
            return Ok(result);
        }
    }
}