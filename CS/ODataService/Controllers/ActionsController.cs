using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Xpo;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using ODataService.Helpers;
using ODataService.Models;

namespace ODataService.Controllers {
    public class ActionsController : ODataController {
        private UnitOfWork Session;

        public ActionsController(UnitOfWork uow) {
            this.Session = uow;
        }
        [ODataRoute("InitializeDatabase")]
        public IActionResult InitializeDatabase() {
            DemoDataHelper.CleanupDatabase(Session);
            DemoDataHelper.CreateDemoData(Session);
            return Ok();
        }

        [HttpGet]
        [ODataRoute("TotalSalesByYear(year={year})")]
        public IActionResult TotalSalesByYear(int year) {
            decimal result = Session.Query<Order>()
                    .Where(o => o.Date.Value.Year == year)
                    .Sum(o => o.OrderDetails.Sum(d => d.Quantity * d.UnitPrice));
            return Ok(result);
        }
    }
}