using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System;

namespace HealthCheckDemo
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
            services.AddControllers();

            var redisString = "";
            services.AddDistributedRedisCache(options =>
            {
                options.InstanceName = "";
                options.Configuration = redisString;
            });

            var connectionString = Configuration.GetConnectionString("BloggingDatabase");
            var weatherServiceUri = "https://localhost:44385";

            services.AddHealthChecks()
                    .AddSqlServer(connectionString)
                    .AddRedis(redisString)
                    .AddUrlGroup(new Uri($"{weatherServiceUri}/weatherforecast"), "Weather API Health Check", HealthStatus.Degraded, timeout: new System.TimeSpan(0,0,3));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
