using System.Collections.Generic;
using DevExpress.Xpo;

namespace ODataService.Models {
    [Persistent("Products")]
    public partial class Product : XPLiteObject {

        public Product() { }
        public Product(Session session) : base(session) { }

        int fProductID;
        [Key(true)]
        public int ProductID {
            get { return fProductID; }
            set { SetPropertyValue<int>(nameof(ProductID), ref fProductID, value); }
        }

        string fProductName;
        [Indexed(Name = @"ProductName")]
        [Size(40)]
        [Nullable(false)]
        public string ProductName {
            get { return fProductName; }
            set { SetPropertyValue<string>(nameof(ProductName), ref fProductName, value); }
        }

        decimal? fUnitPrice;
        [ColumnDbDefaultValue("(0)")]
        public decimal? UnitPrice {
            get { return fUnitPrice; }
            set { SetPropertyValue<decimal?>(nameof(UnitPrice), ref fUnitPrice, value); }
        }

        byte[] fPicture;
        public byte[] Picture {
            get { return fPicture; }
            set { SetPropertyValue<byte[]>(nameof(Picture), ref fPicture, value); }
        }

        [Association(@"ProductsReferencesOrderDetails")]
        public XPCollection<OrderDetail> OrderDetails { get { return GetCollection<OrderDetail>(nameof(OrderDetails)); } }
    }
}