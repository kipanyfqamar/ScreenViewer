using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ScreenViewer.Controllers
{
    public class CallHistoryController : Controller
    {
        // GET: /CallHistory/
        public ActionResult Index()
        {
            return PartialView("_CallHistory");
        }
	}
}