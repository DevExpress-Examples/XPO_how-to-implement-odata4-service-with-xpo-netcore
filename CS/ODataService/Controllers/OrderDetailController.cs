using System.Linq;
using DevExpress.Xpo;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using ODataService.Helpers;
using ODataService.Models;

namespace ODataService.Controllers {
    public class OrderDetailController : ODataController {

        private UnitOfWork Session;
        public OrderDetailController(UnitOfWork uow) {
            this.Session = uow;
        }

        [EnableQuery]
        public IQueryable<OrderDetail> Get() {
            return Session.Query<OrderDetail>();
        }

        [EnableQuery]
        public SingleResult<OrderDetail> Get([FromODataUri] int key) {
            var result = Session.Query<OrderDetail>().Where(t => t.OrderDetailID == key);
            return SingleResult.Create(result);
        }

        [HttpPut]
        public IActionResult Put([FromODataUri] int key, OrderDetail orderDetail) {
            if(!ModelState.IsValid) {
                return BadRequest();
            }
            if(key != orderDetail.OrderDetailID) {
                return BadRequest();
            }
            OrderDetail existing = Session.GetObjectByKey<OrderDetail>(key);
            if(existing == null) {
                OrderDetail entity = new OrderDetail(Session) {
                    Order = orderDetail.Order,
                    Quantity = orderDetail.Quantity,
                    UnitPrice = orderDetail.UnitPrice
                };
                Session.CommitChanges();
                return Created(entity);
            } else {
                existing.Quantity = orderDetail.Quantity;
                existing.UnitPrice = orderDetail.UnitPrice;
                Session.CommitChanges();
                return Updated(existing);
            }
        }

        [HttpPatch]
        public IActionResult Patch([FromODataUri] int key, Delta<OrderDetail> orderDetail) {
            if(!ModelState.IsValid) {
                return BadRequest();
            }
            var result = ApiHelper.Patch<OrderDetail, int>(key, orderDetail, Session);
            if(result != null) {
                return Updated(result);
            }
            return NotFound();
        }

        [HttpDelete]
        public IActionResult Delete([FromODataUri] int key) {
            return StatusCode(ApiHelper.Delete<OrderDetail, int>(key, Session));
        }
    }
}