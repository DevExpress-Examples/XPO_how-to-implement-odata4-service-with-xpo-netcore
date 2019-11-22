How to implement OData4 service with XPO (.NET Core)
========================================

This example describes how to implement an OData4 service with XPO. This example is an ASP.NET Core 2.2 MVC Web API project and provides simple REST API for data access.

Steps to implement:

1. Create a new **ASP.NET Core Web Application** project and select the **API** project template.
2. Install the following nuget packages:
	* DevExpress.Xpo
	* Microsoft.AspNetCore.OData
3. Define your data model - implement persistent classes and initialize the data layer. If you are new to XPO, refer to the following articles to learn how to do this: [Create Persistent Class](https://docs.devexpress.com/CoreLibraries/2256/devexpress-orm-tool/getting-started/tutorial-1-your-first-data-aware-application-with-xpo), [Map to Existing Tables](https://docs.devexpress.com/CoreLibraries/3264/devexpress-orm-tool/concepts/basics-of-creating-persistent-objects-for-existing-data-tables).
4. Add files from the **CS\OdataService\Helpers** folder in this example to your project ([Quick Tip: Add files to Visual Studio projects the easy way](https://blogs.msdn.microsoft.com/davidklinems/2007/12/18/quick-tip-add-files-to-visual-studio-projects-the-easy-way/)).
5. Add Connection String for your database to **CS\OdataService\appsettings.json** file (in this example, used MS SQL LocalDB database):

```
  "ConnectionStrings": {
    "MSSqlServer": "XpoProvider=MSSqlServer;data source=(LocalDB)\\MSSQLLocalDB;Integrated Security=true;MultipleActiveResultSets=true;initial catalog=ODataTest"
  }
```
 
6. Modify the `ConfigureServices()` method declared in the *Startup.cs* file: register XPO UnitOfWork and OData services in Dependency Injection:

```cs
  public void ConfigureServices(IServiceCollection services) {
     services.AddOData();
     services.AddODataQueryFilter();
     services.AddMvc(options => {
        options.EnableEndpointRouting = false;
     })
     .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
     services.AddXpoDefaultUnitOfWork(true, (DataLayerOptionsBuilder options) =>
        options.UseConnectionString(Configuration.GetConnectionString("MSSqlServer"))
        .UseAutoCreationOption(AutoCreateOption.DatabaseAndSchema) // debug only
        .UseEntityTypes(ConnectionHelper.GetPersistentTypes()));
  }
```

7. Modify the `Configure()` method declared in the *Startup.cs* file: add middleware for OData Services and add mapping for OData Service route:

```cs
public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
    if (env.IsDevelopment()) {
    	app.UseDeveloperExceptionPage();
    } else {
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
    }

    //app.UseHttpsRedirection();

    app.UseODataBatching();

    app.UseMvc(b => { 
	b.Count().Filter().OrderBy().Expand().Select().MaxTop(null);
        b.MapODataServiceRoute("odata", "odata", SingletonEdmModel.GetEdmModel(), new DefaultODataBatchHandler());
    });
  }
```
   
8. Add OData controllers to the Controllers folder. An OData controller is a class inherited from the Microsoft.AspNet.OData.ODataController class. Each controller represents a separate data model class created on the third step.
9. Implement the required methods in controllers (e.g., `Get`, `Post`, `Put`, `Path`, `Delete`, etc.). For reference, use existing controllers in this example. For example: **CS\ODataService\Controllers\CustomersController.cs**.
