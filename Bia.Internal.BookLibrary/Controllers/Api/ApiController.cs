using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Wangkanai.Detection;
using Microsoft.Extensions.Logging;

namespace Bia.Internal.BookLibrary.Controllers.Api
{
    [ApiController]
    [Route("ApiController")]
    public class ApiController : ControllerBase
    {
        private IHostingEnvironment _hostingEnvironment;
        private IBrowser _browser;
        private readonly ILogger _log;

        public ApiController(IHostingEnvironment hostingEnvironment, ILogger<ApiController> log, IBrowserResolver browserResolver)
        {
            _hostingEnvironment = hostingEnvironment;
            _browser = browserResolver.Browser;
            _log = log;
        }


        //[HttpGet("GetDirectoryName")]
        //public string GetDirectoryName()
        //{
        //    return _hostingEnvironment.WebRootPath;
        //}
    }
}