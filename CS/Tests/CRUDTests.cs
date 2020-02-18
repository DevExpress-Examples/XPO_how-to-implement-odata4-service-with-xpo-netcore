using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Default;
using Microsoft.OData.Client;
using NUnit.Framework;
using ODataService.Models;

namespace Tests
{
    [TestFixture]
    public class CRUDOperationTests : ODataTestsBase {

        [Test]
        public async Task CreateObject() {
            Container container = GetODataContainer();
            Customer customer = new Customer() {
                CustomerID = "C0001",
                CompanyName = "Test Company"
            };
            container.AddToCustomer(customer);
            var response = await container.SaveChangesAsync();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.Created, response.First().StatusCode);

            container = GetODataContainer();
            Customer createdItem = await container.Customer.Where(t => t.CustomerID == customer.CustomerID).FirstAsync();
            Assert.AreEqual(customer.CustomerID, createdItem.CustomerID);
            Assert.AreEqual(customer.CompanyName, createdItem.CompanyName);
        }

        [Test]
        public async Task UpdateObject() {
            Container container = GetODataContainer();
            Customer customer = await container.Customer.Where(t => t.CustomerID == "BSBEV").FirstAsync();

            customer.CompanyName = "Test Company Renamed";
            container.UpdateObject(customer);
            var response = await container.SaveChangesAsync();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.NoContent, response.First().StatusCode);

            container = GetODataContainer();
            Customer updatedItem = await container.Customer.Where(t => t.CustomerID == "BSBEV").FirstAsync();
            Assert.AreEqual(customer.CustomerID, updatedItem.CustomerID);
            Assert.AreEqual(customer.CompanyName, updatedItem.CompanyName);
        }

        [Test]
        public async Task BatchUpdate() {
            Container container = GetODataContainer();
            var customers = await container.Customer.ToListAsync();
            foreach(var customer in customers) {
                customer.CompanyName += " -renamed";
                container.UpdateObject(customer);
            }
            var response = await container.SaveChangesAsync(Microsoft.OData.Client.SaveChangesOptions.BatchWithSingleChangeset);

            Assert.AreEqual(4, response.Count());
            foreach(var res in response) {
                Assert.AreEqual((int)HttpStatusCode.NoContent, res.StatusCode);
            }

            container = GetODataContainer();
            customers = await container.Customer.ToListAsync();
            foreach(var customer in customers) {
                Assert.IsTrue(customer.CompanyName.EndsWith(" -renamed"));
            }
        }

        [Test]
        public async Task DeleteObject() {
            Container container = GetODataContainer();
            Customer customer = await container.Customer.Where(t => t.CustomerID == "WARTH").FirstAsync();

            container.DeleteObject(customer);
            var response = await container.SaveChangesAsync();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.NoContent, response.First().StatusCode);
        }

        [Test]
        public async Task CreateObjectWithBlobField() {
            byte[] pictureData = new byte[256 * 256 * 4];
            for(int i = 0; i < pictureData.Length; i++) {
                pictureData[i] = (byte)i;
            }
            Container container = GetODataContainer();
            Product product = new Product() {
                ProductName = "test product",
                Picture = pictureData
            };
            container.AddToProduct(product);
            var response = await container.SaveChangesAsync();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.Created, response.First().StatusCode);

            container = GetODataContainer();
            Product createdItem = await container.Product.Where(t => t.ProductName == product.ProductName).FirstAsync();

            Assert.IsNotNull(createdItem.Picture);
            Assert.AreEqual(product.Picture.Length, createdItem.Picture.Length);
        }

        [Test]
        public async Task UpdateObjectWithBlobField() {
            byte[] pictureData = new byte[256 * 256 * 4];
            for(int i = 0; i < pictureData.Length; i++) {
                pictureData[i] = (byte)i;
            }
            Container container = GetODataContainer();
            var product = await container.Product.Where(t => t.ProductName == "Queso Cabrales").FirstAsync();
            byte[] oldPicture = product.Picture;
            product.Picture = pictureData;
            container.UpdateObject(product);
            var response = await container.SaveChangesAsync();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.NoContent, response.First().StatusCode);

            container = GetODataContainer();
            Product updatedItem = await container.Product.Where(t => t.ProductID == product.ProductID).FirstAsync();
            Assert.IsNull(oldPicture);
            Assert.IsNotNull(updatedItem.Picture);
            Assert.AreEqual(pictureData.Length, updatedItem.Picture.Length);
        }

        [Test]
        public async Task AddRelatedObject() {
            Container container = GetODataContainer();
            Order order = await container.Order.OrderByDescending(t => t.ID).FirstAsync();

            OrderDetail detail = new OrderDetail() {
                Quantity = 105,
                UnitPrice = 201.37m
            };
            container.AddRelatedObject(order, "OrderDetails", detail);
            var response = await container.SaveChangesAsync();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.Created, response.First().StatusCode);

            container = GetODataContainer();
            OrderDetail createdItem = await container.OrderDetail.OrderByDescending(t => t.OrderDetailID).FirstAsync();
            Assert.AreEqual(detail.Quantity, createdItem.Quantity);
            Assert.AreEqual(detail.UnitPrice, createdItem.UnitPrice);
            Assert.Greater(createdItem.OrderDetailID, 0);
        }

        [Test]
        public async Task UpdateRelatedObject() {
            Container container = GetODataContainer();
            Order order = await container.Order.Expand(t => t.OrderDetails).OrderBy(t => t.Date).FirstAsync();
            OrderDetail detail = order.OrderDetails.OrderBy(t => t.OrderDetailID).First();

            short oldQuantity = detail.Quantity;
            detail.Quantity += 1;
            container.UpdateObject(detail);
            var response = await container.SaveChangesAsync();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.NoContent, response.First().StatusCode);

            container = GetODataContainer();
            OrderDetail updatedItem = (await container.Order.Expand(t => t.OrderDetails)
                .Where(t => t.ID == order.ID).FirstAsync())
                .OrderDetails.Where(d => d.OrderDetailID == detail.OrderDetailID).First();
            Assert.AreNotEqual(oldQuantity, updatedItem.Quantity);
            Assert.AreEqual(detail.Quantity, updatedItem.Quantity);
        }

        [Test]
        public async Task DeleteRelatedObject() {
            Container container = GetODataContainer();
            Order order = await container.Order.Expand(t => t.OrderDetails).OrderBy(t => t.Date).FirstAsync();
            OrderDetail detail = order.OrderDetails.OrderBy(t => t.OrderDetailID).First();

            container.DeleteLink(order, "OrderDetails", detail);
            var response = await container.SaveChangesAsync();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.NoContent, response.First().StatusCode);

            container = GetODataContainer();
            Order updatedItem = await container.Order.Expand(t => t.OrderDetails).Where(t => t.ID == order.ID).FirstAsync();
            Assert.False(updatedItem.OrderDetails.Any(d => d.OrderDetailID == detail.OrderDetailID));
        }

        [Test]
        public async Task DeleteAggregatedCollection() {
            Container container = GetODataContainer();
            Order order = await container.Order.Expand(t => t.OrderDetails).OrderBy(t => t.Date).FirstAsync();

            container.DeleteObject(order);
            var response = await container.SaveChangesAsync();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.NoContent, response.First().StatusCode);

            container = GetODataContainer();
            var details = await container.OrderDetail.Where(t => t.Order.ID == order.ID).ToListAsync();
            Assert.AreEqual(0, details.Count);
        }

        [Test]
        public async Task AddLink() {
            Container container = GetODataContainer();
            Order order = await container.Order.Where(t => t.Customer == null).OrderByDescending(t => t.ID).FirstAsync();
            Customer customer = await container.Customer.Where(t => t.CustomerID == "BSBEV").FirstAsync();

            container.AddLink(customer, "Orders", order);
            var response = await container.SaveChangesAsync();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.NoContent, response.First().StatusCode);

            container = GetODataContainer();
            Order updatedItem = await container.Order.Expand(t => t.Customer).Where(t => t.ID == order.ID).FirstAsync();
            Assert.IsNull(order.Customer);
            Assert.NotNull(updatedItem.Customer);
            Assert.AreEqual(updatedItem.Customer.CustomerID, customer.CustomerID);
        }

        [Test]
        public async Task UpdateLink() {
            Container container = GetODataContainer();
            Order order = await container.Order.Where(t => t.Customer.CustomerID == "BSBEV").OrderByDescending(t => t.ID).FirstAsync();
            Customer customer = await container.Customer.Where(t => t.CustomerID == "SANTG").FirstAsync();

            container.AddLink(customer, "Orders", order);
            var response = await container.SaveChangesAsync();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.NoContent, response.First().StatusCode);

            container = GetODataContainer();
            Order updatedItem = await container.Order.Expand(t => t.Customer).Where(t => t.ID == order.ID).FirstAsync();
            Assert.AreEqual(updatedItem.Customer.CustomerID, customer.CustomerID);
        }

        [Test]
        public async Task DeleteLink() {
            Container container = GetODataContainer();
            Order order = await container.Order.Expand(t => t.Customer).Where(t => t.Customer.CustomerID == "BSBEV").OrderByDescending(t => t.ID).FirstAsync();

            container.DeleteLink(order.Customer, "Orders", order);
            var response = await container.SaveChangesAsync();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.NoContent, response.First().StatusCode);

            container = GetODataContainer();
            Order updatedItem = await container.Order.Expand(t => t.Customer).Where(t => t.ID == order.ID).FirstAsync();
            Assert.IsNotNull(order.Customer);
            Assert.IsNull(updatedItem.Customer);
        }

        [Test]
        public async Task DeleteInheritedObject() {
            Container container = GetODataContainer();
            int contractId = (await container.Contract.Where(t => t.Number == "2018-0003").FirstAsync()).ID;
            BaseDocument contract = await container.BaseDocument.Where(t => t.ID == contractId).FirstAsync();

            container.DeleteObject(contract);
            var response = await container.SaveChangesAsync();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.NoContent, response.First().StatusCode);

            container = GetODataContainer();
            int count = (await container.BaseDocument.ToListAsync()).Count(t => t.ID == contractId);
            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task AddLinkToInherited() {
            Container container = GetODataContainer();
            Order order = await container.Order.Where(t => t.ParentDocument == null).OrderByDescending(t => t.ID).FirstAsync();
            Contract contract = await container.Contract.Where(t => t.Number == "2018-0003").FirstAsync();

            container.AddLink(contract, "LinkedDocuments", order);
            var response = await container.SaveChangesAsync();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.NoContent, response.First().StatusCode);

            container = GetODataContainer();
            Order updatedItem = await container.Order.Expand(t => t.ParentDocument).Where(t => t.ID == order.ID).FirstAsync();
            Assert.IsNull(order.ParentDocument);
            Assert.NotNull(updatedItem.ParentDocument);
            Assert.AreEqual(updatedItem.ParentDocument.GetType(), typeof(Contract));
            Assert.AreEqual(updatedItem.ParentDocument.ID, contract.ID);
        }

        [Test]
        public async Task UpdateLinkToInherited() {
            Container container = GetODataContainer();
            Order order = await container.Order.Where(t => (t.ParentDocument as Contract).Number == "2018-0002").FirstAsync();
            Contract contract = await container.Contract.Where(t => t.Number == "2018-0003").FirstAsync();

            container.AddLink(contract, "LinkedDocuments", order);
            var response = await container.SaveChangesAsync();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.NoContent, response.First().StatusCode);

            container = GetODataContainer();
            Order updatedItem = await container.Order.Expand(t => t.ParentDocument).Where(t => t.ID == order.ID).FirstAsync();
            Assert.AreEqual(updatedItem.ParentDocument.ID, contract.ID);
        }

        [Test]
        public async Task DeleteLinkToInherited() {
            Container container = GetODataContainer();
            Order order = await container.Order.Expand(t => t.ParentDocument).Where(t => (t.ParentDocument as Contract).Number == "2018-0002").FirstAsync();

            container.DeleteLink(order.ParentDocument, "LinkedDocuments", order);
            var response = await container.SaveChangesAsync();

            Assert.AreEqual(1, response.Count());
            Assert.AreEqual((int)HttpStatusCode.NoContent, response.First().StatusCode);

            container = GetODataContainer();
            Order updatedItem = await container.Order.Expand(t => t.ParentDocument).Where(t => t.ID == order.ID).FirstAsync();
            Assert.IsNotNull(order.ParentDocument);
            Assert.IsNull(updatedItem.ParentDocument);
        }

        [Test]
        public async Task SetLink() {
            Container container = GetODataContainer();
            Order order = new Order() {
                Date = DateTimeOffset.Now,
                OrderStatus = OrderStatus.New
            };
            container.AddToOrder(order);
            var response1 = await container.SaveChangesAsync();

            Assert.AreEqual(1, response1.Count());
            Assert.AreEqual((int)HttpStatusCode.Created, response1.First().StatusCode);
            Customer customer = new Customer() {
                CustomerID = "Duffy",
                CompanyName = "Acme"
            };
            container.AddToCustomer(customer);
            order.Customer = customer;
            container.SetLink(order, "Customer", customer);
            var response2 = await container.SaveChangesAsync();
            Assert.AreEqual(2, response2.Count());
            Assert.IsTrue(response2.Any(r => r.StatusCode == (int)HttpStatusCode.Created));
            Assert.IsTrue(response2.Any(r => r.StatusCode == (int)HttpStatusCode.NoContent));

            container = GetODataContainer();
            Order createdItem = await container.Order.Expand(t => t.Customer).Where(t => t.ID == order.ID).FirstAsync();
            Assert.IsNotNull(createdItem.Customer);
            Assert.AreEqual(order.Customer.CustomerID, createdItem.Customer.CustomerID);
        }


        [Test]
        public async Task RemoveLink() {
            Container container = GetODataContainer();
            Order order = new Order() {
                Date = DateTimeOffset.Now,
                OrderStatus = OrderStatus.New
            };
            container.AddToOrder(order);
            Customer customer = new Customer() {
                CustomerID = "Duffy",
                CompanyName = "Acme"
            };
            container.AddToCustomer(customer);
            container.SetLink(order, "Customer", customer);
            var response1 = await container.SaveChangesAsync();
            Assert.AreEqual(3, response1.Count());
            Assert.AreEqual(2, response1.Count(r => r.StatusCode == (int)HttpStatusCode.Created));
            Assert.IsTrue(response1.Any(r => r.StatusCode == (int)HttpStatusCode.NoContent));

            container = GetODataContainer();
            Order theOrder = await container.Order.Expand(t => t.Customer).Where(t => t.ID == order.ID).FirstAsync();
            Assert.IsNotNull(theOrder.Customer);
            Assert.AreEqual(customer.CustomerID, theOrder.Customer.CustomerID);

            container.SetLink(theOrder, "Customer", null);
            var response2 = await container.SaveChangesAsync();
            Assert.AreEqual(1, response2.Count());
            Assert.AreEqual(1, response2.Count(r => r.StatusCode == (int)HttpStatusCode.NoContent));

            container = GetODataContainer();
            Order theOrder2 = await container.Order.Expand(t => t.Customer).Where(t => t.ID == order.ID).FirstAsync();
            Assert.IsNull(theOrder2.Customer);
        }
    }
}
