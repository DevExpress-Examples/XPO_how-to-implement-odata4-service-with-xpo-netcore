using System;
using System.Linq;
using DevExpress.Xpo;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using ODataService.Helpers;
using ODataService.Models;

namespace ODataService.Controllers {
    public class CustomersController : ODataController {

        private UnitOfWork Session;
        public CustomersController(UnitOfWork uow) {
            this.Session = uow;
        }

        [EnableQuery]
        public IQueryable<Customer> Get() {
            return Session.Query<Customer>().AsWrappedQuery();
        }

        [EnableQuery]
        public SingleResult<Customer> Get([FromODataUri] string key) {
            var result = Session.Query<Customer>().AsWrappedQuery().Where(t => t.CustomerID == key);
            return SingleResult.Create(result);
        }

        [EnableQuery]
        public IQueryable<Order> GetOrders([FromODataUri] string key) {
            return Session.Query<Customer>().AsWrappedQuery().Where(m => m.CustomerID == key).SelectMany(m => m.Orders);
        }


        [HttpPost]
        public IActionResult Post([FromBody]Customer customer) {
            if(!ModelState.IsValid) {
                return BadRequest();
            }
            Customer entity = new Customer(Session) {
                CustomerID = customer.CustomerID,
                CompanyName = customer.CompanyName
            };
            Session.CommitChanges();
            return Created(entity);
        }

        [HttpPut]
        public IActionResult Put([FromODataUri] string key, [FromBody] Customer customer) {
            if(!ModelState.IsValid) {
                return BadRequest();
            }
            if(key != customer.CustomerID) {
                return BadRequest();
            }
            Customer existing = Session.GetObjectByKey<Customer>(key);
            if(existing == null) {
                Customer entity = new Customer(Session) {
                    CustomerID = customer.CustomerID,
                    CompanyName = customer.CompanyName
                };
                Session.CommitChanges();
                return Created(entity);
            } else {
                existing.CustomerID = customer.CustomerID;
                existing.CompanyName = customer.CompanyName;
                Session.CommitChanges();
                return Updated(customer);
            }
        }

        [HttpPatch]
        public IActionResult Patch([FromODataUri] string key, [FromBody] Delta<Customer> customer) {
            if(!ModelState.IsValid) {
                return BadRequest();
            }
            var result = ApiHelper.Patch<Customer, string>(key, customer, Session);
            if(result != null) {
                return Updated(result);
            }
            return NotFound();
        }

        [HttpDelete]
        public IActionResult Delete([FromODataUri] string key) {
            return StatusCode(ApiHelper.Delete<Customer, string>(key, Session));
        }

        [HttpPost, HttpPut]
        public IActionResult CreateRef([FromODataUri]string key, string navigationProperty, [FromBody] Uri link) {
            return StatusCode(ApiHelper.CreateRef<Customer, string>(Request, key, navigationProperty, link, Session));
        }

        [HttpDelete]
        public IActionResult DeleteRef([FromODataUri] string key, [FromODataUri] int relatedKey, string navigationProperty) {
            return StatusCode(ApiHelper.DeleteRef<Customer, string, int>(key, relatedKey, navigationProperty, Session));
        }
    }
}