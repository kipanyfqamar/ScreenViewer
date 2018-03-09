using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ScreenViewer.API;
using ScreenViewer.Models;
using System.IO;
using System.Text;
using ScreenViewer.Data;
using ScreenViewer.Models.Elements;
using ScreenViewer.API.Elements;
using System.Web.Http.Results;
namespace ScreenViewer.Controllers
{
    public class ScriptLayoutController : Controller
    {
        // GET: SectionLayout
        public ActionResult Index()
        {
            return View();
        }

        // GET: SectionLayout/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: SectionLayout/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SectionLayout/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public string Render(Int32 id, ScreenViewer.Models.CriticalElements criticalelement, ControllerContext ContCont)
        {

            ScreenViewer.API.ScriptLayoutsController SC = new API.ScriptLayoutsController();
            var actionResult = SC.GetScriptLayout(Convert.ToInt32(id));
            var response = actionResult as OkNegotiatedContentResult<ScriptLayout>;

            if (response != null)
            {
                ScriptLayout SL = response.Content;
                string qincluded = "";

                bool critItems = false;

                QuestionController QuestController = new QuestionController();
                QuestionLayout questionLayout = null;

                List<ScriptLayoutE> ScriptLayouts = new List<ScriptLayoutE>();

                foreach (ScriptLayoutDetail slitem in SL.ScriptLayoutDetails)
                {

                        ScriptLayoutE layout = new ScriptLayoutE();
                    string aclass = "";
                        switch (slitem.ElementAlignment)
                        {
                        case "top-left":
                            aclass = "div_top text-left";
                            break;
                        case "top-center":
                            aclass = "div_top text-center";

                            break;
                        case "top-right":
                            aclass = "div_top text-right";

                            break;
                        case "middle-left":
                            aclass = "div_middle text-left";
                            break;
                        case "middle-center":
                            aclass = "div_middle text-center";

                            break;
                        case "middle-right":
                            aclass = "div_middle text-right";

                            break;
                        case "bottom-left":
                            aclass = "div_bottom text-left";

                            break;
                        case "bottom-center":
                            aclass = "div_bottom text-center";

                            break;
                        case "bottom-right":
                            aclass = "div_bottom text-right";

                            break;
                        default:
                            aclass = "div_top text-left";

                            break;
                    }

                        
                        layout.Scriptlayoutdetail = slitem;
                    layout.AlignClass = aclass;    

                        if (slitem.ElementType == "Question")
                        {
                            qincluded = qincluded == "" ? slitem.ElementID.ToString() : qincluded + "," + slitem.ElementID.ToString();

                            if (criticalelement.CriticalItems)
                            {
                                if (criticalelement.CriticalQuestions.Contains(slitem.ElementID.ToString()))
                                {
                                    layout.CriticalElement = true;
                                }

                            }

                        }
                        else if (slitem.ElementType == "OrderItem" && critItems)
                        {
                            layout.CriticalElement = true;

                        }
                        else
                        {
                            layout.CriticalElement = false;
                        }


                    switch (slitem.ElementType)
                    {
                        case "Question":

                            questionLayout = QuestController.RenderQuestionParts((decimal)layout.Scriptlayoutdetail.ElementID, layout.CriticalElement, ContCont);

                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace("\r", "");
                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace("\n", "");
                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace(@"""", @"\""");
                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            questionLayout.QuestionLabelText = questionLayout.QuestionLabelText.Replace("\r", "");
                            questionLayout.QuestionLabelText = questionLayout.QuestionLabelText.Replace("\n", "");
                            questionLayout.QuestionLabelText = questionLayout.QuestionLabelText.Replace(@"""", @"\""");
                            layout.QuestionLayout = questionLayout;
                            break;

 
                        case "TextHTML":
                            TextHTMLController THC = new TextHTMLController();
                            layout.TextHTMLLayout = THC.Render(Convert.ToInt32(layout.Scriptlayoutdetail.ElementID), ContCont);
                            layout.TextHTMLLayout = HttpUtility.HtmlDecode(layout.TextHTMLLayout);
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("\r", "");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("\n", "");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace(@"""", @"\""");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace(@"\\", @"\");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("'s", "&apos;s");
                            break;

                        case "Link":
                            LinkController LC = new LinkController();
                            layout.LinkLayout = LC.Render(Convert.ToInt32(layout.Scriptlayoutdetail.ElementID), ContCont);
                            layout.LinkLayout = layout.LinkLayout.Replace("\r", "");
                            layout.LinkLayout = layout.LinkLayout.Replace("\n", "");
                            layout.LinkLayout = layout.LinkLayout.Replace(@"""", @"\""");
                            layout.LinkLayout = layout.LinkLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));

                            break;
                    }
                    
                    ScriptLayouts.Add(layout);
                }

                List<ScriptLayoutE> orderedLayout = ScriptLayouts.OrderBy(o => o.Scriptlayoutdetail.RowNumber).ThenBy(o => o.Scriptlayoutdetail.Sequence).ToList();

                ViewBag.QuestionList = qincluded;



                return RenderHelper.RenderViewToString(ContCont, "~/Views/ScriptLayout/_ScriptLayout.cshtml", orderedLayout, ViewData);

            }
            else
            {
                return "";
            }
        }

        public ActionResult Preview(Int32 id, ScreenViewer.Models.CriticalElements criticalelement)
        {

            ScreenViewer.API.ScriptLayoutsController SC = new API.ScriptLayoutsController();
            var actionResult = SC.GetScriptLayout(Convert.ToInt32(id));
            var response = actionResult as OkNegotiatedContentResult<ScriptLayout>;

            if (response != null)
            {
                ScriptLayout SL = response.Content;
                string qincluded = "";

                bool critItems = false;
                ViewBag.LayoutWidth = SL.LayoutWidth;
                QuestionController QuestController = new QuestionController();
                QuestionLayout questionLayout = null;

                List<ScriptLayoutE> ScriptLayouts = new List<ScriptLayoutE>();

                foreach (ScriptLayoutDetail slitem in SL.ScriptLayoutDetails)
                {

                    ScriptLayoutE layout = new ScriptLayoutE();

                    string aclass = "";
                    switch (slitem.ElementAlignment)
                    {
                        case "top-left":
                            aclass = "div_top text-left";
                            break;
                        case "top-center":
                            aclass = "div_top text-center";

                            break;
                        case "top-right":
                            aclass = "div_top text-right";

                            break;
                        case "middle-left":
                            aclass = "div_middle text-left";
                            break;
                        case "middle-center":
                            aclass = "div_middle text-center";

                            break;
                        case "middle-right":
                            aclass = "div_middle text-right";

                            break;
                        case "bottom-left":
                            aclass = "div_bottom text-left";

                            break;
                        case "bottom-center":
                            aclass = "div_bottom text-center";

                            break;
                        case "bottom-right":
                            aclass = "div_bottom text-right";

                            break;
                        default:
                            aclass = "div_top text-left";

                            break;
                    }


                    layout.Scriptlayoutdetail = slitem;
                    layout.AlignClass = aclass;


                    if (slitem.ElementType == "Question")
                    {
                        qincluded = qincluded == "" ? slitem.ElementID.ToString() : qincluded + "," + slitem.ElementID.ToString();

                        if (criticalelement.CriticalItems)
                        {
                            if (criticalelement.CriticalQuestions.Contains(slitem.ElementID.ToString()))
                            {
                                layout.CriticalElement = true;
                            }

                        }

                    }
                    else if (slitem.ElementType == "OrderItem" && critItems)
                    {
                        layout.CriticalElement = true;

                    }
                    else
                    {
                        layout.CriticalElement = false;
                    }


                    switch (slitem.ElementType)
                    {
                        case "Question":

                            questionLayout = QuestController.RenderQuestionParts((decimal)layout.Scriptlayoutdetail.ElementID, layout.CriticalElement, ControllerContext);

                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace("\r", "");
                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace("\n", "");
                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace(@"""", @"\""");
                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            questionLayout.QuestionLabelText = questionLayout.QuestionLabelText.Replace("\r", "");
                            questionLayout.QuestionLabelText = questionLayout.QuestionLabelText.Replace("\n", "");
                            questionLayout.QuestionLabelText = questionLayout.QuestionLabelText.Replace(@"""", @"\""");
                            layout.QuestionLayout = questionLayout;
                            break;


                        case "TextHTML":
                            TextHTMLController THC = new TextHTMLController();
                            layout.TextHTMLLayout = THC.Render(Convert.ToInt32(layout.Scriptlayoutdetail.ElementID), ControllerContext);
                            layout.TextHTMLLayout = HttpUtility.HtmlDecode(layout.TextHTMLLayout);
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("\r", "");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("\n", "");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace(@"""", @"\""");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace(@"\\", @"\");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("'s", "&apos;s");
                            break;

                        case "Link":
                            LinkController LC = new LinkController();
                            layout.LinkLayout = LC.Render(Convert.ToInt32(layout.Scriptlayoutdetail.ElementID), ControllerContext);
                            layout.LinkLayout = layout.LinkLayout.Replace("\r", "");
                            layout.LinkLayout = layout.LinkLayout.Replace("\n", "");
                            layout.LinkLayout = layout.LinkLayout.Replace(@"""", @"\""");
                            layout.LinkLayout = layout.LinkLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));

                            break;
                    }


                    ScriptLayouts.Add(layout);

                }
                List<ScriptLayoutE> orderedLayout = ScriptLayouts.OrderBy(o => o.Scriptlayoutdetail.RowNumber).ThenBy(o => o.Scriptlayoutdetail.Sequence).ToList();

                ViewBag.QuestionList = qincluded;

                string viewTemplate = "_ScriptLayout";


                return View(viewTemplate, orderedLayout);
            }
            else
            {
                return null;
            }
        }



        // GET: SectionLayout/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: SectionLayout/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: SectionLayout/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: SectionLayout/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
