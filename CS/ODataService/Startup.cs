using System;
using System.Linq;
using DevExpress.Xpo.DB;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ODataService.Helpers;
using ODataService.Models;

namespace ODataService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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
    }
}
