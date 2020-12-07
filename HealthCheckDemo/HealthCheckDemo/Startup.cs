using HealthCheckDemo.Messages;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

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
            var weatherServiceUri = "https://localhost:5001";

            services.AddHealthChecks()
                    .AddSqlServer(connectionString, tags: new[] { "storage" })
                    .AddRedis(redisString, tags: new[] { "storage" })
                    .AddUrlGroup(new Uri($"{weatherServiceUri}/weatherforecast"), "Weather API Health Check", HealthStatus.Degraded, timeout: new System.TimeSpan(0, 0, 3), tags: new[] { "service" });

            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(5);
                setup.MaximumHistoryEntriesPerEndpoint(10);
            }).AddInMemoryStorage();
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
                endpoints.MapHealthChecks("/health", new HealthCheckOptions() 
                {
                    ResponseWriter = CreateHealthCheckResponse
                });

                endpoints.MapHealthChecks("/storageHealth", new HealthCheckOptions()
                {
                    Predicate = (check) => check.Tags.Contains("storage"),
                    ResponseWriter = CreateHealthCheckResponse
                });

                endpoints.MapHealthChecks("/healthui", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            });

            app.UseHealthChecksUI();
        }

        private Task CreateHealthCheckResponse(HttpContext httpContext, HealthReport result)
        {
            httpContext.Response.ContentType = "application/json";

            var response = new HealthCheckResponse()
            {
                Status = result.Status.ToString(),
                TotalDuration = result.TotalDuration.TotalSeconds.ToString("0:0.00"),
                DependencyServices = result.Entries.Select(service => new DependencyService()
                {
                    Key = service.Key,
                    Duration = service.Value.Duration.TotalSeconds.ToString("0:0.00"),
                    Exception = service.Value.Exception?.Message,
                    Status = service.Value.Status.ToString(),
                    Tags = string.Join(",", service.Value.Tags?.ToArray())

                }).ToArray()
            };

            return httpContext.Response.WriteAsync(JsonConvert.SerializeObject(response, Formatting.Indented));
        }
    }
}
