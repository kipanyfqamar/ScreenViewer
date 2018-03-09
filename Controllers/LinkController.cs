using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Http.Results;
using ScreenViewer.API;
using ScreenViewer.Models;
using System.IO;
using System.Text;
using ScreenViewer.Models.Elements;

namespace ScreenViewer.Controllers
{
    public class LinkController : Controller
    {
        //
        // GET: /Link/
        public ActionResult Index()
        {
            return View();
        }

        public string Render(int ID, ControllerContext ContCont)
        {

            ScreenViewer.API.Elements.LinkController LC = new API.Elements.LinkController();
            var actionResult = LC.GetScriptLink(Convert.ToDecimal(ID));
            var lresponse = actionResult as OkNegotiatedContentResult<Data.ScriptLink>;
            Data.ScriptLink theLink = lresponse.Content;

            ViewBag.LinkDesc = theLink.LinkDesc;
            ViewBag.LinkDisplay = theLink.LinkDisplay;
            ViewBag.LinkType = theLink.LinkType;

            switch (theLink.LinkType)
            {
                case "Web":
                case "Document":
                    ViewBag.LinkURL = theLink.LinkURL;
                    break;
                case "Section":
                case "Workflow":
                    ViewBag.LinkTypeID = theLink.LinkTypeID;
                    break;
            }

            
            ViewBag.Target = theLink.LinkNewWindow.Equals(true) ? "_blank" : "_self";
            return RenderHelper.RenderViewToString(ContCont, "~/Views/Link/_Link.cshtml", theLink, ViewData);

        }
	}
}