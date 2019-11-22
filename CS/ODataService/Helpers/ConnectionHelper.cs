using System;
using ODataService.Models;

namespace ODataService.Helpers {
    public static class ConnectionHelper {
        static Type[] persistentTypes = new Type[] {
            typeof(Customer),
            typeof(OrderDetail),
            typeof(Order),
            typeof(Product)
        };
        public static Type[] GetPersistentTypes() {
            Type[] copy = new Type[persistentTypes.Length];
            Array.Copy(persistentTypes, copy, persistentTypes.Length);
            return copy;
        }
    }
}