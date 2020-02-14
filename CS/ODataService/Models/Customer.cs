using System.Collections.Generic;
using DevExpress.Xpo;

namespace ODataService.Models {
    [Persistent("Customers")]
    public class Customer : XPLiteObject {

        public Customer(Session session) : base(session) { }
        public Customer() : base(XpoDefault.Session) { }

        string fCustomerID;
        [Key]
        [Size(5)]
        [Nullable(false)]
        public string CustomerID {
            get { return fCustomerID; }
            set { SetPropertyValue<string>(nameof(CustomerID), ref fCustomerID, value); }
        }

        string fCompanyName;
        [Indexed(Name = @"CompanyName")]
        [Size(40)]
        [Nullable(false)]
        public string CompanyName {
            get { return fCompanyName; }
            set { SetPropertyValue<string>(nameof(CompanyName), ref fCompanyName, value); }
        }

        [Association(@"OrdersReferencesCustomers")]
        public XPCollection<Order> Orders { get { return GetCollection<Order>(nameof(Orders)); } }

        [Association(@"ContractsReferencesCustomers")]
        public XPCollection<Contract> Contracts { get { return GetCollection<Contract>(nameof(Contracts)); } }
    }
}