using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Default;
using Microsoft.OData.Client;
using NUnit.Framework;

namespace Tests {
    public abstract class ODataTestsBase {

        protected const string ODataServiceUrl = "http://localhost:5000/odata/";
        Process dotnetProcess = null;

        protected Container GetODataContainer() {
            return new Container(new Uri(ODataServiceUrl));
        }

        [OneTimeSetUp]
        public void OneTimeSetup() {
            string appPath = Path.GetDirectoryName(this.GetType().Assembly.Location);
            appPath = Path.GetFullPath(Path.Combine(appPath, "..", "..", "..", "..", "ODataService", "bin", "debug", "netcoreapp3.1"));
            dotnetProcess = Process.Start(new ProcessStartInfo("dotnet") { WorkingDirectory = appPath, Arguments = "ODataService.dll" });
        }

        [OneTimeTearDown]
        public void OneTimeTearDown() {
            if(dotnetProcess != null) {
                dotnetProcess.Kill();
            }
        }

        [SetUp]
        public async Task Setup() {
            Container container = GetODataContainer();
            await container.InitializeDatabase().ExecuteAsync();
        }
    }


    static class QueryableAsyncExtensions {
        static Task<IEnumerable<T>> GetEnumerableAsync<T>(IQueryable<T> queryable) {
            var query = queryable as DataServiceQuery<T>;
            var container = query.Context;
            var taskFactory = new TaskFactory<IEnumerable<T>>();
            return taskFactory.FromAsync(query.BeginExecute(null, null), data => query.EndExecute(data));
        }
        public static async Task<T> FirstAsync<T>(this IQueryable<T> queryable) {
            return (await GetEnumerableAsync(queryable).ConfigureAwait(false)).First();
        }
        public static async Task<List<T>> ToListAsync<T>(this IQueryable<T> queryable) {
            return (await GetEnumerableAsync(queryable).ConfigureAwait(false)).ToList();
        }
    }
}
