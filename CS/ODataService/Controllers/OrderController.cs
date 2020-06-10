using System;
using System.Linq;
using DevExpress.Xpo;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using ODataService.Helpers;
using ODataService.Models;

namespace ODataService.Controllers {
    public class OrderController : ODataController {

        private UnitOfWork Session;
        public OrderController(UnitOfWork uow) {
            this.Session = uow;
        }

        [EnableQuery]
        public IQueryable<Order> Get() {
            return Session.Query<Order>();
        }

        [EnableQuery]
        public SingleResult<Order> Get([FromODataUri] int key) {
            var result = Session.Query<Order>().Where(t => t.ID == key);
            return SingleResult.Create(result);
        }

        [EnableQuery]
        public SingleResult<Customer> GetCustomer([FromODataUri] int key) {
            var result = Session.Query<Order>().Where(m => m.ID == key).Select(m => m.Customer);
            return SingleResult.Create(result);
        }

        [EnableQuery]
        public IQueryable<OrderDetail> GetOrderDetails([FromODataUri] int key) {
            return Session.Query<OrderDetail>().Where(t => t.Order.ID == key);
        }

        [EnableQuery]
        public SingleResult<BaseDocument> GetParentDocument([FromODataUri] int key) {
            var result = Session.Query<Order>().Where(m => m.ID == key).Select(m => m.ParentDocument);
            return SingleResult.Create(result);
        }

        [EnableQuery]
        public IQueryable<BaseDocument> GetLinkedDocuments([FromODataUri] int key) {
            return Session.Query<Order>().Where(m => m.ID == key).SelectMany(t => t.LinkedDocuments);
        }

        [HttpPost]
        public IActionResult Post([FromBody]Order order) {
            if(!ModelState.IsValid) {
                return BadRequest();
            }
            Order entity = new Order(Session) {
                ID = order.ID,
                Date = order.Date,
                OrderStatus = order.OrderStatus
            };
            Session.CommitChanges();
            return Created(entity);
        }

        [HttpPut]
        public IActionResult Put([FromODataUri] int key, Order order) {
            if(!ModelState.IsValid) {
                return BadRequest();
            }
            if(key != order.ID) {
                return BadRequest();
            }
            Order existing = Session.GetObjectByKey<Order>(key);
            if(existing == null) {
                Order entity = new Order(Session) {
                    ID = order.ID,
                    Date = order.Date,
                    OrderStatus = order.OrderStatus
                };
                Session.CommitChanges();
                return Created(entity);
            } else {
                existing.Date = order.Date;
                Session.CommitChanges();
                return Updated(existing);
            }
        }

        [HttpPatch]
        public IActionResult Patch([FromODataUri] int key, Delta<Order> order) {
            if(!ModelState.IsValid) {
                return BadRequest();
            }
            var result = ApiHelper.Patch<Order, int>(key, order, Session);
            if(result != null) {
                return Updated(result);
            }
            return NotFound();
        }

        [HttpPost]
        [HttpPut]
        [ODataRoute("Order({key})/OrderDetails")]
        public IActionResult AddToOrderDetails([FromODataUri] int key, OrderDetail orderDetail) {
            Order order = Session.GetObjectByKey<Order>(key);
            if(order == null) {
                return NotFound();
            }
            OrderDetail existing = order.OrderDetails.FirstOrDefault(d => d.OrderDetailID == orderDetail.OrderDetailID);
            if(existing == null) {
                OrderDetail entity = new OrderDetail(Session) {
                    Quantity = orderDetail.Quantity,
                    UnitPrice = orderDetail.UnitPrice,
                };
                order.OrderDetails.Add(entity);
                Session.CommitChanges();
                return Created(entity);
            } else {
                existing.Quantity = orderDetail.Quantity;
                existing.UnitPrice = orderDetail.UnitPrice;
                Session.CommitChanges();
                return Updated(existing);
            }
        }

        [HttpDelete]
        public IActionResult Delete([FromODataUri] int key) {
            return StatusCode(ApiHelper.Delete<Order, int>(key, Session));
        }

        [HttpPost, HttpPut]
        public IActionResult CreateRef([FromODataUri]int key, string navigationProperty, [FromBody] Uri link) {
            return StatusCode(ApiHelper.CreateRef<Order, int>(Request, key, navigationProperty, link, Session));
        }

        [HttpDelete]
        public IActionResult DeleteRef([FromODataUri] int key, string navigationProperty, [FromBody] Uri link) {
            return StatusCode(ApiHelper.DeleteRef<Order, int>(Request, key, navigationProperty, link, Session));
        }

        [HttpDelete]
        public IActionResult DeleteRef([FromODataUri] int key, [FromODataUri] int relatedKey, string navigationProperty) {
            return StatusCode(ApiHelper.DeleteRef<Order, int, int>(key, relatedKey, navigationProperty, Session));
        }
    }
}