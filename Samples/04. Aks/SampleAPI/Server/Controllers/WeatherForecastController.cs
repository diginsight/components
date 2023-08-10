#region using
using Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web.Resource;
using SampleAPI.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; 
#endregion

namespace SampleAPI.Controllers
{
    //[Authorize]
    //[ApiController]
    //[Route("[controller]")]
    [Route("api/[controller]")] // api/v{v:apiVersion}/
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
        private readonly ILogger<WeatherForecastController> _logger;
        // The Web API will only accept tokens 1) for users, and 2) having the "access_as_user" scope for this API
        static readonly string[] scopeRequiredByApi = new string[] { "user_impersonation" }; //https://microsoft.com/23e4a1ee-9677-4acc-b5fd-39cf6d98987e/user_impersonation "access_as_user"

        #region .ctor
        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            using var scope = _logger.BeginMethodScope();
            _logger = logger;
        } 
        #endregion

        [HttpGet] 
        public IEnumerable<WeatherForecast> Get()
        {
            using var scope = _logger.BeginMethodScope();

            //HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);

            var rng = new Random();
            var ret = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();

            scope.Result = ret.GetLogString();
            return ret;
        }
    }
}
