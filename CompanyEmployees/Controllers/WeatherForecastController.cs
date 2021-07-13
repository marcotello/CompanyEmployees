using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CompanyEmployees.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILoggerManager _logger;
        private readonly IRepositoryManager _repositoryManager;
        
        public WeatherForecastController(ILoggerManager logger, IRepositoryManager repositoryManager)
        {
            _logger = logger;
            _repositoryManager = repositoryManager;
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            _logger.LogInfo("Here is an info message from our values controller.");
            _logger.LogDebug("Here is a debug message from our values controller.");
            _logger.LogWarn("Here is a warn message from our values controller.");
            _logger.LogError("Here is an error message from our values controller.");

            return new string[] {"value1", "value2"};
        }
    }
}