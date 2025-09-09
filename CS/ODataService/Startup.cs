using DevExpress.Xpo.DB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ODataService.Helpers;
using ODataService.Models;
using System;
using System.Linq;

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
            services.AddControllers()
                .AddOData(opt => opt
                    .Select()
                    .Filter()
                    .OrderBy()
                    .Expand()
                    .Count()
                    .SetMaxTop(null)
                    .AddRouteComponents("odata", SingletonEdmModel.GetEdmModel()));

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

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
