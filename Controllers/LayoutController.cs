using System;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Http.Results;
using System.Text.RegularExpressions;

namespace ScreenViewer.Controllers
{
    public class LayoutController : Controller
    {
        public ActionResult RenderMenu(ControllerContext ContCont)
        {
            return PartialView("_Menu");
        }

        public ActionResult RenderLogo(ControllerContext ContCont)
        {
            return PartialView("_Logo");
        }

        public ActionResult RenderHeaderContent(ControllerContext ContCont)
        {
            return PartialView("_HeaderContent");
        }

        public ActionResult RenderFooterContent(ControllerContext ContCont)
        {
            return PartialView("_FooterContent");
        }

        public ActionResult RenderContent(ControllerContext ContCont)
        {
            return PartialView("_Content");
        }
    }
}