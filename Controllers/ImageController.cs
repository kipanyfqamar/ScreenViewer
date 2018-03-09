using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Http.Results;
namespace ScreenViewer.Controllers
{
    public class ImageController : Controller
    {
        //
        // GET: /Image/
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Display(int ID)
        {
            ViewBag.ViewID = ID;
            return PartialView("_ImageView");
        }
   
        public string Render(int ID, ControllerContext ContCont)
        {
            ViewBag.ViewID = ID;
            return RenderHelper.RenderViewToString(ContCont, "~/Views/Image/_ImageView.cshtml", null, ViewData);

        }

   
        public ActionResult DisplayImage(int ID)
        {

            ScreenViewer.API.Elements.ImageController IC = new API.Elements.ImageController();
            var actionResult = IC.GetImage(ID);

            var response = actionResult as OkNegotiatedContentResult<Data.ScriptImage>;
            //Assert.IsNotNull(response);
            Data.ScriptImage theImage = response.Content;
            
            var imageData = theImage.Image;
            return File( imageData, "image/jpg" );
        }
	}
}