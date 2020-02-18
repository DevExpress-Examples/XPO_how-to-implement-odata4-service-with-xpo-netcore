using System.Collections.Generic;
using DevExpress.Xpo;

namespace ODataService.Models {
    [Persistent("Orders")]
    public class Order : BaseDocument {

        public Order(Session session) : base(session) { }
        public Order() { }

        OrderStatus fOrderStatus;
        public OrderStatus OrderStatus {
            get { return fOrderStatus; }
            set { SetPropertyValue<OrderStatus>(nameof(OrderStatus), ref fOrderStatus, value); }
        }

        Customer fCustomerID;
        [Size(5)]
        [Association(@"OrdersReferencesCustomers")]
        [Persistent("CustomerID")]
        public Customer Customer {
            get { return fCustomerID; }
            set { SetPropertyValue<Customer>(nameof(Customer), ref fCustomerID, value); }
        }

        [Association(@"OrdersReferencesOrderDetails")]
        [Aggregated]
        public XPCollection<OrderDetail> OrderDetails { get { return GetCollection<OrderDetail>(nameof(OrderDetails)); } }
    }
}