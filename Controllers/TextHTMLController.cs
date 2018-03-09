using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Http.Results;
using System.Text.RegularExpressions;

namespace ScreenViewer.Controllers
{
    public class TextHTMLController : Controller
    {
        //
        // GET: /TextHTML/
        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /TextHTML/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        public ActionResult KBSearch()
        {

            string SearchTerm = Request.Form["SearchTerm"];
            return SearchKnowledgeBase(SearchTerm);
        }
        public ActionResult SearchKnowledgeBase(string SearchPhrase)
        {
           ScreenViewer.API.Elements.TextHTMLController TC = new API.Elements.TextHTMLController();
            string clientID = SessionControl.SessionManager.GetClientId(HttpContext.Session);
            var actionResult = TC.GetKBSearchResults(SearchPhrase,clientID);

            var tresponse = actionResult as OkNegotiatedContentResult<List<Data.KBSearch_Result>>;
            List<Data.KBSearch_Result> sr = tresponse.Content;

            foreach (Data.KBSearch_Result KR in sr)
            {
                KR.TextHTMLContent = SPutilities.ReplaceObjectsandQuestions(HttpContext.Session, KR.TextHTMLContent, true);
                KR.TextHTMLContent = KR.TextHTMLContent.Replace("\r", "");
                KR.TextHTMLContent = KR.TextHTMLContent.Replace("\n", "");
                KR.TextHTMLContent = KR.TextHTMLContent.Replace(@"""", @"\""");
                KR.TextHTMLContent = KR.TextHTMLContent.Replace(@"\\", @"\");
                KR.TextHTMLContent = KR.TextHTMLContent.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                KR.TextHTMLContent = KR.TextHTMLContent.Replace("'s", "&apos;s");
            }
            ViewBag.SearchTerm = SearchPhrase;
            return PartialView("KBResultView",sr);
        }

        public ActionResult Display(int ID)
        {

            ScreenViewer.API.Elements.TextHTMLController TC = new API.Elements.TextHTMLController();
            var actionResult = TC.GetScriptTextHTML(Convert.ToDecimal(ID));

            var tresponse = actionResult as OkNegotiatedContentResult<Data.ScriptTextHTML>;
            //Assert.IsNotNull(response);
            Data.ScriptTextHTML theText = tresponse.Content;

            var TextHTML = SPutilities.ReplaceObjectsandQuestions(HttpContext.Session,theText.TextHTMLContent,true);
            ViewBag.TextTitle = theText.TextHTMLDesc;
            ViewBag.Content = TextHTML;
            ViewBag.TextID = theText.ScriptTextHTMLID;
            bool whatif;
            try
            {
               whatif = (bool)theText.ShowDescTooltip;
            }
            catch
            {
                whatif = false;
            }
                if (whatif)
                {
                return PartialView("_TextHTMLpop");

            }
            else
            {
                return PartialView("_TextHTML");
            }

        }
        public string Render(int ID,ControllerContext ContCont)
        {

            ScreenViewer.API.Elements.TextHTMLController TC = new API.Elements.TextHTMLController();
            var actionResult = TC.GetScriptTextHTML(Convert.ToDecimal(ID));

            var tresponse = actionResult as OkNegotiatedContentResult<Data.ScriptTextHTML>;
            //Assert.IsNotNull(response);
            Data.ScriptTextHTML theText = tresponse.Content;

            var TextHTML = SPutilities.ReplaceObjectsandQuestions(ContCont.HttpContext.Session, theText.TextHTMLContent,true);
            var TextHTML2 = SPutilities.ReplaceObjectsandQuestions(ContCont.HttpContext.Session, theText.TextHTMLAltContent, true);

            
            ViewBag.TextID = theText.ScriptTextHTMLID;
            ViewBag.TextTitle = theText.TextHTMLDesc;

            if (!string.IsNullOrEmpty(SessionControl.SessionManager.ReturnParameter(ContCont.HttpContext.Session, "SwitchLanguage")))
            {
                ViewBag.Content = !string.IsNullOrEmpty(TextHTML2) ? TextHTML2 : TextHTML;
            }
            else
                ViewBag.Content = TextHTML;

            

            bool whatif;
            try
            {
                whatif = (bool)theText.ShowDescTooltip;
            }
            catch
            {
                whatif = false;
            }
            if (whatif)
            {
                return RenderHelper.RenderViewToString(ContCont, "~/Views/TextHTML/_TextHTMLpop.cshtml", theText, ViewData);

            }
            else
            {
                return RenderHelper.RenderViewToString(ContCont, "~/Views/TextHTML/_TextHTML.cshtml", theText, ViewData);
            }

        }

        public string RenderHtml(int ID, ControllerContext ContCont)
        {

            ScreenViewer.API.Elements.TextHTMLController TC = new API.Elements.TextHTMLController();
            var actionResult = TC.GetScriptHTML(Convert.ToDecimal(ID));

            var tresponse = actionResult as OkNegotiatedContentResult<Data.ScriptHTML>;
            Data.ScriptHTML theText = tresponse.Content;

            var TextHTML = SPutilities.ReplaceObjectsandQuestions(ContCont.HttpContext.Session, theText.HTMLContent, true);

            ViewBag.TextID = theText.ScriptHTMLID;
            ViewBag.TextTitle = theText.HTMLDesc;
            ViewBag.Content = TextHTML;

            bool whatif;

            try
            {
                whatif = (bool)theText.ShowDescTooltip;
            }
            catch
            {
                whatif = false;
            }

            if (whatif)
                return RenderHelper.RenderViewToString(ContCont, "~/Views/TextHTML/_TextHTMLpop.cshtml", theText, ViewData);
            else
                return RenderHelper.RenderViewToString(ContCont, "~/Views/TextHTML/_TextHTML.cshtml", theText, ViewData);
        }


        public string ReplaceObjectsandQuestions(string TheText)
        {

            string patt4 = @"\{IMPImage:[^}]*\}";
            string newtext = TheText;
            Regex Rg4 = new Regex(patt4);

            if (TheText != "")
            {
                MatchCollection MC4 = Rg4.Matches(TheText);
                for (int i = 0; i < MC4.Count; i++)
                {
                    string ival = MC4[i].Value;
                    string iid = ival.Split(new char[] { ':' })[1];

                    ScreenViewer.API.Elements.ImageController IC = new API.Elements.ImageController();
                    var actionResult = IC.GetImage(Convert.ToInt32(iid));

                    var response = actionResult as OkNegotiatedContentResult<Data.ScriptImage>;
                    //Assert.IsNotNull(response);
                    Data.ScriptImage theImage = response.Content;

                    string alttext = theImage.ImageDesc;
                    string replacestr = "<img src=\\Image\\DisplayImage\\" + iid;
                    newtext = newtext.Replace(MC4[i].Value, replacestr);

                }
            }
            return newtext;
        }

    }
}
