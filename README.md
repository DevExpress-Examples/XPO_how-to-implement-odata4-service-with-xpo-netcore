<!-- default badges list -->
![](https://img.shields.io/endpoint?url=https://codecentral.devexpress.com/api/v1/VersionRange/223364607/20.1.5%2B)
[![](https://img.shields.io/badge/Open_in_DevExpress_Support_Center-FF7200?style=flat-square&logo=DevExpress&logoColor=white)](https://supportcenter.devexpress.com/ticket/details/T835143)
[![](https://img.shields.io/badge/📖_How_to_use_DevExpress_Examples-e9f6fc?style=flat-square)](https://docs.devexpress.com/GeneralInformation/403183)
<!-- default badges end -->
# How to Implement OData v4 Service with XPO (.NET Core 3.1)

This example demonstrates how to create **an ASP.NET Core 3.1 Web API** project and provide a simple REST API using the XPO ORM for data access. For the .NET Framework-based example, refer to [How to Implement OData v4 Service with XPO (.NET Framework)](https://github.com/DevExpress-Examples/XPO_how-to-implement-odata4-service-with-xpo).

## Prerequisites

* [Visual Studio 2019](https://visualstudio.microsoft.com/vs/) with the following workloads:
  * **ASP.NET and web development**
  * **.NET Core cross-platform development**
* [.NET Core SDK 3.1 or later](https://www.microsoft.com/net/download/all)

## Steps To Implement

### Step 1: Create Solution and Add Required Dependencies
- Create a new **ASP.NET Core Web Application** project and select the **API** project template.
- Install the following NuGet packages:
	* **DevExpress.Xpo**
	* **Microsoft.AspNetCore.OData**
- Add files from the **CS\ODataService\Helpers** folder in this example to your project ([Quick Tip: Add files to Visual Studio projects the easy way](https://blogs.msdn.microsoft.com/davidklinems/2007/12/18/quick-tip-add-files-to-visual-studio-projects-the-easy-way/)). These files contain helpers for demo data generation, LINQ and OData API extensions that will be used later.

### Step 2: Define XPO and EDM Data Model
- Create an XPO data model or add files from the **CS\ODataService\Models** folder in this example to your project. For more information on how to create your own data model, refer to the following articles: [Create Persistent Class](https://docs.devexpress.com/XPO/2077/create-a-data-model/create-a-persistent-object) | [Map to Existing Tables](https://docs.devexpress.com/CoreLibraries/3264/devexpress-orm-tool/concepts/basics-of-creating-persistent-objects-for-existing-data-tables). Make sure that the **CS\ODataService\Helpers\ConnectionHelper.cs** file includes your custom persistent types.
- Add the [SingletonEdmModel.cs](CS/ODataService/Models/SingletonEdmModel.cs) file to your project. The [SingletonEdmModel.cs](CS/ODataService/Models/SingletonEdmModel.cs) file demonstrates two ways to populate the EDM model with XPO classes. Also, in this file, we demonstrate how to add custom actions and functions (*InitializeDatabase* and *TotalSalesByYear*). For additional information, refer to the [Convention model builder](https://docs.microsoft.com/en-us/odata/webapi/convention-model-builder) and [Actions and Functions in OData v4 Using ASP.NET Web API 2.2](https://docs.microsoft.com/en-us/aspnet/web-api/overview/odata-support-in-aspnet-web-api/odata-v4/odata-actions-and-functions) articles.

### Step 3. Initialize Data Layer and Configure ASP.NET Core Middleware
- Specify a connection string for your database in the **CS\ODataService\appsettings.json** file (Microsoft SQL Server LocalDB is used by default).
```
  "ConnectionStrings": {
    "MSSqlServer": "XpoProvider=MSSqlServer;data source=(LocalDB)\\MSSQLLocalDB;Integrated Security=true;MultipleActiveResultSets=true;initial catalog=ODataTest"
  }
```
- Modify the `ConfigureServices()` method in the *Startup.cs* file to initialize the data layer and register XPO UnitOfWork and OData services in Dependency Injection.
```cs
  public void ConfigureServices(IServiceCollection services) {
  	services.AddOData();
  	services.AddODataQueryFilter();
      services.AddMvc(options => {
          options.EnableEndpointRouting = false;
          options.ModelValidatorProviders.Clear();
      });
  
      services.AddSingleton<IObjectModelValidator, CustomModelValidator>();
  
      services.AddXpoDefaultUnitOfWork(true, (DataLayerOptionsBuilder options) =>
          options.UseConnectionString(Configuration.GetConnectionString("MSSqlServer"))
          .UseAutoCreationOption(AutoCreateOption.DatabaseAndSchema) // debug only
          .UseEntityTypes(ConnectionHelper.GetPersistentTypes()));
  }
```
- Modify the `Configure()` method in the *Startup.cs* file to add middleware for OData services and specify mapping for the service route. Note that we will pass the EDM model defined on the second step as a parameter (`SingletonEdmModel.GetEdmModel()`).
```cs
  public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
      if (env.IsDevelopment())
      {
          app.UseDeveloperExceptionPage();
      }
  
      app.UseODataBatching();
  
      app.UseMvc(b =>
      {
          b.Count().Filter().OrderBy().Expand().Select().MaxTop(null);
          b.MapODataServiceRoute("odata", "odata", SingletonEdmModel.GetEdmModel(), new DefaultODataBatchHandler());
      });
  }
```

### Step 4: Implement OData Controllers for CRUD and Actions/Functions
- In the Controllers folder, add classes inherited from **Microsoft.AspNet.OData.ODataController** for each data model class created on the second step. 
- Implement the required methods in OData controllers (e.g., `Get`, `Post`, `Put`, `Patch`, `Delete`, etc.) as shown in this example (for instance, **CS\ODataService\Controllers\CustomersController.cs**).
- Implement methods in an OData Controller for required OData Actions and Functions as shown in  **CS\ODataService\Controllers\ActionsController.cs**.
