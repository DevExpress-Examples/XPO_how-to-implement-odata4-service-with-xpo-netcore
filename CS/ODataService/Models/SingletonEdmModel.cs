using DevExpress.Xpo.Metadata;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using ODataService.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ODataService.Models {
    internal class SingletonEdmModel {
        static IEdmModel edmModel;
        public static IEdmModel GetEdmModel() {
            if(edmModel != null) {
                return edmModel;
            }

            var builder = new ODataConventionModelBuilder();

            // Approach 1: Automatically add all persistent classes to EDM
            // This approach has a naming convention: a controller name must 
            // match the corresponding XPO class name

            var dictionary = new ReflectionDictionary();
            foreach(var type in ConnectionHelper.GetPersistentTypes()) {
                XPClassInfo classInfo = dictionary.GetClassInfo(type);
                CreateEntitySet(classInfo, builder);
            }

            // Approach 2: Manually add persistent classes to EDM

            /*var documents = builder.EntitySet<BaseDocument>("BaseDocument");
            var customers = builder.EntitySet<Customer>("Customer");
            var orders = builder.EntitySet<Order>("Order");
            var contracts = builder.EntitySet<Contract>("Contract");
            var products = builder.EntitySet<Product>("Product");
            var orderDetails = builder.EntitySet<OrderDetail>("OrderDetail");
            documents.EntityType.HasKey(t => t.ID);
            customers.EntityType.HasKey(t => t.CustomerID);
            products.EntityType.HasKey(t => t.ProductID);
            orderDetails.EntityType.HasKey(t => t.OrderDetailID);
            orders.EntityType.DerivesFrom<BaseDocument>();
            contracts.EntityType.DerivesFrom<BaseDocument>();*/

            // Add actions and functions to EDM

            builder.Action("InitializeDatabase");
            builder.Function("TotalSalesByYear")
                .Returns<decimal>()
                .Parameter<int>("year");

            edmModel = builder.GetEdmModel();
            return edmModel;
        }

        static EntitySetConfiguration CreateEntitySet(XPClassInfo classInfo, ODataModelBuilder builder) {
            EntitySetConfiguration entitySetConfig = builder.EntitySets.FirstOrDefault(t => t.EntityType.ClrType == classInfo.ClassType);
            if(entitySetConfig != null) {
                return entitySetConfig;
            }
            EntityTypeConfiguration entityTypeConfig = builder.AddEntityType(classInfo.ClassType);
            entitySetConfig = builder.AddEntitySet(classInfo.ClassType.Name, entityTypeConfig);
            if(classInfo.PersistentBaseClass != null) {
                EntitySetConfiguration baseClassEntitySetConfig = CreateEntitySet(classInfo.PersistentBaseClass, builder);
                entityTypeConfig.DerivesFrom(baseClassEntitySetConfig.EntityType);
            } else {
                entityTypeConfig.HasKey(classInfo.ClassType.GetProperty(classInfo.KeyProperty.Name));
            }
            return entitySetConfig;
        }
    }
}