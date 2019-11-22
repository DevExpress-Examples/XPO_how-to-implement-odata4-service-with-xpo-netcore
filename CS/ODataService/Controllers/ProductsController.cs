using System;
using System.Linq;
using DevExpress.Xpo;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using ODataService.Helpers;
using ODataService.Models;

namespace ODataService.Controllers {
    public class ProductsController : ODataController {

        private UnitOfWork Session;
        public ProductsController(UnitOfWork uow) {
            this.Session = uow;
        }

        [EnableQuery]
        public IQueryable<Product> Get() {
            return Session.Query<Product>().AsWrappedQuery();
        }

        [EnableQuery]
        public SingleResult<Product> Get([FromODataUri] int key) {
            var result = Session.Query<Product>().AsWrappedQuery().Where(t => t.ProductID == key);
            return SingleResult.Create(result);
        }

        [HttpPost]
        public IActionResult Post([FromBody]Product product) {
            if(!ModelState.IsValid) {
                return BadRequest();
            }
            Product entity = new Product(Session) {
                ProductName = product.ProductName,
                Picture = product.Picture
            };
            Session.CommitChanges();
            return Created(entity);
        }

        [HttpPut]
        public IActionResult Put([FromODataUri] int key, Product product) {
            if(!ModelState.IsValid) {
                return BadRequest();
            }
            if(key != product.ProductID) {
                return BadRequest();
            }
            Product existing = Session.GetObjectByKey<Product>(key);
            if(existing == null) {
                Product entity = new Product(Session) {
                    ProductName = product.ProductName,
                    Picture = product.Picture
                };
                Session.CommitChanges();
                return Created(entity);
            } else {
                existing.ProductName = product.ProductName;
                existing.Picture = product.Picture;
                Session.CommitChanges();
                return Updated(product);
            }
        }

        [HttpPatch]
        public IActionResult Patch([FromODataUri] int key, Delta<Product> product) {
            if(!ModelState.IsValid) {
                return BadRequest();
            }
            var result = ApiHelper.Patch<Product, int>(key, product, Session);
            if(result != null) {
                return Updated(result);
            }
            return NotFound();
        }

        [HttpDelete]
        public IActionResult Delete([FromODataUri] int key) {
            return StatusCode(ApiHelper.Delete<Product, int>(key, Session));
        }

        [HttpPost, HttpPut]
        public IActionResult CreateRef([FromODataUri]int key, string navigationProperty, [FromBody] Uri link) {
            return StatusCode(ApiHelper.CreateRef<Product, int>(Request, key, navigationProperty, link, Session));
        }

        [HttpDelete]
        public IActionResult DeleteRef([FromODataUri] int key, [FromODataUri] int relatedKey, string navigationProperty) {
            return StatusCode(ApiHelper.DeleteRef<Product, int, int>(key, relatedKey, navigationProperty, Session));
        }
    }
}