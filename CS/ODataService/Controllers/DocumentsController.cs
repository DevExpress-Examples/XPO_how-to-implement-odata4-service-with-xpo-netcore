using System;
using System.Linq;
using DevExpress.Xpo;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using ODataService.Helpers;
using ODataService.Models;

namespace ODataService.Controllers {
    public class DocumentsController : ODataController {

        private UnitOfWork Session;
        public DocumentsController(UnitOfWork uow) {
            this.Session = uow;
        }

        [EnableQuery]
        public IQueryable<BaseDocument> Get() {
            return Session.Query<BaseDocument>().AsWrappedQuery();
        }

        [EnableQuery]
        public SingleResult<BaseDocument> Get([FromODataUri] int key) {
            var result = Session.Query<BaseDocument>().AsWrappedQuery().Where(t => t.ID == key);
            return SingleResult.Create(result);
        }

        [HttpPost, HttpPut]
        public IActionResult CreateRef([FromODataUri]int key, string navigationProperty, [FromBody] Uri link) {
            return StatusCode(ApiHelper.CreateRef<BaseDocument, int>(Request, key, navigationProperty, link, Session));
        }

        [HttpDelete]
        public IActionResult DeleteRef([FromODataUri] int key, [FromODataUri] int relatedKey, string navigationProperty) {
            return StatusCode(ApiHelper.DeleteRef<BaseDocument, int, int>(key, relatedKey, navigationProperty, Session));
        }

        [HttpDelete]
        public IActionResult Delete([FromODataUri] int key) {
            return StatusCode(ApiHelper.Delete<BaseDocument, int>(key, Session));
        }
    }
}