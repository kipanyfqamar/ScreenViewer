using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ScreenViewer.Controllers
{
    public class CallbackController : Controller
    {
        // GET: Callback
        public ActionResult Display()
        {
            return View("CallbackView");
        }

    
    }
}
