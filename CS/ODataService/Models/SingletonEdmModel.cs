using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;

namespace ODataService.Models {
    internal class SingletonEdmModel {
        public static IEdmModel GetEdmModel() {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var documents = builder.EntitySet<BaseDocument>("Documents");
            var customers = builder.EntitySet<Customer>("Customers");
            var orders = builder.EntitySet<Order>("Orders");
            var contracts = builder.EntitySet<Contract>("Contracts");
            var products = builder.EntitySet<Product>("Products");
            var orderDetails = builder.EntitySet<OrderDetail>("OrderDetails");

            documents.EntityType.HasKey(t => t.ID);
            customers.EntityType.HasKey(t => t.CustomerID);
            products.EntityType.HasKey(t => t.ProductID);
            orderDetails.EntityType.HasKey(t => t.OrderDetailID);
            orders.EntityType.DerivesFrom<BaseDocument>();
            contracts.EntityType.DerivesFrom<BaseDocument>();

            builder.Action("InitializeDatabase");
            builder.Function("TotalSalesByYear")
                .Returns<decimal>()
                .Parameter<int>("year");

            return builder.GetEdmModel();
        }
    }
}