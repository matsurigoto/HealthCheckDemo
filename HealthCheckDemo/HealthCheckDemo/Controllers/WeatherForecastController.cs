﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace HealthCheckDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IDistributedCache _cache;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IDistributedCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            var key = "key";
            var valueByte = await _cache.GetAsync(key);
            if (valueByte == null)
            {
                await _cache.SetAsync(key, Encoding.UTF8.GetBytes("value"), new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(3000)));
                valueByte = await _cache.GetAsync(key);
            }
            var valueString = Encoding.UTF8.GetString(valueByte);

            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
