using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddZeroJsonConsoleLogging();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    var logger = context.RequestServices.GetService<ILoggerFactory>().CreateLogger("Test");

                    using (logger.BeginScope("myScope"))
                    {
                        logger.LogInformation("test1");
                    }

                    using (logger.BeginScope("OrderId : {orderId}, UserId : {userId}", 1, 2))
                    {
                        logger.LogInformation("test2");
                    }

                    using (logger.BeginScope(new OrderLogScope(3, 4)))
                    {
                        logger.LogInformation("test3");
                    }

                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
