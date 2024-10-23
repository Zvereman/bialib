using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Wangkanai.Detection;

namespace Bia.Internal.BookLibrary.ViewComponents
{
    public class BrowserDetectionViewComponent: ViewComponent
    {

        public IViewComponentResult Invoke()
        {
            var type = Request.Browser().Type;
            if (type != BrowserType.Chrome && type != BrowserType.Opera)
            {
                return View("NotSupposedBrowser");
            }

            return View("SupposedBrowser");
        }
    }
}
