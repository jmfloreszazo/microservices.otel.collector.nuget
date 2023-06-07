using Microservices.Otel.Collector.Nuget.Errors;
using Microsoft.AspNetCore.Mvc;

namespace WebAppForTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OtelTestController : ControllerBase
    {
        private readonly ILogger<OtelTestController> _logger;

        public OtelTestController(ILogger<OtelTestController> logger)
        {
            _logger = logger;
        }

        [HttpGet, Route("GetUnhandleException")]
        public bool GetUnhandleException()
        {
            _logger.LogInformation("GetUnhandleException called");

            int i = 0;
            int j = 1 / i;
            
            return true;
        }

        [HttpGet, Route("GetHandleException")]
        public bool GetHandleException()
        {
            _logger.LogInformation("GetHandleException called");

            throw new BadRequestException("My Bad Request");

            return true;
        }

        [HttpGet, Route("Get")]
        public bool Get()
        {
            return true;
        }
    }
}