using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Default;
using NUnit.Framework;

namespace Tests {
    public class ActionTests : ODataTestsBase {

        [Test]
        public async Task FunctionTest() {
            Container container = GetODataContainer();
            decimal sales2017 = await container.TotalSalesByYear(2017).GetValueAsync();
            decimal sales2018 = await container.TotalSalesByYear(2018).GetValueAsync();

            Assert.AreEqual(0, sales2017);
            Assert.AreEqual(3501.55m, sales2018);
        }
    }
}
